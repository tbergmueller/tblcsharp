/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 30.03.2011
 * Time: 12:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

// Some operating system Routines...

using System.Runtime.InteropServices;
using System.Text;
using System;

using IWshRuntimeLibrary;

using System.Collections;

using Microsoft.Win32;
using System.Reflection;


namespace TBL 
{
	/// <summary>
	/// Stellt eine Reihe von Modifikationsroutinen und Umgebungsvariablen (auslesen) des Betriebssystems zur verfügung. Unter anderem einige Standardkomponenten, die bei Installationen verwendet werden können.
	/// </summary>
	public static class OperatingSystem
	{ 
		/// <summary>
		/// Determines whether execution platform is unixoid (Linux, MAC, ...) or not
		/// </summary>
		/// <value>true ... Unix based, false ... Windows</value>
		/// <remarks>Checks Environment.OSVersion.Platform for Values 4, 6, 128</remarks>
		public static bool IsUnix
		{
			get
			{
				 int p = (int) Environment.OSVersion.Platform;
		         return (p == 4) || (p == 6) || (p == 128);
			}
		}
		
		
		private const string RUN_LOCATION = @"Software\Microsoft\Windows\CurrentVersion\Run";
		
		/// <summary>
		/// Pfad zum Desktop des aktuell eingeloggten Users
		/// </summary>
		public static string DesktopDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		
		/// <summary>
		/// Pfad zum Application-Data Verzeichnis des aktuellen Users mit abschließendem Backslash
		/// </summary>
		public static string ApplicationDataCurrentUser = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";
		
		
		/// <summary>
		/// Liefert (egal auf welchem Betriebssystem) den Pfad des Startverzeichnis der 32-Bit-Anwendungen
		/// </summary>
		/// <returns>Pfad zu den Startmenüeinträgen</returns>
		public static string GetProgramFilesx86()
        {
            if( 8 == IntPtr.Size 
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
		
		
		// Benötigt für getCommonStartMenuPath
		[DllImport("shell32.dll")]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner,
        [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
        const int CSIDL_COMMON_STARTMENU = 0x16;  // \Windows\Start Menu\Programs
		
		/// <summary>
		/// Liefert den Pfad zum Startmenü aller User
		/// </summary>
		/// <returns>Pfad als string</returns>
		public static string GetMenuPathCommon()
		{     		 
       		StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_STARTMENU, false);
            return(path.ToString());
		}
		
		/// <summary>
		/// Liefert den PFad zum Startmenü des aktuellen Users
		/// </summary>
		/// <returns>Pfad als string</returns>
		public static string GetMenuPathCurrentUser()
		{
			return(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
		}
		
		
		private static void createShortCut(ShortCutProperties pShortcut, string pShortcutPath)
		{
			WshShellClass shellClass = new WshShellClass();
			IWshShortcut workingShortCut;
			
			// Shortcuts erzeugen						
			workingShortCut = (IWshRuntimeLibrary.IWshShortcut) shellClass.CreateShortcut(pShortcutPath);
			workingShortCut.TargetPath = @pShortcut.TargetPath;			
			workingShortCut.Description = pShortcut.Description;						
			workingShortCut.IconLocation = @pShortcut.IconPath;
			workingShortCut.Save();			
		}
		
		/// <summary>
		/// Legt beliebig viele Desktopicons an
		/// </summary>
		/// <param name="pShortCuts">Shortcut-Array vom Typ <see cref="ShortCutProperties">ShortCutProperties</see></param>
		public static void CreateDesktopIcons(ShortCutProperties[] pShortCuts)
		{
			foreach(ShortCutProperties s in pShortCuts)
			{
				string shortcutPath = TBL.OperatingSystem.DesktopDirectory + @"\" + s.Name + ".lnk";
				createShortCut(s, shortcutPath);
			}	
		}
		
		/// <summary>
		/// Erstellt einen neuen Ordner im Startmenü und legt darin die übergebenen Icons ab.
		/// </summary>
		/// <param name="pShortCuts">Abzulegende Icon-Eigenschaften</param>
		/// <param name="pFolderName">Name des Ordners im Startmenü</param>
		/// <param name="pCommon">Startmenü für alle Nutzer anlegen? (aktueller User = false)</param>
		public static void CreateStartMenuEntry(ShortCutProperties[] pShortCuts, string pFolderName, bool pCommon)
		{				
			string startMenuDir;			
						
			// Ordnerpfad ermitteln			
			if(pCommon)
			{
				startMenuDir = TBL.OperatingSystem.GetMenuPathCommon();
			}
			else
			{
				startMenuDir = TBL.OperatingSystem.GetMenuPathCurrentUser();
			}
						
			if(pFolderName != "")
			{
				startMenuDir = System.IO.Path.Combine(startMenuDir, pFolderName);
				
				// Anlegen
				try
				{
					System.IO.Directory.CreateDirectory(startMenuDir);
				}
				catch
				{
					// TODO throw zugriffsverletzungsexception
					
				}
			}
			
			foreach(ShortCutProperties s in pShortCuts)
			{
				string shortcutPath = startMenuDir + @"\" + s.Name + ".lnk";
				createShortCut(s, shortcutPath);
			}		
		}		
		
		/// <summary>
		/// Löscht Shortcuts aufgrund eines bestimmten Ziels (Teilstring oder gesamter String).
		/// </summary>
		/// <param name="pTarget">Teilstring oder gesamter Pfad des Targets (case unsensitive)</param>
		/// <param name="pFolderName">Ordner, in dem gesucht werden soll</param>
		/// <param name="pRekursive">Unterordner auch durchsuchen?</param>
		/// <returns>
		/// Arraylist mit allen gelöschten Shortcuts. Leere Arraylist (Count = 0) wenn nichts gelöscht wurde
		/// </returns>
		public static ArrayList DeleteShortcutsByTarget(string pTarget, string pFolderName, bool pRekursive)
		{
			ArrayList deleted = new ArrayList(); // loggt alle gelöschten Links..
			
			IWshShell shell = new WshShellClass();
			// get all icons
			
			ArrayList foldersToSearch = new ArrayList();
			foldersToSearch.Add(pFolderName);
			
			if(pRekursive)
			{
				string[] subfolders = System.IO.Directory.GetDirectories(pFolderName);
				
				foreach(string s in subfolders)
				{
					foldersToSearch.Add(s);
				}
			}
			
			foreach(string folder in foldersToSearch)
			{
				string[] shortcutsInDir = System.IO.Directory.GetFiles(folder, "*.lnk");
											
				foreach(string sc in shortcutsInDir)
				{
					IWshShortcut link = (IWshShortcut)shell.CreateShortcut(sc);
					
					if(link.TargetPath.ToLower().Contains(pTarget.ToLower()))
					{
						System.IO.File.Delete(sc);
						deleted.Add(sc); // Löschen loggen...
					}
				}			
			}
			return(deleted);		
		}
				
		
		internal enum MoveFileFlags
		{
		    MOVEFILE_REPLACE_EXISTING = 1,
		    MOVEFILE_COPY_ALLOWED = 2,
		    MOVEFILE_DELAY_UNTIL_REBOOT = 4,
		    MOVEFILE_WRITE_THROUGH  = 8
		}	
		[System.Runtime.InteropServices.DllImportAttribute("kernel32.dll",EntryPoint="MoveFileEx")]
		internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName,
		MoveFileFlags dwFlags);
		
		/// <summary>
		/// Löscht eine Datei nach dem nächsten Neustart
		/// </summary>
		/// <param name="pFileName">Zu löschende Datei</param>
		public static void DeleteFileAfterReboot(string pFileName)
		{
			MoveFileEx(pFileName, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
		}	
		
		
		
		 #region Autostart
		 
		 /// <summary>
		 /// Fügt das aktuell ausgeführte Executable der Autostartliste hinzu (unter dem übergebenen Key)
		 /// </summary>
		 /// <param name="pKeyName">Key, unter dem der Listeneintrag in der Autostartliste abgelegt wird.</param>
		 public static void AutoStartAddCurrentExe(string pKeyName)
		 {
		  RegistryKey key = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
		  string pathToSave = TBL.Runtime.ExecutablePath;
		  key.SetValue(pKeyName, pathToSave);
		 }
		
		
		 /// <summary>
		 /// Entfernt ein Programm aus der Autostartliste, indem der Key neutralisiert wird
		 /// </summary>
		 /// <param name="pKeyName">Key des zu neutralisierenden Autostartlisteneintrag</param>
		 public static void AutoStartRemove(string pKeyName)
		 {
		  RegistryKey key = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
		  key.DeleteValue(pKeyName);
		 }
		
		 /// <summary>
		 /// Ermittelt, ob sich das aktuell ausgeführte Executable in der Autostartliste (@pKeyname) vermerkt ist oder nicht.
		 /// </summary>
		 /// <param name="pKeyName">Key, unter dem der Pfad zum aktuell ausgeführten Executable abgelegt sein müsste</param>
		 /// <returns></returns>
		 public static bool AutoStartEnabledCurrentExe(string pKeyName)
		 {
		  
		   RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION);
		   if (key == null)
		    return false;
		
		   string value = (string)key.GetValue(pKeyName);
		   if (value == null)
		    return false;
		   return (value == TBL.Runtime.ExecutablePath);
		  
		 }
		  	 
		 
		 /// <summary>
		 /// Fügt ein Executable der Autostartliste hinzu
		 /// </summary>
		 /// <param name="pExePath">Absoluter Pfad zum Executable</param>
		 /// <param name="pKeyName">Key, unter diesem Namen wird der Pfad Autostartliste abgelegt.</param>
		 public static void AutoStartAdd(string pExePath,string pKeyName)
		 {
		 	// TODO: Check Filepath
		  	RegistryKey key = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
		 	 key.SetValue(pKeyName, pExePath);
		 }
		 
		 /// <summary>
		 /// Ermittelt, ob ein bestimmtes Executable (pExePath) in der Registry für Autostart vermerkt wurde oder nicht.
		 /// </summary>
		 /// <param name="pExePath">Absoluter Pfad zum Executable</param>
		 /// <param name="pKeyName">Key, unter dem der Pfad zum Executable abgelegt sein müsste</param>
		 /// <returns>
		 /// Gibt boolsch an, ob sich das Programm im Autostart befindet oder nicht. (@pKeyName value == pExePath)
		 /// </returns>
		 public static bool AutoStartEnabled(string pExePath, string pKeyName)
		 { 
		   RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION);
		   if (key == null)
		    return false;
		
		   string value = (string)key.GetValue(pKeyName);
		   if (value == null)
		    return false;
		   return (value == pExePath);
		  
		 }
		 #endregion
		
		 /// <summary>
		 /// Gibt an, ob das Betriebssystem, unter dem die Anwendung ausgeführt wird, Linux ist oder nicht 
		 /// </summary>
		 /// <remarks>
		 /// Die Ermittlung erfolgt über die Dateistruktur, es wird analysiert ob am Anfang beliebiger Dateipfade ein '/' (Linux) oder ein Laufwerksbuchstabe (Windows) steht
		 /// </remarks>
		 public static bool IsLinux
		 {
		 	get
		 	{
		 		return(TBL.Runtime.ExecutableDirectory.StartsWith("/"));
		 	}
		 }
		
		
		
		
		
		
		
	}
	/// <summary>
	/// Eigenschaften der Shortcuts, die mit <see cref="OperatingSystem.CreateDesktopIcons">OperatingSystem.CreateDesktopIcons</see> oder  <see cref="OperatingSystem.CreateStartMenuEntry">OperatingSystem.CreateStartMenuEntry</see>
	/// </summary>
	public struct ShortCutProperties
	{
		/// <summary>
		/// Anzeigename des Icons
		/// </summary>
		public string Name;
		/// <summary>
		/// Verknüpfungsziel (Windows-Pfad)
		/// </summary>
		public string TargetPath;
		/// <summary>
		/// Beschreibung, die beim Mouseover angezeigt wird
		/// </summary>
		public string Description;
		/// <summary>
		/// Pfad zum Icon
		/// </summary>
		public string IconPath;
	}
	
}
