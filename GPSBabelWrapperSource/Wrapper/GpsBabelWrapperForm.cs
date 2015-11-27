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
		 
using GpsBabelCmd;
using GpsBabelGUI;
using LibSys;
using LibNet;

namespace Wrapper
{
	/// <summary>
	/// Summary description for GpsBabelWrapper.
	/// </summary>
	public class GpsBabelWrapperForm : System.Windows.Forms.Form
	{
		private static string m_warning = "Use at your own risk. Do not use while driving.\nData quality or trouble-free operation not guaranteed.";
//		private static string m_greeting = "GpsBabel Wrapper";
//		private static Thread m_greetingThread = null;

		// processing dropped files on the icon (which end up in args):
		private static string[] Args;

		private GpsBabelCommand m_gbc;
		private InputControl m_inputControl;
		private OutputControl m_outputControl;
		private FiltersPane m_filtersPane;

		private bool keystrokeProcessed;
		
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label statusBar;
		private System.Windows.Forms.Label statusBar2;
		private System.Windows.Forms.MenuItem aboutMenuItem;
		private System.Windows.Forms.ProgressBar mainProgressBar;
		private System.Windows.Forms.MenuItem fileMainMenuItem;
		private System.Windows.Forms.TabControl mainTabControl;
		private System.Windows.Forms.TabPage actionTabPage;
		private System.Windows.Forms.TabPage optionsTabPage;
		private System.Windows.Forms.TextBox cmdTextBox;
		private System.Windows.Forms.Button executeButton;
		private System.Windows.Forms.MenuItem helpMenuItem;
		private System.Windows.Forms.MenuItem fileOpenMenuItem;
		private System.Windows.Forms.GroupBox inputGroupBox;
		private System.Windows.Forms.GroupBox outputGroupBox;
		private System.Windows.Forms.GroupBox resultGroupBox;
		private System.Windows.Forms.Label resultsLabel;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.MenuItem hMenuItem;
		private System.Windows.Forms.Panel filterPanel;
		private System.Windows.Forms.TabPage filterTabPage;
		private System.Windows.Forms.CheckBox wptCheckBox;
		private System.Windows.Forms.CheckBox trkCheckBox;
		private System.Windows.Forms.CheckBox rteCheckBox;
		private System.Windows.Forms.TextBox adHocTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel moreFilterLinkLabel;
		private System.Windows.Forms.MenuItem helpGBHelpMenuItem;
		private System.Windows.Forms.MenuItem helpGBHomeMenuItem;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label exeLabel;
		private System.Windows.Forms.TextBox executableTextBox;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button browseQmButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox executableQmTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown qmStartDelay;
		private System.Windows.Forms.CheckBox shortnamesCheckBox;
		private System.Windows.Forms.Panel actionBottomPanel;
		private System.Windows.Forms.Panel actionFillPanel;
		private System.Windows.Forms.Panel filterTabPanel;
		private System.Windows.Forms.Panel fillPanel;
		private System.Windows.Forms.MenuItem fileSaveAsMenuItem;
		private System.Windows.Forms.MenuItem updateMenuItem;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem helpOptionsMenuItem;
		private System.Windows.Forms.MenuItem helpReportBugMenuItem;
		private System.Windows.Forms.LinkLabel reportBugLinkLabel;
		private System.Windows.Forms.MenuItem exitMenuItem;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GpsBabelWrapperForm(string[] args)		// args will contain list of files dragged onto QM icon when starting the program
		{
			LibSys.StatusBar.WriteLine("Starting GUI");
			Project.mainForm = this;		// Project.MessageBox needs this one (TODO:check this....)
			Args = (string[])args.Clone();	// we need to null out some elements as we process them

			Project.serverAvailable = true;
			ProgressMonitor.CountActive();		// just to have this class loaded and working.

//			LibSys.StatusBar.WriteLine("Greeting");
//			if(m_greeting.Length > 0)
//			{
//				m_greeting = m_greeting + "\n\n...initializing, please wait...\n...framework " + Environment.Version;
//				// display greeting in a separate thread, and proceed with initializing:
//				m_greetingThread =	new Thread( new ThreadStart(ShowGreeting));
//				m_greetingThread.IsBackground = true;	// terminate with the main process
//				m_greetingThread.Name = "Greeting";
//				m_greetingThread.Start();
//			}

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.Text = Project.PROGRAM_NAME_HUMAN;
			this.aboutMenuItem.Text = "&About " + Project.PROGRAM_NAME_HUMAN;

			LibSys.StatusBar.setInterface(statusBar, statusBar2, mainProgressBar);
			LibSys.StatusBar.WriteLine("GUI up");

			Project.recentFilesFirstIndex = this.fileMainMenuItem.MenuItems.Count;

			this.Text = Project.PROGRAM_NAME_HUMAN;

			Project.ReadOptions();

			qmStartDelay.Value = Project.QuakeMapStartupDelay;
			
			if(Project.GpsBabelExecutable.Length == 0)
			{
				Project.resetGpsBabelExecutable();
			}

			executableTextBox.Text = Project.GpsBabelExecutable;

			if(Project.QuakeMapExecutable.Length == 0)
			{
				Project.resetQuakeMapExecutable();
			}

			executableQmTextBox.Text = Project.QuakeMapExecutable;

			m_gbc = new GpsBabelCommand();
			m_inputControl = new InputControl();
			m_outputControl = new OutputControl();
			m_filtersPane = new FiltersPane();

			m_inputControl.formatDoc = m_gbc.GetHelpXml();
			m_outputControl.formatDoc = m_gbc.GetHelpXml();
			m_filtersPane.filterDoc = m_gbc.GetHelpXml();

            if (Project.firstRun)
            {
                m_outputControl.selectQuakeMapMode();
                string sampleFile = Path.Combine(Project.startupPath, "dana-point.gpx");
                if (File.Exists(sampleFile))
                {
                    m_inputControl.InputFileName = sampleFile;
                }
            }

			m_inputControl.Dock = DockStyle.Fill;
			inputGroupBox.Controls.Add(m_inputControl);
			m_inputControl.selectionChanged += new EventHandler(onInputSelectionChanged);
			m_inputControl.gpsSelectionChanged += new EventHandler(onInputGpsSelectionChanged);

			m_outputControl.Dock = DockStyle.Fill;
			outputGroupBox.Controls.Add(m_outputControl);
			m_outputControl.selectionChanged += new EventHandler(onOutputSelectionChanged);
			m_outputControl.gpsSelectionChanged += new EventHandler(onOutputGpsSelectionChanged);

			//m_filtersPane.Dock = DockStyle.Fill;
			filterTabPanel.Controls.Add(m_filtersPane);
			m_filtersPane.selectionChanged += new EventHandler(onFilterSelectionChanged);

			// good place to have camera coordinates set here, if supplied in command line:
			//        /lat=34.123 /lon=-117.234 /elev=5000 /map=aerial (topo,color)
			processArgs();

			// we need it as early as possible, before first web load - WebsiteInfo.init(Project.CGIBINPTR_URL) below - occured.
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
				//this.Size = new System.Drawing.Size(Project.mainFormWidth, Project.mainFormHeight);		// causes Resize()
//				this.Width = Project.mainFormWidth;
//				this.Height = Project.mainFormHeight;
				//this.Bounds = new System.Drawing.Rectangle(Project.mainFormX, Project.mainFormY, Project.mainFormWidth, Project.mainFormHeight);
			}

			WebsiteInfo.init(Project.CGIBINPTR_URL);		// try to reach the QuakeMap server

			if(WebsiteInfo.HasReachedServer)
			{
				setInternetAvailable(true);

//				string message = "\nReached server: " + Project.PROGRAM_MAIN_SERVER	+ "\n ";
//				Project.MessageBox(null, message);
			}
			else
			{
				setInternetAvailable(false);

//				string message = "\nWarning: couldn't reach server: " + Project.PROGRAM_MAIN_SERVER	+ "\n\nWorking offline.\n ";
//				Project.ErrorBox(null, message);
			}

			if(Project.upgradeMessage.Length > 0)
			{
				new DlgUpgradeMessage(Project.upgradeMessage).ShowDialog();
			}

			LibSys.StatusBar.WriteLine("Init");
			//add dt worker method to the thread pool / queue a task 
			//Project.threadPool.PostRequest (new WorkRequestDelegate (runInit), "MainForm init"); 
			ThreadPool2.QueueUserWorkItem(new WaitCallback (runInit), "MainForm init"); 

			periodicMaintenanceTimer.Interval = 1000;
			periodicMaintenanceTimer.Tick += new EventHandler(periodicMaintenance);
			periodicMaintenanceTimer.Start();

			this.fillCommandObject(m_gbc);
			m_inputControl.fillCommandObject(m_gbc);
			m_outputControl.fillCommandObject(m_gbc);
			reEvaluate();

#if DEBUG
			try
			{
				string fileName = Project.GetMiscPath("gbhelp.xml");
				m_gbc.GetHelpXml().Save(fileName);

//				Project.RunBrowser(fileName);
			} 
			catch {}
#endif

			inResize = false;
		}

		#region Recent Files Menu related

		public void rebuildRecentFilesMenu()
		{
			// clean out existing menu items:
			for(int i=fileMainMenuItem.MenuItems.Count-1; i >= Project.recentFilesFirstIndex ;i--)
			{
				fileMainMenuItem.MenuItems.RemoveAt(i);
			}

			if(Project.recentFiles.Count > 0)
			{
				MenuItem menuItem = new MenuItem();

				menuItem.Text = "-";
				menuItem.Index = fileMainMenuItem.MenuItems.Count;

				fileMainMenuItem.MenuItems.Add(menuItem);
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
				menuItem.Index = fileMainMenuItem.MenuItems.Count;

				fileMainMenuItem.MenuItems.Add(menuItem);
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
			Project.ShowPopup(this.mainProgressBar, " \n... processing " + recentFilePath + " \n", Point.Empty);
		}

		//private void processRecentFileItemClick(object state, DateTime requestEnqueueTime)
		private void processRecentFileItemClick(object state)
		{
			try 
			{
				LibSys.StatusBar.Trace("* processing " + recentFilePath);

				string[] fileNames = new string[] { recentFilePath };

				//bool anySuccess = FileAndZipIO.readFiles(fileNames);

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
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.fileMainMenuItem = new System.Windows.Forms.MenuItem();
			this.fileOpenMenuItem = new System.Windows.Forms.MenuItem();
			this.fileSaveAsMenuItem = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.hMenuItem = new System.Windows.Forms.MenuItem();
			this.helpMenuItem = new System.Windows.Forms.MenuItem();
			this.aboutMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.helpGBHomeMenuItem = new System.Windows.Forms.MenuItem();
			this.helpGBHelpMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.updateMenuItem = new System.Windows.Forms.MenuItem();
			this.helpReportBugMenuItem = new System.Windows.Forms.MenuItem();
			this.helpOptionsMenuItem = new System.Windows.Forms.MenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.mainProgressBar = new System.Windows.Forms.ProgressBar();
			this.statusBar2 = new System.Windows.Forms.Label();
			this.statusBar = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.fillPanel = new System.Windows.Forms.Panel();
			this.mainTabControl = new System.Windows.Forms.TabControl();
			this.actionTabPage = new System.Windows.Forms.TabPage();
			this.actionFillPanel = new System.Windows.Forms.Panel();
			this.inputGroupBox = new System.Windows.Forms.GroupBox();
			this.filterPanel = new System.Windows.Forms.Panel();
			this.shortnamesCheckBox = new System.Windows.Forms.CheckBox();
			this.moreFilterLinkLabel = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.adHocTextBox = new System.Windows.Forms.TextBox();
			this.rteCheckBox = new System.Windows.Forms.CheckBox();
			this.trkCheckBox = new System.Windows.Forms.CheckBox();
			this.wptCheckBox = new System.Windows.Forms.CheckBox();
			this.outputGroupBox = new System.Windows.Forms.GroupBox();
			this.resultGroupBox = new System.Windows.Forms.GroupBox();
			this.panel4 = new System.Windows.Forms.Panel();
			this.resultsLabel = new System.Windows.Forms.Label();
			this.filterTabPage = new System.Windows.Forms.TabPage();
			this.filterTabPanel = new System.Windows.Forms.Panel();
			this.optionsTabPage = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.qmStartDelay = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.browseQmButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.executableQmTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.exeLabel = new System.Windows.Forms.Label();
			this.executableTextBox = new System.Windows.Forms.TextBox();
			this.actionBottomPanel = new System.Windows.Forms.Panel();
			this.reportBugLinkLabel = new System.Windows.Forms.LinkLabel();
			this.executeButton = new System.Windows.Forms.Button();
			this.cmdTextBox = new System.Windows.Forms.TextBox();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.fillPanel.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.actionTabPage.SuspendLayout();
			this.actionFillPanel.SuspendLayout();
			this.filterPanel.SuspendLayout();
			this.resultGroupBox.SuspendLayout();
			this.panel4.SuspendLayout();
			this.filterTabPage.SuspendLayout();
			this.optionsTabPage.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.qmStartDelay)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.actionBottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.fileMainMenuItem,
																					  this.hMenuItem});
			// 
			// fileMainMenuItem
			// 
			this.fileMainMenuItem.Index = 0;
			this.fileMainMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							 this.fileOpenMenuItem,
																							 this.fileSaveAsMenuItem,
																							 this.exitMenuItem});
			this.fileMainMenuItem.Text = "File";
			this.fileMainMenuItem.Popup += new System.EventHandler(this.fileMainMenuItem_Popup);
			// 
			// fileOpenMenuItem
			// 
			this.fileOpenMenuItem.Index = 0;
			this.fileOpenMenuItem.Text = "&Open...      [Input]";
			this.fileOpenMenuItem.Click += new System.EventHandler(this.fileOpenMenuItem_Click);
			// 
			// fileSaveAsMenuItem
			// 
			this.fileSaveAsMenuItem.Index = 1;
			this.fileSaveAsMenuItem.Text = "&Save As...  [Output]";
			this.fileSaveAsMenuItem.Click += new System.EventHandler(this.fileSaveAsMenuItem_Click);
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 2;
			this.exitMenuItem.Text = "&Exit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			// 
			// hMenuItem
			// 
			this.hMenuItem.Index = 1;
			this.hMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.helpMenuItem,
																					  this.aboutMenuItem,
																					  this.menuItem1,
																					  this.helpGBHomeMenuItem,
																					  this.helpGBHelpMenuItem,
																					  this.menuItem2,
																					  this.updateMenuItem,
																					  this.helpReportBugMenuItem,
																					  this.helpOptionsMenuItem});
			this.hMenuItem.Text = "Help";
			// 
			// helpMenuItem
			// 
			this.helpMenuItem.Index = 0;
			this.helpMenuItem.Text = "&Help (GPSBabelWrapper User Manual)";
			this.helpMenuItem.Click += new System.EventHandler(this.helpMenuItem_Click);
			// 
			// aboutMenuItem
			// 
			this.aboutMenuItem.Index = 1;
			this.aboutMenuItem.Text = "&About...";
			this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 2;
			this.menuItem1.Text = "-";
			// 
			// helpGBHomeMenuItem
			// 
			this.helpGBHomeMenuItem.Index = 3;
			this.helpGBHomeMenuItem.Text = "GPSBabel Home";
			this.helpGBHomeMenuItem.Click += new System.EventHandler(this.helpGBHomeMenuItem_Click);
			// 
			// helpGBHelpMenuItem
			// 
			this.helpGBHelpMenuItem.Index = 4;
			this.helpGBHelpMenuItem.Text = "GPSBabel Command Line Help";
			this.helpGBHelpMenuItem.Click += new System.EventHandler(this.helpGBHelpMenuItem_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 5;
			this.menuItem2.Text = "-";
			// 
			// updateMenuItem
			// 
			this.updateMenuItem.Index = 6;
			this.updateMenuItem.Text = "Check for &Update";
			this.updateMenuItem.Click += new System.EventHandler(this.updateMenuItem_Click);
			// 
			// helpReportBugMenuItem
			// 
			this.helpReportBugMenuItem.Index = 7;
			this.helpReportBugMenuItem.Text = "Report Bug";
			this.helpReportBugMenuItem.Click += new System.EventHandler(this.helpReportBugMenuItem_Click);
			// 
			// helpOptionsMenuItem
			// 
			this.helpOptionsMenuItem.Index = 8;
			this.helpOptionsMenuItem.Text = "&Options...";
			this.helpOptionsMenuItem.Click += new System.EventHandler(this.helpOptionsMenuItem_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.mainProgressBar,
																				 this.statusBar2,
																				 this.statusBar});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 485);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(752, 25);
			this.panel1.TabIndex = 0;
			// 
			// mainProgressBar
			// 
			this.mainProgressBar.Location = new System.Drawing.Point(5, 2);
			this.mainProgressBar.Name = "mainProgressBar";
			this.mainProgressBar.Size = new System.Drawing.Size(85, 20);
			this.mainProgressBar.TabIndex = 3;
			// 
			// statusBar2
			// 
			this.statusBar2.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.statusBar2.Location = new System.Drawing.Point(725, 5);
			this.statusBar2.Name = "statusBar2";
			this.statusBar2.Size = new System.Drawing.Size(20, 14);
			this.statusBar2.TabIndex = 2;
			this.statusBar2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.statusBar2.Visible = false;
			// 
			// statusBar
			// 
			this.statusBar.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.statusBar.Cursor = System.Windows.Forms.Cursors.Hand;
			this.statusBar.Location = new System.Drawing.Point(90, 5);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(655, 13);
			this.statusBar.TabIndex = 6;
			this.statusBar.Text = "Ready";
			this.statusBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel2
			// 
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.fillPanel,
																				 this.actionBottomPanel});
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(752, 485);
			this.panel2.TabIndex = 1;
			// 
			// fillPanel
			// 
			this.fillPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.mainTabControl});
			this.fillPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fillPanel.Name = "fillPanel";
			this.fillPanel.Size = new System.Drawing.Size(752, 385);
			this.fillPanel.TabIndex = 3;
			// 
			// mainTabControl
			// 
			this.mainTabControl.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.actionTabPage,
																						 this.filterTabPage,
																						 this.optionsTabPage});
			this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTabControl.Name = "mainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = new System.Drawing.Size(752, 385);
			this.mainTabControl.TabIndex = 0;
			// 
			// actionTabPage
			// 
			this.actionTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.actionFillPanel});
			this.actionTabPage.Location = new System.Drawing.Point(4, 22);
			this.actionTabPage.Name = "actionTabPage";
			this.actionTabPage.Size = new System.Drawing.Size(744, 359);
			this.actionTabPage.TabIndex = 0;
			this.actionTabPage.Text = "Action";
			// 
			// actionFillPanel
			// 
			this.actionFillPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.inputGroupBox,
																						  this.filterPanel,
																						  this.outputGroupBox,
																						  this.resultGroupBox});
			this.actionFillPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.actionFillPanel.Name = "actionFillPanel";
			this.actionFillPanel.Size = new System.Drawing.Size(744, 359);
			this.actionFillPanel.TabIndex = 5;
			// 
			// inputGroupBox
			// 
			this.inputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.inputGroupBox.Location = new System.Drawing.Point(0, 5);
			this.inputGroupBox.Name = "inputGroupBox";
			this.inputGroupBox.Size = new System.Drawing.Size(745, 100);
			this.inputGroupBox.TabIndex = 1;
			this.inputGroupBox.TabStop = false;
			this.inputGroupBox.Text = "Input";
			// 
			// filterPanel
			// 
			this.filterPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.filterPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.filterPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.shortnamesCheckBox,
																					  this.moreFilterLinkLabel,
																					  this.label1,
																					  this.adHocTextBox,
																					  this.rteCheckBox,
																					  this.trkCheckBox,
																					  this.wptCheckBox});
			this.filterPanel.Location = new System.Drawing.Point(0, 110);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = new System.Drawing.Size(745, 30);
			this.filterPanel.TabIndex = 2;
			// 
			// shortnamesCheckBox
			// 
			this.shortnamesCheckBox.Location = new System.Drawing.Point(240, 5);
			this.shortnamesCheckBox.Name = "shortnamesCheckBox";
			this.shortnamesCheckBox.Size = new System.Drawing.Size(110, 20);
			this.shortnamesCheckBox.TabIndex = 7;
			this.shortnamesCheckBox.Text = "synt.shortnames";
			this.shortnamesCheckBox.CheckedChanged += new System.EventHandler(this.filterMainOptionsChanged);
			// 
			// moreFilterLinkLabel
			// 
			this.moreFilterLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.moreFilterLinkLabel.Location = new System.Drawing.Point(675, 5);
			this.moreFilterLinkLabel.Name = "moreFilterLinkLabel";
			this.moreFilterLinkLabel.Size = new System.Drawing.Size(100, 20);
			this.moreFilterLinkLabel.TabIndex = 9;
			this.moreFilterLinkLabel.TabStop = true;
			this.moreFilterLinkLabel.Text = "more filter...";
			this.moreFilterLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.moreFilterLinkLabel.VisitedLinkColor = System.Drawing.Color.Blue;
			this.moreFilterLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.moreFilterLinkLabel_LinkClicked);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(435, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 20);
			this.label1.TabIndex = 8;
			this.label1.Text = "ad-hoc:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// adHocTextBox
			// 
			this.adHocTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.adHocTextBox.Location = new System.Drawing.Point(480, 5);
			this.adHocTextBox.Name = "adHocTextBox";
			this.adHocTextBox.Size = new System.Drawing.Size(165, 20);
			this.adHocTextBox.TabIndex = 8;
			this.adHocTextBox.Text = "";
			this.adHocTextBox.TextChanged += new System.EventHandler(this.filterMainOptionsChanged);
			// 
			// rteCheckBox
			// 
			this.rteCheckBox.Location = new System.Drawing.Point(165, 5);
			this.rteCheckBox.Name = "rteCheckBox";
			this.rteCheckBox.Size = new System.Drawing.Size(60, 20);
			this.rteCheckBox.TabIndex = 6;
			this.rteCheckBox.Text = "routes";
			this.rteCheckBox.CheckedChanged += new System.EventHandler(this.filterMainOptionsChanged);
			// 
			// trkCheckBox
			// 
			this.trkCheckBox.Location = new System.Drawing.Point(100, 5);
			this.trkCheckBox.Name = "trkCheckBox";
			this.trkCheckBox.Size = new System.Drawing.Size(60, 20);
			this.trkCheckBox.TabIndex = 5;
			this.trkCheckBox.Text = "tracks";
			this.trkCheckBox.CheckedChanged += new System.EventHandler(this.filterMainOptionsChanged);
			// 
			// wptCheckBox
			// 
			this.wptCheckBox.Checked = true;
			this.wptCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.wptCheckBox.Location = new System.Drawing.Point(10, 5);
			this.wptCheckBox.Name = "wptCheckBox";
			this.wptCheckBox.Size = new System.Drawing.Size(80, 20);
			this.wptCheckBox.TabIndex = 4;
			this.wptCheckBox.Text = "waypoints";
			this.wptCheckBox.CheckedChanged += new System.EventHandler(this.filterMainOptionsChanged);
			// 
			// outputGroupBox
			// 
			this.outputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.outputGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.outputGroupBox.Location = new System.Drawing.Point(0, 145);
			this.outputGroupBox.Name = "outputGroupBox";
			this.outputGroupBox.Size = new System.Drawing.Size(745, 100);
			this.outputGroupBox.TabIndex = 3;
			this.outputGroupBox.TabStop = false;
			this.outputGroupBox.Text = "Output";
			// 
			// resultGroupBox
			// 
			this.resultGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.resultGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.panel4});
			this.resultGroupBox.Location = new System.Drawing.Point(0, 250);
			this.resultGroupBox.Name = "resultGroupBox";
			this.resultGroupBox.Size = new System.Drawing.Size(745, 110);
			this.resultGroupBox.TabIndex = 2;
			this.resultGroupBox.TabStop = false;
			this.resultGroupBox.Text = "Result";
			// 
			// panel4
			// 
			this.panel4.AutoScroll = true;
			this.panel4.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.resultsLabel});
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new System.Drawing.Point(3, 16);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(739, 91);
			this.panel4.TabIndex = 1;
			// 
			// resultsLabel
			// 
			this.resultsLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.resultsLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.resultsLabel.Name = "resultsLabel";
			this.resultsLabel.Size = new System.Drawing.Size(715, 20);
			this.resultsLabel.TabIndex = 0;
			this.resultsLabel.Click += new System.EventHandler(this.resultsLabel_Click);
			this.resultsLabel.TextChanged += new System.EventHandler(this.resultsLabel_TextChanged);
			// 
			// filterTabPage
			// 
			this.filterTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.filterTabPanel});
			this.filterTabPage.Location = new System.Drawing.Point(4, 22);
			this.filterTabPage.Name = "filterTabPage";
			this.filterTabPage.Size = new System.Drawing.Size(744, 359);
			this.filterTabPage.TabIndex = 2;
			this.filterTabPage.Text = "Filters";
			// 
			// filterTabPanel
			// 
			this.filterTabPanel.AutoScroll = true;
			this.filterTabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterTabPanel.Name = "filterTabPanel";
			this.filterTabPanel.Size = new System.Drawing.Size(744, 359);
			this.filterTabPanel.TabIndex = 0;
			// 
			// optionsTabPage
			// 
			this.optionsTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.groupBox2,
																						 this.groupBox1});
			this.optionsTabPage.Location = new System.Drawing.Point(4, 22);
			this.optionsTabPage.Name = "optionsTabPage";
			this.optionsTabPage.Size = new System.Drawing.Size(744, 359);
			this.optionsTabPage.TabIndex = 1;
			this.optionsTabPage.Text = "Options";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label4,
																					this.qmStartDelay,
																					this.label3,
																					this.browseQmButton,
																					this.label2,
																					this.executableQmTextBox});
			this.groupBox2.Location = new System.Drawing.Point(15, 105);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(480, 90);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "QuakeMap Executable Location";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(140, 55);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(70, 20);
			this.label4.TabIndex = 12;
			this.label4.Text = "seconds";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// qmStartDelay
			// 
			this.qmStartDelay.Location = new System.Drawing.Point(90, 55);
			this.qmStartDelay.Maximum = new System.Decimal(new int[] {
																		 20,
																		 0,
																		 0,
																		 0});
			this.qmStartDelay.Minimum = new System.Decimal(new int[] {
																		 2,
																		 0,
																		 0,
																		 0});
			this.qmStartDelay.Name = "qmStartDelay";
			this.qmStartDelay.Size = new System.Drawing.Size(45, 20);
			this.qmStartDelay.TabIndex = 11;
			this.qmStartDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.qmStartDelay.Value = new System.Decimal(new int[] {
																	   2,
																	   0,
																	   0,
																	   0});
			this.qmStartDelay.ValueChanged += new System.EventHandler(this.qmStartDelay_ValueChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(15, 55);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(70, 20);
			this.label3.TabIndex = 10;
			this.label3.Text = "Start delay:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// browseQmButton
			// 
			this.browseQmButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.browseQmButton.Location = new System.Drawing.Point(440, 25);
			this.browseQmButton.Name = "browseQmButton";
			this.browseQmButton.Size = new System.Drawing.Size(24, 20);
			this.browseQmButton.TabIndex = 9;
			this.browseQmButton.Text = "...";
			this.browseQmButton.Click += new System.EventHandler(this.browseQmButton_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(5, 25);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 20);
			this.label2.TabIndex = 5;
			this.label2.Text = "Executable:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// executableQmTextBox
			// 
			this.executableQmTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.executableQmTextBox.Location = new System.Drawing.Point(90, 25);
			this.executableQmTextBox.Name = "executableQmTextBox";
			this.executableQmTextBox.Size = new System.Drawing.Size(340, 20);
			this.executableQmTextBox.TabIndex = 4;
			this.executableQmTextBox.Text = "";
			this.executableQmTextBox.TextChanged += new System.EventHandler(this.executableQmTextBox_TextChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.browseButton,
																					this.exeLabel,
																					this.executableTextBox});
			this.groupBox1.Location = new System.Drawing.Point(15, 20);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(480, 65);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "GPSBabel Executable Location";
			// 
			// browseButton
			// 
			this.browseButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.browseButton.Location = new System.Drawing.Point(440, 25);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 20);
			this.browseButton.TabIndex = 9;
			this.browseButton.Text = "...";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// exeLabel
			// 
			this.exeLabel.Location = new System.Drawing.Point(5, 25);
			this.exeLabel.Name = "exeLabel";
			this.exeLabel.Size = new System.Drawing.Size(80, 20);
			this.exeLabel.TabIndex = 5;
			this.exeLabel.Text = "Executable:";
			this.exeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// executableTextBox
			// 
			this.executableTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.executableTextBox.Location = new System.Drawing.Point(90, 25);
			this.executableTextBox.Name = "executableTextBox";
			this.executableTextBox.Size = new System.Drawing.Size(340, 20);
			this.executableTextBox.TabIndex = 4;
			this.executableTextBox.Text = "";
			this.executableTextBox.TextChanged += new System.EventHandler(this.executableTextBox_TextChanged);
			// 
			// actionBottomPanel
			// 
			this.actionBottomPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.reportBugLinkLabel,
																							this.executeButton,
																							this.cmdTextBox});
			this.actionBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.actionBottomPanel.Location = new System.Drawing.Point(0, 385);
			this.actionBottomPanel.Name = "actionBottomPanel";
			this.actionBottomPanel.Size = new System.Drawing.Size(752, 100);
			this.actionBottomPanel.TabIndex = 2;
			// 
			// reportBugLinkLabel
			// 
			this.reportBugLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.reportBugLinkLabel.Location = new System.Drawing.Point(680, 75);
			this.reportBugLinkLabel.Name = "reportBugLinkLabel";
			this.reportBugLinkLabel.Size = new System.Drawing.Size(65, 20);
			this.reportBugLinkLabel.TabIndex = 10;
			this.reportBugLinkLabel.TabStop = true;
			this.reportBugLinkLabel.Text = "report bug";
			this.reportBugLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.reportBugLinkLabel.VisitedLinkColor = System.Drawing.Color.Blue;
			this.reportBugLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.reportBugLinkLabel_LinkClicked);
			// 
			// executeButton
			// 
			this.executeButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.executeButton.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.executeButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.executeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.executeButton.Location = new System.Drawing.Point(680, 5);
			this.executeButton.Name = "executeButton";
			this.executeButton.Size = new System.Drawing.Size(65, 23);
			this.executeButton.TabIndex = 5;
			this.executeButton.Text = "<- Run";
			this.executeButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.executeButton.Click += new System.EventHandler(this.executeButton_Click);
			this.executeButton.MouseHover += new System.EventHandler(this.executeButton_MouseHover);
			// 
			// cmdTextBox
			// 
			this.cmdTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.cmdTextBox.Location = new System.Drawing.Point(5, 5);
			this.cmdTextBox.Multiline = true;
			this.cmdTextBox.Name = "cmdTextBox";
			this.cmdTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.cmdTextBox.Size = new System.Drawing.Size(670, 90);
			this.cmdTextBox.TabIndex = 4;
			this.cmdTextBox.Text = "";
			this.cmdTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cmdTextBox_KeyDown);
			// 
			// GpsBabelWrapper
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(752, 510);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel2,
																		  this.panel1});
			this.Menu = this.mainMenu1;
			this.Name = "GpsBabelWrapper";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "GpsBabel Wrapper";
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
			this.Move += new System.EventHandler(this.MainForm_Move);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.fillPanel.ResumeLayout(false);
			this.mainTabControl.ResumeLayout(false);
			this.actionTabPage.ResumeLayout(false);
			this.actionFillPanel.ResumeLayout(false);
			this.filterPanel.ResumeLayout(false);
			this.resultGroupBox.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.filterTabPage.ResumeLayout(false);
			this.optionsTabPage.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.qmStartDelay)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.actionBottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

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
		}

		private System.Windows.Forms.Timer periodicMaintenanceTimer = new System.Windows.Forms.Timer();

		public void periodicMaintenance(object obj, System.EventArgs args)
		{
			// ensure that we always have reftreshed picture:
			//if(!Project.goingDown && !Project.inhibitRefresh && (Project.pictureDirty || periodicMaintenanceCounter % 10 == 0))
//			if(!Project.goingDown && !Project.inhibitRefresh && (Project.pictureDirty || Project.pictureDirtyCounter > 0))
//			{
//			}
//			if((DateTime.Now - modifierPressedTime).Seconds > 5)
//			{
//				// avoid sticky modifiers, happens when you release ctrl, alt or shift outside the window
//				modifierPressedTime = DateTime.MaxValue;
//				controlDown = false;
//				shiftDown = false;
//				altDown = false;
//			}
			Project.popupMaintenance();
			periodicMaintenanceTimer.Enabled = true;
		}

		private void loadSetDlgIcon()
		{
			// try loading icon file (will be used for Project.setDlgIcon() for all dialogs):
			string iconFileName = Project.GetMiscPath(Project.PROGRAM_NAME_LOGICAL + ".ico");
			try 
			{
				if(!File.Exists(iconFileName))
				{
					// load the file remotely, don't hold diagnostics if load fails:
					string url = Project.MISC_FOLDER_URL + "/" + Project.PROGRAM_NAME_LOGICAL + ".ico";
					DloadProgressForm loaderForm = new DloadProgressForm(url, iconFileName, false, true);
					loaderForm.ShowDialog();
				}
				this.Icon = new Icon(iconFileName);
			} 
			catch {}
		}

		private void saveState()
		{
			Project.SaveOptions();
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
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );

//			HideGreeting();
		}

//		#region Greeting form
//
//		private Form m_greetingForm = null;
//		private DateTime m_greetingFormStarted = DateTime.Now;
//		private int KEEP_GREETING_UP_SECONDS = 1;
//		private bool hasShownUpdateForm = false;
//
//		private void ShowGreeting()
//		{
//			m_greetingFormStarted = DateTime.Now;
//			m_greetingForm = new DlgGreeting(m_warning, m_greeting);
//			//WebsiteInfo.greetingForm = m_greetingForm;
//			m_greetingForm.ShowDialog();
//		}
//
//		private void HideGreeting()
//		{
//			TimeSpan sinceUp = DateTime.Now - m_greetingFormStarted;
//			int toSleep = (Project.upgradeMessage.Length == 0 ? KEEP_GREETING_UP_SECONDS : 3) - sinceUp.Seconds; 
//			if(toSleep > 0 && !Project.goingDown)
//			{
//				hideGreetingTimer.Interval = toSleep * 1000;
//				hideGreetingTimer.Tick += new EventHandler(_hideGreeting2);
//				hideGreetingTimer.Start();
//			}
//			else
//			{
//				try 
//				{
//					if(this.InvokeRequired)
//					{
//						this.Invoke(new MethodInvoker(_hideGreeting));
//					} 
//					else
//					{
//						_hideGreeting();
//					}
//				}
//				catch {}
//			}
//		}
//		
//		private System.Windows.Forms.Timer hideGreetingTimer = new System.Windows.Forms.Timer();
//
//		private void _hideGreeting2(object obj, System.EventArgs args)
//		{
//			hideGreetingTimer.Stop();	// it is a one-time deal, stop the timer
//			_hideGreeting();
//		}
//
//		private void _hideGreeting()
//		{
//			//this.Focus();
//			//this.BringToFront();	// main form to front
//
//			//WebsiteInfo.greetingForm = null;
//			try
//			{
//				if(m_greetingForm != null)
//				{
//					//m_greetingForm.Hide();
//					m_greetingForm.Close();
//					m_greetingForm = null;
//				}
//				if(m_greetingThread != null)
//				{
//					Thread.Sleep(500);		// let the dialog close before we abort the thread
//					m_greetingThread.Abort();
//					m_greetingThread = null;
//				}
//			} 
//			catch
//			{
//			}
//
//
//			if(!hasShownUpdateForm && Project.upgradeMessage.Length > 0)
//			{
//				hasShownUpdateForm = true;
//				new DlgUpgradeMessage(Project.upgradeMessage).ShowDialog();
//			}
//			else
//			{
//				//Thread.Sleep(1000);
//				this.Focus();
//				this.BringToFront();	// main form to front
//			}
//		}
//		#endregion

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			LibSys.StatusBar.WriteLine("MainForm: closing");
			Project.mainFormMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);

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

		private void MainForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
//			if(m_greetingForm != null)
//			{
//				// first paint makes sure that the greeting form is closed:
//				HideGreeting();
//			}
		}
		#endregion

		// this method is run in a worker thread:
		//private void runInit(object state, DateTime requestEnqueueTime)
		private void runInit(object state)
		{
			LibSys.StatusBar.WriteLine("Initializing");
			//Thread.Sleep(1000);	// it somehow goes smoother when we sleep here. frame and buttons appear faster.

			statusBar.Click += new System.EventHandler(LibSys.StatusBar.statusBar_Click);

			// now see if any files were dropped on our icon (or supplied as cmd line args):
			if(Args != null && Args.Length > 0)
			{
				foreach (string arg in Args)
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

                                    // want to do something useful with the file here?
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

//			FileAndZipIO.readPersistentAndDroppedFiles(!m_hadLonLatArgs, initialMessageLabel);	// dropped on icon or command line arguments

			loadSetDlgIcon();

			LibSys.StatusBar.WriteLine("Ready");
		}

		#region processArgs()

		private bool m_hadLonLatArgs = false;

		private void processArgs()
		{
			// process arguments from the command line:  /mode=file /mode=gps /lat=34.123 /lon=-117.234 /elev=5000
			for(int i=0; i < Args.Length ;i++)
			{
				string arg = Args[i];

				if(arg != null)
				{
					try
					{
						if(arg.ToLower().StartsWith("/mode="))
						{
							//  /mode=file   or   /mode=gps
							Args[i] = null;
						}
						else if(arg.ToLower().StartsWith("/lat="))
						{
							double lat = Convert.ToDouble(arg.Substring(5));
							if(lat < 90.0d && lat > -90.0d)
							{
//								Project.cameraLat = lat;
								m_hadLonLatArgs = true;
							}
							Args[i] = null;
						}
						else if(arg.ToLower().StartsWith("/lon="))
						{
							double lon = Convert.ToDouble(arg.Substring(5));
							if(lon < 180.0d && lon > -180.0d)
							{
//								Project.cameraLng = lon;
								m_hadLonLatArgs = true;
							}
							Args[i] = null;
						}
						else if(arg.ToLower().StartsWith("/elev="))
						{
							double elev = Convert.ToDouble(arg.Substring(6));
//							Project.cameraElev = elev;
							m_hadLonLatArgs = true;
							Args[i] = null;
						}
					}
					catch (Exception ee)
					{
						Args[i] = null;
						string message = "argument '" + arg + "' - wrong format (" + ee.Message + ")";
						LibSys.StatusBar.Error(message);
						Project.ErrorBox(null, message);
					}
				}
			}
		}

		#endregion

		private void fillCommandObject(GpsBabelCommand gbc)
		{
			gbc.ProcessWaypointsInformation = wptCheckBox.Checked;
			gbc.ProcessTracksInformation = trkCheckBox.Checked;
			gbc.ProcessRoutesInformation = rteCheckBox.Checked;
			gbc.SynthesizeShortnames = shortnamesCheckBox.Checked;
			gbc.AdHoc = adHocTextBox.Text.Trim();
		}

		#region Event handlers

		private void onInputSelectionChanged(object sender, System.EventArgs e)
		{
			m_inputControl.fillCommandObject(m_gbc);
			reEvaluate();
		}

		private void onInputGpsSelectionChanged(object sender, System.EventArgs e)
		{
			m_outputControl.updateGpsControl();
		}

		private void onOutputSelectionChanged(object sender, System.EventArgs e)
		{
			m_outputControl.fillCommandObject(m_gbc);
			reEvaluate();
		}

		private void onOutputGpsSelectionChanged(object sender, System.EventArgs e)
		{
			m_inputControl.updateGpsControl();
		}

		private void onFilterSelectionChanged(object sender, System.EventArgs e)
		{
			m_filtersPane.fillCommandObject(m_gbc);
			reEvaluate();
		}

		private void reEvaluate()
		{
			try
			{
				// Evaluate GpsBabel options string:
				string options = m_gbc.Evaluate();

				cmdTextBox.Text = options;
			}
			catch (Exception exc)
			{
				LibSys.StatusBar.Error("" + exc.Message);
			}
		}

		#endregion

		#region Clickers

		private void aboutMenuItem_Click(object sender, System.EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.ShowDialog();
		}

		private void helpMenuItem_Click(object sender, System.EventArgs e)
		{
			showHelp();
		}

		private void fileMainMenuItem_Popup(object sender, System.EventArgs e)
		{
			rebuildRecentFilesMenu();
		}

		private void executeButton_Click(object sender, System.EventArgs e)
		{
			execute();
		}

		private void execute()
		{
			string result = "";
			resultsLabel.Text = "IP: working...";
			resultsLabel.Refresh();

			executeButton.Enabled = false;
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				m_gbc.Execute(cmdTextBox.Text.Trim());
				result = m_gbc.ResultString + "\n" + m_gbc.ErrorString;
			}
			catch (Exception e)
			{
				result = e.Message;
				if(!result.StartsWith("Error:"))
				{
					result = "Error: " + result;
				}
			}
			resultsLabel.Text = result;
			LibSys.StatusBar.Trace(result);
			executeButton.Enabled = true;
		}

		private void executeButton_MouseHover(object sender, System.EventArgs e)
		{
			Project.ShowPopup(executeButton, "executes command in the box to the left", Point.Empty);
		}

		private void resultsLabel_TextChanged(object sender, System.EventArgs e)
		{
			SizeF size = resultsLabel.CreateGraphics().MeasureString(resultsLabel.Text, resultsLabel.Font);
			int labelHeight = (int)(size.Height); 

			resultsLabel.Height = labelHeight + 10;
		}

		#endregion

		private void cmdTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyData) 
			{
				case Keys.Enter:
					execute();
					break;
			}
		}

		private void resultsLabel_Click(object sender, System.EventArgs e)
		{
			if(resultsLabel.Text.Length > 0)
			{
				ListForm lf = new ListForm();
				lf.setText(resultsLabel.Text);
				lf.Text = "Results";
				lf.ShowDialog();
			}
		}

		private void moreFilterLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			this.mainTabControl.SelectedTab = filterTabPage;
		}

		private void filterMainOptionsChanged(object sender, System.EventArgs e)
		{
			this.fillCommandObject(m_gbc);
			reEvaluate();
		}

		private void helpGBHomeMenuItem_Click(object sender, System.EventArgs e)
		{
			Project.RunBrowser(Project.GPSBABEL_URL);
		}

		private void helpGBHelpMenuItem_Click(object sender, System.EventArgs e)
		{
			try
			{
				string help = m_gbc.GetHelp();
				if(help.Length > 0)
				{
					ListForm lf = new ListForm();
					lf.setText(help);
					lf.Text = "Help on command line options";
					lf.ShowDialog();
				}
			}
			catch (Exception exc)
			{
				string result = exc.Message;
				if(!result.StartsWith("Error:"))
				{
					result = "Error: " + result;
				}
				resultsLabel.Text = result;
			}
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

			if(File.Exists(this.executableTextBox.Text))
			{
				openFileDialog.FileName = this.executableTextBox.Text;
			}
			else
			{
				openFileDialog.InitialDirectory = this.executableTextBox.Text;
			}
			openFileDialog.DefaultExt = ".exe";
			openFileDialog.AddExtension = false;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "executable files (*.exe)|*.exe|All files (*.*)|*.*";

			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileInfo fi = new FileInfo(openFileDialog.FileName);
				this.executableTextBox.Text = Project.GetLongPathName(fi.FullName);
			}
		}

		private void executableTextBox_TextChanged(object sender, System.EventArgs e)
		{
			setGpsBabelExecutable();
		}

		private void setGpsBabelExecutable()
		{
			Project.GpsBabelExecutable = executableTextBox.Text;
			if(m_gbc != null)
			{
				m_gbc.resetExecutable();
			}
		}

		private void browseQmButton_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

			if(File.Exists(this.executableQmTextBox.Text))
			{
				openFileDialog.FileName = this.executableQmTextBox.Text;
			}
			else
			{
				openFileDialog.InitialDirectory = this.executableQmTextBox.Text;
			}
			openFileDialog.DefaultExt = ".exe";
			openFileDialog.AddExtension = false;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "executable files (*.exe)|*.exe|All files (*.*)|*.*";

			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileInfo fi = new FileInfo(openFileDialog.FileName);
				this.executableQmTextBox.Text = Project.GetLongPathName(fi.FullName);
			}
		}

		private void executableQmTextBox_TextChanged(object sender, System.EventArgs e)
		{
			setQuakeMapExecutable();
		}

		private void setQuakeMapExecutable()
		{
			Project.QuakeMapExecutable = executableQmTextBox.Text;
		}

		private void qmStartDelay_ValueChanged(object sender, System.EventArgs e)
		{
			Project.QuakeMapStartupDelay = (int)qmStartDelay.Value;
		}

		private void fileOpenMenuItem_Click(object sender, System.EventArgs e)
		{
			this.mainTabControl.SelectedIndex = 0;
			m_inputControl.onFileOpen();
		}

		private void fileSaveAsMenuItem_Click(object sender, System.EventArgs e)
		{
			this.mainTabControl.SelectedIndex = 0;
			m_outputControl.onFileSaveAs();
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

		private void helpOptionsMenuItem_Click(object sender, System.EventArgs e)
		{
			this.mainTabControl.SelectedIndex = 2;
		}

		private void helpReportBugMenuItem_Click(object sender, System.EventArgs e)
		{
				Project.RunBrowser(Project.REPORT_BUG_URL);
		}

		private void reportBugLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(Project.REPORT_BUG_URL);
		}

		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
