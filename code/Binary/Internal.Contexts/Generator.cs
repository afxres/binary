using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class Generator : IGenerator
    {
        private readonly ImmutableArray<IConverterCreator> creators;

        private readonly ConcurrentDictionary<Type, IConverter> converters;

        public Generator(ImmutableArray<IConverterCreator> creators, ImmutableDictionary<Type, IConverter> converters)
        {
            var builder = creators.ToBuilder();
            builder.Reverse();
            this.creators = builder.ToImmutable();
            this.converters = new ConcurrentDictionary<Type, IConverter>(converters) { [typeof(object)] = new GeneratorObjectConverter(this) };
            Debug.Assert(this.converters.All(x => x.Value is not null));
            Debug.Assert(this.creators.Length is 0 || this.creators.All(x => x is not null));
        }

        public IConverter GetConverter(Type type)
        {
            if (type is null)
                ThrowHelper.ThrowTypeNull();
            if (this.converters.TryGetValue(type, out var result))
                return result;
            var context = new GeneratorContext(this.creators, this.converters);
            try
            {
                return context.GetConverter(type);
            }
            finally
            {
                context.Destroy();
            }
        }

        public override string ToString() => $"{nameof(Generator)}(Converters: {this.converters.Count}, Creators: {this.creators.Length})";
    }
}
