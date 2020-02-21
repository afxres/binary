﻿using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.TryGetGenericArguments(typeof(KeyValuePair<,>), out var arguments))
                return null;
            var itemConverters = arguments.Select(context.GetConverter).ToList();
            var itemLength = ContextMethods.GetItemLength(type, itemConverters);
            var converterArguments = new List<object>(itemConverters) { itemLength }.ToArray();
            var converterType = typeof(KeyValuePairConverter<,>).MakeGenericType(arguments);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
