/*
 * Created by SharpDevelop.
 * User: bert
 * Date: 02.05.2012
 * Time: 09:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using TBL.Communication.Protocol;

namespace TBL.Communication
	
{
	/// <summary>
	/// Virtual Two Wire Interface via M3S-Protocol.
	/// </summary>
	/// <remarks>
	/// This is some more or less freaky stuff and was originally created for Virtualization purposes and remote maintenance. A Microcontroller (it was tested with Atmega88) is connected via M3S-Interface to a PC/embedded computer/... on the one and and acts as a TWI/IIC-Master on the other end. 
	/// //TODO: Bild
	/// </remarks>
	public class VirtualTWIMaster
	{
		private DevComMaster master;
		private int m3sAddr;
		/// <summary>
		/// Creates a new Instance of VirtualTWI-Master (which is operating via M3S-Master/Slave)
		/// </summary>
		/// <param name="rDevComMaster">Ready Instance of M3S-Master</param>
		/// <param name="vM3SSlaveAddress">Address of M3S-Slave which is operating as a M3SSlave-to-TWIMaster</param>
		public VirtualTWIMaster(DevComMaster rDevComMaster, int vM3SSlaveAddress)
		{
			int checkValue = TBL.Check.CheckM3SSlaveAddress(vM3SSlaveAddress);
			if( checkValue != 0)
			{
				throw new System.ArgumentException("The passed vM3SSlaveAddress is invalid!! "+ EDOLL.EDOLLHandler.Verbalize(checkValue) + System.Environment.NewLine + "Refer Documentation of M3S-Protocol for details. ");
			}
			master = rDevComMaster;
			m3sAddr = vM3SSlaveAddress;
		}
		
		public bool Send(int vTWIAddress, byte[] rDataBytes)
		{
			int dummy;
			return this.Send(vTWIAddress, rDataBytes, out dummy);
		}
		/// <summary>
		/// Sends Data on the TWI-Bus via the M3S-Slave (=TWI-Master)
		/// </summary>
		/// <param name="vTWIAddress">Address of the Slave on TWI-Bus</param>
		/// <param name="rDataBytes">Databytes that should be transfered on TWI-Bus to TWI-Slave</param>
		/// <param name="oErrorCode">Output parameter: <see cref="EDOLLHandler">EDOLL-Errorcode</see> (0 if successful)</param>
		/// <returns>successful...true, failure...false, Details via oErrorCode</returns>
		/// <remarks>
		/// 	<para>On the M3S-Communication this operates as a Command Unicast with response required (Request).</para>
		/// 	<para>If the execution of TWI-Task was successful, the M3S-Slave returns a CommandResponse-Frame with the TWI-Slaveaddress.</para>
		/// 	<para>If errors occured while executing the TWI-Task, M3S-Slave returns in a CommandResponse-Frame the bitwise complement of the TWI-Slaveaddress. The following Bytes inside the payload are representing the TWI-Errorcode</para>
		/// </remarks>
		
		public bool Send(int vTWIAddress, byte[] rDataBytes, out int oErrorCode)
		{
			byte[] cmdFrame = new byte[rDataBytes.Length + 1 + 1];
						
			cmdFrame[0] = (byte)M3SCommand.TWIWrite;
			cmdFrame[1] = (byte)(vTWIAddress);
			
			for(int i=0; i<rDataBytes.Length; i++)
			{
				cmdFrame[i+2] = rDataBytes[i];
			}
			byte[] response;
			
			
			if(!master.Request(m3sAddr, cmdFrame, out response, out oErrorCode))
			{
				return(false);
			}
			else
			{
				if(response.Length <= 1)
				{
					oErrorCode = -219; // VTWIM: Wrong Response format
					return(false);
				}
				
				if(response[0] == vTWIAddress)
				{
					return(true);
				}
				else
				{
					if(response.Length != 3)
					{
						oErrorCode = -219; // VTWIM: Wrong Response format
						return(false);
					}
					
					oErrorCode = ((int)(response[1])<<8 | (int)(response[2])); // convert error code..
					return(false);
				}
			}
		}
		
		/// <summary>
		/// Reads response from a specific slave on TWI Bus after sending initial Commandbytes.
		/// </summary>
		/// <param name="vTWIAddress">Address of the Slave on TWI-Bus</param>
		/// <param name="rCommandBytes">Databytes of initial Command (sent before reading the response on TWI bus)</param>
		/// <param name="vExpectedResponseByteAmount">Amount of Databytes that are expected to be received from TWI-Slave</param>
		/// <param name="oReadData">Output-Parameter: Read data, null if process fails</param>
		/// <param name="oErrorCode">Output parameter: <see cref="TBL.EDOLL.EDOLLHandler">EDOLL-Errorcode</see> (0 if successful)</param>
		/// <returns>successful...true, failure...false, Details via oErrorCode</returns>
		/// <remarks>
		/// <para>On the M3S-Communication this operates as a Command Unicast with response required (Request).</para>
		/// <para>If the execution of TWI-Task was successful, the M3S-Slave returns a CommandResponse-Frame with the TWI-Slaveaddress.</para>
		///	<para>When executing the command, Payload of the M3S-Frame is structured as the following:
		/// <list type="bullet">
		/// 	<item>Byte 0: Command 't'</item>
		/// 	<item>Byte 1: TWI-Slaveaddress (max. 127)</item>
		/// 	<item>Byte 2: Amount of expected Databytes that will be received from TWI-Slave</item>
		/// 	<item>Byte 3..n: Initial Commanddatabytes</item>
		/// </list>
		/// </para>
		/// </remarks>
		public bool Read(int vTWIAddress, byte[] rCommandBytes, int vExpectedResponseByteAmount, out byte[] oReadData, out int oErrorCode)
		{
			byte[] cmdFrame = new byte[rCommandBytes.Length + 1 + 2];
			oReadData = null;			
			cmdFrame[0] = (byte)M3SCommand.TWIRead;
			cmdFrame[1] = (byte)(vTWIAddress);
			cmdFrame[2] = (byte)(vExpectedResponseByteAmount);
			
			for(int i=0; i<rCommandBytes.Length; i++)
			{
				cmdFrame[i+3] = rCommandBytes[i];
			}
			
			byte[] response;
			
			if(!master.Request(m3sAddr, cmdFrame, out response, out oErrorCode))
			{
				return(false);
			}
			else
			{
				if(response[0] == vTWIAddress) // successful
				{
					oReadData = new byte[response.Length-1];
					
					for(int i=0; i<response.Length-1; i++)
					{
						oReadData[i] = response[i+1];
					}
					
					// TODO: Extract response bytes
					return(true);
				}
				else
				{
					if(response.Length != 3)
					{
						oErrorCode = -219; // wrong format or wrong slave answered
						return(false);
					}
					
					oErrorCode = ((int)(response[1])<<8 | (int)(response[2])); // convert error code..
					return(false);
				}
			}
			
			
			
		}
	}
}
