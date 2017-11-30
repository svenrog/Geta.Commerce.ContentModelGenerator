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

        public override void Export(IEnumerable<ClassBuilder> builders)
        {
            var classBuilders = builders as ClassBuilder[] ?? builders.ToArray();

            base.Export(classBuilders);

            if (!Directory.Exists(ExportDirectoryPath))
            {
                Directory.CreateDirectory(ExportDirectoryPath);
            }

            WriteBuilders(classBuilders);
        }

        protected virtual bool ValidatePropertyForBaseClass(CommerceContentType type, string name)
        {
            return true;
        }

        public virtual IEnumerable<ClassBuilder> GenerateBuilders(IDictionary<string, ClassBuilder> existingBuilders = null)
        {
            var result = existingBuilders ?? new Dictionary<string, ClassBuilder>();
            var metaClasses = GetMetaClasses();

            if (metaClasses == null) return result.Values;

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
                            if (!ValidatePropertyForBaseClass(group.Key, metaField.Name)) continue;
                            if (!group.Select(x => x.Value).All(x => x.Any(y => y.Equals(metaField)))) continue;

                            commonProperties[group.Key].Add(metaField);
                        }
                    }
                }

                foreach (var keyValue in commonProperties)
                {
                    if (!keyValue.Value.Any()) continue;

                    var builder = GetBaseClassBuilder(keyValue.Key, keyValue.Value);
                    var fullname = GetFullName(builder);

                    baseClasses.Add(keyValue.Key, builder.ClassName);

                    if (result.ContainsKey(fullname))
                    {
                        result[fullname].Merge(builder);
                    }
                    else
                    {
                        result.Add(fullname, builder);
                    }
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

                var fullname = GetFullName(builder);
                if (result.ContainsKey(fullname))
                {
                    result[fullname].Merge(builder);
                }
                else
                {
                    result.Add(fullname, builder);
                }
            }

            return result.Values;
        }

        protected virtual string GetFullName(ClassBuilder builder)
        {
            return $"{NameSpace}.{builder.ClassName}";
        }

        protected virtual ClassBuilder GetBaseClassBuilder(CommerceContentType type, IEnumerable<MetaField> metaFields)
        {
            var builder = new CommerceContentModelBuilder($"{type}ContentBase", NameSpace);

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

            var protectedProperties = builder.GetProtectedProperties();

            foreach (var metaField in metaFields)
            {
                if (protectedProperties.Contains(metaField.Name)) continue;
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

            if (name.Contains("Activity")) return CommerceContentType.Product;
            if (name.Contains("Campaign")) return CommerceContentType.Product;
            if (name.Contains("Specification")) return CommerceContentType.Product;
            if (name.Contains("Product")) return CommerceContentType.Product;
            if (name.Contains("Package")) return CommerceContentType.Package;
            if (name.Contains("Item")) return CommerceContentType.Variation;
            if (name.Contains("Variation")) return CommerceContentType.Variation;
            if (name.Contains("Bundle")) return CommerceContentType.Bundle;

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