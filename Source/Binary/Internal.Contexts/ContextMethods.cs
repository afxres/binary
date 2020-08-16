﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethods
    {
        internal static int GetItemLength(IReadOnlyCollection<IConverter> values)
        {
            Debug.Assert(values.Any() && values.All(x => x != null && x.Length >= 0));
            var source = values.Select(x => x.Length).ToList();
            return source.All(x => x > 0) ? source.Sum() : 0;
        }

        internal static bool CanCreateInstance(Type type, IReadOnlyList<PropertyInfo> properties, ConstructorInfo constructor)
        {
            Debug.Assert(properties.Any());
            Debug.Assert(constructor is null || (!type.IsAbstract && !type.IsInterface));
            if (type.IsAbstract || type.IsInterface)
                return false;
            if (constructor != null)
                return true;
            return (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null) && properties.All(x => x.GetSetMethod() != null);
        }

        internal static Delegate GetDecodeDelegateUseMembers(Type delegateType, Type parameterType, Func<ParameterExpression, IReadOnlyList<Expression>> initializer, IReadOnlyList<Func<Expression, Expression>> members)
        {
            var type = delegateType.GetGenericArguments().Single();
            var data = Expression.Parameter(parameterType, "parameter");
            var item = Expression.Variable(type, "item");
            var targets = members.Select(x => x.Invoke(item)).ToList();
            var sources = initializer.Invoke(data);
            Debug.Assert(sources.Count == members.Count);
            Debug.Assert(sources.Count == targets.Count);
            var expressions = new List<Expression> { Expression.Assign(item, Expression.New(type)) };
            expressions.AddRange(sources.Select((x, i) => Expression.Assign(targets[i], x)));
            expressions.Add(item);
            var lambda = Expression.Lambda(delegateType, Expression.Block(new[] { item }, expressions), data);
            return lambda.Compile();
        }

        internal static Delegate GetDecodeDelegateUseConstructor(Type delegateType, Type parameterType, Func<ParameterExpression, IReadOnlyList<Expression>> initializer, IReadOnlyList<int> indexes, ConstructorInfo constructor)
        {
            var data = Expression.Parameter(parameterType, "parameter");
            var sources = initializer.Invoke(data);
            var targets = sources.Select((x, i) => Expression.Variable(x.Type, $"{i}")).ToList();
            Debug.Assert(sources.Count == indexes.Count);
            var expressions = new List<Expression>();
            expressions.AddRange(sources.Select((x, i) => Expression.Assign(targets[i], x)));
            expressions.Add(Expression.New(constructor, indexes.Select(x => targets[x]).ToList()));
            var lambda = Expression.Lambda(delegateType, Expression.Block(targets, expressions), data);
            return lambda.Compile();
        }

        internal static IConverter EnsureConverter(IConverter converter, Type type)
        {
            Debug.Assert(converter != null);
            var expectedType = typeof(Converter<>).MakeGenericType(type);
            var instanceType = converter.GetType();
            if (!expectedType.IsAssignableFrom(instanceType))
                throw new ArgumentException($"Can not convert '{instanceType}' to '{expectedType}'");
            return converter;
        }

        internal static IConverter EnsureConverter(IConverter converter, Type type, Type creatorType)
        {
            var expectedType = typeof(Converter<>).MakeGenericType(type);
            if (converter is null)
                throw new ArgumentException($"Can not convert 'null' to '{expectedType}', converter creator type: {creatorType}");
            var instanceType = converter.GetType();
            if (!expectedType.IsAssignableFrom(instanceType))
                throw new ArgumentException($"Can not convert '{instanceType}' to '{expectedType}', converter creator type: {creatorType}");
            return converter;
        }
    }
}
