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

using LibSys;

namespace Wrapper
{
	/// <summary>
	/// Summary description for DlgDisableProgram.
	/// </summary>
	public class DlgDisableProgram : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.LinkLabel downloadLinkLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgDisableProgram(int reason)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			switch(reason)
			{
				case 0:		// trial expired
				case 1:
					messageLabel.Text = "Program Expired. Please download new version at www." + Project.WEBSITE_NAME_HUMAN + "\n\n\n"
						+ "Thank you for trying " + Project.PROGRAM_NAME_HUMAN + ".";
					break;
				case 2:
					messageLabel.Text = "You need to accept terms of End User License Agreement.\n\n\n"
						+ "Thank you for trying " + Project.PROGRAM_NAME_HUMAN + ".";
					break;
				case 3:
					messageLabel.Text = "Error: Corrupted installation.\n\nPlease download new version at www." + Project.WEBSITE_NAME_HUMAN + "\n";
					break;
			}
			Project.setDlgIcon(this);
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
			this.closeButton = new System.Windows.Forms.Button();
			this.messageLabel = new System.Windows.Forms.Label();
			this.downloadLinkLabel = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(460, 23);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(69, 21);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(22, 45);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(424, 197);
			this.messageLabel.TabIndex = 1;
			// 
			// downloadLinkLabel
			// 
			this.downloadLinkLabel.Location = new System.Drawing.Point(20, 10);
			this.downloadLinkLabel.Name = "downloadLinkLabel";
			this.downloadLinkLabel.Size = new System.Drawing.Size(138, 22);
			this.downloadLinkLabel.TabIndex = 2;
			this.downloadLinkLabel.TabStop = true;
			this.downloadLinkLabel.Text = "download new version";
			this.downloadLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.downloadLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadLinkLabel_LinkClicked);
			// 
			// DlgDisableProgram
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(540, 261);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.downloadLinkLabel,
																		  this.messageLabel,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DlgDisableProgram";
			this.Text = "Program Disabled";
			this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void downloadLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(Project.DLOAD_URL);
		}
	}
}
