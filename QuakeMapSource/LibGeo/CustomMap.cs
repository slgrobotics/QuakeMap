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
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using LibSys;
using LibFormats;

namespace LibGeo
{
	public class CustomMap : LiveObject
	{
		// we need to have a handy CustomMap object for GetType() comparisons:
		public static CustomMap customMap = new CustomMap();
		public static Type getType() { return CustomMap.customMap.GetType(); }

		public virtual string MapName { get { return "custom map"; } set { ; } }

		public virtual string CustomMapType { get { return "undefined"; } }
		
		protected long		m_id = 0;
		public long         Id { get { return m_id; } set { m_id = value; } }

		// desc may contain several lines worth of descriptive text, useful for popups etc.
		protected string	m_desc = "";
		public string		Desc  { get { return m_desc; } set { m_desc = value; } }

		public virtual string Source { get { return "n/a"; } }
		
		public bool Persist 
		{
			get 
			{
				bool persist = false;
				for(int i=0;  i < Project.FileDescrList.Count ;i++)
				{
					FormattedFileDescr ffd = (FormattedFileDescr)Project.FileDescrList[i];
					if(ffd.filename.Equals(this.Source))
					{
						persist = ffd.isPersistent;
						break;
					}
				}
				return persist;
			}
			set 
			{
				if("undefined".Equals(CustomMapType))
				{
					return;
				}

				FormattedFileDescr ffd;
				for(int i=0;  i < Project.FileDescrList.Count ;i++)
				{
					ffd = (FormattedFileDescr)Project.FileDescrList[i];
					if(ffd.filename.Equals(this.Source))
					{
						ffd.isPersistent = value;
						return;
					}
				}
				// should not really happen: we couldn't find the map among the persistent.
				ffd = new FormattedFileDescr(this.Source, FileGeoTIFF.FormatName, value);
				Project.FileDescrList.Add(ffd);
			}
		}
		
		// the top-left is actually base m_pixelLocation
		protected Point m_pixelLocationBottomLeft;
		protected Point m_pixelLocationTopRight;
		protected Point m_pixelLocationBottomRight;

		public virtual GeoCoord TopLeft 
		{
			get 
			{
				return this.m_location;
			}
		}

		public virtual GeoCoord BottomRight 
		{
			get 
			{
				return this.m_location;
			}
		}

		public CustomMap()
		{
		}

		public CustomMap(string name, GeoCoord loc) : base(name, loc)
		{
		}

		public override string ToString()
		{
			return MapName + " - " + Location;
		}

		public override string toTableString()
		{
			return ToString();
		}

		public bool sameAs(CustomMap other)
		{
			return Location.Equals(other.Location) && CustomMapType.Equals(other.CustomMapType);
		}

		//
		// map-related (visual) part below
		//

		protected int m_fontSize = 10;
		public int    FontSize { get { return m_fontSize; } set { m_fontSize = value; } }
		public const int MIN_MAP_PIXEL_RADIUS = 20;

		public override void init(bool doName)
		{
			bool enabled = m_enabled;
			base.init(doName);
			m_enabled = enabled;
		}

		public virtual string toStringPopup()
		{
			return MapName + "\n" + Desc;
		}

		public override void PaintLabel(Graphics graphics, IDrawingSurface tileSet, ITile itile, bool isPrint)
		{
		}

		public override void PaintLabel(Graphics graphics, IDrawingSurface tileSet, ITile iTile, bool isPrint, int offsetX, int offsetY)
		{
		}

		public override void Paint(Graphics graphics, IDrawingSurface tileSet, ITile iTile)
		{
			Paint(graphics, tileSet, iTile, 0, 0);
		}

		public override void Paint(Graphics graphics, IDrawingSurface tileSet, ITile iTile, int offsetX, int offsetY)
		{
		}
	}
}
