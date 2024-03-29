﻿namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public partial class UriConverterTests
{
    private sealed class SimpleStringConverter : Converter<string>
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public List<string> CallingSteps { get; } = [];

        public void RecordCalling([CallerMemberName] string? name = default)
        {
            ArgumentNullException.ThrowIfNull(name);
            CallingSteps.Add(name);
        }

        public override string Decode(in ReadOnlySpan<byte> span)
        {
            RecordCalling();
            return Encoding.GetString(span);
        }

        public override string DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            RecordCalling();
            return Encoding.GetString(Converter.DecodeWithLengthPrefix(ref span));
        }

        public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            RecordCalling();
            return Encoding.GetString(Converter.DecodeWithLengthPrefix(ref span));
        }

        public override void Encode(ref Allocator allocator, string? item)
        {
            RecordCalling();
            Allocator.Append(ref allocator, item.AsSpan(), Encoding);
        }

        public override void EncodeAuto(ref Allocator allocator, string? item)
        {
            RecordCalling();
            Allocator.AppendWithLengthPrefix(ref allocator, item.AsSpan(), Encoding);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, string? item)
        {
            RecordCalling();
            Allocator.AppendWithLengthPrefix(ref allocator, item.AsSpan(), Encoding);
        }
    }
}
