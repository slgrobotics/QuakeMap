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
using LibFormats;

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPhotoToGeotiff.
	/// </summary>
	public class DlgPhotoToGeotiff : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox4;
		public System.Windows.Forms.TextBox fileTextBox;
		public System.Windows.Forms.Button browseFileButton;
		public System.Windows.Forms.Button selectFileButton;
		private System.Windows.Forms.Label label5;
		public System.Windows.Forms.TextBox traceTextBox;
		public System.Windows.Forms.Button selectGeotiffPointButton;
		public System.Windows.Forms.Button selectMapPointButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgPhotoToGeotiff()
		{
			InitializeComponent();

			this.fileTextBox.Text = Project.togeotiffFileName;

			toState(1);

			Project.setDlgIcon(this);
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

		private void toState(int state)
		{
			switch(state)
			{
				case 1:
					groupBox1.Enabled = true;
					groupBox2.Enabled = false;
					groupBox3.Enabled = false;
					groupBox4.Enabled = false;
					break;
				case 2:
					groupBox1.Enabled = false;
					groupBox2.Enabled = true;
					groupBox3.Enabled = false;
					groupBox4.Enabled = false;
					break;
				case 3:
					groupBox1.Enabled = false;
					groupBox2.Enabled = false;
					groupBox3.Enabled = true;
					groupBox4.Enabled = false;
					break;
				case 4:
					groupBox1.Enabled = false;
					groupBox2.Enabled = false;
					groupBox3.Enabled = false;
					groupBox4.Enabled = true;
					break;
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.browseFileButton = new System.Windows.Forms.Button();
			this.selectFileButton = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.traceTextBox = new System.Windows.Forms.TextBox();
			this.selectGeotiffPointButton = new System.Windows.Forms.Button();
			this.selectMapPointButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.fileTextBox,
																					this.browseFileButton,
																					this.selectFileButton,
																					this.label5});
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(560, 80);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "1. Select Image";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.selectMapPointButton,
																					this.selectGeotiffPointButton});
			this.groupBox2.Location = new System.Drawing.Point(8, 96);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(560, 80);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "2. Position the image roughly over the map";
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox3.Location = new System.Drawing.Point(8, 184);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(560, 80);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "3. Precisely adjust position by reference points";
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.traceTextBox});
			this.groupBox4.Location = new System.Drawing.Point(8, 272);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(560, 104);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "4. Produce GeoTIFF and open it";
			// 
			// fileTextBox
			// 
			this.fileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.fileTextBox.Location = new System.Drawing.Point(55, 20);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(395, 20);
			this.fileTextBox.TabIndex = 10;
			this.fileTextBox.Text = "";
			// 
			// browseFileButton
			// 
			this.browseFileButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.browseFileButton.Location = new System.Drawing.Point(455, 20);
			this.browseFileButton.Name = "browseFileButton";
			this.browseFileButton.Size = new System.Drawing.Size(30, 20);
			this.browseFileButton.TabIndex = 12;
			this.browseFileButton.Text = "...";
			this.browseFileButton.Click += new System.EventHandler(this.browseFileButton_Click);
			// 
			// selectFileButton
			// 
			this.selectFileButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.selectFileButton.Location = new System.Drawing.Point(490, 20);
			this.selectFileButton.Name = "selectFileButton";
			this.selectFileButton.Size = new System.Drawing.Size(60, 20);
			this.selectFileButton.TabIndex = 13;
			this.selectFileButton.Text = "Select";
			this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(10, 20);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 20);
			this.label5.TabIndex = 11;
			this.label5.Text = "File:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// traceTextBox
			// 
			this.traceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.traceTextBox.Location = new System.Drawing.Point(15, 25);
			this.traceTextBox.Multiline = true;
			this.traceTextBox.Name = "traceTextBox";
			this.traceTextBox.Size = new System.Drawing.Size(535, 70);
			this.traceTextBox.TabIndex = 11;
			this.traceTextBox.Text = "";
			// 
			// selectGeotiffPointButton
			// 
			this.selectGeotiffPointButton.Location = new System.Drawing.Point(15, 50);
			this.selectGeotiffPointButton.Name = "selectGeotiffPointButton";
			this.selectGeotiffPointButton.Size = new System.Drawing.Size(190, 20);
			this.selectGeotiffPointButton.TabIndex = 14;
			this.selectGeotiffPointButton.Text = "Click here, then on the Image";
			this.selectGeotiffPointButton.Click += new System.EventHandler(this.selectGeotiffPointButton_Click);
			// 
			// selectMapPointButton
			// 
			this.selectMapPointButton.Location = new System.Drawing.Point(215, 50);
			this.selectMapPointButton.Name = "selectMapPointButton";
			this.selectMapPointButton.Size = new System.Drawing.Size(190, 20);
			this.selectMapPointButton.TabIndex = 15;
			this.selectMapPointButton.Text = "Click here, then on the map";
			this.selectMapPointButton.Click += new System.EventHandler(this.selectMapPointButton_Click);
			// 
			// DlgPhotoToGeotiff
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(576, 390);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox4,
																		  this.groupBox3,
																		  this.groupBox2,
																		  this.groupBox1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgPhotoToGeotiff";
			this.Text = "Convert Photo to GeoTIFF";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void setFileText(string txt)
		{
			this.fileTextBox.Text = txt;
			this.fileTextBox.Select(txt.Length,0);
			this.fileTextBox.ScrollToCaret();
		}

		private void browseFileButton_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

			if(File.Exists(Project.togeotiffFileName))
			{
				openFileDialog.FileName = Project.togeotiffFileName;
			}
			else
			{
				openFileDialog.InitialDirectory = Project.togeotiffFolderPath;
			}
			openFileDialog.DefaultExt = "";
			openFileDialog.AddExtension = false;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "JPG, GIF, PNG and TIFF files (*.jpg;*.gif;*.png;" + FileGeoTIFF.FileExtensions + ")|*.jpg;*.gif;*.png;" + FileGeoTIFF.FileExtensions + "|All files (*.*)|*.*";

			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileInfo fi = new FileInfo(openFileDialog.FileName);
				Project.togeotiffFileName = Project.GetLongPathName(fi.FullName);
				Project.togeotiffFolderPath = fi.Directory.FullName;
				setFileText(Project.togeotiffFileName);
				traceTextBox.Text = Project.togeotiffFolderPath;
				//processSingleFile();
			}
		}

		private void selectFileButton_Click(object sender, System.EventArgs e)
		{
			traceTextBox.Text = "selected: " + Project.togeotiffFileName;

			GeoTiff geoTiff = new GeoTiff(Project.togeotiffFileName);

			geoTiff.TopLeftLat = geoTiff.TopRightLat = CameraManager.This.CoverageTopLeft34.Lat;
			geoTiff.TopLeftLng = geoTiff.BottomLeftLng = CameraManager.This.CoverageTopLeft34.Lng;
			geoTiff.BottomRightLat = geoTiff.BottomLeftLat = CameraManager.This.CoverageBottomRight34.Lat;
			geoTiff.BottomRightLng = geoTiff.TopRightLng = CameraManager.This.CoverageBottomRight34.Lng;

			geoTiff.initImageOnly();

			if(geoTiff.isValid)
			{
				CustomMapsCache.RemoveCustomMapsBySource(Project.togeotiffFileName);

				CustomMap cm = new CustomMapGeotiff(geoTiff);
				Project.customMapId++;
				cm.Id = Project.customMapId;
				CustomMapsCache.AddCustomMap(cm);

				WaypointsCache.pushBoundaries(cm.Location);
				WaypointsCache.pushBoundaries(new GeoCoord(geoTiff.BottomLeftLng, geoTiff.BottomLeftLat));
				WaypointsCache.pushBoundaries(new GeoCoord(geoTiff.TopRightLng, geoTiff.TopRightLat));
				WaypointsCache.pushBoundaries(new GeoCoord(geoTiff.BottomRightLng, geoTiff.BottomRightLat));

				string msg = "OK: read image file " + Project.togeotiffFileName;
				LibSys.StatusBar.Trace(msg);
				LibSys.StatusBar.Trace("* " + msg);

				toState(2);

				selectGeotiffPointButton.Enabled = true;
				selectMapPointButton.Enabled = false;

				PictureManager.This.Refresh();
			}
			else
			{
				LibSys.StatusBar.Error("cannot make a GeoTIFF file: " + Project.togeotiffFileName);
			}
		}

		private void expectClickOnImage()
		{
		}

		private void expectClickOnMap()
		{
		}

		private void selectGeotiffPointButton_Click(object sender, System.EventArgs e)
		{
		
		}

		private void selectMapPointButton_Click(object sender, System.EventArgs e)
		{
		
		}
	}
}
