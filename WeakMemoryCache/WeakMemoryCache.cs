using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.Memory
{
    public class WeakMemoryCache : IMemoryCache
    {
        private readonly IMemoryCache _cache;
        public WeakMemoryCache(IOptions<MemoryCacheOptions> optionsAccessor) => _cache = new MemoryCache(optionsAccessor);
        public WeakMemoryCache(IOptions<MemoryCacheOptions> optionsAccessor, ILoggerFactory loggerFactory) => _cache = new MemoryCache(optionsAccessor, loggerFactory);

        public ICacheEntry CreateEntry(object key) => _cache.CreateEntry(key);

        public void Dispose() => _cache.Dispose();

        public T Get<T>(object key) where T : class
        {
            if (_cache.TryGetValue(key, out WeakReference<T> reference))
            {
                reference.TryGetTarget(out T value);
                return value;
            }
            return null;
        }

        public WeakReference<T> GetWeakRef<T>(object key) where T : class
        {
            if (_cache.TryGetValue(key, out WeakReference<T> reference))
                return reference;
            return null;
        }

        public WeakReference GetWeakRef(object key)
        {
            if (_cache.TryGetValue(key, out WeakReference reference))
                return reference;
            return null;
        }

        public void Remove(object key) => _cache.Remove(key);

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

        public bool TryGetValue<T>(object key, out T value) where T : class
        {
            if (_cache.TryGetValue(key, out WeakReference<T> v))
                return v.TryGetTarget(out value);
            value = null;
            return false;
        }
    }
}