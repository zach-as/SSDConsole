using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.Equality
{
    public static partial class SEquality
    {
        internal static bool IsNullOrEmpty(this object? o)
            => o is null || (o is string str && string.IsNullOrEmpty(str));

        public static bool Matches(this IEqualityComparable comp1, IEqualityComparable comp2)
        {
            if (comp1 is null || comp2 is null) return false;
            var eval = comp1.EqualityExpression().Evaluate(comp2);
            if (eval == EEqualityResult.Invalid) throw new Exception($"Invalid comparison when comparing {comp1.ToString()} and {comp2.ToString()}.");
            return eval.IsTrue();
        }
    }
}
