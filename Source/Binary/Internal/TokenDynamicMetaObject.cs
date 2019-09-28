using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;

namespace Mikodev.Binary.Internal
{
    internal sealed class TokenDynamicMetaObject : DynamicMetaObject
    {
        private static readonly List<Type> assignableTypes;

        static TokenDynamicMetaObject()
        {
            var type = typeof(Token);
            var collection = new List<Type>(type.GetInterfaces());
            do
                collection.Add(type);
            while ((type = type.BaseType) != null);
            assignableTypes = collection;
            Debug.Assert(assignableTypes.Count == 7);
            Debug.Assert(new HashSet<Type>(assignableTypes).Count == 7);
        }

        public TokenDynamicMetaObject(Expression parameter, object value) : base(parameter, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            var value = Value;
            var type = binder.Type;
            if (!assignableTypes.Contains(type))
                value = ((Token)value).As(type);
            var constant = Expression.Constant(value, type);
            return new DynamicMetaObject(constant, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ((Token)Value).GetTokens().Keys;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var value = ((Token)Value).GetTokens()[binder.Name];
            var constant = Expression.Constant(value);
            return new DynamicMetaObject(constant, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }
    }
}
