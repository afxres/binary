﻿using System;

namespace Mikodev.Binary
{
    public static class GeneratorExtensions
    {
        public static Converter<T> GetConverter<T>(this IGenerator generator) => (Converter<T>)generator.GetConverter(typeof(T));

        public static Converter<T> GetConverter<T>(this IGenerator generator, T anonymous) => generator.GetConverter<T>();

        public static byte[] Encode(this IGenerator generator, object item, Type type) => generator.GetConverter(type).Encode(item);

        public static byte[] Encode<T>(this IGenerator generator, T item) => generator.GetConverter<T>().Encode(item);

        public static object Decode(this IGenerator generator, byte[] buffer, Type type) => generator.GetConverter(type).Decode(buffer);

        public static object Decode(this IGenerator generator, ReadOnlySpan<byte> span, Type type) => generator.GetConverter(type).Decode(in span);

        public static T Decode<T>(this IGenerator generator, byte[] buffer) => generator.GetConverter<T>().Decode(buffer);

        public static T Decode<T>(this IGenerator generator, byte[] buffer, T anonymous) => generator.Decode<T>(buffer);

        public static T Decode<T>(this IGenerator generator, ReadOnlySpan<byte> span) => generator.GetConverter<T>().Decode(in span);

        public static T Decode<T>(this IGenerator generator, ReadOnlySpan<byte> span, T anonymous) => generator.Decode<T>(span);
    }
}
