using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator.Extensions
{
    public static class TypeExtensions
    {
        public static string ToTypeName(this Type type)
        {
            var genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                return type.Name;
            }
            
            var typeBaseName = type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.InvariantCulture));

            return $"{typeBaseName}<{string.Join(",", genericArguments.Select(ToTypeName))}>";
        }

        public static string ToInheritsDeclaration(this Type type)
        {
            var result = new List<string>();

            // Todo: Extract generic type constraints
            var constraints = new List<string>();

            if (type.BaseType != null)
                result.Add(type.BaseType.ToTypeName());

            var interfaces = type.GetImmediateInterfaces();
            foreach (var face in interfaces)
            {
                if (face.IsAssignableFrom(type.BaseType))
                    continue;

                result.Add(face.ToTypeName());
            }

            return string.Join(", ", result);
        }

        public static IEnumerable<Type> GetImmediateInterfaces(this Type type)
        {
            IEnumerable<Type> interfaces = type.GetInterfaces();
            interfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()));

            return new HashSet<Type>(interfaces);
        }

        public static AttributeDefinition ToAttributeDefinition(this CustomAttributeData data)
        {
            var definition = new AttributeDefinition();
            var contents = new List<string>();

            foreach (var argument in data.ConstructorArguments)
            {
                if (argument.Value == null) continue;

                contents.Add($"{argument.Value}");
            }

            if (data.NamedArguments != null)
            {
                foreach (var argument in data.NamedArguments)
                {
                    if (argument.TypedValue.Value == null) continue;

                    contents.Add($"{argument.MemberName} = {argument.TypedValue.Value}");
                }
            }
            
            definition.Name = data.AttributeType.Name.Remove(data.AttributeType.Name.Length - 9);
            definition.Contents = string.Join(", ", contents);

            return definition;
        }
    }
}
