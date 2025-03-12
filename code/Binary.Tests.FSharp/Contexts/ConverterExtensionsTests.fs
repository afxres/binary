namespace Contexts

open Mikodev.Binary
open Xunit

type ConverterExtensionsTests() =
    let generator = Generator.CreateDefault()

    static member ``Data Alpha``: (obj array) seq = seq {
        yield [| 1024 |]
        yield [| 512.0 |]
        yield [| "some" |]
        yield [| (10, "ten") |]
        yield [| struct ('A', 47, "K") |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Encode``(value: 'A) =
        let converter = generator.GetConverter<'A>()
        let bytes = generator.Encode(value)

        let alpha = converter.Encode(value)
        Assert.Equal<byte>(bytes, alpha)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Encode No Generic``(value: obj) =
        let converter = generator.GetConverter(value.GetType()) |> box :?> IConverter
        let bytes = generator.Encode(value, value.GetType())

        let alpha = converter.Encode(value)
        Assert.Equal<byte>(bytes, alpha)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Decode``(value: 'A) =
        let converter = generator.GetConverter<'A>()
        let bytes = generator.Encode(value)

        let alpha = converter.Decode(bytes)
        Assert.Equal<'A>(value, alpha)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Decode No Generic``(value: obj) =
        let converter = generator.GetConverter(value.GetType()) |> box :?> IConverter
        let bytes = generator.Encode(value, value.GetType())

        let alpha = converter.Decode(bytes)
        Assert.Equal(value, alpha)
        ()
