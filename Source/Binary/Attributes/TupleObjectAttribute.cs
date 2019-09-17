using System;

namespace Mikodev.Binary.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class TupleObjectAttribute : Attribute { }
}
