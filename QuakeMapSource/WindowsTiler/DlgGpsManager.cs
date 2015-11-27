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
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

using LibSys;
using LibNet;
using LibGeo;
using LibGps;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgGpsManager.
	/// </summary>
	public class DlgGpsManager : System.Windows.Forms.Form, IButtonPanelResponder
	{
		public static DlgGpsManager This = null;	// helps to ensure that only one dialog stays up, and supplies Form to MagellanProtoAppLayer ErrorBox
		public static bool isUp { get { return This != null; } }

		public static bool isBusy	// talking to device
									{ get { return This != null && This.m_commandThread != null; }	}

		private Thread m_commandThread = null;
		private GpsBase m_gps = null;
		private DateTime m_started = DateTime.Now;
		private Color m_initialColor;

		// the "duplicate"-related logic here is not needed, as all creators check for isUp and isBusy
		private bool duplicate;	// disables all functions, causes close()

		private GpsInsertWaypoint	m_insertWaypoint;
		private CameraManager		m_cameraManager;
		private PictureManager		m_pictureManager;
		private ArrayList			m_waypoints;
		private ArrayList			m_routes;
		private string				m_command;

		private bool inSet = false;
		private bool closing = false;

		private bool inResize = false;
		private static int gpsManagerX = 200;
		private static int gpsManagerY = 200;

		private GpsButtonPanelControl m_gpsButtonPanel = null;	// device-dependent, created as needed

		#region controls defined here

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox makeComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox modelComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox portComboBox;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Button testButton;
		private System.Windows.Forms.Button portSettingsButton;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Button logButton;
		private System.Windows.Forms.Button stopButton;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage setupTabPage;
		private System.Windows.Forms.TabPage dataTabPage;
		private System.Windows.Forms.TabPage trackingTabPage;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ComboBox interfaceComboBox;
		private System.Windows.Forms.Button tracksButton;
		private System.Windows.Forms.LinkLabel toTransferLinkLabel;
		private System.Windows.Forms.TabPage optionsTabPage;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.CheckBox logErrorsCheckBox;
		private System.Windows.Forms.CheckBox logPacketsCheckBox;
		private System.Windows.Forms.CheckBox logProtocolCheckBox;
		private System.Windows.Forms.Label positionLabel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label timeLabel;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label accuracyLabel;
		private System.Windows.Forms.Button rtStartButton;
		private System.Windows.Forms.Button rtStopButton;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label speedLabel;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label altLabel;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label fixLabel;
		private System.Windows.Forms.CheckBox trackOnMapCheckBox;
		private System.Windows.Forms.CheckBox keepInViewCheckBox;
		private System.Windows.Forms.CheckBox trackLogCheckBox;
		private System.Windows.Forms.CheckBox gpsSimulateCheckBox;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.ComboBox comboBoxBaud;
		private System.Windows.Forms.Panel dataTabPagePanel;
		private System.Windows.Forms.Label trkptCountLabel;
		private System.Windows.Forms.ComboBox trackToleranceComboBox;
		private System.Windows.Forms.GroupBox progressGroupBox;
		private System.Windows.Forms.Button progressDetailButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox vehicleIconComboBox;
		private System.Windows.Forms.Label gpsIconHintLabel;
		private System.Windows.Forms.CheckBox mgnHandshakeCheckBox;
		private System.Windows.Forms.Panel setupPagePanel;
		private System.Windows.Forms.Panel rtPagePanel;
		private System.Windows.Forms.TextBox messageLabel;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label slopeLabel;
		private System.Windows.Forms.Button shrinkMaxButton;
		private System.Windows.Forms.Label magnHeadingLabel;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label trueHeadingLabel;
		private System.Windows.Forms.GroupBox gpsBasicsGroupBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		public DlgGpsManager(GpsInsertWaypoint insertWaypoint, CameraManager cameraManager,
			PictureManager pictureManager, ArrayList waypoints, ArrayList routes, string command)
		{
			//LibSys.StatusBar.Trace("DlgGpsManager()");
			if(isUp) 
			{
				// duplicate for this instance stays true, so that all functions are disabled
				// and the form will be closed on load.
				this.duplicate = true;
				this.Load += new System.EventHandler(this.DlgGpsManager_Load);	// we will close this instance on load
			}
			else
			{
				This = this;
				duplicate = false;

				m_insertWaypoint = insertWaypoint;
				m_cameraManager = cameraManager;
				m_pictureManager = pictureManager;
				m_waypoints = waypoints == null ? WaypointsCache.WaypointsAll : waypoints;
				m_routes = routes == null ? WaypointsCache.RoutesAll : routes;
				m_command = command;

				//
				// Required for Windows Form Designer support
				//
				InitializeComponent();

				this.SuspendLayout();
				populateGpsPanel();

				//#if DEBUG
				//			Project.gpsLogProtocol = true;
				//			Project.gpsLogPackets = true;
				//			Project.gpsLogErrors = true;
				//#else
				Project.gpsLogProtocol = false;
				Project.gpsLogPackets = false;
				Project.gpsLogErrors = false;
				//#endif

				Project.setDlgIcon(this);
				//LibSys.StatusBar.Trace("DlgGpsManager() done");

				m_initialColor = gpsBasicsGroupBox.BackColor;
				gpsBasicsGroupBox.BackColor = Color.Yellow;
				gpsBasicsGroupBox.Text = "stopped";

				progressDetailDelta = progressGroupBox.Height;
				maxShrinkDelta = progressDetailDelta + Math.Abs(this.PointToScreen(progressDetailButton.Bounds.Location).Y - this.PointToScreen(shrinkMaxButton.Bounds.Location).Y) - slopeLabel.Height;
				this.ResumeLayout(true);
			}
		}

		private void DlgGpsManager_Load(object sender, System.EventArgs e)
		{
			//LibSys.StatusBar.Trace("DlgGpsManager_Load()");
			if(duplicate)
			{
				this.Dispose();
				return;
			}

			this.SuspendLayout();
			inSet = true;
			
			this.makeComboBox.Items.AddRange(AllGpsDevices.AllMakesNames);
			this.makeComboBox.SelectedIndex = Project.gpsMakeIndex;

			// we do not want to put bauld rate here because it may have been changed from default:
			//Project.gpsPortSettings.baudRate = AllGpsDevices.AllMakesDefaultBaudRates[this.makeComboBox.SelectedIndex];

			this.modelComboBox.Items.AddRange(AllGpsDevices.AllModelsNames[this.makeComboBox.SelectedIndex]);
			this.modelComboBox.SelectedIndex = Project.gpsModelIndex;

			this.interfaceComboBox.Items.AddRange(AllGpsDevices.AllInterfacesNames[this.makeComboBox.SelectedIndex]);
			this.interfaceComboBox.SelectedIndex = Project.gpsInterfaceIndex;

			keepInViewCheckBox.Enabled = trackOnMapCheckBox.Checked;

			this.portComboBox.Items.AddRange(new object[] {	  "COM1:",
															  "COM2:",
															  "COM3:",
															  "COM4:",
															  "COM5:",
															  "COM6:",
															  "COM7:",
															  "COM8:", "COM9:", "COM10:", "COM11:", "COM12:", "COM13:", "COM14:", "COM15:", "COM16:", "COM17:", "COM18:", "COM19:"});

			this.comboBoxBaud.Items.AddRange(new object[] {	  "1200",
															  "2400",
															  "4800",
															  "9600",
															  "19200",
															  "38400",
															  "57600",
															  "115200"});

			this.portComboBox.Text = Project.gpsPortSettings.port;
			this.comboBoxBaud.Text = "" + Project.gpsPortSettings.baudRate;

			inSet = false;

			progressBar.Visible = false;
			progressBar.Minimum = 0;
			stopButton.Enabled = false;
			rtStartButton.Visible = true;
			rtStopButton.Visible = false;
			tracksButton.Visible = !this.Modal;	// no show for modal dialogs

			logProtocolCheckBox.Checked = Project.gpsLogProtocol;
			logPacketsCheckBox.Checked = Project.gpsLogPackets;
			logErrorsCheckBox.Checked = Project.gpsLogErrors;
			mgnHandshakeCheckBox.Checked = Project.gpsMagellanHandshake;


			trackToleranceComboBox.SelectedIndex = Project.gpsRtToleranceIndex;
			trackOnMapCheckBox.Checked = Project.gpsRtTrackOnMap;
			keepInViewCheckBox.Checked = Project.gpsRtKeepInView;
			trackLogCheckBox.Checked = Project.gpsRtTrackLog;
			trackToleranceComboBox.Enabled = trackLogCheckBox.Checked;
			trkptCountLabel.Visible = trackLogCheckBox.Checked;
			gpsSimulateCheckBox.Checked = Project.gpsSimulate;

			if(m_command != null)
			{
				switch (m_command)
				{
					case "startRealtime":
						maxShrinked = true;
						tabControl1.SelectedIndex = 2;
						rtStartButton_Click(null, EventArgs.Empty);
						break;
					case "uploadTracks":
					case "uploadRoutes":
						tabControl1.SelectedIndex = 1;
						doRefresh = Project.gpsZoomIntoTracks;
						mayZoom = true;
						doGpsCommand(m_command);
						break;
					default:		// getWaypoints and others - likely on the Data tab
						tabControl1.SelectedIndex = 1;
						doGpsCommand(m_command);
						break;
				}
			}
			else
			{
				tabControl1.SelectedIndex = 1;
			}

#if !DEBUG
			logButton.Visible = false;
#endif

			// preload important vehicle images:
			VehiclesCache.getVehicleImage("gps_nofix");
			VehiclesCache.getVehicleImage("gps_car_default");

			initialWidth = this.ClientSize.Width;
			initialHeight = this.ClientSize.Height;

			sizeTheDialog();

			if(Project.fitsScreen(gpsManagerX, gpsManagerY, this.Width, this.Height))
			{
				inResize = true;
				this.Location = new Point(gpsManagerX, gpsManagerY);
				inResize = false;
			}

			this.ResumeLayout(true);

			this.BringToFront();
			//LibSys.StatusBar.Trace("DlgGpsManager_Load() done");
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			//LibSys.StatusBar.Trace("DlgGpsManager: Dispose()");
			if(!duplicate)
			{
				if( disposing )
				{
					if(components != null)
					{
						components.Dispose();
					}
				}
				base.Dispose( disposing );
				//LibSys.StatusBar.Trace("DlgGpsManager: Dispose() done");
			}
		}

		// can be called from different thread:
		public static void CloseGently()
		{
			try 
			{
				if(This != null)
				{
					if(Project.mainForm.InvokeRequired)
					{
						Project.mainForm.Invoke(new MethodInvoker(thisClose));
					} 
					else
					{
						thisClose();
					}
				} 
			} 
			catch {}
			//GC.Collect();
			//GC.WaitForPendingFinalizers();
		}

		private static void thisClose()
		{
			This.Close();
		}


		// can be called from different thread:
		public static void BringFormUp()
		{
			try 
			{
				if(This != null)
				{
					if(Project.mainForm.InvokeRequired)
					{
						Project.mainForm.Invoke(new MethodInvoker(RunBringUp));
					} 
					else
					{
						RunBringUp();
					}
				} 
			} 
			catch {}
			//GC.Collect();
			//GC.WaitForPendingFinalizers();
		}

		private static void RunBringUp()
		{
			This.BringToFront();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.makeComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.modelComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.portComboBox = new System.Windows.Forms.ComboBox();
			this.closeButton = new System.Windows.Forms.Button();
			this.testButton = new System.Windows.Forms.Button();
			this.portSettingsButton = new System.Windows.Forms.Button();
			this.progressGroupBox = new System.Windows.Forms.GroupBox();
			this.messageLabel = new System.Windows.Forms.TextBox();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.logButton = new System.Windows.Forms.Button();
			this.stopButton = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.setupTabPage = new System.Windows.Forms.TabPage();
			this.setupPagePanel = new System.Windows.Forms.Panel();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label12 = new System.Windows.Forms.Label();
			this.comboBoxBaud = new System.Windows.Forms.ComboBox();
			this.gpsSimulateCheckBox = new System.Windows.Forms.CheckBox();
			this.toTransferLinkLabel = new System.Windows.Forms.LinkLabel();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.interfaceComboBox = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.dataTabPage = new System.Windows.Forms.TabPage();
			this.dataTabPagePanel = new System.Windows.Forms.Panel();
			this.trackingTabPage = new System.Windows.Forms.TabPage();
			this.rtPagePanel = new System.Windows.Forms.Panel();
			this.gpsBasicsGroupBox = new System.Windows.Forms.GroupBox();
			this.positionLabel = new System.Windows.Forms.Label();
			this.altLabel = new System.Windows.Forms.Label();
			this.trueHeadingLabel = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.magnHeadingLabel = new System.Windows.Forms.Label();
			this.slopeLabel = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.shrinkMaxButton = new System.Windows.Forms.Button();
			this.speedLabel = new System.Windows.Forms.Label();
			this.rtStartButton = new System.Windows.Forms.Button();
			this.rtStopButton = new System.Windows.Forms.Button();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.trkptCountLabel = new System.Windows.Forms.Label();
			this.trackToleranceComboBox = new System.Windows.Forms.ComboBox();
			this.trackLogCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.keepInViewCheckBox = new System.Windows.Forms.CheckBox();
			this.trackOnMapCheckBox = new System.Windows.Forms.CheckBox();
			this.accuracyLabel = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.timeLabel = new System.Windows.Forms.Label();
			this.statusLabel = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.fixLabel = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.optionsTabPage = new System.Windows.Forms.TabPage();
			this.mgnHandshakeCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.vehicleIconComboBox = new System.Windows.Forms.ComboBox();
			this.gpsIconHintLabel = new System.Windows.Forms.Label();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.logErrorsCheckBox = new System.Windows.Forms.CheckBox();
			this.logPacketsCheckBox = new System.Windows.Forms.CheckBox();
			this.logProtocolCheckBox = new System.Windows.Forms.CheckBox();
			this.tracksButton = new System.Windows.Forms.Button();
			this.progressDetailButton = new System.Windows.Forms.Button();
			this.progressGroupBox.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.setupTabPage.SuspendLayout();
			this.setupPagePanel.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.dataTabPage.SuspendLayout();
			this.trackingTabPage.SuspendLayout();
			this.rtPagePanel.SuspendLayout();
			this.gpsBasicsGroupBox.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.optionsTabPage.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(13, 33);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(79, 34);
			this.label1.TabIndex = 0;
			this.label1.Text = "Make:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// makeComboBox
			// 
			this.makeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.makeComboBox.Location = new System.Drawing.Point(106, 36);
			this.makeComboBox.Name = "makeComboBox";
			this.makeComboBox.Size = new System.Drawing.Size(331, 21);
			this.makeComboBox.TabIndex = 1;
			this.makeComboBox.SelectedIndexChanged += new System.EventHandler(this.makeComboBox_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(13, 90);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(79, 34);
			this.label2.TabIndex = 2;
			this.label2.Text = "Model:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.Visible = false;
			// 
			// modelComboBox
			// 
			this.modelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.modelComboBox.Location = new System.Drawing.Point(106, 97);
			this.modelComboBox.Name = "modelComboBox";
			this.modelComboBox.Size = new System.Drawing.Size(331, 21);
			this.modelComboBox.TabIndex = 2;
			this.modelComboBox.Visible = false;
			this.modelComboBox.SelectedIndexChanged += new System.EventHandler(this.modelComboBox_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(37, 33);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(58, 33);
			this.label3.TabIndex = 4;
			this.label3.Text = "Port:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// portComboBox
			// 
			this.portComboBox.Location = new System.Drawing.Point(106, 38);
			this.portComboBox.Name = "portComboBox";
			this.portComboBox.Size = new System.Drawing.Size(200, 21);
			this.portComboBox.TabIndex = 5;
			this.portComboBox.SelectedIndexChanged += new System.EventHandler(this.portComboBox_SelectedIndexChanged);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(726, 370);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(79, 22);
			this.closeButton.TabIndex = 38;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// testButton
			// 
			this.testButton.Location = new System.Drawing.Point(530, 45);
			this.testButton.Name = "testButton";
			this.testButton.Size = new System.Drawing.Size(242, 35);
			this.testButton.TabIndex = 0;
			this.testButton.Text = "Test / Identify GPS";
			this.testButton.Click += new System.EventHandler(this.testButton_Click);
			// 
			// portSettingsButton
			// 
			this.portSettingsButton.Location = new System.Drawing.Point(595, 33);
			this.portSettingsButton.Name = "portSettingsButton";
			this.portSettingsButton.Size = new System.Drawing.Size(134, 34);
			this.portSettingsButton.TabIndex = 4;
			this.portSettingsButton.Text = "Advanced...";
			this.portSettingsButton.Click += new System.EventHandler(this.portSettingsButton_Click);
			// 
			// progressGroupBox
			// 
			this.progressGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.progressGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.messageLabel});
			this.progressGroupBox.Location = new System.Drawing.Point(5, 395);
			this.progressGroupBox.Name = "progressGroupBox";
			this.progressGroupBox.Size = new System.Drawing.Size(810, 140);
			this.progressGroupBox.TabIndex = 11;
			this.progressGroupBox.TabStop = false;
			this.progressGroupBox.Text = "Progress";
			// 
			// messageLabel
			// 
			this.messageLabel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.messageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageLabel.Location = new System.Drawing.Point(10, 15);
			this.messageLabel.Multiline = true;
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.ReadOnly = true;
			this.messageLabel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.messageLabel.Size = new System.Drawing.Size(795, 120);
			this.messageLabel.TabIndex = 7;
			this.messageLabel.Text = "";
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.Location = new System.Drawing.Point(64, 370);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(237, 21);
			this.progressBar.TabIndex = 12;
			// 
			// logButton
			// 
			this.logButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.logButton.Location = new System.Drawing.Point(404, 370);
			this.logButton.Name = "logButton";
			this.logButton.Size = new System.Drawing.Size(80, 22);
			this.logButton.TabIndex = 36;
			this.logButton.Text = "Log";
			this.logButton.Click += new System.EventHandler(this.logButton_Click);
			// 
			// stopButton
			// 
			this.stopButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.stopButton.Location = new System.Drawing.Point(314, 370);
			this.stopButton.Name = "stopButton";
			this.stopButton.Size = new System.Drawing.Size(81, 22);
			this.stopButton.TabIndex = 35;
			this.stopButton.Text = "Stop";
			this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.setupTabPage,
																					  this.dataTabPage,
																					  this.trackingTabPage,
																					  this.optionsTabPage});
			this.tabControl1.Location = new System.Drawing.Point(5, 5);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(810, 359);
			this.tabControl1.TabIndex = 15;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// setupTabPage
			// 
			this.setupTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.setupPagePanel});
			this.setupTabPage.Location = new System.Drawing.Point(4, 22);
			this.setupTabPage.Name = "setupTabPage";
			this.setupTabPage.Size = new System.Drawing.Size(802, 333);
			this.setupTabPage.TabIndex = 0;
			this.setupTabPage.Text = "Setup and Test";
			// 
			// setupPagePanel
			// 
			this.setupPagePanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.groupBox3,
																						 this.gpsSimulateCheckBox,
																						 this.testButton,
																						 this.toTransferLinkLabel,
																						 this.groupBox2});
			this.setupPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.setupPagePanel.Name = "setupPagePanel";
			this.setupPagePanel.Size = new System.Drawing.Size(802, 333);
			this.setupPagePanel.TabIndex = 15;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label12,
																					this.comboBoxBaud,
																					this.portComboBox,
																					this.label3,
																					this.portSettingsButton});
			this.groupBox3.Location = new System.Drawing.Point(20, 230);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(765, 86);
			this.groupBox3.TabIndex = 14;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Connection";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(324, 33);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(60, 30);
			this.label12.TabIndex = 15;
			this.label12.Text = "Baud:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label12.UseMnemonic = false;
			// 
			// comboBoxBaud
			// 
			this.comboBoxBaud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBaud.Location = new System.Drawing.Point(393, 38);
			this.comboBoxBaud.Name = "comboBoxBaud";
			this.comboBoxBaud.Size = new System.Drawing.Size(155, 21);
			this.comboBoxBaud.TabIndex = 6;
			this.comboBoxBaud.SelectedIndexChanged += new System.EventHandler(this.comboBoxBaud_SelectedIndexChanged);
			// 
			// gpsSimulateCheckBox
			// 
			this.gpsSimulateCheckBox.Location = new System.Drawing.Point(605, 170);
			this.gpsSimulateCheckBox.Name = "gpsSimulateCheckBox";
			this.gpsSimulateCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.gpsSimulateCheckBox.Size = new System.Drawing.Size(163, 37);
			this.gpsSimulateCheckBox.TabIndex = 10;
			this.gpsSimulateCheckBox.Text = "simulate GPS";
			this.gpsSimulateCheckBox.CheckedChanged += new System.EventHandler(this.gpsSimulateCheckBox_CheckedChanged);
			// 
			// toTransferLinkLabel
			// 
			this.toTransferLinkLabel.Location = new System.Drawing.Point(560, 120);
			this.toTransferLinkLabel.Name = "toTransferLinkLabel";
			this.toTransferLinkLabel.Size = new System.Drawing.Size(210, 36);
			this.toTransferLinkLabel.TabIndex = 9;
			this.toTransferLinkLabel.TabStop = true;
			this.toTransferLinkLabel.Text = "more tasks";
			this.toTransferLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.toTransferLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.toTransferLinkLabel_LinkClicked);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.interfaceComboBox,
																					this.makeComboBox,
																					this.modelComboBox,
																					this.label2,
																					this.label4,
																					this.label1});
			this.groupBox2.Location = new System.Drawing.Point(20, 15);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(457, 206);
			this.groupBox2.TabIndex = 13;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "GPS Device";
			// 
			// interfaceComboBox
			// 
			this.interfaceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.interfaceComboBox.Location = new System.Drawing.Point(106, 156);
			this.interfaceComboBox.Name = "interfaceComboBox";
			this.interfaceComboBox.Size = new System.Drawing.Size(331, 21);
			this.interfaceComboBox.TabIndex = 3;
			this.interfaceComboBox.Visible = false;
			this.interfaceComboBox.SelectedIndexChanged += new System.EventHandler(this.interfaceComboBox_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(5, 151);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(87, 35);
			this.label4.TabIndex = 11;
			this.label4.Text = "Interface:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label4.Visible = false;
			// 
			// dataTabPage
			// 
			this.dataTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.dataTabPagePanel});
			this.dataTabPage.Location = new System.Drawing.Point(4, 22);
			this.dataTabPage.Name = "dataTabPage";
			this.dataTabPage.Size = new System.Drawing.Size(802, 333);
			this.dataTabPage.TabIndex = 1;
			this.dataTabPage.Text = "Data Up/Download";
			// 
			// dataTabPagePanel
			// 
			this.dataTabPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataTabPagePanel.Name = "dataTabPagePanel";
			this.dataTabPagePanel.Size = new System.Drawing.Size(802, 333);
			this.dataTabPagePanel.TabIndex = 0;
			// 
			// trackingTabPage
			// 
			this.trackingTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.rtPagePanel});
			this.trackingTabPage.Location = new System.Drawing.Point(4, 22);
			this.trackingTabPage.Name = "trackingTabPage";
			this.trackingTabPage.Size = new System.Drawing.Size(802, 333);
			this.trackingTabPage.TabIndex = 2;
			this.trackingTabPage.Text = "Current Position Tracking";
			// 
			// rtPagePanel
			// 
			this.rtPagePanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.gpsBasicsGroupBox,
																					  this.groupBox5,
																					  this.groupBox4,
																					  this.accuracyLabel,
																					  this.label7,
																					  this.timeLabel,
																					  this.statusLabel,
																					  this.label8,
																					  this.label11,
																					  this.fixLabel,
																					  this.label6});
			this.rtPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtPagePanel.Name = "rtPagePanel";
			this.rtPagePanel.Size = new System.Drawing.Size(802, 333);
			this.rtPagePanel.TabIndex = 37;
			// 
			// gpsBasicsGroupBox
			// 
			this.gpsBasicsGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.positionLabel,
																							this.altLabel,
																							this.trueHeadingLabel,
																							this.label14,
																							this.label10,
																							this.label5,
																							this.magnHeadingLabel,
																							this.slopeLabel,
																							this.label9,
																							this.shrinkMaxButton,
																							this.speedLabel,
																							this.rtStartButton,
																							this.rtStopButton});
			this.gpsBasicsGroupBox.Location = new System.Drawing.Point(5, 5);
			this.gpsBasicsGroupBox.Name = "gpsBasicsGroupBox";
			this.gpsBasicsGroupBox.Size = new System.Drawing.Size(790, 165);
			this.gpsBasicsGroupBox.TabIndex = 44;
			this.gpsBasicsGroupBox.TabStop = false;
			this.gpsBasicsGroupBox.Text = "stopped";
			// 
			// positionLabel
			// 
			this.positionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.positionLabel.Location = new System.Drawing.Point(110, 75);
			this.positionLabel.Name = "positionLabel";
			this.positionLabel.Size = new System.Drawing.Size(380, 45);
			this.positionLabel.TabIndex = 0;
			this.positionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// altLabel
			// 
			this.altLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
			this.altLabel.Location = new System.Drawing.Point(605, 25);
			this.altLabel.Name = "altLabel";
			this.altLabel.Size = new System.Drawing.Size(180, 40);
			this.altLabel.TabIndex = 21;
			this.altLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// trueHeadingLabel
			// 
			this.trueHeadingLabel.Location = new System.Drawing.Point(495, 85);
			this.trueHeadingLabel.Name = "trueHeadingLabel";
			this.trueHeadingLabel.Size = new System.Drawing.Size(290, 25);
			this.trueHeadingLabel.TabIndex = 43;
			this.trueHeadingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(275, 30);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(105, 35);
			this.label14.TabIndex = 42;
			this.label14.Text = "magnetic heading:";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(520, 30);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(75, 35);
			this.label10.TabIndex = 22;
			this.label10.Text = "altitude:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(5, 80);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(94, 34);
			this.label5.TabIndex = 1;
			this.label5.Text = "position:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// magnHeadingLabel
			// 
			this.magnHeadingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
			this.magnHeadingLabel.Location = new System.Drawing.Point(390, 25);
			this.magnHeadingLabel.Name = "magnHeadingLabel";
			this.magnHeadingLabel.Size = new System.Drawing.Size(125, 40);
			this.magnHeadingLabel.TabIndex = 41;
			this.magnHeadingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// slopeLabel
			// 
			this.slopeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.slopeLabel.Location = new System.Drawing.Point(110, 125);
			this.slopeLabel.Name = "slopeLabel";
			this.slopeLabel.Size = new System.Drawing.Size(555, 35);
			this.slopeLabel.TabIndex = 39;
			this.slopeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(20, 30);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(80, 35);
			this.label9.TabIndex = 20;
			this.label9.Text = "speed:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// shrinkMaxButton
			// 
			this.shrinkMaxButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.shrinkMaxButton.Location = new System.Drawing.Point(5, 130);
			this.shrinkMaxButton.Name = "shrinkMaxButton";
			this.shrinkMaxButton.Size = new System.Drawing.Size(37, 22);
			this.shrinkMaxButton.TabIndex = 40;
			this.shrinkMaxButton.Text = "^";
			this.shrinkMaxButton.Click += new System.EventHandler(this.shrinkMaxButton_Click);
			// 
			// speedLabel
			// 
			this.speedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
			this.speedLabel.Location = new System.Drawing.Point(110, 25);
			this.speedLabel.Name = "speedLabel";
			this.speedLabel.Size = new System.Drawing.Size(160, 40);
			this.speedLabel.TabIndex = 19;
			this.speedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// rtStartButton
			// 
			this.rtStartButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.rtStartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.rtStartButton.Location = new System.Drawing.Point(675, 120);
			this.rtStartButton.Name = "rtStartButton";
			this.rtStartButton.Size = new System.Drawing.Size(110, 35);
			this.rtStartButton.TabIndex = 30;
			this.rtStartButton.Text = "Start";
			this.rtStartButton.Click += new System.EventHandler(this.rtStartButton_Click);
			// 
			// rtStopButton
			// 
			this.rtStopButton.Location = new System.Drawing.Point(710, 120);
			this.rtStopButton.Name = "rtStopButton";
			this.rtStopButton.Size = new System.Drawing.Size(75, 35);
			this.rtStopButton.TabIndex = 31;
			this.rtStopButton.Text = "Stop";
			this.rtStopButton.Click += new System.EventHandler(this.rtStopButton_Click);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.trkptCountLabel,
																					this.trackToleranceComboBox,
																					this.trackLogCheckBox});
			this.groupBox5.Location = new System.Drawing.Point(225, 245);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(315, 80);
			this.groupBox5.TabIndex = 38;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Accumulate Track Log";
			// 
			// trkptCountLabel
			// 
			this.trkptCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.trkptCountLabel.Location = new System.Drawing.Point(140, 20);
			this.trkptCountLabel.Name = "trkptCountLabel";
			this.trkptCountLabel.Size = new System.Drawing.Size(167, 25);
			this.trkptCountLabel.TabIndex = 35;
			this.trkptCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// trackToleranceComboBox
			// 
			this.trackToleranceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.trackToleranceComboBox.Items.AddRange(new object[] {
																		"0 m",
																		"1 m",
																		"10 m",
																		"20 m",
																		"50 m",
																		"100 m"});
			this.trackToleranceComboBox.Location = new System.Drawing.Point(15, 50);
			this.trackToleranceComboBox.Name = "trackToleranceComboBox";
			this.trackToleranceComboBox.Size = new System.Drawing.Size(116, 21);
			this.trackToleranceComboBox.TabIndex = 36;
			this.trackToleranceComboBox.SelectedIndexChanged += new System.EventHandler(this.trackToleranceComboBox_SelectedIndexChanged);
			// 
			// trackLogCheckBox
			// 
			this.trackLogCheckBox.Location = new System.Drawing.Point(15, 20);
			this.trackLogCheckBox.Name = "trackLogCheckBox";
			this.trackLogCheckBox.Size = new System.Drawing.Size(116, 25);
			this.trackLogCheckBox.TabIndex = 34;
			this.trackLogCheckBox.Text = "track log";
			this.trackLogCheckBox.CheckedChanged += new System.EventHandler(this.trackLogCheckBox_CheckedChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.keepInViewCheckBox,
																					this.trackOnMapCheckBox});
			this.groupBox4.Location = new System.Drawing.Point(10, 245);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(200, 80);
			this.groupBox4.TabIndex = 37;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Vehicle Icon";
			// 
			// keepInViewCheckBox
			// 
			this.keepInViewCheckBox.Location = new System.Drawing.Point(30, 50);
			this.keepInViewCheckBox.Name = "keepInViewCheckBox";
			this.keepInViewCheckBox.Size = new System.Drawing.Size(153, 25);
			this.keepInViewCheckBox.TabIndex = 33;
			this.keepInViewCheckBox.Text = "keep in view";
			this.keepInViewCheckBox.CheckedChanged += new System.EventHandler(this.keepInViewCheckBox_CheckedChanged);
			// 
			// trackOnMapCheckBox
			// 
			this.trackOnMapCheckBox.Location = new System.Drawing.Point(30, 20);
			this.trackOnMapCheckBox.Name = "trackOnMapCheckBox";
			this.trackOnMapCheckBox.Size = new System.Drawing.Size(153, 25);
			this.trackOnMapCheckBox.TabIndex = 32;
			this.trackOnMapCheckBox.Text = "show on map";
			this.trackOnMapCheckBox.CheckedChanged += new System.EventHandler(this.trackOnMapCheckBox_CheckedChanged);
			// 
			// accuracyLabel
			// 
			this.accuracyLabel.Location = new System.Drawing.Point(115, 185);
			this.accuracyLabel.Name = "accuracyLabel";
			this.accuracyLabel.Size = new System.Drawing.Size(338, 20);
			this.accuracyLabel.TabIndex = 4;
			this.accuracyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(10, 215);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(94, 20);
			this.label7.TabIndex = 18;
			this.label7.Text = "status:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// timeLabel
			// 
			this.timeLabel.Location = new System.Drawing.Point(115, 155);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(338, 20);
			this.timeLabel.TabIndex = 2;
			this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusLabel
			// 
			this.statusLabel.Location = new System.Drawing.Point(115, 215);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(675, 20);
			this.statusLabel.TabIndex = 17;
			this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(10, 185);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(94, 20);
			this.label8.TabIndex = 5;
			this.label8.Text = "accuracy:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(460, 185);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(80, 20);
			this.label11.TabIndex = 24;
			this.label11.Text = "fix:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// fixLabel
			// 
			this.fixLabel.Location = new System.Drawing.Point(550, 185);
			this.fixLabel.Name = "fixLabel";
			this.fixLabel.Size = new System.Drawing.Size(245, 20);
			this.fixLabel.TabIndex = 23;
			this.fixLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(10, 155);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(94, 20);
			this.label6.TabIndex = 3;
			this.label6.Text = "GPS  time:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// optionsTabPage
			// 
			this.optionsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.mgnHandshakeCheckBox,
																						 this.groupBox1,
																						 this.groupBox6});
			this.optionsTabPage.Location = new System.Drawing.Point(4, 22);
			this.optionsTabPage.Name = "optionsTabPage";
			this.optionsTabPage.Size = new System.Drawing.Size(802, 333);
			this.optionsTabPage.TabIndex = 3;
			this.optionsTabPage.Text = "Options";
			// 
			// mgnHandshakeCheckBox
			// 
			this.mgnHandshakeCheckBox.Location = new System.Drawing.Point(55, 270);
			this.mgnHandshakeCheckBox.Name = "mgnHandshakeCheckBox";
			this.mgnHandshakeCheckBox.Size = new System.Drawing.Size(373, 25);
			this.mgnHandshakeCheckBox.TabIndex = 43;
			this.mgnHandshakeCheckBox.Text = "Magellan handshake (much slower)";
			this.mgnHandshakeCheckBox.CheckedChanged += new System.EventHandler(this.mgnHandshakeCheckBox_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.vehicleIconComboBox,
																					this.gpsIconHintLabel});
			this.groupBox1.Location = new System.Drawing.Point(295, 39);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(486, 178);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "GPS Current Position Vehicle Icon";
			// 
			// vehicleIconComboBox
			// 
			this.vehicleIconComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.vehicleIconComboBox.Location = new System.Drawing.Point(90, 39);
			this.vehicleIconComboBox.Name = "vehicleIconComboBox";
			this.vehicleIconComboBox.Size = new System.Drawing.Size(293, 21);
			this.vehicleIconComboBox.TabIndex = 0;
			this.vehicleIconComboBox.SelectedIndexChanged += new System.EventHandler(this.vehicleIconComboBox_SelectedIndexChanged);
			// 
			// gpsIconHintLabel
			// 
			this.gpsIconHintLabel.Location = new System.Drawing.Point(13, 89);
			this.gpsIconHintLabel.Name = "gpsIconHintLabel";
			this.gpsIconHintLabel.Size = new System.Drawing.Size(460, 76);
			this.gpsIconHintLabel.TabIndex = 14;
			this.gpsIconHintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.logErrorsCheckBox,
																					this.logPacketsCheckBox,
																					this.logProtocolCheckBox});
			this.groupBox6.Location = new System.Drawing.Point(29, 36);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(194, 181);
			this.groupBox6.TabIndex = 12;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Log";
			// 
			// logErrorsCheckBox
			// 
			this.logErrorsCheckBox.Location = new System.Drawing.Point(24, 110);
			this.logErrorsCheckBox.Name = "logErrorsCheckBox";
			this.logErrorsCheckBox.Size = new System.Drawing.Size(152, 35);
			this.logErrorsCheckBox.TabIndex = 42;
			this.logErrorsCheckBox.Text = "errors";
			this.logErrorsCheckBox.CheckedChanged += new System.EventHandler(this.logErrorsCheckBox_CheckedChanged);
			// 
			// logPacketsCheckBox
			// 
			this.logPacketsCheckBox.Location = new System.Drawing.Point(24, 72);
			this.logPacketsCheckBox.Name = "logPacketsCheckBox";
			this.logPacketsCheckBox.Size = new System.Drawing.Size(152, 38);
			this.logPacketsCheckBox.TabIndex = 41;
			this.logPacketsCheckBox.Text = "packets";
			this.logPacketsCheckBox.CheckedChanged += new System.EventHandler(this.logPacketsCheckBox_CheckedChanged);
			// 
			// logProtocolCheckBox
			// 
			this.logProtocolCheckBox.Location = new System.Drawing.Point(24, 36);
			this.logProtocolCheckBox.Name = "logProtocolCheckBox";
			this.logProtocolCheckBox.Size = new System.Drawing.Size(152, 36);
			this.logProtocolCheckBox.TabIndex = 40;
			this.logProtocolCheckBox.Text = "protocol";
			this.logProtocolCheckBox.CheckedChanged += new System.EventHandler(this.logProtocolCheckBox_CheckedChanged);
			// 
			// tracksButton
			// 
			this.tracksButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.tracksButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tracksButton.Location = new System.Drawing.Point(570, 370);
			this.tracksButton.Name = "tracksButton";
			this.tracksButton.Size = new System.Drawing.Size(140, 22);
			this.tracksButton.TabIndex = 37;
			this.tracksButton.Text = "Wpt Trk Rte";
			this.tracksButton.Click += new System.EventHandler(this.tracksButton_Click);
			// 
			// progressDetailButton
			// 
			this.progressDetailButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.progressDetailButton.Location = new System.Drawing.Point(13, 370);
			this.progressDetailButton.Name = "progressDetailButton";
			this.progressDetailButton.Size = new System.Drawing.Size(37, 22);
			this.progressDetailButton.TabIndex = 39;
			this.progressDetailButton.Text = "^";
			this.progressDetailButton.Click += new System.EventHandler(this.progressDetailButton_Click);
			// 
			// DlgGpsManager
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(819, 536);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.progressDetailButton,
																		  this.tracksButton,
																		  this.tabControl1,
																		  this.stopButton,
																		  this.logButton,
																		  this.progressBar,
																		  this.progressGroupBox,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.Name = "DlgGpsManager";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "GPS Manager";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgGpsManager_Closing);
			this.Load += new System.EventHandler(this.DlgGpsManager_Load);
			this.Move += new System.EventHandler(this.DlgGpsManager_Move);
			this.progressGroupBox.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.setupTabPage.ResumeLayout(false);
			this.setupPagePanel.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.dataTabPage.ResumeLayout(false);
			this.trackingTabPage.ResumeLayout(false);
			this.rtPagePanel.ResumeLayout(false);
			this.gpsBasicsGroupBox.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.optionsTabPage.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private static bool progressDetailShown = true;
		private bool maxShrinked = false;
		private static int initialWidth = 535;
		private static int initialHeight = 497;
		private static int progressDetailDelta = 0;
		private static int maxShrinkDelta = 0;


		private void sizeTheDialog()
		{
			int delta = 0;

			if(maxShrinked)
			{
				delta = maxShrinkDelta;
			}
			else if(!progressDetailShown)
			{
				delta = progressDetailDelta;
			}

			this.ClientSize = new System.Drawing.Size(initialWidth, initialHeight - delta);
		}

		private void allButtonsDisable()
		{
			testButton.Enabled = false;
			m_gpsButtonPanel.allButtonsDisable();
			stopButton.Enabled = true;
		}

		private void allButtonsEnable()
		{
			testButton.Enabled = true;
			m_gpsButtonPanel.allButtonsEnable();
			stopButton.Enabled = false;
		}

		private string m_message = "";

		private void setMessageText(string text)
		{
			lock(messageLabel)
			{
				m_message = text;
				messageLabel.Text = m_message.Replace("\n", "\r\n");
			}
		}

		private void appendMessageText(string text)
		{
			lock(messageLabel)
			{
				m_message += text;
				messageLabel.Text = m_message.Replace("\n", "\r\n");
			}
		}

		private void doGpsCommand(string command)
		{
			allButtonsDisable();
			progressBar.Visible = true;
			setMessageText("IP: ...command " + command + "\n\n");
			trkptCountLabel.Text = "";

			//if("startRealtime".Equals(command))
			if(!Project.drawVehicles || !Project.drawWaypoints)
			{
				m_pictureManager.LayersManager.ShowVehicles = true;
				Project.drawVehicles = true;
				m_pictureManager.LayersManager.ShowWaypoints = true;
				Project.drawWaypoints = true;
			}

			try 
			{
				m_gps = AllGpsDevices.MakeGpsDriver();
				if(m_gps != null)		// null should never happen here anyway
				{
					if(m_waypoints == null)
					{
						m_waypoints = WaypointsCache.WaypointsAll;
					}

					if(m_routes == null)
					{
						m_routes = WaypointsCache.RoutesAll;
					}

					ThreadGpsControl gt = new ThreadGpsControl(m_gps, m_insertWaypoint, m_waypoints, m_routes);

					gt.MessageCallback  += new GpsMessageHandler( messageCallback );
					gt.ProgressCallback += new GpsProgressHandler( progressCallback );
					gt.CompleteCallback += new GpsCompleteHandler( completeCallback );
					gt.RealTimeCallback += new GpsRealTimeHandler( realtimeCallback );
					gt.RealTimeCallback += new GpsRealTimeHandler( vehicleMovedCallback );
					gt.command = command;

					m_commandThread = new Thread( new System.Threading.ThreadStart(gt.communicate ));
					m_commandThread.Name = "GpsController";
					m_commandThread.IsBackground = true;
					// see Entry.cs for how the current culture is set:
					m_commandThread.CurrentCulture = Thread.CurrentThread.CurrentCulture; //new CultureInfo("en-US", false);
					m_commandThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture; //new CultureInfo("en-US", false);
					m_commandThread.Priority = ThreadPriority.Highest; //.AboveNormal;
					m_started = DateTime.Now;
					m_commandThread.Start();
					Thread.Sleep(0); //Give thread time to start. By documentation, 0 should work, but it does not!
				}
			}
			catch (Exception exc)
			{
				switch(ThreadGpsControl.m_errorAdviceTry++)
				{
					case 0:
						appendMessageText("\n" + exc.Message + "  --  make sure your GPS communication protocol is set to " + AllGpsDevices.AllMakesNames[Project.gpsMakeIndex] + ".");
						break;
					default:
						appendMessageText("\n" + exc.Message);
						break;
				}
			}
		}

		private VehicleGps m_vehicle = null;
 
		private void vehicleMovedCallback(GpsRealTimeData rtData)
		{
			if(rtData.location != null && !closing)
			{
				if(Project.gpsRtTrackLog)
				{
					if(logTrack(rtData))
					{
						trkptCountLabel.Text = "" + totalTrkpt + " points";
					}
				}
				if(trackOnMapCheckBox.Checked)
				{
					if(keepInViewCheckBox.Checked)
					{
						m_cameraManager.keepInView(rtData.location);
					}
					if(m_vehicle == null)
					{
						m_vehicle = new VehicleGps(rtData, "GPS", Project.gpsVehicleIcon, "GPS", Project.WEBSITE_LINK_WEBSTYLE, "", false);
						VehiclesCache.addVehicle(m_vehicle);
					}
					else
					{
						m_vehicle.ProcessMove(rtData);
					}
				}
			}
		}

		#region Track logging -- logTrack()

		private bool trackCreated = false;
		private int totalTrkpt = 0;
		private double lastLat = -1000.0d;
		private double lastLng = -1000.0d;
		private double lastElev = -1000000.0d;
		private GeoCoord lastTrackLoc = null;

		// real-time track logging:
		private bool logTrack(GpsRealTimeData  rtData)
		{
			bool ret = false;

			// only good fixes with non-null location reach here
			string source = "GPS-" + DateTime.Now;

			string trackName = "GPSLOG-" + Project.trackId;

			if(!trackCreated)
			{
				Project.trackId++;

				CreateInfo createInfo = new CreateInfo();

				createInfo.init("trk");
				createInfo.name	= trackName;
				createInfo.id = Project.trackId;
				createInfo.source = source;

				totalTrkpt = 0;

				WaypointsCache.insertWaypoint(createInfo);	// actually inserts a track
				trackCreated = true;
				WaypointsCache.isDirty = true;
			}

			// garmin sends data every second, even if it is same point. Ignore repetetions to save memory.

			double[] howfars = new Double[] { 0.0d, 1.0d, 10.0d, 20.0d, 50.0d, 100.0d };

			double howFarMeters = howfars[trackToleranceComboBox.SelectedIndex];
			GeoCoord loc = new GeoCoord(rtData.location.Lng, rtData.location.Lat, rtData.location.Elev);
			// tolerance 0.0001 degree = 10 meters
			if(trackToleranceComboBox.SelectedIndex == 0 || !loc.almostAs(lastTrackLoc, howFarMeters * 0.00001d))		// 0 means every incoming reading
			//if(rtData.location.Lat != lastLat || rtData.location.Lng != lastLng || rtData.location.Elev != lastElev)
			{
				CreateInfo createInfo = new CreateInfo();

				createInfo.init("trkpt");
				createInfo.id = Project.trackId;			// relate waypoint to track
				createInfo.dateTime = rtData.time;			// always zulu
				createInfo.lat = rtData.location.Lat;
				createInfo.lng = rtData.location.Lng;
				createInfo.elev = rtData.location.Elev;
				createInfo.source = source;
				//createInfo.name = "" + totalTrkpt;		// track point name

				lastLat = rtData.location.Lat;
				lastLng = rtData.location.Lng;
				lastElev = rtData.location.Elev;

				totalTrkpt++;
				ret = true;

				WaypointsCache.insertWaypoint(createInfo);
				lastTrackLoc = loc;
			}
			return ret;
		}
		#endregion

		private GeoCoord lastFixLoc = null;
		private DateTime lastFixDateTime;

		private void realtimeCallback ( GpsRealTimeData rtData )
		{
			if(!closing)
			{
				this.timeLabel.Text = rtData.time.Equals(DateTime.MinValue) ? "" : ("" + Project.zuluToLocal(rtData.time));
				this.fixLabel.Text = rtData.fixStr;
				this.statusLabel.Text = "PC: " + DateTime.Now.ToLongTimeString() + " - " + rtData.comment;
				switch (rtData.fix)
				{
					case 0:
					case 1:
					case -1:
						this.positionLabel.Text = "No GPS fix";
						this.altLabel.Text = "";
						this.accuracyLabel.Text = "";
						this.speedLabel.Text = "";
						this.magnHeadingLabel.Text = "";
						this.trueHeadingLabel.Text = "";
						this.slopeLabel.Text = "";
						lastFixLoc = null;
						gpsBasicsGroupBox.BackColor = Color.Pink;
						gpsBasicsGroupBox.Text = "";
						break;
					default:
						if(rtData.location != null)
						{
							string sLoc = rtData.location.ToString();		// alt not included
							this.positionLabel.Text = sLoc.Replace(" ", "   ");
							//LibSys.StatusBar.Trace2(sLoc);
							Distance dAlt = new Distance(rtData.location.Elev);
							this.altLabel.Text = "" + dAlt.ToString(Distance.UNITS_DISTANCE_FEET);
						} 
						else
						{
							this.positionLabel.Text = "";
							this.altLabel.Text = "";
							//LibSys.StatusBar.Trace2("GPS: no fix");
						}
						Distance dAccH = new Distance(rtData.posErrorH);
						Distance dAccV = new Distance(rtData.posErrorV);
						this.accuracyLabel.Text =
							(rtData.posErrorH > 0 ? (dAccH.ToString(Distance.UNITS_DISTANCE_FEET) + "(Horiz)") : "") + "   "
							+ (rtData.posErrorV > 0 ? (dAccV.ToString(Distance.UNITS_DISTANCE_FEET) + "(Vert)") : "");


						double variation = Math.Round(rtData.location.magneticVariation());
						string variationDir = variation > 0.0d ? "W" : "E";

						bool hasHeading = false;
						double headingTrue = 0.0d;
						double headingMagn = 0.0d;
						string slopeInfo = "";

						if(lastFixLoc != null)
						{
							headingTrue = Math.Round(lastFixLoc.bearing(rtData.location) * 180.0d / Math.PI, 1);
							headingMagn = (headingTrue + variation + 360.0d) % 360.0d;
							hasHeading = true;

							double dAlt = rtData.location.Elev - lastFixLoc.Elev;	// meters
							double dDist = rtData.location.distanceFrom(lastFixLoc).Meters;
							double dSlope = Math.Atan(dAlt / dDist) * 180.0d / Math.PI;
							double dTimeMs = (DateTime.Now - lastFixDateTime).TotalMilliseconds + 1.0d;		// 1.0d to avoid 0
							double fpm = dAlt / dTimeMs / Distance.METERS_PER_FOOT * 1000.0d * 60.0d;
							string fpmFormat = Math.Abs(fpm) > 20.0d ? "1:F0" : "1:F1";

							if(fpm > 1.0d || dSlope >= 0.1d)
							{
								slopeInfo = String.Format("slope: {0:F1}° (up) - climbing at {" + fpmFormat + "} fpm", dSlope, fpm);
							}
							else if(fpm < -1.0d || dSlope <= -0.1d)
							{
								slopeInfo = String.Format("slope: {0:F1}° (down) - descending at {" + fpmFormat + "} fpm", -dSlope, -fpm);
							} 
						}


						Speed dSpeed = new Speed(rtData.velocity * 3600.0d);		// rtData.velocity is in meters/sec, Speed requires meters/hr
						if(dSpeed.Meters < 300)
						{
							this.speedLabel.Text = "0";
							this.magnHeadingLabel.Text = "";
							this.trueHeadingLabel.Text = "";
						}
						else
						{
							this.speedLabel.Text = dSpeed.ToString();
						}

						this.magnHeadingLabel.Text = (hasHeading ? String.Format("{0:000}°", Math.Round(headingMagn)) : "");
						this.trueHeadingLabel.Text = (hasHeading ? String.Format("true heading: {0:000}°", Math.Round(headingTrue)) : "") + String.Format("  variation {0:F0}{1}", Math.Abs(variation), variationDir);

						this.slopeLabel.Text = slopeInfo;

						lastFixLoc = new GeoCoord(rtData.location);
						lastFixDateTime = DateTime.Now;
						gpsBasicsGroupBox.BackColor = Color.LightGreen;
						gpsBasicsGroupBox.Text = "";
						break;
				}
			}
		}

		private void messageCallback ( string message )
		{
			if(!closing)
			{
				appendMessageText("\n\n" + message);
			}
		}

		private static bool doRefresh = false;	// makes things appear on the map after upload operations
		private static bool doRefreshFull = false;	// makes ProcessCameraMove() called after realtime operations
		private static bool mayZoom = false;
		private string message = "";
		private bool isSuccess = false;

		private void completeCallback ( string _message, bool _isSuccess )
		{
			//LibSys.StatusBar.Trace("completeCallback - " + message);

			message = _message;
			isSuccess = _isSuccess;

			try 
			{
				if(this.InvokeRequired)
				{
					//LibSys.StatusBar.Trace("InvokeRequired");
					this.Invoke(new MethodInvoker(_completeCallback));
				} 
				else
				{
					//LibSys.StatusBar.Trace("NOT InvokeRequired");
					_completeCallback();
				}
			}
			catch {}
		}

		// we want this one to run in the dialog's thread.
		private void _completeCallback ()
		{
			//LibSys.StatusBar.Trace("_completeCallback()");

			//string deviceType = m_gps.ToString();
			m_commandThread = null;
			m_gps = null;
			cleanGpsVehicle();

			Project.inhibitRefresh = false;

			TimeSpan duration = DateTime.Now - m_started;
			string sDuration = duration.ToString();
			sDuration = sDuration.Substring(0, Math.Min(sDuration.IndexOf(".")+3, sDuration.Length));
			appendMessageText("\n\n" + message + "   (" + sDuration + ")");
			//if(!closing)
			{
				allButtonsEnable();
				rtStopButton.Visible = false;
				rtStartButton.Visible = false;
			}

			//LibSys.StatusBar.Trace("completeCallback: 1");
			if((closing || isSuccess) && doRefresh && m_pictureManager != null) 
			{
				//LibSys.StatusBar.Trace("completeCallback: 2");
				Cursor.Current = Cursors.WaitCursor;
				Project.drawWaypoints = true;	// force draw - it is frustrating to not see freshly imported track on the map...
				m_pictureManager.LayersManager.ShowWaypoints = true;
				LayerWaypoints.This.init();

				//LibSys.StatusBar.Trace("completeCallback: 3");
				if(Project.gpsZoomIntoTracks && mayZoom)
				{
					try
					{
						m_cameraManager.zoomToCorners();
					}
					catch
					{
					}
				}

				//LibSys.StatusBar.Trace("completeCallback: 4");
				if(doRefreshFull)
				{
					m_cameraManager.ProcessCameraMove();
				}
				else
				{
					m_pictureManager.Refresh();
				}
				Cursor.Current = Cursors.Default;
				doRefresh = doRefreshFull = false;
			}
			mayZoom = false;

//			this.setupTabPage.Enabled = true;
//			this.dataTabPage.Enabled = true;
//			this.optionsTabPage.Enabled = true;

			rtStartButton.Visible = true;
			closeButton.Enabled = true;
			closing = false;

			/*
			if(closing)
			{
				//LibSys.StatusBar.Trace("_completeCallback - in closing");
				closing = false;
				this.closeButton.Enabled = false;
				//this.Close();		// causes interrupted Main after several cycles
			}
			else
			{
				rtStartButton.Visible = true;
			}
			*/
			//LibSys.StatusBar.Trace("completeCallback: end");
		}

		DateTime m_lastProgressCalled = DateTime.Now;

		private void progressCallback ( string message, int unitsSoFar, int unitsTotal )
		{
			if(!closing)
			{
				if(unitsTotal > 0)
				{
					setMessageText(message + " " + unitsSoFar + " of " + unitsTotal);
					if ( unitsSoFar >= 0 && unitsTotal > 10 && unitsSoFar <= unitsTotal )
					{
						progressBar.Visible = true;
						progressBar.Maximum = unitsTotal;
						progressBar.Value = unitsSoFar;
					}
					else
					{
						progressBar.Visible = false;
					}
				}
				else	// we don't know where the end is, but want to show activity
				{
					if((DateTime.Now - m_lastProgressCalled).Milliseconds > 300)
					{
						if ( unitsSoFar >= 0 )
						{
							setMessageText(message + " " + unitsSoFar);
							progressBar.Visible = true;
							progressBar.Maximum = 100;
							progressBar.Value = unitsSoFar % 100;
						}
						else
						{
							setMessageText(message);
							progressBar.Visible = false;
						}
						m_lastProgressCalled = DateTime.Now;
					}
				}
			}
		}

		/// <summary>
		/// a non-kill, set flag type of request to GPS thread, if any.
		/// </summary>
		private void stopRealTimeThread()
		{
			if(m_gps != null)
			{
				// if thread is alive and was doing realtime, it should call CompleteCallback real soon - which will set m_commandThread to null
				m_gps.StopRealTime();
			}
		}

		/// <summary>
		/// we need to kill the thread if a long upload/download is in progress and we click "Stop" or close the dialog
		/// </summary>
		/// <returns></returns>
		private bool killGpsThread()
		{
			bool ret = (m_commandThread != null);

			Thread rtThread = m_commandThread;

			LibSys.StatusBar.Trace("killGpsThread");
			if(rtThread != null)
			{
				try 
				{
					LibSys.StatusBar.Trace("killGpsThread - aborting");
					if(rtThread.IsAlive)
					{
						rtThread.Abort();
					} 
				}
				catch (Exception e)
				{
					LibSys.StatusBar.Error("killGpsThread - while aborting - " + e);
				}
				m_commandThread = null;
				Project.inhibitRefresh = false;
				LibSys.StatusBar.Trace("killGpsThread - finished");
			}
			else
			{
				LibSys.StatusBar.Trace("killGpsThread - no thread to kill");
			}
			return ret;
		}

		private void DlgGpsManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//LibSys.StatusBar.Trace("DlgGpsManager_Closing");
			if(!duplicate)
			{
				this.closeButton.Enabled = false;
				Cursor.Current = Cursors.WaitCursor;

				if(m_commandThread != null)
				{
					//LibSys.StatusBar.Trace("DlgGpsManager_Closing (thread running)");
					closing = true;		// disables all callbacks; make CompleteCallback close dialog
					if(this.rtStopButton.Visible)
					{
						doRefresh = doRefreshFull = true;

						stopRealTimeThread();	// will likely cause completeCallback called, where doRefreshFull will cause ProcesCameraMove()

						//m_cameraManager.ProcessCameraMove();	// make sure that trackpoints are renumbered and end point labeled

//						this.setupTabPage.Enabled = true;
//						this.dataTabPage.Enabled = true;
//						this.optionsTabPage.Enabled = true;

						gpsBasicsGroupBox.BackColor = Color.Yellow;
						gpsBasicsGroupBox.Text = "stopped";

						e.Cancel = true;
						return;				// will have to close in callback
					}
					else
					{
						killGpsThread();
					}
				}

				if(this.makeComboBox.SelectedIndex >= 0)		// may be -1 in some weird dialog-closing phases
				{
					Project.gpsMakeIndex = this.makeComboBox.SelectedIndex;
					Project.gpsModelIndex = this.modelComboBox.SelectedIndex;
					Project.gpsInterfaceIndex = this.interfaceComboBox.SelectedIndex;
					Project.restoreOrDefaultGpsSettings();
				}

				this.dataTabPage.Controls.Clear();		// we don't want static GPS buttons panel to dispose, so we remove it early

				cleanGpsVehicle();

				This = null;
			}
		}

		// IButtonPanelResponder implementation:
		public void buttonPressed(string cmd)
		{
			doRefreshFull = false;
			switch(cmd)
			{
				case "uploadTracks":
					setMessageText("IP: ...getting all tracks at " + Project.gpsPortSettings.baudRate + " baud...\n\n");
					doRefresh = true;
					mayZoom = true;
					WaypointsCache.resetBoundaries();
					doGpsCommand("uploadTracks");
					break;
				case "uploadRoutes":
					setMessageText("IP: ...getting all routes at " + Project.gpsPortSettings.baudRate + " baud...\n\n");
					doRefresh = true;
					mayZoom = true;
					WaypointsCache.resetBoundaries();
					doGpsCommand("uploadRoutes");
					break;
				case "uploadWaypoints":
					setMessageText("IP: ...getting all waypoints at " + Project.gpsPortSettings.baudRate + " baud...\n\n");
					doRefresh = true;
					mayZoom = true;
					WaypointsCache.resetBoundaries();
					doGpsCommand("uploadWaypoints");
					break;
				case "downloadRoutes":
					setMessageText("IP: ...downloading all routes to GPS at " + Project.gpsPortSettings.baudRate + " baud...\n\n");
					doRefresh = false;
					doGpsCommand("downloadRoutes");
					break;
				case "downloadWaypoints":
					setMessageText("IP: ...downloading all waypoints to GPS at " + Project.gpsPortSettings.baudRate + " baud...\n\n");
					doRefresh = false;
					doGpsCommand("downloadWaypoints");
					break;
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(m_commandThread != null)
			{
				tabControl1.SelectedTab = this.trackingTabPage;
				this.stopButton.Focus();
				return;
			}

			if(tabControl1.SelectedTab == this.dataTabPage)
			{
				populateGpsPanel();

				tracksButton.Visible = !this.Modal;	// no show for modal dialogs
			}
			else if(tabControl1.SelectedTab == this.optionsTabPage)
			{
				populateVehicleIcons();
				gpsIconHintLabel.Text = "put your icons in:\n" + Project.GetVehPath("");
				tracksButton.Visible = false;
			}
			else
			{
				tracksButton.Visible = false;
			}

			if(tabControl1.SelectedTab == this.trackingTabPage || tabControl1.SelectedTab == this.optionsTabPage)
			{
				stopButton.Visible = false;
			}
			else
			{
				stopButton.Visible = true;
			}

			if(maxShrinked && tabControl1.SelectedTab != this.trackingTabPage)
			{
				maxShrinked = false;
				sizeTheDialog();
			}
		}

		private void populateGpsPanel()
		{
			this.dataTabPage.SuspendLayout();
			this.dataTabPagePanel.Controls.Clear();

			m_gpsButtonPanel = AllGpsDevices.GetGpsPanel(Project.gpsMakeIndex);
			m_gpsButtonPanel.Responder = this;
			m_gpsButtonPanel.Dock = System.Windows.Forms.DockStyle.Fill;

			this.dataTabPagePanel.Controls.Add(m_gpsButtonPanel);
			this.dataTabPage.ResumeLayout(true);
		}

		private void populateVehicleIcons()
		{
			inSet = true;
			DirectoryInfo di = new DirectoryInfo(Project.GetVehPath(""));

			SortedList icons = new SortedList();
			string icon = "gps_car_default";
			icons.Add(icon, icon);
			icon = "none";
			icons.Add(icon, icon);
			foreach(FileInfo fi in di.GetFiles("*.gif"))
			{
				icon = fi.Name.Substring(0, fi.Name.Length - 4);
				if(!"gps_nofix".Equals(icon) && !"gps_car_default".Equals(icon) && !"none".Equals(icon))
				{
					icons.Add(icon, icon);
				}
			}

			this.vehicleIconComboBox.DataSource = icons.Keys;
			if(!icons.ContainsKey(Project.gpsVehicleIcon))
			{
				Project.gpsVehicleIcon = "gps_car_default";
			}
			this.vehicleIconComboBox.SelectedItem = Project.gpsVehicleIcon;
			inSet = false;
		}
 
		private void vehicleIconComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.gpsVehicleIcon = this.vehicleIconComboBox.SelectedItem.ToString();
			}
		}

		private void cleanGpsVehicle()
		{
			if(m_vehicle != null)
			{
				VehiclesCache.deleteVehicle(m_vehicle);	// takes care of removing it from LayerVehicle
				m_vehicle = null;
			}
		}

		#region clickers

		private void progressDetailButton_Click(object sender, System.EventArgs e)
		{
			progressDetailShown = !progressDetailShown;
			sizeTheDialog();
		}

		private void shrinkMaxButton_Click(object sender, System.EventArgs e)
		{
			maxShrinked = !maxShrinked;
			progressDetailShown = false;
			sizeTheDialog();
		}


		private void toTransferLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			tabControl1.SelectedIndex = 1;	// switch to data transfer page
		}

		private void testButton_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			setMessageText("IP: ...testing connection at " + Project.gpsPortSettings.baudRate + " baud...\n\n");
			doGpsCommand("getDeviceInfo");
			progressDetailShown = true;
			sizeTheDialog();
		}

		private void makeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.gpsMakeIndex = this.makeComboBox.SelectedIndex;

				this.modelComboBox.Items.Clear();
				this.modelComboBox.Items.AddRange(AllGpsDevices.AllModelsNames[this.makeComboBox.SelectedIndex]);
				Project.gpsModelIndex = this.modelComboBox.SelectedIndex = 0;

				this.interfaceComboBox.Items.Clear();
				this.interfaceComboBox.Items.AddRange(AllGpsDevices.AllInterfacesNames[this.makeComboBox.SelectedIndex]);
				Project.gpsInterfaceIndex = this.interfaceComboBox.SelectedIndex = 0;

				if(!Project.restoreOrDefaultGpsSettings())
				{
					Project.gpsPortSettings.baudRate = AllGpsDevices.AllMakesDefaultBaudRates[this.makeComboBox.SelectedIndex];
				}

				this.portComboBox.Text = Project.gpsPortSettings.port;
				this.comboBoxBaud.Text = "" + Project.gpsPortSettings.baudRate;

				ThreadGpsControl.m_errorAdviceTry = 0;
			}
		}

		private void modelComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.gpsModelIndex = this.modelComboBox.SelectedIndex;
			}
		}

		private void interfaceComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.gpsInterfaceIndex = this.interfaceComboBox.SelectedIndex;
			}
		}

		private void portSettingsButton_Click(object sender, System.EventArgs e)
		{
			new DlgPortSettings().ShowDialog();
		}

		private void logButton_Click(object sender, System.EventArgs e)
		{
			LibSys.StatusBar.showLog();
		}

		private void stopButton_Click(object sender, System.EventArgs e)
		{
			doRefresh = doRefreshFull = false;
			
			killGpsThread();

			allButtonsEnable();
			cleanGpsVehicle();
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			// see DlgGpsManager_Closing() for processing logic
			Close();
		}

		private void tracksButton_Click(object sender, System.EventArgs e)
		{
			new DlgWaypointsManager(m_pictureManager, m_cameraManager, 0, -1).Show();
		}

		private void logProtocolCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsLogProtocol = logProtocolCheckBox.Checked;
			logButton.Visible = true;
		}

		private void logPacketsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsLogPackets = logPacketsCheckBox.Checked;
			logButton.Visible = true;
		}

		private void logErrorsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsLogErrors = logErrorsCheckBox.Checked;
			logButton.Visible = true;
		}

		private void rtStartButton_Click(object sender, System.EventArgs e)
		{
			setMessageText("IP: ...getting real time position at " + Project.gpsPortSettings.baudRate + " baud...\n\n");

//			this.setupTabPage.Enabled = false;
//			this.dataTabPage.Enabled = false;
//			this.optionsTabPage.Enabled = false;

			closing = false;

			rtStopButton.Visible = true;
			rtStartButton.Visible = false;

			trackCreated = false;
			totalTrkpt = 0;
			lastLat = -1000.0d;
			lastLng = -1000.0d;
			lastElev = -1000000.0d;
			lastFixLoc = null;

			doGpsCommand("startRealtime");
		}

		private void rtStopButton_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			rtStopButton.Visible = false;

			gpsBasicsGroupBox.BackColor = Color.Yellow;
			gpsBasicsGroupBox.Text = "stopped";

			try 
			{
				doRefresh = doRefreshFull = (m_commandThread != null);
			
				stopRealTimeThread();

//				this.setupTabPage.Enabled = true;
//				this.dataTabPage.Enabled = true;
//				this.optionsTabPage.Enabled = true;

				// the following will happen in completeCallback:
				//cleanVehicles();
				//m_cameraManager.ProcessCameraMove();	// re-number the trackpoints and make sure the End label shows up
			}
			catch (Exception ee)
			{
				setMessageText("Error: " + ee.Message);
			}
			//rtStartButton.Visible = true;		// see completeCallback()
		}

		private void trackOnMapCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsRtTrackOnMap = trackOnMapCheckBox.Checked;

			keepInViewCheckBox.Enabled = trackOnMapCheckBox.Checked;
			//trackLogCheckBox.Enabled = trackOnMapCheckBox.Checked;

			if(!trackOnMapCheckBox.Checked)
			{
				cleanGpsVehicle();
			}
		}

		private void keepInViewCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsRtKeepInView = keepInViewCheckBox.Checked;
		}

		private void trackLogCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsRtTrackLog = trackLogCheckBox.Checked;
			trackToleranceComboBox.Enabled = trackLogCheckBox.Checked;
			trkptCountLabel.Visible = trackLogCheckBox.Checked;
		}

		private void trackToleranceComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Project.gpsRtToleranceIndex = trackToleranceComboBox.SelectedIndex;
		}

		private void gpsSimulateCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsSimulate = gpsSimulateCheckBox.Checked;
		}

		private void portComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.gpsPortSettings.port = this.portComboBox.Text;
				Project.savePortSettings();
			}
		}

		private void comboBoxBaud_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(!inSet)
			{
				Project.gpsPortSettings.baudRate = Convert.ToInt32(this.comboBoxBaud.Text);
				Project.savePortSettings();
			}
		}
		#endregion

		private void mgnHandshakeCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.gpsMagellanHandshake = mgnHandshakeCheckBox.Checked;
		}

		private void DlgGpsManager_Move(object sender, System.EventArgs e)
		{
			memorizePosition();
		}

		private void memorizePosition()
		{
			if(!inResize)
			{
				gpsManagerX = this.Location.X;
				gpsManagerY = this.Location.Y;
			}
		}

	}
}
