namespace Mikodev.Binary.SourceGeneration;

using System;

[Flags]
public enum SymbolTypeAliasesFlags
{
    Type = 1,

    ConverterType = 2,

    All = Type | ConverterType,
}
