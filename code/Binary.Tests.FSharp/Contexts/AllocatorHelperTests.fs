module Contexts.AllocatorHelperTests

open Mikodev.Binary
open System
open System.Reflection
open System.Runtime.CompilerServices
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
    let methodInfos = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4)
    let parameter = methodInfos |> Array.map (fun x -> x.GetParameters() |> Array.skip 1 |> Array.head) |> Array.filter (fun x -> x.ParameterType = typeof<int>) |> Array.exactlyOne
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    Assert.Equal("length", error.ParamName)
    Assert.Equal("length", parameter.Name)
    ()

[<Fact>]
let ``Append (default constructor, overflow)`` () =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.Append(&allocator, Int32.MaxValue + 1, null :> obj, fun a b -> ())
        ())
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
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
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    Assert.Equal("length", error.ParamName)
    ()

[<Fact>]
let ``Append (limited to zero, length zero with raise expression)`` () =
    let mutable allocator = Allocator(Span(), maxCapacity = 0)
    AllocatorHelper.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()))
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Fact>]
let ``Append (1 byte 512 times, capacity test)`` () =
    let mutable allocator = Allocator()
    for item in 1..512 do
        AllocatorHelper.Append(&allocator, 1, null :> obj,
            fun (a : Span<byte>) b ->
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
        fun (a : Span<byte>) b ->
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
        let mutable allocator = Allocator(Span(), limits)
        AllocatorHelper.Append(&allocator, limits + 1, null :> obj, fun a b -> ()))
    Assert.Null(error.ParamName)
    Assert.Equal("Maximum capacity has been reached.", error.Message)
    ()

[<Fact>]
let ``Append (limited)`` () =
    let mutable allocator = Allocator(Span (Array.zeroCreate 96), 640)
    AllocatorHelper.Append(&allocator, 192, null :> obj, fun a b -> ())
    Assert.Equal(96 <<< 2, allocator.Capacity)
    AllocatorHelper.Append(&allocator, 448, null :> obj, fun a b -> ())
    Assert.Equal(640, allocator.Length)
    Assert.Equal(640, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(4)>]
let ``Append (default constructor, action null)`` (length : int) =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.Append(&allocator, length, null :> obj, null))
    let methodInfos = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[1].ParameterType = typeof<int>) |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", parameter.Name)
    Assert.Equal("action", error.ParamName)
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData("data")>]
let ``Append (with data)`` (data : 'a) =
    let mutable flag : 'a option = None
    let mutable allocator = Allocator()
    AllocatorHelper.Append(&allocator, 1, data, fun a b -> flag <- Some b)
    Assert.Equal<'a>(data, flag |> Option.get)
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

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Anchor (length invalid)`` (length : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        let _ = AllocatorHelper.Anchor(&allocator, length)
        ())
    let methodInfo = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Anchor") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    Assert.Equal("length", error.ParamName)
    Assert.Equal("length", parameter.Name)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(4)>]
let ``Anchor Then Append (action null)`` (length : int) =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        let anchor = AllocatorHelper.Anchor(&allocator, length)
        AllocatorHelper.Append(&allocator, anchor, null :> obj, null)
        ())
    let methodInfos = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[1].ParameterType.Name = "AllocatorAnchor") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", parameter.Name)
    Assert.Equal("action", error.ParamName)
    ()

[<Fact>]
let ``Anchor Then Append (length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    let anchor = AllocatorHelper.Anchor(&allocator, 0)
    Assert.Equal("AllocatorAnchor(Offset: 0, Length: 0)", anchor.ToString())
    AllocatorHelper.Append(&allocator, anchor, null :> obj, fun a b -> raise (NotSupportedException()))
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Fact>]
let ``Anchor Then Append (append some then, length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    AllocatorHelper.Append(&allocator, 8, null :> obj, fun a b -> ())
    let anchor = AllocatorHelper.Anchor(&allocator, 0)
    Assert.Equal("AllocatorAnchor(Offset: 8, Length: 0)", anchor.ToString())
    AllocatorHelper.Append(&allocator, anchor, null :> obj, fun a b -> raise (NotSupportedException()))
    Assert.Equal(8, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(255)>]
[<InlineData(513)>]
[<InlineData(4097)>]
let ``Anchor Then Append`` (length : int) =
    let mutable allocator = Allocator()
    let anchor = AllocatorHelper.Anchor(&allocator, length)
    Assert.Equal(length, allocator.Length)
    Assert.Equal(sprintf "AllocatorAnchor(Offset: 0, Length: %d)" length, anchor.ToString())
    let buffer = Array.zeroCreate<byte> 4
    random.NextBytes buffer
    AllocatorHelper.Append(&allocator, ReadOnlySpan buffer)
    let source = Array.zeroCreate length
    random.NextBytes source
    AllocatorHelper.Append(&allocator, anchor, source, fun (a : Span<byte>) b ->
        Assert.Equal(length, a.Length)
        Assert.True(obj.ReferenceEquals(source, b))
        b.CopyTo a)
    Assert.Equal(length + 4, allocator.Length)
    let result = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(result, Array.concat [ source; buffer ])
    ()

[<Theory>]
[<InlineData(1, 1)>]
[<InlineData(127, 512)>]
[<InlineData(384, 192)>]
[<InlineData(2048, 4096)>]
let ``Anchor Then Append (append some then)`` (prefix : int, length : int) =
    let mutable allocator = Allocator()
    let origin = Array.zeroCreate<byte> prefix
    random.NextBytes origin
    AllocatorHelper.Append(&allocator, ReadOnlySpan origin)
    let anchor = AllocatorHelper.Anchor(&allocator, length)
    Assert.Equal(prefix + length, allocator.Length)
    Assert.Equal(sprintf "AllocatorAnchor(Offset: %d, Length: %d)" prefix length, anchor.ToString())
    let buffer = Array.zeroCreate<byte> 4
    random.NextBytes buffer
    AllocatorHelper.Append(&allocator, ReadOnlySpan buffer)
    let source = Array.zeroCreate length
    random.NextBytes source
    AllocatorHelper.Append(&allocator, anchor, source, fun (a : Span<byte>) b ->
        Assert.Equal(length, a.Length)
        Assert.True(obj.ReferenceEquals(source, b))
        b.CopyTo a)
    Assert.Equal(prefix + length + 4, allocator.Length)
    let result = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(result, Array.concat [ origin; source; buffer ])
    ()

[<Fact>]
let ``Append With Length Prefix (action null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        AllocatorHelper.AppendWithLengthPrefix(&allocator, Array.empty<byte>, null)
        ())
    let methodInfo = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "AppendWithLengthPrefix") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", parameter.Name)
    Assert.Equal("action", error.ParamName)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(256)>]
[<InlineData(65536)>]
let ``Append With Length Prefix`` (length : int) =
    let source = Array.zeroCreate length
    random.NextBytes source
    let mutable allocator = Allocator()
    AllocatorHelper.AppendWithLengthPrefix(&allocator, source, fun a b -> AllocatorHelper.Append(&a, ReadOnlySpan b))
    let mutable span = allocator.AsSpan()
    let result = PrimitiveHelper.DecodeBufferWithLengthPrefix &span
    Assert.True(span.IsEmpty)
    Assert.Equal<byte>(source, result.ToArray())
    ()

[<Fact>]
let ``Invoke Action (contravariant)`` () =
    let t = typedefof<AllocatorAction<_>>
    let parameter = t.GetGenericArguments() |> Array.exactlyOne
    Assert.Equal(GenericParameterAttributes.Contravariant, parameter.GenericParameterAttributes)
    ()

[<Fact>]
let ``Invoke (action null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> AllocatorHelper.Invoke(0, null) |> ignore)
    let methodInfo = typeof<AllocatorHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "Invoke") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", error.ParamName)
    Assert.Equal("action", parameter.Name)
    ()

[<Fact>]
let ``Invoke (empty action)`` () =
    let mutable length = -1;
    let mutable capacity = -1;
    let mutable maxCapacity = -1
    let buffer = AllocatorHelper.Invoke(0, fun allocator _ ->
        length <- allocator.Length
        capacity <- allocator.Capacity
        maxCapacity <- allocator.MaxCapacity)
    Assert.Equal(0, length)
    Assert.Equal(capacity, 65536)
    Assert.Equal(Int32.MaxValue, maxCapacity)
    Assert.NotNull(buffer)
    Assert.Equal(0, buffer.Length)
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData("Hello, 世界")>]
[<InlineData("一二三四五六七八九十")>]
let ``Invoke (encode some string with length prefix)`` (text : string) =
    let buffer = AllocatorHelper.Invoke(text, fun allocator item -> PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, item.AsSpan()))
    let mutable span = ReadOnlySpan<byte>(buffer)
    let result = PrimitiveHelper.DecodeStringWithLengthPrefix &span
    Assert.Equal(text, result)
    Assert.Equal(0, span.Length)
    ()

[<Theory>]
[<InlineData(0, 0, 1)>]
[<InlineData(16, 0, 17)>]
[<InlineData(16, 12, 5)>]
[<InlineData(16, 16, 1)>]
[<InlineData(4096, 1024, 8192)>]
[<InlineData(8192, 5120, 3840)>]
let ``Ensure (invalid)`` (maxCapacity : int, offset : int, length : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator(Span(), maxCapacity)
        AllocatorHelper.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
        Assert.Equal(offset, allocator.Length)
        Assert.Equal(maxCapacity, allocator.MaxCapacity)
        AllocatorHelper.Ensure(&allocator, length))
    let message = "Maximum capacity has been reached."
    Assert.Null error.ParamName
    Assert.Equal(message, error.Message)
    ()

[<Theory>]
[<InlineData(0, 0, 0)>]
[<InlineData(16, 0, 0)>]
[<InlineData(16, 12, 0)>]
[<InlineData(16, 16, 0)>]
[<InlineData(16, 12, 4)>]
[<InlineData(4096, 0, 2048)>]
[<InlineData(4096, 1024, 2048)>]
[<InlineData(4096, 2048, 2048)>]
let ``Ensure (no resize)`` (capacity : int, offset : int, length : int) =
    let mutable allocator = Allocator(Span(Array.zeroCreate capacity), capacity)
    let origin = &Unsafe.AsRef(&allocator.GetPinnableReference())
    AllocatorHelper.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &Unsafe.AsRef(&allocator.GetPinnableReference())))

    AllocatorHelper.Ensure(&allocator, length)
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &Unsafe.AsRef(&allocator.GetPinnableReference())))
    ()

[<Theory>]
[<InlineData(0, 0, 1)>]
[<InlineData(16, 0, 17)>]
[<InlineData(16, 12, 5)>]
[<InlineData(16, 16, 1)>]
[<InlineData(4096, 1024, 8192)>]
[<InlineData(8192, 5120, 3840)>]
let ``Expand (invalid)`` (maxCapacity : int, offset : int, length : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator(Span(), maxCapacity)
        AllocatorHelper.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
        Assert.Equal(offset, allocator.Length)
        Assert.Equal(maxCapacity, allocator.MaxCapacity)
        AllocatorHelper.Expand(&allocator, length))
    let message = "Maximum capacity has been reached."
    Assert.Null error.ParamName
    Assert.Equal(message, error.Message)
    ()

[<Theory>]
[<InlineData(0, 0, 0)>]
[<InlineData(16, 0, 0)>]
[<InlineData(16, 12, 0)>]
[<InlineData(16, 16, 0)>]
[<InlineData(16, 12, 4)>]
[<InlineData(4096, 0, 2048)>]
[<InlineData(4096, 1024, 2048)>]
[<InlineData(4096, 2048, 2048)>]
let ``Expand (no resize)`` (capacity : int, offset : int, length : int) =
    let mutable allocator = Allocator(Span(Array.zeroCreate capacity), capacity)
    let origin = &Unsafe.AsRef(&allocator.GetPinnableReference())
    AllocatorHelper.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &Unsafe.AsRef(&allocator.GetPinnableReference())))

    AllocatorHelper.Expand(&allocator, length)
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset + length, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &Unsafe.AsRef(&allocator.GetPinnableReference())))
    ()
