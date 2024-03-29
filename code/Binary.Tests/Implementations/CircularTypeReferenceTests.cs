﻿namespace Mikodev.Binary.Tests.Implementations;

using System;
using Xunit;

public class CircularTypeReferenceTests
{
    private class LinkedList<T>(T data, LinkedList<T>? next)
    {
        public T Data { get; } = data;

        public LinkedList<T>? Next { get; } = next;

        public LinkedList<T> Add(T data) => new LinkedList<T>(data, this);
    }

    private class A
    {
        public B? Data { get; set; }
    }

    private class B
    {
        public C? Data { get; set; }
    }

    private class C
    {
        public A? Data { get; set; }
    }

    private readonly IGenerator generator = Generator.CreateDefault();

    [Fact(DisplayName = "Circular Type Reference (custom linked list)")]
    public void CircularTypeReferenceLinkedList()
    {
        var list = new LinkedList<int>(2, null).Add(4).Add(6);
        var error = Assert.Throws<ArgumentException>(() => this.generator.GetConverter(list));
        var message = $"Circular type reference detected, type: {typeof(LinkedList<int>)}";
        Assert.Equal(message, error.Message);
    }

    [Fact(DisplayName = "Circular Type Reference")]
    public void CircularTypeReference()
    {
        var data = new A();
        var error = Assert.Throws<ArgumentException>(() => this.generator.Encode(data));
        var message = $"Circular type reference detected, type: {typeof(A)}";
        Assert.Equal(message, error.Message);
    }
}
