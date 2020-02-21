using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using ItemInitializer = System.Func<(System.Linq.Expressions.ParameterExpression, System.Linq.Expressions.Expression[])>;
using MemberList = System.Collections.Generic.IReadOnlyList<(System.Reflection.MemberInfo Member, Mikodev.Binary.Converter Converter)>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethods
    {
        internal static int GetItemLength(Type type, IReadOnlyCollection<Converter> values)
        {
            Debug.Assert(values.Any() && values.All(x => x != null && x.Length >= 0));
            var length = values.All(x => x.Length > 0) ? values.Sum(x => (long)x.Length) : 0;
            if ((ulong)length > int.MaxValue)
                throw new ArgumentException($"Converter length overflow, type: {type}");
            return (int)length;
        }

        internal static bool CanCreateInstance(Type type, IReadOnlyList<MemberInfo> members, ConstructorInfo constructor)
        {
            Debug.Assert(members.Any());
            Debug.Assert(type.IsAbstract || type.IsInterface ? constructor is null : true);
            if (type.IsAbstract || type.IsInterface)
                return false;
            if (constructor != null)
                return true;
            var predicate = new Func<MemberInfo, bool>(x =>
                (x is PropertyInfo property && property.GetSetMethod() != null) ||
                (x is FieldInfo field && !field.IsLiteral && !field.IsInitOnly));
            return (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null) && members.All(predicate);
        }

        internal static Delegate GetDecodeDelegateUseMembers(Type delegateType, ItemInitializer initializer, MemberList metadata, Type type)
        {
            var item = Expression.Variable(type, "item");
            var expressions = new List<Expression> { Expression.Assign(item, Expression.New(type)) };
            var targets = metadata
                .Select(x => x.Member)
                .Select(x => x is FieldInfo field ? Expression.Field(item, field) : Expression.Property(item, (PropertyInfo)x))
                .ToList();
            var (parameter, values) = initializer.Invoke();
            for (var i = 0; i < metadata.Count; i++)
                expressions.Add(Expression.Assign(targets[i], values[i]));
            expressions.Add(item);
            var lambda = Expression.Lambda(delegateType, Expression.Block(new[] { item }, expressions), parameter);
            return lambda.Compile();
        }

        internal static Delegate GetDecodeDelegateUseConstructor(Type delegateType, ItemInitializer initializer, IReadOnlyList<Converter> converters, ItemIndexes indexes, ConstructorInfo constructor)
        {
            var expressions = new List<Expression>();
            var variables = converters.Select((x, i) => Expression.Variable(x.ItemType, $"{i}")).ToList();
            var (parameter, values) = initializer.Invoke();
            for (var i = 0; i < converters.Count; i++)
                expressions.Add(Expression.Assign(variables[i], values[i]));
            expressions.Add(Expression.New(constructor, indexes.Select(x => variables[x]).ToList()));
            var lambda = Expression.Lambda(delegateType, Expression.Block(variables, expressions), parameter);
            return lambda.Compile();
        }
    }
}
