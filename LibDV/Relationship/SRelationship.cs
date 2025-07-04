using LibCMS.Data.Associable;
using LibDV.DVEntity;
using LibDV.EntityType;
using Microsoft.Xrm.Sdk;

namespace LibDV.Relationship
{
    internal static partial class SRelationship
    {
        #region getrelationshiptype
        private static List<ERelationshipType>? relationships;
        internal static List<ERelationshipType> RelationshipTypes()
        {
            // if relationships is not initialized, initialize with reflection
            if (relationships is null) relationships = Enum.GetValues<ERelationshipType>().ToList();
            return relationships;
        }

        internal static ERelationshipType RelationshipType(EEntityType a, EEntityType b)
        {
            if (a == b) throw new ArgumentException($"RelationshipType({a}, {b}): Entities of same type can not be related!");
            var types = RelationshipTypes();
            foreach (var t in types)
            {
                var aMatch = a == t.EntA() || a == t.EntB();
                var bMatch = b == t.EntA() || b == t.EntB();
                if (aMatch && bMatch) return t;
            }
            throw new Exception($"RelationshipType({a}, {b}): Unable to find matching relationship type."); // this should never be reached
        }
        internal static ERelationshipType RelationshipType(CAssociable a, CAssociable b)
            => RelationshipType(a.EntityType(), b.EntityType());
        internal static ERelationshipType RelationshipType(CEntity a, CEntity b)
            => RelationshipType(a.EntityType(), b.EntityType());
        internal static ERelationshipType RelationshipType(CEntity relEntity)
            => RelationshipTypes()
            .First(relType => relType.LogicalName() == relEntity.LogicalName());

        #endregion getrelationshiptype

        // Creates a new Entity for the purpose of recording a relationship in DV
        internal static CEntity NewRelationship(CEntity ca, CEntity cb)
        {
            var a = ca.Entity();
            var b = cb.Entity();

            var relType = RelationshipType(a.EntityType(), b.EntityType());
            var entity = new Entity(relType.LogicalName());

            // assign the reference value of A's col to A's id
            entity[a.LogicalName] = a.ToEntityReference();
            // assign the reference value of B's col to B's id
            entity[b.LogicalName] = b.ToEntityReference();

            return new CEntity(entity);
        }

        // This accepts three entities
        // relationship = the entity storing the relationship information
        // a = the first entity to check
        // b = the second entity to check
        // this function returns true if the relationship entity is of the appropriate relType and if its stored relationship info matches the given  types
        internal static bool RelationshipMatch(CEntity relationship, CEntity a, CEntity b)
        {
            var relType = RelationshipType(a.EntityType(), b.EntityType());
            if (relationship.LogicalName() != relType.LogicalName()) return false;

            var aRef = (EntityReference)relationship.Entity()[a.LogicalName()];
            var bRef = (EntityReference)relationship.Entity()[b.LogicalName()];

            var aMatch = aRef.LogicalName == a.LogicalName() && aRef.Id == a.Id();
            var bMatch = bRef.LogicalName == b.LogicalName() && bRef.Id == b.Id();

            return aMatch && bMatch;
        }

        public static CEntitySuperSet BuildRelationships(List<CAssociable> associables)
        {
            var set = new CEntitySuperSet();
            foreach (var a in associables)
            {
                var relEnts = new List<CEntity>();
                var ent = new CEntity(a);
                foreach (var relA in a.Associations())
                {
                    var relEnt = NewRelationship(ent, new CEntity(relA));
                    if (!relEnts.Contains(relEnt) && !set.HasEntity(relEnt)) // ensure we don't add duplicates
                    {
                        relEnts.Add(relEnt);
                    }
                }
                set.AddEntities(relEnts); // add all relationships for this entity to the set
            }
            return set;
        }
    }
}
