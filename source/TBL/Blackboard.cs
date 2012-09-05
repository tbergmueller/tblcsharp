/*
 * Created by SharpDevelop.
 * User: thomas
 * Date: 11/06/2012
 * Time: 08:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using TBL.Data;

namespace TBL.IPC
{
	/// <summary>
	/// Provides the interface to the Blackboard-Data. A Blackboard could be implemented with files, <see cref="BlackboardDatabaseConnector">MYSQL-Databases</see> or other methods
	/// </summary>
	public interface IBlackboardConnector
	{
		bool Commit(BlackboardEntry vEntry);
		BlackboardEntry[] GetEntries(BlackboardQuery vQuery);
		
		bool Start(bool vCreateIfNotExist, out int oErrorCode);
		bool Stop();
		
	}
	
	
	/// <summary>
	/// A Messageboard for Inter-Process-Communication
	/// </summary>
	public class Blackboard
	{
		int id;
		IBlackboardConnector blackboard;
		
		public Blackboard(IBlackboardConnector rConnector, int vMyID)
		{
			int err;
			blackboard = rConnector;
			
			if(!blackboard.Start(true, out err))
			{
				throw new TBL.Exceptions.ObjectNotReady(EDOLL.EDOLLHandler.Verbalize(err) + Environment.NewLine + rConnector.ToString());
			}
		}
		
		
	}
	
	public class BlackboardEntry
	{
		
	}
	
	public enum BlackboardAction
	{
		CommitForAll,
		Commit,
		GetFromSpecificSender,
		Get,
	}
	
	public class BlackboardQuery
	{
		private bool filterSender=false;
		private bool forMeOnly=false;
		
		private int senderID=-1;
		private int receiverID=-1;
		private BlackboardAction action;
		
		private string msg;
				
		public BlackboardQuery(BlackboardAction vAction, int vFrom, int vTo, string vMsg)
		{
			throw new NotImplementedException();
		}
		
		public BlackboardQuery(BlackboardAction vAction, int vFrom, int vTo)
		{
			senderID = vFrom;
			receiverID = vTo;
			
			if(vAction != BlackboardAction.GetFromSpecificSender)
			{
				throw new ArgumentException("Action does not match arguments. You have to use BlackboardAction.GetFromSpecificSender in order to get this constructor working...");
			}
			
			action = vAction;
		}
		
	}
	
	public class BlackboardDatabaseConnector:IBlackboardConnector
	{
		private string tblName;
		MySQLDatabaseHandler conn;
		
		private const string colSender="from";
		private const string colReceiver="to";
		private const string colID="id";
		private const string colType="type";
		private const string colMsg="message";
		
		
		private string tableCreationQuery;
		
		
		
		public bool Commit(BlackboardEntry vEntry)
		{
			throw new NotImplementedException();
			
		}
		
		public BlackboardEntry[] GetEntries(BlackboardQuery vQuery)
		{
			throw new NotImplementedException();
			
		}
		
		
		public BlackboardDatabaseConnector(MySQLDatabaseHandler rHandler, string vTableName)
		{
			tblName = vTableName;
			tableCreationQuery="CREATE TABLE IF NOT EXISTS `"+vTableName+"` (`id` int(15) NOT NULL AUTO_INCREMENT,`from` int(5) NOT NULL, `to` int(5) NOT NULL,`type` int(5) NOT NULL,`message` text NOT NULL, PRIMARY KEY (`id`)) ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;";
		
			
			if(rHandler==null)
			{
				throw new ArgumentException("DatabaseHandler has to be != null!!");
			}
			
			conn=rHandler;
		}
		
		
		private string createQuery(string whereBlock)
		{
			string query = "SELECT " + 
				"`"+colID + "`, " +
				"`"+colSender + "`, " +
				"`"+colReceiver + "`, " +
				"`"+colType + "`, " +
				"`"+colMsg + "` " +
				"FROM `" + tblName + "`";
			
			if(string.IsNullOrEmpty(whereBlock))
			{
				return(query);
			}
			else
			{
				return(query + " WHERE " + whereBlock);
			}
		}
		
		
		
		public bool Start(bool vCreateIfNotExist, out int oErrorCode)
		{
			if(!conn.Connect(out oErrorCode))
			{
				return(false);
			}
			
			if(vCreateIfNotExist)
			{
				if(!conn.WriteToDB(tableCreationQuery, out oErrorCode))
				{
					return(false);
				}
			}
			
			// Check if Table has right format
			MySql.Data.MySqlClient.MySqlDataReader reader;			
			return conn.ReadFromDB(createQuery(null), out reader, out oErrorCode);			
		}
		
		public bool Stop()
		{
			throw new NotImplementedException();
		}
	}
}
