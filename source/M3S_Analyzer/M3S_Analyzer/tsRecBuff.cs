using TBL;
using System.Collections;
using System;
using TBL.Communication.Protocol;
using TBL.EDOLL;
using TBL.Exceptions;

using System.Threading;

namespace TBL.Communication
{
	// Für toDelete-Liste
	internal class fromToPair
	{
		internal int From;
		internal int To;
	}	
	

	/// <summary>
	/// Asynchroner flexibler threadsicherer Byteempfangsbuffer (byte In, byte[] out) 
	/// </summary>
	/// <remarks>
	/// <para>
	/// Der Name der Klasse receiveBuffer dürfte die Funktion bereits ziemlich gut umreißen. Es handelt sich um einen Empfangsbuffer, der Byteweise beschrieben wird, und aus dem Blockweise gelesen werden kann.
	/// Er verfügt über eine Löschmethode, die zu löschende Bytes vorerst nur markiert und erst beim Aufruf receiveBuffer.Flush() gelöscht werden. Die nachfolgend im Buffer stehenden Bytes rücken dabei vor.
	/// Außerdem ist eine automatische Entleerung inkludiert. Daten, die unbenutzt über n handleGarbage()-Aufrufe im Speicher stehen, werden automatisch gelöscht. 
	/// </para>
	/// <para>
	/// <list type="table">
	/// Features
	/// <listheader><term>Was</term><description>Features</description></listheader>
	/// <item><term>Schreiben</term><description>Byteweise</description></item>
	/// <item><term>Lesen</term><description>Blockweise</description></item>
	/// <item><term>Löschen</term><description>Löschen zu gezielten Zeitpunkten (<see cref="receiveBuffer.Flush()">Flush</see>), vorher nur markieren</description></item>
	/// <item><term>Garbage Collection</term><description>Interne Beseitigung nicht gelöschter Datenbytes</description></item>
	/// <item><term>Suchen</term><description>Überprüfung / Suchen von beliebig langen Bytesequenzen</description></item>
	/// <item><term>Errorhandling</term><description><see cref="EDOLL">EDOLL-Fehlercodes</see></description></item>
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
	/// <item><term>Garbage-Collection</term><description>Automatische Entleerung: Anzahl der Aufrufe von <see cref="receiveBuffer.HandleGarbage()">HandleGarbage()</see> über Property <see cref="receiveBuffer.GarbageCollectionSpeed">GarbageCollectionSpeed</see> einstellbar.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// // <list type="table">
	/// <listheader><term>Entfernte Methoden/Properties</term><description>Version</description><description>Grund</description></listheader>
	/// <item><term>Busy</term><description>1.1</description><description>Umstellung von normalem receiveBuffer auf Threadsicheren Receivebuffer => Semaphore können von außerhalb nicht ausgewertet werden, das wäre nicht threadsicher</description></item>
	/// <item><term>Adding</term><description>1.1</description><description>Umstellung von normalem receiveBuffer auf Threadsicheren Receivebuffer => Semaphore können von außerhalb nicht ausgewertet werden, das wäre nicht threadsicher</description></item>
	/// <item><term>Deleting</term><description>1.1</description><description>Umstellung von normalem receiveBuffer auf Threadsicheren Receivebuffer => Semaphore können von außerhalb nicht ausgewertet werden, das wäre nicht threadsicher</description></item>
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
		
		public event ReceiveBufferByteAddedEventHandler ByteAdded;
				
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
		/// <description>receiveBuffer.GarbageCollectionSpeed</description>
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
				IndexOutOfRangeException ex = new IndexOutOfRangeException("@ Function receiveBuffer.ReadByte(int): Übergebene Position liegt außerhalb des Buffers. Aktuelle Buffergröße: " + dptr.ToString() + "Bytes");
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
			/// Wie schnell tot im Buffer liegende Daten gelöscht werden, wird über die Property <see cref="receiveBuffer.GarbageCollectionSpeed">receiveBuffer.GarbageCollectionSpeed</see> eingestellt.
			/// </para>
			/// <para>
			/// Zu beachten ist, dass receiveBuffer.handleGarbage() bei Vorliegen von Datenmüll die Methode <see cref="receiveBuffer.Flush()">receiveBuffer.Flush()</see> aufruft.
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
						// TODO: DRINGENDST HIER DAS AUF EINE LISTE SETZEN WAS NICHT SEIN DARF; NICHT ABSULUT LASSEN
						if(this.containsSequence(m3sDataframe.SlaveRequestRestartSequence) == -1)
						{
							this.FreeBytes(0,garbageBuffer.GetUpperBound(0));
							this.Flush();
							garbargeCount = -1; // das nächste mal reinlesen...
						}					
					}				
				}
				else
				{
					garbargeCount = -1; // das nächste mal reinlesen...
				}		
				
			}
				
			///<summary>	
			/// Die Flush-Methode arbeitet die intern vorliegende toDelete-Liste ab. Dabei werden alle durch die Methode receiveBuffer.FreeBytes() auf die Liste geschriebenen Bufferbereiche gelöscht und dahinterliegende Bytes vorgeschoben. 		
			/// </summary>
			/// <description>receiveBuffer.Flush()</description>
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
				if(ByteAdded != null)
				{
					ByteAdded(this, EventArgs.Empty); // Call Events (if there are any)
				}
				
				return(0); // EDOLL no Error
			}					
			else
			{ 
				
				EDOLLHandler.Error(tmpError); // Buffer overflow...				
				BufferOverflow ex = new BufferOverflow(EDOLLHandler.GetLastError() + "Datapointer: " + this.dptr.ToString());
				accessControl.Release();
				throw ex;
			}			
		} 
		
		private int availableSema;
		
		
		public int AddBytes(byte[] rBytes)
		{
			int tmpError;
			accessControl.WaitOne();
			for(int i=0; i<rBytes.Length; i++) // Der reihe nach hinzufügen
			{
				tmpError = addByteInternal(rBytes[i]);
				
				if(tmpError != 0) // sollte beispielsweise der Buffer überlaufen, Exception raus
				{
					
					EDOLLHandler.Error(tmpError); // Buffer overflow...				
					BufferOverflow ex = new BufferOverflow(EDOLLHandler.GetLastError() + "Datapointer: " + this.dptr.ToString());
					accessControl.Release();
					throw ex;	
				}
			}
			// wenn ich bis hierher gekommen bin, gabs keinen Fehler...
			
			availableSema = accessControl.Release();
			
			if(ByteAdded != null)
			{
				ByteAdded(this, EventArgs.Empty); // Fire Events (if there are any)
			}
			
			
			return(0);
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
		/// Standardkonstruktor, Initialisiert alle wichtigen internen Komponenten mit defaultBufferSize (2048 Bytes)
		/// </summary>
		public ThreadsafeReceiveBuffer()
		{
			buffersize = defaultBufferSize;
			commonInit();
		}
		
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="vBufferSize"></param>
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
		
		#endregion

		#region Private Funktionen
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
}