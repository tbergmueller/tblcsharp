/*
 * Created by SharpDevelop.
 * User: bert
 * Date: 02.04.2012
 * Time: 11:33
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using TBL.Communication;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TBL.Communication
{
	
	internal enum DevComTcpServerDataToWrite
	{
		Pending=0,
		NoResponseRequired=2,
		DataReady=1,
		
	}
	
	/// <summary>
	/// Description of DevComTcpServer.
	/// </summary>
	public class DevComTcpServer:IDevComHardwareInterface 
	{
		private TcpListener listener;
		private bool isListening = false;
		private Thread listenThread;
		private ThreadsafeReceiveBuffer recBuffer;		
		private DevComTcpServerDataToWrite toWrite;
		private byte[] dataToWrite;
		int recBufferSize= 2048;
		
		
		/// <summary>
		/// Initializes DevComserver
		/// </summary>
		/// <param name="vPort"></param>
		public DevComTcpServer(int vPort)
		{
			IPEndPoint endPnt = new IPEndPoint(IPAddress.Any, vPort);
			listener = new TcpListener(endPnt);
		}
		
		public ThreadsafeReceiveBuffer ReceiveBuffer
		{
			get
			{
				return(recBuffer);
			}
			set
			{
				recBuffer = value;
			}
		}
		
		public void setReceiveBuffer(ThreadsafeReceiveBuffer rReceiveBuffer)
		{
			recBuffer = rReceiveBuffer;
		}
		
		public int Baudrate
		{
			get
			{
				throw new TBL.Exceptions.NotImplemented("Baudrate is not do be set with TCP-Connections");
			}
			set
			{
				throw new TBL.Exceptions.NotImplemented("Baudrate is not do be set with TCP-Connections");
			
			}
		}
		
		public bool isConnected()
		{
			return(isListening);
		}
		
		public int Connect()
		{
			listenThread = new Thread(new ThreadStart(listenForClients));
			listenThread.Start();
			
			return(0);
		}
		
		
		
		public void Disconnect()
		{
			isListening = false; // Abschalten
		}
		
		public string GetInfo()
		{
			throw new TBL.Exceptions.NotImplemented("DevComTcpServer.GetInfo() To be done!!");
		}
		
		public int WriteData(byte[] rDataToWrite)
		{
			dataToWrite = rDataToWrite;
			toWrite = DevComTcpServerDataToWrite.DataReady;
			
			return(0);
		}
		
		public void RemainListening()
		{			
			toWrite = DevComTcpServerDataToWrite.NoResponseRequired;			
		}
				
		public void Shutdown()
		{
			isListening = false;
		}		
				
		private void listenForClients()
		{
			this.listener.Start();
			isListening = true;
			
			while(isListening) // Is listening has to be set to false to stop
			{
				TcpClient client = this.listener.AcceptTcpClient();
				
				Thread clientThread = new Thread(new ParameterizedThreadStart(handleClientCommunication));
				clientThread.Start(client);
			}
		}
				
		private void handleClientCommunication(object rClient)
		{
			TcpClient client = (TcpClient)(rClient);
			
			NetworkStream stream = client.GetStream(); 
			
			byte[] receiving = new byte[recBufferSize];
			
			while(true)
			{
				int bytesRead = 0;
				
				try
				{
					// blocks until a client sends a message
					bytesRead = stream.Read(receiving,0, recBufferSize);
					
				}
				catch
				{
					break; // Socket error
				}				
				
				if(bytesRead == 0)
				{
					break; // client has disconnected
				}
				
				byte[] receivedBytes = new byte[bytesRead];
				
				for(int i=0; i<bytesRead; i++)
				{
					receivedBytes[i] = receiving[i];
				}
								
				recBuffer.AddBytes(receivedBytes);		

				// wait for Data to write...
				while(toWrite == DevComTcpServerDataToWrite.Pending)
				{
					
				}
				
				if(toWrite == DevComTcpServerDataToWrite.DataReady)
				{
					// Answer					
					stream.Write(dataToWrite,0,dataToWrite.Length);
					stream.Flush();
				}
				
				toWrite = DevComTcpServerDataToWrite.Pending; // set to pending for next cycle...				
			}
			
			client.Close();			
		}		
	}
}
