using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;

namespace Tests
{
    public class Tests
    {
        private readonly WeakMemoryCache cache = new(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        [Test]
        public void Test1()
        {
            for (int i = 0; i < 100; i++)
            {
                cache.Set($"kek{i}", new DisposableClass("lol"));
            }

            using (var d = cache.Get<DisposableClass>("kek99"))
            {
                Assert.NotNull(d);
                Assert.AreEqual(d.Name, "lol");
            }
            
            GC.Collect(0);
            GC.Collect(1);
            GC.Collect(2);
            GC.Collect();
            Assert.False(cache.TryGetValue<DisposableClass>("kek1", out _));

            Assert.Pass();
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