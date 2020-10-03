module Implementations.InheritanceTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

[<Interface>]
type IPerson =
    abstract member Id : unit -> Guid with get

    abstract member Name : unit -> string with get

type Person(id, name) =
    interface IPerson with
        member __.Id = id

        member __.Name = name

[<AbstractClass>]
type Book () =
    abstract member Name : unit -> string with get

    abstract member Pages : unit -> int with get

type SomeBook(name, pages, price) =
    inherit Book ()

    override __.Name = name

    override __.Pages = pages

    member __.Price : decimal = price

type MiscBook(count, name, pages, price) =
    inherit SomeBook(name, pages, price)

    member __.Count : int = count

let generator = Generator.CreateDefault()

[<Fact>]
let ``Interface`` () =
    let a = new Person(Guid.NewGuid(), "Tom") :> IPerson
    let bytes = generator.Encode a
    Assert.NotEmpty bytes
    let token = Token(generator, bytes |> ReadOnlyMemory)
    let dictionary = token :> IReadOnlyDictionary<string, Token>
    Assert.Equal(2, dictionary.Count)
    let id = token.["Id"].As<Guid>()
    let name = token.["Name"].As<string>()
    Assert.Equal(a.Id, id)
    Assert.Equal(a.Name, name)
    ()

[<Fact>]
let ``Interface Decode`` () =
    let a = new Person(Guid.NewGuid(), "Bob") :> IPerson
    let bytes = generator.Encode a
    let error = Assert.Throws<NotSupportedException>(fun () -> generator.Decode<IPerson> bytes |> ignore)
    Assert.Equal(sprintf "No suitable constructor found, type: %O" typeof<IPerson>, error.Message)
    ()

[<Fact>]
let ``Abstract Class`` () =
    let a = new SomeBook("C# To F# ...", 1024, decimal 54.3) :> Book
    let bytes = generator.Encode a
    Assert.NotEmpty bytes
    let token = Token(generator, bytes |> ReadOnlyMemory)
    let dictionary = token :> IReadOnlyDictionary<string, Token>
    Assert.Equal(2, dictionary.Count)
    Assert.False(dictionary.ContainsKey("Price"))
    let name = dictionary.["Name"].As<string>()
    let pages = dictionary.["Pages"].As<int>()
    Assert.Equal(a.Name, name)
    Assert.Equal(a.Pages, pages)
    ()

[<Fact>]
let ``Abstract Class Decode`` () =
    let a = new SomeBook("C# To F# ...", 1024, decimal 54.3) :> Book
    let bytes = generator.Encode a
    let error = Assert.Throws<NotSupportedException>(fun () -> generator.Decode<Book> bytes |> ignore)
    Assert.Equal(sprintf "No suitable constructor found, type: %O" typeof<Book>, error.Message)
    ()

[<Fact>]
let ``Sub Bytes To Base Value`` () =
    let a = new MiscBook(321, "ABC ...", 987, decimal 6.54)
    let bytes = generator.Encode a
    Assert.NotEmpty bytes
    let value = generator.Decode<SomeBook> bytes
    Assert.Equal(a.Name, value.Name)
    Assert.Equal(a.Pages, value.Pages)
    Assert.Equal(a.Price, value.Price)
    let dictionary = Token(generator, bytes |> ReadOnlyMemory) :> IReadOnlyDictionary<string, Token>
    Assert.Equal(4, dictionary.Count)
    let count = dictionary.["Count"].As<int>()
    Assert.Equal(a.Count, count)
    ()

[<Fact>]
let ``Base Bytes To Sub Value`` () =
    let a = new SomeBook("Overflow", 357, decimal 26.8)
    let bytes = generator.Encode a
    Assert.NotEmpty bytes
    let error = Assert.Throws<ArgumentException>(fun () -> generator.Decode<MiscBook> bytes |> ignore)
    let message = sprintf "Named key '%s' does not exist, type: %O" "Count" typeof<MiscBook>
    Assert.Equal(message, error.Message)
    let dictionary = Token(generator, bytes |> ReadOnlyMemory) :> IReadOnlyDictionary<string, Token>
    Assert.Equal(3, dictionary.Count)
    ()
