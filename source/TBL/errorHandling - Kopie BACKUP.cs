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

namespace TBL.EDOLL
{ 
	/// <summary>
	/// Description of errorHandling.
	/// </summary>
	/// <description>EDOLL</description>	
	/// 
	
	
	
	
	public static class EDOLLHandler
	{   
		internal static classErrorHandling errHan = null;
		internal static ArrayList errors;
		
					
		public static bool Start(string pConfFile, string pLogFile)
		{
			errHan = new classErrorHandling(pConfFile, pLogFile);
			
			if(errHan != null) 
				return(true);
			return(false);
		}
		
		public static bool Start(string pConfFile)
		{
			errHan = new classErrorHandling(pConfFile);
			
			if(errHan != null) 
				return(true);
			return(false);
		}
		
		public static bool Start()
		{
			errHan = new classErrorHandling();
			
			if(errHan!=null)
				return(true);
			return(false);
		}
		
		
		public static bool noError(int pErrCode)
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
	
		public static string getLastError()
		{
			if(errHan != null)
			{
				return(errHan.getLastError());
			}
			else
			{
				stdOut.Error("ErrorHandler nicht verbunden");
				return("errorHandler nicht aktiv");
			}
		}
		
		public static string getLastErrorTech()
		{
			if(errHan != null)
			{
				return(errHan.getLastErrorTech());
			}
			else
			{
				stdOut.Error("ErrorHandler nicht verbunden");
				return("errorHandler nicht aktiv");
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
		private string errorFileLocation = "";
		private string confFileLocation = "";
		
		private FileStream fs_conf;
		private FileStream fs_error;
		private StreamReader sr_conf;
		private StreamWriter sw_error;
		
		private bool logErrors;
		private bool active;
		private int errNum = 0;	

		private int lastErrCode = 0;
		
		private errorBundle[] err;
		
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
			// Do nothing...	
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
			if(this.mkFileStreams())
			{
				// Zeilen zählen
				errNum = 0;				
				while(sr_conf.ReadLine() != null)
				{
					errNum++;
				}				
						
				errNum--; // first line with coloumn names mustn't go into RAM				
				
				// Speicherplatz reservieren
				err = new errorBundle[errNum];
								
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
				
				return(true); //if reached this line it must be all gone well
			}
			else
			{
				return(false);
			}
			
			
		}
				
		private bool mkFileStreams()
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
				sr_conf = new StreamReader(fs_conf);
			}
			catch
			{
				stdOut.Error("Fehler: StreamReader für Fehlerkonfigurationsdatei nicht erstellbar. Dateizugriff aber möglich.");
				return(false);
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
			int errCnt;		
			int arr_cnt;			
			int tempCode;
			bool tempLogg;
			
			fs_conf.Seek(0, SeekOrigin.Begin);			
			sr_conf.ReadLine(); // ignore first one;
			
			buffer = sr_conf.ReadLine();
			
			for(errCnt = 0; buffer != null && errCnt < errNum; errCnt++)
			{
				string[] splitbuffer = buffer.Split(';');
			
				// Exit readConfFile(): when there's an incomplete set of data (not every field filled out properly) 
				
				if(splitbuffer.GetUpperBound(0) < (errorBundle.cntMember-1))
				{
					stdOut.Error("Fehler beim Lesen der Fehlerkonfigurationsdatei:\nZeile " + (errCnt + 2).ToString() + ": unvollständiger Datensatz");
					return(false);					
				}
				
				try
				{
					tempCode		= Convert.ToInt32(splitbuffer[0]);
				}
				catch
				{
					stdOut.Error("Fehler beim Lesen der Fehlerkonfigurationsdatei:\nZeile " + (errCnt + 2).ToString() + ": nicht in Errorcode konvertierbar");
					return(false);
				}
				
				// Scan through and break if error code exists twice, continue otherwise:
				for(arr_cnt = 0; arr_cnt < errCnt; arr_cnt++)
				{
					if(err[arr_cnt].Code == tempCode)
					{
						stdOut.Error("Fehler beim Lesen der Fehlerkonfigurationsdatei:\nZeile " + (errCnt+2).ToString() + ": Doppelter Errorcode, vorher definiert in Zeile " + (arr_cnt + 1).ToString());
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
				err[errCnt] = new errorBundle(tempCode,splitbuffer[1],splitbuffer[2],splitbuffer[3], splitbuffer[4], tempLogg);
				
				// Continue with next line
				buffer = sr_conf.ReadLine();
				
			}
			sr_conf.Close();
			fs_conf.Close();
			
			return(true);			
		}
		#endregion
		
		#region Runtime
		private string getErrorMsg(int pErrCode, bool techError)
		{
			int arr_cnt;
			if(active)
			{
				if(lastErrCode == 0)
				{
					return("Error #0: Kein Fehler");
				}
				for (arr_cnt = 0; arr_cnt < errNum; arr_cnt++)
				{
					if(err[arr_cnt].Code == pErrCode)
					{
						if(techError)
						{
							return(err[arr_cnt].getErrorForLog());
						}
						else
						{
							return(err[arr_cnt].ToString());
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
			if(pErrCode == 0)
			{
				return(true);
			}
			else
			{
				lastErrCode = pErrCode;
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
			int arr_cnt;
			errorBundle temp_err = null;
			if(logErrors)
			{
				for (arr_cnt = 0; arr_cnt < errNum; arr_cnt++)
				{
					if(err[arr_cnt].Code == pErrCode)
					{
						temp_err = err[arr_cnt];
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
}
