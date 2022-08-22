namespace Mikodev.Binary;

using Mikodev.Binary.Components;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

[DebuggerTypeProxy(typeof(TokenDebuggerTypeProxy))]
public sealed class Token : IDynamicMetaObjectProvider
{
    private readonly IGenerator generator;

    private readonly ReadOnlyMemory<byte> memory;

    private readonly DecodeReadOnlyDelegate<string> decode;

    private readonly Token? parent;

    private readonly Lazy<(ImmutableDictionary<string, Token> Tokens, Exception? Error)> create;

    public IReadOnlyDictionary<string, Token> Children => this.create.Value.Tokens;

    public ReadOnlyMemory<byte> Memory => this.memory;

    public Token? Parent => this.parent;

    private Token(IGenerator generator, ReadOnlyMemory<byte> memory, Token? parent, DecodeReadOnlyDelegate<string> decode)
    {
        Debug.Assert(generator is not null);
        Debug.Assert(decode is not null);
        this.generator = generator;
        this.memory = memory;
        this.parent = parent;
        this.decode = decode;
        this.create = new Lazy<(ImmutableDictionary<string, Token>, Exception?)>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private static (ImmutableDictionary<string, Token>, Exception?) GetTokens(Token origin)
    {
        var generator = origin.generator;
        var memory = origin.memory;
        var decode = origin.decode;
        var body = memory.Span;

        try
        {
            var builder = ImmutableDictionary.CreateBuilder<string, Token>();
            while (body.Length is not 0)
            {
                var header = Converter.DecodeWithLengthPrefix(ref body);
                var buffer = Converter.DecodeWithLengthPrefix(ref body);
                var offset = memory.Length - buffer.Length - body.Length;
                var target = memory.Slice(offset, buffer.Length);
                var source = decode.Invoke(in header);
                var result = new Token(generator, target, origin, decode);
                builder.Add(source, result);
            }
            return (builder.ToImmutable(), null);
        }
        catch (Exception e)
        {
            return (ImmutableDictionary.Create<string, Token>(), e);
        }
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static DecodeReadOnlyDelegate<string> GetDelegate(IGenerator generator)
    {
        if (generator is null)
            throw new ArgumentNullException(nameof(generator));
        var converter = generator.GetConverter<string>();
        if (converter is null)
            throw new ArgumentException("No available string converter found.");
        return converter.Decode;
    }

    private static Token GetToken(Token origin, string key)
    {
        var source = origin.create.Value;
        if (source.Tokens.TryGetValue(key, out var result))
            return result;
        throw new KeyNotFoundException($"Key '{key}' not found.", source.Error);
    }

    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new TokenDynamicMetaObject(parameter, this);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public Token(IGenerator generator, ReadOnlyMemory<byte> memory) : this(generator, memory, null, GetDelegate(generator)) { }

    public Token this[string key] => GetToken(this, key);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public object? As(Type type) => this.generator.GetConverter(type).Decode(this.memory.Span);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public T As<T>() => ((Converter<T>)this.generator.GetConverter(typeof(T))).Decode(this.memory.Span);

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public T As<T>(T? anonymous) => As<T>();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => $"{nameof(Token)}({nameof(Children)}: {Children.Count}, {nameof(Memory)}: {Memory.Length})";
}
