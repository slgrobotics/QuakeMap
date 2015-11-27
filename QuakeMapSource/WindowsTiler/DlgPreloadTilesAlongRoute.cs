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

//using LibNet.TerraServer;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPreloadTilesAlongRoute.
	/// </summary>
	public class DlgPreloadTilesAlongRoute : System.Windows.Forms.Form
	{
		private Track m_trk;
		private static bool scaleUpDefaultChecked = true;
		private static int m_preloadScale = 1;
		private double[] m_spreads = new double[] {0.25d, 0.5d, 1.0d, 2.0d, 4.0d, 8.0d, 16.0d};	// miles; keep in sync with spreadComboBox collection
		private double m_spread;	// miles
		private static int m_spreadIndex = 0;
//		private static Thread m_greetingThread = null;
		private System.Windows.Forms.Timer refreshPictureTimer = new System.Windows.Forms.Timer();
		private bool m_loaded = false;
		private PdaExportOptions pdaExportOptions = new PdaExportOptions(false);
		private Hashtable m_listOfTiles = new Hashtable();

		private static bool m_doAerial = false;
		private static bool m_doTopo = false;
		private static bool m_doColor = false;

		private System.Windows.Forms.Button loadButton;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label hintLabel;
		private System.Windows.Forms.Label estimateLabel;
		private System.Windows.Forms.ComboBox scaleComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox scaleUpCheckBox;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.ComboBox spreadComboBox;
		private System.Windows.Forms.GroupBox selectParametersGroupBox;
		private System.Windows.Forms.CheckBox colorCheckBox;
		private System.Windows.Forms.CheckBox topoCheckBox;
		private System.Windows.Forms.CheckBox aerialCheckBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.LinkLabel progressLinkLabel;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Button exportButton;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage preloadTabPage;
		private System.Windows.Forms.TabPage pdaOptionsTabPage;
		private System.Windows.Forms.GroupBox exportOptionsGroupBox;
		private System.Windows.Forms.ProgressBar progressBar1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgPreloadTilesAlongRoute(Track trk)
		{
			m_trk = trk;

			InitializeComponent();

			progressBar1.Visible = false;

			hintLabel.Text = "Terraserver tiles will be loaded to:\r\n" + Project.GetTerraserverMapsBasePath()
					+ "\r\n\r\nPlease note that loading too many tiles wastes valuable resources.";

			selectParametersGroupBox.Enabled = false;

			this.scaleComboBox.Items.Clear();
			this.scaleComboBox.Items.AddRange(new object[] {
																"1",
																"2",
																"4",
																"8",
																"16",
																"32",
																"64"
														   });
			scaleComboBox.SelectedIndex = m_preloadScale;

			this.spreadComboBox.Items.AddRange(new object[] {
																"0.25 mile",
																"0.5 mile",
																"1 mile",
																"2 miles",
																"4 miles",
																"8 miles",
																"16 miles"
															});

			spreadComboBox.SelectedIndex = m_spreadIndex;
			m_spread = m_spreads[spreadComboBox.SelectedIndex];

			scaleUpCheckBox.Checked = scaleUpDefaultChecked;

			pdaExportOptions.Dock = DockStyle.Fill;
			exportOptionsGroupBox.Controls.Add(pdaExportOptions);
			pdaExportOptions.OptionsChanged += new EventHandler(pdaOptionsChanged);

			Project.setDlgIcon(this);
		}

		private void DlgPreloadTilesAlongRoute_Load(object sender, System.EventArgs e)
		{
			initiateRefresh();

			this.spreadComboBox.SelectedIndexChanged += new System.EventHandler(this.spreadComboBox_SelectedIndexChanged);

			if(!m_doAerial && !m_doTopo && !m_doColor)
			{
				// first time - set according to the current theme:
				switch(Project.drawTerraserverMode)
				{
					default:
					case "relief":
					case "aerial":
						m_doAerial = true;
						break;
					case "color aerial":
						m_doColor = true;
						break;
					case "topo":
						m_doTopo = true;
						break;
				}
			}

			aerialCheckBox.Checked = m_doAerial;
			topoCheckBox.Checked = m_doTopo;
			colorCheckBox.Checked = m_doColor;

			m_loaded = true;
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
			this.loadButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.hintLabel = new System.Windows.Forms.Label();
			this.estimateLabel = new System.Windows.Forms.Label();
			this.selectParametersGroupBox = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.colorCheckBox = new System.Windows.Forms.CheckBox();
			this.topoCheckBox = new System.Windows.Forms.CheckBox();
			this.aerialCheckBox = new System.Windows.Forms.CheckBox();
			this.spreadComboBox = new System.Windows.Forms.ComboBox();
			this.label13 = new System.Windows.Forms.Label();
			this.scaleComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.scaleUpCheckBox = new System.Windows.Forms.CheckBox();
			this.progressLinkLabel = new System.Windows.Forms.LinkLabel();
			this.browseButton = new System.Windows.Forms.Button();
			this.exportButton = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.preloadTabPage = new System.Windows.Forms.TabPage();
			this.pdaOptionsTabPage = new System.Windows.Forms.TabPage();
			this.exportOptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.selectParametersGroupBox.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.preloadTabPage.SuspendLayout();
			this.pdaOptionsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// loadButton
			// 
			this.loadButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.loadButton.Location = new System.Drawing.Point(455, 230);
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(85, 20);
			this.loadButton.TabIndex = 7;
			this.loadButton.Text = "Download";
			this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.Location = new System.Drawing.Point(455, 320);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(85, 20);
			this.closeButton.TabIndex = 6;
			this.closeButton.Text = "Cancel";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// hintLabel
			// 
			this.hintLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.hintLabel.Location = new System.Drawing.Point(5, 5);
			this.hintLabel.Name = "hintLabel";
			this.hintLabel.Size = new System.Drawing.Size(530, 65);
			this.hintLabel.TabIndex = 8;
			this.hintLabel.Text = "hint";
			// 
			// estimateLabel
			// 
			this.estimateLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.estimateLabel.Location = new System.Drawing.Point(10, 260);
			this.estimateLabel.Name = "estimateLabel";
			this.estimateLabel.Size = new System.Drawing.Size(410, 85);
			this.estimateLabel.TabIndex = 9;
			this.estimateLabel.Click += new System.EventHandler(this.estimateLabel_Click);
			// 
			// selectParametersGroupBox
			// 
			this.selectParametersGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.selectParametersGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																								   this.label3,
																								   this.colorCheckBox,
																								   this.topoCheckBox,
																								   this.aerialCheckBox,
																								   this.spreadComboBox,
																								   this.label13,
																								   this.scaleComboBox,
																								   this.label1,
																								   this.label2,
																								   this.scaleUpCheckBox});
			this.selectParametersGroupBox.Location = new System.Drawing.Point(5, 75);
			this.selectParametersGroupBox.Name = "selectParametersGroupBox";
			this.selectParametersGroupBox.Size = new System.Drawing.Size(530, 110);
			this.selectParametersGroupBox.TabIndex = 10;
			this.selectParametersGroupBox.TabStop = false;
			this.selectParametersGroupBox.Text = "Select Parameters";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(10, 20);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 21);
			this.label3.TabIndex = 32;
			this.label3.Text = "Tiles:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// colorCheckBox
			// 
			this.colorCheckBox.Location = new System.Drawing.Point(255, 20);
			this.colorCheckBox.Name = "colorCheckBox";
			this.colorCheckBox.Size = new System.Drawing.Size(85, 21);
			this.colorCheckBox.TabIndex = 31;
			this.colorCheckBox.Text = "color";
			this.colorCheckBox.CheckedChanged += new System.EventHandler(this.anyCheckBox_CheckedChanged);
			// 
			// topoCheckBox
			// 
			this.topoCheckBox.Location = new System.Drawing.Point(165, 20);
			this.topoCheckBox.Name = "topoCheckBox";
			this.topoCheckBox.Size = new System.Drawing.Size(85, 21);
			this.topoCheckBox.TabIndex = 30;
			this.topoCheckBox.Text = "topo";
			this.topoCheckBox.CheckedChanged += new System.EventHandler(this.anyCheckBox_CheckedChanged);
			// 
			// aerialCheckBox
			// 
			this.aerialCheckBox.Location = new System.Drawing.Point(75, 20);
			this.aerialCheckBox.Name = "aerialCheckBox";
			this.aerialCheckBox.Size = new System.Drawing.Size(85, 21);
			this.aerialCheckBox.TabIndex = 29;
			this.aerialCheckBox.Text = "aerial";
			this.aerialCheckBox.CheckedChanged += new System.EventHandler(this.anyCheckBox_CheckedChanged);
			// 
			// spreadComboBox
			// 
			this.spreadComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.spreadComboBox.Location = new System.Drawing.Point(75, 50);
			this.spreadComboBox.Name = "spreadComboBox";
			this.spreadComboBox.Size = new System.Drawing.Size(100, 21);
			this.spreadComboBox.TabIndex = 28;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(10, 50);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(60, 21);
			this.label13.TabIndex = 27;
			this.label13.Text = "Spread:";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// scaleComboBox
			// 
			this.scaleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scaleComboBox.Location = new System.Drawing.Point(75, 80);
			this.scaleComboBox.Name = "scaleComboBox";
			this.scaleComboBox.Size = new System.Drawing.Size(100, 21);
			this.scaleComboBox.TabIndex = 23;
			this.scaleComboBox.SelectedIndexChanged += new System.EventHandler(this.scaleComboBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(10, 80);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 21);
			this.label1.TabIndex = 24;
			this.label1.Text = "Scale:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(175, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 21);
			this.label2.TabIndex = 25;
			this.label2.Text = "meters/pixel";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// scaleUpCheckBox
			// 
			this.scaleUpCheckBox.Location = new System.Drawing.Point(300, 80);
			this.scaleUpCheckBox.Name = "scaleUpCheckBox";
			this.scaleUpCheckBox.Size = new System.Drawing.Size(85, 21);
			this.scaleUpCheckBox.TabIndex = 26;
			this.scaleUpCheckBox.Text = "and up";
			this.scaleUpCheckBox.CheckedChanged += new System.EventHandler(this.scaleUpCheckBox_CheckedChanged);
			// 
			// progressLinkLabel
			// 
			this.progressLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.progressLinkLabel.Location = new System.Drawing.Point(235, 230);
			this.progressLinkLabel.Name = "progressLinkLabel";
			this.progressLinkLabel.Size = new System.Drawing.Size(215, 22);
			this.progressLinkLabel.TabIndex = 15;
			this.progressLinkLabel.TabStop = true;
			this.progressLinkLabel.Text = "download progress...";
			this.progressLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.progressLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.progressLinkLabel_LinkClicked);
			// 
			// browseButton
			// 
			this.browseButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.browseButton.Location = new System.Drawing.Point(425, 260);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 20);
			this.browseButton.TabIndex = 22;
			this.browseButton.Text = "...";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// exportButton
			// 
			this.exportButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.exportButton.Location = new System.Drawing.Point(455, 260);
			this.exportButton.Name = "exportButton";
			this.exportButton.Size = new System.Drawing.Size(85, 20);
			this.exportButton.TabIndex = 21;
			this.exportButton.Text = "Export";
			this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
			this.exportButton.MouseHover += new System.EventHandler(this.exportButton_MouseHover);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.preloadTabPage,
																					  this.pdaOptionsTabPage});
			this.tabControl1.Location = new System.Drawing.Point(0, 5);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(545, 215);
			this.tabControl1.TabIndex = 23;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// preloadTabPage
			// 
			this.preloadTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.hintLabel,
																						 this.selectParametersGroupBox});
			this.preloadTabPage.Location = new System.Drawing.Point(4, 22);
			this.preloadTabPage.Name = "preloadTabPage";
			this.preloadTabPage.Size = new System.Drawing.Size(537, 189);
			this.preloadTabPage.TabIndex = 0;
			this.preloadTabPage.Text = "Preload";
			// 
			// pdaOptionsTabPage
			// 
			this.pdaOptionsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.exportOptionsGroupBox});
			this.pdaOptionsTabPage.Location = new System.Drawing.Point(4, 22);
			this.pdaOptionsTabPage.Name = "pdaOptionsTabPage";
			this.pdaOptionsTabPage.Size = new System.Drawing.Size(537, 189);
			this.pdaOptionsTabPage.TabIndex = 1;
			this.pdaOptionsTabPage.Text = "PDA Options";
			// 
			// exportOptionsGroupBox
			// 
			this.exportOptionsGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.exportOptionsGroupBox.Location = new System.Drawing.Point(25, 25);
			this.exportOptionsGroupBox.Name = "exportOptionsGroupBox";
			this.exportOptionsGroupBox.Size = new System.Drawing.Size(485, 140);
			this.exportOptionsGroupBox.TabIndex = 6;
			this.exportOptionsGroupBox.TabStop = false;
			this.exportOptionsGroupBox.Text = "Export Tiles";
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(10, 235);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(200, 15);
			this.progressBar1.TabIndex = 24;
			// 
			// DlgPreloadTilesAlongRoute
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(549, 346);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.progressBar1,
																		  this.tabControl1,
																		  this.browseButton,
																		  this.exportButton,
																		  this.progressLinkLabel,
																		  this.estimateLabel,
																		  this.loadButton,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgPreloadTilesAlongRoute";
			this.Text = "Preload Tiles Along the Route";
			this.Load += new System.EventHandler(this.DlgPreloadTilesAlongRoute_Load);
			this.Move += new System.EventHandler(this.DlgPreloadTilesAlongRoute_Move);
			this.Closed += new System.EventHandler(this.DlgPreloadTilesAlongRoute_Closed);
			this.selectParametersGroupBox.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.preloadTabPage.ResumeLayout(false);
			this.pdaOptionsTabPage.ResumeLayout(false);
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

		private void estimateOrAct(bool doAct, bool doDraw)
		{
			if(!m_loaded || m_working)
			{
				return;
			}

			estimateLabel.Text = "estimating download... please wait ...";
			estimateLabel.Refresh();
			Application.DoEvents();

			Graphics graphics = PictureManager.This.Graphics;
			Cursor.Current = Cursors.WaitCursor;

			double spreadMeters = m_spread * Distance.METERS_PER_MILE;
			double spreadDegrees = spreadMeters / Distance.METERS_PER_DEGREE;

			int scaleIndex = 10 + m_preloadScale;

			if(!doAct)
			{
				m_listOfTiles.Clear();
			}

			for(int i=0; i < m_trk.Trackpoints.Count-1 ;i++)
			{
				Waypoint trkpt1 = (Waypoint)m_trk.Trackpoints.GetByIndex(i);
				Waypoint trkpt2 = (Waypoint)m_trk.Trackpoints.GetByIndex(i+1);

				Distance d = trkpt2.Location.distanceFrom(trkpt1.Location);
				int steps = (int)Math.Ceiling(d.Meters / spreadMeters * 2);
				double stepLng = (trkpt2.Location.Lng - trkpt1.Location.Lng) / steps;
				double stepLat = (trkpt2.Location.Lat - trkpt1.Location.Lat) / steps;

				GeoCoord center = new GeoCoord(trkpt1.Location.Lng, trkpt1.Location.Lat);

				for(int j=0; j < steps ;j++)
				{
					GeoCoord topLeft = new GeoCoord(center.Lng - spreadDegrees, center.Lat + spreadDegrees);
					GeoCoord bottomRight = new GeoCoord(center.Lng + spreadDegrees, center.Lat - spreadDegrees);

					if(doAct)
					{
						TileSetTerraLayout.downloadThemesAtLevel(topLeft, bottomRight, scaleIndex, scaleUpCheckBox.Checked, m_doAerial, m_doColor, m_doTopo);
					}
					else
					{
						TileSetTerraLayout.listTilesAtLevelsWithType(m_listOfTiles, topLeft, bottomRight,
							scaleIndex, scaleUpCheckBox.Checked, m_doAerial, m_doColor, m_doTopo);
					}

					if(doDraw)
					{
						CameraManager.This.PaintGeoRect(topLeft, bottomRight, graphics, Pens.Red, null);

						// Brush brush = new SolidBrush(Color.FromArgb(10, 255, 0, 0));
						// CameraManager.This.HighlightGeoRect(topLeft, bottomRight, graphics, brush);
					}

					center.Lng += stepLng;
					center.Lat += stepLat;

					Application.DoEvents();
				}
			}

			if(doAct)
			{
				estimateLabel.Text = "You can close this dialog now and do other things,\r\nincluding preload along another route.\r\nDownload will continue, watch the green button at the bottom of the screen.";
			}
			else
			{
				// try to come up with some numbers here:
				int factor = 0;
				double mbFactor = 0;
				if(m_doAerial) { factor++; mbFactor += 10.0d; }
				if(m_doColor) { factor++; mbFactor += 15.0d; }
				if(m_doTopo) { factor++; mbFactor += 10.0d; }
				mbFactor /= factor;
				int toCover = m_listOfTiles.Count;
				int toLoad = toCover;	// no way to know which are loaded
				int toLoadMb = (int)Math.Round(m_listOfTiles.Count * mbFactor / 1000.0d);
				estimateLabel.Text = "tiles cache: " + Project.GetTerraserverMapsBasePath()
					+ "\r\ntiles along the route: " + toCover
					+ (toLoadMb > 0 ? "  - approximately " + toLoadMb + " Mb" : "");

				// Estimate exported size:
				int toLoadMbExport = toLoadMb;
				double tileSize = 41.5d;
				double aerialTileSize = Project.pdaExportUseWebsafeGrayscalePalette ? 40.1d : 19.6d;
				double aerialShare = m_doAerial ? (1.0d / factor) : 0.0d;
				double nonAerialShare = 1.0d - aerialShare;

				switch(Project.pdaExportImageFormat)
				{
					case 0:		// bmp websafe
					case 1:		// bmp optimized
						toLoadMbExport = (int)Math.Round(m_listOfTiles.Count * (tileSize * nonAerialShare + aerialTileSize * aerialShare) / 1000.0d);
						break;
					case 2:		// jpeg
						break;
				}
				estimateLabel.Text += "\r\nPDA export size: " + toLoadMbExport + " Mb - to folder: " + Project.exportDestFolder;

			}

			// wrap-up and return:
			Cursor.Current = Cursors.Default;
			if(doAct)
			{
				closeButton.Text = "Close";
			}

			LibSys.StatusBar.Trace("Preload along route: working...");
		}

		private void loadButton_Click(object sender, System.EventArgs e)
		{
			estimateOrAct(true, false);
		}

		private void DlgPreloadTilesAlongRoute_Move(object sender, System.EventArgs e)
		{
			estimateOrAct(false, true);
		}

		private void DlgPreloadTilesAlongRoute_Closed(object sender, System.EventArgs e)
		{
			CameraManager.This.ProcessCameraMove();
		}

		private void scaleUpCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			scaleUpDefaultChecked = scaleUpCheckBox.Checked;	// for the next time the dialig opens
			estimateOrAct(false, false);
		}

		private void scaleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			m_preloadScale = scaleComboBox.SelectedIndex;
			estimateOrAct(false, false);
		}

		private void spreadComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			m_spreadIndex = spreadComboBox.SelectedIndex;

			m_spread = m_spreads[m_spreadIndex];

			PictureManager.This.Refresh();

			initiateRefresh();
		}

		private void initiateRefresh()
		{
			selectParametersGroupBox.Enabled = false;

			refreshPictureTimer.Interval = 1500;
			refreshPictureTimer.Tick += new EventHandler(refreshPictureCallback);
			refreshPictureTimer.Start();
		}

		private void refreshPictureCallback(object obj, System.EventArgs args)
		{
			refreshPictureTimer.Stop();	// it is a one-time deal, stop the timer
			estimateOrAct(false, true);
			selectParametersGroupBox.Enabled = true;
		}

		private void anyCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(m_loaded)
			{
				if(!aerialCheckBox.Checked && !topoCheckBox.Checked && !colorCheckBox.Checked)
				{
					//this.estimateLabel.TabIndexChanged = "at least one of the checkboxes must be checked";
					Project.ShowPopup(colorCheckBox, "at least one of the checkboxes must be checked", Point.Empty);
					aerialCheckBox.Checked = m_doAerial;
					topoCheckBox.Checked = m_doTopo;
					colorCheckBox.Checked = m_doColor;
				}
				else
				{
					m_doAerial = aerialCheckBox.Checked;
					m_doTopo = topoCheckBox.Checked;
					m_doColor = colorCheckBox.Checked;
				}
				estimateOrAct(false, false);
			}
		}

		private void progressLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ProgressMonitorForm.BringFormUp(false);
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

		private bool m_canceling = false;
		private bool m_working = false;

		private void exportButton_Click(object sender, System.EventArgs e)
		{
			exportButton.Enabled = false;
			browseButton.Enabled = false;
			loadButton.Enabled = false;
			closeButton.Text = "Cancel";
			progressBar1.Visible = true;
			Cursor.Current = Cursors.WaitCursor;

			if(Project.exportDestFolder.Length == 0)
			{
				setExportDestFolder();
			}

			if(Project.exportDestFolder.Length > 0 && Directory.Exists(Project.exportDestFolder))
			{
				int countExported = 0;
				int countTotal = 0;

				m_working = true;
				m_canceling = false;

				exportByList(m_listOfTiles, out countExported, out countTotal);

				if(m_canceling)
				{
					this.estimateLabel.Text = "aborted copying tiles to folder: " + Project.exportDestFolder;
				}
				else
				{
					bool didFrame = false;

					if(CameraManager.This.Elev < 87000.0d)
					{
						Hashtable listOfTiles = new Hashtable();
						int scaleIndex = 14; // TileSetTerra.This.ScaleIndex
						TileSetTerraLayout.listTilesAtLevelsWithType(listOfTiles,
							CameraManager.This.CoverageTopLeft, CameraManager.This.CoverageBottomRight,
							scaleIndex, true, m_doAerial, m_doColor, m_doTopo);

						// remove the tiles that were already downloaded/exported before: 
						foreach(string key in m_listOfTiles.Keys)
						{
							if(listOfTiles.ContainsKey(key))
							{
								listOfTiles.Remove(key);
							}
						}

						string message = "OK: Exported " + countTotal + " tiles along the route.\r\n\r\n"
							+ "Do you want to also export " + listOfTiles.Count + " tiles covering the map as you see it? (16m to 64m per pixel tiles)";

						if(Project.YesNoBox(this, message))
						{
							int countExported1 = 0;
							int countTotal1 = 0;

							exportByList(listOfTiles, out countExported1, out countTotal1);

							countExported += countExported1;
							countTotal += countTotal1;
							didFrame = true;
						}
					}

					if(didFrame)
					{
						this.estimateLabel.Text = "OK: " + countExported + " tiles exported to folder:\r\n        " + Project.exportDestFolder;
					}
					else
					{
						this.estimateLabel.Text = "OK: " + countExported + " tiles out of "
							+ countTotal + "\n  exported to folder: " + Project.exportDestFolder;
					}
				}
			}
			else
			{
				this.estimateLabel.Text = "Error: folder '" + Project.exportDestFolder + "' not valid.";
				setExportDestFolder();
			}
			exportButton.Enabled = true;
			browseButton.Enabled = true;
			loadButton.Enabled = true;
			Cursor.Current = Cursors.Default;
			closeButton.Text = "Close";
			progressBar1.Visible = false;
			m_working = false;
		}

		private void exportByList(Hashtable listOfTiles, out int countExported, out int countTotal)
		{
			countExported = 0;
			countTotal = 0;

			int toExport = listOfTiles.Count;
			DateTime lastDisplayed = DateTime.Now;
			DateTime started = DateTime.Now;

			// save selected tiles. This loop takes time, if BMP conversion is performed.
			foreach(string tileName in listOfTiles.Keys)
			{
				if(m_canceling)
				{
					break;
				}

				string fileNameJpg = tileName + ".jpg";
				string srcPath = Project.GetTerraserverPath(fileNameJpg);

				int percentComplete = (int)(countTotal * 100.0d / toExport);

				if(File.Exists(srcPath))
				{
					try 
					{
						string dstPath = PdaHelper.exportSingleFile(tileName, srcPath, true);

						if((DateTime.Now - lastDisplayed).TotalMilliseconds > 500)
						{
							string timeLeftStr = "";
							if(percentComplete > 1.0d)
							{
								TimeSpan elapsed = DateTime.Now - started;
								TimeSpan projected = new TimeSpan((long)(((double)elapsed.Ticks) / ((double)percentComplete) * 100.0d));
								TimeSpan left = projected - elapsed;
								timeLeftStr = "    (" + left.Minutes + " min. " + left.Seconds + " sec. left)";
							}

							this.estimateLabel.Text = "" + countTotal + " of " + toExport + "  -  " + percentComplete + "%" + timeLeftStr + "\r\n" + dstPath;
							this.estimateLabel.Refresh();
							progressBar1.Value = Math.Min(percentComplete, 100);
							progressBar1.Refresh();
							Application.DoEvents();
							lastDisplayed = DateTime.Now;
						}
						countExported++;
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

			this.estimateLabel.Text = "100% complete";
			this.estimateLabel.Refresh();
			progressBar1.Value = 100;
			progressBar1.Refresh();
			Application.DoEvents();
		}

		private static bool shownHint = false;

		private void exportButton_MouseHover(object sender, System.EventArgs e)
		{
			if(!shownHint)
			{
				Project.ShowPopup(exportButton, "copy all tiles inside highlighted area to a folder\nfor use on PDA. Make sure download completed.\n\nCheck PDA Options tab to set format.", Point.Empty);
				shownHint = true;
			}
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			setExportDestFolder();
			estimateOrAct(false, false);
		}

		private void pdaOptionsChanged(object sender, System.EventArgs e)
		{
			estimateOrAct(false, false);
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			m_canceling = true;
			if(!m_working)
			{
				this.Close();
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			estimateOrAct(false, false);
		}

		private void estimateLabel_Click(object sender, System.EventArgs e)
		{
			estimateOrAct(false, false);
		}
	}
}
