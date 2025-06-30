using LibCMS.Data.Associable;
using LibDV.EntityType;
using LibUtil.Equality;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LibDV.DVEntity
{
    // A set of entity sets, sorted by entity types
    public class CEntitySuperSet
    {
        private Dictionary<EEntityType, CEntitySet> sets;
        internal CEntitySuperSet(Dictionary<EEntityType, CEntitySet> sets)
        {
            this.sets = sets;
        }
        public CEntitySuperSet()
        {
            this.sets = new Dictionary<EEntityType, CEntitySet>();
        }

        public Dictionary<EEntityType, CEntitySet> Sets()
            => sets;
        public CEntitySet Set(EEntityType type)
            => sets.ContainsKey(type) ? sets[type] : new CEntitySet();
        public void AddSet(EEntityType type, CEntitySet set)
        {
            if (sets.ContainsKey(type))
                // If the set already exists, merge the new set into the existing one
                sets[type].AddSet(set);
            else
                // Otherwise, add the new set
                sets[type] = set;
        }
        public void AddSet(CEntitySet set)
            => AddSet(set.EntityType(), set);
        public void AddSet(CEntitySuperSet set)
            => set.Sets().Values.ToList().ForEach(s => AddSet(s));
        public int CountAll()
            => sets.Values.Select(set => set.Count()).Sum();
        public int Count(EEntityType type)
            => sets.ContainsKey(type) ? sets[type].Count() : 0;
    }

    // A set of entities
    public class CEntitySet
    {
        private string logicalName = string.Empty;
        private List<CEntity> entities;

        public CEntitySet(List<CAssociable> associables)
        {
            entities = new List<CEntity>();
            associables.ForEach(a => entities.Add(new CEntity(a)));
            SetLogicalName();
        }

        internal CEntitySet(List<Entity> entities)
        {
            this.entities = new List<CEntity>();
            entities.ForEach(e => this.entities.Add(new CEntity(e)));
            SetLogicalName();
        }
        internal CEntitySet(List<CEntity> entities)
        {
            this.entities = entities;
            SetLogicalName();
        }
        internal CEntitySet(EntityCollection entityCol)
        {
            entities = new List<CEntity>();
            entityCol.Entities.ToList().ForEach(
                e => entities.Add(new CEntity(e)));
            SetLogicalName();
        }
        internal CEntitySet()
        {
            entities = new List<CEntity>();
            SetLogicalName();
        }

        private void SetLogicalName()
        {
            if (entities.Count() > 0)
                logicalName = entities.First().LogicalName();
            else
                logicalName = string.Empty;
        }

        internal string LogicalName()
            => logicalName;
        internal List<CEntity> Entities()
            => entities;
        internal EEntityType EntityType()
            => SEntityType.EntityType(LogicalName());
        internal ColumnSet ColumnSet()
            => EntityType().ColumnSet();
        internal QueryExpression QueryExpression()
            => EntityType().QueryExpression();
        internal EntityCollection Collection()
            => entities.Count() == 0 ? new EntityCollection()
                 : new EntityCollection(entities.Select(e => e.Entity()).ToList())
                 { EntityName = LogicalName() };
        internal int Count()
            => entities.Count();
        internal bool AllExist()
            => entities.All(e => e.Exists());
        internal bool AnyExists()
            => entities.Any(e => e.Exists());
        internal bool HasEntity(CEntity ce)
            => entities.Contains(ce);
        internal CEntitySet Subset(int start, int size)
            => new CEntitySet(entities.Skip(start).Take(size).ToList());
        internal void AddSet(CEntitySet newSet)
            // Note that this does not check for duplicate entries, but it shouldn't be a problem... right?
            => entities.AddRange(newSet.Entities());

        // This function identifies and retrieves only elements that are present in both sets
        public CEntitySet Overlapping(CEntitySet otherSet)
        {
            var overlapping = new List<CEntity>();

            foreach (var ce in Entities())
            {
                if (otherSet.HasEntity(ce))
                    overlapping.Add(new CEntity(ce, otherSet.Entities().Find(e => e.Equals(ce))!));
            }

            return new CEntitySet(overlapping);
        }
        // This function returns a CEntitySet that contains elements in which the provided entity set is not present
        public CEntitySet Excluding(CEntitySet otherSet)
        {
            var excluding = new List<CEntity>();

            foreach (var ce in Entities())
            {
                if (!otherSet.HasEntity(ce))
                    excluding.Add(ce);
            }

            return new CEntitySet(excluding);
        }
    }

    // An entity wrapper
    public class CEntity : IEqualityComparable
    {
        private Entity entity;

        public CEntity(CAssociable associable)
        {
            entity = SEntity.EntityFromAssociable(associable);
        }
        internal CEntity(Entity entity)
        {
            this.entity = entity;
        }
        // creates a new CEntity from two inputs, prioritizing a where conflict occurs
        internal CEntity(CEntity a, CEntity b)
        {
            entity = new Entity(a.LogicalName());
            var aAttr = a.Entity().Attributes;
            var bAttr = b.Entity().Attributes;
            foreach (var attr in bAttr)
            {
                if (aAttr.ContainsKey(attr.Key) && aAttr[attr.Key] != null) continue;
                entity[attr.Key] = attr.Value;
            }
            foreach (var attr in aAttr)
            {
                entity[attr.Key] = attr.Value;
            }
        }
        internal CEntity()
        {
            entity = new Entity();
        }

        public CEqualityExpression EqualityExpression()
        {
            var expression = new CEqualityExpression();

            // Add the logical name and ID conditions
            expression.AddEquals(EAttributeName.Entity_LogicalName, entity.LogicalName);
            expression.AddEquals(EAttributeName.Entity_Id, entity.Id);

            var addedAttrNames = new List<EAttributeName>() 
            { 
                EAttributeName.Entity_LogicalName,
                EAttributeName.Entity_Id,
            };

            // for each attribute in the entity, add an equality condition
            foreach (var attr in entity.Attributes)
            {
                var logicalName = attr.Key;
                // Use the logical name of the attribute
                var attrNameExists = SAttributeName.LogicalNameExists(logicalName);
                if (!attrNameExists)
                {
                    // If the logical name does not exist, skip this attribute
                    continue;
                }

                // Convert the attribute key to an EAttributeName
                var attrName = SAttributeName.EnumFromLogical(logicalName);
                expression.AddEquals(attrName, attr.Value);
                addedAttrNames.Add(attrName);
            }

            var missingAttrNames = SAttributeName.AttrNames()
                .Where(attrName => !addedAttrNames.Contains(attrName))
                .ToList();

            // for each attribute NOT in the entity, add a null condition
            missingAttrNames.ForEach(
                attr => expression.AddNull(attr)
            );

            return expression;
        }
        public object? AttributeValue(EAttributeName attrName)
        {
            if (attrName == EAttributeName.Entity_LogicalName)
                return entity.LogicalName;
            if (attrName == EAttributeName.Entity_Id)
                return entity.Id;

            var logicalName = attrName.LogicalName();

            // Try to retrieve the attribute value from the entity
            if (entity.Attributes.TryGetValue(logicalName, out var value))
            {
                return value;
            }

            // Attribute not found on entity
            return null;
        }

        internal string LogicalName() => entity.LogicalName;
        internal EEntityType EntityType() => SEntityType.EntityType(LogicalName());
        internal ColumnSet ColumnSet() => EntityType().ColumnSet();
        internal QueryExpression QueryExpression() => EntityType().QueryExpression();
        internal Guid Id() => entity.Id;
        internal Entity Entity() => entity;
        // Returns true if this entity exists in DV
        internal bool Exists() => Id() != Guid.Empty;
    }
}
