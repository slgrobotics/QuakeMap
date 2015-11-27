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
using System.Threading;

using LibSys;
using LibGeo;

namespace LibGps
{
	public class MagellanProductType
	{
		public string product_ID = "unknown";
		/*
		Product Name		ID
		----------------- -------
		SporTrak Pro		036
		*/

		public string software_version = "unknown";
		public string product_description = "unknown";

		public override string ToString()
		{
			string ret = "Device: " + product_ID + "\nFirmware: " + software_version + "\n";
			
			ret += "Description: " + product_description + "\n";

			return ret;
		}
	}

	public class MagellanProtoAppLayer : IDisposable
	{
		protected MagellanProtoLinkLayer m_linkLayer;

		public MagellanProtoAppLayer()
		{
			m_linkLayer = new MagellanProtoLinkLayer();
		}

		public void Dispose() 
		{
			m_linkLayer.Dispose();
		}

		public MagellanProductType IdentifyDeviceType()
		{
			MagellanProductType ret = null;		// returned if no positive identification

			try
			{
				m_linkLayer.SetHandshake(Project.gpsMagellanHandshake);
				//m_linkLayer.SetHandshake(true);		// safer with handshake on
				if(Project.gpsMagellanHandshake)
				{
					m_linkLayer.SetTransferMode(false);
				}
				m_linkLayer.SendPacket(new MPacketCmd("NMEAOFF"));
				m_linkLayer.SendPacket(new MPacketCmd("STOP"));
				MPacketReceived received = m_linkLayer.Transact(new MPacketCmd("VERSION"));
				if(received.header.Equals("PMGNVER"))
				{
					MagellanProductType _ret = new MagellanProductType();
					_ret.product_ID = "" + received.fields[0];
					_ret.software_version = "" + received.fields[1];
					_ret.product_description = "" + received.fields[2];
					ret = (_ret.product_ID.Length > 0 && !"unknown".Equals(_ret.product_ID)) ? _ret : null;
				}
			}
			catch { }

			return ret;
		}


		public void GetTracks(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			string source = "MGN-GPS-" + DateTime.Now;

			string trackName = "";
			bool trackCreated = false;
			int total = 0;
			int toReceive = -1;		// we don't know how many trackpoints are there in GPS
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;
			DateTime dtPrev = DateTime.MinValue;

			m_linkLayer.SetHandshake(Project.gpsMagellanHandshake);
			if(Project.gpsMagellanHandshake)
			{
				m_linkLayer.SetTransferMode(true);
			}
			MPacketCmd trackCmd = new MPacketCmd("TRACK");
			trackCmd.fields.Add("2");			// undocumented, causes date to be included in PMGNTRK responses 
			m_linkLayer.SendPacket(trackCmd);
			while(!finished)
			{
				MPacketReceived received;
				try
				{
					received = m_linkLayer.ReceivePacket();
				}
				catch(CommPortException e)
				{
					m_linkLayer.logError("exception waiting for GPS response: " + e.Message);
					break;
				}
				//m_linkLayer.log("" + total + " ===  " + received);
				if(!received.isGood)
				{
					m_linkLayer.logError("GetTracks: bad packet received, count=" + total + " content=" + received.m_string);
					continue;
				}
				//m_linkLayer.log("GetTracks: good packet received, count=" + total + "   " + received);
				if(received.header.Equals("PMGNTRK"))
				{
					// $PMGNTRK,3334.100,N,11742.518,W,00147,M,033531.02,A,,280403*63	// 03:35:31.02 28 Apr 2003  UTM
					try
					{
						MPacket trkpt  = received.fromString();

						if(!trackCreated)
						{
							Project.trackId++;

							trackName = "MGN-TRACK-LOG" + Project.trackId;

							// we need to allocate every infos here, as they will be buffered in the reader thread:
							CreateInfo createInfo = new CreateInfo();

							createInfo.init("trk");
							createInfo.id = Project.trackId;	// track id
							createInfo.source = source;
							createInfo.name = trackName;

							insertWaypoint(createInfo);			// actually inserts a track

							trackCreated = true;
						}

						// $PMGNTRK,3334.100,N,11742.518,W,00147,M,033531.02,A,,280403*63	// 03:35:31.02 28 Apr 2003  UTM
						double Lat = trkpt.parseLat(0);
						double Lng = trkpt.parseLng(0);
						double Elev = trkpt.parseElev(4);

						int h, m, s, ms;
						trkpt.parseTime(6, out h, out m, out s, out ms);
						int y, mm, d;
						trkpt.parseDate(9, out y, out mm, out d);

						DateTime dt = new DateTime(y, mm, d, h, m, s, ms);		// UTM

						/*  this is from the days when I didn't know how to get date from Magellan:
						// make sure that as points come, DateTime only grows:
						DateTime dt = new DateTime(m_year, m_month, m_day, h, m, s, ms);
						while(dt.CompareTo(dtPrev) < 0)
						{
							m_day++;
							if(m_day > 28) { m_month++; m_day = 1; }
							dt = new DateTime(m_year, m_month, m_day, h, m, s, ms);
						}
						dtPrev = dt;
						*/

						//m_linkLayer.log("time=" + dt);

					{
						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("trkpt");
						createInfo.id = Project.trackId;			// relate waypoint to track
						createInfo.dateTime = dt;
						createInfo.lat = Lat;
						createInfo.lng = Lng;
						createInfo.elev = Elev;
						createInfo.source = source;

						insertWaypoint(createInfo);
					}
						total++;
						if((total % 3) == 0)
						{
							progressCallback("IP: uploading track: " + trackName + " to PC\n\nleg ", total, toReceive);
						}
						errors = maxErrors;
					} 
					catch(Exception ee)
					{
						m_linkLayer.logError("GetTracks: " + ee.Message);
					}
				}
				else if(received.header.Equals("PMGNCMD"))
				{
					m_linkLayer.log("GetTracks: end of tracks received, count=" + total + "   " + received);
					finished = true;
					break;
				}
				else
				{
					m_linkLayer.log("GetTracks: good other packet received, count=" + total + "   content=" + received);
				}
			} // end while()

			if(Project.gpsMagellanHandshake)
			{
				m_linkLayer.SetTransferMode(false);
			}
			progressCallback("loaded OK", total, total);
			m_linkLayer.log("GetTracks: loaded OK count=" + total + "  trackpoints");
		}


		private ArrayList wptBuffer = new ArrayList();

		private void bufferWaypoint(CreateInfo createInfo)
		{
			wptBuffer.Add(createInfo);
		}

		private static int m_day = 2;
		private static int m_month = 1;
		private static int m_year = 8888;
		private static DateTime m_dt = new DateTime(m_year, m_month, m_day, 0, 0, 0, 0);		// to grow with route points

		public void GetRoutes(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			ArrayList namesBuffer = new ArrayList();
			ArrayList nameRoutesBuffer = new ArrayList();

			// now get the route information, which is actually list of names:
			string source = "MGN-GPS-" + DateTime.Now;

			int total = 0;
			int toReceive = -1;		// we don't know how many routepoints are there in GPS
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;
			DateTime dtPrev = DateTime.MinValue;
			char[] digits = "0123456789".ToCharArray();

			m_linkLayer.SetHandshake(Project.gpsMagellanHandshake);
			if(Project.gpsMagellanHandshake)
			{
				m_linkLayer.SetTransferMode(true);
			}
			MPacketCmd trackCmd = new MPacketCmd("ROUTE");
			m_linkLayer.SendPacket(trackCmd);
			while(!finished)
			{
				MPacketReceived received;
				try
				{
					received = m_linkLayer.ReceivePacket();
				}
				catch(CommPortException e)
				{
					m_linkLayer.logError("exception waiting for GPS response: " + e.Message);
					break;
				}
				//m_linkLayer.log("" + total + " ===  " + received);
				if(!received.isGood)
				{
					m_linkLayer.logError("GetRoutes: bad packet received, count=" + total + " content=" + received.m_string);
					continue;
				}
				m_linkLayer.log("GetRoutes: good packet received, count=" + total + "   " + received);
				if(received.header.Equals("PMGNRTE"))
				{
					try
					{
						MPacket trkpt  = received.fromString();

						// $PMGNRTE,14,2,c,1,T01P03,a,T01P04,a*35   - message 2 of 14, "c" means this is route, route name "1", "m" means it is route message
						//                          a        a   - means default icon

						string routeName = "";
						try
						{
							string sCount	= (string)trkpt.fields[0];
							string sPtr		= (string)trkpt.fields[1];
							string type		= (string)trkpt.fields[2];
							routeName		= (string)trkpt.fields[3];
						
							if("c".Equals(type))
							{
								for(int j=4; j < trkpt.fields.Count ;j++)
								{
									string name	 = (string)trkpt.fields[j];
									if(name != null && name.Length > 0)
									{
										//m_linkLayer.log("GetRoutes: name=" + name);
										namesBuffer.Add(name);
										nameRoutesBuffer.Add(routeName);
										total++;
									}
								}
							}
						}
						catch(Exception e)
						{
							m_linkLayer.logError("exception parsing PMGNRTE: " + e.Message);
						}
						
						if((total % 3) == 0)
						{
							progressCallback("IP: uploading route: " + routeName + " to PC\n\nleg ", total, toReceive);
						}
						errors = maxErrors;
					} 
					catch(Exception ee)
					{
						m_linkLayer.logError("GetRoutes: " + ee.Message);
					}
				}
				else if(received.header.Equals("PMGNCMD"))
				{
					m_linkLayer.log("GetRoutes: end of routes received, count=" + total + "   " + received + "  names: " + namesBuffer.Count);
					finished = true;
					break;
				}
				else
				{
					m_linkLayer.log("GetRoutes: good other packet received, count=" + total + "   content=" + received);
				}
			} // end while()

			if(namesBuffer.Count == 0)
			{
				if(Project.gpsMagellanHandshake)
				{
					m_linkLayer.SetTransferMode(false);
				}
				progressCallback("Error: no routes in GPS", -1, -1);
				return;
			}

			//Thread.Sleep(3000);	// let magellan messages die down

			// now get all waypoints into a separate array, so that we can relate incoming route points to waypoints:
			wptBuffer.Clear();
			gettingRoute = true;
			GetWaypoints(new GpsInsertWaypoint(bufferWaypoint), progressCallback);
			gettingRoute = false;

			m_linkLayer.log("GetRoutes: waypoints received, count=" + wptBuffer.Count);

			if(wptBuffer.Count == 0)
			{
				progressCallback("Error: no waypoints in GPS to define a valid route", -1, -1);
				return;
			}

			// now relate list of names that came with routes to list of waypoints.
			// we need to insert waypoints within the route in the same order as the names came from GPS, with time growing:
			string nameRoutePrev = "";
			for(int i=0; i < namesBuffer.Count; i++)
			{
				string name = (string)namesBuffer[i];
				string nameRoute = (string)nameRoutesBuffer[i];
				if(!nameRoute.Equals(nameRoutePrev))
				{
					Project.trackId++;

					string routeName = "MGN-ROUTE-" + nameRoute; // + "-" + Project.trackId;

					CreateInfo createInfo = new CreateInfo();

					createInfo.init("rte");
					createInfo.id = Project.trackId;	// track id
					createInfo.source = source;
					createInfo.name = routeName;

					insertWaypoint(createInfo);			// actually inserts a Track (route)

					nameRoutePrev = nameRoute;
				}
				// find corresponding waypoint in the big buffer:
				bool wptFound = false;
				string nameL = name.ToLower();

				for(int j=0; j < wptBuffer.Count; j++)
				{
					CreateInfo createInfo = (CreateInfo)wptBuffer[j];
					string infosName = ("" + createInfo.name).Trim();
					string infosDetail = createInfo.desc;
					string infosNameL = infosName.ToLower();
					if(infosNameL.Equals(nameL) || infosNameL.StartsWith(nameL + " "))
					{
						wptFound = true;
						m_dt = m_dt.AddSeconds(1);		// grow time
						createInfo.dateTime = m_dt;
						if(infosDetail != null && infosDetail.Length > 0 && infosDetail.Equals(infosName))
						{
							createInfo.name = infosName;
						}
						if(createInfo.name.StartsWith("T") && createInfo.name.Substring(1,1).IndexOfAny(digits) == 0)
						{
							int ppos = createInfo.name.IndexOf("P");
							if(ppos == 2 || ppos == 3 && createInfo.name.Substring(2,1).IndexOfAny(digits) == 0)
							{
								string toReplace = createInfo.name.Substring(0, ppos + 1);
								createInfo.name = createInfo.name.Replace(toReplace,"");
							}
						}
						createInfo.id = Project.trackId;
						insertWaypoint(new CreateInfo(createInfo));
						//wptBuffer.RemoveAt(j);
						break;
					}
				}
				if(!wptFound)
				{
					m_linkLayer.logError("name='" + name + "' route='" + nameRoute + "' could not be related");
				}
#if DEBUG
				else
				{
					m_linkLayer.log("name='" + name + "' route='" + nameRoute + "' related ok");
				}
#endif
			}
			wptBuffer.Clear();
			namesBuffer.Clear();
			nameRoutesBuffer.Clear();

			progressCallback("loaded OK", total, total);
			m_linkLayer.log("GetRoutes: loaded OK count=" + total + "  trackpoints");
		}

		private bool gettingRoute = false;

		public void GetWaypoints(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			string source = "MGN-GPS-" + DateTime.Now;

			int total = 0;
			int toReceive = -1;		// we don't know how many waypoints are there in GPS
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;

			if(!gettingRoute)
			{
				m_linkLayer.SetHandshake(Project.gpsMagellanHandshake);
				if(Project.gpsMagellanHandshake)
				{
					m_linkLayer.SetTransferMode(true);
				}
			}
			m_linkLayer.SendPacket(new MPacketCmd("WAYPOINT"));
			while(!finished)
			{
				MPacketReceived received;
				try
				{
					received = m_linkLayer.ReceivePacket();
				}
				catch(CommPortException e)
				{
					m_linkLayer.logError("exception waiting for GPS response: " + e.Message);
					break;
				}
				//m_linkLayer.log("" + total + " ===  " + received);
				if(!received.isGood)
				{
					m_linkLayer.logError("GetWaypoints: bad packet received, count=" + total + " content=" + received.m_string);
					continue;
				}
				//m_linkLayer.log("GetWaypoints: good packet received, count=" + total + "   " + received);
				if(received.header.Equals("PMGNWPL") || received.header.Equals("WPL"))	// NMEA WPL may come from some devices
				{
					try
					{
						MPacket wpt  = received.fromString();
						string type = gettingRoute ? "rtept" : "wpt";

						// $PMGNWPL,3328.069,N,11738.812,W,0000000,M,Road to,GCF320 = Road to Wat,a*68
						double Lat = wpt.parseLat(0);
						double Lng = wpt.parseLng(0);
						double Elev = wpt.parseElev(4);
						string sym = null;
						string name = (string)wpt.fields[6];		// short name
						if(name != null && name.StartsWith("GC"))
						{
							type = "geocache";
							sym = "Geocache";
						}
						
						string descr = null;						// description
						if(wpt.fields.Count > 7)
						{
							descr = (string)wpt.fields[7];
							if(descr != null && descr.StartsWith("GC"))
							{
								type = "geocache";
								sym = "Geocache";
								string _name;
								string _descr;
								if(Project.splitGcDescr(descr, out _name, out _descr))
								{
									name = _name;
									descr = _descr;
								}
							}
						}
						if(wpt.fields.Count > 8)
						{
							string icon = "" + (string)wpt.fields[8];	// icon - a (not found), b (found)
							if(icon.Equals("b") && "geocache".Equals(type))
							{
								type += " found";
								sym = "Geocache Found";
							}
						}
						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = Lat;
						createInfo.lng = Lng;
						createInfo.elev = Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						if(!createInfo.name.Equals(descr))
						{
							createInfo.desc = descr;		// can be null
							createInfo.urlName = descr;		// can be null
						}
						//if(!"a".Equals(icon) && !"b".Equals(icon))
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);

						total++;
						if((total % 3) == 0)
						{
							progressCallback("IP: uploading waypoints to PC:", total, toReceive);
						}
						errors = maxErrors;
					} 
					catch(Exception ee)
					{
						m_linkLayer.logError("GetWaypoints: " + ee.Message);
					}
				}
				else if(received.header.Equals("PMGNCMD"))
				{
					m_linkLayer.log("GetWaypoints: end of waypoints received, count=" + total + "   " + received);
					finished = true;
					break;
				}
				else
				{
					m_linkLayer.log("GetWaypoints: good other packet received, count=" + total + "   content=" + received);
				}
			} // end while()

			if(Project.gpsMagellanHandshake)
			{
				m_linkLayer.SetTransferMode(false);
			}
			progressCallback("loaded OK", total, total);
			m_linkLayer.log("GetWaypoints: loaded OK count=" + total + "  waypoints");
		}

		private bool puttingRoute = false;

		public void PutWaypoints(ArrayList waypoints, GpsProgressHandler progressCallback)
		{
			int count = 0;
			int toSend = waypoints.Count;
			int maxErrors = 5;
			int errors = maxErrors;
			int toTrim = 40;

			//m_linkLayer.SetHandshake(Project.gpsMagellanHandshake);
			m_linkLayer.SetHandshake(true);		// safer with handshake on
			m_linkLayer.SetTransferMode(true);

			foreach(Waypoint wpt in waypoints)
			{
				// send toSend MPacket's, followed by PMGNCMD
				try 
				{
					MPacket packetToSend = new MPacket("PMGNWPL");

					// $PMGNWPL,3339.889,N,11736.283,W,0000155,M,WPT001,this is comment,a*7E

					packetToSend.packCoord(wpt.Location);	// adds 6 fields to array

					//string name = wpt.WptName;
					string name = puttingRoute ? (wpt.WptName.Length > 0 ? wpt.WptName : wpt.Name) : wpt.WptName;
					string desc = wpt.Desc.Length > 0 ? wpt.Desc : wpt.UrlName;

					if(!puttingRoute)
					{
						// keep it in sync with GarminProtoLinkProtocols:sendWaypoint()
						if(Project.gpsWptDescrAsName)
						{
							string _desc = desc;
							if(wpt.LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache && name.StartsWith("GC"))
							{
								desc = name + " = " + desc;
							}
							if(_desc.Length > 0)
							{
								name = _desc;
							}
						}
						// end sync

						name = name.Length > 12 ? name.Substring(0, 12) : name;
					}

					desc = desc.Length > toTrim ? desc.Substring(0, toTrim) : desc;

					packetToSend.fields.Add(name);			// name - truncated in GPS
					packetToSend.fields.Add(desc);			// comment has more characters in GPS
					string icon = wpt.Found ? "b" : "a";
					packetToSend.fields.Add(icon);			// icon - "crossed box"=a  or "box in the box"=b

					// link layer will make sure all fields are clean of wrong symbols like ","
					m_linkLayer.SendPacket(packetToSend);
					count++;
					if(count % 10 == 0)
					{
						progressCallback("IP: downloading to GPS - waypoint ", count, toSend);
					}
				}
				catch (Exception e)
				{
					m_linkLayer.logError("PutWaypoints: " + e.Message + "  count=" + count);
					if(errors-- > 0)
					{
						// on timeout, try continue sending and bail out after too many failures
						continue;
					}
					else
					{
						m_linkLayer.logError("PutWaypoints: too many errors, stopping transfer");
						throw(e);
					}
				}
			}
			m_linkLayer.SendPacket(new MPacketCmd("END"));

			if(!puttingRoute)
			{
				m_linkLayer.SetTransferMode(false);
			}

			progressCallback("OK: downloaded to GPS - waypoints", count, toSend);

			m_linkLayer.log("OK: downloaded to GPS - waypoints: " + count);
		}

		private static char[] rteChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

		private string toTptName(int routeNumber, string baseName)		// route number 1 to 20
		{
			switch(Project.gpsMagellanRouteNameConversionMethod)
			{
				default:
				case 0:
					return baseName;
				case 1:
					return routeNumber > 1 ? ("" + rteChars[routeNumber-2] + baseName) : baseName;
				case 2:
					return "T" + routeNumber + "P" + baseName;
			}
		}

		private string truncateName(string name)
		{
			// actually names are truncated to 8 chars in the device, descriptions - to 14 chars
			name = name.Replace(" ", "");
			name = name.Length > 20 ? name.Substring(0, 20) : name;
			if(name.ToLower().EndsWith("endro"))
			{
				name = name.Substring(0, 6);
			}
			return name.ToUpper();
		}

		public void PutRoutes(ArrayList routes, GpsProgressHandler progressCallback, int startRouteNumber)
		{
			// here is a problem: the SporTrack will reject routes which have same waypoint repeating twice on one leg.
			// the waypoint can repeat in the route many times, if separated by other waypoints.
			// rejection happens in the form of "UNABLE" response on the physical layer, m_linkLayer.ErrorCount() shows that.
			// the condition should not happen, as it is cleared during creation of the routes in LayerWaypoints.cs

			// we need to send all waypoints for all routes first, and then send linking info.
			m_linkLayer.resetErrorCount();

			// make list of relevant waypoints and count them
			ArrayList trkptsToSend = new ArrayList();
			int toSendWpt = 0;		// for ProgressCallback
			int toSendPkt = 0;
			int routeNumber = startRouteNumber;	// 1 to 20
			foreach(Track route in routes)
			{
				if(route.isRoute)
				{
					int cnt = 0;
					Waypoint prevWpt = null;
					for(int i=0; i < route.Trackpoints.Count ;i++)
					{
						Waypoint trkpt = (Waypoint)route.Trackpoints.GetByIndex(i);
						if(prevWpt == null || !prevWpt.Location.almostAs(trkpt.Location, 0.0002))	// 0.0001 degree = 10 meters 
						{
							Waypoint cloneTrkpt = new Waypoint(trkpt);
							string trkptName = toTptName(routeNumber, trkpt.Name);		// use .Name here, as WptName is empty for rtept's
							cloneTrkpt.WptName = truncateName(trkptName);
							cloneTrkpt.Desc = trkptName;
							trkptsToSend.Add(cloneTrkpt);
							prevWpt = trkpt;
							cnt++;
						}
					}
					toSendPkt += (cnt + 1) / 2;
					toSendWpt += cnt;
					routeNumber++;
				}
			}
			if(toSendWpt == 0)
			{
				progressCallback("Error: no routes to upload ", 0, 0);
				return;
			}

			puttingRoute = true;
			PutWaypoints(trkptsToSend, progressCallback);		// will leave TransferMode
			puttingRoute = false;

			int count = 0;
			int maxErrors = 5;
			int errors = maxErrors;
			int maxRoutepointCount = 0;
			string maxRoutepointName = "";

			routeNumber = startRouteNumber;
			foreach(Track route in routes)
			{
				if(route.isRoute)
				{
					// make a temp list of trackpoints - specifically for this route, and count them:
					ArrayList trackpoints = new ArrayList();
					foreach(Waypoint wpt in trkptsToSend)
					{
						if(wpt.TrackId == route.Id)
						{
							trackpoints.Add(wpt);
						}
					}
					int toSendThisRoute = (trackpoints.Count + 1) / 2;
					if(trackpoints.Count > maxRoutepointCount)
					{
						maxRoutepointCount = trackpoints.Count;
						maxRoutepointName = route.Name;
					}

					int countThisRoute = 1;
					for(int i=0; i < trackpoints.Count ;i+=2)
					{
						int numPoints = (trackpoints.Count - i > 1) ? 2 : 1;
						Waypoint trkpt1 = (Waypoint)trackpoints[i];
						string trkptName1 = trkpt1.WptName.Trim();
						Waypoint trkpt2 = numPoints > 1 ? (Waypoint)trackpoints[i + 1] : null;
						string trkptName2 = numPoints > 1 ? trkpt2.WptName.Trim() : "";

						// send MPackets, followed by PMGNCMD
						try 
						{
							MPacket packetToSend = new MPacket("PMGNRTE");

							// $PMGNRTE,15,5,c,2,T02P09,a,T02P10,a*3F

							packetToSend.fields.Add("" + toSendThisRoute);
							packetToSend.fields.Add("" + countThisRoute);
							packetToSend.fields.Add("c");
							packetToSend.fields.Add("" + routeNumber);
							packetToSend.fields.Add(trkptName1);
							packetToSend.fields.Add("a");
							if(numPoints > 1)
							{
								packetToSend.fields.Add(trkptName2);
								packetToSend.fields.Add("a");
							}

							m_linkLayer.SendPacket(packetToSend);
							countThisRoute++;
							count++;
							if(count % 10 == 0)
							{
								progressCallback("IP: downloading to GPS - route packet ", count, toSendPkt);
							}
						}
						catch (Exception e)
						{
							m_linkLayer.logError("PutRoutes: " + e.Message + "  count=" + count);
							if(errors-- > 0)
							{
								// on timeout, try continue sending and bail out after too many failures
								continue;
							}
							else
							{
								m_linkLayer.logError("PutRoutes: too many errors, stopping transfer");
								throw(e);
							}
						}
					}
					routeNumber++;
				}
			}

			m_linkLayer.SendPacket(new MPacketCmd("END"));
			m_linkLayer.SetTransferMode(false);

			if(m_linkLayer.ErrorCount() > 0)
			{
				// caught some packets with "UNABLE" response.
				progressCallback("Errors: " + m_linkLayer.ErrorCount() + " out of route packets: ", count, toSendPkt);
				Project.ErrorBox(Project.mainCommand.gpsManagerForm(), "Route '" + maxRoutepointName + "' (" + maxRoutepointCount
					+ " route points after combing) failed to load, possibly due to 30 legs-per-route limitation of the Magellan devices."
					+ "\n\nYou can break the route into smaller ones, or remove some mid-route points."
					+ "\nAlternatively you can try clearing waypoints/routes memory in the device.");
			}
			else
			{
				progressCallback("OK: downloaded to GPS - route packets", count, toSendPkt);
			}
			m_linkLayer.log("OK: downloaded to GPS - route packets: " + count + " errors: " + m_linkLayer.ErrorCount());
		}

		protected static bool m_stopRealTime = false;

		public void StartRealTime(GpsRealTimeHandler realtimeCallback)
		{
			int total = 0;
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;
			m_stopRealTime = false;
			DateTime lastGPGGA = DateTime.MinValue;
			DateTime lastGPRMC = DateTime.MinValue;
			double lastGPRMCvelocity = 0.0d;
			double lastGPRMCvelocityNorth = 0.0d;
			double lastGPRMCvelocityEast = 0.0d;

			//m_linkLayer.SetHandshake(Project.gpsMagellanHandshake);
			m_linkLayer.SetHandshake(true);		// safer with handshake on
			m_linkLayer.SendPacket(new MPacketCmd("FIX"));
			m_linkLayer.SendPacket(new MPacketCmd("NMEAON"));

			while(!finished && !m_stopRealTime)
			{
				GpsRealTimeData pvtData = new GpsRealTimeData();		// will be returned empty if no data

				MPacketReceived received = null;
				try
				{
					received = m_linkLayer.ReceivePacket();
					if(!received.isGood)
					{
						m_linkLayer.logError("StartRealTime: bad packet received, count=" + total);
						pvtData.comment = "bad data packet from GPS";
						realtimeCallback(pvtData);
						continue;
					}
				}
				catch(Exception e)
				{
					m_linkLayer.logError("StartRealTime exception: " + e.Message + "  count=" + total);
					//if(received != null)
					{
						try 
						{
							m_linkLayer.ReOpen();
						} 
						catch (Exception ee)
						{
							m_linkLayer.logError("while ReOpen(): " + ee.Message);
						}
					}
					pvtData.comment = e.Message.StartsWith("Timeout") ?
												"GPS not responding - check cable" : ("error: " + e.Message);
					realtimeCallback(pvtData);
					continue;
				}
				
				m_linkLayer.log("StartRealTime: good packet received, count=" + total + "   " + received);

				if(received.header.Equals("GPGGA"))	// NMEA GGA expected for location (best data packet to hope for from Magellan)
				{
					try
					{
						MPacket pvt  = received.fromString();

						/*
							$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47

							Where:
								GGA          Global Positioning System Fix Data
							0	123519       Fix taken at 12:35:19 UTC
							1	4807.038,N   Latitude 48 deg 07.038' N
							3	01131.000,E  Longitude 11 deg 31.000' E
							5	1            Fix quality: 0 = invalid
														1 = GPS fix
														2 = DGPS fix
														6 = estimated (2.3 feature)
							6	08           Number of satellites being tracked
							7	0.9          Horizontal dilution of position
							8	545.4,M      Altitude, Meters, above mean sea level
							10	46.9,M       Height of geoid (mean sea level) above WGS84
												ellipsoid
							12	(empty field) time in seconds since last DGPS update
							13	(empty field) DGPS station ID number
								*47          the checksum data, always begins with *
						*/

						double Lat = pvt.parseLat(1);
						double Lng = pvt.parseLng(1);
						double Elev = pvt.parseElev(8);		// alt above mean sea level
						pvtData.location = new GeoCoord(Lng, Lat, Elev);
						//m_linkLayer.log("coord=" + pvtData.location);

						// time component:
                        pvtData.time = pvt.parseNmeaTime(0, Project.localToZulu(DateTime.Now));	// UTC

						// errors and velocity:
						pvtData.posErrorH = Convert.ToDouble((string)pvt.fields[7]) * 2.0d;		// ad-hoc factor to match device EPE reading 
						pvtData.posError = pvtData.posErrorH;
						pvtData.posErrorV = -1;
						pvtData.fix = 0;			// failed integrity check
						switch((string)pvt.fields[5])
						{
							case "0":				// invalid
								pvtData.fix = 1;	// invalid
								break;
							case "1":				// GPS fix
								pvtData.fix = 2;	// two dimensional
								break;
							case "2":				// DGPS fix
								pvtData.fix = 5;	// two dimensional differential
								break;
							case "6":				// estimated
								pvtData.fix = 6;	// estimated
								break;
						}

						string msg = "received " + received.header;
						// GPGGA is lacking velocity data, mix it in from a recent GPRMC
						if(DateTime.Now.Ticks < lastGPRMC.AddSeconds(10.0d).Ticks)
						{
							msg += "/GPRMC";
							pvtData.velocity = lastGPRMCvelocity;
							pvtData.velocityNorth = lastGPRMCvelocityNorth;
							pvtData.velocityEast = lastGPRMCvelocityEast;
						}
						else
						{
							pvtData.velocity = -1.0d;
							pvtData.velocityNorth = 0.0d;
							pvtData.velocityEast = 0.0d;
						}
						pvtData.velocityUp = 0.0d;

						pvtData.comment = msg;
						lastGPGGA = DateTime.Now;		// GPGGA is the best we can hope for, don't allow others to be processed if we have this one
						realtimeCallback(pvtData);

						total++;
						errors = maxErrors;
					} 
					catch(Exception ee)
					{
						m_linkLayer.logError("StartRealTime: " + ee.Message);
					}
				}
				else if(received.header.Equals("GPRMC"))	// NMEA GPRMC may come with positioning data
				{
					m_linkLayer.log("StartRealTime: GPRMC packet received, count=" + total + "   content=" + received);
					try
					{
						MPacket pvt  = received.fromString();

						// $GPRMC,052458.73,V,3334.6441,N,11739.6622,W,00.0,000.0,290103,13,E*4F 
                        /*
                         * see http://www.gpsinformation.org/dale/nmea.htm for NMEA reference
                         * 
                        RMC - NMEA has its own version of essential gps pvt (position, velocity, time) data.
                        It is called RMC, The Recommended Minimum, which might look like: 

                        $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A

                        Where:
                            RMC          Recommended Minimum sentence C
                        0	123519       Fix taken at 12:35:19 UTC
                        1	A            Status A=active or V=Void.
                        2	4807.038,N   Latitude 48 deg 07.038' N
                        4	01131.000,E  Longitude 11 deg 31.000' E
                        6	022.4        Speed over the ground in knots
                        7	084.4        Track angle in degrees True
                        8	230394       Date - 23rd of March 1994
                        9	003.1,W      Magnetic Variation
                            *6A          The checksum data, always begins with *

                        Note that, as of the 2.3 release of NMEA, there is a new field in the RMC sentence
                            at the end just prior to the checksum. The value of the entry is
                            A=autonomous, D=differential, E=estimated, N=Data not valid. 
                        */
                        bool status = "A".Equals((string)pvt.fields[1]);
						if(status)
						{
							// velocity is used to mix in GPGGA, compute it first:
							double knots = Double.Parse((string)pvt.fields[6]);
							lastGPRMCvelocity = knots * Distance.METERS_PER_NAUTICAL_MILE / 3600.0d;
							// calculate velocity vector based on track angle:
							double trackAngleRad = Convert.ToDouble(pvt.fields[7]) * Math.PI / 180.0d;
							lastGPRMCvelocityNorth = lastGPRMCvelocity * Math.Cos(trackAngleRad);
							lastGPRMCvelocityEast  = lastGPRMCvelocity * Math.Sin(trackAngleRad);
							lastGPRMC = DateTime.Now;

							// GPGGA is the best data we can hope for, don't allow others to be processed if we had a recent GPGGA
							if(DateTime.Now.Ticks > lastGPGGA.AddSeconds(10.0d).Ticks)
							{
								double Lat = pvt.parseLat(2);
								double Lng = pvt.parseLng(2);
								double Elev = 0.0d;
								pvtData.location = new GeoCoord(Lng, Lat, Elev);
								//m_linkLayer.log("coord=" + pvtData.location);

								// time component (only time part):
                                pvtData.time = pvt.parseNmeaDateTime(0, 8);	// UTC

								pvtData.velocity		= lastGPRMCvelocity;
								pvtData.velocityNorth	= lastGPRMCvelocityNorth;
								pvtData.velocityEast	= lastGPRMCvelocityEast;

								pvtData.fix = 2;			// assume two-dimensional
								pvtData.comment = "received " + received.header;

								realtimeCallback(pvtData);
							}
						}
						total++;
						errors = maxErrors;
					} 
					catch(Exception ee)
					{
						m_linkLayer.logError("StartRealTime: " + ee.Message);
					}
				}
				else if(received.header.Equals("GPGLL") || received.header.Equals("LCGLL"))	// NMEA GPGLL may come with positioning data
				{
					m_linkLayer.log("StartRealTime: GPGLL packet received, count=" + total + "   content=" + received);
					// GPGGA is the best we can hope for, don't allow others to be processed if we had a recent GPGGA
					if(DateTime.Now.Ticks > lastGPGGA.AddSeconds(10.0d).Ticks)
					{
						try
						{
							MPacket pvt  = received.fromString();
							// $GPGLL,3334.6464,N,11739.6583,W,052707.129,A*29
                            /*
                             * see http://www.gpsinformation.org/dale/nmea.htm for NMEA reference
                             * 
                            GLL - Geographic Latitude and Longitude is a holdover from Loran data and some old units
                                 may not send the time and data valid information if they are emulating Loran data.
                                 If a gps is emulating Loran data they may use the LC Loran prefix instead of GP. 

                            $GPGLL,4916.45,N,12311.12,W,225444,A,*31

                            Where:
                                GLL          Geographic position, Latitude and Longitude
                            0	4916.46,N    Latitude 49 deg. 16.45 min. North
                            2	12311.12,W   Longitude 123 deg. 11.12 min. West
                            4	225444       Fix taken at 22:54:44 UTC
                            5	A            Data valid or V (void)
                                *31          checksum data

                            */
                            bool status = "A".Equals((string)pvt.fields[5]);
							if(status)
							{
								double Lat = pvt.parseLat(0);
								double Lng = pvt.parseLng(0);
								double Elev = 0.0d;
								pvtData.location = new GeoCoord(Lng, Lat, Elev);
								//m_linkLayer.log("coord=" + pvtData.location);

								// time component (only time part):
                                pvtData.time = pvt.parseNmeaTime(4, Project.localToZulu(DateTime.Now));	// UTC

								pvtData.fix = 2;			// assume two-dimensional
								pvtData.comment = "received " + received.header;
								realtimeCallback(pvtData);
							}
							total++;
							errors = maxErrors;
						} 
						catch(Exception ee)
						{
							m_linkLayer.logError("StartRealTime: " + ee.Message);
						}
					}
				}
				else if(received.header.Equals("GPGSV"))	// NMEA GPGSV may come for satellite reception status
				{
					m_linkLayer.log("StartRealTime: GPGSV packet received, count=" + total + "   content=" + received);
                    /*
                     * see http://www.gpsinformation.org/dale/nmea.htm for NMEA reference
                     * 
                    $GPGSV,3,1,08,14,69,204,,15,57,105,36,18,45,047,36,03,42,263,36*71
                    $GPGSV,2,1,08,01,40,083,46,02,17,308,41,12,07,344,39,14,22,228,45*75

                    Where:
                        GSV          Satellites in view
                    0	2            Number of sentences for full data
                    1	1            sentence 1 of 2
                    2	08           Number of satellites in view
                    for up to 4 satellites per sentence {
                            3	01           Satellite PRN number
                            4	40           Elevation, degrees
                            5	083          Azimuth, degrees
                            6	46           Signal strength - higher is better
                    }
                        *75          the checksum data, always begins with *
                     */

                    //MPacket pvt  = received.fromString();
					//pvtData.comment = "received";
					//realtimeCallback(pvtData);

					total++;
					errors = maxErrors;
				}
				else if(received.header.Equals("GPGSA"))	// NMEA GPGSA may come for satellite status
				{
					m_linkLayer.log("StartRealTime: GPGSA packet received, count=" + total + "   content=" + received);
					// $GPGSA,A,2,15,18,14,31,,,,,,,,,05.6,05.6,*17
                    /*
                     * see http://www.gpsinformation.org/dale/nmea.htm for NMEA reference
                     * 
                    GSA - GPS DOP and active satellites. This sentence provides details on the nature of the
                    fix. It includes the numbers of the satellites being used in the current solution and 
                    the DOP. DOP (dilution of precision) is an indication of the effect of satellite geometry 
                    on the accuracy of the fix. It is a unitless number where smaller is better. For 3D fixes 
                    using 4 satellites a 1.0 would be considered to be a perfect number. For overdetermined
                    solutions it is possible to see numbers below 1.0. There are differences in the way the
                    PRN's are presented which can effect the ability of some programs to display this data.
                    For example, in the example shown below there are 5 satellites in the solution and
                    the null fields are scattered indicating that the almanac would show satellites in 
                    the null positions that are not being used as part of this solution. Other receivers
                    might output all of the satellites used at the beginning of the sentence with the 
                    null field all stacked up at the end. This difference accounts for some satellite 
                    display programs not always being able to display the satellites being tracked. 

                    $GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39

                    Where:
                        GSA      Satellite status
                        A        Auto selection of 2D or 3D fix (M = manual) 
                        3        3D fix - other values include: 1 = no fix
                                                                2 = 2D fix
                        04,05... PRNs of satellites used for fix (space for 12) 
                        2.5      PDOP (dilution of precision) 
                        1.3      Horizontal dilution of precision (HDOP) 
                        2.1      Vertical dilution of precision (VDOP)
                        *39      the checksum data, always begins with *

                     */
                }
				else if(received.header.Equals("PMGNST"))	// Magellan PMGNST may come for device status
				{
					m_linkLayer.log("StartRealTime: PMGNST packet received, count=" + total + "   content=" + received);
					/*
					$PMGNST,02.12,3,T,534,05.0,+03327,00*40 

					where:
						ST      status information
					0	02.12   Firmware version number
					1	3       2D or 3D
					2	T       True if we have a fix False otherwise
					3	534     numbers change - unknown
					4	05.0    time left on the gps battery in hours
					5	+03327  numbers change (freq. compensation?)
					6	00      PRN number receiving current focus
						*40    checksum
					*/

					MPacket pvt  = received.fromString();
					bool haveFix = "T".Equals((string)pvt.fields[2]);
					if(!haveFix)
					{
						pvtData.comment = "no satellite fix";
						realtimeCallback(pvtData);
					}
					//total++;
					errors = maxErrors;
				}
				else
				{
					m_linkLayer.log("StartRealTime: good other packet received, count=" + total + "   content=" + received);
				}
			} // end while()

			m_linkLayer.SendPacket(new MPacketCmd("NMEAOFF"));
			m_linkLayer.SendPacket(new MPacketCmd("STOP"));
			//if(Project.gpsMagellanHandshake)
			{
				m_linkLayer.SetHandshake(false);
			}
			m_linkLayer.log("StartRealTime: finished receiving fixes count=" + total);
		}

		public void StopRealTime()
		{
			m_stopRealTime = true;
		}
	}
}
