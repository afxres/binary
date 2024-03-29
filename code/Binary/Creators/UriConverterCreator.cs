﻿namespace Mikodev.Binary.Creators;

using System;

internal sealed class UriConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return type == typeof(Uri) ? new UriConverter(context.GetConverter<string>()) : null;
    }
}
