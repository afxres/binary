﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal
{
    internal sealed class TokenDebuggerTypeProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<string, Token>[] Items { get; }

        public TokenDebuggerTypeProxy(Token token) => Items = token?.ToArray() ?? Array.Empty<KeyValuePair<string, Token>>();
    }
}
