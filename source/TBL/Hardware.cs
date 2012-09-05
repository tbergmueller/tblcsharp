/*
 * Created by SharpDevelop.
 * User: Thomas Bergmüller
 * Date: 15.04.2011
 * Time: 10:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Management;
using System.Net.NetworkInformation;

using System.Collections;

namespace TBL.Hardware
{
	/// <summary>
	/// Liefert diverse Hardwareinformationen
	/// </summary>
	public static class HardwareInfo
	{
		/// <summary>
		/// Liefert die Identifikationsnummer des Prozessors
		/// </summary>
		/// <returns>ProzessorID als string</returns>
		public static string GetProcessorID()
		{
			return(getInformation("ProcessorID", "WIN32_Processor"));
		}
		
		
		// Erstellt einen Identifikationsstring
		
		/// <summary>
		/// Erstellt einen Hardware-Identifikationsstring aus ProcessorID (not unique) und MAC-Adresse XOR-Verschlüsselt mit einem Key
		/// </summary>
		/// <param name="pXORKey">Key, mit dem jedes Zeichen XORED werden soll..</param>
		/// <returns>verschlüsselten Identifikationsstring</returns>
		/// <remarks></remarks>
		public static string GetIdentificationString(byte pXORKey)
		{
			string identityString = GetIdentificationString();
			
			string cryptedIdentityString = "";
			// Verschlüsseln
			for (int i = 0; i < identityString.Length; i++)
            {
                int charValue = Convert.ToInt32(identityString[i]); //get the ASCII value of the character
                charValue ^= (int)pXORKey; //xor the value

                cryptedIdentityString += char.ConvertFromUtf32(charValue); //convert back to string
            }
			
			// FIXME: Search for non printable chars, bei Hr. Hug am Notebook wird whs. so eines mitgegeben?
			cryptedIdentityString = cryptedIdentityString.Trim();
			
			
		
			return(cryptedIdentityString);			
		}
		
		/// <summary>
		/// Erstellt einen Hardware-Identifikationsstring aus PRocessorID(not unique) und MAC-Adresse
		/// </summary>
		/// <returns>Identifikationsstring</returns>
		public static string GetIdentificationString()
		{			
			// TODO: test on windows...
			string identityString = GetProcessorID();
			string sTrimmed;
			NetworkInterface[] macs = FindMACAddresses();
			string[] macAddresses = new string[macs.GetUpperBound(0)+1];
			for(int i=0; i<=macs.GetUpperBound(0); i++)
			{
				macAddresses[i] = macs[i].GetPhysicalAddress().ToString();
			}
			
			foreach(string s in macAddresses)
			{
				sTrimmed = s.Trim();
				
				string validString = "";
			
				for(int i=0; i<sTrimmed.Length; i++)
				{
					char c = (char)sTrimmed[i]; // Zeichen herausgreifen
					
					if(c >= 32 && c <254)
					{
						validString += c.ToString();
					}
				}
				
				
				identityString += "_" + validString;
			}		
			
			return(identityString);			
		}
		
		/// <summary>
		/// Liefert eine gewisse HArdwareinformation
		/// </summary>
		/// <param name="pField">Name der auszulesenden Eigenschaft</param>
		/// <param name="pContainername">Name des Containers, aus dem gelesen werden soll (zB. Win32_Processor)</param>
		/// <returns></returns>
		private static string getInformation(string pField, string pContainername)
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT " + pField + " FROM " + pContainername);
			
			foreach(ManagementObject info in searcher.Get())
			{
				return((string)info.GetPropertyValue(pField)); // einfach das erste zurückgeben..
			}
			
			return(string.Empty);			
		}
		
		/// <summary>
		/// Ermittelt die erste physikalische MAC-Adresse des Systems
		/// </summary>
		/// <returns>Erste physikalische MAC-Adresse</returns>
		public static PhysicalAddress FindFirstMACAddress()
		{
			NetworkInterface[] allMAC = FindMACAddresses();
			PhysicalAddress toReturn=null;
			
			if(allMAC == null)
			{
				return(toReturn);
			}
			
			if(OperatingSystem.IsUnix) // Mono
			{
				foreach(NetworkInterface adapter in allMAC)
				{
					if(adapter.Description == "eth0")
					{
						toReturn = (adapter.GetPhysicalAddress());
					}
				}
			}
			else
			{
				return((allMAC[0] as NetworkInterface).GetPhysicalAddress());
			}
			
			return(toReturn);
		}
		
		/// <summary> 
		/// Ermittelt alle physikalischen MAC-Adressen des Systems
		/// </summary>
		/// <returns>Liste der gefundenen MAC-Adressen</returns>
		/// <remarks>NOT Mono compatible</remarks>
		public static string[] FindPhysicalMACAddresses()
		{
		    //create out management class object using the
		    //Win32_NetworkAdapterConfiguration class to get the attributes
		    //af the network adapter
		    ManagementClass mgmt = new ManagementClass("Win32_NetworkAdapterConfiguration");
		    //create our ManagementObjectCollection to get the attributes with
		    ManagementObjectCollection objCol = mgmt.GetInstances();
		    string[] strAddresses;
		    
		    ArrayList addresses = new ArrayList();
		    
		    
		    //loop through all the objects we find
		    foreach (ManagementObject obj in objCol)
		    {		       
		            //grab the value from the first network adapter we find
		            //you can change the string to an array and get all
		            //network adapters found as well
		            if ((bool)obj["IPEnabled"] == true)
		            {
		            	addresses.Add(obj["MacAddress"].ToString());
		            }
		        
		        //dispose of our object
		        obj.Dispose();
		    }
		    //replace the ":" with an empty space, this could also
		    //be removed if you wish
		    //address = address.Replace(":", "");
		    //return the mac address
		    
		    strAddresses = new string[addresses.Count];
		    
		    for(int i=0; i<addresses.Count; i++)
		    {
		    	strAddresses[i] = addresses[i] as string;
		    }
		    
		    return strAddresses;
		} 
		
		/// <summary>
		/// Creates a list of all MAC-Adresses on the system.
		/// </summary>
		/// <returns>List of MAC-Addresses, null in case of error</returns>
		/// <remarks>Mono-compatible</remarks>
		public static NetworkInterface[] FindMACAddresses()
		{
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();            
            return(nics);
		}
		
		
		
			
		/// <summary>
		/// Liefert Gewisse Hardwareinformationen, bei mehreren vorhandenen Datensätzen werden diese durch einen Separator getrennt zurückgegeben (in einem String)
		/// </summary>
		/// <param name="pField">Name der auszulesenden Eigenschaft</param>
		/// <param name="pContainername">Name des Containers, aus dem gelesen werden soll (zB. Win32_Processor)</param>
		/// <param name="pSeparator">Trennzeichen bei der Rückgabe mehrerer Parameter</param>
		/// <returns>String mit ausgelesenen Hardwareparameter(s)</returns>
		private static string getInformation(string pField, string pContainername, string pSeparator)
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT " + pField + " FROM " + pContainername);
			string toReturn = string.Empty;
			
			foreach(ManagementObject info in searcher.Get())
			{
				try
				{
					if(toReturn != "" || toReturn == string.Empty)
					{
						toReturn += pSeparator;
					}
					
					toReturn +=(string)info.GetPropertyValue(pField); // einfach das erste zurückgeben..
				}
				catch
				{
					
				}
				
			}
			
			return(toReturn);			
		}
		
	}
}