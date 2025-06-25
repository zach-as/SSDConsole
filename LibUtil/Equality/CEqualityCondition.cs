using LibUtil.UtilAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.Equality
{

    public class CEqualityCondition
    {
        private EEqualityComparator comparator;
        private EAttributeName attribute;
        private CEqualityConditionValue conditionVal;

        internal CEqualityCondition(EAttributeName attribute, // the attribute to compare
                                    EEqualityComparator comparator, // the comparator to use
                                    CEqualityConditionValue conditionVal) // the value to compare against
        {
            this.comparator = comparator;
            this.attribute = attribute;
            this.conditionVal = conditionVal;
        }

        internal EEqualityComparator Comparator() => comparator;
        internal EAttributeName AttributeName() => attribute;
        internal object? Value() => conditionVal.Value();
        public override string ToString()
            => $"Attribute {AttributeName()} {comparator.Description()} {Value()}";
    }

    internal static partial class SEqualityCondition
    {
        internal static EEqualityResult Evaluate(this CEqualityCondition condition, IEqualityComparable other)
        {
            if (condition == null)
                return EEqualityResult.Invalid;
            if (other == null)
                return EEqualityResult.Invalid;

            var comp = condition.Comparator();
            var attrName = condition.AttributeName();
            var val = condition.Value();
            var otherVal = other.AttributeValue(attrName);

            // are both null or empty? this extra check is needed mainly for strings
            if (val.IsNullOrEmpty() && otherVal.IsNullOrEmpty())
                return comp.CalculateResult(true);

            return comp.CalculateResult(Equals(val, otherVal));
        }

        internal static CEqualityCondition NewCondition(EAttributeName attrName, EEqualityComparator comp, CEqualityConditionValue condVal)
            => new CEqualityCondition(attrName, comp, condVal);
        internal static CEqualityCondition NewCondition(EAttributeName attrName, EEqualityComparator comp, object? val)
            => NewCondition(attrName, comp, new CEqualityConditionValueDirect(val));
        internal static CEqualityCondition NewCondition(EAttributeName attrName, EEqualityComparator comp, Func<object?> valFunc)
            => NewCondition(attrName, comp, new CEqualityConditionValueFunc(valFunc));
        internal static CEqualityCondition NewCondition(EAttributeName attrName, EEqualityComparator comp, IEqualityComparable owner, EAttributeName valAttr)
            => NewCondition(attrName, comp, new CEqualityConditionValueAttr(owner, valAttr));

    }
}
