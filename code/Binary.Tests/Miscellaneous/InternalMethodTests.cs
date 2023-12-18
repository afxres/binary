namespace Mikodev.Binary.Tests.Miscellaneous;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

public class InternalMethodTests
{
    private static T GetCommonModuleMethod<T>(string methodName) where T : Delegate
    {
        var invoke = typeof(T).GetMethodNotNull("Invoke", BindingFlags.Instance | BindingFlags.Public);
        var parameterTypes = invoke.GetParameters().Select(x => x.ParameterType).ToArray();
        var type = typeof(Converter).Assembly.GetTypes().Single(x => x.Name is "CommonModule");
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains(methodName) && x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        return (T)Delegate.CreateDelegate(typeof(T), method);
    }

    [Theory(DisplayName = "Get Public Instance Method Error")]
    [InlineData(typeof(InternalMethodTests), "NotExist")]
    [InlineData(typeof(InternalMethodTests), "SomeMethod")]
    public void GetPublicInstanceMethodError(Type type, string methodName)
    {
        var invoke = GetCommonModuleMethod<Func<Type, string, MethodInfo>>("GetPublicInstanceMethod");
        var error = Assert.Throws<MissingMethodException>(() => invoke.Invoke(type, methodName));
        var message = $"Method not found, method name: {methodName}, type: {type}";
        Assert.Equal(message, error.Message);
        Assert.Contains(type.Name, error.Message);
        Assert.Contains(methodName, error.Message);
    }

    [Theory(DisplayName = "Get Public Instance Field Error")]
    [InlineData(typeof(InternalMethodTests), "Instance")]
    [InlineData(typeof(InternalMethodTests), "StaticData")]
    public void GetPublicInstanceFieldError(Type type, string fieldName)
    {
        var invoke = GetCommonModuleMethod<Func<Type, string, FieldInfo>>("GetPublicInstanceField");
        var error = Assert.Throws<MissingFieldException>(() => invoke.Invoke(type, fieldName));
        var message = $"Field not found, field name: {fieldName}, type: {type}";
        Assert.Equal(message, error.Message);
        Assert.Contains(type.Name, error.Message);
        Assert.Contains(fieldName, error.Message);
    }

    [Theory(DisplayName = "Get Public Instance Property Error")]
    [InlineData(typeof(InternalMethodTests), "Property")]
    [InlineData(typeof(InternalMethodTests), "DataMember")]
    public void GetPublicInstancePropertyError(Type type, string propertyName)
    {
        var invoke = GetCommonModuleMethod<Func<Type, string, PropertyInfo>>("GetPublicInstanceProperty");
        var error = Assert.Throws<MissingMemberException>(() => invoke.Invoke(type, propertyName));
        var message = $"Property not found, property name: {propertyName}, type: {type}";
        Assert.Equal(message, error.Message);
        Assert.Contains(type.Name, error.Message);
        Assert.Contains(propertyName, error.Message);
    }

    [Theory(DisplayName = "Get Public Instance Constructor With Types Error")]
    [InlineData(typeof(InternalMethodTests), new[] { typeof(int) })]
    [InlineData(typeof(InternalMethodTests), new[] { typeof(string) })]
    public void GetPublicInstanceConstructorError(Type type, Type[] types)
    {
        var invoke = GetCommonModuleMethod<Func<Type, Type[], ConstructorInfo>>("GetPublicInstanceConstructor");
        var error = Assert.Throws<MissingMethodException>(() => invoke.Invoke(type, types));
        var message = $"Constructor not found, type: {type}";
        Assert.Equal(message, error.Message);
        Assert.Contains(type.Name, error.Message);
    }

    [Theory(DisplayName = "Create Instance With Null Result")]
    [InlineData(typeof(int?), null)]
    [InlineData(typeof(long?), null)]
    public void CreateInstanceWithNull(Type type, object[]? arguments)
    {
        var invoke = GetCommonModuleMethod<Func<Type, object[]?, object>>("CreateInstance");
        var error = Assert.Throws<InvalidOperationException>(() => invoke.Invoke(type, arguments));
        var message = $"Invalid null instance detected, type: {type}";
        Assert.Equal(message, error.Message);
    }

    [Fact(DisplayName = "Create Instance Internal Invoke Method Test")]
    public void CreateInstanceInternalInvokeMethodTest()
    {
        var invoke = GetCommonModuleMethod<Func<Func<object>, object>>("Invoke");
        var a = Assert.Throws<NotSupportedException>(() => invoke.Invoke(() => throw new TargetInvocationException(new NotSupportedException("Text 01"))));
        Assert.Equal("Text 01", a.Message);
        var b = Assert.Throws<TargetInvocationException>(() => invoke.Invoke(() => throw new TargetInvocationException("Text 02", null)));
        Assert.Null(b.InnerException);
        Assert.Equal("Text 02", b.Message);
    }
}
