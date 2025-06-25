using LibUtil.UtilAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilEquality
{
    internal abstract class CEqualityConditionValue
    {
        internal abstract object? Value();
        public override string ToString() => Value()?.ToString() ?? "null";
    }

    internal class CEqualityConditionValueDirect : CEqualityConditionValue
    {
        private object? value;
        internal CEqualityConditionValueDirect(object? value)
        {
            this.value = value;
        }
        internal override object? Value() => value;
    }

    internal class CEqualityConditionValueFunc : CEqualityConditionValue
    {
        private Func<object?> valueFunc;
        internal CEqualityConditionValueFunc(Func<object?> valueFunc)
        {
            this.valueFunc = valueFunc;
        }
        internal override object? Value() => valueFunc();
    }

    internal class CEqualityConditionValueAttr : CEqualityConditionValue
    {
        private object owner;
        private EAttributeName attrName;

        internal CEqualityConditionValueAttr(object owner, EAttributeName attrName)
        {
            this.owner = owner;
            this.attrName = attrName;
        }

        internal override object? Value()
            => SAttributeUtil.AttributeTagMapping(owner, attrName).Value();
    }
}
