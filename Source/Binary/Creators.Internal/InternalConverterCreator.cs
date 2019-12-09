using System;
using System.Collections.Generic;
using System.Net;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class InternalConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Converter> dictionary = new Dictionary<Type, Converter>
        {
            [typeof(Uri)] = new UriConverter(),
            [typeof(string)] = new StringConverter(),
            [typeof(decimal)] = new DecimalConverter(),
            [typeof(DateTime)] = new DateTimeConverter(),
            [typeof(TimeSpan)] = new TimeSpanConverter(),
            [typeof(IPAddress)] = new IPAddressConverter(),
            [typeof(IPEndPoint)] = new IPEndPointConverter(),
            [typeof(DateTimeOffset)] = new DateTimeOffsetConverter(),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!dictionary.TryGetValue(type, out var result))
                return null;
            return result;
        }
    }
}
