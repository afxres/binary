using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal
{
    internal sealed class TokenDynamicMetaObject : DynamicMetaObject
    {
        private static readonly List<Type> assignableTypes;

        private static readonly MethodInfo convertMethodInfo = typeof(Token).GetMethod(nameof(Token.As), Type.EmptyTypes);

        private static readonly MethodInfo indexerMethodInfo = typeof(Token).GetProperty("Item", new[] { typeof(string) }).GetGetMethod();

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
            var type = binder.Type;
            var body = assignableTypes.Contains(type)
                ? Expression.Convert(Expression, type) as Expression
                : Expression.Call(Expression.Convert(Expression, typeof(Token)), convertMethodInfo.MakeGenericMethod(type));
            return new DynamicMetaObject(body, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var self = Expression.Convert(Expression, typeof(Token));
            var body = Expression.Call(self, indexerMethodInfo, Expression.Constant(binder.Name));
            return new DynamicMetaObject(body, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames() => ((IReadOnlyDictionary<string, Token>)Value).Keys;
    }
}
