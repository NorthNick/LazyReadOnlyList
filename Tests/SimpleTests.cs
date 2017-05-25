using System;
using System.Collections.Generic;
using System.Linq;
using LazyReadOnlyList;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SimpleTests
    {
        private int _badListCounter;

        [SetUp]
        public void Setup()
        {
            _badListCounter = 0;
        }

        private IEnumerable<int> BadList()
        {
            for (int ii = 0; ii < 5; ii++) {
                yield return _badListCounter;
                _badListCounter++;
            }
        }

        private static IEnumerable<int> InfiniteList()
        {
            int val = 0;
            while (true) {
                yield return val;
                val++;
            }
        }

        [Test]
        public void EmptyTest()
        {
            var source = Enumerable.Empty<int>();
            var lazy = new LazyReadOnlyList<int>(source);
            Assert.That(lazy.Count, Is.EqualTo(0));
            Assert.Throws<Exception>(() => {
                var foo = lazy[5];
            });
        }

        [Test]
        public void ConstantListTest()
        {
            var source = new List<string> {"one", "two", "three", "four"};
            var lazy = new LazyReadOnlyList<string>(source);
            for (int ii = 0; ii < source.Count; ii++) {
                Assert.That(lazy[ii], Is.EqualTo(source[ii]));
            }
        }

        [Test]
        public void BadListEnumeratesOnceTest()
        {
            var lazy = new LazyReadOnlyList<int>(BadList());
            var t1 = lazy.Select((index, val) => index == val).All(b => b);
            var t2 = lazy.Select((index, val) => index == val).All(b => b);
            Assert.True(t1);
            Assert.True(t2);
        }

        [Test]
        public void InfiniteListTest()
        {
            var lazy = new LazyReadOnlyList<int>(InfiniteList());
            var prefix = lazy.Take(5);
            Assert.That(prefix, Is.EqualTo(Enumerable.Range(0, 5)));
        }

        [Test]
        public void InfiniteListMultipleIterationsTest()
        {
            var lazy = InfiniteList().ToLazyReadOnlyList();
            var prefix = lazy.Take(5);
            Assert.That(prefix, Is.EqualTo(Enumerable.Range(0, 5)));
            prefix = lazy.Take(5);
            Assert.That(prefix, Is.EqualTo(Enumerable.Range(0, 5)));
        }
    }
}
