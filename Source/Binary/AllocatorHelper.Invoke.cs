using Mikodev.Binary.Internal;

namespace Mikodev.Binary
{
    public static partial class AllocatorHelper
    {
        public static byte[] Invoke<T>(T data, AllocatorAction<T> action)
        {
            if (action is null)
                ThrowHelper.ThrowAllocatorActionInvalid();
            var memory = BufferHelper.Borrow();
            try
            {
                var allocator = new Allocator(BufferHelper.Intent(memory));
                action.Invoke(ref allocator, data);
                return Allocator.Result(ref allocator);
            }
            finally
            {
                BufferHelper.Return(memory);
            }
        }
    }
}
