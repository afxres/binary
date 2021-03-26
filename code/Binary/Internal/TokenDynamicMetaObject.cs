using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal
{
    internal sealed class TokenDynamicMetaObject : DynamicMetaObject
    {
        private static readonly MethodInfo IndexerMethodInfo = CommonHelper.GetMethod<Token, Token>(a => a[null]);

        private static readonly MethodInfo ConvertMethodInfo = CommonHelper.GetMethod<Token, object>(a => a.As<object>()).GetGenericMethodDefinition();

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

        public override IEnumerable<string> GetDynamicMemberNames() => ((Token)Value)?.Children.Keys ?? Array.Empty<string>();
    }
}
