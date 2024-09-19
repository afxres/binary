namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class GeneratorContextFallback : IGeneratorContextFallback
{
    public IConverter GetConverter(IGeneratorContext context, Type type)
    {
        IConverter? converter;
        if ((converter = FallbackPrimitivesMethods.GetConverter(context, type)) is not null)
            return converter;
        if ((converter = FallbackSequentialMethods.GetConverter(context, type)) is not null)
            return converter;

        if (type == typeof(Delegate) || type.IsSubclassOf(typeof(Delegate)))
            throw new ArgumentException($"Invalid delegate type: {type}");
        if (type.Assembly == typeof(IConverter).Assembly)
            throw new ArgumentException($"Invalid internal type: {type}");

        var typeInfo = new MetaTypeInfo(type);
        if (typeInfo.Attributes.Length is 0 && (converter = FallbackCollectionMethods.GetConverter(context, type)) is not null)
            return converter;

        if (typeInfo.Attributes.Length is 0 && (type == typeof(IEnumerable) || typeof(IEnumerable).IsAssignableFrom(type)))
            throw new ArgumentException($"Invalid non-generic collection type: {type}");
        if (type.Assembly == typeof(object).Assembly)
            throw new ArgumentException($"Invalid system type: {type}");

        return FallbackAttributesMethods.GetConverter(context, typeInfo);
    }
}
