using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Mikodev.Binary.Creators.Adapters
{
    internal sealed class AdaptedConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(decimal)] = typeof(DecimalAdapter),
            [typeof(DateTime)] = typeof(DateTimeAdapter),
            [typeof(DateTimeOffset)] = typeof(DateTimeOffsetAdapter),
            [typeof(TimeSpan)] = typeof(TimeSpanAdapter),
            [typeof(IPAddress)] = typeof(IPAddressAdapter),
            [typeof(IPEndPoint)] = typeof(IPEndPointAdapter),
            [typeof(Uri)] = typeof(UriAdapter),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!dictionary.TryGetValue(type, out var adapterType))
                return null;
            var adapterConverters = adapterType.GetConstructors()
                .Single().GetParameters()
                .Select(x => x.ParameterType.GetGenericArguments().Single())
                .Select(context.GetConverter)
                .ToList();
            var adapterArguments = adapterConverters.Cast<object>().ToArray();
            var adapter = Activator.CreateInstance(adapterType, adapterArguments);
            var typeArguments = adapterType.BaseType.GetGenericArguments();
            var arguments = new object[] { adapter, context.GetConverter(typeArguments.Last()) };
            var converterType = typeof(AdaptedConverter<,>).MakeGenericType(typeArguments);
            var converter = Activator.CreateInstance(converterType, arguments);
            return (Converter)converter;
        }
    }
}
