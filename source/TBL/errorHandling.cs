/*
 * Created by SharpDevelop.
 * User: Tom
 * Date: 12.11.2010
 * Time: 12:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;

namespace TBL.EDOLL
{
	/// <summary>
	/// Der EDOLL-Handler ist die Schnittstelle zwischen Bibliothek und Software. Über ihn werden Fehlermeldungen aufgeschlüsselt und automatisch geloggt.
	/// </summary>
	/// <remarks>
	/// <h2>Liste der intern definierten Fehler (TBL + Molin Library)</h2>
	/// <list type="table"><listheader><term>Fehlernummer</term><description>Beschreibung</description></listheader>
	/// <item><term>11</term><description>Keine Verbindung zur Hardware. Vorgang konnte nicht ausgeführt werden</description></item>
	/// <item><term>12</term><description>Eingestellte IP-Adresse ungültig.</description></item>
	/// <item><term>-13</term><description>Verbindungsaufbau nicht möglich.</description></item>
	/// <item><term>14</term><description>Einige Daten konnten nicht gesendet werden.</description></item>
	/// <item><term>15</term><description>Es konnten keine Daten von einem der Geräte empfangen werden</description></item>
	/// <item><term>-16</term><description>Verbindungsaufbau zu Gerät nicht möglich</description></item>
	/// <item><term>-17</term><description>Verbindungsparameter wurden noch nicht oder falsch gesetzt</description></item>
	/// <item><term>-18</term><description>Es wurde noch kein Empfangspuffer festgelegt</description></item>
	/// <item><term>-19</term><description>Zu verarbeitendes M3S-Frame ist ungültig. (Länge, Aufbau oder Prüfsummenfehler)</description></item>
	/// <item><term>-20</term><description>Es kann kein M3S-Datenframe aus der Instanz erzeugt werden, da zu viele oder zu wenig Nutzdatenbytes enthalten sind</description></item>
	/// <item><term>-21</term><description>Acknowledgeframe von einem anderen Slave erhalten wie erwartet</description></item>
	/// <item><term>-22</term><description>Bei dem als Acknowledgeframe empfangenen Frame handelt es sich um kein Acknowledgeframe (falsche Protokollnummer)</description></item>
	/// <item><term>-23</term><description>Masteradresse oder Datenrichtung stimmt nicht</description></item>
	/// <item><term>201</term><description>Empfangspufferüberlauf.</description></item>
	/// <item><term>202</term><description>Keine oder falsche Antwort bei Versuch Pixel auszulesen bei Gerät</description></item>
	/// <item><term>-203</term><description>Keine Antwort beim Versuch Daten an Slave zu übertragen.</description></item>
	/// <item><term>204</term><description>Der Servicecontroller des Servers antwortet nicht oder funktioniert nicht richtig. Dienste konnten nicht ausgelesen werden.</description></item>
	/// <item><term>205</term><description>Es konnte kein Servicecontroller für eine oder mehrere Komponenten gefunden werden. </description></item>
	/// <item><term>206</term><description>Keine Antwort beim Versuch aktuelle Daten von Gerät (Sniffer) auszulesen</description></item>
	/// <item><term>207</term><description>An Empfangsbuffer übergebene Parameter falsch: pFrom > pTo</description></item>
	/// <item><term>208</term><description>Es wird versucht, über die Grenzen des Empfangspuffers hinaus Daten zu löschen</description></item>
	/// <item><term>520</term><description>Die Datei lights.conf konnte nicht gefunden werden.</description></item>
	/// <item><term>521</term><description>Fehler in Adressspalte in lights.conf</description></item>
	/// <item><term>522</term><description>Fehler in der Busspalte</description></item>
	/// <item><term>-523</term><description>Unültige Datensatzlänge / Syntax Error</description></item>
	/// <item><term>524</term><description>Punkte nicht konvertierbar</description></item>
	/// <item><term>525</term><description>Defaultpixel nicht richtig</description></item>
	/// <item><term>-526</term><description>Auf zu öffnende Konfigurationsdatei kann nicht zugegriffen Werden. Möglicherweise ist sie in einem anderen Programm geöffnet</description></item>
	/// <item><term>528</term><description>Fehlerhafter Datensatz, Effekt konnte nicht in Enumeration konvertiert werden</description></item>
	/// <item><term>529</term><description>Fehlerhafter Datensatz, Ungültige ID</description></item>
	/// <item><term>530</term><description>Fehler beim Einlesen der effects.conf - Bus mit dieser ID existiert nicht</description></item>
	/// <item><term>531</term><description>Fehlerhafter Datensatz, Zu wenig Farben übergeben</description></item>
	/// <item><term>-533</term><description>XML-Datei hat ungültiges Format: Nicht genügend Childnodes</description></item>
	/// <item><term>-534</term><description>ConfigFileType-Fehler: Die Datei hat einen falschen ConfigFileType</description></item>
	/// <item><term>-535</term><description>ConfigFileType-Fehler: Die Datei hat eine falsche ConfigFileVersion</description></item>
	/// <item><term>540</term><description>Es konnte keine Verbindung zur Kalenderkomponente aufgenommen werden.</description></item>
	/// <item><term>541</term><description>Falsches Format in sniffer.conf</description></item>
	/// <item><term>542</term><description>Keine Semikolons als Trennzeichen in der sniffer.conf vorhanden</description></item>
	/// <item><term>601</term><description>Gerät nicht bereit.</description></item>
	/// <item><term>-602</term><description>Ungültige Adresse eingegeben</description></item>
	/// <item><term>-603</term><description>Ungültige Farbe eingegeben.</description></item>
	/// <item><term>-604</term><description>Ungültige IP Adresse eingegeben. Vorgang konnte nicht ausgeführt werden.</description></item>
	/// <item><term>-605</term><description>Nicht definiertes Beleuchtungselement ausgewählt.</description></item>
	/// <item><term>606</term><description>Es wird versucht ein Bus anzusprechen, der nicht existiert. Möglicherweise liegt ein Fehler vor oder das Bussystem ist nicht versorgt.</description></item>
	/// <item><term>607</term><description>Es konnten keine Daten übertragen werden, der Steuerdienst wurde nicht gestartet oder funktioniert möglicherweise nicht richtig.</description></item>
	/// <item><term>608</term><description>Der angesprochene Dienst existiert nicht oder wurde noch nicht gestartet.</description></item>
	/// <item><term>609</term><description>EffectSet konnte nicht aus der XML-Datei gelesen werden. Möglicherweise ist das EffectSet mit dieser ID nicht vorhanden. Der Bus mit dieser ID wird im weiteren Programmablauf nicht mehr berücksichtigt.</description></item>
	/// <item><term>-610</term><description>Ungültiger Portname festgestellt</description></item>
	/// <item><term>611</term><description>Eine als Modellsymbol zu verwendende Grafikdatei existiert nicht.</description></item>
	/// <item><term>-612</term><description>Übergebenes byte[] hat eine ungültige Länge für Datensatzkonversion</description></item>
	/// </list>
	/// </remarks>
	public static class EDOLLHandler
	{
		internal static classErrorHandling errHan = new classErrorHandling();
		
		/// <summary>
		/// Bietet die Möglichkeit, die internen Fehler als XMLTable (für Dokumentationszwecke) in eine Datei auszugeben
		/// </summary>
		public static void CreateXmlTableErrorList()
		{
			SaveFileDialog savedia = new SaveFileDialog();
			
			if(savedia.ShowDialog() == DialogResult.OK)
			{
				if(errHan != null)
				{
					errHan.CreateXmlTalbeErrorList(savedia.FileName);
				}
				else
				{
					stdOut.Error("Errorhandler nicht verbunden, Datei kann nicht erstellt werden");
				}
				
			}
		}
		
		/// <summary>
		/// Start mit externer Fehlerkonfigurationsdatei und Logfile
		/// </summary>
		/// <param name="pConfFile">Pfad zum externen Fehlerkonfigurationsfile. Informationen: <a href="doc/errorConfFile.htm">siehe hier</a></param>
		/// <param name="pLogFile">Pfad, wo das Logfile abgelegt werden soll / wo bereits ein Logfile liegt</param>
		/// <returns></returns>
		public static bool Start(string pConfFile, string pLogFile)
		{
			errHan = new classErrorHandling(pConfFile, pLogFile);
			
			if(errHan != null)
				return(true);
			return(false);
		}
		
		/// <summary>
		/// Start mit externer Konfigurationsdatei, ohne Log
		/// </summary>
		/// <param name="pConfFile">Pfad zum externen Fehlerkonfigurationsfile. Informationen: <a href="doc/errorConfFile.htm">siehe hier</a></param>
		/// <returns>true / false ob Start gelungen</returns>
		public static bool Start(string pConfFile)
		{
			errHan = new classErrorHandling(pConfFile);
			
			if(errHan != null)
				return(true);
			return(false);
		}
				
		internal static string GetErrorDescription(int pErrNr)
		{		
			return(errHan.getErrorMsg(pErrNr,false));
		}
		
		internal static string GetErrorDescTech(int pErrNr)
		{
			return(errHan.getErrorMsg(pErrNr,true));
		}
		
		/// <summary>
		/// Liefert die verbale Fehlerbeschreibung einer EDOLL-Fehlernummer
		/// </summary>
		/// <param name="vErrCode">Fehlernummer</param>
		/// <returns>Verbale Fehlerbeschreibung (Fehlerbeschreibung + Aktionsvorschlag)</returns>
		public static string Verbalize(int vErrCode)
		{
			return(GetErrorDescription(vErrCode));
		}
		
		/// <summary>
		/// Liefert die verbale Fehlerbeschreibung einer EDOLL-Fehlernummer mit technischen Zusatzinformationen
		/// </summary>
		/// <param name="vErrCode">Fehlernummer</param>
		/// <returns>Verbale Fehlerbeschreibung (Fehlerbeschreibung + Aktionsvorschlag + mögliche Gründe + technische Fehlerbeschreibung)</returns>
		public static string VerbalizeTech(int vErrCode)
		{
			return(GetErrorDescTech(vErrCode));
		}
		
		
		
		
		/// <summary>
		/// Start mit nur intern definierten Fehlermeldungen, Errorlog abhängig vom Parameter
		/// </summary>
		/// <param name="pLog">Fehler in '$executableDir/errors.log' loggen oder nicht (true / false)</param>
		/// <returns>
		/// true / false ob Start gelungen
		/// </returns>
		public static bool Start(bool pLog)
		{
			if(pLog)
			{
				errHan = new classErrorHandling("internalOnly", "errors.log"); // starten mit dem Hiweis, dass nur interne Fehler verwendet werden..
			}
			else
			{
				errHan = new classErrorHandling();
			}
			
			if(errHan!=null)
				return(true);
			return(false);
		}
		
		/// <summary>
		/// EDOLL-Code zu bool-Converter. Wird ein Fehlercode übergeben, so wird der Fehler automatisch geloggt.
		/// </summary>
		/// <param name="pErrCode">Fehlercode / Rückgabewerte von Funktionen</param>
		/// <returns>
		/// <para>true wenn pErrCode ein Fehlercode ist</para>
		/// <para>false wenn kein Fehler vorliegt (pErrCode >=0)</para>
		/// </returns>
		public static bool NoError(int pErrCode)
		{
			if(errHan != null)
			{
				return(errHan.noError(pErrCode));
			}
			else
			{
				stdOut.Error("ErrorHandler nicht verbunden");
				return(true); // erscheint wie "kein fehler"
			}			
		}
		
		/// <summary>
		/// EDOLL-Code zu bool-Converter. Wird ein Fehlercode übergeben, so wird der Fehler automatisch geloggt.
		/// </summary>
		/// <param name="pErrCode"></param>
		/// <returns>
		/// <para>false wenn pErrCode ein Fehlercode ist</para>
		/// <para>true wenn kein Fehler vorliegt (pErrCode >=0)</para>
		/// </returns>
		public static bool Error(int pErrCode)
		{
			if(errHan != null)
			{
				if(errHan.noError(pErrCode))
					return(false);
				else
					return(true);
			}
			else
			{
				stdOut.Error("ErrorHandler nicht verbunden");
				return(false); // erscheint wie "kein fehler"
			}
			
		}
		
		/// <summary>
		/// Liefert den letzten über <see cref="EDOLLHandler.Error">Error()</see> oder <see cref="EDOLLHandler.NoError">NoError()</see> behandelten Fehler in aufgeschlüsselter Form (Nummer, Beschreibung, Try)
		/// </summary>
		/// <returns>
		/// Aufgeschlüsselte Fehlermeldung als String
		/// </returns>
		public static string GetLastError()
		{
			if(errHan != null)
			{
				return(errHan.getLastError());
			}
			else
			{
				return("EDOLLHandler nicht aktiv. Try: EDOLLHandler.Start()");
			}
		}
		
		/// <summary>
		/// Liefert den letzten über <see cref="EDOLLHandler.Error">Error()</see> oder <see cref="EDOLLHandler.NoError">NoError()</see> behandelten Fehler in aufgeschlüsselter technischer Form (alle Parameter)
		/// </summary>
		/// <returns>
		/// Aufgeschlüsselte Fehlermeldung mit allen Details als String
		/// </returns>
		public static string GetLastErrorTech()
		{
			if(errHan != null)
			{
				return(errHan.getLastErrorTech());
			}
			else
			{
				return("EDOLLHandler nicht aktiv. Try: EDOLLHandler.Start()");
			}
		}
		
		/// <summary>
		/// Liefert den letzten über <see cref="EDOLLHandler.Error">Error()</see> oder <see cref="EDOLLHandler.NoError">NoError()</see> behandelten Fehlercode.
		/// </summary>
		public static int LastErrorCode
		{
			get
			{
				return(errHan.LastErrorCode);
			}
		}
		
	}
	
	internal class errorBundle
	{
		public static readonly int cntMember = 6; // Parameter wie viele Members in der Region "Member" sind
		
		#region Member
		private string desc;
		private string techDesc;
		private string sugg;
		private string reason;
		private int codenr;
		private bool logg;
		#endregion
		
		#region Properties
		public int Code
		{
			get
			{
				return(this.codenr);
			}
			set
			{
				this.codenr = value;
			}
		}
		
		
		public string Description
		{
			get
			{
				return(this.desc);
			}
			set
			{
				this.desc = value;
			}
		}
		
		
		public string techDescription
		{
			get
			{
				return(this.techDesc);
			}
			set
			{
				this.techDesc = value;
			}
		}
		
		
		public string possibleReason
		{
			get
			{
				return(this.reason);
			}
			set
			{
				this.reason = value;
			}
		}
		
		public string Suggestion
		{
			get
			{
				return(this.sugg);
			}
			set
			{
				this.sugg = value;
			}
		}
		
		public bool Logg
		{
			get
			{
				return(this.logg);
			}
			set
			{
				this.logg = value;
			}
		}
		
		#endregion
		
		#region Constructors
		public errorBundle()
		{
			// do nothing
		}
		
		public errorBundle(errorBundle pOld)
		{
			codenr = pOld.Code;
			desc = pOld.Description;
			techDesc = pOld.techDescription;
			reason = pOld.possibleReason;
			sugg = pOld.Suggestion;
			logg = pOld.Logg;
		} // Copykonstruktor
		
		public errorBundle(int pCode, string pDesc, string pTechDesc, string pPossReason, string pSugg, bool pLogg)
		{
			// assign to private values
			codenr = pCode;
			desc = pDesc;
			techDesc = pTechDesc;
			reason = pPossReason;
			sugg = pSugg;
			logg = pLogg;
		}
		#endregion
		
		#region parsing
		public override string ToString()
		{
			return("Error #" + codenr.ToString() + ": " + this.desc + System.Environment.NewLine + "Try: " + this.sugg);
		}
		
		public string getErrorForLog()
		{
			return(DateTime.Now.ToString("yyyy/MM/dd") + ";" + DateTime.Now.ToString("hh:mm:ss") + ";" + codenr.ToString() + ";" + desc  + ";" +  techDesc + ";" + reason + ";" + sugg);
		}
		#endregion
		
	}
	
	
	
	internal class classErrorHandling
	{
		
		public void CreateXmlTalbeErrorList(string pFilename)
		{
			StreamWriter sw = new StreamWriter(pFilename);
			
			sw.WriteLine("<list type=\"table\"><listheader><term>Fehlernummer</term><description>Beschreibung</description></listheader>");
			
			foreach(errorBundle e in errors)
			{
				sw.WriteLine("<item><term>" + (e.Code).ToString() + "</term><description>" + e.Description + "</description></item>");
				sw.Flush();
			}
			
			sw.WriteLine("</list>");
			sw.Close();
		}
		
		private bool makePredefinedErrors()
		{
			// JEWEILS NEGATIVE WERTE
			// -1... 100
			#region Verbindungsfehler und Protokollfehler
			errors.Add(new errorBundle(-1, "FATAL-Error: Programm muss beendet werden", "Siehe gegebene Zusatzinfo", "Diverse gründe, wird allgemein verwendet", "Starten Sie das Programm erneut. Bei mehrmaligem Auftreten kontaktieren Sie bitte den Hersteller", false));
			errors.Add(new errorBundle(11,"Keine bestehende Verbindung zur Hardware. Vorgang kann nicht ausgeführt werden","Es ist keine Verbindung zum Interface vorhanden.",",ggf. noch keine Verbindung aufgebaut, fehlerhafte Parameter","Überprüfen Sie die Verbindungsparameter und versichern Sie sich dass eine Verbindung besteht",true));
			errors.Add(new errorBundle(12,"Eingestellte IP-Adresse ungültig.","IP-Adresse hat ungültiges Format.",",Fehlerhafte Benutzereingabe..","Überprüfen Sie die Verbindungseinstellungen",false));
			errors.Add(new errorBundle(-13,"Verbindungsaufbau trotz richtiger Verbindungsparameter nicht möglich.","Verbindung konnte trotz technisch korrekter Parameter nicht aufgebaut werden.",",Verkabelungsfehler, Interface nicht versorgt, PC im falschen Adressraum.","Überprüfen Sie ob Ihr Computer und die Hardware richtig angeschlossen sind oder eventuell schon eine Verbindung (auch von anderen Teilnehmern) zum Gerät besteht. Gehen Sie hierfür lt. Bedienungsanleitung vor.",true));
			errors.Add(new errorBundle(-14,"Einige Daten konnten nicht gesendet werden.","writeTimeout Netstream",",vl. keine Netzwerkmäßig Verbindung","Überprüfen Sie die Verbindungsparameter und stellen Sie sicher dass eine Verbindung besteht",true));
			errors.Add(new errorBundle(15,"Es konnten keine Daten von einem der Geräte empfangen werden. Readtimeout","readTimeout Netstream",",ggf. keine Spannungsversorgung, Bus unterbrochen","Überprüfen Sie die Verbindungsparameter und stellen Sie sicher dass eine Verbindung besteht. Stellen Sie außerdem sicher, dass die Leuchtmittel eingeschaltet sind.",true));
			errors.Add(new errorBundle(-16,"Verbindungsaufbau zu Gerät nicht möglich","Ping-Timeout zu Gerät",", Gerät nicht angesteckt oder IP-Adresse ungültig"," Prüfen Sie ob das Gerät mit dem Netzwerk verbunden und eingeschaltet ist. Resetten Sie ggf. das Gerät",true));
			errors.Add(new errorBundle(-17,"Verbindungsparameter wurden noch nicht oder falsch gesetzt","möglicherweise schreibfehler","bla","Prüfen Sie die eingestellten Verbindungsparameter",true));
			errors.Add(new errorBundle(-18,"Es wurde noch kein Empfangspuffer festgelegt"," devComError? noch nix übergeben",", receiveBuffer freed","Kontaktieren Sie bitte den Hersteller",true));
			errors.Add(new errorBundle(-19,"M3S: Zu verarbeitendes M3S-Frame ist ungültig. (Länge, Aufbau oder Prüfsummenfehler)","Ungültige Länge, Format oder Prüfsummenfehler",",Falsch empfangen oder Programmierfehler","Übertragen Sie die Daten ggf. erneut.",false));
			errors.Add(new errorBundle(-20,"M3S: Es kann kein M3S-Datenframe aus der Instanz erzeugt werden, da zu viele oder zu wenig Nutzdatenbytes enthalten sind","","","Prüfen Sie ihre Einstellungen und Programmablauf", true));
			errors.Add(new errorBundle(-21,"M3S: Acknowledgeframe von einem anderen Slave erhalten als erwartet", "Adressbyte falsch?", "Kippende Bits, wirklich falscher Slave gesendet, ...","Starten Sie die Datenübertragung ggf. erneut", false));
			errors.Add(new errorBundle(-22, "M3S: Bei dem als Acknowledgeframe empfangenen Frame handelt es sich um kein Acknowledgeframe (falsche Protokollnummer)", "Protokollnummer nicht die des Acknowledgeframes", "Ggf. Timing oder Programmierungsproblem, kippendes bit?", "Starten Sie die Datenübertragung ggf. erneut", false));
			errors.Add(new errorBundle(-23, "M3S: Masteradresse oder Datenrichtung stimmt nicht", "Falsches frame?", "falsches frame?", "Starten Sie die Datenübertragung ggf. erneut", false));
			errors.Add(new errorBundle(-24, "I2P: Empfangenes Frame ist nicht vom erwarteten Sender!", "Erstes Byte & ~0x80 stimmt nicht mit der mit dem Requestframe abgesetzten Adresse überein", "many", "Versuchen Sie die Daten erneut auszulesen", false));
			errors.Add(new errorBundle(-25, "UsbToSerial - Plug'n'Play: Kein geeignetes USB-Gerät gefunden oder USB-Gerät wird bereits von einem anderen Programm verwendet", "ComPort nicht öffenbar, existiert nicht oder M3S-Pixelreadout liefert kein- oder ungültiges Ergebnis", "many", "Stellen Sie sicher dass das entsprechende USB-Gerät angesteckt ist und von keinem anderen Programm verwendet wird.", false));
			errors.Add(new errorBundle(-26, "UsbToSerial - Plug'n'Play: Keine geeigneten USB-Geräte gefunden", "Es existieren keine ComPorts","nix angesteckt", "Stellen Sie sicher dass das entsprechende USB-Gerät angesteckt ist.", false));
			errors.Add(new errorBundle(-27, "M3S: Prüfsummenfehler im Frame", "Errechnete Prüfsumme stimmt nicht mit empfangener Prüfsumme überein", "diverse", "-", false));
			errors.Add(new errorBundle(-28, "M3S: upperBound im Frame stimmt nicht mit Framelänge überein!", "", "diverse", "-", false));
			errors.Add(new errorBundle(-29, "M3S: Protokollnummer ungültig oder kann nicht aus dem Frame extrahiert werden", "Entweder falsches Frame verwendet oder (wahrscheinlicher) falsche Protokollnummer", "..", "-", false));
			errors.Add(new errorBundle(-30, "M3S: Ungültige Slaveadresse; Broadcastframes erfordern Slaveadresse 0", "Slaveadresse hat nicht den Wert 0", "-", "-", false));
			errors.Add(new errorBundle(-31, "M3S: Frame mit Länge eines Acknowledgeframes verwendet nicht das Acknowledge Protokoll", "CtrlByte != 0x8*", "Protokollnummer ist nicht Acknowledgeprotokoll", "-", false));
			errors.Add(new errorBundle(-32, "M3S: (Analyze): Framelänge ist kürzer als Mindestframelänge(5)", "..", "...", "Dieser Fehler sollte nur zu Analysezwecken auftreten", false));
			errors.Add(new errorBundle(-33, "Spezifizierter COM-Port existiert nicht!","..","Gerät evtl. nicht angeschlossen oder Parameterfehler","Prüfen Sie die Verfügbarkeit des spezifizierten Ports im Gerätemanager",false));
			errors.Add(new errorBundle(-34, "DevCom nicht verfügbar", "Es besteht entweder keine Verbindung oder es wurde noch kein Resetframe gesendet", "Verbindungsaufbau fehlt, Resetframe nicht geschickt", "...", true));
			errors.Add(new errorBundle(-35, "Frame ist ungültig. Prüfsummenfehler", "CRC detektiert Fehler", "Störungen, diverses", "Starten Sie den Datentransfer erneut", false));
			errors.Add(new errorBundle(-36, "Not Acknowledge wurde signalisiert.", "ACK Flag im Controlbyte nicht gesetzt", "Slave sendet explizites NAK", "Starten Sie den Datentransfer erneut", false));
			errors.Add(new errorBundle(-37, "Implizites Not Acknowledge wurde signalisiert.", "ACK Flag im Controlbyte mit anderer Protokollnummer als ACK-Protokoll nicht gesetzt", "Slave sendet explizites NAK", "Starten Sie den Datentransfer erneut", false));
			
			#endregion
			
			// 200+
			#region Kommunikationsprobleme Transportlayer
			errors.Add(new errorBundle(-201,"Empfangspufferüberlauf.","readTimeout Netstream",",Softwarefehler, viel Empfangen aber nix auslesen.","Starten Sie die Software neu. Bei mehrmaligem Auftreten kontaktieren Sie bitte GFI.",true));
			errors.Add(new errorBundle(-202,"Keine oder falsche Antwort bei Versuch DataUnit-Anzahl auszulesen bei Gerät","readTimeout Protokoll, softwarewatchdog",",gerät hängt nicht im Bus, ist nicht versorgt, Max defekt","Überprüfen Sie, ob das entsprechende Gerät eingeschaltet ist",true));
			errors.Add(new errorBundle(-203,"Keine Antwort beim Versuch Daten an Slave zu übertragen. Acknowledge-Error.","writeFrame() in devCom meldet kein Acknowledge erhalten (nach xmaligem Versuch)",",Gerät nicht versorgt, Gerät nicht angeschlossen","Überprüfen Sie ob das Gerät mit entsprechender Adresse eingeschaltet ist",false));
			errors.Add(new errorBundle(204,"Der Servicecontroller des Servers antwortet nicht oder funktioniert nicht richtig. Dienste konnten nicht ausgelesen werden.","servicearray konnte nicht gefüllt werden",",vl. Keine Berechtigungen, fehlende Verbindung, falsche Einstellungen","Überprüfen Sie die Einstellungen (Optionen/Einstellungen) und ob der eingestellte Benutzer über ausreichend Berechtigungen am _Host (Server) verfügt",true));
			errors.Add(new errorBundle(205,"Es konnte kein Servicecontroller für eine oder mehrere Komponenten gefunden werden. ","in der ausgelesenen serviceliste des Servers gibt es keine Controller mit entsprechenden Dienstnahmen",",Ggf. nicht am Server installiert, keine Verbindung","Überprüfen Sie, ob die entsprechenden Komponenten am Server installiert sind, der eingestellte Account genügend Berechtigungen hat und eine Netzwerkverbindung besteht.",true));
			errors.Add(new errorBundle(-206,"Keine Antwort beim Versuch Farbe und Helligkeit von Gerät auszulesen","",",","",true));
			errors.Add(new errorBundle(-207,"An Empfangsbuffer übergebene Parameter falsch: pFrom > pTo","bla",",Programmierfehler oder falscher Wert in Variablen","Wenden Sie sich an den Softwarehersteller",true));
			errors.Add(new errorBundle(-208,"Es wird versucht, über die Grenzen des Empfangspuffers hinaus Daten zu löschen","pTo gressa  als dptr",",check your source","Kontaktieren Sie bitte den Softwarehersteller",true));
			errors.Add(new errorBundle(-209,"Es kann beim Dateitransfer keine Verbindung zum Empfänger aufgebaut werden", "Slave bestätigt Ankündigungsframe Dateitransfer nicht mit Acknowledge bzw. sendet keine entsprechende Antwort", "Keine Verbindung, Programmierfehler, ..", "Stellen Sie sicher, dass der betreffende Empfänger mit dem Netzwerk verbunden ist", false));
			errors.Add(new errorBundle(-210,"Beim Datentransfer wurde ein Paketfehler festgestellt; Die Reihenfolge der empfangenen Pakete stimmt nicht überein. Dateitransfer wurde abgebrochen", "Paket mit Paketnummer != lastPackageNumber+1 wurde empfangen", "Programmierfehler im Slave oder Datenübertragungsfehler über Hardwarelayer?", "Starten Sie den Dateitransfer erneut. Bei anhaltenden Problemen prüfen Sie bitte die Verbindung (z.B. Ping, ..)", true));
			errors.Add(new errorBundle(-211, "Die Netzwerkkomponente ist nicht gestartet oder konnte nicht gestartet werden", "Adminrechte?, einfach nicht gestartet..", "Berechtigungsfehler, nicht gestartet", "Stellen Sie sicher, dass alle Komponenten gestartet sind", false));
			errors.Add(new errorBundle(-212, "Die Kalenderkomponente ist nicht gestartet oder konnte nicht gestartet werden", "Adminrechte?, einfach nicht gestartet..", "Berechtigungsfehler, nicht gestartet", "Stellen Sie sicher, dass alle Komponenten gestartet sind", false));
			errors.Add(new errorBundle(-213, "Acknowledge Timeout", "Kein Acknowledge vom gegenüber erhalten..", "", "Starten Sie den Datentransfer erneut", true));
			errors.Add(new errorBundle(-214, "Keine Antwort bezüglich Ausführungsstatus des soeben gesendeten Befehls vom Kommunikationspartner (Readtimeout)", "readTimeout beim Warten auf Antwort (devCom.SendCommandReadAnswer) reattimeout", "", "Versuchen Sie, erneut Daten zu übertragen.", true));
			errors.Add(new errorBundle(-215, "Readtimeout bei einem Datenübertragungspaket. Paket ist nicht rechtzeitig eingetroffen", "evtl. hat sich Komm.Partner aufgehängt oder ist dort ein Programmierfehler", "s.v.", "Starten Sie die Übertragung erneut", true));
			errors.Add(new errorBundle(-216, "Ausgelesene DataUnit-Anzahl ist 0", "", "selbstsprechend", "", true));
			errors.Add(new errorBundle(-217, "Request konnte erfolgreich abgesetzt werden, allerdings konnte keine gültige Antwort empfangen werden (Readtimeout)", "Befehl acknowledged, Readtimeout beim Warten auf Antwort", "Slaveseitig nicht implementiert, Kommando erfordert gar keine Antwort, ...", "Kontaktieren Sie den Hersteller", true));
			errors.Add(new errorBundle(-218, "Antwort auf Ping inkorrekt.", "Gepingter Partner soll mit gleicher Payload antworten (Reflection), Payload war nicht gleich, es wurde aber ein Frame empfangen", "Implementierungsfehler im Gegenüber, Falscher Kommunikationspartner antwortete, ..", "Versuchen Sie erneut zu Pingen. Sollten Sie diesen Fehler öfters angezeigt bekommen, kontaktieren Sie den Hersteller.", false));
			errors.Add(new errorBundle(-219, "VirtualTWIMaster: Response has invalid format or is sent from wrong TWI-Slave", "Response has to have Format (addr/inv.addr) and optional 2 Byte error code (in case of inv.addr)", "Faulty implementation or bus collision", "Check your System", false));
			#endregion
			
			// 500+
			#region Softwarefehler
			errors.Add(new errorBundle(521,"Fehler in Adressspalte in lights.conf","Fehler in Adresszeile",",doppelte Adresse oder keine Zahl","Prüfen Sie ob die entsprechende Adresse ggf. doppelt vorhanden ist oder es sich um keine Zahl handelt",true));
			errors.Add(new errorBundle(522,"Fehler in der Busspalte","Fehler in Busspalte",",falsche Busnummer, keine Zahl","Überprüfen Sie, ob die Busnummer in der entsprechenden Zeile gültig und eine Zahl ist.",true));
			errors.Add(new errorBundle(-523,"Unültige Datensatzlänge / Syntax Error","nicht genug spalten, falsche Trennzeichen",",nicht genug Spalten, Trennzeichen beachten","Überprüfen Sie in einem Tabellenverarbeitungsprogramm (Excel) die Vollständigkeit und Richtigkeit der Spalten in der entsprechenden Zeile.",true));
			errors.Add(new errorBundle(524,"Punkte nicht konvertierbar","Koordinaten nicht erstellbar",",keine Zahl, wenigr spalten,..","Überprüfen Sie die Koordinatenspalten in der entsprechenden Zeile (ob numerisch und vollständig)",true));
			errors.Add(new errorBundle(525,"Defaultpixel nicht richtig","Defaultpixel nicht richtig",",keine Zahl, Null","Überprüfen sie ob die Spalte defaultPixel in der entsprechenden Zeile korrekt und größer 0 ist.",true));
			errors.Add(new errorBundle(-526,"Auf zu öffnende Konfigurationsdatei kann nicht zugegriffen werden. Möglicherweise ist sie in einem anderen Programm geöffnet","Nicht existierend oder wo anders geöffnet",",s.v.","Prüfen Sie die Existenz der Datei und schließen Sie ggf. andere Anwendungen",true));
			errors.Add(new errorBundle(-527,"Zu übertragende Datei kann nicht geöffnet und/oder gelesen werden.", "In einem anderen Programm geöffnet / nicht vorhanden, ..", "In einem anderen Programm geöffnet / nicht vorhanden, RAM nicht ausreichend.", "Prüfen Sie ob die Datei evtl. nicht vorhanden oder anderwertig geöffnet ist.", false));
			errors.Add(new errorBundle(-528,"Fehlerhafter Datensatz, Effekt konnte nicht in Enumeration konvertiert werden","Der String kann nicht in die Enum Effekte konvertiert werden. Entweder in Enum nicht vorhanden od. falsch geschrieben",",Enumeration nicht vollständig oder in der conf-Datei falsch geschrieben","Übermitteln Sie den Konfigurationsordner an GFI zum check.",true));
			errors.Add(new errorBundle(-529,"Fehlerhafter Datensatz, Syntaktisch ungültige ID","Der String kann nicht in einen Integer gewandelt werden",",Vmtl. Tippfehler","Übermitteln Sie den Konfigurationsordner an GFI zum check.",true));
			errors.Add(new errorBundle(-530,"Fehler beim Einlesen der effects.conf - Bus mit dieser ID existiert nicht","outside of bounds bus[]",",Vgl. bus.conf und effects.conf","Übermitteln Sie den Konfigurationsordner an GFI zum check.",true));
			errors.Add(new errorBundle(-531,"Fehlerhafter Datensatz, Zu wenig Farben übergeben","Es wird versucht auf rootColors[] zuzugreifen, outsideofbounds",",Nicht genug Farben für den Effekt übergeben","Übermitteln Sie den Konfigurationsordner an GFI zum check.",true));
			errors.Add(new errorBundle(-532, "Datensatz kann nicht in die DayOfWeek-Enumeration konvertiert werden. Datensatz wird NICHT weiterverarbeitet","Parsen von String auf DayOfWeek ist schiefgegangen", "Syntaxfehler", "Erstellen Sie die Datei neu oder beheben Sie den Fehler", true));
			errors.Add(new errorBundle(-533, "Die LightEffectConfiguration-Datei hat ein ungültiges Format. Es sind zu wenig Effekte für die Leuchtmittelgruppen vorhanden.", "Filestream nicht lesbar, oder FileInfo.Length != tatsächliche Länge", "?! ka", "Prüfen Sie ob Ihre Datei fehlerfrei ist.", false));
			errors.Add(new errorBundle(-534,"ConfigFileType-Fehler: Die Datei hat einen falschen ConfigFileType","",",","Prüfen Sie die die Konfigurationsdatei",true));
			errors.Add(new errorBundle(-535,"ConfigFileType-Fehler: Die Datei hat eine falsche ConfigFileVersion","",",","Prüfen Sie die Konfigurationsdatei",true));
			errors.Add(new errorBundle(540,"Es konnte keine Verbindung zur Kalenderkomponente aufgenommen werden.","svcCtrl oder calCtrler konnten nicht gefunden werden. ",",Keine Netzwerkverbindung, zu wenig Rechte am Server","Überprüfen Sie die entsprechenden Einstellungen und die Netzwerkverbindung zum Server.",true));
			errors.Add(new errorBundle(-541,"Falsches Format in sniffer.conf","Beinhaltet ungültige Zeichen oder wurde nicht vollständig ausgefüllt",",hat vmtl. keine drei spalten oder beinhaltet Strichpunkte","Prüfen Sie die Date",true));
			errors.Add(new errorBundle(-542,"Keine Semikolons als Trennzeichen in der sniffer.conf vorhanden","",",","Prüfen Sie die Datei",true));
			errors.Add(new errorBundle(-550, "Pixelzahl ist zu niedrig, muss >= 1 sein", "s.v.", "s.v.", "Lesen Sie den Pixelwert erneut aus oder starten Sie die Software neu", true));
			errors.Add(new errorBundle(-551, "Zugriffsfehler: Kein Zugriff möglich, da Sie keine ausreichenden Berechtigungen besitzen oder ein Ordner/Datei nicht existiert", "Betriebssystem blockiert...", "Keine Adminrechte", "Beenden Sie die Software und starten Sie sie mit ausreichend Berechtigungen (z.B. Administratorrechte) erneut", false));
			errors.Add(new errorBundle(-552, "Das Grafikverzeichnis '" + Runtime.ExecutableDirectory + "img' ist beschädigt. Einige Grafiken können unter Umständen falsch oder gar nicht angezeigt werden", "s.v.", "fehlende DAteien", "Installieren Sie die Anwendung ggf. erneut", true));
			errors.Add(new errorBundle(-553, "Der DNS-Server kann keine Auflösung des Hostnamen durchführen, eventuell ist ein Tippfehler unterlaufen?", " ", " ", "Prüfen Sie Ihre Einstellungen", true));
			errors.Add(new errorBundle(-554, "Eine temporäre Arbeitsdatei kann nicht angelegt werden. Stellen Sie sicher, dass das Verzeichnis existiert und Sie ausreichend Rechte haben", "Kann auftreten beim Schreiben in geschützte Ordner, fremde Ordner usw. wenn man keine Adminrechte hat", "Windowsspezifisch, vor allem ab Windows Vista. Man kann im Programmordner keine temporären Dateien anlegen!!", "Versuchen Sie als Zielort der Operation eine andere Stelle des Betriebssystems. Tipp: Verwenden Sie absolute Pfade", true));
			errors.Add(new errorBundle(-555, "Ein unbehandelter / nicht identifizierter Fehler ist aufgetreten", "Dieser Fehler wird innerhalb von Methoden oft als Defaultwert verwendet und könnte deshalb auf Programmierfehler / unbehandelte Ereignisse innerhalb der TBL.dll hinweisen", "Unbehandelte Ereignisse, nicht getestete Programmpfade, Programmierfehler, gewünschte nicht sinnvolle Fehlerrückgabe?!", "UNBEDINGT Programmierer kontaktieren!", true));
			errors.Add(new errorBundle(-560, "Die Effektanzahl des EffectSets stimmt nicht mit der Elementzahl des interagierenden devCom-Arrays überein.", "Programmierer hat vmtl. was falsch gemacht, Configfiles falsch..", "alte Configfiles, Falsche Configfiles?", "Prüfen Sie, ob es sich bei der zu verarbeitenden Effektkonfigurationsdatei um eine passende Datei handelt", false));
			errors.Add(new errorBundle(-561, "In der Effektkonfigurationsdatei konnte kein DefaultEffectSet gefunden werden", " " , " ", "Verwenden Sie eine andere Konfigurationsdatei", true));
			errors.Add(new errorBundle(-562, "In der matrixPCB-Konfigurationsdatei befindet sich ein Fehlerhafter Datensatz. 'single' als Datensatztyp erwartet", "Dritte Spalte muss 'single' enthalten", "", "Prüfen Sie die Konfigurationsdatei", false));
			errors.Add(new errorBundle(-563, "In der matrixPCB-Konfigurationsdatei befindet sich ein Fehlerhafter Datensatz. 'rect' als Datensatztyp erwartet", "Dritte Spalte muss 'single' enthalten", "", "Prüfen Sie die Konfigurationsdatei", false));
			errors.Add(new errorBundle(-564, "Datensatzkonversation nicht durchführbar: Intel Hexfiles erfordern einen ':' als erstes Zeichen einer Zeile", "Erstes zeichen des zu parsenden String ist kein :", "Invalides Hexfile? Invalider Datensatz?", "Prüfen Sie die Eingaben", false));
			errors.Add(new errorBundle(-565, "Datensatzkonversation nicht durchführbar: Databyte Count Wert ist keine Hexzahl", "", "Invalider Datensatz", "Prüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-566, "Datensatzkonversation nicht durchführbar: Adresse keine gültige Hexzahl", "2 Byte Zahl keine Hexzahl", "Invalider Datensatz", "Prüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-567, "Datensatzkonversation nicht durchführbar: Datensatztyp ungültig", "Darf nur zwischen 0 und 5 liegen", "Invalider Datensatz", "Prüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-568, "Datensatzkonversation nicht durchführbar: Ungültige Datensatzlänge, stimmt nicht mit DatabyteCnt überein", "Syntaxerror Intel-Hexfile Format", "Invalider Datensatz", "Prüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-569, "Datensatzkonversation nicht durchführbar: Datenbyte konnte nicht konvertiert werden", "Kein gültiger Hexstring", "Kein gültiger Hexstring, invalider Datensatz", "Prüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-570, "Datensatzkonversation nicht durchführbar: Prüfsummenfehler!", "Angegebene Prüfsumme stimmt nicht mit der berechneten Prüfsumme überein", "Invalider Datensatz", "Prüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-571, "Datenbytes konnten der CodePage nicht hinzugefügt werden: Eines oder mehrere der Datenbytes liegen außerhalb des Adressraums dieser Page", "Bytes sind vor Pageadresse angesiedelt oder eines oder mehrere Bytes ragen über die Page hinten hinaus", "Falscher Datensatz", "Überprüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-572, "No IP Addresses Found in Network via ICMP-Pings", "ICMP Pings to all Host-Addresses in Network failed", "Either Ping deactivated (Windows Firewall...) or no Hosts in Network", "..", false));
			errors.Add(new errorBundle(-573, "Could not establish Connection to Database with given parameters", "Either wrong parameters, wrong privileges for user or Database does not exist", "There are serveral..", "Check Configuration and System", true));
			errors.Add(new errorBundle(-574, "SQL-Command execution failed", "Refer details via Debug-Mode", "Serveral...", "Turn on Debugmode and determine where the failure is..", false));
			#endregion
			
			// 600+
			#region Parameterfehler, Syntaktische Sünden
			errors.Add(new errorBundle(601,"Gerät nicht bereit.","Geräteparameter in Softwareinstanz möglicherweise nicht gesetzt.",",Noch keine Einstellungen getätigt.","Überprüfen Sie die Einstellungen der einzelnen Leuchtmittel.",true));
			errors.Add(new errorBundle(-602,"Ungültige M3S-Adresse eingegeben","Adresse befindet sich nicht im vorgegebenen Bereich (zwischen 1 und 255)",",Ungültige Adresse eingegeben.","Überprüfen Sie die Adresseingabe.",true));
			errors.Add(new errorBundle(-603,"Ungültige Farbe eingegeben.","Es handelt sich bei der Eingabe um keinen hexadecmimalen Farbwert.",",Eventuelle Tippfehler korrigieren.","Überprüfen Sie die Farbeingabe.",false));
			errors.Add(new errorBundle(-604,"Ungültige IP Adresse eingegeben. Vorgang konnte nicht ausgeführt werden.","Eingegebene IP weist falsches Format auf.",",Fehlerhafte Eingabe der IP.","Überprüfen Sie die Eingabe.",true));
			errors.Add(new errorBundle(-605,"Nicht definiertes Beleuchtungselement ausgewählt.","In der *.csv Datei befindet sich ein nicht definiertes Beleuchtungselement.",",none","Überprüfen Sie die Eingabe.",true));
			errors.Add(new errorBundle(606,"Es wird versucht ein Bus anzusprechen, der nicht existiert. Möglicherweise liegt ein Fehler vor oder das Bussystem ist nicht versorgt.","Au sder Funktion setEffect bei Fixfarben",",falsche Konfdateien","Kontaktieren Sie bitte GFI",true));
			errors.Add(new errorBundle(-607,"Es konnten keine Daten übertragen werden, der Steuerdienst (Netzwerkkomponente) wurde nicht gestartet oder funktioniert möglicherweise nicht richtig.","Service not running",",Prozess nicht gestartet","Starten Sie Ihren Computer/Server nach Möglichkeit neu, oder gehen Sie lt. Bedienungsanleitung vor.",true));
			errors.Add(new errorBundle(-608,"Der angesprochene Dienst existiert nicht oder wurde noch nicht gestartet.","Service not existing, evtl. nicht installiert",",ggf. nicht installiert","Gehen Sie lt. Handbuch 'Dienstinstallation' vor",true));
			errors.Add(new errorBundle(-609,"EffectSet konnte nicht aus der XML-Datei gelesen werden. Möglicherweise ist das EffectSet mit dieser ID nicht vorhanden. Der Bus mit dieser ID wird im weiteren Programmablauf nicht mehr berücksichtigt.","XML-Datei beinhaltet whs. den entsprechenden Eintrag nicht",",Datensatz nicht enthalten","Erstellen Sie mit der MolinNetControl ein neues EffectSet",true));
			errors.Add(new errorBundle(-610,"Ungültiger Portname festgestellt","Das Wort COM kommt nicht vor oder nachfolgend ist kein Int.",",Falsche Syntax","Prüfen Sie die Einstellungen und die Existenz der eingestellten Ports im Gerätemanager. COM-Ports sind beispielsweise so einzugeben: 'COM1'",true));
			errors.Add(new errorBundle(-611,"Eine als Modellsymbol zu verwendende Grafikdatei existiert nicht.","Bitmap vmtl. nicht da..",",Grafikdatei nicht oder an falschem Ort vorhanden","Überprüfen Sie ob sich die in der folgenden Zeile angeführte Grafikdatei im entsprechenden Verzeichnis (ausgehend vom Installationsverzeichnis) befindet",true));
			errors.Add(new errorBundle(-612,"Übergebenes byte[] hat eine ungültige Länge für Datensatzkonversion / Verarbeitung", "Conversion error", "ungültige Länge", "Prüfen Sie Ihren Datensatz", true));
			errors.Add(new errorBundle(-613, "Dateiname der Zieldatei im Empfänger ist zu lang zum Versenden. : ", "Exceets 252 Zeichen bei 4 Byte PackageInfoLength", "s.v.", "Der Dateiname darf die Länge von 252 Zeichen inkl. Leerzeichen nicht überschreiten. Prüfen Sie den Dateinamen. ACHTUNG: Durch die UTF-8-Codierung brauchen Sonderzeichen und Umlaute den Platz von 3 statt 1 Zeichen!!", false));
			errors.Add(new errorBundle(-614, "Datei ist zu groß um Übertragen zu werden.", "Zu übertragende Datei ist größer als 0.984 TB", "s.v.", "Verwenden Sie Splitdateien. Ein Split darf nicht größer als 0.984 TB sein", false));
			errors.Add(new errorBundle(-615, "Die zum Speichern übergebene Effektsetkonfiguration enthält keine Argumente. Speichervorgang abgebrochen", " " , "", "Erstellen Sie mindestens ein Element (defaultEffektSet)", true));
			errors.Add(new errorBundle(-616, "Der an den Konstruktor der Klasse I2pDevCom übergebene Parameter vPcbsHorizontal ist ungültig. Muss >= 1 sein. Parameter ignoriert. Instanzierung wird fortgesetzt...", "", "", "Überprüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-617, "Der an den Konstruktor der Klasse I2pDevCom übergebene Parameter vPcbsVertical ist ungültig. Muss >= 1 sein. Parameter ignoriert. Instanzierung wird fortgesetzt...", "", "", "Überprüfen Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-618, "Die zu sendenden Setupdaten haben eine ungültige Länge und können vom Empfänger nicht verarbeitet werden. Sendevorgang abgebrochen.", "Datalength der übergebenen Daten != Datenlänge im Gerät (wurde vorher ausgelesen)", "", "Überprüfen Sie die Parameter", false));
			errors.Add(new errorBundle(-619, "Das übergebene byte[] ist zu groß um als M3S-Datenpaket versendet zu werden", "Länge > 256 Bytes", " - ", "Die Länge darf nicht größer als 256 Byte sein", false));
			errors.Add(new errorBundle(-620, "Der Endzeitpunkt liegt vor dem Startpunkt.", "...", "..", "Korrigieren Sie Ihre Eingaben", false));
			errors.Add(new errorBundle(-621, "Ein Frame, das mit anderen gepuffert gesendet werden soll, erfordert ein Acknowledge. Das ist nicht zulässig", "AckRequ-Flag im Frame gesetzt", "Versenden von gepufferten Frames die bestätigt werden müssen ist auf einem halbduplexen Bus nicht zulässig => Datenkollision", "Kontaktieren Sie den Hersteller", true));
			errors.Add(new errorBundle(-622, "Das übergebene Frame hat ungültige Länge. Gemäß M3S-Spezifikation musss der höchste Index dem dataUpperBound entsprechen (oder dieser 0 sein)", "Falsche Bytearraylänge", "Programmierfehler", "Prüfen Sie ihren Datensatz", false));
			#endregion
			
			// 800+
			#region Fremdfehler
			errors.Add(new errorBundle(-801, "Netzwerkkomponente am Server ist nicht gestartet und kann daher nicht gesteuert werden.", " ", "s.v.", "Starten Sie die Netzwerkkomponente am Server.", false));
			errors.Add(new errorBundle(-802, "Kalenderkomponente am Server ist nicht gestartet und kann daher nicht gesteuert werden.", " ", "s.v.", "Starten Sie den Kalender am Server.", false));
			errors.Add(new errorBundle(-803, "Die vom Kommunikationspartner (Server) angeforderte Datei existiert nicht.", " ", "s.v.", "Kontrollieren Sie den eingegebenen Dateinamen und prüfen Sie, ob die Datei am Server vorhanden ist", false));
			errors.Add(new errorBundle(-804, "Der Kommunikationspartner (Server) kann die angeforderte Datei - obwohl sie existiert - nicht lesen versenden.", " ", "s.v.", "Prüfen Sie ob die Datei eventuell beschädigt oder am Kommunikationspartner (Server) von einem anderen Programm geöffnet ist", false));
			errors.Add(new errorBundle(-811, "TWI-Error: No acknowledge from Slave when transmitting his Address (DataDirection Master Send, MT-Mode)", "NAK was detected on TWI while sending Slave Address", "Slave with this address does not exist", "Check your system, probably Slave with this address does not exist", true));
			errors.Add(new errorBundle(-812, "TWI-Error: No acknowledge from Slave when sending data", "NAK was detected pm TWI while sending Databytes", "Slave is not ready to receive Data, probably some invalid Data sent beforehand?", "Check your hard- and Software", true));
			errors.Add(new errorBundle(-813, "TWI-Error: No acknowledge from Slave when sending his Address (dataDirection SlaveSend)", "NAK was detected on TWI while sending Slave Address", "Slave with this address does not exist", "Check your hardware", true));
			errors.Add(new errorBundle(-814, "TWI-Error: Startcondition could not be sent", "-", "-", "Check your Equipment", true));
			errors.Add(new errorBundle(-815, "TWI-Error: Readtimeout while reading Byte from Slave that is supposed to be acknowledged (vulgo reading a Byte that's followed by other Bytes)", "probably some undefined bus state occurs", "?!", "Best approach: Look at it with your Scope...", true));
			errors.Add(new errorBundle(-816, "TWI-Error: Readtimeout while reading Byte from Slave that is supposed to be NOT acknowledged (vulgo last Byte in transmission)", "probably some undefined bus state occurs", "?!", "Best approach: Look at it with your Scope...", true));
			errors.Add(new errorBundle(-817, "TWI-Error: Stop condition could not be sent by Master", "Stop condition could not be initiated, processor goes not in 'After-Stop-State'", "Probably some undefined bus state occured", "Best approach: Look at it with your Scope...", true));
            #endregion
			
			return(true);
		}
		
		
		private string errorFileLocation = "";
		private string confFileLocation = "";
		private const string internalOnly = "internalOnly";
		
		private FileStream fs_conf;
		private FileStream fs_error;
		private StreamReader sr_conf;
		private StreamWriter sw_error;
		
		private bool logErrors;
		private bool active;

		private int lastErrCode = 0;
		
		private ArrayList errors;
				
		#region Properties
		public bool isActive
		{
			get
			{
				return(active);
			}
		}
		
		#endregion  
		
		#region Constructors
		public classErrorHandling()
		{
			this.active = false;
			this.logErrors = false;
			confFileLocation = internalOnly; // flag
			
			if(!this.activate())
			{
				throw new Exception("EDOLL-Handler konnte nicht aktiviert werden!!");
			}
		}
		
		public classErrorHandling(string pConfFileLocation)
		{
			this.active = false;
			this.logErrors = false;
			confFileLocation = pConfFileLocation;
			this.activate();
		}
		
		public classErrorHandling(string pConfFileLocation, string pErrorFileLocation)
		{
			this.active = false;
			this.logErrors = true;
			
			confFileLocation = pConfFileLocation;
			errorFileLocation = pErrorFileLocation;
			
			if(!this.activate())
			{
				active = false;
				logErrors = false;
			}
		}
		
		#endregion
		
		#region Activation / Startup
		private bool activate()
		{
			errors = new ArrayList();
			
			if(!this.makePredefinedErrors())
			{
				return(false);
			}
			
			if(this.mkFileStreams())
			{
				
				if(confFileLocation != internalOnly)
				{
					if(readConfFile())
					{
						active = true;
						// close the filestreams, not needed, all in the RAM
						sr_conf.Close();
						fs_conf.Close();
					}
					else
					{
						return(false);
					}
				}
				active = true;
				return(true); //if reached this line it must be all gone well
			}
			else
			{
				return(false);
			}
		}
		
		private bool mkFileStreams()
		{
			if(confFileLocation != internalOnly)
			{
				try
				{
					fs_conf = new FileStream(confFileLocation, FileMode.Open, FileAccess.Read);
					
				}
				catch
				{
					stdOut.Error("Fehler: Fehlerkonfigurationsdatei nicht gefunden @ " + confFileLocation);
					return(false);
				}
				
				try
				{
					sr_conf = new StreamReader(fs_conf, System.Text.Encoding.Default);
				}
				catch
				{
					stdOut.Error("Fehler: StreamReader für Fehlerkonfigurationsdatei nicht erstellbar. Dateizugriff aber möglich.");
					return(false);
				}
			}
			
			if(logErrors)
			{
				try
				{
					fs_error = new FileStream(errorFileLocation, FileMode.Append, FileAccess.Write);
				}
				catch
				{
					stdOut.Error("Fehler: Fehlerdatei nicht gefunden oder erstellbar @ " + errorFileLocation + "\nPrüfen Sie ob die Datei möglicherweise in einem anderen Programm geöffnet ist");
					return(false);
				}
				
				try
				{
					sw_error = new StreamWriter(fs_error);
				}
				catch
				{
					stdOut.Error("Fehler: StreamWriter für Fehlerdatei nicht erstellbar. Dateizugriff aber möglich.");
					return(false);
				}
			}
			return(true);
		}
		
		private bool readConfFile()
		{
			string buffer;
			int lineCnt;
			int tempCode;
			bool tempLogg;
			
			
			fs_conf.Seek(0, SeekOrigin.Begin);
			sr_conf.ReadLine(); // ignore first one;
			lineCnt = 1;
			
			buffer = sr_conf.ReadLine();
			while(buffer != null)
			{
				lineCnt++;
				string[] splitbuffer = buffer.Split(';');
				
				// Exit readConfFile(): when there's an incomplete set of data (not every field filled out properly)
				
				if(splitbuffer.GetUpperBound(0) < (errorBundle.cntMember-1))
				{
					stdOut.Error("Fehler beim Lesen der Fehlerkonfigurationsdatei:\nZeile " + (lineCnt).ToString() + ": unvollständiger Datensatz");
					return(false);
				}
				
				try
				{
					tempCode		= Convert.ToInt32(splitbuffer[0]);
				}
				catch
				{
					stdOut.Error("Fehler beim Lesen der Fehlerkonfigurationsdatei:\nZeile " + (lineCnt).ToString() + ": nicht in Errorcode konvertierbar");
					return(false);
				}
				
				// Scan through and break if error code exists twice, continue otherwise:
				foreach(errorBundle e in errors)
				{
					if(e.Code == tempCode)
					{
						stdOut.Error("Fehler beim Lesen der Fehlerkonfigurationsdatei:\nZeile " + (lineCnt).ToString() + ": Doppelter Errorcode '" + tempCode.ToString() + "'. Es handelt sich entweder um eine ungültige Benutzerfehlernummer (darf nicht zwischen inklusive 0...-1000 liegen) oder die Fehlernummer ist in der Konfigurationsdatei '" + confFileLocation + "' doppelt vorhanden.");
						return(false);
					}
				}
				
				if(splitbuffer[5] == "0")
				{
					tempLogg = false;
				}
				else
				{
					tempLogg = true;
				}
				// Finally add error-bundle
				errors.Add(new errorBundle(tempCode,splitbuffer[1],splitbuffer[2],splitbuffer[3], splitbuffer[4], tempLogg));
				
				// Continue with next line
				buffer = sr_conf.ReadLine();
				
			}
			sr_conf.Close();
			fs_conf.Close();
			
			return(true);
		}
		#endregion
		
		#region Runtime
		
		public int LastErrorCode
		{
			get
			{
				return(lastErrCode);
			}
		}
		
		internal string getErrorMsg(int pErrCode, bool techError)
		{
			if(active)
			{				
				if(pErrCode == 0)
				{
					return("Error #0: Kein Fehler");
				}
				foreach(errorBundle e in errors)
				{
					if(Math.Abs(e.Code) == Math.Abs(pErrCode))
					{
						if(techError)
						{
							return(e.getErrorForLog());
						}
						else
						{
							return(e.ToString());
						}
						
					}
				}
				
				// nothing found:
				return("Error #" + pErrCode.ToString() + ": keine Beschreibung vorhanden");
				
			}
			else
			{
				return("Error #" + pErrCode.ToString() + ": ~~ Fehlererkennung deaktiviert ~~");
			}
		}
		
		public bool noError(int pErrCode)
		{
			lastErrCode = pErrCode;
			
			if(pErrCode >= 0)
			{
				
				return(true);
			}
			else
			{
				
				if(logErrors)
				{
					logError(pErrCode);
				}
				return(false);
			}
		}
				
		public string getLastError()
		{
			return(getErrorMsg(lastErrCode, false));
		}
		
		public string getLastErrorTech()
		{
			return(getErrorMsg(lastErrCode, true));
		}
		
		private bool logError(int pErrCode)
		{
			errorBundle temp_err = null;
			if(logErrors)
			{
				foreach (errorBundle e in errors)
				{
					if(e.Code == pErrCode)
					{
						temp_err = e;
						break;
					}
				}
			}
			
			if(temp_err == null) // no Error with this nr. found
			{
				return(false);
			}
			else
			{
				if(temp_err.Logg)
				{
					sw_error.WriteLine(temp_err.getErrorForLog());
					sw_error.Flush();
				}
				
			}
			
			return(true);
		}
		
		
		#endregion		
	}
	
	/// <summary>
	/// Die EDOLL-Exception ist die am meisten geworfene innerhalb der TBL. Über den Fehlercode kann von außen eine genaue Lokalisation der Ausnahme vorgenommen werden
	/// </summary>
	/// <remarks>
	/// <para>Die Fehlernummern innerhalb der Library sind eindeutig und zumeinst kleine als 0. Das hat primär historische Gründe
	/// </para>
	/// <para>
	/// Bis inkl. Version 1.1 der TBL wurden alle Fehler über die Rückgabewerte der einzelnen Funktionen gesteuert. Oft mussten auch Werte zurückgegeben werden, deswegen wurden Fehler als negative Integer zurückgegeben und Werte als positive Integer. 0 stand bei diesem System für fehlerfreie Ausführung der Funktion.
	/// </para>
	/// <para>Mit Version 1.2 wurde im Dezember 2011 die gesamte Library auf Exceptions umgestellt. Leider kann aus diesem Grund eine Rückwärtskompabilität der meisten Funktionen nicht mehr gewährleistet werden. Der Vorteil einer "besseren" und vor allem flexibleren Softwarearchitektur wiegt dieses Manko in meinen Augen trotzdem auf.
	/// </para>
	/// </remarks>
	public class EDOLLException: Exception
	{
		int code = 0;
		string[] addInfo;
		
		/// <summary>
		/// Instanziert ein neues Objekt mit <see cref="EDOLLHandler">EDOLL-Fehlernummer</see>
		/// </summary>
		/// <param name="vErrorCode"><see cref="EDOLLHandler">EDOLL-Fehlernummer</see></param>
		public EDOLLException(int vErrorCode)
		{
			code = vErrorCode;
			addInfo = null; // no additional information given
		}
		
		/// <summary>
		/// Instanziert ein neues Objekt mit <see cref="EDOLLHandler">EDOLL-Fehlernummer</see> und zusätzlicher Information
		/// </summary>
		/// <param name="vErrorCode"><see cref="EDOLLHandler">EDOLL-Fehlernummer</see></param>
		/// <param name="vInfo">Zusätzliche Information</param>
		public EDOLLException(int vErrorCode, string vInfo)
		{
			code = vErrorCode;
			addInfo = new string[1];
			addInfo[0] = vInfo;
		}
		
		/// <summary>
		/// Instanziert ein neues Objekt mit <see cref="EDOLLHandler">EDOLL-Fehlernummer</see> und mehreren zusätzlichen Informationsstrings
		/// </summary>
		/// <param name="vErrorCode"><see cref="EDOLLHandler">EDOLL-Fehlernummer</see></param>
		/// <param name="vInfo">Zusätzliche Informationen</param>
		public EDOLLException(int vErrorCode, string[] vInfo)
		{
			code = vErrorCode;
			addInfo = vInfo;
		}
		
		/// <summary>
		/// Liefert den Fehleridentifikationscode der Exception
		/// </summary>
		public int ErrorCode
		{
			get
			{
				return(code);
			}
		}
		
		/// <summary>
		/// Meldet den Fehler an <see cref="EDOLLHandler.Error">EDOLLHandler.Error</see> und loggt ihn (je nach Konfiguration, siehe Remarks)
		/// </summary>
		/// <remarks>
		/// Abhängig von der Konfiguration des Errorhandlers (Parameter bei <see cref="EDOLLHandler.Start(bool)">EDOLLHandler.Start()</see> werden die Errors registriert und geloggt, zumindest jedoch als "Last-Error" eingetragen.
		/// </remarks>
		public void Report()
		{
			EDOLLHandler.Error(code);
		}
		
		/// <summary>
		/// Verbalisiert die auftretende Exception für den Enduser
		/// </summary>
		/// <returns>String mit Fehlermeldung für den Enduser</returns>
		public override string ToString()
		{
			return(EDOLLHandler.Verbalize(code));
			
		}
		
		/// <summary>
		/// Verbalisiert die auftretende Exception für Techniker. Es werden Informationen über Throw-Zeile inkl. detaillierterer Fehlermeldungen angegeben
		/// </summary>
		/// <param name="vTechnical">Gibt an, ob eine technische Ausgabe erfolgen soll. false liefert das gleiche Ergebnis wie ToString()</param>
		/// <returns>Abhängig vom Parameter vTechnichal eine technische oder normale Fehlerverbalisierung</returns>
		public string ToString(bool vTechnical)
		{
			string toReturn = "";
			if(vTechnical)
			{
				toReturn = EDOLLHandler.GetErrorDescTech(code);
				toReturn += Environment.NewLine + Environment.NewLine;
				toReturn += Environment.NewLine + Environment.NewLine + base.ToString();
			}
			else
			{
				toReturn = this.ToString();
			}
			
			
			return toReturn;
		}
		
		
	}
}
