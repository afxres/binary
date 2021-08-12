namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Collections.Immutable;

internal sealed class GeneratorBuilder : IGeneratorBuilder
{
    private readonly ImmutableArray<IConverterCreator>.Builder creators = ImmutableArray.CreateBuilder<IConverterCreator>();

    private readonly ImmutableDictionary<Type, IConverter>.Builder converters = ImmutableDictionary.CreateBuilder<Type, IConverter>();

    public IGeneratorBuilder AddConverter(IConverter converter)
    {
        if (converter is null)
            throw new ArgumentNullException(nameof(converter));
        var itemType = Converter.GetGenericArgument(converter);
        if (itemType == typeof(object))
            throw new ArgumentException($"Can not add converter for '{typeof(object)}'");
        this.converters[itemType] = converter;
        return this;
    }

    public IGeneratorBuilder AddConverterCreator(IConverterCreator creator)
    {
        if (creator is null)
            throw new ArgumentNullException(nameof(creator));
        this.creators.Add(creator);
        return this;
    }

    public IGenerator Build() => new Generator(this.creators.ToImmutable(), this.converters.ToImmutable());

    public override string ToString() => $"{nameof(GeneratorBuilder)}(Converters: {this.converters.Count}, Creators: {this.creators.Count})";
}
