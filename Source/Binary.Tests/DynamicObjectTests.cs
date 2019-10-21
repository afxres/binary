using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class DynamicObjectTests
    {
        private readonly IGenerator generator = new GeneratorBuilder().AddDefaultConverterCreators().Build();

        [Fact(DisplayName = "Expando Object")]
        public void ExpandoObject()
        {
            var head = 3.1;
            var middle = "zero";
            var last = Guid.NewGuid();
            var a = (dynamic)new ExpandoObject();
            a.head = head;
            a.middle = middle;
            a.last = last;
            var bytes = generator.Encode((object)a);
            Assert.NotEmpty(bytes);
            var token = new Token(generator, bytes);
            var d = (dynamic)token;
            Assert.Equal(head, (double)d.head);
            Assert.Equal(middle, (string)d.middle);
            Assert.Equal(last, (Guid)d.last);

            var dictionary = (IReadOnlyDictionary<string, Token>)token;
            Assert.Equal(3, dictionary.Count);
            _ = Assert.Throws<KeyNotFoundException>(() => { _ = d.none; });
        }

        [Fact(DisplayName = "Property")]
        public void Property()
        {
            var value = new
            {
                a = 1,
                b = "two",
                c = new
                {
                    e = 2.3,
                }
            };

            var bytes = generator.Encode(value);
            var token = new Token(generator, bytes);
            var d = (dynamic)token;
            Assert.Equal(value.a, (int)d.a);
            Assert.Equal(value.b, (string)d.b);
            Assert.Equal(value.c.e, (double)d.c.e);
        }

        [Fact(DisplayName = "Property With Null Value")]
        public void PropertyWithNull()
        {
            var value = new
            {
                ip = (IPAddress)null,
            };
            var bytes = generator.Encode(value);
            var token = new Token(generator, bytes);
            var d = (dynamic)token;
            var ip = d.ip;
            Assert.NotNull((object)ip);
            Assert.Null((IPAddress)ip);
        }

        [Fact(DisplayName = "Property Not Found")]
        public void PropertyNotFound()
        {
            var value = new
            {
                some = 1,
            };
            var bytes = generator.Encode(value);
            var token = new Token(generator, bytes);
            var d = (dynamic)token;
            Assert.Equal(1, (int)d.some);
            _ = Assert.Throws<KeyNotFoundException>(() => (string)d.none);
        }

        [Fact(DisplayName = "Convert Directly")]
        public void Convert()
        {
            var value = new
            {
                some = 1,
            };
            var bytes = generator.Encode(value);
            var token = new Token(generator, bytes);
            var d = (dynamic)token;

            Assert.True(ReferenceEquals(token, (object)d));
            Assert.True(ReferenceEquals(token, (Token)d));
            Assert.True(ReferenceEquals(token, (IReadOnlyDictionary<string, Token>)d));
            Assert.True(ReferenceEquals(token, (IDynamicMetaObjectProvider)d));
        }

        [Fact(DisplayName = "Property Reference Equals")]
        public void PropertyReferenceEquals()
        {
            var value = new
            {
                a = 1,
                b = "two",
                c = new
                {
                    e = 2.3,
                }
            };

            var bytes = generator.Encode(value);
            var token = new Token(generator, bytes);
            var d = (dynamic)token;

            Assert.True(ReferenceEquals(token["a"], (object)d.a));
            Assert.True(ReferenceEquals(token["b"], (object)d.b));
            Assert.True(ReferenceEquals(token["c"]["e"], (object)d.c.e));
        }
    }
}
