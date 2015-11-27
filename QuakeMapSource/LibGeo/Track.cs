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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using LibSys;

namespace LibGeo
{
	/// <summary>
	/// Track is an ordered collection of Waypoint's.
	/// </summary>
	public class Track
	{
		protected string	m_name = null;
		public string       Name { get { return m_name; } set { m_name = value; } }

		protected long		m_id = 0;
		public long         Id { get { return m_id; } set { m_id = value; } }

		protected string	m_type = "";
		public string		Type { get { return m_type; } set { m_type = value; } }

		protected string	m_source = "";
		public string		Source { get { return m_source; } set { m_source = value; } }

		protected string	m_url = "";
		public string		Url { get { return m_url; } set { m_url = value; } }
		
		protected string	m_comment = "";
		public string		Comment { get { return m_comment; } set { m_comment = value; } }

		protected bool		m_enabled = true;
		public bool			Enabled { get { return m_enabled; } set { m_enabled = value; } }

		protected bool		m_isRoute = false;
		public bool			isRoute { get { return m_isRoute; } set { m_isRoute = value; } }

		protected int		m_routeNumber = 1;	// 1 to 20
		public int			RouteNumber { get { return m_routeNumber; } set { m_routeNumber = value; } }

		protected float		m_speedMax = -1000000001.0f;		// meters per hour
		public float        SpeedMax { get { return m_speedMax; } set { m_speedMax = value; } }
		protected float		m_speedMin = 1000000001.0f;			// meters per hour
		public float        SpeedMin { get { return m_speedMin; } set { m_speedMin = value; } }
		public bool         HasSpeed { get { return m_speedMin < 1000000000.0f && m_speedMax > -1000000000.0f; } }
		public float        Odometer
		{ 
			get 
			{
				try 
				{
					return ((Waypoint)m_trackpoints.GetByIndex(m_trackpoints.Count-1)).Odometer;
				} 
				catch { return 0.0f; }
			} 
		}

		protected SortedList m_trackpoints = new SortedList();	// by time
		public SortedList	Trackpoints { get { return m_trackpoints; } set { m_trackpoints = value; } }

		// for quick layout of named trackpoints:
		protected ArrayList	m_trackpointsNamed = new ArrayList();	// by time
		public ArrayList	TrackpointsNamed { get { return m_trackpointsNamed; } }

		// for small scale we draw only some trackpoints to speed up drawing tracks:
		private const double BIG_STEP_TRESHOLD = 200000.0d;	// meters
		private const int	 BIG_STEP_DIVIDER = 100;		// how many points we want to have in large tracks on small scale

		private const double DRAW_POINT_TRESHOLD = 5000.0d;

		// track boundaries, so that zoom-to-track is easy:
		protected GeoCoord	m_topLeft = new GeoCoord(180.0d, -90.0d);
		public GeoCoord		TopLeft { get { return m_topLeft; } set { m_topLeft = value; } }
		protected GeoCoord	m_bottomRight = new GeoCoord(-180.0d, 90.0d);
		public GeoCoord		BottomRight { get { return m_bottomRight; } set { m_bottomRight = value; } }
		public double		ElevMax = -100000.0d;
		public double		ElevMin = 100000.0d;

		public DateTime		Start
		{
			get 
			{
				try 
				{
					return ((Waypoint)m_trackpoints.GetByIndex(0)).DateTime;
				} 
				catch { return DateTime.MinValue; }
			} 
		}

		public DateTime		End 
		{
			get 
			{
				try 
				{
					return ((Waypoint)m_trackpoints.GetByIndex(m_trackpoints.Count-1)).DateTime;
				} 
				catch { return DateTime.MinValue; }
			} 
		} 

		protected Brush		brushFont = null;
		protected Brush		brushBackground = null;
		protected Pen		penUnderline = null;

		public Track(CreateInfo createInfo)
		{
			m_name = createInfo.name;
			m_id = createInfo.id;
			m_type = createInfo.type;			// "trk" or "rte"
			m_isRoute = "rte".Equals(m_type);
			m_comment = m_name;
			if(m_isRoute)
			{
				try
				{
					m_routeNumber = Convert.ToInt32(createInfo.par1);
				}
				catch {}
				if(m_routeNumber < 1)
				{
					m_routeNumber = 1;
				}
				if(m_routeNumber > 20)
				{
					m_routeNumber = 20;
				}
			}
			else
			{
				m_url = createInfo.url;
			}
			m_source = createInfo.source;

			if(createInfo.par1 != null && createInfo.par1.Length == 6)		// have <topografix:color>98fb98<topografix:color> tag or something?
			{
				try
				{
					int argb = Convert.ToInt32(("0xff" + createInfo.par1).ToLower(), 16);
					setColor(Color.FromArgb(argb));
				}
				catch {}
			}

			brushFont = Project.trackFontBrush;
			brushBackground = Project.trackBackgroundBrush;
			penUnderline = Project.trackPen;
		}

		public void setColor(Color color)
		{
			presetColor = color;
			presetBrush = new SolidBrush(presetColor);
			isPresetColor = true;
		}

		public void removeWaypoint(Waypoint wpt)
		{
			removeWaypoint(wpt.DateTime);
		}

		public void removeWaypoint(DateTime dt)
		{
			m_trackpoints.Remove(dt);
			rebuildTrackBoundaries();
		}

		public void insertWaypoint(Waypoint wpt)
		{
			while(true)
			{
				// protect from two waypoints having exactly the same time stamp:
				try 
				{
					m_trackpoints.Add(wpt.DateTime, wpt);		// will throw exception if this time is already in the list

					pushBoundaries(wpt);
					return;
				}
				catch
				{
					// make the time different and try again:
					DateTime tmp = wpt.DateTime;
					wpt.DateTime = tmp.AddMilliseconds(1);
				}
			}
		}

		public void splitLeg(Leg leg, GeoCoord midLoc)
		{
			Distance dFrom = leg.WptFrom.distanceFrom(midLoc);
			Distance dTo = leg.WptTo.distanceFrom(midLoc);

			double ratio = dFrom.Meters / (dFrom.Meters + dTo.Meters);

			DateTime dt = leg.WptFrom.DateTime.AddTicks((long)(leg.Duration.Ticks * ratio));
			if(leg.Duration.TotalSeconds > 60.0d)
			{
				dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);	// round to a second
			}
			// assuming that midLoc has been already "new" and will attach to the new waypoint:
			Waypoint wpt = new Waypoint(midLoc, dt, leg.WptFrom.LiveObjectType, leg.WptFrom.TrackId, "", leg.WptFrom.Source, "");

			insertWaypoint(wpt);

			rebuildTrackBoundaries();
		}

		public Waypoint getTrackpointById(int id)
		{
			Waypoint ret = null;
			for(int i=0; i < m_trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(i);
				if(wpt.Id == id)
				{
					ret = wpt;
					break;
				}
			}
			return ret;
		}

		protected void pushBoundaries(Waypoint wpt)
		{
			if(wpt.Location.Lat > m_topLeft.Lat)
			{
				m_topLeft.Lat = wpt.Location.Lat;
			}
			if(wpt.Location.Lat < m_bottomRight.Lat)
			{
				m_bottomRight.Lat = wpt.Location.Lat;
			}
			if(wpt.Location.Lng < m_topLeft.Lng)
			{
				m_topLeft.Lng = wpt.Location.Lng;
			}
			if(wpt.Location.Lng > m_bottomRight.Lng)
			{
				m_bottomRight.Lng = wpt.Location.Lng;
			}
			if(wpt.Location.Elev > ElevMax)
			{
				ElevMax = wpt.Location.Elev;
			}
			if(wpt.Location.Elev < ElevMin)
			{
				ElevMin = wpt.Location.Elev;
			}
		}

		public void rebuildTrackBoundaries()
		{
			m_topLeft.Lng = 180.0d;
			m_topLeft.Lat = -90.0d;
			m_bottomRight.Lng = -180.0d;
			m_bottomRight.Lat = 90.0d;
			ElevMax = -100000.0d;
			ElevMin =  100000.0d;
			SpeedMax = -1000000001.0f;
			SpeedMin =  1000000001.0f;

			double odometer = 0.0d;	// meters
			Waypoint prevWpt = null;

			for(int i=0; i < m_trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(i);
				pushBoundaries(wpt);

				// compute speed:
				if(prevWpt != null)
				{
					Leg leg = new Leg(this, prevWpt, wpt);
					prevWpt.LegFrom = leg;
					wpt.LegTo = leg;

					// compute odometer and speed:
					Distance dSpeed = new Distance(leg.Dist);		// wpt.Location.distanceFrom(prevWpt.Location);	// meters
					odometer += dSpeed.Meters;
					if(!isRoute)
					{
						TimeSpan dt = wpt.DateTime - prevWpt.DateTime;
						dSpeed.Meters = dSpeed.Meters * 36000000000.0d / dt.Ticks;		// meters per hour
						if(dSpeed.Meters > 1000.0d && dSpeed.Meters < 330.0d * 3600.0d)	// sanity check - speed of sound, m/hr
						{
							wpt.Speed = (float)dSpeed.Meters;
							leg.Speed = wpt.Speed;

							if(wpt.Speed > this.SpeedMax)
							{
								this.SpeedMax = wpt.Speed;
							}
							if(wpt.Speed < this.SpeedMin)
							{
								this.SpeedMin = wpt.Speed;
							}
						}
						else
						{
							wpt.HasSpeed = false;
							leg.HasSpeed = false;
						}
					}
				}
				wpt.Odometer = (float)odometer;
				prevWpt = wpt;
			}
		}

		public Waypoint getPreviousTrackpoint(Waypoint wpt)
		{
			Waypoint ret = null;

			int i;
			for(i=0; i < m_trackpoints.Count ;i++)
			{
				Waypoint cur = (Waypoint)m_trackpoints.GetByIndex(i);
				//if(cur.DateTime.CompareTo(wpt.DateTime) >= 0)
				if(cur == wpt)
				{
					break;
				}
				ret = cur;
			}
			if(i == m_trackpoints.Count)
			{
				ret = null;
			}
			return ret;
		}

		/// <summary>
		/// for routes only
		/// </summary>
		public void reverse()
		{
			if(this.isRoute)
			{
				SortedList trackpoints = new SortedList();

				DateTime tmp = DateTime.Now;
				for(int i=0; i < m_trackpoints.Count ;i++)
				{
					Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(i);

					wpt.DateTime = tmp;
					trackpoints.Add(wpt.DateTime, wpt);

					tmp = tmp.AddSeconds(-1);
				}

				m_trackpoints = trackpoints;
				rebuildTrackBoundaries();
			}
		}

		/// <summary>
		/// for tracks only
		/// </summary>
		/// <returns></returns>
		public Track toRoute()
		{
			if(!this.isRoute)
			{
				CreateInfo createInfo = new CreateInfo();
				createInfo.init("rte");
				Project.trackId++;
				createInfo.id = Project.trackId;
				createInfo.name = "From " + this.Name;
				createInfo.source = "From " + m_source;

				Track route = new Track(createInfo);

				int step = (int)Math.Ceiling((double)m_trackpoints.Count / (double)Project.gpsMaxPointsPerRoute);
				if(step <= 0)
				{
					step = 1;
				}

				int iLast = 0;
				for(int i=0; i < m_trackpoints.Count && route.Trackpoints.Count < Project.gpsMaxPointsPerRoute - 1 ;i+=step)
				{
					Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(i);
					if(wpt.ThumbImage != null)
					{
						i -= step - 1;		// move to a trackpoint immediately after the photo point
						continue;
					}
					iLast = i;

					Waypoint wptClone = new Waypoint(wpt);
					wptClone.LiveObjectType = LiveObjectTypes.LiveObjectTypeRoutepoint;
					wptClone.TrackId = route.Id;
					wptClone.Source = route.Source;
					route.Trackpoints.Add(wptClone.DateTime, wptClone);
				}

				// we want the end of the route to be the end track point:
				if(iLast < m_trackpoints.Count - 1)
				{
					Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(m_trackpoints.Count - 1);

					Waypoint wptClone = new Waypoint(wpt);
					wptClone.TrackId = route.Id;
					wptClone.Source = route.Source;
					route.Trackpoints.Add(wptClone.DateTime, wptClone);
				}
				route.rebuildTrackBoundaries();

				return route;
			}
			return null;
		}

		public override string ToString()
		{
			return m_name + " type=" + m_type + " comment='" + m_comment + "' src=" + m_source + " url=" + m_url;
		}

		public string ToStringProfile()
		{
			return m_name;
		}

		private void assignNames(IDrawingSurface layer)
		{
			m_trackpointsNamed.Clear();	// when we are done, will contain list of significant trackpoints (those with name, url, thumbnail or content)

			int step = getStep(this, layer.getCameraElevation());

			int i;
			for(i=0; i < m_trackpoints.Count ;i+=step)
			{
				Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(i);

				if(wpt.ThumbImage != null)	// ignore photopoints for now
				{
					i -= (step - 1);		// move to the next trackpoint
					continue;
				}

				if(i == 0)
				{
					wpt.init(true);		// doName = true
					wpt.isEndpoint = true;
					if(isRoute)
					{
						wpt.Name = "Start " + this.Name;
					}
					else
					{
						switch(Project.trackNameStyle)
						{
							default:
								wpt.Name = "Start-" + m_id + ": " + Project.zuluToLocal(wpt.DateTime);
								break;
							case 1:
								wpt.Name = "Start: " + this.Name;
								break;
							case 2:
								wpt.Name = "" + this.Name;
								break;
							case 3:
								wpt.Name = "";
								break;
						}
					}
					wpt.ForceShow = true;
					m_trackpointsNamed.Add(wpt);
				}
				else if(i == m_trackpoints.Count-1)
				{
					wpt.init(true);		// doName = true
					wpt.isEndpoint = true;
					if(isRoute)
					{
						wpt.Name = "End " + this.Name;
					}
					else
					{
						switch(Project.trackNameStyle)
						{
							default:
								wpt.Name = "End-" + m_id + ": " + Project.zuluToLocal(wpt.DateTime);
								break;
							case 1:
								wpt.Name = "End: " + this.Name;
								break;
							case 2:
								wpt.Name = "" + this.Name;
								break;
							case 3:
								wpt.Name = "";
								break;
						}
					}
					wpt.ForceShow = true;
					m_trackpointsNamed.Add(wpt);
				}
				else
				{
					bool hasContent = wpt.hasContent;
					bool hasUrl = wpt.Url.Length > 0;
					wpt.init(hasContent || hasUrl);	// doName depends on content/url, will be drawn in Track:Paint() if false
					wpt.isEndpoint = false;
					if(hasContent)
					{
						int pos = wpt.Desc.IndexOf("\n");
						wpt.Name = wpt.Desc.Substring(0, pos == -1 ? Math.Min(30, wpt.Desc.Length) : Math.Min(pos, 30));
						m_trackpointsNamed.Add(wpt);
					}
					else if(hasUrl)
					{
						wpt.Name = "" + (i+1) + " [browse]";
						m_trackpointsNamed.Add(wpt);
					}
					else
					{
						wpt.Name = "" + (i+1);
					}
					wpt.ForceShow = hasContent || hasUrl;
				}
				wpt.PixelRadius = 5;	// hover sensitivity zone
				int pointsLeft = m_trackpoints.Count - i;
				if(pointsLeft > 1 && pointsLeft < step)
				{
					step = pointsLeft - 2;
					if(step < 1)
					{
						step = 1;
					}
				}
			}

			if(Project.thumbDoDisplay)
			{
				SortedList ppts = Project.mainCommand.getWaypointsWithThumbs(m_id);
				for (i=0; i < ppts.Count ;i++)
				{
					Waypoint wpt = (Waypoint)ppts.GetByIndex(i);
					wpt.init(true);
					int pos = wpt.Desc.IndexOf("\n");
					wpt.Name = wpt.Desc.Substring(0, pos == -1 ? Math.Min(30, wpt.Desc.Length) : Math.Min(pos, 30));
					m_trackpointsNamed.Add(wpt);
					wpt.PixelRadius = 5;	// larger hover sensitivity zone
					wpt.ForceShow = true;
				}
			}
		}

		#region Paint related

		public static int getStep(Track track, double elev)
		{
			int step = 1;
			if(elev > BIG_STEP_TRESHOLD)
			{
				step = track.Trackpoints.Count / BIG_STEP_DIVIDER;
				if(step < 1)
				{
					step = 1;
				}
			}
			return step;
		}

		public void PutOnMap(IDrawingSurface layer, ITile iTile, IObjectsLayoutManager iOlm)
		{
			assignNames(layer);

			rebuildTrackBoundaries();
			
			/*
			 * can't do this, as other trackpoints are not placed and will not react on mouse hover
			foreach(Waypoint wpt in m_trackpointsNamed)
			{
				wpt.PutOnMap(layer, iTile, iOlm);
			}
			*/

			// keep the stepping logic in sync with WaypointsCache:RefreshWaypointsDisplayed(), because 
			// LayerWaypoints:WaypointByPoint() operates on the WaypointsCache.WaypointsDisplayedNotSorted array
			int step = getStep(this, layer.getCameraElevation());

			for(int i=0; i < m_trackpoints.Count ;i+=step)
			{
				try
				{
					Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(i);
					if(wpt.ThumbImage != null)
					{
						i -= (step - 1);		// move to the next trackpoint
						continue;
					}

					wpt.PutOnMap(layer, iTile, iOlm);
					int pointsLeft = m_trackpoints.Count - i;
					if(pointsLeft > 1 && pointsLeft < step)
					{
						step = pointsLeft - 2;
						if(step < 1)
						{
							step = 1;
						}
					}
				}
				catch {}
			}
			if(Project.thumbDoDisplay)
			{
				SortedList ppts = Project.mainCommand.getWaypointsWithThumbs(m_id);
				for (int i=0; i < ppts.Count ;i++)
				{
					Waypoint wpt = (Waypoint)ppts.GetByIndex(i);
					wpt.PutOnMap(layer, iTile, iOlm);
				}
			}
		}

		public bool isPresetColor = false;
		public Color presetColor = Color.Red;
		private SolidBrush	presetBrush = null;

		public Color color
		{
			get
			{
				if(isPresetColor)
				{
					return presetColor;
				}
				else
				{
					return TrackPalette.getColor(m_id);
				}
			}
		}

		private Brush getBrush()
		{
			if(isPresetColor)
			{
				return presetBrush;
			}
			else
			{
				return TrackPalette.getBrush(m_id);
			}
		}

		private Pen getTrackPen()
		{
			if(isPresetColor)
			{
				return new Pen(presetBrush, TrackPalette.penTrackThickness);
			}
			else
			{
				return TrackPalette.getTrackPen(m_id);
			}
		}

		private Pen getRoutePen()
		{
			if(isPresetColor)
			{
				return new Pen(presetBrush, TrackPalette.penRouteThickness);
			}
			else
			{
				return TrackPalette.getRoutePen(m_id);
			}
		}

		/*
		public void AdjustPlacement(IDrawingSurface map, ITile iTile, IObjectsLayoutManager iOlm)
		{
			for(int i=0; i < m_trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)m_trackpoints.GetByIndex(i);
				wpt.AdjustPlacement(map, iTile, iOlm);
			}
		}
		*/

		// a simplified label for trackpoints:
		protected void PaintLabel(Graphics graphics, string label, int x, int y, bool isPrint)
		{
			Font font = Project.getLabelFont(isPrint ? Project.FONT_SIZE_PRINT : Project.FONT_SIZE_REGULAR);
			if(Project.waypointUseShadow) 
			{
				graphics.DrawString(label, font, brushBackground, x,   y);
				graphics.DrawString(label, font, brushBackground, x+2, y);
				graphics.DrawString(label, font, brushBackground, x,   y-2);
				graphics.DrawString(label, font, brushBackground, x+2, y-2);
			}
			graphics.DrawString(label, font, brushFont, x+1,   y-1);
		}

		public static Waypoint lastHighlightedWaypoint = null;

		public void Paint(Graphics graphics, IDrawingSurface layer, ITile iTile)
		{
			Paint(graphics, layer, iTile, false);
		}

		public void Paint(Graphics graphics, IDrawingSurface layer, ITile iTile, bool isPrint)
		{
			if(!m_enabled) 
			{
				return;
			}

			//LibSys.StatusBar.Trace("Track::Paint()  - " + m_trackpoints.Count + " : " + Name);
        
			int prevX = 0;
			int prevY = 0;

			int arrowWingsTrk = Math.Max(15, (int)(7.0f * TrackPalette.penTrackThickness));
			int arrowWingsRte = Math.Max(18, (int)(9.0f * TrackPalette.penRouteThickness));
			float trackThicknessFactor = TrackPalette.penTrackThickness / 2.0f;

			Pen	penHighlight = new Pen(Color.Red, 3.0f * trackThicknessFactor);
			Pen	penHighlight2 = new Pen(Color.Yellow, 3.0f * trackThicknessFactor);

			int diam = (int)Math.Ceiling(4.0f * trackThicknessFactor);
			int rad = diam/2;

			Waypoint _lastHighlightedWaypoint = null;

			int step = getStep(this, layer.getCameraElevation());

			bool doDrawPoints = Project.drawTrackpoints && (layer.getCameraElevation() < DRAW_POINT_TRESHOLD);
			int pointCount = 0;
			int lastArrowCount = 0;
//			Point tailPoint = Point.Empty;
			Point lastArrowPoint = Point.Empty;

			// prepare default brushes/pens, with either random or preset color:
			Brush trkBrush = getBrush();		// filled circles, arrow heads
			Pen trkPen = getTrackPen();			// lines
			Pen rtePen = getRoutePen();			// lines
			Pen rteTrkPen;

			// square (flat) Cap is a pain in neck; this kind of works:
			GraphicsPath objPath = new GraphicsPath();
//			objPath.AddRectangle(new Rectangle(0, -1, 0, 0));
			CustomLineCap squareCap = new System.Drawing.Drawing2D.CustomLineCap(objPath, null, LineCap.Flat);
			squareCap.WidthScale = 0.0f;

			bool skipOne;

			for(int i=0; i < m_trackpoints.Count ;i+=step)
			{
				skipOne = false;
				try
				{
					Waypoint wp1 = (Waypoint)m_trackpoints.GetByIndex(i);
					bool doDrawThisPoint = doDrawPoints;

					if(wp1.ThumbImage != null)
					{
						skipOne = true;		// draw and move to next trackpoint; keep in sync with the stepping logic 
					}

					pointCount++;
                    Waypoint wp2 = null;
					if(i != m_trackpoints.Count-1 && (i + step) < m_trackpoints.Count)	// last point
					{
						wp2 = (Waypoint)m_trackpoints.GetByIndex(i+step);
					}
					
					if(!layer.insideScreenRectangle(wp1.Location) && wp2 != null && !layer.insideScreenRectangle(wp2.Location))
					{
						goto nextLoop;
					}

					Point p1 = isPrint ? layer.toPixelLocationPrint(wp1.Location, iTile) : layer.toPixelLocation(wp1.Location, iTile);
//					if(tailPoint.IsEmpty)
//					{
//						tailPoint = p1;
//					}

					int dd = diam;
					bool staying = false;
					bool doHighlight = false;
					bool hasContent = wp1.hasContent;
					bool hasUrl = wp1.Url != null && wp1.Url.Length > 0;
					string stayLbl = "";
					if(TimeFilter.Enabled)
					{
						doHighlight = TimeFilter.passes(wp1.DateTime);
						if(!doHighlight && TimeFilter.beforeFrom(wp1.DateTime))
						{
							// in case it doesn't pass directly, but we are in the middle of the track
							// and the time filter boundaries are between this point and next point,
							// we highlight both points because we are sitting there at this time.
							if(i > 0 && wp2 != null)	// mid-track
							{
								if(TimeFilter.afterTo(wp2.DateTime))
								{
									doHighlight = true;
									dd *= 3;
									staying = true;
									DateTime wp2LocalTime = Project.zuluToLocal(wp2.DateTime);
									stayLbl = "staying till " + wp2LocalTime.TimeOfDay;
								}
							}
						}
					}

					if(SelectFilter.Enabled)
					{
						doHighlight = SelectFilter.passes(wp1);
						dd = 2;
					}

					if(doHighlight)
					{
						if(_lastHighlightedWaypoint == null)
						{
							_lastHighlightedWaypoint = wp1;
						}
						graphics.DrawEllipse(penHighlight, p1.X-dd, p1.Y-dd, dd*2, dd*2);
						if(staying)
						{
							int x = p1.X + 10;
							int y = p1.Y - dd*2;
							Font font = Project.getLabelFont(Project.FONT_SIZE_REGULAR + 2);
							if(Project.waypointUseShadow) 
							{
								graphics.DrawString(stayLbl, font, Project.blackBrush, x,   y);
								graphics.DrawString(stayLbl, font, Project.blackBrush, x+2, y);
								graphics.DrawString(stayLbl, font, Project.blackBrush, x,   y-2);
								graphics.DrawString(stayLbl, font, Project.blackBrush, x+2, y-2);
							}
							graphics.DrawString(stayLbl, font, Project.whiteBrush, x+1,   y-1);
						}
					}

					if(hasUrl)
					{
						graphics.DrawEllipse(penHighlight2, p1.X-4, p1.Y-4, 8, 8);
					}

					if(hasContent)
					{
						if(!hasUrl)
						{
							graphics.DrawEllipse(Pens.Cyan, p1.X-4, p1.Y-4, 8, 8);
						}
						graphics.DrawEllipse(penHighlight, p1.X-2, p1.Y-2, 4, 4);
					}

					if(hasUrl || hasContent || (wp2 == null && (i==0 || !Project.makeRouteMode)))	// last point
					{
						int offsetX = 0;
						int offsetY = 0;
						if(isPrint)
						{
							Point pixelPosPrint = layer.toPixelLocationPrint(wp1.Location, null);
							Point pixelPosDispl = wp1.PixelLocation;
							offsetX = pixelPosPrint.X - pixelPosDispl.X;
							offsetY = pixelPosPrint.Y - pixelPosDispl.Y;
						}
						wp1.PaintLabel(graphics, layer, iTile, isPrint, offsetX, offsetY);
					}

					if(wp2 != null)			// not the last point
					{
						Point p2 = isPrint ? layer.toPixelLocationPrint(wp2.Location, iTile) : layer.toPixelLocation(wp2.Location, iTile);

						bool thickPen = false;
						if(m_isRoute)
						{
							rteTrkPen = rtePen;
						}
						else
						{
							if(Project.trackElevColor)
							{
								double elevRange = ElevMax - ElevMin;
								if(elevRange > 1.0d && elevRange < 20000.0d)
								{
									double elevFactor = elevRange / 256.0d;
									double elev = wp2.Location.Elev;
									int r = (int)((elev - ElevMin) / elevFactor);
									if(r > 255)
									{
										r = 255;
									}
									int b = 255 - r;
									int g = (255 - (r > b ? r : b)) * 2;		// will be high where R and B are close to equal, amounts to cyan

									Color legColor = Color.FromArgb(r, g, b);
									trkBrush = new SolidBrush(legColor);
									trkPen = new Pen(trkBrush, (r > 250 ? 5.0f : 3.0f) * trackThicknessFactor);
									thickPen = true;
								}
							}
							else if(Project.trackSpeedColor)
							{
								double speedRange = SpeedMax - SpeedMin;
								if(speedRange > 1000.0d && speedRange < 1000000000.0d)
								{
									double speedFactor = speedRange / 256.0d;
									double speed = wp2.Speed;
									int r = (int)((speed - SpeedMin) / speedFactor);
									if(r > 255)
									{
										r = 255;
									}
									int b = 255 - r;
									int g = (255 - (r > b ? r : b)) * 2;		// will be high where R and B are close to equal, amounts to cyan

									Color legColor = Color.FromArgb(r, g, b);
									trkBrush = new SolidBrush(legColor);
									trkPen = new Pen(trkBrush, (r > 250 ? 5.0f : 3.0f) * trackThicknessFactor);
									thickPen = true;
								}
							}
							rteTrkPen = doHighlight ? penHighlight : trkPen;
						}

						int dltaX = p2.X - p1.X;
						int dltaY = p2.Y - p1.Y;
						double lenSq = dltaX * dltaX + dltaY * dltaY;
						int dlta2X = p2.X - lastArrowPoint.X;
						int dlta2Y = p2.Y - lastArrowPoint.Y;
						double lenSq2 = dlta2X * dlta2X + dlta2Y * dlta2Y;

						if(lenSq2 > 10000.0d || lenSq > 900.0d)	// big enough to hold arrow?
						{
							float arrowWidth = thickPen ? 4 : 3;
							float arrowHeight = 6;
							bool arrowFill = true;
							
							rteTrkPen.CustomEndCap = new AdjustableArrowCap(arrowWidth, arrowHeight, arrowFill);

							lastArrowCount = pointCount;
							lastArrowPoint = p2;
//							tailPoint = p2;
							//doDrawThisPoint = false;
						}
						else
						{
							// no cap - null doesn't work here 
							rteTrkPen.CustomEndCap = squareCap;
						}
						
						graphics.DrawLine(rteTrkPen, p1, p2);

						if(doDrawThisPoint && diam > 2)
						{
							graphics.FillEllipse(trkBrush, p2.X-rad, p2.Y-rad, diam, diam);
						}

						if(i == 0)
						{
							int offsetX = 0;
							int offsetY = 0;
							if(isPrint)
							{
								Point pixelPosPrint = layer.toPixelLocationPrint(wp1.Location, null);
								Point pixelPosDispl = wp1.PixelLocation;
								offsetX = pixelPosPrint.X - pixelPosDispl.X;
								offsetY = pixelPosPrint.Y - pixelPosDispl.Y;
							}
							wp1.PaintLabel(graphics, layer, iTile, isPrint, offsetX, offsetY);
							prevX = p1.X;
							prevY = p1.Y;
						}
						else if((p1.X - prevX)*(p1.X - prevX) + (p1.Y - prevY)*(p1.Y - prevY) > 900)
						{
							if(Project.showTrackpointNumbers)
							{
								// call simplified PaintLabel, not regular one:
								PaintLabel(graphics, wp1.Name, p1.X,   p1.Y, isPrint);
							}
							prevX = p1.X;
							prevY = p1.Y;
						}
					}
					else if(doDrawThisPoint)
					{
						graphics.FillEllipse(trkBrush, p1.X-rad, p1.Y-rad, diam, diam);
					}
				
				} 
				catch (Exception e)
				{
					LibSys.StatusBar.Error("Track::Paint()  - " + e.Message);
				}

			nextLoop:
				if(skipOne)
				{
					i -= (step - 1);		// move to the next trackpoint
					continue;
				}
				int pointsLeft = m_trackpoints.Count - i;
				if(pointsLeft > 1 && pointsLeft < step)
				{
					step = pointsLeft - 2;
					if(step < 1)
					{
						step = 1;
					}
				}
			}

			if(Project.thumbDoDisplay)
			{
				SortedList ppts = Project.mainCommand.getWaypointsWithThumbs(m_id);
				for (int i=0; i < ppts.Count ;i++)
				{
					Waypoint wpt = (Waypoint)ppts.GetByIndex(i);
					Pen thumbBorderPen = Pens.Blue;
					Pen thumbCornerPen = new Pen(Color.Blue, 3.0f);
					int width = wpt.ThumbImage.Width;
					int height = wpt.ThumbImage.Height;
					Point p1 = isPrint ? layer.toPixelLocationPrint(wpt.Location, iTile) : layer.toPixelLocation(wpt.Location, iTile);
					switch(wpt.ThumbPosition)
					{
						case 0:		// top right
							graphics.DrawImage(wpt.ThumbImage, p1.X, p1.Y-height, width, height);
							graphics.DrawRectangle(thumbBorderPen, p1.X, p1.Y-height, width-1, height-1);
							graphics.DrawLine(thumbCornerPen, p1.X+2, p1.Y, p1.X+2, p1.Y-10);
							graphics.DrawLine(thumbCornerPen, p1.X, p1.Y-2, p1.X+10, p1.Y-2);
							break;
						case 1:		// center
						default:
							graphics.DrawImage(wpt.ThumbImage, p1.X-width/2, p1.Y-height/2, width, height);
							graphics.DrawRectangle(thumbBorderPen, p1.X-width/2, p1.Y-height/2, width-1, height-1);
							break;
					}
					int offsetX = 0;
					int offsetY = 0;
					if(isPrint)
					{
						Point pixelPosPrint = layer.toPixelLocationPrint(wpt.Location, null);
						Point pixelPosDispl = wpt.PixelLocation;
						offsetX = pixelPosPrint.X - pixelPosDispl.X;
						offsetY = pixelPosPrint.Y - pixelPosDispl.Y;
					}
					wpt.PaintLabel(graphics, layer, iTile, isPrint, offsetX, offsetY);
				}
			}

			if(_lastHighlightedWaypoint != null)
			{
				lastHighlightedWaypoint = _lastHighlightedWaypoint;
			}
		}
		#endregion

		#region ToGpxXmlNode

		protected static double minDateTimeTicks = (new DateTime(1,1,1)).AddDays(100.0d).Ticks;

		/// <summary>
		/// used to export GPX files
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <returns></returns>
		public XmlNode ToGpxXmlNode(XmlDocument xmlDoc, out int trkptCount)
		{
			trkptCount = 0;

			XmlNode trkNode = xmlDoc.CreateNode(XmlNodeType.Element, isRoute ? "rte" : "trk", null);

			Project.SetValue(xmlDoc, trkNode, "name", this.Name);
			
			XmlNode trksegNode = null;
			if(isRoute)
			{
				// routes have numbers associated with them:
				Project.SetValue(xmlDoc, trkNode, "number", "" + this.RouteNumber);
			}

			if(isPresetColor)
			{
				int argb = presetColor.ToArgb() & 0xFFFFFF;

				string prefix = "topografix";
				string namespaceURI = "http://www.topografix.com/GPX/Private/TopoGrafix/0/2";

				Project.SetValue(xmlDoc, trkNode, prefix, "color", namespaceURI, String.Format("{0:x6}", argb));
			}

			if(!isRoute)
			{
				// for tracks there is additional level (trkseg) under which the trackpoints go:
				trksegNode = Project.SetValue(xmlDoc, trkNode, "trkseg", "");
			}

			for (int ii = 0; ii < this.Trackpoints.Count; ii++)
			{
				Waypoint wpt = (Waypoint)this.Trackpoints.GetByIndex(ii);
				if(wpt.ThumbImage == null)		// do not save photo points
				{
					XmlNode trkptNode = Project.SetValue(xmlDoc, this.isRoute ? trkNode : trksegNode, this.isRoute ? "rtept" : "trkpt", "");
					XmlAttribute attr = xmlDoc.CreateAttribute("lat");
					attr.InnerText = String.Format("{0:F7}", wpt.Location.Lat);
					trkptNode.Attributes.Append(attr);
					attr = xmlDoc.CreateAttribute("lon");
					attr.InnerText = String.Format("{0:F7}", wpt.Location.Lng);
					trkptNode.Attributes.Append(attr);
					if(wpt.Location.Elev != 0.0d)
					{
						Project.SetValue(xmlDoc, trkptNode, "ele", "" + wpt.Location.Elev);
					}

					if(wpt.DateTime.Ticks > minDateTimeTicks)
					{
						Project.SetValue(xmlDoc, trkptNode, "time", "" + wpt.DateTime.ToString(Project.dataTimeFormat));
					}

					if(wpt.Comment.Length > 0)
					{
						Project.SetValue(xmlDoc, trkptNode, "cmt", wpt.Comment);
					}
					if(wpt.Desc.Length > 0)
					{
						Project.SetValue(xmlDoc, trkptNode, "desc", wpt.Desc);
					} 
					// else if(this.isRoute && wpt.Name != null && wpt.Name.Length > 0)
					//{
					//	Project.SetValue(xmlDoc, trkptNode, "desc", "" + wpt.NameDisplayed);
					//}
					if(wpt.Url.Length > 0)
					{
						Project.SetValue(xmlDoc, trkptNode, "url", wpt.Url);
					}
					if(wpt.UrlName.Length > 0)
					{
						Project.SetValue(xmlDoc, trkptNode, "urlname", wpt.UrlName);
					}
					Project.SetValue(xmlDoc, trkptNode, "sym", wpt.Sym.Length > 3 ? wpt.Sym : (this.isRoute ? "Dot" : "Waypoint"));
					trkptCount++;
				}
			}				

			return trkNode;
		}
		#endregion
	}

	public class SelectFilter
	{
		public static Track	track = null;
		public static Waypoint	fromTrkpt = null;
		public static Waypoint	toTrkpt = null;
		public static bool Enabled = false;

		public static bool passes(Waypoint trkpt)
		{
			if(track == null)
			{
				return false;
			}

			int fromIndex = track.Trackpoints.IndexOfValue(fromTrkpt);
			int toIndex = track.Trackpoints.IndexOfValue(toTrkpt);
			int thisIndex = track.Trackpoints.IndexOfValue(trkpt);

			return thisIndex >= fromIndex && thisIndex <= toIndex;
		}
		public static bool beforeFrom(Waypoint trkpt)
		{
			int fromIndex = track.Trackpoints.IndexOfValue(fromTrkpt);
			int thisIndex = track.Trackpoints.IndexOfValue(trkpt);

			return thisIndex < fromIndex;
		}
		public static bool afterTo(Waypoint trkpt)
		{
			int toIndex = track.Trackpoints.IndexOfValue(toTrkpt);
			int thisIndex = track.Trackpoints.IndexOfValue(trkpt);

			return thisIndex > toIndex;
		}

		public static void reset()
		{
			track = null;
			fromTrkpt = null;
			toTrkpt = null;
			Enabled = false;
		}
	}
}
