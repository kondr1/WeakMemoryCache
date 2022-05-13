using System;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary> </summary>
    public static class WeakCacheExtensions
    {
        /// <summary>
        /// Set item with WeakReference wrapper
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the item associated with this key if present.
        /// And get item from refrence wrapper.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TItem GetWeak<TItem>(this IMemoryCache cache, object key) where TItem : class
        {
            if (cache.TryGetValue(key, out WeakReference<TItem> reference))
            {
                reference.TryGetTarget(out TItem value);
                return value;
            }
            return null;
        }

        /// <summary> </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static WeakReference<TItem> GetWeakRef<TItem>(this IMemoryCache cache, object key) where TItem : class
        {
            if (cache.TryGetValue(key, out WeakReference<TItem> reference))
                return reference;
            return null;
        }
    }
}
