namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

public static class GeneratorExtensions
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static Converter<T> GetConverter<T>(this IGenerator generator) => (Converter<T>)generator.GetConverter(typeof(T));

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static Converter<T> GetConverter<T>(this IGenerator generator, T? anonymous) => generator.GetConverter<T>();

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static byte[] Encode(this IGenerator generator, object? item, Type type) => generator.GetConverter(type).Encode(item);

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static byte[] Encode<T>(this IGenerator generator, T item) => generator.GetConverter<T>().Encode(item);

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static object? Decode(this IGenerator generator, byte[]? buffer, Type type) => generator.GetConverter(type).Decode(buffer);

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static object? Decode(this IGenerator generator, scoped ReadOnlySpan<byte> span, Type type) => generator.GetConverter(type).Decode(in span);

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, byte[]? buffer) => generator.GetConverter<T>().Decode(buffer);

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, byte[]? buffer, T? anonymous) => generator.Decode<T>(buffer);

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, scoped ReadOnlySpan<byte> span) => generator.GetConverter<T>().Decode(in span);

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, scoped ReadOnlySpan<byte> span, T? anonymous) => generator.Decode<T>(span);
}
