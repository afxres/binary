namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class NamedObjectAttribute : Attribute { }
