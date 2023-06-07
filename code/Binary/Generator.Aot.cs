namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Creators.Endianness;
using Mikodev.Binary.Creators.Isolated;
using Mikodev.Binary.Internal.Contexts;
using System.Collections.Generic;

public static partial class Generator
{
    private static IEnumerable<IConverterCreator> GetAotConverterCreators()
    {
        yield return new UriConverterCreator();
        yield return new IsolatedConverterCreator();
        yield return new DetectEndianConverterCreator();
    }

    public static IGenerator CreateAot()
    {
        return CreateAotBuilder().Build();
    }

    public static IGeneratorBuilder CreateAotBuilder()
    {
        return new GeneratorBuilder().AddConverterCreators(GetAotConverterCreators());
    }
}
