using System;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class WeakCacheExtensions
    {
        public static TItem SetWeak<TItem>(this IMemoryCache cache, object key, TItem value) where TItem : class
        {
            using (var entry = cache.CreateEntry(key))
            {
                var reference = new WeakReference<TItem>(value);
                entry.AddExpirationToken(new WeakToken<TItem>(reference));
                entry.Value = reference;
            }
            return value;
        }

        public static TItem GetWeak<TItem>(this IMemoryCache cache, object key) where TItem : class
        {
            if (cache.TryGetValue(key, out WeakReference<TItem> reference))
            {
                reference.TryGetTarget(out TItem value);
                return value;
            }
            return null;
        }

        public static WeakReference<TItem> GetWeakRef<TItem>(this IMemoryCache cache, object key) where TItem : class
        {
            if (cache.TryGetValue(key, out WeakReference<TItem> reference))
                return reference;
            return null;
        }
    }
}
