namespace Miscellaneous

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Net
open Xunit

type BadConverter<'T>() =
    inherit Abstractions.VariableConverter<'T>()

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
    let generator = new Generator()

    member private __.Test<'a> () =
        let error = Assert.Throws<ArgumentException>(fun () ->
            let span = ReadOnlySpan<byte>()
            generator.Decode<'a> &span |> ignore)
        let message = sprintf "Not Enough Bytes, type: %O" (typeof<'a>)
        Assert.Equal(message, error.Message)
        ()

    [<Fact>]
    member me.``Bytes Not Enough Int32`` () = me.Test<Int32>

    [<Fact>]
    member me.``Bytes Not Enough DateTimeOffset`` () = me.Test<DateTimeOffset>

    [<Fact>]
    member me.``Bytes Not Enough Decimal`` () = me.Test<Decimal>

    [<Fact>]
    member me.``Bytes Not Enough Guid`` () = me.Test<Guid>

    [<Fact>]
    member me.``Bytes Not Enough IPEndPoint`` () = me.Test<IPEndPoint>

    [<Fact>]
    member me.``Allocator Modified`` () =
        let converter = new BadConverter<string>()
        let error = Assert.Throws<InvalidOperationException>(fun () ->
            let mutable allocator = new Allocator()
            converter.EncodeWithLengthPrefix(&allocator, null))
        Assert.Equal("Allocator has been modified unexpectedly!", error.Message)
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
        [| typeof<Generator list>; typeof<Generator> |];
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
