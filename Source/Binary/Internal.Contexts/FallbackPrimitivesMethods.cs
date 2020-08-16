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

        private static (IReadOnlyList<Func<Expression, Expression>>, IReadOnlyList<Type>, IReadOnlyList<IConverter>, IReadOnlyList<PropertyInfo>) GetValueTupleParameters(IGeneratorContext context, Type type)
        {
            var types = new List<Type>();
            var members = new List<Func<Expression, Expression>>();
            var converters = new List<IConverter>();

            static void Fields(Type type, Action<FieldInfo> action)
            {
                Names.Take(type.GetGenericArguments().Length).Select(x => type.GetField(x, BindingFlags.Instance | BindingFlags.Public)).ToList().ForEach(action);
            }

            void Insert(Func<Expression, Expression> func, Type type, IConverter converter)
            {
                types.Add(type);
                members.Add(func);
                converters.Add(converter);
                Debug.Assert(converters.Count == types.Count);
                Debug.Assert(converters.Count == members.Count);
            }

            void Expand(Func<Expression, Expression> parent, FieldInfo field)
            {
                var type = field.FieldType;
                var func = new Func<Expression, Expression>(x => Expression.Field(parent.Invoke(x), field));
                var converter = context.GetConverter(type);
                if (type.IsValueType && IsTupleOrValueTuple(type) && converter.GetType() == typeof(TupleObjectConverter<>).MakeGenericType(type))
                    Fields(type, x => Expand(func, x));
                else
                    Insert(x => func.Invoke(x), type, converter);
            }

            Fields(type, x => Expand(v => v, x));
            return (members, types, converters, null);
        }

        private static (IReadOnlyList<Func<Expression, Expression>>, IReadOnlyList<Type>, IReadOnlyList<IConverter>, IReadOnlyList<PropertyInfo>) GetTupleParameters(IGeneratorContext context, Type type)
        {
            var properties = Names.Take(type.GetGenericArguments().Length).Select(x => type.GetProperty(x, BindingFlags.Instance | BindingFlags.Public)).ToList();
            var converters = properties.Select(x => context.GetConverter(x.PropertyType)).ToList();
            return (null, null, converters, properties);
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (!IsTupleOrValueTuple(type))
                return null;
            var (members, types, converters, properties) = !type.IsValueType
                ? GetTupleParameters(context, type)
                : GetValueTupleParameters(context, type);
            var typeArguments = type.GetGenericArguments();
            var constructor = type.IsValueType ? null : type.GetConstructor(typeArguments);
            var indexes = Enumerable.Range(0, typeArguments.Length).ToList();
            var converter = ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, indexes, converters, properties, types, members);
            return converter;
        }
    }
}
