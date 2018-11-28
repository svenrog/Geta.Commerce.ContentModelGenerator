using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Geta.Commerce.ContentModelGenerator.Access;
using Geta.Commerce.ContentModelGenerator.Parsers;
using Geta.Commerce.ContentModelGenerator.Builders;
using CommandLine;
using System.IO;
using Geta.Commerce.ContentModelGenerator.Extensions;

namespace Geta.Commerce.ContentModelGenerator.Example
{
    class Program
    {
        const string ConnectionName = "EcfSqlConnection";
        
        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Supply arguments: -p {project path} -n {namespace} -o {output path}, type -? for help");
                return;
            }

            IDictionary<string, ClassBuilder> builders = null;

            if (options.ReflectExistingClasses)
            {
                builders = ReadClasses(options);
            }

            GenerateClasses(options, builders);
        }

        static IDictionary<string, ClassBuilder> ReadClasses(Options options)
        {
            CrossDomainReflector reflector = null;

            try
            {
                reflector = new CrossDomainReflector(options.ProjectPath, options.NameSpace);

                var builders = reflector.GetBuilders();
                return builders.ToDictionary(x => $"{x.NameSpace}.{x.ClassName.ToFileName()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                reflector?.Dispose();
            }

            return null;
        }

        static void GenerateClasses(Options options, IDictionary<string, ClassBuilder> builders = null)
        {
            try
            {
                var settings = GetConnectionSettings(options);
                if (settings == null) throw new ConfigurationErrorsException();

                DataAccessBase.Initialize(settings);

                var exporter = new CommerceInRiverExporter(options.Path, options.NameSpace)
                {
                    GenerateBaseClasses = options.GenerateBaseClasses
                };

                var classBuilders = exporter.GenerateBuilders(builders);

                exporter.Export(classBuilders);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine($"Could not find connection '{ConnectionName}' in given project path.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static ConnectionStringSettings GetConnectionSettings(Options options)
        {
            var configurationPath = Path.Combine(options.ProjectPath, "web.config");
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configurationPath
            };

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var connectionSection = configuration?.ConnectionStrings;
            return connectionSection?.ConnectionStrings[ConnectionName];
        }
    }
}
