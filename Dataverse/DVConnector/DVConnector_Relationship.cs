using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Data.Associable;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;
using SSDConsole.SSDDisplay;
using SSDConsole.Dataverse;
using System.Drawing.Printing;
using Microsoft.Xrm.Sdk.Query;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{
    
    internal static partial class DVConnector
    {
        internal class EntityRelationshipSet
        {
            private Dictionary<DVRelationshipType, List<Entity>> relatedEntities = new Dictionary<DVRelationshipType, List<Entity>>();

            // Adds the provided entity to the lists of related entities
            internal void Add(Entity entity)
            {
                var relType = DVRelationship.RelationshipType(entity);
                if (!relatedEntities.ContainsKey(relType))
                {
                    relatedEntities[relType] = new List<Entity>();
                }
                if (!relatedEntities[relType].Contains(entity))
                {
                    relatedEntities[relType].Add(entity);
                }
            }

            // Returns the most suitable form of the related entities for the purpose of establishing newRelationships
            internal Dictionary<DVRelationshipType, EntityCollection> Collection()
                => relatedEntities.ToDictionary(
                    pair => pair.Key,
                    pair => relatedEntities[pair.Key].EntityCollection());
        }
        
        internal static void CreateAndPushRelationships(List<AEPair> pairs)
        {
            var existingRelationships = ExistingRelationships();
            var newRelationships = CreateRelationships(existingRelationships, pairs);
            PushRelationships(newRelationships);
        }
        
        internal static Dictionary<DVRelationshipType, List<Entity>> ExistingRelationships()
        {
            var relTypes = DVRelationship.RelationshipTypes();
            var results = new Dictionary<DVRelationshipType, List<Entity>>();

            Display.Print("Fetching existing newRelationships.");
            foreach (var relType in relTypes)
            {
                var relName = relType.FriendlyName();
                Display.Print($"Fetching {relName} newRelationships.");
                var query = relType.QueryExpression();
                var response = FetchEntities(query, null, false);
                if (response is null || response.Entities.Count == 0)
                {
                    Display.Print($"No relationship found of type {relName}.");
                    results[relType] = new List<Entity>();
                } else
                {
                    Display.Print($"Found {response.Entities.Count} newRelationships of type {relName}.");
                    results[relType] = response.Entities.ToList();
                }
            }
            Display.Print("Existing newRelationships fetched.",
                            Display.MessageSeverity.Success);

            return results;
        }
        
        private static bool Related(
            Dictionary<DVRelationshipType, List<Entity>> relationships,
            Entity a,
            Entity b)
        {
            var aType = a.EntityType();
            var bType = b.EntityType();
            if (aType == bType) return false;
            
            var relType = DVRelationship.RelationshipType(aType, bType);
            
            foreach (var relationshipEntity in relationships[relType])
            {
                var matching = DVRelationship.RelationshipMatch(relationshipEntity, a, b);
                if (matching) return true;
            }
            return false;
        }
        
        internal static Dictionary<Entity, EntityRelationshipSet> CreateRelationships(
            Dictionary<DVRelationshipType, List<Entity>> existingRelationships,
            List<AEPair> pairs)
        {
            var newRelationships = new Dictionary<Entity, EntityRelationshipSet>();
            
            string id_skip = "id_skip";
            string id_skip_entity = "id_skip_entity";

            Display.Interrupt("CreateAndPushRelationships() called. Creating relationship entities.");
            Display.StartProgressBar($"Entities with relationship created", pairs.Count(),
                                        new Display.ProgressBarInfo(id_skip_entity, "Entities with invalid associations"),
                                        new Display.ProgressBarInfo(id_skip, "Total invalid associations"));
            
            foreach (var pair in pairs)
            {

                var entity = pair.Entity();
                var associable = pair.Associable();
                var associations = associable.Associations();
                var hasInvalidAssociation = false;

                // Initialize the relationship set for this entity
                newRelationships[entity] = new EntityRelationshipSet();
                
                foreach (var associatedAssociable in associations)
                {
                    var associatedPair = pairs.Find(associatedAssociable);

                    // If the associated pair is null, it means an entity matching the associated associable could not be found
                    if (associatedPair is null)
                    {
                        Display.Interrupt($"Attempting to create relationship for entity of type {associable.LogicalName()} with"
                                        + $" target entity of type {associatedAssociable.LogicalName()}, but target could not be found. Skipping.",
                                        Display.MessageSeverity.Warning);
                        Display.UpdateProgressBar(id_skip); // Update the progress bar to reflect the skipped association
                        if (!hasInvalidAssociation) Display.UpdateProgressBar(id_skip_entity); // Update the progress bar to reflect the skipped entity count
                        hasInvalidAssociation = true;
                        continue;
                    }

                    var associatedEntity = associatedPair.Entity();

                    // Pair is found, so perform the relation if needed
                    if (associatedEntity.IsValid() && !Related(existingRelationships, entity, associatedEntity))
                    {
                        var newRelationship = DVRelationship.NewRelationship(entity, associatedEntity);
                        newRelationships[entity].Add(newRelationship);
                    }
                }

                // Update the progress bar to indicate that this entity's newRelationships have been updated, or that its newRelationships are already established
                Display.UpdateProgressBar();
            }

            Display.StopProgressBar();
            Display.Interrupt($"Finished creating new relationship for {pairs.Count} entities.",
                                Display.MessageSeverity.Success);

            return newRelationships;
        }

        internal static void PushRelationships(Dictionary<Entity, EntityRelationshipSet> relationshipSets)
        {
            Display.Interrupt("PushRelationships() called. Adding relationship sets to entities in Dataverse.");
            
            int total = relationshipSets.Keys.Count;

            Display.StartProgressBar("$Relationship sets added to entities", total);
            
            foreach (var pair in relationshipSets)
            {
                var entity = pair.Key;
                var relationshipSet = pair.Value;
                var newRelationships = relationshipSet.Collection();
                if (newRelationships.Values.Count() == 0) continue; // skip adding newRelationships if there are none to add
                
                foreach (var bType in newRelationships.Keys)
                {
                    PushCreates(newRelationships[bType]);
                    try
                    {
                    }
                    catch (Exception ex) { Display.Interrupt(ex.Message, Display.MessageSeverity.Error); }
                }
                
                Display.UpdateProgressBar();
            }

            Display.StopProgressBar();
            
            Display.Interrupt($"Finished adding {total} relationship sets to entities in Dataverse.",
                                Display.MessageSeverity.Success);
        }
    }
}
