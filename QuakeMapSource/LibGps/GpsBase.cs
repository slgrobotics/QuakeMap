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
	public delegate void GpsMessageHandler(string message);
	public delegate void GpsProgressHandler(string message, int unitsRead, int unitsTotal);
	public delegate void GpsCompleteHandler(string message, bool isSuccess);
	public delegate void GpsRealTimeHandler(GpsRealTimeData rtData);

	/// <summary>
	/// GpsBase - base class for all GPS devices
	/// </summary>
	public abstract class GpsBase : IDisposable
	{
		public GpsBase()		{}
		public abstract void init();	// throws GpsException
		public abstract void IdentifyDeviceType();	// throws GpsException
		public abstract bool DeviceValid();			// IdentifyDeviceType found something real
		public abstract void GetTracks(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback);
		public abstract void GetWaypoints(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback);
		public abstract void GetRoutes(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback);
		public abstract void PutWaypoints(ArrayList waypoints, GpsProgressHandler progressCallback);
		public abstract void PutRoutes(ArrayList routes, GpsProgressHandler progressCallback, int startRouteNumber);
		public abstract void StartRealTime(GpsRealTimeHandler realtimeCallback);
		public abstract void StopRealTime();
		public abstract void Dispose();
	}
}
