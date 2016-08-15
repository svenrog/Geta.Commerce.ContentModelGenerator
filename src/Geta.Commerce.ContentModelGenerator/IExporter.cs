namespace Geta.Commerce.ContentModelGenerator
{
    public interface IExporter
    {
        string ExportDirectoryPath { get; set; }
        string NameSpace { get; set; }

        void Export();
    }
}