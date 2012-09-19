/*
 * Created by SharpDevelop.
 * User: Thomas Bergmüller
 * Date: 02.04.2012
 * Time: 08:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using TBL.EDOLL;
using TBL.Data;
using MySql.Data.MySqlClient;
using TBL.IPC;
using TBL.Communication;


namespace debug_with_console
{
	
	class Program
	{	
		
		/*public static void Main(string[] args)
		{
			/*MySQLDatabaseHandler handler = new MySQLDatabaseHandler("localhost","gfi","gfi","habicht");
			handler.DebugMode = true;
			BlackboardDatabaseConnector conn = new BlackboardDatabaseConnector(handler,"MessageBoard");
			
			Blackboard board = new Blackboard(conn,5);
			
			Console.ReadLine();
			
			Console.WriteLine(TBL.Runtime.ExecutableDirectory);
			Console.ReadLine();
			
		}	// */
		
		public static void Main(string[] args)
		{
			DevComMaster[] masters = DevComMaster.CreateDevComsFromCSV(@"D:\ticker\software\tickerOS\tickerOS\bin\DebugVirtualOut\bus.conf");
			Console.ReadLine();
		
			
		}	//*/
	}
	
}