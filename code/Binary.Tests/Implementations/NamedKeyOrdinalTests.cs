namespace Mikodev.Binary.Tests.Implementations;

using Mikodev.Binary;
using Mikodev.Binary.Attributes;
using Xunit;

public class NamedKeyOrdinalTests
{
    private static readonly IGenerator Instance = Generator.CreateDefault();

    [NamedObject]
    private class ComposedAndDecomposedKeys
    {
        // equal under culture-aware comparison, distinct under ordinal comparison
        [NamedKey("\u00E9")]
        public int Composed { get; set; }

        [NamedKey("e\u0301")]
        public int Decomposed { get; set; }
    }

    [Fact(DisplayName = "Named Keys Compared By Ordinal")]
    public void NamedKeysComparedByOrdinal()
    {
        var converter = Instance.GetConverter<ComposedAndDecomposedKeys>();
        var source = new ComposedAndDecomposedKeys { Composed = 1024, Decomposed = 2048 };
        var bytes = Instance.Encode(source);
        var result = converter.Decode(bytes);
        Assert.Equal(1024, result.Composed);
        Assert.Equal(2048, result.Decomposed);
    }
}
