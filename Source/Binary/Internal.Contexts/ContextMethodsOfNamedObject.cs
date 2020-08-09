using Mikodev.Binary.External;
using Mikodev.Binary.Internal.Contexts.Models;
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
        private static readonly MethodInfo InvokeMethodInfo = typeof(MemorySlices).GetMethod(nameof(MemorySlices.Invoke), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo AppendMethodInfo = typeof(Allocator).GetMethod(nameof(Allocator.AppendBuffer), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConstructorInfo ReadOnlySpanByteConstructorInfo = typeof(ReadOnlySpan<byte>).GetConstructor(new[] { typeof(byte[]) });

        internal static IConverter GetConverterAsNamedObject(IGeneratorContext context, Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<IConverter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes, IReadOnlyDictionary<PropertyInfo, string> dictionary)
        {
            // require string converter for named key
            var stringConverter = (Converter<string>)context.GetConverter(typeof(string));
            var memories = dictionary.ToDictionary(x => x.Key, x => new ReadOnlyMemory<byte>(stringConverter.Encode(x.Value)));
            Debug.Assert(dictionary.Count == memories.Count);
            Debug.Assert(dictionary.OrderBy(x => x.Value).Select(x => x.Key).SequenceEqual(properties));
            Debug.Assert(properties.Count == converters.Count);
            var nameList = properties.Select(x => dictionary[x]).ToList();
            var nodeTree = NodeTreeHelper.MakeOrNull(properties.Select((x, i) => new KeyValuePair<ReadOnlyMemory<byte>, int>(memories[x], i)).ToList());
            if (nodeTree is null)
                throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {stringConverter.GetType()}");
            var encode = GetEncodeDelegateAsNamedObject(type, properties, converters, memories);
            var decode = GetDecodeDelegateAsNamedObject(type, properties, converters, constructor, indexes);
            var converterArguments = new object[] { encode, decode, nodeTree, nameList };
            var converterType = typeof(NamedObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }

        private static MethodInfo GetEncodeWithLengthPrefixMethodInfo(Type itemType)
        {
            var types = new[] { typeof(Allocator).MakeByRefType(), itemType };
            var name = nameof(IConverter.EncodeWithLengthPrefix);
            var method = typeof(Converter<>).MakeGenericType(itemType).GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static Delegate GetEncodeDelegateAsNamedObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<IConverter> converters, IReadOnlyDictionary<PropertyInfo, ReadOnlyMemory<byte>> memories)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var converter = converters[i];
                var buffer = AllocatorHelper.Invoke(memories[property], (ref Allocator allocator, ReadOnlyMemory<byte> data) => PrimitiveHelper.EncodeBufferWithLengthPrefix(ref allocator, data.Span));
                var methodInfo = GetEncodeWithLengthPrefixMethodInfo(property.PropertyType);
                // append named key with length prefix (cached), then append value with length prefix
                expressions.Add(Expression.Call(AppendMethodInfo, allocator, Expression.New(ReadOnlySpanByteConstructorInfo, Expression.Constant(buffer))));
                expressions.Add(Expression.Call(Expression.Constant(converter), methodInfo, allocator, Expression.Property(item, property)));
            }

            var delegateType = typeof(OfNamedObject<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsNamedObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<IConverter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes)
        {
            IReadOnlyList<Expression> Initialize(ParameterExpression slices)
            {
                var values = new Expression[properties.Count];
                for (var i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var converter = converters[i];
                    var method = InvokeMethodInfo.MakeGenericMethod(property.PropertyType);
                    var invoke = Expression.Call(slices, method, Expression.Constant(converter), Expression.Constant(i));
                    values[i] = invoke;
                }
                return values;
            }

            if (!ContextMethods.CanCreateInstance(type, properties, constructor))
                return null;
            var delegateType = typeof(ToNamedObject<>).MakeGenericType(type);
            var parameterType = typeof(MemorySlices).MakeByRefType();
            return constructor is null
                ? ContextMethods.GetDecodeDelegateUseMembers(delegateType, parameterType, Initialize, properties.Select(x => new Func<Expression, Expression>(e => Expression.Property(e, x))).ToList(), type)
                : ContextMethods.GetDecodeDelegateUseConstructor(delegateType, parameterType, Initialize, indexes, constructor);
        }
    }
}
