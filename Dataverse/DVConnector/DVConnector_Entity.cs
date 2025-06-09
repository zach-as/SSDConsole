using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Data.Associable;
using Microsoft.Crm.Sdk.Messages;

namespace SSDConsole.Dataverse.DVConnector
{
    internal static partial class DVConnector
    {
        #region entity
        
        // This creates the basic entity objects for later use
        internal static Dictionary<Associable, Entity> CreateAndUpdateEntities(List<Associable> associables)
        {
            Console.WriteLine("CreateAndUpdateEntities() called.");

            // Retrieve all relevant entities
            var allEntities = FetchEntities();
            // Pair entities with provided asociables
            var pairedEntities = PairedList(associables, allEntities);
            // Update entities that have matching associables
            ApplyAttributes(pairedEntities);
            // Push updated entities to dataverse
            PushUpdates(pairedEntities);
            // Create new entities for each associable that does not have a matching entity in Dataverse
            var newEntities = CreateEntities(associables, pairedEntities);
            // Push the newly created entities to dataverse
            var pushedEntities = PushCreates(newEntities);

            #region entity_new
            // Create an enumerable of associables that do not yet exist in the dataverse; these need to be created
            var associablesToCreate = associables.Except(existingEntities.Keys);
            // Create an Entity object for each associable to create and pair them in a dictionary
            var newEntities = CreateEntities(associablesToCreate);
            // Push the newly created entities to dataverse
            if (newEntities.Count > 0)
            {
                Console.WriteLine($"Creating {newEntities.Count} new entities in dataverse.");
                PushEntities(newEntities);
            }
            #endregion entity_new

            Console.WriteLine("Retrieving updated entity information from Dataverse.");
            // Fetch all entities again to ensure we have the most up-to-date information, including entity IDs

            Console.WriteLine("Pairing updated entity information with associables.");
            return PairEntities(associables, allEntities);
        }



        #region entity_private
            // This pushes all changes related to entities to dataverse.
            // This will crash if it attempts to create entities that already exists.
            // Ensure that this function is only called for new entities.
        private static void PushEntities(Dictionary<Associable, Entity> entitiesToPush)
        {
            Console.WriteLine("PushEntities() called.");
            Console.WriteLine($"Pushing {entitiesToPush.Values.Count()} entities to dataverse.");

            Dictionary<Type, List<Associable>> associablesByType = Associable.SortAssociables(entitiesToPush.Keys.ToList());

            foreach (var typelist in associablesByType) {
                var type = typelist.Key;
                var associables = typelist.Value;
                if(associables.Count > 0)
                {
                    var logicalname = associables.ElementAt(0).LogicalName();

                    Console.WriteLine($"Pushing {associables.Count()} entities of type: {logicalname}");

                    // Retrieve the entities that match the logical name of the associables
                    var entities = entitiesToPush.Select(p => p.Value)
                                                 .Where(e => e.LogicalName == logicalname)
                                                 .ToList();

                    // Push the changes in Dataverse
                    DVRequest.ProcessRequests_Push(Service(), entities);
                }
            }

            Console.WriteLine("Entities pushed.");
        }

        // This function updates the properties of existing entities based on the associables provided
        // This will crash if any of the provided Entity objects do not exist in dataverse
        private static void UpdateEntities(Dictionary<Associable, Entity> entityDict)
        {
            Console.WriteLine("PushUpdates() called.");

            int updated = 0;
            int total = entityDict.Count();

            foreach (var pair in entityDict)
            {

                var associable = pair.Key;
                var entity = pair.Value;

                // Update the properties of the entity based on the associable
                associable.ApplyAttributes(entity);

                updated++;
                Console.CursorLeft = 0;
                Console.Write($"Entity objects updated: {updated} / {total}");
            }

            Console.WriteLine();

            Dictionary<Type, List<Associable>> associablesByType =
                Associable.SortAssociables(entityDict.Keys.ToList());

            foreach (var typelist in associablesByType)
            {
                var type = typelist.Key;
                var associables = typelist.Value;
                var logicalname = associables.ElementAt(0).LogicalName();
                var entities = entityDict
                               .Select(p => p.Value)
                               .Where(e => e.LogicalName == logicalname);

                // Perform the updates for the entities of this type
                DVRequest.ProcessRequests_Update(Service(), entities.ToList());
            }
        }

        // Creates new entities based on the associables provided
        // This should only be used for the creation of new entities; updating existing entities should rely upon FetchEntities()
        private static Dictionary<Associable, Entity> CreateEntities(IEnumerable<Associable> associables)
        {
            var results = new Dictionary<Associable, Entity>();

            if (associables.Count() == 0) return results; // skip this function is empty parameters

            Console.WriteLine("CreateEntities() called.");

            int created = 0;
            int total = associables.Count();

            foreach (var associable in associables)
            {

                // Create a new entity object based on the associable logical name
                var entity = new Entity(associable.LogicalName());

                // Add properties to the entity based on the associable
                associable.ApplyAttributes(entity);
                results.Add(associable, entity);

                created++;
                Console.CursorLeft = 0;
                Console.Write($"Created entities: {created} / {total}");
            }

            Console.WriteLine();

            Console.WriteLine($"Entity creation complete.");

            return results;
        }

        // Sorts the provided associables and entities into a paired dictionary
        private static Dictionary<Associable, Entity> PairEntities(IEnumerable<Associable> associables, List<Entity> entities)
        {
            Console.WriteLine("PairEntities() called.");

            var results = new Dictionary<Associable, Entity>();

            var paired = 0;
            var no_match = 0;
            var duplicates = 0;
            var totalEnt = entities.Count();
            var totalAsc = associables.Count();

            if (totalEnt != totalAsc) Console.WriteLine($"There are an unequal numer of associables and entities. Associables: {totalAsc}. Entities: {totalEnt}");

            foreach (var entity in entities)
            {

                // Find the associable that matches this entity
                var associable = associables.FirstOrDefault(a => a.Matches(entity));
                if (associable is not null)
                {
                    if (results.ContainsKey(associable))
                    {
                        Console.WriteLine($"\nWarning: Associable {associable.LogicalName()} already paired with an entity. Comparing entities.");
                        Console.WriteLine("--- Entity 1 ---");
                        results[associable].Attributes.AsEnumerable().ToList().ForEach(a =>
                        {
                            Console.WriteLine($"Entity {entity.LogicalName} has attribute {a.Key} with value {a.Value}");
                        });
                        Console.WriteLine("--- Entity 2 ---");
                        entity.Attributes.AsEnumerable().ToList().ForEach(a =>
                        {
                            Console.WriteLine($"Entity {entity.LogicalName} has attribute {a.Key} with value {a.Value}");
                        });
                        Console.WriteLine("Entity comparison complete. Skipping duplicate entry.");
                        duplicates++;
                        continue; // skip if the associable is already paired
                    }

                    // Add the associable and entity to the paired dictionary
                    results.Add(associable, entity);
                    paired++;
                }
                else
                {
                    Console.WriteLine("\nNo match!");
                    entity.Attributes.AsEnumerable().ToList().ForEach(a =>
                    {
                        Console.WriteLine($"Entity {entity.LogicalName} has attribute {a.Key} with value {a.Value}");
                    });

                    no_match++;
                }

                Console.CursorLeft = 0;
                Console.Write($"Entities paired: {paired} / {totalEnt}. Entities with no match: {no_match}. Duplicate keys: {duplicates}");
            }

            Console.WriteLine();

            var warning = no_match > 0 ? $" Warning: {no_match} entities did not match any associables." : string.Empty;
            Console.WriteLine($"Entity pairing completed.{warning}");

            return results;
        }

        

            #endregion entity_private
        #endregion entity
    }
}