namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;

public sealed partial class InlineArrayConverterContext : SymbolConverterContext
{
    private readonly TypeInfo info;

    private InlineArrayConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, TypeInfo info) : base(context, tracker, symbol)
    {
        var element = info.ElementType;
        AddType(0, element);
        this.info = info;
    }

    private void AppendConverterHead()
    {
        var info = this.info;
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}({GetConverterTypeFullName(0)} cvt0)");
        Output.AppendIndent(2, $": Mikodev.Binary.Converter<{SymbolTypeFullName}>(checked(cvt0.Length * {info.Length}))");
        Output.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail()
    {
        Output.AppendIndent(1, $"}}");
        Output.AppendIndent();
    }

    private void AppendEncodeMethod()
    {
        Output.AppendIndent(2, $"public override void Encode(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"var buffer = (System.ReadOnlySpan<{GetTypeFullName(0)}>)item;");
        Output.AppendIndent(3, $"for (var i = 0; i < buffer.Length; i++)");
        Output.AppendIndent(4, $"cvt0.EncodeAuto(ref allocator, buffer[i]);");
        Output.AppendIndent(2, $"}}");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendDecodeMethod()
    {
        Output.AppendIndent();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(in System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"var body = span;");
        Output.AppendIndent(3, $"var result = default({SymbolTypeFullName});");
        Output.AppendIndent(3, $"var buffer = (System.Span<{GetTypeFullName(0)}>)result;");
        Output.AppendIndent(3, $"for (var i = 0; i < buffer.Length; i++)");
        Output.AppendIndent(4, $"buffer[i] = cvt0.DecodeAuto(ref body);");
        Output.AppendIndent(3, $"return result;");
        Output.AppendIndent(2, $"}}");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterCreatorBody()
    {
        var info = this.info;
        var element = info.ElementType;
        AppendAssignConverterExplicit(element, "cvt0", GetConverterTypeFullName(0), GetTypeFullName(0));
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(cvt0);");
    }

    protected override void Handle()
    {
        AppendConverterHead();
        AppendEncodeMethod();
        AppendDecodeMethod();
        AppendConverterTail();

        AppendConverterCreatorHead();
        AppendConverterCreatorBody();
        AppendConverterCreatorTail();
    }
}
