namespace Mikodev.Binary.Tests.Instance;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Xunit;

public class DynamicTests
{
    private readonly IGenerator generator = Generator.CreateDefault();

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
        var bytes = this.generator.Encode((object)a);
        Assert.NotEmpty(bytes);
        var token = new Token(this.generator, bytes);
        var d = (dynamic)token;
        Assert.Equal(last, (Guid)d.last);
        Assert.Equal(middle, (string)d.middle);
        Assert.Equal(head, (double)d.head);

        var dictionary = token.Children;
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

        var bytes = this.generator.Encode(value);
        var token = new Token(this.generator, bytes);
        var d = (dynamic)token;
        Assert.Equal(value.a, (int)d.a);
        Assert.Equal(value.b, (string)d.b);
        Assert.Equal(value.c.e, (double)d.c.e);
    }

    [Fact(DisplayName = "Dynamic Array")]
    public void Collection()
    {
        var values = Enumerable.Range(0, 10).Select(x => new { id = x, text = $"{x:d2}" }).ToArray();
        var tokens = values.Select(x => new Token(this.generator, this.generator.Encode(x))).ToArray();
        var ds = tokens.Select(x => (dynamic)x).ToArray();

        for (var i = 0; i < values.Length; i++)
        {
            var v = values[i];
            var d = ds[i];
            Assert.Equal(v.id, (int)d.id);
            Assert.Equal(v.text, (string)d.text);
        }
    }

    [Fact(DisplayName = "Property With Null Value")]
    public void PropertyWithNull()
    {
        var value = new { ip = (IPAddress?)null };
        var bytes = this.generator.Encode(value);
        var token = new Token(this.generator, bytes);
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
        var bytes = this.generator.Encode(value);
        var token = new Token(this.generator, bytes);
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
        var bytes = this.generator.Encode(value);
        var token = new Token(this.generator, bytes);
        var d = (dynamic)token;

        Assert.True(ReferenceEquals(token, (object)d));
        Assert.True(ReferenceEquals(token, (Token)d));
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

        var bytes = this.generator.Encode(value);
        var token = new Token(this.generator, bytes);
        var d = (dynamic)token;

        Assert.True(ReferenceEquals(token["a"], (object)d.a));
        Assert.True(ReferenceEquals(token["b"], (object)d.b));
        Assert.True(ReferenceEquals(token["c"]["e"], (object)d.c.e));
    }

    [Fact(DisplayName = "Dynamic Keys")]
    public void DynamicKeys()
    {
        void Test<T>(T value)
        {
            var bytes = this.generator.Encode(value);
            var token = new Token(this.generator, bytes);
            var keys = ((IDynamicMetaObjectProvider)token).GetMetaObject(Expression.Empty()).GetDynamicMemberNames();
            var dictionaryKeys = token.Children.Keys;
            Assert.Equal(dictionaryKeys, keys);
        }

        Test(new
        {
            alpha = 1,
            bravo = "2",
            delta = new[] { 3 },
        });

        Test(new
        {
            id = 0,
            name = string.Empty,
        });
    }

    [Fact(DisplayName = "Dynamic Keys (null instance)")]
    public void DynamicKeysNullValue()
    {
        var types = typeof(Token).GetNestedTypes(BindingFlags.NonPublic);
        var type = types.Single(x => x.Name is "MetaObject");
        var instance = Assert.IsAssignableFrom<DynamicMetaObject>(Activator.CreateInstance(type, [Expression.Parameter(typeof(Token)), null]));
        var keys = instance.GetDynamicMemberNames();
        Assert.Equal([], keys);
        Assert.Null(instance.Value);
        Assert.True(ReferenceEquals(keys, Array.Empty<string>()));
    }

    [Fact(DisplayName = "Dynamic Collection Item")]
    public void DynamicCollectionItem()
    {
        var a = new { id = 1 };
        var b = new { id = 2, tags = Array.Empty<object>() };
        var c = new { id = 3, data = "one" };
        var list = new dynamic[] { a, b, c };
        var converter = this.generator.GetConverter(list);
        _ = Assert.IsAssignableFrom<Converter<object[]>>(converter);
        var buffer = converter.Encode(list);
        var result = converter.Decode(buffer);
        Assert.All(result, x => _ = Assert.IsType<Token>(x));
        var values = result.Select(x => (int)x.id).ToList();
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact(DisplayName = "Dynamic Dictionary Item")]
    public void DynamicDictionaryItem()
    {
        var source = new { id = 1, data = "two", item = new { tags = new[] { "key", "value" } } };
        var buffer = this.generator.Encode(source);
        var result = this.generator.Decode<Dictionary<dynamic, dynamic>>(buffer);
        _ = Assert.IsType<Dictionary<object, object>>(result);
        var values = result.ToDictionary(x => (string)x.Key, x => x.Value);
        var target = (string)(values["item"].tags.key);
        Assert.Equal("value", target);
    }
}
