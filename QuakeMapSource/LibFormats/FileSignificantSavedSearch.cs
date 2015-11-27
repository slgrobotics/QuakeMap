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
using System.Windows.Forms;

using LibSys;

namespace LibFormats
{
	/// <summary>
	/// Summary description for FileSignificantSavedSearch.
	/// </summary>
	public class FileSignificantSavedSearch : BaseFormat
	{
		private static bool warnedBcDates = false;

		public FileSignificantSavedSearch()
		{
		}

		public FileSignificantSavedSearch(InsertEarthquake insertEarthquake) : base(insertEarthquake)
		{
		}

		public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: FileSignificantSavedSearch:process() filename=" + filename);

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
								if(line.IndexOf("Significant Earthquake Database") != -1) 
								{
									state = 1;
									//LibSys.StatusBar.Trace("IP: FileSignificantSavedSearch:process() state 1 filename=" + filename);
								}
								break;

							case 1:  
								if(line.StartsWith("<PRE>")) 
								{
									state = 4;
									//LibSys.StatusBar.Trace("IP: FileSignificantSavedSearch:process() state 4 filename=" + filename);
								}
								break;

							case 4:   // read earthquake lines like:
								/*
								 * from http://www.ngdc.noaa.gov/seg/hazard/sig_srch.shtml
								 * 
								 * 2150B.C. to present
								 * 
<P><h2><center>Results of Significant Earthquake Database Search</center></h2>
...
<pre>
---------------------------------------------------------------------------------------------------------------------------------------------------------------
  Year Mo Da Hr Min   Lat     Long   Depth Mag Int  Number Death Millions Damage Tsunami Geographic Location                                Data Source (Reference)
                                      (km)          Deaths  Des  $ Damage  Des    Assoc.
---------------------------------------------------------------------------------------------------------------------------------------------------------------
</pre>
<PRE>
   648 11 29 0000    33.8    134.2         8.4                                           JAPAN:  NANKAIDO,SHIKOKU,AWAJI,KIIOV               2                     
   745  6  5 0000    35.4    136.8         7.9                                           JAPAN:  MINO                                       2,5                   
   818  8 10 0000    35.1    139.4         7.9                                      1    JAPAN:  SAGAMI                                     2,55                  
*  869  7 13 0000    38.5    143.6         8.6        1000   3              4       1    JAPAN:  SANRIKU                                    56                    
*  869  7 13 0000    38.5    143.8         8.6        1000   3              3            JAPAN:  SANRIKU                                    65                    
*  869  7 13 0000    38.5    145.8         8.6        1000   3              3            JAPAN:  SANRIKU                                    65                    
*  869  7 13 0000    39.0    141.6         8.6                                           JAPAN:  OSJU                                       2                     
   887  8 26 0000    34.6    135.1         8.6                                      1    JAPAN:  MINO                                       2                     
  1096 12 17 0000    34.2    137.3         8.4         400   3                      1    JAPAN:  TOKKAIDO                                   56                    
  1361  8  3 0000    33.4    135.0         8.4         500   3              2       1    JAPAN:  KINAI                                      2,56                  
* 1471              -16.3    -71.0     25  8.0   8                                       PERU                                               218,220               
* 1498  9 20 0000    34.1    138.2         8.6        5000   3              2       1    JAPAN:  NANKAIDO                                   56                    
* 1498  9 20 0000    35.0    138.0         8.6       41000   3                           JAPAN:  TOTOMI,TOKAIKO                             2,51                  
  1513              -17.2    -72.3     30  8.7   8                                       PERU                                               220                   
  1555 11 15 0000   -11.9    -77.6     30  8.4   7                                       PERU                                               220                   
</PRE>
								*/
								if(line.ToLower().IndexOf("</pre>") >= 0) 
								{
									state = 99;	// break from the loop
								}
								else
								{
									infos = new string[9];
									string sYear = "" + line.Substring(1, 5).Trim();
									if(sYear.Length == 0) { sYear = "0"; }
									int year = Convert.ToInt32(sYear);

									string sMonth = "" + line.Substring(7, 2).Trim();
									if(sMonth.Length == 0 || sMonth.Equals("0")) { sMonth = "1"; }
									int month = Convert.ToInt32(sMonth);
									
									string sDay = "" + line.Substring(10, 2).Trim();
									if(sDay.Length == 0 || sDay.Equals("0")) { sDay = "1"; }
									int day = Convert.ToInt32(sDay);
									
									string sHour = "" + line.Substring(13, 2).Trim();
									if(sHour.Length == 0) { sHour = "0"; }
									int hour = Convert.ToInt32(sHour);
									
									string sMinute = "" + line.Substring(15, 2).Trim();
									if(sMinute.Length == 0) { sMinute = "0"; }
									int minute = Convert.ToInt32(sMinute);
									
									int second = 0;
									//LibSys.StatusBar.Trace("line=" + line);
									//LibSys.StatusBar.Trace("y=" + year + " m=" + month + " d=" + day + " h=" + hour + " m=" + minute + " s=" + second);

									string cmnt = line.Substring(89).Trim();
									string comment = "NGDC Sig: ";
									if(year < 0)
									{
										comment += "[" + (-year) + "B.C./" + month + "/" + day + " " + hour + ":" + minute + "] " + cmnt;
									}
									else if(year < 1)
									{
										comment += "[" + year + "/" + month + "/" + day + " " + hour + ":" + minute + "] " + cmnt;
									}
									else
									{
										comment += cmnt;
									}

									if(year < 1) 
									{		// can't process BC dates
										if(!warnedBcDates)
										{
											Project.MessageBox(null, "Warning:\r\n    This system can't process dates prior to year 0001.\r\n    Some earthquake dates will be set to 0001/02/01 UTC.\r\n    Actual event date appears in the comment field"); 
											warnedBcDates = true;
										}
										year = 1;
										month = 2;	// lesser dates cause empty fields in DataGrid
										day = 1;
										//hour = 1;
										//minute = 1;
										//second = 1;
									}

									infos[0] = "" + (new DateTime(year, month, day, hour, minute, second));

									infos[1] = "" + line.Substring(20, 8).Trim();	// lat
									infos[2] = "" + line.Substring(28, 9).Trim();	// lng
									infos[3] = "" + line.Substring(37, 6).Trim();	// depth
									infos[4] = "" + line.Substring(43, 4).Trim();	// mag
									infos[5] = "*";	// quality
									infos[6] = comment;
									infos[7] = "http://www.ngdc.noaa.gov/seg/hazard/sig_srch.shtml";
									infos[8] = source;

									m_insertEarthquake(infos);
									eqCount++;
								} 
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("FileSignificantSavedSearch:process():  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("FileSignificantSavedSearch:process() " + e.Message);
				ret = false;
			}
			return ret;
		}
	}
}
