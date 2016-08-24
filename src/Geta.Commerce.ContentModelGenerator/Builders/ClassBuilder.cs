using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geta.Commerce.ContentModelGenerator.Comparers;
using Geta.Commerce.ContentModelGenerator.Extensions;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator.Builders
{
    public class ClassBuilder
    {
        public ISet<string> UsingNameSpaces { get; set; }

        public string NameSpace { get; set; }
        public string ClassName { get; set; }
        public string Inherits { get; set; }

        public IList<AttributeDefinition> ClassAttributes { get; set; }
        public IList<PropertyDefinition> Properties { get; set; }

        public ClassBuilder(string className, string nameSpace, string inherits = null)
        {
            ClassName = className;
            NameSpace = nameSpace;
            Inherits = inherits;

            var comparer = new NameSpaceComparer();

            UsingNameSpaces = new SortedSet<string>(comparer);
            ClassAttributes = new List<AttributeDefinition>();
            Properties = new List<PropertyDefinition>();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var currentIndent = 0;

            foreach (var nameSpace in UsingNameSpaces)
            {
                builder.AppendLine(ComposeUsingDeclaration(nameSpace));
            }

            builder.AppendLine();
            builder.AppendLine(ComposeNameSpaceDeclaration(NameSpace));

            builder.AppendLine("{");
            currentIndent++;

            foreach (var attribute in ClassAttributes)
            {
                builder.AppendIndentedLine(currentIndent, ComposeAttribute(attribute));
            }

            builder.AppendIndentedLine(currentIndent, ComposeClassDeclaration(ClassName, Inherits));
            builder.AppendIndentedLine(currentIndent, "{");
            currentIndent++;

            foreach (var property in Properties)
            {
                if (property.Attributes != null)
                {
                    foreach (var attribute in property.Attributes)
                    {
                        builder.AppendIndentedLine(currentIndent, ComposeAttribute(attribute));
                    }

                    builder.AppendIndentedLine(currentIndent, ComposeProperty(property));
                    builder.AppendIndentedLine(currentIndent, string.Empty);
                }
            }

            currentIndent--;
            builder.AppendIndentedLine(currentIndent, "}");
            builder.AppendLine("}");

            return builder.ToString();
        }

        public virtual ISet<string> GetProtectedProperties()
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        protected virtual string ComposeUsingDeclaration(string nameSpace)
        {
            return string.Concat("using ", nameSpace, ";");
        }

        protected virtual string ComposeNameSpaceDeclaration(string nameSpace)
        {
            return string.Concat("namespace", " ", nameSpace);
        }

        protected virtual string ComposeClassDeclaration(string className, string inherits = null)
        {
            var builder = new StringBuilder();
            builder.Append("public class ");
            builder.Append(className);

            if (!string.IsNullOrWhiteSpace(inherits))
            {
                builder.Append(" : ");
                builder.Append(inherits);
            }

            return builder.ToString();
        }

        protected virtual string ComposeAttribute(AttributeDefinition attribute)
        {
            var builder = new StringBuilder();

            builder.Append("[");
            builder.Append(attribute.Name);

            if (attribute.Properties != null)
            {
                builder.Append("(");
                builder.Append(string.Join(", ", attribute.Properties.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Key + " = " + x.Value)));
                builder.Append(")");
            }
            else if (!string.IsNullOrWhiteSpace(attribute.Contents))
            {
                builder.Append("(");
                builder.Append(attribute.Contents);
                builder.Append(")");
            }
            
            builder.Append("]");

            return builder.ToString();
        }

        protected virtual string ComposeProperty(PropertyDefinition property)
        {
            var builder = new StringBuilder();

            builder.Append(property.Protection.ToString().ToLower());
            builder.Append(" ");

            if (property.Virtual)
                builder.Append("virtual ");

            if (property.Override)
                builder.Append("override ");

            builder.Append(property.Type);
            builder.Append(" ");

            builder.Append(property.Name);

            if (property.Get || property.Set)
            {
                builder.Append(" { ");

                if (!property.Get)
                    builder.Append("protected ");

                builder.Append("get; ");

                if (!property.Set)
                    builder.Append("protected ");

                builder.Append("set; }");
            }
            else
            {
                builder.Append(";");
            }

            return builder.ToString();
        }
    }
}