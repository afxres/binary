using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading;

namespace Mikodev.Binary
{
    [DebuggerTypeProxy(typeof(TokenDebuggerTypeProxy))]
    public sealed partial class Token : IDynamicMetaObjectProvider
    {
        private static readonly IReadOnlyDictionary<string, Token> empty = new Dictionary<string, Token>();

        private readonly ReadOnlyMemory<byte> memory;

        private readonly IGenerator generator;

        private readonly Converter<string> stringConverter;

        private readonly Lazy<IReadOnlyDictionary<string, Token>> tokens;

        private Token(IGenerator generator, ReadOnlyMemory<byte> memory, Converter<string> stringConverter)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));
            stringConverter ??= generator.GetConverter<string>();
            if (stringConverter is null)
                throw new ArgumentException("Require valid string converter.");
            this.memory = memory;
            this.generator = generator;
            this.stringConverter = stringConverter;
            tokens = new Lazy<IReadOnlyDictionary<string, Token>>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static IReadOnlyDictionary<string, Token> GetTokens(Token instance)
        {
            var memory = instance.memory;
            if (memory.IsEmpty)
                return empty;
            var generator = instance.generator;
            var stringConverter = instance.stringConverter;
            var dictionary = new Dictionary<string, Token>();
            var body = memory.Span;

            try
            {
                while (!body.IsEmpty)
                {
                    var header = stringConverter.DecodeWithLengthPrefix(ref body);
                    var buffer = PrimitiveHelper.DecodeBufferWithLengthPrefix(ref body);
                    var offset = memory.Length - buffer.Length - body.Length;
                    var target = memory.Slice(offset, buffer.Length);
                    var result = new Token(generator, target, stringConverter);
                    dictionary.Add(header, result);
                }
                return dictionary;
            }
            catch (Exception)
            {
                return empty;
            }
        }

        private Token GetToken(string key, bool nothrow)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (tokens.Value.TryGetValue(key, out var value))
                return value;
            if (nothrow)
                return null;
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new TokenDynamicMetaObject(parameter, this);

        public Token(IGenerator generator, ReadOnlyMemory<byte> memory) : this(generator, memory, null) { }

        public Token this[string key] => GetToken(key, nothrow: false);

        public Token this[string key, bool nothrow] => GetToken(key, nothrow);

        public object As(Type type) => ((IConverter)generator.GetConverter(type)).Decode(memory.Span);

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
