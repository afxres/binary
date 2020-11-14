namespace Converters

open Mikodev.Binary
open System
open System.Net
open System.Net.Sockets
open Xunit

type IPEndPointTests() =
    let GetConverterType() = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "IPEndPointConverter") |> Array.exactlyOne

    let GetConverter() = GetConverterType() |> Activator.CreateInstance :?> Converter<IPEndPoint>

    static member ``Data Alpha`` : (obj array) seq = seq {
        yield [| null |]
        yield [| IPEndPoint(IPAddress.Parse("255.255.255.255"), 0) |]
        yield [| IPEndPoint(IPAddress.Parse("0.0.0.0"), 65535) |]
        yield [| IPEndPoint(IPAddress.Parse("fe80::1"), 48000) |]
        yield [| IPEndPoint(IPAddress.Parse("fd78:b85b:02ee:4608:9720:c613:0e11:3371"), 8080) |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Encode Then Decode (span methods & bytes methods)`` (item : IPEndPoint) =
        let converter = GetConverter()
        let b1 = Allocator.Invoke((), fun allocator _ -> converter.Encode(&allocator, item))
        let b2 = converter.Encode item
        Assert.Equal<byte>(b1, b2)

        let byteLength =
            match item with
            | null -> 0
            | x when x.AddressFamily = AddressFamily.InterNetwork -> 6
            | x when x.AddressFamily = AddressFamily.InterNetworkV6 -> 18
            | _ -> raise (NotSupportedException())
        Assert.Equal(byteLength, b1.Length)
        Assert.Equal(byteLength, b2.Length)

        let b1 = ReadOnlySpan b1
        let r1 = converter.Decode &b1
        let r2 = converter.Decode b2
        Assert.Equal(item, r1)
        Assert.Equal(item, r2)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Encode Then Decode (auto methods & length prefix methods)`` (item : IPEndPoint) =
        let converter = GetConverter()
        let b1 = Allocator.Invoke((), fun allocator _ -> converter.EncodeAuto(&allocator, item))
        let b2 = Allocator.Invoke((), fun allocator _ -> converter.EncodeWithLengthPrefix(&allocator, item))
        Assert.Equal<byte>(b1, b2)

        let byteLength =
            match item with
            | null -> 1
            | x when x.AddressFamily = AddressFamily.InterNetwork -> 7
            | x when x.AddressFamily = AddressFamily.InterNetworkV6 -> 19
            | _ -> raise (NotSupportedException())
        Assert.Equal(byteLength, b1.Length)
        Assert.Equal(byteLength, b2.Length)

        let mutable b1 = ReadOnlySpan b1
        let mutable b2 = ReadOnlySpan b2
        let r1 = converter.DecodeAuto &b1
        let r2 = converter.DecodeWithLengthPrefix &b2
        Assert.Equal(item, r1)
        Assert.Equal(item, r2)
        Assert.True b1.IsEmpty
        Assert.True b2.IsEmpty
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Layout`` (item : IPEndPoint) =
        let converter = GetConverter()
        let b1 = converter.Encode item
        let b2 =
            if isNull item then
                Array.empty<byte>
            else
                let generator = Generator.CreateDefault()
                let mutable allocator = Allocator()
                generator.GetConverter<IPAddress>().Encode(&allocator, item.Address)
                generator.GetConverter<uint16>().Encode(&allocator, item.Port |> uint16)
                allocator.AsSpan().ToArray()
        Assert.Equal<byte>(b1, b2)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Layout (with length prefix)`` (item : IPEndPoint) =
        let converter = GetConverter()
        let b1 = Allocator.Invoke((), fun allocator _ -> converter.EncodeWithLengthPrefix(&allocator, item))
        let b2 =
            if isNull item then
                [| 0uy |]
            else
                let byteLength =
                    match item with
                    | null -> 0
                    | x when x.AddressFamily = AddressFamily.InterNetwork -> 6
                    | x when x.AddressFamily = AddressFamily.InterNetworkV6 -> 18
                    | _ -> raise (NotSupportedException())
                let generator = Generator.CreateDefault()
                let mutable allocator = Allocator()
                PrimitiveHelper.EncodeNumber(&allocator, byteLength)
                generator.GetConverter<IPAddress>().Encode(&allocator, item.Address)
                generator.GetConverter<uint16>().Encode(&allocator, item.Port |> uint16)
                allocator.AsSpan().ToArray()
        Assert.Equal<byte>(b1, b2)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Decode With Multiple Length Prefix (auto methods & length prefix methods)`` (item : IPEndPoint) =
        let converter = GetConverter()
        let buffer = converter.Encode item
        let byteLength =
            match item with
            | null -> 0
            | x when x.AddressFamily = AddressFamily.InterNetwork -> 6
            | x when x.AddressFamily = AddressFamily.InterNetworkV6 -> 18
            | _ -> raise (NotSupportedException())

        let b1 = Array.concat [| [| byte byteLength |]; buffer |]
        let b4 = Array.concat [| [| 0x80uy; 0x00uy; 0x00uy; byte byteLength |]; buffer |]

        let DecodeAuto (buffer : byte array) =
            let mutable span = ReadOnlySpan buffer
            let result = converter.DecodeAuto &span
            Assert.True span.IsEmpty
            result

        let DecodeWithLengthPrefix (buffer : byte array) =
            let mutable span = ReadOnlySpan buffer
            let result = converter.DecodeWithLengthPrefix &span
            Assert.True span.IsEmpty
            result

        let alpha = [| b1; b4 |] |> Array.map DecodeAuto
        let bravo = [| b1; b4 |] |> Array.map DecodeWithLengthPrefix
        let value = Array.concat [| alpha; bravo |] |> Array.distinct |> Assert.Single
        Assert.Equal(item, value)
        ()

    [<Fact>]
    member __.``Not Enough Bytes (length 1)`` () =
        let converter = GetConverter()
        let buffer = Array.zeroCreate<byte> 1
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.Equal(message, error.Message)
        ()

    [<Fact>]
    member __.``Not Enough Bytes (length from 2 to max)`` () =
        let converter = GetConverter()
        let parameters = [ for i = 2 to 63 do if i <> 6 && i <> 18 then yield i ]
        Assert.Equal(2, parameters |> List.head)
        Assert.Equal(63, parameters |> List.last)
        Assert.Equal(60, parameters |> List.length)
        for i in parameters do
            let buffer = Array.zeroCreate<byte> i
            let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
            let expected = Assert.Throws<ArgumentException>(fun () -> IPAddress(Array.empty) |> ignore)
            Assert.Equal(expected.Message, error.Message)
            Assert.Equal(expected.ParamName, error.ParamName)
            ()
        ()

    [<Fact>]
    member __.``Port (no overflow)`` () =
        let converter = GetConverter()
        let source = IPEndPoint(IPAddress.Any, IPEndPoint.MaxPort)
        let buffer = converter.Encode source
        let result = converter.Decode buffer
        Assert.Equal(IPEndPoint.MaxPort, result.Port)
        ()
