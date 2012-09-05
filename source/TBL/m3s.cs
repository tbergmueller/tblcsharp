/*
 * Created by SharpDevelop.
 * User: Tom
 * Date: 13.11.2010
 * Time: 21:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using TBL.EDOLL;

namespace TBL.Communication.Protocol
{
			
		#region Enums
		/// <summary>
		/// Protokollcodes gemäß M3S-Spezifikation
		/// </summary>
		/// <description>M3SProtocol</description>
		public enum M3SProtocol
		{	
			/// <summary>
			/// Ungültiges Protokoll
			/// </summary>
			/// <description>M3SProtocol.Invalid</description>
			Invalid = 0x00,
			///<summary>
			/// Befehl zum Reset
			/// </summary>
			Reset = 0x01,
			/// <summary>
			/// Datentransfer an alle Geräte 
			/// </summary>
			BroadCast = 0x02,
			/// <summary>
			/// Regulärer Datentransfer an bestimmte Adresse
			/// </summary>
			DataTransfer = 0x03,
			/// <summary>
			/// Kommandotransfer an Gerät mit bestimmter Adresse
			/// </summary>
			Command = 0x04,
			/// <summary>
			/// Antwort auf Kommando von Gerät mit bestimmter Adresse
			/// </summary>
			CommandResponse = 0x05, 
			/// <summary>
			/// Acknowledgefolge von Slave mit bestimmter Adresse an Master 
			/// </summary>
			Acknowledge = 0x08,
			/// <summary>
			/// Kommandotransfer an alle Geräte
			/// </summary>
			CommandBroadcast = 0x09,
			/// <summary>
			/// Dateitransfer an Gerät mit bestimmter Adresse
			/// </summary>
			FileTransfer = 0x0A,
			/// <summary>
			/// Slave fordert von sich aus Aktionen
			/// </summary>
			SlaveRequest = 0x0D,
		}
		
		/// <summary>
		/// Protokollcodes gemäß M3S-Spezifikation in Kurzschreibweise (Internal only)
		/// </summary>
		/// <description>M3SProtocol</description>
		internal enum M3SProtocolAcrynonyms
		{	
			/// <summary>
			/// Ungültiges Protokoll
			/// </summary>
			/// <description>M3SProtocol.Invalid</description>
			IVD = 0x00,
			///<summary>
			/// Befehl zum Reset
			/// </summary>
			RES = 0x01,
			/// <summary>
			/// Datentransfer an alle Geräte 
			/// </summary>
			DBC = 0x02,
			/// <summary>
			/// Regulärer Datentransfer an bestimmte Adresse
			/// </summary>
			DAT = 0x03,
			/// <summary>
			/// Kommandotransfer an Gerät mit bestimmter Adresse
			/// </summary>
			CMD = 0x04,
			/// <summary>
			/// Antwort auf Kommando von Gerät mit bestimmter Adresse
			/// </summary>
			RSP = 0x05, 
			/// <summary>
			/// Acknowledgefolge von Slave mit bestimmter Adresse an Master 
			/// </summary>
			ACK = 0x08,
			/// <summary>
			/// Kommandotransfer an alle Geräte
			/// </summary>
			CBC = 0x09,
			/// <summary>
			/// Dateitransfer an Gerät mit bestimmter Adresse
			/// </summary>
			FIT = 0x0A,
			/// <summary>
			/// Slave fordert von sich aus Aktionen
			/// </summary>
			SLR = 0x0D,
		}
				
		/// <summary>
		/// Im M3S vereinbarter Befehlssatz
		/// </summary>
		public enum M3SCommand
		{
			/// <summary>
			/// Sendet eine Pingkommando mit Payload. Siehe <see cref="DevComMaster.Ping(int, int, out long, out int)">DevComMaster.Ping</see>
			/// </summary>
			Ping='P',			
			/// <summary>
			/// Fragt bei Slaves (Leuchtmittel) RGB-Daten und Helligkeitsdaten an
			/// </summary>
			ColorAndBrightnessRequest = 'C',
			/// <summary>
			/// Fragt Filetransfer an, als Parameter im gleichen Frame max. 255 Bytes Dateiname (<see cref="IM3S_Handler.GetFileRequestFrame(int, string)">GetFileRequest(int, string)</see>)
			/// </summary>
			FileRequest = 'f',			
			/// <summary>
			/// Kündigt einen bevorstehenden Dateitransfer an, hat als Parameter max 251 Bytes Dateiname und Anzahl der nachfolgenden Datenpakete (siehe <see cref="IM3S_Handler.GetFileTransferAnnouncementFrame">m3sHandler.GetFileAnnouncementFrame</see>)"
			/// </summary>
			FileAnnouncement = 'F',
			/// <summary>
			/// allgemeiner Updatebefehl
			/// </summary>			
			Update = 'U',
			/// <summary>
			/// Skalieren der Daten, nachfolgendes Byte Werte zwischen 0..100 [%]
			/// </summary>
			Scale = 0x0D,
			/// <summary>
			/// Sendeaufforderung der Pixelzahlen
			/// </summary>
			DataUnitAmountRequest = 0x0A,
			/// <summary>
			/// Bus-Reset, devComs aller Teilnehmer am bus werden zurückgesetzt.
			/// </summary>
			Reset = 'R',
			/// <summary>
			/// Liest die Informationen des devCom-Slaves (Version, Type und serialized devCom-Slave Object werden retourniert)
			/// </summary>
			GetInformation= 'i',
			/// <summary>
			/// Setzt die Multicast-Adresse neu, entsprechend nachfolgendem Parameter
			/// </summary>
			SetMulticastAddress= 'M',	
			/// <summary>
			/// Verändert die Baudrate entsprechend nachfolgender Parameter
			/// </summary>
			ChangeBaudrate='B',			
			/// <summary>
			/// Request zum Auslesen der Daten.
			/// </summary>
			GetData='d',
			/// <summary>
			/// Writes something on TWI-Bus. Payload Structure (bytewise): Addr, Byte0, Byte1, ...
			/// </summary>
			TWIWrite= 'T',
			/// <summary>
			/// Read some Bytes from TWI-Bus (with or without initial Command)
			/// </summary>
			/// <remarks>
			/// This command is a little more complex than the TWIWrite one. It's payload is structured as following:
			/// <list type="bullet">
			/// <item>1 Byte TWI-SlaveAddress</item>
			/// <item>1 Byte CommandLength (n)</item>
			/// <item>1 Byte Expected SlaveData-Length (m)</item>
			/// <item>n Commandbytes</item>
			/// </list>
			/// In Response, a Frame is sent with the Slaves-TWI-Address as First byte and followup m SlaveDataBytes in it's payload. If the first byte of the Payload is the bitwise complement of the TWI-Slave's Address, an Error occured. The following two bytes then represent the error code. 
			/// </remarks>
			TWIRead= 't',
			
		}
		
		/// <summary>
		/// Fehlercodes von auszuführenden Kommandos (geben Auskunft warum die Ausführung scheiterte)
		/// </summary>
		/// <description>M3SCommandError</description>
		/// <remarks>
		/// Note: Diese Fehlercodes werden um den Offset 1000 verschoben und können mit diesen Nummern in der <see cref="EDOLL">EDOLL</see> als negative Werte eingegeben werden.
		/// </remarks>
		/// <example>
		/// <para>Kommunikationspartner sendet Fehlercode 4 (File not Readable)</para>
		/// <para>Empfangsroutine macht daraus -1004</para>
		/// <para>In EDOLL-ConfigFile sollte irgendwo ein Eintrag für -1004 zu finden sein</para>
		/// </example>
		public enum m3sExecutionError
		{
			/// <summary>
			/// Erfolgreich ausgeführt
			/// </summary>
			NoError = 0,
			/// <summary>
			/// Netzwerkkomponente ist momentan nicht gestartet oder es kann nicht zugegriffen werden
			/// </summary>
			ServiceNotRunning = 1,
			/// <summary>
			/// Kalenderkomponente ist momentan nicht gestartet oder es kann nicht zugegriffen werden
			/// </summary>
			CalendarNotRunning = 2,
			/// <summary>
			/// Zu lesende Datei existiert nicht. 
			/// </summary>
			FileNotExisting = 3,
			/// <summary>
			/// zu lesende Datei kann nicht geöffnet werden.
			/// </summary>
			FileNotReadable = 4,
		}
		#endregion
				
		/// <summary>
		/// Mit dieser Klasse lassen sich zu sendende Daten und Parameter in korrekte Frames (nach M3S-Konvention) verpacken.
		/// </summary>
		/// <remarks>
		/// <h2>M3S-Datenrahmen</h2>
		/// <para>Über den Konstruktor müssen alle zum Datentransfer benötigten Parameter übergeben werden. Mit der Methode <see cref="IM3S_Dataframe.GetDataframe">getDataFrame()</see> holt man sich dann das gepackte und versandfertige Datenpaket zurück.</para>
		/// <para>Ein Datenrahmen besteht – zusätzlich zu den Nutzdaten – aus einigen weiteren Bytes, die den Datentransfer regeln, kontrollieren und zur Überprüfung dienen. Wie den Spezifikationen zu entnehmen, ist das M3S-Protokoll für die Übertragung größerer Datenmengen geeignet, bei kleinen Datenmengen würden verhältnismäßig große Mengen an Rahmendaten anfallen.</para>  
		/// <para>Jeder Datenrahmen wird von einem Controlbyte begonnen. Er beinhaltet Informationen zur Interpretation der nachfolgenden Daten sowie einige Steuerbits. (Acknowledge, Masteradresse, Datenrichtung, siehe Punkt Controlbytes)</para>
		/// <para>Nachfolgend wird die Slaveadresse gesendet. Da es sich um einen bidirektionalen Bus handelt, kann der Slave sowohl Sender als auch Empfänger sein. Egal ob der Slave gerade sendet oder empfängt, das zweite Byte eines Rahmens ist immer die Adresse des am Transfer beteiligten Slaves.</para>
		/// <para>Das dritte Byte gibt die Anzahl der Daten an – und zwar als Anzahl Datenbytes weniger 1. Das erspart in c-Notation bei Arrays die ständige Dekrementierung um eins um auf den höchsten Index des Datenarrays zu kommen. Aus diesem Grund wird Das Byte 3 auch nachfolgend als upperBound = höchster Arrayindex (vgl. .NET Framework), bezeichnet.</para>
		/// <para>Nun folgen die Nutzdaten, die auf eine maximale Anzahl von 256 Bytes (wegen Wertebereich 1 Byte upperBound) begrenzt sind.</para>
		/// <para>Über den gesamten bis jetzt gesendeten Datenrahmen wird im Sender eine 3-Byte-Prüfsumme gebildet. Diese wird mit 0x544F4D exklusiv Oder (XOR) verknüpft und im Anschluss an die Nutzdaten übertragen.  </para>
		/// <para>Detaillierte Informationen und grafische Abbildung siehe M3S-Dokumentation in PDF-Form</para>
		/// </remarks>
		/// <example>Beispielverwendung von IM3S_Dataframe (Packen und Rückextrahieren von Daten)
		/// <code>
		/// int slaveAddr = 2;                                // Empfängeradresse
		///	byte[] toSend = {0x12, 0x34, 0x56};               // Dummy-Daten für Beispiel
		///	int masterAddr = 1;								  // sendender Master
		///	bool masterSend = true;							  // Datenrichtung: MasterSend / !SlaveSend
		///	bool isAcknowledgeframe = false;				  // Wollen wir ein Acknowledgeframe erzeugen?
		///	M3SProtocol protocol = M3SProtocol.DataTransfer;  // Welches Protokoll soll verwendet werden?
		///	
		///	IM3S_Dataframe frame = this.CreateFrame(slaveAddr, protocol, masterAddr, toSend, masterSend, isAcknowledgeframe);
		///	
		///	Console.WriteLine("gepackte Daten: " + frame.ToString()); 	// Ausgabe zum Debuggen			
		///	byte[] packedFrame = frame.GetDataframe();					// Gepacktes versandfertiges Frame erzeugen
		///	
		///	// Beispielweise Extraktion der einzelnen PArameter mit dem m3sHandler
		///	m3sHandler protocolHandler = new m3sHandler();			
		///	Console.WriteLine("Masteradresse im Paket: " + protocolHandler.ExtractMasterAddress(packedFrame).ToString());     // liefert 1
		///	Console.WriteLine("Protokoll des Pakets: " + protocolHandler.ExtractProtocol(packedFrame).ToString());            // liefert 'DataTransfer'
		/// </code>
		/// </example>
		public class M3S_V1_Dataframe: M3S_Dataframe, IM3S_Dataframe
		{			
			private static readonly int ackFrameLen = 5;			
			private static readonly int overheadLength = 6;
			
			// Einstellungen
			private static char[] chkSumCode = {'T', 'O', 'M'};		// 3 Bytes für 3 Bytes checksum!!
			
			private static M3S_V1_Handler handler = new M3S_V1_Handler();
						
			
			private int checksum_code;		
			
			private int chkSum;		
			
			
			public int Version
			{
				get
				{
					return(1);
				}
			}
			
			#region Properties
			/// <summary>
			/// Länge des Frames (Nutz- und Rahmendaten) in Byte
			/// </summary>
			public int Length
			{
				get
				{
					return(overheadLength+databytes.Length);
				}
			}
			
			
			/// <summary>
			/// Maximal mögliche Framelänge (Payload + Overhead)
			/// </summary>
			public static int MaximumFrameLength
			{
				get
				{
					return(overheadLength + maxPayloadLength);
				}
			}
			
			/// <summary>
			/// Gibt an, ob das Frame vom Slave quittiert werden muss oder nicht.
			/// </summary>
			public bool NeedsAcknowledgement
			{
				get
				{
					if(protocol == M3SProtocol.DataTransfer || protocol==M3SProtocol.FileTransfer ||protocol==M3SProtocol.Command)
					{
						return(send); // Nur in MS-Send Richtung 
					}
					else
					{
						return(false);
					}
				}
			}
			
			/// <value>
			/// Gibt die minimale Framelänge im M3S-Protokoll an (5 Bytes, Acknowledgeframes)
			/// </value>
			
			public M3SProtocol Protocol
			{
				get
				{
					return(protocol);
				}
			}
			
			/// <summary>
			/// M3S-Protokollnummer
			/// </summary>
			public int ProtocolNr
			{
				get
				{
					return((int)protocol);
				}
			}
			
			/// <summary>
			/// Länge des Overheads in Byte
			/// </summary>
			public static int Overhead
			{
				get
				{
					return(overheadLength);
				}
			}
			
			/// <summary>
			/// Minimal mögliche Framelänge (i.d.R <see cref="AcknowledgeFrameLength">AcknowledgeFrameLength</see>)
			/// </summary>
			public static int MinimumFrameLength
			{
				get
				{
					return(ackFrameLen);
				}
			}
			
			
			/// <summary>
			/// Länge eines Acknowledgeframes
			/// </summary>
			public static int AcknowledgeFrameLength
			{
				get
				{
					return(ackFrameLen);
				}
			}
			
			
			/// <summary>
			/// Liefert den 3-Byte-Prüfsummencode, der zum Verschlüsseln verwendet wird, in einem Integer zurück. (0x00xxxx auf 32 bit Maschinen)
			/// </summary>
			public static int CheckSumCode
			{
				get
				{
					return(Convert.ToInt32(chkSumCode[0] <<16) | Convert.ToInt32(chkSumCode[1] << 8) | Convert.ToInt32(chkSumCode[2]));
				}						
			}	

			
			public int SlaveAddress
			{
				get
				{
					return(addr);
				}
			}
			
			public int MulticastAddress
			{
				get
				{
					return(addr);
				}
			}
			
			#endregion
			
			#region Equals and GetHashCode implementation
			/// <summary>
			/// Prüft zwei <see cref="IM3S_Dataframe">M3S-Dataframes</see> auf Gleichheit
			/// </summary>
			/// <param name="toCompare">Zu vergleichendes Objekt (byte[] oder Objekt vom Typ <see cref="IM3S_Dataframe">m3sDataFrame</see></param>
			/// <returns>
			/// true / false</returns>
			/// <remarks>
			/// Vergleicht die beiden Datenframes durch ihre resultierenden Streams (Rahmen + Nutzdaten)
			/// </remarks>
			public override bool Equals(object toCompare)
			{				
				byte[] stream;
				
				try
				{
					// is it a M3s-Dataframe?
					IM3S_Dataframe tmpFrame = toCompare as IM3S_Dataframe;
					stream = tmpFrame.GetDataframe();
				}  
				catch
				{
					// was no IM3S_Dataframe
					try
					{
						stream = toCompare as byte[];
					}
					catch
					{
						TBL.Exceptions.ConversionException e = new TBL.Exceptions.ConversionException("IM3S_Dataframe.CompareTo hat ein Objekt erhalten, das weder ein IM3S_Dataframe noch ein byte-Array ist");
						throw e;
					}
				}	

				byte[] thisStream = this.GetDataframe();
				// wenn hier, hat die Konvertierung hingehaut
				
				if(stream.Length != thisStream.Length)
				{
					return(false);
				}
				
				for(int i=0; i<stream.Length; i++)
				{
					if(stream[i] != thisStream[i])
					{
						return(false);
					}
				}
				
				return(true);
			}
			
			/// <summary>
			/// Bildet einen Hashcode auf Basis verschiedener Members
			/// </summary>
			/// <returns>Hashcode</returns>
			public override int GetHashCode()
			{
				int hashCode = 0;
				unchecked {
					hashCode += 1000000007 * addr.GetHashCode();
					hashCode += 1000000009 * protocol.GetHashCode();
					if (databytes != null)
						hashCode += 1000000021 * databytes.GetHashCode();
					hashCode += 1000000033 * mAddr.GetHashCode();
					hashCode += 1000000087 * acknowledgeOrAckRequest.GetHashCode();
					hashCode += 1000000093 * send.GetHashCode();
					hashCode += 1000000097 * checksum_code.GetHashCode();
					hashCode += 1000000103 * chkSum.GetHashCode();
					hashCode += 1000000123 * ctrlByte.GetHashCode();
				}
				return hashCode;
			}
			
			/// <summary>
			/// Prüft zwei Datenframes auf Gleichheit (Referenz + Daten)
			/// </summary>
			/// <param name="lhs">Linker Operand</param>
			/// <param name="rhs">Rechter Operand</param>
			/// <returns>true wenn gleich, false wenn ungleich</returns>
			public static bool operator ==(M3S_V1_Dataframe lhs, M3S_V1_Dataframe rhs)
			{
				if (ReferenceEquals(lhs, rhs))
					return true;
				if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
					return false;
				return lhs.Equals(rhs);
			}
			
			/// <summary>
			/// Prüft zwei Datenframes auf Ungleichheit (Referenz + Daten)
			/// </summary>
			/// <param name="lhs">Linker Operand</param>
			/// <param name="rhs">Rechter Operand</param>
			/// <returns>true wenn ungleich, false wenn gleich</returns>
			public static bool operator !=(M3S_V1_Dataframe lhs, M3S_V1_Dataframe rhs)
			{
				return !(lhs == rhs);
			}
			#endregion

			/// <summary>
			/// Gibt einen Informationsstring zurück, der alle relevanten Parameter für das aktuelle Frames beinhaltet
			/// </summary>
			/// <returns>Kompakte Informationen über das Datenframe</returns>
			/// <remarks>
			/// Informationen werden protokollspezifisch zurückgegeben, entweder Hexdump oder interpretierte Daten 
			/// </remarks>
			public string GetInterpretation()
			{
				switch(protocol)
				{
						case M3SProtocol.Reset: return(this.ToString());
						case M3SProtocol.Command: return(this.infoStringCommand());
						case M3SProtocol.CommandBroadcast: return(this.infoStringCommand());
						case M3SProtocol.CommandResponse: return(this.infoStringCommandAnswer());
						case M3SProtocol.Acknowledge: if(acknowledgeOrAckRequest) return("[ACK]"); else return("[!!! NOT ACK !!!]");
						case M3SProtocol.DataTransfer: return("[HEXDUMP=" + hexdumpDatabytes(0) + "]");
						case M3SProtocol.BroadCast: return("[HEXDUMP=" + hexdumpDatabytes(0) + "]");
						                                      
						default: return(this.ToString());
				}
			}
			
			#region ProtocolSpecificInfoStrings
			private string infoStringCommand()
			{
				string toReturn;
				
				try
				{
					// Interpret command
					M3SCommand cmdInterpreted = (M3SCommand)(databytes[0]);
					toReturn = "[cmd=" + cmdInterpreted.ToString() + "] ";
				}
				catch
				{
					// unknown command
					toReturn = "[cmd=" + string.Format("{02:x}", databytes[0]) + "] ";
				}
				
				if(databytes.Length > 1)
				{
					toReturn += "[param = " + decimaldumpDatabytes(1) +"] ";
					
				}
				
				return(toReturn);
			}
			
			private string infoStringCommandAnswer()
			{
				if(databytes != null)
				{
					if(databytes.Length > 0)
					{
						return("[RESP] [param= " + decimaldumpDatabytes(0) + "]");
					}
				}
				return("[param= none]");
				
			}
			
			private string hexdumpDatabytes(int offset)
			{
				
				string toReturn = "";
				
				for(int i = offset; i<databytes.Length; i++)
				{
					toReturn += string.Format("{00:x2} ",databytes[i]);
				}
				
				return(toReturn);
			}
			
			private string decimaldumpDatabytes(int offset)
			{
				string toReturn = "";
				
				for(int i = offset; i<databytes.Length; i++)
				{
					toReturn += databytes[i].ToString() + " ";
				}
				
				return(toReturn);
			}
			
			
			#endregion
			
			
			
			/// <summary>
			/// Adresse des am Transfer beteiligten Master
			/// </summary>
			public int MasterAddress
			{
				get
				{
					return(mAddr);
				}
			}
			
			/// <summary>
			/// Identität des Senders (M + Nr für Master, Nr + SL für Slaves)
			/// </summary>
			public string SenderIdentity
			{
				get
				{
					bool mastersend = send;					
					
					if(protocol == M3SProtocol.Acknowledge) // ursprüngliche Datenrichtung beibehalten
					{
						mastersend = !send; // hier genau das gegenteil
					}
					
					if(mastersend)
					{
						return(string.Format("M{0:0}", mAddr));
					}
					else
					{
						return(string.Format("{0:000} (SL)", addr));
					}					
				}
			}
			
			/// <summary>
			/// Identität des Empfängers (M + Nr für Master, Nr + SL für Slaves, MC + Nr für Multicasts, 'all' für Broadcasts)
			/// </summary>
			public string TargetIdentity
			{
				get
				{
					bool mastersend = send;
					
					if(protocol == M3SProtocol.Acknowledge)
					{
						mastersend = !send;
					}
					
					if(mastersend)
					{
						if(protocol == M3SProtocol.Reset)
						{
							return("all");
						}
						
						if((protocol == M3SProtocol.BroadCast || protocol == M3SProtocol.CommandBroadcast))
						{
							if(acknowledgeOrAckRequest)
							{
								// Multicast
								return(string.Format("{0:000} (MC)", addr));
							}
							else
							{
								return("all");
							}
						}

						return(string.Format("{0:000} (SL)", addr));						
					}
					else
					{
						return(string.Format("M{0:0}", mAddr));
					}
				}
			}
			
			///<summary>
			/// Prüft ob es sich beim übergebenen Frame um ein gültiges M3S-Frame handelt und validiert die Rahmendaten
			/// </summary>
			/// <param name="rToCheck">Zu überprüfendes Frame</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Es handelt sich um ein gültiges Frame</description></item>
			/// 	<item><term>false</term><description>Frame ist ungültig. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>		
			/// <remarks>
			/// Die Funktion überprüft das M3S-Datenframe auf folgende Punkte (in dieser Reihenfolge, beim ersten Fehler wird abgebrochen)
			/// <list type="number">
			/// <item>Frame kürzer als Minimale M3S-Framelänge</item>
			/// <item>Checksum-Error (Fehlercode -27)</item>
			/// <item>upperBound stimmt mit Framelänge nicht überein (Fehlercode -28)</item>
			/// <item>Aufgrund von Framelänge als Acknowledgeframe identifiziertes Frame verwendet kein Acknowledgeprotokoll (Fehlercode -31)</item>
			/// <item>Protokollnr ist ungültig oder konnte nicht extrahiert werden (Fehlercode -29)</item>
			/// <item>Protokoll ist Broadcast / Resetframe, führt aber eine SlaveAdresse ungleich 0 (Fehlercode -30)</item>
			/// <item>Slaveadresse ist ungültig - außerhalb [1 255] (Fehlercode -602)</item>
			/// </list>
			/// </remarks>
			public static bool IsFrame(byte[] rToCheck, out int oErrorCode)
			{
				oErrorCode = -555; // general Error
				
				if(rToCheck.Length < ackFrameLen)
				{
					oErrorCode = -32; // Frame zu kurz.. Dieser FEhler sollte an und für sich nur bei Analysen auftreten
				}
				
				bool chkSumOk = IsFrame(rToCheck);
				
				if(!chkSumOk)
				{
					oErrorCode = -27; // Prüfsummenfehler M3S
					return(false);
				}
				// else
				
				if(rToCheck.Length == ackFrameLen)
				{
					// Wenn Acknowledgeframe
					if(handler.ExtractProtocol(rToCheck) == M3SProtocol.Acknowledge)
					{
						oErrorCode = 0; 
/*REGULAR EXIT*/   		return(true);
					}
					else
					{
						oErrorCode = -31;
						return(false);
					}
					
				}
				else
				{
					// Prüfen ob anderes Frame
					if(((int)(rToCheck[2])+1+ 6) != rToCheck.Length)
					{
						oErrorCode = -28; // upperBound stimmt nicht mit Framelänge überein
						return(false);
					}
					
					
					#region Protokoll und Slaveadressprüfung
					// ###### WEICHE: Broadcasts und normal
					M3SProtocol usedM3SProtocol = handler.ExtractProtocol(rToCheck); // extrahiert Protokoll aus dem Controlbyte
					
					if(usedM3SProtocol == M3SProtocol.Invalid)
					{
						// Irgendwas ging schief, return
						oErrorCode = -29; // ungültige Protokollnummer
						return(false);
					}
					
					// Protokoll konnte konvertiert werden
					
					// Überprüfen der Slaveadresse, bei Broadcasts usw. muss sie 0 sein, sonst gültig
					
					if(usedM3SProtocol == M3SProtocol.Reset  || usedM3SProtocol == M3SProtocol.BroadCast  || usedM3SProtocol == M3SProtocol.CommandBroadcast)
					{
						if(rToCheck[1] != 0)
						{
							if((usedM3SProtocol == M3SProtocol.BroadCast) || (usedM3SProtocol == M3SProtocol.CommandBroadcast)) // Letzte Chance: Multicast im Broadcastprotokoll
							{
								if((rToCheck[0] & 0x02) != 0x02) // nein, auch kein Multicast, Pech gehabt...
								{
									oErrorCode = -30; // Broadcasts erfordern Addresse 0
									return(false);
								}
							}
							else
							{
								oErrorCode = -30; // Broadcasts erfordern Addresse 0
								return(false);
							}
							
						}
					}
					else
					{
						// Andere Protokolle brauchen gültige Slaveadresse
						oErrorCode = TBL.Check.CheckM3SSlaveAddress((int)(rToCheck[1]));
					
						if(oErrorCode != 0) // wenn M3s-SlaveAddresse nicht gepasst hat...
						{
							return(false);
						}
					}
				
					#endregion			
									
				}
								
				oErrorCode = 0; // no error
				return(true);
			}
			
			
			/// <summary>
			/// Prüft ob es sich beim übergebenen Frame um ein gültiges M3S-Frame handelt
			/// </summary>
			/// <param name="toCheck">Zu prüfendes Frame</param>
			/// <returns>true / false</returns>
			/// <remarks>Die Methode prüft die Gültigkeit nach M3S des Bytestreams durch Bilden der Prüfsumme und anschließendem Vergleich mit der aus dem Frame extrahierten und decodierten Prüfsumme. Die Sinnhaftigkeit von Controlbyte, upperBound und Slaveadresse wird dabei <b>nicht</b> geprüft</remarks>
			public static bool IsFrame(byte[] toCheck)
			{
				if(toCheck == null)
				{
					return(false);
				}
				
				
				int checksum = 0;
				int laenge = toCheck.GetUpperBound(0)+1;
										
				checksum = 0;
				for(int i=0; i<(laenge-3); i++)
				{
					checksum += Convert.ToInt32(toCheck[i]);
				}			
				
				int chkSumRec = Convert.ToInt32(toCheck[toCheck.GetUpperBound(0)-2] <<16) | Convert.ToInt32(toCheck[toCheck.GetUpperBound(0)-1] << 8) | Convert.ToInt32(toCheck[toCheck.GetUpperBound(0)]); // received
								
				chkSumRec ^= M3S_V1_Dataframe.CheckSumCode;
				
				
				if(chkSumRec == checksum)
				{
					return(true);
				}
				else
				{
					return(false); // Checksum error
				}				
			}
			
			
			
			/// <summary>
			/// Prüft, ob es sich um ein Acknowledgeframe für das übergebene gesendete Frame handelt und wenn ja, ob es Acknowledge oder Not Acknowledge bedeutet
			/// </summary>
			/// <param name="rSent">Gesendetes Frame, das acknowledged werden soll</param>
			/// <param name="rRec">Vermutetes Acknowledgeframe (muss <see cref="AcknowledgeFrameLength">AcknowledgeFrameLength</see> haben)</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
			/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>		
			/// <exception cref="Exceptions.FrameError">Ungültiges Datensatzformat, übergebenes rRec-Frame hat nicht die Länge eines Acknowledgeframes.</exception>
			/// <remarks>Rücgabewert False und oErrorCode==-36 indizieren ein expliztes Not-Acknowledge</remarks>			
			public static bool IsAcknowledge(byte[] rSent, byte[] rRec, out int oErrorCode)
			{
				Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Beim Überprüfen des Acknowledgeframes ist ein Fehler aufgetreten. Frame: " + TBLConvert.BytesToHexString(rRec));
				
				if(rRec.Length != AcknowledgeFrameLength)
				{
					throw ex;
				}			
				
				int chkSum = Convert.ToInt32(rRec[0] + rRec[1]); // calc
				int chkSumRec = Convert.ToInt32(rRec[2] <<16) | Convert.ToInt32(rRec[3] << 8) | Convert.ToInt32(rRec[4]); // received
				chkSumRec ^= CheckSumCode;			
				
				if(chkSum != chkSumRec)
				{
					oErrorCode = -35;
					return(false);			// Prüfsummenfehler
				}
				
				// Ok, Frame ist gültig... check obs das richtige Acknowledge is..
				if(rSent[1] != rRec[1])
				{
					oErrorCode = (-21); // Paket kommt  von anderem Slave...
					return(false);
				}
				
				if((rRec[0] & 0xF0) != ((Convert.ToInt32(M3SProtocol.Acknowledge) << 4) & 0xF0))
				{
					oErrorCode = (-22); // Es handelt sich um kein Acknowledgeframe...
					return(false);
				}				
				
				
				if(((rRec[0] | 0x02) & 0x0F) != ((rSent[0] | 0x02) & 0x0F)) // Masteradresse, Datenrichtung und Acknowledgebit stimmt nicht...
				{
					oErrorCode = -23;
					return(false);
				}
				
				if((rRec[0] & 0x02) == 0x02) // Wenn Acknowledgebit gesetzt
				{
					oErrorCode = 0;
					return(true);	
				}
				else
				{
					oErrorCode = -36; // NAK
					return(false);
				}					
				
			}
			
			/// <summary>
			/// Konstruktor: Alle zu verpackenden Parameter werden übergeben, können später nicht mehr geändert werden.
			/// </summary>			/// 
			/// <param name="pAddr">Slaveadresse (0 bei Broadcasts)</param>
			/// <param name="pProtocol"><see cref="M3SProtocol">M3SProtocol</see></param>
			/// <param name="pMAddr">Masteradresse (1..3), 0 bei Broadcasts und Reset</param>
			/// <param name="pData">max. 256 sendende Datenbytes als byte[]</param>
			/// <param name="pSend">Senden?
			/// <list type="bullet">
			/// <item>true ... Master send, Slave Receive</item>
			/// <item>false... Slave send / Master receive</item>
			/// </list>
			/// </param>
			/// <param name="vAcknowledge">true wenn Acknowledge oder AcknowledgeRequest signalisiert werden soll (nur bei Unicastprotokollen), false bei Not-Acknowledge, Not-AcknowledgeRequest und allen anderen Frames</param>
			/// <example>Das Beispiel zeigt das Verpacken Parametern in versandfertige Daten.
			/// <code>
			/// //TODO: Code Example
			/// </code>
			/// </example>
			public M3S_V1_Dataframe(int pAddr, M3SProtocol pProtocol, int pMAddr, byte[] pData, bool pSend, bool vAcknowledge)
			{
				addr = Convert.ToByte(pAddr);	
				databytes = pData;	
				send = pSend;
				
				if(pProtocol == M3SProtocol.Acknowledge || pProtocol == M3SProtocol.CommandBroadcast || pProtocol == M3SProtocol.BroadCast)
				{
					acknowledgeOrAckRequest = vAcknowledge;
				}
				else
				{
					acknowledgeOrAckRequest = false;
				}
				protocol = pProtocol;
				mAddr = pMAddr;
				
				// init...
				checksum_code = Convert.ToInt32(chkSumCode[0] <<16) | Convert.ToInt32(chkSumCode[1] << 8) | Convert.ToInt32(chkSumCode[2]);							
			}				
			
						
			/// <summary>
			/// Liefert versandfertiges Frame als Bytestream zurück.
			/// </summary>	
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Ausführung</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>byte-Array</term><description>Versandfertiges Datenframe als Bytefolge</description></item>
			/// 	<item><term>null</term><description>Im Fehlerfall. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>
			/// <remarks>
			/// Detaillierte Informationen siehe M3S-Doku in PDF-Form bzw. <see cref="IM3S_Dataframe">Konstruktor IM3S_Dataframe()</see> und <see cref="IM3S_Handler">Interface IM3S_Handler</see>
			/// </remarks> 
			/// <example>
			/// <para>Erzeugen des versandfertigen byte[]-Arrays</para>
			/// <para><c>byte[] packedFrame = frame.GetDataframe(out errorCode);	// Gepacktes versandfertiges Frame erzeugen</c></para>			
			/// </example>
			public byte[] GetDataframe(out int oErrorCode)
			{
				int i;
				int shift;		
				byte[] dataframe;
				
				if(protocol == M3SProtocol.Acknowledge) // Das einzige Frame ohne Datenbytes ist folgendes:
				{
					dataframe = new Byte[AcknowledgeFrameLength];
				}
				else
				{
					if(databytes.GetUpperBound(0) > 255)
					{
						oErrorCode = -20; // zu viele Nutzdatenbytes
						return(null);
					}
					
					if(databytes != null)
					{
						dataframe = new byte[(databytes.GetUpperBound(0)+1)+1+1+1+3]; // C + A + n + Prüfsumme
						dataframe[2] = Convert.ToByte(databytes.GetUpperBound(0));
						
						for(i=3; i <= (databytes.GetUpperBound(0) + 3); i++)
						{
							dataframe[i] = databytes[i-3];
						}
					}
					else
					{			
						oErrorCode = -20; // zu wenig Nutzdatenbytes
						return(null);					
					}
				}							
				
				dataframe[0] = calcCtrlByte();
				dataframe[1] = addr;
				
				// Calculate Checksum
				chkSum = 0;
				for(i=0; i<=dataframe.GetUpperBound(0) - 3; i++)
				{
					chkSum += Convert.ToInt32(dataframe[i]);
				}
				
								
				chkSum ^= checksum_code; // Verknüpfe Prüfsumme mit Code
				
				shift = 0;
				for(i=0; i<3; i++)
				{
					dataframe[dataframe.GetUpperBound(0)-i] = Convert.ToByte((chkSum >> shift) & 0xFF);
					shift += 8;
				}
				oErrorCode = 0;
				return(dataframe);			
			}
			
			/// <summary>
			/// Liefert versandfertiges Frame als Bytestream zurück.
			/// </summary>	
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>byte-Array</term><description>Versandfertiges Datenframe als Bytefolge</description></item>
			/// 	<item><term>null</term><description>Im Fehlerfall.</description></item>	
			/// </list>
			/// </returns>
			/// <remarks>
			/// Detaillierte Informationen siehe M3S-Doku in PDF-Form bzw. <see cref="IM3S_Dataframe">Konstruktor IM3S_Dataframe()</see> und <see cref="IM3S_Handler">Interface IM3S_Handler</see>
			/// </remarks> 
			/// <example>
			/// <para>Erzeugen des versandfertigen byte[]-Arrays</para>
			/// <para><c>byte[] packedFrame = frame.GetDataframe();	// Gepacktes versandfertiges Frame erzeugen</c></para>			
			/// </example>
			public byte[] GetDataframe() 
			{
				int dummy;
				return(GetDataframe(out dummy));
			}
			
			/// <summary>
			/// Konvertiert Parameter und Nutzdaten in M3S-Frame und gibt sie als String zurück.
			/// </summary>
			/// <returns>Hexdump des gepackten Frames als string</returns>
			public override string ToString()
			{
				string toReturn = "";
				
				if(protocol == M3SProtocol.Invalid)
				{
					return("Ungültiges m3sDatenframe");
				}
				
				byte[] frame = this.GetDataframe(); // Datenframe holen
				
				for(int i=0; i<=frame.GetUpperBound(0);i++) // und in String konvertieren
				{
					toReturn += string.Format("{0:X02} ", frame[i]);
				}
				
				return(toReturn);
			}
			
			
			#region String Parameters
			
			/// <summary>
			/// Kurzschreibweise des Protokolls dieses Frames
			/// </summary>
			public string InfoProtocolAcrynonym
			{
				get 
				{
					return(((M3SProtocolAcrynonyms)protocol).ToString());
				}
			}
			
			/// <summary>
			/// Information über das verwendete Protokoll
			/// </summary>
			public string InfoProtocol
			{
				get 
				{
					return((protocol).ToString());
				}
			}
			
			/// <summary>
			/// Nummer des verwendeten Protokolls
			/// </summary>
			public string InfoProtocolNumber
			{
				get
				{
					return(((int)(protocol)).ToString());
				}
				
			}
			#endregion
			
			
			
			// TODO Dokumentation interner Ablauf
			private byte calcCtrlByte()
			{
				ctrlByte = 0;
				ctrlByte |= Convert.ToByte((Convert.ToInt32(protocol) << 4) & 0xF0);
				ctrlByte |= Convert.ToByte((mAddr << 2) & 0x0C);
				
				if(	acknowledgeOrAckRequest)
				{
					ctrlByte |= 0x02;
				}
				
				if(!send)
				{
					ctrlByte |= 0x01; 
				}
				
				return(ctrlByte);
			}					
		}
		
		/// <summary>
		/// Interfacevereinbarung der Datenframe-Klassen unterschiedlicher Protokollversionen
		/// </summary>
		public interface IM3S_Dataframe
		{
			/// <summary>
			/// Methode zum Erzeugen eines versandfertigen Datenframes als Bytefolge
			/// </summary>
			/// <returns></returns>
			byte[] GetDataframe();
			/// <summary>
			/// Informationsstring (Kurzform) des verwendeten Protokolls
			/// </summary>
			string InfoProtocolAcrynonym {get;}
			/// <summary>
			/// Verwendetes Protokoll
			/// </summary>
			M3SProtocol Protocol {get;}
			/// <summary>
			/// Nummer des verwendeten Protokolls
			/// </summary>
			int ProtocolNr{get;}
			/// <summary>
			/// Gibt an, ob dieses Frame mit einem Acknowledge vom Slave quittiert werden muss oder nicht.
			/// </summary>
			bool NeedsAcknowledgement{get;}
			/// <summary>
			/// Anzahl der Datenbytes (inkl. Overhead)
			/// </summary>
			int Length{get;}
			
			/// <summary>
			/// Slave Address, only available if Unicast-Protocol
			/// </summary>
			int SlaveAddress{get;}
			
			/// <summary>
			/// MulticastAddress, only available if MC-Protocol
			/// </summary>
			int MulticastAddress{get;}
			
			/// <summary>
			/// Slave of Master in Transfer
			/// </summary>
			int MasterAddress{get;}
			
			/// <summary>
			/// Version of the M3S-Protocol
			/// </summary>
			int Version{get;}
			
			/// <summary>
			/// Analyses the Payload and Sub-Protocols (TWI, Filetransfer, ...) and gives a short interpretation
			/// </summary>
			/// <returns></returns>
			string GetInterpretation();
		}
		
		
		/// <summary>
		/// Basisklasse aller M3S-Handler für unterschiedliche Protokollversionen, implementiert gemeinsame Methoden. Abstract
		/// </summary>
		public class M3S_Handler
		{
			/// <summary>
			/// Prüft, ob das übergebene Bytearray als Nutzdaten für ein M3S-Datenframe gültig sind
			/// </summary>
			/// <param name="rToCheck">Nutzdatenbytes</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Nutzdatenlänge in Ordnung</description></item>
			/// 	<item><term>false</term><description>Nutzdatenlänge fehlerhaft. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>
			public bool CheckPayloadLength(byte[] rToCheck, out int oErrorCode)
			{
				if(rToCheck.Length > 256)
				{
					oErrorCode = -619;
					return(false); // Bytearray hat ungültige Länge für M3S-Länge
				}
				else
				{
					oErrorCode = 0;
					return(true); // passt
				}
			}
			
			/// <summary>
			/// Prüft, ob das übergebene Bytearray als Nutzdaten für ein M3S-Datenframe gültig sind
			/// </summary>
			/// <param name="rToCheck">Nutzdatenbytes</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Nutzdatenlänge in Ordnung</description></item>
			/// 	<item><term>false</term><description>Nutzdatenlänge fehlerhaft.</description></item>	
			/// </list>
			/// </returns>
			public bool CheckPayloadLength(byte[] rToCheck)
			{
				int dummy;
				return CheckPayloadLength(rToCheck, out dummy);
			}
			
			/// <summary>
			/// Validiert Kommandosequenzen (Gültigkeit, bei bekannten <see cref="M3SCommand">M3S-Commands </see> Parametervalidation)
			/// </summary>
			/// <param name="rCommandWithParameters">Kommando und Parameter, die validiert werden sollen</param>
			/// <param name="vBroadOrMulticast">Angabe, ob das Kommando für Broad- oder Multicasts gültig sein soll </param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreiem Kommando</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Daten erfolgreich gesendet, Acknowledge erhalten</description></item>
			/// 	<item><term>false</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>
			public bool KnownCommandValidation(byte[] rCommandWithParameters, bool vBroadOrMulticast, out int oErrorCode)
			{
				oErrorCode = 0;
				// TODO FIXME DRINGEND IMPLEMENTIEREN
				//throw new NotImplementedException("KnowCommandValidation ist noch nicht implementiert, sollte dringend mal gemacht werden");
				return(true);
			}
			
			/// <summary>
			/// Position (Index) des UpperBound-Bytes im Frame
			/// </summary>
			public int UpperBoundPosition
			{
				get
				{
					return(M3S_Dataframe.UpperBoundPosition);
				}
			}
		}
		
		/// <summary>
		/// Interfacevereinbarung für Protokollhandler (M3S)
		/// </summary>
		/// <remarks>
		/// Die M3S-Protokollhandler stellen Standardmethoden zur Erzeugung, Validierung und Verarbeitung von Datenframes zur Verfügung. Für jede Protokollversion gibt es einen eigenen Handler mit entsprechend angepassten Funktionen. Dieses Interface stellt die Mindestfunktionalität aller M3S-Handler sicher.
		/// <para>Instanzierbare Handler</para>
		/// <list type="bullet">
		/// <item><see cref="M3S_V1_Handler">M3S_V1_Handler</see></item>
		/// <item><see cref="M3S_V2_Handler">M3S_V2_Handler</see></item>
		/// </list>
		/// </remarks>
		public interface IM3S_Handler
		{
			/// <summary>
			/// Erhält einen Bytestream der ein M3S-Frame repräsentiert und wandelt dieses in eine über das Interface <see cref="IM3S_Dataframe">IM3S_Dataframe</see> zugreifbare Instanz um.
			/// </summary>
			/// <param name="rStream">Bytestream, der geparst werden soll</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreiem Kommando</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>M3S Dataframe</term><description>Via <see cref="IM3S_Dataframe">IM3S_Dataframe</see> zugreifbare Instanz eines Datenframes</description></item>
			/// 	<item><term>null</term><description>Daten konnten nicht übermittelt werden. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>
			IM3S_Dataframe CreateFrameByBytestream(byte[] rStream, out int oErrorCode);
			
			/// <summary>
			/// Erzeugt ein neues Datenframe
			/// </summary>
			/// <param name="vSlaveAddress">Adresse oder Multicastadresse des/der am Transfer beteiligten Slaves (1..255)</param>
			/// <param name="vProtocol">Zu verwendendes Protokoll</param>
			/// <param name="vMasterAddress">Adresse des am Transfer beteiligten Master (1-3)</param>
			/// <param name="rPayload">Bytearray mit Nutzdaten / Kommandodaten / Filestream etc. (max 256 Bytes)</param>
			/// <param name="vMasterSend">true... Master versendet dieses Frame, false ... Slave versendet dieses Frame</param>
			/// <param name="vAcknowledge">true wenn Acknowledge oder AcknowledgeRequest signalisiert werden soll (nur bei Unicastprotokollen), false bei Not-Acknowledge, Not-AcknowledgeRequest und allen anderen Frames</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>M3S Dataframe</term><description>Via <see cref="IM3S_Dataframe">IM3S_Dataframe</see> zugreifbare Instanz eines Datenframes</description></item>
			/// 	<item><term>null</term><description>Im Fehlerfall (falsche Parameter etc.)</description></item>	
			/// </list>
			/// </returns>
			IM3S_Dataframe CreateFrame(int vSlaveAddress, M3SProtocol vProtocol, int vMasterAddress, byte[] rPayload, bool vMasterSend, bool vAcknowledge);
			
			/// <summary>
			/// Liefert das Resetframe für das entsprechende Protokoll zurück
			/// </summary>
			/// <returns>
			/// Resetframe (versandfertig als Bytearray)
			/// </returns>
			IM3S_Dataframe GetResetFrame();
			
			/// <summary>
			/// Erzeugt ein Filetransferannouncement Frame
			/// </summary>
			/// <param name="vSlaveAddress">Slave, der das File empfangen soll</param>
			/// <param name="ByteStreamLength">Länge des nachfolgenden Bytestreams</param>
			/// <param name="TargetFilename">Name bzw. Pfad unter der der Slave die Datei ablegen soll</param>
			/// <returns>Versandfertiges Frame als Bytearray</returns>
			IM3S_Dataframe GetFileTransferAnnouncementFrame(int vSlaveAddress, int ByteStreamLength, string TargetFilename);
			
			/// <summary>
			/// Verpackt eine beliebig lange Bytefolge (z.b. File) in versandbereite (durchnummerierte) Pakete
			/// </summary>
			/// <param name="vSlaveAddress">Adresse des Slaves, der das File empfangen soll</param>
			/// <param name="rByteStream">Zu versendende Bytefolge (z.b. Binärdaten eines Files)</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreiem Kommando</param>
			/// <returns>
			/// Array von M3S_Dataframes-Instanzen (zugreifbar über <see cref="IM3S_Dataframe">IM3S_Dataframe</see>)
			/// </returns>
			IM3S_Dataframe[] GetFileTransferFrames(int vSlaveAddress, byte[] rByteStream, out int oErrorCode);
			
			/// <summary>
			/// Erzeugt einen Filerequest, auf den hin der Slave anfängt das geforderte File zu versenden.
			/// </summary>
			/// <param name="vSlaveAddress">Adresse des Slaves</param>
			/// <param name="vRequestFilename">Name und Pfad der Datei, die vom Slave versendet werden soll</param>
			/// <returns>Versandfertiges Frame als Bytearray</returns>
			IM3S_Dataframe GetFileRequestFrame(int vSlaveAddress, string vRequestFilename);
			
			/// <summary>
			/// Erzeugt ein allgemeines Updateframe
			/// </summary>
			/// <param name="vSlaveAddress">Slave, an den das Updatekommando gesendet werden soll</param>
			/// <param name="vAcknowledgeRequest">Gibt an, ob das Updatekommando vom Slave bestätigt werden muss oder nicht</param>
			/// <returns>Versandfertiges Frame als Bytefolge</returns>
			/// <remarks>Updateframes werden beispielsweise dazu eingesetzt, um Slaves zu signalisieren, dass sie deren aktuellen Zustand updaten sollen respektive in einigen Anwendungen permanent Speichern (beispielsweise ins EEPROM schreiben). Wie ein Slave auf das Updatekommando reagiert (oder ob es ignoriert wird) ist der Implementierung überlassen und nicht näher spezifiziert.</remarks>
			IM3S_Dataframe GetUpdateFrame(int vSlaveAddress, bool vAcknowledgeRequest);
			
			/// <summary>
			/// Extrahiert die Datenbytes aus einem Frame (Bytefolge)
			/// </summary>
			/// <param name="rFrame">Komplettes Frame inkl. Rahmendaten</param>
			/// <exception cref="TBL.Exceptions.FrameError">FrameError</exception>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>Bytefolge</term><description>Extrahierte Nutzdatenbytes</description></item>
			/// 	<item><term>null</term><description>Im Fehlerfall.</description></item>	
			/// </list>
			/// </returns>
			byte[] ExtractPayload(byte[] rFrame);
			
			/// <summary>
			/// Erzeugt ein Kommandoframe, das eine Baudratenveränderung im Slave initiiert
			/// </summary>
			/// <param name="vSlaveOrMCAddr">Slaveadresse oder Multicastadresse der Zielgeräte / des Zielgeräts</param>
			/// <param name="vBaudrate">Neue Baudrate</param>
			/// <param name="vBroadcast">Gibt an, ob der Befehl via Multicastprotokoll versendet werden soll. true für Multicast und Broadcast, false für Unicasts </param>
			/// <param name="vMulticast">Im Falle vBroadcast==true: Broadcast oder Multicast (vSlaveOrMCAddr wird verwendet)</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreiem Kommando</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>Bytefolge</term><description>versandfertiges Frame</description></item>
			/// 	<item><term>null</term><description>Im Fehlerfall. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>
			IM3S_Dataframe GetBaudrateChangeFrame(int vSlaveOrMCAddr, int vBaudrate, bool vBroadcast, bool vMulticast, out int oErrorCode);
			
			/// <summary>
			/// Liefert Acknowledgeframe zu einem empfangenen Frame
			/// </summary>
			/// <param name="Acknowledged">Acknowledge (true) / not acknowledge (false)</param>
			/// <param name="rFrameToAcknowledge">Frame, das acknowledged werden soll (inkl. Rahmendaten)</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>Bytefolge</term><description>versandfertiges Frame</description></item>
			/// 	<item><term>null</term><description>Im Fehlerfall.</description></item>	
			/// </list>
			/// </returns>
			IM3S_Dataframe GetAcknowledgeFrame(bool Acknowledged, byte[] rFrameToAcknowledge);
			
			/// <summary>
			/// Prüft, ob ein empfangenes Frame ein Acknowledgeframe für ein gesendetes ist
			/// </summary>
			/// <param name="pSent">gesendetes Frame (inkl. Rahmendaten)</param>
			/// <param name="pRec">empfangenes Frame (inkl. Rahmendaten)</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreiem Kommando</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Empfangenes Frame ist passendes Acknowledge</description></item>
			/// 	<item><term>false</term><description>Empfangenes Frame ist kein Acknowledgeframe oder explizites NOT Acknowledge (oder Acknowledge für ein anderes Paket)</description></item>
			/// </list>
			/// </returns>
			bool IsAcknowledge(byte[] pSent, byte[] pRec, out int oErrorCode);
			
			/// <summary>
			/// Prüft, ob ein empfangenes Frame ein Acknowledgeframe für ein gesendetes ist
			/// </summary>
			/// <param name="pSent">gesendetes Frame (inkl. Rahmendaten)</param>
			/// <param name="pRec">empfangenes Frame (inkl. Rahmendaten)</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreiem Kommando</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Empfangenes Frame hat Acknowledgeflag gesetzt</description></item>
			/// 	<item><term>false</term><description>Hat Acknowledgeflag NICHT gesetzt</description></item>
			/// </list>
			/// </returns>
			bool IsImplicitAcknowledge(byte[] pSent, byte[] pRec, out int oErrorCode);
			
			/// <summary>
			/// Prüft, ob es sich bei einer Bytefolge um ein valides Frame handelt
			/// </summary>
			/// <param name="pFrameToCheck"></param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Frame ist valide</description></item>
			/// 	<item><term>false</term><description>Die übergebene Bytefolge ist kein gültiges Frame</description></item>
			/// </list>
			/// </returns>
			bool IsFrame(byte[] pFrameToCheck);
			
			/// <summary>
			/// Prüft, ob eine Nutzdatenbytefolge (Array) in ein M3S-Frame passt oder nicht
			/// </summary>
			/// <param name="rPayload">Nutzdaten eines Frames (ohne Rahmendaten)</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Ausführung</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Länge der Nutzdaten in Ordnung</description></item>
			/// 	<item><term>false</term><description>Im Fehlerfall. Details über den Rückgabeparameter oErrorCode auswertbar</description></item>	
			/// </list>
			/// </returns>
			bool CheckPayloadLength(byte[] rPayload, out int oErrorCode);
			
			/// <summary>
			/// Länge eines Acknowledgeframes in Byte
			/// </summary>
			int AcknowledgeFrameLength{get;}
			/// <summary>
			/// Minimale mögliche Framelänge in Byte (entspricht i.d.R der AcknowledgeFrameLength)
			/// </summary>
			int MinimumFrameLength{get;}
			/// <summary>
			/// Länge des Overheads in Byte
			/// </summary>
			int Overhead {get;}
			/// <summary>
			/// Position des UpperBound innerhalb eines Frames (Index, Zählung beginnend bei 0)
			/// </summary>
			int UpperBoundPosition {get;}
			/// <summary>
			/// Maximale Framelänge in Bytes
			/// </summary>
			int MaximumFrameLength{get;}
			
			/// <summary>
			/// Validiert ein Kommandoframe inkl. Parameter (wenn Kommando "well-known" (bekannt))
			/// </summary>
			/// <param name="rCommandWithParameters">Kommando (erstes Byte), optional mit Parameter (nachfolgende Bytes)</param>
			/// <param name="vBroadOrMulticast">Spezifiziert, ob das Kommando als Broad- oder Multicast gesendet werden wird</param>
			/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Ausführung</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Kommando in Ordnung, Kommando unknown</description></item>
			/// 	<item><term>false</term><description>Kommando ist Fehlerhaft</description></item>	
			/// </list>
			/// </returns>
			/// <remarks>
			/// <para>Handelt es sich bei dem übergebenen Kommando mit seinen Parametern um einen "well-known"-Command, so werden diese gemäß M3S-Spezifikation auf Validät überprüft. Im Fehlerfall, wird über den Ausgabeparameter eine Fehlernummer zurückgegeben, um dem Programmierer die Fehlersuche zu erleichtern. Ist das Kommando in der M3S-Konvention nicht spezifiziert, so wird das Kommando nicht geprüft und true zurückgegeben, um den weiteren Programmablauf nicht zu behindern.</para>
			/// <para>Für Listen der jeweils geprüften Kommandos <see cref="M3S_V2_Handler">M3S_V2_Handler</see> bzw. den älteren <see cref="M3S_V1_Handler">M3S_V1_Handler</see></para>
			/// </remarks>
			bool KnownCommandValidation(byte[] rCommandWithParameters, bool vBroadOrMulticast, out int oErrorCode);
			
			
			M3SProtocol ExtractProtocol(byte vCtrlByte);
			
			
		}
		
		/// <summary>
		/// Gemeinsame Eigenschaften und Implementierungen aller M3S-Dataframe-Klassen
		/// </summary>
		public abstract class M3S_Dataframe
		{
			/// <summary>
			/// Index des Controlbytes innerhalb eines M3S-Frames
			/// </summary>
			protected static readonly int ctrlBytePos = 0;
			/// <summary>
			/// Index der Slaveadresse innerhalb eines M3S-Frames
			/// </summary>
			protected static readonly int slaveAddrPos = 1;
			/// <summary>
			/// Index des UpperBound (höchster Index der Nutzdatenbytes) innerhalb eines M3S-Frames
			/// </summary>
			protected static readonly int upperBoundPos = 2;
			
			/// <summary>
			/// Länge des Headers in Byte
			/// </summary>
			protected static readonly int headerLength = 3;
			
			/// <summary>
			/// Maximale Länge der Nutzdaten
			/// </summary>
			protected static readonly int maxPayloadLength = 256;
			
			/// <summary>
			/// Am Transfer beteiligter Slave
			/// </summary>
			protected byte addr;
			/// <summary>
			/// Verwendetes Protokoll
			/// </summary>
			protected M3SProtocol protocol = M3SProtocol.Invalid;
			/// <summary>
			/// Payload
			/// </summary>
			protected byte[] databytes;
			/// <summary>
			/// Am Transfer beteiligter Master
			/// </summary>
			protected int mAddr;
			/// <summary>
			/// Acknowledge/Multicast-Flag setzen ja/nein
			/// </summary>
			protected bool acknowledgeOrAckRequest;
			/// <summary>
			/// Datenrichtung: MasterSend / !SlaveSend
			/// </summary>
			protected bool send;
			///<summary>
			/// Controlbyte des Frames 
			///</summary>
			protected byte ctrlByte;
			
			/// <summary>
			/// Index / Position des UpperBound innerhalb eines M3S-Datenframes
			/// </summary>
			public static int UpperBoundPosition
			{
				get
				{
					return(upperBoundPos);
				}
			}			
			
			/// <summary>
			/// Länge des Frameheaders in Byte
			/// </summary>
			public static int HeaderLength
			{
				get
				{
					return(headerLength);
				}
			}
			
			/// <summary>
			/// Position (Index) der Slaveadresse im Frame
			/// </summary>
			public static int SlaveAddressPosition
			{
				get
				{
					return(slaveAddrPos);
				}
			}
			
			/// <summary>
			/// Position (Index) des Controlbytes im Frame
			/// </summary>
			public static int CtrlBytePosition
			{
				get
				{
					return(ctrlBytePos);
				}
			}
		}
		
		
		/// <summary>
		/// Datenframes des M3S-Protokolls (Version 2.x)
		/// </summary>
		public class M3S_V2_Dataframe: M3S_Dataframe, IM3S_Dataframe
		{		

			private static readonly int ackFrameLen = 3;			
						
			private const byte crcInitialValue=0;	
			private static readonly int crcLength = 1;
			private static readonly int overheadLength = headerLength+crcLength;
			
			private static M3S_V2_Handler handler = new M3S_V2_Handler();			
			private static Crc8 crcHandler = new Crc8(crcInitialValue);
			 
			
			#region Properties
			
			/// <summary>
			/// Länge des Frames (Nutz- und Rahmendaten) in Byte
			/// </summary>
			public int Length
			{
				get
				{
					return(overheadLength+databytes.Length);
				}
			}
			
			public int Version
			{
				get
				{
					return(2);
				}
			}
			
			public int SlaveAddress
			{
				get
				{
					return(addr);
				}
			}
			
			public int MulticastAddress
			{
				get
				{
					return(addr);
				}
			}
			
			/// <summary>
			/// Maximale Länge, die ein Frame erreichen kann, in Byte
			/// </summary>
			public static int MaximumFrameLength
			{
				get
				{
					return(overheadLength + maxPayloadLength);
				}
			}
			
			public bool NeedsAcknowledgement
			{
				get
				{
					if(protocol == M3SProtocol.Command || protocol == M3SProtocol.FileTransfer ||protocol == M3SProtocol.DataTransfer)
					{
						return(acknowledgeOrAckRequest && send); // Wenn flag gesetzt, wird Acknowledge requested
					}
					else
					{
						return false; // Alle anderen Protokolle werden nicht acknowledged
					}
				}
			}
			
			
			
			/// <summary>
			/// Länge des CRC-Wertes in Byte
			/// </summary>
			public static int CrcLength
			{
				get
				{
					return(crcLength);
				}
			}
			
			/// <summary>
			/// Startwert des CRC, das für die Datensicherheit innerhalb der Frames verwendet wird
			/// </summary>
			public static byte CrcInitialValue
			{
				get
				{
					return(crcInitialValue);
				}
			}
			
			
			/// <summary>
			/// Gibt die minimale Framelänge im M3S-Protokoll an (5 Bytes, Acknowledgeframes)
			/// </summary>			
			public M3SProtocol Protocol
			{
				get
				{
					return(protocol);
				}
			}
			
			/// <summary>
			/// Nummer des verwendeten Protokolls (0...15)
			/// </summary>
			public int ProtocolNr
			{
				get
				{
					return((int)protocol);
				}
			}
			
			/// <summary>
			/// Länge der Rahmendaten in Byte
			/// </summary>
			public static int Overhead
			{
				get
				{
					return(overheadLength);
				}
			}
			
			/// <summary>
			/// Minimale mögliche Framelänge in Byte (i.d.R. AcknowledgeFrameLength)
			/// </summary>
			public static int MinimumFrameLength
			{
				get
				{
					return(ackFrameLen);
				}
			}
						
			/// <summary>
			/// Länge eines Acknowledgeframes in Byte
			/// </summary>
			public static int AcknowledgeFrameLength
			{
				get
				{
					return(ackFrameLen);
				}
			}			
			
			#endregion
			
			#region Equals and GetHashCode implementation
			/// <summary>
			/// Prüft zwei <see cref="M3S_V2_Dataframe">M3S-Dataframes</see> auf Gleichheit
			/// </summary>
			/// <param name="toCompare">Zu vergleichendes Objekt (byte[] oder Objekt vom Typ <see cref="M3S_V2_Dataframe">M3S_V2_Dataframe</see></param>
			/// <returns>
			/// true / false</returns>
			/// <remarks>
			/// Vergleicht die beiden Datenframes durch ihre resultierenden Streams (Rahmen + Nutzdaten)
			/// </remarks>
			public override bool Equals(object toCompare)
			{				
				byte[] stream;
				
				try
				{
					// is it a M3s-Dataframe?
					M3S_V2_Dataframe tmpFrame = toCompare as M3S_V2_Dataframe;
					stream = tmpFrame.GetDataframe();
				}  
				catch
				{
					// was no IM3S_Dataframe
					try
					{
						stream = toCompare as byte[];
					}
					catch
					{
						TBL.Exceptions.ConversionException e = new TBL.Exceptions.ConversionException("IM3S_Dataframe.CompareTo hat ein Objekt erhalten, das weder ein IM3S_Dataframe noch ein byte-Array ist");
						throw e;
					}
				}	

				byte[] thisStream = this.GetDataframe();
				// wenn hier, hat die Konvertierung hingehaut
				
				if(stream.Length != thisStream.Length)
				{
					return(false);
				}
				
				for(int i=0; i<stream.Length; i++)
				{
					if(stream[i] != thisStream[i])
					{
						return(false);
					}
				}
				
				return(true);
			}
			
			/// <summary>
			/// Erzeugt aufgrund einiger Felder einen Hashcode
			/// </summary>
			/// <returns>Hashcode</returns>
			public override int GetHashCode()
			{
				int hashCode = 0;
				unchecked {
					hashCode += 1000000007 * addr.GetHashCode();
					hashCode += 1000000009 * protocol.GetHashCode();
					if (databytes != null)
						hashCode += 1000000021 * databytes.GetHashCode();
					hashCode += 1000000033 * mAddr.GetHashCode();
					hashCode += 1000000087 * acknowledgeOrAckRequest.GetHashCode();
					hashCode += 1000000093 * send.GetHashCode();
					hashCode += 1000000123 * ctrlByte.GetHashCode();
				}
				return hashCode;
			}
			
			/// <summary>
			/// Vergleichsoperator (Referenz und/oder Wertvergleich)
			/// </summary>
			/// <param name="lhs">Linker Operand</param>
			/// <param name="rhs">Rechter Operand</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Bei Gleichheit</description></item>
			/// 	<item><term>false</term><description>Bei Ungleichheit</description></item>	
			/// </list>
			/// </returns>
			/// <remarks>Der Vergleich wird bezüglich Referenzen und der Werte des versandfertigen Frames (Bytefolge) durchgeführt</remarks>
			public static bool operator ==(M3S_V2_Dataframe lhs, M3S_V2_Dataframe rhs)
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
			/// <param name="rhs">Rechter Operand</param>
			/// <returns>
			/// <list type="table">
			/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
			/// 	<item><term>true</term><description>Bei Ungleichheit</description></item>
			/// 	<item><term>false</term><description>Bei Gleichheit</description></item>	
			/// </list>
			/// </returns>
			/// <remarks>Der Vergleich wird bezüglich Referenzen und der Werte des versandfertigen Frames (Bytefolge) durchgeführt</remarks>
			public static bool operator !=(M3S_V2_Dataframe lhs, M3S_V2_Dataframe rhs)
			{
				return !(lhs == rhs);
			}
			#endregion

			/// <summary>
			/// Gibt einen Informationsstring zurück, der alle relevanten Parameter für das aktuelle Frames beinhaltet
			/// </summary>
			/// <returns>Kompakter Informationsstring</returns>
			/// <remarks>Die Informationen werden dabei als verbal, als Wort-/Wert-Paar oder Bytestream (hex) dargestellt.</remarks>
			public string GetInterpretation()
			{
				switch(protocol)
				{
						case M3SProtocol.Reset: return(this.ToString());
						case M3SProtocol.Command: return(this.infoStringCommand());
						case M3SProtocol.CommandBroadcast: return(this.infoStringCommand());
						case M3SProtocol.CommandResponse: return(this.infoStringCommandAnswer());
						case M3SProtocol.Acknowledge: if(acknowledgeOrAckRequest) return("[ACK]"); else return("[!!! NOT ACK !!!]");
						case M3SProtocol.DataTransfer: return("[HEXDUMP=" + hexdumpDatabytes(0) + "]");
						case M3SProtocol.BroadCast: return("[HEXDUMP=" + hexdumpDatabytes(0) + "]");
						                                      
						default: return(this.ToString());
				}
			}
			
			#region ProtocolSpecificInfoStrings
			private string infoStringCommand()
			{
				string toReturn;
				
				try
				{
					// Interpret command
					M3SCommand cmdInterpreted = (M3SCommand)(databytes[0]);
					toReturn = "[cmd=" + cmdInterpreted.ToString() + "] ";
				}
				catch
				{
					// unknown command
					toReturn = "[cmd=" + string.Format("{02:x}", databytes[0]) + "] ";
				}
				
				if(databytes.Length > 1)
				{
					toReturn += "[param = " + hexdumpDatabytes(1) +"] ";
					
				}
				
				return(toReturn);
			}
			
			private string infoStringCommandAnswer()
			{
				if(databytes != null)
				{
					if(databytes.Length > 0)
					{
						return("[RESP] [param= " + hexdumpDatabytes(0) + "]");
					}
				}
				return("[param= none]");
				
			}
			
			private string hexdumpDatabytes(int offset)
			{
				
				string toReturn = "";
				
				for(int i = offset; i<databytes.Length; i++)
				{
					toReturn += string.Format("{00:x} ",databytes[i]);
				}
				
				return(toReturn);
			}
			
			private string decimaldumpDatabytes(int offset)
			{
				string toReturn = "";
				
				for(int i = offset; i<databytes.Length; i++)
				{
					toReturn += databytes[i].ToString() + " ";
				}
				
				return(toReturn);
			}
			
			
			#endregion
			
		
			/// <summary>
			/// Adresse des am Transfer beteiligten Masters
			/// </summary>
			public int MasterAddress
			{
				get
				{
					return(mAddr);
				}
			}
			
			/// <summary>
			/// Identität des Senders dieses Frames als String
			/// </summary>
			/// <remarks>
			/// <para>
			/// Mögliche Werte (n steht dabei für eine Ziffer)
			/// </para>
			/// <list type="table">
			/// 	<listheader><term>Muster</term><description>Beschreibung</description></listheader>
			/// 	<item><term>Mn</term><description>Sender ist Master mit Adresse 0...3 (z.B. M1)</description></item>
			/// 	<item><term>nnn (SL)</term><description>Sender ist Slave mit Adresse 1...255</description></item>	
			/// </list>
			/// </remarks>
			public string SenderIdentity
			{
				get
				{
					bool mastersend = send;					
					
					if(protocol == M3SProtocol.Acknowledge) // ursprüngliche Datenrichtung beibehalten
					{
						mastersend = !send; // hier genau das gegenteil
					}
					
					if(mastersend)
					{
						return(string.Format("M{0:0}", mAddr));
					}
					else
					{
						return(string.Format("{0:000} (SL)", addr));
					}					
				}
			}
			
			/// <summary>
			/// Identität des Empfängers des Frames
			/// </summary>
			/// <remarks>
			/// <para>
			/// Mögliche Werte (n steht dabei für eine Ziffer)
			/// </para>
			/// <list type="table">
			/// 	<listheader><term>Muster</term><description>Beschreibung</description></listheader>
			/// 	<item><term>all</term><description>Empfänger sind alle Slaves (Broadcast)</description></item>
			/// 	<item><term>nnn (MC)</term><description>Empfänger ist eine Gruppe von Slaves (Multicastdomain), Adresse 1...255</description></item>
			/// 	<item><term>nnn (SL)</term><description>Sender ist Slave mit Adresse 1...255</description></item>						
			/// 	<item><term>Mn</term><description>Sender ist Master mit Adresse 0..3 (z.b. M1)</description></item>
			/// </list>
			/// </remarks>
			public string TargetIdentity
			{
				get
				{
					bool mastersend = send;
					
					if(protocol == M3SProtocol.Acknowledge)
					{
						mastersend = !send;
					}
					
					if(mastersend)
					{
						if(protocol == M3SProtocol.Reset)
						{
							return("all");
						}
						
						if((protocol == M3SProtocol.BroadCast || protocol == M3SProtocol.CommandBroadcast))
						{
							if(acknowledgeOrAckRequest)
							{
								// Multicast
								return(string.Format("{0:000} (MC)", addr));
							}
							else
							{
								return("all");
							}
						}

						return(string.Format("{0:000} (SL)", addr));						
					}
					else
					{
						return(string.Format("M{0:0}", mAddr));
					}
				}
			}
			
			
			
			
			/// <summary>
			/// Konstruktor: Alle zu verpackenden Parameter werden übergeben, können später nicht mehr geändert werden.
			/// </summary>			/// 
			/// <param name="pAddr">Slaveadresse (0 bei Broadcasts)</param>
			/// <param name="pProtocol"><see cref="M3SProtocol">M3SProtocol</see></param>
			/// <param name="pMAddr">Masteradresse (1..3), 0 bei Broadcasts und Reset</param>
			/// <param name="pData">max. 256 sendende Datenbytes als byte[]</param>
			/// <param name="pSend">Senden?
			/// <list type="bullet">
			/// <item>true ... Master send, Slave Receive</item>
			/// <item>false... Slave send / Master receive</item>
			/// </list>
			/// </param>
			/// <param name="vAcknowledge">true wenn Acknowledge oder AcknowledgeRequest signalisiert werden soll (nur bei Unicastprotokollen), false bei Not-Acknowledge, Not-AcknowledgeRequest und allen anderen Frames</param>
			/// <example>Das Beispiel zeigt das Verpacken diverser Parameter in versandfertige Datenbytes
			/// <code>
			/// TODO Codexample...
			/// </code>
			/// </example>
			public M3S_V2_Dataframe(int pAddr, M3SProtocol pProtocol, int pMAddr, byte[] pData, bool pSend, bool vAcknowledge)
			{
				addr = Convert.ToByte(pAddr);	
				databytes = pData;	
				send = pSend;
								
				acknowledgeOrAckRequest = vAcknowledge;
				protocol = pProtocol;
				mAddr = pMAddr;										
			}				
			
			
			
			/// <summary>
			/// Prüft, ob das übergebene Bytearray als Nutzdaten für ein M3S-Datenframe gültig sind
			/// </summary>
			/// <param name="rToCheck">Zu überprüfendes Bytearray</param>
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>0</term><description>Daten erfolgreich gesendet</description></item>
			/// 	<item><term>-619</term><description>Ungültige M3S-Adresse (Unicast oder Multicast). Gültig: 1...255</description></item>		
			/// </list>
			public static int CheckPayloadLength(byte[] rToCheck)
			{
				if(rToCheck==null)
				{
					throw new TBL.Exceptions.ObjectNull("passed Payload to check is null-Pointer @ function IM3S_Dataframe.CheckPayloadLength(byte[])");
				}
				if(rToCheck.Length > maxPayloadLength)
				{
					return(-619); // Bytearray hat ungültige Länge für M3S-Nutzdaten
				}
				else
				{
					return(0); // passt
				}
			}
			
		
			
			
			
			/// <summary>
			/// Liefert im Konstruktor übergebene Daten gemäß M3S-Protokoll gepackt zurück.
			/// </summary>			
			/// <returns> 
			/// <list type="bullet">
			/// <item>Verpacktes Datenframe (max. 256+6 Bytes) </item>
			/// <item>null im Fehlerfall</item>
			/// </list></returns>
			/// <remarks>
			/// Die Klasse ist <see cref="EDOLL">EDOLL-Implementiert</see>. Es können folgende Fehler auftreten
			/// <list type="table">
			/// <listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// <item><term>-20</term><description>Ungültige Länge an Nutzdatenbytes, entweder zu viele oder zu wenig</description></item>
			/// </list>
			/// Detaillierte Informationen siehe M3S-Doku in PDF-Form bzw. <see cref="IM3S_Dataframe">Konstruktor IM3S_Dataframe()</see> und <see cref="IM3S_Handler">Interface IM3S_Handler</see>
			/// </remarks> 
			/// <example>
			/// <para>Erzeugen des versandfertigen byte[]-Arrays</para>
			/// <para><c>byte[] packedFrame = frame.GetDataframe();	// Gepacktes versandfertiges Frame erzeugen</c></para>			
			/// </example>
			public byte[] GetDataframe()
			{
				int i;		
				byte[] dataframe;
				
				if(protocol == M3SProtocol.Acknowledge) // Das einzige Frame ohne Datenbytes ist folgendes:
				{
					dataframe = new Byte[ackFrameLen];
				}
				else
				{
					if(databytes.GetUpperBound(0) > 255)
					{
						EDOLLHandler.Error(-20); // Zu viele Nutzdatenbytes
						return(null);
					}
					
					if(databytes != null)
					{
						dataframe = new byte[databytes.Length + overheadLength];
						
						dataframe[2] = Convert.ToByte(databytes.GetUpperBound(0));
						
						for(i=3; i <= (databytes.GetUpperBound(0) + 3); i++)
						{
							dataframe[i] = databytes[i-3];
						}
					}
					else
					{			
						EDOLLHandler.Error(-20);
						return(null);					
					}
				}							
				
				dataframe[0] = calcCtrlByte();
				dataframe[1] = addr;
				
				dataframe[dataframe.Length-1] = crcHandler.Calculate(dataframe, dataframe.Length-1); // Insert CRC				
				
				return(dataframe);			
			}
			
			/// <summary>
			/// Konvertiert Parameter und Nutzdaten in M3S-Frame und gibt sie als String zurück.
			/// </summary>
			/// <returns>Hexdump des gepackten Frames als string</returns>
			public override string ToString()
			{
				string toReturn = "";
				
				if(protocol == M3SProtocol.Invalid)
				{
					return("Ungültiges m3sDatenframe");
				}
				
				byte[] frame = this.GetDataframe(); // Datenframe holen
				
				for(int i=0; i<=frame.GetUpperBound(0);i++) // und in String konvertieren
				{
					toReturn += string.Format("{0:X02} ", frame[i]);
				}
				
				return(toReturn);
			}
			
			
			#region String Parameters
			
			/// <summary>
			/// Informationsstring (3-Buchstaben Akrynonym)) des verwendeten Protokolls
			/// </summary>
			/// <seealso cref="Protocol">Protocol</seealso>
			/// <seealso cref="ProtocolNr">ProtocolNr</seealso>
			public string InfoProtocolAcrynonym
			{
				get 
				{
					return(((M3SProtocolAcrynonyms)protocol).ToString());
				}
			}
			#endregion
			
			
			
			// UNDONE Dokumentation interner Ablauf
			private byte calcCtrlByte()
			{
				ctrlByte = 0;
				ctrlByte |= Convert.ToByte((Convert.ToInt32(protocol) << 4) & 0xF0);
				ctrlByte |= Convert.ToByte((mAddr << 2) & 0x0C);
				
				if(	acknowledgeOrAckRequest)
				{
					ctrlByte |= 0x02;
				}
				
				if(!send)
				{
					ctrlByte |= 0x01; 
				}
				
				return(ctrlByte);
			}					
		}
		
		
		
		/// <summary>
		/// Der M3S_V1_Handler ist das Protokollinterface für M3S Version 1.x. Datenextraktion/-zusammenführung + Befehlsbildung findet in der Instanz dieser Klasse statt.
		/// </summary>
		/// <remarks>
		/// <para>Der Handler stellt im Wesentlichen die Schnittstelle zwischen Datenframe und DevCom her. Hier werden Datenpakete erstellt und ausgewertet. Außerdem sind einige höhere Methoden (zum Beispiel das Splitten von Dateien in Datenframes) direkt implementiert (u.a. mit Festplattenzugriff).</para>
		/// <para></para>
		/// </remarks>
		public class M3S_V1_Handler: M3S_Handler, IM3S_Handler
		{			
			private int masteraddr	= 1;
			private int chkSum_code = M3S_V1_Dataframe.CheckSumCode;
				

			private const int packageNumberLength = 4;	
			
			/// <summary>
			/// Maximal mögliche Länge eines Datenframes in Byte
			/// </summary>
			public int MaximumFrameLength
			{
				get
				{
					return M3S_V1_Dataframe.MaximumFrameLength;
				}
			}
			///<summary>Länge der Rahmendaten in Bytes</summary>			
			public int Overhead
			{
				get
				{
					return(M3S_V1_Dataframe.Overhead);
				}
			}
			/// <summary>
			/// Länge eines Acknowledgeframes in Byte
			/// </summary>
			public int AcknowledgeFrameLength
			{
				get
				{
					return(M3S_V1_Dataframe.AcknowledgeFrameLength);
				}
			}
			
			/// <summary>
			/// Länge des kleinstmöglichen Frames (i.d.R AcknowledgeFrameLength)
			/// </summary>
			public int MinimumFrameLength
			{
				get
				{
					return(M3S_V1_Dataframe.MinimumFrameLength);
				}
			}
			
			/// <summary>
			/// Erzeugt eine neue <see cref="M3S_V1_Dataframe">M3S_V1_Dataframe</see> Instanz
			/// </summary>
			/// <param name="vSlaveAddress">Am Transfer beteiligter Frame</param>
			/// <param name="vProtocol">Verwendetes Protokoll</param>
			/// <param name="vMasterAddress">Am Transfer beteiligter Master</param>
			/// <param name="rData">Payload</param>
			/// <param name="vMasterSend">Datenrichtung: MasterSend / !SlaveSEnd</param>
			/// <param name="vAcknowledge">
			///<list type="table">
			/// 	<listheader><term>Verwendetes Protokoll</term><description>Werte</description></listheader>
			/// 	<item><term>Acknowledge</term><description>True... Acknowledge, False ... Not Acknowledge></description></item>
			/// 	<item><term>Unicastprotokoll, Datenrichtung MasterSend</term><description>true ... Acknowledge Request, false ... Frame darf nicht quittiert werden</description></item>
			/// 	<item><term>Andere Protokolle</term><description>false übergeben</description></item>			
			/// </list>
			/// </param>
			/// <returns>Via <see cref="IM3S_Dataframe">IM3S_Dataframe</see> zugreifbare Instanz eines Datenframes</returns>			
			public IM3S_Dataframe CreateFrame(int vSlaveAddress, M3SProtocol vProtocol, int vMasterAddress, byte[] rData, bool vMasterSend, bool vAcknowledge)
			{
				return((IM3S_Dataframe)(new M3S_V1_Dataframe(vSlaveAddress,vProtocol,vMasterAddress,rData,vMasterSend,vAcknowledge)));
			}
			
				/// <summary>
			/// Erstellt ein m3sDatenframe aus einem M3S-Bytestream
			/// </summary>
			/// <param name="rStream">Bytestream (gültiges M3S-Frame)</param>
			/// <param name="oErrorCode">Ausgabe: M3S-Errorcode (0 bei Korrekter Funktion)</param>
			/// <returns>
			/// <list type="table">
			/// <item><term>Objekt vom Typ <see cref="IM3S_Dataframe">m3sDataframe</see></term><description>bei fehlerfreier Ausführung</description></item>
			/// <item><term>null</term><description>im Fehlerfall (Errorcode über out int pErrorCode)</description></item>
			/// </list>
			/// </returns>
			public IM3S_Dataframe CreateFrameByBytestream(byte[] rStream, out int oErrorCode)
			{
				oErrorCode = -555; // common error
							
				if(!IsFrame(rStream, out oErrorCode))
				{
					// Errorcode wird über out gesetzt...
				 	return(null); 
				}
				// else Frame ist zumindest syntaktisch sinnvoll
				
				int tmpAddress = rStream[1];
				M3SProtocol protokoll = this.ExtractProtocol(rStream);
				int masterAddr = this.ExtractMasterAddress(rStream);
				byte[] nutzdaten = this.ExtractPayload(rStream);
				
				bool ack = false;
				bool send = true; // aus Mastersicht
				
				if((rStream[0] & 0x02) == 0x02) // Falls Acknowledgebit gesetzt
				{
					ack = true;	
				}
				
				if((rStream[0] & 0x01) == 0x01) // Falls SS-Bit gesetzt
				{
					send = false; // Slave hat gesendet	
				}
				
				M3S_V1_Dataframe tempFrame = new M3S_V1_Dataframe(tmpAddress,protokoll, masterAddr, nutzdaten, send, ack);
				
				
				return((IM3S_Dataframe)(tempFrame)); // Fehler
			}
			
			
			
			/// <summary>
			/// Prüft ob es sich beim übergebenen Frame um ein gültiges M3S-Frame handelt und validiert die Rahmendaten
			/// </summary>
			/// <param name="rToCheck">Zu überprüfendes Frame</param>
			/// <param name="oErrorCode">Über diesen out-Parameter wird ein <see cref="EDOLLHandler">EDOLL-Fehlercode</see> zurückgegeben</param>
			/// <returns>
			/// true / false. Im Fehlerfall außerdem über den oErrorCode-Parameter EDOLL-Errorcodes
			/// </returns>
			/// <remarks>
			/// Die Funktion überprüft das M3S-Datenframe auf folgende Punkte (in dieser Reihenfolge, beim ersten Fehler wird abgebrochen)
			/// <list type="number">
			/// <item>Frame kürzer als Minimale M3S-Framelänge</item>
			/// <item>Checksum-Error (Fehlercode -27)</item>
			/// <item>upperBound stimmt mit Framelänge nicht überein (Fehlercode -28)</item>
			/// <item>Aufgrund von Framelänge als Acknowledgeframe identifiziertes Frame verwendet kein Acknowledgeprotokoll (Fehlercode -31)</item>
			/// <item>Protokollnr ist ungültig oder konnte nicht extrahiert werden (Fehlercode -29)</item>
			/// <item>Protokoll ist Broadcast / Resetframe, führt aber eine SlaveAdresse ungleich 0 (Fehlercode -30)</item>
			/// <item>Slaveadresse ist ungültig - außerhalb [1 255] (Fehlercode -602)</item>
			/// </list>
			/// </remarks>
			public bool IsFrame(byte[] rToCheck, out int oErrorCode)
			{
				oErrorCode = -555; // general Error
				
				if(rToCheck.Length < M3S_V1_Dataframe.AcknowledgeFrameLength)
				{
					oErrorCode = -32; // Frame zu kurz.. Dieser FEhler sollte an und für sich nur bei Analysen auftreten
				}
				
				bool chkSumOk = this.IsFrame(rToCheck);
				
				if(!chkSumOk)
				{
					oErrorCode = -27; // Prüfsummenfehler M3S
					return(false);
				}
				// else
				
				if(rToCheck.Length == M3S_V1_Dataframe.AcknowledgeFrameLength)
				{
					// Wenn Acknowledgeframe
					if(this.ExtractProtocol(rToCheck) == M3SProtocol.Acknowledge)
					{
						oErrorCode = 0; 
/*REGULAR EXIT*/   		return(true);
					}
					else
					{
						oErrorCode = -31;
						return(false);
					}
					
				}
				else
				{
					// Prüfen ob anderes Frame
					if(((int)(rToCheck[2])+1+ 6) != rToCheck.Length)
					{
						oErrorCode = -28; // upperBound stimmt nicht mit Framelänge überein
						return(false);
					}
					
					
					#region Protokoll und Slaveadressprüfung
					// ###### WEICHE: Broadcasts und normal
					M3SProtocol usedM3SProtocol = this.ExtractProtocol(rToCheck); // extrahiert Protokoll aus dem Controlbyte
					
					if(usedM3SProtocol == M3SProtocol.Invalid)
					{
						// Irgendwas ging schief, return
						oErrorCode = -29; // ungültige Protokollnummer
						return(false);
					}
					
					// Protokoll konnte konvertiert werden
					
					// Überprüfen der Slaveadresse, bei Broadcasts usw. muss sie 0 sein, sonst gültig
					
					if(usedM3SProtocol == M3SProtocol.Reset  || usedM3SProtocol == M3SProtocol.BroadCast  || usedM3SProtocol == M3SProtocol.CommandBroadcast)
					{
						if(rToCheck[1] != 0)
						{
							if((usedM3SProtocol == M3SProtocol.BroadCast) || (usedM3SProtocol == M3SProtocol.CommandBroadcast)) // Letzte Chance: Multicast im Broadcastprotokoll
							{
								if((rToCheck[0] & 0x02) != 0x02) // nein, auch kein Multicast, Pech gehabt...
								{
									oErrorCode = -30; // Broadcasts erfordern Addresse 0
									return(false);
								}
							}
							else
							{
								oErrorCode = -30; // Broadcasts erfordern Addresse 0
								return(false);
							}
							
						}
					}
					else
					{
						// Andere Protokolle brauchen gültige Slaveadresse
						oErrorCode = TBL.Check.CheckM3SSlaveAddress((int)(rToCheck[1]));
					
						if(oErrorCode != 0) // wenn M3s-SlaveAddresse nicht gepasst hat...
						{
							return(false);
						}
					}
				
					#endregion			
									
				}
								
				oErrorCode = 0; // no error
				return(true);
			}
			
			/// <summary>
			/// Prüft, ob es sich um ein Acknowledgeframe handelt und wenn ja, ob es ein Acknowledge oder Not Acknowledge bedeutet
			/// </summary>
			/// <param name="pSent"></param>
			/// <param name="pRec"></param>
			/// <returns>
			/// Acknowledge / not Acknowledge (true / false)
			/// </returns>
			/// <exception cref="Exceptions.FrameError">Ungültiges Datensatzformat</exception>
			/// <remarks>
			/// <para>
			/// 	<para>Interne <see cref="EDOLLHandler">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (Exception) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>EDOLLHandler.GetLastError();</c></para> 
			/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHandler.getLastError()</see>)
			/// </para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">EDOLL-Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Unültiges Frame</description></item>
			/// 	<item><term>-21</term><description>Acknowledgeframe ist von falschem Slave</description></item>
			/// 	<item><term>-22</term><description>Das zu prüfende Frame ist kein Acknowledgeframe (Protokollnummer falsch)</description></item>
			/// 	<item><term>-23</term><description>Masteradresse oder Datenrichtung ist falsch</description></item>
			/// </list>
			/// </remarks>			
			public bool IsAcknowledge(byte[] pSent, byte[] pRec, out int oErrorCode)
			{
				if(pRec.Length != AcknowledgeFrameLength)
				{					
					oErrorCode = -19;
					return(false);
				}			
				
				int chkSum = Convert.ToInt32(pRec[0] + pRec[1]); // calc
				int chkSumRec = Convert.ToInt32(pRec[2] <<16) | Convert.ToInt32(pRec[3] << 8) | Convert.ToInt32(pRec[4]); // received
				chkSumRec ^= M3S_V1_Dataframe.CheckSumCode;			
				
				if(chkSum != chkSumRec)
				{
					oErrorCode = -19;
					return(false);
				}
				
				// Ok, Frame ist gültig... check obs das richtige Acknowledge is..
				if(pSent[1] != pRec[1])
				{
					
					oErrorCode = -21;
					return(false);
				}
				
				if((pRec[0] & 0xF0) != ((Convert.ToInt32(M3SProtocol.Acknowledge) << 4) & 0xF0))
				{

					
					oErrorCode = -22;
					return(false);
				}				
				
				
				if(((pRec[0] | 0x02) & 0x0F) != ((pSent[0] | 0x02) & 0x0F)) // Masteradresse, Datenrichtung und Acknowledgebit stimmt nicht...
				{					
					oErrorCode = -23;
					return(false);
				}
				
				oErrorCode = 0;
				
				if((pRec[0] & 0x02) == 0x02) // Wenn Acknowledgebit gesetzt
				{
					return(true);	
				}
				else
				{
					oErrorCode = -36; // Explizites NAK
					return(false);
				}					
				
			}
				
			
			public bool IsImplicitAcknowledge(byte[] pSent, byte[] pRec, out int oErrorCode)
			{
				oErrorCode = 0;
				return(false);	// Version 1 unterstützt keine impliziten Acknowledges
			}

			/// <summary>
			/// Prüft ob es sich beim übergebenen Frame um ein gültiges M3S-Frame handelt
			/// </summary>
			/// <param name="toCheck">Zu prüfendes Frame</param>
			/// <returns>true / false</returns>
			/// <remarks>Die Methode prüft die Gültigkeit nach M3S des Bytestreams durch Bilden der Prüfsumme und anschließendem Vergleich mit der aus dem Frame extrahierten und decodierten Prüfsumme. Die Sinnhaftigkeit von Controlbyte, upperBound und Slaveadresse wird dabei <b>nicht</b> geprüft</remarks>
			public bool IsFrame(byte[] toCheck)
			{
				if(toCheck == null)
				{
					return(false);
				}
				
				
				int checksum = 0;
				int laenge = toCheck.GetUpperBound(0)+1;
										
				checksum = 0;
				for(int i=0; i<(laenge-3); i++)
				{
					checksum += Convert.ToInt32(toCheck[i]);
				}			
				
				int chkSumRec = Convert.ToInt32(toCheck[toCheck.GetUpperBound(0)-2] <<16) | Convert.ToInt32(toCheck[toCheck.GetUpperBound(0)-1] << 8) | Convert.ToInt32(toCheck[toCheck.GetUpperBound(0)]); // received
								
				chkSumRec ^= M3S_V1_Dataframe.CheckSumCode;
				
				
				if(chkSumRec == checksum)
				{
					return(true);
				}
				else
				{
					return(false); // Checksum error
				}				
			}
			
			
			
			#region Extraction			
			/// <summary>
			/// Extrahiert die Masteradresse aus einem Datenframe
			/// </summary>
			/// <param name="pFrame">Gültiges M3S-Datenframe</param>
			/// <returns>
			/// 	<list type="table">
			/// 		<item><term>0...3</term><description>Regulär</description></item>
			/// 		<item><term>-1</term><description>im Fehlerfall</description></item>
			/// 	</list>
			/// </returns>
			/// <remarks>
			/// <para>
			/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
			/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// 	</para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
			/// </list>
			/// </remarks>
			/// <example> Extrahieren aus byte[] packedFrame = IM3S_Dataframe.getDataframe();
			/// <c>Console.WriteLine("Masteradresse im Paket: " + m3sHandler.ExtractMasterAddress(packedFrame).ToString());</c>
			/// </example> 
			/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>			
			public int ExtractMasterAddress(byte[] pFrame)
			{
				if(!this.IsFrame(pFrame))
				{
					EDOLLHandler.Error(-19); // Frameerror
					Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
					throw ex;					
				}
				
				return (pFrame[0] >> 2) & 0x03; // muss sich eh immer zwischen 0 und 3 befinden...				
			}		
			
			/// <summary>
			/// Extrahiert das Protokoll aus einem Datenframe
			/// </summary>
			/// <param name="pFrame">Gültiges M3S-Datenframe</param>
			/// <returns>
			/// <list type="table">
			/// <item><term><see cref="M3SProtocol">M3SProtocol</see></term><description>Bei gültiger Konversion</description></item>
			/// <item><term><see cref="M3SProtocol.Invalid">M3SProtocol.Invalid</see></term><description>im Fehlerfall</description></item>
			/// </list>
			/// </returns> 
			/// <remarks>
			/// Das Protokoll wird aus dem higher Nibble des Controlbytes extrahiert und in die Enumeration <see cref="M3SProtocol">M3SProtocol</see> gecastet.
			/// /// <para>
			/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
			/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// 	</para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
			/// </list>
			///  (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// </remarks>
			/// <example> Extrahieren aus byte[] packedFrame = IM3S_Dataframe.getDataframe();
			/// <c>Console.WriteLine("Protokoll des Pakets: " + m3sHandler.ExtractProtocol(packedFrame).ToString());</c>
			/// </example> 
			/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>
			public M3SProtocol ExtractProtocol(byte[] pFrame)
			{				
				if(!this.IsFrame(pFrame))
				{
					EDOLLHandler.Error(-19); // Frameerror
					Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
					throw ex;					
				}
				
				M3SProtocol toReturn;
				
				try
				{
					toReturn = (M3SProtocol)(Convert.ToInt32(pFrame[0]) >> 4);
				}
				catch
				{
					toReturn = M3SProtocol.Invalid;
				}
								 
				return(toReturn);
			} 
			
			/// <summary>
			/// Extrahiert das Protokoll aus einem (möglichen) Controlbyte. Intern keinerlei Sicherheitsabfragen ob es sich um ein gültiges Controlbyte handelt. 
			/// </summary>
			/// <param name="pCtrlByte">Controlbyte, aus dem Extrahiert werdne soll</param>
			/// <returns>
			/// <list type="table">
			/// <item><term><see cref="M3SProtocol">M3SProtocol</see></term><description>Bei gültiger Konversion</description></item>
			/// <item><term><see cref="M3SProtocol.Invalid">M3SProtocol.Invalid</see></term><description>im Fehlerfall</description></item>
			/// </list> 
			///</returns>
			public M3SProtocol ExtractProtocol(byte pCtrlByte)
			{	
				M3SProtocol toReturn;				
				try
				{
					toReturn = (M3SProtocol)(Convert.ToInt32(pCtrlByte) >> 4);
				}
				catch
				{
					toReturn = M3SProtocol.Invalid;
				}
								 
				return(toReturn);
			} 
			
					
			/// <summary>
			/// Extrahiert die Nutzdatenbytes aus einem M3S-Rahmen
			/// </summary>
			/// <param name="pFrame">Gültiges M3S-Datenframe</param>
			/// <returns>
			/// <list type="table">
			/// <item><term>NutzdatenbyteArray</term><description>Bei gültiger Konversion</description></item>
			/// <item><term>null</term><description>Bei Acknowledgeframes</description></item>
			/// <item><term>null</term><description>im Fehlerfall</description></item>
			/// </list>
			/// </returns> 
			/// <remarks>
			/// Das übergebene Datenframe wird erst auf Gültigkeit überprüft, anschließend werden die Rahmendaten weggeschnitten (vorne und hinten je 3 Byte) und das Nutzdatenpaket zurückgegeben.
			/// <para>
			/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>string EDOLLHandler.GetLastError();</c></para>
			/// 	</para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
			/// </list>
			///  (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// </remarks>
			/// <example> Extrahieren aus byte[] packedFrame = IM3S_Dataframe.getDataframe(); und zweites Nutzdatenbyte ausgeben
			/// <c>Console.WriteLine("Zweites Nutzdatenbyte in packedFrame: " + string.Format("0x{0:x02}", m3sHandler.ExtractPayload(packedFrame)[1])); </c>
			/// </example> 
			/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception> 
			public byte[] ExtractPayload(byte[] pFrame)
			{
				if(!this.IsFrame(pFrame))
				{
					EDOLLHandler.Error(-19); // Frameerror
					Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
					throw ex;					
				}

				if(pFrame.Length == 5)
				{
					// ack
					return(null);
				}
				
				byte[] toReturn = new byte[pFrame.Length -6];
				
				for(int i=3; i<pFrame.Length - 3; i++)
				{
					toReturn[i-3] = pFrame[i];
				}
				
				return(toReturn);
			}
			
			
			#endregion
						
			#region Build Frames
			
				/// <summary>
				/// Erstellt aufgrund von pStatus eine versandfertige byteFolge mit der entsprechenden Kommandorückmeldung (siehe <see cref="m3sExecutionError">m3sExecutionError</see>) im einzigen Nutzdatenbyte. 
				/// </summary>
				/// <param name="pStatus">Ausführungsstatus des Kommandos (<see cref="m3sExecutionError">m3sExecutionError</see>)</param>
				/// <param name="pCommandFrame">Frame, aufgrund dessen ein Kommando ausgeführt wurde</param>
				/// <returns>
				/// <list type="table">
				/// <item><term>versandfertiges ByteArray</term><description>bei fehlerfreier Ausführung</description></item>
				/// <item><term>Exception</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns> 
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
				/// </list>
				/// (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
				/// </remarks>
				/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>
				public byte[] GetCommandExecutionStateFrame(m3sExecutionError pStatus, byte[] pCommandFrame)
				{
					byte[] data = new byte[1];
					
					if(!this.IsFrame(pCommandFrame))
					{
						EDOLLHandler.Error(-19); // Frameerror
						Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
						throw ex;					
					}				
					
					data[0] = Convert.ToByte(pStatus);					
					IM3S_Dataframe toReturn = this.CreateFrame(Convert.ToInt32(pCommandFrame[1]), M3SProtocol.CommandResponse,this.ExtractMasterAddress(pCommandFrame), data, true, false);
					
					return(toReturn.GetDataframe());				
				}	

				/// <summary>
				/// Liefert ein Kommandoframe, das eine Übertragungsratenänderung im Slave bewirkt.
				/// </summary>
				/// <param name="vSlaveAddr">Slaveadresse (oder Multicastadresse), 1...255</param>
				/// <param name="vBaudrate">Neue Baudrate in bps</param>
				/// <param name="vBroadcast">Befehl als Broadcast versenden?</param>
				/// <param name="vMulticast">Wenn als Broadcast versendet; als Multicast versenden?</param>
				/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
				/// <returns>
				/// <list type="table">
				///	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
				/// 	<item><term>Bytefolge</term><description>Versandfertiges Kommandoframe</description></item>
				/// 	<item><term>null</term><description>Im Fehlerfall, Details über den Ausgabeparameter oErrorCode</description></item>
				/// </list>
				/// </returns>
				public IM3S_Dataframe GetBaudrateChangeFrame(int vSlaveAddr, int vBaudrate, bool vBroadcast, bool vMulticast, out int oErrorCode)
				{
					if(!vMulticast)
					{
						oErrorCode = Check.CheckM3SSlaveAddress(vSlaveAddr);
					}
					else
					{
						oErrorCode = 0;
					}
					
					if(oErrorCode != 0)
					{
						return(null);
					}
					
					M3SProtocol prot ;
					
					if(vBroadcast)
					{
						prot = M3SProtocol.CommandBroadcast;
					}
					else
					{
						prot = M3SProtocol.Command;
						vMulticast = false;
					}
					
					if(vBroadcast && !vMulticast)
					{
						vSlaveAddr = 0;
					}
					
					byte[] cmd = new byte[5];
					cmd[0] = (byte) M3SCommand.ChangeBaudrate;
					cmd[1] = (byte)((vBaudrate >> 24) & 0xff);
					cmd[2] = (byte)((vBaudrate >> 16) & 0xff);
					cmd[3] = (byte)((vBaudrate >> 8) & 0xff);
					cmd[4] = (byte)((vBaudrate >> 0) & 0xff);
					
					IM3S_Dataframe frame = this.CreateFrame(vSlaveAddr,prot,masteraddr, cmd,true,vMulticast);
					
					return(frame);
				}

				/// <summary>
				///  Erstellt aufgrund von pStatus eine versandfertige byteFolge mit der entsprechenden Kommandorückmeldung (siehe <see cref="m3sExecutionError">m3sExecutionError</see>) im ersten Nutzdatenbyte und zusätzlichen Informationen (nachfolgende Nutzdatenbytes)
				/// </summary>
				/// <param name="pStatus">Ausführungsstatus des Kommandos (siehe <see cref="m3sExecutionError">m3sExecutionError</see>)</param>
				/// <param name="pCommandFrame">Frame, aufgrunddessen das Kommando ausgeführt wurde</param>
				/// <param name="pInfo">Informationsdatenbytes (max. 255) als byte[], auch null kann übergeben werden.</param>
				/// <returns>
				/// <list type="table">
				/// <item><term>versandfertiges ByteArray</term><description>bei fehlerfreier Ausführung</description></item>
				/// <item><term>Exception</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// Die Funktion kann zusätzliche als Parameter übergebene Informationsbytes an den Kommandosender zurückliefern. Diese werden als byte[] übergeben und hinten an das ExecutionStatus-Byte angehängt. 
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
				/// 	<item><term>-612</term><description>Es wurden zu viele Informationsbyte (>255) übergeben.</description></item>
				/// </list>
				/// (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
				/// </remarks>
				/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>
				public byte[] GetCommandExecutionStateFrame(m3sExecutionError pStatus, byte[] pCommandFrame, byte[] pInfo)
				{				
					if(pInfo == null)
					{
						return(this.GetCommandExecutionStateFrame(pStatus,pCommandFrame)); // Version ohne Info aufrufen..
					}
					
					if(!this.IsFrame(pCommandFrame))
					{
						EDOLLHandler.Error(-19); // Frameerror
						Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
						throw ex;					
					}		

					if(pInfo.Length > 255)
					{
						EDOLLHandler.Error(-612);
						Exceptions.ConversionException ex2 = new TBL.Exceptions.ConversionException("Es wurden zu viele Informationsbytes (" + pInfo.Length.ToString() + ") übergeben an: TBL.Communication.Protocol.m3sHandler.GetCommandExecutionStateFrame(). max 255 erlaubt!");
						throw ex2;
					}
					
					byte[] data = new byte[1 + pInfo.Length];
					
					data[0] = Convert.ToByte(pStatus); // Kommando ins Frame einfügen
					
					for(int i=0; i<pInfo.Length; i++)	// Informationsbytes einfügen
					{
						data[i+1] = pInfo[i];
					}					
								
					IM3S_Dataframe toReturn = this.CreateFrame(Convert.ToInt32(pCommandFrame[1]), M3SProtocol.CommandResponse,this.ExtractMasterAddress(pCommandFrame), data, true, false);
					
					return(toReturn.GetDataframe());				
				}				
				
				/// <summary>
				/// Bildet ein DataUnitAmount-Requestframe (früher Pixelrequestframe) an übergebene Slaveadresse
				/// </summary>
				/// <param name="pSlaveAddr">Slaveadresse, von dem DataUnit-Anzahl (Pixel) gelesen werden sollen</param>
				/// <returns>Versandfertiges byte[]-Frame</returns>
				public byte[] GetCommandDataUnitAmountRequest(int pSlaveAddr)
				{
					IM3S_Dataframe cmdFrame;
					byte[] cmdData = new byte[1];
					cmdData[0] = (byte)M3SCommand.DataUnitAmountRequest;
					
					cmdFrame = this.CreateFrame(pSlaveAddr,M3SProtocol.Command, masteraddr,cmdData,true,false);
					
					return(cmdFrame.GetDataframe());
				}		
				
				/// <summary>
				/// Übernimmt Datei als byte[] und packt - über mehrere Aufrufe hinweg - die Datei in M3S-Gerechte durchnummerierte Pakete
				/// </summary>
				/// <param name="pAddr">Adresse, an die gesendet werden soll</param>
				/// <param name="pFileContent">Zu sendende Datei als byte[]</param>
				/// <param name="oErrorCode">Ausgabeparameter mit EDOLL Fehlernummer</param>
				/// <returns>
				/// <list type="table">
				/// <listheader><term>Return</term><description>Beschreibung</description></listheader>
				/// <item><term>byte[] m3sDatenframe</term><description>Versandfertiges Datenpaket</description></item>
				/// <item><term></term></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-602</term><description>Ungültige Adresse (siehe <see cref="Check.CheckM3SSlaveAddress">Check. CheckM3SSlaveAddress</see></description></item>
				/// </list>
				/// (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLERGgetLastError()</see>)
				/// </remarks>
				/// <exception cref="TBL.Exceptions.InvalidSlaveAddress">Ungültige Slaveadresse übergeben</exception>
				public IM3S_Dataframe[] GetFileTransferFrames(int pAddr, byte[] pFileContent, out int oErrorCode)
				{	
					throw new NotImplementedException("Implementierung muss erst überarbeitet werden...");
					
					/*
					// TODO: Check ob noch immer der gleiche Bytestream...					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pAddr)))
					{
						TBL.Exceptions.InvalidSlaveAddress ex = new TBL.Exceptions.InvalidSlaveAddress("Ungültige Slaveadresse '" +  pAddr.ToString() + "'");
						throw ex;						           
					}					
					
					if(fileTransferPosition >= pFileContent.GetUpperBound(0))
					{
						fileTransferPosition = 0; // reset
						fileTransferPackage = 1;
						return(null); // abgearbeitet, keine weiteren Frames
					}
					else
					{
						// Es gibt 252 Nutzdaten, 4 Paketinfodaten
						if(pFileContent.GetUpperBound(0) <= (255-packageNumberLength)) 		// ############### wenn Die Datei kleiner als eine Paketlänge ist
						{
							byte[] toSend = new byte[pFileContent.GetUpperBound(0)+1+packageNumberLength];					
							
							// Packagenumber
							toSend[0] = 0; // MSB
							toSend[1] = 0;
							toSend[2] = 0;
							toSend[3] = Convert.ToByte(fileTransferPackage); // LSB
							
							for(int i=0; i<=pFileContent.GetUpperBound(0); i++) // Nutzdaten veracken..
							{
								toSend[i+packageNumberLength] = pFileContent[i];
								fileTransferPosition = i;				// TODO ++ verwenden!
							}
							
							// in Frame packen							
							IM3S_Dataframe frame = this.CreateFrame(pAddr, M3SProtocol.FileTransfer,masteraddr,toSend, true, false);
							
							fileTransferPackage++; // erhöhen, damit die Funktion das nächste mal null liefert und so signalisiert dass Datei raus is...
							return(frame.GetDataframe());
						}
						else // ############################################################### Wenn mehrere Pakete gebraucht werden
						{
							byte[] toSend;
							
							if((pFileContent.GetUpperBound(0)+packageNumberLength)-fileTransferPosition > 255) // wenn Verbleibende Bytes + Paketinfo nicht in einem Frame platz haben
							{
								toSend = new byte[256];
							}
							else // Wenn ich beim letzten Paket angekommen bin
							{
								toSend = new byte[(pFileContent.Length - fileTransferPosition) + packageNumberLength]; // Brauch ich verbleibende Bytes + Paketinfoplatz
							}
							
							// Paketnummer zuweisen							
							toSend[0] = Convert.ToByte((fileTransferPackage >> 24) & 0xff); // MSB
							toSend[1] = Convert.ToByte((fileTransferPackage >> 16) & 0xff);
							toSend[2] = Convert.ToByte((fileTransferPackage >> 8) & 0xFF);
							toSend[3] = Convert.ToByte(fileTransferPackage & 0xff); // LSB							
							
							
							for(int i=(int)fileTransferPosition; (i<=pFileContent.GetUpperBound(0)) && i<((256-packageNumberLength)*(fileTransferPackage-1))+(256-packageNumberLength); i++)
							{
								toSend[i-((256-packageNumberLength)*(fileTransferPackage-1))+packageNumberLength] = pFileContent[i]; // An richtiger STelle einfügen
								fileTransferPosition = i; // TODO ++?
							}
							
							fileTransferPosition++; // auf nächstes Byte zeigen für nächsten Methodendurchlauf
							
							
							IM3S_Dataframe frame = this.CreateFrame(pAddr, M3SProtocol.FileTransfer,masteraddr,toSend, true, false);
							fileTransferPackage++;						
							
							return(frame.GetDataframe());
						}
					}
					*/
				}					
				
			
				
				
				#region getFrames							
			
				/// <summary>
				/// 
				/// </summary>
				/// <param name="pAddr">Adresse des Slaves, von dem empfangen werden soll</param>
				/// <returns>
				/// <list type="table">
				/// <item><term>versandfertiges ByteArray</term><description>Requestframe bei fehlerfreier Ausführung</description></item>
				/// <item><term>null</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns> 
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-602</term><description>Ungültige Adresse (siehe <see cref="Check.CheckM3SSlaveAddress">Check. CheckM3SSlaveAddress</see></description></item>
				/// </list>
				/// </remarks>
				/// <exception cref="TBL.Exceptions.InvalidSlaveAddress">Ungültige Adresse übergeben</exception>
				public byte[] GetColorAndBrightnessRequestFrame(int pAddr)
				{
					IM3S_Dataframe requestFrame;
					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pAddr)))
					{
						TBL.Exceptions.InvalidSlaveAddress ex = new TBL.Exceptions.InvalidSlaveAddress("Ungültige Slaveadresse '" +  pAddr.ToString() + "'");
						throw ex;						           
					}
					
					byte[] rData = new byte[1];
					rData[0] = Convert.ToByte(M3SCommand.ColorAndBrightnessRequest);
					
					requestFrame = this.CreateFrame(pAddr, M3SProtocol.Command,masteraddr,rData,true,false);
					
					byte[] returnDataFrame = requestFrame.GetDataframe();
					
					return(returnDataFrame);
				}
			
				/// <summary>
				/// Liefert Kommandoframe für einen FileRequest an bestimmte Adresse
				/// </summary>
				/// <param name="pAddr">Adresse, an die der Request gehen soll</param>
				/// <param name="fileName">max. 255 Bytes Dateiname</param>
				/// <returns>
				/// <list type="table">
				/// <listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
				/// <item><term>Versandfertiges Dateianforderungsframe</term><description>beinhaltet Dateinamen / Speicherort im Empfänger. (UTF-8 codiert)</description></item>
				/// <item><term>null</term><description>Im Fehlerfall (siehe Remarks)</description></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.getLastError()</see>)
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>0</term><description>Fehlerfreie Ausführung</description></item>
				/// 	<item><term>-602</term><description>Ungültige M3S-Slaveadresse</description></item>
				/// </list>
				/// </remarks>
				public IM3S_Dataframe GetFileRequestFrame(int pAddr, string fileName)
				{					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pAddr))) // Prüft adresse und legt Errorcode ggf. im Edollhandler ab
					{
						return(null);
					}
					
					IM3S_Dataframe requestFrame;
					byte[] rData = new byte[1 + fileName.Length];
					byte[] codedFilename = System.Text.Encoding.UTF8.GetBytes(fileName);
					rData[0] = Convert.ToByte(M3SCommand.FileRequest);
					
					for(int i=0; i<=codedFilename.GetUpperBound(0); i++)
					{
						rData[i+1] = codedFilename[i];
					}
					
					requestFrame = this.CreateFrame(pAddr, M3SProtocol.Command,masteraddr,rData,true,false);
					
					return(requestFrame);					
				}
				
				/// <summary>
				/// Liefert Kommandoframe für nachfolgenden Dateitransfer an bestimmten Slave. 
				/// </summary>
				/// <param name="pSlaveAddr">M3S-Slaveadresse des Empfängers</param>
				/// <param name="pFileLength">Dateigröße in Bytes</param>
				/// <param name="pTargetFileName">Soll-Speicherpfad beim Empfänger</param>
				/// <returns>
				/// <list type="table">
				/// <listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
				/// <item><term>Versandfertiges Ankündigungsframe</term><description>beinhaltet in wie vielen Paketen die Datei geschickt wird und den Speicherort im Empfänger (UTF-8 codiert)</description></item>
				/// <item><term>null</term><description>Im Fehlerfall (siehe Remarks)</description></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// Detaillierte Informationen zum Ablauf von Filetransfers siehe <a href="doc/M3SProtocol.htm">M3S-Protokolldokumentation</a>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.getLastError()</see>)
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>0</term><description>Fehlerfreie Ausführung</description></item>
				/// 	<item><term>-602</term><description>Ungültige M3S-Slaveadresse</description></item>
				/// 	<item><term>-613</term><description>ÜBergebener Dateiname zu lang (länger als 251 Zeichen)</description></item>
				/// </list>
				/// </remarks>
				public IM3S_Dataframe GetFileTransferAnnouncementFrame(int pSlaveAddr, int pFileLength, string pTargetFileName)
				{
					IM3S_Dataframe annFrame; // Das Frame wird erzeugt
					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pSlaveAddr))) // Prüft adresse und legt Errorcode ggf. im Edollhandler ab
					{
						return(null);
					}	

					// else	
					byte[] codedFilename = null;					
					
					
					codedFilename = System.Text.Encoding.UTF8.GetBytes(pTargetFileName); // Dateiname codieren
										
					if(codedFilename.Length > (256-packageNumberLength-1)) // pTargetFilename länger als Nutzdatenbytes - Paketinfodaten - Befehl
					{
						EDOLLHandler.Error(-613);
						return(null);
					}
					
					// Ermitteln der Datenpaktanzahl
					int paketzahl = (int)(Math.Ceiling((float)pFileLength / (float)(256-packageNumberLength)));					
					
					byte[] rData = new byte[1 + packageNumberLength + codedFilename.Length]; // Kommando, Paketzahl und NAme
					
					
					rData[0] = Convert.ToByte(M3SCommand.FileAnnouncement); // Kommando Dateitransfer Ankündigen
					
					// Anzahl der nachfolgenden Pakete zuweisen
					rData[1] = Convert.ToByte((paketzahl >> 24) & 0xFF);
					rData[2] = Convert.ToByte((paketzahl >> 16) & 0xFF);
					rData[3] = Convert.ToByte((paketzahl >> 8) & 0xFF);
					rData[4] = Convert.ToByte(paketzahl & 0xFF);
					
					
					for(int i=0; i<=codedFilename.GetUpperBound(0); i++)
					{
						rData[i+1+packageNumberLength] = codedFilename[i];
					}
					
					annFrame = this.CreateFrame(pSlaveAddr, M3SProtocol.Command,masteraddr, rData, true, false);
					
					return(annFrame);
					
					
				}
				
				/// <summary>
				/// Erstellt ein Acknowledgeframe auf einen bestimmten Datenrahmen (Argument 2)
				/// </summary>
				/// <param name="pAck">Acknowledge ja / nein (true / false)</param>
				/// <param name="pFrame">Datenrahmen, der acknowledged werden soll</param>
				/// <returns>
				/// Versandbereites Acknowledgeframe
				/// </returns>
				/// <exception cref="Exceptions.FrameError">Ungültiges Datenframe übergeben</exception>
				/// <exception cref="Exceptions.ConversionException">Aus übergebenem Datenframe kann kein Acknowledgeframe erstellt werden.</exception>
				public IM3S_Dataframe GetAcknowledgeFrame(bool pAck, byte[] pFrame)
				{					
					if(!this.IsFrame(pFrame))
					{
						Exceptions.FrameError ex = new TBL.Exceptions.FrameError("In Argument 2 übergebenes Dataframe ist ungültig: " + TBLConvert.BytesToHexString(pFrame));
						throw ex;
					}
					
					IM3S_Dataframe ackFrame = this.CreateFrame(Convert.ToInt32(pFrame[1]), M3SProtocol.Acknowledge, this.ExtractMasterAddress(pFrame), null, true, true);
					
					if(ackFrame == null)
					{
						Exceptions.ConversionException ex = new Exceptions.ConversionException("Aus dem übergebenen Dataframe kann kein Acknowledgeframe erstellt werden." + Environment.NewLine + TBLConvert.BytesToHexString(pFrame));
						throw ex; 
					}					
					
					return ackFrame;
				}
				
				/// <summary>
				/// Liefert eine die Bus-Resetfolge gem. M3S-Protokoll zurück. 
				/// </summary>
				/// <returns>				
				/// <list type="table">
				/// <item><term>versandfertiges ByteArray</term><description>Requestframe bei fehlerfreier Ausführung</description></item>
				/// <item><term>null</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns> 
				/// <remarks>
				/// <para>Der Resetrahmen ist eine fix vorgegebene Bytefolge, der jedoch trotzdem ein gültiges Datenpaket darstellt, um die Prüfsumme bilden zu können und so zusätzliche Sicherheit zum Vorliegen eines gültigen Reset zu gewinnen.</para>
				/// <para>Resetfolge: 0x10 0x00 0x00 0x52 0x54 0x4F 0x2F</para>
				/// </remarks>
				public IM3S_Dataframe GetResetFrame()
				{
						// Build frame
						IM3S_Dataframe resetFrm;
						byte[] rData = new byte[1];
						rData[0] = Convert.ToByte(M3SCommand.Reset);
						
						resetFrm = this.CreateFrame(0,M3SProtocol.Reset,0,rData,true,false);
							
						
						return(resetFrm);								
				}
				
				/// <summary>
				/// Kommandobytefolge an Slave: "Update" 
				/// </summary>
				/// <param name="pAddr"></param>
				/// <param name="vAcknowledgeRequest"></param>
				/// <returns>
				/// <list type="table">
				/// <item><term>versandfertiges Frame</term><description>Requestframe bei fehlerfreier Ausführung</description></item>
				/// <item><term>null</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns> 
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-602</term><description>Ungültige Adresse (siehe <see cref="Check. CheckM3SSlaveAddress">Check. CheckM3SSlaveAddress</see></description></item>
				/// </list>
				/// </remarks>
				public IM3S_Dataframe GetUpdateFrame(int pAddr, bool vAcknowledgeRequest)
				{
					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pAddr)))
					{
						return(null);
					}
					
					byte[] cmdData = new byte[1];
					cmdData[0] = Convert.ToByte(M3SCommand.Update);
					
					IM3S_Dataframe toReturn = this.CreateFrame(pAddr,M3SProtocol.Command, masteraddr, cmdData, true, vAcknowledgeRequest);
					
					return(toReturn);
				}
				#endregion
			
			#endregion
						
			/// <summary>
			/// Standardkonstruktor, macht eigentlich nix ;)
			/// </summary>
			public M3S_V1_Handler()
			{
				
			}				
		}
		
		
		/// <summary>
		/// Der M3S_V2_Handler ist das Protokollinterface für M3S Version 2.x. Datenextraktion/-zusammenführung + Befehlsbildung findet in der Instanz dieser Klasse statt.
		/// </summary>
		/// <remarks>
		/// <para>Der Handler stellt im Wesentlichen die Schnittstelle zwischen Datenframe und DevCom her. Hier werden Datenpakete erstellt und ausgewertet. Außerdem sind einige höhere Methoden (zum Beispiel das Splitten von Dateien in Datenframes) direkt implementiert (u.a. mit Festplattenzugriff).</para>
		/// <para></para>
		/// </remarks>
		public class M3S_V2_Handler: M3S_Handler, IM3S_Handler
		{			
			private int masteraddr	= 1;
			

			private const int packageNumberLength = 4;	
			
			/// <summary>
			/// Framelänge des größtmöglichen Frames in Bytes
			/// </summary>
			public int MaximumFrameLength
			{
				get
				{
					return M3S_V2_Dataframe.MaximumFrameLength;
				}
			}
			
			
			
			/// <summary>
			/// Länge eines Acknowledgeframes in Bytes
			/// </summary>
			public int AcknowledgeFrameLength
			{
				get
				{
					return(M3S_V2_Dataframe.AcknowledgeFrameLength);
				}
			}
			
			/// <summary>
			/// Länge des kürzestmöglichen Frames (i.d.R. AcknowledgeFrameLength)
			/// </summary>
			public int MinimumFrameLength
			{
				get
				{
					return(M3S_V2_Dataframe.MinimumFrameLength);
				}
			}
			
			/// <summary>
			/// Rahmendatenlänge in Byte
			/// </summary>
			public int Overhead
			{
				get
				{
					return(M3S_V2_Dataframe.Overhead);
				}
			}
			
			
			/// <summary>
			/// Erzeugt eine neue <see cref="M3S_V2_Dataframe">M3S_V2_Dataframe</see> Instanz
			/// </summary>
			/// <param name="vSlaveAddress">Am Transfer beteiligter Frame</param>
			/// <param name="vProtocol">Verwendetes Protokoll</param>
			/// <param name="vMasterAddress">Am Transfer beteiligter Master</param>
			/// <param name="rData">Payload</param>
			/// <param name="vMasterSend">Datenrichtung: MasterSend / !SlaveSEnd</param>
			/// <param name="vAcknowledge">
			///<list type="table">
			/// 	<listheader><term>Verwendetes Protokoll</term><description>Werte</description></listheader>
			/// 	<item><term>Acknowledge</term><description>True... Acknowledge, False ... Not Acknowledge></description></item>
			/// 	<item><term>Unicastprotokoll, Datenrichtung MasterSend</term><description>true ... Acknowledge Request, false ... Frame darf nicht quittiert werden</description></item>
			/// 	<item><term>Andere Protokolle</term><description>false übergeben</description></item>			
			/// </list>
			/// </param>
			/// <returns>Via <see cref="IM3S_Dataframe">IM3S_Dataframe</see> zugreifbare Instanz eines Datenframes</returns>			
			public IM3S_Dataframe CreateFrame(int vSlaveAddress, M3SProtocol vProtocol, int vMasterAddress, byte[] rData, bool vMasterSend, bool vAcknowledge)
			
			{
				return(new M3S_V2_Dataframe(vSlaveAddress,vProtocol,vMasterAddress,rData,vMasterSend,vAcknowledge));
			}
				
			
			/// <summary>
			/// Erstellt ein m3sDatenframe aus einem M3S-Bytestream
			/// </summary>
			/// <param name="rStream">Bytestream (gültiges M3S-Frame)</param>
			/// <param name="oErrorCode">Ausgabe: M3S-Errorcode (0 bei Korrekter Funktion)</param>
			/// <returns>
			/// <list type="table">
			/// <item><term>Objekt vom Typ <see cref="IM3S_Dataframe">m3sDataframe</see></term><description>bei fehlerfreier Ausführung</description></item>
			/// <item><term>null</term><description>im Fehlerfall (Errorcode über out int pErrorCode)</description></item>
			/// </list>
			/// </returns>
			public IM3S_Dataframe CreateFrameByBytestream(byte[] rStream, out int oErrorCode)
			{
				oErrorCode = -555; // common error
							
				if(!IsFrame(rStream, out oErrorCode))
				{
					// Errorcode wird über out gesetzt...
				 	return(null); 
				}
				// else Frame ist zumindest syntaktisch sinnvoll
				
				int tmpAddress = rStream[1];
				M3SProtocol protokoll = this.ExtractProtocol(rStream);
				int masterAddr = this.ExtractMasterAddress(rStream);
				byte[] nutzdaten = this.ExtractPayload(rStream);
				
				bool ack = false;
				bool send = true; // aus Mastersicht
				
				if((rStream[0] & 0x02) == 0x02) // Falls Acknowledgebit gesetzt
				{
					ack = true;	
				}
				
				if((rStream[0] & 0x01) == 0x01) // Falls SS-Bit gesetzt
				{
					send = false; // Slave hat gesendet	
				}
				
				IM3S_Dataframe tempFrame = this.CreateFrame(tmpAddress,protokoll, masterAddr, nutzdaten, send, ack);
				
				
				return(tempFrame); // Fehler
			}
			
			/// <summary>
				/// Liefert ein Kommandoframe, das eine Übertragungsratenänderung im Slave bewirkt.
				/// </summary>
				/// <param name="vSlaveAddr">Slaveadresse (oder Multicastadresse), 1...255</param>
				/// <param name="vBaudrate">Neue Baudrate in bps</param>
				/// <param name="vBroadcast">Befehl als Broadcast versenden?</param>
				/// <param name="vMulticast">Wenn als Broadcast versendet; als Multicast versenden?</param>
				/// <param name="oErrorCode">Ausgabeparameter: <see cref="EDOLLHandler">EDOLL-Fehlercode</see>, 0 bei fehlerfreier Übertragung</param>
				/// <returns>
				/// <list type="table">
				///	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
				/// 	<item><term>Bytefolge</term><description>Versandfertiges Kommandoframe</description></item>
				/// 	<item><term>null</term><description>Im Fehlerfall, Details über den Ausgabeparameter oErrorCode</description></item>
				/// </list>
				/// </returns>
				public IM3S_Dataframe GetBaudrateChangeFrame(int vSlaveAddr, int vBaudrate, bool vBroadcast, bool vMulticast, out int oErrorCode)
				{
					if(!vMulticast)
					{
						oErrorCode = Check.CheckM3SSlaveAddress(vSlaveAddr);
					}
					else
					{
						oErrorCode = 0;
					}
					
					if(oErrorCode != 0)
					{
						return(null);
					}
					
					M3SProtocol prot ;
					
					if(vBroadcast)
					{
						prot = M3SProtocol.CommandBroadcast;
					}
					else
					{
						prot = M3SProtocol.Command;
						vMulticast = false;
					}
					
					if(vBroadcast && !vMulticast)
					{
						vSlaveAddr = 0;
					}
					
					byte[] cmd = new byte[5];
					cmd[0] = (byte) M3SCommand.ChangeBaudrate;
					cmd[1] = (byte)((vBaudrate >> 24) & 0xff);
					cmd[2] = (byte)((vBaudrate >> 16) & 0xff);
					cmd[3] = (byte)((vBaudrate >> 8) & 0xff);
					cmd[4] = (byte)((vBaudrate >> 0) & 0xff);
					
					IM3S_Dataframe frame = this.CreateFrame(vSlaveAddr,prot,masteraddr, cmd,true,!(vMulticast || vBroadcast));
					
					return(frame);
				}


			/// <summary>
			/// Prüft ob es sich beim übergebenen Frame um ein gültiges M3S-Frame handelt und validiert die Rahmendaten
			/// </summary>
			/// <param name="rToCheck">Zu überprüfendes Frame</param>
			/// <param name="oErrorCode">Über diesen out-Parameter wird ein <see cref="EDOLLHandler">EDOLL-Fehlercode</see> zurückgegeben</param>
			/// <returns>
			/// true / false. Im Fehlerfall außerdem über den oErrorCode-Parameter EDOLL-Errorcodes
			/// </returns>
			/// <remarks>
			/// Die Funktion überprüft das M3S-Datenframe auf folgende Punkte (in dieser Reihenfolge, beim ersten Fehler wird abgebrochen)
			/// <list type="number">
			/// <item>Frame kürzer als Minimale M3S-Framelänge</item>
			/// <item>Checksum-Error (Fehlercode -27)</item>
			/// <item>upperBound stimmt mit Framelänge nicht überein (Fehlercode -28)</item>
			/// <item>Aufgrund von Framelänge als Acknowledgeframe identifiziertes Frame verwendet kein Acknowledgeprotokoll (Fehlercode -31)</item>
			/// <item>Protokollnr ist ungültig oder konnte nicht extrahiert werden (Fehlercode -29)</item>
			/// <item>Protokoll ist Broadcast / Resetframe, führt aber eine SlaveAdresse ungleich 0 (Fehlercode -30)</item>
			/// <item>Slaveadresse ist ungültig - außerhalb [1 255] (Fehlercode -602)</item>
			/// </list>
			/// </remarks>
			public bool IsFrame(byte[] rToCheck, out int oErrorCode)
			{
				oErrorCode = -555; // general Error
				
				if(rToCheck.Length < MinimumFrameLength)
				{
					oErrorCode = -32; // Frame zu kurz.. Dieser FEhler sollte an und für sich nur bei Analysen auftreten
				}
				
				bool chkSumOk = IsFrame(rToCheck);
				
				if(!chkSumOk)
				{
					oErrorCode = -27; // CRC M3S
					return(false);
				}
				// else
				
				if(rToCheck.Length == AcknowledgeFrameLength)
				{
					// Wenn Acknowledgeframe
					if(this.ExtractProtocol(rToCheck) == M3SProtocol.Acknowledge)
					{
						oErrorCode = 0; 
/*REGULAR EXIT*/   		return(true);
					}
					else
					{
						oErrorCode = -31;
						return(false);
					}
					
				}
				else
				{
					
					// Prüfen ob anderes Frame
					if(((int)(rToCheck[2])+1+ Overhead) != rToCheck.Length)
					{
						oErrorCode = -28; // upperBound stimmt nicht mit Framelänge überein
						return(false);
					}
					
					
					#region Protokoll und Slaveadressprüfung
					// ###### WEICHE: Broadcasts und normal
					M3SProtocol usedM3SProtocol = this.ExtractProtocol(rToCheck); // extrahiert Protokoll aus dem Controlbyte
					
					if(usedM3SProtocol == M3SProtocol.Invalid)
					{
						// Irgendwas ging schief, return
						oErrorCode = -29; // ungültige Protokollnummer
						return(false);
					}
					
					// Protokoll konnte konvertiert werden
					
					// Überprüfen der Slaveadresse, bei Broadcasts usw. muss sie 0 sein, sonst gültig
					
					if(usedM3SProtocol == M3SProtocol.Reset  || usedM3SProtocol == M3SProtocol.BroadCast  || usedM3SProtocol == M3SProtocol.CommandBroadcast)
					{
						if(rToCheck[1] != 0)
						{
							if((usedM3SProtocol == M3SProtocol.BroadCast) || (usedM3SProtocol == M3SProtocol.CommandBroadcast)) // Letzte Chance: Multicast im Broadcastprotokoll
							{
								if((rToCheck[0] & 0x02) != 0x02) // nein, auch kein Multicast, Pech gehabt...
								{
									oErrorCode = -30; // Broadcasts erfordern Addresse 0
									return(false);
								}
							}
							else
							{
								oErrorCode = -30; // Broadcasts erfordern Addresse 0
								return(false);
							}							
						}
					}
					else
					{
						// Andere Protokolle brauchen gültige Slaveadresse
						oErrorCode = TBL.Check.CheckM3SSlaveAddress((int)(rToCheck[1]));
					
						if(oErrorCode != 0) // wenn M3s-SlaveAddresse nicht gepasst hat...
						{
							return(false);
						}
					}
				
					#endregion			
									
				}
								
				oErrorCode = 0; // no error
				return(true);
			}
			
			
			/// <summary>
			/// Prüft ob es sich beim übergebenen Frame um ein gültiges M3S-Frame handelt
			/// </summary>
			/// <param name="toCheck">Zu prüfendes Frame</param>
			/// <returns>true / false</returns>
			/// <remarks>Die Methode prüft die Gültigkeit nach M3S des Bytestreams durch Bilden der Prüfsumme und anschließendem Vergleich mit der aus dem Frame extrahierten und decodierten Prüfsumme. Die Sinnhaftigkeit von Controlbyte, upperBound und Slaveadresse wird dabei <b>nicht</b> geprüft</remarks>
			public bool IsFrame(byte[] toCheck)
			{
				if(toCheck == null)
				{
					return(false);
				}
						
				return(crcHandler.FrameValid(toCheck));
				
			}
		
			Crc8 crcHandler = new Crc8(M3S_V2_Dataframe.CrcInitialValue);
			
			/// <summary>
			/// Prüft, ob es sich um ein Acknowledgeframe handelt und wenn ja, ob es ein Acknowledge oder Not Acknowledge bedeutet
			/// </summary>
			/// <param name="pSent"></param>
			/// <param name="pRec"></param>
			/// <returns>
			/// Acknowledge / not Acknowledge (true / false)
			/// </returns>
			/// <exception cref="Exceptions.FrameError">Ungültiges Datensatzformat</exception>
			/// <remarks>
			/// <para>
			/// 	<para>Interne <see cref="EDOLLHandler">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (Exception) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>EDOLLHandler.GetLastError();</c></para> 
			/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHandler.getLastError()</see>)
			/// </para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">EDOLL-Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Unültiges Frame</description></item>
			/// 	<item><term>-21</term><description>Acknowledgeframe ist von falschem Slave</description></item>
			/// 	<item><term>-22</term><description>Das zu prüfende Frame ist kein Acknowledgeframe (Protokollnummer falsch)</description></item>
			/// 	<item><term>-23</term><description>Masteradresse oder Datenrichtung ist falsch</description></item>
			/// </list>
			/// </remarks>			
			public bool IsAcknowledge(byte[] pSent, byte[] pRec, out int oErrorCode)
			{				
				
				if(pRec.Length != AcknowledgeFrameLength)
				{
					oErrorCode = -19;
					return(false);
				}							
								
				if(!crcHandler.FrameValid(pRec))
				{
					oErrorCode = -19;
					return(false);
				}
				
				
				// Ok, Frame ist gültig... check obs das richtige Acknowledge is..
				if(pSent[1] != pRec[1])
				{
					oErrorCode = -21;
					return(false);
				}
				
				if((pRec[0] & 0xF0) != ((Convert.ToInt32(M3SProtocol.Acknowledge) << 4) & 0xF0))
				{
					oErrorCode = -22;
					return(false);
				}				
				
				
				if((pRec[0] & 0x0C) != (pSent[0] & 0x0C))
				{
					oErrorCode = -23; // Masteradresse
					return(false);
				}
				
				if((pRec[0] & 0x01) != 0x01)
				{
					oErrorCode = -23;
					return(false);
				}
				
				oErrorCode = 0;
				if((pRec[0] & 0x02) == 0x02) // Wenn Acknowledgebit gesetzt
				{
					return(true);	
				}
				else
				{
					oErrorCode = -36; // Explizites NAK
					return(false);
				}					
				
			}
			
			public bool IsImplicitAcknowledge(byte[] pSent, byte[] pRec, out int oErrorCode)
			{								
				if(!crcHandler.FrameValid(pRec))
				{
					oErrorCode = -19;
					return(false);
				}
				
				// Ok, Frame ist gültig... check obs das richtige Acknowledge is..
				if(pSent[1] != pRec[1])
				{
					oErrorCode = -21; // Falsche Slaveadresse
					return(false);
				}
				
				if((pRec[0] & 0x0C) != (pSent[0] & 0x0C))
				{
					oErrorCode = -23; // Masteradresse
					return(false);
				}
				
				if((pRec[0] & 0x01) != 0x01)
				{
					oErrorCode = -23;	// Datenrichtung
					return(false);
				}
				
				oErrorCode = 0;
				if((pRec[0] & 0x02) == 0x02) // Wenn Acknowledgebit gesetzt
				{
					return(true);	
				}
				else
				{
					// Implizites NAK...
					oErrorCode = -37;
					return(false);
				}	
				
			}
						
			
			#region Extraction			
			/// <summary>
			/// Extrahiert die Masteradresse aus einem Datenframe
			/// </summary>
			/// <param name="pFrame">Gültiges M3S-Datenframe</param>
			/// <returns>
			/// 	<list type="table">
			/// 		<item><term>0...3</term><description>Regulär</description></item>
			/// 		<item><term>-1</term><description>im Fehlerfall</description></item>
			/// 	</list>
			/// </returns>
			/// <remarks>
			/// <para>
			/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
			/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// 	</para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
			/// </list>
			/// </remarks>
			/// <example> Extrahieren aus byte[] packedFrame = IM3S_Dataframe.getDataframe();
			/// <c>Console.WriteLine("Masteradresse im Paket: " + m3sHandler.ExtractMasterAddress(packedFrame).ToString());</c>
			/// </example> 
			/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>			
			public int ExtractMasterAddress(byte[] pFrame)
			{
				if(!this.IsFrame(pFrame))
				{
					EDOLLHandler.Error(-19); // Frameerror
					Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
					throw ex;					
				}
				
				return (pFrame[0] >> 2) & 0x03; // muss sich eh immer zwischen 0 und 3 befinden...				
			}		
			
			/// <summary>
			/// Extrahiert das Protokoll aus einem Datenframe
			/// </summary>
			/// <param name="pFrame">Gültiges M3S-Datenframe</param>
			/// <returns>
			/// <list type="table">
			/// <item><term><see cref="M3SProtocol">M3SProtocol</see></term><description>Bei gültiger Konversion</description></item>
			/// <item><term><see cref="M3SProtocol.Invalid">M3SProtocol.Invalid</see></term><description>im Fehlerfall</description></item>
			/// </list>
			/// </returns> 
			/// <remarks>
			/// Das Protokoll wird aus dem higher Nibble des Controlbytes extrahiert und in die Enumeration <see cref="M3SProtocol">M3SProtocol</see> gecastet.
			/// /// <para>
			/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
			/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// 	</para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
			/// </list>
			///  (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// </remarks>
			/// <example> Extrahieren aus byte[] packedFrame = IM3S_Dataframe.getDataframe();
			/// <c>Console.WriteLine("Protokoll des Pakets: " + m3sHandler.ExtractProtocol(packedFrame).ToString());</c>
			/// </example> 
			/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>
			public M3SProtocol ExtractProtocol(byte[] pFrame)
			{				
				if(!this.IsFrame(pFrame))
				{
					EDOLLHandler.Error(-19); // Frameerror
					Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
					throw ex;					
				}
				
				M3SProtocol toReturn;
				
				try
				{
					toReturn = (M3SProtocol)(Convert.ToInt32(pFrame[0]) >> 4);
				}
				catch
				{
					toReturn = M3SProtocol.Invalid;
				}
								 
				return(toReturn);
			} 
			
			/// <summary>
			/// Extrahiert das Protokoll aus einem (möglichen) Controlbyte. Intern keinerlei Sicherheitsabfragen ob es sich um ein gültiges Controlbyte handelt. 
			/// </summary>
			/// <param name="pCtrlByte">Controlbyte, aus dem Extrahiert werdne soll</param>
			/// <returns>
			/// <list type="table">
			/// <item><term><see cref="M3SProtocol">M3SProtocol</see></term><description>Bei gültiger Konversion</description></item>
			/// <item><term><see cref="M3SProtocol.Invalid">M3SProtocol.Invalid</see></term><description>im Fehlerfall</description></item>
			/// </list> 
			///</returns>
			public M3SProtocol ExtractProtocol(byte pCtrlByte)
			{	
				M3SProtocol toReturn;				
				try
				{
					toReturn = (M3SProtocol)(Convert.ToInt32(pCtrlByte) >> 4);
				}
				catch
				{
					toReturn = M3SProtocol.Invalid;
				}
								 
				return(toReturn);
			} 
			
					
			/// <summary>
			/// Extrahiert die Nutzdatenbytes aus einem M3S-Rahmen
			/// </summary>
			/// <param name="pFrame">Gültiges M3S-Datenframe</param>
			/// <returns>
			/// <list type="table">
			/// <item><term>NutzdatenbyteArray</term><description>Bei gültiger Konversion</description></item>
			/// <item><term>null</term><description>Bei Acknowledgeframes</description></item>
			/// <item><term>null</term><description>im Fehlerfall</description></item>
			/// </list>
			/// </returns> 
			/// <remarks>
			/// Das übergebene Datenframe wird erst auf Gültigkeit überprüft, anschließend werden die Rahmendaten weggeschnitten (vorne und hinten je 3 Byte) und das Nutzdatenpaket zurückgegeben.
			/// <para>
			/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
			/// 	<para><c>string EDOLLHandler.GetLastError();</c></para>
			/// 	</para>
			/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
			/// <list type="table">
			/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
			/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
			/// </list>
			///  (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
			/// </remarks>
			/// <example> Extrahieren aus byte[] packedFrame = IM3S_Dataframe.getDataframe(); und zweites Nutzdatenbyte ausgeben
			/// <c>Console.WriteLine("Zweites Nutzdatenbyte in packedFrame: " + string.Format("0x{0:x02}", m3sHandler.ExtractPayload(packedFrame)[1])); </c>
			/// </example> 
			/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception> 
			public byte[] ExtractPayload(byte[] pFrame)
			{
				if(!this.IsFrame(pFrame))
				{
					EDOLLHandler.Error(-19); // Frameerror
					Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
					throw ex;					
				}

				if(pFrame.Length == AcknowledgeFrameLength)
				{
					// ack
					return(null);
				}
				
				byte[] toReturn = new byte[pFrame.Length - Overhead];
				
				for(int i=M3S_V2_Dataframe.HeaderLength; i<pFrame.Length - M3S_V2_Dataframe.CrcLength; i++)
				{
					toReturn[i-M3S_V2_Dataframe.HeaderLength] = pFrame[i];
				}
				
				return(toReturn);
			}
			
			
			#endregion
						
			#region Build Frames
			
				/// <summary>
				/// Erstellt aufgrund von pStatus eine versandfertige byteFolge mit der entsprechenden Kommandorückmeldung (siehe <see cref="m3sExecutionError">m3sExecutionError</see>) im einzigen Nutzdatenbyte. 
				/// </summary>
				/// <param name="pStatus">Ausführungsstatus des Kommandos (<see cref="m3sExecutionError">m3sExecutionError</see>)</param>
				/// <param name="pCommandFrame">Frame, aufgrund dessen ein Kommando ausgeführt wurde</param>
				/// <returns>
				/// <list type="table">
				/// <item><term>versandfertiges ByteArray</term><description>bei fehlerfreier Ausführung</description></item>
				/// <item><term>Exception</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns> 
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
				/// </list>
				/// (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
				/// </remarks>
				/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>
				public byte[] GetCommandExecutionStateFrame(m3sExecutionError pStatus, byte[] pCommandFrame)
				{
					byte[] data = new byte[1];
					
					if(!this.IsFrame(pCommandFrame))
					{
						EDOLLHandler.Error(-19); // Frameerror
						Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
						throw ex;					
					}				
					
					data[0] = Convert.ToByte(pStatus);					
					IM3S_Dataframe toReturn = this.CreateFrame(Convert.ToInt32(pCommandFrame[1]), M3SProtocol.CommandResponse,this.ExtractMasterAddress(pCommandFrame), data, true, false);
					
					return(toReturn.GetDataframe());				
				}	

				
				
				/// <summary>
				///  Erstellt aufgrund von pStatus eine versandfertige byteFolge mit der entsprechenden Kommandorückmeldung (siehe <see cref="m3sExecutionError">m3sExecutionError</see>) im ersten Nutzdatenbyte und zusätzlichen Informationen (nachfolgende Nutzdatenbytes)
				/// </summary>
				/// <param name="pStatus">Ausführungsstatus des Kommandos (siehe <see cref="m3sExecutionError">m3sExecutionError</see>)</param>
				/// <param name="pCommandFrame">Frame, aufgrunddessen das Kommando ausgeführt wurde</param>
				/// <param name="pInfo">Informationsdatenbytes (max. 255) als byte[], auch null kann übergeben werden.</param>
				/// <returns>
				/// <list type="table">
				/// <item><term>versandfertiges ByteArray</term><description>bei fehlerfreier Ausführung</description></item>
				/// <item><term>Exception</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// Die Funktion kann zusätzliche als Parameter übergebene Informationsbytes an den Kommandosender zurückliefern. Diese werden als byte[] übergeben und hinten an das ExecutionStatus-Byte angehängt. 
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-19</term><description>Ungültiges Datenframe; Länge, Frameaufbau oder Prüfsumme inkorrekt</description></item>
				/// 	<item><term>-612</term><description>Es wurden zu viele Informationsbyte (>255) übergeben.</description></item>
				/// </list>
				/// (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
				/// </remarks>
				/// <exception cref="TBL.Exceptions.FrameError">Ungültiges Frame versucht zu verarbeiten</exception>
				public byte[] GetCommandExecutionStateFrame(m3sExecutionError pStatus, byte[] pCommandFrame, byte[] pInfo)
				{				
					if(pInfo == null)
					{
						return(this.GetCommandExecutionStateFrame(pStatus,pCommandFrame)); // Version ohne Info aufrufen..
					}
					
					if(!this.IsFrame(pCommandFrame))
					{
						EDOLLHandler.Error(-19); // Frameerror
						Exceptions.FrameError ex = new TBL.Exceptions.FrameError("Ungültiges Frame versucht zu verarbeiten");	
						throw ex;					
					}		

					if(pInfo.Length > 255)
					{
						EDOLLHandler.Error(-612);
						Exceptions.ConversionException ex2 = new TBL.Exceptions.ConversionException("Es wurden zu viele Informationsbytes (" + pInfo.Length.ToString() + ") übergeben an: TBL.Communication.Protocol.m3sHandler.GetCommandExecutionStateFrame(). max 255 erlaubt!");
						throw ex2;
					}
					
					byte[] data = new byte[1 + pInfo.Length];
					
					data[0] = Convert.ToByte(pStatus); // Kommando ins Frame einfügen
					
					for(int i=0; i<pInfo.Length; i++)	// Informationsbytes einfügen
					{
						data[i+1] = pInfo[i];
					}					
								
					IM3S_Dataframe toReturn = this.CreateFrame(Convert.ToInt32(pCommandFrame[1]), M3SProtocol.CommandResponse,this.ExtractMasterAddress(pCommandFrame), data, true, false);
					
					return(toReturn.GetDataframe());				
				}				
								
				
				/// <summary>
				/// Übernimmt Datei als byte[] und packt - über mehrere Aufrufe hinweg - die Datei in M3S-Gerechte durchnummerierte Pakete
				/// </summary>
				/// <param name="pAddr">Adresse, an die gesendet werden soll</param>
				/// <param name="pFileContent">Zu sendende Datei als byte[]</param>
				/// <param name="oErrorCode">EDOLL-Fehlercode</param>
				/// <returns>
				/// <list type="table">
				/// <listheader><term>Return</term><description>Beschreibung</description></listheader>
				/// <item><term>byte[] m3sDatenframe</term><description>Versandfertiges Datenpaket</description></item>
				/// <item><term></term></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-602</term><description>Ungültige Adresse (siehe <see cref="Check.CheckM3SSlaveAddress">Check. CheckM3SSlaveAddress</see></description></item>
				/// </list>
				/// (siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLERGgetLastError()</see>)
				/// </remarks>
				/// <exception cref="TBL.Exceptions.InvalidSlaveAddress">Ungültige Slaveadresse übergeben</exception>
				public IM3S_Dataframe[] GetFileTransferFrames(int pAddr, byte[] pFileContent, out int oErrorCode)
				{	
					throw new NotImplementedException("Implementierung muss erst überarbeitet werden...");
					
					
					/*
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pAddr)))
					{
						TBL.Exceptions.InvalidSlaveAddress ex = new TBL.Exceptions.InvalidSlaveAddress("Ungültige Slaveadresse '" +  pAddr.ToString() + "'");
						throw ex;						           
					}					
					
					if(fileTransferPosition >= pFileContent.GetUpperBound(0))
					{
						fileTransferPosition = 0; // reset
						fileTransferPackage = 1;
						return(null); // abgearbeitet, keine weiteren Frames
					}
					else
					{
						// Es gibt 252 Nutzdaten, 4 Paketinfodaten
						if(pFileContent.GetUpperBound(0) <= (255-packageNumberLength)) 		// ############### wenn Die Datei kleiner als eine Paketlänge ist
						{
							byte[] toSend = new byte[pFileContent.GetUpperBound(0)+1+packageNumberLength];					
							
							// Packagenumber
							toSend[0] = 0; // MSB
							toSend[1] = 0;
							toSend[2] = 0;
							toSend[3] = Convert.ToByte(fileTransferPackage); // LSB
							
							for(int i=0; i<=pFileContent.GetUpperBound(0); i++) // Nutzdaten veracken..
							{
								toSend[i+packageNumberLength] = pFileContent[i];
								fileTransferPosition = i;				// TODO ++ verwenden!
							}
							
							// in Frame packen							
							IM3S_Dataframe frame = this.CreateFrame(pAddr, M3SProtocol.FileTransfer,masteraddr,toSend, true, false);
							
							fileTransferPackage++; // erhöhen, damit die Funktion das nächste mal null liefert und so signalisiert dass Datei raus is...
							return(frame.GetDataframe());
						}
						else // ############################################################### Wenn mehrere Pakete gebraucht werden
						{
							byte[] toSend;
							
							if((pFileContent.GetUpperBound(0)+packageNumberLength)-fileTransferPosition > 255) // wenn Verbleibende Bytes + Paketinfo nicht in einem Frame platz haben
							{
								toSend = new byte[256];
							}
							else // Wenn ich beim letzten Paket angekommen bin
							{
								toSend = new byte[(pFileContent.Length - fileTransferPosition) + packageNumberLength]; // Brauch ich verbleibende Bytes + Paketinfoplatz
							}
							
							// Paketnummer zuweisen							
							toSend[0] = Convert.ToByte((fileTransferPackage >> 24) & 0xff); // MSB
							toSend[1] = Convert.ToByte((fileTransferPackage >> 16) & 0xff);
							toSend[2] = Convert.ToByte((fileTransferPackage >> 8) & 0xFF);
							toSend[3] = Convert.ToByte(fileTransferPackage & 0xff); // LSB							
							
							
							for(int i=(int)fileTransferPosition; (i<=pFileContent.GetUpperBound(0)) && i<((256-packageNumberLength)*(fileTransferPackage-1))+(256-packageNumberLength); i++)
							{
								toSend[i-((256-packageNumberLength)*(fileTransferPackage-1))+packageNumberLength] = pFileContent[i]; // An richtiger STelle einfügen
								fileTransferPosition = i; // TODO ++?
							}
							
							fileTransferPosition++; // auf nächstes Byte zeigen für nächsten Methodendurchlauf
							
							
							IM3S_Dataframe frame = this.CreateFrame(pAddr, M3SProtocol.FileTransfer,masteraddr,toSend, true, false);
							fileTransferPackage++;						
							
							return(frame.GetDataframe());
						}
					}
					*/
				}					
				
				
				
				
				
				#region getFrames							
			
				
				/// <summary>
				/// Liefert Kommandoframe für einen FileRequest an bestimmte Adresse
				/// </summary>
				/// <param name="pAddr">Adresse, an die der Request gehen soll</param>
				/// <param name="fileName">max. 255 Bytes Dateiname</param>
				/// <returns>
				/// <list type="table">
				/// <listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
				/// <item><term>Versandfertiges Dateianforderungsframe</term><description>beinhaltet Dateinamen / Speicherort im Empfänger. (UTF-8 codiert)</description></item>
				/// <item><term>null</term><description>Im Fehlerfall (siehe Remarks)</description></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.getLastError()</see>)
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>0</term><description>Fehlerfreie Ausführung</description></item>
				/// 	<item><term>-602</term><description>Ungültige M3S-Slaveadresse</description></item>
				/// </list>
				/// </remarks>
				public IM3S_Dataframe GetFileRequestFrame(int pAddr, string fileName)
				{					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pAddr))) // Prüft adresse und legt Errorcode ggf. im Edollhandler ab
					{
						return(null);
					}
					
					IM3S_Dataframe requestFrame;
					byte[] rData = new byte[1 + fileName.Length];
					byte[] codedFilename = System.Text.Encoding.UTF8.GetBytes(fileName);
					rData[0] = Convert.ToByte(M3SCommand.FileRequest);
					
					for(int i=0; i<=codedFilename.GetUpperBound(0); i++)
					{
						rData[i+1] = codedFilename[i];
					}
					
					requestFrame = this.CreateFrame(pAddr, M3SProtocol.Command,masteraddr,rData,true,true);
					
					
					return(requestFrame);					
				}
				
				/// <summary>
				/// Liefert Kommandoframe für nachfolgenden Dateitransfer an bestimmten Slave. 
				/// </summary>
				/// <param name="pSlaveAddr">M3S-Slaveadresse des Empfängers</param>
				/// <param name="pFileLength">Dateigröße in Bytes</param>
				/// <param name="pTargetFileName">Soll-Speicherpfad beim Empfänger</param>
				/// <returns>
				/// <list type="table">
				/// <listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
				/// <item><term>Versandfertiges Ankündigungsframe</term><description>beinhaltet in wie vielen Paketen die Datei geschickt wird und den Speicherort im Empfänger (UTF-8 codiert)</description></item>
				/// <item><term>null</term><description>Im Fehlerfall (siehe Remarks)</description></item>
				/// </list>
				/// </returns>
				/// <remarks>
				/// Detaillierte Informationen zum Ablauf von Filetransfers siehe <a href="doc/M3SProtocol.htm">M3S-Protokolldokumentation</a>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.getLastError()</see>)
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>0</term><description>Fehlerfreie Ausführung</description></item>
				/// 	<item><term>-602</term><description>Ungültige M3S-Slaveadresse</description></item>
				/// 	<item><term>-613</term><description>ÜBergebener Dateiname zu lang (länger als 251 Zeichen)</description></item>
				/// </list>
				/// </remarks>
				public IM3S_Dataframe GetFileTransferAnnouncementFrame(int pSlaveAddr, int pFileLength, string pTargetFileName)
				{
					IM3S_Dataframe annFrame; // Das Frame wird erzeugt
					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pSlaveAddr))) // Prüft adresse und legt Errorcode ggf. im Edollhandler ab
					{
						return(null);
					}	

					// else	
					byte[] codedFilename = null;					
					
					
					codedFilename = System.Text.Encoding.UTF8.GetBytes(pTargetFileName); // Dateiname codieren
										
					if(codedFilename.Length > (256-packageNumberLength-1)) // pTargetFilename länger als Nutzdatenbytes - Paketinfodaten - Befehl
					{
						EDOLLHandler.Error(-613);
						return(null);
					}
					
					// Ermitteln der Datenpaktanzahl
					int paketzahl = (int)(Math.Ceiling((float)pFileLength / (float)(256-packageNumberLength)));					
					
					byte[] rData = new byte[1 + packageNumberLength + codedFilename.Length]; // Kommando, Paketzahl und NAme
					
					
					rData[0] = Convert.ToByte(M3SCommand.FileAnnouncement); // Kommando Dateitransfer Ankündigen
					
					// Anzahl der nachfolgenden Pakete zuweisen
					rData[1] = Convert.ToByte((paketzahl >> 24) & 0xFF);
					rData[2] = Convert.ToByte((paketzahl >> 16) & 0xFF);
					rData[3] = Convert.ToByte((paketzahl >> 8) & 0xFF);
					rData[4] = Convert.ToByte(paketzahl & 0xFF);
					
					
					for(int i=0; i<=codedFilename.GetUpperBound(0); i++)
					{
						rData[i+1+packageNumberLength] = codedFilename[i];
					}
					
					annFrame = this.CreateFrame(pSlaveAddr, M3SProtocol.Command,masteraddr, rData, true, true);
					
					return(annFrame);
					
					
				}
				
				/// <summary>
				/// Erstellt ein Acknowledgeframe auf einen bestimmten Datenrahmen (Argument 2)
				/// </summary>
				/// <param name="pAck">Acknowledge ja / nein (true / false)</param>
				/// <param name="pFrame">Datenrahmen, der acknowledged werden soll</param>
				/// <returns>
				/// Versandbereites Acknowledgeframe
				/// </returns>
				/// <exception cref="Exceptions.FrameError">Ungültiges Datenframe übergeben</exception>
				/// <exception cref="Exceptions.ConversionException">Aus übergebenem Datenframe kann kein Acknowledgeframe erstellt werden.</exception>
				public IM3S_Dataframe GetAcknowledgeFrame(bool pAck, byte[] pFrame)
				{					
					if(!this.IsFrame(pFrame))
					{
						Exceptions.FrameError ex = new TBL.Exceptions.FrameError("In Argument 2 übergebenes Dataframe ist ungültig: " + TBLConvert.BytesToHexString(pFrame));
						throw ex;
					}
					
					M3S_V2_Dataframe ackFrame = new M3S_V2_Dataframe(Convert.ToInt32(pFrame[1]), M3SProtocol.Acknowledge, this.ExtractMasterAddress(pFrame), null, false, pAck);
					
					if(ackFrame == null)
					{
						Exceptions.ConversionException ex = new Exceptions.ConversionException("Aus dem übergebenen Dataframe kann kein Acknowledgeframe erstellt werden." + Environment.NewLine + TBLConvert.BytesToHexString(pFrame));
						throw ex; 
					}					
					
					return ackFrame;
				}
				
				/// <summary>
				/// Liefert eine die Bus-Resetfolge gem. M3S-Protokoll zurück. 
				/// </summary>
				/// <returns>				
				/// <list type="table">
				/// <item><term>versandfertiges ByteArray</term><description>Requestframe bei fehlerfreier Ausführung</description></item>
				/// <item><term>null</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns> 
				/// <remarks>
				/// <para>Der Resetrahmen ist eine fix vorgegebene Bytefolge, der jedoch trotzdem ein gültiges Datenpaket darstellt, um die Prüfsumme bilden zu können und so zusätzliche Sicherheit zum Vorliegen eines gültigen Reset zu gewinnen.</para>
				/// <para>Resetfolge: 0x10 0x00 0x00 0x52 0x54 0x4F 0x2F</para>
				/// </remarks>
				public IM3S_Dataframe GetResetFrame()
				{
						// Build frame
						M3S_V2_Dataframe resetFrm;
						byte[] rData = new byte[1];
						rData[0] = Convert.ToByte(M3SCommand.Reset);
						
						resetFrm = new M3S_V2_Dataframe(0,M3SProtocol.Reset,0,rData,true,false);
						
						return(resetFrm);								
				}
				
				/// <summary>
				/// Kommandobytefolge an Slave: "Update" 
				/// </summary>
				/// <param name="pAddr"></param>
				/// <param name="vAcknowledgeRequest">Gibt an, ob das Frame vom Slave mit einem Acknowledge zu bestätigen ist oder nicht</param>
				/// <returns>
				/// <list type="table">
				/// <item><term>versandfertiges M3S-Datenframe</term><description>Requestframe bei fehlerfreier Ausführung</description></item>
				/// <item><term>null</term><description>im Fehlerfall</description></item>
				/// </list>
				/// </returns> 
				/// <remarks>
				/// <para>
				/// 	<para>Interne <see cref="EDOLL">EDOLL-Behandlung</see> ist implementiert. Das heißt im Fehlerfall (siehe Rückgabewert) kann mit einer Codezeile der Fehler ausgegeben werden:</para>
				/// 	<para><c>string EDOLLHandler.GetLastError();</c></para> 
				/// 	(siehe <see cref="EDOLLHandler.GetLastError">EDOLLHANDLER.GetLastError()</see>)
				/// 	</para>
				/// 	Liste möglicherweise auftretenden EDOLL-Fehler:
				/// <list type="table">
				/// 	<listheader><term><see cref="EDOLLHandler">(interne) Fehlernummer</see></term><description>Beschreibung</description></listheader>
				/// 	<item><term>-602</term><description>Ungültige Adresse (siehe <see cref="Check. CheckM3SSlaveAddress">Check. CheckM3SSlaveAddress</see></description></item>
				/// </list>
				/// </remarks>
				public IM3S_Dataframe GetUpdateFrame(int pAddr, bool vAcknowledgeRequest)
				{
					
					if(EDOLLHandler.Error(Check.CheckM3SSlaveAddress(pAddr)))
					{
						return(null);
					}
					
					byte[] cmdData = new byte[1];
					cmdData[0] = Convert.ToByte(M3SCommand.Update);
					
					IM3S_Dataframe toReturn = this.CreateFrame(pAddr,M3SProtocol.Command, masteraddr, cmdData, true, vAcknowledgeRequest);
					
					return(toReturn);
				}
				#endregion
			
			#endregion
						
			/// <summary>
			/// Standardkonstruktor, macht eigentlich nix ;)
			/// </summary>
			public M3S_V2_Handler()
			{
				
			}		
			
			
				
			
			 
			
		}
	
}
