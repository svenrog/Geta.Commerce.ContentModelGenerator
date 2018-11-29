using System.Collections.Generic;

namespace Geta.Commerce.ContentModelGenerator
{
    public static class Constants
    {
        public static readonly ISet<string> IgnoreTypes = new HashSet<string>
        {
            "Image",
            "Variant",
            "Numeric",
            "Sysname",
            "StringDictionary",
            "File",
            "ImageFile",
            "MetaObject"
        };

        public static readonly IDictionary<string, string> TypeAliases = new Dictionary<string, string>
        {
            { "Int32", "int" },
            { "Int64", "long" },
            { "Int16", "short" },
            { "Uint32", "uint" },
            { "Uint64", "ulong" },
            { "Uint16", "ushort" },
            { "Byte", "byte" },
            { "SByte", "sbyte" },
            { "Boolean", "bool" },
            { "Float", "float" },
            { "Double", "double" },
            { "Decimal", "decimal" },
            { "String", "string" },
            { "Char", "char" },
            { "Enum", "enum" },
            { "Object", "object" }
        };

        public static readonly IDictionary<string, string> TypeMappings = new Dictionary<string, string>
        {
            { "UniqueIdentifier", "Guid" },
            { "BigInt", "long" },
            { "Binary", "byte[]" },
            { "Bit", "bool" },
            { "Char", "char" },
            { "DateTime", "DateTime" },
            { "Decimal", "double" },
            { "Float", "double" },
            { "Int", "int" },
            { "NChar", "char" },
            { "NText", "string" },
            { "NVarChar", "string" },
            { "SmallDateTime", "DateTime" },
            { "SmallMoney", "Money" },
            { "Text", "string" },
            { "Timestamp", "DateTime" },
            { "Money", "Money" },
            { "Real", "double" },
            { "SmallInt", "int" },
            { "TinyInt", "int" },
            { "VarBinary", "byte[]" },
            { "VarChar", "string" },
            { "Integer", "int" },
            { "Boolean", "bool" },
            { "Email", "string" },
            { "URL", "Url" },
            { "ShortString", "string" },
            { "LongString", "string" },
            { "LongHtmlString", "XhtmlString" },
            { "DictionarySingleValue", "string" },
            { "DictionaryMultiValue", "ItemCollection<string>" },
            { "EnumSingleValue", "string" },
            { "EnumMultiValue", "ItemCollection<string>" }
        };

        // System.dll is already included
        public static readonly IDictionary<string, string> UsingDirectives = new Dictionary<string, string>
        {
            { "Money", "Mediachase.Commerce" },
            { "EnumSingleValue", "EPiServer.Commerce.SpecializedProperties" },
            { "EnumMultiValue", "EPiServer.Commerce.SpecializedProperties" },
            { "DictionaryMultiValue", "EPiServer.Commerce.SpecializedProperties" }
        };

        public static readonly IDictionary<string, string> BackingTypes = new Dictionary<string, string>
        {
            { "EnumSingleValue", "typeof(PropertyDictionarySingle)" },
            { "EnumMultiValue", "typeof(PropertyDictionaryMultiple)" },
            { "DictionaryMultiValue", "typeof(PropertyDictionaryMultiple)" }
        };
    }
}