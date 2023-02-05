namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class VariableBoundArrayConverterTests
{
    public static IEnumerable<object[]> Array2DData()
    {
        var alpha = new int[2, 3];
        for (var i = 0; i < alpha.GetLength(0); i++)
            for (var k = 0; k < alpha.GetLength(1); k++)
                alpha[i, k] = (i << 16) | k;
        var bravo = new string[3, 4];
        for (var i = 0; i < bravo.GetLength(0); i++)
            for (var k = 0; k < bravo.GetLength(1); k++)
                bravo[i, k] = $"({i}, {k})";
        yield return new object[] { alpha };
        yield return new object[] { bravo };
    }

    public static IEnumerable<object[]> Array2DNonZeroBasedData()
    {
        var alpha = (int[,])Array.CreateInstance(typeof(int), new[] { 2, 3 }, new[] { 4, 5 });
        for (var i = alpha.GetLowerBound(0); i <= alpha.GetUpperBound(0); i++)
            for (var k = alpha.GetLowerBound(1); k <= alpha.GetUpperBound(1); k++)
                alpha[i, k] = (i << 16) | k;
        var bravo = (string[,])Array.CreateInstance(typeof(string), new[] { 3, 4 }, new[] { 5, 6 });
        for (var i = bravo.GetLowerBound(0); i <= bravo.GetUpperBound(0); i++)
            for (var k = bravo.GetLowerBound(1); k <= bravo.GetUpperBound(1); k++)
                bravo[i, k] = $"({i}, {k})";
        yield return new object[] { alpha };
        yield return new object[] { bravo };
    }

    [Theory(DisplayName = "Array2D Test")]
    [MemberData(nameof(Array2DData))]
    [MemberData(nameof(Array2DNonZeroBasedData))]
    public void Array2DTest<T>(T[,] values)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T[,]>();
        Assert.Equal("VariableBoundArrayConverter`2", converter.GetType().Name);
        var buffer = converter.Encode(values);
        var result = converter.Decode(buffer);
        Assert.False(ReferenceEquals(values, result));
        Assert.Equal(values.GetLowerBound(0), result.GetLowerBound(0));
        Assert.Equal(values.GetLowerBound(1), result.GetLowerBound(1));
        Assert.Equal(values.GetLength(0), result.GetLength(0));
        Assert.Equal(values.GetLength(1), result.GetLength(1));
        var sourceSequence = values.Cast<T>().ToList();
        var actualSequence = values.Cast<T>().ToList();
        Assert.Equal(sourceSequence, actualSequence);
    }

    public static IEnumerable<object[]> EmptyArrayData()
    {
        yield return new object[] { new int[3, 0] };
        yield return new object[] { new string[0, 2, 4] };
        var alpha = Array.CreateInstance(typeof(int), new[] { 2, 0, 2, 3 }, new[] { 0, 1, 0, 2 });
        var bravo = Array.CreateInstance(typeof(string), new[] { 0, 2, 4 }, new[] { 4, 2, 0 });
        yield return new object[] { alpha };
        yield return new object[] { bravo };
    }

    [Theory(DisplayName = "Empty Or Null Array Test")]
    [MemberData(nameof(EmptyArrayData))]
    public void EmptyOrNullArrayTest<T>(T values) where T : class
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        Assert.Equal("VariableBoundArrayConverter`2", converter.GetType().Name);
        var buffer = converter.Encode(values);
        var result = converter.Decode(buffer);
        Assert.Equal(((Array)(object)values).Rank, ((Array)(object)result).Rank);
        Assert.Equal(((Array)(object)values).Cast<object>(), ((Array)(object)result).Cast<object>());
        for (var i = 0; i < ((Array)(object)values).Rank; i++)
        {
            Assert.Equal(((Array)(object)values).GetLowerBound(i), ((Array)(object)result).GetLowerBound(i));
            Assert.Equal(((Array)(object)values).GetLength(i), ((Array)(object)result).GetLength(i));
        }

        var expected = new List<int>();
        var array = (Array)(object)values;
        for (var i = 0; i < array.Rank; i++)
            expected.Add(array.GetLength(i));
        for (var i = 0; i < array.Rank; i++)
            expected.Add(array.GetLowerBound(i));

        var actual = new List<int>();
        var intent = new ReadOnlySpan<byte>(buffer);
        while (intent.Length is not 0)
            actual.Add(Converter.Decode(ref intent));
        Assert.Equal(0, intent.Length);
        Assert.Equal(expected, actual);

        var nullBuffer = converter.Encode(null);
        Assert.Empty(nullBuffer);
        var nullResult = converter.Decode(Array.Empty<byte>());
        Assert.Null(nullResult);
    }

    public static IEnumerable<object[]> Array1DNonZeroBasedData()
    {
        var alpha = Array.CreateInstance(typeof(int), new[] { 10 }, new[] { 11 });
        for (var i = alpha.GetLowerBound(0); i <= alpha.GetUpperBound(0); i++)
            alpha.SetValue(i, i);
        var bravo = Array.CreateInstance(typeof(string), new[] { 20 }, new[] { 21 });
        for (var i = bravo.GetLowerBound(0); i <= bravo.GetUpperBound(0); i++)
            bravo.SetValue(i.ToString(), i);
        yield return new object[] { alpha };
        yield return new object[] { bravo };
    }

    [Theory(DisplayName = "Array1D Non Zero Based Test")]
    [MemberData(nameof(Array1DNonZeroBasedData))]
    public void Array1DNonZeroBasedTest<T>(T values) where T : class
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        Assert.Equal("VariableBoundArrayConverter`2", converter.GetType().Name);
        var buffer = converter.Encode(values);
        var result = converter.Decode(buffer);
        Assert.Equal(1, ((Array)(object)result).Rank);
        Assert.Equal(((Array)(object)values).GetLowerBound(0), ((Array)(object)result).GetLowerBound(0));
        Assert.Equal(((Array)(object)values).GetLength(0), ((Array)(object)result).GetLength(0));
        Assert.Equal(((Array)(object)values).Cast<object>(), ((Array)(object)result).Cast<object>());
    }
}
