﻿using Mikodev.Binary.Internal.Contexts;
using System.Linq;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T>
    {
        private enum EncodeOption { Constant, Variable, VariableOverride };

        private EncodeOption EncodeOptionInternal()
        {
            var length = this.length;
            if (length is not 0)
                return EncodeOption.Constant;
            var method = ContextMethods.GetEncodeMethodInfo(typeof(T), nameof(IConverter.EncodeWithLengthPrefix));
            var result = GetType().GetMethods().Single(x => x.GetBaseDefinition() == method);
            if (result.DeclaringType == typeof(Converter<T>))
                return EncodeOption.Variable;
            else
                return EncodeOption.VariableOverride;
        }

        private void EncodeAutoInternal(ref Allocator allocator, T item)
        {
            var option = this.encode;
            if (option is EncodeOption.Constant)
                Encode(ref allocator, item);
            else if (option is EncodeOption.Variable)
                EncodeWithLengthPrefixVariable(ref allocator, item);
            else
                EncodeWithLengthPrefix(ref allocator, item);
        }
    }
}