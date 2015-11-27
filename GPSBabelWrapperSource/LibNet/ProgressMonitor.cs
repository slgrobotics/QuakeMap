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
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Threading;

using LibNet;
using LibSys;

namespace LibNet
{
	public class ProgressMonitor
	{
		private static Thread m_monitorThread = null;
		public static SortedList List = new SortedList();
		public static Button Indicator = null;
		public static ProgressMonitorForm Display = null;

		/* DEBUG
		static ProgressMonitor()
		{
			Monitored mm = new Monitored();
			mm.Progress = 20;
			addMonitored(mm);
			mm = new Monitored();
			mm.Progress = 40;
			addMonitored(mm);
			mm = new Monitored();
			mm.Progress = 80;
			addMonitored(mm);
		}
		// end DEBUG			*/

		static ProgressMonitor()
		{
			// we better have this thread separate and real (not pooled) as it has to exist 
			// throughout  application's life:
			m_monitorThread = new Thread( new ThreadStart(RunMonitor));
			m_monitorThread.Name = "Monitor";
			m_monitorThread.IsBackground = true;	// terminate with the application
			m_monitorThread.Priority = ThreadPriority.Lowest;
			m_monitorThread.Start();
		}

		~ProgressMonitor()
		{
			if(m_monitorThread != null)
			{
				m_monitorThread.Abort();
			}
		}

		private static void RunMonitor()
		{
			Thread.Sleep(2000);
			while(!Project.goingDown)
			{
				try
				{
					LibSys.StatusBar.AllSync();
					purge();
					Thread.Sleep(400);
				} 
				catch
				{
				}
			}
		}

		public static DateTime addMonitored(Monitored mm)
		{
			DateTime key = DateTime.Now;
			lock(List)
			{
				while(true)
				{
					try 
					{
						List.Add(key, mm);
						mm.Started = key;
						if(Display != null)
						{
							try
							{
								Display.RebuildGui();		// thread safe method, can call here
							}
							catch
							{
							}
						}
						break;
					} 
					catch
					{
						key = key.AddTicks(-1);
					}
				}
			}
			return key;
		}

		public static void markComplete(Monitored mm, bool success, string comment)
		{
			if(mm != null)
			{
				mm.Complete = true;
				mm.Finished = DateTime.Now;
				mm.Success = success;
				if(comment != null)
				{
					mm.Comment = comment + " " + mm.Comment;
				}

				WorkValues();	// make sure values are updated on display
			}
		}

		static int count = 0;

		// called 3 times per second; can throw exceptions:
		public static void purge()
		{
			int minValue = 100;
			bool hasFailed = false;

			int secsToHold = 30;
			if(List.Count > 20)
			{
				secsToHold = 0;	// with way too many monitors in effect, holding complete ones is too expensive
			} else if(List.Count > 10)
			{
				secsToHold = 2;	// with many monitors in effect, holding complete ones for long time is too expensive
			}
			lock(List)
			{
				// purge the list - remove old completed entries
				for(int i=List.Count-1; i >=0 ; i--)
				{
					Monitored mm = (Monitored)List.GetByIndex(i);
					if(mm.Complete && (secsToHold == 0 || mm.Finished.AddSeconds(secsToHold).CompareTo(DateTime.Now) < 0))
					{
						List.Remove(mm.Started); //.RemoveAt(i);
						mm.Purged = true;
						continue;
					}
					else if(!mm.Complete && mm.Progress > 0 && mm.Progress < minValue)
					{
						minValue = mm.Progress;
					}

					if(mm.Complete && !mm.Success)
					{
						hasFailed = true;
					}
				}
			}

			if(Display != null)
			{
				Display.purge();		// thread safe method, can call here
				if(doWorkValues || ++count > 10)	// force periodic update, avoid stale values
				{
					doWorkValues = false;
					Display.WorkValues();		// thread safe method, can call here
					if(count > 10)
					{
						count = 0;
					}
				}
				Display.MayRefresh();		// thread safe method, can call here
			}

			if(Indicator != null)
			{
				int countActive = CountActive();

				if(countActive > 0)
				{
					Indicator.BackColor = hasFailed ? Color.Red : Color.Green;
					if(minValue > 0 && minValue < 100)
					{
						Indicator.Text = "" + minValue + "%";
						LibSys.StatusBar.Progress(minValue);
					}
					else if(countActive > 0)
					{
						Indicator.Text = "" + countActive + "+" + ThreadPool2.WaitingCallbacks; //Project.threadPool.RequestQueueCount;
						LibSys.StatusBar.ProgressOff();
					}
				}
				else
				{
					Indicator.BackColor = Color.LightGray;
					Indicator.Text = "";
					LibSys.StatusBar.ProgressOff();
				}
			}
		}

		private static bool doWorkValues = false;

		// usually called in progress callbacks after monitored values have been changed,
		public static void WorkValues()
		{
			doWorkValues = true;	// will be processed in the next purge()
		}

		public static int CountActive()
		{
			int ret = 0;
			for(int i=0; i < List.Count ; i++)
			{
				Monitored mm = (Monitored)List.GetByIndex(i);
				if(!mm.Complete)
				{
					ret++;
				}
			}
			return ret;
		}
	}
}
