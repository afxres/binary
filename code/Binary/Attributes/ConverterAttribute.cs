using System;

namespace Mikodev.Binary.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ConverterAttribute : Attribute
    {
        public Type Type { get; }

        public ConverterAttribute(Type type) => Type = type;
    }
}
