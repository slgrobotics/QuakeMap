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
using System.Text;
using System.Xml;
using System.Reflection;
using System.Data;
using System.Threading;

using ICSharpCode.SharpZipLib.Zip;
using CookComputing.XmlRpc;

using LibSys;
using LibGeo;
using LibGui;
using LibFormats;
using XmlRpcSmugmugLib;

//
// see http://www.codeproject.com/cs/media/photoproperties.asp for Photo Properties Library
//

namespace WindowsTiler
{
	// returns number of photo files (jpg files) actually processed (usually returns 0 or 1). 
	// if zip != null, containerFileName is ignored - otherwise it is likely .gpx
	// sourceFileName is something like thumbnail URL or filename of the image.
    public delegate int ProcessFile(ZipFile zip, ZipEntry zipEntry, string containerFileName, string sourceFileName);

	/// <summary>
	/// DlgPhotoManager organizes photo collections and links the JPEG/EXIFFs to the trackpoints.
	/// </summary>
	public sealed class DlgPhotoManager : System.Windows.Forms.Form
	{
		#region Variables defined here

		public static DlgPhotoManager This = null;
		// controls that don't fit in Designer:
		private LibSys.PhotoViewerControl photoViewerControl;
		private LibSys.MyFileOrFolder myFileOrFolder;

		private static bool m_keepInView = true;
		private static bool m_doZoomIn = true;
		private int m_mode;
		private static int m_photoTimeShiftTypeSelected = 0;	// camera by default; what we selected in the drop-down.
		private int m_tzId;

		private DataSet m_trackpointsDS = null;
		private DataGridTableStyle m_trackpointsTS = null;
		private bool rebuildingTrackpoints = false;
		private bool deletingTrackpoints = false;

		private DataSet m_unrelatedDS = null;
		private DataGridTableStyle m_unrelatedTS = null;
		private bool rebuildingUnrelated = false;
		private bool deletingUnrelated = false;

		private Color origButtonBackColor;

		private static SortableDateTime sortableDateTime = new SortableDateTime();	// for GetType()

		private const string hintText = @"Your digital camera clock is likely not as good as GPS atomic clock.
Even more, you probably forgot to set it when you took those photos.
And the clock was set in a different timezone anyway.
No problem, you can fix it all here.";

		/*
		/// <summary>Define the InitializePhotoPropertiesDelegate delegate signature.</summary>
		private delegate bool InitializePhotoPropertiesDelegate(string xmlFileName);
		/// <summary>An instance of the PhotoProperties class</summary>
		private PhotoProperties _photoProps;
		/// <summary>Has the PhotoProperties instance successfully initialized?</summary>
		private bool _photoPropsInitialized = false;
		/// <summary>An instance of the ResultOptions class defining 
		/// properties used when creating XML output.</summary>
		private ResultOptions _resultOptions;
		/// <summary>Was the current analysis successful?</summary>
		private bool _isAnalysisSuccessful = false;
		*/

		#endregion // Variables defined here

		#region Controls defined here

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.LinkLabel logLinkLabel;
		private System.Windows.Forms.ComboBox timeSignComboBox;
		private System.Windows.Forms.ComboBox timeHoursComboBox;
		private System.Windows.Forms.NumericUpDown timeMinutesNumericUpDown;
		private System.Windows.Forms.NumericUpDown timeSecondsNumericUpDown;
		private System.Windows.Forms.CheckBox previewCheckBox;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Label waypointLabel;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage associateTabPage;
		private System.Windows.Forms.TabPage optionsTabPage;
		private System.Windows.Forms.NumericUpDown thumbWidthNumericUpDown;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown thumbHeightNumericUpDown;
		private System.Windows.Forms.CheckBox thumbDisplayCheckBox;
		private System.Windows.Forms.TabPage trackpointsTabPage;
		private System.Windows.Forms.Button close1Button;
		private System.Windows.Forms.Button close2Button;
		private System.Windows.Forms.CheckBox autoAnalyzeCheckBox;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.RadioButton topRightRadioButton;
		private System.Windows.Forms.RadioButton centerRadioButton;
		private System.Windows.Forms.Button thumbApplyBbutton;
		private System.Windows.Forms.Button rewindButton;
		private System.Windows.Forms.Button ffwdButton;
		private System.Windows.Forms.Button backButton;
		private System.Windows.Forms.Button fwdButton;
		private System.Windows.Forms.Button fullSizeButton;
		private System.Windows.Forms.CheckBox keepInViewCheckBox;
		private System.Windows.Forms.Label labelSeconds;
		private System.Windows.Forms.Label labelMinutes;
		private System.Windows.Forms.Label labelHours;
		private System.Windows.Forms.CheckBox timeShiftEnabledCheckBox;
		private System.Windows.Forms.CheckBox zoomInCheckBox;
		private System.Windows.Forms.DataGrid trackpointsDataGrid;
		private System.Windows.Forms.Label messageTrkptLabel;
		private System.Windows.Forms.TabPage unrelatedTabPage;
		private System.Windows.Forms.Button close3Button;
		private System.Windows.Forms.DataGrid unrelatedDataGrid;
		private System.Windows.Forms.Label messageUnrelatedLabel;
		private System.Windows.Forms.Button clearUnrelatedButton;
		private System.Windows.Forms.Button zoomCloseButton;
		private System.Windows.Forms.Button zoomOutButton;
		private System.Windows.Forms.Panel fileOrFolderPanel;
		private System.Windows.Forms.GroupBox timeShiftGroupBox;
		private System.Windows.Forms.Button saveTimeShiftButton;
		private System.Windows.Forms.Button reprocessFileOrFolderButton;
		private System.Windows.Forms.Label traceLabel;
		private System.Windows.Forms.ComboBox shiftTypeComboBox;
		private System.Windows.Forms.Button setTimeShiftButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox autoAnalyzeReminderCheckBox;
		private System.Windows.Forms.Panel photoViewerPanel;
		private System.Windows.Forms.Button exportButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox pictureSizeComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel associatePanel;
		private System.Windows.Forms.Panel photoTrackpointsPanel;
		private System.Windows.Forms.Panel unrelatedTrackpointsPanel;
		private System.Windows.Forms.Panel optionsPanel;
		private System.Windows.Forms.TabPage cameraTabPage;
		private System.Windows.Forms.Panel cameraPanel;
		private System.Windows.Forms.Button close4Button;
		private System.Windows.Forms.Label cameraShiftLabel;
		private System.Windows.Forms.LinkLabel adjustCameraShiftLinkLabel;
		private System.Windows.Forms.LinkLabel useLinkLabel;
		private System.Windows.Forms.Label cameraShiftLabel2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox timeZoneComboBox;
		private System.Windows.Forms.Label hintLabel;
		private System.Windows.Forms.LinkLabel helpExportLinkLabel;
		private System.Windows.Forms.Panel cameraShiftPanel;
		private System.Windows.Forms.GroupBox pictureGroupBox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox photoParametersAskCheckBox;
		private System.Windows.Forms.CheckBox photoParametersDonotwriteCheckBox;
		private System.Windows.Forms.CheckBox photoPositionsDonotwriteCheckBox;
		private System.Windows.Forms.CheckBox photoPositionsAskCheckBox;
		private System.Windows.Forms.Button clearButton;
		private System.Windows.Forms.LinkLabel adjustByPhotoLinkLabel;

		#endregion // Controls defined here

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Constructor and Lifecycle

		public DlgPhotoManager(int mode)	// 0 - asooc.tab,  1 - trackpoints tab,  2 - unrelated tab,   3 - options tab
		{
			if(This != null) 
			{
				This.Dispose();
			}

			This = this;
			m_mode = mode;

			Project.photoModeReprocess = false;
			Project.photoSelectPhotoMode = 0;

			DlgPhotoFullSize.DisposeIfUp();

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			setTimeShiftButton.Enabled = false;		// note: also serves as a flag
			origButtonBackColor = saveTimeShiftButton.BackColor;

			this.SuspendLayout();

			hintLabel.Text = hintText;

			this.timeZoneComboBox.Items.AddRange(MyTimeZone.TimeZones);

			// making photoViewerControl as dumb VS Designer removes it:
			this.photoViewerControl = new LibSys.PhotoViewerControl();
			this.photoViewerPanel.Controls.AddRange(new System.Windows.Forms.Control[] { this.photoViewerControl });
			this.photoViewerControl.Dock = DockStyle.Fill;
			this.photoViewerControl.Cursor = System.Windows.Forms.Cursors.Hand;
			//this.photoViewerControl.Location = new System.Drawing.Point(336, 176);
			this.photoViewerControl.Name = "photoViewerControl";
			this.photoViewerControl.photoDescr = null;
			//this.photoViewerControl.Size = new System.Drawing.Size(224, 184);
			this.photoViewerControl.TabIndex = 112;
			this.photoViewerControl.Click += new System.EventHandler(this.photoViewerControl_Click);

			// making myFileOrFolder control as dumb VS Designer removes it:
			this.myFileOrFolder = new LibSys.MyFileOrFolder();
			this.myFileOrFolder.Dock = DockStyle.Fill;
			this.fileOrFolderPanel.Controls.Add(this.myFileOrFolder);
			this.myFileOrFolder.Name = "myFileOrFolder";
			this.myFileOrFolder.browseFileButton.Click += new System.EventHandler(this.browseFileButton_Click);
			this.myFileOrFolder.browseFolderButton.Click += new System.EventHandler(this.browseFolderButton_Click);
			this.myFileOrFolder.verifyGalleryButton.Click += new System.EventHandler(this.verifyGalleryButton_Click);
			this.myFileOrFolder.importFileButton.Click += new System.EventHandler(this.importFileButton_Click);
			this.myFileOrFolder.importFolderButton.Click += new System.EventHandler(this.importFolderButton_Click);
			this.myFileOrFolder.importGalleryButton.Click += new System.EventHandler(this.importGalleryButton_Click);
			this.myFileOrFolder.tabControl1.SelectedIndexChanged += new System.EventHandler(this.myFileOrFolder_ModeChanged);

			configureReprocessButton();		// "Reprocess File" or "Reprocess Folder"

			this.ResumeLayout(true);

			// whatever is needed to initialize checkboxes etc:
			keepInViewCheckBox.Checked = m_keepInView;
			zoomInCheckBox.Checked = m_doZoomIn;
			
			configureTimeshiftChoices(false, true);
			shiftTypeComboBox.SelectedIndex = m_photoTimeShiftTypeSelected;

			if(Project.photoGalleryPictureSize >= Project.photoGalleryPictureSizes.Length)
			{
				Project.photoGalleryPictureSize = 2;
			}
			pictureSizeComboBox.SelectedIndex = Project.photoGalleryPictureSize;

			this.trackpointsDataGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackpointsDataGrid_MouseUp);

			this.unrelatedDataGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.unrelatedDataGrid_MouseUp);

			Project.setDlgIcon(this);
		}

		private bool hasLoaded = false;

		private void DlgPhotoManager_Load(object sender, System.EventArgs e)
		{
			if(Project.photoFileName.Length == 0)
			{
				Project.photoFileName = Project.photoFolderPath;
			}
			setFileText(Project.photoFileName);
			setRootText(Project.photoFolderPath);
			setGalleryText(Project.photoGalleryPath);
			displayTimeShiftControlValues();
			autoAnalyzeCheckBox.Checked = Project.photoAnalyzeOnLoad;
			autoAnalyzeReminderCheckBox.Checked = Project.photoAnalyzeOnLoadReminder;
			previewCheckBox.Checked = Project.photoDoPreview;
			
			thumbDisplayCheckBox.Checked = Project.thumbDoDisplay;
			thumbWidthNumericUpDown.Value = Project.thumbWidth;
			thumbHeightNumericUpDown.Value = Project.thumbHeight;
			switch(Project.thumbPosition)
			{
				case 0:
					this.topRightRadioButton.Checked = true;
					break;
				case 1:
					this.centerRadioButton.Checked = true;
					break;
			}

			showTimeSiftControls(timeShiftEnabledCheckBox.Checked);

			this.photoViewerControl.fitToSize = true;	// also performs layout

			Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
			preview(wpt, false);

			switch (m_mode)
			{
				case 0:
					break;
				case 1:
					tabControl.SelectedTab = this.trackpointsTabPage;
					break;
				case 2:
					tabControl.SelectedTab = this.unrelatedTabPage;
					break;
				case 3:
					tabControl.SelectedTab = this.optionsTabPage;
					break;
				case 4:
					tabControl.SelectedTab = this.cameraTabPage;
					break;
			}

			setTimeShiftButton.Enabled = true;		// note: also serves as a flag

			this.timeZoneComboBox.SelectedIndexChanged += new System.EventHandler(this.timeZoneComboBox_SelectedIndexChanged); 

			if(Project.fitsScreen(Project.photoManagerX, Project.photoManagerY, Project.photoManagerWidth, Project.photoManagerHeight))
			{
				inResize = true;
				this.Location = new Point(Project.photoManagerX, Project.photoManagerY);
				this.ClientSize = new System.Drawing.Size(Project.photoManagerWidth, Project.photoManagerHeight);		// causes Resize()
			}

			this.Resize += new System.EventHandler(this.DlgPhotoManager_Resize);
			this.Move += new System.EventHandler(this.DlgPhotoManager_Move);

			photoParametersAskCheckBox.Checked = Project.photoParametersAsk;
			photoParametersDonotwriteCheckBox.Checked = Project.photoParametersDonotwrite;
			photoPositionsAskCheckBox.Checked = Project.photoPositionsAsk;
			photoPositionsDonotwriteCheckBox.Checked = Project.photoPositionsDonotwrite;
			photoParametersAskCheckBox.Enabled = !photoParametersDonotwriteCheckBox.Checked;
			photoPositionsAskCheckBox.Enabled = !photoPositionsDonotwriteCheckBox.Checked;

			hasLoaded = true;
			inResize = false;

			if(m_mode == 4)
			{
				adjustTimeShiftByPhotopoint();
				this.timeShiftEnabledCheckBox.Checked = true;
			}
		}

		private void refreshPicture()
		{
			if(hasLoaded)
			{
				PictureManager.This.Refresh();
			}
		}

		private void DlgPhotoManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Project.photoModeReprocess = false;
			This = null;
		}

		bool inResize = false;

		private void DlgPhotoManager_Resize(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private void DlgPhotoManager_Move(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private void memorizeSizeAndPosition()
		{
			bool formMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
			if(!formMaximized && !inResize)
			{
				Project.photoManagerWidth = this.ClientSize.Width;
				Project.photoManagerHeight = this.ClientSize.Height;
				Project.photoManagerX = this.Location.X;
				Project.photoManagerY = this.Location.Y;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			This = null;
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			photoViewerControl.Dispose();
			base.Dispose( disposing );
		}

		private void DlgPhotoManager_Activated(object sender, System.EventArgs e)
		{
			try
			{
				Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
				preview(wpt, false);
			}
			catch {}
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cancelButton = new System.Windows.Forms.Button();
			this.logLinkLabel = new System.Windows.Forms.LinkLabel();
			this.timeShiftGroupBox = new System.Windows.Forms.GroupBox();
			this.cameraShiftPanel = new System.Windows.Forms.Panel();
			this.adjustByPhotoLinkLabel = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.timeSecondsNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.timeHoursComboBox = new System.Windows.Forms.ComboBox();
			this.timeSignComboBox = new System.Windows.Forms.ComboBox();
			this.labelMinutes = new System.Windows.Forms.Label();
			this.saveTimeShiftButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.labelHours = new System.Windows.Forms.Label();
			this.labelSeconds = new System.Windows.Forms.Label();
			this.timeZoneComboBox = new System.Windows.Forms.ComboBox();
			this.timeMinutesNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.setTimeShiftButton = new System.Windows.Forms.Button();
			this.reprocessFileOrFolderButton = new System.Windows.Forms.Button();
			this.shiftTypeComboBox = new System.Windows.Forms.ComboBox();
			this.timeShiftEnabledCheckBox = new System.Windows.Forms.CheckBox();
			this.previewCheckBox = new System.Windows.Forms.CheckBox();
			this.messageLabel = new System.Windows.Forms.Label();
			this.waypointLabel = new System.Windows.Forms.Label();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.associateTabPage = new System.Windows.Forms.TabPage();
			this.associatePanel = new System.Windows.Forms.Panel();
			this.pictureGroupBox = new System.Windows.Forms.GroupBox();
			this.photoViewerPanel = new System.Windows.Forms.Panel();
			this.adjustCameraShiftLinkLabel = new System.Windows.Forms.LinkLabel();
			this.cameraShiftLabel = new System.Windows.Forms.Label();
			this.fileOrFolderPanel = new System.Windows.Forms.Panel();
			this.zoomCloseButton = new System.Windows.Forms.Button();
			this.fwdButton = new System.Windows.Forms.Button();
			this.keepInViewCheckBox = new System.Windows.Forms.CheckBox();
			this.fullSizeButton = new System.Windows.Forms.Button();
			this.backButton = new System.Windows.Forms.Button();
			this.rewindButton = new System.Windows.Forms.Button();
			this.ffwdButton = new System.Windows.Forms.Button();
			this.zoomInCheckBox = new System.Windows.Forms.CheckBox();
			this.zoomOutButton = new System.Windows.Forms.Button();
			this.trackpointsTabPage = new System.Windows.Forms.TabPage();
			this.trackpointsDataGrid = new System.Windows.Forms.DataGrid();
			this.photoTrackpointsPanel = new System.Windows.Forms.Panel();
			this.clearButton = new System.Windows.Forms.Button();
			this.exportButton = new System.Windows.Forms.Button();
			this.messageTrkptLabel = new System.Windows.Forms.Label();
			this.close1Button = new System.Windows.Forms.Button();
			this.unrelatedTabPage = new System.Windows.Forms.TabPage();
			this.unrelatedDataGrid = new System.Windows.Forms.DataGrid();
			this.unrelatedTrackpointsPanel = new System.Windows.Forms.Panel();
			this.clearUnrelatedButton = new System.Windows.Forms.Button();
			this.messageUnrelatedLabel = new System.Windows.Forms.Label();
			this.close3Button = new System.Windows.Forms.Button();
			this.optionsTabPage = new System.Windows.Forms.TabPage();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.photoPositionsDonotwriteCheckBox = new System.Windows.Forms.CheckBox();
			this.photoPositionsAskCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.photoParametersDonotwriteCheckBox = new System.Windows.Forms.CheckBox();
			this.photoParametersAskCheckBox = new System.Windows.Forms.CheckBox();
			this.autoAnalyzeCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.pictureSizeComboBox = new System.Windows.Forms.ComboBox();
			this.autoAnalyzeReminderCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.thumbApplyBbutton = new System.Windows.Forms.Button();
			this.thumbDisplayCheckBox = new System.Windows.Forms.CheckBox();
			this.label9 = new System.Windows.Forms.Label();
			this.thumbHeightNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.thumbWidthNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.centerRadioButton = new System.Windows.Forms.RadioButton();
			this.topRightRadioButton = new System.Windows.Forms.RadioButton();
			this.close2Button = new System.Windows.Forms.Button();
			this.cameraTabPage = new System.Windows.Forms.TabPage();
			this.cameraPanel = new System.Windows.Forms.Panel();
			this.traceLabel = new System.Windows.Forms.Label();
			this.helpExportLinkLabel = new System.Windows.Forms.LinkLabel();
			this.hintLabel = new System.Windows.Forms.Label();
			this.cameraShiftLabel2 = new System.Windows.Forms.Label();
			this.useLinkLabel = new System.Windows.Forms.LinkLabel();
			this.close4Button = new System.Windows.Forms.Button();
			this.timeShiftGroupBox.SuspendLayout();
			this.cameraShiftPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.timeSecondsNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.timeMinutesNumericUpDown)).BeginInit();
			this.tabControl.SuspendLayout();
			this.associateTabPage.SuspendLayout();
			this.associatePanel.SuspendLayout();
			this.pictureGroupBox.SuspendLayout();
			this.trackpointsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackpointsDataGrid)).BeginInit();
			this.photoTrackpointsPanel.SuspendLayout();
			this.unrelatedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.unrelatedDataGrid)).BeginInit();
			this.unrelatedTrackpointsPanel.SuspendLayout();
			this.optionsTabPage.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.thumbHeightNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.thumbWidthNumericUpDown)).BeginInit();
			this.groupBox5.SuspendLayout();
			this.cameraTabPage.SuspendLayout();
			this.cameraPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(694, 386);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(71, 25);
			this.cancelButton.TabIndex = 97;
			this.cancelButton.Text = "Close";
			// 
			// logLinkLabel
			// 
			this.logLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.logLinkLabel.Location = new System.Drawing.Point(695, 330);
			this.logLinkLabel.Name = "logLinkLabel";
			this.logLinkLabel.Size = new System.Drawing.Size(70, 27);
			this.logLinkLabel.TabIndex = 100;
			this.logLinkLabel.TabStop = true;
			this.logLinkLabel.Text = "View Log";
			this.logLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.logLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.logLinkLabel_LinkClicked);
			// 
			// timeShiftGroupBox
			// 
			this.timeShiftGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.timeShiftGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.cameraShiftPanel,
																							this.shiftTypeComboBox,
																							this.timeShiftEnabledCheckBox});
			this.timeShiftGroupBox.Location = new System.Drawing.Point(5, 155);
			this.timeShiftGroupBox.Name = "timeShiftGroupBox";
			this.timeShiftGroupBox.Size = new System.Drawing.Size(771, 125);
			this.timeShiftGroupBox.TabIndex = 101;
			this.timeShiftGroupBox.TabStop = false;
			this.timeShiftGroupBox.Text = "Compensate for the time shift between your GPS and Digital Camera";
			// 
			// cameraShiftPanel
			// 
			this.cameraShiftPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.cameraShiftPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.adjustByPhotoLinkLabel,
																						   this.label1,
																						   this.timeSecondsNumericUpDown,
																						   this.timeHoursComboBox,
																						   this.timeSignComboBox,
																						   this.labelMinutes,
																						   this.saveTimeShiftButton,
																						   this.label3,
																						   this.labelHours,
																						   this.labelSeconds,
																						   this.timeZoneComboBox,
																						   this.timeMinutesNumericUpDown,
																						   this.setTimeShiftButton,
																						   this.reprocessFileOrFolderButton});
			this.cameraShiftPanel.Location = new System.Drawing.Point(5, 50);
			this.cameraShiftPanel.Name = "cameraShiftPanel";
			this.cameraShiftPanel.Size = new System.Drawing.Size(760, 70);
			this.cameraShiftPanel.TabIndex = 19;
			// 
			// adjustByPhotoLinkLabel
			// 
			this.adjustByPhotoLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.adjustByPhotoLinkLabel.Location = new System.Drawing.Point(315, 5);
			this.adjustByPhotoLinkLabel.Name = "adjustByPhotoLinkLabel";
			this.adjustByPhotoLinkLabel.Size = new System.Drawing.Size(235, 32);
			this.adjustByPhotoLinkLabel.TabIndex = 127;
			this.adjustByPhotoLinkLabel.TabStop = true;
			this.adjustByPhotoLinkLabel.Text = "<- adjust by photo";
			this.adjustByPhotoLinkLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.adjustByPhotoLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.adjustByPhotoLinkLabel_LinkClicked);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(5, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 18);
			this.label1.TabIndex = 16;
			this.label1.Text = "Time Shift:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// timeSecondsNumericUpDown
			// 
			this.timeSecondsNumericUpDown.Location = new System.Drawing.Point(260, 20);
			this.timeSecondsNumericUpDown.Maximum = new System.Decimal(new int[] {
																					 59,
																					 0,
																					 0,
																					 0});
			this.timeSecondsNumericUpDown.Name = "timeSecondsNumericUpDown";
			this.timeSecondsNumericUpDown.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.timeSecondsNumericUpDown.Size = new System.Drawing.Size(47, 20);
			this.timeSecondsNumericUpDown.TabIndex = 7;
			this.timeSecondsNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.timeSecondsNumericUpDown.TextChanged += new System.EventHandler(this.timeSecondsNumericUpDown_TextChanged);
			this.timeSecondsNumericUpDown.Leave += new System.EventHandler(this.timeSecondsNumericUpDown_Leave);
			// 
			// timeHoursComboBox
			// 
			this.timeHoursComboBox.Items.AddRange(new object[] {
																   "0",
																   "1",
																   "2",
																   "3",
																   "4",
																   "5",
																   "6",
																   "7",
																   "8",
																   "9",
																   "10",
																   "11",
																   "12",
																   "13",
																   "14",
																   "15",
																   "16",
																   "17",
																   "18",
																   "19",
																   "20",
																   "21",
																   "22",
																   "23",
																   "24"});
			this.timeHoursComboBox.Location = new System.Drawing.Point(140, 20);
			this.timeHoursComboBox.Name = "timeHoursComboBox";
			this.timeHoursComboBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.timeHoursComboBox.Size = new System.Drawing.Size(57, 21);
			this.timeHoursComboBox.TabIndex = 5;
			this.timeHoursComboBox.Text = "0";
			this.timeHoursComboBox.TextChanged += new System.EventHandler(this.timeHoursComboBox_TextChanged);
			// 
			// timeSignComboBox
			// 
			this.timeSignComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.timeSignComboBox.Font = new System.Drawing.Font("Arial Black", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.timeSignComboBox.Items.AddRange(new object[] {
																  "+",
																  "-"});
			this.timeSignComboBox.Location = new System.Drawing.Point(85, 19);
			this.timeSignComboBox.Name = "timeSignComboBox";
			this.timeSignComboBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.timeSignComboBox.Size = new System.Drawing.Size(46, 23);
			this.timeSignComboBox.TabIndex = 4;
			this.timeSignComboBox.TextChanged += new System.EventHandler(this.timeSignComboBox_TextChanged);
			// 
			// labelMinutes
			// 
			this.labelMinutes.Location = new System.Drawing.Point(195, 0);
			this.labelMinutes.Name = "labelMinutes";
			this.labelMinutes.Size = new System.Drawing.Size(56, 18);
			this.labelMinutes.TabIndex = 9;
			this.labelMinutes.Text = "Minutes";
			this.labelMinutes.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// saveTimeShiftButton
			// 
			this.saveTimeShiftButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.saveTimeShiftButton.Location = new System.Drawing.Point(600, 43);
			this.saveTimeShiftButton.Name = "saveTimeShiftButton";
			this.saveTimeShiftButton.Size = new System.Drawing.Size(155, 24);
			this.saveTimeShiftButton.TabIndex = 13;
			this.saveTimeShiftButton.Text = "Save to File/Folder...";
			this.saveTimeShiftButton.Click += new System.EventHandler(this.saveTimeShiftButton_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(5, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(75, 18);
			this.label3.TabIndex = 18;
			this.label3.Text = "Time Zone:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelHours
			// 
			this.labelHours.Location = new System.Drawing.Point(140, 0);
			this.labelHours.Name = "labelHours";
			this.labelHours.Size = new System.Drawing.Size(47, 18);
			this.labelHours.TabIndex = 8;
			this.labelHours.Text = "Hours";
			this.labelHours.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// labelSeconds
			// 
			this.labelSeconds.Location = new System.Drawing.Point(255, 0);
			this.labelSeconds.Name = "labelSeconds";
			this.labelSeconds.Size = new System.Drawing.Size(56, 18);
			this.labelSeconds.TabIndex = 10;
			this.labelSeconds.Text = "Seconds";
			this.labelSeconds.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// timeZoneComboBox
			// 
			this.timeZoneComboBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.timeZoneComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.timeZoneComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.timeZoneComboBox.Location = new System.Drawing.Point(85, 46);
			this.timeZoneComboBox.Name = "timeZoneComboBox";
			this.timeZoneComboBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.timeZoneComboBox.Size = new System.Drawing.Size(460, 21);
			this.timeZoneComboBox.TabIndex = 17;
			// 
			// timeMinutesNumericUpDown
			// 
			this.timeMinutesNumericUpDown.Location = new System.Drawing.Point(205, 20);
			this.timeMinutesNumericUpDown.Maximum = new System.Decimal(new int[] {
																					 59,
																					 0,
																					 0,
																					 0});
			this.timeMinutesNumericUpDown.Name = "timeMinutesNumericUpDown";
			this.timeMinutesNumericUpDown.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.timeMinutesNumericUpDown.Size = new System.Drawing.Size(47, 20);
			this.timeMinutesNumericUpDown.TabIndex = 6;
			this.timeMinutesNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.timeMinutesNumericUpDown.TextChanged += new System.EventHandler(this.timeMinutesNumericUpDown_TextChanged);
			this.timeMinutesNumericUpDown.Leave += new System.EventHandler(this.timeMinutesNumericUpDown_Leave);
			// 
			// setTimeShiftButton
			// 
			this.setTimeShiftButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.setTimeShiftButton.Location = new System.Drawing.Point(555, 43);
			this.setTimeShiftButton.Name = "setTimeShiftButton";
			this.setTimeShiftButton.Size = new System.Drawing.Size(38, 24);
			this.setTimeShiftButton.TabIndex = 14;
			this.setTimeShiftButton.Text = "Set";
			this.setTimeShiftButton.Click += new System.EventHandler(this.setTimeShiftButton_Click);
			// 
			// reprocessFileOrFolderButton
			// 
			this.reprocessFileOrFolderButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.reprocessFileOrFolderButton.Location = new System.Drawing.Point(555, 10);
			this.reprocessFileOrFolderButton.Name = "reprocessFileOrFolderButton";
			this.reprocessFileOrFolderButton.Size = new System.Drawing.Size(200, 24);
			this.reprocessFileOrFolderButton.TabIndex = 11;
			this.reprocessFileOrFolderButton.Text = "Reprocess Folder";
			this.reprocessFileOrFolderButton.Click += new System.EventHandler(this.reprocessFileOrFolderButton_Click);
			// 
			// shiftTypeComboBox
			// 
			this.shiftTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.shiftTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.shiftTypeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.shiftTypeComboBox.Location = new System.Drawing.Point(350, 25);
			this.shiftTypeComboBox.Name = "shiftTypeComboBox";
			this.shiftTypeComboBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.shiftTypeComboBox.Size = new System.Drawing.Size(410, 21);
			this.shiftTypeComboBox.TabIndex = 15;
			this.shiftTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.shiftTypeComboBox_SelectedIndexChanged);
			// 
			// timeShiftEnabledCheckBox
			// 
			this.timeShiftEnabledCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.timeShiftEnabledCheckBox.Location = new System.Drawing.Point(9, 25);
			this.timeShiftEnabledCheckBox.Name = "timeShiftEnabledCheckBox";
			this.timeShiftEnabledCheckBox.Size = new System.Drawing.Size(341, 20);
			this.timeShiftEnabledCheckBox.TabIndex = 12;
			this.timeShiftEnabledCheckBox.Text = "use values below to relate photos to track as --->";
			this.timeShiftEnabledCheckBox.CheckedChanged += new System.EventHandler(this.timeShiftEnabledCheckBox_CheckedChanged);
			// 
			// previewCheckBox
			// 
			this.previewCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.previewCheckBox.Location = new System.Drawing.Point(671, 185);
			this.previewCheckBox.Name = "previewCheckBox";
			this.previewCheckBox.Size = new System.Drawing.Size(84, 18);
			this.previewCheckBox.TabIndex = 103;
			this.previewCheckBox.Text = "preview";
			this.previewCheckBox.CheckedChanged += new System.EventHandler(this.previewCheckBox_CheckedChanged);
			// 
			// messageLabel
			// 
			this.messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.messageLabel.Location = new System.Drawing.Point(10, 250);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(650, 20);
			this.messageLabel.TabIndex = 104;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// waypointLabel
			// 
			this.waypointLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left);
			this.waypointLabel.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.waypointLabel.Location = new System.Drawing.Point(10, 25);
			this.waypointLabel.Name = "waypointLabel";
			this.waypointLabel.Size = new System.Drawing.Size(280, 220);
			this.waypointLabel.TabIndex = 105;
			this.waypointLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabControl
			// 
			this.tabControl.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.associateTabPage,
																					 this.trackpointsTabPage,
																					 this.unrelatedTabPage,
																					 this.optionsTabPage,
																					 this.cameraTabPage});
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(2, 2);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(790, 445);
			this.tabControl.TabIndex = 106;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// associateTabPage
			// 
			this.associateTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.associatePanel});
			this.associateTabPage.Location = new System.Drawing.Point(4, 22);
			this.associateTabPage.Name = "associateTabPage";
			this.associateTabPage.Size = new System.Drawing.Size(782, 419);
			this.associateTabPage.TabIndex = 0;
			this.associateTabPage.Text = "Associate Photos with Trackpoints";
			// 
			// associatePanel
			// 
			this.associatePanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.pictureGroupBox,
																						 this.adjustCameraShiftLinkLabel,
																						 this.cameraShiftLabel,
																						 this.fileOrFolderPanel,
																						 this.zoomCloseButton,
																						 this.fwdButton,
																						 this.keepInViewCheckBox,
																						 this.fullSizeButton,
																						 this.logLinkLabel,
																						 this.backButton,
																						 this.rewindButton,
																						 this.ffwdButton,
																						 this.cancelButton,
																						 this.zoomInCheckBox,
																						 this.zoomOutButton,
																						 this.previewCheckBox});
			this.associatePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.associatePanel.Name = "associatePanel";
			this.associatePanel.Size = new System.Drawing.Size(782, 419);
			this.associatePanel.TabIndex = 124;
			// 
			// pictureGroupBox
			// 
			this.pictureGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.pictureGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.waypointLabel,
																						  this.photoViewerPanel,
																						  this.messageLabel});
			this.pictureGroupBox.Location = new System.Drawing.Point(0, 145);
			this.pictureGroupBox.Name = "pictureGroupBox";
			this.pictureGroupBox.Size = new System.Drawing.Size(665, 275);
			this.pictureGroupBox.TabIndex = 126;
			this.pictureGroupBox.TabStop = false;
			this.pictureGroupBox.Text = "Current Picture";
			// 
			// photoViewerPanel
			// 
			this.photoViewerPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.photoViewerPanel.Location = new System.Drawing.Point(295, 10);
			this.photoViewerPanel.Name = "photoViewerPanel";
			this.photoViewerPanel.Size = new System.Drawing.Size(365, 235);
			this.photoViewerPanel.TabIndex = 123;
			// 
			// adjustCameraShiftLinkLabel
			// 
			this.adjustCameraShiftLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.adjustCameraShiftLinkLabel.Location = new System.Drawing.Point(695, 100);
			this.adjustCameraShiftLinkLabel.Name = "adjustCameraShiftLinkLabel";
			this.adjustCameraShiftLinkLabel.Size = new System.Drawing.Size(75, 20);
			this.adjustCameraShiftLinkLabel.TabIndex = 125;
			this.adjustCameraShiftLinkLabel.TabStop = true;
			this.adjustCameraShiftLinkLabel.Text = "<-- change";
			this.adjustCameraShiftLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.adjustCameraShiftLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.adjustCameraShiftLinkLabel_LinkClicked);
			// 
			// cameraShiftLabel
			// 
			this.cameraShiftLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.cameraShiftLabel.Location = new System.Drawing.Point(10, 105);
			this.cameraShiftLabel.Name = "cameraShiftLabel";
			this.cameraShiftLabel.Size = new System.Drawing.Size(680, 40);
			this.cameraShiftLabel.TabIndex = 124;
			this.cameraShiftLabel.Text = "Camera Time Shift:";
			this.cameraShiftLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// fileOrFolderPanel
			// 
			this.fileOrFolderPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.fileOrFolderPanel.Location = new System.Drawing.Point(5, 5);
			this.fileOrFolderPanel.Name = "fileOrFolderPanel";
			this.fileOrFolderPanel.Size = new System.Drawing.Size(771, 90);
			this.fileOrFolderPanel.TabIndex = 121;
			// 
			// zoomCloseButton
			// 
			this.zoomCloseButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.zoomCloseButton.Location = new System.Drawing.Point(671, 265);
			this.zoomCloseButton.Name = "zoomCloseButton";
			this.zoomCloseButton.Size = new System.Drawing.Size(84, 23);
			this.zoomCloseButton.TabIndex = 118;
			this.zoomCloseButton.Text = "1m/pix";
			this.zoomCloseButton.Click += new System.EventHandler(this.zoomCloseButton_Click);
			// 
			// fwdButton
			// 
			this.fwdButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.fwdButton.Location = new System.Drawing.Point(731, 245);
			this.fwdButton.Name = "fwdButton";
			this.fwdButton.Size = new System.Drawing.Size(19, 19);
			this.fwdButton.TabIndex = 110;
			this.fwdButton.Text = ">";
			this.fwdButton.Click += new System.EventHandler(this.fwdButton_Click);
			// 
			// keepInViewCheckBox
			// 
			this.keepInViewCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.keepInViewCheckBox.Location = new System.Drawing.Point(671, 170);
			this.keepInViewCheckBox.Name = "keepInViewCheckBox";
			this.keepInViewCheckBox.Size = new System.Drawing.Size(104, 18);
			this.keepInViewCheckBox.TabIndex = 116;
			this.keepInViewCheckBox.Text = "view on map";
			this.keepInViewCheckBox.CheckedChanged += new System.EventHandler(this.keepInViewCheckBox_CheckedChanged);
			// 
			// fullSizeButton
			// 
			this.fullSizeButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.fullSizeButton.Location = new System.Drawing.Point(671, 205);
			this.fullSizeButton.Name = "fullSizeButton";
			this.fullSizeButton.Size = new System.Drawing.Size(105, 24);
			this.fullSizeButton.TabIndex = 111;
			this.fullSizeButton.Text = "Full Size";
			this.fullSizeButton.Click += new System.EventHandler(this.fullSizeButton_Click);
			// 
			// backButton
			// 
			this.backButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.backButton.Location = new System.Drawing.Point(703, 245);
			this.backButton.Name = "backButton";
			this.backButton.Size = new System.Drawing.Size(19, 19);
			this.backButton.TabIndex = 109;
			this.backButton.Text = "<";
			this.backButton.Click += new System.EventHandler(this.backButton_Click);
			// 
			// rewindButton
			// 
			this.rewindButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.rewindButton.Location = new System.Drawing.Point(671, 245);
			this.rewindButton.Name = "rewindButton";
			this.rewindButton.Size = new System.Drawing.Size(29, 19);
			this.rewindButton.TabIndex = 107;
			this.rewindButton.Text = "|<";
			this.rewindButton.Click += new System.EventHandler(this.rewindButton_Click);
			// 
			// ffwdButton
			// 
			this.ffwdButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.ffwdButton.Location = new System.Drawing.Point(749, 245);
			this.ffwdButton.Name = "ffwdButton";
			this.ffwdButton.Size = new System.Drawing.Size(28, 19);
			this.ffwdButton.TabIndex = 108;
			this.ffwdButton.Text = ">|";
			this.ffwdButton.Click += new System.EventHandler(this.ffwdButton_Click);
			// 
			// zoomInCheckBox
			// 
			this.zoomInCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.zoomInCheckBox.Location = new System.Drawing.Point(671, 155);
			this.zoomInCheckBox.Name = "zoomInCheckBox";
			this.zoomInCheckBox.Size = new System.Drawing.Size(104, 19);
			this.zoomInCheckBox.TabIndex = 117;
			this.zoomInCheckBox.Text = "zoom in";
			this.zoomInCheckBox.CheckedChanged += new System.EventHandler(this.zoomInCheckBox_CheckedChanged);
			// 
			// zoomOutButton
			// 
			this.zoomOutButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.zoomOutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomOutButton.Location = new System.Drawing.Point(758, 265);
			this.zoomOutButton.Name = "zoomOutButton";
			this.zoomOutButton.Size = new System.Drawing.Size(19, 23);
			this.zoomOutButton.TabIndex = 119;
			this.zoomOutButton.Text = "^";
			this.zoomOutButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zoomOutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// trackpointsTabPage
			// 
			this.trackpointsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																							 this.trackpointsDataGrid,
																							 this.photoTrackpointsPanel});
			this.trackpointsTabPage.Location = new System.Drawing.Point(4, 22);
			this.trackpointsTabPage.Name = "trackpointsTabPage";
			this.trackpointsTabPage.Size = new System.Drawing.Size(782, 419);
			this.trackpointsTabPage.TabIndex = 2;
			this.trackpointsTabPage.Text = "Photo Trackpoints";
			// 
			// trackpointsDataGrid
			// 
			this.trackpointsDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.trackpointsDataGrid.DataMember = "";
			this.trackpointsDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.trackpointsDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.trackpointsDataGrid.Name = "trackpointsDataGrid";
			this.trackpointsDataGrid.Size = new System.Drawing.Size(782, 376);
			this.trackpointsDataGrid.TabIndex = 100;
			// 
			// photoTrackpointsPanel
			// 
			this.photoTrackpointsPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																								this.clearButton,
																								this.exportButton,
																								this.messageTrkptLabel,
																								this.close1Button});
			this.photoTrackpointsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.photoTrackpointsPanel.Location = new System.Drawing.Point(0, 376);
			this.photoTrackpointsPanel.Name = "photoTrackpointsPanel";
			this.photoTrackpointsPanel.Size = new System.Drawing.Size(782, 43);
			this.photoTrackpointsPanel.TabIndex = 99;
			// 
			// clearButton
			// 
			this.clearButton.Location = new System.Drawing.Point(10, 10);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(57, 26);
			this.clearButton.TabIndex = 107;
			this.clearButton.Text = "Clear";
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// exportButton
			// 
			this.exportButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.exportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.exportButton.Location = new System.Drawing.Point(603, 10);
			this.exportButton.Name = "exportButton";
			this.exportButton.Size = new System.Drawing.Size(79, 25);
			this.exportButton.TabIndex = 100;
			this.exportButton.Text = "Export";
			this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
			// 
			// messageTrkptLabel
			// 
			this.messageTrkptLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.messageTrkptLabel.Location = new System.Drawing.Point(75, 10);
			this.messageTrkptLabel.Name = "messageTrkptLabel";
			this.messageTrkptLabel.Size = new System.Drawing.Size(520, 27);
			this.messageTrkptLabel.TabIndex = 99;
			this.messageTrkptLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// close1Button
			// 
			this.close1Button.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.close1Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.close1Button.Location = new System.Drawing.Point(694, 10);
			this.close1Button.Name = "close1Button";
			this.close1Button.Size = new System.Drawing.Size(71, 26);
			this.close1Button.TabIndex = 98;
			this.close1Button.Text = "Close";
			this.close1Button.Click += new System.EventHandler(this.close1Button_Click);
			// 
			// unrelatedTabPage
			// 
			this.unrelatedTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.unrelatedDataGrid,
																						   this.unrelatedTrackpointsPanel});
			this.unrelatedTabPage.Location = new System.Drawing.Point(4, 22);
			this.unrelatedTabPage.Name = "unrelatedTabPage";
			this.unrelatedTabPage.Size = new System.Drawing.Size(782, 419);
			this.unrelatedTabPage.TabIndex = 3;
			this.unrelatedTabPage.Text = "Unrelated Photos";
			// 
			// unrelatedDataGrid
			// 
			this.unrelatedDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.unrelatedDataGrid.DataMember = "";
			this.unrelatedDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.unrelatedDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.unrelatedDataGrid.Name = "unrelatedDataGrid";
			this.unrelatedDataGrid.Size = new System.Drawing.Size(782, 376);
			this.unrelatedDataGrid.TabIndex = 101;
			// 
			// unrelatedTrackpointsPanel
			// 
			this.unrelatedTrackpointsPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																									this.clearUnrelatedButton,
																									this.messageUnrelatedLabel,
																									this.close3Button});
			this.unrelatedTrackpointsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.unrelatedTrackpointsPanel.Location = new System.Drawing.Point(0, 376);
			this.unrelatedTrackpointsPanel.Name = "unrelatedTrackpointsPanel";
			this.unrelatedTrackpointsPanel.Size = new System.Drawing.Size(782, 43);
			this.unrelatedTrackpointsPanel.TabIndex = 100;
			// 
			// clearUnrelatedButton
			// 
			this.clearUnrelatedButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.clearUnrelatedButton.Location = new System.Drawing.Point(10, 10);
			this.clearUnrelatedButton.Name = "clearUnrelatedButton";
			this.clearUnrelatedButton.Size = new System.Drawing.Size(57, 26);
			this.clearUnrelatedButton.TabIndex = 100;
			this.clearUnrelatedButton.Text = "Clear";
			this.clearUnrelatedButton.Click += new System.EventHandler(this.clearUnrelatedButton_Click);
			// 
			// messageUnrelatedLabel
			// 
			this.messageUnrelatedLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.messageUnrelatedLabel.Location = new System.Drawing.Point(75, 9);
			this.messageUnrelatedLabel.Name = "messageUnrelatedLabel";
			this.messageUnrelatedLabel.Size = new System.Drawing.Size(615, 27);
			this.messageUnrelatedLabel.TabIndex = 99;
			this.messageUnrelatedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// close3Button
			// 
			this.close3Button.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.close3Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.close3Button.Location = new System.Drawing.Point(694, 10);
			this.close3Button.Name = "close3Button";
			this.close3Button.Size = new System.Drawing.Size(71, 26);
			this.close3Button.TabIndex = 98;
			this.close3Button.Text = "Close";
			this.close3Button.Click += new System.EventHandler(this.close2Button_Click);
			// 
			// optionsTabPage
			// 
			this.optionsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.optionsPanel});
			this.optionsTabPage.Location = new System.Drawing.Point(4, 22);
			this.optionsTabPage.Name = "optionsTabPage";
			this.optionsTabPage.Size = new System.Drawing.Size(782, 419);
			this.optionsTabPage.TabIndex = 1;
			this.optionsTabPage.Text = "Options";
			// 
			// optionsPanel
			// 
			this.optionsPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.groupBox3,
																					   this.groupBox2,
																					   this.autoAnalyzeCheckBox,
																					   this.groupBox1,
																					   this.autoAnalyzeReminderCheckBox,
																					   this.groupBox4,
																					   this.groupBox5,
																					   this.close2Button});
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(782, 419);
			this.optionsPanel.TabIndex = 109;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.photoPositionsDonotwriteCheckBox,
																					this.photoPositionsAskCheckBox});
			this.groupBox3.Location = new System.Drawing.Point(340, 250);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(435, 65);
			this.groupBox3.TabIndex = 110;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Photo Positions file";
			// 
			// photoPositionsDonotwriteCheckBox
			// 
			this.photoPositionsDonotwriteCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.photoPositionsDonotwriteCheckBox.Location = new System.Drawing.Point(15, 40);
			this.photoPositionsDonotwriteCheckBox.Name = "photoPositionsDonotwriteCheckBox";
			this.photoPositionsDonotwriteCheckBox.Size = new System.Drawing.Size(415, 20);
			this.photoPositionsDonotwriteCheckBox.TabIndex = 106;
			this.photoPositionsDonotwriteCheckBox.Text = "do not write";
			this.photoPositionsDonotwriteCheckBox.CheckedChanged += new System.EventHandler(this.photoPositionsDonotwriteCheckBox_CheckedChanged);
			// 
			// photoPositionsAskCheckBox
			// 
			this.photoPositionsAskCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.photoPositionsAskCheckBox.Location = new System.Drawing.Point(15, 20);
			this.photoPositionsAskCheckBox.Name = "photoPositionsAskCheckBox";
			this.photoPositionsAskCheckBox.Size = new System.Drawing.Size(415, 20);
			this.photoPositionsAskCheckBox.TabIndex = 105;
			this.photoPositionsAskCheckBox.Text = "ask before overwriting";
			this.photoPositionsAskCheckBox.CheckedChanged += new System.EventHandler(this.photoPositionsAskCheckBox_CheckedChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.photoParametersDonotwriteCheckBox,
																					this.photoParametersAskCheckBox});
			this.groupBox2.Location = new System.Drawing.Point(340, 180);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(435, 65);
			this.groupBox2.TabIndex = 109;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Photo Parameters file";
			// 
			// photoParametersDonotwriteCheckBox
			// 
			this.photoParametersDonotwriteCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.photoParametersDonotwriteCheckBox.Location = new System.Drawing.Point(15, 40);
			this.photoParametersDonotwriteCheckBox.Name = "photoParametersDonotwriteCheckBox";
			this.photoParametersDonotwriteCheckBox.Size = new System.Drawing.Size(415, 20);
			this.photoParametersDonotwriteCheckBox.TabIndex = 106;
			this.photoParametersDonotwriteCheckBox.Text = "do not write";
			this.photoParametersDonotwriteCheckBox.CheckedChanged += new System.EventHandler(this.photoParametersDonotwriteCheckBox_CheckedChanged);
			// 
			// photoParametersAskCheckBox
			// 
			this.photoParametersAskCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.photoParametersAskCheckBox.Location = new System.Drawing.Point(15, 20);
			this.photoParametersAskCheckBox.Name = "photoParametersAskCheckBox";
			this.photoParametersAskCheckBox.Size = new System.Drawing.Size(415, 20);
			this.photoParametersAskCheckBox.TabIndex = 105;
			this.photoParametersAskCheckBox.Text = "ask before overwriting";
			this.photoParametersAskCheckBox.CheckedChanged += new System.EventHandler(this.photoParametersAskCheckBox_CheckedChanged);
			// 
			// autoAnalyzeCheckBox
			// 
			this.autoAnalyzeCheckBox.Location = new System.Drawing.Point(10, 5);
			this.autoAnalyzeCheckBox.Name = "autoAnalyzeCheckBox";
			this.autoAnalyzeCheckBox.Size = new System.Drawing.Size(455, 28);
			this.autoAnalyzeCheckBox.TabIndex = 105;
			this.autoAnalyzeCheckBox.Text = "analyze photos in folders when loading .GPX files";
			this.autoAnalyzeCheckBox.CheckedChanged += new System.EventHandler(this.autoAnalyzeCheckBox_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label2,
																					this.pictureSizeComboBox});
			this.groupBox1.Location = new System.Drawing.Point(10, 180);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(310, 135);
			this.groupBox1.TabIndex = 108;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "SmugMug.com Gallery";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(10, 25);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 23);
			this.label2.TabIndex = 9;
			this.label2.Text = "Picture Size:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pictureSizeComboBox
			// 
			this.pictureSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pictureSizeComboBox.Items.AddRange(new object[] {
																	 "Small",
																	 "Medium",
																	 "Large",
																	 "Original"});
			this.pictureSizeComboBox.Location = new System.Drawing.Point(105, 25);
			this.pictureSizeComboBox.Name = "pictureSizeComboBox";
			this.pictureSizeComboBox.Size = new System.Drawing.Size(130, 21);
			this.pictureSizeComboBox.TabIndex = 0;
			this.pictureSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.pictureSizeComboBox_SelectedIndexChanged);
			// 
			// autoAnalyzeReminderCheckBox
			// 
			this.autoAnalyzeReminderCheckBox.Location = new System.Drawing.Point(10, 30);
			this.autoAnalyzeReminderCheckBox.Name = "autoAnalyzeReminderCheckBox";
			this.autoAnalyzeReminderCheckBox.Size = new System.Drawing.Size(455, 28);
			this.autoAnalyzeReminderCheckBox.TabIndex = 107;
			this.autoAnalyzeReminderCheckBox.Text = "remind me if analyzing";
			this.autoAnalyzeReminderCheckBox.CheckedChanged += new System.EventHandler(this.autoAnalyzeReminderCheckBox_CheckedChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.thumbApplyBbutton,
																					this.thumbDisplayCheckBox,
																					this.label9,
																					this.thumbHeightNumericUpDown,
																					this.label8,
																					this.thumbWidthNumericUpDown});
			this.groupBox4.Location = new System.Drawing.Point(10, 65);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(309, 102);
			this.groupBox4.TabIndex = 9;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Thumbnail Images";
			// 
			// thumbApplyBbutton
			// 
			this.thumbApplyBbutton.Location = new System.Drawing.Point(168, 60);
			this.thumbApplyBbutton.Name = "thumbApplyBbutton";
			this.thumbApplyBbutton.Size = new System.Drawing.Size(66, 25);
			this.thumbApplyBbutton.TabIndex = 105;
			this.thumbApplyBbutton.Text = "Apply";
			this.thumbApplyBbutton.Click += new System.EventHandler(this.thumbApplyBbutton_Click);
			// 
			// thumbDisplayCheckBox
			// 
			this.thumbDisplayCheckBox.Location = new System.Drawing.Point(168, 25);
			this.thumbDisplayCheckBox.Name = "thumbDisplayCheckBox";
			this.thumbDisplayCheckBox.Size = new System.Drawing.Size(132, 20);
			this.thumbDisplayCheckBox.TabIndex = 104;
			this.thumbDisplayCheckBox.Text = "display on map";
			this.thumbDisplayCheckBox.CheckedChanged += new System.EventHandler(this.thumbDisplayCheckBox_CheckedChanged);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(9, 60);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(57, 20);
			this.label9.TabIndex = 10;
			this.label9.Text = "Height:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// thumbHeightNumericUpDown
			// 
			this.thumbHeightNumericUpDown.Increment = new System.Decimal(new int[] {
																					   10,
																					   0,
																					   0,
																					   0});
			this.thumbHeightNumericUpDown.Location = new System.Drawing.Point(75, 60);
			this.thumbHeightNumericUpDown.Maximum = new System.Decimal(new int[] {
																					 400,
																					 0,
																					 0,
																					 0});
			this.thumbHeightNumericUpDown.Minimum = new System.Decimal(new int[] {
																					 40,
																					 0,
																					 0,
																					 0});
			this.thumbHeightNumericUpDown.Name = "thumbHeightNumericUpDown";
			this.thumbHeightNumericUpDown.ReadOnly = true;
			this.thumbHeightNumericUpDown.Size = new System.Drawing.Size(56, 20);
			this.thumbHeightNumericUpDown.TabIndex = 9;
			this.thumbHeightNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.thumbHeightNumericUpDown.Value = new System.Decimal(new int[] {
																				   80,
																				   0,
																				   0,
																				   0});
			this.thumbHeightNumericUpDown.ValueChanged += new System.EventHandler(this.thumbHeightNumericUpDown_ValueChanged);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(9, 25);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(57, 20);
			this.label8.TabIndex = 8;
			this.label8.Text = "Width:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// thumbWidthNumericUpDown
			// 
			this.thumbWidthNumericUpDown.Increment = new System.Decimal(new int[] {
																					  10,
																					  0,
																					  0,
																					  0});
			this.thumbWidthNumericUpDown.Location = new System.Drawing.Point(75, 25);
			this.thumbWidthNumericUpDown.Maximum = new System.Decimal(new int[] {
																					400,
																					0,
																					0,
																					0});
			this.thumbWidthNumericUpDown.Minimum = new System.Decimal(new int[] {
																					40,
																					0,
																					0,
																					0});
			this.thumbWidthNumericUpDown.Name = "thumbWidthNumericUpDown";
			this.thumbWidthNumericUpDown.ReadOnly = true;
			this.thumbWidthNumericUpDown.Size = new System.Drawing.Size(56, 20);
			this.thumbWidthNumericUpDown.TabIndex = 7;
			this.thumbWidthNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.thumbWidthNumericUpDown.Value = new System.Decimal(new int[] {
																				  80,
																				  0,
																				  0,
																				  0});
			this.thumbWidthNumericUpDown.ValueChanged += new System.EventHandler(this.thumbWidthNumericUpDown_ValueChanged);
			// 
			// groupBox5
			// 
			this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox5.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.centerRadioButton,
																					this.topRightRadioButton});
			this.groupBox5.Location = new System.Drawing.Point(340, 65);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(435, 103);
			this.groupBox5.TabIndex = 106;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Thumbnail Position (relative to trackpoint)";
			// 
			// centerRadioButton
			// 
			this.centerRadioButton.Location = new System.Drawing.Point(18, 60);
			this.centerRadioButton.Name = "centerRadioButton";
			this.centerRadioButton.Size = new System.Drawing.Size(123, 28);
			this.centerRadioButton.TabIndex = 1;
			this.centerRadioButton.Text = "Center";
			this.centerRadioButton.CheckedChanged += new System.EventHandler(this.centerRadioButton_CheckedChanged);
			// 
			// topRightRadioButton
			// 
			this.topRightRadioButton.Location = new System.Drawing.Point(18, 35);
			this.topRightRadioButton.Name = "topRightRadioButton";
			this.topRightRadioButton.Size = new System.Drawing.Size(123, 28);
			this.topRightRadioButton.TabIndex = 0;
			this.topRightRadioButton.Text = "Top Right";
			this.topRightRadioButton.CheckedChanged += new System.EventHandler(this.topRightRadioButton_CheckedChanged);
			// 
			// close2Button
			// 
			this.close2Button.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.close2Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.close2Button.Location = new System.Drawing.Point(695, 386);
			this.close2Button.Name = "close2Button";
			this.close2Button.Size = new System.Drawing.Size(70, 25);
			this.close2Button.TabIndex = 98;
			this.close2Button.Text = "Close";
			this.close2Button.Click += new System.EventHandler(this.close2Button_Click);
			// 
			// cameraTabPage
			// 
			this.cameraTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.cameraPanel});
			this.cameraTabPage.Location = new System.Drawing.Point(4, 22);
			this.cameraTabPage.Name = "cameraTabPage";
			this.cameraTabPage.Size = new System.Drawing.Size(782, 419);
			this.cameraTabPage.TabIndex = 4;
			this.cameraTabPage.Text = "Camera Time";
			// 
			// cameraPanel
			// 
			this.cameraPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.traceLabel,
																					  this.helpExportLinkLabel,
																					  this.hintLabel,
																					  this.cameraShiftLabel2,
																					  this.useLinkLabel,
																					  this.timeShiftGroupBox,
																					  this.close4Button});
			this.cameraPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cameraPanel.Name = "cameraPanel";
			this.cameraPanel.Size = new System.Drawing.Size(782, 419);
			this.cameraPanel.TabIndex = 0;
			// 
			// traceLabel
			// 
			this.traceLabel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.traceLabel.Location = new System.Drawing.Point(10, 285);
			this.traceLabel.Name = "traceLabel";
			this.traceLabel.Size = new System.Drawing.Size(765, 70);
			this.traceLabel.TabIndex = 122;
			this.traceLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// helpExportLinkLabel
			// 
			this.helpExportLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.helpExportLinkLabel.Location = new System.Drawing.Point(695, 360);
			this.helpExportLinkLabel.Name = "helpExportLinkLabel";
			this.helpExportLinkLabel.Size = new System.Drawing.Size(70, 16);
			this.helpExportLinkLabel.TabIndex = 129;
			this.helpExportLinkLabel.TabStop = true;
			this.helpExportLinkLabel.Text = "Help";
			this.helpExportLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.helpExportLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpExportLinkLabel_LinkClicked);
			// 
			// hintLabel
			// 
			this.hintLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.hintLabel.Location = new System.Drawing.Point(10, 15);
			this.hintLabel.Name = "hintLabel";
			this.hintLabel.Size = new System.Drawing.Size(765, 80);
			this.hintLabel.TabIndex = 128;
			this.hintLabel.Text = "hint text here...";
			// 
			// cameraShiftLabel2
			// 
			this.cameraShiftLabel2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.cameraShiftLabel2.Location = new System.Drawing.Point(10, 105);
			this.cameraShiftLabel2.Name = "cameraShiftLabel2";
			this.cameraShiftLabel2.Size = new System.Drawing.Size(675, 40);
			this.cameraShiftLabel2.TabIndex = 127;
			this.cameraShiftLabel2.Text = "Camera Time Shift:";
			this.cameraShiftLabel2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// useLinkLabel
			// 
			this.useLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.useLinkLabel.Location = new System.Drawing.Point(690, 100);
			this.useLinkLabel.Name = "useLinkLabel";
			this.useLinkLabel.Size = new System.Drawing.Size(90, 20);
			this.useLinkLabel.TabIndex = 126;
			this.useLinkLabel.TabStop = true;
			this.useLinkLabel.Text = "<-- associate";
			this.useLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.useLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.useLinkLabel_LinkClicked);
			// 
			// close4Button
			// 
			this.close4Button.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.close4Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.close4Button.Location = new System.Drawing.Point(695, 386);
			this.close4Button.Name = "close4Button";
			this.close4Button.Size = new System.Drawing.Size(70, 25);
			this.close4Button.TabIndex = 99;
			this.close4Button.Text = "Close";
			this.close4Button.Click += new System.EventHandler(this.close2Button_Click);
			// 
			// DlgPhotoManager
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.close1Button;
			this.ClientSize = new System.Drawing.Size(794, 449);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl});
			this.DockPadding.All = 2;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(640, 450);
			this.Name = "DlgPhotoManager";
			this.Text = "Photo Collection Manager";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgPhotoManager_Closing);
			this.Load += new System.EventHandler(this.DlgPhotoManager_Load);
			this.Activated += new System.EventHandler(this.DlgPhotoManager_Activated);
			this.timeShiftGroupBox.ResumeLayout(false);
			this.cameraShiftPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.timeSecondsNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.timeMinutesNumericUpDown)).EndInit();
			this.tabControl.ResumeLayout(false);
			this.associateTabPage.ResumeLayout(false);
			this.associatePanel.ResumeLayout(false);
			this.pictureGroupBox.ResumeLayout(false);
			this.trackpointsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackpointsDataGrid)).EndInit();
			this.photoTrackpointsPanel.ResumeLayout(false);
			this.unrelatedTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.unrelatedDataGrid)).EndInit();
			this.unrelatedTrackpointsPanel.ResumeLayout(false);
			this.optionsTabPage.ResumeLayout(false);
			this.optionsPanel.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.thumbHeightNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.thumbWidthNumericUpDown)).EndInit();
			this.groupBox5.ResumeLayout(false);
			this.cameraTabPage.ResumeLayout(false);
			this.cameraPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Keyboard processing
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
				case Keys.PageUp:
					preview(PhotoWaypoints.FirstWaypoint(), m_keepInView);
					break;
				case Keys.PageDown:
					preview(PhotoWaypoints.LastWaypoint(), m_keepInView);
					break;
				case Keys.Back:
				case Keys.Up:
				case Keys.Left:
					preview(PhotoWaypoints.PreviousWaypoint(), m_keepInView);
					break;
				case Keys.Down:
				case Keys.Right:
					preview(PhotoWaypoints.NextWaypoint(), m_keepInView);
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
		#endregion

		#region Trackpoints Tab and DataGrid related

		private void rebuildTrackpointsTab()
		{
			Cursor.Current = Cursors.WaitCursor;
			trackpointsDataGrid.SuspendLayout();
			makeTrackpointsDS();
			makeTrackpointsDataGridStyle();
			//trackpointsDataGrid.SetDataBinding(m_trackpointsDS, "trackpoints");
			trackpointsDataGrid.DataSource = m_trackpointsDS.Tables["trackpoints"].DefaultView;
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[trackpointsDataGrid.DataSource, trackpointsDataGrid.DataMember];
			DataView dv = (DataView)cm.List;
			dv.AllowEdit = false;
			dv.AllowNew = false;		// causes selecting by arrows and shift to throw exception when selection reaches end and you press arrow up.
			trackpointsDataGrid.ResumeLayout();
			Cursor.Current = Cursors.Default;

			Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
			for(int i=0; wpt != null && i < m_trackpointsDS.Tables[0].Rows.Count ;i++)
			{
				int wptId = (int)m_trackpointsDS.Tables[0].Rows[i]["id"];
				if(wpt.Id == wptId)
				{
					trackpointsDataGrid.Select(i);
					break;
				}
			}

		}

		private void makeTrackpointsDataGridStyle()
		{
			if(m_trackpointsTS != null)
			{
				return;
			}

			//STEP 1: Create a DataTable style object and set properties if required. 
			m_trackpointsTS = new DataGridTableStyle(); 
			//specify the table from dataset (required step) 
			m_trackpointsTS.MappingName = "trackpoints";
			//m_trackpointsTS.RowHeadersVisible = false;
			// Set other properties (optional step) 
			//m_trackpointsTS.AlternatingBackColor = Color.LightBlue; 

			int colCount = 0;

			//STEP 0: Create an int column style and add it to the tablestyle 
			//this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[m_trackpointsDS, "trackpoints"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 1;
			m_trackpointsTS.GridColumnStyles.Add(csIDInt); 
			colCount++;

			//STEP 1: Create an int column style and add it to the tablestyle 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csTrackIDInt = new DataGridDigitsTextBoxColumn(pdc["trackid"], "i", true); 
			csTrackIDInt.MappingName = "trackid"; 
			csTrackIDInt.HeaderText = "Track Id"; 
			csTrackIDInt.Width = 1;
			m_trackpointsTS.GridColumnStyles.Add(csTrackIDInt); 
			colCount++;

			//STEP 2: Create an int column style and add it to the tablestyle 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csWptIdxInt = new DataGridDigitsTextBoxColumn(pdc["wptidx"], "i", true); 
			csWptIdxInt.MappingName = "wptidx"; 
			csWptIdxInt.HeaderText = "Wpt Idx"; 
			csWptIdxInt.Width = 1;
			m_trackpointsTS.GridColumnStyles.Add(csWptIdxInt); 
			colCount++;

			//STEP 3: Create a button-like column and add it to the tablestyle 
			DataGridColumnStyle ViewCol = new MyDataGridButtonColumn(); 
			ViewCol.MappingName = "view"; 
			ViewCol.HeaderText = "View"; 
			ViewCol.Width = 35; 
			//hook the new event to our handler in the grid
			((MyDataGridButtonColumn)ViewCol).CellButtonClick += new CellButtonClickEventHandler(HandleTrackpointsViewBtn);
			m_trackpointsTS.GridColumnStyles.Add(ViewCol);
			colCount++;

			//STEP 4: Create a string column and add it to the tablestyle 
			DataGridColumnStyle NameCol = new DataGridTextBoxColumn(); 
			NameCol.MappingName = "name"; //from dataset table 
			NameCol.HeaderText = "Name"; 
			NameCol.Width = 150; 
			m_trackpointsTS.GridColumnStyles.Add(NameCol); 
			colCount++;

			//STEP 5: Create a string column and add it to the tablestyle 
			DataGridColumnStyle TimeCol = new DataGridTextBoxColumn(); 
			TimeCol.MappingName = "time"; //from dataset table 
			TimeCol.HeaderText = "Time"; 
			TimeCol.Width = 170; 
			m_trackpointsTS.GridColumnStyles.Add(TimeCol); 
			colCount++;

			//STEP 6: Create a string column and add it to the tablestyle 
			DataGridColumnStyle LocationCol = new DataGridTextBoxColumn(); 
			LocationCol.MappingName = "location"; //from dataset table 
			LocationCol.HeaderText = "Location"; 
			LocationCol.Width = 180; 
			m_trackpointsTS.GridColumnStyles.Add(LocationCol); 
			colCount++;

			//STEP 7: Create a string column and add it to the tablestyle 
			DataGridColumnStyle SpeedCol = new DataGridTextBoxColumn(); 
			SpeedCol.MappingName = "speed"; //from dataset table 
			SpeedCol.HeaderText = "Speed";
			SpeedCol.Width = 80;
			m_trackpointsTS.GridColumnStyles.Add(SpeedCol);
			colCount++;

			//STEP 8: Create a string column and add it to the tablestyle 
			DataGridColumnStyle DistanceCol = new DataGridTextBoxColumn(); 
			DistanceCol.MappingName = "distance"; //from dataset table 
			DistanceCol.HeaderText = "Odometer";
			DistanceCol.Width = 80;
			m_trackpointsTS.GridColumnStyles.Add(DistanceCol);
			colCount++;

			//STEP 9: Create a string column and add it to the tablestyle 
			DataGridColumnStyle TrackCol = new DataGridTextBoxColumn(); 
			TrackCol.MappingName = "track"; //from dataset table 
			TrackCol.HeaderText = "Track"; 
			TrackCol.Width = 150; 
			m_trackpointsTS.GridColumnStyles.Add(TrackCol); 
			colCount++;

			//STEP 10: Create a string column and add it to the tablestyle 
			DataGridColumnStyle SourceCol = new DataGridTextBoxColumn(); 
			SourceCol.MappingName = "source"; //from dataset table 
			SourceCol.HeaderText = "Source"; 
			SourceCol.Width = 800; 
			m_trackpointsTS.GridColumnStyles.Add(SourceCol); 
			colCount++;

			trackpointsDataGrid.CaptionVisible = false;

			//STEP 8: Add the tablestyle to your datagrid's tablestlye collection:
			trackpointsDataGrid.TableStyles.Add(m_trackpointsTS);
		}

		private void makeTrackpointsDS()
		{
			DataTable trackpointsTable;
			
			rebuildingTrackpoints = true;

			string sortOrder = "";

			if(m_trackpointsDS != null)
			{
				sortOrder = m_trackpointsDS.Tables["trackpoints"].DefaultView.Sort;	// we will restore it later
				m_trackpointsDS.Tables.Clear();
				m_trackpointsDS.Dispose();
			}

			trackpointsTable = new DataTable("trackpoints");

			DataColumn myDataColumn;

			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "id";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			trackpointsTable.Columns.Add(myDataColumn);

			// needed to relate waypoint to track
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "trackid";
			myDataColumn.ReadOnly = true;
			trackpointsTable.Columns.Add(myDataColumn);

			// needed to index waypoint inside a track
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "wptidx";
			myDataColumn.ReadOnly = true;
			trackpointsTable.Columns.Add(myDataColumn);

			// make column to support checkbox/button
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "view";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "View";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create name column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Name";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create date/time column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = sortableDateTime.GetType();
			myDataColumn.ColumnName = "time";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Time";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create location column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "location";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Location";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create track column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "track";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Track";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create source column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "source";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Source";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "displayed";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Display";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Make the ID column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = trackpointsTable.Columns["id"];
			trackpointsTable.PrimaryKey = PrimaryKeyColumns;

			// Add the new DataTable to the DataSet.
			m_trackpointsDS = new DataSet();
			m_trackpointsDS.Tables.Add(trackpointsTable);

			// Create DataRow objects and add them to the DataTable
			DataRow myDataRow;
			int count = 0;
			Waypoint prevWpt = null;

			for (int i = 0; i < PhotoWaypoints.WaypointsWithThumbs.Count; i++)
			{
				Waypoint wpt = (Waypoint)PhotoWaypoints.WaypointsWithThumbs.GetByIndex(i);

				// wpt.Location.Lng, wpt.Location.Lat, wpt.Location.Elev
				if(wpt != null)
				{
					count++;

					Track trk = WaypointsCache.getTrackById(wpt.TrackId);
					myDataRow = trackpointsTable.NewRow();
					myDataRow["id"] = wpt.Id;
					myDataRow["trackid"] = wpt.TrackId;
					myDataRow["wptidx"] = i;		// in track
					string contentOrUrl = (wpt.Desc != null && wpt.Desc.Length > 0) ? "*" : "";
					contentOrUrl += (wpt.Url != null && wpt.Url.Length > 0) ? "@" : "";
					myDataRow["view"] = false;
					myDataRow["name"] = contentOrUrl + ((wpt.Name == null || wpt.Name.Length == 0) ? ("" + (i+1)) : wpt.Name);
					myDataRow["time"] = new SortableDateTime(wpt.DateTime, DateTimeDisplayMode.ConvertToLocal);
					myDataRow["location"] = wpt.Location.ToStringWithElev();

					myDataRow["track"] = trk == null ? "" : trk.Name;
					myDataRow["source"] = wpt.Source;
					myDataRow["displayed"] = true;
					trackpointsTable.Rows.Add(myDataRow);
					prevWpt = wpt;
				}
			}
			this.messageTrkptLabel.Text = ("total " + count + " track points with photos");

			trackpointsTable.RowDeleting += new DataRowChangeEventHandler(this.trackpoints_RowDeleting);

			trackpointsTable.DefaultView.Sort = sortOrder;

			if(trackpointsTable.DefaultView.Sort.Length == 0)
			{
				trackpointsTable.DefaultView.Sort = "time";
			}

			rebuildingTrackpoints = false;
		}

		private void trackpoints_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e) 
		{
			if (!rebuildingTrackpoints && !deletingTrackpoints) 
			{
				try 
				{
					int columnTrackId = 1;
					int columnTime = 4;
					DataTable table = m_trackpointsDS.Tables[0];
					long trackId = (long)((int)e.Row[columnTrackId]);
					SortableDateTime sdt = (SortableDateTime)e.Row[columnTime];
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null && sdt != null)
					{
						trk.Trackpoints.Remove(sdt.DateTime);		// wpt.DateTime is a key there
						PhotoWaypoints.WaypointsWithThumbs.Remove(sdt.DateTime);
						//WaypointsCache.isDirty = true;
					}
				} 
				catch {}
				finally
				{
					PictureManager.This.PictureBox.Invoke(new MethodInvoker(PictureManager.This.PictureBox.Refresh));	// invoke Paint
					Thread.Sleep(1);
				}
			}
		}

		private void HandleTrackpointsViewBtn(object sender, CellButtonClickEventArgs e)
		{
			int column = 0;
			DataTable table = m_trackpointsDS.Tables[0];
			int row = e.Row;
			int wptId = (int)trackpointsDataGrid[row, column];
			Waypoint wpt = PhotoWaypoints.getWaypointById(wptId);
			if(wpt != null)
			{
				DlgPhotoFullSize.BringFormUp(null, wpt);
				GeoCoord loc = new GeoCoord(wpt.Location.X, wpt.Location.Y, PictureManager.This.CameraManager.Elev);
				PictureManager.This.CameraManager.MarkLocation(loc, 0);
				PhotoWaypoints.SetCurrentWaypoint(wpt);
				if(m_keepInView)
				{
					PictureManager.This.CameraManager.keepInView(wpt.Location);
				}
			}
		}

		private void trackpointsDataGrid_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point pt = new Point(e.X, e.Y); 
			DataGrid.HitTestInfo hti = trackpointsDataGrid.HitTest(pt); 
			if(hti.Type == DataGrid.HitTestType.Cell && !trackpointsDataGrid.IsSelected(hti.Row)) 
			{ 
				trackpointsDataGrid.Select(hti.Row); 
				int column = 0;
				int columnTrackId = 1;
				int trackpointId = (int)trackpointsDataGrid[hti.Row, column];
				long trackId = (long)((int)trackpointsDataGrid[hti.Row, columnTrackId]);
				Track trk = WaypointsCache.getTrackById(trackId);
				if(trk != null)
				{
					Waypoint wpt = trk.getTrackpointById(trackpointId);
					if(wpt != null)
					{
						GeoCoord loc = new GeoCoord(wpt.Location.X, wpt.Location.Y, PictureManager.This.CameraManager.Elev);
						PictureManager.This.CameraManager.MarkLocation(loc, 0);
						PhotoWaypoints.SetCurrentWaypoint(wpt);
						if(m_keepInView)
						{
							PictureManager.This.CameraManager.keepInView(wpt.Location);
						}
					}
				}
			}
		}

		#endregion

		#region Unrelated Tab and DataGrid related

		private void rebuildUnrelatedTab()
		{
			Cursor.Current = Cursors.WaitCursor;
			unrelatedDataGrid.SuspendLayout();
			makeUnrelatedDS();
			makeUnrelatedDataGridStyle();
			//unrelatedDataGrid.SetDataBinding(m_unrelatedDS, "unrelated");
			unrelatedDataGrid.DataSource = m_unrelatedDS.Tables["unrelated"].DefaultView;
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[unrelatedDataGrid.DataSource, unrelatedDataGrid.DataMember];
			DataView dv = (DataView)cm.List;
			dv.AllowEdit = false;
			dv.AllowNew = false;		// causes selecting by arrows and shift to throw exception when selection reaches end and you press arrow up.
			unrelatedDataGrid.ResumeLayout();
			if(m_unrelatedDS.Tables[0].Rows.Count > 0)
			{
				unrelatedDataGrid.Select(0);
			}
			Cursor.Current = Cursors.Default;
		}

		private void makeUnrelatedDataGridStyle()
		{
			if(m_unrelatedTS != null)
			{
				return;
			}

			//STEP 1: Create a DataTable style object and set properties if required. 
			m_unrelatedTS = new DataGridTableStyle(); 
			//specify the table from dataset (required step) 
			m_unrelatedTS.MappingName = "unrelated";
			//m_unrelatedTS.RowHeadersVisible = false;
			// Set other properties (optional step) 
			//m_unrelatedTS.AlternatingBackColor = Color.LightBlue; 

			int colCount = 0;

			//STEP 1: Create an int column style and add it to the tablestyle 
			//this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[m_unrelatedDS, "unrelated"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 1;
			m_unrelatedTS.GridColumnStyles.Add(csIDInt); 
			colCount++;

			//STEP 2: Create a button-like column and add it to the tablestyle 
			DataGridColumnStyle ViewCol = new MyDataGridButtonColumn(); 
			ViewCol.MappingName = "view"; 
			ViewCol.HeaderText = "View"; 
			ViewCol.Width = 35; 
			//hook the new event to our handler in the grid
			((MyDataGridButtonColumn)ViewCol).CellButtonClick += new CellButtonClickEventHandler(HandleUnrelatedViewBtn);
			m_unrelatedTS.GridColumnStyles.Add(ViewCol);
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle NameCol = new DataGridTextBoxColumn(); 
			NameCol.MappingName = "name"; //from dataset table 
			NameCol.HeaderText = "Name"; 
			NameCol.Width = 150; 
			m_unrelatedTS.GridColumnStyles.Add(NameCol); 
			colCount++;

			//STEP 5: Create a string column and add it to the tablestyle 
			DataGridColumnStyle TimeCol = new DataGridTextBoxColumn(); 
			TimeCol.MappingName = "time"; //from dataset table 
			TimeCol.HeaderText = "Time"; 
			TimeCol.Width = 170; 
			m_unrelatedTS.GridColumnStyles.Add(TimeCol); 
			colCount++;

			//STEP 5: Create a string column and add it to the tablestyle 
			DataGridColumnStyle SourceCol = new DataGridTextBoxColumn(); 
			SourceCol.MappingName = "source"; //from dataset table 
			SourceCol.HeaderText = "Source"; 
			SourceCol.Width = 800; 
			m_unrelatedTS.GridColumnStyles.Add(SourceCol); 
			colCount++;

			unrelatedDataGrid.CaptionVisible = false;

			//STEP 8: Add the tablestyle to your datagrid's tablestlye collection:
			unrelatedDataGrid.TableStyles.Add(m_unrelatedTS);
		}

		private void makeUnrelatedDS()
		{
			DataTable unrelatedTable;
			
			rebuildingUnrelated = true;

			string sortOrder = "";

			if(m_unrelatedDS != null)
			{
				sortOrder = m_unrelatedDS.Tables["unrelated"].DefaultView.Sort;	// we will restore it later
				m_unrelatedDS.Tables.Clear();
				m_unrelatedDS.Dispose();
			}

			unrelatedTable = new DataTable("unrelated");

			DataColumn myDataColumn;

			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "id";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			unrelatedTable.Columns.Add(myDataColumn);

			// make column to support checkbox/button
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "view";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "View";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			unrelatedTable.Columns.Add(myDataColumn);

			// Create name column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Name";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			unrelatedTable.Columns.Add(myDataColumn);

			// Create date/time column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = sortableDateTime.GetType();
			myDataColumn.ColumnName = "time";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Time";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			unrelatedTable.Columns.Add(myDataColumn);

			// Create source column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "source";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Source";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			unrelatedTable.Columns.Add(myDataColumn);

			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "displayed";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Display";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			unrelatedTable.Columns.Add(myDataColumn);

			// Make the ID column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = unrelatedTable.Columns["id"];
			unrelatedTable.PrimaryKey = PrimaryKeyColumns;

			// Add the new DataTable to the DataSet.
			m_unrelatedDS = new DataSet();
			m_unrelatedDS.Tables.Add(unrelatedTable);

			// Create DataRow objects and add them to the DataTable
			DataRow myDataRow;
			int count = 0;

			for (int i = 0; i < PhotoWaypoints.PhotosUnrelated.Count; i++)
			{
				PhotoDescr pd = (PhotoDescr)PhotoWaypoints.PhotosUnrelated.GetByIndex(i);

				// wpt.Location.Lng, wpt.Location.Lat, wpt.Location.Elev
				if(pd != null)
				{
					count++;

					myDataRow = unrelatedTable.NewRow();
					myDataRow["id"] = pd.Id;
					myDataRow["view"] = false;
					myDataRow["name"] = pd.imageName;
					myDataRow["time"] = new SortableDateTime(pd.DTOrig, DateTimeDisplayMode.DisplayAsIs);

					myDataRow["source"] = pd.imageSource;
					unrelatedTable.Rows.Add(myDataRow);
				}
			}
			this.messageUnrelatedLabel.Text = ("" + count + " photos that couldn't be related to trackpoints");

			unrelatedTable.RowDeleting += new DataRowChangeEventHandler(this.unrelated_RowDeleting);

			unrelatedTable.DefaultView.Sort = sortOrder;

			if(unrelatedTable.DefaultView.Sort.Length == 0)
			{
				unrelatedTable.DefaultView.Sort = "time";
			}

			rebuildingUnrelated = false;
		}

		private void unrelated_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e) 
		{
			if (!rebuildingUnrelated && !deletingUnrelated) 
			{
				try 
				{
					int columnPhotoId = 0;
					DataTable table = m_unrelatedDS.Tables[0];
					int photoId = (int)e.Row[columnPhotoId];
					PhotoWaypoints.RemoveUnrelatedById(photoId);
				} 
				catch {}
				finally
				{
				}
			}
		}

		private void HandleUnrelatedViewBtn(object sender, CellButtonClickEventArgs e)
		{
			int columnPhotoId = 0;
			DataTable table = m_unrelatedDS.Tables[0];
			int row = e.Row;
			int photoId = (int)unrelatedDataGrid[row, columnPhotoId];
			PhotoDescr pd = PhotoWaypoints.getUnrelatedById(photoId);
			if(pd != null)
			{
				DlgPhotoFullSize.BringFormUp(pd, null);
			}
		}

		private void unrelatedDataGrid_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point pt = new Point(e.X, e.Y); 
			DataGrid.HitTestInfo hti = unrelatedDataGrid.HitTest(pt); 
			if(hti.Type == DataGrid.HitTestType.Cell && !unrelatedDataGrid.IsSelected(hti.Row)) 
			{ 
				unrelatedDataGrid.Select(hti.Row); 
			}
		}

		#endregion

		#region  Helpers

		private void setFileText(string txt)
		{
			myFileOrFolder.fileTextBox.Text = txt;
			myFileOrFolder.fileTextBox.Select(txt.Length,0);
			myFileOrFolder.fileTextBox.ScrollToCaret();
		}

		private void setRootText(string txt)
		{
			myFileOrFolder.folderTextBox.Text = txt;
			myFileOrFolder.folderTextBox.Select(txt.Length,0);
			myFileOrFolder.folderTextBox.ScrollToCaret();
		}

		private void setGalleryText(string txt)
		{
			myFileOrFolder.galleryTextBox.Text = txt.Trim();
			myFileOrFolder.galleryTextBox.Select(txt.Length,0);
			myFileOrFolder.galleryTextBox.ScrollToCaret();
		}

		// highlight the buttons that must be clicked after changing time shift values:
		private void setButtonsAgitated(bool agitate)
		{
			if(agitate)
			{
				saveTimeShiftButton.BackColor = Color.LightGreen;
				setTimeShiftButton.BackColor = Color.LightGreen;
				reprocessFileOrFolderButton.BackColor = Color.LightGreen;
			}
			else
			{
				saveTimeShiftButton.BackColor = origButtonBackColor;
				setTimeShiftButton.BackColor = origButtonBackColor;
				reprocessFileOrFolderButton.BackColor = origButtonBackColor;
			}
		}

		public void setImportButtonsAgitated()
		{
			this.myFileOrFolder.importFileButton.BackColor = Color.LightGreen;
		}

		private void disableButtons()
		{
			fwdButton.Enabled = ffwdButton.Enabled = false;
			backButton.Enabled = rewindButton.Enabled = false;
			fullSizeButton.Enabled = false;
		}

		private void setupButtons()
		{
			clearUnrelatedButton.Enabled = PhotoWaypoints.PhotosUnrelated.Count > 0;
			clearButton.Enabled = exportButton.Enabled = PhotoWaypoints.hasCurrentWaypoint();
			fwdButton.Enabled = ffwdButton.Enabled = PhotoWaypoints.hasNextWaypoint();
			backButton.Enabled = rewindButton.Enabled = PhotoWaypoints.hasPreviousWaypoint();
			fullSizeButton.Enabled = photoViewerControl.photoDescr != null;
		}

		private static DateTime timeZero = new DateTime(1, 1, 1, 0, 0, 0);
 
		private void setMessageLabel(PhotoDescr photoDescr, Waypoint wpt)
		{
			if(wpt != null && wpt.DateTime.CompareTo(timeZero) > 0)
			{
				this.messageLabel.Text = "" + Project.zuluToLocal(wpt.DateTime) + "   " + photoDescr.Width + " x " + photoDescr.Height;
			}
			else
			{
				this.messageLabel.Text = "[no time stamp]   " + photoDescr.Width + " x " + photoDescr.Height;
			}
		}

		private void preview(Waypoint wpt, bool keepInView)
		{
			if(photoViewerControl.photoDescr != null)
			{
				photoViewerControl.photoDescr.Dispose();
			}
			photoViewerControl.photoDescr = null;

			if(wpt != null)
			{
				disableButtons();

				// Create an PhotoDescr object from the specified file.
				PhotoDescr photoDescr = null;
				try 
				{
					photoDescr = PhotoDescr.FromThumbnail(wpt.ThumbSource);

					photoViewerControl.photoDescr = photoDescr;

					this.waypointLabel.Text = wpt.toStringPopup();
					setMessageLabel(photoDescr, wpt);

					if(keepInView)
					{
						PictureManager.This.CameraManager.keepInView(wpt.Location);
					}
					PictureManager.This.CameraManager.MarkLocation(wpt.Location, 0);

					if(DlgPhotoFullSize.isUp && tabControl.SelectedTab == this.associateTabPage)	// wpt not relevant when viewing unrelated photos
					{
						DlgPhotoFullSize.BringFormUp(photoViewerControl.photoDescr, wpt);	// we are sure that image and wpt are in sync
					}
				}
					// An invalid image will throw an OutOfMemoryException 
					// exception
				catch (OutOfMemoryException) 
				{
					setupButtons();
					this.waypointLabel.Text = "";
					throw new InvalidOperationException("'"	+ wpt.ThumbSource + "' is not a valid image file.");
				}
				catch
				{
					setupButtons();
					this.waypointLabel.Text = "";
				}
			}
			else
			{
				this.waypointLabel.Text = "";
			}
			setupButtons();
		}

		/// <summary>
		/// called from DlgPhotoFullSize to keep picture/list syncronized.
		/// call BringToFront() in your form after calling this function, or your form will be lowered.
		/// </summary>
		public static void sync(bool releaseCurrent)
		{
			if(This != null)
			{
				if(releaseCurrent)
				{
					// make sure that the file behing the current image is not locked
					if(This.photoViewerControl.photoDescr != null)
					{
						This.photoViewerControl.photoDescr.Dispose();
					}
					This.photoViewerControl.photoDescr = null;
					This.photoViewerControl.Refresh();
				}

				if(This.tabControl.SelectedTab == This.trackpointsTabPage)
				{
					This.rebuildTrackpointsTab();
				}
				else if(This.tabControl.SelectedTab == This.unrelatedTabPage)
				{
					This.rebuildUnrelatedTab();
				}
				
				if(This.tabControl.SelectedTab == This.associateTabPage || releaseCurrent)
				{
					Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
					This.preview(wpt, false);
				}
			}
		}

		private void setCurrentPhotoPreview()
		{
			try
			{
				if(Project.photoDoPreview && PhotoWaypoints.hasCurrentWaypoint())
				{
					Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
					This.preview(wpt, false);
				}
				else
				{
					photoViewerControl.photoDescr = null;
					photoViewerControl.Refresh();	// even if img==null, need to clear the picture
					this.waypointLabel.Text = "";
				}
			}
			catch
			{
				photoViewerControl.photoDescr = null;
				photoViewerControl.Refresh();	// even if img==null, need to clear the picture
				this.waypointLabel.Text = "";
			}
		}

		private void setQuickPhotoPreview(PhotoDescr photoDescr)
		{
				if(Project.photoDoPreview)
				{
					photoViewerControl.photoDescr = photoDescr;
				}
				else
				{
					photoViewerControl.photoDescr = null;
				}
	
				photoViewerControl.Refresh();	// even if img==null, need to clear the picture
		}

		#endregion // Helpers

		#region Managing TimeShift group of controls

		private void configureTimeshiftChoices(bool force, bool keepSelectedIndex)
		{
			int photoTimeShiftTypeSelected = shiftTypeComboBox.SelectedIndex;

			this.shiftTypeComboBox.Items.Clear();

			if(FileAndZipIO.lastPhotoParametersFileName == null || force)
			{
				this.shiftTypeComboBox.Items.AddRange(new object[] {
																	   "memorized for my camera",
																	   "temporary, just to try",
																	   "to be written into photoParameters.xml file"
																   });
			}
			else
			{
				this.shiftTypeComboBox.Items.AddRange(new object[] {
																	   "memorized for my camera",
																	   "temporary, just to try",
																	   "as read from photoParameters.xml file"
																   });
			}

			if(keepSelectedIndex)
			{
				// restore as they were before:
				shiftTypeComboBox.SelectedIndex = Project.photoTimeShiftTypeCurrent = photoTimeShiftTypeSelected;
			}
			else
			{
				// try to figure out what the settings should be:
				if(FileAndZipIO.lastPhotoParametersFileName != null && FileAndZipIO.usedPhotoParametersFile)
				{
					this.timeShiftEnabledCheckBox.Checked = true;
					shiftTypeComboBox.SelectedIndex = Project.photoTimeShiftTypeCurrent = m_photoTimeShiftTypeSelected = 2;
				}
				else
				{
					shiftTypeComboBox.SelectedIndex = m_photoTimeShiftTypeSelected = Project.photoTimeShiftTypeCurrent;
				}
			}
		}

		private void showTimeSiftControls(bool enabled)
		{
			cameraShiftPanel.Enabled = enabled;

			if(!enabled)
			{
				setButtonsAgitated(false);
			}
		}

		private void displayTimeShiftControlValues()
		{
			TimeSpan ts = Project.photoTimeShift(m_photoTimeShiftTypeSelected);

			int hours = (int)Math.Floor(Math.Abs(ts.TotalHours));
			int minutes = Math.Abs(ts.Minutes);
			int seconds = Math.Abs(ts.Seconds);

			this.timeSignComboBox.SelectedIndex = ts.TotalSeconds >= 0.0d ? 0 : 1;
			this.timeHoursComboBox.Text = "" + hours;
			this.timeMinutesNumericUpDown.Value = minutes;
			this.timeSecondsNumericUpDown.Value = seconds;

			int tzId = Project.photoTimeZoneId(m_photoTimeShiftTypeSelected);

			this.timeZoneComboBox.SelectedIndex = MyTimeZone.indexById(tzId);

			// make sure that buttons reflect the mode:
			switch(m_photoTimeShiftTypeSelected)
			{
				case 0:		// My Camera
					saveTimeShiftButton.Visible = false;
					break;
				case 1:		// temp
					saveTimeShiftButton.Visible = false;
					saveTimeShiftButton.Text = "Save As";	// TODO
					break;
				case 2:		// file
					saveTimeShiftButton.Visible = true;
					saveTimeShiftButton.Text = "Save to File";
					break;
			}

			// make sure it is also displayed in plain English:
			displayCameraShiftText();
		}

		private void displayCameraShiftText()
		{
			string str = "time shift  ";

			switch(Project.photoTimeShiftTypeCurrent)
			{
				case 0:		// My Camera -- save into a persistent Project variable:
					str += "[my camera]  ";
					break;
				case 1:		// temp
					str += "[temporary]  ";
					break;
				case 2:		// from file
					str += "[last file read]  ";
					break;
			}

			TimeSpan ts = Project.photoTimeShiftCurrent;

			str += ts.ToString();

			int tzId = Project.photoTimeZoneIdCurrent;

			str += "  " + MyTimeZone.timeZoneById(tzId).name;

			if(FileAndZipIO.lastPhotoParametersFileName != null)
			{
				str += "\r\nmeta file: " + FileAndZipIO.lastPhotoParametersFileName;
				if(Project.photoModeReprocess) // || !Project.photoModeReprocess && (m_photoTimeShiftTypeSelected != 2 || !timeShiftEnabledCheckBox.Checked))
				{
					str += " (not used)";
				}
			}

			cameraShiftLabel.Text = str;
			cameraShiftLabel2.Text = str;
		}

		/// <summary>
		/// from controls, also sets m_tzId
		/// </summary>
		private TimeSpan collectTimeShiftControlValues()
		{
			int days = 0;
			int hours = 0;

			try
			{
				hours = Convert.ToInt32(this.timeHoursComboBox.Text);
			}
			catch
			{
				MessageBox.Show("Error: hours must be numeric, not '" + this.timeHoursComboBox.Text + "'");
			}

			int minutes = Convert.ToInt32(this.timeMinutesNumericUpDown.Value);
			int seconds = Convert.ToInt32(this.timeSecondsNumericUpDown.Value);

			days = hours / 24;
			hours = hours % 24;

			bool timeShiftSignPositive = "+".Equals(this.timeSignComboBox.Text);

			if(!timeShiftSignPositive)
			{
				days = -days;
				hours = -hours;
				minutes = -minutes;
				seconds = -seconds;
			}

			if(setTimeShiftButton.Enabled)
			{
				// store it in case one of the Set buttons is clicked:
				m_tzId = MyTimeZone.TimeZones[this.timeZoneComboBox.SelectedIndex].id;

				setButtonsAgitated(true);
			}

			return new TimeSpan(days, hours, minutes, seconds, 0);
		}

		private void timeSignComboBox_TextChanged(object sender, System.EventArgs e)
		{
			collectTimeShiftControlValues();
		}

		private void timeHoursComboBox_TextChanged(object sender, System.EventArgs e)
		{
			collectTimeShiftControlValues();
		}

		private void timeMinutesNumericUpDown_TextChanged(object sender, System.EventArgs e)
		{
			collectTimeShiftControlValues();
		}

		private void timeSecondsNumericUpDown_TextChanged(object sender, System.EventArgs e)
		{
			collectTimeShiftControlValues();
		}

		private void timeZoneComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			collectTimeShiftControlValues();
		}

		private void timeMinutesNumericUpDown_Leave(object sender, System.EventArgs e)
		{
			int minutes = Convert.ToInt32(timeMinutesNumericUpDown.Text);
			timeMinutesNumericUpDown.Value = minutes > 59 ? 59 : minutes;
		}

		private void timeSecondsNumericUpDown_Leave(object sender, System.EventArgs e)
		{
			int seconds = Convert.ToInt32(timeSecondsNumericUpDown.Text);
			timeSecondsNumericUpDown.Value = seconds > 59 ? 59 : seconds;
		}

		private void timeShiftEnabledCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			showTimeSiftControls(timeShiftEnabledCheckBox.Checked);
		}

		private void shiftTypeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			m_photoTimeShiftTypeSelected = shiftTypeComboBox.SelectedIndex;
			displayTimeShiftControlValues();
		}

		/// <summary>
		/// into the Project variables, as selected in the combo box.
		/// </summary>
		/// <param name="ts"></param>
		/// <param name="tzId"></param>
		private void setTimeShift(TimeSpan ts, int tzId)
		{
			string str = "" + ts + "\r\nTime zone: " + MyTimeZone.timeZoneById(tzId).name;
			string str1 = "" + ts + " " + MyTimeZone.timeZoneById(tzId).name;
			// button action depends on the mode:
			switch(m_photoTimeShiftTypeSelected)
			{
				case 0:		// My Camera -- save into a persistent Project variable:
				{
					this.traceLabel.Text = "OK: camera shift set to: " + str1;
					if(Project.photoTimeShiftYourCamera.TotalSeconds != ts.TotalSeconds
						|| Project.photoTimeZoneIdYourCamera != tzId)
					{
						Project.ShowPopup(this.waypointLabel, "Set My Camera time shift to " + str + "\r\nClick \"Reprocess...\" to see new image positions on map.", Point.Empty);

						Project.photoTimeShiftYourCamera = ts;
						Project.photoTimeZoneIdYourCamera = tzId;
					}
				}
					break;

				case 1:		// temp
					this.traceLabel.Text = "OK: temp shift set to: " + str1;
					Project.photoTimeShiftTemp = ts;
					Project.photoTimeZoneIdTemp = tzId;
					break;

				case 2:		// from file
					this.traceLabel.Text = "OK: file shift set to: " + str1;
					Project.photoTimeShiftFile = ts;
					Project.photoTimeZoneIdFile = tzId;
					break;
			}

			setButtonsAgitated(false);

			displayCameraShiftText();
		}

		#endregion

		#region Processing functions -- file, folder, gallery

		private static bool hasWarned = false;

		private void processSingleFile()
		{
			Cursor.Current = Cursors.WaitCursor;
			myFileOrFolder.importFileButton.Enabled = false;
			myFileOrFolder.browseFileButton.Enabled = false;
			WaypointsCache.resetBoundaries();

			string photoFileName = myFileOrFolder.fileTextBox.Text;
			Project.photoFileName = photoFileName;
			Project.insertRecentFile(photoFileName);

			bool isGpx = AllFormats.isGpxFile(photoFileName);
			bool isZip = AllFormats.isZipFile(photoFileName);

			if(!Project.photoAnalyzeOnLoad && (!hasWarned || isGpx))
			{
				string sWarn = "Warning: you have previously chosen not to analyze JPEG files\nby scanning containing folder and subfolders.\n\n"
					+ "To turn ON the \"Analyze when loading .GPX\" mode answer \"Yes\" here or use Options tab.\nAnswering \"No\" will read in the file, but not the photos.";

				if(Project.YesNoBox(this, sWarn))
				{
					Project.photoAnalyzeOnLoad = true;
				}
				hasWarned = true;
			}

			if(isZip || isGpx)
			{
				// Photo collection import:
				ProcessFile imageFileProcessor = new ProcessFile(processImageFile);

				string cleanupName = photoFileName;
				if(isGpx)
				{
					cleanupName = new FileInfo(photoFileName).DirectoryName;
				}
				PhotoWaypoints.cleanPhotoTrackpointsBySource(cleanupName);
				collectTimeShiftControlValues();

				SortedList foldersToProcess = new SortedList();
				FileAndZipIO.readFile(photoFileName, null, null, imageFileProcessor, foldersToProcess, true);		// may add to foldersToProcess

				if(foldersToProcess.Count == 0)
				{
					// there are no .gpx or loc files there, but may be only images. We are explicitly asked to scan the images and
					// relate them to the trackpoints which are already in memory (say, just received from GPS)

					FileAndZipIO.enlistPhotoFolder(photoFileName, foldersToProcess, imageFileProcessor);
				}

				bool needsRefresh = false;
				// delayed processing of photo images in folder(s), as we need to have all trackpoints in memory first to relate to them:
				for(int i=0; i < foldersToProcess.Count ;i++)
				{
					PhotoFolderToProcess pf = (PhotoFolderToProcess)foldersToProcess.GetByIndex(i);

					if(!timeShiftEnabledCheckBox.Checked)
					{
						bool hasFoundFile = false;
						FileAndZipIO.readPhotoParameters(pf.name, out hasFoundFile);
						FileAndZipIO.usedPhotoParametersFile = hasFoundFile;
						if(hasFoundFile)
						{
							needsRefresh = true;
						}
					}

					FileAndZipIO.processImagesInPhotoFolder(pf.imageFileProcessor, pf.name, true);
				}

				if(m_doZoomIn)
				{
					PictureManager.This.CameraManager.zoomToCorners();
				}
				else
				{
					PictureManager.This.CameraManager.ProcessCameraMove();
				}

				if(needsRefresh)
				{
					configureTimeshiftChoices(false, false);
				}
			}
			else
			{
				// single JPEG file import:
				PhotoWaypoints.cleanPhotoTrackpoints(photoFileName);
				collectTimeShiftControlValues();

				Cursor.Current = Cursors.WaitCursor;
				myFileOrFolder.browseFileButton.Enabled = false;

				try
				{
					processImageFile(null, null, photoFileName, photoFileName);
				}
				catch (Exception exc)
				{
					LibSys.StatusBar.Error("Exception: " + exc.Message);
				}

				PictureManager.This.CameraManager.ProcessCameraMove();
			}

			setCurrentPhotoPreview();

			myFileOrFolder.browseFileButton.Enabled = true;
			myFileOrFolder.importFileButton.Enabled = true;
			Cursor.Current = Cursors.Default;

			setupButtons();
		}

		private void processFolder()
		{
			Cursor.Current = Cursors.WaitCursor;
			myFileOrFolder.importFolderButton.Enabled = false;
			myFileOrFolder.browseFolderButton.Enabled = false;
			WaypointsCache.resetBoundaries();

			string photoFolderPath = myFileOrFolder.folderTextBox.Text.Trim();
			Project.photoFolderPath = photoFolderPath;
			Project.insertRecentFile(photoFolderPath);

			ProcessFile imageFileProcessor = new ProcessFile(processImageFile);

			PhotoWaypoints.cleanPhotoTrackpointsBySource(photoFolderPath);
			collectTimeShiftControlValues();

			// read all .gpx files in the folder first:
			DirectoryInfo di = new DirectoryInfo(photoFolderPath);

			SortedList foldersToProcess = new SortedList();
			foreach(FileInfo fi in di.GetFiles())
			{
				if(AllFormats.isGpxFile(fi.FullName) || AllFormats.isLocFile(fi.FullName))
				{
					FileAndZipIO.readFile(fi.FullName, null, null, imageFileProcessor, foldersToProcess, false);
				}
			}

			if(foldersToProcess.Count == 0)
			{
				// there are no .gpx or loc files there, but may be only images. We are explicitly asked to scan the images and
				// relate them to the trackpoints which are already in memory (say, just received from GPS)

				FileAndZipIO.enlistPhotoFolder(photoFolderPath, foldersToProcess, imageFileProcessor);
			}

			bool needsRefresh = false;

			// delayed processing of photo images in folder(s), as we need to have all trackpoints in memory first to relate to them:
			for(int i=0; i < foldersToProcess.Count ;i++)
			{
				PhotoFolderToProcess pf = (PhotoFolderToProcess)foldersToProcess.GetByIndex(i);

				if(!timeShiftEnabledCheckBox.Checked)
				{
					bool hasFoundFile = false;
					FileAndZipIO.readPhotoParameters(pf.name, out hasFoundFile);
					FileAndZipIO.usedPhotoParametersFile = hasFoundFile;
					if(hasFoundFile)
					{
						needsRefresh = true;
					}
				}

				FileAndZipIO.processImagesInPhotoFolder(pf.imageFileProcessor, pf.name, true);
			}

			if(m_doZoomIn)
			{
				PictureManager.This.CameraManager.zoomToCorners();
			}
			else
			{
				PictureManager.This.CameraManager.ProcessCameraMove();
			}

			if(needsRefresh)
			{
				configureTimeshiftChoices(false, false);
			}

			setCurrentPhotoPreview();

			myFileOrFolder.importFolderButton.Enabled = true;
			myFileOrFolder.browseFolderButton.Enabled = true;
			Cursor.Current = Cursors.Default;

			setupButtons();
		}

		private void processGallery()
		{
			string galleryUrl = myFileOrFolder.galleryTextBox.Text.Trim();

			if(WaypointsCache.TracksAll.Count == 0)
			{
				string warning = "Warning: there are no tracks to relate images to.\r\nTo see images on the map, you need a track.\r\n";
				if("http://heysmile.smugmug.com/gallery/576522".Equals(galleryUrl))
				{
					if(Project.YesNoBox(this, warning + "\r\nWant to download a matching sample file?")) 
					{
						if(!((MainForm)Project.mainForm).sampleGalleryTrackOpen())
						{
							return;
						}
					}
					else
					{
						return;
					}
				}
				else
				{
					if(Project.YesNoBox(this, warning + "\r\nStill want to continue?")) 
					{
					}
					else
					{
						return;
					}
				}
			}

			// need to refresh windows or the picture is messed up.
			this.Refresh();
			PictureManager.This.CameraManager.ProcessCameraMove();

			Cursor.Current = Cursors.WaitCursor;
			myFileOrFolder.importGalleryButton.Enabled = false;
			myFileOrFolder.verifyGalleryButton.Enabled = false;
			WaypointsCache.resetBoundaries();

			PhotoWaypoints.cleanPhotoTrackpointsBySource(galleryUrl);
//			PhotoWaypoints.cleanPhotoTrackpoints(null);		// cleans all "related"
			PhotoWaypoints.cleanUnrelated("smugmug");
			collectTimeShiftControlValues();

			bool anonymously = true;

			int galleryId = -1;
			try
			{
				int pos = galleryUrl.LastIndexOf("/");
				if(pos >=0)
				{
					string sGalleryId = galleryUrl.Substring(pos+1);
					galleryId = Convert.ToInt32(sGalleryId);
				}
				else
				{
					galleryId = Convert.ToInt32(galleryUrl);
				}
			}
			catch 
			{
				Project.ErrorBox(this, "Gallery URL must end with ID like http://heysmile.smugmug.com/gallery/576522");
				return;
			}

			if(anonymously)
			{
				SmugMugApi.SmugMug = new SmugMugApi();
				SmugMugApi.SmugMug.LoginAnonymously();
			}
			else
			{
				if (RegistrySettings.Password.Length > 0 && RegistrySettings.AccountName.Length > 0)
				{
					SmugMugApi.SmugMug = new SmugMugApi(RegistrySettings.Username, RegistrySettings.Password);
					if (SmugMugApi.SmugMug.Login() == false)
					{
						Login login = new Login();
						login.ShowDialog();
					}
				}
				else
				{
					Login login = new Login();
					login.ShowDialog();
				}
			}

			if (SmugMugApi.SmugMug.Connected)
			{
				if(!anonymously)
				{
					LibSys.StatusBar.Trace("OK: connected to SmugMug -- account: " + RegistrySettings.AccountName);

//					Album[] albums = SmugMugApi.SmugMug.GetAlbums();
//					foreach(Album album in albums)
//					{
//						LibSys.StatusBar.Trace("-------------");
//						LibSys.StatusBar.Trace("AlbumID: " + album.AlbumID);
//						LibSys.StatusBar.Trace("Title: " + album.Title);
//						LibSys.StatusBar.Trace("" + album.ToString());
//					}
				}
				else
				{
					LibSys.StatusBar.Trace("OK: connected to SmugMug -- anonymously");
				}

				ProcessFile imageFileProcessor = new ProcessFile(processImageFile);

				int[] imageIds = SmugMugApi.SmugMug.GetImages(galleryId);
				foreach(int imageId in imageIds)
				{
					LibSys.StatusBar.Trace(" ===== ImageID: " + imageId + "  =====================");

					XmlRpcStruct imageUrls = SmugMugApi.SmugMug.GetImageURLs(imageId);
					foreach(string key in imageUrls.Keys)
					{
						LibSys.StatusBar.Trace("    " + key + ": " + imageUrls[key]);
					}

					XmlRpcStruct imageExif = SmugMugApi.SmugMug.GetImageExif(imageId);
					foreach(string key in imageExif.Keys)
					{
						LibSys.StatusBar.Trace("    " + key + ": " + imageExif[key]);
					}

					string imageName = "" + imageId;

					// will not show progress in photo viewer, fromLocal flag protects from it.
					PhotoGalleryIO.processSmugmugPhotoGalleryImage(imageFileProcessor, galleryUrl, imageName, imageUrls, imageExif);
				}
				if(m_doZoomIn)
				{
					PictureManager.This.CameraManager.zoomToCorners();
				}
				else
				{
					PictureManager.This.CameraManager.ProcessCameraMove();
				}

				setCurrentPhotoPreview();
			}
			else
			{
				Project.ErrorBox(this, "failed to connect to SmugMug");
				LibSys.StatusBar.Trace("Error: failed to connect to SmugMug");
			}

			myFileOrFolder.importGalleryButton.Enabled = true;
			myFileOrFolder.verifyGalleryButton.Enabled = true;
			Cursor.Current = Cursors.Default;

			setupButtons();
		}

		// used in delegates;
		// returns number of successfully related images
		// a similar (default) function is located in FileAndZipIO.cs, this one has preview functionality on top of that default one
        private int processImageFile(ZipFile zip, ZipEntry zipEntry, string gpxFileName, string imageFileName)
		{
			int ret = 0;
			bool fromLocal = true;

			PhotoDescr photoDescr;
			
			if(imageFileName != null && imageFileName.StartsWith("http://"))
			{
				fromLocal = false;
				photoDescr = PhotoDescr.FromThumbnail(imageFileName);	// assume the photoDescr was placed in cache
			}
			else
			{
				photoDescr = PhotoDescr.FromFileOrZipEntry(zip, zipEntry, imageFileName, null);
			}

			if(fromLocal)
			{
				setQuickPhotoPreview(photoDescr);
			}

			// figure out which timeshift to use:
			TimeSpan ts;
			int tzId;
			if(Project.photoModeReprocess && timeShiftEnabledCheckBox.Checked)
			{
				ts = Project.photoTimeShift(m_photoTimeShiftTypeSelected);
				tzId = Project.photoTimeZoneId(m_photoTimeShiftTypeSelected);
			}
			else
			{
				ts = Project.photoTimeShiftCurrent;
				tzId = Project.photoTimeZoneIdCurrent;
			}

			// relate to and create a trackpoint, memorizing current Project.photoTimeShift inside every photo trackpoint:
			Waypoint wpt = PhotoWaypoints.createPhotoTrackpoint(photoDescr, ts, tzId);
			if(wpt != null)
			{
				// be careful here. The source is used for clearing images out, beautifying it can break the Reprocess.
				wpt.Source = (zip == null) ? gpxFileName : zip.Name;
//				wpt.ImageSource = imageFileName;

				if(photoViewerControl.photoDescr == null || photoViewerControl.photoDescr.image == null)
				{
					PictureManager.This.CameraManager.MarkLocation(wpt.Location, 1);
				}
				else
				{
					PictureManager.This.CameraManager.invalidateAround(wpt.Location, wpt.ThumbImage.Width, wpt.ThumbImage.Height, wpt.ThumbPosition);
				}
				this.waypointLabel.Text = wpt.toStringPopup();
				ret++;
				if(wpt.TrackId != -1)
				{
					imagesRelatedCount++;				// image related to a trackpoint, not by EXIF coordinate tags
				}
				else
				{
					imagesWithExifCoordsCount++;		// image related to a new waypoint by EXIF coordinate tags
				}
			}
			else
			{
				this.waypointLabel.Text = "No associated trackpoint";
			}

			setMessageLabel(photoDescr, wpt);

			if(fromLocal && Project.photoDoPreview)
			{
				this.messageLabel.Refresh();
				this.waypointLabel.Refresh();
			}
			else
			{
				if(photoDescr.image != null)
				{
					photoDescr.image.Dispose();
					photoDescr.image = null;
					GC.Collect();
				}
			}
			return ret;
		}

		private int imagesRelatedCount = 0;
		private int imagesWithExifCoordsCount = 0;

		/// <summary>
		/// called from clickers and in turn calls object-specific functions
		/// </summary>
		/// <param name="reprocess"></param>
		private void processFileOrFolderOrGallery(bool reprocess)
		{

			if(reprocess)
			{
				Project.photoModeReprocess = true;					// ignore photoParameters file

				TimeSpan ts = collectTimeShiftControlValues();		// also sets m_tzId

				setTimeShift(ts, m_tzId);	// into Project variables
			}
			else
			{
				// use "my camera shift" as default for files w/o photo settings - they will be reset for each folder.
				// if photoParameters file is found, values from there will be used:
				Project.photoModeReprocess = false;
				timeShiftEnabledCheckBox.Checked = false;
			}

			reprocessFileOrFolderButton.Enabled = false;

			try
			{
				PhotoWaypoints.resetImagesTimestamps();
				imagesRelatedCount = 0;
			
				bool doZoomInSave = m_doZoomIn;
				m_doZoomIn = false;					// disable zoom in temporarily

				if(this.myFileOrFolder.tabMode == 0)
				{
					// processing single file or archive
					processSingleFile();
					m_doZoomIn = doZoomInSave;
					if(reprocess && imagesRelatedCount > 0 && AllFormats.isZipFile(Project.photoFileName) && m_photoTimeShiftTypeSelected != 1)
					{
						savePhotoParametersAndPositions(Project.photoFileName);
					}
				}
				else if(this.myFileOrFolder.tabMode == 1)
				{
					// processing folder
					processFolder();
					m_doZoomIn = doZoomInSave;
					if(reprocess && imagesRelatedCount > 0 && m_photoTimeShiftTypeSelected != 1)
					{
						savePhotoParametersAndPositions(myFileOrFolder.folderTextBox.Text);
					}
				}
				else if(this.myFileOrFolder.tabMode == 2)
				{
					// processing smugmug.com gallery
					processGallery();
					m_doZoomIn = doZoomInSave;
				}
			}
			catch (Exception exc)
			{
				LibSys.StatusBar.Error("Exception: " + exc);
			}

			setButtonsAgitated(false);
			reprocessFileOrFolderButton.Enabled = true;

			configureTimeshiftChoices(reprocess, true);
			this.timeShiftEnabledCheckBox.Checked = true;
			displayTimeShiftControlValues();

			analyzeImagesTimeSpread();
		}

		/// <summary>
		/// helps diagnosing time shift problems; brings up a message if images could not be related to tracks.
		/// </summary>
		private void analyzeImagesTimeSpread()
		{
			if(imagesRelatedCount > 0)
			{
				return;
			}

			StringBuilder message = new StringBuilder();

//			message.Append(String.Format("Related {0} images\r\n\r\n", imagesRelatedCount));
			message.Append("Could not relate any images to trackpoints\r\n\r\n");
			message.Append("(all times below local to your computer)\r\n\r\n");
			message.Append(String.Format("Images time from {0} to {1}\r\n\r\n",
				Project.zuluToLocal(PhotoWaypoints.imagesTimestampMin), Project.zuluToLocal(PhotoWaypoints.imagesTimestampMax)));

			const string separator = "-----------------------------------------------------------------------------------------    \r\n";

			foreach(Track trk in WaypointsCache.TracksAll)
			{
				if(trk.Enabled && !trk.isRoute)	// timestamps on routes are for sorting only, they don't have any other meaning
				{
					DateTime tStartZulu = trk.Start;
					DateTime tSEndZulu = trk.End;
					DateTime trkMedZulu = new DateTime(tStartZulu.Ticks / 2 + tSEndZulu.Ticks / 2);
					DateTime tStart = Project.zuluToLocal(tStartZulu);
					DateTime tEnd = Project.zuluToLocal(tSEndZulu);
					message.Append(separator);
					message.Append(String.Format("Track {0} time from {1} to {2}\r\n", trk.Id, tStart, tEnd));
					TimeSpan ts0 = PhotoWaypoints.imagesTimestampMin - trkMedZulu;
					TimeSpan ts1 = PhotoWaypoints.imagesTimestampMax - trkMedZulu;
					message.Append(String.Format("       -- images shifted relative to track between {0} and {1}\r\n",
										TimeSpanToString(ts0), TimeSpanToString(ts1)));

					TimeSpan timeZoneSpan = MyTimeZone.timeSpanById(Project.photoTimeZoneId(m_photoTimeShiftTypeSelected));
					message.Append(String.Format("       -- time zone shift you selected UTC{0}\r\n", TimeSpanToString(timeZoneSpan)));
					message.Append(String.Format("       -- your computer time zone shift UTC{0}\r\n", TimeSpanToString(MyTimeZone.zoneTimeShift)));

//					timeZoneSpan = Project.photoTimeShift(m_photoTimeShiftTypeSelected) + MyTimeZone.zoneTimeShift - timeZoneSpan;
//					message.Append(String.Format("       --      overall time zone shift {0}\r\n", TimeSpanToString(timeZoneSpan)));

					TimeSpan tsToFix0 = -ts0;
					TimeSpan tsToFix1 = -ts1;
					message.Append(String.Format("  to fix, adjust time shift by {0} to {1}\r\n",
										TimeSpanToString(tsToFix0), TimeSpanToString(tsToFix1)));
				}
			}

			message.Append(separator);

			message.Append("\r\nHints:\r\n  set Time Zone in sync with your camera, then adjust Time Shift\r\n");
			message.Append("  see Unrelated Photos tab for image timestamps\r\n");
			message.Append("  see Waypoints Manager for track timing\r\n");

			if(imagesWithExifCoordsCount > 0)
			{
				message.Append("  note that " + imagesWithExifCoordsCount + " images were positioned by their EXIF coordinates tags\r\n");
			}

			Project.MessageBox(this, message.ToString());
		}

		/// <summary>
		/// formats TimeSpan nicely
		/// </summary>
		/// <param name="ts"></param>
		/// <returns></returns>
		private string TimeSpanToString(TimeSpan ts)
		{
			if(ts.Days == 0)
			{
				if(ts.Hours == 0)
				{
					return String.Format("{0:D2}min", (int)ts.TotalMinutes);
				}
				else
				{
					return String.Format("{0}h{1:D2}m", (int)ts.Hours, (int)Math.Abs(ts.Minutes));
				}
			}
			else
			{
				return String.Format("{0}d{1:D2}h{2:D2}m", (int)ts.Days, (int)Math.Abs(ts.Hours), (int)Math.Abs(ts.Minutes));
			}
		}

		private void savePhotoParametersAndPositions(string fileOrFolderName)
		{
			FileAndZipIO.savePhotoParameters(this, fileOrFolderName);
			FileAndZipIO.savePhotoPositions(this, fileOrFolderName);
		}

		#endregion // processing functions -- file, folder, gallery

		#region Clickers

		private void browseFileButton_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

			if(File.Exists(Project.photoFileName))
			{
				openFileDialog.FileName = Project.photoFileName;
			}
			else
			{
				openFileDialog.InitialDirectory = myFileOrFolder.fileTextBox.Text;
			}
			openFileDialog.DefaultExt = "";
			openFileDialog.AddExtension = false;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "GPX, GPZ, ZIP and LOC files (*.gpx;*.gpz;*.zip;*.loc)|*.gpx;*.gpz;*.zip;*.loc|ZIP'ed GPX/Photo Collections (*.zip;*.gpz)|*.zip;*.gpz|JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*";

			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileInfo fi = new FileInfo(openFileDialog.FileName);
				setFileText(Project.GetLongPathName(fi.FullName));
				processFileOrFolderOrGallery(false);
				//processSingleFile();
			}
		}

		private void browseFolderButton_Click(object sender, System.EventArgs e)
		{
			IntPtr owner = this.Handle;
			clsFolderBrowser folderBrowser = new clsFolderBrowser(owner, "Select Root Folder");

			string tmp = folderBrowser.BrowseForFolder(myFileOrFolder.folderTextBox.Text);
			if(!"\\".Equals(tmp))		// cancel button causes "\" returned
			{
				setRootText(tmp);
			}
		}

		private void importFileButton_Click(object sender, System.EventArgs e)
		{
			myFileOrFolder.importFileButton.BackColor = origButtonBackColor;
			Project.photoFileName = myFileOrFolder.fileTextBox.Text;
			if(File.Exists(Project.photoFileName))
			{
				processFileOrFolderOrGallery(false);

				Project.photoModeReprocess = true;

				zoomCloseButton.Enabled = true;
				zoomOutButton.Enabled = false;
			}
			else
			{
				browseFileButton_Click(sender, e);
			}
		}

		private void importFolderButton_Click(object sender, System.EventArgs e)
		{
			if(!Directory.Exists(myFileOrFolder.folderTextBox.Text))
			{
				Project.ShowPopup(myFileOrFolder.importFolderButton, "Error: " + myFileOrFolder.folderTextBox.Text + "is not a valid folder", Point.Empty);
				return;
			}

			processFileOrFolderOrGallery(false);

			Project.photoModeReprocess = true;

			zoomCloseButton.Enabled = true;
			zoomOutButton.Enabled = false;

//			configureTimeshiftChoices(false, false);
//			displayTimeShiftControlValues();
		}

		private void verifyGalleryButton_Click(object sender, System.EventArgs e)
		{
			Project.RunBrowser(this.myFileOrFolder.galleryTextBox.Text.Trim());
		}

		private void importGalleryButton_Click(object sender, System.EventArgs e)
		{
			WaypointsCache.resetBoundaries();

			processFileOrFolderOrGallery(false);

			Project.photoModeReprocess = true;
			
//			processGallery();

			zoomCloseButton.Enabled = true;
			zoomOutButton.Enabled = false;
		}

		/// <summary>
		/// this is for Reprocess button on the Camera Shift tab, it needs to ignore the photoParameters file and use the
		/// settings from the form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void reprocessFileOrFolderButton_Click(object sender, System.EventArgs e)
		{
			processFileOrFolderOrGallery(true);
		}

		private void setTimeShiftButton_Click(object sender, System.EventArgs e)
		{
			TimeSpan ts = collectTimeShiftControlValues();		// also sets m_tzId

			setTimeShift(ts, m_tzId);	// into Project variables

			setButtonsAgitated(false);
		}

		/// <summary>
		/// photoTimeShift - "Save" and "Save As" action button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveTimeShiftButton_Click(object sender, System.EventArgs e)
		{
			TimeSpan ts = collectTimeShiftControlValues();		// also sets m_tzId

			setTimeShift(ts, m_tzId);	// into Project variables

			// button action depends on the mode:
			switch(Project.photoTimeShiftTypeCurrent)
			{
				case 0:		// My Camera -- button disabled
					break;
				case 1:		// temp -- Save As into a PhotoParameters.xml file of your choosing
					this.traceLabel.Text = "TODO: into a PhotoParameters.xml file of your choosing: " + ts;
					break;
				case 2:		// from file -- save to folder/archive of origin
					if(this.myFileOrFolder.tabMode == 0)
					{
						if(AllFormats.isZipFile(myFileOrFolder.fileTextBox.Text))
						{
							this.traceLabel.Text = "IP: saving to archive file of origin: " + ts;
							FileAndZipIO.savePhotoParameters(this, myFileOrFolder.fileTextBox.Text);
						}
						else
						{
							this.traceLabel.Text = "Error: source file is not an archive";
						}
					}
					else if(this.myFileOrFolder.tabMode == 1)
					{
						this.traceLabel.Text = "IP: saving to folder of origin: " + ts;
						FileAndZipIO.savePhotoParameters(this, myFileOrFolder.folderTextBox.Text);
					}
					else
					{
						// smugmug.com Gallery
						this.traceLabel.Text = "IP: skipped saving time shift: " + ts;
					}
					break;
			}
			setButtonsAgitated(false);

			configureTimeshiftChoices(false, false);
		}

		private void myFileOrFolder_ModeChanged(object sender, System.EventArgs e)
		{
			configureReprocessButton();
		}

		private void configureReprocessButton()
		{
			string[] labels = new string[] { "File", "Folder", "Gallery" };

			reprocessFileOrFolderButton.Text = "Reprocess " + labels[this.myFileOrFolder.tabMode];
		}

		private void logLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			LibSys.StatusBar.showLog();
		}

		private void thumbWidthNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			Project.thumbWidth = Convert.ToInt32(thumbWidthNumericUpDown.Value);
		}

		private void thumbHeightNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			Project.thumbHeight = Convert.ToInt32(thumbHeightNumericUpDown.Value);
		}

		private void previewCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoDoPreview = previewCheckBox.Checked;
		}

		private void thumbDisplayCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.thumbDoDisplay = thumbDisplayCheckBox.Checked;
			refreshPicture();
		}

		private void autoAnalyzeCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoAnalyzeOnLoad = autoAnalyzeCheckBox.Checked;
		}

		private void autoAnalyzeReminderCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoAnalyzeOnLoadReminder = autoAnalyzeReminderCheckBox.Checked;
		}

		private void close1Button_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void close2Button_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void topRightRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.thumbPosition = 0;
			PhotoWaypoints.resetThumbPositions();
			refreshPicture();
		}

		private void centerRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.thumbPosition = 1;
			PhotoWaypoints.resetThumbPositions();
			refreshPicture();
		}

		private void thumbApplyBbutton_Click(object sender, System.EventArgs e)
		{
			thumbApplyBbutton.Enabled = false;
			Cursor.Current = Cursors.WaitCursor;
			PhotoWaypoints.reprocessThumbnails();
			refreshPicture();
			Cursor.Current = Cursors.Default;
			thumbApplyBbutton.Enabled = true;
		}

		private void clearButton_Click(object sender, System.EventArgs e)
		{
			photoViewerControl.photoDescr = null;
			photoViewerControl.Refresh();
			PhotoWaypoints.cleanPhotoTrackpoints(null);
			rebuildTrackpointsTab();
			refreshPicture();
			Project.photoModeReprocess = false;
			messageTrkptLabel.Text = "OK: Photo trackpoints removed";

			setupButtons();
		}

		private void rewindButton_Click(object sender, System.EventArgs e)
		{
			Waypoint wpt = PhotoWaypoints.FirstWaypoint();
			preview(wpt, m_keepInView);
		}

		private void backButton_Click(object sender, System.EventArgs e)
		{
			Waypoint wpt = PhotoWaypoints.PreviousWaypoint();
			preview(wpt, m_keepInView);
		}

		private void fwdButton_Click(object sender, System.EventArgs e)
		{
			Waypoint wpt = PhotoWaypoints.NextWaypoint();
			preview(wpt, m_keepInView);
		}

		private void ffwdButton_Click(object sender, System.EventArgs e)
		{
			Waypoint wpt = PhotoWaypoints.LastWaypoint();
			preview(wpt, m_keepInView);
		}

		private void fullSizeButton_Click(object sender, System.EventArgs e)
		{
			Waypoint wpt = PhotoWaypoints.CurrentWaypoint();	// figure out what wpt is that
			DlgPhotoFullSize.BringFormUp(photoViewerControl.photoDescr, wpt);
		}

		private void photoViewerControl_Click(object sender, System.EventArgs e)
		{
			if(photoViewerControl.photoDescr != null)		// no clicking on blank picture
			{
				Waypoint wpt = PhotoWaypoints.CurrentWaypoint();	// figure out what wpt is that
				DlgPhotoFullSize.BringFormUp(photoViewerControl.photoDescr, wpt);
			}
		}

		private void keepInViewCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_keepInView = keepInViewCheckBox.Checked;
			
			Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
			if(hasLoaded && wpt != null && m_keepInView)
			{
				PictureManager.This.CameraManager.keepInView(wpt.Location);
			}
		}

		private void zoomInCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_doZoomIn = zoomInCheckBox.Checked;
		}

		private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(tabControl.SelectedTab == this.trackpointsTabPage)
			{
				rebuildTrackpointsTab();
			}
			else if(tabControl.SelectedTab == this.unrelatedTabPage)
			{
				rebuildUnrelatedTab();
			}
			else if(tabControl.SelectedTab == this.associateTabPage)
			{
				Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
				preview(wpt, m_keepInView);
			}
			else if(tabControl.SelectedTab == this.optionsTabPage)
			{
				autoAnalyzeCheckBox.Checked = Project.photoAnalyzeOnLoad;
			}
		}

		private void clearUnrelatedButton_Click(object sender, System.EventArgs e)
		{
			PhotoWaypoints.cleanUnrelated(null);
			rebuildUnrelatedTab();
			setupButtons();
		}

		public static GeoCoord savedLocation = null;		// also used by DlgPhotoFullSize

		private void zoomCloseButton_Click(object sender, System.EventArgs e)
		{
			Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
			if(wpt != null)
			{
				if(Math.Round(PictureManager.This.CameraManager.Location.Elev/10.0d) != Math.Round(Project.CAMERA_HEIGHT_REAL_CLOSE * 100.0d))
				{
					savedLocation = new GeoCoord(PictureManager.This.CameraManager.Location);
				}
				GeoCoord newLocation = new GeoCoord(wpt.Location);
				newLocation.Elev = Project.CAMERA_HEIGHT_REAL_CLOSE * 1000.0d;		// for 1m/pixel
				PictureManager.This.CameraManager.SpoilPicture();
				PictureManager.This.CameraManager.Location = newLocation;		// calls ProcessCameraMove();
				zoomCloseButton.Enabled = false;
				zoomOutButton.Enabled = true;
			}
		}

		private void zoomOutButton_Click(object sender, System.EventArgs e)
		{
			if(savedLocation != null)
			{
				PictureManager.This.CameraManager.SpoilPicture();
				PictureManager.This.CameraManager.Elev = savedLocation.Elev;		// calls ProcessCameraMove();
				savedLocation = null;
				zoomCloseButton.Enabled = true;
				zoomOutButton.Enabled = false;
			}
		}

		private void exportButton_Click(object sender, System.EventArgs e)
		{
			ArrayList waypoints = new ArrayList();		// array of Waypoint
			for (int i=0; i < PhotoWaypoints.WaypointsWithThumbs.Count; i++)
			{
				Waypoint wpt = (Waypoint)PhotoWaypoints.WaypointsWithThumbs.GetByIndex(i);
				Waypoint wptClone = new Waypoint(wpt);
				wptClone.LiveObjectType = LiveObjectTypes.LiveObjectTypeWaypoint;
				if(wptClone.WptName == null || wptClone.WptName.Length == 0)
				{
					wptClone.WptName = wpt.Desc;
				}
				wptClone.Url = wpt.ThumbSource;
				waypoints.Add(wptClone);
			}

			if(waypoints.Count > 0)
			{
				FileExportForm fileExportForm = new FileExportForm(null, false, waypoints, true);
				fileExportForm.ShowDialog();
			}
			else
			{
				Project.ShowPopup(exportButton, "\nno photo points to export", Point.Empty);
			}
		}

		private void pictureSizeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.photoGalleryPictureSize = pictureSizeComboBox.SelectedIndex;
		}

		private void adjustCameraShiftLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			tabControl.SelectedIndex = 4;
		}

		private void useLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			tabControl.SelectedIndex = 0;
		}
		private void helpExportLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser("http://" + Project.PROGRAM_MAIN_SERVER + "/" + Project.PROGRAM_NAME_LOGICAL + "/help/camerashift.html");
		}

		private void photoParametersAskCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoParametersAsk = photoParametersAskCheckBox.Checked;
		}

		private void photoParametersDonotwriteCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoParametersDonotwrite = photoParametersDonotwriteCheckBox.Checked;
			photoParametersAskCheckBox.Enabled = !photoParametersDonotwriteCheckBox.Checked;
		}

		private void photoPositionsAskCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoPositionsAsk = photoPositionsAskCheckBox.Checked;
		}

		private void photoPositionsDonotwriteCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoPositionsDonotwrite = photoPositionsDonotwriteCheckBox.Checked;
			photoPositionsAskCheckBox.Enabled = !photoPositionsDonotwriteCheckBox.Checked;
		}

		#endregion	// Clickers

		#region	Adjust camera shift by photo positioning

		private void adjustByPhotoLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.ShowPopup(adjustByPhotoLinkLabel, "\r\nSelect a photo, click on it\r\n ", Project.mainForm.PointToScreen(new Point(200, 200)));
			Project.photoSelectPhotoMode = 1;
			Project.delegateProcessingComplete = new DelegateProcessingComplete(photoSelectComplete);
			this.Close();
		}

		private static void photoSelectComplete()
		{
			Project.delegateProcessingComplete = null;
			Project.photoSelectPhotoMode = 0;

			Application.DoEvents();

			Project.mainForm.Invoke(new MethodInvoker(_photoSelectComplete));
		}

		private static void _photoSelectComplete()
		{
			DlgPhotoManager dlg = new DlgPhotoManager(4);
			//dlg.TopMost = true;
			dlg.Show();		// ShowDialog doesn't work here, the dialog appears and closes right away
			dlg.BringToFront();
		}

		private void adjustTimeShiftByPhotopoint()
		{
			try
			{
				Waypoint wptToAdjust = PhotoWaypoints.getWaypointById(Project.photoSelectedWptId);
				long trackId = wptToAdjust.TrackId;
				Track track = WaypointsCache.getTrackById(trackId);
				GeoCoord photoDesiredLocation = new GeoCoord(Project.photoSelectedCoord.X, Project.photoSelectedCoord.Y);
				this.traceLabel.Text = "Adjusting photopoint " + wptToAdjust.Name + " track: " + trackId + " to: " + photoDesiredLocation + "\r\n";

				// compute the closest trackpoint (to the click):
				Waypoint closestTrackpoint1 = null;
				int closestTrackpointIndex = -1;
				double closestDistance1 = 100000000.0;
				double bearing1 = 0.0d;
				for(int j=0; j < track.Trackpoints.Count ;j++)
				{
					Waypoint trkpt = (Waypoint)track.Trackpoints.GetByIndex(j);
					Distance dst = trkpt.Location.distanceFrom(photoDesiredLocation);
					if(dst.Meters < closestDistance1)
					{
						closestDistance1 = dst.Meters;
						closestTrackpoint1 = trkpt;
						closestTrackpointIndex = j;
						bearing1 = photoDesiredLocation.bearing(closestTrackpoint1.Location);
					}
				}
//				this.traceLabel.Text += "point1: " + closestTrackpoint1.Name + " (" + closestDistance1 + ")";

				// measure the previous and next trackpoints, choose the closest of them to the click:  
				Waypoint closestTrackpoint2 = null;
				double closestDistance2 =  10000000000.0d;
				double bearing2 = 0.0d;

				if(closestTrackpointIndex > 0)
				{
					// previous:
					closestTrackpoint2 = (Waypoint)track.Trackpoints.GetByIndex(closestTrackpointIndex - 1);
					closestDistance2 = closestTrackpoint2.Location.distanceFrom(photoDesiredLocation).Meters;
					bearing2 = photoDesiredLocation.bearing(closestTrackpoint2.Location);
				}

				Waypoint closestTrackpoint3 = null;
				double closestDistance3 =  10000000000.0d;
				double bearing3 = 0.0d;

				if(closestTrackpointIndex < track.Trackpoints.Count - 1)
				{
					// next:
					closestTrackpoint3 = (Waypoint)track.Trackpoints.GetByIndex(closestTrackpointIndex + 1);
					closestDistance3 = closestTrackpoint3.Location.distanceFrom(photoDesiredLocation).Meters;
					bearing3 = photoDesiredLocation.bearing(closestTrackpoint3.Location);
				}

				bool useNext = closestTrackpoint2 == null
							|| closestTrackpoint3 != null && (Math.Abs(bearing3 - bearing1) > Math.Abs(bearing2 - bearing1));

				if(useNext)
				{
					// using the next - it's leg is turned more towards the desired point:
					closestTrackpoint2 = closestTrackpoint3;
					closestDistance2 = closestDistance3;
				}

				// at this point closestTrackpoint3 is irrelevant, all is defined by the 1 and 2 and the ratio:

//				this.traceLabel.Text += "  point2: " + closestTrackpoint2.Name + " (" + closestDistance2 + ")";

				Graphics g = PictureManager.This.Graphics;
				Point p0 = CameraManager.This.toPixelLocation(photoDesiredLocation, null);

//				Point p1 = CameraManager.This.toPixelLocation(closestTrackpoint1.Location, null);
//				Point p2 = CameraManager.This.toPixelLocation(closestTrackpoint2.Location, null);
//				g.DrawLine(Pens.Yellow, p0, p1);
//				g.DrawLine(Pens.Yellow, p1, p2);
//				g.DrawLine(Pens.Yellow, p2, p0);

				// The main point is closestTrackpoint1, the other point is always closestTrackpoint2. 
				// positive ratio means go forward towards the next point, negative - go back towards the previous:
				double ratio = closestDistance1 / (closestDistance1 + closestDistance2);

//				this.traceLabel.Text += "\r\nratio: " + ratio;

				double lat = closestTrackpoint1.Location.Lat + (closestTrackpoint2.Location.Lat - closestTrackpoint1.Location.Lat) * ratio;
				double lng = closestTrackpoint1.Location.Lng + (closestTrackpoint2.Location.Lng - closestTrackpoint1.Location.Lng) * ratio;

				long midTicks = (long)Math.Round(closestTrackpoint1.DateTime.Ticks + (closestTrackpoint2.DateTime.Ticks - closestTrackpoint1.DateTime.Ticks) * ratio);

				GeoCoord midpoint = new GeoCoord(lng, lat);
				Point pMid = CameraManager.This.toPixelLocation(midpoint, null);
				g.DrawLine(Pens.Red, pMid, p0);

				long ticksToAdjust = midTicks - wptToAdjust.DateTime.Ticks;
				TimeSpan toAdjust = new TimeSpan(ticksToAdjust);

				this.traceLabel.Text += "   midpoint: " + midpoint + "  toAdjust=" + toAdjust;

//				//CameraManager.This.MarkLocation(midpoint, 0);
//				Waypoint wpt = new Waypoint(wptToAdjust);
//				wpt.TrackId = -1L;
//				wpt.Location = midpoint;
//				wpt.UrlName = "new";
//				WaypointsCache.addWaypoint(wpt);
//				CameraManager.This.ProcessCameraMove();

				Project.adjustPhotoTimeShift(m_photoTimeShiftTypeSelected, toAdjust);

				displayTimeShiftControlValues();

				this.setButtonsAgitated(true);
			}
			catch (Exception exc)
			{
				this.traceLabel.Text = "oops: " + exc;
			}
		}

		#endregion	// Adjust camera shift by photo positioning

	}
}
