namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class TupleKeyAttribute : Attribute
{
    public int Key { get; }

    public TupleKeyAttribute(int key) => Key = key;
}
