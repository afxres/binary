namespace Mikodev.Binary.Tests.Converters.Primitive;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xunit;

public class RepeatLittleEndianInfoTests
{
    [Fact(DisplayName = "Shared Converters With Known Types")]
    public void SharedConverters()
    {
        var knownTypes = new[]
        {
            typeof(Range),
            typeof(Complex),
            typeof(Matrix3x2),
            typeof(Matrix4x4),
            typeof(Plane),
            typeof(Quaternion),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
        };

        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "LittleEndianConverterCreator");
        var field = ReflectionExtensions.GetFieldNotNull(type, "SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsType<IReadOnlyDictionary<Type, IConverter>>(field.GetValue(null), exactMatch: false);

        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static Type[] GetFieldTypeOrInternalFieldTypes(FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;
            if (type == typeof(float) || type == typeof(double))
                return [type];
            return [.. type.GetFields(Flags).Select(x => x.FieldType)];
        }

        foreach (var i in knownTypes)
        {
            var converter = actual[i];
            Assert.Equal("RepeatLittleEndianConverter`2", converter.GetType().Name);
            var fields = i.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(fields);
            Assert.NotEmpty(fields);
            var elementType = fields.SelectMany(GetFieldTypeOrInternalFieldTypes).Distinct().Single();
            var arguments = converter.GetType().GetGenericArguments();
            Assert.Equal(2, arguments.Length);
            Assert.Equal(i, arguments[0]);
            Assert.Equal(elementType, arguments[1]);
        }
    }
}
