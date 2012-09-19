/*
 * Created by SharpDevelop.
 * User: bert
 * Date: 21.12.2010
 * Time: 19:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;


namespace TBL
{	
	/// <summary>
	/// Statische Klasse die diverse Laufzeitvariablen beinhaltet.
	/// </summary>
	public static class Runtime
	{
		/// <summary>
		/// Absoluter Pfad zur ausführbaren Datei (Assembly)
		/// </summary>
		/// <remarks>Mono tested...</remarks>
		public static string ExecutablePath = System.Windows.Forms.Application.ExecutablePath;
		/// <summary>
		/// Name (inkl. .exe) der ausführbaren Datei
		/// </summary>
		/// <remarks>Wenn via Mono verwendet: Liefert Pfad zur Mono-Executable...</remarks>
		public static string ExecutableName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";		  
		/// <summary>
		/// Verzeichnis, in dem die ausführbare Datei (Assembly) liegt.
		/// </summary>
		/// <remarks>Mono tested...</remarks>
		public static string ExecutableDirectory
		{
			get
			{				
				int lastSlash = ExecutablePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
				return(ExecutablePath.Substring(0,lastSlash+1));
			}
		}
		/// <summary>
		/// Aktuelle Bibliotheksversion der TBL
		/// </summary>
		public static string LibraryVersion = "1.2"; // TODO Hier ändern wenn neue Library rausgegeben wird

	}
	
	/// <summary>
	/// Die statische Klasse TBL.Routines enthält einige Basisroutinen, die immer wieder gebraucht werden.
	/// </summary>
	public static class Routines
	{ 		
		/// <summary>
		/// Ermittelt alle IP-Adressen des Hosts
		/// </summary>
		/// <returns>
		/// IP-Adressarray des Hosts.
		/// </returns>
		public static IPAddress[] GetIPAddresses()
		  {
		     string strHostName = System.Net.Dns.GetHostName();
		     IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
		     IPAddress ipAddress = ipHostInfo.AddressList[0];
		
		     return ipHostInfo.AddressList;
		  }
		   
		 /// <summary>
		 /// Prüft zwei Dateien beliebigen Types, deren Pfade übergeben werden, auf Gleichheit
		 /// </summary>
		 /// <param name="file1">Pfad zur Datei 1</param>
		 /// <param name="file2">Pfad zur Datei 2</param>
		 /// <returns>
		 /// <list type="table">
		 /// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		 /// <item><term>true</term><description>Dateien sind gleich</description></item>
		 /// <item><term>false</term><description>Dateien sind nicht gleich</description></item>
		 /// </list>
		 /// </returns>
		 public static bool FileCompare(string file1, string file2)
			{
			    int file1byte;
			    int file2byte;
			    FileStream fs1;
			    FileStream fs2;
			 
			    if (file1 == file2)
			    {
			          return true;
			    }
			 
			    fs1 = new FileStream(file1, FileMode.Open);
			    fs2 = new FileStream(file2, FileMode.Open);
			 
			    if (fs1.Length != fs2.Length)
			    {
			          fs1.Close();
			          fs2.Close();
			 
			          return false;
			    }
			 
			    do 
			    {
			          file1byte = fs1.ReadByte();
			          file2byte = fs2.ReadByte();
			    }
			    while ((file1byte == file2byte) && (file1byte != -1));
			 
			    fs1.Close();
			    fs2.Close();
			 
			    return ((file1byte - file2byte) == 0);
			}		 
		
		/// <summary>
		/// Prüft ob zwei beliebige Objekte gleichen Inhalt haben.
		/// </summary>
		/// <param name="a">Element a</param>
		/// <param name="b">Element b</param>
		/// <returns>
		/// <list type="table">
		 /// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		 /// <item><term>true</term><description>Dateien sind gleich</description></item>
		 /// <item><term>false</term><description>Dateien sind nicht gleich</description></item>
		 /// </list>
		 /// </returns>
		public static bool EqualContent(object a,object b)
		{
			//Falls beide Leer sind
			if (a == null && b==null)
				//Erfolgreich
				return true;
		
			//Wenn eins von beiden null ist
			if (a == null || b == null)
				//Nicht gleich
				return false;
			
			//Stream für vergleich öffnen
			System.IO.MemoryStream sA = new System.IO.MemoryStream();
			System.IO.MemoryStream sB = new System.IO.MemoryStream();
		
			//Binär beide Serialisieren
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bin = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			bin.Serialize(sA,a);
			bin.Serialize(sB,b);
		
			//In Array lesen
			byte[] saB = sA.ToArray();
			byte[] sbB = sB.ToArray();
		
			//Array vergleichen
			if (saB.Length == sbB.Length)
			{
				for(int i=0;i<saB.Length;i++)
					if (!saB[i].Equals(sbB[i]))
						return false;
			}
			else
			{
				//Länge stimmt schonmal nicht überein
				return false;
			}
		
			//Jetzt kann es nur noch übereinstimmen ;)
			return true;
		}
		
		/// <summary>
		/// Öffnet grafische Eingabeaufforderung (ähnlich zu VB6)
		/// </summary>
		/// <param name="prompt">Verbale Beschreibung der Eingabeaufforderung</param>
		/// <param name="title">Titel des Dialogs</param>
		/// <param name="defaultValue">Initialwert</param>
		/// <returns>
		/// String in Textbox zum Zeitpunkt des Drückens auf OK / Abbrechen
		/// </returns>
		public static string InputBox(string prompt, string title, string defaultValue)
		  {
		    InputBoxDialog ib = new InputBoxDialog();
		    ib.FormPrompt = prompt;
		    ib.FormCaption = title;
		    ib.DefaultValue = defaultValue;
		    ib.ShowDialog();
		    string s = ib.InputResponse;
		    ib.Close();
		    return s;
		  } // method: InputBox		 
		}
	
	/// <summary>
	/// Die statische Klasse TBL.TBLConvert stellt spezielle Konvertierungsroutinen bereit.
	/// </summary>
	public static class TBLConvert
	{	
		/// <summary>
		/// Konvertiert eine Arraylist mit byte-Elementen in ein Byte-Array
		/// </summary>
		/// <param name="pArray">CArray mit unsigned char (byte)-Werten</param>
		/// <returns></returns>
		public static byte[] CArrayToByteArray(System.Array pArray)
		{
			byte[] toReturn = new byte[pArray.Length];
			int cnt = 0;
			
			foreach(byte b in pArray)
			{
				toReturn[cnt] = b;
				cnt++;
			}
			
			return(toReturn);
			
		}
		
		
		
		
		/// <summary>
		/// Wandelt einen Hexstring in eine .NET-Farbe (System.Drawing.Color) 
		/// </summary>
		/// <param name="pHexstring">String im format #xxxxxx oder xxxxxx</param>
		/// <returns>Gewandelte Farbe</returns>
		/// <exception cref="Exceptions.ConversionException">Ungültiges Hexfileformat</exception>
		public static Color HexToColor(String pHexstring)
		{			
			if(EDOLL.EDOLLHandler.Error(Check.CheckRGBString(pHexstring)))
			{
				Exceptions.ConversionException ex = new TBL.Exceptions.ConversionException("String " + pHexstring + " kann nicht in System.Drawing.Color gewandelt werden - Syntax Error");
				throw(ex);
			}			
			
			Color actColor;
			int r,g,b;
			r=0;
			g=0;
			b=0;
			if ((pHexstring.StartsWith("#"))&&(pHexstring.Length==7))
			{
				r=Convert.ToInt32(pHexstring.Substring(1,2),16);
				g=Convert.ToInt32(pHexstring.Substring(3,2),16);
				b=Convert.ToInt32(pHexstring.Substring(5,2),16);
				actColor=Color.FromArgb(r,g,b);
			}
			else if(pHexstring.Length==6)
			{
				r=Convert.ToInt32(pHexstring.Substring(0,1),16);
				g=Convert.ToInt32(pHexstring.Substring(2,2),16);
				b=Convert.ToInt32(pHexstring.Substring(4,2),16);
				actColor=Color.FromArgb(r,g,b);
			}
			
			else
			{
				actColor=Color.White; // Some kind of default value...
			}
			
			return actColor;
		}
	
		/// <summary>
		/// Wandelt eine System.Drawing.Color in ein byte-Array
		/// </summary>
		/// <param name="pCol">Zu konvertierende Farbe</param>
		/// <returns>byte[] = {Rotanteil, Grünanteil, Blauanteil}</returns>
		public static byte[] ColorToBytes(Color pCol)
		{
			byte[] bytes = new byte[3];
			bytes[0] = Convert.ToByte(pCol.R);
			bytes[1] = Convert.ToByte(pCol.G);
			bytes[2] = Convert.ToByte(pCol.B);
			
			return(bytes);
		}
		
		/// <summary>
		/// Wandelt ein Bytearray in einen String (2stellige Hexzahlen mit Leerzeichen) zur Ausgabe um 
		/// </summary>
		/// <param name="toConvert">Zu konvertierendes Bytearray</param>
		/// <returns>1-Byte Hexdump breathless</returns>
		/// <exception cref="System.ArgumentNullException">null-Zeiger übergeben</exception>
		public static string BytesToHexString(byte[] toConvert)
		{			
			if(toConvert == null)
			{
				System.ArgumentNullException exNull = new ArgumentNullException("toConvert", "Übergebenes Byte-Array ist null @ TBLConvert.BytesToHexString()");
				throw exNull;
			}
			
			string toReturn = "";
			
			foreach(byte b in toConvert)
			{
				toReturn += string.Format("{0:x02} ", b);
			}
			
			return(toReturn);
		}
	}	
}