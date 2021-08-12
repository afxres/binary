namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class NamedKeyAttribute : Attribute
{
    public string Key { get; }

    public NamedKeyAttribute(string key) => Key = key;
}
