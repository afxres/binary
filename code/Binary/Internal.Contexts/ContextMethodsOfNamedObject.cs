using Mikodev.Binary.External;
using Mikodev.Binary.Internal.Contexts.Instance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfNamedObject
    {
        private static readonly MethodInfo InvokeMethodInfo = CommonHelper.GetMethod(typeof(MemorySlices), nameof(MemorySlices.Invoke), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo AppendMethodInfo = CommonHelper.GetMethod(typeof(Allocator), nameof(Allocator.Append), new[] { typeof(Allocator).MakeByRefType(), typeof(ReadOnlySpan<byte>) });

        private static readonly ConstructorInfo ReadOnlySpanByteConstructorInfo = CommonHelper.GetConstructor(typeof(ReadOnlySpan<byte>), new[] { typeof(byte[]) });

        internal static IConverter GetConverterAsNamedObject(Type type, ContextObjectConstructor constructor, IReadOnlyList<IConverter> converters, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<string> names, Converter<string> encoder)
        {
            var memories = names.Select(x => new ReadOnlyMemory<byte>(encoder.Encode(x))).ToList();
            Debug.Assert(properties.Count == names.Count);
            Debug.Assert(properties.Count == memories.Count);
            Debug.Assert(properties.Count == converters.Count);
            var dictionary = BinaryDictionary<int>.Create(memories.Select((x, i) => new KeyValuePair<ReadOnlyMemory<byte>, int>(x, i)).ToList(), -1);
            if (dictionary is null)
                throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {encoder.GetType()}");
            var encode = GetEncodeDelegateAsNamedObject(type, converters, properties, memories);
            var decode = GetDecodeDelegateAsNamedObject(type, converters, properties, constructor);
            var converterArguments = new object[] { encode, decode, names, dictionary };
            var converterType = typeof(NamedObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }

        private static Delegate GetEncodeDelegateAsNamedObject(Type type, IReadOnlyList<IConverter> converters, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<ReadOnlyMemory<byte>> memories)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var converter = converters[i];
                var buffer = Allocator.Invoke(memories[i], (ref Allocator allocator, ReadOnlyMemory<byte> data) => Converter.EncodeWithLengthPrefix(ref allocator, data.Span));
                var methodInfo = ContextMethods.GetEncodeMethodInfo(property.PropertyType, nameof(IConverter.EncodeWithLengthPrefix));
                // append named key with length prefix (cached), then append value with length prefix
                expressions.Add(Expression.Call(AppendMethodInfo, allocator, Expression.New(ReadOnlySpanByteConstructorInfo, Expression.Constant(buffer))));
                expressions.Add(Expression.Call(Expression.Constant(converter), methodInfo, allocator, Expression.Property(item, property)));
            }

            var delegateType = typeof(NamedObjectEncoder<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsNamedObject(Type type, IReadOnlyList<IConverter> converters, IReadOnlyList<PropertyInfo> properties, ContextObjectConstructor constructor)
        {
            IReadOnlyList<Expression> Initialize(ParameterExpression slices)
            {
                var results = new Expression[properties.Count];
                for (var i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var converter = converters[i];
                    var method = ContextMethods.GetDecodeMethodInfo(property.PropertyType, nameof(IConverter.Decode));
                    var invoke = Expression.Call(slices, InvokeMethodInfo, Expression.Constant(i));
                    var decode = Expression.Call(Expression.Constant(converter), method, invoke);
                    results[i] = decode;
                }
                return results;
            }

            return constructor?.Invoke(typeof(NamedObjectDecoder<>).MakeGenericType(type), Initialize);
        }
    }
}
