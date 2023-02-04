namespace Mikodev.Binary.Experimental.Tests;

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
        var generator = Generator.CreateDefaultBuilder().AddConverterCreator(new VariableBoundArrayConverterCreator()).Build();
        var converter = generator.GetConverter<T[,]>();
        Assert.Equal(typeof(VariableBoundArrayConverter<T[,], T>), converter.GetType());
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
}
