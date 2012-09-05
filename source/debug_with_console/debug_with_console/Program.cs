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
			IPHostEntry[] entries;
			int err;
			
			Console.Write("Enter Network-Address: ");
			string netaddr = Console.ReadLine();
			Console.WriteLine("Scanning network...");
			if((new TBL.Networking.IPv4NetExplorer()).FindIPHosts(IPAddress.Parse(netaddr), IPAddress.Parse("255.255.255.0"), out entries, out err))
			{
				foreach(IPHostEntry entry in entries)
				{
					Console.WriteLine(entry.AddressList[0].ToString() + " ... " + entry.HostName.ToString());
	
				}
				
				Console.ReadLine();
			}
			
		}	//*/
	}
	
}