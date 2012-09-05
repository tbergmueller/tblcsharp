/*
 * Created by SharpDevelop.
 * User: thomas
 * Date: 06/06/2012
 * Time: 12:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using MySql.Data.MySqlClient;
using TBL.EDOLL;

namespace TBL.Data
{
	/// <summary>
	/// Description of Database.
	/// </summary>
	public class MySQLDatabaseHandler
	{
		MySqlConnection conn = null;
		bool debugMode = false;
		
		public MySQLDatabaseHandler(string vHostOrIP, string vDatabaseName, string vUser, string vPassword)
		{
			MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
			builder.Database = vDatabaseName;
			builder.UserID = vUser;
			builder.Password = vPassword;
			builder.Server = vHostOrIP;
			
			string conSTring = builder.GetConnectionString(true);
			
			conn = new MySqlConnection(builder.GetConnectionString(true));
			
		}
		
		public bool DebugMode
		{
			get
			{
				return(debugMode);
			}
			set
			{
				debugMode = value;
			}
		}
		
		
		public bool Connect(out int oErrorCode)
		{
			try
			{
				conn.Open();
				oErrorCode = 0;
				return(true);
			}
			catch(Exception e)
			{
				if(debugMode)
				{
					stdOut.Error("Could not connect to Database.", e.ToString());
				}
				oErrorCode = -573; // Could not establish database connection
				return(false);
			}
		}
		
		
		public bool ReadFromDB(String vQuery, out MySqlDataReader oReader, out int oErrorCode)
		{
			MySqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = vQuery;
			
			MySqlDataReader reader = null;
			
			try
			{
				reader = cmd.ExecuteReader();
			}
			catch(MySqlException e)
			{
				oReader = null;
				oErrorCode = -574; // SQL execution failure
				
				if(debugMode)
				{
					stdOut.Error(e.ToString());
				}
				return(false);
			}
			
			oReader = reader;
			oErrorCode = 0;
			return(true);
		}
		
		public bool WriteToDB(String vQuery, out int oErrorCode)
		{
			MySqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = vQuery;
			int rowsAffected;
			try
			{
				rowsAffected = cmd.ExecuteNonQuery();
			}
			catch(MySqlException e)
			{
				if(debugMode)
				{
					stdOut.Error(e.ToString());
				}
				
				oErrorCode = -574;
				return(false);
			}
			
			if(rowsAffected >= 0)
			{
				oErrorCode = 0;
				return(true);
			}
			else
			{
				oErrorCode = -574;
				return(false);
			}
		}
		
	}
	
	
}
