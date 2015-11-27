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
using Microsoft.Win32;		// for registry

using LibSys;
using LibNet;
using LibGui;
using LibGeo;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for OptionsForm.
	/// </summary>
	public class OptionsForm : System.Windows.Forms.Form
	{
		protected PictureManager m_pictureManager;
		private static int selectedTab = 0;
		private double[] camAlts = new double[] {0.4d, 0.2d, 0.1d, 0.05d};	// keep in sync with minCamAltComboBox collection
		private bool inSet = false;

		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage mapTabPage;
		private System.Windows.Forms.TabPage earthquakeTabPage;
		private System.Windows.Forms.TabPage waypointsTabPage;
		private System.Windows.Forms.ComboBox distanceUnitsComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox coordStyleComboBox;
		private System.Windows.Forms.CheckBox mapPopupsCheckBox;
		private System.Windows.Forms.CheckBox eqUseOldDataCheckBox;
		private System.Windows.Forms.CheckBox showWaypointsCheckBox;
		private System.Windows.Forms.CheckBox showNumbersCheckBox;
		private System.Windows.Forms.CheckBox fetchOnStartCheckBox;
		private System.Windows.Forms.PictureBox depthCcPictureBox;
		private System.Windows.Forms.CheckBox drawCameraAimCheckBox;
		private System.Windows.Forms.CheckBox drawRulersCheckBox;
		private System.Windows.Forms.CheckBox printShadowRulersCheckBox;
		private System.Windows.Forms.CheckBox showEarthquakesCheckBox;
		private System.Windows.Forms.CheckBox drawCornersCheckBox;
		private System.Windows.Forms.TabPage filesTabPage;
		private System.Windows.Forms.CheckBox associateLocCheckBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button associateApplyButton;
		private System.Windows.Forms.CheckBox associateGpxCheckBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.TabPage proxyTabPage;
		private System.Windows.Forms.CheckBox useProxyCheckBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox proxyServerTextBox;
		private System.Windows.Forms.TextBox proxyPortTextBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.CheckBox showNamesCheckBox;
		private System.Windows.Forms.CheckBox associateGpzCheckBox;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ComboBox waypointNameStyleComboBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox distortionFactorTextBox;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label assocGpxLabel;
		private System.Windows.Forms.Label assocGpzLabel;
		private System.Windows.Forms.Label assocLocLabel;
		public System.Windows.Forms.Button browseFolderButton;
		private System.Windows.Forms.Label label12;
		public System.Windows.Forms.TextBox cacheFolderTextBox;
		public System.Windows.Forms.Button resetMappingCacheLocationButton;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.ComboBox minCamAltComboBox;
		private System.Windows.Forms.CheckBox suspendKeepAliveCheckBox;
		private System.Windows.Forms.CheckBox forceEmptyProxyCheckBox;
		private System.Windows.Forms.LinkLabel supportFaqLinkLabel;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox eqFillModeTimeComboBox;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.CheckBox eqFillOnlyRecentCheckBox;
		private System.Windows.Forms.CheckBox useIconsCheckBox;
		private System.Windows.Forms.Label iconsHintLabel;
		private System.Windows.Forms.TabPage otherTabPage;
		private System.Windows.Forms.CheckBox helpAtStartCheckBox;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.RadioButton mouseWheelActionNoneRadioButton;
		private System.Windows.Forms.RadioButton mouseWheelActionZoomRadioButton;
		private System.Windows.Forms.RadioButton mouseWheelActionPanRadioButton;
		private System.Windows.Forms.Label wheelNoteLabel;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.ComboBox trackNameStyleComboBox;
		private System.Windows.Forms.LinkLabel moreOptionsLinkLabel;
		private System.Windows.Forms.RadioButton mouseWheelActionZoomRevRadioButton;
		private System.Windows.Forms.TabPage GEexportTabPage;
		private System.Windows.Forms.Panel kmlPanel;
		private System.Windows.Forms.CheckBox kmlShowPopupCheckBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OptionsForm(PictureManager pictureManager, int mode)
		{
			m_pictureManager = pictureManager;

			inSet = true;

			InitializeComponent();

			distanceUnitsComboBox.SelectedIndex = Project.unitsDistance;
			coordStyleComboBox.SelectedIndex = Project.coordStyle;
			distortionFactorTextBox.Text = "" + Project.distortionFactor;
			mapPopupsCheckBox.Checked = Project.allowMapPopups;
			showEarthquakesCheckBox.Checked = Project.drawEarthquakes;
			eqUseOldDataCheckBox.Checked = Project.eqUseOldData;
			fetchOnStartCheckBox.Checked = Project.eqFetchOnStart;
			showWaypointsCheckBox.Checked = Project.drawWaypoints;
			eqFillOnlyRecentCheckBox.Checked = Project.earthquakeStyleFillRecent;
			eqFillModeTimeComboBox.SelectedIndex = Project.earthquakeStyleFillHowRecentIndex;

			waypointNameStyleComboBox.Items.AddRange(Project.waypointNameStyleChoices);
			waypointNameStyleComboBox.SelectedIndex = Project.waypointNameStyle;

			suspendKeepAliveCheckBox.Checked = Project.suspendKeepAlive;
			useProxyCheckBox.Checked = Project.useProxy;
			forceEmptyProxyCheckBox.Checked = Project.forceEmptyProxy;
			forceEmptyProxyCheckBox.Enabled = !Project.useProxy;
			proxyServerTextBox.Text  = Project.proxyServer;
			proxyPortTextBox.Text    = "" + Project.proxyPort;

			kmlShowPopupCheckBox.Checked = Project.kmlOptions.ShowInPopup;

//			kmlShowPopupCheckBox.DataBindings.Add("Checked", Project.kmlOptions, "ShowInPopup");

			KmlDocumentControl kdc = new KmlDocumentControl();
			kdc.Dock = System.Windows.Forms.DockStyle.Fill;
			kmlPanel.Controls.Add(kdc);


			switch(mode)
			{
				default:
				case 0:
					// stay on the last chosen 
					tabControl.SelectedIndex = (selectedTab == 1) ? 0 : selectedTab;
					break;
				case 1:
					// first tab 
					break;
				case 2:
					// go to Earthquakes tab
					tabControl.SelectedIndex = 1;
					break;
				case 3:
					// go to Waypoints tab
					tabControl.SelectedIndex = 2;
					break;
				case 4:
					// go to GE export options tab
					tabControl.SelectedIndex = 3;
					break;
				case 5:
					// go to Files tab
					tabControl.SelectedIndex = 4;
					break;
			}
			Project.setDlgIcon(this);

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
			} 
			catch {}

			drawCameraAimCheckBox.Checked = Project.drawCentralPoint;
			drawCornersCheckBox.Checked = Project.drawCornerArrows;
			drawRulersCheckBox.Checked = Project.drawRulers;
			printShadowRulersCheckBox.Checked = Project.printMinuteRulersUseShadow;
			showNumbersCheckBox.Checked = Project.showTrackpointNumbers;
			showNamesCheckBox.Checked = Project.showWaypointNames;

			for(int i=0; i < camAlts.Length ;i++)
			{
				if(Project.cameraHeightMin >= camAlts[i])
				{
					minCamAltComboBox.SelectedIndex = i;
					break;
				}
			}

			cacheFolderTextBox.Text = Project.mapsPath;
			useIconsCheckBox.Checked = Project.useWaypointIcons;
			helpAtStartCheckBox.Checked = Project.showHelpAtStart;

			string iconsPath = Project.GetWptPath("");
			iconsHintLabel.Text = "Note: place your .gif icons in\r\n     " + iconsPath;

			if(Project.mouseWheelAction == 0)
			{
				mouseWheelActionNoneRadioButton.Checked = true;
			}
			else if(Project.mouseWheelAction == 1)
			{
				mouseWheelActionZoomRadioButton.Checked = true;
			}
			else if(Project.mouseWheelAction == 2)
			{
				mouseWheelActionZoomRevRadioButton.Checked = true;
			}
			else if(Project.mouseWheelAction == 3)
			{
				mouseWheelActionPanRadioButton.Checked = true;
			}

			wheelNoteLabel.Text = "1. Hold Shift key to enable Pan when Zoom is selected and vice versa.\r\n\r\n2. Hold Ctrl to pan horizontally.";

			inSet = false;
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
			this.tabControl = new System.Windows.Forms.TabControl();
			this.mapTabPage = new System.Windows.Forms.TabPage();
			this.minCamAltComboBox = new System.Windows.Forms.ComboBox();
			this.label13 = new System.Windows.Forms.Label();
			this.resetMappingCacheLocationButton = new System.Windows.Forms.Button();
			this.cacheFolderTextBox = new System.Windows.Forms.TextBox();
			this.browseFolderButton = new System.Windows.Forms.Button();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.distortionFactorTextBox = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.drawCornersCheckBox = new System.Windows.Forms.CheckBox();
			this.printShadowRulersCheckBox = new System.Windows.Forms.CheckBox();
			this.drawRulersCheckBox = new System.Windows.Forms.CheckBox();
			this.drawCameraAimCheckBox = new System.Windows.Forms.CheckBox();
			this.mapPopupsCheckBox = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.coordStyleComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.distanceUnitsComboBox = new System.Windows.Forms.ComboBox();
			this.earthquakeTabPage = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label14 = new System.Windows.Forms.Label();
			this.eqFillModeTimeComboBox = new System.Windows.Forms.ComboBox();
			this.eqFillOnlyRecentCheckBox = new System.Windows.Forms.CheckBox();
			this.showEarthquakesCheckBox = new System.Windows.Forms.CheckBox();
			this.depthCcPictureBox = new System.Windows.Forms.PictureBox();
			this.fetchOnStartCheckBox = new System.Windows.Forms.CheckBox();
			this.eqUseOldDataCheckBox = new System.Windows.Forms.CheckBox();
			this.waypointsTabPage = new System.Windows.Forms.TabPage();
			this.moreOptionsLinkLabel = new System.Windows.Forms.LinkLabel();
			this.label19 = new System.Windows.Forms.Label();
			this.trackNameStyleComboBox = new System.Windows.Forms.ComboBox();
			this.iconsHintLabel = new System.Windows.Forms.Label();
			this.useIconsCheckBox = new System.Windows.Forms.CheckBox();
			this.label9 = new System.Windows.Forms.Label();
			this.waypointNameStyleComboBox = new System.Windows.Forms.ComboBox();
			this.showNamesCheckBox = new System.Windows.Forms.CheckBox();
			this.showWaypointsCheckBox = new System.Windows.Forms.CheckBox();
			this.showNumbersCheckBox = new System.Windows.Forms.CheckBox();
			this.GEexportTabPage = new System.Windows.Forms.TabPage();
			this.kmlShowPopupCheckBox = new System.Windows.Forms.CheckBox();
			this.kmlPanel = new System.Windows.Forms.Panel();
			this.filesTabPage = new System.Windows.Forms.TabPage();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.assocLocLabel = new System.Windows.Forms.Label();
			this.assocGpzLabel = new System.Windows.Forms.Label();
			this.assocGpxLabel = new System.Windows.Forms.Label();
			this.associateGpzCheckBox = new System.Windows.Forms.CheckBox();
			this.messageLabel = new System.Windows.Forms.Label();
			this.associateGpxCheckBox = new System.Windows.Forms.CheckBox();
			this.associateApplyButton = new System.Windows.Forms.Button();
			this.associateLocCheckBox = new System.Windows.Forms.CheckBox();
			this.proxyTabPage = new System.Windows.Forms.TabPage();
			this.supportFaqLinkLabel = new System.Windows.Forms.LinkLabel();
			this.forceEmptyProxyCheckBox = new System.Windows.Forms.CheckBox();
			this.suspendKeepAliveCheckBox = new System.Windows.Forms.CheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.proxyPortTextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.proxyServerTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.useProxyCheckBox = new System.Windows.Forms.CheckBox();
			this.otherTabPage = new System.Windows.Forms.TabPage();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.mouseWheelActionZoomRevRadioButton = new System.Windows.Forms.RadioButton();
			this.wheelNoteLabel = new System.Windows.Forms.Label();
			this.mouseWheelActionPanRadioButton = new System.Windows.Forms.RadioButton();
			this.mouseWheelActionZoomRadioButton = new System.Windows.Forms.RadioButton();
			this.mouseWheelActionNoneRadioButton = new System.Windows.Forms.RadioButton();
			this.label15 = new System.Windows.Forms.Label();
			this.helpAtStartCheckBox = new System.Windows.Forms.CheckBox();
			this.closeButton = new System.Windows.Forms.Button();
			this.tabControl.SuspendLayout();
			this.mapTabPage.SuspendLayout();
			this.earthquakeTabPage.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.waypointsTabPage.SuspendLayout();
			this.GEexportTabPage.SuspendLayout();
			this.filesTabPage.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.proxyTabPage.SuspendLayout();
			this.otherTabPage.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.mapTabPage,
																					 this.earthquakeTabPage,
																					 this.waypointsTabPage,
																					 this.GEexportTabPage,
																					 this.filesTabPage,
																					 this.proxyTabPage,
																					 this.otherTabPage});
			this.tabControl.Location = new System.Drawing.Point(15, 8);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(453, 264);
			this.tabControl.TabIndex = 0;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// mapTabPage
			// 
			this.mapTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.minCamAltComboBox,
																					 this.label13,
																					 this.resetMappingCacheLocationButton,
																					 this.cacheFolderTextBox,
																					 this.browseFolderButton,
																					 this.label12,
																					 this.label11,
																					 this.distortionFactorTextBox,
																					 this.label10,
																					 this.drawCornersCheckBox,
																					 this.printShadowRulersCheckBox,
																					 this.drawRulersCheckBox,
																					 this.drawCameraAimCheckBox,
																					 this.mapPopupsCheckBox,
																					 this.label2,
																					 this.coordStyleComboBox,
																					 this.label1,
																					 this.distanceUnitsComboBox});
			this.mapTabPage.Location = new System.Drawing.Point(4, 22);
			this.mapTabPage.Name = "mapTabPage";
			this.mapTabPage.Size = new System.Drawing.Size(445, 238);
			this.mapTabPage.TabIndex = 0;
			this.mapTabPage.Text = "Maps";
			// 
			// minCamAltComboBox
			// 
			this.minCamAltComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.minCamAltComboBox.Items.AddRange(new object[] {
																   "400 m",
																   "200 m",
																   "100 m",
																   "50 m"});
			this.minCamAltComboBox.Location = new System.Drawing.Point(335, 48);
			this.minCamAltComboBox.Name = "minCamAltComboBox";
			this.minCamAltComboBox.Size = new System.Drawing.Size(100, 21);
			this.minCamAltComboBox.TabIndex = 16;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(325, 24);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(112, 16);
			this.label13.TabIndex = 15;
			this.label13.Text = "Min Camera Altitude:";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// resetMappingCacheLocationButton
			// 
			this.resetMappingCacheLocationButton.Location = new System.Drawing.Point(380, 176);
			this.resetMappingCacheLocationButton.Name = "resetMappingCacheLocationButton";
			this.resetMappingCacheLocationButton.Size = new System.Drawing.Size(55, 16);
			this.resetMappingCacheLocationButton.TabIndex = 14;
			this.resetMappingCacheLocationButton.Text = "reset";
			this.resetMappingCacheLocationButton.Click += new System.EventHandler(this.resetMappingCacheLocationButton_Click);
			// 
			// cacheFolderTextBox
			// 
			this.cacheFolderTextBox.Location = new System.Drawing.Point(10, 200);
			this.cacheFolderTextBox.Name = "cacheFolderTextBox";
			this.cacheFolderTextBox.ReadOnly = true;
			this.cacheFolderTextBox.Size = new System.Drawing.Size(395, 20);
			this.cacheFolderTextBox.TabIndex = 11;
			this.cacheFolderTextBox.Text = "";
			// 
			// browseFolderButton
			// 
			this.browseFolderButton.Location = new System.Drawing.Point(410, 200);
			this.browseFolderButton.Name = "browseFolderButton";
			this.browseFolderButton.Size = new System.Drawing.Size(25, 20);
			this.browseFolderButton.TabIndex = 13;
			this.browseFolderButton.Text = "...";
			this.browseFolderButton.Click += new System.EventHandler(this.browseFolderButton_Click);
			// 
			// label12
			// 
			this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label12.Location = new System.Drawing.Point(10, 176);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(344, 16);
			this.label12.TabIndex = 12;
			this.label12.Text = "Mapping cache location:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(180, 80);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(232, 22);
			this.label11.TabIndex = 10;
			this.label11.Text = "(0.8 to 1.2 - to make printed map rectangular)";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// distortionFactorTextBox
			// 
			this.distortionFactorTextBox.Location = new System.Drawing.Point(115, 80);
			this.distortionFactorTextBox.Name = "distortionFactorTextBox";
			this.distortionFactorTextBox.Size = new System.Drawing.Size(56, 20);
			this.distortionFactorTextBox.TabIndex = 9;
			this.distortionFactorTextBox.Text = "";
			this.distortionFactorTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.distortionFactorTextBox_Validating);
			this.distortionFactorTextBox.TextChanged += new System.EventHandler(this.distortionFactorTextBox_TextChanged);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(5, 80);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(104, 22);
			this.label10.TabIndex = 8;
			this.label10.Text = "Distortion Factor:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// drawCornersCheckBox
			// 
			this.drawCornersCheckBox.Location = new System.Drawing.Point(205, 120);
			this.drawCornersCheckBox.Name = "drawCornersCheckBox";
			this.drawCornersCheckBox.Size = new System.Drawing.Size(146, 23);
			this.drawCornersCheckBox.TabIndex = 7;
			this.drawCornersCheckBox.Text = "draw corner arrows";
			this.drawCornersCheckBox.CheckedChanged += new System.EventHandler(this.drawCornersCheckBox_CheckedChanged);
			// 
			// printShadowRulersCheckBox
			// 
			this.printShadowRulersCheckBox.Location = new System.Drawing.Point(205, 144);
			this.printShadowRulersCheckBox.Name = "printShadowRulersCheckBox";
			this.printShadowRulersCheckBox.Size = new System.Drawing.Size(219, 22);
			this.printShadowRulersCheckBox.TabIndex = 6;
			this.printShadowRulersCheckBox.Text = "print shadow for minute rulers";
			this.printShadowRulersCheckBox.CheckedChanged += new System.EventHandler(this.printShadowRulersCheckBox_CheckedChanged);
			// 
			// drawRulersCheckBox
			// 
			this.drawRulersCheckBox.Location = new System.Drawing.Point(30, 144);
			this.drawRulersCheckBox.Name = "drawRulersCheckBox";
			this.drawRulersCheckBox.Size = new System.Drawing.Size(146, 22);
			this.drawRulersCheckBox.TabIndex = 5;
			this.drawRulersCheckBox.Text = "draw minute rulers";
			this.drawRulersCheckBox.CheckedChanged += new System.EventHandler(this.drawRulersCheckBox_CheckedChanged);
			// 
			// drawCameraAimCheckBox
			// 
			this.drawCameraAimCheckBox.Location = new System.Drawing.Point(30, 128);
			this.drawCameraAimCheckBox.Name = "drawCameraAimCheckBox";
			this.drawCameraAimCheckBox.Size = new System.Drawing.Size(146, 23);
			this.drawCameraAimCheckBox.TabIndex = 4;
			this.drawCameraAimCheckBox.Text = "draw camera aim";
			this.drawCameraAimCheckBox.CheckedChanged += new System.EventHandler(this.drawCameraAimCheckBox_CheckedChanged);
			// 
			// mapPopupsCheckBox
			// 
			this.mapPopupsCheckBox.Location = new System.Drawing.Point(30, 112);
			this.mapPopupsCheckBox.Name = "mapPopupsCheckBox";
			this.mapPopupsCheckBox.Size = new System.Drawing.Size(146, 23);
			this.mapPopupsCheckBox.TabIndex = 2;
			this.mapPopupsCheckBox.Text = "allow map popups";
			this.mapPopupsCheckBox.CheckedChanged += new System.EventHandler(this.mapPopupsCheckBox_CheckedChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(5, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 22);
			this.label2.TabIndex = 3;
			this.label2.Text = "Coordinates Style:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// coordStyleComboBox
			// 
			this.coordStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.coordStyleComboBox.Items.AddRange(new object[] {
																	"N37°28\'54\"  W117°43\'21\"",
																	"N37°28.893\'  W117°43.368\'",
																	"N37.48567  W117.73672",
																	"UTM (NAD83)"});
			this.coordStyleComboBox.Location = new System.Drawing.Point(115, 48);
			this.coordStyleComboBox.Name = "coordStyleComboBox";
			this.coordStyleComboBox.Size = new System.Drawing.Size(205, 21);
			this.coordStyleComboBox.TabIndex = 1;
			this.coordStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.coordStyleComboBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(5, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 22);
			this.label1.TabIndex = 1;
			this.label1.Text = "Distance Units:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// distanceUnitsComboBox
			// 
			this.distanceUnitsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.distanceUnitsComboBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.distanceUnitsComboBox.Items.AddRange(new object[] {
																	   "Km,             m,  km/hr",
																	   "meters,             m/sec",
																	   "miles,          ft, mi/hr",
																	   "nautical miles, ft, knots",
																	   "feet,               ft/sec"});
			this.distanceUnitsComboBox.Location = new System.Drawing.Point(115, 16);
			this.distanceUnitsComboBox.Name = "distanceUnitsComboBox";
			this.distanceUnitsComboBox.Size = new System.Drawing.Size(205, 22);
			this.distanceUnitsComboBox.TabIndex = 0;
			this.distanceUnitsComboBox.SelectedIndexChanged += new System.EventHandler(this.distanceUnitsComboBox_SelectedIndexChanged);
			// 
			// earthquakeTabPage
			// 
			this.earthquakeTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.groupBox2,
																							this.showEarthquakesCheckBox,
																							this.depthCcPictureBox,
																							this.fetchOnStartCheckBox,
																							this.eqUseOldDataCheckBox});
			this.earthquakeTabPage.Location = new System.Drawing.Point(4, 22);
			this.earthquakeTabPage.Name = "earthquakeTabPage";
			this.earthquakeTabPage.Size = new System.Drawing.Size(445, 238);
			this.earthquakeTabPage.TabIndex = 1;
			this.earthquakeTabPage.Text = "Earthquakes";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label14,
																					this.eqFillModeTimeComboBox,
																					this.eqFillOnlyRecentCheckBox});
			this.groupBox2.Location = new System.Drawing.Point(16, 112);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(272, 72);
			this.groupBox2.TabIndex = 23;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Fill mode";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(16, 40);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(88, 22);
			this.label14.TabIndex = 33;
			this.label14.Text = "Within the last";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// eqFillModeTimeComboBox
			// 
			this.eqFillModeTimeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.eqFillModeTimeComboBox.Items.AddRange(new object[] {
																		"1 hour",
																		"2 hours",
																		"4 hours",
																		"8 hours",
																		"12 hours",
																		"18 hours",
																		"1 day",
																		"2 days",
																		"3 days",
																		"5 days",
																		"10 days"});
			this.eqFillModeTimeComboBox.Location = new System.Drawing.Point(120, 40);
			this.eqFillModeTimeComboBox.Name = "eqFillModeTimeComboBox";
			this.eqFillModeTimeComboBox.Size = new System.Drawing.Size(102, 21);
			this.eqFillModeTimeComboBox.TabIndex = 32;
			this.eqFillModeTimeComboBox.SelectedIndexChanged += new System.EventHandler(this.eqFillModeTimeComboBox_SelectedIndexChanged);
			// 
			// eqFillOnlyRecentCheckBox
			// 
			this.eqFillOnlyRecentCheckBox.Location = new System.Drawing.Point(16, 16);
			this.eqFillOnlyRecentCheckBox.Name = "eqFillOnlyRecentCheckBox";
			this.eqFillOnlyRecentCheckBox.Size = new System.Drawing.Size(248, 23);
			this.eqFillOnlyRecentCheckBox.TabIndex = 22;
			this.eqFillOnlyRecentCheckBox.Text = "Fill only recent earthquakes on the map";
			this.eqFillOnlyRecentCheckBox.CheckedChanged += new System.EventHandler(this.eqFillOnlyRecentCheckBox_CheckedChanged);
			// 
			// showEarthquakesCheckBox
			// 
			this.showEarthquakesCheckBox.Location = new System.Drawing.Point(32, 32);
			this.showEarthquakesCheckBox.Name = "showEarthquakesCheckBox";
			this.showEarthquakesCheckBox.Size = new System.Drawing.Size(175, 23);
			this.showEarthquakesCheckBox.TabIndex = 21;
			this.showEarthquakesCheckBox.Text = "Show earthquakes";
			this.showEarthquakesCheckBox.CheckedChanged += new System.EventHandler(this.showEarthquakesCheckBox_CheckedChanged);
			// 
			// depthCcPictureBox
			// 
			this.depthCcPictureBox.Location = new System.Drawing.Point(304, 32);
			this.depthCcPictureBox.Name = "depthCcPictureBox";
			this.depthCcPictureBox.Size = new System.Drawing.Size(110, 155);
			this.depthCcPictureBox.TabIndex = 12;
			this.depthCcPictureBox.TabStop = false;
			// 
			// fetchOnStartCheckBox
			// 
			this.fetchOnStartCheckBox.Location = new System.Drawing.Point(32, 80);
			this.fetchOnStartCheckBox.Name = "fetchOnStartCheckBox";
			this.fetchOnStartCheckBox.Size = new System.Drawing.Size(255, 23);
			this.fetchOnStartCheckBox.TabIndex = 11;
			this.fetchOnStartCheckBox.Text = "Fetch earthquakes on start";
			this.fetchOnStartCheckBox.CheckedChanged += new System.EventHandler(this.fetchOnStartCheckBox_CheckedChanged);
			// 
			// eqUseOldDataCheckBox
			// 
			this.eqUseOldDataCheckBox.Location = new System.Drawing.Point(32, 56);
			this.eqUseOldDataCheckBox.Name = "eqUseOldDataCheckBox";
			this.eqUseOldDataCheckBox.Size = new System.Drawing.Size(255, 23);
			this.eqUseOldDataCheckBox.TabIndex = 10;
			this.eqUseOldDataCheckBox.Text = "Use old data when cannot reach new";
			this.eqUseOldDataCheckBox.CheckedChanged += new System.EventHandler(this.eqUseOldDataCheckBox_CheckedChanged);
			// 
			// waypointsTabPage
			// 
			this.waypointsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.moreOptionsLinkLabel,
																						   this.label19,
																						   this.trackNameStyleComboBox,
																						   this.iconsHintLabel,
																						   this.useIconsCheckBox,
																						   this.label9,
																						   this.waypointNameStyleComboBox,
																						   this.showNamesCheckBox,
																						   this.showWaypointsCheckBox,
																						   this.showNumbersCheckBox});
			this.waypointsTabPage.Location = new System.Drawing.Point(4, 22);
			this.waypointsTabPage.Name = "waypointsTabPage";
			this.waypointsTabPage.Size = new System.Drawing.Size(445, 238);
			this.waypointsTabPage.TabIndex = 2;
			this.waypointsTabPage.Text = "Waypoints";
			// 
			// moreOptionsLinkLabel
			// 
			this.moreOptionsLinkLabel.Location = new System.Drawing.Point(330, 25);
			this.moreOptionsLinkLabel.Name = "moreOptionsLinkLabel";
			this.moreOptionsLinkLabel.Size = new System.Drawing.Size(100, 20);
			this.moreOptionsLinkLabel.TabIndex = 72;
			this.moreOptionsLinkLabel.TabStop = true;
			this.moreOptionsLinkLabel.Text = "more options...";
			this.moreOptionsLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.moreOptionsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.moreOptionsLinkLabel_LinkClicked);
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(20, 130);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(145, 20);
			this.label19.TabIndex = 71;
			this.label19.Text = "Tracks Naming Style:";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// trackNameStyleComboBox
			// 
			this.trackNameStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.trackNameStyleComboBox.Location = new System.Drawing.Point(170, 130);
			this.trackNameStyleComboBox.Name = "trackNameStyleComboBox";
			this.trackNameStyleComboBox.Size = new System.Drawing.Size(165, 21);
			this.trackNameStyleComboBox.TabIndex = 70;
			this.trackNameStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.trackNameStyleComboBox_SelectedIndexChanged);
			// 
			// iconsHintLabel
			// 
			this.iconsHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.iconsHintLabel.Location = new System.Drawing.Point(20, 190);
			this.iconsHintLabel.Name = "iconsHintLabel";
			this.iconsHintLabel.Size = new System.Drawing.Size(415, 40);
			this.iconsHintLabel.TabIndex = 67;
			this.iconsHintLabel.Text = "Note: place your .gif icons in ";
			// 
			// useIconsCheckBox
			// 
			this.useIconsCheckBox.Location = new System.Drawing.Point(20, 160);
			this.useIconsCheckBox.Name = "useIconsCheckBox";
			this.useIconsCheckBox.Size = new System.Drawing.Size(175, 23);
			this.useIconsCheckBox.TabIndex = 66;
			this.useIconsCheckBox.Text = "use GPS-style icons";
			this.useIconsCheckBox.CheckedChanged += new System.EventHandler(this.useIconsCheckBox_CheckedChanged);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 100);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(149, 22);
			this.label9.TabIndex = 65;
			this.label9.Text = "Waypoints Naming Style:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// waypointNameStyleComboBox
			// 
			this.waypointNameStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.waypointNameStyleComboBox.Location = new System.Drawing.Point(170, 100);
			this.waypointNameStyleComboBox.Name = "waypointNameStyleComboBox";
			this.waypointNameStyleComboBox.Size = new System.Drawing.Size(165, 21);
			this.waypointNameStyleComboBox.TabIndex = 64;
			this.waypointNameStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.waypointNameStyleComboBox_SelectedIndexChanged);
			// 
			// showNamesCheckBox
			// 
			this.showNamesCheckBox.Location = new System.Drawing.Point(16, 72);
			this.showNamesCheckBox.Name = "showNamesCheckBox";
			this.showNamesCheckBox.Size = new System.Drawing.Size(175, 23);
			this.showNamesCheckBox.TabIndex = 63;
			this.showNamesCheckBox.Text = "show waypoints names";
			this.showNamesCheckBox.CheckedChanged += new System.EventHandler(this.showNamesCheckBox_CheckedChanged);
			// 
			// showWaypointsCheckBox
			// 
			this.showWaypointsCheckBox.Location = new System.Drawing.Point(16, 24);
			this.showWaypointsCheckBox.Name = "showWaypointsCheckBox";
			this.showWaypointsCheckBox.Size = new System.Drawing.Size(192, 23);
			this.showWaypointsCheckBox.TabIndex = 20;
			this.showWaypointsCheckBox.Text = "show tracks and waypoints";
			this.showWaypointsCheckBox.CheckedChanged += new System.EventHandler(this.showWaypointsCheckBox_CheckedChanged);
			// 
			// showNumbersCheckBox
			// 
			this.showNumbersCheckBox.Location = new System.Drawing.Point(16, 48);
			this.showNumbersCheckBox.Name = "showNumbersCheckBox";
			this.showNumbersCheckBox.Size = new System.Drawing.Size(175, 23);
			this.showNumbersCheckBox.TabIndex = 21;
			this.showNumbersCheckBox.Text = "show trackpoint numbers";
			this.showNumbersCheckBox.CheckedChanged += new System.EventHandler(this.showNumbersCheckBox_CheckedChanged);
			// 
			// GEexportTabPage
			// 
			this.GEexportTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.kmlShowPopupCheckBox,
																						  this.kmlPanel});
			this.GEexportTabPage.Location = new System.Drawing.Point(4, 22);
			this.GEexportTabPage.Name = "GEexportTabPage";
			this.GEexportTabPage.Size = new System.Drawing.Size(445, 238);
			this.GEexportTabPage.TabIndex = 6;
			this.GEexportTabPage.Text = "Google Earth";
			// 
			// kmlShowPopupCheckBox
			// 
			this.kmlShowPopupCheckBox.Location = new System.Drawing.Point(15, 210);
			this.kmlShowPopupCheckBox.Name = "kmlShowPopupCheckBox";
			this.kmlShowPopupCheckBox.Size = new System.Drawing.Size(270, 24);
			this.kmlShowPopupCheckBox.TabIndex = 3;
			this.kmlShowPopupCheckBox.Text = "show options pop-up";
			this.kmlShowPopupCheckBox.CheckedChanged += new System.EventHandler(this.kmlShowPopupCheckBox_CheckedChanged);
			// 
			// kmlPanel
			// 
			this.kmlPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.kmlPanel.Name = "kmlPanel";
			this.kmlPanel.Size = new System.Drawing.Size(445, 205);
			this.kmlPanel.TabIndex = 0;
			// 
			// filesTabPage
			// 
			this.filesTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.label4,
																					   this.label3,
																					   this.groupBox1});
			this.filesTabPage.Location = new System.Drawing.Point(4, 22);
			this.filesTabPage.Name = "filesTabPage";
			this.filesTabPage.Size = new System.Drawing.Size(445, 238);
			this.filesTabPage.TabIndex = 3;
			this.filesTabPage.Text = "Files";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 168);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 22);
			this.label4.TabIndex = 24;
			this.label4.Text = "Hint:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(80, 168);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(344, 40);
			this.label3.TabIndex = 23;
			this.label3.Text = "choose to associate files with QuakeMap if you want to see your data by double-cl" +
				"icking on files in Windows";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.assocLocLabel,
																					this.assocGpzLabel,
																					this.assocGpxLabel,
																					this.associateGpzCheckBox,
																					this.messageLabel,
																					this.associateGpxCheckBox,
																					this.associateApplyButton,
																					this.associateLocCheckBox});
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(408, 144);
			this.groupBox1.TabIndex = 22;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Associate Files With QuakeMap";
			// 
			// assocLocLabel
			// 
			this.assocLocLabel.Location = new System.Drawing.Point(304, 48);
			this.assocLocLabel.Name = "assocLocLabel";
			this.assocLocLabel.Size = new System.Drawing.Size(80, 23);
			this.assocLocLabel.TabIndex = 106;
			this.assocLocLabel.Text = "(associated)";
			this.assocLocLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// assocGpzLabel
			// 
			this.assocGpzLabel.Location = new System.Drawing.Point(304, 72);
			this.assocGpzLabel.Name = "assocGpzLabel";
			this.assocGpzLabel.Size = new System.Drawing.Size(80, 23);
			this.assocGpzLabel.TabIndex = 105;
			this.assocGpzLabel.Text = "(associated)";
			this.assocGpzLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// assocGpxLabel
			// 
			this.assocGpxLabel.Location = new System.Drawing.Point(304, 24);
			this.assocGpxLabel.Name = "assocGpxLabel";
			this.assocGpxLabel.Size = new System.Drawing.Size(80, 23);
			this.assocGpxLabel.TabIndex = 104;
			this.assocGpxLabel.Text = "(associated)";
			this.assocGpxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// associateGpzCheckBox
			// 
			this.associateGpzCheckBox.Location = new System.Drawing.Point(24, 72);
			this.associateGpzCheckBox.Name = "associateGpzCheckBox";
			this.associateGpzCheckBox.Size = new System.Drawing.Size(240, 23);
			this.associateGpzCheckBox.TabIndex = 103;
			this.associateGpzCheckBox.Text = ".gpz (Zipped Photo Collections)";
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(136, 104);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(256, 24);
			this.messageLabel.TabIndex = 102;
			// 
			// associateGpxCheckBox
			// 
			this.associateGpxCheckBox.Location = new System.Drawing.Point(24, 24);
			this.associateGpxCheckBox.Name = "associateGpxCheckBox";
			this.associateGpxCheckBox.Size = new System.Drawing.Size(240, 23);
			this.associateGpxCheckBox.TabIndex = 101;
			this.associateGpxCheckBox.Text = ".gpx (TopoGrafix: EasyGPS, ExpertGPS)";
			// 
			// associateApplyButton
			// 
			this.associateApplyButton.Location = new System.Drawing.Point(24, 104);
			this.associateApplyButton.Name = "associateApplyButton";
			this.associateApplyButton.Size = new System.Drawing.Size(95, 22);
			this.associateApplyButton.TabIndex = 100;
			this.associateApplyButton.Text = "Apply";
			this.associateApplyButton.Click += new System.EventHandler(this.associateApplyButton_Click);
			// 
			// associateLocCheckBox
			// 
			this.associateLocCheckBox.Location = new System.Drawing.Point(24, 48);
			this.associateLocCheckBox.Name = "associateLocCheckBox";
			this.associateLocCheckBox.Size = new System.Drawing.Size(136, 23);
			this.associateLocCheckBox.TabIndex = 21;
			this.associateLocCheckBox.Text = ".loc (Geocaching)";
			// 
			// proxyTabPage
			// 
			this.proxyTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.supportFaqLinkLabel,
																					   this.forceEmptyProxyCheckBox,
																					   this.suspendKeepAliveCheckBox,
																					   this.label8,
																					   this.label7,
																					   this.proxyPortTextBox,
																					   this.label6,
																					   this.proxyServerTextBox,
																					   this.label5,
																					   this.useProxyCheckBox});
			this.proxyTabPage.Location = new System.Drawing.Point(4, 22);
			this.proxyTabPage.Name = "proxyTabPage";
			this.proxyTabPage.Size = new System.Drawing.Size(445, 238);
			this.proxyTabPage.TabIndex = 4;
			this.proxyTabPage.Text = "Proxy";
			// 
			// supportFaqLinkLabel
			// 
			this.supportFaqLinkLabel.Location = new System.Drawing.Point(344, 208);
			this.supportFaqLinkLabel.Name = "supportFaqLinkLabel";
			this.supportFaqLinkLabel.Size = new System.Drawing.Size(88, 16);
			this.supportFaqLinkLabel.TabIndex = 12;
			this.supportFaqLinkLabel.TabStop = true;
			this.supportFaqLinkLabel.Text = "Support FAQ";
			this.supportFaqLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.supportFaqLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.supportFaqLinkLabel_LinkClicked);
			// 
			// forceEmptyProxyCheckBox
			// 
			this.forceEmptyProxyCheckBox.Location = new System.Drawing.Point(96, 32);
			this.forceEmptyProxyCheckBox.Name = "forceEmptyProxyCheckBox";
			this.forceEmptyProxyCheckBox.Size = new System.Drawing.Size(146, 23);
			this.forceEmptyProxyCheckBox.TabIndex = 11;
			this.forceEmptyProxyCheckBox.Text = "force Empty Proxy";
			this.forceEmptyProxyCheckBox.CheckedChanged += new System.EventHandler(this.forceEmptyProxyCheckBox_CheckedChanged);
			// 
			// suspendKeepAliveCheckBox
			// 
			this.suspendKeepAliveCheckBox.Location = new System.Drawing.Point(96, 8);
			this.suspendKeepAliveCheckBox.Name = "suspendKeepAliveCheckBox";
			this.suspendKeepAliveCheckBox.Size = new System.Drawing.Size(146, 23);
			this.suspendKeepAliveCheckBox.TabIndex = 10;
			this.suspendKeepAliveCheckBox.Text = "suspend Keep-Alive";
			this.suspendKeepAliveCheckBox.CheckedChanged += new System.EventHandler(this.suspendKeepAliveCheckBox_CheckedChanged);
			// 
			// label8
			// 
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label8.Location = new System.Drawing.Point(32, 160);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(48, 23);
			this.label8.TabIndex = 9;
			this.label8.Text = "Note:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(96, 160);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(328, 48);
			this.label7.TabIndex = 8;
			this.label7.Text = "you probably will never need to set proxy here, as these settings should be autom" +
				"atically picked up from your Internet Explorer settings. See Support FAQ for det" +
				"ails.";
			// 
			// proxyPortTextBox
			// 
			this.proxyPortTextBox.Location = new System.Drawing.Point(96, 120);
			this.proxyPortTextBox.Name = "proxyPortTextBox";
			this.proxyPortTextBox.TabIndex = 7;
			this.proxyPortTextBox.Text = "";
			this.proxyPortTextBox.TextChanged += new System.EventHandler(this.proxyPortTextBox_TextChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 120);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(80, 23);
			this.label6.TabIndex = 6;
			this.label6.Text = "Proxy Port:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// proxyServerTextBox
			// 
			this.proxyServerTextBox.Location = new System.Drawing.Point(96, 88);
			this.proxyServerTextBox.Name = "proxyServerTextBox";
			this.proxyServerTextBox.Size = new System.Drawing.Size(264, 20);
			this.proxyServerTextBox.TabIndex = 5;
			this.proxyServerTextBox.Text = "";
			this.proxyServerTextBox.TextChanged += new System.EventHandler(this.proxyServerTextBox_TextChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 88);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 23);
			this.label5.TabIndex = 4;
			this.label5.Text = "Proxy Server:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// useProxyCheckBox
			// 
			this.useProxyCheckBox.Location = new System.Drawing.Point(96, 56);
			this.useProxyCheckBox.Name = "useProxyCheckBox";
			this.useProxyCheckBox.Size = new System.Drawing.Size(146, 23);
			this.useProxyCheckBox.TabIndex = 3;
			this.useProxyCheckBox.Text = "use HTTP proxy";
			this.useProxyCheckBox.CheckedChanged += new System.EventHandler(this.useProxyCheckBox_CheckedChanged);
			// 
			// otherTabPage
			// 
			this.otherTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.groupBox3,
																					   this.helpAtStartCheckBox});
			this.otherTabPage.Location = new System.Drawing.Point(4, 22);
			this.otherTabPage.Name = "otherTabPage";
			this.otherTabPage.Size = new System.Drawing.Size(445, 238);
			this.otherTabPage.TabIndex = 5;
			this.otherTabPage.Text = "Other";
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.mouseWheelActionZoomRevRadioButton,
																					this.wheelNoteLabel,
																					this.mouseWheelActionPanRadioButton,
																					this.mouseWheelActionZoomRadioButton,
																					this.mouseWheelActionNoneRadioButton,
																					this.label15});
			this.groupBox3.Location = new System.Drawing.Point(10, 105);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(425, 120);
			this.groupBox3.TabIndex = 4;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Mouse Wheel Action";
			// 
			// mouseWheelActionZoomRevRadioButton
			// 
			this.mouseWheelActionZoomRevRadioButton.Location = new System.Drawing.Point(15, 65);
			this.mouseWheelActionZoomRevRadioButton.Name = "mouseWheelActionZoomRevRadioButton";
			this.mouseWheelActionZoomRevRadioButton.Size = new System.Drawing.Size(110, 20);
			this.mouseWheelActionZoomRevRadioButton.TabIndex = 6;
			this.mouseWheelActionZoomRevRadioButton.Text = "Zoom (reverse)";
			this.mouseWheelActionZoomRevRadioButton.CheckedChanged += new System.EventHandler(this.mouseWheelActionRadioButton_CheckedChanged);
			// 
			// wheelNoteLabel
			// 
			this.wheelNoteLabel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.wheelNoteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.wheelNoteLabel.Location = new System.Drawing.Point(135, 30);
			this.wheelNoteLabel.Name = "wheelNoteLabel";
			this.wheelNoteLabel.Size = new System.Drawing.Size(280, 80);
			this.wheelNoteLabel.TabIndex = 3;
			this.wheelNoteLabel.Text = "text here";
			// 
			// mouseWheelActionPanRadioButton
			// 
			this.mouseWheelActionPanRadioButton.Location = new System.Drawing.Point(15, 85);
			this.mouseWheelActionPanRadioButton.Name = "mouseWheelActionPanRadioButton";
			this.mouseWheelActionPanRadioButton.Size = new System.Drawing.Size(60, 20);
			this.mouseWheelActionPanRadioButton.TabIndex = 2;
			this.mouseWheelActionPanRadioButton.Text = "Pan";
			this.mouseWheelActionPanRadioButton.CheckedChanged += new System.EventHandler(this.mouseWheelActionRadioButton_CheckedChanged);
			// 
			// mouseWheelActionZoomRadioButton
			// 
			this.mouseWheelActionZoomRadioButton.Location = new System.Drawing.Point(15, 45);
			this.mouseWheelActionZoomRadioButton.Name = "mouseWheelActionZoomRadioButton";
			this.mouseWheelActionZoomRadioButton.Size = new System.Drawing.Size(60, 20);
			this.mouseWheelActionZoomRadioButton.TabIndex = 1;
			this.mouseWheelActionZoomRadioButton.Text = "Zoom";
			this.mouseWheelActionZoomRadioButton.CheckedChanged += new System.EventHandler(this.mouseWheelActionRadioButton_CheckedChanged);
			// 
			// mouseWheelActionNoneRadioButton
			// 
			this.mouseWheelActionNoneRadioButton.Location = new System.Drawing.Point(15, 25);
			this.mouseWheelActionNoneRadioButton.Name = "mouseWheelActionNoneRadioButton";
			this.mouseWheelActionNoneRadioButton.Size = new System.Drawing.Size(60, 20);
			this.mouseWheelActionNoneRadioButton.TabIndex = 0;
			this.mouseWheelActionNoneRadioButton.Text = "None";
			this.mouseWheelActionNoneRadioButton.CheckedChanged += new System.EventHandler(this.mouseWheelActionRadioButton_CheckedChanged);
			// 
			// label15
			// 
			this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label15.Location = new System.Drawing.Point(85, 30);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(45, 23);
			this.label15.TabIndex = 5;
			this.label15.Text = "Note:";
			this.label15.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// helpAtStartCheckBox
			// 
			this.helpAtStartCheckBox.Location = new System.Drawing.Point(25, 25);
			this.helpAtStartCheckBox.Name = "helpAtStartCheckBox";
			this.helpAtStartCheckBox.Size = new System.Drawing.Size(146, 23);
			this.helpAtStartCheckBox.TabIndex = 3;
			this.helpAtStartCheckBox.Text = "show hints at start";
			this.helpAtStartCheckBox.CheckedChanged += new System.EventHandler(this.helpAtStartCheckBox_CheckedChanged);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(373, 280);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(95, 22);
			this.closeButton.TabIndex = 99;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// OptionsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(476, 312);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.closeButton,
																		  this.tabControl});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OptionsForm";
			this.Text = "Options";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.OptionsForm_Closing);
			this.tabControl.ResumeLayout(false);
			this.mapTabPage.ResumeLayout(false);
			this.earthquakeTabPage.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.waypointsTabPage.ResumeLayout(false);
			this.GEexportTabPage.ResumeLayout(false);
			this.filesTabPage.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.proxyTabPage.ResumeLayout(false);
			this.otherTabPage.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void OptionsForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			selectedTab = tabControl.SelectedIndex;
			try
			{
				Project.cameraHeightMin = camAlts[minCamAltComboBox.SelectedIndex];
			}
			catch
			{
				Project.cameraHeightMin = Project.CAMERA_HEIGHT_MIN_DEFAULT;
			}
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void distanceUnitsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.unitsDistance = distanceUnitsComboBox.SelectedIndex;
			this.m_pictureManager.Refresh();
		}

		private void waypointNameStyleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.waypointNameStyle = waypointNameStyleComboBox.SelectedIndex;
			this.m_pictureManager.CameraManager.ProcessCameraMove();
		}

		private void trackNameStyleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.trackNameStyle = trackNameStyleComboBox.SelectedIndex;
			this.m_pictureManager.CameraManager.ProcessCameraMove();
		}

		private void coordStyleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.coordStyle = coordStyleComboBox.SelectedIndex;
			this.m_pictureManager.CameraManager.ProcessCameraMove();
		}

		private void mapPopupsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.allowMapPopups = mapPopupsCheckBox.Checked;
		}

		private void eqUseOldDataCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.eqUseOldData = eqUseOldDataCheckBox.Checked;
		}

		private void showWaypointsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.mainCommand.wptEnabled(showWaypointsCheckBox.Checked);
		}

		private void gcNamesCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
		}

		private void showNumbersCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.showTrackpointNumbers = showNumbersCheckBox.Checked;
			m_pictureManager.Refresh();
		}

		private void showNamesCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.showWaypointNames = showNamesCheckBox.Checked;
			m_pictureManager.Refresh();
		}

		private void fetchOnStartCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.eqFetchOnStart = fetchOnStartCheckBox.Checked;
		}

		private void drawCameraAimCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.drawCentralPoint = drawCameraAimCheckBox.Checked;
			m_pictureManager.Refresh();
		}

		private void drawCornersCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.drawCornerArrows = drawCornersCheckBox.Checked;
			m_pictureManager.Refresh();
		}

		private void drawRulersCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.drawRulers = drawRulersCheckBox.Checked;
			m_pictureManager.Refresh();
		}

		private void printShadowRulersCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.printMinuteRulersUseShadow = printShadowRulersCheckBox.Checked;
		}

		private void showEarthquakesCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.mainCommand.eqEnabled(showEarthquakesCheckBox.Checked);
		}

		private void associateApplyButton_Click(object sender, System.EventArgs e)
		{
			messageLabel.Text = "";
			string openCmd = Project.GetLongPathName(Application.ExecutablePath) + " \"%1\"";
			if(associateLocCheckBox.Checked)
			{
				try 
				{
					FileAssociation FA = new FileAssociation();
					FA.Extension = "loc";
					FA.ContentType = "application/geocaching";
					FA.FullName = "Geocaching.com .LOC File";
					FA.ProperName = "LOC File";
					FA.IconPath = Project.GetMiscPath(Project.PROGRAM_NAME_HUMAN + ".ico");
					FA.AddCommand("open", openCmd);
					FA.Create();
					associateLocCheckBox.Enabled = false;
					associateLocCheckBox.Checked = false;
					assocLocLabel.Visible = true;
					messageLabel.Text += "Success ";
				} 
				catch (Exception ee)
				{
					messageLabel.Text += ee.Message + " ";
					associateLocCheckBox.Checked = false;
					assocLocLabel.Visible = false;
				}
			}
			if(associateGpxCheckBox.Checked)
			{
				try 
				{
					FileAssociation FA = new FileAssociation();
					FA.Extension = "gpx";
					FA.ContentType = "application/gps-mapping";
					FA.FullName = "GPS eXchange File";
					FA.ProperName = "GPX File";
					FA.IconPath = Project.GetMiscPath(Project.PROGRAM_NAME_HUMAN + ".ico");
					FA.AddCommand("open", openCmd);
					FA.Create();

					associateGpxCheckBox.Enabled = false;
					associateGpxCheckBox.Checked = false;
					assocGpxLabel.Visible = true;

					messageLabel.Text += "Success ";
				} 
				catch (Exception ee)
				{
					messageLabel.Text += ee.Message + " ";
					associateGpxCheckBox.Checked = false;
					assocGpxLabel.Visible = false;
				}
			}
			if(associateGpzCheckBox.Checked)
			{
				try 
				{
					FileAssociation FA = new FileAssociation();
					FA.Extension = "gpz";
					FA.ContentType = "application/gps-photo-collection";
					FA.FullName = "QuakeMap.com .GPZ File";
					FA.ProperName = "GPZ File";
					FA.IconPath = Project.GetMiscPath(Project.PROGRAM_NAME_HUMAN + ".ico");
					FA.AddCommand("open", openCmd);
					FA.Create();

					associateGpzCheckBox.Enabled = false;
					associateGpzCheckBox.Checked = false;
					assocGpzLabel.Visible = true;
					messageLabel.Text += "Success";
				} 
				catch (Exception ee)
				{
					messageLabel.Text += ee.Message;
					associateGpzCheckBox.Checked = false;
					assocGpzLabel.Visible = false;
				}
			}
		}

		private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(tabControl.SelectedTab == filesTabPage)
			{
				checkFileAssociations();
			}
			else if(tabControl.SelectedTab == mapTabPage)
			{
				cacheFolderTextBox.Text = Project.mapsPath;
			}
			else if(tabControl.SelectedTab == waypointsTabPage)
			{
				trackNameStyleComboBox.Items.Clear();
				trackNameStyleComboBox.Items.AddRange(Project.trackNameStyleChoices);
				trackNameStyleComboBox.SelectedIndex = Project.trackNameStyle;
			}
		}

		private void checkFileAssociations()
		{
			string openCmd = Project.GetLongPathName(Application.ExecutablePath) + " \"%1\"";

			FileAssociation FA = new FileAssociation();
			FA.Extension = "gpx";
			FA.AddCommand("open", openCmd);

			associateGpxCheckBox.Checked = !FA.Test();
			associateGpxCheckBox.Enabled = true;
			assocGpxLabel.Visible = !associateGpxCheckBox.Checked;

			FA = new FileAssociation();
			FA.Extension = "loc";
			FA.AddCommand("open", openCmd);

			associateLocCheckBox.Checked = !FA.Test();
			associateLocCheckBox.Enabled = true;
			assocLocLabel.Visible = !associateLocCheckBox.Checked;

			FA = new FileAssociation();
			FA.Extension = "gpz";
			FA.AddCommand("open", openCmd);

			associateGpzCheckBox.Checked = !FA.Test();
			associateGpzCheckBox.Enabled = true;
			assocGpzLabel.Visible = !associateGpzCheckBox.Checked;
		}

		private void suspendKeepAliveCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.suspendKeepAlive = suspendKeepAliveCheckBox.Checked;
				Project.mainCommand.trySetInternetAvailable(true);
			}
		}

		private void forceEmptyProxyCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.forceEmptyProxy = forceEmptyProxyCheckBox.Checked;
				Project.ApplyGlobalHTTPProxy(true);
				Project.mainCommand.trySetInternetAvailable(true);
			}
		}

		private void useProxyCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.useProxy = useProxyCheckBox.Checked;
				forceEmptyProxyCheckBox.Enabled = !Project.useProxy;
				Project.ApplyGlobalHTTPProxy(true);
				Project.mainCommand.trySetInternetAvailable(true);
			}
		}

		private void proxyServerTextBox_TextChanged(object sender, System.EventArgs e)
		{
			Project.proxyServer = proxyServerTextBox.Text;
			Project.ApplyGlobalHTTPProxy(false);
		}

		private void proxyPortTextBox_TextChanged(object sender, System.EventArgs e)
		{
			try 
			{
				Project.proxyPort = Convert.ToInt32(proxyPortTextBox.Text);
			} 
			catch
			{
			}
			if(proxyPortTextBox.Text.Length > 0)
			{
				proxyPortTextBox.Text = "" + Project.proxyPort;
				Project.ApplyGlobalHTTPProxy(false);
			}
		}

		private void distortionFactorTextBox_TextChanged(object sender, System.EventArgs e)
		{
		}

		private void distortionFactorTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				Project.distortionFactor = Convert.ToDouble(distortionFactorTextBox.Text);
			}
			catch {}
			PictureManager.This.CameraManager.ProcessCameraMove();	// will bring it within range 0.8 to 1.2
			distortionFactorTextBox.Text = "" + Project.distortionFactor;
			e.Cancel = false;
		}

		private void browseFolderButton_Click(object sender, System.EventArgs e)
		{
			IntPtr owner = this.Handle;
			clsFolderBrowser folderBrowser = new clsFolderBrowser(owner, "Select Mapping Cache Folder");

			string tmp = folderBrowser.BrowseForFolder(cacheFolderTextBox.Text);
			if(!"\\".Equals(tmp))		// cancel button causes "\" returned
			{
				TileCache.setMappingCacheLocation(tmp);
				cacheFolderTextBox.Text = Project.mapsPath;
				Project.AttemptTerrafolderMigration();
				m_pictureManager.CameraManager.ProcessCameraMove();
			}
		}

		private void resetMappingCacheLocationButton_Click(object sender, System.EventArgs e)
		{
			Project.resetMappingCacheLocation();
			TileCache.setMappingCacheLocation(Project.mapsPath);
			cacheFolderTextBox.Text = Project.mapsPath;
			Project.AttemptTerrafolderMigration();
			m_pictureManager.CameraManager.ProcessCameraMove();
		}

		private void supportFaqLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser("http://" + Project.PROGRAM_MAIN_SERVER + "/faq.html");
		}

		private void eqFillModeTimeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.earthquakeStyleFillHowRecentIndex = eqFillModeTimeComboBox.SelectedIndex;
			m_pictureManager.CameraManager.ProcessCameraMove();
		}

		private void eqFillOnlyRecentCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.earthquakeStyleFillRecent = eqFillOnlyRecentCheckBox.Checked;
			m_pictureManager.CameraManager.ProcessCameraMove();
		}

		private void useIconsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.useWaypointIcons = useIconsCheckBox.Checked;
			PictureManager.This.Refresh();
		}

		private void helpAtStartCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.showHelpAtStart = helpAtStartCheckBox.Checked;
		}

		private void mouseWheelActionRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(mouseWheelActionNoneRadioButton.Checked)
			{
				Project.mouseWheelAction = 0;
			}
			else if(mouseWheelActionZoomRadioButton.Checked)
			{
				Project.mouseWheelAction = 1;
			}
			else if(mouseWheelActionZoomRevRadioButton.Checked)
			{
				Project.mouseWheelAction = 2;
			}
			else if(mouseWheelActionPanRadioButton.Checked)
			{
				Project.mouseWheelAction = 3;
			}
		}

		private void moreOptionsLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DlgWaypointsManager newDlg = new DlgWaypointsManager(PictureManager.This, CameraManager.This, 2, -1);
			newDlg.TopMost = true;
			newDlg.Owner = Project.mainForm;
			newDlg.Show();
			this.Close();
		}

		private void kmlShowPopupCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.kmlOptions.ShowInPopup = kmlShowPopupCheckBox.Checked;
		}
	}
}
