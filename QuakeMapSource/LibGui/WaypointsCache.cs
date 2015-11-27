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
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;

using LibSys;
using LibGeo;
using LibNet;
using LibFormats;

namespace LibGui
{
	public class FileListStruct
	{
		public string filename;
		public string formatName;
		public FormatProcessor processor;
		public bool persistent;

		public FileListStruct(string fn, string ffn, FormatProcessor p, bool ps)
		{
			filename = fn;
			formatName = ffn;
			processor = p;
			persistent = ps;
		}
	}

	/// <summary>
	/// Summary description for WaypointsCache.
	/// </summary>
	public class WaypointsCache : IImageBySymbol
	{
		public static bool isDirty = false; // user waypoints, GPS track logs, real-time GPS tracks...

		private static ArrayList m_waypointsAll = new ArrayList();
		public  static ArrayList WaypointsAll { get { return m_waypointsAll; } }

		// these waypoints are sorted north to south, east to west to ease up layout of labels:
		private static ArrayList m_waypointsDisplayed = new ArrayList();
		public  static ArrayList WaypointsDisplayed { get { return m_waypointsDisplayed; } }

		// these waypoints are coming in natural order (same as they are drawn in tracks) so that LayerWaypoints can sort the upper thumbnail photo:
		private static ArrayList m_waypointsDisplayedNotSorted = new ArrayList();
		public  static ArrayList WaypointsDisplayedNotSorted { get { return m_waypointsDisplayedNotSorted; } }

		private static ArrayList m_legsDisplayed = new ArrayList();
		public  static ArrayList LegsDisplayed { get { return m_legsDisplayed; } }

		private static ArrayList m_tracksAll = new ArrayList();
		public  static ArrayList TracksAll { get { return m_tracksAll; } }

		private static ArrayList m_routesAll = new ArrayList();
		public  static ArrayList RoutesAll {
			get {
				m_routesAll.Clear();
				foreach(Track trk in m_tracksAll)
				{
					if(trk.isRoute)
					{
						m_routesAll.Add(trk);
					}
				}
				return m_routesAll;		// may be empty, never null
			}
		}

		protected static PictureManager m_pictureManager = null;
		public static PictureManager PictureManager { get { return m_pictureManager; } set { m_pictureManager = value; } }

		protected static CameraManager m_cameraManager = null;
		public static CameraManager CameraManager { get { return m_cameraManager; } set { m_cameraManager = value; } }

		public static void shutdown()
		{
		}

		public static void init()
		{
			Project.waypointImageGetter = new WaypointsCache();

			/*
			Waypoint wp;

			// this is to test that labels are properly laid out in all positions in Waypoint.cs:
			for (int i=0; i < 12 ;i++) 
			{
				wp = new Waypoint(new GeoCoord(-130.0d, 42.0d, -330000.0d), DateTime.Now, "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
				m_waypointsAll.Add(wp);
			}
			*/
		}

		#region getImageBySymbol()

		private static Hashtable bitmaps = new Hashtable();		// bitmap by sym
		private static Hashtable tried   = new Hashtable();		// if already tried to load bitmap by name, bool

		public Bitmap getImageBySymbol(string sym)
		{
			if(sym == null || sym.Length == 0 || "none".Equals(sym))
			{
				return null;
			}

			sym = sym.ToLower();

			Bitmap ret = (Bitmap)bitmaps[sym];

			if(ret != null)
			{
				return ret;
			}

			try 
			{
				string filePath = Project.GetWptPath(sym + ".gif");
				if(!File.Exists(filePath) && (tried[sym] == null || !(bool)tried[sym]))
				{
					tried[sym] = true;
					// load the file remotely, don't hold diagnostics if load fails:
					string url = Project.MISC_FOLDER_URL + "/waypoints/" + sym + ".gif";
//					DloadProgressForm loaderForm = new DloadProgressForm(url, filePath, false, true);
//					loaderForm.ShowDialog();
					new DloadNoForm(url, filePath, 3000);
				}

				if(File.Exists(filePath))
				{
					ret = new Bitmap(filePath);
					bitmaps[sym] = ret;
				}
			} 
			catch
			{
				return null;
			}

			return ret;
		}
		#endregion

		public static Waypoint getTrackpointByWptId(long wptId)
		{
			foreach(Track trk in WaypointsCache.TracksAll)
			{
				for(int i=0; i < trk.Trackpoints.Count ;i++)
				{
					Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
					if(wpt.Id == wptId)
					{
						return wpt;
					}
				}
			}
			return null;
		}


		// returns two trackpoints around the given time - used to relate photos to trackpoints. P2 is a possible exact match:
		public static bool trackpointByTime(DateTime timeStamp, out Waypoint p1, out Waypoint p2)
		{
			Waypoint wpt = null;
			Waypoint wptPrev = null;	// good to have it stay between tracks (unless too old), so that interrupted tracks take photos ok at the beginning.
			for(int j = WaypointsCache.TracksAll.Count-1; j >= 0 ;j--)
			{
				Track trk = (Track)WaypointsCache.TracksAll[j];

				if(trk.Enabled && !trk.isRoute)	// timestamps on routes are for sorting only, they don't have any other meaning
				{
					for(int i=0; i < trk.Trackpoints.Count ;i++)
					{
						wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
						if(wptPrev != null && Math.Abs((wpt.DateTime - wptPrev.DateTime).TotalMinutes) > 20.0d)
						{
							// we don't want very old tracks be part of our scope.
							wptPrev = null;
						}

						if(wptPrev != null && wptPrev.DateTime.CompareTo(timeStamp) < 0 && wpt.DateTime.CompareTo(timeStamp) >= 0 )
						{
							goto found;
						}
						else if(wptPrev == null && wpt.DateTime.CompareTo(timeStamp) == 0 )
						{
							// exact match
							goto found;
						}

						wptPrev = wpt;
					}
				}
			}
			p1 = null;
			p2 = null;
			return false;

		found:
			p1 = wptPrev;
			p2 = wpt;
			return true;
		}

		static Track currentTrack = null;

		// takes array of Track 
		public static Track groupTracks(ArrayList tracks)
		{
			Project.trackId++;

			string tType = ((Track)tracks[0]).Type;
			string newTrackSource = "joined" + ("trk".Equals(tType) ? (" " + DateTime.Now) : ("-" + Project.trackId));

			string newTrackName = newTrackSource;

			CreateInfo createInfo = new CreateInfo();

			createInfo.init(tType);
			createInfo.id = Project.trackId;
			createInfo.name = newTrackName;
			createInfo.source = newTrackSource;

			Track joinedTrack = new Track(createInfo);

			m_tracksAll.Add(joinedTrack);

			DateTime dateTime = new DateTime(8888, 1, 1, 1, 1, 1, 1);	// for routes, we make new time sequence

			foreach(Track trk in tracks)
			{
				for(int i=0; i < trk.Trackpoints.Count ;i++)
				{
					Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
					Waypoint cloneWpt = new Waypoint(wpt);
					cloneWpt.TrackId = joinedTrack.Id;
					if(joinedTrack.isRoute)
					{
						cloneWpt.DateTime = dateTime;
						dateTime = dateTime.AddSeconds(1);
						cloneWpt.NameDisplayed = "";		// let it be the number or Detail
					}
					joinedTrack.insertWaypoint(cloneWpt);
				}
			}
			joinedTrack.rebuildTrackBoundaries();

			return joinedTrack;
		}
		
		// return array of track IDs or null (if track can't be ungrouped)
		public static ArrayList ungroupTrack(Track trk, double breakTimeMinutes)
		{
			double maxJumpMeters = 5000.0d;		// for sanity filter. make sure it is big enough, as GPSV makes big gaps.
			DateTime lastTime = DateTime.MinValue;
			GeoCoord lastLoc = null;
			int trip = 0;
			int i;

			// first count how many trips we can break the track into:
			for(i=0; i < trk.Trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
				DateTime wptTime = wpt.DateTime;
				if(wptTime.CompareTo(lastTime.AddMinutes(breakTimeMinutes)) > 0 || (lastLoc != null && wpt.Location.distanceFrom(lastLoc).Meters > maxJumpMeters))
				{
					trip++;
				}
				if(trip > 0)
				{
					lastTime = wptTime;
				}
				lastLoc = wpt.Location;
			}

			if(trip < 2)
			{
				return null;
			}

			// more than one trip, go ahead break it:
			string[] infos = new string[8];
			string newTrackSource = trk.Source;

			lastTime = DateTime.MinValue;
			lastLoc = null;
			trip = 0;

			ArrayList ret = new ArrayList();

			for(i=0; i < trk.Trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
				DateTime wptTime = wpt.DateTime;
				if(wptTime.CompareTo(lastTime.AddMinutes(breakTimeMinutes)) > 0 || (lastLoc != null && wpt.Location.distanceFrom(lastLoc).Meters > maxJumpMeters))
				{
					Project.trackId++;
					trip++;

					string newTrackName = trk.Name + "-trip-" + trip;

					CreateInfo createInfo = new CreateInfo();

					createInfo.init(trk.Type);
					createInfo.id = Project.trackId;
					createInfo.name = newTrackName;
					createInfo.source = newTrackSource;
					createInfo.url = trk.Url;

					currentTrack = new Track(createInfo);

					m_tracksAll.Add(currentTrack);
					ret.Add(Project.trackId);
				}
				if(trip > 0)
				{
					Waypoint cloneWpt = new Waypoint(wpt);
					cloneWpt.TrackId = Project.trackId;
					currentTrack.insertWaypoint(cloneWpt);
					lastTime = wptTime;
				}
				lastLoc = wpt.Location;
			}

			// make sure the speed and odometer values are computed:
			foreach(long trackId in ret)
			{
				Track ttrk = getTrackById(trackId);
				ttrk.rebuildTrackBoundaries();
			}

			// now apply Sanity Filter:
			if(Project.sanityFilter)
			{
				ArrayList ret1 = new ArrayList();
				foreach(long trackId in ret)
				{
					Track ttrk = getTrackById(trackId);
					if(ttrk.Trackpoints.Count >= 5 && ttrk.Odometer > 10.0f)		// odometer in meters, eliminates standing points
					{
						ret1.Add(trackId);
					}
					else
					{
						RemoveTrackById(trackId);
					}
				}
				if(ret1.Count < 2)
				{
					//return null;
				}
				ret = ret1;
			}

			if(ret != null)
			{
				isDirty = true;
			}
			return ret;
		}
		
		// return array of (two) track IDs or null (if track can't be ungrouped for some reason)
		public static ArrayList ungroupTrackAtTrackpoint(Track trk, Waypoint trkptBreak)
		{
			int trip = 0;
			int i;

			// more than one trip, go ahead break it:
			string newTrackSource = trk.Source;

			trip = 0;

			ArrayList ret = new ArrayList();

			for(i=0; i < trk.Trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
				if(i == 0 || wpt == trkptBreak)
				{
					Project.trackId++;
					trip++;

					string newTrackName = trk.Name + "-trip-" + trip;

					CreateInfo createInfo = new CreateInfo();

					createInfo.init(trk.Type);
					createInfo.id = Project.trackId;
					createInfo.name = newTrackName;
					createInfo.source = newTrackSource;
					createInfo.url = trk.Url;

					currentTrack = new Track(createInfo);

					m_tracksAll.Add(currentTrack);
					ret.Add(Project.trackId);
				}
				if(trip > 0)
				{
					Waypoint cloneWpt = new Waypoint(wpt);
					cloneWpt.TrackId = Project.trackId;
					currentTrack.insertWaypoint(cloneWpt);
				}
			}

			foreach(long id in ret)
			{
				Track ttrk = getTrackById(id);
				if(ttrk != null)
				{
					ttrk.rebuildTrackBoundaries();
				}
			}

			isDirty = true;
			return ret;
		}
		
		// waypoints boundaries, so that zoom-to-group-of-waypoints is easy:
		protected static GeoCoord	m_topLeft = new GeoCoord(180.0d, -90.0d);
		public static GeoCoord		TopLeft { get { return m_topLeft; } set { m_topLeft = value; } }
		protected static GeoCoord	m_bottomRight = new GeoCoord(-180.0d, 90.0d);
		public static GeoCoord		BottomRight { get { return m_bottomRight; } set { m_bottomRight = value; } }

		/// <summary>
		/// resets boundaries (TopLeft, BottomRight) so that newly added waypoints' boundaries are calculated
		/// useful for reading in .loc files 
		/// </summary>
		public static void resetBoundaries()
		{
			m_topLeft = new GeoCoord(180.0d, -90.0d);
			m_bottomRight = new GeoCoord(-180.0d, 90.0d);
		}

		public static bool boundariesWereMoved()
		{
			return !(m_topLeft.Lng == 180.0d && m_topLeft.Lat == -90.0d &&
					 m_bottomRight.Lng == -180.0d && m_bottomRight.Lat == 90.0d);
		}

		public static void pushBoundaries(GeoCoord loc)
		{
			if(loc.Lat > m_topLeft.Lat)
			{
				m_topLeft.Lat = loc.Lat;
			}
			if(loc.Lat < m_bottomRight.Lat)
			{
				m_bottomRight.Lat = loc.Lat;
			}
			if(loc.Lng < m_topLeft.Lng)
			{
				m_topLeft.Lng = loc.Lng;
			}
			if(loc.Lng > m_bottomRight.Lng)
			{
				m_bottomRight.Lng = loc.Lng;
			}
		}

		// keep in mind to lock(m_waypointsAll)
		public static void addWaypoint(Waypoint wp)
		{
			WaypointsCache.pushBoundaries(wp.Location);		// need this for files dropped on the icon at start, or on the program

			/*
						if (wp.Location.X > -127 || wp.Location.X < -130 || wp.Location.Y > 45 || wp.Location.Y < 43)
						{
							return;
						}
						*/

			Track _currentTrack;

			if(currentTrack != null && wp.TrackId == currentTrack.Id)		// save some time
			{
				currentTrack.insertWaypoint(wp);
			}
			else if(wp.TrackId != -1 && (_currentTrack=WaypointsCache.getTrackById(wp.TrackId)) != null)
			{
				_currentTrack.insertWaypoint(wp);
			}
			else
			{
				//double lng = wp.Location.Lng;
				double lat = wp.Location.Lat;

				int ii = 0;
				for (ii=0; ii < m_waypointsAll.Count ;ii++)
				{
					Waypoint other = (Waypoint)m_waypointsAll[ii];
					double lngo = other.Location.Lng;
					double lato = other.Location.Lat;

					//if(lngo == lng) 
					if(lato == lat) 
					{
						if(wp.LiveObjectType != LiveObjectTypes.LiveObjectTypeTrackpoint
							&& wp.LiveObjectType != LiveObjectTypes.LiveObjectTypeRoutepoint
							&& wp.sameAs(other))
						{
							//LibSys.StatusBar.Trace(" ---- ignored duplicate waypoint (orig from " + other.Source + ") : " + wp);
							return;		// ignore if it ain't new, but push boundaries for zooming
						}
					} 
						//else if(lngo < lng)	// sort east to west
						//else if(lato > lat)	// sort south to north
					else if(lato < lat)	// sort north to south
					{
						break;
					}
				}
				if(ii == m_waypointsAll.Count)
				{
					m_waypointsAll.Add(wp);
				}
				else
				{
					m_waypointsAll.Insert(ii, wp);
				}
			}

			// invalidating every object's drawing area is VERY expensive, we do PictureManager.Refresh() instead
			// at the end of interpreting all files. So the next line is commented out.
			//DynamicObjectCreateCallback(wp);
		}

		public static void insertWaypoint(CreateInfo createInfo)
		{
			lock(m_waypointsAll)
			{
				switch(createInfo.type)
				{
					case "trk":
					case "rte":
						currentTrack = new Track(createInfo);
						m_tracksAll.Add(currentTrack);
						break;
					default:
						Waypoint wpt = new Waypoint(createInfo);
						addWaypoint(wpt);
						break;
				}
			}
		}

		private static int InsertWaypointToDisplay(Waypoint wp)
		{
			double lng = wp.Location.Lng;
			double lat = wp.Location.Lat;

			int ii;
			for (ii=0; ii < m_waypointsDisplayed.Count ;ii++)
			{
				Waypoint other = (Waypoint)m_waypointsDisplayed[ii];
				double lato = other.Location.Lat;

				if(lato == lat) 
				{
					if(wp.sameAs(other))
					{
						//LibSys.StatusBar.Trace(" ---- ignored duplicate waypoint (orig from " + other.Source + ") : " + wp);
						return 0;		// ignore if it ain't new
					}
				} 
				else if(lato < lat)	// sort north to south
				{
					break;
				}
			}
			if(ii == m_waypointsDisplayed.Count)
			{
				m_waypointsDisplayed.Add(wp);
			}
			else
			{
				m_waypointsDisplayed.Insert(ii, wp);
			}
			m_waypointsDisplayedNotSorted.Add(wp);
			return 1;
		}

		public static void RefreshWaypointsDisplayed()
		{
			if(m_cameraManager == null)
			{
				return;
			}
			
			lock(m_waypointsDisplayed)
			{
				m_waypointsDisplayed.Clear();
				m_waypointsDisplayedNotSorted.Clear();
				m_legsDisplayed.Clear();

				bool haveTerraTiles = (Project.terraserverAvailable || Project.terraserverDisconnected)
										&& (m_cameraManager.terraTopLeft.Lng < m_cameraManager.terraBottomRight.Lng)
										&& (m_cameraManager.terraTopLeft.Lat > m_cameraManager.terraBottomRight.Lat);

				// for PDA we need to cover all area covered by tiles, which is wider than visible window:
				double lngCLeft = haveTerraTiles ? m_cameraManager.terraTopLeft.Lng : m_cameraManager.CoverageTopLeft.Lng;
				double lngCRight = haveTerraTiles ? m_cameraManager.terraBottomRight.Lng : m_cameraManager.CoverageBottomRight.Lng;
				double latCTop = haveTerraTiles ? m_cameraManager.terraTopLeft.Lat : m_cameraManager.CoverageTopLeft.Lat;
				double latCBottom = haveTerraTiles ? m_cameraManager.terraBottomRight.Lat : m_cameraManager.CoverageBottomRight.Lat;

				// here is a catch: wp coordinates are <180 >-180 (normaliized) while coverage coordinates are not.
				bool span180W = lngCLeft < -180.0d;
				bool span180E = lngCRight > 180.0d;

				foreach(Waypoint wp in m_waypointsAll)
				{
					if(wp.TrackId != -1L)
					{
						continue;
					}

					try
					{
						double lng = wp.Location.Lng;
						double lat = wp.Location.Lat;

						// de-normalize lng for comparisons, if span180 takes place:
						if(span180W && lng > 0)
						{
							lng -= 360.0d;
						}
						if(span180E && lng < 0)
						{
							lng += 360.0d;
						}

						if (lng < lngCRight && lng > lngCLeft && lat < latCTop && lat > latCBottom)
						{
							InsertWaypointToDisplay(wp);
						}
					} 
					catch (Exception e)
					{
					}
				}

				for(int i=0; i < m_tracksAll.Count ;i++)
				{
					Track trk = (Track)m_tracksAll[i];
					if(trk.Enabled)
					{
						try
						{
							int step = Track.getStep(trk, CameraManager.Elev);

							for(int ii=0; ii < trk.Trackpoints.Count ;ii+=step)
							{
								Waypoint wp = (Waypoint)trk.Trackpoints.GetByIndex(ii);

								if(wp.ThumbImage != null)
								{
									ii -= (step - 1);		// move to the next trackpoint
									continue;
								}

								double lng = wp.Location.Lng;
								double lat = wp.Location.Lat;

								if (lng < lngCRight && lng > lngCLeft && lat < latCTop && lat > latCBottom)
								{
									InsertWaypointToDisplay(wp);
								}
							}
							foreach(Waypoint wp in trk.TrackpointsNamed)
							{
								double lng = wp.Location.Lng;
								double lat = wp.Location.Lat;

								if (lng < lngCRight && lng > lngCLeft && lat < latCTop && lat > latCBottom)
								{
									InsertWaypointToDisplay(wp);
								}
							}
						} 
						catch (Exception e)
						{
						}
					}
				}

				if(m_tracksAll.Count > 0)		// no need to do it if there are no tracks or routes
				{
					for(int i=m_waypointsDisplayedNotSorted.Count-1; i >= 0 ;i--)
					{
						Waypoint wpt = (Waypoint)m_waypointsDisplayedNotSorted[i];
						if(wpt.LegFrom != null && !m_legsDisplayed.Contains(wpt.LegFrom))
						{
							m_legsDisplayed.Add(wpt.LegFrom);
						}
						if(wpt.LegTo != null && !m_legsDisplayed.Contains(wpt.LegTo))
						{
							m_legsDisplayed.Add(wpt.LegTo);
						}
					}
				}
			}
#if DEBUG
			LibSys.StatusBar.Trace("RefreshWaypointsDisplayed() total=" + m_waypointsDisplayed.Count);
#endif
		}

		public static void RemoveWaypointByName(string name)
		{
			lock(m_waypointsAll)
			{
			again2:
				for(int i=0; i < m_waypointsAll.Count ;i++)
				{
					Waypoint wp = (Waypoint)m_waypointsAll[i];
					try
					{
						if(wp.WptName.Equals(name))
						{
							m_waypointsAll.RemoveAt(i);
							isDirty = true;
							goto again2;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
			RefreshWaypointsDisplayed();
		}

		public static void RemoveWaypointByUrlname(string urlname)
		{
			lock(m_waypointsAll)
			{
			again2:
				for(int i=0; i < m_waypointsAll.Count ;i++)
				{
					Waypoint wp = (Waypoint)m_waypointsAll[i];
					try
					{
						if(wp.UrlName.Equals(urlname))
						{
							m_waypointsAll.RemoveAt(i);
							isDirty = true;
							goto again2;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
			RefreshWaypointsDisplayed();
		}

		public static void RemoveWaypointsBySource(string source)
		{
			lock(m_waypointsAll)
			{
			again:
				for(int i=0; i < m_tracksAll.Count ;i++)
				{
					Track trk = (Track)m_tracksAll[i];
					try
					{
						if(trk.Source.Equals(source))
						{
							m_tracksAll.RemoveAt(i);
							isDirty = true;
							goto again;
						}
					} 
					catch (Exception e)
					{
					}
				}

			again2:
				for(int i=0; i < m_waypointsAll.Count ;i++)
				{
					Waypoint wp = (Waypoint)m_waypointsAll[i];
					try
					{
						if(wp.Source.Equals(source))
						{
							m_waypointsAll.RemoveAt(i);
							isDirty = true;
							goto again2;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
			PhotoWaypoints.RemoveWaypointsBySource(source);
			RefreshWaypointsDisplayed();
		}

		public static Waypoint getWaypointByName(string name)
		{
			Waypoint ret = null;
			lock(m_waypointsAll)
			{
				for(int i=0; i < m_waypointsAll.Count ;i++)
				{
					Waypoint wp = (Waypoint)m_waypointsAll[i];
					try
					{
						if(wp.Name.Equals(name))
						{
							ret = wp;
							break;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
			return ret;
		}

		public static Waypoint getWaypointById(int id)
		{
			Waypoint ret = null;
			lock(m_waypointsAll)
			{
				for(int i=0; i < m_waypointsAll.Count ;i++)
				{
					Waypoint wp = (Waypoint)m_waypointsAll[i];
					try
					{
						if(wp.Id == id)
						{
							ret = wp;
							break;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
			return ret;
		}

		public static void RemoveWaypointById(int id)
		{
			lock(m_waypointsAll)
			{
				for(int i=0; i < m_waypointsAll.Count ;i++)
				{
					Waypoint wp = (Waypoint)m_waypointsAll[i];
					try
					{
						if(wp.Id == id)
						{
							m_waypointsAll.RemoveAt(i);
							isDirty = true;
							break;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
			RefreshWaypointsDisplayed();
		}

		public static Track getTrackById(long trackId)
		{
			foreach(Track trk in TracksAll)
			{
				if(trk.Id == trackId)
				{
					return trk;
				}
			}
			return null;
		}

		// returns zulu min time of all tracks
		public static DateTime getMinTime()
		{
			DateTime ret = DateTime.MaxValue;

			for(int i=0; i < m_tracksAll.Count ;i++)
			{
				Track trk = (Track)m_tracksAll[i];
				DateTime dt = trk.Start;

				if (dt.CompareTo(ret) < 0)
				{
					ret = dt;
				}
			}
			return (ret.Equals(DateTime.MaxValue) ? DateTime.MinValue : ret);
		}

		// returns zulu max time of all tracks
		public static DateTime getMaxTime()
		{
			DateTime ret = DateTime.MinValue;

			for(int i=0; i < m_tracksAll.Count ;i++)
			{
				Track trk = (Track)m_tracksAll[i];
				DateTime dt = trk.End;

				if (dt.CompareTo(ret) > 0)
				{
					ret = dt;
				}
			}
			return (ret.Equals(DateTime.MinValue) ? DateTime.MaxValue : ret);
		}

		public static void RemoveTrackById(long trackId)
		{
			for(int i=0; i < m_tracksAll.Count ;i++)
			{
				Track trk = (Track)m_tracksAll[i];
				try
				{
					if(trk.Id == trackId)
					{
						PhotoWaypoints.RemoveWaypointsByTrackId(trackId);
						m_tracksAll.RemoveAt(i);
						isDirty = true;
						break;	// can't be duplicate track Ids
					}
				} 
				catch (Exception e)
				{
				}
			}

			RefreshWaypointsDisplayed();
		}

		/// <summary>
		/// removes all tracks having this name
		/// </summary>
		/// <param name="name"></param>
		public static void RemoveTracksByName(string name)
		{
			if(name == null || name.Length == 0)
			{
				return;
			}

		  again:
			for(int i=0; i < m_tracksAll.Count ;i++)
			{
				Track trk = (Track)m_tracksAll[i];
				try
				{
					if(name.Equals(trk.Name))
					{
						PhotoWaypoints.RemoveWaypointsByTrackId(trk.Id);
						m_tracksAll.RemoveAt(i);
						isDirty = true;
						goto again;
					}
				} 
				catch (Exception e)
				{
				}
			}
			RefreshWaypointsDisplayed();
		}

		/// <summary>
		/// removes all tracks having this source
		/// </summary>
		/// <param name="name"></param>
		public static void RemoveTracksBySource(string source)
		{
			if(source == null || source.Length == 0)
			{
				return;
			}

			again:
				for(int i=0; i < m_tracksAll.Count ;i++)
				{
					Track trk = (Track)m_tracksAll[i];
					try
					{
						if(source.Equals(trk.Source))
						{
							PhotoWaypoints.RemoveWaypointsByTrackId(trk.Id);
							m_tracksAll.RemoveAt(i);
							isDirty = true;
							goto again;
						}
					} 
					catch (Exception e)
					{
					}
				}
			RefreshWaypointsDisplayed();
		}

	}
}
