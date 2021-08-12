namespace Mikodev.Binary.Tests;

using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class ThrowTests
{
    private class CollectionWithMultipleEnumerableInterfaces : IEnumerable<int>, IEnumerable<double>
    {
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw new NotImplementedException();

        IEnumerator<double> IEnumerable<double>.GetEnumerator() => throw new NotImplementedException();
    }

    [Theory(DisplayName = "Multiple Interface Implementations")]
    [InlineData(typeof(CollectionWithMultipleEnumerableInterfaces), typeof(IEnumerable<>))]
    public void MultipleEnumerable(Type type, Type definition)
    {
        var generator = Generator.CreateDefault();
        var error = Assert.Throws<ArgumentException>(() => generator.GetConverter(type));
        var message = $"Multiple interface implementations detected, type: {type}, interface type: {definition}";
        Assert.Equal(message, error.Message);
    }
}
