using LibDV.EntityType;
using LibUtil.Equality;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LibDV.Entity
{
    internal class CEntity : IEqualityComparable
    {
        private Microsoft.Xrm.Sdk.Entity entity;

        internal CEntity(Microsoft.Xrm.Sdk.Entity entity)
        {
            this.entity = entity;
        }
        internal CEntity()
        {
            this.entity = new Microsoft.Xrm.Sdk.Entity();
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
        internal Microsoft.Xrm.Sdk.Entity Entity() => entity;
    }
}
