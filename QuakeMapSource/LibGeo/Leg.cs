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
using System.Globalization;
using System.Xml;
using System.Drawing;
using System.Text;

using LibSys;

namespace LibGeo
{
	/// <summary>
	/// Leg represents travel between two routepoints/trackpoints and serves to ease calculations for drawing, mouse over etc.
	/// </summary>
	public class Leg
	{
		private Waypoint m_wptFrom;
		private Waypoint m_wptTo;
		private Track m_track;

		public Waypoint WptFrom	{ get { return m_wptFrom; } }
		public Waypoint WptTo	{ get { return m_wptTo; } }

		public Point MiddlePoint	{
			get {
				// keep in mind that PixelLocation is stored once PutOnMap had been called for those waypoints, so there is no CPU waste here.
				return new Point((m_wptFrom.PixelLocation.X + m_wptTo.PixelLocation.X) / 2, (m_wptFrom.PixelLocation.Y + m_wptTo.PixelLocation.Y) / 2);
			}
		}

		private Distance m_dist;
		public Distance Dist { get { return m_dist; } }

		private TimeSpan m_duration;
		public TimeSpan Duration { get { return m_duration; } }

		protected float		m_speed = 1000000001.0f;		// meters per hour
		public float        Speed { get { return m_speed; } set { m_speed = value; } }
		public bool         HasSpeed { get { return m_speed <= 1000000000.0f; }  set { m_speed = 1000000001.0f; } }

		protected static double minDateTimeTicks = DateTime.MinValue.AddDays(100.0d).Ticks;

		public Leg(Track track, Waypoint wptFrom, Waypoint wptTo)
		{
			m_track = track;

			m_wptFrom = wptFrom;
			m_wptTo = wptTo;

			m_dist = m_wptTo.distanceFrom(m_wptFrom.Location);		// it is a new Distance()
			m_duration = wptTo.DateTime - wptFrom.DateTime;

			if(m_wptFrom.DateTime.Ticks > minDateTimeTicks && m_wptTo.DateTime.Ticks > minDateTimeTicks)
				// waypoint times are added to to avoid duplicates, we need to compare with tolerance
			{
				this.Speed = (float)(Dist.Meters / m_duration.TotalHours);
			}
		}

		public void split(GeoCoord midLoc)
		{
			GeoCoord midLocWithAlt = new GeoCoord(midLoc);
			midLocWithAlt.Elev = m_wptFrom.Location.Elev;

			m_track.splitLeg(this, midLocWithAlt);
		}

		#region isPointOver() for leg hover-over

		/// <summary>
		/// for hover-over, calculates if a point belongs to vicinity of the leg
		/// </summary>
		/// <param name="p"></param>
		public bool isPointOver(Point point) //, Graphics g)
		{
			bool ret = false;

			int sw = 10;	// pixels, sensitivity margin to each side

			Point p1, p2;	// p1 is always to the left of p2. Keep in mind that axis Y points down on the screen.

			int toX = WptTo.PixelLocation.X;
			int toY = WptTo.PixelLocation.Y;
			
			int fromX = WptFrom.PixelLocation.X;
			int fromY = WptFrom.PixelLocation.Y;

			if(toX >= fromX)
			{
				p1 = new Point(fromX, fromY);
				p2 = new Point(toX, toY);
			}
			else
			{
				p1 = new Point(toX, toY);
				p2 = new Point(fromX, fromY);
			}

			Point p1a, p1b, p2a, p2b;

			if(p2.Y >= p1.Y)
			{
				p1a = new Point(p1.X, p1.Y + sw);
				p1b = new Point(p1.X + sw, p1.Y);
				p2a = new Point(p2.X - sw, p2.Y);
				p2b = new Point(p2.X, p2.Y - sw);
			}
			else
			{
				p1a = new Point(p1.X + sw, p1.Y);
				p1b = new Point(p1.X, p1.Y - sw);
				p2a = new Point(p2.X, p2.Y + sw);
				p2b = new Point(p2.X - sw, p2.Y);
			}

//			Point[] points = new Point[] { p1, p1a, p2a, p2, p2b, p1b };
//			g.DrawPolygon(Pens.Yellow, points);

			Rectangle rect = new Rectangle(p1.X, Math.Min(fromY, toY), p2.X - p1.X, Math.Abs(fromY - toY));
			bool flatRect = false;

			if(rect.Width < sw || rect.Height < sw)
			{
				rect.Inflate(sw, sw);
				//rect.Offset(-sw, -sw);
				flatRect = true;
			}

			if(rect.Contains(point) && (flatRect || above(point, p1a, p2a) && !above(point, p2b, p1b)))
			{
				ret = true;
			}

			return ret;
		}

		/// <summary>
		/// true if point p is above the line that goes through p1, p2
		/// </summary>
		/// <param name="p"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		private bool above(Point p, Point a, Point b)
		{
			if(b.X == a.X)
			{
				return true;
			}
			else
			{
				// equation describes a line going through two points
				return (a.Y - p.Y + (b.Y - a.Y) * (p.X - a.X) / (b.X - a.X)) > 0;
			}
		}

		#endregion // isPointOver() for leg hover-over

		public override string ToString()
		{
			return "Leg: " + m_dist + " in " + Project.TimeSpanToString(m_duration);
		}

		#region toStringPopup()

		/// <summary>
		/// returns a formatted string suitable for popups
		/// </summary>
		/// <returns></returns>
		public string toStringPopup()
		{
			StringBuilder builder = new StringBuilder();
			string speedLbl = null;
			string odometerLbl = null;
			string timeTraveledLbl = null;
			string timeThisLegLbl = null;
			string timeRemainingLbl = null;

			if(m_wptFrom.TrackId != -1)
			{
				Track trk = (Track)Project.mainCommand.getTrackById(m_wptFrom.TrackId);
				Distance dOdometer;

				if(trk != null)
				{
					int i = trk.Trackpoints.IndexOfValue(this);
					builder.Append("Leg : " + m_wptFrom.Name + " --> " + m_wptTo.Name);
					builder.Append("\nof " + (trk.isRoute ? "Route" : "Track") + m_wptFrom.TrackId + " : " + trk.Name + "\n");

					if(HasSpeed)
					{
						Speed dSpeed = new Speed(this.Speed);				// meters per hour
						speedLbl = dSpeed.ToString();
					}

					TimeSpan ts = m_wptFrom.DateTime - trk.Start;
					timeTraveledLbl = Project.TimeSpanToString(ts);

					timeThisLegLbl = Project.TimeSpanToString(m_duration);

					ts = trk.End - m_wptTo.DateTime;
					timeRemainingLbl = Project.TimeSpanToString(ts);

					dOdometer = new Distance(m_wptFrom.Odometer);	// meters
					odometerLbl = dOdometer.ToString() + " plus this leg: " + Dist.ToString();
				}
			}
			builder.Append(m_wptFrom.Location.toString00(false, Project.coordStyle));
			Distance dElev = new Distance(m_wptFrom.Location.Elev);
			if(dElev.Meters != 0.0d)
			{
				builder.Append("  elev: " + dElev.ToStringCompl() + "\n");
			}

			if(m_wptFrom.DateTime.Ticks > minDateTimeTicks)
				// waypoint times are added to to avoid duplicates, we need to compare with tolerance
			{
				builder.Append("\n  " + Project.zuluToLocal(m_wptFrom.DateTime));
			}
			if(odometerLbl != null)
			{
				builder.Append((speedLbl == null ? "\n" : "  ") +  "odometer: " + odometerLbl);
			}
			if(speedLbl != null)
			{
				builder.Append("\n  speed: " + speedLbl);
			}
			builder.Append("\n time");
			if(timeTraveledLbl != null)
			{
				builder.Append(" traveled: " + timeTraveledLbl);
			}
			builder.Append(" this leg: " + timeThisLegLbl);
			if(timeRemainingLbl != null)
			{
				builder.Append("   remaining: " + timeRemainingLbl);
			}

			addCourseData(builder, false);

			return builder.ToString();
		}

		private void addCourseData(StringBuilder builder, bool isHtml)
		{
			string boldStart = isHtml ? "<b>" : "";
			string boldEnd = isHtml ? "</b>" : "";
			string newline = isHtml ? "<br/>\n" : "\n";

			double variation = Math.Round(m_wptFrom.Location.magneticVariation());
			string variationDir = variation > 0.0d ? "W" : "E";
			double headingTrue = Math.Round(m_wptFrom.Location.bearing(m_wptTo.Location) * 180.0d / Math.PI, 1);
			double headingMagn = (headingTrue + variation + 360.0d) % 360.0d;

			double dAlt = m_wptTo.Location.Elev - m_wptFrom.Location.Elev;	// meters
			double dDist = m_wptFrom.distanceFrom(m_wptTo.Location).Meters;
			double dSlope = Math.Atan(dAlt / dDist) * 180.0d / Math.PI;
			double dTimeMs = (m_wptTo.DateTime - m_wptFrom.DateTime).TotalMilliseconds + 1.0d;		// 1.0d to avoid 0
			double fpm = dAlt / dTimeMs / Distance.METERS_PER_FOOT * 1000.0d * 60.0d;
			string fpmFormat = Math.Abs(fpm) > 20.0d ? "4:F0" : "4:F1";

			if(fpm > 1.0d || dSlope >= 0.1d)
			{
				builder.Append(String.Format("{0}{1}Slope:{2} {3:F1}° (up) - climbing at {" + fpmFormat + "} fpm {0}", newline, boldStart, boldEnd, dSlope, fpm));
			}
			else if(fpm < -1.0d || dSlope <= -0.1d)
			{
				builder.Append(String.Format("{0}{1}Slope:{2} {3:F1}° (down) - descending at {" + fpmFormat + "} fpm {0}", newline, boldStart, boldEnd, -dSlope, -fpm));
			} 
			else //if(!isHtml)
			{
				builder.Append(newline);
			}
			builder.Append(String.Format("{1}Heading: true:{2} {3:000}° {1}magnetic:{2} {4:000}°  (var {5:F0}{6})", newline, boldStart, boldEnd,
				Math.Round(headingTrue), Math.Round(headingMagn), Math.Abs(variation), variationDir));
		}

		#endregion // toStringPopup()
	}
}
