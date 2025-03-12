module Contexts.GeneratorContextTests

open Mikodev.Binary
open System
open Xunit

let Key = "D6D65841-19FC-4F29-AC4D-B852664F8D3E"

type Fake() = class end

type FakeType() = class end

type FakeConverter() =
    inherit Converter<Fake>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : Fake = raise (NotSupportedException())

type FakeConverterCreator() =
    member val Converter = Unchecked.defaultof<IConverter> with get, set

    member val Context = Unchecked.defaultof<IGeneratorContext> with get, set

    interface IConverterCreator with
        member me.GetConverter(context, _) =
            me.Context <- context
            me.Converter <- context.GetConverter typeof<Fake>
            raise (NotSupportedException Key)

[<Fact>]
let ``Generator Context Disposed`` () =
    let creator = FakeConverterCreator()
    let converter = FakeConverter()
    Assert.Null creator.Context
    Assert.Null creator.Context
    let generator = Generator.CreateDefaultBuilder().AddConverter(converter).AddConverterCreator(creator).Build()
    let alpha = Assert.Throws<NotSupportedException>(fun () -> generator.GetConverter<FakeType>() |> ignore)
    Assert.Equal(Key, alpha.Message)
    Assert.True(obj.ReferenceEquals(converter, creator.Converter))
    let context = creator.Context
    Assert.NotNull context
    let bravo = Assert.Throws<InvalidOperationException>(fun () -> context.GetConverter typeof<Fake> |> ignore)
    Assert.Equal("Generator context has been disposed!", bravo.Message)
    ()
