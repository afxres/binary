namespace Miscellaneous

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Runtime.Serialization
open Xunit

type BadConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(allocator, _) = allocator <- Allocator()

    override __.Decode (span : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

[<Class>]
type BadClassTypeWithPrivateProperty() =
    member private __.Name with get () : string = String.Empty and set (_ : string) = ()

[<Struct>]
type BadValueTypeWithPrivateProperty =
    member private __.Name with get () : string = String.Empty and set (_ : string) = ()

[<Class>]
type BadClassTypeWithSetOnlyProperty() =
    member __.Haha with set (_ : string) = ()

[<Struct>]
type BadValueTypeWithSetOnlyProperty =
    member __.Lmao with set (_ : string) = ()

[<Class>]
type BadClassTypeWithPrivateGetter() =
    member __.Source with private get () = 0 and set (_ : int) = ()

[<Struct>]
type BadValueTypeWithPrivateGetter =
    member __.Target with private get () = 0 and set (_ : int) = ()

[<Class>]
type BadClassTypeWithOnlyIndexer() =
    member __.Item with get (_i : int) : string = String.Empty and set (_i : int) (_item : string) = ()

[<Struct>]
type BadValueTypeWithOnlyIndexer =
    member __.Item with get (_i : int) : string = String.Empty and set (_i : int) (_item : string) = ()

type EmptyDelegate = delegate of uint -> unit

type EmptyConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(allocator, _) = ()

    override __.Decode (span : inref<ReadOnlySpan<byte>>) : 'T = Unchecked.defaultof<'T>

type EmptyConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(_, t) =
            if t = typeof<ValueTuple> || t.IsSubclassOf typeof<Delegate> || t.Assembly = typeof<IConverter>.Assembly || t.Assembly = typeof<obj>.Assembly then
                Activator.CreateInstance(typedefof<EmptyConverter<_>>.MakeGenericType t) :?> IConverter
            else
                null

type ThrowConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(_, _) =
            raise (NotSupportedException("Not supported."))

type ThrowTests() =
    let generator = Generator.CreateDefault()

    let outofrange = ArgumentOutOfRangeException().Message

    let GeneratorBuilder() =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorBuilder") |> Array.exactlyOne
        let builder = Activator.CreateInstance(t)
        builder :?> IGeneratorBuilder

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
    member __.``Allocator Modified`` () =
        let converter = BadConverter<string>()
        let error = Assert.Throws<InvalidOperationException>(fun () ->
            let mutable allocator = Allocator()
            converter.EncodeWithLengthPrefix(&allocator, null))
        let message = "Allocator has been modified unexpectedly!"
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Alpha`` = [|
        [| typeof<BadClassTypeWithPrivateProperty> |];
        [| typeof<BadValueTypeWithPrivateProperty> |];
        [| typeof<BadClassTypeWithOnlyIndexer> |];
        [| typeof<BadValueTypeWithOnlyIndexer> |];
    |]

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``No Available Property``(t : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(sprintf "No available member found, type: %O" t, error.Message)
        ()

    static member ``Data No Public Getter`` = [|
        [| box typeof<BadClassTypeWithSetOnlyProperty>; box "Haha" |];
        [| box typeof<BadValueTypeWithSetOnlyProperty>; box "Lmao" |];
        [| box typeof<BadClassTypeWithPrivateGetter>; box "Source" |];
        [| box typeof<BadValueTypeWithPrivateGetter>; box "Target" |];
    |]

    [<Theory>]
    [<MemberData("Data No Public Getter")>]
    member __.``No Available Getter`` (t : Type, name : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(sprintf "No available getter found, member name: %s, type: %O" name t, error.Message)
        ()

    static member ``Data Delegate`` = [|
        [| typeof<Delegate>; typeof<Delegate> |];
        [| typeof<Predicate<int>>; typeof<Predicate<int>> |];
        [| typeof<EmptyDelegate>; typeof<EmptyDelegate> |];
        [| typeof<EmptyDelegate array>; typeof<EmptyDelegate> |];
        [| typeof<EmptyDelegate * int>; typeof<EmptyDelegate> |];
    |]

    static member ``Data Internal`` = [|
        [| typeof<Token>; typeof<Token> |];
        [| typeof<Token array>; typeof<Token> |];
        [| typeof<Token ResizeArray>; typeof<Token> |];
        [| typeof<Token IList>; typeof<Token> |];
        [| typeof<Token Memory>; typeof<Token> |];
        [| typeof<Token * int>; typeof<Token> |];
        [| typeof<struct (int * Token)>; typeof<Token> |];
        [| typeof<IConverter HashSet>; typeof<IConverter> |];
        [| typeof<IConverter list>; typeof<IConverter> |];
    |]

    static member ``Data Invalid`` = [|
        [| typeof<ValueTuple Set>; typeof<ValueTuple> |];
        [| typeof<ValueTuple ICollection>; typeof<ValueTuple> |];
        [| typeof<ValueTuple IEnumerable>; typeof<ValueTuple> |];
        [| typeof<Map<ValueTuple, int>>; typeof<ValueTuple> |];
        [| typeof<Dictionary<ValueTuple, int>>; typeof<ValueTuple> |];
        [| typeof<IDictionary<ValueTuple, int>>; typeof<ValueTuple> |];
    |]

    static member ``Data Pointer`` = [|
        [| typeof<int>.MakePointerType(); typeof<int>.MakePointerType() |];
        [| typeof<double>.MakePointerType(); typeof<double>.MakePointerType() |];
    |]

    static member ``Data Static`` = [|
        [| typeof<BitConverter>; typeof<BitConverter> |];
        [| typeof<MemoryExtensions>; typeof<MemoryExtensions> |];
    |]

    static member ``Data System`` : (obj array) seq = seq {
        yield [| typeof<ICloneable>; typeof<ICloneable> |]
        yield [| typeof<ISerializable>; typeof<ISerializable> |]
        yield [| typeof<Nullable<double>>; typeof<Nullable<double>> |]
    }

    [<Theory>]
    [<MemberData("Data Static")>]
    [<MemberData("Data Pointer")>]
    member __.``Simple Or Complex Type Strictly Invalid`` (t : Type, by : Type) =
        let g = GeneratorBuilder().AddConverterCreator(ThrowConverterCreator()).Build()
        let a = Assert.Throws<NotSupportedException>(fun () -> g.GetConverter<int>() |> ignore)
        Assert.Equal("Not supported.", a.Message)
        let b = Assert.Throws<ArgumentException>(fun () -> g.GetConverter t |> ignore)
        Assert.StartsWith("Invalid ", b.Message)
        Assert.EndsWith($" type: {by}", b.Message)
        ()

    [<Theory>]
    [<MemberData("Data System")>]
    [<MemberData("Data Invalid")>]
    [<MemberData("Data Internal")>]
    [<MemberData("Data Delegate")>]
    member __.``Simple Or Complex Type Control Group`` (t : Type, by : Type) =
        let g = GeneratorBuilder().AddConverterCreator(EmptyConverterCreator()).Build()
        let a = g.GetConverter t
        let b = g.GetConverter by
        Assert.NotNull a
        Assert.IsType(typedefof<EmptyConverter<_>>.MakeGenericType by, b)
        ()

    [<Theory>]
    [<MemberData("Data Invalid")>]
    member __.``Simple Or Complex Type With Invalid Type`` (t : Type, by : Type) =
        let message = sprintf "Invalid type: %O" by
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Data Internal")>]
    member __.``Simple Or Complex Type With Invalid Internal Type`` (t : Type, by : Type) =
        let message = sprintf "Invalid internal type: %O" by
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Data Delegate")>]
    member __.``Simple Or Complex Type With Delegate Type`` (t : Type, by : Type) =
        let message = sprintf "Invalid delegate type: %O" by
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Data Pointer")>]
    member __.``Simple Or Complex Type With Pointer Type`` (t : Type, by : Type) =
        let message = sprintf "Invalid pointer type: %O" by
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Data System")>]
    member __.``Invalid System Type`` (t : Type, expected : Type) =
        let generator = GeneratorBuilder().Build()
        Assert.Equal("Converter Count = 1, Converter Creator Count = 0", generator.ToString())
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
        let message = sprintf "Invalid system type: %O" expected
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Delta`` : (obj array) seq = seq {
        yield [| Memory<int32>(); Int32(); 15; |]
        yield [| ArraySegment<int64>(); Int64(); 7; |]
        yield [| ResizeArray<TimeSpan>(); TimeSpan(); 10; |]
        yield [| Dictionary<int32, int16>(); KeyValuePair<int32, int16>(); 23; |]
    }

    [<Theory>]
    [<MemberData("Data Delta")>]
    member __.``Unmanaged Collection Bytes Not Match`` (_ : 'a, _ : 'b, length : int) =
        let buffer = Array.zeroCreate<byte> length
        let converter = generator.GetConverter<'a> ()
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
        let message = sprintf "Not enough bytes for collection element, byte length: %d, element type: %O" length typeof<'b>
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()
