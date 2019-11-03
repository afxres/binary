using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Text;

namespace Mikodev.Binary
{
    public abstract class Converter
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        public static readonly bool UseLittleEndian = true;

        public Type ItemType { get; }

        public int Length { get; }

        internal Converter(Type type, int length)
        {
            if (length < 0)
                ThrowHelper.ThrowConverterLengthInvalid();
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
