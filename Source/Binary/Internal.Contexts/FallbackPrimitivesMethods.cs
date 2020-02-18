using System;
using System.Collections.Generic;
using System.Linq;
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

        internal static Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            if (!type.IsGenericType || !Types.Contains(type.GetGenericTypeDefinition()))
                return null;
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;
            var typeArguments = type.GetGenericArguments();
            var metadata = Names.Take(typeArguments.Length)
                .Select(x => type.IsValueType ? (MemberInfo)type.GetField(x, Flags) : type.GetProperty(x, Flags))
                .Select((x, i) => (x, context.GetConverter(typeArguments[i])))
                .ToList();
            var constructor = type.IsValueType ? null : type.GetConstructor(typeArguments);
            var indexes = Enumerable.Range(0, typeArguments.Length).ToList();
            var converter = ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, indexes, metadata);
            return converter;
        }
    }
}
