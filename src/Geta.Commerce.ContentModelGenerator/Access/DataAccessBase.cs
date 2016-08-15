using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Geta.Commerce.ContentModelGenerator.Access
{
	public abstract class DataAccessBase
	{
		#region Fields
		private DbTransaction mTran;
		private DbConnection mConn;
		#endregion

		#region Methods
		protected virtual void BeginTransaction()
		{
			if (mTran == null)
			{
				mTran = mConn.BeginTransaction();
			}
		}

		protected virtual void CloseConnection()
		{
			if (mConn != null)
			{
				if (mConn.State == ConnectionState.Open)
				{
					mConn.Close();
				}

				mConn = null;
				mTran = null;
			}
		}

		protected virtual void CommitTransaction()
		{
			if (mTran != null)
			{
				mTran.Commit();
				mTran = null;
			}
		}

		protected virtual DbCommand CreateCommand()
		{
			DbCommand cmd = InternalCreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			return cmd;
		}

        protected virtual DbCommand CreateTextCommand(string commandText)
        {
            DbCommand cmd = InternalCreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = commandText;
            return cmd;
        }

        public virtual DbCommand CreateCommand(string commandText)
		{
			DbCommand cmd = CreateCommand();
			cmd.CommandText = commandText;
			return cmd;
		}

		public virtual DbDataAdapter CreateDataAdapter() {
			return DatabaseFactory.CreateDataAdapter();
		}

		public DbDataAdapter CreateDataAdapter(DbCommand selectCommand)
		{
			DbDataAdapter adapter = CreateDataAdapter();
			adapter.SelectCommand = selectCommand;
			return adapter;
		}

		public DbDataAdapter CreateDataAdapter(DbCommand insertCommand, DbCommand updateCommand, DbCommand deleteCommand)
		{
			DbDataAdapter adapter = CreateDataAdapter();
			adapter.InsertCommand = insertCommand;
			adapter.UpdateCommand = updateCommand;
			adapter.DeleteCommand = deleteCommand;
			return adapter;
		}

		public DbDataAdapter CreateDataAdapter(DbCommand selectCommand, DbCommand insertCommand, DbCommand updateCommand, DbCommand deleteCommand)
		{
			DbDataAdapter adapter = CreateDataAdapter(insertCommand, updateCommand, deleteCommand);
			adapter.SelectCommand = selectCommand;
			return adapter;
		}

		protected virtual IDbDataParameter CreateParameter(string name, object value)
		{
			return DatabaseFactory.CreateParameter(name, value);
		}

		public static void Initialize(ConnectionStringSettings connectionSettings)
		{
			ConnectionSettings = connectionSettings;

			if (ConnectionSettings.ProviderName.EndsWith("SqlClient"))
			{
				DatabaseFactory = new SqlDatabaseFactory(ConnectionSettings);
			}

			try
			{
				using (DatabaseFactory.CreateConnection())
				{
				}
			}
			catch (Exception exception)
			{
				throw new Exception(string.Format("Failed to connect to database using connection string with name '{0}'", DatabaseFactory.ConnectionSettings.Name), exception);
			}
		}

		public T Execute<T>(DatabaseFactory.Action<T> action)
		{
			return DatabaseFactory.Execute<T>(
				delegate
				{
					T local;

					try
					{
						OpenConnection();
						local = action();
					}
					finally
					{
						CloseConnection();
					}

					return local;
				}
			);
		}

		private DbCommand InternalCreateCommand()
		{
			if (mConn == null)
			{
				throw new Exception("Cannot call CreateCommand before calling OpenConnection");
			}

			DbCommand cmd = DatabaseFactory.CreateCommand();
			cmd.Connection = mConn;

			if (mTran != null)
				cmd.Transaction = mTran;

			return cmd;
		}

		protected virtual void OpenConnection()
		{
			if (mConn == null)
			{
				mConn = DatabaseFactory.CreateConnection();
			}
		}

		public static string ProviderSpecificParameterName(string parameterName)
		{
			return DatabaseFactory.ProviderSpecificParameterName(parameterName);
		}
		#endregion

		#region Properties
		public static ConnectionStringSettings ConnectionSettings
		{
			get;
			set;
		}

		public static DatabaseFactory DatabaseFactory
		{
			get;
			set;
		}
		#endregion
	}
}
