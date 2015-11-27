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
using System.Globalization;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Data;
using System.Data.OleDb;

using QuakeMapApi;

namespace NtsbConverter
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Button openButton;
		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button processButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton filterNoneRadioButton;
		private System.Windows.Forms.RadioButton filterHARadioButton;
		private System.Windows.Forms.RadioButton filterWestRadioButton;
		private System.Windows.Forms.RadioButton filterCentralRadioButton;
		private System.Windows.Forms.RadioButton filterEastRadioButton;
		private System.Windows.Forms.Label filterLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			InitializeComponent();

			processButton.Enabled = false;
			filterNoneRadioButton.Checked = true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
            this.messageLabel = new System.Windows.Forms.Label();
            this.openButton = new System.Windows.Forms.Button();
            this.fileTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.filterLabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.filterEastRadioButton = new System.Windows.Forms.RadioButton();
            this.filterCentralRadioButton = new System.Windows.Forms.RadioButton();
            this.filterWestRadioButton = new System.Windows.Forms.RadioButton();
            this.filterHARadioButton = new System.Windows.Forms.RadioButton();
            this.filterNoneRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.processButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // messageLabel
            // 
            this.messageLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.messageLabel.Location = new System.Drawing.Point(0, 510);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(856, 40);
            this.messageLabel.TabIndex = 0;
            this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(520, 16);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 1;
            this.openButton.Text = "Open";
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // fileTextBox
            // 
            this.fileTextBox.Location = new System.Drawing.Point(120, 16);
            this.fileTextBox.Name = "fileTextBox";
            this.fileTextBox.Size = new System.Drawing.Size(360, 20);
            this.fileTextBox.TabIndex = 2;
            this.fileTextBox.Text = "C:\\Projects\\_aviation-crashes-db\\AV2007.MDB";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "NTSB Data File:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(480, 16);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(24, 23);
            this.browseButton.TabIndex = 4;
            this.browseButton.Text = "...";
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // dataGrid1
            // 
            this.dataGrid1.CaptionText = "Contents of the NTSB data file (only entries with coordinates specified)";
            this.dataGrid1.DataMember = "";
            this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(0, 80);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.Size = new System.Drawing.Size(856, 430);
            this.dataGrid1.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.filterLabel);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.processButton);
            this.panel1.Controls.Add(this.openButton);
            this.panel1.Controls.Add(this.fileTextBox);
            this.panel1.Controls.Add(this.browseButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(856, 80);
            this.panel1.TabIndex = 6;
            // 
            // filterLabel
            // 
            this.filterLabel.Location = new System.Drawing.Point(536, 48);
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Size = new System.Drawing.Size(304, 23);
            this.filterLabel.TabIndex = 8;
            this.filterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.filterEastRadioButton);
            this.panel2.Controls.Add(this.filterCentralRadioButton);
            this.panel2.Controls.Add(this.filterWestRadioButton);
            this.panel2.Controls.Add(this.filterHARadioButton);
            this.panel2.Controls.Add(this.filterNoneRadioButton);
            this.panel2.Location = new System.Drawing.Point(112, 48);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(416, 24);
            this.panel2.TabIndex = 7;
            // 
            // filterEastRadioButton
            // 
            this.filterEastRadioButton.Location = new System.Drawing.Point(344, 0);
            this.filterEastRadioButton.Name = "filterEastRadioButton";
            this.filterEastRadioButton.Size = new System.Drawing.Size(72, 24);
            this.filterEastRadioButton.TabIndex = 4;
            this.filterEastRadioButton.Text = "East";
            this.filterEastRadioButton.CheckedChanged += new System.EventHandler(this.filterRadioButton_CheckedChanged);
            // 
            // filterCentralRadioButton
            // 
            this.filterCentralRadioButton.Location = new System.Drawing.Point(264, 0);
            this.filterCentralRadioButton.Name = "filterCentralRadioButton";
            this.filterCentralRadioButton.Size = new System.Drawing.Size(72, 24);
            this.filterCentralRadioButton.TabIndex = 3;
            this.filterCentralRadioButton.Text = "Central";
            this.filterCentralRadioButton.CheckedChanged += new System.EventHandler(this.filterRadioButton_CheckedChanged);
            // 
            // filterWestRadioButton
            // 
            this.filterWestRadioButton.Location = new System.Drawing.Point(192, 0);
            this.filterWestRadioButton.Name = "filterWestRadioButton";
            this.filterWestRadioButton.Size = new System.Drawing.Size(72, 24);
            this.filterWestRadioButton.TabIndex = 2;
            this.filterWestRadioButton.Text = "West";
            this.filterWestRadioButton.CheckedChanged += new System.EventHandler(this.filterRadioButton_CheckedChanged);
            // 
            // filterHARadioButton
            // 
            this.filterHARadioButton.Location = new System.Drawing.Point(80, 0);
            this.filterHARadioButton.Name = "filterHARadioButton";
            this.filterHARadioButton.Size = new System.Drawing.Size(104, 24);
            this.filterHARadioButton.TabIndex = 1;
            this.filterHARadioButton.Text = "Hawaii/Alaska";
            this.filterHARadioButton.CheckedChanged += new System.EventHandler(this.filterRadioButton_CheckedChanged);
            // 
            // filterNoneRadioButton
            // 
            this.filterNoneRadioButton.Location = new System.Drawing.Point(8, 0);
            this.filterNoneRadioButton.Name = "filterNoneRadioButton";
            this.filterNoneRadioButton.Size = new System.Drawing.Size(72, 24);
            this.filterNoneRadioButton.TabIndex = 0;
            this.filterNoneRadioButton.Text = "none";
            this.filterNoneRadioButton.CheckedChanged += new System.EventHandler(this.filterRadioButton_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 23);
            this.label2.TabIndex = 6;
            this.label2.Text = "Filter:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // processButton
            // 
            this.processButton.Location = new System.Drawing.Point(680, 16);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(168, 23);
            this.processButton.TabIndex = 5;
            this.processButton.Text = "Pass data to QuakeMap";
            this.processButton.Click += new System.EventHandler(this.processButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(856, 550);
            this.Controls.Add(this.dataGrid1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.messageLabel);
            this.Name = "Form1";
            this.Text = "NTSB Crash Data Converter";
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
        //[STAThread]
        //static void Main() 
        //{
        //    Application.Run(new Form1());
        //}

        public delegate void UpdateTextHandler(string mymessage);

        private void UpdateMessageText(string mymessage)
        {
            messageLabel.Text = mymessage;
        }

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

			if(File.Exists(fileTextBox.Text))
			{
				openFileDialog.FileName = fileTextBox.Text;
			}
			else
			{
				openFileDialog.InitialDirectory = "C:\\";
			}
			openFileDialog.DefaultExt = "";
			openFileDialog.AddExtension = false;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "Access95 MDB files (*.mdb)|*.mdb|All files (*.*)|*.*";

			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileInfo fi = new FileInfo(openFileDialog.FileName);
				fileTextBox.Text = fi.FullName;
				open();
			}
		}

		private const string DBConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Data Source=";

		private DataSet m_dataSet = null;

		/// <summary>
		/// Just open the file and show it in the data grid. Allow Process button to work, if open is a success.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void openButton_Click(object sender, System.EventArgs e)
		{
			open();
		}

		private void open()
		{
			processButton.Enabled = false;
			dataGrid1.DataSource = null;

			string strSelect = "select * from events where longitude like '%W'";

			// Create the dataset and add the table to it:
			m_dataSet = new DataSet();
			m_dataSet.Tables.Add("events");
	
			// Create Access objects:
			string strAccessConn = DBConnectionString + fileTextBox.Text;

			OleDbConnection myAccessConn = new OleDbConnection(strAccessConn);
			OleDbCommand mySelectCommand = new OleDbCommand(strSelect, myAccessConn);

			myAccessConn.Open();
			OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(mySelectCommand);

			try
			{
				myDataAdapter.Fill(m_dataSet,"events");

				DataColumnCollection drc = m_dataSet.Tables["events"].Columns;
				DataRowCollection dra = m_dataSet.Tables["events"].Rows;
				if(dra.Count == 0) 
				{
					messageLabel.Text = "Error: empty data set.";
				} 
				else 
				{
					int count = dra.Count;

					messageLabel.Text = "OK: opened data set. Records count: " + count;

					dataGrid1.DataSource = m_dataSet.Tables["events"];

					processButton.Enabled = count > 0;
				}
			}
			catch (Exception exc)
			{
				messageLabel.Text = "Error: Caught an exception: " + exc.Message;
			}
			finally
			{
				// Close the connection when done with it.
				myAccessConn.Close();
				myDataAdapter.Dispose();
				m_dataSet.Dispose();
			}
		}

		private void processButton_Click(object sender, System.EventArgs e)
		{
			process();
		}

		private void process()
		{
			m_source = new FileInfo(fileTextBox.Text).Name;

			m_workerThread = new Thread( new ThreadStart(doWork));
			m_workerThread.IsBackground = true;	// terminate with the main process
			m_workerThread.Name = "Worker";
			m_workerThread.Priority = ThreadPriority.BelowNormal;
			m_workerThread.Start();
		}

		private Thread m_workerThread = null;
		private QMApiLib qmApiLib = new QMApiLib();
		string m_source = "NTSB";

		private void doWork()
		{
			try
			{
				// first make sure we are at a good altitude:
				string strCmd = "/map=aerial";
				qmApiLib.CommandMappingEngine(strCmd);
				
				qmApiLib.resetZoom();		// prepare for a zoom into whole set of waypoints created below

				DataRowCollection dra = m_dataSet.Tables["events"].Rows;

				int count = 0;
				foreach(DataRow row in dra)
				{
					string sLat = "" + row["latitude"];
					double lat = toDegree(sLat);
					string sLon = "" + row["longitude"];
					double lon = toDegree(sLon);

					if(lon < lngMin || lon > lngMax)
					{
						continue;
					}

					string eventId = "" + row["ev_id"];
					DateTime date = DateTime.MinValue;
					string sDT0 = "";
					try
					{
						DateTime dateLoc = (DateTime)row["ev_date"];
						string sTimeLoc = "" + row["ev_time"];
						if(sTimeLoc.Length < 4)
						{
							sTimeLoc = "0" + sTimeLoc;
						}
						sTimeLoc = sTimeLoc.Substring(0,2) + ":" + sTimeLoc.Substring(2,2) + ":00";
						string timeZone = "" + row["ev_tmzn"];

						sDT0 = dateLoc.ToShortDateString() + " " + sTimeLoc + " " + timeZone;

						string timeShift = "+0";
						switch (timeZone)
						{
							case "GMT":
							case "UTC":
								break;

							case "ADT":
								timeShift = "-9";
								break;
							case "AST":
								timeShift = "-10";
								break;
							case "BDT":				// British
								timeShift = "+0";
								break;
							case "BST":
								timeShift = "+1";
								break;
							case "CDT":
								timeShift = "-5";
								break;
							case "CST":
								timeShift = "-6";
								break;
							case "EDT":
								timeShift = "-4";
								break;
							case "EST":
								timeShift = "-5";
								break;
							case "HDT":
								timeShift = "-10";
								break;
							case "HST":
								timeShift = "-11";
								break;
							case "MDT":
								timeShift = "-6";
								break;
							case "MST":
								timeShift = "-7";
								break;
							case "PDT":
								timeShift = "-7";
								break;
							case "PST":
								timeShift = "-8";
								break;
							case "YDT":					// Yukon
								timeShift = "-9";
								break;
							case "YST":
								timeShift = "-10";
								break;
						}
						string sDT = dateLoc.ToShortDateString() + " " + sTimeLoc + timeShift;

						string format = "M/d/yyyy H:mm:ssz";

						CultureInfo en = new CultureInfo("en-US");
						Thread.CurrentThread.CurrentCulture = en;

						DateTime timeLoc = DateTime.ParseExact(sDT, format, en.DateTimeFormat);

						TimeZone tz = TimeZone.CurrentTimeZone;

						date = tz.ToUniversalTime(timeLoc);
					}
					catch {}
					string type = "" + row["ev_type"];
					string injLevel = "" + row["ev_highest_injury"];
					int fatalities = 0;
					string desc = "" + injLevel;
					
					try
					{
						fatalities = Convert.ToInt32("" + row["inj_tot_f"]);
					}
					catch {}
					
					if(fatalities > 0)
					{
						type += "-" + injLevel + "-" + fatalities;
						desc += " - " + fatalities + " fatalities";
					}

					desc += " " + sDT0;

                    this.Invoke(new UpdateTextHandler(UpdateMessageText), new object[1] { eventId + " " + sLat + "=" + lat + " " + sLon + "=" + lon });

					string url = "http://www.ntsb.gov/ntsb/brief.asp?ev_id=" + eventId;

					CreateInfo ci = new CreateInfo();

					ci.lng = lon;
					ci.lat = lat;
					ci.name = eventId;
					ci.urlName = type + (date.Equals(DateTime.MinValue) ? "" : "-" + date.ToShortDateString());
					ci.type = "waypoint";
					ci.typeExtra = "NTSB Crash";
					ci.url = url;
					ci.dateTime = date;
					ci.desc = desc;
					ci.source = "NTSB Database - " + m_source;

					bool keepInView = false;

                    this.Invoke(new UpdateTextHandler(UpdateMessageText), new object[1] { "Creating Waypoint" + eventId });

					qmApiLib.createWaypoint(ci, keepInView);

					count++;

					Thread.Sleep(10);
				}
                this.Invoke(new UpdateTextHandler(UpdateMessageText), new object[1] { "Created " + count + " waypoints" });

				// perform zoom into the whole set of points:
				qmApiLib.doZoom();
			}
			catch (Exception exc)
			{
                this.Invoke(new UpdateTextHandler(UpdateMessageText), new object[1] { exc.Message });
			}
		}

		protected double toDegree(string str)	// str in form: 293313N or 0982859W             117.23'45 or 117,42.945
		{
			str = str.Trim();

			string sDeg = "0";
			string sMin = "0";
			string sSec = "0";
			double sign = str.EndsWith("W") || str.EndsWith("S") ? -1 : 1;

			if(str.IndexOf(":") >= 0)
			{
				str = str.Substring(0, str.Length - 1).Trim();
				string[] split = str.Split(new char[] { ':' });
				try
				{
					sDeg = split[0];
					sMin = split[1];
					sSec = split[2];
				}
				catch {}
			}
			else
			{
				if(str.Length > 7)
				{
					sDeg = str.Substring(0, 3);
					sMin = str.Substring(3, 2);
					sSec = str.Substring(5, 2);
				}
                else if (str.Length >= 6)
				{
					sDeg = str.Substring(0, 2);
					sMin = str.Substring(2, 2);
					sSec = str.Substring(4, 2);
				}
			}

			try
			{
				int deg = Convert.ToInt32(sDeg);
				int min = Convert.ToInt32(sMin);
				int sec = Convert.ToInt32(sSec);

				return (((double)deg) + ((double)min) / 60 + ((double)sec) / 3600) * sign;
			}
			catch
			{
				return 0.0d;
			}
		}

		double lngMax =  180.0d;
		double lngMin = -180.0d;

		private void filterRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(filterNoneRadioButton.Checked)
			{
				lngMax =  180.0d;
				lngMin = -180.0d;
			}
			else if(filterHARadioButton.Checked)
			{
				lngMax = -125.0d;
				lngMin = -180.0d;
			}
			else if(filterWestRadioButton.Checked)
			{
				lngMax = -110.0d;
				lngMin = -125.0d;
			}
			else if(filterCentralRadioButton.Checked)
			{
				lngMax =  -90.0d;
				lngMin = -110.0d;
			}
			else if(filterEastRadioButton.Checked)
			{
				lngMax =  180.0d;
				lngMin =  -90.0d;
			}
			filterLabel.Text = filterNoneRadioButton.Checked ? "all coordinates" : ("longitudes from  " + lngMin + "  to  " + lngMax);
		}
	}
}
