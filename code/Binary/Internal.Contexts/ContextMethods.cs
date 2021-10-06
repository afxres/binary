namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

internal delegate Delegate ContextObjectConstructor(Type delegateType, ContextObjectInitializer initializer);

internal delegate Expression ContextMemberInitializer(Expression expression);

internal delegate ImmutableArray<Expression> ContextObjectInitializer(ImmutableArray<ParameterExpression> parameters);

internal static class ContextMethods
{
    internal static int GetItemLength(ImmutableArray<IConverter> values)
    {
        Debug.Assert(values.Any());
        var source = values.Select(x => x.Length).ToList();
        return source.All(x => x > 0) ? source.Sum() : 0;
    }

    internal static ImmutableArray<ContextMemberInitializer> GetMemberInitializers(ImmutableArray<PropertyInfo> properties)
    {
        return properties.Select(x => new ContextMemberInitializer(e => Expression.Property(e, x))).ToImmutableArray();
    }

    internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        Debug.Assert(parameters.Any());
        var objectIndexes = Enumerable.Range(0, parameters.Length).ToImmutableArray();
        return GetDecodeDelegate(delegateType, initializer, constructor, objectIndexes, ImmutableArray.Create<ContextMemberInitializer>(), ImmutableArray.Create<int>());
    }

    internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ImmutableArray<ContextMemberInitializer> members)
    {
        Debug.Assert(members.Any());
        var memberIndexes = Enumerable.Range(0, members.Length).ToImmutableArray();
        return GetDecodeDelegate(delegateType, initializer, null, ImmutableArray.Create<int>(), members, memberIndexes);
    }

    internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ConstructorInfo? constructor, ImmutableArray<int> objectIndexes, ImmutableArray<ContextMemberInitializer> members, ImmutableArray<int> memberIndexes)
    {
        var delegateInvoke = CommonModule.GetMethod(delegateType, "Invoke", BindingFlags.Public | BindingFlags.Instance);
        Debug.Assert(delegateInvoke.GetParameters().Length is 1 or 2);
        var type = delegateInvoke.ReturnType;
        var parameterTypes = delegateInvoke.GetParameters().Select(x => x.ParameterType).ToList();
        var parameters = parameterTypes.Select((x, i) => Expression.Parameter(x, $"arg{i}")).ToImmutableArray();
        var item = Expression.Variable(type, "item");

        var sources = initializer.Invoke(parameters);
        var targets = sources.Select((x, i) => Expression.Variable(x.Type, $"var{i}")).ToList();
        Debug.Assert(sources.Length == objectIndexes.Length + memberIndexes.Length);
        Debug.Assert(members.Length == memberIndexes.Length);
        Debug.Assert(Enumerable.Range(0, sources.Length).Except(objectIndexes).Except(memberIndexes).Any() is false);

        var expressions = new List<Expression>();
        expressions.AddRange(sources.Select((x, i) => Expression.Assign(targets[i], x)));
        expressions.Add(Expression.Assign(item, constructor is null ? Expression.New(type) : Expression.New(constructor, objectIndexes.Select(x => targets[x]).ToList())));
        expressions.AddRange(memberIndexes.Select((x, i) => Expression.Assign(members[i].Invoke(item), targets[x])));
        expressions.Add(item);
        var lambda = Expression.Lambda(delegateType, Expression.Block(ImmutableArray.Create(item).AddRange(targets), expressions), parameters);
        return lambda.Compile();
    }
}
