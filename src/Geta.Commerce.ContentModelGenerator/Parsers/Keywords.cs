using System;
using System.Collections.Generic;

namespace Geta.Commerce.ContentModelGenerator.Parsers
{
    public class Keywords
    {
        public readonly Dictionary<string, string> Organizational = new Dictionary<string, string>
        {
            { "using static", ";" },
            { "using", ";" },
            { "namespace", "{" },
            { "class", "{" },
            { "//", Environment.NewLine },
            { "/*", "*/" }
        };

        public readonly HashSet<string> Modifiers = new HashSet<string>
        {
            "public", "protected", "private",
            "internal", "sealed", "static",
            "abstract", "async", "const",
            "event", "extern", "override",
            "readonly", "unsafe", "virtual",
            "volatile"
        };
    }
}