namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using TokenValue = (System.Collections.Immutable.ImmutableDictionary<string, Token> Tokens, System.Exception? Error);

[DebuggerDisplay(CommonDefine.DebuggerDisplayValue)]
public sealed partial class Token : IDynamicMetaObjectProvider
{
    private readonly IGenerator generator;

    private readonly ReadOnlyMemory<byte> memory;

    private readonly Token? parent;

    private readonly Lazy<TokenValue> create;

    public IReadOnlyDictionary<string, Token> Children => this.create.Value.Tokens;

    public ReadOnlyMemory<byte> Memory => this.memory;

    public Token? Parent => this.parent;

    private Token(IGenerator generator, ReadOnlyMemory<byte> memory, Token? parent)
    {
        Debug.Assert(generator is not null);
        this.generator = generator;
        this.memory = memory;
        this.parent = parent;
        this.create = new Lazy<TokenValue>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private static TokenValue GetTokens(Token origin)
    {
        var generator = origin.generator;
        var memory = origin.memory;
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
                // named keys are always encoded as utf-8, see ContextMethodsOfNamedObject
                var source = Encoding.UTF8.GetString(header);
                var result = new Token(generator, target, origin);
                builder.Add(source, result);
            }
            return (builder.ToImmutable(), null);
        }
        catch (Exception e)
        {
            // token memory is decoded lazily, keep the failure for the later member access instead of throwing here
            return (ImmutableDictionary.Create<string, Token>(), e);
        }
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

    public Token(IGenerator generator, ReadOnlyMemory<byte> memory) : this(generator ?? throw new ArgumentNullException(nameof(generator)), memory, null) { }

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
