using Mikodev.Binary.Converters;
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
        private static readonly IReadOnlyList<IConverterCreator> sharedCreators;

        static Generator()
        {
            sharedCreators = typeof(Converter).Assembly.GetTypes()
                .Where(x => !x.IsAbstract && typeof(IConverterCreator).IsAssignableFrom(x))
                .OrderBy(x => x.Namespace)
                .ThenBy(x => x.Name)
                .Select(x => (IConverterCreator)Activator.CreateInstance(x))
                .ToArray();
        }

        private ConcurrentDictionary<Type, Converter> CombineConverters(IEnumerable<Converter> values)
        {
            var result = new ConcurrentDictionary<Type, Converter>();
            // add user-defined converters
            if (values != null)
                foreach (var i in values.Where(x => x != null))
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

        private readonly ConcurrentDictionary<Type, Converter> converters;

        private readonly IReadOnlyList<IConverterCreator> creators;

        public Generator() : this(null, null) { }

        public Generator(IEnumerable<Converter> converters = null, IEnumerable<IConverterCreator> creators = null)
        {
            this.converters = CombineConverters(converters);
            this.creators = CombineCreators(creators);
            Debug.Assert(this.converters != null);
            Debug.Assert(this.creators != null);
        }

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

        public byte[] Encode<T>(T item) => GetConverter<T>().Encode(item);

        public byte[] Encode(object item, Type type) => ((IConverter)GetConverter(type)).Encode(item);

        public object Decode(in ReadOnlySpan<byte> span, Type type) => ((IConverter)GetConverter(type)).Decode(in span);

        public T Decode<T>(in ReadOnlySpan<byte> span) => GetConverter<T>().Decode(in span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Decode<T>(in ReadOnlySpan<byte> span, T anonymous) => Decode<T>(in span);

        public Token AsToken(in ReadOnlyMemory<byte> memory) => new Token(this, in memory);

        public object Decode(byte[] buffer, Type type) => ((IConverter)GetConverter(type)).Decode(buffer);

        public T Decode<T>(byte[] buffer) => GetConverter<T>().Decode(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Decode<T>(byte[] buffer, T anonymous) => Decode<T>(buffer);

        public Token AsToken(byte[] buffer) => new Token(this, buffer);

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override bool Equals(object obj) => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override int GetHashCode() => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override string ToString() => $"{nameof(Generator)}(Converters: {converters.Count}, Creators: {creators.Count})";
    }
}
