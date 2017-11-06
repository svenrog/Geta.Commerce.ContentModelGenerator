using System;
using System.Configuration;
using Geta.Commerce.ContentModelGenerator.Access;
using CommandLine;

namespace Geta.Commerce.ContentModelGenerator.Example
{
    class Program
    {
        const string _connectionName = "EcfSqlConnection";
        const string _connectionDefaultProvider = "System.Data.SqlClient";

        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Supply arguments: -p {path} -n {namespace}, type -? for help");
                return;
            }

            try
            {
                ConnectionStringSettings configuration;

                if (string.IsNullOrEmpty(options.ConnectionString))
                {
                    configuration = ConfigurationManager.ConnectionStrings[_connectionName];
                }
                else
                {
                    configuration = new ConnectionStringSettings(_connectionName, options.ConnectionString, options.ConnectionProvider ?? _connectionDefaultProvider);
                }

                if (configuration == null) throw new ConfigurationErrorsException();

                DataAccessBase.Initialize(configuration);

                var exporter = new CommerceInRiverExporter(options.Path, options.NameSpace)
                {
                    GenerateBaseClasses = options.GenerateBaseClasses
                };

                exporter.Export();
            }
            catch (ConfigurationErrorsException)
            {
                Console.Write("Provide a connection with name '{0}' in the application configuration or supply a -c argument.", _connectionName);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
