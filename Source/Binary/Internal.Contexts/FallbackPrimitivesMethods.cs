using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackPrimitivesMethods
    {
        private static readonly IReadOnlyCollection<Type> collection = new[]
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

        private static readonly IReadOnlyCollection<string> names = new List<string>(Enumerable.Range(0, 7).Select(x => $"Item{x + 1}")) { "Rest" };

        internal static Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            if (!type.IsGenericType || !collection.Contains(type.GetGenericTypeDefinition()))
                return null;
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;
            var typeArguments = type.GetGenericArguments();
            var query = names.Take(typeArguments.Length);
            var members = type.IsValueType
                ? query.Select(x => (MemberInfo)type.GetField(x, Flags)).ToList()
                : query.Select(x => (MemberInfo)type.GetProperty(x, Flags)).ToList();
            var constructor = type.IsValueType ? null : type.GetConstructor(typeArguments);
            var indexes = Enumerable.Range(0, typeArguments.Length).ToList();
            var metadata = members.Select((x, i) => (x, context.GetConverter(typeArguments[i]))).ToList();
            var converter = ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, indexes, metadata);
            return converter;
        }
    }
}
