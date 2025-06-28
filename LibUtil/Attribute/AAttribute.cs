using LibUtil.UtilGlobal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class AOverrideValueAttribute : System.Attribute
    {
        private object? value;
        private Type? cType; // class type
        private string fName = string.Empty; // func name

        public AOverrideValueAttribute(object? value)
        {
            this.value = value;
        }
        // This constructor is used to specify a static method in a class that returns the value.
        public AOverrideValueAttribute(Type cType, string fName)
        {
            this.cType = cType;
            this.fName = fName;
        }

        public object? Value(params object?[] inputs)
        {
            if (cType != null)
            {
                var func = cType.GetMethod(fName);
                if (func == null)
                    throw new ArgumentException($"Method '{fName}' not found in type '{cType.FullName}'.");
                return func?.Invoke(null, inputs);
            }
            return value;
        }
    }

    public static class SAttributeTagOverrideValue
    {
        public static object? Value(this AOverrideValueAttribute attr, params object[] inputs)
        {
            if (attr == null)
                return null;
            return attr.Value(inputs);
        }
    }
}