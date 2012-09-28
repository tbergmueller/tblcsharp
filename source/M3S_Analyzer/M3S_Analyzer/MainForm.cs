/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 02.11.2011
 * Time: 20:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using TBL.Communication.Protocol;
using TBL.Communication;
using TBL.EDOLL;


namespace M3S_Analyzer
{
	/// <summary>
	/// Logged Frames können entweder m3s-Frames sein, oder aber Störungen beinhalten
	/// </summary>
	public class M3sLogFrame
	{
		private DateTime logTime;
		private bool isValidFrame;
		private bool acknowledged = false;
		
		private int resetCounter = -1;
		
		private DataGridView dgv;
		
		
		IM3S_Handler protocol = new M3S_V2_Handler();
		// Eine der beiden 
		byte[] interference;
		IM3S_Dataframe validFrame;		
		
		public Point Location
		{
			get
			{
				return(dgv.Location);
			}
			set
			{
				dgv.Location = value;
			}
		}
		
		public Size Size
		{
			get
			{
				return(dgv.Size);
			}
			set
			{
				dgv.Size = value;
			}
		}
		
		public DataGridView Visualisation
		{
			get
			{
				return(dgv);
			}
		}
		
		public IM3S_Dataframe ValidFrame
		{
			get
			{
				return(validFrame);
			}
		}
				
		public M3sLogFrame(byte[] rInterference)
		{
			analyseAndStore(rInterference);
			
		}
		public M3sLogFrame(IM3S_Dataframe rValidFrame)
		{
			analyseAndStore(rValidFrame);
		}		
		
		public M3sLogFrame(object rToStore)
		{
			analyseAndStore(rToStore);
		}
		
		public string Hexdump
		{
			get
			{
				if(isValidFrame)
				{
					return(validFrame.ToString());
				}
				else
				{
					return(TBL.TBLConvert.BytesToHexString(interference));
				}
			}
		}
		
		/// <summary>
		/// Sollte nur vom Konstruktor aufgerufen werden, versucht übergebenes Objekt in M3S-Datenframe oder Interference zu konvertieren
		/// </summary>
		/// <param name="rToStore">m3sDataframe oder Bytestream</param>
		private void analyseAndStore(object rToStore)
		{
			// Differenziere was es ist...
			logTime = DateTime.Now;	
			dgv = new DataGridView();
			
			if(rToStore.GetType() == typeof(IM3S_Dataframe))
			{
				validFrame = rToStore as IM3S_Dataframe;
			}
			
			else if(rToStore.GetType() == typeof(byte[]))
			{	
				
				// Konvertierung war nicht erfolgreich, probiere es in einen Bytestream zu verwandeln
				
					byte[] tempStream = rToStore as byte[];
										
					try
					{						
						int errCode = -555;
						validFrame = protocol.CreateFrameByBytestream(tempStream, out errCode);
						
						if(errCode == 0) // Konvertierung war erfolgreich
						{
							isValidFrame = true;
							interference = null;							
						}
						else // Konvertierung war nicht erfolgreich
						{
							validFrame = null;
							interference = tempStream; // Ablegen
							isValidFrame = false;
						}						
					}
					catch
					{
						// Nothing, war einfach nicht konvertierbar... Muss also Störung sein
						validFrame = null;
						interference = tempStream; // Ablegen
						isValidFrame = false;
					}					
			}
			else if(rToStore.GetType() == typeof(byte))
			{					
									
			}
			else
			{
				// War auch kein Bytearray, also Exception
				TBL.Exceptions.ConversionException ex = new TBL.Exceptions.ConversionException("Das bei der Instanzierung an M3sLogFrame übergebene hat keinen gültigen Typ (erlaubt: m3sDataframe und byte[])");
				throw ex;
			}
			
			if(this.isValidFrame)
			{
				if(this.validFrame.Protocol == M3SProtocol.Reset)			
				{
					resetCounter++;
				}
			}
		}
		
		public DateTime LogTime
		{
			get
			{
				return(logTime);
			}
		}
		
		
		public bool IsFrame
		{
			get
			{
				return(isValidFrame);
			}
		}
		
		public int ResetCounter
		{
			get
			{
				return resetCounter;
			}
			set
			{
				resetCounter = value;
			}
		}
		
		public bool IsInterference
		{
			get
			{
				return(!isValidFrame);
			}
		}
		
		
	}
	
	
	public class M3SFrameLog: System.Collections.ArrayList
	{
	   private DataGridView dgv;
		
		// Colors
		private Color[] protocolCol;
		
		/// <summary>
		/// Delegat für das threadsichere Aufrufen der addVisualMethod. Wird zum Hinzfügen eines Frames zum Datagridview benötigt
		/// </summary>
		private delegate void addVisualCallback(int addAt);		
		addVisualCallback addVisual;							// Instanz des Delegats. Wird im Konstruktor initialisiert
		
		
		public Point Location
		{
			get
			{
				return(dgv.Location);
			}
			set
			{
				dgv.Location = value;
			}
		}
		
		public Size Size
		{
			get
			{
				return(dgv.Size);
			}
			set
			{
				dgv.Size = value;
			}
		}
		
		public DataGridView Visualisation
		{
			get
			{
				return(dgv);
			}
		}		
		
		
		public M3SFrameLog(): base()
		{
			addVisual = addVisualMethod;
			visualSetup();
			
		}
		
		// TODO: Update if Resetframe is detected... Visual problem
		public override int Add(object value)
		{		
			int addedAt = -1;
			
			byte[] test = value as byte[];
					
			M3sLogFrame loggedFrame;			
			loggedFrame = new M3sLogFrame(value); // Loggframe erzeugen		

			// Sonderfall Resetframe
			if(loggedFrame.IsFrame)
			{
				if(loggedFrame.ValidFrame.Protocol == M3SProtocol.Reset)
				{
					if(base.Count > 0)
					{
						M3sLogFrame lastFrame = base[base.Count-1] as M3sLogFrame;
						if(lastFrame.IsFrame)
						{
							if(lastFrame.ValidFrame.Protocol == M3SProtocol.Reset)
							{
								lastFrame.ResetCounter++;
								
								
								return base.Count-1;
							}
						}
					}				
				}
			}
						
			addedAt =  base.Add(loggedFrame);	// In Liste aufnehmen...
			
			visualize(addedAt);		// Hinzufügen im Datagridview...		
			return(addedAt);			
		}
		
		
		#region Visualisieren
		private void visualize(int vPosition)
		{
			if(dgv.InvokeRequired)
			{
				dgv.Invoke(addVisual,vPosition);
			}
			else
			{
				addVisualMethod(vPosition);
			}
		}
		
		/// <summary>
		/// Fügt eine Zeile zum Datagridview hinzu. Diese Methode soll nur Threadsicher aufgerufen werden!
		/// </summary>
		/// <param name="vPosition">Position in Liste, die hinzugefügt werden soll</param>
		private void addVisualMethod(int vPosition)
		{			
			/*
			 * dgv.Columns[0].Name = "No.";
				dgv.Columns[1].Name = "Time";
				dgv.Columns[2].Name = "DDR";
				dgv.Columns[3].Name = "Master";
				dgv.Columns[4].Name = "Slave";
				dgv.Columns[5].Name = "Protocol";
				dgv.Columns[6].Name = "Info / Details";
			 * */
			
			M3sLogFrame frame = base[vPosition] as M3sLogFrame;
			
			string[] rowToAdd = new string[dgv.ColumnCount];
			rowToAdd[0] = vPosition.ToString();
			rowToAdd[1] = frame.LogTime.ToString("HH:mm");
			rowToAdd[2] = "x";
			rowToAdd[fieldIdxSender] = rowToAdd[fieldIdxReceiver] = ""; // empty
						
			
			if(frame.IsFrame)
			{
				
				rowToAdd[fieldIdxReceiver] = frame.ValidFrame.SlaveAddress.ToString();
				rowToAdd[fieldIdxSender] = frame.ValidFrame.MasterAddress.ToString();
				rowToAdd[5] = frame.ValidFrame.InfoProtocolAcrynonym.ToString();
				
				
				if(frame.ValidFrame.Protocol == M3SProtocol.Reset)
				{
					if(frame.ResetCounter > 0)
					{
						rowToAdd[6] = "[SEQUENCE of "+frame.ResetCounter.ToString()+" frames] ";
					}
					else
					{
						rowToAdd[6] = "";
					}
				}
			 	else
				{
					rowToAdd[6] = "";
				}
				
				rowToAdd[6] += frame.ValidFrame.GetInterpretation();
				
			}
			else
			{
				rowToAdd[5] = "Intfrce";
				rowToAdd[6] = frame.Hexdump;
			}
			
			
					
			dgv.Rows.Add(rowToAdd);	
			
			colorRows();
			dgv.FirstDisplayedScrollingRowIndex = vPosition;	// Etwas hässliche notation, aber scrollt ganz nach unten...	

			dgv.Refresh();
		}
				
		private void dgv_SizeChanged(object sender, EventArgs e)
		{
			// Adjust coloumn widths
			dgv.Columns[0].Width = 30;
			dgv.Columns[1].Width = 50;
			dgv.Columns[2].Width = 30;
			dgv.Columns[fieldIdxSender].Width = 50;
			dgv.Columns[fieldIdxReceiver].Width = 50;
			dgv.Columns[5].Width = 50;
			dgv.Columns[6].Width = dgv.Width-20-(4*50 + 2*30);
			
			
		}
		#endregion
		
				
		/// <summary>
		/// Färbt Zeilen (und Zellen) entsprechend ein. muss immer am Ende der <see cref="addVisualMethod">addVisualMethod</see> aufgerufen werden. 
		/// </summary>
		private void colorRows()
		{
			for(int i=1; i<dgv.RowCount-1; i++)
			{
				M3sLogFrame frame = (M3sLogFrame)(base[i]) ;
				if(frame.IsFrame)
				{
					dgv.Rows[i].DefaultCellStyle.BackColor = protocolCol[(int)(frame.ValidFrame.Protocol)];
				}
				else
				{
					// Color Interference
				}
			}
		}
				
		private void createDefaultColors()
		{
			protocolCol = new Color[16]; // 4 Bit Protokollnummer
			
			protocolCol[0] = protocolCol[15] = Color.Red; // Invalid
			
			protocolCol[(int)M3SProtocol.Reset] = Color.PaleGreen;
			protocolCol[(int)M3SProtocol.Acknowledge] = Color.Gainsboro;			
			protocolCol[(int)M3SProtocol.DataTransfer] = Color.Khaki;
			protocolCol[(int)M3SProtocol.Command] = Color.PaleTurquoise;
			protocolCol[(int)M3SProtocol.CommandBroadcast] = Color.Turquoise;
			protocolCol[(int)M3SProtocol.BroadCast] = Color.Gold;			
			
		}
		
		public bool VisualizationFitWindowSize(Form rAdjustTo)
		{
			if(rAdjustTo.Width < (4*50 + 2*30 + 200))
			{
				return(false);
			}
			else
			{
				dgv.Width = rAdjustTo.Width-20;
				return(true);
			}
		}
		
		private const int fieldIdxSender = 3;
		private const int fieldIdxReceiver = 4;
						
		private void visualSetup()
		{
			createDefaultColors();
			
			#region DAtaGridView
				dgv = new DataGridView();
				
				dgv.Location = new Point(0,0);
								
				dgv.ColumnCount = 7;
				
				dgv.Columns[0].Name = "No.";
				dgv.Columns[1].Name = "Time";
				dgv.Columns[2].Name = "xxx";
				dgv.Columns[fieldIdxSender].Name = "From";
				dgv.Columns[fieldIdxReceiver].Name = "To";
				dgv.Columns[5].Name = "Protocol";
				dgv.Columns[6].Name = "Info / Details";
				
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;				
				dgv.SizeChanged += new EventHandler(dgv_SizeChanged);
				
				dgv.Font = System.Drawing.SystemFonts.DefaultFont;
				
				dgv.DefaultCellStyle.WrapMode =  DataGridViewTriState.True;
				dgv.Size = new Size(500,500);
				
				dgv.RowHeadersVisible = false;
				
				
			#endregion			
		}
		
		
	}

	
	
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private DevComMaster busListener;
		private M3SFrameLog log;
		private Point frameLogTopLeft = new Point(10,50);
		
		private IM3S_Handler protocol = new M3S_V2_Handler();
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			searching = new System.Threading.Semaphore(1,1);
			stdOut.DeactivateDebug();
			stdOut.SetInterface(StdOutInterfaces.WindowsForm);
			EDOLLHandler.Start(false);
			
			log = new M3SFrameLog();
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			
			if(log != null)
			{
				log.VisualizationFitWindowSize(this);
			}
		}
		
		static IM3S_Handler han = new M3S_V2_Handler();
		
				
		/// <summary>
		/// Sucht die Position des ersten gültigen M3S-Frames in einem übergebenen Buffer
		/// </summary>
		/// <param name="rBuffer">Buffer, der durchsucht werden soll</param>
		/// <returns>Position des ersten gültigen M3S-Frames (-1 wenn nichts gefunden wurde)</returns>
		int GetFirstValidFramePosition(ThreadsafeReceiveBuffer rBuffer, out M3SProtocol oProtocol)
		{
			oProtocol = M3SProtocol.Invalid;
			
			if(!rBuffer.DataAvailable)								// Buffer Empty
			{
				return(-1);
			}
			if(rBuffer.DataPtr < protocol.MinimumFrameLength)   // not enough Data available
			{
				return(-1);
			}
			
			int upperBound = -1;
			
			for(int start = 0; start<=rBuffer.DataPtr - protocol.MinimumFrameLength; start++)
			{
				
				if(han.ExtractProtocol(rBuffer.ReadByte(start))== M3SProtocol.Acknowledge)
				{
					if(protocol.IsFrame(rBuffer.readBytes(start, start+4)))
					{
						oProtocol = M3SProtocol.Acknowledge;
						return(start);
					}
				}			
				
				
				upperBound = rBuffer.ReadByte(start + protocol.UpperBoundPosition); // Ausgelesenes Upperbound aus angenommener Bitposition..
				byte[] frameToTest = rBuffer.readBytes(start, start + upperBound + protocol.Overhead); // Zu testendes Frame
				
				if(protocol.IsFrame(frameToTest))
				{
					oProtocol = han.ExtractProtocol(frameToTest[0]);
					return(start);
				}
			}
			
			
			
			return(-1);
		}
		
		private System.Threading.Semaphore searching;
		
		void searchForFrames(ThreadsafeReceiveBuffer rBuffer)
		{
			searching.WaitOne();
			
			M3SProtocol lastAnalyzedProtocol;
			int firstAvailable = GetFirstValidFramePosition(rBuffer, out lastAnalyzedProtocol);
						
			while(firstAvailable != -1)
			{				
				if(firstAvailable != 0) // ganz am Anfang im Buffer steht irgendwas, aber kein gültiges Frame
				{					
					byte[] someStream = rBuffer.readBytes(0, firstAvailable-1);
										
					log.Add(someStream);					
				}
				
				int frameEndPosition;
				
				if(lastAnalyzedProtocol == M3SProtocol.Acknowledge)
				{					
					frameEndPosition = firstAvailable + protocol.AcknowledgeFrameLength-1;
				}
				else
				{
					frameEndPosition = firstAvailable + rBuffer.ReadByte(firstAvailable + protocol.UpperBoundPosition)+protocol.Overhead;
				}
				
				
				byte[] frame = rBuffer.readBytes(firstAvailable, frameEndPosition );
				log.Add(frame);
				
				// Entfernen der gelesenen Byte..
				rBuffer.FreeBytes(0, frameEndPosition);
				rBuffer.Flush();
				
				firstAvailable = GetFirstValidFramePosition(rBuffer, out lastAnalyzedProtocol); // Search for next files...
			}
			searching.Release();
		}
					
		void bufferAdded(object sender, EventArgs e)
		{
			searchForFrames(sender as ThreadsafeReceiveBuffer);
			//this.Refresh();
		}	
				
		void Mnu_startClick(object sender, EventArgs e)
		{
			
			string readPort;
			// TODO: Auswahl TCP-Client oder USB
			
			form_cps selector = new form_cps();
			
			if(selector.ShowDialog() == DialogResult.OK)
			{
			 	readPort = selector.SelectedPort;
			}
			else
			{
				return;
			}
			
			string readBaudrate =  TBL.Routines.InputBox("Specify Baudrate (bps): ","Baudrate", devComSerialInterface.DefaultBaudrate.ToString());
			
			
			if(readBaudrate == string.Empty)
			{
				return;
			}
			
			int baudrate;
			
			if(!int.TryParse(readBaudrate,out baudrate))
			{
				stdOut.Error("Could not parse Baudrate. Only integer values allowed. Please try again");
				return;
			}
			
			
			
			devComSerialInterface client = new devComSerialInterface(readPort, 38400);			
			busListener = new DevComMaster(client);
		
			if(busListener.ConnectWithoutReset())
			{
				this.Controls.Add(log.Visualisation);
				log.VisualizationFitWindowSize(this);
				log.Location = frameLogTopLeft;
				busListener.ByteReceived += new ByteReceivedEventHandler(bufferAdded);
				mnu_start.Enabled = false;
				mnu_stop.Enabled = true;
			}
			else
			{
				stdOut.Error(EDOLLHandler.GetLastError(), busListener.InfoString);
			}			
		}
		
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			
			if(busListener != null)
			{
				busListener.Shutdown();
			}
		}
		
		void Mnu_stopClick(object sender, EventArgs e)
		{
			if(busListener != null)
			{
				busListener.Shutdown();
				mnu_start.Enabled = true;
				mnu_stop.Enabled = false;
				busListener.ByteReceived -= bufferAdded;
			}
		}
		
		void CreateDummyContentToolStripMenuItemClick(object sender, EventArgs e)
		{
			
			
			
			
			
			
		}
		
		
		public int GetNumOfCharsInFile(string filePath)
	   {
	       int count = 0;
	       
	       using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath))
	       {
	           while (sr.Read() != -1)
	               count++;
	       }
	       
	       return count;
	   }

		
		
		void Mnu_openHexdumpClick(object sender, EventArgs e)
		{
			OpenFileDialog opendia = new OpenFileDialog();
			
			opendia.InitialDirectory = TBL.OperatingSystem.DesktopDirectory;
		    opendia.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*" ;
		    opendia.FilterIndex = 1;
		    opendia.RestoreDirectory = false ;
		    
			if(opendia.ShowDialog() == DialogResult.OK)
			{
				int charCount = GetNumOfCharsInFile(opendia.FileName);
				
				int byteCount = charCount / 2;
				
				ThreadsafeReceiveBuffer recBuffer = new ThreadsafeReceiveBuffer(byteCount+1);
				
				
				
				int currentCharacter = 0;
				string curFigure = "";
				int ascii;
				
				// well.. now read
				try
				{
					using (System.IO.StreamReader sr = new System.IO.StreamReader(opendia.FileName))
			        {
						while ((ascii = sr.Read()) != -1)
			           {
							curFigure += ((char)(ascii)).ToString();
							
							if((currentCharacter % 2) == 1)
							{
								int intval = Convert.ToInt32(curFigure, 16);  //Using ToUInt32 not ToUInt64, as per OP comment
								recBuffer.AddByte((byte)(intval));
								curFigure = "";
							}
			           	
			           	  currentCharacter++;         	  
			           	  
			           }
			        }
				}
				catch
				{
					stdOut.Error("Parsing error at Character " + currentCharacter.ToString());
				}
				
				searchForFrames(recBuffer);
				
				this.Controls.Add(log.Visualisation);
				log.VisualizationFitWindowSize(this);
				stdOut.Info("should now open " + opendia.FileName);
			}
			
			
			
		}
	}
}
