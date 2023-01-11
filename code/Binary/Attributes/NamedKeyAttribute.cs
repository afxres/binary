namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class NamedKeyAttribute : Attribute
{
    public string Key { get; }

    public NamedKeyAttribute(string key) => Key = key;
}
