namespace Attributes

open Mikodev.Binary
open Mikodev.Binary.Attributes
open System
open Xunit

[<AttributeUsage(AttributeTargets.All, AllowMultiple = true)>]
type PlaceholderAttribute() =
    inherit Attribute()

[<Class>]
[<Placeholder>]
type BadClassIndexerWithNamedKey() =
    [<Placeholder>]
    [<NamedKey("Indexer???")>]
    member __.Some
        with get (_: int) = String.Empty
        and set _ _ = ()

[<Placeholder>]
[<Struct>]
type BadValueIndexerWithTupleKey =
    [<TupleKey(1)>]
    [<Placeholder>]
    member __.Just
        with set (_: single) (_: double) = ()

[<Class>]
[<Placeholder>]
type BadClassIndexerWithConverter() =
    [<Placeholder>]
    [<Converter(null)>]
    member __.Maybe
        with get (_: string) = 0

[<Placeholder>]
[<Struct>]
type BadValueIndexerWithConverterCreator =
    [<ConverterCreator(null)>]
    [<Placeholder>]
    member __.None
        with get (_: single) = double 0
        and set _ _ = ()

[<NamedObject>]
[<TupleObject>]
[<Converter(null)>]
[<ConverterCreator(null)>]
type BadClassWithMultipleAttributesOnAllElements() =
    [<NamedKey("5")>]
    [<TupleKey(-87)>]
    [<Converter(null)>]
    [<ConverterCreator(null)>]
    member __.``Set Only Property``
        with set (_: Guid) = ()

    [<NamedKey("2")>]
    [<TupleKey(-10)>]
    [<Converter(null)>]
    [<ConverterCreator(null)>]
    member __.Item
        with set (_: single) (_: double) = ()

[<Class>]
[<Placeholder>]
[<NamedObject>]
type ClassAsNamedObjectWithSetOnlyIndexer(alpha: double, text: string) =
    [<Placeholder>]
    [<NamedKey("A l p h a")>]
    member __.Alpha = alpha

    [<Placeholder>]
    member __.``Index What``
        with set (_: int) (_: obj) = ()

    [<NamedKey("String")>]
    [<Placeholder>]
    member __.Text = text

    override __.Equals a =
        match a with
        | :? ClassAsNamedObjectWithSetOnlyIndexer as b -> b.Alpha = alpha && b.Text = text
        | _ -> false

    override __.GetHashCode() = alpha.GetHashCode() ^^^ text.GetHashCode()

[<Struct>]
[<TupleObject>]
[<Placeholder>]
[<StructuralEquality>]
[<StructuralComparison>]
type ValueAsTupleObjectWithIndexer =
    val mutable private b: byte

    val mutable private t: string

    [<Placeholder>]
    member __.``This is an indexer``
        with get (_: string) = obj ()
        and set _ _ = raise (NotSupportedException())

    [<TupleKey(1)>]
    [<Placeholder>]
    member me.Bravo
        with get () = me.b
        and set x = me.b <- x

    [<Placeholder>]
    [<TupleKey(0)>]
    member me.Delta
        with get () = me.t
        and set x = me.t <- x

type AttributeWithIndexerTests() =
    let generator = Generator.CreateDefault()

    static member ``Data Indexer Invalid`` = [|
        [| typeof<BadClassIndexerWithNamedKey>; typeof<NamedKeyAttribute> |]
        [| typeof<BadValueIndexerWithTupleKey>; typeof<TupleKeyAttribute> |]
        [| typeof<BadClassIndexerWithConverter>; typeof<ConverterAttribute> |]
        [| typeof<BadValueIndexerWithConverterCreator>; typeof<ConverterCreatorAttribute> |]
    |]

    [<Theory>]
    [<MemberData("Data Indexer Invalid")>]
    member __.``Indexer With Attribute Invalid``(t: Type, attribute: Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
        let message = sprintf "Can not apply '%s' to an indexer, type: %O" attribute.Name t
        Assert.Null error.ParamName
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Multiple Integration`` = [| [| typeof<BadClassWithMultipleAttributesOnAllElements> |] |]

    [<Theory>]
    [<MemberData("Data Multiple Integration")>]
    member __.``Multiple Attributes Integration Test``(t: Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
        let message = sprintf "Multiple attributes found, type: %O" t
        Assert.Null error.ParamName
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Type With Indexer`` = [|
        [| box (ClassAsNamedObjectWithSetOnlyIndexer(2.3, "233")) |]
        [| box (ValueAsTupleObjectWithIndexer(Bravo = 254uy, Delta = "Δ")) |]
    |]

    [<Theory>]
    [<MemberData("Data Type With Indexer")>]
    member __.``Type With Indexer``(source: 'T) =
        let converter = generator.GetConverter<'T>()
        let buffer = converter.Encode source
        let result = converter.Decode buffer
        Assert.Equal<'T>(source, result)
        ()
