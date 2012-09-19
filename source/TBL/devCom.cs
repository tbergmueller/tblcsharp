/*
 * Created by SharpDevelop.
 * User: Thomas Bergmüller
 * Date: 14.11.2010 - 25.11
 * Time: 15:07
 * 
 * Die devCom stellt die Schnittstelle zwischen Protokoll und Hardwareinterface dar und überwacht den gesamten Datentransfer.
 */
 
using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Timers;
using TBL.EDOLL; 
using TBL.Communication.Protocol;
using System.Collections.Generic;

using System.Diagnostics;

namespace TBL.Communication 
{
	// TODO Genausete Checks mit Dokumentation + TCP-Clientcheck
	
	
	/// <summary>
	/// Interface-Methoden, die in allen devCom????-Verbindungsklassen zu finden sind.
	/// </summary>
	/// <remarks>
	/// Verfügbare Interfaces
	/// <list type="table">
	/// 	<listheader><term>Name</term><description>Kurzbeschreibung</description></listheader>
	/// 	<item><term><see cref="devComTcpClient">devComTcpClient</see></term><description>Hardwareinterface über TCP/IP. Die Klasse ist die Clientkomponente, divere Platinen bzw. die m3sServer-Klasse stellen die Serverkomponente da (siehe devCom-Implementierung auf AtMega und AtXMega).</description></item>
	/// 	<item><term><see cref="devComSerialInterface">devComSerialInterface</see></term><description>z.B. USB (mit Hardwareaufwand) bzw. RS232 über COM-Ports</description></item>
	/// 	</list>
	/// </remarks>	
	public interface IDevComHardwareInterface
	{
		/// <summary>
		/// Quasi-Property für den  <see cref="TBL.Communication.ThreadsafeReceiveBuffer">Empfangsbuffers</see>
		/// </summary>
		/// <param name="pRecBuff">Instanz des <see cref="TBL.Communication.ThreadsafeReceiveBuffer">Empfangsbuffers</see></param>
		void setReceiveBuffer(ThreadsafeReceiveBuffer pRecBuff);
		
		/// <summary>
		/// Hardwareverbindung trennen
		/// </summary>
		void Disconnect();
		
		/// <summary>
		/// Bytes an Hardware senden
		/// </summary>
		/// <param name="pData"></param>
		/// <returns>
		/// <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 bei Erfolg)
		/// </returns>
		/// <remarks>
		/// Datenempfang findet über den konfigurierten Empfangsbuffer (siehe <see cref="IDevComHardwareInterface.setReceiveBuffer">setReceiveBuffer()</see>) statt.
		/// </remarks>
		int WriteData(byte[] pData);
		
		/// <summary>
		/// Einfache Ausgabemethode
		/// </summary>
		/// <returns>Liefert eingestellte Verbindungsparameter und Verbindungsstatus</returns>
		string GetInfo(); // returns IP or Name
		
		
		/// <summary>
		/// Verbindung herstellen (ggf. können auch Fehlermeldungen ausgegeben werden)
		/// </summary>
		/// <returns><see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 bei Erfolg)</returns>
		int Connect();	
		
		///<summary>
		/// Quasi-Property
		/// </summary>
		/// <returns>
		/// <list type="table">
		/// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		/// <item><term>true</term><description>Hardwareinterface ist verbunden</description></item>
		/// <item><term>false</term><description>Hardwareinterface nicht verbunden</description></item>
		/// </list>
		/// </returns>
		bool isConnected();		
		
		///<summary>
		/// Baudrate, mit der das Hardwareinterface operiert
		/// </summary>	
		int Baudrate{get;set;}
	}
	
		
	
	/// <summary>
	/// Typ einer DevCom-Instanz
	/// </summary>
	/// <value><para>Auf unterschiedlichen Maschinen gibt es unterschiedliche Implementierungen von DevCom-Masters und -Slaves, die jedoch alle miteinander kommunizieren können.</para><para>Mittels DevComType identifizieren sich die einzelnen Typen. Die Identifikation erfolgt stets über <see cref="DevComMaster.ReadSlaveInformation">GetSlaveInformation</see> und kann nur masterseitig durchgeführt werden.</para> </value>
	/// <remarks>
	/// Die dieser Werte 8 Bit Enumeration sind so gewählt, dass das MSB Master (1) und Slave (0) unterscheidet. Die unteren sieben Bytes sind frei wählbar und sollten Hardwaretypen oder andere Spezifikationen aufschlüsseln.
	/// </remarks>
	public enum DevComType: byte
	{
		/// <summary>
		/// Slaveimplementierung im Atmega
		/// </summary>
		SlaveAtmega = 0x01,
		/// <summary>
		/// Slaveimplementierung im AtXMega
		/// </summary>
		SlaveXmega=0x02,
		/// <summary>
		/// Slaveimplementierung auf Windows-Betriebssystem
		/// </summary>
		SlaveWindows = 0x03,
		/// <summary>
		/// Masterimplementierung auf Atmega
		/// </summary>
		MasterAtmega=0x81,
		/// <summary>
		/// Masterimplementierung auf AtXMega
		/// </summary> 
		MasterXmega=0x82,
		/// <summary>
		/// Masterimplementierung auf Windows-Betriebssystem
		/// </summary>
		MasterWindows=0x03,
		/// <summary>
		/// 
		/// </summary>
		Unknown=0xff,
	}
	
	/// <summary>
	/// Indizes definierte Bytepositionen, die alle Versionen des <see cref="M3SCommand.GetInformation">GetInformation-Kommandos</see> gemein haben
	/// </summary>
	public enum DevComSlaveInfoIndex
	{
		/// <summary>
		/// Versionsnummer
		/// </summary>
		Version=0,
		/// <summary>
		/// Unterversionsnummer
		/// </summary>
		Subversion=1,
		/// <summary>
		/// Revisionsnummer (oft alphanumerisch)
		/// </summary>
		Revision=2,
		/// <summary>
		/// Type (siehe <see cref="DevComType">DevComType</see>)
		/// </summary>
		Type=3,
		/// <summary>
		/// Index des ersten Datenbytes der Slaveinformationen
		/// </summary>
		FirstDataByte=4,		
	}
		
	/// <summary>
	/// Identifiziert das verwendete M3S-Protokoll
	/// </summary>
	/// <remarks>
	/// Da die TBL ständig weiterentwickelt wird, gibt es mittlerweile schon mehrere Major-Versions des M3S-Protokolls. Defaultmäßig verwendet die DevCom das aktuellste Protokoll, aus Gründen der Rückwärtskompabilität ist es aber auch möglich, ältere Protokolle zu verwenden. Siehe dazu <see cref="DevComMaster(IDevComHardwareInterface, DevComProtocol)">Konstruktoraufruf mit Protokollspezifikation</see>"Detaillierte Infro
	/// </remarks>
	public enum DevComProtocol
	{
		/// <summary>
		/// Protokollversion 1.2, 1.1 und 1.0 (<a href="../documents/protocols/m3s_v1-2.pdf">M3S-Version 1.2</a>)
		/// </summary>
		M3S_Version1,
		/// <summary>
		/// Protokollversion 2.0 (<a href="../documents/protocols/m3s_v2-0.pdf">M3S-Version 2.0</a>) - AKTUELLE VERSION
		/// </summary>
		M3S_Version2,
	}
	
	
	/// <summary>
	/// Schnittstelle zur Gerätekommunikation mittels M3S-Protokoll auf unterschiedlichen physikalischen Layers 
	/// </summary>
	/// <remarks>
	/// <h1>devCom - Device Communication</h1>
	/// <para>Die devCom-Klasse stellt die Basisschnittstelle zu Kommunikationspartnern dar. Eine Instanz der devCom-Klasse kann nur mit einer bereits bestehenden Instanz eines <see cref="IDevComHardwareInterface">devComHardwareInterface</see> erstellt werden. Alternativ dazu kann die Erstellung von devCom-Instanzen über externe Konfigurationsfiles erfolgen (siehe dazu <see cref="DevComMaster.CreateDevComsFromCSV">CreateDevComsFromCSV</see>)</para>
	/// <h2>Benutzung der devCom-Klasse</h2>
	/// <para>Will man mit einem Gerät über eine devCom-Instanz kommunizieren, so sind mehrere Schritte notwendig:
	/// <list type="bullet">
	/// 	<item>Erstellen der Instanz mit spezifiziertem Hardwareinterface <see cref="IDevComHardwareInterface">IDevComHardwareInterface</see></item>
	/// 	<item>Herstellen der Verbindung mit <see cref="Connect()">Connect()</see></item>
	/// 	<item>Senden einer Resetfolge (da M3S-Protokollspezifikation) mit <see cref="Reset(out int)">Reset()</see></item>
	/// 	<item>Nun ist die devCom einsatzbereit. Dieser Status kann über <see cref="Ready">die Ready-Property</see> abgefragt werden</item>
	/// </list>
	/// Details dazu sind am besten dem untenstehenden Codebeispiel zu entnehmen
	/// </para>
	/// <para>Please note when using DevComMaster with DevComSerialInterface: If you run your application in Linux (with Mono-Framework) the external tool "stty" has to be installed and accessible from commandline (added in path), because it is used to allow custom baudrates. If not installed, you might try something similar to "apt-get install stty"</para>
	/// </remarks>
	public class DevComMaster  
	{	
		/// <summary>
		/// Dieses Delegat ist für eine allgemeine Ausgabefunktion (z.B. Progressbarupdate) bestimmt. Als Parameter wird ein ganzzahliger Prozentwert (0..100%) übergeben
		/// </summary>
		/// <param name="vPercentage">Aktueller Arbeitsfortschritt bei bestimmten Methoden in ganzzahligen Prozenten</param>
		/// <remarks>
		/// Methoden der DevCom, die ihren Arbeitsfortschritt über dieses Delegat ausgeben:
		/// <list type="bullet">
		/// 	<item><see cref="SendFile">SendFile()</see></item>
		/// 	<item><see cref="ReadFile">ReadFile()</see></item>
		/// 	<item><see cref="SendByteStream"></see></item>
		/// </list>
		/// </remarks>
		public delegate void DDevComPrintProgress(int vPercentage);		
						
		#region Felder		
		
		/// <summary>
		/// Ausgabefunktion für allgemeinen Arbeitsfortschritt in Prozent. Siehe <see cref="DDevComPrintProgress">DDevComPrintProgress</see>
		/// </summary>
		private DDevComPrintProgress 	progressOut = null;
		
		/// <summary>
		/// Hardwareinterface, über das die Kommunikation abgewickelt werden soll. Siehe <see cref="IDevComHardwareInterface">IDevComHardwareInterface</see>
		/// </summary>
		private IDevComHardwareInterface hwInterface;	
		
		/// <summary>
		/// Gibt das aktuell verwendete Protokoll an. Defaultwert: neuestes Protokoll
		/// </summary>
		private IM3S_Handler 			protocol;
		
		// Statusvariablen
		private bool 					available;					// gibt an, ob die gesamte Devcom bereit ist um zu operieren (verbunden, reset, ..)		
		private bool 					readTimeoutReached;			// Flag, das auf true gesetzt wird, sobald ein Readtimeout erreicht wird	
		
		// Komponenten
		private ThreadsafeReceiveBuffer recBuffer; 					// asynchroner Empfangsbuffer (operiert in eigenem Thread)
		private System.Timers.Timer 	watchDog;					// Watchdog für diverse Timeouts		
				
		// Konfiguration und Betriebsmodi			
		private bool 					debugMode = false;			// Debugmodus an/aus. Wenn Debugmodus an, umfangreiche Debug- und interne Fehlermeldungen auf stdOut ausgeben	
		private string 					devComName = "DevCom"; 		// Name der Instanz
		private bool 					printProgress = true;		// Gibt an, ob Arbeitsfortschritte über das progress-Delegat ausgegeben werden sollen oder nicht
		
		// Parameter
		private const int				readTimeoutDefaultMs = 250;	// Default-Readtimeout für diverse Übertragungen
		private int 					tryXTimes = 1; 				// Anzahl an Retries, die durchgeführt werden etwas zu übertragen, bevor aufgegeben wird
		private int 					mAddr = 1;					// Masteradresse
		private bool 					interferenceHandling=true;	// Gibt an, ob der Receivebuffer regelmäßig von Störbytes geleert werden soll		
		private const string 			configFileType = "DevCom";	// Typangabe in Konfigurationsfiles, NICHT case sensitive
		
		
		#endregion
		
				
		#region Properties
		
		/// <summary>
		/// Interner Empfangspuffer
		/// </summary>
		/// <value>
		/// Auf den Empfangspuffer kann nur lesend zugegriffen werden. Er wird nur zu Debugzwecken bei der Softwareentwicklung nach außen sichtbar gemacht.
		/// </value>
		public ThreadsafeReceiveBuffer ReceiveBuffer
		{
			get
			{
				return(recBuffer);
			}
		}
		
		/// <summary>
		/// Setzt die Baudrate, über die das Objekt sendet (in bps)
		/// </summary>
		/// <remarks>
		/// Diese Eigenschaft ist nicht mit den Methoden <see cref="SetBaudrate(int, out int)">SetBaudrate</see> zu verwechseln. Diese Eigenschaft setzt die Baudrate des DevCom-Objekts, mit der dieses die Daten liest und schreibt. Die SetBaudrate()-Methoden verändern die Baudrateneinstellungen innerhalb bestimmter oder aller Slaves.
		/// </remarks>
		public int Baudrate
		{
			set
			{
				hwInterface.Baudrate = value;
				
			}
			get
			{
				return hwInterface.Baudrate;
			}
		}
		
		
		/// <summary>
		/// Readtimeout diverser Schreib- und Lesevorgänge in Millisekunden. Default: 500ms
		/// </summary>
		/// <value>Zeitüberschreitung in Millisekunden</value>
		/// <remarks>
		/// Das Readtimeout wird bei bei allen Schreib- und Lesevorgängen eingesetzt. Bevor das Frame abgesetzt wird, wird ein Timer gestartet. Bis das Acknowledge bzw. die Antwort auf ein Kommando im Empfangsbuffer liegt und interpretiert wurde, darf nicht Zeit vergangen sein, als dieses Readtimeout angibt.
		/// Andernfalls wird ggf. ein wiederholter Sendevorgang der Daten / des Befehls durchgeführt und erneut auf Antwort gewartet. Siehe dazu <see cref="RetrysOnFailure">RetrysOnFailure</see>
		/// <para>Die Zeit, bis ein Übertragungsfehler detektiert wird, geht hier Proportional zum Readtimeout.</para><para>Zeit zur Fehlerausgabe = RetrysOnFailure * ReadTimeoutMs</para> 
		/// </remarks>
		/// <seealso cref="RetrysOnFailure">RetrysOnFailure</seealso>
		public int ReadTimeout
		{
			get
			{
				return((int)watchDog.Interval);
			}
			set
			{
				if(value > 0)
				{
					watchDog.Interval = (double)value;
				}
			}
		}
		
		/// <summary>
		/// Gibt an, wie oft eine Datenübertragung nach einem auftretenden Fehler wiederholt wird, bis die Aktion als nicht durchführbar gilt.
		/// </summary>
		/// <value>
		/// 0 ... sizeof(int)
		/// </value>
		/// <remarks>
		/// <para>Die Zeit, bis ein Übertragungsfehler detektiert wird, geht hier Proportional zum Readtimeout.</para><para>Zeit zur Fehlerausgabe = RetrysOnFailure * ReadTimeout</para> 
		/// </remarks>
		/// <seealso cref="ReadTimeout">ReadTimeout</seealso>
		public int RetrysOnFailure
		{
			get
			{
				return(tryXTimes);
			}
			set
			{
				tryXTimes = value+1;
			}
		}
		
		/// <summary>
		/// Setzt die Ausgabefunktion diverser Arbeitsschrittfortschritte.
		/// </summary>
		/// <value>Es muss eine Methode übergeben werden, die mit dem Prototyp des Delegates <see cref="DDevComPrintProgress">DDevComPrintProgress</see> übereinstimmt.</value>
		public DDevComPrintProgress ProgressOutput
		{
			set
			{
				progressOut = value;
			}
		}
		
		/// <summary>
		/// Legt fest, ob der Prozessfortschritt (z.B. beim Senden von Dateien) auf die im <see cref="ProgressOutput">ProgressOutput</see> zugewiesene Methode erfolgen soll oder nicht. Default: True
		/// </summary>
		/// <value>
		/// <list type="table">
		/// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		/// <item><term>true</term><description>Ausgabe diverser Arbeitsfortschritte erfolgt auf die dem Delegat <see cref="ProgressOutput">ProgressOutput</see> zugewiesene Methode</description></item>
		/// <item><term>false</term><description>Fortschrittsausgabe deaktiviert</description></item>
		/// </list>
		/// </value>
		public bool PrintProgress
		{
			get
			{
				return(printProgress);
			}
			set
			{
				printProgress = value;
			}
			
		}
				
		/// <summary>
		/// Automatische Empfangsbufferentleerung Ein / Aus
		/// </summary>
		/// <value>true ... ein, false ... aus</value>
		/// <remarks>
		/// <para>In Bussystemen treten ab und an Störungen oder unbehandelte Ereignisse auf. Diese werden vom Empfangsbuffer als Bytes interpretiert und empfangen. Da niemand die Daten erwartet bzw. sie als Fehler interpretiert werden, verbleiben Sie im Empfangspuffer.</para>
		/// <para>Bei langen Programmausführungszeiten können diese Störungen dazu führen, dass sich der Empfangspuffer suczessive füllt und schließlich überläuft. Um dem vorzubeugen, wurde im Empfangspuffer (<see cref="ThreadsafeReceiveBuffer">ThreadsafeReceiveBuffer</see>) ein Mechanismus implementiert, der ab einer bestimmten Anzahl von Lesezugriffen nicht gelesene Bytes automatisch erkennt und entfernt.</para>
		/// </remarks>
		public bool AutoGarbageCollection
		{
			get
			{
				return(interferenceHandling);
			}
			set
			{
				interferenceHandling = value;
			}
		}
		
		
		/// <summary>
		/// Via <see cref="TBL.Communication.GarbageCollectionSpeed">gleichnamiger Enumeration</see> kann hier die Geschwindigkeit der automatischen Bufferentleerung eingestellt werden.
		/// </summary>
		/// <exception cref="Exceptions.ObjectNull">Wenn Empfangsbuffer noch nicht instanziert wurde (kann in Praxis normal nicht auftreten)</exception>
		public GarbageCollectionSpeed GarbageCollectionSpeed
		{
			get
			{
				if(recBuffer == null) // Sollte eigtl. nicht passieren
				{
					EDOLLHandler.Error(-18); // Empfangsbuffer nicht zugewiesen
					Exceptions.ObjectNull ex = new TBL.Exceptions.ObjectNull("Empfangsbuffer wurde noch nicht instanziert");
					throw ex;
				}
				return(recBuffer.GarbageCollectionSpeed);
			}
			set
			{
				if(recBuffer == null) // Sollte eigtl. nicht passieren
				{
					EDOLLHandler.Error(-18); // Empfangsbuffer nicht zugewiesen
					Exceptions.ObjectNull ex = new TBL.Exceptions.ObjectNull("Empfangsbuffer wurde noch nicht instanziert");
					throw ex;
				}
				
				recBuffer.GarbageCollectionSpeed = GarbageCollectionSpeed;
			}
		}
		
		/// <summary>
		/// Versetzt die devCom in den debugModus - es werden Trace-Meldungen, Statusmeldungen und interne Fehler ausgegeben (auf <see cref="stdOut">stdOut.Debug und stdOut.Error</see>)
		/// </summary>
		/// <value>
		/// <list type="bullet">
		/// 	<item>true ... Es werden Debug- und Fehlermeldungen ausgegeben</item>
		/// 	<item>false ... keine Meldungen, stummer Modus</item></list>
		/// </value>
		/// <remarks>
		/// Der Debugmodus wurde für Softwareentwickler implementiert, um bei der erstmaligen Inbetriebnahme genauere Informationen über die internen Vorgänge in der DevCom zu erhalten. Ein weiteres Entwicklungstool ist der M3S-Analyzer, der den physikalischen Bytefluss auf den Busleitungen verfolgen und interpretieren kann.
		/// </remarks>
		public bool DebugMode
		{
			get
			{
				return(debugMode);
			}
			set
			{
				debugMode = value;
			}
		}		
		
		/// <summary>
		/// Liefert in einem String alle wichtigen Parameter und Zuständen des DevCom-Objekts zurück.
		/// </summary>
		/// <value>
		/// Neben dem Namen des Objekts liefert der InfoString außerdem noch Verbindungsdaten und Status der Verbindung.
		/// </value>
		public string InfoString
		{
			get
			{
				return("devCom '"+devComName + "': " + hwInterface.GetInfo());
			}
		}
		
		/// <summary>
		/// Bezeichner der devCom-Instanz, kann für die Identifizierung von Objekten verwendet werden
		/// </summary>
		/// <value>Default-Value ist "DevCom" (wie der Klassenname)</value>
		public string Name
		{
			get
			{
				return(devComName);
			}
			set
			{
				devComName = value;
			}
		}
		
		/// <summary>
		/// Statusvariable die angibt, ob DevCom betriebsbereit ist oder nicht
		/// </summary>
		/// <remarks>Die DevCom ist bereit, wenn eine Verbindung besteht und mindestens ein Reset gesendet wurde.</remarks>
		public bool Ready
		{
			get
			{
				return(available);
			}
		}
		#endregion
		
		
		#region Events
		
		/// <summary>
		/// Byte wurde soeben Empfangen und liegt im Empfangspuffer
		/// </summary>
		/// <value>
		/// Übergeben muss eine Funktion, die dem Funktionsprototypen des <see cref="ByteReceivedEventHandler">ByteReceivedEventHandler</see> entspricht.
		/// </value>
		public event ByteReceivedEventHandler ByteReceived
		{
			add
			{
				recBuffer.ByteReceived += value;
			}
			
			remove
			{
				recBuffer.ByteReceived -= value;
			}
		}
		#endregion
		
		
		//TODO: The statics..	Error output usw.	
		#region statics
		/// <summary>
		/// Erstellt gemäß einem übergebenen Konfigurationsfile devCom[] Array mit den entsprechenden <see cref="IDevComHardwareInterface">Interfaces</see>
		/// </summary>
		/// <param name="pConfigFile">Pfad zum Konfigurationsfile vom Typ 'devCom'</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>Instanzenarray devCom[]</term><description>Es wird ein Array mit den Instanzierten devComs zurückgegeben</description></item>
		/// 	<item><term>null</term><description>im Fehlerfall (mit <see cref="EDOLLHandler.GetLastError()">EDOLLHandler.GetLastError()</see> kann der Grund ermittelt werden</description></item>		 	
		/// </list>		
		///  </returns>
		/// <remarks>
		/// <h3>Im Fehlerfall kann die Methode folgende EDOLL-Codes liefern:</h3>
		/// <list type="table">
		/// 	<listheader><term><see cref="TBL.EDOLL.EDOLLHandler.GetLastError">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>-523</term><description>Ungültige Datensatzlänge in Konfigurationsfile</description></item>
		/// 	<item><term>-526</term><description>Konfigurationsdatei kann nicht zum Lesen geöffnet werden</description></item>
		/// 	<item><term>-534</term><description>Konfigurationsdateityp stimmt nicht.</description></item>
		/// 	<item><term>-535</term><description>Falsche Dateiversion, stimmt nicht mit Bibliotheksversion überein.</description></item>
		/// </list>
		/// <h2>Aufbau der Konfigurationsdatei</h2>
		/// <para>Die Konfigurationsdatei kann in Tabellenkalkulationsprogrammen (Excel, ..) oder im Texteditor erstellt werden. Ein Konfigfileeditor für die TBL ist in Planung. Dabei hat die erste Zeile Informationen über die Dateiversion und den Typ der Konfigurationsdatei, die zweite Zeile beinhaltet die Spaltenbenennung. Ab der dritten Zeile folgen die Daten.</para>
		/// // TODO Konfigurationsfileanleitung...
		/// </remarks>
		public static DevComMaster[] CreateDevComsFromCSV(string pConfigFile)
		{
			// TODO: Sonderzeichen die Excel macht rund um die Felder entfernen
			
			StreamReader sr;
			
			// Dateityp prüfen
			if(EDOLLHandler.Error(Check.CheckFileHeaderCSV(pConfigFile, configFileType)))
			{
				stdOut.Error(EDOLLHandler.GetLastError(), "Zeile 1: Dateiheader in '" + pConfigFile + "'");
				return(null);
			}			
			
			int anzDataSets = GetBusNumFromCSV(pConfigFile);			
			
			DevComMaster[] busses = new DevComMaster[anzDataSets];
			
			// rewind
					
			sr= new StreamReader(pConfigFile);
			
			sr.ReadLine(); // Header
			sr.ReadLine(); // Tabellenheader
			
			
			for(int i = 0; i<anzDataSets; i++)
			{				
				string[] buff = sr.ReadLine().Split(';');				
				  
				// TODO Check ob gültiger datensatz!!!!
				// TODO Mitloggen der Busnummern
				// TODO Kein Bus darf 2x vorkommen...			
				DevComMaster tmp;
				
				if(buff[1].Contains("COM") || buff[1].Contains("tty"))
				{
					devComSerialInterface myIfc = new devComSerialInterface(buff[1], int.Parse(buff[2]));
					
					 tmp = new DevComMaster(myIfc);
					 
					 
					 if(buff.Length > 3)
					{
						tmp.Name = buff[3];
					}
					else
					{
						tmp.Name = "Untitled";
					}
				}
				else
				{
					devComTcpClient myTmpClient = new devComTcpClient(buff[1], Convert.ToInt32(buff[2]));				
					tmp = new DevComMaster(myTmpClient);
					
					if(buff.Length > 3)
					{
						tmp.Name = buff[3];
					}
					else
					{
						tmp.Name = "Untitled";
					}
					
				}				
				
				busses[i] = tmp;
			}
			
			return(busses);				
		}	
		
		
		
		
		/// <summary>
		/// Ermittelt die Anzahl der Datensätze in einer Konfigurationsdatei
		/// </summary>
		/// <param name="pConfigFile">Pfad zur Konfigurationsdatei vom Typ 'devCom' </param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="TBL.EDOLL.EDOLLHandler.GetLastError">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>FileHeader in Ordnung</description></item>	
		/// 	<item><term>-523</term><description>Ungültige Datensatzlänge in Konfigurationsfile</description></item>
		/// 	<item><term>-526</term><description>Konfigurationsdatei kann nicht zum Lesen geöffnet werden</description></item>
		/// 	<item><term>-534</term><description>Konfigurationsdateityp stimmt nicht.</description></item>
		/// 	<item><term>-535</term><description>Falsche Dateiversion, stimmt nicht mit Bibliotheksversion überein.</description></item>
		/// </list>
		/// </returns>
		public static int GetBusNumFromCSV(string pConfigFile)
		{
			// TODO: Sonderzeichen die Excel macht rund um die Felder entfernen
			StreamReader sr;
			string line;
			int anzDataSets = 0;
			int storeResult = Check.CheckFileHeaderCSV(pConfigFile, configFileType);
			
			if(EDOLLHandler.Error(storeResult))
			{
				stdOut.Error(EDOLLHandler.GetLastError(), "Zeile 1: Dateiheader in '" + pConfigFile + "'");
				return(storeResult);
			}			
			
			try
			{
				sr= new StreamReader(pConfigFile);				
			}
			catch
			{
				EDOLLHandler.Error(-526);						
				stdOut.Error(EDOLLHandler.GetLastError());
				return(-1);
			}
			
			sr.ReadLine();
			
			line = sr.ReadLine(); // Header
			line = sr.ReadLine(); // Tabellenheader
				
			for(anzDataSets = 0; line != null; anzDataSets++)
			{
				line = sr.ReadLine();
			}
			sr.Close();	
			
			return(anzDataSets);
		}
		#endregion
				
		#region Steuerung und Verbindung
		
		/// <summary>
		/// Stellt eine Verbindung über das Hardwareinterface her.
		/// </summary>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler"><see cref="EDOLLHandler">EDOLL-Fehlercode</see></see>, 0 bei fehlerfreier Ausführung</param>
		/// <returns>
		///  true ... Verbindung hergestellt, false ... Verbindung konnte NICHT hergestellt werden
		/// </returns>
		/// <remarks>
		/// <para>Details zur Verbindungsherstellung siehe einzelne <see cref="IDevComHardwareInterface">Interfaces</see></para>
		/// <para>Wurde die Verbindung erfolgreich hergestellt, ist die DevCom noch NICHT <see cref="Ready">Ready</see>!! Es müsste noch zwingend ein <see cref="Reset(out int)">Reset</see> an die Slaves gesendet werden, um den Bus in einen definierten Zustand zu bringen. Dies wird in dieser Methode unterlassen.</para>
		/// </remarks>
		/// <seealso cref="Connect(out int)">Connect</seealso>
		public bool ConnectWithoutReset(out int oErrorCode)
		{			
			// Validate IP-Addresse
			oErrorCode = hwInterface.Connect(); 			
			
			if(oErrorCode==0)
			{		
				if(debugMode)
				{
					stdOut.Debug("Connection established with: " + hwInterface.GetInfo());
				}
				return(true);
			}
			else
			{
				if(debugMode)
				{
					stdOut.Error("Connection could NOT be established to: " + hwInterface.GetInfo());
				}				
				return(false); // Verbindungsaufbau nicht möglich
			}					
		}
		
		/// <summary>
		/// Stellt eine Verbindung über das Hardwareinterface her.
		/// </summary>
		/// <returns>true .. Verbindung wurde hergestellt, false ... Verbindung konnte nicht hergestellt werden</returns>
		/// <remarks>
		/// <para>Details zur Verbindungsherstellung siehe einzelne <see cref="IDevComHardwareInterface">Interfaces</see></para>
		/// <para>Wurde die Verbindung erfolgreich hergestellt, ist die DevCom noch NICHT <see cref="Ready">Ready</see>!! Es müsste noch zwingend ein <see cref="Reset(out int)">Reset</see> an die Slaves gesendet werden, um den Bus in einen definierten Zustand zu bringen. Dies wird in dieser Methode unterlassen.</para>
		/// </remarks>
		/// <seealso cref="Connect(out int)">Connect</seealso>
		public bool ConnectWithoutReset()
		{
			int dummy;
			
			return(ConnectWithoutReset(out dummy));
		}
		
		/// <summary>
		/// Stellt eine Verbindung über das Hardwareinterface her und sendet das obligatorische Resetframe.
		/// </summary>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerloser Ausführung</param>
		/// <returns>true .. Verbindung wurde hergestellt, DevCom ist jetzt <see cref="Ready">Ready</see>, false ... Verbindung konnte nicht hergestellt werden</returns>
		/// <remarks>
		/// <para>Details zur Verbindungsherstellung siehe einzelne <see cref="IDevComHardwareInterface">Interfaces</see></para>
		/// <para>Wurde die Verbindung erfolgreich hergestellt, wird ein <see cref="Reset(out int)">Reset</see> versendet. Ist auch dieses erfolgreich versendet worden, befindet sich die DevCom im arbeitsbereiten Zustand (siehe <see cref="Ready">Ready</see>).</para>
		/// <para>Das Versenden des Resetframes versetzt den Bus in einen definierten Zustand und somit alle richtig konfigurierten Slaves (Baudrate übereinstimmend) in Empfangsbereitschaft. Nachdem eine Verbindung hergestellt wurde, empfielt es sich, sicherheitshalber einen, mehrere oder alle Slaves zu pingen, um Nicht-Erreichbarkeit von Slaves aufzudecken und/oder defekte Teilnehmer zu identifizieren</para>
		/// <para>Please note when using DevComMaster with DevComSerialInterface: If you run your application in Linux (with Mono-Framework) the external tool "stty" has to be installed and accessible from commandline (added in path), because it is used to allow custom baudrates. If not installed, you might try something similar to "apt-get install stty"</para>
		/// </remarks>
		/// <seealso cref="ConnectWithoutReset(out int)">ConnectWithoutReset</seealso>
		public bool Connect(out int oErrorCode)
		{
			if(ConnectWithoutReset(out oErrorCode))
			{
				return(Reset(out oErrorCode));
			}
			else
			{
				return(false);
			}
		}
		
		/// <summary>
		/// Stellt eine Verbindung über das Hardwareinterface her und sendet das obligatorische Resetframe.
		/// </summary>
		/// <returns>true .. Verbindung wurde hergestellt, DevCom ist jetzt <see cref="Ready">Ready</see>, false ... Verbindung konnte nicht hergestellt werden</returns>
		/// <remarks>
		/// <para>Details zur Verbindungsherstellung siehe einzelne <see cref="IDevComHardwareInterface">Interfaces</see></para>
		/// <para>Wurde die Verbindung erfolgreich hergestellt, wird ein <see cref="Reset(out int)">Reset</see> versendet. Ist auch dieses erfolgreich versendet worden, befindet sich die DevCom im arbeitsbereiten Zustand (siehe <see cref="Ready">Ready</see>).</para>
		/// <para>Das Versenden des Resetframes versetzt den Bus in einen definierten Zustand und somit alle richtig konfigurierten Slaves (Baudrate übereinstimmend) in Empfangsbereitschaft. Nachdem eine Verbindung hergestellt wurde, empfielt es sich, sicherheitshalber einen, mehrere oder alle Slaves zu pingen, um Nicht-Erreichbarkeit von Slaves aufzudecken und/oder defekte Teilnehmer zu identifizieren</para>
		/// <para>Please note when using DevComMaster with DevComSerialInterface: If you run your application in Linux (with Mono-Framework) the external tool "stty" has to be installed and accessible from commandline (added in path), because it is used to allow custom baudrates. If not installed, you might try something similar to "apt-get install stty"</para>
		/// </remarks>
		/// <seealso cref="ConnectWithoutReset(out int)">ConnectWithoutReset</seealso>
		public bool Connect()
		{
			int dummy;
			
			return(Connect(out dummy));
		}
				
		
		/// <summary>
		/// Transmits a Single Resetframe
		/// </summary>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="TBL.EDOLL.EDOLLHandler"><see cref="EDOLLHandler">EDOLL-Fehlercode</see></see>, 0 bei fehlerfreier Übertragung</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Reset erfolgreich gesendet</description></item>
		/// 	<item><term>false</term><description>Es besteht keine Verbindung und auch der Reconnect war erfolglos. Details siehe Ausgabeparameter</description></item>		 	
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>Das Versenden eines einzelnen Resetframes macht nur unter bestimmten Umständen Sinn. Da einige Slaveimplementierungen keine Frameerror-Timeouts haben (müssen), benötigten Sie mindestens ein Frame der maximalen Framelänge um einen Framefehler zu erkennen. Diese Fall tritt ein, wenn ein fälschlicherweise als upperBound interpretiertes Byte den Wert 0xFF hatte.</para>
		/// <para>Wurden seitdem am Bus aber mit Sicherheit genug Daten übertragen (z.B. durch erneute Sendeversuche), sodass die maximale Framelänge zumindest einmal überschritten wurde, reicht ein einzelnes Resetframe aus, um den Bus wieder in einen definierten Zustand zu versetzen.</para>
		/// </remarks>
		public bool SingleReset(out int oErrorCode)
		{			
			if(!hwInterface.isConnected()) // Versuche reconnect
			{
				return(Connect(out oErrorCode)); // probier mal.. DAs ruft die Methode hier nochmal auf...
			}
			 
			if(hwInterface.isConnected())
			{
				byte[] dataBytes = protocol.GetResetFrame().GetDataframe();
				
				available = true; // probiers zumindest mal...	Variable muss hier glaub ich gesezt werden für die unten aufgerufene Funktion, sunst geht die nicht				
				oErrorCode = sendToHardware(dataBytes);
				
				if(oErrorCode==0)
			    {
			   		available = true; // jetzt ists alles im grünen bereich.
			   		
			   		if(debugMode)
			   		{
			   			stdOut.Debug("Single Resetframe successfully sent on bus...");
			   		}
			   		
			   		return(true);
				}			
				else
				{
					if(debugMode)
			   		{
			   			stdOut.Debug("Single Resetframe could NOT be sent on bus...");
			   		}
					
					available = false; // überflüssig, wird eh in sendToHardware gemacht, aber trotzdem übersichtlicher
					return(false);
				}
			}	
			else
			{
				oErrorCode = -11; // keine Verbindung
				return(false);
			}			
		}
		
		/// <summary>
		/// Sendet eine Reset-Sequenz auf den Bus
		/// </summary>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="TBL.EDOLL.EDOLLHandler"><see cref="EDOLLHandler">EDOLL-Fehlercode</see></see>, 0 bei fehlerfreier Übertragung</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Resetsequenz erfolgreich gesendet</description></item>
		/// 	<item><term>false</term><description>Resetsequenz konnte nicht versendet werden. Details siehe Ausgabeparameter oErrorCode</description></item>		 	
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>In der Regel reicht auf dem Bus ein einzelnes Resetframe nicht aus, um zuverlässig den definierten Buszustand (wieder)herzustellen. Das hängt mit dem asynchronen Funktionsprinzip der Kommunikation zusammen.</para>
		/// <para>Da die DevCom-Komponenten vollkommen asynchron arbeiten, sind auch diverse Frame-Timeouts nicht detektierbar. Sollte nun beispielsweise eine Störung eine falsche Startcondition am Bus auslösen, wird dieses Byte vorerst als Controlbyte des nächsten Frames interpretiert. Das darauffolgende als Slaveadresse und das dritte als upperBound. Nun wird solange empfangen, bis das upperBound an Daten erreicht ist, dann erst wird über den CRC festgestellt, dass es sich um ein fehlerhaftes Frame handelt.</para>
		/// <para>Um nun sicherzustellen, dass ein Reset wirklich den Bus in einen definierten Zustand versetzt, müssen (n+1) Resetframes versendet werden, wobei n die Anzahl an Resetframes ist, die hintereinandergereiht die maximale protokollspezifische Framelänge (inkl. Overhead) überschreiten.</para>
		/// <para>Diese Anzahl ist nötig, um etwaige offenen Datentransfers zu beenden (und einen Framefehler festzustellen) und danach noch ein Resetframe, um den definierten Zustand wiederherzustellen. Wenn bei einem definierten Buszustand Resetframes empfangen werden, werden diese akzeptiert, es ändert sich am Status der jeweiligen Slaves nichts.</para>
		/// <para>Konkret werden bei verwendetem Protokoll M3S-V2 53, bei M3S-V1 39 Resetframes in einer Sequenz versendet.</para>
		/// </remarks>
		public bool Reset(out int oErrorCode)
		{			
			byte[] dummyFrame = protocol.GetResetFrame().GetDataframe();
			
			int neededFrames = (int)Math.Ceiling(((decimal)(protocol.MaximumFrameLength) / (decimal)(dummyFrame.Length)));
			
			neededFrames += 1; // one more weil integer Cast abrundet, 1 tatsächliches Resetframe
			
			if(debugMode)
			{
				stdOut.Debug("Start Transmission of "+neededFrames.ToString()+" Reset-Sequences..");
			}
			
			for(int i=0; i<neededFrames; i++)
			{
				if(!SingleReset(out oErrorCode))
				{
					return(false); // wenn was schief ging, breche ab
				}
			}
			
			System.Threading.Thread.Sleep(10);
			
			if(debugMode)
			{
				stdOut.Debug("Reset Sequence complete");
			}
			
			oErrorCode = 0;
			return(true);			
		}
		
		/// <summary>
		/// Sendet eine Reset-Sequenz auf den Bus
		/// </summary>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Resetsequenz erfolgreich gesendet</description></item>
		/// 	<item><term>false</term><description>Resetsequenz konnte nicht gesendet werden</description></item>		 	
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>In der Regel reicht auf dem Bus ein einzelnes Resetframe nicht aus, um zuverlässig den definierten Buszustand (wieder)herzustellen. Das hängt mit dem asynchronen Funktionsprinzip der Kommunikation zusammen.</para>
		/// <para>Da die DevCom-Komponenten vollkommen asynchron arbeiten, sind auch diverse Frame-Timeouts nicht detektierbar. Sollte nun beispielsweise eine Störung eine falsche Startcondition am Bus auslösen, wird dieses Byte vorerst als Controlbyte des nächsten Frames interpretiert. Das darauffolgende als Slaveadresse und das dritte als upperBound. Nun wird solange empfangen, bis das upperBound an Daten erreicht ist, dann erst wird über den CRC festgestellt, dass es sich um ein fehlerhaftes Frame handelt.</para>
		/// <para>Um nun sicherzustellen, dass ein Reset wirklich den Bus in einen definierten Zustand versetzt, müssen (n+1) Resetframes versendet werden, wobei n die Anzahl an Resetframes ist, die hintereinandergereiht die maximale protokollspezifische Framelänge (inkl. Overhead) überschreiten.</para>
		/// <para>Diese Anzahl ist nötig, um etwaige offenen Datentransfers zu beenden (und einen Framefehler festzustellen) und danach noch ein Resetframe, um den definierten Zustand wiederherzustellen. Wenn bei einem definierten Buszustand Resetframes empfangen werden, werden diese akzeptiert, es ändert sich am Status der jeweiligen Slaves nichts.</para>
		/// <para>Konkret werden bei verwendetem Protokoll M3S-V2 53, bei M3S-V1 39 Resetframes in einer Sequenz versendet.</para>
		/// </remarks>
		public bool Reset()
		{
			int dummy;			
			return(this.Reset(out dummy));
		}
		
		#endregion
		
		#region Datentransfer	

		// TODO: This stuff here...
		
		#region file
		
		bool SendFile()
		{
			throw new NotImplementedException();
			//return(false);
		}
		
		bool ReadFile()
		{
			throw new NotImplementedException();
			//return(false);
		}
		
		bool SendByteStream()
		{
			throw new NotImplementedException();
			//return(false);
		}
		#endregion
		
		
		/*
		#region File- and Streamtransfer	
	    // TODO: Diese Doku überarbeiten!!
	    
	    /// <summary>
		/// Liest eine Datei vom Kommunikationspartner
		/// </summary>
		/// <param name="pM3sSlaveAddress">Slaveadresse des Kommunikationspartners</param>
		/// <param name="pRequestFilename">Pfad zur Datei beim Kommunikationspartner</param>
		/// <param name="pSaveFileName">Pfad der Zieldatei, in die geschrieben werden soll</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Datei erfolgreich empfangen und auf Zieldateiort geschrieben</description></item>
		/// 	<item><term>-214</term><description>Kommunikationspartner gibt keine Auskunft über die Ausführung des File-Announcement-Frame. Readtimeout erreicht</description></item>		
		/// 	<item><term>-215</term><description>Datenpaket kommt nicht rechtzeitig an. Readtimeout erreicht, Dateiempfang abgebrochen</description></item>		
		/// 	<item><term>-801 ... xxx -</term><description>Custom-Errors und Execution Errors. <see cref="m3sExecutionError">m3sExecutionError</see></description></item>	
		/// </list>
		/// </returns>
		public int ReadFile(int pM3sSlaveAddress, string pRequestFilename, string pSaveFileName)
		{
			int returnValue = 0;
			float currentProgress = 0;
			float progressPerStep = 0;
			
			bufferOffsetForFileReceiving = recBuffer.DataPtr;
			int err;
			byte[] answerFrame;
												
			if(this.sendCommandReadAnswer(protocol.GetFileRequestFrame(pM3sSlaveAddress, pRequestFilename), out answerFrame, out err))
			{
				bufferOffsetForFileReceiving += 0; //everything should be cleared..
			
				// Visualisiere mal dass gestartet ist...
				currentProgress = 5;
				this.progress((int)(currentProgress));
								
				
				if((m3sExecutionError)answerFrame[3] == m3sExecutionError.NoError)
				{
					fileReceiveComplete = false;					
					
					// Check Dateigröße
					fileReceivePackageAmount = Convert.ToInt32((int)(answerFrame[4] << 24) | (int)(answerFrame[5] << 16) | (int)(answerFrame[6] << 8) | (int)(answerFrame[7]));
					lastReceivedFileFrame = 0; // RESET
						
					if(debugMode)
					{
						stdOut.Debug("Die mit devCom.ReadFile() angeforderte Datei '" + pRequestFilename + "' (Dateiname im Slave) wird in " + fileReceivePackageAmount.ToString() + " M3S-Datenpaketen zu je 252 Nutzdatenbytes geliefert");
					}
					
					progressPerStep = 95 / (float)(fileReceivePackageAmount);
					
					while(!fileReceiveComplete)
					{						
						fileReadTimeout = new System.Timers.Timer(2000);
						fileReadTimeout.Elapsed += new System.Timers.ElapsedEventHandler(FileReadTimeoutTimerElapsed);
						
						fileReadTimeoutReached = false;
						fileReadTimeout.Enabled = true;
						
						int length = 5;		// Mindestframelänge...							
						bool receiveStart = true;
						int errCodeBuffer = 0;
											
						while((isWritingFile || receiveStart) && !fileReadTimeoutReached) 
						{
							if((bufferOffsetForFileReceiving + length < recBuffer.DataPtr) && (bufferOffsetForFileReceiving + length >= 5)) // warten..., fünf ist die mindestframelänge...
							{
								byte[] toCheck = recBuffer.readBytes(bufferOffsetForFileReceiving, bufferOffsetForFileReceiving+length);
																							
								if(protocol.IsFrame(toCheck))
								{
									fileReadTimeout.Enabled = false; // reset
								
									receiveStart = false;
									recBuffer.FreeBytes(bufferOffsetForFileReceiving, bufferOffsetForFileReceiving+length);	
									errCodeBuffer = writeToFile(pSaveFileName,toCheck);
									
									if(EDOLLHandler.Error(errCodeBuffer))
									{
										fileReadTimeout.Enabled = false;
										if(debugMode)
										{
											stdOut.Debug(EDOLLHandler.GetLastError());
										}
										
										return(errCodeBuffer);										
									}
									
									bufferOffsetForFileReceiving = bufferOffsetForFileReceiving + length + 1; // offset vorrücken...
									length = 5; 						// mindestlänge?!	
									currentProgress += progressPerStep; // progressbar...
									
									this.progress(Convert.ToInt16(currentProgress));
									
									recBuffer.Flush(); // Buffer leeren...
									bufferOffsetForFileReceiving = recBuffer.DataPtr; // Neuer offset...
									
									// Acknowledge senden und so signal zum fortfahren geben
									byte[] ackFrame = protocol.GetAcknowledgeFrame(true, toCheck);
									
									hwInterface.WriteData(ackFrame);
									fileReadTimeout.Enabled = true;
								}	
								
								length++; // nächstes frame, nicht so tragisch, weil 1 wirds eh nie haben(wegen dem rücksetzen im if..);								
							
							}							
						}
						
						fileReadTimeout.Enabled = false;
						
						recBuffer.Flush(); // Alles verbleibende entfernen...
						
						if(!fileReadTimeoutReached)
						{							 
							returnValue = 0; // no error
						}		
						else
						{
							returnValue = -215; // Readtimeout, Paket ist nich rechtzeitig angekommen
							
							if(debugMode)
							{
								stdOut.Debug("FileReadTimeout reached...");
							}
							// Abbruch
							fileReceiveComplete = true; // Dateiempfang abbrechen...
						}
					}
					
					return(returnValue);
				}
				else
				{					
					// TODO Errornummer...
					if(debugMode)
					{
						stdOut.Debug("Serverkomponente kann gerade Konfigurationsdatei nicht liefern: " + ((m3sExecutionError)answerFrame[3]).ToString());
					}
						// customOffset - customFehlernummer
					return(-800 -((int)(answerFrame[3]))); // Custom fehlernummer zurückgeben...
				}
				
			}
			else
			{
				recBuffer.Flush(); // Buffer löschen was auch immer zum löschen markiert wurde...
				
				return(err); // No answer of executionstate...
			}
		
		}
			    
	    
	    /// <summary>
	    /// Sendet ein bestimmtes File an den Slave
	    /// </summary>
	    /// <param name="pFilePath">Zu sendende Datei</param>
	    /// <param name="pSlaveAddr">Slaveadresse, an den das File übertragen werden soll</param>
	    /// <param name="pTargetFileName">Dateiname / Speicherort im Empfänger</param>
	    /// <returns>
	    /// <para>
		/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
		/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
		/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.getLastError()</see>)
		/// 	</para>
		/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Fehlerfreie Ausführung</description></item>
		/// 	<item><term>-203</term><description>Geschriebenes Frame wurde nicht bestätigt und ist daher offiziell nicht angekommen.</description></item> 	
		/// 	<item><term>-209</term><description>Empfänger antwortet nicht auf das File-Announcement-Frame (siehe M3S-Doku). Wird im Vorfeld des eigentlichen Dateitransfers geschickt</description></item>	
		/// 	<item><term>-527</term><description>Zu übertragende Datei kann nicht geöffnet oder gelesen werden</description></item>
		/// 	<item><term>-614</term><description>Die zu versendende Datei ist zu groß. Siehe Remarks zur maximalen Dateigröße</description></item>
		/// </list>
	    /// </returns>
	    /// <remarks>
	    /// Die zu sendende Datei wird über das M3S-Protokoll in kleine Stücke (jeweils 256 Nutzdatenbytes) zerhackt und über das spezifizierte im <see cref="DevComMaster(IDevComHardwareInterface)">Konstruktor</see> definierte <see cref="IDevComHardwareInterface">Hardwareinterface</see> übertragen - vorrausgesetzt eine Verbindung besteht.
	    /// <para>ACHTUNG: In dieser Version der Library wird das File als gesamtes ins RAM geladen (via Filestream) und dann die Übertragung gestartet. Es findet kein gepuffertes Einlesen statt.</para>
	    /// <para>Maximal zu übertragende Dateigröße (theor.) 1.008 TB</para>
	    /// </remarks>
	    public int SendFile(int pSlaveAddr, string pTargetFileName, string pFilePath)
		{
			// TODO Große Dateien, nicht alles auf einmal in den Buffer lesen...
					
			FileStream fs;
			
			FileInfo FI = new FileInfo(pFilePath);
					
			
			try
			{
				fs = new FileStream(pFilePath, FileMode.Open);
			}
			catch
			{
				return(-527); // Datei nicht gefunden oder nicht öffenbar
			}		
			
			if(fs != null) // Wenn geöffnet
			{						
				byte[] readBuffer = new byte[FI.Length];
				
				try
				{
					fs.Read(readBuffer,0, (int) FI.Length); // Einlesen
					fs.Close();
				}
				catch
				{
					fs.Close();	
					return(-527); // Datei nicht lesbar
				}
				
				
				return(SendByteStream(pSlaveAddr, pTargetFileName, readBuffer));
			}
			else
			{
				return(-527); // Mit dem File ist was faul, kann nicht zum Lesen öffnen..
			}
				
				
		}
	   
	    // TODO: Diese Doku überarbeiten
	    /// <summary>
	    /// Sendet einen Bytestream an einen M3S-Empfänger
	    /// </summary>
	    /// <param name="pSlaveAddress">M3S-Adresse des Empfängers</param>
	    /// <param name="pTargetFileName">Dateiname im Empfänger (Tipp: use "default" wenn sich der Empfänger darum kümmern soll)</param>
	    /// <param name="pByteStream">Zu versendender Bytestream</param>
	    /// <returns>
	    /// <para>
		/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
		/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
		/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.getLastError()</see>)
		/// 	</para>
		/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Fehlerfreie Ausführung</description></item>
		/// 	<item><term>-203</term><description>Geschriebenes Frame wurde nicht bestätigt und ist daher offiziell nicht angekommen.</description></item> 	
		/// 	<item><term>-209</term><description>Empfänger antwortet nicht auf das File-Announcement-Frame (siehe M3S-Doku). Wird im Vorfeld des eigentlichen Dateitransfers geschickt</description></item>	
		/// 	<item><term>-614</term><description>Die zu versendender Bytestream ist zu groß. Siehe Remarks zur maximalen Streamlänge</description></item>
		/// </list>
	    /// </returns>
	    /// <remarks> 
	    /// <para>TODO: Announcementframe, verweis auf M3S-Doku...</para>
	    /// Der zu sendende Stream wird über das M3S-Protokoll in kleine Stücke (jeweils 252 Nutzdatenbytes) zerhackt und über das spezifizierte im <see cref="DevComMaster(IDevComHardwareInterface)">Konstruktor</see> definierte <see cref="IDevComHardwareInterface">Hardwareinterface</see> übertragen - vorrausgesetzt eine Verbindung besteht.
	    /// <para>Jedes Paket wird bei dieser Übertragung mit einer Paketnummer versehen.</para>
	    /// <para>Maximal zu übertragende Datenbytes (theor.) 1.008 TB</para>
	    /// </remarks>
	    public int SendByteStream(int pSlaveAddress, string pTargetFileName, byte[] pByteStream)
	    {	    	
	    	if((UInt64)pByteStream.Length > (UInt64)(Math.Pow(2,32)*252)) // 32 Bit paketlänge, 252 Bytes pro Paket => ~1TB
			{
				return(-614); // Datei zu groß, kann Paketlänge nicht angeben
			}	
	    	
	    	if(debugMode)
			{
				stdOut.Debug("Beginne mit Bytestream-Transfer (" + pByteStream.Length.ToString() + " Nutzdatenbytes)" + Environment.NewLine + "Kündige Filetransfer an...");
			}
    	
	    	byte[] announcementFrame = protocol.GetFileTransferAnnouncementFrame(pSlaveAddress, pByteStream.Length, pTargetFileName);	
			int errCode;	    	
			byte[] answerFrame;
			// FIXME: Hier wurde geändert, obige Zeile Flusht jetzt innerhalb der Methode, vorher wurde hier nicht geflusht, problem?
			
			if(!this.sendCommandReadAnswer(announcementFrame, out answerFrame, out errCode))
			{
				return(errCode); // Slave antwortet nicht zum Dateitransfer
			}				
			
			
			
			if(answerFrame[3] != (byte)(m3sExecutionError.NoError))
			{
				if(debugMode)
				{
					stdOut.Debug("Antwort vom Empfänger erhalten, Empfänger nicht bereit. Details: " + ((m3sExecutionError)answerFrame[3]).ToString());
				}
				return((int)(answerFrame[3]) * (-1) - 800); 
			}
			
			
			
			progress(5); // Mal 5 Prozent anzeigen, zum Zeichen dass was passiert ist
			
			float progressPerStep;
			
			if(pByteStream.Length < 252)
				progressPerStep = 95;
			else
				progressPerStep	= 95 / ((float)(pByteStream.Length) / 252);
			
			float currentProgress = 5; // Ausgangswert...
			
			// Jetzt in kleinen Paketen senden...
			byte[] toSend = new Byte[256+6];
			
			toSend = protocol.GetFileTransferFrame(pSlaveAddress,pByteStream);
			
			while( toSend != null)
			{
				int errNrBuff = this.writeFrame((toSend));
				recBuffer.Flush(); // Acknowledge löschen
				
				if(EDOLLHandler.Error(errNrBuff))
				{
					stdOut.Error(EDOLLHandler.GetLastError(), "Fehler beim Dateiübertragen, kein Acknowledge? => sendFile()");
					recBuffer.Flush();
					return(errNrBuff); // Fehlernummern von WriteFrame
				}
				
				// Visualisierung...
				currentProgress += progressPerStep;					
				progress(Convert.ToInt16(currentProgress));
				
				toSend = protocol.GetFileTransferFrame(pSlaveAddress,pByteStream);						
			}
			
			
			if(debugMode)
			{
				stdOut.Debug("Dateitransfer erfolgreich abgeschlossen");
			}
			
			recBuffer.Flush();
			return(0);
	    }
	    
	    #endregion
	     
	    */
	    #region Datentransfer
	    /// <summary>
	    /// Sendet maximal 256 Byte Payload an einen Slave mit spezifizierter Adresse
	    /// </summary>
	    /// <param name="vSlaveAddress">Slaveadresse, an die das Datenpaket geht, 1... 255</param>
	    /// <param name="rData">maximal 256 Bytes Nutzdaten</param>
	    /// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
	    /// <param name="vAcknowledgeRequest">Gibt an, ob Slave mit Acknowledge quittieren muss (true) oder ob eine Quittierung entfällt (false)</param>	    
	    /// <returns>
	    /// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">Rückgabewert</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
		/// </list>
	    ///</returns>
	    /// <remarks>
	    /// Empfangspuffer wird nach Abarbeitung der Methode geflushed.
	    /// </remarks>
		public bool SendData(int vSlaveAddress, byte[] rData, out int oErrorCode, bool vAcknowledgeRequest)
		{
			// TODO datenlängencheck			
			IM3S_Dataframe toSend = protocol.CreateFrame(vSlaveAddress,M3SProtocol.DataTransfer, mAddr, rData,true,vAcknowledgeRequest);	
						
			oErrorCode = this.writeFrame(toSend);
						
			recBuffer.Flush();
			
			if(oErrorCode==0)
			{
				return(true);
			}
			else
			{
				return(false);
			}	
		}	
		
		 /// <summary>
	    /// Sendet maximal 256 Byte Payload und Acknowledgerequest an einen Slave mit übergebener Adresse, wartet auf Acknowledge.
	    /// </summary>
	    /// <param name="vSlaveAddress">Slaveadresse, an die das Datenpaket geht, 1... 255</param>
	    /// <param name="rData">maximal 256 Bytes Nutzdaten</param>
	    /// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
	     /// <returns>
	    /// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">Rückgabewert</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
		/// </list>
	    ///</returns>
		public bool SendData(int vSlaveAddress, byte[] rData, out int oErrorCode)
		{
			// TODO datenlängencheck			
			
			return(SendData(vSlaveAddress, rData, out oErrorCode, true));
		}	
		
		/// <summary>
	    /// Sendet maximal 256 Byte Payload an einen Slave mit spezifizierter Adresse
	    /// </summary>
	    /// <param name="vSlaveAddress">Slaveadresse, an die das Datenpaket geht, 1... 255</param>
	    /// <param name="rData">Nutzdaten (max. 256 Byte)</param>
	    /// <returns>
	    /// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
	    ///</returns>
		public bool SendData(int vSlaveAddress, byte[] rData)
		{
			int dummy;			
			return(SendData(vSlaveAddress,rData, out dummy));
		}
		
		/// <summary>
		/// Sendet Frames gepuffert über das Hardwareinterface. Es dürfen dabei nur Frames, die keine Antwort erfordern, versendet werden (Broadcasts, Multicasts, Unicasts ohne gesetztes AckRequ-Flag) 
		/// </summary>
		/// <param name="rFrames">Zu versendende Frames</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
	    /// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">Rückgabewert</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
		/// </list>
		/// <remarks>
		/// Intern werden die Frames in einen durchgehenden Bytestream umgewandelt, der dann an das Hardwareinterface versendet wird. Das macht Sinn um den Sendepuffer der Hardwareinterfaces (z.B. COM-Port, USB2Serial-Converter, TCP-Server) auszunutzen. In der Regel sind moderne Datenübertragungsprotokolle auf Protokolle mit einem Payload von 1000+ Bytes ausgelegt, Der Verbindungsauf- und abbau für Einzelpakete von ein paar Bytes würde sich definitiv nicht lohnen und ist daher ineffizient. 
		/// </remarks>
		public bool SendFramesBuffered(List<IM3S_Dataframe> rFrames, out int oErrorCode)
		{
			// TODO Exception wenn ein Frame kein Acknowledge erfordert
			
			int bytes = 0;
			int cnt = 0;
			foreach(IM3S_Dataframe frame in rFrames)
			{
				if(frame.NeedsAcknowledgement)
				{
					throw new EDOLLException(-621, "Frame Index " + cnt.ToString() + ": " + frame.ToString());
				}
				cnt++;
				bytes += frame.Length;
			}
			
			byte[] arrayToSend = new byte[bytes];
			
			int byteCnt = 0;
			foreach(IM3S_Dataframe frame in rFrames)
			{
				byte[] frameBytes = frame.GetDataframe();
				
				for(int i=0; i<frameBytes.Length; i++)
				{
					arrayToSend[byteCnt] = frameBytes[i];
					byteCnt++;
				}				
			}
			
			oErrorCode = sendToHardware(arrayToSend);
			
			return(errCode2bool(oErrorCode));
			
			
			
		}
		
		/// <summary>
		/// Sendet maximal 256 Byte Payload an alle Slaves
		/// </summary>
		/// <param name="rData">Nutzdaten (max. 256 Byte)</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
		/// </list>
		/// </returns>
		public bool SendDataBroadcast(byte[] rData, out int oErrorCode)
		{
			oErrorCode = sendBroadOrMulticast(0, rData, M3SProtocol.BroadCast, false);			
			return(errCode2bool(oErrorCode));
		}	
		
		/// <summary>
		/// Sendet maximal 256 Byte Payload an alle Slaves
		/// </summary>
		/// <param name="rData">Nutzdaten (max. 256 Byte)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>
		public bool SendDataBroadcast(byte[] rData)
		{
			int dummy;
			return(SendDataBroadcast(rData, out dummy));
			
		}	
		
		
		/// <summary>
		/// Sendet maximal 256 Byte Payload an alle Slaves einer Multicastgruppe
		/// </summary>
		/// <param name="vMulticastAddress">Multicastadresse 1... 255(Gruppe)</param>
		/// <param name="rData">Nutzdaten (max. 256 Byte)</param>		
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
		/// </list>
		/// </returns>		
		public bool SendDataMulticast(int vMulticastAddress, byte[] rData, out int oErrorCode)
		{			
			oErrorCode = sendBroadOrMulticast(vMulticastAddress, rData, M3SProtocol.BroadCast, true); // send Multicast
			return(errCode2bool(oErrorCode));
		}
		
		/// <summary>
		/// Sendet maximal 256 Byte Payload an alle Slaves einer Multicastgruppe
		/// </summary>
		/// <param name="vMulticastAddress">Multicastadresse 1... 255(Gruppe)</param>
		/// <param name="rData">Nutzdaten (max. 256 Byte)</param>		
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">Rückgabewert</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>		
		public bool SendDataMulticast(int vMulticastAddress, byte[] rData)
		{	
			int dummy;			
			return(SendDataMulticast(vMulticastAddress, rData, out dummy));
		}
		#endregion						
		
		#region CommandTransfer
		/// <summary>
		/// Sendet Kommando an einen bestimmten Slave
		/// </summary>
		/// <param name="pSlaveAddr">Slaveadresse</param>
		/// <param name="rCommandWithParameters">Zu sendendes Kommando mit optionalen Parametern (siehe <exception cref="M3SCommand">M3SCommands</exception></param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		/// <param name="vAcknowledgeRequest">Gibt an, ob Slave das Kommando mit Acknowledge quittieren muss (true) oder ob eine Quittierung entfällt (false)</param>	    
	    /// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// Bekannte Kommandos (siehe <see cref="IM3S_Handler.KnownCommandValidation">IM3S_Handler.KnowCommandValidation</see>) werden hinsichtlich ihrer Parameter validiert. Sind sie invalide, werden sie nicht versendet. Die Methode liefert false zurück. Details können über den Ausgabeparameter oErrorCode gewonnen werden.
		/// </remarks> 
		public bool SendCommand(int pSlaveAddr, byte[] rCommandWithParameters, out int oErrorCode, bool vAcknowledgeRequest)
		{			
			if(!protocol.KnownCommandValidation(rCommandWithParameters, false, out oErrorCode))
			{
				return(false);
			}
			
			int bef = recBuffer.DataPtr;			
			IM3S_Dataframe toSend = protocol.CreateFrame(pSlaveAddr,M3SProtocol.Command, mAddr, rCommandWithParameters ,true,vAcknowledgeRequest);
			oErrorCode = this.writeFrame(toSend);			
			recBuffer.Flush();
			
			return(errCode2bool(oErrorCode));
		}	
		
		/// <summary>
		/// Sendet Kommando mit Acknowledgerequest an einen bestimmten Slave
		/// </summary>
		/// <param name="pSlaveAddr">Slaveadresse</param>
		/// <param name="rCommandWithParameters">Zu sendendes Kommando mit optionalen Parametern (siehe <exception cref="M3SCommand">M3SCommands</exception></param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		 /// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// Bekannte Kommandos (siehe <see cref="IM3S_Handler.KnownCommandValidation">IM3S_Handler.KnowCommandValidation</see>) werden hinsichtlich ihrer Parameter validiert. Sind sie invalide, werden sie nicht versendet. Die Methode liefert false zurück. Details können über den Ausgabeparameter oErrorCode gewonnen werden.
		/// </remarks> 
		public bool SendCommand(int pSlaveAddr, byte[] rCommandWithParameters, out int oErrorCode)
		{
			return this.SendCommand(pSlaveAddr,rCommandWithParameters,out oErrorCode,true);
		}
		
		/// <summary>
		/// Sendet ein Kommando an alle Slaves.
		/// </summary>
		/// <param name="rCommandWithParameters">Zu sendendes Kommando mit optionalen Parametern (siehe <exception cref="M3SCommand">M3SCommands</exception></param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// Bekannte Kommandos (siehe <see cref="IM3S_Handler.KnownCommandValidation">IM3S_Handler.KnowCommandValidation</see>) werden hinsichtlich ihrer Parameter validiert. Sind sie invalide, werden sie nicht versendet. Die Methode liefert false zurück. Details können über den Ausgabeparameter oErrorCode gewonnen werden. Bei der Validierung wird auch darauf geachtet, ob dieses Kommando als Broadcast verschickt werden darf.
		/// </remarks> 
		public bool SendCommandBroadcast(byte[] rCommandWithParameters, out int oErrorCode)
		{
			oErrorCode = sendCommandBroadOrMulticast(0, rCommandWithParameters, false);
			return(errCode2bool(oErrorCode));
		}	
		
		/// <summary>
		/// Sendet ein Kommando an eine Gruppe Slaves (Multicast-Domäne)
		/// </summary>
		/// <param name="vMulticastAddress">Multicast- bzw. Gruppenadresse</param>
		/// <param name="rCommandWithParameters">Zu sendendes Kommando mit optionalen Parametern (siehe <exception cref="M3SCommand">M3SCommands</exception></param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// Bekannte Kommandos (siehe <see cref="IM3S_Handler.KnownCommandValidation">IM3S_Handler.KnowCommandValidation</see>) werden hinsichtlich ihrer Parameter validiert. Sind sie invalide, werden sie nicht versendet. Die Methode liefert false zurück. Details können über den Ausgabeparameter oErrorCode gewonnen werden. Bei der Validierung wird auch darauf geachtet, ob dieses Kommando als Multicast verschickt werden darf.
		/// </remarks> 
		public bool SendCommandMulticast(int vMulticastAddress, byte[] rCommandWithParameters, out int oErrorCode)
		{
			oErrorCode = sendCommandBroadOrMulticast(vMulticastAddress, rCommandWithParameters, true);
			return(errCode2bool(oErrorCode));
		}
		
		/// <summary>
		/// Sendet ein Kommando an eine Gruppe Slaves (Multicast-Domäne)
		/// </summary>
		/// <param name="vMulticastAddress">Multicast- bzw. Gruppenadresse</param>
		/// <param name="rCommandWithParameters">Zu sendendes Kommando mit optionalen Parametern (siehe <exception cref="M3SCommand">M3SCommands</exception></param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// Bekannte Kommandos (siehe <see cref="IM3S_Handler.KnownCommandValidation">IM3S_Handler.KnowCommandValidation</see>) werden hinsichtlich ihrer Parameter validiert. Sind sie invalide, werden sie nicht versendet. Die Methode liefert false zurück. Bei der Validierung wird auch darauf geachtet, ob dieses Kommando als Multicast verschickt werden darf.
		/// </remarks> 
		public bool SendCommandMulticast(int vMulticastAddress, byte[] rCommandWithParameters)
		{
			int dummyOut = sendCommandBroadOrMulticast(vMulticastAddress, rCommandWithParameters, true);
			return(errCode2bool(dummyOut));
		}
					
		#endregion
		
		
		#region Multicast Settings
			
			/// <summary>
			/// Setzt die Multicastadresse eines bestimmten Slaves
			/// </summary>
			/// <param name="vSlaveAdress">Slave, auf dem die Operation ausgeführt werden soll</param>
			/// <param name="vNewMulticastAddress">Wert, auf den die Multicastadresse des Slaves gesetzt werden soll.</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
			/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
			/// </list> 
			/// </returns>
			/// <remarks>Diese Funktion setzt (verändert) die Multicastadresse eines einzelnen Slaves. Daraufhin ist er nur mehr durch seine Adresse und seine neue Multicastadresse erreichbar, die alte verfällt.</remarks>
			public bool SetMulticastAddress(int vSlaveAdress, int vNewMulticastAddress)
			{
				int dummyOut;				
				return(SetMulticastAddress(vSlaveAdress, vNewMulticastAddress, out dummyOut));
			}
			
			/// <summary>
			/// Setzt die Multicastadresse eines bestimmten Slaves
			/// </summary>
			/// <param name="vSlaveAddress">Slave, auf dem die Operation ausgeführt werden soll</param>
			/// <param name="vNewMulticastAddress">Wert, auf den die Multicastadresse des Slaves gesetzt werden soll.</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
			/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>	
			/// <remarks>Diese Funktion setzt (verändert) die Multicastadresse eines einzelnen Slaves. Daraufhin ist er nur mehr durch seine Adresse und seine neue Multicastadresse erreichbar, die alte verfällt.</remarks>
			public bool SetMulticastAddress(int vSlaveAddress, int vNewMulticastAddress, out int oErrorCode)
			{
				#region Parameter validation			
				oErrorCode=Check.CheckM3SSlaveAddress(vSlaveAddress);
				
				if(oErrorCode!=0)
				{
					if(debugMode)
					{
						stdOut.Error(EDOLLHandler.GetErrorDescription(oErrorCode), "SlaveAddress: " + vSlaveAddress.ToString());
					}
					return(false);
				}
				
				oErrorCode=Check.CheckM3SSlaveAddress(vNewMulticastAddress);
				
				if(oErrorCode!=0)
				{
					if(debugMode)
					{
						stdOut.Error(EDOLLHandler.GetErrorDescription(oErrorCode), "NewMulticastAddress: " + vNewMulticastAddress.ToString());
					}
					return(false);
				}
				#endregion
				
				byte[] cmdFrame = {(byte)M3SCommand.SetMulticastAddress, (byte)vNewMulticastAddress};
				
				if(debugMode)
				{
					stdOut.Debug("Changing Multicast Address from Slave (Addr. " +vSlaveAddress.ToString()+") to " + vNewMulticastAddress.ToString());
				}
				this.SendCommand(vSlaveAddress,cmdFrame, out oErrorCode, true);
				
				return(errCode2bool(oErrorCode));
			}
			
			/// <summary>
			/// Verändert die Multicastadresse einer bestehenden Multicastgruppe
			/// </summary>
			/// <param name="vCurrentMulticastAddress">Aktuelle Multicastadresse der Gruppe</param>
			/// <param name="vNewMulticastAddress">Neue Multicastadresse, die die Gruppe erhalten soll</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn kein Fehler)</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
			/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>
			/// <remarks><para>Diese Funktion setzt (verändert) die Multicastadresse einer ganzen Gruppe, die bereits die gleiche Multicastadresse haben. Daraufhin ist die Gruppe nur mehr durch die neue Multicastadresse erreichbar.</para>
			/// Achtung: Slaves, die sich bereits in der Multicastdomäne der neuen Multicastadresse befanden, gehören dann automatisch auch zu dieser Gruppe!
			/// </remarks>
			public bool ChangeMulticastAddress(int vCurrentMulticastAddress, int vNewMulticastAddress, out int oErrorCode)
			{
				#region Parameter validation	
				oErrorCode = 0;
				
				oErrorCode=Check.CheckM3SSlaveAddress(vCurrentMulticastAddress);
				
				if(oErrorCode!=0)
				{
					if(debugMode)
					{
						stdOut.Error(EDOLLHandler.GetErrorDescription(oErrorCode), "CurrentMulticastAddress: " + vCurrentMulticastAddress.ToString());
					}
					return(false);
				}
				
				oErrorCode=Check.CheckM3SSlaveAddress(vNewMulticastAddress);
				
				if(oErrorCode!=0)
				{
					if(debugMode)
					{
						stdOut.Error(EDOLLHandler.GetErrorDescription(oErrorCode), "NewMulticastAddress: " + vNewMulticastAddress.ToString());
					}
					return(false);
				}
				#endregion
				
				byte[] cmdFrame = {(byte)M3SCommand.SetMulticastAddress, (byte)vNewMulticastAddress};
				
				if(debugMode)
				{
					stdOut.Debug("Changing Multicast Address from MulticastGroup (Addr. " +vCurrentMulticastAddress.ToString()+") to " + vNewMulticastAddress.ToString());
				}
				
				return this.SendCommandMulticast(vCurrentMulticastAddress, cmdFrame, out oErrorCode);
				
			}
			
			/// <summary>
			/// Verändert die Multicastadresse einer bestehenden Multicastgruppe
			/// </summary>
			/// <param name="vCurrentMulticastAddress">Aktuelle Multicastadresse der Gruppe</param>
			/// <param name="vNewMulticastAddress">Neue Multicastadresse, die die Gruppe erhalten soll</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
			/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden.</description></item>	
			/// </list>
			/// </returns>
			/// <remarks><para>Diese Funktion setzt (verändert) die Multicastadresse einer ganzen Gruppe, die bereits die gleiche Multicastadresse haben. Daraufhin ist die Gruppe nur mehr durch die neue Multicastadresse erreichbar.</para>
			/// Achtung: Slaves, die sich bereits in der Multicastdomäne der neuen Multicastadresse befanden, gehören dann automatisch auch zu dieser Gruppe!
			/// </remarks>
			public bool ChangeMulticastAddress(int vCurrentMulticastAddress, int vNewMulticastAddress)
			{
				int dummyErrorCode;
				
				return(ChangeMulticastAddress(vCurrentMulticastAddress, vNewMulticastAddress, out dummyErrorCode));
			}		
		#endregion
		
				
		#region predefined commands / often used
		
	
		/// <summary>
		/// Mit Ping wird die Erreichbarkeit eines Slaves überprüft.
		/// </summary>
		/// <param name="pSlaveAddress">Slaveadresse, die gepingt werden soll</param>
		/// <param name="oRTT">Ausgabeparameter: Roundtrip-Time in ms (siehe Remarks)</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Ping erfolgreich durchgeführt.</description></item>
		/// 	<item><term>false</term><description>Slave konnte nicht gepingt werden. Details über den Ausgabeparamter oErrorCode</description></item>	
		/// </list> 
		/// </returns>
		/// <remarks>
		/// <para>Beim Pingen eines Slaves wird ein Kommandoframe bestimmter Länge an diesen versendet. Es soll - im Idealfall - vom Slave acknowledged werden. Die Zeit zwischen Versand des Frames und Antwort wird gemessen und gibt so Auskunft über die Versanddauer von Frames.</para>
		/// <para>Sollte ein Ping nicht erfolgreich sein, kann dies mehrere Möglichkeiten haben;
		/// <list type="bullet">
		/// <item>Master kann Daten nicht versenden</item>
		/// <item>Daten werden nicht / inkorrekt am Bus übertragen</item>
		/// <item>Slave existiert nicht</item>
		/// <item>Slave kann Daten nicht empfangen, verarbeiten oder acknowledgen (Bauteildefekte, Implementierungsfehler,...)</item>
		/// <item>Ein falscher Slave antwortet, doppelte Adressvergabe am Bus,</item>
		/// <item>Das Acknowledge wird fehlerhaft übertragen.</item>
		/// </list>
		/// </para>
		/// <para>
		/// Die Roundtriptime ist jene Zeit, die vom Versenden des Ping-Frames bis zum vollständigen Empfang des Acknowledgeframes vergeht.
		/// </para>
		/// <para>Die hier gemessene Roundtriptime ist NICHT repräsentativ, da der Ping aus der Anwendungsschicht aus gestartet und hier auch wieder beendet wird. Das heißt zur Roundtrip-Time kommt diverser nicht kontrollierbarer Overhead wie Bufferzeiten von Bauteilen, Multitasking, Betriebssystem-Overhead, ... dazu. Auf Realtime-OS ist die RTT repräsentativer, da hier ein vergleichbarer Wert geschaffen werden kann wegen der garantierten Rechenzeiten.</para>
		/// </remarks>
		public bool Ping(int pSlaveAddress, out long oRTT, out int oErrorCode)
		{		
			// generate payload
			oRTT = -1;
			byte[] pingCmd = {(byte)(M3SCommand.Ping)};
						
			
			IM3S_Dataframe pingframe = protocol.CreateFrame(pSlaveAddress, M3SProtocol.Command, mAddr, pingCmd, true, true);
			
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Reset();
			watch.Start();
			
			oErrorCode = this.writeFrame(pingframe);
			
			watch.Stop();			
			oRTT = watch.ElapsedMilliseconds;
			
			
			return(errCode2bool(oErrorCode));			
		}
		
				
		// TODO: THIS METHOD DOES NOT WHAT IT SHOULD
		/// <summary>
		/// Sendet ein allgemeines acknowledge-erforderndes Updatekommando an den Slave (IMPLEMENTATION PROBABLY FAULTY!!!)
		/// </summary>
		/// <param name="pAddr">M3S-Slaveadresse des Kommandoempfängers</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten, gültige Antwort erhlaten</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details sind dem Ausgabeparamter oErrorCode zu entnehmen.</description></item>	
		/// </list>
		/// </returns>
		public bool SendUpdateCommand(int pAddr, out int oErrorCode)
		{	
			SendUpdateCommand(pAddr, out oErrorCode, true);
			return(errCode2bool(oErrorCode));			
		}
		
		public bool SendUpdateCommand(int vSlaveAddress, out int oErrorCode, bool vAcknowledgeRequest)
		{
			if(recBuffer == null)
			{
				oErrorCode = -18;
				Exception e = new NullReferenceException(EDOLLHandler.Verbalize(oErrorCode) + Environment.NewLine + this.InfoString + " (Receive buffer has no instance...)");
				throw(e);                               
			
			}
			
			//recBuffer.Flush(); // sicherheitshalber mal alles weg was zum löschen markiert war..
			IM3S_Dataframe updateCmdFrame = protocol.GetUpdateFrame(vSlaveAddress, vAcknowledgeRequest);				
			oErrorCode = writeFrame(updateCmdFrame);
			
			return(errCode2bool(oErrorCode));			
		}
		
		
		/// <summary>
		/// Liest Informationen eines Slaves und parst diese in ein DevComRemoteSlave-Objekt
		/// </summary>
		/// <param name="vSlaveAddress">Slaveadresse, dessen Informationen gelesen werden sollen</param>
		/// <param name="oSlaveInformation">Ausgabeparameter: Retourniert ein Objekt, das den entfernten Slave repräsentiert und alle wesentlichen Eigenschaften desselben hält</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten, gültige Antwort erhalten und in repräsentatives <see cref="DevComRemoteSlave">DevComRemoteSlave-Objekt</see> geparst.</description></item>
		/// 	<item><term>false</term><description>Daten konnten nicht übermittelt oder Antwort nicht gelesen werden. Details sind dem Ausgabeparameter oErrorCode zu entnehmen</description></item>	
		/// </list>
		/// </returns>
		public bool ReadSlaveInformation(int vSlaveAddress, out DevComRemoteSlave oSlaveInformation, out int oErrorCode)
		{
			oErrorCode=-555; // default  error...
			oSlaveInformation = null;
			if(debugMode)
			{
				stdOut.Debug("Try to get Slave Information from Address " + vSlaveAddress.ToString()+"...");
			}
			
			byte[] cmd = {(byte)(M3SCommand.GetInformation)};
			IM3S_Dataframe infoRequest = protocol.CreateFrame(vSlaveAddress,M3SProtocol.Command,mAddr,cmd,true,true);
			
			byte[] response;
			
			if(this.sendCommandReadAnswer(infoRequest, out response, out oErrorCode))
			{
				DevComRemoteSlave dcrs;
				try
				{
					dcrs = new DevComRemoteSlave(protocol.ExtractPayload(response)); // try to convert and Parse..
				}
				catch(TBL.EDOLL.EDOLLException ex)
				{
					if(debugMode)
					{
						stdOut.Error("Conversion Error when trying to Create DevComRemoteSlave from Bytestream..." + ex.ToString() + Environment.NewLine + TBL.TBLConvert.BytesToHexString(response));
					}
					oErrorCode = ex.ErrorCode;
					oSlaveInformation = null;
					return(false);
				}
				
				oErrorCode=0;
				oSlaveInformation = dcrs;
				return true;
			}
			
			oSlaveInformation = null;
			return(false);
		}
		
		
		/// <summary>
		/// Liest Informationen eines Slaves und parst diese in ein DevComRemoteSlave-Objekt
		/// </summary>
		/// <param name="vSlaveAddress">Slaveadresse, dessen Informationen gelesen werden sollen</param>
		/// <param name="oSlaveData">Ausgabeparameter: Daten, die im Slave abgespeichert sind. Haben die Länge des im Slave spezifizierten DataUpperBound, ist dieser nicht festgelegt (=0), werden 256 Byte Datenbytes übertragen. Wie viel davon gültige Daten sind, ist ohne Zusatzinformation nicht feststellbar. null im Fehlerfall</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Request erfolgreich gesendet, gültige Antwort erhalten und in Ausgabeparameter oSlaveData als Bytestream gespeichert.</description></item>
		/// 	<item><term>false</term><description>Request konnte nicht übermittelt oder Antwort nicht gelesen werden. Details sind dem Ausgabeparameter oErrorCode zu entnehmen</description></item>	
		/// </list>
		/// </returns>
		public bool ReadSlaveData(int vSlaveAddress, out byte[] oSlaveData, out int oErrorCode)
		{
			oErrorCode=-555; // default  error...
			oSlaveData = null;
			
			if(debugMode)
			{
				stdOut.Debug("Try to get Slave DataBytes from Address " + vSlaveAddress.ToString()+"...");
			}
			
			byte[] cmd = {(byte)(M3SCommand.GetData)};
			IM3S_Dataframe infoRequest = protocol.CreateFrame(vSlaveAddress,M3SProtocol.Command,mAddr,cmd,true,true);
			
			byte[] response;
			
			if(this.sendCommandReadAnswer(infoRequest, out response, out oErrorCode))
			{
				oSlaveData = protocol.ExtractPayload(response);
				oErrorCode = 0;
				return true;
			}
			
			oSlaveData = null;
			return(false);
		}
		
		#endregion 
		
		#region Alter Baudrate of Slaves
		
		/// <summary>
		/// Setzt die Baudrate aller Slaves am Bus auf einen übergebenen Wert und versendet anschließend einen Reset mit der neuen Übertragungsgeschwindigkeit
		/// </summary>
		/// <param name="vNewBaudrate">Baudrate, mit der am Bus zukünftig kommuniziert werden soll</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Baudrate im DevComMasterObjekt verändert und Reset mit neuer Baudrate verschickt</description></item>
		/// 	<item><term>false</term><description>Ein Fehler ist aufgetraten (Befehl nicht sendbar, konnte meine eigenen Baudrate nicht verändern, konnte kein Reset senden..) Details liefert der Ausgabeparameter oErrorCode</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>Diese Methode ist erst ab dem M3S-Protokoll 2.0 und höher verfügbar.</para>
		/// <para>Nachdem die Slaves per Commandbroadcast auf eine neue Baudrate gesetzt wurden, ändert das DevComMaster-Objekt innerhalb dieser Methode seine Baudrate auf den Zielwert und versendet ein Reset.</para>
		/// </remarks>
		public bool ChangeBusBaudrate(int vNewBaudrate, out int oErrorCode)
		{
			bool success = setBaudrate(0,vNewBaudrate,true,false, out oErrorCode);
			
			if(success)
			{
				this.Baudrate = vNewBaudrate; // Verändere meine Baudrate				
				this.Reset(out oErrorCode); // sende reset mit neuer geschwindigkeit
			}
			
			return(success);
		}
		
		
		/// <summary>
		/// Setzt die Baudrate eines einzelnen Slaves auf einen neuen Wert.
		/// </summary>
		/// <param name="vSlaveAddress"></param>
		/// <param name="vNewBaudrate">Baudrate, mit der der Zielslave künftig kommunizieren soll (in bps)</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Befehl erfolgreich gesendet, Acknowledge mit alter Baudrate erhalten</description></item>
		/// 	<item><term>false</term><description>Baudrate des Slaves konnte nicht verändert werden. Details liefert der Ausgabeparameter oErrorCode</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>Wichtig: Nach erfolgreicher Abarbeitung dieser Methode operiert das DevComMaster-Objekt selbst noch noch mit der alten Baudrate. Deshalb ist der soeben behandlete Slave vorerst nicht mehr erreichbar (da unterschiedliche Datenraten). Es muss bei Bedarf die Eigenschaft <cref cref="Baudrate">Baudrate</cref> neu gesetzt werden.</remarks>
		public bool SetBaudrate(int vSlaveAddress, int vNewBaudrate, out int oErrorCode)
		{
			return(setBaudrate(vSlaveAddress,vNewBaudrate,false,false, out oErrorCode));
		}
		
		/// <summary>
		/// Setzt die Baudrate eines einzelnen Slaves oder einer Gruppe auf einen neuen WErt
		/// </summary>
		/// <param name="vSlaveOrMulticastAddress">Zieladresse (entweder Slaveadresse oder Multicastadresse - hängt von vMulticast ab)</param>
		/// <param name="vNewBaudrate">Neue Baudrate in bps</param>
		/// <param name="vMulticast">Flag, entscheidet ob der Befehl auf eine Gruppe (true) oder einen einzelnen Slave (false) angewandt werden soll</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Befehl erfolgreich gesendet, Acknowledge mit alter Baudrate erhalten</description></item>
		/// 	<item><term>false</term><description>Baudrate des Slaves konnte nicht verändert werden. Details liefert der Ausgabeparameter oErrorCode</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>Wichtig: Nach erfolgreicher Abarbeitung dieser Methode operiert das DevComMaster-Objekt selbst noch noch mit der alten Baudrate. Deshalb sind der soeben behandlete Slave oder die soeben behandelte Gruppe vorerst nicht mehr erreichbar (da unterschiedliche Datenraten). Es muss bei Bedarf die Eigenschaft <cref cref="Baudrate">Baudrate</cref> neu gesetzt werden.</remarks>
		public bool SetBaudrate(int vSlaveOrMulticastAddress, int vNewBaudrate, bool vMulticast, out int oErrorCode)
		{
			return(setBaudrate(vSlaveOrMulticastAddress,vNewBaudrate,false,vMulticast, out oErrorCode));
		}
		
		/// <summary>
		/// Setzt die Baudrate aller derzeit aktiven Busteilnehmer auf einen neuen Wert (via Broadcast)
		/// </summary>
		/// <param name="vNewBaudrate">Neue Baudrate in bps</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Befehl erfolgreich gesendet</description></item>
		/// 	<item><term>false</term><description>Befehl konnte nicht versendet werden. Details liefert der Ausgabeparameter oErrorCode</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// Diese Methode verändert die Eigenschaft <cref cref="Baudrate">Baudrate</cref> des DevComMaster-Objekts nach erfolgreicher Übertragung auf die neue Baudrate, da sonst keine Busteilnehmer erreichbar wären. Sollte dieses Verhalten nicht gewünscht sein, muss ein expliziter Reset der <cref cref="Baudrate">Baudrate</cref> auf den alten Wert erfolgen.
		/// </remarks>
		public bool SetBaudrate(int vNewBaudrate, out int oErrorCode)
		{
			return(setBaudrate(0,vNewBaudrate,true,false,out oErrorCode));
		}
		
		#endregion
		
		#endregion
		
		#region ConfigData
		/*
		#region Configdata
		public int SendConfigData(int vM3sSlaveAddress, byte[] rSetupData, out int oErrorCode)
		{	
			oErrorCode = 0;			
			int readoutSetupDataLength = -1;
			
			if(this.ReadConfigdataLength(vM3sSlaveAddress, out readoutSetupDataLength) != 0)
			{
				return(EDOLLHandler.LastErrorCode);
			}
			
			if(readoutSetupDataLength != rSetupData.Length) // Wenn ausgelesene Datenlänge nicht für das Paket passt..
			{
				return(-618);
			}
			
			
			byte[] fullCmd = new byte[rSetupData.Length + 1];
			
			fullCmd[0] = (byte)M3SCommand.SetupData;
			
			for(int i = 0; i<rSetupData.Length; i++)
			{
				fullCmd[i+1] = rSetupData[i];
			}
			this.SendCommand(vM3sSlaveAddress,fullCmd, out oErrorCode);
			return(oErrorCode);
		}		
		
		public int ReadConfigdataLength(int vM3sSlaveAddress, out int rDataLength)
		{
			rDataLength = -1; // rückgabewert im fehlerfall
			
			byte[] cmd = new byte[1];
			
			cmd[0] = (byte)M3SCommand.SetupDataLengthRequest;	
			int err;			
			byte[] answer;
			
	
			if(this.SendCommandReadAnswer(protocol.PackCommandIntoFrame(vM3sSlaveAddress, cmd),out answer, out err))
			{
				return(err);
			}
			
			rDataLength = (int)(answer[3]);
			
			if(debugMode)
			{
				stdOut.Debug("ReadSetupData f. Adr. " + vM3sSlaveAddress.ToString() +": gelesene Setupdatenlänge: " + rDataLength.ToString() + "Bytes");
			}
			
			return(0);			
		}
		
		public bool ReadConfigData(int vM3sSlaveAddress, out byte[] rSetupData, out int oErrorCode)
		{
			rSetupData = null; // rückgabewert im Fehlerfall
			oErrorCode = -555;
			
			int setupDataLength;
			
			if(this.ReadConfigdataLength(vM3sSlaveAddress, out setupDataLength) != 0)
			{
				oErrorCode = EDOLLHandler.LastErrorCode;
				return(false);
			}
			
			byte[] cmd = new byte[1];						
			cmd[0] = (byte)M3SCommand.SetupDataReadout;
			
									
			if(this.SendCommandReadAnswer( protocol.PackCommandIntoFrame(vM3sSlaveAddress,cmd), out rSetupData, out oErrorCode))
			{
				return(false);
			}
			
			rSetupData = protocol.ExtractPayload(rSetupData);
			
			return(true);
		}
		#endregion
		*/
		#endregion
		
		#region privates
		
		/// <summary>
		/// Liefert die Anzahld der Pixel eines Slaves, der mit M3S Version 1 operiert.
		/// </summary>
		/// <param name="vAddr">Adresse des Slaves, von dem gelesen werden soll</param>
		/// <param name="oPixels">Anzahl der gelesenen Pixel</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		/// <returns>
		/// Anzahl der Pixel, -1 bei Fehler
		/// </returns>
		private bool m3sV1_readPixels(int vAddr, out int oPixels, out int oErrorCode)
		{
			oErrorCode = -555;
			oPixels = -1;
			return(false); // not implemented, TODO
		}
		
		
		private bool setBaudrate(int vSlaveAddr, int vBaudrate, bool vBroadcast, bool vMulticast, out int oErrorCode)
		{	
						
			IM3S_Dataframe frame = protocol.GetBaudrateChangeFrame(vSlaveAddr,vBaudrate,false,false,out oErrorCode);
			
			if(oErrorCode!= 0)
			{
				throw new EDOLLException(oErrorCode); // something went wrong while frame Building
			}
			
			oErrorCode = this.writeFrame(frame);
			recBuffer.Flush();			
			return(errCode2bool(oErrorCode));
		}
		
		
		private int sendCommandBroadOrMulticast(int vMulticastAddress, byte[] rData, bool vMulticast)
		{
			// TODO: PARAMTER VALIDATION!!! (Standardcommands
			
			
			if(!vMulticast) // wenn broadcast
			{
				vMulticastAddress = 0; // muss übergebene Adresse null sein
			}
			
			return(sendBroadOrMulticast(vMulticastAddress, rData, M3SProtocol.CommandBroadcast, false));
		}
		
		
			
		
		/// <summary>
		/// Sendet einen Broad- oder Multicast (je nach parameter)
		/// </summary>
		/// <param name="vMulticastAddr">Multicastadresse, pass 0 if Broadcast should be sent</param>
		/// <param name="rData">Payload</param>
		/// <param name="vProtocol">M3S-Protocol Number</param>
		/// <param name="vMulticast">Determines wheter Multicast (true) or Broadcast is sent</param>
		/// <returns></returns>
		private int sendBroadOrMulticast(int vMulticastAddr, byte[] rData, M3SProtocol vProtocol, bool vMulticast)
		{
			int tmpErr;
		
			if(vMulticast) // wenn multicast, dann multicastadresse prüfen
			{
				tmpErr	= TBL.Check.CheckM3SSlaveAddress(vMulticastAddr);
				
				if(tmpErr != 0)
				{
					return(tmpErr);
				}	
			}			
			
			protocol.CheckPayloadLength(rData, out tmpErr);
			
			if(tmpErr != 0)
			{
				return(tmpErr);
			}				

			// Eigentliches Senden 			
			
			if(interferenceHandling) // Expliziter Aufruf da ich nix empfangen werde
			{
				recBuffer.HandleGarbage();
			}
		
			// Erzeuge Frame
			IM3S_Dataframe toSend = protocol.CreateFrame(vMulticastAddr,vProtocol, mAddr, rData,true,false); // Als Multicast deklarieren
			
			if(debugMode)
			{
				if(vMulticast)
				{
					stdOut.Debug("Send Multicast ("+rData.Length+" bytes) to Multicast-Address " + vMulticastAddr.ToString() + " via Protocol " + vProtocol.ToString());
				}
				else
				{
					if(vMulticast)
					{
						stdOut.Debug("Send Broadcast ("+rData.Length+" bytes) via Protocol " + vProtocol.ToString());
					}
				}
			}	
			
			return(sendToHardware(toSend.GetDataframe()));
		}
				
		
		private bool errCode2bool(int errorCode)
		{
			if(errorCode == 0)
			{
				return(true);
			}
			else
			{
				return(false);
			}
		}		
		#endregion
				
				
		/// <summary>
		/// Instanziert einen DevCom_Master und bereitet eine Verbindung via übergebenem Interface vor 
		/// </summary>
		/// <param name="pHwIfc">Instanziertes <see cref="IDevComHardwareInterface">devComHardwareInterface</see></param>
		/// <remarks>Protocol defaults to latest Version of M3S-Version (Version 2)</remarks>
		public DevComMaster(IDevComHardwareInterface pHwIfc)
		{			
			construct(pHwIfc, DevComProtocol.M3S_Version2);			
		}
		
		/// <summary>
		/// Konstruktor Method with most arguments. Every Constructor should call this with default values
		/// </summary>
		/// <param name="pHwIfc">Hardwareinterface</param>
		/// <param name="vProtocol">Verwendetes Protokoll</param>
		private void construct(IDevComHardwareInterface pHwIfc, DevComProtocol vProtocol)
		{
			hwInterface = pHwIfc;
			switch(vProtocol)
			{
					case DevComProtocol.M3S_Version1: protocol = new M3S_V1_Handler(); break;
					case DevComProtocol.M3S_Version2: protocol = new M3S_V2_Handler(); break;
					
			}
			
			init_members();
		}
		
		
		/// <summary>
		/// Erzeugt eine neue Instanz der Klasse mit spezifiziertem Hardwareinterface und Protokoll
		/// </summary>
		/// <param name="pHwIfc">Zu verwendendes Hardwareinterface</param>
		/// <param name="vProtocol">Zu verwendendes Protokoll</param>
		public DevComMaster (IDevComHardwareInterface pHwIfc, DevComProtocol vProtocol)
		{
			construct(pHwIfc, vProtocol);
		}
		
		/// <summary>
		/// Aufgrund von Multithreading / Hardwarezugriffe etc. müssen diverse Verbindungen und Threads beendet werden. Diese Methode ist UNBEDINGT bei Programmabbrüchen und beim Verlassen des Programms aufzurufen!
		/// </summary>
		public void Shutdown()
		{
			if(hwInterface != null)
			{
				if(hwInterface.isConnected())
				{
					hwInterface.Disconnect();
				}	
			}	
			available = false;			
		}
		
		/// <summary>
		/// Gibt die aktuellen Verbindungsdaten und Verbindungsstatus der devCom + Interface aus.
		/// </summary>
		/// <returns>Statusstring</returns>
		public string GetInfo()
		{
			return hwInterface.GetInfo() + " receiveBufferDataPtr = " + recBuffer.DataPtr.ToString();
		}
		
		/// <summary>
		/// Creates n random Bytes of Interference and transmits via Hardwareinterface
		/// </summary>
		/// <param name="vInterferenceLength">Anzahl von Störbytes die versendet werden sollen (1...n)</param>
		/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see> (0 wenn fehlerfreie Ausführung)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Störbytes erfolgreich gesendet, Acknowledge erhalten</description></item>
		/// 	<item><term>false</term><description>Störbytes konnten nicht übermittelt werden.</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>Diese Methode sollte nur zu Testzwecken verwendet werden, beispielsweise um die Robustheit eines Bussystems zu überprüfen.</para>
		/// <para>Es empfiehlt sich, vor Auslieferung einer Anlage einen Interference-Test zu fahren, um festzustellen, ob Störbytes (die eine beliebige Störung simulieren) das System trotz Versand von <see cref="Reset(out int)">Resetfolgen</see> zum Absturz bringen können oder nicht. Zusätzlich kann durch Überwachen des Empfangspuffers (siehe  <see cref="ByteReceived">ByteReceived</see>) die Reaktion des Systems auf Störungen untersucht werden.</para>
		/// </remarks>
		public bool CauseInterference(int vInterferenceLength, out int oErrorCode)
		{
			byte[] interference = new byte[vInterferenceLength];
			System.Random rand = new System.Random();
						
			for(int i=0; i<interference.Length; i++)
			{
				interference[i] = (byte)(rand.Next(0,255));
			}
			
			if(debugMode)
			{
				stdOut.Debug("Transmitting Interference: " + TBLConvert.BytesToHexString(interference));
			}
			oErrorCode = hwInterface.WriteData(interference);
			
			return(errCode2bool(oErrorCode));
		}
		
		
		#region Private
		
		private void progress(int pPercentage)
		{
			if(printProgress && (progressOut != null)) // Wenn Ausgabe aktiviert und eine Methode zugewiesen
			{
				progressOut(pPercentage);
			}
		}		
		
		private void onReadTimeOut(object sender, System.Timers.ElapsedEventArgs e)
		{
			watchDog.Enabled = false;		// Duty done...
			readTimeoutReached = true;
		}	
		
		// Schreibt Frame und stellt fest, ob ein Acknowledge benötigt wird oder nicht. Wenn ja, wird auf Acknowledge gewartet, sonst nur versendet		
		private int writeFrame(IM3S_Dataframe vFrame)
		{		
			bool sendSuccesful = false;
			int ptrBefore;			
			int error=0;
			
			
			// TODO: Derive acknowledge from data..
			
			if(available)
			{
				if(interferenceHandling)
				{
					recBuffer.HandleGarbage();
				}	

				if(!vFrame.NeedsAcknowledgement) // Wenn kein Acknowledge gefordert wird
				{
					return(this.sendToHardware(vFrame.GetDataframe()));
				}
				
				// ELSE
							
				for(int i= 0; (i < tryXTimes) && !(sendSuccesful); i++)
				{
					ptrBefore = recBuffer.DataPtr;
					//stdOut.Debug("PtrBefore now (before sending..): " + ptrBefore.ToString()); // Ausgabe zum Bibliotheksdebuggen
					
					if(sendToHardware(vFrame.GetDataframe()) == 0) // wenn erfolgreich gesendet wurde
					{
						
						// stdOut.Debug("Data Sent: " + TBLConvert.BytesToHexString(pData)); // Ausgabe zum Bibliotheksdebuggen						
						
						readTimeoutReached = false;
						watchDog.Enabled = true;					// Starte für Timeout
						
						// hier komm ich her...
						
						while(recBuffer.DataPtr < (ptrBefore+protocol.MinimumFrameLength) && !readTimeoutReached)  // wait until dataptr points to Place BEHIND last Databyte
					      {
					      		// wait
					      }
						
						watchDog.Enabled = false; // watchdog aus...
						
						
						if(!readTimeoutReached)
						{
							byte[] received = recBuffer.readBytes(ptrBefore, ptrBefore + (protocol.AcknowledgeFrameLength -1)); // read Frame that is supposed to be Acknowledge frame..
							
													
							// Check if Acknowledge
							try
							{
								if(protocol.IsAcknowledge(vFrame.GetDataframe(), received, out error))
								{
									sendSuccesful = true;					
									
									recBuffer.FreeBytes(ptrBefore, ptrBefore + received.Length-1);
									recBuffer.Flush();
								}
								else
								{
									if(error == -36)
									{
										return(error); // Explizites NAK erhalten...
									}
									
									sendSuccesful = false;
									int err;
									
									if(!Reset(out err))
									{
										return(err);
									}
									
									// send again, probably log error?!
									// ERROR: not Acknowledge, Log?
								}		
							}		
							catch(Exception e) // Frame error in irgend einer Weise...
							{
								// getLastError hat die FEhlernummer der Exception inna..
								if(debugMode)
								{
									stdOut.Error("Intern TBL.Communication.devCom.writeFrame(byte[]), Acknowledge auswerten: " + Environment.NewLine + EDOLLHandler.GetLastError(), e.ToString());
								}
																
							}
						}
						else
						{
							// Error: readtimeout reached
							if(debugMode)
							{
								stdOut.Debug("Readtimeout reached @ " + watchDog.Interval.ToString() + " ms!! Buffer contains: " + recBuffer.ToString());
							
							}							
						}	
					}					
				}
				
				if(!sendSuccesful)
				{
					if(error == 0)
					{
						error =  -203; // Timeout
					}
					
					return(error); 
				}
				else
				{					
					return(0);
				}			
			}
			else
			{
				// TODO: Fehlernummer "unavailable"
				return(-555); // ruhig weiterlaufen lassen, sonst werden dauernd fehler ausgegeben... Da wird getäuscht!!
			}
				
		}
				
		// Methode für Broadcasts aller Art..
		private int sendToHardware(byte[] pData)
		{
			int save = 0;
			
			if(available)
			{
				save = hwInterface.WriteData(pData);
				
				if(save!=0)
			    {					
			   		available = false;
					throw new Exception("Hardwareinterface could not send Data: " + EDOLLHandler.Verbalize(save));
					
			   		
			    }
			}
			
			return(save);
		}	
				
		public bool Request(int vSlaveAddress, byte[] rRequestCommand, out byte[] oAnswer, out int oErrorCode)
		{
			IM3S_Dataframe frame = protocol.CreateFrame(vSlaveAddress, M3SProtocol.Command,mAddr, rRequestCommand, true, true);
			byte[] answer;

			if(sendCommandReadAnswer(frame, out answer, out oErrorCode))
			{
				oAnswer = protocol.ExtractPayload(answer);
				return(true);
			}
			else
			{
				oAnswer = null;
				return(false);
			}
		}
		
		
		private bool sendCommandReadAnswer(IM3S_Dataframe pCmdFrame, out byte[] oAnswer, out int oErrorCode)
		{
			oErrorCode=-555;
			oAnswer = null;
			bool acknowledged = !pCmdFrame.NeedsAcknowledgement; // wenn kein acknowledge benötigt wird, von vornherein acknowledged..
			
			int ptrBefore = recBuffer.DataPtr;
			
			if(available)
			{
				if(interferenceHandling)
				{
					recBuffer.HandleGarbage();
				}				
							
				for(int i= 0; (i < tryXTimes); i++)
				{
					oErrorCode = sendToHardware(pCmdFrame.GetDataframe());
					
					if( oErrorCode == 0) // wenn erfolgreich gesendet wurde
					{						
						// stdOut.Debug("Data Sent: " + TBLConvert.BytesToHexString(pData)); // Ausgabe zum Bibliotheksdebuggen						
						
						readTimeoutReached = false;
						watchDog.Enabled = true;					// Starte für Timeout
						
						// hier komm ich her...
						
						while(!readTimeoutReached)  // wait until dataptr points to Place BEHIND last Databyte
					      {
							if(recBuffer.DataPtr >= (ptrBefore+protocol.MinimumFrameLength))
							{
								byte[] received = recBuffer.readBytes(ptrBefore, recBuffer.DataPtr-1); // read Frame that is supposed to be Acknowledge frame..
							
								if(received.Length == protocol.AcknowledgeFrameLength)
								{									
									if(protocol.IsAcknowledge(pCmdFrame.GetDataframe(),received, out oErrorCode))
									{
										acknowledged = true;
										watchDog.Enabled = false;
										watchDog.Enabled = true;
										acknowledged = true;
										ptrBefore += protocol.AcknowledgeFrameLength; // offset erhöhen
									}		
									else
									{
										if(oErrorCode == -36)
										{
											// Explizites NAK
											return(false);
										}
									}
								}
								else
								{
									if(protocol.IsFrame(received))
									{																				
										if(acknowledged || protocol.IsImplicitAcknowledge(pCmdFrame.GetDataframe(), received, out oErrorCode))
										{
											oAnswer = received;
											oErrorCode = 0;
											recBuffer.FreeBytes(ptrBefore, recBuffer.DataPtr-1);
											recBuffer.Flush();
											return(true);		// FOUND!!
										}
										else
										{
											if(oErrorCode == -37)
											{
												// Explizites NAKimplizit
												return(false);
											}
										}
									}
								}
							}
					      }
						
						watchDog.Enabled = false; // watchdog aus...
												
						if(readTimeoutReached)
						{		
							if(debugMode)
							{
								stdOut.Debug("Readtimeout reached @ " + watchDog.Interval.ToString() + " ms!! Buffer contains: " + recBuffer.ToString());							
							}	
							
							recBuffer.FreeBytes(ptrBefore, recBuffer.DataPtr-1);
							recBuffer.Flush();	

							Reset(out oErrorCode);
							
							if(oErrorCode != 0)			// RESET ist schiefgegangen
							{
								return(false);
							}							
						}
						else
						{
							throw new Exception("internal sendCommandReadAnswer: This case should not be reached... Please contact Author of Library");																			
						}	
					}
				}
				
				
				
				
				
				
				
				
				
				
				
				
				if(oErrorCode == 0)
				{
					ptrBefore += protocol.AcknowledgeFrameLength;
					
					if(debugMode)
					{
						stdOut.Debug("Command successfully sent, got acknowledged by slave @ internal sendCommandReadAnswer(byte[]) .. waiting for response...");
					}
					
					readTimeoutReached = false;
					watchDog.Enabled = true;
					
					while(!readTimeoutReached)
					{
						int bufferEnd = recBuffer.DataPtr;
						int possFrameStart = ptrBefore;
										
						if((bufferEnd - ptrBefore) >= protocol.MinimumFrameLength)
						{
							//starte suche#
							int possUpperBound;
							int possFrameEnd;
														
							while(possFrameStart + protocol.MinimumFrameLength <= recBuffer.DataPtr)
							{	
								possUpperBound = recBuffer.ReadByte(possFrameStart + protocol.UpperBoundPosition);
								possFrameEnd = possFrameStart+protocol.Overhead + possUpperBound;
								
								byte[] possFrame = recBuffer.readBytes(possFrameStart, possFrameEnd);
								
								if(protocol.IsFrame(possFrame))
								{
									watchDog.Enabled = false;
									
									if(debugMode)
									{									
										stdOut.Debug("Response frame ("+(possFrameEnd-possFrameStart).ToString() +" bytes) found in receive buffer (holding " + recBuffer.DataPtr.ToString() + " bytes); Byte " + possFrameStart.ToString() + " to " +  (possFrameEnd).ToString() + " .. Removing them from ReceiveBuffer (Free and Flush)");
									}
									
									recBuffer.FreeBytes(possFrameStart,possFrameEnd);									
									recBuffer.Flush(); // Buffer leeren
									
									
									oErrorCode =0;
									oAnswer = possFrame;
									return(true);								
								}
								// else
								possFrameStart++; // eines weiter hinten starten
							}							
						}
					}
					
					// Watchdog hat gezogen
					
					readTimeoutReached = false; // rücksetzen					
					oErrorCode=-217; // Watchdog gezogen...
					oAnswer = null;
					return(false);					
				}
				
			}
			else
			{
				oErrorCode=-34;//not available
				oAnswer = null;
			}
			return(false);
		}
				
		 private void init_members()
		{
			available = false;
			watchDog = new System.Timers.Timer(readTimeoutDefaultMs);
			watchDog.Enabled = false;
			watchDog.AutoReset = true;
			watchDog.Elapsed += new System.Timers.ElapsedEventHandler(onReadTimeOut);	
			
			recBuffer = new ThreadsafeReceiveBuffer();
			hwInterface.setReceiveBuffer(recBuffer);			
		}	
		#endregion					
	}
	
	
	
	/// <summary>
	/// Hardwareinterface für serielle Verbindungen (COM-Schnittstellen des PCs)
	/// </summary>
	/// <remarks>
	/// <para><img src="img/monoTested.png" /> Mono-getestet.</para>
	/// <para>
	/// Der Einfachheit halber werden auch heute noch oft serielle Schnittstellen in der Programmierung verwendet. 
	/// Das liegt daran, dass Betriebssysteme auch heute noch standardmäßig COM-Schnittstellen implementiert haben und viele Hersteller Chips anbieten, die virtuelle COM-Ports darstellen. Üblich sind beispielsweise USB / RS232 Chips - Silabs CP2102, USB/RS485 - CP2103 usw.
	/// </para>
	/// <para>
	/// Dieses Interface öffnet auf Basis des Portnames (COMn) eine Verbindung. 
	/// </para>
	/// <para>Mono-implementation modified 11.05.2012: Mono does not support SerialPort.Received-Events, therefore implemented reading-Thread, thus DevComSerialInterface.Disconnect() has to be called before application exit in order to terminate the listening-thread.</para>
	/// </remarks>
	public class devComSerialInterface: IDevComHardwareInterface
	{
		private bool connected;
		private string portname = "";
		private SerialPort sPort = null;
		private ThreadsafeReceiveBuffer recBuff = null;
		private Thread readingThread;
		
		private const int defaultBaudrate = 38400;
		private int baudrate = defaultBaudrate;
		private const StopBits stopbits = StopBits.One;
		private const Parity parity = Parity.None;
		private const int databits = 8;		
		private const int sPortReadtimeout = 2000;		// regulates CPU load, the higher the less CPU load
		
		private bool triggerShutdown = false;
		
		/// <summary>
		/// Quasi-Property
		/// </summary>
		/// <returns>true wenn Port geöffnet, false otherwise</returns>
		public bool isConnected()
		{
			return(connected);
		}
		
		/// <summary>
		/// Baudrate, mit der die Instanz operiert (in bps)
		/// </summary>
		public int Baudrate
		{
			get
			{
				return(sPort.BaudRate);
			}
			set
			{
				sPort.BaudRate = value;
				// reopening required?
			}
			
		}
		
		/// <summary>
		/// Gibt Auskunft über die spezifizierte Defaultbaudrate
		/// </summary>
		public static int DefaultBaudrate
		{
			get
			{
				return defaultBaudrate;
			}
		}
		
		/// <summary>
		/// Liefert Informationsstring über Verbindungsparameter und Verbindungsstatus
		/// </summary>
		/// <returns>Infostring</returns>
		public string GetInfo()
		{
			return("PARAM: " + portname + ", " + baudrate.ToString() + ", " + databits.ToString() + ", " + stopbits.ToString() + ", " + parity.ToString() + " VERBINDUNG: " + connected.ToString());
		}
		
		/// <summary>
		/// Adds eventhandler (Windows, .NET) or starts listening Thread (unix, Mono)
		/// </summary>
		private void startListening()
		{
			// set up listening
			if(TBL.OperatingSystem.IsUnix)
			{
				// Mono Framework does NOT support Events with serial ports... we have to deal with threads...
				readingThread = new System.Threading.Thread(this.portListener);
				readingThread.Priority = ThreadPriority.Highest; // So oft wie möglich aufrufen..
				readingThread.Name = "TBL.Communication.DevComSerialInterface.ReadingThread";
				readingThread.Start();
			}
			else // windows (and therefore .NET) supports events
			{
				sPort.DataReceived += new SerialDataReceivedEventHandler(dataReceivedHandler);
			}
		}
		
		
		/// <summary>
		/// Stellt Verbindung über die serielle Schnittstelle (COM-Port) her
		/// </summary>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Verbindung erfolgreich hergestellt</description></item>
		/// 	<item><term>-13</term><description>Verbindung kann trotz richtiger Parameter nicht hergestellt werden</description></item>	
		/// 	<item><term>-17</term><description>Verbindungsparameter wurden noch nicht gesetzt</description></item>	
		/// 	<item><term>-18</term><description>Empfangspuffer hat null-Zeiger (wurde mit keiner Instanz belegt)</description></item>
		/// 	<item><term>-33</term><description>ComPort existiert nicht</description></item>			
		/// </list>
		/// </returns>
		/// <remarks>
		/// When executing in Linux (via Mono-Framework) this method requires the external stty tool to be installed and accessible from commandline (added in Path).
		/// </remarks>
		public int Connect()
		{			
			if(recBuff == null)
			{
				return(-18);
			}
			
			if(portname != "") // wenn zugewiesen wurde..
			{
				try
				{					
					#region check existence#
					string[] availablePorts = SerialPort.GetPortNames();
					bool portExists = false;
					
					for(int i=0; !portExists && i<availablePorts.Length;i++)
					{
						if(availablePorts[i] == portname)
						{
							portExists=true;
						}
					}
					
					if(!portExists)
					{
						return(-33); // specified Port does not exist on machine
					}
					#endregion 
					
					sPort.BaudRate = baudrate;
					sPort.Open();
					
					// some tricks are needed to make sure also custom baudrates are applied to the comport
					if(OperatingSystem.IsUnix)
					{			
						ProcessStartInfo pStartInfo = new ProcessStartInfo("stty");
						pStartInfo.Arguments = "-F "+portname+" speed " + baudrate.ToString();
						
						// Two times setting the parameter, only one time is not enough... linux behaves strange sometimes..
						Process p = Process.Start(pStartInfo);						
						p.WaitForExit(1000); // wat max. a second						
						Process p2 = Process.Start(pStartInfo);						
						p2.WaitForExit(1000); // wait max. a second
					}			
			
			
					if(sPort.IsOpen)
					{						
						connected = true;
						sPort.DtrEnable = true;  // Die USB-Bridge braucht das...
						this.startListening();
						return(0);
					}
					else
					{ 
						connected = false;
						return(-13);
					}
				}
				catch
				{
					connected = false;
					return(-13);
				}
			}
			
			return(-17);
		}
		
		///<summary>
		/// Schreibt byteArray über die Serielle Schnittstelle (wenn geöffnet)
		/// </summary>
		/// <param name="pData">Zu schreibende Daten</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Daten erfolgreich gesendet</description></item>
		/// 	<item><term>-11</term><description>keine bestehende Verbindung</description></item>		
		/// </list>
		/// </returns>
		public int WriteData(byte[] pData)
		{		
			if(connected)
			{
				sPort.Write(pData, 0, pData.Length);
				return(0);
			}
			else
			{
				return(-11); // Keine Verbindung vorhanden
			}
		}
		
		
		public void portListener()
		{
			byte[] read = new byte[1];
			sPort.ReadTimeout=sPortReadtimeout;
			
			while(!triggerShutdown)
			{
				try
				{
					int tmp = sPort.ReadByte();
					
					if(tmp < 0)
					{
						continue;
					} 
					
					read[0] = (byte)(tmp);
					processReceived(read);
				}
				catch
				{
					// Timeout catched
					// Do nothing, this is normal behaviour
				}
			}
		}
		
		
		/// <summary>
		/// Schließt den Seriellen Port
		/// </summary>
		public void Disconnect()
		{
			this.triggerShutdown = false;
			if(sPort.IsOpen)
			{
				sPort.Close();
			}
		}
		
		/// <summary>
		/// devCom übergibt via setReceiveBuffer() Referenz auf den zu verwendenden Empfangspuffer
		/// </summary>
		/// <param name="rBuff">Instanzierter receiveBuffer</param>
		public void setReceiveBuffer(ThreadsafeReceiveBuffer rBuff)
		{
			recBuff = rBuff;
		}
		
		/// <summary>
		/// Konstruktor für SerialInterface. Instanziert Members und setzt Parameter. Standardbaudrate: 38400
		/// </summary>
		/// <param name="pPortname">Portname (z.B. COM1, /dev/ttyS0, /dev/ttyUSB0)</param>
		/// <remarks>Wird ein ungültiger Portname übergeben, so bleibt dieser Parameter ungesetzt.</remarks>
		public devComSerialInterface(string pPortname)
		{		
			setup(pPortname, 38400);			
		}
		
		
		private void setup(string pPortname, int pBaudrate)
		{
			portname = ""; // invalid value	
			
			if(EDOLLHandler.NoError(TBL.Check.CheckSerialPortName(pPortname)))
			{
				portname = pPortname;
			}
			
			baudrate = pBaudrate;
			
			sPort = new SerialPort(portname, baudrate,parity,databits,stopbits);
			sPort.ReadBufferSize = 2048;
			sPort.Encoding = System.Text.Encoding.ASCII;
			
			
		}
		
		/// <summary>
		/// Konstruktor für das SerialInterrface
		/// </summary>
		/// <param name="pPortname">Portname (z.b. COM1, /dev/ttyS0, ...)</param>
		/// <param name="pBaudrate">Baudrate des Bussystems in bps</param>
		/// <remarks>Wird ein ungültiger Portname übergeben, so bleibt dieser Parameter ungesetzt.</remarks>
		public devComSerialInterface(string pPortname, int pBaudrate)
		{		
			setup(pPortname, pBaudrate);
		}
		
		/// <summary>
		/// Checks received Bytes (not...) and adds to ReceiverBuffer
		/// </summary>
		/// <param name="readBytes"></param>
		private void processReceived(byte[] readBytes)
		{
			recBuff.AddBytes(readBytes);
		}
		
		
		private void dataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
		{			
			int toRead = sPort.BytesToRead;
			byte[] read = new byte[toRead];
			
			sPort.Read(read, 0, toRead); // read buffer to here...
			
			this.processReceived(read);
		}
	}
	 
	/// <summary>
	/// Netzwerkinterface (TcpClient) für <see cref="DevComMaster">DevComMaster</see>	
	/// </summary>
	/// <remarks>
	/// <para><b>Wichtig:</b> Bei bestehender Verbindung läuft in diesem Interface ein eigener Thread zum lesen. Unbedingt <see cref="devComTcpClient.Disconnect()">Disconnect()</see> aufrufen!</para>
	/// //TODO LONG Description!!
	/// </remarks>
	public class devComTcpClient: IDevComHardwareInterface
	{
		// MEMBERVARIABLEN		
		private IPAddress ipAdr;
		private int port;
		private IPEndPoint ipEnPt;
		private bool connected;
		
		private TcpClient client;
		private NetworkStream netStream;
		private int writeTimeOut = 20;
		private int pingTimeout = 2000; // ms
		
		private bool threadstop = false;
		
			
		private ThreadsafeReceiveBuffer recBuff;
	
		// METHODEN
		
		#region Properties
		/// <summary>
		/// devCom übergibt via setReceiveBuffer() Referenz auf den zu verwendenden Empfangspuffer
		/// </summary>
		/// <param name="rRecBuff">Instanzierter receiveBuffer</param>
		public void setReceiveBuffer(ThreadsafeReceiveBuffer rRecBuff)
		{
			recBuff = rRecBuff;
		}		
		
		/// <summary>
		/// (NOT IMPLEMENTED) Bestimmt die Baudrate, mit der am Bus kommuniziert wird (in bps)
		/// </summary>
		/// <remarks>
		/// Diese Eigenschaft ist nicht implementiert, da via IP-Tunnel das Setzen der Baudrate auf jedem Schnittstellengerät unterschiedlich ist. 
		/// </remarks>
		/// <exception cref="TBL.Exceptions.NotImplemented">Not Implemented Exception</exception>
		public int Baudrate
		{
			get
			{
				throw new TBL.Exceptions.NotImplemented("Baudratesetting is not supported by TBL.DevCom via TCP-Client Interface");
			}
			set
			{
				throw new TBL.Exceptions.NotImplemented("Baudratesetting is not supported by TBL.DevCom via TCP-Client Interface");
			}			
		}
		
		/// <summary>
		/// Zeigt an, ob der TCP-Client zum Server verbunden ist oder nicht.
		/// </summary>
		/// <returns></returns>
		public bool isConnected()
		{			
			return(connected);			
		}		
	
		
		#endregion
		private System.Threading.Thread readingThread = null;
		 
		/// <summary>
		/// Stellt Netzwerkverbindung zum TCP-Server her
		/// </summary>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Verbindung erfolgreich hergestellt</description></item>
		/// 	<item><term>-13</term><description>Verbindung kann trotz richtiger Parameter nicht hergestellt werden</description></item>
		/// 	<item><term>-16</term><description>Kommunikationspartner kann nicht gepingt werden</description></item>		
		/// 	<item><term>-17</term><description>Verbindungsparameter wurden noch nicht gesetzt</description></item>	
		/// 	<item><term>-18</term><description>Empfangspuffer hat null-Zeiger (wurde mit keiner Instanz belegt)</description></item>		
		/// </list>
		/// </returns>
		public int Connect()
		{
			if(recBuff == null)
			{
				return(18);
			}
			
			if(mk_ipEnPt()) // if set
			{				
				if(recBuff == null)
				{
					return(-18); // noch kein Empfangspuffer festgelegt
				}				
								
				// Ping
				Ping p = new Ping();
				PingReply pReply = p.Send(ipAdr,pingTimeout);
								
				if(pReply.Status != IPStatus.Success)
				{
					return(-16);							 // Abbruch! Konnte nicht Pingen, Ping-Error
				}				
				
				client = new TcpClient();
				
				try
				{					
					client.Connect(ipAdr, port); 		// Versuche zu verbinden...
				}
				catch
				{
					return(-13); // EDOLLHandler-Code					
				}
				
				if(client.Connected)
				{
					netStream = client.GetStream();
					netStream.WriteTimeout = writeTimeOut;
					connected = true;
					
					// Lesen starten..
					recBuff.Clear(); // auf alle fälle mal leer machen...
					readingThread = new System.Threading.Thread(this.readData);
					readingThread.Priority = ThreadPriority.Highest; // So oft wie möglich aufrufen..
					readingThread.Name = "TBL.Communication.DevComTcpClient.ReadingThread";
					readingThread.Start();
					
					
					return(0);
				}
				else
				{
					return(-13); // Verbindung nicht möglich, EDOLLHandler-Code
				}
			}
			else
			{
				return(-17); // Verbindungsparameter noch nicht gesetzt
			}
		}
		
		/// <summary>
		/// Liefert Informationsstring über Verbindungsparameter und Verbindungsstatus
		/// </summary>
		/// <returns>Infostring</returns>
		public string GetInfo()
		{
			return("IP: " + ipAdr.ToString() + " Port: " + port.ToString() + " Verbindung: " + connected.ToString());
		}
		
		///<summary>
		/// Schreibt byteArray über die Netzwerkkarte (wenn Verbindung zu TCPServer hergestellt)
		/// </summary>
		/// <param name="pData">Zu schreibende Daten</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
		/// 	<item><term>0</term><description>Daten erfolgreich gesendet</description></item>
		/// 	<item><term>-11</term><description>keine bestehende Verbindung</description></item>		
		/// 	<item><term>-14</term><description>writeTimeout @ netstream</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// In früheren Versionen gab es einen Ping vor jedem Datentransfer. DAs verursachte zu viele Fehler, daher wurde das wieder entfernt.
		/// </remarks>
		public int WriteData(byte[] pData)
		{
			if(connected)
			{
				try
				{
					// UNDONE: rausfinden ob überhaupt verbindung aktiv, Ping deaktivieren!
					/*Ping p = new Ping();
					PingReply pReply = p.Send(ipAdr,pingTimeout);
					
					if(pReply.Status == IPStatus.Success)*/
					if(true)
					{
						netStream.Write(pData, 0, (pData.GetUpperBound(0) + 1));
						return(0); // everything went fine
					}
					/*else
					{
						connected = false;		
						return(-16);
					}*/
											
				}
				catch
				{
					return(-14); // no sending possible
				}
			}
			else
			{
				return(-11);		// no connection established	yet
			}
		}				
		
		/// <summary>
		/// Konstruktor, Instanziert Members und setzt Verbindungsparameter.
		/// </summary>
		/// <param name="pStrIp">IP-Adresse des TCP-Servers</param>
		/// <param name="pPort">Port am TCP-Server</param>
		public devComTcpClient(string pStrIp, int pPort) // KONSTRUKTOR
		{
			init_members();	// ACHTUNG: Wichtig das das hier ganz vorn is, sonst überschreibts die Settings die reinübergeben wurden..
			
			if(EDOLLHandler.NoError(Check.CheckIP(pStrIp)))
			{
				ipAdr = IPAddress.Parse(pStrIp);
			}
			
			port = pPort;		
		}
		
		/// <summary>
		/// Verbindung trennen, readingThread stoppen
		/// </summary>
		public void Disconnect()
		{
			threadstop = true;					
			netStream.Close(); // ganz brutal...
		}
				
		private void init_members()
		{
			// invalid values for asking if set
			ipAdr = IPAddress.Parse("0.0.0.0");
			port = 0;
			connected = false;			
		}			
		
		private void readData()
		{		
			while(!threadstop)
			{
				try // weil dann ganz brutal im threadstoprequest der stream unterbrochen wrid..
				{
					byte receiveBufferTmp = Convert.ToByte(netStream.ReadByte());
				
					if(recBuff != null)
					{						
						recBuff.AddByte(receiveBufferTmp);						
					}				
				}
				catch
				{
				}
			}
			
			client = null;
			netStream = null;
			
			threadstop = false;
		}
		
		private bool mk_ipEnPt()
		{
			if(ipAdr.ToString() != "0.0.0.0" && port != 0)
			{
				ipEnPt = new IPEndPoint(ipAdr, port);
			}
			else
				return(false);
			return(true);
				
		}
	}	
		
	
	///<summary>
	/// Anzahl der Aufrufe von ThreadSafeReceiveBuffer.handleGarbage(), bis dass nicht gelöschte Daten automatisch entfernt werden. 
	/// </summary>
	public enum GarbageCollectionSpeed
	{
		///<summary>	
		/// Beim nächsten Aufruf werden die Daten gelöscht
		/// </summary>
		Next = 1,
		/// <summary>
		/// 10 Aufrufe
		/// </summary>
		Ten = 10,
		/// <summary>
		/// 20 Aufrufe
		/// </summary>
		Twenty = 20,
		/// <summary>
		/// 50 Aufrufe
		/// </summary>
		Fifty = 50
	};
	
	
	/// <summary>
	/// Delegatvereinbarung für Evenhandler, die über die Eventliste <see cref="ThreadsafeReceiveBuffer.ByteReceived">ByteAdded</see> getriggert werden.
	/// </summary>
	public delegate void ByteReceivedEventHandler(object sender, EventArgs e);
	
	/// <summary>
	/// Asynchroner flexibler threadsicherer Byteempfangsbuffer (byte In, byte[] out) 
	/// </summary>
	/// <remarks>
	/// <para>
	/// Der Name der Klasse receiveBuffer dürfte die Funktion bereits ziemlich gut umreißen. Es handelt sich um einen Empfangsbuffer, der Byteweise beschrieben wird, und aus dem Blockweise gelesen werden kann.
	/// Er verfügt über eine Löschmethode, die zu löschende Bytes vorerst nur markiert und erst beim Aufruf ThreadSafeReceiveBuffer.Flush() gelöscht werden. Die nachfolgend im Buffer stehenden Bytes rücken dabei vor.
	/// Außerdem ist eine automatische Entleerung inkludiert. Daten, die unbenutzt über n handleGarbage()-Aufrufe im Speicher stehen, werden automatisch gelöscht. 
	/// </para>
	/// <para>
	/// <list type="table">
	/// Features
	/// <listheader><term>Was</term><description>Features</description></listheader>
	/// <item><term>Schreiben</term><description>Byteweise</description></item>
	/// <item><term>Lesen</term><description>Blockweise</description></item>
	/// <item><term>Löschen</term><description>Löschen zu gezielten Zeitpunkten (<see cref="ThreadsafeReceiveBuffer.Flush()">Flush</see>), vorher nur markieren</description></item>
	/// <item><term>Garbage Collection</term><description>Interne Beseitigung nicht gelöschter Datenbytes</description></item>
	/// <item><term>Suchen</term><description>Überprüfung / Suchen von beliebig langen Bytesequenzen</description></item>
	/// <item><term>Errorhandling</term><description><see cref="EDOLL"><see cref="EDOLLHandler">EDOLL-Fehlercode</see>s</see></description></item>
	/// </list>
	/// </para>
	/// <para>
	/// <h3>Technische Daten und ben. Ressourcen</h3>
	/// <list type="table">
	/// <listheader><term>Was</term><description>Beschreibung</description></listheader>
	/// <item><term>Klassenname</term><description>receiveBuffer</description></item>
	/// <item><term>Konstruktoren</term><description>1 (Standardkonstruktor)</description></item>
	/// <item><term>Buffergröße</term><description>2048 Byte fix</description></item>
	/// <item><term>Schreiben</term><description>Byteweise</description></item>
	/// <item><term>Lesen</term><description>Blockweise, beliebige Position</description></item>
	/// <item><term>Garbage-Collection</term><description>Automatische Entleerung: Anzahl der Aufrufe von <see cref="ThreadsafeReceiveBuffer.HandleGarbage()">HandleGarbage()</see> über Property <see cref="ThreadsafeReceiveBuffer.GarbageCollectionSpeed">GarbageCollectionSpeed</see> einstellbar.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// // <list type="table">
	/// <listheader><term>Entfernte Methoden/Properties</term><description>Version</description><description>Grund</description></listheader>
	/// <item><term>Busy</term><description>1.1</description><description>Umstellung von normalem receiveBuffer auf Threadsicheren Receivebuffer => Semaphore können von außerhalb nicht ausgewertet werden, das wäre nicht threadsicher</description></item>
	/// <item><term>Adding</term><description>1.1</description><description>Umstellung von normalem receiveBuffer auf Threadsicheren Receivebuffer => Semaphore können von außerhalb nicht ausgewertet werden, das wäre nicht threadsicher</description></item>
	/// <item><term>Deleting</term><description>1.1</description><description>Umstellung von normalem receiveBuffer auf Threadsicheren Receivebuffer => Semaphore können von außerhalb nicht ausgewertet werden, das wäre nicht threadsicher</description></item>
	/// </list>
	/// </para>
	/// </remarks>	
	public class ThreadsafeReceiveBuffer
	{
		#region Private Members
		private int dptr;
		private byte[] buffer;
		const int defaultBufferSize = 2048; // TODO TO BE UNDONE
		private int garbegeCountStartValue = (int)GarbageCollectionSpeed.Twenty;
		private int garbargeCount = -1;
		private byte[] garbageBuffer;
		private int buffersize;
		
		private ArrayList toDelete;
		
		private Semaphore accessControl; // Semaphor wird für die Zugriffskontrolle verwendet, wird nur ein Zugriff erlaubt...		
		
		/// <summary>
		/// Dieses Element wird immer dann getriggert, sobald ein Byte dem Empfangspuffer hinzugefügt wurde.
		/// </summary>
		public event ByteReceivedEventHandler ByteReceived;
				
		#endregion
		
		#region Properties
				
		
		/// <summary>
		/// DataPtr liefert die Position auf den ersten verfügbaren leeren Speicherplatz im Buffer.
		/// </summary>
		/// <example>10 Bytes im Buffer (Adr. 0-9) .... DataPtr zeigt auf 10</example>
		public int DataPtr						
		{
			get
			{
				return(dptr);
			}
		}		
		/// <summary>
		/// Data Available liefert True, wenn Daten im Buffer stehen, false otherwise
		/// </summary>
		public bool DataAvailable				
		{
			get
			{
				if(dptr > 0)
				{
					return(true);
				}
				else
				{
					return(false);
				}
			}
		}
		/// <summary>
		/// Festlegen der Abfuhrgeschwindigkeit der internen Müllabfuhr.
		/// </summary>
		/// <description>ThreadSafeReceiveBuffer.GarbageCollectionSpeed</description>
		/// <remarks>
		/// Je höher die Geschwindigkeit eingestellt wird, umso eher werden nicht zum Löschen markierte Bytes automatisch gelöscht.
		/// </remarks> 
		public GarbageCollectionSpeed GarbageCollectionSpeed
		{
			get
			{
				return((GarbageCollectionSpeed)(garbegeCountStartValue));
			}
			set
			{
				garbegeCountStartValue = (int)value;
			}
		}
		#endregion	

	 	// TODO: setUndeleteableSequences für garbageCollection anstatt fix einprogrammiert
		
		#region Publics
		  #region Lesezugriffe
	   
	   /// <summary>
	   /// Liest einen Block von inkl. der Bufferposition pFrom bis inkl. Der Bufferposition pTo und gibt diesen als byte-Array zurück.
	   /// </summary>
	   /// <param name="pFrom">Bufferposition, von der aus gelesen werden soll</param>
	   /// <param name="pTo">Bufferposition, bis zu der gelesen werden soll</param>
	   /// <returns>
	   /// <list type="bullet">
	   /// 	<item>Liefert die angeforderten Daten als byte-Array zurück</item>
	   ///    <item>null im Fehlerfall (z.B. wenn pFrom > pTo)</item>
	   /// </list>	   
	   /// </returns>
	   /// <remarks>Der Zugriff erfolgt Threadsafe, dh. wenn gerade ein anderer Thread auf diese Methode zugreift, müssen alle anderen möglicherweise zugreifenden Threads warten...</remarks>
	   /// <example>
	   /// <code>
	   /// receiveBuffer testBuffer = new receiveBuffer();
	   /// 
	   /// if(testBuffer.Dptr > 7)
	   /// {
	   ///   byte[] readBytes = testBuffer.readBytes(0,7); // liest die ersten 7 Bytes
	   ///   // readBytes beispielsweise ausgeben...
	   /// }
	   /// </code>
	   /// </example>
		public byte[] readBytes(int pFrom, int pTo)
		{
			accessControl.WaitOne(); // Sperren des Threads...
			
			if(pFrom > pTo) // Wenns maximal ein einelementiges Array is..
			{				
				accessControl.Release(); // Release, andere können wider zugreifen
				return(null);							
			}
			else
			{
				byte[] toReturn = new byte[pTo - pFrom +1];
				
				for(int i = pFrom; i<=pTo; i++)
				{
					toReturn[i-pFrom] = buffer[i];
				}
				accessControl.Release(); // Release, andere können wider zugreifen
				return(toReturn);
			}
		}
		
		/// <summary>
		/// Liefert ein Byte an einer bestimmten Position im Buffer
		/// </summary>
		/// <param name="vPos"></param>
		/// <exception cref="IndexOutOfRangeException">Index out of range - Exception wenn übergebene Position außerhalb des Buffers liegt</exception>
		/// <returns>Byte an angegebener Position</returns>
		/// <remarks>Threadsicherer Aufruf. Wollen andere Threads auf den Buffer zugreifen, müssen sie warten.</remarks>
		public byte ReadByte(int vPos)
		{
			accessControl.WaitOne(); // Sperren des Threads...
			if(vPos >= dptr)
			{
				accessControl.Release(); // Release, andere können wider zugreifen, es wird nichts gemacht
				IndexOutOfRangeException ex = new IndexOutOfRangeException("@ Function ThreadSafeReceiveBuffer.ReadByte(int): Übergebene Position liegt außerhalb des Buffers. Aktuelle Buffergröße: " + dptr.ToString() + "Bytes");
				throw ex;
			}
			
			accessControl.Release(); // Release, andere können wieder zugreifen
			return(buffer[vPos]);
			
		}
		
		/// <summary>
		/// Liest den gesamten Buffer und gibt ihn als byte-Array zurück.
		/// </summary>
		/// <returns>
		/// <list type="bullet">
		/// <item>byte-Array mit gesamten Bufferinhalt</item>
		/// <item>null im Fehlerfall</item>
		/// </list>
		/// </returns>
		/// <remarks>Threadsichere Implementierung, wollen andere Threads auf den Buffer zugreifen, während diese Methode abgearbeitet wird, müssen sie warten</remarks>
		/// <example>
	   /// <code>
	   /// receiveBuffer testBuffer = new receiveBuffer();
	   /// 
	   /// if(testBuffer.DataAvailable)
	   /// {
	   ///   byte[] readBytes = testBuffer.readBuffer(); // liest die ersten 7 Bytes
	   ///   // readBytes beispielsweise ausgeben...
	   /// }
	   /// </code>
	   /// </example>
		public byte[] readBuffer()
		{			
			if(dptr == 0)
			{
				return(null);
			}
			
			accessControl.WaitOne();
			
			byte[] retBuffer = new byte[dptr]; // only dptr because its one higher than available bytes...
			
			for(int i=0; i<retBuffer.Length; i++)
			{
				retBuffer[i] = buffer[i];
			}
			accessControl.Release();
			
			return(retBuffer);
		} // liest gesamten Buffer
		
		/// <summary>
		/// Gibt Bufferinhalte in zweistellen Hexwerten (also byteweise) als String zurück. Zwischen den einzelnen Bytes ist jeweils ein Leerzeichen
		/// </summary>
		/// <returns>
		/// Bufferinhalt als String (leerer String wenn Buffer leer)
		/// </returns>
		/// <example>
		/// <code>
		/// receiveBuffer testBuffer = new receiveBuffer();
		/// Console.WriteLine(testBuffer.ToString());
		/// </code>
		/// </example>
		public override string ToString()
		{
			string toReturn = "";
			
			for(int i=0; i<dptr; i++)
			{
				toReturn += string.Format("{0:x2} ", buffer[i]);
			}
			
			return(toReturn);
		}
 
		
		
		#endregion
		
		  #region Garbage und Loeschen
		  	/// <summary>
			/// Leert gesamten Buffer
			/// </summary>
			/// <returns>
			/// 0 bei Erfolg (EDOLL-Code)
			/// </returns>
			/// <remarks>Threadsichere Implementierung, wollen gleichzeitig andere Threads auf den Buffer zugreifen, müssen sie warten..</remarks>
			/// <example>
			/// Beispieloperationen mit dem Buffer
			/// <para><img src="tbl_img/receiveBufferExample.jpg" /></para>
			/// <code> 
			/// public static void receiveBufferExample()
			/// {
			/// 	receiveBuffer testBuffer = new receiveBuffer();
			/// 
			///		testBuffer.AddByte((byte)'a');
			///     testBuffer.AddByte((byte)'b');
			///     testBuffer.AddByte((byte)'c');
			/// 
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // printet 3
			/// 	stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
			///  
			///     testBuffer.FreeBytes(0,1); // markiert 'a' und 'b' zum Löschen
			///            
			///     stdOut.Info("a und b marked for erasing");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 3, wurde ja erst markiert
			///                            
			///     testBuffer.Flush();
			///            
			///     stdOut.Info("buffer flushed...");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 1, 'c' wurde ganz nach vorne gerutscht
			///     stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
			/// 
			///     testBuffer.Clear(); // Buffer wieder ausleeren
			///            
			///     stdOut.Info("buffer cleared");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 0
			/// }
			/// </code>
			/// </example>
			public int Clear()
			{
				accessControl.WaitOne();
				buffer = new byte[buffersize];
				dptr = 0;
				accessControl.Release();
				return(dptr);
			}
			
			///<summary>
			/// Prüft und löscht ggf. lange im Buffer stehende Bytes, auf die nie zugegriffen wurde. Verhindert so Bufferüberlauf.
			///</summary>
			/// <remarks>
			/// <para>
			/// Die bufferinterne Müllabfuhr kümmert sich um Bytes, die in den Buffer gelangen, aber nie von außen gelöscht werden. So wird ein Buffer-Overflow durch unbehandelte Datentransfers bzw. physikalische Störungen auf den Leitungen vorgebeugt. 
			/// </para>
			/// <para>
			/// Wie schnell tot im Buffer liegende Daten gelöscht werden, wird über die Property <see cref="ThreadsafeReceiveBuffer.GarbageCollectionSpeed">ThreadSafeReceiveBuffer.GarbageCollectionSpeed</see> eingestellt.
			/// </para>
			/// <para>
			/// Zu beachten ist, dass bei Vorliegen von Datenmüll die Methode <see cref="ThreadsafeReceiveBuffer.Flush()">ThreadSafeReceiveBuffer.Flush()</see> aufgerufen wird. Etwaige schon zum Löschen markierte Datenbytes werden mit dem Flush ebenfalls entfernt.
			/// </para>
			/// </remarks>
			public void HandleGarbage()
			{					
				if(garbargeCount == -1)
				{
					// reinkopieren
					this.copyValuesToGarbageBuffer();		
					garbargeCount = garbegeCountStartValue;								
				}
				
				if(this.BufferContainsGarbageBuffer())
				{
					garbargeCount--;
					
					if(garbargeCount <= 0)
					{
						
						this.FreeBytes(0,garbageBuffer.GetUpperBound(0));
						this.Flush();
						garbargeCount = -1; // das nächste mal reinlesen...
											
					}				
				}
				else
				{
					garbargeCount = -1; // das nächste mal reinlesen...
				}		
				
			}
				
			///<summary>	
			/// Die Flush-Methode arbeitet die intern vorliegende toDelete-Liste ab. Dabei werden alle durch die Methode ThreadSafeReceiveBuffer.FreeBytes() auf die Liste geschriebenen Bufferbereiche gelöscht und dahinterliegende Bytes vorgeschoben. 		
			/// </summary>
			/// <description>ThreadSafeReceiveBuffer.Flush()</description>
			/// <remarks>
			/// Die Methode arbeitet Bereichsweise anhand der vorliegenden toDelete-Liste. Alle zum Zeitpunkt des Programmaufrufs auf der Liste stehenden Speicherbereiche werden gelöscht. 
			/// Es wird mit dem ersten Speicherbereich begonnen, dieser wird gelöscht und alle dahinter im Buffer liegenden Bytes werden vorgerückt, der interne Datenpointer hinter das letzte im Buffer stehende Byte gesetzt.
			/// Danach wird mit dem nächsten auf der Liste stehenden Bereich fortgefahren usw.
			/// Nach Abarbeitung der Methode ist der Buffer wieder kompakt von Dptr-Adresse 0 aufwärts und der Dptr zeigt auf den ersten freien Speicherplatz.
			/// </remarks>
			/// <example>
			/// Beispieloperationen mit dem Buffer
			/// <para><img src="tbl_img/receiveBufferExample.jpg" /></para>
			/// <code> 
			/// public static void receiveBufferExample()
			/// {
			/// 	receiveBuffer testBuffer = new receiveBuffer();
			/// 
			///		testBuffer.AddByte((byte)'a');
			///     testBuffer.AddByte((byte)'b');
			///     testBuffer.AddByte((byte)'c');
			/// 
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // printet 3
			/// 	stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
			///  
			///     testBuffer.FreeBytes(0,1); // markiert 'a' und 'b' zum Löschen
			///            
			///     stdOut.Info("a und b marked for erasing");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 3, wurde ja erst markiert
			///                            
			///     testBuffer.Flush();
			///            
			///     stdOut.Info("buffer flushed...");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 1, 'c' wurde ganz nach vorne gerutscht
			///     stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
			/// 
			///     testBuffer.Clear(); // Buffer wieder ausleeren
			///            
			///     stdOut.Info("buffer cleared");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 0
			/// }
			/// </code>
			/// </example>
			public void Flush()
			{			

				if(toDelete.Count <= 0) // nichts zu tun
				{
					return;
				}		
				
				accessControl.WaitOne();				
								
				foreach(fromToPair pair in toDelete)
				{	
										
					int pFrom = pair.From;
					int pTo = pair.To;
					// cnt => so viele bytes habe ich...
					
					// difference..
					int i;
					int dptrBef = dptr;					
					int pastTo = pTo - pFrom;						// wie viele bytes muss ich verrücken...			
					
					// Clear the bytes
					for(i=pFrom; i <= pastTo; i++)
					{
						buffer[i] = 0;									// default value
					}
					
										
					// Vorrücken im Speicher...
					for(i=0; i <= pastTo; i++)
					{
						for(int j = pFrom + pastTo - i; j<dptr-1; j++)
						{
							buffer[j] = buffer[j+1];
						}				
					}
					
					
					// Alle Pairs die dahinter waren vorrücken...
					
					foreach(fromToPair toShift in toDelete)
					{
						if(toShift != pair)
						{
							if(toShift.From >= pTo)
							{
								toShift.From -= (pTo-pFrom)+1;
								toShift.To -= (pTo-pFrom)+1;
							}
						}
					}			
					
					dptr = dptrBef - pastTo -1;		 // point to next byte, i is 1 higher than last index of the buffer(for abgebrochen)...
					
					
					
					
					for(int cnt = dptr; cnt<=dptrBef; cnt++)
					{
						buffer[cnt] = 0;
					}				
				}				
						
				toDelete.Clear();
				accessControl.Release(); // uuund weitergehts für andere Threads
				
			} 
			
			/// <summary>
			/// Markiert einen Speicherbereich zum Löschen. Löscht Sie allerdings nicht!!! Dazu muss Flush() aufgerufen werden
			/// </summary>
			/// <param name="pFrom">From Position</param>
			/// <param name="pTo">To Position</param>
			/// <returns>
			/// <list type="bullet">
			/// <item>0 bei bei fehlerfreier Abarbeitung (<see cref="EDOLLHandler">EDOLL-Code</see>)</item>
			/// <item>-207 wenn pFrom größer als pTo (<see cref="EDOLLHandler">EDOLL-Code</see>)</item>
			/// <item>-208 wenn pTo über den beschriebenen Speicher hinauszeigt (<see cref="EDOLLHandler">EDOLL-Code</see>)</item>
			/// </list>
			/// </returns>
			/// <example>
			/// Beispieloperationen mit dem Buffer
			/// <para><img src="tbl_img/receiveBufferExample.jpg" /></para>
			/// <code> 
			/// public static void receiveBufferExample()
			/// {
			/// 	receiveBuffer testBuffer = new receiveBuffer();
			/// 
			///		testBuffer.AddByte((byte)'a');
			///     testBuffer.AddByte((byte)'b');
			///     testBuffer.AddByte((byte)'c');
			/// 
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // printet 3
			/// 	stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
			///  
			///     testBuffer.FreeBytes(0,1); // markiert 'a' und 'b' zum Löschen
			///            
			///     stdOut.Info("a und b marked for erasing");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 3, wurde ja erst markiert
			///                            
			///     testBuffer.Flush();
			///            
			///     stdOut.Info("buffer flushed...");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 1, 'c' wurde ganz nach vorne gerutscht
			///     stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
			/// 
			///     testBuffer.Clear(); // Buffer wieder ausleeren
			///            
			///     stdOut.Info("buffer cleared");
			///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 0
			/// }
			/// </code>
			/// </example>
			public int FreeBytes(int pFrom, int pTo)
			{
				// Syntaxprüfung
				if(pFrom > pTo)
				{
					return(-207);
				}
				
				if(pTo >= dptr)
				{
					return(-208);
				}
				
				accessControl.WaitOne();
				
				fromToPair tmp = new fromToPair();
				
				tmp.From = pFrom;
				tmp.To = pTo;
				
				toDelete.Add(tmp);
				accessControl.Release();
				
				return(0);
			} 
			#endregion
			
		///<summary>
		/// Sucht nach der übergebenen Sequenz pSequence. Liefert den internen Datenpointer auf das erste Element des ersten Auftretens der Sequenz im Buffer
		/// </summary>
		/// <param name="pSequence">Byte-Sequenz nach der gesucht werden soll.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item>Interner Datenpointer auf das erste Element des ersten Auftretens der übergebenen Sequenz</item>
		/// <item>-1 bei Misserfolg</item>
		/// </list>		
		/// </returns>
		public int containsSequence(byte[] pSequence)
		{
			// Läuft den Buffer durch und schaut obs eine Sequenz enthält			
			int chkCnt = 0;		
			int firstByte = -1;
			
			for(int i=0; i<dptr;i++)
			{
				if(buffer[i] == pSequence[chkCnt])
				{
					if(chkCnt == 0)
					{
						firstByte = i;
					}
					chkCnt++;
				}
				else
				{
					// RESET					
					if(chkCnt != 0) // wenn schon mal ein Framecheck angefangen wurde
					{
						i--; // das Abbruchbyte nochmal prüfen, ist ja vl. start vom frame
						firstByte = -1;
						chkCnt = 0;
					}									
				}
				
				if(chkCnt > pSequence.GetUpperBound(0))
				{
					return(firstByte);					
				}
			}
			return(-1);
		}		
		
		///<summary>
		/// Fügt ein Byte hinten an den Buffer an (an die Stelle, auf die der Datenpointer vorher zeigte)
		/// </summary>
		/// <param name="pByte">Anzuhängendes Byte</param>
		/// <returns>
		/// <list type="bullet">
		/// <item>0 bei Erfolg (<see cref="EDOLLHandler">EDOLL-Code</see>)</item>
		/// <item>-201 wenn Buffer voll (Byte wurde nicht geschrieben) (<see cref="EDOLLHandler">EDOLL-Code</see>)</item>
		/// </list>
		/// </returns>
		/// <example>
		/// Beispieloperationen mit dem Buffer
		/// <para><img src="tbl_img/receiveBufferExample.jpg" /></para>
		/// <code> 
		/// public static void receiveBufferExample()
		/// {
		/// 	receiveBuffer testBuffer = new receiveBuffer();
		/// 
		///		testBuffer.AddByte((byte)'a');
		///     testBuffer.AddByte((byte)'b');
		///     testBuffer.AddByte((byte)'c');
		/// 
		///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // printet 3
		/// 	stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
		///  
		///     testBuffer.FreeBytes(0,1); // markiert 'a' und 'b' zum Löschen
		///            
		///     stdOut.Info("a und b marked for erasing");
		///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 3, wurde ja erst markiert
		///                            
		///     testBuffer.Flush();
		///            
		///     stdOut.Info("buffer flushed...");
		///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 1, 'c' wurde ganz nach vorne gerutscht
		///     stdOut.Debug("Buffer content: " + testBuffer.ToString());// Zeigt Hexwerte im Buffer an
		/// 
		///     testBuffer.Clear(); // Buffer wieder ausleeren
		///            
		///     stdOut.Info("buffer cleared");
		///     stdOut.Debug("Dataptr now: " + testBuffer.DataPtr.ToString());     // liefert 0
		/// }
		/// </code>
		/// </example>>
		// TODO: EXCEPTION dokumentieren
		public int AddByte(byte pByte)
		{		
			int tmpError;
			
			accessControl.WaitOne();
			tmpError = addByteInternal(pByte);
						
			if(tmpError == 0)
			{
				accessControl.Release();
				if(ByteReceived != null)
				{
					ByteReceived(this, EventArgs.Empty); // Call Events (if there are any)
				}
				
				return(0); // EDOLL no Error
			}					
			else
			{ 
				
				EDOLLHandler.Error(tmpError); // Buffer overflow...				
				InternalBufferOverflowException ex = new InternalBufferOverflowException(EDOLLHandler.GetLastError() + "Datapointer: " + this.dptr.ToString());
				accessControl.Release();
				throw ex;
			}			
		} 
		
		private int availableSema;
		
		/// <summary>
		/// Fügt dem Empfangspuffer Datenbytes hinzu
		/// </summary>
		/// <param name="rBytes">Datenbytes, die angefügt werden sollen</param>
		/// <exception cref="InternalBufferOverflowException">InternalBufferOverflowException, wird geworfen, wenn Buffer überläuft</exception>
		public void AddBytes(byte[] rBytes)
		{
			accessControl.WaitOne();
			for(int i=0; i<rBytes.Length; i++) // Der reihe nach hinzufügen
			{				
				if(addByteInternal(rBytes[i]) != 0) // sollte beispielsweise der Buffer überlaufen, Exception raus, eigtl. kann hier eh nur ein bufferoverflow ausschlaggebend sein
				{
					InternalBufferOverflowException ex = new InternalBufferOverflowException(EDOLLHandler.GetLastError() + "Datapointer: " + this.dptr.ToString());
					accessControl.Release();
					throw ex;	
				}
			}
			// wenn ich bis hierher gekommen bin, gabs keinen Fehler...
			
			availableSema = accessControl.Release();
			
			if(ByteReceived != null)
			{
				ByteReceived(this, EventArgs.Empty); // Fire Events (if there are any)
			}
			
		}
		
		/// <summary>
		/// Gibt die Standardbuffergröße an
		/// </summary>
		public int DefaultBufferSize
		{
			get
			{
				return(defaultBufferSize);
			}
		}
		
		private int addByteInternal(byte pByte)
		{
			if(dptr < buffersize)
			{					
					lock(this)
					{
						buffer[dptr] = pByte;
						dptr++;
						
					}
					return(0); // no error..
			}
			else
			{
				// buffer full...
				return(-201);
			}
		}
		
		/// <summary>
		/// Standardkonstruktor, Initialisiert alle wichtigen internen Komponenten mit <see cref="DefaultBufferSize">DefaultBufferSize</see>
		/// </summary>
		public ThreadsafeReceiveBuffer()
		{
			buffersize = defaultBufferSize;
			commonInit();
		}
		
		
		/// <summary>
		/// Initialisiert alle wichtigen internen Komponenten und erzeugt einen Buffer mit übergebener Größe
		/// </summary>
		/// <param name="vBufferSize">Buffergröße in Byte</param>
		public ThreadsafeReceiveBuffer(int vBufferSize)
		{
			if(vBufferSize > 0)
			{
				buffersize = vBufferSize;
			}
			else
			{
				buffersize = defaultBufferSize;
			}
			
			commonInit(); // Call common initialisationmethod
		}
		
		
		
		#endregion

		#region Private Funktionen
		
			/// <summary>
			/// Initialisiert diverse Members
			/// </summary>
			private void commonInit()
			{
				dptr = 0;
				buffer = new byte[buffersize];
				garbageBuffer = new byte[buffersize];
				toDelete = new ArrayList();	
	
				// Multithreading Part: Only one Thread at a Time is allowed to access...			
				accessControl = new Semaphore(0,1); 	
				accessControl.Release();
			}
		
			// TODO Dokumentieren
			private void copyValuesToGarbageBuffer()
			{
				if(dptr > 0)
				{
					garbageBuffer = new byte[dptr];
					
					for(int cnt = 0; cnt<dptr; cnt++)
					{
						garbageBuffer[cnt] = buffer[cnt];
					}
				}
				else
				{
					garbageBuffer = null;
				}			
			}	
			// TODO Dokumentieren			
			private bool BufferContainsGarbageBuffer()
			{
				bool isEqual = true;
				
				if(garbageBuffer != null)
				{
					for(int cnt = 0; cnt<=garbageBuffer.GetUpperBound(0); cnt++)
					{
						if(garbageBuffer[cnt] != buffer[cnt])
						{
							isEqual &= false;
						}			
					}
				}
				else
				{
					isEqual = false;
				}							
				
				return(isEqual);
			}
		#endregion		
	}
	
	// Für toDelete-Liste
	internal class fromToPair
	{
		internal int From;
		internal int To;
	}	
	
	
	
	
	
	/// <summary>
	/// Repräsentiert einen entfernten Slave, Informationen und Daten werden über devCom.GetSlaveInformation() via Datenübertragung ermittelt.
	/// </summary>
	public class DevComRemoteSlave
	{		
		private int version;
		private int subversion;
		private char revision;
		private DevComType type=DevComType.Unknown;
		private short id;
		
		private int address;
		private int mcaddress;
		private int curDataUpperBound = -1;
				
		private byte[] bytestream;
		
		/// <summary>
		/// Konstruktor, muss eine gültige Bytefolge der Slaveinformationen übergeben bekommen!
		/// </summary>
		/// <param name="vReceivedBytestream">Empfangener Bytestream (Payload eines Datenframes)</param>
		/// <remarks><para>Der empfangene Bytestreams muss bestimmte Konditionen erfüllen um geparst werden zu können. Siehe dazu auch <see cref="DevComSlaveInfoIndex">DevComSlaveInfoIndex</see> hinsichtlich definierter Informationsbytes am Anfang des Streams</para>
		/// </remarks>
		public DevComRemoteSlave(byte[] vReceivedBytestream)
		{
			if(vReceivedBytestream == null)
			{
				TBL.Exceptions.ObjectNull ex = new TBL.Exceptions.ObjectNull("in constructor of DevComRemoteSlave");
				throw(ex);
			}
			
			bytestream = vReceivedBytestream;
			
			switch((int)vReceivedBytestream[(int)DevComSlaveInfoIndex.Version])
			{
					case 2: parseVersion2Info(vReceivedBytestream); break;
					default: throw new Exception("Unknown Version number (Index " + ((int)DevComSlaveInfoIndex.Version).ToString() + ") passed vReceivedBytestream at DevComRemoteSlaveConstructor");
			}
		}
		
		
				
		#region Equals and GetHashCode implementation
		/// <summary>
		/// Vergleicht zwei DevComSlaveInformation-Objekte mittels deren Bytestream (so wie sie vom Slave übermittelt werden).
		/// </summary>
		/// <param name="obj">Objekt, mit dem das aktuelle verglichen werden soll</param>
		/// <returns>
		/// true bei Gleichheit, false bei Ungleichheit
		/// </returns>
		public override bool Equals(object obj)
		{			
			DevComRemoteSlave other = obj as DevComRemoteSlave;
			if (other == null)
				return false;
			
			for(int i=0; i<bytestream.Length; i++)
			{
				if(this.bytestream[i] != other.bytestream[i])
				{
					return(false);
				}
			}
			
			return(true);			
		}
		
		/// <summary>
		/// Vergleicht zwei DevComRemoteSlave-Objekte hinsichtlich Gleichheit (Referenz oder Wertgleichheit)
		/// </summary>
		/// <param name="lhs">Linker Operand</param>
		/// <param name="rhs">Rechter Operand</param>
		/// <returns>true bei Gleichheit, false bei Ungleichheit</returns>
		public static bool operator ==(DevComRemoteSlave lhs, DevComRemoteSlave rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}
		
		/// <summary>
		/// Ungleichheitsoperator
		/// </summary>
		/// <param name="lhs">Linker Operand</param>
		/// <param name="rhs">Recher Operand</param>
		/// <returns>true wenn ungleich, false wenn gleich</returns>
		public static bool operator !=(DevComRemoteSlave lhs, DevComRemoteSlave rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// Erzeugt Hashcode aus diversen Members
		/// </summary>
		/// <returns>Hashcode</returns>
		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				hashCode += 1000000007 * version.GetHashCode();
				hashCode += 1000000009 * subversion.GetHashCode();
				hashCode += 1000000021 * revision.GetHashCode();
				hashCode += 1000000033 * type.GetHashCode();
				hashCode += 1000000087 * address.GetHashCode();
				hashCode += 1000000093 * mcaddress.GetHashCode();
				
				if (bytestream != null)
					hashCode += 1000000297 * bytestream.GetHashCode();
			}
			return hashCode;
		}
		
		#endregion

		/// <summary>
		/// Fügt alle relevanten Informationen in einen String (mit Zeilenumbrüchen) zusammen.
		/// </summary>
		/// <returns>Slaveinformationen</returns>
		public override string ToString()
		{
			string toReturn = "";
			string separator = Environment.NewLine;
			
			toReturn += type.ToString() + " Version " + this.Version + separator;
			toReturn +=" Implementierung: " + type.ToString();
			toReturn +=" Address:" + address.ToString() + separator;
			toReturn +=" Multicast-Address:" + mcaddress.ToString() + separator;
			toReturn +=" Data Upper Bound (n-1): " + curDataUpperBound.ToString() + separator;
			toReturn +=" Device ID: " + id.ToString() + separator;
			
			
			return(toReturn);
		}
		
		#region Parser
			#region Subversion Switching
				private void parseVersion2Info(byte[] vReceivedByteStream)
			{
				version = (int)vReceivedByteStream[(int)DevComSlaveInfoIndex.Version];
				switch(vReceivedByteStream[(int)DevComSlaveInfoIndex.Subversion])
				{
					case 0: parseVersion2_0(vReceivedByteStream); break;
					default: throw new Exception("Unknown Subversion number (Index " + DevComSlaveInfoIndex.Subversion.ToString() + ") in passed vReceivedBytestream at DevComRemoteSlaveConstructor");
				}
				                          
			}
			#endregion
			
			#region TypeSwitching
				private void parseVersion2_0(byte[] vReceivedByteStream)
			{
				subversion=(int)vReceivedByteStream[(int)DevComSlaveInfoIndex.Subversion];
				revision = (char)vReceivedByteStream[(int)DevComSlaveInfoIndex.Revision];
				
				try
				{
					type = (DevComType)vReceivedByteStream[(int)DevComSlaveInfoIndex.Type];
				}
				catch
				{
					throw new TBL.Exceptions.ConversionException("Unable to convert data byte that is supposed to be Type-Information to known DevComType. (Index " + DevComSlaveInfoIndex.Type.ToString() + " in passed Information).");
				}
				
				address = (int)vReceivedByteStream[(int)DevComSlaveInfoIndex.FirstDataByte+0];
				mcaddress = (int)vReceivedByteStream[(int)DevComSlaveInfoIndex.FirstDataByte+1];
				id = (short)((vReceivedByteStream[(int)DevComSlaveInfoIndex.FirstDataByte+2] << 8) | (vReceivedByteStream[(int)DevComSlaveInfoIndex.FirstDataByte+3]));
				curDataUpperBound = (int)vReceivedByteStream[(int)DevComSlaveInfoIndex.FirstDataByte+4];
				
				
			}
			#endregion
			
		#endregion
		
		#region Properties
		
		/// <summary>
		/// Versionsstring (Meist Version.SubversionRevision, z.b. V1.2a)
		/// </summary>
		public String Version
		{
			get
			{
				return(version.ToString() + "." + subversion.ToString() + revision.ToString());
			}
		}
		
		/// <summary>
		/// Upperbound (höchster Arrayindex) der aktuellen Daten
		/// </summary>
		public int DataUpperBound
		{
			get
			{
				return(curDataUpperBound);
			}
			
		}
		
		/// <summary>
		/// Slave-Addresse
		/// </summary>
		public int Address
		{
			get
			{
				return address;
			}
		}
		
		/// <summary>
		/// Aktuelle Multicast-Adresse
		/// </summary>
		public int MulticastAddress
		{
			get
			{
				return mcaddress;
			}
		}
		
		#endregion
	}
	
}	