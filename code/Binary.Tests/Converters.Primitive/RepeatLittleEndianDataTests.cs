namespace Mikodev.Binary.Tests.Converters.Primitive;

using Mikodev.Binary.Tests.Contexts;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xunit;

public class RepeatLittleEndianDataTests
{
    public static IEnumerable<object[]> RangeData()
    {
        yield return [Range.All];
        yield return [Range.StartAt(Index.FromStart(2))];
        yield return [Range.StartAt(Index.FromEnd(3))];
        yield return [Range.EndAt(Index.FromStart(4))];
        yield return [Range.EndAt(Index.FromEnd(5))];
    }

    public static IEnumerable<object[]> ComplexData()
    {
        yield return [Complex.Zero];
        yield return [Complex.NaN];
        yield return [Complex.Infinity];
        yield return [Complex.One];
        yield return [Complex.ImaginaryOne];
        yield return [new Complex(1.1, 2.2)];
    }

    public static IEnumerable<object[]> Matrix3x2Data()
    {
        yield return [Matrix3x2.Identity];
        yield return [new Matrix3x2(1.1F, 1.2F, 2.1F, 2.2F, 3.1F, 3.2F)];
    }

    public static IEnumerable<object[]> Matrix4x4Data()
    {
        yield return [Matrix4x4.Identity];
        yield return [new Matrix4x4(1.1F, 1.2F, 1.3F, 1.4F, 2.1F, 2.2F, 2.3F, 2.4F, 3.1F, 3.2F, 3.3F, 3.4F, 4.1F, 4.2F, 4.3F, 4.4F)];
    }

    public static IEnumerable<object[]> PlaneData()
    {
        yield return [new Plane(1.1F, 2.2F, 3.4F, 4.4F)];
    }

    public static IEnumerable<object[]> QuaternionData()
    {
        yield return [Quaternion.Zero];
        yield return [Quaternion.Identity];
        yield return [new Quaternion(1.1F, 2.2F, 3.3F, 4.4F)];
    }

    public static IEnumerable<object[]> Vector2Data()
    {
        yield return [Vector2.Zero];
        yield return [Vector2.One];
        yield return [Vector2.UnitX];
        yield return [Vector2.UnitY];
        yield return [new Vector2(1.1F, 2.2F)];
    }

    public static IEnumerable<object[]> Vector3Data()
    {
        yield return [Vector3.Zero];
        yield return [Vector3.One];
        yield return [Vector3.UnitX];
        yield return [Vector3.UnitY];
        yield return [Vector3.UnitZ];
        yield return [new Vector3(1.1F, 2.2F, 3.3F)];
    }

    public static IEnumerable<object[]> Vector4Data()
    {
        yield return [Vector4.Zero];
        yield return [Vector4.One];
        yield return [Vector4.UnitX];
        yield return [Vector4.UnitY];
        yield return [Vector4.UnitZ];
        yield return [Vector4.UnitW];
        yield return [new Vector4(1.1F, 2.2F, 3.3F, 4.4F)];
    }

    [Theory(DisplayName = "Encode Decode")]
    [MemberData(nameof(RangeData))]
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
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "LittleEndianConverterCreator");
        var field = ReflectionExtensions.GetFieldNotNull(type, "SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsType<IReadOnlyDictionary<Type, IConverter>>(field.GetValue(null), exactMatch: false);
        var converter = actual[typeof(T)];
        ConverterTests.TestConstantEncodeDecodeMethods((Converter<T>)converter, data);
    }

    [Theory(DisplayName = "Encode Decode Multiple")]
    [MemberData(nameof(RangeData))]
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
