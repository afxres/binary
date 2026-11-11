namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using Mikodev.Binary.SourceGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

public sealed partial class NamedObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolNamedObjectMemberInfo> members;

    private readonly SymbolConstructorInfo<SymbolNamedObjectMemberInfo>? constructor;

    private readonly bool hasSelfTypeReference;

    private NamedObjectConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<SymbolNamedObjectMemberInfo> members, SymbolConstructorInfo<SymbolNamedObjectMemberInfo>? constructor) : base(context, tracker, symbol)
    {
        var types = members.Select(x => x.Type).ToImmutableArray();
        types.ForEach(AddType);
        this.members = members;
        this.constructor = constructor;
        this.hasSelfTypeReference = members.Any(x => SymbolEqualityComparer.Default.Equals(x.Type, Symbol));
    }

    private void AppendConverterHead()
    {
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName} : Mikodev.Binary.Converter<{SymbolTypeFullName}>");
        Output.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendFields()
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            Output.AppendIndent(2, $"private {GetConverterTypeFullName(i)} cvt{i};");
            Output.AppendIndent();
            CancellationToken.ThrowIfCancellationRequested();
        }
    }

    private void AppendInitializeMethod()
    {
        var members = this.members;
        Output.AppendIndent(2, $"public void Initialize(", ")", members.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        Output.AppendIndent(2, $"{{");
        for (var i = 0; i < members.Length; i++)
        {
            Output.AppendIndent(3, $"this.cvt{i} = cvt{i};");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
    }

    private void AppendConverterTail()
    {
        Output.AppendIndent(1, $"}}");
        Output.AppendIndent();
    }

    private void AppendEnsureSufficientExecutionStack()
    {
        if (hasSelfTypeReference is false)
            return;
        Output.AppendIndent(3, $"System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeMethod()
    {
        var members = this.members;
        Output.AppendIndent(2, $"public override void Encode(ref Mikodev.Binary.Allocator allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        if (Symbol.IsValueType is false)
        {
            Output.AppendIndent(3, $"if (item is null)");
            Output.AppendIndent(4, $"return;");
        }
        AppendEnsureSufficientExecutionStack();
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i].IsOptional is false)
            {
                Output.AppendIndent(3, $"Mikodev.Binary.Converter.EncodeWithLengthPrefix(ref allocator, {members[i].NamedKeyLiteral}u8);");
                Output.AppendIndent(3, $"cvt{i}.EncodeWithLengthPrefix(ref allocator, item.{members[i].NameInSourceCode});");
            }
            else
            {
                Output.AppendIndent(3, $"var var{i} = item.{members[i].NameInSourceCode};");
                Output.AppendIndent(3, $"if (System.Collections.Generic.EqualityComparer<{GetTypeFullName(i)}>.Default.Equals(var{i}, default) is false)");
                Output.AppendIndent(3, $"{{");
                Output.AppendIndent(4, $"Mikodev.Binary.Converter.EncodeWithLengthPrefix(ref allocator, {members[i].NamedKeyLiteral}u8);");
                Output.AppendIndent(4, $"cvt{i}.EncodeWithLengthPrefix(ref allocator, var{i});");
                Output.AppendIndent(3, $"}}");
            }
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
    }

    private void AppendDecodeMethod()
    {
        var members = this.members;
        var constructor = this.constructor;
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(in System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        if (Symbol.IsValueType)
        {
            // an empty span is the encoding of a null reference, a value type has no null value
            Output.AppendIndent(3, $"if (span.Length is 0)");
            Output.AppendIndent(4, $"throw new System.ArgumentException(\"Not enough bytes or byte sequence invalid.\");");
        }
        else
        {
            Output.AppendIndent(3, $"if (span.Length is 0)");
            Output.AppendIndent(4, $"return default!;");
        }
        if (constructor is null)
        {
            Output.AppendIndent(3, $"throw new System.NotSupportedException($\"No suitable constructor found, type: {{typeof({SymbolTypeFullName})}}\");");
        }
        else
        {
            AppendDecodeBody();
        }
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
    }

    /// <summary>
    /// Emits the scanning loop and the member decoding. The loop only uses the public members of
    /// <see cref="Mikodev.Binary.Converter"/> and <see cref="Mikodev.Binary.Converter{T}"/>, so the
    /// generated converter does not depend on the public named object components.
    /// </summary>
    private void AppendDecodeBody()
    {
        var members = this.members;
        var constructor = this.constructor!;
        AppendEnsureSufficientExecutionStack();
        Output.AppendIndent(3, $"System.Span<long> slices = stackalloc long[{members.Length}];");
        Output.AppendIndent(3, $"var total = span.Length;");
        Output.AppendIndent(3, $"var rest = span;");
        Output.AppendIndent(3, $"while (rest.Length is not 0)");
        Output.AppendIndent(3, $"{{");
        Output.AppendIndent(4, $"var key = Mikodev.Binary.Converter.DecodeWithLengthPrefix(ref rest);");
        Output.AppendIndent(4, $"var body = Mikodev.Binary.Converter.DecodeWithLengthPrefix(ref rest);");
        Output.AppendIndent(4, $"var cursor = Resolve(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(key), key.Length);");
        Output.AppendIndent(4, $"if ((uint)cursor >= (uint){members.Length})");
        Output.AppendIndent(5, $"continue;");
        Output.AppendIndent(4, $"if (slices[cursor] is not 0)");
        Output.AppendIndent(5, $"throw new System.ArgumentException($\"Named key '{{GetName(cursor)}}' already exists, type: {{typeof({SymbolTypeFullName})}}\");");
        // the low 32 bits hold the length, the high 32 bits the offset of the value in the original span
        Output.AppendIndent(4, $"slices[cursor] = (long)((ulong)(uint)(total - rest.Length - body.Length) << 32 | (uint)body.Length);");
        Output.AppendIndent(3, $"}}");
        AppendCheckRequiredMembers();
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i].IsOptional is false)
                Output.AppendIndent(3, $"var var{i} = cvt{i}.Decode(GetValue(span, slices[{i}]));");
            else
                Output.AppendIndent(3, $"var var{i} = slices[{i}] is not 0 ? cvt{i}.Decode(GetValue(span, slices[{i}])) : default;");
            CancellationToken.ThrowIfCancellationRequested();
        }
        constructor.AppendTo(Output, SymbolTypeFullName, CancellationToken);
    }

    /// <summary>
    /// Emits the missing member diagnostic as a single loop over the slice table, instead of one
    /// guarded throw per member. The named key comes from the same table as the duplicated key
    /// diagnostic, so the message literal and the type handle are emitted only once.
    /// </summary>
    private void AppendCheckRequiredMembers()
    {
        var members = this.members;
        // every member is optional, the table can not miss anything
        if (members.Any(x => x.IsOptional is false) is false)
            return;
        // only the optional members are allowed to be absent, everything else must be present
        var condition = members.Any(x => x.IsOptional) ? $"if (slices[cursor] is not 0 || IsOptional(cursor))" : $"if (slices[cursor] is not 0)";
        Output.AppendIndent(3, $"for (var cursor = 0; cursor < slices.Length; cursor++)");
        Output.AppendIndent(3, $"{{");
        Output.AppendIndent(4, condition);
        Output.AppendIndent(5, $"continue;");
        Output.AppendIndent(4, $"throw new System.ArgumentException($\"Named key '{{GetName(cursor)}}' does not exist, type: {{typeof({SymbolTypeFullName})}}\");");
        Output.AppendIndent(3, $"}}");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private static int GetWordSize(int length)
    {
        if (length > 15)
            return 0;
        if (length >= 8)
            return 8;
        if (length >= 4)
            return 4;
        return length >= 2 ? 2 : 1;
    }

    private static ulong Load(byte[] bytes, int offset, int count)
    {
        var value = 0UL;
        for (var i = 0; i < count; i++)
            value |= (ulong)bytes[offset + i] << (0x08 * i);
        return value;
    }

    private static string GetNamedKeyPropertyName(int index) => $"NamedKey{index}";

    /// <summary>
    /// Composes the comparison for one named key. The literal is assembled from the utf-8 bytes in
    /// little endian order, and the emitted reader reads words with the same byte order, so the
    /// generated table does not depend on the native byte order of the machine that runs either the
    /// build (the generator) or the result (the application).
    /// </summary>
    private string GetResolveCondition(int index)
    {
        var bytes = Encoding.UTF8.GetBytes(this.members[index].NamedKey);
        var length = bytes.Length;
        var size = GetWordSize(length);
        // a key whose length matches the word size only needs one read, the same word would
        // otherwise be compared twice
        if (size is 8)
        {
            var head = $"Word64(ref key, 0) == 0x{Load(bytes, 0, 8):X16}UL";
            return length is 8 ? head : $"{head} && Word64(ref key, {length - 8}) == 0x{Load(bytes, length - 8, 8):X16}UL";
        }
        if (size is 4)
        {
            var head = $"Word32(ref key, 0) == 0x{Load(bytes, 0, 4):X8}U";
            return length is 4 ? head : $"{head} && Word32(ref key, {length - 4}) == 0x{Load(bytes, length - 4, 4):X8}U";
        }
        return size switch
        {
            2 when length is 3 => $"Word16(ref key, 0) == 0x{Load(bytes, 0, 2):X4} && Byte(ref key, 2) == 0x{bytes[2]:X2}",
            2 => $"Word16(ref key, 0) == 0x{Load(bytes, 0, 2):X4}",
            1 => $"Byte(ref key, 0) == 0x{bytes[0]:X2}",
            0 => $"System.MemoryExtensions.SequenceEqual(System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref key, length), {GetNamedKeyPropertyName(index)})",
            _ => throw new ArgumentOutOfRangeException($"Invalid named key: '{this.members[index].NamedKey}'"),
        };
    }

    private void AppendResolveMethod()
    {
        var members = this.members;
        var conditions = new string[members.Length];
        var sizes = new bool[9];
        var needByte = false;
        var buckets = new SortedDictionary<int, List<int>>();
        for (var i = 0; i < members.Length; i++)
        {
            var length = Encoding.UTF8.GetByteCount(members[i].NamedKey);
            if (length is 0)
            {
                // an empty named key matches an empty wire key, nothing to compare
                conditions[i] = "true";
            }
            else
            {
                var size = GetWordSize(length);
                conditions[i] = GetResolveCondition(i);
                sizes[size] = true;
                needByte |= length is 1 or 3;
            }
            if (buckets.TryGetValue(length, out var bucket) is false)
                buckets.Add(length, bucket = []);
            bucket.Add(i);
            CancellationToken.ThrowIfCancellationRequested();
        }

        needByte |= buckets.Values.Any(x => x.Count > 1);
        Output.AppendIndent(2, $"private static int Resolve(ref byte key, int length)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"switch (length)");
        Output.AppendIndent(3, $"{{");
        foreach (var pair in buckets)
        {
            var bucket = pair.Value;
            Output.AppendIndent(4, $"case {pair.Key}:");
            if (pair.Key is 0)
            {
                Output.AppendIndent(5, $"return {bucket[0]};");
                continue;
            }
            if (bucket.Count is 1)
            {
                Output.AppendIndent(5, $"return {conditions[bucket[0]]} ? {bucket[0]} : -1;");
                continue;
            }
            var groups = bucket.GroupBy(x => Encoding.UTF8.GetBytes(members[x].NamedKey)[0]).OrderBy(x => x.Key);
            Output.AppendIndent(5, $"switch (Byte(ref key, 0))");
            Output.AppendIndent(5, $"{{");
            foreach (var group in groups)
            {
                var items = group.ToList();
                if (items.Count is 1)
                {
                    var item = items[0];
                    // a one byte key is already fully compared by the enclosing switch, repeating
                    // the first byte test here would be dead work
                    var repeated = conditions[item] == $"Byte(ref key, 0) == 0x{group.Key:X2}";
                    if (repeated)
                        Output.AppendIndent(6, $"case {group.Key}: return {item};");
                    else
                        Output.AppendIndent(6, $"case {group.Key}: return {conditions[item]} ? {item} : -1;");
                    continue;
                }
                Output.AppendIndent(6, $"case {group.Key}:");
                foreach (var item in items)
                    Output.AppendIndent(7, $"if ({conditions[item]}) return {item};");
                Output.AppendIndent(7, $"return -1;");
            }
            Output.AppendIndent(6, $"default:");
            Output.AppendIndent(7, $"return -1;");
            Output.AppendIndent(5, $"}}");
        }
        Output.AppendIndent(4, $"default:");
        Output.AppendIndent(5, $"return -1;");
        Output.AppendIndent(3, $"}}");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();

        if (needByte)
            AppendByteReader();
        AppendWordReader("ushort", "Word16", "ReadUInt16LittleEndian", sizes[2]);
        AppendWordReader("uint", "Word32", "ReadUInt32LittleEndian", sizes[4]);
        AppendWordReader("ulong", "Word64", "ReadUInt64LittleEndian", sizes[8]);
        AppendValueReader();
        AppendNameReader();
        AppendOptionalReader();
        for (var i = 0; i < members.Length; i++)
        {
            var bytes = Encoding.UTF8.GetBytes(members[i].NamedKey);
            if (bytes.Length <= 15)
                continue;
            Output.AppendIndent(2, $"private static System.ReadOnlySpan<byte> {GetNamedKeyPropertyName(i)} => new byte[] {{ ", $" }};", bytes.Length, x => $"0x{bytes[x]:X2}");
            Output.AppendIndent();
            CancellationToken.ThrowIfCancellationRequested();
        }
    }

    private void AppendValueReader()
    {
        Output.AppendIndent(2, $"[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        Output.AppendIndent(2, $"private static System.ReadOnlySpan<byte> GetValue(System.ReadOnlySpan<byte> source, long item)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"return source.Slice((int)(item >> 32), (int)item);");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
    }

    /// <summary>
    /// Maps a member index back to its named key, used by the duplicated key and the missing
    /// member diagnostics. A switch keeps this allocation free, a string array would be allocated
    /// on first use.
    /// </summary>
    private void AppendNameReader()
    {
        var members = this.members;
        Output.AppendIndent(2, $"private static string GetName(int cursor)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"return cursor switch");
        Output.AppendIndent(3, $"{{");
        for (var i = 0; i < members.Length; i++)
        {
            Output.AppendIndent(4, $"{i} => {members[i].NamedKeyLiteral},");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(4, $"_ => string.Empty,");
        Output.AppendIndent(3, $"}};");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
    }

    /// <summary>
    /// Emits the optional member table used by the missing member diagnostic. A single pattern is
    /// emitted per optional member, so the check stays a plain comparison chain for any count.
    /// </summary>
    private void AppendOptionalReader()
    {
        var optional = new List<int>();
        for (var i = 0; i < this.members.Length; i++)
            if (this.members[i].IsOptional)
                optional.Add(i);
        // the table is only used by the missing member check, which needs at least one required
        // member to check and at least one optional member to skip
        if (optional.Count is 0 || optional.Count == this.members.Length)
            return;
        Output.AppendIndent(2, $"private static bool IsOptional(int cursor) => cursor is {string.Join(" or ", optional)};");
        Output.AppendIndent();
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendByteReader()
    {
        Output.AppendIndent(2, $"[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        Output.AppendIndent(2, $"private static byte Byte(ref byte key, int offset)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"return System.Runtime.CompilerServices.Unsafe.Add(ref key, offset);");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
    }

    private void AppendWordReader(string type, string name, string read, bool used)
    {
        if (used is false)
            return;
        // BinaryPrimitives already picks the little endian fast path, the explicit span carries a
        // constant length so the JIT drops the range check instead of testing it at run time
        Output.AppendIndent(2, $"[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        Output.AppendIndent(2, $"private static {type} {name}(ref byte key, int offset)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"return System.Buffers.Binary.BinaryPrimitives.{read}(System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref System.Runtime.CompilerServices.Unsafe.Add(ref key, offset), sizeof({type})));");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
    }

    private void AppendConverterCreatorBody()
    {
        var members = this.members;
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}();");
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(member, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i), allowsSelfTypeReference: hasSelfTypeReference);
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"converter.Initialize(", ");", members.Length, x => $"cvt{x}");
    }

    protected override void Handle()
    {
        AppendConverterHead();
        AppendFields();
        AppendInitializeMethod();

        AppendEncodeMethod();
        AppendDecodeMethod();
        AppendResolveMethod();
        AppendConverterTail();

        AppendConverterCreatorHead();
        AppendConverterCreatorBody();
        AppendConverterCreatorTail();
    }
}
