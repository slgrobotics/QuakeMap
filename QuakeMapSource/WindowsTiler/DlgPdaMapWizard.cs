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
using WindowsTiler.DlgControls;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPdaMapWizard.
	/// </summary>
	public class DlgPdaMapWizard : System.Windows.Forms.Form, IEnableDisable
	{
		private IWizardStepControl[] m_wizardSteps;
		int m_step = 1;

		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Panel switchPanel;
		private System.Windows.Forms.Button nextButton;
		private System.Windows.Forms.LinkLabel helpExportLinkLabel;
		private System.Windows.Forms.Panel measurePanel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgPdaMapWizard()
		{
			InitializeComponent();

			m_wizardSteps = new IWizardStepControl[] {
														new MapWizardStep1(),
														new MapWizardStep2(),
														new MapWizardStep3(),
														new MapWizardStep4(this)
											 };

			doStep(1);

			Project.setDlgIcon(this);
		}

		private void doStep(int step)
		{
			m_step = step;
			if(step > 1)
			{
				m_wizardSteps[m_step - 2].deactivate(this);
			}
			IWizardStepControl wsc = m_wizardSteps[m_step - 1];
			((UserControl)wsc).Dock = DockStyle.Fill;
			switchPanel.Controls.Clear();
			switchPanel.Controls.Add((UserControl)wsc);
			nextButton.Text = wsc.getNextText();
			wsc.activate(this);
		}

		// implementation of IEnableDisable:
		public void enable(int feature)
		{
			switch(feature)
			{
				case 1:
					nextButton.Visible = true;
					break;
				case 3:
					m_heightFull = this.Height;
					this.Height = ((MapWizardStep3)m_wizardSteps[2]).measurePanel.Height + this.measurePanel.Height + 30;
					break;
				default:
					nextButton.Enabled = true;
					break;
			}
		}

		private int m_heightFull = 0;

		public void disable(int feature)
		{
			switch(feature)
			{
				case 1:
					nextButton.Visible = false;
					closeButton.Text = "Close";
					break;
				case 2:
					nextButton.Enabled = false;
				{
					string text = "Zoom out far enough to see all the zoom centers you want to get tiles for\r\n(10 to 40 miles Camera Altitude recommended),\r\n\r\n"
						+ "Hold the left mouse button and drag the mouse to make a rectangle,\r\nselecting smaller areas (no more than 4x4 miles) around a zoom center.\r\n"
						+ "Click inside the rectangle to copy all the tiles it contains to the Export Folder.\r\n"
						+ "Repeat for any other zoom centers you want to cover.\r\n\r\n"
						+ "Click \"Next\" when you have exported all the tiles you need.";

					Project.ShowPopup(nextButton, text, Point.Empty);
				}
					break;
				case 3:
					this.Height = m_heightFull;
					break;
				default:
					nextButton.Enabled = false;
					break;
			}
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
			this.switchPanel = new System.Windows.Forms.Panel();
			this.nextButton = new System.Windows.Forms.Button();
			this.helpExportLinkLabel = new System.Windows.Forms.LinkLabel();
			this.measurePanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// closeButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(414, 256);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 25);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Cancel";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// switchPanel
			// 
			this.switchPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.switchPanel.Name = "switchPanel";
			this.switchPanel.Size = new System.Drawing.Size(500, 251);
			this.switchPanel.TabIndex = 4;
			// 
			// nextButton
			// 
			this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.nextButton.Location = new System.Drawing.Point(222, 256);
			this.nextButton.Name = "nextButton";
			this.nextButton.Size = new System.Drawing.Size(63, 25);
			this.nextButton.TabIndex = 5;
			this.nextButton.Text = "Next >>";
			this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
			// 
			// helpExportLinkLabel
			// 
			this.helpExportLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.helpExportLinkLabel.Location = new System.Drawing.Point(332, 261);
			this.helpExportLinkLabel.Name = "helpExportLinkLabel";
			this.helpExportLinkLabel.Size = new System.Drawing.Size(56, 18);
			this.helpExportLinkLabel.TabIndex = 21;
			this.helpExportLinkLabel.TabStop = true;
			this.helpExportLinkLabel.Text = "Help";
			this.helpExportLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.helpExportLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpExportLinkLabel_LinkClicked);
			// 
			// measurePanel
			// 
			this.measurePanel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.measurePanel.Location = new System.Drawing.Point(5, 250);
			this.measurePanel.Name = "measurePanel";
			this.measurePanel.Size = new System.Drawing.Size(25, 35);
			this.measurePanel.TabIndex = 22;
			// 
			// DlgPdaMapWizard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(499, 286);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.measurePanel,
																		  this.helpExportLinkLabel,
																		  this.nextButton,
																		  this.switchPanel,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Location = new System.Drawing.Point(10, 10);
			this.Name = "DlgPdaMapWizard";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "PDA Memory Card Map Wizard";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgPdaMapWizard_Closing);
			this.Load += new System.EventHandler(this.DlgPdaMapWizard_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void nextButton_Click(object sender, System.EventArgs e)
		{
			if(m_step < m_wizardSteps.Length + 1)
			{
				doStep(m_step + 1);			// re-assigns m_step
			}
			if(m_step >= m_wizardSteps.Length)
			{
				nextButton.Enabled = false;
			}
		}

		private void helpExportLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser("http://www.mapadvisor.com/help/qmmapwizard.html");
		}

		private void DlgPdaMapWizard_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(TerraserverCache.TileNamesCollection != null)
			{
				TerraserverCache.TileNamesCollection.Clear();
				TerraserverCache.TileNamesCollection = null;
			}
			Project.pdaWizardStepControl = null;
			Project.pdaWellSelectingMode = false;
			PictureManager.This.CameraManager.resetDrag(false);
			PictureManager.This.Refresh();
		}

		private void DlgPdaMapWizard_Load(object sender, System.EventArgs e)
		{
			PictureManager.This.CameraManager.resetDrag(true);
		}
	}

}
