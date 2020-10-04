using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal delegate Expression ContextMemberInitializer(Expression expression);

    internal delegate Delegate ContextObjectConstructor(Type delegateType, ContextObjectInitializer initializer);

    internal delegate IReadOnlyList<Expression> ContextObjectInitializer(ParameterExpression parameter);

    internal static class ContextMethods
    {
        internal static int GetItemLength(IReadOnlyCollection<IConverter> values)
        {
            Debug.Assert(values.Any());
            var source = values.Select(x => x.Length).ToList();
            return source.All(x => x > 0) ? source.Sum() : 0;
        }

        internal static IReadOnlyList<ContextMemberInitializer> GetMemberInitializers(IReadOnlyList<PropertyInfo> properties)
        {
            return properties.Select(x => new ContextMemberInitializer(e => Expression.Property(e, x))).ToList();
        }

        internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ConstructorInfo constructor)
        {
            var parameters = (IReadOnlyList<ParameterInfo>)constructor.GetParameters();
            Debug.Assert(parameters.Any());
            var objectIndexes = Enumerable.Range(0, parameters.Count).ToList();
            return GetDecodeDelegate(delegateType, initializer, constructor, objectIndexes, Array.Empty<ContextMemberInitializer>(), Array.Empty<int>());
        }

        internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, IReadOnlyList<ContextMemberInitializer> members)
        {
            Debug.Assert(members.Any());
            var memberIndexes = Enumerable.Range(0, members.Count).ToList();
            return GetDecodeDelegate(delegateType, initializer, null, Array.Empty<int>(), members, memberIndexes);
        }

        internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ConstructorInfo constructor, IReadOnlyList<int> objectIndexes, IReadOnlyList<ContextMemberInitializer> members, IReadOnlyList<int> memberIndexes)
        {
            var delegateInvoke = delegateType.GetMethod("Invoke");
            Debug.Assert(delegateInvoke.GetParameters().Length is 1);
            var type = delegateInvoke.ReturnType;
            var parameterType = delegateInvoke.GetParameters().Single().ParameterType;
            var data = Expression.Parameter(parameterType, "parameter");
            var item = Expression.Variable(type, "item");

            var sources = initializer.Invoke(data);
            var targets = sources.Select((x, i) => Expression.Variable(x.Type, $"{i}")).ToList();
            Debug.Assert(sources.Count == objectIndexes.Count + memberIndexes.Count);
            Debug.Assert(members.Count == memberIndexes.Count);
            Debug.Assert(Enumerable.Range(0, sources.Count).Except(objectIndexes).Except(memberIndexes).Any() is false);

            var expressions = new List<Expression>();
            expressions.AddRange(sources.Select((x, i) => Expression.Assign(targets[i], x)));
            expressions.Add(Expression.Assign(item, constructor is null ? Expression.New(type) : Expression.New(constructor, objectIndexes.Select(x => targets[x]).ToList())));
            expressions.AddRange(memberIndexes.Select((x, i) => Expression.Assign(members[i].Invoke(item), targets[x])));
            expressions.Add(item);
            var lambda = Expression.Lambda(delegateType, Expression.Block(CommonHelper.Concat(item, targets), expressions), data);
            return lambda.Compile();
        }

        internal static IConverter EnsureConverter(IConverter converter, Type type)
        {
            Debug.Assert(converter is not null);
            var expectedType = typeof(Converter<>).MakeGenericType(type);
            var instanceType = converter.GetType();
            if (expectedType.IsAssignableFrom(instanceType) is false)
                throw new ArgumentException($"Can not convert '{instanceType}' to '{expectedType}'");
            return converter;
        }

        internal static IConverter EnsureConverter(IConverter converter, Type type, Type creatorType)
        {
            var expectedType = typeof(Converter<>).MakeGenericType(type);
            if (converter is null)
                throw new ArgumentException($"Can not convert 'null' to '{expectedType}', converter creator type: {creatorType}");
            var instanceType = converter.GetType();
            if (expectedType.IsAssignableFrom(instanceType) is false)
                throw new ArgumentException($"Can not convert '{instanceType}' to '{expectedType}', converter creator type: {creatorType}");
            return converter;
        }
    }
}
