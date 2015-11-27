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
using System.Threading;
using System.IO;

using LibGui;
using LibSys;
using LibGeo;
using WindowsTiler.DlgControls;

using LibNet.TerraServer;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPreloadTiles.
	/// </summary>
	public class DlgPreloadTiles : System.Windows.Forms.Form
	{
		private bool m_forPda;
		private static int m_scaleShift = 0;	// 0=use m_scale1  or 4=use m_scale2
		private const double MAX_ALLOWED_CAMERA_HEIGHT_MILES = 4.0d;
		private static bool scaleUpDefaultChecked = true;
		private System.Windows.Forms.Label warningLabel;
		private PdaExportOptions pdaExportOptions = new PdaExportOptions(false);

		private LibSys.DirectionPadControl directionPadControl;

		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Button loadButton;
		private System.Windows.Forms.ComboBox scaleComboBox;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel switchAltLinkLabel;
		private System.Windows.Forms.Label switchAltLabel;
		private System.Windows.Forms.LinkLabel progressLinkLabel;
		private System.Windows.Forms.Button previewButton;
		private System.Windows.Forms.Label helpLabel;
		private System.Windows.Forms.Panel directionPadControlPanel;
		private System.Windows.Forms.CheckBox doProgressCheckBox;
		private System.Windows.Forms.Button exportButton;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.LinkLabel helpExportLinkLabel;
		private System.Windows.Forms.CheckBox scaleUpCheckBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage preloadTabPage;
		private System.Windows.Forms.TabPage optionsTabPage;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.GroupBox exportOptionsGroupBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgPreloadTiles(bool forPda)
		{
			m_forPda = forPda;

			InitializeComponent();

			this.warningLabel = new System.Windows.Forms.Label();
			this.warningLabel.Location = new System.Drawing.Point(10, 100);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Size = new System.Drawing.Size(280, 44);
			this.warningLabel.Text = "";
			this.preloadTabPage.Controls.Add(warningLabel);

			// making directionPadControl control as dumb VS Designer removes it:
			this.directionPadControl = new LibSys.DirectionPadControl();
			this.directionPadControlPanel.Controls.Add(this.directionPadControl);
			this.directionPadControl.Name = "directionPadControl";
			this.directionPadControl.clickHandler += new LibSys.ClickEventHandler(this.padClickEventHandler);
			directionPadControlPanel.Enabled = false;
			scaleUpCheckBox.Checked = scaleUpDefaultChecked;

			setLabels();

			pdaExportOptions.Dock = DockStyle.Fill;
			exportOptionsGroupBox.Controls.Add(pdaExportOptions);

			doProgressCheckBox.Checked = Project.preloadDoProgress;

			Project.setDlgIcon(this);
		}

		private void setLabels()
		{
			if(m_forPda)
			{
				this.Text = "Make Supersized Frame for PDA";
				messageLabel.Text = "To record a large size frame click \"Download\" or \"Preview\".\nYou can use arrowpad to record more frames.\nWhen finished, click \"Close\".";
				helpLabel.Text = "You can use tiles already on the hard drive or download the missing ones.\n";
				exportButton.Visible = false;
				browseButton.Visible = false;
			}
			else
			{
				helpExportLinkLabel.Visible = false;
				messageLabel.Text = "This function loads all tiles within the visible screen area to the hard drive, to allow QuakeMap work in disconnected mode.";
				helpLabel.Text = "You can preview tiles already on the hard drive or download the missing ones.\nTo download a full set for the visible area, do it at 4 and 50 miles with \"and up\" box checked.";
			}

			warningLabel.Visible = true;
			loadButton.Enabled = true;
			exportButton.Enabled = true;
			browseButton.Enabled = true;
			progressLinkLabel.Visible = true;
			doProgressCheckBox.Visible = true;

			this.scaleComboBox.Items.Clear();
			string warning = "";
			if(PictureManager.This.CameraManager.Elev > 87000.0d)
			{
				switchAltLinkLabel.Text = "50 miles";
				loadButton.Enabled = false;
				previewButton.Enabled = false;
				exportButton.Enabled = false;
				scaleComboBox.Enabled = false;
				scaleUpCheckBox.Enabled = false;
				warning = "Error: this function works only when Camera altitude is less than 87 km (55 miles). Click \"50 miles\" link below.";
			}
			else if(PictureManager.This.CameraManager.Elev > (MAX_ALLOWED_CAMERA_HEIGHT_MILES + 0.1d) * Distance.METERS_PER_MILE)
			{
				previewButton.Enabled = true;
				exportButton.Enabled = true;
				scaleComboBox.Enabled = true;
				scaleUpCheckBox.Enabled = true;
				switchAltLinkLabel.Text = "4 miles";
				this.scaleComboBox.Items.AddRange(new object[] {
																   "4",
																   "8",
																   "16",
																   "32",
																   "64" });
				warning = "Note: scales 1 and 2 are available when Camera altitude is less than " + MAX_ALLOWED_CAMERA_HEIGHT_MILES + " miles. Scale 4 works only at 25 miles and closer.";
				m_scaleShift = 2;
				scaleComboBox.SelectedIndex = Project.preloadScale2;
			}
			else
			{
				previewButton.Enabled = true;
				exportButton.Enabled = true;
				scaleComboBox.Enabled = true;
				scaleUpCheckBox.Enabled = true;
				switchAltLinkLabel.Text = "50 miles";
				this.scaleComboBox.Items.AddRange(new object[] {
																   "1",
																   "2",
																   "4",
																   "8" });
				warning = "Note: scales 16,32,64 are available when Camera altitude is " + MAX_ALLOWED_CAMERA_HEIGHT_MILES + " to 50 miles";
				m_scaleShift = 0;
				scaleComboBox.SelectedIndex = Project.preloadScale1;
			}

			warningLabel.Text = warning;
		}

		private void DlgPreloadTiles_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			TileSetTerra.This.RetilingSpecial = false;
			TerraserverCache.Clear();	// make sure empty tiles are not stuck after preview
			PictureManager.This.CameraManager.ProcessCameraMove();
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
            this.loadButton = new System.Windows.Forms.Button();
            this.scaleComboBox = new System.Windows.Forms.ComboBox();
            this.messageLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.switchAltLinkLabel = new System.Windows.Forms.LinkLabel();
            this.switchAltLabel = new System.Windows.Forms.Label();
            this.progressLinkLabel = new System.Windows.Forms.LinkLabel();
            this.previewButton = new System.Windows.Forms.Button();
            this.helpLabel = new System.Windows.Forms.Label();
            this.directionPadControlPanel = new System.Windows.Forms.Panel();
            this.doProgressCheckBox = new System.Windows.Forms.CheckBox();
            this.exportButton = new System.Windows.Forms.Button();
            this.browseButton = new System.Windows.Forms.Button();
            this.helpExportLinkLabel = new System.Windows.Forms.LinkLabel();
            this.scaleUpCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.preloadTabPage = new System.Windows.Forms.TabPage();
            this.optionsTabPage = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.exportOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.tabControl1.SuspendLayout();
            this.preloadTabPage.SuspendLayout();
            this.optionsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(330, 260);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(72, 20);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(330, 90);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(72, 20);
            this.loadButton.TabIndex = 5;
            this.loadButton.Text = "Download";
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // scaleComboBox
            // 
            this.scaleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scaleComboBox.Location = new System.Drawing.Point(55, 70);
            this.scaleComboBox.Name = "scaleComboBox";
            this.scaleComboBox.Size = new System.Drawing.Size(56, 21);
            this.scaleComboBox.TabIndex = 6;
            this.scaleComboBox.SelectedIndexChanged += new System.EventHandler(this.scaleComboBox_SelectedIndexChanged);
            // 
            // messageLabel
            // 
            this.messageLabel.Location = new System.Drawing.Point(10, 140);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(385, 92);
            this.messageLabel.TabIndex = 7;
            this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 23);
            this.label1.TabIndex = 8;
            this.label1.Text = "Scale:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(120, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 23);
            this.label2.TabIndex = 9;
            this.label2.Text = "meters/pixel";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // switchAltLinkLabel
            // 
            this.switchAltLinkLabel.Location = new System.Drawing.Point(240, 260);
            this.switchAltLinkLabel.Name = "switchAltLinkLabel";
            this.switchAltLinkLabel.Size = new System.Drawing.Size(60, 22);
            this.switchAltLinkLabel.TabIndex = 12;
            this.switchAltLinkLabel.TabStop = true;
            this.switchAltLinkLabel.Text = "50 miles";
            this.switchAltLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.switchAltLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.switchAltLinkLabel_LinkClicked);
            // 
            // switchAltLabel
            // 
            this.switchAltLabel.Location = new System.Drawing.Point(95, 260);
            this.switchAltLabel.Name = "switchAltLabel";
            this.switchAltLabel.Size = new System.Drawing.Size(136, 23);
            this.switchAltLabel.TabIndex = 13;
            this.switchAltLabel.Text = "Move camera to:";
            this.switchAltLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // progressLinkLabel
            // 
            this.progressLinkLabel.Location = new System.Drawing.Point(25, 260);
            this.progressLinkLabel.Name = "progressLinkLabel";
            this.progressLinkLabel.Size = new System.Drawing.Size(80, 22);
            this.progressLinkLabel.TabIndex = 14;
            this.progressLinkLabel.TabStop = true;
            this.progressLinkLabel.Text = "progress...";
            this.progressLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.progressLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.progressLinkLabel_LinkClicked);
            // 
            // previewButton
            // 
            this.previewButton.Location = new System.Drawing.Point(330, 70);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(72, 20);
            this.previewButton.TabIndex = 15;
            this.previewButton.Text = "Preview";
            this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
            // 
            // helpLabel
            // 
            this.helpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpLabel.Location = new System.Drawing.Point(10, 5);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(312, 56);
            this.helpLabel.TabIndex = 16;
            this.helpLabel.Text = "You can preview tiles already on the hard drive...";
            // 
            // directionPadControlPanel
            // 
            this.directionPadControlPanel.Location = new System.Drawing.Point(340, 5);
            this.directionPadControlPanel.Name = "directionPadControlPanel";
            this.directionPadControlPanel.Size = new System.Drawing.Size(53, 53);
            this.directionPadControlPanel.TabIndex = 17;
            // 
            // doProgressCheckBox
            // 
            this.doProgressCheckBox.Location = new System.Drawing.Point(10, 260);
            this.doProgressCheckBox.Name = "doProgressCheckBox";
            this.doProgressCheckBox.Size = new System.Drawing.Size(16, 24);
            this.doProgressCheckBox.TabIndex = 18;
            this.doProgressCheckBox.CheckedChanged += new System.EventHandler(this.doProgressCheckBox_CheckedChanged);
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(330, 110);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(72, 20);
            this.exportButton.TabIndex = 19;
            this.exportButton.Text = "Export";
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            this.exportButton.MouseHover += new System.EventHandler(this.exportButton_MouseHover);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(300, 110);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(24, 20);
            this.browseButton.TabIndex = 20;
            this.browseButton.Text = "...";
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // helpExportLinkLabel
            // 
            this.helpExportLinkLabel.Location = new System.Drawing.Point(280, 70);
            this.helpExportLinkLabel.Name = "helpExportLinkLabel";
            this.helpExportLinkLabel.Size = new System.Drawing.Size(48, 16);
            this.helpExportLinkLabel.TabIndex = 21;
            this.helpExportLinkLabel.TabStop = true;
            this.helpExportLinkLabel.Text = "Help";
            this.helpExportLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.helpExportLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpExportLinkLabel_LinkClicked);
            // 
            // scaleUpCheckBox
            // 
            this.scaleUpCheckBox.Location = new System.Drawing.Point(200, 70);
            this.scaleUpCheckBox.Name = "scaleUpCheckBox";
            this.scaleUpCheckBox.Size = new System.Drawing.Size(59, 24);
            this.scaleUpCheckBox.TabIndex = 22;
            this.scaleUpCheckBox.Text = "and up";
            this.scaleUpCheckBox.CheckedChanged += new System.EventHandler(this.scaleUpCheckBox_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(75, 240);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(328, 16);
            this.label3.TabIndex = 23;
            this.label3.Text = "downloading THOUSANDS of tiles takes _REALLY_ long time.";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 240);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 16);
            this.label4.TabIndex = 24;
            this.label4.Text = "Warning:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.preloadTabPage);
            this.tabControl1.Controls.Add(this.optionsTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(414, 311);
            this.tabControl1.TabIndex = 25;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // preloadTabPage
            // 
            this.preloadTabPage.Controls.Add(this.scaleComboBox);
            this.preloadTabPage.Controls.Add(this.switchAltLabel);
            this.preloadTabPage.Controls.Add(this.browseButton);
            this.preloadTabPage.Controls.Add(this.switchAltLinkLabel);
            this.preloadTabPage.Controls.Add(this.label3);
            this.preloadTabPage.Controls.Add(this.previewButton);
            this.preloadTabPage.Controls.Add(this.messageLabel);
            this.preloadTabPage.Controls.Add(this.label4);
            this.preloadTabPage.Controls.Add(this.label1);
            this.preloadTabPage.Controls.Add(this.helpExportLinkLabel);
            this.preloadTabPage.Controls.Add(this.label2);
            this.preloadTabPage.Controls.Add(this.directionPadControlPanel);
            this.preloadTabPage.Controls.Add(this.loadButton);
            this.preloadTabPage.Controls.Add(this.exportButton);
            this.preloadTabPage.Controls.Add(this.helpLabel);
            this.preloadTabPage.Controls.Add(this.scaleUpCheckBox);
            this.preloadTabPage.Controls.Add(this.closeButton);
            this.preloadTabPage.Controls.Add(this.progressLinkLabel);
            this.preloadTabPage.Controls.Add(this.doProgressCheckBox);
            this.preloadTabPage.Location = new System.Drawing.Point(4, 22);
            this.preloadTabPage.Name = "preloadTabPage";
            this.preloadTabPage.Size = new System.Drawing.Size(406, 285);
            this.preloadTabPage.TabIndex = 0;
            this.preloadTabPage.Text = "Preload";
            // 
            // optionsTabPage
            // 
            this.optionsTabPage.Controls.Add(this.button1);
            this.optionsTabPage.Controls.Add(this.exportOptionsGroupBox);
            this.optionsTabPage.Location = new System.Drawing.Point(4, 22);
            this.optionsTabPage.Name = "optionsTabPage";
            this.optionsTabPage.Size = new System.Drawing.Size(406, 285);
            this.optionsTabPage.TabIndex = 1;
            this.optionsTabPage.Text = "Options";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(330, 260);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 20);
            this.button1.TabIndex = 6;
            this.button1.Text = "Close";
            // 
            // exportOptionsGroupBox
            // 
            this.exportOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.exportOptionsGroupBox.Location = new System.Drawing.Point(10, 50);
            this.exportOptionsGroupBox.Name = "exportOptionsGroupBox";
            this.exportOptionsGroupBox.Size = new System.Drawing.Size(385, 145);
            this.exportOptionsGroupBox.TabIndex = 5;
            this.exportOptionsGroupBox.TabStop = false;
            this.exportOptionsGroupBox.Text = "Export Tiles";
            // 
            // DlgPreloadTiles
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(414, 311);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DlgPreloadTiles";
            this.Text = "Preload Aerial/Topo Tiles";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgPreloadTiles_Closing);
            this.tabControl1.ResumeLayout(false);
            this.preloadTabPage.ResumeLayout(false);
            this.optionsTabPage.ResumeLayout(false);
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

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void previewButton_Click(object sender, System.EventArgs e)
		{
			ThreadPool2.EmptyQueue();
			action(true);
		}

		private void loadButton_Click(object sender, System.EventArgs e)
		{
			action(false);
		}

		private bool m_lastAction = true;

		private void action(bool previewOnly)
		{
			// a little hack here to disallow loading 4 meters tiles at 50 miles (like 15,000 tiles at once).
			if(PictureManager.This.CameraManager.Elev > (25.1d) * Distance.METERS_PER_MILE && m_scaleShift != 0 && Project.preloadScale2 == 0)
			{
				messageLabel.Text = "Error: too many tiles to load, move closer or try another scale";
				return;
			}

			messageLabel.Text = "IP: please wait, computing layout...";
			this.Refresh();
			Cursor.Current = Cursors.WaitCursor;
			int total = 0;
			int toLoad = 0;

			directionPadControlPanel.Enabled = true;

			bool saveOffline = Project.serverAvailable;
			if(previewOnly)
			{
				Project.serverAvailable = false;
			}
			//else
			//{
				// in case we had tiles in cache marked Empty, we need to make them become reloadable
				TileSetTerra.This.ReTileSpecialClear();
			//}

			GeoCoord topLeft;
			GeoCoord bottomRight;
			int scale = 10 + (m_scaleShift == 0 ? Project.preloadScale1 : (Project.preloadScale2 + m_scaleShift));
			if(TileSetTerra.This.ReTileSpecial(scale, out total, out toLoad, out topLeft, out bottomRight))
			{
				int toLoadMb = toLoad * 6 / 1000;
				messageLabel.Text = "IP: total number of tiles: " + total + " to load : " + toLoad
									+ (toLoadMb > 0 ? "   (approx" + toLoadMb + "Mb)" : "");
				if(scaleUpCheckBox.Checked)
				{
					if(toLoad > 10)
					{
						messageLabel.Text += "\n       (and up to 30% more from higher scale tiles)";
					}
					else
					{
						messageLabel.Text += "\n       (and may be more from higher scale tiles)";
					}
				}
				if(!previewOnly)
				{
					if(scaleUpCheckBox.Checked)
					{
						TileSetTerraLayout.downloadAtLevel(topLeft, bottomRight, scale+1, true);
					}
					messageLabel.Text += "\nIP: loading...\n\nYou can close this dialog or continue to define new areas -\n        downloads will proceed in the background.\nTo stop downloading, click \"Preview\".";
				}
				PictureManager.This.Refresh();
				if(!previewOnly && Project.preloadDoProgress)
				{
					ProgressMonitorForm.BringFormUp(false);
				}
			}
			else
			{
				messageLabel.Text = "Error: cannot compute layout, try another scale";
			}
			Project.serverAvailable = saveOffline;
			m_lastAction = previewOnly;
		}

		private void scaleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(m_scaleShift == 0)
			{
				Project.preloadScale1 = scaleComboBox.SelectedIndex;
			}
			else
			{
				Project.preloadScale2 = scaleComboBox.SelectedIndex;
			}
		}

		private void switchAltLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			directionPadControlPanel.Enabled = false;
			TileSetTerra.This.RetilingSpecial = false;

			if(switchAltLinkLabel.Text.StartsWith("50"))
			{
				PictureManager.This.CameraManager.Elev = 50.0d * Distance.METERS_PER_MILE;
			}
			else
			{
				PictureManager.This.CameraManager.Elev = 4.0d * Distance.METERS_PER_MILE;
			}
			this.setLabels();
		}

		private void progressLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ProgressMonitorForm.BringFormUp(false);
		}

		private void padClickEventHandler(object sender, DirectionPadEventArgs e)
		{
			directionPadControlPanel.Enabled = false;
			switch(e.btn)
			{
				case 1:
					PictureManager.This.CameraManager.shiftUpLeftSpecial();
					break;
				case 2:
					PictureManager.This.CameraManager.shiftUpSpecial();
					break;
				case 3:
					PictureManager.This.CameraManager.shiftUpRightSpecial();
					break;
				case 4:
					PictureManager.This.CameraManager.shiftLeftSpecial();
					break;
				case 6:
					PictureManager.This.CameraManager.shiftRightSpecial();
					break;
				case 7:
					PictureManager.This.CameraManager.shiftDownLeftSpecial();
					break;
				case 8:
					PictureManager.This.CameraManager.shiftDownSpecial();
					break;
				case 9:
					PictureManager.This.CameraManager.shiftDownRightSpecial();
					break;
			}
			action(m_lastAction);
			directionPadControlPanel.Enabled = true;
		}

		private void doProgressCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.preloadDoProgress = doProgressCheckBox.Checked;
		}

		private void setExportDestFolder()
		{
			IntPtr owner = this.Handle;
			clsFolderBrowser folderBrowser = new clsFolderBrowser(owner, "Select Export Folder");

			folderBrowser.Title = "Locate/Create Tiles Export Folder";

			string tmp = folderBrowser.BrowseForFolder(Project.exportDestFolder);
			if(!"\\".Equals(tmp))		// cancel button causes "\" returned
			{
				Project.exportDestFolder = tmp;
			}
		}

		private void exportButton_Click(object sender, System.EventArgs e)
		{
			exportButton.Enabled = false;
			Cursor.Current = Cursors.WaitCursor;

			if(Project.exportDestFolder.Length == 0)
			{
				setExportDestFolder();
			}

			if(Project.exportDestFolder.Length > 0 && Directory.Exists(Project.exportDestFolder))
			{
				Hashtable listOfTiles = new Hashtable();

				int scaleIndex = 10 + (m_scaleShift == 0 ? Project.preloadScale1 : (Project.preloadScale2 + m_scaleShift));

				int levels = TileSetTerraLayout.listTilesAtLevels(listOfTiles,
									PictureManager.This.CameraManager.CoverageTopLeft,
									PictureManager.This.CameraManager.CoverageBottomRight,
									scaleIndex, scaleUpCheckBox.Checked);

				int countLoaded = 0;
				int countTotal = 0;

				// save selected tiles:
				foreach(string tileName in listOfTiles.Keys)
				{
					string fileNameJpg = tileName + ".jpg";
					string srcPath = Project.GetTerraserverPath(fileNameJpg);

					if(File.Exists(srcPath))
					{
						try 
						{
							PdaHelper.exportSingleFile(tileName, srcPath, true);
							countLoaded++;
						}
						catch (Exception ee)
						{
#if DEBUG
							// some files may be broken/missing, not a big deal
							LibSys.StatusBar.Error("PreloadTiles:export " + ee.Message);
#endif
						}
					}
					countTotal++;
				}
				this.messageLabel.Text = "OK: " + countLoaded + " downloaded tiles out of "
							+ countTotal + " covering whole view on " + levels + " levels\n  exported to folder: " + Project.exportDestFolder;
			}
			else
			{
				this.messageLabel.Text = "Error: folder '" + Project.exportDestFolder + "' not valid.";
				setExportDestFolder();
			}
			exportButton.Enabled = true;
			Cursor.Current = Cursors.Default;
		}

		private void exportButton_MouseHover(object sender, System.EventArgs e)
		{
			Project.ShowPopup(exportButton, "copy all tiles on the visible map to a folder\nfor use on PDA.\n\nCheck Options tab to set format.", Point.Empty);
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			setExportDestFolder();
		}

		private void helpExportLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser("http://www.mapadvisor.com/help/qmexportpdbsuperframe.html");
		}

		private void scaleUpCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			scaleUpDefaultChecked = scaleUpCheckBox.Checked;	// for the next time the dialig opens
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			pdaExportOptions.setRadioButtons();
		}
	}
}
