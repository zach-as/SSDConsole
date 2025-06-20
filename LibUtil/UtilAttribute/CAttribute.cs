namespace LibUtil.UtilAttribute
{
    public static class CUtilAttribute

    {
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
    }
}
