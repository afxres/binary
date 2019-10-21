module External.CollectionTests

open Mikodev.Binary
open System.Net
open Xunit

let generator = Generator.CreateDefaultBuilder()
                    .AddFSharpConverterCreators()
                    .Build();

let test (value : 'a when 'a :> 'e seq) =
    let buffer = generator.Encode value
    let result : 'a = generator.Decode buffer
    Assert.Equal<'e seq>(value, result)
    ()

[<Fact>]
let ``Array Instance`` () =
    let alpha = [| 1; 2; 4 |]
    let bravo = [| "one"; "three"; "ten" |]

    test alpha
    test bravo
    ()

[<Fact>]
let ``Array (empty)`` () =
    let source : int array = [| |]
    let buffer = generator.Encode source
    let result : int array = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Equal<int seq>(source, result)
    ()

[<Fact>]
let ``List Instance`` () =
    let alpha = [ 9; 8; 4; 3 ]
    let bravo = [ IPAddress.Loopback; IPAddress.IPv6Loopback ]

    test alpha
    test bravo
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
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(32768)>]
let ``List (value type, stack overflow)`` (count : int) =
    let source = Array.zeroCreate<byte> count |> Array.toList
    let buffer = generator.Encode source
    let result = generator.Decode<byte list> buffer
    Assert.Equal<byte>(source, result)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(32768)>]
let ``List (class type, stack overflow)`` (count : int) =
    let source = seq { for i in 0..(count - 1) do yield sprintf "%d" i } |> Seq.toList
    let buffer = generator.Encode source
    let result = generator.Decode<string list> buffer
    Assert.Equal<string>(source, result)
    ()

[<Fact>]
let ``Sequence`` () =
    let alpha = seq { for i in 3..9 do yield i * 3 }
    let bravo = Seq.empty<string>

    test alpha
    test bravo
    ()

[<Fact>]
let ``Set`` () =
    let alpha = [ 2..6 ] |> List.map ((*) 2) |> List.map (sprintf "%d") |> Set
    let bravo = Set.empty<double>

    test alpha
    test bravo

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
    test value
    ()

[<Fact>]
let ``Map (null)`` () =
    let source = Unchecked.defaultof<Map<string, int>>
    let buffer = generator.Encode source
    let result : Map<string, int> = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Empty(result)
    ()
