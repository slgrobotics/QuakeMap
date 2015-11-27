/*
* Copyright (c) 2002..., Sergei Grichine
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of Sergei Grichine nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY Sergei Grichine ''AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL Sergei Grichine BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
* this is a X11 (BSD Revised) license - you do not have to publish your changes,
* although doing so, donating and contributing is always appreciated
*/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace LibSys
{
	/// <summary>
	/// Summary description for DlgMigrateTerraFolder.
	/// </summary>
	public class DlgMigrateTerraFolder : System.Windows.Forms.Form
	{
		internal bool migrated = false;

		private const string migrationMessage = "QuakeMap needs to migrate terraserver cache folder {0} to new optimized format.\n\n"
			+ "This one-time optimization process may take long time (10 minutes and more), but will ensure fast operation of the cache in the future.\n\n" 
			+ "You can press \"No\" to skip the migration and will be reminded again later.\nAlso, press \"No\" if your cache is on a CD-ROM or DVD.\n\n"
			+ "Optimize now?";


		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Button yesButton;
		private System.Windows.Forms.Button noButton;
		private System.Windows.Forms.Label progressLabel;
		private System.Windows.Forms.ProgressBar progressBar;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgMigrateTerraFolder(string folderName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			messageLabel.Text = String.Format(migrationMessage, folderName);

			Project.setDlgIcon(this);
			this.BringToFront();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.messageLabel = new System.Windows.Forms.Label();
			this.yesButton = new System.Windows.Forms.Button();
			this.noButton = new System.Windows.Forms.Button();
			this.progressLabel = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(10, 5);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(580, 165);
			this.messageLabel.TabIndex = 0;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// yesButton
			// 
			this.yesButton.Location = new System.Drawing.Point(350, 210);
			this.yesButton.Name = "yesButton";
			this.yesButton.Size = new System.Drawing.Size(112, 24);
			this.yesButton.TabIndex = 3;
			this.yesButton.Text = "Optimize Now";
			this.yesButton.Click += new System.EventHandler(this.yesButton_Click);
			// 
			// noButton
			// 
			this.noButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.noButton.Location = new System.Drawing.Point(475, 210);
			this.noButton.Name = "noButton";
			this.noButton.Size = new System.Drawing.Size(112, 23);
			this.noButton.TabIndex = 2;
			this.noButton.Text = "No, later";
			this.noButton.Click += new System.EventHandler(this.noButton_Click);
			// 
			// progressLabel
			// 
			this.progressLabel.Location = new System.Drawing.Point(10, 210);
			this.progressLabel.Name = "progressLabel";
			this.progressLabel.Size = new System.Drawing.Size(330, 25);
			this.progressLabel.TabIndex = 4;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(5, 175);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(585, 23);
			this.progressBar.TabIndex = 5;
			// 
			// DlgMigrateTerraFolder
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.noButton;
			this.ClientSize = new System.Drawing.Size(599, 241);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.progressBar,
																		  this.progressLabel,
																		  this.yesButton,
																		  this.noButton,
																		  this.messageLabel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgMigrateTerraFolder";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Warning: Mapping Cache needs optimization";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		protected override bool ProcessDialogKey(Keys keyData)
		{
			bool keystrokeProcessed = true;
			switch (keyData) 
			{
				case Keys.Escape:
				case Keys.Alt | Keys.F4:
					break;
				default:
					keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					break;
			}
			if(keystrokeProcessed) 
			{
				this.Close();
				return true;
			} 
			else 
			{
				return base.ProcessDialogKey(keyData);
			}
		}

		private void noButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void yesButton_Click(object sender, System.EventArgs e)
		{
			noButton.Enabled = false;
			yesButton.Enabled = false;

			ThreadPool2.QueueUserWorkItem (new WaitCallback (doMigrate), "terrafolder migration"); 
		}

		private void doMigrate(object obj)
		{
			migrated = Project.migrateTerraserverFiles(this, progressLabel, progressBar);

			noButton.Enabled = true;
			noButton.Text = "Continue";
			yesButton.Visible = false;
			progressLabel.Width += yesButton.Width;
			noButton.Focus();
		}
	}
}
