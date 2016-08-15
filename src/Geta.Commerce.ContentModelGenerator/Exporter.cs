using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Geta.Commerce.ContentModelGenerator.Builders;

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

        public virtual void Export()
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

        protected virtual void WriteBuilders(IList<ClassBuilder> builders)
        {
            if (builders == null || !builders.Any()) return;

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
            return string.Concat(ExportDirectoryPath, Path.DirectorySeparatorChar, builder.ClassName, ".cs");
        }
    }
}