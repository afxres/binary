using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class Generator : IGenerator
    {
        private readonly ConcurrentDictionary<Type, IConverter> converters;

        private readonly IReadOnlyCollection<IConverterCreator> creators;

        public Generator(IReadOnlyDictionary<Type, IConverter> converters, IReadOnlyCollection<IConverterCreator> creators)
        {
            this.converters = new ConcurrentDictionary<Type, IConverter>(converters) { [typeof(object)] = new GeneratorObjectConverter(this) };
            this.creators = creators.ToArray();
            Debug.Assert(this.converters.All(x => x.Value != null));
            Debug.Assert(this.creators.Count == 0 || this.creators.All(x => x != null));
        }

        public IConverter GetConverter(Type type)
        {
            if (type is null)
                ThrowHelper.ThrowTypeNull();
            if (converters.TryGetValue(type, out var result))
                return result;
            var context = new GeneratorContext(converters, creators);
            return context.GetConverter(type);
        }

        public override string ToString() => $"{nameof(Generator)}(Converters: {converters.Count}, Creators: {creators.Count})";
    }
}
