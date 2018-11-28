using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Geta.Commerce.ContentModelGenerator.Access;
using Geta.Commerce.ContentModelGenerator.Parsers;
using Geta.Commerce.ContentModelGenerator.Builders;
using CommandLine;

namespace Geta.Commerce.ContentModelGenerator.Example
{
    class Program
    {
        const string ConnectionName = "EcfSqlConnection";
        const string ConnectionDefaultProvider = "System.Data.SqlClient";

        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Supply arguments: -p {path} -n {namespace}, type -? for help");
                return;
            }

            IDictionary<string, ClassBuilder> builders = null;

            if (!string.IsNullOrEmpty(options.ProjectPath))
            {
                builders = ReadClasses(options);
            }

            GenerateClasses(options, builders);
        }

        static IDictionary<string, ClassBuilder> ReadClasses(Options options)
        {
            try
            {
                var domainReflector = new DomainReflector(options.ProjectPath, options.NameSpace);
                var builders = domainReflector.GetBuilders(domainReflector.Types);

                return builders.ToDictionary(x => $"{x.NameSpace}.{x.ClassName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        static void GenerateClasses(Options options, IDictionary<string, ClassBuilder> builders = null)
        {
            try
            {
                ConnectionStringSettings configuration;

                if (string.IsNullOrEmpty(options.ConnectionString))
                {
                    configuration = ConfigurationManager.ConnectionStrings[ConnectionName];
                }
                else
                {
                    configuration = new ConnectionStringSettings(ConnectionName, options.ConnectionString, options.ConnectionProvider ?? ConnectionDefaultProvider);
                }

                if (configuration == null) throw new ConfigurationErrorsException();

                DataAccessBase.Initialize(configuration);

                var exporter = new CommerceInRiverExporter(options.Path, options.NameSpace)
                {
                    GenerateBaseClasses = options.GenerateBaseClasses
                };

                var classBuilders = exporter.GenerateBuilders(builders);

                exporter.Export(classBuilders);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Provide a connection with name '{0}' in the application configuration or supply a -c argument.", ConnectionName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
