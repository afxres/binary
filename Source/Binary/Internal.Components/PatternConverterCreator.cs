using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Components
{
    internal readonly struct PatternConverterCreator
    {
        private readonly IEnumerable<Type> interfaces;

        private readonly Type assignable;

        private readonly Type definition;

        public PatternConverterCreator(IEnumerable<Type> interfaces, Type assignable, Type definition)
        {
            Debug.Assert(interfaces.Any() && interfaces.All(x => x.IsInterface));
            Debug.Assert(assignable != null);
            Debug.Assert(definition != null);
            this.interfaces = interfaces;
            this.assignable = assignable;
            this.definition = definition;
        }

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            var arguments = default(Type[]);
            if (type.IsValueType || !interfaces.Any(x => type.TryGetInterfaceArguments(x, out arguments)) || !type.IsAssignableFrom(assignable.MakeGenericType(arguments)))
                return null;
            var typeArguments = new[] { type }.Concat(arguments).ToArray();
            var converterType = definition.MakeGenericType(typeArguments);
            var constructor = converterType.GetConstructors().Single();
            var converterTypes = constructor.GetParameters()
                .Select(x => x.ParameterType)
                .Select(x => x.GetGenericArguments().Single())
                .ToArray();
            var converterArguments = converterTypes.Select(context.GetConverter).Cast<object>().ToArray();
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
