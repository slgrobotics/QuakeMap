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
	/// Summary description for DlgMakeWaypoint.
	/// </summary>
	public class DlgMakeWaypoint : System.Windows.Forms.Form
	{
		public Waypoint m_wpt = null;		// in case the caller needs it

		protected CameraManager m_cameraManager;
		protected GeoCoord m_clickLocation;
		protected string m_latText;
		protected string m_lngText;
		protected string m_elevText;
		protected Point m_clickPoint;

		protected System.Windows.Forms.ComboBox waypointTypeComboBox;
		protected System.Windows.Forms.TextBox waypointNameTextBox;
		protected System.Windows.Forms.Label waypointNameLabel;
		protected System.Windows.Forms.Button goButton;
		protected System.Windows.Forms.TextBox elevationTextBox;
		protected System.Windows.Forms.Label elevLabel;
		protected System.Windows.Forms.TextBox longitudeTextBox;
		protected System.Windows.Forms.TextBox latitudeTextBox;
		protected System.Windows.Forms.Label lngLabel;
		protected System.Windows.Forms.Label latLabel;
		protected System.Windows.Forms.Button cancelButton;
		protected System.Windows.Forms.TextBox detailTextBox;
		protected System.Windows.Forms.Label label2;
		protected System.Windows.Forms.Label label4;
		protected System.Windows.Forms.TextBox urlTextBox;
		protected System.Windows.Forms.TextBox urlNameTextBox;
		protected System.Windows.Forms.Label label5;
		protected System.Windows.Forms.TextBox commentTextBox;
		protected System.Windows.Forms.Label label6;
		protected System.Windows.Forms.TextBox symbolTextBox;
		protected System.Windows.Forms.Label label7;
		protected System.Windows.Forms.Label typeLabel;
		protected System.Windows.Forms.Label coordFormatLabel;
		protected System.Windows.Forms.Label timeLabel;
		protected MyTimePicker timePicker;
		protected System.Windows.Forms.Label headerLabel;
		protected System.Windows.Forms.Label elevationUnitsLabel;
		protected System.Windows.Forms.Label speedUnitsLabel;
		protected System.Windows.Forms.TextBox speedTextBox;
		protected System.Windows.Forms.Label speedLabel;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgMakeWaypoint(CameraManager cameraManager)
		{
			m_cameraManager = cameraManager;

			InitializeComponent();

			// the time picker comes inactive, but if activated by button - current time will be displayed.
			timePicker = new MyTimePicker();
			timePicker.Location = new System.Drawing.Point(88, 136);
			timePicker.TabIndex = 6;
			this.Controls.Add(timePicker);

			waypointTypeComboBox.Items.Add("user waypoint");
			waypointTypeComboBox.Items.Add("geocache");
			waypointTypeComboBox.Items.Add("geocache found");
			waypointTypeComboBox.SelectedIndex = 0;
		}			

		public DlgMakeWaypoint(CameraManager cameraManager, Point clickPoint) : this(cameraManager)
		{
			m_clickPoint = clickPoint;

			m_clickLocation = m_cameraManager.toGeoLocation(m_clickPoint);
			setCoordFields(m_clickLocation);

			Project.setDlgIcon(this);
			waypointNameTextBox.Focus();
		}

		private PhotoDescr m_photoDescr = null;

		public DlgMakeWaypoint(CameraManager cameraManager, PhotoDescr photoDescr, Waypoint wpt) : this(cameraManager)
		{
			this.TopMost = true;

			if(wpt == null)
			{
				m_clickLocation = m_cameraManager.Location.Clone();
				m_clickLocation.Elev = 0.0d;
			}
			else
			{
				m_clickLocation = wpt.Location.Clone();
				elevLabel.Text = wpt.TrackId == -1 ? "Elevation:" : "Altitude";
			}
			setCoordFields(m_clickLocation);

			m_photoDescr = photoDescr;

			this.waypointNameTextBox.Text = m_photoDescr.imageName;
			this.urlTextBox.Text = m_photoDescr.imageUrl;
			this.detailTextBox.Text = m_photoDescr.imageSource;
			this.timePicker.dateTime = m_photoDescr.DTOrig;

			// all we can realistically change here is coordinates and elevation, so disable other conrols:
			this.symbolTextBox.Enabled = false;
			this.waypointNameTextBox.Enabled = false;
			this.waypointTypeComboBox.Enabled = false;
			this.urlNameTextBox.Enabled = false;
			this.urlTextBox.Enabled = false;
			this.detailTextBox.Enabled = false;
			this.commentTextBox.Enabled = false;
			//this.timePicker.Enabled = false;

			Project.setDlgIcon(this);
			waypointNameTextBox.Focus();
		}

		protected void setCoordFields(GeoCoord location)
		{
			speedLabel.Visible = false;
			speedTextBox.Visible = false;
			speedUnitsLabel.Visible = false;

			m_cameraManager.MarkLocation(location, 0);

			if(Project.coordStyle == 3)		// UTM?
			{
				coordFormatLabel.Text = "type like\r\n11S 432345E 3712345N";
				latLabel.Text = "* UTM:";
				lngLabel.Visible = false;
				longitudeTextBox.Visible = false;

				latitudeTextBox.Text = m_latText = location.ToString();
				m_lngText = longitudeTextBox.Text;	// will be a part of comparison later
			}
			else
			{
				coordFormatLabel.Text = "type like 117,33.355 -\r\nfirst comma, then dot";
				latLabel.Text = "* Latitude:";
				lngLabel.Visible = true;
				longitudeTextBox.Visible = true;

				latitudeTextBox.Text = m_latText = "" + GeoCoord.latToString(location.Lat, Project.coordStyle, true, true);
				longitudeTextBox.Text = m_lngText = "" + GeoCoord.lngToString(location.Lng, Project.coordStyle, true, true);
			}
			Distance elevDist = new Distance(location.Elev);
			int unitsCompl = elevDist.UnitsCompl;
			elevationTextBox.Text = m_elevText = elevDist.toStringN(unitsCompl);
			elevationUnitsLabel.Text = elevDist.toStringU(unitsCompl);
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
			this.waypointTypeComboBox = new System.Windows.Forms.ComboBox();
			this.typeLabel = new System.Windows.Forms.Label();
			this.waypointNameTextBox = new System.Windows.Forms.TextBox();
			this.waypointNameLabel = new System.Windows.Forms.Label();
			this.goButton = new System.Windows.Forms.Button();
			this.elevationUnitsLabel = new System.Windows.Forms.Label();
			this.elevationTextBox = new System.Windows.Forms.TextBox();
			this.elevLabel = new System.Windows.Forms.Label();
			this.longitudeTextBox = new System.Windows.Forms.TextBox();
			this.latitudeTextBox = new System.Windows.Forms.TextBox();
			this.lngLabel = new System.Windows.Forms.Label();
			this.latLabel = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.detailTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.urlTextBox = new System.Windows.Forms.TextBox();
			this.urlNameTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.commentTextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.symbolTextBox = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.coordFormatLabel = new System.Windows.Forms.Label();
			this.timeLabel = new System.Windows.Forms.Label();
			this.headerLabel = new System.Windows.Forms.Label();
			this.speedUnitsLabel = new System.Windows.Forms.Label();
			this.speedTextBox = new System.Windows.Forms.TextBox();
			this.speedLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// waypointTypeComboBox
			// 
			this.waypointTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.waypointTypeComboBox.Location = new System.Drawing.Point(88, 12);
			this.waypointTypeComboBox.Name = "waypointTypeComboBox";
			this.waypointTypeComboBox.Size = new System.Drawing.Size(153, 21);
			this.waypointTypeComboBox.TabIndex = 1;
			this.waypointTypeComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// typeLabel
			// 
			this.typeLabel.Location = new System.Drawing.Point(32, 12);
			this.typeLabel.Name = "typeLabel";
			this.typeLabel.Size = new System.Drawing.Size(44, 23);
			this.typeLabel.TabIndex = 13;
			this.typeLabel.Text = "Type:";
			this.typeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// waypointNameTextBox
			// 
			this.waypointNameTextBox.Location = new System.Drawing.Point(88, 64);
			this.waypointNameTextBox.Name = "waypointNameTextBox";
			this.waypointNameTextBox.Size = new System.Drawing.Size(152, 20);
			this.waypointNameTextBox.TabIndex = 3;
			this.waypointNameTextBox.Text = "";
			this.waypointNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// waypointNameLabel
			// 
			this.waypointNameLabel.Location = new System.Drawing.Point(16, 64);
			this.waypointNameLabel.Name = "waypointNameLabel";
			this.waypointNameLabel.Size = new System.Drawing.Size(56, 22);
			this.waypointNameLabel.TabIndex = 12;
			this.waypointNameLabel.Text = "* Name:";
			this.waypointNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// goButton
			// 
			this.goButton.Location = new System.Drawing.Point(320, 16);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(88, 24);
			this.goButton.TabIndex = 40;
			this.goButton.Text = "OK";
			this.goButton.Click += new System.EventHandler(this.goButton_Click);
			// 
			// elevationUnitsLabel
			// 
			this.elevationUnitsLabel.Location = new System.Drawing.Point(150, 216);
			this.elevationUnitsLabel.Name = "elevationUnitsLabel";
			this.elevationUnitsLabel.Size = new System.Drawing.Size(55, 22);
			this.elevationUnitsLabel.TabIndex = 18;
			this.elevationUnitsLabel.Text = "meters";
			this.elevationUnitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// elevationTextBox
			// 
			this.elevationTextBox.Location = new System.Drawing.Point(88, 216);
			this.elevationTextBox.Name = "elevationTextBox";
			this.elevationTextBox.Size = new System.Drawing.Size(57, 20);
			this.elevationTextBox.TabIndex = 9;
			this.elevationTextBox.Text = "";
			this.elevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// elevLabel
			// 
			this.elevLabel.Location = new System.Drawing.Point(16, 216);
			this.elevLabel.Name = "elevLabel";
			this.elevLabel.Size = new System.Drawing.Size(64, 22);
			this.elevLabel.TabIndex = 17;
			this.elevLabel.Text = "Elevation:";
			this.elevLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// longitudeTextBox
			// 
			this.longitudeTextBox.Location = new System.Drawing.Point(88, 192);
			this.longitudeTextBox.Name = "longitudeTextBox";
			this.longitudeTextBox.Size = new System.Drawing.Size(152, 20);
			this.longitudeTextBox.TabIndex = 8;
			this.longitudeTextBox.Text = "";
			this.longitudeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// latitudeTextBox
			// 
			this.latitudeTextBox.Location = new System.Drawing.Point(88, 168);
			this.latitudeTextBox.Name = "latitudeTextBox";
			this.latitudeTextBox.Size = new System.Drawing.Size(152, 20);
			this.latitudeTextBox.TabIndex = 7;
			this.latitudeTextBox.Text = "";
			this.latitudeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			this.latitudeTextBox.TextChanged += new System.EventHandler(this.latitudeTextBox_TextChanged);
			// 
			// lngLabel
			// 
			this.lngLabel.Location = new System.Drawing.Point(8, 192);
			this.lngLabel.Name = "lngLabel";
			this.lngLabel.Size = new System.Drawing.Size(73, 22);
			this.lngLabel.TabIndex = 15;
			this.lngLabel.Text = "* Longitude:";
			this.lngLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// latLabel
			// 
			this.latLabel.Location = new System.Drawing.Point(8, 168);
			this.latLabel.Name = "latLabel";
			this.latLabel.Size = new System.Drawing.Size(72, 21);
			this.latLabel.TabIndex = 19;
			this.latLabel.Text = "* Latitude:";
			this.latLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(320, 48);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(88, 24);
			this.cancelButton.TabIndex = 50;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// detailTextBox
			// 
			this.detailTextBox.AcceptsReturn = true;
			this.detailTextBox.Location = new System.Drawing.Point(8, 296);
			this.detailTextBox.Multiline = true;
			this.detailTextBox.Name = "detailTextBox";
			this.detailTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.detailTextBox.Size = new System.Drawing.Size(400, 64);
			this.detailTextBox.TabIndex = 32;
			this.detailTextBox.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(9, 272);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(400, 22);
			this.label2.TabIndex = 38;
			this.label2.Text = "Enter description text in the box below";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 248);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(58, 22);
			this.label4.TabIndex = 39;
			this.label4.Text = "URL:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// urlTextBox
			// 
			this.urlTextBox.Location = new System.Drawing.Point(88, 248);
			this.urlTextBox.Name = "urlTextBox";
			this.urlTextBox.Size = new System.Drawing.Size(320, 20);
			this.urlTextBox.TabIndex = 30;
			this.urlTextBox.Text = "http://";
			// 
			// urlNameTextBox
			// 
			this.urlNameTextBox.Location = new System.Drawing.Point(88, 88);
			this.urlNameTextBox.Name = "urlNameTextBox";
			this.urlNameTextBox.Size = new System.Drawing.Size(320, 20);
			this.urlNameTextBox.TabIndex = 4;
			this.urlNameTextBox.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 88);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(64, 22);
			this.label5.TabIndex = 41;
			this.label5.Text = "Url Name:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// commentTextBox
			// 
			this.commentTextBox.Location = new System.Drawing.Point(88, 112);
			this.commentTextBox.Name = "commentTextBox";
			this.commentTextBox.Size = new System.Drawing.Size(320, 20);
			this.commentTextBox.TabIndex = 5;
			this.commentTextBox.Text = "";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 112);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(64, 22);
			this.label6.TabIndex = 43;
			this.label6.Text = "Comment:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// symbolTextBox
			// 
			this.symbolTextBox.Location = new System.Drawing.Point(88, 40);
			this.symbolTextBox.Name = "symbolTextBox";
			this.symbolTextBox.Size = new System.Drawing.Size(152, 20);
			this.symbolTextBox.TabIndex = 2;
			this.symbolTextBox.Text = "";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 40);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(64, 22);
			this.label7.TabIndex = 45;
			this.label7.Text = "Symbol:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// coordFormatLabel
			// 
			this.coordFormatLabel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.coordFormatLabel.Location = new System.Drawing.Point(256, 168);
			this.coordFormatLabel.Name = "coordFormatLabel";
			this.coordFormatLabel.Size = new System.Drawing.Size(152, 48);
			this.coordFormatLabel.TabIndex = 51;
			this.coordFormatLabel.Text = "type like 117,33.355 - first comma, then dot";
			this.coordFormatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// timeLabel
			// 
			this.timeLabel.Location = new System.Drawing.Point(16, 136);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(64, 22);
			this.timeLabel.TabIndex = 53;
			this.timeLabel.Text = "Time:";
			this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// headerLabel
			// 
			this.headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.headerLabel.Location = new System.Drawing.Point(8, 8);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(304, 26);
			this.headerLabel.TabIndex = 54;
			this.headerLabel.Text = "headerLabel";
			this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.headerLabel.Visible = false;
			// 
			// speedUnitsLabel
			// 
			this.speedUnitsLabel.Location = new System.Drawing.Point(335, 216);
			this.speedUnitsLabel.Name = "speedUnitsLabel";
			this.speedUnitsLabel.Size = new System.Drawing.Size(75, 22);
			this.speedUnitsLabel.TabIndex = 57;
			this.speedUnitsLabel.Text = "meters/sec";
			this.speedUnitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// speedTextBox
			// 
			this.speedTextBox.Location = new System.Drawing.Point(275, 216);
			this.speedTextBox.Name = "speedTextBox";
			this.speedTextBox.Size = new System.Drawing.Size(57, 20);
			this.speedTextBox.TabIndex = 55;
			this.speedTextBox.Text = "";
			// 
			// speedLabel
			// 
			this.speedLabel.Location = new System.Drawing.Point(220, 216);
			this.speedLabel.Name = "speedLabel";
			this.speedLabel.Size = new System.Drawing.Size(50, 22);
			this.speedLabel.TabIndex = 56;
			this.speedLabel.Text = "Speed:";
			this.speedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DlgMakeWaypoint
			// 
			this.AcceptButton = this.goButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(418, 368);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.speedUnitsLabel,
																		  this.speedTextBox,
																		  this.speedLabel,
																		  this.headerLabel,
																		  this.timeLabel,
																		  this.coordFormatLabel,
																		  this.symbolTextBox,
																		  this.label7,
																		  this.commentTextBox,
																		  this.label6,
																		  this.urlNameTextBox,
																		  this.label5,
																		  this.label4,
																		  this.urlTextBox,
																		  this.label2,
																		  this.detailTextBox,
																		  this.cancelButton,
																		  this.elevationUnitsLabel,
																		  this.elevationTextBox,
																		  this.elevLabel,
																		  this.longitudeTextBox,
																		  this.latitudeTextBox,
																		  this.lngLabel,
																		  this.latLabel,
																		  this.goButton,
																		  this.waypointTypeComboBox,
																		  this.typeLabel,
																		  this.waypointNameTextBox,
																		  this.waypointNameLabel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgMakeWaypoint";
			this.Text = "Make Waypoint";
			this.ResumeLayout(false);

		}
		#endregion

		private void goButton_Click(object sender, System.EventArgs e)
		{
			act();
		}

		protected bool validateCoord(out double lng, out double lat, out double elev)
		{
			bool allcool = true;

			lng = 0.0d;
			lat = 0.0d;
			elev = 0.0d;

			if(Project.coordStyle == 3)		// UTM?
			{
				// something like "11S 0432345E 3712345N" in the latitudeTextBox
				allcool = Project.mainCommand.fromUtmString(latitudeTextBox.Text, out lng, out lat); 
				if(allcool)
				{
					latLabel.ForeColor = Color.Black;
				}
				else
				{
					latLabel.ForeColor = Color.Red;
				}
			}
			else
			{
				try 
				{
					lng = GeoCoord.stringLngToDouble(longitudeTextBox.Text);
					lngLabel.ForeColor = Color.Black;
				}
				catch
				{
					lngLabel.ForeColor = Color.Red;
					allcool = false;
				}
		
				try 
				{
					lat = GeoCoord.stringLatToDouble(latitudeTextBox.Text);
					latLabel.ForeColor = Color.Black;
				}
				catch
				{
					latLabel.ForeColor = Color.Red;
					allcool = false;
				}
			}

			try 
			{
				Distance elevDist = new Distance(0.0d);
				int unitsCompl = elevDist.UnitsCompl;

				double nuElev = Convert.ToDouble(elevationTextBox.Text.Replace(",",""));
				elevDist.byUnits(nuElev, unitsCompl);

				elev = elevDist.Meters;

				elevationUnitsLabel.Text = elevDist.toStringU(unitsCompl);

				elevLabel.ForeColor = Color.Black;
			}
			catch
			{
				elevLabel.ForeColor = Color.Red;
				allcool = false;
			}
			return allcool;
		}

		protected virtual void act()
		{
			double lng;
			double lat;
			double elev;

			bool allcool = validateCoord(out lng, out lat, out elev);

			if(waypointNameTextBox.Text.Length == 0) 
			{
				waypointNameLabel.ForeColor = Color.Red;
				allcool = false;
			}
			else
			{
				waypointNameLabel.ForeColor = Color.Black;
			}

			DateTime dateTime = timePicker.isActive ? Project.localToZulu(timePicker.dateTime) : DateTime.MinValue;

			if(allcool)
			{
				try 
				{
					GeoCoord location = m_clickLocation;
					if(!m_latText.Equals(latitudeTextBox.Text) || !m_lngText.Equals(longitudeTextBox.Text) || !m_elevText.Equals(elevationTextBox.Text))
					{
						location = new GeoCoord(lng, lat, elev);
					}
					location.Normalize();
					LiveObjectTypes type = LiveObjectTypes.LiveObjectTypeWaypoint;
					bool isFound = false;
					switch(waypointTypeComboBox.SelectedIndex)
					{
						case 0:
							type = LiveObjectTypes.LiveObjectTypeWaypoint;
							break;
						case 1:
							type = LiveObjectTypes.LiveObjectTypeGeocache;
							break;
						case 2:
							type = LiveObjectTypes.LiveObjectTypeGeocache;
							isFound = true;
							break;
					}
					string wptName = waypointNameTextBox.Text;
					Waypoint wpt = new Waypoint(location, dateTime, type, -1L, wptName, "", "");
					wpt.Found = isFound;	
					wpt.UrlName = urlNameTextBox.Text.Trim();
					wpt.Sym = symbolTextBox.Text.Trim();
					wpt.Comment = commentTextBox.Text.Trim();
					wpt.Desc = detailTextBox.Text.Trim();
					string url = urlTextBox.Text.Trim();
					if(url.Length > 0 && !url.Equals("http://"))		// if something meaningful has been entered
					{
						wpt.Url = url;
					}

					if(m_photoDescr != null)
					{
						wpt.ThumbSource = m_photoDescr.imageThumbSource;
						wpt.ThumbImage = PhotoDescr.rebuildThumbnailImage(wpt.ThumbSource);
						wpt.ThumbPosition = Project.thumbPosition;
						wpt.imageWidth = m_photoDescr.Width;
						wpt.imageHeight = m_photoDescr.Height;
						wpt.PhotoTimeShift = new TimeSpan(0L);
					}

					WaypointsCache.WaypointsAll.Add(wpt);
					WaypointsCache.WaypointsDisplayed.Add(wpt);
					WaypointsCache.isDirty = true;
					Project.drawWaypoints = true;

					m_wpt = wpt;	// in case the caller needs it

					m_cameraManager.MarkLocation(location, 0);
					m_cameraManager.PictureManager.LayersManager.ShowWaypoints = true;
					m_cameraManager.ProcessCameraMove();
					this.Close();
				}
				catch
				{
					elevLabel.ForeColor = Color.Red;
					allcool = false;
				}
			}
		}

		private void anyTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyData) 
			{
				case Keys.Enter:
					act();
					break;
			}
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void latitudeTextBox_TextChanged(object sender, System.EventArgs e)
		{
		}
	}
}
