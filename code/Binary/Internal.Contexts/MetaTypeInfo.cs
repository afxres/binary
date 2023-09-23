namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

internal sealed class MetaTypeInfo
{
    private readonly bool required;

    private readonly ImmutableArray<Attribute> attributes;

    private readonly Type type;

    public bool HasRequiredMember => this.required;

    public bool IsNamedObject => this.attributes is { Length: 1 } attributes && attributes.Single() is NamedObjectAttribute;

    public bool IsTupleObject => this.attributes is { Length: 1 } attributes && attributes.Single() is TupleObjectAttribute;

    public Type Type => this.type;

    public ImmutableArray<Attribute> Attributes => this.attributes;

    public MetaTypeInfo(Type type)
    {
        var required = CommonModule.GetAttributes(type, x => x is RequiredMemberAttribute).Any();
        var attributes = CommonModule.GetAttributes(type, a => a is NamedObjectAttribute or TupleObjectAttribute or ConverterAttribute or ConverterCreatorAttribute);
        this.type = type;
        this.required = required;
        this.attributes = attributes;
    }
}
