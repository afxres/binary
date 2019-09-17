namespace Classes

open Mikodev.Binary
open System
open Xunit

type GeneratorExtensionsTests() =
    let generator = Generator()

    [<Fact>]
    member __.``As Token`` () =
        let buffer = generator.ToBytes ({| alpha = 1 |})
        let memory = ReadOnlyMemory buffer   
        
        let token = generator.AsToken &memory
        let alpha = generator.AsToken buffer
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
        let buffer = generator.ToBytes(value, value.GetType())
        let memory = ReadOnlySpan buffer
        
        let alpha = generator.ToValue(&memory, value.GetType())
        let bravo = generator.ToValue(buffer, value.GetType())
        Assert.Equal(value, alpha)
        Assert.Equal(value, bravo)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Value`` (value : 'A) =
        let buffer = generator.ToBytes value
        let memory = ReadOnlySpan buffer

        let alpha = generator.ToValue<'A> &memory
        let bravo = generator.ToValue<'A> buffer
        Assert.Equal<'A>(value, alpha)
        Assert.Equal<'A>(value, bravo)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To Value With Anonymous`` (value : 'A) =
        let buffer = generator.ToBytes value
        let memory = ReadOnlySpan buffer

        let alpha = generator.ToValue(&memory, anonymous = value)
        let bravo = generator.ToValue(buffer, anonymous = value)
        Assert.Equal<'A>(value, alpha)
        Assert.Equal<'A>(value, bravo)
        ()
