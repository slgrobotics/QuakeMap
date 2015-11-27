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

using System.IO;
using LibSys;
using LibNet;

namespace Wrapper
{
	/// <summary>
	/// Summary description for DlgEula.
	/// </summary>
	public class DlgEula : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button disagreeButton;
		private System.Windows.Forms.Button agreeButton;
		private System.Windows.Forms.ListBox eulaListBox;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgEula()
		{
			InitializeComponent();

			TextReader reader = null;

            string fileName = "LicenseBSD.txt";

            string filePath = Project.GetMiscPath(fileName);

			try 
			{
				if(File.Exists(fileName)
					&& File.GetLastWriteTime(fileName).AddDays(Project.LICENSE_FILE_MAX_AGE_DAYS).CompareTo(DateTime.Now) > 0)
				{
					reader = new StreamReader(fileName);
				} 
				else
				{
					// load the file remotely, don't hold diagnostics if load fails:
                    string url = Project.MISC_FOLDER_URL + "/" + fileName;
                    DloadProgressForm loaderForm = new DloadProgressForm(url, fileName, false, true);
					loaderForm.ShowDialog();
					reader = new StreamReader(fileName);
				}
			} 
			catch {}

			if(reader == null)
			{
				reader = new StringReader(eulaText);	// in case everything else fails
			}

			string strEula;
			while((strEula=reader.ReadLine()) != null)
			{
				eulaListBox.Items.Add(strEula);
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
			this.disagreeButton = new System.Windows.Forms.Button();
			this.agreeButton = new System.Windows.Forms.Button();
			this.eulaListBox = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// disagreeButton
			// 
			this.disagreeButton.Location = new System.Drawing.Point(400, 416);
			this.disagreeButton.Name = "disagreeButton";
			this.disagreeButton.Size = new System.Drawing.Size(120, 23);
			this.disagreeButton.TabIndex = 3;
			this.disagreeButton.Text = "I Disagree";
			this.disagreeButton.Click += new System.EventHandler(this.disagreeButton_Click);
			// 
			// agreeButton
			// 
			this.agreeButton.Location = new System.Drawing.Point(184, 416);
			this.agreeButton.Name = "agreeButton";
			this.agreeButton.Size = new System.Drawing.Size(120, 23);
			this.agreeButton.TabIndex = 2;
			this.agreeButton.Text = "I Agree";
			this.agreeButton.Click += new System.EventHandler(this.agreeButton_Click);
			// 
			// eulaListBox
			// 
			this.eulaListBox.ItemHeight = 16;
			this.eulaListBox.Location = new System.Drawing.Point(16, 56);
			this.eulaListBox.Name = "eulaListBox";
			this.eulaListBox.Size = new System.Drawing.Size(648, 340);
			this.eulaListBox.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(648, 23);
			this.label1.TabIndex = 19;
			this.label1.Text = "Please scroll down to read whole Agreement";
			// 
			// DlgEula
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(680, 460);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label1,
																		  this.eulaListBox,
																		  this.agreeButton,
																		  this.disagreeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DlgEula";
			this.Text = "End User License Agreement";
			this.ResumeLayout(false);

		}
		#endregion

		private void disagreeButton_Click(object sender, System.EventArgs e)
		{
			new DlgDisableProgram(2).ShowDialog();

			Project.Exit();
		}

		private void agreeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		// if we cannot reach the EULA on the Internet, print a short version:
		private static string eulaText = 
              "This computer software is Copyright (c) 2002..., Sergei Grichine.\n\n"
            + " All rights reserved.\n"
            + "\n"
            + " Redistribution and use in source and binary forms, with or without\n"
            + " modification, are permitted provided that the following conditions are met:\n"
            + "     * Redistributions of source code must retain the above copyright\n"
            + "       notice, this list of conditions and the following disclaimer.\n"
            + "     * Redistributions in binary form must reproduce the above copyright\n"
            + "       notice, this list of conditions and the following disclaimer in the\n"
            + "       documentation and/or other materials provided with the distribution.\n"
            + "     * Neither the name of Sergei Grichine nor the\n"
            + "       names of its contributors may be used to endorse or promote products\n"
            + "       derived from this software without specific prior written permission.\n"
            + "\n"
            + " THIS SOFTWARE IS PROVIDED BY Sergei Grichine ''AS IS'' AND ANY\n"
            + " EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED\n"
            + " WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE\n"
            + " DISCLAIMED. IN NO EVENT SHALL Sergei Grichine BE LIABLE FOR ANY\n"
            + " DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES\n"
            + " (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;\n"
            + " LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND\n"
            + " ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT\n"
            + " (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS\n"
            + " SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.\n"
            + "\n"
            + " this is a X11 (BSD Revised) license - you do not have to publish your changes,\n"
            + " although doing so, donating and contributing is always appreciated\n";
	}
}
