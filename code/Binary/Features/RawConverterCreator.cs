namespace Mikodev.Binary.Features;

using Mikodev.Binary.Features.Instance;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;

#if NET7_0_OR_GREATER
internal sealed class RawConverterCreator : IConverterCreator
{
    private static readonly ImmutableArray<Type> Types = ImmutableArray.Create(new[]
{
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(Half),
        typeof(BitVector32),
        typeof(Int128),
        typeof(UInt128),
    });

    private static readonly ImmutableDictionary<Type, IConverter> SharedConverters;

    static RawConverterCreator()
    {
        var query =
            from i in typeof(IConverter).Assembly.GetTypes()
            where i.Namespace is "Mikodev.Binary.Features.Instance" && i.IsGenericType is false
            let k = i.GetInterfaces().Single().GetGenericArguments().Single()
            let t = typeof(RawConverter<,>).MakeGenericType(k, i)
            let v = (IConverter)CommonModule.CreateInstance(t, null)
            select KeyValuePair.Create(k, v);
        SharedConverters = ImmutableDictionary.CreateRange(query);
    }

    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(Type type, bool native)
        {
            if (Types.Contains(type) is false && type.IsEnum is false)
                return null;
            var definition = native
                ? typeof(NativeEndianRawConverter<>)
                : typeof(LittleEndianRawConverter<>);
            var converterType = typeof(RawConverter<,>).MakeGenericType(type, definition.MakeGenericType(type));
            var converter = CommonModule.CreateInstance(converterType, null);
            return (IConverter)converter;
        }

        if (SharedConverters.TryGetValue(type, out var result))
            return result;
        return Invoke(type, BitConverter.IsLittleEndian);
    }
}
#endif
