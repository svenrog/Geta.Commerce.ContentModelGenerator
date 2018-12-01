using CommandLine;

namespace Geta.Commerce.ContentModelGenerator.Example
{
    public class Options
    {
        [Option('o', "output", Required = true, HelpText = "Output path.")]
        public string Path { get; set; }

        [Option('n', "namespace", Required = true, HelpText = "Base namespace.")]
        public string NameSpace { get; set; }

        [Option('p', "project", Required = false, HelpText = "Path to project (required if using -r).")]
        public string ProjectPath { get; set; }

        [Option('c', "connectionString", Required = false, HelpText = "Connection string to utilize.")]
        public string ConnectionString { get; set; }

        [Option('v', "connectionProvider", Required = false, HelpText = "Connection provider to utilize.")]
        public string ConnectionProvider { get; set; }

        [Option('b', "generateBaseClasses", Required = false, HelpText = "True if base classes are to be aggregated.")]
        public bool GenerateBaseClasses { get; set; }

        [Option('r', "reflectExistingClasses", Required = false, HelpText = "True if existing class files are to be reflected and merged with result.")]
        public bool ReflectExistingClasses { get; set; }
    }
}
