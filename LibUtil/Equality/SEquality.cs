using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.Equality
{
    internal static partial class SEquality
    {
        internal static bool IsNullOrEmpty(this object? o)
            => o is null || (o is string str && string.IsNullOrEmpty(str));
    }
}
