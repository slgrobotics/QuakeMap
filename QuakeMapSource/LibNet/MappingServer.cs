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
using LibSys;

namespace LibNet
{
	/// <summary>
	/// Summary description for MappingServer.
	/// </summary>
	public class MappingServer
	{
		protected string m_url = null;
		public string Url { get { return m_url; } }
		protected string m_mapExtension = Project.MAP_DESCR_EXTENSION;
		public string MapExtension { get { return m_mapExtension; } }
		protected string m_jpgExtension = Project.JPG_DESCR_EXTENSION;
		public string JpgExtension { get { return m_jpgExtension; } }
		protected string m_fdbExtension = Project.FDB_DESCR_EXTENSION;
		public string FdbExtension { get { return m_fdbExtension; } }
		protected string m_maptreeExtension = Project.MAPTREE_DESCR_EXTENSION;
		public string MapTreeExtension { get { return m_maptreeExtension; } }
		protected string m_levels = "012345";
    
		/**
		 * Creates new MappingServer 
		 * the descriptor string format like this:
		 *        MAPSERVER=URL=http://odin.prohosting.com/sgrichin/maps33;MTR=.txt;MAP=.mds;JPG=.jpg;FDB=.fdb
		 *
		 * you can skip URL= and specify only non-default conversions:
		 *        MAPSERVER=http://odin.prohosting.com/sgrichin/maps33;MAP=.mds
		 */
		public MappingServer(string descr) 
		{
			//m_url = descr;
			char[] splitter = { ';' }; 
			string[] st = descr.Split(splitter);
			foreach(string token in st)  
			{
				if(token.StartsWith("URL=")) 
				{
					m_url = token.Substring(4);
				} 
				else if(token.StartsWith("MTR=")) 
				{
					m_maptreeExtension = token.Substring(4);
				} 
				else if(token.StartsWith("MAP=")) 
				{
					m_mapExtension = token.Substring(4);
				} 
				else if(token.StartsWith("JPG=")) 
				{
					m_jpgExtension = token.Substring(4);
				} 
				else if(token.StartsWith("FDB=")) 
				{
					m_fdbExtension = token.Substring(4);
				} 
				else if(token.StartsWith("LEV=")) 
				{
					m_levels = token.Substring(4);
				} 
				else if(token.IndexOf('=') != -1) 
				{
					; // ignore unknown options - compatibility with future extentions
				} 
				else 
				{
					m_url = token;
				}
			}
		}
    
		public bool isLevelSupported(int level)
		{
			if(level < 0) 
			{
				return true;    // unknown zoom, the server may support it - need to try
			}
			string sLevel = "" + level;
			bool ret = (m_levels.IndexOf(sLevel) != -1);
			return ret;
		}
    
		/**
		 * return appropriate remote extention for local file name specified
		 */
		public string getRightExtention(string localFileName)
		{
			if(localFileName.EndsWith(Project.MAP_DESCR_EXTENSION)) 
			{
				return m_mapExtension;
			} 
			else if(localFileName.EndsWith(Project.JPG_DESCR_EXTENSION)) 
			{
				return m_jpgExtension;
			} 
			else if(localFileName.EndsWith(Project.FDB_DESCR_EXTENSION)) 
			{
				return m_fdbExtension;
			} 
			else if(localFileName.EndsWith(Project.MAPTREE_DESCR_EXTENSION)) 
			{
				return m_maptreeExtension;
			}
			return "";
		}
    
		public override string ToString()
		{
			return "MappingServer[" + ToShortString() + "]"; 
		}
    
		public string ToShortString()
		{
			return "URL=" + m_url + ";MAP=" + m_mapExtension + ";FDB=" + m_fdbExtension + ";JPG=" + m_jpgExtension + ";LEV=" + m_levels; 
		}
    
		public bool Equals(MappingServer other)
		{
			bool ret;
			ret = Url.Equals(other.Url);
			// System.Console.WriteLine("MappingServer.Equals()=" + ret + " this=" + this + " other=" + other );
			return ret;
		}
	}
}
