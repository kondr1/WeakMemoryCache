using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        private readonly WeakMemoryCache cache = new(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        [Test]
        public void Test1()
        {
            Enumerable
                .Range(0, 10000)
                .AsParallel()
                .ForAll(i =>
                    cache.Set($"kek{i}", new DisposableClass("lol"))
                );

            using (var d = cache.Get<DisposableClass>("kek9999"))
            {
                Assert.NotNull(d);
                Assert.AreEqual(d.Name, "lol");
            }
            
            GC.Collect(0);
            Assert.False(cache.TryGetValue<DisposableClass>("kek1", out _));
        }
    }

    public class DisposableClass : IDisposable
    {
        public string Name { get; }
        public DisposableClass(string name) => Name = name;

        public void Dispose() => GC.SuppressFinalize(this);
        ~DisposableClass() => Dispose();
    }
}