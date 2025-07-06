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
    }
}
