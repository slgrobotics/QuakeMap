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
using LibGui;
using LibFormats;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for ImageExportForm.
	/// </summary>
	public class ImageExportForm : System.Windows.Forms.Form
	{
		private static string m_selectedFileName = "";
		private const int whMin = 400;
		private const int whMax = 5000;
		private static int width = -1;
		private static int height = -1;

		private static ImageFormat m_imageFormat = ImageFormat.Jpeg;
		private static string m_defaultExt = ".jpg";
		private static string m_selectedFormat = "JPEG";

		private System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Button fileBrowseButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox widthTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox heightTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label messageLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ImageExportForm()
		{
			InitializeComponent();

			Project.setDlgIcon(this);
		}

		private const string m_hintToBrowse = "click \"Browse\" or type full path here";

		private void ImageExportForm_Load(object sender, System.EventArgs e)
		{
			fileTextBox.Text = m_selectedFileName.Length > 0 ? m_selectedFileName : m_hintToBrowse; // will invoke fileTextBox_TextChanged()

			if(width <= whMin || height <= whMin)
			{
				width = PictureManager.This.Width;
			}

			height = (int)Math.Round((double)width * (double)PictureManager.This.Height / (double)PictureManager.This.Width);

			widthTextBox.Text = "" + width;
			heightTextBox.Text = "" + height;

			fileTextBox.SelectAll();
			fileTextBox.Focus();
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.heightTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.widthTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.fileBrowseButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.messageLabel = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label4,
																					this.heightTextBox,
																					this.label3,
																					this.label2,
																					this.widthTextBox,
																					this.label1,
																					this.fileTextBox,
																					this.fileBrowseButton,
																					this.okButton});
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(489, 120);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Select Destination";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(120, 72);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 23);
			this.label4.TabIndex = 12;
			this.label4.Text = "pixels";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// heightTextBox
			// 
			this.heightTextBox.Location = new System.Drawing.Point(56, 88);
			this.heightTextBox.Name = "heightTextBox";
			this.heightTextBox.ReadOnly = true;
			this.heightTextBox.Size = new System.Drawing.Size(56, 20);
			this.heightTextBox.TabIndex = 20;
			this.heightTextBox.TabStop = false;
			this.heightTextBox.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 22);
			this.label3.TabIndex = 10;
			this.label3.Text = "Height:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 9;
			this.label2.Text = "Width:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// widthTextBox
			// 
			this.widthTextBox.Location = new System.Drawing.Point(56, 56);
			this.widthTextBox.Name = "widthTextBox";
			this.widthTextBox.Size = new System.Drawing.Size(56, 20);
			this.widthTextBox.TabIndex = 18;
			this.widthTextBox.Text = "";
			this.widthTextBox.Leave += new System.EventHandler(this.widthTextBox_Leave);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 23);
			this.label1.TabIndex = 7;
			this.label1.Text = "File:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// fileTextBox
			// 
			this.fileTextBox.Location = new System.Drawing.Point(56, 24);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(336, 20);
			this.fileTextBox.TabIndex = 10;
			this.fileTextBox.Text = "";
			this.fileTextBox.TextChanged += new System.EventHandler(this.fileTextBox_TextChanged);
			// 
			// fileBrowseButton
			// 
			this.fileBrowseButton.Location = new System.Drawing.Point(408, 24);
			this.fileBrowseButton.Name = "fileBrowseButton";
			this.fileBrowseButton.Size = new System.Drawing.Size(72, 22);
			this.fileBrowseButton.TabIndex = 14;
			this.fileBrowseButton.Text = "Browse...";
			this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(408, 56);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(72, 21);
			this.okButton.TabIndex = 26;
			this.okButton.Text = "Save";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(416, 136);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(72, 21);
			this.cancelButton.TabIndex = 30;
			this.cancelButton.Text = "Close";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(16, 136);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(384, 23);
			this.messageLabel.TabIndex = 31;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ImageExportForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(504, 168);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.messageLabel,
																		  this.groupBox1,
																		  this.cancelButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ImageExportForm";
			this.Text = "Export Image";
			this.Load += new System.EventHandler(this.ImageExportForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		
		private void fileBrowseButton_Click(object sender, System.EventArgs e)
		{
			browseAndSave();
		}

		private void fileTextBox_TextChanged(object sender, System.EventArgs e)
		{
			if(fileTextBox.Text.Length == 0 || fileTextBox.Text.Equals(m_hintToBrowse))
			{
				okButton.Enabled = false;
			}
			else
			{
				m_selectedFileName = fileTextBox.Text;
				okButton.Enabled = true;
			}
		}

		private void okButton_Click(object sender, System.EventArgs e)
		{
			// prevent saving in the bin folder, which occurs if file name is not a valid path:
			if(m_selectedFileName == null
				|| m_selectedFileName.Length == 0
				|| m_selectedFileName.LastIndexOf("\\") <= 0)
			{
				browseAndSave();
			} 
			else
			{
				int pos = m_selectedFileName.LastIndexOf("\\");
				string folder = m_selectedFileName.Substring(0, pos);
				if(!Directory.Exists(folder))
				{
					browseAndSave();
				}
				else
				{
					doWrite();
				}
			}
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void browseAndSave()
		{
			if(m_selectedFileName == null || m_selectedFileName.Length == 0)
			{
				saveFileDialog1.InitialDirectory = Project.imageInitialDirectory;
				saveFileDialog1.FileName = "";
			} 
			else if(!File.Exists(m_selectedFileName))
			{
				saveFileDialog1.FileName = m_selectedFileName;
			}
			else
			{
				saveFileDialog1.FileName = m_selectedFileName;
			}
			saveFileDialog1.DefaultExt = m_defaultExt;
			saveFileDialog1.AddExtension = true;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			saveFileDialog1.Filter = m_selectedFormat + "(*" + m_defaultExt + ")|*" + m_defaultExt
				+ "|JPEG files (*.jpg)|*.jpg"
				+ "|Windows BMP files (*.bmp)|*.bmp"
				+ "|GIF files (*.gif)|*.gif"
				+ "|TIFF files (*.tif)|*.tif"
				+ "|PNG files (*.png)|*.png"
				+ "|All files (*.*)|*.*";
			DialogResult result = saveFileDialog1.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				string fileName = saveFileDialog1.FileName;
				m_selectedFileName = fileName;
				int pos = fileName.LastIndexOf("\\");
				if(pos > 0)
				{
					Project.imageInitialDirectory = fileName.Substring(0, pos);
				}
				fileTextBox.Text = m_selectedFileName;
				doWrite();
			}
			okButton.Enabled = fileTextBox.Text.Length > 0;
		}

		private void doWrite()
		{
			string filename = m_selectedFileName;

			if(!collectWidth())
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				Rectangle physicalBounds = new Rectangle(0, 0, width, height);

				Bitmap bitmap = new Bitmap(width,height, PixelFormat.Format32bppArgb);

				Graphics graphics = Graphics.FromImage(bitmap);

				graphics.FillRectangle(Project.whiteBrush, physicalBounds);

				Project.mainCommand.doPrintPage(physicalBounds, graphics, false);

				// default is JPEG
				m_imageFormat = ImageFormat.Jpeg;
				m_selectedFormat = "JPEG";
				m_defaultExt = ".jpg";

				if(filename.ToLower().EndsWith(".bmp"))
				{
					m_imageFormat = ImageFormat.Bmp;
					m_selectedFormat = "BMP";
					m_defaultExt = ".bmp";
				}
				else if(filename.ToLower().EndsWith(".gif"))
				{
					m_imageFormat = ImageFormat.Gif;
					m_selectedFormat = "GIF";
					m_defaultExt = ".gif";
				}
				else if(AllFormats.isTiffFile(filename))
				{
					m_imageFormat = ImageFormat.Tiff;
					m_selectedFormat = "TIFF";
					m_defaultExt = ".tif";
				}
				else if(filename.ToLower().EndsWith(".png"))
				{
					m_imageFormat = ImageFormat.Png;
					m_selectedFormat = "PNG";
					m_defaultExt = ".png";
				}

				if(!filename.ToLower().EndsWith(m_defaultExt))
				{
					filename += m_defaultExt;
				}

				filename = new FileInfo(filename).FullName;
				fileTextBox.Text = filename;
				m_selectedFileName = filename;

				bitmap.Save(filename, m_imageFormat);

				messageLabel.Text = "OK: saved";
			}
			catch(Exception ee)
			{
				messageLabel.Text = ee.Message;
			}
			Cursor.Current = Cursors.Default;
		}

		private void widthTextBox_Leave(object sender, System.EventArgs e)
		{
			collectWidth();
		}

		private bool collectWidth()
		{
			messageLabel.Text = "";
			string errorMessage = "width and height must be numbers between " + whMin + " and " + whMax;
			try
			{
				width = Convert.ToInt32(widthTextBox.Text);
				height = (int)Math.Round((double)width * (double)PictureManager.This.Height / (double)PictureManager.This.Width);
				heightTextBox.Text = "" + height;

				if(width < whMin || width > whMax || height < whMin || height > whMax)
				{
					messageLabel.Text = errorMessage;
					return false;
				}
			}
			catch
			{
				messageLabel.Text = errorMessage;
				return false;
			}
			return true;
		}
	}
}
