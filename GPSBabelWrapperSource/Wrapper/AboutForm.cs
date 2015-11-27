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
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using LibSys;
using LibNet;

namespace Wrapper
{
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class AboutForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.LinkLabel aboutLinkLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.LinkLabel eulaLinkLabel;
		private System.Windows.Forms.PictureBox logoPictureBox;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label creditsLabel;
		private System.Windows.Forms.LinkLabel gpsbabelLinkLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AboutForm()
		{
			InitializeComponent();

			this.Text = "About " + Project.PROGRAM_NAME_HUMAN;
			versionLabel.Text = Project.PROGRAM_NAME_HUMAN + " version " + Project.PROGRAM_VERSION_HUMAN + " build " + Project.PROGRAM_VERSION_RELEASEDATE;

			this.gpsbabelLinkLabel.Text = Project.GPSBABEL_URL;

			creditsLabel.Text = "Included here is GPSBabel - a program by Robert Lipe\n\nPlease visit www.gpsbabel.org for more info."; 

			Project.setDlgIcon(this);

			// try loading logo file:
			string logoFileName = Project.GetMiscPath("about.jpg");
			try 
			{
				if(!File.Exists(logoFileName))
				{
					// load the file remotely, don't hold diagnostics if load fails:
					string url = Project.MISC_FOLDER_URL + "/about.jpg";
					DloadProgressForm loaderForm = new DloadProgressForm(url, logoFileName, false, true);
					loaderForm.ShowDialog();
				}
				logoPictureBox.Image = new Bitmap(logoFileName);
			} 
			catch {}
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
			this.aboutLinkLabel = new System.Windows.Forms.LinkLabel();
			this.versionLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.logoPictureBox = new System.Windows.Forms.PictureBox();
			this.creditsLabel = new System.Windows.Forms.Label();
			this.eulaLinkLabel = new System.Windows.Forms.LinkLabel();
			this.gpsbabelLinkLabel = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(288, 280);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(94, 22);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// aboutLinkLabel
			// 
			this.aboutLinkLabel.Location = new System.Drawing.Point(235, 204);
			this.aboutLinkLabel.Name = "aboutLinkLabel";
			this.aboutLinkLabel.Size = new System.Drawing.Size(335, 22);
			this.aboutLinkLabel.TabIndex = 1;
			this.aboutLinkLabel.TabStop = true;
			this.aboutLinkLabel.Text = "http://www.earthquakemap.com";
			this.aboutLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.aboutLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.aboutLinkLabel_LinkClicked);
			// 
			// versionLabel
			// 
			this.versionLabel.Location = new System.Drawing.Point(232, 30);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(343, 22);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.Text = "QuakeMap";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(232, 61);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(343, 21);
			this.label2.TabIndex = 3;
			this.label2.Text = "Copyright (c) 2006... Sergei Grichine";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(232, 182);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(343, 22);
			this.label3.TabIndex = 4;
			this.label3.Text = "To see the latest information click on the link below:";
			// 
			// logoPictureBox
			// 
			this.logoPictureBox.Location = new System.Drawing.Point(15, 23);
			this.logoPictureBox.Name = "logoPictureBox";
			this.logoPictureBox.Size = new System.Drawing.Size(198, 272);
			this.logoPictureBox.TabIndex = 5;
			this.logoPictureBox.TabStop = false;
			// 
			// creditsLabel
			// 
			this.creditsLabel.Location = new System.Drawing.Point(232, 91);
			this.creditsLabel.Name = "creditsLabel";
			this.creditsLabel.Size = new System.Drawing.Size(343, 54);
			this.creditsLabel.TabIndex = 6;
			this.creditsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// eulaLinkLabel
			// 
			this.eulaLinkLabel.Location = new System.Drawing.Point(352, 235);
			this.eulaLinkLabel.Name = "eulaLinkLabel";
			this.eulaLinkLabel.Size = new System.Drawing.Size(219, 22);
			this.eulaLinkLabel.TabIndex = 2;
			this.eulaLinkLabel.TabStop = true;
			this.eulaLinkLabel.Text = "End User License Agreement";
			this.eulaLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.eulaLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.eulaLinkLabel_LinkClicked);
			// 
			// gpsbabelLinkLabel
			// 
			this.gpsbabelLinkLabel.Location = new System.Drawing.Point(235, 146);
			this.gpsbabelLinkLabel.Name = "gpsbabelLinkLabel";
			this.gpsbabelLinkLabel.Size = new System.Drawing.Size(340, 22);
			this.gpsbabelLinkLabel.TabIndex = 7;
			this.gpsbabelLinkLabel.TabStop = true;
			this.gpsbabelLinkLabel.Text = "http://www.gpsbabel.org";
			this.gpsbabelLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.gpsbabelLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.gpsbabelLinkLabel_LinkClicked);
			// 
			// AboutForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(586, 314);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.gpsbabelLinkLabel,
																		  this.eulaLinkLabel,
																		  this.creditsLabel,
																		  this.logoPictureBox,
																		  this.label3,
																		  this.label2,
																		  this.versionLabel,
																		  this.aboutLinkLabel,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AboutForm";
			this.Text = "About";
			this.Load += new System.EventHandler(this.AboutForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void AboutForm_Load(object sender, System.EventArgs e)
		{
			aboutLinkLabel.Text = Project.ABOUT_URL;
		}

		private void aboutLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(Project.ABOUT_URL);
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void eulaLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DlgEula dlge = new DlgEula();
			dlge.ShowDialog();
		}

		private void gpsbabelLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(Project.GPSBABEL_URL);
		}
	}
}
