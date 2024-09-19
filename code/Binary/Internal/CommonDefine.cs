namespace Mikodev.Binary.Internal;

using System.Reflection;

internal static class CommonDefine
{
    internal const string DebuggerDisplayValue = "{ToString(),nq}";

    internal const string RequiresDynamicCodeMessage = "Requires dynamic code for binary serialization.";

    internal const string RequiresUnreferencedCodeMessage = "Requires public members for binary serialization.";

    internal const BindingFlags PublicInstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public;
}
