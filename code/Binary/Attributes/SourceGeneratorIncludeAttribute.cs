namespace Mikodev.Binary.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class SourceGeneratorIncludeAttribute<T> : Attribute { }
