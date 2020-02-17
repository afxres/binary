﻿using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;
using NameDictionary = System.Collections.Generic.IReadOnlyDictionary<System.Reflection.PropertyInfo, string>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfNamedObject
    {
        private static readonly MethodInfo invokeMethodInfo = typeof(LengthList).GetMethod(nameof(LengthList.Invoke), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo appendMethodInfo = typeof(Allocator).GetMethod(nameof(Allocator.AppendBuffer), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConstructorInfo bufferConstructorInfo = typeof(ReadOnlySpan<byte>).GetConstructor(new[] { typeof(byte[]) });

        internal static Converter GetConverterAsNamedObject(IGeneratorContext context, Type type, ConstructorInfo constructor, ItemIndexes indexes, MetaList metadata, NameDictionary dictionary)
        {
            // require string converter for named key
            var stringConverter = (Converter<string>)context.GetConverter(typeof(string));
            var stringContainer = dictionary.Values.ToDictionary(x => x, x => new ReadOnlyMemory<byte>(stringConverter.Encode(x)));
            Debug.Assert(dictionary.Count == stringContainer.Count);
            Debug.Assert(dictionary.OrderBy(x => x.Value).Select(x => x.Key).SequenceEqual(metadata.Select(x => x.Property)));
            var names = metadata.Select(x => dictionary[x.Property]).ToList();
            var entry = BinaryNodeHelper.CreateOrDefault(names.Select((x, i) => (stringContainer[x], i)).ToList());
            if (entry is null)
                throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {stringConverter.GetType()}");
            var encode = GetEncodeDelegateAsNamedObject(type, metadata, dictionary, stringContainer);
            var decode = GetDecodeDelegateAsNamedObject(type, metadata, constructor, indexes);
            var converterArguments = new object[] { encode, decode, entry, names };
            var converterType = typeof(NamedObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }

        private static Delegate GetEncodeDelegateAsNamedObject(Type type, MetaList metadata, NameDictionary dictionary, IReadOnlyDictionary<string, ReadOnlyMemory<byte>> buffers)
        {
            byte[] EncodeStringWithLengthPrefix(string text)
            {
                var allocator = new Allocator();
                PrimitiveHelper.EncodeBufferWithLengthPrefix(ref allocator, buffers[text].Span);
                return allocator.AsSpan().ToArray();
            }

            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var buffer = EncodeStringWithLengthPrefix(dictionary[property]);
                var propertyType = property.PropertyType;
                var propertyExpression = Expression.Property(item, property);
                var methodInfo = typeof(Converter<>).MakeGenericType(propertyType).GetMethod(nameof(IConverter.EncodeWithLengthPrefix));
                // append named key with length prefix (cached), then append value with length prefix
                expressions.Add(Expression.Call(appendMethodInfo, allocator, Expression.New(bufferConstructorInfo, Expression.Constant(buffer))));
                expressions.Add(Expression.Call(Expression.Constant(converter), methodInfo, allocator, propertyExpression));
            }
            var delegateType = typeof(OfNamedObject<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsNamedObject(Type type, MetaList metadata, ConstructorInfo constructor, ItemIndexes indexes)
        {
            (ParameterExpression, Expression[]) Initialize()
            {
                var list = Expression.Parameter(typeof(LengthList), "list");
                var values = new Expression[metadata.Count];

                for (var i = 0; i < metadata.Count; i++)
                {
                    var converter = metadata[i].Converter;
                    var method = invokeMethodInfo.MakeGenericMethod(converter.ItemType);
                    var invoke = Expression.Call(list, method, Expression.Constant(converter), Expression.Constant(i));
                    values[i] = invoke;
                }
                return (list, values);
            }

            if (!ContextMethods.CanCreateInstance(type, metadata.Select(x => x.Property).ToList(), constructor))
                return null;
            var delegateType = typeof(ToNamedObject<>).MakeGenericType(type);
            return constructor == null
                ? ContextMethods.GetDecodeDelegateUseMembers(delegateType, Initialize, metadata.Select(x => ((MemberInfo)x.Property, x.Converter)).ToList(), type)
                : ContextMethods.GetDecodeDelegateUseConstructor(delegateType, Initialize, metadata.Select(x => x.Converter).ToList(), indexes, constructor);
        }
    }
}
