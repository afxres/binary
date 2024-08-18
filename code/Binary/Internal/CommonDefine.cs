namespace Mikodev.Binary.Internal;

using System.Reflection;

internal static class CommonDefine
{
    internal const string DebuggerDisplayValue = "{ToString(),nq}";

    internal const string RequiresUnreferencedCodeMessage = "Require public members for binary serialization.";

    internal const BindingFlags PublicInstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public;
}
