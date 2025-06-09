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

        internal static void CreateRelationships(Dictionary<Associable, Entity> dict)
        {
            var relationshipSets = BuildRelationships(dict);
            AddRelationships(relationshipSets);
        }

        #region relationshipschema
        internal static void CreateRelationshipSchemas()
        {
            Console.WriteLine("Creating relationship schemas for all entity types as needed.");
            var entityTypes = Enum.GetValues(typeof(EntityType));
            foreach (var referencing in entityTypes)
            {
                foreach (var referenced in entityTypes)
                {
                    if (referencing.Equals(referenced)) continue; // do not create a relationship schema for the same entity type
                    TryCreateRelationshipSchema((EntityType)referencing, (EntityType)referenced);
                }
            }
        }

        private static void TryCreateRelationshipSchema( EntityType referencing, EntityType referenced)
        {

            if (!RelationshipSchemaExists(referencing, referenced))
            {
                CreateRelationshipSchema(referencing, referenced);
            }

        }

        private static void CreateRelationshipSchema(EntityType referencing, EntityType referenced)
        {
            var relationship = Relationship(referencing, referenced);
            Console.WriteLine($"Creating relationship schema for {relationship.SchemaName}.");

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
            Console.WriteLine($"Relationship schema for {relationship.SchemaName} created successfully.");
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
                Console.WriteLine($"Relationship schema for {relationship.SchemaName} does not exist.");
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

        internal static Dictionary<Entity, Dictionary<EntityType, EntityReferenceCollection>> BuildRelationships(Dictionary<Associable, Entity> dict)
        {
            Console.WriteLine("BuildRelationships() called. Establishing relationship links.");
            var relationships = new Dictionary<Entity, Dictionary<EntityType, EntityReferenceCollection>>();

            int entLinked = 0;
            int skipped = 0;
            int totalEntities = dict.Count();

            foreach (var pair in dict)
            {

                var entity = pair.Value;
                var associable = pair.Key;

                // for each entity associated with this entity, add the association
                var associationsByType = associable.AssociationsByType();
                foreach (var associationsOfType in associationsByType)
                {
                    var entityType = associationsOfType.Key;
                    var relatedEntities = new EntityReferenceCollection();
                    foreach (var association in associationsOfType.Value)
                    {
                        if (!dict.ContainsKey(association))
                        {
                            Console.WriteLine($"\nWarning: Attempting to associate entity of type {associable.LogicalName()} with target entity of type {association.LogicalName()}, but target could not be found. Skipping.");
                            skipped++;
                            continue;
                        }
                        if (!Related(entity, dict[association]))
                        {
                            relatedEntities.Add(dict[association].ToEntityReference());
                        }
                    }

                    if (!relationships.ContainsKey(entity))
                    {
                        relationships[entity] = new Dictionary<EntityType, EntityReferenceCollection>();
                    }

                    relationships[entity][entityType] = relatedEntities;
                }

                entLinked++;
                Console.CursorLeft = 0;
                Console.Write($"Entities with relationship links established: {entLinked} / {totalEntities}. Failed associations: {skipped}");
            }

            Console.WriteLine();
            Console.WriteLine("Relationship linking completed.");

            return relationships;
        }

        internal static void AddRelationships(Dictionary<Entity, Dictionary<EntityType, EntityReferenceCollection>> relationshipSets)
        {
            Console.WriteLine("AddRelationships() called. Adding relationship sets to entities in Dataverse.");
            int count = 0;
            int total = relationshipSets.Keys.Count;
            foreach (var relationshipSet in relationshipSets)
            {
                var entity = relationshipSet.Key;
                var referencingType = entity.EntityType();
                foreach (var relationships in relationshipSet.Value)
                {
                    var referencedType = relationships.Key;
                    var entityReferences = relationships.Value;
                    try
                    {
                        var request = new AssociateRequest
                        {
                            RelatedEntities = entityReferences,
                            Relationship = Relationship(referencingType, referencedType),
                            Target = entity.ToEntityReference()
                        };

                        Service().Execute(request);
                        //Service().Associate(entity.LogicalName, entity.Id, Relationship(referencingType, referencedType), entityReferences);
                    } catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
                count++;
                Console.CursorLeft = 0;
                Console.Write($"Relationship sets added to dataverse: {count} / {total}");
            }
        }
    }
}
