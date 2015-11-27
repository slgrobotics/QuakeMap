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
	/// Summary description for FileCnssSavedSearch.
	/// </summary>
	public class FileCnssSavedSearch : BaseFormat
	{
		public FileCnssSavedSearch()
		{
		}

		public FileCnssSavedSearch(InsertEarthquake insertEarthquake) : base(insertEarthquake)
		{
		}

		public override bool process(string url, string filename, string source)
		{
			LibSys.StatusBar.Trace("IP: FileCnssSavedSearch:process() filename=" + filename);

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
								if(line.IndexOf("PLEASE BE PATIENT") != -1) 
								{
									state = 1;
									//LibSys.StatusBar.Trace("IP: FileCnssSavedSearch:process() state 1 filename=" + filename);
								}
								break;

							case 1:   // look for starting "--------------------------"
								if(line.StartsWith("--------------------------")) 
								{
									state = 4;
									//LibSys.StatusBar.Trace("IP: FileCnssSavedSearch:process() state 4 filename=" + filename);
								}
								break;

							case 4:   // read earthquake lines like:
								/*
								 *  from http://quake.geo.berkeley.edu/anss/catalog-search.html
								 * 
								 * 1898 to present
								 * 
<center><h1>PLEASE BE PATIENT</h2></center>
...
<PRE>
Date       Time             Lat       Lon  Depth   Mag Magt  Nst Gap  Clo  RMS  SRC   Event ID
----------------------------------------------------------------------------------------------
2002/01/01 10:39:06.82 -55.2140 -129.0000  10.00  6.00   Mw   78          1.07  NEI 200201012017
2002/01/01 11:29:22.73   6.3030  125.6500 138.10  6.30   Mw  236          0.90  NEI 200201012018
2002/01/02 17:22:48.76 -17.6000  167.8560  21.00  7.30   Mw  427          0.90  NEI 200201022041
2002/01/03 07:05:27.67  36.0880   70.6870 129.30  6.20   Mw  431          0.87  NEI 200201032025
2002/01/03 10:17:36.30 -17.6640  168.0040  10.00  6.70   Mw  386          1.14  NEI 200201032042
2002/01/10 11:14:56.93  -3.2120  142.4270  11.00  6.70   Mw  333          1.18  NEI 200201102039
2002/01/13 14:10:56.52  -5.6510  151.0740  43.60  6.30   Mw  441          1.06  NEI 200201132060
2003/02/15 11:02:02.63  12.1840  123.9680  33.00  6.30   Mw   70          1.19  NEI 200302151018
2003/02/19 03:32:36.23  53.8730 -164.6490  19.00  6.60   Mw  193          1.15  NEI 200302190005
</PRE>
								*/
								if(line.ToLower().IndexOf("</pre>") >= 0) 
								{
									state = 99;	// break from the loop
								}
								else
								{
									infos = new string[9];
									infos[0] = "" + line.Substring(0, 22).Trim();

									infos[1] = "" + line.Substring(23, 8).Trim();	// lat
									infos[2] = "" + line.Substring(32, 9).Trim();	// lng
									infos[3] = "" + line.Substring(42, 6).Trim();	// depth
									infos[4] = "" + line.Substring(50, 4).Trim();	// mag
									infos[5] = "" + line.Substring(57, 2).Trim();	// quality
									infos[6] = "CNSS event " + line.Substring(80).Trim();	// comment
									infos[7] = "http://quake.geo.berkeley.edu/anss";
									infos[8] = source;

									m_insertEarthquake(infos);
									eqCount++;
								} 
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("FileCnssSavedSearch:process():  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("FileCnssSavedSearch:process() " + e.Message);
			}
			return eqCount > 0;
		}
	}
}
