namespace Mikodev.Binary.Experimental.Tests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

public class MultidimensionalArrayConverterTests
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

    [Theory(DisplayName = "Array2D Test")]
    [MemberData(nameof(Array2DData))]
    public void Array2DTest<T>(T[,] values)
    {
        var generator = Generator.CreateDefaultBuilder().AddConverterCreator(new MultidimensionalArrayConverterCreator()).Build();
        var converter = generator.GetConverter<T[,]>();
        Assert.Equal(typeof(Array2DConverter<T>), converter.GetType());
        var buffer = converter.Encode(values);
        var result = converter.Decode(buffer);
        Assert.False(ReferenceEquals(values, result));
        Assert.Equal(values.GetLength(0), result.GetLength(0));
        Assert.Equal(values.GetLength(1), result.GetLength(1));
        var sourceSequence = values.Cast<T>().ToList();
        var actualSequence = values.Cast<T>().ToList();
        Assert.Equal(sourceSequence, actualSequence);
    }
}
