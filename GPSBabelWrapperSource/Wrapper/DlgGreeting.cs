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
using LibNet;

namespace Wrapper
{
	/// <summary>
	/// Summary description for DlgGreeting.
	/// </summary>
	public class DlgGreeting : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.Label warningLabel;
		private System.Windows.Forms.Label headerLabel;
		private System.Windows.Forms.Button offlineButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgGreeting(string warning, string info)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			headerLabel.Text = Project.PROGRAM_NAME_HUMAN + " " + Project.PROGRAM_VERSION_HUMAN;
			warningLabel.Text = warning;
			infoLabel.Text = info;
			if(info.StartsWith("Error"))
			{
				infoLabel.ForeColor = Color.Red;
				this.Text = Project.PROGRAM_NAME_HUMAN + " - Error";
			}
			else
			{
				this.Text = Project.PROGRAM_NAME_HUMAN;
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
			this.infoLabel = new System.Windows.Forms.Label();
			this.warningLabel = new System.Windows.Forms.Label();
			this.headerLabel = new System.Windows.Forms.Label();
			this.offlineButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// infoLabel
			// 
			this.infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.infoLabel.Location = new System.Drawing.Point(24, 160);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(472, 120);
			this.infoLabel.TabIndex = 1;
			// 
			// warningLabel
			// 
			this.warningLabel.Font = new System.Drawing.Font("Arial", 12F, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.warningLabel.ForeColor = System.Drawing.Color.Red;
			this.warningLabel.Location = new System.Drawing.Point(24, 80);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Size = new System.Drawing.Size(469, 72);
			this.warningLabel.TabIndex = 2;
			this.warningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// headerLabel
			// 
			this.headerLabel.Font = new System.Drawing.Font("Arial", 18F, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.headerLabel.Location = new System.Drawing.Point(22, 16);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(469, 56);
			this.headerLabel.TabIndex = 3;
			this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// offlineButton
			// 
			this.offlineButton.Location = new System.Drawing.Point(388, 282);
			this.offlineButton.Name = "offlineButton";
			this.offlineButton.Size = new System.Drawing.Size(105, 28);
			this.offlineButton.TabIndex = 4;
			this.offlineButton.Text = "Work Offline";
			this.offlineButton.Click += new System.EventHandler(this.offlineButton_Click);
			// 
			// DlgGreeting
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(0)), ((System.Byte)(0)), ((System.Byte)(50)));
			this.ClientSize = new System.Drawing.Size(512, 320);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.offlineButton,
																		  this.headerLabel,
																		  this.warningLabel,
																		  this.infoLabel});
			this.ForeColor = System.Drawing.Color.Gold;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "DlgGreeting";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.ResumeLayout(false);

		}
		#endregion

		private void offlineButton_Click(object sender, System.EventArgs e)
		{
			Project.serverAvailable = false;
			offlineButton.Enabled = false;
			try 
			{
				WebDownload.allDoneLast.Set();		// allDoneLast may be null, if WebDownload is not active
			}
			catch {}
		}
	}
}
