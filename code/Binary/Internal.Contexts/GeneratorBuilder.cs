namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

[DebuggerDisplay(CommonModule.DebuggerDisplayValue)]
internal sealed class GeneratorBuilder : IGeneratorBuilder
{
    private readonly LinkedList<IConverterCreator> creators = new LinkedList<IConverterCreator>();

    private readonly Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();

    public IGeneratorBuilder AddConverter(IConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        var itemType = Converter.GetGenericArgument(converter);
        if (itemType == typeof(object))
            throw new ArgumentException($"Can not add converter for '{typeof(object)}'");
        this.converters[itemType] = converter;
        return this;
    }

    public IGeneratorBuilder AddConverterCreator(IConverterCreator creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        _ = this.creators.AddFirst(creator);
        return this;
    }

    public IGenerator Build() => new Generator(this.creators.ToImmutableArray(), this.converters.ToImmutableDictionary());

    public override string ToString() => $"Converter Count = {this.converters.Count}, Converter Creator Count = {this.creators.Count}";
}
