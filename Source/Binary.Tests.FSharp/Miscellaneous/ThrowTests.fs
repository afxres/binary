namespace Miscellaneous

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

type BadConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(allocator, item) = allocator <- new Allocator()

    override __.Decode (span : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

[<Class>]
type BadClassTypeWithPrivateProperty() =
    member private __.Name with get () : string = String.Empty and set (_ : string) = ()

[<Struct>]
type BadValueTypeWithPrivateProperty =
    member private __.Name with get () : string = String.Empty and set (_ : string) = ()

[<Class>]
type BadClassTypeWithSetOnlyProperty() =
    member __.Name with set (_ : string) = ()

[<Struct>]
type BadValueTypeWithSetOnlyProperty =
    member __.Name with set (_ : string) = ()

[<Class>]
type BadClassTypeWithOnlyIndexer() =
    member __.Item with get (i : int) : string = String.Empty and set (i : int) (item : string) = ()

[<Struct>]
type BadValueTypeWithOnlyIndexer =
    member __.Item with get (i : int) : string = String.Empty and set (i : int) (item : string) = ()

type ThrowTests() =
    let generator = Generator.CreateDefault()

    member private __.Test<'a> () =
        let converter = generator.GetConverter<'a>()
        let buffer = Array.zeroCreate<byte> (converter.Length - 1)
        let alpha = Assert.ThrowsAny<ArgumentException>(fun () ->
            let span = ReadOnlySpan buffer
            generator.Decode<'a> span |> ignore)
        let bravo = Assert.ThrowsAny<ArgumentException>(fun () -> generator.Decode<'a> buffer |> ignore)
        let delta = Assert.ThrowsAny<ArgumentException>(fun () -> generator.Decode<'a> null |> ignore)
        let message = "Not enough bytes."
        Assert.True(alpha :? ArgumentOutOfRangeException || alpha.Message = message)
        Assert.True(bravo :? ArgumentOutOfRangeException || bravo.Message = message)
        Assert.True(delta :? ArgumentOutOfRangeException || delta.Message = message)
        ()

    [<Fact>]
    member me.``Bytes Not Enough Or Null Int32 Int64...`` () =
        me.Test<Int32>()
        me.Test<Int64>()
        me.Test<UInt32>()
        me.Test<UInt64>()
        ()

    [<Fact>]
    member me.``Bytes Not Enough Or Null DateTimeOffset`` () = me.Test<DateTimeOffset>()

    [<Fact>]
    member me.``Bytes Not Enough Or Null Decimal`` () = me.Test<Decimal>()

    [<Fact>]
    member me.``Bytes Not Enough Or Null Guid`` () = me.Test<Guid>()

    [<Fact>]
    member me.``Allocator Modified`` () =
        let converter = new BadConverter<string>()
        let error = Assert.Throws<ArgumentException>(fun () ->
            let mutable allocator = new Allocator()
            converter.EncodeWithLengthPrefix(&allocator, null))
        Assert.Equal("Invalid length prefix anchor or allocator modified.", error.Message)
        ()

    static member ``Data Alpha`` = [|
        [| typeof<BadClassTypeWithPrivateProperty> |];
        [| typeof<BadValueTypeWithPrivateProperty> |];
        [| typeof<BadClassTypeWithSetOnlyProperty> |];
        [| typeof<BadValueTypeWithSetOnlyProperty> |];
        [| typeof<BadClassTypeWithOnlyIndexer> |];
        [| typeof<BadValueTypeWithOnlyIndexer> |];
    |]

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member me.``No Available Property``(t : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Contains(sprintf "No available property found, type: %O" t, error.Message)
        ()

    static member ``Data Bravo`` = [|
        [| typeof<Token>; typeof<Token> |];
        [| typeof<Token array>; typeof<Token> |];
        [| typeof<Token ResizeArray>; typeof<Token> |];
        [| typeof<Token IList>; typeof<Token> |];
        [| typeof<Token Memory>; typeof<Token> |];
        [| typeof<Token ArraySegment>; typeof<Token> |];
        [| typeof<Token * int>; typeof<Token> |];
        [| typeof<struct (int * Token)>; typeof<Token> |];
        [| typeof<Converter HashSet>; typeof<Converter> |];
        [| typeof<Converter Stack>; typeof<Converter> |];
        [| typeof<Converter Queue>; typeof<Converter> |];
        [| typeof<Converter list>; typeof<Converter> |];
        [| typeof<ValueTuple Set>; typeof<ValueTuple> |];
        [| typeof<ValueTuple ICollection>; typeof<ValueTuple> |];
        [| typeof<ValueTuple IEnumerable>; typeof<ValueTuple> |];
        [| typeof<Map<ValueTuple, int>>; typeof<ValueTuple> |];
        [| typeof<Dictionary<ValueTuple, int>>; typeof<ValueTuple> |];
        [| typeof<IDictionary<ValueTuple, int>>; typeof<ValueTuple> |];
    |]

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member me.``Simple Or Complex Type With Invalid Type`` (t : Type, by : Type) =
        let message = sprintf "Invalid type: %O" by
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(message, error.Message)
        ()
