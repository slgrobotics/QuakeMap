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

using LibSys;
using LibGeo;
using LibNet;

namespace LibGui
{
	public delegate void DynamicObjectCreateHandler(LiveObject lo);
	public delegate void DynamicObjectDeleteHandler(LiveObject lo);
	public delegate void DynamicObjectMoveHandler(LiveObject lo, Rectangle previous);

	/// <summary>
	/// Summary description for VehiclesCache.
	/// </summary>
	public class VehiclesCache
	{
		private static ArrayList m_vehicles = new ArrayList();
		public  static ArrayList LiveObjects { get { return m_vehicles; } }

		public static event DynamicObjectCreateHandler DynamicObjectCreateCallback;
		public static event DynamicObjectDeleteHandler DynamicObjectDeleteCallback;
		public static event DynamicObjectMoveHandler DynamicObjectMoveCallback;

		public static void addVehicle(Vehicle veh)
		{
			m_vehicles.Add(veh);
			DynamicObjectCreateCallback(veh);		// takes care of displaying it on LayerVehicle
		}

		public static void deleteVehicle(Vehicle veh)
		{
			DynamicObjectDeleteCallback(veh);
			m_vehicles.Remove(veh);
			veh.cleanup();
		}

		public static Vehicle getVehicleByName(string name)
		{
			foreach(Vehicle veh in LiveObjects)
			{
				if(name.Equals(veh.Label))
				{
					return veh;
				}
			}
			return null;
		}

		public static void vehicleMoved(Vehicle veh, Rectangle prev)
		{
			DynamicObjectMoveCallback(veh, prev);
			keepInView();
		}

		/// <summary>
		/// keep ALL vehicles with keepInView flag on the screen
		/// </summary>
		public static void keepInView()
		{
			resetBoundaries();
			int count = 0;
			bool allInView = true;

			foreach(Vehicle veh in m_vehicles)
			{
				if(veh.KeepInView)
				{
					if(!PictureManager.This.CameraManager.isInView(veh.Location))
					{
						allInView = false;
					}
					pushBoundaries(veh.Location);
					count++;
				}
			}
			if(count > 1 && !allInView)
			{
				// add a margin of 1/4 to the boundaries to avoid zooming in too often:
				double covX = -TopLeft.Lng + BottomRight.Lng;
				double covY = TopLeft.Lat - BottomRight.Lat;
				double mFactor = 0.25d;

				TopLeft.Lng -= covX * mFactor;
				TopLeft.Lat += covY * mFactor;
				BottomRight.Lng += covX * mFactor;
				BottomRight.Lat -= covY * mFactor;

				PictureManager.This.CameraManager.zoomToCorners(TopLeft, BottomRight);
			}
			else if(count == 1)
			{
				PictureManager.This.CameraManager.keepInView(TopLeft);
			}
		}

		public static void init()
		{
			/*
			Vehicle veh;
			veh = new Vehicle(new GeoCoord(-179.9d, -15.0d), "auto", "http://www.quakemap.com/sergei/index.html");
			m_vehicles.Add(veh);
			veh = new Vehicle(new GeoCoord(179.9d, -15.0d), "auto", "http://www.quakemap.com/sergei/index.html");
			m_vehicles.Add(veh);
			*/
		}

		public static void shutdown()
		{
			//m_thread.Abort();
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

		#region Vehicle icons cache

		private static Hashtable bitmaps = new Hashtable();		// bitmap by sym
		private static Hashtable tried   = new Hashtable();		// if already tried to load bitmap by name, bool
		//private static Hashtable bitmaps = new Hashtable();		// bitmap by name

		public static Bitmap getVehicleImage(string sym)
		{
			if(sym == null || sym.Length == 0 || "none".Equals(sym))
			{
				return null;
			}

			Bitmap ret = (Bitmap)bitmaps[sym];

			if(ret != null)
			{
				return ret;
			}

			try 
			{
				string filePath = Project.GetVehPath(sym + ".gif");
				if(!File.Exists(filePath) && (tried[sym] == null || !(bool)tried[sym]))
				{
					tried[sym] = true;
					// load the file remotely, don't hold diagnostics if load fails:
					string url = Project.MISC_FOLDER_URL + "/vehicles/" + sym + ".gif";
					DloadProgressForm loaderForm = new DloadProgressForm(url, filePath, false, true);
					loaderForm.ShowDialog();
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

		#region testing code

		/*
		private static Thread m_thread;
		static VehiclesCache()
		{
			m_thread = new Thread( new ThreadStart(Run) );
			m_thread.Priority = ThreadPriority.Lowest;
			m_thread.Start();
		}

		private static double m_lng;
		private static double m_lat;

		public static void Run()
		{
			Vehicle veh = null;

			while(true)
			{
				if(Project.drawableReady && Project.cameraElev > 0.0d)
				{
					//LibSys.StatusBar.Trace("IP: VehiclesCache:Run()");
					try
					{
						/*
						 * this is how to move object via Create/Remove:
						if(veh == null)
						{
							m_lng = Project.cameraLng;
							m_lat = Project.cameraLat;
						}
						else 
						{
							deleteVehicle(veh);
						}
						veh = new Vehicle(new GeoCoord(m_lng, m_lat), "auto", "http://www.quakemap.com/sergei/index.html");
						m_vehicles.Add(veh);
						vehicleCreated(veh);
						* /

						// better way to move object around is via Move:
						if(veh == null) 
						{
							m_lng = Project.cameraLng;
							m_lat = Project.cameraLat;
							veh = new Vehicle(new GeoCoord(m_lng, m_lat), "auto", "http://www.quakemap.com/sergei/index.html");
							addVehicle(veh);
						}
						else
						{
							Rectangle prev = veh.BoundingRect;
							veh.Location.Lng = m_lng;
							veh.Location.Lat = m_lat;
							veh.tick();
							vehicleMoved(veh, prev);
							m_lng -= 0.001d;
							m_lat -= 0.001d;
						}
					}
					catch (Exception ee)
					{
						LibSys.StatusBar.Error("VehiclesCache:Run() - " + ee);
					}
				}
				Thread.Sleep(1000);
			}
		}
		*/
		#endregion
	}
}
