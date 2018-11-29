using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Geta.Commerce.ContentModelGenerator.Builders;
using Geta.Commerce.ContentModelGenerator.Extensions;

namespace Geta.Commerce.ContentModelGenerator
{
    public abstract class Exporter : IExporter
    {
        public string ExportDirectoryPath { get; set; }
        public string NameSpace { get; set; }

        protected Exporter(string path, string nameSpace)
        {
            ExportDirectoryPath = path;
            NameSpace = nameSpace;

            ThrowErrorIfMisconfigured();
        }

        public virtual void Export(IEnumerable<ClassBuilder> builders)
        {
            ThrowErrorIfMisconfigured();
        }

        protected void ThrowErrorIfMisconfigured()
        {
            if (string.IsNullOrWhiteSpace(NameSpace))
            {
                throw new ArgumentException("Namespace needs to be set");
            }

            if (string.IsNullOrWhiteSpace(ExportDirectoryPath))
            {
                throw new ArgumentException("ExportFilePath needs to be set");
            }
        }

        protected virtual void WriteBuilders(IEnumerable<ClassBuilder> builders)
        {
            foreach (var builder in builders)
            {
                using (var stream = new FileStream(GetExportFilePath(builder), FileMode.Create))
                {
                    var bytes = Encoding.UTF8.GetBytes(builder.ToString());
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        protected string GetExportFilePath(ClassBuilder builder)
        {
            var fileName = builder.ClassName.ToFileName();
            return string.Concat(ExportDirectoryPath, Path.DirectorySeparatorChar, fileName, ".cs");
        }
    }
}