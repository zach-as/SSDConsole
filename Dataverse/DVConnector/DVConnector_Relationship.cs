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
using System.Drawing.Printing;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{
    internal static partial class DVConnector
    {

        private static Relationship Relationship(EntityType referencing, EntityType referenced)
        {
            if (referencing == referenced) throw new ArgumentException("Cannot create a relationship schema for the same entity type.", nameof(referencing));
            bool referencingFirst = referencing.GoesBefore(referenced);
            var first = referencingFirst ? referencing.SchemaName() : referenced.SchemaName();
            var second = referencingFirst ? referenced.SchemaName() : referencing.SchemaName();
            return new Relationship($"{first}_{second}");
        }
        private static Relationship Relationship(Associable referencing, Associable referenced)
            => Relationship(referencing.EntityType(), referenced.EntityType());
        private static Relationship Relationship(Entity referencing, Entity referenced)
            => Relationship(referencing.EntityType(), referenced.EntityType());
        private static string LogicalName(this Relationship relationship)
            => relationship.SchemaName.ToLower();

        internal static void CreateRelationships(List<AEPair> pairs)
        {
            var relationshipSets = BuildRelationships(pairs);
            AddRelationships(relationshipSets);
        }

        #region relationshipschema
        internal static void CreateRelationshipSchemas()
        {
            Display.Interrupt("CreateRelationshipSchemas() called. Creating relationship schemas for all entity types as needed.");
            var entityTypes = Enum.GetValues(typeof(EntityType));
            var schemaCreated = false;
            foreach (var referencing in entityTypes)
            {
                foreach (var referenced in entityTypes)
                {
                    if (referencing.Equals(referenced)) continue; // do not create a relationship schema for the same entity type
                    schemaCreated = schemaCreated || TryCreateRelationshipSchema((EntityType)referencing, (EntityType)referenced);
                }
            }
            if (!schemaCreated)
            {
                Display.Interrupt("No relationship schemas created.");
            } else
            {
                Display.Interrupt("Schema creation finished.");
            }
        }

        private static bool TryCreateRelationshipSchema( EntityType referencing, EntityType referenced)
        {

            if (!RelationshipSchemaExists(referencing, referenced))
            {
                CreateRelationshipSchema(referencing, referenced);
                return true;
            }
            return false;
        }

        private static void CreateRelationshipSchema(EntityType referencing, EntityType referenced)
        {
            var relationship = Relationship(referencing, referenced);
            Display.Print($"Creating relationship schema for {relationship.SchemaName}");

            var request = new CreateManyToManyRequest()
            {
                IntersectEntitySchemaName = relationship.SchemaName,
                ManyToManyRelationship =
                new ManyToManyRelationshipMetadata()
                {
                    SchemaName = relationship.SchemaName,
                    Entity1LogicalName = referencing.LogicalName(),
                    Entity1AssociatedMenuConfiguration =
                        new AssociatedMenuConfiguration()
                        {
                            Behavior = AssociatedMenuBehavior.UseLabel,
                            Group = AssociatedMenuGroup.Details,
                            Label = new Label(referencing.FriendlyName(), 1033),
                            Order = 10000

                        },
                    Entity2LogicalName = referenced.LogicalName(),
                    Entity2AssociatedMenuConfiguration =
                        new AssociatedMenuConfiguration()
                        {
                            Behavior = AssociatedMenuBehavior.UseLabel,
                            Group = AssociatedMenuGroup.Details,
                            Label = new Label(referenced.FriendlyName(), 1033),
                            Order = 10000
                        }
                },
                SolutionUniqueName = SOLUTION
            };

            Service().Execute(request);
            Display.Print($"Relationship schema for {relationship.SchemaName} created successfully.",
                                Display.MessageSeverity.Success);
        }

        private static bool RelationshipSchemaExists(EntityType referencing, EntityType referenced)
        {
            var relationship = Relationship(referencing, referenced);
            try
            {
                var request = new RetrieveRelationshipRequest()
                {
                    Name = relationship.SchemaName,
                    RetrieveAsIfPublished = true
                };
                var response = (RetrieveRelationshipResponse)Service().Execute(request);
                return true;
            }
            catch (Exception ex)
            {
                Display.Print($"Relationship schema for {relationship.SchemaName} does not exist.",
                                    Display.MessageSeverity.Warning);
                return false;
            }
        }
        #endregion relationshipschema

        private static bool Related(EntityReference a, EntityReference b)
            => Related(a.Id, a.EntityType(), b.Id, b.EntityType());
        private static bool Related(Entity a, Entity b)
            => Related(a.Id, a.EntityType(), b.Id, b.EntityType());
        private static bool Related(Entity a, EntityReference b)
            => Related(a.Id, a.EntityType(), b.Id, b.EntityType());
        private static bool Related(Guid aId, EntityType aType, Guid bId, EntityType bType)
        {
            if (aType == bType) return false;

            var context = new OrganizationServiceContext(Service());
            
            var aGoesFirst = aType.GoesBefore(bType);

            var aIdAttr = DVAttribute.IdAttribute(aType).Attribute();
            var bIdAttr = DVAttribute.IdAttribute(bType).Attribute();

            var firstId = aGoesFirst ? aId : bId;
            var secondId = aGoesFirst ? bId : aId;
            var firstIdAttr = aGoesFirst ? aIdAttr : bIdAttr;
            var secondIdAttr = aGoesFirst ? bIdAttr : aIdAttr;

            var relatedRecords = from rel in context.CreateQuery(Relationship(aType, bType).LogicalName())
                                  where rel[firstIdAttr].Equals(firstId) && rel[secondIdAttr].Equals(secondId)
                                  select rel;

            return relatedRecords.ToArray().Any();
        }

        internal class EntityRelationshipSet
        {
            private Dictionary<EntityType, List<AEPair>> relatedPairs = new Dictionary<EntityType, List<AEPair>>();
            
            // Adds a pair to the related pairs dictionary, creating a new list if the entity type does not already exist
            internal void Add(AEPair pair)
            {
                var entityType = pair.EntityType();
                if (!relatedPairs.ContainsKey(entityType))
                {
                    relatedPairs[entityType] = [];
                }
                if (!relatedPairs[entityType].Contains(pair.Entity()))
                {
                    relatedPairs[entityType].Add(pair);
                }
            }
            
            // Returns the most suitable form of the related entities for the purpose of establishing relationships
            internal Dictionary<EntityType, EntityReferenceCollection> ReferenceCollection()
                => relatedPairs.ToDictionary(
                    pair => pair.Key,
                    pair => relatedPairs[pair.Key].ReferenceCollection());
        }
        
        internal static Dictionary<Entity, EntityRelationshipSet> BuildRelationships(List<AEPair> pairs)
        {
            var relationships = new Dictionary<Entity, EntityRelationshipSet>();
            
            string id_skip = "id_skip";
            string id_skip_entity = "id_skip_entity";

            Display.Interrupt("BuildRelationships() called. Establishing relationship links.");
            Display.StartProgressBar($"Entities with relationship links established", pairs.Count(),
                                        new Display.ProgressBarInfo(id_skip_entity, "Entities with invalid associations"),
                                        new Display.ProgressBarInfo(id_skip, "Total invalid associations"));
            
            foreach (var pair in pairs)
            {

                var entity = pair.Entity();
                var associable = pair.Associable();
                var associations = associable.Associations();
                var hasInvalidAssociation = false;

                // Initialize the relationship set for this entity
                relationships[entity] = new EntityRelationshipSet();
                
                foreach (var associatedAssociable in associations)
                {
                    var associatedPair = pairs.Find(associatedAssociable);

                    // If the associated pair is null, it means an entity matching the associated associable could not be found
                    if (associatedPair is null)
                    {
                        Display.Interrupt($"Attempting to associate entity of type {associable.LogicalName()} with"
                                        + $" target entity of type {associatedAssociable.LogicalName()}, but target could not be found. Skipping.",
                                        Display.MessageSeverity.Warning);
                        Display.UpdateProgressBar(id_skip); // Update the progress bar to reflect the skipped association
                        if (!hasInvalidAssociation) Display.UpdateProgressBar(id_skip_entity); // Update the progress bar to reflect the skipped entity count
                        hasInvalidAssociation = true;
                        continue;
                    }

                    // Pair is found, so perform the relation if needed
                    if (!Related(entity, associatedPair.Entity()))
                    {
                        relationships[entity].Add(associatedPair);
                    }
                }

                // Update the progress bar to indicate that this entity's relationships have been updated, or that its relationships are already established
                Display.UpdateProgressBar();
            }

            Display.StopProgressBar();
            Display.Interrupt($"Finished establishing relationship links for {pairs.Count} entities.",
                                Display.MessageSeverity.Success);

            return relationships;
        }

        internal static void AddRelationships(Dictionary<Entity, EntityRelationshipSet> relationshipSets)
        {
            Display.Interrupt("AddRelationships() called. Adding relationship sets to entities in Dataverse.");
            
            int total = relationshipSets.Keys.Count;

            Display.StartProgressBar("$Relationship sets added to entities", total);
            
            foreach (var pair in relationshipSets)
            {
                var entity = pair.Key;
                var referencingType = entity.EntityType();
                var relationshipSet = pair.Value;
                var entityReferences = relationshipSet.ReferenceCollection();
                
                foreach (var referencedType in entityReferences.Keys)
                {
                    try
                    {
                        var request = new AssociateRequest
                        {
                            RelatedEntities = entityReferences[referencedType],
                            Relationship = Relationship(referencingType, referencedType),
                            Target = entity.ToEntityReference()
                        };

                        Service().Execute(request);
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
