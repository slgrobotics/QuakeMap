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

using LibSys;
using LibGeo;

namespace LibGps
{
	/// <summary>
	/// GarminProtoAppLayer translates GPS-context calls into send/receive-packet requests to GarminProtoLinkLayer.
	/// </summary>
	public class GarminProtoAppLayer : IDisposable
	{
		protected GarminProtoLinkLayer m_linkLayer;

		public GarminProtoAppLayer()
		{
			m_linkLayer = new GarminProtoLinkLayer();
		}

		public void Dispose() 
		{
			m_linkLayer.Dispose();
		}

		public GarminProductType IdentifyDeviceType()
		{
			return m_linkLayer.IdentifyDeviceType();
		}

		public void GetTracks(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			m_linkLayer.getTrackLogTransferProtocol().GetTracks(insertWaypoint, progressCallback);
		}

		public void GetRoutes(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			m_linkLayer.getRouteTransferProtocol().GetRoutes(insertWaypoint, progressCallback);
		}

		public void GetWaypoints(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			m_linkLayer.getWaypointTransferProtocol().GetWaypoints(insertWaypoint, progressCallback);
		}

		public void PutWaypoints(ArrayList waypoints, GpsProgressHandler progressCallback)
		{
			m_linkLayer.getWaypointTransferProtocol().PutWaypoints(waypoints, progressCallback);
		}

		public void PutRoutes(ArrayList routes, GpsProgressHandler progressCallback, int startRouteNumber)
		{
			m_linkLayer.getRouteTransferProtocol().PutRoutes(routes, progressCallback, startRouteNumber);
		}

		public void StartRealTime(GpsRealTimeHandler realtimeCallback)
		{
			m_linkLayer.getPvtTransferProtocol().StartRealTime(realtimeCallback);
		}

		public void StopRealTime()
		{
			try
			{
				m_linkLayer.getPvtTransferProtocol().StopRealTime();
			}
			catch {}
		}

		public void log(string str)
		{
			m_linkLayer.log(str);
		}
	}
}
