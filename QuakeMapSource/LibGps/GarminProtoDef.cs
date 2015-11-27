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

// see garmin.com for document named garmin_iop_spec.pdf
// http://www.garmin.com/support/commProtocol.html

using Records_Type	= System.Int16;
using Symbol_Type	= System.Int16;
using longword		= System.Int32;
using word			= System.Int16;

namespace LibGps
{
	// any Garmin protocol packet:
	public class GPacket
	{
		protected int m_pid = -1;
		public int pid { get { return m_pid; } set { m_pid = value; } }

		protected int m_size = -1;
		public int size { get { return m_size; } set { m_size = value; } }

		public GPacket()	{}

		public GPacket(int pid)		{ m_pid = pid; }

		// returns "pure body" structure: byte[] { pid, count, ...data... }
		public virtual byte[] toBytes() { return null; }

		#region Conversion of bytes into primitive types and back

		public int fromIntBytes(byte[] bytes, int offset)
		{
			return (int)(bytes[offset] + bytes[offset+1]*256);
		}

		public int fromLongBytes(byte[] bytes, int offset)
		{
			return (int)(bytes[offset] + bytes[offset+1]*256 + bytes[offset+2]*65536 + bytes[offset+3]*16777216);
		}

		public float fromFloatBytes(byte[] bytes, int offset)
		{
			//string trace = "";
			int i;
			ulong lRet = 0;
			for(i=0; i < 4 ;i++)
			{
				ulong tmp = (ulong)bytes[offset + i];
				//trace += string.Format("{0:X2}",tmp) + " ";
				tmp = tmp << (i*8);
				lRet += tmp;
			}
			//LibSys.StatusBar.Trace("FLT: " + trace);
			int sign = (lRet & 0x80000000UL) == 0 ? 1 : -1;
			long exponent = (long)((lRet >> 23) & 0xFFUL);		// 8 bits signed exponent, offset 127
			exponent -= 127L;
			ulong mantissa = lRet & 0x7FFFFFUL;	// 23 bits
			mantissa |= 0x800000UL;	// restore the chopped bit
			double uMant = (double)mantissa * Math.Pow(2.0d, -23.0d);
			float ret = (float)(uMant * Math.Pow(2.0d, (double)exponent) * sign); 

			//trace = string.Format("{0:X8}",lRet);
			//LibSys.StatusBar.Trace("FLT: " + trace + " ret=" + ret + " exp=" + exponent + " mant=" + mantissa);

			return ret;
		}

		public Double fromDoubleBytes(byte[] bytes, int offset)
		{
			return GPacket.fromDoubleBytesS(bytes, offset);
		}

		public static Double fromDoubleBytesS(byte[] bytes, int offset)
		{
			//string trace = "";
			int i;
			ulong lRet = 0;
			for(i=0; i < 8 ;i++)
			{
				ulong tmp = (ulong)bytes[offset + i];
				//trace += string.Format("{0:X2}",tmp) + " ";
				tmp = tmp << (i*8);
				lRet += tmp;
			}
			//LibSys.StatusBar.Trace("DBL: " + trace);
			int sign = (lRet & 0x8000000000000000UL) == 0 ? 1 : -1;
			long exponent = (long)((lRet >> 52) & 0x7FFUL);		// 11 bits signed exponent, offset 1023
			exponent -= 1023L;
			ulong mantissa = lRet & 0xFFFFFFFFFFFFFUL;	// 52 bits
			mantissa |= 0x10000000000000UL;	// restore the chopped bit
			double uMant = (double)mantissa * Math.Pow(2.0d, -52.0d);
 			double ret = uMant * Math.Pow(2.0d, (double)exponent) * sign; 

			//trace = string.Format("{0:X16}",lRet);
			//LibSys.StatusBar.Trace("DBL: " + trace + " ret=" + ret + " exp=" + exponent + " mant=" + mantissa);

			return ret;
		}

		public int toFloatBytes(float f, byte[] bytes, int offset)
		{
			long exponent = 0L;
			bool isNegative = (f < 0.0f);
			f = Math.Abs(f);

			if(f != 0.0f)
			{
				while(f >= 2.0f)
				{
					exponent++;
					f /= 2.0f;
				}
				while(f < 1.0f)
				{
					exponent--;
					f *= 2.0f;
				}
			}
			ulong mantissa = (ulong)(f * Math.Pow(2.0d, 23.0d));	// 23 bits
			mantissa &= 0x7FFFFFUL;	// chop the "always 1" bit
			exponent += 127;		// apply offset to exponent
			ulong lRet = 0;
			if(isNegative) { lRet |= 0x80000000UL; }
			lRet |= mantissa;
			lRet |= ((ulong)exponent << 23);

			bytes[offset]   = (byte)(lRet & 0xFF);
			bytes[offset+1] = (byte)((lRet >> 8) & 0xFF);
			bytes[offset+2] = (byte)((lRet >> 16) & 0xFF);
			bytes[offset+3] = (byte)((lRet >> 24) & 0xFF);
			return 4;
		}

		public int toWordBytes(word w, byte[] bytes, int offset)
		{
			bytes[offset]   = (byte)(w & 0xFF);
			bytes[offset+1] = (byte)((w >> 8) & 0xFF);
			return 2;
		}

		public int toIntBytes(int i, byte[] bytes, int offset)
		{
			bytes[offset]   = (byte)(i & 0xFF);
			bytes[offset+1] = (byte)((i >> 8) & 0xFF);
			return 2;
		}

		public int toLongwordBytes(longword lw, byte[] bytes, int offset)
		{
			bytes[offset]   = (byte)(lw & 0xFF);
			bytes[offset+1] = (byte)((lw >> 8) & 0xFF);
			bytes[offset+2] = (byte)((lw >> 16) & 0xFF);
			bytes[offset+3] = (byte)((lw >> 24) & 0xFF);
			return 4;
		}

		public longword fromTimeBytes(byte[] bytes, int offset)
		{
			return bytes[offset] + bytes[offset+1]*256 + bytes[offset+2]*65536 + bytes[offset+3]*16777216;
		}

		public string fromVarString(byte[] bytes, int offset, out int offsetNext)
		{
			string ret = "";
			char[] chars = new char[bytes.Length - offset];
			int pos = offset;
			int i = 0;
			for(; pos < bytes.Length ;pos++,i++)
			{
				if(bytes[pos] == 0)
				{
					break;
				}
				chars[i] = (char)bytes[pos];
			}
			ret = new String(chars, 0, i);
			offsetNext = pos + 1;
			return ret;
		}

		/// <summary>
		/// maxLength includes terminating 0, so a 20 means 19 chars and 0
		/// </summary>
		/// <param name="str"></param>
		/// <param name="bytes"></param>
		/// <param name="offset"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public int toVarStringBytes(string str, byte[] bytes, int offset, int maxLength)
		{
			char[] chars = str.ToCharArray();
			int cnt = 0;
			foreach(char ch in chars)
			{
				bytes[offset + (cnt++)]   = (byte)ch;
				if(cnt >= maxLength)
				{
					break;
				}
			}
			bytes[offset + (cnt++)]   = 0;
			return cnt;
		}

		public string fromFixedString(byte[] bytes, int offset, int length)
		{
			string ret = "";
			char[] chars = new char[length];
			int pos = offset;
			int i = 0;
			for(; pos < bytes.Length && i < length ;pos++,i++)
			{
				chars[i] = (char)bytes[pos];
			}
			ret = new String(chars, 0, i);
			return ret.Trim();
		}

		public int toFixedStringBytes(string str, byte[] bytes, int offset, int length)
		{
			char[] chars = str.ToCharArray();
			int cnt = 0;
			foreach(char ch in chars)
			{
				bytes[offset + (cnt++)]   = (byte)ch;
				if(cnt > length)
				{
					break;
				}
			}
			while(cnt < length)
			{
				bytes[offset + (cnt++)]   = (byte)' ';
			}
			return length;
		}
		#endregion
	}

	// Basic protocol packet (BasicPids types):
	public class GPacketBasic : GPacket
	{
		public GPacketBasic(BasicPids pid) : base((int)pid)		{}

		public override byte[] toBytes()
		{
			byte[] bytes = new byte[] { 0x0, 0x0 };

			bytes[0] = (byte)pid;

			return bytes;
		}
	}

	// A010 protocol packet (A010Commands types):
	public class GPacketA010Commands : GPacket
	{
		byte[] bytes = new byte[] { 0x0, 0x2, 0x0, 0x0 };

		public GPacketA010Commands(A010Commands pid) : base((int)L001Pids.Pid_Command_Data)
		{
			bytes[0] = (byte)m_pid;
			bytes[2] = (byte)pid;
		}

		public override byte[] toBytes()
		{
			return bytes;
		}
	}

	/// <summary>
	/// when a packet is received, it can be converted to a class packet using fromBytes() factory
	/// in this class
	/// </summary>
	public class GPacketReceived : GPacket
	{
		public byte[] bytes = null;		// will be allocated as the packet is received
		public bool isGood = true;

		public GPacketReceived()
		{
		}

		// toBytes is meaningful for packets being sent, but as this class is boxed into GPacket,
		// we need toBytes to return packet body:
		public override byte[] toBytes()
		{
			return bytes;
		}

		#region fromBytes() - this is a "class packet" factory which converts received packets into usable classes
		// (packet classes defined in GarminProtoDef.cs)

		public GPacket fromBytes(int dataPacketType)
		{
			GPacket ret = null;

			if(bytes == null)
			{
				logError("GPacketReceived:fromBytes() -- bytes==null");
				return ret;
			}

			switch(m_pid)
			{
				case (int)BasicPids.Pid_Product_Data:
				{
					if(bytes.Length < 4)
					{
						logError("GPacketReceived:fromBytes() -- Pid_Product_Data length=" + bytes.Length);
						break;
					}
					A000Product_Data_Type rret = new A000Product_Data_Type();
					rret.product_ID = this.fromIntBytes(bytes, 0);
					rret.software_version = this.fromIntBytes(bytes, 2);
					char[] chars = new char[bytes.Length];
					int chLen = 0;
					for (int i=4; i < bytes.Length ;i++)
					{
						char ch = (char)bytes[i];
						if(ch == '\0')
						{
							rret.strings.Add(new string(chars, 0, chLen));
							chLen = 0;
						}
						else
						{
							chars[chLen++] = ch;
						}
					}
					ret = rret;
				}
					break;
				case (int)L001Pids.Pid_Records:
				{
					L001Records_Type rret = new L001Records_Type();
					if(bytes.Length == 0)
					{
						logError("GPacketReceived:fromBytes() -- L001Records_Type length=0");
						break;
					}
					rret.records = bytes[0];
					if(bytes.Length > 1)
					{
						rret.records += (bytes[1] * 256);
					}
					ret = rret;
				}
					break;
				case (int)L001Pids.Pid_Trk_Hdr:
				{
					D310_Trk_Hdr_Type rret = new D310_Trk_Hdr_Type();
					if(bytes.Length < 2)
					{
						logError("GPacketReceived:fromBytes() -- D310_Trk_Hdr_Type length=" + bytes.Length);
						// bad, but not a big deal
						ret = rret;
						break;
					}
					rret.dspl = bytes[0] != 0;
					rret.color = bytes[1];
					char[] chars = new char[bytes.Length];
					int chLen = 0;
					for (int i=2; i < bytes.Length ;i++)
					{
						char ch = (char)bytes[i];
						if(ch == '\0')
						{
							rret.trk_ident = new string(chars, 0, chLen);
							break;
						}
						else
						{
							chars[chLen++] = ch;
						}
					}
					ret = rret;
				}
					break;
				case (int)L001Pids.Pid_Trk_Data:
				{
					switch(dataPacketType)
					{
						case 301:
							if(bytes.Length < 21)
							{
								logError("GPacketReceived:fromBytes() -- D301_Trk_Point_Type length=" + bytes.Length);
								break;
							}
							D301_Trk_Point_Type rret = new D301_Trk_Point_Type();

							// in:   <34> cnt=24 : 78 224 23 0 14 84 172 121 62 53 24 192 208 231 65 81 89 4 105 0 97 114 101

							//	public Semicircle_Type posn;		// position
							//	public longword time;				// time
							//	public float alt;					// altitude in meters
							//	public float dpth;					// depth in meters
							//	public bool new_trk;				// new track segment?

							rret.posn = new Semicircle_Type(bytes, 0);
							rret.time = rret.fromTimeBytes(bytes, 8);
							/*
							string str = "bytes: ";
							for(int i=0; i < bytes.Length && i < 40 ;i++)
							{
								str += " " + bytes[i];
							}
							LibSys.StatusBar.Trace(str + "  ========= time=" + rret.time);
							*/
							rret.alt = rret.fromFloatBytes(bytes, 12);
							rret.dpth = rret.fromFloatBytes(bytes, 16);
							rret.new_trk = (bytes[20] != 0);
							ret = rret;
							break;
						case 300:
							if(bytes.Length < 13)
							{
								logError("GPacketReceived:fromBytes() -- D300_Trk_Point_Type length=" + bytes.Length);
								break;
							}
							D300_Trk_Point_Type rrret = new D300_Trk_Point_Type();

							// in:   <34> cnt=16 : 78 224 23 0 14 84 172 121 62 53 24 192 208 114 101 (?)

							//	public Semicircle_Type posn;		// position
							//	public longword time;				// time
							//	public bool new_trk;				// new track segment?

							rrret.posn = new Semicircle_Type(bytes, 0);
							rrret.time = rrret.fromTimeBytes(bytes, 8);
							/*
							string str = "bytes: ";
							for(int i=0; i < bytes.Length && i < 40 ;i++)
							{
								str += " " + bytes[i];
							}
							LibSys.StatusBar.Trace(str + "  ========= time=" + rrret.time);
							*/
							rrret.new_trk = (bytes[12] != 0);
							ret = rrret;
							break;
						default:
							break;
					}
				}
					break;
				case (int)L001Pids.Pid_Rte_Hdr:
				{
					switch(dataPacketType)
					{
						case 200:
						{
							D200_Rte_Hdr_Type rret = new D200_Rte_Hdr_Type();
							rret.nmbr = (int)bytes[0];
							ret = rret;
						}
							break;
						case 201:
						{
							D201_Rte_Hdr_Type rret = new D201_Rte_Hdr_Type();
							rret.nmbr = (int)bytes[0];
							if(bytes.Length < 2)
							{
								logError("GPacketReceived:fromBytes() -- D201_Rte_Hdr_Type length=" + bytes.Length);
								// bad, but hopefully not a big deal
								ret = rret;
								break;
							}
							char[] chars = new char[bytes.Length];
							int chLen = 0;
							for (int i=1; i < bytes.Length ;i++)
							{
								char ch = (char)bytes[i];
								if(ch == '\0')
								{
									break;
								}
								else
								{
									chars[chLen++] = ch;
								}
							}
							if(chLen > 0)
							{
								rret.cmnt = new string(chars, 0, chLen);
							}
							ret = rret;
						}
							break;
						case 202:
						{
							D202_Rte_Hdr_Type rret = new D202_Rte_Hdr_Type();
							if(bytes.Length < 2)
							{
								logError("GPacketReceived:fromBytes() -- D202_Rte_Hdr_Type length=" + bytes.Length);
								// bad, but hopefully not a big deal
								ret = rret;
								break;
							}
							char[] chars = new char[bytes.Length];
							int chLen = 0;
							for (int i=0; i < bytes.Length ;i++)
							{
								char ch = (char)bytes[i];
								if(ch == '\0')
								{
									break;
								}
								else
								{
									chars[chLen++] = ch;
								}
							}
							if(chLen > 0)
							{
								rret.rte_ident = new string(chars, 0, chLen);
							}
							ret = rret;
						}
							break;
					}
				}
					break;
				case (int)L001Pids.Pid_Rte_Wpt_Data:
				case (int)L001Pids.Pid_Wpt_Data:
				{
					switch(dataPacketType)
					{
						case 100:
						{
							if(bytes.Length != 6 + 8 + 4 + 40)	// should be 58
							{
								logError("GPacketReceived:fromBytes() -- D100_Wpt_Type length=" + bytes.Length);
								//break;
							}
							D100_Wpt_Type rret = new D100_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
						}
							break;

						case 101:
						{
							if(bytes.Length != 6 + 8 + 4 + 40 + 5)	// should be 63
							{
								logError("GPacketReceived:fromBytes() -- D101_Wpt_Type length=" + bytes.Length);
								//break;
							}
							D101_Wpt_Type rret = new D101_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// float dst;				// 58 proximity distance (meters) 4
							// byte smbl;				// 62 symbol 1

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.dst = rret.fromFloatBytes(bytes, 58);
							rret.smbl = bytes[62];
						}
							break;

						case 102:
						{
							if(bytes.Length != 6 + 8 + 4 + 40 + 6)	// should be 64
							{
								logError("GPacketReceived:fromBytes() -- D102_Wpt_Type length=" + bytes.Length);
								//break;
							}
							D102_Wpt_Type rret = new D102_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// float dst;				// 58 proximity distance (meters) 4
							// Symbol_Type smbl;		// 62 symbol id 2

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.dst = rret.fromFloatBytes(bytes, 58);
							rret.smbl = (short)rret.fromIntBytes(bytes, 62);
						}
							break;

						case 103:
						{
							if(bytes.Length != 8 + 6 + 4 + 40 + 2)	// should be 64
							{
								logError("GPacketReceived:fromBytes() -- D103_Wpt_Type length=" + bytes.Length);
								//break;
							}
							D103_Wpt_Type rret = new D103_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// byte smbl;				// 58 symbol 1
							// byte dspl;				// 59 display option 1

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.smbl = (D103_Wpt_Type_Symbols)bytes[58];
							rret.dspl = (D103_Wpt_Type_Display_Modes)bytes[59];
						}
							break;

						case 104:
						{
							if(bytes.Length != 8 + 6 + 4 + 40 + 7)	// should be 65
							{
								logError("GPacketReceived:fromBytes() -- D104_Wpt_Type length=" + bytes.Length);
								//break;
							}
							D104_Wpt_Type rret = new D104_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// float dst;				// 58 proximity distance (meters) 4
							// Symbol_Type smbl;		// 62 symbol id 2
							// byte dspl;				// 64 display option 1

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.dst = rret.fromFloatBytes(bytes, 58);
							rret.smbl = (short)rret.fromIntBytes(bytes, 62);
							rret.dspl = (D104_Wpt_Type_Display_Modes)bytes[64];
						}
							break;

						case 105:
						{
							if(bytes.Length < 8 + 2 + 1)
							{
								logError("GPacketReceived:fromBytes() -- D105_Wpt_Type length=" + bytes.Length);
								//break;
							}
							D105_Wpt_Type rret = new D105_Wpt_Type();

							/*
							Semicircle_Type posn;	// 0 - 32 bit semicircle 8
							Symbol_Type smbl;		// 8  - waypoint symbol 2 =  18=userWpt, 63=geocache  64=geocacheFound
							char ident[];			// 10 - variable length string 1-51
							*/

							rret.posn = new Semicircle_Type(bytes, 0);
							rret.smbl = (short)rret.fromIntBytes(bytes, 8);
							int varPos = 10;
							int nextPos;
							rret.ident = rret.fromVarString(bytes, varPos, out nextPos);
							ret = rret;
						}
							break;

						case 106:
						{
							if(bytes.Length < 1 + 13 + 8 + 2 + 2)
							{
								logError("GPacketReceived:fromBytes() -- D106_Wpt_Type length=" + bytes.Length);
								break;
							}
							D106_Wpt_Type rret = new D106_Wpt_Type();

							/*
							byte wpt_class;			// 0  - class (see below) 1
							byte subclass[13];		// 1  - subclass 13
							Semicircle_Type posn;	// 14 - 32 bit semicircle 8
							Symbol_Type smbl;		// 22 - waypoint symbol 2 =  18=userWpt, 63=geocache  64=geocacheFound
							// char ident[];		// 24 - variable length string 1-51
							// char lnk_ident[];	// xx - variable length string 1-51
							*/

							rret.wpt_class = bytes[0];
							rret.posn = new Semicircle_Type(bytes, 14);
							rret.smbl = (short)rret.fromIntBytes(bytes, 22);
							int varPos = 24;
							int nextPos;
							rret.ident = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.lnk_ident = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							ret = rret;
						}
							break;

						case 107:
						{
							if(bytes.Length != 2 + 6 + 8 + 4 + 40 + 2 + 4 + 1)	// should be 65
							{
								logError("GPacketReceived:fromBytes() -- D107_Wpt_Type length=" + bytes.Length);
								//break;
							}
							D107_Wpt_Type rret = new D107_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// byte smbl;				// 58 symbol id 1
							// byte dspl;				// 59 display option 1
							// float dst;				// 60 proximity distance (meters) 4
							// byte color;				// 64 display option 1

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.smbl = bytes[58];
							rret.dspl = (D103_Wpt_Type_Display_Modes)bytes[59];
							rret.dst = rret.fromFloatBytes(bytes, 60);
							rret.color = (D107_Wpt_Type_Colors)bytes[64];
						}
							break;

						case 108:
						{
							if(bytes.Length < 48)
							{
								logError("GPacketReceived:fromBytes() -- D108_Wpt_Type length=" + bytes.Length);
								break;
							}
							D108_Wpt_Type rret = new D108_Wpt_Type();

							//in:   <35=Pid_Wpt_Data> cnt=57 : 0 255 0 96 18 0 0 0 0 0 0 0 255 255 255 255 255 255 255 255 255 255 255 255 230 191 141 25 183 136 39 172 81 89 4 105 81 89 4 105 81 89 4 105 32 32 32 32 48 48 49 0 0 0 0 0 0
							//in:   <35=Pid_Wpt_Data> cnt=57 : 0 255 0 96 18 0 0 0 0 0 0 0 255 255 255 255 255 255 255 255 255 255 255 255 66 217 147 25 143 49 39 172 81 89 4 105 81 89 4 105 81 89 4 105 32 32 32 32 48 48 50 0 0 0 0 0 0
							//in:   <35=Pid_Wpt_Data> cnt=57 : 0 255 0 96 18 0 0 0 0 0 0 0 255 255 255 255 255 255 255 255 255 255 255 255 250 45 10 26 169 61 10 172 81 89 4 105 81 89 4 105 81 89 4 105 32 32 32 32 48 48 51 0 0 0 0 0 0

							/*
							byte wpt_class;			// class (see below) 1
							byte color;				// color (see below) 1
							byte dspl;				// display options (see below) 1
							byte attr;				// attributes (see below) 1
							Symbol_Type smbl;		// 4  - waypoint symbol 2 =  18=userWpt, 63=geocache  64=geocacheFound
							byte subclass[18];		// 6  - subclass 18
							Semicircle_Type posn;	// 24 - 32 bit semicircle 8
							float alt;				// 32 - altitude in meters 4
							float dpth;				// 36 - depth in meters 4
							float dist;				// 40 - proximity distance in meters 4
							char state[2];			// 44 - state 2
							char cc[2];				// 46 - country code 2
							// char ident[];		// 48 - variable length string 1-51
							// char comment[];		// waypoint user comment 1-51
							// char facility[];		// facility name 1-31
							// char city[];			// city name 1-25
							// char addr[];			// address number 1-51
							// char cross_road[];	// intersecting road label 1-51
							*/

							rret.smbl = (short)rret.fromIntBytes(bytes, 4);
							rret.posn = new Semicircle_Type(bytes, 24);
							/*
							string str = "bytes: ";
							for(int i=0; i < bytes.Length && i < 40 ;i++)
							{
								str += " " + bytes[i];
							}
							LibSys.StatusBar.Trace(str + "  ========= time=" + rret.time);
							*/
							rret.alt = rret.fromFloatBytes(bytes, 32);
							rret.dpth = rret.fromFloatBytes(bytes, 36);
							int varPos = 48;
							int nextPos;
							rret.ident = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.comment = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.facility = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.city = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.addr = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.cross_road = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							ret = rret;
						}
							break;

						case 109:
						{
							if(bytes.Length < 52)
							{
								logError("GPacketReceived:fromBytes() -- D109_Wpt_Type length=" + bytes.Length);
								break;
							}
							D109_Wpt_Type rret = new D109_Wpt_Type();

							/*
							public byte        dtyp;				// data packet type (0x01 for D109)1    
							public byte        wpt_class;			// public class                    1    
							public byte        dspl_color;			// display & color (see below)     1    
							public byte        attr;				// attributes (0x70 for D109)      1    
							public Symbol_Type smbl;				// waypoint symbol                 2    
							public byte[]      subclass = new byte[18];   // subclass                 18   6
							public Semicircle_Type posn;			// 32 bit semicircle               8   24 
							public float       alt;					// altitude in meters              4   32 
							public float       dpth;				// depth in meters                 4   36 
							public float       dist;				// proximity distance in meters    4   40 
							public char[]      state = new char[2]; // state                           2   44 
							public char[]      cc = new char[2];    // country code                    2   46 
							public longword    ete;					// outbound link ete in seconds    4   48 
							public string ident;					// variable length string 1-51         52
							public string comment;					// waypoint user comment 1-51
							public string facility;					// facility name 1-31
							public string city;						// city name 1-25
							public string addr;						// address number 1-51
							public string cross_road;				// intersecting road label 1-51
							*/

							rret.smbl = (short)rret.fromIntBytes(bytes, 4);
							rret.posn = new Semicircle_Type(bytes, 24);
							/*
							string str = "bytes: ";
							for(int i=0; i < bytes.Length && i < 40 ;i++)
							{
								str += " " + bytes[i];
							}
							LibSys.StatusBar.Trace(str + "  ========= time=" + rret.time);
							*/
							rret.alt = rret.fromFloatBytes(bytes, 32);
							rret.dpth = rret.fromFloatBytes(bytes, 36);
							int varPos = 52;
							int nextPos;
							rret.ident = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.comment = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.facility = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.city = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.addr = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							rret.cross_road = rret.fromVarString(bytes, varPos, out nextPos);
							varPos = nextPos;
							ret = rret;
						}
							break;
						
						case 150:
						{
							D150_Wpt_Type rret = new D150_Wpt_Type();

							// char[6] ident;			// 0  identifier  6
							// char[2] cc;				// 6 country code 2
							// byte D150_Wpt_Type_Wpt_Class wpt_class;		// 8 byte - public class 1
							// Semicircle_Type posn;	// 9  position   8
							// int alt;					// 17 altitude (meters) 2
							// char[24] city;			// 19 city 24
							// char[2] state;			// 43 state 2
							// char[30] name;			// 45 facility name  30
							// char[40] comment;		// 75 comment  40

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.cc = rret.fromFixedString(bytes, 6, 2);
							rret.wpt_class = (D150_Wpt_Type_Wpt_Class)bytes[8];
							rret.posn = new Semicircle_Type(bytes, 9);
							rret.alt = rret.fromIntBytes(bytes, 17);
							rret.city = rret.fromFixedString(bytes, 19, 24);
							rret.state = rret.fromFixedString(bytes, 43, 2);
							rret.name = rret.fromFixedString(bytes, 45, 30);
							rret.comment = rret.fromFixedString(bytes, 75, 40);
						}
							break;

						case 151:
						{
							D151_Wpt_Type rret = new D151_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// float dst;				// 58 proximity distance (meters) 4
							// char[30] name;			// 62 facility name  30
							// char[24] city;			// 92 city 24
							// char[2] state;			// 116 state 2
							// int alt;					// 118 altitude (meters) 2
							// char[2] cc;				// 120 country code      2
							// char unused2;			// 122 should be set to zero 1
							// byte D151_Wpt_Type_Wpt_Class wpt_class;		// 123 public class 1


							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.dst = rret.fromFloatBytes(bytes, 58);
							rret.name = rret.fromFixedString(bytes, 62, 30);
							rret.city = rret.fromFixedString(bytes, 92, 24);
							rret.state = rret.fromFixedString(bytes, 116, 2);
							rret.alt = rret.fromIntBytes(bytes, 118);
							rret.cc = rret.fromFixedString(bytes, 120, 2);
							rret.wpt_class = (D151_Wpt_Type_Wpt_Class)bytes[123];
						}
							break;
						
						case 152:
						{
							D152_Wpt_Type rret = new D152_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// float dst;				// 58 proximity distance (meters) 4
							// char[30] name;			// 62 facility name  30
							// char[24] city;			// 92 city 24
							// char[2] state;			// 116 state 2
							// int alt;					// 118 altitude (meters) 2
							// char[2] cc;				// 120 country code      2
							// char unused2;			// 122 should be set to zero 1
							// byte D152_Wpt_Type_Wpt_Class wpt_class;		// 123 public class 1

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.dst = rret.fromFloatBytes(bytes, 58);
							rret.name = rret.fromFixedString(bytes, 62, 30);
							rret.city = rret.fromFixedString(bytes, 92, 24);
							rret.state = rret.fromFixedString(bytes, 116, 2);
							rret.alt = rret.fromIntBytes(bytes, 118);
							rret.cc = rret.fromFixedString(bytes, 120, 2);
							rret.wpt_class = (D152_Wpt_Type_Wpt_Class)bytes[123];
						}
							break;
					
						case 154:
						{
							D154_Wpt_Type rret = new D154_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// float dst;				// 58 proximity distance (meters) 4
							// char[30] name;			// 62 facility name  30
							// char[24] city;			// 92 city 24
							// char[2] state;			// 116 state 2
							// int alt;					// 118 altitude (meters) 2
							// char[2] cc;				// 120 country code      2
							// char unused2;			// 122 should be set to zero 1
							// byte D154_Wpt_Type_Wpt_Class wpt_class;		// 123 public class 1
							// Symbol_Type smbl;							// 124 symbol id 2

							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.dst = rret.fromFloatBytes(bytes, 58);
							rret.name = rret.fromFixedString(bytes, 62, 30);
							rret.city = rret.fromFixedString(bytes, 92, 24);
							rret.state = rret.fromFixedString(bytes, 116, 2);
							rret.alt = rret.fromIntBytes(bytes, 118);
							rret.cc = rret.fromFixedString(bytes, 120, 2);
							rret.wpt_class = (D154_Wpt_Type_Wpt_Class)bytes[123];
							rret.smbl = (short)rret.fromIntBytes(bytes, 124);
						}
							break;
						
						case 155:
						{
							D155_Wpt_Type rret = new D155_Wpt_Type();

							// char[6] ident;			// 0  identifier 6
							// Semicircle_Type posn;	// 6  position   8
							// longword unused;			// 14 should be set to zero 4
							// char[40] cmnt;			// 18 comment 40
							// float dst;				// 58 proximity distance (meters) 4
							// char[30] name;			// 62 facility name  30
							// char[24] city;			// 92 city 24
							// char[2] state;			// 116 state 2
							// int alt;					// 118 altitude (meters) 2
							// char[2] cc;				// 120 country code      2
							// char unused2;			// 122 should be set to zero 1
							// byte D155_Wpt_Type_Wpt_Class wpt_class;		// 123 public class 1
							// Symbol_Type smbl;							// 124 symbol id 2
							// byte D155_Wpt_Type_Display_Mode dspl;		// 126 display option 1


							rret.ident = rret.fromFixedString(bytes, 0, 6);
							rret.posn = new Semicircle_Type(bytes, 6);
							rret.comment = rret.fromFixedString(bytes, 18, 40);
							rret.dst = rret.fromFloatBytes(bytes, 58);
							rret.name = rret.fromFixedString(bytes, 62, 30);
							rret.city = rret.fromFixedString(bytes, 92, 24);
							rret.state = rret.fromFixedString(bytes, 116, 2);
							rret.alt = rret.fromIntBytes(bytes, 118);
							rret.cc = rret.fromFixedString(bytes, 120, 2);
							rret.wpt_class = (D155_Wpt_Type_Wpt_Class)bytes[123];
							rret.smbl = (short)rret.fromIntBytes(bytes, 124);
							rret.dspl = (D155_Wpt_Type_Display_Mode)bytes[126];
						}
							break;

						default:
							break;
					}
				}
					break;
				case (int)L001Pids.Pid_Pvt_Data:
				{
					D800_Pvt_Data_Type rret = new D800_Pvt_Data_Type();
					/*
					public float alt;			// altitude above WGS 84 ellipsoid (meters)
					public float epe;			// estimated position error, 2 sigma (meters)
					public float eph;			// epe, but horizontal only (meters)
					public float epv;			// epe, but vertical only (meters)
					public int fix;				// type of position fix
					public double tow;			// time of week (seconds)
					public Radian_Type posn;	// latitude and longitude (radians)
					public float east;			// velocity east (meters/second)
					public float north;			// velocity north (meters/second)
					public float up;			// velocity up (meters/second)
					public float msl_hght;		// height of WGS 84 ellipsoid above MSL (meters)
					public int leap_scnds;		// difference between GPS and UTC (seconds)
					public long wn_days;		// week number days
					*/
					rret.alt = rret.fromFloatBytes(bytes, 0);
					rret.epe = rret.fromFloatBytes(bytes, 4);
					rret.eph = rret.fromFloatBytes(bytes, 8);
					rret.epv = rret.fromFloatBytes(bytes, 12);
					rret.fix = rret.fromIntBytes(bytes, 16);
					rret.tow = rret.fromDoubleBytes(bytes, 18);
					rret.posn = new Radian_Type(bytes, 26);
					rret.east = rret.fromFloatBytes(bytes, 42);
					rret.north = rret.fromFloatBytes(bytes, 46);
					rret.up = rret.fromFloatBytes(bytes, 50);
					rret.msl_hght = rret.fromFloatBytes(bytes, 54);
					rret.leap_scnds = rret.fromIntBytes(bytes, 58);
					rret.wn_days = rret.fromLongBytes(bytes, 60);
					ret = rret;
				}
					break;
				default:
					logError("GPacketReceived:fromBytes -- unknown pid=" + m_pid);
					break;
			}

			return ret;
		}
		#endregion

		public void logError(string str)
		{
			if(Project.gpsLogErrors)
			{
				LibSys.StatusBar.Error(str);
			}
		}
	}

	#region Semicircle and Radian types

	public class Semicircle_Type
	{
		public Int32 lat; // latitude in semicircles
		public Int32 lon; // longitude in semicircles

		private const Int64 factor = (1 << 31);
		private const double df = -180.0d / (double)factor;

		public Semicircle_Type(GeoCoord loc)
		{
			// The following formulas show how to convert between degrees and semicircles:
			//		degrees = semicircles * ( 180 / 2^31 )
			//		semicircles = degrees * ( 2^31 / 180 )

			lon = (int)(loc.Lng / df);
			lat = (int)(loc.Lat / df);
		}

		public GeoCoord toGeoCoord()
		{
			// The following formulas show how to convert between degrees and semicircles:
			//		degrees = semicircles * ( 180 / 2^31 )
			//		semicircles = degrees * ( 2^31 / 180 )

			return new GeoCoord((double)lon * df, (double)lat * df);
		}

		public Semicircle_Type(byte[] bytes, int offset)
		{
			lat = bytes[offset] + bytes[offset+1]*256 + bytes[offset+2]*65536 + bytes[offset+3]*16777216;
			lon = bytes[offset+4] + bytes[offset+5]*256 + bytes[offset+6]*65536 + bytes[offset+7]*16777216;
		}

		public int toBytes(byte[] bytes, int offset)
		{
			bytes[offset]   = (byte)(lat & 0xFF);
			bytes[offset+1] = (byte)((lat >> 8) & 0xFF);
			bytes[offset+2] = (byte)((lat >> 16) & 0xFF);
			bytes[offset+3] = (byte)((lat >> 24) & 0xFF);
			bytes[offset+4] = (byte)(lon & 0xFF);
			bytes[offset+5] = (byte)((lon >> 8) & 0xFF);
			bytes[offset+6] = (byte)((lon >> 16) & 0xFF);
			bytes[offset+7] = (byte)((lon >> 24) & 0xFF);
			return 8;
		}
	}

	public class Radian_Type
	{
		double lat; // latitude in radians  64bit
		double lon; // longitude in radians

		public Radian_Type(byte[] bytes, int offset)
		{
			lat = GPacket.fromDoubleBytesS(bytes, offset);
			lon = GPacket.fromDoubleBytesS(bytes, offset + 8);
		}

		const double df = 180.0d / Math.PI;

		public GeoCoord toGeoCoord()
		{
			// The following formulas show how to convert between degrees and radians:
			//		degrees = radians * ( 180 / p )
			//		radians = degrees * ( p / 180 )

			return new GeoCoord(lon * df, lat * df);
		}
	}
	#endregion

	#region actual packets according to Garmin Protocol Document (garmin_iop_spec.pdf):

	public enum BasicPids
	{
		Pid_Ack_Byte = 6,
		Pid_Nak_Byte = 21,
		Pid_Protocol_Array = 253,          // may not be implemented in all products
		Pid_Product_Rqst = 254,
		Pid_Product_Data = 255
	};
	
	public enum L001Pids
	{
		Pid_Command_Data = 10,
		Pid_Xfer_Cmplt = 12,
		Pid_Date_Time_Data = 14,
		Pid_Position_Data = 17,
		Pid_Prx_Wpt_Data = 19,
		Pid_Records = 27,
		Pid_Rte_Hdr = 29,
		Pid_Rte_Wpt_Data = 30,
		Pid_Almanac_Data = 31,
		Pid_Trk_Data = 34,
		Pid_Wpt_Data = 35,
		Pid_Pvt_Data = 51,
		Pid_Rte_Link_Data = 98,
		Pid_Trk_Hdr = 99
	};

	public enum L002Pids
	{
		Pid_Almanac_Data = 4,
		Pid_Command_Data = 11,
		Pid_Xfer_Cmplt = 12,
		Pid_Date_Time_Data = 20,
		Pid_Position_Data = 24,
		Pid_Records = 35,
		Pid_Rte_Hdr = 37,
		Pid_Rte_Wpt_Data = 39,
		Pid_Wpt_Data = 43
	};

	public class A000Product_Data_Type : GPacket
	{
		public int product_ID;
		public int software_version;
		// char product_description[]; null-terminated string
		// ... zero or more additional null-terminated strings
		public ArrayList strings = new ArrayList();
	}

	public class A001Protocol_Data_Type
	{
		byte tag;
		word data;
	}

	public enum A001Protocol_Data_Type_Tags
	{
		Tag_Phys_Prot_Id = 'P',		// tag for Physical protocol ID
		Tag_Link_Prot_Id = 'L',		// tag for Link protocol ID
		Tag_Appl_Prot_Id = 'A',		// tag for Application protocol ID
		Tag_Data_Type_Id = 'D'		// tag for Data Type ID
	}

	public enum A010Commands
	{
		Cmnd_Abort_Transfer = 0,	// abort current transfer
		Cmnd_Transfer_Alm = 1,		// transfer almanac
		Cmnd_Transfer_Posn = 2,		// transfer position
		Cmnd_Transfer_Prx = 3,		// transfer proximity waypoints
		Cmnd_Transfer_Rte = 4,		// transfer routes
		Cmnd_Transfer_Time = 5,		// transfer time
		Cmnd_Transfer_Trk = 6,		// transfer track log
		Cmnd_Transfer_Wpt = 7,		// transfer waypoints
		Cmnd_Turn_Off_Pwr = 8,		// turn off power
		Cmnd_Start_Pvt_Data = 49,	// start transmitting PVT data
		Cmnd_Stop_Pvt_Data = 50		// stop transmitting PVT data
	}

	public enum A011Commands
	{
		Cmnd_Abort_Transfer = 0,    // abort current transfer
		Cmnd_Transfer_Alm = 4,      // transfer almanac
		Cmnd_Transfer_Rte = 8,      // transfer routes
		Cmnd_Transfer_Time = 20,    // transfer time
		Cmnd_Transfer_Wpt = 21,     // transfer waypoints
		Cmnd_Turn_Off_Pwr = 26		// turn off power
	}

	public class L001Records_Type : GPacket
	{
		public int records;
		byte[] bytes = null;

		public L001Records_Type() : base((int)L001Pids.Pid_Records)
		{
		}

		public L001Records_Type(int length) : base((int)L001Pids.Pid_Records)
		{
			bytes = new byte[] { 0x0, 0x2, 0x0, 0x0 };
			bytes[2] = (byte)(length & 0xFF);
			bytes[3] = (byte)((length >> 8) & 0xFF);
		}

		public override byte[] toBytes()
		{
			bytes[0] = (byte)m_pid;
			return bytes;
		}

	}

	public class L001Xfer_Cmplt : GPacket		// actually it is Command_Id_type = GPacketA010Commands
	{
		byte[] bytes = new byte[] { 0x0, 0x2, 0x0, 0x0 };

		public L001Xfer_Cmplt(A010Commands cmd) : base((int)L001Pids.Pid_Xfer_Cmplt)
		{
			bytes[0] = (byte)m_pid;
			bytes[2] = (byte)cmd;
		}

		public override byte[] toBytes()
		{
			return bytes;
		}
	}

	public enum Symbol_Types
	{
		// ---------------------------------------------------------------
		//   Symbols for marine (group 0...0-8191...bits 15-13=000).
		// ---------------------------------------------------------------
		sym_anchor = 0,					// white anchor symbol
		sym_bell = 1,					// white bell symbol
		sym_diamond_grn = 2,			// green diamond symbol
		sym_diamond_red = 3,			// red diamond symbol
		sym_dive1 = 4,					// diver down flag 1
		sym_dive2 = 5,					// diver down flag 2
		sym_dollar = 6,					// white dollar symbol
		sym_fish = 7,					// white fish symbol
		sym_fuel = 8,					// white fuel symbol
		sym_horn = 9,					// white horn symbol
		sym_house = 10,					// white house symbol
		sym_knife = 11,					// white knife & fork symbol
		sym_light = 12,					// white light symbol
		sym_mug = 13,					// white mug symbol
		sym_skull = 14,					// white skull and crossbones symbol
		sym_square_grn = 15,			// green square symbol
		sym_square_red = 16,			// red square symbol
		sym_wbuoy = 17,					// white buoy waypoint symbol
		sym_wpt_dot = 18,				// waypoint dot
		sym_wreck = 19,					// white wreck symbol
		sym_null = 20,					// null symbol (transparent)
		sym_mob = 21,					// man overboard symbol
		// ------------------------------------------------------
		//   marine navaid symbols
		// ------------------------------------------------------
		sym_buoy_ambr = 22,				// amber map buoy symbol
		sym_buoy_blck = 23,				// black map buoy symbol
		sym_buoy_blue = 24,				// blue map buoy symbol
		sym_buoy_grn = 25,				// green map buoy symbol
		sym_buoy_grn_red = 26,          // green/red map buoy symbol
		sym_buoy_grn_wht = 27,          // green/white map buoy symbol
		sym_buoy_orng = 28,				// orange map buoy symbol
		sym_buoy_red = 29,				// red map buoy symbol
		sym_buoy_red_grn = 30,          // red/green map buoy symbol
		sym_buoy_red_wht = 31,          // red/white map buoy symbol
		sym_buoy_violet = 32,			// violet map buoy symbol
		sym_buoy_wht = 33,				// white map buoy symbol
		sym_buoy_wht_grn = 34,          // white/green map buoy symbol
		sym_buoy_wht_red = 35,          // white/red map buoy symbol
		sym_dot = 36,					// white dot symbol
		sym_rbcn = 37,					// radio beacon symbol
		// ------------------------------------------------------
		//   leave space for more navaids (up to 128 total)
		// ------------------------------------------------------
		sym_boat_ramp = 150,			// boat ramp symbol
		sym_camp = 151,					// campground symbol
		sym_restrooms = 152,			// restrooms symbol
		sym_showers = 153,				// shower symbol
		sym_drinking_wtr = 154,         // drinking water symbol
		sym_phone = 155,				// telephone symbol
		sym_1st_aid = 156,				// first aid symbol
		sym_info = 157,					// information symbol
		sym_parking = 158,				// parking symbol
		sym_park = 159,					// park symbol
		sym_picnic = 160,				// picnic symbol
		sym_scenic = 161,				// scenic area symbol
		sym_skiing = 162,				// skiing symbol
		sym_swimming = 163,				// swimming symbol
		sym_dam = 164,					// dam symbol
		sym_controlled = 165,			// controlled area symbol
		sym_danger = 166,				// danger symbol
		sym_restricted = 167,			// restricted area symbol
		sym_null_2 = 168,				// null symbol
		sym_ball = 169,					// ball symbol
		sym_car = 170,					// car symbol
		sym_deer = 171,					// deer symbol
		sym_shpng_cart = 172,			// shopping cart symbol
		sym_lodging = 173,				// lodging symbol
		sym_mine = 174,					// mine symbol
		sym_trail_head = 175,			// trail head symbol
		sym_truck_stop = 176,			// truck stop symbol
		sym_user_exit = 177,			// user exit symbol
		sym_flag = 178,					// flag symbol
		sym_circle_x = 179,				// circle with x in the center
		// ---------------------------------------------------------------
		//   Symbols for land (group 1...8192-16383...bits 15-13=001).
		// ---------------------------------------------------------------
		sym_is_hwy = 8192,				// interstate hwy symbol
		sym_us_hwy = 8193,				// us hwy symbol
		sym_st_hwy = 8194,				// state hwy symbol
		sym_mi_mrkr = 8195,				// mile marker symbol
		sym_trcbck = 8196,				// TracBack (feet) symbol
		sym_golf = 8197,				// golf symbol
		sym_sml_cty = 8198,				// small city symbol
		sym_med_cty = 8199,				// medium city symbol
		sym_lrg_cty = 8200,				// large city symbol
		sym_freeway = 8201,				// intl freeway hwy symbol
		sym_ntl_hwy = 8202,				// intl national hwy symbol
		sym_cap_cty = 8203,				// capitol city symbol (star)
		sym_amuse_pk = 8204,			// amusement park symbol
		sym_bowling = 8205,				// bowling symbol
		sym_car_rental = 8206,          // car rental symbol
		sym_car_repair = 8207,          // car repair symbol
		sym_fastfood = 8208,			// fast food symbol
		sym_fitness = 8209,				// fitness symbol
		sym_movie = 8210,				// movie symbol
		sym_museum = 8211,				// museum symbol
		sym_pharmacy = 8212,			// pharmacy symbol
		sym_pizza = 8213,				// pizza symbol
		sym_post_ofc = 8214,			// post office symbol
		sym_rv_park = 8215,				// RV park symbol
		sym_school = 8216,				// school symbol
		sym_stadium = 8217,				// stadium symbol
		sym_store = 8218,				// dept. store symbol
		sym_zoo = 8219,					// zoo symbol
		sym_gas_plus = 8220,			// convenience store symbol
		sym_faces = 8221,				// live theater symbol
		sym_ramp_int = 8222,			// ramp intersection symbol
		sym_st_int = 8223,				// street intersection symbol
		sym_weigh_sttn = 8226,          // inspection/weigh station symbol
		sym_toll_booth = 8227,          // toll booth symbol
		sym_elev_pt = 8228,				// elevation point symbol
		sym_ex_no_srvc = 8229,          // exit without services symbol
		sym_geo_place_mm = 8230,        // Geographic place name, man-made
		sym_geo_place_wtr = 8231,       // Geographic place name, water
		sym_geo_place_lnd = 8232,       // Geographic place name, land
		sym_bridge = 8233,				// bridge symbol
		sym_building = 8234,			// building symbol
		sym_cemetery = 8235,			// cemetery symbol
		sym_church = 8236,				// church symbol
		sym_civil = 8237,				// civil location symbol
		sym_crossing = 8238,			// crossing symbol
		sym_hist_town = 8239,			// historical town symbol
		sym_levee = 8240,				// levee symbol
		sym_military = 8241,			// military location symbol
		sym_oil_field = 8242,			// oil field symbol
		sym_tunnel = 8243,				// tunnel symbol
		sym_beach = 8244,				// beach symbol
		sym_forest = 8245,				// forest symbol
		sym_summit = 8246,				// summit symbol
		sym_lrg_ramp_int = 8247,        // large ramp intersection symbol
		sym_lrg_ex_no_srvc = 8248,      // large exit without services smbl
		sym_badge = 8249,				// police/official badge symbol
		sym_cards = 8250,				// gambling/casino symbol
		sym_snowski = 8251,				// snow skiing symbol
		sym_iceskate = 8252,			// ice skating symbol
		sym_wrecker = 8253,				// tow truck (wrecker) symbol
		sym_border = 8254,				// border crossing (port of entry)
		sym_geocache = 8255,			// geocache
		sym_geocache_found = 8256,		// geocache found
		// ---------------------------------------------------------------
		//  Symbols for aviation (group 2...16383-24575...bits 15-13=010).
		// ---------------------------------------------------------------
		sym_airport = 16384,			// airport symbol
		sym_int = 16385,				// intersection symbol
		sym_ndb = 16386,				// non-directional beacon symbol
		sym_vor = 16387,				// VHF omni-range symbol
		sym_heliport = 16388,			// heliport symbol
		sym_private = 16389,			// private field symbol
		sym_soft_fld = 16390,			// soft field symbol
		sym_tall_tower = 16391,         // tall tower symbol
		sym_short_tower = 16392,        // short tower symbol
		sym_glider = 16393,				// glider symbol
		sym_ultralight = 16394,         // ultralight symbol
		sym_parachute = 16395,          // parachute symbol
		sym_vortac = 16396,				// VOR/TACAN symbol
		sym_vordme = 16397,				// VOR-DME symbol
		sym_faf = 16398,				// first approach fix
		sym_lom = 16399,				// localizer outer marker
		sym_map = 16400,				// missed approach point
		sym_tacan = 16401,				// TACAN symbol
		sym_seaplane = 16402,			// Seaplane Base
	}


	// ===============================  start of waypoints definitions =================================

	public class D100_Wpt_Type : GPacket
	{
		// char[] ident = new char[6];		// identifier
		public string ident = "";
		public Semicircle_Type posn;		// position
		// longword unused;					// should be set to zero
		// char[] cmnt = new char[40];		// comment
		public string comment = "";

		public D100_Wpt_Type() : base((int)L001Pids.Pid_Wpt_Data)
		{
		}

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// Example products: GPSMAP 210 and GPSMAP 220 (both prior to version 4.00).
	public class D101_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];		// identifier
		//Semicircle_Type posn;				// position
		//longword unused;					// should be set to zero
		//char[] cmnt = new char[40];		// comment
		public float dst = 1.0F;			// proximity distance (meters)
		public byte smbl;					// symbol id

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 5;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			pos += toFloatBytes(dst, bytes, pos);
			bytes[pos++] = smbl;

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}
	
	// Example products: GPSMAP 175, GPSMAP 210 and GPSMAP 220.
	public class D102_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];		// identifier
		//Semicircle_Type posn;				// position
		//longword unused;					// should be set to zero
		//char[] cmnt = new char[40];		// comment
		public float dst;					// proximity distance (meters)
		public Symbol_Type smbl;			// symbol id

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 6;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			pos += toFloatBytes(dst, bytes, pos);
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// Example products: GPS 12, GPS 12 XL, GPS 48 and GPS II Plus.
	public class D103_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];		// identifier
		//Semicircle_Type posn;				// position
		//longword unused;					// should be set to zero
		//char[] cmnt = new char[40];		// comment
		public D103_Wpt_Type_Symbols smbl;	// actually byte - symbol
		public D103_Wpt_Type_Display_Modes dspl;	// display option

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 2;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			bytes[pos++] = (byte)smbl;
			bytes[pos++] = (byte)dspl;

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “smbl” member of the D103_Wpt_Type are shown below:
	public enum D103_Wpt_Type_Symbols
	{
		smbl_dot = 0,            // dot symbol
		smbl_house = 1,          // house symbol
		smbl_gas = 2,            // gas symbol
		smbl_car = 3,            // car symbol
		smbl_fish = 4,           // fish symbol
		smbl_boat = 5,           // boat symbol
		smbl_anchor = 6,         // anchor symbol
		smbl_wreck = 7,          // wreck symbol
		smbl_exit = 8,           // exit symbol
		smbl_skull = 9,          // skull symbol
		smbl_flag = 10,          // flag symbol
		smbl_camp = 11,          // camp symbol
		smbl_circle_x = 12,      // circle with x symbol
		smbl_deer = 13,          // deer symbol
		smbl_1st_aid = 14,       // first aid symbol
		smbl_back_track = 15     // back track symbol
	}

	// The enumerated values for the “dspl” member of the D103_Wpt_Type are shown below:
	public enum D103_Wpt_Type_Display_Modes
	{
		dspl_name = 0,          // Display symbol with waypoint name
		dspl_none = 1,          // Display symbol by itself
		dspl_cmnt = 2           // Display symbol with comment
	}

	// Example products: GPS III.
	public class D104_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];		// identifier
		//Semicircle_Type posn;				// position
		//longword unused;					// should be set to zero
		//char[] cmnt = new char[40];		// comment
		public float dst = 1.0f;			// proximity distance (meters)
		public Symbol_Type smbl = (Symbol_Type)Symbol_Types.sym_wpt_dot;			// symbol id
		public D104_Wpt_Type_Display_Modes dspl = D104_Wpt_Type_Display_Modes.dspl_smbl_name;	// display option

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 7;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			pos += toFloatBytes(dst, bytes, pos);
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);
			bytes[pos++] = (byte)dspl;

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “dspl” member of the D104_Wpt_Type are shown below:
	public enum D104_Wpt_Type_Display_Modes
	{
		dspl_smbl_none = 0,			// Display symbol by itself
		dspl_smbl_only = 1,			// Display symbol by itself
		dspl_smbl_name = 3,			// Display symbol with waypoint name
		dspl_smbl_cmnt = 5			// Display symbol with comment
	}

	// Example products: StreetPilot (user waypoints).
	public class D105_Wpt_Type : GPacket
	{
		public Semicircle_Type posn;		// position
		public Symbol_Type smbl;			// symbol id
		// char wpt_ident[]; null-terminated string
		public string ident = "";

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 8 + 2 + ident.Length + 1;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += posn.toBytes(bytes, pos);
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);
			pos += toVarStringBytes(ident, bytes, pos, 50);

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			if(pos < sizeBytesEval)
			{
				// we have truncated some strings, so the body is shorter than evaluated before:
				byte[] ret = new byte[pos];
				int i = 0;
				foreach(byte b in bytes)
				{
					ret[i++] = b;
					if(i >= pos)
					{
						break;
					}
				}
				bytes = ret;
			}

			return bytes;
		}
	}

	// Example products: StreetPilot (route waypoints).
	public class D106_Wpt_Type : GPacket
	{
		public byte wpt_class = 0;				// byte -  public class
		public byte[] subclass = new byte[13];	// subclass - must be valid when wpt_class > 0
		public Semicircle_Type posn;			// position
		public Symbol_Type smbl;				// symbol id
		// char wpt_ident[];					// null-terminated string
		public string ident = "";				// variable length string 1-?
		// char lnk_ident[];					// null-terminated string
		public string lnk_ident = "";			// variable length string 1-?

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 1 + 13 + 8 + 2 + ident.Length + lnk_ident.Length + 2;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			bytes[pos++] = wpt_class;
			pos += subclass.GetLength(0);
			pos += posn.toBytes(bytes, pos);
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);
			pos += toVarStringBytes(ident, bytes, pos, 50);
			pos += toVarStringBytes(lnk_ident, bytes, pos, 50);

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			if(pos < sizeBytesEval)
			{
				// we have truncated some strings, so the body is shorter than evaluated before:
				byte[] ret = new byte[pos];
				int i = 0;
				foreach(byte b in bytes)
				{
					ret[i++] = b;
					if(i >= pos)
					{
						break;
					}
				}
				bytes = ret;
			}

			return bytes;
		}
	}

	// Example products: GPS 12CX.
	public class D107_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];		// identifier
		//Semicircle_Type posn;				// position
		//longword unused;					// should be set to zero
		//char[] cmnt = new char[40];		// comment
		public byte smbl;					// symbol 
		public D103_Wpt_Type_Display_Modes dspl;	// byte - display option
		public float dst = 1.0f;			// proximity distance (meters)
		public D107_Wpt_Type_Colors color;	// byte - waypoint color

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 2 + 4 + 1;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			bytes[pos++] = smbl;
			bytes[pos++] = (byte)dspl;
			pos += toFloatBytes(dst, bytes, pos);
			bytes[pos++] = (byte)color;

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “smbl” member of the D107_Wpt_Type are the same as the the “smbl” member of
	// the D103_Wpt_Type.
	// The enumerated values for the “dspl” member of the D107_Wpt_Type are the same as the the “dspl” member of the
	// D103_Wpt_Type.

	// The enumerated values for the “color” member of the D107_Wpt_Type are shown below:
	public enum D107_Wpt_Type_Colors
	{
		clr_default = 0,			// Default waypoint color
		clr_red		= 1,			// Red
		clr_green	= 2,			// Green
		clr_blue	= 3				// Blue
	}

	// Example products: GPSMAP 162/168, eMap, GPSMAP 295.
	public class D108_Wpt_Type : GPacket
	{
		public D108_Wpt_Type_Wpt_Class wpt_class = D108_Wpt_Type_Wpt_Class.USER_WPT;	// public class (see below) 1
		public D108_Wpt_Type_Color color = D108_Wpt_Type_Color.Default_Color; 			// color (see below) 1
		public D103_Wpt_Type_Display_Modes dspl = D103_Wpt_Type_Display_Modes.dspl_name;// display options (see below) 1
		public byte attr = 0x60;					// attributes (see below) 1
		public Symbol_Type smbl = (Symbol_Type)Symbol_Types.sym_wpt_dot; 				// waypoint symbol 2
		public byte[] subclass = new byte[] {
				  0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255
											  };		// subclass 18
		public Semicircle_Type posn; 				// 32 bit semicircle 8
		public float alt = 0.0f; 					// altitude in meters 4
		public float dpth = 0.0f; 					// depth in meters 4
		public float dist = 0.0f; 					// proximity distance in meters 4
		public char[] state = new char[2];			// state 2
		public char[] cc = new char[2];				// country code 2
		public string ident = "";					// variable length string 1-51
		public string comment = "";					// waypoint user comment 1-51
		public string facility = "";				// facility name 1-31
		public string city = "";					// city name 1-25
		public string addr = "";					// address number 1-51
		public string cross_road = "";				// intersecting road label 1-51

		public D108_Wpt_Type() : base((int)L001Pids.Pid_Wpt_Data)
		{
		}

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 48 + ident.Length + comment.Length
								+ facility.Length + city.Length + addr.Length + cross_road.Length + 6;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			bytes[pos++] = (byte)wpt_class;
			bytes[pos++] = (byte)color;
			bytes[pos++] = (byte)dspl;
			bytes[pos++] = (byte)attr;
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);
			subclass.CopyTo(bytes, pos);
			pos += subclass.GetLength(0);
			pos += posn.toBytes(bytes, pos);
			pos += toFloatBytes(alt, bytes, pos);
			pos += toFloatBytes(dpth, bytes, pos);
			pos += toFloatBytes(dist, bytes, pos);
			bytes[pos++] = (byte)state[0];
			bytes[pos++] = (byte)state[1];
			bytes[pos++] = (byte)cc[0];
			bytes[pos++] = (byte)cc[1];
			pos += toVarStringBytes(ident, bytes, pos, 50);
			pos += toVarStringBytes(comment, bytes, pos, 50);
			pos += toVarStringBytes(facility, bytes, pos, 30);
			pos += toVarStringBytes(city, bytes, pos, 24);
			pos += toVarStringBytes(addr, bytes, pos, 50);
			pos += toVarStringBytes(cross_road, bytes, pos, 50);
			bytes[1] = (byte)(pos - 2);	// now fill cnt

			if(pos < sizeBytesEval)
			{
				// we have truncated some strings, so the body is shorter than evaluated before:
				byte[] ret = new byte[pos];
				int i = 0;
				foreach(byte b in bytes)
				{
					ret[i++] = b;
					if(i >= pos)
					{
						break;
					}
				}
				bytes = ret;
			}

			return bytes;
		}
	}

	// The enumerated values for the “wpt_class” member of the D108_Wpt_Type are defined as follows:
	public enum D108_Wpt_Type_Wpt_Class
	{
		USER_WPT = 0x00,              // User waypoint
		AVTN_APT_WPT = 0x40,          // Aviation Airport waypoint
		AVTN_INT_WPT = 0x41,          // Aviation Intersection waypoint
		AVTN_NDB_WPT = 0x42,          // Aviation NDB waypoint
		AVTN_VOR_WPT = 0x43,          // Aviation VOR waypoint
		AVTN_ARWY_WPT = 0x44,         // Aviation Airport Runway waypoint
		AVTN_AINT_WPT = 0x45,         // Aviation Airport Intersection
		AVTN_ANDB_WPT = 0x46,         // Aviation Airport NDB waypoint
		MAP_PNT_WPT = 0x80,           // Map Point waypoint
		MAP_AREA_WPT = 0x81,          // Map Area waypoint
		MAP_INT_WPT = 0x82,           // Map Intersection waypoint
		MAP_ADRS_WPT = 0x83,          // Map Address waypoint
		MAP_LABEL_WPT = 0x84,         // Map Label Waypoint
		MAP_LINE_WPT = 0x85,          // Map Line Waypoint
	}

	// The “color” member can be one of the following values:
	public enum D108_Wpt_Type_Color
	{
		Black, Dark_Red, Dark_Green, Dark_Yellow,
		Dark_Blue, Dark_Magenta, Dark_Cyan, Light_Gray,
		Dark_Gray, Red, Green, Yellow,
		Blue, Magenta, Cyan, White,
		Default_Color = 0xFF
	}

	// The enumerated values for the “dspl” member of the D108_Wpt_Type are the same as
	//  the “dspl” member of the D103_Wpt_Type.


	// D109 Waypoint Type Quick Reference
	//		All fields are defined the same as D108 except as noted below.
	//		dtyp - Data packet type, must be 0x01 for D109.
	//		dsp_color - The 'dspl_color' member contains three fields; bits 0-4 specify
	//		the color, bits 5-6 specify the waypoint display attribute and bit 7 is unused
	//		and must be 0. Color values are as specified for D108 except that the default
	//		value is 0x1f. Display attribute values are as specified for D108.
	//		attr - Attribute. Must be 0x70 for D109.
	//		ete - Estimated time en route in seconds to next waypoint. Default value is 0xffffffff.
	public class D109_Wpt_Type : GPacket
	{
		/*
		public byte        dtyp;				// data packet type (0x01 for D109)1    
		public byte        wpt_class;			// public class                    1    
		public byte        dspl_color;			// display & color (see below)     1    
		public byte        attr;				// attributes (0x70 for D109)      1    
		public Symbol_Type smbl;				// waypoint symbol                 2    
		public byte[]      subclass = new byte[18];   // subclass                 18   6
		public Semicircle_Type posn;			// 32 bit semicircle               8   24 
		public float       alt;					// altitude in meters              4   32 
		public float       dpth;				// depth in meters                 4   36 
		public float       dist;				// proximity distance in meters    4   40 
		public char[]      state = new char[2]; // state                           2   44 
		public char[]      cc = new char[2];    // country code                    2   46 
		public longword    ete;					// outbound link ete in seconds    4   48 
		public string ident;					// variable length string 1-51         52
		public string comment;					// waypoint user comment 1-51
		public string facility;					// facility name 1-31
		public string city;						// city name 1-25
		public string addr;						// address number 1-51
		public string cross_road;				// intersecting road label 1-51
		*/

		public byte dtyp = 0x01;					// data packet type (0x01 for D109)1    
		public D108_Wpt_Type_Wpt_Class wpt_class = D108_Wpt_Type_Wpt_Class.USER_WPT;	// public class (see below) 1
		public byte dspl_color = 0x1f; 				// color (see below) 1
		public byte attr = 0x70;					// attributes (see below) 1
		public Symbol_Type smbl = (Symbol_Type)Symbol_Types.sym_wpt_dot; 				// waypoint symbol 2
		public byte[] subclass = new byte[] {
												0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255
											};		// subclass 18
		public Semicircle_Type posn; 				// 32 bit semicircle 8
		public float alt = 0.0f; 					// altitude in meters 4
		public float dpth = 0.0f; 					// depth in meters 4
		public float dist = 0.0f; 					// proximity distance in meters 4
		public char[] state = new char[2];			// state 2
		public char[] cc = new char[2];				// country code 2
		public longword ete = -1;					// outbound link ete in seconds    4    
		public string ident = "";					// variable length string 1-51
		public string comment = "";					// waypoint user comment 1-51
		public string facility = "";				// facility name 1-31
		public string city = "";					// city name 1-25
		public string addr = "";					// address number 1-51
		public string cross_road = "";				// intersecting road label 1-51


		public D109_Wpt_Type() : base((int)L001Pids.Pid_Wpt_Data)
		{
		}

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 52 + ident.Length + comment.Length
									+ facility.Length + city.Length + addr.Length + cross_road.Length + 6;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			bytes[pos++] = dtyp;
			bytes[pos++] = (byte)wpt_class;
			bytes[pos++] = (byte)dspl_color;
			bytes[pos++] = (byte)attr;
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);
			subclass.CopyTo(bytes, pos);
			pos += subclass.GetLength(0);
			pos += posn.toBytes(bytes, pos);
			pos += toFloatBytes(alt, bytes, pos);
			pos += toFloatBytes(dpth, bytes, pos);
			pos += toFloatBytes(dist, bytes, pos);
			bytes[pos++] = (byte)state[0];
			bytes[pos++] = (byte)state[1];
			bytes[pos++] = (byte)cc[0];
			bytes[pos++] = (byte)cc[1];
			pos += toLongwordBytes(ete, bytes, pos);
			pos += toVarStringBytes(ident, bytes, pos, 50);
			pos += toVarStringBytes(comment, bytes, pos, 50);
			pos += toVarStringBytes(facility, bytes, pos, 30);
			pos += toVarStringBytes(city, bytes, pos, 24);
			pos += toVarStringBytes(addr, bytes, pos, 50);
			pos += toVarStringBytes(cross_road, bytes, pos, 50);
			bytes[1] = (byte)(pos - 2);

			if(pos < sizeBytesEval)
			{
				// we have truncated some strings, so the body is shorter than evaluated before:
				byte[] ret = new byte[pos];
				int i = 0;
				foreach(byte b in bytes)
				{
					ret[i++] = b;
					if(i >= pos)
					{
						break;
					}
				}
				bytes = ret;
			}

			return bytes;
		}
	}

	// Example products: GPS 150, GPS 155, GNC 250 and GNC 300.
	public class D150_Wpt_Type : GPacket
	{
		// char[6] ident;					// identifier
		public string ident = "";
		// char[2] cc;						// country code
		public string cc = "";
		public D150_Wpt_Type_Wpt_Class wpt_class = D150_Wpt_Type_Wpt_Class.usr_wpt_class;	// byte - public class
		public Semicircle_Type posn;		// position
		public int alt;						// altitude (meters)
		// char[24] city;					// city
		public string city = "";
		// char[2] state;					// state
		public string state = "";
		// char[30] name;					// facility name
		public string name = "";
		// char[40] cmnt;					// comment
		public string comment = "";

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 2 + 1 + 8 + 4 + 24 + 2 + 30 + 40;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += toFixedStringBytes(cc, bytes, pos, 2);
			bytes[pos++] = (byte)wpt_class;
			pos += posn.toBytes(bytes, pos);
			pos += toIntBytes(alt, bytes, pos);
			pos += toFixedStringBytes(city,    bytes, pos, 24);
			pos += toFixedStringBytes(state,   bytes, pos, 2);
			pos += toFixedStringBytes(name,    bytes, pos, 30);
			pos += toFixedStringBytes(comment, bytes, pos, 40);

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “wpt_class” member of the D150_Wpt_Type are shown below:
	public enum D150_Wpt_Type_Wpt_Class
	{
		apt_wpt_class = 0,			// airport waypoint public class
		int_wpt_class = 1,			// intersection waypoint public class
		ndb_wpt_class = 2,			// NDB waypoint public class
		vor_wpt_class = 3,			// VOR waypoint public class
		usr_wpt_class = 4,			// user defined waypoint public class
		rwy_wpt_class = 5,			// airport runway threshold waypoint public class
		aint_wpt_class = 6,			// airport intersection waypoint public class
		locked_wpt_class = 7		// locked waypoint public class
	}

	// The “locked_wpt_class” code indicates that a route within a GPS contains an aviation database waypoint
	// that the GPS could not find in its aviation database (presumably because the aviation database was updated
	// to a newer

	public class D151_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];	// identifier
		//Semicircle_Type posn;			// position
		//longword unused;				// should be set to zero
		//char[] cmnt = new char[40];	// comment
		public float dst = 0.0f;		// proximity distance (meters)
		// char[30] name;				// facility name
		public string name = "";
		// char[24] city;				// city
		public string city = "";
		// char[2] state;				// state
		public string state = "";
		public int alt = 0;				// altitude (meters)
		// char[2] cc;					// country code
		public string cc = "";
		//char unused2;					// should be set to zero
		public D151_Wpt_Type_Wpt_Class wpt_class = D151_Wpt_Type_Wpt_Class.usr_wpt_class;	// byte - public class

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 4 + 30 + 24 + 2 + 2 + 2 + 1 + 1;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			pos += toFloatBytes(dst, bytes, pos);
			pos += toFixedStringBytes(name, bytes, pos, 30);
			pos += toFixedStringBytes(city, bytes, pos, 24);
			pos += toFixedStringBytes(state, bytes, pos, 2);
			pos += toIntBytes(alt, bytes, pos);
			pos += toFixedStringBytes(cc, bytes, pos, 2);
			bytes[pos++] = (byte)0;
			bytes[pos++] = (byte)wpt_class;

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “wpt_class” member of the D151_Wpt_Type are shown below:
	public enum D151_Wpt_Type_Wpt_Class
	{
		apt_wpt_class = 0,			// airport waypoint public class
		vor_wpt_class = 1,			// VOR waypoint public class
		usr_wpt_class = 2,			// user defined waypoint public class
		locked_wpt_class = 3		// locked waypoint public class
	}

	// Example products: GPS 90, GPS 95 AVD, GPS 95 XL and GPSCOM 190.
	public class D152_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];	// identifier
		//Semicircle_Type posn;			// position
		//longword unused;				// should be set to zero
		//char[] cmnt = new char[40];	// comment
		public float dst = 0.0f;		// proximity distance (meters)
		// char[30] name;				// facility name
		public string name = "";
		// char[24] city;				// city
		public string city = "";
		// char[2] state;				// state
		public string state = "";
		public int alt = 0;				// altitude (meters)
		// char[2] cc;					// country code
		public string cc = "";
		//char unused2;					// should be set to zero
		public D152_Wpt_Type_Wpt_Class wpt_class = D152_Wpt_Type_Wpt_Class.usr_wpt_class;	// byte - public class

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 4 + 30 + 24 + 2 + 2 + 2 + 1 + 1;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			pos += toFloatBytes(dst, bytes, pos);
			pos += toFixedStringBytes(name, bytes, pos, 30);
			pos += toFixedStringBytes(city, bytes, pos, 24);
			pos += toFixedStringBytes(state, bytes, pos, 2);
			pos += toIntBytes(alt, bytes, pos);
			pos += toFixedStringBytes(cc, bytes, pos, 2);
			bytes[pos++] = (byte)0;
			bytes[pos++] = (byte)wpt_class;

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “wpt_class” member of the D152_Wpt_Type are shown below:
	public enum D152_Wpt_Type_Wpt_Class
	{
		apt_wpt_class = 0,		// airport waypoint public class
		int_wpt_class = 1,		// intersection waypoint public class
		ndb_wpt_class = 2,		// NDB waypoint public class
		vor_wpt_class = 3,		// VOR waypoint public class
		usr_wpt_class = 4,		// user defined waypoint public class
		locked_wpt_class = 5	// locked waypoint public class
	}

	// Example products: GPSMAP 195.
	public class D154_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];	// identifier
		//Semicircle_Type posn;			// position
		//longword unused;				// should be set to zero
		//char[] cmnt = new char[40];	// comment
		public float dst = 0.0f;		// proximity distance (meters)
		// char[30] name;				// facility name
		public string name = "";
		// char[24] city;				// city
		public string city = "";
		// char[2] state;				// state
		public string state = "";
		public int alt = 0;				// altitude (meters)
		// char[2] cc;					// country code
		public string cc = "";
		//char unused2;					// should be set to zero
		public D154_Wpt_Type_Wpt_Class wpt_class = D154_Wpt_Type_Wpt_Class.usr_wpt_class;	// byte - public class
		public Symbol_Type smbl;			// symbol id

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 4 + 30 + 24 + 2 + 2 + 2 + 1 + 1 + 2;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			pos += toFloatBytes(dst, bytes, pos);
			pos += toFixedStringBytes(name, bytes, pos, 30);
			pos += toFixedStringBytes(city, bytes, pos, 24);
			pos += toFixedStringBytes(state, bytes, pos, 2);
			pos += toIntBytes(alt, bytes, pos);
			pos += toFixedStringBytes(cc, bytes, pos, 2);
			bytes[pos++] = (byte)0;
			bytes[pos++] = (byte)wpt_class;
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “wpt_class” member of the D154_Wpt_Type are shown below:
	public enum D154_Wpt_Type_Wpt_Class
	{
		apt_wpt_class = 0,			// airport waypoint public class
		int_wpt_class = 1,			// intersection waypoint public class
		ndb_wpt_class = 2,			// NDB waypoint public class
		vor_wpt_class = 3,			// VOR waypoint public class
		usr_wpt_class = 4,			// user defined waypoint public class
		rwy_wpt_class = 5,			// airport runway threshold waypoint public class
		aint_wpt_class = 6,			// airport intersection waypoint public class
		andb_wpt_class = 7,			// airport NDB waypoint public class
		sym_wpt_class = 8,			// user defined symbol-only waypoint public class
		locked_wpt_class = 9		// locked waypoint public class
	}

	// Example products: GPS III Pilot.
	public class D155_Wpt_Type : D100_Wpt_Type
	{
		//char[] ident = new char[6];	// identifier
		//Semicircle_Type posn;			// position
		//longword unused;				// should be set to zero
		//char[] cmnt = new char[40];	// comment
		public float  dst = 1.0f;		// proximity distance (meters)
		// char[30] name;				// facility name
		public string name = "";
		// char[24] city;				// city
		public string city = "";
		// char[2] state;				// state
		public string state = "";
		public int    alt;				// altitude (meters)
		// char[2] cc;					// country code
		public string cc = "";
		//char unused2;					// should be set to zero
		public D155_Wpt_Type_Wpt_Class wpt_class = D155_Wpt_Type_Wpt_Class.usr_wpt_class;		// byte - public class
		public Symbol_Type smbl;		// symbol id
		public D155_Wpt_Type_Display_Mode dspl = D155_Wpt_Type_Display_Mode.dspl_smbl_name;		// display option

		public override byte[] toBytes()
		{
			// 2 bytes for pid and cnt
			int sizeBytesEval = 2 + 6 + 8 + 4 + 40 + 4 + 30 + 24 + 2 + 2 + 2 + 1 + 1 + 2 + 1;

			byte[] bytes = new byte[sizeBytesEval];

			int pos = 0;
			bytes[pos++] = (byte)m_pid; // Pid_Wpt_Data
			pos++;	// for cnt
			pos += toFixedStringBytes(ident, bytes, pos, 6);
			pos += posn.toBytes(bytes, pos);
			pos += toLongwordBytes(0, bytes, pos);
			pos += toFixedStringBytes(comment, bytes, pos, 40);
			pos += toFloatBytes(dst, bytes, pos);
			pos += toFixedStringBytes(name, bytes, pos, 30);
			pos += toFixedStringBytes(city, bytes, pos, 24);
			pos += toFixedStringBytes(state, bytes, pos, 2);
			pos += toIntBytes(alt, bytes, pos);
			pos += toFixedStringBytes(cc, bytes, pos, 2);
			bytes[pos++] = (byte)0;
			bytes[pos++] = (byte)wpt_class;
			bytes[pos++] = (byte)(((int)smbl) & 0xFF);
			bytes[pos++] = (byte)((((int)smbl) >> 8) & 0xFF);
			bytes[pos++] = (byte)dspl;

			bytes[1] = (byte)(pos - 2);	// now fill cnt

			return bytes;
		}
	}

	// The enumerated values for the “dspl” member of the D155_Wpt_Type are shown below:
	public enum D155_Wpt_Type_Display_Mode
	{
		dspl_smbl_only = 1,			// Display symbol by itself
		dspl_smbl_name = 3,			// Display symbol with waypoint name
		dspl_smbl_cmnt = 5,			// Display symbol with comment
	}

	// The enumerated values for the “wpt_class” member of the D155_Wpt_Type are shown below:
	public enum D155_Wpt_Type_Wpt_Class
	{
		apt_wpt_class    = 0,		// airport waypoint public class
		int_wpt_class    = 1,		// intersection waypoint public class
		ndb_wpt_class    = 2,		// NDB waypoint public class
		vor_wpt_class    = 3,		// VOR waypoint public class
		usr_wpt_class    = 4,		// user defined waypoint public class
		locked_wpt_class = 5		// locked waypoint public class
	}

	// ===============================  end of waypoints definitions =================================

	//	Example products: GPS 55 and GPS 55 AVD.
	// The route number contained in the D200_Rte_Hdr_Type must be unique for each route.
	public class D200_Rte_Hdr_Type : GPacket
	{
		public int nmbr = 1;			// byte nmbr;	   - route number

		public D200_Rte_Hdr_Type() : base((int)L001Pids.Pid_Rte_Hdr)	{}

		public override byte[] toBytes()
		{
			byte[] bytes = new byte[] { 0x0, 0x2, 0x0, 0x0 };

			bytes[0] = (byte)m_pid;
			bytes[2] = (byte)nmbr;
			return bytes;
		}
	}

	//	Example products: all products unless otherwise noted.
	public class D201_Rte_Hdr_Type : GPacket
	{
		public int nmbr = 1;			// byte nmbr;	   - route number
		public string cmnt = "";		// char[20] cmnt;  - comment

		public D201_Rte_Hdr_Type() : base((int)L001Pids.Pid_Rte_Hdr)	{}

		public override byte[] toBytes()
		{
			byte [] bytes = new byte[(cmnt.Length > 20 ? 20 : cmnt.Length) + 4];

			bytes[0] = (byte)m_pid;
			bytes[2] = (byte)nmbr;
			int cnt = toVarStringBytes(cmnt, bytes, 3, 20);
			bytes[1] = (byte)(cnt + 1);
			return bytes;
		}
	}

	// Example products: StreetPilot, eTrex Vista.
	public class D202_Rte_Hdr_Type : GPacket
	{
		public string rte_ident = "";		// char rte_ident[]; null-terminated string

		public D202_Rte_Hdr_Type() : base((int)L001Pids.Pid_Rte_Hdr)	{}

		public override byte[] toBytes()
		{
			byte [] bytes = new byte[(rte_ident.Length > 20 ? 20 : rte_ident.Length) + 3];

			bytes[0] = (byte)m_pid;
			int cnt = toVarStringBytes(rte_ident, bytes, 2, 20);
			bytes[1] = (byte)cnt;
			return bytes;
		}
	}

	//	Example products: GPSMAP 162/168, eMap, GPSMAP 295.
	public class D210_Rte_Link_Type : GPacket
	{
		public D210_Rte_Link_Type_Link_Class link_class;						// link public class; see below
		public byte[] subclass = new byte[18];		// sublcass
		// char ident[];		variable length string

		public D210_Rte_Link_Type() : base((int)L001Pids.Pid_Rte_Link_Data)	{}

		public override byte[] toBytes()
		{
			byte [] bytes = new byte[] { 0, 21, 3, 0,   0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0 };

			bytes[0] = (byte)m_pid;
			bytes[1] = (byte)(bytes.Length - 2);
			toWordBytes((word)link_class, bytes, 2);
			return bytes;
		}
	}

	//	The “public class” member can be one of the following values:
	public enum D210_Rte_Link_Type_Link_Class
	{
		line = 0,
		link = 1,
		net = 2,
		direct = 3,
		snap = 0xFF
	}

	//	Example products: all products unless otherwise noted.
	public class D300_Trk_Point_Type : GPacket
	{
		public Semicircle_Type posn;		// position
		public longword time;				// time
		public bool new_trk;				// new track segment?

		private static int m_day = 2;
		private static int m_month = 1;
		private static int m_year = 8888;

		private static int h = 0;
		private static int m = 0;
		private static int s = 0;
		private static int ms = 0;

		// returns zulu DateTime from "longword time" member:
		public DateTime toDateTime()
		{
			if(time == -1 || time == 0x7FFFFFFF)
			{
				// return DateTime.MinValue;		// this makes time just disappear in the datagrid, but works poorly with PDA

				// For PDA to operate correctly we better make a 1 sec increments and place track in year 8888, as MagellanProtoAppLayer does it.
				// The difference is that here we don't have even the time of the day, so we just construct it all.
				DateTime dt = new DateTime(m_year, m_month, m_day, h, m, s, ms);
				s++;
				if(s >= 60)
				{
					s = 0;
					m++;
					if(m >= 60)
					{
						m = 0;
						h++;
					}
				}
				return dt;
			}
			DateTime ret = new DateTime(1990, 1, 1, 0, 0, 0);
			return ret.AddSeconds(time).AddDays(-1.0d);
		}

		// there is no need to convert it to bytes, as it is never send to the device
	}

	// Example products: GPSMAP 162/168, eMap, GPSMAP 295.
	public class D301_Trk_Point_Type : D300_Trk_Point_Type
	{
		//public Semicircle_Type posn;		// position
		//public longword time;				// time - seconds, zulu, from 1990/1/1 00:00:00 
		public float alt;					// altitude in meters
		public float dpth;					// depth in meters
		//public bool new_trk;				// new track segment?

		// there is no need to convert it to bytes, as it is never send to the device
	}

	// Example products: GPSMAP 162/168, eMap, GPSMAP 295.
	public class D310_Trk_Hdr_Type : GPacket
	{
		public bool dspl;					// display on the map?
		public byte color;					// color (same as D108)
		public string trk_ident = "";		// char trk_ident[]; null-terminated string

		// there is no need to convert it to bytes, as it is never send to the device
	}

	// Example products: GPS 55 and GPS 75.
	public class D400_Prx_Wpt_Type
	{
		D100_Wpt_Type wpt;			// waypoint
		float dst;					// proximity distance (meters)
	}

	// Example products: GPS 12, GPS 12 XL and GPS 48.
	public class D403_Prx_Wpt_Type
	{
		D103_Wpt_Type wpt;			// waypoint
		float dst;					// proximity distance (meters)
	}

	// Example products: GPS 150, GPS 155, GNC 250 and GNC 300.
	public class D450_Prx_Wpt_Type
	{
		int idx;					// proximity index
		D150_Wpt_Type wpt;			// waypoint
		float dst;					// proximity distance (meters)
	}

	// Example products: GPS 38, GPS 40, GPS 45, GPS 55, GPS 75, GPS 95 and GPS II.
	public class D500_Almanac_Type
	{
		int wn;				// week number (weeks)
		float toa;			// almanac data reference time (s)
		float af0;			// clock correction coefficient (s)
		float af1;			// clock correction coefficient (s/s)
		float e;			// eccentricity (-)
		float sqrta;		// square root of semi-major axis (a) (m**1/2)
		float m0;			// mean anomaly at reference time (r)
		float w;			// argument of perigee (r)
		float omg0;			// right ascension (r)
		float odot;			// rate of right ascension (r/s)
		float i;			// inclination angle (r)
	}

	//	Example products: GPS 12, GPS 12 XL, GPS 48, GPS II Plus and GPS III.
	public class D501_Almanac_Type
	{
		int wn;				// week number (weeks)
		float toa;			// almanac data reference time (s)
		float af0;			// clock correction coefficient (s)
		float af1;			// clock correction coefficient (s/s)
		float e;			// eccentricity (-)
		float sqrta;		// square root of semi-major axis (a) (m**1/2)
		float m0;			// mean anomaly at reference time (r)
		float w;			// argument of perigee (r)
		float omg0;			// right ascension (r)
		float odot;			// rate of right ascension (r/s)
		float i;			// inclination angle (r)
		byte hlth;			// almanac health
	}

	//	Example products: GPS 150, GPS 155, GNC 250 and GNC 300.
	public class D550_Almanac_Type
	{
		char svid;			// satellite id
		int wn;				// week number (weeks)
		float toa;			// almanac data reference time (s)
		float af0;			// clock correction coefficient (s)
		float af1;			// clock correction coefficient (s/s)
		float e;			// eccentricity (-)
		float sqrta;		// square root of semi-major axis (a) (m**1/2)
		float m0;			// mean anomaly at reference time (r)
		float w;			// argument of perigee (r)
		float omg0;			// right ascension (r)
		float odot;			// rate of right ascension (r/s)
		float i;			// inclination angle (r)
	}

	// Example products: GPS 150 XL, GPS 155 XL, GNC 250 XL and GNC 300 XL.
	public class D551_Almanac_Type
	{
		char svid;			// satellite id
		int wn;				// week number (weeks)
		float toa;			// almanac data reference time (s)
		float af0;			// clock correction coefficient (s)
		float af1;			// clock correction coefficient (s/s)
		float e;			// eccentricity (-)
		float sqrta;		// square root of semi-major axis (a) (m**1/2)
		float m0;			// mean anomaly at reference time (r)
		float w;			// argument of perigee (r)
		float omg0;			// right ascension (r)
		float odot;			// rate of right ascension (r/s)
		float i;			// inclination angle (r)
		byte hlth;			// almanac health bits 17:24 (coded)
	}

	//  Example products: all products unless otherwise noted.
	public class D600_Date_Time_Type
	{
		byte month;			// month (1-12)
		byte day;			// day (1-31)
		word year;			// year (1990 means 1990)
		int hour;			// hour (0-23)
		byte minute;		// minute (0-59)
		byte second;		// second (0-59)
	}

	//  Example products: all products unless otherwise noted.
	//typedef Radian_Type D700_Position_Type;

	//	Example products: GPS III and StreetPilot.
	public class D800_Pvt_Data_Type : GPacket
	{
		public float alt;			// altitude above WGS 84 ellipsoid (meters)
		public float epe;			// estimated position error, 2 sigma (meters)
		public float eph;			// epe, but horizontal only (meters)
		public float epv;			// epe, but vertical only (meters)
		public int fix;				// type of position fix
		public double tow;			// time of week (seconds)
		public Radian_Type posn;	// latitude and longitude (radians)
		public float east;			// velocity east (meters/second)
		public float north;			// velocity north (meters/second)
		public float up;			// velocity up (meters/second)
		public float msl_hght;		// height of WGS 84 ellipsoid above MSL (meters)
		public int leap_scnds;		// difference between GPS and UTC (seconds)
		public long wn_days;		// week number days
	}

	// The enumerated values for the “fix” member of the D800_Pvt_Data_Type are shown below.
	public enum D800_Pvt_Data_Type_Fix_Type
	{
		unusable = 0,		// failed integrity check
		invalid = 1,		// invalid or unavailable
		_2D = 2,			// two dimensional
		_3D = 3,			// three dimensional
		_2D_diff = 4,		// two dimensional differential
		_3D_diff = 5		// three dimensional differential
	}
	#endregion
}
