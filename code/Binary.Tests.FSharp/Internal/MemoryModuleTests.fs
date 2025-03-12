namespace Internal

open Mikodev.Binary
open System
open System.Linq
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Xunit

type Encode<'T> = delegate of location: byref<byte> * item: 'T -> unit

type Decode<'T> = delegate of location: byref<byte> -> 'T

type EnsureLength = delegate of span: ReadOnlySpan<byte> * length: int -> byref<byte>

type EnsureLengthReference = delegate of span: byref<ReadOnlySpan<byte>> * length: int -> byref<byte>

type MemoryModuleTests() =
    member private __.MakeDelegate<'T when 'T :> Delegate>(method: string) =
        let s = method.Split('.')
        Assert.Equal(2, s.Length)
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = s.[0]) |> Array.exactlyOne
        let a = t.GetMethods(BindingFlags.Static ||| BindingFlags.NonPublic) |> Array.filter (fun x -> x.Name = s.[1])
        if not typeof<'T>.IsGenericType then
            let m =
                a
                |> Array.filter (fun x ->
                    let p = [| for i in x.GetParameters() -> i.ParameterType |] in
                    let v = [| for i in (typeof<'T>.GetMethod("Invoke").GetParameters()) -> i.ParameterType |] in
                    p.SequenceEqual(v))
                |> Seq.exactlyOne
            let d = Delegate.CreateDelegate(typeof<'T>, m)
            d :?> 'T
        else
            let m =
                match a with
                | [| x |] -> x
                | _ ->
                    a
                    |> Array.filter (fun x -> let p = x.GetParameters() in p.Length > 0 && p.[0].ParameterType = typeof<byte>.MakeByRefType())
                    |> Array.exactlyOne
            Assert.NotNull m
            let d = Delegate.CreateDelegate(typeof<'T>, m.MakeGenericMethod(typeof<'T>.GetGenericArguments()))
            d :?> 'T

    static member ``Data Delta``: (obj array) seq = seq {
        yield [| int16 0xAC96; [| 0x96uy; 0xACuy |] |]
        yield [| int32 0x1122_CCDD; [| 0xDDuy; 0xCCuy; 0x22uy; 0x11uy |] |]
        yield [| int64 0xEFFE_3289_DC5A_7503UL; [| 0x03uy; 0x75uy; 0x5Auy; 0xDCuy; 0x89uy; 0x32uy; 0xFEuy; 0xEFuy |] |]
    }

    [<Theory>]
    [<MemberData("Data Delta")>]
    member me.``Encode Then Decode (integration test)``(item: 'a, littleEndian: byte array) =
        let appendLittleEndian = me.MakeDelegate<Encode<'a>>("LittleEndian.Encode")
        let detachLittleEndian = me.MakeDelegate<Decode<'a>>("LittleEndian.Decode")

        let buffer = Array.create 128 0uy
        let span = Span buffer
        let location = &MemoryMarshal.GetReference span

        span.Fill 0uy
        appendLittleEndian.Invoke(&location, item)
        let littleResult = detachLittleEndian.Invoke(&location)
        Assert.Equal<'a>(item, littleResult)
        Assert.Equal<byte>(littleEndian, span.Slice(0, littleEndian.Length).ToArray())
        ()

    [<Theory>]
    [<InlineData(0, 1)>]
    [<InlineData(0, -1)>]
    [<InlineData(15, 16)>]
    [<InlineData(127, -16)>]
    member me.``Ensure Length (enough, error)``(actual: int, required: int) =
        let ensure = me.MakeDelegate<EnsureLength> "MemoryModule.EnsureLength"
        let error =
            Assert.Throws<ArgumentException>(fun () ->
                let buffer = Array.zeroCreate actual
                ensure.Invoke(ReadOnlySpan buffer, required) |> ignore)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(1, 1)>]
    [<InlineData(16, 16)>]
    member me.``Ensure Length (enough)``(actual: int, required: int) =
        let ensure = me.MakeDelegate<EnsureLength> "MemoryModule.EnsureLength"
        let buffer = Array.zeroCreate actual
        let location = &ensure.Invoke(ReadOnlySpan buffer, required)
        Assert.True(Unsafe.AreSame(&location, &buffer.[0]))
        ()

    [<Theory>]
    [<InlineData(0, 1)>]
    [<InlineData(0, -1)>]
    [<InlineData(15, 16)>]
    [<InlineData(127, -16)>]
    member me.``Ensure Length (reference, error)``(actual: int, required: int) =
        let ensure = me.MakeDelegate<EnsureLengthReference> "MemoryModule.EnsureLength"
        let error =
            Assert.Throws<ArgumentException>(fun () ->
                let buffer = Array.zeroCreate actual
                let mutable span = ReadOnlySpan buffer
                ensure.Invoke(&span, required) |> ignore)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.StartsWith(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(1, 1, 0)>]
    [<InlineData(8, 5, 3)>]
    [<InlineData(16, 16, 0)>]
    [<InlineData(64, 16, 48)>]
    member me.``Ensure Length (reference)``(actual: int, required: int, remain: int) =
        let ensure = me.MakeDelegate<EnsureLengthReference> "MemoryModule.EnsureLength"
        let buffer = Array.zeroCreate actual
        let mutable span = ReadOnlySpan buffer
        let location = &ensure.Invoke(&span, required)
        Assert.True(Unsafe.AreSame(&location, &buffer.[0]))
        Assert.Equal(remain, span.Length)
        ()
