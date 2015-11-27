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
using LibSys;

namespace LibGeo
{
	public class Backdrop : MyCacheable
	{
		protected string m_fileName;
		protected Bitmap m_bitmap = null;
		public Bitmap Image { get { return m_bitmap; } }
		public bool HasImage { get { return m_bitmap != null; } }

		private bool m_imageCorrupted = false;
		public bool ImageCorrupted { get { return m_imageCorrupted; } }

		protected int m_width = 0;
		protected int m_height = 0;

		public int Width  { get { return m_width; } }
		public int Height { get { return m_height; } }

		public Backdrop(string fileName, string baseName, bool doFill) : base(baseName)
		{
			m_fileName = fileName;
			if(doFill)
			{
				if(m_fileName != null) 
				{
					Fill();
				} 
				else 
				{
					IsEmpty = true;
				}
			}
		}

		// the Dispose will be called before removing from cache, so release anything that holds files or memory
		public override void Dispose()
		{
			if(m_bitmap != null)
			{
				m_bitmap.Dispose();		// makes sure the file is not locked for writing - and of course the memory is free
				m_bitmap = null;
			}
		}

		public void Fill()
		{
			try 
			{
				//LibSys.StatusBar.WriteLine("OK: Backdrop::Fill() - file " + m_fileName);
				m_bitmap = new Bitmap(m_fileName, true);
				if(m_bitmap == null)
				{
					IsEmpty = true;
				}
				else 
				{
					m_width = m_bitmap.Width;
					m_height = m_bitmap.Height;
				}
				m_imageCorrupted = false;
			}
			catch //(Exception exc)
			{
				m_imageCorrupted = true;
//				LibSys.StatusBar.Error("image '" + m_fileName + "' corrupted");
			}
		}

		public override string ToString()
		{
			lock(this) 
			{
				return "Backdrop: " + m_baseName + " " + Width + "x" + Height
					+ " used " + (new DateTime(LastUsed)) + " usage " + m_usageCounter + " IsEmpty=" + IsEmpty;
			}
		}
	}
}
