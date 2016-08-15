using System;
using System.Configuration;
using Geta.Commerce.ContentModelGenerator.Access;

namespace Geta.Commerce.ContentModelGenerator.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Supply arguments: Executeable {path} {namespace}");
                return;
            }

            try
            {
                var configuration = ConfigurationManager.ConnectionStrings["EcfSqlConnection"];
                if (configuration == null) throw new ConfigurationErrorsException();

                DataAccessBase.Initialize(configuration);

                var exporter = new CommerceInRiverExporter(args[0], args[1])
                {
                    GenerateBaseClasses = true
                };

                exporter.Export();
            }
            catch (ConfigurationErrorsException)
            {
                Console.Write("Provide a connection with name 'EcfSqlConnection' in the application configuration.");
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
