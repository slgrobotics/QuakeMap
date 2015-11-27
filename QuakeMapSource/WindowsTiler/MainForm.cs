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
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using System.Runtime.InteropServices;
		 
using LibSys;
using LibNet;
using LibFormats;
using LibGeo;
using LibGps;
using LibGui;

using LibNet.TerraServer;
using LonLatPt = LibNet.TerraServer.LonLatPt;


namespace WindowsTiler
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public sealed class MainForm : System.Windows.Forms.Form, ILocationWatcher, IMainCommand
	{
		#region Some members defined here
 
		private CameraTrack m_cameraTrack;
		private CameraManager m_cameraManager;
		private PictureManager m_pictureManager;

		private bool m_firstTime = false;
		private int m_daysSinceInstall = Project.TRIAL_DAYS;
		private static string m_warning = "Use at your own risk. Do not use while driving.\nData quality or trouble-free operation not guaranteed.";
		private static string m_greeting = "";
		private static Thread m_greetingThread = null;

		// scrollbar does not achieve the max value if you just move the slider to the bottom.
		// the values below reflect actual range of values achieved by moving the slider:
		private int heightScrollBar_Minimum = 0;
		private int heightScrollBar_Maximum = 91;

		// processing dropped files on the icon (which end up in args):
		private static string[] m_args;

		#endregion // Some members defined here

		#region GUI elements defined
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.Panel centerPanel;
		private System.Windows.Forms.Panel sb2Panel;
		private System.Windows.Forms.Panel sb1Panel;
		private System.Windows.Forms.Label statusBar;
		private System.Windows.Forms.Label statusBar2;
		private System.Windows.Forms.Panel leftPanel;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel picturePanel;
		private System.Windows.Forms.PictureBox mainPictureBox;
		private OpacityPopup opacityPopup;
		private TrackProfileControl m_plannerControl;

        private bool keystrokeProcessed;
//        private IContainer components;

		private System.Windows.Forms.TextBox scaleTextBox;
		private System.Windows.Forms.VScrollBar heightScrollBar;
		private System.Windows.Forms.CheckBox gridCheckBox;
		private System.Windows.Forms.CheckBox citiesCheckBox;
		private System.Windows.Forms.MenuItem helpMenuItem;
		private System.Windows.Forms.MenuItem optionsMenuItem;
		private System.Windows.Forms.MenuItem aboutMenuItem;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.RadioButton radioButton5;
		private System.Windows.Forms.RadioButton radioButton6;
		private System.Windows.Forms.RadioButton radioButton7;
		private System.Windows.Forms.CheckBox eqCheckBox;
		private System.Windows.Forms.CheckBox wptCheckBox;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.CheckBox vehCheckBox;
		private System.Windows.Forms.CheckBox reliefCheckBox;
		private System.Windows.Forms.CheckBox aerialCheckBox;
		private System.Windows.Forms.CheckBox topoCheckBox;
		private System.Windows.Forms.Button gotoButton;
		private System.Windows.Forms.CheckBox lmCheckBox;
		private System.Windows.Forms.Button lmButton;
        private System.Windows.Forms.MenuItem importMenuItem;
		private System.Windows.Forms.MenuItem gpsManagerMenuItem;
		private System.Windows.Forms.Button wptButton;
		private System.Windows.Forms.MenuItem exportMenuItem;
		private System.Windows.Forms.MenuItem addToFavoritesMenuItem;
		private System.Windows.Forms.MenuItem organizeFavoritesMenuItem;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem startCamTrackMenuItem;
		private System.Windows.Forms.MenuItem stopCamTrackMenuItem;
		private System.Windows.Forms.Button progressButton;
		private System.Windows.Forms.ProgressBar mainProgressBar;
		private System.Windows.Forms.MenuItem streetMapMenuItem;
		private System.Windows.Forms.ContextMenu mainPictureBox_ContextMenu;
		private System.Windows.Forms.MenuItem saveTrackMenuItem;
		private System.Windows.Forms.MenuItem fileOpenMenuItem;
		private System.Windows.Forms.MenuItem streetMap1MenuItem;
		private System.Windows.Forms.MenuItem aerialMenuItem;
		private System.Windows.Forms.MenuItem topoMenuItem;
		private System.Windows.Forms.MenuItem reliefMenuItem;
		private System.Windows.Forms.MenuItem fetchEqMenuItem;
		private System.Windows.Forms.MenuItem citiesMenuItem;
		private System.Windows.Forms.MenuItem eqMenuItem;
		private System.Windows.Forms.MenuItem lmMenuItem;
		private System.Windows.Forms.MenuItem vehMenuItem;
		private System.Windows.Forms.MenuItem wptMenuItem;
		private System.Windows.Forms.MenuItem gpsCurrentMenuItem;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem backMenuItem;
		private System.Windows.Forms.MenuItem forwardMenuItem;
		private System.Windows.Forms.Button eqButton;
		private System.Windows.Forms.MenuItem eqLocalMenuItem;
		private System.Windows.Forms.MenuItem eqWorldMenuItem;
		private System.Windows.Forms.MenuItem fetchEq1MenuItem;
		private System.Windows.Forms.MenuItem getTrackLogMenuItem;
		private System.Windows.Forms.MenuItem menuItem12;
		private System.Windows.Forms.MenuItem fileExitMenuItem;
		private System.Windows.Forms.Label initialMessageLabel;
		private System.Drawing.Printing.PrintDocument mainPrintDocument;
		private System.Windows.Forms.MenuItem menuItem17;
		private System.Windows.Forms.MenuItem printPreviewMenuItem;
		private System.Windows.Forms.MenuItem printMenuItem;
		private System.Windows.Forms.MenuItem pageSetupMenuItem;
		private System.Windows.Forms.MenuItem newWaypointMenuItem;
		private System.Windows.Forms.MenuItem eqHistoricalMenuItem;
		private System.Windows.Forms.MenuItem eqClearMenuItem;
		private System.Windows.Forms.MenuItem eqOptionsMenuItem;
		private System.Windows.Forms.MenuItem menuItem13;
		private System.Windows.Forms.MenuItem refreshMenuItem;
		private System.Windows.Forms.MenuItem optionsMapMenuItem;
		private System.Windows.Forms.Label cameraHeightLabel;
		private System.Windows.Forms.Panel eqStylePanel;
		private System.Windows.Forms.Panel opacityPanel;
		private System.Windows.Forms.Panel hintsPanel;
		private System.Windows.Forms.MenuItem gotoMenuItem;
		private System.Windows.Forms.MenuItem updateMenuItem;
		private System.Windows.Forms.MenuItem nearestGeocachesMenuItem;
		private System.Windows.Forms.MenuItem nearestGeocaches1MenuItem;
		private System.Windows.Forms.Button eqFilterOnButton;
		private System.Windows.Forms.Button camtrackOnButton;
		private System.Windows.Forms.MenuItem fileMainMmenuItem;
		private System.Windows.Forms.MenuItem internetMainMenuItem;
		private System.Windows.Forms.MenuItem gpsMainMenuItem;
		private System.Windows.Forms.MenuItem pdaMainMenuItem;
		private System.Windows.Forms.MenuItem mapMainMenuItem;
		private System.Windows.Forms.MenuItem eqMainMenuItem;
		private System.Windows.Forms.MenuItem favMainMenuItem;
		private System.Windows.Forms.MenuItem helpMainMenuItem;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.MenuItem importsListMenuItem;
		private System.Windows.Forms.MenuItem onlineMenuItem;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem newRouteMenuItem;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button scale1mButton;
		private System.Windows.Forms.Button scale2mButton;
		private System.Windows.Forms.Button scale4mButton;
		private System.Windows.Forms.Button scale8mButton;
		private System.Windows.Forms.Button scale16mButton;
		private System.Windows.Forms.Button scale32mButton;
		private System.Windows.Forms.Button scale64mButton;
		private System.Windows.Forms.Button scaleSafeButton;
		private System.Windows.Forms.Button scaleYButton;
		private System.Windows.Forms.Button scaleWorldButton;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.MenuItem useServicesMenuItem;
		private System.Windows.Forms.MenuItem loadTilesMenuItem;
		private System.Windows.Forms.MenuItem exportFavoritesMenuItem;
		private System.Windows.Forms.MenuItem importFavoritesMenuItem;
		private System.Windows.Forms.MenuItem trackManagerMenuItem;
		private System.Windows.Forms.MenuItem colorCodeMenuItem;
		private System.Windows.Forms.MenuItem colorSpeedMenuItem;
		private System.Windows.Forms.MenuItem photoMainMenuItem;
		private System.Windows.Forms.MenuItem photoManagerMenuItem;
		private System.Windows.Forms.MenuItem viewImageMenuItem;
		private System.Windows.Forms.MenuItem thumbMenuItem;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem thumb2MenuItem;
		private System.Windows.Forms.MenuItem unrelatedPhotosMenuItem;
		private System.Windows.Forms.MenuItem photoOptionsMenuItem;
		private System.Windows.Forms.MenuItem listPhotosMenuItem;
		private System.Windows.Forms.Label notAssociatedLabel;
		private System.Windows.Forms.Button notAssociatedButton;
		private System.Windows.Forms.CheckBox colorAerialCheckBox;
		private System.Windows.Forms.MenuItem toolsMainMenuItem;
		private System.Windows.Forms.MenuItem manageToolsMenuItem;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.Label hintsLabel;
		private System.Windows.Forms.Panel bottomHintsPanel;
		private System.Windows.Forms.Panel centerHintsPanel;
		private System.Windows.Forms.MenuItem saveImageMenuItem;
		private System.Windows.Forms.MenuItem pdaSuperframeMenuItem;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem pdaCancelMenuItem;
		private System.Windows.Forms.CheckBox overlayCheckBox;
		private System.Windows.Forms.MenuItem photoAnalyzeGpxMenuItem;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem clearImportsListMenuItem;
		private System.Windows.Forms.MenuItem distanceMenuItem;
		private System.Windows.Forms.MenuItem menuItem11;
		private System.Windows.Forms.MenuItem pdaPreloadAndExportMenuItem;
		private System.Windows.Forms.MenuItem fileGpsBabelMenuItem;
		private System.Windows.Forms.MenuItem gpsGpsBabelMenuItem;
		private System.Windows.Forms.MenuItem geoTiffManagerMenuItem;
		private System.Windows.Forms.MenuItem mapWizardMenuItem;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.CheckBox helpAtStartCheckBox;
		private System.Windows.Forms.LinkLabel opacityLinkLabel;
		private System.Windows.Forms.LinkLabel backLinkLabel;
		private System.Windows.Forms.LinkLabel forwardLinkLabel;
		private System.Windows.Forms.LinkLabel findLinkLabel;
		private System.Windows.Forms.MenuItem helpSampleFilesMenuItem;
		private System.Windows.Forms.MenuItem helpSamplePhotoMenuItem;
		private System.Windows.Forms.MenuItem helpSampleGeotiffMenuItem;
		private System.Windows.Forms.MenuItem helpSampleWebpageMenuItem;
		private System.Windows.Forms.MenuItem helpSampleAnnotatedTripMenuItem;
		private System.Windows.Forms.MenuItem helpSamplePocketqueryMenuItem;
		private System.Windows.Forms.MenuItem helpSampleNtsbMenuItem;
		private System.Windows.Forms.MenuItem helpSampleFleetApiMenuItem;
		private System.Windows.Forms.MenuItem menuItem14;
		private System.Windows.Forms.MenuItem helpSampleDeleteFilesMenuItem;
		private System.Windows.Forms.MenuItem pdaPreloadAlongRouteMenuItem;
		private System.Windows.Forms.MenuItem loadAlongRouteMenuItem;
		private System.Windows.Forms.MenuItem photoToGeotiffMenuItem;
		private System.Windows.Forms.MenuItem eqFilterMenuItem;
		private System.Windows.Forms.MenuItem helpSampleFlyingMenuItem;
		private System.Windows.Forms.Panel plannerPanel;
		private System.Windows.Forms.LinkLabel plannerLinkLabel;
        private IContainer components;
		private System.Windows.Forms.MenuItem saveWebPageMenuItem;
		#endregion

		#region Constructor and initial processing

		public MainForm(string[] args)		// args will contain list of files dragged onto QM icon when starting the program
		{
			LibSys.StatusBar.WriteLine("Starting GUI");
			Project.mainForm = this;		// Project.MessageBox needs this one (TODO:check this....)
			Project.mainCommand = this;

			m_args = (string[])args.Clone();	// we need to null out some elements as we process them
			/*
			#if DEBUG
						if(m_args != null && m_args.Length > 0)
						{
							string allargs = "ARGs:\n";
							foreach (string arg in m_args)
							{
								allargs += arg;
								allargs += "\n";
							}
							Project.MessageBox(this, allargs);
						}
			#endif
			*/
			/*
			byte[] bytes = new byte[] { 20, 235, 205, 66, 253, 124, 179, 65, 122, 193, 88, 65, 6, 19, 143, 65, 3, 0, 245, 195, 255, 255, 165, 65, 34, 65, 183, 96, 45, 131, 203, 192, 226, 63, 63, 112, 14, 149, 182, 109, 0, 192, 204, 60, 137, 59, 0, 176, 12, 187, 53, 22, 35, 187, 12, 119, 0, 66, 13, 0, 159, 18, 0, 0 };
			float f = new GPacket().fromFloatBytes(bytes, 0);
			new GPacket().toFloatBytes(f, bytes, 0);
			GPacket.fromDoubleBytesS(bytes, 26);
			*/

			// it is important to have access to quakemap.ini so that we do not create
			// mapping and terraserver cache folders in random places. Check it and exit if it is not there:
            Project.mapsPath = Project.mapsPathDefault; //  Project.readIniFile("MAPSDIR");
			LibSys.StatusBar.WriteLine("Default location for mapping cache in " + Project.mapsPath);

			LibSys.StatusBar.WriteLine("Getting ready");
			while(true) 
			{
				if(Project.goingDown)
				{
					return;
				}

                if (File.Exists(Project.GetMiscPath(Project.OPTIONS_FILE_NAME)))
                {
                    break;
                }

				m_firstTime = true;
				Project.serverAvailable = true;

				// End User License Agreement:
				new DlgEula().ShowDialog();

				// Home Location:
				new DlgZipcode().ShowDialog();
			}

			LibSys.StatusBar.WriteLine("System check 1");
			
			// debug
			/*
			Project.ReadOptions();
			new DlgGpsSelect(new GpsInsertWaypoint(WaypointsCache.insertWaypoint)).ShowDialog();
			Project.Exit();
			return;
			*/
			// end debug


			DateTime t1 = DateTime.Now;
			Project.ReadOptions();
			TimeSpan ts = DateTime.Now - t1;
			LibSys.StatusBar.WriteLine("ReadOptions() took " + ts.Milliseconds + " ms");

			LibSys.StatusBar.WriteLine("Greeting");
			if(Project.showHelpAtStart && !m_firstTime && m_greeting.Length > 0)
			{
				m_greeting = m_greeting + "\n\n...initializing, please wait...\n...trying to reach the server...\n...framework " + Environment.Version;
				// display greeting in a separate thread, and proceed with initializing:
				m_greetingThread =	new Thread( new ThreadStart(ShowGreeting));
				// see Entry.cs for how the current culture is set:
				m_greetingThread.CurrentCulture = Thread.CurrentThread.CurrentCulture; //new CultureInfo("en-US", false);
				m_greetingThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture; //new CultureInfo("en-US", false);
				m_greetingThread.IsBackground = true;	// terminate with the main process
				m_greetingThread.Name = "Greeting";
				m_greetingThread.Start();
			}

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.photoToGeotiffMenuItem.Visible = false;

			this.m_plannerControl = new TrackProfileControl();
			this.m_plannerControl.Dock = System.Windows.Forms.DockStyle.Fill;

			this.plannerPanel.Controls.Add(m_plannerControl);

			opacityPopup = new OpacityPopup();

			Project.resetGpsBabelWrapperExecutable();

			this.aboutMenuItem.Text = "&About " + Project.PROGRAM_NAME_HUMAN;

			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseWheel);

			LibSys.StatusBar.setInterface(statusBar, statusBar2, mainProgressBar);
			LibSys.StatusBar.WriteLine("GUI up");
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{
			LibSys.StatusBar.WriteLine("On Load");

			this.hintsLabel.Text = "Mouse tips:\n\n"
									+ "    double click - to recenter\n"
									+ "    drag and click inside - to zoom\n"
									+ "    right click on map for more options\n"
									+ "    press F1 for Help\n"
									+ "    check out Help->Sample Files menu\n";

#if !DEBUG
			//gridCheckBox.Visible = false;
			vehCheckBox.Visible = false;
#endif
			Project.favoritesFirstIndex = this.favMainMenuItem.MenuItems.Count;
			Project.recentFilesFirstIndex = this.fileMainMmenuItem.MenuItems.Count;
			Project.toolsFirstIndex = this.toolsMainMenuItem.MenuItems.Count;

			backLinkLabel.Enabled = false;
			forwardLinkLabel.Enabled = false;

			ProgressMonitor.Indicator = progressButton;
			progressButton.Text = "";

			this.Text = Project.PROGRAM_NAME_HUMAN;

			//this.hintsPanel.BackColor = System.Drawing.Color.FromArgb(255, Color.Navy);

			if(m_firstTime)
			{
				this.notAssociatedButton.Visible = true;
				this.notAssociatedLabel.Visible = true;
			}
			else
			{
				// first time the greeting does not start, so HideGreeting will not set hintsPanel to visible
				this.hintsPanel.Visible = Project.showHelpAtStart && !hintsPanelClicked;
			} 

			// good place to have camera coordinates set here, if supplied in command line:
			//        /lat=34.123 /lon=-117.234 /elev=5000 /map=aerial (topo,color)
			processArgs();

			// we need it as early as possible, before first web load - TileCache.init(Project.CGIBINPTR_URL) below - occured.
			Project.ApplyGlobalHTTPProxy(false);

			if(Project.mainFormMaximized)
			{
				this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			}
			else if(Project.fitsScreen(Project.mainFormX, Project.mainFormY, Project.mainFormWidth, Project.mainFormHeight)
				&& Project.mainFormWidth > 300 && Project.mainFormHeight > 200) 
			{
				this.Location = new Point(Project.mainFormX, Project.mainFormY);
				this.ClientSize = new System.Drawing.Size(Project.mainFormWidth, Project.mainFormHeight);		// causes Resize()
				//this.Bounds = new System.Drawing.Rectangle(Project.mainFormX, Project.mainFormY, Project.mainFormWidth, Project.mainFormHeight);
			}

			reliefCheckBox.Checked = Project.drawRelief;
			overlayCheckBox.Checked = Project.terraUseOverlay;

			gridCheckBox.Enabled = Project.drawRelief || Project.drawTerraserver;
			citiesCheckBox.Enabled = Project.drawRelief;
			gridCheckBox.Checked = Project.drawGrid;
			if(Project.drawTerraserver)
			{
				if(Project.drawTerraserverMode.Equals("aerial"))
				{
					aerialCheckBox.Checked = true;
				}
				else if(Project.drawTerraserverMode.Equals("color aerial"))
				{
					colorAerialCheckBox.Checked = true;
				}
				else if(Project.drawTerraserverMode.Equals("topo"))
				{
					topoCheckBox.Checked = true;
					overlayCheckBox.Enabled = false;
				}
				lmCheckBox.Enabled = true;
			}
			else
			{
				Project.terraserverAvailable = false;
				aerialCheckBox.Checked = false;
				colorAerialCheckBox.Checked = false;
				topoCheckBox.Checked = false;
				lmCheckBox.Enabled = false;
			}
			//opacityPanel.Visible = Project.drawTerraserver;
			citiesCheckBox.Checked = Project.drawCities;
			lmCheckBox.Checked = Project.drawLandmarks;
			eqCheckBox.Checked = Project.drawEarthquakes;
			eqStylePanelVisible(Project.drawEarthquakes);
			wptCheckBox.Checked = Project.drawWaypoints;
			vehCheckBox.Checked = Project.drawVehicles;

			TileCache.init(Project.CGIBINPTR_URL);		// try to reach the QuakeMap server

			if(TileCache.ZipcodeServer == null)
			{
				setInternetAvailable(false);

				this.hintsLabel.Text = "\n[offline]\n\nuse Internet-->Offline\n   or Map-->Options-->Proxy menu\nif you think you should be connected\n ";

				string message = "\nWarning: couldn't reach server: " + Project.PROGRAM_MAIN_SERVER	+ "\n\nWorking offline.\n ";
				if(m_greetingForm == null)
				{
					Project.ErrorBox(null, message);
				}
				else
				{
					Point popupOffset = new Point(34, 230);
					Point screenPoint = m_greetingForm.PointToScreen(popupOffset);
					Project.ShowPopup (m_greetingForm, message, screenPoint);
				}
			}
			else
			{
				setInternetAvailable(true);
			}

			m_cameraTrack = new CameraTrack();
			m_cameraManager = new CameraManager(this, m_cameraTrack);

			m_pictureManager = new PictureManager(mainPictureBox, this, m_cameraTrack);

			m_cameraManager.init(m_pictureManager);
			m_pictureManager.init(m_cameraManager);

			if(Project.hasOldTerrafolderStyleFiles())
			{
				HideGreeting();
				Project.AttemptTerrafolderMigration();
			}

			findLinkLabel.Focus();

			// on the very first run, bring up "Find by zipcode" window and try positioning 
			// the map there:
			if(m_firstTime && TileCache.ZipcodeServer != null) 
			{
                Project.findKeyword = Project.zipcode;
				FindForm findForm = new FindForm(m_cameraManager, true);
				findForm.ShowDialog();
			}

			LibSys.StatusBar.WriteLine("IP: running init thread");
			//add dt worker method to the thread pool / queue a task 
			//Project.threadPool.PostRequest (new WorkRequestDelegate (runInit), "MainForm init"); 
			//ThreadPool2.QueueUserWorkItem(new WaitCallback (runInit), "MainForm init"); 
            runInit("MainForm init");

			//ThreadPool2.QueueUserWorkItem(new WaitCallback (timeEater), "MainForm timeEater"); 

			periodicMaintenanceTimer = new System.Windows.Forms.Timer();
			periodicMaintenanceTimer.Interval = 1200;
			periodicMaintenanceTimer.Tick += new EventHandler(periodicMaintenance);
			periodicMaintenanceTimer.Start();
			LibSys.StatusBar.WriteLine("OK: Maintenance ON");

			if(!Project.showHelpAtStart)
			{
				if(!hasShownUpdateForm && Project.upgradeMessage.Length > 0)
				{
					hasShownUpdateForm = true;
					new DlgUpgradeMessage(Project.upgradeMessage).ShowDialog();
				}
			}

			inResize = false;

			LibSys.StatusBar.WriteLine("On Load done");
		}

		/*
		// debugging ThreadPool2
		private void timeEater(object state)
		{
			LibSys.StatusBar.WriteLine("timeEater started");
			int i = 0;
			while(true)
			{
				i++;
			}
		}
		*/

		// this method is run in a worker thread:
		//private void runInit(object state, DateTime requestEnqueueTime)
		private void runInit(object state)
		{
			LibSys.StatusBar.WriteLine("Initializing");
			//Thread.Sleep(1000);	// it somehow goes smoother when we sleep here. frame and buttons appear faster.

			double elev = m_cameraManager.Location.Elev;	// meters
			double elevKm = elev / 1000.0d;
			setScrollBarValue(elevKm);
			setCameraAltitudeTextBoxValue(elevKm);
			//m_cameraManager.Elev = elev;

			if(Project.eqFetchOnStart && Project.upgradeMessage.Length == 0 && Project.serverAvailable)
			{
				LibSys.StatusBar.WriteLine("Progress monitor UP - fetching eq info");
				progressFormTimer.Interval = 3000;
				progressFormTimer.Tick += new EventHandler(RunProgressMonitorForm);
				progressFormTimer.Start();
			}

			// at this moment camera manager has location (after init()) but has not yet calculated it's 
			// coverage etc. Make sure it has a chance to do so:
			m_cameraManager.ProcessCameraMove();

			switch(Project.earthquakeStyle)
			{
				default:
				case Earthquake.STYLE_DOT:
					radioButton1.Checked = true;
					break;
				case Earthquake.STYLE_CIRCLES:
					radioButton2.Checked = true;
					break;
				case Earthquake.STYLE_FILLCIRCLES:
					radioButton3.Checked = true;
					break;
				case Earthquake.STYLE_CONCENTRICCIRCLES:
					radioButton4.Checked = true;
					break;
				case Earthquake.STYLE_SQUARES:
					radioButton5.Checked = true;
					break;
				case Earthquake.STYLE_FILLSQUARES:
					radioButton6.Checked = true;
					break;
				case Earthquake.STYLE_TRIANGLE:
					radioButton7.Checked = true;
					break;
			}

			mainPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(m_pictureManager.LayersManager.Paint);
			mainPictureBox.Resize += new System.EventHandler(m_pictureManager.PictureManager_Resize);
			statusBar.Click += new System.EventHandler(LibSys.StatusBar.statusBar_Click);

			// Make the list of files marked for persistent loading:
			// create a delegate to pass to processor:
			InsertWaypoint insertWaypoint = new InsertWaypoint(WaypointsCache.insertWaypoint);
			InsertEarthquake insertEarthquake = new InsertEarthquake(EarthquakesCache.insertEarthquake);
		formatAgain:
			foreach(FormattedFileDescr ffd in Project.FileDescrList)
			{
				BaseFormat format = AllFormats.formatByName(ffd.formatName);
				if(format != null)
				{
					format.InsertWaypoint = insertWaypoint;
					format.InsertEarthquake = insertEarthquake;
					Project.FileList.Add(new FileListStruct(ffd.filename, ffd.formatName, new FormatProcessor(format.process), true));
				}
				else
				{
					// something wrong got stuck there. Remove it.
					Project.FileDescrList.Remove(ffd);
					goto formatAgain;
				}
			}

			// now see if any files were dropped on our icon (or supplied as cmd line args):
			if(m_args != null && m_args.Length > 0)
			{
				foreach (string arg in m_args)
				{
					if(arg != null)
					{
						if(arg.StartsWith("/"))
						{
							Project.ErrorBox(this, "Invalid option \"" + arg + "\"");
						}
						else
						{
							try 
							{
								if(File.Exists(arg))
								{
									string fileNameFull = Project.GetLongPathName(new FileInfo(arg).FullName);	// make sure we are not dealing with 8.3 notation here

									BaseFormat format = AllFormats.formatByFileName(fileNameFull);
									if(format != null)
									{
										string formatName = AllFormats.formatNameByFileName(fileNameFull);
										format.InsertWaypoint = insertWaypoint;
										format.InsertEarthquake = insertEarthquake;
										Project.FileList.Add(new FileListStruct(fileNameFull, formatName, new FormatProcessor(format.process), false));
									}
									else
									{
										Project.ErrorBox(this, "Unrecognized format:\n\n" + fileNameFull);
									}
								}
								else
								{
									Project.ErrorBox(this, "Non-existent file:\n\n" + arg);
								}
							}
							catch
							{
								Project.ErrorBox(this, "Error processing file:\n\n" + arg);
							}
						}
					}
				}
			}

			FileAndZipIO.readPersistentAndDroppedFiles(!m_hadLonLatArgs, initialMessageLabel);	// dropped on icon or command line arguments

			rebuildFavoritesMenu();

			loadSetDlgIcon();

			// default page layout for printing:
			m_pgSettings.Landscape = true;
			m_pgSettings.Margins.Left = 0;
			m_pgSettings.Margins.Right = 0;
			m_pgSettings.Margins.Top = 0;
			m_pgSettings.Margins.Bottom = 0;
			mainPrintDocument.DefaultPageSettings = m_pgSettings;

			EarthquakesCache.goFetch = Project.eqFetchOnStart;
			//add dt worker method to the thread pool; queue a task that will run forever:
			//Project.threadPool.PostRequest (new WorkRequestDelegate (EarthquakesCache.Run), "fetch eq"); 
			ThreadPool2.QueueUserWorkItem (new WaitCallback (EarthquakesCache.Run), "fetch eq"); 

			WaypointsCache.RefreshWaypointsDisplayed();
			if(!Project.eqFetchOnStart)		// or it will be refreshed when the fetching is complete.
			{
				EarthquakesCache.RefreshEarthquakesDisplayed();
			}
			else if(Project.drawEarthquakes)
			{
				initialMessageLabel.Text = "...fetching earthquakes...";
			}

			if(Project.drawLandmarks)
			{
				initLandmarkService();

				warnWsOff();
			}

			initialMessageLabel.Size = new Size(1,1);
			initialMessageLabel.Visible = false;
			LibSys.StatusBar.WriteLine("Ready");

			/*
			for(double ddd=2.0d; ddd < 1000.0d ;ddd *= 2)
			{
				LibSys.StatusBar.WriteLine("ddd=" + ddd + " m/sec");
				Speed dSpeed = new Speed(ddd * 3600.0d);
				LibSys.StatusBar.WriteLine("      FEET: " + dSpeed.ToString(Distance.UNITS_DISTANCE_FEET));
				LibSys.StatusBar.WriteLine("      KM: " + dSpeed.ToString(Distance.UNITS_DISTANCE_KM));
				LibSys.StatusBar.WriteLine("      M: " + dSpeed.ToString(Distance.UNITS_DISTANCE_M));
				LibSys.StatusBar.WriteLine("      MILES: " + dSpeed.ToString(Distance.UNITS_DISTANCE_MILES));
				LibSys.StatusBar.WriteLine("      NMILES: " + dSpeed.ToString(Distance.UNITS_DISTANCE_NMILES));
			}
			*/
		}

		private bool m_hadLonLatArgs = false;

		private void processArgs()
		{
			// process arguments from the command line:  /lat=34.123 /lon=-117.234 /elev=5000 /map=aerial (topo,color)
			for(int i=0; i < m_args.Length ;i++)
			{
				string arg = m_args[i];

				if(arg != null)
				{
					this.hintsPanel.Visible = false;
					try
					{
						if(arg.ToLower().StartsWith("/lat="))
						{
							double lat = Convert.ToDouble(arg.Substring(5));
							if(lat < 90.0d && lat > -90.0d)
							{
								Project.cameraLat = lat;
								m_hadLonLatArgs = true;
							}
							m_args[i] = null;
						}
						else if(arg.ToLower().StartsWith("/lon="))
						{
							double lon = Convert.ToDouble(arg.Substring(5));
							if(lon < 180.0d && lon > -180.0d)
							{
								Project.cameraLng = lon;
								m_hadLonLatArgs = true;
							}
							m_args[i] = null;
						}
						else if(arg.ToLower().StartsWith("/elev="))
						{
							double elev = Convert.ToDouble(arg.Substring(6));
							if(elev < Project.CAMERA_HEIGHT_MAX * 1000.0d && elev > Project.CAMERA_HEIGHT_MIN_DEFAULT * 1000.0d)
							{
								Project.cameraElev = elev;
								m_hadLonLatArgs = true;
							}
							m_args[i] = null;
						}
						else if(arg.ToLower().StartsWith("/map="))
						{
							string mode = arg.Substring(5);
							switch(mode)
							{
								case "aerial":
								case "topo":
									Project.drawTerraserverMode = mode;
									break;
								case "color":
									Project.drawTerraserverMode = "color aerial";
									break;
							}
							m_args[i] = null;
						}
						else if(arg.ToLower().StartsWith("/quick"))		// accelerated start for GPSBabelWrapper
						{
							Project.mainFormMaximized = false;
							KEEP_GREETING_UP_SECONDS = 0;
							m_args[i] = null;
						}
					}
					catch (Exception ee)
					{
						m_args[i] = null;
						string message = "argument '" + arg + "' - wrong format (" + ee.Message + ")";
						LibSys.StatusBar.Error(message);
						Project.ErrorBox(null, message);
					}
				}
			}
		}

		#endregion

		#region Helpers and Maintenance

		private System.Windows.Forms.Timer progressFormTimer = new System.Windows.Forms.Timer();

		private void RunProgressMonitorForm(object obj, System.EventArgs args)
		{
			progressFormTimer.Stop();	// it is a one-time deal, stop the timer

			if(ProgressMonitor.CountActive() > 0) 
			{
				ProgressMonitorForm.BringFormUp(true);

				//new ProgressMonitorForm(true).ShowDialog();		// .Show() causes hanging of the form
				//ProgressMonitorForm.BringFormUp(true);	// this would hang because of Show()
			}
		}

		private void setInternetAvailable(bool on)
		{
			Project.serverAvailable = on;
			if(on)
			{
				// connected, enable some Internet-related functions: 
				LibSys.StatusBar.WriteLine("Working online");
				this.updateMenuItem.Enabled = true;
				this.findLinkLabel.Enabled = true;
				this.fetchEqMenuItem.Enabled = true;
				this.streetMap1MenuItem.Enabled = true;
				this.nearestGeocaches1MenuItem.Enabled = true;
				this.fetchEqMenuItem.Enabled = true;
				this.fetchEq1MenuItem.Enabled = true;
				this.streetMapMenuItem.Enabled = true;
				this.nearestGeocachesMenuItem.Enabled = true;
				this.streetMap1MenuItem.Enabled = true;
				this.scaleTextBox.BackColor = Color.White;
			}
			else
			{
				// disconnected, disable some Internet-related functions: 
				LibSys.StatusBar.WriteLine("Working offline");
				this.updateMenuItem.Enabled = false;
				this.findLinkLabel.Enabled = false;
				this.fetchEqMenuItem.Enabled = false;
				this.streetMap1MenuItem.Enabled = false;
				this.nearestGeocaches1MenuItem.Enabled = false;
				this.fetchEqMenuItem.Enabled = false;
				this.fetchEq1MenuItem.Enabled = false;
				this.streetMapMenuItem.Enabled = false;
				this.nearestGeocachesMenuItem.Enabled = false;
				this.streetMap1MenuItem.Enabled = false;
				this.scaleTextBox.BackColor = Color.Yellow;
			}
		}

		private System.Windows.Forms.Timer periodicMaintenanceTimer = null;

		public void periodicMaintenance(object obj, System.EventArgs args)
		{
			try
			{
				//LibSys.StatusBar.WriteLine("...maintenance ticker...");
			
				// ensure that we always have reftreshed picture:
				//if(!Project.goingDown && !Project.inhibitRefresh && (Project.pictureDirty || periodicMaintenanceCounter % 10 == 0))
				if(!Project.goingDown && !Project.inhibitRefresh && (Project.pictureDirty || Project.pictureDirtyCounter > 0))
				{
					m_pictureManager._Refresh();
					Project.pictureDirty = false;
					if(Project.pictureDirtyCounter > 0)
					{
						Project.pictureDirtyCounter--;
					}
				}
				if((DateTime.Now - modifierPressedTime).Seconds > 5)
				{
					// avoid sticky modifiers, happens when you release ctrl, alt or shift outside the window
					modifierPressedTime = DateTime.MaxValue;
					controlDown = false;
					shiftDown = false;
					altDown = false;
				}
				Project.popupMaintenance();

				if(opacityPopup.Visible)
				{
					opacityPopup.tick();
				}
			} 
			catch {}

			periodicMaintenanceTimer.Enabled = true;
		}

		private void eqStylePanelVisible(bool visible)
		{
			eqStylePanel.Width = visible ? 29 : 1;
		}


		private void loadSetDlgIcon()
		{
			// try loading icon file (will be used for Project.setDlgIcon() for all dialogs):
			string iconFileName = Project.GetMiscPath(Project.PROGRAM_NAME_HUMAN + ".ico");
			try 
			{
				if(!File.Exists(iconFileName))
				{
					// load the file remotely, don't hold diagnostics if load fails:
					string url = Project.MISC_FOLDER_URL + "/" + Project.PROGRAM_NAME_HUMAN + ".ico";
					DloadProgressForm loaderForm = new DloadProgressForm(url, iconFileName, false, true);
					loaderForm.ShowDialog();
				}
				this.Icon = new Icon(iconFileName);
			} 
			catch {}
		}

		public void doDisableProgram(int reason)
		{
			this.Close();
			this.Dispose();

			new DlgDisableProgram(reason).ShowDialog();

			Project.Exit();
		}

		private void saveState()
		{
			Project.SaveOptions();
			TerraserverCache.saveState();
		}

		private void showHelp()
		{
			string fileName = Project.GetMiscPath(Project.HELP_FILE_PATH);

			if(File.Exists(fileName)
				&& (!Project.serverAvailable || File.GetLastWriteTime(fileName).CompareTo(Project.HELP_FILE_DATE) > 0))
			{
				Help.ShowHelp(this, fileName);
			} 
			else if(Project.serverAvailable)
			{
				// load the file remotely:
				DloadProgressForm loaderForm = new DloadProgressForm(Project.HELP_FILE_URL, fileName, true, false);
				loaderForm.ShowDialog();
			} 
			else
			{
				Project.ErrorBox(this, "Need to connect to the Internet to download Help file");
			}
		}

		private void gotoLocation()
		{
			GotoForm gotoForm = new GotoForm(m_cameraManager);
			gotoForm.ShowDialog();
		}

		public void doForward()
		{
			try
			{
				CamPos camPos = (CamPos)Project.cameraPositionsFwdStack.Pop();
				m_cameraManager.SpoilPicture();
				if(camPos.Type != null && camPos.Type.Length > 0)
				{
					Project.drawTerraserverMode = camPos.Type;
					setTerraCheckboxes();
				}
				m_cameraManager.Location = new GeoCoord(camPos);	// calls ProcessCameraMove()
			}
			catch (InvalidOperationException exc)
			{
				// empty stack
				//LibSys.StatusBar.Error("no more history stored");
			}
		}

		public void doBack()
		{
			try
			{
				// remove the current location, so that we can keep moving back:
				Project.cameraPositionsFwdStack.Push(Project.cameraPositionsBackStack.Pop());

				CamPos camPos = (CamPos)Project.cameraPositionsBackStack.Pop();
				m_cameraManager.SpoilPicture();
				if(camPos.Type != null && camPos.Type.Length > 0)
				{
					Project.drawTerraserverMode = camPos.Type;
					setTerraCheckboxes();
				}
				m_cameraManager.Location = new GeoCoord(camPos);	// calls ProcessCameraMove()
			}
			catch(InvalidOperationException exc)
			{
				// empty stack
				//LibSys.StatusBar.Error("no more history stored");
			}
		}

		private void initLandmarkService()
		{
			if(TerraserverCache.ls == null)
			{
				Cursor.Current = Cursors.WaitCursor; 
				TerraserverCache.initLandmarkService();
				Cursor.Current = Cursors.Default; 
			}
		}

		#endregion

		#region Greeting form

		private Form m_greetingForm = null;
		private DateTime m_greetingFormStarted = DateTime.Now;
		private int KEEP_GREETING_UP_SECONDS = 5;
		private bool hasShownUpdateForm = false;

		private void ShowGreeting()
		{
			m_greetingFormStarted = DateTime.Now;
			m_greetingForm = new DlgGreeting(m_warning, m_greeting);
			TileCache.greetingForm = m_greetingForm;
			if(KEEP_GREETING_UP_SECONDS > 0)
			{
				m_greetingForm.ShowDialog();
			}
		}

		private void HideGreeting()
		{
			TimeSpan sinceUp = DateTime.Now - m_greetingFormStarted;
			int toSleep = (Project.upgradeMessage.Length == 0 ? KEEP_GREETING_UP_SECONDS : 3) - sinceUp.Seconds; 
			if(toSleep > 0 && !Project.goingDown)
			{
				hideGreetingTimer.Interval = toSleep * 1000;
				hideGreetingTimer.Tick += new EventHandler(_hideGreeting2);
				hideGreetingTimer.Start();
			}
			else
			{
				try 
				{
					if(this.InvokeRequired)
					{
						this.Invoke(new MethodInvoker(_hideGreeting));
					} 
					else
					{
						_hideGreeting();
					}
				}
				catch {}
			}
		}
		
		private System.Windows.Forms.Timer hideGreetingTimer = new System.Windows.Forms.Timer();

		private void _hideGreeting2(object obj, System.EventArgs args)
		{
			hideGreetingTimer.Stop();	// it is a one-time deal, stop the timer
			_hideGreeting();
		}

		private void _hideGreeting()
		{
			/*
			if(m_daysSinceInstall >= Project.SPOILER_DAYS_START && !MyLicense.unlocked)
			{
				//new DlgExplainSpoiling().Show();
			}
			*/

			//this.Focus();
			//this.BringToFront();	// main form to front

			TileCache.greetingForm = null;
			try
			{
				if(m_greetingForm != null)
				{
					//m_greetingForm.Hide();
					m_greetingForm.Close();
					m_greetingForm = null;
				}
				if(m_greetingThread != null)
				{
					Thread.Sleep(500);		// let the dialog close before we abort the thread
					m_greetingThread.Abort();
					m_greetingThread = null;
				}
			} 
			catch
			{
			}

			this.hintsPanel.Visible = Project.showHelpAtStart && (m_args.Length == 0) && !hintsPanelClicked;

			try
			{
				bool allAssociated;
				string openCmd = Project.GetLongPathName(Application.ExecutablePath) + " \"%1\"";

				FileAssociation FA = new FileAssociation();
				FA.Extension = "loc";
				FA.AddCommand("open", openCmd);

				allAssociated = FA.Test();

				if(allAssociated)
				{
					FA = new FileAssociation();
					FA.Extension = "gpx";
					FA.AddCommand("open", openCmd);

					allAssociated = FA.Test();
				}

				if(allAssociated)
				{
					FA = new FileAssociation();
					FA.Extension = "gpz";
					FA.AddCommand("open", openCmd);

					allAssociated = FA.Test();
				}

				this.notAssociatedButton.Visible = !allAssociated;
				this.notAssociatedLabel.Visible = !allAssociated;
			} 
			catch {}


			if(!hasShownUpdateForm && Project.upgradeMessage.Length > 0)
			{
				hasShownUpdateForm = true;
				new DlgUpgradeMessage(Project.upgradeMessage).ShowDialog();
			}
			else
			{
				//Thread.Sleep(1000);
				this.Focus();
				this.BringToFront();	// main form to front
			}
		}
		#endregion

		#region Printing

		private void mainPrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs ppe)
		{
			/* ---------------------------------
			Rectangle pageBounds = ppe.PageBounds;		// whole page
			LibSys.StatusBar.Trace("IP: page bounds: " + pageBounds);
			ppe.Graphics.DrawRectangle(Pens.Cyan, pageBounds);

			Rectangle physicalBounds = getPhysicalBounds(ppe);		// right from the printer
			LibSys.StatusBar.Trace("IP: physical bounds: " + physicalBounds);
			ppe.Graphics.DrawRectangle(Pens.Yellow, physicalBounds);

			Rectangle marginBounds = ppe.MarginBounds;	// printable area of the page
			LibSys.StatusBar.Trace("IP: margin bounds: " + marginBounds);
			ppe.Graphics.DrawRectangle(Pens.Red, marginBounds);
			marginBounds = physicalBounds;
			//marginBounds = new Rectangle(marginBounds.X, marginBounds.Y, 600, 400);
			LibSys.StatusBar.Trace("IP: printing clip rect: " + marginBounds);

			marginBounds = Rectangle.Inflate(physicalBounds, -20, -20);
			Rectangle frameRect = marginBounds;
			ppe.Graphics.DrawRectangle(Pens.Green, frameRect);
			
			// the bounds we fit printed picture to are same as window bounds:
			Rectangle marginBoundsPict = new Rectangle(marginBounds.X, marginBounds.Y, m_pictureManager.Width, m_pictureManager.Height);

			// center of the printout must be the center of the picture:
			int offsetX = (marginBoundsPict.Width - marginBounds.Width) / 2;
			int offsetY = (marginBoundsPict.Height - marginBounds.Height) / 2;
			marginBoundsPict.Offset(-offsetX, -offsetY);
			//ppe.Graphics.DrawRectangle(Pens.Blue, marginBoundsPict);

			//frameRect = Rectangle.Inflate(marginBoundsPict, 3, 3);
			//ppe.Graphics.DrawRectangle(Pens.Green, frameRect);
			
			// the bounds we fit printed picture to are same as window bounds:
			Rectangle marginBoundsPict = marginBounds; //new Rectangle(marginBounds.X, marginBounds.Y, m_pictureManager.Width, m_pictureManager.Height);

			// center of the printout must be the center of the picture:
			int offsetX = (marginBoundsPict.Width - marginBounds.Width) / 2;
			int offsetY = (marginBoundsPict.Height - marginBounds.Height) / 2;
			marginBoundsPict.Offset(-offsetX, -offsetY);

			ppe.Graphics.Clip = new Region(marginBounds);
			// usually marginBoundsPict is bigger than marginBounds. Their center points coincide.
			//m_pictureManager.printPicture(ppe, marginBoundsPict);
			--------------------------------- */

			Rectangle physicalBounds = getPhysicalBounds(ppe);		// right from the printer
			//LibSys.StatusBar.Trace("IP: physical bounds: " + physicalBounds);
			//ppe.Graphics.DrawRectangle(Pens.Yellow, physicalBounds);
			Graphics graphics = ppe.Graphics;

			doPrintPage(physicalBounds, graphics, false);
		}

		// implements IMainCommand interface:
		public string doGenerateClickMap(Rectangle physicalBounds)
		{
			return new ClickMapGenerator(physicalBounds, new Rectangle(0, 0, PictureManager.This.Width, PictureManager.This.Height)).generateClickMap();
		}

		// implements IMainCommand interface:
		public void doPrintPage(Rectangle physicalBounds, Graphics graphics, bool webStyle)
		{
			int margin = webStyle ? 0 : 3;
			Rectangle marginBounds = Rectangle.Inflate(physicalBounds, -margin, -margin);
			Rectangle frameRect = Rectangle.Inflate(marginBounds, 2, 2);
			if(!webStyle)
			{
				graphics.DrawRectangle(Pens.Green, frameRect);
			}

			string labelPrint;
			Distance d = new Distance(m_cameraManager.Elev);
			string locLabel = "" + m_cameraManager.Location.ToString() + " / " + d;
			if(TileSetTerra.PlaceDescription.Length > 0)
			{
				labelPrint = this.Text + " @ " + locLabel;
			}
			else
			{
				labelPrint = Project.PROGRAM_NAME_HUMAN + " - view at " + locLabel;
			}
			int fontSize = 6;
			Font font = Project.getLabelFont(fontSize);
			SizeF size = graphics.MeasureString(labelPrint, font);
			int labelWidth = (int)(size.Width); 

			int x = marginBounds.X + marginBounds.Width - labelWidth;
			int y = frameRect.Y + 1;
			graphics.DrawString(labelPrint, font, Brushes.Black, x, y);

			if(marginBounds.Width - labelWidth > 200)
			{
				x = marginBounds.X + 5;
				y = frameRect.Y + 1;
				graphics.DrawString(Project.WEBSITE_NAME_HUMAN, font, Brushes.Red, x, y);
			}

			marginBounds = Rectangle.Inflate(physicalBounds, -margin, -margin - fontSize + 1);
			marginBounds.Offset(0, fontSize - 1);

			graphics.Clip = new Region(marginBounds);
			m_pictureManager.printPicture(graphics, marginBounds);
			printWatermark(graphics, marginBounds);
			//ppe.Graphics.DrawRectangle(Pens.Red, marginBounds);
		}

		private void printWatermark(Graphics graphics, Rectangle marginBounds)
		{
			//LibSys.StatusBar.Trace("IP: printing marginBounds: " + marginBounds);
		}
		#endregion

		#region Printer page bounds
		/*
			http://www.fawcette.com/vsm/2002_11/magazine/features/eaton/default_pf.asp
			The .NET Framework 1.0 can't retrieve a printer's "hard" margins (the outermost area of the page
			it can actually print to). However, PrintPageEventArgs lets you retrieve something close with the
			MarginBounds property. Unfortunately, MarginBounds doesn't take into account the hard margins,
			so your output might not end up exactly where you expect. You must resort to P/Invoke and calling
			the Win32 GetDeviceCaps function to retrieve the printer's hard margins (see Listing 3).
			After you retrieve the hard margins, you can begin creating the Rectangle objects and printing
			the information on your report 
		 */
		private void createPrintingAreas()
		{
			/*
			// create the header area:
			int headerHeight = hf.GetHeight(ev.Graphics);
			RectangleF header = new RectangleF(leftMargin, topMargin, pageWidth, headerHeight);

			// create the footer
			int bodyFontHeight = bodyFont.GetHeight(ev.Graphics);
			RectangleF footer = new RectangleF(leftMargin, body.Bottom, pageWidth, bodyFontHeight);
			// create the body section
			RectangleF body = new RectangleF(leftMargin, header.Bottom, pageWidth, pageHeight - bodyFontHeight);
			*/
		}

		// returns maximal printable area of the physical device, basically paper boundaries:
		private Rectangle getPhysicalBounds(System.Drawing.Printing.PrintPageEventArgs ppe)
		{
			IntPtr hdc = new IntPtr();
			hdc = ppe.Graphics.GetHdc();
			marginInfo((int)hdc);
			ppe.Graphics.ReleaseHdc(hdc);

			//Rectangle ret = new Rectangle((int)_leftMargin, (int)_topMargin,
			//								(int)(_rightMargin - _leftMargin), (int)(_bottomMargin - _topMargin));

			Rectangle ret = new Rectangle(0, 0,
				(int)(_rightMargin - _leftMargin) - 1, (int)(_bottomMargin - _topMargin) - 1);
			
			return ret;
		}

		[DllImport("gdi32.dll")]
		private static extern Int16 GetDeviceCaps([In] 
			[MarshalAs (UnmanagedType.U4)] int hDc, [In] [MarshalAs (UnmanagedType.U2)] Int16 funct);

		private float _leftMargin = 0;
		private float _topMargin = 0;
		private float _rightMargin = 0;
		private float _bottomMargin = 0;

		const short HORZSIZE      = 4;
		const short VERTSIZE      = 6;
		const short HORZRES       = 8;
		const short VERTRES       = 10;
		const short PHYSICALOFFSETX = 112;
		const short PHYSICALOFFSETY = 113;
		public void marginInfo(int deviceHandle) 
		{
			float offx = Convert.ToSingle(
				GetDeviceCaps(deviceHandle, 
				PHYSICALOFFSETX));
			float offy = Convert.ToSingle(
				GetDeviceCaps(deviceHandle, 
				PHYSICALOFFSETY));
			float resx = Convert.ToSingle(
				GetDeviceCaps(deviceHandle, HORZRES));
			float resy = Convert.ToSingle(
				GetDeviceCaps(deviceHandle, VERTRES));
			float hsz = Convert.ToSingle(
				GetDeviceCaps(deviceHandle, HORZSIZE))/25.4f;
			float vsz = Convert.ToSingle(
				GetDeviceCaps(deviceHandle,VERTSIZE))/25.4f;
			float ppix = resx/hsz;
			float ppiy = resy/vsz;
			_leftMargin  = (offx/ppix) * 100.0f;
			_topMargin   = (offy/ppix) * 100.0f;
			_bottomMargin  = _topMargin + (vsz * 100.0f);
			_rightMargin  = _leftMargin + (hsz * 100.0f);
		}
		#endregion

		#region Drag and Drop processing

		private void MainForm_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor; 
			this.Focus();
			this.BringToFront();

			try 
			{
				string[] fileNames = (string[])e.Data.GetData("FileDrop");

				bool anySuccess = FileAndZipIO.readFiles(fileNames);

				if(anySuccess)	// we need to zoom into whole set of dropped files, or refresh in case of JPG
				{
					this.wptEnabled(true);
					if(!m_cameraManager.zoomToCorners())
					{
						m_cameraManager.ProcessCameraMove();	// nowhere to zoom in - just refresh
					}
				} 
				else if(fileNames.GetLength(0) == 1 && fileNames[0].ToLower().EndsWith(".jpg"))
				{
					DlgPhotoManager dlg = new DlgPhotoManager(2);
					dlg.TopMost = true;
					dlg.ShowDialog();
				}
			} 
			catch
			{
				LibSys.StatusBar.Trace("* Error: drag-and-drop... cannot accept file(s)");
			}
			e.Effect = DragDropEffects.None;
			Cursor.Current = Cursors.Default; 
		}

		private void MainForm_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			this.hintsPanel.Visible = false;
			//string[] formats = e.Data.GetFormats();
			if (e.Data.GetDataPresent("FileDrop"))		// (DataFormats.Text)) 
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void MainForm_DragLeave(object sender, System.EventArgs e)
		{
			//e.Effect = DragDropEffects.None;
		}
		#endregion

		#region Favorites related code

		private void addToFavoritesMenuItem_Click(object sender, System.EventArgs e)
		{
			CamPos camPos = new CamPos(m_cameraManager.Location.Lng, m_cameraManager.Location.Lat, m_cameraManager.Location.Elev);
			string name  = "new item - " + this.favMainMenuItem.MenuItems.Count;

			// try to be smart with the new name, get the closest waypoint or city name:
			double searchRadius = m_cameraManager.Location.Elev / 5;

			SortedList closest = m_pictureManager.getCloseObjects(m_cameraManager.Location, searchRadius);

			if(closest.Count > 0)
			{
				LiveObject lo = (LiveObject)closest.GetByIndex(0);
				if(lo.Name.Length > 0)
				{
					name = lo.Name;
				}
			}
			camPos.Name = name;
			camPos.Type = Project.drawTerraserverMode;

			new DlgFavoritesAddTo(camPos, closest, this.favMainMenuItem, new System.EventHandler(favoriteItemClick)).ShowDialog();
		}

		public void rebuildFavoritesMenu()
		{
			// clean out existing menu items:
			for(int i=favMainMenuItem.MenuItems.Count-1; i >= Project.favoritesFirstIndex ;i--)
			{
				favMainMenuItem.MenuItems.RemoveAt(i);
			}

			// rebuild from Project.favorites:
			foreach(CamPos camPos in Project.favorites)
			{
				MenuItem menuItem = new MenuItem();

				menuItem.Text = camPos.Name;
				menuItem.Click += new System.EventHandler(favoriteItemClick);
				menuItem.Index = favMainMenuItem.MenuItems.Count;

				favMainMenuItem.MenuItems.Add(menuItem);
			}
		}

		private void setTerraCheckboxes()
		{
			if(Project.drawTerraserver)
			{
				if(Project.drawTerraserverMode.Equals("aerial"))
				{
					aerialCheckBox.Checked = true;
					colorAerialCheckBox.Checked = false;
					topoCheckBox.Checked = false;
				}
				else if(Project.drawTerraserverMode.Equals("color aerial"))
				{
					aerialCheckBox.Checked = false;
					colorAerialCheckBox.Checked = true;
					topoCheckBox.Checked = false;
				}
				else if(Project.drawTerraserverMode.Equals("topo"))
				{
					aerialCheckBox.Checked = false;
					colorAerialCheckBox.Checked = false;
					topoCheckBox.Checked = true;
				}
			}
		}

		private void favoriteItemClick(object sender, System.EventArgs e)
		{
			MenuItem menuItem = (MenuItem)sender;
			int index = menuItem.Index - Project.favoritesFirstIndex;
			CamPos camPos = (CamPos)Project.favorites[index];
			//LibSys.StatusBar.Trace("favoriteItemClick: item=" + menuItem.Index + " '" + menuItem.Text + "'  camPos='" + camPos.Name + "'");
			GeoCoord loc = new GeoCoord(camPos);
			m_cameraManager.SpoilPicture();
			if(camPos.Type != null && camPos.Type.Length > 0)
			{
				Project.drawTerraserverMode = camPos.Type;
				setTerraCheckboxes();
			}
			m_cameraManager.Location = loc;				// calls ProcessCameraMove()
		}

		private void organizeFavoritesMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgFavoritesOrganize(m_cameraManager).ShowDialog();
			rebuildFavoritesMenu();
			Project.SaveFavorites();
		}

		private void exportFavoritesMenuItem_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			
			saveFileDialog.InitialDirectory = Project.fileInitialDirectory;
			saveFileDialog.FileName = "";
			saveFileDialog.DefaultExt = ".gpx";
			saveFileDialog.AddExtension = true;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			saveFileDialog.Filter = "GPX files (*.gpx)|*.gpx|All files (*.*)|*.*";
			DialogResult result = saveFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				string filename = saveFileDialog.FileName;
				// could be "new DateTimeFormatInfo().UniversalSortableDateTimePattern;" - but it has space instead of 'T'

				try 
				{
					string seedXml = Project.SEED_XML
						+ "<gpx version=\"1.0\" creator=\"" + Project.PROGRAM_NAME_HUMAN + " " + Project.PROGRAM_VERSION_HUMAN + " - " + Project.WEBSITE_LINK_WEBSTYLE + "\"/>";

					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(seedXml);

					XmlNode root = xmlDoc.DocumentElement;

					Project.fillGpxRootNode(xmlDoc, root);	// fills in attributes so so that validation goes clean

					Project.SetValue(xmlDoc, root, "time", Project.localToZulu(DateTime.Now).ToString(Project.dataTimeFormat));

					/*
						* This is what EasyGPS produces, keep our format close enough:

						<wpt lat="33.591700000" lon="-117.699750000">
							<time>2003-03-02T00:00:00Z</time>
							<name>GCDD94</name>
							<desc>LHCC Cache by mulaguna, Traditional Cache (2/1)</desc>
							<url>http://www.geocaching.com/seek/cache_details.aspx?wp=GCDD94</url>
							<urlname>LHCC Cache by mulaguna</urlname>
							<sym>Geocache</sym>
							<type>Geocache</type>
						</wpt>
						*/
					for (int i = 0; i < Project.favorites.Count; i++)
					{
						CamPos wpt = (CamPos)Project.favorites[i];
						XmlNode wptNode = Project.SetValue(xmlDoc, root, "wpt", "");
						XmlAttribute attr = xmlDoc.CreateAttribute("lat");
						attr.InnerText = "" + wpt.Lat;
						wptNode.Attributes.Append(attr);
						attr = xmlDoc.CreateAttribute("lon");
						attr.InnerText = "" + wpt.Lng;
						wptNode.Attributes.Append(attr);

						string name = wpt.Name.Trim();

						Project.SetValue(xmlDoc, wptNode, "ele", "" + wpt.Elev);
						Project.SetValue(xmlDoc, wptNode, "name", name);
						Project.SetValue(xmlDoc, wptNode, "type", "Waypoint");
					}

					xmlDoc.Save(filename);
				}
				catch (Exception ee) 
				{
					LibSys.StatusBar.Error("Export favorites: " + ee.Message);
				}
			}
		}

		private void importFavoritesMenuItem_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();

			// use .gpx and .loc format:
			openFileDialog.InitialDirectory = Project.fileInitialDirectory;
			openFileDialog.FileName = "";
			openFileDialog.DefaultExt = ".gpx";
			openFileDialog.AddExtension = true;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "GPX files (*.gpx)|*.gpx";
			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				fvCount = 0;
				string fileName = openFileDialog.FileName;
				BaseFormat format = AllFormats.gpsFormatObjects[0];
				FileImportForm.m_selectedFormat = AllFormats.gpsFormatNames[0];
				format.InsertWaypoint = new InsertWaypoint(insertFavorite);
				FormatProcessor fileProcessor = new FormatProcessor(format.process);
				fileProcessor("", fileName, fileName);
				if(fvCount > 0)
				{
					rebuildFavoritesMenu();
					Project.SaveFavorites();		// save favorites
					Project.MessageBox(this, "" + fvCount + " items were imported successfully and will appear in Favorites menu");
				}
				else
				{
					Project.ErrorBox(this, "No items were imported from\r\n\r\n" + fileName + "\r\n\r\n - could not find any named waypoints there.");
				}
			}
		}

		private static int fvCount = 0;
 
		// called from fileProcessor:
		private static void insertFavorite(CreateInfo createInfo)
		{
			switch(createInfo.type)
			{
				case "trk":
				case "rte":
					return;
			}

			string name = ("" + createInfo.name).Trim();

			if(createInfo.urlName != null && createInfo.urlName.Length > 0)
			{
				name += " - " + createInfo.urlName.Trim();
			}
			else if(createInfo.desc != null && createInfo.desc.Length > 0)
			{
				name += " - " + createInfo.desc.Trim();
			}

			if(name.StartsWith(" - "))
			{
				name = name.Substring(3);
			}

			name = name.Replace("\r", " ").Replace("\n", " ").Trim().Replace("  ", " ");

			if(name != null && name.Length > 0)
			{
				CamPos favPos = new CamPos(createInfo.lng, createInfo.lat, createInfo.elev, name.Trim(), Project.drawTerraserverMode);

				Project.favorites.Add(favPos);
				fvCount++;
			}
		}

		#endregion

		#region Recent Files Menu related

		public void rebuildRecentFilesMenu()
		{
			// clean out existing menu items:
			for(int i=fileMainMmenuItem.MenuItems.Count-1; i >= Project.recentFilesFirstIndex ;i--)
			{
				fileMainMmenuItem.MenuItems.RemoveAt(i);
			}

			if(Project.recentFiles.Count > 0)
			{
				MenuItem menuItem = new MenuItem();

				menuItem.Text = "-";
				menuItem.Index = fileMainMmenuItem.MenuItems.Count;

				fileMainMmenuItem.MenuItems.Add(menuItem);
			}

			// rebuild from Project.recentFiles:
			foreach(string filePath in Project.recentFiles)
			{
				MenuItem menuItem = new MenuItem();
				if((new DirectoryInfo(filePath)).Exists)
				{
					menuItem.Text = filePath + "  [folder]";
				}
				else
				{
					menuItem.Text = filePath;
				}
				menuItem.Click += new System.EventHandler(recentFileItemClick);
				menuItem.Index = fileMainMmenuItem.MenuItems.Count;

				fileMainMmenuItem.MenuItems.Add(menuItem);
			}
		}

		private string recentFilePath = "";

		private void recentFileItemClick(object sender, System.EventArgs e)
		{
			MenuItem menuItem = (MenuItem)sender;
			int index = menuItem.Index - Project.recentFilesFirstIndex - 1;
			recentFilePath = (string)Project.recentFiles[index];
			//LibSys.StatusBar.Trace("recentFileItemClick: item=" + menuItem.Index + "  path='" + recentFilePath + "'");

			//Project.threadPool.PostRequest (new WorkRequestDelegate (processRecentFileItemClick), "ProcessRecentFileItemClick"); 
			ThreadPool2.QueueUserWorkItem (new WaitCallback (processRecentFileItemClick), "ProcessRecentFileItemClick"); 

			Cursor.Current = Cursors.WaitCursor;
			Project.ShowPopup(this.cameraHeightLabel, " \n... processing " + recentFilePath + " \n", Point.Empty);
		}

		//private void processRecentFileItemClick(object state, DateTime requestEnqueueTime)
		private void processRecentFileItemClick(object state)
		{
			try 
			{
				LibSys.StatusBar.Trace("* processing " + recentFilePath);

				string[] fileNames = new string[] { recentFilePath };

				bool anySuccess = FileAndZipIO.readFiles(fileNames);

				if(anySuccess)	// we need to zoom into whole set of dropped files, or refresh in case of JPG
				{
					this.wptEnabled(true);
					if(!m_cameraManager.zoomToCorners())
					{
						m_cameraManager.ProcessCameraMove();	// nowhere to zoom in - just refresh
					}
				}
				LibSys.StatusBar.Trace("* Ready");
			} 
			catch
			{
				LibSys.StatusBar.Trace("* Error: recent file not valid: " + recentFilePath);
			}
			Project.ClearPopup();
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.fileMainMmenuItem = new System.Windows.Forms.MenuItem();
            this.fileOpenMenuItem = new System.Windows.Forms.MenuItem();
            this.fileGpsBabelMenuItem = new System.Windows.Forms.MenuItem();
            this.importsListMenuItem = new System.Windows.Forms.MenuItem();
            this.clearImportsListMenuItem = new System.Windows.Forms.MenuItem();
            this.importMenuItem = new System.Windows.Forms.MenuItem();
            this.exportMenuItem = new System.Windows.Forms.MenuItem();
            this.saveTrackMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this.printPreviewMenuItem = new System.Windows.Forms.MenuItem();
            this.printMenuItem = new System.Windows.Forms.MenuItem();
            this.pageSetupMenuItem = new System.Windows.Forms.MenuItem();
            this.saveImageMenuItem = new System.Windows.Forms.MenuItem();
            this.saveWebPageMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem17 = new System.Windows.Forms.MenuItem();
            this.fileExitMenuItem = new System.Windows.Forms.MenuItem();
            this.internetMainMenuItem = new System.Windows.Forms.MenuItem();
            this.fetchEqMenuItem = new System.Windows.Forms.MenuItem();
            this.streetMap1MenuItem = new System.Windows.Forms.MenuItem();
            this.nearestGeocaches1MenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.onlineMenuItem = new System.Windows.Forms.MenuItem();
            this.useServicesMenuItem = new System.Windows.Forms.MenuItem();
            this.gpsMainMenuItem = new System.Windows.Forms.MenuItem();
            this.gpsManagerMenuItem = new System.Windows.Forms.MenuItem();
            this.getTrackLogMenuItem = new System.Windows.Forms.MenuItem();
            this.gpsCurrentMenuItem = new System.Windows.Forms.MenuItem();
            this.gpsGpsBabelMenuItem = new System.Windows.Forms.MenuItem();
            this.pdaMainMenuItem = new System.Windows.Forms.MenuItem();
            this.mapWizardMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.startCamTrackMenuItem = new System.Windows.Forms.MenuItem();
            this.pdaSuperframeMenuItem = new System.Windows.Forms.MenuItem();
            this.stopCamTrackMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem11 = new System.Windows.Forms.MenuItem();
            this.pdaPreloadAndExportMenuItem = new System.Windows.Forms.MenuItem();
            this.pdaPreloadAlongRouteMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.pdaCancelMenuItem = new System.Windows.Forms.MenuItem();
            this.mapMainMenuItem = new System.Windows.Forms.MenuItem();
            this.gotoMenuItem = new System.Windows.Forms.MenuItem();
            this.distanceMenuItem = new System.Windows.Forms.MenuItem();
            this.streetMapMenuItem = new System.Windows.Forms.MenuItem();
            this.nearestGeocachesMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.newWaypointMenuItem = new System.Windows.Forms.MenuItem();
            this.newRouteMenuItem = new System.Windows.Forms.MenuItem();
            this.trackManagerMenuItem = new System.Windows.Forms.MenuItem();
            this.geoTiffManagerMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.reliefMenuItem = new System.Windows.Forms.MenuItem();
            this.aerialMenuItem = new System.Windows.Forms.MenuItem();
            this.topoMenuItem = new System.Windows.Forms.MenuItem();
            this.citiesMenuItem = new System.Windows.Forms.MenuItem();
            this.eqMenuItem = new System.Windows.Forms.MenuItem();
            this.lmMenuItem = new System.Windows.Forms.MenuItem();
            this.vehMenuItem = new System.Windows.Forms.MenuItem();
            this.wptMenuItem = new System.Windows.Forms.MenuItem();
            this.thumbMenuItem = new System.Windows.Forms.MenuItem();
            this.colorCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.colorSpeedMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.backMenuItem = new System.Windows.Forms.MenuItem();
            this.forwardMenuItem = new System.Windows.Forms.MenuItem();
            this.refreshMenuItem = new System.Windows.Forms.MenuItem();
            this.loadTilesMenuItem = new System.Windows.Forms.MenuItem();
            this.loadAlongRouteMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.optionsMapMenuItem = new System.Windows.Forms.MenuItem();
            this.photoMainMenuItem = new System.Windows.Forms.MenuItem();
            this.photoManagerMenuItem = new System.Windows.Forms.MenuItem();
            this.listPhotosMenuItem = new System.Windows.Forms.MenuItem();
            this.unrelatedPhotosMenuItem = new System.Windows.Forms.MenuItem();
            this.viewImageMenuItem = new System.Windows.Forms.MenuItem();
            this.photoToGeotiffMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.photoAnalyzeGpxMenuItem = new System.Windows.Forms.MenuItem();
            this.thumb2MenuItem = new System.Windows.Forms.MenuItem();
            this.photoOptionsMenuItem = new System.Windows.Forms.MenuItem();
            this.eqMainMenuItem = new System.Windows.Forms.MenuItem();
            this.fetchEq1MenuItem = new System.Windows.Forms.MenuItem();
            this.eqLocalMenuItem = new System.Windows.Forms.MenuItem();
            this.eqWorldMenuItem = new System.Windows.Forms.MenuItem();
            this.eqFilterMenuItem = new System.Windows.Forms.MenuItem();
            this.eqHistoricalMenuItem = new System.Windows.Forms.MenuItem();
            this.eqClearMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this.eqOptionsMenuItem = new System.Windows.Forms.MenuItem();
            this.favMainMenuItem = new System.Windows.Forms.MenuItem();
            this.addToFavoritesMenuItem = new System.Windows.Forms.MenuItem();
            this.organizeFavoritesMenuItem = new System.Windows.Forms.MenuItem();
            this.exportFavoritesMenuItem = new System.Windows.Forms.MenuItem();
            this.importFavoritesMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.toolsMainMenuItem = new System.Windows.Forms.MenuItem();
            this.manageToolsMenuItem = new System.Windows.Forms.MenuItem();
            this.helpMainMenuItem = new System.Windows.Forms.MenuItem();
            this.helpMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSampleFilesMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSamplePhotoMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSampleFlyingMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSampleAnnotatedTripMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSampleGeotiffMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSampleWebpageMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSamplePocketqueryMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSampleNtsbMenuItem = new System.Windows.Forms.MenuItem();
            this.helpSampleFleetApiMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.helpSampleDeleteFilesMenuItem = new System.Windows.Forms.MenuItem();
            this.updateMenuItem = new System.Windows.Forms.MenuItem();
            this.optionsMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.aboutMenuItem = new System.Windows.Forms.MenuItem();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.progressButton = new System.Windows.Forms.Button();
            this.sb1Panel = new System.Windows.Forms.Panel();
            this.mainProgressBar = new System.Windows.Forms.ProgressBar();
            this.statusBar = new System.Windows.Forms.Label();
            this.sb2Panel = new System.Windows.Forms.Panel();
            this.statusBar2 = new System.Windows.Forms.Label();
            this.gotoButton = new System.Windows.Forms.Button();
            this.centerPanel = new System.Windows.Forms.Panel();
            this.picturePanel = new System.Windows.Forms.Panel();
            this.plannerPanel = new System.Windows.Forms.Panel();
            this.hintsPanel = new System.Windows.Forms.Panel();
            this.centerHintsPanel = new System.Windows.Forms.Panel();
            this.hintsLabel = new System.Windows.Forms.Label();
            this.bottomHintsPanel = new System.Windows.Forms.Panel();
            this.helpAtStartCheckBox = new System.Windows.Forms.CheckBox();
            this.notAssociatedButton = new System.Windows.Forms.Button();
            this.notAssociatedLabel = new System.Windows.Forms.Label();
            this.initialMessageLabel = new System.Windows.Forms.Label();
            this.mainPictureBox = new System.Windows.Forms.PictureBox();
            this.mainPictureBox_ContextMenu = new System.Windows.Forms.ContextMenu();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.plannerLinkLabel = new System.Windows.Forms.LinkLabel();
            this.findLinkLabel = new System.Windows.Forms.LinkLabel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.scaleWorldButton = new System.Windows.Forms.Button();
            this.scaleYButton = new System.Windows.Forms.Button();
            this.scaleSafeButton = new System.Windows.Forms.Button();
            this.scale64mButton = new System.Windows.Forms.Button();
            this.scale32mButton = new System.Windows.Forms.Button();
            this.scale16mButton = new System.Windows.Forms.Button();
            this.scale8mButton = new System.Windows.Forms.Button();
            this.scale4mButton = new System.Windows.Forms.Button();
            this.scale2mButton = new System.Windows.Forms.Button();
            this.scale1mButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.camtrackOnButton = new System.Windows.Forms.Button();
            this.eqFilterOnButton = new System.Windows.Forms.Button();
            this.opacityPanel = new System.Windows.Forms.Panel();
            this.opacityLinkLabel = new System.Windows.Forms.LinkLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.forwardLinkLabel = new System.Windows.Forms.LinkLabel();
            this.backLinkLabel = new System.Windows.Forms.LinkLabel();
            this.overlayCheckBox = new System.Windows.Forms.CheckBox();
            this.eqButton = new System.Windows.Forms.Button();
            this.wptButton = new System.Windows.Forms.Button();
            this.lmButton = new System.Windows.Forms.Button();
            this.lmCheckBox = new System.Windows.Forms.CheckBox();
            this.topoCheckBox = new System.Windows.Forms.CheckBox();
            this.aerialCheckBox = new System.Windows.Forms.CheckBox();
            this.colorAerialCheckBox = new System.Windows.Forms.CheckBox();
            this.reliefCheckBox = new System.Windows.Forms.CheckBox();
            this.eqCheckBox = new System.Windows.Forms.CheckBox();
            this.wptCheckBox = new System.Windows.Forms.CheckBox();
            this.citiesCheckBox = new System.Windows.Forms.CheckBox();
            this.gridCheckBox = new System.Windows.Forms.CheckBox();
            this.eqStylePanel = new System.Windows.Forms.Panel();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.heightScrollBar = new System.Windows.Forms.VScrollBar();
            this.cameraHeightLabel = new System.Windows.Forms.Label();
            this.scaleTextBox = new System.Windows.Forms.TextBox();
            this.vehCheckBox = new System.Windows.Forms.CheckBox();
            this.mainPrintDocument = new System.Drawing.Printing.PrintDocument();
            this.bottomPanel.SuspendLayout();
            this.sb1Panel.SuspendLayout();
            this.sb2Panel.SuspendLayout();
            this.centerPanel.SuspendLayout();
            this.picturePanel.SuspendLayout();
            this.hintsPanel.SuspendLayout();
            this.centerHintsPanel.SuspendLayout();
            this.bottomHintsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainPictureBox)).BeginInit();
            this.leftPanel.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.opacityPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.eqStylePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMainMmenuItem,
            this.internetMainMenuItem,
            this.gpsMainMenuItem,
            this.pdaMainMenuItem,
            this.mapMainMenuItem,
            this.photoMainMenuItem,
            this.eqMainMenuItem,
            this.favMainMenuItem,
            this.toolsMainMenuItem,
            this.helpMainMenuItem});
            // 
            // fileMainMmenuItem
            // 
            this.fileMainMmenuItem.Index = 0;
            this.fileMainMmenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileOpenMenuItem,
            this.fileGpsBabelMenuItem,
            this.importsListMenuItem,
            this.clearImportsListMenuItem,
            this.importMenuItem,
            this.exportMenuItem,
            this.saveTrackMenuItem,
            this.menuItem12,
            this.printPreviewMenuItem,
            this.printMenuItem,
            this.pageSetupMenuItem,
            this.saveImageMenuItem,
            this.saveWebPageMenuItem,
            this.menuItem17,
            this.fileExitMenuItem});
            this.fileMainMmenuItem.Text = "&File";
            this.fileMainMmenuItem.Popup += new System.EventHandler(this.fileMainMmenuItem_Popup);
            // 
            // fileOpenMenuItem
            // 
            this.fileOpenMenuItem.Index = 0;
            this.fileOpenMenuItem.Text = "&Open...";
            this.fileOpenMenuItem.Click += new System.EventHandler(this.fileOpenMenuItem_Click);
            // 
            // fileGpsBabelMenuItem
            // 
            this.fileGpsBabelMenuItem.Index = 1;
            this.fileGpsBabelMenuItem.Text = "Open/Save via GPSBabel...";
            this.fileGpsBabelMenuItem.Click += new System.EventHandler(this.fileGpsBabelMenuItem_Click);
            // 
            // importsListMenuItem
            // 
            this.importsListMenuItem.Index = 2;
            this.importsListMenuItem.Text = "&List Imported";
            this.importsListMenuItem.Click += new System.EventHandler(this.importsListMenuItem_Click);
            // 
            // clearImportsListMenuItem
            // 
            this.clearImportsListMenuItem.Index = 3;
            this.clearImportsListMenuItem.Text = "Clear All Imported...";
            this.clearImportsListMenuItem.Click += new System.EventHandler(this.clearImportsListMenuItem_Click);
            // 
            // importMenuItem
            // 
            this.importMenuItem.Index = 4;
            this.importMenuItem.Text = "&Import...";
            this.importMenuItem.Click += new System.EventHandler(this.importMenuItem_Click);
            // 
            // exportMenuItem
            // 
            this.exportMenuItem.Index = 5;
            this.exportMenuItem.Text = "&Export All...";
            this.exportMenuItem.Click += new System.EventHandler(this.exportMenuItem_Click);
            // 
            // saveTrackMenuItem
            // 
            this.saveTrackMenuItem.Index = 6;
            this.saveTrackMenuItem.Text = "&Save Tracks, Routes...";
            this.saveTrackMenuItem.Click += new System.EventHandler(this.saveTrackMenuItem_Click);
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 7;
            this.menuItem12.Text = "-";
            // 
            // printPreviewMenuItem
            // 
            this.printPreviewMenuItem.Index = 8;
            this.printPreviewMenuItem.Text = "&Print preview...";
            this.printPreviewMenuItem.Click += new System.EventHandler(this.printPreviewMenuItem_Click);
            // 
            // printMenuItem
            // 
            this.printMenuItem.Index = 9;
            this.printMenuItem.Text = "Print...";
            this.printMenuItem.Click += new System.EventHandler(this.printMenuItem_Click);
            // 
            // pageSetupMenuItem
            // 
            this.pageSetupMenuItem.Index = 10;
            this.pageSetupMenuItem.Text = "Page Setup...";
            this.pageSetupMenuItem.Click += new System.EventHandler(this.pageSetupMenuItem_Click);
            // 
            // saveImageMenuItem
            // 
            this.saveImageMenuItem.Index = 11;
            this.saveImageMenuItem.Text = "Save as Image...";
            this.saveImageMenuItem.Click += new System.EventHandler(this.saveImageMenuItem_Click);
            // 
            // saveWebPageMenuItem
            // 
            this.saveWebPageMenuItem.Index = 12;
            this.saveWebPageMenuItem.Text = "Save as Web Page...";
            this.saveWebPageMenuItem.Click += new System.EventHandler(this.saveWebPageMenuItem_Click);
            // 
            // menuItem17
            // 
            this.menuItem17.Index = 13;
            this.menuItem17.Text = "-";
            // 
            // fileExitMenuItem
            // 
            this.fileExitMenuItem.Index = 14;
            this.fileExitMenuItem.Text = "E&xit";
            this.fileExitMenuItem.Click += new System.EventHandler(this.fileExitMenuItem_Click);
            // 
            // internetMainMenuItem
            // 
            this.internetMainMenuItem.Index = 1;
            this.internetMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fetchEqMenuItem,
            this.streetMap1MenuItem,
            this.nearestGeocaches1MenuItem,
            this.menuItem2,
            this.onlineMenuItem,
            this.useServicesMenuItem});
            this.internetMainMenuItem.Text = "&Internet";
            this.internetMainMenuItem.Popup += new System.EventHandler(this.internetMainMenuItem_Popup);
            // 
            // fetchEqMenuItem
            // 
            this.fetchEqMenuItem.Index = 0;
            this.fetchEqMenuItem.Text = "&Fetch Recent Earthquakes";
            this.fetchEqMenuItem.Click += new System.EventHandler(this.fetchEqMenuItem_Click);
            // 
            // streetMap1MenuItem
            // 
            this.streetMap1MenuItem.Index = 1;
            this.streetMap1MenuItem.Text = "&Street Map...          (in browser)";
            this.streetMap1MenuItem.Click += new System.EventHandler(this.streetMapMenuItem_Click);
            // 
            // nearestGeocaches1MenuItem
            // 
            this.nearestGeocaches1MenuItem.Index = 2;
            this.nearestGeocaches1MenuItem.Text = "&Nearest Geocaches  (in browser)";
            this.nearestGeocaches1MenuItem.Click += new System.EventHandler(this.nearestGeocachesMenuItem_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 3;
            this.menuItem2.Text = "-";
            // 
            // onlineMenuItem
            // 
            this.onlineMenuItem.Index = 4;
            this.onlineMenuItem.Text = "&Online [click to toggle]";
            this.onlineMenuItem.Click += new System.EventHandler(this.onlineMenuItem_Click);
            // 
            // useServicesMenuItem
            // 
            this.useServicesMenuItem.Index = 5;
            this.useServicesMenuItem.Text = "Use &Web Services";
            this.useServicesMenuItem.Click += new System.EventHandler(this.useServicesMenuItem_Click);
            // 
            // gpsMainMenuItem
            // 
            this.gpsMainMenuItem.Index = 2;
            this.gpsMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.gpsManagerMenuItem,
            this.getTrackLogMenuItem,
            this.gpsCurrentMenuItem,
            this.gpsGpsBabelMenuItem});
            this.gpsMainMenuItem.Text = "&GPS";
            // 
            // gpsManagerMenuItem
            // 
            this.gpsManagerMenuItem.Index = 0;
            this.gpsManagerMenuItem.Text = "GPS &Manager";
            this.gpsManagerMenuItem.Click += new System.EventHandler(this.gpsManagerMenuItem_Click);
            // 
            // getTrackLogMenuItem
            // 
            this.getTrackLogMenuItem.Index = 1;
            this.getTrackLogMenuItem.Text = "Get &Track Log";
            this.getTrackLogMenuItem.Click += new System.EventHandler(this.getTrackLogMenuItem_Click);
            // 
            // gpsCurrentMenuItem
            // 
            this.gpsCurrentMenuItem.Index = 2;
            this.gpsCurrentMenuItem.Text = "&Current Position";
            this.gpsCurrentMenuItem.Click += new System.EventHandler(this.gpsCurrentMenuItem_Click);
            // 
            // gpsGpsBabelMenuItem
            // 
            this.gpsGpsBabelMenuItem.Index = 3;
            this.gpsGpsBabelMenuItem.Text = "GPSBabel Wrapper";
            this.gpsGpsBabelMenuItem.Click += new System.EventHandler(this.gpsGpsBabelMenuItem_Click);
            // 
            // pdaMainMenuItem
            // 
            this.pdaMainMenuItem.Index = 3;
            this.pdaMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mapWizardMenuItem,
            this.menuItem9,
            this.startCamTrackMenuItem,
            this.pdaSuperframeMenuItem,
            this.stopCamTrackMenuItem,
            this.menuItem11,
            this.pdaPreloadAndExportMenuItem,
            this.pdaPreloadAlongRouteMenuItem,
            this.menuItem7,
            this.pdaCancelMenuItem});
            this.pdaMainMenuItem.Text = "PDA";
            this.pdaMainMenuItem.Popup += new System.EventHandler(this.pdaMainMenuItem_Popup);
            // 
            // mapWizardMenuItem
            // 
            this.mapWizardMenuItem.Index = 0;
            this.mapWizardMenuItem.Text = "PDA Memory Card Map &Wizard...";
            this.mapWizardMenuItem.Click += new System.EventHandler(this.mapWizardMenuItem_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 1;
            this.menuItem9.Text = "-";
            // 
            // startCamTrackMenuItem
            // 
            this.startCamTrackMenuItem.Index = 2;
            this.startCamTrackMenuItem.Text = "Start Camera &Tracking";
            this.startCamTrackMenuItem.Click += new System.EventHandler(this.startCamTrackMenuItem_Click);
            // 
            // pdaSuperframeMenuItem
            // 
            this.pdaSuperframeMenuItem.Index = 3;
            this.pdaSuperframeMenuItem.Text = "Make Superframe...";
            this.pdaSuperframeMenuItem.Visible = false;
            this.pdaSuperframeMenuItem.Click += new System.EventHandler(this.pdaSuperframeMenuItem_Click);
            // 
            // stopCamTrackMenuItem
            // 
            this.stopCamTrackMenuItem.Index = 4;
            this.stopCamTrackMenuItem.Text = "&Stop / Export to PDB...";
            this.stopCamTrackMenuItem.Click += new System.EventHandler(this.stopCamTrackMenuItem_Click);
            // 
            // menuItem11
            // 
            this.menuItem11.Index = 5;
            this.menuItem11.Text = "-";
            // 
            // pdaPreloadAndExportMenuItem
            // 
            this.pdaPreloadAndExportMenuItem.Index = 6;
            this.pdaPreloadAndExportMenuItem.Text = "&Preload  and Export Tiles...";
            this.pdaPreloadAndExportMenuItem.Click += new System.EventHandler(this.pdaPreloadAndExportMenuItem_Click);
            // 
            // pdaPreloadAlongRouteMenuItem
            // 
            this.pdaPreloadAlongRouteMenuItem.Index = 7;
            this.pdaPreloadAlongRouteMenuItem.Text = "Preload Along &Route...";
            this.pdaPreloadAlongRouteMenuItem.Click += new System.EventHandler(this.pdaPreloadAlongRouteMenuItem_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 8;
            this.menuItem7.Text = "-";
            // 
            // pdaCancelMenuItem
            // 
            this.pdaCancelMenuItem.Index = 9;
            this.pdaCancelMenuItem.Text = "Cancel";
            this.pdaCancelMenuItem.Click += new System.EventHandler(this.pdaCancelMenuItem_Click);
            // 
            // mapMainMenuItem
            // 
            this.mapMainMenuItem.Index = 4;
            this.mapMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.gotoMenuItem,
            this.distanceMenuItem,
            this.streetMapMenuItem,
            this.nearestGeocachesMenuItem,
            this.menuItem3,
            this.newWaypointMenuItem,
            this.newRouteMenuItem,
            this.trackManagerMenuItem,
            this.geoTiffManagerMenuItem,
            this.menuItem4,
            this.reliefMenuItem,
            this.aerialMenuItem,
            this.topoMenuItem,
            this.citiesMenuItem,
            this.eqMenuItem,
            this.lmMenuItem,
            this.vehMenuItem,
            this.wptMenuItem,
            this.thumbMenuItem,
            this.colorCodeMenuItem,
            this.colorSpeedMenuItem,
            this.menuItem10,
            this.backMenuItem,
            this.forwardMenuItem,
            this.refreshMenuItem,
            this.loadTilesMenuItem,
            this.loadAlongRouteMenuItem,
            this.menuItem5,
            this.optionsMapMenuItem});
            this.mapMainMenuItem.Text = "&Map";
            this.mapMainMenuItem.Popup += new System.EventHandler(this.menuItem8_Popup);
            // 
            // gotoMenuItem
            // 
            this.gotoMenuItem.Index = 0;
            this.gotoMenuItem.Text = "&Go To Coordinates...";
            this.gotoMenuItem.Click += new System.EventHandler(this.gotoMenuItem_Click);
            // 
            // distanceMenuItem
            // 
            this.distanceMenuItem.Index = 1;
            this.distanceMenuItem.Text = "Distance...";
            this.distanceMenuItem.Click += new System.EventHandler(this.distanceMenuItem_Click);
            // 
            // streetMapMenuItem
            // 
            this.streetMapMenuItem.Index = 2;
            this.streetMapMenuItem.Text = "&Street Map              (in browser)";
            this.streetMapMenuItem.Click += new System.EventHandler(this.streetMapMenuItem_Click);
            // 
            // nearestGeocachesMenuItem
            // 
            this.nearestGeocachesMenuItem.Index = 3;
            this.nearestGeocachesMenuItem.Text = "&Nearest Geocaches   (in browser)";
            this.nearestGeocachesMenuItem.Click += new System.EventHandler(this.nearestGeocachesMenuItem_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 4;
            this.menuItem3.Text = "-";
            // 
            // newWaypointMenuItem
            // 
            this.newWaypointMenuItem.Index = 5;
            this.newWaypointMenuItem.Text = "New &Waypoint...";
            this.newWaypointMenuItem.Click += new System.EventHandler(this.newWaypointMenuItem_Click);
            // 
            // newRouteMenuItem
            // 
            this.newRouteMenuItem.Index = 6;
            this.newRouteMenuItem.Text = "New &Route...";
            this.newRouteMenuItem.Click += new System.EventHandler(this.newRouteMenuItem_Click);
            // 
            // trackManagerMenuItem
            // 
            this.trackManagerMenuItem.Index = 7;
            this.trackManagerMenuItem.Text = "Waypoints &Manager";
            this.trackManagerMenuItem.Click += new System.EventHandler(this.trackManagerMenuItem_Click);
            // 
            // geoTiffManagerMenuItem
            // 
            this.geoTiffManagerMenuItem.Index = 8;
            this.geoTiffManagerMenuItem.Text = "Geo&TIFF Manager";
            this.geoTiffManagerMenuItem.Click += new System.EventHandler(this.geoTiffManagerMenuItem_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 9;
            this.menuItem4.Text = "-";
            // 
            // reliefMenuItem
            // 
            this.reliefMenuItem.Index = 10;
            this.reliefMenuItem.Text = "&Relief";
            this.reliefMenuItem.Click += new System.EventHandler(this.reliefMenuItem_Click);
            // 
            // aerialMenuItem
            // 
            this.aerialMenuItem.Index = 11;
            this.aerialMenuItem.Text = "&Aerial";
            this.aerialMenuItem.Click += new System.EventHandler(this.aerialMenuItem_Click);
            // 
            // topoMenuItem
            // 
            this.topoMenuItem.Index = 12;
            this.topoMenuItem.Text = "&Topo";
            this.topoMenuItem.Click += new System.EventHandler(this.topoMenuItem_Click);
            // 
            // citiesMenuItem
            // 
            this.citiesMenuItem.Index = 13;
            this.citiesMenuItem.Text = "&Cities";
            this.citiesMenuItem.Click += new System.EventHandler(this.citiesMenuItem_Click);
            // 
            // eqMenuItem
            // 
            this.eqMenuItem.Index = 14;
            this.eqMenuItem.Text = "&Earthquakes";
            this.eqMenuItem.Click += new System.EventHandler(this.eqMenuItem_Click);
            // 
            // lmMenuItem
            // 
            this.lmMenuItem.Index = 15;
            this.lmMenuItem.Text = "&Landmarks";
            this.lmMenuItem.Click += new System.EventHandler(this.lmMenuItem_Click);
            // 
            // vehMenuItem
            // 
            this.vehMenuItem.Index = 16;
            this.vehMenuItem.Text = "&Vehicles";
            this.vehMenuItem.Click += new System.EventHandler(this.vehMenuItem_Click);
            // 
            // wptMenuItem
            // 
            this.wptMenuItem.Index = 17;
            this.wptMenuItem.Text = "Waypoints, Routes and Tracks";
            this.wptMenuItem.Click += new System.EventHandler(this.wptMenuItem_Click);
            // 
            // thumbMenuItem
            // 
            this.thumbMenuItem.Index = 18;
            this.thumbMenuItem.Text = "Photo Thumbnails";
            this.thumbMenuItem.Click += new System.EventHandler(this.thumbMenuItem_Click);
            // 
            // colorCodeMenuItem
            // 
            this.colorCodeMenuItem.Index = 19;
            this.colorCodeMenuItem.Text = "Color-code Tracks Elevation";
            this.colorCodeMenuItem.Click += new System.EventHandler(this.colorCodeMenuItem_Click);
            // 
            // colorSpeedMenuItem
            // 
            this.colorSpeedMenuItem.Index = 20;
            this.colorSpeedMenuItem.Text = "Color-code Tracks Speed";
            this.colorSpeedMenuItem.Click += new System.EventHandler(this.colorSpeedMenuItem_Click);
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 21;
            this.menuItem10.Text = "-";
            // 
            // backMenuItem
            // 
            this.backMenuItem.Index = 22;
            this.backMenuItem.Text = "Back (Backspace)";
            this.backMenuItem.Click += new System.EventHandler(this.backMenuItem_Click);
            // 
            // forwardMenuItem
            // 
            this.forwardMenuItem.Index = 23;
            this.forwardMenuItem.Text = "Forward";
            this.forwardMenuItem.Click += new System.EventHandler(this.forwardMenuItem_Click);
            // 
            // refreshMenuItem
            // 
            this.refreshMenuItem.Index = 24;
            this.refreshMenuItem.Text = "Reload and refresh (Ctrl/F5)";
            this.refreshMenuItem.Click += new System.EventHandler(this.refreshMenuItem_Click);
            // 
            // loadTilesMenuItem
            // 
            this.loadTilesMenuItem.Index = 25;
            this.loadTilesMenuItem.Text = "&Preload Tiles...";
            this.loadTilesMenuItem.Click += new System.EventHandler(this.loadTilesMenuItem_Click);
            // 
            // loadAlongRouteMenuItem
            // 
            this.loadAlongRouteMenuItem.Index = 26;
            this.loadAlongRouteMenuItem.Text = "Preload Along Route...";
            this.loadAlongRouteMenuItem.Click += new System.EventHandler(this.pdaPreloadAlongRouteMenuItem_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 27;
            this.menuItem5.Text = "-";
            // 
            // optionsMapMenuItem
            // 
            this.optionsMapMenuItem.Index = 28;
            this.optionsMapMenuItem.Text = "&Options";
            this.optionsMapMenuItem.Click += new System.EventHandler(this.optionsMapMenuItem_Click);
            // 
            // photoMainMenuItem
            // 
            this.photoMainMenuItem.Index = 5;
            this.photoMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.photoManagerMenuItem,
            this.listPhotosMenuItem,
            this.unrelatedPhotosMenuItem,
            this.viewImageMenuItem,
            this.photoToGeotiffMenuItem,
            this.menuItem1,
            this.photoAnalyzeGpxMenuItem,
            this.thumb2MenuItem,
            this.photoOptionsMenuItem});
            this.photoMainMenuItem.Text = "&Photo";
            this.photoMainMenuItem.Popup += new System.EventHandler(this.photoMainMenuItem_Popup);
            // 
            // photoManagerMenuItem
            // 
            this.photoManagerMenuItem.Index = 0;
            this.photoManagerMenuItem.Text = "Photo &Manager";
            this.photoManagerMenuItem.Click += new System.EventHandler(this.photoManagerMenuItem_Click);
            // 
            // listPhotosMenuItem
            // 
            this.listPhotosMenuItem.Index = 1;
            this.listPhotosMenuItem.Text = "&List Photo Trackpoints";
            this.listPhotosMenuItem.Click += new System.EventHandler(this.listPhotosMenuItem_Click);
            // 
            // unrelatedPhotosMenuItem
            // 
            this.unrelatedPhotosMenuItem.Index = 2;
            this.unrelatedPhotosMenuItem.Text = "&Unrelated Photos";
            this.unrelatedPhotosMenuItem.Click += new System.EventHandler(this.unrelatedPhotosMenuItem_Click);
            // 
            // viewImageMenuItem
            // 
            this.viewImageMenuItem.Index = 3;
            this.viewImageMenuItem.Text = "&View Photo";
            this.viewImageMenuItem.Click += new System.EventHandler(this.viewImageMenuItem_Click);
            // 
            // photoToGeotiffMenuItem
            // 
            this.photoToGeotiffMenuItem.Index = 4;
            this.photoToGeotiffMenuItem.Text = "Convert toGeoTIFF...";
            this.photoToGeotiffMenuItem.Click += new System.EventHandler(this.photoToGeotiffMenuItem_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 5;
            this.menuItem1.Text = "-";
            // 
            // photoAnalyzeGpxMenuItem
            // 
            this.photoAnalyzeGpxMenuItem.Index = 6;
            this.photoAnalyzeGpxMenuItem.Text = "&Analyze when loading a .GPX";
            this.photoAnalyzeGpxMenuItem.Click += new System.EventHandler(this.photoAnalyzeGpxMenuItem_Click);
            // 
            // thumb2MenuItem
            // 
            this.thumb2MenuItem.Index = 7;
            this.thumb2MenuItem.Text = "Show Photo &Thumbnails";
            this.thumb2MenuItem.Click += new System.EventHandler(this.thumb2MenuItem_Click);
            // 
            // photoOptionsMenuItem
            // 
            this.photoOptionsMenuItem.Index = 8;
            this.photoOptionsMenuItem.Text = "&Options...";
            this.photoOptionsMenuItem.Click += new System.EventHandler(this.photoOptionsMenuItem_Click);
            // 
            // eqMainMenuItem
            // 
            this.eqMainMenuItem.Index = 6;
            this.eqMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fetchEq1MenuItem,
            this.eqLocalMenuItem,
            this.eqWorldMenuItem,
            this.eqFilterMenuItem,
            this.eqHistoricalMenuItem,
            this.eqClearMenuItem,
            this.menuItem13,
            this.eqOptionsMenuItem});
            this.eqMainMenuItem.Text = "&Earthquakes";
            // 
            // fetchEq1MenuItem
            // 
            this.fetchEq1MenuItem.Index = 0;
            this.fetchEq1MenuItem.Text = "&Fetch Recent Earthquakes";
            this.fetchEq1MenuItem.Click += new System.EventHandler(this.fetchEqMenuItem_Click);
            // 
            // eqLocalMenuItem
            // 
            this.eqLocalMenuItem.Index = 1;
            this.eqLocalMenuItem.Text = "On the &Visible Map";
            this.eqLocalMenuItem.Click += new System.EventHandler(this.eqLocalMenuItem_Click);
            // 
            // eqWorldMenuItem
            // 
            this.eqWorldMenuItem.Index = 2;
            this.eqWorldMenuItem.Text = "&World";
            this.eqWorldMenuItem.Click += new System.EventHandler(this.eqWorldMenuItem_Click);
            // 
            // eqFilterMenuItem
            // 
            this.eqFilterMenuItem.Index = 3;
            this.eqFilterMenuItem.Text = "Filter...";
            this.eqFilterMenuItem.Click += new System.EventHandler(this.eqFilterMenuItem_Click);
            // 
            // eqHistoricalMenuItem
            // 
            this.eqHistoricalMenuItem.Index = 4;
            this.eqHistoricalMenuItem.Text = "&Historical...";
            this.eqHistoricalMenuItem.Click += new System.EventHandler(this.eqHistoricalMenuItem_Click);
            // 
            // eqClearMenuItem
            // 
            this.eqClearMenuItem.Index = 5;
            this.eqClearMenuItem.Text = "&Clear All";
            this.eqClearMenuItem.Click += new System.EventHandler(this.eqClearMenuItem_Click);
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 6;
            this.menuItem13.Text = "-";
            // 
            // eqOptionsMenuItem
            // 
            this.eqOptionsMenuItem.Index = 7;
            this.eqOptionsMenuItem.Text = "&Options...";
            this.eqOptionsMenuItem.Click += new System.EventHandler(this.eqOptionsMenuItem_Click);
            // 
            // favMainMenuItem
            // 
            this.favMainMenuItem.Index = 7;
            this.favMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.addToFavoritesMenuItem,
            this.organizeFavoritesMenuItem,
            this.exportFavoritesMenuItem,
            this.importFavoritesMenuItem,
            this.menuItem6});
            this.favMainMenuItem.Text = "Fa&vorites";
            // 
            // addToFavoritesMenuItem
            // 
            this.addToFavoritesMenuItem.Index = 0;
            this.addToFavoritesMenuItem.Text = "&Add to Favorites...";
            this.addToFavoritesMenuItem.Click += new System.EventHandler(this.addToFavoritesMenuItem_Click);
            // 
            // organizeFavoritesMenuItem
            // 
            this.organizeFavoritesMenuItem.Index = 1;
            this.organizeFavoritesMenuItem.Text = "&Organize Favorites...";
            this.organizeFavoritesMenuItem.Click += new System.EventHandler(this.organizeFavoritesMenuItem_Click);
            // 
            // exportFavoritesMenuItem
            // 
            this.exportFavoritesMenuItem.Index = 2;
            this.exportFavoritesMenuItem.Text = "&Export...";
            this.exportFavoritesMenuItem.Click += new System.EventHandler(this.exportFavoritesMenuItem_Click);
            // 
            // importFavoritesMenuItem
            // 
            this.importFavoritesMenuItem.Index = 3;
            this.importFavoritesMenuItem.Text = "&Import...";
            this.importFavoritesMenuItem.Click += new System.EventHandler(this.importFavoritesMenuItem_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 4;
            this.menuItem6.Text = "-";
            // 
            // toolsMainMenuItem
            // 
            this.toolsMainMenuItem.Index = 8;
            this.toolsMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.manageToolsMenuItem});
            this.toolsMainMenuItem.Text = "&Tools";
            this.toolsMainMenuItem.Popup += new System.EventHandler(this.toolsMainMenuItem_Popup);
            // 
            // manageToolsMenuItem
            // 
            this.manageToolsMenuItem.Index = 0;
            this.manageToolsMenuItem.Text = "&Manage Tools...";
            this.manageToolsMenuItem.Click += new System.EventHandler(this.manageToolsMenuItem_Click);
            // 
            // helpMainMenuItem
            // 
            this.helpMainMenuItem.Index = 9;
            this.helpMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.helpMenuItem,
            this.helpSampleFilesMenuItem,
            this.updateMenuItem,
            this.optionsMenuItem,
            this.menuItem8,
            this.aboutMenuItem});
            this.helpMainMenuItem.Text = "&Help";
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.Index = 0;
            this.helpMenuItem.Text = "&Help (User Manual)";
            this.helpMenuItem.Click += new System.EventHandler(this.helpMenuItem_Click);
            // 
            // helpSampleFilesMenuItem
            // 
            this.helpSampleFilesMenuItem.Index = 1;
            this.helpSampleFilesMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.helpSamplePhotoMenuItem,
            this.helpSampleFlyingMenuItem,
            this.helpSampleAnnotatedTripMenuItem,
            this.helpSampleGeotiffMenuItem,
            this.helpSampleWebpageMenuItem,
            this.helpSamplePocketqueryMenuItem,
            this.helpSampleNtsbMenuItem,
            this.helpSampleFleetApiMenuItem,
            this.menuItem14,
            this.helpSampleDeleteFilesMenuItem});
            this.helpSampleFilesMenuItem.Text = "Sample Files";
            // 
            // helpSamplePhotoMenuItem
            // 
            this.helpSamplePhotoMenuItem.Index = 0;
            this.helpSamplePhotoMenuItem.Text = "Photos on Track";
            this.helpSamplePhotoMenuItem.Click += new System.EventHandler(this.helpSamplePhotoMenuItem_Click);
            // 
            // helpSampleFlyingMenuItem
            // 
            this.helpSampleFlyingMenuItem.Index = 1;
            this.helpSampleFlyingMenuItem.Text = "Flying Around";
            this.helpSampleFlyingMenuItem.Click += new System.EventHandler(this.helpSampleFlyingMenuItem_Click);
            // 
            // helpSampleAnnotatedTripMenuItem
            // 
            this.helpSampleAnnotatedTripMenuItem.Index = 2;
            this.helpSampleAnnotatedTripMenuItem.Text = "Annotated Trip";
            this.helpSampleAnnotatedTripMenuItem.Click += new System.EventHandler(this.helpSampleAnnotatedTripMenuItem_Click);
            // 
            // helpSampleGeotiffMenuItem
            // 
            this.helpSampleGeotiffMenuItem.Index = 3;
            this.helpSampleGeotiffMenuItem.Text = "GeoTIFF";
            this.helpSampleGeotiffMenuItem.Click += new System.EventHandler(this.helpSampleGeotiffMenuItem_Click);
            // 
            // helpSampleWebpageMenuItem
            // 
            this.helpSampleWebpageMenuItem.Index = 4;
            this.helpSampleWebpageMenuItem.Text = "Generated Web Page";
            this.helpSampleWebpageMenuItem.Click += new System.EventHandler(this.helpSampleWebpageMenuItem_Click);
            // 
            // helpSamplePocketqueryMenuItem
            // 
            this.helpSamplePocketqueryMenuItem.Index = 5;
            this.helpSamplePocketqueryMenuItem.Text = "Geocaching Pocket Query";
            this.helpSamplePocketqueryMenuItem.Click += new System.EventHandler(this.helpSamplePocketqueryMenuItem_Click);
            // 
            // helpSampleNtsbMenuItem
            // 
            this.helpSampleNtsbMenuItem.Index = 6;
            this.helpSampleNtsbMenuItem.Text = "NTSB Crash Sites";
            this.helpSampleNtsbMenuItem.Click += new System.EventHandler(this.helpSampleNtsbMenuItem_Click);
            // 
            // helpSampleFleetApiMenuItem
            // 
            this.helpSampleFleetApiMenuItem.Index = 7;
            this.helpSampleFleetApiMenuItem.Text = "API Fleet Sample";
            this.helpSampleFleetApiMenuItem.Click += new System.EventHandler(this.helpSampleFleetApiMenuItem_Click);
            // 
            // menuItem14
            // 
            this.menuItem14.Index = 8;
            this.menuItem14.Text = "-";
            // 
            // helpSampleDeleteFilesMenuItem
            // 
            this.helpSampleDeleteFilesMenuItem.Index = 9;
            this.helpSampleDeleteFilesMenuItem.Text = "Delete Sample Files...";
            this.helpSampleDeleteFilesMenuItem.Click += new System.EventHandler(this.helpSampleDeleteFilesMenuItem_Click);
            // 
            // updateMenuItem
            // 
            this.updateMenuItem.Index = 2;
            this.updateMenuItem.Text = "Check for &update";
            this.updateMenuItem.Click += new System.EventHandler(this.updateMenuItem_Click);
            // 
            // optionsMenuItem
            // 
            this.optionsMenuItem.Index = 3;
            this.optionsMenuItem.Text = "&Options";
            this.optionsMenuItem.Click += new System.EventHandler(this.optionsMenuItem_Click);
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 4;
            this.menuItem8.Text = "-";
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Index = 5;
            this.aboutMenuItem.Text = "&About...";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.progressButton);
            this.bottomPanel.Controls.Add(this.sb1Panel);
            this.bottomPanel.Controls.Add(this.sb2Panel);
            this.bottomPanel.Controls.Add(this.gotoButton);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 769);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1066, 25);
            this.bottomPanel.TabIndex = 1;
            // 
            // progressButton
            // 
            this.progressButton.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressButton.ForeColor = System.Drawing.Color.Yellow;
            this.progressButton.Location = new System.Drawing.Point(3, 5);
            this.progressButton.Name = "progressButton";
            this.progressButton.Size = new System.Drawing.Size(55, 17);
            this.progressButton.TabIndex = 2;
            this.progressButton.Text = "99";
            this.progressButton.Click += new System.EventHandler(this.progressButton_Click);
            // 
            // sb1Panel
            // 
            this.sb1Panel.Controls.Add(this.mainProgressBar);
            this.sb1Panel.Controls.Add(this.statusBar);
            this.sb1Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sb1Panel.Location = new System.Drawing.Point(0, 0);
            this.sb1Panel.Name = "sb1Panel";
            this.sb1Panel.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            this.sb1Panel.Size = new System.Drawing.Size(646, 25);
            this.sb1Panel.TabIndex = 1;
            // 
            // mainProgressBar
            // 
            this.mainProgressBar.Location = new System.Drawing.Point(64, 5);
            this.mainProgressBar.Name = "mainProgressBar";
            this.mainProgressBar.Size = new System.Drawing.Size(69, 17);
            this.mainProgressBar.TabIndex = 1;
            // 
            // statusBar
            // 
            this.statusBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.statusBar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.statusBar.Location = new System.Drawing.Point(142, 6);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(498, 15);
            this.statusBar.TabIndex = 0;
            this.statusBar.Text = "Ready";
            this.statusBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sb2Panel
            // 
            this.sb2Panel.Controls.Add(this.statusBar2);
            this.sb2Panel.Dock = System.Windows.Forms.DockStyle.Right;
            this.sb2Panel.Location = new System.Drawing.Point(646, 0);
            this.sb2Panel.Name = "sb2Panel";
            this.sb2Panel.Padding = new System.Windows.Forms.Padding(0, 3, 10, 3);
            this.sb2Panel.Size = new System.Drawing.Size(367, 25);
            this.sb2Panel.TabIndex = 0;
            // 
            // statusBar2
            // 
            this.statusBar2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusBar2.Location = new System.Drawing.Point(0, 3);
            this.statusBar2.Name = "statusBar2";
            this.statusBar2.Size = new System.Drawing.Size(357, 19);
            this.statusBar2.TabIndex = 0;
            this.statusBar2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gotoButton
            // 
            this.gotoButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.gotoButton.Location = new System.Drawing.Point(1013, 0);
            this.gotoButton.Name = "gotoButton";
            this.gotoButton.Size = new System.Drawing.Size(53, 25);
            this.gotoButton.TabIndex = 1;
            this.gotoButton.Text = "GoTo";
            this.gotoButton.Click += new System.EventHandler(this.gotoButton_Click);
            // 
            // centerPanel
            // 
            this.centerPanel.Controls.Add(this.picturePanel);
            this.centerPanel.Controls.Add(this.splitter1);
            this.centerPanel.Controls.Add(this.leftPanel);
            this.centerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.centerPanel.Location = new System.Drawing.Point(0, 0);
            this.centerPanel.Name = "centerPanel";
            this.centerPanel.Size = new System.Drawing.Size(1066, 769);
            this.centerPanel.TabIndex = 3;
            // 
            // picturePanel
            // 
            this.picturePanel.Controls.Add(this.plannerPanel);
            this.picturePanel.Controls.Add(this.hintsPanel);
            this.picturePanel.Controls.Add(this.initialMessageLabel);
            this.picturePanel.Controls.Add(this.mainPictureBox);
            this.picturePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picturePanel.Location = new System.Drawing.Point(80, 0);
            this.picturePanel.Name = "picturePanel";
            this.picturePanel.Size = new System.Drawing.Size(986, 769);
            this.picturePanel.TabIndex = 7;
            // 
            // plannerPanel
            // 
            this.plannerPanel.BackColor = System.Drawing.SystemColors.Control;
            this.plannerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.plannerPanel.Location = new System.Drawing.Point(0, 469);
            this.plannerPanel.Name = "plannerPanel";
            this.plannerPanel.Size = new System.Drawing.Size(986, 300);
            this.plannerPanel.TabIndex = 12;
            this.plannerPanel.Visible = false;
            // 
            // hintsPanel
            // 
            this.hintsPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(50)))));
            this.hintsPanel.Controls.Add(this.centerHintsPanel);
            this.hintsPanel.Controls.Add(this.bottomHintsPanel);
            this.hintsPanel.Location = new System.Drawing.Point(84, 103);
            this.hintsPanel.Name = "hintsPanel";
            this.hintsPanel.Padding = new System.Windows.Forms.Padding(5);
            this.hintsPanel.Size = new System.Drawing.Size(668, 297);
            this.hintsPanel.TabIndex = 11;
            this.hintsPanel.Click += new System.EventHandler(this.hintsPanel_Click);
            // 
            // centerHintsPanel
            // 
            this.centerHintsPanel.BackColor = System.Drawing.Color.Transparent;
            this.centerHintsPanel.Controls.Add(this.hintsLabel);
            this.centerHintsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.centerHintsPanel.Location = new System.Drawing.Point(5, 5);
            this.centerHintsPanel.Name = "centerHintsPanel";
            this.centerHintsPanel.Padding = new System.Windows.Forms.Padding(17, 0, 0, 0);
            this.centerHintsPanel.Size = new System.Drawing.Size(658, 231);
            this.centerHintsPanel.TabIndex = 22;
            // 
            // hintsLabel
            // 
            this.hintsLabel.BackColor = System.Drawing.Color.Transparent;
            this.hintsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hintsLabel.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hintsLabel.ForeColor = System.Drawing.Color.Red;
            this.hintsLabel.Location = new System.Drawing.Point(17, 0);
            this.hintsLabel.Name = "hintsLabel";
            this.hintsLabel.Size = new System.Drawing.Size(641, 231);
            this.hintsLabel.TabIndex = 7;
            this.hintsLabel.Text = "Double click - to recenter...";
            this.hintsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.hintsLabel.Click += new System.EventHandler(this.hintsPanel_Click);
            // 
            // bottomHintsPanel
            // 
            this.bottomHintsPanel.BackColor = System.Drawing.Color.Transparent;
            this.bottomHintsPanel.Controls.Add(this.helpAtStartCheckBox);
            this.bottomHintsPanel.Controls.Add(this.notAssociatedButton);
            this.bottomHintsPanel.Controls.Add(this.notAssociatedLabel);
            this.bottomHintsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomHintsPanel.Location = new System.Drawing.Point(5, 236);
            this.bottomHintsPanel.Name = "bottomHintsPanel";
            this.bottomHintsPanel.Size = new System.Drawing.Size(658, 56);
            this.bottomHintsPanel.TabIndex = 21;
            this.bottomHintsPanel.Click += new System.EventHandler(this.hintsPanel_Click);
            // 
            // helpAtStartCheckBox
            // 
            this.helpAtStartCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.helpAtStartCheckBox.ForeColor = System.Drawing.Color.Red;
            this.helpAtStartCheckBox.Location = new System.Drawing.Point(8, 32);
            this.helpAtStartCheckBox.Name = "helpAtStartCheckBox";
            this.helpAtStartCheckBox.Size = new System.Drawing.Size(256, 23);
            this.helpAtStartCheckBox.TabIndex = 21;
            this.helpAtStartCheckBox.Text = "do not show this window at start";
            this.helpAtStartCheckBox.UseVisualStyleBackColor = false;
            this.helpAtStartCheckBox.CheckedChanged += new System.EventHandler(this.helpAtStartCheckBox_CheckedChanged);
            // 
            // notAssociatedButton
            // 
            this.notAssociatedButton.BackColor = System.Drawing.Color.Red;
            this.notAssociatedButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.notAssociatedButton.Location = new System.Drawing.Point(616, 8);
            this.notAssociatedButton.Name = "notAssociatedButton";
            this.notAssociatedButton.Size = new System.Drawing.Size(37, 19);
            this.notAssociatedButton.TabIndex = 20;
            this.notAssociatedButton.UseVisualStyleBackColor = false;
            this.notAssociatedButton.Visible = false;
            this.notAssociatedButton.Click += new System.EventHandler(this.notAssociatedButton_Click);
            // 
            // notAssociatedLabel
            // 
            this.notAssociatedLabel.BackColor = System.Drawing.Color.Transparent;
            this.notAssociatedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.notAssociatedLabel.ForeColor = System.Drawing.Color.Red;
            this.notAssociatedLabel.Location = new System.Drawing.Point(160, 8);
            this.notAssociatedLabel.Name = "notAssociatedLabel";
            this.notAssociatedLabel.Size = new System.Drawing.Size(449, 19);
            this.notAssociatedLabel.TabIndex = 11;
            this.notAssociatedLabel.Text = "Warning: files are not associated  with QuakeMap. Click here -->";
            this.notAssociatedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.notAssociatedLabel.Click += new System.EventHandler(this.hintsPanel_Click);
            // 
            // initialMessageLabel
            // 
            this.initialMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.initialMessageLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.initialMessageLabel.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Italic);
            this.initialMessageLabel.ForeColor = System.Drawing.Color.Red;
            this.initialMessageLabel.Location = new System.Drawing.Point(0, 0);
            this.initialMessageLabel.Name = "initialMessageLabel";
            this.initialMessageLabel.Size = new System.Drawing.Size(991, 56);
            this.initialMessageLabel.TabIndex = 6;
            this.initialMessageLabel.Text = "Initializing, please wait...";
            // 
            // mainPictureBox
            // 
            this.mainPictureBox.BackColor = System.Drawing.SystemColors.Control;
            this.mainPictureBox.ContextMenu = this.mainPictureBox_ContextMenu;
            this.mainPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPictureBox.Location = new System.Drawing.Point(0, 0);
            this.mainPictureBox.Name = "mainPictureBox";
            this.mainPictureBox.Size = new System.Drawing.Size(986, 769);
            this.mainPictureBox.TabIndex = 5;
            this.mainPictureBox.TabStop = false;
            this.mainPictureBox.DoubleClick += new System.EventHandler(this.mainPictureBox_DoubleClick);
            this.mainPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mainPictureBox_MouseMove);
            this.mainPictureBox.Click += new System.EventHandler(this.mainPictureBox_Click);
            this.mainPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mainPictureBox_MouseDown);
            this.mainPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mainPictureBox_MouseUp);
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.HotTrack;
            this.splitter1.Location = new System.Drawing.Point(72, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(8, 769);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.plannerLinkLabel);
            this.leftPanel.Controls.Add(this.findLinkLabel);
            this.leftPanel.Controls.Add(this.panel3);
            this.leftPanel.Controls.Add(this.panel1);
            this.leftPanel.Controls.Add(this.opacityPanel);
            this.leftPanel.Controls.Add(this.panel2);
            this.leftPanel.Controls.Add(this.eqStylePanel);
            this.leftPanel.Controls.Add(this.heightScrollBar);
            this.leftPanel.Controls.Add(this.cameraHeightLabel);
            this.leftPanel.Controls.Add(this.scaleTextBox);
            this.leftPanel.Controls.Add(this.vehCheckBox);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(72, 769);
            this.leftPanel.TabIndex = 5;
            // 
            // plannerLinkLabel
            // 
            this.plannerLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.plannerLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.plannerLinkLabel.Location = new System.Drawing.Point(5, 750);
            this.plannerLinkLabel.Name = "plannerLinkLabel";
            this.plannerLinkLabel.Size = new System.Drawing.Size(60, 15);
            this.plannerLinkLabel.TabIndex = 31;
            this.plannerLinkLabel.TabStop = true;
            this.plannerLinkLabel.Text = "plan";
            this.plannerLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.plannerLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.plannerLinkLabel_LinkClicked);
            // 
            // findLinkLabel
            // 
            this.findLinkLabel.Location = new System.Drawing.Point(15, 345);
            this.findLinkLabel.Name = "findLinkLabel";
            this.findLinkLabel.Size = new System.Drawing.Size(45, 15);
            this.findLinkLabel.TabIndex = 30;
            this.findLinkLabel.TabStop = true;
            this.findLinkLabel.Text = "Find";
            this.findLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.findLinkLabel_LinkClicked);
            this.findLinkLabel.MouseHover += new System.EventHandler(this.findLinkLabel_MouseHover);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.scaleWorldButton);
            this.panel3.Controls.Add(this.scaleYButton);
            this.panel3.Controls.Add(this.scaleSafeButton);
            this.panel3.Controls.Add(this.scale64mButton);
            this.panel3.Controls.Add(this.scale32mButton);
            this.panel3.Controls.Add(this.scale16mButton);
            this.panel3.Controls.Add(this.scale8mButton);
            this.panel3.Controls.Add(this.scale4mButton);
            this.panel3.Controls.Add(this.scale2mButton);
            this.panel3.Controls.Add(this.scale1mButton);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Location = new System.Drawing.Point(0, 71);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(16, 265);
            this.panel3.TabIndex = 28;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Arial", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(3, 119);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 12);
            this.label8.TabIndex = 30;
            this.label8.Text = "64";
            // 
            // scaleWorldButton
            // 
            this.scaleWorldButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scaleWorldButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scaleWorldButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scaleWorldButton.Location = new System.Drawing.Point(5, 19);
            this.scaleWorldButton.Name = "scaleWorldButton";
            this.scaleWorldButton.Size = new System.Drawing.Size(9, 11);
            this.scaleWorldButton.TabIndex = 29;
            this.scaleWorldButton.UseVisualStyleBackColor = false;
            this.scaleWorldButton.Click += new System.EventHandler(this.scaleWorldButton_Click);
            // 
            // scaleYButton
            // 
            this.scaleYButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scaleYButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scaleYButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scaleYButton.Location = new System.Drawing.Point(5, 56);
            this.scaleYButton.Name = "scaleYButton";
            this.scaleYButton.Size = new System.Drawing.Size(9, 12);
            this.scaleYButton.TabIndex = 28;
            this.scaleYButton.UseVisualStyleBackColor = false;
            this.scaleYButton.Click += new System.EventHandler(this.scaleYButton_Click);
            // 
            // scaleSafeButton
            // 
            this.scaleSafeButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scaleSafeButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scaleSafeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scaleSafeButton.Location = new System.Drawing.Point(5, 94);
            this.scaleSafeButton.Name = "scaleSafeButton";
            this.scaleSafeButton.Size = new System.Drawing.Size(9, 11);
            this.scaleSafeButton.TabIndex = 27;
            this.scaleSafeButton.UseVisualStyleBackColor = false;
            this.scaleSafeButton.Click += new System.EventHandler(this.scaleSafeButton_Click);
            // 
            // scale64mButton
            // 
            this.scale64mButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scale64mButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scale64mButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scale64mButton.Location = new System.Drawing.Point(5, 131);
            this.scale64mButton.Name = "scale64mButton";
            this.scale64mButton.Size = new System.Drawing.Size(9, 12);
            this.scale64mButton.TabIndex = 26;
            this.scale64mButton.UseVisualStyleBackColor = false;
            this.scale64mButton.Click += new System.EventHandler(this.scale64mButton_Click);
            // 
            // scale32mButton
            // 
            this.scale32mButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scale32mButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scale32mButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scale32mButton.Location = new System.Drawing.Point(5, 147);
            this.scale32mButton.Name = "scale32mButton";
            this.scale32mButton.Size = new System.Drawing.Size(9, 12);
            this.scale32mButton.TabIndex = 25;
            this.scale32mButton.UseVisualStyleBackColor = false;
            this.scale32mButton.Click += new System.EventHandler(this.scale32mButton_Click);
            // 
            // scale16mButton
            // 
            this.scale16mButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scale16mButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scale16mButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scale16mButton.Location = new System.Drawing.Point(5, 164);
            this.scale16mButton.Name = "scale16mButton";
            this.scale16mButton.Size = new System.Drawing.Size(9, 11);
            this.scale16mButton.TabIndex = 24;
            this.scale16mButton.UseVisualStyleBackColor = false;
            this.scale16mButton.Click += new System.EventHandler(this.scale16mButton_Click);
            // 
            // scale8mButton
            // 
            this.scale8mButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scale8mButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scale8mButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scale8mButton.Location = new System.Drawing.Point(5, 196);
            this.scale8mButton.Name = "scale8mButton";
            this.scale8mButton.Size = new System.Drawing.Size(9, 12);
            this.scale8mButton.TabIndex = 23;
            this.scale8mButton.UseVisualStyleBackColor = false;
            this.scale8mButton.Click += new System.EventHandler(this.scale8mButton_Click);
            // 
            // scale4mButton
            // 
            this.scale4mButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scale4mButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scale4mButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scale4mButton.Location = new System.Drawing.Point(5, 213);
            this.scale4mButton.Name = "scale4mButton";
            this.scale4mButton.Size = new System.Drawing.Size(9, 11);
            this.scale4mButton.TabIndex = 22;
            this.scale4mButton.UseVisualStyleBackColor = false;
            this.scale4mButton.Click += new System.EventHandler(this.scale4mButton_Click);
            // 
            // scale2mButton
            // 
            this.scale2mButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scale2mButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scale2mButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scale2mButton.Location = new System.Drawing.Point(5, 229);
            this.scale2mButton.Name = "scale2mButton";
            this.scale2mButton.Size = new System.Drawing.Size(9, 11);
            this.scale2mButton.TabIndex = 21;
            this.scale2mButton.UseVisualStyleBackColor = false;
            this.scale2mButton.Click += new System.EventHandler(this.scale2mButton_Click);
            // 
            // scale1mButton
            // 
            this.scale1mButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.scale1mButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.scale1mButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.scale1mButton.Location = new System.Drawing.Point(5, 245);
            this.scale1mButton.Name = "scale1mButton";
            this.scale1mButton.Size = new System.Drawing.Size(9, 12);
            this.scale1mButton.TabIndex = 20;
            this.scale1mButton.UseVisualStyleBackColor = false;
            this.scale1mButton.Click += new System.EventHandler(this.scale1mButton_Click);
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Arial", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(5, 185);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(11, 11);
            this.label7.TabIndex = 29;
            this.label7.Text = "8";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.camtrackOnButton);
            this.panel1.Controls.Add(this.eqFilterOnButton);
            this.panel1.Location = new System.Drawing.Point(37, 70);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(33, 59);
            this.panel1.TabIndex = 27;
            // 
            // camtrackOnButton
            // 
            this.camtrackOnButton.BackColor = System.Drawing.Color.Red;
            this.camtrackOnButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.camtrackOnButton.Location = new System.Drawing.Point(5, 24);
            this.camtrackOnButton.Name = "camtrackOnButton";
            this.camtrackOnButton.Size = new System.Drawing.Size(24, 25);
            this.camtrackOnButton.TabIndex = 19;
            this.camtrackOnButton.UseVisualStyleBackColor = false;
            this.camtrackOnButton.Visible = false;
            this.camtrackOnButton.Click += new System.EventHandler(this.camtrackOnButton_Click);
            this.camtrackOnButton.MouseHover += new System.EventHandler(this.camtrackOnButton_MouseHover);
            // 
            // eqFilterOnButton
            // 
            this.eqFilterOnButton.BackColor = System.Drawing.Color.Red;
            this.eqFilterOnButton.Location = new System.Drawing.Point(5, 5);
            this.eqFilterOnButton.Name = "eqFilterOnButton";
            this.eqFilterOnButton.Size = new System.Drawing.Size(12, 16);
            this.eqFilterOnButton.TabIndex = 19;
            this.eqFilterOnButton.UseVisualStyleBackColor = false;
            this.eqFilterOnButton.Visible = false;
            this.eqFilterOnButton.Click += new System.EventHandler(this.eqFilterOnButton_Click);
            this.eqFilterOnButton.MouseHover += new System.EventHandler(this.eqFilterOnButton_MouseHover);
            // 
            // opacityPanel
            // 
            this.opacityPanel.Controls.Add(this.opacityLinkLabel);
            this.opacityPanel.Location = new System.Drawing.Point(0, 620);
            this.opacityPanel.Name = "opacityPanel";
            this.opacityPanel.Size = new System.Drawing.Size(68, 27);
            this.opacityPanel.TabIndex = 26;
            // 
            // opacityLinkLabel
            // 
            this.opacityLinkLabel.Location = new System.Drawing.Point(5, 5);
            this.opacityLinkLabel.Name = "opacityLinkLabel";
            this.opacityLinkLabel.Size = new System.Drawing.Size(60, 20);
            this.opacityLinkLabel.TabIndex = 0;
            this.opacityLinkLabel.TabStop = true;
            this.opacityLinkLabel.Text = "opacity";
            this.opacityLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.opacityLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.opacityLinkLabel_LinkClicked);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.forwardLinkLabel);
            this.panel2.Controls.Add(this.backLinkLabel);
            this.panel2.Controls.Add(this.overlayCheckBox);
            this.panel2.Controls.Add(this.eqButton);
            this.panel2.Controls.Add(this.wptButton);
            this.panel2.Controls.Add(this.lmButton);
            this.panel2.Controls.Add(this.lmCheckBox);
            this.panel2.Controls.Add(this.topoCheckBox);
            this.panel2.Controls.Add(this.aerialCheckBox);
            this.panel2.Controls.Add(this.colorAerialCheckBox);
            this.panel2.Controls.Add(this.reliefCheckBox);
            this.panel2.Controls.Add(this.eqCheckBox);
            this.panel2.Controls.Add(this.wptCheckBox);
            this.panel2.Controls.Add(this.citiesCheckBox);
            this.panel2.Controls.Add(this.gridCheckBox);
            this.panel2.Location = new System.Drawing.Point(0, 364);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(84, 291);
            this.panel2.TabIndex = 23;
            // 
            // forwardLinkLabel
            // 
            this.forwardLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.forwardLinkLabel.Location = new System.Drawing.Point(40, 235);
            this.forwardLinkLabel.Name = "forwardLinkLabel";
            this.forwardLinkLabel.Size = new System.Drawing.Size(33, 15);
            this.forwardLinkLabel.TabIndex = 30;
            this.forwardLinkLabel.TabStop = true;
            this.forwardLinkLabel.Text = "fwd>";
            this.forwardLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.forwardLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.forwardLinkLabel_LinkClicked);
            this.forwardLinkLabel.MouseHover += new System.EventHandler(this.forwardLinkLabel_MouseHover);
            // 
            // backLinkLabel
            // 
            this.backLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backLinkLabel.Location = new System.Drawing.Point(0, 235);
            this.backLinkLabel.Name = "backLinkLabel";
            this.backLinkLabel.Size = new System.Drawing.Size(40, 15);
            this.backLinkLabel.TabIndex = 29;
            this.backLinkLabel.TabStop = true;
            this.backLinkLabel.Text = "<back";
            this.backLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.backLinkLabel_LinkClicked);
            this.backLinkLabel.MouseHover += new System.EventHandler(this.backLinkLabel_MouseHover);
            // 
            // overlayCheckBox
            // 
            this.overlayCheckBox.Location = new System.Drawing.Point(5, 109);
            this.overlayCheckBox.Name = "overlayCheckBox";
            this.overlayCheckBox.Size = new System.Drawing.Size(62, 23);
            this.overlayCheckBox.TabIndex = 28;
            this.overlayCheckBox.Text = "ovr";
            this.overlayCheckBox.CheckedChanged += new System.EventHandler(this.overlayCheckBox_CheckedChanged);
            // 
            // eqButton
            // 
            this.eqButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.eqButton.Location = new System.Drawing.Point(55, 159);
            this.eqButton.Name = "eqButton";
            this.eqButton.Size = new System.Drawing.Size(13, 16);
            this.eqButton.TabIndex = 20;
            this.eqButton.Click += new System.EventHandler(this.eqButton_Click);
            this.eqButton.MouseHover += new System.EventHandler(this.eqButton_MouseHover);
            // 
            // wptButton
            // 
            this.wptButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.wptButton.Location = new System.Drawing.Point(55, 179);
            this.wptButton.Name = "wptButton";
            this.wptButton.Size = new System.Drawing.Size(13, 16);
            this.wptButton.TabIndex = 22;
            this.wptButton.Click += new System.EventHandler(this.wptButton_Click);
            this.wptButton.MouseHover += new System.EventHandler(this.wptButton_MouseHover);
            // 
            // lmButton
            // 
            this.lmButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lmButton.Location = new System.Drawing.Point(55, 139);
            this.lmButton.Name = "lmButton";
            this.lmButton.Size = new System.Drawing.Size(13, 16);
            this.lmButton.TabIndex = 18;
            this.lmButton.Click += new System.EventHandler(this.lmButton_Click);
            this.lmButton.MouseHover += new System.EventHandler(this.lmButton_MouseHover);
            // 
            // lmCheckBox
            // 
            this.lmCheckBox.Location = new System.Drawing.Point(5, 136);
            this.lmCheckBox.Name = "lmCheckBox";
            this.lmCheckBox.Size = new System.Drawing.Size(48, 25);
            this.lmCheckBox.TabIndex = 17;
            this.lmCheckBox.Text = "lmk";
            this.lmCheckBox.CheckedChanged += new System.EventHandler(this.lmCheckBox_CheckedChanged);
            // 
            // topoCheckBox
            // 
            this.topoCheckBox.Location = new System.Drawing.Point(5, 91);
            this.topoCheckBox.Name = "topoCheckBox";
            this.topoCheckBox.Size = new System.Drawing.Size(60, 24);
            this.topoCheckBox.TabIndex = 16;
            this.topoCheckBox.Text = "topo";
            this.topoCheckBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.topoCheckBox_MouseUp);
            // 
            // aerialCheckBox
            // 
            this.aerialCheckBox.Location = new System.Drawing.Point(5, 53);
            this.aerialCheckBox.Name = "aerialCheckBox";
            this.aerialCheckBox.Size = new System.Drawing.Size(63, 23);
            this.aerialCheckBox.TabIndex = 14;
            this.aerialCheckBox.Text = "aerial";
            this.aerialCheckBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.aerialCheckBox_MouseUp);
            // 
            // colorAerialCheckBox
            // 
            this.colorAerialCheckBox.Location = new System.Drawing.Point(5, 74);
            this.colorAerialCheckBox.Name = "colorAerialCheckBox";
            this.colorAerialCheckBox.Size = new System.Drawing.Size(63, 22);
            this.colorAerialCheckBox.TabIndex = 15;
            this.colorAerialCheckBox.Text = "color";
            this.colorAerialCheckBox.MouseHover += new System.EventHandler(this.colorAerialCheckBox_MouseHover);
            this.colorAerialCheckBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.aerialCheckBox_MouseUp);
            // 
            // reliefCheckBox
            // 
            this.reliefCheckBox.Location = new System.Drawing.Point(5, 7);
            this.reliefCheckBox.Name = "reliefCheckBox";
            this.reliefCheckBox.Size = new System.Drawing.Size(60, 23);
            this.reliefCheckBox.TabIndex = 11;
            this.reliefCheckBox.Text = "relief";
            this.reliefCheckBox.CheckedChanged += new System.EventHandler(this.reliefCheckBox_CheckedChanged);
            // 
            // eqCheckBox
            // 
            this.eqCheckBox.Location = new System.Drawing.Point(5, 156);
            this.eqCheckBox.Name = "eqCheckBox";
            this.eqCheckBox.Size = new System.Drawing.Size(48, 25);
            this.eqCheckBox.TabIndex = 19;
            this.eqCheckBox.Text = "eq";
            this.eqCheckBox.CheckedChanged += new System.EventHandler(this.eqCheckBox_CheckedChanged);
            // 
            // wptCheckBox
            // 
            this.wptCheckBox.Location = new System.Drawing.Point(5, 176);
            this.wptCheckBox.Name = "wptCheckBox";
            this.wptCheckBox.Size = new System.Drawing.Size(48, 25);
            this.wptCheckBox.TabIndex = 21;
            this.wptCheckBox.Text = "wpt";
            this.wptCheckBox.CheckedChanged += new System.EventHandler(this.wptCheckBox_CheckedChanged);
            // 
            // citiesCheckBox
            // 
            this.citiesCheckBox.Location = new System.Drawing.Point(5, 26);
            this.citiesCheckBox.Name = "citiesCheckBox";
            this.citiesCheckBox.Size = new System.Drawing.Size(60, 24);
            this.citiesCheckBox.TabIndex = 13;
            this.citiesCheckBox.Text = "cities";
            this.citiesCheckBox.CheckedChanged += new System.EventHandler(this.citiesCheckBox_CheckedChanged);
            // 
            // gridCheckBox
            // 
            this.gridCheckBox.Location = new System.Drawing.Point(5, 205);
            this.gridCheckBox.Name = "gridCheckBox";
            this.gridCheckBox.Size = new System.Drawing.Size(60, 23);
            this.gridCheckBox.TabIndex = 26;
            this.gridCheckBox.Text = "grid";
            this.gridCheckBox.CheckedChanged += new System.EventHandler(this.gridCheckBox_CheckedChanged);
            // 
            // eqStylePanel
            // 
            this.eqStylePanel.Controls.Add(this.radioButton6);
            this.eqStylePanel.Controls.Add(this.radioButton3);
            this.eqStylePanel.Controls.Add(this.radioButton2);
            this.eqStylePanel.Controls.Add(this.radioButton7);
            this.eqStylePanel.Controls.Add(this.radioButton4);
            this.eqStylePanel.Controls.Add(this.radioButton5);
            this.eqStylePanel.Controls.Add(this.radioButton1);
            this.eqStylePanel.Location = new System.Drawing.Point(34, 204);
            this.eqStylePanel.Name = "eqStylePanel";
            this.eqStylePanel.Size = new System.Drawing.Size(34, 132);
            this.eqStylePanel.TabIndex = 20;
            // 
            // radioButton6
            // 
            this.radioButton6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton6.Location = new System.Drawing.Point(8, 97);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(18, 9);
            this.radioButton6.TabIndex = 8;
            this.radioButton6.Click += new System.EventHandler(this.radioButton6_Click);
            this.radioButton6.MouseHover += new System.EventHandler(this.radioButton6_MouseHover);
            // 
            // radioButton3
            // 
            this.radioButton3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton3.Location = new System.Drawing.Point(8, 44);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(18, 9);
            this.radioButton3.TabIndex = 5;
            this.radioButton3.Click += new System.EventHandler(this.radioButton3_Click);
            this.radioButton3.MouseHover += new System.EventHandler(this.radioButton3_MouseHover);
            // 
            // radioButton2
            // 
            this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton2.Location = new System.Drawing.Point(8, 27);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(18, 8);
            this.radioButton2.TabIndex = 4;
            this.radioButton2.Click += new System.EventHandler(this.radioButton2_Click);
            this.radioButton2.MouseHover += new System.EventHandler(this.radioButton2_MouseHover);
            // 
            // radioButton7
            // 
            this.radioButton7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton7.Location = new System.Drawing.Point(8, 115);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(18, 9);
            this.radioButton7.TabIndex = 9;
            this.radioButton7.Click += new System.EventHandler(this.radioButton7_Click);
            this.radioButton7.MouseHover += new System.EventHandler(this.radioButton7_MouseHover);
            // 
            // radioButton4
            // 
            this.radioButton4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton4.Location = new System.Drawing.Point(8, 62);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(18, 9);
            this.radioButton4.TabIndex = 6;
            this.radioButton4.Click += new System.EventHandler(this.radioButton4_Click);
            this.radioButton4.MouseHover += new System.EventHandler(this.radioButton4_MouseHover);
            // 
            // radioButton5
            // 
            this.radioButton5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton5.Location = new System.Drawing.Point(8, 80);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(18, 9);
            this.radioButton5.TabIndex = 7;
            this.radioButton5.Click += new System.EventHandler(this.radioButton5_Click);
            this.radioButton5.MouseHover += new System.EventHandler(this.radioButton5_MouseHover);
            // 
            // radioButton1
            // 
            this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton1.Location = new System.Drawing.Point(8, 9);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(18, 9);
            this.radioButton1.TabIndex = 3;
            this.radioButton1.Click += new System.EventHandler(this.radioButton1_Click);
            this.radioButton1.MouseHover += new System.EventHandler(this.radioButton1_MouseHover);
            // 
            // heightScrollBar
            // 
            this.heightScrollBar.Location = new System.Drawing.Point(16, 71);
            this.heightScrollBar.Name = "heightScrollBar";
            this.heightScrollBar.Size = new System.Drawing.Size(17, 265);
            this.heightScrollBar.TabIndex = 1;
            this.heightScrollBar.MouseLeave += new System.EventHandler(this.heightScrollBar_MouseLeave);
            this.heightScrollBar.ValueChanged += new System.EventHandler(this.heightScrollBar_ValueChanged);
            this.heightScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.heightScrollBar_Scroll);
            // 
            // cameraHeightLabel
            // 
            this.cameraHeightLabel.Location = new System.Drawing.Point(8, 9);
            this.cameraHeightLabel.Name = "cameraHeightLabel";
            this.cameraHeightLabel.Size = new System.Drawing.Size(60, 35);
            this.cameraHeightLabel.TabIndex = 7;
            this.cameraHeightLabel.Text = "Camera Alt., km";
            // 
            // scaleTextBox
            // 
            this.scaleTextBox.Location = new System.Drawing.Point(8, 44);
            this.scaleTextBox.Name = "scaleTextBox";
            this.scaleTextBox.Size = new System.Drawing.Size(51, 20);
            this.scaleTextBox.TabIndex = 0;
            this.scaleTextBox.Text = "0";
            this.scaleTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.scaleTextBox_KeyDown);
            this.scaleTextBox.Leave += new System.EventHandler(this.scaleTextBox_Leave);
            this.scaleTextBox.Enter += new System.EventHandler(this.scaleTextBox_Enter);
            this.scaleTextBox.MouseEnter += new System.EventHandler(this.scaleTextBox_MouseEnter);
            // 
            // vehCheckBox
            // 
            this.vehCheckBox.Location = new System.Drawing.Point(37, 169);
            this.vehCheckBox.Name = "vehCheckBox";
            this.vehCheckBox.Size = new System.Drawing.Size(60, 24);
            this.vehCheckBox.TabIndex = 22;
            this.vehCheckBox.Text = "veh";
            this.vehCheckBox.CheckedChanged += new System.EventHandler(this.vehCheckBox_CheckedChanged);
            // 
            // mainPrintDocument
            // 
            this.mainPrintDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.mainPrintDocument_PrintPage);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.ClientSize = new System.Drawing.Size(1066, 794);
            this.Controls.Add(this.centerPanel);
            this.Controls.Add(this.bottomPanel);
            this.KeyPreview = true;
            this.Menu = this.mainMenu1;
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Windows Tiler";
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.DragLeave += new System.EventHandler(this.MainForm_DragLeave);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.Leave += new System.EventHandler(this.MainForm_Leave);
            this.Click += new System.EventHandler(this.MainForm_Click);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.Move += new System.EventHandler(this.MainForm_Move);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.bottomPanel.ResumeLayout(false);
            this.sb1Panel.ResumeLayout(false);
            this.sb2Panel.ResumeLayout(false);
            this.centerPanel.ResumeLayout(false);
            this.picturePanel.ResumeLayout(false);
            this.hintsPanel.ResumeLayout(false);
            this.centerHintsPanel.ResumeLayout(false);
            this.bottomHintsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainPictureBox)).EndInit();
            this.leftPanel.ResumeLayout(false);
            this.leftPanel.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.opacityPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.eqStylePanel.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		#region Lifecycle methods

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			Project.goingDown = true;
			Thread.Sleep(100);

			if( disposing )
			{
//				if (components != null) 
//				{
//					components.Dispose();
//				}
			}
			if(m_pictureManager != null)
			{
				m_pictureManager.shutdown();
			}
			base.Dispose( disposing );

			HideGreeting();
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			LibSys.StatusBar.WriteLine("MainForm: closing");
			Project.mainFormMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);

			if(WaypointsCache.isDirty)
			{
				string message = "Closing: You created or deleted waypoints or tracks - here is a chance to save them.\n\nDo you want to save the data?";
				if(Project.YesNoBox(null, message)) 
				{
					new DlgWaypointsManager(m_pictureManager, m_cameraManager, 0, -1).ShowDialog();
					e.Cancel = true;
					return;
				}
			}
			Project.goingDown = true;
			Project.Closing();		// save options and license, shut down ThreadPool
		}

		private void MainForm_Move(object sender, System.EventArgs e)
		{
			if(!inResize)
			{
				Project.mainFormMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
				if(!Project.mainFormMaximized)
				{
					Project.mainFormX = this.Location.X;
					Project.mainFormY = this.Location.Y;
				}
			}
		}

		bool inResize = true;	// initially true

		private void MainForm_Resize(object sender, System.EventArgs e)
		{
			if(!inResize)
			{
				Project.mainFormMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
				if(!Project.mainFormMaximized)
				{
					Project.mainFormWidth = this.ClientSize.Width;
					Project.mainFormHeight = this.ClientSize.Height;
					Project.mainFormX = this.Location.X;
					Project.mainFormY = this.Location.Y;
				}
			}
		}
		#endregion

		#region IMainCommand implementation

		// IMainCommand implementation:
		public void zoomOut()
		{
			allowScroll = true;
			if(heightScrollBar.Value > heightScrollBar_Minimum)
			{
				heightScrollBar.Value--;
				collectHeightScrollBarValue();
			}
			Cursor.Current = Cursors.Default;
		}

		// IMainCommand implementation:
		public void zoomIn()
		{
			allowScroll = true;
			if(heightScrollBar.Value < heightScrollBar_Maximum)
			{
				heightScrollBar.Value++;
				collectHeightScrollBarValue();
			} 
			Cursor.Current = Cursors.Default;
		}

		// IMainCommand implementation:
		public void zoomOutLarge()
		{
			allowScroll = true;
			if(heightScrollBar.Value > heightScrollBar_Minimum + 10)
			{
				heightScrollBar.Value -= 10;
				collectHeightScrollBarValue();
			}
			else if(heightScrollBar.Value > heightScrollBar_Minimum)
			{
				heightScrollBar.Value = heightScrollBar_Minimum;
				collectHeightScrollBarValue();
			}
			Cursor.Current = Cursors.Default;
		}

		// IMainCommand implementation:
		public void zoomInLarge()
		{
			allowScroll = true;
			if(heightScrollBar.Value < heightScrollBar_Maximum - 10)
			{
				heightScrollBar.Value += 10;
				collectHeightScrollBarValue();
			}
			else if(heightScrollBar.Value < heightScrollBar_Maximum)
			{
				heightScrollBar.Value = heightScrollBar_Maximum;
				collectHeightScrollBarValue();
			}
			Cursor.Current = Cursors.Default;
		}

		// IMainCommand implementation:
		public void photoViewerUp(object wpt)
		{
			Waypoint waypoint = (Waypoint)wpt;

			DlgPhotoFullSize.BringFormUp(null, waypoint);
		}

		public void photoEditorUp(object wpt)
		{
			Waypoint waypoint = (Waypoint)wpt;

			DlgPhotoDraw dlgPhotoDraw = new DlgPhotoDraw(waypoint, null);

			if(DlgPhotoDraw.m_dlgSizeX < 100)
			{
				dlgPhotoDraw.Top = this.PointToScreen(new Point(0, 0)).Y + 20;
				dlgPhotoDraw.Left = this.PointToScreen(new Point(0, 0)).X + 20;
			}
	
			dlgPhotoDraw.ShowDialog();
		}

		// IMainCommand implementation:
		public void eqEnabled(bool enabled)
		{
			eqCheckBox.Checked = enabled;
		}

		// IMainCommand implementation:
		public void eqFilterOn(bool on)			// on/off earthquakes filter indicator
		{
			eqFilterOnButton.Visible = on;
		}

		// IMainCommand implementation:
		public void camtrackOn(bool on)			// on/off camera track indicator
		{
			camtrackOnButton.Visible = on;
			this.heightScrollBar.Enabled = !on;
			this.scaleTextBox.Enabled = !on;
			this.favMainMenuItem.Enabled = !on;
		}

		// IMainCommand implementation:
		public object getTrackById(long trackId)
		{
			return WaypointsCache.getTrackById(trackId);	// type Track, may be null
		}

		// IMainCommand implementation:
		public void wptEnabled(bool enable)			// enable/disable waypoints layer
		{
			wptCheckBox.Checked = enable;
		}

		// LibSys.IMainCommand implementation:
		public void trackProperties(long trackId)
		{
			Track trk = WaypointsCache.getTrackById(trackId);
			if(trk != null)
			{
				new DlgWaypointsManager(m_pictureManager, m_cameraManager, trk.isRoute ? 3 : 1, trackId).Show();
			}
		}

		// IMainCommand implementation:
		public Form gpsManagerForm()
		{
			return DlgGpsManager.This;
		}

		// IMainCommand implementation:
		public SortedList getWaypointsWithThumbs(long trkid)
		{
			return PhotoWaypoints.getWaypointsWithThumbs(trkid);
		}

		// IMainCommand implementation:
		public string toUtmString(double lon, double lat, double elev)
		{
			string ret = "";		// UTM = 11S 0433603E 3778359N

			LonLatPt lonlat = new LonLatPt();
			lonlat.Lat = lat;
			lonlat.Lon = lon;

			UtmPt utmpt = Projection.LonLatPtToUtmNad83Pt(lonlat);

			ret = "" + utmpt.Zone + "S " + string.Format("{0:d07}", (int)Math.Round(utmpt.X)) + "E " + string.Format("{0:d07}", (int)Math.Round(utmpt.Y)) + "N";

			return ret;
		}

		// IMainCommand implementation:
		public bool fromUtmString(string text, out double lng, out double lat)
		{
			bool ret = false;
			lng = 0.0d;
			lat = 0.0d;

			try
			{
				// UTM = 11S E433603 N3778359
				char[] sep = new Char[] { ' ' };
				string[] split = text.Split(sep);
				string sZ = split[0];
				string sE = split[1];
				string sN = split[2];

				if(sZ.ToUpper().EndsWith("S"))
				{
					sZ = sZ.Substring(0, sZ.Length - 1);
				}

				if(sE.ToUpper().EndsWith("E"))
				{
					sE = sE.Substring(0, sE.Length - 1);
				}

				if(sN.ToUpper().EndsWith("N"))
				{
					sN = sN.Substring(0, sN.Length - 1);
				}

				UtmPt utmpt = new UtmPt();
				utmpt.Zone = Convert.ToInt32(sZ);
				utmpt.X = Convert.ToDouble(sE);
				utmpt.Y = Convert.ToDouble(sN);

				LonLatPt pt = Projection.UtmNad83PtToLonLatPt(utmpt);

				lat = pt.Lat;
				lng = pt.Lon;

				ret = true;
			}
			catch {}

			return ret;
		}

		// IMainCommand implementation
		public void setMapMode(string mode)
		{
			switch(mode)
			{
				case "none":
					if(aerialCheckBox.Checked)
					{
						aerialCheckBox.Checked = false;
						aerialCheckBox_MouseUp(aerialCheckBox, null);
					}
					if(topoCheckBox.Checked)
					{
						topoCheckBox.Checked = false;
						topoCheckBox_MouseUp(topoCheckBox, null);
					}
					if(colorAerialCheckBox.Checked)
					{
						colorAerialCheckBox.Checked = false;
						aerialCheckBox_MouseUp(colorAerialCheckBox, null);
					}
					break;
				case "aerial":
					if(!aerialCheckBox.Checked)
					{
						aerialCheckBox.Checked = true;
						aerialCheckBox_MouseUp(aerialCheckBox, null);
					}
					break;
				case "topo":
					if(!topoCheckBox.Checked)
					{
						topoCheckBox.Checked = true;
						topoCheckBox_MouseUp(topoCheckBox, null);
					}
					break;
				case "color":
					if(!colorAerialCheckBox.Checked)
					{
						colorAerialCheckBox.Checked = true;
						aerialCheckBox_MouseUp(colorAerialCheckBox, null);
					}
					break;
			}
		}

		// IMainCommand implementation:
		public Bitmap getVehicleImage(string sym)
		{
			return VehiclesCache.getVehicleImage(sym);
		}

		public bool PlannerPaneVisible()
		{
			return this.plannerPanel.Visible;
		}

		#endregion

		#region Camera height scrollbar and text box

		public void LocationChangedCallback(GeoCoord location)
		{
			sbScrolling = true;
			double elevKm = location.Elev / 1000.0d;
			setCameraAltitudeTextBoxValue(elevKm);
			if(elevKm >= Project.cameraHeightMin && elevKm <= Project.CAMERA_HEIGHT_MAX) 
			{
				setScrollBarValue(elevKm);
			}
			sbScrolling = false;
			backLinkLabel.Enabled = (Project.cameraPositionsBackStack.Count > 1);
			forwardLinkLabel.Enabled = (Project.cameraPositionsFwdStack.Count > 0);
		}

		private const double LOG_FACTOR = 9.0d;

		private void setScrollBarValue(double elevKm)
		{
			try 
			{
				if(elevKm < Project.cameraHeightMin)
				{
					elevKm = Project.cameraHeightMin;
				}
				elevKm = 1.0d + elevKm - Project.cameraHeightMin;		// always more than 1.0d

				double bar = heightScrollBar_Maximum - Math.Log(elevKm) * LOG_FACTOR;
				int barValue = (int)Math.Round(bar);
				//LibSys.StatusBar.Trace("IP: elevKm=" + elevKm + " bar=" + bar + " barValue=" + barValue);
				heightScrollBar.Value = barValue;
			} 
			catch (Exception e)
			{
			}
		}

		private double getScrollBarValue()
		{
			double elevKm;
			double dValue = (double)(heightScrollBar_Maximum - heightScrollBar.Value);
			elevKm = Math.Exp(dValue / LOG_FACTOR);
			elevKm = elevKm - 1.0d + Project.cameraHeightMin;

			/*
			 * reverse formula debugging:
			elevKm = 1.0d + elevKm - Project.cameraHeightMin;		// always more than 1.0d
			double bar = heightScrollBar_Maximum - Math.Log(elevKm) * LOG_FACTOR;
			int barValue = (int)Math.Round(bar);
			LibSys.StatusBar.Trace("IP: scrollbar.Value=" + heightScrollBar.Value + "  elevKm=" + elevKm + " barValue=" + barValue + " bar=" + bar);
			*/

			return elevKm;
		}

		private void setCameraAltitudeTextBoxValue(double elevKm)
		{
			switch(Project.unitsDistance)
			{
				case Distance.UNITS_DISTANCE_KM:
				case Distance.UNITS_DISTANCE_M:
					if(elevKm < 10.0d)
					{
						elevKm = Math.Round(elevKm * 100.0d) / 100.0d;
					}
					else if(elevKm < 100.0d)
					{
						elevKm = Math.Round(elevKm * 10.0d) / 10.0d;
					}
					else if(elevKm > 10000.0d)
					{
						elevKm = Math.Round(elevKm / 100.0d) * 100.0d;
					}
					else if(elevKm > 1000.0d)
					{
						elevKm = Math.Round(elevKm / 10.0d) * 10.0d;
					}
					else
					{
						elevKm = Math.Round(elevKm);
					}
					scaleTextBox.Text = "" + elevKm;
					cameraHeightLabel.Text = "Camera Alt., km";
					break;
				default:
				{
					double elevMiles = elevKm * 1000.0d / Distance.METERS_PER_MILE;
					if(elevMiles < 10.0d)
					{
						elevMiles = Math.Round(elevMiles * 100.0d) / 100.0d;
					}
					else if(elevMiles < 100.0d)
					{
						elevMiles = Math.Round(elevMiles * 10.0d) / 10.0d;
					}
					else if(elevMiles > 10000.0d)
					{
						elevMiles = Math.Round(elevMiles / 100.0d) * 100.0d;
					}
					else if(elevMiles > 1000.0d)
					{
						elevMiles = Math.Round(elevMiles / 10.0d) * 10.0d;
					}
					else
					{
						elevMiles = Math.Round(elevMiles);
					}
					scaleTextBox.Text = "" + elevMiles;
					cameraHeightLabel.Text = "Camera Alt., mi";
				}
					break;
			}
		}

		// timer makes it possible to leave mouse inside scroll bar (Win98).
		// Normally MouseLeave triggers collectHeightScrollBarValue(), works fine on XP - it calls MouseLeave when you release the button,
		// but Win98 seems to not through MouseLeave on button up.
		DateTime lastScrollAttempt = DateTime.MinValue;
		private System.Windows.Forms.Timer scrollTimer = new System.Windows.Forms.Timer();
		private bool scrollTimerActive = false;
		private const int SCROLL_DELAY = 1000; 

		public void scrollHandler(object obj, System.EventArgs args)
		{
			if((DateTime.Now - lastScrollAttempt).TotalMilliseconds >= SCROLL_DELAY)
			{
#if DEBUG
//				LibSys.StatusBar.Trace("timer: finished scrolling");
#endif
				scrollTimer.Tick -= new EventHandler(scrollHandler);
				scrollTimerActive = false;
				collectHeightScrollBarValue();
			}
			else
			{
#if DEBUG
//				LibSys.StatusBar.Trace("waiting to complete scroll");
#endif
				scrollTimer.Enabled = true;		// wait another second
			}
		}

		private bool allowScroll = false;		// set to true after the first scroll by mouse, and stays true
		private bool sbScrolling = false;		// set to true when we are in process of scrolling, till value is accepted and applied to the map.

		private void heightScrollBar_ValueChanged(object sender, System.EventArgs e)
		{
			sbScrolling = true;		// means we let the text box change, but not the map.
			double elevKm = getScrollBarValue();
			setCameraAltitudeTextBoxValue(elevKm);

			if(allowScroll && !scrollTimerActive)
			{
#if DEBUG
				LibSys.StatusBar.Trace("activating scroll timer");
#endif
				scrollTimer.Interval = 500;
				scrollTimer.Tick += new EventHandler(scrollHandler);
				scrollTimer.Start();
				scrollTimerActive = true;
			}
			lastScrollAttempt = DateTime.Now;
		}

		private void heightScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			if(!allowScroll)
			{
				this.hintsPanel.Visible = false;
			}
			// first actual move of scroll bar allows changes on MouseLeave.
			allowScroll = true;
		}

		private void heightScrollBar_MouseLeave(object sender, System.EventArgs e)
		{
			collectHeightScrollBarValue();
		}

		private bool canSpoil = true;

		private void collectHeightScrollBarValue()
		{
			try 
			{
				if(sbScrolling && allowScroll) 
				{
					// accept the value and apply to the map
					sbScrolling = false;
					if(canSpoil)
					{
						m_cameraManager.SpoilPicture();
					}
					double elev = getScrollBarValue() * 1000.0d;		// meters
					m_cameraManager.Elev = elev;						// calls ProcessCameraMove();
					setCameraAltitudeTextBoxValue(m_cameraManager.Elev / 1000.0d);
				}
			} 
			catch {}
			canSpoil = true;
		}

		private void scaleTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			this.hintsPanel.Visible = false;

			switch (e.KeyData) 
			{
				case Keys.Enter:
					try 
					{
						double elev;
						switch(Project.unitsDistance)
						{
							case Distance.UNITS_DISTANCE_KM:
							case Distance.UNITS_DISTANCE_M:
							{
								double elevKm = double.Parse(scaleTextBox.Text);
								elev = elevKm * 1000.0d;	// meters
							}
								break;
							default:
							{
								double elevMiles = double.Parse(scaleTextBox.Text);
								elev = elevMiles * Distance.METERS_PER_MILE;	// meters
							}
								break;
						}
						double elevMax = (double)Project.CAMERA_HEIGHT_MAX * 1000.0d;
						double elevMin = (double)Project.cameraHeightMin * 1000.0d;
						elev = (elev > elevMax) ? elevMax : elev;
						elev = (elev < elevMin) ? elevMin : elev;
						setScrollBarValue(elev / 1000.0d);
						//LibSys.StatusBar.Trace("  scaleTextBox.Text=" + scaleTextBox.Text + "   elev=" + elev + "  elevKm=" + elevKm);
						m_cameraManager.SpoilPicture();
						m_cameraManager.Elev = elev;	// calls ProcessCameraMove();
					}
					catch (Exception exc) 
					{
						LibSys.StatusBar.Error("can't parse as double: " + scaleTextBox.Text);
					}
					break;
			}
		}

		// make sure that scaleTextBox keyboard input is not intercepted by ProcessDialogKey()
		private bool inScaleBox = false;
		private bool warnedScaleBoxYellow = false;

		private void scaleTextBox_Enter(object sender, System.EventArgs e)
		{
			inScaleBox = true;
		}

		private void scaleTextBox_Leave(object sender, System.EventArgs e)
		{
			inScaleBox = false;
		}

		private void scaleTextBox_MouseEnter(object sender, System.EventArgs e)
		{
			if(!warnedScaleBoxYellow && !Project.serverAvailable)
			{
				Project.ShowPopup(scaleTextBox, "yellow color of the camera altitude box\r\nmeans the program is working off-line.\r\nCheck your Internet connection.", Point.Empty);
				warnedScaleBoxYellow = true;
			}
		}

		#endregion

		#region Opacity Popup

		// bring up the popup:
		private void opacityLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			opacityPopup.Top = opacityLinkLabel.PointToScreen(new Point(0, 0)).Y;
			opacityPopup.Left = opacityLinkLabel.PointToScreen(new Point(0, 0)).X;
			opacityPopup.Show();
		}

		#endregion

		#region Key Clicks processing

		private DateTime modifierPressedTime = DateTime.MaxValue;
		private bool controlDown = false;
		private bool shiftDown = false;
		private bool altDown = false;

		protected override bool ProcessDialogKey(Keys keyData)
		{
			//LibSys.StatusBar.Trace("ProcessDialogKey: " + keyData);
			if(inScaleBox)
			{
				return base.ProcessDialogKey(keyData);
			}
			this.hintsPanel.Visible = false;

			keystrokeProcessed = true;		// by default do not let KeyPress handle the keystroke
			switch (keyData)
			{
				case Keys.PageUp:
					m_cameraManager.zoomOutLarge();
					break;
				case Keys.PageDown:
					m_cameraManager.zoomInLarge();
					break;
				case Keys.Down:
					m_cameraManager.shiftDown();
					break;
				case Keys.Up:
					m_cameraManager.shiftUp();
					break;
				case Keys.Left:
					m_cameraManager.shiftLeft();
					break;
				case Keys.Right:
					m_cameraManager.shiftRight();
					break;
				case Keys.Back:
					doBack();
					break;
				case Keys.Control | Keys.W:
					newWaypointMenuItem_Click(null, EventArgs.Empty);
					break;
				case Keys.Control | Keys.R:
					break;
				case Keys.Control | Keys.G:
					break;
				case Keys.Control | Keys.B:
					break;
				case Keys.Control | Keys.Alt | Keys.R:
					break;
				case Keys.Control | Keys.Alt | Keys.G:
					break;
				case Keys.Control | Keys.Alt | Keys.B:
					break;
				case Keys.Escape:
					LayerWaypoints.This.makeRouteMode = false;
					LibSys.StatusBar.WriteLine("* finished making route");
					break;
				case Keys.Control | Keys.S:
					saveState();
					LibSys.StatusBar.WriteLine("* Saved program state");
					break;
				case Keys.Control | Keys.P:
					saveImageMenuItem_Click(null, EventArgs.Empty);
					break;
				case Keys.Alt | Keys.F4:
					Project.mainFormMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
					//Project.Exit(); //uu... abrupt exit.
					this.Close();
					break;
				case Keys.F1:
					showHelp();
					break;
				case Keys.F5:
					m_pictureManager._Refresh();
					LibSys.StatusBar.WriteLine("* Refreshed the picture");
					break;
				case Keys.Control | Keys.F5:
					LibSys.StatusBar.WriteLine("* Reloading and refreshing the picture...");
					m_cameraManager.ReloadAndRefresh();		// will end up in LayersManager:t_cameraMoved() in a separate thread
					break;
				default:
					if ((int)(Keys.Control & keyData) != 0) 
					{
//						LibSys.StatusBar.Trace("*ProcessDialogKey: Control down");
						controlDown = true;
						modifierPressedTime = DateTime.Now;
						keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					}
					if ((int)(Keys.Shift & keyData) != 0) 
					{
						//The Shift key is pressed. Do something here if you want.
//						LibSys.StatusBar.Trace("*ProcessDialogKey: Shift down");
						shiftDown = true;
						modifierPressedTime = DateTime.Now;
						keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					}
					if ((int)(Keys.Alt & keyData) != 0) 
					{
						//The Alt key is pressed. Do something here if you want.
						//LibSys.StatusBar.Trace("*ProcessDialogKey: Alt down");
						altDown = true;
						modifierPressedTime = DateTime.Now;
						keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					}
//					else 
//					{
//						keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
//					}
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

		private void MainForm_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (keystrokeProcessed) 
			{
				return;
			}

			if(e.KeyChar.Equals('w')) 
			{
				m_cameraManager.shiftUp();
			}
			else if(e.KeyChar.Equals('z')) 
			{
				m_cameraManager.shiftDown();
			}
			else if(e.KeyChar.Equals('s')) 
			{
				m_cameraManager.shiftRight();
			}
			else if(e.KeyChar.Equals('a')) 
			{
				m_cameraManager.shiftLeft();
			}
			else if(e.KeyChar.Equals('+') || e.KeyChar.Equals('=')) 
			{
				m_cameraManager.zoomIn();
			}
			else if(e.KeyChar.Equals('-') || e.KeyChar.Equals('_')) 
			{
				m_cameraManager.zoomOut();
			}
		}

		private void MainForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
//			string str = "  Keys.ControlKey=" + String.Format("{0:X}", Keys.ControlKey);

			if((e.KeyData & Keys.KeyCode) == Keys.ControlKey)
			{
				controlDown = false;
//				str += " C";
			}
			if((e.KeyData & Keys.KeyCode) == Keys.ShiftKey)
			{
				shiftDown = false;
//				str += " S";
			}
			if((e.KeyData & Keys.KeyCode) == Keys.Menu)
			{
				altDown = false;
//				str += " A";
			}

//			LibSys.StatusBar.Trace("*MainForm_KeyUp: e.keyData=" + String.Format("{0:X}", e.KeyData) + str  + "  " + DateTime.Now.Ticks);
		}

		#endregion

		#region Mouse processing

		private void MainForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Project.ClearPopup();
		}

		private void MainForm_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int mouseWheelAction = Project.mouseWheelAction;

			if(mouseWheelAction == 0)
			{
				return;
			}

			modifierPressedTime = DateTime.Now;

			int delta = (mouseWheelAction == 2) ? e.Delta : -e.Delta;
			bool dirUpDown = !controlDown && btnDown == MouseButtons.None;	// any mouse btn or key Ctrl causes panning to the side
			switch(mouseWheelAction)
			{
				case 1:
				case 2:
					if(shiftDown)
					{
						mouseWheelAction = 3;
					}
					break;
				case 3:
					if(shiftDown)
					{
						mouseWheelAction = 1;
					}
					break;
			}

			const int deltaTreshold = 1500;

			switch(mouseWheelAction)
			{
				case 1:		// zoom Google Earth style
				case 2:		// zoom reverse 
					// for some reason one click of the mouse wheel I have is 120 notches
					if(delta > 0)
					{
						canSpoil = false;
						if(delta <= deltaTreshold)
						{
							m_cameraManager.zoomIn();
						}
						else
						{
							m_cameraManager.zoomInLarge();
						}
					}
					else
					{
						canSpoil = false;
						if(delta >= -deltaTreshold)
						{
							m_cameraManager.zoomOut();
						}
						else
						{
							m_cameraManager.zoomOutLarge();
						}
					}
					break;

				case 3:
					if(delta > 0)
					{
						if(delta <= deltaTreshold)
						{
							if(dirUpDown)
							{
								m_cameraManager.shiftUpSmall();
							}
							else
							{
								m_cameraManager.shiftRightSmall();
							}
						}
						else
						{
							if(dirUpDown)
							{
								m_cameraManager.shiftUpNoSpoil();
							}
							else
							{
								m_cameraManager.shiftRightNoSpoil();
							}
						}
					}
					else
					{
						if(delta >= -deltaTreshold)
						{
							if(dirUpDown)
							{
								m_cameraManager.shiftDownSmall();
							} 
							else
							{
								m_cameraManager.shiftLeftSmall();
							}
						}
						else
						{
							if(dirUpDown)
							{
								m_cameraManager.shiftDownNoSpoil();
							}
							else
							{
								m_cameraManager.shiftLeftNoSpoil();
							}
						}
					}
					break;
			}
		}

		private bool rightClick = false;

		private void mainPictureBox_Click(object sender, System.EventArgs e)
		{
			try {
				mainPictureBox.Focus();
				if(!rightClick)
				{
					if(!m_cameraManager.pictureClick(e, controlDown, shiftDown, altDown))
					{
						m_pictureManager.pictureClick(e, controlDown, shiftDown, altDown);
					}
				}
				this.hintsPanel.Visible = false;
			} 
			catch {}
		}

		private void mainPictureBox_DoubleClick(object sender, System.EventArgs e)
		{
			try {
				m_cameraManager.pictureDoubleClick(e, controlDown, shiftDown, altDown);
			} 
			catch {}
		}

		private void mainPictureBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			try 
			{
				m_cameraManager.pictureMouseMove(e, controlDown, shiftDown, altDown);
				m_pictureManager.pictureMouseMove(e, controlDown, shiftDown, altDown);
			} 
			catch {}
		}

		private bool hintsPanelClicked = false;

		private void hintsPanel_Click(object sender, System.EventArgs e)
		{
			this.hintsPanel.Visible = false;
			hintsPanelClicked = true;
		}

		private MouseButtons btnDown = MouseButtons.None;
		private Point rightClickPoint;
		private Waypoint rightClickWaypoint = null;
		private Vehicle rightClickVehicle = null;
		private Earthquake rightClickEarthquake = null;
		private MenuItem menuItemWptNaming = new MenuItem("Waypoints Naming Style"); 
		private MenuItem menuItemTools = new MenuItem("Tools"); 

		/// <summary>
		/// build context menu on the right mouse click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mainPictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			try 
			{
				btnDown = e.Button;
				Project.ClearPopup();
				rightClickWaypoint = null;
				rightClickEarthquake = null;

				LibSys.StatusBar.Trace("mainPictureBox_MouseDown: ctrl=" + controlDown + "  shift=" + shiftDown + "  alt=" + altDown);

				m_pictureManager.pictureMouseDown(e, controlDown, shiftDown, altDown);
				m_cameraManager.pictureMouseDown(e, controlDown, shiftDown, altDown);

				bool backClick = (e.Button == MouseButtons.XButton1);
				if(backClick)
				{
					doBack();
				}
				bool forwardClick = (e.Button == MouseButtons.XButton2);
				if(forwardClick)
				{
					doForward();
				}
				rightClick = (e.Button == MouseButtons.Right);
				if(rightClick)
				{
					rightClickPoint = new Point(e.X, e.Y);
					if(m_cameraManager.pointInsideDragRectangle(rightClickPoint)) 
					{
						// process right-clicking into drag rectangle here
						// Clear all previously added MenuItems.
						mainPictureBox_ContextMenu.MenuItems.Clear();

						int index = 0;
						MenuItem menuItem = null;

						menuItem = new MenuItem("Zoom in");
						menuItem.Index = index++;
						menuItem.Click += new System.EventHandler(cm_zoomToDragRectangleMenuItem_Click);
						mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

						menuItem = new MenuItem("Preload Tiles");
						menuItem.Index = index++;
						menuItem.Click += new System.EventHandler(cm_preloadMenuItem_Click);
						mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

						menuItem = new MenuItem("GoogleEarth - show selected area");
						menuItem.Index = index++;
						menuItem.Click += new System.EventHandler(cm_areaGoogleEarthMenuItem_Click);
						mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

						menuItem = new MenuItem("-");
						menuItem.Index = index++;
						mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

						menuItem = new MenuItem("Help");
						menuItem.Index = index++;
						menuItem.Click += new System.EventHandler(cm_helpMenuItem_Click);
						mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
					}
					else
					{
						// generic right-click, or right-click outside the drag rectangle
						m_cameraManager.resetDrag(true);

						// Clear all previously added MenuItems.
						mainPictureBox_ContextMenu.MenuItems.Clear();

						int index = 0;
						MenuItem menuItem = null;
						bool putGeTrackItem = false;
						bool putGeWptItem = false;

						LiveObject lo = m_pictureManager.evalPictureClick(e, controlDown, shiftDown, altDown);

						if(LayerWaypoints.This.makeRouteMode)
						{
							if(lo != null)
							{
								if(lo.GetType().Equals(Waypoint.getType()))
								{
									rightClickWaypoint = (Waypoint)lo;

									if(rightClickWaypoint.TrackId == -1)
									{
										menuItem = new MenuItem("Add waypoint to route");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_addWptToRouteMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}

									if(rightClickWaypoint.TrackId == LayerWaypoints.This.routeBeingBuilt.Id)
									{
										menuItem = new MenuItem("Edit routepoint");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_editMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

										menuItem = new MenuItem("Move routepoint");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_moveMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

										menuItem = new MenuItem("Remove waypoint from the route");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_removeWptFromRouteMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}
								}
								else if(lo.GetType().Equals(Earthquake.getType()))
								{
									rightClickEarthquake = (Earthquake)lo;

									menuItem = new MenuItem("Add earthquake to route");
									menuItem.Index = index++;
									menuItem.Click += new System.EventHandler(cm_addEqToRouteMenuItem_Click);
									mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
								}
							}
							menuItem = new MenuItem("End making route [Esc]");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_endrouteMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
						}
						else
						{
							if(lo != null)
							{
								if(lo.GetType().Equals(Waypoint.getType()))
								{
									rightClickWaypoint = (Waypoint)lo;
									if(!Project.allowMapPopups)
									{
										menuItem = new MenuItem("Detail (in popup)");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_detailMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}
									if((cm_trackId=rightClickWaypoint.TrackId) != -1)
									{
										Track trk = WaypointsCache.getTrackById(cm_trackId);
										if(trk != null)
										{
											menuItem = new MenuItem("Track Detail [" + cm_trackId + ": " + trk.Name + "]");
											menuItem.Index = index++;
											menuItem.Click += new System.EventHandler(cm_trackMenuItem_Click);
											mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

											menuItem = new MenuItem("View Whole Trip");
											menuItem.Index = index++;
											menuItem.Click += new System.EventHandler(cm_trackZoomMenuItem_Click);
											mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

											menuItem = new MenuItem("Hide Track");
											menuItem.Index = index++;
											menuItem.Click += new System.EventHandler(cm_trackHideMenuItem_Click);
											mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

											menuItem = new MenuItem("Break into Trips");
											menuItem.Index = index++;
											menuItem.Click += new System.EventHandler(cm_trackUngroupMenuItem_Click);
											mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

											menuItem = new MenuItem("Break Here");
											menuItem.Index = index++;
											menuItem.Click += new System.EventHandler(cm_trackUngroupHereMenuItem_Click);
											mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

											menuItem = new MenuItem("Preload Tiles Along the Route");
											menuItem.Index = index++;
											menuItem.Click += new System.EventHandler(cm_preloadAlongRouteMenuItem_Click);
											mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

											menuItem = new MenuItem("Save Track As...");
											menuItem.Index = index++;
											menuItem.Click += new System.EventHandler(cm_trackSaveAsMenuItem_Click);
											mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

											putGeTrackItem = true;
										}
									}
									else
									{
										putGeWptItem = true;
									}

									if(rightClickWaypoint.Url != null && rightClickWaypoint.Url.StartsWith("http://") && Project.serverAvailable)
									{
										menuItem = new MenuItem("Detail (in browser)");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_browseMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}

									if(rightClickWaypoint.ThumbImage == null)
									{
										menuItem = new MenuItem(cm_trackId < 0 ? "Edit waypoint" : "Edit trackpoint");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_editMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

										menuItem = new MenuItem(cm_trackId < 0 ? "Move waypoint" : "Move trackpoint");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_moveMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

										menuItem = new MenuItem(cm_trackId < 0 ? "Delete waypoint" : "Delete trackpoint");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_deleteMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}
									else
									{
										menuItem = new MenuItem("View photo");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_viewPhotoMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

										menuItem = new MenuItem("Edit/Draw over photo");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_viewPhotoDrawMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}

									if(putGeTrackItem)
									{
										menuItem = new MenuItem("GoogleEarth - show trip");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_trackGoogleEarthMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}
									else if(putGeWptItem)
									{
										menuItem = new MenuItem("GoogleEarth - show waypoint");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_wptGoogleEarthMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}

									menuItem = new MenuItem("-");
									menuItem.Index = index++;
									mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
								}
								else if(lo.GetType().Equals(Vehicle.getType()) || lo.GetType().Equals(VehicleGps.getType()))
								{
									rightClickVehicle = (Vehicle)lo;
									if(!Project.allowMapPopups)
									{
										menuItem = new MenuItem("Detail (in popup)");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_detailMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}
									if(rightClickVehicle.Url != null && rightClickVehicle.Url.StartsWith("http:") && Project.serverAvailable)
									{
										menuItem = new MenuItem("Detail (in browser)");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_browseMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}
									
									menuItem = new MenuItem("Delete vehicle");
									menuItem.Index = index++;
									menuItem.Click += new System.EventHandler(cm_deleteVehicleMenuItem_Click);
									mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

									menuItem = new MenuItem("-");
									menuItem.Index = index++;
									mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
								}
								else if(lo.GetType().Equals(Earthquake.getType()))
								{
									rightClickEarthquake = (Earthquake)lo;
									if(!Project.allowMapPopups)
									{
										menuItem = new MenuItem("Detail (in popup)");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_detailMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}
									if(rightClickEarthquake.Url != null && rightClickEarthquake.Url.StartsWith("http:") && Project.serverAvailable)
									{
										menuItem = new MenuItem("Detail (in browser)");
										menuItem.Index = index++;
										menuItem.Click += new System.EventHandler(cm_browseMenuItem_Click);
										mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
									}

									menuItem = new MenuItem("GoogleEarth - show earthquake");
									menuItem.Index = index++;
									menuItem.Click += new System.EventHandler(cm_eqGoogleEarthMenuItem_Click);
									mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

									menuItem = new MenuItem("-");
									menuItem.Index = index++;
									mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
								}
							}

							menuItem = new MenuItem((Project.allowMapPopups ? "Disable" : "Enable") + " Popups");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_popupsMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("New Waypoint");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_newWaypointMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("New Route");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_newRouteMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("Waypoints Manager");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(this.trackManagerMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							if(lo == null)
							{
								menuItem = new MenuItem("GoogleEarth - this point on map");
								menuItem.Index = index++;
								menuItem.Click += new System.EventHandler(cm_pointGoogleEarthMenuItem_Click);
								mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
							}

							menuItem = new MenuItem("-");
							menuItem.Index = index++;
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("Center here (same as double click)");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_centerMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("Zoom In (same as +)");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_zoomInMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("Zoom Out (same as - or Shift/Click)");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_zoomOutMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem((Project.trackElevColor ? "Disable tracks elevation color" : "Color code tracks elevation"));
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_colorCodeMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem((Project.trackSpeedColor ? "Disable tracks speed color" : "Color code tracks by speed"));
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_colorSpeedMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItemWptNaming.Index = index++;
							menuItemWptNaming.MenuItems.Clear();
							for (int i=0; i < Project.waypointNameStyleChoices.Length ;i++)
							{
								menuItem = new MenuItem(Project.waypointNameStyleChoices[i]);
								menuItem.Checked = Project.waypointNameStyle == i;
								menuItem.Click += new System.EventHandler(cm_wptNamingStyleMenuItem_Click);
								menuItemWptNaming.MenuItems.Add(menuItem);
							}
							mainPictureBox_ContextMenu.MenuItems.Add(menuItemWptNaming);

							menuItem = new MenuItem("-");
							menuItem.Index = index++;
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItemTools.Index = index++;
							menuItemTools.MenuItems.Clear();
							for (int i=0; i < Project.tools.tools.Count ;i++)
							{
								ToolDescr td = (ToolDescr)Project.tools.tools[i];
								menuItem = new MenuItem(td.ToString().Replace("&", "&&"));
								menuItem.Click += new System.EventHandler(cm_toolMenuItem_Click);
								menuItemTools.MenuItems.Add(menuItem);
							}
							mainPictureBox_ContextMenu.MenuItems.Add(menuItemTools);

							menuItem = new MenuItem("Street Map (in browser)");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_streetMapMenuItem_Click);
							if(!Project.serverAvailable)
							{
								// obviously disconnected, disable Internet-related function: 
								menuItem.Enabled = false;
							}
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("Nearest Geocaches (in browser)");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_nearestGeocachesMenuItem_Click);
							if(!Project.serverAvailable)
							{
								// obviously disconnected, disable Internet-related function: 
								menuItem.Enabled = false;
							}
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);

							menuItem = new MenuItem("Help (F1)");
							menuItem.Index = index++;
							menuItem.Click += new System.EventHandler(cm_helpMenuItem_Click);
							mainPictureBox_ContextMenu.MenuItems.Add(menuItem);
						}
					}
				}
			} 
			catch {}
		}

		private void mainPictureBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			try {
				btnDown = MouseButtons.None;
				m_cameraManager.pictureMouseUp(e, controlDown, shiftDown, altDown);
				m_pictureManager.pictureMouseUp(e, controlDown, shiftDown, altDown);
			} 
			catch {}
		}
		#endregion

		#region Context menu processing

		private void cm_browseMenuItem_Click(object sender, System.EventArgs e)
		{
			m_pictureManager.pictureClick(e, controlDown, shiftDown, false);
		}

		private void cm_editMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickWaypoint != null)
			{
				rightClickWaypoint.ForceShow = true;	// make sure label shows on the map, even if crowded
				new DlgEditWaypoint(m_cameraManager, rightClickWaypoint).ShowDialog();
			}
		}

		private void cm_moveMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickWaypoint != null)
			{
				Point screenPoint = this.picturePanel.PointToScreen(rightClickPoint);
				Project.ShowPopup(this.picturePanel, " \n\nHold Shift and left mouse button, drag mouse where you want new waypoint location to be.\n\n ", screenPoint);
			}
		}

		private void cm_deleteMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickWaypoint != null)
			{
				if(rightClickWaypoint.TrackId != -1)
				{
					Track trk = WaypointsCache.getTrackById(rightClickWaypoint.TrackId);
					if(trk != null)
					{
						trk.removeWaypoint(rightClickWaypoint.DateTime);		// wpt.DateTime is a key there
					}
				}
				else
				{
					WaypointsCache.WaypointsAll.Remove(rightClickWaypoint);
				}
				WaypointsCache.RefreshWaypointsDisplayed();
				WaypointsCache.isDirty = true;
				m_pictureManager.Refresh();
			}
		}

		private void cm_deleteVehicleMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickVehicle != null)
			{
				VehiclesCache.deleteVehicle(rightClickVehicle);
				m_pictureManager.Refresh();
			}
		}

		private void cm_detailMenuItem_Click(object sender, System.EventArgs e)
		{
			bool saved = Project.allowMapPopups;
			Project.allowMapPopups = true;
			m_pictureManager.pictureClick(e, controlDown, shiftDown, true);
			Project.allowMapPopups = saved;
		}

		private void cm_viewPhotoMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickWaypoint != null && rightClickWaypoint.ThumbImage != null)
			{
				Project.mainCommand.photoViewerUp(rightClickWaypoint);
			}
		}

		private void cm_viewPhotoDrawMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickWaypoint != null && rightClickWaypoint.ThumbImage != null)
			{
				Project.mainCommand.photoEditorUp(rightClickWaypoint);
			}
		}

		private void cm_popupsMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.allowMapPopups = !Project.allowMapPopups;
		}

		private void cm_colorCodeMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.trackElevColor = !Project.trackElevColor;
			if(Project.trackElevColor)
			{
				Project.trackSpeedColor = false;
			}
			m_pictureManager.Refresh();
		}

		private void cm_colorSpeedMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.trackSpeedColor = !Project.trackSpeedColor;
			if(Project.trackSpeedColor)
			{
				Project.trackElevColor = false;
			}
			m_pictureManager.Refresh();
		}

		private void cm_wptNamingStyleMenuItem_Click(object sender, System.EventArgs e)
		{
			int index = menuItemWptNaming.MenuItems.IndexOf((MenuItem)sender);
			Project.waypointNameStyle = index;
			m_cameraManager.ProcessCameraMove();
		}

		private void cm_endrouteMenuItem_Click(object sender, System.EventArgs e)
		{
			LayerWaypoints.This.makeRouteMode = false;
			LibSys.StatusBar.WriteLine("* finished making route");
		}

		private void cm_addWptToRouteMenuItem_Click(object sender, System.EventArgs e)
		{
			LayerWaypoints.This.addWptToRoute(rightClickWaypoint);
		}

		private void cm_removeWptFromRouteMenuItem_Click(object sender, System.EventArgs e)
		{
			LayerWaypoints.This.removeWptFromRoute(rightClickWaypoint);
		}

		private void cm_addEqToRouteMenuItem_Click(object sender, System.EventArgs e)
		{
			LayerWaypoints.This.addEqToRoute(rightClickEarthquake);
		}

		private long cm_trackId = -1;
		private bool warnedSanityFilter = false;

		private void cm_trackMenuItem_Click(object sender, System.EventArgs e)
		{
			trackProperties(cm_trackId);
		}

		private void cm_trackZoomMenuItem_Click(object sender, System.EventArgs e)
		{
			Track trk = WaypointsCache.getTrackById(cm_trackId);
			if(trk != null)
			{
				m_cameraManager.zoomToCorners(trk.TopLeft, trk.BottomRight);
			}
		}

		private void cm_trackHideMenuItem_Click(object sender, System.EventArgs e)
		{
			Track trk = WaypointsCache.getTrackById(cm_trackId);
			if(trk != null)
			{
				trk.Enabled = false;
				m_pictureManager.Refresh();
			}
		}

		private void cm_trackUngroupMenuItem_Click(object sender, System.EventArgs e)
		{
			Track trk = WaypointsCache.getTrackById(cm_trackId);
			if(trk != null)
			{
				Cursor.Current = Cursors.WaitCursor;
				//messageLabel.Text = "trk:" + trackId + " ungrouping...";

				// the Project.sanityFilter flag will be used in the following method:
				ArrayList trkIds = WaypointsCache.ungroupTrack(trk, Project.breakTimeMinutes);
				if(trkIds != null)
				{
					trk.Enabled = false;
					WaypointsCache.isDirty = true;

					m_cameraManager.ProcessCameraMove();

					Cursor.Current = Cursors.Default;
					if(!warnedSanityFilter && Project.sanityFilter)
					{
						warnedSanityFilter = true;
						string msg =  "Sanity Filter is activated and has been applied. Use Options tab in Waypoints Manager to turn it off.";
						//messageLabel.Text = msg;
						Project.MessageBox(this, msg);
					}
				}
				else
				{
					Cursor.Current = Cursors.Default;
					string msg =  "Sorry: selected track '" + trk.Name + "' is one trip - can't break it.\nUse Waypoints Manager to select other options.";
					Point popupOffset = new Point(10, 20);
					Point screenPoint = this.PointToScreen(rightClickPoint);
					Project.ShowPopup(null, msg, screenPoint);
				}
				Cursor.Current = Cursors.Default;
			}
		}

		private void cm_trackUngroupHereMenuItem_Click(object sender, System.EventArgs e)
		{
			Track trk = WaypointsCache.getTrackById(cm_trackId);
			if(rightClickWaypoint != null && trk != null)
			{
				Cursor.Current = Cursors.WaitCursor;
				//messageLabel.Text = "trk:" + trackId + " ungrouping...";

				ArrayList trkIds = WaypointsCache.ungroupTrackAtTrackpoint(trk, rightClickWaypoint);
				if(trkIds != null)
				{
					trk.Enabled = false;
					WaypointsCache.isDirty = true;

					m_cameraManager.ProcessCameraMove();

					Cursor.Current = Cursors.Default;
					if(!warnedSanityFilter && Project.sanityFilter)
					{
						warnedSanityFilter = true;
						string msg =  "Sanity Filter is activated and has been applied. Use Options tab in Waypoints Manager to turn it off.";
						//messageLabel.Text = msg;
						Project.MessageBox(this, msg);
					}
				}
				else
				{
					Cursor.Current = Cursors.Default;
					string msg =  "Sorry: cannot break selected track '" + trk.Name + "'\nUse Waypoints Manager to select other options.";
					Point popupOffset = new Point(10, 20);
					Point screenPoint = this.PointToScreen(rightClickPoint);
					Project.ShowPopup(null, msg, screenPoint);
				}
				Cursor.Current = Cursors.Default;
			}
		}

		private void cm_centerMenuItem_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.pictureDoubleClick(e, controlDown, shiftDown, altDown);
		}

		private void cm_newWaypointMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgMakeWaypoint dlg = new DlgMakeWaypoint(m_cameraManager, rightClickPoint);
			dlg.ShowDialog();
		}

		private void cm_newRouteMenuItem_Click(object sender, System.EventArgs e)
		{
			wptEnabled(true);
			Point screenPoint = this.picturePanel.PointToScreen(rightClickPoint);

			DlgNewRouteParameters dlgNewRouteParameters = new DlgNewRouteParameters();
			dlgNewRouteParameters.Location = screenPoint;
			dlgNewRouteParameters.setStartTime(Project.zuluToLocal(LayerWaypoints.This.RouteStartDateTime));
			dlgNewRouteParameters.ShowDialog();
			if(dlgNewRouteParameters.result)
			{
				Project.ShowPopup(this.picturePanel, " \n\nClick or Double-click on the map to make a route\n\nUse keyboard arrow keys to shift the map\n\nUse + - PageUp/Down to zoom in and out\n\nRight mouse click for altitude and speed\n\npress Escape key to finish\n\n ", screenPoint);
				Cursor.Current = Cursors.Cross;
				LibSys.StatusBar.WriteLine("* start making route");
				LayerWaypoints.This.makeRouteMode = true;
				GeoCoord loc = m_cameraManager.toGeoLocation(rightClickPoint);
				loc.Elev = DlgNewRouteParameters.altitude;
				Project.routeSpeed = DlgNewRouteParameters.speed;
				if(Project.routeSpeed > 0.0d)
				{
					LayerWaypoints.This.RouteStartDateTime = Project.localToZulu(new DateTime(7000, 1, 1));
				}
				if(DlgNewRouteParameters.startTime > DateTimePicker.MinDateTime)
				{
					LayerWaypoints.This.RouteStartDateTime = Project.localToZulu(DlgNewRouteParameters.startTime);
				}
				LayerWaypoints.This.addLocToRoute(loc);
			}
		}

		private void cm_trackSaveAsMenuItem_Click(object sender, System.EventArgs e)
		{
			Track trk = WaypointsCache.getTrackById(cm_trackId);
			if(trk != null)
			{
				m_cameraManager.zoomToCorners(trk.TopLeft, trk.BottomRight);

				ArrayList tracks = new ArrayList();		// array of Track
				tracks.Add(trk);

				FileExportForm fileExportForm = new FileExportForm(tracks, true, null, false);
				fileExportForm.ShowDialog();
			}
		}

		private void cm_pointGoogleEarthMenuItem_Click(object sender, System.EventArgs e)
		{
			GeoCoord clickCoord = m_cameraManager.toGeoLocation(rightClickPoint);
			GoogleEarthManager.runAtPoint(clickCoord, m_cameraManager.Location.Elev, "-", "QuakeMap click point");
			m_cameraManager.MarkLocation(clickCoord, 1);
		}

		private void cm_eqGoogleEarthMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickEarthquake != null)
			{
				ArrayList earthquakes = new ArrayList(1);
				earthquakes.Add(rightClickEarthquake);

				GoogleEarthManager.runEarthquakes(earthquakes);
			}
		}

		private void cm_wptGoogleEarthMenuItem_Click(object sender, System.EventArgs e)
		{
			if(rightClickWaypoint != null)
			{
				bool skipIt = false;

				if(Project.kmlOptions.ShowInPopup)
				{
					KmlOptionsPopup kmlOptionsPopup = new KmlOptionsPopup();

					kmlOptionsPopup.Top = this.mainPictureBox.PointToScreen(rightClickPoint).Y + 10;
					kmlOptionsPopup.Left = this.mainPictureBox.PointToScreen(rightClickPoint).X + 10;

					kmlOptionsPopup.ShowDialog(this);
					skipIt = !kmlOptionsPopup.doContinue;
				}

				ArrayList waypoints = new ArrayList(1);
				waypoints.Add(rightClickWaypoint);

				if(!skipIt)
				{
					GoogleEarthManager.runWaypoints(waypoints);
				}
			}
		}

		private void cm_trackGoogleEarthMenuItem_Click(object sender, System.EventArgs e)
		{
			Track trk = WaypointsCache.getTrackById(cm_trackId);
			if(trk != null)
			{
				m_cameraManager.zoomToCorners(trk.TopLeft, trk.BottomRight);
				
				bool skipIt = false;

				if(Project.kmlOptions.ShowInPopup)
				{
					KmlOptionsPopup kmlOptionsPopup = new KmlOptionsPopup();

					kmlOptionsPopup.Top = this.mainPictureBox.PointToScreen(rightClickPoint).Y + 10;
					kmlOptionsPopup.Left = this.mainPictureBox.PointToScreen(rightClickPoint).X + 10;

					kmlOptionsPopup.ShowDialog(this);
					skipIt = !kmlOptionsPopup.doContinue;
				}

				if(!skipIt)
				{
					GoogleEarthManager.runTrack(trk);
				}
			}
		}

		private void cm_streetMapMenuItem_Click(object sender, System.EventArgs e)
		{
			GeoCoord clickCoord = m_cameraManager.toGeoLocation(rightClickPoint);
			Project.streetMap(clickCoord.Lng, clickCoord.Lat, m_cameraManager.Location.Elev);
			m_cameraManager.MarkLocation(clickCoord, 1);
		}

		private void cm_nearestGeocachesMenuItem_Click(object sender, System.EventArgs e)
		{
			GeoCoord clickCoord = m_cameraManager.toGeoLocation(rightClickPoint);
			Project.nearestGeocaches(clickCoord.Lng, clickCoord.Lat);
		}

		private void cm_zoomInMenuItem_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.zoomIn();
		}

		private void cm_zoomOutMenuItem_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.zoomOut();
		}

		private void cm_toolMenuItem_Click(object sender, System.EventArgs e)
		{
			int index = menuItemTools.MenuItems.IndexOf((MenuItem)sender);
			ToolDescr td = (ToolDescr)Project.tools.tools[index];
			//LibSys.StatusBar.Trace("toolsItemClick: item=" + menuItem.Index + "  tool='" + td + "'");
			try 
			{
				GeoCoord clickCoord = m_cameraManager.toGeoLocation(rightClickPoint);
				
				string wptName = null;
				string wptGuid = null;

				if(rightClickWaypoint != null)
				{
					wptName = rightClickWaypoint.WptName;
					wptGuid = rightClickWaypoint.WptGuid;
					clickCoord = new GeoCoord(rightClickWaypoint.Location);
				}
				
				m_cameraManager.MarkLocation(clickCoord, 1);
				td.run(clickCoord.Lat, clickCoord.Lng, wptName, wptGuid);
			} 
			catch
			{
				LibSys.StatusBar.Trace("* Error: tool not valid: " + td);
			}
		}

		private void cm_helpMenuItem_Click(object sender, System.EventArgs e)
		{
			showHelp();
		}

		private void cm_zoomToDragRectangleMenuItem_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.zoomToDragRectangle();
		}

		private void cm_areaGoogleEarthMenuItem_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.googleEarthShowDragRectangle();
		}

		private void cm_preloadMenuItem_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.zoomToDragRectangle();
			new DlgPreloadTiles(pdaSuperframeMenuItem.Enabled).ShowDialog();
		}

		private void cm_preloadAlongRouteMenuItem_Click(object sender, System.EventArgs e)
		{
			Track trk = WaypointsCache.getTrackById(cm_trackId);

			if(trk != null)
			{
				DlgPreloadTilesAlongRoute dlg = new DlgPreloadTilesAlongRoute(trk);
				dlg.ShowDialog();
			}
		}
		#endregion

		#region Menu and button clicks and other action handlers

		private void MainForm_Click(object sender, System.EventArgs e)
		{
			mainPictureBox.Focus();
		}

		private void splitter1_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			//LibSys.StatusBar.Trace2("=" + e.X);
		}

		private void splitter1_SplitterMoving(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			//LibSys.StatusBar.Trace2("..." + e.X);
		}

		private void findLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(TileCache.ZipcodeServer != null) 
			{
				FindForm findForm = new FindForm(m_cameraManager, false);
				findForm.ShowDialog();
			} 
			else 
			{
				Point screenPoint = findLinkLabel.PointToScreen(popupOffset);
				Project.ShowPopup(null, "Cannot use Find - need Internet connection", screenPoint);
				findLinkLabel.Enabled = false;
			}
		}

		private bool hasShownFind = false;
		private void findLinkLabel_MouseHover(object sender, System.EventArgs e)
		{
			if(!hasShownFind)
			{
				Point screenPoint = findLinkLabel.PointToScreen(popupOffset);
				Project.ShowPopup(null, "Find a US city by zipcode or name", screenPoint);
				hasShownFind = true;
			}
		}

		private void reliefMenuItem_Click(object sender, System.EventArgs e)
		{
			reliefMenuItem.Checked = !reliefMenuItem.Checked;
			reliefCheckBox.Checked = reliefMenuItem.Checked;
			reliefCheckBox_CheckedChanged(sender, e);
		}

		private void reliefCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.drawRelief = reliefCheckBox.Checked;
			gridCheckBox.Enabled = Project.drawRelief || Project.drawTerraserver;
			citiesCheckBox.Enabled = Project.drawRelief;

			if(m_pictureManager != null) 
			{
				if(!Project.drawRelief)
				{
					Project.terraLayerOpacity = 1.0d;
				}
				else
				{
					opacityPopup.setOpacityByScrollBar();	// scroll bar remembers the last value
				}
				//m_cameraManager.SpoilPicture();	// can't do it without ProcessCameraMove()
				m_pictureManager.LayersManager.ShowBasicMap = reliefCheckBox.Checked;
				m_pictureManager.Refresh();
			}
		}

		private void gridCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.drawGrid = gridCheckBox.Checked;
			if(m_pictureManager != null) 
			{
				m_pictureManager.Refresh();
			}
		}

		private void citiesMenuItem_Click(object sender, System.EventArgs e)
		{
			if(Project.drawRelief)
			{
				citiesMenuItem.Checked = !citiesMenuItem.Checked;
				citiesCheckBox.Checked = citiesMenuItem.Checked;
				citiesCheckBox_CheckedChanged(this, null);
			}
		}

		private void citiesCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.drawCities = citiesCheckBox.Checked;
			if(m_pictureManager != null) 
			{
				m_pictureManager.Refresh();
			}
		}

		private void aerialMenuItem_Click(object sender, System.EventArgs e)
		{
			aerialMenuItem.Checked = !aerialMenuItem.Checked;
			aerialCheckBox.Checked = aerialMenuItem.Checked;
			// fake mouse event on aerialCheckBox:
			aerialCheckBox_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
		}

		// MouseUp is used because CheckedChanged is called when Checked=... is issued programmatically
		private void aerialCheckBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(sender == aerialCheckBox)
			{
				if(aerialCheckBox.Checked)
				{
					Project.drawTerraserverMode = "aerial";
					colorAerialCheckBox.Checked = false;
					overlayCheckBox.Enabled = true;
				}
			}
			else
			{
				if(colorAerialCheckBox.Checked)
				{
					Project.drawTerraserverMode = "color aerial";
					aerialCheckBox.Checked = false;
					overlayCheckBox.Enabled = true;
				}
			}
			topoCheckBox.Checked = false;	// will call CheckedChanged, but not MouseUp
			Project.drawTerraserver = aerialCheckBox.Checked || colorAerialCheckBox.Checked || topoCheckBox.Checked;
			//opacityPanel.Visible = Project.drawTerraserver;
			lmCheckBox.Enabled = Project.drawTerraserver;
			if(m_pictureManager != null && m_cameraManager != null) 
			{
				m_cameraManager.SpoilPicture();
				Project.terraserverAvailable = false;
				m_pictureManager.LayersManager.ShowTerraserver = Project.drawTerraserver;	// calls CameraMoved and ReTile
				//m_pictureManager.Refresh();
				m_cameraManager.ProcessCameraMove();
			}
		}

		private void topoMenuItem_Click(object sender, System.EventArgs e)
		{
			topoMenuItem.Checked = !topoMenuItem.Checked;
			topoCheckBox.Checked = topoMenuItem.Checked;
			// fake mouse event on topoCheckBox:
			topoCheckBox_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
		}

		// MouseUp is used because CheckedChanged is called when Checked=... is issued programmatically
		private void topoCheckBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(topoCheckBox.Checked)
			{
				Project.drawTerraserverMode = "topo";
				overlayCheckBox.Enabled = false;
			}
			aerialCheckBox.Checked = false;	// will call CheckedChanged, but not MouseUp
			colorAerialCheckBox.Checked = false;	// will call CheckedChanged, but not MouseUp
			Project.drawTerraserver = aerialCheckBox.Checked || colorAerialCheckBox.Checked || topoCheckBox.Checked;
			//opacityPanel.Visible = Project.drawTerraserver;
			lmCheckBox.Enabled = Project.drawTerraserver;
			if(m_pictureManager != null && m_cameraManager != null) 
			{
				m_cameraManager.SpoilPicture();
				Project.terraserverAvailable = false;
				m_pictureManager.LayersManager.ShowTerraserver = Project.drawTerraserver;	// calls CameraMoved and ReTile
				//m_pictureManager.Refresh();
				m_cameraManager.ProcessCameraMove();
			}
		}

		private void overlayCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.terraUseOverlay = overlayCheckBox.Checked;

			if(m_cameraManager != null)
			{
				m_cameraManager.ProcessCameraMove();
			}
		}

		private void lmMenuItem_Click(object sender, System.EventArgs e)
		{
			lmMenuItem.Checked = !lmMenuItem.Checked;
			lmCheckBox.Checked = lmMenuItem.Checked;
			// fake event on lmCheckBox:
			lmCheckBox_CheckedChanged(null, EventArgs.Empty);
		}

		private void lmCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(m_pictureManager != null) 
			{
				//m_pictureManager.LayersManager.ShowTerraserver = lmCheckBox.Checked;
				Project.drawLandmarks = lmCheckBox.Checked;
				if(Project.drawLandmarks)
				{
					if(TerraserverCache.ls == null)
					{
						// need to initialize the service and also bring up selection dialog
						lmButton_Click(null, EventArgs.Empty);
					}
					else
					{
						m_cameraManager.ProcessCameraMove();	// make sure that terraserver is reached for the current frame for Landmarks
					}
				}
				else
				{
					m_pictureManager.Refresh();			// just redraw the screen
				}
			}
		}

		private void eqMenuItem_Click(object sender, System.EventArgs e)
		{
			eqMenuItem.Checked = !eqMenuItem.Checked;
			eqCheckBox.Checked = eqMenuItem.Checked;
			// fake event on eqCheckBox:
			eqCheckBox_CheckedChanged(null, EventArgs.Empty);
		}

		private void eqCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(m_pictureManager != null) 
			{
				m_pictureManager.LayersManager.ShowEarthquakes = eqCheckBox.Checked;
				Project.drawEarthquakes = eqCheckBox.Checked;
				eqStylePanelVisible(Project.drawEarthquakes);
				if(Project.drawEarthquakes && EarthquakesCache.EarthquakesAll.Count == 0)
				{
					EarthquakesCache.goFetch = true;	// provoke a single fetch
				}
				else
				{
					m_pictureManager.Refresh();
				}
			}
		}

		private void wptMenuItem_Click(object sender, System.EventArgs e)
		{
			wptMenuItem.Checked = !wptMenuItem.Checked;
			wptCheckBox.Checked = wptMenuItem.Checked;
			// fake event on wptCheckBox:
			wptCheckBox_CheckedChanged(null, EventArgs.Empty);
		}

		private void wptCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(m_pictureManager != null) 
			{
				m_pictureManager.LayersManager.ShowWaypoints = wptCheckBox.Checked;
				Project.drawWaypoints = wptCheckBox.Checked;
				m_pictureManager.Refresh();
			}
		}

		private void colorCodeMenuItem_Click(object sender, System.EventArgs e)
		{
			if(m_pictureManager != null) 
			{
				m_pictureManager.LayersManager.ShowWaypoints = true;
				Project.trackElevColor = !colorCodeMenuItem.Checked;
				if(Project.trackElevColor)
				{
					Project.trackSpeedColor = false;
				}
				m_pictureManager.Refresh();
			}
		}

		private void colorSpeedMenuItem_Click(object sender, System.EventArgs e)
		{
			if(m_pictureManager != null) 
			{
				m_pictureManager.LayersManager.ShowWaypoints = true;
				Project.trackSpeedColor = !colorSpeedMenuItem.Checked;
				if(Project.trackSpeedColor)
				{
					Project.trackElevColor = false;
				}
				m_pictureManager.Refresh();
			}
		}

		private void thumbMenuItem_Click(object sender, System.EventArgs e)
		{
			thumbMenuItem.Checked = !thumbMenuItem.Checked;
			if(m_pictureManager != null) 
			{
				Project.thumbDoDisplay = thumbMenuItem.Checked;
				m_pictureManager.Refresh();
			}
		}

		private void thumb2MenuItem_Click(object sender, System.EventArgs e)
		{
			thumb2MenuItem.Checked = !thumb2MenuItem.Checked;
			if(m_pictureManager != null) 
			{
				Project.thumbDoDisplay = thumb2MenuItem.Checked;
				m_pictureManager.Refresh();
			}
		}

		private void vehMenuItem_Click(object sender, System.EventArgs e)
		{
			vehMenuItem.Checked = !vehMenuItem.Checked;
			vehCheckBox.Checked = vehMenuItem.Checked;
			// fake event on vehCheckBox:
			vehCheckBox_CheckedChanged(null, EventArgs.Empty);
		}

		private void vehCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(m_pictureManager != null) 
			{
				m_pictureManager.LayersManager.ShowVehicles = vehCheckBox.Checked;
				Project.drawVehicles = vehCheckBox.Checked;
				m_pictureManager.Refresh();
			}
		}

		private void registrationMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgZipcode dlg = new DlgZipcode();
			dlg.ShowDialog();
		}

		private void registrationMenuItem2_Click(object sender, System.EventArgs e)
		{
			DlgZipcode dlg = new DlgZipcode();
			dlg.ShowDialog();
		}

		private void importMenuItem_Click(object sender, System.EventArgs e)
		{
			FileImportForm fileImportForm = new FileImportForm(false, FileImportForm.MODE_GPS, FileImportForm.TYPE_ANY);
			fileImportForm.ShowDialog();
		}

		private void importsListMenuItem_Click(object sender, System.EventArgs e)
		{
			FileImportForm fileImportForm = new FileImportForm(true, FileImportForm.MODE_GPS, FileImportForm.TYPE_ANY);
			fileImportForm.ShowDialog();
		}

		private void clearImportsListMenuItem_Click(object sender, System.EventArgs e)
		{
			string sWarn = "Clearing list of files currently loaded in QuakeMap (and all associated waypoints, tracks...).\nFiles will NOT be deleted.\nPersistent files entries will be also cleared from the list.";
			if(Project.YesNoBox(this, sWarn))
			{
				FileImportForm.clearImportsList();
				PhotoWaypoints.cleanPhotoTrackpoints(null);
				PhotoWaypoints.cleanUnrelated(null);
			}
		}

		private void saveTrackMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgWaypointsManager(m_pictureManager, m_cameraManager, 1, -1).Show();
			wptCheckBox.Checked = Project.drawWaypoints;
		}

		private void fileOpenMenuItem_Click(object sender, System.EventArgs e)
		{
			FileAndZipIO.QuickOpen();
		}

		private void helpMenuItem_Click(object sender, System.EventArgs e)
		{
			showHelp();
		}

		private void optionsMenuItem_Click(object sender, System.EventArgs e)
		{
			OptionsForm optionsForm = new OptionsForm(m_pictureManager, 0);
			optionsForm.ShowDialog();
		}

		private void aboutMenuItem_Click(object sender, System.EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.ShowDialog();
		}

		private void radioButton1_Click(object sender, System.EventArgs e)
		{
			Project.earthquakeStyle = Earthquake.STYLE_DOT;
			m_cameraManager.ProcessCameraMove();	// we need more than Refresh, label layout is needed.
		}

		private void radioButton2_Click(object sender, System.EventArgs e)
		{
			Project.earthquakeStyle = Earthquake.STYLE_CIRCLES;
			m_cameraManager.ProcessCameraMove();
		}

		private void radioButton3_Click(object sender, System.EventArgs e)
		{
			Project.earthquakeStyle = Earthquake.STYLE_FILLCIRCLES;
			m_cameraManager.ProcessCameraMove();
			new DlgDepthCc().Show();
		}

		private void radioButton4_Click(object sender, System.EventArgs e)
		{
			Project.earthquakeStyle = Earthquake.STYLE_CONCENTRICCIRCLES;
			m_cameraManager.ProcessCameraMove();
		}

		private void radioButton5_Click(object sender, System.EventArgs e)
		{
			Project.earthquakeStyle = Earthquake.STYLE_SQUARES;
			m_cameraManager.ProcessCameraMove();
		}

		private void radioButton6_Click(object sender, System.EventArgs e)
		{
			Project.earthquakeStyle = Earthquake.STYLE_FILLSQUARES;
			m_cameraManager.ProcessCameraMove();
			new DlgDepthCc().Show();
		}

		private void radioButton7_Click(object sender, System.EventArgs e)
		{
			Project.earthquakeStyle = Earthquake.STYLE_TRIANGLE;
			m_cameraManager.ProcessCameraMove();
		}

		private const string eqStylePrefix = "Earthquake Style: ";
		private static Point popupOffset = new Point(10, 10);

		private void radioButton1_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = radioButton1.PointToScreen(popupOffset);
			Project.ShowPopup(null, eqStylePrefix + "dot", screenPoint);
		}

		private void radioButton2_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = radioButton2.PointToScreen(popupOffset);
			Project.ShowPopup(null, eqStylePrefix + "circle", screenPoint);
		}

		private void radioButton3_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = radioButton3.PointToScreen(popupOffset);
			Project.ShowPopup(null, eqStylePrefix + "\ncolor coded circle", screenPoint);
		}

		private void radioButton4_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = radioButton4.PointToScreen(popupOffset);
			Project.ShowPopup(null, eqStylePrefix + "\nconcentric circles", screenPoint);
		}

		private void radioButton5_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = radioButton5.PointToScreen(popupOffset);
			Project.ShowPopup(null, eqStylePrefix + "square", screenPoint);
		}

		private void radioButton6_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = radioButton6.PointToScreen(popupOffset);
			Project.ShowPopup(null, eqStylePrefix + "\ncolor coded square", screenPoint);
		}

		private void radioButton7_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = radioButton7.PointToScreen(popupOffset);
			Project.ShowPopup(null, eqStylePrefix + "triangle", screenPoint);
		}

		private void gotoButton_Click(object sender, System.EventArgs e)
		{
			gotoLocation();
		}

		private void gotoMenuItem_Click(object sender, System.EventArgs e)
		{
			gotoLocation();
		}

		private void distanceMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.MessageBox(this, "To measure distance, hold Ctrl and click mouse on start point,\nthen move the mouse to end point while holding left mouse button"); 		
		}

		private void newWaypointMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgMakeWaypoint dlg = new DlgMakeWaypoint(m_cameraManager, m_cameraManager.toPixelLocation(m_cameraManager.Location, null));
			dlg.ShowDialog();
		}

		private void newRouteMenuItem_Click(object sender, System.EventArgs e)
		{
			wptEnabled(true);
			Project.ShowPopup(this.picturePanel, " \n\nClick or Double-click on the map to make a route\n\nUse keyboard arrow keys to shift the map\n\nUse + - PageUp/Down to zoom in and out\n\npress Escape key to finish\n\n ", Point.Empty);
			Cursor.Current = Cursors.Cross;
			LibSys.StatusBar.WriteLine("* start making route");
			LayerWaypoints.This.makeRouteMode = true;
		}

		private void toolsItemClick(object sender, System.EventArgs e)
		{
			MenuItem menuItem = (MenuItem)sender;
			int index = menuItem.Index - Project.toolsFirstIndex - 1;
			ToolDescr td = (ToolDescr)Project.tools.tools[index];
			//LibSys.StatusBar.Trace("toolsItemClick: item=" + menuItem.Index + "  tool='" + td + "'");
			try 
			{
				td.run(m_cameraManager.Location.Lat, m_cameraManager.Location.Lng, null, null);
			} 
			catch
			{
				LibSys.StatusBar.Trace("* Error: tool not valid: " + td);
			}
		}

		private void eqButton_Click(object sender, System.EventArgs e)
		{
			eqCheckBox.Checked = true;
			//eqCheckBox_CheckedChanged(null, EventArgs.Empty);
			if(EarthquakesCache.isFetching || EarthquakesCache.EarthquakesAll.Count == 0)
			{
				Project.MessageBox(this, "The program is still fetching data from earthquake servers.\n\nData in table and graph may be incomplete");
				ProgressMonitorForm.BringFormUp(false);
			}
			DlgEarthquakesManager form = new DlgEarthquakesManager(m_pictureManager, m_cameraManager, 0, -1);
			form.ShowDialog();
		}

		private bool warnedWsOff = false;
		
		private void warnWsOff()
		{
			if(Project.serverAvailable && Project.drawLandmarks && !Project.terraserverUseServices && !warnedWsOff)
			{
				Project.MessageBox(this, "Warning: \"Internet-->Use Web Services\" menu option is turned off.\nOnly previously cached landmarks will be shown.\nTo see landmarks you need to enable web services.");
				warnedWsOff = true;
				m_pictureManager.Refresh();
			}

		}

		private void lmButton_Click(object sender, System.EventArgs e)
		{
			initLandmarkService();

			warnWsOff();

			if(TerraserverCache.landmarkPointShow != null && TerraserverCache.landmarkPointShow.Length > 0)
			{
				lmCheckBox.Checked = true;
				//lmCheckBox_CheckedChanged(null, EventArgs.Empty);
				OptionsLandmarksForm optionsLandmarksForm = new OptionsLandmarksForm(m_pictureManager, sender==null);
				optionsLandmarksForm.ShowDialog();
			}
			else
			{
				lmCheckBox.Checked = false;
				Project.ShowPopup(lmButton, "Sorry, Landmarks Server was not reached", Point.Empty);
			}
			m_pictureManager.Refresh();
		}

		private void wptButton_Click(object sender, System.EventArgs e)
		{
			wptCheckBox.Checked = true;
			wptCheckBox_CheckedChanged(null, EventArgs.Empty);
			new DlgWaypointsManager(m_pictureManager, m_cameraManager, 0, -1).Show();
		}

		private void MainForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			if(m_greetingForm != null)
			{
				// first paint makes sure that the greeting form is closed:
				HideGreeting();
			}
		}

		private void openGpsManager(string cmd)
		{
			try
			{
				if(DlgGpsManager.isBusy)
				{
					Project.ShowPopup(this.cameraHeightLabel, "GPS Manager is still active and communicating to GPS", Point.Empty);
					DlgGpsManager.BringFormUp();
				}
				else
				{
					if(DlgGpsManager.isUp)
					{
						DlgGpsManager.CloseGently();
					}
					new DlgGpsManager(new GpsInsertWaypoint(WaypointsCache.insertWaypoint), m_cameraManager, m_pictureManager, null, null, cmd).Show();
				}
			}
			catch {}
		}

		private void gpsManagerMenuItem_Click(object sender, System.EventArgs e)
		{
			openGpsManager(null);
			wptCheckBox.Checked = Project.drawWaypoints;
		}

		private void gpsCurrentMenuItem_Click(object sender, System.EventArgs e)
		{
			openGpsManager("startRealtime");
			wptCheckBox.Checked = Project.drawWaypoints;
		}

		private void getTrackLogMenuItem_Click(object sender, System.EventArgs e)
		{
			openGpsManager("uploadTracks");
			wptCheckBox.Checked = Project.drawWaypoints;
		}

		private void trackManagerMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgWaypointsManager(m_pictureManager, m_cameraManager, 0, -1).Show();
			wptCheckBox.Checked = Project.drawWaypoints;
		}

		private void exportMenuItem_Click(object sender, System.EventArgs e)
		{
			FileExportForm fileExportForm = new FileExportForm(null, true, null, true);	// null means all tracks/wpts from cache
			fileExportForm.ShowDialog();
		}

		private void eqLocalMenuItem_Click(object sender, System.EventArgs e)
		{
			eqCheckBox.Checked = true;
			// fake event on eqCheckBox:
			eqCheckBox_CheckedChanged(null, EventArgs.Empty);

			DlgEarthquakesManager form = new DlgEarthquakesManager(m_pictureManager, m_cameraManager, 4, 1);
			form.ShowDialog();
		}

		private void eqWorldMenuItem_Click(object sender, System.EventArgs e)
		{
			eqCheckBox.Checked = true;
			// fake event on eqCheckBox:
			eqCheckBox_CheckedChanged(null, EventArgs.Empty);

			DlgEarthquakesManager form = new DlgEarthquakesManager(m_pictureManager, m_cameraManager, 4, 0);
			form.ShowDialog();
		}

		private void eqFilterMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgEarthquakesManager form = new DlgEarthquakesManager(m_pictureManager, m_cameraManager, 3, 0);
			form.ShowDialog();
		}

		private void backMenuItem_Click(object sender, System.EventArgs e)
		{
			doBack();
		}


		private void backLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			doBack();
		}

		private bool hasShownBack = false;
		private void backLinkLabel_MouseHover(object sender, System.EventArgs e)
		{
			if(!hasShownBack)
			{
				Point screenPoint = backLinkLabel.PointToScreen(popupOffset);
				Project.ShowPopup(null, "Back to previous Camera location", screenPoint);
				hasShownBack = true;
			}
		}

		private void forwardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			doForward();
		}

		private bool hasShownForward = false;
		private void forwardLinkLabel_MouseHover(object sender, System.EventArgs e)
		{
			if(!hasShownForward)
			{
				Point screenPoint = forwardLinkLabel.PointToScreen(popupOffset);
				Project.ShowPopup(null, "Forward to next Camera location", screenPoint);
				hasShownForward = true;
			}
		}

		private void backButton_Click(object sender, System.EventArgs e)
		{
			doBack();
		}

		private void forwardMenuItem_Click(object sender, System.EventArgs e)
		{
			doForward();
		}

		private void pdaMainMenuItem_Popup(object sender, System.EventArgs e)
		{
			startCamTrackMenuItem.Enabled = !Project.camTrackOn;
			pdaSuperframeMenuItem.Enabled = Project.camTrackOn;
			stopCamTrackMenuItem.Enabled = Project.camTrackOn;
			pdaCancelMenuItem.Enabled = Project.camTrackOn;
		}

		private void startCamTrackMenuItem_Click(object sender, System.EventArgs e)
		{
			if(Project.terraUseOverlay)
			{
				this.overlayCheckBox.Checked = false;
			}

			m_cameraTrack.init();	// makes/cleans folder

			Project.camTrackOn = true;

			LibSys.StatusBar.Trace("* camera tracking activated");

			m_cameraManager.ProcessCameraMove();

			camtrackOn(true);
		}

		private void pdaSuperframeMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPreloadTiles(true).ShowDialog();
		}

		private void stopCamTrackMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.camTrackOn = false;

			camtrackOn(false);

			(new DlgPdaExport(m_cameraTrack)).ShowDialog();
		}

		private void pdaCancelMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.camTrackOn = false;

			camtrackOn(false);
		}

		private void progressButton_Click(object sender, System.EventArgs e)
		{
			ProgressMonitorForm.BringFormUp(false);
		}

		private void streetMapMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.streetMap(m_cameraManager.Location.Lng, m_cameraManager.Location.Lat, m_cameraManager.Location.Elev);
			m_cameraManager.MarkLocation(m_cameraManager.Location, 1);
		}

		private void nearestGeocachesMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.nearestGeocaches(m_cameraManager.Location.Lng, m_cameraManager.Location.Lat);
		}


		private void photoMainMenuItem_Popup(object sender, System.EventArgs e)
		{
			photoAnalyzeGpxMenuItem.Checked = Project.photoAnalyzeOnLoad;
			thumb2MenuItem.Checked = Project.thumbDoDisplay;
			viewImageMenuItem.Enabled = PhotoWaypoints.hasCurrentWaypoint();
			listPhotosMenuItem.Enabled = PhotoWaypoints.WaypointsWithThumbs.Count > 0;
			unrelatedPhotosMenuItem.Enabled = PhotoWaypoints.PhotosUnrelated.Count > 0;
		}

		private void photoAnalyzeGpxMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.photoAnalyzeOnLoad = !Project.photoAnalyzeOnLoad;
		}

		private void menuItem8_Popup(object sender, System.EventArgs e)
		{
			reliefMenuItem.Checked = Project.drawRelief;
			aerialMenuItem.Checked = Project.drawTerraserver && Project.drawTerraserverMode.Equals("aerial");
			topoMenuItem.Checked = Project.drawTerraserver && Project.drawTerraserverMode.Equals("topo");
			citiesMenuItem.Enabled = Project.drawRelief;
			citiesMenuItem.Checked = Project.drawCities;
			eqMenuItem.Checked = Project.drawEarthquakes;
			lmMenuItem.Enabled = Project.drawTerraserver;
			lmMenuItem.Checked = Project.drawLandmarks;
			vehMenuItem.Checked = Project.drawVehicles;
			wptMenuItem.Checked = Project.drawWaypoints;
			thumbMenuItem.Checked = Project.thumbDoDisplay;
			colorCodeMenuItem.Checked = Project.trackElevColor;
			colorSpeedMenuItem.Checked = Project.trackSpeedColor;
			backMenuItem.Enabled = (Project.cameraPositionsBackStack.Count > 1);
			forwardMenuItem.Enabled = (Project.cameraPositionsFwdStack.Count > 0);
		}

		private void fetchEqMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.drawEarthquakes = true;
			eqCheckBox.Checked = true;
			m_pictureManager.LayersManager.ShowEarthquakes = eqCheckBox.Checked;

			EarthquakesCache.goFetch = true;	// triggers a single fetch, is reset after it

			ProgressMonitorForm.BringFormUp(true);
		}

		private void fileExitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void printMenuItem_Click(object sender, System.EventArgs e)
		{
			try
			{
				PrintDialog pd = new PrintDialog();
				pd.Document = mainPrintDocument;
				if (pd.ShowDialog() == DialogResult.OK) 
				{
					mainPrintDocument.Print();
				}
			}
			catch
			{
				Project.ErrorPrinterNotInstalled(this);
			}
		}

		private void printPreviewMenuItem_Click(object sender, System.EventArgs e)
		{
			try 
			{
				PrintPreviewDialog ppd = new PrintPreviewDialog();
				ppd.Document = mainPrintDocument;
				ppd.ShowDialog();
			}
			catch
			{
				Project.ErrorPrinterNotInstalled(this);
			}
		}

		private PageSettings m_pgSettings = new PageSettings();

		private void pageSetupMenuItem_Click(object sender, System.EventArgs e)
		{
			try
			{
				PageSetupDialog psd = new PageSetupDialog();
				psd.Document = mainPrintDocument;
				psd.PageSettings = m_pgSettings;
				psd.AllowOrientation = true;
				psd.AllowMargins = true;

				psd.ShowDialog();
			}
			catch
			{
				Project.ErrorPrinterNotInstalled(this);
			}
		}

		private void eqClearMenuItem_Click(object sender, System.EventArgs e)
		{
			EarthquakesCache.EarthquakesAll.Clear();
			EarthquakesCache.EarthquakesDisplayed.Clear();
			m_pictureManager.Refresh();
		}

		private void eqHistoricalMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgEarthquakesHistorical dlg = new DlgEarthquakesHistorical(m_pictureManager);
			dlg.ShowDialog();
		}

		private void eqOptionsMenuItem_Click(object sender, System.EventArgs e)
		{
			OptionsForm optionsForm = new OptionsForm(m_pictureManager, 2);
			optionsForm.ShowDialog();
		}

		private void geocachingRecentEmailMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgWeeklyCacheImport dlg = new DlgWeeklyCacheImport(m_pictureManager);
			dlg.ShowDialog();
		}

		private void refreshMenuItem_Click(object sender, System.EventArgs e)
		{
			LibSys.StatusBar.WriteLine("* Reloading and refreshing the picture...");
			m_cameraManager.ReloadAndRefresh();		// will end up in LayersManager:t_cameraMoved() in a separate thread
		}

		private void optionsMapMenuItem_Click(object sender, System.EventArgs e)
		{
			OptionsForm optionsForm = new OptionsForm(m_pictureManager, 1);
			optionsForm.ShowDialog();
		}

		private void updateMenuItem_Click(object sender, System.EventArgs e)
		{
			if(Project.upgradeMessage.Length > 0)
			{
				new DlgUpgradeMessage(Project.upgradeMessage).ShowDialog();
			}
			else
			{
				new DlgUpdate().ShowDialog();
			}
		}

		private void lmButton_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = lmButton.PointToScreen(new Point(0, 0));
			Project.ShowPopup(lmButton, "Landmarks", screenPoint);
		}

		private void eqButton_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = eqButton.PointToScreen(new Point(0, 0));
			Project.ShowPopup(eqButton, "Earthquakes", screenPoint);
		}

		private void wptButton_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = wptButton.PointToScreen(new Point(0, 0));
			Project.ShowPopup(wptButton, "Waypoints, Tracks and Geocaches", screenPoint);
		}

		private void camtrackOnButton_Click(object sender, System.EventArgs e)
		{
			stopCamTrackMenuItem_Click(sender, e);
		}

		private void camtrackOnButton_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = camtrackOnButton.PointToScreen(new Point(0, 0));
			Project.ShowPopup(camtrackOnButton, "Camera Tracking is ON", screenPoint);
		}

		private void eqFilterOnButton_Click(object sender, System.EventArgs e)
		{
			eqCheckBox.Checked = true;
			DlgEarthquakesManager form = new DlgEarthquakesManager(m_pictureManager, m_cameraManager, 3, -1);
			form.ShowDialog();
		}

		private void eqFilterOnButton_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = eqFilterOnButton.PointToScreen(new Point(0, 0));
			Project.ShowPopup(eqFilterOnButton, "Earthquakes Filter is ON", screenPoint);
		}

		private void onlineMenuItem_Click(object sender, System.EventArgs e)
		{
			trySetInternetAvailable(!Project.serverAvailable);
		}

		// IMainCommand implementation:
		public void trySetInternetAvailable(bool onoff)
		{
			this.setInternetAvailable(onoff);
			string reason = "";
			
			if(Project.serverAvailable)		// && TileCache.RegistrationServer == null)
			{
				TileCache.init(Project.CGIBINPTR_URL);		// try to reach the QuakeMap server
				if(TileCache.ZipcodeServer == null)
				{
					reason = " - Could not reach server";
					this.setInternetAvailable(false);
				}
			}
			if(Project.serverAvailable)
			{
				TileCache.Clear();						// empty tiles may be reloaded now
				TerraserverCache.Clear();				// empty tiles may be reloaded now
				m_cameraManager.ProcessCameraMove();	// make sure the picture is refreshed in new on-line mode
			}
			Point popupOffset = new Point(30, 30);
			Point screenPoint = this.PointToScreen(popupOffset);
			Project.ShowPopup (this, (Project.serverAvailable ? "Working ON-line" : "Working OFF-line") + reason, screenPoint);
		}

		private void internetMainMenuItem_Popup(object sender, System.EventArgs e)
		{
			onlineMenuItem.Text = Project.serverAvailable ? "Online [click to disconnect]" : "Offline [click to connect]";
			onlineMenuItem.Checked = Project.serverAvailable;
			useServicesMenuItem.Text = Project.terraserverUseServices ? "Using Web Services [click to compute locally]" : "Not using Web Services [click to use]";
			useServicesMenuItem.Checked = Project.terraserverUseServices;
		}

		private void useServicesMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.terraserverUseServices = !Project.terraserverUseServices;
			warnWsOff();
			m_cameraManager.ProcessCameraMove();
		}

		private void scale1mButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 1000.0d;	// calls ProcessCameraMove();
		}

		private void scale2mButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 2000.0d;	// calls ProcessCameraMove();
		}

		private void scale4mButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 4000.0d;	// calls ProcessCameraMove();
		}

		private void scale8mButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 8000.0d;	// calls ProcessCameraMove();
		}

		private void scale16mButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 16000.0d;	// calls ProcessCameraMove();
		}

		private void scale32mButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 32000.0d;	// calls ProcessCameraMove();
		}

		private void scale64mButton_Click(object sender, System.EventArgs e)
		{
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 64000.0d;	// calls ProcessCameraMove();
		}

		private void scaleSafeButton_Click(object sender, System.EventArgs e)
		{
			// x-scale is 370 to 2000 km
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 400000.0d;	// calls ProcessCameraMove();
		}

		private void scaleYButton_Click(object sender, System.EventArgs e)
		{
			// y-scale is 2000 to 5000 km
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 3500000.0d;	// calls ProcessCameraMove();
		}

		private void scaleWorldButton_Click(object sender, System.EventArgs e)
		{
			// world scale is above 5000 km
			m_cameraManager.SpoilPicture();
			m_cameraManager.Elev = 20000000.0d;	// calls ProcessCameraMove();
		}

		private void loadTilesMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPreloadTiles(false).ShowDialog();
		}

		private void pdaPreloadAndExportMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPreloadTiles(false).ShowDialog();
		}

		private void photoManagerMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPhotoManager(0).ShowDialog();
		}

		private void listPhotosMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPhotoManager(1).ShowDialog();
		}

		private void unrelatedPhotosMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPhotoManager(2).ShowDialog();
		}

		private void photoOptionsMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPhotoManager(3).ShowDialog();
		}

		private void viewImageMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgPhotoFullSize.BringFormUp(null, null);
		}

        private void photoToGeotiffMenuItem_Click(object sender, EventArgs e)
        {
            new DlgPhotoToGeotiff().ShowDialog();
        }

        private void MainForm_Leave(object sender, System.EventArgs e)
		{
			if(LayerWaypoints.This != null)
			{
				LayerWaypoints.This.makeRouteMode = false;	// just in case route mode is still on
			}
		}

		private void MainForm_Deactivate(object sender, System.EventArgs e)
		{
			if(LayerWaypoints.This != null)
			{
				// if we end route mode here, switching to other app or scrolling to the side
				// would end the route mode too.
				//LayerWaypoints.This.makeRouteMode = false;	// just in case route mode is still on
			}
		}

		private void fileMainMmenuItem_Popup(object sender, System.EventArgs e)
		{
			bool hasFiles = Project.FileDescrList.Count > 0;
			this.importsListMenuItem.Enabled = hasFiles;
			this.clearImportsListMenuItem.Enabled = hasFiles;

			rebuildRecentFilesMenu();
		}

		private void notAssociatedButton_Click(object sender, System.EventArgs e)
		{
			this.hintsPanel.Visible = false;
			OptionsForm optionsForm = new OptionsForm(m_pictureManager, 5);
			optionsForm.ShowDialog();
		}

		private void colorAerialCheckBox_MouseHover(object sender, System.EventArgs e)
		{
			Point screenPoint = colorAerialCheckBox.PointToScreen(popupOffset);
			Project.ShowPopup(null, "Color Aerial imagery is experimental and available around Seattle, WA.\nSee quakemap.com Support FAQ for more info.", screenPoint);
		}

		private void manageToolsMenuItem_Click(object sender, System.EventArgs e)
		{
			DlgToolManager toolManager = new DlgToolManager();
			toolManager.ShowDialog();
		}

		private void toolsMainMenuItem_Popup(object sender, System.EventArgs e)
		{
			rebuildToolsMenu();
		}

		public void rebuildToolsMenu()
		{
			// clean out existing menu items:
			for(int i=toolsMainMenuItem.MenuItems.Count-1; i >= Project.toolsFirstIndex ;i--)
			{
				toolsMainMenuItem.MenuItems.RemoveAt(i);
			}

			if(Project.tools.tools.Count > 0)
			{
				MenuItem menuItem = new MenuItem();

				menuItem.Text = "-";
				menuItem.Index = toolsMainMenuItem.MenuItems.Count;

				toolsMainMenuItem.MenuItems.Add(menuItem);
			}

			// rebuild from Project.tools:
			foreach(ToolDescr td in Project.tools.tools)
			{
				MenuItem menuItem = new MenuItem();

				menuItem.Text = td.ToString().Replace("&", "&&");
				menuItem.Click += new System.EventHandler(toolsItemClick);
				menuItem.Index = toolsMainMenuItem.MenuItems.Count;

				toolsMainMenuItem.MenuItems.Add(menuItem);
			}
		}

		private void saveImageMenuItem_Click(object sender, System.EventArgs e)
		{
			ImageExportForm dlg = new ImageExportForm();
			dlg.ShowDialog(this);
		}

		private void saveWebPageMenuItem_Click(object sender, System.EventArgs e)
		{
			WebPageExportForm dlg = new WebPageExportForm();
			dlg.ShowDialog(this);
		}

		private void fileGpsBabelMenuItem_Click(object sender, System.EventArgs e)
		{
			if(Project.GpsBabelWrapperExecutable.Length > 0)
			{
				Project.runGpsBabelWrapper("/mode=file");
			}
			else
			{
				string err = "GpsBabelWrapper missing.\nPlease install GpsBabelWrapper from www.quakemap.com";
				Project.ErrorBox(this, err);
			}
		}

		private void gpsGpsBabelMenuItem_Click(object sender, System.EventArgs e)
		{
			if(Project.GpsBabelWrapperExecutable.Length > 0)
			{
				Project.runGpsBabelWrapper("/mode=gps");
			}
			else
			{
				string err = "GpsBabelWrapper missing.\nPlease install GpsBabelWrapper from www.quakemap.com";
				Project.ErrorBox(this, err);
			}
		}

		private void geoTiffManagerMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgCustomMapsManager().Show();
		}

		private void mapWizardMenuItem_Click(object sender, System.EventArgs e)
		{
			new DlgPdaMapWizard().Show();
		}

		private void helpAtStartCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.showHelpAtStart = false;
			this.hintsPanel.Visible = false;
		}

		private bool sampleFileEnsureAndOpen(string url, string sampleFileName)
		{
			bool ret = false;

			string sampleFilePath = Project.GetSamplePath(sampleFileName);
			try
			{
				if(!File.Exists(sampleFilePath))
				{
					// load the file remotely, don't hold diagnostics if load fails:
					DloadProgressForm loaderForm = new DloadProgressForm(url, sampleFilePath, false, false);
					loaderForm.ShowDialog();
				}

				if(File.Exists(sampleFilePath))
				{
					string[] fileNames = new string[] { sampleFilePath };

					bool anySuccess = FileAndZipIO.readFiles(fileNames);

					if(anySuccess)	// we need to zoom into whole set of dropped files, or refresh in case of JPG
					{
						this.wptEnabled(true);
						if(!m_cameraManager.zoomToCorners())
						{
							m_cameraManager.ProcessCameraMove();	// nowhere to zoom in - just refresh
						}
						ret = true;
					}
					LibSys.StatusBar.Trace("* Ready");
				}
				else
				{
					Project.ErrorBox(this, "Error: could not locate sample file " + sampleFilePath);
				}
			} 
			catch
			{
				Project.ErrorBox(this, "Error: could not download sample file " + url);
			}

			return ret;
		}

		private void sampleFileZippedEnsureAndOpen(string url, string sampleZipFileName, string entryName)
		{
			string sampleZipFilePath = Project.GetSamplePath(sampleZipFileName);
			string sampleFilePath = Project.GetSamplePath(entryName);

			try
			{
				if(!File.Exists(sampleZipFilePath))
				{
					// load the file remotely, don't hold diagnostics if load fails:
					DloadProgressForm loaderForm = new DloadProgressForm(url, sampleZipFilePath, false, false);
					loaderForm.ShowDialog();
				}

				if(File.Exists(sampleZipFilePath) && !File.Exists(sampleFilePath))
				{
					FileAndZipIO.unzipOneFile(sampleZipFilePath, entryName, sampleFilePath);
				}

				if(File.Exists(sampleFilePath))
				{
					string[] fileNames = new string[] { sampleFilePath };

					bool anySuccess = FileAndZipIO.readFiles(fileNames);

					if(anySuccess)	// we need to zoom into whole set of dropped files, or refresh in case of JPG
					{
						this.wptEnabled(true);
						if(!m_cameraManager.zoomToCorners())
						{
							m_cameraManager.ProcessCameraMove();	// nowhere to zoom in - just refresh
						}
					}
					LibSys.StatusBar.Trace("* Ready");
				}
				else
				{
					Project.ErrorBox(this, "Error: could not locate unzipped sample file " + sampleFilePath);
				}
			} 
			catch
			{
				Project.ErrorBox(this, "Error: could not download sample file " + url);
			}
		}

		public bool sampleGalleryTrackOpen()
		{
			Project.gpsZoomIntoTracks = true;
//			Project.photoAnalyzeOnLoad = true;
			Project.thumbDoDisplay = true;

			string sampleFileName = "dana-point-track-only.zip";
			string url = Project.SAMPLES_FOLDER_URL + "/" + sampleFileName;

			return sampleFileEnsureAndOpen(url, sampleFileName);
		}

		private void helpSamplePhotoMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.gpsZoomIntoTracks = true;
//			Project.photoAnalyzeOnLoad = true;
			Project.thumbDoDisplay = true;

			string sampleFileName = "dana-point.zip";
			string url = Project.SAMPLES_FOLDER_URL + "/" + sampleFileName;

			sampleFileEnsureAndOpen(url, sampleFileName);
		}

		private void helpSampleFlyingMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.gpsZoomIntoTracks = true;
//			Project.photoAnalyzeOnLoad = true;
			Project.thumbDoDisplay = true;

			string sampleFileName = "flying-around.zip";
			string url = Project.SAMPLES_FOLDER_URL + "/" + sampleFileName;

			sampleFileEnsureAndOpen(url, sampleFileName);
		}

		private void helpSampleAnnotatedTripMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.gpsZoomIntoTracks = true;
			Project.thumbDoDisplay = true;

			string sampleFileName = "annotated_death_valley.zip";
			string url = Project.SAMPLES_FOLDER_URL + "/" + sampleFileName;

			sampleFileEnsureAndOpen(url, sampleFileName);
		}

		private void helpSampleGeotiffMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.gpsZoomIntoTracks = true;
			Project.thumbDoDisplay = true;

			string sampleFileName = "o33117d6.zip";
			string sampleEntryName = "o33117d6.tiff";
			string url = Project.SAMPLES_FOLDER_URL + "/" + sampleFileName;

			sampleFileZippedEnsureAndOpen(url, sampleFileName, sampleEntryName);
		}

		private void helpSampleWebpageMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.RunBrowser(Project.WEBSITE_LINK_WEBSTYLE + "/demo/test1");
		}

		private void helpSamplePocketqueryMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.gpsZoomIntoTracks = true;
			Project.thumbDoDisplay = true;

			string sampleFileName = "geocaching_pocket_query_39725.zip";
			string url = Project.SAMPLES_FOLDER_URL + "/" + sampleFileName;

			sampleFileEnsureAndOpen(url, sampleFileName);
		}

		private void helpSampleNtsbMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.gpsZoomIntoTracks = true;
			Project.thumbDoDisplay = true;

			string sampleFileName = "ntsb-west.zip";
			string url = Project.SAMPLES_FOLDER_URL + "/" + sampleFileName;

			sampleFileEnsureAndOpen(url, sampleFileName);
		}

		private void helpSampleFleetApiMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.RunBrowser(Project.WEBSITE_LINK_WEBSTYLE + "/api.html");
		}

		private void helpSampleDeleteFilesMenuItem_Click(object sender, System.EventArgs e)
		{
			if(Project.sampleFolderPath.Length == 0 || !Directory.Exists(Project.sampleFolderPath)) 
			{
				Project.MessageBox(this, "There are no sample files on your hard drive.\r\nUse \"Help->Sample Files\" menu to get some from our web site.");
				return;
			}

			DirectoryInfo dir = new DirectoryInfo(Project.sampleFolderPath);
			string message = "Do you want to delete all sample files from your hard drive?\r\n"
				+ "(folder \"" + dir.FullName + "\" will be deleted)\r\n";

			if(Project.YesNoBox(this, message))
			{
				try
				{
					dir.Delete(true);
					Project.MessageBox(this, "Sample files were deleted.\r\nYou can always use \"Help->Sample Files\" menu to get them from our web site.");
				}
				catch
				{
					Project.ErrorBox(this, "Error: could not delete Sample folder.\r\nIt is likely that files are being used by QuakeMap or other program.");
				}
			}
		}

		private void pdaPreloadAlongRouteMenuItem_Click(object sender, System.EventArgs e)
		{
			string message = "To preload tiles along a route or track:\r\n\r\n"
				+ "  1. Make a route by right-clicking on the map, or converting a track to route in Waypoints Manager\r\n"
				+ "  2. Right-click on the end of the route and select \"Preload Tiles Along the Route...\"\r\n"
				+ "  3. Follow the dialog to download Terraserver tiles for use off-line\r\n"
				+ "  4. To see the map on PDA, click \"Export\" button - selecting Memory Card for destination\r\n"
				+ "  5. Put the Memory Card in PDA and use MapAdvisor to show the map.\r\n";
			Project.MessageBox(this, message);
		}

		#endregion

		#region WndProc() to receive API messages (including one with a file name from other instance)

		//static int UM_MYMESSAGE = Project.RegisterWindowMessage("UM_MYMESSAGE");

		protected override void WndProc(ref Message m)
		{
			//if(m.Msg == UM_MYMESSAGE)
			//{
			//	LibSys.StatusBar.Trace("Received message UM_MYMESSAGE");
			//}

			// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/dataexchange/datacopy/datacopyreference/datacopymessages/wm_copydata.asp
			if(m.Msg == Project.WM_COPYDATA)
			{
				try 
				{
					//LibSys.StatusBar.Trace("Received message WM_COPYDATA");
					string strArgs = Project.GetMsgString(m.LParam);
#if DEBUG
					LibSys.StatusBar.Trace("Received Windows message '" + strArgs + "'");
#endif
					this.hintsPanel.Visible = false;

					Api.processApiCall(strArgs);

				} 
				catch(Exception e)
				{
					LibSys.StatusBar.Error("while receiving Windows message string: " + e);
				}
			}
			//LibSys.StatusBar.Trace("Received message " + m.Msg);
			base.WndProc(ref m);
		}
		#endregion

		private void plannerLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			this.plannerPanel.Visible = !this.plannerPanel.Visible;
			PictureManager.This.CameraManager.removeMarkLocation();
			if(!this.plannerPanel.Visible)
			{
				TrackProfileControl.resetTrack();
			}
		}

	}
}
