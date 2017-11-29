using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        protected readonly string NameSpace;
        protected readonly string OutputFile;
        protected StreamReader Reader;

        public ClassCompiler(string assemblyPath, string nameSpace)
        {
            CodeProvider = new CSharpCodeProvider();
            OutputFile = $"{assemblyPath}\\compiled.dll";

            CompilerParameters = new CompilerParameters
            {
                CompilerOptions = "/nostdlib",
                OutputAssembly = OutputFile
            };

            var assemblies = Directory.GetFiles(assemblyPath, "*.dll")
                                      .Where(x => !x.EndsWith("System.Runtime.dll"))
                                      .ToArray();            

            var defaultAssemblies = new []
            {
                "System.ComponentModel.DataAnnotations.dll"
            };

            CompilerParameters.ReferencedAssemblies.AddRange(defaultAssemblies);
            CompilerParameters.ReferencedAssemblies.AddRange(assemblies);

            NameSpace = nameSpace;
        }

        public ClassBuilder[] ParseFiles(string[] files)
        {
            var result = CodeProvider.CompileAssemblyFromFile(CompilerParameters, files);

            foreach (CompilerError error in result.Errors)
            {
                Console.WriteLine(error.ErrorText);
            }

            if (result.Errors.HasErrors)
                throw new Exception("Build failed");

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
            var builder = new ClassBuilder(type.Name, type.Namespace, type.ToInheritsDeclaration());

            var classAttributes = type.GetCustomAttributesData();
            foreach (var attribute in classAttributes)
            {
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

                var attributes = property.GetCustomAttributesData();
                foreach (var attribute in attributes)
                {
                    var attributeDefinition = attribute.ToAttributeDefinition();
                    definition.Attributes.Add(attributeDefinition);
                }

                definition.Name = property.Name;
                definition.Get = property.GetMethod != null;
                definition.Set = property.SetMethod != null;
                definition.Virtual = property.GetMethod?.IsVirtual ?? false;
                definition.Override = !property.GetMethod?.Equals(property.GetMethod?.GetBaseDefinition()) ?? false;

                builder.Properties.Add(definition);
            }

            return builder;
        }

        protected virtual bool ShouldRegisterType(Type type)
        {
            if (NameSpace == null && type.Namespace == null) return true;
            if (type.Namespace.StartsWith(NameSpace)) return true;

            return false;
        }
    }
};