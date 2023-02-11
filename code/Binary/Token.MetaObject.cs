namespace Mikodev.Binary;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

public sealed partial class Token
{
    private sealed class MetaObject : DynamicMetaObject
    {
        private static readonly MethodInfo IndexerMethodInfo = new Func<Token, string, Token>(GetToken).Method;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        private static readonly MethodInfo ConvertMethodInfo = new Func<Token, object>(GetValue<object>).Method.GetGenericMethodDefinition();
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        public MetaObject(Expression parameter, object value) : base(parameter, BindingRestrictions.Empty, value) { }

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

        public override IEnumerable<string> GetDynamicMemberNames() => ((Token?)Value)?.Children.Keys ?? Array.Empty<string>();
    }
}
