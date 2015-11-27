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
	/// Summary description for OptionsLandmarksForm.
	/// </summary>
	public class OptionsLandmarksForm : System.Windows.Forms.Form
	{
		protected PictureManager m_pictureManager;
		protected bool m_getLmInfo;
		private CheckBox[] checkboxes = new CheckBox[TerraserverCache.landmarkPointTypes.Length];

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button selectAllButton;
		private System.Windows.Forms.Button selectNoneButton;
		private System.Windows.Forms.Button closeButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OptionsLandmarksForm(PictureManager pictureManager, bool getLmInfo)
		{
			m_pictureManager = pictureManager;
			m_getLmInfo = getLmInfo;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			try
			{
				if(checkboxes.Length > 0)
				{
					this.SuspendLayout();
					int i = 0;
					int y = 18;
					foreach(string lpt in TerraserverCache.landmarkPointTypes)
					{
						checkboxes[i] = new CheckBox();
						checkboxes[i].Location = new System.Drawing.Point(16, y);
						checkboxes[i].Name = "checkboxxx" + i;
						checkboxes[i].Size = new System.Drawing.Size(224, 24);
						checkboxes[i].TabIndex = 1;
						checkboxes[i].Text = lpt;
						checkboxes[i].Checked = TerraserverCache.landmarkPointShow[i];
						y += 24;
						i++;
					}
					this.Controls.AddRange(checkboxes);
					this.ClientSize = new System.Drawing.Size(352, y + 30);
					this.ResumeLayout();
				}
			} 
			catch {}

			Project.setDlgIcon(this);
			closeButton.Focus();
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
			this.okButton = new System.Windows.Forms.Button();
			this.selectAllButton = new System.Windows.Forms.Button();
			this.selectNoneButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(224, 88);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(96, 21);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "Apply";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// selectAllButton
			// 
			this.selectAllButton.Location = new System.Drawing.Point(224, 24);
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.Size = new System.Drawing.Size(96, 22);
			this.selectAllButton.TabIndex = 1;
			this.selectAllButton.Text = "Select All";
			this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
			// 
			// selectNoneButton
			// 
			this.selectNoneButton.Location = new System.Drawing.Point(224, 56);
			this.selectNoneButton.Name = "selectNoneButton";
			this.selectNoneButton.Size = new System.Drawing.Size(96, 22);
			this.selectNoneButton.TabIndex = 2;
			this.selectNoneButton.Text = "Select None";
			this.selectNoneButton.Click += new System.EventHandler(this.selectNoneButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(224, 368);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(96, 21);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// OptionsLandmarksForm
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(330, 404);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.closeButton,
																		  this.selectNoneButton,
																		  this.selectAllButton,
																		  this.okButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "OptionsLandmarksForm";
			this.Text = "Display Landmarks by Type";
			this.ResumeLayout(false);

		}
		#endregion

		private void okButton_Click(object sender, System.EventArgs e)
		{
			for(int i=0; i < checkboxes.Length ;i++)
			{
				TerraserverCache.landmarkPointShow[i] = checkboxes[i].Checked;
			}
			if(m_getLmInfo)
			{
				// if we got here because lm checkbox was just checked, we need to go to LM server for landmarks:
				m_pictureManager.CameraManager.ProcessCameraMove();
			}
			else
			{
				// if we are just changing set of types, refresh works (given that we have whole set in cache anyway):
				m_pictureManager.Refresh();
			}
		}

		private void selectAllButton_Click(object sender, System.EventArgs e)
		{
			foreach(CheckBox cb in checkboxes)
			{
				cb.Checked = true;
			}
		}

		private void selectNoneButton_Click(object sender, System.EventArgs e)
		{
			foreach(CheckBox cb in checkboxes)
			{
				cb.Checked = false;
			}
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			m_pictureManager.CameraManager.ProcessCameraMove();
			this.Close();
		}
	}
}
