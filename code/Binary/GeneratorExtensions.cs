namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

public static class GeneratorExtensions
{
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static Converter<T> GetConverter<T>(this IGenerator generator) => (Converter<T>)generator.GetConverter(typeof(T));

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static Converter<T> GetConverter<T>(this IGenerator generator, T? anonymous) => generator.GetConverter<T>();

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static byte[] Encode(this IGenerator generator, object? item, Type type) => generator.GetConverter(type).Encode(item);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static byte[] Encode<T>(this IGenerator generator, T item) => generator.GetConverter<T>().Encode(item);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static object? Decode(this IGenerator generator, byte[]? buffer, Type type) => generator.GetConverter(type).Decode(buffer);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static object? Decode(this IGenerator generator, ReadOnlySpan<byte> span, Type type) => generator.GetConverter(type).Decode(in span);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, byte[]? buffer) => generator.GetConverter<T>().Decode(buffer);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, byte[]? buffer, T? anonymous) => generator.Decode<T>(buffer);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, ReadOnlySpan<byte> span) => generator.GetConverter<T>().Decode(in span);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static T Decode<T>(this IGenerator generator, ReadOnlySpan<byte> span, T? anonymous) => generator.Decode<T>(span);
}
