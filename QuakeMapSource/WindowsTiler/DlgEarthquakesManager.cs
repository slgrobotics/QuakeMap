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
using System.Threading;
using System.Data;

using LibSys;
using LibGui;
using LibGeo;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgEarthquakesManager.
	/// </summary>
	public class DlgEarthquakesManager : System.Windows.Forms.Form, IEnableDisable
	{
		private static SortableDateTime sortableDateTime = new SortableDateTime();	// for GetType()

		private PictureManager m_pictureManager;
		private CameraManager m_cameraManager;

		private static int m_timeFilterMode = -1;	// no time filter
		private static double aroundTimeMinutes = 60.0d;
		private static int selectedTab = 1;
		private static int m_scope = 0;				// 0 - world, 1 - visible on map, 2 - selected

		private int m_mode;
		private bool inSet = false;
		private bool playing = false;

		private MyDataGrid eqDataGrid;
		private DataSet m_eqDS = null;
		private DataGridTableStyle m_eqTS = null;

		private LibGui.GraphByTimeControl graphByTimeControl;

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage eqTabPage;
		private System.Windows.Forms.TabPage filterTabPage;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button closeButton1;
		private System.Windows.Forms.Button stopButton;
		private System.Windows.Forms.Button playButton;
		private System.Windows.Forms.Label filterMessageLabel;
		private System.Windows.Forms.RadioButton noFilterRadioButton;
		private System.Windows.Forms.ComboBox aroundTimeComboBox;
		private System.Windows.Forms.DateTimePicker aroundTimeTimeDateTimePicker;
		private System.Windows.Forms.DateTimePicker aroundTimeDateTimePicker;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.RadioButton aroundTimeRadioButton;
		private System.Windows.Forms.RadioButton fromToRadioButton;
		private System.Windows.Forms.DateTimePicker toTimeDateTimePicker;
		private System.Windows.Forms.DateTimePicker fromTimeDateTimePicker;
		private System.Windows.Forms.DateTimePicker toDateTimePicker;
		private System.Windows.Forms.DateTimePicker fromDateTimePicker;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown magFromNumericUpDown;
		private System.Windows.Forms.NumericUpDown magToNumericUpDown;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button zoomButton;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.RadioButton scopeWorldRadioButton;
		private System.Windows.Forms.RadioButton scopeViewRadioButton;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.CheckBox magFilterCheckBox;
		private System.Windows.Forms.TabPage graphTabPage;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button closeButton2;
		private System.Windows.Forms.Button showSelectedButton;
		private System.Windows.Forms.RadioButton scopeFilteredRadioButton;
		private System.Windows.Forms.RadioButton scopeSelectedGraphRadioButton;
		private System.Windows.Forms.RadioButton scopeViewGraphRadioButton;
		private System.Windows.Forms.RadioButton scopeWorldGraphRadioButton;
		private System.Windows.Forms.Button browseGraphButton;
		private System.Windows.Forms.Button zoomGraphButton;
		private System.Windows.Forms.Panel graphPanel;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button applyFilterButton;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button exportButton;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Panel gridPanel;
		private System.Windows.Forms.Button hideButton;
		private System.Windows.Forms.Button showButton;
		private System.Windows.Forms.Button firstInViewButton;
		private System.Windows.Forms.Button applySelectedButton;
		private System.Windows.Forms.RadioButton timeLocalRadioButton;
		private System.Windows.Forms.RadioButton timeUtcRadioButton;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.RadioButton timeUtc2RadioButton;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.RadioButton timeLocal2RadioButton;
		private System.Windows.Forms.RadioButton timeLocal3RadioButton;
		private System.Windows.Forms.RadioButton timeUtc3RadioButton;
		private System.Windows.Forms.Label utcFilterLabel;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Button toWaypointsButton;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgEarthquakesManager(PictureManager pictureManager, CameraManager cameraManager, int mode, int scope)
		{
			m_pictureManager = pictureManager;
			m_cameraManager = cameraManager; 
			m_mode = mode;
			if(scope != -1)		// or, stay with previously selected scope
			{
				m_scope = scope;
			}

			//EarthquakesCache.RefreshEarthquakesDisplayed();	// make sure we have correct data to fill the datagrid
			//												// for the visible map.
			
			InitializeComponent();

			Project.setDlgIcon(this);
		}

		private void DlgEarthquakesManager_Load(object sender, System.EventArgs e)
		{
			inSet = true;	// avoid collecting values on programmatic set
			Cursor.Current = Cursors.WaitCursor;

			this.gridPanel.SuspendLayout();

			eqDataGrid = new MyDataGrid();
			((System.ComponentModel.ISupportInitialize)(eqDataGrid)).BeginInit();
			this.gridPanel.Controls.Add(eqDataGrid);
			this.eqDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.eqDataGrid.DataMember = "";
			this.eqDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eqDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.eqDataGrid.Name = "eqDataGrid";
			this.eqDataGrid.Size = new System.Drawing.Size(824, 511);
			this.eqDataGrid.TabIndex = 6;
			this.eqDataGrid.RowHeadersVisible = false;
			this.eqDataGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.eqDataGrid_MouseUp);
			((System.ComponentModel.ISupportInitialize)(this.eqDataGrid)).EndInit();

			this.gridPanel.ResumeLayout();

			this.graphTabPage.SuspendLayout();

			graphByTimeControl = new LibGui.GraphByTimeControl();
			this.graphPanel.Controls.Add(graphByTimeControl);
			this.graphByTimeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.graphByTimeControl.Name = "graphByTimeControl";

			this.graphTabPage.ResumeLayout();

			switch (m_scope)
			{
				case 0:		// world
					scopeWorldRadioButton.Checked = true;
					scopeViewRadioButton.Checked = false;
					break;
				default:
				case 1:		// visible on map
					m_scope = 1;
					scopeWorldRadioButton.Checked = false;
					scopeViewRadioButton.Checked = true;
					break;
			}

			if(Project.fitsScreen(100, 100, Project.eqManagerWidth, Project.eqManagerHeight))
			{
				this.Size = new Size(Project.eqManagerWidth, Project.eqManagerHeight);
			}

			this.fromTimeDateTimePicker.CustomFormat = "h:mm tt";
			this.toTimeDateTimePicker.CustomFormat = "h:mm tt";
			this.aroundTimeTimeDateTimePicker.CustomFormat = "h:mm tt";

			SetFilterDefaults();
			inSet = true;	// once again, as SetFilterDefaults resets it

			switch(m_mode)
			{
				default:
				case 0:			// stay on the last chosen 
					rebuildTableTab(false);
					if(selectedTab == 1)
					{
						rebuildGraphTab();
					}
					tabControl1.SelectedIndex = selectedTab;
					break;
				case 1:			// Table tab, show the DataGrid:
					rebuildTableTab(false);
					break;
				case 2:			// Graph tab
					// make sure the data is there:
					rebuildTableTab(false);
					rebuildGraphTab();
					// go to Graph tab
					tabControl1.SelectedIndex = 1;
					break;
				case 3:			// Filter tab:
					tabControl1.SelectedIndex = 2;
					break;
				case 4:			// stay on the last chosen, unless it was a Filter (then switch to Graph) 
					if(selectedTab != 1 && selectedTab != 0)
					{
						selectedTab = 1;
					}
					rebuildTableTab(false);
					if(selectedTab == 1)
					{
						rebuildGraphTab();
					}
					tabControl1.SelectedIndex = selectedTab;
					break;
			}

			magFilterCheckBox.Checked = TimeMagnitudeFilter.EnabledMagn;	// used in setFilter...() below

			switch(m_timeFilterMode)
			{
				case 0:		// from/to
					setFilterModeFromTo();
					fromToRadioButton.Checked = true;
					break;
				case 1:		// around time
					setFilterModeAroundTime();
					aroundTimeRadioButton.Checked = true;
					break;
				default:	// no filter
					setFilterModeNone();
					noFilterRadioButton.Checked = true;
					break;
			}

			aroundTimeComboBox.SelectedIndex = aroundTimeIndex(aroundTimeMinutes);

			magFromNumericUpDown.Enabled = magFilterCheckBox.Checked;
			magToNumericUpDown.Enabled = magFilterCheckBox.Checked;
			scopeSelectedGraphRadioButton.Enabled = false;				// do not allow clicking on it. Use button to zoom to selected interval

			magFromNumericUpDown.Value = (decimal)TimeMagnitudeFilter.fromMagn;
			magToNumericUpDown.Value = (decimal)TimeMagnitudeFilter.toMagn;

			timeUtcRadioButton.Checked  = Project.useUtcTime;
			timeUtc2RadioButton.Checked = Project.useUtcTime;
			timeUtc3RadioButton.Checked = Project.useUtcTime;
			timeLocalRadioButton.Checked  = !Project.useUtcTime;
			timeLocal2RadioButton.Checked = !Project.useUtcTime;
			timeLocal3RadioButton.Checked = !Project.useUtcTime;
			setUtcLabels();

			inSet = false;

			if(EarthquakesCache.EarthquakesAll.Count < 10)
			{
				string msg =  "There are no earthquakes in the cache.\n\nThis may happen because you opened this form while earthquakes were still loading into the program.\n\nPlease close this form and reopen when earthquakes are fully loaded.";
				Project.ShowPopup(tabControl1, msg, Point.Empty);
			}
			else
			{
				if(m_scope != 0 && m_eqDS.Tables[0].Rows.Count == 0)
				{
					//Project.MessageBox(this, "There are no earthquakes on the visible map.\n\nTry switching to \"world wide\" to see more, or zoom out.");
					//Point popupOffset = new Point(5, -5);
					//Point screenPoint = scopeFilteredRadioButton.PointToScreen(popupOffset);
					string msg =  "There are no earthquakes on the visible map.\nTry switching to \"world wide\" to see more, or zoom out.";
					Project.ShowPopup(scopeFilteredRadioButton, msg, Point.Empty);
				}
			}
		}

		private void setUtcLabels()
		{
			string msg = Project.useUtcTime ? "all times are UTC" : "all times are local\nto YOUR COMPUTER";
			utcFilterLabel.Text = msg;
			msg = Project.useUtcTime ? "all times are UTC" : "all times are local to YOUR COMPUTER";
			messageLabel.Text = msg;
		}

		private void DlgEarthquakesManager_Activated(object sender, System.EventArgs e)
		{
			if(tabControl1.SelectedTab == graphTabPage)
			{
				graphByTimeControl.Focus();
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

		/*
			"1  min", "2  min", "5  min", "10 min", "15 min", "20 min", "30 min", "45 min",
			"1 hour", "2 hours", "4 hours", "8 hours", "12 hours", "18 hours", "1 day",
			"2 days", "3 days", "5 days", "10 days"
		 */
		double[] aroundTimes = new double[] {
												1.0d, 2.0d, 5.0d, 10.0d, 15.0d, 20.0d, 30.0d, 45.0d,
												60.0d, 120.0d, 240.0d, 480.0d, 720.0d, 1080.0d, 1440.0d,
												2880.0d, 4320.0d, 7200.0d, 14400.0d
											};

		private int aroundTimeIndex(double timeMinutes)
		{
			int indx;
			for(indx=0; indx < aroundTimes.Length ;indx++)
			{
				if(aroundTimes[indx] >= timeMinutes)
				{
					break;
				}
			}
			return indx;
		}

		private void SetFilterDefaults()
		{
			inSet = true;
			int tmp = 0;

			if(!TimeMagnitudeFilter.fromTime.Equals(DateTime.MaxValue))
			{

				fromDateTimePicker.Value = Project.useUtcTime ? TimeMagnitudeFilter.fromTime : Project.zuluToLocal(TimeMagnitudeFilter.fromTime);
				fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
				tmp++;
			}
			else
			{
				try
				{
					fromDateTimePicker.Value = Project.useUtcTime ? EarthquakesCache.getMinTime() : Project.zuluToLocal(EarthquakesCache.getMinTime());
					fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
					tmp++;
				} 
				catch {}
			}

			if(!TimeMagnitudeFilter.toTime.Equals(DateTime.MinValue))
			{
				toDateTimePicker.Value = Project.useUtcTime ? TimeMagnitudeFilter.toTime : Project.zuluToLocal(TimeMagnitudeFilter.toTime);
				toTimeDateTimePicker.Value = toDateTimePicker.Value;
				tmp++;
			}
			else
			{
				try
				{
					toDateTimePicker.Value = Project.useUtcTime ? EarthquakesCache.getMaxTime() : Project.zuluToLocal(EarthquakesCache.getMaxTime());
					toTimeDateTimePicker.Value = toDateTimePicker.Value;
					tmp++;
				} 
				catch {}
			}

			if(tmp == 2)
			{
				try
				{
					aroundTimeDateTimePicker.Value = toDateTimePicker.Value;
					aroundTimeTimeDateTimePicker.Value = toDateTimePicker.Value;
				} 
				catch {}
			}

			inSet = false;
			//filterMessageLabel.Text = "Zulu From: " + TimeMagnitudeFilter.fromTime + "\r\n" + "   To: " + TimeMagnitudeFilter.toTime;
		}

		private void setFilterModeFromTo()
		{
			TimeMagnitudeFilter.EnabledTime = true;
			TimeMagnitudeFilter.EnabledMagn = magFilterCheckBox.Checked;
			playing = false;
			fromDateTimePicker.Enabled = true;
			toDateTimePicker.Enabled = true;
			fromTimeDateTimePicker.Enabled = true;
			toTimeDateTimePicker.Enabled = true;
			aroundTimeTimeDateTimePicker.Enabled = false;
			aroundTimeDateTimePicker.Enabled = false;
			aroundTimeComboBox.Enabled = false;
			playButton.Enabled = false;
			stopButton.Enabled = false;
		}

		private void setFilterModeAroundTime()
		{
			TimeMagnitudeFilter.EnabledTime = true;
			TimeMagnitudeFilter.EnabledMagn = magFilterCheckBox.Checked;
			fromDateTimePicker.Enabled = false;
			toDateTimePicker.Enabled = false;
			fromTimeDateTimePicker.Enabled = false;
			toTimeDateTimePicker.Enabled = false;
			aroundTimeTimeDateTimePicker.Enabled = true;
			aroundTimeDateTimePicker.Enabled = true;
			aroundTimeComboBox.Enabled = true;
			playButton.Enabled = true;
			stopButton.Enabled = false;
		}

		private void setFilterModeNone()
		{
			TimeMagnitudeFilter.EnabledTime = false;
			TimeMagnitudeFilter.EnabledMagn = magFilterCheckBox.Checked;
			playing = false;
			fromDateTimePicker.Enabled = false;
			toDateTimePicker.Enabled = false;
			fromTimeDateTimePicker.Enabled = false;
			toTimeDateTimePicker.Enabled = false;
			aroundTimeTimeDateTimePicker.Enabled = false;
			aroundTimeDateTimePicker.Enabled = false;
			aroundTimeComboBox.Enabled = false;
			playButton.Enabled = false;
			stopButton.Enabled = false;
		}

		#region IEnableDisable implementation

		public void disable(int feature)
		{
			switch(feature) 
			{
				case GraphByTimeControl.ED_SELECTED:
					showSelectedButton.Enabled = false;
					applySelectedButton.Enabled = false;
					scopeFilteredRadioButton.Enabled = false;
					scopeSelectedGraphRadioButton.Enabled = false;
					break;
				case GraphByTimeControl.ED_ALLDATA:
					//m_buttonAllData.disable();
					break;
				case GraphByTimeControl.ED_ZOOM:
					zoomGraphButton.Enabled = false;
					break;
				case GraphByTimeControl.ED_BROWSE:
					browseGraphButton.Enabled = false;
					break;
			}
		}

		public void enable(int feature)
		{
			switch(feature) 
			{
				case 0:
					showSelectedButton.Enabled = true;
					try
					{
						// DateTime range that can be selected is much more that DateTimePicker can take:
						if((fromDateTimePicker.MinDate - graphByTimeControl.getStartSelectedDateTime()).Seconds < 0)
						{
							applySelectedButton.Enabled = true;
						}
					} 
					catch {}
					scopeFilteredRadioButton.Enabled = true;
					//scopeSelectedGraphRadioButton.Enabled = true;
					break;
				case 1:
					//m_buttonAllData.enable();
					break;
				case 2:
					zoomGraphButton.Enabled = true;
					break;
				case 3:
					browseGraphButton.Enabled = true;
					break;
			}
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.eqTabPage = new System.Windows.Forms.TabPage();
			this.gridPanel = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.toWaypointsButton = new System.Windows.Forms.Button();
			this.panel5 = new System.Windows.Forms.Panel();
			this.label7 = new System.Windows.Forms.Label();
			this.timeUtcRadioButton = new System.Windows.Forms.RadioButton();
			this.timeLocalRadioButton = new System.Windows.Forms.RadioButton();
			this.showButton = new System.Windows.Forms.Button();
			this.hideButton = new System.Windows.Forms.Button();
			this.exportButton = new System.Windows.Forms.Button();
			this.scopeFilteredRadioButton = new System.Windows.Forms.RadioButton();
			this.browseButton = new System.Windows.Forms.Button();
			this.scopeViewRadioButton = new System.Windows.Forms.RadioButton();
			this.scopeWorldRadioButton = new System.Windows.Forms.RadioButton();
			this.messageLabel = new System.Windows.Forms.Label();
			this.zoomButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.graphTabPage = new System.Windows.Forms.TabPage();
			this.graphPanel = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.label8 = new System.Windows.Forms.Label();
			this.timeUtc2RadioButton = new System.Windows.Forms.RadioButton();
			this.timeLocal2RadioButton = new System.Windows.Forms.RadioButton();
			this.applySelectedButton = new System.Windows.Forms.Button();
			this.scopeSelectedGraphRadioButton = new System.Windows.Forms.RadioButton();
			this.scopeViewGraphRadioButton = new System.Windows.Forms.RadioButton();
			this.scopeWorldGraphRadioButton = new System.Windows.Forms.RadioButton();
			this.browseGraphButton = new System.Windows.Forms.Button();
			this.zoomGraphButton = new System.Windows.Forms.Button();
			this.showSelectedButton = new System.Windows.Forms.Button();
			this.closeButton2 = new System.Windows.Forms.Button();
			this.filterTabPage = new System.Windows.Forms.TabPage();
			this.panel4 = new System.Windows.Forms.Panel();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.utcFilterLabel = new System.Windows.Forms.Label();
			this.firstInViewButton = new System.Windows.Forms.Button();
			this.magToNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.filterMessageLabel = new System.Windows.Forms.Label();
			this.noFilterRadioButton = new System.Windows.Forms.RadioButton();
			this.aroundTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.toTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.fromDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.magFilterCheckBox = new System.Windows.Forms.CheckBox();
			this.aroundTimeRadioButton = new System.Windows.Forms.RadioButton();
			this.fromTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.label4 = new System.Windows.Forms.Label();
			this.playButton = new System.Windows.Forms.Button();
			this.aroundTimeComboBox = new System.Windows.Forms.ComboBox();
			this.stopButton = new System.Windows.Forms.Button();
			this.aroundTimeTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.toDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.label1 = new System.Windows.Forms.Label();
			this.magFromNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.fromToRadioButton = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label9 = new System.Windows.Forms.Label();
			this.timeLocal3RadioButton = new System.Windows.Forms.RadioButton();
			this.timeUtc3RadioButton = new System.Windows.Forms.RadioButton();
			this.applyFilterButton = new System.Windows.Forms.Button();
			this.closeButton1 = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.eqTabPage.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel5.SuspendLayout();
			this.graphTabPage.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel6.SuspendLayout();
			this.filterTabPage.SuspendLayout();
			this.panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.magToNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.magFromNumericUpDown)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.eqTabPage,
																					  this.graphTabPage,
																					  this.filterTabPage});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(759, 416);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.TabStop = false;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// eqTabPage
			// 
			this.eqTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.gridPanel,
																					this.panel2});
			this.eqTabPage.Location = new System.Drawing.Point(4, 22);
			this.eqTabPage.Name = "eqTabPage";
			this.eqTabPage.Size = new System.Drawing.Size(751, 390);
			this.eqTabPage.TabIndex = 0;
			this.eqTabPage.Text = "Table";
			// 
			// gridPanel
			// 
			this.gridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridPanel.Name = "gridPanel";
			this.gridPanel.Size = new System.Drawing.Size(751, 335);
			this.gridPanel.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.toWaypointsButton,
																				 this.panel5,
																				 this.showButton,
																				 this.hideButton,
																				 this.exportButton,
																				 this.scopeFilteredRadioButton,
																				 this.browseButton,
																				 this.scopeViewRadioButton,
																				 this.scopeWorldRadioButton,
																				 this.messageLabel,
																				 this.zoomButton,
																				 this.closeButton});
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 335);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(751, 55);
			this.panel2.TabIndex = 5;
			// 
			// toWaypointsButton
			// 
			this.toWaypointsButton.Location = new System.Drawing.Point(5, 30);
			this.toWaypointsButton.Name = "toWaypointsButton";
			this.toWaypointsButton.Size = new System.Drawing.Size(75, 21);
			this.toWaypointsButton.TabIndex = 19;
			this.toWaypointsButton.Text = "To Wpt";
			this.toWaypointsButton.Click += new System.EventHandler(this.toWaypointsButton_Click);
			// 
			// panel5
			// 
			this.panel5.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label7,
																				 this.timeUtcRadioButton,
																				 this.timeLocalRadioButton});
			this.panel5.Location = new System.Drawing.Point(560, 0);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(110, 55);
			this.panel5.TabIndex = 18;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(0, 16);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(45, 22);
			this.label7.TabIndex = 17;
			this.label7.Text = "Time:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// timeUtcRadioButton
			// 
			this.timeUtcRadioButton.Location = new System.Drawing.Point(50, 5);
			this.timeUtcRadioButton.Name = "timeUtcRadioButton";
			this.timeUtcRadioButton.Size = new System.Drawing.Size(57, 22);
			this.timeUtcRadioButton.TabIndex = 15;
			this.timeUtcRadioButton.Text = "UTC";
			this.timeUtcRadioButton.CheckedChanged += new System.EventHandler(this.timeUtcRadioButton_CheckedChanged);
			// 
			// timeLocalRadioButton
			// 
			this.timeLocalRadioButton.Location = new System.Drawing.Point(50, 25);
			this.timeLocalRadioButton.Name = "timeLocalRadioButton";
			this.timeLocalRadioButton.Size = new System.Drawing.Size(57, 23);
			this.timeLocalRadioButton.TabIndex = 16;
			this.timeLocalRadioButton.Text = "local";
			this.timeLocalRadioButton.CheckedChanged += new System.EventHandler(this.timeLocalRadioButton_CheckedChanged);
			// 
			// showButton
			// 
			this.showButton.Location = new System.Drawing.Point(402, 5);
			this.showButton.Name = "showButton";
			this.showButton.Size = new System.Drawing.Size(68, 21);
			this.showButton.TabIndex = 7;
			this.showButton.Text = "Show";
			this.showButton.Click += new System.EventHandler(this.showButton_Click);
			// 
			// hideButton
			// 
			this.hideButton.Location = new System.Drawing.Point(329, 5);
			this.hideButton.Name = "hideButton";
			this.hideButton.Size = new System.Drawing.Size(68, 21);
			this.hideButton.TabIndex = 6;
			this.hideButton.Text = "Hide";
			this.hideButton.Click += new System.EventHandler(this.hideButton_Click);
			// 
			// exportButton
			// 
			this.exportButton.Location = new System.Drawing.Point(5, 5);
			this.exportButton.Name = "exportButton";
			this.exportButton.Size = new System.Drawing.Size(75, 21);
			this.exportButton.TabIndex = 0;
			this.exportButton.Text = "Export";
			this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
			// 
			// scopeFilteredRadioButton
			// 
			this.scopeFilteredRadioButton.Location = new System.Drawing.Point(168, 36);
			this.scopeFilteredRadioButton.Name = "scopeFilteredRadioButton";
			this.scopeFilteredRadioButton.Size = new System.Drawing.Size(127, 20);
			this.scopeFilteredRadioButton.TabIndex = 5;
			this.scopeFilteredRadioButton.Text = "filtered";
			this.scopeFilteredRadioButton.CheckedChanged += new System.EventHandler(this.scopeFilteredRadioButton_CheckedChanged);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(85, 30);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(75, 22);
			this.browseButton.TabIndex = 2;
			this.browseButton.Text = "Browse";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// scopeViewRadioButton
			// 
			this.scopeViewRadioButton.Location = new System.Drawing.Point(168, 18);
			this.scopeViewRadioButton.Name = "scopeViewRadioButton";
			this.scopeViewRadioButton.Size = new System.Drawing.Size(137, 20);
			this.scopeViewRadioButton.TabIndex = 4;
			this.scopeViewRadioButton.Text = "on the visible map";
			this.scopeViewRadioButton.CheckedChanged += new System.EventHandler(this.scopeViewRadioButton_CheckedChanged);
			// 
			// scopeWorldRadioButton
			// 
			this.scopeWorldRadioButton.Location = new System.Drawing.Point(168, 0);
			this.scopeWorldRadioButton.Name = "scopeWorldRadioButton";
			this.scopeWorldRadioButton.Size = new System.Drawing.Size(122, 20);
			this.scopeWorldRadioButton.TabIndex = 3;
			this.scopeWorldRadioButton.Text = "world wide";
			this.scopeWorldRadioButton.CheckedChanged += new System.EventHandler(this.scopeWorldRadioButton_CheckedChanged);
			// 
			// messageLabel
			// 
			this.messageLabel.ForeColor = System.Drawing.Color.Red;
			this.messageLabel.Location = new System.Drawing.Point(305, 35);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(252, 15);
			this.messageLabel.TabIndex = 14;
			// 
			// zoomButton
			// 
			this.zoomButton.Location = new System.Drawing.Point(85, 5);
			this.zoomButton.Name = "zoomButton";
			this.zoomButton.Size = new System.Drawing.Size(75, 21);
			this.zoomButton.TabIndex = 1;
			this.zoomButton.Text = "Go There";
			this.zoomButton.Click += new System.EventHandler(this.zoomButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(675, 15);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(69, 22);
			this.closeButton.TabIndex = 8;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// graphTabPage
			// 
			this.graphTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.graphPanel,
																					   this.panel3});
			this.graphTabPage.Location = new System.Drawing.Point(4, 22);
			this.graphTabPage.Name = "graphTabPage";
			this.graphTabPage.Size = new System.Drawing.Size(751, 390);
			this.graphTabPage.TabIndex = 2;
			this.graphTabPage.Text = "Graph";
			// 
			// graphPanel
			// 
			this.graphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.graphPanel.Name = "graphPanel";
			this.graphPanel.Size = new System.Drawing.Size(751, 335);
			this.graphPanel.TabIndex = 0;
			// 
			// panel3
			// 
			this.panel3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.panel6,
																				 this.applySelectedButton,
																				 this.scopeSelectedGraphRadioButton,
																				 this.scopeViewGraphRadioButton,
																				 this.scopeWorldGraphRadioButton,
																				 this.browseGraphButton,
																				 this.zoomGraphButton,
																				 this.showSelectedButton,
																				 this.closeButton2});
			this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel3.Location = new System.Drawing.Point(0, 335);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(751, 55);
			this.panel3.TabIndex = 4;
			// 
			// panel6
			// 
			this.panel6.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label8,
																				 this.timeUtc2RadioButton,
																				 this.timeLocal2RadioButton});
			this.panel6.Location = new System.Drawing.Point(550, 0);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(120, 56);
			this.panel6.TabIndex = 21;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(5, 15);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(50, 22);
			this.label8.TabIndex = 20;
			this.label8.Text = "Time:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// timeUtc2RadioButton
			// 
			this.timeUtc2RadioButton.Location = new System.Drawing.Point(60, 5);
			this.timeUtc2RadioButton.Name = "timeUtc2RadioButton";
			this.timeUtc2RadioButton.Size = new System.Drawing.Size(62, 22);
			this.timeUtc2RadioButton.TabIndex = 18;
			this.timeUtc2RadioButton.Text = "UTC";
			this.timeUtc2RadioButton.CheckedChanged += new System.EventHandler(this.timeUtcRadioButton_CheckedChanged);
			// 
			// timeLocal2RadioButton
			// 
			this.timeLocal2RadioButton.Location = new System.Drawing.Point(60, 25);
			this.timeLocal2RadioButton.Name = "timeLocal2RadioButton";
			this.timeLocal2RadioButton.Size = new System.Drawing.Size(62, 23);
			this.timeLocal2RadioButton.TabIndex = 19;
			this.timeLocal2RadioButton.Text = "local";
			this.timeLocal2RadioButton.CheckedChanged += new System.EventHandler(this.timeLocalRadioButton_CheckedChanged);
			// 
			// applySelectedButton
			// 
			this.applySelectedButton.Location = new System.Drawing.Point(430, 5);
			this.applySelectedButton.Name = "applySelectedButton";
			this.applySelectedButton.Size = new System.Drawing.Size(79, 45);
			this.applySelectedButton.TabIndex = 17;
			this.applySelectedButton.Text = "Set Time Filter";
			this.applySelectedButton.Click += new System.EventHandler(this.applySelectedButton_Click);
			// 
			// scopeSelectedGraphRadioButton
			// 
			this.scopeSelectedGraphRadioButton.Location = new System.Drawing.Point(168, 36);
			this.scopeSelectedGraphRadioButton.Name = "scopeSelectedGraphRadioButton";
			this.scopeSelectedGraphRadioButton.Size = new System.Drawing.Size(157, 20);
			this.scopeSelectedGraphRadioButton.TabIndex = 15;
			this.scopeSelectedGraphRadioButton.Text = "selected / filtered";
			// 
			// scopeViewGraphRadioButton
			// 
			this.scopeViewGraphRadioButton.Location = new System.Drawing.Point(168, 18);
			this.scopeViewGraphRadioButton.Name = "scopeViewGraphRadioButton";
			this.scopeViewGraphRadioButton.Size = new System.Drawing.Size(157, 20);
			this.scopeViewGraphRadioButton.TabIndex = 14;
			this.scopeViewGraphRadioButton.Text = "on the visible map";
			this.scopeViewGraphRadioButton.CheckedChanged += new System.EventHandler(this.scopeViewGraphRadioButton_CheckedChanged);
			// 
			// scopeWorldGraphRadioButton
			// 
			this.scopeWorldGraphRadioButton.Location = new System.Drawing.Point(168, 0);
			this.scopeWorldGraphRadioButton.Name = "scopeWorldGraphRadioButton";
			this.scopeWorldGraphRadioButton.Size = new System.Drawing.Size(157, 20);
			this.scopeWorldGraphRadioButton.TabIndex = 13;
			this.scopeWorldGraphRadioButton.Text = "world wide";
			this.scopeWorldGraphRadioButton.CheckedChanged += new System.EventHandler(this.scopeWorldGraphRadioButton_CheckedChanged);
			// 
			// browseGraphButton
			// 
			this.browseGraphButton.Location = new System.Drawing.Point(85, 30);
			this.browseGraphButton.Name = "browseGraphButton";
			this.browseGraphButton.Size = new System.Drawing.Size(75, 22);
			this.browseGraphButton.TabIndex = 12;
			this.browseGraphButton.Text = "Browse";
			this.browseGraphButton.Click += new System.EventHandler(this.browseGraphButton_Click);
			// 
			// zoomGraphButton
			// 
			this.zoomGraphButton.Location = new System.Drawing.Point(85, 5);
			this.zoomGraphButton.Name = "zoomGraphButton";
			this.zoomGraphButton.Size = new System.Drawing.Size(75, 21);
			this.zoomGraphButton.TabIndex = 11;
			this.zoomGraphButton.Text = "Go There";
			this.zoomGraphButton.Click += new System.EventHandler(this.zoomGraphButton_Click);
			// 
			// showSelectedButton
			// 
			this.showSelectedButton.Location = new System.Drawing.Point(336, 5);
			this.showSelectedButton.Name = "showSelectedButton";
			this.showSelectedButton.Size = new System.Drawing.Size(84, 45);
			this.showSelectedButton.TabIndex = 10;
			this.showSelectedButton.Text = "Show Selected";
			this.showSelectedButton.Click += new System.EventHandler(this.showSelectedButton_Click);
			// 
			// closeButton2
			// 
			this.closeButton2.Location = new System.Drawing.Point(675, 15);
			this.closeButton2.Name = "closeButton2";
			this.closeButton2.Size = new System.Drawing.Size(69, 22);
			this.closeButton2.TabIndex = 16;
			this.closeButton2.Text = "Close";
			this.closeButton2.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// filterTabPage
			// 
			this.filterTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.panel4,
																						this.panel1});
			this.filterTabPage.Location = new System.Drawing.Point(4, 22);
			this.filterTabPage.Name = "filterTabPage";
			this.filterTabPage.Size = new System.Drawing.Size(751, 390);
			this.filterTabPage.TabIndex = 1;
			this.filterTabPage.Text = "Filter by Time and Magnitude";
			// 
			// panel4
			// 
			this.panel4.AutoScroll = true;
			this.panel4.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label11,
																				 this.label10,
																				 this.utcFilterLabel,
																				 this.firstInViewButton,
																				 this.magToNumericUpDown,
																				 this.filterMessageLabel,
																				 this.noFilterRadioButton,
																				 this.aroundTimeDateTimePicker,
																				 this.toTimeDateTimePicker,
																				 this.fromDateTimePicker,
																				 this.magFilterCheckBox,
																				 this.aroundTimeRadioButton,
																				 this.fromTimeDateTimePicker,
																				 this.label4,
																				 this.playButton,
																				 this.aroundTimeComboBox,
																				 this.stopButton,
																				 this.aroundTimeTimeDateTimePicker,
																				 this.toDateTimePicker,
																				 this.label1,
																				 this.magFromNumericUpDown,
																				 this.label3,
																				 this.fromToRadioButton,
																				 this.label2});
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(751, 329);
			this.panel4.TabIndex = 42;
			// 
			// label11
			// 
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label11.Location = new System.Drawing.Point(445, 145);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(15, 21);
			this.label11.TabIndex = 44;
			this.label11.Text = "+-";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(740, 32);
			this.label10.TabIndex = 43;
			this.label10.Text = "Time/Magnitude filter instantly applies to earthquakes on the visible map. See no" +
				"te below.";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// utcFilterLabel
			// 
			this.utcFilterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.utcFilterLabel.Location = new System.Drawing.Point(460, 50);
			this.utcFilterLabel.Name = "utcFilterLabel";
			this.utcFilterLabel.Size = new System.Drawing.Size(280, 56);
			this.utcFilterLabel.TabIndex = 42;
			this.utcFilterLabel.Text = "utc?";
			this.utcFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// firstInViewButton
			// 
			this.firstInViewButton.Location = new System.Drawing.Point(310, 115);
			this.firstInViewButton.Name = "firstInViewButton";
			this.firstInViewButton.Size = new System.Drawing.Size(161, 22);
			this.firstInViewButton.TabIndex = 26;
			this.firstInViewButton.Text = "Set  to First  In View";
			this.firstInViewButton.Click += new System.EventHandler(this.firstInViewButton_Click);
			// 
			// magToNumericUpDown
			// 
			this.magToNumericUpDown.DecimalPlaces = 1;
			this.magToNumericUpDown.Increment = new System.Decimal(new int[] {
																				 1,
																				 0,
																				 0,
																				 65536});
			this.magToNumericUpDown.Location = new System.Drawing.Point(315, 205);
			this.magToNumericUpDown.Maximum = new System.Decimal(new int[] {
																			   10,
																			   0,
																			   0,
																			   0});
			this.magToNumericUpDown.Name = "magToNumericUpDown";
			this.magToNumericUpDown.Size = new System.Drawing.Size(52, 20);
			this.magToNumericUpDown.TabIndex = 35;
			this.magToNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.magToNumericUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.magToNumericUpDown_Validating);
			this.magToNumericUpDown.ValueChanged += new System.EventHandler(this.magToNumericUpDown_ValueChanged);
			// 
			// filterMessageLabel
			// 
			this.filterMessageLabel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.filterMessageLabel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.filterMessageLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.filterMessageLabel.Location = new System.Drawing.Point(15, 235);
			this.filterMessageLabel.Name = "filterMessageLabel";
			this.filterMessageLabel.Size = new System.Drawing.Size(725, 88);
			this.filterMessageLabel.TabIndex = 35;
			// 
			// noFilterRadioButton
			// 
			this.noFilterRadioButton.Location = new System.Drawing.Point(15, 175);
			this.noFilterRadioButton.Name = "noFilterRadioButton";
			this.noFilterRadioButton.Size = new System.Drawing.Size(577, 23);
			this.noFilterRadioButton.TabIndex = 32;
			this.noFilterRadioButton.Text = "Show all earthquakes - do not apply time filter";
			this.noFilterRadioButton.CheckedChanged += new System.EventHandler(this.noFilterRadioButton_CheckedChanged);
			// 
			// aroundTimeDateTimePicker
			// 
			this.aroundTimeDateTimePicker.Location = new System.Drawing.Point(110, 145);
			this.aroundTimeDateTimePicker.Name = "aroundTimeDateTimePicker";
			this.aroundTimeDateTimePicker.Size = new System.Drawing.Size(219, 20);
			this.aroundTimeDateTimePicker.TabIndex = 29;
			this.aroundTimeDateTimePicker.ValueChanged += new System.EventHandler(this.aroundTimeDateTimePicker_ValueChanged);
			// 
			// toTimeDateTimePicker
			// 
			this.toTimeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.toTimeDateTimePicker.Location = new System.Drawing.Point(350, 85);
			this.toTimeDateTimePicker.Name = "toTimeDateTimePicker";
			this.toTimeDateTimePicker.ShowUpDown = true;
			this.toTimeDateTimePicker.Size = new System.Drawing.Size(88, 20);
			this.toTimeDateTimePicker.TabIndex = 24;
			this.toTimeDateTimePicker.ValueChanged += new System.EventHandler(this.toTimeDateTimePicker_ValueChanged);
			// 
			// fromDateTimePicker
			// 
			this.fromDateTimePicker.Location = new System.Drawing.Point(110, 60);
			this.fromDateTimePicker.Name = "fromDateTimePicker";
			this.fromDateTimePicker.Size = new System.Drawing.Size(219, 20);
			this.fromDateTimePicker.TabIndex = 21;
			this.fromDateTimePicker.ValueChanged += new System.EventHandler(this.fromDateTimePicker_ValueChanged);
			// 
			// magFilterCheckBox
			// 
			this.magFilterCheckBox.Location = new System.Drawing.Point(15, 205);
			this.magFilterCheckBox.Name = "magFilterCheckBox";
			this.magFilterCheckBox.Size = new System.Drawing.Size(205, 22);
			this.magFilterCheckBox.TabIndex = 33;
			this.magFilterCheckBox.Text = "also, select Magnitude from:";
			this.magFilterCheckBox.CheckedChanged += new System.EventHandler(this.magFilterCheckBox_CheckedChanged);
			// 
			// aroundTimeRadioButton
			// 
			this.aroundTimeRadioButton.Location = new System.Drawing.Point(15, 115);
			this.aroundTimeRadioButton.Name = "aroundTimeRadioButton";
			this.aroundTimeRadioButton.Size = new System.Drawing.Size(270, 23);
			this.aroundTimeRadioButton.TabIndex = 25;
			this.aroundTimeRadioButton.Text = "Show only earthquakes dated:";
			this.aroundTimeRadioButton.CheckedChanged += new System.EventHandler(this.aroundTimeRadioButton_CheckedChanged);
			// 
			// fromTimeDateTimePicker
			// 
			this.fromTimeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.fromTimeDateTimePicker.Location = new System.Drawing.Point(350, 60);
			this.fromTimeDateTimePicker.Name = "fromTimeDateTimePicker";
			this.fromTimeDateTimePicker.ShowUpDown = true;
			this.fromTimeDateTimePicker.Size = new System.Drawing.Size(88, 20);
			this.fromTimeDateTimePicker.TabIndex = 22;
			this.fromTimeDateTimePicker.ValueChanged += new System.EventHandler(this.fromTimeDateTimePicker_ValueChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(30, 145);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(58, 21);
			this.label4.TabIndex = 30;
			this.label4.Text = "Around:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// playButton
			// 
			this.playButton.Location = new System.Drawing.Point(520, 115);
			this.playButton.Name = "playButton";
			this.playButton.Size = new System.Drawing.Size(29, 22);
			this.playButton.TabIndex = 27;
			this.playButton.Text = ">";
			this.playButton.Visible = false;
			this.playButton.Click += new System.EventHandler(this.playButton_Click);
			// 
			// aroundTimeComboBox
			// 
			this.aroundTimeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.aroundTimeComboBox.Items.AddRange(new object[] {
																	"1  min",
																	"2  min",
																	"5  min",
																	"10 min",
																	"15 min",
																	"20 min",
																	"30 min",
																	"45 min",
																	"1 hour",
																	"2 hours",
																	"4 hours",
																	"8 hours",
																	"12 hours",
																	"18 hours",
																	"1 day",
																	"2 days",
																	"3 days",
																	"5 days",
																	"10 days"});
			this.aroundTimeComboBox.Location = new System.Drawing.Point(465, 145);
			this.aroundTimeComboBox.Name = "aroundTimeComboBox";
			this.aroundTimeComboBox.Size = new System.Drawing.Size(102, 21);
			this.aroundTimeComboBox.TabIndex = 31;
			this.aroundTimeComboBox.SelectedIndexChanged += new System.EventHandler(this.aroundTimeComboBox_SelectedIndexChanged);
			// 
			// stopButton
			// 
			this.stopButton.Location = new System.Drawing.Point(565, 115);
			this.stopButton.Name = "stopButton";
			this.stopButton.Size = new System.Drawing.Size(29, 22);
			this.stopButton.TabIndex = 28;
			this.stopButton.Text = "[]";
			this.stopButton.Visible = false;
			this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
			// 
			// aroundTimeTimeDateTimePicker
			// 
			this.aroundTimeTimeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.aroundTimeTimeDateTimePicker.Location = new System.Drawing.Point(350, 145);
			this.aroundTimeTimeDateTimePicker.Name = "aroundTimeTimeDateTimePicker";
			this.aroundTimeTimeDateTimePicker.ShowUpDown = true;
			this.aroundTimeTimeDateTimePicker.Size = new System.Drawing.Size(88, 20);
			this.aroundTimeTimeDateTimePicker.TabIndex = 30;
			this.aroundTimeTimeDateTimePicker.ValueChanged += new System.EventHandler(this.aroundTimeTimeDateTimePicker_ValueChanged);
			// 
			// toDateTimePicker
			// 
			this.toDateTimePicker.Location = new System.Drawing.Point(110, 85);
			this.toDateTimePicker.Name = "toDateTimePicker";
			this.toDateTimePicker.Size = new System.Drawing.Size(219, 20);
			this.toDateTimePicker.TabIndex = 23;
			this.toDateTimePicker.ValueChanged += new System.EventHandler(this.toDateTimePicker_ValueChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(275, 205);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 22);
			this.label1.TabIndex = 41;
			this.label1.Text = "to:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// magFromNumericUpDown
			// 
			this.magFromNumericUpDown.DecimalPlaces = 1;
			this.magFromNumericUpDown.Increment = new System.Decimal(new int[] {
																				   1,
																				   0,
																				   0,
																				   65536});
			this.magFromNumericUpDown.Location = new System.Drawing.Point(220, 205);
			this.magFromNumericUpDown.Maximum = new System.Decimal(new int[] {
																				 10,
																				 0,
																				 0,
																				 0});
			this.magFromNumericUpDown.Name = "magFromNumericUpDown";
			this.magFromNumericUpDown.Size = new System.Drawing.Size(51, 20);
			this.magFromNumericUpDown.TabIndex = 34;
			this.magFromNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.magFromNumericUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.magFromNumericUpDown_Validating);
			this.magFromNumericUpDown.ValueChanged += new System.EventHandler(this.magFromNumericUpDown_ValueChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(30, 85);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(58, 22);
			this.label3.TabIndex = 23;
			this.label3.Text = "To:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// fromToRadioButton
			// 
			this.fromToRadioButton.Location = new System.Drawing.Point(15, 35);
			this.fromToRadioButton.Name = "fromToRadioButton";
			this.fromToRadioButton.Size = new System.Drawing.Size(358, 22);
			this.fromToRadioButton.TabIndex = 20;
			this.fromToRadioButton.Text = "Show only earthquakes dated:";
			this.fromToRadioButton.CheckedChanged += new System.EventHandler(this.fromToRadioButton_CheckedChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(30, 60);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 22);
			this.label2.TabIndex = 22;
			this.label2.Text = "From:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label9,
																				 this.timeLocal3RadioButton,
																				 this.timeUtc3RadioButton,
																				 this.applyFilterButton,
																				 this.closeButton1,
																				 this.label5,
																				 this.label6});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 329);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(751, 61);
			this.panel1.TabIndex = 5;
			// 
			// label9
			// 
			this.label9.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.label9.Location = new System.Drawing.Point(565, 20);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(40, 22);
			this.label9.TabIndex = 45;
			this.label9.Text = "Time:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// timeLocal3RadioButton
			// 
			this.timeLocal3RadioButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.timeLocal3RadioButton.Location = new System.Drawing.Point(610, 30);
			this.timeLocal3RadioButton.Name = "timeLocal3RadioButton";
			this.timeLocal3RadioButton.Size = new System.Drawing.Size(62, 23);
			this.timeLocal3RadioButton.TabIndex = 44;
			this.timeLocal3RadioButton.Text = "local";
			this.timeLocal3RadioButton.CheckedChanged += new System.EventHandler(this.timeLocalRadioButton_CheckedChanged);
			// 
			// timeUtc3RadioButton
			// 
			this.timeUtc3RadioButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.timeUtc3RadioButton.Location = new System.Drawing.Point(610, 10);
			this.timeUtc3RadioButton.Name = "timeUtc3RadioButton";
			this.timeUtc3RadioButton.Size = new System.Drawing.Size(62, 22);
			this.timeUtc3RadioButton.TabIndex = 43;
			this.timeUtc3RadioButton.Text = "UTC";
			this.timeUtc3RadioButton.CheckedChanged += new System.EventHandler(this.timeUtcRadioButton_CheckedChanged);
			// 
			// applyFilterButton
			// 
			this.applyFilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.applyFilterButton.Location = new System.Drawing.Point(15, 15);
			this.applyFilterButton.Name = "applyFilterButton";
			this.applyFilterButton.Size = new System.Drawing.Size(145, 35);
			this.applyFilterButton.TabIndex = 36;
			this.applyFilterButton.Text = "Apply to Table / Graph";
			this.applyFilterButton.Click += new System.EventHandler(this.applyFilterButton_Click);
			// 
			// closeButton1
			// 
			this.closeButton1.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton1.Location = new System.Drawing.Point(675, 20);
			this.closeButton1.Name = "closeButton1";
			this.closeButton1.Size = new System.Drawing.Size(69, 22);
			this.closeButton1.TabIndex = 37;
			this.closeButton1.Text = "Close";
			this.closeButton1.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label5.Location = new System.Drawing.Point(170, 8);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(395, 22);
			this.label5.TabIndex = 42;
			this.label5.Text = "Time/Magnitude filter instantly applies to earthquakes on the map.";
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label6.Location = new System.Drawing.Point(170, 30);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(395, 23);
			this.label6.TabIndex = 42;
			this.label6.Text = "<-- Click \"Apply...\" to filter the earthquakes in table and graph. ";
			// 
			// DlgEarthquakesManager
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(759, 416);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.MinimumSize = new System.Drawing.Size(650, 150);
			this.Name = "DlgEarthquakesManager";
			this.Text = "Earthquakes";
			this.Resize += new System.EventHandler(this.DlgEarthquakesManager_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgEarthquakesManager_Closing);
			this.Load += new System.EventHandler(this.DlgEarthquakesManager_Load);
			this.Activated += new System.EventHandler(this.DlgEarthquakesManager_Activated);
			this.tabControl1.ResumeLayout(false);
			this.eqTabPage.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.graphTabPage.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.filterTabPage.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.magToNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.magFromNumericUpDown)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void rebuildTableTab(bool useFilter)
		{
			isFilterAppliedToGraphAndTable = useFilter;
			Cursor.Current = Cursors.WaitCursor;
			makeEqDS(useFilter);
			makeEqDataGridStyle();
			//eqDataGrid.SetDataBinding(m_eqDS, "eq");
			DataView eqDV = m_eqDS.Tables["eq"].DefaultView;
			//eqDV.AllowDelete = false;
			eqDV.AllowEdit = false;
			eqDV.AllowNew = false;
			eqDataGrid.DataSource = eqDV;
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[eqDataGrid.DataSource, eqDataGrid.DataMember];
			((DataView)cm.List).AllowNew = false;
			//Cursor.Current = Cursors.Default;
			eqTabDirty = false;
			bool saveInSet = inSet;
			inSet = true;
			switch(m_scope)
			{
				case 0:		// world
					scopeWorldRadioButton.Checked = true;
					scopeWorldGraphRadioButton.Checked = true;
					break;
				default:
				case 1:		// visible on map
					scopeViewRadioButton.Checked = true;
					scopeViewGraphRadioButton.Checked = true;
					break;
				case 2:		// selected in graph or filtered in grid
					scopeFilteredRadioButton.Checked = true;
					scopeSelectedGraphRadioButton.Checked = true;
					break;
			}
			inSet = saveInSet;
		}

		public string descrById(long id)
		{
			string ret = "";
			Earthquake eq = EarthquakesCache.getEarthquakeById(id);
			if(eq != null)
			{
				string sTime;
				if(Project.useUtcTime)
				{
					DateTime time =  eq.DateTime;
					string format = "HH:mm:ss";
					sTime = time.ToLongDateString() + " " + time.ToString(format) + " UTC";
				}
				else
				{
					DateTime time = Project.zuluToLocal(eq.DateTime);
					sTime = time.ToLongDateString() + " " + time.ToLongTimeString();
				}
				ret = string.Format("{0:F1}", eq.Magn) + " -- " + sTime + " -- " + eq.Comment + " -- " + eq.Location;
				GeoCoord loc = new GeoCoord(eq.Location.X, eq.Location.Y, m_cameraManager.Elev);
				m_cameraManager.MarkLocation(loc, 0);
			}
			return ret;
		}

		public string zoomById(long id)
		{
			string ret = "";
			Earthquake eq = EarthquakesCache.getEarthquakeById(id);
			if(eq != null)
			{
				GeoCoord newLoc = new GeoCoord(eq.Location.X, eq.Location.Y, m_cameraManager.Elev);
				m_cameraManager.Location = newLoc;
				ret = "" + newLoc;
			}
			return ret;		// just info
		}

		public static string browseById(long id)
		{
			string ret = "";
			Earthquake eq = EarthquakesCache.getEarthquakeById(id);
			if(eq != null)
			{
				string url = eq.Url;
				if(url != null && url.StartsWith("http://"))
				{
					ret = url;
					Project.RunBrowser(url);
				}
			}
			return ret;		// just info
		}

		private static StringById dBrowseById = new StringById(browseById); 

		private void rebuildGraphTab()
		{
			// we need to have m_eqDS in place and current - we use it's data for the graph
			Cursor.Current = Cursors.WaitCursor;
			SortedList events = new SortedList();
			DataTable table = m_eqDS.Tables["eq"];
			foreach(DataRow row in table.Rows)
			{
				/*
				 * this is what the columns are:
				myDataRow["id"] = eq.Id;
				myDataRow["magn"] = eq.Magn;
				myDataRow["location"] = eq.Location.ToString();
				myDataRow["quality"] = eq.Quality;
				myDataRow["comment"] = eq.Comment;
				myDataRow["time"] = new SortableDateTime(eq.DateTime);
				myDataRow["displayed"] = eq.Enabled;
				myDataRow["source"] = eq.Source;
				myDataRow["url"] = eq.Url;
				*/

				DateTime time = ((SortableDateTime)row["time"]).DateTime;
				long id = (long)((int)row["id"]);
				double magn = (double)row["magn"];
				string url = "" + (string)row["url"];
				string properties = (url.Length == 0) ? "" : "w";

				long[] ids = new long[1];
				ids[0] = id;
				double[] values = new double[1];
				values[0] = magn;
				string[] sValues = new string[1];
				sValues[0] = properties;
				// depending on how DS was created, we may need to convert it to UTC. DatedEvent is always UTC: 
				DatedEvent ev = new DatedEvent(dsUseUtcTime ? time : Project.localToZulu(time), values, sValues, ids, 1);

				while(true)
				{
					try
					{
						events.Add(time, ev);
						break;
					}
					catch
					{
						time = time.AddTicks(1);
					}
				}
			}
			string selHint = "Click on \"Show Selected\" to see selected interval";
			StringById dZoomById = new StringById(zoomById); 
			StringById dDescrById = new StringById(descrById); 
			graphByTimeControl.init(this, "", "", selHint, dDescrById, dZoomById, dBrowseById, new MethodInvoker(showSelected));
			graphByTimeControl.setGraphData(events, Project.FORMAT_EARTHQUAKES_STRONGEST, 1, false);
			//Cursor.Current = Cursors.Default;
		}

		/*
		// call PutOnMap() when set of earthquakes changed, but the view point doesn't move.
		// this may happen if we directly enable/disable selected earthquakes, not using the filter.
		private void PutOnMap()
		{
			EarthquakesCache.RefreshEarthquakesDisplayed();
			LayerEarthquakes.This.init();		// provoke PutOnMap on layer's Paint()
			m_pictureManager.Refresh();
		}
		*/

		#region DataGrid and DataSet formation - for earthquakes

		private DataGridColumnStyle m_timeCol = null; 

		private void makeEqDataGridStyle()
		{
			if(m_eqTS != null)
			{
				return;
			}

			//STEP 1: Create a DataTable style object and set properties if required. 
			m_eqTS = new DataGridTableStyle(); 
			//specify the table from dataset (required step) 
			m_eqTS.MappingName = "eq";
			m_eqTS.RowHeadersVisible = false;
			// Set other properties (optional step) 
			//m_eqTS.AlternatingBackColor = Color.LightBlue; 

			int colCount = 0;

			//STEP 1: Create an int column style and add it to the tablestyle 
			//this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[m_eqDS, "eq"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 1; 
			csIDInt.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(csIDInt); 
			colCount++;

			//STEP 2: Create a double column and add it to the tablestyle 
			DataGridColumnStyle MagnCol = new DataGridDigitsTextBoxColumn(pdc["magn"], "F1", true); 
			MagnCol.MappingName = "magn"; //from dataset table 
			MagnCol.HeaderText = "Magn";
			MagnCol.Width = 40;
			MagnCol.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(MagnCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle LocCol = new DataGridTextBoxColumn(); 
			LocCol.MappingName = "location"; //from dataset table 
			LocCol.HeaderText = "Location"; 
			LocCol.Width = 200; 
			LocCol.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(LocCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle QCol = new DataGridTextBoxColumn(); 
			QCol.MappingName = "quality"; //from dataset table 
			QCol.HeaderText = "Quality"; 
			QCol.Width = 1; 
			QCol.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(QCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle ComCol = new DataGridTextBoxColumn(); 
			ComCol.MappingName = "comment"; //from dataset table 
			ComCol.HeaderText = "Comment"; 
			ComCol.Width = 270; 
			ComCol.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(ComCol); 
			colCount++;

			//STEP 5: Create a string column and add it to the tablestyle 
			m_timeCol = new DataGridTextBoxColumn(); 
			m_timeCol.MappingName = "time"; //from dataset table 
			m_timeCol.HeaderText = "Time  " + (Project.useUtcTime ? "[UTC]" : "[your computer]"); 
			m_timeCol.Width = 170; 
			m_timeCol.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(m_timeCol); 
			colCount++;

			//STEP 3: Add the checkbox 
			//DataGridColumnStyle boolCol = new MyDataGridBoolColumn(colCount); 
			DataGridColumnStyle boolCol = new DataGridTextBoxColumn(); 
			boolCol.MappingName = "displayed"; 
			boolCol.HeaderText = "Shown"; 
			//hook the new event to our handler in the grid
			//((MyDataGridBoolColumn)boolCol).BoolValueChanged += new BoolValueChangedEventHandler(HandleEarthquakeShowChanges);
			//uncomment this line to get a two-state checkbox 
			//((DataGridBoolColumn)boolCol).AllowNull = false;
			boolCol.Width = 60; 
			boolCol.ReadOnly = true;
			boolCol.Alignment = HorizontalAlignment.Center;
			m_eqTS.GridColumnStyles.Add(boolCol); 
			colCount++;

			//STEP 7: Create a string column and add it to the tablestyle 
			DataGridColumnStyle SourceCol = new DataGridTextBoxColumn(); 
			SourceCol.MappingName = "source"; //from dataset table 
			SourceCol.HeaderText = "Source"; 
			SourceCol.Width = 100; 
			SourceCol.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(SourceCol); 
			colCount++;

			//STEP 7: Create a string column and add it to the tablestyle 
			DataGridColumnStyle UrlCol = new DataGridTextBoxColumn();
			UrlCol.MappingName = "url"; //from dataset table
			UrlCol.HeaderText = "URL";
			UrlCol.Width = 500;
			UrlCol.ReadOnly = true;
			m_eqTS.GridColumnStyles.Add(UrlCol);
			colCount++;

			eqDataGrid.CaptionVisible = false;

			//STEP 8: Add the tablestyle to your datagrid's tablestlye collection:
			eqDataGrid.TableStyles.Clear();
			eqDataGrid.TableStyles.Add(m_eqTS);

			/* how to test for checked checkboxes:
			if((bool)eqDataGrid[row, column]) 
				MessageBox.Show("I am true"); 
			else 
				MessageBox.Show("I am false");
			*/
		}

		private bool dsUseUtcTime;

		private void makeEqDS(bool useFilter)
		{
			DataTable eqTable;

			if(m_eqDS == null)
			{
				eqTable = new DataTable("eq");

				DataColumn myDataColumn;
 
				// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.Int32");
				myDataColumn.ColumnName = "id";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = true;
				eqTable.Columns.Add(myDataColumn);
 
				// Create magn column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.Double");
				myDataColumn.ColumnName = "magn";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Magn";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Create location column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "location";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Location";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Create quality column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "quality";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Quality";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Create comment column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "comment";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Comment";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Create time column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = sortableDateTime.GetType();
				myDataColumn.ColumnName = "time";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Time";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Create displayed column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				//myDataColumn.DataType = System.Type.GetType("System.Boolean");
				myDataColumn.ColumnName = "displayed";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Shown";
				myDataColumn.ReadOnly = false;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Create source column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "source";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Source";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Create url column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "url";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "URL";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				eqTable.Columns.Add(myDataColumn);
 
				// Make the ID column the primary key column.
				DataColumn[] PrimaryKeyColumns = new DataColumn[1];
				PrimaryKeyColumns[0] = eqTable.Columns["id"];
				eqTable.PrimaryKey = PrimaryKeyColumns;
 
				// Add the new DataTable to the DataSet.
				m_eqDS = new DataSet();
				m_eqDS.Tables.Add(eqTable);
			}
 
			ArrayList src = null;
			switch(m_scope)
			{
				case 0:		// world
					src = EarthquakesCache.EarthquakesAll;
					break;
				default:
				case 1:		// visible on map
					src = EarthquakesCache.EarthquakesDisplayed;
					break;
				case 2:		// selected in graph or filtered in grid
					src = EarthquakesCache.EarthquakesDisplayed;
					break;
			}
			eqTable = m_eqDS.Tables[0];
			eqTable.Rows.Clear();
			// Create DataRow objects and add them to the DataTable
			dsUseUtcTime = Project.useUtcTime; // also memorize how the DS was created
			DataRow myDataRow;
			for (int i = 0; i < src.Count; i++)
			{
				Earthquake eq = (Earthquake)src[i];
				if(!useFilter || useFilter && TimeMagnitudeFilter.passesAll(eq.DateTime, eq.Magn))
				{
					myDataRow = eqTable.NewRow();

					myDataRow["id"] = eq.Id;
					myDataRow["magn"] = eq.Magn;
					myDataRow["location"] = eq.Location.ToString();
					myDataRow["quality"] = eq.Quality;
					myDataRow["comment"] = eq.Comment;
					myDataRow["time"] = new SortableDateTime(dsUseUtcTime ? eq.DateTime : Project.zuluToLocal(eq.DateTime));
					myDataRow["displayed"] = eq.Enabled ? "yes" : "no";
					myDataRow["source"] = eq.Source;
					myDataRow["url"] = eq.Url;

					try 
					{
						eqTable.Rows.Add(myDataRow);
					} 
					catch {}
				}
			}

			if(eqTable.DefaultView.Sort.Length == 0)
			{
				eqTable.DefaultView.Sort = "magn DESC";
			}

			messageLabel.Text = "" + src.Count + " earthquakes";
		}
		#endregion

		public ArrayList GetSelectedRows(DataGrid dataGrid) 
		{ 
			ArrayList al = new ArrayList(); 
 
			for(int i = 0; i < ((DataView)dataGrid.DataSource).Table.Rows.Count; ++i) 
			{ 
				if(dataGrid.IsSelected(i))
				{
					al.Add(i);
				}
			} 
			return al; 
		} 

		private void HandleEarthquakeShowChanges(object sender, BoolValueChangedEventArgs e)
		{
			/*
			string str = "Bool changed: row " + e.Row + "   value " + e.BoolValue;

			int column = 0;
			DataTable table = m_eqDS.Tables[0];
			int row = e.Row;
			long eqId = (long)((int)eqDataGrid[row, column]);
			Earthquake eq = EarthquakesCache.getEarthquakeById(eqId);
			str += "  " + row + "=" + eqId + " ";
			if(eq != null)
			{
				eq.Enabled = e.BoolValue;
				PutOnMap();
			}

			LibSys.StatusBar.Trace(str);
			messageLabel.Text = str;
			*/
		}

		private void DlgEarthquakesManager_Resize(object sender, System.EventArgs e)
		{
			Project.eqManagerWidth = this.Width;
			Project.eqManagerHeight = this.Height;
		}

		private void DlgEarthquakesManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			selectedTab = tabControl1.SelectedIndex;
			m_cameraManager.removeMarkLocation();
		}

		#region Scope radio buttons changes

		// set world or map scope:
		private void setScope(int scope)
		{
			m_scope = scope;
			rebuildTableTab(false);
			rebuildGraphTab();
			graphByTimeControl.Invalidate();
			graphByTimeControl.Focus();
		}

		private void scopeWorldRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.ClearPopup();
			bool saveInSet = inSet;
			inSet = true;
			scopeWorldGraphRadioButton.Checked = scopeWorldRadioButton.Checked;
			inSet = saveInSet;
			if(!inSet && scopeWorldRadioButton.Checked)
			{
				setScope(0);
			}
		}

		private void scopeWorldGraphRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.ClearPopup();
			bool saveInSet = inSet;
			inSet = true;
			scopeWorldRadioButton.Checked = scopeWorldGraphRadioButton.Checked;
			inSet = saveInSet;
			if(!inSet && scopeWorldGraphRadioButton.Checked)
			{
				setScope(0);
			}
		}

		private void scopeViewRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			bool saveInSet = inSet;
			inSet = true;
			scopeViewGraphRadioButton.Checked = scopeViewRadioButton.Checked;
			inSet = saveInSet;
			if(!inSet && scopeViewRadioButton.Checked)
			{
				setScope(1);
			}
		}

		private void scopeViewGraphRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			bool saveInSet = inSet;
			inSet = true;
			scopeViewRadioButton.Checked = scopeViewGraphRadioButton.Checked;
			inSet = saveInSet;
			if(!inSet && scopeViewGraphRadioButton.Checked)
			{
				setScope(1);
			}
		}

		private void scopeFilteredRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!inSet && scopeFilteredRadioButton.Checked)
			{
				m_scope = 2;
				rebuildTableTab(true);		// this happens on the table tab, Graph is not involved
			}
		}
		#endregion

		private void zoomButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(eqDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				int row = (int)selected[0];
				int column = 0;
				DataTable table = m_eqDS.Tables[0];
				long eqId = (long)((int)eqDataGrid[row, column]);
				Earthquake eq = EarthquakesCache.getEarthquakeById(eqId);
				if(eq != null)
				{
					GeoCoord newLoc = new GeoCoord(eq.Location.Lng, eq.Location.Lat, m_cameraManager.Location.Elev);
					messageLabel.Text = "" + eq;
					m_cameraManager.Location = newLoc;
				}
			}		
			else
			{
				Project.ShowPopup(zoomButton, "\nselect an earthquake to zoom to." + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(eqDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				int row = (int)selected[0];
				int column = 0;
				DataTable table = m_eqDS.Tables[0];
				long eqId = (long)((int)eqDataGrid[row, column]);
				Earthquake eq = EarthquakesCache.getEarthquakeById(eqId);
				if(eq != null && eq.Url != null && eq.Url.StartsWith("http://"))
				{
					messageLabel.Text = eq.Url;
					Project.RunBrowser(eq.Url);
				}
			}		
			else
			{
				Project.ShowPopup(browseButton, "\nselect an earthquake to browse." + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void ClearMessages()
		{
			messageLabel.Text = "";
			filterMessageLabel.Text = "";
		}

		private void collectTimeFilterValues()
		{
			if(!inSet)	// do nothing if this was caused by programmatically setting the values
			{
				string message = "Time filter:";
				TimeMagnitudeFilter.fromMagn = (double)magFromNumericUpDown.Value;
				TimeMagnitudeFilter.toMagn = (double)magToNumericUpDown.Value;

				switch(m_timeFilterMode)
				{
					case 0:		// from/to
					{
						DateTime tmp = fromDateTimePicker.Value.Date + fromTimeDateTimePicker.Value.TimeOfDay;
						TimeMagnitudeFilter.fromTime = Project.useUtcTime ? tmp : Project.localToZulu(tmp);
						tmp = toDateTimePicker.Value.Date + toTimeDateTimePicker.Value.TimeOfDay;
						TimeMagnitudeFilter.toTime = Project.useUtcTime ? tmp : Project.localToZulu(tmp);
						DateTime fromDT = Project.useUtcTime ? TimeMagnitudeFilter.fromTime : Project.zuluToLocal(TimeMagnitudeFilter.fromTime);
						DateTime toDT = Project.useUtcTime ? TimeMagnitudeFilter.toTime : Project.zuluToLocal(TimeMagnitudeFilter.toTime);
						message += "\r\n    From: " + fromDT.ToLongDateString() + "  " + fromDT.ToLongTimeString() + "\r\n" + "        To: " + toDT.ToLongDateString() + "  " + toDT.ToLongTimeString();
					}
						break;
					case 1:		// around time
					{
						DateTime tmp = aroundTimeDateTimePicker.Value.Date + aroundTimeTimeDateTimePicker.Value.TimeOfDay;
						DateTime aroundTime = Project.useUtcTime ? tmp : Project.localToZulu(tmp);
						TimeSpan ts = new TimeSpan(0, (int)aroundTimeMinutes, 0);
						TimeMagnitudeFilter.fromTime = aroundTime - ts;
						TimeMagnitudeFilter.toTime = aroundTime + ts;
						DateTime fromDT = Project.useUtcTime ? TimeMagnitudeFilter.fromTime : Project.zuluToLocal(TimeMagnitudeFilter.fromTime);
						DateTime toDT = Project.useUtcTime ? TimeMagnitudeFilter.toTime : Project.zuluToLocal(TimeMagnitudeFilter.toTime);
						message += "\r\n    From: " + fromDT.ToLongDateString() + "  " + fromDT.ToLongTimeString() + "\r\n" + "        To: " + toDT.ToLongDateString() + "  " + toDT.ToLongTimeString();
					}
						break;
					default:
						message += " disabled";
						break;
				}
				if(TimeMagnitudeFilter.EnabledMagn)
				{
					message += "\r\n\r\nMagnitude filter: from " + TimeMagnitudeFilter.fromMagn + " to " + TimeMagnitudeFilter.toMagn;
				}
				else
				{
					message += "\r\n\r\nMagnitude filter: disabled";
				}
				filterMessageLabel.Text = message;
				if(!playing && !inSet)
				{
					Cursor.Current = Cursors.WaitCursor;
					LayerEarthquakes.This.init();	// provoke PutOnMap()
					m_pictureManager.Refresh();
				}
				eqTabDirty = true;
			}
			applyFilterButton.Enabled = TimeMagnitudeFilter.Enabled;
		}

		private void fromToRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(fromToRadioButton.Checked)
			{
				setFilterModeFromTo();
				m_timeFilterMode = 0;
				collectTimeFilterValues();
			}
		}

		private void aroundTimeRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(aroundTimeRadioButton.Checked)
			{
				setFilterModeAroundTime();
				m_timeFilterMode = 1;
				collectTimeFilterValues();
			}
		}

		private void noFilterRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(noFilterRadioButton.Checked)
			{
				setFilterModeNone();
				string message = "Time filter: disabled";
				m_timeFilterMode = -1;
				if(TimeMagnitudeFilter.EnabledMagn)
				{
					message += "\r\n\r\nMagnitude filter: from " + TimeMagnitudeFilter.fromMagn + " to " + TimeMagnitudeFilter.toMagn;
				}
				else
				{
					message += "\r\n\r\nMagnitude filter: disabled";
				}
				filterMessageLabel.Text = message;
				playing = false;
				playButton.Enabled = false;
				stopButton.Enabled = false;
				if(!inSet)
				{
					Cursor.Current = Cursors.WaitCursor;
					LayerEarthquakes.This.init();	// provoke PutOnMap()
					m_pictureManager.Refresh();
				}
			}
		}

		private void fromDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
			collectTimeFilterValues();
		}

		private void fromTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			fromDateTimePicker.Value = fromTimeDateTimePicker.Value;
			collectTimeFilterValues();
		}

		private void toDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			toTimeDateTimePicker.Value = toDateTimePicker.Value;
			collectTimeFilterValues();
		}

		private void toTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			toDateTimePicker.Value = toTimeDateTimePicker.Value;
			collectTimeFilterValues();
		}

		private void aroundTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			aroundTimeTimeDateTimePicker.Value = aroundTimeDateTimePicker.Value;
			collectTimeFilterValues();
		}

		private void aroundTimeTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			aroundTimeDateTimePicker.Value = aroundTimeTimeDateTimePicker.Value;
			collectTimeFilterValues();
		}

		private void aroundTimeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			aroundTimeMinutes = aroundTimes[aroundTimeComboBox.SelectedIndex];
			collectTimeFilterValues();
		}

		private void playButton_Click(object sender, System.EventArgs e)
		{
			ClearMessages();
			/*
			m_playThread = new Thread(new ThreadStart(Play));
			m_playThread.Start();
			*/
			stopButton.Enabled = true;
			playButton.Enabled = false;
		}

		private void stopButton_Click(object sender, System.EventArgs e)
		{
			playing = false;
			playButton.Enabled = true;
			stopButton.Enabled = false;
		}

		private void magFilterCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			magFromNumericUpDown.Enabled = magFilterCheckBox.Checked;
			magToNumericUpDown.Enabled = magFilterCheckBox.Checked;
			TimeMagnitudeFilter.EnabledMagn = magFilterCheckBox.Checked;
			collectTimeFilterValues();
		}

		private void magFromNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private void magToNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private void magFromNumericUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Object obj = magFromNumericUpDown.Value;
		}

		private void magToNumericUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Object obj = magToNumericUpDown.Value;
		}

		private bool eqTabDirty = false;

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ClearMessages();
			if(!inSet)
			{
				if(tabControl1.SelectedTab == eqTabPage)
				{
					if(eqTabDirty)
					{
						rebuildTableTab(false);
					}
					if(graphByTimeControl.SelectedId != -1)
					{
						selectRowByEarthquakeId(graphByTimeControl.SelectedId);
					}
				}
				else if(tabControl1.SelectedTab == filterTabPage)
				{
				}
				else if(tabControl1.SelectedTab == graphTabPage)
				{
					if(m_eqTS == null)
					{
						rebuildTableTab(false);
						tabControl1.SelectedIndex = 0;
					} 
					else
					{
						rebuildGraphTab();
						graphByTimeControl.Focus();
					}
				}
			}
		}

		private void showSelectedButton_Click(object sender, System.EventArgs e)
		{
			showSelected();
		}

		private void showSelected()
		{
			scopeSelectedGraphRadioButton.Checked = true;
			graphByTimeControl.GraphSelectedArea();
			graphByTimeControl.Focus();
		}

		private static bool hasWarned = false;

		private void applySelectedButton_Click(object sender, System.EventArgs e)
		{
			// set time filter according to selected on the graph time interval:
			setFilterModeFromTo();
			m_timeFilterMode = 0;

			inSet = true;
			fromDateTimePicker.Value = Project.useUtcTime ? graphByTimeControl.getStartSelectedDateTime() : Project.zuluToLocal(graphByTimeControl.getStartSelectedDateTime());
			fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
			toDateTimePicker.Value = Project.useUtcTime ? graphByTimeControl.getEndSelectedDateTime() : Project.zuluToLocal(graphByTimeControl.getEndSelectedDateTime());
			toTimeDateTimePicker.Value = toDateTimePicker.Value;

			applyFilterButton.Enabled = true;
			fromToRadioButton.Enabled = true;
			fromToRadioButton.Checked = true;
			scopeFilteredRadioButton.Checked = true;
			inSet = false;

			collectTimeFilterValues();

			tabControl1.SelectedIndex = 2;	// show filter values
			if(!hasWarned)
			{
				Project.MessageBox(this, "Time filter has been set to selected interval. Events on the map have been filtered.\n\nClick \"Apply to Table/Graph\" button if you want to apply the filter to table and graph");
				hasWarned = true;
			}
		}

		private void applyFilterButton_Click(object sender, System.EventArgs e)
		{
			m_scope = 2;
			rebuildTableTab(true);
			tabControl1.SelectedIndex = 0;
		}

		private void exportButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(eqDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				ArrayList waypoints = new ArrayList();		// array of Waypoint
				foreach(int row in selected) 
				{
					int column = 0;
					DataTable table = m_eqDS.Tables[0];
					long eqId = (long)((int)eqDataGrid[row, column]);
					int rowNum = row; //(int)eqDataGrid[row, column];
					Earthquake eq = EarthquakesCache.getEarthquakeById(eqId);
					if(eq != null)
					{
						Waypoint wpt = new Waypoint(eq);
						if(wpt != null)
						{
							//m_wptIdToSelect = wpt.Id;
							waypoints.Add(wpt);
						}
					}
				}
				FileExportForm fileExportForm = new FileExportForm(null, false, waypoints, true);
				fileExportForm.ShowDialog();
			}		
			else
			{
				Project.ShowPopup(exportButton, "\nselect earthquakes to export first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void toWaypointsButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(eqDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				foreach(int row in selected) 
				{
					int column = 0;
					DataTable table = m_eqDS.Tables[0];
					long eqId = (long)((int)eqDataGrid[row, column]);
					int rowNum = row;
					Earthquake eq = EarthquakesCache.getEarthquakeById(eqId);
					if(eq != null)
					{
						Waypoint wpt = new Waypoint(eq);
						if(wpt != null)
						{
							WaypointsCache.addWaypoint(wpt);
						}
					}
				}
				PictureManager.This.CameraManager.ProcessCameraMove();
			}		
			else
			{
				Project.ShowPopup(toWaypointsButton, "\nselect earthquakes to convert to waypoints" + Project.SELECT_HELP, Point.Empty);
			}
		}

		// moves camera to location of selected quake:
		private void zoomGraphButton_Click(object sender, System.EventArgs e)
		{
			string sLoc = graphByTimeControl.zoom();
			graphByTimeControl.Focus();
		}

		// brings up browser with detail:
		private void browseGraphButton_Click(object sender, System.EventArgs e)
		{
			string url = graphByTimeControl.browse();
			graphByTimeControl.Focus();
		}

		private void showHide(bool show)
		{
			ArrayList selected = GetSelectedRows(eqDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				foreach(int row in selected) 
				{
					int column = 0;
					DataTable table = m_eqDS.Tables[0];
					long eqId = (long)((int)eqDataGrid[row, column]);
					int rowNum = row; //(int)eqDataGrid[row, column];
					Earthquake eq = EarthquakesCache.getEarthquakeById(eqId);
					if(eq != null)
					{
						eq.Enabled = show;
						foreach(DataRow dataRow in table.Rows)
						{
							if((long)((int)dataRow[column]) == eqId)
							{
								dataRow["displayed"] = show ? "yes" : "no";
								break;
							}
						}
					}
				}
				m_pictureManager.Refresh();
			}		
			else
			{
				Project.ShowPopup(show ? showButton : hideButton, "\nselect earthquakes to " + (show ? "show." : "hide.") + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void hideButton_Click(object sender, System.EventArgs e)
		{
			showHide(false);
		}

		private void showButton_Click(object sender, System.EventArgs e)
		{
			showHide(true);
		}

		private void selectRowByEarthquakeId(long eqId)
		{
			for(int row = 0; row < ((DataView)eqDataGrid.DataSource).Table.Rows.Count; ++row) 
			{ 
				int column = 0;
				long curId = (long)((int)eqDataGrid[row, column]);
				if(curId == eqId)
				{
					eqDataGrid.Select(row);
					eqDataGrid.ScrollToRow(row);
				}
				else
				{
					eqDataGrid.UnSelect(row);
				}
			} 
		}

		private void eqDataGrid_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point pt = new Point(e.X, e.Y); 
			DataGrid.HitTestInfo hti = eqDataGrid.HitTest(pt); 
			if(hti.Type == DataGrid.HitTestType.Cell && !eqDataGrid.IsSelected(hti.Row)) 
			{ 
				//eqDataGrid.CurrentCell = new DataGridCell(hti.Row, hti.Column); 
				eqDataGrid.Select(hti.Row); 
				int column = 0;
				long eqId = (long)((int)eqDataGrid[hti.Row, column]);
				Earthquake eq = EarthquakesCache.getEarthquakeById(eqId);
				if(eq != null)
				{
					GeoCoord loc = new GeoCoord(eq.Location.X, eq.Location.Y, m_cameraManager.Elev);
					m_cameraManager.MarkLocation(loc, 0);
					graphByTimeControl.SelectedId = eqId;
				}
			}
		}

		private void firstInViewButton_Click(object sender, System.EventArgs e)
		{
			aroundTimeDateTimePicker.Value = Project.zuluToLocal(EarthquakesCache.getMinTimeDisplayed());
			aroundTimeTimeDateTimePicker.Value = aroundTimeDateTimePicker.Value;
		}

		private void changeTimeOffset(bool useUtcTime)
		{
			inSet = true;
			if(useUtcTime)
			{
				fromDateTimePicker.Value       = Project.localToZulu(fromDateTimePicker.Value);
				toDateTimePicker.Value         = Project.localToZulu(toDateTimePicker.Value);
				aroundTimeDateTimePicker.Value = Project.localToZulu(aroundTimeDateTimePicker.Value);
			}
			else
			{
				fromDateTimePicker.Value       = Project.zuluToLocal(fromDateTimePicker.Value);
				toDateTimePicker.Value         = Project.zuluToLocal(toDateTimePicker.Value);
				aroundTimeDateTimePicker.Value = Project.zuluToLocal(aroundTimeDateTimePicker.Value);
			}
			fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
			toTimeDateTimePicker.Value = toDateTimePicker.Value;
			aroundTimeTimeDateTimePicker.Value = aroundTimeDateTimePicker.Value;
			inSet = false;
		}

		private bool isFilterAppliedToGraphAndTable = false;

		private void timeUtcRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				bool isChecked = ((RadioButton)sender).Checked;
				timeUtcRadioButton.Checked  = isChecked;
				timeUtc2RadioButton.Checked = isChecked;
				timeUtc3RadioButton.Checked = isChecked;
				if(m_timeCol != null)
				{
					m_timeCol.HeaderText = "Time  [UTC]"; 
				}
				if(!Project.useUtcTime && isChecked)
				{
					graphByTimeControl.UseUtcTime = isChecked;		// also sets Project.useUtcTime
					changeTimeOffset(true);
					setUtcLabels();
					rebuildTableTab(isFilterAppliedToGraphAndTable);
					LayerEarthquakes.This.init();	// provoke PutOnMap()
					m_pictureManager.Refresh();
				}
			}
			if(tabControl1.SelectedTab == graphTabPage)
			{
				graphByTimeControl.Focus();
			}
		}

		private void timeLocalRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				bool isChecked = ((RadioButton)sender).Checked;
				timeLocalRadioButton.Checked  = isChecked;
				timeLocal2RadioButton.Checked = isChecked;
				timeLocal3RadioButton.Checked = isChecked;
				if(m_timeCol != null)
				{
					m_timeCol.HeaderText = "Time  [your computer]"; 
				}
				if(Project.useUtcTime && isChecked)
				{
					graphByTimeControl.UseUtcTime = !isChecked;		// also sets Project.useUtcTime
					changeTimeOffset(false);
					setUtcLabels();
					rebuildTableTab(isFilterAppliedToGraphAndTable);
					LayerEarthquakes.This.init();	// provoke PutOnMap()
					m_pictureManager.Refresh();
				}
			}
			if(tabControl1.SelectedTab == graphTabPage)
			{
				graphByTimeControl.Focus();
			}
		}
	}
}
