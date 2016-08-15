using System.Data;
using Geta.Commerce.ContentModelGenerator.Access;

namespace Geta.Commerce.ContentModelGenerator.Data.Access
{
    public class MetaFieldDb : DataAccessBase
    {
        public DataSet List(int id, string filter = null)
        {
            return Execute(delegate
            {
                var commandText = @"SELECT mf.[MetaFieldId], mf.[Name], dt.[Name] as [Type], mf.[FriendlyName], mf.[Description], mf.[MultiLanguageValue]
                                    FROM MetaClassMetaFieldRelation mcfr
                                        INNER JOIN MetaField mf ON mcfr.MetaFieldId = mf.MetaFieldId
                                        INNER JOIN MetaDataType dt ON mf.DataTypeId = dt.DataTypeId
                                    WHERE MetaClassId = @id AND mf.[SystemMetaClassId] = 0";           

                var emptyFilter = string.IsNullOrWhiteSpace(filter);

                if (!emptyFilter)
                    commandText += " AND CAST(mf.[Description] as nvarchar(max)) = @filter";

                var ds = new DataSet();
                var cmd = CreateTextCommand(commandText);
                cmd.Parameters.Add(CreateParameter("id", id));

                if (!emptyFilter)
                {
                    cmd.Parameters.Add(CreateParameter("filter", filter));
                }
                
                CreateDataAdapter(cmd).Fill(ds);
                return ds;
            });
        }
    }
}