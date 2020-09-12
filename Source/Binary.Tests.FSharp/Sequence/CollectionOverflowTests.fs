namespace Sequence

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

type SimpleVariableLengthConverter () =
    inherit Converter<byte> 0

    member val DecodeAutoCalledCount = 0 with get, set

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : byte = raise (NotSupportedException())

    override __.EncodeAuto(_, _) = raise (NotSupportedException())

    override me.DecodeAuto _ =
        me.DecodeAutoCalledCount <- me.DecodeAutoCalledCount + 1
        0x7Fuy

type Backup<'a> = { data : 'a }

type BackupConverter<'a>(length : int) =
    inherit Converter<Backup<'a>>(length)

    override __.Encode(_, _) = ()

    override __.Decode (_ : inref<ReadOnlySpan<byte>>) : Backup<'a> = raise (NotSupportedException())

type CollectionOverflowTests () =
    [<Fact>]
    member __.``Large Collection Decode (variable item length, overflow or out of memory)`` () =
        let simpleConverter = SimpleVariableLengthConverter()
        let generator = Generator.CreateDefaultBuilder().AddConverter(simpleConverter).Build()
        let converter = generator.GetConverter<Memory<byte>>()

        Assert.Equal(0, simpleConverter.DecodeAutoCalledCount)
        let error = Assert.ThrowsAny<Exception>(fun () -> converter.Decode [| 0uy |] |> ignore)
        Assert.True(error :? OverflowException || error :? OutOfMemoryException)
        if Environment.Is64BitProcess then
            Assert.IsType<OverflowException>(error) |> ignore
        if error :? OverflowException then
            Assert.Equal((1 <<< 30) + 1, simpleConverter.DecodeAutoCalledCount)
        ()

    [<Fact>]
    member __.``Large Collection Encode With Length Prefix (constant item length, overflow)`` () =
        let backupConverter = BackupConverter<int>(0x10_0000)
        let generator = Generator.CreateDefaultBuilder().AddConverter(backupConverter).Build()
        let backup = { data = 4 }
        Assert.Throws<OverflowException>(fun () ->
            let mutable allocator = Allocator()
            generator.GetConverter<Backup<int> array>().EncodeWithLengthPrefix(&allocator, Array.create 0x1000 backup) |> ignore) |> ignore
        Assert.Throws<OverflowException>(fun () ->
            let mutable allocator = Allocator()
            generator.GetConverter<LinkedList<Backup<int>>>().EncodeWithLengthPrefix(&allocator, Array.create 0x1000 backup |> LinkedList<_>) |> ignore) |> ignore
        ()
