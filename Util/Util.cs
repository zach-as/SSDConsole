
namespace SSDConsole
{
    internal static class Util
    {
        internal static List<List<T>> SplitList<T>(List<T> list, int part_size)
            => list.Select((item, index) => new { item, index })
                .GroupBy(x => x.index / part_size)
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();
    }
    
}
