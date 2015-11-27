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
using System.Data;
using System.Windows.Forms;

namespace LibSys
{
	/// <summary>
	/// Summary description for MyFileOrFolder.
	/// </summary>
	public class MyFileOrFolder : System.Windows.Forms.UserControl
	{
		public System.Windows.Forms.Button importFileButton;
		public System.Windows.Forms.TextBox fileTextBox;
		public System.Windows.Forms.Button browseFileButton;
		public System.Windows.Forms.Button browseFolderButton;
		public System.Windows.Forms.Button importFolderButton;
		public System.Windows.Forms.TextBox folderTextBox;
		public System.Windows.Forms.TabControl tabControl1;

		private System.Windows.Forms.TabPage fileTabPage;
		private System.Windows.Forms.TabPage folderTabPage;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label msgLabel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TabPage smugmugTabPage;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.Button importGalleryButton;
		public System.Windows.Forms.Button verifyGalleryButton;
		public System.Windows.Forms.TextBox galleryTextBox;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MyFileOrFolder()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			try
			{
				tabControl1.SelectedIndex = Project.pmLastTabMode;
			}
			catch
			{
				tabControl1.SelectedTab = fileTabPage;
			}
		}

		// 0 = file,  1 = folder,  2 = gallery
		public int tabMode { get { Project.pmLastTabMode = tabControl1.SelectedIndex; return Project.pmLastTabMode; } }

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.fileTabPage = new System.Windows.Forms.TabPage();
			this.panel2 = new System.Windows.Forms.Panel();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.browseFileButton = new System.Windows.Forms.Button();
			this.importFileButton = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.folderTabPage = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.folderTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.importFolderButton = new System.Windows.Forms.Button();
			this.browseFolderButton = new System.Windows.Forms.Button();
			this.msgLabel = new System.Windows.Forms.Label();
			this.smugmugTabPage = new System.Windows.Forms.TabPage();
			this.panel3 = new System.Windows.Forms.Panel();
			this.galleryTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.importGalleryButton = new System.Windows.Forms.Button();
			this.verifyGalleryButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.fileTabPage.SuspendLayout();
			this.panel2.SuspendLayout();
			this.folderTabPage.SuspendLayout();
			this.panel1.SuspendLayout();
			this.smugmugTabPage.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.fileTabPage,
																					  this.folderTabPage,
																					  this.smugmugTabPage});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Multiline = true;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(648, 90);
			this.tabControl1.TabIndex = 0;
			// 
			// fileTabPage
			// 
			this.fileTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.panel2});
			this.fileTabPage.Location = new System.Drawing.Point(4, 4);
			this.fileTabPage.Name = "fileTabPage";
			this.fileTabPage.Size = new System.Drawing.Size(640, 64);
			this.fileTabPage.TabIndex = 0;
			this.fileTabPage.Text = "File";
			// 
			// panel2
			// 
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.fileTextBox,
																				 this.browseFileButton,
																				 this.importFileButton,
																				 this.label5});
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(640, 64);
			this.panel2.TabIndex = 10;
			// 
			// fileTextBox
			// 
			this.fileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.fileTextBox.Location = new System.Drawing.Point(100, 10);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(360, 20);
			this.fileTextBox.TabIndex = 6;
			this.fileTextBox.Text = "";
			// 
			// browseFileButton
			// 
			this.browseFileButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.browseFileButton.Location = new System.Drawing.Point(465, 10);
			this.browseFileButton.Name = "browseFileButton";
			this.browseFileButton.TabIndex = 8;
			this.browseFileButton.Text = "Browse...";
			// 
			// importFileButton
			// 
			this.importFileButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.importFileButton.Location = new System.Drawing.Point(545, 10);
			this.importFileButton.Name = "importFileButton";
			this.importFileButton.Size = new System.Drawing.Size(85, 23);
			this.importFileButton.TabIndex = 9;
			this.importFileButton.Text = "Import";
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(10, 10);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(85, 23);
			this.label5.TabIndex = 7;
			this.label5.Text = "File:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// folderTabPage
			// 
			this.folderTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.panel1});
			this.folderTabPage.Location = new System.Drawing.Point(4, 4);
			this.folderTabPage.Name = "folderTabPage";
			this.folderTabPage.Size = new System.Drawing.Size(640, 64);
			this.folderTabPage.TabIndex = 1;
			this.folderTabPage.Text = "Folder";
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.folderTextBox,
																				 this.label2,
																				 this.importFolderButton,
																				 this.browseFolderButton,
																				 this.msgLabel});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(640, 64);
			this.panel1.TabIndex = 10;
			// 
			// folderTextBox
			// 
			this.folderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.folderTextBox.Location = new System.Drawing.Point(100, 10);
			this.folderTextBox.Name = "folderTextBox";
			this.folderTextBox.Size = new System.Drawing.Size(360, 20);
			this.folderTextBox.TabIndex = 5;
			this.folderTextBox.Text = "";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(5, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(90, 23);
			this.label2.TabIndex = 6;
			this.label2.Text = "Folder:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// importFolderButton
			// 
			this.importFolderButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.importFolderButton.Location = new System.Drawing.Point(545, 10);
			this.importFolderButton.Name = "importFolderButton";
			this.importFolderButton.Size = new System.Drawing.Size(85, 23);
			this.importFolderButton.TabIndex = 8;
			this.importFolderButton.Text = "Bulk Import";
			// 
			// browseFolderButton
			// 
			this.browseFolderButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.browseFolderButton.Location = new System.Drawing.Point(465, 10);
			this.browseFolderButton.Name = "browseFolderButton";
			this.browseFolderButton.TabIndex = 7;
			this.browseFolderButton.Text = "Browse...";
			// 
			// msgLabel
			// 
			this.msgLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.msgLabel.Font = new System.Drawing.Font("Arial", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.msgLabel.Location = new System.Drawing.Point(35, 40);
			this.msgLabel.Name = "msgLabel";
			this.msgLabel.Size = new System.Drawing.Size(595, 20);
			this.msgLabel.TabIndex = 9;
			this.msgLabel.Text = "Select a folder (with subfolders) containing all JPEG files to be related to trac" +
				"kpoints. May contain GPX files.";
			// 
			// smugmugTabPage
			// 
			this.smugmugTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.panel3});
			this.smugmugTabPage.Location = new System.Drawing.Point(4, 4);
			this.smugmugTabPage.Name = "smugmugTabPage";
			this.smugmugTabPage.Size = new System.Drawing.Size(640, 64);
			this.smugmugTabPage.TabIndex = 2;
			this.smugmugTabPage.Text = "Gallery - smugmug.com";
			// 
			// panel3
			// 
			this.panel3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.galleryTextBox,
																				 this.label1,
																				 this.importGalleryButton,
																				 this.verifyGalleryButton,
																				 this.label3});
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(640, 64);
			this.panel3.TabIndex = 11;
			// 
			// galleryTextBox
			// 
			this.galleryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.galleryTextBox.Location = new System.Drawing.Point(115, 10);
			this.galleryTextBox.Name = "galleryTextBox";
			this.galleryTextBox.Size = new System.Drawing.Size(345, 20);
			this.galleryTextBox.TabIndex = 5;
			this.galleryTextBox.Text = "";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(5, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(105, 23);
			this.label1.TabIndex = 6;
			this.label1.Text = "Gallery URL:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// importGalleryButton
			// 
			this.importGalleryButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.importGalleryButton.Location = new System.Drawing.Point(545, 10);
			this.importGalleryButton.Name = "importGalleryButton";
			this.importGalleryButton.Size = new System.Drawing.Size(85, 23);
			this.importGalleryButton.TabIndex = 8;
			this.importGalleryButton.Text = "Bulk Import";
			// 
			// verifyGalleryButton
			// 
			this.verifyGalleryButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.verifyGalleryButton.Location = new System.Drawing.Point(465, 10);
			this.verifyGalleryButton.Name = "verifyGalleryButton";
			this.verifyGalleryButton.TabIndex = 7;
			this.verifyGalleryButton.Text = "Verify...";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label3.Font = new System.Drawing.Font("Arial", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(35, 40);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(595, 20);
			this.label3.TabIndex = 9;
			this.label3.Text = "copy URL of a gallery, containing all JPEG files to be related to trackpoints. GP" +
				"X file must be already loaded.";
			// 
			// MyFileOrFolder
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.Name = "MyFileOrFolder";
			this.Size = new System.Drawing.Size(648, 90);
			this.tabControl1.ResumeLayout(false);
			this.fileTabPage.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.folderTabPage.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.smugmugTabPage.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
