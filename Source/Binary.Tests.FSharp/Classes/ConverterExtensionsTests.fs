namespace Classes

open Mikodev.Binary
open Xunit

type ConverterExtensionsTests() =
    let generator = Generator.CreateDefault()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| 1024 |]
            yield [| 512.0 |]
            yield [| "some" |]
            yield [| (10, "ten") |]
            yield [| struct ('A', 47, "K") |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Bytes`` (value : 'A) =
        let converter = generator.GetConverter<'A>()
        let bytes = generator.Encode(value)

        let alpha = converter.Encode(value)
        Assert.Equal<byte>(bytes, alpha)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Bytes No Generic`` (value : obj) =
        let converter = generator.GetConverter(value.GetType()) |> box :?> IConverter
        let bytes = generator.Encode(value, value.GetType())

        let alpha = converter.Encode(value)
        Assert.Equal<byte>(bytes, alpha)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Value`` (value : 'A) =
        let converter = generator.GetConverter<'A>()
        let bytes = generator.Encode(value)

        let alpha = converter.Decode(bytes)
        Assert.Equal<'A>(value, alpha)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Value No Generic`` (value : obj) =
        let converter = generator.GetConverter(value.GetType()) |> box :?> IConverter
        let bytes = generator.Encode(value, value.GetType())

        let alpha = converter.Decode(bytes)
        Assert.Equal(value, alpha)
        ()
