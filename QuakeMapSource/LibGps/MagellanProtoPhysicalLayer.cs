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
using System.Threading;
using System.Text;

using LibSys;
using LibFormats;

namespace LibGps
{
	/// <summary>
	/// Summary description for MagellanProtoPhysicalLayer.
	/// </summary>
	public class MagellanProtoPhysicalLayer : CommLine
	{
		// magellan makes short delays when sending lots of data, we need to be patient receiving it:
		public uint transactTimeout		= 2000;		// (ms)
		private bool m_handshake;
		public bool MagellanHandshake {
			get { return m_handshake; }
			set {
				// we don't know if the device is in Handshake state; to avoid unexpected CSM 
				// put it in known HANDOFF state:
				MPacketCmd packet = new MPacketCmd("HANDOFF");
				string str = packet.ToString();
				this.Send(str);
				m_handshake = false;	// do not wait for CSM on Send() below
				if(value)
				{
					packet = new MPacketCmd("HANDON");
					str = packet.ToString();
					this.Send(str);
					m_handshake = true;
				}
			} 
		}

		private bool m_transferMode;
		public bool MagellanTransferMode 
		{
			get { return m_transferMode; }
			set 
			{
				// when handshake is off, TON should not be set.
//				if(!m_handshake)
//				{
//					MPacketCmd packet = new MPacketCmd("TOFF");
//					string str = packet.ToString();
//					this.Send(str);
//					m_transferMode = false;
//				}
//				else

//				if(m_handshake || m_transferMode && !value)
				{
					MPacketCmd packet = new MPacketCmd(value ? "TON" : "TOFF");
					string str = packet.ToString();
					this.Send(str);
					m_transferMode = value;
				}
			} 
		}

		CommLineSettings settings; 
		private int m_errorCount = 0;

		public MagellanProtoPhysicalLayer()
		{
			settings = new CommLineSettings();
			settings.rxStringBufferSize = 1024;
			settings.transactTimeout = (int)transactTimeout;			// (ms)
			settings.rxTerminator = ASCII.LF;

			Setup(settings);	// we use only line-related portion of settings here
			log("MagellanProtoPhysicalLayer() buffer=" + RxBuffer.Length + " timeout=" + transactTimeout);
			base.Open();
		}

		// the base wants the right settings when it comes time to setting the port:
		protected override CommBaseSettings CommSettings()
		{
			return Project.gpsPortSettings;
		}

		public override void Dispose() 
		{
			try
			{
				log("MagellanProtoPhysicalLayer: Dispose()");
				base.Dispose();
			}
			catch {}
		}

		protected override void OnRxException(Exception e)
		{
			logError("MagellanProtoPhysicalLayer: OnRxException() " + e.Message);
		}

		// given a packet body only (no checksum), packs it
		// and sends.
		public override void Send(string packetBodyOut)
		{
            string strToSend = packetBodyOut + NmeaPacket.checksum(packetBodyOut);
			logPacket("sending packet=" + strToSend);
			base.Send(strToSend);
			if(m_handshake)
			{
				try 
				{
					string csResponse = base.Receive();
					log(String.Format("received handshake response: {0}", csResponse));
					// watch for "UNABLE" responses, indicating invalid route linkage (0 length leg) etc:
					if(csResponse.StartsWith("$PMGNCMD,UNABLE"))
					{
						m_errorCount++;
					}
				}
				catch(CommPortException e)
				{
					logError("exception waiting for handshake response: " + e.Message);
					base.Open();
				}
			}
		}

		public void Receive(MPacketReceived packetIn)
		{
			packetIn.isGood = false;
			int tries = 0;
			while(!packetIn.isGood && tries++ <= 3)
			{
				//log("Receive() - waiting... try " + tries);
				packetIn.m_string = base.Receive();
				//log("received: " + packetIn.m_string);
				if(packetIn.m_string.StartsWith("$PMGNCSM"))
				{
					log("received astray CSM: " + packetIn.m_string);
					tries--;
					continue;
				}
				if(m_handshake)
				{
                    string cs = NmeaPacket.checksumReceived(packetIn.m_string);
					MPacketCsm packet = new MPacketCsm(cs);
                    string str = packet.ToString() + NmeaPacket.checksum(packet.ToString());
					log("sending handshake response: " + str);
					base.Send(str);
				}
                if (!NmeaPacket.checksumVerify(packetIn.m_string))
				{
					packetIn.isGood = false;
					if(m_handshake)
					{
						logError("bad checksum, waiting for resent packet");
					}
					else
					{
						logError("bad checksum, returning bad packet");
						break;	// just report the received package, isGood = false
					}
				}
				else
				{
					packetIn.isGood = packetIn.basicParse(packetIn.m_string);
					// packet is good and parsed, so the fields array is filled
				}
			}
			logPacket("received " + (packetIn.isGood?"good":"bad") + " packet=" + packetIn);
		}

		public void Transact(string packetBodyOut, MPacketReceived packetIn)
		{
			//log("Transact --------------------------- started"); 
			packetIn.isGood = false;
			int tries = 0;
			while(!packetIn.isGood && tries++ <= 3)
			{
				if(tries > 1)
				{
					logError("Transact - try " + tries);
				}
				this.Send(packetBodyOut);
				this.Receive(packetIn);
			}
			//log("Transact --------------------------- finished"); 
		}

		public void logPacket(string str)
		{
			if(Project.gpsLogPackets)
			{
				LibSys.StatusBar.Trace(str);
			}
		}

		public void resetErrorCount()
		{
			m_errorCount = 0;
		}

		public int ErrorCount()
		{
			return m_errorCount;
		}

		public void log(string str)
		{
			if(Project.gpsLogProtocol)
			{
				LibSys.StatusBar.Trace(str);
			}
		}

		public void logError(string str)
		{
			if(Project.gpsLogErrors)
			{
				LibSys.StatusBar.Error(str);
			}
		}
	}
}
