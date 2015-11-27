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

using LibGeo;
using LibSys;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for KmlOptionsPopup.
	/// </summary>
	public class KmlOptionsPopup : System.Windows.Forms.Form
	{
		public bool doContinue = false;

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Panel mainPanel;
		private System.Windows.Forms.CheckBox dontShowCheckBox;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public KmlOptionsPopup()
		{
			InitializeComponent();

			Project.setDlgIcon(this);

			dontShowCheckBox.Checked = Project.kmlOptions.DontShowInPopup;

//			dontShowCheckBox.DataBindings.Add("Checked", Project.kmlOptions, "DontShowInPopup");

			KmlDocumentControl kdc = new KmlDocumentControl();
			kdc.Dock = System.Windows.Forms.DockStyle.Fill;
			mainPanel.Controls.Add(kdc);
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
			this.mainPanel = new System.Windows.Forms.Panel();
			this.dontShowCheckBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.okButton.Location = new System.Drawing.Point(290, 125);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(95, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "Continue";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// mainPanel
			// 
			this.mainPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.mainPanel.Location = new System.Drawing.Point(5, 5);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = new System.Drawing.Size(385, 115);
			this.mainPanel.TabIndex = 1;
			// 
			// dontShowCheckBox
			// 
			this.dontShowCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.dontShowCheckBox.Location = new System.Drawing.Point(10, 125);
			this.dontShowCheckBox.Name = "dontShowCheckBox";
			this.dontShowCheckBox.Size = new System.Drawing.Size(270, 24);
			this.dontShowCheckBox.TabIndex = 2;
			this.dontShowCheckBox.Text = "do not show this dialog";
			this.dontShowCheckBox.CheckedChanged += new System.EventHandler(this.dontShowCheckBox_CheckedChanged);
			// 
			// KmlOptionsPopup
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(394, 156);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.dontShowCheckBox,
																		  this.mainPanel,
																		  this.okButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "KmlOptionsPopup";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Confirm KML Options";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		private void okButton_Click(object sender, System.EventArgs e)
		{
			doContinue = true;
			Close();
		}

		private void dontShowCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.kmlOptions.DontShowInPopup = dontShowCheckBox.Checked;
		}
	}
}
