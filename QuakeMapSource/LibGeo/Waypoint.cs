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
	public class Waypoint : LabeledPoint, IComparable
	{
		#region variables defined here

		// we need to have a handy Waypoint object for GetType() comparisons:
		public static Waypoint waypoint = new Waypoint();
		public static Type getType() { return Waypoint.waypoint.GetType(); }

		public static int nextWaypointId = 0;	// used to assign unique m_id for every new waypoint 

		protected int		m_id;
		public int			Id { get { return m_id; } }

		protected DateTime	m_dateTime;		// always UTC
		public DateTime		DateTime { get { return m_dateTime; } set { m_dateTime = value; } }

		protected string	m_source = "";
		public string		Source { get { return m_source; } set { m_source = value; } }

		protected string	m_url = "";
		public string		Url { get { return m_url; } set { m_url = value; } }

		protected string	m_urlName = "";
		public string		UrlName { get { return m_urlName; } set { m_urlName = value; } }

		// getLabel is generally called on labels layout, and it will know to use Project.waypointNameStyle
		public string		NameDisplayed { get { return m_name; } set { m_name = value; Name = getLabel(true); } }

		// this is the shortest possible name for maps, "GCSHGFFX" being example for geocaches
		protected string	m_wptName = "";
		public string		WptName  { get { return m_wptName; } set { m_wptName = value; } }

		// geocache GUID, "5fa5702d-e9f1-42f1-893e-b20fc100692d" being example
		protected string	m_wptGuid = null;
		public string		WptGuid  { get { return m_wptGuid; } set { m_wptGuid = value; } }

		// in most cases type-related processing is associated with LiveObjectType property (kind of "wpt", "rtept", "trkpt", "geocache", "earthquake"...)
		// if some descriptive type comes from the source ("geocache|Traditional Geocache") - WptType is the place to put it.
		protected string	m_wptType = "";
		public string		WptType  { get { return m_wptType; } set { m_wptType = value; } }

		// detail corresponds to <desc> tag in GPX and may contain several lines worth of descriptive text, useful for popups etc.
		protected string	m_desc = "";
		public string		Desc  { get { return m_desc; } set { m_desc = value; } }

		// comment may come from source and generally is appended to detail in popups
		protected string	m_comment = "";
		public string		Comment { get { return m_comment; } set { m_comment = value; } }

		// Sym is an icon name, "Dot" being the popular default
		protected string	m_sym = "";
		public string		Sym { get { return m_sym; } set { m_sym = value; } }

		// photo-related members:
//		protected string	m_imageSource = null;
//		public string		ImageSource { get { return m_imageSource; } set { m_imageSource = value; } }
		protected Image		m_thumbImage = null;
		public Image		ThumbImage { get { return m_thumbImage; } set { m_thumbImage = value; } }
		protected string	m_thumbSource = null;
		public string		ThumbSource { get { return m_thumbSource; } set { m_thumbSource = value; } }
		protected int		m_thumbPosition = 0;
		public int			ThumbPosition { get { return m_thumbPosition; } set { m_thumbPosition = value; } }
		public TimeSpan		m_photoTimeShift;
		public TimeSpan		PhotoTimeShift { get { return m_photoTimeShift; } set { m_photoTimeShift = value; } }
		public int			imageWidth = 0;
		public int			imageHeight = 0;

		// if cache info came from PocketQuery as part of <groundspeak:cache> tag, here is where it ends:
		public GroundspeakCache groundspeakCache = null;
		protected bool		m_found = false;
		public bool         Found { get { return m_found; } set { m_found = value; } }

		// track-related members:
		protected long		m_trackId = -1L;
		public long         TrackId { get { return m_trackId; } set { m_trackId = value; } }
		public bool			isEndpoint = false;

		protected float		m_speed = 1000000001.0f;		// meters per hour
		public float        Speed { get { return m_speed; } set { m_speed = value; } }
		public bool         HasSpeed { get { return m_speed <= 1000000000.0f; }  set { m_speed = 1000000001.0f; } }

		protected float		m_odometer = 1000000001.0f;		// meters
		public float        Odometer { get { return m_odometer; } set { m_odometer = value; } }
		public bool         HasOdometer { get { return m_odometer <= 1000000000.0f; } }

		protected bool		m_forceShow = false;
		public bool         ForceShow { get { return m_forceShow; } set { m_forceShow = value; } }

		public bool hasContent { get { return m_desc.Length > 0 || m_urlName.Length > 0; } }

		public Leg			LegTo = null;
		public Leg			LegFrom = null;

		public string		DetailAny	// tries best to return at least some detail text for Project.InfoBox 
		{
			get 
			{
				string txt = "Detail:\n\n";
				//if(Desc.Length == 0)
				//{
					txt += toStringPopup();
				//}
				//else
				//{
				//	txt += Desc;
				//}
				return txt;
			}
		}

		#endregion // variables defined here

		#region comparison methods
 
		public int CompareTo(object other)
		{
			Waypoint wptOther = (Waypoint)other;
			return m_dateTime.CompareTo(wptOther.DateTime);
		}

		public bool sameAs(Waypoint other)
		{
			return Location.sameAs(other.Location) && Name.Equals(other.Name);
		}

		#endregion // comparison methods

		#region constructors and lifecycle

		/// <summary>
		/// default constructor, allocates good new id for the waypoint
		/// </summary>
		public Waypoint()
		{
			m_id = nextWaypointId++;
			setBrushes();
		}

		/// <summary>
		/// clone constructor
		/// </summary>
		/// <param name="wpt"></param>
		public Waypoint(Waypoint wpt)
		{
			m_id = nextWaypointId++;

			m_location = new GeoCoord(wpt.Location);
			m_dateTime = wpt.DateTime;		// always UTC; value type is copied
			m_trackId = wpt.TrackId;
			m_forceShow = wpt.ForceShow;
			m_found = wpt.Found;
			LiveObjectType = wpt.LiveObjectType;
			m_name = wpt.m_name;
			m_wptName = wpt.m_wptName;
			m_wptType = wpt.m_wptType;
			m_sym = wpt.m_sym;
			m_comment = wpt.m_comment;
			m_desc = wpt.m_desc;
			m_source = wpt.m_source;
			m_url = wpt.m_url;
			m_urlName = wpt.m_urlName;
			m_thumbImage = wpt.ThumbImage == null ? null : new Bitmap(wpt.ThumbImage);
			m_thumbSource = wpt.ThumbSource;
			m_thumbPosition = wpt.ThumbPosition;
			m_photoTimeShift = wpt.PhotoTimeShift;
			groundspeakCache = wpt.groundspeakCache;	// by reference

			setBrushes();
		}
		
		public Waypoint(Earthquake eq)
		{
			m_id = nextWaypointId++;

			m_location = new GeoCoord(eq.Location);
			m_dateTime = eq.DateTime;		// always UTC; value type is copied
			LiveObjectType = LiveObjectTypes.LiveObjectTypeWaypoint; //eq.LiveObjectType;
			m_name = eq.Name;
			m_url = eq.Url;
			m_wptType = "earthquake";
			m_desc = eq.Comment + " (" + eq.Source + ")";
			m_source = eq.Source;
			m_wptName = String.Format("{0:F1}", eq.Magn);	//eq.Name;
			m_urlName = eq.Name;
			//m_comment = eq.Comment;
			m_sym = "earthquake";

			setBrushes();
		}
		
		/// <summary>
		/// constructor with the most common parameters set
		/// </summary>
		/// <param name="loc"></param>
		/// <param name="dateTime"></param>
		/// <param name="liveObjectType"></param>
		/// <param name="trackId"></param>
		/// <param name="comment"></param>
		/// <param name="source"></param>
		/// <param name="url"></param>
		public Waypoint(GeoCoord loc, DateTime dateTime, LiveObjectTypes liveObjectType, long trackId, string name, string source, string url)
		{
			m_id = nextWaypointId++;
			
			m_location = loc;
			m_dateTime = dateTime;		// always UTC
			LiveObjectType = liveObjectType;
			m_trackId = trackId;
			m_name = name;
			m_wptName = name;
			m_source = source;
			m_url = url;

			Name = getLabel(false);

			setBrushes();
		}

		/// <summary>
		/// full constructor for InsertWaypoint type of action
		/// </summary>
		/// <param name="createInfo"></param>
		public Waypoint(CreateInfo createInfo)
		{
			m_id = nextWaypointId++;
			
			m_dateTime = createInfo.dateTime;
			m_location = new GeoCoord(createInfo.lng, createInfo.lat, createInfo.elev);

			if(createInfo.typeExtra == null)
			{
				switch(createInfo.type)	// null, "wpt", "rtept", "trkpt", "geocache", "earthquake"
				{
					default:
					case "earthquake":
					case "wpt":
						LiveObjectType = LiveObjectTypes.LiveObjectTypeWaypoint;
						break;
					case "geocache":
						LiveObjectType = LiveObjectTypes.LiveObjectTypeGeocache;
						break;
					case "rtept":
						LiveObjectType = LiveObjectTypes.LiveObjectTypeRoutepoint;
						break;
					case "trkpt":
						LiveObjectType = LiveObjectTypes.LiveObjectTypeTrackpoint;
						break;
				}
			}
			else
			{
				string typeExtra = createInfo.typeExtra.ToLower().Trim();		// can be "geocache found" or "geocache|traditional cache|found"
				if(typeExtra.IndexOf("geocache") != -1)
				{
					LiveObjectType = LiveObjectTypes.LiveObjectTypeGeocache;
				}
				else
				{
					switch(createInfo.type)	// null, "wpt", "rtept", "trkpt"
					{
						default:
						case "wpt":
							LiveObjectType = LiveObjectTypes.LiveObjectTypeWaypoint;
							break;
						case "rtept":
							LiveObjectType = LiveObjectTypes.LiveObjectTypeRoutepoint;
							break;
						case "trkpt":
							LiveObjectType = LiveObjectTypes.LiveObjectTypeTrackpoint;
							break;
					}
				}
				if(typeExtra.IndexOf("found") != -1)
				{
					m_found = true;
				}
				m_wptType = createInfo.typeExtra.Trim();
			}

			m_sym = createInfo.sym == null ? "" : createInfo.sym;
			m_desc = createInfo.desc == null ? "" : createInfo.desc;
			m_wptName = createInfo.name == null ? "" : createInfo.name;
			m_comment = createInfo.comment == null ? "" : createInfo.comment;
			m_trackId = createInfo.id;
			m_url = createInfo.url == null ? "" : createInfo.url;
			m_wptGuid = extractGuid(createInfo.url);					// pocket queries have GUID-based URLs, get guid for Tools sring
			m_urlName = createInfo.urlName == null ? "" : createInfo.urlName;
			m_source = createInfo.source == null ? "" : createInfo.source;
			if(createInfo.node1 != null)
			{
				groundspeakCache = new GroundspeakCache(createInfo.node1);
			}

			Name = getLabel(false);

			setBrushes();
		}

		private string extractGuid(string url)
		{
			string ret = null;
			int pos;

			if(url != null && (pos=url.IndexOf("guid=")) != -1)
			{
				ret = url.Substring(pos+5);
				if((pos=ret.IndexOf("&")) != -1)
				{
					ret = ret.Substring(0, pos);
				}
			}

			return ret;
		}

		public void setBrushes()
		{
			brushFont = Project.waypointFontBrush;
			brushBackground = Project.waypointBackgroundBrush;
			penMain = Project.waypointPen;
			penUnderline = Project.waypointPen;
		}

		#endregion // constructors and lifecycle

		#region ToString() variations

		public override string ToString()
		{
			return Location + " " + (m_dateTime.Equals(DateTime.MinValue) ? "" : Project.zuluToLocal(m_dateTime).ToString()) + " type=" + LiveObjectTypeToString(this.LiveObjectType) + " comment='" + m_comment + "' src=" + m_source + " url=" + m_url;
		}

		public string ToString2()
		{
			return Location + " " + (m_dateTime.Equals(DateTime.MinValue) ? "" : Project.zuluToLocal(m_dateTime).ToString()) + ((m_source != null && m_source.Length > 0) ? " src=" + m_source : (" " + LiveObjectTypeToString(this.LiveObjectType)));
		}

		public override string toTableString()
		{
			return ToString();
		}

		public string ToStringProfile()
		{
			StringBuilder builder = new StringBuilder();
			Waypoint nextWpt = null;
			string speedLbl = null;
			string odometerLbl = null;
			string timeTraveledLbl = null;
			string timeRemainingLbl = null;

			bool doTime = true;
			if(this.Desc != null && this.Desc.Length > 0 && !this.Desc.Equals(this.Name))
			{
				builder.Append(this.Desc.Trim() + "  ");
			}
			if(this.TrackId != -1)
			{
				Track trk = (Track)Project.mainCommand.getTrackById(this.TrackId);
				if(trk != null)
				{
					int i = trk.Trackpoints.IndexOfValue(this);
					nextWpt = (i < trk.Trackpoints.Count-1) ? (Waypoint)trk.Trackpoints.GetByIndex(i+1) : null;
					string name = this.Name;
					if(trk.isRoute)
					{
						builder.Append(name + " | ");
						doTime = false;
						if(HasOdometer)
						{
							Distance dOdometer = new Distance(this.Odometer);	// meters
							odometerLbl = dOdometer.ToString();
						}
					}
					else
					{
						builder.Append(name + " | ");
						if(HasSpeed)
						{
							Speed dSpeed = new Speed(this.Speed);				// meters per hour
							speedLbl = dSpeed.ToString();
						}
						TimeSpan ts = this.DateTime - trk.Start;
						timeTraveledLbl = Project.TimeSpanToString(ts);

						ts = trk.End - this.DateTime;
						timeRemainingLbl = Project.TimeSpanToString(ts);
					}
					if(HasOdometer)
					{
						Distance dOdometer = new Distance(this.Odometer);	// meters
						odometerLbl = dOdometer.ToString();
					}
				}
			}
			builder.Append(this.Location.toString00(false, Project.coordStyle));
			Distance dElev = new Distance(this.Location.Elev);
			if(dElev.Meters != 0.0d)
			{
				builder.Append("  elev: " + dElev.ToStringCompl() + " ");
			}

			if(doTime && this.DateTime.Ticks > minDateTimeTicks)
				// waypoint times are added to to avoid duplicates, we need to compare with tolerance
			{
				builder.Append(" " + Project.zuluToLocal(this.DateTime));
			}
			if(speedLbl != null)
			{
				builder.Append("  speed: " + speedLbl);
			}
			if(odometerLbl != null)
			{
				builder.Append("  odometer: " + odometerLbl);
			}
			if(timeTraveledLbl != null)
			{
				builder.Append("  time traveled: " + timeTraveledLbl);
			}
			if(timeRemainingLbl != null)
			{
				builder.Append("  time remaining: " + timeRemainingLbl);
			}
			if(nextWpt != null && !this.Location.sameAs(nextWpt.Location))
			{
				addCourseData(builder, nextWpt, false);
			}
			if(this.WptName != null && this.WptName.Length > 0)
			{
				builder.Append(" Name: " + this.WptName.Trim());
			}
			if(this.TrackId == -1 && this.LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache && this.DateTime.Ticks > minDateTimeTicks)
			{
				builder.Append("      Hidden: " + Project.zuluToLocal(this.DateTime).ToShortDateString());
			}
			if(this.UrlName != null && this.UrlName.Length > 0)
			{
				builder.Append(" Url Name: " + this.UrlName.Trim());
			}
			if(this.Comment != null && this.Comment.Length > 0)
			{
				builder.Append(" Comment: " + this.Comment.Trim());
			}
			if(this.ThumbImage != null && this.ThumbSource != null && this.ThumbSource.Length > 0)
			{
				string sTimeShift  = toDelayString(PhotoTimeShift);
				builder.Append((imageWidth > 0 && imageHeight > 0 ? (" " + imageWidth + "x" + imageHeight + "       ") : " "));
				builder.Append("camera time shift: " + sTimeShift + " ");
				builder.Append(this.ThumbSource);
			}

			builder.Replace("\n", " ");

			return builder.ToString();
		}

		#endregion // ToString() variations

		#region toStringKmlDescr() and related helpers

		private void addKmlCoords(StringBuilder builder, bool addNewlines)
		{
			string sep = addNewlines ? "<br/>" : "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";

			int coordStyle = Project.coordStyle;
			if(Project.coordStyle == 3)
			{
				coordStyle = 1;
			}
			builder.Append("<b>Loc:</b> " + this.Location.toString00(false, coordStyle) + sep);
			builder.Append("<b>UTM:</b> " + this.Location.toString0(false, 3) + sep);

			Distance dElev = new Distance(this.Location.Elev);
			if(dElev.Meters != 0.0d)
			{
				builder.Append("<b>Altitude:</b> " + dElev.ToStringCompl());
			}

			if(dElev.Meters == 0.0d || !addNewlines)
			{
				builder.Append("<br/>\n");
			}
		}

		private void addCourseData(StringBuilder builder, Waypoint nextWpt, bool isHtml)
		{
			string boldStart = isHtml ? "<b>" : "";
			string boldEnd = isHtml ? "</b>" : "";
			string newline = isHtml ? "<br/>\n" : "\n";

			double variation = Math.Round(this.Location.magneticVariation());
			string variationDir = variation > 0.0d ? "W" : "E";
			double headingTrue = Math.Round(this.Location.bearing(nextWpt.Location) * 180.0d / Math.PI, 1);
			double headingMagn = (headingTrue + variation + 360.0d) % 360.0d;

			double dAlt = nextWpt.Location.Elev - this.Location.Elev;	// meters
			double dDist = this.distanceFrom(nextWpt.Location).Meters;
			double dSlope = Math.Atan(dAlt / dDist) * 180.0d / Math.PI;
			double dTimeMs = (nextWpt.DateTime - this.DateTime).TotalMilliseconds + 1.0d;		// 1.0d to avoid 0
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

/*
		private const string m_aaa = @"<b>Trackpoint</b>&nbsp;=&nbsp;248&nbsp;of&nbsp;1415<br/>
<b>Latitude</b>&nbsp;=&nbsp;31.6947°<br/>
<b>Longitude</b>&nbsp;=&nbsp;-110.86196°<br/>
<b>UTM</b>&nbsp;=&nbsp;12R 513082E 3506603N<br/>
<b>Heading</b>&nbsp;=&nbsp;132.7°<br/>
<b>Slope</b>&nbsp;=&nbsp;7.25% (up)<br/>
<b>Distance&nbsp;Completed</b>&nbsp;=&nbsp;21%<br/>
<b>Distance&nbsp;Traveled</b>&nbsp;=&nbsp;2.5&nbsp;miles<br/>
<b>Distance&nbsp;Remaining</b>&nbsp;=&nbsp;9.3&nbsp;miles<br/>
<b>Net&nbsp;Distance</b>&nbsp;=&nbsp;1.5&nbsp;miles<br/>
<b>Net&nbsp;Distance&nbsp;Remaining</b>&nbsp;=&nbsp;1.5&nbsp;miles</td>

<td><b>Altitude</b>&nbsp;=&nbsp;6919&nbsp;ft<br/>
<b>Altitude&nbsp;Completed</b>&nbsp;=&nbsp;36%<br/>
<b>Net&nbsp;Altitude</b>&nbsp;=&nbsp;1503&nbsp;ft<br/>
<b>Net&nbsp;Altitude&nbsp;Remaining</b>&nbsp;=&nbsp;2630&nbsp;ft<br/>
<b>Time&nbsp;Completed</b>&nbsp;=&nbsp;4%<br/>
<b>Time&nbsp;Elapsed</b>&nbsp;=&nbsp;01:58:59<br/>
<b>Time&nbsp;Remaining</b>&nbsp;=&nbsp;44:20:21<br/>
<b>Ground&nbsp;Speed</b>&nbsp;=&nbsp;0.8989&nbsp;mph<br/>
<b>Moving&nbsp;Average</b>&nbsp;=&nbsp;1.345&nbsp;mph<br/>
<b>Vertical&nbsp;Speed</b>&nbsp;=&nbsp;0.096&nbsp;ft/sec&nbsp;(up)<br/>
<b>Acceleration</b>&nbsp;=&nbsp;-0.009&nbsp;ft/sec/sec";
*/

		/// <summary>
		/// returns a formatted string suitable for popups
		/// </summary>
		/// <returns></returns>
		public string toStringKmlDescr()
		{
			StringBuilder builder = new StringBuilder();

			if(this.ThumbImage != null)
			{
				if(this.UrlName != null && this.UrlName.Length > 0)
				{
					builder.Append("<b>Url Name:</b> " + this.UrlName.Trim() + "<br/>");
				}
				if(this.DateTime.Ticks > minDateTimeTicks)
				// waypoint times are added to to avoid duplicates, we need to compare with tolerance
				{
					builder.Append("<b>Taken:</b> " + Project.zuluToLocal(this.DateTime) + "<br/>");
				}
				if(this.Comment != null && this.Comment.Length > 0)
				{
					builder.Append("<b>Comment:</b> " + this.Comment.Trim() + "<br/>");
				}

				addKmlCoords(builder, false);

				builder.Append("<img src='images/" + m_name + ".jpg' width='" + imageWidth + "' height='" + imageHeight + "'><br/>");
			}
			else
			{
				builder.Append(m_startTable);
				builder.Append(m_startTr1);
//				builder.Append(m_aaa);
//				builder.Append(m_endTable);

				Waypoint nextWpt = null;
				string speedLbl = null;
				string odometerLbl = null;
				string timeTraveledLbl = null;
				string timeRemainingLbl = null;

				if(this.Desc != null && this.Desc.Length > 0)
				{
					builder.Append(this.Desc.Replace("\n","<br/>\n").Trim() + "<br/>\n");
				}
				if(this.Url != null && this.Url.Length > 0)
				{
					builder.Append(String.Format("<a href=\"{0}\">{0}</a><br/>\n", this.Url));
				}
				if(this.TrackId != -1)
				{
					Track trk = (Track)Project.mainCommand.getTrackById(this.TrackId);
					if(trk != null)
					{
						int i = trk.Trackpoints.IndexOfValue(this);
						nextWpt = (i < trk.Trackpoints.Count-1) ? (Waypoint)trk.Trackpoints.GetByIndex(i+1) : null;
						string pointOfLabel = "<b>Point:</b> " + (i+1) + " of " + trk.Trackpoints.Count + "<br/>\n";

						if(trk.isRoute)
						{
							builder.Append("<b>Route:</b> " + trk.Name + "<br/>\n");
							builder.Append(pointOfLabel);
						}
						else
						{
							builder.Append("<b>Track:</b> " + trk.Name + "<br/>\n");
							builder.Append(pointOfLabel);
							if(this.DateTime.Ticks > minDateTimeTicks)
								// waypoint times are added to to avoid duplicates, we need to compare with tolerance
							{
								builder.Append("<b>Time:</b> " + Project.zuluToLocal(this.DateTime) + "<br/>\n");
							}
							if(HasSpeed)
							{
								Speed dSpeed = new Speed(this.Speed);				// meters per hour
								speedLbl = dSpeed.ToString();
							}
							TimeSpan ts = this.DateTime - trk.Start;
							timeTraveledLbl = Project.TimeSpanToString(ts);

							ts = trk.End - this.DateTime;
							timeRemainingLbl = Project.TimeSpanToString(ts);
						}
						if(HasOdometer)
						{
							Distance dOdometer = new Distance(this.Odometer);		// meters
							odometerLbl = dOdometer.ToString() + " of " + (new Distance(trk.Odometer)).ToString() + "<br/>";
						}
					}
				}

				addKmlCoords(builder, true);

				if(nextWpt != null && !this.Location.sameAs(nextWpt.Location))
				{
					addCourseData(builder, nextWpt, true);
				}

				if(this.TrackId != -1)
				{
					builder.Append(m_endTr);
					builder.Append(m_startTr2);

					if(speedLbl != null)
					{
						builder.Append("<b>Speed:</b> " + speedLbl + "<br/>");
					}
					if(odometerLbl != null)
					{
						builder.Append("<b>Distance:</b> " + odometerLbl + "<br/>");
					}

					builder.Append(m_nextTd);
				
					if(timeTraveledLbl != null)
					{
						builder.Append("<b>Time traveled:</b> " + timeTraveledLbl + "<br/>");
					}
					if(timeRemainingLbl != null)
					{
						builder.Append("<b>Time remaining:</b> " + timeRemainingLbl + "<br/>");
					}

					builder.Append(m_endTr);
					builder.Append(m_startTr1);
				}

				if(this.TrackId == -1)
				{
					string strLoType = this.LiveObjectTypeToString();
					builder.Append("<b>Type:</b> " + strLoType + (this.Found ? " - FOUND" : "") + " ");
				}
				if(m_wptType.Length > 0)
				{
					builder.Append(" " + m_wptType + (this.Found ? " - FOUND" : "") + " ");
				}

				if(this.TrackId == -1 && this.LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache && this.DateTime.Ticks > minDateTimeTicks)
				{
					builder.Append("<b>Hidden:</b> " + Project.zuluToLocal(this.DateTime).ToShortDateString() + "<br/>");
				}
				if(this.UrlName != null && this.UrlName.Length > 0)
				{
					builder.Append("<b>Url Name:</b> " + this.UrlName.Trim() + "<br/>");
				}
				if(this.Sym != null && this.Sym.Length > 0)
				{
					builder.Append("<b>Symbol:</b> " + this.Sym.Trim() + "<br/>");
				}
				if(this.Comment != null && this.Comment.Length > 0)
				{
					builder.Append("<b>Comment:</b> " + this.Comment.Trim() + "<br/>");
				}
				if(groundspeakCache != null)
				{
					builder.Append("<br/>" + groundspeakCache.ToStringPopup().Replace("\n","<br>\n"));
				}

				builder.Append(m_endTr);
				builder.Append(m_endTable);
			}

			builder.Replace("<br><br>","<br>");
			builder.Replace("<br>\n<br>","<br>");

			return builder.ToString();
		}

		#endregion // toStringKmlDescr() and related helpers

		#region toStringPopup()

		/// <summary>
		/// returns a formatted string suitable for popups
		/// </summary>
		/// <returns></returns>
		public string toStringPopup()
		{
			StringBuilder builder = new StringBuilder();
			Waypoint nextWpt = null;
			string speedLbl = null;
			string odometerLbl = null;
			//Waypoint prevWpt = null;
			string timeTraveledLbl = null;
			string timeRemainingLbl = null;

			bool doTime = this.DateTime.Ticks > minDateTimeTicks;

			if(this.Desc != null && this.Desc.Length > 0)
			{
				builder.Append(this.Desc.Trim() + "\n\n");
			}
			if(this.Url != null && this.Url.Length > 0)
			{
				builder.Append(Project.serverAvailable ? "[click waypoint or label to browse]\n\n" : "[can't browse offline]\n\n");
			}
			if(this.TrackId != -1)
			{
				Track trk = (Track)Project.mainCommand.getTrackById(this.TrackId);
				if(trk != null)
				{
					int i = trk.Trackpoints.IndexOfValue(this);
					nextWpt = (i < trk.Trackpoints.Count-1) ? (Waypoint)trk.Trackpoints.GetByIndex(i+1) : null;
					string name = " : " + this.Name;
					if(name.Length > 7)
					{
						name = "";
					}

					if(builder.Length == 0)
					{
						builder.Append("Waypoint on ");
					}

					builder.Append((trk.isRoute ? "Route " : "Track ") + this.TrackId + ": " + trk.Name + name + "\n");

					if(HasSpeed)
					{
						Speed dSpeed = new Speed(this.Speed);				// meters per hour
						speedLbl = dSpeed.ToString();
					}
					TimeSpan ts = this.DateTime - trk.Start;
					timeTraveledLbl = Project.TimeSpanToString(ts);

					ts = trk.End - this.DateTime;
					timeRemainingLbl = Project.TimeSpanToString(ts);

					if(HasOdometer)
					{
						Distance dOdometer = new Distance(this.Odometer);	// meters
						odometerLbl = dOdometer.ToString();
					}
				}
			}
			builder.Append(this.Location.toString00(false, Project.coordStyle));
			Distance dElev = new Distance(this.Location.Elev);
			if(dElev.Meters != 0.0d)
			{
				builder.Append("  elev: " + dElev.ToStringCompl() + "\n");
			}

			if(this.TrackId == -1)
			{
				string strLoType = this.LiveObjectTypeToString();
				builder.Append("Type: (" + strLoType + (this.Found ? " - FOUND" : "") + ")");
			}
			if(m_wptType.Length > 0)
			{
				builder.Append(" (" + m_wptType + (this.Found ? " - FOUND" : "") + ")");
			}
			if(doTime && this.DateTime.Ticks > minDateTimeTicks)
				// waypoint times are added to to avoid duplicates, we need to compare with tolerance
			{
				builder.Append("\n  " + Project.zuluToLocal(this.DateTime));
			}
			if(speedLbl != null)
			{
				builder.Append("\n  speed: " + speedLbl);
			}
			if(odometerLbl != null)
			{
				builder.Append((speedLbl == null ? "\n" : "  ") +  "odometer: " + odometerLbl);
			}
			if(timeTraveledLbl != null)
			{
				builder.Append("\n time traveled: " + timeTraveledLbl);
			}
			if(timeRemainingLbl != null)
			{
				builder.Append("  time remaining: " + timeRemainingLbl);
			}
			if(nextWpt != null && !this.Location.sameAs(nextWpt.Location))
			{
				addCourseData(builder, nextWpt, false);
			}
			if(this.WptName != null && this.WptName.Length > 0)
			{
				builder.Append("\nName: " + this.WptName.Trim());
			}
			if(this.TrackId == -1 && this.LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache && this.DateTime.Ticks > minDateTimeTicks)
			{
				builder.Append("      Hidden: " + Project.zuluToLocal(this.DateTime).ToShortDateString());
			}
			if(this.UrlName != null && this.UrlName.Length > 0)
			{
				builder.Append("\nUrl Name: " + this.UrlName.Trim());
			}
			if(this.Sym != null && this.Sym.Length > 0)
			{
				builder.Append("\nSymbol: " + this.Sym.Trim());
			}
			if(this.Comment != null && this.Comment.Length > 0)
			{
				builder.Append("\nComment: " + this.Comment.Trim());
			}
			if(groundspeakCache != null)
			{
				builder.Append("\n\n" + groundspeakCache.ToStringPopup());
			}
			if(this.ThumbImage != null && this.ThumbSource != null && this.ThumbSource.Length > 0)
			{
				string sTimeShift  = toDelayString(PhotoTimeShift);
				builder.Append((imageWidth > 0 && imageHeight > 0 ? ("\n" + imageWidth + "x" + imageHeight + "       ") : "\n"));
				builder.Append("camera time shift: " + sTimeShift + "\n");
				builder.Append(this.ThumbSource);
			}
			if(LegTo != null)
			{
				builder.Append("\nIncoming " + this.LegTo);
			}
			if(LegFrom != null)
			{
				builder.Append("\nOutgoing " + this.LegFrom);
			}

			return builder.ToString();
		}

		#endregion // toStringPopup()

		#region some lesser toString metods

		protected static double minDateTimeTicks = DateTime.MinValue.AddDays(100.0d).Ticks;

		/// <summary>
		/// short info string about the track trackpoint belongs to, needed to DlgEditWaypoint dialog
		/// </summary>
		/// <returns></returns>
		public string trackInfoString()
		{
			string tmp = "";

			string speedLbl = null;
			string odometerLbl = null;
			
			Track trk = (Track)Project.mainCommand.getTrackById(this.TrackId);
			if(trk != null)
			{
				if(trk.isRoute)
				{
					tmp += "Route " + this.TrackId + ": " + trk.Name;
					if(HasOdometer)
					{
						Distance dOdometer = new Distance(this.Odometer);	// meters
						odometerLbl = dOdometer.ToString();
					}
				}
				else
				{
					tmp += "Track " + this.TrackId + ": " + trk.Name;
					if(HasSpeed)
					{
						Speed dSpeed = new Speed(this.Speed);				// meters per hour
						speedLbl = dSpeed.ToString();
					}
					if(HasOdometer)
					{
						Distance dOdometer = new Distance(this.Odometer);	// meters
						odometerLbl = dOdometer.ToString();
					}
				}
			}
			if(speedLbl != null)
			{
				tmp += "\nspeed: " + speedLbl;
			}
			if(odometerLbl != null)
			{
				tmp += "  odometer: " + odometerLbl;
			}

			return tmp;
		}

		/// <summary>
		/// short info string about the trackpoint for elevation graph
		/// </summary>
		/// <returns></returns>
		public string graphInfoString()
		{
			string tmp = "";

			string speedLbl = null;
			string odometerLbl = null;
			
			Track trk = (Track)Project.mainCommand.getTrackById(this.TrackId);
			if(trk != null)
			{
				if(trk.isRoute)
				{
					tmp += "Route " + this.TrackId + ": " + trk.Name;
					if(HasOdometer)
					{
						Distance dOdometer = new Distance(this.Odometer);	// meters
						odometerLbl = dOdometer.ToString();
					}
				}
				else
				{
					tmp += "Track " + this.TrackId + ": " + trk.Name;
					if(HasSpeed)
					{
						Speed dSpeed = new Speed(this.Speed);				// meters per hour
						speedLbl = dSpeed.ToString();
					}
					if(HasOdometer)
					{
						Distance dOdometer = new Distance(this.Odometer);	// meters
						odometerLbl = dOdometer.ToString();
					}
				}
			}

			Distance dElev = new Distance(this.Location.Elev);
			string elevLbl = dElev.ToStringCompl();

			tmp += "       elevation: " + elevLbl;

			if(speedLbl != null)
			{
				tmp += "  speed: " + speedLbl;
			}
			if(odometerLbl != null)
			{
				tmp += "  odometer: " + odometerLbl;
			}

			return tmp;
		}

		public string toDelayString(TimeSpan timeShift)
		{
			return Project.TimeSpanToString(timeShift);
		}

		/// <summary>
		/// returns a formatted string suitable for MapAdvisor exports
		/// </summary>
		/// <returns></returns>
		public string toStringCamtrack()
		{
			string tmp = "";
			bool doTime = true;
			if(this.Desc.Length > 0)
			{
				tmp += "\n" + this.Desc.Trim() + "\n";
			}

			string strLoType = this.LiveObjectTypeToString();
			tmp += "\nType: " + strLoType + (this.Found ? " - FOUND" : "");

			if(LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache && this.DateTime.Ticks > minDateTimeTicks)
			{
				tmp += " Hidden: " + Project.zuluToLocal(this.DateTime).ToShortDateString();
			}
			else if(doTime && this.DateTime.Ticks > minDateTimeTicks)
			// waypoint times are added to - to avoid duplicates, we need to compare with tolerance
			{
				tmp += "\nTime: " + Project.zuluToLocal(this.DateTime);
			}
			if(this.UrlName.Length > 0)
			{
				tmp += "\nUrl Name: " + this.UrlName.Trim();
			}
			if(this.LiveObjectType != LiveObjectTypes.LiveObjectTypeGeocache && this.Sym.Length > 0)
			{
				tmp += "\nSymbol: " + this.Sym.Trim();
			}
			if(this.Comment != null && this.Comment.Length > 0)
			{
				tmp += "\nComment: " + this.Comment.Trim();
			}
			if(groundspeakCache != null)
			{
				tmp += "\n" + groundspeakCache.ToStringPopup();
			}
			return cleanString(tmp);
		}

		// for old versions of MapAdvisor the parser doesn't like many symbols:
		private string cleanString(string tmp)
		{
			tmp = tmp.Replace("&quot;", "");
			tmp = tmp.Replace("&nbsp;", " ");
			tmp = tmp.Replace("&#060;", "");
			tmp = tmp.Replace("&amp;", " and ");
			tmp = tmp.Replace("&gt;", "");
			tmp = tmp.Replace("&#062;", "");
			tmp = tmp.Replace("&lt;", "");

			tmp = tmp.Replace("\"", "");
			tmp = tmp.Replace("<", "");
			tmp = tmp.Replace("&", " and ");
			tmp = tmp.Replace(">", "");

			tmp = tmp.Replace("  ", " ");
			tmp = tmp.Replace("\n", "\r");

			// convert to plain ASCII:
			ASCIIEncoding enc = new ASCIIEncoding();
			byte[] s = enc.GetBytes(tmp);
			tmp = enc.GetString(s);

			return tmp.Trim();
		}

		/// <summary>
		/// getLabel is called during layout and defines how the label will look on map.
		/// </summary>
		/// <param name="getAll"></param>
		/// <returns></returns>
		protected override string getLabel(bool getAll)
		{
			if(m_trackId >= 0)
			{
				if((m_name == null || m_name.Length == 0) && UrlName.Length > 0)
				{
					return UrlName;
				}
				return m_name;	// track points keep names assigned in Track:PutOnMap()
			}

			// no time on world map and state maps
			if(getAll || m_doName) 
			{
				// see Project.
				switch(Project.waypointNameStyle)
				{
					default:
					case 0:	// auto: try best to not leave it blank
						return (UrlName.Length > 0)? UrlName : WptName;
					case 1:
						return WptName;
					case 2:
						return UrlName;
					case 3:
						return Desc;
					case 4:
						return Comment;
					case 5:
						return Sym;
					case 6:
						return this.Location.ToStringWithElev();
				}
			}
			return "";
		}

		#endregion // some lesser toString metods

		#region Paint related

		// bigPlace ignored
		public override void placeLabel(int bigPlace, int fontSize, bool isAdjusting)
		{
			switch (LiveObjectType)
			{
				case LiveObjectTypes.LiveObjectTypeGeocache:
					bigPlace = 4;	// 5 will make label stay no matter what.
					break;
				default:
					bigPlace = 0;
					break;
			}
			if(m_forceShow)
			{
				bigPlace = 5;
			}
			base.placeLabel(bigPlace, fontSize, isAdjusting);
		}

		protected override int fontSizeByType(LiveObjectTypes type)
		{
			int ret = Project.FONT_SIZE_REGULAR;

			switch(type)
			{
				case LiveObjectTypes.LiveObjectTypeGeocache:
					ret = Project.FONT_SIZE_REGULAR;
					break;
			}

			return ret;
		}

		protected override int imageSizeByType(LiveObjectTypes type)
		{
			int ret = PixelRadius;

			Image symImage = null;
			
			if(Project.useWaypointIcons)
			{
				symImage = Project.waypointImageGetter.getImageBySymbol(m_sym);
			}
			
			if(symImage != null)
			{
				ret = Math.Max(symImage.Width, symImage.Height);
			}
			else
			{
				switch(type)
				{
					case LiveObjectTypes.LiveObjectTypeGeocache:
						ret = 12;
						break;
				}
			}
			return ret;
		}

		// see LabeledPoint
		public override Rectangle imageBoundingRect()
		{
			Image symImage = null;
			
			if(Project.useWaypointIcons)
			{
				symImage = Project.waypointImageGetter.getImageBySymbol(m_sym);
			}
			
			if(symImage != null)
			{
				m_imageBoundingRect = new Rectangle(
					m_pixelLocation.X - symImage.Width / 2, m_pixelLocation.Y - symImage.Height / 2,
					symImage.Width, symImage.Height);

				m_shift = Math.Max(MIN_SHIFT, Math.Min(m_imageBoundingRect.Width / 2, MAX_SHIFT));
			}
			else
			{
				base.imageBoundingRect();
			}

			return m_imageBoundingRect;
		}

		public override void PaintLabel(Graphics graphics, IDrawingSurface layer, ITile iTile, bool isPrint, int offsetX, int offsetY)
		{
			if(Project.showWaypointNames) 
			{
				base.PaintLabel(graphics, layer, iTile, isPrint, offsetX, offsetY);
			}
		}
		
		public override void Paint(Graphics graphics, IDrawingSurface layer, ITile iTile)
		{
			Paint(graphics, layer, iTile, 0, 0);
		}

		public override void Paint(Graphics graphics, IDrawingSurface layer, ITile iTile, int offsetX, int offsetY)
		{
			// waypoints belonging to tracks are painted in Track:Paint()
			if(!m_enabled || m_trackId != -1L) 
			{
				return;
			}

			//LibSys.StatusBar.Trace("Waypoint::Paint()  - " + Location + " : " + m_doName + m_labelBoundingRect);
        
			int x, y, w, h;

			x = m_imageBoundingRect.X + 1 + offsetX;
			y = m_imageBoundingRect.Y + 1 + offsetY;
			w = m_imageBoundingRect.Width - 2;
			h = m_imageBoundingRect.Height - 2;
 
			Image symImage = null;
			
			if(Project.useWaypointIcons)
			{
				symImage = Project.waypointImageGetter.getImageBySymbol(m_sym);
			}
			
			if(Project.thumbDoDisplay && this.ThumbImage != null)
			{
				Pen thumbBorderPen = Pens.Blue;
				Pen thumbCornerPen = new Pen(Color.Blue, 3.0f);
				int width = this.ThumbImage.Width;
				int height = this.ThumbImage.Height;
				Point p1 = layer.toPixelLocation(this.Location, iTile);
				int tx = p1.X + offsetX;
				int ty = p1.Y + offsetY;
				switch(this.ThumbPosition)
				{
					case 0:		// top right
						graphics.DrawImage(this.ThumbImage, tx, ty-height, width, height);
						graphics.DrawRectangle(thumbBorderPen, tx, ty-height, width-1, height-1);
						graphics.DrawLine(thumbCornerPen, tx+2, ty, tx+2, ty-10);
						graphics.DrawLine(thumbCornerPen, tx, ty-2, tx+10, ty-2);
						break;
					case 1:		// center
					default:
						graphics.DrawImage(this.ThumbImage, tx-width/2, ty-height/2, width, height);
						graphics.DrawRectangle(thumbBorderPen, tx-width/2, ty-height/2, width-1, height-1);
						break;
				}
			}
			else if(symImage != null)
			{
				graphics.DrawImage(symImage, x, y);
			}
			else
			{
				switch(LiveObjectType)
				{
					case LiveObjectTypes.LiveObjectTypeGeocache:
						if(m_found)
						{
							graphics.DrawEllipse(Project.waypointPen1bold, x, y, w, h);
							graphics.DrawEllipse(Project.waypointPen2bold, x+4, y+4, w-8, h-8);
							//graphics.DrawEllipse(Project.waypointPen3, x+8, y+8, w-16, h-16);
						}
						else
						{
							graphics.DrawEllipse(Project.waypointPen1, x, y, w, h);
							graphics.DrawEllipse(Project.waypointPen2, x+4, y+4, w-8, h-8);
							//graphics.DrawEllipse(Project.waypointPen3, x+8, y+8, w-16, h-16);
						}
						break;
					default:
						graphics.DrawEllipse(Project.waypointPen, x, y, w, h);
						break;
				}
			}

			// debug only - show bounding rectangles:
			//graphics.DrawRectangle(Project.debugPen, m_boundingRect);				// red
			//graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);		// green
			//graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);		// yellow
		}

		public Rectangle thumbBoundingRect(IDrawingSurface map)
		{
			if(this.ThumbImage == null)
			{
				return Rectangle.Empty;
			}

			try
			{
				Point pPoint = map.toPixelLocation(this.Location, null);

				int width  = this.ThumbImage.Width;
				int height = this.ThumbImage.Height;

				Rectangle re;

				switch(this.ThumbPosition)
				{
					case 0:		// top right
						re = new Rectangle(pPoint.X, pPoint.Y-height, width, height);
						break;
					case 1:		// center
					default:
						re = new Rectangle(pPoint.X-width/2, pPoint.Y-height/2, width, height);
						break;
				}
				return re;
			}
			catch
			{
				return Rectangle.Empty;
			}
		}

		public override Rectangle boundingRect()
		{
			if(this.ThumbImage == null)
			{
				return base.boundingRect();
			}

			base.boundingRect();	// sets m_boundingRect

			try
			{
				int width  = this.ThumbImage.Width;
				int height = this.ThumbImage.Height;

				Rectangle re;

				switch(this.ThumbPosition)
				{
					case 0:		// top right
						re = new Rectangle(m_pixelLocation.X, m_pixelLocation.Y-height, width, height);
						break;
					case 1:		// center
					default:
						re = new Rectangle(m_pixelLocation.X-width/2, m_pixelLocation.Y-height/2, width, height);
						break;
				}

				m_boundingRect = Rectangle.Union(m_boundingRect, re);
			}
			catch {}

			return m_boundingRect;
		}

		#endregion

		#region ToCamtrackXmlNode

		/// <summary>
		/// produces MapAdvisor format; called from LayerWaypoints
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <returns></returns>
		public XmlNode ToCamtrackXmlNode(XmlDocument xmlDoc)
		{
			XmlNode ret = xmlDoc.CreateNode(XmlNodeType.Element, "wpt", null);

			// see FileExportForm.cs for similar code (don't need to keep it in sync though)
			// <wpt lon="-74.35268" lat="43.49349" elev="-0.11438" type="campground" name="CAMP WOODS" time="2003-10-11T14:20:10Z">CAMP WOODS</wpt> 
  			string name = cleanString(WptName.Length == 0 ? UrlName : WptName);
			string desc = cleanString(UrlName);
			string detail = toStringCamtrack();		// goes into waypoints body, displayed as popup in MapAdvisor
			string type = (Sym.Length == 0 ? LiveObjectTypeToString() : Sym).ToLower();

			if(m_trackId == -1)
			{
				// regular waypoint
				if(Found && !type.EndsWith(" found"))
				{
					type += " found";
				}
				ret.InnerText = detail;
			}
			else
			{
				// track/route points
				ret.InnerText = Name;
				// patch for now:
				if("rtept".Equals(type))
				{
					type = "trkpt";
				}
			}

			XmlAttribute attr = xmlDoc.CreateAttribute("lon");
			attr.InnerText = "" + ((float)m_location.Lng);
			ret.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("lat");
			attr.InnerText = "" + ((float)m_location.Lat);
			ret.Attributes.Append(attr);
			if(m_location.Elev != 0.0d)
			{
				attr = xmlDoc.CreateAttribute("elev");
				attr.InnerText = "" + ((float)m_location.Elev);
				ret.Attributes.Append(attr);
			}
			attr = xmlDoc.CreateAttribute("type");
			attr.InnerText = type;
			ret.Attributes.Append(attr);
			if(m_trackId == -1)
			{
				attr = xmlDoc.CreateAttribute("name");
				attr.InnerText = "" + name;
				ret.Attributes.Append(attr);
				if(desc != null && !desc.Equals(name))
				{
					attr = xmlDoc.CreateAttribute("descr");
					attr.InnerText = "" + desc;
					ret.Attributes.Append(attr);
				}
			}
			else
			{
				attr = xmlDoc.CreateAttribute("trkid");
				attr.InnerText = "" + m_trackId;
				ret.Attributes.Append(attr);
			}
			attr = xmlDoc.CreateAttribute("time");
			// could be "new DateTimeFormatInfo().UniversalSortableDateTimePattern;" - but it has space instead of 'T'
			attr.InnerText = "" + m_dateTime.ToString(Project.dataTimeFormat);		// always Zulu
			ret.Attributes.Append(attr);

			return ret;
		}
		#endregion

		#region ToStreetsTripsCsv

		/*
		Name,Latitude,Longitude,Name 2,URL,Type[,Elev,Description,Time,Track,Speed,Odometer,Found,Photo,Coord,Elev ft]
		Active Pass Lighthouse,48.873472,-123.290139,"","",""
		Albert Head Light (removed),48.386444,-123.478167,"","",""
		Amphitrite Point Lighthouse,48.921139,-125.541167,"","",""
		Ballenas Island Light,49.350556,-124.160222,"","",""
		*/
		
		public string ToStreetsTripsCsv()
		{
			string desc = this.Desc;
			string name = this.WptName;
			string urlname = (this.UrlName == null || this.UrlName.Length == 0) ? this.Desc : this.UrlName;
			string type = this.WptType.Length > 0 ? this.WptType : this.LiveObjectTypeToString();
			string sym = "geocache".Equals(type.ToLower()) ? "Geocache" : this.Sym;
			string url = this.Url;
			string elevFt = (new Distance(this.Location.Elev)).toStringN(Distance.UNITS_DISTANCE_FEET);

			return csvCleanup(name) + "," + this.Location.Lat + "," + this.Location.Lng + "," + csvCleanup(urlname)
				+ "," + csvCleanup(url) + "," + csvCleanup(this.LiveObjectTypeToString()) + "," + this.Location.Elev + "," + csvCleanup(desc)
				+ "," + csvCleanup("" + DateTime) + "," + csvCleanup("" + this.trackInfoString()) + "," + csvCleanup("" + Speed) + "," + csvCleanup("" + Odometer)
				+ "," + csvCleanup("" + Found) + "," + csvCleanup("" + ThumbSource) + "," + csvCleanup(this.Location.ToStringWithElev()) + "," + elevFt
				+ "\n";
		}

		private string csvCleanup(string str)
		{
			if(str == null)
			{
				return "";
			}
			return "\"" + str.Replace("\"", "'").Replace("\n", " ").Replace('°',' ') + "\"";
		}

		#endregion

		#region ToGpxXmlNode

		/// <summary>
		/// used to export GPX files
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <returns></returns>
		public XmlNode ToGpxXmlNode(XmlDocument xmlDoc)
		{
			XmlNode ret = xmlDoc.CreateElement("wpt"); 
			//XmlNode ret = xmlDoc.CreateNode(XmlNodeType.Element, "wpt", null);

			/* This is what EasyGPS produces, keep our format close enough:
				<wpt lat="33.591700000" lon="-117.699750000">
					<time>2003-03-02T00:00:00Z</time>
					<name>GCDD94</name>
					<desc>LHCC Cache by mulaguna, Traditional Cache (2/1)</desc>
					<url>http://www.geocaching.com/seek/cache_details.aspx?wp=GCDD94</url>
					<urlname>LHCC Cache by mulaguna</urlname>
					<sym>Geocache</sym>
					<type>Geocache</type>
				</wpt>
			*/

			XmlAttribute attr = xmlDoc.CreateAttribute("lat");
			attr.InnerText = String.Format("{0:F7}", this.Location.Lat);
			ret.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("lon");
			attr.InnerText = String.Format("{0:F7}", this.Location.Lng);
			ret.Attributes.Append(attr);

			// see Waypoint.cs for remotely similar code (for Camtrack; no need to keep it in sync though)
			string name = this.WptName;
			string urlname = this.UrlName;
			string desc = this.Desc;
			string type = this.WptType.Length > 0 ? this.WptType : this.LiveObjectTypeToString();
			string sym = this.Sym.Length == 0 ? ("geocache".Equals(type.ToLower()) ? "Geocache" : this.Sym) : this.Sym;
			string url = this.Url;

			int pos = url.IndexOf("?wp=GC");
			if(pos > 0)		// we are dealing with EasyGPS format, produce very similar to what input was.
			{
				// restore waypoint name (like "GC2107") from URL:
				pos += 4;
				name = url.Substring(pos);
				sym = this.Sym.Length == 0 ? "Geocache" : this.Sym;
				type = "Geocache";
				if(groundspeakCache != null)
				{
					type += "|" + groundspeakCache.type;
				}
				if(this.Found)
				{
					type += "|Found";
				}
				desc = desc.Length == 0 ? this.UrlName : desc;
			}

//			if("Geocache".Equals(type) && this.Found)
//			{
//				sym += " Found";
//			}

			if(this.Location.Elev != 0.0d)
			{
				Project.SetValue(xmlDoc, ret, "ele", "" + this.Location.Elev);
			}
			if(this.DateTime.Ticks > minDateTimeTicks)
			{
				Project.SetValue(xmlDoc, ret, "time", "" + this.DateTime.ToString(Project.dataTimeFormat));
			}
			if(name.Length > 0)
			{
				Project.SetValue(xmlDoc, ret, "name", name);
			}
			//Project.SetValue(xmlDoc, ret, "cmt", cmt);
			if(desc.Length > 0)
			{
				Project.SetValue(xmlDoc, ret, "desc", desc);
			}
			if(url.Length > 0)
			{
				Project.SetValue(xmlDoc, ret, "url", url);
			}
			if(urlname.Length > 0)
			{
				Project.SetValue(xmlDoc, ret, "urlname", urlname);
			}
			if(sym.Length > 0)
			{
				Project.SetValue(xmlDoc, ret, "sym", sym);
			}
			if(type.Length > 3)
			{
				Project.SetValue(xmlDoc, ret, "type", type);
			}

			if(groundspeakCache != null)
			{
				XmlNode gsNode = groundspeakCache.ToGpxXmlNode(xmlDoc);
				ret.AppendChild(gsNode);
			}

			return ret;
		}
		#endregion

	}
}
