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
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPdaHelp.
	/// </summary>
	public class DlgPdaHelp : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.LinkLabel pdaInfoLinkLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgPdaHelp()
		{
			InitializeComponent();

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
			this.label2 = new System.Windows.Forms.Label();
			this.closeButton = new System.Windows.Forms.Button();
			this.pdaInfoLinkLabel = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(16, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(440, 48);
			this.label2.TabIndex = 6;
			this.label2.Text = "PDA support not available yet. Please check quakemap.com for the latest news";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(392, 184);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(68, 22);
			this.closeButton.TabIndex = 5;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// pdaInfoLinkLabel
			// 
			this.pdaInfoLinkLabel.Location = new System.Drawing.Point(200, 88);
			this.pdaInfoLinkLabel.Name = "pdaInfoLinkLabel";
			this.pdaInfoLinkLabel.Size = new System.Drawing.Size(256, 23);
			this.pdaInfoLinkLabel.TabIndex = 7;
			this.pdaInfoLinkLabel.TabStop = true;
			this.pdaInfoLinkLabel.Text = "click here for the latest news on PDA support";
			this.pdaInfoLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.pdaInfoLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.pdaInfoLinkLabel_LinkClicked);
			// 
			// DlgPdaHelp
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(472, 215);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.pdaInfoLinkLabel,
																		  this.label2,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgPdaHelp";
			this.Text = "PDA Support Note";
			this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void pdaInfoLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(Project.PDA_URL);
		}
	}
}
