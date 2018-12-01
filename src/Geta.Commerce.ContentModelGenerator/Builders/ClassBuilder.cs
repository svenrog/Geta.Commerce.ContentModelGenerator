using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geta.Commerce.ContentModelGenerator.Comparers;
using Geta.Commerce.ContentModelGenerator.Extensions;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator.Builders
{
    [Serializable]
    public class ClassBuilder
    {
        public ISet<string> UsingNamespaces { get; set; }

        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string Inherits { get; set; }

        public IList<string> Constraints { get; set; }
        public IList<AttributeDefinition> ClassAttributes { get; set; }
        public IList<PropertyDefinition> Properties { get; set; }

        public ClassBuilder(string className, string @namespace, string inherits = null, IList<string> constraints = null)
        {
            ClassName = className;
            Namespace = @namespace;
            Inherits = inherits;
            Constraints = constraints ?? new List<string>(0);

            var comparer = new NameSpaceComparer();

            UsingNamespaces = new SortedSet<string>(comparer);
            ClassAttributes = new List<AttributeDefinition>();
            Properties = new List<PropertyDefinition>();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var currentIndent = 0;

            foreach (var @namespace in UsingNamespaces)
            {
                builder.AppendLine(ComposeUsingDeclaration(@namespace));
            }

            builder.AppendLine();
            builder.AppendLine(ComposeNameSpaceDeclaration(Namespace));

            builder.AppendLine("{");
            currentIndent++;

            foreach (var attribute in ClassAttributes)
            {
                builder.AppendIndentedLine(currentIndent, ComposeAttribute(attribute));
            }

            if (string.IsNullOrEmpty(Inherits) && Constraints.Count == 1)
            {
                builder.AppendIndent(currentIndent);
                builder.Append(ComposeClassDeclaration(ClassName));
                builder.Append(" ");
                builder.AppendLine(ComposeConstraint(Constraints[0]));
            }
            else
            {
                builder.AppendIndentedLine(currentIndent, ComposeClassDeclaration(ClassName, Inherits));
                currentIndent++;
                foreach (var constraint in Constraints)
                {
                    builder.AppendIndentedLine(currentIndent, ComposeConstraint(constraint));
                }
                currentIndent--;
            }
            
            builder.AppendIndentedLine(currentIndent, "{");
            currentIndent++;

            foreach (var property in Properties)
            {
                if (property.Attributes == null) continue;

                foreach (var attribute in property.Attributes)
                {
                    builder.AppendIndentedLine(currentIndent, ComposeAttribute(attribute));
                }

                builder.AppendIndentedLine(currentIndent, ComposeProperty(property));
                builder.AppendIndentedLine(currentIndent, string.Empty);
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

        protected virtual string ComposeUsingDeclaration(string @namespace)
        {
            return string.Concat("using ", @namespace, ";");
        }

        protected virtual string ComposeNameSpaceDeclaration(string @namespace)
        {
            return string.Concat("namespace ", @namespace);
        }

        public virtual string ComposeConstraint(string constraint)
        {
            return string.Concat("where ", constraint);
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