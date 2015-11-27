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
using System.Threading;

using LibSys;
using LibGeo;
using LibGps;

namespace LibGui
{
	/// <summary>
	/// ThreadGpsControl is actually talking to GPS, calling callbacks and events when needed.
	/// </summary>
	public class ThreadGpsControl
	{
		public event GpsMessageHandler  MessageCallback;
		public event GpsProgressHandler ProgressCallback;
		public event GpsCompleteHandler CompleteCallback;
		public event GpsRealTimeHandler RealTimeCallback;

		public string command = "";

		protected GpsBase				m_gps;
		protected GpsInsertWaypoint		m_insertWaypoint;
		private ArrayList				m_waypoints;
		private ArrayList				m_routes;
		public static int				m_errorAdviceTry = 0;

		public ThreadGpsControl(GpsBase gps, GpsInsertWaypoint insertWaypoint, ArrayList waypoints, ArrayList routes)
		{
			m_gps = gps;
			m_insertWaypoint = insertWaypoint;
			m_waypoints = waypoints;	// may be empty, but never null
			m_routes = routes;			// may be empty, but never null
		}

		private ArrayList wptBuffer = new ArrayList();

		private void bufferWaypoint(CreateInfo createInfo)
		{
			wptBuffer.Add(createInfo);
		}

		public void communicate()
		{
			if(Project.gpsSimulate)
			{
				m_gps.init();
				switch (command)
				{
					case "uploadTracks":
					case "uploadRoutes":
					case "downloadRoutes":
					case "uploadWaypoints":
					case "downloadWaypoints":
					case "getDeviceInfo":
						MessageCallback( "FYI: operation '" + command + "' ignored by GPS simulator");
						break;
					case "startRealtime":
						simulateStartRealTime(RealTimeCallback);
						break;
					default:
						MessageCallback( "Error: GPS operation '" + command + "' not supported");
						break;
				}
				CompleteCallback( "GPS communication complete (simulated)", true);
				m_gps.Dispose();
				return;
			}

			try 
			{
				bool isSuccess = false;
				m_gps.init();
				for(int cnt=0; cnt < 3 ;cnt++)
				{
					m_gps.IdentifyDeviceType();
					if(m_gps.DeviceValid())
					{
						break;
					}
					Thread.Sleep(2000);
				}
				if(m_gps.DeviceValid())
				{
					// now we know type of device and it's capabilities, do the task.
					string strDeviceInfo = "";
					if(!"getDeviceInfo".Equals(command))
					{
						ProgressCallback(m_gps.ToString(), -1, 0);
					}
					else
					{
						ProgressCallback("", -1, 0);
					}
					wptBuffer.Clear();
					switch (command)
					{
						case "uploadTracks":
							Project.inhibitRefresh = true;
							WaypointsCache.resetBoundaries();
							m_gps.GetTracks(new GpsInsertWaypoint(bufferWaypoint), ProgressCallback);
							MessageCallback( "IP: drawing tracks on the map\n\nUse 'Wpt Trk Rte' button to see the track." );
							// now when all waypoint/track infos are buffered, use them to fill cache
							foreach(CreateInfo createInfo in wptBuffer)
							{
								m_insertWaypoint(createInfo);
								isSuccess = true;
							}
							WaypointsCache.isDirty = isSuccess;
							break;
						case "uploadRoutes":
							Project.inhibitRefresh = true;
							WaypointsCache.resetBoundaries();
							m_gps.GetRoutes(new GpsInsertWaypoint(bufferWaypoint), ProgressCallback);
							MessageCallback( "IP: drawing routes on the map\n\nUse 'Wpt Trk Rte' button to see the routes." );
							// now when all waypoint/route infos are buffered, use them to fill cache
							foreach(CreateInfo createInfo in wptBuffer)
							{
								m_insertWaypoint(createInfo);
								isSuccess = true;
							}
							WaypointsCache.isDirty = isSuccess;
							break;
						case "uploadWaypoints":
							Project.inhibitRefresh = true;
							WaypointsCache.resetBoundaries();
							m_gps.GetWaypoints(new GpsInsertWaypoint(bufferWaypoint), ProgressCallback);
							MessageCallback( "IP: drawing waypoints on the map\n\nUse 'Wpt Trk Rte' button to see the list of waypoints." );
							// now when all waypoint/track infos are buffered, use them to fill cache
							foreach(CreateInfo createInfo in wptBuffer)
							{
								m_insertWaypoint(createInfo);
								isSuccess = true;
							}
							WaypointsCache.isDirty = isSuccess;
							break;
						case "downloadWaypoints":
							Project.inhibitRefresh = true;
							m_gps.PutWaypoints(m_waypoints, ProgressCallback);
							isSuccess = true;
							break;
						case "downloadRoutes":
							Project.inhibitRefresh = true;
							m_gps.PutRoutes(m_routes, ProgressCallback, Project.startRouteNumber);
							isSuccess = true;
							break;
						case "startRealtime":
							m_gps.StartRealTime(RealTimeCallback);
							isSuccess = true;
							break;
							//case "stopRealtime":		// cannot do it here, as it doesn't require separate thread		
							//	m_gps.StopRealTime();
							//	break;
						default:
							MessageCallback( "Error: ThreadGpsControl operation '" + command + "' not supported" );
							break;
						case "getDeviceInfo":
							strDeviceInfo = m_gps.ToString() + "\n\n";
							isSuccess = true;
							break;
					}
					wptBuffer.Clear();
					CompleteCallback( strDeviceInfo + "GPS communication complete", isSuccess);
				} 
				else 
				{
					switch(m_errorAdviceTry++)
					{
						case 0:
							CompleteCallback( "GPS not responding, check cable and baud rate.\nMake sure your GPS communication protocol is set to " + AllGpsDevices.AllMakesNames[Project.gpsMakeIndex] + ".", false);
							break;
						default:
							CompleteCallback( "GPS not responding, check cable and baud rate.", false);
							break;
					}
				}
			}
			catch (Exception exc)
			{
				switch(m_errorAdviceTry++)
				{
					case 0:
						CompleteCallback("" + exc.Message + "   -- make sure your GPS communication protocol is set to " + AllGpsDevices.AllMakesNames[Project.gpsMakeIndex] + ".", false);
						break;
					default:
						CompleteCallback("" + exc.Message, false);
						break;
				}
			}
			finally
			{
				Project.inhibitRefresh = false;
				m_gps.Dispose();
			}
		}

		private void simulateStartRealTime(GpsRealTimeHandler realtimeCallback)
		{
			int toWait = 3;
			int toCount = 10;
			double degIncr = 360.0d / (toCount * 4);
			double degree = 0.0d;
			int count = toCount;
			GeoCoord location = new GeoCoord(CameraManager.This.Location.Lng, CameraManager.This.Location.Lat);
			while(count-- > 0)
			{
				GpsRealTimeData rtData = new GpsRealTimeData();
				rtData.location = location;
				rtData.fix = 2;
				rtData.time = DateTime.Now;

				rtData.velocity = 10.0d;
				rtData.velocityEast = 5.0d * Math.Sin(degree * Math.PI / 180.0d);
				rtData.velocityNorth = 5.0d * Math.Cos(degree * Math.PI / 180.0d);
				degree += degIncr;

				realtimeCallback(rtData);

				location.Lat += 0.0005d;
				location.Lng += 0.0005d;

				Thread.Sleep(toWait);
			}
			count = toCount;
			while(count-- > 0)
			{
				GpsRealTimeData rtData = new GpsRealTimeData();
				rtData.location = location;
				rtData.fix = 2;
				rtData.time = DateTime.Now;

				rtData.velocity = 10.0d;
				rtData.velocityEast = 5.0d * Math.Sin(degree * Math.PI / 180.0d);
				rtData.velocityNorth = 5.0d * Math.Cos(degree * Math.PI / 180.0d);
				degree += degIncr;

				realtimeCallback(rtData);

				location.Lat -= 0.0005d;
				location.Lng += 0.0005d;

				Thread.Sleep(toWait);
			}
			count = toCount;
			while(count-- > 0)
			{
				GpsRealTimeData rtData = new GpsRealTimeData();
				rtData.location = location;
				rtData.fix = 2;
				rtData.time = DateTime.Now;

				rtData.velocity = 10.0d;
				rtData.velocityEast = 5.0d * Math.Sin(degree * Math.PI / 180.0d);
				rtData.velocityNorth = 5.0d * Math.Cos(degree * Math.PI / 180.0d);
				degree += degIncr;

				realtimeCallback(rtData);

				location.Lat -= 0.0005d;
				location.Lng -= 0.0005d;

				Thread.Sleep(toWait);
			}
			count = toCount;
			while(count-- > 0)
			{
				GpsRealTimeData rtData = new GpsRealTimeData();
				rtData.location = location;
				rtData.fix = 2;
				rtData.time = DateTime.Now;

				rtData.velocity = 10.0d;
				rtData.velocityEast = 5.0d * Math.Sin(degree * Math.PI / 180.0d);
				rtData.velocityNorth = 5.0d * Math.Cos(degree * Math.PI / 180.0d);
				degree += degIncr;

				realtimeCallback(rtData);

				location.Lat += 0.0005d;
				location.Lng -= 0.0005d;

				Thread.Sleep(toWait);
			}
		}
	}
}
