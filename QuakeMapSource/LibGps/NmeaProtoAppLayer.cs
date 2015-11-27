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
	/// <summary>
	/// Summary description for NmeaProtoAppLayer.
	/// </summary>
	public class NmeaProtoAppLayer : IDisposable
	{
		protected NmeaProtoLinkLayer m_linkLayer;

		public NmeaProtoAppLayer()
		{
			m_linkLayer = new NmeaProtoLinkLayer();
		}

		public void Dispose() 
		{
			m_linkLayer.Dispose();
		}

		protected static bool m_stopRealTime = false;

		public void StartRealTime(GpsRealTimeHandler realtimeCallback)
		{
			int total = 0;
			int maxErrors = 5;
			int errors = maxErrors;
			bool finished = false;
			m_stopRealTime = false;

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
					m_linkLayer.logError("StartRealTime: " + e.Message + "  count=" + total);
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
				//m_linkLayer.log("" + total + " ===  " + received);
				m_linkLayer.log("StartRealTime: good packet received, count=" + total + "   " + received);
				if(received.header.EndsWith("GGA"))	// NMEA GGA expected for location
				{
					m_linkLayer.log("StartRealTime: " + received.header + " packet received, count=" + total + "   content=" + received);
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
						pvtData.posError = 0;
						pvtData.posErrorH = 0;
						pvtData.posErrorV = 0;
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
								pvtData.fix = 4;	// two dimensional differential
								break;
							case "6":				// estimated
								pvtData.fix = 6;	// estimated
								break;
						}
						pvtData.velocityEast = 0;
						pvtData.velocityNorth = 0;
						pvtData.velocityUp = 0;

						pvtData.comment = "received " + received.header;
						//realtimeCallback(pvtData);

						total++;
						errors = maxErrors;
					} 
					catch(Exception ee)
					{
						m_linkLayer.logError("StartRealTime: " + ee.Message);
					}
				}
				else if(received.header.EndsWith("RMC"))	// NMEA RMC may come
				{
					m_linkLayer.log("StartRealTime: " + received.header + " packet received, count=" + total + "   content=" + received);
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
                        $GPRMC,050821.000,A,3334.5551,N,11739.7705,W,0.00,,251006,,,A*67

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
                        bool status = "A".Equals(pvt.fields[1]);
						if(status)
						{
							double Lat = pvt.parseLat(2);
							double Lng = pvt.parseLng(2);
							double Elev = 0.0d;
							pvtData.location = new GeoCoord(Lng, Lat, Elev);
							//m_linkLayer.log("coord=" + pvtData.location);

							// time component (only time part):
                            pvtData.time = pvt.parseNmeaDateTime(0, 8);	// UTC

							if(!"".Equals(pvt.fields[6]))
							{
								double knots = Convert.ToDouble(pvt.fields[6]);
								pvtData.velocity = knots * Distance.METERS_PER_NAUTICAL_MILE / 3600.0d;
								// calculate velocity vector based on track angle:
								if(!"".Equals(pvt.fields[7]))
								{
									double trackAngleRad = Convert.ToDouble(pvt.fields[7]) * Math.PI / 180.0d;
									pvtData.velocityNorth = pvtData.velocity * Math.Cos(trackAngleRad);
									pvtData.velocityEast = pvtData.velocity * Math.Sin(trackAngleRad);
								}
							}

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
				else if(received.header.EndsWith("GLL"))	// NMEA GLL may come
				{
					m_linkLayer.log("StartRealTime: GLL packet received, count=" + total + "   content=" + received);
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
                        bool status = "A".Equals(pvt.fields[5]);
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
							//realtimeCallback(pvtData);
						}
						total++;
						errors = maxErrors;
					} 
					catch(Exception ee)
					{
						m_linkLayer.logError("StartRealTime: " + ee.Message);
					}
				}
				else if(received.header.EndsWith("GSV"))	// NMEA GSV may come for satellite reception status
				{
					m_linkLayer.log("StartRealTime: GSV packet received, count=" + total + "   content=" + received);
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
				else if(received.header.EndsWith("VTG"))	// NMEA GSA may come
				{
					m_linkLayer.log("StartRealTime: GPVTG packet received, count=" + total + "   content=" + received);
					// $GPVTG,309.62,T,,M,0.13,N,0.2,K*6E
					/*
					    VTG - Course Over Ground and Ground Speed

						Name		Example		Units	Description
						Message ID	$GPVTG				VTG protocol header
						Course		309.62		degrees Measured heading
						Reference	T					True
						Course					degrees	Measured heading
						Reference	M					Magnetic
						Speed		0.13		knots	Measured horizontal speed
						Units		N			Knots
						Speed		0.2			Km/hr	Measured horizontal speed
						Units		K					Kilometers per hour
					 */
				}
				else if(received.header.EndsWith("GSA"))	// NMEA GSA may come
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
				else
				{
					m_linkLayer.log("StartRealTime: good other (unrecognized) packet received, count=" + total + "   content=" + received);
				}
			} // end while()

			m_linkLayer.log("StartRealTime: finished receiving fixes count=" + total);
		}

		public void StopRealTime()
		{
			m_stopRealTime = true;
		}
	}
}
