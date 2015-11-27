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

namespace LibSys
{
	/// <summary>
	/// Summary description for DlgUpgradeMessage.
	/// </summary>
	public class DlgUpgradeMessage : System.Windows.Forms.Form
	{
		private string m_message;

		private System.Windows.Forms.Button noButton;
		private System.Windows.Forms.Button yesButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label messageLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgUpgradeMessage(string message)
		{
			m_message = message;

			InitializeComponent();

			this.Text = Project.PROGRAM_NAME_HUMAN + " - Update Available";

			this.messageLabel.Text = m_message;
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
			this.noButton = new System.Windows.Forms.Button();
			this.yesButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.messageLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// noButton
			// 
			this.noButton.Location = new System.Drawing.Point(320, 255);
			this.noButton.Name = "noButton";
			this.noButton.Size = new System.Drawing.Size(112, 23);
			this.noButton.TabIndex = 0;
			this.noButton.Text = "No, later";
			this.noButton.Click += new System.EventHandler(this.noButton_Click);
			// 
			// yesButton
			// 
			this.yesButton.Location = new System.Drawing.Point(192, 255);
			this.yesButton.Name = "yesButton";
			this.yesButton.Size = new System.Drawing.Size(112, 24);
			this.yesButton.TabIndex = 1;
			this.yesButton.Text = "Update Now";
			this.yesButton.Click += new System.EventHandler(this.yesButton_Click);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(25, 210);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(408, 40);
			this.label2.TabIndex = 3;
			this.label2.Text = "When you click \"Update Now\", the program will quit and our Upgrade Center page wi" +
				"ll show up in the browser";
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(25, 15);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(408, 184);
			this.messageLabel.TabIndex = 4;
			// 
			// DlgUpgradeMessage
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(458, 293);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.messageLabel,
																		  this.label2,
																		  this.yesButton,
																		  this.noButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DlgUpgradeMessage";
			this.Text = "Update Available";
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
			Project.RunBrowser(Project.UPDATE_URL);
			this.Close();
			Project.Exit();
		}
	}
}
