/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Gernot Scheibenpflug
 * Datum: 15.11.2010
 * Zeit: 16:49
 * Funktion: Hier werden alle Überprüfungen durchgeführt:
 * 			 - überprüfen, ob Adresseingabe richtig
 * 			 - überprüfen, ob Farbeingabe richtig
 * 			 - überprüfen, ob Verbindung aufgebaut werden kann
 * 			 - überprüfen der IP Adresse auf korrekte Eingabe.
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
 
using System;
using System.Text.RegularExpressions;
using System.IO;


namespace TBL
{	
	
	/// <summary>
	/// Die statische Klasse Check beinhaltet diverse Prüfmethoden, die allesamt <see cref="EDOLL">EDOLL-Implementiert</see> sind.
	/// </summary>
	public static class Check
	{
		private static int maxAdress = 255;		// maximale Adresse 0xff
												// minimale Adresse 0x01
		
		/// <summary>
		/// Prüft Portnamen auf syntaktische Richtigkeit (nicht Existenz!!)
		/// </summary>
		/// <param name="pPortname">Der zu überprüfende Portname</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLL">EDOLL-Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Portname in Ordnung</description></item>
		/// 	<item><term>-610</term><description>Ungültiger Portname</description></item>
		/// </list>
		/// </returns>
		/// <remarks><para><img src="img/monoTested.png" /> Mono-getestet.</para>
		/// <para>Unterstützte Syntax für Portnamen: "COM*" und "/dev/ttyS*" und "/dev/ttyUSB*"	... * steht für eine beliebige positive Ganzzahl</para>
		/// </remarks>
		public static int CheckSerialPortName(string pPortname)
		{
			if(pPortname.ToLower().Contains("com"))
			{
				string rest = pPortname.ToUpper().Replace("COM","");		// Windows
				
				int portNr = -1;
				try
				{
					portNr = System.Convert.ToInt32(rest);
					if(portNr > 0)
					{
						return(0);
					}
					else
					{
						return(-610);
					}
				}
				catch
				{
					return(-610);
				}		
			}
			else
			{
				
				string rest = pPortname.Replace("/dev/ttyS", "");			// Mono-Implementierung unter Linux
				rest = rest.Replace("/dev/ttyUSB", "");			// Mono-Implementierung unter Linux
				int portNr = -1;
				try
				{
					portNr = System.Convert.ToInt32(rest);
					if(portNr >= 0)
					{
						return(0);
					}
					else
					{
						return(-610);
					}
				}
				catch
				{
					return(-610);
				}		
			}			
		} 
		
		
		
		/// <summary>
		/// Prüft den Dateiheader auf Syntax und die Versionsnummer auf Bibliotheksverträglichkeit 
		/// </summary>
		/// <param name="pConfigFilePath">Pfad der zu überprüfenden Konfigurationsdatei</param>
		/// <param name="pConfigFileType">Dateitypbezeichnung (string)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLL">EDOLL-Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Adresse in Ordnung</description></item>
		/// 	<item><term>-523</term><description>Ungültige Datensatzlänge</description></item>
		/// 	<item><term>-526</term><description>Konfigurationsdatei kann nicht zum Lesen geöffnet werden</description></item>
		/// 	<item><term>-534</term><description>Konfigurationsdateityp stimmt nicht.</description></item>
		/// 	<item><term>-535</term><description>Falsche Dateiversion, stimmt nicht mit Bibliotheksversion überein.</description></item>
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>
		/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
		/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
		/// 	(siehe <see cref="TBL.EDOLL.EDOLLHandler.GetLastError">EDOLLHANDLER.getLastError()</see>)
		/// 	</para>
		/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
		/// <list type="table">
		/// 	<listheader><term><see cref="TBL.EDOLL.EDOLLHandler.GetLastError">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Fileheader in Ordnung</description></item>	
		/// 	<item><term>-523</term><description>Ungültige Datensatzlänge in Konfigurationsfile</description></item>
		/// 	<item><term>-526</term><description>Konfigurationsdatei kann nicht zum Lesen geöffnet werden</description></item>
		/// 	<item><term>-534</term><description>Konfigurationsdateityp stimmt nicht.</description></item>
		/// 	<item><term>-535</term><description>Falsche Dateiversion, stimmt nicht mit Bibliotheksversion überein.</description></item>
		/// </list>
		/// </remarks>
		public static int CheckFileHeaderCSV(string pConfigFilePath, string pConfigFileType)
		{
			StreamReader sr;
			
			try
			{
				sr= new StreamReader(pConfigFilePath);
			}
			catch
			{
				return(-526); // Datei kann nicht geöffnet werden UNDONE Exception here?		
			}
			
			try
			{
				string header = sr.ReadLine();
				sr.Close();
				
				return(CheckCSVHeader(header, pConfigFileType));
			}
			catch
			{
				return(-523); // Syntax error
				
			}
			
		}

		/// <summary>
		/// Prüft den Dateiheader auf Bibliothekskompabilität
		/// </summary>
		/// <param name="pHeaderStr"></param>
		/// <param name="pConfigFileType"></param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLL">EDOLL-Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Header in Ordnung</description></item>
		/// 	<item><term>-523</term><description>Ungültige Datensatzlänge</description></item>
		/// 	<item><term>-534</term><description>Konfigurationsdateityp stimmt nicht.</description></item>
		/// 	<item><term>-535</term><description>Falsche Dateiversion, stimmt nicht mit Bibliotheksversion überein.</description></item>
		/// </list>
		/// </returns>
		public static int CheckCSVHeader(string pHeaderStr, string pConfigFileType)
		{
			string[] header = pHeaderStr.Split(';');
			
			if(header.Length != 2)
			{
				return(-523); // Syntax error					
			}
			
			if(header[0].ToLower() != pConfigFileType.ToLower())
			{
				return(-534); // Syntax error
				
			}
			
			if(header[1] != Runtime.LibraryVersion)
			{
				return(-535); // Syntax error
				
			}	

			return(0);
		}
		
		
		/// <summary>
		/// Prüft M3S-Slaveadresse auf Gültigkeit (muss sich bei M3S V1.X im Bereich 1...255 befinden)
		/// </summary>
		/// <param name="pAdress">Zu prüfende Adresse</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>EDOLL-Fehlernummer</term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Adresse in Ordnung</description></item>
		/// 	<item><term>-602</term><description>Adresse außerhalb des Bereichs</description></item>
		/// </list>
		/// </returns>
		public static int CheckM3SSlaveAddress(int pAdress) // überprüfen, ob Adresse korrekt eingegeben wurde...
		{
			if (pAdress > maxAdress)
			{
				return(-602);
			} 
			else if (pAdress <= 0)
			{
				return(-602);
			}
			else
			{
				return(0);
			}
		}

		/// <summary>
		/// Prüft RGB-String auf syntaktische Richtigkeit.
		/// </summary>
		/// <param name="pColor">Zu prüfe Farbe als String</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>EDOLL-Fehlernummer</term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Adresse in Ordnung</description></item>
		/// 	<item><term>-603</term><description>Ungültiger Farbstring</description></item>
		/// </list>		
		/// </returns>
		public static int CheckRGBString(string pColor)
		{
			// überprüfen, ob korrekter Farbwert eingegeben wurde...
			pColor.Replace("#", "");
			
			if (pColor.Length != 6)
			{
				return(-603);
			}
			else
			{
				Regex myRegex = new Regex("[A-Fa-f0-9]");
				bool test = myRegex.IsMatch(pColor);
				if (test == true)
				{
					return(0);
				}
				else
				{
					return(-603);
				}
			}
		}
		
		
		/// <summary>
		/// Überprüft IP (string) auf syntaktische Richtigkeit
		/// </summary>
		/// <param name="IP">IP als String</param>
		/// <returns>
		///<list type="table">
		/// 	<listheader><term>EDOLL-Fehlernummer</term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Adresse in Ordnung</description></item>
		/// 	<item><term>-604</term><description>Ungültige IP</description></item>
		/// </list>		
		/// </returns>
		public static int CheckIP(string IP)
       	{
			if (Regex.IsMatch(IP, @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b"))
           {
   				return(0);
   			}
           else
			{
				return(-604);
        	}
		}
			
		///<summary>
		/// Prüft einen Wert ob er im prozentualen Dimmbereich (0...100) liegt
		/// </summary>
		/// <param name="val">Zu prüfender Wert</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>EDOLL-Fehlernummer</term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Adresse in Ordnung</description></item>
		/// 	<item><term>-605</term><description>Wert außerhalb des gültigen Bereichs</description></item>
		/// </list>	
		/// </returns>
		public static int CheckDimmfactor(int val)
		{
			if(val >= 0 && val <=100)
			{
				return(0);
			}
			else
			{
				return(-606); 
			}
		}
	}
}
