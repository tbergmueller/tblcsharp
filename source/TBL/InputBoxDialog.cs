using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using TBL.EDOLL;

namespace TBL
{  
  internal class InputBoxDialog : System.Windows.Forms.Form
  { 

    #region Windows Contols and Constructor

    private System.Windows.Forms.Label lblPrompt;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox txtInput;
    
    
    private System.ComponentModel.Container components = null;

    public InputBoxDialog()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
    }

    #endregion

    #region Dispose

   
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if(components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    #endregion

    #region Windows Form Designer generated code
   
    private void InitializeComponent()
    {
    	this.lblPrompt = new System.Windows.Forms.Label();
    	this.txtInput = new System.Windows.Forms.TextBox();
    	this.btnOk = new System.Windows.Forms.Button();
    	this.button1 = new System.Windows.Forms.Button();
    	this.chk_1 = new System.Windows.Forms.CheckBox();
    	this.chk_2 = new System.Windows.Forms.CheckBox();
    	this.chk_3 = new System.Windows.Forms.CheckBox();
    	this.chk_4 = new System.Windows.Forms.CheckBox();
    	this.chk_5 = new System.Windows.Forms.CheckBox();
    	this.chk_6 = new System.Windows.Forms.CheckBox();
    	this.chk_0 = new System.Windows.Forms.CheckBox();
    	
    	
    	chk_1.Visible = false;
    	chk_2.Visible = false;
    	chk_3.Visible = false;
    	chk_4.Visible = false;
    	chk_5.Visible = false;
    	chk_6.Visible = false;
    	chk_0.Visible = false;
    	
    	
    	this.SuspendLayout();
    	// 
    	// lblPrompt
    	// 
    	this.lblPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
    	    	    	| System.Windows.Forms.AnchorStyles.Left) 
    	    	    	| System.Windows.Forms.AnchorStyles.Right)));
    	this.lblPrompt.BackColor = System.Drawing.SystemColors.Control;
    	this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.lblPrompt.Location = new System.Drawing.Point(12, 9);
    	this.lblPrompt.Name = "lblPrompt";
    	this.lblPrompt.Size = new System.Drawing.Size(265, 55);
    	this.lblPrompt.TabIndex = 3;
    	// 
    	// txtInput
    	// 
    	this.txtInput.Location = new System.Drawing.Point(8, 100);
    	this.txtInput.Name = "txtInput";
    	this.txtInput.Size = new System.Drawing.Size(379, 20);
    	this.txtInput.TabIndex = 0;
    	this.txtInput.TextChanged += new System.EventHandler(this.TxtInputTextChanged);
    	// 
    	// btnOk
    	// 
    	this.btnOk.Location = new System.Drawing.Point(311, 12);
    	this.btnOk.Name = "btnOk";
    	this.btnOk.Size = new System.Drawing.Size(75, 23);
    	this.btnOk.TabIndex = 4;
    	this.btnOk.Text = "&OK";
    	this.btnOk.UseVisualStyleBackColor = true;
    	this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
    	// 
    	// button1
    	// 
    	this.button1.Location = new System.Drawing.Point(311, 41);
    	this.button1.Name = "button1";
    	this.button1.Size = new System.Drawing.Size(75, 23);
    	this.button1.TabIndex = 5;
    	this.button1.Text = "Abbrechen";
    	this.button1.UseVisualStyleBackColor = true;
    	this.button1.Click += new System.EventHandler(this.Button1Click);
    	
    	// 
    	// InputBoxDialog
    	// 
    	this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
    	this.ClientSize = new System.Drawing.Size(398, 134);
    	this.Controls.Add(this.chk_0);
    	this.Controls.Add(this.chk_6);
    	this.Controls.Add(this.chk_5);
    	this.Controls.Add(this.chk_4);
    	this.Controls.Add(this.chk_3);
    	this.Controls.Add(this.chk_2);
    	this.Controls.Add(this.chk_1);
    	this.Controls.Add(this.button1);
    	this.Controls.Add(this.btnOk);
    	this.Controls.Add(this.txtInput);
    	this.Controls.Add(this.lblPrompt);
    	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
    	this.MaximizeBox = false;
    	this.MinimizeBox = false;
    	this.Name = "InputBoxDialog";
    	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
    	this.Text = "InputBox";
    	this.Load += new System.EventHandler(this.InputBox_Load);
    	this.ResumeLayout(false);
    	this.PerformLayout();
    }
    
    private System.Windows.Forms.CheckBox chk_0;
    private System.Windows.Forms.CheckBox chk_6;
    private System.Windows.Forms.CheckBox chk_5;
    private System.Windows.Forms.CheckBox chk_4;
    private System.Windows.Forms.CheckBox chk_3;
    private System.Windows.Forms.CheckBox chk_2;
    private System.Windows.Forms.CheckBox chk_1;
    private System.Windows.Forms.Button btnOk;
    #endregion

    #region Private Variables
    string formCaption = string.Empty;
    string formPrompt = string.Empty;
    string inputResponse = string.Empty;
    string defaultValue = string.Empty;
    #endregion

    private void showDayBoxes()
    {
    	// 
    	// chk_1
    	// 
    	this.chk_1.Checked = true;
    	this.chk_1.CheckState = System.Windows.Forms.CheckState.Checked;
    	this.chk_1.Location = new System.Drawing.Point(8, 70);
    	this.chk_1.Name = "chk_1";
    	this.chk_1.Size = new System.Drawing.Size(49, 24);
    	this.chk_1.TabIndex = 6;
    	this.chk_1.Text = "Mo";
    	this.chk_1.UseVisualStyleBackColor = true;
    	this.chk_1.Visible = true;
    	this.chk_1.CheckedChanged += new System.EventHandler(this.chkBoxCheckedChanged);
    	// 
    	// chk_2
    	// 
    	this.chk_2.Checked = true;
    	this.chk_2.CheckState = System.Windows.Forms.CheckState.Checked;
    	this.chk_2.Location = new System.Drawing.Point(63, 70);
    	this.chk_2.Name = "chk_2";
    	this.chk_2.Size = new System.Drawing.Size(49, 24);
    	this.chk_2.TabIndex = 6;
    	this.chk_2.Text = "Di";
    	this.chk_2.UseVisualStyleBackColor = true;
    	this.chk_2.Visible = true;
    	this.chk_2.CheckedChanged += new System.EventHandler(this.chkBoxCheckedChanged);
    	// 
    	// chk_3
    	// 
    	this.chk_3.Checked = true;
    	this.chk_3.CheckState = System.Windows.Forms.CheckState.Checked;
    	this.chk_3.Location = new System.Drawing.Point(118, 70);
    	this.chk_3.Name = "chk_3";
    	this.chk_3.Size = new System.Drawing.Size(49, 24);
    	this.chk_3.TabIndex = 6;
    	this.chk_3.Text = "Mi";
    	this.chk_3.Visible = true;
    	this.chk_3.UseVisualStyleBackColor = true;
    	this.chk_3.CheckedChanged += new System.EventHandler(this.chkBoxCheckedChanged);
    	// 
    	// chk_4
    	// 
    	this.chk_4.Checked = true;
    	this.chk_4.CheckState = System.Windows.Forms.CheckState.Checked;
    	this.chk_4.Location = new System.Drawing.Point(173, 70);
    	this.chk_4.Name = "chk_4";
    	this.chk_4.Size = new System.Drawing.Size(49, 24);
    	this.chk_4.TabIndex = 6;
    	this.chk_4.Text = "Do";
    	this.chk_4.UseVisualStyleBackColor = true;
    	this.chk_4.Visible = true;
    	this.chk_4.CheckedChanged += new System.EventHandler(this.chkBoxCheckedChanged);
    	// 
    	// chk_5
    	// 
    	this.chk_5.Checked = true;
    	this.chk_5.CheckState = System.Windows.Forms.CheckState.Checked;
    	this.chk_5.Location = new System.Drawing.Point(228, 70);
    	this.chk_5.Name = "chk_5";
    	this.chk_5.Size = new System.Drawing.Size(49, 24);
    	this.chk_5.TabIndex = 6;
    	this.chk_5.Text = "Fr";
    	this.chk_5.UseVisualStyleBackColor = true;
    	this.chk_5.Visible = true;
    	this.chk_5.CheckedChanged += new System.EventHandler(this.chkBoxCheckedChanged);
    	// 
    	// chk_6
    	// 
    	this.chk_6.Checked = true;
    	this.chk_6.CheckState = System.Windows.Forms.CheckState.Checked;
    	this.chk_6.Location = new System.Drawing.Point(283, 70);
    	this.chk_6.Name = "chk_6";
    	this.chk_6.Size = new System.Drawing.Size(49, 24);
    	this.chk_6.TabIndex = 6;
    	this.chk_6.Text = "Sa";
    	this.chk_6.Visible = true;
    	this.chk_6.UseVisualStyleBackColor = true;
    	this.chk_6.CheckedChanged += new System.EventHandler(this.chkBoxCheckedChanged);
    	// 
    	// chk_0
    	// 
    	this.chk_0.Checked = true;
    	this.chk_0.CheckState = System.Windows.Forms.CheckState.Checked;
    	this.chk_0.Location = new System.Drawing.Point(337, 70);
    	this.chk_0.Name = "chk_0";
    	this.chk_0.Size = new System.Drawing.Size(49, 24);
    	this.chk_0.TabIndex = 6;
    	this.chk_0.Text = "So";
    	this.chk_0.UseVisualStyleBackColor = true;
    	this.chk_0.Visible = true;
    	this.chk_0.CheckedChanged += new System.EventHandler(this.chkBoxCheckedChanged);
    }
    
    #region Public Properties
    public string FormCaption
    {
      get{return formCaption;}
      set{formCaption = value;}
    } // property FormCaption
    public string FormPrompt
    {
      get{return formPrompt;}
      set{formPrompt = value;}
    } // property FormPrompt
    public string InputResponse
    {
      get{return inputResponse;}
      set{inputResponse = value;}
    } // property InputResponse
   
    public string DefaultValue
    {
      get{return defaultValue;}
      set{defaultValue = value;}
    } // property DefaultValue
    
   

    #endregion

    #region Form and Control Events
    private void InputBox_Load(object sender, System.EventArgs e)
    {
      this.txtInput.Text=defaultValue;
      this.lblPrompt.Text=formPrompt;
      this.Text=formCaption;
      this.txtInput.SelectionStart=0;
      this.txtInput.SelectionLength=this.txtInput.Text.Length;
      this.txtInput.Focus();
      this.KeyPreview = true;
      
      this.lblPrompt.Font = new Font("Verdana, Arial", 8);
      
      
    }

    private void btnOK_Click(object sender, System.EventArgs e)
    {
      InputResponse = this.txtInput.Text;
      this.Close();
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }
    #endregion

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		
		if(e.KeyCode == Keys.Enter)
		{
			btnOK_Click(this, new EventArgs());
		}
	}
    
    void BtnOkClick(object sender, EventArgs e)
    {
    	btnOK_Click(sender, e);
    }
    
    void TxtInputTextChanged(object sender, EventArgs e)
    {
    	
    }
    
    void Button1Click(object sender, EventArgs e)
    {
    	this.DialogResult = DialogResult.Cancel;
    	this.Close();
    }
    
    void chkBoxCheckedChanged(object sender, EventArgs e)
    {
    	//effektSet.toggleDay(Convert.ToInt32((sender as CheckBox).Name.Replace("chk_", "")));
    }
  }
}