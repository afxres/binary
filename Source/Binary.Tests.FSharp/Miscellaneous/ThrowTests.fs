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

    let outofrange = ArgumentOutOfRangeException().Message

    member private __.Test<'a> () =
        let converter = generator.GetConverter<'a>()
        let buffer = Array.zeroCreate<byte> (converter.Length - 1)
        let alpha = Assert.ThrowsAny<ArgumentException>(fun () ->
            let span = ReadOnlySpan buffer
            converter.Decode &span |> ignore)
        let bravo = Assert.ThrowsAny<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
        let delta = Assert.ThrowsAny<ArgumentException>(fun () -> converter.Decode null |> ignore)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.True(alpha :? ArgumentOutOfRangeException || alpha.Message = message)
        Assert.True(bravo :? ArgumentOutOfRangeException || bravo.Message = message)
        Assert.True(delta :? ArgumentOutOfRangeException || delta.Message = message)

        let hotel = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
            let mutable span = ReadOnlySpan buffer
            converter.DecodeAuto &span |> ignore)
        Assert.Equal(outofrange, hotel.Message)
        ()

    [<Fact>]
    member me.``Bytes Not Enough Or Null Int32 Int64...`` () =
        me.Test<Int32>()
        me.Test<Int64>()
        me.Test<UInt32>()
        me.Test<UInt64>()
        ()

    [<Fact>]
    member me.``Bytes Not Enough Or Null DateTimeOffset`` () =
        me.Test<DateTimeOffset>()
        ()

    [<Fact>]
    member me.``Bytes Not Enough Or Null Decimal`` () =
        me.Test<Decimal>()
        ()

    [<Fact>]
    member me.``Bytes Not Enough Or Null Guid`` () =
        me.Test<Guid>()
        ()

    [<Fact>]
    member __.``Allocator Modified`` () =
        let converter = new BadConverter<string>()
        let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
            let mutable allocator = new Allocator()
            converter.EncodeWithLengthPrefix(&allocator, null))
        Assert.Contains(outofrange, error.Message)
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
    member __.``No Available Property``(t : Type) =
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
    member __.``Simple Or Complex Type With Invalid Type`` (t : Type, by : Type) =
        let message = sprintf "Invalid type: %O" by
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Delta`` : (obj array) seq =
        seq {
            yield [| Memory<int32>(); Int32(); 15; 3 |]
            yield [| ArraySegment<int64>(); Int64(); 7; 7 |]
            yield [| ResizeArray<TimeSpan>(); TimeSpan(); 10; 2 |]
            yield [| HashSet<DateTimeOffset>(); DateTimeOffset(); 39; 9 |]
            yield [| Dictionary<int32, int16>(); KeyValuePair<int32, int16>(); 23; 5 |]
        }

    [<Theory>]
    [<MemberData("Data Delta")>]
    member __.``Unmanaged Collection Bytes Not Match`` (collection : 'a, item : 'b, length : int, remainder : int) =
        let buffer = Array.zeroCreate<byte> length
        let converter = generator.GetConverter<'a> ()
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
        let message = sprintf "Invalid collection bytes, byte count: %d, remainder: %d, item type: %O" length remainder typeof<'b>
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()
