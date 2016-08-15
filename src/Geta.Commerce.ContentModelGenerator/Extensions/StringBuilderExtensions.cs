using System.Text;

namespace Geta.Commerce.ContentModelGenerator.Extensions
{
    public static class StringBuilderExtensions
    {
        private const string _indent = "    ";

        public static StringBuilder AppendIndentedLine(this StringBuilder builder, int indent, string line)
        {
            builder.AppendIndent(indent);
            return builder.AppendLine(line);
        }

        public static StringBuilder AppendIndent(this StringBuilder builder, int indent)
        {
            for (var i = 0; i < indent; i++)
            {
                builder = builder.Append(_indent);
            }

            return builder;
        }
    }
}