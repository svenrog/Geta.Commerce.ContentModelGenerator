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
                return type.Name.GetAlias();
            }
            
            var typeBaseName = type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.InvariantCulture));

            return $"{typeBaseName.GetAlias()}<{string.Join(",", genericArguments.Select(ToTypeName))}>";
        }

        private static string GetAlias(this string typeName)
        {
            if (Constants.TypeAliases.ContainsKey(typeName))
                return Constants.TypeAliases[typeName];

            return typeName;
        }

        public static string ToInheritsDeclaration(this Type type, out ISet<string> namespaces)
        {
            var result = new List<string>();
            namespaces = new HashSet<string>();

            // Todo: Extract generic type constraints
            var constraints = new List<string>();

            if (type.BaseType != null)
            {
                if (type.BaseType.Namespace != null && !namespaces.Contains(type.BaseType.Namespace))
                    namespaces.Add(type.BaseType.Namespace);

                result.Add(type.BaseType.ToTypeName());
            }
            
            var interfaces = type.GetImmediateInterfaces();
            foreach (var @interface in interfaces)
            {
                if (@interface.IsAssignableFrom(type.BaseType))
                    continue;

                if (@interface.Namespace != null && !namespaces.Contains(@interface.Namespace))
                    namespaces.Add(@interface.Namespace);

                result.Add(@interface.ToTypeName());
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

                contents.Add(FormatCustomAttributeArgumentValue(argument));
            }

            if (data.NamedArguments != null)
            {
                foreach (var argument in data.NamedArguments)
                {
                    if (argument.TypedValue.Value == null) continue;

                    contents.Add($"{argument.MemberName} = {FormatCustomAttributeArgumentValue(argument.TypedValue)}");
                }
            }
            
            definition.Name = data.AttributeType.Name.Remove(data.AttributeType.Name.Length - 9);
            definition.Contents = string.Join(", ", contents);

            return definition;
        }

        private static string FormatCustomAttributeArgumentValue(CustomAttributeTypedArgument argument)
        {
            if (typeof(string).IsAssignableFrom(argument.ArgumentType))
                return $"\"{argument.Value}\"";

            if (typeof(Type).IsAssignableFrom(argument.ArgumentType))
                return $"typeof({((Type) argument.Value).ToTypeName()})";

            if (typeof(decimal).IsAssignableFrom(argument.ArgumentType))
                return $"{argument.Value}m";

            if (typeof(float).IsAssignableFrom(argument.ArgumentType))
                return $"{argument.Value}f";

            return $"{argument.Value}";
        }
    }
}
