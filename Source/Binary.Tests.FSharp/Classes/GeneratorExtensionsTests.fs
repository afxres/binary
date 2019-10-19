namespace Classes

open Mikodev.Binary
open System
open Xunit

type GeneratorExtensionsTests() =
    let generator = Generator()

    [<Fact>]
    member __.``As Token`` () =
        let buffer = generator.Encode ({| alpha = 1 |})
        let memory = ReadOnlyMemory buffer

        let token = Token(generator, &memory)
        let alpha = Token(generator, buffer)
        Assert.Equal<byte>(buffer, token.AsMemory().ToArray())
        Assert.Equal<byte>(buffer, alpha.AsMemory().ToArray())
        ()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| 256 |]
            yield [| 2.0 |]
            yield [| struct (2L, 'Z') |]
            yield [| ("001", 20, struct (true, Guid.NewGuid())) |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Value No Generic`` (value : obj) =
        let buffer = generator.Encode(value, value.GetType())
        let memory = ReadOnlySpan buffer

        let alpha = generator.Decode(&memory, value.GetType())
        let bravo = generator.Decode(buffer, value.GetType())
        Assert.Equal(value, alpha)
        Assert.Equal(value, bravo)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Value`` (value : 'A) =
        let buffer = generator.Encode value
        let memory = ReadOnlySpan buffer

        let alpha = generator.Decode<'A> &memory
        let bravo = generator.Decode<'A> buffer
        Assert.Equal<'A>(value, alpha)
        Assert.Equal<'A>(value, bravo)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Value With Anonymous`` (value : 'A) =
        let buffer = generator.Encode value
        let memory = ReadOnlySpan buffer

        let alpha = generator.Decode(&memory, anonymous = value)
        let bravo = generator.Decode(buffer, anonymous = value)
        Assert.Equal<'A>(value, alpha)
        Assert.Equal<'A>(value, bravo)
        ()
