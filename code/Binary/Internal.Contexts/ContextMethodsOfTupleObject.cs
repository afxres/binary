using Mikodev.Binary.Internal.Contexts.Instance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfTupleObject
    {
        internal static IConverter GetConverterAsTupleObject(Type type, ContextObjectConstructor constructor, IReadOnlyList<IConverter> converters, IReadOnlyList<Type> types, IReadOnlyList<ContextMemberInitializer> members)
        {
            Debug.Assert(converters.Count == types.Count);
            Debug.Assert(converters.Count == members.Count);
            var encode = GetEncodeDelegateAsTupleObject(type, types, converters, members, auto: false);
            var encodeAuto = GetEncodeDelegateAsTupleObject(type, types, converters, members, auto: true);
            var decode = GetDecodeDelegateAsTupleObject(type, types, converters, constructor, auto: false);
            var decodeAuto = GetDecodeDelegateAsTupleObject(type, types, converters, constructor, auto: true);
            var itemLength = ContextMethods.GetItemLength(converters);
            var converterArguments = new object[] { encode, encodeAuto, decode, decodeAuto, itemLength };
            var converterType = typeof(TupleObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }

        private static MethodInfo GetEncodeMethodInfo(Type itemType, bool auto)
        {
            var types = new[] { typeof(Allocator).MakeByRefType(), itemType };
            var name = auto ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode);
            var method = typeof(Converter<>).MakeGenericType(itemType).GetMethod(name, types);
            Debug.Assert(method is not null);
            return method;
        }

        private static MethodInfo GetDecodeMethodInfo(Type itemType, bool auto)
        {
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var name = auto ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode);
            var method = typeof(Converter<>).MakeGenericType(itemType).GetMethod(name, types);
            Debug.Assert(method is not null);
            return method;
        }

        private static Delegate GetEncodeDelegateAsTupleObject(Type type, IReadOnlyList<Type> types, IReadOnlyList<IConverter> converters, IReadOnlyList<ContextMemberInitializer> members, bool auto)
        {
            Debug.Assert(converters.Count == types.Count);
            Debug.Assert(converters.Count == members.Count);
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < types.Count; i++)
            {
                var itemType = types[i];
                var converter = converters[i];
                var method = GetEncodeMethodInfo(itemType, auto || i != types.Count - 1);
                var invoke = Expression.Call(Expression.Constant(converter), method, allocator, members[i].Invoke(item));
                expressions.Add(invoke);
            }
            var delegateType = typeof(TupleObjectEncoder<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsTupleObject(Type type, IReadOnlyList<Type> types, IReadOnlyList<IConverter> converters, ContextObjectConstructor constructor, bool auto)
        {
            IReadOnlyList<Expression> Initialize(ParameterExpression span)
            {
                var values = new Expression[types.Count];
                for (var i = 0; i < types.Count; i++)
                {
                    var itemType = types[i];
                    var converter = converters[i];
                    var method = GetDecodeMethodInfo(itemType, auto || i != types.Count - 1);
                    var invoke = Expression.Call(Expression.Constant(converter), method, span);
                    values[i] = invoke;
                }
                return values;
            }

            return constructor?.Invoke(typeof(TupleObjectDecoder<>).MakeGenericType(type), Initialize);
        }
    }
}
