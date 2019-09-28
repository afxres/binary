using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Components
{
    internal abstract class PatternConverterCreator : IConverterCreator
    {
        private readonly IEnumerable<Type> interfaces;

        private readonly Type assignable;

        private readonly Type definition;

        public PatternConverterCreator(IEnumerable<Type> interfaces, Type assignable, Type definition)
        {
            Debug.Assert(interfaces.Any());
            Debug.Assert(assignable != null);
            Debug.Assert(definition != null);
            this.interfaces = interfaces;
            this.assignable = assignable;
            this.definition = definition;
        }

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            var arguments = default(Type[]);
            if (!interfaces.Any(x => type.TryGetInterfaceArguments(x, out arguments)))
                return null;
            if (!type.IsAssignableFrom(assignable.MakeGenericType(arguments)))
                return null;
            var typeArguments = new[] { type }.Concat(arguments).ToArray();
            var converters = arguments.Select(context.GetConverter).Cast<object>().ToArray();
            var converterType = definition.MakeGenericType(typeArguments);
            var converter = Activator.CreateInstance(converterType, converters);
            return (Converter)converter;
        }
    }
}
