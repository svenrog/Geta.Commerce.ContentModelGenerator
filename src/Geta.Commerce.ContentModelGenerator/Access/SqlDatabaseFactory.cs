using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Geta.Commerce.ContentModelGenerator.Access
{
	public class SqlDatabaseFactory : DatabaseFactory
	{
		#region Fields
		private ConnectionStringSettings mConnectionSettings;
		private DbProviderFactory mDbFactory;
		#endregion

		#region Constructors
		public SqlDatabaseFactory(ConnectionStringSettings connectionSettings)
		{
			mConnectionSettings = connectionSettings;
			mDbFactory = DbProviderFactories.GetFactory(ConnectionSettings.ProviderName);
		}
		#endregion

		public override IDbDataParameter CreateParameter(string name, object value)
		{
			return new SqlParameter(ProviderSpecificParameterName(name), value);
		}

		public override IDbDataParameter CreateParameter(string parameterName, DbType dbType, ParameterDirection paramDir, object paramValue)
		{
			IDbDataParameter parameter = new SqlParameter();
			parameter.ParameterName = ProviderSpecificParameterName(parameterName);
			parameter.Value = paramValue;
			parameter.Direction = paramDir;
			parameter.DbType = dbType;
			return parameter;
		}

		public override IDbDataParameter CreateTextParameter(string parameterName, string paramValue)
		{
			IDbDataParameter parameter = new SqlParameter(ProviderSpecificParameterName(parameterName), SqlDbType.NText, (paramValue == null) ? 1 : paramValue.Length);
			parameter.Value = paramValue;
			return parameter;
		}

		public override string ProviderSpecificParameterName(string parameterName)
		{
			return "@" + parameterName;
		}

		public override ConnectionStringSettings ConnectionSettings
		{
			get
			{
				return mConnectionSettings;
			}
		}

		public override DbProviderFactory DbFactory
		{
			get
			{
				return mDbFactory;
			}
		}
	}
}
