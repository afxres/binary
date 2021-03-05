using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T>
    {
        private byte[] EncodeInternal(T item)
        {
            var length = this.length;
            if (length is not 0)
                return EncodeConstant(item);
            else
                return EncodeVariable(item);
        }

        private byte[] EncodeConstant(T item)
        {
            var length = this.length;
            var buffer = new byte[length];
            var allocator = new Allocator(new Span<byte>(buffer), maxCapacity: length);
            Encode(ref allocator, item);
            return buffer;
        }

        private byte[] EncodeVariable(T item)
        {
            var handle = BufferHelper.Borrow();
            try
            {
                var allocator = new Allocator(BufferHelper.Result(handle));
                Encode(ref allocator, item);
                return allocator.ToArray();
            }
            finally
            {
                BufferHelper.Return(handle);
            }
        }
    }
}
