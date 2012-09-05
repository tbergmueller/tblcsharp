/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 22.01.2012
 * Time: 16:11
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace TBL.Communication.Protocol
{
	/// <summary>
	/// Zyklische Redundanzprüfung nach Dallas 1-Wire AN27 über Lookup-Algorithmus in Tabelle
	/// </summary>
	/// <remarks>
	/// <h1>Allgemeine Information zu CRC</h1>
	/// <para>Allgemeine Beschreibung der zyklischen Redundanzprüfung lt. Wikipedia:</para>
	/// <para>Für jeden Datenblock wird nach einem bestimmten Verfahren ein sogenannter CRC-Wert berechnet, der dem Datenblock angefügt wird. Zur Überprüfung der Daten wird dasselbe Berechnungsverfahren auf den Datenblock einschließlich des angefügten CRC-Werts angewandt. Ist das Ergebnis dann Null, kann angenommen werden, dass der Datenblock unverfälscht ist. Verschiedene technische Anwendungen weichen allerdings von diesem Schema ab, indem sie beispielsweise die Berechnung mit einem bestimmten Wert initialisieren oder den CRC-Wert vor der Übermittlung invertieren.</para>
	/// <para>CRC ist so ausgelegt, dass Fehler bei der Übertragung der Daten, wie sie beispielsweise durch Rauschen auf der Leitung verursacht werden könnten, mit hoher Wahrscheinlichkeit entdeckt werden. CRCs von seriellen Datenübertragungen können sehr einfach in Hardware realisiert werden. Zum Beispiel werden Datenübertragungen über Ethernet, sowie die meisten Festplatten-Übertragungen mit CRC-Verfahren geprüft.</para>
	/// <para>Das CRC-Verfahren ist nur für die Erkennung von zufälligen Fehlern ausgelegt. Es ist nicht geeignet, die Integrität der Daten zu bestätigen. Das heißt, es ist verhältnismäßig leicht, durch beabsichtigte Modifikation einen Datenstrom zu erzeugen, der den gleichen CRC-Wert wie eine gegebene Nachricht hat. Wenn eine solche Sicherheit gefordert ist, müssen kryptografische Hash-Funktionen wie beispielsweise SHA zum Einsatz kommen.</para>
	/// <para>Der Name des Verfahrens beruht darauf, dass der angefügte Wert keinen Informationsgehalt besitzt, der nicht bereits in dem zugrunde liegenden Datenblock enthalten ist. Er ist deshalb redundant. CRCs beruhen auf zyklischen Codes. Das sind Block-Codes, die die Eigenschaft haben, dass jede zyklische Verschiebung der Bits eines gültigen Code-Worts ebenfalls ein gültiges Code-Wort ist.</para>
	/// <h1>Implementierungsspezifisches</h1>
	/// <para>Diese Implementierung des CRC-8 basiert auf einer Application Note von Dallas/Maxim zu deren 1Wire-Bus. Siehe dazu <a href="http://www.microshadow.com/files/files8051/app27.pdf">AN27</a></para>
	/// <para>Das äquivalente zugrunde liegende CRC-Polynom ist X8 + X5 + X4 + 1 </para>
	/// <para>Zugrunde liegende CRC-Tabelle:</para>
	/// <code>
	/// static readonly byte[] crc8_array = {
	///       0x00, 0x5e, 0xbc, 0xe2, 0x61, 0x3f, 0xdd, 0x83,
	///       0xc2, 0x9c, 0x7e, 0x20, 0xa3, 0xfd, 0x1f, 0x41,
	///       0x9d, 0xc3, 0x21, 0x7f, 0xfc, 0xa2, 0x40, 0x1e,
	///       0x5f, 0x01, 0xe3, 0xbd, 0x3e, 0x60, 0x82, 0xdc,
	///       0x23, 0x7d, 0x9f, 0xc1, 0x42, 0x1c, 0xfe, 0xa0,
	///       0xe1, 0xbf, 0x5d, 0x03, 0x80, 0xde, 0x3c, 0x62,
	///       0xbe, 0xe0, 0x02, 0x5c, 0xdf, 0x81, 0x63, 0x3d,
	///       0x7c, 0x22, 0xc0, 0x9e, 0x1d, 0x43, 0xa1, 0xff,
	///       0x46, 0x18, 0xfa, 0xa4, 0x27, 0x79, 0x9b, 0xc5,
	///       0x84, 0xda, 0x38, 0x66, 0xe5, 0xbb, 0x59, 0x07,
	///       0xdb, 0x85, 0x67, 0x39, 0xba, 0xe4, 0x06, 0x58,
	///       0x19, 0x47, 0xa5, 0xfb, 0x78, 0x26, 0xc4, 0x9a,
	///       0x65, 0x3b, 0xd9, 0x87, 0x04, 0x5a, 0xb8, 0xe6,
	///       0xa7, 0xf9, 0x1b, 0x45, 0xc6, 0x98, 0x7a, 0x24,
	///       0xf8, 0xa6, 0x44, 0x1a, 0x99, 0xc7, 0x25, 0x7b,
	///       0x3a, 0x64, 0x86, 0xd8, 0x5b, 0x05, 0xe7, 0xb9,
	///       0x8c, 0xd2, 0x30, 0x6e, 0xed, 0xb3, 0x51, 0x0f,
	///       0x4e, 0x10, 0xf2, 0xac, 0x2f, 0x71, 0x93, 0xcd,
	///       0x11, 0x4f, 0xad, 0xf3, 0x70, 0x2e, 0xcc, 0x92,
	///       0xd3, 0x8d, 0x6f, 0x31, 0xb2, 0xec, 0x0e, 0x50,
	///       0xaf, 0xf1, 0x13, 0x4d, 0xce, 0x90, 0x72, 0x2c,
	///       0x6d, 0x33, 0xd1, 0x8f, 0x0c, 0x52, 0xb0, 0xee,
	///       0x32, 0x6c, 0x8e, 0xd0, 0x53, 0x0d, 0xef, 0xb1,
	///       0xf0, 0xae, 0x4c, 0x12, 0x91, 0xcf, 0x2d, 0x73,
	///       0xca, 0x94, 0x76, 0x28, 0xab, 0xf5, 0x17, 0x49,
	///       0x08, 0x56, 0xb4, 0xea, 0x69, 0x37, 0xd5, 0x8b,
	///       0x57, 0x09, 0xeb, 0xb5, 0x36, 0x68, 0x8a, 0xd4,
	///       0x95, 0xcb, 0x29, 0x77, 0xf4, 0xaa, 0x48, 0x16,
	///       0xe9, 0xb7, 0x55, 0x0b, 0x88, 0xd6, 0x34, 0x6a,
	///       0x2b, 0x75, 0x97, 0xc9, 0x4a, 0x14, 0xf6, 0xa8,
	///       0x74, 0x2a, 0xc8, 0x96, 0x15, 0x4b, 0xa9, 0xf7,
	///       0xb6, 0xe8, 0x0a, 0x54, 0xd7, 0x89, 0x6b, 0x35,
	///       };  
	/// </code>
	/// </remarks>
	public class Crc8
	{
		static readonly byte[] crc8_array = {
      0x00, 0x5e, 0xbc, 0xe2, 0x61, 0x3f, 0xdd, 0x83,
      0xc2, 0x9c, 0x7e, 0x20, 0xa3, 0xfd, 0x1f, 0x41,
      0x9d, 0xc3, 0x21, 0x7f, 0xfc, 0xa2, 0x40, 0x1e,
      0x5f, 0x01, 0xe3, 0xbd, 0x3e, 0x60, 0x82, 0xdc,
      0x23, 0x7d, 0x9f, 0xc1, 0x42, 0x1c, 0xfe, 0xa0,
      0xe1, 0xbf, 0x5d, 0x03, 0x80, 0xde, 0x3c, 0x62,
      0xbe, 0xe0, 0x02, 0x5c, 0xdf, 0x81, 0x63, 0x3d,
      0x7c, 0x22, 0xc0, 0x9e, 0x1d, 0x43, 0xa1, 0xff,
      0x46, 0x18, 0xfa, 0xa4, 0x27, 0x79, 0x9b, 0xc5,
      0x84, 0xda, 0x38, 0x66, 0xe5, 0xbb, 0x59, 0x07,
      0xdb, 0x85, 0x67, 0x39, 0xba, 0xe4, 0x06, 0x58,
      0x19, 0x47, 0xa5, 0xfb, 0x78, 0x26, 0xc4, 0x9a,
      0x65, 0x3b, 0xd9, 0x87, 0x04, 0x5a, 0xb8, 0xe6,
      0xa7, 0xf9, 0x1b, 0x45, 0xc6, 0x98, 0x7a, 0x24,
      0xf8, 0xa6, 0x44, 0x1a, 0x99, 0xc7, 0x25, 0x7b,
      0x3a, 0x64, 0x86, 0xd8, 0x5b, 0x05, 0xe7, 0xb9,
      0x8c, 0xd2, 0x30, 0x6e, 0xed, 0xb3, 0x51, 0x0f,
      0x4e, 0x10, 0xf2, 0xac, 0x2f, 0x71, 0x93, 0xcd,
      0x11, 0x4f, 0xad, 0xf3, 0x70, 0x2e, 0xcc, 0x92,
      0xd3, 0x8d, 0x6f, 0x31, 0xb2, 0xec, 0x0e, 0x50,
      0xaf, 0xf1, 0x13, 0x4d, 0xce, 0x90, 0x72, 0x2c,
      0x6d, 0x33, 0xd1, 0x8f, 0x0c, 0x52, 0xb0, 0xee,
      0x32, 0x6c, 0x8e, 0xd0, 0x53, 0x0d, 0xef, 0xb1,
      0xf0, 0xae, 0x4c, 0x12, 0x91, 0xcf, 0x2d, 0x73,
      0xca, 0x94, 0x76, 0x28, 0xab, 0xf5, 0x17, 0x49,
      0x08, 0x56, 0xb4, 0xea, 0x69, 0x37, 0xd5, 0x8b,
      0x57, 0x09, 0xeb, 0xb5, 0x36, 0x68, 0x8a, 0xd4,
      0x95, 0xcb, 0x29, 0x77, 0xf4, 0xaa, 0x48, 0x16,
      0xe9, 0xb7, 0x55, 0x0b, 0x88, 0xd6, 0x34, 0x6a,
      0x2b, 0x75, 0x97, 0xc9, 0x4a, 0x14, 0xf6, 0xa8,
      0x74, 0x2a, 0xc8, 0x96, 0x15, 0x4b, 0xa9, 0xf7,
      0xb6, 0xe8, 0x0a, 0x54, 0xd7, 0x89, 0x6b, 0x35,
      };  
	
	
		byte initval = 0;
	
		/// <summary>
		/// Erzeugt eine neue Instanz der CRC8-Klasse mit übergebenem Initialwert (Startwert des CRC)
		/// </summary>
		/// <param name="vInitValue">Startwert des CRC / der CRC-Berechnung</param>
		public Crc8(byte vInitValue)
		{
			initval = vInitValue;
		}
		
		/// <summary>
		/// Startwert der CRC-Prüfung/Berechnung
		/// </summary>
		public byte InitialValue
		{
			get
			{
				return(initval);
			}
			set
			{
				initval = value;
			}
		}
		
		/// <summary>
		/// Erzeugt eine neue Instanz der CRC8-Klasse, Initialwert (Startwert des CRC) ist definiert 0
		/// </summary>
		public Crc8()
		{
			initval = 0;
		}
		
		/// <summary>
		/// Überprüft ein Frame mittels CRC-8 auf Fehlerfreiheit
		/// </summary>
		/// <param name="rFrame">Zu prüfendes Frame (inkl. CRC Byte)</param>
		///<returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Frame ist Fehlerlos (CRC über Frame = 0)</description></item>
		/// 	<item><term>false</term><description>Frame beinhaltet Fehler, CRC != 0</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>Wird der CRC-8 über ein empfangenes Frame gebildet, das das vom Empfänger berechnete CRC-Byte enthält, ist der CRC-Wert nach Durchlaufen aller Bytes bei fehlerfreier Übertragung immer 0 (vorausgesetzt, der Startwert sind im Empfänger und Sender gleich definiert)</remarks>
		public bool FrameValid(byte[] rFrame)
		{			
			byte crc = calcCrc(rFrame);
			
			if(crc == 0)
			{
				
				return(true);
			}
			else
			{
				return(false);
			}
		}
		
		/// <summary>
		/// Berechnet den CRC Wert einer übergebenen Bytefolge, kann zum Berechnen und kontrollieren verwendet werden
		/// </summary>
		/// <param name="rFrame">Über dieses Frame wird CRC gebildet</param>
		/// <returns>CRC-Wert</returns>
		private byte calcCrc(byte[] rFrame)
		{
			byte curCrc = initval;
			
			for(int i=0; i<rFrame.Length; i++)
			{
				curCrc = crc8_array[(int)(rFrame[i] ^ curCrc)];
			}
			
			return(curCrc);
		}
		
		/// <summary>
		/// Berechnet den anzuhängenden CRC-Wert für ein Datenframe.
		/// </summary>
		/// <param name="rFrame">Frame OHNE CRC-Wert</param>
		/// <returns>CRC-Wert als Byte</returns>
		/// <remarks>Diese Methode wird üblicherweise beim Versenden von Datenframes verwendet. Der Rückgabewert (CRC-Wert) wird dem Datenframe angehängt. Bildet der Empfänger dann über das empfangene Frame (inkl. CRC-Wert) den CRC, so sollte das Ergebnis bei fehlerfreier Datenübertragung 0 ergeben (vorausgesetzt der Startwert ist gleich)</remarks>
		public byte Calculate(byte[] rFrame)
		{
			if(rFrame == null)
			{
				throw new ArgumentNullException("Parameter rFrame is invalid to Function Crc8.Calculate!");
			}
			
			return(calcCrc(rFrame));			
		}
		
		/// <summary>
		/// Berechnet den anzuhängenden CRC-Wert für einige Bytes innerhalb einer Bytefolge (Frame)
		/// </summary>
		/// <param name="rFrame">Bytefolge</param>
		/// <param name="vSubFrameLength">Anzahl der Datenbytes in rFrame (vom ersten Element an), für die das CRC-Byte berechnet werden soll.</param>
		/// <returns>CRC-Wert als Byte</returns>
		/// <remarks>Diese Methode wird üblicherweise beim Versenden von Datenframes verwendet. Der Rückgabewert (CRC-Wert) wird dem Datenframe angehängt. Bildet der Empfänger dann über das empfangene Frame (inkl. CRC-Wert) den CRC, so sollte das Ergebnis bei fehlerfreier Datenübertragung 0 ergeben (vorausgesetzt der Startwert ist gleich)</remarks>
		public byte Calculate(byte[] rFrame, int vSubFrameLength)
		{
			if(vSubFrameLength == 0 || vSubFrameLength > rFrame.Length)
			{
				throw new ArgumentOutOfRangeException("vSubFrameLength has to be between 1 and rFrame.Length!");	
			}
			
			byte[] subframe = new byte[vSubFrameLength];
			
			for(int i = 0; i<vSubFrameLength; i++)
			{
				subframe[i] = rFrame[i];
			}
			
			return(Calculate(subframe));
			
			
		}
	}
}
