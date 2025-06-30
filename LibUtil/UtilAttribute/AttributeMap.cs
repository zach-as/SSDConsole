using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilAttribute
{
    public class CAttributeMap<T> where T : System.Attribute
    {
        private List<CAttributeMapping<T>> mappings;
        public CAttributeMap()
            => mappings = new List<CAttributeMapping<T>>();

        public void Add(T attr, List<Attribute> otherAttributes, string? fieldName, object? value)
            => mappings.Add(new CAttributeMapping<T>(attr, otherAttributes, fieldName, value));

        public void Add(CAttributeMapping<T> attrMapping)
            => mappings.Add(attrMapping);

        public void Add(CAttributeMap<T> attrMap)
            => mappings.AddRange(attrMap.Mappings());

        public List<CAttributeMapping<T>> Mappings() => mappings;
        public CAttributeMapping<T>? Mapping(string fieldName)
            => mappings.FirstOrDefault(m => m.FieldName() == fieldName);

        public List<string> FieldNames()
            => mappings
                .Select(m => m.FieldName())
                .Where(f => f is not null)
                .Select(f => f!)
                .ToList();

        public bool HasMapping(string fieldName)
            => mappings.Any(m => m.FieldName() == fieldName);
    }

    public class CAttributeMapping<T> where T : System.Attribute
    {
        private T attr;
        private string? fieldName;
        private object? value;
        private List<Attribute> otherAttributes;
        public CAttributeMapping(T attr, List<Attribute> otherAttr, string? fieldName, object? value)
        {
            this.attr = attr;
            this.otherAttributes = otherAttr;
            this.fieldName = fieldName;
            this.value = value;
        }
        public T Attribute() => attr;
        public List<Attribute> OtherAttributes() => otherAttributes;
        public string? FieldName() => fieldName;
        public object? Value() => value;
    }

    public static class SAttributeMap
    {
        private static Dictionary<object, Dictionary<Type, CAttributeMap<Attribute>>> maps = new Dictionary<object, Dictionary<Type, CAttributeMap<Attribute>>>();

        // This function returns a mapping of all fields of the provided object that
        // have Attribute tags of the specified type along with the field names and values
        public static CAttributeMap<T> AttributeMap_old<T>(object o) where T : System.Attribute
        {
            // Does the object have any existing mappings?
            if (maps.TryGetValue(o, out var existingMaps))
            {
                // If the map already exists, return it
                if (existingMaps.TryGetValue(typeof(T), out var targetMap))
                {
                    return (targetMap as CAttributeMap<T>)!;
                }
            }

            // If the map could not be found, create a new map and add it to the dictionary
            var newMap = AttributeMap<T>(o);
            if (!maps.ContainsKey(o))
                maps[o] = new Dictionary<Type, CAttributeMap<Attribute>>();
                
            // Store the map in the dictionary for later reference
            maps[o][typeof(T)] = (newMap as CAttributeMap<Attribute>)!;

            return newMap;
        }
        public static CAttributeMap<T> AttributeMap<T>(object o) where T : System.Attribute
        {
            var map = new CAttributeMap<T>();
            if (o is null)
                return map; // return empty map if the object is null
            var props = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var initVal = prop.GetValue(o);
                object? finalVal = initVal;

                var targetAttrs = prop.GetCustomAttributes(typeof(T), false) as T[];
                var otherAttrs = ((prop.GetCustomAttributes(typeof(Attribute), false) as Attribute[])?
                    .Where(attr => attr is not T) ?? new List<Attribute>()).ToList();

                // Check if there is an override value attribute present
                if (otherAttrs.Any(attr => attr is AOverrideValueAttribute))
                    // There is an override value attribute, so we use it to get the value
                    finalVal = otherAttrs
                        .OfType<AOverrideValueAttribute>()
                        .First()
                        .Value(o, initVal);

                // Add this field as a mapping if it has the target attribute
                if (targetAttrs is not null && targetAttrs.Count() > 0)
                    map.Add(targetAttrs[0], otherAttrs, prop.Name, finalVal);

                // dont check for nested attrs for null fields
                if (initVal is null) continue;
                var valType = initVal.GetType();
                // only check for nested attrs in complex types
                if (valType.IsPrimitive ||
                    valType == typeof(string) ||
                    valType.IsEnum) continue;
                // only check for nested attrs if the field has subfields
                if (!valType.GetFields(BindingFlags.Public | BindingFlags.Instance).Any()) continue;

                // if the field is a complex type with subfields, recursively check for attributes in it
                var nestedMap = AttributeMap<T>(initVal);
                map.Add(nestedMap);
            }
            return map;
        }

        // retrieve all fields of the specified object that have attributes of the specified types
        public static CAttributeMap<Attribute> AttributeMapAll(object o)
            => AttributeMap<Attribute>(o);

        // retrieve all fields of the specified object where the field has an attribute of type AAttributeTagAttribute
        // this attribute indicates that the field has a relevant logical name which we use in EAttributeName and in DV
        public static CAttributeMap<AAttributeTagAttribute> AttributeTagMap(object o)
            => AttributeMap<AAttributeTagAttribute>(o);

        public static bool HasMapping(this CAttributeMap<AAttributeTagAttribute> map, EAttributeName attr)
            => map.Mappings().Any(m => m.Attribute().AttributeName() == attr);
        public static CAttributeMapping<AAttributeTagAttribute> Mapping(this CAttributeMap<AAttributeTagAttribute> map, EAttributeName attr)
        {
            var mapping =  map.Mappings().FirstOrDefault(m => m.Attribute().AttributeName() == attr);
            if (mapping is null)
                throw new ArgumentException($"No mapping found for attribute {attr.LogicalName()}");
            return mapping;
        }
    }
}
