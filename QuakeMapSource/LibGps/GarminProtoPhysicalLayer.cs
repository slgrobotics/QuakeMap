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

using LibSys;

namespace LibGps
{
	public class RxPacket : IDisposable
	{
		public const uint rxBufferSize		= 2048;

		public byte RxPacketId = 0;	
		public byte[] RxBuffer;		// data portion of the packet
		public uint RxBufferP = 0;		// packet pointer
		public int RxDatalength = 0;
		public bool RxBadPacket = false;

		public RxPacket()
		{
			RxBuffer = new byte[rxBufferSize];
		}

		public void Dispose()
		{
		}
	}

	/// <summary>
	/// GarminProtoPhysicalLayer - right on top of CommBase from IOSerial.cs.
	/// actually, this is a "bottom link" layer, responsible for packing/sending/receiving/unpacking packets
	/// and sending ACK/NAK back to the device. It allows the "upper link" layer work in terms of packet body,
	/// and "purified" packet classes, and not to be bothered with particulars of packet exchanging.
	/// </summary>
	public class GarminProtoPhysicalLayer : CommBase
	{
		public uint transactTimeout		= 2000;		// (ms)

		// completely received packet is placed here:
		private RxPacket rxPacket;
		private bool useTwoByteAck = true;

		private ManualResetEvent TransFlag = new ManualResetEvent(true);
		private uint TransTimeout;					// (ms)

		public GarminProtoPhysicalLayer()
		{
			//base.auto = true;
			rxPacket = new RxPacket();
			TransTimeout = transactTimeout;			// (ms)
#if DEBUG
			LibSys.StatusBar.Trace("GarminProtoPhysicalLayer() buffer=" + rxPacket.RxBuffer.Length + " timeout=" + transactTimeout);
#endif
			Open();
		}

		public override bool Open() 
		{
			this.resetReceiveState();
			return base.Open();
		}

		public override void Dispose() 
		{
			try
			{
#if DEBUG
				LibSys.StatusBar.Trace("GarminProtoPhysicalLayer: Dispose()");
#endif
				base.Dispose();
			} 
			catch {}
		}

		// the base wants the right settings when it comes time to setting the port:
		protected override CommBaseSettings CommSettings()
		{
			return Project.gpsPortSettings;
		}

		protected override void OnRxException(Exception e)
		{
			logError("GarminProtoPhysicalLayer: OnRxException() " + e.Message);
		}

		// given a packet body only (no transport codes and no checksum), packs it
		// and sends.
		// packet body structure: byte[] { pid, count, ...data... }
		public new void Send(byte[] packetBody)
		{
			// provide header/footer and duplicate all occurrences of 0x10 in the array:
			byte[] buf = new Byte[520];
			int p = 0;
			buf[p++] = 0x10;

			byte cs = 0;
			foreach(byte b in packetBody)
			{
				cs += b;
				buf[p++] = b;
				if(b == 0x10)
				{
					buf[p++] = 0x10;		// DLE stuffing
				}
			}
			cs = (byte)(256 - cs);
			buf[p++] = cs;
			if(cs == 0x10)
			{
				buf[p++] = 0x10;		// DLE stuffing
			}
			buf[p++] = 0x10;
			buf[p++] = 0x3;
			logOutcoming(packetBody);
			resetReceiveState();
			base.Send(buf, p);
		}

		public void Receive(GPacketReceived packetIn)
		{
			//log("Receive() - waiting...");
			TransFlag.Reset();
			if (!TransFlag.WaitOne((int)TransTimeout, false))
			{
				string str = "Timeout: receive state=" + packetState;
				if(arxPacket != null)
				{
					byte packetId = arxPacket.RxBuffer[0];
					// packet body structure: byte[] { pid, count, ...data... }
					string hdr = arxPacket.RxBufferP > 0 ? ("<" + pidToString(packetId) + ">") : "";
					str += "  received so far: " + hdr + " RxBufferP=" + arxPacket.RxBufferP + " :";
					for(int i=1; i < arxPacket.RxBufferP && i < 20 ;i++)
					{
						str += " " + arxPacket.RxBuffer[i];
					}
				}
				logError(str);

				ThrowException("Timeout - check GPS cable and baud rate");
			}
			lock(rxPacket) 
			{
				packetIn.isGood = !rxPacket.RxBadPacket;
				packetIn.pid = rxPacket.RxPacketId;
				packetIn.size = (int)rxPacket.RxBufferP;
				packetIn.bytes = new byte[packetIn.size];
				Array.Copy(rxPacket.RxBuffer, packetIn.bytes, packetIn.size);
			}
			rxPacket.Dispose();
			//log("Receive() - received good packet - pid=" + packetIn.pid);
		}

		// given a packet body only (no transport codes and no checksum), packs it
		// and sends, waiting for ACK and resending if needed.
		// packet body structure: byte[] { pid, count, ...data... }
		public void SendWaitAck(byte[] packetBody)
		{
			int tries = 0;
			while(tries++ <= 3)
			{
				if(tries > 1)
				{
					logError("SendWaitAck - try " + tries);
				}
				Send(packetBody);
				TransFlag.Reset();
				if (!TransFlag.WaitOne((int)TransTimeout, false))
				{
					logError("SendWaitAck - Timeout waiting for ACK");
					if(tries == 3)
					{
						ThrowException("SendWaitAck waiting for ACK");
					}
					else
					{
						// just send the packet again
						continue;
					}
				}

				// analyze response, should be ACK (for our Send()):
				if(rxPacket.RxPacketId == (int)BasicPids.Pid_Ack_Byte)
				{
					//log("SendWaitAck - got ACK"); 
					break;
				}
				else if(rxPacket.RxPacketId == (int)BasicPids.Pid_Nak_Byte)
				{
					// just send the packet again
					logError("SendWaitAck - got NAK for my packet (id=" + packetBody[0] + ")"); 
					continue;
				}
				else
				{
					logError("SendWaitAck - expected ACK for my packet (id=" + packetBody[0] + ") -- came RxPacketId=" + rxPacket.RxPacketId); 
					// may be we missed ACK, instead next packet came. Bail out and let things sync.
					break;
				}
			}
		}

		public void Transact(byte[] packetBodyOut, GPacketReceived packetIn)
		{
			//log("Transact --------------------------- started"); 
			int tries = 0;
			while(tries++ <= 3)
			{
				if(tries > 1)
				{
					logError("Transact - try " + tries);
				}
				Send(packetBodyOut);
				TransFlag.Reset();
				if (!TransFlag.WaitOne((int)TransTimeout, false))
				{
					logError("Transact - Timeout waiting for ACK"); 
					if(tries == 3)
					{
						ThrowException("Timeout - check GPS cable and baud rate");
					}
					else
					{
						// just send the packet again
						continue;
					}
				}

				// analyze response, hopefully should be ACK (for our Send()):
				if(rxPacket.RxPacketId != (int)BasicPids.Pid_Nak_Byte)
				{
					// if this is ACK, wait for the real packet to come.
					// if not, assume we received the response packet
					if(rxPacket.RxPacketId == (int)BasicPids.Pid_Ack_Byte)
					{
						//log("Transact - got ACK");
						// receive actual response:
						TransFlag.Reset();
						if (!TransFlag.WaitOne((int)TransTimeout, false))
						{
							logError("Transact - Timeout"); 
							if(tries == 3)
							{
								ThrowException("Timeout - check GPS cable and baud rate");
							}
							else
							{
								// just send the packet again
								continue;
							}
						}
					}
					if(rxPacket.RxBadPacket)
					{
						logError("Transact - received bad packet - pid=" + rxPacket.RxPacketId);
						continue;
					}
					else
					{
						//log("Transact - received good packet - pid=" + rxPacket.RxPacketId);
						lock(rxPacket) 
						{
							packetIn.isGood = !rxPacket.RxBadPacket;
							packetIn.size = (int)rxPacket.RxBufferP;
							packetIn.pid = rxPacket.RxPacketId;
							packetIn.bytes = new byte[packetIn.size];
							Array.Copy(rxPacket.RxBuffer, packetIn.bytes, packetIn.size);
						}
						rxPacket.Dispose();
						break;
					}
				}
				else // NAK
				{
					// just send the packet again
					logError("Transact - got NAK for my packet (id=" + pidToString(packetBodyOut[0]) + ")"); 
					continue;
				}
				/*
				else
				{
					RxBufferP = 0;
					logError("Transact - expected ACK for my packet (id=" + packetBodyOut[0] + ") -- came RxPacketId=" + RxPacketId); 
					// may be we missed ACK, instead next packet came.
				}
				*/
			}
			//log("Transact --------------------------- finished"); 
		}

		// picks up unsolicited packets
		protected virtual void OnRxPacket(int pid, byte[] packetBody)
		{
			if(rxPacket.RxBadPacket)
			{
				logError("OnRxPacket() - unsolicited bad packet - pid=" + pid);
			}
			else
			{
				log("OnRxPacket() - unsolicited good packet - pid=" + pid);
			}
		}

		enum PacketState  { outside, pid, datalength, data, chksum, trailer, resync }

		private PacketState packetState = PacketState.outside;
		private bool gotDle = false;	// make sure it is set to false before the next packet reception
		private byte prevCh = 0x0;		// helps to resync
		private int resyncLostCount = 0;

		// packet being received is assembled here:
		public RxPacket arxPacket = null;	

		public void resetReceiveState()
		{
			//packetState = PacketState.outside;
			gotDle = false;	// make sure it is set to false before the next packet reception
			prevCh = 0x0;	// helps to resync
			resyncLostCount = 0;
			//RxBufferP = 0;
		}

		// this function knows about the boundaries of the packets and receives complete packets,
		// sending ACK / NAK as needed.
		protected override void OnRxChar(byte ch) 
		{
			bool prevDle = gotDle;
			gotDle = (ch == 0x10);
			uint uch = (uint)ch;

			/*
			if(gotDle || prevDle)
			{
				log("-- ch: " + ((byte)ch) + " gotDle=" + gotDle + " prevDle=" + prevDle + " p=" + rxPacket.RxBufferP + " state=" + packetState);
			}
			else
			{
				log("ch: " + ((byte)ch));
			}
			*/

			switch(packetState)
			{
				case PacketState.resync:
					//logError("resync -- ch: " + ((byte)ch));
					resyncLostCount++;
					if(prevDle && ch == 0x3)		// wait for opening DLE
					{
						logError("resync --> outside, lost " + resyncLostCount + " chars");
						packetState = PacketState.outside;
						resyncLostCount = 0;
					}
					else if(gotDle && prevCh == 0x3)
					{
						logError("resync --> pid, lost " + resyncLostCount + " chars");
						packetState = PacketState.pid;
						resyncLostCount = 0;
					}
					break;
				case PacketState.outside:
					if(gotDle)		// wait for opening DLE
					{
						packetState = PacketState.pid;
					}
					else
					{
						if(ch != 0x3)
						{
							//logError("-- inbetween: " + ((byte)ch));
						}
						else
						{
							//log("-- inbetween: " + ((byte)ch));
						}
					}
					break;
				case PacketState.pid:
					if(gotDle || ch == 0x3)
					{
						// two DLE's is a wrong place ro resync, go on syncing:
						packetState = PacketState.resync;
						logError("-- two DLE's or 0x3 in pid");
						break;
					}
					arxPacket = new RxPacket();
					arxPacket.RxPacketId = ch;
					//log("-- pid -- RxPacketId: " + arxPacket.RxPacketId);
					packetState = PacketState.datalength;
					break;
				case PacketState.datalength:
					// first DLE in sequence of two is ignored:
					if(gotDle && prevDle || !gotDle)		// DLE stuffing
					{
						if(prevDle && !gotDle)		// single DLE not allowed - resync
						{
							logError("-- datalength ch: " + ((byte)ch) + " after single DLE caused resync");
							packetState = PacketState.resync;
							break;
						}
						arxPacket.RxDatalength = (int)uch;
						//log("-- datalength -- RxDatalength: " + arxPacket.RxDatalength);
						packetState = PacketState.data;
						arxPacket.RxBufferP = 0;
						gotDle = false;	// so that prevDle won't be influenced by two legit DLE's
					}
					break;
				case PacketState.data:
					// first DLE in sequence of two is ignored:
					if(gotDle && prevDle || !gotDle)		// DLE stuffing
					{
						if(prevDle && !gotDle)		// single DLE not allowed - resync
						{
							logError("-- data ch: " + ((byte)ch) + " after single DLE caused resync");
							packetState = PacketState.resync;
							break;
						}
						arxPacket.RxBuffer[arxPacket.RxBufferP++] = ch;
						if(arxPacket.RxBufferP >= arxPacket.RxDatalength)
						{
							packetState = PacketState.chksum;
						}
						gotDle = false;	// so that prevDle won't be influenced by two legit DLE's
					}
					break;
				case PacketState.chksum:
					// first DLE in sequence of two is ignored:
					if(gotDle && prevDle || !gotDle)		// DLE stuffing
					{
						if(prevDle && !gotDle)		// single DLE not allowed - resync
						{
							logError("-- chksum ch: " + ((byte)ch) + " after single DLE caused resync");
							packetState = PacketState.resync;
							break;
						}
						arxPacket.RxBuffer[arxPacket.RxBufferP] = ch;
						packetState = PacketState.trailer;
						gotDle = false;	// so that prevDle won't be influenced by two legit DLE's
					}
					break;
				case PacketState.trailer:
					logIncoming(arxPacket);
					if(arxPacket.RxPacketId != (byte)BasicPids.Pid_Ack_Byte && arxPacket.RxPacketId != (byte)BasicPids.Pid_Nak_Byte)
					{
						// some devices respond with two-byte ACK/NAK and do not understand standard
						// one-byte ACK/NAK. Match our response with whatever they have sent us before:
						byte[] rspBody = useTwoByteAck ? new byte[] { 0x0, 0x2, 0x0, 0x0 }
													   : new byte[] { 0x0, 0x1, 0x0 };
						// incoming ACK/NAK are not responded to, others are:
						byte cs = RxChecksum(arxPacket);
						if(ch != 0x10 || cs != arxPacket.RxBuffer[arxPacket.RxBufferP])
						{
							logError("bad packet received (sending NAK) - ch=" + ((byte)ch) + " RxPacketId=" + arxPacket.RxPacketId + " cs=" + cs + " rec_cs=" + arxPacket.RxBuffer[arxPacket.RxBufferP]);
							arxPacket.RxBufferP = 0;			// buffer data no good
							// prepare to send NAK:
							rspBody[0] = (byte)BasicPids.Pid_Nak_Byte;
							arxPacket.RxBadPacket = true;		// signal bad packet has been received and NAK'ed
							rspBody[2] = arxPacket.RxPacketId;
							Send(rspBody);				// send NAK
						} 
						else
						{
							//log("good packet received (sending ACK) - ch=" + ((byte)ch) + " RxPacketId=" + arxPacket.RxPacketId + " cs=" + cs);
							// prepare to send ACK:
							rspBody[0] = (byte)BasicPids.Pid_Ack_Byte;
							arxPacket.RxBadPacket = false;
							rspBody[2] = arxPacket.RxPacketId;
							Send(rspBody);				// send ACK
						}
					}
					else
					{
						// incoming ACK / NAK are not responded to
						arxPacket.RxBadPacket = false;		// ACK / NAK considered good no matter what.
						useTwoByteAck = (arxPacket.RxDatalength == 2);	// make sure we match their ACK size
					}
					rxPacket = arxPacket;
					arxPacket = null;

					gotDle = false;	// trailer is a real DLE and doesn't participate in DLE stuffing
					resyncLostCount = 0;

					/*
					if (TransFlag.WaitOne(0,false))		// Transact/Receive waiting for result?
					{
						// we are not in Transact, let async OnRxPacket() work:
						if(!RxBadPacket)
						{
							byte[] packetBody;
							lock(RxBuffer)
							{
								packetBody = new Byte[RxBufferP - 1];
								Array.Copy(RxBuffer, packetBody, (int)RxBufferP - 1);
								RxBufferP = 0;
							}
							OnRxPacket(RxPacketId, packetBody);	// good unsolicited packet - call the callback function
							// knowing that ACK or NAK has been already sent
						}
						else
						{
							RxBufferP = 0;
							OnRxPacket(RxPacketId, null);	// bad unsolicited packet - call the callback function
							// knowing that ACK or NAK has been already sent
						}
					} 
					else
					*/ 
					{
						TransFlag.Set();		// we are in Transact or Receive, let it pick the result, good or bad,
												// knowing that ACK or NAK has been already sent
					}

					packetState = PacketState.outside;	// 0x3 will be ignored in search for 0x10
					break;
			}
			prevCh = ch;
		}

		/// <summary>
		///  computes checksum on the Rx buffer
		/// </summary>
		/// <returns>checksum</returns>
		public byte RxChecksum(RxPacket rxPacket)
		{
			byte cs = (byte)(rxPacket.RxPacketId + rxPacket.RxDatalength);
			for(int i=0; i < rxPacket.RxBufferP ;i++)
			{
				cs += rxPacket.RxBuffer[i];
			}
			cs = (byte)(256 - cs);
			return cs;
		}

		public string pidToString(byte pid)
		{
			string sPid = "" + pid;
			if(pid == (byte)BasicPids.Pid_Nak_Byte)
			{
				sPid += "=NAK";
			}
			else if(pid == (byte)BasicPids.Pid_Ack_Byte)
			{
				sPid += "=ACK";
			}
			else if(pid == (byte)BasicPids.Pid_Protocol_Array)
			{
				sPid += "=Protocol_Array";
			}
			else if(pid == (byte)L001Pids.Pid_Xfer_Cmplt)
			{
				sPid += "=Pid_Xfer_Cmplt";
			}
			else if(pid == (byte)BasicPids.Pid_Product_Rqst)
			{
				sPid += "=Product_Rqst";
			}
			else if(pid == (byte)BasicPids.Pid_Product_Data)
			{
				sPid += "=Product_Data";
			}
			else if(pid == (byte)L001Pids.Pid_Command_Data)
			{
				sPid += "=Command_Data";
			}
			else if(pid == (byte)L001Pids.Pid_Rte_Hdr)
			{
				sPid += "=Pid_Rte_Hdr";
			}
			else if(pid == (byte)L001Pids.Pid_Rte_Link_Data)
			{
				sPid += "=Pid_Rte_Link_Data";
			}
			else if(pid == (byte)L001Pids.Pid_Rte_Wpt_Data)
			{
				sPid += "=Pid_Rte_Wpt_Data";
			}
			else if(pid == (byte)L001Pids.Pid_Wpt_Data)
			{
				sPid += "=Pid_Wpt_Data";
			}
			else if(pid == (byte)L001Pids.Pid_Trk_Hdr)
			{
				sPid += "=Pid_Trk_Hdr";
			}
			else if(pid == (byte)L001Pids.Pid_Trk_Data)
			{
				sPid += "=Pid_Trk_Data";
			}
			else if(pid == (byte)L001Pids.Pid_Pvt_Data)
			{
				sPid += "=Pid_Pvt_Data";
			}
			return sPid;
		}

		public void logIncoming(RxPacket rxPacket)
		{
			if(Project.gpsLogPackets)
			{
				string str = "in:   <" + pidToString(rxPacket.RxPacketId) + "> cnt=" + rxPacket.RxDatalength + " :";
				int i;
				for(i=0; i < rxPacket.RxBufferP && i < 100 ;i++)
				{
					str += " " + rxPacket.RxBuffer[i];
				}
				if(i < rxPacket.RxBufferP)
				{
					str += "...";
				}
				LibSys.StatusBar.Trace(str);
			}		
		}

		// packet body structure: byte[] { pid, count, ...data... }
		public void logOutcoming(byte[] packetBody)
		{
			if(Project.gpsLogPackets)
			{
				byte TxPacketId = packetBody[0];
				// packet body structure: byte[] { pid, count, ...data... }
				string str = "out: <" + pidToString(TxPacketId) + "> cnt=" + packetBody[1] + " :";
				int i;
				for(i=2; i < packetBody.Length && i < 100 ;i++)
				{
					str += " " + packetBody[i];
				}
				if(i < packetBody.Length)
				{
					str += "...";
				}
				LibSys.StatusBar.Trace(str);
			}		
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
