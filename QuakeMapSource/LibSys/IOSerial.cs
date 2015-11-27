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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml.Serialization;

namespace LibSys
{
	/// <summary>
	/// Lowest level Com driver handling all Win32 API calls and processing send and receive in terms of
	/// individual bytes. Used as a base class for higher level drivers.
	/// </summary>
	public abstract class CommBase : IDisposable
	{
		private IntPtr hPort;
		private IntPtr ptrUWO = IntPtr.Zero;
		private Thread rxThread = null;
		private bool online = false;
		private bool auto = false;
		private bool checkSends = true;
		private Exception rxException = null;
		private bool rxExceptionReported = false;
		private int writeCount = 0;
		private ManualResetEvent writeEvent = new ManualResetEvent(false);
		//JH 1.2: Added below to improve robustness of thread start-up.
		private ManualResetEvent startEvent = new ManualResetEvent(false);
		private int stateRTS = 2;
		private int stateDTR = 2;
		private int stateBRK = 2;
		
		/// <summary>
		/// Opens the com port and configures it with the required settings
		/// </summary>
		/// <returns>false if the port could not be opened</returns>
		public virtual bool Open() 
		{
			Win32Com.DCB PortDCB = new Win32Com.DCB();
			Win32Com.COMMTIMEOUTS CommTimeouts = new Win32Com.COMMTIMEOUTS();
			CommBaseSettings cs;
			Win32Com.OVERLAPPED wo = new Win32Com.OVERLAPPED();
			Win32Com.COMMPROP cp;

			if (online) return false;
			cs = CommSettings();

			hPort = Win32Com.CreateFile(cs.port, Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
				Win32Com.OPEN_EXISTING, Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
			if (hPort == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
			{
				if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
				{
					return false;
				}
				else
				{
					throw new CommPortException("Port Open Failure");
				}
			}

			online = true;

			//JH1.1: Changed from 0 to "magic number" to give instant return on ReadFile:
			CommTimeouts.ReadIntervalTimeout = Win32Com.MAXDWORD;
			CommTimeouts.ReadTotalTimeoutConstant = 0;
			CommTimeouts.ReadTotalTimeoutMultiplier = 0;

			//JH1.2: 0 does not seem to mean infinite on non-NT platforms, so default it to 10
			//seconds per byte which should be enough for anyone.
			if (cs.sendTimeoutMultiplier == 0)
			{
				if (System.Environment.OSVersion.Platform == System.PlatformID.Win32NT)
				{
					CommTimeouts.WriteTotalTimeoutMultiplier = 0;
				}
				else
				{
					CommTimeouts.WriteTotalTimeoutMultiplier = 10000;
				}
			}
			else
			{
				CommTimeouts.WriteTotalTimeoutMultiplier = cs.sendTimeoutMultiplier;
			}
			CommTimeouts.WriteTotalTimeoutConstant = cs.sendTimeoutConstant;
			
			PortDCB.init(((cs.parity == Parity.odd) || (cs.parity == Parity.even)), cs.txFlowCTS, cs.txFlowDSR,
				(int)cs.useDTR, cs.rxGateDSR, !cs.txWhenRxXoff, cs.txFlowX, cs.rxFlowX, (int)cs.useRTS);
			PortDCB.BaudRate = cs.baudRate;
			PortDCB.ByteSize = (byte)cs.dataBits;
			PortDCB.Parity = (byte)cs.parity;
			PortDCB.StopBits = (byte)cs.stopBits;
			PortDCB.XoffChar = (byte)cs.XoffChar;
			PortDCB.XonChar = (byte)cs.XonChar;
			if ((cs.rxQueue != 0) || (cs.txQueue != 0))
				if (!Win32Com.SetupComm(hPort, (uint)cs.rxQueue, (uint)cs.txQueue)) ThrowException("Bad queue settings");

			//JH 1.2: Defaulting mechanism for handshake thresholds - prevents problems of setting specific
			//defaults which may violate the size of the actually granted queue. If the user specifically sets
			//these values, it's their problem!
			if ((cs.rxLowWater == 0) || (cs.rxHighWater == 0)) 
			{
				if (!Win32Com.GetCommProperties(hPort, out cp))	cp.dwCurrentRxQueue = 0;
				if (cp.dwCurrentRxQueue > 0)
				{
					//If we can determine the queue size, default to 1/10th, 8/10ths, 1/10th.
					//Note that HighWater is measured from top of queue.
					PortDCB.XoffLim = PortDCB.XonLim = (short)((int)cp.dwCurrentRxQueue / 10);
				}
				else
				{
					//If we do not know the queue size, set very low defaults for safety.
					PortDCB.XoffLim = PortDCB.XonLim = 8;
				}
			}
			else
			{
				PortDCB.XoffLim = (short)cs.rxHighWater;
				PortDCB.XonLim = (short)cs.rxLowWater;
			}
			
			if (!Win32Com.SetCommState(hPort, ref PortDCB)) ThrowException("Bad com settings");
			if (!Win32Com.SetCommTimeouts(hPort, ref CommTimeouts)) ThrowException("Bad timeout settings");

			stateBRK = 0;
			if (cs.useDTR == HSOutput.none) stateDTR = 0;
			if (cs.useDTR == HSOutput.online) stateDTR = 1;
			if (cs.useRTS == HSOutput.none) stateRTS = 0;
			if (cs.useRTS == HSOutput.online) stateRTS = 1;

			checkSends = cs.checkAllSends;
			wo.Offset = 0;
			wo.OffsetHigh = 0;
			if (checkSends)
				wo.hEvent = writeEvent.Handle;
			else
				wo.hEvent = IntPtr.Zero;
			ptrUWO = Marshal.AllocHGlobal(Marshal.SizeOf(wo));
			Marshal.StructureToPtr(wo, ptrUWO, true);
			writeCount = 0;
						
			rxException = null;
			rxExceptionReported = false;
			rxThread = new Thread(new ThreadStart(this.ReceiveThread));
			// see Entry.cs for how the current culture is set:
			rxThread.CurrentCulture = Thread.CurrentThread.CurrentCulture; //new CultureInfo("en-US", false);
			rxThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture; //new CultureInfo("en-US", false);
			rxThread.Name = "CommBaseRx";
			rxThread.IsBackground = true;
			rxThread.Priority = ThreadPriority.AboveNormal;
			rxThread.Start();

			//JH1.2: More robust thread start-up wait.
			//Thread.Sleep(1); //Give rx thread time to start. By documentation, 0 should work, but it does not!
			startEvent.WaitOne(500, false);

			auto = false;
			if (AfterOpen()) 
			{
				auto = cs.autoReopen;
				return true;
			}
			else 
			{
				Close();
				return false;
			}
		}

		/// <summary>
		/// Closes the com port.
		/// </summary>
		public void Close()
		{
			if (online)
			{
				auto = false;
				BeforeClose(false);
				InternalClose();
				rxException = null;
			}
		}
		
		private void InternalClose() 
		{
			Win32Com.CancelIo(hPort);
			if (rxThread != null)
			{
				rxThread.Abort();
				rxThread = null;
			}
			Win32Com.CloseHandle(hPort);
			if (ptrUWO != IntPtr.Zero) Marshal.FreeHGlobal(ptrUWO);
			stateRTS = 2;
			stateDTR = 2;
			stateBRK = 2;
			online = false;
		}
		
		/// <summary>
		/// For IDisposable
		/// </summary>
		public virtual void Dispose() {Close();}

		/// <summary>
		/// Destructor (just in case)
		/// </summary>
		~CommBase() {Close();}
		
		/// <summary>
		/// True if online.
		/// </summary>
		public bool Online {get {if (!online) return false; else return CheckOnline();}}

		/// <summary>
		/// Block until all bytes in the queue have been transmitted.
		/// </summary>
		public void Flush() 
		{
			CheckOnline();
			CheckResult();
		}

		/// <summary>
		/// Use this to throw exceptions in derived classes. Correctly handles threading issues
		/// and closes the port if necessary.
		/// </summary>
		/// <param name="reason">Description of fault</param>
		protected void ThrowException(string reason)
		{
			if (Thread.CurrentThread == rxThread)
			{
				throw new CommPortException(reason);
			}
			else
			{
				if (online)
				{
					BeforeClose(true);
					InternalClose();
				}
				if (rxException == null)
				{
					throw new CommPortException(reason);
				}
				else
				{
					throw new CommPortException(rxException);
				}
			}
		}

		/// <summary>
		/// Queues bytes for transmission. 
		/// </summary>
		/// <param name="tosend">Array of bytes to be sent</param>
		protected void Send(byte[] tosend, int writeCnt) 
		{
			// flush whatever is left in the buffer:
			CheckOnline();
			CheckResult();

			// send the new portion:
			uint sent = 0;
			writeCount = writeCnt;
			if (Win32Com.WriteFile(hPort, tosend, (uint)writeCnt, out sent, ptrUWO))
			{
				writeCount -= (int)sent;
			}
			else
			{
				int err = Marshal.GetLastWin32Error();
				if (err != Win32Com.ERROR_IO_PENDING) ThrowException("Unexpected failure - " + err);
			}
		}

		/// <summary>
		/// Queues bytes for transmission. 
		/// </summary>
		/// <param name="tosend">Array of bytes to be sent</param>
		protected void Send(byte[] tosend) 
		{
			uint sent = 0;
			CheckOnline();
			CheckResult();
			writeCount = tosend.GetLength(0);
			if (Win32Com.WriteFile(hPort, tosend, (uint)writeCount, out sent, ptrUWO))
			{
				writeCount -= (int)sent;
			}
			else
			{
				if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING) ThrowException("Unexpected failure - Send()");
			}
		}

		/// <summary>
		/// Queues a single byte for transmission.
		/// </summary>
		/// <param name="tosend">Byte to be sent</param>
		protected void Send(byte tosend) 
		{
			byte[] b = new byte[1];
			b[0] = tosend;
			Send(b);
		}

		private void CheckResult() 
		{
			uint sent = 0;
			if (writeCount > 0)
			{
				if (Win32Com.GetOverlappedResult(hPort, ptrUWO, out sent, checkSends))
				{
					writeCount -= (int)sent;
					if (writeCount != 0) ThrowException("Send Timeout");
				}
				else
				{
					if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING) ThrowException("Unexpected failure");
				}
			}
		}

		/// <summary>
		/// Sends a protocol byte immediately ahead of any queued bytes.
		/// </summary>
		/// <param name="tosend">Byte to send</param>
		/// <returns>False if an immediate byte is already scheduled and not yet sent</returns>
		protected void SendImmediate(byte tosend) 
		{
			CheckOnline();
			if (!Win32Com.TransmitCommChar(hPort, tosend)) ThrowException("Transmission failure");
		}
	
		/// <summary>
		/// Delay processing.
		/// </summary>
		/// <param name="milliseconds">Milliseconds to delay by</param>
		protected void Sleep(int milliseconds)
		{
			Thread.Sleep(milliseconds);
		}

		/// <summary>
		/// Represents the status of the modem control input signals.
		/// </summary>
		public struct ModemStatus 
		{
			private uint status;
			internal ModemStatus(uint val) {status = val;}
			/// <summary>
			/// Condition of the Clear To Send signal.
			/// </summary>
			public bool cts {get{return ((status & Win32Com.MS_CTS_ON) != 0);}}
			/// <summary>
			/// Condition of the Data Set Ready signal.
			/// </summary>
			public bool dsr {get{return ((status & Win32Com.MS_DSR_ON) != 0);}}
			/// <summary>
			/// Condition of the Receive Line Status Detection signal.
			/// </summary>
			public bool rlsd {get{return ((status & Win32Com.MS_RLSD_ON) != 0);}}
			/// <summary>
			/// Condition of the Ring Detection signal.
			/// </summary>
			public bool ring {get{return ((status & Win32Com.MS_RING_ON) != 0);}}
		}

		/// <summary>
		/// Gets the status of the modem control input signals.
		/// </summary>
		/// <returns>Modem status object</returns>
		protected ModemStatus GetModemStatus() 
		{
			uint f;

			CheckOnline();
			if (!Win32Com.GetCommModemStatus(hPort, out f)) ThrowException("Unexpected failure - modem status");
			return new ModemStatus(f);
		}

		/// <summary>
		/// Represents the current condition of the port queues.
		/// </summary>
		public struct QueueStatus 
		{
			private uint status;
			private uint inQueue;
			private uint outQueue;
			private uint inQueueSize;
			private uint outQueueSize;

			internal QueueStatus(uint stat, uint inQ, uint outQ, uint inQs, uint outQs)
			{status = stat; inQueue = inQ; outQueue = outQ; inQueueSize = inQs; outQueueSize = outQs;}
			/// <summary>
			/// Output is blocked by CTS handshaking.
			/// </summary>
			public bool ctsHold {get{return ((status & Win32Com.COMSTAT.fCtsHold) != 0);}}
			/// <summary>
			/// Output is blocked by DRS handshaking.
			/// </summary>
			public bool dsrHold {get{return ((status & Win32Com.COMSTAT.fDsrHold) != 0);}}
			/// <summary>
			/// Output is blocked by RLSD handshaking.
			/// </summary>
			public bool rlsdHold {get{return ((status & Win32Com.COMSTAT.fRlsdHold) != 0);}}
			/// <summary>
			/// Output is blocked because software handshaking is enabled and XOFF was received.
			/// </summary>
			public bool xoffHold {get{return ((status & Win32Com.COMSTAT.fXoffHold) != 0);}}
			/// <summary>
			/// Output was blocked because XOFF was sent and this station is not yet ready to receive.
			/// </summary>
			public bool xoffSent {get{return ((status & Win32Com.COMSTAT.fXoffSent) != 0);}}
			
			/// <summary>
			/// There is a character waiting for transmission in the immediate buffer.
			/// </summary>
			public bool immediateWaiting {get{return ((status & Win32Com.COMSTAT.fTxim) != 0);}}

			/// <summary>
			/// Number of bytes waiting in the input queue.
			/// </summary>
			public long InQueue {get{return (long)inQueue;}}
			/// <summary>
			/// Number of bytes waiting for transmission.
			/// </summary>
			public long OutQueue {get{return (long)outQueue;}}
			/// <summary>
			/// Total size of input queue (0 means information unavailable)
			/// </summary>
			public long InQueueSize {get{return (long)inQueueSize;}}
			/// <summary>
			/// Total size of output queue (0 means information unavailable)
			/// </summary>
			public long OutQueueSize {get{return (long)outQueueSize;}}

			public override string ToString()
			{
				StringBuilder m = new StringBuilder("The reception queue is ", 60);
				if (inQueueSize == 0)
				{
					m.Append("of unknown size and ");
				}
				else
				{
					m.Append(inQueueSize.ToString() + " bytes long and ");
				}
				if (inQueue == 0)
				{
					m.Append("is empty.");
				}
				else if (inQueue == 1)
				{
					m.Append("contains 1 byte.");
				}
				else
				{
					m.Append("contains ");
					m.Append(inQueue.ToString());
					m.Append(" bytes.");
				}
				m.Append(" The transmission queue is ");
				if (outQueueSize == 0)
				{
					m.Append("of unknown size and ");
				}
				else
				{
					m.Append(outQueueSize.ToString() + " bytes long and ");
				}
				if (outQueue == 0)
				{
					m.Append("is empty");
				}
				else if (outQueue == 1)
				{
					m.Append("contains 1 byte. It is ");
				}
				else
				{
					m.Append("contains ");
					m.Append(outQueue.ToString());
					m.Append(" bytes. It is ");
				}
				if (outQueue > 0)
				{
					if (ctsHold || dsrHold || rlsdHold || xoffHold || xoffSent)
					{
						m.Append("holding on");
						if (ctsHold) m.Append(" CTS");
						if (dsrHold) m.Append(" DSR");
						if (rlsdHold) m.Append(" RLSD");
						if (xoffHold) m.Append(" Rx XOff");
						if (xoffSent) m.Append(" Tx XOff");
					}
					else
					{
						m.Append("pumping data");
					}
				}
				m.Append(". The immediate buffer is ");
				if (immediateWaiting)
					m.Append("full.");
				else
					m.Append("empty.");
				return m.ToString();
			}
		}

		/// <summary>
		/// Get the status of the queues
		/// </summary>
		/// <returns>Queue status object</returns>
		protected QueueStatus GetQueueStatus() 
		{
			Win32Com.COMSTAT cs;
			Win32Com.COMMPROP cp;
			uint er;

			CheckOnline();
			if (!Win32Com.ClearCommError(hPort, out er, out cs)) ThrowException("Unexpected failure - clear comm error");
			if (!Win32Com.GetCommProperties(hPort, out cp)) ThrowException("Unexpected failure - get comm props");
			return new QueueStatus(cs.Flags, cs.cbInQue, cs.cbOutQue, cp.dwCurrentRxQueue, cp.dwCurrentTxQueue);
		}

		/// <summary>
		/// True if the RTS pin is controllable via the RTS property
		/// </summary>
		protected bool RTSavailable { get { return (stateRTS < 2);}}
		
		/// <summary>
		/// Set the state of the RTS modem control output
		/// </summary>
		protected bool RTS 
		{
			set 
			{
				if (stateRTS > 1) return;
				CheckOnline();
				if (value)
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETRTS))
						stateRTS = 1;
					else
						ThrowException("Unexpected Failure - escape comm SETRTS");
				}
				else
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRRTS))
						stateRTS = 1;
					else
						ThrowException("Unexpected Failure - escape comm CLRRTS");
				}
			}
			get 
			{
				return (stateRTS == 1);
			}
		}

		/// <summary>
		/// True if the DTR pin is controllable via the DTR property
		/// </summary>
		protected bool DTRavailable { get { return (stateDTR < 2);}}
		
		/// <summary>
		/// The state of the DTR modem control output
		/// </summary>
		protected bool DTR 
		{
			set 
			{
				if (stateDTR > 1) return;
				CheckOnline();
				if (value)
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETDTR))
						stateDTR = 1;
					else
						ThrowException("Unexpected Failure - escape comm SETDTR");
				}
				else
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRDTR))
						stateDTR = 0;
					else
						ThrowException("Unexpected Failure - escape comm CLRDTR");
				}
			}
			get 
			{
				return (stateDTR == 1);
			}
		}

		/// <summary>
		/// Assert or remove a break condition from the transmission line
		/// </summary>
		protected bool Break 
		{
			set 
			{
				if (stateBRK > 1) return;
				CheckOnline();
				if (value)
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETBREAK))
						stateBRK = 0;
					else
						ThrowException("Unexpected Failure - escape comm SETBREAK");
				}
				else
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRBREAK))
						stateBRK = 0;
					else
						ThrowException("Unexpected Failure - escape comm CLRBREAK");
				}
			}
			get 
			{
				return (stateBRK == 1);
			}
		}

		/// <summary>
		/// Override this to provide settings. (NB this is called during Open method)
		/// </summary>
		/// <returns>CommBaseSettings, or derived object with required settings initialised</returns>
		protected virtual CommBaseSettings CommSettings() {return new CommBaseSettings();}

		/// <summary>
		/// Override this to provide processing after the port is openned (i.e. to configure remote
		/// device or just check presence).
		/// </summary>
		/// <returns>false to close the port again</returns>
		protected virtual bool AfterOpen() {return true;}

		/// <summary>
		/// Override this to provide processing prior to port closure.
		/// </summary>
		/// <param name="error">True if closing due to an error</param>
		protected virtual void BeforeClose(bool error) {}
		
		/// <summary>
		/// Override this to process received bytes.
		/// </summary>
		/// <param name="ch">The byte that was received</param>
		protected virtual void OnRxChar(byte ch) {}

		/// <summary>
		/// Override this to take action when transmission is complete (i.e. all bytes have actually
		/// been sent, not just queued).
		/// </summary>
		protected virtual void OnTxDone() {}

		/// <summary>
		/// Override this to take action when a break condition is detected on the input line.
		/// </summary>
		protected virtual void OnBreak() {}

		/// <summary>
		/// Override this to take action when a ring condition is signalled by an attached modem.
		/// </summary>
		protected virtual void OnRing() {}

		/// <summary>
		/// Override this to take action when one or more modem status inputs change state
		/// </summary>
		/// <param name="mask">The status inputs that have changed state</param>
		/// <param name="state">The state of the status inputs</param>
		protected virtual void OnStatusChange(ModemStatus mask, ModemStatus state) {}

		/// <summary>
		/// Override this to take action when the reception thread closes due to an exception being thrown.
		/// </summary>
		/// <param name="e">The exception which was thrown</param>
		protected virtual void OnRxException(Exception e) {}

		private void ReceiveThread() 
		{
			byte[] buf = new Byte[1];
			uint gotbytes;
			bool starting;

			starting = true;
			AutoResetEvent sg = new AutoResetEvent(false);
			Win32Com.OVERLAPPED ov = new Win32Com.OVERLAPPED();
			IntPtr unmanagedOv = Marshal.AllocHGlobal(Marshal.SizeOf(ov));
			ov.Offset = 0; ov.OffsetHigh = 0;
			ov.hEvent = sg.Handle;
			Marshal.StructureToPtr(ov, unmanagedOv, true);

			uint eventMask = 0;
			IntPtr uMask = Marshal.AllocHGlobal(Marshal.SizeOf(eventMask));

			try
			{
				while(true) 
				{
					if (!Win32Com.SetCommMask(hPort, Win32Com.EV_RXCHAR | Win32Com.EV_TXEMPTY | Win32Com.EV_CTS | Win32Com.EV_DSR
						| Win32Com.EV_BREAK | Win32Com.EV_RLSD | Win32Com.EV_RING | Win32Com.EV_ERR))
					{
						throw new CommPortException("IO Error [001]");
					}
					Marshal.WriteInt32(uMask, 0);
					//JH1.2: Tells the main thread that this thread is ready for action.
					if (starting) {startEvent.Set(); starting = false;}
					if (!Win32Com.WaitCommEvent(hPort, uMask, unmanagedOv)) 
					{
						if (Marshal.GetLastWin32Error() == Win32Com.ERROR_IO_PENDING) 
						{
							sg.WaitOne();
						}
						else
						{
							throw new CommPortException("IO Error [002]");
						}
					}
					eventMask = (uint)Marshal.ReadInt32(uMask);
					if ((eventMask & Win32Com.EV_ERR) != 0)
					{
						UInt32 errs;
						if (Win32Com.ClearCommError(hPort, out errs, IntPtr.Zero))
						{
							//JH1.2: BREAK condition has an error flag and and an event flag. Not sure if both
							//are always raised, so if CE_BREAK is only error flag ignore it and set the EV_BREAK
							//flag for normal handling. Also made more robust by handling case were no recognised
							//error was present in the flags. (Thanks to Fred Pittroff for finding this problem!)
							int ec = 0;
							StringBuilder s = new StringBuilder("UART Error: ", 40);
							if ((errs & Win32Com.CE_FRAME) != 0) {s = s.Append("Framing,"); ec++;}
							if ((errs & Win32Com.CE_IOE) != 0) {s = s.Append("IO,"); ec++;}
							if ((errs & Win32Com.CE_OVERRUN) != 0) {s = s.Append("Overrun,"); ec++;}
							if ((errs & Win32Com.CE_RXOVER) != 0) {s = s.Append("Receive Cverflow,"); ec++;}
							if ((errs & Win32Com.CE_RXPARITY) != 0) {s = s.Append("Parity,"); ec++;}
							if ((errs & Win32Com.CE_TXFULL) != 0) {s = s.Append("Transmit Overflow,"); ec++;}
							if (ec > 0)
							{
								s.Length = s.Length - 1;
								throw new CommPortException(s.ToString());
							}
							else
							{
								if (errs == Win32Com.CE_BREAK)
								{
									eventMask |= Win32Com.EV_BREAK;
								}
								else
								{
									throw new CommPortException("IO Error [003]");
								}
							}
						}
						else
						{
							throw new CommPortException("IO Error [003]");
						}
					}
					if ((eventMask & Win32Com.EV_RXCHAR) != 0) 
					{
						do 
						{
							gotbytes = 0;
							if (!Win32Com.ReadFile(hPort, buf, 1, out gotbytes, unmanagedOv)) 
							{
								//JH1.1: Removed ERROR_IO_PENDING handling as comm timeouts have now
								//been set so ReadFile returns immediately. This avoids use of CancelIo
								//which was causing loss of data. Thanks to Daniel Moth for suggesting this
								//might be a problem, and to many others for reporting that it was!
								throw new CommPortException("IO Error [004]");
							}
							if (gotbytes == 1) OnRxChar(buf[0]);
						} while (gotbytes > 0);
					}
					if ((eventMask & Win32Com.EV_TXEMPTY) != 0)
					{
						OnTxDone();
					}
					if ((eventMask & Win32Com.EV_BREAK) != 0) OnBreak();
			
					uint i = 0;
					if ((eventMask & Win32Com.EV_CTS) != 0) i |= Win32Com.MS_CTS_ON;
					if ((eventMask & Win32Com.EV_DSR) != 0) i |= Win32Com.MS_DSR_ON;
					if ((eventMask & Win32Com.EV_RLSD) != 0) i |= Win32Com.MS_RLSD_ON;
					if ((eventMask & Win32Com.EV_RING) != 0) i |= Win32Com.MS_RING_ON;
					if (i != 0)
					{
						uint f;
						if (!Win32Com.GetCommModemStatus(hPort, out f)) throw new CommPortException("IO Error [005]");
						OnStatusChange(new ModemStatus(i), new ModemStatus(f));
					}
				}
			}
			catch (Exception e)
			{
				if (uMask != IntPtr.Zero) Marshal.FreeHGlobal(uMask);
				if (unmanagedOv != IntPtr.Zero) Marshal.FreeHGlobal(unmanagedOv);
				if (!(e is ThreadAbortException))
				{
					rxException = e;
					OnRxException(e);
				}
			}
		}

		private bool CheckOnline()
		{
			if ((rxException != null) && (!rxExceptionReported))
			{
				rxExceptionReported = true;
				ThrowException("rx");
			}
			if (online) 
			{
				//JH1.1: Avoid use of GetHandleInformation for W98 compatability.
				if (hPort != (System.IntPtr)Win32Com.INVALID_HANDLE_VALUE) return true;
				ThrowException("Offline");
				return false;
			}
			else
			{
				if (auto) 
				{
					if (Open()) return true;
				}
				ThrowException("Offline");
				return false;
			}
		}

	}

	/// <summary>
	/// Overlays CommBase to provide line or packet oriented communications to derived classes. ASCII strings
	/// are sent and received and the Transact method is added which transmits a string and then blocks until
	/// a reply string has been received (subject to a timeout).
	/// </summary>
	public abstract class CommLine : CommBase 
	{
		protected byte[] RxBuffer;
		protected uint RxBufferP = 0;
		protected ASCII RxTerm;
		protected ASCII[] TxTerm;
		protected ASCII[] RxFilter;
		protected string RxString = "";
		protected ManualResetEvent TransFlag = new ManualResetEvent(true);
		protected uint TransTimeout;

		public string Receive() 
		{
			TransFlag.Reset();
			if (!TransFlag.WaitOne((int)TransTimeout, false)) ThrowException("Timeout - Receive");
			string s;
			lock(RxString) {s = RxString;}
			return s;
		}
		
		/// <summary>
		/// Queue the ASCII representation of a string and then the set terminator bytes for sending.
		/// </summary>
		/// <param name="toSend">String to be sent.</param>
		public virtual void Send(string toSend) 
		{
			//JH1.1: Use static encoder for efficiency. Thanks to Prof. Dr. Peter Jesorsky!
			Encoding enc = Encoding.ASCII;
			uint l = (uint)enc.GetByteCount(toSend);
			if (TxTerm != null) l += (uint)TxTerm.GetLength(0);
			byte[] b = new byte[l];
			byte[] s = enc.GetBytes(toSend);
			int i;
			for (i = 0; (i <= s.GetUpperBound(0)); i++) b[i] = s[i];
			if (TxTerm != null) for (int j = 0; (j <= TxTerm.GetUpperBound(0)); j++, i++) b[i] = (byte)TxTerm[j];
			Send(b);
		}

		/// <summary>
		/// Transmits the ASCII representation of a string followed by the set terminator bytes and then
		/// awaits a response string.
		/// </summary>
		/// <param name="toSend">The string to be sent.</param>
		/// <returns>The response string.</returns>
		public string Transact(string toSend) 
		{
			Send(toSend);
			TransFlag.Reset();
			if (!TransFlag.WaitOne((int)TransTimeout, false)) ThrowException("Timeout");
			string s;
			lock(RxString) {s = RxString;}
			return s;
		}
		
		/// <summary>
		/// If a derived class overrides ComSettings(), it must call this prior to returning the settings to
		/// the base class.
		/// </summary>
		/// <param name="s">Class containing the appropriate settings.</param>
		protected void Setup(CommLineSettings s) 
		{
			RxBuffer = new byte[s.rxStringBufferSize];
			RxTerm = s.rxTerminator;
			RxFilter = s.rxFilter;
			TransTimeout = (uint)s.transactTimeout;
			TxTerm = s.txTerminator;
		}

		/// <summary>
		/// Override this to process unsolicited input lines (not a result of Transact).
		/// </summary>
		/// <param name="s">String containing the received ASCII text.</param>
		protected virtual void OnRxLine(string s) {}

		protected override void OnRxChar(byte ch) 
		{
			ASCII ca = (ASCII)ch;
			if ((ca == RxTerm) || (RxBufferP > RxBuffer.GetUpperBound(0))) 
			{
				//JH1.1: Use static encoder for efficiency. Thanks to Prof. Dr. Peter Jesorsky!
				Encoding enc = Encoding.ASCII;
				lock(RxString) {RxString = enc.GetString(RxBuffer, 0, (int)RxBufferP);}
				RxBufferP = 0;
				if (TransFlag.WaitOne(0,false)) 
				{
					OnRxLine(RxString);
				} 
				else 
				{
					TransFlag.Set();
				}
			} 
			else 
			{
				bool wr = true;
				if (RxFilter != null) 
				{
					for (int i=0; i <= RxFilter.GetUpperBound(0); i++) if (RxFilter[i] == ca) wr = false;
				}
				if (wr) 
				{
					RxBuffer[RxBufferP] = ch;
					RxBufferP++;
				}
			}
		}
	}

	/// <summary>
	/// Exception used for all errors.
	/// </summary>
	public class CommPortException : ApplicationException
	{
		/// <summary>
		/// Constructor for raising direct exceptions
		/// </summary>
		/// <param name="desc">Description of error</param>
		public CommPortException(string desc) : base(desc) {}

		/// <summary>
		/// Constructor for re-raising exceptions from receive thread
		/// </summary>
		/// <param name="e">Inner exception raised on receive thread</param>
		public CommPortException(Exception e) : base("Receive Thread Exception", e) {}
	}

	internal class Win32Com 
	{

		/// <summary>
		/// Opening Testing and Closing the Port Handle.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError=true)]
		internal static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
			IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		//Constants for errors:
		internal const UInt32 ERROR_FILE_NOT_FOUND = 2;
		internal const UInt32 ERROR_INVALID_NAME = 123;
		internal const UInt32 ERROR_ACCESS_DENIED = 5;
		internal const UInt32 ERROR_IO_PENDING = 997;

		//Constants for return value:
		internal const Int32 INVALID_HANDLE_VALUE = -1;

		//Constants for dwFlagsAndAttributes:
		internal const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;

		//Constants for dwCreationDisposition:
		internal const UInt32 OPEN_EXISTING = 3;

		//Constants for dwDesiredAccess:
		internal const UInt32 GENERIC_READ = 0x80000000;
		internal const UInt32 GENERIC_WRITE = 0x40000000;

		[DllImport("kernel32.dll")]
		internal static extern Boolean CloseHandle(IntPtr hObject);

		/// <summary>
		/// Manipulating the communications settings.
		/// </summary>

		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommState(IntPtr hFile, ref DCB lpDCB);

		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommTimeouts(IntPtr hFile, out COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		internal static extern Boolean BuildCommDCBAndTimeouts(String lpDef, ref DCB lpDCB, ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCommState(IntPtr hFile, [In] ref DCB lpDCB);

		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCommTimeouts(IntPtr hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		internal static extern Boolean SetupComm(IntPtr hFile, UInt32 dwInQueue, UInt32 dwOutQueue);

		[StructLayout( LayoutKind.Sequential )] internal struct COMMTIMEOUTS 
		{
			//JH1.1: Changed Int32 to UInt32 to allow setting to MAXDWORD
			internal UInt32 ReadIntervalTimeout;
			internal UInt32 ReadTotalTimeoutMultiplier;
			internal UInt32 ReadTotalTimeoutConstant;
			internal UInt32 WriteTotalTimeoutMultiplier;
			internal UInt32 WriteTotalTimeoutConstant;
		}
		//JH1.1: Added to enable use of "return immediately" timeout.
		internal const UInt32 MAXDWORD = 0xffffffff;


		[StructLayout( LayoutKind.Sequential )] internal struct DCB 
		{
			internal Int32 DCBlength;
			internal Int32 BaudRate;
			internal Int32 PackedValues;
			internal Int16 wReserved;
			internal Int16 XonLim;
			internal Int16 XoffLim;
			internal Byte  ByteSize;
			internal Byte  Parity;
			internal Byte  StopBits;
			internal Byte XonChar;
			internal Byte XoffChar;
			internal Byte ErrorChar;
			internal Byte EofChar;
			internal Byte EvtChar;
			internal Int16 wReserved1;

			internal void init(bool parity, bool outCTS, bool outDSR, int dtr, bool inDSR, bool txc, bool xOut,
				bool xIn, int rts)
			{
				DCBlength = 28; PackedValues = 0x8001;
				if (parity) PackedValues |= 0x0002;
				if (outCTS) PackedValues |= 0x0004;
				if (outDSR) PackedValues |= 0x0008;
				PackedValues |= ((dtr & 0x0003) << 4);
				if (inDSR) PackedValues |= 0x0040;
				if (txc) PackedValues |= 0x0080;
				if (xOut) PackedValues |= 0x0100;
				if (xIn) PackedValues |= 0x0200;
				PackedValues |= ((rts & 0x0003) << 12);

			}
		}

		/// <summary>
		/// Reading and writing.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError=true)]
		internal static extern Boolean WriteFile(IntPtr fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
			out UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

		[StructLayout( LayoutKind.Sequential )] internal struct OVERLAPPED 
		{
			internal UIntPtr Internal;
			internal UIntPtr InternalHigh;
			internal UInt32 Offset;
			internal UInt32 OffsetHigh;
			internal IntPtr hEvent;
		}

		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCommMask(IntPtr hFile, UInt32 dwEvtMask);

		// Constants for dwEvtMask:
		internal const UInt32 EV_RXCHAR = 0x0001;
		internal const UInt32 EV_RXFLAG = 0x0002;
		internal const UInt32 EV_TXEMPTY = 0x0004;
		internal const UInt32 EV_CTS = 0x0008;
		internal const UInt32 EV_DSR = 0x0010;
		internal const UInt32 EV_RLSD = 0x0020;
		internal const UInt32 EV_BREAK = 0x0040;
		internal const UInt32 EV_ERR = 0x0080;
		internal const UInt32 EV_RING = 0x0100;
		internal const UInt32 EV_PERR = 0x0200;
		internal const UInt32 EV_RX80FULL = 0x0400;
		internal const UInt32 EV_EVENT1 = 0x0800;
		internal const UInt32 EV_EVENT2 = 0x1000;

		[DllImport("kernel32.dll", SetLastError=true)]
		internal static extern Boolean WaitCommEvent(IntPtr hFile, IntPtr lpEvtMask, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		internal static extern Boolean CancelIo(IntPtr hFile);
		
		[DllImport("kernel32.dll", SetLastError=true)]
		internal static extern Boolean ReadFile(IntPtr hFile, [Out] Byte[] lpBuffer, UInt32 nNumberOfBytesToRead,
			out UInt32 nNumberOfBytesRead, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		internal static extern Boolean TransmitCommChar(IntPtr hFile, Byte cChar);

		/// <summary>
		/// Control port functions.
		/// </summary>
		[DllImport("kernel32.dll")]
		internal static extern Boolean EscapeCommFunction(IntPtr hFile, UInt32 dwFunc);

		// Constants for dwFunc:
		internal const UInt32 SETXOFF = 1;
		internal const UInt32 SETXON = 2;
		internal const UInt32 SETRTS = 3;
		internal const UInt32 CLRRTS = 4;
		internal const UInt32 SETDTR = 5;
		internal const UInt32 CLRDTR = 6;
		internal const UInt32 RESETDEV = 7;
		internal const UInt32 SETBREAK = 8;
		internal const UInt32 CLRBREAK = 9;
		
		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommModemStatus(IntPtr hFile, out UInt32 lpModemStat);

		// Constants for lpModemStat:
		internal const UInt32 MS_CTS_ON = 0x0010;
		internal const UInt32 MS_DSR_ON = 0x0020;
		internal const UInt32 MS_RING_ON = 0x0040;
		internal const UInt32 MS_RLSD_ON = 0x0080;

		/// <summary>
		/// Status Functions.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError=true)]
		internal static extern Boolean GetOverlappedResult(IntPtr hFile, IntPtr lpOverlapped,
			out UInt32 nNumberOfBytesTransferred, Boolean bWait);

		[DllImport("kernel32.dll")]
		internal static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, IntPtr lpStat);
		[DllImport("kernel32.dll")]
		internal static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, out COMSTAT cs);

		//Constants for lpErrors:
		internal const UInt32 CE_RXOVER = 0x0001;
		internal const UInt32 CE_OVERRUN = 0x0002;
		internal const UInt32 CE_RXPARITY = 0x0004;
		internal const UInt32 CE_FRAME = 0x0008;
		internal const UInt32 CE_BREAK = 0x0010;
		internal const UInt32 CE_TXFULL = 0x0100;
		internal const UInt32 CE_PTO = 0x0200;
		internal const UInt32 CE_IOE = 0x0400;
		internal const UInt32 CE_DNS = 0x0800;
		internal const UInt32 CE_OOP = 0x1000;
		internal const UInt32 CE_MODE = 0x8000;

		[StructLayout( LayoutKind.Sequential )] internal struct COMSTAT 
		{
			internal const uint fCtsHold = 0x1;
			internal const uint fDsrHold = 0x2;
			internal const uint fRlsdHold = 0x4;
			internal const uint fXoffHold = 0x8;
			internal const uint fXoffSent = 0x10;
			internal const uint fEof = 0x20;
			internal const uint fTxim = 0x40;
			internal UInt32 Flags;
			internal UInt32 cbInQue;
			internal UInt32 cbOutQue;
		}
		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommProperties(IntPtr hFile, out COMMPROP cp);

		[StructLayout( LayoutKind.Sequential )] internal struct COMMPROP
		{
			internal UInt16 wPacketLength; 
			internal UInt16 wPacketVersion; 
			internal UInt32 dwServiceMask; 
			internal UInt32 dwReserved1; 
			internal UInt32 dwMaxTxQueue; 
			internal UInt32 dwMaxRxQueue; 
			internal UInt32 dwMaxBaud; 
			internal UInt32 dwProvSubType; 
			internal UInt32 dwProvCapabilities; 
			internal UInt32 dwSettableParams; 
			internal UInt32 dwSettableBaud; 
			internal UInt16 wSettableData; 
			internal UInt16 wSettableStopParity; 
			internal UInt32 dwCurrentTxQueue; 
			internal UInt32 dwCurrentRxQueue; 
			internal UInt32 dwProvSpec1; 
			internal UInt32 dwProvSpec2; 
			internal Byte wcProvChar; 
		}
	
	}

}
