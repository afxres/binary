using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Converters.Runtime
{
    internal sealed class NamedObjectConverter<T> : VariableConverter<T>
    {
        private readonly OfNamedObject<T> ofObject;

        private readonly ToNamedObject<T> toObject;

        private readonly PropertyInfo[] properties;

        private readonly HybridList indexes;

        public NamedObjectConverter(OfNamedObject<T> ofObject, ToNamedObject<T> toObject, PropertyInfo[] properties, KeyValuePair<string, byte[]>[] buffers)
        {
            Debug.Assert(properties.Length == buffers.Length);
            this.ofObject = ofObject;
            this.toObject = toObject;
            this.properties = properties;
            indexes = new HybridList(buffers);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T ThrowKeyExists(int propertyIndex)
        {
            var property = properties[propertyIndex];
            var message = $"Key '{property.Name}' already exists, property type: {property.PropertyType}, declaring type: {ItemType}";
            throw new ArgumentException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T ThrowNotExists(int propertyIndex)
        {
            var property = properties[propertyIndex];
            var message = $"Key '{property.Name}' not found, property type: {property.PropertyType}, declaring type: {ItemType}";
            throw new ArgumentException(message);
        }

        public override void ToBytes(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            ofObject.Invoke(ref allocator, item);
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            if (toObject == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var byteCount = span.Length;
            if (byteCount == 0)
                return default(T) == null ? default : ThrowHelper.ThrowNotEnoughBytes<T>();

            var itemCount = properties.Length;
            var items = new LengthItem[itemCount];
            var reader = new LengthReader(byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);

            while (reader.Any())
            {
                reader.Update(ref source);
                var index = indexes.Get(span.Slice(reader.Offset, reader.Length));
                reader.Update(ref source);
                if (index < 0)
                    continue;
                Debug.Assert((uint)index < (uint)itemCount);
                if (items[index].Offset != 0)
                    return ThrowKeyExists(index);
                items[index] = new LengthItem(reader.Offset, reader.Length);
            }

            for (var i = 0; i < itemCount; i++)
                if (items[i].Offset == 0)
                    return ThrowNotExists(i);
            var list = new LengthList(items, in span);
            return toObject.Invoke(in list);
        }
    }
}
