using System.Data;
using Geta.Commerce.ContentModelGenerator.Access;

namespace Geta.Commerce.ContentModelGenerator.Data.Access
{
    public class MetaClassDb : DataAccessBase
    {
        public DataSet List(string filter = null)
        {
            return Execute(delegate
            {
                var commandText = "SELECT [MetaClassId], [FriendlyName], [ParentClassId], [Name], [Description], [TableName] FROM [MetaClass] WHERE [IsSystem] = 0 AND [IsAbstract] = 0";
                var emptyFilter = string.IsNullOrWhiteSpace(filter);

                if (!emptyFilter)
                    commandText += " AND CAST([Description] as nvarchar(max)) = @filter";

                var ds = new DataSet();
                var cmd = CreateTextCommand(commandText);

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