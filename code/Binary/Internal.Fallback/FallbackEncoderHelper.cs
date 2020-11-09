using Mikodev.Binary.Internal.Contexts;
using Mikodev.Binary.Internal.Fallback.Encoders;
using System.Linq;

namespace Mikodev.Binary.Internal.Fallback
{
    internal static class FallbackEncoderHelper
    {
        internal static FallbackEncoder<T> Create<T>(Converter<T> converter)
        {
            var length = converter.Length;
            if (length is not 0)
                return new ConstantEncoder<T>(converter);
            var method = ContextMethods.GetEncodeMethodInfo(typeof(T), nameof(IConverter.EncodeWithLengthPrefix));
            var result = converter.GetType().GetMethods().Single(x => x.GetBaseDefinition() == method);
            if (result.DeclaringType == typeof(Converter<T>))
                return new VariableEncoder<T>(converter);
            else
                return new VariableOverriddenEncoder<T>(converter);
        }
    }
}
