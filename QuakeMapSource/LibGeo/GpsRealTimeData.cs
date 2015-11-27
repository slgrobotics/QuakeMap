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

namespace LibGeo
{
	/// <summary>
	/// GpsRealTimeData describes all positioning error data and velocity on top of location/time pair
	/// as such data is provided by GPS, it repeats some specific GPS values 
	/// </summary>
	public class GpsRealTimeData
	{
		// generic values:
		public GeoCoord location = null;
		public DateTime time = DateTime.MinValue;		// always Zulu
		public string comment = "no data";

		// Magellan inspired:


		// Garmin-inspired values:
		public double posError;			// estimated position error, meters
		public double posErrorH;		// horizontal
		public double posErrorV;		// vertical
		public double velocity;			// meters/sec
		public double velocityEast;		// meters/sec
		public double velocityNorth;	// meters/sec
		public double velocityUp;		// meters/sec
		/* fix is same as Garmin D800_Pvt_Data_Type_Fix_Type (GarminProtoDef.cs)
		unusable = 0,		// failed integrity check
		invalid = 1,		// invalid or unavailable
		_2D = 2,			// two dimensional
		_3D = 3,			// three dimensional
		_2D_diff = 4,		// two dimensional differential
		_3D_diff = 5		// three dimensional differential
		*/
		public int fix = -1;			// indicates that packet has not come from GPS
		public string fixStr 
		{
			get 
			{
				string sFix = "" + fix;
				switch (fix)
				{
					case -1:
						sFix = "";
						break;
					case 0:
						sFix = "failed integrity check";
						break;
					case 1:
						sFix = "invalid or unavailable";
						break;
					case 2:
						sFix = "two dimensional";
						break;
					case 3:
						sFix = "three dimensional";
						break;
					case 4:
						sFix = "two dimensional differential";
						break;
					case 5:
						sFix = "three dimensional differential";
						break;
					case 6:
						sFix = "estimated";		// Magellan
						break;
				}
				return sFix;
			}
		}
	}
}
