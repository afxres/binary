namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Collections.Generic;
using Xunit;

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

        Invoke([("Alpha", 3), ("Bravo", 10)]);
        Invoke([(2.1, "A"), (5.5, "B+"), (7.6, "C")]);
    }
}
