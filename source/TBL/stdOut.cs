/*
 * Created by SharpDevelop.
 * User: Thomas Bergmüller
 * Date: 13.11.2010
 * Time: 13:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace TBL.EDOLL
{
	/// <summary>
	/// Interface-Vereinbarung für StdOut-Interfaces.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Die EDOLL-Standardausgabe sollte prinzipiell immer verwendet werden. Sie stellt eine Möglichkeit dar, Code einfach und ohne Änderungen vornehmen zu müssen, in anderen Programmtypen wiederzuverwenden. (Windows-Forms, Konsolen, Dienste, ...).
	/// Natürlich könnte man auch die C#-eigenen Funktionen für Debug usw. verwenden, allerdings sind diese nicht so vielfältig und individuell anpassbar.
	/// </para>
	/// <para>
	/// Eine weitere Falle wird beseitigt, indem sich Debugmeldungen einfach über die Methode <see cref="stdOut.DeactivateDebug()">stdOut.DeactivateDebug()</see> ausgeschaltet werden können. So kommt man nicht in die Verlegenheit, dass der Kunde in der Release-Version Debugmeldungen angezeigt bekommt, die eigentlich nur für den Entwickler bestimmt sind - Nicht selten werden hier auch sprachlich nicht so schöne Ausdrücke verwendet ;)
	/// </para>
	/// <para>
	/// Liste der Standardinterfaces:
	/// <list type="table">
	/// <listheader><term>Interface</term><description>Beschreibung</description></listheader>
	/// <item><term><see cref="StdOutInterfaces">WindowsForms</see></term><description>Die Ausgabe erfolgt über Messageboxen, Debugmeldungen werden in eine eigene - automatisch öffnende - Debugkonsole geschrieben.</description></item>
	/// <item><term><see cref="StdOutInterfaces">Console</see></term><description>Ausgabe erfolgt über die Konsole in unterschiedlichen Farben. Ja/Nein Optionen werden über einfache Zeicheneingabe realisiert</description></item>
	/// <item><term><see cref="stdOut.SetInterfaceToEventlog">Eventlogs (Ereignisanzeige)</see></term><description>Ausgabe erfolgt auf Eventlogs (Rechtsklick auf Computer => Verwalten => Ereignisanzeige), dabei werden Fehler, Warnung und Informationen unterschiedlich gekennzeichnet. Ja/Nein-Optionen sind hier nicht möglich und werden als Fehler behandelt.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// Erstellen und Benutzen eines eigenen Interface (Basierend auf einer einfachen Konsolenanwendung):
	/// <para><img src="tbl_img/stdOutOwnInterface.jpg" /></para>
	/// <code>	
	/// 	public class myStdInterface : IStdOutInterface
	/// 	{
	/// 		TextBox outTxt = null;
	/// 		
	/// 		// Konstruktor
	/// 		public myStdInterface(TextBox pTxt)
	/// 		{
	/// 			outTxt = pTxt; // Textboxinstanzzeiger speichern			
	/// 		}
	/// 		
	/// 		public DialogResult QuestionYesNo(string pPrompt)
	/// 		{
	/// 			return(MessageBox.Show(pPrompt, "Frage", MessageBoxButtons.YesNo, MessageBoxIcon.Question));
	/// 		}		
	/// 		
	/// 		public DialogResult WarningYesNo(string pPrompt)
	/// 		{
	/// 			return(MessageBox.Show(pPrompt, "Warnung", MessageBoxButtons.YesNo, MessageBoxIcon.Warning));
	/// 		}
	/// 		
	/// 		public void Warning(string pPrompt)
	/// 		{
	/// 			outTxt.Text += "> WARNING " + pPrompt + Environment.NewLine;
	/// 		}
	/// 		
	/// 		public void Error(string pPrompt)
	/// 		{
	/// 			outTxt.Text += "> ERROR " + pPrompt + Environment.NewLine;
	/// 		}
	/// 		
	/// 		public void Info(string pPrompt)
	/// 		{
	/// 			outTxt.Text += "> INFO " + pPrompt + Environment.NewLine;
	/// 		}
	/// 		
	/// 		public void Debug(string pPrompt)
	/// 		{			
	/// 			outTxt.Text += "> DEBUG " + pPrompt + Environment.NewLine;
	/// 			
	/// 		} 		
	/// 	}
	/// 		
	/// 	class Program
	/// 	{	
	/// 		public static void Main(string[] args)
	/// 		{		
	/// 			EDOLLHandler.Start(true); // Start EDOLL mit internen Fehlermeldungen und Log
	/// 			
	/// 			Form dummyForm = new Form();
	/// 			dummyForm.Size = new System.Drawing.Size(400,400);
	/// 			
	/// 			// Erzeuge Textbox
	/// 			TextBox txtOut = new TextBox();
	/// 			txtOut.Multiline = true;
	/// 			txtOut.Size = new System.Drawing.Size(300,200);
	/// 			txtOut.Location = new System.Drawing.Point(50,100);
	/// 			
	/// 			dummyForm.Controls.Add(txtOut);
	/// 			
	/// 			// Eigenes Interface instanzieren
	/// 			myStdInterface ifc = new myStdInterface(txtOut); // Zieltextbox wird dem Interface übergeben
	/// 						
	/// 			// stdOut konfigurieren
	/// 			stdOut.DeactivateDebug(); 			// Zeigt Debugmeldungen nicht mehr an
	/// 			stdOut.SetInterface(ifc);			// Ausgabe auf das selbst erschaffene Interface umleiten
	/// 			
	/// 			// Beispielausgaben machen
	/// 			stdOut.Warning("Das ist eine Warnung");
	/// 			stdOut.Error("Und das ein Fehler...");
	/// 			
	/// 			if(stdOut.QuestionYesNo("Soll ich noch was ausgeben?") == DialogResult.Yes)
	/// 			{
	/// 				stdOut.Info("Der Frage-Dialog wurde mit OK bestätigt");
	/// 			}
	/// 			
	/// 			// Praxisbeispiel in Verbindung mit Fehlerhandling:
	/// 			EDOLLHandler.Error(11); // Irgendein Verbindungsfehler
	/// 			stdOut.Error(EDOLLHandler.GetLastError(), "Platz für Details");
	/// 			
	/// 			// Anzeigen was wir gerade prodoziert haben		
	/// 			dummyForm.ShowDialog();						
	/// 			
	/// 			Console.Write(Environment.NewLine + "Press any key to continue . . . ");
	/// 			Console.ReadKey(true);
	/// 		}
	/// 	}
	/// </code>
	/// </para>
	/// </remarks>
	public interface IStdOutInterface
	{
		/// <summary>
		/// Standardausgabe für Fehler
		/// </summary>
		/// <param name="pStr">Ausgabestring</param>
		void Error(string pStr);		
		/// <summary>
		/// Standard-Out for Errors
		/// </summary>
		/// <param name="pErrormessage">OutputString</param>
		/// <param name="rErrorData">Additional information, dependent on stdOut-Interface</param>
		void Error(string pErrormessage, object rErrorData);
		/// <summary>
		/// Standardausgabe für Debugmeldungen. Kann über <see cref="stdOut.DeactivateDebug()">stdout.DeactivateDebug()</see> ausgeschaltet werden.
		/// </summary>
		/// <param name="pStr">Ausgabestring</param>
		void Debug(string pStr);
		/// <summary>
		/// Standardausgabe für Warnungen
		/// </summary>
		/// <param name="pStr">Ausgabestring</param>
		void Warning(string pStr);
		
		/// <summary>
		/// Standard-Out for Errors
		/// </summary>
		/// <param name="pErrormessage">OutputString</param>
		/// <param name="rErrorData">Additional information, dependent on stdOut-Interface</param>
		void Info(string pErrormessage, object rErrorData);
		/// <summary>
		/// Standardausgabe für Informationen
		/// </summary>
		/// <param name="pStr">Ausgabestring</param>
		void Info(string pStr);
		/// <summary>
		/// Warnung mit beinhalteter Frage. (z.b. Blabla - Vorgang trotzdem fortsetzen?)
		/// </summary>
		/// <param name="pStr">Warnung mit Frage</param>
		/// <returns>
		/// <list type="table">
		/// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		/// <item><term>DialogResult.Yes</term><description>Ja</description></item>
		/// <item><term>DialogResult.No</term><description>Nein</description></item>
		/// </list>
		/// </returns>
		DialogResult WarningYesNo(string pStr);
		/// <summary>
		/// Ja/Nein-Frage für Benutzerinteraktion
		/// </summary>
		/// <param name="pStr">Fragestring</param>
		/// <returns>
		/// <list type="table">
		/// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		/// <item><term>DialogResult.Yes</term><description>Ja</description></item>
		/// <item><term>DialogResult.No</term><description>Nein</description></item>
		/// </list>
		/// </returns>
		DialogResult QuestionYesNo(string pStr);
	}
	
	/// <summary>
	/// Enumeration: Vordefinierte Standardinterfaces für <see cref="stdOut">stdOut</see>
	/// </summary>
	/// <remarks>
	/// Zusätzlich existiert noch eine Standardausgabe auf Eventlogs. Dieses lässt sich nur über die Methode <see cref="stdOut.SetInterfaceToEventlog">SetInterfaceToEventlog</see> aktivieren. 
	/// </remarks>
	public enum StdOutInterfaces
	{
		/// <summary>
		/// Dummyinterface, lässt alle Meldungen der stdOut verschwinden.
		/// </summary>
		Mute = 0,
		/// <summary>
		/// Interface für GUI Anwendungen. Ausgabe über Messageboxes, grafisches Debugwindow, siehe <see cref="StdOutInterfaceForm">StdOutInterfaceForm</see>
		/// </summary>
		WindowsForm = 1,
		/// <summary>
		/// Interface für Konsolenanwendungen. Ausgabe in verschiedenen Farben und vorgestelltem Bezeichner, siehe <see cref="StdOutInterfaceConsole">StdOutInterfaceConsole</see> 
		/// </summary>
		Console = 2
	};
	
	/// <summary>
	/// Standardausgabe über vordefiniertes oder benutzerdefiniertes Interface (siehe <see cref="IStdOutInterface">IStdOutInterface</see>)
	/// </summary>
	/// <remarks>
	/// <para>Die statische Klasse stdOut ist quasi der Tunnel vom Programm zum definierten Ausgabeinterface.</para>
	/// <para>Das erste Kommando in Programmen, in denen die EDOLL benutzt wird, sollte die Standardausgabe konfigurieren. Dazu hat man folgende Möglichkeiten:
	/// 	<list type="table">
	/// 	<listheader><term>Interface</term><description>Bemerkung</description></listheader>
	/// 	<item>
	/// 		<term><see cref="stdOut.SetInterface(IStdOutInterface)">stdOut.Setinterface(IStdOutInterface)</see></term>
	/// 		<description>eigenes Interface</description>
	///		</item>
	/// 	<item>
	/// 		<term><see cref="stdOut.SetInterface(StdOutInterfaces)">stdOut.Setinterface(StdOutInterfaces)</see></term>
	/// 		<description>Vorgefertigtes Interfaces - siehe <see cref="StdOutInterfaces">StdOutInterfaces</see></description>
	///		</item>
	/// 	<item>
	/// 		<term><see cref="stdOut.SetInterfaceToEventlog">SetInterfaceToEventlog</see></term>
	/// 		<description>Ausgabe als Eventlog (Achtung: zum Erstellen eines neuen Logs (erstmalige Programmausführung) müssen ggf. Administratorrechte verfügbar sein)</description>
	/// 	</item>
	/// 	</list>
	/// 	</para>
	/// <para></para>
	/// </remarks>
	/// <example>
	/// Benutzung von vorgefertigten Interfaces:
	/// <para>Konsole</para>
	/// <img src="tbl_img/stdOutConsole.jpg" />
	/// <code>
	/// public static void stdOutConsoleExample()
	/// {
	/// 	stdOut.SetInterface(StdOutInterfaces.Console);
	/// 	
	/// 	stdOut.Info("Konsolenbeispiel");
	/// 	
	/// 	if(stdOut.QuestionYesNo("Wollen Sie die nachfolgende Debugmeldung ausgeben?") == DialogResult.Yes)
	/// 	{
	/// 		stdOut.Debug("Das ist die Debugmeldung");
	/// 	}
	/// 	else
	/// 	{
	/// 		stdOut.Warning("Es wurde eine Debugmeldung übersprungen");
	/// 	}
	/// 	
	/// 	stdOut.Debug("Beispiel beendet");
	/// 	
	/// 	stdOut.DeactivateDebug();
	/// 	
	/// 	stdOut.Debug("Wird nicht mehr angezeigt");	
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>WindowsForms</para>
	/// <img src="tbl_img/stdOutForm.jpg" />
	/// <code>
	/// public static void stdOutFormExample()
	/// {
	/// 	//stdOut.DeactivateDebug(); // Würde Debugmodus verlassen und DebugConsole nicht öffnen
	/// 	stdOut.SetInterface(StdOutInterfaces.WindowsForm); // Auf Windows Form festlegen, öffnet automatisch Debugconsole
	/// 	
	/// 	stdOut.Debug("Debugmeldung 1");
	/// 	
	/// 	if(stdOut.WarningYesNo("Soll noch etwas ins Debugfenster geschrieben werden?") == DialogResult.Yes)
	/// 	{
	///			stdOut.Debug("Zweite Debugmeldung");
	/// 	}
	/// 	else
	/// 	{
	/// 		stdOut.Info("Es wurde auf Nein geklickt");
	/// 	}
	/// 	
	/// 	stdOut.Error("Ich bin eine Fehlermeldung", "mit vielen Details");
	/// }
	/// </code>
	/// </example>
	public static class stdOut
	{
		private static IStdOutInterface outInterface = new StdOutInterfaceConsole();
		private static bool printDebug = true;
		private static bool logOutput = false;
		
		/// <summary>
		/// Setzt Ausgabeinterface auf eine übergebene InterfaceInstanz (siehe <see cref="IStdOutInterface">IStdOutInterface</see>)
		/// </summary>
		/// <param name="pInterface"></param>
		/// <example>siehe <see cref="IStdOutInterface">IStdOutInterface</see>
		/// </example>
		public static void SetInterface(IStdOutInterface pInterface)
		{
			outInterface = pInterface;
		}
		
		/// <summary>
		/// Creates an error output based on rErrorData to the set Output-Interface.
		/// </summary>
		/// <param name="rErrorData">From currently set Interface castable ErrorData</param>
		public static void Error(object rErrorData)
		{
			outInterface.Error("",rErrorData);
		}
		
		/// <summary>
		/// Creates an informational output based on rInfoData to the set Output-Interface
		/// </summary>
		/// <param name="rInfoData">From currently set Interface castable InfoData</param>
		public static void Info(object rInfoData)
		{
			outInterface.Info("",rInfoData);
		}
		
		/// <summary>
		/// Setzt das Standardausgabeinterface als EventLog.
		/// </summary>
		/// <param name="pEventLogname">Name des Eventlogs (z.B. 'MeinProjekt')</param>
		/// <param name="pEventSourcename">Name der Eventlog-Quelle (z.B. 'MeinUnterprogramm')</param>
		/// <remarks>
		/// <para>In neueren Betriebssystemen (Vista, Win 7) muss das Programm, indem eine Eventlogausgabe mit bestimmten Namen verwendet wird, erstmalig als Administrator ausgeführt werden, da sonst das Anlegen eines Eventlogs mit übergebenem pEventLogName nicht möglich ist. </para>
		/// <para>Außerdem ist zu beachten, dass zwei Ausgabearten (nämlich die Benutzereingaben <see cref="QuestionYesNo">QuestionYesNo</see> und <see cref="WarningYesNo">WarningYesNo</see>) nicht verfügbar sind. Sie werden als Fehler behandelt und liefern DialogResult.No zurück.</para>
		/// </remarks>
		/// <example>
		/// <img src="tbl_img/stdOutEventLog.jpg" />
		/// <code>		
		/// public static void EventlogExample()
		/// {
		/// 	stdOut.SetInterfaceToEventlog("MeinProjekt", "MeinUnterprogramm");
		/// 	EDOLLHandler.Start(false); 	// Edollhandler für Fehlerausgabe starten  mit internen Fehlern, ohne Logfile	
		/// 	
		/// 	// Typische Anwendung:
		/// 	int fehlerNr = -17; // -17 ist ein (Verbindungs)Fehler
		/// 	if(EDOLLHandler.Error(fehlerNr)) 
		/// 	{
		/// 		stdOut.Error(EDOLLHandler.GetLastError(), fehlerNr);
		/// 	}	
		/// 	
		/// 	stdOut.Info("Kann allerdings auch Infos über Programmzustände ausgeben");
		/// 	
		/// 	stdOut.Debug("Selbstverständlich ist auch Debuggen möglich - Werde aber als Info angezeigt");
		/// 	
		/// 	stdOut.QuestionYesNo("Ich bin eine Frage, werde allerdings als Fehler ausgegeben, da keine Benutzerinteraktion möglich ist");
		/// 	
		/// 	stdOut.DeactivateDebug();
		/// 	
		/// 	stdOut.Debug("Diese Meldung wird nicht mehr angezeigt werden...");
		/// }
		/// </code>
		/// </example>
		public static void SetInterfaceToEventlog(string pEventLogname, string pEventSourcename)
		{
			StdOutInterfaceEventlog ifc = new StdOutInterfaceEventlog(pEventLogname,pEventSourcename);
			SetInterface(ifc);
			logOutput = true;
		}
		
		
		/// <summary>
		/// Benutzt ein vorgefertigtes Interface. Siehe <see cref="stdOut">stdOut</see> und <see cref="StdOutInterfaces">StdOutInterfaces</see>
		/// </summary>
		/// <param name="pPredefinedInterface">Interfaceidentifikation (siehe <see cref="StdOutInterfaces">StdOutInterfaces</see>)</param>
		/// <example>siehe <see cref="stdOut">stdOut</see></example>
		public static void SetInterface(StdOutInterfaces pPredefinedInterface)
		{
			switch(pPredefinedInterface)
			{
				case StdOutInterfaces.Console:
					StdOutInterfaceConsole ifc = new StdOutInterfaceConsole();
					SetInterface(ifc);
					break;
				case StdOutInterfaces.WindowsForm:
					StdOutInterfaceForm ifc2 = new StdOutInterfaceForm();
					SetInterface(ifc2);
					if(printDebug)
					{
						ifc2.Show(); // Automatisch anzeigen
					}
					break;
				case StdOutInterfaces.Mute:
					SetInterface(null); // alles schlucken
					break;
				default:
					StdOutInterfaceConsole ifc3 = new StdOutInterfaceConsole();
					SetInterface(ifc3);
					break;					
			}
		}
		
		
		/// <summary>
		/// Aktiviert den Debugmodus. Debugmeldungen werden ausgegeben. Bei Benutzung des WindowsForm-Interface (siehe <see cref="SetInterface(StdOutInterfaces)">SetInterface()</see>) wird die grafische Debugkonsole automatisch geöffnet
		/// </summary>
		public static void ActivateDebug()
		{
			printDebug = true;			
		}
		
		/// <summary>
		/// Deaktiviert den Debugmodus. Debugmeldungen werden nicht ausgegeben. Bei Benutzung des WindowsForm-Interface (siehe <see cref="SetInterface(StdOutInterfaces)">SetInterface()</see>) wird die grafische Debugkonsole nicht geöffnet
		/// </summary>
		public static void DeactivateDebug()
		{
			printDebug = false;
		}
		
		/// <summary>
		/// Fehlerausgabe am jeweiligen Interface
		/// </summary>
		/// <param name="pString">Fehlermeldung</param>
		public static void Error(string pString)
		{
			if(outInterface != null)
			{
				outInterface.Error(pString);
			}
		}
		
		/// <summary>
		/// Fehlerausgabe am jeweiligen Interface mit Details
		/// </summary>
		/// <param name="pPrompt">Fehlermeldung</param>
		/// <param name="pDetails">Details die zum Fehler führten</param>
		public static void Error(string pPrompt, string pDetails)
		{
			if(outInterface != null)
			{
				outInterface.Error(pPrompt + Environment.NewLine + Environment.NewLine + pDetails);
			}
		}
		
		/// <summary>
		/// Fehlerausgabe am jeweiligen Interface mit EDOLL-Fehlernummer
		/// </summary>
		/// <param name="pString">Fehlermeldung</param>
		/// <param name="pNr">Fehlernummer</param>
		public static void Error(string pString, int pNr)
		{
			if(outInterface != null)
			{
				if(logOutput) // Wenn ich auf einen Log ausgib
				{
					(outInterface as StdOutInterfaceEventlog).Error(pString, Math.Abs(pNr)); // hab ich die möglichkeit die Nummer als Event zu deklarieren
				}
				else
				{
					outInterface.Error(pString + Environment.NewLine + pNr.ToString()); // Sonst nicht..
				}
				
			}
		}
		
		
		/// <summary>
		/// Debugmeldung
		/// </summary>
		/// <param name="pString">Auszugebende Meldung</param>
		public static void Debug(string pString)
		{
			if(outInterface != null && printDebug)
			{
				outInterface.Debug(pString);
			}
		}
		
		/// <summary>
		/// Allgemeine Warnung
		/// </summary>
		/// <param name="pPrompt">Auszugebende Warnung</param>
		public static void Warning(string pPrompt)
		{
			if(outInterface != null)
			{
				outInterface.Warning(pPrompt);
			}
		}
		
		/// <summary>
		/// Informationsausgabe
		/// </summary>
		/// <param name="pPrompt">Auszugebende Information</param>
		public static void Info(string pPrompt)
		{
			outInterface.Info(pPrompt);
		}
		
		/// <summary>
		/// Warnung mit Frage, liefert DialogResult zurück
		/// </summary>
		/// <param name="pPrompt">Warnung mit Frage</param>
		/// <returns>
		/// <list type="table">
		/// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		/// <item><term>DialogResult.Yes</term><description>Benutzer hat auf die Frage mit Ja geantwortet</description></item>
		/// <item><term>DialogResult.No</term><description>Benutzer hat auf die Frage mit Nein oder ungültig geantwortet</description></item>
		/// </list>
		/// </returns>
		/// <remarks>Benutzerinteraktionen bei Eventlogs sind nicht möglich. Siehe dazu <see cref="SetInterfaceToEventlog">SetInterfaceToEventlog</see> </remarks>
		public static DialogResult WarningYesNo(string pPrompt)
		{
			return outInterface.WarningYesNo(pPrompt);		
		}		
		

		/// <summary>
		/// Benutzerinteraktion durch Frage
		/// </summary>
		/// <param name="pPrompt">Zu beantwortende Frage</param>
		/// <returns>
		/// <list type="table">
		/// <listheader><term>Wert</term><description>Beschreibung</description></listheader>
		/// <item><term>DialogResult.Yes</term><description>Benutzer hat auf die Frage mit Ja geantwortet</description></item>
		/// <item><term>DialogResult.No</term><description>Benutzer hat auf die Frage mit Nein oder ungültig geantwortet</description></item>
		/// </list>
		/// </returns>
		/// <remarks>Benutzerinteraktionen bei Eventlogs sind nicht möglich. Siehe dazu <see cref="SetInterfaceToEventlog">SetInterfaceToEventlog</see> </remarks>
		public static DialogResult QuestionYesNo(string pPrompt)
		{
			return(outInterface.QuestionYesNo(pPrompt));
		}
	}	
	
	
	#region Internal (z.B. vorgefertigte Ausgabeinterfaces)
	
	internal class StdOutInterfaceConsole: IStdOutInterface
	{
		internal StdOutInterfaceConsole()
		{
			
		}
		
		public void Error(string vMessage, object rAdditionalInfo)
		{
			this.Error(vMessage + Environment.NewLine + rAdditionalInfo.ToString());
		}
		
		public void Info(string vMessage, object rAdditionalInfo)
		{
			this.Error(vMessage + Environment.NewLine + rAdditionalInfo.ToString());
		}
		
		public DialogResult QuestionYesNo(string pPrompt)
		{	
			Console.ForegroundColor = ConsoleColor.Gray;		
			Console.Write("> QUESTION: " + pPrompt + "Y(es)/N(o):");
			string c = Console.ReadLine();
			
			if(c.ToLower() == "y" || c.ToLower() == "yes")
			{
				return(DialogResult.Yes);
			}
			else
			{
				return(DialogResult.No);
			}
		}
		
		
		public DialogResult WarningYesNo(string pPrompt)
		{	
			Console.ForegroundColor = ConsoleColor.Yellow;		
			Console.Write("> WARNING: " + pPrompt + "Y(es)/N(o):");
			string c = Console.ReadLine();
			
			if(c.ToLower() == "y" || c.ToLower() == "yes")
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				return(DialogResult.Yes);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				return(DialogResult.No);
			}			
		}	
		
		public void Warning(string pPrompt)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("> WARNING: " + pPrompt);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		
		public void Error(string pPrompt)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("> ERROR: " + pPrompt);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		
		public void Info(string pPrompt)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine("> INFO: " + pPrompt);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		
		public void Debug(string pPrompt)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("> DEBUG: " + pPrompt);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		
	}
	
	internal delegate void printDelegateType(string toPrint);
	internal delegate DialogResult printDelegateDialogResultType(string toPrint);
	
	internal class StdOutInterfaceForm: IStdOutInterface
	{
		private DebugConsole console = null;
		
		//private printDelegateType printDelegate;		
		//private printDelegateDialogResultType printDelegateDialogResult;
		
		public StdOutInterfaceForm()
		{
			console = new DebugConsole();
		}
		
		public void Error(string vMessage, object rAdditionalInfo)
		{
			this.Error(vMessage + Environment.NewLine + rAdditionalInfo.ToString());
		}
		
		public void Info(string vMessage, object rAdditionalInfo)
		{
			this.Error(vMessage + Environment.NewLine + rAdditionalInfo.ToString());
		}
		
		public void Show()
		{
			console.Show();			
		}
		
		
		public void Hide()
		{
			console.Hide();
		}
		
		public DialogResult QuestionYesNo(string pPrompt)
		{
			return(MessageBox.Show(pPrompt, "Frage", MessageBoxButtons.YesNo, MessageBoxIcon.Question));
		}		
		
		public DialogResult WarningYesNo(string pPrompt)
		{
			return(MessageBox.Show(pPrompt, "Warnung", MessageBoxButtons.YesNo, MessageBoxIcon.Warning));
		}
		
		public void Warning(string pPrompt)
		{
			
			MessageBox.Show(pPrompt, "Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		
		public void Error(string pPrompt)
		{
			MessageBox.Show(pPrompt, "Fehler in der Programmausführung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		
		public void Info(string pPrompt)
		{
			MessageBox.Show(pPrompt, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		
		public void Debug(string pPrompt)
		{
			if(console != null)
			{
				console.writeLine(pPrompt);
			}
		}
	}	
	
	internal  class StdOutInterfaceEventlog: IStdOutInterface
	{
		private  EventLog stdOutLog;
		private  string evLogName;
		private  string evSrcName;		
		
		public void Error(string vMessage, object rAdditionalInfo)
		{
			this.Error(vMessage + Environment.NewLine + rAdditionalInfo.ToString());
		}
		
		public void Info(string vMessage, object rAdditionalInfo)
		{
			this.Error(vMessage + Environment.NewLine + rAdditionalInfo.ToString());
		}
		
		public  StdOutInterfaceEventlog(string pEventLogName, string pEventSourceName)
		{
			evLogName = pEventLogName;		
			evSrcName = pEventSourceName;
			
			if(!EventLog.SourceExists(evSrcName)) // Wenns den Log noch nicht gibt, dann erstelle...
			{
				EventLog.CreateEventSource(evSrcName, evLogName);
			}			
			
			stdOutLog = new EventLog();
			stdOutLog.Source = evSrcName;			
		}
		
		public  void clrEventLog() // Für diese MEthode gibt es momentan kein interface
		{
			stdOutLog.Clear();
		}
		
		public  void Error(string pString)
		{
			stdOutLog.WriteEntry("### Fehlermeldung ###" + Environment.NewLine + pString, EventLogEntryType.Error);
		}
		
		public void Error(string pString, int ereignis)
		{
			stdOutLog.WriteEntry("### Fehlermeldung ###" + Environment.NewLine + pString, EventLogEntryType.Error, ereignis);
		}
				
		public  void Debug(string pString)
		{
			stdOutLog.WriteEntry("### Debug ###" + Environment.NewLine + pString, EventLogEntryType.FailureAudit);
		}		
		
		public  void Info(string pString)
		{
			stdOutLog.WriteEntry("### Information ###" + Environment.NewLine + pString, EventLogEntryType.Information);
		}
		
		public DialogResult QuestionYesNo(string pPrompt)
		{
			stdOutLog.WriteEntry("### Ignored QUESTION ###" +Environment.NewLine + pPrompt, EventLogEntryType.Error);
			return(DialogResult.None);
		}
		
		public  void Warning(string pString)
		{
			stdOutLog.WriteEntry("### Warning ###" + Environment.NewLine + pString, EventLogEntryType.Warning);
		}
		
		public DialogResult WarningYesNo(string pString)
		{
			this.Warning(pString);
			return(DialogResult.None);
		}
		
		
	}
	
	#region Debugkonsole GUI	
	
	internal class DebugConsole
	{
		private  form_virtualConsole debugger; 
		
		internal DebugConsole()
		{
			debugger = new form_virtualConsole();
			
		}		
		
		internal  void Show()
		{
			debugger.Show();
		}
		internal  void Hide()
		{
			debugger.Hide();
		}
		
		internal  void writeLine(string pString)
		{
			debugger.WriteLine(pString);
		}
		
		internal  void statusPrompt(string pString)
		{
			debugger.statusPrompt(pString);
		}
	}
	
	internal partial class form_virtualConsole : Form
	{
		private System.ComponentModel.IContainer components = null;
		
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.rtxt_output = new System.Windows.Forms.RichTextBox();
			this.bnt_hide = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// rtxt_output
			// 
			this.rtxt_output.Location = new System.Drawing.Point(3, 34);
			this.rtxt_output.Name = "rtxt_output";
			this.rtxt_output.Size = new System.Drawing.Size(705, 396);
			this.rtxt_output.TabIndex = 0;
			this.rtxt_output.Text = "";
			// 
			// bnt_hide
			// 
			this.bnt_hide.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bnt_hide.Location = new System.Drawing.Point(654, 8);
			this.bnt_hide.Name = "bnt_hide";
			this.bnt_hide.Size = new System.Drawing.Size(45, 20);
			this.bnt_hide.TabIndex = 1;
			this.bnt_hide.Text = "Hide";
			this.bnt_hide.UseVisualStyleBackColor = true;
			this.bnt_hide.Click += new System.EventHandler(this.Bnt_hideClick);
			// 
			// form_virtualConsole
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(711, 435);
			this.Controls.Add(this.bnt_hide);
			this.Controls.Add(this.rtxt_output);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "form_virtualConsole";
			this.Opacity = 0.85D;
			this.Text = "form_virtualConsole";
			this.Load += new System.EventHandler(this.Form_virtualConsoleLoad);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button bnt_hide;
		private System.Windows.Forms.RichTextBox rtxt_output;
		
		internal form_virtualConsole()
		{
			
			InitializeComponent();			
			
			rtxt_output.Font = new Font("Verdana", 10F, GraphicsUnit.Point);
			rtxt_output.ForeColor = Color.LightGray;
			rtxt_output.BackColor = Color.Black;
			
			rtxt_output.Text = "~~~~~~~~~~~~~~debugConsole V1.0 gestartet ~~~~~~~~~~~~~~~~~~~~~~";
			rtxt_output.AppendText(System.Environment.NewLine);
			
		}
		
		void Form_virtualConsoleLoad(object sender, EventArgs e)
		{
			this.IsAccessible = false;
			this.Text = "TB Debugconsole V1.0";
		}
		
		internal void WriteLine(string pString)
		{
			rtxt_output.Text += System.Environment.NewLine + "> " + pString;	
		}
		
		internal void statusPrompt(string pString)
		{
			pString = "> " + pString;
			rtxt_output.Text += System.Environment.NewLine + pString;
			
			if(rtxt_output.Find(pString) > 0)
			{
				rtxt_output.SelectionStart = rtxt_output.Find(pString);
				rtxt_output.SelectionLength = pString.Length;
				rtxt_output.SelectionColor = Color.GreenYellow;				
			}			
			
		}
		
		void Bnt_hideClick(object sender, EventArgs e)
		{
			this.Hide();
			MessageBox.Show("hide");
		}
	}
	
	#endregion	
	
	#endregion	
	
}