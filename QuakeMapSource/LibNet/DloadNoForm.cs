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
using System.IO;
using System.Collections;
using System.Threading;

using LibSys;

namespace LibNet
{
	/// <summary>
	/// same functionality as DloadProgressForm, but never brings up a dialog. Quietly quits on failure.
	/// does not require ShowDialog().
	/// </summary>
	public class DloadNoForm
	{
		bool m_completed = false;
 
		public DloadNoForm(string dloadUrl, string dloadFileName, int msToWait)
		{
			// load file:
			DownloadThread dt = new DownloadThread();
			dt.DownloadUrl = dloadUrl;
			dt.tile = this;
			dt.baseName = "dld-quiet";
			dt.fileName = dloadFileName;
			dt.CompleteCallback += new DownloadCompleteHandler( completeCallback );

			//add dt worker method to the thread pool / queue a task 
			//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), dloadUrl); 
			ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), dloadUrl); 

#if DEBUG
			LibSys.StatusBar.Trace("OK: DloadNoForm:DloadNoForm() - loading remote from " + dloadUrl);
#endif

			// wait no more than specified time for delivery:
			if(msToWait > 0)
			{
				DateTime started = DateTime.Now;
				while(!m_completed && (DateTime.Now - started).Milliseconds < msToWait)
				{
					Thread.Sleep(100);
				}
			}
		}

		private void completeCallback ( object formObj, DownloadInfo info, string dldFileName, byte[] dataDownloaded )
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: DloadNoForm:completeCallback() - " + info.baseName + " " + (dataDownloaded == null? "null" : "" + dataDownloaded.Length) + " bytes loaded");
#endif
			if(dataDownloaded == null || Project.is404(dataDownloaded)) 
			{
				string message = "failed: " + (dataDownloaded == null ? "no connection or no data" : "404 - file not found");
				LibSys.StatusBar.Error(message);
			}
			else
			{
				FileStream fs = null;
				try 
				{
					fs = new FileStream(dldFileName, FileMode.Create);
					fs.Write(dataDownloaded, 0, dataDownloaded.Length);
					fs.Close();
					fs = null;
					LibSys.StatusBar.Trace("OK: file " + dldFileName + " created");
				} 
				catch (Exception e) 
				{
					string message = "failed: " + dldFileName + " - " + e.Message;
					LibSys.StatusBar.Error(message);
				}
				finally
				{
					if(fs != null)
					{
						fs.Close();
					}
				}
			}
			m_completed = true;
		}
	}
}
