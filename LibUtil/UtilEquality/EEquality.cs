using LibUtil.UtilAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilEquality
{

    public enum EEqualityComparator
    {
        [Description("=")]
        Equal, // the two objects are equal
        [Description("!=")]
        NotEqual, // the two objects are not equal
    }

    public enum EEqualityResult
    {
        [Description("true")]
        True, // the two objects are equal
        [Description("false")]
        False, // the two objects are not equal
        [Description("invalid")]
        Invalid // the two objects cannot be compared
    }

    public enum EEqualityExpressionOperator
    {
        And, // all conditions and expressions must be true
        Or, // at least one condition or expression must be true
    }

    internal static partial class SEqualityComparator
    {
        private static DescriptionAttribute DescAttribute(this EEqualityComparator comparator)
            => comparator.InternalAttribute<DescriptionAttribute>();
        internal static string Description(this EEqualityComparator comparator)
            => comparator.DescAttribute().Description;
        internal static EEqualityResult CalculateResult(this EEqualityComparator comparator, bool equivalent)
            => comparator == EEqualityComparator.Equal
                ? (equivalent ? EEqualityResult.True : EEqualityResult.False)
                : (equivalent ? EEqualityResult.False : EEqualityResult.True);
    }

    internal static partial class SEqualityResult
    {
        internal static DescriptionAttribute DescAttribute(this EEqualityResult comparator)
            => comparator.InternalAttribute<DescriptionAttribute>();
        internal static string Description(this EEqualityResult comparator)
            => comparator.DescAttribute().Description;
        internal static bool IsTrue(this EEqualityResult result)
            => result switch
            {
                EEqualityResult.True => true,
                EEqualityResult.False => false,
                _ => throw new Exception($"Invalid EEqualityResult: {result} in IsTrue()."),
            };
    }

    internal static partial class SEqualityExpressionOperator
    {
        internal static bool Compound(this EEqualityExpressionOperator op, bool? previous, bool current)
            => op switch
            {
                EEqualityExpressionOperator.And => previous.HasValue ? previous.Value && current : current,
                EEqualityExpressionOperator.Or => previous.HasValue ? previous.Value || current : current,
                _ => throw new ArgumentOutOfRangeException(nameof(op), $"Unsupported operator: {op}"),
            };
    }
}
