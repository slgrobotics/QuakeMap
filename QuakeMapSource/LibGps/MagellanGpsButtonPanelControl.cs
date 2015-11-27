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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using LibSys;

namespace LibGps
{
	public class MagellanGpsButtonPanelControl : LibGps.GpsButtonPanelControl
	{
		public System.Windows.Forms.GroupBox groupBox5;
		public System.Windows.Forms.Button loadTracksButton;
		public System.Windows.Forms.Button loadWaypointsButton;
		public System.Windows.Forms.GroupBox groupBox4;
		public System.Windows.Forms.Button downloadWaypointsButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox zoomCheckBox;
		public System.Windows.Forms.Button loadRoutesButton;
		public System.Windows.Forms.Button downloadRoutesButton;
		private System.Windows.Forms.NumericUpDown startRouteNumericUpDown;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox nameConversionComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox wptNamingCheckBox;
		private System.ComponentModel.IContainer components = null;

		public MagellanGpsButtonPanelControl()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			wptNamingCheckBox.Checked = Project.gpsWptDescrAsName;
			zoomCheckBox.Checked = Project.gpsZoomIntoTracks;
			startRouteNumericUpDown.Value = Project.startRouteNumber;
			nameConversionComboBox.SelectedIndex = Project.gpsMagellanRouteNameConversionMethod;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.loadRoutesButton = new System.Windows.Forms.Button();
			this.zoomCheckBox = new System.Windows.Forms.CheckBox();
			this.loadTracksButton = new System.Windows.Forms.Button();
			this.loadWaypointsButton = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.wptNamingCheckBox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.nameConversionComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.startRouteNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.downloadRoutesButton = new System.Windows.Forms.Button();
			this.downloadWaypointsButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.startRouteNumericUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox5
			// 
			this.groupBox5.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.groupBox5.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.loadRoutesButton,
																					this.zoomCheckBox,
																					this.loadTracksButton,
																					this.loadWaypointsButton});
			this.groupBox5.Location = new System.Drawing.Point(295, 40);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(260, 180);
			this.groupBox5.TabIndex = 14;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Upload from GPS";
			// 
			// loadRoutesButton
			// 
			this.loadRoutesButton.Location = new System.Drawing.Point(130, 65);
			this.loadRoutesButton.Name = "loadRoutesButton";
			this.loadRoutesButton.Size = new System.Drawing.Size(112, 23);
			this.loadRoutesButton.TabIndex = 10;
			this.loadRoutesButton.Text = "Get Routes";
			this.loadRoutesButton.Click += new System.EventHandler(this.loadRoutesButton_Click);
			// 
			// zoomCheckBox
			// 
			this.zoomCheckBox.Location = new System.Drawing.Point(45, 136);
			this.zoomCheckBox.Name = "zoomCheckBox";
			this.zoomCheckBox.Size = new System.Drawing.Size(208, 24);
			this.zoomCheckBox.TabIndex = 9;
			this.zoomCheckBox.Text = "zoom in after upload";
			this.zoomCheckBox.CheckedChanged += new System.EventHandler(this.zoomCheckBox_CheckedChanged);
			// 
			// loadTracksButton
			// 
			this.loadTracksButton.Location = new System.Drawing.Point(130, 30);
			this.loadTracksButton.Name = "loadTracksButton";
			this.loadTracksButton.Size = new System.Drawing.Size(112, 23);
			this.loadTracksButton.TabIndex = 7;
			this.loadTracksButton.Text = "Get Track Log";
			this.loadTracksButton.Click += new System.EventHandler(this.loadTracksButton_Click);
			// 
			// loadWaypointsButton
			// 
			this.loadWaypointsButton.Location = new System.Drawing.Point(130, 100);
			this.loadWaypointsButton.Name = "loadWaypointsButton";
			this.loadWaypointsButton.Size = new System.Drawing.Size(112, 23);
			this.loadWaypointsButton.TabIndex = 8;
			this.loadWaypointsButton.Text = "Get Waypoints";
			this.loadWaypointsButton.Click += new System.EventHandler(this.loadWaypointsButton_Click);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.wptNamingCheckBox,
																					this.label3,
																					this.nameConversionComboBox,
																					this.label2,
																					this.startRouteNumericUpDown,
																					this.downloadRoutesButton,
																					this.downloadWaypointsButton});
			this.groupBox4.Location = new System.Drawing.Point(16, 40);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(260, 180);
			this.groupBox4.TabIndex = 13;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Download to GPS";
			// 
			// wptNamingCheckBox
			// 
			this.wptNamingCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.wptNamingCheckBox.Location = new System.Drawing.Point(10, 132);
			this.wptNamingCheckBox.Name = "wptNamingCheckBox";
			this.wptNamingCheckBox.Size = new System.Drawing.Size(114, 32);
			this.wptNamingCheckBox.TabIndex = 16;
			this.wptNamingCheckBox.Text = "description as name";
			this.wptNamingCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.wptNamingCheckBox.CheckedChanged += new System.EventHandler(this.wptNamingCheckBox_CheckedChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(10, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(119, 26);
			this.label3.TabIndex = 15;
			this.label3.Text = "route name conversion";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// nameConversionComboBox
			// 
			this.nameConversionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.nameConversionComboBox.Items.AddRange(new object[] {
																		"none",
																		"A<name>",
																		"T1P<name>"});
			this.nameConversionComboBox.Location = new System.Drawing.Point(140, 64);
			this.nameConversionComboBox.Name = "nameConversionComboBox";
			this.nameConversionComboBox.Size = new System.Drawing.Size(112, 21);
			this.nameConversionComboBox.TabIndex = 14;
			this.nameConversionComboBox.SelectedIndexChanged += new System.EventHandler(this.nameConversionComboBox_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(15, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(115, 28);
			this.label2.TabIndex = 13;
			this.label2.Text = "starting route number in GPS";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// startRouteNumericUpDown
			// 
			this.startRouteNumericUpDown.Location = new System.Drawing.Point(140, 32);
			this.startRouteNumericUpDown.Maximum = new System.Decimal(new int[] {
																					20,
																					0,
																					0,
																					0});
			this.startRouteNumericUpDown.Minimum = new System.Decimal(new int[] {
																					1,
																					0,
																					0,
																					0});
			this.startRouteNumericUpDown.Name = "startRouteNumericUpDown";
			this.startRouteNumericUpDown.Size = new System.Drawing.Size(48, 20);
			this.startRouteNumericUpDown.TabIndex = 12;
			this.startRouteNumericUpDown.Value = new System.Decimal(new int[] {
																				  1,
																				  0,
																				  0,
																				  0});
			// 
			// downloadRoutesButton
			// 
			this.downloadRoutesButton.Location = new System.Drawing.Point(140, 104);
			this.downloadRoutesButton.Name = "downloadRoutesButton";
			this.downloadRoutesButton.Size = new System.Drawing.Size(112, 23);
			this.downloadRoutesButton.TabIndex = 11;
			this.downloadRoutesButton.Text = "Put Routes";
			this.downloadRoutesButton.Click += new System.EventHandler(this.downloadRoutesButton_Click);
			// 
			// downloadWaypointsButton
			// 
			this.downloadWaypointsButton.Location = new System.Drawing.Point(140, 136);
			this.downloadWaypointsButton.Name = "downloadWaypointsButton";
			this.downloadWaypointsButton.Size = new System.Drawing.Size(112, 23);
			this.downloadWaypointsButton.TabIndex = 9;
			this.downloadWaypointsButton.Text = "Put Waypoints";
			this.downloadWaypointsButton.Click += new System.EventHandler(this.downloadWaypointsButton_Click);
			// 
			// label1
			// 
			this.label1.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.label1.Font = new System.Drawing.Font("Impact", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(450, 10);
			this.label1.Name = "label1";
			this.label1.TabIndex = 16;
			this.label1.Text = "Magellan";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// MagellanGpsButtonPanelControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label1,
																		  this.groupBox5,
																		  this.groupBox4});
			this.Name = "MagellanGpsButtonPanelControl";
			this.Size = new System.Drawing.Size(570, 260);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.MagellanGpsButtonPanelControl_Layout);
			this.groupBox5.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.startRouteNumericUpDown)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		
		public override void allButtonsDisable()
		{
			loadTracksButton.Enabled = false;
			loadWaypointsButton.Enabled = false;
			loadRoutesButton.Enabled = false;
			downloadWaypointsButton.Enabled = false;
			downloadRoutesButton.Enabled = false;
		}

		public override void allButtonsEnable()
		{
			loadTracksButton.Enabled = true;
			loadWaypointsButton.Enabled = true;
			loadRoutesButton.Enabled = true;
			downloadWaypointsButton.Enabled = true;
			downloadRoutesButton.Enabled = true;
		}

		private void loadTracksButton_Click(object sender, System.EventArgs e)
		{
			m_responder.buttonPressed("uploadTracks");
		}

		private void loadWaypointsButton_Click(object sender, System.EventArgs e)
		{
			m_responder.buttonPressed("uploadWaypoints");
		}

		private void loadRoutesButton_Click(object sender, System.EventArgs e)
		{
			m_responder.buttonPressed("uploadRoutes");
		}

		private void downloadWaypointsButton_Click(object sender, System.EventArgs e)
		{
			m_responder.buttonPressed("downloadWaypoints");
		}

		private void downloadRoutesButton_Click(object sender, System.EventArgs e)
		{
			Project.startRouteNumber = (int)startRouteNumericUpDown.Value;
			m_responder.buttonPressed("downloadRoutes");
		}

		private void zoomCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsZoomIntoTracks = zoomCheckBox.Checked;
		}

		private void MagellanGpsButtonPanelControl_Layout(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			zoomCheckBox.Checked = Project.gpsZoomIntoTracks;
			startRouteNumericUpDown.Value = Project.startRouteNumber;
		}

		private void nameConversionComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.gpsMagellanRouteNameConversionMethod = nameConversionComboBox.SelectedIndex;
		}

		private void wptNamingCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsWptDescrAsName = wptNamingCheckBox.Checked;
		}
	}
}

