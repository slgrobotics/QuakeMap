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
using System.Xml;

using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace QuakeMapApi
{
	#region class MappingEngineException

	/// <summary>
	/// MappingEngineException is thrown every time things go wrong connecting to QuakeMap.
	/// </summary>
	public class MappingEngineException : Exception
	{
		public MappingEngineException(string msg) : base(msg)
		{
		}
	}
	#endregion

	#region class QMApiLib

	/// <summary>
	/// QMApiLib encapsulates API calls to QuakeMap.
	/// </summary>
	public class QMApiLib
	{
		public string errorText = "";

		public QMApiLib()
		{
		}

		public void refresh()
		{
			CommandMappingEngine("api|refresh");
		}

		public void resetZoom()
		{
			CommandMappingEngine("api|resetzoom");
		}

		public void doZoom()
		{
			CommandMappingEngine("api|dozoom");
		}

		public void createTrack(string name, string source)
		{
			string cmd = "api|newtrk";

			cmd += "|" + name;
			cmd += "|" + "trk";
			cmd += "|" + source;

			CommandMappingEngine(cmd);
		}

		public void createRoute(string name, string source)
		{
			string cmd = "api|newtrk";

			cmd += "|" + name;
			cmd += "|" + "rte";
			cmd += "|" + source;

			CommandMappingEngine(cmd);
		}

		public void deleteTrackOrRoute(string name, string source)
		{
			string cmd = "api|deltrk";

			cmd += "|" + name;
			cmd += "|" + source;

			CommandMappingEngine(cmd);
		}

		public void createWaypoint(CreateInfo ci, bool keepInView)
		{
			string cmd = "api|newwpt";

			cmd += "|" + ci.lat;
			cmd += "|" + ci.lng;
			cmd += "|" + ci.elev;
			cmd += "|" + ci.dateTime;
			cmd += "|" + ci.name;
			cmd += "|" + ci.urlName;
			cmd += "|" + ci.type;
			cmd += "|" + ci.typeExtra;
			cmd += "|" + ci.sym;
			cmd += "|" + ci.id;
			cmd += "|" + ci.url;
			cmd += "|" + ci.desc;
			cmd += "|" + ci.source;
			cmd += "|" + keepInView;

			CommandMappingEngine(cmd);
		}

		/// <summary>
		/// delete waypoint by one of: name, urlName, source
		/// </summary>
		/// <param name="ci"></param>
		/// <returns></returns>
		public void deleteWaypoint(CreateInfo ci)
		{
			string cmd = "api|delwpt";

			cmd += "|" + ci.name;
			cmd += "|" + ci.urlName;
			cmd += "|" + ci.source;

			CommandMappingEngine(cmd);
		}

		public void createVehicle(double lng, double lat, double elev, DateTime dateTime, string name, string sym,
									string source, string url, string descr, bool keepInView, bool doTrackLog)
		{
			string cmd = "api|newvehicle";

			cmd += "|" + lat;
			cmd += "|" + lng;
			cmd += "|" + elev;
			cmd += "|" + dateTime;
			cmd += "|" + name;
			cmd += "|" + sym;
			cmd += "|" + url;
			cmd += "|" + source;
			cmd += "|" + descr;
			cmd += "|" + keepInView;
			cmd += "|" + doTrackLog;

			CommandMappingEngine(cmd);
		}

		public void moveVehicle(double lng, double lat, double elev, DateTime dateTime, string name, string sym,
									string source, string url, string descr, bool keepInView, bool doTrackLog)
		{
			string cmd = "api|movevehicle";

			cmd += "|" + lat;
			cmd += "|" + lng;
			cmd += "|" + elev;
			cmd += "|" + dateTime;
			cmd += "|" + name;
			cmd += "|" + sym;
			cmd += "|" + url;
			cmd += "|" + source;
			cmd += "|" + descr;
			cmd += "|" + keepInView;
			cmd += "|" + doTrackLog;

			CommandMappingEngine(cmd);
		}

		public void deleteVehicle(string name, string source)
		{
			string cmd = "api|delvehicle";

			cmd += "|" + name;
			cmd += "|" + source;

			CommandMappingEngine(cmd);
		}


		#region CommandMappingEngine

		private Thread m_discoveryThread = null;
		private int m_otherProcessesNumber = -1;
		private IntPtr otherProcessWindowHandle = IntPtr.Zero;

		/// <summary>
		/// this method tried to detect/talk to QuakeMap, ensuring that dangerous Process calls don't hang the current instance
		/// </summary>
		/// <param name="args"></param>
		public void CommandMappingEngine(string strCmd) 
		{
			bool result = false;
			bool triedDiscover = false;

		again:
			if(otherProcessWindowHandle == IntPtr.Zero)
			{
				triedDiscover = true;
				m_otherProcessesNumber = -1;

				m_discoveryThread =	new Thread( new ThreadStart(tryDiscoverQuakeMap));
				m_discoveryThread.IsBackground = true;	// terminate with the main process
				m_discoveryThread.Name = "Discovery";
				m_discoveryThread.Priority = ThreadPriority.BelowNormal;
				m_discoveryThread.Start();

				// now we either timeout in 4 seconds, or the discovery will deliver m_otherProcessesNumber >= 0
				int i = 0;
				for (; i < 10 ;i++)
				{
					Thread.Sleep(400);		// let the thread try discover/communicate to other instance
					if(m_otherProcessesNumber >= 0)
					{
						break;
					}
				}

				try
				{
					i = 0;
					while(m_discoveryThread.IsAlive && i++ < 10)
					{
						Trace.WriteLine("IP: aborting discovery thread");
						m_discoveryThread.Abort();
						Thread.Sleep(200);		// let the thread try discover/communicate to other instance
						Trace.WriteLine("IP: after aborting discovery thread, isAlive=" + m_discoveryThread.IsAlive);
					}
				}
				catch (Exception e)
				{
					Trace.WriteLine("Error: while aborting discovery thread: " + e.Message);
				}

				m_discoveryThread = null;
				System.GC.Collect();
				System.GC.WaitForPendingFinalizers();
			}

			if(m_otherProcessesNumber > 0 && otherProcessWindowHandle != IntPtr.Zero)
			{
				// we have QuakeMap process running, send a message:
				//Trace.WriteLine("Sending message to handle=" + otherProcessWindowHandle);
				try
				{
					//Trace.WriteLine("Sending Windows message '" + strCmd + "'");
					SendMsgString(otherProcessWindowHandle, strCmd);
					result = true;
				}
				catch (Exception e)
				{
					if(!triedDiscover)
					{
						otherProcessWindowHandle = IntPtr.Zero;
						goto again;
					}
					errorText = "Error: while sending message: " + e;
					throw new MappingEngineException(errorText);
				}
			}
			else
			{
				errorText = m_otherProcessesNumber == -1 ? "Error: QuakeMap discovery thread hung" : "Error: QuakeMap mapping engine not running";
				throw new MappingEngineException(errorText);
			}

			if(!result)
			{
				errorText = "Error communicating";
				throw new MappingEngineException(errorText);
			}
		}

		/// <summary>
		/// The Process related calls tend to hang, so we try to play it safely in a separate thread:
		/// </summary>
		private void tryDiscoverQuakeMap()
		{
			Process[] runningProcesses = null;
			Process otherProcess = null;
			string processName = "";
			int otherProcessesNumber = 0;
			
			try
			{
				// see discussion here: Hyperthreading http://www.dotnet247.com/247reference/msgs/35/178985.aspx
				// (related to GetProcessesByName() can't get process information from remote machine)

				Process currentProcess = Process.GetCurrentProcess();
				processName = "QuakeMap";
				runningProcesses = Process.GetProcessesByName(processName);
				otherProcessesNumber = 0;

				// process: QuakeMap   title: 'QuakeMap - [offline]   4m/pixel * 1.1875'  
				// process: quakemap   title: 'QuakeMap Installation'   - not counted
				// process: QuakeMap   title: ''  

				foreach (Process process in runningProcesses)
				{
					string isCounted = " - not counted";
					// don't count installation in process, which happens to be quakemap.exe too
					if(processName.ToLower().Equals(process.ProcessName.ToLower()))
					{
						otherProcess = process;
						otherProcessesNumber++;
						isCounted = "";
						otherProcessWindowHandle = process.MainWindowHandle;
					}
					//Trace.WriteLine("process: " + process.ProcessName + "   title: '" + process.MainWindowTitle + "'  " + isCounted);
				}

				if (otherProcessesNumber > 0) 
				{
					Trace.WriteLine("QuakeMap with process name '" + otherProcess.ProcessName + "'  id=" + otherProcess.Id + " is running.");
				}

				m_otherProcessesNumber = otherProcessesNumber;
				Trace.WriteLine("QuakeMap discovery thread delivered " + otherProcessesNumber);
			}
			catch (Exception e)
			{
				// HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PerfProc\Performance appeared key "Disable Performance Counters"=dword:00000001
				//  Set it to 0 and all worked. -- from http://www.gotdotnet.ru/Forums/CommonForum/23263.aspx
				
				Trace.WriteLine("Error: exception while in tryDiscoverOtherInstance(): " + e.Message);
			}
		}
		#endregion

		#region Windows messaging to pass strings between processes

		/// <summary>
		/// on the Windows messaging see http://www.dotnet247.com/247reference/msgs/31/159687.aspx
		/// </summary>

		struct COPYDATASTRUCT
		{
			public int dwData;
			public int cbData;
			public IntPtr lpData;
		}

		const int LMEM_FIXED = 0x0000;
		const int LMEM_ZEROINIT = 0x0040;
		const int LPTR = (LMEM_FIXED | LMEM_ZEROINIT);
		public const int WM_COPYDATA = 0x004A;
		public const int HWND_BROADCAST  = 0xffff;      
		public const int WM_USER = 0x400;

		[DllImport("kernel32.dll")]
		public static extern IntPtr LocalAlloc(int flag, int size);

		[DllImport("kernel32.dll")]
		public static extern IntPtr LocalFree(IntPtr p);

		[DllImport("user32",EntryPoint="SendMessage")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32",EntryPoint="PostMessage")]
		public static extern bool PostMessage(IntPtr hwnd,int msg,int wparam,int lparam);

		[DllImport("user32",EntryPoint="RegisterWindowMessage")]
		public static extern int RegisterWindowMessage(string msgString);

		//Create wrappers for the memory API's similar to
		//Marshal.AllocHGlobal and Marshal.FreeHGlobal

		private static IntPtr AllocHGlobal(int cb)
		{
			IntPtr hMemory = new IntPtr();
			hMemory = LocalAlloc(LPTR, cb);
			return hMemory;
		}

		private static void FreeHGlobal(IntPtr hMemory)
		{
			if (hMemory != IntPtr.Zero)
			{
				LocalFree(hMemory);
			}
		}

		public static void SendMsgString(IntPtr hWndDest, string sMsg)
		{
			COPYDATASTRUCT oCDS = new COPYDATASTRUCT();
			oCDS.cbData = (sMsg.Length + 1) * 2;
			oCDS.lpData = LocalAlloc(0x40, oCDS.cbData);
			Marshal.Copy(sMsg.ToCharArray(), 0, oCDS.lpData, sMsg.Length);
			oCDS.dwData = 1;
			IntPtr lParam = AllocHGlobal(oCDS.cbData);
			Marshal.StructureToPtr(oCDS, lParam,false);
			SendMessage(hWndDest, WM_COPYDATA, IntPtr.Zero, lParam);
			LocalFree(oCDS.lpData);
			FreeHGlobal(lParam);
		}
		#endregion

	}
	#endregion

	#region class CreateInfo

	public class CreateInfo
	{
		// generic often required fields to be passed to insert functions:
		public long		id = -1;			// usually track id, for creating one or relating to one
		public string	type = null;		// object type, something like "eq", "wpt", "trkpt", "rtept", "trk", "rte"
		public string	typeExtra = null;	// something like "Geocache|Traditional Cache" or "Geocache" or "Geocache Found" - <type> tag in GPX
		public double	lat = 0.0d;
		public double	lng = 0.0d;
		public double	elev = 0.0d;
		public double	magn = 0.0d;
		public DateTime dateTime = DateTime.MinValue;	// always zulu
		public string	name = null;		// shortest possible, as it will be often shown on map (by user's choice) - like GCSDXYZ
		public string	urlName = null;		// actually a longer name, may be quite descriptive	- like "Mike's Memorial Cache"
		public string	url = null;
		public string	desc = null;		// description, sometimes including urlName or name too
		public string	comment = null;		// comment, "<cmt>" tag in GPX
		public string	sym = null;			// symbol to show on map
		public string	source = null;		// file or device (gps) name

		// some exotic cases that don't fit right into the generic fields above:
		public string	par1 = null;		// just in case, used for route number so far
		public XmlNode	node1 = null;		// for passing portions of XML documents to be parsed inside the "insert" functions; i.e. groundspeak extensions.
		public XmlNode	node2 = null;

		public CreateInfo()
		{
		}

		/// <summary>
		/// Clone constructor is needed for Magellan GetRoutes, when CreateInfo instances are buffered
		/// </summary>
		/// <param name="ci"></param>
		public CreateInfo(CreateInfo ci)
		{
			id = ci.id;
			type = ci.type;
			typeExtra = ci.typeExtra;
			lat = ci.lat;
			lng = ci.lng;
			elev = ci.elev;
			magn = ci.magn;
			dateTime = ci.dateTime;
			name = ci.name;
			urlName = ci.urlName;
			url = ci.url;
			desc = ci.desc;
			comment = ci.comment;
			sym = ci.sym;
			source = ci.source;
			par1 = ci.par1;
			node1 = ci.node1;	// ref
			node2 = ci.node2;	// ref
		}

		/// <summary>
		/// CreateInfo is often reused, and init() is designed to make it like new
		/// </summary>
		public void init(string _type)
		{
			id = -1;
			type = _type;
			typeExtra = null;
			lat = 0.0d;
			lng = 0.0d;
			elev = 0.0d;
			magn = 0.0d;
			dateTime = DateTime.MinValue;
			name = null;
			urlName = null;
			url = null;
			desc = null;
			comment = null;
			sym = null;
			source = null;
			par1 = null;
			node1 = null;
			node2 = null;
		}

		/// <summary>
		/// set lat from a string in all possible ways (-23.234 or 33.6033N)
		/// </summary>
		/// <param name="sLat"></param>
		public void setLat(string sLat)
		{
			sLat = sLat.ToLower().Trim();
			bool isSouth = false;
			if(sLat.EndsWith("n") || (isSouth=sLat.EndsWith("s"))) 
			{
				lat = Convert.ToDouble(sLat.Substring(0, sLat.Length - 1));
				if(isSouth) 
				{
					lat = -lat;
				}
			} 
			else 
			{
				lat = Convert.ToDouble(sLat);
			}
		}

		/// <summary>
		/// set lng from a string in all possible ways (-117.234 or 117.6033W)
		/// </summary>
		/// <param name="sLng"></param>
		public void setLng(string sLng)
		{
			sLng = sLng.ToLower().Trim();
			bool isEast = false;
			if(sLng.EndsWith("w") || (isEast=sLng.EndsWith("e"))) 
			{
				lng = Convert.ToDouble(sLng.Substring(0, sLng.Length - 1));
				if(isEast) 
				{
					lng = -lng;
				}
			} 
			else 
			{
				lng = Convert.ToDouble(sLng);
			}
		}

		public void setElev(string sElev)
		{
			try 
			{
				elev = Convert.ToDouble(sElev.Trim());
			} 
			catch
			{
				elev = 0.0d;
			}
		}

		public void setDateTime(string sDateTime)
		{
			sDateTime = sDateTime.Trim();
			if(sDateTime.EndsWith("Z"))		// UTC (zulu) time is supposed to be there, as came from GPS or .gpx file
			{
				// deceive Convert into thinking that it is local timezone (by removing Z at the end)
				string tmp = sDateTime.Substring(0, sDateTime.Length - 1);
				dateTime = Convert.ToDateTime(tmp);
				// now we have zulu time in dateTime
			} 
			else
			{
				// not supposed to happen for waypoints
				dateTime = Convert.ToDateTime(sDateTime);		// zulu
			}
		}

	}
	#endregion
}
