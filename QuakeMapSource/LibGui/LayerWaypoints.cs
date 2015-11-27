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
using System.Diagnostics;
using System.Xml;
using System.IO;

using LibSys;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// Summary description for LayerWaypoints.
	/// </summary>
	public class LayerWaypoints : Layer, IObjectsLayoutManager
	{
		public override string Name() { return "LayerWaypoints"; }

		public static LayerWaypoints This = null;

		private Track m_routeTrack = null;
		public Track routeBeingBuilt { get { return m_routeTrack; } }

		private bool m_saveAllowMapPopups = true;

		public bool makeRouteMode  {
			get { return LibSys.Project.makeRouteMode; }
			set {
				if(value && !LibSys.Project.makeRouteMode)
				{
					// starting make route mode
					LibSys.Project.makeRouteMode = true;			// first click on the map will start building new route
					m_saveAllowMapPopups = Project.allowMapPopups;
					Project.allowMapPopups = false;
				}
				else if(!value && LibSys.Project.makeRouteMode)
				{
					// finishing make route mode - we get here on Escape key or via right-click menu
					finishMakeRoute();
					Project.allowMapPopups = m_saveAllowMapPopups;
					LibSys.Project.makeRouteMode = false;
				}
			}
		}

		private Waypoint wptToMove = null;
 
		public bool movePointMode  {
			get { return LibSys.Project.movePointMode; }
			set {
				if(value && !LibSys.Project.movePointMode)
				{
					// starting moving point mode
					LibSys.Project.movePointMode = true;			// dragging mouse will move the point
					m_saveAllowMapPopups = Project.allowMapPopups;
					Project.allowMapPopups = false;
				}
				else if(!value && LibSys.Project.movePointMode)
				{
					// finishing moving point mode - we get here on Escape key or releasing the button
					Project.allowMapPopups = m_saveAllowMapPopups;
					LibSys.Project.movePointMode = false;
				}
			}
		}

		private Leg legToMove = null;

		public bool moveLegMode  
		{
			get { return LibSys.Project.moveLegMode; }
			set 
			{
				if(value && !LibSys.Project.moveLegMode)
				{
					// starting moving leg mode
					LibSys.Project.moveLegMode = true;			// dragging mouse will split the leg and move the leg's mid-point
					m_saveAllowMapPopups = Project.allowMapPopups;
					Project.allowMapPopups = false;
				}
				else if(!value && LibSys.Project.moveLegMode)
				{
					// finishing moving leg mode - we get here on Escape key or releasing the button
					Project.allowMapPopups = m_saveAllowMapPopups;
					LibSys.Project.moveLegMode = false;
				}
			}
		}

		protected bool m_hasPutOnMap = false;
		public bool HasPutOnMap { get { return m_hasPutOnMap; } set { m_hasPutOnMap = value; } }

		public LayerWaypoints(PictureManager pm, CameraManager cm) : base(pm, cm)
		{
			WaypointsCache.PictureManager = pm;
			WaypointsCache.CameraManager = cm;

			WaypointsCache.init();
			This = this;
		}

		public override void shutdown()
		{
			WaypointsCache.shutdown();
		}

		public void init()
		{
			m_hasPutOnMap = false;
		}

		public override void PictureResized()
		{
			// LibSys.StatusBar.Trace("IP: LayerWaypoints::PictureResized()  drawableReady=" + Project.drawableReady);
			init();
		}

		public override void CameraMoved()
		{
			// LibSys.StatusBar.Trace("IP: LayerWaypoints::CameraMoved()  drawableReady=" + Project.drawableReady);
			init();
			base.CameraMoved();
			WaypointsCache.RefreshWaypointsDisplayed();
			sLiveObjectPopup = null;

			logCamtrackFrame();
		}

		private Waypoint WaypointByPoint(Point point)
		{
			try
			{
				if(makeRouteMode && m_routeTrack != null && m_routeTrack.Trackpoints.Count > 1)
				{
					for(int i=0; i < m_routeTrack.Trackpoints.Count ;i++)
					{
						Waypoint wpt = (Waypoint)m_routeTrack.Trackpoints.GetByIndex(i);
						try 
						{
							if(wpt.labelBoundingRect().Contains(point) || wpt.imageBoundingRect().Contains(point))
							{
								return wpt;
							}
						} 
						catch(Exception e) 
						{
							LibSys.StatusBar.Error("LW:WaypointByPoint(): " + e.Message);
						}
					}
				}

				for(int i=WaypointsCache.WaypointsDisplayedNotSorted.Count-1; i >= 0 ;i--)
				{
					Waypoint wpt = (Waypoint)WaypointsCache.WaypointsDisplayedNotSorted[i];
					try 
					{
						if(wpt.labelBoundingRect().Contains(point) || wpt.imageBoundingRect().Contains(point)
							|| Project.thumbDoDisplay && wpt.ThumbImage != null && wpt.thumbBoundingRect(this).Contains(point))
						{
							return wpt;
						}
					} 
					catch(Exception e) 
					{
						LibSys.StatusBar.Error("LW:WaypointByPoint(): " + e.Message);
					}
				}
			} 
			catch {}
			return null;
		}


		private Leg LegByPoint(Point point)
		{
			try
			{
				foreach(Leg leg in WaypointsCache.LegsDisplayed)
				{
					if(leg.isPointOver(point)) //, m_pictureManager.Graphics))
					{
						return leg;
					}
				}
			} 
			catch {}
			return null;
		}

		protected static string sLiveObjectPopup = null;

		private Rectangle hotWptRect = Rectangle.Empty;
		private DateTime timeHotWptRectDrawn = DateTime.MinValue;
		private Waypoint hotWpt = null;

		private Rectangle hotLegRect = Rectangle.Empty;
		private DateTime timeHotLegRectDrawn = DateTime.MinValue;
		private Leg hotLeg = null;

		private void hideHotWptRect()
		{
			if(!hotWptRect.IsEmpty)
			{
				hotWptRect.Inflate(2, 2);
				hotWptRect.Offset(-1, -1);
				m_pictureManager.Invalidate(hotWptRect);
				hotWptRect = Rectangle.Empty;
			}
		}

		private void hideHotLegRect()
		{
			if(!hotLegRect.IsEmpty)
			{
				hotLegRect.Inflate(2, 2);
				hotLegRect.Offset(-1, -1);
				m_pictureManager.Invalidate(hotLegRect);
				hotLegRect = Rectangle.Empty;
			}
		}

		private Point m_movePoint;
		private Rectangle m_lastMoveLegRect;

		public override void ProcessMouseMove(Point movePoint, GeoCoord mouseGeoLocation,
													bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				m_movePoint = movePoint;

				if(controlDown || altDown || m_cameraManager.HasDragRectangle)
				{
					LibSys.StatusBar.WriteLine("* " + m_cameraManager.DragEndHint);		// will not be added to log
					return;
				}

				if(shiftDown && movePointMode)
				{
					LibSys.StatusBar.WriteLine("* drag waypoint to new location");		// will not be added to log
					Point screenPoint = m_pictureManager.PointToScreen(movePoint);
					Project.MapShowPopup(m_pictureManager.PictureBox, "dragging", screenPoint);
					Cursor.Current = Cursors.Hand;
					return;
				}

				if(moveLegMode)
				{
					int MARK_RADIUS = 5;

					int minX = Math.Min(Math.Min(m_movePoint.X - MARK_RADIUS - 1, hotLeg.WptFrom.PixelLocation.X), hotLeg.WptTo.PixelLocation.X);
					int maxX = Math.Max(Math.Max(m_movePoint.X + MARK_RADIUS + 1, hotLeg.WptFrom.PixelLocation.X), hotLeg.WptTo.PixelLocation.X);
					int minY = Math.Min(Math.Min(m_movePoint.Y - MARK_RADIUS - 1, hotLeg.WptFrom.PixelLocation.Y), hotLeg.WptTo.PixelLocation.Y);
					int maxY = Math.Max(Math.Max(m_movePoint.Y + MARK_RADIUS + 1, hotLeg.WptFrom.PixelLocation.Y), hotLeg.WptTo.PixelLocation.Y);

					if(!m_lastMoveLegRect.IsEmpty)
					{
						minX = Math.Min(m_lastMoveLegRect.X, minX);
						maxX = Math.Max(m_lastMoveLegRect.X + m_lastMoveLegRect.Width, maxX);
						minY = Math.Min(m_lastMoveLegRect.Y, minY);
						maxY = Math.Max(m_lastMoveLegRect.Y + m_lastMoveLegRect.Height, maxY);
					}

					Rectangle r = new Rectangle(minX - 20, minY - 20, maxX - minX + 22, maxY - minY + 22);

					m_pictureManager.Invalidate(r);

					Cursor.Current = Cursors.Hand;
					return;
				}
				else if(!m_lastMoveLegRect.IsEmpty)
				{
					m_pictureManager.Invalidate(m_lastMoveLegRect);
				}

				Waypoint hoverWpt = WaypointByPoint(movePoint);
				Leg hoverLeg = null;

				if(hoverWpt != null)
				{
					Point wptPoint = hoverWpt.PixelLocation;
					if(hoverWpt != hotWpt || (DateTime.Now - timeHotWptRectDrawn).TotalMilliseconds > 500)
					{
						if(hoverWpt != hotWpt)
						{
							hideHotWptRect();
						}

						int MARK_RADIUS = 5;
						hotWptRect = new Rectangle(wptPoint.X - MARK_RADIUS, wptPoint.Y - MARK_RADIUS, MARK_RADIUS * 2, MARK_RADIUS * 2);
						m_pictureManager.Graphics.DrawRectangle(Pens.Yellow, hotWptRect);
						timeHotWptRectDrawn = DateTime.Now;
					}

					string tmp = hoverWpt.toStringPopup();
					if(!tmp.Equals(sLiveObjectPopup)) // && movePoint.Y > 30)	// make sure the popup will not overlap menu
					{
						sLiveObjectPopup = tmp;
						LibSys.StatusBar.WriteLine("* " + sLiveObjectPopup);		// will not be added to log
						//movePoint.Offset(0, 0);
						Point screenPoint = m_pictureManager.PointToScreen(wptPoint);
						Project.MapShowPopup(m_pictureManager.PictureBox, sLiveObjectPopup, screenPoint);
					}
					Cursor.Current = Cursors.Hand;
				}

				if(hoverWpt == null)
				{
					hoverLeg = LegByPoint(movePoint);
					if(hoverLeg != null)
					{
						if(hoverLeg != hotLeg || (DateTime.Now - timeHotLegRectDrawn).TotalMilliseconds > 500)
						{
							if(hoverLeg != hotLeg)
							{
								hideHotLegRect();
							}

							Point middleLegPoint = hoverLeg.MiddlePoint;
							int MARK_RADIUS = 5;
							hotLegRect = new Rectangle(middleLegPoint.X - MARK_RADIUS, middleLegPoint.Y - MARK_RADIUS, MARK_RADIUS * 2, MARK_RADIUS * 2);
							m_pictureManager.Graphics.DrawRectangle(Pens.Yellow, hotLegRect);
							timeHotLegRectDrawn = DateTime.Now;
						}

						string tmp = hoverLeg.toStringPopup();
						if(!tmp.Equals(sLiveObjectPopup)) // && movePoint.Y > 30)	// make sure the popup will not overlap menu
						{
							sLiveObjectPopup = tmp;
							LibSys.StatusBar.WriteLine("* " + sLiveObjectPopup);		// will not be added to log
							//movePoint.Offset(0, 0);
							Point screenPoint = m_pictureManager.PointToScreen(movePoint);
							Project.MapShowPopup(m_pictureManager.PictureBox, sLiveObjectPopup, screenPoint);
						}
						Cursor.Current = Cursors.Hand;
					}
				}

				hotLeg = hoverLeg;

				if(hoverLeg == null)
				{
					hideHotLegRect();
				}

				hotWpt = hoverWpt;

				if(hoverWpt == null)
				{
					hideHotWptRect();
				}

				if(hoverWpt == null && hoverLeg == null)
				{
					sLiveObjectPopup = null;	// mouse reentry into the same label is allowed
				}
			} 
			catch {}
		}

		private Point m_downPoint;

		public override void ProcessMouseDown(Point downPoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				// LibSys.StatusBar.Trace("LayerWaypoints:ProcessMouseDown()");

				if(m_cameraManager.CanDrillDown)
				{
					Waypoint clickWpt = null;

					if(!moveLegMode && !hotLegRect.IsEmpty && hotLegRect.Contains(downPoint))
					{
						moveLegMode = true;
						legToMove = hotLeg;
						m_downPoint = downPoint;
					}
					else if(!movePointMode && !hotWptRect.IsEmpty && hotWptRect.Contains(downPoint))
					{
						if((clickWpt = WaypointByPoint(downPoint)) != null && (shiftDown || clickWpt.TrackId != -1))
						{
							Point screenPoint = m_pictureManager.PointToScreen(downPoint);
							movePointMode = true;
							wptToMove = clickWpt;
						}
					}
				}
			}
			catch {}
		}

		public override void ProcessMouseUp(Point upPoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
			//LibSys.StatusBar.Trace("LayerWaypoints:ProcessMouseUp()");

			if(movePointMode)
			{
				movePointMode = false;
				if(wptToMove != null)
				{
					if(!upPoint.Equals(m_downPoint))
					{
						wptToMove.Location = mouseGeoLocation;
						WaypointsCache.isDirty = true;
						wptToMove = null;
						CameraManager.This.SpoilPicture(upPoint);
						CameraManager.This.ProcessCameraMove();
					}
					else
					{
						Point screenPoint = m_pictureManager.PointToScreen(upPoint);
						Project.ShowPopup(m_pictureManager.PictureBox, " you can move this waypoint by dragging mouse where you want new waypoint location to be. ", screenPoint);
						wptToMove = null;
					}
				}
			}

			if(moveLegMode)
			{
				moveLegMode = false;
				if(!upPoint.Equals(m_downPoint))
				{
					CameraManager.This.SpoilPicture(upPoint);
					legToMove.split(mouseGeoLocation);
					legToMove = null;
					CameraManager.This.ProcessCameraMove();
				}
				else
				{
					Point screenPoint = m_pictureManager.PointToScreen(upPoint);
					Project.ShowPopup(m_pictureManager.PictureBox, " you can split this leg by dragging mouse where you want new waypoint location to be. ", screenPoint);
					legToMove = null;
				}
			}
		}

		// support for menu-building on the right click:
		public override LiveObject EvalMouseClick(Point movePoint, GeoCoord mouseGeoLocation, bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				return WaypointByPoint(movePoint);		// may be null
			} 
			catch {}
			return null;
		}

		// finishing make route mode
		private void finishMakeRoute()
		{
			if(m_routeTrack != null)
			{
				if(m_routeTrack.Trackpoints.Count < 2)
				{
					// that was an empty or one-point route, delete it.
					WaypointsCache.TracksAll.Remove(m_routeTrack);
					m_cameraManager.ProcessCameraMove();		// make sure that single label, if any, is removed the map
				}
				else
				{
					// real route, show it all with the endpoint named appropriately:
					Waypoint wpt = (Waypoint)m_routeTrack.Trackpoints.GetByIndex(m_routeTrack.Trackpoints.Count - 1);
					string wptName = "End route";
					wpt.NameDisplayed = wptName;
					m_cameraManager.ProcessCameraMove();		// make sure label is positionsed on the map
					WaypointsCache.isDirty = true;
				}
				m_routeTrack = null;
			}
		}
		
		public void addLocToRoute(GeoCoord loc)
		{
			addToRoute(loc, null, null);
		}

		public void addWptToRoute(Waypoint wpt)
		{
			addToRoute(null, wpt, null);
		}

		public void removeWptFromRoute(Waypoint wpt)
		{
			m_routeTrack.removeWaypoint(wpt);
			m_cameraManager.ProcessCameraMove();
		}

		public void addEqToRoute(Earthquake eq)
		{
			addToRoute(null, null, eq);
		}

		private int rteptNumber = 1;
		private static int rteNumber = 1;
		private Speed m_speed = null;
		private Waypoint m_lastWpt = null;

		private DateTime m_dateTimeRte = Project.localToZulu(new DateTime(1, 1, 1));		// time zulu; keep in mind DateTimePicker allows date years 1758 to 9999
		public DateTime RouteStartDateTime { get { return m_dateTimeRte; } set { m_dateTimeRte = value; } }				// time zulu

		private void addToRoute(GeoCoord location, Waypoint wpt, Earthquake eq)
		{
			rteptNumber++;
			string wptName = "" + rteptNumber;
			if(m_routeTrack == null)
			{
				// first route-making click on the map, create track to hold the new route 
				string newTrackSource = "route - user created " + DateTime.Now;

				Project.trackId++;
				rteptNumber = 1;
				m_lastWpt = null;

				string newTrackName = "Route-" + Project.trackId;

				CreateInfo createInfo = new CreateInfo();

				createInfo.init("rte");
				createInfo.id = Project.trackId;
				createInfo.name = newTrackName;
				createInfo.source = newTrackSource;
				createInfo.par1 = "" + rteNumber;
				rteNumber++;
				if(rteNumber > 20)
				{
					rteNumber = 1;
				}

				m_routeTrack = new Track(createInfo);
				m_routeTrack.isRoute = true;
				WaypointsCache.TracksAll.Add(m_routeTrack);
				wptName = "Start route";
			}

			m_speed = null;

			if(m_lastWpt != null && m_lastWpt.HasSpeed)
			{
				m_speed = new Speed(m_lastWpt.Speed);
			}
			else
			{
				m_speed = new Speed(Project.routeSpeed);
			}

			TimeSpan dur = new TimeSpan(100000);

			Waypoint routeWpt = null;
			bool wasNew = false;
			
			if(wpt != null)
			{
				routeWpt = new Waypoint(wpt);
				routeWpt.LiveObjectType = LiveObjectTypes.LiveObjectTypeRoutepoint;
				routeWpt.DateTime = m_dateTimeRte;
			}
			else if(eq != null)
			{
				//wptName = eq.ToString();
				wptName = string.Format("{0:F1} - ", eq.Magn) + eq.sDateTime + " - " + eq.Comment;
				routeWpt = new Waypoint(eq.Location, m_dateTimeRte, LiveObjectTypes.LiveObjectTypeRoutepoint, Project.trackId, wptName, eq.Source, eq.Url);
			}
			else
			{
				// location must not be null then:
				routeWpt = new Waypoint(location, m_dateTimeRte, LiveObjectTypes.LiveObjectTypeRoutepoint, Project.trackId, "", "user created", "");		// no URL
				routeWpt.NameDisplayed = wptName;
				wasNew = true;
			}

			if(m_speed != null)
			{
				routeWpt.Speed = (float)m_speed.Meters;
			}

			if(m_lastWpt != null && m_lastWpt.TrackId == Project.trackId)
			{
				Distance dist = routeWpt.distanceFrom(m_lastWpt.Location);
				double durSeconds = m_speed.Meters <= 0.0d ? 0.000001d : (dist.Meters / m_speed.Meters * 3600.0d);
				dur = new TimeSpan((long)(durSeconds * 10000000.0d));

				m_dateTimeRte += dur;
			}

			routeWpt.DateTime = m_dateTimeRte;

			m_lastWpt = routeWpt;

			// we need to make sure that the point added is different from the last point in track.
			// Magellan will not accept routes with zero-length legs.
			Waypoint lastWpt = null;
			if(m_routeTrack.Trackpoints.Count > 0)
			{
				lastWpt = (Waypoint)m_routeTrack.Trackpoints.GetByIndex(m_routeTrack.Trackpoints.Count - 1);
			}

			if(lastWpt == null || lastWpt.Location.distanceFrom(routeWpt.Location).Meters > 2.0d)
			{
				if(wasNew && lastWpt != null)
				{
					routeWpt.Location.Elev = lastWpt.Location.Elev;
				}
				m_routeTrack.Trackpoints.Add(routeWpt.DateTime, routeWpt);
				m_routeTrack.PutOnMap(this, null, this);

				if(wptName.Length > 2)
				{
					//m_cameraManager.MarkLocation(mouseGeoLocation, 0);
					m_cameraManager.ProcessCameraMove();		// make sure label is positionsed on the map
				} 
				else
				{
					// invalidate picture region around just created leg:
					Waypoint prevWpt = (Waypoint)m_routeTrack.Trackpoints.GetByIndex(m_routeTrack.Trackpoints.Count - 2);
					Point p1 = m_cameraManager.toPixelLocation(routeWpt.Location, null);
					Point p2 = m_cameraManager.toPixelLocation(prevWpt.Location, null);
					int x = Math.Min(p1.X, p2.X);
					int y = Math.Min(p1.Y, p2.Y);
					int w = Math.Abs(p1.X - p2.X);
					int h = Math.Abs(p1.Y - p2.Y);
					Rectangle toInv = new Rectangle(x, y, w, h);
					toInv.Offset(-5, -5);
					toInv.Inflate(10, 10);
					m_pictureManager.Invalidate(toInv);
				}
			}
		}
		
		public override void ProcessMouseClick(Point clickPoint, GeoCoord mouseGeoLocation, bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				if(makeRouteMode && !m_cameraManager.HasDragRectangle)
				{
					GeoCoord rteLoc = new GeoCoord(mouseGeoLocation);
					rteLoc.Elev = 0.0d;
					addLocToRoute(rteLoc);
				}
				else if(Project.photoSelectPhotoMode == 1)
				{
					Waypoint clickWpt = WaypointByPoint(clickPoint);
					if(clickWpt != null && clickWpt.ThumbImage != null)
					{
						Project.photoSelectPhotoMode = 2;
						Project.photoSelectedWptId = clickWpt.Id;
						Project.ShowPopup(Project.mainForm, "Now select a position where the photo should be, click on it", Project.mainForm.PointToScreen(clickPoint));
					}
					else
					{
						Project.ShowPopup(Project.mainForm, "please select a photo and click on it", Project.mainForm.PointToScreen(clickPoint));
					}
				}
				else if(Project.photoSelectPhotoMode == 2)
				{
					Project.photoSelectPhotoMode = 0;
					GeoCoord clickCoord = CameraManager.This.toGeoLocation(clickPoint);
					Project.photoSelectedCoord = new CamPos(clickCoord.Lng, clickCoord.Lat);
					//Project.ShowPopup(Project.mainForm, "Got the position where the photo should be: " + clickCoord, Point.Empty);

					if(Project.delegateProcessingComplete != null)
					{
						Project.delegateProcessingComplete();
					}
				}
				else if(m_cameraManager.CanDrillDown)
				{
					Waypoint clickWpt = WaypointByPoint(clickPoint);
					if(clickWpt != null)
					{
						if(altDown)
						{
							Point screenPoint = m_pictureManager.PointToScreen(clickPoint);
							Project.MapShowPopup(m_pictureManager.PictureBox, clickWpt.toStringPopup(), screenPoint);
						}
						else if(clickWpt.ThumbImage != null)
						{
							if(clickWpt.TrackId != -1L)
							{
								TrackProfileControl.setTrackByTrackpoint(clickWpt);
							}
							Project.mainCommand.photoViewerUp(clickWpt);
						}
						else 
						{
							string tmp = clickWpt.Url;
							if(tmp != null && tmp.StartsWith("http://"))
							{
								tmp = "\"" + tmp + "\"";
								//LibSys.StatusBar.Trace(tmp);
								Project.RunBrowser(tmp);
							}
							else if(tmp != null && tmp.IndexOf("\\") != -1 && File.Exists(tmp))
							{
								//LibSys.StatusBar.Trace(tmp);
								Project.RunFile(tmp);
							}
							else if(clickWpt.TrackId != -1L)
							{
								TrackProfileControl.setTrackByTrackpoint(clickWpt);

								if(clickWpt.isEndpoint)
								{
									Project.mainCommand.trackProperties(clickWpt.TrackId);
								}
							}
							else if(!Project.allowMapPopups)
							{
								Point screenPoint = m_pictureManager.PointToScreen(clickPoint);
								Project.MapShowPopup1(m_pictureManager.PictureBox, clickWpt.toStringPopup(), screenPoint);
							}
						}
					}
				}
			} 
			catch {}
		}
		
		private void PutOnMap()
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: LayerWaypoints:PutOnMap()  " + WaypointsCache.WaypointsAll.Count);
#endif

			foreach(LiveObject lo in WaypointsCache.WaypointsAll)
			{
				try 
				{
					lo.init(true);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LW:PutOnMap(): lo=" + lo + " " + e.Message);
				}
			}
			foreach(LiveObject lo in WaypointsCache.WaypointsAll)
			{
				try 
				{
					if(m_cameraManager.insideScreenRectangle(lo.Location))
					{
						lo.PutOnMap(this, null, this);
					} 
				}
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LW:PutOnMap(): lo=" + lo + " " + e.Message);
				}
			}
			foreach(LiveObject lo in WaypointsCache.WaypointsAll)
			{
				try 
				{
					if(m_cameraManager.insideScreenRectangle(lo.Location))
					{
						lo.AdjustPlacement(this, null, this);
					} 
				}
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LW:PutOnMap() - AdjustPlacement: lo=" + lo + " " + e.Message);
				}
			}
			foreach(Track trk in WaypointsCache.TracksAll)
			{
				try 
				{
					trk.PutOnMap(this, null, this);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LW:PutOnMap(): trk=" + trk + " " + e.Message);
				}
			}
			/*
			foreach(Track trk in WaypointsCache.TracksAll)
			{
				try 
				{
					trk.AdjustPlacement(this, null, this);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LW:PutOnMap(): trk=" + trk + " " + e.Message);
				}
			}
			*/
			m_hasPutOnMap = true;

			WaypointsCache.RefreshWaypointsDisplayed();

			//logCamtrackFrame();
		}

		private void logCamtrackFrame()
		{
			if(Project.camTrackOn)
			{
				CameraTrack cameraTrack = WaypointsCache.CameraManager.CameraTrack;
				XmlDocument xmlDoc = cameraTrack.XmlDoc;

				XmlNode wNode = xmlDoc.CreateNode(XmlNodeType.Element, "waypoints", null);
				XmlAttribute attr = xmlDoc.CreateAttribute("description");
				attr.InnerText = "Waypoints: " + DateTime.Now;
				wNode.Attributes.Append(attr);

				XmlNode tNode = xmlDoc.CreateNode(XmlNodeType.Element, "trackpoints", null);
				attr = xmlDoc.CreateAttribute("description");
				attr.InnerText = "Trackpoints: " + DateTime.Now;
				tNode.Attributes.Append(attr);

				ArrayList trackIds = new ArrayList();
				
				SortedList tmpList = new SortedList();
				// trackpoints are added to tmplist, other waypoints are added directly to <waypoints>:
				foreach(Waypoint wpt in WaypointsCache.WaypointsDisplayed)
				{
					try 
					{
						if(wpt.TrackId != -1)
						{
							tmpList.Add(wpt.DateTime, wpt);
						} 
						else
						{
							XmlNode wptNode = wpt.ToCamtrackXmlNode(xmlDoc);
							wNode.AppendChild(wptNode);
						}
					} 
					// we will probably lose some trackpoints (with same time, up to second). The benefit is that we will have a sorted list.
#if DEBUG
					catch (Exception e)
					{
						LibSys.StatusBar.Error("" + e);
					}
#else
					catch { }
#endif
				}

				attr = xmlDoc.CreateAttribute("count");
				attr.InnerText = "" + wNode.ChildNodes.Count;
				wNode.Attributes.Append(attr);

				cameraTrack.log(wNode);

				// now convert trackpoints list to nodes under <trackpoints>
				for(int i=0; i < tmpList.Count ;i++)
				{
					Waypoint wpt = (Waypoint)tmpList.GetByIndex(i);
					XmlNode wptNode = wpt.ToCamtrackXmlNode(xmlDoc);
					tNode.AppendChild(wptNode);
					if(!trackIds.Contains(wpt.TrackId))
					{
						trackIds.Add(wpt.TrackId);
					}
				}
				tmpList.Clear();

				attr = xmlDoc.CreateAttribute("count");
				attr.InnerText = "" + tNode.ChildNodes.Count;
				tNode.Attributes.Append(attr);

				cameraTrack.log(tNode);

				// make a list of track IDs used by trackpoints under <tracks>:
				XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, "tracks", null);
				foreach(long trackId in trackIds)
				{
					Track trk = WaypointsCache.getTrackById(trackId);
					if(trk != null)
					{
						XmlNode trkNode = xmlDoc.CreateNode(XmlNodeType.Element, "trk", null);
						attr = xmlDoc.CreateAttribute("trkid");
						attr.InnerText = "" + trk.Id;
						trkNode.Attributes.Append(attr);
						trkNode.InnerText = trk.Name;
						node.AppendChild(trkNode);
					}
				}
				attr = xmlDoc.CreateAttribute("count");
				attr.InnerText = "" + node.ChildNodes.Count;
				node.Attributes.Append(attr);

				cameraTrack.log(node);
			}
		}

		public override void Paint(object sender, PaintEventArgs e)
		{
			try 
			{
				base.Paint(sender, e);
				if(!Project.spoiling)
				{
					if(!HasPutOnMap) 
					{
						//LibSys.StatusBar.Trace("LayerWaypoints:Paint(): calling PutOnMap()");
						PutOnMap();
					}

					foreach(LiveObject lo in WaypointsCache.WaypointsAll)
					{
						if(lo.BoundingRect.IsEmpty)	// new object just added dynamically, or old object moved
						{
							lo.PutOnMap(this, null, this);
							PictureManager.Invalidate(lo.BoundingRect);
						}
						Point pixelPos = toPixelLocation(lo.Location, null);
						if(e.ClipRectangle.IntersectsWith(lo.BoundingRect)) 
						{
							lo.Paint(e.Graphics, this, null);
						}
					}

					foreach(LiveObject lo in WaypointsCache.WaypointsAll)
					{
						lo.AdjustPlacement(this, null, this);
					}

					foreach(LiveObject lo in WaypointsCache.WaypointsAll)
					{
						Point pixelPos = toPixelLocation(lo.Location, null);
						if(e.ClipRectangle.IntersectsWith(lo.BoundingRect)) 
						{
							lo.PaintLabel(e.Graphics, this, null, false);
						}
					}

					foreach(Track trk in WaypointsCache.TracksAll)
					{
						trk.Paint(e.Graphics, this, null);
					}

					if(moveLegMode)
					{
						int MARK_RADIUS = 5;

						int minX = Math.Min(Math.Min(m_movePoint.X - MARK_RADIUS - 1, hotLeg.WptFrom.PixelLocation.X), hotLeg.WptTo.PixelLocation.X);
						int maxX = Math.Max(Math.Max(m_movePoint.X + MARK_RADIUS + 1, hotLeg.WptFrom.PixelLocation.X), hotLeg.WptTo.PixelLocation.X);
						int minY = Math.Min(Math.Min(m_movePoint.Y - MARK_RADIUS - 1, hotLeg.WptFrom.PixelLocation.Y), hotLeg.WptTo.PixelLocation.Y);
						int maxY = Math.Max(Math.Max(m_movePoint.Y + MARK_RADIUS + 1, hotLeg.WptFrom.PixelLocation.Y), hotLeg.WptTo.PixelLocation.Y);

						m_lastMoveLegRect = new Rectangle(minX - 10, minY - 10, maxX - minX + 12, maxY - minY + 12);

						Rectangle r = new Rectangle(m_movePoint.X - MARK_RADIUS, m_movePoint.Y - MARK_RADIUS, MARK_RADIUS * 2, MARK_RADIUS * 2);
						e.Graphics.DrawRectangle(Pens.Red, r);
						e.Graphics.DrawLine(Pens.Red, m_movePoint, hotLeg.WptFrom.PixelLocation);
						e.Graphics.DrawLine(Pens.Red, m_movePoint, hotLeg.WptTo.PixelLocation);
					}
					else
					{
						m_lastMoveLegRect = Rectangle.Empty;
					}
				}
			}
			catch(Exception eee) 
			{
				//LibSys.StatusBar.Error("LayerWaypoints:Paint(): " + eee);
			}
		}

		//
		// printing support:
		//
		public override void printPaint(Graphics graphics)
		{
			foreach(LiveObject lo in WaypointsCache.WaypointsAll)
			{
				Point pixelPosPrint = toPixelLocationPrint(lo.Location, null);
				Point pixelPosDispl = lo.PixelLocation;
				int offsetX = pixelPosPrint.X - pixelPosDispl.X;
				int offsetY = pixelPosPrint.Y - pixelPosDispl.Y;
				lo.Paint(graphics, this, null, offsetX, offsetY);
				lo.PaintLabel(graphics, this, null, true, offsetX, offsetY);
			}

			foreach(Track trk in WaypointsCache.TracksAll)
			{
				trk.Paint(graphics, this, null, true);	// true for isPrint
			}
		}

		public override void printPaint2(Graphics graphics)
		{
		}

		// actually counts intesections' total surface in pixels:
		public int countIntersections(LiveObject llo, Rectangle r)
		{
			int total = 0;
			try
			{
				foreach(LiveObject lo in WaypointsCache.WaypointsAll) 
				{
					if(llo != lo)		// self don't count
					{
						Rectangle sr = lo.intersectionSensitiveRect();
						sr.Intersect(r);
						total += (sr.Width * sr.Height);
						sr = lo.intersectionSensitiveRect2();
						sr.Intersect(r);
						total += (sr.Width * sr.Height / 2);     // count only part of it, less important
					}
				}

				foreach(Track trk in WaypointsCache.TracksAll)
				{
					if(trk.Enabled)
					{
						for(int i=0; i < trk.TrackpointsNamed.Count ;i++)
						{
							Waypoint wpt = (Waypoint)trk.TrackpointsNamed[i];
							bool hasContent = wpt.hasContent;
							bool hasUrl = wpt.Url.Length > 0;
							bool isPhoto = wpt.ThumbImage != null;

							if((i==0 || i == trk.Trackpoints.Count-1 || hasContent || hasUrl || isPhoto) && llo != wpt)
							{
								Rectangle sr = wpt.intersectionSensitiveRect();
								sr.Intersect(r);
								total += (sr.Width * sr.Height);
								sr = wpt.intersectionSensitiveRect2();
								sr.Intersect(r);
								total += (sr.Width * sr.Height / 2);     // count only part of it, less important
							}
						}
					}
				}
				//LibSys.StatusBar.Trace("Intersection total=" + total + " at pos " + llo.LabelPosition);
			} 
			catch(Exception e)
			{
				LibSys.StatusBar.Error("LW:countIntersections " + e.Message);
			}
			return total;
		}

		// returns number of LiveObject based objects added to the list:
		public override int getCloseObjects(SortedList list, GeoCoord loc, double radiusMeters)
		{
			int count = 0;

			// list of LiveObject by key=double (distance from loc)
			try 
			{
				// WaypointsDisplayed contains trackpoints, we don't want them in the list so far
				foreach(Waypoint wpt in WaypointsCache.WaypointsAll)
				{
					double dMeters = loc.distanceFrom(wpt.Location).Meters;
					if(dMeters <= radiusMeters)
					{
						list.Add(dMeters, wpt);
						count++;
					}
				}
			} 
			catch {}

			return count;
		}
	}
}
