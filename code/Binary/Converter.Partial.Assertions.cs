namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System.Reflection;

public abstract partial class Converter<T>
{
    private void EnsureOverride<TDelegate>(TDelegate auto, TDelegate prefix) where TDelegate : Delegate
    {
        EnsureOverride(auto.Method, prefix.Method);
    }

    private void EnsureOverride(MethodInfo auto, MethodInfo prefix)
    {
        if (auto.DeclaringType != typeof(Converter<T>) || prefix.DeclaringType == typeof(Converter<T>))
            return;
        ThrowHelper.ThrowNotOverride(auto.Name, prefix.Name, GetType());
    }
}
