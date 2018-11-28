using CommandLine;
using CommandLine.Text;

namespace Geta.Commerce.ContentModelGenerator.Example
{
    public class Options
    {
        [Option('o', "output", Required = true, HelpText = "Output path.")]
        public string Path { get; set; }

        [Option('n', "namespace", Required = true, HelpText = "Base namespace.")]
        public string NameSpace { get; set; }

        [Option('p', "project", Required = true, HelpText = "Path to project.")]
        public string ProjectPath { get; set; }

        [Option('b', "generateBaseClasses", Required = false, HelpText = "True if base classes are to be aggregated.")]
        public bool GenerateBaseClasses { get; set; }

        [Option('r', "reflectExistingClasses", Required = false, HelpText = "True if existing class files are to be reflected and merged with result.")]
        public bool ReflectExistingClasses { get; set; }

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
