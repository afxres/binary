using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading;

namespace Mikodev.Binary
{
    [DebuggerTypeProxy(typeof(TokenDebuggerTypeProxy))]
    public sealed partial class Token : IDynamicMetaObjectProvider
    {
        private readonly ReadOnlyMemory<byte> memory;

        private readonly IGenerator generator;

        private readonly Converter<string> encoder;

        private readonly Lazy<Dictionary<string, Token>> tokens;

        private Token(IGenerator generator, ReadOnlyMemory<byte> memory, Converter<string> encoder)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));
            encoder ??= generator.GetConverter<string>();
            if (encoder is null)
                throw new ArgumentException("No available string converter found.");
            this.memory = memory;
            this.generator = generator;
            this.encoder = encoder;
            this.tokens = new Lazy<Dictionary<string, Token>>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static Dictionary<string, Token> GetTokens(Token instance)
        {
            var memory = instance.memory;
            var generator = instance.generator;
            var encoder = instance.encoder;
            var body = memory.Span;

            try
            {
                var dictionary = new Dictionary<string, Token>();
                while (!body.IsEmpty)
                {
                    var header = encoder.DecodeWithLengthPrefix(ref body);
                    var buffer = PrimitiveHelper.DecodeBufferWithLengthPrefix(ref body);
                    var offset = memory.Length - buffer.Length - body.Length;
                    var target = memory.Slice(offset, buffer.Length);
                    var result = new Token(generator, target, encoder);
                    dictionary.Add(header, result);
                }
                return dictionary;
            }
            catch (Exception)
            {
                return new Dictionary<string, Token>(capacity: 0);
            }
        }

        [DebuggerStepThrough, DoesNotReturn]
        private static Token ExceptKeyNull() => throw new ArgumentNullException("key");

        [DebuggerStepThrough, DoesNotReturn]
        private static Token ExceptKeyNotFound(string key) => throw new KeyNotFoundException($"Key '{key}' not found.");

        private Token GetToken(string key, bool nothrow)
        {
            if (key is null)
                return ExceptKeyNull();
            if (tokens.Value.TryGetValue(key, out var value))
                return value;
            return nothrow ? null : ExceptKeyNotFound(key);
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new TokenDynamicMetaObject(parameter, this);

        public Token(IGenerator generator, ReadOnlyMemory<byte> memory) : this(generator, memory, null) { }

        public Token this[string key] => GetToken(key, nothrow: false);

        public Token this[string key, bool nothrow] => GetToken(key, nothrow);

        public object As(Type type) => generator.GetConverter(type).Decode(memory.Span);

        public T As<T>() => ((Converter<T>)generator.GetConverter(typeof(T))).Decode(memory.Span);

        public T As<T>(T anonymous) => As<T>();

        public ReadOnlyMemory<byte> AsMemory() => memory;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(Token)}(Items: {tokens.Value.Count}, Bytes: {memory.Length})";
    }
}
