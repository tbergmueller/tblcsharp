/*
 * Created by SharpDevelop.
 * User: bert
 * Date: 18.07.2012
 * Time: 08:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace M3S_Analyzer
{
	partial class form_cps
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
			this.cmb_ports = new System.Windows.Forms.ComboBox();
			this.btn_ok = new System.Windows.Forms.Button();
			this.lbl_select = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cmb_ports
			// 
			this.cmb_ports.FormattingEnabled = true;
			this.cmb_ports.Location = new System.Drawing.Point(12, 39);
			this.cmb_ports.Name = "cmb_ports";
			this.cmb_ports.Size = new System.Drawing.Size(121, 21);
			this.cmb_ports.TabIndex = 0;
			// 
			// btn_ok
			// 
			this.btn_ok.Location = new System.Drawing.Point(139, 37);
			this.btn_ok.Name = "btn_ok";
			this.btn_ok.Size = new System.Drawing.Size(75, 23);
			this.btn_ok.TabIndex = 1;
			this.btn_ok.Text = "OK";
			this.btn_ok.UseVisualStyleBackColor = true;
			this.btn_ok.Click += new System.EventHandler(this.Btn_okClick);
			// 
			// lbl_select
			// 
			this.lbl_select.Location = new System.Drawing.Point(12, 13);
			this.lbl_select.Name = "lbl_select";
			this.lbl_select.Size = new System.Drawing.Size(100, 23);
			this.lbl_select.TabIndex = 2;
			this.lbl_select.Text = "Select COM-Port:";
			// 
			// form_cps
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(224, 100);
			this.Controls.Add(this.lbl_select);
			this.Controls.Add(this.btn_ok);
			this.Controls.Add(this.cmb_ports);
			this.Name = "form_cps";
			this.Text = "Select Com Port";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label lbl_select;
		private System.Windows.Forms.Button btn_ok;
		private System.Windows.Forms.ComboBox cmb_ports;
	}
}
