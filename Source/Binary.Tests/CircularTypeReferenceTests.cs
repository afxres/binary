using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class CircularTypeReferenceTests
    {
        private class LinkedList<T>
        {
            public T Data { get; }

            public LinkedList<T> Next { get; }

            public LinkedList(T data, LinkedList<T> next)
            {
                Data = data;
                Next = next;
            }

            public LinkedList<T> Add(T data) => new LinkedList<T>(data, this);
        }

        private class A
        {
            public B Data { get; set; }
        }

        private class B
        {
            public C Data { get; set; }
        }

        private class C
        {
            public A Data { get; set; }
        }

        private readonly Generator generator = new Generator();

        [Fact(DisplayName = "Linked List")]
        public void GetConverterForLinkedList()
        {
            var list = new LinkedList<int>(2, null).Add(4).Add(6);
            var error = Assert.Throws<ArgumentException>(() => generator.GetConverter(list));
            Assert.Contains("Circular type reference", error.Message);
        }

        [Fact(DisplayName = "Circular Type Reference")]
        public void CircularTypeReference()
        {
            var data = new A();
            var error = Assert.Throws<ArgumentException>(() => generator.Encode(data));
            Assert.Contains("Circular type reference", error.Message);
        }
    }
}
