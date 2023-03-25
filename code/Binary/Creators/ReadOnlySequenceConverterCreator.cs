namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

internal sealed class ReadOnlySequenceConverterCreator : IConverterCreator
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return CommonModule.GetConverter(context, type, typeof(ReadOnlySequence<>), typeof(ReadOnlySequenceConverter<>));
    }
}
