namespace Mikodev.Binary.WireTests;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// `Bare` is `Encode`, `Auto` is `EncodeAuto`, `-` means zero bytes, and `Langs`
// lists the implementations that assert the row. A 4 byte header is always legal
// and a 1 byte one only below 128, so `Auto` lists every accepted form with `|`.
public sealed record WireRow(string Name, string Bare, string Auto, Lang Langs)
{
    public IReadOnlyList<string> AutoForms => this.Auto.Split('|');
}

public static class WireFixture
{
    private const string Empty = "-";

    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "wire.tsv");

    public static IReadOnlyList<WireRow> Rows { get; } = Load();

    public static byte[] ParseHex(string hex)
    {
        return hex is Empty ? [] : Convert.FromHexString(hex);
    }

    public static string ToHex(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length is 0 ? Empty : Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // Every width a row's own header may use. The encode direction only asserts what
    // producers write, but a reader has to accept both widths. A row carries its own
    // header only when the auto bytes end with the bare payload: a tuple object
    // writes its members' prefixes while adding nothing of its own.
    public static IReadOnlyList<string> DecodeForms(WireRow row)
    {
        var payload = ParseHex(row.Bare);
        var result = new List<string>();
        foreach (var form in row.AutoForms)
        {
            if (HeaderWidth(ParseHex(form), payload) is 0)
            {
                Add(result, form);
                continue;
            }

            if (payload.Length < 128)
                Add(result, Frame(1, payload));
            Add(result, Frame(4, payload));
        }

        return result;
    }

    private static int HeaderWidth(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> payload)
    {
        if (payload.Length is 0)
            return bytes.Length is 1 or 4 ? bytes.Length : 0;

        var width = bytes.Length - payload.Length;
        if (width is not (1 or 4))
            return 0;
        return bytes[width..].SequenceEqual(payload) ? width : 0;
    }

    private static string Frame(int width, ReadOnlySpan<byte> payload)
    {
        var result = new byte[width + payload.Length];
        if (width is 1)
            result[0] = (byte)payload.Length;
        else
            BinaryPrimitives.WriteUInt32BigEndian(result, (uint)payload.Length | 0x8000_0000U);
        payload.CopyTo(result.AsSpan(width));
        return ToHex(result);
    }

    private static void Add(List<string> forms, string form)
    {
        if (!forms.Contains(form))
            forms.Add(form);
    }

    private static Lang ParseLangs(string text, string line)
    {
        var result = (Lang)0;
        foreach (var part in text.Split(','))
        {
            result |= part switch
            {
                "cs" => Lang.CSharp,
                "cxx" => Lang.Cxx,
                "rs" => Lang.Rust,
                var other => throw new FormatException($"bad language {other}: {line}"),
            };
        }

        if (result is 0)
            throw new FormatException($"no language listed: {line}");
        return result;
    }

    private static List<WireRow> Load()
    {
        if (!File.Exists(FixturePath))
            throw new FileNotFoundException($"missing wire fixture: {FixturePath} (run the wire table generator in the workspace)", FixturePath);

        var result = new List<WireRow>();
        foreach (var raw in File.ReadAllLines(FixturePath))
        {
            var line = raw.TrimStart('\uFEFF').TrimEnd();
            if (line.Length is 0 || line[0] is '#')
                continue;

            var columns = line.Split('\t');
            if (columns.Length is not 4)
                throw new FormatException($"expected 4 columns, got {columns.Length}: {line}");

            var row = new WireRow(columns[0], columns[1], columns[2], ParseLangs(columns[3], line));
            foreach (var form in row.AutoForms)
                _ = ParseHex(form);
            result.Add(row);
        }

        if (result.Count is 0)
            throw new FormatException($"empty wire fixture: {FixturePath}");
        return result;
    }

    internal static string Describe(WireRow row, string entryPoint)
    {
        return string.Create(CultureInfo.InvariantCulture, $"{row.Name} ({entryPoint})");
    }
}
