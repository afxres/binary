namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class TupleKeyAttribute : Attribute
{
    public int Key { get; }

    public TupleKeyAttribute(int key) => Key = key;
}
