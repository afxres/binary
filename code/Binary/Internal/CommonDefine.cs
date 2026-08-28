namespace Mikodev.Binary.Internal;

using System.Reflection;

internal static class CommonDefine
{
    internal const string DebuggerDisplayValue = "{ToString(),nq}";

    internal const string RequiresDynamicCodeMessage = "Dynamic code required for binary serialization.";

    internal const string RequiresUnreferencedCodeMessage = "Public members required for binary serialization.";

    internal const BindingFlags PublicInstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public;
}
