/*
 * Created by SharpDevelop.
 * User: Thomas Bergmüller
 * Date: 14.05.2012
 * Time: 11:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
 using System;
using TBL.Communication;
using TBL.Communication.Protocol;
using TBL.EDOLL;

namespace TBL.Communication
{
	
	public enum DevComSlaveState
	{
		NotReady=0,
		Ready=1,
	}
	
	
	public delegate bool DevComSlaveOnCommandReceived(object sender, M3SCommandReceivedEventArgs e);
	
	public class M3SCommandReceivedEventArgs: EventArgs
	{
		IM3S_Dataframe frame;
		byte cmd;
		byte[] paramBytes;
		
		public M3SCommandReceivedEventArgs(IM3S_Dataframe rReceivedFrame)
		{
			frame = rReceivedFrame;
			byte[] frameBytes = frame.GetDataframe();
			int upperBound = frameBytes[2]; // M3s-Upperbound
			
			cmd = frameBytes[3];
			paramBytes = new byte[upperBound];
			
			for(int i=0; i<upperBound; i++)
			{
				paramBytes[i] = frameBytes[i+4];
			}
			
				
		}
		
		public IM3S_Dataframe ReceivedFrame
		{
			get
			{
				return(frame);
			}
		}
		
		public byte M3SCommand
		{
			get
			{
				return(cmd);
			}
		}
		
		public byte[] ParameterBytes
		{
			get
			{				
				return(paramBytes);
			}
		}
	}
	
	
	public class DevComSlave
	{
		IDevComHardwareInterface hwInterface;
		ThreadsafeReceiveBuffer recBuffer;
		DevComSlaveState state;
		IM3S_Handler protocol = new M3S_V2_Handler();
		
		bool debugmode = false;
		
		// Slave Parameter
		int slaveAddr;	
		int mcAddr = 0;
		short deviceID = 0;
		byte[] validData;
		
		int dataUpperBound = 0;		
		const byte protocol_version = 2;
		const byte protocol_subv = 0;
		const byte protocol_revision = (byte)('a');
		const byte implementation = 1;
		
		private DevComSlaveOnCommandReceived delegateCommand;
		
		bool ackAlreadySent = false;
		
		
		byte[] resetFrame; 								// Aus performancegründen extra abspeichern
		
		// Some stuff for humble receiving => Frame error detection...
		
		
		public int MulticastAddress
		{
			get
			{
				return(mcAddr);
			}
			set
			{
				if(value <=255)
				{
					mcAddr = (byte)value;
				}
			}
		}
		
		public short DeviceID
		{
			get
			{
				return(deviceID);
			}
			set
			{
				deviceID = value;
			}
		}
		
		public int DataUpperBound
		{
			get
			{
				return(dataUpperBound);
			}
			set
			{
				if(value >= 0 && value <= 255)
				{
					dataUpperBound = value;
				}
			}
		}
		
		
		public DevComSlave(IDevComHardwareInterface rInterface, int vSlaveAddress)
		{
			hwInterface = rInterface;
			recBuffer = new ThreadsafeReceiveBuffer();			
			hwInterface.setReceiveBuffer(recBuffer);
			slaveAddr = vSlaveAddress;
			
			recBuffer.ByteReceived += new ByteReceivedEventHandler(recBuffer_ByteReceived);
			resetFrame = protocol.GetResetFrame().GetDataframe();
			state = DevComSlaveState.NotReady;
			
			
		}
		
		private void reset()
		{
			state = DevComSlaveState.Ready;	
			
		}
		
		private void goNotReady()
		{
			state = DevComSlaveState.NotReady;
		}
		
		
		int cnt = 0;
		
		private bool processCommand(IM3S_Dataframe rDataFrame, out int oErrorCode)
		{
			oErrorCode = -555;
			byte[] payload = protocol.ExtractPayload(rDataFrame.GetDataframe());
			
			byte cmd = payload[0];
			
			switch(cmd)
			{
			       case (byte)M3SCommand.GetInformation:
						oErrorCode = sendSlaveInfo(rDataFrame); 
						break;
						
					case (byte)M3SCommand.Ping:
						oErrorCode = 0; // no error, ack is sent outside...
						cnt++;
						
						break;
						
						default: 
						
							if(delegateCommand !=null)
							{
								return delegateCommand(this, new M3SCommandReceivedEventArgs(rDataFrame));
							}
							return(false);
			}
			
			if(oErrorCode==0)
			{
				return(true);
			}
			else
			{
				return(false);
			}
			
			
			
			
		}
		
		public DevComSlaveOnCommandReceived ReceivedCustomCommands
		{
			get
			{
				return(delegateCommand);
			}
			set
			{
				delegateCommand = value;
			}
		}
		
		/// <summary>
		/// Delegates Frames to appropriate processing Methods
		/// </summary>
		/// <param name="rDataframe">Frame that should be processed</param>
		/// <returns>true, if frame was successfully processed, false otherwise</returns>
		private bool processFrame(IM3S_Dataframe rDataframe, out int oErrorCode)
		{
			oErrorCode = -555;
			if(rDataframe.Protocol == M3SProtocol.Command || rDataframe.Protocol == M3SProtocol.CommandBroadcast)
			{
				return(processCommand(rDataframe, out oErrorCode));
			}
						
			return(false);
		}
		
		public bool SendCommandResponse(IM3S_Dataframe rCommandFrame, byte[] rResponseBytes, out int oErrorCode)
		{
			oErrorCode = 0;
			if(rCommandFrame.Protocol == M3SProtocol.Command)
			{
				IM3S_Dataframe toSend = protocol.CreateFrame(rCommandFrame.SlaveAddress, M3SProtocol.CommandResponse,rCommandFrame.MasterAddress,rResponseBytes,false,true);
				oErrorCode = writeToHardware(toSend);
				
				if(oErrorCode == 0)
				{
					return(true);
				}
				else
				{
					return(false);
				}
			}
			else
			{
				throw new TBL.Exceptions.FrameError("In DevComSlave.SendCommandResponse: passed rCommandFrame has a wrong protocol, only Command-Protocol allowed here...");
				
			}
		}
		
		
		private int sendSlaveInfo(IM3S_Dataframe requestFrame)
		{			
			byte[] payload = {
				protocol_version,
				protocol_subv,
				protocol_revision,
				implementation,
				(byte)(slaveAddr),
				(byte)(mcAddr),
				(byte)(deviceID >> 8),
				(byte)(deviceID & 0xff),
				(byte)(dataUpperBound),				
			};
			
			IM3S_Dataframe frameToSend = protocol.CreateFrame(slaveAddr, M3SProtocol.CommandResponse, requestFrame.MasterAddress, payload, false, true);
			
			return writeToHardware(frameToSend);			
		}
		
		
		
		private int writeToHardware(IM3S_Dataframe frameToWrite)
		{
			ackAlreadySent = true;
			somethingToSend = true;
			return(hwInterface.WriteData(frameToWrite.GetDataframe()));
		}
		
		bool somethingToSend = false;
		
			
		private void recBuffer_ByteReceived(object sender, EventArgs e)
		{
			int err;
			
			while(recBuffer.DataPtr > protocol.MinimumFrameLength) // während im Buffer entsprechen dda
			{
				somethingToSend = false;
				bool ackResult = false;
				// immer von vorne her anschaun
				
				
				if(state == DevComSlaveState.NotReady)
				{
					for(int i=0; i<resetFrame.Length && i<recBuffer.DataPtr; i++)
					{
						if(recBuffer.ReadByte(i) != resetFrame[i]) // If it doesnt match frame
						{
							recBuffer.FreeBytes(0,i); // Remove Bytes incl. faulty byte
							recBuffer.Flush();			
							break;								
						}
						
						if(i == resetFrame.GetUpperBound(0)) // got here without error => reset gotten
						{
							recBuffer.FreeBytes(0,i);
							recBuffer.Flush();
							reset();
						}
					}					
				}
				else
				{
					for(int bufferIdx=protocol.MinimumFrameLength-1; bufferIdx<recBuffer.DataPtr; bufferIdx++)
					{
						byte[] possFrame = recBuffer.readBytes(0, bufferIdx);
						
						if(protocol.IsFrame(possFrame))
						{
							// Do something with the frame
							IM3S_Dataframe frame = protocol.CreateFrameByBytestream(possFrame, out err);
							
							if(err != 0)
							{
								if(debugmode)
								{
									stdOut.Error("Could not convert Bytestream to valid M3S-Dataframe: " + TBLConvert.BytesToHexString(possFrame));									
								}
							}
							else
							{
								switch(frame.Protocol)
								{
									case M3SProtocol.CommandBroadcast:
										if(frame.MulticastAddress == 0 || frame.MulticastAddress == mcAddr)
										{
											ackResult = processFrame(frame, out err);
										}
										break;
									case M3SProtocol.DataTransfer:
										if(frame.SlaveAddress == slaveAddr)
										{
											ackResult = processFrame(frame, out err);
										}
										break;
										
									case M3SProtocol.Command:
										if(frame.SlaveAddress == slaveAddr)
										{
											ackResult = processFrame(frame, out err);
										}
										break;
										
									case M3SProtocol.BroadCast:
										if(frame.MulticastAddress == 0 ||frame.MulticastAddress == mcAddr)
										{
											ackResult = processFrame(frame, out err);
										}
										break;
								}
								if(frame.NeedsAcknowledgement && !ackAlreadySent)
								{
									hwInterface.WriteData(protocol.GetAcknowledgeFrame(ackResult, possFrame).GetDataframe());
									somethingToSend = true;
								}
							}							
							
							// remove the frame
							recBuffer.FreeBytes(0, bufferIdx);
							recBuffer.Flush();							
						}
					}
				}			
				

				if(!somethingToSend)
				{
					// Only for TCP clients
					try
					{
						DevComTcpServer serv = hwInterface as DevComTcpServer; // if castable, its a server
						
						serv.RemainListening();
					}
					catch
					{
						// do nothing instead
					}
				}
								
			}			
		}
		
		
		
		/// <summary>
		/// Connects Hardwareinterface and starts listening on Bus
		/// </summary>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler"><see cref="EDOLLHandler">EDOLL-Fehlercode</see></see>, 0 bei fehlerfreier Ausführung</param>
		/// <returns>
		/// Returns whether connection could be established (true) or not (false)
		/// </returns>
		public bool Connect(out int oErrorCode)
		{
			oErrorCode = 0;
			hwInterface.Connect();			
			return(true);
		}	
	}
	
	
}