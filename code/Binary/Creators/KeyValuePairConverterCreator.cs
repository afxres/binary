namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

internal sealed class KeyValuePairConverterCreator : IConverterCreator
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return CommonModule.GetConverter(context, type, typeof(KeyValuePair<,>), typeof(KeyValuePairConverter<,>));
    }
}
