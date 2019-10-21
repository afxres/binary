using Mikodev.Binary.Converters.Runtime.Collections;
using Mikodev.Binary.Internal.Delegates;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfCollections
    {
        private static readonly IReadOnlyList<Type> reverseTypes = new[] { typeof(Stack<>), typeof(ConcurrentStack<>) };

        internal static Converter GetConverterAsCollectionOrDictionary(IGeneratorContext context, Type type, Type itemType)
        {
            return itemType.TryGetGenericArguments(typeof(KeyValuePair<,>), out var types)
                ? GetConverterAsDictionary(context, type, itemType, types)
                : GetConverterAsCollection(context, type, itemType);
        }

        private static Converter GetConverterAsCollection(IGeneratorContext context, Type type, Type itemType)
        {
            var typeArguments = new[] { type, itemType };
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var constructor = GetDecodeDelegateAsEnumerable(type, enumerableType, typeof(ToCollection<,>).MakeGenericType(typeArguments));
            var reverse = type.IsGenericType && reverseTypes.Contains(type.GetGenericTypeDefinition());
            var converterType = typeof(GenericCollectionConverter<,>).MakeGenericType(typeArguments);
            var converterArguments = new object[] { constructor, context.GetConverter(itemType), reverse };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }

        private static Converter GetConverterAsDictionary(IGeneratorContext context, Type type, Type itemType, Type[] types)
        {
            var typeArguments = new[] { type, types[0], types[1] };
            var dictionaryType = typeof(IDictionary<,>).MakeGenericType(types);
            var constructor = GetDecodeDelegateAsEnumerable(type, dictionaryType, typeof(ToDictionary<,,>).MakeGenericType(typeArguments));
            var itemConverter = context.GetConverter(itemType);
            var converterType = typeof(GenericDictionaryConverter<,,>).MakeGenericType(typeArguments);
            var converterArguments = new object[] { constructor, itemConverter, };
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
