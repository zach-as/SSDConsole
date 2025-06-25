namespace LibUtil.UtilAttribute
{
    public static class SAttributeUtil

    {
        #region internalattribute
        public static bool HasInternalAttribute<T>(this object o, string? fieldName = null) where T : System.Attribute
            => InternalAttribute_Nullable<T>(o, fieldName) is not null;

        public static T InternalAttribute<T>(this object o, string? fieldName = null) where T : System.Attribute
            => InternalAttribute_Nullable<T>(o, fieldName) ?? throw new Exception("InternalAttribute() called but null value returned.");
        private static T? InternalAttribute_Nullable<T>(object o, string? fieldName = null) where T : System.Attribute
        {
            var fieldInfo = o.GetType().GetField(fieldName ?? o?.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(T), false) as T[];

            return attributes is not null && attributes.Length > 0 ?
                attributes[0]
                : null;
        }
        #endregion internalattribute

        #region attributemap
        public class CAttributeMapping<T> where T : System.Attribute
        {
            private T attr;
            private string? fieldName;
            private object? value;
            public CAttributeMapping(T attr, string? fieldName, object? value)
            {
                this.attr = attr;
                this.fieldName = fieldName;
                this.value = value;
            }
            public T Attribute() => attr;
            public string? FieldName() => fieldName;
            public object? Value() => value;
        }

        // This function returns a mapping of all fields of the provided object that
        // have Attribute tags of the specified type along with the field names and values
        public static List<CAttributeMapping<T>> AttributeMap<T>(object o) where T : System.Attribute
        {
            var results = new List<CAttributeMapping<T>>();
            var props = o.GetType().GetProperties();
            foreach (var prop in props)
            {
                // check if the property has an attribute of type T
                var attr = InternalAttribute_Nullable<T>(o, prop.Name);
                var val = prop.GetValue(o);
                if (attr is not null)
                {
                    // if the property has an attribute of type T, add it to the results
                    results.Add(new CAttributeMapping<T>(attr, prop.Name, val));
                }

                var propType = prop.PropertyType;
                if (propType.IsPrimitive) continue; // dont check for nested attrs in primitive types
                if (val is null) continue; // dont check for nested attrs for null fields

                // if the property is a complex type, recursively check for attributes in it
                var nestedMappings = AttributeMap<T>(val);
                foreach (var nestedMapping in nestedMappings)
                {
                    results.Add(nestedMapping); // add the nested attributes
                }
            }
            return results;
        }

        // retrieve all fields of the specified object that have attributes of the specified types
        public static List<CAttributeMapping<Attribute>> AttributeMapAll(object o)
            => AttributeMap<Attribute>(o);

        // retrieve all fields of the specified object where the field has an attribute of type AAttributeTagAttribute
        // this attribute indicates that the field has a relevant logical name which we use in EAttributeName and in DV
        public static List<CAttributeMapping<AAttributeTagAttribute>> AttributeTagMap(object o)
            => AttributeMap<AAttributeTagAttribute>(o);
        
        // retrieve a single field of the specified object which has an attribute tag matching the specified attribute name
        public static CAttributeMapping<AAttributeTagAttribute> AttributeTagMapping(object o, EAttributeName attrName)
            => AttributeTagMap(o)
                .FirstOrDefault(mapping => mapping.Attribute().AttributeName() == attrName)
                ?? throw new ArgumentException($"No attribute mapping found for {attrName} in {o.GetType().Name}.");

        #endregion attributemap
    }
}
