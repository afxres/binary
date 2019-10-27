using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        public readonly ref struct LengthPrefixAnchor
        {
            internal readonly int Offset;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal LengthPrefixAnchor(int anchor) => Offset = anchor;

            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            public override bool Equals(object obj) => throw new NotSupportedException();

            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            public override int GetHashCode() => throw new NotSupportedException();

            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            public override string ToString() => $"{nameof(LengthPrefixAnchor)}({nameof(Offset)}: {Offset})";
        }
    }
}
