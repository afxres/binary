using Mikodev.Binary.Internal.Adapters;
using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackCollectionMethods
    {
        private static readonly IReadOnlyCollection<Type> reverseTypes = new[] { typeof(Stack<>), typeof(ConcurrentStack<>) };

        internal static Converter GetConverter(IGeneratorContext context, Type type, Type itemType)
        {
            return itemType.TryGetGenericArguments(typeof(KeyValuePair<,>), out var types)
                ? GetConverterAsDictionary(context, type, itemType, types)
                : GetConverterAsCollection(context, type, itemType);
        }

        private static Converter GetConverterAsCollection(IGeneratorContext context, Type type, Type itemType)
        {
            var itemConverter = context.GetConverter(itemType);
            var typeArguments = new[] { type, itemType };

            object MakeBuilder()
            {
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
                var constructor = GetDecodeDelegateAsEnumerable(type, enumerableType, typeof(ToCollection<,>).MakeGenericType(typeArguments));
                var reverse = type.IsGenericType && reverseTypes.Contains(type.GetGenericTypeDefinition());
                var builderType = typeof(DelegateCollectionBuilder<,>).MakeGenericType(typeArguments);
                var builderArguments = new object[] { constructor, reverse };
                return Activator.CreateInstance(builderType, builderArguments);
            }

            var builder = MakeBuilder();
            var converterType = typeof(EnumerableAdaptedConverter<,>).MakeGenericType(typeArguments);
            var converterArguments = new object[] { itemConverter, builder };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }

        private static Converter GetConverterAsDictionary(IGeneratorContext context, Type type, Type itemType, Type[] types)
        {
            var typeArguments = new[] { type, types[0], types[1] };
            var itemConverter = context.GetConverter(itemType);

            object MakeBuilder()
            {
                var dictionaryType = typeof(IDictionary<,>).MakeGenericType(types);
                var constructor = GetDecodeDelegateAsEnumerable(type, dictionaryType, typeof(ToDictionary<,,>).MakeGenericType(typeArguments));
                var builderType = typeof(DelegateDictionaryBuilder<,,>).MakeGenericType(typeArguments);
                var builderArguments = new object[] { constructor };
                return Activator.CreateInstance(builderType, builderArguments);
            }

            var builder = MakeBuilder();
            var converterType = typeof(DictionaryAdaptedConverter<,,>).MakeGenericType(typeArguments);
            var converterArguments = new object[] { itemConverter, builder };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }

        private static Delegate GetDecodeDelegateAsEnumerable(Type type, Type enumerableType, Type delegateType)
        {
            if (type.IsAbstract || type.IsInterface)
                return null;
            var constructor = type.GetConstructor(new[] { enumerableType });
            if (constructor == null)
                return null;
            var enumerable = Expression.Parameter(enumerableType, "enumerable");
            var result = Expression.New(constructor, enumerable);
            var lambda = Expression.Lambda(delegateType, result, enumerable);
            return lambda.Compile();
        }
    }
}
