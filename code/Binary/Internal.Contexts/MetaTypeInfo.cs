namespace Mikodev.Binary.Internal.Contexts;

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
        this.type = type;
        this.required = CommonModule.GetAttributes(type, x => x is RequiredMemberAttribute).Any();
        this.attribute = CommonModule.GetObjectAttributes(type).SingleOrDefault();
    }
}
