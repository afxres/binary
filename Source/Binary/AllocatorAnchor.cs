using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public readonly ref struct AllocatorAnchor
    {
        internal readonly int Offset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal AllocatorAnchor(int anchor) => Offset = anchor;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(AllocatorAnchor)}({nameof(Offset)}: {Offset})";
    }
}
