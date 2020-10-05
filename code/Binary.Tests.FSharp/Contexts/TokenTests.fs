namespace Contexts

open Mikodev.Binary
open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics
open System.Linq
open System.Reflection
open System.Threading
open Xunit

type Packet<'a> = { id : int; data : 'a }

type Person = { name : string; age : int }

type TokenTests() =
    let generator = Generator.CreateDefault()

    [<Fact>]
    member __.``Constructor With Null Generator`` () =
        let bytes = Array.zeroCreate<byte> 0
        let error = Assert.Throws<ArgumentNullException>(fun () -> let memory = ReadOnlyMemory bytes in Token(null, memory) |> ignore)
        Assert.Equal("generator", error.ParamName)
        ()

    [<Fact>]
    member __.``As`` () =
        let source = { id = 1; data = { name = "alice"; age = 20 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(source.id, token.["id"].As<int>())
        Assert.Equal(source.data.name, token.["data"].["name"].As<string>())
        ()

    [<Fact>]
    member __.``As (via type)`` () =
        let source = { id = 2; data = { name = "bob"; age = 22 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(box source.data.name, token.["data"].["name"].As(typeof<string>))
        Assert.Equal(box source.data.age, token.["data"].["age"].As<int>())
        ()

    [<Fact>]
    member __.``As (via type, argument null)`` () =
        let token = Token(generator, ReadOnlyMemory())
        let error = Assert.Throws<ArgumentNullException>(fun () -> token.As null |> ignore)
        Assert.Equal("type", error.ParamName)
        ()

    [<Fact>]
    member __.``Index (match)`` () =
        let source = { id = 5; data = { name = "echo"; age = 24 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let id = token.["id"]
        let name = token.["data"].["name"]
        Assert.Equal(source.id, id.As<int>())
        Assert.Equal(source.data.name, name.As<string>())
        ()

    [<Fact>]
    member __.``Index (nothrow true, match)`` () =
        let source = { id = 5; data = { name = "echo"; age = 24 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let id = token.["id", nothrow = true]
        let name = token.["data"].["name", nothrow = true]
        Assert.Equal(source.id, id.As<int>())
        Assert.Equal(source.data.name, name.As<string>())
        ()

    [<Fact>]
    member __.``Index (nothrow false, match)`` () =
        let source = { id = 5; data = { name = "echo"; age = 24 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let id = token.["id", nothrow = false]
        let name = token.["data"].["name", nothrow = false]
        Assert.Equal(source.id, id.As<int>())
        Assert.Equal(source.data.name, name.As<string>())
        ()

    [<Fact>]
    member __.``Index (mismatch)`` () =
        let source = { id = 6; data = { name = "golf"; age = 18 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(source.data, token.["data"].As<Person>())
        let error = Assert.Throws<KeyNotFoundException>(fun () -> token.["item"] |> ignore)
        Assert.Equal("Key 'item' not found.", error.Message)
        ()

    [<Fact>]
    member __.``Index (nothrow true, mismatch)`` () =
        let source = { id = 6; data = { name = "golf"; age = 18 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(source.data, token.["data"].As<Person>())
        let item = token.["item", nothrow = true]
        Assert.Null(item)
        ()

    [<Fact>]
    member __.``Index (nothrow false, mismatch)`` () =
        let source = { id = 6; data = { name = "golf"; age = 18 }}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        Assert.Equal(source.data, token.["data"].As<Person>())
        let error = Assert.Throws<KeyNotFoundException>(fun () -> token.["empty", nothrow = false] |> ignore)
        Assert.Equal("Key 'empty' not found.", error.Message)
        ()

    [<Fact>]
    member __.``Index (null)`` () =
        let token = Token(generator, ReadOnlyMemory())
        let error = Assert.Throws<ArgumentNullException>(fun () -> token.[null] |> ignore)
        let property = typeof<Token>.GetProperties() |> Array.filter (fun x -> x.GetIndexParameters().Length = 1) |> Array.exactlyOne
        let parameter = property.GetIndexParameters() |> Array.exactlyOne
        Assert.Equal("key", error.ParamName)
        Assert.Equal("key", parameter.Name)
        ()

    [<Fact>]
    member __.``Index (nothrow true, null)`` () =
        let token = Token(generator, ReadOnlyMemory())
        let error = Assert.Throws<ArgumentNullException>(fun () -> token.[null, nothrow = true] |> ignore)
        let property = typeof<Token>.GetProperties() |> Array.filter (fun x -> x.GetIndexParameters().Length = 2) |> Array.exactlyOne
        let parameter = property.GetIndexParameters() |> Array.head
        let last = property.GetIndexParameters() |> Array.last
        Assert.Equal("key", error.ParamName)
        Assert.Equal("key", parameter.Name)
        Assert.Equal("nothrow", last.Name)
        ()

    [<Fact>]
    member __.``Index (nothrow false, null)`` () =
        let token = Token(generator, ReadOnlyMemory())
        let error = Assert.Throws<ArgumentNullException>(fun () -> token.[null, nothrow = false] |> ignore)
        let property = typeof<Token>.GetProperties() |> Array.filter (fun x -> x.GetIndexParameters().Length = 2) |> Array.exactlyOne
        let parameter = property.GetIndexParameters() |> Array.head
        let last = property.GetIndexParameters() |> Array.last
        Assert.Equal("key", error.ParamName)
        Assert.Equal("key", parameter.Name)
        Assert.Equal("nothrow", last.Name)
        ()

    [<Fact>]
    member __.``As Memory`` () =
        let buffer = [| 32uy..128uy |]
        let origin = ReadOnlyMemory<byte>(buffer, 8, 48)
        let source = Token(generator, origin)
        let memory = source.AsMemory()
        Assert.Equal<byte>(origin.ToArray(), memory.ToArray())
        ()

    [<Fact>]
    member __.``Empty Key Only`` () =
        let buffer = generator.Encode (dict [ "", box 1.41 ])
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let dictionary = token :> IReadOnlyDictionary<string, Token>
        Assert.Equal(1, dictionary.Count)
        Assert.Equal(1.41, token.[""].As<double>())
        ()

    [<Fact>]
    member __.``Empty Key`` () =
        let buffer = generator.Encode (dict [ "a", box 'a'; "", box 2048; "data", box "value" ])
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let dictionary = token :> IReadOnlyDictionary<string, Token>
        Assert.Equal(3, dictionary.Count)
        Assert.Equal(2048, token.[""].As<int>())
        ()

    [<Fact>]
    member __.``Equals (not supported)`` () =
        Assert.Throws<NotSupportedException>(fun () -> Token(generator, ReadOnlyMemory()).Equals null |> ignore) |> ignore
        ()

    [<Fact>]
    member __.``Get Hash Code (not supported)`` () =
        Assert.Throws<NotSupportedException>(fun () -> Token(generator, ReadOnlyMemory()).GetHashCode() |> ignore) |> ignore
        ()

    [<Fact>]
    member __.``To String (debug)`` () =
        let token = Token(generator, ReadOnlyMemory())
        Assert.Equal("Token(Items: 0, Bytes: 0)", token.ToString())
        ()

    [<Fact>]
    member __.``Interface Index (mismatch)`` () =
        let token = Token(generator, ReadOnlyMemory())
        let d = token :> IReadOnlyDictionary<string, Token>
        let error = Assert.Throws<KeyNotFoundException>(fun () -> d.[""] |> ignore)
        Assert.Equal("Key '' not found.", error.Message)
        ()

    [<Fact>]
    member __.``Interface (integration, empty)`` () =
        let token = Token(generator, ReadOnlyMemory())
        let d = token :> IReadOnlyDictionary<string, Token>

        Assert.Equal(0, d.Count)
        Assert.False(d.ContainsKey(""))
        let (flag, data) = d.TryGetValue("")
        Assert.False flag
        Assert.Null data
        Assert.NotNull d.Keys
        Assert.Empty d.Keys
        Assert.NotNull d.Values
        Assert.Empty d.Values

        let a = (token :> IEnumerable).GetEnumerator()
        Assert.NotNull a
        Assert.False(a.MoveNext())
        let b = d.GetEnumerator()
        Assert.NotNull b
        Assert.False(b.MoveNext())
        ()

    [<Fact>]
    member __.``Interface (integration, with data)`` () =
        let source = {| id = 1024; data = "sharp" |}
        let buffer = generator.Encode source
        let token = Token(generator, buffer |> ReadOnlyMemory)
        let d = token :> IReadOnlyDictionary<string, Token>

        Assert.Equal(2, d.Count)
        Assert.True(d.ContainsKey("id"))
        Assert.True(d.ContainsKey("data"))
        Assert.False(d.ContainsKey(""))

        let (flag, data) = d.TryGetValue("")
        Assert.False flag
        Assert.Null data

        let (flag, data) = d.TryGetValue("id")
        Assert.True flag
        Assert.NotNull data

        let (flag, data) = d.TryGetValue("data")
        Assert.True flag
        Assert.NotNull data

        Assert.NotNull d.Keys
        Assert.Equal(2, d.Keys |> Seq.length)
        Assert.NotNull d.Values
        Assert.Equal(2, d.Values |> Seq.length)

        let a = (token :> IEnumerable).GetEnumerator()
        Assert.NotNull a
        Assert.True(a.MoveNext())
        Assert.True(a.MoveNext())
        Assert.False(a.MoveNext())
        let b = d.GetEnumerator()
        Assert.NotNull b
        Assert.True(b.MoveNext())
        Assert.True(b.MoveNext())
        Assert.False(b.MoveNext())
        ()

    [<Fact>]
    member __.``Operate Without Valid String Converter`` () =
        let generator = {
            new IGenerator with
                member __.GetConverter t =
                    raise (NotSupportedException(sprintf "Invalid type '%O'" t))
        }
        let error = Assert.Throws<NotSupportedException>(fun () -> Token(generator, ReadOnlyMemory<byte>()) |> ignore)
        let message = sprintf "Invalid type '%O'" typeof<string>
        Assert.Equal(message, error.Message)
        ()

    [<Fact>]
    member __.``Operate With Calling Counter`` () =
        let backup = Generator.CreateDefault()
        let mutable count = 0
        let generator = {
            new IGenerator with
                member __.GetConverter t =
                    if t = typeof<string> then
                        Interlocked.Increment &count |> ignore
                    backup.GetConverter t
        }
        let bytes = backup.Encode ({| alpha = 10; beta = {| data = 20.0 |} |})
        let token = Token(generator, ReadOnlyMemory bytes)
        Assert.Equal(10, token.["alpha"].As<int>())
        Assert.Equal(20.0, token.["beta"].["data"].As<double>())
        Assert.Equal(1, count)
        ()

    [<Fact>]
    member __.``Operate With Null String Converter`` () =
        let generator = {
            new IGenerator with
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
    member __.``Operate With Private Constructor`` () =
        let constructors = typeof<Token>.GetConstructors(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
        Assert.Equal(2, constructors.Length)
        let constructor = Assert.Single(constructors |> Array.filter (fun x -> not x.IsPublic))
        let generator = {
            new IGenerator with
                member __.GetConverter t =
                    raise (NotSupportedException(sprintf "Invalid type '%O'" t))
        }
        let error = Assert.Throws<TargetInvocationException>(fun () -> constructor.Invoke([| box generator; box (ReadOnlyMemory<byte>()); null |]))
        let inner = Assert.IsType<NotSupportedException>(error.InnerException)
        let message = sprintf "Invalid type '%O'" typeof<string>
        Assert.Equal(message, inner.Message)
        ()

    [<Fact>]
    member __.``From Empty Bytes`` () =
        let token = Token(generator, ReadOnlyMemory())
        let dictionary = token :> IReadOnlyDictionary<string, Token>
        Assert.Equal(0, dictionary.Count)
        let tokens = typeof<Token>.GetField("tokens", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue(token) :?> Lazy<Dictionary<string, Token>>
        Assert.Equal(0, tokens.Value.Count)
        let buckets = typeof<Dictionary<string, Token>>.GetFields(BindingFlags.Instance ||| BindingFlags.NonPublic).Single(fun x -> x.FieldType = typeof<int array>).GetValue(tokens.Value) :?> int array
        Assert.Null buckets
        ()

    [<Fact>]
    member __.``From Invalid Bytes`` () =
        let buffer = generator.Encode [ "alpha"; "value"; "empty" ]
        let token = Token(generator, ReadOnlyMemory buffer)
        let dictionary = token :> IReadOnlyDictionary<string, Token>
        Assert.Equal(0, dictionary.Count)
        let tokens = typeof<Token>.GetField("tokens", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue(token) :?> Lazy<Dictionary<string, Token>>
        Assert.Equal(0, tokens.Value.Count)
        let dictionary = tokens.Value;
        let buckets = typeof<Dictionary<string, Token>>.GetFields(BindingFlags.Instance ||| BindingFlags.NonPublic).Single(fun x -> x.FieldType = typeof<int array>).GetValue(dictionary) :?> int array
        Assert.Null buckets
        ()

    [<Fact>]
    member __.``Debugger Proxy (property attribute)`` () =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "TokenDebuggerTypeProxy") |> Array.exactlyOne
        let property = t.GetProperty("Items")
        let attribute = property.GetCustomAttributes() |> Seq.choose (fun x -> match x with | :? DebuggerBrowsableAttribute as a -> Some a | _ -> None) |> Seq.exactlyOne
        Assert.Equal(DebuggerBrowsableState.RootHidden, attribute.State)
        ()

    [<Fact>]
    member __.``Debugger Proxy (token null)`` () =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "TokenDebuggerTypeProxy") |> Array.exactlyOne
        let token = Token(generator, ReadOnlyMemory())
        let proxy = Activator.CreateInstance(t, [| box null |])
        let items = t.GetProperty("Items").GetValue(proxy) :?> KeyValuePair<string, Token>[]
        Assert.Equal(0, items.Length)
        ()

    [<Fact>]
    member __.``Debugger Proxy (token from empty bytes)`` () =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "TokenDebuggerTypeProxy") |> Array.exactlyOne
        let token = Token(generator, ReadOnlyMemory())
        let proxy = Activator.CreateInstance(t, [| box token |])
        let items = t.GetProperty("Items").GetValue(proxy) :?> KeyValuePair<string, Token>[]
        Assert.Equal(0, items.Length)
        ()

    static member ``Anonymous Objects`` : (obj array) seq = seq {
        [| box {| id = 1 |} |]
        [| box {| name = "one"; data = Double.Epsilon |} |]
        [| box {| guid = Guid.NewGuid(); flag = true; context = {| text = "Hello, world" |} |} |]
    }

    [<Theory>]
    [<MemberData("Anonymous Objects")>]
    member __.``Debugger Proxy (token from object)`` (item : obj) =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "TokenDebuggerTypeProxy") |> Array.exactlyOne
        let buffer = generator.Encode item
        let token = Token(generator, ReadOnlyMemory buffer)
        let proxy = Activator.CreateInstance(t, [| box token |])
        let items = t.GetProperty("Items").GetValue(proxy) :?> KeyValuePair<string, Token>[]
        let r = items |> Array.map (|KeyValue|) |> dict
        let d = token :> IReadOnlyDictionary<_, _>
        Assert.Equal(d.Count, items.Length)
        Assert.Equal(d.Count, r.Count)
        Assert.Equal<string>(d.Keys |> HashSet, r.Keys |> HashSet)
        Assert.All(d.Keys, fun x -> Assert.True(obj.ReferenceEquals(d.[x], r.[x])))
        ()
