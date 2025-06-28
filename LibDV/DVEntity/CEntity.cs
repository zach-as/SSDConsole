using LibCMS.Data.Associable;
using LibDV.EntityType;
using LibUtil.Equality;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LibDV.DVEntity
{
    public class CEntitySet
    {
        private string logicalName = string.Empty;
        private List<CEntity> entities;

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
        internal CEntitySet(List<CAssociable> associables)
        {
            entities = new List<CEntity>();
            associables.ForEach(a => entities.Add(new CEntity(a)));
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
                    { EntityName = LogicalName()};
        internal int Count()
            => entities.Count();
        internal bool AllExist()
            => entities.All(e => e.Exists());
        internal bool AnyExists()
            => entities.Any(e => e.Exists());
        internal CEntitySet Subset(int start, int size)
            => new CEntitySet(entities.Skip(start).Take(size).ToList());
        internal void Add(CEntitySet newSet)
            => entities.AddRange(newSet.Entities());
    }

    public class CEntity : IEqualityComparable
    {
        private Entity entity;

        internal CEntity(CAssociable associable)
        {
            entity = SEntity.EntityFromAssociable(associable);
        }
        internal CEntity(Entity entity)
        {
            this.entity = entity;
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
