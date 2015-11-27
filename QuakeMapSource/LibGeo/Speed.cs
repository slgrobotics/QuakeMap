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
using LibSys;

namespace LibGeo
{
	/// <summary>
	/// use Speed class to represent speed (distance/time) wherever needed. Inherits units from Distance.
	/// </summary>
	public class Speed : Distance
	{
		public bool isValid = true;

		public Speed(double d /* meters per hour */)
			: base(d)
		{
			if (d < 0.0d)
			{
				isValid = false;
			}
			roundToInts = false;
		}

		public bool isSane { get { return Meters > 1.0d && Meters < 330.0d * 3600.0d; } }	// sanity check - speed of sound, m/hr

		public override string ToString(int un)
		{
			string ret = "";
			if(isValid && isSane)
			{
				switch(un) 
				{ 
					case UNITS_DISTANCE_M:
					case UNITS_DISTANCE_FEET:
						// for meters and feet we have per second:
						double d = m_distance;
						m_distance /= 3600.0d;
						ret = base.ToString(un);
						m_distance = d;
						break;
					default:
						// km, knots and miles are per hour:
						ret = base.ToString(un);
						break;
				}
			}
			else
			{
				ret = "n/a";
			}
			return ret;
		}

		public override string ToString()
		{
			return this.ToString(Project.unitsDistance);
		}

		public override string toStringU(int un)
		{
			// for meters and feet we have per second:
			switch(un) 
			{ 
				case UNITS_DISTANCE_M:
				case UNITS_DISTANCE_FEET:
					return base.toStringU(un) + "/sec";
				case UNITS_DISTANCE_NMILES:
					return "knots";
			}
			// km and miles are per hour:
			return base.toStringU(un) + "/hr";
		}
	}
}
