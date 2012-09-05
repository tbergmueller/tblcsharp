using System;
using System.Net;
using System.IO;

using System.Windows.Forms;

namespace TBL.License
{
	/// <summary>
	/// Mögliche Antworten von der Onlineaktivierung
	/// </summary>
	public enum OnlineActivationResponse
	{
		/// <summary>
		/// Übertragener Schlüssel wurde in der übertragenen Anzahl der Datenbank hinzugefügt.
		/// </summary>
		KeyInserted = 4,
		/// <summary>
		/// Übertragener Schlüssel ist für diese Applikation bereits in der Datenbank vorhanden (bei CheckKey)
		/// </summary>
		KeyAlreadyExists = 2,
		/// <summary>
		/// Übertragener Schlüssel ist für diese Applikation noch nicht vorhanden und kann also verwendet werden (bei CheckKey)
		/// </summary>
		KeyNotExists = 3,
		/// <summary>
		/// Onlineaktivierung erfolgreich durchgeführt
		/// </summary>
		Successful = 1,
		/// <summary>
		/// Keine Verbindung zum Aktivierungszentrum
		/// </summary>
		NoConnection = 0,
		/// <summary>
		/// Onlineaktivierung verweigert, keine Aktivierungen mehr verfügbar (Zähler kleiner gleich 0)
		/// </summary>
		NoActivationLeft = -1,
		/// <summary>
		/// Der übergebene Key ist ungültig 
		/// </summary>
		InvalidKeyLength = -2, 
		/// <summary>
		/// Ungültiger Computerindentifikationsschlüssel, möglicherweise nicht identifizierbar
		/// </summary>
		/// <remarks>
		/// Dieser Fehler kann auftreten, wenn der PC über keine Netzwerkkarte verfügt und so keine zu verschlüsselnde MAC-Adresse hat.
		/// </remarks>
		InvalidComputerID = -3,
		/// <summary>
		/// ApplikationsID ist in der Onlinedatenbank nicht vorhanden
		/// </summary>
		ApplicationIDNotExists = -4,
		/// <summary>
		/// Aktivierungsvorgang wurde abgebrochen ohne näher spezifierten Grund
		/// </summary>
		ActivationCancelled = -5,
		/// <summary>
		/// Ungültiger Antwortcode der Onlineaktivierung - Syntax und in weiterer Folge Conversion-Fehler
		/// </summary>
		InvalidResponseCode = -6,
		/// <summary>
		/// Die Serveranwendung (php-Teil) kann keine Verbindung zur Datenbank aufbauen. Prüfen Sie Verbindung(sanzahl) und Einstellungen im Webinterface
		/// </summary>
		ServerCannotConnectDatabase = -7,
		/// <summary>
		/// Server kann die Datenbank nicht auswählen, evtl. zu viele Verbindungen, Datenbank existiert nicht usw. Prüfen Sie das Webinterface und action.php
		/// </summary>
		ServerCannotSelectDatabase = -8,
		
		
	}
	/// <summary>
	/// Stellt Routinen zum Generieren und Prüfen von 25-stelligen Keys zur Verfügung
	/// </summary>
	public static class Key
	{
		/// <summary>
		/// Länge eines Keys / Anzahl der Zeichen
		/// </summary>
		public static readonly int Length = 25;
		
		/// <summary>
		/// Erstellt einen neuen 25stelligen Key, in dem auch die Programm-ID verpackt ist
		/// </summary>
		/// <returns>Generierten Key</returns>
		public static string CreateNew()
		{
			Random rand = new Random();
			string toReturn = "";
			int tmpRand = 0;
				
			for(int i=0; i<Length; i++)
			{
				tmpRand = rand.Next('A'-10, 'Z');
				
				if(tmpRand < 'A')
				{
					toReturn += Convert.ToChar(tmpRand - 'A' + '9' + 1); 
				}
				else
				{
					toReturn += Convert.ToChar(tmpRand);
				}
				
			}
			
			// XOR von links nach rechts mit der ProgramID
			
			return(toReturn);
			
		}
		
		
		/// <summary>
		/// Registriert in der Onlinedatenbank einen Key (prüft aber vorher ob dieser Key bereits vorhanden ist)
		/// </summary>
		/// <param name="pAddress">URL zum Aktivierungsskript</param>
		/// <param name="pKey">Zu registrierender Key</param>
		/// <param name="pAppID">ApplikationsID als String</param>
		/// <param name="pAmount">Anzahl der zu vergebenen Lizenzen (so oft kann auf unterschiedlichen Rechnern aktiviert werden)</param>
		/// <returns></returns>
		public static OnlineActivationResponse RegisterKey(string pAddress, string pKey, string pAppID, int pAmount)
		{
			string requestUrl = pAddress;
			string parameters = "?";
			
			parameters += "key=" + pKey;
			parameters += "&app=" + pAppID;
			parameters += "&action=3"; // Fire activation
			parameters += "&amount=" + pAmount.ToString();			
			
			// Parameter prüfen
			// Mit PC-ID versehen
			// Request
			HttpWebRequest requ = (HttpWebRequest)HttpWebRequest.Create(requestUrl + parameters);
			HttpWebResponse resp = (HttpWebResponse)requ.GetResponse();
			
			StreamReader sr = new StreamReader(resp.GetResponseStream());
									
			string answer = sr.ReadLine();
			
			sr.Close();
			resp.Close();
				
				
			int intAnswer=0;
			
			try
			{
				intAnswer = Convert.ToInt32(answer);
				
			}
			catch
			{
							
				return(OnlineActivationResponse.InvalidResponseCode);
			}
			
			
			OnlineActivationResponse response = (OnlineActivationResponse)(intAnswer);
			return(response);
			// answer auswerten
			
			//return(OnlineActivationResponse.ActivationCancelled);
		}
	
		
		/// <summary>
		/// Delegat zum Ausgeben des Antwortcodes bei Telefonaktivierung. 
		/// </summary>
		public delegate void AnswerCodePrinter(string pAnswerCode);
		
		/// <summary>
		/// Routine zum Durchführen einer manuellen Konfiguration (Kunde gibt über Telefon Codes durch und erhält Antwortcode)
		/// </summary>
		/// <param name="pAddress">URL zum Aktivierungsskript</param>
		/// <param name="pKey">Key, für den aktiviert werden soll</param>
		/// <param name="pAppID">ApplikationsID für die aktiviert werden soll</param>
		/// <param name="pHWID">Hardwarekennung, für die aktiviert werden soll</param>
		/// <param name="pAnswerCodePrinter">Delgat (siehe <see cref="AnswerCodePrinter">AnswerCodePrinter</see></param>
		/// <returns>Aktivierungsstatus, Aktivierungscode über Delegate</returns>
	 
		public static OnlineActivationResponse TelephoneActivation(string pAddress, string pKey, string pAppID, string pHWID, AnswerCodePrinter pAnswerCodePrinter)
		{
			string requestUrl = pAddress;
			string parameters = "?";			
			
			parameters += "key=" + pKey;
			parameters += "&app=" + pAppID;
			parameters += "&machine=" + pHWID;
			parameters += "&action=4"; // Fire activation
			
		
			// Parameter prüfen
			// Mit PC-ID versehen
			// Request
			HttpWebRequest requ = (HttpWebRequest)HttpWebRequest.Create(requestUrl + parameters);
			HttpWebResponse resp = (HttpWebResponse)requ.GetResponse();
			
			StreamReader sr = new StreamReader(resp.GetResponseStream());
			
			string answer = sr.ReadLine();
			
			sr.Close();
			resp.Close();
			
			string[] parts;
			try
			{
				parts = answer.Split(';');
			}
			catch
			{
				
				TBL.Exceptions.ConversionException ex = new TBL.Exceptions.ConversionException("In Telefonaktivierungsantwort zurückgelieferter String lässt sich nicht nach Separator ';' zerteilen. Empfangener Wert: " + answer);
				throw(ex);
			}
			
			int intAnswer=0;
			
			try
			{
				intAnswer = Convert.ToInt32(parts[0]);
			}
			catch
			{
				return(OnlineActivationResponse.InvalidResponseCode);
			}
			
					
			OnlineActivationResponse response = (OnlineActivationResponse)(intAnswer);
			
			if(response == OnlineActivationResponse.Successful)
			{
				try
				{
					int activationCode = Convert.ToInt32(sr.ReadLine());
					pAnswerCodePrinter(parts[1].ToString());
				}
				catch
				{
					return(OnlineActivationResponse.InvalidResponseCode);
				}

			}
			//else
			return(response);
			
		}
	
		/// <summary>
		/// Prüft, ob ein Telefonaktivierungsantwortcode (siehe <see cref="TelephoneActivation">TelephoneActivation</see>) richtig oder falsch ist.
		/// </summary>
		/// <param name="pRequestcode">Requestcode, mit dem die Telefonaktivierung durchgeführt wurde</param>
		/// <param name="pAnswer">Antwortcode, den der Kunde eingegeben hat</param>
		/// <returns></returns>
		public static bool IsValidTelephoneCode(string pRequestcode, string pAnswer)
		{
			int sum = 0;		
			
			for(int i=0; i<pRequestcode.Length; i++)
			{
				sum += (int)pRequestcode[i];
				
			}
			
			sum = sum ^ 0x240290;
			
			// crypten..
			
			
			int answerCode = 0;
			try
			{
				answerCode = Convert.ToInt32(pAnswer);
			}
			catch
			{
				TBL.Exceptions.ConversionException ex = new TBL.Exceptions.ConversionException("AnswerCode hat ungültiges Format. Antwortcode: " + pAnswer);
				throw ex;
			}
			
			
			return(sum == answerCode);
		}
	}
	
	/// <summary>
	/// Implementiert Aktivierungsvorgänge und Prüfungen, ob eine Lizenzdatei vorliegt
	/// </summary>
	public static class Activation
	{
		private const string configFileType = "tblLicense";
		
		
		/// <summary>
		/// Führt eine Onlineaktivierung durch und meldet den Status zurück
		/// </summary>
		/// <param name="pAddress">URL zum Aktivierungsskript (activate.php) TODO: Verweis</param>
		/// <param name="pKey">Vom Benutzer eingegebener Key (25-stellig)</param>
		/// <param name="pAppID">ApplikationsID, hardcoded und muss in OnlineDatenbank eingetragen werden</param>
		/// <param name="pHWID">HardwareID</param>
		/// <returns><see cref="OnlineActivationResponse">OnlineActivationResponse-Wert</see> wird zurückgegeben. Bei Erfolg sollte OnlineActivationResponse.Successful zurückgegeben werden</returns>
		public static OnlineActivationResponse OnlineActivation(string pAddress, string pKey, string pAppID, string pHWID)
		{
			string requestUrl = pAddress;
			string parameters = "?";
			
			parameters += "key=" + pKey;
			parameters += "&app=" + pAppID;
			parameters += "&machine=" + pHWID;
			parameters += "&action=1"; // Fire activation
			
			#if DEBUG
				StreamWriter sw = new StreamWriter("OnlineActivationLastCall.txt");
				sw.Write(requestUrl + parameters);
				sw.Flush();
				sw.Close();
			#endif
			
			// Parameter prüfen
			// Mit PC-ID versehen
			// Request
			HttpWebRequest requ = (HttpWebRequest)HttpWebRequest.Create(requestUrl + parameters);
			HttpWebResponse resp = (HttpWebResponse)requ.GetResponse();
			
			StreamReader sr = new StreamReader(resp.GetResponseStream());
			
			string answer = sr.ReadLine();
			
			sr.Close();
			resp.Close();
			
			int intAnswer=0;
			
			try
			{
				intAnswer = Convert.ToInt32(answer);
			}
			catch
			{
				return(OnlineActivationResponse.InvalidResponseCode); // Tritt bei Syntaxfehlern auf...
			}
			
					
			OnlineActivationResponse response = (OnlineActivationResponse)(intAnswer);
			return(response);
			// answer auswerten
			
			//return(OnlineActivationResponse.ActivationCancelled);
		}		
	
				
		
		/// <summary>
		/// Prüft, ob die Lizenzdatei gültig und aktiviert ist
		/// </summary>
		/// <param name="pLicensePath">Pfad zur Lizenzdatei</param>
		/// <param name="pAppID">ApplikationsID (Valid, muss am Webserver vorhanden sein)</param>
		/// <returns>true ... aktiviert, false .. nicht aktiviert, falsche Lizenzdatei</returns>
		/// <exception cref="TBL.Exceptions.ConversionException">ConversionException (Zeilen und Spaltensplit), ';'-csv</exception>
		public static bool IsLicensed(string pLicensePath, string pAppID)
		{
			if(!File.Exists(pLicensePath)) // Wenns nit existiert kanns auch nicht aktiviert sein..
			{
				return(false);
			}
			
			string fileContent = TBL.Crypt.RatsEncryptionManager.DecryptFiletoString(pLicensePath);
			string[] lines;
			
			try
			{
				lines = fileContent.Split(Environment.NewLine[0]);
			}
			catch
			{
				TBL.Exceptions.ConversionException ex = new TBL.Exceptions.ConversionException("Dekryptographierter String aus Lizenzdatei kann nicht in mehrere Zeilen aufgelöst werden, hat somit ungültiges Format. Ausgabe des Strings aus Sicherheitsgründen unterbunden");
				throw(ex);
			}
			
			if(EDOLL.EDOLLHandler.NoError(TBL.Check.CheckCSVHeader(lines[0], configFileType)))
			{
				try
				{					
					string[] data = lines[1].Split(';');
					
					if(data[0].Trim() != Hardware.HardwareInfo.GetIdentificationString().Trim())
					
					{
						EDOLL.stdOut.Error("Die Lizenzdatei ist für eine andere Hardware bestimmt. Setzen Sie sich bitte mit der Herstellerfirma in Verbindung...");
						return(false);
					}
					
					if(data[2].Trim() == pAppID.Trim())
					{						
						return(true);
					}
					else
					{
						EDOLL.stdOut.Error("Die Lizenzdatei ist für eine andere Software bestimmt. Setzen Sie sich bitte mit der Herstellerfirma in Verbindung...");
						return(false);
					}
				}
				catch
				{
					TBL.Exceptions.ConversionException ex2 = new TBL.Exceptions.ConversionException("Dekryptographierter String (LizenzID + Key) aus Lizenzdatei kann nicht in mehrere Spalten aufgelöst werden, hat somit ungültiges Format. Ausgabe des Strings aus Sicherheitsgründen unterbunden");
					throw(ex2);
			
				}
			}
			else
			{
				EDOLL.stdOut.Error(EDOLL.EDOLLHandler.GetLastError(), "Beim Lesen der Lizenzdatei.");
			}
			
			return(false);
		}
		
		/// <summary>
		/// Erstellt eine Lizenzdatei
		/// </summary>
		/// <param name="pLicensePath">Pfad zur Lizenzdatei</param>
		/// <param name="pAppID">ApplikationsID (valid, muss am Webserver vorhanden sein)</param>
		/// <param name="pKey">Key, auf den lizenziert wurde</param>
		/// <returns>
		/// <para>
		/// 	<para>Interne <see cref="TBL.EDOLL.EDOLLHandler">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (Exception) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
		/// 	<para><c>EDOLLHandler.GetLastError();</c></para> 
		/// 	(siehe <see cref="TBL.EDOLL.EDOLLHandler.GetLastError">EDOLLHandler.getLastError()</see>)
		/// </para>
		/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
		/// <list type="table">
		/// 	<listheader><term><see cref="TBL.EDOLL.EDOLLHandler">EDOLL-Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Erfolgreich gespeichert</description></item>
		/// 	<item><term>-551</term><description>Datenzugriffsfehler</description></item>
		/// </list>
		/// </returns>
		public static int WriteLicenseFile(string pLicensePath, string pAppID, string pKey)
		{
			// configFiletype;LibraryVersion
			// HWID;Key
					
			
			string toSave = configFileType + ";" + Runtime.LibraryVersion + Environment.NewLine;
			toSave += TBL.Hardware.HardwareInfo.GetIdentificationString() + ";" + pKey + ";" + pAppID;
				
			try
			{
				if(!Directory.Exists(Path.GetDirectoryName(pLicensePath)))
			    {
					Directory.CreateDirectory(Path.GetDirectoryName(pLicensePath));
			    }
				
				TBL.Crypt.RatsEncryptionManager.EncryptStringtoFile(toSave, pLicensePath);
			}
			
			catch
			{
				return(-551); // Zugriffsfehler
			}
			
			return(0);
		}
		
		/// <summary>
		/// Überprüft, ob die Software lizenziert wurde oder nicht und ruft bei Bedarf das Lizenzierungsformular auf
		/// </summary>
		/// <param name="pLicensePath">Lizenzierungspfad</param>
		/// <param name="pAppID">ApplikationsID (valid, muss in Onlinedatenbank vorhanden sein)</param>
		///  <param name="pOnlineScript">URL zum Onlinescript TODO:Link</param>
		/// <param name="pContactInfo">Kontaktinformationen, z.B. Max Mustermann, Tel: 0043 123 4568</param>
		/// <returns></returns>
		/// <example>
		/// <para>Der Aufruf der Funktion sollte beim Programmstart erfolgen. Gibt die Funktion true, so soll das Programm ausgeführt werden, gibt es false, dann soll es abgebrochen werden</para>
		/// </example>
		public static bool CheckLicense(string pLicensePath, string pAppID, string pOnlineScript, string pContactInfo)
		{
			if(IsLicensed(pLicensePath, pAppID))
				return(true); // alles in bester ordnung
			
			LicenseForm gui = new LicenseForm(pLicensePath, pAppID, pOnlineScript, pContactInfo);
			
			// else
			if(DialogResult.OK == gui.ShowDialog())
			{
				// Aktivierung war erfolgreich
				return(true);
			}
			else
			{
				return(false);
			}
		}
		
		
	}

}
