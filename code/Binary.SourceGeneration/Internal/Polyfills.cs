// Polyfills must declare the BCL namespaces so the compiler can find them by full name, which never matches this folder.
#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System.Runtime.CompilerServices
{
    using System;

    // C# 15 union types require these types, which are not available in netstandard2.0.
    // Remove this file when the project starts targeting a framework that ships them.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class UnionAttribute : Attribute { }

    public interface IUnion
    {
        object? Value { get; }
    }

    internal sealed class IsExternalInit { }
}

namespace System.Diagnostics.CodeAnalysis
{
    using System;

    // Nullable flow analysis requires this attribute, which is not available in netstandard2.0.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
    }
}

#pragma warning restore IDE0130 // Namespace does not match folder structure
