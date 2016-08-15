using System.Collections.Generic;
using Geta.Commerce.ContentModelGenerator.Data.Abstraction;

namespace Geta.Commerce.ContentModelGenerator
{
    public class CommerceInRiverExporter : CommerceExporter
    {
        private const string _metaFilter = "From inRiver";

        public CommerceInRiverExporter(string path, string nameSpace) : base(path, nameSpace) {}

        protected override IList<MetaClass> GetMetaClasses()
        {
            return MetaClass.List(_metaFilter);
        }

        protected override IList<MetaField> GetMetaFieldsForClass(MetaClass metaClass)
        {
            return MetaField.List(metaClass.Id, _metaFilter);
        }
    }
}