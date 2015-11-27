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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Xml;

using GpsBabelCmd;

using LibSys;

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for OutputControl.
	/// </summary>
	public class OutputControl : System.Windows.Forms.UserControl
	{
		public event EventHandler selectionChanged;
		public event EventHandler gpsSelectionChanged;

		public string OutputFileName { get { return fileSelector.FileName; } }
		public string OutputCustomArgs { get { return customArgs.CustomArgs; } }

		public XmlDocument formatDoc 
		{
			set 
			{
				fileSelector.formatDoc = value;
                if (Project.outputFormatIndex == -1)
                {
                    findDefaultOutputFormatIndex(value);
                }
                fileSelector.FormatIndex = Project.outputFormatIndex;
			} 
		}

        private void findDefaultOutputFormatIndex(XmlDocument formatDoc)
        {
            Project.outputFormatIndex = 0;

            try
            {
                //LibSys.StatusBar.Trace(formatDoc.OuterXml);

                XmlNode node = formatDoc.DocumentElement.SelectSingleNode("//option[@name='gpx']");
                if (node != null)
                {
                    int i = 0;
                    foreach (XmlNode node1 in node.ParentNode.ChildNodes)
                    {
                        if (node1 == node)
                        {
                            Project.outputFormatIndex = i;
                            Project.firstRun = true;
                            break;
                        }
                        i++;
                    }

                    LibSys.StatusBar.Trace("gpx node=" + Project.outputFormatIndex);
                }
            }
            catch { }
        }

		private FileFormatSelectorControl fileSelector = new FileFormatSelectorControl(true);
		private CustomArgsControl customArgs = new CustomArgsControl();
		private GpsSelectorControl gpsSelector = new GpsSelectorControl();

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage fileTabPage;
		private System.Windows.Forms.TabPage gpsTabPage;
		private System.Windows.Forms.TabPage quakeMapTabPage;
		private System.Windows.Forms.TabPage customTabPage;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButtonAerial;
		private System.Windows.Forms.RadioButton radioButtonColorAerial;
		private System.Windows.Forms.RadioButton radioButtonTopo;
		private System.Windows.Forms.RadioButton radioButtonNone;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OutputControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			fileSelector.Dock = DockStyle.Fill;
			fileTabPage.Controls.Add(fileSelector);

			gpsSelector.Dock = DockStyle.Fill;
			gpsTabPage.Controls.Add(gpsSelector);

			updateGpsControl();

			gpsSelector.makeComboBox.SelectedIndexChanged +=  new EventHandler(onGpsSelectionChanged);
			gpsSelector.portComboBox.SelectedIndexChanged +=  new EventHandler(onGpsSelectionChanged);
			gpsSelector.comboBoxBaud.SelectedIndexChanged +=  new EventHandler(onGpsSelectionChanged);
			gpsSelector.noackCheckBox.CheckedChanged +=  new EventHandler(onGpsSelectionChanged);

			customArgs.Dock = DockStyle.Fill;
			customTabPage.Controls.Add(customArgs);

			tabControl1.SelectedIndex = Project.outputSelectedIndex;
			collectIotype();
			fileSelector.FileName = Project.outputFile;

			fileSelector.selectionChanged += new EventHandler(onOutputSelectionChanged);
			customArgs.selectionChanged += new EventHandler(onOutputSelectionChanged);
			tabControl1.SelectedIndexChanged  += new EventHandler(onOutputSelectionChanged);
			radioButtonAerial.CheckedChanged  += new EventHandler(onOutputSelectionChanged);
			radioButtonColorAerial.CheckedChanged  += new EventHandler(onOutputSelectionChanged);
			radioButtonTopo.CheckedChanged  += new EventHandler(onOutputSelectionChanged);
			radioButtonNone.CheckedChanged  += new EventHandler(onOutputSelectionChanged);
		}

		public void updateGpsControl()
		{
			gpsSelector.makeComboBox.SelectedIndex = Project.gpsMakeIndex;
			gpsSelector.portComboBox.Text = Project.gpsPort;
			gpsSelector.comboBoxBaud.Text = Project.gpsBaudRate;
			gpsSelector.noackCheckBox.Checked = Project.gpsNoack;
		}

        public void selectQuakeMapMode()
        {
            Project.outputSelectedIndex = 2;        // select QuakeMap mode
            tabControl1.SelectedIndex = Project.outputSelectedIndex;

            onSelectionChanged();
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.fileTabPage = new System.Windows.Forms.TabPage();
			this.gpsTabPage = new System.Windows.Forms.TabPage();
			this.quakeMapTabPage = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioButtonNone = new System.Windows.Forms.RadioButton();
			this.radioButtonTopo = new System.Windows.Forms.RadioButton();
			this.radioButtonColorAerial = new System.Windows.Forms.RadioButton();
			this.radioButtonAerial = new System.Windows.Forms.RadioButton();
			this.customTabPage = new System.Windows.Forms.TabPage();
			this.tabControl1.SuspendLayout();
			this.quakeMapTabPage.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.fileTabPage,
																					  this.gpsTabPage,
																					  this.quakeMapTabPage,
																					  this.customTabPage});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.HotTrack = true;
			this.tabControl1.Multiline = true;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(885, 80);
			this.tabControl1.TabIndex = 1;
			// 
			// fileTabPage
			// 
			this.fileTabPage.Location = new System.Drawing.Point(4, 25);
			this.fileTabPage.Name = "fileTabPage";
			this.fileTabPage.Size = new System.Drawing.Size(877, 51);
			this.fileTabPage.TabIndex = 0;
			this.fileTabPage.Text = "File";
			// 
			// gpsTabPage
			// 
			this.gpsTabPage.Location = new System.Drawing.Point(4, 25);
			this.gpsTabPage.Name = "gpsTabPage";
			this.gpsTabPage.Size = new System.Drawing.Size(877, 91);
			this.gpsTabPage.TabIndex = 1;
			this.gpsTabPage.Text = "GPS";
			this.gpsTabPage.Visible = false;
			// 
			// quakeMapTabPage
			// 
			this.quakeMapTabPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.groupBox1});
			this.quakeMapTabPage.Location = new System.Drawing.Point(4, 25);
			this.quakeMapTabPage.Name = "quakeMapTabPage";
			this.quakeMapTabPage.Size = new System.Drawing.Size(877, 51);
			this.quakeMapTabPage.TabIndex = 2;
			this.quakeMapTabPage.Text = "QuakeMap";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.radioButtonNone,
																					this.radioButtonTopo,
																					this.radioButtonColorAerial,
																					this.radioButtonAerial});
			this.groupBox1.Location = new System.Drawing.Point(95, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(305, 40);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "type of map";
			// 
			// radioButtonNone
			// 
			this.radioButtonNone.Location = new System.Drawing.Point(240, 15);
			this.radioButtonNone.Name = "radioButtonNone";
			this.radioButtonNone.Size = new System.Drawing.Size(60, 20);
			this.radioButtonNone.TabIndex = 3;
			this.radioButtonNone.Text = "none";
			// 
			// radioButtonTopo
			// 
			this.radioButtonTopo.Location = new System.Drawing.Point(175, 15);
			this.radioButtonTopo.Name = "radioButtonTopo";
			this.radioButtonTopo.Size = new System.Drawing.Size(60, 20);
			this.radioButtonTopo.TabIndex = 2;
			this.radioButtonTopo.Text = "topo";
			// 
			// radioButtonColorAerial
			// 
			this.radioButtonColorAerial.Location = new System.Drawing.Point(80, 15);
			this.radioButtonColorAerial.Name = "radioButtonColorAerial";
			this.radioButtonColorAerial.Size = new System.Drawing.Size(90, 20);
			this.radioButtonColorAerial.TabIndex = 1;
			this.radioButtonColorAerial.Text = "color aerial";
			// 
			// radioButtonAerial
			// 
			this.radioButtonAerial.Location = new System.Drawing.Point(15, 15);
			this.radioButtonAerial.Name = "radioButtonAerial";
			this.radioButtonAerial.Size = new System.Drawing.Size(60, 20);
			this.radioButtonAerial.TabIndex = 0;
			this.radioButtonAerial.Text = "aerial";
			// 
			// customTabPage
			// 
			this.customTabPage.Location = new System.Drawing.Point(4, 25);
			this.customTabPage.Name = "customTabPage";
			this.customTabPage.Size = new System.Drawing.Size(877, 91);
			this.customTabPage.TabIndex = 3;
			this.customTabPage.Text = "Custom";
			// 
			// OutputControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.Name = "OutputControl";
			this.Size = new System.Drawing.Size(885, 80);
			this.tabControl1.ResumeLayout(false);
			this.quakeMapTabPage.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event handlers

		private void onGpsSelectionChanged(object sender, System.EventArgs e)
		{
			onSelectionChanged();

			if(gpsSelectionChanged != null)
			{
				gpsSelectionChanged(this, null);
			}
		}

		private void onOutputSelectionChanged(object sender, System.EventArgs e)
		{
			onSelectionChanged();
		}

		public IOType iotype;

		private void collectIotype()
		{
			if(this.tabControl1.SelectedTab == fileTabPage)
			{
				iotype = IOType.File;
			}
			else if(this.tabControl1.SelectedTab == gpsTabPage)
			{
				iotype = IOType.Gps;
			}
			else if(this.tabControl1.SelectedTab == quakeMapTabPage)
			{
				iotype = IOType.QuakeMap;
			}
			else if(this.tabControl1.SelectedTab == customTabPage)
			{
				iotype = IOType.Custom;
			}
		}

		private void onSelectionChanged()
		{
			collectIotype();

			Project.outputSelectedIndex = tabControl1.SelectedIndex;
			Project.outputFile = OutputFileName;
			Project.outputFormatIndex = fileSelector.FormatIndex;

			fileSelector.setFormat();

			Project.gpsMakeIndex = gpsSelector.makeComboBox.SelectedIndex;
			Project.gpsPort = gpsSelector.portComboBox.Text;
			Project.gpsBaudRate = gpsSelector.comboBoxBaud.Text;
			Project.gpsNoack = gpsSelector.noackCheckBox.Checked;

			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		#endregion

		public void onFileSaveAs()
		{
			tabControl1.SelectedTab = fileTabPage;
			fileSelector.bringUpFileSelectionDialog();
		}

		public void fillCommandObject(GpsBabelCommand gbc)
		{
			if(radioButtonAerial.Checked)
			{
				gbc.MapType = MapType.Aerial;
			}
			else if(radioButtonColorAerial.Checked)
			{
				gbc.MapType = MapType.ColorAerial;
			}
			else if(radioButtonTopo.Checked)
			{
				gbc.MapType = MapType.Topo;
			}
			else if(radioButtonNone.Checked)
			{
				gbc.MapType = MapType.None;
			}
			gbc.OutputIOType = iotype;
			gbc.OutputFileName = OutputFileName;
			gbc.OutputCustom = OutputCustomArgs;
			gbc.OutputFormat = fileSelector.FormatName;
			gbc.OutputOptions = fileSelector.getOptionsText();
		}
	}
}
