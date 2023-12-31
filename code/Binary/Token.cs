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
using TokenValue = (System.Collections.Immutable.ImmutableDictionary<string, Token> Tokens, System.Exception? Error);

[DebuggerDisplay(CommonModule.DebuggerDisplayValue)]
public sealed partial class Token : IDynamicMetaObjectProvider
{
    private readonly IGenerator generator;

    private readonly ReadOnlyMemory<byte> memory;

    private readonly DecodeReadOnlyDelegate<string> decode;

    private readonly Token? parent;

    private readonly Lazy<TokenValue> create;

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
        this.create = new Lazy<TokenValue>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private static TokenValue GetTokens(Token origin)
    {
        var generator = origin.generator;
        var memory = origin.memory;
        var decode = origin.decode;
        var intent = memory.Span;

        try
        {
            var builder = ImmutableDictionary.CreateBuilder<string, Token>();
            while (intent.Length is not 0)
            {
                var header = Converter.DecodeWithLengthPrefix(ref intent);
                var buffer = Converter.DecodeWithLengthPrefix(ref intent);
                var offset = memory.Length - buffer.Length - intent.Length;
                var target = memory.Slice(offset, buffer.Length);
                var source = decode.Invoke(in header);
                var result = new Token(generator, target, origin, decode);
                builder.Add(source, result);
            }
            return new TokenValue(builder.ToImmutable(), null);
        }
        catch (Exception e)
        {
            return new TokenValue(ImmutableDictionary.Create<string, Token>(), e);
        }
    }

    private static DecodeReadOnlyDelegate<string> GetDelegate(IGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
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

    private static T GetValue<T>(Token origin)
    {
        var span = origin.memory.Span;
        var converter = (Converter<T>)origin.generator.GetConverter(typeof(T));
        return converter.Decode(in span);
    }

    private static object? GetValue(Token origin, Type type)
    {
        var span = origin.memory.Span;
        var converter = origin.generator.GetConverter(type);
        return converter.Decode(in span);
    }

    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaObject(parameter, this);

    public Token(IGenerator generator, ReadOnlyMemory<byte> memory) : this(generator, memory, null, GetDelegate(generator)) { }

    public Token this[string key] => GetToken(this, key);

    public object? As(Type type) => GetValue(this, type);

    public T As<T>() => GetValue<T>(this);

    public T As<T>(T? anonymous) => GetValue<T>(this);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => $"Child Count = {Children.Count}, Memory Length = {Memory.Length}";
}
