namespace Mikodev.Binary.Tests.Converters.Numerics;

using Mikodev.Binary.Tests.Contexts;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

public class ConverterIntegrationTests
{
    public static IEnumerable<object[]> ComplexData()
    {
        yield return new object[] { Complex.Zero };
        yield return new object[] { Complex.NaN };
        yield return new object[] { Complex.Infinity };
        yield return new object[] { Complex.One };
        yield return new object[] { Complex.ImaginaryOne };
        yield return new object[] { new Complex(1.1, 2.2) };
    }

    public static IEnumerable<object[]> Matrix3x2Data()
    {
        yield return new object[] { Matrix3x2.Identity };
        yield return new object[] { new Matrix3x2(1.1F, 1.2F, 2.1F, 2.2F, 3.1F, 3.2F) };
    }

    public static IEnumerable<object[]> Matrix4x4Data()
    {
        yield return new object[] { Matrix4x4.Identity };
        yield return new object[] { new Matrix4x4(1.1F, 1.2F, 1.3F, 1.4F, 2.1F, 2.2F, 2.3F, 2.4F, 3.1F, 3.2F, 3.3F, 3.4F, 4.1F, 4.2F, 4.3F, 4.4F) };
    }

    public static IEnumerable<object[]> PlaneData()
    {
        yield return new object[] { new Plane(1.1F, 2.2F, 3.4F, 4.4F) };
    }

    public static IEnumerable<object[]> QuaternionData()
    {
        yield return new object[] { Quaternion.Zero };
        yield return new object[] { Quaternion.Identity };
        yield return new object[] { new Quaternion(1.1F, 2.2F, 3.3F, 4.4F) };
    }

    public static IEnumerable<object[]> Vector2Data()
    {
        yield return new object[] { Vector2.Zero };
        yield return new object[] { Vector2.One };
        yield return new object[] { Vector2.UnitX };
        yield return new object[] { Vector2.UnitY };
        yield return new object[] { new Vector2(1.1F, 2.2F) };
    }

    public static IEnumerable<object[]> Vector3Data()
    {
        yield return new object[] { Vector3.Zero };
        yield return new object[] { Vector3.One };
        yield return new object[] { Vector3.UnitX };
        yield return new object[] { Vector3.UnitY };
        yield return new object[] { Vector3.UnitZ };
        yield return new object[] { new Vector3(1.1F, 2.2F, 3.3F) };
    }

    public static IEnumerable<object[]> Vector4Data()
    {
        yield return new object[] { Vector4.Zero };
        yield return new object[] { Vector4.One };
        yield return new object[] { Vector4.UnitX };
        yield return new object[] { Vector4.UnitY };
        yield return new object[] { Vector4.UnitZ };
        yield return new object[] { Vector4.UnitW };
        yield return new object[] { new Vector4(1.1F, 2.2F, 3.3F, 4.4F) };
    }

    [Theory(DisplayName = "Encode Decode")]
    [MemberData(nameof(ComplexData))]
    [MemberData(nameof(Matrix3x2Data))]
    [MemberData(nameof(Matrix4x4Data))]
    [MemberData(nameof(PlaneData))]
    [MemberData(nameof(QuaternionData))]
    [MemberData(nameof(Vector2Data))]
    [MemberData(nameof(Vector3Data))]
    [MemberData(nameof(Vector4Data))]
    public void EncodeDecode<T>(T data) where T : unmanaged
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "DetectEndianConverterCreator");
        var field = ReflectionExtensions.GetFieldNotNull(type, "SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<IReadOnlyDictionary<Type, (IConverter, IConverter)>>(field.GetValue(null));

        var (little, native) = actual[typeof(T)];
        var a = little.Encode(data);
        var b = native.Encode(data);
        Assert.Equal(a, b);

        ConverterTests.TestConstantEncodeDecodeMethods((Converter<T>)little, data);
        ConverterTests.TestConstantEncodeDecodeMethods((Converter<T>)native, data);
    }

    [Theory(DisplayName = "Encode Decode Multiple")]
    [MemberData(nameof(ComplexData))]
    [MemberData(nameof(Matrix3x2Data))]
    [MemberData(nameof(Matrix4x4Data))]
    [MemberData(nameof(PlaneData))]
    [MemberData(nameof(QuaternionData))]
    [MemberData(nameof(Vector2Data))]
    [MemberData(nameof(Vector3Data))]
    [MemberData(nameof(Vector4Data))]
    public void EncodeDecodeMultiple<T>(T data) where T : unmanaged
    {
        var array = Enumerable.Repeat(data, 100).ToArray();
        var linkedList = new LinkedList<T>(array);
        var set = new HashSet<T>(array);

        var generator = Generator.CreateDefault();
        var ca = generator.GetConverter<T[]>();
        var cl = generator.GetConverter<LinkedList<T>>();
        var cs = generator.GetConverter<HashSet<T>>();

        var ba = ca.Encode(array);
        var bl = cl.Encode(linkedList);
        var bs = cs.Encode(set);

        var span = new ReadOnlySpan<T>(array);
        var bytes = MemoryMarshal.AsBytes(span);
        var buffer = bytes.ToArray();
        Assert.Equal(buffer, ba);
        Assert.Equal(buffer, bl);

        var ra = ca.Decode(ba);
        var rl = cl.Decode(bl);
        var rs = cs.Decode(bs);

        Assert.Equal(array, ra);
        Assert.Equal(array, rl);
        Assert.Equal(set, rs);

        Assert.Equal(100, ra.Length);
        Assert.Equal(100, rl.Count);
        _ = Assert.Single(rs);
    }
}
