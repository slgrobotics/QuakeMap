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

namespace LibSys
{
	/// <summary>
	/// Summary description for MyTimePicker.
	/// </summary>
	public class MyTimePicker : System.Windows.Forms.UserControl
	{
		private DateTime m_dateTime;

		private System.Windows.Forms.Button activeButton;
		protected System.Windows.Forms.DateTimePicker timeDateTimePicker;
		protected System.Windows.Forms.DateTimePicker dateDateTimePicker;
		private System.Windows.Forms.Label infoLabel;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MyTimePicker()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.timeDateTimePicker.CustomFormat = "h:mm:ss tt";
			dateTime = DateTime.MinValue;

			// initial values for both pickers is Now, so if control is activated current time will show up.
		}

		public bool isDateTimeValid
		{
			get { return m_dateTime.Ticks > DateTimePicker.MinDateTime.Ticks && m_dateTime.Ticks < DateTimePicker.MaxDateTime.Ticks; }

			set { 
				if(value)
				{
					m_dateTime = DateTime.Now;
					isActive = true;
				}
				else
				{
					m_dateTime = DateTime.MinValue;
					isActive = false;
				}
			}
		}

		// to make control inactive, set dateTime to DateTime.MinValue  or use isDateTimeValid=false
		public DateTime dateTime 
		{
			get
			{
				if(dateDateTimePicker.Visible && this.Enabled)
				{
					m_dateTime = dateDateTimePicker.Value.Date + timeDateTimePicker.Value.TimeOfDay;
				}
				return m_dateTime;
			}

			set
			{
				m_dateTime = value;
				if(isDateTimeValid)
				{
					isActive = true;
					dateDateTimePicker.Value = m_dateTime;
					timeDateTimePicker.Value = m_dateTime;
				}
				else
				{
					isActive = false;
				}
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
			this.activeButton = new System.Windows.Forms.Button();
			this.timeDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.dateDateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.infoLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// activeButton
			// 
			this.activeButton.Name = "activeButton";
			this.activeButton.Size = new System.Drawing.Size(24, 20);
			this.activeButton.TabIndex = 1;
			this.activeButton.Text = "...";
			this.activeButton.Click += new System.EventHandler(this.activeButton_Click);
			// 
			// timeDateTimePicker
			// 
			this.timeDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.timeDateTimePicker.Location = new System.Drawing.Point(224, 0);
			this.timeDateTimePicker.Name = "timeDateTimePicker";
			this.timeDateTimePicker.ShowUpDown = true;
			this.timeDateTimePicker.Size = new System.Drawing.Size(96, 20);
			this.timeDateTimePicker.TabIndex = 3;
			// 
			// dateDateTimePicker
			// 
			this.dateDateTimePicker.Location = new System.Drawing.Point(24, 0);
			this.dateDateTimePicker.Name = "dateDateTimePicker";
			this.dateDateTimePicker.Size = new System.Drawing.Size(192, 20);
			this.dateDateTimePicker.TabIndex = 2;
			// 
			// infoLabel
			// 
			this.infoLabel.Location = new System.Drawing.Point(32, 3);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(240, 23);
			this.infoLabel.TabIndex = 4;
			this.infoLabel.Text = "(not set. click the button to set)";
			// 
			// MyTimePicker
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.infoLabel,
																		  this.activeButton,
																		  this.timeDateTimePicker,
																		  this.dateDateTimePicker});
			this.Name = "MyTimePicker";
			this.Size = new System.Drawing.Size(320, 20);
			this.ResumeLayout(false);

		}
		#endregion

		private void activeButton_Click(object sender, System.EventArgs e)
		{
			isActive = !isActive;
		}

		public bool isActive
		{
			get { return dateDateTimePicker.Visible; }

			set 
			{
				infoLabel.Visible = !value && this.Enabled;
				dateDateTimePicker.Visible = value;
				timeDateTimePicker.Visible = value;
				activeButton.Text = value ? "X" : "..";

				// not active means that the timestamp is not set, so all we see is a message about it and the button ".."
				// active state displays date/time and the "X" button - clicking on which sets waypoint's timestamp to none

//				if(isDateTimeValid)
//				{
//					this.infoLabel.Text = "" + m_dateTime.ToString();
//				}
//				else
//				{
//					this.infoLabel.Text = "(not set. click the button to set)";
//				}
			}
		}
	}
}
