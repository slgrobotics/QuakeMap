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
	public class GarminProductType
	{
		public int product_ID;
		/*
		Product Name		ID
		----------------- -------
		GNC 250				52
		GNC 250 XL			64
		GNC 300				33
		GNC 300 XL			98
		GPS 12				77
		GPS 12				87
		GPS 12				96
		GPS 12 XL			77
		GPS 12 XL			96
		GPS 12 XL Chinese	106
		GPS 12 XL Japanese	105
		GPS 120				47
		GPS 120 Chinese		55
		GPS 120 XL			74
		GPS 125 Sounder		61
		GPS 126				95
		GPS 126 Chinese		100
		GPS 128				95
		GPS 128 Chinese		100
		GPS 150				20
		GPS 150 XL			64
		GPS 155				34
		GPS 155 XL			98
		GPS 165				34
		GPS 38				41
		GPS 38 Chinese		56
		GPS 38 Japanese		62
		GPS 40				31
		GPS 40				41
		GPS 40 Chinese		56
		GPS 40 Japanese		62
		GPS 45				31
		GPS 45				41
		GPS 45 Chinese		56
		GPS 45 XL			41
		GPS 48				96
		GPS 50				7
		GPS 55				14
		GPS 55 AVD			15
		GPS 65				18
		GPS 75				13
		GPS 75				23
		GPS 75				42
		GPS 85				25
		GPS 89				39
		GPS 90				45
		GPS 92				112
		GPS 95				24
		GPS 95				35
		GPS 95 AVD			22
		GPS 95 AVD			36
		GPS 95 XL			36
		GPS II				59
		GPS II Plus			73
		GPS II Plus			97
		GPS III				72
		GPS III Pilot		71
		GPSCOM 170			50
		GPSCOM 190			53
		GPSMAP 130			49
		GPSMAP 130 Chinese	76
		GPSMAP 135 Sounder	49
		GPSMAP 175			49
		GPSMAP 195			48
		GPSMAP 205			29
		GPSMAP 205			44
		GPSMAP 210			29
		GPSMAP 215			88
		GPSMAP 220			29
		GPSMAP 225			88
		GPSMAP 230			49
		GPSMAP 230 Chinese	76
		GPSMAP 235 Sounder	49
		GPS V				155
		eTrex				130
		eTrex Vista			169
		*/

		public int software_version;
		public string product_description = "";
		public ArrayList supported_protocols = new ArrayList();  // strings like "A010"
		public string[] descriptions = null;

		public override string ToString()
		{
			string ret = "Device: " + product_ID + "\nFirmware: " + software_version + "\n";
			
			ret += "Description: " + product_description + "\n";
			if(descriptions != null)
			{
				foreach(string s in descriptions)
				{
					ret += "    " + s + "\n";
				}
			}
			ret += "Supported Protocols: " + supported_protocols.Count + "\n";

			return ret;
		}

		// intended for non-D... protocol queries:
		public bool supports(string protocol)
		{
			return supported_protocols.Contains(protocol);
		}

		public ArrayList supportsData(string protocol)		// of strings like "D108"
		{
			ArrayList ret = new ArrayList();
			bool done = false;

			for(int i=0; i < supported_protocols.Count && !done ;i++)
			{
				string prot = (string)supported_protocols[i];
				if(protocol.Equals(prot))
				{
					for(i++; i < supported_protocols.Count && !done ;i++)
					{
						string protD = (string)supported_protocols[i];
						if(protD.StartsWith("D"))
						{
							ret.Add(protD);
						}
						else
						{
							done = true;		// ret contains all data types for this protocol
						}
					}
				}
			}

			return ret;		// may be empty, never null
		}

		/*
		public bool supports(string protocol, string datapacket)
		{
			return supported_protocols.Contains(protocol) && supportsData(protocol).Contains(datapacket);
		}
		*/
	}

	/// <summary>
	/// GarminProtoLinkLayer is responsible for sending/receiving/managing packets.
	/// Actually this is an "upper link" layer, working in terms of packet body,
	/// and "purified" packet classes. See GarminProtoPhysicalLayer for involved send/receive operations
	/// on packets.
	/// </summary>
	public class GarminProtoLinkLayer : IDisposable
	{
		protected GarminProtoPhysicalLayer m_physLayer = null;
		protected GarminProductType m_productType = null;
		public GarminProductType ProductType { get { return m_productType; } }

		protected TrackLogTransferProtocol  m_trackLogTransferProtocol = null;
		protected RouteTransferProtocol     m_routeTransferProtocol = null;
		protected WaypointTransferProtocol  m_waypointTransferProtocol = null;
		protected PvtTransferProtocol       m_pvtTransferProtocol = null;

		public GarminProtoLinkLayer()
		{
			m_physLayer = new GarminProtoPhysicalLayer();
		}

		public TrackLogTransferProtocol getTrackLogTransferProtocol()
		{
			return m_trackLogTransferProtocol;
		}

		public RouteTransferProtocol getRouteTransferProtocol()
		{
			return m_routeTransferProtocol;
		}

		public WaypointTransferProtocol getWaypointTransferProtocol()
		{
			return m_waypointTransferProtocol;
		}

		public PvtTransferProtocol getPvtTransferProtocol()
		{
			return m_pvtTransferProtocol;
		}

		public void ReOpen() 
		{
			log("IP: ReOpen()...");
			m_physLayer.Close();
			//Thread.Sleep(20);
			m_physLayer.Open();
			//log("OK: ReOpen()");
		}

		public void Dispose() 
		{
			m_physLayer.Dispose();
		}

		public GarminProductType IdentifyDeviceType()
		{
			m_productType = null;	// make sure we can't have a stale product data instance,
									// and return null if no positive identification

			GPacketReceived received = Transact(new GPacketBasic(BasicPids.Pid_Product_Rqst));

			/* this works fine too:
			SendPacketWaitAck(new GPacketBasic(BasicPids.Pid_Product_Rqst));
			GPacketReceived received = ReceivePacket();
			*/

			if (received.pid != (int)BasicPids.Pid_Product_Data) 
			{
				throw new GpsException("GPS did not reply with product data - check GPS cable");
			}

			if (received.size < 5) 
			{
				throw new GpsException("Product data packet is too small");
			}

			A000Product_Data_Type productData = (A000Product_Data_Type)received.fromBytes(0);

			m_productType = new GarminProductType();

			m_productType.product_ID = productData.product_ID;
			m_productType.software_version = productData.software_version;
			m_productType.descriptions = new string[productData.strings.Count];
			int i = 0;
			foreach(string s in productData.strings)
			{
				m_productType.descriptions[i++] = s;
				StatusBar.Trace(s);
			}

			// most models send also protocol capabilities (A001) data, wait for those to come.
			// this is what GPS V sends:
			//      P000 L001 A010 T001 A100 D109 A201 D202 D109 D210 A301 D310 D301
			//      A400 D109 A500 D501 A600 D600 A700 D700 A800 D800 A900 A902 A903 A904
			//
			// this is eTrex Vista response:
			//      P000 L001 A010 A100 D108 A201 D202 D108 D210 A301 D310 D301
			//      A500 D501 A600 D600 A700 D700 A800 D800 A900 A902 A903
			try 
			{
				while (true)
				{
					received = ReceivePacket();
					int packetId = received.pid;
					// packet body structure: byte[] { pid, count, ...data... }
					string str = "<" + packetId + "> cnt=" + received.size + " :";
					if(packetId == (int)BasicPids.Pid_Protocol_Array)
					{
						for(int ii=0; ii < received.size ;ii+=3)
						{
							int iProt = received.bytes[ii+1] + received.bytes[ii+2] * 256;
							string sProt = "" + ((char)received.bytes[ii]) + string.Format("{0:D3}", iProt);
							str += " " + sProt;
							m_productType.supported_protocols.Add(sProt);
						}
						StatusBar.Trace("IP: device ID=" + m_productType.product_ID + " " + m_productType.product_description + " Firmware " +
											m_productType.software_version + " received Protocol Capabilities data: " + str);
						break;
					}
				}
			} 
			catch
			{
				// DEBUG:
				//m_productType.product_ID = 73;
				//m_productType.software_version = 200;

				m_productType.supported_protocols.Clear();

				StatusBar.Trace("FYI: legacy device ID=" + m_productType.product_ID + " Firmware " +
					m_productType.software_version + " doesn't send Protocol Capabilities data");

				string legacyProtos = devices.tryUseLegacyDevice(m_productType.product_ID,
																	m_productType.software_version, m_productType.supported_protocols );

				if(m_productType.supported_protocols.Count > 0)
				{
					StatusBar.Trace("OK: reconstructed Protocol Capabilities data: " + legacyProtos);
				}
				else
				{
					StatusBar.Error("unknown legacy device, couldn't reconstruct Protocol Capabilities data");
				}
			}

			if(m_productType.supports("A010") || m_productType.supports("A011"))
			{
				// make sure that whatever transfer has been going on is terminated:
				SendPacketWaitAck(new GPacketA010Commands(A010Commands.Cmnd_Abort_Transfer));
			}

			// prepare all protocols here, as making them on the fly may overrun serial connection:
			if(m_productType.supports("A100"))
			{
				log("FYI: supports A100 protocol for waypoints transfer");
				m_waypointTransferProtocol = new WaypointTransferProtocol(this);
			}

			if(m_productType.supports("A201"))
			{
				log("FYI: will use A201 protocol for transferring routes");
				m_routeTransferProtocol = new RouteTransferProtocol(this, "A201");
			}
			else
			{
				log("FYI: will use A200 protocol for transferring routes");
				m_routeTransferProtocol = new RouteTransferProtocol(this, "A200");
			}

			if(m_productType.supports("A301"))
			{
				log("FYI: will use A301 protocol for uploading tracks");
				m_trackLogTransferProtocol = new TrackLogTransferProtocol(this, "A301");
			}
			else
			{
				log("FYI: will use A300 protocol for uploading tracks");
				m_trackLogTransferProtocol = new TrackLogTransferProtocol(this, "A300");
			}

			// none of the legacy devices implement A800 protocol (PVT data transfer), so we need explicit indication.
			if(m_productType.supports("A800"))
			{
				log("FYI: will use A800 protocol for real time data");
				m_pvtTransferProtocol = new PvtTransferProtocol(this);
			}

			return m_productType;
		}

		public void SendPacket(GPacket packet)
		{
			byte[] bytes = packet.toBytes();
			m_physLayer.Send(bytes);
		}

		public void SendNak(byte pidToNak)
		{
			log("IP: SendNak(" + pidToNak + ")...");
			byte[] rspBody = new byte[] { (byte)BasicPids.Pid_Nak_Byte, 0x1, 0x0 };
			rspBody[2] = pidToNak;
			m_physLayer.Send(rspBody);
			//log("OK: SendNak()");
		}

		public void SendPacketWaitAck(GPacket packet)
		{
			byte[] bytes = packet.toBytes();
			m_physLayer.SendWaitAck(bytes);
		}

		public GPacketReceived ReceivePacket()
		{
			GPacketReceived packetIn = new GPacketReceived();
			m_physLayer.Receive(packetIn);
			return packetIn;
		}

		// send, wait for response and return received packet:
		public GPacketReceived Transact(GPacket packet)
		{
			byte[] bytes = packet.toBytes();
			GPacketReceived packetIn = new GPacketReceived();
			m_physLayer.Transact(bytes, packetIn);
			return packetIn;
		}

		public void logError(string str)
		{
			m_physLayer.logError(str);
		}

		public void log(string str)
		{
			m_physLayer.log(str);
		}
	}
}
