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

// code by Ray Hayes http://codeproject.com/csharp/WebDownload.asp

namespace LibNet
{
	public delegate void DownloadCompleteHandler( object otile, DownloadInfo info, string fileName, byte[] dataDownloaded );

	/// <summary>
	/// Summary description for DownloadThread.
	/// </summary>
	public class DownloadThread
	{
		public event DownloadCompleteHandler CompleteCallback;
		public event DownloadProgressHandler ProgressCallback;
		public AddMonitoredMethod addMonitoredMethod = null;

		public static int maxTries = 3;

		private string _downloadUrl = "";
		public string DownloadUrl {	get	{ return _downloadUrl; } set {	_downloadUrl = value; }	}
		private object _tile;
		public object tile { get { return _tile; } set { _tile = value; } }
		private string _baseName;
		public string baseName { get { return _baseName; } set { _baseName = value; } }
		private string _fileName;
		public string fileName { get { return _fileName; } set { _fileName = value; } }

		public static int threadCount = 0;

		//public void Download(object state, DateTime requestEnqueueTime )	// good for LibSys.ThreadPool
		public void Download(object state )	// good for LibSys.ThreadPool2
		{
			//LibSys.StatusBar.Trace("IP: " + threadCount + " DownloadThread:Download() - " + DownloadUrl);
			if ( CompleteCallback != null && DownloadUrl != "" )
			{
				threadCount++;
				WebDownload webDL = new WebDownload(0);		// no timeout, except for natural TCP/IP stack one.
				// Create the state object.
				DownloadInfo info = new DownloadInfo();
				info.baseName = _baseName;
				info.strUrl = _downloadUrl;
				info.addMonitoredMethod = addMonitoredMethod;	// may be null

				// Make sure progress monitor is created:
				if(info.addMonitoredMethod != null)
				{
					info.monitored = new Monitored();
					info.monitored.Comment = info.strUrl;
					info.addMonitoredMethod(info.monitored);
				}

				byte[] downloadedData = null;
				if(Project.serverAvailable)
				{
					int tries = 1;
					while (true)
					{
						//try 
						//{
						downloadedData = webDL.Download(info, ProgressCallback);
						//} 
						//catch (Exception e)
						//{
						//	LibSys.StatusBar.Error("try " + tries + " DownloadThread:Download() - " + DownloadUrl + " exception " + e.Message );
						//}
#if DEBUG
						if(downloadedData != null) 
						{
							LibSys.StatusBar.Trace("IP: try " + tries + " DownloadThread:Download() - " + DownloadUrl + " delivered " + downloadedData.Length + " bytes" );
						} 
						else 
						{
							LibSys.StatusBar.Trace("IP: try " + tries + " DownloadThread:Download() - " + DownloadUrl + " delivered null bytes" );
						}
#endif
						tries++;
						if(tries <= maxTries && downloadedData == null &&  !info.is404)
						{
							// if there was a server error (Web Exception, error 500), give it some time to get in shape before retrying:
							Thread.Sleep(3000);
						}
						else
						{
							break;
						}
					}
				}
				threadCount--;
				CompleteCallback( _tile, info, _fileName, downloadedData );
			}
		}
	}
}
