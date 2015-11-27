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
	/// <summary>
	/// Summary description for FileDelormeTxt.
	/// </summary>
	public class FileDelormeTxt : BaseFormat
	{
		public const double METERS_PER_FOOT = 0.3048d;

		public FileDelormeTxt()
		{
		}

		public static string FormatName = "DeLorme .txt";
		public static string FileExtension = ".txt";
		public static string FileExtensions = "*.txt";

		public FileDelormeTxt(InsertWaypoint insertWaypoint) : base(insertWaypoint)
		{
			m_insertWaypoint = insertWaypoint;
		}

		public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: FileDelormeTxt:process() filename=" + filename);

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
							case 0:   // look for "Date, Time"
								// Date, Time ((GMT-05:00) Eastern Time (DST)), Latitude, Longitude, Elevation (ft), Heading, Speed (mi/hr), GPS Status, Log Type
								if(line.StartsWith("Date, Time")) 
								{
									state = 1;
									//LibSys.StatusBar.Trace("IP: FileDelormeTxt:process() state 1 filename=" + filename);
									Project.trackId++;
									createInfo.init("trk");
									createInfo.id = Project.trackId;
									createInfo.source = source;
									createInfo.name = "DeLorme Blue Logger log";

									m_insertWaypoint(createInfo);

									int pos = line.IndexOf("((GMT");
									if(pos >= 0)
									{
										string sZoneShift = line.Substring(pos+5, 3);
										zoneShift = Convert.ToDouble(sZoneShift);
									}
								}
								break;
							case 1:   // read WPT numpoints lines like:
								// 08/01/2004, 11:04:04, 42:50:54.539, -70:52:16.195, -64.269, 44.08, 32.81, 3, 3
								char[] sep = new Char[1] { ',' };
								string[] split = line.Split(sep);

								DateTime dateTime = Convert.ToDateTime(split[0] + " " + split[1]);
								dateTime = dateTime.AddHours(-zoneShift);

								double lat = ParseCoord(split[2]);
								double lng = ParseCoord(split[3]);
								double elev = Convert.ToDouble(split[4]) * METERS_PER_FOOT;

								createInfo.init("trkpt");
								createInfo.id = Project.trackId;					// relate waypoint to track
								createInfo.dateTime = dateTime;
								createInfo.lat = lat;
								createInfo.lng = lng;
								createInfo.elev = elev;
								createInfo.source = source;

								m_insertWaypoint(createInfo);
								wpCount++;
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("FileDelormeTxt:process():  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception eee) 
			{
				LibSys.StatusBar.Error("FileDelormeTxt:process(): " + eee.Message);
				ret = false;
			}
			//LibSys.StatusBar.Trace("OK: FileDelormeTxt:process() filename=" + filename + " lines=" + lineno + " waypoints=" + wpCount);
			return ret;
		}

		/// <summary>
		/// Parses coordinate string in the form "-70:52:16.195" into a double
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private double ParseCoord(string str)
		{
			char[] sep = new Char[1] { ':' };
			string[] split = str.Replace(" ", "").Split(sep);

			double deg = Convert.ToDouble(split[0]);
			double min = Convert.ToDouble(split[1]);
			double sec = Convert.ToDouble(split[2]);

			double ret = Math.Abs(deg) + min / 60 + sec / 3600;
			if(deg < 0)
			{
				ret = -ret;
			}

			return ret;
		}
	}
}
