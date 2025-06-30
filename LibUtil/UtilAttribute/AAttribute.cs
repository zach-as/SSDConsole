using LibUtil.Reflection;
using LibUtil.UtilGlobal;

namespace LibUtil.UtilAttribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class AAttributeTagAttribute : System.Attribute
    {
        private EAttributeName attrName; // the enum representing the statically typed attribute names

        public AAttributeTagAttribute(EAttributeName attrName)
        {
            this.attrName = attrName;
        }

        public EAttributeName AttributeName() => attrName;
        public override string ToString() => LogicalName();
        public string LogicalName() => CGlobal.Prefix() + attrName.Name();
    }

    // An attribute for overridiing the value of an attribute in a class.
    // This is most useful if the existing value in the relevant field should not be directly used.
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
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
                if (method is null) return null;
                object? caller = !(method?.IsStatic ?? true) ? owner : null; // if the method is static, we don't need an owner
                var reqParams = method?.GetParameters().Count(); // how many input params there are

                if (reqParams == 0)
                    return method!.Invoke(caller, null);
                if (reqParams == inputs.Length) // this is the ideal case
                    return method!.Invoke(caller, inputs);
                // something fucked up somewhere because reqParams != inputs.Length && reqParams != 0
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