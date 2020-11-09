using Mikodev.Binary.Internal.Contexts;
using Mikodev.Binary.Internal.Fallback.Decoders;
using System.Linq;

namespace Mikodev.Binary.Internal.Fallback
{
    internal static class FallbackDecoderHelper
    {
        internal static FallbackDecoder<T> Create<T>(Converter<T> converter)
        {
            var length = converter.Length;
            if (length is not 0)
                return new ConstantDecoder<T>(converter);
            var method = ContextMethods.GetDecodeMethodInfo(typeof(T), nameof(IConverter.DecodeWithLengthPrefix));
            var result = converter.GetType().GetMethods().Single(x => x.GetBaseDefinition() == method);
            if (result.DeclaringType == typeof(Converter<T>))
                return new VariableDecoder<T>(converter);
            else
                return new VariableOverriddenDecoder<T>(converter);
        }
    }
}
