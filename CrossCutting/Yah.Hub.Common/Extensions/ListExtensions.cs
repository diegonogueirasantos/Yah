namespace Yah.Hub.Common.Extensions
{
    public static class ListExtensions
    {
        public static List<TValue> TryAddRange<TValue>(this List<TValue> sourceList, IEnumerable<TValue> list)
        {
            if (sourceList == null)
                sourceList = new List<TValue>();

            if (list != null && list.Any())
                sourceList.AddRange(list);

            return sourceList;
        }
    }
}
