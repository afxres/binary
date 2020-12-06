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
            Debug.Assert(this.converters.All(x => x.Value is not null));
            Debug.Assert(this.creators.Count is 0 || this.creators.All(x => x is not null));
        }

        public IConverter GetConverter(Type type)
        {
            if (type is null)
                ThrowHelper.ThrowTypeNull();
            if (this.converters.TryGetValue(type, out var result))
                return result;
            var context = new GeneratorContext(this.converters, this.creators);
            try
            {
                return context.GetConverter(type);
            }
            finally
            {
                context.Destroy();
            }
        }

        public override string ToString() => $"{nameof(Generator)}(Converters: {this.converters.Count}, Creators: {this.creators.Count})";
    }
}
