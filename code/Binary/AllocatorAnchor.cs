﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public readonly ref struct AllocatorAnchor
    {
        internal readonly int Offset;

        internal readonly int Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal AllocatorAnchor(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(AllocatorAnchor)}({nameof(Offset)}: {Offset}, {nameof(Length)}: {Length})";
    }
}