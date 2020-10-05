module External.CollectionTests

open Mikodev.Binary
open System
open System.Net
open Xunit

let generator =
    Generator.CreateDefaultBuilder()
        .AddFSharpConverterCreators()
        .Build();

let Test (value : 'a when 'a :> 'e seq) =
    let buffer = generator.Encode value
    let result : 'a = generator.Decode buffer
    Assert.Equal<'e seq>(value, result)
    ()

[<Fact>]
let ``Array Instance`` () =
    let alpha = [| 1; 2; 4 |]
    let bravo = [| "one"; "three"; "ten" |]

    Test alpha
    Test bravo
    ()

[<Fact>]
let ``Array (empty)`` () =
    let source : int array = Array.empty
    let buffer = generator.Encode source
    let result : int array = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Equal<int seq>(source, result)
    ()

[<Fact>]
let ``List Instance`` () =
    let alpha = [ 9; 8; 4; 3 ]
    let bravo = [ IPAddress.Loopback; IPAddress.IPv6Loopback ]

    Test alpha
    Test bravo
    ()

[<Fact>]
let ``List (empty)`` () =
    let source : string list = []
    let buffer = generator.Encode source
    let result : string list = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Equal<string seq>(source, result)
    ()

[<Fact>]
let ``List (null)`` () =
    let source = Unchecked.defaultof<int list>
    let buffer = generator.Encode source
    let result : int list = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Empty(result)
    ()

[<Theory>]
[<InlineData(1, 1)>]
[<InlineData(4, 4)>]
[<InlineData(23, 7)>]
let ``List (value type, invalid byte count)`` (bytes : int, remainder : int) =
    let buffer = Array.zeroCreate<byte> bytes
    let converter = generator.GetConverter<List<double>>()
    let otherConverter = generator.GetConverter<double array>()
    let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
    let otherError = Assert.Throws<ArgumentException>(fun () -> otherConverter.Decode buffer |> ignore)
    let message = sprintf "Not enough bytes for collection element, byte length: %d, element type: %O" bytes typeof<double>
    Assert.Null(error.ParamName)
    Assert.Null(otherError.ParamName)
    Assert.Equal(message, error.Message)
    Assert.Equal(message, otherError.Message)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(32768)>]
let ``List (value type, no stack overflow)`` (count : int) =
    let source = Array.zeroCreate<byte> count |> Array.toList
    let buffer = generator.Encode source
    let result = generator.Decode<byte list> buffer
    Assert.Equal<byte>(source, result)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(32768)>]
let ``List (class type, no stack overflow)`` (count : int) =
    let source = seq { for i in 0..(count - 1) do yield sprintf "%d" i } |> Seq.toList
    let buffer = generator.Encode source
    let result = generator.Decode<string list> buffer
    Assert.Equal<string>(source, result)
    ()

[<Fact>]
let ``Sequence`` () =
    let alpha = seq { for i in 3..9 do yield i * 3 }
    let bravo = Seq.empty<string>

    Test alpha
    Test bravo
    ()

[<Fact>]
let ``Set`` () =
    let alpha = [ 2..6 ] |> List.map ((*) 2) |> List.map (sprintf "%d") |> Set
    let bravo = Set.empty<double>

    Test alpha
    Test bravo
    ()

[<Fact>]
let ``Set (null)`` () =
    let source = Unchecked.defaultof<Set<string>>
    let buffer = generator.Encode source
    let result : Set<string> = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Empty(result)
    ()

[<Fact>]
let ``Map`` () =
    let value = [ 1, "one"; 2, "two"; -1, "minus one" ] |> Map
    Test value
    ()

[<Fact>]
let ``Map (null)`` () =
    let source = Unchecked.defaultof<Map<string, int>>
    let buffer = generator.Encode source
    let result : Map<string, int> = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Empty(result)
    ()
