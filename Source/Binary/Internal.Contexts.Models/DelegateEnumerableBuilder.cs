using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class DelegateEnumerableBuilder<T, R> : CollectionBuilder<T, T, R>
    {
        private readonly Func<R, T> constructor;

        public DelegateEnumerableBuilder(Func<R, T> constructor) => this.constructor = constructor;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<R> adapter, ReadOnlySpan<byte> span)
        {
            if (constructor is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var data = adapter.To(span);
            return constructor.Invoke(data);
        }
    }
}
