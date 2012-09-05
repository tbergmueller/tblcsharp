/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 30.01.2012
 * Time: 16:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


namespace TBL.License
{
	using System;
	using TBL.EDOLL;
	using System.IO;
	
	static class TimeLicense
	{
		public static bool Create(string rLicenseFile, string rSecretCode, DateTime vStartTime, DateTime vEndTime, out int oErrorCode)
		{
			string licenceString;		

			if(vEndTime.CompareTo(vStartTime) < 0)
			{
				oErrorCode = -620;
				return(false);
			}
						
			
			licenceString = rSecretCode + ";" + vEndTime.ToString() + ";" + vStartTime.ToString();
			
			TBL.Crypt.RatsEncryptionManager.EncryptStringtoFile(licenceString, rLicenseFile); // Store
			
			oErrorCode = 0;
			return(true);
		}
			
		/// <summary>
		/// Überprüft, ob eine zeitlich begrenzte Lizenz gültig und vorhanden ist
		/// </summary>
		/// <param name="rLicenseFile">Pfad zur Lizenzdatei</param>
		/// <param name="rSecretCode">Secretcode, der das Programm identifiziert (evtl. Programmid)</param>
		/// <param name="oOutputString">Ausgabeparameter: Fehlermeldungen (deutsch)</param>
		/// <returns>
		/// <list type="table">
		/// 	<listheader><term>Rückgabewert</term><description>Beschreibung</description></listheader>
		/// 	<item><term>true</term><description>Lizenzdatei gültig und vorhanden</description></item>
		/// 	<item><term>false</term><description>Lizenz abgelaufen, Datei nicht vorhanden, Lizenz für anderes Programm oder sonstige Fehler. Nähere Informationen sind dem Ausgabeparameter oOutputString zu entnehmen.</description></item>	
		/// </list>
		/// </returns>
		/// <remarks>
		/// Die Rückgabe der Fehlermeldungen erfolgt derzeit leider nur in deutscher Sprache. Sollte diese Bibliothek um ein englisches Sprachpaket (oder andere Sprachen) erweitert werden, wird selbstverständlich auch für diese Methode eine entsprechende Möglichkeit angeboten.
		/// </remarks>
		public static bool Check(string rLicenseFile, string rSecretCode, out string oOutputString)
		{
			DateTime dueDate;
			DateTime lastCall;
					
			// License-File aufbau:
			
			// secretCode;DueDate;LastProgrammCall
			// lastProgrammCall has to be actualized 
			
			if(File.Exists(rLicenseFile))
			{
				string licenseString = TBL.Crypt.RatsEncryptionManager.DecryptFiletoString("bts.dat");			
				string[] licenseParts = licenseString.Split(';');
				
				#region Lesen und Prüfen der Lizenzdatei
				
				if(licenseParts.Length != 3)
				{
					oOutputString = ("Beim Lesen der Lizenzdatei '"+rLicenseFile+"' ist ein Fehler aufgetreten. Möglicherweise handelt es sich um keine gültige Lizenzdatei. Kontaktieren Sie den Hersteller");
					return(false);
				}
				
				if(licenseParts[0] != rSecretCode)
				{
					oOutputString = ("Bei der Lizenzdatei '"+rLicenseFile+"' handelt es sich um keine gültige Lizenz für dieses Programm");
					return(false);
				}
				
				if(!DateTime.TryParse(licenseParts[1], out dueDate))
				{
					oOutputString = ("Beim Lesen der Lizenzdatei '"+rLicenseFile+"' ist ein Fehler aufgetreten. Möglicherweise handelt es sich um keine gültige Lizenzdatei. Kontaktieren Sie den Hersteller");
					return(false);
				}
				
				if(!DateTime.TryParse(licenseParts[2], out lastCall))
				{
					oOutputString = ("Beim Lesen der Lizenzdatei '"+rLicenseFile+"' ist ein Fehler aufgetreten. Möglicherweise handelt es sich um keine gültige Lizenzdatei. Kontaktieren Sie den Hersteller");
					return(false);
				}
				
				// Prüfen
				
				// Betrug abfangen
				if(DateTime.Compare(lastCall,DateTime.Now) > 0)
			    {
					oOutputString = ("Ihre Datumseinstellungen am Rechner sind inkorrekt oder Sie versuchen gerade mutwillig durch Systemmanipulation unsere zeitlich begrenzte Lizenz zu umgehen (strafbar, Betrug)");
					return(false);
				}
				
				// Gültigkeit prüfen
				if(DateTime.Compare(dueDate, DateTime.Now) <= 0)
				{
					oOutputString = ("Ihre Lizenz ist mit " + dueDate.ToString("dd.MM.yyyy") + " abgelaufen. Für eine Lizenzverlängerung kontaktieren Sie bitte den Hersteller");
					return(false);
				}
				
				#endregion
				
				// Wenn bis hierher gekommen versucht uns keiner zu betrügen und es scheint auch noch eine gültige Lizenz zu sein
				
				string toPrint = licenseParts[0] + ";" + licenseParts[1] + ";" + DateTime.Now.ToString();
				
				try
				{
					TBL.Crypt.RatsEncryptionManager.EncryptStringtoFile(toPrint, rLicenseFile);
					
					oOutputString = "Lizenzdatei gültig";
					return(true);
				}
				catch
				{
					
					oOutputString = ("Das Programm befindet sich in einem Verzeichnis Ihres Computers auf das Sie eingeschränkte Zugriffe haben. Führen Sie das Programm als Administrator aus oder kopieren Sie es in ein geeignetes Verzeichnis (z.B. eigene Dateien)");
					
					return(false);
				}
				
				
				
			}
			else
			{
				oOutputString =("Die Lizenzdatei '" + rLicenseFile+ "' konnte nicht im Programmverzeichnis gefunden werden. Programmausführung abgebrochen.");
				return(false);
			}
			
		}
	}
}