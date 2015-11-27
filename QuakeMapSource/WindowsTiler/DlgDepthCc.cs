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

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgDepthCc.
	/// </summary>
	public class DlgDepthCc : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox depthCcPictureBox;
		private static DlgDepthCc This = null;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgDepthCc()
		{
			if(This != null)
			{
				This.Dispose();
				//This.Hide();
				//This.Close();
			}
			This = this;

			InitializeComponent();
		}

		private void DlgDepthCc_Load(object sender, System.EventArgs e)
		{
			string imageFileName = Project.GetMiscPath("depthcc.gif");
			try 
			{
				if(!File.Exists(imageFileName))
				{
					// load the file remotely, don't hold diagnostics if load fails:
					string url = Project.MISC_FOLDER_URL + "/depthcc.gif";
//					DloadProgressForm loaderForm = new DloadProgressForm(url, imageFileName, false, true);
//					loaderForm.ShowDialog();
					new DloadNoForm(url, imageFileName, 3000);
				}
				this.depthCcPictureBox.Image = new Bitmap(imageFileName);
				Project.setDlgIcon(this);
			} 
			catch
			{
				// probably no connection, do not bring it up.
				Hide();
				Close();
				This = null;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(this.depthCcPictureBox.Image != null)
				{
					this.depthCcPictureBox.Image.Dispose();
					this.depthCcPictureBox.Image = null;
				}
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
			this.depthCcPictureBox = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// depthCcPictureBox
			// 
			this.depthCcPictureBox.Location = new System.Drawing.Point(7, 8);
			this.depthCcPictureBox.Name = "depthCcPictureBox";
			this.depthCcPictureBox.Size = new System.Drawing.Size(110, 155);
			this.depthCcPictureBox.TabIndex = 13;
			this.depthCcPictureBox.TabStop = false;
			// 
			// DlgDepthCc
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(124, 174);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.depthCcPictureBox});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgDepthCc";
			this.Text = "Earthquakes";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgDepthCc_Closing);
			this.Load += new System.EventHandler(this.DlgDepthCc_Load);
			this.ResumeLayout(false);

		}
		#endregion

		protected override bool ProcessDialogKey(Keys keyData)
		{
			//LibSys.StatusBar.Trace("ProcessDialogKey: " + keyData);

			bool keystrokeProcessed = true;
			switch (keyData) 
			{
				case Keys.Escape:
				case Keys.Alt | Keys.F4:
					this.Close();
					break;
				default:
					keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					break;
			}
			if(keystrokeProcessed) 
			{
				return true;
			} 
			else 
			{
				return base.ProcessDialogKey(keyData);
			}
		}

		private void DlgDepthCc_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			This = null;
		}
	}
}
