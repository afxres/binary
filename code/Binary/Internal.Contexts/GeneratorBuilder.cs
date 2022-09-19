namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Collections.Immutable;
using System.Diagnostics;

[DebuggerDisplay(CommonModule.DebuggerDisplayValue)]
internal sealed class GeneratorBuilder : IGeneratorBuilder
{
    private readonly ImmutableArray<IConverterCreator>.Builder creators = ImmutableArray.CreateBuilder<IConverterCreator>();

    private readonly ImmutableDictionary<Type, IConverter>.Builder converters = ImmutableDictionary.CreateBuilder<Type, IConverter>();

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
        this.creators.Add(creator);
        return this;
    }

    public IGenerator Build() => new Generator(this.creators.ToImmutable(), this.converters.ToImmutable());

    public override string ToString() => $"Converter Count = {this.converters.Count}, Converter Creator Count = {this.creators.Count}";
}
