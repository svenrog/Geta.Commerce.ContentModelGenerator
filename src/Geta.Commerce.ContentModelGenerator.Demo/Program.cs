using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
            var timer = new Stopwatch();
            var options = new Options();

            timer.Start();

            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.Write("Supply arguments: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("-p");
                Console.ResetColor();
                Console.Write(" (project path) ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("-n");
                Console.ResetColor();
                Console.Write(" (namespace) ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("-o");
                Console.ResetColor();
                Console.Write(" (output path), type ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("-?");
                Console.ResetColor();
                Console.WriteLine(" for help.");
                return;
            }

            IDictionary<string, ClassBuilder> builders = null;

            if (options.ReflectExistingClasses)
            {
                builders = ReadClasses(options);
            }

            GenerateClasses(options, builders);

            timer.Stop();

            Console.Write("Operation done in ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{timer.Elapsed:g}");
            Console.ResetColor();
        }

        static IDictionary<string, ClassBuilder> ReadClasses(Options options)
        {
            Console.Write("Loading existing classes from project... ");

            CrossDomainReflector reflector = null;

            try
            {
                reflector = new CrossDomainReflector(options.ProjectPath, options.NameSpace);

                var builders = reflector.GetBuilders();
                
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("done");
                Console.ResetColor();

                Console.Write(" (found ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(builders.Count());
                Console.ResetColor();
                Console.WriteLine(" items).");

                return builders.GroupBy(x => $"{x.Namespace}.{x.ClassName.ToFileName()}")
                               .ToDictionary(x => x.Key, x => x.Last());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();

                Console.WriteLine("Aborted loading of existing classes.");
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
                Console.Write("Reading configuration... ");

                var configuration = GetConfiguration(options) ?? throw new FileNotFoundException();
                var settings = GetConnectionSettings(configuration) ?? throw new ConfigurationErrorsException();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("done");
                Console.ResetColor();
                
                Console.Write("Connecting to commerce database... ");

                DataAccessBase.Initialize(settings);

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("done");
                Console.ResetColor();

                Console.Write("Querying database for models... ");

                var exporter = new CommerceInRiverExporter(options.Path, options.NameSpace)
                {
                    GenerateBaseClasses = options.GenerateBaseClasses
                };
                
                var classBuilders = exporter.GenerateBuilders(builders);

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("done");
                Console.ResetColor();

                Console.Write(" (found ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(classBuilders.Count());
                Console.ResetColor();
                Console.WriteLine(" items).");
                
                Console.Write("Writing files... ");

                exporter.Export(classBuilders);

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("done");
                Console.ResetColor();
            }
            catch (FileNotFoundException)
            {
                Console.Write("Could not find a configuration file in path '");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Path.Combine(options.ProjectPath, "web.config"));
                Console.ResetColor();
                Console.WriteLine($"'.");
            }
            catch (ConfigurationErrorsException)
            {
                Console.Write("Could not find a connection '");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(ConnectionName);
                Console.ResetColor();
                Console.Write("' in path '");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(options.ProjectPath);
                Console.ResetColor();
                Console.WriteLine($"'.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        static Configuration GetConfiguration(Options options)
        {
            var configurationPath = Path.Combine(options.ProjectPath, "web.config");
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configurationPath
            };

            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        static ConnectionStringSettings GetConnectionSettings(Configuration configuration)
        {
            var connectionSection = configuration?.ConnectionStrings;
            return connectionSection?.ConnectionStrings[ConnectionName];
        }
    }
}
