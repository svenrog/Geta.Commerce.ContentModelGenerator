using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Geta.Commerce.ContentModelGenerator.Structure
{
    public class AssemblyMappingCollection
    {
        protected readonly AssemblyMapping[] Mappings;

        public AssemblyMappingCollection(Configuration configuration)
        {
            var runtime = configuration.GetSection("runtime");

            var xml = runtime.SectionInformation.GetRawXml();
            var document = XDocument.Parse(xml);

            var manager = new XmlNamespaceManager(new NameTable());
            manager.AddNamespace("x", "urn:schemas-microsoft-com:asm.v1");

            var dependentAssemblies = document.XPathSelectElements("//x:dependentAssembly", manager)
                                              .ToArray();
            
            Mappings = new AssemblyMapping[dependentAssemblies.Length];

            for (var i = 0; i < Mappings.Length; i++)
            {
                var dependentAssembly = dependentAssemblies[i];
                var assemblyIdentity = dependentAssembly.XPathSelectElement("./x:assemblyIdentity", manager);
                var bindingRedirect = dependentAssembly.XPathSelectElement("./x:bindingRedirect", manager);

                Mappings[i] = new AssemblyMapping
                {
                    Name = assemblyIdentity.Attribute(XName.Get("name"))?.Value,
                    Culture = assemblyIdentity.Attribute(XName.Get("culture"))?.Value,
                    PublicKeyToken = assemblyIdentity.Attribute(XName.Get("publicKeyToken"))?.Value,
                    Range = VersionRange.Parse(bindingRedirect?.Attribute(XName.Get("oldVersion"))?.Value),
                    To = Version.Parse(bindingRedirect?.Attribute(XName.Get("newVersion"))?.Value)
                };
            }
        }

        public AssemblyName GetRedirectedAssemblyName(AssemblyName assembly)
        {
            foreach (var mapping in Mappings)
            {
                if (!mapping.Name.Equals(assembly.Name)) continue;
                if (!string.IsNullOrEmpty(assembly.CultureName) && !mapping.Culture.Equals(assembly.CultureName)) continue;

                if (assembly.Version > mapping.Range.To) continue;
                if (assembly.Version < mapping.Range.From) continue;

                assembly.Version = mapping.To;

                return assembly;
            }

            return assembly;
        }
    }
}