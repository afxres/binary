using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;

namespace Mikodev.Binary
{
    public abstract class Converter
    {
        public Type ItemType { get; }

        public int Length { get; }

        internal Converter(Type type, int length)
        {
            if (length < 0)
                ThrowHelper.ThrowArgumentLengthInvalid();
            Length = length;
            ItemType = type;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(Converter)}({nameof(Length)}: {Length}, {nameof(ItemType)}: {ItemType})";
    }
}
