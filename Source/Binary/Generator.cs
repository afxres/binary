using Mikodev.Binary.Converters;
using Mikodev.Binary.Converters.Default;
using Mikodev.Binary.Converters.Unsafe;
using Mikodev.Binary.Converters.Unsafe.Generic;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public sealed class Generator : IGenerator
    {
        #region private & static
        private static readonly IReadOnlyList<Converter> sharedConverters;

        private static readonly IReadOnlyList<IConverterCreator> sharedCreators;

        static Generator()
        {
            var converterTypes = new[]
            {
                typeof(UriConverter),
                typeof(StringConverter),
                typeof(IPAddressConverter),
                typeof(IPEndPointConverter),
                typeof(UnsafeDateTimeConverter),
                typeof(UnsafeDateTimeOffsetConverter),
                typeof(UnsafeTimeSpanConverter),
                typeof(UnsafeGuidConverter),
                typeof(UnsafeDecimalConverter),
            };

            var types = new[]
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
            };

            sharedConverters = converterTypes
                .Concat(types.Select(x => typeof(UnsafePrimitiveConverter<>).MakeGenericType(x)))
                .Select(x => (Converter)Activator.CreateInstance(x))
                .OrderBy(x => x.Length)
                .ThenBy(x => x.ItemType.Name)
                .ToArray();

            sharedCreators = typeof(Converter).Assembly.GetTypes()
                .Where(x => !x.IsAbstract && typeof(IConverterCreator).IsAssignableFrom(x))
                .OrderBy(x => x.Namespace)
                .ThenBy(x => x.Name)
                .Select(x => (IConverterCreator)Activator.CreateInstance(x))
                .ToArray();

            Debug.Assert(sharedConverters.Any(x => x.ItemType == typeof(int)));
        }

        private ConcurrentDictionary<Type, Converter> CombineConverters(IEnumerable<Converter> values)
        {
            var result = new ConcurrentDictionary<Type, Converter>();
            // add user-defined converters
            if (values != null)
                foreach (var i in values.Where(x => x != null))
                    _ = result.TryAdd(i.ItemType, i);
            // try add internal converters
            foreach (var i in sharedConverters)
                _ = result.TryAdd(i.ItemType, i);
            // set object converter
            result[typeof(object)] = new ObjectConverter(this);
            return result;
        }

        private List<IConverterCreator> CombineCreators(IEnumerable<IConverterCreator> values)
        {
            var result = new List<IConverterCreator>();
            // add user-defined creators
            if (values != null)
                foreach (var i in values.Where(x => x != null))
                    result.Add(i);
            // add internal creators
            result.AddRange(sharedCreators);
            return result;
        }
        #endregion

        #region private readonly fields
        private readonly ConcurrentDictionary<Type, Converter> converters;

        private readonly IReadOnlyList<IConverterCreator> creators;
        #endregion

        public Generator() : this(null, null) { }

        public Generator(IEnumerable<Converter> converters = null, IEnumerable<IConverterCreator> creators = null)
        {
            this.converters = CombineConverters(converters);
            this.creators = CombineCreators(creators);
            Debug.Assert(this.converters != null);
            Debug.Assert(this.creators != null);
        }

        #region get or generate converter
        public Converter GetConverter(Type type)
        {
            if (type == null)
                ThrowHelper.ThrowArgumentNull(nameof(type));
            if (converters.TryGetValue(type, out var result))
                return result;
            var context = new GeneratorContext(converters, creators);
            return context.GetConverter(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Converter<T> GetConverter<T>() => (Converter<T>)GetConverter(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Converter<T> GetConverter<T>(T anonymous) => GetConverter<T>();
        #endregion

        #region to bytes
        public byte[] ToBytes<T>(T item) => GetConverter<T>().ToBytes(item);

        public byte[] ToBytes(object item, Type type) => ((IConverter)GetConverter(type)).ToBytes(item);
        #endregion

        #region to value
        public object ToValue(in ReadOnlySpan<byte> span, Type type) => ((IConverter)GetConverter(type)).ToValue(in span);

        public T ToValue<T>(in ReadOnlySpan<byte> span) => GetConverter<T>().ToValue(in span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ToValue<T>(in ReadOnlySpan<byte> span, T anonymous) => ToValue<T>(in span);

        public Token AsToken(in ReadOnlyMemory<byte> memory) => new Token(this, in memory);
        #endregion

        #region to value (bytes)
        public object ToValue(byte[] buffer, Type type) => ((IConverter)GetConverter(type)).ToValue(buffer);

        public T ToValue<T>(byte[] buffer) => GetConverter<T>().ToValue(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ToValue<T>(byte[] buffer, T anonymous) => ToValue<T>(buffer);

        public Token AsToken(byte[] buffer) => new Token(this, buffer);
        #endregion

        #region override
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override bool Equals(object obj) => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override int GetHashCode() => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override string ToString() => $"{nameof(Generator)}(Converters: {converters.Count}, Creators: {creators.Count})";
        #endregion
    }
}
