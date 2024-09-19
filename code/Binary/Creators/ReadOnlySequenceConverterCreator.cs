namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class ReadOnlySequenceConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return CommonModule.GetConverter(context, type, typeof(ReadOnlySequence<>), typeof(ReadOnlySequenceConverter<>));
    }
}
