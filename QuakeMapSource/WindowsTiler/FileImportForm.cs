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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using LibSys;
using LibGui;
using LibGeo;
using LibFormats;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for FileImportForm.
	/// </summary>
	public sealed class FileImportForm : System.Windows.Forms.Form
	{
		public const int MODE_GPS		= 0;
		public const int MODE_EQ		= 1;

		public const int TYPE_ANY		= 0;
		public const int TYPE_GEOTIFF	= 1;

		private static int m_mode = 0;
		private static int m_type = 0;

		private const int TAB_STATUS	= 0;
		private const int TAB_FORMAT	= 1;
		private const int TAB_FILE		= 2;
		private const int TAB_IMPORT	= 3;
		private const int TAB_RESULTS	= 4;

		private static int m_tab = 0;

		private string m_defaultExt = "";
		private string m_defaultExts = "";

		private StatusTab	m_statusTab;
		private FormatTab	m_formatTab;
		private FileTab		m_fileTab;
		private ImportTab	m_importTab;
		private FinishTab	m_finishTab;

		private static PictureManager m_pictureManager;
		public string m_selectedFileName = null;
		public static string m_selectedFormat = null;
		private static int elementCount = 0;
		private static string elementName = "";		// earthquakes or waypoints
		private static bool doPersist = false;
		private static bool doZoomIn = true;

		// create a delegate to pass to processors:
		private static InsertWaypoint m_countWaypoint = new InsertWaypoint(countWaypoint);
		private static InsertEarthquake m_countEarthquake = new InsertEarthquake(countEarthquake);
		private static FormatProcessor m_fileProcessor;
		
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button previousButton;
		private System.Windows.Forms.Button nextButton;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage formatTabPage;
		private System.Windows.Forms.TabPage fileTabPage;
		private System.Windows.Forms.TabPage importTabPage;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TabPage resultsTabPage;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton gpsRadioButton;
		private System.Windows.Forms.ComboBox detailComboBox;
		private System.Windows.Forms.Label detailLabel;
		private System.Windows.Forms.RadioButton eqRadioButton;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Button fileBrowseButton;
		private System.Windows.Forms.Label fileStatusLabel;
		private System.Windows.Forms.CheckBox persistCheckBox;
		private System.Windows.Forms.Label headerLabel;
		private System.Windows.Forms.Label diagLabel;
		private System.Windows.Forms.TabPage statusTabPage;
		private System.Windows.Forms.CheckedListBox allFilesCheckedListBox;
		private System.Windows.Forms.Button removeButton;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button detectFormatButton;
		private System.Windows.Forms.Label importFileNameLabel;
		private System.Windows.Forms.Label importFormatLabel;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button persistButton;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.LinkLabel unlockLinkLabel;
		private System.Windows.Forms.Label unlockLabel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Button quickOpenButton;
		private System.Windows.Forms.Button persistOffButton;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.CheckBox zoomInCheckBox;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FileImportForm(bool startOnList, int mode, int type)
		{
			m_pictureManager = PictureManager.This;
			m_mode = mode;
			m_type = type;

			m_statusTab	= new StatusTab(this);
			m_formatTab	= new FormatTab(this);
			m_fileTab	= new FileTab(this);
			m_importTab = new ImportTab(this);
			m_finishTab = new FinishTab(this);

			// Required for Windows Form Designer support
			InitializeComponent();

			//tabControl1.FlatStyle = FlatStyle.Flat;

			setLockedFeatures();

			detailComboBox.Items.Clear();
			detailComboBox.Text = "";

			previousButton.Enabled = false;
			nextButton.Enabled = true;
			doPersist = false;

			m_statusTab.tabActivated();		// make sure the list of current imports appears there, in case the Back button is clicked
			m_tab = startOnList ? 0 : 1;
			this.tabControl1.SelectedIndex = m_tab;
			m_formatTab.tabActivated();

			/*
			 * how do we make tabs not clickable?
			tabControl1.
			fileTabPage.CanSelect = false;
			importTabPage.CanSelect = false;
			resultsTabPage.CanSelect = false;
			*/

			Project.setDlgIcon(this);
		}

		private void setLockedFeatures()
		{
			unlockLabel.Visible = false;
			unlockLinkLabel.Visible = false;
			eqRadioButton.Enabled = true;
		}

		private static void countWaypoint(CreateInfo createInfo)
		{
			elementCount++;
			if(m_tab == TAB_RESULTS)
			{
				WaypointsCache.insertWaypoint(createInfo);
			}
		}

		// this one we need for Quick Open:
		private static void insertWaypoint(CreateInfo createInfo)
		{
			elementCount++;
			WaypointsCache.insertWaypoint(createInfo);
		}

		private static void countEarthquake(string[] infos)
		{
			elementCount++;
			if(m_tab == TAB_RESULTS)
			{
				EarthquakesCache.insertEarthquake(infos);
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
			this.cancelButton = new System.Windows.Forms.Button();
			this.previousButton = new System.Windows.Forms.Button();
			this.nextButton = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.statusTabPage = new System.Windows.Forms.TabPage();
			this.label12 = new System.Windows.Forms.Label();
			this.persistOffButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.persistButton = new System.Windows.Forms.Button();
			this.removeButton = new System.Windows.Forms.Button();
			this.allFilesCheckedListBox = new System.Windows.Forms.CheckedListBox();
			this.formatTabPage = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.detailLabel = new System.Windows.Forms.Label();
			this.detailComboBox = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.unlockLinkLabel = new System.Windows.Forms.LinkLabel();
			this.unlockLabel = new System.Windows.Forms.Label();
			this.eqRadioButton = new System.Windows.Forms.RadioButton();
			this.gpsRadioButton = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.fileTabPage = new System.Windows.Forms.TabPage();
			this.detectFormatButton = new System.Windows.Forms.Button();
			this.fileStatusLabel = new System.Windows.Forms.Label();
			this.fileBrowseButton = new System.Windows.Forms.Button();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.importTabPage = new System.Windows.Forms.TabPage();
			this.label6 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.importFormatLabel = new System.Windows.Forms.Label();
			this.importFileNameLabel = new System.Windows.Forms.Label();
			this.persistCheckBox = new System.Windows.Forms.CheckBox();
			this.zoomInCheckBox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.resultsTabPage = new System.Windows.Forms.TabPage();
			this.label11 = new System.Windows.Forms.Label();
			this.diagLabel = new System.Windows.Forms.Label();
			this.headerLabel = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.quickOpenButton = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.statusTabPage.SuspendLayout();
			this.formatTabPage.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.fileTabPage.SuspendLayout();
			this.importTabPage.SuspendLayout();
			this.resultsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(10, 275);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(68, 22);
			this.cancelButton.TabIndex = 96;
			this.cancelButton.Text = "Close";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// previousButton
			// 
			this.previousButton.Location = new System.Drawing.Point(270, 275);
			this.previousButton.Name = "previousButton";
			this.previousButton.Size = new System.Drawing.Size(80, 21);
			this.previousButton.TabIndex = 98;
			this.previousButton.Text = "List Imports";
			this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
			// 
			// nextButton
			// 
			this.nextButton.Location = new System.Drawing.Point(355, 275);
			this.nextButton.Name = "nextButton";
			this.nextButton.Size = new System.Drawing.Size(80, 22);
			this.nextButton.TabIndex = 97;
			this.nextButton.Text = "Next >>";
			this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.statusTabPage,
																					  this.formatTabPage,
																					  this.fileTabPage,
																					  this.importTabPage,
																					  this.resultsTabPage});
			this.tabControl1.ItemSize = new System.Drawing.Size(166, 21);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Padding = new System.Drawing.Point(0, 0);
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(445, 272);
			this.tabControl1.TabIndex = 4;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// statusTabPage
			// 
			this.statusTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.label12,
																						this.persistOffButton,
																						this.label4,
																						this.label7,
																						this.persistButton,
																						this.removeButton,
																						this.allFilesCheckedListBox});
			this.statusTabPage.Location = new System.Drawing.Point(4, 25);
			this.statusTabPage.Name = "statusTabPage";
			this.statusTabPage.Size = new System.Drawing.Size(437, 243);
			this.statusTabPage.TabIndex = 4;
			this.statusTabPage.Text = "Status                                       ";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(248, 210);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(80, 22);
			this.label12.TabIndex = 12;
			this.label12.Text = "Load on start:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// persistOffButton
			// 
			this.persistOffButton.Location = new System.Drawing.Point(376, 210);
			this.persistOffButton.Name = "persistOffButton";
			this.persistOffButton.Size = new System.Drawing.Size(32, 22);
			this.persistOffButton.TabIndex = 11;
			this.persistOffButton.Text = " No";
			this.persistOffButton.Click += new System.EventHandler(this.persistOffButton_Click);
			this.persistOffButton.MouseHover += new System.EventHandler(this.persistOffButton_MouseHover);
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(20, 10);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(392, 22);
			this.label4.TabIndex = 10;
			this.label4.Text = "Currently Imported                  [L] - Load on start";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(5, 210);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(66, 22);
			this.label7.TabIndex = 9;
			this.label7.Text = "Selected:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// persistButton
			// 
			this.persistButton.Location = new System.Drawing.Point(336, 210);
			this.persistButton.Name = "persistButton";
			this.persistButton.Size = new System.Drawing.Size(32, 22);
			this.persistButton.TabIndex = 2;
			this.persistButton.Text = "Yes";
			this.persistButton.Click += new System.EventHandler(this.persistButton_Click);
			this.persistButton.MouseHover += new System.EventHandler(this.persistButton_MouseHover);
			// 
			// removeButton
			// 
			this.removeButton.Location = new System.Drawing.Point(75, 210);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = new System.Drawing.Size(120, 22);
			this.removeButton.TabIndex = 1;
			this.removeButton.Text = "Remove from List";
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			this.removeButton.MouseHover += new System.EventHandler(this.removeButton_MouseHover);
			// 
			// allFilesCheckedListBox
			// 
			this.allFilesCheckedListBox.HorizontalScrollbar = true;
			this.allFilesCheckedListBox.Location = new System.Drawing.Point(15, 35);
			this.allFilesCheckedListBox.Name = "allFilesCheckedListBox";
			this.allFilesCheckedListBox.Size = new System.Drawing.Size(400, 169);
			this.allFilesCheckedListBox.TabIndex = 0;
			// 
			// formatTabPage
			// 
			this.formatTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.groupBox2,
																						this.groupBox1,
																						this.label1});
			this.formatTabPage.Location = new System.Drawing.Point(4, 25);
			this.formatTabPage.Name = "formatTabPage";
			this.formatTabPage.Size = new System.Drawing.Size(437, 243);
			this.formatTabPage.TabIndex = 0;
			this.formatTabPage.Text = "Format";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.detailLabel,
																					this.detailComboBox});
			this.groupBox2.Location = new System.Drawing.Point(192, 38);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(240, 194);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "File Format";
			// 
			// detailLabel
			// 
			this.detailLabel.Location = new System.Drawing.Point(15, 61);
			this.detailLabel.Name = "detailLabel";
			this.detailLabel.Size = new System.Drawing.Size(217, 123);
			this.detailLabel.TabIndex = 1;
			// 
			// detailComboBox
			// 
			this.detailComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.detailComboBox.Location = new System.Drawing.Point(8, 23);
			this.detailComboBox.Name = "detailComboBox";
			this.detailComboBox.Size = new System.Drawing.Size(224, 21);
			this.detailComboBox.TabIndex = 2;
			this.detailComboBox.SelectedIndexChanged += new System.EventHandler(this.detailComboBox_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.unlockLinkLabel,
																					this.unlockLabel,
																					this.eqRadioButton,
																					this.gpsRadioButton});
			this.groupBox1.Location = new System.Drawing.Point(8, 38);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(178, 194);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Data Type";
			// 
			// unlockLinkLabel
			// 
			this.unlockLinkLabel.Location = new System.Drawing.Point(22, 167);
			this.unlockLinkLabel.Name = "unlockLinkLabel";
			this.unlockLinkLabel.Size = new System.Drawing.Size(131, 21);
			this.unlockLinkLabel.TabIndex = 3;
			this.unlockLinkLabel.TabStop = true;
			this.unlockLinkLabel.Text = "unlock your program";
			// 
			// unlockLabel
			// 
			this.unlockLabel.Location = new System.Drawing.Point(15, 98);
			this.unlockLabel.Name = "unlockLabel";
			this.unlockLabel.Size = new System.Drawing.Size(153, 61);
			this.unlockLabel.TabIndex = 7;
			this.unlockLabel.Text = "msg";
			// 
			// eqRadioButton
			// 
			this.eqRadioButton.Location = new System.Drawing.Point(15, 61);
			this.eqRadioButton.Name = "eqRadioButton";
			this.eqRadioButton.Size = new System.Drawing.Size(153, 22);
			this.eqRadioButton.TabIndex = 1;
			this.eqRadioButton.Text = "Earthquakes";
			this.eqRadioButton.CheckedChanged += new System.EventHandler(this.eqRadioButton_CheckedChanged);
			// 
			// gpsRadioButton
			// 
			this.gpsRadioButton.Location = new System.Drawing.Point(15, 30);
			this.gpsRadioButton.Name = "gpsRadioButton";
			this.gpsRadioButton.Size = new System.Drawing.Size(153, 23);
			this.gpsRadioButton.TabIndex = 0;
			this.gpsRadioButton.Text = "GPS Waypoints";
			this.gpsRadioButton.CheckedChanged += new System.EventHandler(this.gpsRadioButton_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(10, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(372, 22);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select Format";
			// 
			// fileTabPage
			// 
			this.fileTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.detectFormatButton,
																					  this.fileStatusLabel,
																					  this.fileBrowseButton,
																					  this.fileTextBox,
																					  this.label5,
																					  this.label2});
			this.fileTabPage.Location = new System.Drawing.Point(4, 25);
			this.fileTabPage.Name = "fileTabPage";
			this.fileTabPage.Size = new System.Drawing.Size(437, 243);
			this.fileTabPage.TabIndex = 1;
			this.fileTabPage.Text = "File";
			// 
			// detectFormatButton
			// 
			this.detectFormatButton.Location = new System.Drawing.Point(51, 70);
			this.detectFormatButton.Name = "detectFormatButton";
			this.detectFormatButton.Size = new System.Drawing.Size(146, 22);
			this.detectFormatButton.TabIndex = 1;
			this.detectFormatButton.Text = "Auto Detect Format";
			this.detectFormatButton.Click += new System.EventHandler(this.detectFormatButton_Click);
			// 
			// fileStatusLabel
			// 
			this.fileStatusLabel.Location = new System.Drawing.Point(22, 100);
			this.fileStatusLabel.Name = "fileStatusLabel";
			this.fileStatusLabel.Size = new System.Drawing.Size(402, 140);
			this.fileStatusLabel.TabIndex = 5;
			// 
			// fileBrowseButton
			// 
			this.fileBrowseButton.Location = new System.Drawing.Point(336, 70);
			this.fileBrowseButton.Name = "fileBrowseButton";
			this.fileBrowseButton.Size = new System.Drawing.Size(88, 22);
			this.fileBrowseButton.TabIndex = 2;
			this.fileBrowseButton.Text = "Browse...";
			this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
			// 
			// fileTextBox
			// 
			this.fileTextBox.Location = new System.Drawing.Point(51, 40);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(373, 20);
			this.fileTextBox.TabIndex = 0;
			this.fileTextBox.Text = "";
			this.fileTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fileTextBox_KeyDown);
			this.fileTextBox.TextChanged += new System.EventHandler(this.fileTextBox_TextChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(15, 40);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(29, 22);
			this.label5.TabIndex = 2;
			this.label5.Text = "File:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(10, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(372, 22);
			this.label2.TabIndex = 1;
			this.label2.Text = "Select File";
			// 
			// importTabPage
			// 
			this.importTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.label6,
																						this.label10,
																						this.label9,
																						this.importFormatLabel,
																						this.importFileNameLabel,
																						this.persistCheckBox,
																						this.zoomInCheckBox,
																						this.label3});
			this.importTabPage.Location = new System.Drawing.Point(4, 25);
			this.importTabPage.Name = "importTabPage";
			this.importTabPage.Size = new System.Drawing.Size(437, 243);
			this.importTabPage.TabIndex = 2;
			this.importTabPage.Text = "Import";
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.Location = new System.Drawing.Point(10, 10);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(372, 22);
			this.label6.TabIndex = 9;
			this.label6.Text = "Verify";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(15, 72);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(51, 22);
			this.label10.TabIndex = 8;
			this.label10.Text = "Format:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(29, 48);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(37, 22);
			this.label9.TabIndex = 7;
			this.label9.Text = "File:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// importFormatLabel
			// 
			this.importFormatLabel.Location = new System.Drawing.Point(80, 72);
			this.importFormatLabel.Name = "importFormatLabel";
			this.importFormatLabel.Size = new System.Drawing.Size(336, 22);
			this.importFormatLabel.TabIndex = 6;
			this.importFormatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// importFileNameLabel
			// 
			this.importFileNameLabel.Location = new System.Drawing.Point(80, 48);
			this.importFileNameLabel.Name = "importFileNameLabel";
			this.importFileNameLabel.Size = new System.Drawing.Size(351, 22);
			this.importFileNameLabel.TabIndex = 5;
			this.importFileNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// persistCheckBox
			// 
			this.persistCheckBox.Location = new System.Drawing.Point(56, 112);
			this.persistCheckBox.Name = "persistCheckBox";
			this.persistCheckBox.Size = new System.Drawing.Size(373, 23);
			this.persistCheckBox.TabIndex = 0;
			this.persistCheckBox.Text = "make persistent (load on start)";
			this.persistCheckBox.CheckedChanged += new System.EventHandler(this.persistCheckBox_CheckedChanged);
			// 
			// zoomInCheckBox
			// 
			this.zoomInCheckBox.Location = new System.Drawing.Point(56, 144);
			this.zoomInCheckBox.Name = "zoomInCheckBox";
			this.zoomInCheckBox.Size = new System.Drawing.Size(373, 23);
			this.zoomInCheckBox.TabIndex = 10;
			this.zoomInCheckBox.Text = "zoom in after loading";
			this.zoomInCheckBox.CheckedChanged += new System.EventHandler(this.zoomInCheckBox_CheckedChanged);
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(48, 208);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(373, 22);
			this.label3.TabIndex = 1;
			this.label3.Text = "Click Next>> to import data";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// resultsTabPage
			// 
			this.resultsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.label11,
																						 this.diagLabel,
																						 this.headerLabel});
			this.resultsTabPage.Location = new System.Drawing.Point(4, 25);
			this.resultsTabPage.Name = "resultsTabPage";
			this.resultsTabPage.Size = new System.Drawing.Size(437, 243);
			this.resultsTabPage.TabIndex = 3;
			this.resultsTabPage.Text = "Results";
			// 
			// label11
			// 
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label11.Location = new System.Drawing.Point(16, 216);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(408, 22);
			this.label11.TabIndex = 7;
			this.label11.Text = "you can Close  now  -  or click Next>> to see imported files";
			// 
			// diagLabel
			// 
			this.diagLabel.Location = new System.Drawing.Point(18, 48);
			this.diagLabel.Name = "diagLabel";
			this.diagLabel.Size = new System.Drawing.Size(402, 160);
			this.diagLabel.TabIndex = 6;
			// 
			// headerLabel
			// 
			this.headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.headerLabel.Location = new System.Drawing.Point(10, 10);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(372, 22);
			this.headerLabel.TabIndex = 2;
			this.headerLabel.Text = "OK: Data imported - no errors";
			// 
			// label8
			// 
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(445, 23);
			this.label8.TabIndex = 5;
			// 
			// quickOpenButton
			// 
			this.quickOpenButton.Location = new System.Drawing.Point(180, 275);
			this.quickOpenButton.Name = "quickOpenButton";
			this.quickOpenButton.Size = new System.Drawing.Size(80, 22);
			this.quickOpenButton.TabIndex = 99;
			this.quickOpenButton.Text = "Quick Open";
			this.quickOpenButton.Click += new System.EventHandler(this.quickOpenButton_Click);
			// 
			// FileImportForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(444, 303);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.quickOpenButton,
																		  this.label8,
																		  this.tabControl1,
																		  this.nextButton,
																		  this.previousButton,
																		  this.cancelButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FileImportForm";
			this.Text = "File Import Wizard";
			this.tabControl1.ResumeLayout(false);
			this.statusTabPage.ResumeLayout(false);
			this.formatTabPage.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.fileTabPage.ResumeLayout(false);
			this.importTabPage.ResumeLayout(false);
			this.resultsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void nextButton_Click(object sender, System.EventArgs e)
		{
			previousButton.Text = "<< Back";
			switch(m_tab)
			{
				case TAB_STATUS:
					m_tab = TAB_FORMAT;
					tabControl1.SelectedIndex = m_tab;
					m_formatTab.tabActivated();
					break;
				case TAB_FORMAT:
					m_tab = TAB_FILE;
					tabControl1.SelectedIndex = m_tab;
					m_fileTab.tabActivated();
					break;
				case TAB_FILE:
					m_tab = TAB_IMPORT;
					tabControl1.SelectedIndex = m_tab;
					m_importTab.tabActivated();
					break;
				case TAB_IMPORT:
					m_tab = TAB_RESULTS;
					tabControl1.SelectedIndex = m_tab;
					m_finishTab.tabActivated();
					break;
				case TAB_RESULTS:
					m_tab = TAB_STATUS;
					tabControl1.SelectedIndex = m_tab;
					m_statusTab.tabActivated();
					break;
			}
		}

		private void previousButton_Click(object sender, System.EventArgs e)
		{
			switch(m_tab)
			{
				case TAB_RESULTS:
					m_tab = TAB_IMPORT;
					tabControl1.SelectedIndex = m_tab;
					break;
				case TAB_IMPORT:
					m_tab = TAB_FILE;
					tabControl1.SelectedIndex = m_tab;
					break;
				case TAB_FILE:
					m_tab = TAB_FORMAT;
					tabControl1.SelectedIndex = m_tab;
					break;
				case TAB_FORMAT:
					m_tab = TAB_STATUS;
					tabControl1.SelectedIndex = m_tab;
					break;
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			int m_tabPrev = m_tab;
			m_tab = tabControl1.SelectedIndex;
			switch(m_tab)
			{
				case TAB_STATUS:
					previousButton.Enabled = false;
					nextButton.Enabled = true;
					break;
				default:
					previousButton.Enabled = true;
					nextButton.Enabled = true;
					break;
			}
		}

		private void gpsRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			m_formatTab.gpsRadioButton_CheckedChanged(sender, e);
		}

		private void eqRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			m_formatTab.eqRadioButton_CheckedChanged(sender, e);
		}

		private void detailComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			m_selectedFileName = "";
			fileTextBox.Text = "";
			m_formatTab.detailComboBox_SelectedIndexChanged(sender, e);
		}

		private void fileBrowseButton_Click(object sender, System.EventArgs e)
		{
			if(m_fileTab.browse())
			{
				nextButton.Enabled = m_fileTab.tryRead();
				if(nextButton.Enabled)
				{
					Project.ShowPopup(nextButton, "Click Next>> to actually read the file.", Point.Empty);
				}
				detectFormatButton.Enabled = true;
			}
		}

		private void fileTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyData) 
			{
				case Keys.Enter:
					m_fileTab.fileNameChanged();
					detectFormatButton.Enabled = true;
					break;
			}
		}

		private void persistCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			doPersist = persistCheckBox.Checked;
		}

		private void zoomInCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			doZoomIn = zoomInCheckBox.Checked;
		}

		private void removeButton_Click(object sender, System.EventArgs e)
		{
			if(removeFileElements())
			{
				m_statusTab.fillAllFileList();
			}
			else
			{
				Project.ShowPopup(removeButton, "Select files by checking the boxes.", Point.Empty);
			}
		}

		private void persistButton_Click(object sender, System.EventArgs e)
		{
			if(setFilesPersistence(true))
			{
				m_statusTab.fillAllFileList();
			}
			else
			{
				Project.ShowPopup(persistButton, "Select files by checking the boxes.", Point.Empty);
			}
		}

		private void persistOffButton_Click(object sender, System.EventArgs e)
		{
			if(setFilesPersistence(false))
			{
				m_statusTab.fillAllFileList();
			}
			else
			{
				Project.ShowPopup(persistOffButton, "Select files by checking the boxes.", Point.Empty);
			}
		}

		/// <summary>
		/// removes all files from the file list and wipes out related waypoints from the map.
		/// </summary>
		public static void clearImportsList()
		{
			Cursor.Current = Cursors.WaitCursor;
			for(int i=Project.FileDescrList.Count-1;  i >= 0 ;i--)
			{
				FormattedFileDescr ffd = (FormattedFileDescr)Project.FileDescrList[i];
				WaypointsCache.RemoveWaypointsBySource(ffd.filename);
				CustomMapsCache.RemoveCustomMapsBySource(ffd.filename);
				EarthquakesCache.RemoveEarthquakesBySource(ffd.filename);
				Project.FileDescrList.RemoveAt(i);
			}
			PictureManager.This.Refresh();
		}

		/// <summary>
		/// removes selected files from the file list and wipes out related waypoints from the map.
		/// </summary>
		public bool removeFileElements()
		{
			bool ret = false;
			Cursor.Current = Cursors.WaitCursor;
			foreach(int index in allFilesCheckedListBox.CheckedIndices)
			{
				FormattedFileDescr ffd = (FormattedFileDescr)Project.FileDescrList[index];
				WaypointsCache.RemoveWaypointsBySource(ffd.filename);
				CustomMapsCache.RemoveCustomMapsBySource(ffd.filename);
				EarthquakesCache.RemoveEarthquakesBySource(ffd.filename);
				Project.FileDescrList[index] = null;
			}

			again:
				for(int i=0;  i < Project.FileDescrList.Count ;i++)
				{
					FormattedFileDescr ffd = (FormattedFileDescr)Project.FileDescrList[i];
					if(ffd == null)
					{
						Project.FileDescrList.RemoveAt(i);
						ret = true;
						goto again;
					}
				}
			m_pictureManager.Refresh();
			return ret;
		}

		public bool setFilesPersistence(bool on)
		{
			bool ret = false;
			foreach(int index in allFilesCheckedListBox.CheckedIndices)
			{
				FormattedFileDescr ffd = (FormattedFileDescr)Project.FileDescrList[index];
				ffd.isPersistent = on;
				ret = true;
			}
			return ret;
		}

		private void detectFormatButton_Click(object sender, System.EventArgs e)
		{
			string filename = fileTextBox.Text;
			string goodFormat = "";

			int numFormats = 0;
			switch(FileImportForm.m_mode)
			{
				case MODE_GPS:
					numFormats = AllFormats.gpsFormatObjects.Length;
					elementName = "waypoints";
					break;
				case MODE_EQ:
					numFormats = AllFormats.eqFormatObjects.Length;
					elementName = "earthquakes";
					break;
			}
			
			fileStatusLabel.Text = "selected format: " + m_selectedFormat + "\nTrying available formats:\n";
			for(int i=0; i < numFormats ;i++)
			{
				string formatName = "";
				BaseFormat format = null;
				FormatProcessor fileProcessor = null;

				switch(FileImportForm.m_mode)
				{
					case MODE_GPS:
					{
						formatName = AllFormats.gpsFormatNames[i];
						format = AllFormats.gpsFormatObjects[i];
						format.InsertWaypoint = m_countWaypoint;
						fileProcessor = new FormatProcessor(format.process);
					}
						break;
					case MODE_EQ:
					{
						formatName = AllFormats.eqFormatNames[i];
						format = AllFormats.eqFormatObjects[i];
						format.InsertEarthquake = m_countEarthquake;
						fileProcessor = new FormatProcessor(format.process);
					}
						break;
				}
				elementCount = 0;
				fileProcessor("", filename, filename);

				string diag = "  ...trying: " + formatName; 

				if(elementCount > 0)
				{
					diag += " - OK: detected " + elementCount + " " + elementName + ".\n";
					goodFormat = formatName;
				}
				else
				{
					diag += " - no match\n";
				}
				
				fileStatusLabel.Text += diag;
			}

			if(goodFormat.Length > 0) 
			{
				if(goodFormat.Equals(m_selectedFormat))
				{
					fileStatusLabel.Text += "Click \"Next>>\" to continue";
					fileStatusLabel.ForeColor = Color.Black;
				}
				else
				{
					fileStatusLabel.Text += "\nClick \"<<Back\" and select " + goodFormat + " format";
					fileStatusLabel.ForeColor = Color.Red;
				}
			} 
			else
			{
				fileStatusLabel.Text += "Sorry, the file cannot be imported";
				fileStatusLabel.ForeColor = Color.Red;
			}
		}

		private void fileTextBox_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		/*
		 *  obsolete - see FileAndZipIO.cs
		 * 
		/// <summary>
		/// Brings up file dialog, and then calls fif.m_finishTab.doRead() to do the reading
		/// </summary>
		public static void QuickOpen()
		{
			FileImportForm fif = new FileImportForm(PictureManager.This, false);
			System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();

			// use .gpx and .loc format:
			openFileDialog.InitialDirectory = Project.fileInitialDirectory;
			openFileDialog.FileName = "";
			openFileDialog.DefaultExt = ".gpx";
			openFileDialog.AddExtension = true;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "GPX, GPZ, ZIP and LOC files (*.gpx;*.gpz;*.zip;*.loc)|*.gpx;*.gpz;*.zip;*.loc|ZIP'ed GPX/Photo Collections (*.zip;*.gpz)|*.zip;*.gpz|GPS Exchange files (*.gpx)|*.gpx|Geocaching files (*.loc)|*.loc";
			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				Cursor.Current = Cursors.WaitCursor;
				string fileName = openFileDialog.FileName;
				fif.m_selectedFileName = fileName;
				int pos = fileName.LastIndexOf("\\");
				if(pos > 0)
				{
					Project.fileInitialDirectory = fileName.Substring(0, pos);
					Project.photoFileName = fileName;
				}

				BaseFormat format = AllFormats.formatByFileName(fileName);
				FileImportForm.m_selectedFormat = AllFormats.formatNameByFileName(fileName);
				format.InsertWaypoint = new InsertWaypoint(insertWaypoint);
				m_fileProcessor = new FormatProcessor(format.process);
				//fif.detailComboBox.Text = FileImportForm.m_selectedFormat;	// just for diag messages
				
				// call actual reader to do the job; we do it for GPX family only here (gpx, zip, gpz, loc):
				fif.m_finishTab.doRead(MODE_GPS);

				Track trk = WaypointsCache.getTrackById(Project.trackId);		// last track read
				if(trk != null || FileImportForm.elementCount > 0)
				{
					Project.mainCommand.wptEnabled(true);
					PictureManager.This.CameraManager.zoomToCorners();
				}
				Cursor.Current = Cursors.Default;
			}
		}
		*/

		private void quickOpenButton_Click(object sender, System.EventArgs e)
		{
			this.Bounds = new Rectangle(-100, -100, 0, 0);	// hide it real quick
			this.Close();
			FileAndZipIO.QuickOpen();
			//this.Dispose();
		}

		private void removeButton_MouseHover(object sender, System.EventArgs e)
		{
			Project.ShowPopup(removeButton, "Removes selected files from the list and deletes all the files' waypoints from the map. Does not delete files from hard drive.", Point.Empty);
		}

		private void persistButton_MouseHover(object sender, System.EventArgs e)
		{
			Project.ShowPopup(persistButton, "Will load selected files next time the program starts.", Point.Empty);
		}

		private void persistOffButton_MouseHover(object sender, System.EventArgs e)
		{
			Project.ShowPopup(persistOffButton, "Will NOT load selected files next time the program starts.", Point.Empty);
		}

		public class StatusTab
		{
			public FileImportForm m_fileImportForm;

			public StatusTab(FileImportForm fileImportForm)
			{
				m_fileImportForm = fileImportForm;
			}

			public void tabActivated()
			{
				fillAllFileList();
				if(m_fileImportForm.allFilesCheckedListBox.Items.Count > 0)
				{
					m_fileImportForm.removeButton.Enabled = true;
					m_fileImportForm.persistButton.Enabled = true;
					m_fileImportForm.persistOffButton.Enabled = true;
					//m_fileImportForm.fileListLabel.Enabled = true;
				}
				else
				{
					m_fileImportForm.removeButton.Enabled = false;
					m_fileImportForm.persistButton.Enabled = false;
					m_fileImportForm.persistOffButton.Enabled = false;
					//m_fileImportForm.fileListLabel.Enabled = false;
				}
			}

			public void fillAllFileList()
			{
				m_fileImportForm.allFilesCheckedListBox.Items.Clear();

				foreach(FormattedFileDescr ffd in Project.FileDescrList)
				{
					string sPersistent = "[" + (ffd.isPersistent ? "L" : "   ") + "] ";
					string name = sPersistent + ffd.filename + " [" + ffd.formatName + "]";
					m_fileImportForm.allFilesCheckedListBox.Items.Add(name, false);
				}
			}
		}
			
		public class FormatTab
		{
			public FileImportForm m_fileImportForm;

			public FormatTab(FileImportForm fileImportForm)
			{
				m_fileImportForm = fileImportForm;
			}

			public void tabActivated()
			{
				m_fileImportForm.nextButton.Enabled = m_fileImportForm.detailComboBox.SelectedIndex >= 0;

				switch(FileImportForm.m_mode)
				{
					case MODE_GPS:
						m_fileImportForm.gpsRadioButton.Checked = true;
						setFormatFields(m_fileImportForm.gpsRadioButton, AllFormats.gpsFormatNames, AllFormats.gpsFormatHelps,
							AllFormats.gpsDefaultExtension, AllFormats.gpsDefaultExtensions, AllFormats.gpsFormatObjects, MODE_GPS);
						if(FileImportForm.m_type == FileImportForm.TYPE_GEOTIFF)
						{
							for(int i = 0; i < m_fileImportForm.detailComboBox.Items.Count; i++)
							{
								string itemLabel = m_fileImportForm.detailComboBox.Items[i].ToString().ToLower();
								if(itemLabel.IndexOf("geotiff") >= 0)
								{
									m_fileImportForm.detailComboBox.SelectedIndex =  i;
									setFormat();
									break;
								}
							}
						}
						break;
					case MODE_EQ:
						m_fileImportForm.eqRadioButton.Checked = true;
						setFormatFields(m_fileImportForm.eqRadioButton, AllFormats.eqFormatNames, AllFormats.eqFormatHelps,
							AllFormats.eqDefaultExtension, AllFormats.eqDefaultExtensions, AllFormats.eqFormatObjects, MODE_EQ);
						break;
				}
			}

			public void detailComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
			{
				setFormat();
			}

			private void setFormat()
			{
				int index = m_fileImportForm.detailComboBox.SelectedIndex;
				switch(FileImportForm.m_mode)
				{
					case MODE_GPS:
					{
						m_fileImportForm.detailLabel.Text = AllFormats.gpsFormatHelps[index];
						m_fileImportForm.m_defaultExt = AllFormats.gpsDefaultExtension[index];
						m_fileImportForm.m_defaultExts = AllFormats.gpsDefaultExtensions[index];
						FileImportForm.m_selectedFormat = AllFormats.gpsFormatNames[index];
						BaseFormat format = AllFormats.gpsFormatObjects[index];
						format.InsertWaypoint = m_countWaypoint;
						m_fileProcessor = new FormatProcessor(format.process);
						elementName = "waypoints";
					}
						break;
					case MODE_EQ:
					{
						m_fileImportForm.detailLabel.Text = AllFormats.eqFormatHelps[index];
						m_fileImportForm.m_defaultExt = AllFormats.eqDefaultExtension[index];
						m_fileImportForm.m_defaultExts = AllFormats.eqDefaultExtensions[index];
						FileImportForm.m_selectedFormat = AllFormats.eqFormatNames[index];
						BaseFormat format = AllFormats.eqFormatObjects[index];
						format.InsertEarthquake = m_countEarthquake;
						m_fileProcessor = new FormatProcessor(format.process);
						elementName = "earthquakes";
					}
						break;
				}
			}

			public void gpsRadioButton_CheckedChanged(object sender, System.EventArgs e)
			{
				setFormatFields(m_fileImportForm.gpsRadioButton, AllFormats.gpsFormatNames, AllFormats.gpsFormatHelps,
									AllFormats.gpsDefaultExtension, AllFormats.gpsDefaultExtensions, AllFormats.gpsFormatObjects, MODE_GPS);
			}

			public void eqRadioButton_CheckedChanged(object sender, System.EventArgs e)
			{
				setFormatFields(m_fileImportForm.eqRadioButton, AllFormats.eqFormatNames, AllFormats.eqFormatHelps,
									AllFormats.eqDefaultExtension, AllFormats.eqDefaultExtensions, AllFormats.eqFormatObjects, MODE_EQ);
			}


			private void setFormatFields(RadioButton radioButton, string[] formatNames, string[] formatHelps,
											string[] formatExt, string[] formatExts, BaseFormat[] formatObjects, int mode)
			{
				if(radioButton.Checked)
				{
					FileImportForm.m_mode = mode;
					m_fileImportForm.detailComboBox.Items.Clear();
					m_fileImportForm.detailComboBox.Items.AddRange(formatNames);
					// try to keep previously selected format:
					int index = 0;
					int i=0;
					for (; i < formatNames.Length ;i++)
					{
						if(formatNames[i].Equals(FileImportForm.m_selectedFormat))
						{
							index = i;
							break;
						}
					}
					m_fileImportForm.detailComboBox.SelectedIndex = index;
					m_fileImportForm.detailLabel.Text = formatHelps[index];
					m_fileImportForm.m_defaultExt = formatExt[index];
					m_fileImportForm.m_defaultExts = formatExts[index];
					m_fileProcessor = new FormatProcessor(formatObjects[index].process);
					m_fileImportForm.nextButton.Enabled = true;
				}
			}
		}

		public class FileTab
		{
			public FileImportForm m_fileImportForm;
			private System.Windows.Forms.OpenFileDialog openFileDialog1;

			public FileTab(FileImportForm fileImportForm)
			{
				m_fileImportForm = fileImportForm;
				this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			}

			public void tabActivated()
			{
				if(FileImportForm.m_selectedFormat == null || FileImportForm.m_selectedFormat.Length == 0)
				{
					switch(m_type)
					{
						case TYPE_GEOTIFF:
							m_fileImportForm.tabControl1.SelectedIndex = 1;	// make sure format is valid
							break;

						case TYPE_ANY:
						default:
							m_fileImportForm.tabControl1.SelectedIndex = 1;	// make sure format is valid
							break;
					}
				}
				else
				{
					m_fileImportForm.detectFormatButton.Enabled = false;
					if(m_fileImportForm.m_selectedFileName == null
						|| m_fileImportForm.m_selectedFileName.Length == 0
						|| !File.Exists(m_fileImportForm.m_selectedFileName))
					{
						browse();
					}
					else
					{
						m_fileImportForm.fileTextBox.Text = m_fileImportForm.m_selectedFileName;
					}

					if(m_fileImportForm.m_selectedFileName == null
						|| m_fileImportForm.m_selectedFileName.Length == 0
						|| !File.Exists(m_fileImportForm.m_selectedFileName))
					{
						m_fileImportForm.fileStatusLabel.Text = "please select a file to import";
						m_fileImportForm.fileStatusLabel.ForeColor = Color.Red;
						m_fileImportForm.nextButton.Enabled = false;
					}
					else
					{
						m_fileImportForm.fileStatusLabel.Text = "";
						m_fileImportForm.fileStatusLabel.ForeColor = Color.Black;
						m_fileImportForm.nextButton.Enabled = tryRead();
						if(m_fileImportForm.nextButton.Enabled)
						{
							Project.ShowPopup(m_fileImportForm.nextButton, "Click Next>> to actually read the file.", Point.Empty);
						}
						m_fileImportForm.detectFormatButton.Enabled = true;
					}
				}
			}

			public void fileNameChanged()
			{
				m_fileImportForm.m_selectedFileName = m_fileImportForm.fileTextBox.Text;
			}

			public bool browse()
			{
				if(m_fileImportForm.m_selectedFileName == null
					|| m_fileImportForm.m_selectedFileName.Length == 0
					|| !File.Exists(m_fileImportForm.m_selectedFileName))
				{
					openFileDialog1.InitialDirectory = Project.fileInitialDirectory;
					openFileDialog1.FileName = "";
				} 
				else
				{
					openFileDialog1.FileName = m_fileImportForm.m_selectedFileName;
				}
				openFileDialog1.DefaultExt = m_fileImportForm.m_defaultExt;
				openFileDialog1.AddExtension = true;
				// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
				openFileDialog1.Filter = FileImportForm.m_selectedFormat + "(" + m_fileImportForm.m_defaultExts + ")|" + m_fileImportForm.m_defaultExts + "|All files (*.*)|*.*";
				DialogResult result = openFileDialog1.ShowDialog();
				if (result == DialogResult.OK)
				{
					string fileName = openFileDialog1.FileName;
					m_fileImportForm.m_selectedFileName = fileName;
					int pos = fileName.LastIndexOf("\\");
					if(pos > 0)
					{
						Project.fileInitialDirectory = fileName.Substring(0, pos);
					}
					m_fileImportForm.fileTextBox.Text = m_fileImportForm.m_selectedFileName;
					return true;
				}
				return false;
			}

			public bool tryRead()
			{
				bool ret = false;	// success indicator, assume failed
				string filename = m_fileImportForm.m_selectedFileName;

				string sRepeatRead = FileAndZipIO.isRepeatRead(filename) ? " (repeat read)" : "";

				// try reading the file, just counting elements:
				elementCount = 0;
				m_fileProcessor("", filename, filename);

				string formatName = m_fileImportForm.detailComboBox.Text;

				string diag = "Selected format: " + formatName + sRepeatRead + "\n\n"; 

				if(AllFormats.isTiffFile(filename))
				{
					ret = true;	
					m_fileImportForm.detectFormatButton.Visible = false;
				}
				else
				{
					m_fileImportForm.detectFormatButton.Visible = true;
					if(elementCount > 0)
					{
						diag += "OK: the file seems to contain " + elementCount + " " + elementName + "." + "\n\nClick \"Next>>\" to continue";
						m_fileImportForm.nextButton.Enabled = true;
						m_fileImportForm.fileStatusLabel.ForeColor = Color.Black;
						ret = true;
					}
					else
					{
						diag += "Error: the file seems to contain no " + elementName + ".\nIt is probably in other format than you have selected."
							+ "\n\nClick \"<<Back\" to select another format";
						m_fileImportForm.nextButton.Enabled = false;
						m_fileImportForm.fileStatusLabel.ForeColor = Color.Red;
					}
				}
				
				m_fileImportForm.fileStatusLabel.Text = diag;
				return ret;
			}
		}

		public class ImportTab
		{
			public FileImportForm m_fileImportForm;

			public ImportTab(FileImportForm fileImportForm)
			{
				m_fileImportForm = fileImportForm;
			}

			public void tabActivated()
			{
				if(FileImportForm.m_selectedFormat == null || FileImportForm.m_selectedFormat.Length == 0)
				{
					m_fileImportForm.tabControl1.SelectedIndex = 1;	// make sure format is valid
				}
				else if(m_fileImportForm.m_selectedFileName == null
					|| m_fileImportForm.m_selectedFileName.Length == 0
					|| !File.Exists(m_fileImportForm.m_selectedFileName))
				{
					m_fileImportForm.tabControl1.SelectedIndex = 2;	// make sure file name is valid
				}
				else
				{
					m_fileImportForm.importFileNameLabel.Text = m_fileImportForm.m_selectedFileName;
					m_fileImportForm.importFormatLabel.Text = FileImportForm.m_selectedFormat;

					// make sure that if we are having repeat read, the checkbox reflects persistence status of the original file:
					string filename = m_fileImportForm.m_selectedFileName;
					bool persist = false;
					foreach(FormattedFileDescr ffd in Project.FileDescrList)
					{
						if(ffd.filename.Equals(filename))
						{
							persist = ffd.isPersistent;
							break;
						}
					}
					m_fileImportForm.persistCheckBox.Checked = persist;
					m_fileImportForm.zoomInCheckBox.Checked = FileImportForm.doZoomIn;
				}
			}
		}

		public class FinishTab
		{
			public FileImportForm m_fileImportForm;

			public FinishTab(FileImportForm fileImportForm)
			{
				m_fileImportForm = fileImportForm;
			}

			public void tabActivated()
			{
				m_fileImportForm.m_selectedFileName = m_fileImportForm.fileTextBox.Text;
				doRead(FileImportForm.m_mode);
			}

			/// <summary>
			/// Calls format.process() and then FileAndZipIO.processPhotoFolderByFile(...true), (true for reading photoParameters file) 
			/// </summary>
			public void doRead(int mode)
			{
				Cursor.Current = Cursors.WaitCursor;
				string filename = m_fileImportForm.m_selectedFileName;

				bool repeatRead = false;
				string sRepeatRead = "";
				FormattedFileDescr ffdRepeat = null;
				foreach(FormattedFileDescr ffd in Project.FileDescrList)
				{
					if(ffd.filename.Equals(filename))
					{
						repeatRead = true;
						ffdRepeat = ffd;
						sRepeatRead = " (repeat read)";
						switch(mode)
						{
							case MODE_GPS:
								WaypointsCache.RemoveWaypointsBySource(filename);
								CustomMapsCache.RemoveCustomMapsBySource(filename);
								break;
							case MODE_EQ:
								EarthquakesCache.RemoveEarthquakesBySource(filename);
								break;
						}
						LayerWaypoints.This.init();		// provoke PutOnMap on layer's Paint()
						m_pictureManager.Refresh();
						break;
					}
				}

				// actually read the file. If ZIP processor is there, it will read all enclosed .GPX files.
				WaypointsCache.resetBoundaries();

				string formatName = FileImportForm.m_selectedFormat; //m_fileImportForm.detailComboBox.Text;

				string diag = "Selected format: " + formatName + sRepeatRead + "\n\n"; 

				if(AllFormats.isTiffFile(filename))
				{
					string[] fileNames = new string[] { filename };

					bool anySuccess = FileAndZipIO.readFiles(fileNames);

					if(anySuccess)
					{
						elementCount = 1;
						elementName = "GeoTIFF image"; 
					}
				}
				else
				{
					elementCount = 0;
					m_fileProcessor("", filename, filename);

					switch(mode)
					{
						case MODE_GPS:
							FileAndZipIO.processPhotoFolderByFile(filename, null);	// use default photo files processor and read in parameters file
							break;
					}
				}

				if(elementCount > 0)
				{
					diag += "OK: read " + elementCount + " " + elementName + "."
						+ "\n\nClick \"Next>>\" to see list of imported files";
					m_fileImportForm.nextButton.Enabled = true;
					LibSys.StatusBar.Trace("OK: read file " + filename + " containing " + elementCount + " " + elementName + (repeatRead ? " (repeat read)" : ""));

					if(repeatRead)
					{
						ffdRepeat.isPersistent = m_fileImportForm.persistCheckBox.Checked;
					}
					else
					{
						FileInfo fi = new FileInfo(Project.GetLongPathName(m_fileImportForm.m_selectedFileName));
						FormattedFileDescr fd = new FormattedFileDescr(fi.FullName, formatName, m_fileImportForm.persistCheckBox.Checked);
						Project.FileDescrListAdd(fd);
					}
					m_fileImportForm.diagLabel.ForeColor = Color.Black;

					switch(mode)
					{
						case MODE_GPS:
							Project.mainCommand.wptEnabled(true);
							WaypointsCache.RefreshWaypointsDisplayed();
							if(doZoomIn)
							{
								m_pictureManager.CameraManager.zoomToCorners();
							}
							Project.insertRecentFile(filename);
							break;
						case MODE_EQ:
							Project.mainCommand.eqEnabled(true);
							EarthquakesCache.RefreshEarthquakesDisplayed();
							break;
					}
					LayerWaypoints.This.init();		// provoke PutOnMap on layer's Paint()
					m_pictureManager.Refresh();
				}
				else
				{
					diag += "Error: the file seems to contain no " + elementName + ".\nIt is probably in other format than you have selected."
						+ "\n\nClick \"<<Back\" to select another format";
					m_fileImportForm.nextButton.Enabled = false;
					m_fileImportForm.diagLabel.ForeColor = Color.Red;
					LibSys.StatusBar.Error("the file " + filename + " seems to contain no " + elementName);
				}
				
				m_fileImportForm.diagLabel.Text = diag;
				Cursor.Current = Cursors.Default;
			}
		}
	}
}
