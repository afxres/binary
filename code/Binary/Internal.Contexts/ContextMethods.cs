namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

internal delegate Delegate ContextObjectConstructor(Type delegateType, ContextObjectInitializer initializer);

internal delegate Expression ContextMemberInitializer(Expression expression);

internal delegate ImmutableArray<Expression> ContextObjectInitializer(ImmutableArray<ParameterExpression> parameters);

internal static class ContextMethods
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        Debug.Assert(parameters.Length is not 0);
        var objectIndexes = Enumerable.Range(0, parameters.Length).ToImmutableArray();
        return GetDecodeDelegate(delegateType, initializer, constructor, objectIndexes, [], []);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ImmutableArray<ContextMemberInitializer> members)
    {
        Debug.Assert(members.Any());
        var memberIndexes = Enumerable.Range(0, members.Length).ToImmutableArray();
        return GetDecodeDelegate(delegateType, initializer, null, [], members, memberIndexes);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static Delegate GetDecodeDelegate(Type delegateType, ContextObjectInitializer initializer, ConstructorInfo? constructor, ImmutableArray<int> objectIndexes, ImmutableArray<ContextMemberInitializer> members, ImmutableArray<int> memberIndexes)
    {
        var delegateInvoke = CommonModule.GetPublicInstanceMethod(delegateType, "Invoke");
        Debug.Assert(delegateInvoke.GetParameters().Length is 1);
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
