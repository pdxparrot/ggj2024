using System.Collections.Generic;

namespace pdxpartyparrot.ggj2024.Collections
{
    public static class DictionaryExtensions
    {
        public static TV GetValueOrDefault<TK, TV>(this Dictionary<TK, TV> dictionary, TK key, TV defaultValue = default)
        {
            return dictionary.TryGetValue(key, out TV value) ? value : defaultValue;
        }

        public static TV GetOrAdd<TK, TV>(this Dictionary<TK, TV> dict, TK key) where TV : new()
        {
            if(dict.TryGetValue(key, out var value)) {
                return value;
            }

            value = new TV();
            dict.Add(key, value);
            return value;
        }
    }
}
