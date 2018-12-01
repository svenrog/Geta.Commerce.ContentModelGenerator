using System.Linq;
using Geta.Commerce.ContentModelGenerator.Builders;

namespace Geta.Commerce.ContentModelGenerator.Extensions
{
    public static class ClassBuilderExtensions
    {
        public static ClassBuilder Merge(this ClassBuilder first, ClassBuilder second)
        {
            foreach (var @namespace in second.UsingNamespaces)
            {
                if (first.UsingNamespaces.Contains(@namespace)) continue;
                first.UsingNamespaces.Add(@namespace);
            }

            foreach (var attribute in second.ClassAttributes)
            {
                var existingAttribute = first.ClassAttributes.FirstOrDefault(x => x.Name.Equals(attribute.Name));
                if (existingAttribute == null)
                {
                    first.ClassAttributes.Add(attribute);
                }
                else
                {
                    existingAttribute.Contents = existingAttribute.Contents ?? attribute.Contents;
                    existingAttribute.Properties = existingAttribute.Properties ?? attribute.Properties;
                }
            }

            foreach (var property in second.Properties)
            {
                var existingProperty = first.Properties.FirstOrDefault(x => x.Name.Equals(property.Name));
                if (existingProperty == null)
                {
                    first.Properties.Add(property);
                }
                else
                {
                    foreach (var attribute in property.Attributes)
                    {
                        var existingAttribute = existingProperty.Attributes.FirstOrDefault(x => x.Name.Equals(attribute.Name));
                        if (existingAttribute == null)
                        {
                            existingProperty.Attributes.Add(attribute);
                        }
                        else
                        {
                            existingAttribute.Contents = existingAttribute.Contents ?? attribute.Contents;
                            existingAttribute.Properties = existingAttribute.Properties ?? attribute.Properties;
                        }
                    }

                    existingProperty.Type = property.Type;
                    existingProperty.Get = property.Get;
                    existingProperty.Set = property.Set;
                    existingProperty.Virtual = property.Virtual;
                    existingProperty.Override = property.Override;
                    existingProperty.Protection = property.Protection;
                }
            }

            return first;
        }
    }
}
