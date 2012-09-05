/*
 * Created by SharpDevelop.
 * User: bert
 * Date: 18.07.2012
 * Time: 08:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

using System.IO.Ports;

namespace M3S_Analyzer
{
	/// <summary>
	/// Description of ComPortSelector.
	/// </summary>
	public partial class form_cps : Form
	{
		public form_cps()
		{
			
			InitializeComponent();
			
			
			string[] ports = SerialPort.GetPortNames();
			
			if(ports != null)
			{
				cmb_ports.Items.Clear();
				foreach(String s in ports)
				{
					cmb_ports.Items.Add(s);
				}
			}
			
			this.DialogResult = DialogResult.Cancel;
			
			
		}
		
	
		
		void Btn_okClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
			
		}
		
		
		public string SelectedPort
		{
			get
			{
				return((string)(cmb_ports.SelectedItem));
			}
		}
	}
}
