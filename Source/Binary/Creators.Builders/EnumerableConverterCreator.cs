using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class EnumerableConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type.IsValueType || !type.TryGetInterfaceArguments(typeof(IEnumerable<>), out var arguments))
                return null;
            var interfaceArguments = default(Type[]);
            Type Detect(Type definition, Type assignable, params Type[] interfaces) =>
                interfaces.Any(x => type.TryGetInterfaceArguments(x, out interfaceArguments)) && type.IsAssignableFrom(assignable.MakeGenericType(interfaceArguments))
                    ? definition
                    : null;
            var builderDefinition =
                Detect(typeof(IDictionaryBuilder<,,>), typeof(Dictionary<,>), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>)) ??
                Detect(typeof(ISetBuilder<,>), typeof(HashSet<>), typeof(ISet<>)) ??
                Detect(typeof(IEnumerableBuilder<,>), typeof(ArraySegment<>), typeof(IEnumerable<>));
            if (builderDefinition is null)
                return null;

            var itemType = arguments.Single();
            return builderDefinition == typeof(IDictionaryBuilder<,,>)
                ? GetDictionaryConverter(context, type, itemType, interfaceArguments)
                : GetEnumerableConverter(context, type, itemType, builderDefinition);
        }

        private static Converter GetEnumerableConverter(IGeneratorContext context, Type type, Type itemType, Type builderDefinition)
        {
            var itemConverter = context.GetConverter(itemType);
            var converterDefinition = typeof(EnumerableAdaptedConverter<,>);
            var typeArguments = new[] { type, itemType };
            var builderType = builderDefinition.MakeGenericType(typeArguments);
            var builder = Activator.CreateInstance(builderType);
            var converterArguments = new object[] { builder, itemConverter };
            var converterType = converterDefinition.MakeGenericType(typeArguments);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }

        private static Converter GetDictionaryConverter(IGeneratorContext context, Type type, Type itemType, Type[] interfaceArguments)
        {
            var itemConverters = interfaceArguments.Select(context.GetConverter).ToArray();
            var converterDefinition = typeof(DictionaryAdaptedConverter<,,>);
            var typeArguments = new[] { type, interfaceArguments[0], interfaceArguments[1] };
            var builderType = typeof(IDictionaryBuilder<,,>).MakeGenericType(typeArguments);
            var builder = Activator.CreateInstance(builderType);
            var itemLength = ContextMethods.GetItemLength(itemType, itemConverters);
            var converterArguments = new object[] { builder, itemConverters[0], itemConverters[1], itemLength };
            var converterType = converterDefinition.MakeGenericType(typeArguments);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
