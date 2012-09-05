/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 16.03.2011
 * Time: 19:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
 using System;


namespace TBL.Exceptions
{
	/// <summary>
	/// Exception tritt auf, wenn übergebene Slaveadresse außerhalb des zulässigen Bereichs liegt.
	/// </summary>
	public class InvalidSlaveAddress: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public InvalidSlaveAddress(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
	
	/// <summary>
	/// Exception wird geworfen, wenn ein nicht implementiertes Feature aufgerufen wird. Jedes Auftreten dieser Exception sollte sehr ernst genommen werden!
	/// </summary>
	public class NotImplemented: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public NotImplemented(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns>String mit entsprechender Fehlermeldung</returns>
		public override string ToString()
		{
			return string.Format("NOT IMPLEMENTED YET:" + msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
	
	/// <summary>
	/// Exception tritt auf, wenn versucht wird, ungültige Datenframes zu verarbeiten
	/// </summary>
	public class FrameError: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public FrameError(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
	
	/// <summary>
	/// Exception tritt auf, wenn versucht wird, eine Aktion in einem nicht vollkommen initialisierten oder konfigurierten Objekt durchzuführen
	/// </summary>
	public class ObjectNotReady: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public ObjectNotReady(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
	
	/// <summary>
	/// Exception tritt auf, wenn ein Buffer überläuft
	/// </summary>
	public class BufferOverflow: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public BufferOverflow(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
	
	/// <summary>
	/// Exception tritt auf, wenn ein Buffer überläuft
	/// </summary>
	public class NoConnection: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public NoConnection(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
	
	/// <summary>
	/// Exception tritt auf, wenn ein Buffer überläuft
	/// </summary>
	public class ConversionException: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public ConversionException(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
	
	/// <summary>
	/// Exception tritt auf, wenn auf ein nicht instanziertes Objekt zugegriffen wird
	/// </summary>
	public class ObjectNull: Exception
	{
		private string msg;
		
		/// <summary>
		/// Konstruktor, Aufruf nur mit Debugmeldung
		/// </summary>
		/// <param name="pDebugInfo">Details zum Debuggen</param>
		public ObjectNull(string pDebugInfo)
		{
			msg = pDebugInfo;
		}
		
		/// <summary>
		/// Gibt Fehlermeldung inkl. Basisexceptioncode aus.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(msg + Environment.NewLine + Environment.NewLine + base.ToString());
		}
	}
}
