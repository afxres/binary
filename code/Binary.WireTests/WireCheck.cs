namespace Mikodev.Binary.WireTests;

using Mikodev.Binary;
using Mikodev.Binary.Attributes;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

public static class WireCheck
{
    // The table lists the legal forms of a row, so the produced bytes must equal one
    // of them. A 4 byte header is always legal and a 1 byte one only below 128; which
    // one an implementation writes is its own business. Do not add a decode fallback:
    // it hides a real byte difference behind a value comparison.
    public static string? AutoMismatch(IGenerator generator, WireCase item, WireRow row)
    {
        var actual = WireFixture.ToHex(WireEncoder.Encode(generator, item, auto: true));
        var forms = row.AutoForms;
        foreach (var form in forms)
        {
            if (actual == form)
                return null;
        }

        return $"{WireFixture.Describe(row, "EncodeAuto")}: bytes {actual} are not one of [{string.Join(", ", forms)}]";
    }

    // null: no structural comparison is available, and callers treat that as a failure.
    internal static bool? Compare(object? expected, object? actual)
    {
        if (expected is null || actual is null)
            return expected is null && actual is null;

        var type = expected.GetType();
        if (type != actual.GetType())
            return false;

        if (expected is string)
            return expected.Equals(actual);

        // `Memory<T>` / `ReadOnlyMemory<T>` implement only the generic enumerable, so compare their spans first.
        if (expected is Memory<int> memoryInts)
            return memoryInts.Span.SequenceEqual(((Memory<int>)actual).Span);
        if (expected is ReadOnlyMemory<int> readOnlyInts)
            return readOnlyInts.Span.SequenceEqual(((ReadOnlyMemory<int>)actual).Span);

        if (expected is IEnumerable items)
        {
            var left = items.Cast<object?>().ToArray();
            var right = ((IEnumerable)actual).Cast<object?>().ToArray();
            if (left.Length != right.Length)
                return false;

            var known = true;
            for (var index = 0; index < left.Length; index++)
            {
                var one = Compare(left[index], right[index]);
                if (one is false)
                    return false;
                known &= one is true;
            }

            return known ? true : null;
        }

        if (expected.Equals(actual))
            return true;

        // Named and tuple objects have no value equality, so compare their members.
        if (!type.IsDefined(typeof(NamedObjectAttribute), inherit: true) && !type.IsDefined(typeof(TupleObjectAttribute), inherit: true))
            return null;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && x.GetIndexParameters().Length is 0).ToArray();
        if (properties.Length is 0)
            return null;

        foreach (var property in properties)
        {
            if (Compare(property.GetValue(expected), property.GetValue(actual)) is false)
                return false;
        }

        return true;
    }
}
