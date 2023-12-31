namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
internal sealed class InlineArrayConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type.IsValueType is false || type.GetCustomAttributes(false).OfType<InlineArrayAttribute>().FirstOrDefault() is not { } attribute)
            return null;
        var itemType = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Single().FieldType;
        var itemConverter = context.GetConverter(itemType);
        var converterType = typeof(InlineArrayConverter<,>).MakeGenericType(type, itemType);
        var converter = CommonModule.CreateInstance(converterType, [itemConverter, attribute.Length]);
        return (IConverter)converter;
    }
}
