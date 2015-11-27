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
using System.Threading;

using LibSys;


namespace LibFormats
{
	/// <summary>
	/// FileGpx is a base for FileEasyGps and FileGpxZip.
	/// </summary>
	public abstract class FileGpx : BaseFormat
	{
		public FileGpx()
		{
		}

		public FileGpx(InsertWaypoint insertWaypoint) : base(insertWaypoint)
		{
		}

		// xmlDoc must be Load'ed; can throw exceptions
		protected bool processGpx(string url, XmlDocument xmlDoc, string source)
		{
			bool ret = true;

			XmlNodeList waypointNodes = xmlDoc.DocumentElement.ChildNodes; //.SelectNodes("/gpx/wpt"); - this don't work
#if DEBUG
			LibSys.StatusBar.Trace("IP: FileGpx:process() first level nodeCount=" + waypointNodes.Count);
#endif


			/*
				<gpx
				version="1.0"
				creator="EasyGPS 1.1.9 - http://www.topografix.com"
				xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
				xmlns="http://www.topografix.com/GPX/1/0"
				xmlns:topografix="http://www.topografix.com/GPX/Private/TopoGrafix/0/1"
				xsi:schemaLocation="http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd http://www.topografix.com/GPX/Private/TopoGrafix/0/1 http://www.topografix.com/GPX/Private/TopoGrafix/0/1/topografix.xsd">
				<time>2002-10-04T18:49:55Z</time>
				<bounds minlat="25.061783" minlon="-123.003111" maxlat="50.982883" maxlon="121.640267"/>
				<wpt lat="33.575106480" lon="-117.735883651">
					<ele>159.703613</ele>
					<time>2002-10-04T18:47:47Z</time>
					<name><![CDATA[001]]></name>
					<cmt><![CDATA[29-AUG-02 11:21:24PM]]></cmt>
					<desc><![CDATA[001]]></desc>
					<sym>Dot</sym>
					<type><![CDATA[Dot]]></type>
				</wpt>
				<rte>
					<name><![CDATA[HOME TO CNTRYC]]></name>
					<number>1</number>
					<topografix:color>ff0000</topografix:color>
					<rtept lat="33.574991229" lon="-117.736144077">
						<time>2002-10-11T05:34:36Z</time>
						<name><![CDATA[HOME]]></name>
						<cmt><![CDATA[AURORA]]></cmt>
						<desc><![CDATA[HOME]]></desc>
						<sym>House</sym>
						<type><![CDATA[Residence]]></type>
					</rtept>
				</rte>
				<trk>
					<name><![CDATA[ACTIVE LOG]]></name>
					<number>1</number>
					<topografix:color>ff0000</topografix:color>
					<trkseg>
						<trkpt lat="33.570749483" lon="-117.723665938">
							<ele>106.363037</ele>
							<time>2002-10-11T04:32:08Z</time>
							<sym>Waypoint</sym>
						</trkpt>
						<trkpt lat="33.571032289" lon="-117.722491633">
							<ele>99.153076</ele>
							<time>2002-10-11T04:32:18Z</time>
							<sym>Waypoint</sym>
						</trkpt>
					</trkseg>
				</trk>
				</gpx>
				*/
			
			CreateInfo createInfo = new CreateInfo();	// we will recycle it in place, filling every time.

			// we want to traverse XmlDocument fast, as tile load operations can be numerous
			// and come in pack. So we avoid using XPath and rely mostly on "foreach child":
			foreach(XmlNode nnode in waypointNodes) 
			{
				try 
				{
					switch(nnode.Name)
					{
						case "author":
							//LibSys.StatusBar.Trace("FileGpx:process() node=" + nnode.Name);
							break;
						case "desc":
							//LibSys.StatusBar.Trace("FileGpx:process() node=" + nnode.Name);
							break;
						case "bounds":
							//LibSys.StatusBar.Trace("FileGpx:process() node=" + nnode.Name);
							break;
						case "wpt":
							//LibSys.StatusBar.Trace("FileGpx:process() node=" + nnode.Name);
							createInfo.init("wpt");
							createInfo.setLat(nnode.Attributes["lat"].InnerText);
							createInfo.setLng(nnode.Attributes["lon"].InnerText);
							createInfo.typeExtra = "unknown";	// type: ppl, school, park, locale, airport, reservoir, dam,
																//       civil, cemetery, valley, building
							createInfo.source = source;
							foreach(XmlNode node in nnode.ChildNodes)
							{
								//LibSys.StatusBar.Trace("    child node=" + node.Name);
								switch(node.Name)
								{
									case "time":
										createInfo.setDateTime(node.InnerText);
										break;
									case "ele":
										createInfo.setElev(node.InnerText);
										break;
									case "name":
										createInfo.name = node.InnerText.Trim();	// number like 001 for route-related wpts or GCSDFX for geocaches
										break;
									case "desc":
										createInfo.desc = node.InnerText.Trim();
										break;
									case "groundspeak:cache":
										createInfo.node1 = node;		// will be parsed in Waypoint() constructor
										break;
									case "urlname":		// may overwrite name in Pocket Queries
										createInfo.urlName = node.InnerText.Trim();
										break;
									case "url":
										createInfo.url = node.InnerText.Trim();
										break;
									case "cmt":
										createInfo.comment = node.InnerText.Trim();
										// may contain time, so try to extract it if possible:
										try 
										{
											createInfo.setDateTime(node.InnerText);
										} 
										catch {}
										break;
									case "sym":
										createInfo.sym = node.InnerText.Trim();
										break;
									case "type":	// like "user waypoint" or "geocache"
										createInfo.typeExtra = node.InnerText.Trim();
										break;
								}
							} 
							m_insertWaypoint(createInfo);
							break;
						case "rte":
						{
							string routeName = "";
							string routeColor = "";
							bool routeCreated = false;
							foreach(XmlNode nnnode in nnode.ChildNodes)
							{
								switch(nnnode.Name)
								{
									case "name":	// route name
										routeName = nnnode.InnerText.Trim();
										break;
									case "number":	// route number
										break;
									case "topografix:color":	// like 00ffee
										routeColor = nnnode.InnerText.Trim();
										break;
									case "rtept":
										/*
											<rtept lat="38.518697986" lon="-122.978969274">
												<ele>4.211426</ele>
												<time>2002-10-04T18:48:23Z</time>
												<name><![CDATA[006]]></name>
												<cmt><![CDATA[28-SEP-02 1:56:26PM]]></cmt>
												<desc><![CDATA[006]]></desc>
												<sym>Dot</sym>
												<type><![CDATA[Dot]]></type>
											</rtept>
										*/
										//LibSys.StatusBar.Trace("FileGpx:process() node=" + nnode.Name);
										
										if(!routeCreated)
										{
											Project.trackId++;

											createInfo.init("rte");
											createInfo.name = routeName;
											createInfo.id = Project.trackId;
											createInfo.par1 = nnnode.InnerText.Trim();	// number for route
											createInfo.source = source;
											createInfo.par1 = routeColor;

											m_insertWaypoint(createInfo);	// actually inserts a route

											routeCreated = true;
										}
										createInfo.init("rtept");
										createInfo.setLat(nnnode.Attributes["lat"].InnerText);
										createInfo.setLng(nnnode.Attributes["lon"].InnerText);
										createInfo.id = Project.trackId;	// relate waypoint to track
										createInfo.source = source;

										foreach(XmlNode node in nnnode.ChildNodes)
										{
											//LibSys.StatusBar.Trace("    child node=" + node.Name);
											switch(node.Name)
											{
												case "time":
													createInfo.setDateTime(node.InnerText);
													break;
												case "ele":
													createInfo.setElev(node.InnerText);
													break;
												case "name":
													createInfo.name = node.InnerText.Trim();	// number like 001 for route-related wpts or GCSDFX for geocaches
													break;
												case "desc":
													createInfo.desc = node.InnerText.Trim();
													break;
												case "urlname":
													createInfo.urlName = node.InnerText.Trim();
													break;
												case "url":
													createInfo.url = node.InnerText.Trim();
													break;
												case "cmt":
													createInfo.comment = node.InnerText.Trim();
													break;
												case "sym":
													createInfo.sym = node.InnerText.Trim();
													if("Waypoint".Equals(createInfo.sym))
													{
														createInfo.sym = null;
													}
													break;
												case "type":	// like "user waypoint" or "geocache"
													createInfo.typeExtra = node.InnerText.Trim();
													break;
											}
										} 

										m_insertWaypoint(createInfo);
										break;
								}
							}
						}
							break;
						case "trk":
						{
							string trackName = "";
							string trackNumber = "";
							string trackColor = "";
							foreach(XmlNode nnnode in nnode.ChildNodes)
							{
								switch(nnnode.Name)
								{
									case "name":	// track name
										trackName = nnnode.InnerText.Trim();
										break;
									case "number":	// track number
										trackNumber = nnnode.InnerText.Trim();
										break;
									case "topografix:color":	// like 00ffee
										trackColor = nnnode.InnerText.Trim();
										break;
									case "trkseg":
										Project.trackId++;

										createInfo.init("trk");
										createInfo.name = trackName;
										createInfo.id = Project.trackId;
										createInfo.source = source;
										createInfo.par1 = trackColor;

										m_insertWaypoint(createInfo);	// actually inserts a track

										foreach(XmlNode nnnnode in nnnode.ChildNodes)
										{
											switch(nnnnode.Name)
											{
												case "trkpt":	// track point
													/*
														<trkpt lat="33.570749483" lon="-117.723665938">
															<ele>106.363037</ele>
															<time>2002-10-11T04:32:08Z</time>
															<sym>Waypoint</sym>
														</trkpt>
													*/
													//LibSys.StatusBar.Trace("FileGpx:process() node=" + nnode.Name);

													createInfo.init("trkpt");
													createInfo.setLat(nnnnode.Attributes["lat"].InnerText);
													createInfo.setLng(nnnnode.Attributes["lon"].InnerText);
													createInfo.id = Project.trackId;	// relate waypoint to track
													createInfo.source = source;

													foreach(XmlNode node in nnnnode.ChildNodes)
													{
														//LibSys.StatusBar.Trace("    child node=" + node.Name);
														switch(node.Name)
														{
															case "time":
																createInfo.setDateTime(node.InnerText);
																break;
															case "ele":
																createInfo.setElev(node.InnerText);
																break;
															case "name":
																createInfo.name = node.InnerText.Trim();
																break;
															case "desc":
																createInfo.desc = node.InnerText.Trim();
																break;
															case "urlname":
																createInfo.urlName = node.InnerText.Trim();
																break;
															case "url":
																createInfo.url = node.InnerText.Trim();
																break;
															case "cmt":
																createInfo.comment = node.InnerText.Trim();
																break;
															case "sym":
																createInfo.sym = node.InnerText.Trim();
																if("Waypoint".Equals(createInfo.sym))
																{
																	createInfo.sym = null;
																}
																break;
															case "type":	// like "user waypoint" or "geocache"
																createInfo.typeExtra = node.InnerText.Trim();
																break;
														}
													} 

													m_insertWaypoint(createInfo);
													break;
											}
										}
										break;
								}
							}
						}
							break;
					}
				}
				catch (Exception ee) 
				{
					// bad node - not a big deal...
					LibSys.StatusBar.Error("FileGpx process node=" + nnode.Name + " " + ee); //.Message);
					//LibSys.StatusBar.WriteLine("Culture: " + Thread.CurrentThread.CurrentCulture + " (" + Thread.CurrentThread.CurrentCulture.DisplayName + ")  UseUserOverride=" + Thread.CurrentThread.CurrentCulture.UseUserOverride);
				}
			}
			return ret;
		}

	}
}
