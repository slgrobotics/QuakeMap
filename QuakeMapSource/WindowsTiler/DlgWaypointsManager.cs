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

using System.Text;
using System.IO;

using LibSys;
using LibGui;
using LibGeo;
using LibGps;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgWaypointsManager.
	/// </summary>
	public sealed class DlgWaypointsManager : System.Windows.Forms.Form, IEnableDisable
	{
		private static DlgWaypointsManager This = null;	// helps to ensure that only one dialog stays up

		private static SortableDateTime sortableDateTime = new SortableDateTime();	// for GetType()

		private DataSet m_tracksDS = null;
		private DataGridTableStyle m_tracksTS = null;
		private DataSet m_routesDS = null;
		private DataGridTableStyle m_routesTS = null;
		private DataSet m_trackpointsDS = null;
		private DataGridTableStyle m_trackpointsTS = null;
		private DataSet m_waypointsDS = null;
		private DataGridTableStyle m_waypointsTS = null;

		// ensure persistence of some columns' widths:
		private DataGridColumnStyle SymbolCol; 
		private DataGridColumnStyle NameCol; 
		private DataGridColumnStyle UrlNameCol; 
		private DataGridColumnStyle DescCol; 
		private DataGridColumnStyle CommentCol; 

		private PictureManager m_pictureManager;
		private CameraManager m_cameraManager;
		private static int m_timeFilterMode = -1;	// no filter
		private static double aroundTimeMinutes = 1.0d;

		private static int selectedTabIndex = 0;
		private long m_trackIdToSelect = -1;
		private long m_routeIdToSelect = -1;
		private bool inResize = false;
		private static int wptManagerX = 200;
		private static int wptManagerY = 200;

		private LibGui.GraphByTimeControl graphByTimeControl;

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tracksTabPage;
		private System.Windows.Forms.TabPage trackpointsTabPage;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.DataGrid tracksDataGrid;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label messageTrkLabel;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button goStartButton;
		private System.Windows.Forms.Button goEndButton;
		private System.Windows.Forms.Button showButton;
		private System.Windows.Forms.Button hideButton;
		private System.Windows.Forms.Button ungroupButton;
		private System.Windows.Forms.Button groupButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button deleteTrkButton;
		private System.Windows.Forms.Label messageTrkptLabel;
		private System.Windows.Forms.Button closeButton1;
		private System.Windows.Forms.DataGrid trackpointsDataGrid;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Button gotoTrkButton;
		private System.Windows.Forms.TabPage optionsTabPage;
		private System.Windows.Forms.CheckBox showNumbersCheckBox;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button closeButton2;
		private System.Windows.Forms.Button zoomButton;
		private System.Windows.Forms.TabPage waypointsTabPage;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Button closeButton3;
		private System.Windows.Forms.Button gotoWptButton;
		private System.Windows.Forms.Button deleteWptButton;
		private System.Windows.Forms.DataGrid waypointsDataGrid;
		private System.Windows.Forms.Label messageWptLabel;
		private System.Windows.Forms.ComboBox breakTimeComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox showWaypointsCheckBox;
		private System.Windows.Forms.Button importButton;
		private System.Windows.Forms.TabPage filterTabPage;
		private System.Windows.Forms.Button closeButton4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DateTimePicker fromDateTimePicker;
		private System.Windows.Forms.DateTimePicker toDateTimePicker;
		private System.Windows.Forms.DateTimePicker fromTimeDateTimePicker;
		private System.Windows.Forms.DateTimePicker toTimeDateTimePicker;
		private System.Windows.Forms.RadioButton fromToRadioButton;
		private System.Windows.Forms.RadioButton aroundTimeRadioButton;
		private System.Windows.Forms.DateTimePicker aroundTimeTimeDateTimePicker;
		private System.Windows.Forms.DateTimePicker aroundTimeDateTimePicker;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox aroundTimeComboBox;
		private System.Windows.Forms.RadioButton noFilterRadioButton;
		private System.Windows.Forms.Label messageFilterLabel;
		private System.Windows.Forms.Button playButton;
		private System.Windows.Forms.Button stopButton;
		private System.Windows.Forms.Button rewindButton;
		private System.Windows.Forms.Button wptDloadToGpsButton;
		private System.Windows.Forms.Button wptUploadFromGpsButton;
		private System.Windows.Forms.Button tracksGpsButton;
		private System.Windows.Forms.Button timeFilterDefaultsButton;
		private System.Windows.Forms.ComboBox timeFilterDefaultsComboBox;
		private System.Windows.Forms.TrackBar playSpeedTrackBar;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button wptSaveButton;
		private System.Windows.Forms.CheckBox sanityCheckBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button detailButton;
		private System.Windows.Forms.Button import1Button;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button editWptButton;
		private System.Windows.Forms.Button editTptButton;
		private System.Windows.Forms.Button browseTptButton;
		private System.Windows.Forms.Button detailTptButton;
		private System.Windows.Forms.Button ungroupHereButton;
		private System.Windows.Forms.TabPage routesTabPage;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.DataGrid routesDataGrid;
		private System.Windows.Forms.Button fromGpsRteButton;
		private System.Windows.Forms.Button importRteButton;
		private System.Windows.Forms.Button zoomRteButton;
		private System.Windows.Forms.Button saveRteButton;
		private System.Windows.Forms.Button joinRteButton;
		private System.Windows.Forms.Button hideRteButton;
		private System.Windows.Forms.Button showRteButton;
		private System.Windows.Forms.Button goEndRteButton;
		private System.Windows.Forms.Button goStartRteButton;
		private System.Windows.Forms.Button deleteRteButton;
		private System.Windows.Forms.Label messageRteLabel;
		private System.Windows.Forms.Button closeRteButton;
		private System.Windows.Forms.Button toGpsRteButton;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.NumericUpDown startRouteNumericUpDown;
		private System.Windows.Forms.CheckBox colorElevTracksCheckBox;
		private System.Windows.Forms.CheckBox colorSpeedTracksCheckBox;
		private System.Windows.Forms.GroupBox colorCodeGroupBox;
		private System.Windows.Forms.GroupBox colorPaletteGroupBox;
		private System.Windows.Forms.ColorDialog paletteColorDialog;
		private System.Windows.Forms.Button cp1Button;
		private System.Windows.Forms.Button cp2Button;
		private System.Windows.Forms.Button cp3Button;
		private System.Windows.Forms.Button cp4Button;
		private System.Windows.Forms.Button cp5Button;
		private System.Windows.Forms.Button cp6Button;
		private System.Windows.Forms.Button cp7Button;
		private System.Windows.Forms.Button cp8Button;
		private System.Windows.Forms.NumericUpDown trackThicknessNumericUpDown;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.NumericUpDown routeThicknessNumericUpDown;
		private System.Windows.Forms.Label colorPaletteMessageLabel;
		private System.Windows.Forms.Button thicknessApplyButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox showNamesCheckBox;
		private System.Windows.Forms.Button printTptButton;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.ComboBox printFontComboBox;
		private System.Windows.Forms.LinkLabel printLinkLabel;
		private System.Windows.Forms.LinkLabel print2LinkLabel;
		private System.Windows.Forms.Button zoomOutButton;
		private System.Windows.Forms.Button zoomCloseButton;
		private System.Windows.Forms.Button zoom4OutButton;
		private System.Windows.Forms.Button zoomClose4Button;
		private System.Windows.Forms.Button zoomOut2Button;
		private System.Windows.Forms.Button zoomClose2Button;
		private System.Windows.Forms.Button zoomOut3Button;
		private System.Windows.Forms.Button zoomClose3Button;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.ComboBox waypointNameStyleComboBox;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Button reverseRteButton;
		private System.Windows.Forms.Button toRteButton;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.NumericUpDown pointsPerRouteNumericUpDown;
		private System.Windows.Forms.Button setTrkColorButton;
		private System.Windows.Forms.Button setRteColorButton;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.ComboBox printFontComboBox1;
		private System.Windows.Forms.Button printWptButton;
		private System.Windows.Forms.CheckBox wptPrintDistanceCheckBox;
		private System.Windows.Forms.TabPage graphTabPage;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Panel graphPanel;
		private System.Windows.Forms.Panel panel7;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.RadioButton timeUtcRadioButton;
		private System.Windows.Forms.RadioButton timeLocalRadioButton;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Button zoomOut5Button;
		private System.Windows.Forms.Button zoomClose5Button;
		private System.Windows.Forms.Button gotoTrk5Button;
		private System.Windows.Forms.Button PrintGraphButton;
		private System.Windows.Forms.Button showAllButton;
		private System.Windows.Forms.Button closeButton5;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.ComboBox trackNameStyleComboBox;
		private System.Windows.Forms.CheckBox showTrackpointsCheckBox;
		private System.Windows.Forms.Button showInGeButton;
		private System.Windows.Forms.Button showInGeRteButton;
		private System.Windows.Forms.Button showInGeWptButton;
		private System.Drawing.Printing.PrintDocument printDocumentRteTrks;
		private System.Drawing.Printing.PrintDocument printDocumentWpts;
		private System.Windows.Forms.TabPage duatsTabPage;
		private System.Windows.Forms.TextBox duatsTextBox;
		private System.Windows.Forms.Button duatsConvertButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgWaypointsManager(PictureManager pictureManager, CameraManager cameraManager, int mode, long trackIdToSelect)
		{
			LayerWaypoints.This.makeRouteMode = false;	// just in case route mode is still on

			if(This != null)
			{
				This.Dispose();
				//GC.Collect();
				//GC.WaitForPendingFinalizers();
			}
			This = this;

			m_pictureManager = pictureManager;
			m_cameraManager = cameraManager;
			m_trackIdToSelect = trackIdToSelect;
			m_routeIdToSelect = trackIdToSelect;

			InitializeComponent();

			this.SuspendLayout();
			if(Project.fitsScreen(wptManagerX, wptManagerY, Project.wptManagerWidth, Project.wptManagerHeight))
			{
				inResize = true;
				this.Location = new Point(wptManagerX, wptManagerY);
				this.ClientSize = new System.Drawing.Size(Project.wptManagerWidth, Project.wptManagerHeight);
				inResize = false;
			}

			this.fromTimeDateTimePicker.CustomFormat = "h:mm tt";
			this.toTimeDateTimePicker.CustomFormat = "h:mm tt";
			this.aroundTimeTimeDateTimePicker.CustomFormat = "h:mm tt";

			SetTimeFilterDefaults();

			showWaypointsCheckBox.Checked = Project.drawWaypoints;
			showTrackpointsCheckBox.Checked = Project.drawTrackpoints;
			showNumbersCheckBox.Checked = Project.showTrackpointNumbers;
			showNamesCheckBox.Checked = Project.showWaypointNames;
			sanityCheckBox.Checked = Project.sanityFilter;
			colorElevTracksCheckBox.Checked  = Project.trackElevColor;
			colorSpeedTracksCheckBox.Checked = Project.trackSpeedColor;

			// order of tabs: tracks-trackpoints-routes-waypoints-filter-options
			switch(mode)
			{
				default:
				case 0:
					// stay on the last chosen 
					rebuildTracksTab();
					rebuildRoutesTab();
					rebuildWaypointsTab();
					// never jump straight to trackpoints tab:
					tabControl1.SelectedIndex = (selectedTabIndex == 1 || selectedTabIndex == 2) ? 0 : selectedTabIndex;
					if(tabControl1.SelectedTab == tracksTabPage || tabControl1.SelectedTab == routesTabPage)
					{
						prevSelectedTab = tabControl1.SelectedTab;
					}
					break;
				case 1:
					// tracks tab, show the grid:
					rebuildTracksTab();
					rebuildRoutesTab();
					prevSelectedTab = this.tracksTabPage;
					break;
				case 2:
					// go to Options tab
					rebuildTracksTab();
					rebuildRoutesTab();
					prevSelectedTab = this.tracksTabPage;
					tabControl1.SelectedTab = this.optionsTabPage;
					break;
				case 3:
					// routes tab, show the grid:
					rebuildTracksTab();
					rebuildRoutesTab();
					tabControl1.SelectedTab = this.routesTabPage;
					prevSelectedTab = this.routesTabPage;
					break;
			}

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

			breakTimeComboBox.SelectedIndex = breakTimeIndex(Project.breakTimeMinutes);
			aroundTimeComboBox.SelectedIndex = aroundTimeIndex(aroundTimeMinutes);
			playSleepTimeMs = 500 + (playSpeedTrackBar.Maximum - playSpeedTrackBar.Value) * 200;
			Project.setDlgIcon(this);

			this.waypointsDataGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.waypointsDataGrid_MouseUp);
			this.trackpointsDataGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackpointsDataGrid_MouseUp);
			setColorPaletteButtons();
			colorPaletteMessageLabel.Text = "Note: track/route colors are picked\nrandomly from the choices above, unless explicitly set.";
			try
			{
				trackThicknessNumericUpDown.Value = (decimal)TrackPalette.penTrackThickness;
				routeThicknessNumericUpDown.Value = (decimal)TrackPalette.penRouteThickness;
				this.pointsPerRouteNumericUpDown.Value = (decimal)Project.gpsMaxPointsPerRoute;
			} 
			catch {}

			graphByTimeControl = new LibGui.GraphByTimeControl();
			this.graphPanel.Controls.Add(graphByTimeControl);
			this.graphByTimeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.graphByTimeControl.Name = "graphByTimeControl";

			inSet = true;

			timeUtcRadioButton.Checked  = Project.useUtcTime;
			timeLocalRadioButton.Checked  = !Project.useUtcTime;
			setUtcLabels();

			inSet = false;


			this.ResumeLayout(true);
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tracksTabPage = new System.Windows.Forms.TabPage();
			this.tracksDataGrid = new System.Windows.Forms.DataGrid();
			this.panel1 = new System.Windows.Forms.Panel();
			this.showInGeButton = new System.Windows.Forms.Button();
			this.setTrkColorButton = new System.Windows.Forms.Button();
			this.toRteButton = new System.Windows.Forms.Button();
			this.zoomOut2Button = new System.Windows.Forms.Button();
			this.zoomClose2Button = new System.Windows.Forms.Button();
			this.printLinkLabel = new System.Windows.Forms.LinkLabel();
			this.tracksGpsButton = new System.Windows.Forms.Button();
			this.importButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.breakTimeComboBox = new System.Windows.Forms.ComboBox();
			this.zoomButton = new System.Windows.Forms.Button();
			this.saveButton = new System.Windows.Forms.Button();
			this.ungroupButton = new System.Windows.Forms.Button();
			this.groupButton = new System.Windows.Forms.Button();
			this.hideButton = new System.Windows.Forms.Button();
			this.showButton = new System.Windows.Forms.Button();
			this.goEndButton = new System.Windows.Forms.Button();
			this.goStartButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.messageTrkLabel = new System.Windows.Forms.Label();
			this.closeButton = new System.Windows.Forms.Button();
			this.graphTabPage = new System.Windows.Forms.TabPage();
			this.graphPanel = new System.Windows.Forms.Panel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.showAllButton = new System.Windows.Forms.Button();
			this.messageLabel = new System.Windows.Forms.Label();
			this.panel7 = new System.Windows.Forms.Panel();
			this.label17 = new System.Windows.Forms.Label();
			this.timeUtcRadioButton = new System.Windows.Forms.RadioButton();
			this.timeLocalRadioButton = new System.Windows.Forms.RadioButton();
			this.zoomOut5Button = new System.Windows.Forms.Button();
			this.zoomClose5Button = new System.Windows.Forms.Button();
			this.PrintGraphButton = new System.Windows.Forms.Button();
			this.gotoTrk5Button = new System.Windows.Forms.Button();
			this.label18 = new System.Windows.Forms.Label();
			this.closeButton5 = new System.Windows.Forms.Button();
			this.trackpointsTabPage = new System.Windows.Forms.TabPage();
			this.trackpointsDataGrid = new System.Windows.Forms.DataGrid();
			this.panel2 = new System.Windows.Forms.Panel();
			this.zoomOutButton = new System.Windows.Forms.Button();
			this.zoomCloseButton = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.printFontComboBox = new System.Windows.Forms.ComboBox();
			this.printTptButton = new System.Windows.Forms.Button();
			this.ungroupHereButton = new System.Windows.Forms.Button();
			this.editTptButton = new System.Windows.Forms.Button();
			this.browseTptButton = new System.Windows.Forms.Button();
			this.detailTptButton = new System.Windows.Forms.Button();
			this.gotoTrkButton = new System.Windows.Forms.Button();
			this.deleteTrkButton = new System.Windows.Forms.Button();
			this.messageTrkptLabel = new System.Windows.Forms.Label();
			this.closeButton1 = new System.Windows.Forms.Button();
			this.routesTabPage = new System.Windows.Forms.TabPage();
			this.routesDataGrid = new System.Windows.Forms.DataGrid();
			this.panel5 = new System.Windows.Forms.Panel();
			this.showInGeRteButton = new System.Windows.Forms.Button();
			this.setRteColorButton = new System.Windows.Forms.Button();
			this.reverseRteButton = new System.Windows.Forms.Button();
			this.zoomOut3Button = new System.Windows.Forms.Button();
			this.zoomClose3Button = new System.Windows.Forms.Button();
			this.print2LinkLabel = new System.Windows.Forms.LinkLabel();
			this.label8 = new System.Windows.Forms.Label();
			this.startRouteNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.toGpsRteButton = new System.Windows.Forms.Button();
			this.fromGpsRteButton = new System.Windows.Forms.Button();
			this.importRteButton = new System.Windows.Forms.Button();
			this.zoomRteButton = new System.Windows.Forms.Button();
			this.saveRteButton = new System.Windows.Forms.Button();
			this.joinRteButton = new System.Windows.Forms.Button();
			this.hideRteButton = new System.Windows.Forms.Button();
			this.showRteButton = new System.Windows.Forms.Button();
			this.goEndRteButton = new System.Windows.Forms.Button();
			this.goStartRteButton = new System.Windows.Forms.Button();
			this.deleteRteButton = new System.Windows.Forms.Button();
			this.messageRteLabel = new System.Windows.Forms.Label();
			this.closeRteButton = new System.Windows.Forms.Button();
			this.waypointsTabPage = new System.Windows.Forms.TabPage();
			this.waypointsDataGrid = new System.Windows.Forms.DataGrid();
			this.panel4 = new System.Windows.Forms.Panel();
			this.showInGeWptButton = new System.Windows.Forms.Button();
			this.wptPrintDistanceCheckBox = new System.Windows.Forms.CheckBox();
			this.label16 = new System.Windows.Forms.Label();
			this.printFontComboBox1 = new System.Windows.Forms.ComboBox();
			this.printWptButton = new System.Windows.Forms.Button();
			this.zoom4OutButton = new System.Windows.Forms.Button();
			this.zoomClose4Button = new System.Windows.Forms.Button();
			this.editWptButton = new System.Windows.Forms.Button();
			this.browseButton = new System.Windows.Forms.Button();
			this.import1Button = new System.Windows.Forms.Button();
			this.detailButton = new System.Windows.Forms.Button();
			this.wptSaveButton = new System.Windows.Forms.Button();
			this.wptUploadFromGpsButton = new System.Windows.Forms.Button();
			this.wptDloadToGpsButton = new System.Windows.Forms.Button();
			this.gotoWptButton = new System.Windows.Forms.Button();
			this.deleteWptButton = new System.Windows.Forms.Button();
			this.messageWptLabel = new System.Windows.Forms.Label();
			this.closeButton3 = new System.Windows.Forms.Button();
			this.filterTabPage = new System.Windows.Forms.TabPage();
			this.label7 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.playSpeedTrackBar = new System.Windows.Forms.TrackBar();
			this.rewindButton = new System.Windows.Forms.Button();
			this.stopButton = new System.Windows.Forms.Button();
			this.playButton = new System.Windows.Forms.Button();
			this.messageFilterLabel = new System.Windows.Forms.Label();
			this.noFilterRadioButton = new System.Windows.Forms.RadioButton();
			this.aroundTimeComboBox = new System.Windows.Forms.ComboBox();
			this.aroundTimeTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.aroundTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.label4 = new System.Windows.Forms.Label();
			this.aroundTimeRadioButton = new System.Windows.Forms.RadioButton();
			this.fromToRadioButton = new System.Windows.Forms.RadioButton();
			this.toTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.fromTimeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.toDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.fromDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.timeFilterDefaultsButton = new System.Windows.Forms.Button();
			this.timeFilterDefaultsComboBox = new System.Windows.Forms.ComboBox();
			this.closeButton4 = new System.Windows.Forms.Button();
			this.duatsTabPage = new System.Windows.Forms.TabPage();
			this.duatsConvertButton = new System.Windows.Forms.Button();
			this.duatsTextBox = new System.Windows.Forms.TextBox();
			this.optionsTabPage = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.pointsPerRouteNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label14 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label6 = new System.Windows.Forms.Label();
			this.sanityCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label19 = new System.Windows.Forms.Label();
			this.trackNameStyleComboBox = new System.Windows.Forms.ComboBox();
			this.label12 = new System.Windows.Forms.Label();
			this.waypointNameStyleComboBox = new System.Windows.Forms.ComboBox();
			this.showNamesCheckBox = new System.Windows.Forms.CheckBox();
			this.showWaypointsCheckBox = new System.Windows.Forms.CheckBox();
			this.showNumbersCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.showTrackpointsCheckBox = new System.Windows.Forms.CheckBox();
			this.thicknessApplyButton = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.routeThicknessNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label9 = new System.Windows.Forms.Label();
			this.trackThicknessNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.colorPaletteGroupBox = new System.Windows.Forms.GroupBox();
			this.colorPaletteMessageLabel = new System.Windows.Forms.Label();
			this.cp1Button = new System.Windows.Forms.Button();
			this.cp2Button = new System.Windows.Forms.Button();
			this.cp3Button = new System.Windows.Forms.Button();
			this.cp4Button = new System.Windows.Forms.Button();
			this.cp5Button = new System.Windows.Forms.Button();
			this.cp6Button = new System.Windows.Forms.Button();
			this.cp7Button = new System.Windows.Forms.Button();
			this.cp8Button = new System.Windows.Forms.Button();
			this.colorCodeGroupBox = new System.Windows.Forms.GroupBox();
			this.colorSpeedTracksCheckBox = new System.Windows.Forms.CheckBox();
			this.colorElevTracksCheckBox = new System.Windows.Forms.CheckBox();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label15 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.closeButton2 = new System.Windows.Forms.Button();
			this.paletteColorDialog = new System.Windows.Forms.ColorDialog();
			this.printDocumentRteTrks = new System.Drawing.Printing.PrintDocument();
			this.printDocumentWpts = new System.Drawing.Printing.PrintDocument();
			this.tabControl1.SuspendLayout();
			this.tracksTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tracksDataGrid)).BeginInit();
			this.panel1.SuspendLayout();
			this.graphTabPage.SuspendLayout();
			this.panel6.SuspendLayout();
			this.panel7.SuspendLayout();
			this.trackpointsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackpointsDataGrid)).BeginInit();
			this.panel2.SuspendLayout();
			this.routesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.routesDataGrid)).BeginInit();
			this.panel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.startRouteNumericUpDown)).BeginInit();
			this.waypointsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.waypointsDataGrid)).BeginInit();
			this.panel4.SuspendLayout();
			this.filterTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.playSpeedTrackBar)).BeginInit();
			this.duatsTabPage.SuspendLayout();
			this.optionsTabPage.SuspendLayout();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pointsPerRouteNumericUpDown)).BeginInit();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.routeThicknessNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackThicknessNumericUpDown)).BeginInit();
			this.colorPaletteGroupBox.SuspendLayout();
			this.colorCodeGroupBox.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.tracksTabPage,
																					  this.graphTabPage,
																					  this.trackpointsTabPage,
																					  this.routesTabPage,
																					  this.waypointsTabPage,
																					  this.filterTabPage,
																					  this.duatsTabPage,
																					  this.optionsTabPage});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(957, 530);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tracksTabPage
			// 
			this.tracksTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.tracksDataGrid,
																						this.panel1});
			this.tracksTabPage.Location = new System.Drawing.Point(4, 22);
			this.tracksTabPage.Name = "tracksTabPage";
			this.tracksTabPage.Size = new System.Drawing.Size(949, 504);
			this.tracksTabPage.TabIndex = 0;
			this.tracksTabPage.Text = "Track Logs";
			// 
			// tracksDataGrid
			// 
			this.tracksDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.tracksDataGrid.DataMember = "";
			this.tracksDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tracksDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tracksDataGrid.Name = "tracksDataGrid";
			this.tracksDataGrid.Size = new System.Drawing.Size(949, 424);
			this.tracksDataGrid.TabIndex = 1;
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.showInGeButton,
																				 this.setTrkColorButton,
																				 this.toRteButton,
																				 this.zoomOut2Button,
																				 this.zoomClose2Button,
																				 this.printLinkLabel,
																				 this.tracksGpsButton,
																				 this.importButton,
																				 this.label1,
																				 this.breakTimeComboBox,
																				 this.zoomButton,
																				 this.saveButton,
																				 this.ungroupButton,
																				 this.groupButton,
																				 this.hideButton,
																				 this.showButton,
																				 this.goEndButton,
																				 this.goStartButton,
																				 this.deleteButton,
																				 this.messageTrkLabel,
																				 this.closeButton});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 424);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(949, 80);
			this.panel1.TabIndex = 0;
			// 
			// showInGeButton
			// 
			this.showInGeButton.Location = new System.Drawing.Point(680, 30);
			this.showInGeButton.Name = "showInGeButton";
			this.showInGeButton.Size = new System.Drawing.Size(160, 25);
			this.showInGeButton.TabIndex = 128;
			this.showInGeButton.Text = "Show in Google Earth";
			this.showInGeButton.Click += new System.EventHandler(this.showInGeButton_Click);
			// 
			// setTrkColorButton
			// 
			this.setTrkColorButton.Location = new System.Drawing.Point(275, 5);
			this.setTrkColorButton.Name = "setTrkColorButton";
			this.setTrkColorButton.Size = new System.Drawing.Size(75, 25);
			this.setTrkColorButton.TabIndex = 127;
			this.setTrkColorButton.Text = "Set Color";
			this.setTrkColorButton.Click += new System.EventHandler(this.setTrkColorButton_Click);
			// 
			// toRteButton
			// 
			this.toRteButton.Location = new System.Drawing.Point(275, 30);
			this.toRteButton.Name = "toRteButton";
			this.toRteButton.Size = new System.Drawing.Size(75, 25);
			this.toRteButton.TabIndex = 126;
			this.toRteButton.Text = "To Route";
			this.toRteButton.Click += new System.EventHandler(this.toRteButton_Click);
			// 
			// zoomOut2Button
			// 
			this.zoomOut2Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomOut2Button.Location = new System.Drawing.Point(115, 30);
			this.zoomOut2Button.Name = "zoomOut2Button";
			this.zoomOut2Button.Size = new System.Drawing.Size(23, 24);
			this.zoomOut2Button.TabIndex = 125;
			this.zoomOut2Button.Text = "^";
			this.zoomOut2Button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zoomOut2Button.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomClose2Button
			// 
			this.zoomClose2Button.Location = new System.Drawing.Point(10, 30);
			this.zoomClose2Button.Name = "zoomClose2Button";
			this.zoomClose2Button.Size = new System.Drawing.Size(103, 24);
			this.zoomClose2Button.TabIndex = 124;
			this.zoomClose2Button.Text = "1m/pix";
			this.zoomClose2Button.Click += new System.EventHandler(this.zoomCloseButton_Click);
			// 
			// printLinkLabel
			// 
			this.printLinkLabel.Location = new System.Drawing.Point(395, 5);
			this.printLinkLabel.Name = "printLinkLabel";
			this.printLinkLabel.Size = new System.Drawing.Size(47, 19);
			this.printLinkLabel.TabIndex = 14;
			this.printLinkLabel.TabStop = true;
			this.printLinkLabel.Text = "print";
			this.printLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.printLinkLabel_LinkClicked);
			// 
			// tracksGpsButton
			// 
			this.tracksGpsButton.Location = new System.Drawing.Point(585, 30);
			this.tracksGpsButton.Name = "tracksGpsButton";
			this.tracksGpsButton.Size = new System.Drawing.Size(86, 25);
			this.tracksGpsButton.TabIndex = 10;
			this.tracksGpsButton.Text = "From GPS";
			this.tracksGpsButton.Click += new System.EventHandler(this.tracksGpsButton_Click);
			// 
			// importButton
			// 
			this.importButton.Location = new System.Drawing.Point(760, 5);
			this.importButton.Name = "importButton";
			this.importButton.Size = new System.Drawing.Size(80, 25);
			this.importButton.TabIndex = 12;
			this.importButton.Text = "Import";
			this.importButton.Click += new System.EventHandler(this.importButton_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(360, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 22);
			this.label1.TabIndex = 12;
			this.label1.Text = "Break Time:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// breakTimeComboBox
			// 
			this.breakTimeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.breakTimeComboBox.Items.AddRange(new object[] {
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
																   "1 day"});
			this.breakTimeComboBox.Location = new System.Drawing.Point(455, 32);
			this.breakTimeComboBox.Name = "breakTimeComboBox";
			this.breakTimeComboBox.Size = new System.Drawing.Size(119, 21);
			this.breakTimeComboBox.TabIndex = 9;
			this.breakTimeComboBox.SelectedIndexChanged += new System.EventHandler(this.breakTimeComboBox_SelectedIndexChanged);
			// 
			// zoomButton
			// 
			this.zoomButton.Location = new System.Drawing.Point(200, 5);
			this.zoomButton.Name = "zoomButton";
			this.zoomButton.Size = new System.Drawing.Size(60, 25);
			this.zoomButton.TabIndex = 5;
			this.zoomButton.Text = "Zoom";
			this.zoomButton.Click += new System.EventHandler(this.zoomButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(680, 5);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(80, 25);
			this.saveButton.TabIndex = 11;
			this.saveButton.Text = "Save";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// ungroupButton
			// 
			this.ungroupButton.Location = new System.Drawing.Point(455, 5);
			this.ungroupButton.Name = "ungroupButton";
			this.ungroupButton.Size = new System.Drawing.Size(119, 25);
			this.ungroupButton.TabIndex = 8;
			this.ungroupButton.Text = "Break into Trips";
			this.ungroupButton.Click += new System.EventHandler(this.ungroupButton_Click);
			// 
			// groupButton
			// 
			this.groupButton.Location = new System.Drawing.Point(585, 5);
			this.groupButton.Name = "groupButton";
			this.groupButton.Size = new System.Drawing.Size(86, 25);
			this.groupButton.TabIndex = 7;
			this.groupButton.Text = "Join Tracks";
			this.groupButton.Click += new System.EventHandler(this.groupButton_Click);
			// 
			// hideButton
			// 
			this.hideButton.Location = new System.Drawing.Point(150, 30);
			this.hideButton.Name = "hideButton";
			this.hideButton.Size = new System.Drawing.Size(51, 25);
			this.hideButton.TabIndex = 4;
			this.hideButton.Text = "Hide";
			this.hideButton.Click += new System.EventHandler(this.hideButton_Click);
			// 
			// showButton
			// 
			this.showButton.Location = new System.Drawing.Point(150, 5);
			this.showButton.Name = "showButton";
			this.showButton.Size = new System.Drawing.Size(51, 25);
			this.showButton.TabIndex = 3;
			this.showButton.Text = "Show";
			this.showButton.Click += new System.EventHandler(this.showButton_Click);
			// 
			// goEndButton
			// 
			this.goEndButton.Location = new System.Drawing.Point(100, 5);
			this.goEndButton.Name = "goEndButton";
			this.goEndButton.Size = new System.Drawing.Size(38, 25);
			this.goEndButton.TabIndex = 2;
			this.goEndButton.Text = "End";
			this.goEndButton.Click += new System.EventHandler(this.goEndButton_Click);
			// 
			// goStartButton
			// 
			this.goStartButton.Location = new System.Drawing.Point(10, 5);
			this.goStartButton.Name = "goStartButton";
			this.goStartButton.Size = new System.Drawing.Size(86, 25);
			this.goStartButton.TabIndex = 1;
			this.goStartButton.Text = "Go to Start";
			this.goStartButton.Click += new System.EventHandler(this.goStartButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Location = new System.Drawing.Point(200, 30);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(60, 25);
			this.deleteButton.TabIndex = 6;
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// messageTrkLabel
			// 
			this.messageTrkLabel.Location = new System.Drawing.Point(15, 60);
			this.messageTrkLabel.Name = "messageTrkLabel";
			this.messageTrkLabel.Size = new System.Drawing.Size(1708, 18);
			this.messageTrkLabel.TabIndex = 1;
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(860, 15);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(81, 26);
			this.closeButton.TabIndex = 13;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// graphTabPage
			// 
			this.graphTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.graphPanel,
																					   this.panel6});
			this.graphTabPage.Location = new System.Drawing.Point(4, 22);
			this.graphTabPage.Name = "graphTabPage";
			this.graphTabPage.Size = new System.Drawing.Size(949, 504);
			this.graphTabPage.TabIndex = 6;
			this.graphTabPage.Text = "-> graph";
			// 
			// graphPanel
			// 
			this.graphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.graphPanel.Name = "graphPanel";
			this.graphPanel.Size = new System.Drawing.Size(949, 424);
			this.graphPanel.TabIndex = 3;
			// 
			// panel6
			// 
			this.panel6.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.showAllButton,
																				 this.messageLabel,
																				 this.panel7,
																				 this.zoomOut5Button,
																				 this.zoomClose5Button,
																				 this.PrintGraphButton,
																				 this.gotoTrk5Button,
																				 this.label18,
																				 this.closeButton5});
			this.panel6.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel6.Location = new System.Drawing.Point(0, 424);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(949, 80);
			this.panel6.TabIndex = 2;
			// 
			// showAllButton
			// 
			this.showAllButton.Location = new System.Drawing.Point(155, 5);
			this.showAllButton.Name = "showAllButton";
			this.showAllButton.Size = new System.Drawing.Size(80, 25);
			this.showAllButton.TabIndex = 126;
			this.showAllButton.Text = "Show All";
			this.showAllButton.Click += new System.EventHandler(this.showAllButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.ForeColor = System.Drawing.Color.Red;
			this.messageLabel.Location = new System.Drawing.Point(335, 35);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(295, 18);
			this.messageLabel.TabIndex = 125;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel7
			// 
			this.panel7.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label17,
																				 this.timeUtcRadioButton,
																				 this.timeLocalRadioButton});
			this.panel7.Location = new System.Drawing.Point(650, -5);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(112, 59);
			this.panel7.TabIndex = 124;
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(9, 19);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(41, 18);
			this.label17.TabIndex = 17;
			this.label17.Text = "Time:";
			// 
			// timeUtcRadioButton
			// 
			this.timeUtcRadioButton.Location = new System.Drawing.Point(56, 9);
			this.timeUtcRadioButton.Name = "timeUtcRadioButton";
			this.timeUtcRadioButton.Size = new System.Drawing.Size(56, 26);
			this.timeUtcRadioButton.TabIndex = 15;
			this.timeUtcRadioButton.Text = "UTC";
			this.timeUtcRadioButton.CheckedChanged += new System.EventHandler(this.timeUtcRadioButton_CheckedChanged);
			// 
			// timeLocalRadioButton
			// 
			this.timeLocalRadioButton.Location = new System.Drawing.Point(56, 28);
			this.timeLocalRadioButton.Name = "timeLocalRadioButton";
			this.timeLocalRadioButton.Size = new System.Drawing.Size(56, 27);
			this.timeLocalRadioButton.TabIndex = 16;
			this.timeLocalRadioButton.Text = "local";
			this.timeLocalRadioButton.CheckedChanged += new System.EventHandler(this.timeLocalRadioButton_CheckedChanged);
			// 
			// zoomOut5Button
			// 
			this.zoomOut5Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomOut5Button.Location = new System.Drawing.Point(115, 30);
			this.zoomOut5Button.Name = "zoomOut5Button";
			this.zoomOut5Button.Size = new System.Drawing.Size(23, 24);
			this.zoomOut5Button.TabIndex = 123;
			this.zoomOut5Button.Text = "^";
			this.zoomOut5Button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zoomOut5Button.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomClose5Button
			// 
			this.zoomClose5Button.Location = new System.Drawing.Point(10, 30);
			this.zoomClose5Button.Name = "zoomClose5Button";
			this.zoomClose5Button.Size = new System.Drawing.Size(103, 24);
			this.zoomClose5Button.TabIndex = 122;
			this.zoomClose5Button.Text = "1m/pix";
			this.zoomClose5Button.Click += new System.EventHandler(this.zoomClose5Button_Click);
			// 
			// PrintGraphButton
			// 
			this.PrintGraphButton.Location = new System.Drawing.Point(330, 5);
			this.PrintGraphButton.Name = "PrintGraphButton";
			this.PrintGraphButton.Size = new System.Drawing.Size(81, 25);
			this.PrintGraphButton.TabIndex = 44;
			this.PrintGraphButton.Text = "Print...";
			this.PrintGraphButton.Visible = false;
			// 
			// gotoTrk5Button
			// 
			this.gotoTrk5Button.Location = new System.Drawing.Point(10, 5);
			this.gotoTrk5Button.Name = "gotoTrk5Button";
			this.gotoTrk5Button.Size = new System.Drawing.Size(128, 26);
			this.gotoTrk5Button.TabIndex = 20;
			this.gotoTrk5Button.Text = "Go to Trackpoint";
			this.gotoTrk5Button.Click += new System.EventHandler(this.gotoTrk5Button_Click);
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(15, 60);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(1708, 18);
			this.label18.TabIndex = 1;
			// 
			// closeButton5
			// 
			this.closeButton5.Location = new System.Drawing.Point(860, 15);
			this.closeButton5.Name = "closeButton5";
			this.closeButton5.Size = new System.Drawing.Size(81, 26);
			this.closeButton5.TabIndex = 22;
			this.closeButton5.Text = "Close";
			this.closeButton5.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// trackpointsTabPage
			// 
			this.trackpointsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																							 this.trackpointsDataGrid,
																							 this.panel2});
			this.trackpointsTabPage.Location = new System.Drawing.Point(4, 22);
			this.trackpointsTabPage.Name = "trackpointsTabPage";
			this.trackpointsTabPage.Size = new System.Drawing.Size(949, 504);
			this.trackpointsTabPage.TabIndex = 1;
			this.trackpointsTabPage.Text = "-> points <-";
			// 
			// trackpointsDataGrid
			// 
			this.trackpointsDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.trackpointsDataGrid.DataMember = "";
			this.trackpointsDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.trackpointsDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.trackpointsDataGrid.Name = "trackpointsDataGrid";
			this.trackpointsDataGrid.Size = new System.Drawing.Size(949, 424);
			this.trackpointsDataGrid.TabIndex = 2;
			// 
			// panel2
			// 
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.zoomOutButton,
																				 this.zoomCloseButton,
																				 this.label11,
																				 this.printFontComboBox,
																				 this.printTptButton,
																				 this.ungroupHereButton,
																				 this.editTptButton,
																				 this.browseTptButton,
																				 this.detailTptButton,
																				 this.gotoTrkButton,
																				 this.deleteTrkButton,
																				 this.messageTrkptLabel,
																				 this.closeButton1});
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 424);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(949, 80);
			this.panel2.TabIndex = 1;
			// 
			// zoomOutButton
			// 
			this.zoomOutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomOutButton.Location = new System.Drawing.Point(115, 30);
			this.zoomOutButton.Name = "zoomOutButton";
			this.zoomOutButton.Size = new System.Drawing.Size(22, 24);
			this.zoomOutButton.TabIndex = 123;
			this.zoomOutButton.Text = "^";
			this.zoomOutButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zoomOutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomCloseButton
			// 
			this.zoomCloseButton.Location = new System.Drawing.Point(10, 30);
			this.zoomCloseButton.Name = "zoomCloseButton";
			this.zoomCloseButton.Size = new System.Drawing.Size(103, 24);
			this.zoomCloseButton.TabIndex = 122;
			this.zoomCloseButton.Text = "1m/pix";
			this.zoomCloseButton.Click += new System.EventHandler(this.zoomCloseTrkptButton_Click);
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(270, 32);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(84, 22);
			this.label11.TabIndex = 46;
			this.label11.Text = "Print Font:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// printFontComboBox
			// 
			this.printFontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.printFontComboBox.Items.AddRange(new object[] {
																   "6",
																   "7",
																   "8",
																   "9",
																   "10"});
			this.printFontComboBox.Location = new System.Drawing.Point(355, 32);
			this.printFontComboBox.Name = "printFontComboBox";
			this.printFontComboBox.Size = new System.Drawing.Size(57, 21);
			this.printFontComboBox.TabIndex = 45;
			this.printFontComboBox.SelectedIndexChanged += new System.EventHandler(this.printFontComboBox_SelectedIndexChanged);
			// 
			// printTptButton
			// 
			this.printTptButton.Location = new System.Drawing.Point(330, 5);
			this.printTptButton.Name = "printTptButton";
			this.printTptButton.Size = new System.Drawing.Size(80, 25);
			this.printTptButton.TabIndex = 44;
			this.printTptButton.Text = "Print...";
			this.printTptButton.Click += new System.EventHandler(this.printTptButton_Click);
			// 
			// ungroupHereButton
			// 
			this.ungroupHereButton.Location = new System.Drawing.Point(540, 5);
			this.ungroupHereButton.Name = "ungroupHereButton";
			this.ungroupHereButton.Size = new System.Drawing.Size(187, 25);
			this.ungroupHereButton.TabIndex = 43;
			this.ungroupHereButton.Text = "Break at Selected Point";
			this.ungroupHereButton.Click += new System.EventHandler(this.ungroupHereButton_Click);
			// 
			// editTptButton
			// 
			this.editTptButton.Location = new System.Drawing.Point(425, 5);
			this.editTptButton.Name = "editTptButton";
			this.editTptButton.Size = new System.Drawing.Size(81, 25);
			this.editTptButton.TabIndex = 42;
			this.editTptButton.Text = "Edit";
			this.editTptButton.Click += new System.EventHandler(this.editTptButton_Click);
			// 
			// browseTptButton
			// 
			this.browseTptButton.Location = new System.Drawing.Point(145, 30);
			this.browseTptButton.Name = "browseTptButton";
			this.browseTptButton.Size = new System.Drawing.Size(80, 25);
			this.browseTptButton.TabIndex = 41;
			this.browseTptButton.Text = "Browse";
			this.browseTptButton.Click += new System.EventHandler(this.browseTptButton_Click);
			// 
			// detailTptButton
			// 
			this.detailTptButton.Location = new System.Drawing.Point(145, 5);
			this.detailTptButton.Name = "detailTptButton";
			this.detailTptButton.Size = new System.Drawing.Size(80, 25);
			this.detailTptButton.TabIndex = 40;
			this.detailTptButton.Text = "Detail";
			this.detailTptButton.Click += new System.EventHandler(this.detailTptButton_Click);
			// 
			// gotoTrkButton
			// 
			this.gotoTrkButton.Location = new System.Drawing.Point(10, 5);
			this.gotoTrkButton.Name = "gotoTrkButton";
			this.gotoTrkButton.Size = new System.Drawing.Size(127, 26);
			this.gotoTrkButton.TabIndex = 20;
			this.gotoTrkButton.Text = "Go to Trackpoint";
			this.gotoTrkButton.Click += new System.EventHandler(this.gotoTrkButton_Click);
			// 
			// deleteTrkButton
			// 
			this.deleteTrkButton.Location = new System.Drawing.Point(235, 5);
			this.deleteTrkButton.Name = "deleteTrkButton";
			this.deleteTrkButton.Size = new System.Drawing.Size(82, 26);
			this.deleteTrkButton.TabIndex = 21;
			this.deleteTrkButton.Text = "Delete";
			this.deleteTrkButton.Click += new System.EventHandler(this.deleteTrackpointButton_Click);
			// 
			// messageTrkptLabel
			// 
			this.messageTrkptLabel.Location = new System.Drawing.Point(15, 60);
			this.messageTrkptLabel.Name = "messageTrkptLabel";
			this.messageTrkptLabel.Size = new System.Drawing.Size(1708, 18);
			this.messageTrkptLabel.TabIndex = 1;
			// 
			// closeButton1
			// 
			this.closeButton1.Location = new System.Drawing.Point(860, 15);
			this.closeButton1.Name = "closeButton1";
			this.closeButton1.Size = new System.Drawing.Size(81, 26);
			this.closeButton1.TabIndex = 22;
			this.closeButton1.Text = "Close";
			this.closeButton1.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// routesTabPage
			// 
			this.routesTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.routesDataGrid,
																						this.panel5});
			this.routesTabPage.Location = new System.Drawing.Point(4, 22);
			this.routesTabPage.Name = "routesTabPage";
			this.routesTabPage.Size = new System.Drawing.Size(949, 504);
			this.routesTabPage.TabIndex = 5;
			this.routesTabPage.Text = "Routes";
			// 
			// routesDataGrid
			// 
			this.routesDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.routesDataGrid.DataMember = "";
			this.routesDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.routesDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.routesDataGrid.Name = "routesDataGrid";
			this.routesDataGrid.Size = new System.Drawing.Size(949, 424);
			this.routesDataGrid.TabIndex = 3;
			// 
			// panel5
			// 
			this.panel5.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.showInGeRteButton,
																				 this.setRteColorButton,
																				 this.reverseRteButton,
																				 this.zoomOut3Button,
																				 this.zoomClose3Button,
																				 this.print2LinkLabel,
																				 this.label8,
																				 this.startRouteNumericUpDown,
																				 this.toGpsRteButton,
																				 this.fromGpsRteButton,
																				 this.importRteButton,
																				 this.zoomRteButton,
																				 this.saveRteButton,
																				 this.joinRteButton,
																				 this.hideRteButton,
																				 this.showRteButton,
																				 this.goEndRteButton,
																				 this.goStartRteButton,
																				 this.deleteRteButton,
																				 this.messageRteLabel,
																				 this.closeRteButton});
			this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel5.Location = new System.Drawing.Point(0, 424);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(949, 80);
			this.panel5.TabIndex = 2;
			// 
			// showInGeRteButton
			// 
			this.showInGeRteButton.Location = new System.Drawing.Point(680, 30);
			this.showInGeRteButton.Name = "showInGeRteButton";
			this.showInGeRteButton.Size = new System.Drawing.Size(160, 25);
			this.showInGeRteButton.TabIndex = 130;
			this.showInGeRteButton.Text = "Show in Google Earth";
			this.showInGeRteButton.Click += new System.EventHandler(this.showInGeRteButton_Click);
			// 
			// setRteColorButton
			// 
			this.setRteColorButton.Location = new System.Drawing.Point(275, 5);
			this.setRteColorButton.Name = "setRteColorButton";
			this.setRteColorButton.Size = new System.Drawing.Size(75, 25);
			this.setRteColorButton.TabIndex = 129;
			this.setRteColorButton.Text = "Set Color";
			this.setRteColorButton.Click += new System.EventHandler(this.setRteColorButton_Click);
			// 
			// reverseRteButton
			// 
			this.reverseRteButton.Location = new System.Drawing.Point(275, 30);
			this.reverseRteButton.Name = "reverseRteButton";
			this.reverseRteButton.Size = new System.Drawing.Size(75, 25);
			this.reverseRteButton.TabIndex = 128;
			this.reverseRteButton.Text = "Reverse";
			this.reverseRteButton.Click += new System.EventHandler(this.reverseRteButton_Click);
			// 
			// zoomOut3Button
			// 
			this.zoomOut3Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomOut3Button.Location = new System.Drawing.Point(115, 30);
			this.zoomOut3Button.Name = "zoomOut3Button";
			this.zoomOut3Button.Size = new System.Drawing.Size(23, 24);
			this.zoomOut3Button.TabIndex = 127;
			this.zoomOut3Button.Text = "^";
			this.zoomOut3Button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zoomOut3Button.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomClose3Button
			// 
			this.zoomClose3Button.Location = new System.Drawing.Point(10, 30);
			this.zoomClose3Button.Name = "zoomClose3Button";
			this.zoomClose3Button.Size = new System.Drawing.Size(103, 24);
			this.zoomClose3Button.TabIndex = 126;
			this.zoomClose3Button.Text = "1m/pix";
			this.zoomClose3Button.Click += new System.EventHandler(this.zoomCloseButton_Click);
			// 
			// print2LinkLabel
			// 
			this.print2LinkLabel.Location = new System.Drawing.Point(380, 5);
			this.print2LinkLabel.Name = "print2LinkLabel";
			this.print2LinkLabel.Size = new System.Drawing.Size(46, 19);
			this.print2LinkLabel.TabIndex = 17;
			this.print2LinkLabel.TabStop = true;
			this.print2LinkLabel.Text = "print";
			this.print2LinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.print2LinkLabel_LinkClicked);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(445, 5);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(90, 33);
			this.label8.TabIndex = 16;
			this.label8.Text = "starting route nmbr in GPS";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// startRouteNumericUpDown
			// 
			this.startRouteNumericUpDown.Location = new System.Drawing.Point(540, 5);
			this.startRouteNumericUpDown.Maximum = new System.Decimal(new int[] {
																					20,
																					0,
																					0,
																					0});
			this.startRouteNumericUpDown.Minimum = new System.Decimal(new int[] {
																					1,
																					0,
																					0,
																					0});
			this.startRouteNumericUpDown.Name = "startRouteNumericUpDown";
			this.startRouteNumericUpDown.ReadOnly = true;
			this.startRouteNumericUpDown.Size = new System.Drawing.Size(42, 20);
			this.startRouteNumericUpDown.TabIndex = 15;
			this.startRouteNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.startRouteNumericUpDown.Value = new System.Decimal(new int[] {
																				  1,
																				  0,
																				  0,
																				  0});
			// 
			// toGpsRteButton
			// 
			this.toGpsRteButton.Location = new System.Drawing.Point(585, 5);
			this.toGpsRteButton.Name = "toGpsRteButton";
			this.toGpsRteButton.Size = new System.Drawing.Size(86, 25);
			this.toGpsRteButton.TabIndex = 14;
			this.toGpsRteButton.Text = "To GPS";
			this.toGpsRteButton.Click += new System.EventHandler(this.toGpsRteButton_Click);
			// 
			// fromGpsRteButton
			// 
			this.fromGpsRteButton.Location = new System.Drawing.Point(585, 30);
			this.fromGpsRteButton.Name = "fromGpsRteButton";
			this.fromGpsRteButton.Size = new System.Drawing.Size(86, 25);
			this.fromGpsRteButton.TabIndex = 10;
			this.fromGpsRteButton.Text = "From GPS";
			this.fromGpsRteButton.Click += new System.EventHandler(this.fromGpsRteButton_Click);
			// 
			// importRteButton
			// 
			this.importRteButton.Location = new System.Drawing.Point(760, 5);
			this.importRteButton.Name = "importRteButton";
			this.importRteButton.Size = new System.Drawing.Size(80, 25);
			this.importRteButton.TabIndex = 12;
			this.importRteButton.Text = "Import";
			this.importRteButton.Click += new System.EventHandler(this.importButton_Click);
			// 
			// zoomRteButton
			// 
			this.zoomRteButton.Location = new System.Drawing.Point(200, 5);
			this.zoomRteButton.Name = "zoomRteButton";
			this.zoomRteButton.Size = new System.Drawing.Size(60, 25);
			this.zoomRteButton.TabIndex = 5;
			this.zoomRteButton.Text = "Zoom";
			this.zoomRteButton.Click += new System.EventHandler(this.zoomRteButton_Click);
			// 
			// saveRteButton
			// 
			this.saveRteButton.Location = new System.Drawing.Point(680, 5);
			this.saveRteButton.Name = "saveRteButton";
			this.saveRteButton.Size = new System.Drawing.Size(80, 25);
			this.saveRteButton.TabIndex = 11;
			this.saveRteButton.Text = "Save";
			this.saveRteButton.Click += new System.EventHandler(this.saveRteButton_Click);
			// 
			// joinRteButton
			// 
			this.joinRteButton.Location = new System.Drawing.Point(360, 30);
			this.joinRteButton.Name = "joinRteButton";
			this.joinRteButton.Size = new System.Drawing.Size(85, 25);
			this.joinRteButton.TabIndex = 7;
			this.joinRteButton.Text = "Join Routes";
			this.joinRteButton.Click += new System.EventHandler(this.joinRteButton_Click);
			// 
			// hideRteButton
			// 
			this.hideRteButton.Location = new System.Drawing.Point(150, 30);
			this.hideRteButton.Name = "hideRteButton";
			this.hideRteButton.Size = new System.Drawing.Size(51, 25);
			this.hideRteButton.TabIndex = 4;
			this.hideRteButton.Text = "Hide";
			this.hideRteButton.Click += new System.EventHandler(this.hideRteButton_Click);
			// 
			// showRteButton
			// 
			this.showRteButton.Location = new System.Drawing.Point(150, 5);
			this.showRteButton.Name = "showRteButton";
			this.showRteButton.Size = new System.Drawing.Size(51, 25);
			this.showRteButton.TabIndex = 3;
			this.showRteButton.Text = "Show";
			this.showRteButton.Click += new System.EventHandler(this.showRteButton_Click);
			// 
			// goEndRteButton
			// 
			this.goEndRteButton.Location = new System.Drawing.Point(100, 5);
			this.goEndRteButton.Name = "goEndRteButton";
			this.goEndRteButton.Size = new System.Drawing.Size(38, 25);
			this.goEndRteButton.TabIndex = 2;
			this.goEndRteButton.Text = "End";
			this.goEndRteButton.Click += new System.EventHandler(this.goEndRteButton_Click);
			// 
			// goStartRteButton
			// 
			this.goStartRteButton.Location = new System.Drawing.Point(10, 5);
			this.goStartRteButton.Name = "goStartRteButton";
			this.goStartRteButton.Size = new System.Drawing.Size(86, 25);
			this.goStartRteButton.TabIndex = 1;
			this.goStartRteButton.Text = "Go to Start";
			this.goStartRteButton.Click += new System.EventHandler(this.goStartRteButton_Click);
			// 
			// deleteRteButton
			// 
			this.deleteRteButton.Location = new System.Drawing.Point(200, 30);
			this.deleteRteButton.Name = "deleteRteButton";
			this.deleteRteButton.Size = new System.Drawing.Size(60, 25);
			this.deleteRteButton.TabIndex = 6;
			this.deleteRteButton.Text = "Delete";
			this.deleteRteButton.Click += new System.EventHandler(this.deleteRteButton_Click);
			// 
			// messageRteLabel
			// 
			this.messageRteLabel.Location = new System.Drawing.Point(17, 60);
			this.messageRteLabel.Name = "messageRteLabel";
			this.messageRteLabel.Size = new System.Drawing.Size(1708, 18);
			this.messageRteLabel.TabIndex = 1;
			// 
			// closeRteButton
			// 
			this.closeRteButton.Location = new System.Drawing.Point(860, 15);
			this.closeRteButton.Name = "closeRteButton";
			this.closeRteButton.Size = new System.Drawing.Size(81, 26);
			this.closeRteButton.TabIndex = 13;
			this.closeRteButton.Text = "Close";
			this.closeRteButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// waypointsTabPage
			// 
			this.waypointsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.waypointsDataGrid,
																						   this.panel4});
			this.waypointsTabPage.Location = new System.Drawing.Point(4, 22);
			this.waypointsTabPage.Name = "waypointsTabPage";
			this.waypointsTabPage.Size = new System.Drawing.Size(949, 504);
			this.waypointsTabPage.TabIndex = 3;
			this.waypointsTabPage.Text = "Waypoints";
			// 
			// waypointsDataGrid
			// 
			this.waypointsDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.waypointsDataGrid.DataMember = "";
			this.waypointsDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.waypointsDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.waypointsDataGrid.Name = "waypointsDataGrid";
			this.waypointsDataGrid.Size = new System.Drawing.Size(949, 424);
			this.waypointsDataGrid.TabIndex = 3;
			// 
			// panel4
			// 
			this.panel4.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.showInGeWptButton,
																				 this.wptPrintDistanceCheckBox,
																				 this.label16,
																				 this.printFontComboBox1,
																				 this.printWptButton,
																				 this.zoom4OutButton,
																				 this.zoomClose4Button,
																				 this.editWptButton,
																				 this.browseButton,
																				 this.import1Button,
																				 this.detailButton,
																				 this.wptSaveButton,
																				 this.wptUploadFromGpsButton,
																				 this.wptDloadToGpsButton,
																				 this.gotoWptButton,
																				 this.deleteWptButton,
																				 this.messageWptLabel,
																				 this.closeButton3});
			this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel4.Location = new System.Drawing.Point(0, 424);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(949, 80);
			this.panel4.TabIndex = 2;
			// 
			// showInGeWptButton
			// 
			this.showInGeWptButton.Location = new System.Drawing.Point(680, 30);
			this.showInGeWptButton.Name = "showInGeWptButton";
			this.showInGeWptButton.Size = new System.Drawing.Size(160, 25);
			this.showInGeWptButton.TabIndex = 130;
			this.showInGeWptButton.Text = "Show in Google Earth";
			this.showInGeWptButton.Click += new System.EventHandler(this.showInGeWptButton_Click);
			// 
			// wptPrintDistanceCheckBox
			// 
			this.wptPrintDistanceCheckBox.Location = new System.Drawing.Point(450, 30);
			this.wptPrintDistanceCheckBox.Name = "wptPrintDistanceCheckBox";
			this.wptPrintDistanceCheckBox.Size = new System.Drawing.Size(85, 28);
			this.wptPrintDistanceCheckBox.TabIndex = 129;
			this.wptPrintDistanceCheckBox.Text = "distance";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(310, 32);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(84, 22);
			this.label16.TabIndex = 128;
			this.label16.Text = "Print Font:";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// printFontComboBox1
			// 
			this.printFontComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.printFontComboBox1.Items.AddRange(new object[] {
																	"6",
																	"7",
																	"8",
																	"9",
																	"10"});
			this.printFontComboBox1.Location = new System.Drawing.Point(395, 32);
			this.printFontComboBox1.Name = "printFontComboBox1";
			this.printFontComboBox1.Size = new System.Drawing.Size(47, 21);
			this.printFontComboBox1.TabIndex = 127;
			this.printFontComboBox1.SelectedIndexChanged += new System.EventHandler(this.printFontComboBox_SelectedIndexChanged);
			// 
			// printWptButton
			// 
			this.printWptButton.Location = new System.Drawing.Point(325, 5);
			this.printWptButton.Name = "printWptButton";
			this.printWptButton.Size = new System.Drawing.Size(140, 25);
			this.printWptButton.TabIndex = 126;
			this.printWptButton.Text = "Print Selected...";
			this.printWptButton.Click += new System.EventHandler(this.printWptButton_Click);
			this.printWptButton.MouseHover += new System.EventHandler(this.printWptButton_MouseHover);
			// 
			// zoom4OutButton
			// 
			this.zoom4OutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoom4OutButton.Location = new System.Drawing.Point(115, 30);
			this.zoom4OutButton.Name = "zoom4OutButton";
			this.zoom4OutButton.Size = new System.Drawing.Size(20, 24);
			this.zoom4OutButton.TabIndex = 125;
			this.zoom4OutButton.Text = "^";
			this.zoom4OutButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zoom4OutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomClose4Button
			// 
			this.zoomClose4Button.Location = new System.Drawing.Point(10, 30);
			this.zoomClose4Button.Name = "zoomClose4Button";
			this.zoomClose4Button.Size = new System.Drawing.Size(103, 24);
			this.zoomClose4Button.TabIndex = 124;
			this.zoomClose4Button.Text = "1m/pix";
			this.zoomClose4Button.Click += new System.EventHandler(this.zoomCloseWptButton_Click);
			// 
			// editWptButton
			// 
			this.editWptButton.Location = new System.Drawing.Point(475, 5);
			this.editWptButton.Name = "editWptButton";
			this.editWptButton.Size = new System.Drawing.Size(81, 25);
			this.editWptButton.TabIndex = 39;
			this.editWptButton.Text = "Edit";
			this.editWptButton.Click += new System.EventHandler(this.editWptButton_Click);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(145, 30);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(80, 25);
			this.browseButton.TabIndex = 38;
			this.browseButton.Text = "Browse";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// import1Button
			// 
			this.import1Button.Location = new System.Drawing.Point(760, 5);
			this.import1Button.Name = "import1Button";
			this.import1Button.Size = new System.Drawing.Size(80, 25);
			this.import1Button.TabIndex = 37;
			this.import1Button.Text = "Import";
			this.import1Button.Click += new System.EventHandler(this.importButton_Click);
			// 
			// detailButton
			// 
			this.detailButton.Location = new System.Drawing.Point(145, 5);
			this.detailButton.Name = "detailButton";
			this.detailButton.Size = new System.Drawing.Size(80, 25);
			this.detailButton.TabIndex = 36;
			this.detailButton.Text = "Detail";
			this.detailButton.Click += new System.EventHandler(this.detailButton_Click);
			// 
			// wptSaveButton
			// 
			this.wptSaveButton.Location = new System.Drawing.Point(680, 5);
			this.wptSaveButton.Name = "wptSaveButton";
			this.wptSaveButton.Size = new System.Drawing.Size(80, 25);
			this.wptSaveButton.TabIndex = 34;
			this.wptSaveButton.Text = "Save";
			this.wptSaveButton.Click += new System.EventHandler(this.wptSaveButton_Click);
			// 
			// wptUploadFromGpsButton
			// 
			this.wptUploadFromGpsButton.Location = new System.Drawing.Point(585, 30);
			this.wptUploadFromGpsButton.Name = "wptUploadFromGpsButton";
			this.wptUploadFromGpsButton.Size = new System.Drawing.Size(85, 25);
			this.wptUploadFromGpsButton.TabIndex = 32;
			this.wptUploadFromGpsButton.Text = "From GPS";
			this.wptUploadFromGpsButton.Click += new System.EventHandler(this.wptUploadFromGpsButton_Click);
			// 
			// wptDloadToGpsButton
			// 
			this.wptDloadToGpsButton.Location = new System.Drawing.Point(585, 5);
			this.wptDloadToGpsButton.Name = "wptDloadToGpsButton";
			this.wptDloadToGpsButton.Size = new System.Drawing.Size(85, 25);
			this.wptDloadToGpsButton.TabIndex = 33;
			this.wptDloadToGpsButton.Text = "To GPS";
			this.wptDloadToGpsButton.Click += new System.EventHandler(this.wptDloadToGpsButton_Click);
			// 
			// gotoWptButton
			// 
			this.gotoWptButton.Location = new System.Drawing.Point(10, 5);
			this.gotoWptButton.Name = "gotoWptButton";
			this.gotoWptButton.Size = new System.Drawing.Size(125, 25);
			this.gotoWptButton.TabIndex = 30;
			this.gotoWptButton.Text = "Go to Waypoint";
			this.gotoWptButton.Click += new System.EventHandler(this.gotoWptButton_Click);
			// 
			// deleteWptButton
			// 
			this.deleteWptButton.Location = new System.Drawing.Point(235, 5);
			this.deleteWptButton.Name = "deleteWptButton";
			this.deleteWptButton.Size = new System.Drawing.Size(81, 25);
			this.deleteWptButton.TabIndex = 31;
			this.deleteWptButton.Text = "Delete";
			this.deleteWptButton.Click += new System.EventHandler(this.deleteWptButton_Click);
			// 
			// messageWptLabel
			// 
			this.messageWptLabel.Location = new System.Drawing.Point(17, 60);
			this.messageWptLabel.Name = "messageWptLabel";
			this.messageWptLabel.Size = new System.Drawing.Size(1708, 18);
			this.messageWptLabel.TabIndex = 1;
			// 
			// closeButton3
			// 
			this.closeButton3.Location = new System.Drawing.Point(860, 15);
			this.closeButton3.Name = "closeButton3";
			this.closeButton3.Size = new System.Drawing.Size(81, 26);
			this.closeButton3.TabIndex = 35;
			this.closeButton3.Text = "Close";
			this.closeButton3.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// filterTabPage
			// 
			this.filterTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.label7,
																						this.label5,
																						this.playSpeedTrackBar,
																						this.rewindButton,
																						this.stopButton,
																						this.playButton,
																						this.messageFilterLabel,
																						this.noFilterRadioButton,
																						this.aroundTimeComboBox,
																						this.aroundTimeTimeDateTimePicker,
																						this.aroundTimeDateTimePicker,
																						this.label4,
																						this.aroundTimeRadioButton,
																						this.fromToRadioButton,
																						this.toTimeDateTimePicker,
																						this.fromTimeDateTimePicker,
																						this.toDateTimePicker,
																						this.fromDateTimePicker,
																						this.label3,
																						this.label2,
																						this.timeFilterDefaultsButton,
																						this.timeFilterDefaultsComboBox,
																						this.closeButton4});
			this.filterTabPage.Location = new System.Drawing.Point(4, 22);
			this.filterTabPage.Name = "filterTabPage";
			this.filterTabPage.Size = new System.Drawing.Size(949, 504);
			this.filterTabPage.TabIndex = 4;
			this.filterTabPage.Text = "By Time";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(20, 195);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(122, 30);
			this.label7.TabIndex = 57;
			this.label7.Text = "For track:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(680, 65);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(106, 18);
			this.label5.TabIndex = 24;
			this.label5.Text = "Replay Speed";
			// 
			// playSpeedTrackBar
			// 
			this.playSpeedTrackBar.Location = new System.Drawing.Point(710, 85);
			this.playSpeedTrackBar.Name = "playSpeedTrackBar";
			this.playSpeedTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.playSpeedTrackBar.Size = new System.Drawing.Size(45, 115);
			this.playSpeedTrackBar.TabIndex = 52;
			this.playSpeedTrackBar.Value = 5;
			this.playSpeedTrackBar.Scroll += new System.EventHandler(this.playSpeedTrackBar_Scroll);
			// 
			// rewindButton
			// 
			this.rewindButton.Location = new System.Drawing.Point(580, 130);
			this.rewindButton.Name = "rewindButton";
			this.rewindButton.Size = new System.Drawing.Size(36, 26);
			this.rewindButton.TabIndex = 48;
			this.rewindButton.Text = "<<";
			this.rewindButton.Click += new System.EventHandler(this.rewindButton_Click);
			// 
			// stopButton
			// 
			this.stopButton.Location = new System.Drawing.Point(495, 130);
			this.stopButton.Name = "stopButton";
			this.stopButton.Size = new System.Drawing.Size(34, 26);
			this.stopButton.TabIndex = 47;
			this.stopButton.Text = "[]";
			this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
			// 
			// playButton
			// 
			this.playButton.Location = new System.Drawing.Point(450, 130);
			this.playButton.Name = "playButton";
			this.playButton.Size = new System.Drawing.Size(34, 26);
			this.playButton.TabIndex = 46;
			this.playButton.Text = ">";
			this.playButton.Click += new System.EventHandler(this.playButton_Click);
			// 
			// messageFilterLabel
			// 
			this.messageFilterLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left);
			this.messageFilterLabel.Location = new System.Drawing.Point(20, 270);
			this.messageFilterLabel.Name = "messageFilterLabel";
			this.messageFilterLabel.Size = new System.Drawing.Size(745, 225);
			this.messageFilterLabel.TabIndex = 19;
			// 
			// noFilterRadioButton
			// 
			this.noFilterRadioButton.Location = new System.Drawing.Point(20, 240);
			this.noFilterRadioButton.Name = "noFilterRadioButton";
			this.noFilterRadioButton.Size = new System.Drawing.Size(405, 26);
			this.noFilterRadioButton.TabIndex = 53;
			this.noFilterRadioButton.Text = "No highlighting - do not apply time filter";
			this.noFilterRadioButton.CheckedChanged += new System.EventHandler(this.noFilterRadioButton_CheckedChanged);
			// 
			// aroundTimeComboBox
			// 
			this.aroundTimeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.aroundTimeComboBox.Items.AddRange(new object[] {
																	"10 sec",
																	"30 sec",
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
																	"1 day"});
			this.aroundTimeComboBox.Location = new System.Drawing.Point(580, 165);
			this.aroundTimeComboBox.Name = "aroundTimeComboBox";
			this.aroundTimeComboBox.Size = new System.Drawing.Size(121, 21);
			this.aroundTimeComboBox.TabIndex = 51;
			this.aroundTimeComboBox.SelectedIndexChanged += new System.EventHandler(this.aroundTimeComboBox_SelectedIndexChanged);
			// 
			// aroundTimeTimeDateTimePicker
			// 
			this.aroundTimeTimeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.aroundTimeTimeDateTimePicker.Location = new System.Drawing.Point(450, 165);
			this.aroundTimeTimeDateTimePicker.Name = "aroundTimeTimeDateTimePicker";
			this.aroundTimeTimeDateTimePicker.ShowUpDown = true;
			this.aroundTimeTimeDateTimePicker.Size = new System.Drawing.Size(102, 20);
			this.aroundTimeTimeDateTimePicker.TabIndex = 50;
			this.aroundTimeTimeDateTimePicker.ValueChanged += new System.EventHandler(this.aroundTimeTimeDateTimePicker_ValueChanged);
			// 
			// aroundTimeDateTimePicker
			// 
			this.aroundTimeDateTimePicker.Location = new System.Drawing.Point(170, 165);
			this.aroundTimeDateTimePicker.Name = "aroundTimeDateTimePicker";
			this.aroundTimeDateTimePicker.Size = new System.Drawing.Size(256, 20);
			this.aroundTimeDateTimePicker.TabIndex = 49;
			this.aroundTimeDateTimePicker.ValueChanged += new System.EventHandler(this.aroundTimeDateTimePicker_ValueChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(20, 165);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(120, 25);
			this.label4.TabIndex = 14;
			this.label4.Text = "Around:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// aroundTimeRadioButton
			// 
			this.aroundTimeRadioButton.Location = new System.Drawing.Point(20, 130);
			this.aroundTimeRadioButton.Name = "aroundTimeRadioButton";
			this.aroundTimeRadioButton.Size = new System.Drawing.Size(419, 27);
			this.aroundTimeRadioButton.TabIndex = 45;
			this.aroundTimeRadioButton.Text = "Highlight waypoints / trackpoints dated:";
			this.aroundTimeRadioButton.CheckedChanged += new System.EventHandler(this.aroundTimeRadioButton_CheckedChanged);
			// 
			// fromToRadioButton
			// 
			this.fromToRadioButton.Location = new System.Drawing.Point(20, 15);
			this.fromToRadioButton.Name = "fromToRadioButton";
			this.fromToRadioButton.Size = new System.Drawing.Size(419, 26);
			this.fromToRadioButton.TabIndex = 40;
			this.fromToRadioButton.Text = "Highlight waypoints / trackpoints dated:";
			this.fromToRadioButton.CheckedChanged += new System.EventHandler(this.fromToRadioButton_CheckedChanged);
			// 
			// toTimeDateTimePicker
			// 
			this.toTimeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.toTimeDateTimePicker.Location = new System.Drawing.Point(450, 85);
			this.toTimeDateTimePicker.Name = "toTimeDateTimePicker";
			this.toTimeDateTimePicker.ShowUpDown = true;
			this.toTimeDateTimePicker.Size = new System.Drawing.Size(102, 20);
			this.toTimeDateTimePicker.TabIndex = 44;
			this.toTimeDateTimePicker.ValueChanged += new System.EventHandler(this.toTimeDateTimePicker_ValueChanged);
			// 
			// fromTimeDateTimePicker
			// 
			this.fromTimeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.fromTimeDateTimePicker.Location = new System.Drawing.Point(450, 50);
			this.fromTimeDateTimePicker.Name = "fromTimeDateTimePicker";
			this.fromTimeDateTimePicker.ShowUpDown = true;
			this.fromTimeDateTimePicker.Size = new System.Drawing.Size(102, 20);
			this.fromTimeDateTimePicker.TabIndex = 42;
			this.fromTimeDateTimePicker.ValueChanged += new System.EventHandler(this.fromTimeDateTimePicker_ValueChanged);
			// 
			// toDateTimePicker
			// 
			this.toDateTimePicker.Location = new System.Drawing.Point(170, 85);
			this.toDateTimePicker.Name = "toDateTimePicker";
			this.toDateTimePicker.Size = new System.Drawing.Size(256, 20);
			this.toDateTimePicker.TabIndex = 43;
			this.toDateTimePicker.ValueChanged += new System.EventHandler(this.toDateTimePicker_ValueChanged);
			// 
			// fromDateTimePicker
			// 
			this.fromDateTimePicker.Location = new System.Drawing.Point(170, 50);
			this.fromDateTimePicker.Name = "fromDateTimePicker";
			this.fromDateTimePicker.Size = new System.Drawing.Size(256, 20);
			this.fromDateTimePicker.TabIndex = 41;
			this.fromDateTimePicker.ValueChanged += new System.EventHandler(this.fromDateTimePicker_ValueChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(75, 85);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 26);
			this.label3.TabIndex = 6;
			this.label3.Text = "To:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(75, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(69, 26);
			this.label2.TabIndex = 5;
			this.label2.Text = "From:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// timeFilterDefaultsButton
			// 
			this.timeFilterDefaultsButton.Location = new System.Drawing.Point(450, 200);
			this.timeFilterDefaultsButton.Name = "timeFilterDefaultsButton";
			this.timeFilterDefaultsButton.Size = new System.Drawing.Size(171, 23);
			this.timeFilterDefaultsButton.TabIndex = 55;
			this.timeFilterDefaultsButton.Text = "Recalculate Defaults";
			this.timeFilterDefaultsButton.Click += new System.EventHandler(this.timeFilterDefaultsButton_Click);
			// 
			// timeFilterDefaultsComboBox
			// 
			this.timeFilterDefaultsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.timeFilterDefaultsComboBox.Location = new System.Drawing.Point(170, 200);
			this.timeFilterDefaultsComboBox.Name = "timeFilterDefaultsComboBox";
			this.timeFilterDefaultsComboBox.Size = new System.Drawing.Size(256, 21);
			this.timeFilterDefaultsComboBox.TabIndex = 54;
			this.timeFilterDefaultsComboBox.SelectedIndexChanged += new System.EventHandler(this.timeFilterDefaultsComboBox_SelectedIndexChanged);
			// 
			// closeButton4
			// 
			this.closeButton4.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.closeButton4.Location = new System.Drawing.Point(780, 440);
			this.closeButton4.Name = "closeButton4";
			this.closeButton4.Size = new System.Drawing.Size(81, 26);
			this.closeButton4.TabIndex = 56;
			this.closeButton4.Text = "Close";
			this.closeButton4.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// duatsTabPage
			// 
			this.duatsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.duatsConvertButton,
																					   this.duatsTextBox});
			this.duatsTabPage.Location = new System.Drawing.Point(4, 22);
			this.duatsTabPage.Name = "duatsTabPage";
			this.duatsTabPage.Size = new System.Drawing.Size(949, 504);
			this.duatsTabPage.TabIndex = 7;
			this.duatsTabPage.Text = "duats.com";
			// 
			// duatsConvertButton
			// 
			this.duatsConvertButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.duatsConvertButton.Location = new System.Drawing.Point(815, 465);
			this.duatsConvertButton.Name = "duatsConvertButton";
			this.duatsConvertButton.Size = new System.Drawing.Size(125, 23);
			this.duatsConvertButton.TabIndex = 1;
			this.duatsConvertButton.Text = "Convert to Route";
			this.duatsConvertButton.Click += new System.EventHandler(this.duatsConvertButton_Click);
			// 
			// duatsTextBox
			// 
			this.duatsTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.duatsTextBox.Location = new System.Drawing.Point(5, 5);
			this.duatsTextBox.Multiline = true;
			this.duatsTextBox.Name = "duatsTextBox";
			this.duatsTextBox.Size = new System.Drawing.Size(800, 485);
			this.duatsTextBox.TabIndex = 0;
			this.duatsTextBox.Text = "paste duats.com flight plan here";
			// 
			// optionsTabPage
			// 
			this.optionsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.groupBox4,
																						 this.groupBox3,
																						 this.groupBox2,
																						 this.groupBox1,
																						 this.colorPaletteGroupBox,
																						 this.colorCodeGroupBox,
																						 this.panel3});
			this.optionsTabPage.Location = new System.Drawing.Point(4, 22);
			this.optionsTabPage.Name = "optionsTabPage";
			this.optionsTabPage.Size = new System.Drawing.Size(949, 504);
			this.optionsTabPage.TabIndex = 2;
			this.optionsTabPage.Text = "Options";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.pointsPerRouteNumericUpDown,
																					this.label14});
			this.groupBox4.Location = new System.Drawing.Point(45, 280);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(393, 60);
			this.groupBox4.TabIndex = 101;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Converting Tracks to Routes";
			// 
			// pointsPerRouteNumericUpDown
			// 
			this.pointsPerRouteNumericUpDown.Increment = new System.Decimal(new int[] {
																						  10,
																						  0,
																						  0,
																						  0});
			this.pointsPerRouteNumericUpDown.Location = new System.Drawing.Point(196, 28);
			this.pointsPerRouteNumericUpDown.Maximum = new System.Decimal(new int[] {
																						10000,
																						0,
																						0,
																						0});
			this.pointsPerRouteNumericUpDown.Minimum = new System.Decimal(new int[] {
																						20,
																						0,
																						0,
																						0});
			this.pointsPerRouteNumericUpDown.Name = "pointsPerRouteNumericUpDown";
			this.pointsPerRouteNumericUpDown.Size = new System.Drawing.Size(66, 20);
			this.pointsPerRouteNumericUpDown.TabIndex = 102;
			this.pointsPerRouteNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.pointsPerRouteNumericUpDown.Value = new System.Decimal(new int[] {
																					  20,
																					  0,
																					  0,
																					  0});
			this.pointsPerRouteNumericUpDown.ValueChanged += new System.EventHandler(this.pointsPerRouteNumericUpDown_ValueChanged);
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(18, 28);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(172, 18);
			this.label14.TabIndex = 101;
			this.label14.Text = "Max points per route:";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label6,
																					this.sanityCheckBox});
			this.groupBox3.Location = new System.Drawing.Point(45, 175);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(393, 95);
			this.groupBox3.TabIndex = 100;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Sanity Filter";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(30, 50);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(360, 38);
			this.label6.TabIndex = 63;
			this.label6.Text = "* Sanity Filter eliminates trips with low trackpoints  count when breaking tracks" +
				" into trips.";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// sanityCheckBox
			// 
			this.sanityCheckBox.Location = new System.Drawing.Point(28, 20);
			this.sanityCheckBox.Name = "sanityCheckBox";
			this.sanityCheckBox.Size = new System.Drawing.Size(362, 27);
			this.sanityCheckBox.TabIndex = 62;
			this.sanityCheckBox.Text = "apply Sanity Filter when breaking tracks into trips*";
			this.sanityCheckBox.CheckedChanged += new System.EventHandler(this.sanityCheckBox_CheckedChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label19,
																					this.trackNameStyleComboBox,
																					this.label12,
																					this.waypointNameStyleComboBox,
																					this.showNamesCheckBox,
																					this.showWaypointsCheckBox,
																					this.showNumbersCheckBox});
			this.groupBox2.Location = new System.Drawing.Point(45, 10);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(393, 155);
			this.groupBox2.TabIndex = 100;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Waypoints and Tracks Appearance";
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(20, 120);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(169, 26);
			this.label19.TabIndex = 69;
			this.label19.Text = "Tracks Naming Style:";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// trackNameStyleComboBox
			// 
			this.trackNameStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.trackNameStyleComboBox.Location = new System.Drawing.Point(195, 120);
			this.trackNameStyleComboBox.Name = "trackNameStyleComboBox";
			this.trackNameStyleComboBox.Size = new System.Drawing.Size(168, 21);
			this.trackNameStyleComboBox.TabIndex = 68;
			this.trackNameStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.trackNameStyleComboBox_SelectedIndexChanged);
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(18, 85);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(169, 26);
			this.label12.TabIndex = 67;
			this.label12.Text = "Waypoints Naming Style:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// waypointNameStyleComboBox
			// 
			this.waypointNameStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.waypointNameStyleComboBox.Location = new System.Drawing.Point(196, 85);
			this.waypointNameStyleComboBox.Name = "waypointNameStyleComboBox";
			this.waypointNameStyleComboBox.Size = new System.Drawing.Size(168, 21);
			this.waypointNameStyleComboBox.TabIndex = 66;
			this.waypointNameStyleComboBox.SelectedIndexChanged += new System.EventHandler(this.waypointNameStyleComboBox_SelectedIndexChanged);
			// 
			// showNamesCheckBox
			// 
			this.showNamesCheckBox.Location = new System.Drawing.Point(28, 60);
			this.showNamesCheckBox.Name = "showNamesCheckBox";
			this.showNamesCheckBox.Size = new System.Drawing.Size(257, 20);
			this.showNamesCheckBox.TabIndex = 62;
			this.showNamesCheckBox.Text = "show waypoints names";
			this.showNamesCheckBox.CheckedChanged += new System.EventHandler(this.showNamesCheckBox_CheckedChanged);
			// 
			// showWaypointsCheckBox
			// 
			this.showWaypointsCheckBox.Location = new System.Drawing.Point(28, 20);
			this.showWaypointsCheckBox.Name = "showWaypointsCheckBox";
			this.showWaypointsCheckBox.Size = new System.Drawing.Size(257, 20);
			this.showWaypointsCheckBox.TabIndex = 60;
			this.showWaypointsCheckBox.Text = "show tracks and waypoints";
			this.showWaypointsCheckBox.CheckedChanged += new System.EventHandler(this.showWaypointsCheckBox_CheckedChanged);
			// 
			// showNumbersCheckBox
			// 
			this.showNumbersCheckBox.Location = new System.Drawing.Point(28, 40);
			this.showNumbersCheckBox.Name = "showNumbersCheckBox";
			this.showNumbersCheckBox.Size = new System.Drawing.Size(257, 20);
			this.showNumbersCheckBox.TabIndex = 61;
			this.showNumbersCheckBox.Text = "show trackpoints numbers";
			this.showNumbersCheckBox.CheckedChanged += new System.EventHandler(this.showNumbersCheckBox_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.showTrackpointsCheckBox,
																					this.thicknessApplyButton,
																					this.label10,
																					this.routeThicknessNumericUpDown,
																					this.label9,
																					this.trackThicknessNumericUpDown});
			this.groupBox1.Location = new System.Drawing.Point(470, 250);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(275, 90);
			this.groupBox1.TabIndex = 100;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Line Thickness";
			// 
			// showTrackpointsCheckBox
			// 
			this.showTrackpointsCheckBox.Location = new System.Drawing.Point(150, 25);
			this.showTrackpointsCheckBox.Name = "showTrackpointsCheckBox";
			this.showTrackpointsCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.showTrackpointsCheckBox.Size = new System.Drawing.Size(115, 20);
			this.showTrackpointsCheckBox.TabIndex = 101;
			this.showTrackpointsCheckBox.Text = "draw points";
			this.showTrackpointsCheckBox.CheckedChanged += new System.EventHandler(this.showTrackpointsCheckBox_CheckedChanged);
			// 
			// thicknessApplyButton
			// 
			this.thicknessApplyButton.Location = new System.Drawing.Point(185, 55);
			this.thicknessApplyButton.Name = "thicknessApplyButton";
			this.thicknessApplyButton.Size = new System.Drawing.Size(80, 25);
			this.thicknessApplyButton.TabIndex = 68;
			this.thicknessApplyButton.Text = "Apply";
			this.thicknessApplyButton.Click += new System.EventHandler(this.thicknessApplyButton_Click);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(10, 56);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(55, 19);
			this.label10.TabIndex = 11;
			this.label10.Text = "routes";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// routeThicknessNumericUpDown
			// 
			this.routeThicknessNumericUpDown.DecimalPlaces = 1;
			this.routeThicknessNumericUpDown.Increment = new System.Decimal(new int[] {
																						  5,
																						  0,
																						  0,
																						  65536});
			this.routeThicknessNumericUpDown.Location = new System.Drawing.Point(75, 56);
			this.routeThicknessNumericUpDown.Maximum = new System.Decimal(new int[] {
																						5,
																						0,
																						0,
																						0});
			this.routeThicknessNumericUpDown.Minimum = new System.Decimal(new int[] {
																						1,
																						0,
																						0,
																						0});
			this.routeThicknessNumericUpDown.Name = "routeThicknessNumericUpDown";
			this.routeThicknessNumericUpDown.Size = new System.Drawing.Size(56, 20);
			this.routeThicknessNumericUpDown.TabIndex = 67;
			this.routeThicknessNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.routeThicknessNumericUpDown.Value = new System.Decimal(new int[] {
																					  1,
																					  0,
																					  0,
																					  0});
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(10, 25);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(55, 18);
			this.label9.TabIndex = 100;
			this.label9.Text = "tracks";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// trackThicknessNumericUpDown
			// 
			this.trackThicknessNumericUpDown.DecimalPlaces = 1;
			this.trackThicknessNumericUpDown.Increment = new System.Decimal(new int[] {
																						  5,
																						  0,
																						  0,
																						  65536});
			this.trackThicknessNumericUpDown.Location = new System.Drawing.Point(75, 25);
			this.trackThicknessNumericUpDown.Maximum = new System.Decimal(new int[] {
																						5,
																						0,
																						0,
																						0});
			this.trackThicknessNumericUpDown.Minimum = new System.Decimal(new int[] {
																						1,
																						0,
																						0,
																						0});
			this.trackThicknessNumericUpDown.Name = "trackThicknessNumericUpDown";
			this.trackThicknessNumericUpDown.Size = new System.Drawing.Size(56, 20);
			this.trackThicknessNumericUpDown.TabIndex = 66;
			this.trackThicknessNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.trackThicknessNumericUpDown.Value = new System.Decimal(new int[] {
																					  1,
																					  0,
																					  0,
																					  0});
			// 
			// colorPaletteGroupBox
			// 
			this.colorPaletteGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																							   this.colorPaletteMessageLabel,
																							   this.cp1Button,
																							   this.cp2Button,
																							   this.cp3Button,
																							   this.cp4Button,
																							   this.cp5Button,
																							   this.cp6Button,
																							   this.cp7Button,
																							   this.cp8Button});
			this.colorPaletteGroupBox.Location = new System.Drawing.Point(470, 70);
			this.colorPaletteGroupBox.Name = "colorPaletteGroupBox";
			this.colorPaletteGroupBox.Size = new System.Drawing.Size(275, 170);
			this.colorPaletteGroupBox.TabIndex = 100;
			this.colorPaletteGroupBox.TabStop = false;
			this.colorPaletteGroupBox.Text = "Color Palette for Tracks/Routes";
			// 
			// colorPaletteMessageLabel
			// 
			this.colorPaletteMessageLabel.Location = new System.Drawing.Point(18, 112);
			this.colorPaletteMessageLabel.Name = "colorPaletteMessageLabel";
			this.colorPaletteMessageLabel.Size = new System.Drawing.Size(242, 47);
			this.colorPaletteMessageLabel.TabIndex = 100;
			this.colorPaletteMessageLabel.Text = "colorPaletteMessageLabel";
			// 
			// cp1Button
			// 
			this.cp1Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp1Button.Location = new System.Drawing.Point(18, 28);
			this.cp1Button.Name = "cp1Button";
			this.cp1Button.Size = new System.Drawing.Size(103, 18);
			this.cp1Button.TabIndex = 0;
			this.cp1Button.TabStop = false;
			this.cp1Button.Click += new System.EventHandler(this.cp1Button_Click);
			// 
			// cp2Button
			// 
			this.cp2Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp2Button.Location = new System.Drawing.Point(121, 28);
			this.cp2Button.Name = "cp2Button";
			this.cp2Button.Size = new System.Drawing.Size(104, 18);
			this.cp2Button.TabIndex = 1;
			this.cp2Button.TabStop = false;
			this.cp2Button.Click += new System.EventHandler(this.cp2Button_Click);
			// 
			// cp3Button
			// 
			this.cp3Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp3Button.Location = new System.Drawing.Point(18, 46);
			this.cp3Button.Name = "cp3Button";
			this.cp3Button.Size = new System.Drawing.Size(103, 19);
			this.cp3Button.TabIndex = 2;
			this.cp3Button.TabStop = false;
			this.cp3Button.Click += new System.EventHandler(this.cp3Button_Click);
			// 
			// cp4Button
			// 
			this.cp4Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp4Button.Location = new System.Drawing.Point(121, 46);
			this.cp4Button.Name = "cp4Button";
			this.cp4Button.Size = new System.Drawing.Size(104, 19);
			this.cp4Button.TabIndex = 3;
			this.cp4Button.TabStop = false;
			this.cp4Button.Click += new System.EventHandler(this.cp4Button_Click);
			// 
			// cp5Button
			// 
			this.cp5Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp5Button.Location = new System.Drawing.Point(18, 65);
			this.cp5Button.Name = "cp5Button";
			this.cp5Button.Size = new System.Drawing.Size(103, 19);
			this.cp5Button.TabIndex = 4;
			this.cp5Button.TabStop = false;
			this.cp5Button.Click += new System.EventHandler(this.cp5Button_Click);
			// 
			// cp6Button
			// 
			this.cp6Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp6Button.Location = new System.Drawing.Point(121, 65);
			this.cp6Button.Name = "cp6Button";
			this.cp6Button.Size = new System.Drawing.Size(104, 19);
			this.cp6Button.TabIndex = 5;
			this.cp6Button.TabStop = false;
			this.cp6Button.Click += new System.EventHandler(this.cp6Button_Click);
			// 
			// cp7Button
			// 
			this.cp7Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp7Button.Location = new System.Drawing.Point(18, 84);
			this.cp7Button.Name = "cp7Button";
			this.cp7Button.Size = new System.Drawing.Size(103, 19);
			this.cp7Button.TabIndex = 6;
			this.cp7Button.TabStop = false;
			this.cp7Button.Click += new System.EventHandler(this.cp7Button_Click);
			// 
			// cp8Button
			// 
			this.cp8Button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cp8Button.Location = new System.Drawing.Point(121, 84);
			this.cp8Button.Name = "cp8Button";
			this.cp8Button.Size = new System.Drawing.Size(104, 19);
			this.cp8Button.TabIndex = 7;
			this.cp8Button.TabStop = false;
			this.cp8Button.Click += new System.EventHandler(this.cp8Button_Click);
			// 
			// colorCodeGroupBox
			// 
			this.colorCodeGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.colorSpeedTracksCheckBox,
																							this.colorElevTracksCheckBox});
			this.colorCodeGroupBox.Location = new System.Drawing.Point(470, 10);
			this.colorCodeGroupBox.Name = "colorCodeGroupBox";
			this.colorCodeGroupBox.Size = new System.Drawing.Size(275, 50);
			this.colorCodeGroupBox.TabIndex = 100;
			this.colorCodeGroupBox.TabStop = false;
			this.colorCodeGroupBox.Text = "Color Coding for Tracks";
			// 
			// colorSpeedTracksCheckBox
			// 
			this.colorSpeedTracksCheckBox.Location = new System.Drawing.Point(140, 20);
			this.colorSpeedTracksCheckBox.Name = "colorSpeedTracksCheckBox";
			this.colorSpeedTracksCheckBox.Size = new System.Drawing.Size(125, 25);
			this.colorSpeedTracksCheckBox.TabIndex = 65;
			this.colorSpeedTracksCheckBox.Text = "by speed";
			this.colorSpeedTracksCheckBox.CheckedChanged += new System.EventHandler(this.colorSpeedTracksCheckBox_CheckedChanged);
			// 
			// colorElevTracksCheckBox
			// 
			this.colorElevTracksCheckBox.Location = new System.Drawing.Point(18, 20);
			this.colorElevTracksCheckBox.Name = "colorElevTracksCheckBox";
			this.colorElevTracksCheckBox.Size = new System.Drawing.Size(112, 25);
			this.colorElevTracksCheckBox.TabIndex = 64;
			this.colorElevTracksCheckBox.Text = "by elevation";
			this.colorElevTracksCheckBox.CheckedChanged += new System.EventHandler(this.colorElevTracksCheckBox_CheckedChanged);
			// 
			// panel3
			// 
			this.panel3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label15,
																				 this.label13,
																				 this.closeButton2});
			this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel3.Location = new System.Drawing.Point(0, 419);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(949, 85);
			this.panel3.TabIndex = 3;
			// 
			// label15
			// 
			this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label15.Location = new System.Drawing.Point(18, 19);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(94, 32);
			this.label15.TabIndex = 71;
			this.label15.Text = "Hint:";
			this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(131, 5);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(614, 70);
			this.label13.TabIndex = 70;
			this.label13.Text = "to change the color of your route or track, use \"Set Color\" buttons in \"Track Log" +
				"s\" and \"Routes\" tabs above. If you want to change random color choices, click on" +
				" the color palette buttons above.";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// closeButton2
			// 
			this.closeButton2.Location = new System.Drawing.Point(780, 20);
			this.closeButton2.Name = "closeButton2";
			this.closeButton2.Size = new System.Drawing.Size(81, 26);
			this.closeButton2.TabIndex = 69;
			this.closeButton2.Text = "Close";
			this.closeButton2.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// printDocumentRteTrks
			// 
			this.printDocumentRteTrks.BeginPrint += new System.Drawing.Printing.PrintEventHandler(this.printDocumentRteTrks_BeginPrint);
			this.printDocumentRteTrks.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocumentRteTrks_PrintPage);
			// 
			// printDocumentWpts
			// 
			this.printDocumentWpts.BeginPrint += new System.Drawing.Printing.PrintEventHandler(this.printDocumentWpts_BeginPrint);
			this.printDocumentWpts.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocumentWpts_PrintPage);
			// 
			// DlgWaypointsManager
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(957, 530);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(800, 200);
			this.Name = "DlgWaypointsManager";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Manage Routes, Tracks and Waypoints";
			this.Resize += new System.EventHandler(this.DlgWaypointsManager_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgTrackpointsManager_Closing);
			this.Load += new System.EventHandler(this.DlgWaypointsManager_Load);
			this.Move += new System.EventHandler(this.DlgWaypointsManager_Move);
			this.tabControl1.ResumeLayout(false);
			this.tracksTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tracksDataGrid)).EndInit();
			this.panel1.ResumeLayout(false);
			this.graphTabPage.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.panel7.ResumeLayout(false);
			this.trackpointsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackpointsDataGrid)).EndInit();
			this.panel2.ResumeLayout(false);
			this.routesTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.routesDataGrid)).EndInit();
			this.panel5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.startRouteNumericUpDown)).EndInit();
			this.waypointsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.waypointsDataGrid)).EndInit();
			this.panel4.ResumeLayout(false);
			this.filterTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.playSpeedTrackBar)).EndInit();
			this.duatsTabPage.ResumeLayout(false);
			this.optionsTabPage.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pointsPerRouteNumericUpDown)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.routeThicknessNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackThicknessNumericUpDown)).EndInit();
			this.colorPaletteGroupBox.ResumeLayout(false);
			this.colorCodeGroupBox.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public ArrayList GetSelectedRows(DataGrid dataGrid) 
		{ 
			ArrayList al = new ArrayList(); 
 
			for(int i = 0; i < ((DataSet)dataGrid.DataSource).Tables[0].Rows.Count; ++i) 
			{ 
				if(dataGrid.IsSelected(i))
				{
					al.Add(i);
				}
			} 
			return al; 
		} 

		public ArrayList GetSelectedRowsView(DataGrid dataGrid) 
		{ 
			ArrayList al = new ArrayList(); 
			DataTable srcTable = ((DataView)dataGrid.DataSource).Table;
 
			for(int i = 0; i < srcTable.Rows.Count; ++i) 
			{ 
				if(dataGrid.IsSelected(i))
				{
					al.Add(i);
				}
			} 
			return al; 
		} 

		// this one works for waypoints grid only so far, as columnId is specific to waypoints table.
		public DataView GetSelectedRowsDataView(DataGrid dataGrid) 
		{ 
			DataTable srcTable = ((DataView)dataGrid.DataSource).Table;
			DataTable newTable = srcTable.Clone();
			newTable.Clear();
			for(int i=0; i < newTable.Columns.Count ;i++)
			{
				newTable.Columns[i].ReadOnly = false;
			}
 
			int columnId = 0;
			for(int i = 0; i < srcTable.Rows.Count; ++i) 
			{ 
				if(dataGrid.IsSelected(i))
				{
					int id = (int)dataGrid[i, columnId];
					DataRow srcRow = null;
					foreach(DataRow row in srcTable.Rows)
					{
						if((int)row["id"] == id)
						{
							srcRow = row;
							break;
						}
					}
					if(srcRow != null)
					{
						DataRow newRow = newTable.NewRow();
						for(int j=0; j < srcRow.ItemArray.Length ;j++)
						{
							newRow[j] = srcRow[j];
						}
						newTable.Rows.Add(newRow);
					}
				}
			} 
			return newTable.DefaultView; 
		} 

		private void selectTrackRow()
		{
			DataTable table = ((DataSet)tracksDataGrid.DataSource).Tables[0];
			int column = 0;
			for(int row = 0; m_trackIdToSelect >= 0 && row < table.Rows.Count; ++row) 
			{
				long trackId = (long)((int)tracksDataGrid[row, column]);
				if(trackId == m_trackIdToSelect)
				{
					tracksDataGrid.Select(row);
					return;
				}
			}
			if(table.Rows.Count > 0)
			{
				tracksDataGrid.Select(0);
			}
		}

		private void selectRouteRow()
		{
			DataTable table = ((DataSet)routesDataGrid.DataSource).Tables[0];
			int column = 0;
			for(int row = 0; m_routeIdToSelect >= 0 && row < table.Rows.Count; ++row) 
			{
				long routeId = (long)((int)routesDataGrid[row, column]);
				if(routeId == m_routeIdToSelect)
				{
					routesDataGrid.Select(row);
					return;
				}
			}
			if(table.Rows.Count > 0)
			{
				routesDataGrid.Select(0);
			}
		}

		// call PutOnMap() when set of tracks/trackpoints changed, but the view point doesn't move:
		private void PutOnMap()
		{
			WaypointsCache.RefreshWaypointsDisplayed();
			LayerWaypoints.This.init();		// provoke PutOnMap on layer's Paint()
			m_pictureManager.Refresh();
		}

		private void HandleTrackShowChanges(object sender, BoolValueChangedEventArgs e)
		{
#if DEBUG
			string str = "Bool changed: row " + e.Row + "   value " + e.BoolValue;
#endif
			int column = 0;
			DataTable table = m_tracksDS.Tables[0];
			int row = e.Row;
			long trackId = (long)((int)tracksDataGrid[row, column]);
			Track trk = WaypointsCache.getTrackById(trackId);
#if DEBUG
			str += "  " + row + "=" + trackId + " ";
#endif
			if(trk != null)
			{
				trk.Enabled = e.BoolValue;
				PutOnMap();
			}

#if DEBUG
			LibSys.StatusBar.Trace(str);
			//messageTrkLabel.Text = str;
#endif
		}

		private void HandleRouteShowChanges(object sender, BoolValueChangedEventArgs e)
		{
#if DEBUG
			string str = "Bool changed: row " + e.Row + "   value " + e.BoolValue;
#endif
			int column = 0;
			DataTable table = m_routesDS.Tables[0];
			int row = e.Row;
			long routeId = (long)((int)routesDataGrid[row, column]);
			Track rte = WaypointsCache.getTrackById(routeId);
#if DEBUG
			str += "  " + row + "=" + routeId + " ";
#endif
			if(rte != null)
			{
				rte.Enabled = e.BoolValue;
				PutOnMap();
			}

#if DEBUG
			LibSys.StatusBar.Trace(str);
			//messageTrkLabel.Text = str;
#endif
		}

		private void DlgWaypointsManager_Resize(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private void DlgWaypointsManager_Move(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private void memorizeSizeAndPosition()
		{
			bool formMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
			if(!formMaximized && !inResize)
			{
				Project.wptManagerWidth = this.ClientSize.Width;
				Project.wptManagerHeight = this.ClientSize.Height;
				wptManagerX = this.Location.X;
				wptManagerY = this.Location.Y;
			}
		}

		private void DlgTrackpointsManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try		// columns may be not initialized yet
			{
				Project.symbolColWidth = SymbolCol.Width; 
				Project.nameColWidth = NameCol.Width; 
				Project.urlNameColWidth = UrlNameCol.Width; 
				Project.descColWidth = DescCol.Width; 
				Project.commentColWidth = CommentCol.Width; 
			} 
			catch {}

			selectedTabIndex = tabControl1.SelectedIndex;
			m_cameraManager.removeMarkLocation();
			playing = false;
			TimeFilter.Enabled = false;
			m_pictureManager.Refresh();

			/*
			if(m_playThread != null)
			{
				m_playThread.Abort();
				m_playThread = null;
			}
			*/
			This = null;
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void ClearMessages()
		{
			messageTrkLabel.Text = "";
			messageTrkptLabel.Text = "";
			messageRteLabel.Text = "";
			messageWptLabel.Text = "";
			messageFilterLabel.Text = "";
		}

		private bool rebuildingTracks = false;
		private bool rebuildingRoutes = false;
		private bool rebuildingTrackpoints = false;
		private bool rebuildingWaypoints = false;

		private void tracks_ColumnChanging(object sender, System.Data.DataColumnChangeEventArgs e) 
		{
			//Only check for errors in the Name column
			if (!rebuildingTracks && e.Column.ColumnName.Equals("name")) 
			{
				//MessageBox.Show ("Col is " + tracksDataGrid.CurrentCell.ColumnNumber
				//	+ ", Row is " + tracksDataGrid.CurrentCell.RowNumber 
				//	+ ", Proposed value is " + e.ProposedValue);

				//Do not allow "aaa" as name
				if (e.ProposedValue.Equals("aaa")) 
				{
					object badValue = e.ProposedValue;
					e.ProposedValue = "Bad Data";
					e.Row.RowError = "The Name column contains an error";
					e.Row.SetColumnError(e.Column, "Name cannot be " + badValue);
				}

				ClearMessages();
				int column = 0;
				DataTable table = m_tracksDS.Tables[0];
				int row = tracksDataGrid.CurrentCell.RowNumber;
				long trackId = (long)((int)tracksDataGrid[row, column]);
				Track trk = WaypointsCache.getTrackById(trackId);
				if(trk != null)
				{
					trk.Name = "" + e.ProposedValue;
				}
				PutOnMap();
			}
		}

		private void tracks_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e) 
		{
			if (!rebuildingTracks && !deletingTracksRoutes) 
			{
				try 
				{
					int column = 0;
					long trackId = (long)((int)e.Row[column]);
					DataTable table = m_tracksDS.Tables[0];
					WaypointsCache.RemoveTrackById(trackId);
					PutOnMap();
				} 
				catch {}
			}
		}

		private void routes_ColumnChanging(object sender, System.Data.DataColumnChangeEventArgs e) 
		{
			//Only check for errors in the Name column
			if (!rebuildingRoutes && e.Column.ColumnName.Equals("name"))
			{
				//MessageBox.Show ("Col is " + routesDataGrid.CurrentCell.ColumnNumber
				//	+ ", Row is " + routesDataGrid.CurrentCell.RowNumber 
				//	+ ", Proposed value is " + e.ProposedValue);

				//Do not allow "aaa" as name
				if (e.ProposedValue.Equals("aaa")) 
				{
					object badValue = e.ProposedValue;
					e.ProposedValue = "Bad Data";
					e.Row.RowError = "The Name column contains an error";
					e.Row.SetColumnError(e.Column, "Name cannot be " + badValue);
				}

				ClearMessages();
				int column = 0;
				DataTable table = m_routesDS.Tables[0];
				int row = routesDataGrid.CurrentCell.RowNumber;
				long routeId = (long)((int)routesDataGrid[row, column]);
				Track rte = WaypointsCache.getTrackById(routeId);
				if(rte != null)
				{
					rte.Name = "" + e.ProposedValue;
				}
				PutOnMap();
			}
		}

		private void routes_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e) 
		{
			if (!rebuildingRoutes && !deletingTracksRoutes) 
			{
				try 
				{
					int column = 0;
					long routeId = (long)((int)e.Row[column]);
					DataTable table = m_routesDS.Tables[0];
					WaypointsCache.RemoveTrackById(routeId);
					PutOnMap();
				} 
				catch {}
			}
		}

		private bool deletingTracksRoutes = false;

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			deleteTrackRoute(tracksDataGrid, m_tracksDS);
		}

		private void deleteRteButton_Click(object sender, System.EventArgs e)
		{
			deleteTrackRoute(routesDataGrid, m_routesDS);
		}

		private void deleteTrackRoute(DataGrid dataGrid, DataSet dataSet)
		{
			ArrayList selected = GetSelectedRows(dataGrid);
			if(selected.Count > 0)
			{
				deletingTracksRoutes = true;
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = dataSet.Tables[0];
				for(int i=selected.Count-1; i >=0 ;i--)
				{
					int row = (int)selected[i];
					long trackId = (long)((int)dataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + trackId + " ";
#endif
					// rows in the table are numbered differently than rows in the grid, so we go by track ID:
					for(int indx=0; indx < table.Rows.Count ;indx++ )
					{
						long rowTrackId = (long)((int)table.Rows[indx][column]);
						if(rowTrackId == trackId)
						{
							table.Rows[indx].Delete();
							WaypointsCache.RemoveTrackById(trackId);
							break;
						}
					}
				}
				m_trackIdToSelect = -1;
#if DEBUG
				messageTrkLabel.Text = "selected: " + str;
#else
				messageTrkLabel.Text = "";
#endif
				PutOnMap();
				deletingTracksRoutes = false;
			}
			else
			{
				//messageTrkLabel.Text = "Error: select a track to delete first";
				Project.ShowPopup(deleteButton, "\nselect a track/route to delete first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void wptSaveButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int columnWptId = 0;
				DataTable table = m_waypointsDS.Tables[0];
				ArrayList waypoints = new ArrayList();		// array of Waypoint
				for(int i=0; i < selected.Count ;i++)
				{
					int row = (int)selected[i];
					int waypointId = (int)waypointsDataGrid[row, columnWptId];
#if DEBUG
					str += "" + row + "=" + waypointId + " ";
#endif
					Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
					if(wpt != null)
					{
						//m_wptIdToSelect = wpt.Id;
						waypoints.Add(wpt);
					}
				}
#if DEBUG
				messageWptLabel.Text = "selected: " + str;
#else
				messageWptLabel.Text = "";
#endif
				FileExportForm fileExportForm = new FileExportForm(null, false, waypoints, true);
				fileExportForm.ShowDialog();
			}
			else if (Project.goingDown)		// make it a little bit more easy on closing
			{
				FileExportForm fileExportForm = new FileExportForm(null, true, null, true);
				fileExportForm.ShowDialog();
			}
			else
			{
				//messageWptLabel.Text = "Error: select a waypoint first";
				Project.ShowPopup(wptSaveButton, "\nselect a waypoint(s) first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void saveButton_Click(object sender, System.EventArgs e)
		{
			saveTracksRoutes(tracksDataGrid, m_tracksDS);
			this.rebuildTracksTab();
		}

		private void saveRteButton_Click(object sender, System.EventArgs e)
		{
			saveTracksRoutes(routesDataGrid, m_routesDS);
			this.rebuildRoutesTab();
		}

		private void saveTracksRoutes(DataGrid dataGrid, DataSet dataSet)
		{
			ArrayList selected = GetSelectedRows(dataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = dataSet.Tables[0];
				ArrayList tracks = new ArrayList();		// array of Track
				for(int i=0; i < selected.Count ;i++)
				{
					int row = (int)selected[i];
					long trackId = (long)((int)dataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + trackId + " ";
#endif
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						tracks.Add(trk);
					}
				}
#if DEBUG
				messageTrkLabel.Text = "selected: " + str;
#else
				messageTrkLabel.Text = "";
#endif
				FileExportForm fileExportForm = new FileExportForm(tracks, true, null, false);
				fileExportForm.ShowDialog();
			}
			else if (Project.goingDown)		// make it a little bit more easy on closing
			{
				FileExportForm fileExportForm = new FileExportForm(null, true, null, true);
				fileExportForm.ShowDialog();
			}
			else
			{
				//messageTrkLabel.Text = "Error: select a track first";
				Project.ShowPopup(saveButton, "\nselect a track/route first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		// first trackpoint of the first selected trail
		private void goStartButton_Click(object sender, System.EventArgs e)
		{
			goStartEnd(tracksDataGrid, m_tracksDS, true);
		}

		// first routepoint of the first selected route
		private void goStartRteButton_Click(object sender, System.EventArgs e)
		{
			goStartEnd(routesDataGrid, m_routesDS, true);
		}

		// last trackpoint of the last selected trail
		private void goEndButton_Click(object sender, System.EventArgs e)
		{
			goStartEnd(tracksDataGrid, m_tracksDS, false);
		}

		// last routepoint of the last selected route
		private void goEndRteButton_Click(object sender, System.EventArgs e)
		{
			goStartEnd(routesDataGrid, m_routesDS, false);
		}

		private void goStartEnd(DataGrid dataGrid, DataSet dataSet, bool toStart)
		{
			ArrayList selected = GetSelectedRows(dataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				int row = (int)selected[toStart ? 0 : (selected.Count-1)];
				int column = 0;
				DataTable table = dataSet.Tables[0];
				long trackId = (long)((int)dataGrid[row, column]);
				Track trk = WaypointsCache.getTrackById(trackId);
				if(trk != null)
				{
					m_trackIdToSelect = trk.Id;
					m_routeIdToSelect = trk.Id;
					Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(toStart ? 0 : (trk.Trackpoints.Count-1));
					//messageTrkLabel.Text = "trk:" + trackId + " wpt: " + wpt;
					GeoCoord newLoc = new GeoCoord(wpt.Location.Lng, wpt.Location.Lat, m_cameraManager.Location.Elev);
					m_cameraManager.Location = newLoc;
				}
			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a track/route first";
				Project.ShowPopup(goStartButton, "\nselect a track/route first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void showButton_Click(object sender, System.EventArgs e)
		{
			setTracksEnabled(tracksDataGrid, m_tracksDS, true);
		}

		private void hideButton_Click(object sender, System.EventArgs e)
		{
			setTracksEnabled(tracksDataGrid, m_tracksDS, false);
		}

		private void showRteButton_Click(object sender, System.EventArgs e)
		{
			setTracksEnabled(routesDataGrid, m_routesDS, true);
		}

		private void hideRteButton_Click(object sender, System.EventArgs e)
		{
			setTracksEnabled(routesDataGrid, m_routesDS, false);
		}

		private void setTracksEnabled(DataGrid dataGrid, DataSet dataSet, bool enabled)
		{
			ArrayList selected = GetSelectedRows(dataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				int column = 0;
				DataTable table = dataSet.Tables[0];
				foreach(int row in selected)
				{
					long trackId = (long)((int)dataGrid[row, column]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						// rows in the table are numbered differently than rows in the grid, so we go by track ID:
						for(int indx=0; indx < table.Rows.Count ;indx++ )
						{
							long rowTrackId = (long)((int)table.Rows[indx][column]);
							if(rowTrackId == trackId)
							{
								m_trackIdToSelect = trk.Id;
								m_routeIdToSelect = trk.Id;
								trk.Enabled = enabled;
								table.Rows[indx]["displayed"] = enabled;
								break;
							}
						}
					}
				}
				PutOnMap();
			}
			else
			{
				//messageTrkLabel.Text = "Error: select a track first";
				Project.ShowPopup(hideButton, "\nselect a track/route first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void zoomButton_Click(object sender, System.EventArgs e)
		{
			zoomToTrackRoute(tracksDataGrid, m_tracksDS);
		}

		private void zoomRteButton_Click(object sender, System.EventArgs e)
		{
			zoomToTrackRoute(routesDataGrid, m_routesDS);
		}

		private void zoomToTrackRoute(DataGrid dataGrid, DataSet dataSet)
		{
			ArrayList selected = GetSelectedRows(dataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				WaypointsCache.resetBoundaries();
				int column = 0;
				DataTable table = dataSet.Tables[0];
				foreach(int row in selected)
				{
					long trackId = (long)((int)dataGrid[row, column]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						m_routeIdToSelect = trk.Id;
						//messageTrkLabel.Text = "trk:" + trackId + " topLeft: " + trk.TopLeft + "  bottomRight: " + trk.BottomRight;
						WaypointsCache.pushBoundaries(trk.TopLeft);
						WaypointsCache.pushBoundaries(trk.BottomRight);
					}
				}
				m_cameraManager.zoomToCorners();
			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a track first";
				Project.ShowPopup(zoomButton, "\nselect a track/route first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void groupButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(tracksDataGrid);
			if(selected.Count >= 2)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = m_tracksDS.Tables[0];
				ArrayList tracks = new ArrayList();		// of Track
				for(int i=0; i < selected.Count; i++)
				{
					int row = (int)selected[i];
					long trackId = (long)((int)tracksDataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + trackId + " ";
#endif
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						tracks.Add(trk);
						// hide the original tracks because the joint track will cover them anyway:
						trk.Enabled = false;
					}
				}
				//messageTrkLabel.Text = "selected: " + str;
				Track joinedTrack = WaypointsCache.groupTracks(tracks);
				joinedTrack.Enabled = true;
				m_trackIdToSelect = joinedTrack.Id;
				// refresh and rebind the datagrid:
				rebuildTracksTab();
				PutOnMap();
				WaypointsCache.isDirty = true;
			}
			else
			{
				//messageTrkLabel.Text = "Error: select at least two tracks to join";
				Project.ShowPopup(groupButton, "\nselect at least two tracks to join" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void toRteButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(tracksDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = m_tracksDS.Tables[0];
				for(int i=0; i < selected.Count; i++)
				{
					int row = (int)selected[i];
					long trackId = (long)((int)tracksDataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + trackId + " ";
#endif
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						Track rte = trk.toRoute();
						WaypointsCache.TracksAll.Add(rte);
						m_routeIdToSelect = rte.Id;
					}
				}
				// refresh map and rebind the datagrid:
				PutOnMap();
				WaypointsCache.isDirty = true;
				//rebuildTracksTab();
				rebuildRoutesTab();
				tabControl1.SelectedTab = this.routesTabPage;
				prevSelectedTab = this.routesTabPage;

			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a route to reverse";
				Project.ShowPopup(toRteButton, "\nselect track(s) to convert to route(s)" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void setRteColorButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(routesDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();

				bool haveColor = false;
				int column = 0;
				DataTable table = m_routesDS.Tables[0];
				for(int i=0; i < selected.Count; i++)
				{
					int row = (int)selected[i];
					long routeId = (long)((int)routesDataGrid[row, column]);

					Track rte = WaypointsCache.getTrackById(routeId);
					if(rte != null)
					{
						if(!haveColor)
						{
							if(rte.isPresetColor)
							{
								paletteColorDialog.Color = rte.presetColor;
							}
							if(this.paletteColorDialog.ShowDialog() == DialogResult.OK)
							{
								haveColor = true;
							}
							else
							{
								break;
							}
						}
						rte.setColor(paletteColorDialog.Color);
						WaypointsCache.isDirty = true;
					}
				}
				// refresh map and rebind the datagrid:
				rebuildRoutesTab();
				m_pictureManager.Refresh();
			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a route to set color";
				Project.ShowPopup(setRteColorButton, "\nselect route(s) to set color" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void setTrkColorButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(tracksDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();

				bool haveColor = false;
				int column = 0;
				DataTable table = m_tracksDS.Tables[0];
				for(int i=0; i < selected.Count; i++)
				{
					int row = (int)selected[i];
					long trackId = (long)((int)tracksDataGrid[row, column]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						if(!haveColor)
						{
							if(trk.isPresetColor)
							{
								paletteColorDialog.Color = trk.presetColor;
							}
							if(this.paletteColorDialog.ShowDialog() == DialogResult.OK)
							{
								haveColor = true;
							}
							else
							{
								break;
							}
						}
						trk.setColor(paletteColorDialog.Color);
						WaypointsCache.isDirty = true;
					}
				}
				// refresh map and rebind the datagrid:
				rebuildTracksTab();
				m_pictureManager.Refresh();
			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a track to set color";
				Project.ShowPopup(setTrkColorButton, "\nselect track(s) to set color" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void reverseRteButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(routesDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = m_routesDS.Tables[0];
				for(int i=0; i < selected.Count; i++)
				{
					int row = (int)selected[i];
					long routeId = (long)((int)routesDataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + routeId + " ";
#endif
					Track rte = WaypointsCache.getTrackById(routeId);
					if(rte != null)
					{
						rte.reverse();
					}
				}
				// refresh and rebind the datagrid:
				rebuildRoutesTab();
				PutOnMap();
				WaypointsCache.isDirty = true;
			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a route to reverse";
				Project.ShowPopup(reverseRteButton, "\nselect route(s) to reverse" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void joinRteButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(routesDataGrid);
			if(selected.Count >= 2)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = m_routesDS.Tables[0];
				ArrayList routes = new ArrayList();		// of Track
				for(int i=0; i < selected.Count; i++)
				{
					int row = (int)selected[i];
					long routeId = (long)((int)routesDataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + routeId + " ";
#endif
					Track rte = WaypointsCache.getTrackById(routeId);
					if(rte != null)
					{
						routes.Add(rte);
						// hide the original routes because the joint route will cover them anyway:
						rte.Enabled = false;
					}
				}
				//messageTrkLabel.Text = "selected: " + str;
				Track joinedRoute = WaypointsCache.groupTracks(routes);
				joinedRoute.Enabled = true;
				m_routeIdToSelect = joinedRoute.Id;
				// refresh and rebind the datagrid:
				rebuildRoutesTab();
				PutOnMap();
				WaypointsCache.isDirty = true;
			}
			else
			{
				//messageTrkLabel.Text = "Error: select at least two routes to join";
				Project.ShowPopup(joinRteButton, "\nselect at least two routes to join" + Project.SELECT_HELP, Point.Empty);
			}
		}

		static bool warnedSanityFilter = false;

		private void ungroupButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(tracksDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				//messageTrkLabel.Text = "";
				int row = (int)selected[0];
				int column = 0;
				DataTable table = m_tracksDS.Tables[0];
				long trackId = (long)((int)tracksDataGrid[row, column]);
				Track trk = WaypointsCache.getTrackById(trackId);
				if(trk != null)
				{
					Cursor.Current = Cursors.WaitCursor;
					//messageTrkLabel.Text = "trk:" + trackId + " ungrouping...";

					// the Project.sanityFilter flag will be used in the following method:
					ArrayList trkIds = WaypointsCache.ungroupTrack(trk, Project.breakTimeMinutes);
					if(trkIds != null)
					{
						trk.Enabled = false;
						m_trackIdToSelect = (long)trkIds[0];
						// refresh and rebind the datagrid:
						rebuildTracksTab();
						PutOnMap();
						WaypointsCache.isDirty = true;

						Cursor.Current = Cursors.Default;
						if(!warnedSanityFilter && Project.sanityFilter)
						{
							warnedSanityFilter = true;
							string msg =  "Sanity Filter is activated and has been applied. Use Options tab to turn it off.";
							//messageTrkLabel.Text = msg;
							Project.MessageBox(this, msg);
						}
					}
					else
					{
						Cursor.Current = Cursors.Default;
						string msg =  "selected track " + trackId + " is one trip";
						Project.ShowPopup(ungroupButton, msg, Point.Empty);
					}
				}
			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a track first";
				Project.ShowPopup(ungroupButton, "\nselect a track first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void ungroupHereButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(trackpointsDataGrid);
			if(selected.Count > 0)
			{
				try
				{
					//int columnWptId = 0;
					int columnTrackId = 1;
					int columnTime = 4;
					DataTable table = m_trackpointsDS.Tables[0];
					int row = (int)selected[0];
					//int waypointId = (int)trackpointsDataGrid[row, columnWptId];
					long trackId = (long)((int)trackpointsDataGrid[row, columnTrackId]);
					SortableDateTime sdt = (SortableDateTime)trackpointsDataGrid[row, columnTime];
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null && sdt != null)
					{
						Waypoint wpt = (Waypoint)trk.Trackpoints[sdt.DateTime];		// wpt.DateTime is a key there
						if(wpt != null)
						{
							Cursor.Current = Cursors.WaitCursor;
							ArrayList trkIds = WaypointsCache.ungroupTrackAtTrackpoint(trk, wpt);
							if(trkIds != null)
							{
								trk.Enabled = false;
								m_trackIdToSelect = (long)trkIds[0];
								// refresh and rebind the datagrid:
								tabControl1.SelectedTab = (prevSelectedTab == this.tracksTabPage) ? this.tracksTabPage : this.routesTabPage;
								PutOnMap();
								WaypointsCache.isDirty = true;
								Cursor.Current = Cursors.Default;
							}
							else
							{
								Cursor.Current = Cursors.Default;
								string msg =  "selected track " + trackId + " cannot be broken into trips";
								Project.ShowPopup(ungroupHereButton, msg, Point.Empty);
							}
						}
					}
				}
				catch (Exception exc)
				{
					LibSys.StatusBar.Error("ungroupHereButton: " + exc.Message);
					messageTrkptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				//messageTrkLabel.Text = "Error: select a track first";
				Project.ShowPopup(ungroupHereButton, "\nselect a trackpoint - the track will be\nbroken into two trips at selected point" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void trackpoints_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e) 
		{
			if (!rebuildingTrackpoints && !deletingTrackpoints) 
			{
				try 
				{
					int columnTrackId = 1;
					int columnTime = 4;
					DataTable table = m_trackpointsDS.Tables[0];
					long trackId = (long)((int)e.Row[columnTrackId]);
					SortableDateTime sdt = (SortableDateTime)e.Row[columnTime];
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null && sdt != null)
					{
						trk.Trackpoints.Remove(sdt.DateTime);		// wpt.DateTime is a key there
						WaypointsCache.isDirty = true;
					}
				} 
				catch {}
				finally
				{
					PutOnMap();	// will also rename all trackpoints in tracks when Paint is invoked on the map
					PictureManager.This.PictureBox.Invoke(new MethodInvoker(PictureManager.This.PictureBox.Refresh));	// invoke Paint
					Thread.Sleep(1);
					string msg = "Warning: all trackpoints have been reordered and assigned new numbers";
					Project.ShowPopup(deleteTrkButton, msg, Point.Empty);
					m_trackIdToSelect = -1;
					// refresh and rebind the track boundaries (tracks will be rebuild on tab change):
					ArrayList selectedTracksRows = GetSelectedRows(tracksDataGrid);
					rebuildTracksBoundaries(selectedTracksRows);
				}
			}
		}

		private bool deletingTrackpoints = false;

		private void deleteTrackpointButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(trackpointsDataGrid);
			if(selected.Count > 0)
			{
				deletingTrackpoints = true;
				ClearMessages();
				try
				{
					//int columnWptId = 0;
					int columnTrackId = 1;
					int columnTime = 4;
					DataTable table = m_trackpointsDS.Tables[0];
					foreach(int row in selected)
					{
						//int waypointId = (int)trackpointsDataGrid[row, columnWptId];
						long trackId = (long)((int)trackpointsDataGrid[row, columnTrackId]);
						SortableDateTime sdt = (SortableDateTime)trackpointsDataGrid[row, columnTime];
						Track trk = WaypointsCache.getTrackById(trackId);
						if(trk != null && sdt != null)
						{
							trk.Trackpoints.Remove(sdt.DateTime);		// wpt.DateTime is a key there
							WaypointsCache.isDirty = true;
						}
					}
				}
				catch (Exception exc)
				{
					LibSys.StatusBar.Error("" + exc);
					messageTrkptLabel.Text = "Error: " + exc.Message;
				}
				finally
				{
					PutOnMap();	// will also rename all trackpoints in tracks when Paint is invoked on the map
					PictureManager.This.PictureBox.Invoke(new MethodInvoker(PictureManager.This.PictureBox.Refresh));	// invoke Paint
					Thread.Sleep(1);
					string msg = "Warning: all trackpoints have been reordered and assigned new numbers";
					Project.ShowPopup(deleteTrkButton, msg, Point.Empty);
					m_trackIdToSelect = -1;
					// refresh and rebind the trackpoints datagrid (tracks will be rebuild on tab change):
					rebuildTrackpointsTab2();
					deletingTrackpoints = false;
				}
			}		
			else
			{
				//messageTrkptLabel.Text = "Error: select trackpoints to delete";
				Project.ShowPopup(this.deleteTrkButton, "\nselect trackpoints to delete" + Project.SELECT_HELP, Point.Empty);
			}
		}

		// goto Trackpoint
		private void gotoTrkButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(trackpointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int column  = 0;
					int column1 = 1;
					DataTable table = m_trackpointsDS.Tables[0];
					int waypointId = (int)trackpointsDataGrid[row, column];
					long trackId = (long)((int)trackpointsDataGrid[row, column1]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						Waypoint wpt = trk.getTrackpointById(waypointId);
						//messageTrkptLabel.Text = "trk:" + trackId + " trkpt: " + wpt;
						GeoCoord newLoc = new GeoCoord(wpt.Location.Lng, wpt.Location.Lat, m_cameraManager.Location.Elev);
						m_cameraManager.Location = newLoc;
					}
				}
				catch (Exception exc)
				{
					messageTrkptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				messageTrkptLabel.Text = "Error: Select a trackpoint first";
				Project.ShowPopup(gotoTrkButton, "\nselect a trackpoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		#region printing track/route points and waypoints

		private void printTptButton_Click(object sender, System.EventArgs e)
		{
			ClearMessages();
			try
			{
				printFontSize = getPrintFontSize();
				printFont = new System.Drawing.Font("Arial", printFontSize);
				printFontBold = new System.Drawing.Font("Arial", printFontSize, FontStyle.Bold);
				printInterLineSpace = printFontSize * 0.75f;
				printFontSmall = new System.Drawing.Font("Arial", 6.0f);
				PrintPreviewDialog ppd = new PrintPreviewDialog();
				//PrintDialog ppd = new PrintDialog();
				//ppd.AllowSomePages = true;
				ppd.Document = printDocumentRteTrks;
				ppd.ShowDialog();
			}
			catch
			{
				Project.ErrorPrinterNotInstalled(this);
			}
		}
		
		private DataView m_dataView;
		private float printFontSize = 8.0f;

		private void printWptButton_Click(object sender, System.EventArgs e)
		{
			DataView selected = GetSelectedRowsDataView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					m_dataView = selected;

					printFontSize = getPrintFontSize();
					printFont = new System.Drawing.Font("Arial", printFontSize);
					printFontBold = new System.Drawing.Font("Arial", printFontSize, FontStyle.Bold);
					printInterLineSpace = printFontSize * 0.75f;
					printFontSmall = new System.Drawing.Font("Arial", 6.0f);
					PrintPreviewDialog ppd = new PrintPreviewDialog();
					//PrintDialog ppd = new PrintDialog();
					//ppd.AllowSomePages = true;
					ppd.Document = printDocumentWpts;
					ppd.ShowDialog();
				}
				catch
				{
					Project.ErrorPrinterNotInstalled(this);
				}
			}
			else
			{
				//messageWptLabel.Text = "Error: select a waypoint first";
				Project.ShowPopup(printWptButton, "\nselect waypoints to print" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private float getPrintFontSize()
		{
			float printFontSize;
			
			switch(Project.printTextFont)
			{
				default:
				case 0:
					printFontSize = 6.0f;
					break;
				case 1:
					printFontSize = 7.0f;
					break;
				case 2:
					printFontSize = 8.0f;
					break;
				case 3:
					printFontSize = 9.0f;
					break;
				case 4:
					printFontSize = 10.0f;
					break;
			}

			return printFontSize;
		}

		private Font printFont;
		private Font printFontBold;
		private Font printFontSmall;
		private int printLineCount;
		private int printPageCount;
		private float printInterLineSpace;
		private string printTrackName;
		private string printDate;

		private void printDocumentRteTrks_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			printLineCount = 0;
			printPageCount = 0;
			printTrackName = null;
			printDate = null;
		}

		private void printDocumentRteTrks_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			try
			{
				printPageCount++;

				float yPos = 0f;
				float xPos = 0f;
				int count = 0;

				float leftMargin = e.MarginBounds.Left;
				float rightMargin = e.MarginBounds.Right;
				float topMargin = e.MarginBounds.Top;
				float fontHeight = printFont.GetHeight(e.Graphics) + printInterLineSpace;
				float linesPerPage = e.MarginBounds.Height/fontHeight;

				int bgColorIntensity = 230;
				Brush bgBrush = new SolidBrush(Color.FromArgb(bgColorIntensity, bgColorIntensity, bgColorIntensity));

				DataTable table = m_trackpointsDS.Tables[0];
				int totalPages = (int)Math.Ceiling(table.Rows.Count / (linesPerPage - 3));		// 3 lines for header

				bool fromRoutes = this.prevSelectedTab == this.routesTabPage;

				if(fromRoutes)
				{
					string str = Project.PROGRAM_NAME_HUMAN + " --- Route printout ------ " + DateTime.Now + "      Page " + printPageCount + " of " + totalPages;
					yPos = topMargin + count * fontHeight;
					e.Graphics.DrawString(str, printFontSmall, Brushes.Black, leftMargin, yPos, new StringFormat());
					count++;

					float posName = 0.0f;
					float posLocation = 250.0f;
					float posHeading = 450.0f;
					float posLeg = 510.0f;
					float posOdometer = 580.0f;

					yPos = topMargin + count * fontHeight;
					str = "   Name";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posName, yPos, new StringFormat());
					str = "Location";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posLocation, yPos, new StringFormat());
					str = "Heading";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posHeading - 10, yPos, new StringFormat());
					str = "  Leg";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posLeg, yPos, new StringFormat());
					str = "Odometer";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posOdometer - 10, yPos, new StringFormat());
					count++;

					yPos = topMargin + count * fontHeight;
					e.Graphics.DrawLine(new Pen(Brushes.Black, 2.0f), new Point((int)leftMargin, (int)yPos), new Point((int)rightMargin, (int)yPos));
					count++;

					for (int i=printLineCount; count < linesPerPage && printLineCount < table.Rows.Count ;i++)
					{
						DataRow row = table.Rows[i];
						string trackName = "" + row["track"];
						if(printTrackName == null || !printTrackName.Equals(trackName))
						{
							if(printTrackName != null)
							{
								count++;
								str = "----------------------------------------------------------------";
								yPos = topMargin + count * fontHeight;
								e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin, yPos, new StringFormat());
								count++;
							}
							str = "Route: " + trackName;
							yPos = topMargin + count * fontHeight;
							e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin, yPos, new StringFormat());

							GeoCoord location = (GeoCoord)row["locationReal"];
							double variation = Math.Round(location.magneticVariation());
							string variationDir = variation > 0.0d ? "W" : "E";

							str = String.Format("(showing magnetic heading, variation {0:F0}{1})", Math.Abs(variation), variationDir);
							SizeF size = e.Graphics.MeasureString(str, printFont);
							int labelWidth = (int)Math.Round(size.Width); 
							e.Graphics.DrawString(str, printFont, Brushes.Black, rightMargin - labelWidth, yPos, new StringFormat());

							count++;
							printTrackName = trackName;
						}

						if(count % 2 == 0)
						{
							yPos = topMargin + count * fontHeight - 1;
							float h = fontHeight - 4;
							float w = posHeading - 10;
							e.Graphics.FillRectangle(bgBrush, leftMargin, yPos, w, h);
							w = (int)(rightMargin - leftMargin - posOdometer + 5);
							e.Graphics.FillRectangle(bgBrush, leftMargin + posOdometer - 5, yPos, w, h);
						}

						{
							yPos = topMargin + count * fontHeight + fontHeight / 2 - 3;
							float h = 2;
							float w = posOdometer - posHeading + 15;
							e.Graphics.FillRectangle(bgBrush, leftMargin + posHeading - 15, yPos, w, h);
						}

						int shifted = 0;
						foreach(DataColumn column in row.Table.Columns)
						{
							object cell = row[column.ColumnName];

							str = null;
							shifted = 0;
							int pos = -1;
							switch (column.ColumnName)
							{
								case "name":
									str = ("" + cell);
									if((pos=str.IndexOf("\n")) > 0)
									{
										str = str.Substring(0, pos-1);
									}
									str = str.Substring(0, Math.Min((int)(30.0f * 8.0f / printFontSize),str.Length));
									xPos = posName;
									break;

								case "location":
									str = "" + cell;
									if(str.EndsWith(" high"))
									{
										str = str.Substring(0, str.Length - " high".Length);
									}
									xPos = posLocation;
									break;

								case "headingReal":
								{
									double headingMagn = (double)cell;
									if(headingMagn == 999.0d)
									{
										str = "";
									}
									else
									{
										str = String.Format("{0:000}°", Math.Round(headingMagn));
									}
									xPos = posHeading;
									shifted = 1;
								}
									break;

								case "leg":
									if(!("" + cell).StartsWith("0 "))
									{
										str = "" + cell;
										xPos = posLeg;
										shifted = -1;
									}
									break;

								case "distance":
									str = "" + cell;
									xPos = posOdometer;
									break;
							}
							if(str != null)
							{
								yPos = topMargin + count * fontHeight + fontHeight * shifted / 2.0f;
								e.Graphics.DrawString(str, printFont, Brushes.Black, leftMargin + xPos, yPos, new StringFormat());
							}
						}
						count++;
						printLineCount++;
					}
				} 
				else	// not fromRoutes, track printout
				{
					string str = Project.PROGRAM_NAME_HUMAN + " --- Track printout ------ " + DateTime.Now + "      Page " + printPageCount + " of " + totalPages;
					yPos = topMargin + count * fontHeight;
					e.Graphics.DrawString(str, printFontSmall, Brushes.Black, leftMargin, yPos, new StringFormat());
					count++;

					float posName = 0.0f;
					float posLocation = 80.0f   * 8.0f / printFontSize;  //printFontComboBox.SelectedIndex == 0 ? 80.0f : 60.0f;
					float posHeading = 290.0f;
					float posSpeed = 350.0f;
					float posLeg = 410.0f;
					float posOdometer = 460.0f;
					float posTime = 530.0f;
					int toTruncateName = (int)(9  * 8.0f / printFontSize);

					yPos = topMargin + count * fontHeight;
					str = " Name";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posName, yPos, new StringFormat());
					str = "Location and Altitude";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posLocation, yPos, new StringFormat());
					str = "Heading";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posHeading - 10, yPos, new StringFormat());
					str = "Speed";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posSpeed, yPos, new StringFormat());
					str = "  Leg";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posLeg, yPos, new StringFormat());
					str = "Odometer";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posOdometer - 10, yPos, new StringFormat());
					str = "  Time";
					e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posTime, yPos, new StringFormat());
					count++;

					yPos = topMargin + count * fontHeight;
					e.Graphics.DrawLine(new Pen(Brushes.Black, 2.0f), new Point((int)leftMargin, (int)yPos), new Point((int)rightMargin, (int)yPos));
					count++;

					for (int i=printLineCount; count < linesPerPage
						&& printLineCount < table.Rows.Count ;i++)
					{
						DataRow row = table.Rows[i];
						string trackName = "" + row["track"];
						SortableDateTime sortableDateTime = (SortableDateTime)row["time"];
						DateTime dateTime = Project.zuluToLocal(sortableDateTime.DateTime);
						string date = dateTime.ToLongDateString();
						if(printTrackName == null || !printTrackName.Equals(trackName) || !date.Equals(printDate))
						{
							if(printTrackName != null)
							{
								count++;
								str = "----------------------------------------------------------------";
								yPos = topMargin + count * fontHeight;
								e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin, yPos, new StringFormat());
								count++;
							}

							str = "Track: " + trackName;
							yPos = topMargin + count * fontHeight;
							e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin, yPos, new StringFormat());

							GeoCoord location = (GeoCoord)row["locationReal"];
							double variation = Math.Round(location.magneticVariation());
							string variationDir = variation > 0.0d ? "W" : "E";

							str = String.Format("(showing magnetic heading, variation {0:F0}{1}", Math.Abs(variation), variationDir) + ")       " + date;
							SizeF size = e.Graphics.MeasureString(str, printFont);
							int labelWidth = (int)Math.Round(size.Width); 
							e.Graphics.DrawString(str, printFont, Brushes.Black, rightMargin - labelWidth, yPos, new StringFormat());

							count++;
							printTrackName = trackName;
							printDate = date;
						}

						if(count % 2 == 0)
						{
							yPos = topMargin + count * fontHeight - 1;
							float h = fontHeight - 4;
							float w = posHeading - 10;
							e.Graphics.FillRectangle(bgBrush, leftMargin, yPos, w, h);
							w = (int)(rightMargin - leftMargin - posOdometer + 5);
							e.Graphics.FillRectangle(bgBrush, leftMargin + posOdometer - 5, yPos, w, h);
						}

						{
							yPos = topMargin + count * fontHeight + fontHeight / 2 - 3;
							float h = 2;
							float w = posOdometer - posHeading + 15;
							e.Graphics.FillRectangle(bgBrush, leftMargin + posHeading - 15, yPos, w, h);
						}

						int shifted = 0;
						foreach(DataColumn column in row.Table.Columns)
						{
							object cell = row[column.ColumnName];

							str = null;
							shifted = 0;
							switch (column.ColumnName)
							{
								case "name":
									str = ("" + cell).Substring(0, Math.Min(toTruncateName,("" + cell).Length));
									xPos = posName;
									break;
								case "time":
									str = dateTime.ToLongTimeString();
									xPos = posTime;
									break;
								case "location":
									str = "" + cell;
									if(str.EndsWith(" high"))
									{
										str = str.Substring(0, str.Length - " high".Length);
									}
									xPos = posLocation;
									break;
								case "speed":
									str = "" + cell;
									if(str.Equals("n/a"))
									{
										str = null;
									} 
									else if(Project.printTextFont > 2 && str.EndsWith("/hr"))
									{
										str = str.Substring(0, str.Length - 3);
									}
									xPos = posSpeed;
									shifted = -1;
									break;
								case "headingReal":
								{
									double headingMagn = (double)cell;
									if(headingMagn == 999.0d)
									{
										str = "";
									}
									else
									{
										str = String.Format("{0:000}°", Math.Round(headingMagn));
									}
									xPos = posHeading;
									shifted = 1;
								}
									break;
								case "leg":
									str = "" + cell;
									if(str.StartsWith("0 "))
									{
										str = null;
									}
									xPos = posLeg;
									shifted = -1;
									break;
								case "distance":
									str = "" + cell;
									xPos = posOdometer;
									break;
							}
							if(str != null)
							{
								yPos = topMargin + count * fontHeight + fontHeight * shifted / 2.0f;
								e.Graphics.DrawString(str, printFont, Brushes.Black, leftMargin + xPos, yPos, new StringFormat());
							}
						}
						count++;
						printLineCount++;
					}
				}

				if (printLineCount < table.Rows.Count)
				{
					e.HasMorePages = true;
				}
			}
			catch (Exception exc)
			{
				messageTrkptLabel.Text = "Error: " + exc.Message;
				LibSys.StatusBar.Error("while printing track/route - " + exc.Message);
			}
		}

		
		private void printDocumentWpts_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			printLineCount = 0;
			printPageCount = 0;
			printTrackName = null;
		}

		private void printDocumentWpts_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			try
			{
				printPageCount++;

				float yPos = 0f;
				float xPos = 0f;
				int count = 0;

				float leftMargin = e.MarginBounds.Left;
				float rightMargin = e.MarginBounds.Right;
				float topMargin = e.MarginBounds.Top;
				float fontHeight = printFont.GetHeight(e.Graphics) + printInterLineSpace;
				float linesPerPage = e.MarginBounds.Height/fontHeight;

				int bgColorIntensity = 230;
				Brush bgBrush = new SolidBrush(Color.FromArgb(bgColorIntensity, bgColorIntensity, bgColorIntensity));

				int totalPages = (int)Math.Ceiling(m_dataView.Count / (linesPerPage - 3));		// 3 lines for header

				string str = Project.PROGRAM_NAME_HUMAN + " --- Waypoints printout ------ " + DateTime.Now + "      Page " + printPageCount + " of " + totalPages;
				yPos = topMargin;
				e.Graphics.DrawString(str, printFontSmall, Brushes.Black, leftMargin, yPos, new StringFormat());
				count++;
				yPos += fontHeight;

				float posWptName = 0.0f;
				float posUrlName = 90.0f;
				float posDesc = 270.0f;
				float posLocation = 500.0f;
				float posFound = 460.0f; //rightMargin - leftMargin - 40;

				string distanceLabel = wptPrintDistanceCheckBox.Checked ? "Distance / " : "";
				str = "   Name";
				e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posWptName, yPos, new StringFormat());
				str = "  " + distanceLabel + "URL Name / Description";
				e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posUrlName - 10, yPos, new StringFormat());
//				str = "  Description";
//				e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posDesc, yPos, new StringFormat());
				str = "Location";
				e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posLocation + 30, yPos, new StringFormat());
				str = "Found";
				e.Graphics.DrawString(str, printFontBold, Brushes.Black, leftMargin + posFound, yPos, new StringFormat());
				count++;
				yPos += fontHeight;

				e.Graphics.DrawLine(new Pen(Brushes.Black, 2.0f), new Point((int)leftMargin, (int)yPos), new Point((int)rightMargin, (int)yPos));
				count++;
				yPos += fontHeight;

				for (int i=printLineCount; count < linesPerPage && printLineCount < m_dataView.Count ;i++)
				{
					DataRowView row = m_dataView[i];

					if(count % 2 == 0)
					{
						float h = fontHeight - 4;
						float w = rightMargin - leftMargin + 10;
						e.Graphics.FillRectangle(bgBrush, leftMargin - 5, yPos - 1, w, h);
					}

					string wptname	= ("" + row["wptname"]);
					string urlname	= ("" + row["urlname"]);
					string desc		= ("" + row["desc"]);
					string comment	= ("" + row["comment"]);
					string time		= ("" + row["time"]);
					string location = ("" + row["location"]);
					string distance = ("" + row["distance"]);
					string sym		= ("" + row["sym"]);
					string found	= "yes".Equals("" + row["found"]) ? "***" : "";
					string source	= ("" + row["source"]);

					int toTruncate = (int)(70 * 8.0f / printFontSize);
					if(desc.StartsWith(urlname))
					{
						urlname = desc;
						desc = "";
					}
					else if(urlname.ToLower().Equals("cache detail"))
					{
						urlname = desc;
						desc = "";
					}
					else
					{
						urlname += (" / " + desc);
						desc = "";
					}

					if(wptPrintDistanceCheckBox.Checked)
					{
						urlname = distance + " - " + urlname;
					}

					foreach(DataColumn column in row.Row.Table.Columns)
					{
						str = null;
						switch (column.ColumnName)
						{
							case "wptname":
								str = wptname.Substring(0, Math.Min((int)(15.0f * 8.0f / printFontSize), wptname.Length));
								xPos = posWptName;
								break;
							case "urlname":
								str = urlname.Substring(0, Math.Min(toTruncate,urlname.Length));
								xPos = posUrlName;
								break;
							case "desc":
								str = desc.Substring(0, Math.Min(toTruncate,desc.Length));
								xPos = posDesc;
								break;
							case "location":
								str = location;
								xPos = posLocation;
								break;
							case "found":
								str = found;
								xPos = posFound + 20;
								break;
						}
						if(str != null)
						{
							e.Graphics.DrawString(str, printFont, Brushes.Black, leftMargin + xPos, yPos, new StringFormat());
						}
					}
					count++;
					yPos += fontHeight;
					printLineCount++;
				}

				if (printLineCount < m_dataView.Count)
				{
					e.HasMorePages = true;
				}
			}
			catch (Exception exc)
			{
				messageTrkptLabel.Text = "Error: " + exc.Message;
				LibSys.StatusBar.Error("while printing waypoints - " + exc.Message);
			}
		}

		#endregion

		private void editTptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(trackpointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int column  = 0;
					int column1 = 1;
					DataTable table = m_trackpointsDS.Tables[0];
					int waypointId = (int)trackpointsDataGrid[row, column];
					long trackId = (long)((int)trackpointsDataGrid[row, column1]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						Waypoint wpt = trk.getTrackpointById(waypointId);
						wpt.ForceShow = true;	// make sure label shows on the map, even if crowded
						m_pictureManager.Refresh();
						new DlgEditWaypoint(m_cameraManager, wpt).ShowDialog();
						PutOnMap();	// will also rename all trackpoints in tracks when Paint is invoked on the map
						PictureManager.This.PictureBox.Invoke(new MethodInvoker(PictureManager.This.PictureBox.Refresh));	// invoke Paint
						// refresh and rebind the trackpoints datagrid (tracks/routes will be rebuild on tab change):
						rebuildTrackpointsTab2();
					}
				}
				catch (Exception exc)
				{
					messageTrkptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				messageTrkptLabel.Text = "Error: Select a trackpoint first";
				Project.ShowPopup(gotoTrkButton, "\nselect a trackpoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void detailTptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(trackpointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int column  = 0;
					int column1 = 1;
					DataTable table = m_trackpointsDS.Tables[0];
					int waypointId = (int)trackpointsDataGrid[row, column];
					long trackId = (long)((int)trackpointsDataGrid[row, column1]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						Waypoint wpt = trk.getTrackpointById(waypointId);
						wpt.ForceShow = true;	// make sure label shows on the map, even if crowded
						m_pictureManager.Refresh();
						if(wpt.ThumbImage != null)
						{
							Project.mainCommand.photoViewerUp(wpt);
						}
						else
						{
							Project.InfoBox(this, wpt.DetailAny);
						}
					}
				}
				catch (Exception exc)
				{
					messageTrkptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				messageTrkptLabel.Text = "Error: Select a trackpoint first";
				Project.ShowPopup(gotoTrkButton, "\nselect a trackpoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void browseTptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(trackpointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int column  = 0;
					int column1 = 1;
					DataTable table = m_trackpointsDS.Tables[0];
					int waypointId = (int)trackpointsDataGrid[row, column];
					long trackId = (long)((int)trackpointsDataGrid[row, column1]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						Waypoint wpt = trk.getTrackpointById(waypointId);
						wpt.ForceShow = true;	// make sure label shows on the map, even if crowded
						m_pictureManager.Refresh();
						string tmp = wpt.Url;
						if(tmp != null && tmp.StartsWith("http://"))
						{
							tmp = "\"" + tmp + "\"";
							//LibSys.StatusBar.Trace(tmp);
							Project.RunBrowser(tmp);
						}
						else
						{
							Project.InfoBox(this, wpt.DetailAny);
						}
					}
				}
				catch (Exception exc)
				{
					messageTrkptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				messageTrkptLabel.Text = "Error: Select a trackpoint first";
				Project.ShowPopup(gotoTrkButton, "\nselect a trackpoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		// goto Waypoint
		private void gotoWptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					int waypointId = (int)waypointsDataGrid[row, columnWptId];
					Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
					messageWptLabel.Text = "waypoint: " + wpt;
					wpt.ForceShow = true;	// make sure label shows on the map, even if crowded
					GeoCoord newLoc = new GeoCoord(wpt.Location.Lng, wpt.Location.Lat, m_cameraManager.Location.Elev);
					m_cameraManager.Location = newLoc;
					// refresh and rebind the waypoints datagrid, so that distances are refreshed:
					rebuildWaypointsTab();
				}
				catch (Exception exc)
				{
					messageWptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				//messageWptLabel.Text = "Error: Select a waypoint first";
				Project.ShowPopup(gotoWptButton, "\nselect a waypoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void editWptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					int waypointId = (int)waypointsDataGrid[row, columnWptId];
					Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
					messageWptLabel.Text = "waypoint: " + wpt;
					wpt.ForceShow = true;	// make sure label shows on the map, even if crowded

					new DlgEditWaypoint(m_cameraManager, wpt).ShowDialog();

					// refresh and rebind the waypoints datagrid, so that data is refreshed:
					rebuildWaypointsTab();
				}
				catch (Exception exc)
				{
					messageWptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				//messageWptLabel.Text = "Error: Select a waypoint first";
				Project.ShowPopup(editWptButton, "\nselect a waypoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					int waypointId = (int)waypointsDataGrid[row, columnWptId];
					Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
					messageWptLabel.Text = "waypoint: " + wpt;
					wpt.ForceShow = true;	// make sure label shows on the map, even if crowded
					m_pictureManager.Refresh();
					string tmp = wpt.Url;
					if(tmp != null && tmp.StartsWith("http://"))
					{
						tmp = "\"" + tmp + "\"";
						//LibSys.StatusBar.Trace(tmp);
						Project.RunBrowser(tmp);
					}
					else
					{
						Project.InfoBox(this, wpt.DetailAny);
					}
				}
				catch (Exception exc)
				{
					messageWptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				//messageWptLabel.Text = "Error: Select a waypoint first";
				Project.ShowPopup(browseButton, "\nselect a waypoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void waypoints_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e) 
		{
			if (!rebuildingWaypoints && !deletingWaypoints) 
			{
				try 
				{
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					int waypointId = (int)e.Row[columnWptId];
					WaypointsCache.RemoveWaypointById(waypointId);
				} 
				catch (Exception ee)
				{
				}
				finally
				{
					PutOnMap();
					// refresh and rebind the waypoints datagrid:
					//rebuildWaypointsTab();
				}
			}
		}

		private void detailButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					int waypointId = (int)waypointsDataGrid[row, columnWptId];
					Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
					messageWptLabel.Text = "waypoint: " + wpt;
					Project.InfoBox(this, wpt.DetailAny);
				}
				catch (Exception exc)
				{
					messageWptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				//messageWptLabel.Text = "Error: Select a waypoint first";
				Project.ShowPopup(detailButton, "\nselect a waypoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private bool deletingWaypoints = false;

		private void deleteWptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				deletingWaypoints = true;
				ClearMessages();
				try
				{
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					ArrayList toDelete = new ArrayList();
					// make a list of wpts to be deleted without moving them in WaypointsAll[]:
					foreach(int row in selected)
					{
						int waypointId = (int)waypointsDataGrid[row, columnWptId];
						Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
						messageWptLabel.Text = "waypoint: " + wpt;
						toDelete.Add(wpt);
					}

					foreach(Waypoint wpt in toDelete)
					{
						WaypointsCache.WaypointsAll.Remove(wpt);
					}
				}
				catch (Exception exc)
				{
					LibSys.StatusBar.Error("" + exc);
					messageWptLabel.Text = "Error: " + exc.Message;
				}
				finally
				{
					PutOnMap();

					// refresh and rebind the waypoints datagrid:
					rebuildWaypointsTab();
					deletingWaypoints = false;
				}
			}		
			else
			{
				//messageWptLabel.Text = "Select waypoints to delete first";
				Project.ShowPopup(deleteWptButton, "\nselect a waypoint to delete" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void rebuildTracksBoundaries(ArrayList selectedRows)
		{
			int column = 0;
			foreach (int row in selectedRows)
			{
				long trackId = (long)((int)tracksDataGrid[row, column]);
				Track trk = WaypointsCache.getTrackById(trackId);
				if(trk != null)
				{
					trk.rebuildTrackBoundaries();
				}
			}
		}

		private void rebuildTracksTab()
		{
			tracksDataGrid.SuspendLayout();
			makeTracksDS();
			makeTracksDataGridStyle();
			tracksDataGrid.SetDataBinding(m_tracksDS, "tracks");
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[tracksDataGrid.DataSource, tracksDataGrid.DataMember];
			DataView dv = (DataView)cm.List;
			//dv.AllowEdit = false;
			dv.AllowNew = false;		// causes selecting by arrows and shift to throw exception when selection reaches end and you press arrow up.
			selectTrackRow();
			tracksDataGrid.ResumeLayout();
		}

		private void rebuildTrackpointsTab2()
		{
			if(prevSelectedTab == this.tracksTabPage)
			{
				m_trackIdToSelect = -1;
				ArrayList selectedTracksRows = GetSelectedRows(tracksDataGrid);
				rebuildTrackpointsTab(selectedTracksRows);
				rebuildTracksBoundaries(selectedTracksRows);
			}
			else
			{
				m_routeIdToSelect = -1;
				ArrayList selectedRoutesRows = GetSelectedRows(routesDataGrid);
				rebuildTrackpointsTab(selectedRoutesRows);
				rebuildTracksBoundaries(selectedRoutesRows);
			}
		}

		private void rebuildTrackpointsTab(ArrayList selectedTracksRows)
		{
			Cursor.Current = Cursors.WaitCursor;
			trackpointsDataGrid.SuspendLayout();
			makeTrackpointsDS(selectedTracksRows);
			makeTrackpointsDataGridStyle();
			//trackpointsDataGrid.SetDataBinding(m_trackpointsDS, "trackpoints");
			trackpointsDataGrid.DataSource = m_trackpointsDS.Tables["trackpoints"].DefaultView;
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[trackpointsDataGrid.DataSource, trackpointsDataGrid.DataMember];
			DataView dv = (DataView)cm.List;
			dv.AllowEdit = false;
			dv.AllowNew = false;		// causes selecting by arrows and shift to throw exception when selection reaches end and you press arrow up.
			trackpointsDataGrid.ResumeLayout();
			Cursor.Current = Cursors.Default;
		}

		Waypoint m_lastGraphWpt = null;

		public string descrById(long id)
		{
			string ret = "";

			Waypoint wpt = WaypointsCache.getTrackpointByWptId((int)id);
			m_lastGraphWpt = wpt;
			if(wpt != null)
			{
				string sTime;
				if(Project.useUtcTime)
				{
					DateTime time =  wpt.DateTime;
					string format = "HH:mm:ss";
					sTime = time.ToLongDateString() + " " + time.ToString(format) + " UTC";
				}
				else
				{
					DateTime time = Project.zuluToLocal(wpt.DateTime);
					sTime = time.ToLongDateString() + " " + time.ToLongTimeString();
				}

				ret = sTime + " -- " + wpt.graphInfoString();

				GeoCoord loc = new GeoCoord(wpt.Location.X, wpt.Location.Y, m_cameraManager.Elev);
				m_cameraManager.MarkLocation(loc, 0);
			}
			setZoomButtons();
			return ret;
		}

		public string zoomById(long id)
		{
			string ret = "";
			Waypoint wpt = WaypointsCache.getTrackpointByWptId((int)id);
			m_lastGraphWpt = wpt;
			if(wpt != null)
			{
				GeoCoord newLoc = new GeoCoord(wpt.Location.X, wpt.Location.Y, m_cameraManager.Elev);
				m_cameraManager.Location = newLoc;
				ret = "" + newLoc;
			}
			setZoomButtons();
			return ret;		// just info
		}

		public static string browseById(long id)
		{
			string ret = "";
			/*
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
			*/
			return ret;		// just info
		}

		private static StringById dBrowseById = new StringById(browseById); 
		private ArrayList m_selectedTracksRows = null;

		private void rebuildGraphTab()
		{
			if(m_selectedTracksRows != null)
			{
				rebuildGraphTab(m_selectedTracksRows);
			}
		}

		private void rebuildGraphTab(ArrayList selectedTracksRows)
		{
			Cursor.Current = Cursors.WaitCursor;

			makeTrackpointsDS(selectedTracksRows);

			double maxElev = -1000.0d;		// feet
			double maxSpeed = -1000.0d;		// miles per hour

			SortedList events = new SortedList();
			DataTable table = m_trackpointsDS.Tables["trackpoints"];
			foreach(DataRow row in table.Rows)
			{
				/*
				 * this is what the columns are:
				myDataRow["id"];
				myDataRow["trackid"];
				myDataRow["wptidx"];
				myDataRow["name"];
				myDataRow["time"] = new SortableDateTime(wpt.DateTime);
				myDataRow["location"] = wpt.Location.ToStringWithElev();

				myDataRow["speed"] = new Speed();
				myDataRow["heading"] = string;
				myDataRow["leg"] = Distance;
				myDataRow["track"] = string;
				myDataRow["source"] = string;
				myDataRow["displayed"] = string;
				*/

				DateTime time = ((SortableDateTime)row["time"]).DateTime;
				long id = (long)((int)row["id"]);

				double elev = 0.0d;		// feet
				try
				{
					elev = Math.Round((Double)row["elevation"] / Distance.METERS_PER_FOOT);	// * factor;
				} 
				catch {}

				double speed = 0.0d;	// miles per hour
				try
				{
					speed = Math.Round(((Speed)row["speed"]).Meters / Distance.METERS_PER_MILE);	// * factor;
				} 
				catch {}

				maxElev = Math.Max(elev, maxElev);
				maxSpeed = Math.Max(speed, maxSpeed);

				string url = "";
				string properties = (url.Length == 0) ? "" : "w";

				long[] ids = new long[2];
				ids[0] = id;
				ids[1] = id;
				double[] values = new double[2];
				values[0] = elev;
				values[1] = speed;
				string[] sValues = new string[2];
				sValues[0] = properties;
				sValues[1] = "";

				DatedEvent ev = new DatedEvent(time, values, sValues, ids, 2);

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

			if(maxElev > 0.0d && maxSpeed > 0.0d)
			{
				// make speed graph match the elevation:
				double speedFactor = 0.6d * maxElev / maxSpeed;
				foreach(DatedEvent ev in events.Values)
				{
					double[] values = ev.values(Project.FORMAT_TRACK_ELEVATION);
					values[1] = Math.Round(values[1] * speedFactor);
				}
			}

			string selHint = "Click into grey area to see selected interval";
			StringById dZoomById = new StringById(zoomById); 
			StringById dDescrById = new StringById(descrById);

			double rounder = 200.0d;
			if(this.m_elevMax > 10000.0d)
			{
				rounder = 2000.0d;
			}
			else if(this.m_elevMax > 1000.0d)
			{
				rounder = 1000.0d;
			}
			else if(this.m_elevMax <= 50.0d)
			{
				this.m_elevMax = 50.0d;
				rounder = 100.0d;
			}
			double graphGridMaxValue = Math.Ceiling(this.m_elevMax / (Distance.METERS_PER_FOOT * rounder)) * rounder;
			int steps = (int) (graphGridMaxValue / rounder);
			graphByTimeControl.MaxValueY = graphGridMaxValue;
			graphByTimeControl.MinTimeMargin = 1 * 60 * 1000;	// ms - 1 min
			graphByTimeControl.StepY = graphGridMaxValue / steps;

			graphByTimeControl.MarginLeft = 45;
			graphByTimeControl.MarginRight = 30;

			graphByTimeControl.initialHint = "Click on graph to see details. Use arrow keys. Drag mouse to select time interval, click into it to zoom.";

			graphByTimeControl.init(this, "", "", selHint, dDescrById, dZoomById, dBrowseById, new MethodInvoker(showSelected));
			graphByTimeControl.setGraphData(events, Project.FORMAT_TRACK_ELEVATION, 2, false);

			graphByTimeControl.resetLegends();
			graphByTimeControl.setLegend(0, "elevation");
			graphByTimeControl.setLegend(1, "speed");

			m_selectedTracksRows = selectedTracksRows;

			Cursor.Current = Cursors.Default;
		}

		private void showSelectedButton_Click(object sender, System.EventArgs e)
		{
			showSelected();
		}

		private void showSelected()
		{
			//scopeSelectedGraphRadioButton.Checked = true;
			graphByTimeControl.GraphSelectedArea();
			graphByTimeControl.Focus();
		}

		#region IEnableDisable implementation

		public void disable(int feature)
		{
			/*
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
			*/
		}

		public void enable(int feature)
		{
			/*
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
			*/
		}
		#endregion

		private void rebuildRoutesTab()
		{
			routesDataGrid.SuspendLayout();
			makeRoutesDS();
			makeRoutesDataGridStyle();
			routesDataGrid.SetDataBinding(m_routesDS, "routes");
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[routesDataGrid.DataSource, routesDataGrid.DataMember];
			DataView dv = (DataView)cm.List;
			//dv.AllowEdit = false;
			dv.AllowNew = false;		// causes selecting by arrows and shift to throw exception when selection reaches end and you press arrow up.
			selectRouteRow();
			routesDataGrid.ResumeLayout();
		}

		private void rebuildWaypointsTab()
		{
			Cursor.Current = Cursors.WaitCursor;
			waypointsDataGrid.SuspendLayout();
			makeWaypointsDS();
			makeWaypointsDataGridStyle();
			//waypointsDataGrid.SetDataBinding(m_waypointsDS, "waypoints");
			waypointsDataGrid.DataSource = m_waypointsDS.Tables["waypoints"].DefaultView;
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[waypointsDataGrid.DataSource, waypointsDataGrid.DataMember];
			DataView dv = (DataView)cm.List;
			dv.AllowEdit = false;
			dv.AllowNew = false;		// causes selecting by arrows and shift to throw exception when selection reaches end and you press arrow up.
			waypointsDataGrid.ResumeLayout();
			Cursor.Current = Cursors.Default;
		}

		private TabPage prevSelectedTab = null;

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.ClearPopup();
			ClearMessages();
			m_selectedTracksRows = null;
			m_cameraManager.removeMarkLocation();
			ArrayList selectedRows = GetSelectedRows(tracksDataGrid);
			if(selectedRows != null && selectedRows.Count > 0)
			{
				int column = 0;
				long trackId = (long)((int)tracksDataGrid[(int)selectedRows[0], column]);
				this.m_trackIdToSelect = trackId;
			}

			selectedRows = GetSelectedRows(routesDataGrid);
			if(selectedRows != null && selectedRows.Count > 0)
			{
				int column = 0;
				long routeId = (long)((int)routesDataGrid[(int)selectedRows[0], column]);
				this.m_routeIdToSelect = routeId;
			}

			if(tabControl1.SelectedTab == tracksTabPage)
			{
				rebuildTracksTab();
			}
			else if(tabControl1.SelectedTab == routesTabPage)
			{
				startRouteNumericUpDown.Value = Project.startRouteNumber;
				rebuildRoutesTab();
			}
			else if(tabControl1.SelectedTab == graphTabPage)
			{
				if(prevSelectedTab == this.tracksTabPage)
				{
					if(m_tracksTS == null)
					{
						rebuildTracksTab();
						tabControl1.SelectedTab = this.tracksTabPage;
					}
					selectedRows = GetSelectedRows(tracksDataGrid);
					if(selectedRows.Count > 0)
					{
						rebuildGraphTab(selectedRows);
					}
					else
					{
						tabControl1.SelectedTab = this.tracksTabPage;
						//messageTrkLabel.Text = "Error: select a track first";
						Project.ShowPopup(this.tracksDataGrid, "\nselect a track or a route first - then click on Graph tab" + Project.SELECT_HELP, Point.Empty);
					}
				}
			}
			else if(tabControl1.SelectedTab == trackpointsTabPage)
			{
				printFontComboBox.SelectedIndex = Project.printTextFont;
				if(prevSelectedTab == this.routesTabPage)
				{
					if(m_routesTS == null)
					{
						rebuildRoutesTab();
						tabControl1.SelectedTab = this.tracksTabPage;
					}
					selectedRows = GetSelectedRows(routesDataGrid);
					if(selectedRows.Count > 0)
					{
						rebuildTrackpointsTab(selectedRows);
					}
					else
					{
						tabControl1.SelectedTab = this.routesTabPage;
						//messageTrkLabel.Text = "Error: select a route first";
						Project.ShowPopup(this.routesDataGrid, "\nselect a route first - then click on Points tab" + Project.SELECT_HELP, Point.Empty);
					}
				} 
				else
				{
					if(m_tracksTS == null)
					{
						rebuildTracksTab();
						tabControl1.SelectedTab = this.tracksTabPage;
					}
					selectedRows = GetSelectedRows(tracksDataGrid);
					if(selectedRows.Count > 0)
					{
						rebuildTrackpointsTab(selectedRows);
					}
					else
					{
						tabControl1.SelectedTab = this.tracksTabPage;
						//messageTrkLabel.Text = "Error: select a track first";
						Project.ShowPopup(this.tracksDataGrid, "\nselect a track or a route first - then click on Points tab" + Project.SELECT_HELP, Point.Empty);
					}
				}
			}
			else if(tabControl1.SelectedTab == waypointsTabPage)
			{
				printFontComboBox1.SelectedIndex = Project.printTextFont;
				rebuildWaypointsTab();
			}
			else if(tabControl1.SelectedTab == filterTabPage)
			{
				timeFilterDefaultsComboBox.Items.Clear();
				timeFilterDefaultsComboBox.Items.Add("All Tracks");
				for (int i = 0; i < WaypointsCache.TracksAll.Count; i++)
				{
					Track track = (Track)WaypointsCache.TracksAll[i];
					if(!track.isRoute)
					{
						timeFilterDefaultsComboBox.Items.Add("" + track.Id + ": " + track.Name);
					}
				}
				timeFilterDefaultsComboBox.SelectedIndex = 0;
			}
			else if(tabControl1.SelectedTab == optionsTabPage)
			{
				waypointNameStyleComboBox.Items.Clear();
				waypointNameStyleComboBox.Items.AddRange(Project.waypointNameStyleChoices);
				waypointNameStyleComboBox.SelectedIndex = Project.waypointNameStyle;
				trackNameStyleComboBox.Items.Clear();
				trackNameStyleComboBox.Items.AddRange(Project.trackNameStyleChoices);
				trackNameStyleComboBox.SelectedIndex = Project.trackNameStyle;
				colorElevTracksCheckBox.Checked  = Project.trackElevColor;
				colorSpeedTracksCheckBox.Checked = Project.trackSpeedColor;
			}

			if(tabControl1.SelectedTab == this.tracksTabPage || tabControl1.SelectedTab == this.routesTabPage)
			{
				prevSelectedTab = tabControl1.SelectedTab;
			}
		}

		private void showNumbersCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.showTrackpointNumbers = showNumbersCheckBox.Checked;
			m_pictureManager.Refresh();
		}


		private void showNamesCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.showWaypointNames = showNamesCheckBox.Checked;
			m_pictureManager.Refresh();
		}

		private void breakTimeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			int indx = breakTimeComboBox.SelectedIndex;
			Project.breakTimeMinutes = breakTimes[indx];
		}

		/*
			"1  min", "2  min", "5  min", "10 min", "15 min", "20 min", "30 min", "45 min",
			"1 hour", "2 hours", "4 hours", "8 hours", "12 hours", "18 hours", "1 day"
		 */
		double[] breakTimes = new double[] {
											   1.0d, 2.0d, 5.0d, 10.0d, 15.0d, 20.0d, 30.0d, 45.0d,
											   60.0d, 120.0d, 240.0d, 480.0d, 720.0d, 1080.0d, 1440.0d
										   };

		private int breakTimeIndex(double timeMinutes)
		{
			int indx;
			for(indx=0; indx < breakTimes.Length ;indx++)
			{
				if(breakTimes[indx] >= timeMinutes)
				{
					break;
				}
			}
			return indx;
		}

		/*
			"10 sec", "30 sec", "1  min", "2  min", "5  min", "10 min", "15 min", "20 min", "30 min", "45 min",
			"1 hour", "2 hours", "4 hours", "8 hours", "12 hours", "18 hours", "1 day"
		 */
		double[] aroundTimes = new double[] {
												0.1666666666666666d, 0.5d, 1.0d, 2.0d, 5.0d, 10.0d, 15.0d, 20.0d,
												30.0d, 45.0d, 60.0d, 120.0d, 240.0d, 480.0d, 720.0d, 1080.0d, 1440.0d
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

		private void showWaypointsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(m_pictureManager != null) 
			{
				Cursor.Current = Cursors.WaitCursor;
				m_pictureManager.LayersManager.ShowWaypoints = showWaypointsCheckBox.Checked;
				Project.drawWaypoints = showWaypointsCheckBox.Checked;
				m_pictureManager.Refresh();
				Cursor.Current = Cursors.Default;
			}
		}

		private void showTrackpointsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			Project.drawTrackpoints = showTrackpointsCheckBox.Checked;
			m_pictureManager.Refresh();
			Cursor.Current = Cursors.Default;
		}

		private void importButton_Click(object sender, System.EventArgs e)
		{
			FileImportForm fileImportForm = new FileImportForm(false, FileImportForm.MODE_GPS, FileImportForm.TYPE_ANY);
			fileImportForm.ShowDialog();
			rebuildTracksTab();			// import may add tracks, we need to see new ones in the grid
			rebuildRoutesTab();			// import may add routes, we need to see new ones in the grid
			rebuildWaypointsTab();		// import may add waypoints, we need to see new ones in the grid
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
				messageFilterLabel.Text = "Time Filter: disabled";
				m_timeFilterMode = -1;
				m_pictureManager.Refresh();
			}
		}

		private void setFilterModeFromTo()
		{
			TimeFilter.Enabled = true;
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
			TimeFilter.Enabled = true;
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
			TimeFilter.Enabled = false;
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

		private void aroundTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private void aroundTimeTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private void aroundTimeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			aroundTimeMinutes = aroundTimes[aroundTimeComboBox.SelectedIndex];
			collectTimeFilterValues();
		}

		private void fromDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private void fromTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private void toDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private void toTimeDateTimePicker_ValueChanged(object sender, System.EventArgs e)
		{
			collectTimeFilterValues();
		}

		private bool inSet = false;

		private void collectTimeFilterValues()
		{
			if(!inSet)	// do nothing if this was caused by programmatically setting the values
			{
				string message = "Time filter:";
				switch(m_timeFilterMode)
				{
					case 0:		// from/to
					{
						TimeFilter.fromTime = Project.localToZulu(fromDateTimePicker.Value.Date + fromTimeDateTimePicker.Value.TimeOfDay);
						TimeFilter.toTime = Project.localToZulu(toDateTimePicker.Value.Date + toTimeDateTimePicker.Value.TimeOfDay);
						DateTime fromDT = Project.zuluToLocal(TimeFilter.fromTime);
						DateTime toDT = Project.zuluToLocal(TimeFilter.toTime);
						message += "\r\n    From: " + fromDT.ToLongDateString() + "  " + fromDT.ToLongTimeString() + "\r\n" + "        To: " + toDT.ToLongDateString() + "  " + toDT.ToLongTimeString();
					}
						break;
					case 1:		// around time
					{
						DateTime aroundTime = Project.localToZulu(aroundTimeDateTimePicker.Value.Date + aroundTimeTimeDateTimePicker.Value.TimeOfDay);
						TimeSpan ts = new TimeSpan(0, (int)aroundTimeMinutes, 0);
						TimeFilter.fromTime = aroundTime - ts;
						TimeFilter.toTime = aroundTime + ts;
						DateTime fromDT = Project.zuluToLocal(TimeFilter.fromTime);
						DateTime toDT = Project.zuluToLocal(TimeFilter.toTime);
						message += "\r\n    From: " + fromDT.ToLongDateString() + "  " + fromDT.ToLongTimeString() + "\r\n" + "        To: " + toDT.ToLongDateString() + "  " + toDT.ToLongTimeString();
					}
						break;
					default:
						message += " disabled";
						break;
				}
				if(!playing)
				{
					m_pictureManager.Refresh();
				}
				messageFilterLabel.Text = message;
			}
		}

		private void SetTimeFilterDefaults()
		{
			inSet = true;
			int tmp = 0;

			if(!TimeFilter.fromTime.Equals(DateTime.MaxValue))
			{
				fromDateTimePicker.Value = Project.zuluToLocal(TimeFilter.fromTime);
				fromTimeDateTimePicker.Value = Project.zuluToLocal(TimeFilter.fromTime);
				tmp++;
			}
			else
			{
				try
				{
					fromDateTimePicker.Value = Project.zuluToLocal(WaypointsCache.getMinTime());
					fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
					tmp++;
				} 
				catch {}
			}

			if(!TimeFilter.toTime.Equals(DateTime.MinValue))
			{
				toDateTimePicker.Value = Project.zuluToLocal(TimeFilter.toTime);
				toTimeDateTimePicker.Value = Project.zuluToLocal(TimeFilter.toTime);
				tmp++;
			}
			else
			{
				try
				{
					toDateTimePicker.Value = Project.zuluToLocal(WaypointsCache.getMaxTime());
					toTimeDateTimePicker.Value = toDateTimePicker.Value;
					tmp++;
				} 
				catch {}
			}

			if(tmp == 2)
			{
				try
				{
					aroundTimeDateTimePicker.Value = fromDateTimePicker.Value;
					aroundTimeTimeDateTimePicker.Value = fromDateTimePicker.Value;
				} 
				catch {}
			}

			inSet = false;
			//messageFilterLabel.Text = "Zulu From: " + TimeFilter.fromTime + "\r\n" + "   To: " + TimeFilter.toTime;
		}

		private bool playing = false;
		private Thread m_playThread = null;
		private int playSleepTimeMs = 500;

		private void playButton_Click(object sender, System.EventArgs e)
		{
			ClearMessages();
			m_playThread = new Thread(new ThreadStart(Play));
			// see Entry.cs for how the current culture is set:
			m_playThread.CurrentCulture = Thread.CurrentThread.CurrentCulture; //new CultureInfo("en-US", false);
			m_playThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture; //new CultureInfo("en-US", false);
			m_playThread.Name = "Track play";
			m_playThread.Start();
			stopButton.Enabled = true;
			playButton.Enabled = false;
		}

		private void stopButton_Click(object sender, System.EventArgs e)
		{
			playing = false;
			playButton.Enabled = true;
			stopButton.Enabled = false;
		}

		private void Play()
		{
			Track.lastHighlightedWaypoint = null;
			playing = true;

			while(playing)
			{
				switch(m_timeFilterMode)
				{
					default:
						playing = false;
						return;
					case 1:		// around time
						inSet = true;
						DateTime aroundTime = aroundTimeDateTimePicker.Value.Date + aroundTimeTimeDateTimePicker.Value.TimeOfDay;
						aroundTime = aroundTime.AddMinutes(aroundTimes[0]);
						aroundTimeDateTimePicker.Value = aroundTime;
						aroundTimeTimeDateTimePicker.Value = aroundTime;
						//TimeSpan ts = new TimeSpan(0, (int)aroundTimeMinutes, 0);
						//TimeFilter.fromTime = aroundTime - ts;
						//TimeFilter.toTime = aroundTime + ts;

						//fromDateTimePicker.Value = Project.zuluToLocal(TimeFilter.fromTime);
						//fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
						//toDateTimePicker.Value = Project.zuluToLocal(TimeFilter.toTime);
						//toTimeDateTimePicker.Value = toDateTimePicker.Value;
						inSet = false;

						collectTimeFilterValues();

						//messageFilterLabel.Text = "From: " + TimeFilter.fromTime + "\r\n" + "   To: " + TimeFilter.toTime;

						//Track.lastHighlightedWaypoint = null;
						//
						// given that Refresh is asynchronous, we can only hope to have lastHighlightedWaypoint
						// somewhat updated from the previous cycles. This kind of works though.
						//
						m_pictureManager.Refresh();
						if(Track.lastHighlightedWaypoint != null)
						{
							m_cameraManager.keepInView(Track.lastHighlightedWaypoint.Location);
						}
						break;
				}
				Thread.Sleep(playSleepTimeMs);
			}
			ClearMessages();
		}

		#region DataGrid and DataSet formation - for tracks, routes, trackpoints and waypoints

		#region Tracks
		private void makeTracksDataGridStyle()
		{
			if(m_tracksTS != null)
			{
				return;
			}

			//STEP 1: Create a DataTable style object and set properties if required. 
			m_tracksTS = new DataGridTableStyle(); 
			//specify the table from dataset (required step) 
			m_tracksTS.MappingName = "tracks"; 
			// Set other properties (optional step) 
			//m_tracksTS.AlternatingBackColor = Color.LightBlue; 

			int colCount = 0;

			//STEP 1: Create an int column style and add it to the tablestyle 
			//this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[m_tracksDS, "tracks"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 30; 
			m_tracksTS.GridColumnStyles.Add(csIDInt); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle NameCol = new DataGridTextBoxColumn(); 
			NameCol.MappingName = "name"; //from dataset table 
			NameCol.HeaderText = "Name"; 
			NameCol.Width = 150; 
			m_tracksTS.GridColumnStyles.Add(NameCol); 
			colCount++;

			//STEP 3: Add the checkbox 
			DataGridColumnStyle boolCol = new MyDataGridBoolColumn(colCount); 
			boolCol.MappingName = "displayed"; 
			boolCol.HeaderText = "Shown"; 
			//hook the new event to our handler in the grid
			((MyDataGridBoolColumn)boolCol).BoolValueChanged += new BoolValueChangedEventHandler(HandleTrackShowChanges);
			//uncomment this line to get a two-state checkbox 
			((DataGridBoolColumn)boolCol).AllowNull = false; 
			boolCol.Width = 60; 
			m_tracksTS.GridColumnStyles.Add(boolCol); 
			colCount++;

			//STEP 4: Create an int column style and add it to the tablestyle 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csLegsInt = new DataGridDigitsTextBoxColumn(pdc["legs"], "i", true); 
			csLegsInt.MappingName = "legs"; 
			csLegsInt.HeaderText = "Legs"; 
			csLegsInt.Width = 50; 
			m_tracksTS.GridColumnStyles.Add(csLegsInt); 
			colCount++;

			//STEP 5: Create a string column and add it to the tablestyle 
			DataGridColumnStyle StartCol = new DataGridTextBoxColumn(); 
			StartCol.MappingName = "start"; //from dataset table 
			StartCol.HeaderText = "Start"; 
			StartCol.Width = 170; 
			m_tracksTS.GridColumnStyles.Add(StartCol); 
			colCount++;

			//STEP 6: Create a string column and add it to the tablestyle 
			DataGridColumnStyle EndCol = new DataGridTextBoxColumn(); 
			EndCol.MappingName = "end"; //from dataset table 
			EndCol.HeaderText = "End"; 
			EndCol.Width = 170; 
			m_tracksTS.GridColumnStyles.Add(EndCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle DistanceCol = new DataGridTextBoxColumn(); 
			DistanceCol.MappingName = "distance";	//from dataset table 
			DistanceCol.HeaderText = "";			// won't fit in 80 pixels
			DistanceCol.Width = 50;
			m_tracksTS.GridColumnStyles.Add(DistanceCol);
			colCount++;

			//STEP 7: Create a string column and add it to the tablestyle 
			DataGridColumnStyle SourceCol = new DataGridTextBoxColumn(); 
			SourceCol.MappingName = "source"; //from dataset table 
			SourceCol.HeaderText = "Info"; 
			SourceCol.Width = 800; 
			m_tracksTS.GridColumnStyles.Add(SourceCol); 
			colCount++;

			tracksDataGrid.CaptionVisible = false;

			//STEP 8: Add the tablestyle to your datagrid's tablestlye collection:
			tracksDataGrid.TableStyles.Add(m_tracksTS);

			/* how to test for checked checkboxes:
			if((bool)tracksDataGrid[row, column]) 
				MessageBox.Show("I am true"); 
			else 
				MessageBox.Show("I am false");
			*/
		}

		private void makeTracksDS()
		{
			DataTable tracksTable;

			rebuildingTracks = true;

			// we cannot reuse the table, as boolean "Show" checkboxes will otherwise have their events messed up badly.
			if(m_tracksDS != null)
			{
				m_tracksDS.Dispose();
			}

			tracksTable = new DataTable("tracks");

			DataColumn myDataColumn;

			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "id";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			tracksTable.Columns.Add(myDataColumn);

			// Create name column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Name";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tracksTable.Columns.Add(myDataColumn);

			// Create distance traveled column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = (new Distance(0.0d)).GetType();
			myDataColumn.ColumnName = "distance";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Distance";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			tracksTable.Columns.Add(myDataColumn);
 
			// Create source column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "source";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Source";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			tracksTable.Columns.Add(myDataColumn);

			// Create "legs" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "legs";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Legs";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			tracksTable.Columns.Add(myDataColumn);

			// Create start column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = sortableDateTime.GetType();
			myDataColumn.ColumnName = "start";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Start";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			tracksTable.Columns.Add(myDataColumn);

			// Create end column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = sortableDateTime.GetType();
			myDataColumn.ColumnName = "end";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "End";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			tracksTable.Columns.Add(myDataColumn);

			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "displayed";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Display";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tracksTable.Columns.Add(myDataColumn);

			// Make the ID column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = tracksTable.Columns["id"];
			tracksTable.PrimaryKey = PrimaryKeyColumns;

			// Add the new DataTable to the DataSet.
			m_tracksDS = new DataSet();
			m_tracksDS.Tables.Add(tracksTable);
 
			tracksTable = m_tracksDS.Tables[0];
			tracksTable.ColumnChanging -= new DataColumnChangeEventHandler(this.tracks_ColumnChanging);
			tracksTable.RowDeleting -= new DataRowChangeEventHandler(this.tracks_RowDeleting);

			// Create DataRow objects and add them to the DataTable
			DataRow myDataRow;
			for (int i = 0; i < WaypointsCache.TracksAll.Count; i++)
			{
				Track track = (Track)WaypointsCache.TracksAll[i];
				if(!track.isRoute)
				{
					myDataRow = tracksTable.NewRow();
					myDataRow["id"] = track.Id;
					
					Speed dMaxSpeed = new Speed(track.SpeedMax);		// meters per hour
					Speed dMinSpeed = new Speed(track.SpeedMin);		// meters per hour
					Distance dOdometer = new Distance(track.Odometer);	// meters

					myDataRow["name"] = track.Name;
					string sSpeed = dMinSpeed.ToString() + " to " + dMaxSpeed.ToString();
					if(!dMinSpeed.isSane && !dMaxSpeed.isSane)
					{
						sSpeed = "n/a";
					}
					myDataRow["source"] = "speed:" + sSpeed + "   source: " + track.Source;
					myDataRow["legs"] = track.Trackpoints.Count;
					myDataRow["start"] = new SortableDateTime(track.Start, DateTimeDisplayMode.ConvertToLocal);
					myDataRow["end"] = new SortableDateTime(track.End, DateTimeDisplayMode.ConvertToLocal);
					myDataRow["distance"] = dOdometer;
					myDataRow["displayed"] = track.Enabled;
					tracksTable.Rows.Add(myDataRow);
				}
			}

			tracksTable.ColumnChanging += new DataColumnChangeEventHandler(this.tracks_ColumnChanging);
			tracksTable.RowDeleting += new DataRowChangeEventHandler(this.tracks_RowDeleting);

			rebuildingTracks = false;
		}
		#endregion

		#region Routes
		private void makeRoutesDataGridStyle()
		{
			if(m_routesTS != null)
			{
				return;
			}

			//STEP 1: Create a DataTable style object and set properties if required. 
			m_routesTS = new DataGridTableStyle(); 
			//specify the table from dataset (required step) 
			m_routesTS.MappingName = "routes"; 
			// Set other properties (optional step) 
			//m_routesTS.AlternatingBackColor = Color.LightBlue; 

			int colCount = 0;

			//STEP 1: Create an int column style and add it to the tablestyle 
			//this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[m_routesDS, "routes"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 30; 
			m_routesTS.GridColumnStyles.Add(csIDInt); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle NameCol = new DataGridTextBoxColumn(); 
			NameCol.MappingName = "name"; //from dataset table 
			NameCol.HeaderText = "Name"; 
			NameCol.Width = 150; 
			m_routesTS.GridColumnStyles.Add(NameCol); 
			colCount++;

			//STEP 3: Add the checkbox 
			DataGridColumnStyle boolCol = new MyDataGridBoolColumn(colCount); 
			boolCol.MappingName = "displayed"; 
			boolCol.HeaderText = "Shown"; 
			//hook the new event to our handler in the grid
			((MyDataGridBoolColumn)boolCol).BoolValueChanged += new BoolValueChangedEventHandler(HandleRouteShowChanges);
			//uncomment this line to get a two-state checkbox 
			((DataGridBoolColumn)boolCol).AllowNull = false; 
			boolCol.Width = 60; 
			m_routesTS.GridColumnStyles.Add(boolCol); 
			colCount++;

			//STEP 4: Create an int column style and add it to the tablestyle 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csLegsInt = new DataGridDigitsTextBoxColumn(pdc["legs"], "i", true); 
			csLegsInt.MappingName = "legs"; 
			csLegsInt.HeaderText = "Legs"; 
			csLegsInt.Width = 50; 
			m_routesTS.GridColumnStyles.Add(csLegsInt); 
			colCount++;

			//STEP 7: Create a string column and add it to the tablestyle 
			DataGridColumnStyle SourceCol = new DataGridTextBoxColumn(); 
			SourceCol.MappingName = "source"; //from dataset table 
			SourceCol.HeaderText = "Info"; 
			SourceCol.Width = 800; 
			m_routesTS.GridColumnStyles.Add(SourceCol); 
			colCount++;

			routesDataGrid.CaptionVisible = false;

			//STEP 8: Add the tablestyle to your datagrid's tablestlye collection:
			routesDataGrid.TableStyles.Add(m_routesTS);

			/* how to test for checked checkboxes:
			if((bool)routesDataGrid[row, column]) 
				MessageBox.Show("I am true"); 
			else 
				MessageBox.Show("I am false");
			*/
		}

		private void makeRoutesDS()
		{
			DataTable routesTable;

			rebuildingRoutes = true;

			// we cannot reuse the table, as boolean "Show" checkboxes will otherwise have their events messed up badly.
			if(m_routesDS != null)
			{
				m_routesDS.Dispose();
			}

			routesTable = new DataTable("routes");

			DataColumn myDataColumn;

			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "id";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			routesTable.Columns.Add(myDataColumn);

			// Create name column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Name";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			routesTable.Columns.Add(myDataColumn);

			// Create source column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "source";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Source";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			routesTable.Columns.Add(myDataColumn);

			// Create "legs" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "legs";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Legs";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			routesTable.Columns.Add(myDataColumn);

			// Create start column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = sortableDateTime.GetType();
			myDataColumn.ColumnName = "start";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Start";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			routesTable.Columns.Add(myDataColumn);

			// Create end column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = sortableDateTime.GetType();
			myDataColumn.ColumnName = "end";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "End";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			routesTable.Columns.Add(myDataColumn);

			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "displayed";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Display";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			routesTable.Columns.Add(myDataColumn);

			// Make the ID column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = routesTable.Columns["id"];
			routesTable.PrimaryKey = PrimaryKeyColumns;

			// Add the new DataTable to the DataSet.
			m_routesDS = new DataSet();
			m_routesDS.Tables.Add(routesTable);
 
			routesTable = m_routesDS.Tables[0];
			// Create DataRow objects and add them to the DataTable
			DataRow myDataRow;
			for (int i = 0; i < WaypointsCache.TracksAll.Count; i++)
			{
				Track route = (Track)WaypointsCache.TracksAll[i];
				if(route.isRoute)
				{
					myDataRow = routesTable.NewRow();
					myDataRow["id"] = route.Id;

					Distance dOdometer = new Distance(route.Odometer);	// meters

					myDataRow["name"] = route.Name;
					myDataRow["source"] = "" + dOdometer.ToString() + "      source: " + route.Source;
					myDataRow["legs"] = route.Trackpoints.Count - 1;
					myDataRow["start"] = new SortableDateTime(route.Start, DateTimeDisplayMode.ConvertToLocal);
					myDataRow["end"] = new SortableDateTime(route.End, DateTimeDisplayMode.ConvertToLocal);
					myDataRow["displayed"] = route.Enabled;
					routesTable.Rows.Add(myDataRow);
				}
			}

			// now make sure the event handlers are in place:
			routesTable.ColumnChanging += new DataColumnChangeEventHandler(this.routes_ColumnChanging);
			routesTable.RowDeleting += new DataRowChangeEventHandler(this.routes_RowDeleting);

			rebuildingRoutes = false;
		}
		#endregion

		#region Trackpoints
		private void makeTrackpointsDataGridStyle()
		{
			bool fromRoutes = this.prevSelectedTab == this.routesTabPage;
			if(m_trackpointsTS == null)
			{
				//STEP 1: Create a DataTable style object and set properties if required. 
				m_trackpointsTS = new DataGridTableStyle(); 
				//specify the table from dataset (required step) 
				m_trackpointsTS.MappingName = "trackpoints";
				//m_trackpointsTS.RowHeadersVisible = false;
				// Set other properties (optional step) 
				//m_trackpointsTS.AlternatingBackColor = Color.LightBlue; 

				int colCount = 0;

				//STEP 1: Create an int column style and add it to the tablestyle 
				//this requires setting the format for the column through its property descriptor 
				PropertyDescriptorCollection pdc = this.BindingContext[m_trackpointsDS, "trackpoints"].GetItemProperties(); 
				//now created a formated column using the pdc 
				DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
				csIDInt.MappingName = "id"; 
				csIDInt.HeaderText = ""; 
				csIDInt.Width = 1;
				m_trackpointsTS.GridColumnStyles.Add(csIDInt); 
				colCount++;

				//STEP 1: Create an int column style and add it to the tablestyle 
				//now created a formated column using the pdc 
				DataGridDigitsTextBoxColumn csTrackIDInt = new DataGridDigitsTextBoxColumn(pdc["trackid"], "i", true); 
				csTrackIDInt.MappingName = "trackid"; 
				csTrackIDInt.HeaderText = "Track Id"; 
				csTrackIDInt.Width = 1;
				m_trackpointsTS.GridColumnStyles.Add(csTrackIDInt); 
				colCount++;

				//STEP 1: Create an int column style and add it to the tablestyle 
				//now created a formated column using the pdc 
				DataGridDigitsTextBoxColumn csWptIdxInt = new DataGridDigitsTextBoxColumn(pdc["wptidx"], "i", true); 
				csWptIdxInt.MappingName = "wptidx"; 
				csWptIdxInt.HeaderText = "Wpt Idx"; 
				csWptIdxInt.Width = 1;
				m_trackpointsTS.GridColumnStyles.Add(csWptIdxInt); 
				colCount++;

				//STEP 2: Create a string column and add it to the tablestyle 
				DataGridColumnStyle NameCol = new DataGridTextBoxColumn(); 
				NameCol.MappingName = "name"; //from dataset table 
				NameCol.HeaderText = "Name"; 
				NameCol.Width = 150; 
				m_trackpointsTS.GridColumnStyles.Add(NameCol); 
				colCount++;

				//STEP 5: Create a string column and add it to the tablestyle 
				DataGridColumnStyle TimeCol = new DataGridTextBoxColumn(); 
				TimeCol.MappingName = "time"; //from dataset table 
				TimeCol.HeaderText = "Time"; 
				TimeCol.Width = 170; 
				m_trackpointsTS.GridColumnStyles.Add(TimeCol); 
				colCount++;

				//STEP 2: Create a string column and add it to the tablestyle 
				DataGridColumnStyle LocationCol = new DataGridTextBoxColumn(); 
				LocationCol.MappingName = "location"; //from dataset table 
				LocationCol.HeaderText = "Location"; 
				LocationCol.Width = 180; 
				m_trackpointsTS.GridColumnStyles.Add(LocationCol); 
				colCount++;

				//STEP 3: Create a string column and add it to the tablestyle 
				DataGridColumnStyle SpeedCol = new DataGridTextBoxColumn(); 
				SpeedCol.MappingName = "speed"; //from dataset table 
				SpeedCol.HeaderText = "Speed";
				SpeedCol.Width = 80;
				m_trackpointsTS.GridColumnStyles.Add(SpeedCol);
				colCount++;

				//STEP 3: Create a string column and add it to the tablestyle 
				DataGridColumnStyle HeadingCol = new DataGridTextBoxColumn(); 
				HeadingCol.MappingName = "heading"; //from dataset table 
				HeadingCol.HeaderText = "heading";
				HeadingCol.Width = 80;
				m_trackpointsTS.GridColumnStyles.Add(HeadingCol);
				colCount++;

				//STEP 3: Create a string column and add it to the tablestyle 
				DataGridColumnStyle LegCol = new DataGridTextBoxColumn(); 
				LegCol.MappingName = "leg"; //from dataset table 
				LegCol.HeaderText = "Leg";
				LegCol.Width = 60;
				m_trackpointsTS.GridColumnStyles.Add(LegCol);
				colCount++;

				//STEP 3: Create a string column and add it to the tablestyle 
				DataGridColumnStyle DistanceCol = new DataGridTextBoxColumn(); 
				DistanceCol.MappingName = "distance"; //from dataset table 
				DistanceCol.HeaderText = "Odometer";
				DistanceCol.Width = 80;
				m_trackpointsTS.GridColumnStyles.Add(DistanceCol);
				colCount++;

				//STEP 4: Create a string column and add it to the tablestyle 
				DataGridColumnStyle TrackCol = new DataGridTextBoxColumn(); 
				TrackCol.MappingName = "track"; //from dataset table 
				TrackCol.HeaderText = "Track"; 
				TrackCol.Width = 150; 
				m_trackpointsTS.GridColumnStyles.Add(TrackCol); 
				colCount++;

				//STEP 5: Create a string column and add it to the tablestyle 
				DataGridColumnStyle SourceCol = new DataGridTextBoxColumn(); 
				SourceCol.MappingName = "source"; //from dataset table 
				SourceCol.HeaderText = "Source"; 
				SourceCol.Width = 800; 
				m_trackpointsTS.GridColumnStyles.Add(SourceCol); 
				colCount++;

				trackpointsDataGrid.CaptionVisible = false;

				//STEP 8: Add the tablestyle to your datagrid's tablestlye collection:
				trackpointsDataGrid.TableStyles.Add(m_trackpointsTS);
			}
			//m_trackpointsTS.GridColumnStyles[4].Width = fromRoutes ? 3 : 170;	// need to leave clickable area for sorting
			m_trackpointsTS.GridColumnStyles[6].Width = fromRoutes ? 1 : 80;
			m_trackpointsTS.GridColumnStyles[7].Width = fromRoutes ? 60 : 1;
			//m_trackpointsTS.GridColumnStyles[8].Width = fromRoutes ? 60 : 1;
			m_trackpointsTS.GridColumnStyles[10].HeaderText = fromRoutes ? "Route" : "Track";
		}

		private double m_elevMax = 0.0d;	// meters
		private double m_elevMin = 0.0d;

		private void makeTrackpointsDS(ArrayList selectedTracksRows)
		{
			DataTable trackpointsTable;
			
			rebuildingTrackpoints = true;

			string sortOrder = "";

			if(m_trackpointsDS != null)
			{
				sortOrder = m_trackpointsDS.Tables["trackpoints"].DefaultView.Sort;	// we will restore it later
				m_trackpointsDS.Tables.Clear();
				m_trackpointsDS.Dispose();
			}

			trackpointsTable = new DataTable("trackpoints");

			DataColumn myDataColumn;

			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "id";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			trackpointsTable.Columns.Add(myDataColumn);

			// needed to relate waypoint to track
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "trackid";
			myDataColumn.ReadOnly = true;
			trackpointsTable.Columns.Add(myDataColumn);

			// needed to index waypoint inside a track
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "wptidx";
			myDataColumn.ReadOnly = true;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create name column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Name";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create date/time column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = sortableDateTime.GetType();
			myDataColumn.ColumnName = "time";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Time";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create location column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "location";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Location";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create elevation column (for graph).
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "elevation";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Elevation";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create speed column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = (new Speed(0.0d)).GetType();
			myDataColumn.ColumnName = "speed";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Speed";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create heading column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "heading";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Heading";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create leg distance column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = (new Distance(0.0d)).GetType();
			myDataColumn.ColumnName = "leg";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Leg";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create distance traveled (odometer) column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = (new Distance(0.0d)).GetType();
			myDataColumn.ColumnName = "distance";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Distance";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create track column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "track";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Track";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create source column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "source";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Source";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "displayed";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Display";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create real location column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = GeoCoord.This.GetType();
			myDataColumn.ColumnName = "locationReal";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Create real heading column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "headingReal";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			trackpointsTable.Columns.Add(myDataColumn);

			// Make the ID column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = trackpointsTable.Columns["id"];
			trackpointsTable.PrimaryKey = PrimaryKeyColumns;

			// Add the new DataTable to the DataSet.
			m_trackpointsDS = new DataSet();
			m_trackpointsDS.Tables.Add(trackpointsTable);

			// Create DataRow objects and add them to the DataTable
			DataRow myDataRow;
			int column = 0;
			int count = 0;
			m_elevMax = 0.0d;	// meters
			m_elevMin = 0.0d;
			bool fromRoutes = this.prevSelectedTab == this.routesTabPage;
			this.messageTrkptLabel.Text = fromRoutes ? "Routes:" : "Tracks:";
			foreach (int row in selectedTracksRows)
			{
				long trackId = (long)((int)(fromRoutes ? routesDataGrid[row, column] : tracksDataGrid[row, column]));
				Track trk = WaypointsCache.getTrackById(trackId);
				Waypoint prevWpt = null;
				if(trk != null)
				{
					this.messageTrkptLabel.Text += (" " + trk.Name + " ");

					double odometer = 0.0d;	// meters

					for (int i = 0; i < trk.Trackpoints.Count; i++)
					{
						Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
						Waypoint nextWpt = (i < trk.Trackpoints.Count-1) ? (Waypoint)trk.Trackpoints.GetByIndex(i+1) : null;

						// wpt.Location.Lng, wpt.Location.Lat, wpt.Location.Elev
						if(wpt != null)
						{
							count++;
							myDataRow = trackpointsTable.NewRow();
							myDataRow["id"] = wpt.Id;
							myDataRow["trackid"] = trackId;
							myDataRow["wptidx"] = i;		// in track
							string contentOrUrl = (wpt.Desc != null && wpt.Desc.Length > 0) ? "*" : "";
							contentOrUrl += (wpt.Url != null && wpt.Url.Length > 0) ? "@" : "";
							string name = contentOrUrl;

							name += ((wpt.Name == null || wpt.Name.Length == 0) ? ("" + (i+1)) : wpt.Name);
							if(wpt.WptName.Length > 0 && !name.ToLower().StartsWith(wpt.WptName.ToLower()))		// avoid duplicates like "2: 2"
							{
								name += ": " + wpt.WptName;
							}
							myDataRow["name"] = name.Trim();
							myDataRow["time"] = new SortableDateTime(wpt.DateTime, DateTimeDisplayMode.ConvertToLocal);
							myDataRow["location"] = wpt.Location.ToStringWithElev();
							myDataRow["locationReal"] = new GeoCoord(wpt.Location);

							double elev = wpt.Location.Elev;
							myDataRow["elevation"] = elev;

							m_elevMax = Math.Max(m_elevMax, elev);
							m_elevMin = Math.Min(m_elevMin, elev);

							Speed speed = null;
							Distance leg = null;
							if(prevWpt != null)
							{
								// compute odometer and speed:
								TimeSpan dt = wpt.DateTime - prevWpt.DateTime;
								leg = wpt.Location.distanceFrom(prevWpt.Location);	// meters
								double legMeters = leg.Meters;
								odometer += legMeters;
								double legMetersPerHour = legMeters * 36000000000.0d / dt.Ticks;		// meters per hour
								if(!fromRoutes && legMetersPerHour > 1000.0d && legMetersPerHour < 330.0d * 3600.0d)	// sanity check - speed of sound, m/hr
								{
									wpt.Speed = (float)legMetersPerHour;
									if(wpt.Speed > trk.SpeedMax)
									{
										trk.SpeedMax = wpt.Speed;
									}
									if(wpt.Speed < trk.SpeedMin)
									{
										trk.SpeedMin = wpt.Speed;
									}
									speed = new Speed(legMetersPerHour);
								}
							}
							wpt.Odometer = (float)odometer;

							myDataRow["speed"] = speed == null ? new Speed(-1.0d) : speed;

							double variation = Math.Round(wpt.Location.magneticVariation());
							double headingTrue = (nextWpt == null || wpt.Location.sameAs(nextWpt.Location)) ? 999.0d : (wpt.Location.bearing(nextWpt.Location) * 180.0d / Math.PI);
							double headingMagn = (headingTrue == 999.0d) ? 999.0d : ((headingTrue + variation + 360.0d) % 360.0d);

							myDataRow["headingReal"] = headingMagn;
							myDataRow["heading"] = (headingTrue == 999.0d) ? "" : String.Format("{0:000}°", Math.Round(headingMagn));
							myDataRow["leg"] = leg == null ? new Distance(0.0d) : leg;
							myDataRow["distance"] = new Distance(odometer);
							myDataRow["track"] = trk.Name;
							myDataRow["source"] = wpt.Source;
							myDataRow["displayed"] = true;
							trackpointsTable.Rows.Add(myDataRow);
							prevWpt = wpt;
						}
					}
				}
			}
			this.messageTrkptLabel.Text += ("   - total " + count + " points");

			trackpointsTable.RowDeleting += new DataRowChangeEventHandler(this.trackpoints_RowDeleting);

			trackpointsTable.DefaultView.Sort = sortOrder;

			if(trackpointsTable.DefaultView.Sort.Length == 0)
			{
				trackpointsTable.DefaultView.Sort = "time";
			}

			rebuildingTrackpoints = false;
		}
		#endregion

		#region Waypoints
		private void makeWaypointsDataGridStyle()
		{
			if(m_waypointsTS != null)
			{
				return;
			}

			//STEP 1: Create a DataTable style object and set properties if required. 
			m_waypointsTS = new DataGridTableStyle(); 
			//specify the table from dataset (required step) 
			m_waypointsTS.MappingName = "waypoints"; 
			// Set other properties (optional step) 
			//m_waypointsTS.AlternatingBackColor = Color.LightBlue; 

			int colCount = 0;

			//STEP 1: Create an int column style and add it to the tablestyle 
			//this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[m_waypointsDS, "waypoints"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 1;
			m_waypointsTS.GridColumnStyles.Add(csIDInt); 
			colCount++;

			//STEP 7: Create a string column and add it to the tablestyle 
			SymbolCol = new DataGridTextBoxColumn(); 
			SymbolCol.MappingName = "sym"; //from dataset table 
			SymbolCol.HeaderText = "Symbol"; 
			SymbolCol.Width = Math.Max(10, Project.symbolColWidth); 
			m_waypointsTS.GridColumnStyles.Add(SymbolCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			NameCol = new DataGridTextBoxColumn(); 
			NameCol.MappingName = "wptname"; //from dataset table 
			NameCol.HeaderText = "Name"; 
			NameCol.Width = Math.Max(10, Project.nameColWidth); 
			m_waypointsTS.GridColumnStyles.Add(NameCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			UrlNameCol = new DataGridTextBoxColumn(); 
			UrlNameCol.MappingName = "urlname"; //from dataset table 
			UrlNameCol.HeaderText = "URL Name"; 
			UrlNameCol.Width = Math.Max(10, Project.urlNameColWidth); 
			m_waypointsTS.GridColumnStyles.Add(UrlNameCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DescCol = new DataGridTextBoxColumn(); 
			DescCol.MappingName = "desc"; //from dataset table 
			DescCol.HeaderText = "Description"; 
			DescCol.Width = Math.Max(10, Project.descColWidth); 
			m_waypointsTS.GridColumnStyles.Add(DescCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			CommentCol = new DataGridTextBoxColumn(); 
			CommentCol.MappingName = "comment"; //from dataset table 
			CommentCol.HeaderText = "Comment"; 
			CommentCol.Width = Math.Max(10, Project.commentColWidth); 
			m_waypointsTS.GridColumnStyles.Add(CommentCol); 
			colCount++;

			//STEP 5: Create a string column and add it to the tablestyle 
			DataGridColumnStyle TimeCol = new DataGridTextBoxColumn(); 
			TimeCol.MappingName = "time"; //from dataset table 
			TimeCol.HeaderText = "Time"; 
			TimeCol.Width = 1; 
			m_waypointsTS.GridColumnStyles.Add(TimeCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle LocationCol = new DataGridTextBoxColumn(); 
			LocationCol.MappingName = "location"; //from dataset table 
			LocationCol.HeaderText = "Location"; 
			LocationCol.Width = 180; 
			m_waypointsTS.GridColumnStyles.Add(LocationCol); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle DistanceCol = new DataGridTextBoxColumn(); 
			DistanceCol.MappingName = "distance"; //from dataset table 
			DistanceCol.HeaderText = "Distance from Camera";	// won't fit in 80 pixels
			DistanceCol.Width = 80;
			m_waypointsTS.GridColumnStyles.Add(DistanceCol);
			colCount++;

			/*
			//STEP 3: Add the checkbox 
			DataGridColumnStyle boolCol = new MyDataGridBoolColumn(colCount); 
			boolCol.MappingName = "displayed"; 
			boolCol.HeaderText = "Shown"; 
			//hook the new event to our handler in the grid
			//((MyDataGridBoolColumn)boolCol).BoolValueChanged += new BoolValueChangedEventHandler(HandleBoolChanges);
			//uncomment this line to get a two-state checkbox 
			((DataGridBoolColumn)boolCol).ReadOnly = true; 
			((DataGridBoolColumn)boolCol).AllowNull = false; 
			boolCol.Width = 55; 
			m_waypointsTS.GridColumnStyles.Add(boolCol); 
			colCount++;
			*/

			//STEP 2: Create a string column and add it to the tablestyle 
			DataGridColumnStyle FoundCol = new DataGridTextBoxColumn(); 
			FoundCol.MappingName = "found"; //from dataset table 
			FoundCol.HeaderText = "Found";
			FoundCol.Alignment = HorizontalAlignment.Center;
			FoundCol.Width = 45;
			m_waypointsTS.GridColumnStyles.Add(FoundCol);
			colCount++;

			//STEP 7: Create a string column and add it to the tablestyle 
			DataGridColumnStyle SourceCol = new DataGridTextBoxColumn(); 
			SourceCol.MappingName = "source"; //from dataset table 
			SourceCol.HeaderText = "Source"; 
			SourceCol.Width = 800; 
			m_waypointsTS.GridColumnStyles.Add(SourceCol); 
			colCount++;

			waypointsDataGrid.CaptionVisible = false;

			//STEP 8: Add the tablestyle to your datagrid's tablestlye collection:
			waypointsDataGrid.TableStyles.Add(m_waypointsTS);
		}

		private void makeWaypointsDS()
		{
			DataTable waypointsTable;
			
			rebuildingWaypoints = true;

			string sortOrder = "";

			if(m_waypointsDS != null)
			{
				sortOrder = m_waypointsDS.Tables["waypoints"].DefaultView.Sort;	// we will restore it later
				m_waypointsDS.Tables.Clear();
				m_waypointsDS.Dispose();
			}

			{
				waypointsTable = new DataTable("waypoints");

				DataColumn myDataColumn;
 
				// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.Int32");
				myDataColumn.ColumnName = "id";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = true;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create wptName column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "wptname";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Short Name";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create name column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "urlname";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Url Name";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create desc column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "desc";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Description";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create comment column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "comment";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Comment";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create date/time column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = sortableDateTime.GetType();
				myDataColumn.ColumnName = "time";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Time";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create location column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "location";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Location";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create distance from camera column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = (new Distance(0.0d)).GetType();
				myDataColumn.ColumnName = "distance";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Distance";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create symbol column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "sym";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Symbol";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create found column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "found";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Found";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Create source column.
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.String");
				myDataColumn.ColumnName = "source";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Source";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				myDataColumn = new DataColumn();
				myDataColumn.DataType = System.Type.GetType("System.Boolean");
				myDataColumn.ColumnName = "displayed";
				myDataColumn.AutoIncrement = false;
				myDataColumn.Caption = "Display";
				myDataColumn.ReadOnly = true;
				myDataColumn.Unique = false;
				waypointsTable.Columns.Add(myDataColumn);
 
				// Make the ID column the primary key column.
				DataColumn[] PrimaryKeyColumns = new DataColumn[1];
				PrimaryKeyColumns[0] = waypointsTable.Columns["id"];
				waypointsTable.PrimaryKey = PrimaryKeyColumns;
 
				// Add the new DataTable to the DataSet.
				m_waypointsDS = new DataSet();
				m_waypointsDS.Tables.Add(waypointsTable);
			}
 
			// Create DataRow objects and add them to the DataTable
			DataRow myDataRow;
			foreach (Waypoint wpt in WaypointsCache.WaypointsAll)
			{
				myDataRow = waypointsTable.NewRow();
				myDataRow["id"] = wpt.Id;
				myDataRow["wptname"] = wpt.WptName;
				myDataRow["urlname"] = wpt.UrlName;
				string desc = wpt.Desc.Replace("\n", " ").Replace("\r", "");
				desc= desc.Length > 150 ? (desc.Substring(0, 150) + "...") : desc;
				myDataRow["desc"] = desc;
				string comment = wpt.Comment.Replace("\n", " ").Replace("\r", "");
				comment= comment.Length > 150 ? (comment.Substring(0, 150) + "...") : comment;
				myDataRow["comment"] = comment;
				myDataRow["time"] = new SortableDateTime(wpt.DateTime, DateTimeDisplayMode.ConvertToLocal);
				myDataRow["location"] = wpt.Location.ToStringWithElev();
				myDataRow["distance"] = m_cameraManager.Location.distanceFrom(wpt.Location);
				myDataRow["sym"] = wpt.Sym;
				myDataRow["found"] = wpt.Found ? "yes" : "";
				myDataRow["source"] = wpt.Source;
				myDataRow["displayed"] = wpt.Enabled;
				waypointsTable.Rows.Add(myDataRow);
			}

			waypointsTable.RowDeleting += new DataRowChangeEventHandler(this.waypoints_RowDeleting);

			waypointsTable.DefaultView.Sort = sortOrder;

			if(waypointsTable.DefaultView.Sort.Length == 0)
			{
				waypointsTable.DefaultView.Sort = "distance";
			}

			rebuildingWaypoints = false;
		}
		#endregion

		#endregion

		private void waypointsDataGrid_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point pt = new Point(e.X, e.Y); 
			DataGrid.HitTestInfo hti = waypointsDataGrid.HitTest(pt); 
			if(hti.Type == DataGrid.HitTestType.Cell && !waypointsDataGrid.IsSelected(hti.Row)) 
			{ 
				//waypointsDataGrid.CurrentCell = new DataGridCell(hti.Row, hti.Column); 
				waypointsDataGrid.Select(hti.Row); 
				int column = 0;
				int waypointId = (int)waypointsDataGrid[hti.Row, column];
				Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
				messageWptLabel.Text = "waypoint: " + wpt;
				if(wpt != null)
				{
					GeoCoord loc = new GeoCoord(wpt.Location.X, wpt.Location.Y, m_cameraManager.Elev);
					m_cameraManager.MarkLocation(loc, 0);
				}
			}
		}

		private void trackpointsDataGrid_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point pt = new Point(e.X, e.Y); 
			DataGrid.HitTestInfo hti = trackpointsDataGrid.HitTest(pt); 
			if(hti.Type == DataGrid.HitTestType.Cell && !trackpointsDataGrid.IsSelected(hti.Row)) 
			{ 
				//trackpointsDataGrid.CurrentCell = new DataGridCell(hti.Row, hti.Column); 
				trackpointsDataGrid.Select(hti.Row); 
				int column = 0;
				int columnTrackId = 1;
				int trackpointId = (int)trackpointsDataGrid[hti.Row, column];
				long trackId = (long)((int)trackpointsDataGrid[hti.Row, columnTrackId]);
				Track trk = WaypointsCache.getTrackById(trackId);
				if(trk != null)
				{
					Waypoint wpt = trk.getTrackpointById(trackpointId);
					if(wpt != null)
					{
						GeoCoord loc = new GeoCoord(wpt.Location.X, wpt.Location.Y, m_cameraManager.Elev);
						m_cameraManager.MarkLocation(loc, 0);
					}
				}
			}
		}

		private void rewindButton_Click(object sender, System.EventArgs e)
		{
			timeFilterDefaultsButton_Click(sender, e);

			// provoke SetTimeFilterDefaults() to rewind to the beginning of the earliest track
			//TimeFilter.fromTime = DateTime.MaxValue;
			//TimeFilter.toTime = DateTime.MinValue;
			//SetTimeFilterDefaults();
			//collectTimeFilterValues();
		}

		private void wptDloadToGpsButton_Click(object sender, System.EventArgs e)
		{
			if(DlgGpsManager.isBusy)
			{
				Project.ShowPopup(wptDloadToGpsButton, "GPS Manager is still active and communicating to GPS", Point.Empty);
				DlgGpsManager.BringFormUp();
				return;
			}
			if(DlgGpsManager.isUp)
			{
				DlgGpsManager.CloseGently();
			}

			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					ArrayList toDownload = new ArrayList();
					foreach(int row in selected)
					{
						int waypointId = (int)waypointsDataGrid[row, columnWptId];
						Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
						messageWptLabel.Text = "waypoint: " + wpt;
						toDownload.Add(wpt);
					}
					new DlgGpsManager(new GpsInsertWaypoint(WaypointsCache.insertWaypoint),
								m_cameraManager, m_pictureManager, toDownload, null, "downloadWaypoints").ShowDialog();
				}
				catch (Exception exc)
				{
					LibSys.StatusBar.Error("" + exc);
					messageWptLabel.Text = "Error: " + exc.Message;
				}
				finally
				{
					PutOnMap();

					// refresh and rebind the waypoints datagrid:
					rebuildWaypointsTab();
				}
			}		
			else
			{
				//messageWptLabel.Text = "Select waypoints to download first";
				Project.ShowPopup(wptDloadToGpsButton, "\nselect waypoints to download" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void wptUploadFromGpsButton_Click(object sender, System.EventArgs e)
		{
			if(DlgGpsManager.isBusy)
			{
				Project.ShowPopup(wptUploadFromGpsButton, "GPS Manager is still active and communicating to GPS", Point.Empty);
				DlgGpsManager.BringFormUp();
				return;
			}
			if(DlgGpsManager.isUp)
			{
				DlgGpsManager.CloseGently();
			}
			
			WaypointsCache.resetBoundaries();

			new DlgGpsManager(new GpsInsertWaypoint(WaypointsCache.insertWaypoint),
				m_cameraManager, m_pictureManager, null, null, "uploadWaypoints").ShowDialog();	// need to wait till upload is done

			PutOnMap();

			// refresh and rebind the waypoints datagrid:
			rebuildWaypointsTab();
		}

		private void tracksGpsButton_Click(object sender, System.EventArgs e)
		{
			if(DlgGpsManager.isBusy)
			{
				Project.ShowPopup(tracksGpsButton, "GPS Manager is still active and communicating to GPS", Point.Empty);
				DlgGpsManager.BringFormUp();
				return;
			}
			if(DlgGpsManager.isUp)
			{
				DlgGpsManager.CloseGently();
			}
			
			WaypointsCache.resetBoundaries();

			new DlgGpsManager(new GpsInsertWaypoint(WaypointsCache.insertWaypoint),
				m_cameraManager, m_pictureManager, null, null, "uploadTracks").ShowDialog();	// need to wait till upload is done

			PutOnMap();

			// refresh and rebind the tracks datagrid:
			rebuildTracksTab();
		}

		private void toGpsRteButton_Click(object sender, System.EventArgs e)
		{
			if(DlgGpsManager.isBusy)
			{
				Project.ShowPopup(toGpsRteButton, "GPS Manager is still active and communicating to GPS", Point.Empty);
				DlgGpsManager.BringFormUp();
				return;
			}
			if(DlgGpsManager.isUp)
			{
				DlgGpsManager.CloseGently();
			}
			
			Project.startRouteNumber = (int)startRouteNumericUpDown.Value;

			ArrayList selected = GetSelectedRows(routesDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int column = 0;
					DataTable table = m_routesDS.Tables[0];
					ArrayList toDownload = new ArrayList();
					foreach(int row in selected)
					{
						long routeId = (long)((int)routesDataGrid[row, column]);
						Track rte = WaypointsCache.getTrackById(routeId);
						if(rte != null)
						{
							toDownload.Add(rte);
						}
					}
					new DlgGpsManager(new GpsInsertWaypoint(WaypointsCache.insertWaypoint),
										m_cameraManager, m_pictureManager, null, toDownload, "downloadRoutes").ShowDialog();
				}
				catch (Exception exc)
				{
					LibSys.StatusBar.Error("" + exc);
					messageRteLabel.Text = "Error: " + exc.Message;
				}
				finally
				{
					PutOnMap();

					// refresh and rebind the routes datagrid:
					rebuildRoutesTab();
				}
			}		
			else
			{
				Project.ShowPopup(toGpsRteButton, "\nselect routes to download to GPS" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void fromGpsRteButton_Click(object sender, System.EventArgs e)
		{
			if(DlgGpsManager.isBusy)
			{
				Project.ShowPopup(fromGpsRteButton, "GPS Manager is still active and communicating to GPS", Point.Empty);
				DlgGpsManager.BringFormUp();
				return;
			}
			if(DlgGpsManager.isUp)
			{
				DlgGpsManager.CloseGently();
			}
			
			WaypointsCache.resetBoundaries();

			new DlgGpsManager(new GpsInsertWaypoint(WaypointsCache.insertWaypoint),
				m_cameraManager, m_pictureManager, null, null, "uploadRoutes").ShowDialog();	// need to wait till upload is done

			PutOnMap();

			// refresh and rebind the routes datagrid:
			rebuildRoutesTab();
		}

		private long defaultTrackId = -1;

		private void timeFilterDefaultsButton_Click(object sender, System.EventArgs e)
		{
			inSet = true;

			switch (defaultTrackId)
			{
				case -1:
					try
					{
						fromDateTimePicker.Value = Project.zuluToLocal(WaypointsCache.getMinTime());
						toDateTimePicker.Value = Project.zuluToLocal(WaypointsCache.getMaxTime());
						fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
						toTimeDateTimePicker.Value = toDateTimePicker.Value;
						aroundTimeDateTimePicker.Value = fromDateTimePicker.Value;
						aroundTimeTimeDateTimePicker.Value = fromDateTimePicker.Value;
					} 
					catch {}
					break;
				default:
					Track trk = WaypointsCache.getTrackById(defaultTrackId);
					if(trk != null)
					{
						fromDateTimePicker.Value = Project.zuluToLocal(trk.Start);
						toDateTimePicker.Value = Project.zuluToLocal(trk.End);
						fromTimeDateTimePicker.Value = fromDateTimePicker.Value;
						toTimeDateTimePicker.Value = toDateTimePicker.Value;
						aroundTimeDateTimePicker.Value = fromDateTimePicker.Value;
						aroundTimeTimeDateTimePicker.Value = fromDateTimePicker.Value;
					}
					break;
			}

			inSet = false;
		}

		private void timeFilterDefaultsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			defaultTrackId = -1;	// use global ("All Tracks") values
			try 
			{
				string strSelected = "" + timeFilterDefaultsComboBox.SelectedItem;
				int pos = strSelected.IndexOf(":");
				if(pos > 0)
				{
					defaultTrackId = Convert.ToInt64(strSelected.Substring(0, pos));
				}
			} 
			catch {}
		}

		private void playSpeedTrackBar_Scroll(object sender, System.EventArgs e)
		{
			playSleepTimeMs = 500 + (playSpeedTrackBar.Maximum - playSpeedTrackBar.Value) * 200;
		}

		private void sanityCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.sanityFilter = sanityCheckBox.Checked;
		}

		private void zoomInButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.zoomIn();
		}

		private void colorElevTracksCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.trackElevColor = colorElevTracksCheckBox.Checked;
			if(Project.trackElevColor)
			{
				colorSpeedTracksCheckBox.Checked = false;
			}
			m_pictureManager.LayersManager.ShowWaypoints = true;
			m_pictureManager.Refresh();
		}

		private void colorSpeedTracksCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.trackSpeedColor = colorSpeedTracksCheckBox.Checked;
			if(Project.trackSpeedColor)
			{
				colorElevTracksCheckBox.Checked = false;
			}
			m_pictureManager.LayersManager.ShowWaypoints = true;
			m_pictureManager.Refresh();
		}

		#region Tracks color palette related
 
		private void setColorPaletteButtons()
		{
			cp1Button.BackColor = TrackPalette.getColor(0);
			cp2Button.BackColor = TrackPalette.getColor(1);
			cp3Button.BackColor = TrackPalette.getColor(2);
			cp4Button.BackColor = TrackPalette.getColor(3);
			cp5Button.BackColor = TrackPalette.getColor(4);
			cp6Button.BackColor = TrackPalette.getColor(5);
			cp7Button.BackColor = TrackPalette.getColor(6);
			cp8Button.BackColor = TrackPalette.getColor(7);
		}

		private void colorButtonClick(int index)
		{
			paletteColorDialog.Color = TrackPalette.getColor(index);
			if(this.paletteColorDialog.ShowDialog() == DialogResult.OK)
			{
				TrackPalette.setColor(index, paletteColorDialog.Color);
				setColorPaletteButtons();
				this.m_pictureManager.Refresh();
			}
		}

		private void cp1Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(0);
		}

		private void cp2Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(1);
		}

		private void cp3Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(2);
		}

		private void cp4Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(3);
		}

		private void cp5Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(4);
		}

		private void cp6Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(5);
		}

		private void cp7Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(6);
		}

		private void cp8Button_Click(object sender, System.EventArgs e)
		{
			colorButtonClick(7);
		}

		private void thicknessApplyButton_Click(object sender, System.EventArgs e)
		{
			TrackPalette.penTrackThickness = (float)Convert.ToDouble(trackThicknessNumericUpDown.Value);
			TrackPalette.penRouteThickness = (float)Convert.ToDouble(routeThicknessNumericUpDown.Value);
			this.m_pictureManager.Refresh();
		}
		#endregion

		private void printFontComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.printTextFont = ((ComboBox)sender).SelectedIndex;
		}

		private void printLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			tabControl1.SelectedTab = trackpointsTabPage;
		}

		private void print2LinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			tabControl1.SelectedTab = trackpointsTabPage;
		}

		private void zoomCloseButton_Click(object sender, System.EventArgs e)
		{
			if(Math.Round(PictureManager.This.CameraManager.Location.Elev/10.0d) != Math.Round(Project.CAMERA_HEIGHT_REAL_CLOSE * 100.0d))
			{
				DlgPhotoManager.savedLocation = new GeoCoord(PictureManager.This.CameraManager.Location);
			}
			GeoCoord newLocation = new GeoCoord(DlgPhotoManager.savedLocation);
			newLocation.Elev = Project.CAMERA_HEIGHT_REAL_CLOSE * 1000.0d;		// for 1m/pixel
			PictureManager.This.CameraManager.SpoilPicture();
			PictureManager.This.CameraManager.Location = newLocation;		// calls ProcessCameraMove();
		}

		private void zoomCloseTrkptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(trackpointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int column  = 0;
					int column1 = 1;
					DataTable table = m_trackpointsDS.Tables[0];
					int waypointId = (int)trackpointsDataGrid[row, column];
					long trackId = (long)((int)trackpointsDataGrid[row, column1]);
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						Waypoint wpt = trk.getTrackpointById(waypointId);
						//messageTrkptLabel.Text = "trk:" + trackId + " trkpt: " + wpt;
						if(Math.Round(PictureManager.This.CameraManager.Location.Elev/10.0d) != Math.Round(Project.CAMERA_HEIGHT_REAL_CLOSE * 100.0d))
						{
							DlgPhotoManager.savedLocation = new GeoCoord(m_cameraManager.Location);
						}
						GeoCoord newLocation = new GeoCoord(wpt.Location);
						newLocation.Elev = Project.CAMERA_HEIGHT_REAL_CLOSE * 1000.0d;		// for 1m/pixel
						m_cameraManager.SpoilPicture();
						m_cameraManager.Location = newLocation;		// calls ProcessCameraMove();
					}
				}
				catch (Exception exc)
				{
					messageTrkptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				messageTrkptLabel.Text = "Error: Select a trackpoint first";
				Project.ShowPopup(gotoTrkButton, "\nselect a trackpoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void zoomCloseWptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				try
				{
					int row = (int)selected[0];
					int columnWptId = 0;
					DataTable table = m_waypointsDS.Tables[0];
					int waypointId = (int)waypointsDataGrid[row, columnWptId];
					Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
					messageWptLabel.Text = "waypoint: " + wpt;
					wpt.ForceShow = true;	// make sure label shows on the map, even if crowded
					if(Math.Round(PictureManager.This.CameraManager.Location.Elev/10.0d) != Math.Round(Project.CAMERA_HEIGHT_REAL_CLOSE * 100.0d))
					{
						DlgPhotoManager.savedLocation = new GeoCoord(m_cameraManager.Location);
					}
					GeoCoord newLocation = new GeoCoord(wpt.Location);
					newLocation.Elev = Project.CAMERA_HEIGHT_REAL_CLOSE * 1000.0d;		// for 1m/pixel
					m_cameraManager.SpoilPicture();
					m_cameraManager.Location = newLocation;		// calls ProcessCameraMove();
					// refresh and rebind the waypoints datagrid, so that distances are refreshed:
					rebuildWaypointsTab();
				}
				catch (Exception exc)
				{
					messageWptLabel.Text = "Error: " + exc.Message;
				}
			}		
			else
			{
				//messageWptLabel.Text = "Error: Select a waypoint first";
				Project.ShowPopup(gotoWptButton, "\nselect a waypoint first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void zoomOutButton_Click(object sender, System.EventArgs e)
		{
			if(DlgPhotoManager.savedLocation != null)
			{
				PictureManager.This.CameraManager.SpoilPicture();
				PictureManager.This.CameraManager.Elev = DlgPhotoManager.savedLocation.Elev;		// calls ProcessCameraMove();
				DlgPhotoManager.savedLocation = null;
			}
		}

		private void waypointNameStyleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.waypointNameStyle = waypointNameStyleComboBox.SelectedIndex;
			m_cameraManager.ProcessCameraMove();
		}

		private void trackNameStyleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.trackNameStyle = trackNameStyleComboBox.SelectedIndex;
			m_cameraManager.ProcessCameraMove();
		}

		private void pointsPerRouteNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			Project.gpsMaxPointsPerRoute = Convert.ToInt32(this.pointsPerRouteNumericUpDown.Value);
		}

		private void printWptButton_MouseHover(object sender, System.EventArgs e)
		{
			Project.ShowPopup(printWptButton, "Waypoints will be printed in the sort order selected in the list", printWptButton.PointToScreen(new Point(40, 20)));
		}

		private void timeUtcRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				bool isChecked = ((RadioButton)sender).Checked;
				timeUtcRadioButton.Checked  = isChecked;
//				timeUtc2RadioButton.Checked = isChecked;
//				timeUtc3RadioButton.Checked = isChecked;
//				if(m_timeCol != null)
//				{
//					m_timeCol.HeaderText = "Time  [UTC]"; 
//				}
				if(!Project.useUtcTime && isChecked)
				{
					graphByTimeControl.UseUtcTime = isChecked;		// also sets Project.useUtcTime
					changeTimeOffset(true);
					setUtcLabels();
//					rebuildTableTab(isFilterAppliedToGraphAndTable);
//					LayerEarthquakes.This.init();	// provoke PutOnMap()
//					m_pictureManager.Refresh();
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
//				timeLocal2RadioButton.Checked = isChecked;
//				timeLocal3RadioButton.Checked = isChecked;
//				if(m_timeCol != null)
//				{
//					m_timeCol.HeaderText = "Time  [your computer]"; 
//				}
				if(Project.useUtcTime && isChecked)
				{
					graphByTimeControl.UseUtcTime = !isChecked;		// also sets Project.useUtcTime
					changeTimeOffset(false);
					setUtcLabels();
//					rebuildTableTab(isFilterAppliedToGraphAndTable);
//					LayerEarthquakes.This.init();	// provoke PutOnMap()
//					m_pictureManager.Refresh();
				}
			}
			if(tabControl1.SelectedTab == graphTabPage)
			{
				graphByTimeControl.Focus();
			}
		}

		private void changeTimeOffset(bool useUtcTime)
		{
			inSet = true;

			inSet = false;
		}

		private void setUtcLabels()
		{
			string msg = Project.useUtcTime ? "all times are UTC" : "all times are local to YOUR COMPUTER";
			messageLabel.Text = msg;
		}

		private void setZoomButtons()
		{
			gotoTrk5Button.Enabled = m_lastGraphWpt != null;
			zoomClose5Button.Enabled = m_lastGraphWpt != null;
		}

		private void gotoTrk5Button_Click(object sender, System.EventArgs e)
		{
			if(m_lastGraphWpt != null)
			{
				GeoCoord newLoc = new GeoCoord(m_lastGraphWpt.Location.X, m_lastGraphWpt.Location.Y, m_cameraManager.Elev);
				m_cameraManager.Location = newLoc;
			}
		}

		private void zoomClose5Button_Click(object sender, System.EventArgs e)
		{
			if(m_lastGraphWpt != null)
			{
				Waypoint wpt = m_lastGraphWpt;
				if(Math.Round(PictureManager.This.CameraManager.Location.Elev/10.0d) != Math.Round(Project.CAMERA_HEIGHT_REAL_CLOSE * 100.0d))
				{
					DlgPhotoManager.savedLocation = new GeoCoord(m_cameraManager.Location);
				}
				GeoCoord newLocation = new GeoCoord(wpt.Location);
				newLocation.Elev = Project.CAMERA_HEIGHT_REAL_CLOSE * 1000.0d;		// for 1m/pixel
				m_cameraManager.SpoilPicture();
				m_cameraManager.Location = newLocation;		// calls ProcessCameraMove();
			}
		}

		private void showAllButton_Click(object sender, System.EventArgs e)
		{
			rebuildGraphTab();
			this.graphByTimeControl.Refresh();
		}

		private void DlgWaypointsManager_Load(object sender, System.EventArgs e)
		{
//			this.BringToFront();
		}

		private void showInGeTracksRoutes(DataGrid dataGrid, DataSet dataSet)
		{
			ArrayList selected = GetSelectedRows(dataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = dataSet.Tables[0];
				ArrayList tracks = new ArrayList();		// array of Track
				for(int i=0; i < selected.Count ;i++)
				{
					int row = (int)selected[i];
					long trackId = (long)((int)dataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + trackId + " ";
#endif
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						m_trackIdToSelect = trk.Id;
						tracks.Add(trk);
					}
				}
#if DEBUG
				messageTrkLabel.Text = "selected: " + str;
#else
				messageTrkLabel.Text = "";
#endif

				bool skipIt = false;

				if(Project.kmlOptions.ShowInPopup)
				{
					KmlOptionsPopup kmlOptionsPopup = new KmlOptionsPopup();

					Point ppop = showInGeButton.PointToScreen(new Point(-270, -145));
					kmlOptionsPopup.Top = ppop.Y;
					kmlOptionsPopup.Left = ppop.X;

					kmlOptionsPopup.ShowDialog(this);
					skipIt = !kmlOptionsPopup.doContinue;
				}

				if(!skipIt)
				{
					GoogleEarthManager.runTracks(tracks);
				}
			}
			else
			{
				//messageTrkLabel.Text = "Error: select a track first";
				Project.ShowPopup(saveButton, "\nselect a track/route first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void showInGeButton_Click(object sender, System.EventArgs e)
		{
			showInGeTracksRoutes(tracksDataGrid, m_tracksDS);
		}

		private void showInGeRteButton_Click(object sender, System.EventArgs e)
		{
			showInGeTracksRoutes(routesDataGrid, m_routesDS);
		}

		private void showInGeWptButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRowsView(waypointsDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int columnWptId = 0;
				DataTable table = m_waypointsDS.Tables[0];
				ArrayList waypoints = new ArrayList();		// array of Waypoint
				for(int i=0; i < selected.Count ;i++)
				{
					int row = (int)selected[i];
					int waypointId = (int)waypointsDataGrid[row, columnWptId];
#if DEBUG
					str += "" + row + "=" + waypointId + " ";
#endif
					Waypoint wpt = WaypointsCache.getWaypointById(waypointId);
					if(wpt != null)
					{
						//m_wptIdToSelect = wpt.Id;
						waypoints.Add(wpt);
					}
				}
#if DEBUG
				messageWptLabel.Text = "selected: " + str;
#else
				messageWptLabel.Text = "";
#endif

				bool skipIt = false;

				if(Project.kmlOptions.ShowInPopup)
				{
					KmlOptionsPopup kmlOptionsPopup = new KmlOptionsPopup();

					Point ppop = showInGeWptButton.PointToScreen(new Point(-270, -145));
					kmlOptionsPopup.Top = ppop.Y;
					kmlOptionsPopup.Left = ppop.X;

					kmlOptionsPopup.ShowDialog(this);
					skipIt = !kmlOptionsPopup.doContinue;
				}

				if(!skipIt)
				{
					GoogleEarthManager.runWaypoints(waypoints);
				}
			}
			else
			{
				//messageWptLabel.Text = "Error: select a waypoint first";
				Project.ShowPopup(wptSaveButton, "\nselect a waypoint(s) first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		/*
CSC DUATS on the Web
Flight Plan SNA-LAX 

From: SNA -- Santa Ana CA (John Wayne Airport-Orange County)
To:   KLAX -- Los Angeles CA
Alt.: 4,500 ft.  Profile: Cessna172P
Time: Wed Jun 20 05:45 (UTC)

Routing options selected:  Direct.
Flight plan route:
  OKB F70 HMT RAL CNO ONT EMT BUR SMO LAX 
Flight totals: fuel: 16 gallons, time: 1:44, distance 170.0 nm.

    Ident  Type/Morse Code  |                        | Fuel
    Name or Fix/radial/dist |                        | Time
    Latitude Longitude Alt. | Route   Mag  KTS  Fuel | Dist
---+--------+---------+-----| Winds   Crs  TAS  Time |------
 1. SNA    Apt.             | Temp    Hdg   GS  Dist |  0.0 
    Santa Ana CA (John Wayn |--------+----+---+------| 0:00 
    33:40:32 117:52:05    1 | Direct             4.2 |  170 
---+--------+---------+-----| 218/9   123   92  0:25 |------
 2. OKB    Apt.             | +25C    128   90    38 |  4.2 
    Oceanside CA            |--------+----+---+------| 0:25 
    33:13:04 117:21:05   45 | Direct             2.1 |  132 
---+--------+---------+-----| 178/3   015  100  0:14 |------
 3. F70    Apt.             | +22C    015  103    24 |  6.3 
    Murrieta/Temecula CA (F |--------+----+---+------| 0:39 
    33:34:27 117:07:42   45 | Direct             1.0 |  108 
---+--------+---------+-----| 207/6   016  100  0:07 |------
 4. HMT    Apt.             | +21C    016  106    11 |  7.3 
    Hemet CA (Hemet-Ryan)   |--------+----+---+------| 0:46 
    33:44:02 117:01:21   45 | Direct             2.2 |   97 
---+--------+---------+-----| 170/3   289  100  0:14 |------
 5. RAL    .-.  .-  .-..    | +22C    287  102    25 |  9.5 
    112.4  Riverside        |--------+----+---+------| 1:00 
    33:57:07 117:26:57   45 | Direct             0.8 |   72 
---+--------+---------+-----| 170/3   263  100  0:06 |------
 6. CNO    Apt.             | +22C    262  101     9 | 10.3 
    Chino CA                |--------+----+---+------| 1:06 
    33:58:28 117:38:11   45 | Direct             0.5 |   63 
---+--------+---------+-----| 170/3   006  100  0:03 |------
 7. ONT    Apt.             | +22C    007  103     5 | 10.8 
    Ontario CA              |--------+----+---+------| 1:09 
    34:03:21 117:36:04   45 | Direct             1.9 |   58 
---+--------+---------+-----| 170/3   261  100  0:13 |------
 8. EMT                     | +22C    260  101    22 | 12.7 
    359    El Monte         |--------+----+---+------| 1:22 
    34:05:17 118:01:52   45 | Direct             1.6 |   36 
---+--------+---------+-----| 221/3   278  100  0:10 |------
 9. BUR    Apt.             | +22C    276   99    17 | 14.3 
    Burbank CA (Bob Hope)   |--------+----+---+------| 1:32 
    34:12:02 118:21:31   45 | Direct             1.1 |   19 
---+--------+---------+-----| 171/1   188   97  0:08 |------
10. SMO    ...  --  ---     | +23C    188   96    13 | 15.4 
    d110.8 Santa Monica     |--------+----+---+------| 1:40 
    34:00:36 118:27:24   30 | Direct             0.4 |    6 
---+--------+---------+-----| 000/0   150   90  0:03 |------
11. LAX    .-..  .-  -..-   | +25C    150   90     5 | 15.8 
    d113.6 Los Angeles      |--------+----+---+------| 1:43 
    33:55:59 118:25:55    8 | Direct             0.2 |    1 
---+--------+---------+-----| 000/0   050   90  0:01 |------
12. KLAX   Apt.             | +25C    050   90     1 | 16.0 
    Los Angeles CA          |--------+----+---+------| 1:44 
    33:56:33 118:24:25    1 |                        |    0 
---+--------+---------+-----|                        |------

NOTE: fuel calculations do not include required reserves.
Flight totals: fuel: 16 gallons, time: 1:44, distance 170.0 nm.
Average groundspeed 98 knots.
Great circle distance is 31.3 nm -- this route is 444% longer.
Flight Planning System Copyright (C) 1991-2004 Enflight.com.
All rights reserved.
		 */

		private const string m_duatsSample = @"
CSC DUATS on the Web
Flight Plan SNA-LAX 

From: SNA -- Santa Ana CA (John Wayne Airport-Orange County)
To:   KLAX -- Los Angeles CA
Alt.: 4,500 ft.  Profile: Cessna172P
Time: Wed Jun 20 05:45 (UTC)

Routing options selected:  Direct.
Flight plan route:
  OKB F70 HMT RAL CNO ONT EMT BUR SMO LAX 
Flight totals: fuel: 16 gallons, time: 1:44, distance 170.0 nm.

 Ident  Type/Morse Code |                                      | Fuel
    Name or Fix/radial/dist |                                      | Time
    Latitude Longitude Alt. | Route   Mag  KTS  Fuel | Dist
---+--------+---------+-----| Winds   Crs  TAS  Time |------
 1. SNA    Apt.                  | Temp    Hdg   GS  Dist   |  0.0 
    Santa Ana CA (John W|--------+----+---+--------| 0:00 
    33:40:32 117:52:05     | Direct             4.2 |  170 
---+--------+---------+-----| 218/9   123   92  0:25  |------
 2. OKB    Apt.                  | +25C    128   90    38   |  4.2 

.............................................................

---+--------+---------+-----| 000/0   150   90  0:03 |------
11. LAX    .-..  .-  -..-        | +25C    150   90     5   | 15.8 
    d113.6 Los Angeles      |--------+----+---+-------| 1:43 
    33:55:59 118:25:55     | Direct             0.2        |    1 
---+--------+---------+-----| 000/0   050   90  0:01 |------
12. KLAX   Apt.                 | +25C    050   90     1   | 16.0 
    Los Angeles CA             |--------+----+---+------| 1:44 
    33:56:33 118:24:25     |                                     |    0 
---+--------+---------+-----|                                     |------

NOTE: fuel calculations do not include required reserves.
Flight totals: fuel: 16 gallons, time: 1:44, distance 170.0 nm.
Average groundspeed 98 knots.";

		private void duatsConvertButton_Click(object sender, System.EventArgs e)
		{
			string duatsPlanTxt = duatsTextBox.Text;

			TextReader reader = new StringReader(duatsPlanTxt);

			Track route = null;
			
			int state = 0;
			string str;
			
			string fromName = null;
			string toName = null;
			string wptName = null;
			string wptDescr = null;
			string wptLon = null;
			string wptLat = null;
			string wptAlt = null;
			string wptUrl = "";

			string wptFuel = null;
			string wptTime = null;
			string wptDist = null;

			string legFuel = null;
			string legTime = null;
			string legDist = null;

			int wptcnt = 0;
			DateTime startDate = Project.localToZulu(new DateTime(7000, 1, 1));

			try
			{
				while ((str=reader.ReadLine()) != null)
				{
					switch(state)
					{
						case 0:
							if(str.StartsWith("From:"))
							{
								fromName = str.Substring(5);
							}
							if(str.StartsWith("To:"))
							{
								toName = str.Substring(3);
								state = 100;

								CreateInfo createInfo = new CreateInfo();
								createInfo.init("rte");
								Project.trackId++;
								createInfo.id = Project.trackId;
								createInfo.name = fromName + " ->> " + toName;
								createInfo.source = "From DUATS.COM flight plan";

								route = new Track(createInfo);
							}
							break;

						case 100:
							if(str.StartsWith("---+--------+---------+-----"))
							{
								state = 110;
							}
							break;

						case 110:
							if(str.Length > 51 && str.Substring(2,1) == ".")
							{
								wptFuel = str.Substring(54).Trim();
								wptName = str.Substring(4, 24).Replace("Apt.", "").Trim();
								state = 120;
							}
							else
							{
								state = 900;
							}
							break;

						case 120:
							wptTime = str.Substring(54).Trim();
							wptDescr = str.Substring(4, 24).Trim();
							state = 130;
							break;

						case 130:
						{
							wptDist = str.Substring(54).Trim();

							wptLat = str.Substring(4, 8);
							wptLon = str.Substring(13, 9);
							wptAlt = str.Substring(22, 5).Trim() + "00";	// feet
							//LibSys.StatusBar.Trace("lat='" + wptLat + "' lon='" + wptLon + "' alt='" + wptAlt + "'");

							double altMeters = Convert.ToDouble(wptAlt) * Distance.METERS_PER_FOOT;
							// sPos in form: W117.23'45" N36.44'48" [or N33,27.661 W117,42.945]
							string sPos = "W" + wptLon.Substring(0,3).Trim() + "." + wptLon.Substring(4,2) + "'" + wptLon.Substring(7) + "\" "
								+ "N" + wptLat.Substring(0,2).Trim() + "." + wptLat.Substring(3,2) + "'" + wptLat.Substring(6) + "\"";
							//LibSys.StatusBar.Trace("sPos='" + sPos + "'");
							GeoCoord loc = new GeoCoord(sPos);
							loc.Elev = altMeters;
							//LibSys.StatusBar.Trace("wpt='" + loc + "'");
 
							//DateTime wptDateTime = DateTime.Now.AddMilliseconds(wptcnt);

							string[] split = wptTime.Split(new Char[] { ':' });
							double hours = Convert.ToDouble(split[0]);
							double minutes = Convert.ToDouble(split[1]);
							DateTime wptDateTime = startDate.AddHours(hours).AddMinutes(minutes);

							Waypoint wpt = new Waypoint(loc, wptDateTime, LiveObjectTypes.LiveObjectTypeRoutepoint, route.Id, wptName, route.Source, wptUrl);
							wpt.Desc = wptDescr + "\nFuelSpent: " + wptFuel + " TimeInFlight: " + wptTime + " DistLeft: " + wptDist;
							wptcnt++;

							route.Trackpoints.Add(wpt.DateTime, wpt);
						}

							state = 100;
							break;
					}
				}
			}
			catch (Exception exc)
			{
				LibSys.StatusBar.Error("Exception: " + exc);
			}

			if(route == null)
			{
				System.Windows.Forms.MessageBox.Show(this, "Error: No data to parse.\n\nPlease copy DUATS.COM generated flight plan into the text field.\n\nIt should look like this:\n\n" + m_duatsSample);
			}
			else
			{
				WaypointsCache.TracksAll.Add(route);
				route.rebuildTrackBoundaries();
				route.PutOnMap(LayerWaypoints.This, null, LayerWaypoints.This);
				m_routeIdToSelect = route.Id;

				// refresh map and rebind the datagrid:
				PutOnMap();
				WaypointsCache.isDirty = true;
				//rebuildTracksTab();
				rebuildRoutesTab();
				tabControl1.SelectedTab = this.routesTabPage;
				prevSelectedTab = this.routesTabPage;
			}
		}
	}
}
