using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using ItemInitializer = System.Func<(System.Linq.Expressions.ParameterExpression, System.Linq.Expressions.Expression[])>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethods
    {
        internal static int GetConverterLength(Type type, IReadOnlyCollection<Converter> values)
        {
            Debug.Assert(values.Any() && values.All(x => x != null && x.Length >= 0));
            var length = values.All(x => x.Length > 0) ? values.Sum(x => (long)x.Length) : 0;
            if (length < 0 || length > int.MaxValue)
                throw new ArgumentException($"Converter length overflow, type: {type}");
            return (int)length;
        }

        internal static MethodInfo GetEncodeMethodInfo(Type type, bool isAuto)
        {
            var converterType = typeof(Converter<>).MakeGenericType(type);
            var types = new[] { typeof(Allocator).MakeByRefType(), type };
            var method = !isAuto
                ? converterType.GetMethod(nameof(IConverter.Encode), types)
                : converterType.GetMethod(nameof(IConverter.EncodeAuto), types);
            Debug.Assert(method != null);
            return method;
        }

        internal static MethodInfo GetDecodeMethodInfo(Type type, bool isAuto)
        {
            var converterType = typeof(Converter<>).MakeGenericType(type);
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var method = !isAuto
                ? converterType.GetMethod(nameof(IConverter.Decode), types)
                : converterType.GetMethod(nameof(IConverter.DecodeAuto), types);
            Debug.Assert(method != null);
            return method;
        }

        internal static bool CanCreateInstance(Type type, MetaList metadata, ConstructorInfo constructor)
        {
            Debug.Assert(metadata.Any());
            Debug.Assert(type.IsAbstract || type.IsInterface ? constructor == null : true);
            if (type.IsAbstract || type.IsInterface)
                return false;
            if (constructor != null)
                return true;
            return (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null) && metadata.All(x => x.Property.GetSetMethod() != null);
        }

        internal static Delegate GetDecodeDelegateUseProperties(Type delegateType, ItemInitializer initializer, MetaList metadata, Type type)
        {
            var item = Expression.Variable(type, "item");
            var expressions = new List<Expression> { Expression.Assign(item, Expression.New(type)) };
            var targets = metadata.Select(x => Expression.Property(item, x.Property)).ToList();
            var (parameter, values) = initializer.Invoke();
            for (var i = 0; i < metadata.Count; i++)
                expressions.Add(Expression.Assign(targets[i], values[i]));
            expressions.Add(item);
            var lambda = Expression.Lambda(delegateType, Expression.Block(new[] { item }, expressions), parameter);
            return lambda.Compile();
        }

        internal static Delegate GetDecodeDelegateUseConstructor(Type delegateType, ItemInitializer initializer, MetaList metadata, ItemIndexes indexes, ConstructorInfo constructor)
        {
            var expressions = new List<Expression>();
            var variables = metadata.Select((x, i) => Expression.Variable(x.Property.PropertyType, $"{i}")).ToList();
            var (parameter, values) = initializer.Invoke();
            for (var i = 0; i < metadata.Count; i++)
                expressions.Add(Expression.Assign(variables[i], values[i]));
            expressions.Add(Expression.New(constructor, indexes.Select(x => variables[x]).ToList()));
            var lambda = Expression.Lambda(delegateType, Expression.Block(variables, expressions), parameter);
            return lambda.Compile();
        }
    }
}
