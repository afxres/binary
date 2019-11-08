module Classes.AllocatorAnchorTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

[<Fact>]
let ``Length Prefix Buffer With Length From 0 To 4096`` () =
    let random = Random()
    for i = 0 to 4096 do
        let source = Array.zeroCreate<byte> i
        random.NextBytes source
        let mutable allocator = Allocator()
        let span = ReadOnlySpan source
        let anchor = AllocatorHelper.AnchorLengthPrefix &allocator
        AllocatorHelper.Append(&allocator, span)
        AllocatorHelper.AppendLengthPrefix(&allocator, anchor)
        let buffer = allocator.AsSpan().ToArray()
        let mutable span = ReadOnlySpan buffer
        let length = PrimitiveHelper.DecodeNumber &span
        Assert.Equal(i, length)
        Assert.Equal<byte>(source, span.ToArray())
        ()

[<Fact>]
let ``Allocator With Default Length Prefix Anchor`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let anchor = AllocatorAnchor()
        Assert.Equal("AllocatorAnchor(Offset: 0, Length: 0)", anchor.ToString())
        let mutable allocator = Allocator()
        AllocatorHelper.AppendLengthPrefix(&allocator, anchor))
    Assert.Equal("Invalid allocator anchor for length prefix.", error.Message)
    ()

[<Fact>]
let ``Allocator With Another Length Prefix Anchor`` () =
    for i = 0 to 16 do
        let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
            let mutable allocatorOld = Allocator()
            let anchor = AllocatorHelper.AnchorLengthPrefix &allocatorOld
            Assert.Equal("AllocatorAnchor(Offset: 0, Length: 4)", anchor.ToString())
            let mutable allocator = Allocator (Span (Array.zeroCreate<byte> i))
            AllocatorHelper.AppendLengthPrefix(&allocator, anchor))
        Assert.Contains("Specified argument was out of the range of valid values.", error.Message)
    ()

[<Fact>]
let ``Public Members`` () =
    let t = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "AllocatorAnchor") |> Array.exactlyOne
    let constructors = t.GetConstructors()
    let properties = t.GetProperties()
    let members = t.GetMembers()
    Assert.Empty(constructors)
    Assert.Empty(properties)
    Assert.Equal<string>(members |> Seq.map (fun x -> x.Name) |> HashSet, [| "Equals"; "GetHashCode"; "ToString"; "GetType" |] |> HashSet)
    ()
