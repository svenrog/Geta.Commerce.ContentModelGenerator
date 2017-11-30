using System.Collections.Generic;
using Geta.Commerce.ContentModelGenerator.Builders;

namespace Geta.Commerce.ContentModelGenerator
{
    public interface IExporter
    {
        string ExportDirectoryPath { get; set; }
        string NameSpace { get; set; }

        void Export(IEnumerable<ClassBuilder> builders);
    }
}