namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
using System;
using System.Diagnostics;

internal sealed class MetaTypeInfo
{
    private readonly bool required;

    private readonly Attribute? attribute;

    private readonly Type type;

    public bool HasRequiredMember => this.required;

    public bool IsNamedObject => this.attribute is NamedObjectAttribute;

    public bool IsTupleObject => this.attribute is TupleObjectAttribute;

    public Type Type => this.type;

    public MetaTypeInfo(Type type, Attribute? attribute, bool required)
    {
        Debug.Assert(attribute is null or NamedObjectAttribute or TupleObjectAttribute or ConverterAttribute or ConverterCreatorAttribute);
        this.type = type;
        this.required = required;
        this.attribute = attribute;
    }
}
