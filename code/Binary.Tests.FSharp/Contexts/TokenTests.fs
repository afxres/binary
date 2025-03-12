namespace Contexts

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Dynamic
open System.Linq.Expressions
open System.Reflection
open System.Runtime.CompilerServices
open System.Threading
open Xunit

type Packet<'a> = { id: int; data: 'a }

type Person = { name: string; age: int }

type TokenTests() =
    let generator = Generator.CreateDefault()

    [<Fact>]
    member __.``Constructor With Null Generator``() =
        let bytes = Array.zeroCreate<byte> 0
        let error = Assert.Throws<ArgumentNullException>(fun () -> let memory = ReadOnlyMemory bytes in Token(null, memory) |> ignore)
        Assert.Equal("generator", error.ParamName)
        ()

    [<Fact>]
    member __.``As``() =
        let source = { id = 1; data = { name = "alice"; age = 20 } }
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(source.id, token.["id"].As<int>())
        Assert.Equal(source.data.name, token.["data"].["name"].As<string>())
        ()

    [<Fact>]
    member __.``As (via type)``() =
        let source = { id = 2; data = { name = "bob"; age = 22 } }
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(box source.data.name, token.["data"].["name"].As(typeof<string>))
        Assert.Equal(box source.data.age, token.["data"].["age"].As<int>())
        ()

    [<Fact>]
    member __.``As (via type, argument null)``() =
        let token = Token(generator, ReadOnlyMemory())
        let error = Assert.Throws<ArgumentNullException>(fun () -> token.As null |> ignore)
        Assert.Equal("type", error.ParamName)
        ()

    [<Fact>]
    member __.``Index (match)``() =
        let source = { id = 5; data = { name = "echo"; age = 24 } }
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let id = token.["id"]
        let name = token.["data"].["name"]
        Assert.Equal(source.id, id.As<int>())
        Assert.Equal(source.data.name, name.As<string>())
        ()

    [<Fact>]
    member __.``Index (mismatch)``() =
        let source = { id = 6; data = { name = "golf"; age = 18 } }
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(source.data, token.["data"].As<Person>())
        Assert.Throws<KeyNotFoundException>(fun () -> token.["item"] |> ignore) |> ignore
        ()

    [<Fact>]
    member __.``Index (null)``() =
        let token = Token(generator, ReadOnlyMemory())
        let error = Assert.Throws<ArgumentNullException>(fun () -> token.[null] |> ignore)
        let property = typeof<Token>.GetProperties() |> Array.filter (fun x -> x.GetIndexParameters().Length = 1) |> Array.exactlyOne
        let parameter = property.GetIndexParameters() |> Array.exactlyOne
        Assert.Equal("key", error.ParamName)
        Assert.Equal("key", parameter.Name)
        ()

    [<Fact>]
    member __.``Index Property``() =
        let indexes = typeof<Token>.GetProperties() |> Array.filter (fun x -> x.GetIndexParameters().Length <> 0)
        let indexer = Assert.Single indexes
        Assert.Equal("Item", indexer.Name)
        let method = indexer.GetGetMethod()
        Assert.NotNull method
        Assert.Equal("get_Item", method.Name)
        Assert.Equal<Type>([| typeof<string> |], method.GetParameters() |> Array.map (fun x -> x.ParameterType))
        Assert.Equal(typeof<Token>, method.ReturnType)
        ()

    [<Fact>]
    member __.``Memory``() =
        let buffer = [| 32uy .. 128uy |]
        let origin = ReadOnlyMemory<byte>(buffer, 8, 48)
        let source = Token(generator, origin)
        let memory = source.Memory
        Assert.Equal<byte>(origin.ToArray(), memory.ToArray())
        ()

    [<Fact>]
    member __.``Parent (null)``() =
        let source = {| id = 1 |}
        let buffer = generator.Encode source
        let token = Token(generator, ReadOnlyMemory buffer)
        Assert.Null token.Parent
        ()

    [<Fact>]
    member __.``Parent (children)``() =
        let source = {| id = 1; name = "fsharp" |}
        let buffer = generator.Encode source
        let token = Token(generator, ReadOnlyMemory buffer)
        Assert.Null token.Parent
        let children = token.Children
        Assert.Equal(2, children.Count)
        Assert.All(children.Values, fun x -> Assert.True(obj.ReferenceEquals(token, x.Parent)))
        ()

    [<Fact>]
    member __.``Children``() =
        let source = {| id = 1; name = "fsharp" |}
        let buffer = generator.Encode source
        let token = Token(generator, ReadOnlyMemory buffer)
        Assert.Null token.Parent
        let children = token.Children
        Assert.Equal(2, children.Count)
        Assert.Equal<string>([| "id"; "name" |] |> SortedSet, children.Keys |> SortedSet)
        ()

    [<Fact>]
    member __.``Empty Key Only``() =
        let buffer = generator.Encode(dict [ "", box 1.41 ])
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let dictionary = token.Children
        Assert.Equal(1, dictionary.Count)
        Assert.Equal(1.41, token.[""].As<double>())
        ()

    [<Fact>]
    member __.``Empty Key``() =
        let buffer = generator.Encode(dict [ "a", box 'a'; "", box 2048; "data", box "value" ])
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let dictionary = token.Children
        Assert.Equal(3, dictionary.Count)
        Assert.Equal(2048, token.[""].As<int>())
        ()

    [<Fact>]
    member __.``Equals (reference equals)``() =
        let ga = Generator.CreateDefault()
        let gb = Generator.CreateDefault()
        let a = Token(ga, ReadOnlyMemory())
        let b = Token(ga, ReadOnlyMemory())
        let c = Token(gb, ReadOnlyMemory())
        let d = Token(gb, ReadOnlyMemory())
        Assert.True(a.Equals a)
        Assert.True(b.Equals b)
        Assert.True(c.Equals c)
        Assert.True(d.Equals d)
        Assert.False(a.Equals b)
        Assert.False(b.Equals c)
        Assert.False(c.Equals b)
        Assert.False(d.Equals c)
        ()

    [<Fact>]
    member __.``Get Hash Code (runtime hash code)``() =
        let ga = Generator.CreateDefault()
        let gb = Generator.CreateDefault()
        let a = Token(ga, ReadOnlyMemory())
        let b = Token(ga, ReadOnlyMemory())
        let c = Token(gb, ReadOnlyMemory())
        let d = Token(gb, ReadOnlyMemory())
        Assert.Equal(RuntimeHelpers.GetHashCode a, a.GetHashCode())
        Assert.Equal(RuntimeHelpers.GetHashCode b, b.GetHashCode())
        Assert.Equal(RuntimeHelpers.GetHashCode c, c.GetHashCode())
        Assert.Equal(RuntimeHelpers.GetHashCode d, d.GetHashCode())
        ()

    [<Fact>]
    member __.``To String (debug)``() =
        let token = Token(generator, ReadOnlyMemory())
        Assert.Equal("Child Count = 0, Memory Length = 0", token.ToString())
        ()

    [<Fact>]
    member __.``Operate Without Valid String Converter``() =
        let generator =
            { new IGenerator with
                member __.GetConverter t = raise (NotSupportedException(sprintf "Invalid type '%O'" t))
            }
        let error = Assert.Throws<NotSupportedException>(fun () -> Token(generator, ReadOnlyMemory<byte>()) |> ignore)
        let message = sprintf "Invalid type '%O'" typeof<string>
        Assert.Equal(message, error.Message)
        ()

    [<Fact>]
    member __.``Operate With Calling Counter``() =
        let backup = Generator.CreateDefault()
        let mutable count = 0
        let generator =
            { new IGenerator with
                member __.GetConverter t =
                    if t = typeof<string> then
                        Interlocked.Increment &count |> ignore
                    backup.GetConverter t
            }
        let bytes = backup.Encode({| alpha = 10; beta = {| data = 20.0 |} |})
        let token = Token(generator, ReadOnlyMemory bytes)
        Assert.Equal(10, token.["alpha"].As<int>())
        Assert.Equal(20.0, token.["beta"].["data"].As<double>())
        Assert.Equal(1, count)
        ()

    [<Fact>]
    member __.``Operate With Null String Converter``() =
        let generator =
            { new IGenerator with
                member __.GetConverter t =
                    if t = typeof<string> then
                        null
                    else
                        raise (NotSupportedException(sprintf "Invalid type '%O'" t))
            }
        let error = Assert.Throws<ArgumentException>(fun () -> Token(generator, ReadOnlyMemory<byte>()) |> ignore)
        let message = "No available string converter found."
        Assert.Equal(message, error.Message)
        ()

    [<Fact>]
    member __.``From Empty Bytes``() =
        let token = Token(generator, ReadOnlyMemory())
        let dictionary = token.Children
        Assert.Equal(0, dictionary.Count)
        ()

    [<Fact>]
    member __.``From Invalid Bytes``() =
        let buffer = generator.Encode [ "alpha"; "value"; "empty" ]
        let token = Token(generator, ReadOnlyMemory buffer)
        let dictionary = token.Children
        Assert.Equal(0, dictionary.Count)
        ()

    static member ``Anonymous Objects``: (obj array) seq = seq {
        [| box {| id = 1 |} |]
        [| box {| name = "one"; data = Double.Epsilon |} |]
        [| box {| guid = Guid.NewGuid(); flag = true; context = {| text = "Hello, world" |} |} |]
    }

    [<Theory>]
    [<MemberData("Anonymous Objects")>]
    member __.``Dynamic Member Names``(item: obj) =
        let buffer = generator.Encode item
        let token = Token(generator, ReadOnlyMemory buffer)
        let meta = (token :> IDynamicMetaObjectProvider).GetMetaObject(Expression.Parameter(typeof<Token>))
        let names = meta.GetDynamicMemberNames()
        Assert.Equal<string>(names |> SortedSet, token.Children.Keys |> SortedSet)
        ()

    [<Fact>]
    member __.``Internal Exception Wrapper``() =
        let converter =
            { new Converter<string>() with
                override __.Decode(_: inref<ReadOnlySpan<byte>>) : string = raise (Exception("Decode Test Message!"))

                override __.Encode(_, _) = raise (NotSupportedException())
            }
        let generator =
            { new IGenerator with
                member __.GetConverter t = if t = typeof<string> then converter :> _ else raise (NotSupportedException())
            }
        let token = Token(generator, ReadOnlyMemory(Array.zeroCreate 2))
        let field = typeof<Token>.GetField("create", BindingFlags.Instance ||| BindingFlags.NonPublic)
        let value = field.GetValue(token) |> unbox<Lazy<struct (ImmutableDictionary<string, Token> * Exception)>>
        let struct (dictionary, error) = value.Value
        Assert.Empty dictionary
        Assert.IsType(typeof<Exception>, error)
        Assert.Equal("Decode Test Message!", error.Message)
        ()

    [<Fact>]
    member __.``Internal Exception Wrapper (Empty Bytes)``() =
        let generator = Generator.CreateDefault()
        let token = Token(generator, ReadOnlyMemory Array.empty)
        let field = typeof<Token>.GetField("create", BindingFlags.Instance ||| BindingFlags.NonPublic)
        let value = field.GetValue(token) |> unbox<Lazy<struct (ImmutableDictionary<string, Token> * Exception)>>
        let struct (dictionary, error) = value.Value
        Assert.Empty dictionary
        Assert.Null error
        ()

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("a")>]
    [<InlineData("bravo")>]
    member __.``Key Not Found Empty Bytes``(key: string) =
        let generator = Generator.CreateDefault()
        let token = Token(generator, ReadOnlyMemory Array.empty)
        let error = Assert.Throws<KeyNotFoundException>(fun () -> token.[key] |> ignore)
        Assert.Empty token.Children
        Assert.Equal($"Key '{key}' not found.", error.Message)
        Assert.Null error.InnerException
        ()

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("kilo")>]
    [<InlineData("Mega")>]
    member __.``Key Not Found Valid Bytes``(key: string) =
        let generator = Generator.CreateDefault()
        let buffer = generator.Encode({| first = 1.0; second = "Next" |})
        let token = Token(generator, ReadOnlyMemory buffer)
        let error = Assert.Throws<KeyNotFoundException>(fun () -> token.[key] |> ignore)
        Assert.Equal(2, token.Children.Count)
        Assert.Equal($"Key '{key}' not found.", error.Message)
        Assert.Null error.InnerException
        ()

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("A")>]
    [<InlineData("Beta")>]
    member __.``Key Not Found Invalid Bytes``(key: string) =
        let generator = Generator.CreateDefault()
        let token = Token(generator, ReadOnlyMemory [| 0uy |])
        let error = Assert.Throws<KeyNotFoundException>(fun () -> token.[key] |> ignore)
        let inner = Assert.IsType<ArgumentException> error.InnerException
        Assert.Empty token.Children
        Assert.Equal($"Key '{key}' not found.", error.Message)
        Assert.Equal("Not enough bytes or byte sequence invalid.", inner.Message)
        ()

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("α")>]
    [<InlineData("二")>]
    member __.``Key Not Found Converter Exception``(key: string) =
        let converter =
            { new Converter<string>() with
                override __.Decode(_: inref<ReadOnlySpan<byte>>) : string = raise (Exception("Not a good idea!"))

                override __.Encode(_, _) = raise (NotSupportedException())
            }
        let generator = Generator.CreateDefaultBuilder().AddConverter(converter).Build()
        let token = Token(generator, ReadOnlyMemory [| 0uy; 0uy |])
        let error = Assert.Throws<KeyNotFoundException>(fun () -> token.[key] |> ignore)
        let inner = Assert.IsType<Exception> error.InnerException
        Assert.Empty token.Children
        Assert.Equal($"Key '{key}' not found.", error.Message)
        Assert.Equal("Not a good idea!", inner.Message)
        ()
