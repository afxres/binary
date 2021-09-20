namespace Mikodev.Binary.Components;

using System;
using System.Reflection;

internal interface IConverterMetadata
{
    Type GetGenericArgument();

    MethodInfo GetMethod(string name);
}
