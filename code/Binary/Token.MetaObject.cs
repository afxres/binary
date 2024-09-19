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

        private static readonly MethodInfo ConvertMethodInfo = new Func<Token, Type, object?>(GetValue).Method;

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            var type = binder.Type;
            var data = Expression;
            if (type.IsAssignableFrom(typeof(Token)) is false)
                data = Expression.Call(ConvertMethodInfo, Expression.Convert(data, typeof(Token)), Expression.Constant(type));
            var body = Expression.Convert(data, type);
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
