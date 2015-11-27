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
using LibGui;
using LibSys;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for PhotoPropertiesPopup.
	/// </summary>
	public class PhotoPropertiesPopup : System.Windows.Forms.Form
	{
		public bool canClose = false;

		private Waypoint m_wpt;
		private PhotoDescr m_photoDescr;

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.LinkLabel drawLinkLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PhotoPropertiesPopup()
		{
			InitializeComponent();

			Project.setDlgIcon(this);
		}

		public void setParameters(Waypoint wpt, PhotoDescr photoDescr)
		{
			m_photoDescr = photoDescr;
			m_wpt = wpt;
			parametersChanged();
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.drawLinkLabel = new System.Windows.Forms.LinkLabel();
			this.infoLabel = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.drawLinkLabel,
																				 this.infoLabel});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(347, 174);
			this.panel1.TabIndex = 0;
			// 
			// drawLinkLabel
			// 
			this.drawLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.drawLinkLabel.Location = new System.Drawing.Point(250, 0);
			this.drawLinkLabel.Name = "drawLinkLabel";
			this.drawLinkLabel.Size = new System.Drawing.Size(95, 15);
			this.drawLinkLabel.TabIndex = 118;
			this.drawLinkLabel.TabStop = true;
			this.drawLinkLabel.Text = "draw>>";
			this.drawLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.drawLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.drawLinkLabel_LinkClicked);
			// 
			// infoLabel
			// 
			this.infoLabel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.infoLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.infoLabel.Location = new System.Drawing.Point(5, 20);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(340, 150);
			this.infoLabel.TabIndex = 116;
			this.infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PhotoPropertiesPopup
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(347, 174);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(200, 100);
			this.Name = "PhotoPropertiesPopup";
			this.Opacity = 0.800000011920929;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Photo Details";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.PhotoPropertiesPopup_Closing);
			this.panel1.ResumeLayout(false);
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

		private void PhotoPropertiesPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !canClose;
			this.Hide();
		}

		private void drawLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DlgPhotoDraw dlgPhotoDraw = new DlgPhotoDraw(m_wpt, m_photoDescr);

			if(DlgPhotoDraw.m_dlgSizeX < 100)
			{
				dlgPhotoDraw.Top = drawLinkLabel.PointToScreen(new Point(0, 0)).Y - dlgPhotoDraw.Height;
				dlgPhotoDraw.Left = drawLinkLabel.PointToScreen(new Point(0, 0)).X + drawLinkLabel.Width;
			}
	
			dlgPhotoDraw.ShowDialog();
		}

		private void parametersChanged()
		{
			string text;
			if(m_wpt != null)
			{
				this.Text = "Photo: " + m_wpt.Desc;
				text = m_wpt.toStringPopup();
			}
			else if(m_photoDescr != null)
			{
				this.Text = "Photo: " + m_photoDescr.imageName;
				text = "" + m_photoDescr.imageName + "\n\n" + m_photoDescr.imageSource;
				if(m_photoDescr.DTOrig.CompareTo(new DateTime(1, 1, 1, 0, 0, 0)) > 0)
				{
					text += "\n\n" + m_photoDescr.DTOrig + " (local before shift)    " + m_photoDescr.Width + " x " + m_photoDescr.Height;

					TimeSpan photoTimeShift = Project.photoTimeShiftCurrent;
					TimeSpan timeZoneSpan = MyTimeZone.timeSpanById(Project.photoTimeZoneIdCurrent);
					DateTime timeStamp = m_photoDescr.DTOrig + photoTimeShift + MyTimeZone.zoneTimeShift - timeZoneSpan;

					text += "\n" + timeStamp + " (local after shift - must match a trackpoint)";
				}
				else
				{
					text += "\n\n[no time stamp]   " + m_photoDescr.Width + " x " + m_photoDescr.Height;
				}
				text += "\n\n[this photo is not related to any trackpoint]";
			}
			else
			{
				text = "no image or waypoint";
				this.Text = "Photo Preview";
			}

			infoLabel.Text = text;

			drawLinkLabel.Enabled = (m_photoDescr != null);
		}
	}
}
