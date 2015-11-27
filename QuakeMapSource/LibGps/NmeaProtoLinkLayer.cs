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
using System.Text;

using LibSys;
using LibGeo;

namespace LibGps
{
	/// <summary>
	/// Summary description for NmeaProtoLinkLayer.
	/// </summary>
	public class NmeaProtoLinkLayer
	{
		// NMEA link layer uses Magellan physical layer, as NMEA packages are a subset of Magellan's
		// and the bauld rate is also 4800
		protected MagellanProtoPhysicalLayer m_physLayer = null;

		public NmeaProtoLinkLayer()
		{
			m_physLayer = new MagellanProtoPhysicalLayer();
		}

		public void ReOpen() 
		{
			log("IP: ReOpen()...");
			m_physLayer.Close();
			Thread.Sleep(20);
			m_physLayer.Open();
			//log("OK: ReOpen()");
		}

		public void Dispose() 
		{
			m_physLayer.Dispose();
		}

		public void SendPacket(MPacket packet)
		{
			string str = packet.ToString();
			m_physLayer.Send(str);
		}

		public MPacketReceived ReceivePacket()
		{
			MPacketReceived packetIn = new MPacketReceived();
			m_physLayer.Receive(packetIn);
			return packetIn;
		}

		// send, wait for response and return received packet:
		public MPacketReceived Transact(MPacket packet)
		{
			string str = packet.ToString();
			MPacketReceived packetIn = new MPacketReceived();
			m_physLayer.Transact(str, packetIn);
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
