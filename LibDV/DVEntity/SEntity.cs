using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using LibCMS.Data.Associable;
using LibDV.DVAssociable;
using LibDV.DVAttribute;
using LibUtil.UtilGlobal;
using LibDV.DVEntityType;

namespace LibDV.DVEntity
{
    internal static class SEntity
    {

        internal static EntityCollection EntityCollection(this List<Entity>? entities)
        {
            if (entities == null) return new EntityCollection();
            if (entities.Count == 0) return new EntityCollection();
            return new EntityCollection(entities)
            {
                EntityName = entities.ElementAt(0).LogicalName
            };
        }

        internal static object? AttributeValue(this Entity e, string attrName)
            => e.Attributes.TryGetValue(attrName, out var value) ? value : null;
        internal static bool HasAttribute(this Entity e, string attrName)
            => e.Attributes.ContainsKey(attrName);
        
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
