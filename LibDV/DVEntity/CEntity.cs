using LibUtil.Equality;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;

namespace LibDV.DVEntity
{
    internal class CEntity : IEqualityComparable
    {
        private Entity entity;

        internal CEntity(Entity entity)
        {
            this.entity = entity;
        }
        internal CEntity()
        {
            this.entity = new Entity();
        }

        public CEqualityExpression EqualityExpression()
        {
            var expression = new CEqualityExpression();

            // Add the logical name condition
            expression.AddCondition(EAttributeName.LogicalName, EEqualityComparator.Equal, entity.LogicalName);

            return expression;
        }
        public object? AttributeValue(EAttributeName attrName)
        {
            if (attrName == EAttributeName.LogicalName)
                return entity.LogicalName;
            throw new NotImplementedException();
        }

        internal static bool Matches(this Entity e1, Entity? e2)
        {
            if (e1 == null || e2 == null) return false;
            if (e1.LogicalName != e2.LogicalName) return false;
            if (e1.Id != e2.Id) return false;
            // Check if all attributes match
            foreach (var attr in e1.Attributes)
            {
                if (!e2.Attributes.TryGetValue(attr.Key, out var value) || !object.Equals(attr.Value, value))
                {
                    return false;
                }
            }
            // Check if e2 has any additional attributes that e1 does not have
            foreach (var attr in e2.Attributes)
            {
                if (!e1.Attributes.ContainsKey(attr.Key))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
