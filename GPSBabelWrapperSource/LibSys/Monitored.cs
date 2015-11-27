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

namespace LibSys
{
	// class Monitored is used to keep track of downloads and show their progress:

	public delegate DateTime AddMonitoredMethod(Monitored mm);

	public class Monitored
	{
		public DateTime Started;
		public DateTime Finished = DateTime.MaxValue;
		public int Progress = 0;		// preferably 0 to 100
		public bool Complete = false;
		public bool Purged = false;
		public bool Success;			// makes sense only after markComplete
		public string Comment = "";		// filled by markComplete, or initially on creation

		public override string ToString()
		{
			string ret;
			long sec;
			if(Complete)
			{
				ret = Success ? "-- Success in " : "-- Failed in ";
				sec = (Finished.Ticks - Started.Ticks) / 10000000L;
			}
			else
			{
				ret = "" + Progress + "% after ";
				sec = (DateTime.Now.Ticks - Started.Ticks) / 10000000L;
			}
			return ret + sec + " seconds -- " + Comment;
		}
	}

}
