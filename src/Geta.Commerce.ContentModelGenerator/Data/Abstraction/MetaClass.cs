using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using Geta.Commerce.ContentModelGenerator.Access;
using Geta.Commerce.ContentModelGenerator.Data.Access;

namespace Geta.Commerce.ContentModelGenerator.Data.Abstraction
{
    [DataContract]
    [Serializable]
    public class MetaClass : DataAccessBase
    {
        public static List<MetaClass> List(string filter = null)
        {
            DataSet ds = new MetaClassDb().List(filter);
            List<MetaClass> entries = new List<MetaClass>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var entry = CreateEntryFromDataRow(row);

                if (!IsValid(entry)) continue;

                entries.Add(CreateEntryFromDataRow(row));
            }

            return entries;
        }

        private static bool IsValid(MetaClass entry)
        {
            if (string.IsNullOrEmpty(entry.Name)) return false;
            if (string.IsNullOrEmpty(entry.TableName)) return false;

            return true;
        }

        private static MetaClass CreateEntryFromDataRow(DataRow row)
        {
            return new MetaClass(Convert.ToInt32(row["MetaClassId"]))
            {
                ParentId = Convert.ToInt32(row["ParentClassId"]),
                Name = (string)row["Name"],
                FriendlyName = (string)row["FriendlyName"],
                Description = row["Description"] as string,
                TableName = (string)row["TableName"]
            };
        }

        public MetaClass(int id)
        {
            Id = id;
        }

        [DataMember]
        public int Id { get; }

        [DataMember]
        public int ParentId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string FriendlyName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string TableName { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}