/*
 * Created by SharpDevelop.
 * User: tbergmueller
 * Date: 02.11.2011
 * Time: 20:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace M3S_Analyzer
{
	partial class MainForm
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
			this.mnu_main = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnu_ctrl = new System.Windows.Forms.ToolStripMenuItem();
			this.mnu_start = new System.Windows.Forms.ToolStripMenuItem();
			this.mnu_stop = new System.Windows.Forms.ToolStripMenuItem();
			this.createDummyContentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnu_main.SuspendLayout();
			this.SuspendLayout();
			// 
			// mnu_main
			// 
			this.mnu_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.fileToolStripMenuItem,
									this.mnu_ctrl});
			this.mnu_main.Location = new System.Drawing.Point(0, 0);
			this.mnu_main.Name = "mnu_main";
			this.mnu_main.Size = new System.Drawing.Size(900, 24);
			this.mnu_main.TabIndex = 2;
			this.mnu_main.Text = "Main Menu";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// mnu_ctrl
			// 
			this.mnu_ctrl.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.mnu_start,
									this.mnu_stop,
									this.createDummyContentToolStripMenuItem});
			this.mnu_ctrl.Name = "mnu_ctrl";
			this.mnu_ctrl.Size = new System.Drawing.Size(59, 20);
			this.mnu_ctrl.Text = "Control";
			// 
			// mnu_start
			// 
			this.mnu_start.Name = "mnu_start";
			this.mnu_start.Size = new System.Drawing.Size(194, 22);
			this.mnu_start.Text = "Start";
			this.mnu_start.Click += new System.EventHandler(this.Mnu_startClick);
			// 
			// mnu_stop
			// 
			this.mnu_stop.Name = "mnu_stop";
			this.mnu_stop.Size = new System.Drawing.Size(194, 22);
			this.mnu_stop.Text = "Stop";
			this.mnu_stop.Click += new System.EventHandler(this.Mnu_stopClick);
			// 
			// createDummyContentToolStripMenuItem
			// 
			this.createDummyContentToolStripMenuItem.Name = "createDummyContentToolStripMenuItem";
			this.createDummyContentToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
			this.createDummyContentToolStripMenuItem.Text = "CreateDummyContent";
			this.createDummyContentToolStripMenuItem.Click += new System.EventHandler(this.CreateDummyContentToolStripMenuItemClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(900, 451);
			this.Controls.Add(this.mnu_main);
			this.MainMenuStrip = this.mnu_main;
			this.Name = "MainForm";
			this.Text = "M3S_Analyzer";
			this.mnu_main.ResumeLayout(false);
			this.mnu_main.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.ToolStripMenuItem createDummyContentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnu_stop;
		private System.Windows.Forms.ToolStripMenuItem mnu_start;
		private System.Windows.Forms.ToolStripMenuItem mnu_ctrl;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.MenuStrip mnu_main;
	}
}
