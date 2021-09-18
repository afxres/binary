namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
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

    private readonly Lazy<ImmutableDictionary<string, Token>> lazy;

    public IReadOnlyDictionary<string, Token> Children => this.lazy.Value;

    public ReadOnlyMemory<byte> Memory => this.memory;

    public Token? Parent => this.parent;

    private Token(IGenerator generator, ReadOnlyMemory<byte> memory, Token? parent, DecodeReadOnlyDelegate<string>? decode)
    {
        if (generator is null)
            throw new ArgumentNullException(nameof(generator));
        this.generator = generator;
        this.memory = memory;
        this.parent = parent;
        this.decode = decode ?? (generator.GetConverter<string>() ?? throw new ArgumentException("No available string converter found.")).Decode;
        this.lazy = new Lazy<ImmutableDictionary<string, Token>>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private static ImmutableDictionary<string, Token> GetTokens(Token origin)
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
            return builder.ToImmutable();
        }
        catch (Exception)
        {
            return ImmutableDictionary.Create<string, Token>();
        }
    }

    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new TokenDynamicMetaObject(parameter, this);

    public Token(IGenerator generator, ReadOnlyMemory<byte> memory) : this(generator, memory, null, null) { }

    public Token this[string key] => this.lazy.Value[key];

    public object? As(Type type) => this.generator.GetConverter(type).Decode(this.memory.Span);

    public T As<T>() => ((Converter<T>)this.generator.GetConverter(typeof(T))).Decode(this.memory.Span);

    public T As<T>(T? anonymous) => As<T>();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => $"{nameof(Token)}({nameof(Children)}: {Children.Count}, {nameof(Memory)}: {Memory.Length})";
}
