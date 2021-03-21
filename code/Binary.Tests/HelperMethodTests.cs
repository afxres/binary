using Mikodev.Binary.Tests.Internal;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class HelperMethodTests
    {
        private T GetCommonHelperMethod<T>(string methodName) where T : Delegate
        {
            var invoke = typeof(T).GetMethodNotNull("Invoke", BindingFlags.Instance | BindingFlags.Public);
            var parameterTypes = invoke.GetParameters().Select(x => x.ParameterType).ToArray();
            var type = typeof(Converter).Assembly.GetTypes().Single(x => x.Name is "CommonHelper");
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name == methodName && x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
            return (T)Delegate.CreateDelegate(typeof(T), method);
        }

        [Theory(DisplayName = "Get Method With Flags Error")]
        [InlineData(typeof(HelperMethodTests), "NotExist", BindingFlags.Instance | BindingFlags.Public)]
        [InlineData(typeof(HelperMethodTests), "SomeMethod", BindingFlags.Static | BindingFlags.Public)]
        public void GetMethodWithFlagsError(Type type, string methodName, BindingFlags flags)
        {
            var invoke = GetCommonHelperMethod<Func<Type, string, BindingFlags, MethodInfo>>("GetMethod");
            var error = Assert.Throws<MissingMethodException>(() => invoke.Invoke(type, methodName, flags));
            Assert.Contains(type.Name, error.Message);
            Assert.Contains(methodName, error.Message);
        }

        [Theory(DisplayName = "Get Method With Types Error")]
        [InlineData(typeof(HelperMethodTests), "Maybe", new Type[] { })]
        [InlineData(typeof(HelperMethodTests), "NotSure", new[] { typeof(int) })]
        public void GetMethodWithTypesError(Type type, string methodName, Type[] types)
        {
            var invoke = GetCommonHelperMethod<Func<Type, string, Type[], MethodInfo>>("GetMethod");
            var error = Assert.Throws<MissingMethodException>(() => invoke.Invoke(type, methodName, types));
            Assert.Contains(type.Name, error.Message);
            Assert.Contains(methodName, error.Message);
        }

        [Theory(DisplayName = "Get Field With Flags Error")]
        [InlineData(typeof(HelperMethodTests), "Instance", BindingFlags.Instance | BindingFlags.Public)]
        [InlineData(typeof(HelperMethodTests), "StaticData", BindingFlags.Static | BindingFlags.Public)]
        public void GetFieldWithFlagsError(Type type, string fieldName, BindingFlags flags)
        {
            var invoke = GetCommonHelperMethod<Func<Type, string, BindingFlags, FieldInfo>>("GetField");
            var error = Assert.Throws<MissingFieldException>(() => invoke.Invoke(type, fieldName, flags));
            Assert.Contains(type.Name, error.Message);
            Assert.Contains(fieldName, error.Message);
        }

        [Theory(DisplayName = "Get Property With Flags Error")]
        [InlineData(typeof(HelperMethodTests), "Property", BindingFlags.Instance | BindingFlags.Public)]
        [InlineData(typeof(HelperMethodTests), "DataMember", BindingFlags.Static | BindingFlags.Public)]
        public void GetPropertyWithFlagsError(Type type, string propertyName, BindingFlags flags)
        {
            var invoke = GetCommonHelperMethod<Func<Type, string, BindingFlags, PropertyInfo>>("GetProperty");
            var error = Assert.Throws<MissingMemberException>(() => invoke.Invoke(type, propertyName, flags));
            Assert.Contains(type.Name, error.Message);
            Assert.Contains(propertyName, error.Message);
        }

        [Theory(DisplayName = "Make Upper Case Invariant")]
        [InlineData(null, "")]
        [InlineData("Abc", "ABC")]
        [InlineData("XYZ", "XYZ")]
        public void MakeUpperCaseInvariant(string origin, string expected)
        {
            var type = typeof(Converter).Assembly.GetTypes().Single(x => x.Name is "FallbackAttributesMethods");
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("UpperCase"));
            var invoke = (Func<string, string>)Delegate.CreateDelegate(typeof(Func<string, string>), method);
            var result = invoke.Invoke(origin);
            Assert.Equal(origin?.ToUpperInvariant() ?? string.Empty, result);
            Assert.Equal(expected, result);
        }
    }
}
