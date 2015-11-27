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
using System.Collections.Generic;
using System.Text;
using System.Collections;

using LibSys;

namespace LibFormats
{
	// any NMEA or Magellan protocol packet - this class supports parsing of the incoming (or logged) packets:
	public class NmeaPacket
	{
		protected string m_header = "";
		public string header { get { return m_header; } set { m_header = value; } }

		protected ArrayList m_fields = new ArrayList();
		public ArrayList fields { get { return m_fields; } set { m_fields = value; } }

        public const double METERS_PER_FOOT = 0.3048d;

		public NmeaPacket()	{}

        public NmeaPacket(string header) { m_header = header; }

		// returns "pure body" structure: "$PMGNSMTH,2,3,4" (without checksum)
		public override string ToString()
		{
			string ret = "$" + m_header;
			foreach(string fld in m_fields)
			{
				string tmp = fld.Replace(",", " ");
				tmp = tmp.Replace("*", "");
				tmp = tmp.Replace("$", "");
				tmp = tmp.Replace("\n", " ");
				tmp = tmp.Replace("\r", "");
				ret += ("," + tmp.Trim());
			}
			return ret;
		}

		// given a string, parses it into header and fields, returns true if parse is successful
		public bool basicParse(string str)
		{
			bool ret = false;
			// everything between a "$" and "," or "*" is header:
			if(str.StartsWith("$"))
			{
				int posAst = str.IndexOf("*");
				if(posAst > 1)
				{
					int posFirstComma = str.IndexOf(",");
					if(posFirstComma > 1)
					{
						header = str.Substring(1, posFirstComma - 1);
						string fieldsStr = str.Substring(posFirstComma + 1, posAst - posFirstComma - 1);
						char[] sep = new char[] { ',' };
						string[] fieldArr = fieldsStr.Split(sep);
						fields.Clear();
						foreach(string ss in fieldArr)
						{
							fields.Add(ss);
						}
					}
					else
					{
						header = str.Substring(1, posAst - 1);
					}
					ret = true;
				}
			}
			return ret;
		}

        // given a parsed packet (basicParse() has been called on this NmeaPacket), parses Latitude part of it
		public double parseLat(int offset)
		{
			// $PMGNTRK,3334.530,N,11739.837,W,00000,M,034414.24,A*62
			string latStr = (string)fields[offset];
			double latDeg = Convert.ToDouble(latStr.Substring(0,2));
			double latMin = Convert.ToDouble(latStr.Substring(2));
			double Lat = latDeg + latMin / 60.0d;
			if(((string)fields[offset+1]).Equals("S"))
			{
				Lat = -Lat;
			}
			return Lat;
		}

        // given a parsed packet (basicParse() has been called on this NmeaPacket), parses Longitude part of it
		public double parseLng(int offset)
		{
			// $PMGNTRK,3334.530,N,11739.837,W,00000,M,034414.24,A*62
			string lngStr = (string)fields[offset+2];
			double lngDeg = Convert.ToDouble(lngStr.Substring(0,3));
			double lngMin = Convert.ToDouble(lngStr.Substring(3));
			double Lng = lngDeg + lngMin / 60.0d;
			if(((string)fields[offset+3]).Equals("W"))
			{
				Lng = -Lng;
			}
			return Lng;
		}

        // given a parsed packet (basicParse() has been called on this NmeaPacket), parses Elevation part of it
		public double parseElev(int offset)
		{
			// $PMGNTRK,3334.530,N,11739.837,W,00000,M,034414.24,A*62
			double Elev = Convert.ToDouble((string)fields[offset]);
			if(((string)fields[offset+1]).Equals("F"))
			{
				Elev = Elev * METERS_PER_FOOT;
			}
			return Elev;
		}

        // given a parsed packet (basicParse() has been called on this NmeaPacket), parses Time part of it
		// $PMGNTRK,3334.100,N,11742.518,W,00147,M,033531.02,A,,280403*63	// 03:35:31.02 28 Apr 2003  UTM
		public void parseTime(int offset, out int h, out int m, out int s, out int ms)
		{
			string timeStr = (string)fields[offset];
			string hh = timeStr.Substring(0,2);
			h = Convert.ToInt32(hh);
			string mm = timeStr.Substring(2,2);
			m = Convert.ToInt32(mm);
			string ss = timeStr.Substring(4,2);
			s = Convert.ToInt32(ss);
			string mss = timeStr.Substring(7,2) + "0";
			ms = Convert.ToInt32(mss);
		}

        // given a parsed packet (basicParse() has been called on this NmeaPacket), parses Date part of it
		// $PMGNTRK,3334.100,N,11742.518,W,00147,M,033531.02,A,,280403*63	//  03:35:31.02 28 Apr 2003 UTM
		public void parseDate(int offset, out int y, out int m, out int d)
		{
			string dateStr = (string)fields[offset];
			string dd = dateStr.Substring(0,2);
			d = Convert.ToInt32(dd);
			string mm = dateStr.Substring(2,2);
			m = Convert.ToInt32(mm);
			string yy = "20" + dateStr.Substring(4,2);
			y = Convert.ToInt32(yy);
		}

        // given a parsed packet (basicParse() has been called on this NmeaPacket), parses PVT Time
		public DateTime parseNmeaTime(int offset, DateTime trackDateZulu)
		{
			string t = (string)fields[offset];
            int year = trackDateZulu.Year;
            int month = trackDateZulu.Month;
            int day = trackDateZulu.Day;
			int hh = Convert.ToInt32(t.Substring(0,2));
			int mm = Convert.ToInt32(t.Substring(2,2));
			int ss = Convert.ToInt32(t.Substring(4,2));
			return new DateTime(year, month, day, hh, mm, ss);	// UTC
		}

        // given a parsed packet (basicParse() has been called on this NmeaPacket), parses PVT Time and Date
        public DateTime parseNmeaDateTime(int offsetTime, int offsetDate)
		{
            int h, m, s, ms;
            parseTime(offsetTime, out h, out m, out s, out ms);
            int y, mm, d;
            parseDate(offsetDate, out y, out mm, out d);

            DateTime dt = new DateTime(y, mm, d, h, m, s, ms);		// UTC
            return dt;
		}

        public static bool checksumVerify(string str)
        {
            bool ret = false;
            // the $ and * should NOT be included in computation, see http://joe.mehaffey.com/mag-errata.htm
            try
            {
                int posAst = str.IndexOf("*");
                if (posAst != -1 && posAst <= str.Length - 3)
                {
                    string csStr = str.Substring(posAst + 1, 2);

                    ASCIIEncoding enc = new ASCIIEncoding();
                    uint l = (uint)enc.GetByteCount(str);
                    byte[] s = enc.GetBytes(str);
                    byte[] ast = enc.GetBytes("*");
                    int sum = 0;
                    int len = s.GetLength(0);
                    for (int i = 1; i < len - 3; i++)
                    {
                        if (s[i] == ast[0])
                        {
                            break;
                        }
                        sum ^= s[i];
                    }
                    sum = sum & 0xFF;
                    ret = string.Format("{0:X2}", sum).Equals(csStr);
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        public static string checksum(string str)
        {
            // the $ and * should NOT be included in computation, see http://joe.mehaffey.com/mag-errata.htm
            ASCIIEncoding enc = new ASCIIEncoding();
            uint l = (uint)enc.GetByteCount(str);
            byte[] s = enc.GetBytes(str);
            int sum = 0;
            int len = s.GetLength(0);
            for (int i = 1; i < len; i++)
            {
                sum ^= s[i];
            }
            sum = sum & 0xFF;
            return string.Format("*{0:X2}\r\n", sum);
        }

        public static string checksumReceived(string str)
        {
            // the $ and * should NOT be included in computation, see http://joe.mehaffey.com/mag-errata.htm
            ASCIIEncoding enc = new ASCIIEncoding();
            uint l = (uint)enc.GetByteCount(str);
            byte[] s = enc.GetBytes(str);
            int sum = 0;
            int len = s.GetLength(0);
            for (int i = 1; i < len; i++)
            {
                if (s[i] == (byte)'*')
                {
                    break;
                }
                sum ^= s[i];
            }
            sum = sum & 0xFF;
            return string.Format("{0:X2}", sum);
        }
	}

    /// <summary>
    /// when a packet is received, it can be converted to a class packet using fromString() factory
    /// in this class
    /// </summary>
    public class NmeaPacketReceived : NmeaPacket
    {
        public string m_string = null;		// will be allocated as the packet is received
        public bool isGood = false;			// will be true if checksum is ok and basicParse runs well

        public NmeaPacketReceived()
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
        public NmeaPacket fromString()
        {
            NmeaPacket ret = null;

            if (m_string == null)
            {
                logError("NmeaPacketReceived:fromString() -- m_string==null");
                return ret;
            }

            switch (m_header)
            {
                case "ADVER":
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
                    logError("NmeaPacketReceived:fromString() -- unknown header=" + m_header);
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
