namespace Mikodev.Binary.WireTests;

using Mikodev.Binary.Attributes;
using System;

// The object shapes used by the wire corpus, shared by both generators and by `WireCorpus`.

[NamedObject]
public sealed class NamedTiny
{
    [NamedKey("x")]
    public byte X { get; set; }
}

[NamedObject]
public sealed class NamedPerson
{
    [NamedKey("id")]
    public int Id { get; set; }

    [NamedKey("name")]
    public string? Name { get; set; }
}

[NamedObject]
public sealed class NamedMid
{
    [NamedKey("a")]
    public int A { get; set; }

    [NamedKey("b")]
    public int B { get; set; }

    [NamedKey("c")]
    public int C { get; set; }
}

[NamedObject]
public sealed class NamedWithOpt
{
    [NamedKey("v")]
    public int? V { get; set; }
}

// Tuple objects carry no names and no header of their own: members are written in key order, the last one raw. Same wire as Rust `#[binary(tuple)]`.
[TupleObject]
public sealed class TupleTwoInt
{
    [TupleKey(0)]
    public int A { get; set; }

    [TupleKey(1)]
    public int B { get; set; }
}

[TupleObject]
public sealed class TupleIntStr
{
    [TupleKey(0)]
    public int A { get; set; }

    [TupleKey(1)]
    public string? B { get; set; }
}

public enum Color
{
    Red = 1,
    Green = 2,
}

// Everything the source generator has to know about explicitly: the AOT builder already supplies primitives, Uri, Guid, decimal, DateTime and friends.
[SourceGeneratorContext]
[SourceGeneratorInclude<NamedTiny>]
[SourceGeneratorInclude<NamedPerson>]
[SourceGeneratorInclude<NamedMid>]
[SourceGeneratorInclude<NamedWithOpt>]
[SourceGeneratorInclude<TupleTwoInt>]
[SourceGeneratorInclude<TupleIntStr>]
[SourceGeneratorInclude<Color>]
[SourceGeneratorInclude<int[]>]
[SourceGeneratorInclude<int[,]>]
[SourceGeneratorInclude<string[]>]
[SourceGeneratorInclude<System.Collections.Generic.List<int>>]
[SourceGeneratorInclude<System.Collections.Generic.List<string>>]
[SourceGeneratorInclude<System.Collections.Generic.LinkedList<int>>]
[SourceGeneratorInclude<System.Collections.Generic.Queue<int>>]
[SourceGeneratorInclude<System.Collections.Generic.HashSet<int>>]
[SourceGeneratorInclude<System.Collections.Generic.Dictionary<int, string>>]
[SourceGeneratorInclude<System.Collections.Generic.SortedDictionary<int, string>>]
[SourceGeneratorInclude<System.Collections.Generic.SortedDictionary<int, int>>]
[SourceGeneratorInclude<System.Collections.Generic.SortedSet<int>>]
[SourceGeneratorInclude<System.Collections.Generic.KeyValuePair<int, string>>]
[SourceGeneratorInclude<System.ArraySegment<int>>]
[SourceGeneratorInclude<System.Memory<int>>]
[SourceGeneratorInclude<System.ReadOnlyMemory<int>>]
[SourceGeneratorInclude<int?>]
[SourceGeneratorInclude<int?[]>]
[SourceGeneratorInclude<Color[]>]
[SourceGeneratorInclude<byte[]>]
[SourceGeneratorInclude<System.Collections.Immutable.ImmutableList<int>>]
[SourceGeneratorInclude<System.Collections.Generic.List<int[]>>]
[SourceGeneratorInclude<(int, string)>]
[SourceGeneratorInclude<(int, int, string)>]
[SourceGeneratorInclude<(sbyte, short, int, long, System.Int128, System.UInt128, double, System.Half)>]
[SourceGeneratorInclude<((int, string), (int, int))>]
[SourceGeneratorInclude<(sbyte, short, int, long, System.Int128, System.UInt128, double, System.Half, float, decimal)>]
public partial class WireGeneratorContext { }
