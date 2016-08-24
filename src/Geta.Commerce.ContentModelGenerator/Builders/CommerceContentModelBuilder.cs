using System;
using System.Collections.Generic;
using Geta.Commerce.ContentModelGenerator.Extensions;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator.Builders
{
    public class CommerceContentModelBuilder : EPiServerContentModelBuilder
    {
        public CommerceContentModelBuilder(string className, string nameSpace, string inherits = null) : base(className, nameSpace, inherits)
        {
            UsingNameSpaces.Add("EPiServer.Commerce.Catalog.DataAnnotations");
            UsingNameSpaces.Add("Mediachase.Commerce");
        }

        public virtual void SetContentType(CommerceContentType type)
        {
            switch (type)
            {
                case CommerceContentType.Product: Inherits = "ProductContent"; break;
                case CommerceContentType.Variation: Inherits = "VariationContent"; break;
                case CommerceContentType.Category: Inherits = "NodeContent"; break;
                case CommerceContentType.Bundle: Inherits = "BundleContent"; break;
                case CommerceContentType.Entry: Inherits = "EntryContentBase"; break;
                case CommerceContentType.Package: Inherits = "PackageContent"; break;
            }

            if (!UsingNameSpaces.Contains("EPiServer.Commerce.Catalog.ContentTypes"))
                UsingNameSpaces.Add("EPiServer.Commerce.Catalog.ContentTypes");
        }

        public override ISet<string> GetProtectedProperties()
        {
            var properties = base.GetProtectedProperties();

            properties.Add("DisplayName");
            properties.Add("Language");
            properties.Add("MasterLanguage");
            properties.Add("StartPublish");
            properties.Add("StopPublish");
            properties.Add("Status");

            return properties;
        }

        public virtual AttributeDefinition GetContentTypeAttribute(string name, string metaclass, string description = null)
        {
            var attribute = base.GetContentTypeAttribute(name, description);

            attribute.Name = "CatalogContentType";
            attribute.Properties.Add("MetaClassName", metaclass.AppendQuotes());

            return attribute;
        }

        #region Hidden Signatures

        new private AttributeDefinition GetContentTypeAttribute(string name, string description = null, int? order = null, string group = null, bool availableInEditMode = true) { return null; }

        #endregion
    }
}