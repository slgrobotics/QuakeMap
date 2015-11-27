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

using LibSys;
using LibGeo;

namespace LibGps
{
	// ========================= PVT Data Protocol (A800) ============================

	public class PvtTransferProtocol
	{
		protected GarminProtoLinkLayer m_linkLayer;
		protected static bool m_stopRealTime = false;

		public PvtTransferProtocol(GarminProtoLinkLayer linkLayer)
		{
			LibSys.StatusBar.Trace("PvtTransferProtocol A800 D800");
			m_linkLayer = linkLayer;
		}

		public void StartRealTime(GpsRealTimeHandler realtimeCallback)
		{
			int count = 0;
			int maxErrors = 5;
			int errors = maxErrors;
			GPacketReceived received = null;

			m_stopRealTime = false;

			m_linkLayer.SendPacket(new GPacketA010Commands(A010Commands.Cmnd_Start_Pvt_Data));
			// ACK may come and will be ignored in receive loop.

			while(!m_stopRealTime)
			{
				GpsRealTimeData pvtData = new GpsRealTimeData();		// will be returned empty if no data

				// receive many Pid_Pvt_Data packets
				try 
				{
					//LibSys.StatusBar.Trace("StartRealTime: ReceivePacket");
					received = m_linkLayer.ReceivePacket();
					//LibSys.StatusBar.Trace("StartRealTime: ReceivePacket - OK " + m_stopRealTime);
					if(m_stopRealTime)
					{
						break;
					}
					if(!received.isGood)
					{
						LibSys.StatusBar.Error("StartRealTime: bad packet received, count=" + count);
						pvtData.comment = "bad data packet from GPS";
						realtimeCallback(pvtData);
						continue;
					}
				}
				catch (Exception e)
				{
					LibSys.StatusBar.Error("StartRealTime: " + e.Message + "  count=" + count);
					if(m_stopRealTime)
					{
						break;
					}
					// timeout may mean that GPS is not tracking satellites
					//if(received != null)
					{
						try 
						{
							m_linkLayer.ReOpen();
						} 
						catch (Exception ee)
						{
							LibSys.StatusBar.Error("while ReOpen(): " + ee.Message);
						}
					}
					pvtData.comment = e.Message.StartsWith("Timeout") ?
						"no satellite signal or cable not connected" : ("error: " + e.Message);
					realtimeCallback(pvtData);
		
					continue;
				}

				switch(received.pid)
				{
					case (int)L001Pids.Pid_Pvt_Data:
					{
						D800_Pvt_Data_Type pvt = (D800_Pvt_Data_Type)received.fromBytes(0);

						if(pvt != null)
						{
							pvtData.location = pvt.posn.toGeoCoord();
							pvtData.location.Elev = pvt.alt + pvt.msl_hght;	// the sum is alt above mean sea level
							//LibSys.StatusBar.Trace("coord=" + pvtData.location);

							// time component:  wn_days=4774 tow=103842.000957905 leap_scnds=13
							pvtData.time = new DateTime(1989, 12, 31).AddDays(pvt.wn_days).AddSeconds(Math.Floor(pvt.tow) - pvt.leap_scnds);

							// errors and velocity:
							pvtData.posError = pvt.epe;
							pvtData.posErrorH = pvt.eph;
							pvtData.posErrorV = pvt.epv;
							pvtData.fix = pvt.fix;
							pvtData.velocityEast = pvt.east;		// meters per sec
							pvtData.velocityNorth = pvt.north;		// meters per sec
							pvtData.velocityUp = pvt.up;
							pvtData.velocity = Math.Sqrt(pvtData.velocityEast * pvtData.velocityEast + pvtData.velocityNorth * pvtData.velocityNorth); 

							count++;
							pvtData.comment = "received";
							realtimeCallback(pvtData);
							errors = maxErrors;
						}
					}
						break;
					default:
#if DEBUG
						LibSys.StatusBar.Trace("-- rx pid=" + received.pid + " (ignored)");
#endif
						break;
				}
			}
			//LibSys.StatusBar.Trace("StartRealTime: Cmnd_Stop_Pvt_Data");
			m_linkLayer.SendPacketWaitAck(new GPacketA010Commands(A010Commands.Cmnd_Stop_Pvt_Data));

#if DEBUG
			LibSys.StatusBar.Trace("OK: PVT transfer complete: " + count + " packets received");
#endif
		}

		public void StopRealTime()
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: stopping PVT transfer");
#endif
			m_stopRealTime = true;
		}
	}

	// ========================= Waypoint Transfer Protocols (A100) ============================

	public class WaypointTransferProtocol
	{
		protected GarminProtoLinkLayer m_linkLayer;
		protected string m_useProto = "A100";
		protected int DType = 108;	// will be defined based on datatypes supported by useProto protocol (say, A100,D155)

		public WaypointTransferProtocol(GarminProtoLinkLayer linkLayer)
		{
			m_linkLayer = linkLayer;
			//LibSys.StatusBar.Trace("WaypointTransferProtocol()");
			ArrayList datatypes = linkLayer.ProductType.supportsData(m_useProto);
			foreach(string str in datatypes) 
			{
				int dType = Convert.ToInt32(str.Substring(1));
				if(dType >= 100 && dType < 200)
				{
					DType = dType;
				}
			}
			LibSys.StatusBar.Trace("WaypointTransferProtocol " + m_useProto + " D" + DType);
		}

		public static bool onWptData(GPacketReceived received, GpsInsertWaypoint insertWaypoint, int DType, string source)
		{
			bool ret = false;

			switch(DType)
			{
				case 100:
				{
					D100_Wpt_Type wpt = (D100_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string type = "wpt";

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;

						insertWaypoint(createInfo);

						ret = true;
					}
				}
					break;

					#region More proto cases....
				case 101:
				{
					D101_Wpt_Type wpt = (D101_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						//short symb = (short)wpt.smbl;
						string type = "wpt";

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 102:
				{
					D102_Wpt_Type wpt = (D102_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string sym = null;
						short symb = (short)wpt.smbl;
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 103:
				{
					D103_Wpt_Type wpt = (D103_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						//short symb = (short)wpt.smbl;
						string type = "wpt";

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 104:
				{
					D104_Wpt_Type wpt = (D104_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string sym = null;
						short symb = (short)wpt.smbl;
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 105:
				{
					D105_Wpt_Type wpt = (D105_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string sym = null;
						short symb = (short)wpt.smbl;
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 106:
				{
					D106_Wpt_Type wpt = (D106_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string sym = null;
						short symb = (short)wpt.smbl;
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 107:
				{
					D107_Wpt_Type wpt = (D107_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						coord.Elev = 0.0d;
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string sym = null;
						short symb = (short)wpt.smbl;
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 108:		// eTrex Vista
				{
					D108_Wpt_Type wpt = (D108_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						if(wpt.alt < 100000.0d && wpt.alt > -12000.0d)
						{
							coord.Elev = wpt.alt;
						}
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string cmt = wpt.comment;
						string desc = (wpt.facility + " " + wpt.city + " " + wpt.addr + " " + wpt.cross_road).Trim();
						string sym = null;
						short symb = (short)wpt.smbl;		// 18, 8255, 8256
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.comment = cmt;
						createInfo.desc = desc;
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 109:		// GPS V
				{
					D109_Wpt_Type wpt = (D109_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						if(wpt.alt < 100000.0d && wpt.alt > -12000.0d)
						{
							coord.Elev = wpt.alt;
						}
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string cmt = wpt.comment;
						string desc = (wpt.facility + " " + wpt.city + " " + wpt.addr + " " + wpt.cross_road).Trim();
						string sym = null;
						short symb = (short)wpt.smbl;		// 18, 8255, 8256
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.comment = cmt;
						createInfo.desc = desc;
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 150:
				{
					D150_Wpt_Type wpt = (D150_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						if(wpt.alt < 100000.0d && wpt.alt > -12000.0d)
						{
							coord.Elev = wpt.alt;
						}
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string type = "wpt";

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 151:
				{
					D151_Wpt_Type wpt = (D151_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						if(wpt.alt < 100000.0d && wpt.alt > -12000.0d)
						{
							coord.Elev = wpt.alt;
						}
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string type = "wpt";

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 152:
				{
					D152_Wpt_Type wpt = (D152_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						if(wpt.alt < 100000.0d && wpt.alt > -12000.0d)
						{
							coord.Elev = wpt.alt;
						}
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = "wpt";
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 154:
				{
					D154_Wpt_Type wpt = (D154_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						if(wpt.alt < 100000.0d && wpt.alt > -12000.0d)
						{
							coord.Elev = wpt.alt;
						}
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string sym = null;
						short symb = (short)wpt.smbl;
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;

				case 155:
				{
					D155_Wpt_Type wpt = (D155_Wpt_Type)received.fromBytes(DType);

					if(wpt != null)
					{
						GeoCoord coord = wpt.posn.toGeoCoord();
						if(wpt.alt < 100000.0d && wpt.alt > -12000.0d)
						{
							coord.Elev = wpt.alt;
						}
						//LibSys.StatusBar.Trace("coord=" + coord + "  name=" + wpt.);

						string name = wpt.ident;
						string sym = null;
						short symb = (short)wpt.smbl;
						string type = "wpt";
						switch(symb)
						{
							case (short)Symbol_Types.sym_geocache:			// 8255
								type = "geocache";
								sym = "Geocache";
								break;
							case (short)Symbol_Types.sym_geocache_found:	// 8266
								type = "geocache found";
								sym = "Geocache Found";
								break;
						}

						// we need to allocate every createInfo here, as they will be buffered in the reader thread:
						CreateInfo createInfo = new CreateInfo();

						createInfo.init("wpt");
						createInfo.lat = coord.Lat;
						createInfo.lng = coord.Lng;
						createInfo.elev = coord.Elev;
						createInfo.typeExtra = type;
						createInfo.source = source;
						createInfo.name = name.Trim();
						createInfo.desc = wpt.comment;
						if(sym != null)
						{
							createInfo.sym = sym;
						}

						insertWaypoint(createInfo);
						ret = true;
					}
				}
					break;
					#endregion
			}
			return ret;
		}

		public void GetWaypoints(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			string source = "GRM-GPS-" + DateTime.Now;

			int count = 0;
			int toReceive = 0;
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;
			GPacketReceived received = null;

			m_linkLayer.SendPacket(new GPacketA010Commands(A010Commands.Cmnd_Transfer_Wpt));
			// ACK may come and will be ignored in receive loop.

			while(!finished)
			{
				// receive many Pid_Wpt_Data packets, followed by Pid_Xfer_Cmplt
				try 
				{
					received = m_linkLayer.ReceivePacket();
					if(!received.isGood)
					{
						LibSys.StatusBar.Error("GetWaypoints: bad packet received, count=" + count);
						continue;
					}
				}
				catch (Exception e)
				{
					LibSys.StatusBar.Error("GetWaypoints: " + e.Message + "  count=" + count);
					if(errors-- > 0)
					{
						// on timeout, try to NAK the last packet we received
						if(received != null)
						{
							try 
							{
								m_linkLayer.ReOpen();
								m_linkLayer.SendNak((byte)received.pid);
							} 
							catch (Exception ee)
							{
								LibSys.StatusBar.Error("while sending NAK: " + ee.Message);
								try 
								{
									m_linkLayer.ReOpen();
								} 
								catch (Exception eee)
								{
									LibSys.StatusBar.Error("while reopening: " + eee.Message);
								}
							}
						}
						continue;
					}
					else
					{
						LibSys.StatusBar.Error("GetWaypoints: too many errors, stopping transfer");
						throw(e);
					}
				}

				switch(received.pid)
				{
					case (int)L001Pids.Pid_Records:
					{
						L001Records_Type recordsPacket = (L001Records_Type)received.fromBytes(0);
						if(recordsPacket != null)
						{
							toReceive = recordsPacket.records;
#if DEBUG
							LibSys.StatusBar.Trace("-- toReceive=" + toReceive);
#endif
						}
						errors = maxErrors;
					}
						break;
					case (int)L001Pids.Pid_Wpt_Data:
						if(onWptData(received, insertWaypoint, DType, source))
						{
							count++;
							if(count % 10 == 0)
							{
								progressCallback("IP: uploading to PC - waypoint ", count, toReceive);
							}
							errors = maxErrors;
						}
						break;
					case (int)L001Pids.Pid_Xfer_Cmplt:
					{
						progressCallback("OK: uploaded to PC - waypoints", count, toReceive);
#if DEBUG
						LibSys.StatusBar.Trace("-- Xfer_Cmplt");
#endif
						finished = true;
						errors = maxErrors;
					}
						break;
					default:
						LibSys.StatusBar.Trace("-- rx pid=" + received.pid + " (ignored)");
						break;
				}
			}
			LibSys.StatusBar.Trace("OK: uploaded to PC - waypoints: " + count);
		}


		public static void sendWaypoint(GarminProtoLinkLayer linkLayer, Waypoint wpt, int DType, bool isRoute)
		{
			GPacket packetToSend = null;
			int pid = isRoute ? (int)L001Pids.Pid_Rte_Wpt_Data : (int)L001Pids.Pid_Wpt_Data;
			int toTrim = 50;

			string name = isRoute ? (wpt.WptName.Length > 0 ? wpt.WptName : wpt.Name) : wpt.WptName;
			string desc = wpt.Desc.Length > 0 ? wpt.Desc : wpt.UrlName;	// will be truncated to 50 characters

			// keep it in sync with MagellanProtoLayer:PutWaypoints()
			if(!isRoute && Project.gpsWptDescrAsName)
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

			name = name.Replace(","," ").Trim();
			name = name.Length > 20 ? name.Substring(0, 20) : name;

			desc = desc.Replace("\n"," ").Replace("\r","").Trim();
			desc = desc.Length > toTrim ? desc.Substring(0, toTrim) : desc;

			short smbl = (short)(Symbol_Types.sym_wpt_dot);
			byte smblB = (byte)0;
			if(wpt.LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache)
			{
				smbl = (short)(wpt.Found ? Symbol_Types.sym_geocache_found : Symbol_Types.sym_geocache);
				smblB = (byte)(wpt.Found ? 12 : 10);
			}

			switch(DType)
			{
				case 100:
				{
					D100_Wpt_Type wptPacket = new D100_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.comment = desc;
					packetToSend = wptPacket;
				}
					break;

					#region More proto cases....
				case 101:
				{
					D101_Wpt_Type wptPacket = new D101_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.comment = desc;
					wptPacket.smbl = smblB;
					packetToSend = wptPacket;
				}
					break;

				case 102:
				{
					D102_Wpt_Type wptPacket = new D102_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.comment = desc;
					wptPacket.smbl = smbl;
					packetToSend = wptPacket;
				}
					break;

				case 103:
				{
					D103_Wpt_Type wptPacket = new D103_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.comment = desc;
					wptPacket.smbl = (D103_Wpt_Type_Symbols)smblB;
					packetToSend = wptPacket;
				}
					break;

				case 104:
				{
					D104_Wpt_Type wptPacket = new D104_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.comment = desc;
					wptPacket.smbl = smbl;
					packetToSend = wptPacket;
				}
					break;

				case 105:
				{
					D105_Wpt_Type wptPacket = new D105_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.smbl = smbl;
					packetToSend = wptPacket;
				}
					break;

				case 106:
				{
					D106_Wpt_Type wptPacket = new D106_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.smbl = smbl;
					packetToSend = wptPacket;
				}
					break;

				case 107:
				{
					D107_Wpt_Type wptPacket = new D107_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.comment = desc;
					wptPacket.smbl = smblB;
					packetToSend = wptPacket;
				}
					break;

				case 108:
				{
					D108_Wpt_Type wptPacket = new D108_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.comment = desc;
					wptPacket.smbl = smbl;
					packetToSend = wptPacket;
				}
					break;

				case 109:
				{
					D109_Wpt_Type wptPacket1 = new D109_Wpt_Type();
					wptPacket1.pid = pid;

					wptPacket1.ident = name;
					wptPacket1.posn = new Semicircle_Type(wpt.Location);
					wptPacket1.comment = desc;
					wptPacket1.smbl = smbl;
					packetToSend = wptPacket1;
				}
					break;

				case 150:
				{
					D150_Wpt_Type wptPacket = new D150_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.alt = (int)wpt.Location.Elev;
					wptPacket.comment = desc;
					packetToSend = wptPacket;
				}
					break;

				case 151:
				{
					D151_Wpt_Type wptPacket = new D151_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.alt = (int)wpt.Location.Elev;
					wptPacket.comment = desc;
					packetToSend = wptPacket;
				}
					break;

				case 152:
				{
					D152_Wpt_Type wptPacket = new D152_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.alt = (int)wpt.Location.Elev;
					wptPacket.comment = desc;
					packetToSend = wptPacket;
				}
					break;

				case 154:
				{
					D154_Wpt_Type wptPacket = new D154_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.alt = (int)wpt.Location.Elev;
					wptPacket.comment = desc;
					wptPacket.smbl = smbl;
					packetToSend = wptPacket;
				}
					break;

				case 155:
				{
					D155_Wpt_Type wptPacket = new D155_Wpt_Type();
					wptPacket.pid = pid;

					wptPacket.ident = name;
					wptPacket.posn = new Semicircle_Type(wpt.Location);
					wptPacket.alt = (int)wpt.Location.Elev;
					wptPacket.comment = desc;
					wptPacket.smbl = smbl;
					packetToSend = wptPacket;
				}
					break;
					#endregion
			}
			linkLayer.SendPacketWaitAck(packetToSend);
		}

		public void PutWaypoints(ArrayList waypoints, GpsProgressHandler progressCallback)
		{
			int count = 0;
			int toSend = waypoints.Count;
			int maxErrors = 5;
			int errors = maxErrors;

			L001Records_Type recordsPacket = new L001Records_Type(toSend);

			m_linkLayer.SendPacketWaitAck(recordsPacket);

			foreach(Waypoint wpt in waypoints)
			{
				// send toSend Pid_Wpt_Data packets, followed by Pid_Xfer_Cmplt
				try 
				{
					sendWaypoint(m_linkLayer, wpt, DType, false);
					count++;
					if(count % 10 == 0)
					{
						progressCallback("IP: downloading to GPS - waypoint ", count, toSend);
					}
				}
				catch (Exception e)
				{
					LibSys.StatusBar.Error("PutWaypoints: " + e.Message + "  count=" + count);
					if(errors-- > 0)
					{
						// on timeout, try continue sending and bail out after too many failures
						continue;
					}
					else
					{
						LibSys.StatusBar.Error("PutWaypoints: too many errors, stopping transfer");
						throw(e);
					}
				}
			}

			m_linkLayer.SendPacket(new L001Xfer_Cmplt(A010Commands.Cmnd_Transfer_Wpt));

			progressCallback("OK: downloaded to GPS - waypoints", count, toSend);

			LibSys.StatusBar.Trace("OK: downloaded to GPS - waypoints: " + count);
		}
	}

	// ========================= Track Log Transfer Protocols (A300, A301) =========================

	public class TrackLogTransferProtocol
	{
		protected GarminProtoLinkLayer m_linkLayer;
		protected string m_useProto;
		protected int DType = 300;

		public TrackLogTransferProtocol(GarminProtoLinkLayer linkLayer, string useProto)
		{
			m_linkLayer = linkLayer;
			m_useProto = useProto;

			// figure out what data packets we will be dealing with:
			// GPS5 supports A301 D310 D301
			// Vista has     A301 D310 D301
			ArrayList datatypes = linkLayer.ProductType.supportsData(m_useProto);
			foreach(string str in datatypes) 
			{
				int dType = Convert.ToInt32(str.Substring(1));
				if(dType >= 300 && dType < 310)
				{
					DType = dType;
				}
			}

			LibSys.StatusBar.Trace("TrackLogTransferProtocol " + m_useProto + " D310 D" + DType);
		}

		public void GetTracks(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			string source = "GRM-GPS-" + DateTime.Now;

			string trackName = "GRM-TRK-" + (Project.trackId + 1);
			int count = 0;
			int toReceive = 0;
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;
			GPacketReceived received = null;
			bool makeTrack = true;

			m_linkLayer.SendPacket(new GPacketA010Commands(A010Commands.Cmnd_Transfer_Trk));
			// ACK may come and will be ignored in receive loop.

			while(!finished)
			{
				// receive many Pid_Trk_Data packets, followed by Pid_Xfer_Cmplt
				try 
				{
					received = m_linkLayer.ReceivePacket();
					if(!received.isGood)
					{
						LibSys.StatusBar.Error("GetTracks: bad packet received, count=" + count);
						continue;
					}
				}
				catch (Exception e)
				{
					LibSys.StatusBar.Error("GetTracks: " + e.Message + "  count=" + count);
					if(errors-- > 0)
					{
						// on timeout, try to NAK the last packet we received
						if(received != null)
						{
							try 
							{
								m_linkLayer.ReOpen();
								m_linkLayer.SendNak((byte)received.pid);
							} 
							catch (Exception ee)
							{
								LibSys.StatusBar.Error("while sending NAK: " + ee.Message);
								try 
								{
									m_linkLayer.ReOpen();
								} 
								catch (Exception eee)
								{
									LibSys.StatusBar.Error("while reopening: " + eee.Message);
								}
							}
						}
						continue;
					}
					else
					{
						LibSys.StatusBar.Error("GetTracks: too many errors, stopping transfer");
						throw(e);
					}
				}

				switch(received.pid)
				{
					case (int)L001Pids.Pid_Records:
					{
						L001Records_Type recordsPacket = (L001Records_Type)received.fromBytes(0);
						if(recordsPacket != null)
						{
							toReceive = recordsPacket.records;
#if DEBUG
							LibSys.StatusBar.Trace("-- toReceive=" + toReceive);
#endif
							trackName = "GRM-TRK-" + (Project.trackId + 1);
							makeTrack = true;
						}
						errors = maxErrors;
					}
						break;

					case (int)L001Pids.Pid_Trk_Hdr:		// this header won't come in A300 protocol, only data comes in there
					{
						D310_Trk_Hdr_Type hdr = (D310_Trk_Hdr_Type)received.fromBytes(0);
						if(hdr != null)
						{
#if DEBUG
							LibSys.StatusBar.Trace("-- track header=" + hdr.trk_ident);
#endif
							count++;
							errors = maxErrors;

							trackName = (hdr.trk_ident.Length == 0) ? ("GRM-TRK-" + (Project.trackId + 1)) : hdr.trk_ident;

							makeTrack = true;
						}
					}
						break;

					case (int)L001Pids.Pid_Trk_Data:
						
						if(makeTrack)
						{
							// placing track creation here ensures that the missing Hdr packet (in case of A300) will not have adverse effect 

							Project.trackId++;

							// we need to allocate every createInfo here, as they will be buffered in the reader thread:
							CreateInfo createInfo = new CreateInfo();

							createInfo.init("trk");
							createInfo.id = Project.trackId;
							createInfo.name = trackName;
							createInfo.source = source;

							insertWaypoint(createInfo);
							makeTrack = false;
						}

						switch(DType)
						{
							case 300:
							{
								D300_Trk_Point_Type trkpt = (D300_Trk_Point_Type)received.fromBytes(300);

								if(trkpt != null)
								{
									GeoCoord coord = trkpt.posn.toGeoCoord();
									DateTime dt = trkpt.toDateTime();
									//LibSys.StatusBar.Trace("coord=" + coord + "  time=" + dt);

									// we need to allocate every createInfo here, as they will be buffered in the reader thread:
									CreateInfo createInfo = new CreateInfo();

									createInfo.init("trkpt");
									createInfo.id = Project.trackId;					// relate waypoint to track
									createInfo.dateTime = dt;
									createInfo.lat = coord.Lat;
									createInfo.lng = coord.Lng;
									createInfo.elev = coord.Elev;
									createInfo.source = source;

									insertWaypoint(createInfo);
									count++;
									if((count % 20) == 0 || (toReceive > 2 && count > toReceive - 2))
									{
										progressCallback("IP: uploading to PC - track: " + trackName + "\n\nleg ", count, toReceive);
									}
									errors = maxErrors;
								}
							}
								break;

							case 301:
							{
								D301_Trk_Point_Type trkpt = (D301_Trk_Point_Type)received.fromBytes(301);

								if(trkpt != null)
								{
									GeoCoord coord = trkpt.posn.toGeoCoord();
									if(trkpt.alt < 100000.0d && trkpt.alt > -12000.0d)
									{
										coord.Elev = trkpt.alt;
									}
									DateTime dt = trkpt.toDateTime();
									//LibSys.StatusBar.Trace("coord=" + coord + "  time=" + dt);

									// we need to allocate every createInfo here, as they will be buffered in the reader thread:
									CreateInfo createInfo = new CreateInfo();

									createInfo.init("trkpt");
									createInfo.id = Project.trackId;			// relate waypoint to track
									createInfo.dateTime = dt;
									createInfo.lat = coord.Lat;
									createInfo.lng = coord.Lng;
									createInfo.elev = coord.Elev;
									createInfo.source = source;

									insertWaypoint(createInfo);
									count++;
									if((count % 20) == 0 || (toReceive > 2 && count > toReceive - 2))
									{
										progressCallback("IP: uploading to PC - track " + trackName + "\n\nleg ", count, toReceive);
									}
									errors = maxErrors;
								}
							}
								break;
						}
						break;

					case (int)L001Pids.Pid_Xfer_Cmplt:
					{
						progressCallback("OK: uploaded to PC - trackpoint", count, toReceive);
#if DEBUG
						LibSys.StatusBar.Trace("-- Xfer_Cmplt");
#endif
						finished = true;
						errors = maxErrors;
					}
						break;

					default:
						LibSys.StatusBar.Trace("-- rx pid=" + received.pid + " (ignored)");
						break;
				}
			}
			LibSys.StatusBar.Trace("OK: uploaded to PC - trackpoints: " + count + " track: " + Project.trackId);
		}
	}

	// ========================= Route Transfer Protocols (A200, A201) =========================

	public class RouteTransferProtocol
	{
		protected GarminProtoLinkLayer m_linkLayer;
		protected string m_useProto;

		protected int HType = 202;		// Header data type, D200_Rte_Hdr_Type, D201_Rte_Hdr_Type, D202_Rte_Hdr_Type 
		protected int DType = 108;
		protected int LType = -1;		// stays -1 to indicate not to send link data, or turns 210 - link data type, D210_Rte_Link_Type 

		// fake timestamps to keep route points sorted in Track class:
		private static int m_day = 2;
		private static int m_month = 1;
		private static int m_year = 8888;
		private static DateTime m_dt = new DateTime(m_year, m_month, m_day, 0, 0, 0, 0);		// to grow with route points

		public RouteTransferProtocol(GarminProtoLinkLayer linkLayer, string useProto)
		{
			m_linkLayer = linkLayer;
			m_useProto = useProto;

			// figure out what data packets we will be dealing with:
			// GPS5 supports		A201 D202 D109 D210
			// Vista has			A201 D202 D108 D210
			// GPS III has			A200 D201 D104
			// GPS III Pilot has	A200 D201 D155

			ArrayList datatypes = linkLayer.ProductType.supportsData(m_useProto);
			foreach(string str in datatypes) 
			{
				int dType = Convert.ToInt32(str.Substring(1));
				if(dType >= 100 && dType < 200)
				{
					DType = dType;
				}
				else if(dType >= 200 && dType < 210)
				{
					HType = dType;
				}
				else if(dType >= 210 && dType < 300)
				{
					LType = dType;	// if we got here, link data will be sent
				}
			}
			LibSys.StatusBar.Trace("RouteTransferProtocol " + m_useProto + " D" + HType + " D" + DType + (LType==-1?"":(" D" + LType)));
		}

		public static void sendHdr(GarminProtoLinkLayer linkLayer, int HType, int nmbr, string ident)
		{
			GPacket packetToSend = null;

			switch(HType)
			{
				case 200:
				{
					D200_Rte_Hdr_Type hPacket = new D200_Rte_Hdr_Type();
					hPacket.nmbr = nmbr;
					packetToSend = hPacket;
				}
					break;

				case 201:
				{
					D201_Rte_Hdr_Type hPacket = new D201_Rte_Hdr_Type();
					hPacket.nmbr = nmbr;
					hPacket.cmnt = ident;
					packetToSend = hPacket;
				}
					break;

				case 202:
				{
					D202_Rte_Hdr_Type hPacket = new D202_Rte_Hdr_Type();
					hPacket.rte_ident = ident;
					packetToSend = hPacket;
				}
					break;
			}
			if(packetToSend != null)
			{
				linkLayer.SendPacketWaitAck(packetToSend);
			}
		}

		public static void sendLinkData(GarminProtoLinkLayer linkLayer, int LType)
		{
			GPacket packetToSend = null;

			switch(LType)
			{
				case 210:
					D210_Rte_Link_Type lPacket = new D210_Rte_Link_Type();
					lPacket.link_class = D210_Rte_Link_Type_Link_Class.direct;
					packetToSend = lPacket;
					break;
			}
			if(packetToSend != null)
			{
				linkLayer.SendPacketWaitAck(packetToSend);
			}
		}

		public void PutRoutes(ArrayList routes, GpsProgressHandler progressCallback, int startRouteNumber)
		{
			bool doSendLinkData = (LType != -1);

			// for each route we send header, waypoints, optional link data and Pid_Xfer_Cmplt
			int toSend = 2;			// for header and Pid_Xfer_Cmplt
			int toSendWpt = 0;		// for ProgressCallback
			foreach(Track route in routes)
			{
				if(route.isRoute)
				{
					toSend += route.Trackpoints.Count;		// for waypoints
					toSendWpt += route.Trackpoints.Count;

					if(doSendLinkData)
					{
						toSend += route.Trackpoints.Count;		// for link data
					}
				}
			}
			if(toSendWpt == 0)
			{
				progressCallback("Error: no routes to upload ", 0, 0);
				return;
			}

			L001Records_Type recordsPacket = new L001Records_Type(toSend);

			m_linkLayer.SendPacketWaitAck(recordsPacket);

			int count = 0;
			int rteNmbr = startRouteNumber;
			foreach(Track route in routes)
			{
				if(!route.isRoute)
				{
					continue;
				}
				int maxErrors = 5;
				int errors = maxErrors;

				sendHdr(m_linkLayer, HType, rteNmbr, route.Name);

				for(int j=0; j < route.Trackpoints.Count ;j++)
				{
					Waypoint wpt = (Waypoint)route.Trackpoints.GetByIndex(j);
					// send toSend Pid_Wpt_Data packets, followed by Pid_Xfer_Cmplt
					try 
					{
						WaypointTransferProtocol.sendWaypoint(m_linkLayer, wpt, DType, true);
						count++;
						if(doSendLinkData)
						{
							sendLinkData(m_linkLayer, LType);
						}
						if(count % 10 == 0)
						{
							progressCallback("IP: downloading to GPS - waypoint ", count, toSendWpt);
						}
					}
					catch (Exception e)
					{
						LibSys.StatusBar.Error("PutWaypoints: " + e.Message + "  count=" + count);
						if(errors-- > 0)
						{
							// on timeout, try continue sending and bail out after too many failures
							continue;
						}
						else
						{
							LibSys.StatusBar.Error("PutWaypoints: too many errors, stopping transfer");
							throw(e);
						}
					}
				}
				rteNmbr++;
			}

			m_linkLayer.SendPacket(new L001Xfer_Cmplt(A010Commands.Cmnd_Transfer_Wpt));

			progressCallback("OK: downloaded " + routes.Count +  " routes to GPS - waypoints", count, toSendWpt);

			LibSys.StatusBar.Trace("OK: downloaded " + routes.Count +  " routes to GPS - waypoints: " + count);
		}


		public void GetRoutes(GpsInsertWaypoint insertWaypoint, GpsProgressHandler progressCallback)
		{
			string source = "GRM-GPS-" + DateTime.Now;

			string routeName = "ROUTE-" + Project.trackId;
			int count = 0;
			int rteCount = 0;
			int wptCount = 0;
			int toReceive = 0;
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;
			GPacketReceived received = null;

			m_linkLayer.SendPacket(new GPacketA010Commands(A010Commands.Cmnd_Transfer_Rte));
			// ACK may come and will be ignored in receive loop.

			while(!finished)
			{
				// receive many Pid_Rte_Hdr and Pid_Rte_Data and Pid_Rte_Link_Data packets, followed by Pid_Xfer_Cmplt
				try 
				{
					received = m_linkLayer.ReceivePacket();
					if(!received.isGood)
					{
						LibSys.StatusBar.Error("GetRoutes: bad packet received, count=" + count);
						continue;
					}
				}
				catch (Exception e)
				{
					LibSys.StatusBar.Error("GetRoutes: " + e.Message + "  count=" + count);
					if(errors-- > 0)
					{
						// on timeout, try to NAK the last packet we received
						if(received != null)
						{
							try 
							{
								m_linkLayer.ReOpen();
								m_linkLayer.SendNak((byte)received.pid);
							} 
							catch (Exception ee)
							{
								LibSys.StatusBar.Error("while sending NAK: " + ee.Message);
								try 
								{
									m_linkLayer.ReOpen();
								} 
								catch (Exception eee)
								{
									LibSys.StatusBar.Error("while reopening: " + eee.Message);
								}
							}
						}
						continue;
					}
					else
					{
						LibSys.StatusBar.Error("GetRoutes: too many errors, stopping transfer");
						throw(e);
					}
				}

				switch(received.pid)
				{
					case (int)L001Pids.Pid_Records:
					{
						L001Records_Type recordsPacket = (L001Records_Type)received.fromBytes(0);
						if(recordsPacket != null)
						{
							toReceive = recordsPacket.records;
#if DEBUG
							LibSys.StatusBar.Trace("-- toReceive=" + toReceive);
#endif
						}
						errors = maxErrors;
					}
						break;
					case (int)L001Pids.Pid_Rte_Hdr:
					{
						switch(HType)
						{
							case 200:
							{
								D200_Rte_Hdr_Type hdr = (D200_Rte_Hdr_Type)received.fromBytes(HType);
								if(hdr != null)
								{
									//LibSys.StatusBar.Trace("-- D200 route header=" + hdr.rte_ident);
									insertRoute(insertWaypoint, "ROUTE-" + hdr.nmbr, source, out routeName, ref count, ref rteCount);
									errors = maxErrors;
								}
							}
								break;
							case 201:
							{
								D201_Rte_Hdr_Type hdr = (D201_Rte_Hdr_Type)received.fromBytes(HType);
								if(hdr != null)
								{
									//LibSys.StatusBar.Trace("-- D201 route header=" + hdr.rte_ident);
									insertRoute(insertWaypoint, "ROUTE-" + hdr.nmbr + " " + hdr.cmnt, source, out routeName, ref count, ref rteCount);
									errors = maxErrors;
								}
							}
								break;
							case 202:
							{
								D202_Rte_Hdr_Type hdr = (D202_Rte_Hdr_Type)received.fromBytes(HType);
								if(hdr != null)
								{
									//LibSys.StatusBar.Trace("-- D202 route header=" + hdr.rte_ident);
									insertRoute(insertWaypoint, hdr.rte_ident, source, out routeName, ref count, ref rteCount);
									errors = maxErrors;
								}
							}
								break;
						}
					}
						break;
					case (int)L001Pids.Pid_Rte_Wpt_Data:

						// similar code exists in onWptData() (in WaypointsTransferProtocol class)
						switch(DType)
						{
							case 100:
							{
								D100_Wpt_Type rtept = (D100_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

								#region More proto cases....
							case 101:
							{
								D101_Wpt_Type rtept = (D101_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 102:
							{
								D102_Wpt_Type rtept = (D102_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 103:
							{
								D103_Wpt_Type rtept = (D103_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 104:
							{
								D104_Wpt_Type rtept = (D104_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 105:
							{
								D105_Wpt_Type rtept = (D105_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, null, coord, source);
								}
							}
								break;

							case 106:
							{
								D106_Wpt_Type rtept = (D106_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, null, coord, source);
								}
							}
								break;

							case 107:
							{
								D107_Wpt_Type rtept = (D107_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();

									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 108:
							{
								D108_Wpt_Type rtept = (D108_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();
									if(rtept.alt < 100000.0d && rtept.alt > -12000.0d)
									{
										coord.Elev = rtept.alt;
									}
									string desc = (rtept.comment + " " + rtept.facility + " " + rtept.city + " " + rtept.addr + " " + rtept.cross_road).Trim();
									insertRoutePoint(insertWaypoint, "" + rtept.ident, desc, coord, source);
								}
							}
								break;

							case 109:
							{
								D109_Wpt_Type rtept = (D109_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();
									if(rtept.alt < 100000.0d && rtept.alt > -12000.0d)
									{
										coord.Elev = rtept.alt;
									}
									string desc = (rtept.comment + " " + rtept.facility + " " + rtept.city + " " + rtept.addr + " " + rtept.cross_road).Trim();
									insertRoutePoint(insertWaypoint, "" + rtept.ident, desc, coord, source);
								}
							}
								break;

							case 150:
							{
								D150_Wpt_Type rtept = (D150_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();
									if(rtept.alt < 100000.0d && rtept.alt > -12000.0d)
									{
										coord.Elev = rtept.alt;
									}
									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 151:
							{
								D151_Wpt_Type rtept = (D151_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();
									if(rtept.alt < 100000.0d && rtept.alt > -12000.0d)
									{
										coord.Elev = rtept.alt;
									}
									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 152:
							{
								D152_Wpt_Type rtept = (D152_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();
									if(rtept.alt < 100000.0d && rtept.alt > -12000.0d)
									{
										coord.Elev = rtept.alt;
									}
									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 154:
							{
								D154_Wpt_Type rtept = (D154_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();
									if(rtept.alt < 100000.0d && rtept.alt > -12000.0d)
									{
										coord.Elev = rtept.alt;
									}
									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;

							case 155:
							{
								D155_Wpt_Type rtept = (D155_Wpt_Type)received.fromBytes(DType);

								if(rtept != null)
								{
									GeoCoord coord = rtept.posn.toGeoCoord();
									if(rtept.alt < 100000.0d && rtept.alt > -12000.0d)
									{
										coord.Elev = rtept.alt;
									}
									insertRoutePoint(insertWaypoint, "" + rtept.ident, rtept.comment, coord, source);
								}
							}
								break;
								#endregion
						}
						count++;
						wptCount++;
						if((count % 10) == 0 || (toReceive > 2 && count > toReceive - 2))
						{
							progressCallback("IP: uploading to PC - route: " + routeName + "\n\nleg ", count, toReceive);
						}
						errors = maxErrors;
						break;

					case (int)L001Pids.Pid_Rte_Link_Data:
						// we do not process LinkData packets, just ignore them. They come kind of empty on Vista:
						// <98=Pid_Rte_Link_Data> cnt=21 : 3 0 0 0 0 0 0 0 255 255 255 255 255 255 255 255 255 255 255 255 0
						count++;		// it counts against the number of packets received in the Pid_Records
						break;

					case (int)L001Pids.Pid_Xfer_Cmplt:
					{
						progressCallback("OK: uploaded to PC: " + rteCount + " routes   - routepoints", wptCount, wptCount);
#if DEBUG
						LibSys.StatusBar.Trace("-- Xfer_Cmplt");
#endif
						finished = true;
						errors = maxErrors;
					}
						break;

					default:
						LibSys.StatusBar.Trace("-- rx pid=" + received.pid + " (ignored)");
						break;
				}
			}
			LibSys.StatusBar.Trace("OK: uploaded to PC - routepoints: " + wptCount + " route: " + Project.trackId);
		}

		private void insertRoute(GpsInsertWaypoint insertWaypoint, string name, string source, out string routeName, ref int count, ref int rteCount)
		{
			Project.trackId++;

			routeName = (name.Length == 0) ? ("ROUTE-" + Project.trackId) : name;

			// we need to allocate every createInfo here, as they will be buffered in the reader thread:
			CreateInfo createInfo = new CreateInfo();

			createInfo.init("rte");
			createInfo.id = Project.trackId;					// relate waypoint to track
			createInfo.source = source;
			createInfo.name = routeName;

			count++;						// it counts against the number of packets received in the Pid_Records
			rteCount++;

			insertWaypoint(createInfo);		// actually inserts a route
		}

		private char[] digits = "0123456789".ToCharArray();

		private void insertRoutePoint(GpsInsertWaypoint insertWaypoint, string name, string desc, GeoCoord coord, string source)
		{
			m_dt = m_dt.AddSeconds(1);		// grow time
			// weed out the end of names in the form of "23 1" or "23 12" which Garmin adds internally
			int pos = name.LastIndexOf(" ");
			if(pos > 0)
			{
				if(pos == name.Length - 2 && name.LastIndexOfAny(digits) == pos + 1)
				{
					name = name.Substring(0, pos);
				}
				if(pos == name.Length - 3 && name.LastIndexOfAny(digits) == pos + 2)
				{
					name = name.Substring(0, pos);
				}
			}

			//LibSys.StatusBar.Trace("coord=" + coord + "  time=" + m_dt);

			// we need to allocate every createInfo here, as they will be buffered in the reader thread:
			CreateInfo createInfo = new CreateInfo();

			createInfo.init("rtept");
			createInfo.id = Project.trackId;					// relate waypoint to route
			createInfo.dateTime = m_dt;
			createInfo.lat = coord.Lat;
			createInfo.lng = coord.Lng;
			createInfo.elev = coord.Elev;
			createInfo.typeExtra = "rtept";
			createInfo.source = source;
			createInfo.name = name.Trim();
			createInfo.desc = desc;

			insertWaypoint(createInfo);
		}
	}
}
