using System;
using System.Configuration;
using System.IO;
using System.Text;
using Geta.Commerce.ContentModelGenerator.Access;
using CommandLine;
using Geta.Commerce.ContentModelGenerator.Builders;
using Geta.Commerce.ContentModelGenerator.Parsers;

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

            if (!string.IsNullOrEmpty(options.Assemblies))
                ReadClasses(options);
            
            //GenerateClasses(options);
        }

        static void ReadClasses(Options options)
        {
            try
            {
                var classCompiler = new ClassCompiler(options.Assemblies, options.NameSpace);
                var classFiles = Directory.GetFiles(options.Path, "*.cs");
                var builders = classCompiler.ParseFiles(classFiles);

                foreach (var builder in builders)
                {
                    using (var stream = new FileStream($"c:\\temp\\{builder.ClassName}.cs", FileMode.Create))
                    {
                        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void GenerateClasses(Options options)
        {
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
                Console.WriteLine("Provide a connection with name '{0}' in the application configuration or supply a -c argument.", _connectionName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
