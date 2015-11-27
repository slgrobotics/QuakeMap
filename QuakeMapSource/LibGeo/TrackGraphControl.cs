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

namespace LibGeo
{
	/// <summary>
	/// Summary description for TrackGraphControl.
	/// </summary>
	public class TrackGraphControl : GraphControl
	{
		private Track m_track;
		public Track Track { get { return m_track; } }
		
		private Waypoint m_trkpt;

		private double m_elevMax = 0.0d;		// meters
		private double m_elevMin = 0.0d;
		private double m_speedMax = -1000.0d;	// miles per hour

		public IEnableDisable enableDisable;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TrackGraphControl()
		{
		}

		public void setTrackAndTrackpoint(Track trk, Waypoint trkpt)
		{
			m_track = trk;
			m_trkpt = trkpt;

			rebuildGraph();
		}

		private void rebuildGraph()
		{
			if(m_track == null)
			{
				m_allItems = null;
				m_graphItems = null;
				this.Invalidate();

				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			m_elevMax = -100000.0d;	// feet
			m_elevMin =  100000.0d;
			m_speedMax = -1000.0d;

			SortedList graphItems = new SortedList();
			Waypoint prevWpt = null;
			double odometer = 0.0d;		// meters

			for(int i=0; i < m_track.Trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)m_track.Trackpoints.GetByIndex(i);

				Speed speed = null;
				Distance leg = null;
				double dSpeedCurrentUnits = 0.0d;

				if(prevWpt != null)
				{
					// compute odometer and speed:
					TimeSpan dt = wpt.DateTime - prevWpt.DateTime;
					leg = wpt.Location.distanceFrom(prevWpt.Location);	// meters
					double legMeters = leg.Meters;
					odometer += legMeters;
					double legMetersPerHour = legMeters * 36000000000.0d / dt.Ticks;		// meters per hour
					if(!m_track.isRoute && legMetersPerHour > 1000.0d && legMetersPerHour < 330.0d * 3600.0d)	// sanity check - speed of sound, m/hr
					{
						speed = new Speed(legMetersPerHour);
						dSpeedCurrentUnits = speed.ToDouble();
					}
				}
				odometer = (float)odometer;

				long id = (long)i;
				double elevCurrentUnits = new Distance(wpt.Location.Elev).ToDoubleCompl();
				string properties = wpt.toTableString();

				m_elevMax = Math.Max(elevCurrentUnits, m_elevMax);
				m_elevMin = Math.Min(elevCurrentUnits, m_elevMin);
				m_speedMax = Math.Max(dSpeedCurrentUnits, m_speedMax);

				long[] ids = new long[2];
				ids[0] = id;
				ids[1] = id;
				double[] values = new double[2];
				values[0] = elevCurrentUnits;
				values[1] = dSpeedCurrentUnits;
				string[] sValues = new string[2];
				sValues[0] = properties;
				sValues[1] = "";

				GraphItem ev = new GraphItem(odometer, wpt, values, sValues, ids, 2);

				while(true)
				{
					try
					{
						graphItems.Add(odometer, ev);
						break;
					}
					catch
					{
						odometer += 0.01d;
					}
				}
				prevWpt = wpt;
			}

			if(m_elevMax > 0.0d && m_speedMax > 0.0d)
			{
				// make speed graph match the elevation:
				double speedFactor = 0.6d * m_elevMax / m_speedMax;
				foreach(GraphItem ev in graphItems.Values)
				{
					double[] values = ev.values(Project.FORMAT_TRACK_ELEVATION);
					values[1] = Math.Round(values[1] * speedFactor);
				}
			}

			string selHint = "Click into grey area to see selected interval";
			StringById dZoomById = new StringById(zoomById); 
			StringById dDescrById = new StringById(descrById); 

			double rounder = 200.0d;
			if(this.m_elevMax > 10000.0d)
			{
				rounder = 2000.0d;
			}
			else if(this.m_elevMax > 1000.0d)
			{
				rounder = 1000.0d;
			}
			else if(this.m_elevMax <= 50.0d)
			{
				this.m_elevMax = 50.0d;
				rounder = 100.0d;
			}
			double graphGridMaxValue = Math.Ceiling(this.m_elevMax / rounder) * rounder;
			int steps = (int) (graphGridMaxValue / rounder);
			this.MaxValueY = graphGridMaxValue;
			this.MinKeyDblMargin = 100.0d;
			this.StepY = graphGridMaxValue / steps;

			this.MarginLeft = 45;
			this.MarginRight = 30;

			this.initialHint = "Click on graph to see details. Use arrow keys. Drag mouse to select a "
									+ (m_track.isRoute ? "route" : "track") + " segment, click into it to zoom.";

			this.init(this.enableDisable, "", "", selHint, dDescrById, dZoomById, dBrowseById, new MethodInvoker(showSelected));
			this.setGraphData(graphItems, Project.FORMAT_TRACK_ELEVATION, 2, false);
			this.SelectItemByObject(m_trkpt);

			this.resetLegends();
			this.setLegend(0, "elevation");
			this.setLegend(1, "speed");

			this.Invalidate();

			Cursor.Current = Cursors.Default;
		}

		private void showSelectedButton_Click(object sender, System.EventArgs e)
		{
			showSelected();
		}

		private void showSelected()
		{
			//scopeSelectedGraphRadioButton.Checked = true;
			this.GraphSelectedArea();
			this.Focus();
		}

		private static StringById dBrowseById = new StringById(browseById); 

		public static string browseById(long id)
		{
			string ret = "";
			/*
			Earthquake eq = EarthquakesCache.getEarthquakeById(id);
			if(eq != null)
			{
				string url = eq.Url;
				if(url != null && url.StartsWith("http://"))
				{
					ret = url;
					Project.RunBrowser(url);
				}
			}
			*/
			return ret;		// just info
		}

		public string descrById(long id)
		{
			string ret = "";

//			Waypoint wpt = WaypointsCache.getTrackpointByWptId((int)id);
//			m_lastGraphWpt = wpt;
//			if(wpt != null)
//			{
//				string sTime;
//				if(Project.useUtcTime)
//				{
//					DateTime time =  wpt.DateTime;
//					string format = "HH:mm:ss";
//					sTime = time.ToLongDateString() + " " + time.ToString(format) + " UTC";
//				}
//				else
//				{
//					DateTime time = Project.zuluToLocal(wpt.DateTime);
//					sTime = time.ToLongDateString() + " " + time.ToLongTimeString();
//				}
//
//				ret = sTime + " -- " + wpt.graphInfoString();
//
//				GeoCoord loc = new GeoCoord(wpt.Location.X, wpt.Location.Y, m_cameraManager.Elev);
//				m_cameraManager.MarkLocation(loc, 0);
//			}
//			setZoomButtons();
			return ret;
		}

		public string zoomById(long id)
		{
			string ret = "";
//			Waypoint wpt = WaypointsCache.getTrackpointByWptId((int)id);
//			m_lastGraphWpt = wpt;
//			if(wpt != null)
//			{
//				GeoCoord newLoc = new GeoCoord(wpt.Location.X, wpt.Location.Y, m_cameraManager.Elev);
//				m_cameraManager.Location = newLoc;
//				ret = "" + newLoc;
//			}
//			setZoomButtons();
			return ret;		// just info
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
			// 
			// TrackGraphControl
			// 
			this.Name = "TrackGraphControl";
			this.Size = new System.Drawing.Size(800, 120);

		}
		#endregion
	}
}
