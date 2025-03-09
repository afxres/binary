namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class MetaTypeInfo
{
    private readonly bool required;

    private readonly Attribute? attribute;

    private readonly Type type;

    public bool HasRequiredMember => this.required;

    public Type Type => this.type;

    public Attribute? Attribute => this.attribute;

    public MetaTypeInfo(Type type)
    {
        var required = CommonModule.GetAttributes(type, x => x is RequiredMemberAttribute).Any();
        var attributes = CommonModule.GetAttributes(type, a => a is NamedObjectAttribute or TupleObjectAttribute or ConverterAttribute or ConverterCreatorAttribute);
        if (attributes.Length > 1)
            throw new ArgumentException($"Multiple attributes found, type: {type}");
        this.type = type;
        this.required = required;
        this.attribute = attributes.SingleOrDefault();
    }
}
