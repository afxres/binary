namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

internal sealed class NullableConverterCreator : IConverterCreator
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return CommonModule.GetConverter(context, type, typeof(Nullable<>), typeof(NullableConverter<>), null);
    }
}
