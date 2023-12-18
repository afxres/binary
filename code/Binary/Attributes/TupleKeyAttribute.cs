namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class TupleKeyAttribute(int key) : Attribute
{
    public int Key { get; } = key;
}
