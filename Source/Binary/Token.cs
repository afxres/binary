using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    [DebuggerTypeProxy(typeof(TokenDebuggerTypeProxy))]
    public sealed partial class Token : IDynamicMetaObjectProvider
    {
        private static readonly Dictionary<string, Token> shared = new Dictionary<string, Token>();

        private readonly Generator generator;

        private readonly ReadOnlyMemory<byte> memory;

        private Dictionary<string, Token> tokens;

        internal Token(Generator generator, in ReadOnlyMemory<byte> memory)
        {
            Debug.Assert(generator != null);
            Debug.Assert(shared.Count == 0);
            if (memory.IsEmpty)
                tokens = shared;
            else
                this.memory = memory;
            this.generator = generator;
        }

        internal Dictionary<string, Token> GetTokens()
        {
            var result = tokens;
            if (result == null)
                tokens = result = GetDictionary(generator, in memory);
            Debug.Assert(tokens != null);
            Debug.Assert(result != null);
            return result;
        }

        private static Dictionary<string, Token> GetDictionary(Generator generator, in ReadOnlyMemory<byte> memory)
        {
            Debug.Assert(memory.Length > 0);
            Debug.Assert(shared.Count == 0);
            var span = memory.Span;
            var result = new Dictionary<string, Token>(8);
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
                return shared;
            }
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new TokenDynamicMetaObject(parameter, this);

        public Token this[string key] => GetTokens()[key];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object As(Type type) => generator.ToValue(memory.Span, type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T As<T>() => generator.ToValue<T>(memory.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T As<T>(T anonymous) => As<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> AsMemory() => memory;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => memory.Span;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override bool Equals(object obj) => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override int GetHashCode() => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public sealed override string ToString() => $"{nameof(Token)}(Items: {GetTokens().Count}, Bytes: {memory.Length})";
    }
}
