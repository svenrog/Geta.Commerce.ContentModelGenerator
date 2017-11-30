using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Geta.Commerce.ContentModelGenerator.Builders;
using Geta.Commerce.ContentModelGenerator.Extensions;
using Geta.Commerce.ContentModelGenerator.Structure;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

namespace Geta.Commerce.ContentModelGenerator.Parsers
{
    /// <summary>
    /// A simple class parser that handles a single namespace and class inside a file
    /// </summary>
    public class ClassCompiler
    {
        protected readonly CodeDomProvider CodeProvider;
        protected readonly CompilerParameters CompilerParameters;
        protected readonly string Namespace;
        protected readonly string OutputFile;
        protected readonly string[] Assemblies;
        protected StreamReader Reader;

        public ClassCompiler(string assemblyPath, string nameSpace)
        {
            if (!Directory.Exists(AssemblyDirectory))
            {
                Directory.CreateDirectory(AssemblyDirectory);
            }

            CodeProvider = new CSharpCodeProvider();
            OutputFile = $"{AssemblyDirectory}\\Geta.Commerce.ContentModelGenerator.Generated.dll";

            CompilerParameters = new CompilerParameters
            {
                CompilerOptions = "/nostdlib",
                OutputAssembly = OutputFile
            };

            Assemblies = Directory.GetFiles(assemblyPath, "*.dll")
                                  .Where(x => !x.EndsWith("System.Runtime.dll"))
                                  .ToArray();

            CompilerParameters.ReferencedAssemblies.Add("System.ComponentModel.DataAnnotations.dll");
            CompilerParameters.ReferencedAssemblies.AddRange(Assemblies);

            Namespace = nameSpace;
        }
        
        public ClassBuilder[] ParseFiles(string[] files)
        {
            var result = CodeProvider.CompileAssemblyFromFile(CompilerParameters, files);

            foreach (CompilerError error in result.Errors)
            {
                Console.WriteLine(error.ErrorText);
            }

            if (result.Errors.HasErrors)
            {
                Console.ReadKey();
                throw new Exception("Build failed");
            }

            CopyAssembliesForRuntime();

            return GetBuilders(result.CompiledAssembly);
        }

        protected virtual ClassBuilder[] GetBuilders(Assembly assembly)
        {
            var list = new List<ClassBuilder>();
            Type[] types;

            try
            {
                types = assembly.GetExportedTypes();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex);
                types = new Type[0];
            }

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

        protected string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);

                return Path.Combine(Path.GetDirectoryName(path) ?? throw new InvalidOperationException(), "Generated");
            }
        }

        protected virtual void CopyAssembliesForRuntime()
        {
            foreach (var assembly in Assemblies)
            {
                File.Copy(assembly, Path.Combine(AssemblyDirectory, Path.GetFileName(assembly) ?? throw new InvalidOperationException()), true);
            }
        }

        protected virtual bool ShouldRegisterType(Type type)
        {
            if (Namespace == null && type.Namespace == null) return true;
            if (type.Namespace.StartsWith(Namespace)) return true;

            return false;
        }
    }
};