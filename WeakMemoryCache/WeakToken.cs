using Microsoft.Extensions.Primitives;
using System;

namespace Microsoft.Extensions.Caching.Memory
{
    public class WeakToken<T> : IChangeToken where T : class
    {
        private readonly WeakReference<T> _reference;

        public WeakToken(WeakReference<T> reference) => _reference = reference;

        public bool ActiveChangeCallbacks => false;

        public bool HasChanged => !_reference.TryGetTarget(out _);

        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => throw new NotSupportedException();
    }
}
