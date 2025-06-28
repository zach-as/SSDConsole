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
    public class AAttributeOverrideValueAttribute : System.Attribute
    {
        private object? value;
        private Func<object?>? valueFunc;

        public AAttributeOverrideValueAttribute(object? value)
        {
            this.value = value;
        }
        public AAttributeOverrideValueAttribute(Func<object?> valueFunc)
        {
            this.valueFunc = valueFunc;
        }

        public object? Value()
        {
            if (valueFunc != null)
                return valueFunc();
            return value;
        }
    }

    public static class SAttributeOverride
    {
        public static object? Value(this AAttributeOverrideValueAttribute attr)
        {
            if (attr == null)
                return null;
            return attr.Value();
        }
    }
}