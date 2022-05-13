using Microsoft.Extensions.Primitives;
using System;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <inheritdoc cref="ChangeToken"/>
    public class WeakToken<T> : IChangeToken where T : class
    {
        private readonly WeakReference<T> _reference;

        /// <inheritdoc cref="ChangeToken"/>
        public WeakToken(WeakReference<T> reference) => _reference = reference;

        /// <inheritdoc cref="IChangeToken.ActiveChangeCallbacks"/>
        public bool ActiveChangeCallbacks => false;

        /// <inheritdoc cref="IChangeToken.HasChanged"/>
        public bool HasChanged => !_reference.TryGetTarget(out _);

        /// <exception cref="NotSupportedException" />
        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => throw new NotSupportedException();
    }
}
