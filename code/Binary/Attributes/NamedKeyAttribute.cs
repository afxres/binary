using System;

namespace Mikodev.Binary.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NamedKeyAttribute : Attribute
    {
        public string Key { get; }

        public NamedKeyAttribute(string key) => Key = key;
    }
}
