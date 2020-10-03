namespace Mikodev.Binary.Internal

open Mikodev.Binary

type UnionEncoder<'T> = delegate of allocator : byref<Allocator> * item : 'T * mark : byref<int> -> unit
