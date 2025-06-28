using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using LibCMS.Data.Associable;
using LibDV.Associable;
using LibDV.Attribute;
using LibUtil.UtilGlobal;
using LibDV.EntityType;
using LibUtil.UtilAttribute;

namespace LibDV.DVEntity
{
    internal static class SEntity
    {
        // Creates a new Entity from a CAssociable
        internal static Entity EntityFromAssociable(CAssociable a)
        {
            var entity = new Entity(a.EntityType().LogicalName());
            var attributeMap = SAttributeMap.AttributeTagMap(a);
            foreach (var mapping in attributeMap.Mappings())
            {
                var attr = mapping.Attribute();
                var value = mapping.Value();
                if (value != null) // null values should just be left empty in the entity
                {
                    entity[attr.LogicalName()] = value;
                }
            }
            return entity;
        }

        internal static EntityCollection EntityCollection(this List<Entity>? entities)
        {
            if (entities == null) return new EntityCollection();
            if (entities.Count == 0) return new EntityCollection();
            return new EntityCollection(entities)
            {
                EntityName = entities.ElementAt(0).LogicalName
            };
        }

        
        internal static bool Matches(this Microsoft.Xrm.Sdk.Entity e1, Microsoft.Xrm.Sdk.Entity? e2)
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
