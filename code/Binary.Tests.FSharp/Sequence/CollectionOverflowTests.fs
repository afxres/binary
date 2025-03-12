namespace Sequence

open Mikodev.Binary
open System
open System.Reflection
open System.Runtime.InteropServices
open Xunit

type Expand<'a> = delegate of source: byref<'a array> * item: 'a -> unit

type SimpleVariableByteConverter() =
    inherit Converter<byte> 0

    member val DecodeAutoCalledCount = 0 with get, set

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : byte = raise (NotSupportedException())

    override __.EncodeAuto(_, _) = raise (NotSupportedException())

    override me.DecodeAuto span =
        me.DecodeAutoCalledCount <- me.DecodeAutoCalledCount + 1
        let item = span.[0]
        span <- span.Slice 1
        item

type Backup<'a> = { data: 'a }

type BackupConverter<'a>(length: int) =
    inherit Converter<Backup<'a>>(length)

    override __.Encode(_, _) = ()

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : Backup<'a> = raise (NotSupportedException())

type CollectionOverflowTests() =
    static member ``Data Alpha``: (obj array) seq = seq {
        yield [| 0; 0 |]
        yield [| 1; 1 |]
        yield [| 31; 31 |]
        yield [| 47; 64 |]
        yield [| 1024 * 1024 + 1; 1024 * 1024 * 2 |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Large Collection Decode (variable item length)``(byteLength: int, underlyingLength: int) =
        let simpleConverter = SimpleVariableByteConverter()
        let generator = Generator.CreateDefaultBuilder().AddConverter(simpleConverter).Build()
        let converter = generator.GetConverter<ReadOnlyMemory<byte>>()

        Assert.Equal(0, simpleConverter.DecodeAutoCalledCount)
        let buffer = Array.zeroCreate<byte> byteLength
        let memory = converter.Decode buffer
        Assert.Equal(byteLength, simpleConverter.DecodeAutoCalledCount)
        Assert.Equal(byteLength, memory.Length)
        let flag, segment = MemoryMarshal.TryGetArray memory
        Assert.True flag
        Assert.Equal(byteLength, segment.Count)
        Assert.Equal(0, segment.Offset)
        Assert.Equal(underlyingLength, segment.Array.Length)
        ()

    [<Fact>]
    member __.``Large Collection Decode (array length overflow)``() =
        let adapterType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "SpanLikeMethods") |> Array.exactlyOne
        let methods = adapterType.GetMethods(BindingFlags.Static ||| BindingFlags.NonPublic)
        let method = methods |> Array.filter (fun x -> x.Name.Contains "Expand" && x.GetParameters().Length = 2) |> Array.exactlyOne
        let expand = Delegate.CreateDelegate(typeof<Expand<byte>>, method.MakeGenericMethod(typeof<byte>)) :?> Expand<byte>

        let mutable buffer = Array.zeroCreate<byte> (1 <<< 30)
        Assert.Throws<OverflowException>(fun () -> expand.Invoke(&buffer, 0uy)) |> ignore
        ()
