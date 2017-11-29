using CommandLine;
using CommandLine.Text;

namespace Geta.Commerce.ContentModelGenerator.Example
{
    public class Options
    {
        [Option('c', "connectionString", Required = false, HelpText = "Connection string to utilize.")]
        public string ConnectionString { get; set; }

        [Option('r', "connectionProvider", Required = false, HelpText = "Connection provider to utilize.")]
        public string ConnectionProvider { get; set; }

        [Option('b', "generateBaseClasses", Required = false, HelpText = "True if base classes are to be aggregated.")]
        public bool GenerateBaseClasses { get; set; }

        [Option('p', "path", Required = true, HelpText = "Output path.")]
        public string Path { get; set; }

        [Option('a', "assemblies", Required = true, HelpText = "Assembly path.")]
        public string Assemblies { get; set; }

        [Option('n', "namespace", Required = true, HelpText = "Base namespace.")]
        public string NameSpace { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

}
