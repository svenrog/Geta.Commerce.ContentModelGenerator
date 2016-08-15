using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Geta.Commerce.ContentModelGenerator.Access
{
	public abstract class DatabaseFactory
	{
		public DbCommand CreateCommand()
		{
			return DbFactory.CreateCommand();
		}

		public DbConnection CreateConnection()
		{
			DbConnection conn = DbFactory.CreateConnection();
			conn.ConnectionString = ConnectionSettings.ConnectionString;
			conn.Open();
			return conn;
		}

		public DbDataAdapter CreateDataAdapter()
		{
			return DbFactory.CreateDataAdapter();
		}

		public abstract IDbDataParameter CreateParameter(string name, object value);

		public IDbDataParameter CreateParameter(string parameterName, DbType dbType, int size)
		{
			IDbDataParameter parameter = CreateParameter(parameterName, dbType, ParameterDirection.Input, null);
			parameter.Size = (size == 0) ? 1 : size;
			return parameter;
		}

		public abstract IDbDataParameter CreateParameter(string parameterName, DbType dbType, ParameterDirection paramDir, object paramValue);
		public abstract IDbDataParameter CreateTextParameter(string parameterName, string paramValue);

		public T Execute<T>(Action<T> method)
		{
			T local = method();
			return local;
		}

		public abstract string ProviderSpecificParameterName(string parameterName);

		public abstract ConnectionStringSettings ConnectionSettings
		{
			get;
		}

		public abstract DbProviderFactory DbFactory
		{
			get;
		}

		public delegate void Action();
		public delegate T Action<T>();
	}
}