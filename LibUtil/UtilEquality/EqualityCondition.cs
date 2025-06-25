using LibUtil.UtilAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilEquality
{

    public class CEqualityCondition
    {
        private EEqualityComparator comparator;
        private CEqualityConditionValue conditionValA;
        private CEqualityConditionValue conditionValB;

        internal CEqualityCondition(CEqualityConditionValue conditionValA,
                                    EEqualityComparator comparator,
                                    CEqualityConditionValue conditionValB)
        {
            this.comparator = comparator;
            this.conditionValA = conditionValA;
            this.conditionValB = conditionValB;
        }

        internal EEqualityComparator Comparator() => comparator;
        internal object? ValA() => conditionValA.Value();
        internal object? ValB() => conditionValB.Value();
        public override string ToString()
            => $"{ValA()} {comparator.Description()} {ValB()}";
    }

    public static partial class SEqualityCondition
    {
        internal static EEqualityResult Evaluate(this CEqualityCondition condition)
        {
            if (condition == null)
                return EEqualityResult.Invalid;

            var comp = condition.Comparator();
            var valA = condition.ValA();
            var valB = condition.ValB();

            // are both null or empty? this extra check is needed mainly for strings
            if (valA.IsNullOrEmpty() && valB.IsNullOrEmpty())
                return comp.CalculateResult(true);

            return comp.CalculateResult(object.Equals(valA, valB));
        }
        public static bool IsTrue(this CEqualityCondition condition)
            => condition.Evaluate().IsTrue();
    }
}
