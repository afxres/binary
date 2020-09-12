using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackPrimitivesMethods
    {
        private static readonly IReadOnlyCollection<string> Names = new[]
        {
            "Item1",
            "Item2",
            "Item3",
            "Item4",
            "Item5",
            "Item6",
            "Item7",
            "Rest",
        };

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

        private static IReadOnlyList<IConverter> GetValueTupleParameters(IGeneratorContext context, Type type, out IReadOnlyList<Type> types, out IReadOnlyList<Func<Expression, Expression>> members)
        {
            static void Fields(Type type, Action<FieldInfo> action)
            {
                var names = Names.Take(type.GetGenericArguments().Length);
                var fields = names.Select(x => type.GetField(x, BindingFlags.Instance | BindingFlags.Public)).ToList();
                fields.ForEach(action);
            }

            static void Expand(IGeneratorContext context, List<(Type, Func<Expression, Expression>, IConverter)> result, FieldInfo field, Func<Expression, Expression> parent)
            {
                var type = field.FieldType;
                var func = new Func<Expression, Expression>(x => Expression.Field(parent.Invoke(x), field));
                var converter = context.GetConverter(type);
                if (type.IsValueType && IsTupleOrValueTuple(type) && converter.GetType() == typeof(TupleObjectConverter<>).MakeGenericType(type))
                    Fields(type, x => Expand(context, result, x, func));
                else
                    result.Add((type, x => func.Invoke(x), converter));
            }

            var result = new List<(Type Type, Func<Expression, Expression> Member, IConverter Converter)>();
            Fields(type, x => Expand(context, result, x, v => v));
            types = result.Select(x => x.Type).ToList();
            members = result.Select(x => x.Member).ToList();
            return result.Select(x => x.Converter).ToList();
        }

        private static IReadOnlyList<IConverter> GetTupleParameters(IGeneratorContext context, Type type, out IReadOnlyList<PropertyInfo> properties)
        {
            var names = Names.Take(type.GetGenericArguments().Length);
            properties = names.Select(x => type.GetProperty(x, BindingFlags.Instance | BindingFlags.Public)).ToList();
            return properties.Select(x => context.GetConverter(x.PropertyType)).ToList();
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (IsTupleOrValueTuple(type) is false)
                return null;
            var types = default(IReadOnlyList<Type>);
            var members = default(IReadOnlyList<Func<Expression, Expression>>);
            var properties = default(IReadOnlyList<PropertyInfo>);
            var converters = !type.IsValueType
                ? GetTupleParameters(context, type, out properties)
                : GetValueTupleParameters(context, type, out types, out members);
            var typeArguments = type.GetGenericArguments();
            var constructor = type.IsValueType ? null : type.GetConstructor(typeArguments);
            var indexes = Enumerable.Range(0, typeArguments.Length).ToList();
            var converter = ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, indexes, converters, properties, types, members);
            return converter;
        }
    }
}
