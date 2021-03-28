﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mikodev.Binary.Benchmarks.CollectionDecodeTests.Models
{
    public delegate T Decoder<out T>(ReadOnlySpan<byte> span);

    public static class ReflectionMethods
    {
        public static Decoder<T> GetDecoder<T, E>(Converter<E> converter, Expression<Action<T>> insert) where T : IEnumerable<E>
        {
            var span = Expression.Parameter(typeof(ReadOnlySpan<byte>), "span");
            var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
            var method = ((MethodCallExpression)insert.Body).Method;
            var empty = typeof(ReadOnlySpan<byte>).GetProperty(nameof(ReadOnlySpan<byte>.IsEmpty));
            var decode = typeof(Converter<E>).GetMethod(nameof(Converter<E>.DecodeAuto));
            var result = Expression.Variable(typeof(T), "result");
            var target = Expression.Label("break");
            var expressions = Expression.Block(
                new[] { result },
                Expression.Assign(result, Expression.New(constructor)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Property(span, empty),
                        Expression.Break(target),
                        Expression.Call(result, method, Expression.Call(Expression.Constant(converter), decode, span))),
                    target),
                result);
            var lambda = Expression.Lambda<Decoder<T>>(expressions, span);
            return lambda.Compile();
        }

        public static Func<IEnumerable<int>, T> GetConstructor<T, E>() where T : IEnumerable<E>
        {
            var constructor = typeof(T).GetConstructor(new[] { typeof(IEnumerable<E>) });
            var source = Expression.Parameter(typeof(IEnumerable<E>), "source");
            var lambda = Expression.Lambda<Func<IEnumerable<int>, T>>(Expression.New(constructor, source), source);
            return lambda.Compile();
        }
    }
}
