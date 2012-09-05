/*
 * Created by SharpDevelop.
 * User: thomas
 * Date: 18/05/2012
 * Time: 21:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.Collections;

using System.Threading;
using System.Net.NetworkInformation;

using TBL.EDOLL;

namespace TBL.Networking
{
	/// <summary>
	/// Description of Networking.
	/// </summary>
	/// <remarks>I don't know why (yet), but this explorer only works with console-applications. I guess it has something to do with multithreading..</remarks>
	public class IPv4NetExplorer
	{
		private ArrayList foundIPs;
		private ArrayList foundHosts;
		ulong pingsToDo;
		bool additionalResolving = false;
		private bool debugMode = false;
		
		public IPv4NetExplorer()
		{
			foundIPs = new ArrayList();
			ArrayList.Synchronized(foundIPs); // For multithreading acess
			
			foundHosts = new ArrayList();
			ArrayList.Synchronized(foundHosts); // For multithreading acess
			
		}
		
		
		public bool FindIPHosts(IPAddress vNetworkAddress, IPAddress vSubnetMask , out IPHostEntry[] oFoundHosts, out int oErrorCode)
		{
				additionalResolving = true;
				IPAddress[] ips;
				
				bool pingSuccess = scanForIPs(vNetworkAddress, vSubnetMask, out ips, out oErrorCode);
				
				if(!pingSuccess)
				{
					oFoundHosts = null;
					return(false);
				}
				
				IPHostEntry[] entries = new IPHostEntry[foundHosts.Count];
				
				long cnt = 0;
			
				foreach(IPHostEntry entry in foundHosts)
				{
					entries[cnt++] = entry;
				}
				
				oFoundHosts = entries;
				oErrorCode = 0;
				return(true);				
		}
		
		
		
		public bool FindIPEndpoints(IPAddress vNetworkAddress, IPAddress vSubnetMask , out IPAddress[] oFoundEndpoints, out int oErrorCode)
		{
			additionalResolving = false;
			return(scanForIPs(vNetworkAddress,vSubnetMask,out oFoundEndpoints, out oErrorCode));
		}
		
		
		private bool scanForIPs(IPAddress vNetworkAddress, IPAddress vSubnetMask , out IPAddress[] oFoundEndpoints, out int oErrorCode)
		{
			if(!equalIPLengths(vNetworkAddress, vSubnetMask))
			{
				throw new ArgumentException("IP-Address and Subnetmask don't match in Length");
			}
			
			if(!IsNetworkAddress(vNetworkAddress, vSubnetMask))
			{
				throw new ArgumentException("The passed IP is no network address. Network Address is the one with the last Bits 0 ;)");
			}
			
			oErrorCode = 0;
			
			byte[] netAddressBytes = vNetworkAddress.GetAddressBytes();
			byte[] netmaskBytes = vSubnetMask.GetAddressBytes();
			
			foundIPs.Clear(); // New search...
			byte[] netAddr = vNetworkAddress.GetAddressBytes();
		
			byte[] bcAddr = GetBroadcastAddress(vNetworkAddress, vSubnetMask).GetAddressBytes();
			
			ulong lBCA = 0;
			ulong lNA = 0;
			
			for(int i=0; i<netAddr.Length; i++)
			{
				lBCA |= (ulong)((ulong)(bcAddr[i])<<(netAddr.Length-i-1)*8);
				lNA |= (ulong)((ulong)netAddr[i]<<(netAddr.Length-i-1)*8);
			}
			
			ulong diff = lBCA - lNA;
			
			IPAddress[] adresses = new IPAddress[diff-1];
			
			for(ulong i=1; i<diff; i++)
			{
				byte[] curAddr = new byte[netmaskBytes.Length];
				
				ulong lAddr = (lNA + i);
				
				for(int j=0; j<curAddr.Length; j++)
				{
					ulong thisByte = lAddr >> (curAddr.Length-j-1)*8;
					curAddr[j] = (byte)(thisByte & 0xff);
				}
				
				adresses[i-1] = new IPAddress(curAddr);	
			}
			
			
			// Got the adresses... now start pinging threads
			
			pingsToDo = diff-1;
			foundIPs.Clear();
			
			foreach(IPAddress ip in adresses)
			{
				Ping pinger = new Ping();				
				pinger.PingCompleted += new PingCompletedEventHandler(PingComplete);
				pinger.SendAsync(ip,100,ip);
			}
			
			while(pingsToDo > 0)
			{
				// wait, timeout set in Pings..
			}
			
			// Check if really Network address
			
			if(foundIPs.Count <= 0)
			{
				oErrorCode = -572;
				oFoundEndpoints = null;
				return(false);
			}
			IPAddress[] toReturn = new IPAddress[foundIPs.Count];
			
			long cnt = 0;
			
			foreach(IPAddress ip in foundIPs)
			{
				toReturn[cnt++] = ip;
			}
			oFoundEndpoints = toReturn;			
			return true;
		}
		
		private void PingComplete(object sender, PingCompletedEventArgs e)
		{
			
			if(!e.Cancelled && e.Error == null)
			{
				if(e.Reply.Status == IPStatus.Success)
				{
					
					foundIPs.Add(e.UserState as IPAddress);
					
					if(additionalResolving)
					{
						try
						{
							foundHosts.Add(Dns.GetHostEntry((e.UserState as IPAddress)));
						}
						catch(Exception ex)
						{
							if(debugMode)
							{
								stdOut.Error(ex.ToString(), (e.UserState as IPAddress).ToString());
							}
						}
					}
				}
			}
			
			pingsToDo--;			
		}
				
		public static IPAddress GetBroadcastAddress(IPAddress vIP, IPAddress vSubnetMask)
		{
			if(!equalIPLengths(vIP, vSubnetMask))
			{
				throw new ArgumentException("IP-Address and Subnetmask don't match in Length");
			}
			
			byte[] ip = vIP.GetAddressBytes();
			byte[] netmask = vSubnetMask.GetAddressBytes();
			byte[] bcAddr = new byte[netmask.Length];
				
			for(int i=0; i<netmask.Length; i++)
			{
				byte wildcardByte =(byte)(0xff-netmask[i]);
				bcAddr[i] = (byte)((byte)(ip[i]) | wildcardByte);
			}	
			
			return(new IPAddress(bcAddr));
		}
		
		public static IPAddress GetNetworkAddress(IPAddress vIP, IPAddress vSubnetMask)
		{
			if(!equalIPLengths(vIP, vSubnetMask))
			{
				throw new ArgumentException("IP-Address and Subnetmask don't match in Length");
			}
			
			byte[] addrBytes = vIP.GetAddressBytes();
			byte[] netmaskBytes = vSubnetMask.GetAddressBytes();
			
			byte[] netAddr = new byte[addrBytes.Length];
			
			for(int i=0; i<netAddr.Length; i++)
			{
				netAddr[i] = (byte)(addrBytes[i] & netmaskBytes[i]);
			}
			
			return new IPAddress(netAddr);			
		}
		
		public static bool IsNetworkAddress(IPAddress vIPAddress, IPAddress vSubnetMask)
		{
			return vIPAddress.Equals(GetNetworkAddress(vIPAddress,vSubnetMask)); // if Network address from passed IP is the passed IP its a network address ;)
		}
		
		private static bool equalIPLengths(IPAddress vIP1, IPAddress vIP2)
		{
			byte[] ip1 = vIP1.GetAddressBytes();
			byte[] ip2 = vIP2.GetAddressBytes();
			
			return(ip1.Length == ip2.Length);
		}
		
		
		
	}
	
	
}
