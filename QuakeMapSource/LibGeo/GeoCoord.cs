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
using System.Text;

using LibSys;

namespace LibGeo
{
	/// <summary>
	/// Summary description for GeoCoord.
	/// </summary>
	public class GeoCoord
	{
		public const double EARTH_RADIUS = 6371000.0d;		// meters

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

		public static GeoCoord This = new GeoCoord(0.0d, 0.0d);

		public GeoCoord(double lng, double lat)
		{
			m_X = lng;
			m_Y = lat;
			m_H = 0.0d;
		}

		public GeoCoord(double lng, double lat, double elev)
		{
			m_X = lng;
			m_Y = lat;
			m_H = elev;
		}

		public GeoCoord(CamPos camPos)
		{
			m_X = camPos.Lng;
			m_Y = camPos.Lat;
			m_H = camPos.Elev;
		}

		public GeoCoord(GeoCoord loc)
		{
			m_X = loc.Lng;
			m_Y = loc.Lat;
			m_H = loc.Elev;
		}

		public 	GeoCoord(string sPos)	// sPos in form: W117.23'45" N36.44'48" or N33,27.661 W117,42.945
		{
			m_X = 0;
			m_Y = 0;
			m_H = 0;
			bool west  = sPos.IndexOf('W') != -1;
			bool north = sPos.IndexOf('N') != -1;
			int spaceIndex = sPos.IndexOf(' ');
			if(spaceIndex > 0) 
			{
				if(sPos.StartsWith("N") || sPos.StartsWith("S"))
				{
					string sLatitude = sPos.Substring(1, spaceIndex-1);
					m_Y = toDegree(sLatitude);
					if(!north) 
					{
						m_Y *= -1;
					}
					string sLongitude = sPos.Substring(spaceIndex + 2);
					m_X = toDegree(sLongitude);
					if(west) 
					{
						m_X *= -1;
					}
				}
				else
				{
					string sLongitude = sPos.Substring(1, spaceIndex-1);
					m_X = toDegree(sLongitude);
					if(west) 
					{
						m_X *= -1;
					}
					string sLatitude = sPos.Substring(spaceIndex + 2);
					m_Y = toDegree(sLatitude);
					if(!north) 
					{
						m_Y *= -1;
					}
				}
			}
			// System.Console.WriteLine("BACK: " + toString());
		}

		protected double toDegree(string str)	// str in form: 117.23'45 or 117,42.945
		{
			// System.Console.WriteLine("TO DEGREE: |" + str + "|");
			int i = 0;
			if(str.IndexOf(",") > 0)
			{
				int j = str.IndexOf(',');
				string val = str.Substring(0, j);
				// System.Console.WriteLine("val: |" + val + "|");
				int deg = Convert.ToInt32(val);
				i = j + 1;
				val = str.Substring(i);
				// System.Console.WriteLine("val: |" + val + "|");
				double min = Convert.ToDouble(val);
				return ((double)deg) + min / 60;
			}
			else
			{
				int j = str.IndexOf('.');
				string val = str.Substring(0, j);
				// System.Console.WriteLine("val: |" + val + "|");
				int deg = Convert.ToInt32(val);
				i = j + 1;
				j = str.IndexOf('\'');
				val = str.Substring(i, j-i);
				// System.Console.WriteLine("val: |" + val + "|");
				int min = Convert.ToInt32(val);
				i = j + 1;
				val = str.Substring(i).Replace("\"", "");
				// System.Console.WriteLine("val: |" + val + "|");
				int sec = Convert.ToInt32(val);
				return ((double)deg) + ((double)min) / 60 + ((double)sec) / 3600;
			}
		}

		public void Normalize()
		{
			while(m_X >= 180.0d) 
			{
				m_X -= 360.0d;
			}

			while(m_X < -180.0d)
			{
				m_X += 360.0d;
			}
		}
    
		public void	translate(double x, double y, double h)
		{
			m_X = x;
			m_Y = y;
			m_H = h;
		}

		public void	translate(GeoCoord to)
		{
			m_X = to.x();
			m_Y = to.y();
			m_H = to.h();
		}

		public override bool Equals(object obj)
		{
			if(obj == null) 
			{
				return false;
			}
			GeoCoord other = (GeoCoord)obj;
			//other.normalize();
			//normalize();
			return other.Lat == m_Y && other.Lng == m_X && other.Elev == m_H;
		}

		public bool sameAs(object obj)
		{
			if(obj == null) 
			{
				return false;
			}
			GeoCoord other = (GeoCoord)obj;
			other.Normalize();
			Normalize();
			return other.Lat == m_Y && other.Lng == m_X; // && other.Elev == m_H;
		}

		// tolerance 0.0001 degree = 10 meters
		public bool almostAs(object obj, double tolerance)
		{
			if(obj == null) 
			{
				return false;
			}
			GeoCoord other = (GeoCoord)obj;
			other.Normalize();
			Normalize();
			return Math.Abs(other.Lat - m_Y) <= tolerance && Math.Abs(other.Lng - m_X) <= tolerance; // && other.Elev == m_H;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public 	GeoCoord(double x, double xmin, double xsec,
			double y, double ymin, double ysec)
		{
			m_X = x + xmin/60 + xsec/3600;
			m_Y = y + ymin/60 + ysec/3600;
		}

		public double x()
		{
			return m_X;
		}

		public double y()
		{
			return m_Y;
		}

		public double h()
		{
			return m_H;
		}

		public double z()	// another name for "h"
		{
			return m_H;
		}

		public Distance distanceFrom(GeoCoord from)
		{
			/*	old version:
			double x = this.subtract(from, false).x();
			double y = this.subtract(from, false).y();

			// a grad square is cos(latitude) thinner, we need latitude in radians:
			double midLatRad = (this.add(from).y() / 2.0d) * Math.PI / 180.0d;
			double latitudeFactor = Math.Cos(midLatRad);
			double xMeters = Math.Abs(Distance.METERS_PER_DEGREE * x * latitudeFactor); 
			double yMeters = Math.Abs(Distance.METERS_PER_DEGREE * y);
			Distance distance = new Distance(Math.Sqrt(xMeters*xMeters + yMeters*yMeters));
			*/

			/*
			// see http://www.malaysiagis.com/related_technologies/gps/article3.cfm for the formula and some code
			// see http://www.fcaglp.unlp.edu.ar/~esuarez/gmt/1997/0148.html for more
			double Lon1 = this.Lng;
			double Lon2 = from.Lng;
			double Lat1 = this.Lat;
			double Lat2 = from.Lat;

			double rad_dist = Math.Acos(Math.Sin(Lat1) * Math.Sin(Lat2) + Math.Cos(Lat1) * Math.Cos(Lat2) * Math.Cos(Lon1 - Lon2));
			// to convert from radians to nautical miles, use the formula NM = rad_dist × 3437.7387. Then you can convert to land miles or kilometers.
			//double meters = rad_dist * rEarth;	//3437.7387d * Distance.METERS_PER_NAUTICAL_MILE;
			*/

			/*
			double p1 = Math.Cos(Lat) * Math.Cos(Lng) * Math.Cos(from.Lat) * Math.Cos(from.Lng);
			double p2 = Math.Cos(Lat) * Math.Sin(Lng) * Math.Cos(from.Lat) * Math.Sin(from.Lng);
			double p3 = Math.Sin(Lat) * Math.Sin(from.Lat);

			double rad_dist = Math.Acos(p1 + p2 + p3);
			*/

			/*
			double lon1 = this.Lng;
			double lon2 = from.Lng;
			double lat1 = this.Lat;
			double lat2 = from.Lat;

			double dlon = (lon2 - lon1);
			double dlat = (lat2 - lat1);
	
			double a = (Math.Sin(dlat/2))*(Math.Sin(dlat/2)) +
				(Math.Cos(lat1) * Math.Cos(lat2) * (Math.Sin(dlon/2))) *
				(Math.Cos(lat1) * Math.Cos(lat2) * (Math.Sin(dlon/2)));
	
			double rad_dist = 2 * Math.Asin(Math.Min(1.0,Math.Sqrt(a)));
			*/

			/*
			// from http://www.irbs.com/lists/navigation/0111/0022.html
			// The haversine distance formula goes as follows: 
			double lon1 = this.Lng;
			double lon2 = from.Lng;
			double lat1 = this.Lat;
			double lat2 = from.Lat;

			double DLat = lat2 - lat1;
			double DLo = lon2 - lon1; 

			double p1 = Math.Sin(DLat/2);
			double p2 = Math.Sin(DLo/2);
			double A = p1 * p1 + Math.Cos(lat2) * Math.Cos(lat1) * p2 * p2;

			double meters = EARTH_RADIUS * Math.Asin (Math.Sqrt(A)); 
			*/

			// from http://www.movable-type.co.uk/scripts/LatLong.html
			double lon1 = this.Lng * Math.PI / 180.0d;
			double lon2 = from.Lng * Math.PI / 180.0d;
			double lat1 = this.Lat * Math.PI / 180.0d;
			double lat2 = from.Lat * Math.PI / 180.0d;

			double dLat  = lat2 - lat1;
			double dLong = lon2 - lon1;

			double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLong/2) * Math.Sin(dLong/2);
			double c = 2.0d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0d - a));
			double meters = EARTH_RADIUS * c;

			Distance distance = new Distance(meters);

			return distance;
		}

		// returns bearing in rads. To get degrees, multiply by 180.0d / Math.PI
		public double bearing(GeoCoord nextLoc)
		{
			double Lon1 = this.Lng * Math.PI / 180.0d;
			double Lon2 = nextLoc.Lng * Math.PI / 180.0d;
			double Lat1 = this.Lat * Math.PI / 180.0d;
			double Lat2 = nextLoc.Lat * Math.PI / 180.0d;

			double y = Math.Sin(Lon1-Lon2) * Math.Cos(Lat2);
			double x = Math.Cos(Lat1) * Math.Sin(Lat2) - Math.Sin(Lat1) * Math.Cos(Lat2) * Math.Cos(Lon1 - Lon2);

			// from http://www.movable-type.co.uk/scripts/LatLong.html
			if (Math.Sin(Lon2 - Lon1) > 0.0)
			{
				return(Math.Atan2(-y, x));
			} 
			else
			{
				return(2.0d * Math.PI - Math.Atan2(y, x));
			}

			/*
			// see http://www.malaysiagis.com/related_technologies/gps/article3.cfm for the formula and some code
			// see http://www.fcaglp.unlp.edu.ar/~esuarez/gmt/1997/0148.html for more
			double ret = 0.0d;

			double rad_bearing;

			double rad_dist = Math.Acos(Math.Sin(Lat1) * Math.Sin(Lat2) + Math.Cos(Lat1) * Math.Cos(Lat2) * Math.Cos(Lon1 - Lon2));

			if (Math.Sin(Lon2 - Lon1) > 0.0)
			{
				double t1 = Math.Sin(Lat2) - Math.Sin(Lat1) * Math.Cos(rad_dist);
				double t2 = Math.Cos(Lat1) * Math.Sin(rad_dist);
				double t3 = t1 / t2;
				double t4 = Math.Atan(-t3 / Math.Sqrt(-t3 * t3 + 1)) + 2 * Math.Atan(1);
				rad_bearing = t4;
			}
			else
			{
				double t1 = Math.Sin(Lat2) - Math.Sin(Lat1) * Math.Cos(rad_dist);
				double t2 = Math.Cos(Lat1) * Math.Sin(rad_dist);
				double t3 = t1 / t2;
				double t4 = -t3 * t3 + 1;
				double t5 = 2.0d * Math.PI - (Math.Atan(-t3 / Math.Sqrt(-t3 * t3 + 1)) + 2 * Math.Atan(1));
				rad_bearing = t5;
			}

			ret = rad_bearing;

			return ret;
			*/
		}

		public double magneticVariation()
		{
			// see http://www.csgnetwork.com/e6bcalc.html   - US Continental only

			double lat = m_Y;
			double lon = -m_X;

			double v = -65.6811 + .99 * lat + .0128899 * Math.Pow(lat, 2) - .0000905928 *

				Math.Pow(lat, 3) + 2.87622 * lon - .0116268 * lat * lon - .00000603925 *

				Math.Pow(lat, 2) * lon - .0389806 * Math.Pow(lon, 2) - .0000403488 *

				lat * Math.Pow(lon, 2) + .000168556 * Math.Pow(lon, 3);

			return v;
		}

		public GeoCoord add(GeoCoord a)
		{
			return new GeoCoord(m_X + a.x(), m_Y + a.y(), m_H + a.h());
		}

		public GeoCoord subtract(GeoCoord a, bool spans180)
		{
			double x = a.x();
			double dx = m_X - x;
			if(spans180) 
			{     // dx < 360.0 && Math.Abs(dx) > 180.0) {    
				if(x > 90.0 && m_X < -90) 
				{
					x -= 360.0;
				} 
				else if(m_X > 90.0 && x < -90) 
				{
					x += 360.0;
				}
				dx = m_X - x;
			}
			double dy = m_Y - a.y();
			double dz = m_H - a.h();
			return new GeoCoord(dx, dy, dz);
		}

		public bool topLeft(double x, double y)
		{
			bool spans180 = false;
			// System.Console.WriteLine("GeoCoord::topLeft():      before  x=" + x + " m_X=" + m_X + "  y=" + y + " m_Y=" + m_Y);
			if(Math.Abs(x - m_X) < 180.0) 
			{ 
				if(x < m_X) 
				{
					m_X = x;
				}
			} 
			else 
			{
				if(x > 90.0 && m_X < -90) 
				{
					spans180 = true;
					x -= 360.0;
					if(x < m_X) 
					{
						m_X = x;
						m_X += 360.0;
					}
				}
			}
			if(y > m_Y) 
			{
				m_Y = y;
			}
			// System.Console.WriteLine("                          after   x=" + x + " m_X=" + m_X + "  y=" + y + " m_Y=" + m_Y);
			return spans180;
		}

		public bool bottomRight(double x, double y)
		{
			bool spans180 = false;
			// System.Console.WriteLine("GeoCoord::bottomRight():  before  x=" + x + " m_X=" + m_X + "  y=" + y + " m_Y=" + m_Y);
			if(Math.Abs(x - m_X) < 180.0) 
			{ 
				if(x > m_X) 
				{
					m_X = x;
				}
			} 
			else 
			{
				if(m_X > 90.0 && x < -90) 
				{
					spans180 = true;
					x += 360.0;
					if(x > m_X) 
					{
						m_X = x;
						m_X -= 360.0;
					}
				}
			}
			if(y < m_Y) 
			{
				m_Y = y;
			}
			// System.Console.WriteLine("                          after   x=" + x + " m_X=" + m_X + "  y=" + y + " m_Y=" + m_Y);
			return spans180;
		}

		public GeoCoord Clone()
		{
			return new GeoCoord(m_X, m_Y, m_H);
		}

		public string toDbString()	  // make sure GeoCoord(st) matches this
		{
			return "" + m_X + " " + m_Y + " " + m_H;
		}

		public override string ToString()
		{
			return toString0(false, Project.coordStyle);
		}

		public string ToStringWithElev()
		{
			return toString0(true, Project.coordStyle);
		}

		public string toString0(bool doHeight, int coordStyle)
		{
			if(m_X > 180.0 || m_X < -180.0 || m_Y > 90.0 || m_Y < -90.0) 
			{
				return "---";
			}

			double dAbsX = Math.Abs(m_X);
			double dFloorX = Math.Floor(dAbsX);
			double dMinX = (dAbsX - dFloorX) * 60.0;
			long degX = (long)Math.Round(dFloorX);
			long minX = (long)Math.Round(Math.Floor(dMinX));
			long secX = (long)Math.Round(Math.Floor((dMinX - (double)minX) * 60.0));

			double dAbsY = Math.Abs(m_Y);
			double dFloorY = Math.Floor(dAbsY);
			double dMinY = (dAbsY - dFloorY) * 60.0;
			long degY = (long)Math.Round(dFloorY);
			long minY = (long)Math.Round(Math.Floor(dMinY));
			long secY = (long)Math.Round(Math.Floor((dMinY - (double)minY) * 60.0));

			string height = doHeight ? heightToString() : "";

			switch (coordStyle)
			{
				default:
				case 0:
				{
					string mxfiller = minX > 9 ? "°" : "°0";
					string myfiller = minY > 9 ? "°" : "°0";
					string sxfiller = secX > 9 ? "'" : "'0";
					string syfiller = secY > 9 ? "'" : "'0";
					return (m_Y > 0.0 ? "N" : "S") + degY
						+ myfiller + minY + syfiller + secY + "\""
						+ (m_X > 0.0 ? "  E" : "  W") + degX
						+ mxfiller + minX + sxfiller + secX + "\""
						+ height;
				}
				case 1:
				{
					double drMinX = Math.Round(dMinX, 5);
					double drMinY = Math.Round(dMinY, 5);
					string mxfiller = drMinX >= 10.0d ? "°" : "°0";
					string myfiller = drMinY >= 10.0d ? "°" : "°0";
					return (m_Y > 0.0 ? "N" : "S") + degY
						+ myfiller + string.Format("{0:F3}'", drMinY)
						+ (m_X > 0.0 ? "  E" : "  W") + degX
						+ mxfiller + string.Format("{0:F3}'", drMinX)
						+ height;
				}
				case 2:
					return (m_Y > 0.0 ? "N" : "S") + string.Format("{0:F6}", Math.Abs(m_Y))
						+ (m_X > 0.0 ? "  E" : "  W") + string.Format("{0:F6}", Math.Abs(m_X))
						+ height;
				case 3:			// UTM = 11S E433603 N3778359
					// requires LibNet, which would cause circular dependency:
					return Project.mainCommand.toUtmString(m_X, m_Y, m_H) + height;
			}
		}

		// same as toString0() but dir letters after coords
		public string toString00(bool doHeight, int coordStyle)
		{
			if(m_X > 180.0 || m_X < -180.0 || m_Y > 90.0 || m_Y < -90.0) 
			{
				return "---";
			}

			double dAbsX = Math.Abs(m_X);
			double dFloorX = Math.Floor(dAbsX);
			double dMinX = (dAbsX - dFloorX) * 60.0;
			long degX = (long)Math.Round(dFloorX);
			long minX = (long)Math.Round(Math.Floor(dMinX));
			long secX = (long)Math.Round(Math.Floor((dMinX - (double)minX) * 60.0));

			double dAbsY = Math.Abs(m_Y);
			double dFloorY = Math.Floor(dAbsY);
			double dMinY = (dAbsY - dFloorY) * 60.0;
			long degY = (long)Math.Round(dFloorY);
			long minY = (long)Math.Round(Math.Floor(dMinY));
			long secY = (long)Math.Round(Math.Floor((dMinY - (double)minY) * 60.0));

			string height = doHeight ? heightToString() : "";

			switch (coordStyle)
			{
				default:
				case 0:
				{
					string mxfiller = minX > 9 ? "°" : "°0";
					string myfiller = minY > 9 ? "°" : "°0";
					string sxfiller = secX > 9 ? "'" : "'0";
					string syfiller = secY > 9 ? "'" : "'0";
					return ("" + degY
						+ myfiller + minY + syfiller + secY + "\"" + (m_Y > 0.0 ? " N  " : " S  ")
						+ degX
						+ mxfiller + minX + sxfiller + secX + "\"" + (m_X > 0.0 ? " E  " : " W  ") 
						+ height).Trim();
				}
				case 1:
				{
					double drMinX = Math.Round(dMinX, 5);
					double drMinY = Math.Round(dMinY, 5);
					string mxfiller = drMinX >= 10.0d ? "°" : "°0";
					string myfiller = drMinY >= 10.0d ? "°" : "°0";
					return ("" + degY
						+ myfiller + string.Format("{0:F3}'", drMinY) + (m_Y > 0.0 ? " N  " : " S  ")
						+ degX
						+ mxfiller + string.Format("{0:F3}'", drMinX) + (m_X > 0.0 ? " E  " : " W  ")
						+ height).Trim();
				}
				case 2:
					return (string.Format("{0:F6}", Math.Abs(m_Y)) + (m_Y > 0.0 ? " N  " : " S  ")
						+ string.Format("{0:F6}", Math.Abs(m_X)) + (m_X > 0.0 ? " E  " : " W  ")
						+ height).Trim();
				case 3:			// UTM = 11S E433603 N3778359
					// requires LibNet, which would cause circular dependency:
					return Project.mainCommand.toUtmString(m_X, m_Y, m_H) + height;
			}
		}

		public string heightToString()
		{
			string height = "";
			if(m_H > 0.3d) // meters
			{
				Distance d = new Distance(m_H);
				height = " " + d.ToStringCompl() + " high";
			} 
			else if(m_H < -0.3d) // meters
			{
				Distance d = new Distance(-m_H);
				height = " " + d + " deep";
			} 
			return height;
		}

		public static string latToString(double lat, int coordStyle, bool doDegree, bool doDir)
		{
			double dAbsY = Math.Abs(lat);
			double dFloorY = Math.Floor(dAbsY);
			double dMinY = (dAbsY - dFloorY) * 60.0;
			long degY = (long)Math.Round(dFloorY);
			long minY = (long)Math.Round(Math.Floor(dMinY));
			long secY = (long)Math.Round(Math.Floor((dMinY - (double)minY) * 60.0));
			string sDir = doDir ? (lat > 0.0 ? "N" : "S") : "";

			switch (coordStyle)
			{
				default:
				case 0:
				{
					string myfiller = minY > 9 ? "" : "0";
					string syfiller = secY > 9 ? "'" : "'0";
					return sDir + (doDegree ? ("" + degY + "°") : "°")
						+ myfiller + minY + syfiller + secY + "\"";
				}
				case 3:			// UTM = 11S E433603 N3778359 -- never should call here:
				case 1:
				{
					double drMinY = Math.Round(dMinY, 5);
					string myfiller = drMinY >= 10.0d ? "" : "0";
					return sDir + (doDegree ? ("" + degY + "°") : "°") + myfiller + string.Format("{0:F3}'", drMinY);
				}
				case 2:
					return sDir + string.Format("{0:F6}", Math.Abs(lat));
//				case 3:			// UTM = 11S E433603 N3778359 -- never should call here:
//					return "YYYY";
			}
		}

		public static string lngToString(double lng, int coordStyle, bool doDegree, bool doDir)
		{
			double dAbsX = Math.Abs(lng);
			double dFloorX = Math.Floor(dAbsX);
			double dMinX = (dAbsX - dFloorX) * 60.0;
			long degX = (long)Math.Round(dFloorX);
			long minX = (long)Math.Round(Math.Floor(dMinX));
			long secX = (long)Math.Round(Math.Floor((dMinX - (double)minX) * 60.0));
			string sDir = doDir ? (lng > 0.0 ? "E" : "W") : "";

			switch (coordStyle)
			{
				default:
				case 0:
				{
					string mxfiller = minX > 9 ? "" : "0";
					string sxfiller = secX > 9 ? "'" : "'0";
					return sDir + (doDegree ? ("" + degX + "°") : "°")
						+ mxfiller + minX + sxfiller + secX + "\"";
				}
				case 3:			// UTM = 11S E433603 N3778359 -- never should call here:
				case 1:
				{
					double drMinX = Math.Round(dMinX, 5);
					string mxfiller = drMinX >= 10.0d ? "" : "0";
					return sDir + (doDegree ? ("" + degX + "°") : "°") + mxfiller + string.Format("{0:F3}'", drMinX);
				}
				case 2:
					return sDir + string.Format("{0:F6}", Math.Abs(lng));
//				case 3:			// UTM = 11S E433603 N3778359 -- never should call here:
//					return "XXXX";
			}
		}

		public static double stringLatToDouble(string sLat)
		{
			double ret = 0.0d;
			double sign = 1.0d;
			sLat = sLat.Trim();

			if(sLat.StartsWith("-") || sLat.ToLower().StartsWith("s"))
			{
				sign = -1.0d;
				sLat = sLat.Substring(1);
			}

			if(sLat.StartsWith("+") || sLat.ToLower().StartsWith("n"))
			{
				sLat = sLat.Substring(1);
			}

			if(sLat.EndsWith("\"") || sLat.EndsWith("'"))
			{
				sLat = sLat.Substring(0, sLat.Length-1);
			}

			//throw new Exception("invalid format");

			switch (Project.coordStyle)
			{
				default:
				case 0:			// N33,17'12" or N33°17'12"
				{
					int pos1 = sLat.IndexOf("°");
					int pos1a = sLat.IndexOf(",");
					if(pos1 == -1)
					{
						pos1 = pos1a;
					}
					int pos2 = sLat.IndexOf("'");
					if(pos1 == -1 || pos2 == -1)
					{
						throw new Exception("lat - invalid format - needs N33,17'12\" or N33°17'12\"");
					}
					double degrees = Convert.ToDouble(sLat.Substring(0, pos1));
					double mins = Convert.ToDouble(sLat.Substring(pos1+1, pos2-pos1-1));
					double secs = Convert.ToDouble(sLat.Substring(pos2+1));
					ret = degrees + mins / 60.0d + secs / 3600.0d;
				}
					break;
				case 1:			// N33,17.123' or N33°17.123'
				{
					int pos1 = sLat.IndexOf("°");
					int pos1a = sLat.IndexOf(",");
					if(pos1 == -1)
					{
						pos1 = pos1a;
					}
					if(pos1 == -1)
					{
						throw new Exception("lat - invalid format - needs N33,17.123' or N33°17.123'");
					}
					double degrees = Convert.ToDouble(sLat.Substring(0, pos1));
					double mins = Convert.ToDouble(sLat.Substring(pos1+1));
					ret = degrees + mins / 60.0d;
				}
					break;
				case 2:			// N33.123
					ret = Convert.ToDouble(sLat);
					break;
				case 3:			// UTM = 11S E433603 N3778359
				{
				}
					break;
			}

			return ret * sign;
		}

		public static double stringLngToDouble(string sLng)
		{
			double ret = 0.0d;
			double sign = -1.0d;	// assume west
			sLng = sLng.Trim();
			
			if(sLng.StartsWith("-") || sLng.ToLower().StartsWith("w"))
			{
				sign = -1.0d;
				sLng = sLng.Substring(1);
			}

			if(sLng.StartsWith("+") || sLng.ToLower().StartsWith("e"))
			{
				sign = 1.0d;
				sLng = sLng.Substring(1);
			}

			if(sLng.EndsWith("\"") || sLng.EndsWith("'"))
			{
				sLng = sLng.Substring(0, sLng.Length-1);
			}

			switch (Project.coordStyle)
			{
				default:
				case 0:			// W117,17'12" or W117°17'12"
				{
					int pos1 = sLng.IndexOf("°");
					int pos1a = sLng.IndexOf(",");
					if(pos1 == -1)
					{
						pos1 = pos1a;
					}
					int pos2 = sLng.IndexOf("'");
					if(pos1 == -1 || pos2 == -1)
					{
						throw new Exception("lat - invalid format - needs W117,17'12\" or W117°17'12\"");
					}
					double degrees = Convert.ToDouble(sLng.Substring(0, pos1));
					double mins = Convert.ToDouble(sLng.Substring(pos1+1, pos2-pos1-1));
					double secs = Convert.ToDouble(sLng.Substring(pos2+1));
					ret = degrees + mins / 60.0d + secs / 3600.0d;
				}
					break;
				case 1:			// W117,17.123' or W117°17.123'
				{
					int pos1 = sLng.IndexOf("°");
					int pos1a = sLng.IndexOf(",");
					if(pos1 == -1)
					{
						pos1 = pos1a;
					}
					if(pos1 == -1)
					{
						throw new Exception("lng - invalid format - needs W117,17.123' or W117°17.123'");
					}
					double degrees = Convert.ToDouble(sLng.Substring(0, pos1));
					double mins = Convert.ToDouble(sLng.Substring(pos1+1));
					ret = degrees + mins / 60.0d;

				}
					break;
				case 2:			// W117.123
					ret = Convert.ToDouble(sLng);
					break;
				case 3:			// UTM = 11S E433603 N3778359
				{
				}
					break;
			}

			return ret * sign;
		}

		// the following produces tokenizable string suitable for dialog forms.
		// the string looks like the following: "W 114 23 11 N 36 02 45 -2000"
		public string toString2()
		{
			double dAbsX = Math.Abs(m_X);
			double dFloorX = Math.Floor(dAbsX);
			double dMinX = (dAbsX - dFloorX) * 60.0;
			long degX = (long)Math.Round(dFloorX);
			long minX = (long)Math.Round(Math.Floor(dMinX));
			long secX = (long)Math.Round(Math.Floor((dMinX - (double)minX) * 60.0));

			double dAbsY = Math.Abs(m_Y);
			double dFloorY = Math.Floor(dAbsY);
			double dMinY = (dAbsY - dFloorY) * 60.0;
			long degY = (long)Math.Round(dFloorY);
			long minY = (long)Math.Round(Math.Floor(dMinY));
			long secY = (long)Math.Round(Math.Floor((dMinY - (double)minY) * 60.0));

			string mxfiller = minX > 9 ? " " : " 0";
			string myfiller = minY > 9 ? " " : " 0";
			string sxfiller = secX > 9 ? " " : " 0";
			string syfiller = secY > 9 ? " " : " 0";
			return (m_X > 0.0 ? "E " : "W ") + degX
				+ mxfiller + minX + sxfiller + secX +
				(m_Y > 0.0 ? " N " : " S ") + degY
				+ myfiller + minY + syfiller + secY +
				" " + m_H;
		}
	}
}
