﻿using Microsoft.FSharp.Collections;
using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.Creators.External.Collections
{
    internal sealed class FSharpSetConverter<T> : VariableConverter<FSharpSet<T>>
    {
        private readonly Converter<T> converter;

        public FSharpSetConverter(Converter<T> converter)
        {
            this.converter = converter;
        }

        public override void Encode(ref Allocator allocator, FSharpSet<T> item)
        {
            if (item == null)
                return;
            foreach (var i in item)
                converter.EncodeAuto(ref allocator, i);
        }

        public override FSharpSet<T> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item = SetModule.Empty<T>();
            while (!temp.IsEmpty)
                item = item.Add(converter.DecodeAuto(ref temp));
            return item;
        }
    }
}
