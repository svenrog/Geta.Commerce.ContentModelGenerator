using System;

namespace Geta.Commerce.ContentModelGenerator.Extensions
{
    public static class StringExtensions
    {
        public static string AppendQuotes(this string input)
        {
            if (input == null) return null;
            if (input.Equals(string.Empty)) return "\"\"";

            return string.Concat("\"", input, "\"");
        }

        public static string ToMetaNameFromTable(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            if (name.StartsWith("CatalogEntryEx_"))
                return name.Substring(15);

            if (name.StartsWith("CatalogNodeEx_"))
                return name.Substring(14);

            return name;
        }

        public static string ToDotNetType(this string type)
        {
            if (type == null) return null;
            if (Constants.TypeMappings.ContainsKey(type)) return Constants.TypeMappings[type];
            return type;
        }

        public static string ToFileName(this string name)
        {
            if (name == null) return name;
            var index = name.IndexOf("<", StringComparison.InvariantCultureIgnoreCase);
            return index < 0 ? name : name.Substring(0, index);
        }

        public static string ToClassName(this string name)
        {
            if (name == null) return null;
            var index = name.LastIndexOf("_", StringComparison.InvariantCulture);
            return index < 0 ? name : name.Substring(index + 1);
        }

        public static bool IsValidType(this string type)
        {
            if (type == null) return false;
            if (Constants.IgnoreTypes.Contains(type)) return false;

            return true;
        }
    }
}