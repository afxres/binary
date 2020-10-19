using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mikodev.Binary
{
    [DebuggerTypeProxy(typeof(TokenDebuggerTypeProxy))]
    public sealed class Token : IDynamicMetaObjectProvider
    {
        private readonly IGenerator generator;

        private readonly ReadOnlyMemory<byte> memory;

        private readonly Token parent;

        private readonly Converter<string> encoder;

        private readonly Lazy<ImmutableDictionary<string, Token>> lazy;

        public IReadOnlyDictionary<string, Token> Children => this.lazy.Value;

        public ReadOnlyMemory<byte> Memory => this.memory;

        public Token Parent => this.parent;

        private Token(IGenerator generator, ReadOnlyMemory<byte> memory, Token parent, Converter<string> encoder)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));
            if (encoder is null)
                encoder = generator.GetConverter<string>();
            if (encoder is null)
                throw new ArgumentException("No available string converter found.");
            this.parent = parent;
            this.memory = memory;
            this.generator = generator;
            this.encoder = encoder;
            this.lazy = new Lazy<ImmutableDictionary<string, Token>>(() => GetTokens(this), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static ImmutableDictionary<string, Token> GetTokens(Token instance)
        {
            var memory = instance.memory;
            var generator = instance.generator;
            var encoder = instance.encoder;
            var body = memory.Span;

            try
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Token>();
                while (body.IsEmpty is false)
                {
                    var header = encoder.DecodeWithLengthPrefix(ref body);
                    var buffer = PrimitiveHelper.DecodeBufferWithLengthPrefix(ref body);
                    var offset = memory.Length - buffer.Length - body.Length;
                    var target = memory.Slice(offset, buffer.Length);
                    var result = new Token(generator, target, instance, encoder);
                    builder.Add(header, result);
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

        public object As(Type type) => this.generator.GetConverter(type).Decode(this.memory.Span);

        public T As<T>() => ((Converter<T>)this.generator.GetConverter(typeof(T))).Decode(this.memory.Span);

        public T As<T>(T anonymous) => As<T>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => ReferenceEquals(this, obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(Token)}({nameof(Children)}: {Children.Count}, {nameof(Memory)}: {Memory.Length})";
    }
}
