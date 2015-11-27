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
using System.Xml;

namespace LibSys
{
	public delegate void InsertWaypoint(CreateInfo createInfo);
	public delegate void InsertEarthquake(string[] infos); //CreateInfo createInfo);

	/// <summary>
	/// CreateInfo is a container to pass parameters to functions like WaypointsCache.insertWaypoint() in the most generic way.
	/// The delegates above define formal interface. 
	/// </summary>
	public class CreateInfo : ICloneable
	{
		// generic often required fields to be passed to insert functions:
		public long		id = -1;			// usually track id, for creating one or relating to one
		public string	type = null;		// object type, something like "earthquake", "wpt", "geocache", "trkpt", "rtept", "trk", "rte"
		public string	typeExtra = null;	// something like "Geocache|Traditional Cache" or "Geocache" or "Geocache Found" - <type> tag in GPX
		public double	lat = 0.0d;
		public double	lng = 0.0d;
		public double	elev = 0.0d;
		public double	magn = 0.0d;
		public DateTime dateTime = DateTime.MinValue;	// always zulu
		public string	name = null;		// shortest possible, as it will be often shown on map (by user's choice) - like GCSDXYZ
		public string	urlName = null;		// actually a longer name, may be quite descriptive	- like "Mike's Memorial Cache"
		public string	url = null;
		public string	desc = null;		// description, sometimes including urlName or name too
		public string	comment = null;		// comment, "<cmt>" tag in GPX
		public string	sym = null;			// symbol to show on map
		public string	source = null;		// file or device (gps) name

		// some exotic cases that don't fit right into the generic fields above:
		public string	par1 = null;		// just in case, used for route number so far
		public XmlNode	node1 = null;		// for passing portions of XML documents to be parsed inside the "insert" functions; i.e. groundspeak extensions.
		public XmlNode	node2 = null;

		public CreateInfo()
		{
		}

        public bool Equals(CreateInfo other)
        {
            return  other != null &&
                    //other.id == id &&
                    other.type == type &&
                    other.typeExtra == typeExtra &&
                    other.lat == lat &&
                    other.lng == lng &&
                    other.elev == elev &&
                    other.magn == magn &&
                    other.dateTime == dateTime &&
                    other.name == name &&
                    other.urlName == urlName &&
                    other.url == url &&
                    other.desc == desc &&
                    other.comment == comment &&
                    other.sym == sym &&
                    other.source == source &&
                    other.par1 == par1 &&
                    other.node1 == node1 &&
                    other.node2 == node2;
        }

        public bool SameSpot(CreateInfo other)
        {
            return other != null &&
                    other.lat == lat &&
                    other.lng == lng &&
                    other.elev == elev;
        }

        public object Clone()
        {
            CreateInfo clone = new CreateInfo();

            clone.id = id;
            clone.type = type;
            clone.typeExtra = typeExtra;
            clone.lat = lat;
            clone.lng = lng;
            clone.elev = elev;
            clone.magn = magn;
            clone.dateTime = dateTime;
            clone.name = name;
            clone.urlName = urlName;
            clone.url = url;
            clone.desc = desc;
            clone.comment = comment;
            clone.sym = sym;
            clone.source = source;
            clone.par1 = par1;
            clone.node1 = node1;
            clone.node2 = node2;

            return clone;
        }

		/// <summary>
		/// Clone constructor is needed for Magellan GetRoutes, when CreateInfo instances are buffered
		/// </summary>
		/// <param name="ci"></param>
		public CreateInfo(CreateInfo ci)
		{
			id = ci.id;
			type = ci.type;
			typeExtra = ci.typeExtra;
			lat = ci.lat;
			lng = ci.lng;
			elev = ci.elev;
			magn = ci.magn;
			dateTime = ci.dateTime;
			name = ci.name;
			urlName = ci.urlName;
			url = ci.url;
			desc = ci.desc;
			comment = ci.comment;
			sym = ci.sym;
			source = ci.source;
			par1 = ci.par1;
			node1 = ci.node1;	// ref
			node2 = ci.node2;	// ref
		}

		/// <summary>
		/// CreateInfo is often reused, and init() is designed to make it like new
		/// </summary>
		public void init(string _type)
		{
			id = -1;
			type = _type;
			typeExtra = null;
			lat = 0.0d;
			lng = 0.0d;
			elev = 0.0d;
			magn = 0.0d;
			dateTime = DateTime.MinValue;
			name = null;
			urlName = null;
			url = null;
			desc = null;
			comment = null;
			sym = null;
			source = null;
			par1 = null;
			node1 = null;
			node2 = null;
		}

		/// <summary>
		/// set lat from a string in all possible ways (-23.234 or 33.6033N)
		/// </summary>
		/// <param name="sLat"></param>
		public void setLat(string sLat)
		{
			sLat = sLat.ToLower().Trim();
			bool isSouth = false;
			if(sLat.EndsWith("n") || (isSouth=sLat.EndsWith("s"))) 
			{
				lat = Convert.ToDouble(sLat.Substring(0, sLat.Length - 1));
				if(isSouth) 
				{
					lat = -lat;
				}
			} 
			else 
			{
				lat = Convert.ToDouble(sLat);
			}
		}

		/// <summary>
		/// set lng from a string in all possible ways (-117.234 or 117.6033W)
		/// </summary>
		/// <param name="sLng"></param>
		public void setLng(string sLng)
		{
			sLng = sLng.ToLower().Trim();
			bool isEast = false;
			if(sLng.EndsWith("w") || (isEast=sLng.EndsWith("e"))) 
			{
				lng = Convert.ToDouble(sLng.Substring(0, sLng.Length - 1));
				if(isEast) 
				{
					lng = -lng;
				}
			} 
			else 
			{
				lng = Convert.ToDouble(sLng);
			}
		}

		public void setElev(string sElev)
		{
			try 
			{
				elev = Convert.ToDouble(sElev.Trim());
			} 
			catch
			{
				elev = 0.0d;
			}
		}

		public void setDateTime(string sDateTime)
		{
			sDateTime = sDateTime.Trim();
			if(sDateTime.EndsWith("Z"))		// UTC (zulu) time is supposed to be there, as came from GPS or .gpx file
			{
				// deceive Convert into thinking that it is local timezone (by removing Z at the end)
				string tmp = sDateTime.Substring(0, sDateTime.Length - 1);
				dateTime = Convert.ToDateTime(tmp);
				// now we have zulu time in dateTime
			} 
			else
			{
				// not supposed to happen for waypoints
				dateTime = Convert.ToDateTime(sDateTime);		// zulu
			}
		}
	}
}
