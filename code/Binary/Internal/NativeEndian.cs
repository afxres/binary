namespace Mikodev.Binary.Internal;

using Mikodev.Binary.Creators.Endianness;
using System;

internal static class NativeEndian
{
    internal static bool IsNativeEndianConverter(IConverter converter)
    {
        static bool Invoke(IConverter converter, bool isLittleEndian)
        {
            if (isLittleEndian is false)
                return false;
            var type = converter.GetType();
            if (type.IsGenericType is false)
                return false;
            var definition = type.GetGenericTypeDefinition();
            return definition == typeof(LittleEndianConverter<>) || definition == typeof(RepeatLittleEndianConverter<,>);
        }

        return Invoke(converter, BitConverter.IsLittleEndian);
    }
}
