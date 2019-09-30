using Mikodev.Binary.Converters.Runtime.Collections;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed partial class GeneratorContext
    {
        private static readonly IReadOnlyList<Type> reverseCollectionTypeDefinitions = new[] { typeof(Stack<>), typeof(ConcurrentStack<>) };

        private Converter GetConverterAsCollection(Type type, Type itemType)
        {
            var typeArguments = new[] { type, itemType };
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var constructor = ToValueAsEnumerable(type, enumerableType, typeof(ToCollection<,>).MakeGenericType(typeArguments));
            if (constructor == null && itemType.TryGetGenericArguments(typeof(KeyValuePair<,>), out var types))
                return GetConverterAsDictionary(type, types);
            var reverse = type.IsGenericType && reverseCollectionTypeDefinitions.Contains(type.GetGenericTypeDefinition());
            var converterType = typeof(GenericCollectionConverter<,>).MakeGenericType(typeArguments);
            var converter = Activator.CreateInstance(converterType, GetConverter(itemType), constructor, reverse);
            return (Converter)converter;
        }

        private Converter GetConverterAsDictionary(Type type, Type[] types)
        {
            Debug.Assert(types.Length == 2);
            var typeArguments = new[] { type, types[0], types[1] };
            var dictionaryType = typeof(IDictionary<,>).MakeGenericType(types);
            var constructor = ToValueAsEnumerable(type, dictionaryType, typeof(ToDictionary<,,>).MakeGenericType(typeArguments));
            var itemType = typeof(KeyValuePair<,>).MakeGenericType(types);
            var itemConverter = GetConverter(itemType);
            var arguments = new object[] { itemConverter, constructor };
            var converterType = typeof(GenericDictionaryConverter<,,>).MakeGenericType(typeArguments);
            var converter = Activator.CreateInstance(converterType, arguments);
            return (Converter)converter;
        }

        private Delegate ToValueAsEnumerable(Type type, Type enumerableType, Type delegateType)
        {
            if (type.IsAbstract != false || type.IsInterface != false)
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
