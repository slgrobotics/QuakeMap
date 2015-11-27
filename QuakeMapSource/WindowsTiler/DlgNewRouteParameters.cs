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

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgNewRouteParameters.
	/// </summary>
	public class DlgNewRouteParameters : System.Windows.Forms.Form
	{
		public bool result;
		protected MyTimePicker timePicker;
		public static double altitude = 0.0d;		// meters
		public static double speed = 0.0d;			// meters per hour
		public static DateTime startTime = Project.localToZulu(new DateTime(7000, 1, 1));		// keep in mind DateTimePicker allows date years 1758 to 9999

		protected System.Windows.Forms.Button cancelButton;
		protected System.Windows.Forms.Button goButton;
		protected System.Windows.Forms.Label speedUnitsLabel;
		protected System.Windows.Forms.TextBox speedTextBox;
		protected System.Windows.Forms.Label speedLabel;
		protected System.Windows.Forms.Label timeLabel;
		protected System.Windows.Forms.Label elevationUnitsLabel;
		protected System.Windows.Forms.TextBox elevationTextBox;
		protected System.Windows.Forms.Label elevLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgNewRouteParameters()
		{
			result = false;

			InitializeComponent();

			// the time picker comes inactive, but if activated by button - current time will be displayed.
			timePicker = new MyTimePicker();
			timePicker.Location = new System.Drawing.Point(75, 10);
			timePicker.TabIndex = 1;
			this.Controls.Add(timePicker);

			setFields();

			Project.setDlgIcon(this);
		}

		public void setStartTime(DateTime st)
		{
			if(st < new DateTime(6999, 1, 1) && st > DateTimePicker.MinDateTime)
			{
				startTime = st;
				timePicker.isDateTimeValid = true;
				timePicker.dateTime = startTime;
			}
			else
			{
				timePicker.isDateTimeValid = false;
			}
		}

		protected void setFields()
		{
			Distance elevDist = new Distance(altitude);
			int elevUnitsCompl = elevDist.UnitsCompl;
			elevationTextBox.Text = elevDist.toStringN(elevUnitsCompl);
			elevationUnitsLabel.Text = elevDist.toStringU(elevUnitsCompl);

			Speed speedDist = new Speed(speed);
			int speedUnits = speedDist.Units;
			speedTextBox.Text = speedDist.toStringN(speedUnits);
			speedUnitsLabel.Text = speedDist.toStringU(speedUnits);
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
			this.cancelButton = new System.Windows.Forms.Button();
			this.goButton = new System.Windows.Forms.Button();
			this.speedUnitsLabel = new System.Windows.Forms.Label();
			this.speedTextBox = new System.Windows.Forms.TextBox();
			this.speedLabel = new System.Windows.Forms.Label();
			this.timeLabel = new System.Windows.Forms.Label();
			this.elevationUnitsLabel = new System.Windows.Forms.Label();
			this.elevationTextBox = new System.Windows.Forms.TextBox();
			this.elevLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(320, 80);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(88, 24);
			this.cancelButton.TabIndex = 52;
			this.cancelButton.Text = "Cancel";
			// 
			// goButton
			// 
			this.goButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.goButton.Location = new System.Drawing.Point(220, 80);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(88, 24);
			this.goButton.TabIndex = 51;
			this.goButton.Text = "Continue";
			this.goButton.Click += new System.EventHandler(this.goButton_Click);
			// 
			// speedUnitsLabel
			// 
			this.speedUnitsLabel.Location = new System.Drawing.Point(325, 40);
			this.speedUnitsLabel.Name = "speedUnitsLabel";
			this.speedUnitsLabel.Size = new System.Drawing.Size(75, 22);
			this.speedUnitsLabel.TabIndex = 64;
			this.speedUnitsLabel.Text = "meters/sec";
			this.speedUnitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// speedTextBox
			// 
			this.speedTextBox.Location = new System.Drawing.Point(265, 40);
			this.speedTextBox.Name = "speedTextBox";
			this.speedTextBox.Size = new System.Drawing.Size(57, 20);
			this.speedTextBox.TabIndex = 62;
			this.speedTextBox.Text = "";
			// 
			// speedLabel
			// 
			this.speedLabel.Location = new System.Drawing.Point(210, 40);
			this.speedLabel.Name = "speedLabel";
			this.speedLabel.Size = new System.Drawing.Size(50, 22);
			this.speedLabel.TabIndex = 63;
			this.speedLabel.Text = "Speed:";
			this.speedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// timeLabel
			// 
			this.timeLabel.Location = new System.Drawing.Point(5, 10);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(64, 22);
			this.timeLabel.TabIndex = 61;
			this.timeLabel.Text = "Start Time:";
			this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// elevationUnitsLabel
			// 
			this.elevationUnitsLabel.Location = new System.Drawing.Point(140, 40);
			this.elevationUnitsLabel.Name = "elevationUnitsLabel";
			this.elevationUnitsLabel.Size = new System.Drawing.Size(55, 22);
			this.elevationUnitsLabel.TabIndex = 60;
			this.elevationUnitsLabel.Text = "meters";
			this.elevationUnitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// elevationTextBox
			// 
			this.elevationTextBox.Location = new System.Drawing.Point(75, 40);
			this.elevationTextBox.Name = "elevationTextBox";
			this.elevationTextBox.Size = new System.Drawing.Size(57, 20);
			this.elevationTextBox.TabIndex = 58;
			this.elevationTextBox.Text = "";
			// 
			// elevLabel
			// 
			this.elevLabel.Location = new System.Drawing.Point(5, 40);
			this.elevLabel.Name = "elevLabel";
			this.elevLabel.Size = new System.Drawing.Size(64, 22);
			this.elevLabel.TabIndex = 59;
			this.elevLabel.Text = "Altitude:";
			this.elevLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DlgNewRouteParameters
			// 
			this.AcceptButton = this.goButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(415, 110);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.speedUnitsLabel,
																		  this.speedTextBox,
																		  this.speedLabel,
																		  this.timeLabel,
																		  this.elevationUnitsLabel,
																		  this.elevationTextBox,
																		  this.elevLabel,
																		  this.cancelButton,
																		  this.goButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "DlgNewRouteParameters";
			this.Opacity = 0.8;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Route Parameters";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		private void goButton_Click(object sender, System.EventArgs e)
		{
			bool allcool = true;

			try 
			{
				Distance elevDist = new Distance(0.0d);
				int unitsCompl = elevDist.UnitsCompl;

				double nuElev = Convert.ToDouble(elevationTextBox.Text.Replace(",",""));
				elevDist.byUnits(nuElev, unitsCompl);

				altitude = elevDist.Meters;

				elevationUnitsLabel.Text = elevDist.toStringU(unitsCompl);

				elevLabel.ForeColor = Color.Black;
			}
			catch
			{
				elevLabel.ForeColor = Color.Red;
				allcool = false;
			}

			try 
			{
				Speed speedDist = new Speed(0.0d);
				int units = speedDist.Units;

				double nuSpeed = Convert.ToDouble(speedTextBox.Text.Replace(",",""));
				speedDist.byUnits(nuSpeed, units);

				speed = speedDist.Meters;

				speedUnitsLabel.Text = speedDist.toStringU(units);

				elevLabel.ForeColor = Color.Black;
			}
			catch
			{
				elevLabel.ForeColor = Color.Red;
				allcool = false;
			}

			try 
			{
				startTime = timePicker.isDateTimeValid ? timePicker.dateTime : DateTime.MinValue; //new DateTime(7000, 1, 1);
			}
			catch
			{
				allcool = false;
			}

			if(allcool)
			{
				result = true;
				this.Close();
			}
		}
	}
}
