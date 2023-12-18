namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class NamedKeyAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
