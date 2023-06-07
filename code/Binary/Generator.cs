namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Creators.Endianness;
using Mikodev.Binary.Creators.Isolated;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public static partial class Generator
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static IEnumerable<IConverterCreator> GetConverterCreators()
    {
        yield return new KeyValuePairConverterCreator();
        yield return new LinkedListConverterCreator();
        yield return new NullableConverterCreator();
        yield return new PriorityQueueConverterCreator();
        yield return new ReadOnlySequenceConverterCreator();
        yield return new UriConverterCreator();
        yield return new VariableBoundArrayConverterCreator();
        yield return new IsolatedConverterCreator();
        yield return new DetectEndianConverterCreator();
        yield return new DetectEndianEnumConverterCreator();
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static IGenerator CreateDefault()
    {
        return CreateDefaultBuilder().Build();
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static IGeneratorBuilder CreateDefaultBuilder()
    {
        return new GeneratorBuilder(new GeneratorContextFallback()).AddConverterCreators(GetConverterCreators());
    }
}
