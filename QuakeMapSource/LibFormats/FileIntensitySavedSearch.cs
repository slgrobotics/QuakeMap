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
	/// Summary description for FileIntensitySavedSearch.
	/// </summary>
	public class FileIntensitySavedSearch : BaseFormat
	{
		public FileIntensitySavedSearch()
		{
		}

		public FileIntensitySavedSearch(InsertEarthquake insertEarthquake) : base(insertEarthquake)
		{
		}

		public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: FileIntensitySavedSearch:process() filename=" + filename);

			int eqCount = 0;
			int lineno = 0;

			try
			{
				string line;
				int state = 0;
				string[] infos = null;

				StreamReader stream = new StreamReader(filename);

				while((line = stream.ReadLine()) != null && state != 99)
				{
					lineno++;
					try 
					{
						switch(state) 
						{
							case 0:
								if(line.IndexOf("Earthquake Intensity Database") != -1) 
								{
									state = 1;
									//LibSys.StatusBar.Trace("IP: FileIntensitySavedSearch:process() state 1 filename=" + filename);
								}
								break;

							case 1:  
								if(line.StartsWith("<PRE>")) 
								{
									state = 4;
									//LibSys.StatusBar.Trace("IP: FileIntensitySavedSearch:process() state 4 filename=" + filename);
								}
								break;

							case 4:   // read earthquake lines like:
								/*
								 * from http://www.ngdc.noaa.gov/seg/hazard/int_srch.shtml
								 * 
								 * from 1638 to 1985
								 * 
<P><h2><center>Results of Earthquake Intensity Database Search</center></h2>
...
<pre>
--------------------------------------------------------------------------------------------------------------------------------
Year Mo Da Hr Mn  Sec UTC  U/G  Earthquake    Mag Depth   Epi         City        MMI State  City Name                    Data
                      Conv      Lat    Long       (km)    Dis     Lat      Long        Code                               Source
--------------------------------------------------------------------------------------------------------------------------------
</pre>
<PRE>
1899  9  4  0 22       10   U   60.00 -142.00 8.3   25    221     60.46   -145.91   8    2   CAPE WHITSHEAD                  M
1899  9  4  0 22       10       60.00 -142.00 8.3   25     23     60.07   -142.41  11    2   CAPE YAKATAGA                   H
1899  9  4  0 22       10   U   60.00 -142.00 8.3   25    215     60.55   -145.75   8    2   CORDOVA                         M
1899  9 10 17  4       10       60.00 -140.00 7.8   25    134     60.07   -142.41   7    2   CAPE YAKATAGA                   M
1899  9 10 21 41       10   U   60.00 -140.00 8.6         289     58.45   -135.92   7    2   BARTLETT BAY                    M
1899  9 10 21 41       10   U   60.00 -140.00 8.6         754     66.26   -145.81   6    2   BIRCH CREEK                     M
1899  9 10 21 41       10   U   60.00 -140.00 8.6         131     59.13   -138.41   7    2   DRY BAY                         M
1899  9 10 21 41       10   U   60.00 -140.00 8.6         284     58.32   -136.23   7    2   DUNDAS BAY                      M
1899  9 10 21 41       10   U   60.00 -140.00 8.6         270     59.38   -135.33   7    2   SKAGWAY                         M
1899  9 10 21 41       10   U   60.00 -140.00 8.6         369     61.13   -146.36   7    2   VALDEZ                          M
1899  9 10 21 41       10   U   60.00 -140.00 8.6          46     59.60   -139.77  11    2   YAKUTAT BAY                     H
1900 10  9 12 28       10       60.00 -142.00 8.3   25    652     57.75   -152.50   8    2   KODIAK                          H
1900 10  9 12 28       10       60.00 -142.00 8.3   25    380     59.38   -135.33   7    2   SKAGWAY                         H
1900 10  9 12 28       10       60.00 -142.00 8.3   25    389     60.70   -135.05   6   99   WHITEHORSE                      H
</PRE>
								*/
								if(line.ToLower().IndexOf("</pre>") >= 0) 
								{
									state = 99;	// break from the loop
								}
								else
								{
									infos = new string[9];
									string sYear = "" + line.Substring(0, 4).Trim();
									if(sYear.Length == 0) { sYear = "0"; }
									int year = Convert.ToInt32(sYear);

									string sMonth = "" + line.Substring(5, 2).Trim();
									if(sMonth.Length == 0 || sMonth.Equals("0")) { sMonth = "1"; }
									int month = Convert.ToInt32(sMonth);
									
									string sDay = "" + line.Substring(8, 2).Trim();
									if(sDay.Length == 0 || sDay.Equals("0")) { sDay = "1"; }
									int day = Convert.ToInt32(sDay);
									
									string sHour = "" + line.Substring(11, 2).Trim();
									if(sHour.Length == 0) { sHour = "0"; }
									int hour = Convert.ToInt32(sHour);
									
									string sMinute = "" + line.Substring(14, 2).Trim();
									if(sMinute.Length == 0) { sMinute = "0"; }
									int minute = Convert.ToInt32(sMinute);
									
									int second = 0;
									//LibSys.StatusBar.Trace("line=" + line);
									//LibSys.StatusBar.Trace("y=" + year + " m=" + month + " d=" + day + " h=" + hour + " m=" + minute + " s=" + second);

									infos[0] = "" + (new DateTime(year, month, day, hour, minute, second));

									infos[1] = "" + line.Substring(31, 6).Trim();	// lat
									infos[2] = "" + line.Substring(38, 7).Trim();	// lng
									infos[3] = "" + line.Substring(50, 5).Trim();	// depth
									infos[4] = "" + line.Substring(46, 3).Trim();	// mag
									infos[5] = "*";	// quality
									infos[6] = "NGDC Int: " + line.Substring(93).Trim();	// comment
									infos[7] = "http://www.ngdc.noaa.gov/seg/hazard/int_srch.shtml";
									infos[8] = source;

									m_insertEarthquake(infos);
									eqCount++;
								} 
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("FileIntensitySavedSearch:process():  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("FileIntensitySavedSearch:process() " + e.Message);
				ret = false;
			}
			return ret;
		}
	}
}
