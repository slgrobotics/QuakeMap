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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;

using LibSys;
using LibGui;
using LibGeo;
using LibFormats;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for FileExportForm.
	/// </summary>
	public class FileExportForm : System.Windows.Forms.Form
	{
		public static string m_selectedFileName = "";

		public static string m_selectedFormat = FileEasyGps.FormatName;

		private static int waypointCount = 0;
		private static int trkpointCount = 0;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

		private ArrayList m_tracks = new ArrayList();
		private static bool m_saveTracks = true;
		private ArrayList m_waypoints = new ArrayList();
		private static bool m_saveWaypoints = true;

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button fileBrowseButton;
		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.TextBox messageTextBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox persistCheckBox;
		private System.Windows.Forms.CheckBox saveWptCheckBox;
		private System.Windows.Forms.CheckBox saveTracksCheckBox;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// null indicates saving all waypoints or tracks, and do-flags can suspend saving at all
		/// </summary>
		/// <param name="tracks"></param>
		/// <param name="doSaveTracks"></param>
		/// <param name="waypoints"></param>
		/// <param name="doSaveWaypoints"></param>
		public FileExportForm(ArrayList tracks, bool doSaveTracks, ArrayList waypoints, bool doSaveWaypoints)
		{
			m_saveTracks = doSaveTracks;
			m_saveWaypoints = doSaveWaypoints;

			this.Text = "Save ";
			if(m_saveTracks)
			{
				if(tracks == null)
				{
					m_tracks = WaypointsCache.TracksAll;
					this.Text += "All Routes and Tracks ";
				}
				else
				{
					m_tracks = tracks;
					this.Text += "Selected Routes and Tracks ";
				}
			}

			if(m_saveWaypoints)
			{
				if(waypoints == null)
				{
					m_waypoints = WaypointsCache.WaypointsAll;
					this.Text += "All Waypoints ";
				}
				else
				{
					m_waypoints = waypoints;
					this.Text += "Selected Waypoints ";
				}
			}
			this.Text += " to a File";

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Project.setDlgIcon(this);
		}

		private const string m_hintToBrowse = "click \"Browse\" or type full path here";

		private void FileExportForm_Load(object sender, System.EventArgs e)
		{
			showWhatIsSaved();

			fileTextBox.Text = m_selectedFileName.Length > 0 ? m_selectedFileName : m_hintToBrowse; // will invoke fileTextBox_TextChanged()

			saveWptCheckBox.Checked = m_saveWaypoints;
			saveTracksCheckBox.Checked = m_saveTracks;

			fileTextBox.SelectAll();
			fileTextBox.Focus();
		}

		private bool messageBoxDirty = true;

		public void showWhatIsSaved()
		{
			if(messageBoxDirty)
			{
				string str = "";
				if(m_saveWaypoints && m_waypoints != null && m_waypoints.Count > 0)
				{
					str += "   Waypoints: " + m_waypoints.Count + "\r\n\r\n";
				}

				if(m_saveTracks && m_tracks != null)
				{
					foreach(Track trk in m_tracks)
					{
						str += (trk.isRoute ? "   Route [" : "   Track [") + trk.Id + "] -- " + trk.Name + "     (" + trk.Trackpoints.Count + " legs)\r\n"; 
					}
				}

				messageTextBox.ForeColor = Color.Black;
				messageTextBox.Text = "What is being saved:\r\n\r\n" + str;
				messageBoxDirty = false;
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
			this.fileBrowseButton = new System.Windows.Forms.Button();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.messageTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.saveTracksCheckBox = new System.Windows.Forms.CheckBox();
			this.saveWptCheckBox = new System.Windows.Forms.CheckBox();
			this.persistCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(410, 16);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(72, 21);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Close";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// fileBrowseButton
			// 
			this.fileBrowseButton.Location = new System.Drawing.Point(410, 45);
			this.fileBrowseButton.Name = "fileBrowseButton";
			this.fileBrowseButton.Size = new System.Drawing.Size(72, 22);
			this.fileBrowseButton.TabIndex = 2;
			this.fileBrowseButton.Text = "Browse...";
			this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
			// 
			// fileTextBox
			// 
			this.fileTextBox.Location = new System.Drawing.Point(48, 45);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(357, 20);
			this.fileTextBox.TabIndex = 1;
			this.fileTextBox.Text = "";
			this.fileTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fileTextBox_KeyDown);
			this.fileTextBox.TextChanged += new System.EventHandler(this.fileTextBox_TextChanged);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(410, 75);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(72, 21);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "Save";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// messageTextBox
			// 
			this.messageTextBox.AcceptsReturn = true;
			this.messageTextBox.Location = new System.Drawing.Point(5, 115);
			this.messageTextBox.Multiline = true;
			this.messageTextBox.Name = "messageTextBox";
			this.messageTextBox.ReadOnly = true;
			this.messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.messageTextBox.Size = new System.Drawing.Size(489, 136);
			this.messageTextBox.TabIndex = 11;
			this.messageTextBox.TabStop = false;
			this.messageTextBox.Text = "";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label1,
																					this.saveTracksCheckBox,
																					this.saveWptCheckBox,
																					this.persistCheckBox,
																					this.fileTextBox,
																					this.fileBrowseButton,
																					this.okButton,
																					this.cancelButton});
			this.groupBox1.Location = new System.Drawing.Point(5, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(489, 105);
			this.groupBox1.TabIndex = 12;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Select File";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 45);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 23);
			this.label1.TabIndex = 7;
			this.label1.Text = "File:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// saveTracksCheckBox
			// 
			this.saveTracksCheckBox.Location = new System.Drawing.Point(150, 75);
			this.saveTracksCheckBox.Name = "saveTracksCheckBox";
			this.saveTracksCheckBox.Size = new System.Drawing.Size(175, 23);
			this.saveTracksCheckBox.TabIndex = 7;
			this.saveTracksCheckBox.Text = "save tracks / routes";
			this.saveTracksCheckBox.CheckedChanged += new System.EventHandler(this.saveTracksCheckBox_CheckedChanged);
			// 
			// saveWptCheckBox
			// 
			this.saveWptCheckBox.Location = new System.Drawing.Point(15, 75);
			this.saveWptCheckBox.Name = "saveWptCheckBox";
			this.saveWptCheckBox.Size = new System.Drawing.Size(125, 23);
			this.saveWptCheckBox.TabIndex = 6;
			this.saveWptCheckBox.Text = "save waypoints";
			this.saveWptCheckBox.CheckedChanged += new System.EventHandler(this.saveWptCheckBox_CheckedChanged);
			// 
			// persistCheckBox
			// 
			this.persistCheckBox.Location = new System.Drawing.Point(15, 15);
			this.persistCheckBox.Name = "persistCheckBox";
			this.persistCheckBox.Size = new System.Drawing.Size(305, 22);
			this.persistCheckBox.TabIndex = 5;
			this.persistCheckBox.Text = "make persistent (load on start)";
			this.persistCheckBox.CheckedChanged += new System.EventHandler(this.persistCheckBox_CheckedChanged);
			// 
			// FileExportForm
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(499, 253);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox1,
																		  this.messageTextBox});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FileExportForm";
			this.Load += new System.EventHandler(this.FileExportForm_Load);
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
			act();
		}

		private void act()
		{
			// prevent saving in the bin folder, which occurs if file name is not a valid path:
			if(m_selectedFileName == null
				|| m_selectedFileName.Length == 0
				|| m_selectedFileName.IndexOf("\\") < 0)
			{
				browseAndSave();
			} 
			else
			{
				if(m_selectedFileName.ToLower().EndsWith(FileStreetsTripsCsv.FileExtension))
				{
					m_selectedFormat = FileStreetsTripsCsv.FormatName;
				}
				else if(m_selectedFileName.ToLower().EndsWith(FileEasyGps.FileExtension))
				{
					m_selectedFormat = FileEasyGps.FormatName;
				}
				else if(m_selectedFileName.ToLower().EndsWith(FileKml.FileExtension))
				{
					m_selectedFormat = FileKml.FormatName;
				}

				if(m_selectedFormat.Equals(FileStreetsTripsCsv.FormatName) && !m_selectedFileName.ToLower().EndsWith(FileStreetsTripsCsv.FileExtension))
				{
					m_selectedFileName += FileStreetsTripsCsv.FileExtension;
					fileTextBox.Text = m_selectedFileName;
				}
				else if(m_selectedFormat.Equals(FileEasyGps.FormatName) && !m_selectedFileName.ToLower().EndsWith(FileEasyGps.FileExtension))
				{
					m_selectedFileName += FileEasyGps.FileExtension;
					fileTextBox.Text = m_selectedFileName;
				}

				if(File.Exists(m_selectedFileName))
				{
					if(!Project.YesNoBox(this, m_selectedFileName + " already exists.\nDo you want to replace it?"))
					{
						return;
					}
				}
				doWrite();
			}
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		public void browseAndSave()
		{
			if(m_selectedFileName == null || m_selectedFileName.Length == 0)
			{
				saveFileDialog1.InitialDirectory = Project.fileInitialDirectory;
				saveFileDialog1.FileName = "";
				saveFileDialog1.DefaultExt = FileEasyGps.FileExtension;
			} 
//			else if(!File.Exists(m_selectedFileName))
//			{
//				saveFileDialog1.FileName = m_selectedFileName;
//			}
			else
			{
				saveFileDialog1.FileName = m_selectedFileName;
				saveFileDialog1.DefaultExt = m_selectedFileName.ToLower().EndsWith(FileStreetsTripsCsv.FileExtension)
												? FileStreetsTripsCsv.FileExtension : FileEasyGps.FileExtension;
			}
			saveFileDialog1.AddExtension = true;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			saveFileDialog1.Filter = m_selectedFormat + "(*" + FileEasyGps.FileExtension + ")|*" + FileEasyGps.FileExtension + "|" + FileKml.FormatNameShort + " (*" + FileKml.FileExtension + ")|*" + FileKml.FileExtension + "|comma separated Excel (*" + FileStreetsTripsCsv.FileExtension + ")|*" + FileStreetsTripsCsv.FileExtension + "|All files (*.*)|*.*";
			DialogResult result = saveFileDialog1.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				string fileName = saveFileDialog1.FileName;
				m_selectedFileName = fileName;

				m_selectedFormat = FileEasyGps.FormatName;
				if(fileName.ToLower().EndsWith(FileStreetsTripsCsv.FileExtension))
				{
					m_selectedFormat = FileStreetsTripsCsv.FormatName;
				}
				else if(fileName.ToLower().EndsWith(FileKml.FileExtension))
				{
					m_selectedFormat = FileKml.FormatName;
				}

				int pos = fileName.LastIndexOf("\\");
				if(pos > 0)
				{
					Project.fileInitialDirectory = fileName.Substring(0, pos);
				}
				fileTextBox.Text = m_selectedFileName;
				doWrite();
			}
			okButton.Enabled = fileTextBox.Text.Length > 0;
		}

		private bool hasSaved = false;
		protected static double minDateTimeTicks = DateTime.MinValue.AddDays(100.0d).Ticks;

		#region doWrite()

		/*
		 * see http://www.topografix.com/GPX/1/0 for more info
		 * 
		 * Validating your GPX document
				Validation is done using the Xerces XML parser. Download the latest Xerces distribution from the Apache website.
				 Windows users should download the "Latest Xerces-C++ Binary Package for Windows". Unzip the files, and locate the
				 SAXCount.exe program in the bin folder. This is a command-line utility that will validate your GPX file. 

			Assuming your GPX file is named my_gpx_file.gpx, and is located in the same folder as SaxCount.exe, use the following
			 command line to validate your file: 

					SaxCount.exe -v=always -n -s -f test.gpx

			If your file validates successfully, SAXCount will display a count of the elements in your file, like the following: 

					test.gpx: 1012 ms (4025 elems, 1916 attrs, 8048 spaces, 36109 chars)

			Any other output from SAXCount.exe indicates that your GPX file is incorrect. It is your responsibility to ensure that any GPX files you create validate successfully against the GPX schema. 
		 */

		public void doWrite()
		{
			string diag = "Selected format: " + m_selectedFormat + "\r\n\r\n"; 
			trkpointCount = 0;
			waypointCount = 0;
			// could be "new DateTimeFormatInfo().UniversalSortableDateTimePattern;" - but it has space instead of 'T'

			messageBoxDirty = true;
			messageTextBox.Text = diag;

			try 
			{
				if(m_selectedFormat.Equals(FileStreetsTripsCsv.FormatName))
				{
					hasSaved = FileAndZipIO.saveCsv(m_selectedFileName, m_tracks, m_saveTracks,
						m_waypoints, m_saveWaypoints, out waypointCount, out trkpointCount);
				}
				else if(m_selectedFormat.Equals(FileEasyGps.FormatName))
				{
					int tracksCount;
					hasSaved = FileAndZipIO.saveGpx(m_selectedFileName, m_tracks, m_saveTracks,
						m_waypoints, m_saveWaypoints, out waypointCount, out trkpointCount, out tracksCount);

					// try suggesting JPEG correlation here, if the folder has any .JPG files
					if(tracksCount > 0)
					{
						bool hasJpegs = false;
						try
						{
							FileInfo fi = new FileInfo(m_selectedFileName);
							DirectoryInfo di = fi.Directory;
							foreach(FileInfo fii in di.GetFiles())
							{
								if(fii.Name.ToLower().EndsWith(".jpg"))
								{
									hasJpegs = true;
									break;
								}
							}
						}
						catch
						{
						}
						if(hasJpegs)
						{
							string message = "The folder you selected contains images\r\n\r\nDo you want to relate them to trackpoints?";
							if(Project.YesNoBox(this, message))
							{
								Project.photoFileName = m_selectedFileName;
								Project.pmLastTabMode = 0;
								DlgPhotoManager dlg = new DlgPhotoManager(0);
								dlg.setImportButtonsAgitated();
								dlg.ShowDialog();
							}
						}
					}
				}
				else if(m_selectedFormat.Equals(FileKml.FormatName))
				{
					string name = new FileInfo(m_selectedFileName).Name;

					name = name.Substring(0, name.Length - FileKml.FileExtension.Length);
					
					GoogleEarthManager.saveTracksWaypoints(
						m_selectedFileName, name, m_tracks, m_saveTracks,
						m_waypoints, m_saveWaypoints, out waypointCount, out trkpointCount
					);
				}
				else
				{
					messageTextBox.Text = "Error: format " + m_selectedFormat + " not supported for writing.";
					LibSys.StatusBar.Error("FileExportForm:doWrite() format " + m_selectedFormat + " not supported for writing.");
					return;
				}

				WaypointsCache.isDirty = false;

				if(waypointCount > 0 || trkpointCount > 0)
				{
					diag += "OK: " + waypointCount + " waypoints and " + trkpointCount + " legs saved to file.";
					messageTextBox.ForeColor = Color.Black;

					FileInfo fi = new FileInfo(Project.GetLongPathName(m_selectedFileName));
					FormattedFileDescr fd = new FormattedFileDescr(fi.FullName, m_selectedFormat, persistCheckBox.Checked);
					Project.FileDescrListAdd(fd);

					if(!m_selectedFormat.Equals(FileKml.FormatName))	// can't read back kmz
					{
						Project.insertRecentFile(fi.FullName);
					}
				}
				else
				{
					diag += "Error: failed to save to file (0 waypoints, 0 legs).";
					messageTextBox.ForeColor = Color.Red;
				}
			
				messageTextBox.Text = diag;
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("FileExportForm:doWrite() " + e); //.Message);
				messageTextBox.Text = diag + e.Message;
				messageTextBox.ForeColor = Color.Red;
			}
		}

		#endregion

		private void persistCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			// value is used directly in doWrite()
			if(hasSaved && persistCheckBox.Checked)
			{
				Project.MessageBox(this, "Now click on \"Save\" to make the file mode to persistent");
			}
		}

		private void saveWptCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_saveWaypoints = saveWptCheckBox.Checked;
			showWhatIsSaved();
		}

		private void saveTracksCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_saveTracks = saveTracksCheckBox.Checked;
			showWhatIsSaved();
		}

		private void fileTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			showWhatIsSaved();

			switch (e.KeyData) 
			{
				case Keys.Enter:
					act();
					break;
			}
		}
	}
}
