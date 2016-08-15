using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Geta.Commerce.ContentModelGenerator.Extensions;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator.Builders
{
    public class EPiServerContentModelBuilder : ClassBuilder
    {
        public EPiServerContentModelBuilder(string className, string nameSpace, string inherits = null) : base(className, nameSpace, inherits)
        {
            UsingNameSpaces.Add("System");
            UsingNameSpaces.Add("System.Collections.Generic");
            UsingNameSpaces.Add("System.ComponentModel.DataAnnotations");
            UsingNameSpaces.Add("EPiServer");
            UsingNameSpaces.Add("EPiServer.Core");
            UsingNameSpaces.Add("EPiServer.DataAnnotations");
        }

        protected virtual Guid GetContentTypeGuid()
        {
            var input = string.Concat(NameSpace, ".", ClassName);
            var bytes = Encoding.UTF8.GetBytes(input);

            Guid result;

            using (var hasher = MD5.Create())
            {
                var hash = hasher.ComputeHash(bytes);
                result = new Guid(hash);
            }

            return result;
        }

        public virtual AttributeDefinition GetContentTypeAttribute(string name, string description = null, int? order = null, string group = null, bool availableInEditMode = true)
        {
            var properties = new Dictionary<string, string>
            {
                { "DisplayName", name.AppendQuotes() },
                { "GUID", GetContentTypeGuid().ToString("D").ToUpper().AppendQuotes() }
            };

            if (!string.IsNullOrWhiteSpace(description))
                properties.Add("Description", description.AppendQuotes());

            if (order.HasValue)
                properties.Add("Order", order.ToString());

            if (!string.IsNullOrWhiteSpace(group))
                properties.Add("GroupName", group.AppendQuotes());

            return new AttributeDefinition
            {
                Name = "ContentType",
                Properties = properties
            };
        }

        public virtual PropertyDefinition GetPropertyDefinition(string type, string name, IList<AttributeDefinition> attributes = null)
        {
            return new PropertyDefinition
            {
                Name = name,
                Type = type,
                Attributes = attributes,
                Virtual = true,
                Get = true,
                Set = true
            };
        }

        public virtual AttributeDefinition GetDisplayAttributeDefinition(string name, string description, int? order = null, string group = null)
        {
            var properties = new Dictionary<string, string>
            {
                { "Name", name.AppendQuotes() },
                { "Description", description.AppendQuotes() }
            };

            if (!string.IsNullOrWhiteSpace(group))
                properties.Add("GroupName", group.AppendQuotes());

            if (order.HasValue)
                properties.Add("Order", order.ToString());

            return new AttributeDefinition
            {
                Name = "Display",
                Properties = properties
            };
        }
    }
}