using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
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
            var converterDefinition = builderDefinition == typeof(IDictionaryBuilder<,,>)
                ? typeof(DictionaryAdaptedConverter<,,>)
                : typeof(EnumerableAdaptedConverter<,>);
            var itemConverter = context.GetConverter(itemType);
            var typeArguments = new[] { type }.Concat(interfaceArguments).ToArray();
            var builderType = builderDefinition.MakeGenericType(typeArguments);
            var builder = Activator.CreateInstance(builderType);
            var converterType = converterDefinition.MakeGenericType(typeArguments);
            var converterArguments = new object[] { itemConverter, builder };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
