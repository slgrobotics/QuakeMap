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
using LibFormats;

namespace LibGps
{
	// any Magellan protocol packet, expands basic NMEA packet with pieces that can go to the GPS:
	public class MPacket : NmeaPacket
	{
        public MPacket() : base() { }

        public MPacket(string header) : base(header) { }

		// packs location into the next (usually first) 6 cells of the fields array
		public void packCoord(GeoCoord loc)
		{
			// $PMGNWPL,3339.889,N,11736.283,W,0000155,M,WPT001,this is comment,a*7E

			double Lat = loc.Lat;
			string sDir = Lat < 0 ? "S" : "N";
			double aLat = Math.Abs(Lat);
			int iLat = (int)Math.Floor(aLat);
			double latMin = aLat - iLat;
			double latMinM = Math.Round(latMin * 60.0d, 3);
			string format = latMinM < 10.0 ? "{0:D2}0{1:F3}" :"{0:D2}{1:F3}";
			string sLat = string.Format(format, iLat, latMinM);
			fields.Add(sLat);
			fields.Add(sDir);

			double Lng = loc.Lng;
			sDir = Lng < 0 ? "W" : "E";
			double aLng = Math.Abs(Lng);
			int iLng = (int)Math.Floor(aLng);
			double lngMin = aLng - iLng;
			double lngMinM = Math.Round(lngMin * 60.0d, 3);
			format = lngMinM < 10.0 ? "{0:D3}0{1:F3}" :"{0:D3}{1:F3}";
			string sLng = string.Format(format, iLng, lngMinM);
			fields.Add(sLng);
			fields.Add(sDir);

			int Elev = (int)loc.Elev;
			fields.Add("" + Elev);
			fields.Add("M");
		}
	}

	public class MPacketCmd : MPacket
	{
		public MPacketCmd(string cmd) : base("PMGNCMD")
		{
			fields.Add(cmd);
		}
	}

	public class MPacketCsm : MPacket
	{
		public MPacketCsm(string cs) : base("PMGNCSM")
		{
			fields.Add(cs);
		}
	}

	/// <summary>
	/// when a packet is received, it can be converted to a class packet using fromString() factory
	/// in this class
	/// </summary>
	public class MPacketReceived : MPacket
	{
		public string m_string = null;		// will be allocated as the packet is received
		public bool isGood = false;			// will be true if checksum is ok and basicParse runs well

		public MPacketReceived()
		{
		}

		// ToString is meaningful for packets being sent, but as this class is boxed into GPacket,
		// we need ToString to return packet body:
		public override string ToString()
		{
			return m_string;
		}

		// this is a "class packet" factory which converts received packets into usable classes
		// (packet classes defined in MagellanProtoDef.cs). We assume that basicParse() was run 
		// on this packet before calling fromString()
		public MPacket fromString()
		{
			MPacket ret = null;

			if(m_string == null)
			{
				logError("MPacketReceived:fromString() -- m_string==null");
				return ret;
			}

			switch(m_header)
			{
				case "PMGNVER":
					ret = this;	// all we need is already in fields array
					break;
				case "PMGNRTE":
					// $PMGNRTE,14,2,c,1,T01P03,a,T01P04,a*35
				case "PMGNTRK":
					// $PMGNTRK,3334.530,N,11739.837,W,00000,M,034414.24,A*62
				case "PMGNWPL":
					// $PMGNWPL,3328.069,N,11738.812,W,0000000,M,Road to,GCF320 = Road to Wat,a*68
					ret = this;	// all we need is already in fields array
					break;
				case "PMGNST":
				case "GPGSV":
				case "GPGGA":
				case "GPRMC":
				case "GPGLL":
					ret = this;	// all we need is already in fields array
					break;
				default:
					logError("MPacketReceived:fromString() -- unknown header=" + m_header);
					ret = this;	// may be all we need is already in fields array
					break;
			}

			return ret;
		}

		public void logError(string str)
		{
			LibSys.StatusBar.Trace(str);
		}
	}
}
