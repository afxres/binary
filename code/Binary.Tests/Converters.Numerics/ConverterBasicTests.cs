namespace Mikodev.Binary.Tests.Converters.Numerics;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xunit;

public class ConverterBasicTests
{
    [Fact(DisplayName = "Shared Converters With Known Types")]
    public void SharedConverters()
    {
        var knownTypes = new[]
        {
            typeof(Complex),
            typeof(Matrix3x2),
            typeof(Matrix4x4),
            typeof(Plane),
            typeof(Quaternion),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
        };

        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "DetectEndianConverterCreator");
        var field = ReflectionExtensions.GetFieldNotNull(type, "SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<ImmutableDictionary<Type, (IConverter, IConverter)>>(field.GetValue(null));

        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static Type[] GetFieldTypeOrInternalFieldTypes(FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;
            if (type == typeof(float) || type == typeof(double))
                return new[] { type };
            return type.GetFields(Flags).Select(x => x.FieldType).ToArray();
        }

        foreach (var i in knownTypes)
        {
            var (little, native) = actual[i];
            Assert.Equal("NativeEndianConverter`1", native.GetType().Name);
            Assert.Equal("RepeatLittleEndianConverter`2", little.GetType().Name);
            var fields = i.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(fields);
            Assert.NotEmpty(fields);
            var elementType = fields.SelectMany(GetFieldTypeOrInternalFieldTypes).Distinct().Single();
            var arguments = little.GetType().GetGenericArguments();
            Assert.Equal(2, arguments.Length);
            Assert.Equal(i, arguments[0]);
            Assert.Equal(elementType, arguments[1]);
        }
    }
}
