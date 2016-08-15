using System.Collections.Generic;

namespace Geta.Commerce.ContentModelGenerator.Structure
{
    public class AttributeDefinition
    {
        public IDictionary<string, string> Properties { get; set; }
        public string Contents { get; set; }
        public string Name { get; set; }
    }
}