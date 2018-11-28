using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class DomainReflector
    {
        protected readonly string Namespace;
        protected readonly AppDomain Domain;
        protected readonly DomainProxy DomainProxy;
        protected readonly Configuration Configuration;
        protected readonly AssemblyMappingCollection AssemblyMappings;
        
        public readonly Type[] Types;

        public DomainReflector(string projectPath, string nameSpace)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var assemblyPath = Path.Combine(projectPath, "bin");
            var configurationPath = Path.Combine(projectPath, "web.config");
            var configurationMap = new ConfigurationFileMap(configurationPath);

            Configuration = ConfigurationManager.OpenMappedMachineConfiguration(configurationMap);
            AssemblyMappings = new AssemblyMappingCollection(Configuration);
            Domain = CreateDomain(projectPath);
            Domain.Load(currentAssembly.GetName());

            Namespace = nameSpace;

            var filePath = Directory.GetFiles(assemblyPath, "*.dll")
                                    .FirstOrDefault(x => nameSpace.StartsWith(Path.GetFileNameWithoutExtension(x)));



            var proxyType = typeof(DomainProxy);
            DomainProxy = (DomainProxy) Domain.CreateInstanceAndUnwrap(currentAssembly.FullName, proxyType.FullName);

            var assembly = DomainProxy.GetAssembly(filePath);

            Types = assembly?.GetExportedTypes() ?? new Type[0];
        }

        public virtual ClassBuilder[] GetBuilders(Type[] types)
        {
            var list = new List<ClassBuilder>();

            foreach (var type in types)
            {
                if (!ShouldRegisterType(type)) continue;
                list.Add(GetBuilder(type));
            }

            return list.ToArray();
        }

        protected virtual ClassBuilder GetBuilder(Type type)
        {
            var builder = new ClassBuilder(type.Name, type.Namespace, type.ToInheritsDeclaration(out ISet<string> namespaces));

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

        protected AppDomain CreateDomain(string projectPath)
        {
            var assemblyPath = Path.Combine(projectPath, "bin");
            var configurationPath = Path.Combine(projectPath, "web.config");
            var currentDomain = AppDomain.CurrentDomain;
            var currentAssembly = Assembly.GetExecutingAssembly();
            var evidence = currentDomain.Evidence;
            var domaininfo = new AppDomainSetup
            {
                ApplicationBase = assemblyPath,
                ConfigurationFile = configurationPath,
                PrivateBinPath = Path.GetDirectoryName(currentAssembly.Location)
            };

            var domain = AppDomain.CreateDomain("Reflection", evidence, domaininfo);

            //currentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    var assemblyName = new AssemblyName(args.Name);
            //    var transformedName = AssemblyMappings.GetRedirectedAssemblyName(assemblyName);
            //    var fileName = Path.Combine(assemblyPath, $"{transformedName.Name}.dll");

            //    return DomainProxy.GetAssembly(fileName);
            //};

            return domain;
        }

        protected virtual bool ShouldRegisterType(Type type)
        {
            if (Namespace == null && type.Namespace == null) return true;
            if (type.Namespace.StartsWith(Namespace)) return true;

            return false;
        }

        
    }

    public sealed class DomainProxy : MarshalByRefObject
    {
        public Assembly GetAssembly(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Assembly GetAssembly(string assemblyPath)
        {
            try
            {
                return Assembly.LoadFile(assemblyPath);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
};