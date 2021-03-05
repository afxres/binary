using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Linq;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T>
    {
        private enum DecodeOption { Constant, Variable, VariableOverride };

        private DecodeOption DecodeOptionInternal()
        {
            var length = this.length;
            if (length is not 0)
                return DecodeOption.Constant;
            var method = ContextMethods.GetDecodeMethodInfo(typeof(T), nameof(IConverter.DecodeWithLengthPrefix));
            var result = GetType().GetMethods().Single(x => x.GetBaseDefinition() == method);
            if (result.DeclaringType == typeof(Converter<T>))
                return DecodeOption.Variable;
            else
                return DecodeOption.VariableOverride;
        }

        private T DecodeAutoInternal(ref ReadOnlySpan<byte> span)
        {
            var option = this.decode;
            if (option is DecodeOption.Constant)
                return Decode(MemoryHelper.EnsureLengthReturnBuffer(ref span, Length));
            else if (option is DecodeOption.Variable)
                return Decode(Converter.DecodeWithLengthPrefix(ref span));
            else
                return DecodeWithLengthPrefix(ref span);
        }
    }
}
