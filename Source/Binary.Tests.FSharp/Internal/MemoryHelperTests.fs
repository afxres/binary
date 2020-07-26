namespace Internal

open Mikodev.Binary
open System
open System.Reflection
open System.Runtime.InteropServices
open Xunit

type Ensure<'T> = delegate of item : 'T * swap : bool -> 'T

type Encode<'T> = delegate of location : byref<byte> * item : 'T -> unit

type Decode<'T> = delegate of location : byref<byte> -> 'T

type MemoryHelperTests () =
    member private __.MakeDelegate<'T when 'T :> Delegate> (method : string) =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "MemoryHelper") |> Array.exactlyOne
        let a = t.GetMethods(BindingFlags.Static ||| BindingFlags.NonPublic) |> Array.filter (fun x -> x.Name = method)
        let m =
            match a with
            | [| x |] -> x
            | _ -> a |> Array.filter (fun x -> let p = x.GetParameters() in p.Length > 0 && p.[0].ParameterType = typeof<byte>.MakeByRefType()) |> Array.exactlyOne
        Assert.NotNull m
        let d = Delegate.CreateDelegate(typeof<'T>, m.MakeGenericMethod(typeof<'T>.GetGenericArguments()))
        d :?> 'T

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| int16 0x3389 |]
            yield [| int32 0xB5A7_3EF8 |]
            yield [| int64 0x87A6_E592_66DF_1A36UL |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member me.``Ensure (origin)`` (item : 'a) =
        let f = me.MakeDelegate<Ensure<'a>>("EnsureHandleEndian")
        let result = f.Invoke(item, false)
        Assert.Equal<'a>(item, result)
        ()

    static member ``Data Bravo`` : (obj array) seq =
        seq {
            yield [| int16 0x3CED; int16 0xED3C |]
            yield [| int32 0x2B9C_6E74; int32 0x746E_9C2B |]
            yield [| int64 0x11223344_5566AADDL; int64 0xDDAA6655_44332211UL |]
        }

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member me.``Ensure (invert)`` (item : 'a, invert : 'a) =
        let f = me.MakeDelegate<Ensure<'a>>("EnsureHandleEndian")
        let result = f.Invoke(item, true)
        Assert.Equal<'a>(invert, result)
        ()

    static member ``Data Charlie`` : (obj array) seq =
        seq {
            yield [| byte 0x81 |]
            yield [| sbyte 0x23 |]
            yield [| char 0x6666 |]
            yield [| uint16 0x7887 |]
            yield [| uint32 0xCCDD_AABB |]
            yield [| uint64 0xFFEEDDCC_88775544UL |]
            yield [| single 2.1 |]
            yield [| double 3.4 |]
        }

    [<Theory>]
    [<MemberData("Data Charlie")>]
    member me.``Ensure (invalid type)`` (item : 'a) =
        let f = me.MakeDelegate<Ensure<'a>>("EnsureHandleEndian")
        let alpha = Assert.Throws<NotSupportedException>(fun () -> f.Invoke(item, true) |> ignore)
        let bravo = Assert.Throws<NotSupportedException>(fun () -> f.Invoke(item, false) |> ignore)
        let message = NotSupportedException().Message
        Assert.Equal(message, alpha.Message)
        Assert.Equal(message, bravo.Message)
        ()

    static member ``Data Delta`` : (obj array) seq =
        seq {
            yield [| int16 0xAC96 ; [| 0x96uy; 0xACuy |] |]
            yield [| int32 0x1122_CCDD ; [| 0xDDuy; 0xCCuy; 0x22uy; 0x11uy |] |]
            yield [| int64 0xEFFE_3289_DC5A_7503UL ; [| 0x03uy; 0x75uy; 0x5Auy; 0xDCuy; 0x89uy; 0x32uy; 0xFEuy; 0xEFuy |] |]
        }

    [<Theory>]
    [<MemberData("Data Delta")>]
    member me.``Encode Then Decode (integration test)`` (item : 'a, littleEndian : byte array) =
        let appendLittleEndian = me.MakeDelegate<Encode<'a>>("EncodeLittleEndian")
        let appendNumberEndian = me.MakeDelegate<Encode<'a>>("EncodeNumberEndian")
        let detachLittleEndian = me.MakeDelegate<Decode<'a>>("DecodeLittleEndian")
        let detachNumberEndian = me.MakeDelegate<Decode<'a>>("DecodeNumberEndian")

        let buffer = Array.create 128 0uy
        let span = Span buffer
        let location = &MemoryMarshal.GetReference span

        span.Fill 0uy
        appendLittleEndian.Invoke(&location, item)
        let littleResult = detachLittleEndian.Invoke(&location)
        Assert.Equal<'a>(item, littleResult)
        Assert.Equal<byte>(littleEndian, span.Slice(0, littleEndian.Length).ToArray())

        let numberEndian = Array.rev littleEndian
        span.Fill 0uy
        appendNumberEndian.Invoke(&location, item)
        let numberResult = detachNumberEndian.Invoke(&location)
        Assert.Equal<'a>(item, numberResult)
        Assert.Equal<byte>(numberEndian, span.Slice(0, numberEndian.Length).ToArray())
        ()
