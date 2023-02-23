namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[DebuggerDisplay(CommonModule.DebuggerDisplayValue)]
internal sealed class Generator : IGenerator
{
    private readonly ImmutableArray<IConverterCreator> creators;

    private readonly ConcurrentDictionary<Type, IConverter> converters;

    public Generator(ImmutableArray<IConverterCreator> creators, ImmutableDictionary<Type, IConverter> converters)
    {
        this.creators = creators;
        this.converters = new ConcurrentDictionary<Type, IConverter>(converters) { [typeof(object)] = new GeneratorObjectConverter(this) };
        Debug.Assert(this.converters.All(x => x.Value is not null));
        Debug.Assert(this.creators.Length is 0 || this.creators.All(x => x is not null));
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter GetConverter(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (this.converters.TryGetValue(type, out var result))
            return result;
        // auto dispose context to prevent reuse
        using var source = new GeneratorContext(this.creators, this.converters);
        return source.GetConverter(type);
    }

    public override string ToString() => $"Converter Count = {this.converters.Count}, Converter Creator Count = {this.creators.Length}";
}
