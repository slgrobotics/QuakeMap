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
using System.IO;
using System.Text;

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgEditWaypoint.
	/// </summary>
	public class DlgEditWaypoint : DlgMakeWaypoint
	{
		public DlgEditWaypoint(CameraManager cameraManager, Waypoint wpt) : base(cameraManager)
		{
			m_wpt = wpt;

			if(m_wpt.TrackId == -1)
			{
				this.Text = "Edit Waypoint";
				elevLabel.Text = "Elevation:";
			}
			else
			{
				elevLabel.Text = "Altitude:";
				Track trk = (Track)Project.mainCommand.getTrackById(m_wpt.TrackId);
				if(trk != null)
				{
					if(trk.isRoute)
					{
						this.Text = "Edit Route Point";
					}
					else
					{
						this.Text = "Edit Trackpoint";
					}
				}
			}

			GeoCoord location = m_wpt.Location;
			setCoordFields(location);

			setFormFields();

			Project.setDlgIcon(this);

			if(waypointNameTextBox.Enabled)
			{
				waypointNameTextBox.Focus();
			}
			else
			{
				urlNameTextBox.Focus();
			}
		}

		protected void setFormFields()
		{
			if(m_wpt.LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache)
			{
				waypointTypeComboBox.SelectedIndex = m_wpt.Found ? 2 : 1;
			}
			else
			{
				waypointTypeComboBox.SelectedIndex = 0;
			}

			waypointNameTextBox.Text = m_wpt.WptName;
			urlNameTextBox.Text = m_wpt.UrlName;
			urlTextBox.Text = (m_wpt.Url.Trim().Length == 0) ? "http://" : m_wpt.Url.Trim();
			commentTextBox.Text = m_wpt.Comment;
			symbolTextBox.Text = m_wpt.Sym;
			timePicker.dateTime = Project.zuluToLocal(m_wpt.DateTime);

			if(m_wpt.TrackId != -1)
			{
				headerLabel.Visible = true;
				headerLabel.Text = m_wpt.trackInfoString();
				coordFormatLabel.Visible = false;
				waypointNameTextBox.Enabled = false;
				waypointNameTextBox.Text = m_wpt.Name;
				waypointTypeComboBox.Visible = false;
				typeLabel.Visible = false;
				//timePicker.Enabled = false;
				longitudeTextBox.Enabled = false;
				latitudeTextBox.Enabled = false;
				Track trk = (Track)Project.mainCommand.getTrackById(m_wpt.TrackId);
				if(trk != null)
				{
					if(!trk.isRoute)
					{
						// do not allow track elevation editing
						elevationTextBox.Enabled = false;
					}
				}
				else
				{
					elevationTextBox.Enabled = false;
				}

				if(m_wpt.ThumbImage != null)
				{
					// can't edit photo points, they are transient
					coordFormatLabel.Text = "Can't edit photo point";
					urlNameTextBox.Enabled = false;
					commentTextBox.Enabled = false;
					symbolTextBox.Enabled = false;
					urlTextBox.Enabled = false;
					detailTextBox.Enabled = false;
					goButton.Enabled = false;
					cancelButton.Focus();
				}
				else
				{
					detailTextBox.Focus();
				}

				if(m_wpt.HasSpeed)
				{
					speedLabel.Visible = true;
					speedTextBox.Visible = true;
					Speed dSpeed = new Speed(m_wpt.Speed);				// meters per hour
					string speedLbl = dSpeed.toStringN(Project.unitsDistance);
					speedTextBox.Text = speedLbl;
					speedUnitsLabel.Visible = true;
					speedUnitsLabel.Text = dSpeed.toStringU(Project.unitsDistance);
				}
				else
				{
					speedLabel.Visible = false;
					speedTextBox.Visible = false;
					speedUnitsLabel.Visible = false;
				}
			}
			else
			{
				speedLabel.Visible = false;
				speedTextBox.Visible = false;
				speedUnitsLabel.Visible = false;

				waypointNameTextBox.Focus();
			}

			try 
			{
				StringReader reader = new StringReader(m_wpt.Desc);

				string str;
				StringBuilder sb = new StringBuilder();
				while((str=reader.ReadLine()) != null)
				{
					sb.Append(str);
					sb.Append("\r\n");
				}
				detailTextBox.Text = sb.ToString().Trim();
			} 
			catch {}
		}

		protected override void act()
		{
			double lng;
			double lat;
			double elev;

			bool allcool = validateCoord(out lng, out lat, out elev);

			if(m_wpt.TrackId == -1)
			{
				if(waypointNameTextBox.Text.Length == 0) 
				{
					waypointNameLabel.ForeColor = Color.Red;
					allcool = false;
				}
				else
				{
					waypointNameLabel.ForeColor = Color.Black;
				}
			}

			if(allcool)
			{
				try 
				{
					GeoCoord location = new GeoCoord(lng, lat, elev);
					location.Normalize();
					if(m_wpt.TrackId == -1)
					{
						switch(waypointTypeComboBox.SelectedIndex)
						{
							case 0:
								m_wpt.LiveObjectType = LiveObjectTypes.LiveObjectTypeWaypoint;
								m_wpt.Found = false;
								break;
							case 1:
								m_wpt.LiveObjectType = LiveObjectTypes.LiveObjectTypeGeocache;
								m_wpt.Found = false;
								break;
							case 2:
								m_wpt.LiveObjectType = LiveObjectTypes.LiveObjectTypeGeocache;
								m_wpt.Found = true;
								break;
						}
						m_wpt.WptName = waypointNameTextBox.Text.Trim();
					}
					m_wpt.Location = location;
					if(timePicker.isActive)
					{
						m_wpt.DateTime = Project.localToZulu(timePicker.dateTime);
					}
					else
					{
						m_wpt.DateTime = DateTime.MinValue;
					}
					m_wpt.UrlName = urlNameTextBox.Text.Trim();
					string url = urlTextBox.Text.Trim();
					if(url.Equals("http://"))		// if something meaningful has been entered
					{
						url = "";
					}
					m_wpt.Url = url;
					m_wpt.Comment = commentTextBox.Text.Trim();
					m_wpt.Sym = symbolTextBox.Text.Trim();
					m_wpt.Desc = detailTextBox.Text.Trim();
					WaypointsCache.isDirty = true;
					Project.drawWaypoints = true;
					//this.Hide();
					this.ClientSize = new Size(1,1);
					Cursor.Current = Cursors.WaitCursor;
					m_cameraManager.PictureManager.LayersManager.ShowWaypoints = true;
					m_cameraManager.ProcessCameraMove();	// need to put on map
					this.Close();
				}
				catch
				{
					elevLabel.ForeColor = Color.Red;
					allcool = false;
				}
			}
		}
	}
}
