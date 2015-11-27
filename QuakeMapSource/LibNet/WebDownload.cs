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
using System.Net;
using System.IO;
using System.Threading;

using LibSys;

// code by Ray Hayes http://codeproject.com/csharp/WebDownload.asp

namespace LibNet
{
	public delegate void DownloadProgressHandler(DownloadInfo info);

	// The RequestState class passes data across async calls.
	public class DownloadInfo
	{
		public byte[] BufferRead;

		public bool useFastBuffers;
		public byte[] dataBufferFast;
		public System.Collections.ArrayList dataBufferSlow;

		public bool is404 = false;
		public bool hasTimedOut = false;
		public int dataLength;
		public int bytesProcessed;
		public string baseName;
		public string strUrl;

		public HttpWebRequest Request;
		public Stream ResponseStream;

		public DownloadProgressHandler ProgressCallback;
		public AddMonitoredMethod addMonitoredMethod;
		public Monitored monitored = null;

		public DownloadInfo()
		{
			BufferRead = new byte[WebDownload.BUFFER_SIZE];
			Request = null;
			dataLength = -1;
			bytesProcessed = 0;

			useFastBuffers = true;
		}
	}

	// ClientGetAsync issues the async request.
	public class WebDownload
	{
		public static ManualResetEvent allDoneLast = null;

		public ManualResetEvent allDone = new ManualResetEvent(false);
		public const int BUFFER_SIZE = 16384;	// 1024;
		private int m_timeOutMs = 0;			// 0=defined by underlying TCP stack
		protected string m_baseName = "";

		public WebDownload(int timeOutMs)
		{
			m_timeOutMs = timeOutMs;
		}

		public byte[] Download( DownloadInfo info, DownloadProgressHandler progressCallback )
		{
			//LibSys.StatusBar.Trace("IP: downloading - " + info.baseName + " == " + info.strUrl);

			allDoneLast = allDone;

			// Ensure flag set correctly.
			allDone.Reset();
			m_baseName = info.baseName;

			// Get the URI from the command line.
			Uri httpSite = new Uri(info.strUrl);

			// Create the request object.
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpSite);
			if(Project.suspendKeepAlive)
			{
				req.KeepAlive = false;
			}

			/*
			 * see Project.ApplyGlobalHTTPProxy()
			 * 
			if(Project.useProxy)
			{
				WebProxy proxy = new WebProxy(Project.proxyServer, Project.proxyPort);
				req.Proxy = proxy;
			}
			*/
			
#if DEBUG
			LibSys.StatusBar.Trace("IP: downloading - " + info.baseName + " == " + info.strUrl + "   after proxy: " + req.Proxy.GetProxy(httpSite));
#endif

			// Put the request into the state object so it can be passed around.
			info.Request = req;

			// Assign the callbacks
			info.ProgressCallback += progressCallback;

			/*
			 // this is to debug ThreadPool and ProgressMonitor 
			if(info.strUrl.IndexOf("ashx") != -1)
			{
				info.dataLength = 20;
				int ccc = 0;
				while (ccc++ < 20)
				{
					Thread.Sleep(500);
					if ( info.ProgressCallback != null )
					{
						//LibSys.StatusBar.Trace("IP: loading... " + info.baseName);
						info.bytesProcessed = ccc;
						info.ProgressCallback(info);
					}
				}
				LibSys.StatusBar.Trace("OK: finished " + info.baseName);
				return null;
			}
			*/

			// Issue the async request.
			IAsyncResult r = (IAsyncResult) req.BeginGetResponse(new AsyncCallback(ResponseCallback), info);

			// Wait until the ManualResetEvent is set so that the application
			// does not exit until after the callback is called.
			bool hasSignal;
			if(m_timeOutMs == 0)
			{
				hasSignal = allDone.WaitOne();
			}
			else
			{
#if DEBUG
				//LibSys.StatusBar.Trace("IP: WebDownload:Download() at    WaitOne - " + DateTime.Now + " - " + m_baseName + " " + info.strUrl);
#endif
				hasSignal = allDone.WaitOne(m_timeOutMs, false);
#if DEBUG
				//LibSys.StatusBar.Trace("IP: WebDownload:Download() after WaitOne - " + DateTime.Now + " - " + hasSignal + " " + m_baseName + " " + info.strUrl);
#endif
			}

			if(!hasSignal)
			{
				allDone.Set();
				info.hasTimedOut = true;
				return null;
			}

			// Pass back the downloaded information.

			if ( info.useFastBuffers )
			{
				return info.dataBufferFast;
			}
			else
			{
				byte[] data = new byte[ info.dataBufferSlow.Count ];
				for ( int b=0; b<info.dataBufferSlow.Count; b++ )
				{
					data[b] = (byte) info.dataBufferSlow[b];
				}
				return data;
			}
		}

		private void ResponseCallback(IAsyncResult ar)
		{
			// Get the DownloadInfo object from the async result were
			// we're storing all of the temporary data and the download
			// buffer.
			DownloadInfo info = (DownloadInfo) ar.AsyncState;
			//LibSys.StatusBar.Trace("IP: WebDownload:ResponseCallback() - " + info.baseName + " " + info.strUrl);

			// Get the WebRequest from RequestState.
			HttpWebRequest req = info.Request;

			try 
			{
				// Call EndGetResponse, which produces the WebResponse object
				// that came from the request issued above.
				WebResponse resp = req.EndGetResponse(ar);

				// Find the data size from the headers.
				string strContentLength = resp.Headers["Content-Length"];
				//LibSys.StatusBar.Trace("IP: WebDownload:ResponseCallback() - ContentLength=" + strContentLength);
				if ( strContentLength != null )
				{
					info.dataLength = Convert.ToInt32( strContentLength );
					info.dataBufferFast = new byte[ info.dataLength ];
				}
				else
				{
					info.useFastBuffers = false;
					info.dataBufferSlow = new System.Collections.ArrayList( BUFFER_SIZE );
				}

				//  Start reading data from the response stream.
				Stream ResponseStream = resp.GetResponseStream();

				// Store the response stream in RequestState to read
				// the stream asynchronously.
				info.ResponseStream = ResponseStream;

				// we are about to actually read bytes from the pipe.

				//  Pass do.BufferRead to BeginRead.
				IAsyncResult iarRead = ResponseStream.BeginRead(info.BufferRead, 
					0,
					BUFFER_SIZE, 
					new AsyncCallback(ReadCallBack), 
					info);
				//LibSys.StatusBar.Trace("IP: WebDownload:ResponseCallback() " + info.baseName + " " + info.strUrl + " done");
			} 
			catch (WebException we)
			{
				if(we.Message.IndexOf("404") >= 0)
				{
					info.is404 = true;	// signal that there is no need for more tries
				}
				else
				{
					LibSys.StatusBar.Trace("Error: WebDownload:ResponseCallback " + info.baseName + " " + info.strUrl + " Web exception " + we.Message);
				}
				allDone.Set();
			} 
			catch (Exception e)
			{
				LibSys.StatusBar.Trace("Error: WebDownload:ResponseCallback " + info.baseName + " " + info.strUrl + " exception " + e.Message);
				allDone.Set();
			}
		}

		private void ReadCallBack(IAsyncResult asyncResult)
		{
			// Get the DownloadInfo object from AsyncResult.
			DownloadInfo info = (DownloadInfo) asyncResult.AsyncState;

			// Retrieve the ResponseStream that was set in RespCallback.
			Stream responseStream = info.ResponseStream;

			try 
			{
				// Read info.BufferRead to verify that it contains data.
				int bytesRead = responseStream.EndRead( asyncResult );
				if (bytesRead > 0)
				{
					if ( info.useFastBuffers )
					{
						System.Array.Copy(info.BufferRead, 0, 
							info.dataBufferFast, info.bytesProcessed, 
							bytesRead );
					}
					else
					{
						for ( int b=0; b<bytesRead; b++ )
							info.dataBufferSlow.Add( info.BufferRead[b] );
					}
					info.bytesProcessed += bytesRead;
					//LibSys.StatusBar.Trace("IP: WebDownload:ReadCallBack() - " + info.baseName + " " + info.strUrl + " bytesRead=" + bytesRead);

					// If a registered progress-callback, inform it of our download progress so far.
					if ( info.ProgressCallback != null )
					{
						info.ProgressCallback(info);
					}

					// Continue reading data until responseStream.EndRead returns –1.
					IAsyncResult ar = responseStream.BeginRead(
						info.BufferRead, 0, BUFFER_SIZE,
						new AsyncCallback(ReadCallBack), info);
				}
				else
				{
					responseStream.Close();
					allDone.Set();
				}
			} 
			catch (Exception e)
			{
				LibSys.StatusBar.Trace("Error: WebDownload:ResponseCallback " + info.baseName + " " + info.strUrl + " exception " + e.Message);
				responseStream.Close();
				allDone.Set();
			}
			return;
		}

		~WebDownload()
		{
			//LibSys.StatusBar.Trace("OK: ~WebDownload() " + m_baseName);
			// allow thread to terminate:
			try
			{
				// may throw a "cannot access disposed object" in debugging
				if(allDone != null)
				{
					allDone.Set();
					allDoneLast = null;
				}
			} 
			catch {}
		}
	}
}
