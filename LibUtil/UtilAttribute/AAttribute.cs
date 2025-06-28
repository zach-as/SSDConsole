using LibUtil.Reflection;
using LibUtil.UtilGlobal;

namespace LibUtil.UtilAttribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AAttributeTagAttribute : System.Attribute
    {
        private EAttributeName attrName; // the enum representing the statically typed attribute names
        private string logicalName; // the logical name of this attribute as used in dataverse
        public AAttributeTagAttribute(EAttributeName attrName)
        {
            this.attrName = attrName;
            logicalName = CGlobal.Prefix() + attrName.Name();
        }

        public EAttributeName AttributeName() => attrName;
        public override string ToString() => logicalName;
        public string LogicalName() => logicalName;
    }

    // An attribute for overridiing the value of an attribute in a class.
    // This is most useful if the existing value in the relevant field should not be directly used.
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AOverrideValueAttribute : System.Attribute
    {
        private object? value; // a direct value override
        private EFuncName? funcName; // a function to call to get the value, if the value is not directly set

        public AOverrideValueAttribute(object? value)
        {
            this.value = value;
        }
        public AOverrideValueAttribute(EFuncName funcName)
        {
            this.funcName = funcName;
        }

        public object? Value(object? owner, params object?[] inputs)
        {
            // If no func name is provided, return either null or the direct value
            if (funcName is null)
                return value;

            // If a function name is provided, call the function with the inputs
            else
            {
                var method = funcName?.GetMethod();
                var reqParams = method?.GetParameters().Where(p => p.IsIn).Count();
                if (reqParams == 0 && inputs.Length == 1) // this occurs sometimes if a func requires none but the default field val is provided
                    return funcName?.GetMethod().Invoke(owner, null);
                if (reqParams == inputs.Length) // this is the ideal case
                    return funcName?.GetMethod().Invoke(owner, inputs);
                // something fucked up somewhere because reqParams != inputs.Length
                throw new ArgumentException($"Function {funcName?.Name()} requires {reqParams} input parameters, but {inputs.Length} were provided.");
            }
        }
        public object? Value(params object?[] inputs)
            => Value(null, inputs);
    }

    public static class SAttributeTagOverrideValue
    {
        public static object? Value(this AOverrideValueAttribute attr, object? owner, params object?[] inputs)
        {
            if (attr == null)
                return null;
            return attr.Value(inputs);
        }
        public static object? Value(this AOverrideValueAttribute attr, params object?[] inputs)
            => attr.Value(null, inputs);
    }
}