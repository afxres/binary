using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethods
    {
        internal static int GetItemLength(IReadOnlyCollection<Converter> values)
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

        internal static Delegate GetDecodeDelegateUseMembers(Type delegateType, Func<(ParameterExpression, IReadOnlyList<Expression>)> initializer, IReadOnlyList<Func<Expression, Expression>> members, Type type)
        {
            var item = Expression.Variable(type, "item");
            var targets = members.Select(x => x.Invoke(item)).ToList();
            var (parameter, values) = initializer.Invoke();
            Debug.Assert(values.Count == members.Count);
            Debug.Assert(values.Count == targets.Count);
            var expressions = new List<Expression> { Expression.Assign(item, Expression.New(type)) };
            expressions.AddRange(values.Select((x, i) => Expression.Assign(targets[i], x)));
            expressions.Add(item);
            var lambda = Expression.Lambda(delegateType, Expression.Block(new[] { item }, expressions), parameter);
            return lambda.Compile();
        }

        internal static Delegate GetDecodeDelegateUseConstructor(Type delegateType, Func<(ParameterExpression, IReadOnlyList<Expression>)> initializer, IReadOnlyList<int> indexes, ConstructorInfo constructor)
        {
            var (parameter, values) = initializer.Invoke();
            var variables = values.Select((x, i) => Expression.Variable(x.Type, $"{i}")).ToList();
            Debug.Assert(values.Count == indexes.Count);
            var expressions = new List<Expression>();
            expressions.AddRange(values.Select((x, i) => Expression.Assign(variables[i], x)));
            expressions.Add(Expression.New(constructor, indexes.Select(x => variables[x]).ToList()));
            var lambda = Expression.Lambda(delegateType, Expression.Block(variables, expressions), parameter);
            return lambda.Compile();
        }
    }
}
