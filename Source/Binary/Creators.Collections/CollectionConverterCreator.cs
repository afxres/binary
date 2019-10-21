using Mikodev.Binary.CollectionModels.Implementations;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class CollectionConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type.IsValueType || !type.TryGetInterfaceArguments(typeof(IEnumerable<>), out var arguments))
                return null;
            var interfaceArguments = default(Type[]);
            Type Test(Type builderDefinition, Type assignable, params Type[] interfaces) =>
                interfaces.Any(x => type.TryGetInterfaceArguments(x, out interfaceArguments)) && type.IsAssignableFrom(assignable.MakeGenericType(interfaceArguments))
                    ? builderDefinition
                    : null;
            var builderDefinition =
                Test(typeof(IDictionaryBuilder<,,>), typeof(Dictionary<,>), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>)) ??
                Test(typeof(ISetBuilder<,>), typeof(HashSet<>), typeof(ISet<>)) ??
                Test(typeof(IEnumerableBuilder<,>), typeof(ArraySegment<>), typeof(IEnumerable<>));
            if (builderDefinition == null)
                return null;

            var itemType = arguments.Single();
            if (builderDefinition == null)
                return null;
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
