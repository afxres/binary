using Mikodev.Binary.Internal.Fallback.Adapters;

namespace Mikodev.Binary.Internal.Fallback
{
    internal static class FallbackAdapterHelper
    {
        internal static FallbackAdapter<T> Create<T>(Converter<T> converter)
        {
            var length = converter.Length;
            if (length is 0)
                return new VariableAdapter<T>(converter);
            if (MemoryHelper.EncodeNumberLength((uint)length) is 1)
                return new ConstantAdapter<T, byte>(converter);
            else
                return new ConstantAdapter<T, uint>(converter);
        }
    }
}
