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
    public class MetaField : DataAccessBase
    {
        public static List<MetaField> List(int id, string filter = null)
        {
            DataSet ds = new MetaFieldDb().List(id, filter);
            List<MetaField> entries = new List<MetaField>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var entry = CreateEntryFromDataRow(row);

                if (!IsValid(entry)) continue;

                entries.Add(entry);
            }

            return entries;
        }

        public static bool IsValid(MetaField field)
        {
            if (string.IsNullOrEmpty(field.Name)) return false;
            if (string.IsNullOrEmpty(field.Type)) return false;

            return true;
        }

        private static MetaField CreateEntryFromDataRow(DataRow row)
        {
            return new MetaField(Convert.ToInt32(row["MetaFieldId"]))
            {
                Name = (string)row["Name"],
                Type = (string)row["Type"],
                FriendlyName = (string)row["FriendlyName"],
                Description = row["Description"] as string,
                MultiLanguageValue = (bool)row["MultiLanguageValue"]
            };
        }

        public MetaField(int id)
        {
            Id = id;
        }

        [DataMember]
        public int Id { get; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string FriendlyName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool MultiLanguageValue { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var o = obj as MetaField;
            if (o == null)
            {
                return false;
            }

            return Id == o.Id;
        }
    }
}