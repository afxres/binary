namespace Mikodev.Binary;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

public sealed partial class Token
{
    private sealed class MetaObject(Expression parameter, object value) : DynamicMetaObject(parameter, BindingRestrictions.Empty, value)
    {
        private static readonly MethodInfo IndexerMethodInfo = new Func<Token, string, Token>(GetToken).Method;

        private static readonly MethodInfo ConvertMethodInfo = new Func<Token, object>(GetValue<object>).Method.GetGenericMethodDefinition();

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            var type = binder.Type;
            var body = type.IsAssignableFrom(typeof(Token))
                ? Expression.Convert(Expression, type) as Expression
                : Expression.Call(ConvertMethodInfo.MakeGenericMethod(type), Expression.Convert(Expression, typeof(Token)));
            return new DynamicMetaObject(body, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var self = Expression.Convert(Expression, typeof(Token));
            var body = Expression.Call(IndexerMethodInfo, self, Expression.Constant(binder.Name));
            return new DynamicMetaObject(body, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames() => ((Token?)Value)?.Children.Keys ?? [];
    }
}
