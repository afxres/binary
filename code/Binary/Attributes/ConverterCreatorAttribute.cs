namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ConverterCreatorAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
}
