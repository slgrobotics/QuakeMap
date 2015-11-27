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

using LibSys;

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for GpsSelectorControl.
	/// </summary>
	public class GpsSelectorControl : System.Windows.Forms.UserControl
	{
		private bool inSet = false;
		public static string[] AllMakesNames = new string[] {
																"Garmin Serial",
																"Garmin USB",
																"Magellan",
																"Generic NMEA"
															};

		internal System.Windows.Forms.ComboBox makeComboBox;
		private System.Windows.Forms.Label label1;
		internal System.Windows.Forms.ComboBox comboBoxBaud;
		internal System.Windows.Forms.ComboBox portComboBox;
		internal System.Windows.Forms.CheckBox noackCheckBox;
		private System.Windows.Forms.Label baudLabel;
		private System.Windows.Forms.Label portLabel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GpsSelectorControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.makeComboBox.Items.AddRange(AllMakesNames);

			this.portComboBox.Items.AddRange(new object[] {	  "COM1:",
															  "COM2:",
															  "COM3:",
															  "COM4:",
															  "COM5:",
															  "COM6:",
															  "COM7:",
															  "COM8:"});

			this.comboBoxBaud.Items.AddRange(new object[] {	  "1200",
															  "2400",
															  "4800",
															  "9600",
															  "19200",
															  "38400",
															  "57600",
															  "115200"});

			this.makeComboBox.SelectedIndexChanged +=  new EventHandler(onOutputSelectionChanged);

			updateUI();
		}

		private void onOutputSelectionChanged(object sender, System.EventArgs e)
		{
			updateUI();
		}

		private void updateUI()
		{
			switch (makeComboBox.SelectedIndex)
			{
				case 0:
					this.portLabel.Visible = true;
					this.portComboBox.Visible = true;
					this.baudLabel.Visible = false;
					this.comboBoxBaud.Visible = false;
					this.noackCheckBox.Visible = false;
					break;
				case 1:
					this.portLabel.Visible = false;
					this.portComboBox.Visible = false;
					this.baudLabel.Visible = false;
					this.comboBoxBaud.Visible = false;
					this.noackCheckBox.Visible = false;
					break;
				case 2:
					this.portLabel.Visible = true;
					this.portComboBox.Visible = true;
					this.baudLabel.Visible = true;
					this.comboBoxBaud.Visible = true;
					this.noackCheckBox.Visible = true;
					break;
				case 3:
					this.portLabel.Visible = true;
					this.portComboBox.Visible = true;
					this.baudLabel.Visible = true;
					this.comboBoxBaud.Visible = true;
					this.noackCheckBox.Visible = false;
					break;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.makeComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.baudLabel = new System.Windows.Forms.Label();
			this.comboBoxBaud = new System.Windows.Forms.ComboBox();
			this.portComboBox = new System.Windows.Forms.ComboBox();
			this.portLabel = new System.Windows.Forms.Label();
			this.noackCheckBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// makeComboBox
			// 
			this.makeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.makeComboBox.Location = new System.Drawing.Point(50, 5);
			this.makeComboBox.Name = "makeComboBox";
			this.makeComboBox.Size = new System.Drawing.Size(255, 21);
			this.makeComboBox.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(0, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Make:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// baudLabel
			// 
			this.baudLabel.Location = new System.Drawing.Point(165, 30);
			this.baudLabel.Name = "baudLabel";
			this.baudLabel.Size = new System.Drawing.Size(40, 19);
			this.baudLabel.TabIndex = 19;
			this.baudLabel.Text = "Baud:";
			this.baudLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.baudLabel.UseMnemonic = false;
			// 
			// comboBoxBaud
			// 
			this.comboBoxBaud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBaud.Location = new System.Drawing.Point(210, 30);
			this.comboBoxBaud.Name = "comboBoxBaud";
			this.comboBoxBaud.Size = new System.Drawing.Size(95, 21);
			this.comboBoxBaud.TabIndex = 18;
			// 
			// portComboBox
			// 
			this.portComboBox.Location = new System.Drawing.Point(50, 30);
			this.portComboBox.Name = "portComboBox";
			this.portComboBox.Size = new System.Drawing.Size(95, 21);
			this.portComboBox.TabIndex = 17;
			// 
			// portLabel
			// 
			this.portLabel.Location = new System.Drawing.Point(5, 30);
			this.portLabel.Name = "portLabel";
			this.portLabel.Size = new System.Drawing.Size(37, 20);
			this.portLabel.TabIndex = 16;
			this.portLabel.Text = "Port:";
			this.portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// noackCheckBox
			// 
			this.noackCheckBox.Location = new System.Drawing.Point(335, 30);
			this.noackCheckBox.Name = "noackCheckBox";
			this.noackCheckBox.Size = new System.Drawing.Size(65, 24);
			this.noackCheckBox.TabIndex = 20;
			this.noackCheckBox.Text = "noack";
			// 
			// GpsSelectorControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.noackCheckBox,
																		  this.baudLabel,
																		  this.comboBoxBaud,
																		  this.portComboBox,
																		  this.portLabel,
																		  this.makeComboBox,
																		  this.label1});
			this.Name = "GpsSelectorControl";
			this.Size = new System.Drawing.Size(405, 55);
			this.ResumeLayout(false);

		}
		#endregion

	}
}
