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

namespace LibGps
{
	/// <summary>
	/// Gps Controller for Magellan devices.
	/// </summary>
	public class GpsMagellan : GpsBase
	{
		protected MagellanProductType m_deviceType = null;
		protected MagellanProtoAppLayer m_appLayer;

		public GpsMagellan()
		{
		}

		public override void init()	// throws GpsException
		{
			m_appLayer = new MagellanProtoAppLayer();
			Thread.Sleep(100);
		}

		public override void Dispose() 
		{
			m_appLayer.Dispose();
		}

		public override bool DeviceValid()			// IdentifyDeviceType found something real
		{
			return m_deviceType != null;
		}

		public override void IdentifyDeviceType()
		{
			m_deviceType = m_appLayer.IdentifyDeviceType();
		}

		public override void GetTracks(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			m_appLayer.GetTracks(insertWaypoint, progressCallback);
		}

		public override void GetRoutes(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			m_appLayer.GetRoutes(insertWaypoint, progressCallback);
		}

		public override void PutRoutes(ArrayList routes, GpsProgressHandler progressCallback, int startRouteNumber)
		{
			m_appLayer.PutRoutes(routes, progressCallback, startRouteNumber);
		}

		public override void GetWaypoints(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			m_appLayer.GetWaypoints(insertWaypoint, progressCallback);
		}

		public override void PutWaypoints(ArrayList waypoints, GpsProgressHandler progressCallback)
		{
			m_appLayer.PutWaypoints(waypoints, progressCallback);
		}

		public override void StartRealTime(GpsRealTimeHandler realtimeCallback)
		{
			m_appLayer.StartRealTime(realtimeCallback);
		}

		public override void StopRealTime()
		{
			m_appLayer.StopRealTime();
		}

		public override string ToString()
		{
			if(m_deviceType != null)
			{
				return "" + m_deviceType;
			}
			else
			{
				return "Error: GPS device not accessible";
			}
		}
	}
}
