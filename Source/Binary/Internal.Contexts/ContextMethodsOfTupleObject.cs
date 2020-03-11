using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using MemberList = System.Collections.Generic.IReadOnlyList<(System.Reflection.MemberInfo Member, Mikodev.Binary.Converter Converter)>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfTupleObject
    {
        internal static Converter GetConverterAsTupleObject(Type type, ConstructorInfo constructor, ItemIndexes indexes, MemberList metadata)
        {
            var encode = GetEncodeDelegateAsTupleObject(type, metadata, auto: false);
            var decode = GetDecodeDelegateAsTupleObject(type, metadata, constructor, indexes, auto: false);
            var encodeAuto = GetEncodeDelegateAsTupleObject(type, metadata, auto: true);
            var decodeAuto = GetDecodeDelegateAsTupleObject(type, metadata, constructor, indexes, auto: true);
            var itemLength = ContextMethods.GetItemLength(metadata.Select(x => x.Converter).ToList());
            var converterArguments = new object[] { encode, decode, encodeAuto, decodeAuto, itemLength };
            var converterType = typeof(TupleObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }

        private static MethodInfo GetEncodeMethodInfo(Converter converter, bool auto)
        {
            var converterType = converter.GetType();
            var types = new[] { typeof(Allocator).MakeByRefType(), converter.ItemType };
            var name = auto ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode);
            var method = converterType.GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static MethodInfo GetDecodeMethodInfo(Converter converter, bool auto)
        {
            var converterType = converter.GetType();
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var name = auto ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode);
            var method = converterType.GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static Delegate GetEncodeDelegateAsTupleObject(Type type, MemberList metadata, bool auto)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < metadata.Count; i++)
            {
                var (member, converter) = metadata[i];
                var memberExpression = member is FieldInfo fieldInfo
                    ? Expression.Field(item, fieldInfo)
                    : Expression.Property(item, (PropertyInfo)member);
                var method = GetEncodeMethodInfo(converter, auto || i != metadata.Count - 1);
                var invoke = Expression.Call(Expression.Constant(converter), method, allocator, memberExpression);
                expressions.Add(invoke);
            }
            var delegateType = typeof(OfTupleObject<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsTupleObject(Type type, MemberList metadata, ConstructorInfo constructor, ItemIndexes indexes, bool auto)
        {
            (ParameterExpression, Expression[]) Initialize()
            {
                var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
                var values = new Expression[metadata.Count];

                for (var i = 0; i < metadata.Count; i++)
                {
                    var converter = metadata[i].Converter;
                    var method = GetDecodeMethodInfo(converter, auto || i != metadata.Count - 1);
                    var invoke = Expression.Call(Expression.Constant(converter), method, span);
                    values[i] = invoke;
                }
                return (span, values);
            }

            if (!ContextMethods.CanCreateInstance(type, metadata.Select(x => x.Member).ToList(), constructor))
                return null;
            var delegateType = typeof(ToTupleObject<>).MakeGenericType(type);
            return constructor is null
                ? ContextMethods.GetDecodeDelegateUseMembers(delegateType, Initialize, metadata, type)
                : ContextMethods.GetDecodeDelegateUseConstructor(delegateType, Initialize, metadata.Select(x => x.Converter).ToList(), indexes, constructor);
        }
    }
}
