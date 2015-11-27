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
using System.IO;

using LibSys;
using LibGeo;
using LibGui;
using LibFormats;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for WebPageExportForm.
	/// </summary>
	public class WebPageExportForm : System.Windows.Forms.Form, ILocationWatcher
	{
		private static string m_pageHeading = "photos from my last trip";
		private static string m_pageComment = "put page description or comments here";

		private string m_indexFileFullPath = "";

		private const int whMin = 400;
		private const int whMax = 5000;
		private int width = -1;
		private int height = -1;

		private ArrayList templateNames = new ArrayList();
		private DictionaryEditorControl commonDictionaryEditorControl;
		private DictionaryEditorControl templateDictionaryEditorControl;

		private System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox folderTextBox;
		private System.Windows.Forms.Button fileBrowseButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox templateComboBox;
		private System.Windows.Forms.Button previewButton;
		private System.Windows.Forms.CheckBox appendToMasterCheckBox;
		private System.Windows.Forms.LinkLabel helpExportLinkLabel;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox headingTextBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox commentTextBox;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage actionTabPage;
		private System.Windows.Forms.Panel actionPanel;
		private System.Windows.Forms.Label widthLabel;
		private System.Windows.Forms.Label heightLabel;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TabPage commonDictionaryTabPage;
		private System.Windows.Forms.TabPage templateDictionaryTabPage;
		private System.Windows.Forms.Panel commonDictionaryPanel;
		private System.Windows.Forms.Panel templateDictionaryPanel;
		private System.Windows.Forms.LinkLabel forwardLinkLabel;
		private System.Windows.Forms.LinkLabel backLinkLabel;
		private System.Windows.Forms.TabPage optionsTabPage;
		private System.Windows.Forms.Panel optionsPanel;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton linksAsPopupsRadioButton;
		private System.Windows.Forms.RadioButton linksAsSamePageRadioButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox webStorageTextBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.ComboBox pictureSizeComboBox;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button setWebStorageButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WebPageExportForm()
		{
			InitializeComponent();

			int selectedIndex = 0;
			int index = 0;
			string scanFolder = Project.GetWebPageTemplatesFolderBase();
			DirectoryInfo dir = new DirectoryInfo(scanFolder);
			foreach(DirectoryInfo subdir in dir.GetDirectories())
			{
				if(!subdir.Name.StartsWith("_"))
				{
					templateNames.Add(subdir.Name);
					if(Project.webPageTemplate.Equals(subdir.Name))
					{
						selectedIndex = index;
					}
				}
				index++;
			}
			templateComboBox.DataSource = templateNames;
			templateComboBox.SelectedIndex = selectedIndex;

			string filename = Path.Combine(Project.GetWebPageTemplatesFolderBase(), @"_common\Dictionary.xml");

			commonDictionaryEditorControl = new DictionaryEditorControl(filename);

			this.commonDictionaryPanel.Controls.AddRange(new System.Windows.Forms.Control[] { this.commonDictionaryEditorControl});
			// 
			// commonDictionaryEditorControl
			// 
			this.commonDictionaryEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commonDictionaryEditorControl.Name = "commonDictionaryEditorControl";
			this.commonDictionaryEditorControl.TabIndex = 1;

			if(Project.photoGalleryPictureSize >= Project.photoGalleryPictureSizes.Length)
			{
				Project.photoGalleryPictureSize = 2;
			}
			pictureSizeComboBox.SelectedIndex = Project.photoGalleryPictureSize;

			this.webStorageTextBox.Text = Project.webPagePhotoStorageUrlBase;

			if(Project.webPageLinksUsePopup)
			{
				this.linksAsPopupsRadioButton.Checked = true;
			}
			else
			{
				this.linksAsSamePageRadioButton.Checked = true;
			}

			this.linksAsSamePageRadioButton.CheckedChanged += new System.EventHandler(this.linksRadioButton_CheckedChanged);
			this.linksAsPopupsRadioButton.CheckedChanged += new System.EventHandler(this.linksRadioButton_CheckedChanged);

			setTemplateDictionaryControl();

			Project.setDlgIcon(this);
		}

		private void setTemplateDictionaryControl()
		{
			if(templateDictionaryEditorControl != null)
			{
				templateDictionaryEditorControl.lastChanceToSave(false);
				this.templateDictionaryPanel.Controls.Clear();
				templateDictionaryEditorControl.Dispose();
			}

			string filename = Path.Combine(Project.GetWebPageTemplatesFolderBase(), templateComboBox.Text + @"\Dictionary.xml");

			templateDictionaryEditorControl = new DictionaryEditorControl(filename);

			this.templateDictionaryPanel.Controls.AddRange(new System.Windows.Forms.Control[] { this.templateDictionaryEditorControl});
			// 
			// templateDictionaryEditorControl
			// 
			this.templateDictionaryEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.templateDictionaryEditorControl.Name = "templateDictionaryEditorControl";
			this.templateDictionaryEditorControl.TabIndex = 1;
		}

		private const string m_hintToBrowse = "click \"Browse\" or type full path here";

		private void WebPageExportForm_Load(object sender, System.EventArgs e)
		{
			headingTextBox.Text = m_pageHeading;
			commentTextBox.Text = m_pageComment;

			folderTextBox.Text = Project.webPageDestinationFolder.Length > 0 ? Project.webPageDestinationFolder : m_hintToBrowse; // will invoke folderTextBox_TextChanged()

			if(width <= whMin || height <= whMin)
			{
				width = PictureManager.This.Width;
			}

			height = (int)Math.Round((double)width * (double)PictureManager.This.Height / (double)PictureManager.This.Width);

			widthLabel.Text = "" + width;
			heightLabel.Text = "" + height;

			previewButton.Enabled = false;

			backLinkLabel.Enabled = (Project.cameraPositionsBackStack.Count > 1);
			forwardLinkLabel.Enabled = (Project.cameraPositionsFwdStack.Count > 0);

			headingTextBox.SelectAll();
			headingTextBox.Focus();

			CameraManager.This.m_locationWatcher2 = this;
		}

		public void LocationChangedCallback(GeoCoord location)
		{
			backLinkLabel.Enabled = (Project.cameraPositionsBackStack.Count > 1);
			forwardLinkLabel.Enabled = (Project.cameraPositionsFwdStack.Count > 0);
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			CameraManager.This.m_locationWatcher2 = null;

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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.folderTextBox = new System.Windows.Forms.TextBox();
			this.fileBrowseButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.messageLabel = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.heightLabel = new System.Windows.Forms.Label();
			this.widthLabel = new System.Windows.Forms.Label();
			this.previewButton = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.templateComboBox = new System.Windows.Forms.ComboBox();
			this.appendToMasterCheckBox = new System.Windows.Forms.CheckBox();
			this.helpExportLinkLabel = new System.Windows.Forms.LinkLabel();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label6 = new System.Windows.Forms.Label();
			this.headingTextBox = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.commentTextBox = new System.Windows.Forms.TextBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.actionTabPage = new System.Windows.Forms.TabPage();
			this.actionPanel = new System.Windows.Forms.Panel();
			this.forwardLinkLabel = new System.Windows.Forms.LinkLabel();
			this.backLinkLabel = new System.Windows.Forms.LinkLabel();
			this.commonDictionaryTabPage = new System.Windows.Forms.TabPage();
			this.commonDictionaryPanel = new System.Windows.Forms.Panel();
			this.templateDictionaryTabPage = new System.Windows.Forms.TabPage();
			this.templateDictionaryPanel = new System.Windows.Forms.Panel();
			this.optionsTabPage = new System.Windows.Forms.TabPage();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label11 = new System.Windows.Forms.Label();
			this.pictureSizeComboBox = new System.Windows.Forms.ComboBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.setWebStorageButton = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.webStorageTextBox = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.linksAsSamePageRadioButton = new System.Windows.Forms.RadioButton();
			this.linksAsPopupsRadioButton = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.actionTabPage.SuspendLayout();
			this.actionPanel.SuspendLayout();
			this.commonDictionaryTabPage.SuspendLayout();
			this.templateDictionaryTabPage.SuspendLayout();
			this.optionsTabPage.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.panel3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label1,
																					this.folderTextBox,
																					this.fileBrowseButton});
			this.groupBox1.Location = new System.Drawing.Point(5, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(595, 57);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Destination";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(5, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 21);
			this.label1.TabIndex = 7;
			this.label1.Text = "Folder:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// folderTextBox
			// 
			this.folderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.folderTextBox.Location = new System.Drawing.Point(85, 24);
			this.folderTextBox.Name = "folderTextBox";
			this.folderTextBox.Size = new System.Drawing.Size(425, 20);
			this.folderTextBox.TabIndex = 10;
			this.folderTextBox.Text = "";
			this.folderTextBox.TextChanged += new System.EventHandler(this.folderTextBox_TextChanged);
			// 
			// fileBrowseButton
			// 
			this.fileBrowseButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.fileBrowseButton.Location = new System.Drawing.Point(515, 24);
			this.fileBrowseButton.Name = "fileBrowseButton";
			this.fileBrowseButton.Size = new System.Drawing.Size(72, 22);
			this.fileBrowseButton.TabIndex = 14;
			this.fileBrowseButton.Text = "Browse...";
			this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(135, 30);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 20);
			this.label4.TabIndex = 12;
			this.label4.Text = "pixels";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(10, 40);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 20);
			this.label3.TabIndex = 10;
			this.label3.Text = "Height:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(10, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 20);
			this.label2.TabIndex = 9;
			this.label2.Text = "Width:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// okButton
			// 
			this.okButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.okButton.Location = new System.Drawing.Point(520, 280);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 21);
			this.okButton.TabIndex = 26;
			this.okButton.Text = "Save";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(525, 375);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 21);
			this.cancelButton.TabIndex = 30;
			this.cancelButton.Text = "Close";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.messageLabel.Location = new System.Drawing.Point(10, 375);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(465, 20);
			this.messageLabel.TabIndex = 31;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label8,
																					this.heightLabel,
																					this.widthLabel,
																					this.label3,
																					this.label4,
																					this.label2});
			this.groupBox2.Location = new System.Drawing.Point(5, 65);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(215, 85);
			this.groupBox2.TabIndex = 27;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Image";
			// 
			// label8
			// 
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label8.Location = new System.Drawing.Point(5, 60);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(200, 20);
			this.label8.TabIndex = 23;
			this.label8.Text = "(to change, resize QuakeMap window)";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// heightLabel
			// 
			this.heightLabel.Location = new System.Drawing.Point(75, 40);
			this.heightLabel.Name = "heightLabel";
			this.heightLabel.Size = new System.Drawing.Size(35, 20);
			this.heightLabel.TabIndex = 22;
			this.heightLabel.Text = "yyy";
			this.heightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// widthLabel
			// 
			this.widthLabel.Location = new System.Drawing.Point(75, 20);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Size = new System.Drawing.Size(35, 20);
			this.widthLabel.TabIndex = 21;
			this.widthLabel.Text = "xxx";
			this.widthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// previewButton
			// 
			this.previewButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.previewButton.Location = new System.Drawing.Point(520, 310);
			this.previewButton.Name = "previewButton";
			this.previewButton.Size = new System.Drawing.Size(75, 21);
			this.previewButton.TabIndex = 32;
			this.previewButton.Text = "Preview";
			this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(5, 25);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(70, 23);
			this.label5.TabIndex = 33;
			this.label5.Text = "Template:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// templateComboBox
			// 
			this.templateComboBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.templateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.templateComboBox.Location = new System.Drawing.Point(80, 25);
			this.templateComboBox.Name = "templateComboBox";
			this.templateComboBox.Size = new System.Drawing.Size(290, 21);
			this.templateComboBox.TabIndex = 34;
			this.templateComboBox.SelectedIndexChanged += new System.EventHandler(this.templateComboBox_SelectedIndexChanged);
			// 
			// appendToMasterCheckBox
			// 
			this.appendToMasterCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.appendToMasterCheckBox.Location = new System.Drawing.Point(80, 55);
			this.appendToMasterCheckBox.Name = "appendToMasterCheckBox";
			this.appendToMasterCheckBox.Size = new System.Drawing.Size(285, 20);
			this.appendToMasterCheckBox.TabIndex = 35;
			this.appendToMasterCheckBox.Text = "append this URL to ..\\index.html";
			// 
			// helpExportLinkLabel
			// 
			this.helpExportLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.helpExportLinkLabel.Location = new System.Drawing.Point(475, 375);
			this.helpExportLinkLabel.Name = "helpExportLinkLabel";
			this.helpExportLinkLabel.Size = new System.Drawing.Size(45, 18);
			this.helpExportLinkLabel.TabIndex = 36;
			this.helpExportLinkLabel.TabStop = true;
			this.helpExportLinkLabel.Text = "Help";
			this.helpExportLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.helpExportLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpExportLinkLabel_LinkClicked);
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.appendToMasterCheckBox,
																					this.templateComboBox,
																					this.label5});
			this.groupBox3.Location = new System.Drawing.Point(225, 65);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(375, 85);
			this.groupBox3.TabIndex = 37;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Web Page Options";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(10, 165);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(75, 20);
			this.label6.TabIndex = 38;
			this.label6.Text = "Heading:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// headingTextBox
			// 
			this.headingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.headingTextBox.Location = new System.Drawing.Point(90, 165);
			this.headingTextBox.Name = "headingTextBox";
			this.headingTextBox.Size = new System.Drawing.Size(505, 20);
			this.headingTextBox.TabIndex = 39;
			this.headingTextBox.Text = "";
			this.headingTextBox.Leave += new System.EventHandler(this.headingTextBox_Leave);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(5, 195);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(80, 20);
			this.label7.TabIndex = 40;
			this.label7.Text = "Description:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// commentTextBox
			// 
			this.commentTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.commentTextBox.Location = new System.Drawing.Point(90, 195);
			this.commentTextBox.Multiline = true;
			this.commentTextBox.Name = "commentTextBox";
			this.commentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.commentTextBox.Size = new System.Drawing.Size(505, 60);
			this.commentTextBox.TabIndex = 41;
			this.commentTextBox.Text = "";
			this.commentTextBox.Leave += new System.EventHandler(this.commentTextBox_Leave);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.actionTabPage,
																					  this.commonDictionaryTabPage,
																					  this.templateDictionaryTabPage,
																					  this.optionsTabPage});
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(610, 365);
			this.tabControl1.TabIndex = 42;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// actionTabPage
			// 
			this.actionTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.actionPanel});
			this.actionTabPage.Location = new System.Drawing.Point(4, 22);
			this.actionTabPage.Name = "actionTabPage";
			this.actionTabPage.Size = new System.Drawing.Size(602, 339);
			this.actionTabPage.TabIndex = 0;
			this.actionTabPage.Text = "Action";
			// 
			// actionPanel
			// 
			this.actionPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.forwardLinkLabel,
																					  this.backLinkLabel,
																					  this.headingTextBox,
																					  this.label7,
																					  this.groupBox1,
																					  this.commentTextBox,
																					  this.groupBox3,
																					  this.label6,
																					  this.groupBox2,
																					  this.previewButton,
																					  this.okButton});
			this.actionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.actionPanel.Name = "actionPanel";
			this.actionPanel.Size = new System.Drawing.Size(602, 339);
			this.actionPanel.TabIndex = 0;
			// 
			// forwardLinkLabel
			// 
			this.forwardLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.forwardLinkLabel.Location = new System.Drawing.Point(465, 285);
			this.forwardLinkLabel.Name = "forwardLinkLabel";
			this.forwardLinkLabel.Size = new System.Drawing.Size(45, 15);
			this.forwardLinkLabel.TabIndex = 43;
			this.forwardLinkLabel.TabStop = true;
			this.forwardLinkLabel.Text = "fwd>";
			this.forwardLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.forwardLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.forwardLinkLabel_LinkClicked);
			// 
			// backLinkLabel
			// 
			this.backLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.backLinkLabel.Location = new System.Drawing.Point(410, 285);
			this.backLinkLabel.Name = "backLinkLabel";
			this.backLinkLabel.Size = new System.Drawing.Size(50, 15);
			this.backLinkLabel.TabIndex = 42;
			this.backLinkLabel.TabStop = true;
			this.backLinkLabel.Text = "<back";
			this.backLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.backLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.backLinkLabel_LinkClicked);
			// 
			// commonDictionaryTabPage
			// 
			this.commonDictionaryTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																								  this.commonDictionaryPanel});
			this.commonDictionaryTabPage.Location = new System.Drawing.Point(4, 22);
			this.commonDictionaryTabPage.Name = "commonDictionaryTabPage";
			this.commonDictionaryTabPage.Size = new System.Drawing.Size(602, 339);
			this.commonDictionaryTabPage.TabIndex = 1;
			this.commonDictionaryTabPage.Text = "Common Dictionary";
			// 
			// commonDictionaryPanel
			// 
			this.commonDictionaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commonDictionaryPanel.Name = "commonDictionaryPanel";
			this.commonDictionaryPanel.Size = new System.Drawing.Size(602, 339);
			this.commonDictionaryPanel.TabIndex = 0;
			// 
			// templateDictionaryTabPage
			// 
			this.templateDictionaryTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																									this.templateDictionaryPanel});
			this.templateDictionaryTabPage.Location = new System.Drawing.Point(4, 22);
			this.templateDictionaryTabPage.Name = "templateDictionaryTabPage";
			this.templateDictionaryTabPage.Size = new System.Drawing.Size(602, 339);
			this.templateDictionaryTabPage.TabIndex = 2;
			this.templateDictionaryTabPage.Text = "Template Dictionary";
			// 
			// templateDictionaryPanel
			// 
			this.templateDictionaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.templateDictionaryPanel.Name = "templateDictionaryPanel";
			this.templateDictionaryPanel.Size = new System.Drawing.Size(602, 339);
			this.templateDictionaryPanel.TabIndex = 1;
			// 
			// optionsTabPage
			// 
			this.optionsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.optionsPanel});
			this.optionsTabPage.Location = new System.Drawing.Point(4, 22);
			this.optionsTabPage.Name = "optionsTabPage";
			this.optionsTabPage.Size = new System.Drawing.Size(602, 339);
			this.optionsTabPage.TabIndex = 3;
			this.optionsTabPage.Text = "Options";
			// 
			// optionsPanel
			// 
			this.optionsPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.groupBox5,
																					   this.groupBox4});
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(602, 339);
			this.optionsPanel.TabIndex = 0;
			// 
			// groupBox5
			// 
			this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox5.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.panel3});
			this.groupBox5.Location = new System.Drawing.Point(15, 140);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(580, 55);
			this.groupBox5.TabIndex = 1;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "SmugMug.com";
			// 
			// panel3
			// 
			this.panel3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label11,
																				 this.pictureSizeComboBox});
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(3, 16);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(574, 36);
			this.panel3.TabIndex = 12;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(5, 5);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(70, 23);
			this.label11.TabIndex = 11;
			this.label11.Text = "Picture Size:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pictureSizeComboBox
			// 
			this.pictureSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pictureSizeComboBox.Items.AddRange(new object[] {
																	 "Small",
																	 "Medium",
																	 "Large",
																	 "Original"});
			this.pictureSizeComboBox.Location = new System.Drawing.Point(85, 5);
			this.pictureSizeComboBox.Name = "pictureSizeComboBox";
			this.pictureSizeComboBox.Size = new System.Drawing.Size(130, 21);
			this.pictureSizeComboBox.TabIndex = 10;
			this.pictureSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.pictureSizeComboBox_SelectedIndexChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.panel2,
																					this.panel1});
			this.groupBox4.Location = new System.Drawing.Point(10, 15);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(585, 115);
			this.groupBox4.TabIndex = 0;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Link Generator";
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.setWebStorageButton,
																				 this.label10,
																				 this.webStorageTextBox,
																				 this.label9});
			this.panel2.Location = new System.Drawing.Point(5, 50);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(575, 60);
			this.panel2.TabIndex = 1;
			// 
			// setWebStorageButton
			// 
			this.setWebStorageButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.setWebStorageButton.Location = new System.Drawing.Point(530, 5);
			this.setWebStorageButton.Name = "setWebStorageButton";
			this.setWebStorageButton.Size = new System.Drawing.Size(40, 20);
			this.setWebStorageButton.TabIndex = 43;
			this.setWebStorageButton.Text = "Set";
			this.setWebStorageButton.Click += new System.EventHandler(this.setWebStorageButton_Click);
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label10.Location = new System.Drawing.Point(10, 30);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(505, 20);
			this.label10.TabIndex = 42;
			this.label10.Text = "Specify where you store your pictures, for thumbnail links";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// webStorageTextBox
			// 
			this.webStorageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.webStorageTextBox.Location = new System.Drawing.Point(90, 5);
			this.webStorageTextBox.Name = "webStorageTextBox";
			this.webStorageTextBox.Size = new System.Drawing.Size(435, 20);
			this.webStorageTextBox.TabIndex = 41;
			this.webStorageTextBox.Text = "";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(5, 5);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(80, 20);
			this.label9.TabIndex = 40;
			this.label9.Text = "Web Storage:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.linksAsSamePageRadioButton,
																				 this.linksAsPopupsRadioButton});
			this.panel1.Location = new System.Drawing.Point(5, 20);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(575, 30);
			this.panel1.TabIndex = 0;
			// 
			// linksAsSamePageRadioButton
			// 
			this.linksAsSamePageRadioButton.Location = new System.Drawing.Point(215, 5);
			this.linksAsSamePageRadioButton.Name = "linksAsSamePageRadioButton";
			this.linksAsSamePageRadioButton.Size = new System.Drawing.Size(255, 24);
			this.linksAsSamePageRadioButton.TabIndex = 1;
			this.linksAsSamePageRadioButton.Text = "links open in the same window";
			// 
			// linksAsPopupsRadioButton
			// 
			this.linksAsPopupsRadioButton.Location = new System.Drawing.Point(10, 5);
			this.linksAsPopupsRadioButton.Name = "linksAsPopupsRadioButton";
			this.linksAsPopupsRadioButton.Size = new System.Drawing.Size(195, 24);
			this.linksAsPopupsRadioButton.TabIndex = 0;
			this.linksAsPopupsRadioButton.Text = "links open in popup window";
			// 
			// WebPageExportForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(609, 401);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1,
																		  this.helpExportLinkLabel,
																		  this.messageLabel,
																		  this.cancelButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(617, 427);
			this.Name = "WebPageExportForm";
			this.Text = "Export as Web Page";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.WebPageExportForm_Closing);
			this.Load += new System.EventHandler(this.WebPageExportForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.actionTabPage.ResumeLayout(false);
			this.actionPanel.ResumeLayout(false);
			this.commonDictionaryTabPage.ResumeLayout(false);
			this.templateDictionaryTabPage.ResumeLayout(false);
			this.optionsTabPage.ResumeLayout(false);
			this.optionsPanel.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		
		private void fileBrowseButton_Click(object sender, System.EventArgs e)
		{
			clearMessages();
			markPageDirty();
			browseForFolder();
		}

		private void folderTextBox_TextChanged(object sender, System.EventArgs e)
		{
			if(folderTextBox.Text.Length == 0 || folderTextBox.Text.Equals(m_hintToBrowse))
			{
				okButton.Enabled = false;
			}
			else
			{
				Project.webPageDestinationFolder = folderTextBox.Text;
				okButton.Enabled = true;
			}
			markPageDirty();

			clearMessages();
		}

		private void setWebStorageButton_Click(object sender, System.EventArgs e)
		{
			Project.webPagePhotoStorageUrlBase = this.webStorageTextBox.Text.Trim();
		}

		// clicked Save button
		private void okButton_Click(object sender, System.EventArgs e)
		{
			clearMessages();

			markPageDirty();

			Project.webPagePhotoStorageUrlBase = this.webStorageTextBox.Text.Trim();

			// prevent saving in the bin folder, which occurs if file name is not a valid path:
			if(Project.webPageDestinationFolder == null
				|| Project.webPageDestinationFolder.Length == 0
				|| Project.webPageDestinationFolder.LastIndexOf("\\") <= 0)
			{
				browseForFolder();
			} 
			else
			{
				if(!Directory.Exists(Project.webPageDestinationFolder))
				{
					DirectoryInfo di = new DirectoryInfo(Project.webPageDestinationFolder);
					if(di.Parent.Exists)
					{
						try
						{
							Directory.CreateDirectory(Project.webPageDestinationFolder);
						}
						catch
						{
							browseForFolder();
						}
					}
					else
					{
						browseForFolder();
					}
				}

				// if all attempts above failed, do nothing.
				if(Directory.Exists(Project.webPageDestinationFolder))
				{
					doGeneratePage();
				}
			}
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void browseForFolder()
		{
			IntPtr owner = this.Handle;
			clsFolderBrowser folderBrowser = new clsFolderBrowser(owner, "Select Mapping Cache Folder");

			string tmp = folderBrowser.BrowseForFolder(folderTextBox.Text);
			if(!"\\".Equals(tmp))		// cancel button causes "\" returned
			{
				folderTextBox.Text = tmp;
			}
		}

		private void markPageDirty()
		{
			m_indexFileFullPath = "";
			previewButton.Enabled = m_indexFileFullPath.Length > 0;
		}

		private void doGeneratePage()
		{
//			if(!collectWidth())
//			{
//				return;
//			}

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				string templateName = templateComboBox.Text;

				WebPage webPage = new WebPage(templateName);

				webPage.imageWidth = width;
				webPage.imageHeight = height;

				webPage.shortName = new DirectoryInfo(Project.webPageDestinationFolder).Name;
				webPage.description = commentTextBox.Text;
				webPage.heading = headingTextBox.Text;

				webPage.init(commonDictionaryEditorControl.Dictionary, templateDictionaryEditorControl.Dictionary);

				WebPageGenerator webPageGenerator = new WebPageGenerator(Project.webPageDestinationFolder, webPage);

				webPageGenerator.generateAll(this.appendToMasterCheckBox.Checked);

				messageLabel.Text = "OK: web page saved";
				messageLabel.BackColor = Color.Yellow;

				m_indexFileFullPath = webPageGenerator.indexFileFullPath;

				previewButton.Enabled = m_indexFileFullPath.Length > 0;
			}
			catch(Exception ee)
			{
				messageLabel.Text = "Error: failed to generate web page";
				messageLabel.BackColor = Color.Red;
				LibSys.StatusBar.Error("failed to generate web page - " + ee);
			}
			Cursor.Current = Cursors.Default;
		}

//		private void widthTextBox_Leave(object sender, System.EventArgs e)
//		{
//			collectWidth();
//		}
//
//		private bool collectWidth()
//		{
//			clearMessages();
//			string errorMessage = "width and height must be numbers between " + whMin + " and " + whMax;
//			try
//			{
//				int prevWidth = width;
//				int prevHeight = height;
//
//				width = Convert.ToInt32(widthTextBox.Text);
//				height = (int)Math.Round((double)width * (double)PictureManager.This.Height / (double)PictureManager.This.Width);
//				heightTextBox.Text = "" + height;
//
//				if(width < whMin || width > whMax || height < whMin || height > whMax)
//				{
//					messageLabel.Text = errorMessage;
//					return false;
//				}
//
//				if(prevWidth != width || prevHeight != height)
//				{
//					markPageDirty();
//				}
//			}
//			catch
//			{
//				messageLabel.Text = errorMessage;
//				return false;
//			}
//			return true;
//		}

		private void helpExportLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(Project.WEBSITE_LINK_WEBSTYLE + "/help/exportaswebpage.html");
		}

		private void headingTextBox_Leave(object sender, System.EventArgs e)
		{
			if(!m_pageHeading.Equals(headingTextBox.Text.Trim()))
			{
				markPageDirty();
			}
			m_pageHeading = headingTextBox.Text.Trim();
		}

		private void commentTextBox_Leave(object sender, System.EventArgs e)
		{
			if(!m_pageComment.Equals(commentTextBox.Text.Trim()))
			{
				markPageDirty();
			}
			m_pageComment = commentTextBox.Text.Trim();
		}

		private void previewButton_Click(object sender, System.EventArgs e)
		{
			clearMessages();

			if(File.Exists(m_indexFileFullPath))
			{
				Project.RunBrowser(m_indexFileFullPath);
			}
			else
			{
				messageLabel.Text = "Error: page index file not found";
				messageLabel.BackColor = Color.Red;
				markPageDirty();
			}
		}

		private void templateComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			clearMessages();
			Project.webPageTemplate = this.templateComboBox.Text;
			markPageDirty();
			setTemplateDictionaryControl();
		}

		private void WebPageExportForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			commonDictionaryEditorControl.lastChanceToSave(templateDictionaryEditorControl.lastChanceToSave(false));
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
//			commonDictionaryEditorControl.lastChanceToSave(templateDictionaryEditorControl.lastChanceToSave(false));
			clearMessages();
		}

		private void backLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			clearMessages();
			Project.mainCommand.doBack();
		}

		private void forwardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			clearMessages();
			Project.mainCommand.doForward();
		}

		private void clearMessages()
		{
			messageLabel.Text = "";
			messageLabel.BackColor = Color.LightGray;
		}

		private void pictureSizeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.photoGalleryPictureSize = pictureSizeComboBox.SelectedIndex;
		}

		private void linksRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(this.linksAsPopupsRadioButton.Checked)
			{
				Project.webPageLinksUsePopup = true;
			}
			else
			{
				Project.webPageLinksUsePopup = false;
			}
		}
	}
}
