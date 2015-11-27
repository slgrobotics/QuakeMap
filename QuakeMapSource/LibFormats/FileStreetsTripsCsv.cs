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
using System.IO;
using System.Xml;

using LibSys;

namespace LibFormats
{
/*
Name,Latitude,Longitude,Name 2,URL,Type
Active Pass Lighthouse,48.873472,-123.290139,"","",""
Albert Head Light (removed),48.386444,-123.478167,"","",""
Amphitrite Point Lighthouse,48.921139,-125.541167,"","",""
Ballenas Island Light,49.350556,-124.160222,"","",""
Bare Point Light,48.929722,-123.704722,"","",""
Barrett Rock Light,54.242639,-130.343944,"","",""
Berens Island Light,48.424167,-123.391667,"","",""
Brockton Point Light,49.300889,-123.117000,"","",""
Cape Saint James Light,51.936111,-131.014444,"","",""
Capilano Light,49.320500,-123.147444,"","",""
Discovery Island Lighthouse,48.424528,-123.225750,"","",""
Fiddle Reef Light,48.429333,-123.283917,"","",""
Fisgard Lighthouse,48.430389,-123.447556,"","",""
Gallows Point Lighthouse,49.170056,-123.918000,"","",""
Holland Rock Light,54.172333,-130.361028,"","",""
Lawyer Island Light,54.112778,-130.343472,"","",""
Lucy Island Lighthouse,54.295722,-130.608722,"","",""
Pilot Bay Lighthouse,49.638333,-116.884833,"","",""
Pointer Island Light,52.060667,-127.948306,"","",""
Porlier Pass Lighthouse,49.013111,-123.584556,"","",""
Portlock Point Light,48.828056,-123.350556,"","",""
Prospect Point Lighthouse,49.314000,-123.141417,"","",""
Point Atkinson Lighthouse,49.330389,-123.264694,"","",""
Race Rocks Lighthouse,48.298028,-123.581417,"","",""
Sand Heads Light,49.087611,-123.310000,"","",""
Saturna Island Light,48.783000,-123.045833,"","",""
Sheringham Point Lighthouse,48.376694,-123.921083,"","",""
Sisters Islets Light,49.486750,-124.434778,"","",""
Triangle Island,50.862579,-129.082146,"","",""
*/

	/// <summary>
	/// Summary description for FileStreetsTripsCsv.
	/// </summary>
	public class FileStreetsTripsCsv : BaseFormat
	{
		public static string FormatName = "Streets&Trips .csv";
		public static string FileExtension = ".csv";
		public static string FileExtensions = "*.csv";

		public FileStreetsTripsCsv()
		{
		}

		public FileStreetsTripsCsv(InsertWaypoint insertWaypoint) : base(insertWaypoint)
		{
			m_insertWaypoint = insertWaypoint;
		}

		public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: FileStreetsTripsCsv:process() filename=" + filename);

			int wpCount = 0;
			int lineno = 0;

			try 
			{
				string line;
				int state = 0;
				double zoneShift = 0.0d;
				CreateInfo createInfo = new CreateInfo();	// we will recycle it in place, filling every time.

				StreamReader stream = new StreamReader(filename);

				while((line = stream.ReadLine()) != null)
				{
					lineno++;
					try 
					{
						switch(state) 
						{
							case 0:   // look for "Name,Latitude,Longitude,Name 2,URL,Type"
								// Name,Latitude,Longitude,Name 2,URL,Type
								if(line.StartsWith("Name,Latitude,Longitude")) 
								{
									state = 1;
									//LibSys.StatusBar.Trace("IP: FileStreetsTripsCsv:process() state 1 filename=" + filename);
								}
								break;
							case 1:   // read WPT numpoints lines like:
								// Active Pass Lighthouse,48.873472,-123.290139,"","",""
								char[] sep = new Char[1] { ',' };
								string[] split = line.Split(sep);

								if(split.Length < 3)
								{
									// possible empty line or something. Reset to searcing for header again.
									state = 0;
									break;
								}

								string name = stripQuotes(split[0]);
								double lat = ParseCoord(split[1]);
								double lng = ParseCoord(split[2]);
								string name2 = split.Length > 3 ? stripQuotes(split[3]) : null;
								string wurl =  split.Length > 4 ? stripQuotes(split[4]) : null;
								string type =  split.Length > 5 ? stripQuotes(split[5]) : null;
								double elev =  split.Length > 6 ? ParseCoord(split[6]) : 0.0d;
								string desc =  split.Length > 7 ? stripQuotes(split[7]) : null;
								string stime = split.Length > 8 ? stripQuotes(split[8]) : null;
								string track = split.Length > 9 ? stripQuotes(split[9]) : null;
								string speed = split.Length > 10 ? stripQuotes(split[10]) : null;
								string odometer = split.Length > 11 ? stripQuotes(split[11]) : null;
								bool found =   split.Length > 12 ? ("true".Equals(stripQuotes(split[12]).ToLower()) || "yes".Equals(stripQuotes(split[12]).ToLower())) : false;

								string strLiveObjectType = "wpt";
								switch(type)
								{
									case "geocache":
										strLiveObjectType = type;
										break;
									case "trkpt":
									case "rtept":
										if(desc == null && track != null)
										{
											desc = track;
										}
										if(name == null && name2 == null && stime != null)
										{
											name2 = stime;
										}
										strLiveObjectType = type;
										break;
								}

								createInfo.init(strLiveObjectType);
								createInfo.lat = lat;
								createInfo.lng = lng;
								createInfo.elev = elev;
								if(name2 != null)
								{
									createInfo.urlName = name2;
								}
								if(desc != null)
								{
									createInfo.desc = desc;
								}
								if(wurl != null)
								{
									createInfo.url = wurl;
								}
								if(name != null)
								{
									createInfo.name = name;
								}
								if(type != null)
								{
									createInfo.typeExtra = type;
								}
								if(stime != null && stime.Length >= 4)
								{
									try
									{
										createInfo.dateTime = Convert.ToDateTime(stime);
									}
									catch {}
								}

								createInfo.source = source;

								m_insertWaypoint(createInfo);
								wpCount++;
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("FileStreetsTripsCsv:process():  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception eee) 
			{
				LibSys.StatusBar.Error("FileStreetsTripsCsv:process(): " + eee.Message);
				ret = false;
			}
			//LibSys.StatusBar.Trace("OK: FileStreetsTripsCsv:process() filename=" + filename + " lines=" + lineno + " waypoints=" + wpCount);
			return ret;
		}

		private string stripQuotes(string str)
		{
			str = str.Trim();
			if("\"\"".Equals(str))
			{
				return null;
			}

			if(str.Length > 2 && str.StartsWith("\"") && str.EndsWith("\""))
			{
				str = str.Substring(1, str.Length - 2);
			}
			return str;
		}

		/// <summary>
		/// Parses coordinate string in the form "-70:52:16.195" into a double
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private double ParseCoord(string str)
		{
			double ret = 0.0d;
			if(str.IndexOf(":") != -1)
			{
				char[] sep = new Char[1] { ':' };
				string[] split = str.Replace(" ", "").Split(sep);

				double deg = Convert.ToDouble(split[0]);
				double min = Convert.ToDouble(split[1]);
				double sec = Convert.ToDouble(split[2]);

				ret = Math.Abs(deg) + min / 60 + sec / 3600;
				if(deg < 0)
				{
					ret = -ret;
				}
			}
			else
			{
				ret = Convert.ToDouble(str.Trim());
			}

			return ret;
		}
	}
}
