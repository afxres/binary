using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mikodev.Binary
{
    [DebuggerTypeProxy(typeof(TokenDebuggerTypeProxy))]
    public sealed partial class Token : IDynamicMetaObjectProvider
    {
        private static readonly IReadOnlyDictionary<string, Token> empty = new Dictionary<string, Token>();

        private readonly IGenerator generator;

        private readonly ReadOnlyMemory<byte> memory;

        private readonly Lazy<IReadOnlyDictionary<string, Token>> tokens;

        public Token(IGenerator generator, ReadOnlyMemory<byte> memory)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));
            this.memory = memory;
            this.generator = generator;
            tokens = new Lazy<IReadOnlyDictionary<string, Token>>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static IReadOnlyDictionary<string, Token> GetTokens(Token token)
        {
            var memory = token.memory;
            if (memory.IsEmpty)
                return empty;
            var generator = token.generator;
            var span = memory.Span;
            var result = new Dictionary<string, Token>();
            var reader = new LengthReader(memory.Length);
            var encoding = Converter.Encoding;
            ref var source = ref MemoryMarshal.GetReference(span);

            try
            {
                while (reader.Any())
                {
                    reader.Update(ref source);
                    var key = encoding.GetString(span.Slice(reader.Offset, reader.Length));
                    reader.Update(ref source);
                    var value = new Token(generator, memory.Slice(reader.Offset, reader.Length));
                    result.Add(key, value);
                }
                return result;
            }
            catch (ArgumentException)
            {
                return empty;
            }
        }

        private Token GetToken(string key, bool nothrow)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (tokens.Value.TryGetValue(key, out var value))
                return value;
            if (nothrow)
                return null;
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new TokenDynamicMetaObject(parameter, this);

        public Token this[string key] => GetToken(key, nothrow: false);

        public Token this[string key, bool nothrow] => GetToken(key, nothrow: true);

        public object As(Type type) => ((IConverter)generator.GetConverter(type)).Decode(memory.Span);

        public T As<T>() => ((Converter<T>)generator.GetConverter(typeof(T))).Decode(memory.Span);

        public T As<T>(T anonymous) => As<T>();

        public ReadOnlyMemory<byte> AsMemory() => memory;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override string ToString() => $"{nameof(Token)}(Items: {tokens.Value.Count}, Bytes: {memory.Length})";
    }
}
