namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

internal sealed class GeneratorContext(ImmutableArray<IConverterCreator> creators, ConcurrentDictionary<Type, IConverter> converters, IGeneratorContextFallback? fallback = null) : IGeneratorContext, IDisposable
{
    private bool disposed = false;

    private readonly IGeneratorContextFallback? fallback = fallback;

    private readonly HashSet<Type> types = [];

    private readonly ImmutableArray<IConverterCreator> creators = creators;

    private readonly ConcurrentDictionary<Type, IConverter> converters = converters;

    public void Dispose()
    {
        this.disposed = true;
    }

    public IConverter GetConverter(Type type)
    {
        if (this.disposed)
            throw new InvalidOperationException("Generator context has been disposed!");
        var converter = GetOrCreateConverter(type);
        Debug.Assert(converter is not null);
        Debug.Assert(Converter.GetGenericArgument(converter) == type);
        return this.converters.GetOrAdd(type, converter);
    }

    private IConverter GetOrCreateConverter(Type type)
    {
        if (type.IsByRef)
            throw new ArgumentException($"Invalid byref type: {type}");
        if (type.IsByRefLike)
            throw new ArgumentException($"Invalid byref-like type: {type}");
        if (type.IsPointer)
            throw new ArgumentException($"Invalid pointer type: {type}");
        if (type.IsFunctionPointer)
            throw new ArgumentException($"Invalid function pointer type: {type}");
        if (type.IsAbstract && type.IsSealed)
            throw new ArgumentException($"Invalid static type: {type}");
        if (type.IsGenericTypeDefinition || type.IsGenericParameter)
            throw new ArgumentException($"Invalid generic type definition: {type}");

        if (this.converters.TryGetValue(type, out var converter))
            return converter;
        if (this.types.Add(type) is false)
            throw new ArgumentException($"Circular type reference detected, type: {type}");
        foreach (var creator in this.creators)
            if ((converter = creator.GetConverter(this, type)) is not null)
                return CommonModule.GetConverter(converter, type, creator.GetType());

        if (this.fallback is { } fallback)
            return fallback.GetConverter(this, type);
        throw new NotSupportedException($"No available converter found, type: {type}");
    }
}
