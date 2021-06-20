using Mikodev.Binary.External;
using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Contexts.Template;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfNamedObject
    {
        private static readonly MethodInfo InvokeMethodInfo = CommonHelper.GetMethod(typeof(NamedObjectTemplates), nameof(NamedObjectTemplates.GetIndexSpan), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo AppendMethodInfo = CommonHelper.GetMethod(typeof(Allocator), nameof(Allocator.Append), new[] { typeof(Allocator).MakeByRefType(), typeof(ReadOnlySpan<byte>) });

        private static readonly ConstructorInfo ReadOnlySpanByteConstructorInfo = CommonHelper.GetConstructor(typeof(ReadOnlySpan<byte>), new[] { typeof(byte[]) });

        internal static IConverter GetConverterAsNamedObject(Type type, ContextObjectConstructor constructor, ImmutableArray<IConverter> converters, ImmutableArray<PropertyInfo> properties, ImmutableArray<string> names, Converter<string> encoder)
        {
            var memories = names.Select(x => new ReadOnlyMemory<byte>(encoder.Encode(x))).ToImmutableArray();
            Debug.Assert(properties.Length == names.Length);
            Debug.Assert(properties.Length == memories.Length);
            Debug.Assert(properties.Length == converters.Length);
            var dictionary = BinaryObject.Create(memories);
            if (dictionary is null)
                throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {encoder.GetType()}");
            var encode = GetEncodeDelegateAsNamedObject(type, converters, properties, memories);
            var decode = GetDecodeDelegateAsNamedObject(type, converters, constructor);
            var converterArguments = new object[] { encode, decode, names, dictionary };
            var converterType = typeof(NamedObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }

        private static Delegate GetEncodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<PropertyInfo> properties, ImmutableArray<ReadOnlyMemory<byte>> memories)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var converter = converters[i];
                var buffer = Allocator.Invoke(memories[i], (ref Allocator allocator, ReadOnlyMemory<byte> data) => Converter.EncodeWithLengthPrefix(ref allocator, data.Span));
                var methodInfo = ((IConverterMetadata)converter).GetMethod(nameof(IConverter.EncodeWithLengthPrefix));
                // append named key with length prefix (cached), then append value with length prefix
                expressions.Add(Expression.Call(AppendMethodInfo, allocator, Expression.New(ReadOnlySpanByteConstructorInfo, Expression.Constant(buffer))));
                expressions.Add(Expression.Call(Expression.Constant(converter), methodInfo, allocator, Expression.Property(item, property)));
            }

            var delegateType = typeof(EncodeDelegate<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ContextObjectConstructor constructor)
        {
            ImmutableArray<Expression> Initialize(ImmutableArray<ParameterExpression> parameters)
            {
                Debug.Assert(parameters.Length is 2);
                var source = parameters[0];
                var values = parameters[1];
                var result = ImmutableArray.CreateBuilder<Expression>(converters.Length);
                for (var i = 0; i < converters.Length; i++)
                {
                    var converter = converters[i];
                    var method = ((IConverterMetadata)converter).GetMethod(nameof(IConverter.Decode));
                    var invoke = Expression.Call(InvokeMethodInfo, source, values, Expression.Constant(i));
                    var decode = Expression.Call(Expression.Constant(converter), method, invoke);
                    result.Add(decode);
                }
                Debug.Assert(converters.Length == result.Count);
                Debug.Assert(converters.Length == result.Capacity);
                return result.MoveToImmutable();
            }

            return constructor?.Invoke(typeof(NamedObjectDecodeDelegate<>).MakeGenericType(type), Initialize);
        }
    }
}
