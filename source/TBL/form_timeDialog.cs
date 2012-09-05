/*
 * Created by SharpDevelop.
 * User: bert
 * Date: 10.01.2011
 * Time: 16:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TBL
{ 
	/// <summary>
	/// Description of forme_timeDialog.
	/// </summary>
	internal partial class form_timeDialog : Form
	{
		protected override void OnKeyDown(KeyEventArgs e)
		{		 
			if(e.KeyCode == Keys.Enter)
			{
				Btn_okClick(this, new EventArgs());
			}		
			else
			{
				base.OnKeyDown(e);
			}			
		}
		
		public DateTime Value
		{
			get
			{
				return(dtp.Value);
			}
			set
			{
				dtp.Value = value;
			}
		}
		
		public string Description
		{
			set
			{
				lbl_text.Text = value;
			}
		}
		
		public string CustomFormatString
		{
			set
			{
				dtp.CustomFormat = value;
			}
		}
		
		
		public form_timeDialog()
		{
			
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			this.KeyPreview = true;
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void dtp_KeyDown(object sender, KeyEventArgs e)
		{
			
		}
		void Forme_timeDialogLoad(object sender, EventArgs e)
		{
			dtp.Format = DateTimePickerFormat.Custom;
			dtp.ShowUpDown = true;
			dtp.KeyDown += new KeyEventHandler(dtp_KeyDown);
			dtp.Focus();
			
			if(dtp.CustomFormat == "dd.MM.yyyy - HH:mm")
			{
				// Set to Month
				SendKeys.Send("{RIGHT}");
				// Set to Year
				SendKeys.Send("{RIGHT}");
				// Set to Hour
				SendKeys.Send("{RIGHT}");
			}
					
		}
		
		
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
		}
		
		void Btn_okClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
		
		void Btn_cancelClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
		
		void DtpValueChanged(object sender, EventArgs e)
		{
			
		}
	}
	internal partial class form_timeDialog
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
			this.dtp = new System.Windows.Forms.DateTimePicker();
			this.lbl_text = new System.Windows.Forms.Label();
			this.btn_ok = new System.Windows.Forms.Button();
			this.btn_cancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// dtp
			// 
			this.dtp.Location = new System.Drawing.Point(12, 44);
			this.dtp.Name = "dtp";
			this.dtp.Size = new System.Drawing.Size(200, 20);
			this.dtp.TabIndex = 0;
			this.dtp.ValueChanged += new System.EventHandler(this.DtpValueChanged);
			// 
			// lbl_text
			// 
			this.lbl_text.Location = new System.Drawing.Point(12, 16);
			this.lbl_text.Name = "lbl_text";
			this.lbl_text.Size = new System.Drawing.Size(197, 25);
			this.lbl_text.TabIndex = 1;
			this.lbl_text.Text = "label1";
			// 
			// btn_ok
			// 
			this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btn_ok.Location = new System.Drawing.Point(12, 88);
			this.btn_ok.Name = "btn_ok";
			this.btn_ok.Size = new System.Drawing.Size(75, 23);
			this.btn_ok.TabIndex = 2;
			this.btn_ok.Text = "OK";
			this.btn_ok.UseVisualStyleBackColor = true;
			this.btn_ok.Click += new System.EventHandler(this.Btn_okClick);
			// 
			// btn_cancel
			// 
			this.btn_cancel.Location = new System.Drawing.Point(134, 88);
			this.btn_cancel.Name = "btn_cancel";
			this.btn_cancel.Size = new System.Drawing.Size(75, 23);
			this.btn_cancel.TabIndex = 2;
			this.btn_cancel.Text = "Abbrechen";
			this.btn_cancel.UseVisualStyleBackColor = true;
			this.btn_cancel.Click += new System.EventHandler(this.Btn_cancelClick);
			// 
			// form_timeDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(224, 123);
			this.Controls.Add(this.btn_cancel);
			this.Controls.Add(this.btn_ok);
			this.Controls.Add(this.lbl_text);
			this.Controls.Add(this.dtp);
			this.Name = "form_timeDialog";
			this.Text = "forme_timeDialog";
			this.Load += new System.EventHandler(this.Forme_timeDialogLoad);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.DateTimePicker dtp;
		private System.Windows.Forms.Button btn_cancel;
		private System.Windows.Forms.Button btn_ok;
		private System.Windows.Forms.Label lbl_text;
	}
}
