﻿namespace Mikodev.Binary;

using Mikodev.Binary.Internal.Metadata;
using System;
using System.Reflection;

public abstract partial class Converter<T> : IConverterMetadata
{
    Type IConverterMetadata.GetGenericArgument()
    {
        return typeof(T);
    }

    MethodInfo IConverterMetadata.GetMethod(string name)
    {
        return name switch
        {
            nameof(Decode) => new DecodeReadOnlyDelegate<T>(Decode).Method,
            nameof(DecodeAuto) => new DecodeDelegate<T>(DecodeAuto).Method,
            nameof(DecodeWithLengthPrefix) => new DecodeDelegate<T>(DecodeWithLengthPrefix).Method,
            nameof(Encode) => new EncodeDelegate<T>(Encode).Method,
            nameof(EncodeAuto) => new EncodeDelegate<T>(EncodeAuto).Method,
            nameof(EncodeWithLengthPrefix) => new EncodeDelegate<T>(EncodeWithLengthPrefix).Method,
            _ => throw new ArgumentException($"Invalid method name '{name}'", nameof(name)),
        };
    }
}
