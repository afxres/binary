﻿using System;
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
        internal static int GetConverterLength(Type type, IEnumerable<Converter> values)
        {
            Debug.Assert(values.Any() && values.All(x => x != null && x.Length >= 0));
            var length = values.All(x => x.Length > 0) ? values.Sum(x => (long)x.Length) : 0;
            if (length < 0 || length > int.MaxValue)
                throw new ArgumentException($"Converter length overflow, type: {type}");
            return (int)length;
        }

        internal static MethodInfo GetToBytesMethodInfo(Type type, bool withMark)
        {
            var converterType = typeof(Converter<>).MakeGenericType(type);
            var types = new[] { typeof(Allocator).MakeByRefType(), type };
            var method = !withMark
                ? converterType.GetMethod(nameof(IConverter.ToBytes), types)
                : converterType.GetMethod(nameof(IConverter.ToBytesWithMark), types);
            Debug.Assert(method != null);
            return method;
        }

        internal static MethodInfo GetToValueMethodInfo(Type type, bool withMark)
        {
            var converterType = typeof(Converter<>).MakeGenericType(type);
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var method = !withMark
                ? converterType.GetMethod(nameof(IConverter.ToValue), types)
                : converterType.GetMethod(nameof(IConverter.ToValueWithMark), types);
            Debug.Assert(method != null);
            return method;
        }

        internal static bool CanCreateInstance(Type type, ConstructorInfo constructor)
        {
            if (type.IsAbstract || type.IsInterface)
                return false;
            Debug.Assert(constructor == null || constructor.DeclaringType == type);
            // create instance by constructor or default value (for value type)
            return constructor != null || type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        internal static Delegate ToValuePlanAlpha(Type delegateType, ItemInitializer initializer, MetaList metadata, Type type)
        {
            var failed = metadata.Select(x => x.Property).FirstOrDefault(x => x.GetSetMethod() == null);
            if (failed != null)
                throw new ArgumentException($"Property '{failed.Name}' does not have a public setter, type: {type}");
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

        internal static Delegate ToValuePlanBravo(Type delegateType, ItemInitializer initializer, MetaList metadata, ItemIndexes indexes, ConstructorInfo constructor)
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
