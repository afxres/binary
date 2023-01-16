namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

internal sealed class MetaMemberInfo
{
    private readonly bool optional;

    private readonly Attribute? key;

    private readonly Attribute? conversion;

    private readonly MemberInfo member;

    private readonly MethodInfo? setter;

    private readonly ContextMemberInitializer initializer;

    public bool IsOptional => this.optional;

    public bool IsWriteable => this.setter is not null || this.member is FieldInfo;

    public string Name => this.member.Name;

    public Attribute? KeyAttribute => this.key;

    public Attribute? ConverterOrConverterCreatorAttribute => this.conversion;

    public ContextMemberInitializer Initializer => this.initializer;

    public Type Type => this.member is FieldInfo field ? field.FieldType : ((PropertyInfo)this.member).PropertyType;

    public MetaMemberInfo(MemberInfo member, Attribute? key, Attribute? conversion, bool optional)
    {
        static ContextMemberInitializer Invoke(MemberInfo member)
        {
            if (member is FieldInfo field)
                return e => Expression.Field(e, field);
            var property = (PropertyInfo)member;
            return e => Expression.Property(e, property);
        }

        Debug.Assert(member is FieldInfo or PropertyInfo);
        Debug.Assert(member is FieldInfo || ((PropertyInfo)member).GetGetMethod() is not null);
        Debug.Assert(key is null or NamedKeyAttribute or TupleKeyAttribute);
        Debug.Assert(conversion is null or ConverterAttribute or ConverterCreatorAttribute);
        this.optional = optional;
        this.key = key;
        this.conversion = conversion;
        this.member = member;
        this.setter = (member as PropertyInfo)?.GetSetMethod();
        this.initializer = Invoke(member);
    }
}
