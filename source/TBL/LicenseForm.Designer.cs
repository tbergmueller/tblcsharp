/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 18.04.2011
 * Time: 20:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace TBL.License
{
	internal partial class LicenseForm
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
			this.btn_onlineActivate = new System.Windows.Forms.Button();
			this.btn_telephone = new System.Windows.Forms.Button();
			this.lbl_contact = new System.Windows.Forms.Label();
			this.grp_info = new System.Windows.Forms.GroupBox();
			this.lbl_tel1 = new System.Windows.Forms.Label();
			this.btn_activate = new System.Windows.Forms.Button();
			this.lbl_requestCode = new System.Windows.Forms.Label();
			this.txt_activationCode = new System.Windows.Forms.TextBox();
			this.lbl_activation = new System.Windows.Forms.Label();
			this.lbl_capRequ = new System.Windows.Forms.Label();
			this.lbl_capKontakt = new System.Windows.Forms.Label();
			this.lbl_info = new System.Windows.Forms.Label();
			this.lbl_heading = new System.Windows.Forms.Label();
			this.lbl_caption = new System.Windows.Forms.Label();
			this.lbl_hint = new System.Windows.Forms.Label();
			this.mtxt_key = new System.Windows.Forms.MaskedTextBox();
			this.grp_info.SuspendLayout();
			this.SuspendLayout();
			// 
			// btn_onlineActivate
			// 
			this.btn_onlineActivate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_onlineActivate.Location = new System.Drawing.Point(12, 218);
			this.btn_onlineActivate.Name = "btn_onlineActivate";
			this.btn_onlineActivate.Size = new System.Drawing.Size(243, 26);
			this.btn_onlineActivate.TabIndex = 0;
			this.btn_onlineActivate.Text = "Online verifizieren (empfohlen)";
			this.btn_onlineActivate.UseVisualStyleBackColor = true;
			this.btn_onlineActivate.Click += new System.EventHandler(this.Btn_onlineActivateClick);
			// 
			// btn_telephone
			// 
			this.btn_telephone.Location = new System.Drawing.Point(261, 218);
			this.btn_telephone.Name = "btn_telephone";
			this.btn_telephone.Size = new System.Drawing.Size(162, 26);
			this.btn_telephone.TabIndex = 1;
			this.btn_telephone.Text = "Telefonisch verifizieren";
			this.btn_telephone.UseVisualStyleBackColor = true;
			this.btn_telephone.Click += new System.EventHandler(this.Btn_telephoneClick);
			// 
			// lbl_contact
			// 
			this.lbl_contact.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbl_contact.Location = new System.Drawing.Point(106, 70);
			this.lbl_contact.Name = "lbl_contact";
			this.lbl_contact.Size = new System.Drawing.Size(299, 23);
			this.lbl_contact.TabIndex = 2;
			this.lbl_contact.Text = "Kontakt";
			// 
			// grp_info
			// 
			this.grp_info.Controls.Add(this.lbl_tel1);
			this.grp_info.Controls.Add(this.btn_activate);
			this.grp_info.Controls.Add(this.lbl_requestCode);
			this.grp_info.Controls.Add(this.txt_activationCode);
			this.grp_info.Controls.Add(this.lbl_activation);
			this.grp_info.Controls.Add(this.lbl_capRequ);
			this.grp_info.Controls.Add(this.lbl_capKontakt);
			this.grp_info.Controls.Add(this.lbl_contact);
			this.grp_info.Location = new System.Drawing.Point(12, 284);
			this.grp_info.Name = "grp_info";
			this.grp_info.Size = new System.Drawing.Size(411, 244);
			this.grp_info.TabIndex = 3;
			this.grp_info.TabStop = false;
			this.grp_info.Text = "Telefonaktivierung";
			this.grp_info.Visible = false;
			// 
			// lbl_tel1
			// 
			this.lbl_tel1.Location = new System.Drawing.Point(11, 27);
			this.lbl_tel1.Name = "lbl_tel1";
			this.lbl_tel1.Size = new System.Drawing.Size(394, 33);
			this.lbl_tel1.TabIndex = 9;
			this.lbl_tel1.Text = "Verfügen Sie über keine Internetverbindung, kann die Aktivierung natürlich auch g" +
			"erne telefonisch (Mitarbeitergespräch) erfolgen.";
			// 
			// btn_activate
			// 
			this.btn_activate.Enabled = false;
			this.btn_activate.Location = new System.Drawing.Point(11, 205);
			this.btn_activate.Name = "btn_activate";
			this.btn_activate.Size = new System.Drawing.Size(394, 35);
			this.btn_activate.TabIndex = 8;
			this.btn_activate.Text = "Aktivieren";
			this.btn_activate.UseVisualStyleBackColor = true;
			this.btn_activate.Click += new System.EventHandler(this.Btn_activateClick);
			// 
			// lbl_requestCode
			// 
			this.lbl_requestCode.Location = new System.Drawing.Point(222, 105);
			this.lbl_requestCode.Name = "lbl_requestCode";
			this.lbl_requestCode.Size = new System.Drawing.Size(183, 65);
			this.lbl_requestCode.TabIndex = 7;
			this.lbl_requestCode.Text = "123456789asdfasdfasdfjkljkljsadfjlkasjdkasdfasdfjlkasjdfkljklasdjfkf";
			// 
			// txt_activationCode
			// 
			this.txt_activationCode.Location = new System.Drawing.Point(222, 179);
			this.txt_activationCode.Name = "txt_activationCode";
			this.txt_activationCode.Size = new System.Drawing.Size(183, 20);
			this.txt_activationCode.TabIndex = 6;
			this.txt_activationCode.TextChanged += new System.EventHandler(this.Txt_activationCodeTextChanged);
			// 
			// lbl_activation
			// 
			this.lbl_activation.Location = new System.Drawing.Point(11, 176);
			this.lbl_activation.Name = "lbl_activation";
			this.lbl_activation.Size = new System.Drawing.Size(100, 23);
			this.lbl_activation.TabIndex = 5;
			this.lbl_activation.Text = "Aktivierungscode";
			// 
			// lbl_capRequ
			// 
			this.lbl_capRequ.Location = new System.Drawing.Point(11, 108);
			this.lbl_capRequ.Name = "lbl_capRequ";
			this.lbl_capRequ.Size = new System.Drawing.Size(100, 23);
			this.lbl_capRequ.TabIndex = 4;
			this.lbl_capRequ.Text = "RequestCode: ";
			// 
			// lbl_capKontakt
			// 
			this.lbl_capKontakt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbl_capKontakt.Location = new System.Drawing.Point(11, 70);
			this.lbl_capKontakt.Name = "lbl_capKontakt";
			this.lbl_capKontakt.Size = new System.Drawing.Size(89, 23);
			this.lbl_capKontakt.TabIndex = 2;
			this.lbl_capKontakt.Text = "Kontakt";
			// 
			// lbl_info
			// 
			this.lbl_info.Location = new System.Drawing.Point(12, 58);
			this.lbl_info.Name = "lbl_info";
			this.lbl_info.Size = new System.Drawing.Size(423, 37);
			this.lbl_info.TabIndex = 4;
			this.lbl_info.Text = "Vor Gebrauch der Software dürfen wir Sie bitten, ihren Lizenz-Key einzugeben und " +
			"zu verifizieren. Dies kann entweder online (empfohlen) oder telefonisch geschehe" +
			"n.";
			// 
			// lbl_heading
			// 
			this.lbl_heading.Font = new System.Drawing.Font("Arial Rounded MT Bold", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbl_heading.Location = new System.Drawing.Point(12, 25);
			this.lbl_heading.Name = "lbl_heading";
			this.lbl_heading.Size = new System.Drawing.Size(297, 23);
			this.lbl_heading.TabIndex = 5;
			this.lbl_heading.Text = "Lizenz-Key Verifizierung";
			// 
			// lbl_caption
			// 
			this.lbl_caption.Location = new System.Drawing.Point(12, 122);
			this.lbl_caption.Name = "lbl_caption";
			this.lbl_caption.Size = new System.Drawing.Size(79, 23);
			this.lbl_caption.TabIndex = 7;
			this.lbl_caption.Text = "Lizenzkey";
			// 
			// lbl_hint
			// 
			this.lbl_hint.Location = new System.Drawing.Point(12, 157);
			this.lbl_hint.Name = "lbl_hint";
			this.lbl_hint.Size = new System.Drawing.Size(411, 33);
			this.lbl_hint.TabIndex = 8;
			this.lbl_hint.Text = "Den 25-stelligen Key haben Sie beim Erwerb der Software erhalten. Er besteht aus " +
			"Buchstaben und Zahlen";
			// 
			// mtxt_key
			// 
			this.mtxt_key.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mtxt_key.Location = new System.Drawing.Point(74, 119);
			this.mtxt_key.Mask = "AAAAA-AAAAA-AAAAA-AAAAA-AAAAA";
			this.mtxt_key.Name = "mtxt_key";
			this.mtxt_key.Size = new System.Drawing.Size(249, 22);
			this.mtxt_key.TabIndex = 9;
			// 
			// LicenseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(437, 533);
			this.Controls.Add(this.mtxt_key);
			this.Controls.Add(this.lbl_hint);
			this.Controls.Add(this.lbl_caption);
			this.Controls.Add(this.lbl_heading);
			this.Controls.Add(this.lbl_info);
			this.Controls.Add(this.grp_info);
			this.Controls.Add(this.btn_telephone);
			this.Controls.Add(this.btn_onlineActivate);
			this.Name = "LicenseForm";
			this.Text = "LicenseForm";
			this.Load += new System.EventHandler(this.LicenseFormLoad);
			this.grp_info.ResumeLayout(false);
			this.grp_info.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Label lbl_capKontakt;
		private System.Windows.Forms.Label lbl_tel1;
		private System.Windows.Forms.Button btn_activate;
		private System.Windows.Forms.TextBox txt_activationCode;
		private System.Windows.Forms.Label lbl_capRequ;
		private System.Windows.Forms.Label lbl_activation;
		private System.Windows.Forms.Label lbl_requestCode;
		private System.Windows.Forms.Label lbl_contact;
		private System.Windows.Forms.GroupBox grp_info;
		private System.Windows.Forms.MaskedTextBox mtxt_key;
		private System.Windows.Forms.Label lbl_hint;
		private System.Windows.Forms.Label lbl_caption;
		private System.Windows.Forms.Label lbl_heading;
		private System.Windows.Forms.Label lbl_info;
		private System.Windows.Forms.Button btn_telephone;
		private System.Windows.Forms.Button btn_onlineActivate;
	}
}
