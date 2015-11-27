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

namespace LibSys
{
	public class MyCache : Hashtable
	{
		/// <summary>
		/// marks cacheable element as unused
		/// </summary>
		/// <param name="baseName"></param>
		public void MarkUnused(string baseName)
		{
			//LibSys.StatusBar.Trace("OK: MyCache:MarkUnused() - tile '" + baseName + "'");

			if(this.Contains(baseName)) 
			{
				MyCacheable mc = (MyCacheable)this[baseName];
				if(mc != null) 
				{
					mc.MarkUnused();
				} 
				else 
				{
					// non-existent object behind the name, just remove the name:
					this.Remove(baseName);
				}
			}
		}

		// this is mainly for the full reload/refresh:
		public override void Clear()
		{
			try 
			{
				// all bitmaps and features need to be disposed before they are removed from cache.
				// disposing of bitmaps releases files from which they were loaded.
				foreach (IDisposable idisp in this.Values)
				{
					idisp.Dispose();
				}
				base.Clear();
			}
			catch {}
		}

		/// <summary>
		/// remove old unused elements from cache
		/// </summary>
		/// <param name="cacheMinCount"></param>
		public void purge(int cacheMinCount)
		{
			//LibSys.StatusBar.Trace("IP: MyCache:purge()");

			SortedList list = new SortedList(); // will contain ticks-based string keys, oldest first:
			int cnt = countUnused();
			if(cnt <= cacheMinCount) 
			{
				//LibSys.StatusBar.Trace("    too few unused yet: " + cnt);
			} 
			else 
			{
				int toTrim = cnt - cacheMinCount;
				foreach(MyCacheable mc in this.Values) 
				{
					if(mc != null && !mc.IsUsed) 
					{
						long lKey = mc.LastUsed;
						while(list.Contains(lKey))	// SortedList doesn't tolerate repeated keys 
						{
							lKey--;
						}
						list.Add(lKey, mc.BaseName);
						if(--toTrim <= 0) 
						{
							break;
						}
					}
				}
				for(int i=0; i < list.Count ;i++) 
				{
					string key = (string)list.GetByIndex(i);
					//LibSys.StatusBar.Trace("      OK: removing " + this[key]);
					// all bitmaps and features need to be disposed before they are removed from cache.
					// disposing of bitmaps releases files from which they were loaded.
					MyCacheable mc = (MyCacheable)this[key];
					mc.Dispose();
					this.Remove(key);
				}
			}
		}

		public int countUnused()
		{
			int ret = 0;
			foreach(MyCacheable mc in this.Values) 
			{
				if(mc != null && !mc.IsUsed) 
				{
					ret += 1;
				}
			}
			return ret;
		}

	}
}
