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
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using LibSys;
using LibNet;

namespace Wrapper
{
	/// <summary>
	/// Summary description for WebsiteInfo.
	/// </summary>
	public class WebsiteInfo
	{
		public static bool HasReachedServer = false;	// assume no connection

		public static event DownloadProgressHandler ProgressCallback;	// needed for WebDownload, stays empty here

		public static void init(string cgibinptrUrl)
		{
			try 
			{
#if DEBUG
				LibSys.StatusBar.Trace("IP: reaching cgibinptr URL='" + cgibinptrUrl + "'");
#endif

				WebDownload webDL = new WebDownload(Project.webTimeoutMs);		// default 7 seconds to time out
				// Create the state object.
				DownloadInfo info = new DownloadInfo();
				info.baseName = "";
				info.strUrl = cgibinptrUrl;
				info.addMonitoredMethod = null;

				byte[] downloadedData = null;
				int tries = 1;
				int maxTries = 1;
				while (tries <= maxTries && downloadedData == null && !info.is404)
				{
					downloadedData = webDL.Download(info, ProgressCallback);		// will timeout 
#if DEBUG
					if(downloadedData != null) 
					{
						LibSys.StatusBar.Trace("IP: try " + tries + " WebsiteInfo:Download() - " + cgibinptrUrl + " delivered " + downloadedData.Length + " bytes" );
					} 
					else 
					{
						LibSys.StatusBar.Trace("IP: try " + tries + " WebsiteInfo:Download() - " + cgibinptrUrl + " delivered null bytes" );
					}
#endif
					tries++;
				}

				if(downloadedData == null || downloadedData.Length == 0)
				{
					LibSys.StatusBar.Error("failed to reach QuakeMap.com");
					return;		// Project.serverAvailable will be set to false, and no web attempts will take place.
				}
				LibSys.StatusBar.Trace("OK: reached QuakeMap.com");

				string responseString = Project.ByteArrayToStr(downloadedData);
				StringReader reader = new StringReader (responseString);
				
				string upgrVersion = "";
				bool ignoreUpgrade = false;
				int state = 0;
				string line;
				while((line=reader.ReadLine()) != null) 
				{
					try
					{
						switch(state)
						{
							case 0:
								if(line.StartsWith("GBWABOUT=")) 
								{
									Project.ABOUT_URL = line.Substring("GBWABOUT=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: WebsiteInfo() - about='" + Project.ABOUT_URL + "'");
#endif
									HasReachedServer = true;
								}
								else if(line.StartsWith("GBWDLOAD=")) 
								{
									Project.DLOAD_URL = line.Substring("GBWDLOAD=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: WebsiteInfo() - download='" + Project.DLOAD_URL + "'");
#endif
								}
								else if(line.StartsWith("GBWUPDATE=")) 
								{
									Project.UPDATE_URL = line.Substring("GBWUPDATE=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: WebsiteInfo() - update='" + Project.UPDATE_URL + "'");
#endif
								}
								else if(line.StartsWith("GBWHELP=")) 
								{
									Project.HELP_FILE_URL = line.Substring("GBWHELP=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: WebsiteInfo() - help='" + Project.HELP_FILE_URL + "'");
#endif
								}
								else if(line.StartsWith("GBWHDATE=")) 
								{
									string sDate = line.Substring("GBWHDATE=".Length);
									Project.HELP_FILE_DATE = Convert.ToDateTime(sDate);
#if DEBUG
									LibSys.StatusBar.Trace("OK: WebsiteInfo() - helpFileDate='" + Project.HELP_FILE_DATE + "'");
#endif
								}
								else if(line.StartsWith("GBWGPSBABELHOME=")) 
								{
									Project.GPSBABEL_URL = line.Substring("GBWGPSBABELHOME=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: WebsiteInfo() - GPSBabel Home='" + Project.GPSBABEL_URL + "'");
#endif
								}
								else if(line.StartsWith("GBWMISC=")) 
								{
									Project.MISC_FOLDER_URL = line.Substring("GBWMISC=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: WebsiteInfo() - misc='" + Project.MISC_FOLDER_URL + "'");
#endif
								}
								else if(line.StartsWith("MESSAGE=")) 
								{
									state = 1;
									Project.serverMessage = "";
								}
								else if(line.StartsWith("UPGR")) 
								{
									upgrVersion = line.Substring(4);
									if(Project.PROGRAM_VERSION_RELEASEDATE.Equals(upgrVersion))
									{
										ignoreUpgrade = true;
									}
									else
									{
										Project.upgradeMessage = "\n";
									}
									state = 2;
								}
								break;
							case 1:
								if(line.StartsWith("ENDMESSAGE")) 
								{
									state = 0;
								}
								else 
								{
									Project.serverMessage += (line + "\n");
								}
								break;
							case 2:
								if(line.StartsWith("ENDUPGR")) 
								{
									state = 0;
								}
								else 
								{
									if(!ignoreUpgrade)
									{
										Project.upgradeMessage += (line + "\n");
									}
								}
								break;
						}
					}
					catch {}
				}

				if(!Project.serverMessageLast.Equals(Project.serverMessage) && Project.upgradeMessage.Length == 0)
				{
					string message = Project.serverMessage; // + Project.upgradeMessage;   upgrade message shows up in MainForm
					LibSys.StatusBar.Trace(message);
					Project.MessageBox(null, message);
					Project.serverMessageLast = Project.serverMessage;
				}
			} 
			catch (Exception e)
			{
				LibSys.StatusBar.Error("exception: " + e.Message);
			}
		}
	}
}
