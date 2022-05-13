using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// An implementation of <see cref="IMemoryCache"/> using a dictionary to
    /// store its entries with WeakReference wrapper.
    /// </summary>
    public class WeakMemoryCache : IMemoryCache
    {
        private readonly MemoryCache _cache;

        /// <summary>
        /// Creates a new <see cref="WeakMemoryCache"/> instance.
        /// </summary>
        /// <param name="optionsAccessor">The options of the cache.</param>
        public WeakMemoryCache(IOptions<MemoryCacheOptions> optionsAccessor) => _cache = new MemoryCache(optionsAccessor);

        /// <summary>
        /// Creates a new <see cref="WeakMemoryCache"/> instance.
        /// </summary>
        /// <param name="optionsAccessor">The options of the cache.</param>
        /// <param name="loggerFactory">The factory used to create loggers.</param>
        public WeakMemoryCache(IOptions<MemoryCacheOptions> optionsAccessor, ILoggerFactory loggerFactory) => _cache = new MemoryCache(optionsAccessor, loggerFactory);

        /// <inheritdoc cref="MemoryCache.Count"/>
        public int Count => _cache.Count;

        /// <inheritdoc cref="MemoryCache.Compact(double)"/>
        public void Compact(double percentage) => _cache.Compact(percentage);

        /// <inheritdoc cref="MemoryCache.CreateEntry(object)"/>
        public ICacheEntry CreateEntry(object key) => _cache.CreateEntry(key);

        /// <inheritdoc cref="MemoryCache.Dispose()"/>
        public void Dispose() => _cache.Dispose();

        /// <summary>
        /// Gets the item associated with this key if present.
        /// </summary>
        /// <param name="key">An object identifying the requested entry.</param>
        /// <returns>The located value or null.</returns>
        public T Get<T>(object key) where T : class
        {
            if (_cache.TryGetValue(key, out WeakReference<T> reference))
            {
                reference.TryGetTarget(out T value);
                return value;
            }
            return null;
        }

        /// <summary>
        /// Gets the item WeakReference wrapper associated with this key if present.
        /// </summary>
        /// <param name="key">An object identifying the requested entry.</param>
        /// <returns>The located value or null.</returns>
        public WeakReference<T> GetWeakRef<T>(object key) where T : class
        {
            if (_cache.TryGetValue(key, out WeakReference<T> reference))
                return reference;
            return null;
        }

        /// <inheritdoc cref="MemoryCache.Remove(object)"/>
        public void Remove(object key) => _cache.Remove(key);

        /// <summary>
        /// Create or overwrite an entry in the cache.
        /// </summary>
        /// <param name="key">An object identifying the entry.</param>
        /// <param name="value">Object.</param>
        /// <returns>The newly created <see cref="ICacheEntry"/> instance.</returns>
        public T Set<T>(object key, T value) where T : class
        {
            using (var entry = _cache.CreateEntry(key))
            {
                var reference = new WeakReference<T>(value);
                entry.AddExpirationToken(new WeakToken<T>(reference));
                entry.Value = reference;
            }
            return value;
        }

        /// <inheritdoc cref="Set{T}(object, T)"/>
        public T Set<T>(object key, T value, DateTimeOffset absoluteExpiration) where T : class
        {
            using (var entry = _cache.CreateEntry(key))
            {
                var reference = new WeakReference<T>(value);
                entry.AbsoluteExpiration = absoluteExpiration;
                entry.AddExpirationToken(new WeakToken<T>(reference));
                entry.Value = reference;
            }
            return value;
        }

        /// <inheritdoc cref="Set{T}(object, T)"/>
        public T Set<T>(object key, T value, TimeSpan AbsoluteExpirationRelativeToNow) where T : class
        {
            using (var entry = _cache.CreateEntry(key))
            {
                var reference = new WeakReference<T>(value);
                entry.AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow;
                entry.AddExpirationToken(new WeakToken<T>(reference));
                entry.Value = reference;
            }
            return value;
        }

        /// <inheritdoc cref="Set{T}(object, T)"/>
        public T Set<T>(object key, T value, MemoryCacheEntryOptions options) where T : class
        {
            using (var entry = _cache.CreateEntry(key))
            {
                var reference = new WeakReference<T>(value);
                entry.SetOptions(options);
                entry.AddExpirationToken(new WeakToken<T>(reference));
                entry.Value = reference;
            }
            return value;
        }

        /// <inheritdoc cref="MemoryCache.TryGetValue(object, out object)"/>
        public bool TryGetValue(object key, out object value)
        {
            if (_cache.TryGetValue(key, out var v))
            {
                if (v.GetType().IsGenericType && v.GetType().GetGenericTypeDefinition() == typeof(WeakReference<>))
                {
                    var args = new object[] { null };
                    var ok = v.GetType()
                        .GetMethod("TryGetTarget")
                        .Invoke(v, args);
                    if ((bool)ok)
                    {
                        value = args[0];
                        return true;
                    }
                }
            }
            value = null;
            return false;
        }

        /// <inheritdoc cref="MemoryCache.TryGetValue(object, out object)"/>
        public bool TryGetValue<T>(object key, out T value) where T : class
        {
            if (_cache.TryGetValue(key, out WeakReference<T> v))
                return v.TryGetTarget(out value);
            value = null;
            return false;
        }
    }
}