/*
 * Created by SharpDevelop.
 * User: bert
 * Date: 29.08.2012
 * Time: 10:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using TBL.Communication;



namespace M3STools
{
	class Program
	{
		private static void printAvailableComports()
		{
			foreach(String s in System.IO.Ports.SerialPort.GetPortNames())
			{
				Console.WriteLine(s);
			}
		}
		
		private static void ping(string[] mainArgs)
		{
			TBL.Communication.IDevComHardwareInterface hwIfc;
			
			if(0==TBL.Check.CheckSerialPortName(mainArgs[1]))
			{
				int baudrate;
				
				
				if(mainArgs.Length < 4)
				{
					Console.WriteLine("Baudrate defaults to "+TBL.Communication.devComSerialInterface.DefaultBaudrate+"bps");
					baudrate = TBL.Communication.devComSerialInterface.DefaultBaudrate;
				}
				else
				if(!int.TryParse(mainArgs[3], out baudrate))
				{
					Console.WriteLine("The specified Baudrate in parameter 4 suffers from invalid syntax: '"+mainArgs[3]+"'");
					return;					
				}
								
				
				hwIfc = new TBL.Communication.devComSerialInterface(mainArgs[1], baudrate);
				
								
				
				  
				
			}
			else if(0==TBL.Check.CheckIP(mainArgs[1]))
			{
				throw new NotImplementedException();
			}
			else
			{
				Console.WriteLine("The specified Interface (parameter 2) suffers from invalid syntax: '"+mainArgs[1]+"'");
				return;
			}
			
			int slaveAddr;
			DevComMaster m3sMaster = new DevComMaster(hwIfc);
								
			if(!int.TryParse(mainArgs[2], out slaveAddr))
			{
				Console.WriteLine("The specified Slave-Address in parameter 3 suffers from invalid syntax: '"+mainArgs[2]+"'");
				return;					
			}				
			
			
			
			int err;
			
			if(!m3sMaster.Connect(out err))
			{
				Console.WriteLine("Could not connect on Interface." + Environment.NewLine + TBL.EDOLL.EDOLLHandler.Verbalize(err));
			}
			long rtt;
			
			if(m3sMaster.Ping(slaveAddr, out rtt, out err))
			{
				Console.WriteLine("Ping to Slave #"+slaveAddr.ToString()+" successful, RTT=" + rtt.ToString());
			}
			else
			{
				Console.WriteLine("ReadTimeout reached when pinging Slave #"+slaveAddr);
			}
			
			
			byte[] payload = {0xF0, 0xC0, 0xC0 , 0xF8 , 0x00 , 0xC0 , 0x80 , 0x00 , 0xC0 , 0xC0 , 0x80 , 0xC0 , 0x00 , 0x00 , 0x80 , 0x00 , 0xC0 , 0xC0 , 0xC0 , 0xC0 , 0x80 , 0xC0 , 0xC0 , 0x00 , 0xF8 , 0xF0 , 0xC0 , 0xC0 , 0xF0 , 0xC0 , 0xC0 , 0xF8};
			byte[] updateCmd = {(byte)TBL.Communication.Protocol.M3SCommand.Update};
			byte[] errorCMD = {0xff};
			
			if(!m3sMaster.SendCommand(1, errorCMD, out err, true))
			{
				TBL.EDOLL.stdOut.Error(TBL.EDOLL.EDOLLHandler.Verbalize(err));
			}
			
			
			if(!m3sMaster.SendData(1,payload, out err, true))
			{
				TBL.EDOLL.stdOut.Error(TBL.EDOLL.EDOLLHandler.Verbalize(err));
			}
			
			if(!m3sMaster.SendCommandBroadcast(updateCmd, out err))
			{
				TBL.EDOLL.stdOut.Error(TBL.EDOLL.EDOLLHandler.Verbalize(err));
			}
		}
		
		
		
		public static void Main(string[] args)
		{
			
			if(args.Length <= 0)
			{
				Console.WriteLine("FATAL error: No Command specified");
				Environment.Exit(-1);
			}
			
			
			switch(args[0].ToLower()) // This is the command
			{
					case "ping": ping(args);
					break;
					
					case "listports": printAvailableComports();
					break;
					
					
					
					default: Console.WriteLine("Unknown Command '"+args[0]+"'");
					break;
			}
			
			#if DEBUG
			Console.ReadLine();
			#endif
		}
	}
}