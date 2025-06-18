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
using System.Xml;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{
    
    internal static partial class DVConnector
    {
        internal class EntityRelationshipSet
        {
            private Dictionary<DVRelationshipType, List<Entity>> relatedEntities = new Dictionary<DVRelationshipType, List<Entity>>();

            internal EntityRelationshipSet() { }
            internal EntityRelationshipSet(Dictionary<DVRelationshipType, List<Entity>> relatedEntities)
            {
                this.relatedEntities = relatedEntities;
            }

            // Adds the provided entity to the lists of related entities
            internal void Relate(Entity a, Entity b)
            {
                var relEntity = DVRelationship.NewRelationship(a, b);
                var relType = DVRelationship.RelationshipType(relEntity);
                if (!relatedEntities.ContainsKey(relType))
                {
                    relatedEntities[relType] = new List<Entity>();
                }
                if (!relatedEntities[relType].Contains(relEntity))
                {
                    relatedEntities[relType].Add(relEntity);
                }
            }

            // Returns true if the two entities are related
            internal bool Related(Entity a, Entity b)
            {
                var relType = DVRelationship.RelationshipType(a, b);
                if (!relatedEntities.ContainsKey(relType)) return false;
                return relatedEntities[relType]
                    .Any(relationshipEntity => DVRelationship.RelationshipMatch(relationshipEntity, a, b));
            }

            // Returns the most suitable form of the related entities for the purpose of establishing newRelationships
            internal Dictionary<DVRelationshipType, EntityCollection> Collections()
                => relatedEntities.ToDictionary(
                    pair => pair.Key,
                    pair => relatedEntities[pair.Key].EntityCollection());
        }
        
        internal static void CreateAndPushRelationships(List<AEPair> pairs)
        {
            var existingRelationships = ExistingRelationships();
            var newRelationships = CreateRelationships(existingRelationships, pairs);
            Console.WriteLine($"{newRelationships.Collections().Values.Count()} new relationships to add.");
            PushRelationships(newRelationships);
        }
        
        internal static EntityRelationshipSet ExistingRelationships()
        {
            var relTypes = DVRelationship.RelationshipTypes();
            var results = new Dictionary<DVRelationshipType, List<Entity>>();

            foreach (var relType in relTypes)
            {
                var relName = relType.FriendlyName();
                var query = relType.QueryExpression();
                var response = FetchEntities(query, relName, false);
                if (response is null || response.Entities.Count == 0)
                {
                    results[relType] = new List<Entity>();
                } else
                {
                    results[relType] = response.Entities.ToList();
                }
            }

            return new EntityRelationshipSet(results);
        }
        
        
        
        internal static EntityRelationshipSet CreateRelationships(EntityRelationshipSet existingRelationships, List<AEPair> pairs)
        {

            EntityRelationshipSet newRelationships = new EntityRelationshipSet();

            string id_skip = "id_skip";
            string id_skip_entity = "id_skip_entity";
            string id_already_related = "id_already_related";

            var relationshipsCreated = 0;

            Display.Interrupt("CreateAndPushRelationships() called. Creating relationship entities.");
            Display.StartProgressBar($"Entities with relationship created", pairs.Count(),
                                        new Display.ProgressBarInfo(id_skip_entity, "Entities with invalid associations"),
                                        new Display.ProgressBarInfo(id_skip, "Total invalid associations"),
                                        new Display.ProgressBarInfo(id_already_related, "Entities skipped due to existing relations"));
            
            foreach (var pair in pairs)
            {

                var entity = pair.Entity();
                var associable = pair.Associable();
                var associations = associable.Associations();
                var hasInvalidAssociation = false;
                
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

                    // Pair is found, so perform the relation if not related
                    if (!existingRelationships.Related(entity, associatedEntity) && !newRelationships.Related(entity, associatedEntity))
                    {
                        relationshipsCreated++;
                        newRelationships.Relate(entity, associatedEntity);
                    }
                }
                
                // Update the progress bar to indicate that this entity's newRelationships have been updated, or that its newRelationships are already established
                Display.UpdateProgressBar();
            }

            Display.StopProgressBar();
            Display.Interrupt($"Finished creating new {relationshipsCreated} relationships entities.",
                                Display.MessageSeverity.Success);

            return newRelationships;
        }

        internal static void PushRelationships(EntityRelationshipSet relationshipSet)
        {
            Display.Print("PushRelationships() called. Adding relationship sets to entities in Dataverse.");

            var newRelationships = relationshipSet.Collections();

            // Push the newly created relationship entities
            newRelationships.Keys.ToList().ForEach(
                relType => PushCreates(newRelationships[relType]));
            
            Display.Print($"Finished adding relationship sets to entities in Dataverse.",
                                Display.MessageSeverity.Success);
        }
    }
}
