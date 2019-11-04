module Classes.AllocatorHelperTests

open Mikodev.Binary
open System
open Xunit

let random = Random()

[<Fact>]
let ``Append (default constructor, length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    AllocatorHelper.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()))
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append (default constructor, length invalid)`` (length : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.Append(&allocator, length, null :> obj, fun a b -> ())
        ())
    let methodInfo = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4) |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.skip 1 |> Array.head
    Assert.StartsWith("Argument length must be greater than or equal to zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    Assert.Equal("length", parameter.Name)
    Assert.Equal(typeof<int>, parameter.ParameterType)
    ()

[<Fact>]
let ``Append (default constructor, overflow)`` () =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.Append(&allocator, Int32.MaxValue + 1, null :> obj, fun a b -> ())
        ())
    Assert.StartsWith("Argument length must be greater than or equal to zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    ()

[<Fact>]
let ``Append (append some then, length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    AllocatorHelper.Append(&allocator, 8, null :> obj, fun a b -> ())
    Assert.Equal(8, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    AllocatorHelper.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()))
    Assert.Equal(8, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append (append some then, length invalid)`` (length : int) =
    let mutable flag = 0
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.Append(&allocator, 8, null :> obj, fun a b -> ())
        flag <- 1
        AllocatorHelper.Append(&allocator, length, null :> obj, fun a b -> ())
        ())
    Assert.Equal(1, flag)
    Assert.StartsWith("Argument length must be greater than or equal to zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    ()

[<Fact>]
let ``Append (limited to zero, length zero with raise expression)`` () =
    let mutable allocator = Allocator(Array.empty, maxCapacity = 0)
    AllocatorHelper.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()))
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Fact>]
let ``Append (one byte 512 times)`` () =
    let mutable allocator = Allocator()
    for item in 1..512 do
        AllocatorHelper.Append(&allocator, 1, null :> obj,
            fun a b ->
                Assert.Null b
                Assert.Equal(1, a.Length))
        Assert.Equal(item, allocator.Length)
        Assert.Equal((if item > 256 then 1024 else 256), allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(256)>]
[<InlineData(257)>]
[<InlineData(1024)>]
let ``Append (default constructor)`` (length : int) =
    let mutable allocator = Allocator()
    let buffer = Array.zeroCreate<byte> length
    random.NextBytes buffer
    AllocatorHelper.Append(&allocator, length, buffer,
        fun a b ->
            b.CopyTo a
            Assert.Equal(length, a.Length))
    let result = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(buffer, result)
    Assert.Equal((if length > 256 then 1024 else 256), allocator.Capacity)
    ()

[<Theory>]
[<InlineData(32)>]
[<InlineData(256)>]
[<InlineData(768)>]
let ``Append (limited, overflow)`` (limits : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator(Array.empty, limits)
        AllocatorHelper.Append(&allocator, limits + 1, null :> obj, fun a b -> ()))
    Assert.Null(error.ParamName)
    Assert.Equal("Maximum allocator capacity has been reached.", error.Message)
    ()

[<Fact>]
let ``Append (limited)`` () =
    let mutable allocator = Allocator(Array.zeroCreate 96, 640)
    AllocatorHelper.Append(&allocator, 192, null :> obj, fun a b -> ())
    Assert.Equal(96 <<< 2, allocator.Capacity)
    AllocatorHelper.Append(&allocator, 448, null :> obj, fun a b -> ())
    Assert.Equal(640, allocator.Length)
    Assert.Equal(640, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(4)>]
let ``Append (default constructor, action null)`` (length : int) =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.Append(&allocator, length, null :> obj, null))
    let methodInfo = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4) |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", error.ParamName)
    Assert.Equal("action", parameter.Name)
    ()

[<Fact>]
let ``Append (default constructor, length zero with action null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.Append(&allocator, 0, null :> obj, null))
    let methodInfo = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4) |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", error.ParamName)
    Assert.Equal("action", parameter.Name)
    ()

[<Fact>]
let ``Append (exception)`` () =
    let mutable allocator = Allocator()
    AllocatorHelper.Append(&allocator, 16, null :> obj, fun a b -> ())
    Assert.Equal(16, allocator.Length)
    try
        AllocatorHelper.Append(&allocator, 32, null :> obj, fun a b -> raise (NotSupportedException()))
    with
    | :? NotSupportedException -> ()
    Assert.Equal(16, allocator.Length)
    ()

[<Fact>]
let ``Append Buffer (empty)`` () =
    let mutable allocator = Allocator()
    AllocatorHelper.Append(&allocator, ReadOnlySpan (Array.empty))
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(256)>]
[<InlineData(512)>]
[<InlineData(1024)>]
let ``Append Buffer (random)`` (length : int) =
    let buffer = Array.zeroCreate<byte> length
    random.NextBytes buffer
    let mutable allocator = Allocator()
    AllocatorHelper.Append(&allocator, ReadOnlySpan buffer)
    Assert.Equal(length, allocator.Length)
    Assert.Equal((if length > 256 then 1024 else 256), allocator.Capacity)
    ()
