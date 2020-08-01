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
        let throwExpected (action : unit -> unit) =
            let error = Assert.ThrowsAny<ArgumentException> action
            if error :? ArgumentOutOfRangeException then
                Assert.StartsWith(outofrange, error.Message)
            else
                let message = "Not enough bytes or byte sequence invalid."
                Assert.Equal(message, error.Message)
            ()

        let converter = generator.GetConverter<'a>()
        throwExpected (fun () -> converter.Decode null |> ignore)

        for i = 0 to (converter.Length - 1) do
            let buffer = Array.zeroCreate<byte> i
            throwExpected (fun () -> converter.Decode buffer |> ignore)
            throwExpected (fun () -> let span = ReadOnlySpan buffer in converter.Decode &span |> ignore)
            throwExpected (fun () -> let mutable span = ReadOnlySpan buffer in converter.DecodeAuto &span |> ignore)
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
        let error = Assert.Throws<InvalidOperationException>(fun () ->
            let mutable allocator = new Allocator()
            converter.EncodeWithLengthPrefix(&allocator, null))
        let message = "Allocator or internal anchor has been modified unexpectedly!"
        Assert.Equal(message, error.Message)
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
        [| typeof<IConverter HashSet>; typeof<IConverter> |];
        [| typeof<IConverter Queue>; typeof<IConverter> |];
        [| typeof<IConverter list>; typeof<IConverter> |];
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
            yield [| Memory<int32>(); Int32(); 15; |]
            yield [| ArraySegment<int64>(); Int64(); 7; |]
            yield [| ResizeArray<TimeSpan>(); TimeSpan(); 10; |]
            yield [| Dictionary<int32, int16>(); KeyValuePair<int32, int16>(); 23; |]
        }

    [<Theory>]
    [<MemberData("Data Delta")>]
    member __.``Unmanaged Collection Bytes Not Match`` (collection : 'a, item : 'b, length : int) =
        let buffer = Array.zeroCreate<byte> length
        let converter = generator.GetConverter<'a> ()
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
        let message = sprintf "Not enough bytes for collection element, byte length: %d, element type: %O" length typeof<'b>
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()
