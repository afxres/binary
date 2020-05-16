using Mikodev.Binary.Creators.Generics;
using System;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class DelegateEnumerableBuilder<T, R> : GenericsBuilder<T, R>
    {
        private readonly Func<R, T> constructor;

        public DelegateEnumerableBuilder(Func<R, T> constructor) => this.constructor = constructor;

        public override T Invoke(ReadOnlySpan<byte> span, GenericsAdapter<T, R> adapter)
        {
            if (constructor is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var data = adapter.Decode(span);
            return constructor.Invoke(data);
        }
    }
}
