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
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Configuration;
using System.Threading;
using JSG.PhotoPropertiesLibrary;

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPhotoDraw.
	/// </summary>
	public class DlgPhotoDraw : System.Windows.Forms.Form
	{
		private Waypoint m_wpt;
		private PhotoDescr m_photoDescr;
		private Bitmap originalBitmap;
		private Bitmap m_bitmap;
		private int originalOrientation;
		private bool m_useOriginal = true;

		private bool m_sizeChanged = false;		// all false indicates that lossless EXIF saving is possible.
		private bool m_hasDrawn = false;		//
		private bool m_hasRotated = false;		//

		private double whRatio;
		private static bool m_fitToSize = true;
		private static bool m_useBgColor = false;
		private static bool m_addExifCoords = true;
		private static int m_fontSize = 10;		// will adjust to picture size
		private static Color m_color = Color.Red;
		private static Color m_bgColor = Color.LightGray;
		private string m_savedFileName = null;

		private static int m_position = 0;
//			"Top Left"			0
//			"Top Middle"		1
//			"Top Right"			2
//			"Bottom Left"		3
//			"Bottom Middle"		4
//			"Bottom Right"		5

		private bool inResize = false;
		private ASCIIEncoding enc = new ASCIIEncoding();

		private LibSys.PhotoViewerControl photoViewerControl;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

		#region PhotoProperties related definitions

		/// <summary>An instance of the PhotoProperties class</summary>
		private PhotoProperties _photoProps;
		/// <summary>Has the PhotoProperties instance successfully initialized?</summary>
		private bool _photoPropsInitialized = false;
		/// <summary>An instance of the ResultOptions class defining 
		/// properties used when creating XML output.</summary>
		private ResultOptions _resultOptions;
		/// <summary>The ListViewColumnSorter for the _listView control.</summary>
		private ListViewColumnSorter lvwColumnSorter;
		/// <summary>Was the current analysis successful?</summary>
		private bool _isAnalysisSuccessful = false;

		/// <summary>Define the InitializePhotoPropertiesDelegate delegate signature.</summary>
		private delegate bool InitializePhotoPropertiesDelegate(string xmlFileName);

		private ArrayList m_filesToDelete = new ArrayList();

		#endregion // PhotoProperties related definitions

		#region Controls defined here

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage pictureTabPage;
		private System.Windows.Forms.TabPage exifTabPage;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Panel picturePanel;
		private System.Windows.Forms.Panel exifPanel;
		private System.Windows.Forms.Button applyButton;
		private System.Windows.Forms.CheckBox fitToSizeCheckBox;
		private System.Windows.Forms.Button resetButton;
		private System.Windows.Forms.TextBox widthTextBox;
		private System.Windows.Forms.TextBox heightTextBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.CheckBox addExifCoordsCheckBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox positionComboBox;
		private System.Windows.Forms.ListView _listView;
		private System.Windows.Forms.ColumnHeader _colHdrTag;
		private System.Windows.Forms.ColumnHeader _colHdrCategory;
		private System.Windows.Forms.ColumnHeader _colHdrName;
		private System.Windows.Forms.ColumnHeader _colHdrValue;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Panel exifInfoPanel;
		private System.Windows.Forms.Button cancelButton2;
		private System.Windows.Forms.TextBox _viewDescription;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown fontNumericUpDown;
		private System.Windows.Forms.TextBox textTextBox;
		private System.Windows.Forms.CheckBox applyTextCheckBox;
		private System.Windows.Forms.Button pickColorButton;
		private System.Windows.Forms.ColorDialog textColorDialog;
		private System.Windows.Forms.Panel timePanel;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.RadioButton timeTptRadioButton;
		private System.Windows.Forms.RadioButton timeExifRadioButton;
		private System.Windows.Forms.GroupBox captionGroupBox;
		private System.Windows.Forms.GroupBox transformGroupBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.RadioButton rotate90RadioButton;
		private System.Windows.Forms.GroupBox positionGroupBox;
		private System.Windows.Forms.RadioButton rotate270RadioButton;
		private System.Windows.Forms.RadioButton rotate0RadioButton;
		private System.Windows.Forms.LinkLabel linkToWptLinkLabel;
		private System.Windows.Forms.Button pickBgColorButton;
		private System.Windows.Forms.CheckBox useBgColorCheckBox;
		private System.Windows.Forms.LinkLabel previewLinkLabel;
		private System.Windows.Forms.NumericUpDown qualityNumericUpDown;
		private System.Windows.Forms.Label label8;

		#endregion // Controls defined here

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Constructor and lifecycle

		public DlgPhotoDraw(Waypoint wpt, PhotoDescr photoDescr)
		{
			if(photoDescr != null)
			{
				// from DlgPhotoFullSize, wpt may be null
				m_photoDescr = PhotoDescr.FromPhotoDescr(photoDescr);		// with null image
				m_photoDescr.imageDisposable = false;
				m_photoDescr.ensureOriginalImageExists();		// local will be still invoked if not a gallery
			}
			else
			{
				// from right-click on a waypoint; wpt must be non-null
				try 
				{
					m_photoDescr = PhotoDescr.FromThumbnail(wpt.ThumbSource);
					m_photoDescr.imageDisposable = false;
					m_photoDescr.ensureOriginalImageExists();		// local will be still invoked if not a gallery
				}
					// An invalid image will throw an OutOfMemoryException 
					// exception
				catch (OutOfMemoryException) 
				{
					throw new Exception("Corrupted image file?");
				}
			}
			originalOrientation = m_photoDescr.Orientation;
			m_photoDescr.Orientation = 1;			// will force display to actual orientation, no adjustment
			originalBitmap = new Bitmap(m_photoDescr.image);
			whRatio = (double)originalBitmap.Width / (double)originalBitmap.Height;

			foreach(PropertyItem item in m_photoDescr.image.PropertyItems)
			{
				//string str = "" + item.Type + ": " + item.Id + "=" + item.Value;
				//LibSys.StatusBar.Trace(str);
				originalBitmap.SetPropertyItem(item);
			}

			m_wpt = wpt;

			InitializeComponent();

			addExifCoordsCheckBox.Enabled = (m_wpt != null);
			addExifCoordsCheckBox.Checked = m_addExifCoords;

			this.linkToWptLinkLabel.Enabled = (m_wpt == null || m_wpt.TrackId == -1);

			this.photoViewerControl = new LibSys.PhotoViewerControl();
			this.picturePanel.Controls.AddRange(new System.Windows.Forms.Control[] { this.photoViewerControl});
			// 
			// photoViewerControl
			// 
			this.photoViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.photoViewerControl.Name = "photoViewerControl";
			//this.photoViewerControl.photoDescr = null;
			this.photoViewerControl.TabIndex = 1;

			photoViewerControl.fitToSize = m_fitToSize;
			fitToSizeCheckBox.Checked = m_fitToSize;
			this.fitToSizeCheckBox.CheckedChanged += new System.EventHandler(this.fitToSizeCheckBox_CheckedChanged);

			if(m_wpt != null && m_wpt.TrackId != -1)
			{
				timeTptRadioButton.Checked = true;
				this.timePanel.Enabled = true;
			}
			else
			{
				timeExifRadioButton.Checked = true;
				timePanel.Enabled = false;
			}

			setTextTextBox();

			widthTextBox.Text = "" + originalBitmap.Width;		// Y will be set in validation
			widthTextBox_Validating(null, null);

			draw(false);

			if(Project.fitsScreen(m_dlgSizeX, m_dlgSizeY, m_dlgSizeWidth, m_dlgSizeHeight))
			{
				inResize = true;
				this.Location = new Point(m_dlgSizeX, m_dlgSizeY);
				this.ClientSize = new System.Drawing.Size(m_dlgSizeWidth, m_dlgSizeHeight);		// causes Resize()
			}

			// PhotoProperties related:
			// Create an instance of a ListView column sorter and 
			// assign it to the _listView control.
			lvwColumnSorter = new ListViewColumnSorter();
			this._listView.ListViewItemSorter = lvwColumnSorter;

			_resultOptions = new ResultOptions();

			// end of PhotoProperties related.

			positionComboBox.SelectedIndex = m_position;
			this.positionComboBox.SelectedIndexChanged += new System.EventHandler(this.positionComboBox_SelectedIndexChanged);

			m_fontSize = (int)Math.Round(Math.Max(originalBitmap.Width / 50.0d, m_fontSize));
			fontNumericUpDown.Value = Math.Min(m_fontSize, fontNumericUpDown.Maximum);

			qualityNumericUpDown.Value = Project.photoSaveQuality;
			this.qualityNumericUpDown.ValueChanged += new System.EventHandler(this.qualityNumericUpDown_ValueChanged);

			applyTextCheckBox.Checked = true;
			pickColorButton.BackColor = m_color;
			pickBgColorButton.BackColor = m_bgColor;
			this.useBgColorCheckBox.Checked = m_useBgColor;

			this.Text += " - " + m_photoDescr.imageName;

			switch(originalOrientation)
			{
					// see PhotoDescr.cs for EXIF orientation codes:
//				case 4:
//					//thumb.RotateFlip(RotateFlipType.Rotate180FlipNone);
//					break;
				case 8:
					this.rotate270RadioButton.Checked = true;
					//thumb.RotateFlip(RotateFlipType.Rotate270FlipNone);
					break;
				case 6:
					this.rotate90RadioButton.Checked = true;
					//thumb.RotateFlip(RotateFlipType.Rotate90FlipNone);
					break;
				default:
					this.rotate0RadioButton.Checked = true;
					break;
			}

			this.saveButton.Enabled = false;
			this.previewLinkLabel.Enabled = false;

			Project.setDlgIcon(this);
		}

		private void DlgPhotoDraw_Load(object sender, System.EventArgs e)
		{
			// Get the configuration settings (from the <app>.exe.config file)
			string initXmlFile = Project.GetMiscPath("PhotoMetadata.xml");
			_resultOptions.XMLNamespace = "http://tempuri.org/PhotoPropertyOutput.xsd";
			_resultOptions.XSLTransform = Project.GetMiscPath("PhotoPropertyOutput.xslt");

			InitializePhotoProperties(initXmlFile);

			inResize = false;
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

		#endregion // Constructor and lifecycle

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.pictureTabPage = new System.Windows.Forms.TabPage();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.label8 = new System.Windows.Forms.Label();
			this.qualityNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.previewLinkLabel = new System.Windows.Forms.LinkLabel();
			this.positionGroupBox = new System.Windows.Forms.GroupBox();
			this.linkToWptLinkLabel = new System.Windows.Forms.LinkLabel();
			this.addExifCoordsCheckBox = new System.Windows.Forms.CheckBox();
			this.transformGroupBox = new System.Windows.Forms.GroupBox();
			this.rotate0RadioButton = new System.Windows.Forms.RadioButton();
			this.rotate270RadioButton = new System.Windows.Forms.RadioButton();
			this.rotate90RadioButton = new System.Windows.Forms.RadioButton();
			this.label7 = new System.Windows.Forms.Label();
			this.heightTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.widthTextBox = new System.Windows.Forms.TextBox();
			this.captionGroupBox = new System.Windows.Forms.GroupBox();
			this.useBgColorCheckBox = new System.Windows.Forms.CheckBox();
			this.pickBgColorButton = new System.Windows.Forms.Button();
			this.pickColorButton = new System.Windows.Forms.Button();
			this.timePanel = new System.Windows.Forms.Panel();
			this.timeExifRadioButton = new System.Windows.Forms.RadioButton();
			this.timeTptRadioButton = new System.Windows.Forms.RadioButton();
			this.label6 = new System.Windows.Forms.Label();
			this.positionComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textTextBox = new System.Windows.Forms.TextBox();
			this.fontNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.applyTextCheckBox = new System.Windows.Forms.CheckBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.fitToSizeCheckBox = new System.Windows.Forms.CheckBox();
			this.saveButton = new System.Windows.Forms.Button();
			this.resetButton = new System.Windows.Forms.Button();
			this.applyButton = new System.Windows.Forms.Button();
			this.picturePanel = new System.Windows.Forms.Panel();
			this.exifTabPage = new System.Windows.Forms.TabPage();
			this.exifPanel = new System.Windows.Forms.Panel();
			this.exifInfoPanel = new System.Windows.Forms.Panel();
			this.label5 = new System.Windows.Forms.Label();
			this._viewDescription = new System.Windows.Forms.TextBox();
			this.cancelButton2 = new System.Windows.Forms.Button();
			this._listView = new System.Windows.Forms.ListView();
			this._colHdrTag = new System.Windows.Forms.ColumnHeader();
			this._colHdrCategory = new System.Windows.Forms.ColumnHeader();
			this._colHdrName = new System.Windows.Forms.ColumnHeader();
			this._colHdrValue = new System.Windows.Forms.ColumnHeader();
			this.textColorDialog = new System.Windows.Forms.ColorDialog();
			this.tabControl1.SuspendLayout();
			this.pictureTabPage.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.qualityNumericUpDown)).BeginInit();
			this.positionGroupBox.SuspendLayout();
			this.transformGroupBox.SuspendLayout();
			this.captionGroupBox.SuspendLayout();
			this.timePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fontNumericUpDown)).BeginInit();
			this.exifTabPage.SuspendLayout();
			this.exifPanel.SuspendLayout();
			this.exifInfoPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.pictureTabPage,
																					  this.exifTabPage});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(582, 486);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// pictureTabPage
			// 
			this.pictureTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.buttonPanel,
																						 this.picturePanel});
			this.pictureTabPage.Location = new System.Drawing.Point(4, 22);
			this.pictureTabPage.Name = "pictureTabPage";
			this.pictureTabPage.Size = new System.Drawing.Size(574, 460);
			this.pictureTabPage.TabIndex = 0;
			this.pictureTabPage.Text = "Picture";
			// 
			// buttonPanel
			// 
			this.buttonPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.buttonPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.label8,
																					  this.qualityNumericUpDown,
																					  this.previewLinkLabel,
																					  this.positionGroupBox,
																					  this.transformGroupBox,
																					  this.captionGroupBox,
																					  this.cancelButton,
																					  this.fitToSizeCheckBox,
																					  this.saveButton,
																					  this.resetButton,
																					  this.applyButton});
			this.buttonPanel.Location = new System.Drawing.Point(0, 265);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(572, 195);
			this.buttonPanel.TabIndex = 1;
			// 
			// label8
			// 
			this.label8.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.label8.Location = new System.Drawing.Point(500, 99);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(25, 18);
			this.label8.TabIndex = 433;
			this.label8.Text = "Q:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// qualityNumericUpDown
			// 
			this.qualityNumericUpDown.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.qualityNumericUpDown.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.qualityNumericUpDown.Increment = new System.Decimal(new int[] {
																				   5,
																				   0,
																				   0,
																				   0});
			this.qualityNumericUpDown.Location = new System.Drawing.Point(525, 99);
			this.qualityNumericUpDown.Minimum = new System.Decimal(new int[] {
																				 30,
																				 0,
																				 0,
																				 0});
			this.qualityNumericUpDown.Name = "qualityNumericUpDown";
			this.qualityNumericUpDown.Size = new System.Drawing.Size(45, 13);
			this.qualityNumericUpDown.TabIndex = 432;
			this.qualityNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.qualityNumericUpDown.Value = new System.Decimal(new int[] {
																			   30,
																			   0,
																			   0,
																			   0});
			// 
			// previewLinkLabel
			// 
			this.previewLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.previewLinkLabel.Location = new System.Drawing.Point(500, 145);
			this.previewLinkLabel.Name = "previewLinkLabel";
			this.previewLinkLabel.Size = new System.Drawing.Size(70, 20);
			this.previewLinkLabel.TabIndex = 431;
			this.previewLinkLabel.TabStop = true;
			this.previewLinkLabel.Text = "preview";
			this.previewLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.previewLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.previewLinkLabel_LinkClicked);
			// 
			// positionGroupBox
			// 
			this.positionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.positionGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.linkToWptLinkLabel,
																						   this.addExifCoordsCheckBox});
			this.positionGroupBox.Location = new System.Drawing.Point(5, 150);
			this.positionGroupBox.Name = "positionGroupBox";
			this.positionGroupBox.Size = new System.Drawing.Size(485, 40);
			this.positionGroupBox.TabIndex = 30;
			this.positionGroupBox.TabStop = false;
			this.positionGroupBox.Text = "Position";
			// 
			// linkToWptLinkLabel
			// 
			this.linkToWptLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.linkToWptLinkLabel.Location = new System.Drawing.Point(300, 15);
			this.linkToWptLinkLabel.Name = "linkToWptLinkLabel";
			this.linkToWptLinkLabel.Size = new System.Drawing.Size(175, 20);
			this.linkToWptLinkLabel.TabIndex = 310;
			this.linkToWptLinkLabel.TabStop = true;
			this.linkToWptLinkLabel.Text = "associate with waypoint";
			this.linkToWptLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.linkToWptLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkToWptLinkLabel_LinkClicked);
			// 
			// addExifCoordsCheckBox
			// 
			this.addExifCoordsCheckBox.Location = new System.Drawing.Point(15, 15);
			this.addExifCoordsCheckBox.Name = "addExifCoordsCheckBox";
			this.addExifCoordsCheckBox.Size = new System.Drawing.Size(275, 20);
			this.addExifCoordsCheckBox.TabIndex = 300;
			this.addExifCoordsCheckBox.Text = "Add EXIF GPS Coordinates tags";
			this.addExifCoordsCheckBox.CheckedChanged += new System.EventHandler(this.addExifCoordsCheckBox_CheckedChanged);
			// 
			// transformGroupBox
			// 
			this.transformGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.transformGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.rotate0RadioButton,
																							this.rotate270RadioButton,
																							this.rotate90RadioButton,
																							this.label7,
																							this.heightTextBox,
																							this.label1,
																							this.widthTextBox});
			this.transformGroupBox.Location = new System.Drawing.Point(5, 100);
			this.transformGroupBox.Name = "transformGroupBox";
			this.transformGroupBox.Size = new System.Drawing.Size(485, 45);
			this.transformGroupBox.TabIndex = 20;
			this.transformGroupBox.TabStop = false;
			this.transformGroupBox.Text = "Transform";
			// 
			// rotate0RadioButton
			// 
			this.rotate0RadioButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.rotate0RadioButton.Location = new System.Drawing.Point(420, 15);
			this.rotate0RadioButton.Name = "rotate0RadioButton";
			this.rotate0RadioButton.Size = new System.Drawing.Size(60, 20);
			this.rotate0RadioButton.TabIndex = 230;
			this.rotate0RadioButton.Text = "none";
			this.rotate0RadioButton.CheckedChanged += new System.EventHandler(this.rotateRadioButton_CheckedChanged);
			// 
			// rotate270RadioButton
			// 
			this.rotate270RadioButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.rotate270RadioButton.Location = new System.Drawing.Point(340, 15);
			this.rotate270RadioButton.Name = "rotate270RadioButton";
			this.rotate270RadioButton.Size = new System.Drawing.Size(80, 20);
			this.rotate270RadioButton.TabIndex = 220;
			this.rotate270RadioButton.Text = "270 - left";
			this.rotate270RadioButton.CheckedChanged += new System.EventHandler(this.rotateRadioButton_CheckedChanged);
			// 
			// rotate90RadioButton
			// 
			this.rotate90RadioButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.rotate90RadioButton.Location = new System.Drawing.Point(260, 15);
			this.rotate90RadioButton.Name = "rotate90RadioButton";
			this.rotate90RadioButton.Size = new System.Drawing.Size(80, 20);
			this.rotate90RadioButton.TabIndex = 210;
			this.rotate90RadioButton.Text = "90 - right";
			this.rotate90RadioButton.CheckedChanged += new System.EventHandler(this.rotateRadioButton_CheckedChanged);
			// 
			// label7
			// 
			this.label7.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.label7.Location = new System.Drawing.Point(205, 15);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(50, 20);
			this.label7.TabIndex = 108;
			this.label7.Text = "Rotate:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// heightTextBox
			// 
			this.heightTextBox.Location = new System.Drawing.Point(110, 15);
			this.heightTextBox.Name = "heightTextBox";
			this.heightTextBox.ReadOnly = true;
			this.heightTextBox.Size = new System.Drawing.Size(40, 20);
			this.heightTextBox.TabIndex = 201;
			this.heightTextBox.TabStop = false;
			this.heightTextBox.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(10, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 20);
			this.label1.TabIndex = 107;
			this.label1.Text = "Resize:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// widthTextBox
			// 
			this.widthTextBox.Location = new System.Drawing.Point(65, 15);
			this.widthTextBox.Name = "widthTextBox";
			this.widthTextBox.Size = new System.Drawing.Size(40, 20);
			this.widthTextBox.TabIndex = 200;
			this.widthTextBox.Text = "";
			this.widthTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.widthTextBox_Validating);
			this.widthTextBox.TextChanged += new System.EventHandler(this.widthTextBox_TextChanged);
			// 
			// captionGroupBox
			// 
			this.captionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.captionGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.useBgColorCheckBox,
																						  this.pickBgColorButton,
																						  this.pickColorButton,
																						  this.timePanel,
																						  this.positionComboBox,
																						  this.label2,
																						  this.label4,
																						  this.label3,
																						  this.textTextBox,
																						  this.fontNumericUpDown,
																						  this.applyTextCheckBox});
			this.captionGroupBox.Location = new System.Drawing.Point(5, 5);
			this.captionGroupBox.Name = "captionGroupBox";
			this.captionGroupBox.Size = new System.Drawing.Size(485, 90);
			this.captionGroupBox.TabIndex = 10;
			this.captionGroupBox.TabStop = false;
			this.captionGroupBox.Text = "Caption";
			// 
			// useBgColorCheckBox
			// 
			this.useBgColorCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.useBgColorCheckBox.Location = new System.Drawing.Point(385, 65);
			this.useBgColorCheckBox.Name = "useBgColorCheckBox";
			this.useBgColorCheckBox.Size = new System.Drawing.Size(16, 20);
			this.useBgColorCheckBox.TabIndex = 152;
			this.useBgColorCheckBox.CheckedChanged += new System.EventHandler(this.useBgColorCheckBox_CheckedChanged);
			// 
			// pickBgColorButton
			// 
			this.pickBgColorButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.pickBgColorButton.Location = new System.Drawing.Point(405, 65);
			this.pickBgColorButton.Name = "pickBgColorButton";
			this.pickBgColorButton.Size = new System.Drawing.Size(70, 20);
			this.pickBgColorButton.TabIndex = 151;
			this.pickBgColorButton.Text = "Bg color";
			this.pickBgColorButton.Click += new System.EventHandler(this.pickBgColorButton_Click);
			// 
			// pickColorButton
			// 
			this.pickColorButton.Location = new System.Drawing.Point(130, 15);
			this.pickColorButton.Name = "pickColorButton";
			this.pickColorButton.Size = new System.Drawing.Size(50, 20);
			this.pickColorButton.TabIndex = 120;
			this.pickColorButton.Text = "Color";
			this.pickColorButton.Click += new System.EventHandler(this.pickColorButton_Click);
			// 
			// timePanel
			// 
			this.timePanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.timeExifRadioButton,
																					this.timeTptRadioButton,
																					this.label6});
			this.timePanel.Location = new System.Drawing.Point(5, 65);
			this.timePanel.Name = "timePanel";
			this.timePanel.Size = new System.Drawing.Size(310, 20);
			this.timePanel.TabIndex = 150;
			// 
			// timeExifRadioButton
			// 
			this.timeExifRadioButton.Location = new System.Drawing.Point(200, 0);
			this.timeExifRadioButton.Name = "timeExifRadioButton";
			this.timeExifRadioButton.Size = new System.Drawing.Size(104, 20);
			this.timeExifRadioButton.TabIndex = 170;
			this.timeExifRadioButton.Text = "from EXIF";
			this.timeExifRadioButton.CheckedChanged += new System.EventHandler(this.timeRadioButtons_CheckedChanged);
			// 
			// timeTptRadioButton
			// 
			this.timeTptRadioButton.Location = new System.Drawing.Point(75, 0);
			this.timeTptRadioButton.Name = "timeTptRadioButton";
			this.timeTptRadioButton.Size = new System.Drawing.Size(125, 20);
			this.timeTptRadioButton.TabIndex = 160;
			this.timeTptRadioButton.Text = "from trackpoint";
			this.timeTptRadioButton.CheckedChanged += new System.EventHandler(this.timeRadioButtons_CheckedChanged);
			// 
			// label6
			// 
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(70, 20);
			this.label6.TabIndex = 109;
			this.label6.Text = "Text time:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// positionComboBox
			// 
			this.positionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.positionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.positionComboBox.Items.AddRange(new object[] {
																  "Top Left",
																  "Top Middle",
																  "Top Right",
																  "Bottom Left",
																  "Bottom Middle",
																  "Bottom Right"});
			this.positionComboBox.Location = new System.Drawing.Point(290, 15);
			this.positionComboBox.Name = "positionComboBox";
			this.positionComboBox.Size = new System.Drawing.Size(185, 21);
			this.positionComboBox.TabIndex = 130;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(10, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(50, 20);
			this.label2.TabIndex = 108;
			this.label2.Text = "Text:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(220, 15);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 20);
			this.label4.TabIndex = 111;
			this.label4.Text = "Position:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(25, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(35, 20);
			this.label3.TabIndex = 109;
			this.label3.Text = "Font:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textTextBox
			// 
			this.textTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.textTextBox.Location = new System.Drawing.Point(65, 40);
			this.textTextBox.Name = "textTextBox";
			this.textTextBox.Size = new System.Drawing.Size(410, 20);
			this.textTextBox.TabIndex = 140;
			this.textTextBox.Text = "";
			// 
			// fontNumericUpDown
			// 
			this.fontNumericUpDown.Location = new System.Drawing.Point(65, 15);
			this.fontNumericUpDown.Maximum = new System.Decimal(new int[] {
																			  200,
																			  0,
																			  0,
																			  0});
			this.fontNumericUpDown.Minimum = new System.Decimal(new int[] {
																			  6,
																			  0,
																			  0,
																			  0});
			this.fontNumericUpDown.Name = "fontNumericUpDown";
			this.fontNumericUpDown.Size = new System.Drawing.Size(50, 20);
			this.fontNumericUpDown.TabIndex = 110;
			this.fontNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.fontNumericUpDown.Value = new System.Decimal(new int[] {
																			10,
																			0,
																			0,
																			0});
			this.fontNumericUpDown.ValueChanged += new System.EventHandler(this.fontNumericUpDown_ValueChanged);
			// 
			// applyTextCheckBox
			// 
			this.applyTextCheckBox.Location = new System.Drawing.Point(5, 15);
			this.applyTextCheckBox.Name = "applyTextCheckBox";
			this.applyTextCheckBox.Size = new System.Drawing.Size(16, 20);
			this.applyTextCheckBox.TabIndex = 100;
			this.applyTextCheckBox.CheckedChanged += new System.EventHandler(this.applyTextCheckBox_CheckedChanged);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(497, 170);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 20);
			this.cancelButton.TabIndex = 430;
			this.cancelButton.Text = "Cancel";
			// 
			// fitToSizeCheckBox
			// 
			this.fitToSizeCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.fitToSizeCheckBox.Location = new System.Drawing.Point(520, 5);
			this.fitToSizeCheckBox.Name = "fitToSizeCheckBox";
			this.fitToSizeCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.fitToSizeCheckBox.Size = new System.Drawing.Size(45, 20);
			this.fitToSizeCheckBox.TabIndex = 50;
			this.fitToSizeCheckBox.Text = "Fit";
			// 
			// saveButton
			// 
			this.saveButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.saveButton.Location = new System.Drawing.Point(497, 120);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(75, 20);
			this.saveButton.TabIndex = 420;
			this.saveButton.Text = "Save As...";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// resetButton
			// 
			this.resetButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.resetButton.Location = new System.Drawing.Point(497, 45);
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size(75, 20);
			this.resetButton.TabIndex = 400;
			this.resetButton.Text = "Reset";
			this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
			// 
			// applyButton
			// 
			this.applyButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.applyButton.Location = new System.Drawing.Point(497, 70);
			this.applyButton.Name = "applyButton";
			this.applyButton.Size = new System.Drawing.Size(75, 20);
			this.applyButton.TabIndex = 410;
			this.applyButton.Text = "Apply";
			this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
			// 
			// picturePanel
			// 
			this.picturePanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.picturePanel.Name = "picturePanel";
			this.picturePanel.Size = new System.Drawing.Size(574, 265);
			this.picturePanel.TabIndex = 0;
			// 
			// exifTabPage
			// 
			this.exifTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.exifPanel});
			this.exifTabPage.Location = new System.Drawing.Point(4, 22);
			this.exifTabPage.Name = "exifTabPage";
			this.exifTabPage.Size = new System.Drawing.Size(574, 460);
			this.exifTabPage.TabIndex = 1;
			this.exifTabPage.Text = "EXIF";
			// 
			// exifPanel
			// 
			this.exifPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.exifInfoPanel,
																					this._listView});
			this.exifPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exifPanel.Name = "exifPanel";
			this.exifPanel.Size = new System.Drawing.Size(574, 460);
			this.exifPanel.TabIndex = 0;
			// 
			// exifInfoPanel
			// 
			this.exifInfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.exifInfoPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.label5,
																						this._viewDescription,
																						this.cancelButton2});
			this.exifInfoPanel.Location = new System.Drawing.Point(-5, 360);
			this.exifInfoPanel.Name = "exifInfoPanel";
			this.exifInfoPanel.Size = new System.Drawing.Size(580, 100);
			this.exifInfoPanel.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(10, 75);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(480, 23);
			this.label5.TabIndex = 108;
			this.label5.Text = "EXIF viewer courtesy PhotoProperties project by Jeffrey S. Gangel ";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _viewDescription
			// 
			this._viewDescription.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this._viewDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._viewDescription.Location = new System.Drawing.Point(10, 5);
			this._viewDescription.Multiline = true;
			this._viewDescription.Name = "_viewDescription";
			this._viewDescription.ReadOnly = true;
			this._viewDescription.Size = new System.Drawing.Size(565, 65);
			this._viewDescription.TabIndex = 107;
			this._viewDescription.Text = "";
			// 
			// cancelButton2
			// 
			this.cancelButton2.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.cancelButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton2.Location = new System.Drawing.Point(502, 76);
			this.cancelButton2.Name = "cancelButton2";
			this.cancelButton2.Size = new System.Drawing.Size(75, 20);
			this.cancelButton2.TabIndex = 106;
			this.cancelButton2.Text = "Cancel";
			// 
			// _listView
			// 
			this._listView.AllowColumnReorder = true;
			this._listView.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this._colHdrTag,
																						this._colHdrCategory,
																						this._colHdrName,
																						this._colHdrValue});
			this._listView.FullRowSelect = true;
			this._listView.GridLines = true;
			this._listView.MultiSelect = false;
			this._listView.Name = "_listView";
			this._listView.Size = new System.Drawing.Size(577, 359);
			this._listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this._listView.TabIndex = 1;
			this._listView.View = System.Windows.Forms.View.Details;
			this._listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this._listView_ColumnClick);
			this._listView.SelectedIndexChanged += new System.EventHandler(this._listView_SelectedIndexChanged);
			// 
			// _colHdrTag
			// 
			this._colHdrTag.Text = "Tag";
			// 
			// _colHdrCategory
			// 
			this._colHdrCategory.Text = "Category";
			this._colHdrCategory.Width = 100;
			// 
			// _colHdrName
			// 
			this._colHdrName.Text = "Name";
			this._colHdrName.Width = 175;
			// 
			// _colHdrValue
			// 
			this._colHdrValue.Text = "Value";
			this._colHdrValue.Width = 250;
			// 
			// DlgPhotoDraw
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(582, 486);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(550, 390);
			this.Name = "DlgPhotoDraw";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Draw Text on Photo";
			this.TopMost = true;
			this.Resize += new System.EventHandler(this.DlgPhotoDraw_Resize);
			this.Load += new System.EventHandler(this.DlgPhotoDraw_Load);
			this.Move += new System.EventHandler(this.DlgPhotoDraw_Move);
			this.Closed += new System.EventHandler(this.DlgPhotoDraw_Closed);
			this.tabControl1.ResumeLayout(false);
			this.pictureTabPage.ResumeLayout(false);
			this.buttonPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.qualityNumericUpDown)).EndInit();
			this.positionGroupBox.ResumeLayout(false);
			this.transformGroupBox.ResumeLayout(false);
			this.captionGroupBox.ResumeLayout(false);
			this.timePanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.fontNumericUpDown)).EndInit();
			this.exifTabPage.ResumeLayout(false);
			this.exifPanel.ResumeLayout(false);
			this.exifInfoPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region PhotoProperties related

		/// <summary>
		/// Initializes the PhotoProperties tag properties using asynchronous 
		/// method invocation of the initialization.</summary>
		private void InitializePhotoProperties(string initXmlFile) 
		{
			// Create an instance of the PhotoProperties
			_photoProps = new PhotoProperties();

			// Use the asynchronous method invocation of the initialization delegate
			// to initialize the photo properties in a worker thread.
			InitializePhotoPropertiesDelegate initDelegate = 
				new InitializePhotoPropertiesDelegate(_photoProps.Initialize);
			AsyncCallback callback = new AsyncCallback(InitializePhotoPropertiesCompleted);
			initDelegate.BeginInvoke(initXmlFile, callback, initDelegate);
		}

		/// <summary>
		/// The callback method that is automatically called 
		/// when the async invoke has completed.</summary>
		/// <remarks>This function will be called on the pool thread.</remarks>
		public void InitializePhotoPropertiesCompleted(IAsyncResult call) 
		{
			InitializePhotoPropertiesDelegate init = (InitializePhotoPropertiesDelegate)call.AsyncState;
			bool isInitialized = init.EndInvoke(call);

			if (isInitialized) 
			{
				// Raise the continue loading 'event'
				BeginInvoke(new MethodInvoker(ContinueLoadingEvent));
			}
			else 
			{
				string errMessage = _photoProps.GetInitializeErrorMessage();
				if (errMessage == String.Empty)
					errMessage = "An unknown error occurred.";

				MessageBox.Show(errMessage,
					Project.PROGRAM_NAME_HUMAN,
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error); 

				// close the dialog
				this.Close();
			}
		}

		/// <summary>
		/// This 'event' is raised when the initialization of the 
		/// PhotoProperties tag properties completed successfully.</summary>
		/// <remarks>It will be called on the UI thread.</remarks>
		private void ContinueLoadingEvent() 
		{
			// reset the controls
//			this._tbOutput.Text = "";
//			this._tbOutput.Font = TextBox.DefaultFont;
//			this._tbOutput.TextAlign = HorizontalAlignment.Left;
//			this._tbOutput.Enabled = true;

			_photoPropsInitialized = true;
		}

		/// <summary>
		/// Analyzes an image file.
		/// If successful, the result is displayed.</summary>
		private void AnalyzeAndReport()			//string fileName) 
		{
			try 
			{
				Cursor.Current = Cursors.WaitCursor;

				// Clear the text box
//				_tbOutput.Clear();

				// Clear the list view
				_listView.Items.Clear();

				// Analyze the image file and view the XML results.
				_isAnalysisSuccessful = AnalyzeImageFile();			//fileName);
				if (_isAnalysisSuccessful)
				{
					ViewAnalysis();
				}
			}
			catch (System.Exception ex) 
			{
				MessageBox.Show(ex.Message,
					"Analyze Image Exception",
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error); 
			}
			finally 
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Analyzes a given image file.</summary>
		/// <param name="fileName">A valid image file.</param>
		/// <returns>True if the analysis was completed without any errors.</returns>
		private bool AnalyzeImageFile()			//string fileName) 
		{
			bool isAnalyzed = false;
			try 
			{
				string fileName = Path.GetTempFileName();
				m_filesToDelete.Add(fileName);
				if(m_useOriginal)
				{
					originalBitmap.Save(fileName, ImageFormat.Jpeg);
				}
				else
				{
					m_bitmap.Save(fileName, ImageFormat.Jpeg);
				}
				_photoProps.Analyze(fileName);
				isAnalyzed = true;
			}
			catch (InvalidOperationException ex) 
			{
				MessageBox.Show(ex.Message,
					"Analyze Image Exception",
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error); 
			}
			return isAnalyzed;
		}

		/// <summary>
		/// Displays a newly created XML Output of the image file analysis.</summary>
		/// <remarks>The output can be modified through various selections 
		/// of the menu formatting options.</remarks>
		private void ViewAnalysis() 
		{
			// Set the output properties
			_resultOptions.IncludeXSLT = false;		// this._menuItem_AddXSLT.Checked;

			// Create the XML output of the analysis in a memory stream
			MemoryStream memStream = new MemoryStream();
			_photoProps.WriteXml(memStream, _resultOptions);

			// If the stream is closed, create a new stream 
			// with the saved buffer
			if (memStream.CanRead == false) 
			{
				byte[] buf = memStream.GetBuffer();
				memStream = new MemoryStream(buf);
			}

			// Load the stream into an XmlDocument
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			XmlTextReader reader = new XmlTextReader(memStream);
			doc.Load(reader);  

			// Display the XML output
//			_tbOutput.Text = doc.InnerXml;

			// For example the output would include tagDatum elements such as:
			//    <tagDatum id="33434" category="EXIF">
			//      <name>ExposureTime</name>
			//      <description>Exposure time, measured in seconds.</description>
			//      <value>1/4</value>
			//    </tagDatum>

			// Display all the tagDatum nodes in the following four columns:
			//		Tag, Category, Name, and Value.
			// The specific description is shown when a particular tagDatum (row) is selected.
			XmlNodeList elemList = doc.GetElementsByTagName("tagDatum");
			for (int i = 0; i < elemList.Count; i++) 
			{
				XmlNode elem = elemList[i];
				XmlAttributeCollection attrColl = elem.Attributes;
				string tdId = attrColl["id"].Value;
				if (tdId == null || tdId.Length == 0)
					continue;
				string tdCategory = attrColl["category"].Value;
				string tdName = null;
				string tdValue = null;
				string tdDescrip = null;
				string tdPPValue = null;
				for (int j = 0; j < elem.ChildNodes.Count; j++) 
				{
					switch (elem.ChildNodes[j].Name) 
					{
						case "name":
							tdName = elem.ChildNodes[j].InnerXml;
							break;
						case "description":
							tdDescrip = elem.ChildNodes[j].InnerXml;
							break;
						case "value":
							tdValue = elem.ChildNodes[j].InnerXml;
							break;
						case "prettyPrintValue":
							tdPPValue = elem.ChildNodes[j].InnerXml;
							break;
					}
				}
				// View the Id in Hex format
				string tagId = "0x" + Convert.ToInt32(tdId).ToString("X4");
				ListViewItem lvi = _listView.Items.Add(tagId);
				lvi.Tag = tdDescrip;
				// Show the category (or "-")
				if (tdCategory != null && tdCategory != String.Empty)
					_listView.Items[i].SubItems.Add(tdCategory);
				else
					_listView.Items[i].SubItems.Add("-");
				// Show the name
				if (tdName != null)
					_listView.Items[i].SubItems.Add(tdName);
				// Show the result:
				//   If available, show the prettyprint value
				//   Else show the value,
				//   Else show "-".
				if (tdPPValue != null)
					_listView.Items[i].SubItems.Add(tdPPValue);
				else if (tdValue != null)
					_listView.Items[i].SubItems.Add(tdValue);
				else
					_listView.Items[i].SubItems.Add("-");
			}
		}

		#endregion // PhotoProperties related

		#region draw()

		private void draw(bool drawOver)
		{
			if(drawOver)
			{
				string label = textTextBox.Text;
				int width = Convert.ToInt32(widthTextBox.Text);
				int height = Convert.ToInt32(heightTextBox.Text);

				// if size hasn't changed, we have a good shot at lossless save:
				m_sizeChanged = (width != originalBitmap.Width) || (height != originalBitmap.Height);

				if(m_bitmap != null)
				{
					m_bitmap.Dispose();
					m_bitmap = null;
					GC.Collect();
				}

				m_bitmap = new Bitmap(originalBitmap, new Size(width, height));

				if(this.rotate90RadioButton.Checked)
				{
					m_bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
					m_hasRotated = true;
				}
				else if(this.rotate270RadioButton.Checked)
				{
					m_bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
					m_hasRotated = true;
				}

				if(m_hasRotated && applyTextCheckBox.Checked)
				{
					// remove transformation from the picture, or the old clip will be on the way:
					Bitmap tmp = m_bitmap;
					m_bitmap = new Bitmap(m_bitmap);
					tmp.Dispose();
					tmp = null;
					GC.Collect();
				}

				width = m_bitmap.Width;
				height = m_bitmap.Height;

				Graphics graphics = Graphics.FromImage(m_bitmap);

				int margin = 20;

				if(applyTextCheckBox.Checked)
				{
					Font font = Project.getLabelFont(m_fontSize);
					SizeF labelSize = graphics.MeasureString(label, font);
					int labelWidth = (int)Math.Round(labelSize.Width); 
					int labelHeight = (int)Math.Ceiling(labelSize.Height * 0.9f);

					int x = margin;
					int y = margin;

					switch(m_position)
					{
						case 0:			// "Top Left"
							break;
						case 1:			// "Top Middle"
							x = (width - labelWidth) / 2;
							break;
						case 2:			// "Top Right"
							x = width - margin - labelWidth;
							break;
						case 3:			// "Bottom Left"
							y = height - margin - labelHeight;
							break;
						case 4:			// "Bottom Middle"
							y = height - margin - labelHeight;
							x = (width - labelWidth) / 2;
							break;
						case 5:			// "Bottom Right"
							y = height - margin - labelHeight;
							x = width - margin - labelWidth;
							break;
					}

					//graphics.DrawRectangle(new Pen(Color.Yellow, 5.0f), 20, 20, width - 40, height - 40);
					//graphics.DrawString("" + width + " x " + height, font, Brushes.Yellow, width / 2, height / 2);

					if(m_useBgColor)
					{
						Brush bgBrush = new SolidBrush(m_bgColor);

//						graphics.FillRectangle(bgBrush, x, y, (int)(labelWidth * 1.03d), (int)(labelHeight * 1.1d));
//						graphics.FillRectangle(bgBrush, x, y, (int)(labelSize.Width * 1.00d), (int)(labelSize.Height * 1.00d));

						graphics.FillRectangle(bgBrush, x, y, labelWidth, labelHeight);

//						graphics.FillRectangle(bgBrush, x, y, (int)(labelWidth * fontFactor), (int)(labelHeight * 1.1d));

						bgBrush.Dispose();
					}
					Brush brush = new SolidBrush(m_color);
					graphics.DrawString(label, font, brush, x, y);
					brush.Dispose();
					m_hasDrawn = true;
				}

				// see http://www.eggheadcafe.com/articles/20030706.asp for lossless JPEG copying

				// the EXIF is almost empty at this point (only liminance and chrominance tables are there);
				// add tags, some modified that carry already changed parameters:
				foreach(PropertyItem item in originalBitmap.PropertyItems)
				{
					// string str = "" + item.Type + ": " + item.Id + "=" + item.Value;
					// LibSys.StatusBar.Trace(str);

					if(item.Id == 0x5090 || item.Id == 0x5091)
					{
						// liminance and chrominance tables are already in a new Bitmap, better not touch them
						continue;
					}

					if(m_sizeChanged)
					{
						if(item.Id == 0xA002)	// PixXDim
						{
							PhotoDescr.toItemValueInt16(width, item.Value);
						}
						if(item.Id == 0xA003)	// PixYDim
						{
							PhotoDescr.toItemValueInt16(height, item.Value);
						}
					}
					if(originalOrientation != m_photoDescr.Orientation && item.Id == 274)
					{
						PhotoDescr.toItemValueInt16(m_photoDescr.Orientation, item.Value);
					}
					m_bitmap.SetPropertyItem(item);
				}

				// add GPS Coordinates to EXIF tags:
				if(m_wpt != null && addExifCoordsCheckBox.Checked)
				{
					setGpsExifTags(m_bitmap);
				}

				m_photoDescr.image = m_bitmap;
				this.saveButton.Enabled = true;
				m_useOriginal = false;
			}
			else
			{
				m_photoDescr.image = originalBitmap;
				this.saveButton.Enabled = false;
				m_useOriginal = true;
				m_sizeChanged = false;
				m_hasDrawn = false;
				m_hasRotated = false;
			}
			this.previewLinkLabel.Enabled = false;
			photoViewerControl.photoDescr = m_photoDescr;
		}

		#endregion // draw()

		#region setGpsExifTags()

		private void setGpsExifTags(Bitmap bitmap)
		{
			if(m_bitmap.PropertyItems.GetLength(0) == 0)
			{
				return;
			}

			//Bitmap bTmp = originalBitmap;
			Bitmap bTmp = m_bitmap;

			// see PhotoDescr.cs for formatting info:
			PropertyItem item = bTmp.PropertyItems[0];
			//PropertyItem item = bitmap.PropertyItems[0];
			item.Id = 1;
			item.Type = 2;
			item.Value = enc.GetBytes(m_wpt.Location.Lat >= 0.0d ? "N" : "S");
			item.Len = item.Value.GetLength(0);
			bitmap.SetPropertyItem(item);

			item = bTmp.PropertyItems[0];
			item.Id = 2;
			item.Type = 5;
			item.Value = PhotoDescr.ToPackedRationalCoord(m_wpt.Location.Lat);
			item.Len = item.Value.GetLength(0);
			bitmap.SetPropertyItem(item);

			item = bTmp.PropertyItems[0];
			item.Id = 3;
			item.Type = 2;
			item.Value = enc.GetBytes(m_wpt.Location.Lng >= 0.0d ? "E" : "W");
			item.Len = item.Value.GetLength(0);
			bitmap.SetPropertyItem(item);

			item = bTmp.PropertyItems[0];
			item.Id = 4;
			item.Type = 5;
			item.Value = PhotoDescr.ToPackedRationalCoord(m_wpt.Location.Lng);
			item.Len = item.Value.GetLength(0);
			bitmap.SetPropertyItem(item);
		}

		#endregion // setGpsExifTags()

		#region save()

		private void save()
		{
			bool savedOk = false;

			Project.photoSaveQuality = Convert.ToInt64(this.qualityNumericUpDown.Text);

			saveFileDialog1.InitialDirectory = Project.photoSaveInitialDirectory;
			saveFileDialog1.FileName = (m_savedFileName == null) ? "" : m_savedFileName;
			saveFileDialog1.DefaultExt = ".jpg";
			saveFileDialog1.AddExtension = true;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			saveFileDialog1.Filter = "JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*";
			DialogResult result = saveFileDialog1.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				string fileName = saveFileDialog1.FileName;
				int pos = fileName.LastIndexOf("\\");
				if(pos > 0)
				{
					Project.photoSaveInitialDirectory = fileName.Substring(0, pos);
				}

				string message = "There is a way to change EXIF data only, without JPEG quality loss.\r\n\r\nWant to do it?     (answering \"No\" will invoke regular save with Q:"
									+ Project.photoSaveQuality + "% quality vs. size)";

				bool alreadySaved = false;

				if(m_photoDescr.imageSourceIsLocal && !m_hasRotated && !m_sizeChanged && !m_hasDrawn && Project.YesNoBox(this, message))
				{
					// see http://www.eggheadcafe.com/articles/20030706.asp
					EncoderParameters encParms = new EncoderParameters(1);
					// for lossless rewriting must rotate the image by 90 degrees!
					encParms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation,
																(long)EncoderValue.TransformRotate90);

					string filenameTemp = Path.GetTempFileName();

					// write the rotated image with new description:
					Bitmap pic0 = PhotoDescr.bitmapFromFileOrZipfile(m_photoDescr.imageSource);		// file or zip archive
					setGpsExifTags(pic0);
					pic0.Save(filenameTemp, GetEncoderInfo("image/jpeg"), encParms);

					// release memory:
					pic0.Dispose();
					pic0 = null;
					GC.Collect();

					// must rotate back the written picture:
					Image pic = Image.FromFile(filenameTemp);
					encParms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation,
																(long)EncoderValue.TransformRotate270);
					try
					{
						pic.Save(fileName, GetEncoderInfo("image/jpeg"), encParms);
						savedOk = true;
					}
					catch
					{
						Project.ErrorBox(this, "Error: failed to save the file, is it being used by "
							+ Project.PROGRAM_NAME_HUMAN + " or another application?");
					}

					// release memory:
					pic.Dispose();
					pic = null;
					GC.Collect();

					// delete the temporary picture
					System.IO.File.Delete(filenameTemp);
					alreadySaved = true;
				}

				if(!alreadySaved)
				{
					// see http://www.eggheadcafe.com/articles/20030706.asp
					System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
					// Set the quality to, say, 40 (must be a long) -- that is compression to 40%. 1 is real bad, 100 is best quality.
					EncoderParameter ratio = new EncoderParameter(qualityEncoder, Project.photoSaveQuality);

					// Add the quality parameter to the list
					EncoderParameters codecParams = new EncoderParameters(1);
					codecParams.Param[0] = ratio;
					ImageCodecInfo jpegCodecInfo=GetEncoderInfo("image/jpeg");

					// Save to JPG
					try
					{
						// perform a save with compression:
						m_bitmap.Save(fileName, jpegCodecInfo, codecParams);
						savedOk = true;
					}
					catch
					{
						// something wrong, resort to plain save:
						try
						{
							m_bitmap.Save(fileName, ImageFormat.Jpeg);
							savedOk = true;
							Project.ErrorBox(this, "Warning: saved file with default quality due to system error.");
						}
						catch
						{
							Project.ErrorBox(this, "Error: failed to save the file, is it being used by another application?");
						}
					}
				}

				m_savedFileName = fileName;

				if(savedOk)
				{
					this.previewLinkLabel.Enabled = true;
				}

				configureCloseButtons();
			}
		}

		private void configureCloseButtons()
		{
			cancelButton.Text = "Close";
			cancelButton2.Text = "Close";
		}

		#endregion // save()

		#region Keys and Clickers

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

		private void DlgPhotoDraw_Move(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private void DlgPhotoDraw_Resize(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private static int m_dlgSizeWidth = -1;
		private static int m_dlgSizeHeight = -1;
		public static int m_dlgSizeX = -1;
		public static int m_dlgSizeY = -1;

		private void memorizeSizeAndPosition()
		{
			bool formMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
			if(!formMaximized && !inResize)
			{
				m_dlgSizeWidth = this.ClientSize.Width;
				m_dlgSizeHeight = this.ClientSize.Height;
				m_dlgSizeX = this.Location.X;
				m_dlgSizeY = this.Location.Y;
			}
		}

		private void applyButton_Click(object sender, System.EventArgs e)
		{
			m_fontSize = Convert.ToInt32(fontNumericUpDown.Text);
			if(m_fontSize > fontNumericUpDown.Maximum)
			{
				m_fontSize = (int)fontNumericUpDown.Maximum;
				fontNumericUpDown.Text = "" + m_fontSize;
			}
			draw(true);
		}

		private void resetButton_Click(object sender, System.EventArgs e)
		{
			draw(false);
		}

		private void saveButton_Click(object sender, System.EventArgs e)
		{
			save();
		}

		private void previewLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(m_savedFileName != null && File.Exists(m_savedFileName))
			{
				Project.RunBrowser(m_savedFileName);
			}
		}

		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for(j = 0; j < encoders.Length; ++j)
			{
				if(encoders[j].MimeType == mimeType)
				{
					return encoders[j];
				}
			}
			return null;
		}

		private void fitToSizeCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_fitToSize = fitToSizeCheckBox.Checked;
			photoViewerControl.fitToSize = m_fitToSize;
		}

		private void widthTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				setHeightBoxText();
			}
			catch
			{
				e.Cancel = true;
			}
		}

		private void widthTextBox_TextChanged(object sender, System.EventArgs e)
		{
			try
			{
				setHeightBoxText();
			}
			catch {}
		}

		private void setHeightBoxText()
		{
			string sWidth = widthTextBox.Text;
			int width = Convert.ToInt32(sWidth);
			heightTextBox.Text = "" + (Math.Round(width / whRatio));
		}

		private void addExifCoordsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_addExifCoords = addExifCoordsCheckBox.Checked;
		}

		private void positionComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			m_position = positionComboBox.SelectedIndex;
			if(this.saveButton.Enabled)
			{
				draw(true);
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(tabControl1.SelectedTab == exifTabPage)
			{
				_viewDescription.Text = "";
				lvwColumnSorter.SortColumn = 0;		// avoid exception
				AnalyzeAndReport();
			}
		}

		private void _listView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this._listView.SelectedItems.Count > 0) 
			{
				ListViewItem lvi = this._listView.SelectedItems[0];
				_viewDescription.Text = (string)lvi.Tag;
			}
		}

		private void _listView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			// Determine if clicked column is already the column that is being sorted.
			if (e.Column == lvwColumnSorter.SortColumn) 
			{
				// Reverse the current sort direction for this column.
				if (lvwColumnSorter.Order == SortOrder.Ascending) 
				{
					lvwColumnSorter.Order = SortOrder.Descending;
				}
				else 
				{
					lvwColumnSorter.Order = SortOrder.Ascending;
				}
			}
			else 
			{
				// Set the column number that is to be sorted; default to ascending.
				lvwColumnSorter.SortColumn = e.Column;
				lvwColumnSorter.Order = SortOrder.Ascending;
			}

			// Perform the sort with these new sort options.
			this._listView.Sort();
		}

		private void fontNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			m_fontSize = Convert.ToInt32(fontNumericUpDown.Value);
		}

		private void qualityNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			Project.photoSaveQuality = (long)this.qualityNumericUpDown.Value;
		}

		private void applyTextCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			this.textTextBox.Enabled = applyTextCheckBox.Checked;
			this.fontNumericUpDown.Enabled = applyTextCheckBox.Checked;
			this.positionComboBox.Enabled = applyTextCheckBox.Checked;
			this.pickColorButton.Enabled = applyTextCheckBox.Checked;
			this.pickColorButton.BackColor = applyTextCheckBox.Checked ? m_color : cancelButton.BackColor;
			this.useBgColorCheckBox.Enabled = applyTextCheckBox.Checked;
			this.pickBgColorButton.Enabled = applyTextCheckBox.Checked;
			this.pickBgColorButton.BackColor = applyTextCheckBox.Checked ? m_bgColor : cancelButton.BackColor;
			this.timePanel.Enabled = applyTextCheckBox.Checked && m_wpt != null && m_wpt.TrackId != -1;
		}

		private void pickColorButton_Click(object sender, System.EventArgs e)
		{
			textColorDialog.Color = m_color;
			if(this.textColorDialog.ShowDialog() == DialogResult.OK)
			{
				m_color =  textColorDialog.Color;
				pickColorButton.BackColor = m_color;
			}
		}

		private void pickBgColorButton_Click(object sender, System.EventArgs e)
		{
			textColorDialog.Color = m_bgColor;
			if(this.textColorDialog.ShowDialog() == DialogResult.OK)
			{
				m_bgColor =  textColorDialog.Color;
				pickBgColorButton.BackColor = m_bgColor;
			}
		}

		private void useBgColorCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_useBgColor = this.useBgColorCheckBox.Checked;
		}

		private void timeRadioButtons_CheckedChanged(object sender, System.EventArgs e)
		{
			setTextTextBox();
		}

		private void setTextTextBox()
		{
			string str = "";

			if(m_wpt != null)
			{
				str = m_wpt.Location.ToStringWithElev().Replace("high","elev") + "   ";
			}

			if(this.timeTptRadioButton.Checked && m_wpt != null)
			{
				textTextBox.Text = str + Project.zuluToLocal(m_wpt.DateTime);
			}
			else
			{
				textTextBox.Text = str + m_photoDescr.DTOrig;
			}
		}

		private void linkToWptLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DlgMakeWaypoint dlg = new DlgMakeWaypoint(CameraManager.This, m_photoDescr, m_wpt);
			dlg.ShowDialog();

			Waypoint wpt = dlg.m_wpt;

			if(wpt != null)
			{
				if(m_wpt != null)
				{
					// remove the old one:
					WaypointsCache.RemoveWaypointById(m_wpt.Id);
					PhotoWaypoints.RemoveWaypoint(m_wpt);
				}

				// make sure the new waypoint is accounted for in the cache:
				if(PhotoWaypoints.WaypointsWithThumbs.ContainsKey(wpt.DateTime))
				{
					PhotoWaypoints.WaypointsWithThumbs.Remove(wpt.DateTime);
				}
				PhotoWaypoints.WaypointsWithThumbs.Add(wpt.DateTime, wpt);
				PhotoWaypoints.CurrentWptIndex = PhotoWaypoints.WaypointsWithThumbs.Count - 1;

				m_wpt = wpt;
				addExifCoordsCheckBox.Enabled = (m_wpt != null);
				setTextTextBox();

				if(m_wpt != null)
				{
					CameraManager.This.keepInView(m_wpt.Location);
					CameraManager.This.ProcessCameraMove();
				}

				configureCloseButtons();
			}
		}

		private void rotateRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
		
		}

		#endregion // Keys and Clickers

		private void DlgPhotoDraw_Closed(object sender, System.EventArgs e)
		{
			foreach(string fileName in m_filesToDelete)
			{
				try
				{
					File.Delete(fileName);
				}
				catch {}
			}

			this.photoViewerControl.Dispose();

			m_photoDescr.imageDisposable = true;
			m_photoDescr.releaseImage();

			if(originalBitmap != null)
			{
				originalBitmap.Dispose();
				originalBitmap = null;
			}

			if(m_bitmap != null)
			{
				m_bitmap.Dispose();
				m_bitmap = null;
			}

			GC.Collect();
		}
	}

	#region class ListViewColumnSorter

	/// <summary>
	/// This class is an implementation of the 'IComparer' interface.</summary>
	/// <remarks>It is 'borrowed' from Microsoft Knowledge Base Article - 319401: 
	/// HOW TO: Sort a ListView Control by a Column in Visual C# .NET
	/// <newpara></newpara>
	/// See http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q319401
	/// </remarks>
	public class ListViewColumnSorter : IComparer 
	{
		/// <summary>
		/// Specifies the column to be sorted
		/// </summary>
		private int ColumnToSort;
		/// <summary>
		/// Specifies the order in which to sort (i.e. 'Ascending').
		/// </summary>
		private SortOrder OrderOfSort;
		/// <summary>
		/// Case insensitive comparer object
		/// </summary>
		private CaseInsensitiveComparer ObjectCompare;

		/// <summary>
		/// Class constructor.  Initializes various elements
		/// </summary>
		public ListViewColumnSorter() 
		{
			// Initialize the column to '0'
			ColumnToSort = 0;

			// Initialize the sort order to 'none'
			OrderOfSort = SortOrder.None;

			// Initialize the CaseInsensitiveComparer object
			ObjectCompare = new CaseInsensitiveComparer();
		}

		/// <summary>
		/// This method is inherited from the IComparer interface.  
		/// It compares the two objects passed using a case insensitive comparison.
		/// </summary>
		/// <param name="x">First object to be compared</param>
		/// <param name="y">Second object to be compared</param>
		/// <returns>The result of the comparison. 
		/// "0" if equal, negative if 'x' is less than 'y' 
		/// and positive if 'x' is greater than 'y'</returns>
		public int Compare(object x, object y) 
		{
			int compareResult;
			ListViewItem listviewX, listviewY;

			// Cast the objects to be compared to ListViewItem objects
			listviewX = (ListViewItem)x;
			listviewY = (ListViewItem)y;

			// Compare the two items
			compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text,
				listviewY.SubItems[ColumnToSort].Text);
			
			// Calculate correct return value based on object comparison
			if (OrderOfSort == SortOrder.Ascending) 
			{
				// Ascending sort is selected, return normal result of compare operation
				return compareResult;
			}
			else if (OrderOfSort == SortOrder.Descending) 
			{
				// Descending sort is selected, return negative result of compare operation
				return (-compareResult);
			}
			else 
			{
				// Return '0' to indicate they are equal
				return 0;
			}
		}
    
		/// <summary>
		/// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
		/// </summary>
		public int SortColumn 
		{
			set 
			{
				ColumnToSort = value;
			}
			get 
			{
				return ColumnToSort;
			}
		}

		/// <summary>
		/// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
		/// </summary>
		public SortOrder Order 
		{
			set 
			{
				OrderOfSort = value;
			}
			get 
			{
				return OrderOfSort;
			}
		}
	}


	#endregion // class ListViewColumnSorter
}
