namespace Yah.Hub.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static QueryString ToCreateQueryString(this IDictionary<string, string> dictionary)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var item in dictionary)
            {
                list.Add(new KeyValuePair<string, string>(item.Key, item.Value));
            }
            return QueryString.Create(list);
        }

        public static T GetValueOrDefault<T>(this IDictionary<string, object> obj, string key)
        {
            if (obj.ContainsKey(key))
                return (T)Convert.ChangeType(obj[key], typeof(T));

            return default;
        }

        public static TValue TryGetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> obj, TKey key, TValue defaultValue = default)
        {
            if (key != null)
            {
                if (obj?.ContainsKey(key) ?? false)
                    return obj[key];
            }

            return defaultValue;
        }
    }
}
