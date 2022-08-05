namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

internal sealed class UriConverterCreator : IConverterCreator
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return type == typeof(Uri) ? new UriConverter((Converter<string>)context.GetConverter(typeof(string))) : null;
    }
}
