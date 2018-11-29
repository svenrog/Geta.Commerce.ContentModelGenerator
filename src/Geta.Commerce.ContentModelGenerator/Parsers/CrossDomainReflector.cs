using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Geta.Commerce.ContentModelGenerator.Builders;
using Geta.Commerce.ContentModelGenerator.Extensions;
using Geta.Commerce.ContentModelGenerator.Structure;

namespace Geta.Commerce.ContentModelGenerator.Parsers
{
    /// <summary>
    /// A simple class reader that operates on AppDomain level
    /// </summary>
    public class CrossDomainReflector : IDisposable
    {
        protected readonly string Namespace;
        protected readonly string TargetAssemblyPath;

        protected readonly AppDomain Domain;
        protected readonly ReflectionParser Parser;

        protected string AnchorFilePath;
        
        public CrossDomainReflector(string projectPath, string nameSpace)
        {
            var assemblyPath = Path.Combine(projectPath, "bin");
            var filePath = Directory.GetFiles(assemblyPath, "*.dll")
                .FirstOrDefault(x => nameSpace.StartsWith(Path.GetFileNameWithoutExtension(x)));

            Namespace = nameSpace;
            TargetAssemblyPath = filePath;
            Domain = CreateDomain(projectPath);
            Parser = CreateParser(Domain);
        }

        public virtual ClassBuilder[] GetBuilders()
        {
            return Parser.GetBuilders(TargetAssemblyPath, Namespace);
        }

        protected ReflectionParser CreateParser(AppDomain domain)
        {
            var proxyType = typeof(ReflectionParser);
            var proxyAssembly = proxyType.Assembly;
            var proxyAssemblyFileName = Path.GetFileName(proxyAssembly.Location);
            var assemblyName = proxyAssembly.FullName;
            var typeName = proxyType.FullName;
            
            AnchorFilePath = Path.Combine(domain.BaseDirectory, proxyAssemblyFileName);

            File.Copy(proxyAssembly.Location, AnchorFilePath);
            
            return (ReflectionParser) domain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        protected AppDomain CreateDomain(string projectPath)
        {
            var applicationName = "Reflection";
            var assemblyPath = Path.Combine(projectPath, "bin");
            var configurationPath = Path.Combine(projectPath, "web.config");
            var currentDomain = AppDomain.CurrentDomain;
            var evidence = currentDomain.Evidence;
            var domaininfo = new AppDomainSetup
            {
                ApplicationName = applicationName,
                ApplicationBase = projectPath,
                PrivateBinPath = assemblyPath,
                ConfigurationFile = configurationPath
            };

            return AppDomain.CreateDomain(applicationName, evidence, domaininfo);
        }

        public void Dispose()
        {
            if (Domain != null)
            {
                AppDomain.Unload(Domain);
            }

            if (File.Exists(AnchorFilePath))
            {
                File.Delete(AnchorFilePath);
            }   
        }
    }

    public class ReflectionParser : MarshalByRefObject
    {
        public ClassBuilder[] GetBuilders(string assemblyPath, string @namespace)
        {
            var assembly = Assembly.LoadFile(assemblyPath);
            var types = assembly.GetExportedTypes();

            return GetBuilders(types, @namespace);
        }
        
        protected virtual ClassBuilder[] GetBuilders(Type[] types, string @namespace)
        {
            var list = new List<ClassBuilder>();

            foreach (var type in types)
            {
                if (!ShouldRegisterType(type, @namespace)) continue;
                list.Add(GetBuilder(type));
            }

            return list.ToArray();
        }

        protected virtual ClassBuilder GetBuilder(Type type)
        {
            var typeName = type.ToTypeName();
            var builder = new ClassBuilder(typeName, type.Namespace, type.ToInheritsDeclaration(out ISet<string> namespaces));

            var classAttributes = type.GetCustomAttributesData();
            foreach (var attribute in classAttributes)
            {
                if (attribute.AttributeType.Namespace != null && !namespaces.Contains(attribute.AttributeType.Namespace))
                    namespaces.Add(attribute.AttributeType.Namespace);

                var definition = attribute.ToAttributeDefinition();
                builder.ClassAttributes.Add(definition);
            }

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                var definition = new PropertyDefinition
                {
                    Attributes = new List<AttributeDefinition>()
                };

                if (property.PropertyType.Namespace != null && !namespaces.Contains(property.PropertyType.Namespace))
                    namespaces.Add(property.PropertyType.Namespace);

                var attributes = property.GetCustomAttributesData();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeType.Namespace != null && !namespaces.Contains(attribute.AttributeType.Namespace))
                        namespaces.Add(attribute.AttributeType.Namespace);

                    var attributeDefinition = attribute.ToAttributeDefinition();
                    definition.Attributes.Add(attributeDefinition);
                }

                definition.Name = property.Name;
                definition.Type = property.PropertyType.ToTypeName();
                definition.Get = property.GetMethod != null;
                definition.Set = property.SetMethod != null;
                definition.Virtual = property.GetMethod?.IsVirtual ?? false;
                definition.Override = !property.GetMethod?.Equals(property.GetMethod?.GetBaseDefinition()) ?? false;

                builder.Properties.Add(definition);
            }

            foreach (var @namespace in namespaces)
            {
                builder.UsingNameSpaces.Add(@namespace);
            }

            return builder;
        }

        protected virtual bool ShouldRegisterType(Type type, string @namespace)
        {
            if (type == null) return false;
            if (type.IsInterface) return false;
            if (type.IsAbstract) return false;
            if (type.IsEnum) return false;
            if (type.IsPrimitive) return false;

            if (@namespace == null && type.Namespace == null) return true;
            if (type.Namespace == null) return false;
            if (type.Namespace.Equals(@namespace)) return true;

            return false;
        }
    }
};