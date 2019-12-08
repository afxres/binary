using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class CollectionTests
    {
        private class Enumerable<T> : IEnumerable<T>
        {
            IEnumerator IEnumerable.GetEnumerator() => Array.Empty<T>().GetEnumerator();

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException("via Enumerator");
        }

        private class CollectionAlpha<T> : Enumerable<T>
        {
            public T[] ToArray() => throw new NotSupportedException("via 'ToArray()'");

            public T[] ToArray(int placeholder) => throw new NotSupportedException("Text placeholder");
        }

        private class CollectionBravo<T> : Enumerable<T>
        {
            private T[] ToArray() => throw new NotSupportedException("private 'ToArray()'");
        }

        private class CollectionDelta<T> : Enumerable<T>
        {
            public static T[] ToArray() => throw new NotSupportedException("static 'ToArray()'");
        }

        private class CollectionHotel<T, U> : Enumerable<T>
        {
            public U[] ToArray() => throw new NotSupportedException("generic ToArray()");
        }

        public static IEnumerable<object[]> MemberData => new[]
        {
            new object[] { new Enumerable<int>(), "via Enumerator" },
            new object[] { new Enumerable<string>(), "via Enumerator" },
            new object[] { new CollectionAlpha<int>(), "via 'ToArray()'" },
            new object[] { new CollectionAlpha<string>(), "via Enumerator" },
            new object[] { new CollectionBravo<int>(), "via Enumerator" },
            new object[] { new CollectionBravo<string>(), "via Enumerator" },
            new object[] { new CollectionDelta<int>(), "via Enumerator" },
            new object[] { new CollectionDelta<string>(), "via Enumerator" },
            new object[] { new CollectionHotel<int, int>(), "generic ToArray()" },
            new object[] { new CollectionHotel<int, string>(), "via Enumerator" },
            new object[] { new CollectionHotel<string, int>(), "via Enumerator" },
            new object[] { new CollectionHotel<string, string>(), "via Enumerator" },
        };

        private readonly IGenerator generator = Generator.CreateDefault();

        [Theory(DisplayName = "Encode Via 'ToArray()' Or Enumerator")]
        [MemberData(nameof(MemberData))]
        public void ViaToArray<T>(T collection, string expected)
        {
            var source = collection;
            var converter = generator.GetConverter<T>();
            Assert.StartsWith("EnumerableAdaptedConverter`2", converter.GetType().Name);
            var error = Assert.Throws<NotSupportedException>(() =>
            {
                var allocator = new Allocator();
                converter.Encode(ref allocator, source);
            });
            Assert.Equal(expected, error.Message);
        }
    }
}
