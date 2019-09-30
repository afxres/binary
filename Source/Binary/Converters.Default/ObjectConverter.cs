using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.Converters.Default
{
    internal sealed class ObjectConverter : VariableConverter<object>
    {
        private static readonly string message = $"Invalid type: {typeof(object)}";

        private readonly Generator generator;

        public ObjectConverter(Generator generator) => this.generator = generator;

        public override void ToBytes(ref Allocator allocator, object item)
        {
            if (item == null)
                throw new ArgumentException("Can not get type of null object.");
            var type = item.GetType();
            if (type == typeof(object))
                throw new ArgumentException(message);
            var converter = (IConverter)generator.GetConverter(type);
            converter.ToBytes(ref allocator, item);
        }

        public override object ToValue(in ReadOnlySpan<byte> span) => throw new ArgumentException(message);
    }
}
