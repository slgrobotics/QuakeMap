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
	/// Summary description for GotoForm.
	/// </summary>
	public class GotoForm : System.Windows.Forms.Form
	{
		protected CameraManager m_cameraManager;
		private System.Windows.Forms.Button goButton;
		private System.Windows.Forms.TextBox latitudeTextBox;
		private System.Windows.Forms.TextBox longitudeTextBox;
		private System.Windows.Forms.TextBox elevationTextBox;
		private System.Windows.Forms.Label latLabel;
		private System.Windows.Forms.Label elevLabel;
		private System.Windows.Forms.Label lngLabel;
		private System.Windows.Forms.CheckBox moveCameraCheckBox;
		private System.Windows.Forms.CheckBox makeWaypointCheckBox;
		private System.Windows.Forms.TextBox waypointNameTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox waypointTypeComboBox;
		private System.Windows.Forms.Label waypointNameLabel;
		private System.Windows.Forms.Label coordFormatLabel;
		protected System.Windows.Forms.Label elevationUnitsLabel;

		private System.ComponentModel.Container components = null;

		public GotoForm(CameraManager cameraManager)
		{
			m_cameraManager = cameraManager;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			waypointTypeComboBox.Items.Add("user waypoint");
			waypointTypeComboBox.Items.Add("geocache");
			waypointTypeComboBox.Items.Add("geocache found");
			waypointTypeComboBox.SelectedIndex = 0;
			
			moveCameraCheckBox.Checked = true;

			setCoordTextBoxes();

			goButton.Select();
			goButton.Focus();
			Project.setDlgIcon(this);
		}

		private void setCoordTextBoxes()
		{
			m_cameraManager.MarkLocation(m_cameraManager.Location, 0);
			if(Project.coordStyle == 3)		// UTM?
			{
				coordFormatLabel.Text = "type like 11S 432345E 3712345N";
				latLabel.Text = "UTM:";
				lngLabel.Visible = false;
				longitudeTextBox.Visible = false;
				latitudeTextBox.Text = m_cameraManager.Location.ToString();
			}
			else
			{
				coordFormatLabel.Text = (Project.coordStyle == 0) ? 
													"type like 117,33'35 - first comma, then quote"
												:	"type like 117,33.355 - first comma, then dot";
				latLabel.Text = "Latitude:";
				lngLabel.Visible = true;
				longitudeTextBox.Visible = true;
				latitudeTextBox.Text = "" + GeoCoord.latToString(m_cameraManager.Location.Lat, Project.coordStyle, true, true);
				longitudeTextBox.Text = "" + GeoCoord.lngToString(m_cameraManager.Location.Lng, Project.coordStyle, true, true);
			}
			Distance elevDist = new Distance(m_cameraManager.Location.Elev);
			int unitsCompl = elevDist.UnitsCompl;
			elevationTextBox.Text = elevDist.toStringN(unitsCompl);
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
			this.goButton = new System.Windows.Forms.Button();
			this.latLabel = new System.Windows.Forms.Label();
			this.lngLabel = new System.Windows.Forms.Label();
			this.latitudeTextBox = new System.Windows.Forms.TextBox();
			this.longitudeTextBox = new System.Windows.Forms.TextBox();
			this.elevLabel = new System.Windows.Forms.Label();
			this.elevationTextBox = new System.Windows.Forms.TextBox();
			this.moveCameraCheckBox = new System.Windows.Forms.CheckBox();
			this.makeWaypointCheckBox = new System.Windows.Forms.CheckBox();
			this.waypointNameLabel = new System.Windows.Forms.Label();
			this.waypointNameTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.waypointTypeComboBox = new System.Windows.Forms.ComboBox();
			this.coordFormatLabel = new System.Windows.Forms.Label();
			this.elevationUnitsLabel = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// goButton
			// 
			this.goButton.Location = new System.Drawing.Point(305, 35);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(85, 30);
			this.goButton.TabIndex = 7;
			this.goButton.Text = "Go There";
			this.goButton.Click += new System.EventHandler(this.goButton_Click);
			// 
			// latLabel
			// 
			this.latLabel.Location = new System.Drawing.Point(30, 30);
			this.latLabel.Name = "latLabel";
			this.latLabel.Size = new System.Drawing.Size(58, 21);
			this.latLabel.TabIndex = 0;
			this.latLabel.Text = "Latitude:";
			this.latLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lngLabel
			// 
			this.lngLabel.Location = new System.Drawing.Point(15, 55);
			this.lngLabel.Name = "lngLabel";
			this.lngLabel.Size = new System.Drawing.Size(73, 22);
			this.lngLabel.TabIndex = 0;
			this.lngLabel.Text = "Longitude:";
			this.lngLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// latitudeTextBox
			// 
			this.latitudeTextBox.Location = new System.Drawing.Point(95, 30);
			this.latitudeTextBox.Name = "latitudeTextBox";
			this.latitudeTextBox.Size = new System.Drawing.Size(139, 20);
			this.latitudeTextBox.TabIndex = 0;
			this.latitudeTextBox.Text = "";
			this.latitudeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// longitudeTextBox
			// 
			this.longitudeTextBox.Location = new System.Drawing.Point(95, 55);
			this.longitudeTextBox.Name = "longitudeTextBox";
			this.longitudeTextBox.Size = new System.Drawing.Size(139, 20);
			this.longitudeTextBox.TabIndex = 1;
			this.longitudeTextBox.Text = "";
			this.longitudeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// elevLabel
			// 
			this.elevLabel.Location = new System.Drawing.Point(30, 90);
			this.elevLabel.Name = "elevLabel";
			this.elevLabel.Size = new System.Drawing.Size(58, 20);
			this.elevLabel.TabIndex = 0;
			this.elevLabel.Text = "Elevation:";
			this.elevLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// elevationTextBox
			// 
			this.elevationTextBox.Location = new System.Drawing.Point(95, 90);
			this.elevationTextBox.Name = "elevationTextBox";
			this.elevationTextBox.Size = new System.Drawing.Size(95, 20);
			this.elevationTextBox.TabIndex = 2;
			this.elevationTextBox.Text = "";
			this.elevationTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// moveCameraCheckBox
			// 
			this.moveCameraCheckBox.Location = new System.Drawing.Point(275, 90);
			this.moveCameraCheckBox.Name = "moveCameraCheckBox";
			this.moveCameraCheckBox.Size = new System.Drawing.Size(124, 23);
			this.moveCameraCheckBox.TabIndex = 3;
			this.moveCameraCheckBox.Text = "move camera";
			// 
			// makeWaypointCheckBox
			// 
			this.makeWaypointCheckBox.Location = new System.Drawing.Point(260, 25);
			this.makeWaypointCheckBox.Name = "makeWaypointCheckBox";
			this.makeWaypointCheckBox.Size = new System.Drawing.Size(110, 22);
			this.makeWaypointCheckBox.TabIndex = 5;
			this.makeWaypointCheckBox.Text = "make waypoint";
			// 
			// waypointNameLabel
			// 
			this.waypointNameLabel.Location = new System.Drawing.Point(20, 25);
			this.waypointNameLabel.Name = "waypointNameLabel";
			this.waypointNameLabel.Size = new System.Drawing.Size(44, 22);
			this.waypointNameLabel.TabIndex = 7;
			this.waypointNameLabel.Text = "Name:";
			this.waypointNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// waypointNameTextBox
			// 
			this.waypointNameTextBox.Location = new System.Drawing.Point(80, 25);
			this.waypointNameTextBox.Name = "waypointNameTextBox";
			this.waypointNameTextBox.Size = new System.Drawing.Size(139, 20);
			this.waypointNameTextBox.TabIndex = 4;
			this.waypointNameTextBox.Text = "";
			this.waypointNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(15, 55);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 23);
			this.label3.TabIndex = 9;
			this.label3.Text = "Type:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.waypointTypeComboBox,
																					this.label3,
																					this.waypointNameTextBox,
																					this.waypointNameLabel,
																					this.makeWaypointCheckBox});
			this.groupBox1.Location = new System.Drawing.Point(15, 125);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(380, 90);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Make Waypoint";
			// 
			// waypointTypeComboBox
			// 
			this.waypointTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.waypointTypeComboBox.Location = new System.Drawing.Point(80, 55);
			this.waypointTypeComboBox.Name = "waypointTypeComboBox";
			this.waypointTypeComboBox.Size = new System.Drawing.Size(153, 21);
			this.waypointTypeComboBox.TabIndex = 6;
			this.waypointTypeComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.anyTextBox_KeyDown);
			// 
			// coordFormatLabel
			// 
			this.coordFormatLabel.Location = new System.Drawing.Point(5, 5);
			this.coordFormatLabel.Name = "coordFormatLabel";
			this.coordFormatLabel.Size = new System.Drawing.Size(350, 22);
			this.coordFormatLabel.TabIndex = 12;
			this.coordFormatLabel.Text = "type like 117,33.355 - first comma, then dot";
			this.coordFormatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// elevationUnitsLabel
			// 
			this.elevationUnitsLabel.Location = new System.Drawing.Point(195, 90);
			this.elevationUnitsLabel.Name = "elevationUnitsLabel";
			this.elevationUnitsLabel.Size = new System.Drawing.Size(68, 20);
			this.elevationUnitsLabel.TabIndex = 19;
			this.elevationUnitsLabel.Text = "meters";
			this.elevationUnitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// GotoForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(404, 223);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.elevationUnitsLabel,
																		  this.coordFormatLabel,
																		  this.groupBox1,
																		  this.moveCameraCheckBox,
																		  this.elevationTextBox,
																		  this.elevLabel,
																		  this.longitudeTextBox,
																		  this.latitudeTextBox,
																		  this.lngLabel,
																		  this.latLabel,
																		  this.goButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GotoForm";
			this.Text = "Move Camera / Make Waypoint";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void goButton_Click(object sender, System.EventArgs e)
		{
			act();
		}

		private void act()
		{
			double lng = 0.0d;
			double lat = 0.0d;
			double elev = 0.0d;

			bool allcool = true;

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

				if(elev < Project.cameraHeightMin*1000.0d)
				{
					elevDist.byUnits(Project.cameraHeightMin*1000.0d, Distance.UNITS_DISTANCE_M);
					elev = elevDist.Meters;
				}
				else if(elev > Project.CAMERA_HEIGHT_MAX*1000.0d)
				{
					elevDist.byUnits(Project.CAMERA_HEIGHT_MAX*1000.0d, Distance.UNITS_DISTANCE_M);
					elev = elevDist.Meters;
				}

				elevationUnitsLabel.Text = elevDist.toStringU(unitsCompl);

				elevLabel.ForeColor = Color.Black;
			}
			catch
			{
				elevLabel.ForeColor = Color.Red;
				allcool = false;
			}

			if(makeWaypointCheckBox.Checked && waypointNameTextBox.Text.Length == 0) 
			{
				waypointNameLabel.ForeColor = Color.Red;
				allcool = false;
			}
			else
			{
				waypointNameLabel.ForeColor = Color.Black;
			}

			if(allcool)
			{
				try 
				{
					GeoCoord location = new GeoCoord(lng, lat, elev);
					location.Normalize();
					m_cameraManager.MarkLocation(location, 0);
					if(makeWaypointCheckBox.Checked) 
					{
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
						string stype = "" + waypointTypeComboBox.SelectedItem;
						string comment = waypointNameTextBox.Text;
						Waypoint wpt = new Waypoint(location, Project.localToZulu(DateTime.Now), type, -1L, comment, "", "");
						wpt.Found = isFound;	
						WaypointsCache.WaypointsAll.Add(wpt);
						WaypointsCache.WaypointsDisplayed.Add(wpt);
						WaypointsCache.isDirty = true;
						Project.drawWaypoints = true;
						m_cameraManager.PictureManager.LayersManager.ShowWaypoints = true;
						if(!moveCameraCheckBox.Checked)
						{
							Cursor.Current = Cursors.WaitCursor;
						}
						m_cameraManager.PictureManager.Refresh();
					}
					if(moveCameraCheckBox.Checked) 
					{
						Cursor.Current = Cursors.WaitCursor;
						m_cameraManager.SpoilPicture();
						m_cameraManager.Location = new GeoCoord(location);		// must be a new instance of GeoCoord
						setCoordTextBoxes();
						Cursor.Current = Cursors.Default;
					}
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
	}
}
