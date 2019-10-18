﻿using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;
using Mikodev.Binary.Internal.Contexts;
using Mikodev.Binary.Internal.Delegates;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Creators.External
{
    internal sealed class UnionConverterCreator : IConverterCreator
    {
        private delegate int DecodeUnionTag(ref ReadOnlySpan<byte> span);

        private delegate void EncodeUnionTag(ref Allocator allocator, int item);

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            var flags = FSharpOption<BindingFlags>.None;
            if (!FSharpType.IsUnion(type, flags) || type.IsImplementationOf(typeof(FSharpList<>)))
                return null;
            var cases = FSharpType.GetUnionCases(type, flags);
            if (cases == null || cases.Length == 0)
                throw new ArgumentException($"No available union case found, type: {type}");
            var unionType = cases.Select(x => x.DeclaringType).Distinct().Single();
            if (unionType != type)
                throw new ArgumentException($"Invalid union type, you may have to use union type '{unionType}' instead of case type '{type}'");

            var tagMember = FSharpValue.PreComputeUnionTagMemberInfo(type, flags);
            var noNull = !type.IsValueType && !(tagMember is MethodInfo);
            var toBytes = ToBytesExpression(context, type, cases, tagMember, withMark: false);
            var toValue = ToValueExpression(context, type, cases, withMark: false);
            var toBytesWith = ToBytesExpression(context, type, cases, tagMember, withMark: true);
            var toValueWith = ToValueExpression(context, type, cases, withMark: true);
            var converterType = typeof(UnionConverter<>).MakeGenericType(type);
            var converterArguments = new[] { toBytes, toValue, toBytesWith, toValueWith }.Select(x => x.Compile()).Cast<object>().Concat(new object[] { noNull }).ToArray();
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }

        private static LambdaExpression ToBytesExpression(IGeneratorContext context, Type type, UnionCaseInfo[] caseInfos, MemberInfo tagMember, bool withMark)
        {
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var mark = Expression.Parameter(typeof(int).MakeByRefType(), "mark");
            var item = Expression.Parameter(type, "item");
            var flag = Expression.Variable(typeof(int), "flag");

            List<Expression> MakeBody(Expression instance, PropertyInfo[] properties)
            {
                var result = new List<Expression>(properties.Length);
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    var propertyType = property.PropertyType;
                    var converter = context.GetConverter(propertyType);
                    var method = ContextMethods.GetToBytesMethodInfo(propertyType, withMark || i != properties.Length - 1);
                    var invoke = Expression.Call(Expression.Constant(converter), method, allocator, Expression.Property(instance, property));
                    result.Add(invoke);
                }
                return result;
            }

            Expression MakeCase(UnionCaseInfo info)
            {
                var properties = info.GetFields();
                if (properties == null || properties.Length == 0)
                    return Expression.Empty();
                var dataType = properties.Select(x => x.DeclaringType).Distinct().Single();
                if (dataType == type)
                    return Expression.Block(MakeBody(item, properties));
                var data = Expression.Variable(dataType, "data");
                var convert = Expression.Assign(data, Expression.Convert(item, dataType));
                var list = new List<Expression> { convert };
                list.AddRange(MakeBody(data, properties));
                return Expression.Block(new[] { data }, list);
            }

            var switchCases = caseInfos.Select(x => Expression.SwitchCase(MakeCase(x), Expression.Constant(x.Tag))).ToArray();
            var tagExpression = tagMember is PropertyInfo member
                ? Expression.Property(item, member)
                : (Expression)Expression.Call((MethodInfo)tagMember, item);
            var setMethod = new EncodeUnionTag(PrimitiveHelper.EncodeNumber).Method;
            var defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Empty());
            var block = Expression.Block(new[] { flag },
                Expression.Assign(flag, tagExpression),
                Expression.Call(setMethod, allocator, flag),
                Expression.Switch(flag, defaultBlock, switchCases));
            var delegateType = typeof(OfUnion<>).MakeGenericType(type);
            return Expression.Lambda(delegateType, block, allocator, item, mark);
        }

        private static LambdaExpression ToValueExpression(IGeneratorContext context, Type type, UnionCaseInfo[] caseInfos, bool withMark)
        {
            var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
            var mark = Expression.Parameter(typeof(int).MakeByRefType(), "mark");
            var flag = Expression.Variable(typeof(int), "flag");

            Expression MakeBody(UnionCaseInfo info)
            {
                var constructor = FSharpValue.PreComputeUnionConstructorInfo(info, FSharpOption<BindingFlags>.None);
                var parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                    return Expression.Call(constructor);
                var variables = new ParameterExpression[parameters.Length];
                var list = new List<Expression>();
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    var converter = context.GetConverter(parameterType);
                    var method = ContextMethods.GetToValueMethodInfo(parameterType, withMark || i != parameters.Length - 1);
                    var variable = Expression.Variable(parameterType, $"{i}");
                    variables[i] = variable;
                    var invoke = Expression.Call(Expression.Constant(converter), method, span);
                    list.Add(Expression.Assign(variable, invoke));
                }
                list.Add(Expression.Call(constructor, variables));
                return Expression.Block(variables, list);
            }

            var switchCases = caseInfos.Select(x => Expression.SwitchCase(MakeBody(x), Expression.Constant(x.Tag))).ToArray();
            var getMethod = new DecodeUnionTag(PrimitiveHelper.DecodeNumber).Method;
            var defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Default(type));
            var block = Expression.Block(new[] { flag },
                Expression.Assign(flag, Expression.Call(getMethod, span)),
                Expression.Switch(flag, defaultBlock, switchCases));
            var delegateType = typeof(ToUnion<>).MakeGenericType(type);
            return Expression.Lambda(delegateType, block, span, mark);
        }
    }
}
