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

namespace LibSys
{
	// this class is needed for Favorites and Back/Forward functions:
	public class CamPos
	{
		private double m_X;
		public double Lng { get { return m_X; } set { m_X = value; } }
		public double X { get { return m_X; } set { m_X = value; } }

		private double m_Y;
		public double Lat { get { return m_Y; } set { m_Y = value; } }
		public double Y { get { return m_Y; } set { m_Y = value; } }

		private double m_H;	// elevation, meters
		public double Elev { get { return m_H; } set { m_H = value; } }
		public double H { get { return m_H; } set { m_H = value; } }
		public double Z { get { return m_H; } set { m_H = value; } }

		string m_name = "";
		public string Name { get { return m_name; } set { m_name = value; } }

		string m_type = "";
		public string Type { get { return m_type; } set { m_type = value; } }

		public CamPos(double lng, double lat)
		{
			m_X = lng;
			m_Y = lat;
			m_H = 4000.0d;
		}

		public CamPos(double lng, double lat, double elev)
		{
			m_X = lng;
			m_Y = lat;
			if(elev < 1.0d) 
			{
				elev = 4000.0d;
			}
			else
			{
				elev = Math.Max(50.0d, elev);
			}
			m_H = elev;
		}

		public CamPos(double lng, double lat, double elev, bool exactElev)
		{
			m_X = lng;
			m_Y = lat;
			m_H = elev;
		}

		public CamPos(double lng, double lat, double elev, string name, string type) : this(lng, lat, elev)
		{
			m_name = name;
			m_type = type;
		}
	}
}
