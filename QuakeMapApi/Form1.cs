using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace QuakeMapApi
{
	/// <summary>
	/// QuakeMap API test.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private QMApiLib qmApiLib = new QMApiLib();

		private System.Windows.Forms.Button startButton;
		private System.Windows.Forms.Label traceLabel;
		private System.Windows.Forms.Button loopButton;
		private System.Windows.Forms.Button fileButton;
		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Button createWptButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button deleteWptButton;
		private System.Windows.Forms.Button topoButton;
		private System.Windows.Forms.Button aerialButton;
		private System.Windows.Forms.Button colorAerialButton;
		private System.Windows.Forms.Button noneButton;
		private System.Windows.Forms.Button createTrackButton;
		private System.Windows.Forms.Button createRouteButton;
		private System.Windows.Forms.Button deleteTrackButton;
		private System.Windows.Forms.GroupBox vehicleGroupBox;
		private System.Windows.Forms.ComboBox vehicleIconComboBox;
		private System.Windows.Forms.Button cycleVehicleButton;
		private System.Windows.Forms.CheckBox keepInViewCheckBox;
		private System.Windows.Forms.Label vehicleHintLabel;
		private System.Windows.Forms.CheckBox trackLogCheckBox;
		private System.Windows.Forms.Button vehWithDescrButton;
		private System.Windows.Forms.Label label2;

		private System.ComponentModel.Container components = null;

		public Form1()
		{
			InitializeComponent();

			this.Text = "QuakeMap API demo";

			vehicleHintLabel.Text = "creates short-lived vehicles moving randomly in all directions.\nclick \"Add...\" repeatedly to create several at a time."; 
		}

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
			this.startButton = new System.Windows.Forms.Button();
			this.traceLabel = new System.Windows.Forms.Label();
			this.loopButton = new System.Windows.Forms.Button();
			this.fileButton = new System.Windows.Forms.Button();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.createWptButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.deleteWptButton = new System.Windows.Forms.Button();
			this.topoButton = new System.Windows.Forms.Button();
			this.aerialButton = new System.Windows.Forms.Button();
			this.colorAerialButton = new System.Windows.Forms.Button();
			this.noneButton = new System.Windows.Forms.Button();
			this.createTrackButton = new System.Windows.Forms.Button();
			this.createRouteButton = new System.Windows.Forms.Button();
			this.deleteTrackButton = new System.Windows.Forms.Button();
			this.cycleVehicleButton = new System.Windows.Forms.Button();
			this.vehicleGroupBox = new System.Windows.Forms.GroupBox();
			this.vehicleIconComboBox = new System.Windows.Forms.ComboBox();
			this.keepInViewCheckBox = new System.Windows.Forms.CheckBox();
			this.vehicleHintLabel = new System.Windows.Forms.Label();
			this.trackLogCheckBox = new System.Windows.Forms.CheckBox();
			this.vehWithDescrButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.vehicleGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// startButton
			// 
			this.startButton.Location = new System.Drawing.Point(272, 40);
			this.startButton.Name = "startButton";
			this.startButton.Size = new System.Drawing.Size(120, 23);
			this.startButton.TabIndex = 0;
			this.startButton.Text = "Start / Single Step";
			this.startButton.Click += new System.EventHandler(this.startButton_Click);
			// 
			// traceLabel
			// 
			this.traceLabel.Location = new System.Drawing.Point(16, 40);
			this.traceLabel.Name = "traceLabel";
			this.traceLabel.Size = new System.Drawing.Size(248, 23);
			this.traceLabel.TabIndex = 1;
			this.traceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// loopButton
			// 
			this.loopButton.Location = new System.Drawing.Point(272, 80);
			this.loopButton.Name = "loopButton";
			this.loopButton.Size = new System.Drawing.Size(120, 23);
			this.loopButton.TabIndex = 2;
			this.loopButton.Text = "Loop 20 moves";
			this.loopButton.Click += new System.EventHandler(this.loopButton_Click);
			// 
			// fileButton
			// 
			this.fileButton.Location = new System.Drawing.Point(272, 120);
			this.fileButton.Name = "fileButton";
			this.fileButton.Size = new System.Drawing.Size(120, 23);
			this.fileButton.TabIndex = 3;
			this.fileButton.Text = "Pass File to Open";
			this.fileButton.Click += new System.EventHandler(this.fileButton_Click);
			// 
			// fileTextBox
			// 
			this.fileTextBox.Location = new System.Drawing.Point(8, 120);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(248, 20);
			this.fileTextBox.TabIndex = 4;
			this.fileTextBox.Text = "C:\\temp\\dana-point.gpz";
			// 
			// createWptButton
			// 
			this.createWptButton.Location = new System.Drawing.Point(120, 168);
			this.createWptButton.Name = "createWptButton";
			this.createWptButton.Size = new System.Drawing.Size(128, 23);
			this.createWptButton.TabIndex = 5;
			this.createWptButton.Text = "Create 20 Geocaches";
			this.createWptButton.Click += new System.EventHandler(this.createWptButton_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(376, 23);
			this.label1.TabIndex = 6;
			this.label1.Text = "Controlling QuakeMap from your program";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// deleteWptButton
			// 
			this.deleteWptButton.Location = new System.Drawing.Point(264, 168);
			this.deleteWptButton.Name = "deleteWptButton";
			this.deleteWptButton.Size = new System.Drawing.Size(128, 23);
			this.deleteWptButton.TabIndex = 7;
			this.deleteWptButton.Text = "Delete 20 Geocaches";
			this.deleteWptButton.Click += new System.EventHandler(this.deleteWptButton_Click);
			// 
			// topoButton
			// 
			this.topoButton.Location = new System.Drawing.Point(16, 168);
			this.topoButton.Name = "topoButton";
			this.topoButton.Size = new System.Drawing.Size(75, 16);
			this.topoButton.TabIndex = 8;
			this.topoButton.Text = "topo";
			this.topoButton.Click += new System.EventHandler(this.topoButton_Click);
			// 
			// aerialButton
			// 
			this.aerialButton.Location = new System.Drawing.Point(16, 192);
			this.aerialButton.Name = "aerialButton";
			this.aerialButton.Size = new System.Drawing.Size(75, 16);
			this.aerialButton.TabIndex = 9;
			this.aerialButton.Text = "aerial";
			this.aerialButton.Click += new System.EventHandler(this.aerialButton_Click);
			// 
			// colorAerialButton
			// 
			this.colorAerialButton.Location = new System.Drawing.Point(16, 216);
			this.colorAerialButton.Name = "colorAerialButton";
			this.colorAerialButton.Size = new System.Drawing.Size(75, 16);
			this.colorAerialButton.TabIndex = 10;
			this.colorAerialButton.Text = "color aerial";
			this.colorAerialButton.Click += new System.EventHandler(this.colorAerialButton_Click);
			// 
			// noneButton
			// 
			this.noneButton.Location = new System.Drawing.Point(16, 240);
			this.noneButton.Name = "noneButton";
			this.noneButton.Size = new System.Drawing.Size(75, 16);
			this.noneButton.TabIndex = 11;
			this.noneButton.Text = "none";
			this.noneButton.Click += new System.EventHandler(this.noneButton_Click);
			// 
			// createTrackButton
			// 
			this.createTrackButton.Location = new System.Drawing.Point(120, 200);
			this.createTrackButton.Name = "createTrackButton";
			this.createTrackButton.Size = new System.Drawing.Size(128, 23);
			this.createTrackButton.TabIndex = 12;
			this.createTrackButton.Text = "Create Track";
			this.createTrackButton.Click += new System.EventHandler(this.createTrackButton_Click);
			// 
			// createRouteButton
			// 
			this.createRouteButton.Location = new System.Drawing.Point(120, 232);
			this.createRouteButton.Name = "createRouteButton";
			this.createRouteButton.Size = new System.Drawing.Size(128, 23);
			this.createRouteButton.TabIndex = 13;
			this.createRouteButton.Text = "Create Route";
			this.createRouteButton.Click += new System.EventHandler(this.createRouteButton_Click);
			// 
			// deleteTrackButton
			// 
			this.deleteTrackButton.Location = new System.Drawing.Point(264, 216);
			this.deleteTrackButton.Name = "deleteTrackButton";
			this.deleteTrackButton.Size = new System.Drawing.Size(128, 23);
			this.deleteTrackButton.TabIndex = 14;
			this.deleteTrackButton.Text = "Delete Track/Route";
			this.deleteTrackButton.Click += new System.EventHandler(this.deleteTrackButton_Click);
			// 
			// cycleVehicleButton
			// 
			this.cycleVehicleButton.Location = new System.Drawing.Point(160, 16);
			this.cycleVehicleButton.Name = "cycleVehicleButton";
			this.cycleVehicleButton.Size = new System.Drawing.Size(120, 23);
			this.cycleVehicleButton.TabIndex = 15;
			this.cycleVehicleButton.Text = "Add, Move && Delete";
			this.cycleVehicleButton.Click += new System.EventHandler(this.cycleVehicleButton_Click);
			// 
			// vehicleGroupBox
			// 
			this.vehicleGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.label2,
																						  this.vehWithDescrButton,
																						  this.trackLogCheckBox,
																						  this.vehicleHintLabel,
																						  this.keepInViewCheckBox,
																						  this.vehicleIconComboBox,
																						  this.cycleVehicleButton});
			this.vehicleGroupBox.Location = new System.Drawing.Point(8, 280);
			this.vehicleGroupBox.Name = "vehicleGroupBox";
			this.vehicleGroupBox.Size = new System.Drawing.Size(392, 136);
			this.vehicleGroupBox.TabIndex = 19;
			this.vehicleGroupBox.TabStop = false;
			this.vehicleGroupBox.Text = "Vehicle Management";
			// 
			// vehicleIconComboBox
			// 
			this.vehicleIconComboBox.Items.AddRange(new object[] {
																	 "gps_car_default",
																	 "jet",
																	 "gonewint",
																	 "plane737",
																	 "sailboat"});
			this.vehicleIconComboBox.Location = new System.Drawing.Point(8, 16);
			this.vehicleIconComboBox.Name = "vehicleIconComboBox";
			this.vehicleIconComboBox.Size = new System.Drawing.Size(136, 21);
			this.vehicleIconComboBox.TabIndex = 19;
			this.vehicleIconComboBox.Text = "none";
			// 
			// keepInViewCheckBox
			// 
			this.keepInViewCheckBox.Location = new System.Drawing.Point(296, 12);
			this.keepInViewCheckBox.Name = "keepInViewCheckBox";
			this.keepInViewCheckBox.Size = new System.Drawing.Size(88, 16);
			this.keepInViewCheckBox.TabIndex = 20;
			this.keepInViewCheckBox.Text = "keep in view";
			// 
			// vehicleHintLabel
			// 
			this.vehicleHintLabel.Location = new System.Drawing.Point(8, 48);
			this.vehicleHintLabel.Name = "vehicleHintLabel";
			this.vehicleHintLabel.Size = new System.Drawing.Size(376, 48);
			this.vehicleHintLabel.TabIndex = 21;
			this.vehicleHintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// trackLogCheckBox
			// 
			this.trackLogCheckBox.Location = new System.Drawing.Point(296, 30);
			this.trackLogCheckBox.Name = "trackLogCheckBox";
			this.trackLogCheckBox.Size = new System.Drawing.Size(88, 16);
			this.trackLogCheckBox.TabIndex = 22;
			this.trackLogCheckBox.Text = "track log";
			// 
			// vehWithDescrButton
			// 
			this.vehWithDescrButton.Location = new System.Drawing.Point(272, 104);
			this.vehWithDescrButton.Name = "vehWithDescrButton";
			this.vehWithDescrButton.Size = new System.Drawing.Size(104, 23);
			this.vehWithDescrButton.TabIndex = 23;
			this.vehWithDescrButton.Text = "Create";
			this.vehWithDescrButton.Click += new System.EventHandler(this.vehWithDescrButton_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 104);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(248, 23);
			this.label2.TabIndex = 24;
			this.label2.Text = "Vehicle with description, URL, popup -->";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(408, 430);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.vehicleGroupBox,
																		  this.deleteTrackButton,
																		  this.createRouteButton,
																		  this.createTrackButton,
																		  this.noneButton,
																		  this.colorAerialButton,
																		  this.aerialButton,
																		  this.topoButton,
																		  this.deleteWptButton,
																		  this.label1,
																		  this.createWptButton,
																		  this.fileTextBox,
																		  this.fileButton,
																		  this.loopButton,
																		  this.traceLabel,
																		  this.startButton});
			this.Name = "Form1";
			this.Text = "QuakeMap API Demo";
			this.vehicleGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private double altMeters = 500.0d;

		private void startButton_Click(object sender, System.EventArgs e)
		{
			step();
		}

		private void step()
		{
			string strCmd = "/lat=47.55|/lon=-122.135|/map=color|/elev=" + altMeters;

			traceLabel.Text = "Camera at " + altMeters + " meters";
	
			altMeters+= 20.0d;

			try
			{
				qmApiLib.CommandMappingEngine(strCmd);
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		private Thread m_workerThread = null;

		private void loopButton_Click(object sender, System.EventArgs e)
		{
			m_workerThread =	new Thread( new ThreadStart(doWork1));
			m_workerThread.IsBackground = true;	// terminate with the main process
			m_workerThread.Name = "Worker";
			m_workerThread.Priority = ThreadPriority.BelowNormal;
			m_workerThread.Start();
		}

		private void doWork1()
		{
			for (int i=0; i < 20 ;i++)
			{
				Thread.Sleep(1000);
				step();
			}
		}

		/// <summary>
		/// make QuakeMap open a file and zoom into it
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void fileButton_Click(object sender, System.EventArgs e)
		{
			try
			{
				string strCmd = "/map=aerial";
				qmApiLib.CommandMappingEngine(strCmd);

				strCmd = fileTextBox.Text;
				qmApiLib.CommandMappingEngine(strCmd);
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		/// <summary>
		/// Create 20 waypoints and keep them in view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void createWptButton_Click(object sender, System.EventArgs e)
		{
			m_workerThread =	new Thread( new ThreadStart(doWork2));
			m_workerThread.IsBackground = true;	// terminate with the main process
			m_workerThread.Name = "Worker";
			m_workerThread.Priority = ThreadPriority.BelowNormal;
			m_workerThread.Start();
		}

		private void doWork2()
		{
			try
			{
				// first make sure we are at a good altitude:
				string strCmd = "/map=aerial|/elev=6000";
				qmApiLib.CommandMappingEngine(strCmd);
				
				qmApiLib.resetZoom();		// prepare for a zoom into whole set of waypoints created below

				// now create 20 waypoints:
				for (int i=0; i < 20 ;i++)
				{
					CreateInfo ci = new CreateInfo();

					ci.lng = -117.123 + ( i / 100.0d);
					ci.lat = 34.123 + ( i / 100.0d);
					ci.name = "GC" + i;
					ci.urlName = "Waypoint" + i;
					ci.type = "geocache";				// "waypoint"
					ci.typeExtra = (i % 2 == 0) ? "Geocache" : "Geocache Found";		// "Hospital"
					ci.url = "http://www.ntsb.gov/ntsb/brief.asp?ev_id=20001212X22305";
					ci.source = "My API Demo";

					bool keepInView = true;

					traceLabel.Text = "Creating Waypoint" + i;

					qmApiLib.createWaypoint(ci, keepInView);

					Thread.Sleep(1000);		// just to have a visual effect of following the points as they are created
				}

				// perform zoom into the whole set of points:
				// qmApiLib.doZoom();
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		private void deleteWptButton_Click(object sender, System.EventArgs e)
		{
			try
			{
				// now create 20 waypoints:
				for (int i=0; i < 20 ;i++)
				{
					CreateInfo ci = new CreateInfo();

					ci.name = "GC" + i;

					qmApiLib.deleteWaypoint(ci);
				}

				qmApiLib.refresh();
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		/// <summary>
		/// The buttons below switch map type
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void topoButton_Click(object sender, System.EventArgs e)
		{
			setMap("topo");		
		}

		private void aerialButton_Click(object sender, System.EventArgs e)
		{
			setMap("aerial");		
		}

		private void colorAerialButton_Click(object sender, System.EventArgs e)
		{
			setMap("color");		
		}

		private void noneButton_Click(object sender, System.EventArgs e)
		{
			setMap("none");		
		}

		private void setMap(string mapType)
		{
			try
			{
				string strCmd = "/map=" + mapType;

				qmApiLib.CommandMappingEngine(strCmd);
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		private void createTrackButton_Click(object sender, System.EventArgs e)
		{
			try
			{
				// reset zoom rectangle:
				qmApiLib.resetZoom();

				setMap("none");

				qmApiLib.createTrack("new track", "My API Demo Track");

				DateTime dt = DateTime.Now.AddMinutes(-20.0d);
				Random rand = new Random( );

				double lng = -117.223d;
				double lat = 34.223d;

				// now create 20 waypoints:
				for (int i=0; i < 20 ;i++)
				{
					CreateInfo ci = new CreateInfo();

					ci.lng = lng;
					ci.lat = lat;
					ci.dateTime = TimeZone.CurrentTimeZone.ToUniversalTime(dt);
					dt = dt.AddMinutes(1.0d);
					//ci.urlName = "Trkpoint" + i;	// can set it if you want to display a label
					ci.type = "trkpt";
					ci.id = -2;			// join the last track
					ci.source = "My API Demo Track";

					traceLabel.Text = "Creating Trackpoint" + i;

					qmApiLib.createWaypoint(ci, false);

					//lng += (rand.NextDouble() - 0.5d) / 100.0d;
					lng += rand.NextDouble() / 100.0d;
					//lat += (rand.NextDouble() - 0.5d) / 100.0d;
					lat += rand.NextDouble() / 100.0d;
				}

				qmApiLib.doZoom();
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		private void createRouteButton_Click(object sender, System.EventArgs e)
		{
			try
			{
				// reset zoom rectangle:
				qmApiLib.resetZoom();

				setMap("none");

				qmApiLib.createRoute("new route", "My API Demo Route");

				DateTime dt = DateTime.Now.AddMinutes(-20.0d);
				Random rand = new Random( );

				double lng = -117.223d;
				double lat = 34.223d;

				// now create 20 waypoints:
				for (int i=0; i < 20 ;i++)
				{
					CreateInfo ci = new CreateInfo();

					ci.lng = lng;
					ci.lat = lat;
					ci.dateTime = TimeZone.CurrentTimeZone.ToUniversalTime(dt);
					dt = dt.AddMinutes(1.0d);
					//ci.urlName = "Rtepoint" + i;	// can set it if you want to display a label
					ci.type = "rtept";
					ci.id = -2;			// join the last track
					ci.source = "My API Demo Route";

					traceLabel.Text = "Creating Trackpoint" + i;

					qmApiLib.createWaypoint(ci, false);

					//lng += (rand.NextDouble() - 0.5d) / 100.0d;
					lng -= rand.NextDouble() / 100.0d;
					//lat += (rand.NextDouble() - 0.5d) / 100.0d;
					lat += rand.NextDouble() / 100.0d;
				}

				qmApiLib.doZoom();
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		private void deleteTrackButton_Click(object sender, System.EventArgs e)
		{
			try
			{
				//qmApiLib.deleteTrackOrRoute("new route", "");
				//qmApiLib.deleteTrackOrRoute("", "My API Demo Route");
				//qmApiLib.deleteTrackOrRoute("new track", "");
				//qmApiLib.deleteTrackOrRoute("", "My API Demo Track");
				qmApiLib.deleteTrackOrRoute("", "");		// delete the last one
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}

		private void cycleVehicleButton_Click(object sender, System.EventArgs e)
		{
			string iconName = "" + vehicleIconComboBox.SelectedItem;
			new VehicleMover(iconName, traceLabel, keepInViewCheckBox.Checked, this.trackLogCheckBox.Checked);
		}

		private void vehWithDescrButton_Click(object sender, System.EventArgs e)
		{
			double lng = -122.135d;
			double lat = 47.55d;
			double elev = 500.0d;		// camera altitude

			string strCmd = "/lat=" + lat + "|/lon=" + lng + "|/map=color|/elev=" + elev;

			try
			{
				// position the screen:
				qmApiLib.CommandMappingEngine(strCmd);

				DateTime dateTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now);
				string descr = "The F50 began production in 1995 and is clearly in a class by itself.\nKnown as a super exotic, the Ferrari F50 can reach speeds of 200 MPH\nand accelerate to 60 MPH in under 3.8 seconds.\nWith a price tag of over $500,000, only a handful of people can afford it.";

				lng += 0.001d;
				lat += 0.001d;

				qmApiLib.createVehicle(lng, lat, 0.0d, dateTime, "Ferrari F50", "gps_car_default", "My API Demo Vehicle", "http://www.fantasycars.com/Ferrari_F50/ferrari_f50.html", descr, true, false);
			}
			catch (Exception exc)
			{
				traceLabel.Text = exc.Message;
			}
		}
	}

	/// <summary>
	/// test class with it's own thread, responsible for adding, moving around and deleting a vehicle
	/// </summary>
	public class VehicleMover
	{
		static int vehicleNumber = 1;

		private double lng = -122.135d;
		private double lat = 47.55d;
		private double elev = 500.0d;		// camera altitude

		private string m_iconName;
		private string m_vehicleName;
		private string m_descr = "My Corvette is here\nTry catch me\nIf you can\n";
		private int m_direction;

		private Label m_traceLabel;
		private bool m_keepInView;
		private bool m_doTrackLog;
		private Thread m_workerThread = null;
		private QMApiLib qmApiLib = new QMApiLib();

		public VehicleMover(string iconName, Label traceLabel, bool keepInView, bool doTrackLog)
		{
			m_iconName = iconName;
			m_traceLabel = traceLabel;
			m_keepInView = keepInView;
			m_doTrackLog = doTrackLog;
			m_vehicleName = "MyChevy-" + vehicleNumber;
			vehicleNumber++;
			m_direction = vehicleNumber % 4;

			m_workerThread = new Thread( new ThreadStart(doWork));
			m_workerThread.IsBackground = true;	// terminate with the main process
			m_workerThread.Name = "Worker";
			m_workerThread.Priority = ThreadPriority.BelowNormal;
			m_workerThread.Start();
		}

		private void doWork()
		{
			try
			{
				addVehicle();
				moveVehicle();
				deleteVehicle();
			}
			catch (Exception exc)
			{
				m_traceLabel.Text = exc.Message;
			}
		}

		private void addVehicle()
		{
			if(vehicleNumber == 2)
			{
				string strCmd = "/lat=" + lat + "|/lon=" + lng + "|/map=color|/elev=" + elev;

				qmApiLib.CommandMappingEngine(strCmd);
			}

			DateTime dateTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now);

			qmApiLib.createVehicle(lng, lat, 0.0d, dateTime, m_vehicleName, m_iconName, "My API Demo Vehicle", "http://www.quakemap.com", m_descr, m_keepInView, m_doTrackLog);
		}

		private void moveVehicle()
		{
			Random rand = new Random( );

			// now move vehicle 20 times:
			for (int i=0; i < 20 ;i++)
			{
				m_traceLabel.Text = "Vehicle moves " + i;
				DateTime dateTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now);

				qmApiLib.moveVehicle(lng, lat, 0.0d, dateTime, m_vehicleName, "", "", "", "" + DateTime.Now, m_keepInView, m_doTrackLog);

				switch (m_direction)
				{
					case 0:
						//lng += (rand.NextDouble() - 0.5d) / 100.0d;
						lng += rand.NextDouble() / 3000.0d;
						//lat += (rand.NextDouble() - 0.5d) / 100.0d;
						lat += rand.NextDouble() / 3000.0d;
						break;
					case 1:
						lng -= rand.NextDouble() / 3000.0d;
						lat += rand.NextDouble() / 3000.0d;
						break;
					case 2:
						lng += rand.NextDouble() / 3000.0d;
						lat -= rand.NextDouble() / 3000.0d;
						break;
					default:
						lng -= rand.NextDouble() / 3000.0d;
						lat -= rand.NextDouble() / 3000.0d;
						break;
				}

				Thread.Sleep(1000);		// just to have a visual effect of following the points as they are created
			}
		}

		private void deleteVehicle()
		{
			qmApiLib.deleteVehicle(m_vehicleName, "My API Demo Vehicle");
		}
	}
}
