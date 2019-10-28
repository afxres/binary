module Classes.AllocatorLengthPrefixTests

open Mikodev.Binary
open System
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
        AllocatorHelper.Append(&allocator, &span)
        AllocatorHelper.AppendLengthPrefix(&allocator, anchor)
        let buffer = allocator.ToArray()
        let mutable span = ReadOnlySpan buffer
        let length = PrimitiveHelper.DecodeNumber &span
        Assert.Equal(i, length)
        Assert.Equal<byte>(source, span.ToArray())
        ()

[<Fact>]
let ``Allocator With Default Length Prefix Anchor`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let anchor = Allocator.LengthPrefixAnchor()
        Assert.Equal("LengthPrefixAnchor(Offset: 0)", anchor.ToString())
        let mutable allocator = Allocator()
        AllocatorHelper.AppendLengthPrefix(&allocator, anchor))
    Assert.Equal("Invalid length prefix anchor or allocator modified.", error.Message)
    ()

[<Fact>]
let ``Allocator With Another Length Prefix Anchor`` () =
    for i = 0 to 16 do
        let error = Assert.Throws<ArgumentException>(fun () ->
            let mutable allocatorOld = Allocator()
            let anchor = AllocatorHelper.AnchorLengthPrefix &allocatorOld
            Assert.Equal("LengthPrefixAnchor(Offset: 4)", anchor.ToString())
            let mutable allocator = Allocator (Array.zeroCreate<byte> i)
            AllocatorHelper.AppendLengthPrefix(&allocator, anchor))
        Assert.Equal("Invalid length prefix anchor or allocator modified.", error.Message)
    ()
