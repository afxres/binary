using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackPrimitivesMethods
    {
        private static readonly IReadOnlyCollection<string> Names = new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Rest" };

        private static readonly IReadOnlyCollection<Type> Types = new[]
        {
            typeof(Tuple<>),
            typeof(Tuple<,>),
            typeof(Tuple<,,>),
            typeof(Tuple<,,,>),
            typeof(Tuple<,,,,>),
            typeof(Tuple<,,,,,>),
            typeof(Tuple<,,,,,,>),
            typeof(Tuple<,,,,,,,>),
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>),
        };

        private static bool IsTupleOrValueTuple(Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            return type.IsGenericType && Types.Contains(type.GetGenericTypeDefinition());
        }

        private static (IReadOnlyList<Func<Expression, Expression>>, IReadOnlyList<IConverter>, IReadOnlyList<PropertyInfo>) GetValueTupleParameters(IGeneratorContext context, Type type)
        {
            var members = new List<Func<Expression, Expression>>();
            var converters = new List<IConverter>();

            static IReadOnlyList<FieldInfo> GetValueTupleFields(Type type)
            {
                return Names.Take(type.GetGenericArguments().Length).Select(x => type.GetField(x, BindingFlags.Instance | BindingFlags.Public)).ToList();
            }

            void Insert(Func<Expression, Expression> func, IConverter converter)
            {
                members.Add(func);
                converters.Add(converter);
                Debug.Assert(converters.Count == members.Count);
            }

            void Expand(Func<Expression, Expression> parent, FieldInfo field)
            {
                var type = field.FieldType;
                var func = new Func<Expression, Expression>(x => Expression.Field(parent.Invoke(x), field));
                var converter = context.GetConverter(type);
                if (type.IsValueType && IsTupleOrValueTuple(type) && converter.GetType() == typeof(TupleObjectConverter<>).MakeGenericType(type))
                    foreach (var i in GetValueTupleFields(type))
                        Expand(func, i);
                else
                    Insert(x => func.Invoke(x), converter);
            }

            foreach (var field in GetValueTupleFields(type))
                Expand(x => x, field);
            return (members, converters, null);
        }

        private static (IReadOnlyList<Func<Expression, Expression>>, IReadOnlyList<IConverter>, IReadOnlyList<PropertyInfo>) GetTupleParameters(IGeneratorContext context, Type type)
        {
            var members = new List<Func<Expression, Expression>>();
            var converters = new List<IConverter>();
            var properties = Names.Take(type.GetGenericArguments().Length).Select(x => type.GetProperty(x, BindingFlags.Instance | BindingFlags.Public)).ToList();

            foreach (var property in properties)
            {
                var converter = context.GetConverter(property.PropertyType);
                members.Add(x => Expression.Property(x, property));
                converters.Add(converter);
            }
            return (members, converters, properties);
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (!IsTupleOrValueTuple(type))
                return null;
            var (members, converters, properties) = !type.IsValueType
                ? GetTupleParameters(context, type)
                : GetValueTupleParameters(context, type);
            var typeArguments = type.GetGenericArguments();
            var constructor = type.IsValueType ? null : type.GetConstructor(typeArguments);
            var indexes = Enumerable.Range(0, typeArguments.Length).ToList();
            var converter = ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, properties, converters, constructor, indexes, members);
            return converter;
        }
    }
}
