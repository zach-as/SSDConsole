using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibCMS.Data.Associable
{
    internal static class SAssociable
    {
        // Sorts the provided associables by type.
        internal static Dictionary<Type, List<CAssociable>> SortAssociables(List<CAssociable> associables)
        {
            var sorted = new Dictionary<Type, List<CAssociable>>();

            foreach (var associable in associables)
            {
                var type = associable.GetType();

                if (!sorted.ContainsKey(type))
                {
                    sorted[type] = new List<CAssociable>();
                }

                sorted[type].Add(associable);
            }

            return sorted;
        }
    }
}
