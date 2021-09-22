namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Collections.Generic;
using Xunit;

#if NET6_0_OR_GREATER
public class PriorityQueueConverterTests
{
    [Fact(DisplayName = "Null Priority Queue")]
    public void NullValue()
    {
        var generator = Generator.CreateDefault();

        void Invoke<E, P>()
        {
            var converter = generator.GetConverter<PriorityQueue<E, P>>();
            Assert.StartsWith("Mikodev.Binary.Creators.PriorityQueueConverter`2", converter.GetType().FullName);
            var buffer = converter.Encode(null);
            Assert.Empty(buffer);
            var result = converter.Decode(Array.Empty<byte>());
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        Invoke<string, int>();
        Invoke<Uri, double>();
    }

    [Fact(DisplayName = "Priority Queue")]
    public void Value()
    {
        var generator = Generator.CreateDefault();

        void Invoke<E, P>(IEnumerable<(E, P)> values)
        {
            var converter = generator.GetConverter<PriorityQueue<E, P>>();
            Assert.StartsWith("Mikodev.Binary.Creators.PriorityQueueConverter`2", converter.GetType().FullName);
            var source = new PriorityQueue<E, P>(values);
            var buffer = converter.Encode(source);
            var result = converter.Decode(buffer);
            Assert.NotNull(result);
            Assert.Equal(source.UnorderedItems, result.UnorderedItems);
        }

        Invoke(new[] { ("Alpha", 3), ("Bravo", 10) });
        Invoke(new[] { (2.1, "A"), (5.5, "B+"), (7.6, "C") });
    }
}
#endif
