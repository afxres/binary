namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class MetaMemberInfo
{
    private readonly bool optional;

    private readonly bool @readonly;

    private readonly Attribute? key;

    private readonly Attribute? conversion;

    private readonly MemberInfo member;

    private readonly ContextMemberInitializer initializer;

    public bool IsOptional => this.optional;

    public bool IsReadOnly => this.@readonly;

    public string Name => this.member.Name;

    public Attribute? KeyAttribute => this.key;

    public Attribute? ConverterOrConverterCreatorAttribute => this.conversion;

    public ContextMemberInitializer Initializer => this.initializer;

    public Type Type => this.member is FieldInfo field ? @field.FieldType : ((PropertyInfo)this.member).PropertyType;

    public MetaMemberInfo(MemberInfo member, Attribute? key, Attribute? conversion, bool optional)
    {
        static ContextMemberInitializer Invoke(MemberInfo member)
        {
            if (member is FieldInfo field)
                return e => Expression.Field(e, field);
            var property = (PropertyInfo)member;
            return e => Expression.Property(e, property);
        }

        Debug.Assert(member is FieldInfo || ((PropertyInfo)member).GetGetMethod() is not null);
        Debug.Assert(key is null or NamedKeyAttribute or TupleKeyAttribute);
        Debug.Assert(conversion is null or ConverterAttribute or ConverterCreatorAttribute);
        this.optional = optional;
        this.@readonly = (member is FieldInfo field && field.IsInitOnly) || (member is PropertyInfo property && property.GetSetMethod() is null);
        this.key = key;
        this.conversion = conversion;
        this.member = member;
        this.initializer = Invoke(member);
    }
}
