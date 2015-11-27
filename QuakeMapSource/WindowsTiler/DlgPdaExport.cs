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
using System.Threading;

using LibSys;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPdaExport.
	/// </summary>
	public class DlgPdaExport : System.Windows.Forms.Form
	{
		private CameraTrack m_cameraTrack;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		private const string m_defaultExt = ".pdb";
		private string m_defaultFileName;
		private static string m_selectedFileName = null;
		private int lastPdaExportImageFormat = -1;
		private bool lastCompressState = Project.pdaExportUseWebsafeGrayscalePalette;
		private bool isDirty = false;

		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Button fileBrowseButton;
		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage exportTabPage;
		private System.Windows.Forms.TabPage optionsTabPage;
		private System.Windows.Forms.Button closeButton1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButtonJpeg;
		private System.Windows.Forms.RadioButton radioButtonBmpWebsafe;
		private System.Windows.Forms.RadioButton radioButtonBmpOptimized;
		private System.Windows.Forms.LinkLabel helpLinkLabel;
		private System.Windows.Forms.LinkLabel helpExportLinkLabel;
		private System.Windows.Forms.CheckBox compressGreyscaleCheckBox;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgPdaExport(CameraTrack cameraTrack)
		{
			m_cameraTrack = cameraTrack;
			m_defaultFileName = "pmapdata" + m_defaultExt;

			InitializeComponent();

			if(m_selectedFileName == null)
			{
				m_selectedFileName = Path.Combine(Project.pdaInitialDirectory, m_defaultFileName);
			}
			fileTextBox.Text = m_selectedFileName;

			saveButton.Enabled = fileTextBox.Text.Length > 0;

			setRadioButtons();

			Project.setDlgIcon(this);
		}

		private void DlgPdaExport_Load(object sender, System.EventArgs e)
		{
			Project.ClearPopup();
			Cursor.Current = Cursors.WaitCursor;
			this.messageLabel.Text = "\nPlease wait, preparing data...";
			//Project.threadPool.PostRequest (new WorkRequestDelegate (prepareFiles), "ProcessRecentFileItemClick"); 
			ThreadPool2.QueueUserWorkItem (new WaitCallback (prepareFiles), "DlgPdaExport"); 
			this.saveButton.Focus();
			this.Enabled = false;
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
			this.messageLabel = new System.Windows.Forms.Label();
			this.fileBrowseButton = new System.Windows.Forms.Button();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.saveButton = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.exportTabPage = new System.Windows.Forms.TabPage();
			this.helpExportLinkLabel = new System.Windows.Forms.LinkLabel();
			this.optionsTabPage = new System.Windows.Forms.TabPage();
			this.label1 = new System.Windows.Forms.Label();
			this.helpLinkLabel = new System.Windows.Forms.LinkLabel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.compressGreyscaleCheckBox = new System.Windows.Forms.CheckBox();
			this.radioButtonBmpOptimized = new System.Windows.Forms.RadioButton();
			this.radioButtonBmpWebsafe = new System.Windows.Forms.RadioButton();
			this.radioButtonJpeg = new System.Windows.Forms.RadioButton();
			this.closeButton1 = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.exportTabPage.SuspendLayout();
			this.optionsTabPage.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(392, 165);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(64, 22);
			this.closeButton.TabIndex = 2;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(16, 75);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(360, 112);
			this.messageLabel.TabIndex = 2;
			// 
			// fileBrowseButton
			// 
			this.fileBrowseButton.Location = new System.Drawing.Point(392, 40);
			this.fileBrowseButton.Name = "fileBrowseButton";
			this.fileBrowseButton.Size = new System.Drawing.Size(64, 22);
			this.fileBrowseButton.TabIndex = 8;
			this.fileBrowseButton.Text = "Browse...";
			this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
			// 
			// fileTextBox
			// 
			this.fileTextBox.Location = new System.Drawing.Point(40, 40);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(336, 20);
			this.fileTextBox.TabIndex = 5;
			this.fileTextBox.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 40);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(29, 22);
			this.label5.TabIndex = 7;
			this.label5.Text = "File:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(448, 22);
			this.label2.TabIndex = 6;
			this.label2.Text = "Creating a .PDB file to be transferred to your PDA.";
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(392, 72);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(64, 22);
			this.saveButton.TabIndex = 0;
			this.saveButton.Text = "Save";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.exportTabPage,
																					  this.optionsTabPage});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(5, 5);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(474, 224);
			this.tabControl1.TabIndex = 10;
			this.tabControl1.TabStop = false;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// exportTabPage
			// 
			this.exportTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.helpExportLinkLabel,
																						this.fileBrowseButton,
																						this.saveButton,
																						this.label2,
																						this.messageLabel,
																						this.label5,
																						this.fileTextBox,
																						this.closeButton});
			this.exportTabPage.Location = new System.Drawing.Point(4, 22);
			this.exportTabPage.Name = "exportTabPage";
			this.exportTabPage.Size = new System.Drawing.Size(466, 198);
			this.exportTabPage.TabIndex = 0;
			this.exportTabPage.Text = "Export";
			// 
			// helpExportLinkLabel
			// 
			this.helpExportLinkLabel.Location = new System.Drawing.Point(400, 136);
			this.helpExportLinkLabel.Name = "helpExportLinkLabel";
			this.helpExportLinkLabel.Size = new System.Drawing.Size(48, 16);
			this.helpExportLinkLabel.TabIndex = 20;
			this.helpExportLinkLabel.TabStop = true;
			this.helpExportLinkLabel.Text = "Help";
			this.helpExportLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.helpExportLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpExportLinkLabel_LinkClicked);
			// 
			// optionsTabPage
			// 
			this.optionsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.label1,
																						 this.helpLinkLabel,
																						 this.groupBox1,
																						 this.closeButton1});
			this.optionsTabPage.Location = new System.Drawing.Point(4, 22);
			this.optionsTabPage.Name = "optionsTabPage";
			this.optionsTabPage.Size = new System.Drawing.Size(466, 198);
			this.optionsTabPage.TabIndex = 1;
			this.optionsTabPage.Text = "Options";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(235, 75);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(208, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "Click \"Export\" tab above to process";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// helpLinkLabel
			// 
			this.helpLinkLabel.Location = new System.Drawing.Point(392, 136);
			this.helpLinkLabel.Name = "helpLinkLabel";
			this.helpLinkLabel.Size = new System.Drawing.Size(64, 16);
			this.helpLinkLabel.TabIndex = 5;
			this.helpLinkLabel.TabStop = true;
			this.helpLinkLabel.Text = "Help";
			this.helpLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.helpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpLinkLabel_LinkClicked);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.compressGreyscaleCheckBox,
																					this.radioButtonBmpOptimized,
																					this.radioButtonBmpWebsafe,
																					this.radioButtonJpeg});
			this.groupBox1.Location = new System.Drawing.Point(24, 24);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(200, 126);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "PDB Format";
			// 
			// compressGreyscaleCheckBox
			// 
			this.compressGreyscaleCheckBox.Location = new System.Drawing.Point(16, 90);
			this.compressGreyscaleCheckBox.Name = "compressGreyscaleCheckBox";
			this.compressGreyscaleCheckBox.Size = new System.Drawing.Size(160, 24);
			this.compressGreyscaleCheckBox.TabIndex = 13;
			this.compressGreyscaleCheckBox.Text = "compress grayscale aerial";
			this.compressGreyscaleCheckBox.CheckedChanged += new System.EventHandler(this.compressGreyscaleCheckBox_CheckedChanged);
			// 
			// radioButtonBmpOptimized
			// 
			this.radioButtonBmpOptimized.Location = new System.Drawing.Point(15, 60);
			this.radioButtonBmpOptimized.Name = "radioButtonBmpOptimized";
			this.radioButtonBmpOptimized.Size = new System.Drawing.Size(176, 20);
			this.radioButtonBmpOptimized.TabIndex = 12;
			this.radioButtonBmpOptimized.Text = "PocketPC colors";
			this.radioButtonBmpOptimized.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// radioButtonBmpWebsafe
			// 
			this.radioButtonBmpWebsafe.Location = new System.Drawing.Point(15, 40);
			this.radioButtonBmpWebsafe.Name = "radioButtonBmpWebsafe";
			this.radioButtonBmpWebsafe.Size = new System.Drawing.Size(176, 20);
			this.radioButtonBmpWebsafe.TabIndex = 11;
			this.radioButtonBmpWebsafe.Text = "Palm colors";
			this.radioButtonBmpWebsafe.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// radioButtonJpeg
			// 
			this.radioButtonJpeg.Location = new System.Drawing.Point(15, 20);
			this.radioButtonJpeg.Name = "radioButtonJpeg";
			this.radioButtonJpeg.Size = new System.Drawing.Size(176, 20);
			this.radioButtonJpeg.TabIndex = 10;
			this.radioButtonJpeg.Text = "original colors";
			this.radioButtonJpeg.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// closeButton1
			// 
			this.closeButton1.Location = new System.Drawing.Point(392, 165);
			this.closeButton1.Name = "closeButton1";
			this.closeButton1.Size = new System.Drawing.Size(64, 22);
			this.closeButton1.TabIndex = 9;
			this.closeButton1.Text = "Close";
			this.closeButton1.Visible = false;
			this.closeButton1.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// DlgPdaExport
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(484, 234);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.DockPadding.All = 5;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgPdaExport";
			this.Text = "Export Camera Track to PDA";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgPdaExport_Closing);
			this.Load += new System.EventHandler(this.DlgPdaExport_Load);
			this.tabControl1.ResumeLayout(false);
			this.exportTabPage.ResumeLayout(false);
			this.optionsTabPage.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void prepareFiles(object state)
		{
			if(lastPdaExportImageFormat != Project.pdaExportImageFormat || lastCompressState != Project.pdaExportUseWebsafeGrayscalePalette)
			{
				Cursor.Current = Cursors.WaitCursor;
				try 
				{
					m_cameraTrack.savePrepare();
#if DEBUG
					//Project.RunBrowser(m_cameraTrack.DescrFilePath);
#endif
					messageLabel.Text =  "Press \"Save\" --->\n\n" + m_cameraTrack.ToString();		// display how much data we have to export

					lastPdaExportImageFormat = Project.pdaExportImageFormat;
					lastCompressState = Project.pdaExportUseWebsafeGrayscalePalette;
					isDirty = true;
				}
				catch (Exception ee) 
				{
					messageLabel.Text = "Error:\r\n" + ee.Message;
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}
			this.Enabled = true;
			this.saveButton.Focus();
		}

		public void browseAndSave()
		{
			try 
			{
				if(m_selectedFileName == null || m_selectedFileName.Length == 0)
				{
					saveFileDialog1.InitialDirectory = Project.pdaInitialDirectory;
					saveFileDialog1.FileName = m_defaultFileName;
				} 
				else
				{
					saveFileDialog1.FileName = m_selectedFileName;
				}
				saveFileDialog1.DefaultExt = m_defaultExt;
				saveFileDialog1.AddExtension = true;
				// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
				saveFileDialog1.Filter = "PDB files(*" + m_defaultExt + ")|*" + m_defaultExt + "|All files (*.*)|*.*";

				DialogResult result = saveFileDialog1.ShowDialog(this);
				if (result == DialogResult.OK)
				{
					m_selectedFileName = saveFileDialog1.FileName;
					conditionFileName();
					resetPdaInitialDirectory();
					fileTextBox.Text = m_selectedFileName;
					doWrite();
				}
				saveButton.Enabled = fileTextBox.Text.Length > 0;
			}
			catch (Exception e) 
			{
				messageLabel.Text = "Error:\r\n" + e.Message;
			}
		}

		public void conditionFileName()
		{
			if(!m_selectedFileName.ToLower().EndsWith(m_defaultExt))
			{
				m_selectedFileName = m_selectedFileName + m_defaultExt;
			}

			int pos = m_selectedFileName.LastIndexOf("\\");
			if(pos >= 0)
			{
				string directory = m_selectedFileName.Substring(0, pos+1);		// include the backslash
				string filename = m_selectedFileName.Substring(pos+1);
				m_selectedFileName = directory + filename.ToLower();
			}
			else
			{
				m_selectedFileName = m_selectedFileName.ToLower();
			}
		}

		public void resetPdaInitialDirectory()
		{
			int pos = m_selectedFileName.LastIndexOf("\\");
			if(pos >= 0)
			{
				Project.pdaInitialDirectory = m_selectedFileName.Substring(0, pos+1);		// include the backslash
			}
		}

		public void doWrite()
		{
			Cursor.Current = Cursors.WaitCursor;
			saveButton.Enabled = false;
			try 
			{
				// destination files:
				string descrPdbFilePath  = m_selectedFileName;
				string folderName = "";
				string catalogName = "pmapdata";
				int pos = m_selectedFileName.LastIndexOf("\\");
				if(pos > 0)
				{
					folderName = m_selectedFileName.Substring(0, pos);
					catalogName = m_selectedFileName.Substring(pos+1);
					catalogName = catalogName.Substring(0, catalogName.Length-4);
				}
				//string imagesPdbFilePath = Path.Combine(folderName, "palmapdata.pdb");

				m_cameraTrack.save(catalogName, descrPdbFilePath);	//, imagesPdbFilePath);

				FileInfo fi = new FileInfo(descrPdbFilePath);
				long fileSizeKb = fi.Length / 1024L;

				messageLabel.Text = "OK: PDA dataset saved; catalog name: \"" + catalogName + "\"\r\n\r\n" + descrPdbFilePath
										+ "\n\n" + fileSizeKb + " Kb"; // + "\r\n" + imagesPdbFilePath;
				isDirty = false;
			}
			catch (Exception e)
			{
				messageLabel.Text = "Error:\r\n\r\n" + e.Message;
			}
			saveButton.Enabled = true;
			Cursor.Current = Cursors.Default;
		}

		private void fileBrowseButton_Click(object sender, System.EventArgs e)
		{
			browseAndSave();
		}

		private void saveButton_Click(object sender, System.EventArgs e)
		{
			if(this.fileTextBox.Text.Length > 0 && this.fileTextBox.Text.LastIndexOf("\\") > 0)
			{
				m_selectedFileName = this.fileTextBox.Text.Trim();
				conditionFileName();
				this.fileTextBox.Text = m_selectedFileName;
				resetPdaInitialDirectory();
				doWrite();
			}
			else
			{
				messageLabel.Text = "Error:\r\n\r\nselect file name first";
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			setRadioButtons();
			prepareFiles(null);
		}

		private void collectRadioButtons()
		{
			if(radioButtonBmpWebsafe.Checked)
			{
				Project.pdaExportImageFormat = 0;
				this.compressGreyscaleCheckBox.Enabled = true;
			}
			else if(radioButtonBmpOptimized.Checked)
			{
				Project.pdaExportImageFormat = 1;
				this.compressGreyscaleCheckBox.Enabled = true;
			}
			else if(radioButtonJpeg.Checked)
			{
				Project.pdaExportImageFormat = 2;
				this.compressGreyscaleCheckBox.Enabled = false;
			}
			Project.pdaExportUseWebsafeGrayscalePalette = !this.compressGreyscaleCheckBox.Checked;
		}

		private void setRadioButtons()
		{
			this.compressGreyscaleCheckBox.Checked = !Project.pdaExportUseWebsafeGrayscalePalette;
			switch(Project.pdaExportImageFormat)
			{
				case 0:
					radioButtonBmpWebsafe.Checked = true;
					this.compressGreyscaleCheckBox.Enabled = true;
					break;
				case 1:
					radioButtonBmpOptimized.Checked = true;
					this.compressGreyscaleCheckBox.Enabled = true;
					break;
				case 2:
					radioButtonJpeg.Checked = true;
					this.compressGreyscaleCheckBox.Enabled = false;
					break;
			}
		}

		private void radioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			collectRadioButtons();
		}

		private void compressGreyscaleCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.pdaExportUseWebsafeGrayscalePalette = !this.compressGreyscaleCheckBox.Checked;
		}

		private void helpExportLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser("http://www.mapadvisor.com/help/qmexportpdb.html");
		}

		private void helpLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser("http://www.mapadvisor.com/help/qmexportpdbformats.html");
		}

		private void DlgPdaExport_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(isDirty)
			{
				if(Project.YesNoBox(this, "Do you want a chance to save the .PDB file?"))
				{
					e.Cancel = true;
				}
			}
		}
	}
}
