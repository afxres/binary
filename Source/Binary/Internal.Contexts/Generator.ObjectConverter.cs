using System;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed partial class Generator
    {
        private sealed class ObjectConverter : Converter<object>
        {
            private static readonly string message = $"Invalid type: {typeof(object)}";

            private readonly IGenerator generator;

            public ObjectConverter(IGenerator generator) => this.generator = generator;

            public override void Encode(ref Allocator allocator, object item)
            {
                if (item == null)
                    throw new ArgumentException("Can not get type of null object.");
                var type = item.GetType();
                if (type == typeof(object))
                    throw new ArgumentException(message);
                var converter = (IConverter)generator.GetConverter(type);
                converter.Encode(ref allocator, item);
            }

            public override object Decode(in ReadOnlySpan<byte> span) => throw new ArgumentException(message);
        }
    }
}
