using System.Collections.Generic;
using System.IO;
using System.Linq;
using Geta.Commerce.ContentModelGenerator.Builders;
using Geta.Commerce.ContentModelGenerator.Data.Abstraction;
using Geta.Commerce.ContentModelGenerator.Extensions;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator
{
    public class CommerceExporter : Exporter
    {
        public bool GenerateBaseClasses { get; set; }

        public CommerceExporter(string path, string nameSpace) : base(path, nameSpace) {}

        public override void Export()
        {
            base.Export();

            if (!Directory.Exists(ExportDirectoryPath))
            {
                Directory.CreateDirectory(ExportDirectoryPath);
            }

            var builders = GenerateBuilders();

            WriteBuilders(builders);
        }

        protected virtual IList<ClassBuilder> GenerateBuilders()
        {
            var result = new List<ClassBuilder>();
            var metaClasses = GetMetaClasses();

            if (metaClasses == null) return result;

            var metaClassProperties = new Dictionary<MetaClass, IList<MetaField>>();
            var commonProperties = new Dictionary<CommerceContentType, ISet<MetaField>>();
            var baseClasses = new Dictionary<CommerceContentType, string>();

            foreach (var metaClass in metaClasses)
            {
                var fields = GetMetaFieldsForClass(metaClass);
                if (!fields.Any()) continue;

                metaClassProperties.Add(metaClass, GetMetaFieldsForClass(metaClass));    
            }

            if (GenerateBaseClasses)
            {
                var metaGroups = metaClassProperties.GroupBy(x => GetContentType(x.Key));

                foreach (var group in metaGroups)
                {
                    if (!commonProperties.ContainsKey(group.Key))
                        commonProperties.Add(group.Key, new HashSet<MetaField>());

                    foreach (var metaClassKeyValue in group)
                    {
                        foreach (var metaField in metaClassKeyValue.Value)
                        {
                            if (commonProperties[group.Key].Contains(metaField)) continue;
                            if (!group.Select(x => x.Value).All(x => x.Any(y => y.Equals(metaField)))) continue;

                            commonProperties[group.Key].Add(metaField);
                        }
                    }
                }

                foreach (var keyValue in commonProperties)
                {
                    if (!keyValue.Value.Any()) continue;

                    var builder = GetBaseClassBuilder(keyValue.Key, keyValue.Value);

                    baseClasses.Add(keyValue.Key, builder.ClassName);
                    result.Add(builder);
                }
            }

            foreach (var keyValue in metaClassProperties)
            {
                var contentType = GetContentType(keyValue.Key);

                string inherits;
                baseClasses.TryGetValue(contentType, out inherits);

                var builder = GetBuilder(keyValue.Key, 
                                         commonProperties.ContainsKey(contentType) ? 
                                            keyValue.Value.Where(x => !commonProperties[contentType].Contains(x)) :
                                            keyValue.Value,
                                         inherits);
                result.Add(builder);
            }

            return result;
        }

        protected virtual ClassBuilder GetBaseClassBuilder(CommerceContentType type, IEnumerable<MetaField> metaFields)
        {
            var name = string.Format("{0}{1}", type, "ContentBase");
            
            var builder = new CommerceContentModelBuilder(name, NameSpace);

            builder.SetContentType(type);

            AppendMetaFields(builder, metaFields);

            return builder;
        }

        protected virtual ClassBuilder GetBuilder(MetaClass metaClass, IEnumerable<MetaField> metaFields, string inherits = null)
        {
            var name = metaClass.Name.ToClassName();
            var metaName = metaClass.TableName.ToMetaNameFromTable();
            
            var builder = new CommerceContentModelBuilder(name, NameSpace, inherits);

            if (inherits == null)
            {
                var commerceContentType = GetContentType(metaClass);
                builder.SetContentType(commerceContentType);
            }

            var contentTypeAttribute = builder.GetContentTypeAttribute(metaClass.FriendlyName, metaName, metaClass.Description);
            builder.ClassAttributes.Add(contentTypeAttribute);

            AppendMetaFields(builder, metaFields);

            return builder;
        }

        protected virtual void AppendMetaFields(CommerceContentModelBuilder builder, IEnumerable<MetaField> metaFields)
        {
            if (metaFields == null) return;

            foreach (var metaField in metaFields)
            {
                if (!metaField.Type.IsValidType()) continue;

                ValidateBuilderUsingDirective(builder, metaField.Type);

                var attributes = new List<AttributeDefinition>();
                var type = metaField.Type.ToDotNetType();

                attributes.Add(builder.GetDisplayAttributeDefinition(metaField.FriendlyName, metaField.Description));

                if (Constants.BackingTypes.ContainsKey(metaField.Type))
                {
                    attributes.Add(GetBackingTypeAttribute(metaField.Type));
                }

                if (metaField.MultiLanguageValue)
                {
                    attributes.Add(GetCultureSpecificAttribute());
                }

                var property = builder.GetPropertyDefinition(type, metaField.Name, attributes);
                builder.Properties.Add(property);
            }
        }

        protected virtual IList<MetaClass> GetMetaClasses()
        {
            return MetaClass.List();
        }

        protected virtual IList<MetaField> GetMetaFieldsForClass(MetaClass metaClass)
        {
            return MetaField.List(metaClass.Id);
        }

        protected virtual AttributeDefinition GetCultureSpecificAttribute()
        {
            return new AttributeDefinition { Name = "CultureSpecific" };
        }

        protected virtual AttributeDefinition GetBackingTypeAttribute(string type)
        {
            return new AttributeDefinition
            {
                Name = "BackingType",
                Contents = Constants.BackingTypes[type]
            };
        }

        protected virtual CommerceContentType GetContentType(MetaClass metaClass)
        {
            return GetContentType(metaClass.ParentId, metaClass.Name);
        }

        protected virtual CommerceContentType GetContentType(int parent, string name)
        {
            if (parent.Equals(1)) return CommerceContentType.Category;

            if (name.Contains("Bundle")) return CommerceContentType.Bundle;
            if (name.Contains("Item")) return CommerceContentType.Variation;
            if (name.Contains("Product")) return CommerceContentType.Product;

            return CommerceContentType.Entry;
        }

        protected virtual void ValidateBuilderUsingDirective(ClassBuilder builder, string type)
        {
            if (Constants.UsingDirectives.ContainsKey(type))
            {
                var directive = Constants.UsingDirectives[type];
                if (!builder.UsingNameSpaces.Contains(directive))
                {
                    builder.UsingNameSpaces.Add(directive);
                }
            }
        }
    }
}