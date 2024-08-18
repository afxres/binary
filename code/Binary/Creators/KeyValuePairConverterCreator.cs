namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class KeyValuePairConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return CommonModule.GetConverter(context, type, typeof(KeyValuePair<,>), typeof(KeyValuePairConverter<,>));
    }
}
