/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 18.04.2011
 * Time: 20:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TBL.License
{
	/// <summary>
	/// Description of LicenseForm.
	/// </summary>
	internal partial class LicenseForm : Form
	{
		private string licensePath;
		private string appID;
		private string url;
		
		private const int keyLength = 25;
		
		
		internal LicenseForm(string pLicensePath, string pAppID, string pOnlineScript, string pContactInfo)
		{
			licensePath = pLicensePath;
			appID = pAppID;
			url = pOnlineScript;
			
			
			InitializeComponent();
			
			lbl_contact.Text =  pContactInfo;
			lbl_requestCode.Text = TBL.Hardware.HardwareInfo.GetIdentificationString();
			
			this.Height = 310;
			
		}
		
		void Btn_onlineActivateClick(object sender, EventArgs e)
		{
			string key = mtxt_key.Text.Replace("-", "").ToUpper();
			
			if(key.Length != License.Key.Length)
			{
				MessageBox.Show("Sie haben einen Key mit ungültiger Länge eingegeben!");
				mtxt_key.Focus();
				return;
			}
			OnlineActivationResponse result = License.Activation.OnlineActivation(url, key, appID, TBL.Hardware.HardwareInfo.GetIdentificationString());
			if(result == OnlineActivationResponse.Successful)
			{
				MessageBox.Show("Ihre Software wurde erfolgreich lizensiert. Vielen Dank dass Sie sich für uns entschieden haben", "Lizensierung erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information );
				TBL.License.Activation.WriteLicenseFile(licensePath, appID, key);
				
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				MessageBox.Show("Ihre Software konnte nicht lizensiert werden. Prüfen Sie Ihre Internetverbindung und die Eingabe Ihres Keys. Sollte in der Nachfolgenden Zeile 'NoActivationsLeft' stehen, haben Sie bereits alle Ihre erworbenen Lizenzen verbraucht. Kontaktieren Sie den Hersteller." + Environment.NewLine + Environment.NewLine + "Fehlercode: " + result.ToString(), "Aktivierung fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		void LicenseFormLoad(object sender, EventArgs e)
		{
			
		}
		
		void Btn_telephoneClick(object sender, EventArgs e)
		{
			
			string key = mtxt_key.Text.Replace("-", "").ToUpper();
			
			if(key.Length != License.Key.Length)
			{
				MessageBox.Show("Sie haben einen Key mit ungültiger Länge eingegeben!");
				mtxt_key.Focus();
				return;
			}
			mtxt_key.Enabled = false;
			
			grp_info.Visible = true;
			
			this.Height = 570;
		}
		
		void Txt_activationCodeTextChanged(object sender, EventArgs e)
		{
			btn_activate.Enabled = true;
		}
		
		void Btn_activateClick(object sender, EventArgs e)
		{
			if(License.Key.IsValidTelephoneCode(lbl_requestCode.Text, txt_activationCode.Text))
			{	
				MessageBox.Show("Ihre Software wurde erfolgreich lizensiert. Vielen Dank dass Sie sich für uns entschieden haben", "Lizensierung erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information );
				TBL.License.Activation.WriteLicenseFile(licensePath, appID, mtxt_key.Text.Replace("-", "").ToUpper());
				
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				MessageBox.Show("Die Software konnte nicht aktiviert werden. Überprüfen Sie den Aktivierungscode!", "Lizensierung nicht erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Error);
				txt_activationCode.Focus();
			}
		}
	}
}
