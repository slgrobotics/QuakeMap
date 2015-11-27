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
	/// Summary description for FilePlainWpt.
	/// </summary>
	public class FilePlainWpt : BaseFormat
	{
		public FilePlainWpt()
		{
		}

		public static string FormatName = "Garmin .wpt";
		public static string FileExtension = ".wpt";
		public static string FileExtensions = "*.wpt";

		public FilePlainWpt(InsertWaypoint insertWaypoint) : base(insertWaypoint)
		{
			m_insertWaypoint = insertWaypoint;
		}

		public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: FilePlainWpt:process() filename=" + filename);

			int wpCount = 0;
			int lineno = 0;

			try 
			{
				string line;
				int state = 0;
				int numpoints = 0;
				CreateInfo createInfo = new CreateInfo();	// we will recycle it in place, filling every time.

				StreamReader stream = new StreamReader(filename);

				while((line = stream.ReadLine()) != null)
				{
					lineno++;
					try 
					{
						switch(state) 
						{
							case 0:   // look for [WAYPOINTS]
								if(line.StartsWith("[WAYPOINTS]")) 
								{
									state = 2;
									//LibSys.StatusBar.Trace("IP: FilePlainWpt:process() state 2 filename=" + filename);
								}
								if(line.StartsWith("[ROUTES]")) 
								{
									state = 3;
									//LibSys.StatusBar.Trace("IP: FilePlainWpt:process() state 3 filename=" + filename);
									break;
								}
								break;
							case 2:   // look for "NUMPOINTS=" for wpts
							case 3:   // look for "NUMPOINTS=" for routes
								if(line.StartsWith("NUMPOINTS=")) 
								{
									state *= 2;
									string sNumpoints =  line.Substring("NUMPOINTS=".Length);
									numpoints = Convert.ToInt32(sNumpoints);
									if(numpoints <= 0)
									{
										state = 0;
									}
									//LibSys.StatusBar.Trace("IP: FilePlainWpt:process() state " + state + " numpoints=" + numpoints + " filename=" + filename);
								}
								break;
							case 4:   // read WPT numpoints lines like:
								// WPT1=1,5052.677,N,00002.217,E,0000164,M,ME164-,Cliffe Hill ,a
								if(line.StartsWith("WPT")) 
								{
									char[] sep = new Char[1] { ',' };
									string[] split = line.Split(sep);

									double lat = Convert.ToDouble(split[1]) / 100.0d;
									double lng = Convert.ToDouble(split[3]) / 100.0d;

									createInfo.init("wpt");
									createInfo.setLat("" + lat + split[2].ToLower());
									createInfo.setLng("" + lng + split[4].ToLower());
									createInfo.setElev(split[5]);
									createInfo.typeExtra = "waypoint";
									createInfo.source = source;
									createInfo.name = split[8];

									m_insertWaypoint(createInfo);
									wpCount++;
									numpoints--;
									if(numpoints == 0)
									{
										state = 0;	// look for command again
									}
								} 
								break;
							case 6:   // read ROUTE numpoints lines like:
								numpoints--;
								if(numpoints == 0)
								{
									state = 0;	// look for command again
								}
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("FilePlainWpt:process():  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception eee) 
			{
				LibSys.StatusBar.Error("FilePlainWpt:process(): " + eee.Message);
				ret = false;
			}
			//LibSys.StatusBar.Trace("OK: FilePlainWpt:process() filename=" + filename + " lines=" + lineno + " waypoints=" + wpCount);
			return ret;
		}

	}
}
