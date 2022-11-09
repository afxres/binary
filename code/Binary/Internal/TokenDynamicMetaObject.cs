﻿namespace Mikodev.Binary.Internal;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

internal sealed class TokenDynamicMetaObject : DynamicMetaObject
{
    private static readonly MethodInfo IndexerMethodInfo = CommonModule.GetMethod<Token, Token>(a => a[string.Empty]);

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
    private static readonly MethodInfo ConvertMethodInfo = CommonModule.GetMethod<Token, object>(a => a.As<object>()).GetGenericMethodDefinition();
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

    public TokenDynamicMetaObject(Expression parameter, object value) : base(parameter, BindingRestrictions.Empty, value) { }

    public override DynamicMetaObject BindConvert(ConvertBinder binder)
    {
        var type = binder.Type;
        var body = type.IsAssignableFrom(typeof(Token))
            ? Expression.Convert(Expression, type) as Expression
            : Expression.Call(Expression.Convert(Expression, typeof(Token)), ConvertMethodInfo.MakeGenericMethod(type));
        return new DynamicMetaObject(body, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
    {
        var self = Expression.Convert(Expression, typeof(Token));
        var body = Expression.Call(self, IndexerMethodInfo, Expression.Constant(binder.Name));
        return new DynamicMetaObject(body, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
    }

    public override IEnumerable<string> GetDynamicMemberNames() => ((Token?)Value)?.Children.Keys ?? Array.Empty<string>();
}
