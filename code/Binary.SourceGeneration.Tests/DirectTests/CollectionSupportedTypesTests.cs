namespace Mikodev.Binary.SourceGeneration.Tests.DirectTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Xunit;

public class CollectionSupportedTypesTests
{
    // Register 遇到 GetTypeByMetadataName 返回 null 时会静默跳过，
    // 元数据名（尤其反引号后的 arity）写错时登记表只是少一行，所以这里钉住行数。
    [Fact(DisplayName = "Collection Supported Types Count Test")]
    public void CollectionSupportedTypesCountTest()
    {
        var compilation = CompilationModule.CreateCompilation(string.Empty,
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<object>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(LinkedList<object>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ConcurrentStack<object>).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
        ]);
        var types = CollectionSupportedTypes.Create(compilation);
        Assert.Equal(21, types.SupportedCount);
        Assert.Equal(5, types.UnsupportedCount);
    }
}
