using System.Collections.Generic;
using Geta.Commerce.ContentModelGenerator.Data.Abstraction;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator
{
    public class CommerceInRiverExporter : CommerceExporter
    {
        private const string _metaFilter = "From inRiver";

        public CommerceInRiverExporter(string path, string nameSpace) : base(path, nameSpace) {}

        protected override IList<MetaClass> GetMetaClasses()
        {
            return MetaClass.List(_metaFilter);
        }

        protected override bool ValidatePropertyForBaseClass(CommerceContentType type, string name)
        {
            switch (type)
            {
                case CommerceContentType.Category: return name.StartsWith("Channel") == false;
                default: return true;
            }
        }

        protected override CommerceContentType GetContentType(int parent, string name)
        {
            if (parent.Equals(1)) return CommerceContentType.Category;

            if (name.Contains("Activity")) return CommerceContentType.Product;
            if (name.Contains("Campaign")) return CommerceContentType.Product;
            if (name.Contains("Specification")) return CommerceContentType.Product;
            if (name.Contains("Variation")) return CommerceContentType.Variation;
            if (name.Contains("Bundle")) return CommerceContentType.Bundle;

            if (name.Equals("Product") || name.StartsWith("Product_")) return CommerceContentType.Product;
            if (name.Equals("Item") || name.StartsWith("Item_")) return CommerceContentType.Variation;
            if (name.Equals("Package") || name.StartsWith("Package_")) return CommerceContentType.Package;

            return CommerceContentType.Entry;
        }

        protected override IList<MetaField> GetMetaFieldsForClass(MetaClass metaClass)
        {
            return MetaField.List(metaClass.Id, _metaFilter);
        }
    }
}