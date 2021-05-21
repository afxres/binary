using System;
using System.Reflection;

namespace Mikodev.Binary.Internal.Metadata
{
    internal interface IConverterMetadata
    {
        Type GetGenericArgument();

        MethodInfo GetMethodInfo(string methodName);
    }
}
