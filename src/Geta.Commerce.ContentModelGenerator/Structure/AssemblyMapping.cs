using System;

namespace Geta.Commerce.ContentModelGenerator.Structure
{
    public class AssemblyMapping
    {
        public string Name { get; set; }
        public string PublicKeyToken { get; set; }
        public string Culture { get; set; }

        public VersionRange Range { get; set; }
        public Version To { get; set; }
    }
}