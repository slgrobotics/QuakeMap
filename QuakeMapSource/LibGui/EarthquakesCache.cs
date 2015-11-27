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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using LibSys;
using LibGeo;
using LibNet;
using LibFormats;

namespace LibGui
{
	public class UrlListStruct
	{
		public string url;
		public string source;
		public FormatProcessor processor;

		public UrlListStruct(string u, string s, FormatProcessor p)
		{
			url = u;
			source = s;
			processor = p;
		}
	}

	public class RetStr
	{
		public string str;		// return by reference
	}

	/// <summary>
	/// Summary description for EarthquakesCache.
	/// </summary>
	public class EarthquakesCache
	{
		private static ArrayList m_earthquakesAll = new ArrayList();
		public  static ArrayList EarthquakesAll { get { return m_earthquakesAll; } }

		private static ArrayList m_earthquakesDisplayed = new ArrayList();
		public  static ArrayList EarthquakesDisplayed { get { return m_earthquakesDisplayed; } }

		protected static PictureManager m_pictureManager = null;
		public static PictureManager PictureManager { get { return m_pictureManager; } set { m_pictureManager = value; } }

		protected static CameraManager m_cameraManager = null;
		public static CameraManager CameraManager { get { return m_cameraManager; } set { m_cameraManager = value; } }

		public static event DynamicObjectCreateHandler DynamicObjectCreateCallback;
		public static event DynamicObjectDeleteHandler DynamicObjectDeleteCallback;
		public static event DynamicObjectMoveHandler   DynamicObjectMoveCallback;
		
		public static void shutdown()
		{
		}

		public static Earthquake getEarthquakeById(long id)
		{
			foreach(Earthquake eq in m_earthquakesAll)
			{
				if(eq.Id == id)
				{
					return eq;
				}
			}
			return null;
		}

		// returns zulu min time of all earthquakes within view
		public static DateTime getMinTimeDisplayed()
		{
			bool haveTerraTiles = (Project.terraserverAvailable || Project.terraserverDisconnected)
				&& (m_cameraManager.terraTopLeft.Lng < m_cameraManager.terraBottomRight.Lng)
				&& (m_cameraManager.terraTopLeft.Lat > m_cameraManager.terraBottomRight.Lat);

			// for PDA we need to cover all area covered by tiles, which is wider than visible window:
			double lngCLeft = haveTerraTiles ? m_cameraManager.terraTopLeft.Lng : m_cameraManager.CoverageTopLeft.Lng;
			double lngCRight = haveTerraTiles ? m_cameraManager.terraBottomRight.Lng : m_cameraManager.CoverageBottomRight.Lng;
			double latCTop = haveTerraTiles ? m_cameraManager.terraTopLeft.Lat : m_cameraManager.CoverageTopLeft.Lat;
			double latCBottom = haveTerraTiles ? m_cameraManager.terraBottomRight.Lat : m_cameraManager.CoverageBottomRight.Lat;

			DateTime ret = DateTime.MaxValue;

			foreach(Earthquake eq in m_earthquakesAll)
			{
				double lng = eq.Location.Lng;
				double lat = eq.Location.Lat;

				if (lng > lngCRight || lng < lngCLeft || lat > latCTop || lat < latCBottom)
				{
					continue;
				}

				DateTime dt = eq.DateTime;

				if (dt.CompareTo(ret) < 0)
				{
					ret = dt;
				}
			}
			return (ret.Equals(DateTime.MaxValue) ? DateTime.Now : ret);
		}

		// returns zulu min time of all earthquakes
		public static DateTime getMinTime()
		{
			DateTime ret = DateTime.MaxValue;

			foreach(Earthquake eq in m_earthquakesAll)
			{
				DateTime dt = eq.DateTime;

				if (dt.CompareTo(ret) < 0)
				{
					ret = dt;
				}
			}
			return (ret.Equals(DateTime.MaxValue) ? DateTime.MinValue : ret);
		}

		// returns zulu max time of all earthquakes
		public static DateTime getMaxTime()
		{
			DateTime ret = DateTime.MinValue;

			foreach(Earthquake eq in m_earthquakesAll)
			{
				DateTime dt = eq.DateTime;

				if (dt.CompareTo(ret) > 0)
				{
					ret = dt;
				}
			}
			return (ret.Equals(DateTime.MinValue) ? DateTime.MaxValue : ret);
		}


		public static bool goFetch = true;

		//public static void Run(object state, DateTime requestEnqueueTime)
		public static void Run(object state)
		{
			while(!Project.goingDown)
			{
				Thread.Sleep(3000);
				if(Project.drawableReady && Project.cameraElev > 0.0d)
				{
					//LibSys.StatusBar.Trace("IP: EarthquakesCache:Run()");

					if(goFetch)
					{
						FetchEarthquakes();
						goFetch = false;
					}

				}
			}
		}

		public static void init()
		{
			/*
			Earthquake eq;

			// this is to test that labels are properly laid out in all positions in Earthquake.cs:
			for (int i=0; i < 12 ;i++) 
			{
				eq = new Earthquake(new GeoCoord(-130.0d, 42.0d, -330000.0d), 6.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
				m_earthquakesAll.Add(eq);
			}
			*/

			/*
			eq = new Earthquake(new GeoCoord(-149.9d, -37.0d, -330000.0d), 6.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			eq = new Earthquake(new GeoCoord(179.9d, -15.0d), 7.9d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			eq = new Earthquake(new GeoCoord(179.98d, -16.75d), 5.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			eq = new Earthquake(new GeoCoord(179.99d, -16.75d), 4.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			eq = new Earthquake(new GeoCoord(96.0d, 17.0d), 4.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			eq = new Earthquake(new GeoCoord(96.1d, 17.0d), 5.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			eq = new Earthquake(new GeoCoord(96.05d, 16.95d), 6.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			eq = new Earthquake(new GeoCoord(-179.99d, -16.75d), 6.3d, DateTime.Now, "bad", "simulated", "auto", "http://www.quakemap.com/sergei/index.html");
			m_earthquakesAll.Add(eq);
			*/
		}

		private static int m_threadCount = 0;
		public static bool isFetching { get { return m_threadCount > 0; } }

		private static void FetchEarthquakes()
		{
			if(!Project.drawEarthquakes)
			{
				return;
			}
			ArrayList UrlList = new ArrayList();
/*

http://earthquake.usgs.gov/eqcenter/recenteqsww/catalogs/eqs7day-M1.txt
http://earthquake.usgs.gov/eqcenter/recenteqsww/catalogs/eqs1day-M1.txt
http://earthquake.usgs.gov/eqcenter/recenteqsww/catalogs/eqs1hour-M1.txt
 */
			// See http://earthquake.usgs.gov/eqcenter/recenteqsww/Quakes/quakes_all.php for data feeds
			UrlList.Add(new UrlListStruct("http://earthquake.usgs.gov/eqcenter/catalogs/eqs1hour-M1.txt",
											"neic-lasthour-csv", new FormatProcessor(processBulletinCsv)));	// current

			UrlList.Add(new UrlListStruct("http://earthquake.usgs.gov/eqcenter/catalogs/eqs1day-M1.txt",
				"neic-lastday-csv", new FormatProcessor(processBulletinCsv)));	// current

			UrlList.Add(new UrlListStruct("http://earthquake.usgs.gov/eqcenter/catalogs/eqs7day-M1.txt",
				"neic-sevendays-csv", new FormatProcessor(processBulletinCsv)));	// current

//			UrlList.Add(new UrlListStruct("http://earthquake.usgs.gov/recenteqsww/Quakes/quakes_all.html",
//											"neic-bulletin", new FormatProcessor(processBulletin)));	// current

			UrlList.Add(new UrlListStruct("http://neic.usgs.gov/neis/qed/",
											"neic-qed", new FormatProcessor(processQed)));				// last 30 days

//			UrlList.Add(new UrlListStruct("http://www.data.scec.org/recenteqs/Quakes/quakes0.html",
//											"scec-quakes0", new FormatProcessor(processQuakes0)));		// Southern CA
//
//			UrlList.Add(new UrlListStruct("http://tux.wr.usgs.gov/Quakes/quakes0.html",
//											"hawaii-quakes0", new FormatProcessor(processQuakes0)));	// Hawaii
//
//			UrlList.Add(new UrlListStruct("http://www.ess.washington.edu/recenteqs/Quakes/quakes0.htm",
//											"wa-quakes0", new FormatProcessor(processQuakes0)));		// NortWest US

			// the 7-days list seems to be obsolete, contains June 2004 data:
			//			UrlList.Add(new UrlListStruct("http://neic.usgs.gov/neis/qed/last_seven_days.html",
			//											"neic-last_seven_days", new FormatProcessor(process7Days))); // last 7 days

			/*
			// this just hangs waiting. doesn't look like finger is supoported.
			UrlList.Add(new UrlListStruct("finger://gldfs.cr.usgs.gov/quake",
											"gldfs", new FormatProcessor(processQuakes0)));		// Gldfs
			*/
			
			m_threadCount = 0;

			foreach(UrlListStruct uls in UrlList)
			{
				string filename = Project.GetMiscPath(uls.source + ".html");

				if(Project.doFetchEqHtml)
				{
					DownloadThread dt = new DownloadThread();
					dt.DownloadUrl = uls.url;
					dt.tile = uls;
					dt.baseName = uls.url;
					dt.fileName = filename;
					dt.CompleteCallback += new DownloadCompleteHandler( eqDownloadCompleteCallback );
					dt.ProgressCallback += new DownloadProgressHandler( eqDownloadProgressCallback );
					dt.addMonitoredMethod = new AddMonitoredMethod(ProgressMonitor.addMonitored);

					//add dt worker method to the thread pool / queue a task 
					m_threadCount++;
					//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), uls.url); 
					ThreadPool2.QueueUserWorkItem(new WaitCallback (dt.Download), uls.url); 

					LibSys.StatusBar.Trace("IP: - loading remote from " + uls.url);
				}
				else
				{
					uls.processor(uls.url, filename, uls.source);
				}
			}

			if(m_threadCount <= 0 && m_pictureManager != null)	// too quick if fetching
			{
				RefreshEarthquakesDisplayed();
				m_pictureManager.Refresh();
			}
		}

		private static void eqDownloadProgressCallback ( DownloadInfo info )
		{
			//LibSys.StatusBar.Trace("IP: EarthquakesCache:eqDownloadProgressCallback() - " + info.bytesProcessed + " of " + info.dataLength);
			if(info.monitored != null)
			{
				if(info.dataLength > 0)
				{
					info.monitored.Progress = info.bytesProcessed * 100 / info.dataLength;
					ProgressMonitor.WorkValues();
				}
				else
				{
					info.monitored.Progress = 20;
				}
			}
		}

		private static void eqDownloadCompleteCallback ( object ouls, DownloadInfo info, string eqFileName, byte[] dataDownloaded )
		{
			UrlListStruct uls = (UrlListStruct)ouls;

			LibSys.StatusBar.Trace("IP: - " + (dataDownloaded == null? "null" : "" + dataDownloaded.Length) + " bytes loaded from " + uls.url);

			if(dataDownloaded == null || Project.is404(dataDownloaded)) 
			{
				string comment = dataDownloaded == null ? "no data" : "404";
				// proven empty
				LibSys.StatusBar.Error("failed to download from " + uls.url);
				ProgressMonitor.markComplete(info.monitored, false, comment);

				if(Project.eqUseOldData)
				{
					try		// who knows if the file is there in the first place...
					{
						uls.processor(uls.url, eqFileName, uls.source);
					} 
					catch {}
				}
			}
			else
			{
				FileStream fs = null;
				try 
				{
					string comment = "" + dataDownloaded.Length + " bytes";
					fs = new FileStream(eqFileName, FileMode.Create);
					fs.Write(dataDownloaded, 0, dataDownloaded.Length);
					fs.Close();
					fs = null;
					LibSys.StatusBar.Trace("IP: - file " + eqFileName + " created");

					uls.processor(uls.url, eqFileName, uls.source);

					ProgressMonitor.markComplete(info.monitored, true, comment);
				} 
				catch (Exception e) 
				{
					LibSys.StatusBar.Error("file " + eqFileName + " " + e.Message);
					ProgressMonitor.markComplete(info.monitored, false, e.Message);
				}
				finally
				{
					if(fs != null)
					{
						fs.Close();
					}
				}
			}

			m_threadCount--;
	
			//if(m_threadCount <= 0 && m_pictureManager != null)	// last Complete
			{
				RefreshEarthquakesDisplayed();
				m_pictureManager.Refresh();
			}
		}

// obsolete, as the page doesn't exist any more
//		private static bool processBulletin(string url, string filename, string source)
//		{
//			LibSys.StatusBar.Trace("IP: process bulletin filename=" + filename);
//
//			int eqCount = 0;
//			int lineno = 0;
//
//			StreamReader stream = null;
//			try 
//			{
//				string line;
//				int state = 0;
//				string[] infos = null;
//				RetStr retStr = new RetStr();
//
//				stream = new StreamReader(filename);
//
//				while((line = stream.ReadLine()) != null) 
//				{
//					lineno++;
//					line = line.Replace("<STRONG>","");
//					line = line.Replace("</STRONG>","");
//					line = line.Replace("<FONT COLOR=\"#CC0000\">","");
//					line = line.Replace("</FONT>","");
//					string lineL = line.ToLower();
//					try 
//					{
//						switch(state) 
//						{
//							case 0:   // look for header of the info table
//								if(line.StartsWith("<PRE>")) 
//								{
//									state = 1;
//									//LibSys.StatusBar.Trace("IP: EarthquakesCache:processBulletin() state 1 filename=" + filename);
//									retStr.str = null;
//									infos = new string[9];
//									infos[8] = source;
//									//LibSys.StatusBar.Trace("IP: EarthquakesCache:processBulletin() state 2 filename=" + filename);
//								}
//								break;
//							case 1:   // read lines with "<A..."
//								if(line.StartsWith("</PRE>")) 
//								{
//									state = 99;
//									//LibSys.StatusBar.Trace("IP: EarthquakesCache:processBulletin() state 99 filename=" + filename);
//									break;
//								} 
//								else if(line.Length > 10)		// ignore lonely <P>
//								{
//									retStr.str = null;
//									string info = stripTags(line, retStr);
//									if(info.ToLower().StartsWith("map")) 
//									{
//										infos = new string[9];
//
//										char[] sep = new Char[1];
//										sep[0] = ' ';
//										string[] split = info.Substring(4).Split(sep);
//										string comment = "";
//										for(int jj=6; jj < split.Length ;jj++)
//										{
//											comment += split[jj] + " ";
//										}
//										comment = comment.Trim();
//
//										infos[0] = split[1] + " " + split[2];	// Time UTC
//										infos[1] = split[3];	// lat
//										infos[2] = split[4];	// lng
//										infos[3] = split[5];	// depth
//										infos[4] = split[0];	// magn
//										infos[5] = "";			// quality
//										infos[6] = comment;
//										infos[7] = ResolveUrl(url, retStr);
//										infos[8] = source;
//
//										//LibSys.StatusBar.Trace("url: '" + url + "'   retStr.str='" + retStr.str + "'");
//										insertEarthquake(infos);
//										eqCount++;
//									}
//								}
//								break;
//							case 99:      // nothing to parse left
//								break;
//						}
//					}
//					catch (Exception ee) 
//					{
//						LibSys.StatusBar.Error("process bulletin:  file=" + filename + " line=" + lineno + " " + ee);
//					}
//				}
//			}
//			catch (Exception eee) 
//			{
//				LibSys.StatusBar.Error("process bulletin: " + eee.Message);
//			}
//			finally
//			{
//				if(stream != null)
//				{
//					stream.Close();
//				}
//			}
//			LibSys.StatusBar.Trace("OK: filename=" + filename + " lines=" + lineno + " earthquakes=" + eqCount);
//			// warn of possible format problem:
//			if(lineno > 10 && eqCount == 0)
//			{
//				string str = "URL " + url + " was parsed to 0 earthquakes";
//				MessageBox.Show(str, possibleFormatError, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//			}
//			return eqCount > 0;
//		}

		private static string[] parseCsv(string info)
		{
			//	old:	ci,14216812,1,"March 13, 2006 03:05:33 GMT",36.0260,-117.7626,1.3,2.80,11
			
            // Src,Eqid,Version,Datetime,Lat,Lon,Magnitude,Depth,NST,Region
            // nc,40221550,1,"Monday, August  4, 2008 03:21:48 UTC",36.5812,-121.1205,1.9,6.50,12,"Central California"
            
            string [] ret = new string[12];  // 10 will do, but in case they add more fields, as they did before

			bool inQuotes = false;
			int item = 0;
			int lastCommaPos = -1;
			int pos = 0;

			for(; pos < info.Length ;pos++)
			{
				string curchar = info.Substring(pos, 1);
				if(!inQuotes && curchar.Equals(","))
				{
					ret[item++] = info.Substring(lastCommaPos + 1, pos - lastCommaPos - 1).Replace("\"", "");
					lastCommaPos = pos;
				}
				else if(curchar.Equals("\""))
				{
					inQuotes = !inQuotes;
				}
			}
			ret[item++] = info.Substring(lastCommaPos + 1).Replace("\"", "");

			return ret;
		}

		// -------------------------------------------------------------------------------------------------------
		// the following method processes CSV feeds from http://earthquake.usgs.gov/eqcenter/recenteqsww/catalogs/
		// the data looks like this:
		//		Src,Eqid,Version,Datetime,Lat,Lon,Magnitude,Depth,NST
		//		ci,14216812,1,"March 13, 2006 03:05:33 GMT",36.0260,-117.7626,1.3,2.80,11
		//		nc,51169078,1,"March 13, 2006 03:04:10 GMT",38.4913,-122.2845,1.3,4.80, 9
		//		ak,00058271,5,"March 13, 2006 02:23:40 GMT",60.0904,-148.3955,3.3,5.00,19

		//		Src,Eqid,Version,Datetime,Lat,Lon,Magnitude,Depth,NST
		//		nc,51194079,1,"Sunday, January  6, 2008 06:08:28 UTC",38.8253,-122.7992,1.7,2.70,23
		//		nc,51194078,1,"Sunday, January  6, 2008 05:39:34 UTC",38.7757,-122.7150,1.1,2.30,14
		//		ci,10298345,1,"Sunday, January  6, 2008 05:23:28 UTC",34.5043,-116.5191,1.4,4.00,27

		private static bool processBulletinCsv(string url, string filename, string source)
		{
			LibSys.StatusBar.Trace("IP: process bulletin filename=" + filename);

			int eqCount = 0;
			int lineno = 0;

			StreamReader stream = null;
			try 
			{
				string line;
				string[] infos = null;
				RetStr retStr = new RetStr();

				stream = new StreamReader(filename);

				while((line = stream.ReadLine()) != null) 
				{
					lineno++;
					string lineL = line.ToLower();
					try 
					{
						if(lineL.StartsWith("src,")) 
						{
							retStr.str = null;
							infos = new string[9];
							infos[8] = source;
							continue;
						}
						else if(line.Length > 10)		// ignore empty lines etc.
						{
							retStr.str = null;
							infos = new string[9];

							string[] split = parseCsv(line);
							string network = split[0].ToUpper();
							string equrl;
							switch(network)
							{
								case "US":
									equrl = "http://earthquake.usgs.gov/eqcenter/recenteqsww/Quakes/" + split[0] + split[1] + ".php";
									break;
								default:
									equrl = "http://earthquake.usgs.gov/eqcenter/recenteqsus/Quakes/" + split[0] + split[1] + ".php";
									break;
							}
							string comment = split[6] + " from " + network;

							infos[0] = split[3].Replace(" GMT", "").Replace(" UTC", "").Replace("Monday, ", "").Replace("Tuesday, ", "").Replace("Wednesday, ", "").Replace("Thursday, ", "").Replace("Friday, ", "").Replace("Saturday, ", "").Replace("Sunday, ", "");	// Time UTC
							infos[1] = split[4];	// lat
							infos[2] = split[5];	// lng
							infos[3] = split[7];	// depth
							infos[4] = split[6];	// magn
							infos[5] = "";			// quality
							infos[6] = comment;
							infos[7] = equrl;
							infos[8] = source + "-" + network;

							//LibSys.StatusBar.Trace("url: '" + url + "'   retStr.str='" + retStr.str + "'");
							insertEarthquake(infos);
							eqCount++;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("process bulletin-csv:  file=" + filename + " line=" + lineno + " " + ee);
					}
				}
			}
			catch (Exception eee) 
			{
				LibSys.StatusBar.Error("process bulletin-csv: " + eee.Message);
			}
			finally
			{
				if(stream != null)
				{
					stream.Close();
				}
			}
			LibSys.StatusBar.Trace("OK: filename=" + filename + " lines=" + lineno + " earthquakes=" + eqCount);
			// warn of possible format problem:
			if(lineno > 10 && eqCount == 0)
			{
				string str = "URL " + url + " was parsed to 0 earthquakes";
				MessageBox.Show(str, possibleFormatError, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return eqCount > 0;
		}

		private static string possibleFormatError = "Possible Format Error - Update the program or report to QuakeMap.com";

		private static bool process7Days(string url, string filename, string source)
		{
			LibSys.StatusBar.Trace("IP: process 7Days filename=" + filename);

			int eqCount = 0;
			int lineno = 0;

			StreamReader stream = null;

			try 
			{
				string line;
				int state = 0;
				int tdcount = 0;
				string[] infos = null;
				RetStr retStr = new RetStr();

				stream = new StreamReader(filename);

				while((line = stream.ReadLine()) != null) 
				{
					lineno++;
					string lineL = line.ToLower();
					try 
					{
						switch(state) 
						{
							case 0:   // look for header of the info table
								if(lineL.StartsWith("<td headers=\"t7\">&nbsp")) 
								{
									state = 1;
									//LibSys.StatusBar.Trace("IP: EarthquakesCache:process7Days() state 1 filename=" + filename);
								}
								break;
							case 1:   // look for "<tr"
								if(lineL.StartsWith("</table")) 
								{
									state = 99;
									//LibSys.StatusBar.Trace("IP: EarthquakesCache:process7Days() state 99 filename=" + filename);
									break;
								}
								if(lineL.StartsWith("<tr")) 
								{
									state = 2;
									tdcount = 0;
									retStr.str = null;
									infos = new string[9];
									infos[8] = source;
									//LibSys.StatusBar.Trace("IP: EarthquakesCache:process7Days() state 2 filename=" + filename);
								}
								break;
							case 2:   // read lines with "<td"
								if(lineL.StartsWith("</tr")) 
								{
									state = 1;
								} 
								else 
								{
									if(lineL.StartsWith("<td")) 
									{
										string info = stripTags(line, retStr);
										if(tdcount < infos.Length) 
										{
											infos[tdcount] = info;
											//LibSys.StatusBar.Trace("IP: infos[" + tdcount + "]='" + info + "'");
											if(tdcount == 5) 
											{
												infos[5] = "";
												infos[6] = info;

												//LibSys.StatusBar.Trace("url: '" + url + "'   retStr.str='" + retStr.str + "'");
												infos[7] = ResolveUrl(url, retStr);
												insertEarthquake(infos);
												eqCount++;
											}
										} 
										else 
										{
											// wrong number of columns in the table - look for another row
											state = 1;
										}
										tdcount++;
									}
								}
								break;
							case 99:      // nothing to parse left
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("process 7Days:  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception eee) 
			{
				LibSys.StatusBar.Error("process 7Days: " + eee.Message);
			}
			finally
			{
				if(stream != null)
				{
					stream.Close();
				}
			}
			LibSys.StatusBar.Trace("OK: process 7Days filename=" + filename + " lines=" + lineno + " earthquakes=" + eqCount);
			// warn of possible format problem:
			if(lineno > 10 && eqCount == 0)
			{
				string str = "URL " + url + " was parsed to 0 earthquakes";
				MessageBox.Show(str, possibleFormatError, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return eqCount > 0;
		}

		private static bool processQed(string url, string filename, string source)
		{
			LibSys.StatusBar.Trace("IP: process qed filename=" + filename);

			int eqCount = 0;
			int lineno = 0;

			StreamReader stream = null;

			try 
			{
				string line;
				int state = 0;
				int tdcount = 0;
				string[] infos = null;
				RetStr retStr = new RetStr();

				stream = new StreamReader(filename);

				while((line = stream.ReadLine()) != null) 
				{
					lineno++;
					string lineL = line.ToLower();
					try 
					{
						switch(state) 
						{
							case 0:   // look for header of the info table
								if(lineL.StartsWith("<td headers=\"t7\">&nbsp")) 
								{
									state = 1;
									//LibSys.StatusBar.Trace("IP: EarthquakesCache:processQed() state 1 filename=" + filename);
								}
								break;
							case 1:   // look for "<tr"
								if(lineL.StartsWith("</table")) 
								{
									state = 99;
									//LibSys.StatusBar.Trace("IP: EarthquakesCache:processQed() state 99 filename=" + filename);
									break;
								}
								if(lineL.StartsWith("<tr")) 
								{
									state = 2;
									tdcount = 0;
									retStr.str = null;
									infos = new string[9];
									infos[8] = source;
									//LibSys.StatusBar.Trace("IP: EarthquakesCache:processQed() state 2 filename=" + filename);
								}
								break;
							case 2:   // read lines with "<td"
								if(lineL.StartsWith("</tr")) 
								{
									state = 1;
								} 
								else 
								{
									if(lineL.StartsWith("<td")) 
									{
										string info = stripTags(line, retStr);
										if(tdcount < infos.Length) 
										{
											infos[tdcount] = info;
											//LibSys.StatusBar.Trace("IP: infos[" + tdcount + "]='" + info + "'");
											if(tdcount == 5) 
											{
												infos[5] = "";
												infos[6] = info;

												//LibSys.StatusBar.Trace("url: '" + url + "'   retStr.str='" + retStr.str + "'");
												infos[7] = ResolveUrl(url, retStr);
												insertEarthquake(infos);
												eqCount++;
											}
										} 
										else 
										{
											// wrong number of columns in the table - look for another row
											state = 1;
										}
										tdcount++;
									}
								}
								break;
							case 99:      // nothing to parse left
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("process qed:  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception eee) 
			{
				LibSys.StatusBar.Error("process qed: " + eee.Message);
			}
			finally
			{
				if(stream != null)
				{
					stream.Close();
				}
			}
			LibSys.StatusBar.Trace("OK: process qed filename=" + filename + " lines=" + lineno + " earthquakes=" + eqCount);
			// warn of possible format problem:
			if(lineno > 10 && eqCount == 0)
			{
				string str = "URL " + url + " was parsed to 0 earthquakes";
				MessageBox.Show(str, possibleFormatError, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return eqCount > 0;
		}

		private static bool processQuakes0(string url, string filename, string source)
		{
			LibSys.StatusBar.Trace("IP: process quakes0 filename=" + filename);

			int eqCount = 0;
			int lineno = 0;
			string timezone = "PDT";	// Earthquake() knows to parse "PDT", "HST". Assume PDT fo now.

			StreamReader stream = null;

			try 
			{
				string line;
				int state = 0;
				string[] infos = null;
				RetStr retStr = new RetStr();

				stream = new StreamReader(filename);

				while((line = stream.ReadLine()) != null) 
				{
					lineno++;
					string lineL = line.ToLower();
					try 
					{
						switch(state) 
						{
							case 0:   // look for the info section after the second <PRE>, and also try to determine timezone:
								if(line.IndexOf("Hawaiian Standard Time") != -1)
								{
									timezone = "HST";
								}
								if(line.StartsWith("<PRE>")) 
								{
									state = 1;
								}
								break;
							case 1:   // read lines with "<td"
								if(line.StartsWith("</PRE>")) 
								{
									state = 99;
									break;
								} 
								else 
								{
									//LibSys.StatusBar.Trace("IP: EarthquakesCache:processQuakes0() state 1 filename=" + filename);
									retStr.str = null;
									string info = stripTags(line, retStr);
									if(info.ToLower().StartsWith("map")) 
									{
										infos = new string[9];

										char[] sep = new Char[1];
										sep[0] = ' ';
										string[] split = info.Substring(4).Split(sep);
										string comment = "";
										for(int jj=6; jj < split.Length ;jj++)
										{
											comment += split[jj] + " ";
										}
										comment = comment.Trim();

										infos[0] = split[1] + " " + split[2] + " " + timezone;	// Earthquake() knows to parse some timezones
										infos[1] = split[3];	// lat
										infos[2] = split[4];	// lng
										infos[3] = split[5];	// depth
										infos[4] = split[0];	// magn
										infos[5] = "";			// quality
										infos[6] = comment;
										infos[7] = ResolveUrl(url, retStr);
										infos[8] = source;

										//LibSys.StatusBar.Trace("url: '" + url + "'   retStr.str='" + retStr.str + "'");
										insertEarthquake(infos);
										eqCount++;
									}
								}
								break;
							case 99:      // nothing to parse left
								break;
						}
					}
					catch (Exception ee) 
					{
						LibSys.StatusBar.Error("process quakes0:  file=" + filename + " line=" + lineno + " " + ee.Message);
					}
				}
			}
			catch (Exception eee) 
			{
				LibSys.StatusBar.Error("process quakes0: " + eee.Message);
			}
			finally
			{
				if(stream != null)
				{
					stream.Close();
				}
			}
			LibSys.StatusBar.Trace("OK: process quakes0 filename=" + filename + " lines=" + lineno + " earthquakes=" + eqCount);
			// warn of possible format problem:
			if(lineno > 10 && eqCount == 0)
			{
				string str = "URL " + url + " was parsed to 0 earthquakes";
				MessageBox.Show(str, possibleFormatError, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return eqCount > 0;
		}

		public static void insertEarthquake(string[] infos)
		{
			/* DEBUG
			for(int jj=0; jj < infos.Length ;jj++) 
			{
				LibSys.StatusBar.Trace("infos: " + jj + " '" + infos[jj] + "'");
			}
			*/
			Earthquake eq = new Earthquake(infos);

			/* DEBUG
			if (eq.Location.X > -127 || eq.Location.X < -130 || eq.Location.Y > 45 || eq.Location.Y < 43)
			{
				return;
			}
			*/

			// Filter out future quakes:
			DateTime dtInsane = DateTime.Now.AddDays(5);
			if(eq.DateTime.Ticks > dtInsane.Ticks)
			{
				return;
			}

			//double lng = eq.Location.Lng;
			double lat = eq.Location.Lat;

			lock(m_earthquakesAll)
			{

				int ii;
				for (ii=0; ii < m_earthquakesAll.Count ;ii++)
				{
					Earthquake other = (Earthquake)m_earthquakesAll[ii];
					double lngo = other.Location.Lng;
					double lato = other.Location.Lat;

					//if(lngo == lng) 
					if(lato == lat) 
					{
						if(eq.sameAs(other))
						{
							//LibSys.StatusBar.Trace(" ---- ignored duplicate earthquake (orig from " + other.Source + ") : " + eq);
							return;		// ignore if it ain't new
						}
					} 
						//else if(lngo < lng)	// sort east to west
						//else if(lato > lat)	// sort south to north
					else if(lato < lat)	// sort north to south
					{
						break;
					}
				}
				eq.Id = Project.eqId++;		// new earthquake, assign an Id
				if(ii == m_earthquakesAll.Count)
				{
					m_earthquakesAll.Add(eq);
				}
				else
				{
					m_earthquakesAll.Insert(ii, eq);
				}

				// invalidating every object's drawing area is VERY expensive, we do PictureManager.Refresh() instead
				// at the end of interpreting all files. So the next line is commented out.
				//DynamicObjectCreateCallback(eq);
			}
		}

		private static int InsertEarthquakeToDisplay(Earthquake eq)
		{
			if(m_cameraManager == null)
			{
				return 0;
			}

			bool haveTerraTiles = (Project.terraserverAvailable || Project.terraserverDisconnected)
									&& (m_cameraManager.terraTopLeft.Lng < m_cameraManager.terraBottomRight.Lng)
									&& (m_cameraManager.terraTopLeft.Lat > m_cameraManager.terraBottomRight.Lat);

			// for PDA we need to cover all area covered by tiles, which is wider than visible window:
			double lngCLeft = haveTerraTiles ? m_cameraManager.terraTopLeft.Lng : m_cameraManager.CoverageTopLeft.Lng;
			double lngCRight = haveTerraTiles ? m_cameraManager.terraBottomRight.Lng : m_cameraManager.CoverageBottomRight.Lng;
			double latCTop = haveTerraTiles ? m_cameraManager.terraTopLeft.Lat : m_cameraManager.CoverageTopLeft.Lat;
			double latCBottom = haveTerraTiles ? m_cameraManager.terraBottomRight.Lat : m_cameraManager.CoverageBottomRight.Lat;

			double lng = eq.Location.Lng;
			double lat = eq.Location.Lat;

			// here is a catch: eq coordinates are <180 >-180 (normaliized) while coverage coordinates are not.
			bool span180W = lngCLeft < -180.0d;
			bool span180E = lngCRight > 180.0d;
			// de-normalize lng for comparisons, if span180 takes place:
			if(span180W && lng > 0)
			{
				lng -= 360.0d;
			}
			if(span180E && lng < 0)
			{
				lng += 360.0d;
			}

			if (lng > lngCRight || lng < lngCLeft || lat > latCTop || lat < latCBottom)
			{
				return 0;
			}

			int ii;
			for (ii=0; ii < m_earthquakesDisplayed.Count ;ii++)
			{
				Earthquake other = (Earthquake)m_earthquakesDisplayed[ii];
				double lato = other.Location.Lat;

				if(lato == lat) 
				{
					if(eq.sameAs(other))
					{
						//LibSys.StatusBar.Trace(" ---- ignored duplicate earthquake (orig from " + other.Source + ") : " + eq);
						return 0;		// ignore if it ain't new
					}
				} 
				else if(lato < lat)	// sort north to south
				{
					break;
				}
			}
			if(ii == m_earthquakesDisplayed.Count)
			{
				m_earthquakesDisplayed.Add(eq);
			}
			else
			{
				m_earthquakesDisplayed.Insert(ii, eq);
			}
			return 1;
		}

		public static void RefreshEarthquakesDisplayed()
		{
			lock (m_earthquakesDisplayed)
			{
				m_earthquakesDisplayed.Clear();

				if(m_cameraManager != null)
				{
					foreach(Earthquake eq in m_earthquakesAll)
					{
						try
						{
							InsertEarthquakeToDisplay(eq);
						} 
						catch (Exception e)
						{
						}
					}
				}
			}
#if DEBUG
			LibSys.StatusBar.Trace("RefreshEarthquakesDisplayed() total=" + m_earthquakesDisplayed.Count);
#endif
		}

		public static void RemoveEarthquakesBySource(string source)
		{
			lock(m_earthquakesAll)
			{
			again:
				for(int i=0; i < m_earthquakesAll.Count ;i++)
				{
					Earthquake eq = (Earthquake)m_earthquakesAll[i];
					try
					{
						if(eq.Source.Equals(source))
						{
							m_earthquakesAll.RemoveAt(i);
							goto again;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
			RefreshEarthquakesDisplayed();
		}

		private static string ResolveUrl(string sUrl, RetStr retStr)
		{
			string ret = null;
			if(retStr.str != null) 
			{
				int slashPos = sUrl.LastIndexOf("/");
				if(retStr.str.StartsWith("http://"))
				{
					ret = retStr.str;
				}
				else if(retStr.str.StartsWith("/"))
				{
					int dslashPos = sUrl.IndexOf("//", 0);
					slashPos = sUrl.IndexOf("/", dslashPos + 2);
					ret = sUrl.Substring(0, slashPos) + retStr.str;
				} 
				else 
				{
					ret = sUrl.Substring(0, slashPos + 1) + retStr.str;
				}
			}
			return ret;
		}


		// here is an example of line we need to clean up:
		// <td  headers="t7" align="left" valign="top"><font face="helvetica"><a href="020205051243.html" onMouseOver="window.status='&#060;REN> NEVADA 02/02/05 05:12:43'">&#060;REN> NEVADA</a></font></td>
		// retStr returns link, if located in the line

		private static string stripTags(string line, RetStr retStr)
		{
			string ret = "";
			string tmp = line;
			bool inTag = false;
			bool inQuotes = false;
			bool isHref = false;
			int tag0 = 0;
			int tag1 = 0;
			int quote0 = 0;
			int quote1 = 0;

			int maxcnt = 400;
			int cur = 0;
			try 
			{
				//LibSys.StatusBar.Trace("line=" + line);
				while(cur < tmp.Length && maxcnt-- > 0) 
				{
					char curCh = tmp.ToCharArray(cur, 1)[0];
					//LibSys.StatusBar.Trace("char=" + curCh + " at cur=" + cur);
					if(inTag && inQuotes) 
					{ // look for ending quote
						switch(curCh) 
						{
							case '"':
								inQuotes = false;
								quote1 = cur;
								// LibSys.StatusBar.Trace("deleting quotes from=" + quote0 + " to=" + quote1);
								if(isHref) 
								{
									retStr.str = tmp.Substring(quote0+1, quote1-quote0-1);
									isHref = false;
									//LibSys.StatusBar.Trace("linkUrl='" + retStr.str + "'");
								}
								tmp = sbDelete(tmp, quote0, quote1+1);
								// LibSys.StatusBar.Trace("deleted ok - line=" + tmp);
								cur = quote0 - 1;
								break;
						}
					} 
					else if(inTag) 
					{ // look for quotes or end of tag
						switch(curCh) 
						{
							case '"':
								inQuotes = true;
								quote0 = cur;
								break;
							case '>':
								inTag = false;
								tag1 = cur;
								// LibSys.StatusBar.Trace("deleting tag from=" + quote0 + " to=" + quote1);
								tmp = sbDelete(tmp, tag0, tag1+1);
								// LibSys.StatusBar.Trace("deleted ok - line=" + tmp);
								cur = tag0 - 1;
								break;
						}
					} 
					else 
					{        // look for tag
						switch(curCh) 
						{
							case '<':
								inTag = true;
								tag0 = cur;
								if(tmp.Substring(cur).ToLower().StartsWith("<a href=")) 
								{
									isHref = true;
								} 
								else 
								{
									isHref = false;
								}
								break;
						}
					}
					cur++;
				}
                
				tmp = tmp.Replace("&quot;", "\"");
				tmp = tmp.Replace("&nbsp;", " ");
				tmp = tmp.Replace("&#060;", "<");
				tmp = tmp.Replace("&amp;", "&");
				tmp = tmp.Replace("&gt;", ">");
				tmp = tmp.Replace("&#062;", ">");
				tmp = tmp.Replace("&lt;", "<");
				tmp = tmp.Replace("  ", " ");
				tmp = tmp.Replace("  ", " ");
				tmp = tmp.Replace("  ", " ");
				tmp = tmp.Replace("  ", " ");
				ret = tmp.Trim();
			} 
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("stripTags()  cur=" + cur + " e=" + e.Message + " line=" + line);
			}
			return ret;
		}
        
		private static string sbDelete(string src, int from, int to)
		{
			string s0 = src.Substring(0, from);
			string s1 = src.Substring(to);

			string ret = s0 + s1;
			return ret;
		}

        /*
		private static string sbReplace(string src, string what, string toWhat)
		{
			string ret = src;
			int pos;
			while((pos=ret.ToString().IndexOf(what)) != -1) 
			{
				ret = sbDelete(ret, pos, pos + what.Length);
				ret = ret.Substring(0, pos) + toWhat + ret.Substring(pos);
			}
			return ret;
		}
		*/
	}
}
