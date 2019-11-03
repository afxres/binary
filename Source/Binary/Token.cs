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

        public Token(IGenerator generator, in ReadOnlyMemory<byte> memory)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));
            this.memory = memory;
            this.generator = generator;
            tokens = new Lazy<IReadOnlyDictionary<string, Token>>(() => GetDictionary(this), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public Token(IGenerator generator, byte[] buffer) : this(generator, new ReadOnlyMemory<byte>(buffer)) { }

        private static IReadOnlyDictionary<string, Token> GetDictionary(Token token)
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

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new TokenDynamicMetaObject(parameter, this);

        public Token this[string key] => tokens.Value[key];

        public object As(Type type) => ((IConverter)generator.GetConverter(type)).Decode(memory.Span);

        public T As<T>() => ((Converter<T>)generator.GetConverter(typeof(T))).Decode(memory.Span);

        public T As<T>(T anonymous) => As<T>();

        public ReadOnlyMemory<byte> AsMemory() => memory;

        public ReadOnlySpan<byte> AsSpan() => memory.Span;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override string ToString() => $"{nameof(Token)}(Items: {tokens.Value.Count}, Bytes: {memory.Length})";
    }
}
