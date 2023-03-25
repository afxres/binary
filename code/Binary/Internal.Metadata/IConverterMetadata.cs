namespace Mikodev.Binary.Internal.Metadata;

using System;
using System.Reflection;

internal interface IConverterMetadata
{
    Type GetGenericArgument();

    MethodInfo GetMethod(string name);
}
