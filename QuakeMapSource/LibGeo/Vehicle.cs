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
using System.Xml;
using System.Drawing;
using LibSys;

namespace LibGeo
{
	public class Vehicle : LiveObject
	{
		// we need to have a handy Vehicle object for GetType() comparisons:
		public static Vehicle vehicle = new Vehicle();
		public static Type getType() { return Vehicle.vehicle.GetType(); }

		// positioning error data and velocity on top of location/time pair (location duplicated in LiveObject):
		public GpsRealTimeData RealTimeData = null;

		protected string m_label = "";
		public string Label { get { return m_label; } set { m_label = value; } }		// actual vehicle name
		
		protected string m_sym = "";
		public string Sym { get { return m_sym; } set { m_sym = value; } }				// icon file name (less ".gif") in the Vehicles folder
		
		// desc may contain several lines worth of descriptive text, useful for popups etc.
		protected string	m_desc = "";
		public string		Desc  { get { return m_desc; } set { m_desc = value; } }

		protected string m_source = "";
		public string Source { get { return m_source; } set { m_source = value; } }
		
		protected string m_url = "";
		public string Url { get { return m_url; } set { m_url = value; } }

		protected bool m_keepInView = false;
		public bool KeepInView { get { return m_keepInView; } set { m_keepInView = value; } }

		protected bool m_showTime = true;
		public bool ShowTime { get { return m_showTime; } set { m_showTime = value; } }
		protected DateTime m_time = DateTime.MinValue;	// MinValue will show current time. This value is local time, not zulu!
		public DateTime Time { get { return m_time; } set { m_time = value; } }

		private Bitmap m_image = null;
		public Bitmap Image {
			get { return m_image; }
			set {
				m_image = value;
				m_pixelRadius = m_image == null ? MIN_VEH_PIXEL_RADIUS : (m_image.Width + m_image.Height) * 3 / 16;
			}
		}

		public Vehicle()
		{
		}

		public Vehicle(GeoCoord loc, string name, string sym, string source, string url, string desc)   : base("", loc)
		{
			m_label = name;
			m_sym = sym;
			m_source = source;
			m_url = url;
			m_desc = desc;
			Image = null;

			tick();		// set Name
		}

		public Vehicle(XmlNode node)		// may throw an exception if XML node is not right
		{
			m_source = "auto";
			m_url = Project.WEBSITE_LINK_WEBSTYLE;

			Name = "Vehicle";
		}

		public override string ToString()
		{
			return Location + " src=" + m_source + " url=" + m_url;
		}

		public override string toTableString()
		{
			return ToString();
		}

		public bool sameAs(Vehicle other)
		{
			return Location.Equals(other.Location);
		}

		//
		// map-related (visual) part below
		//

		protected int m_fontSize = 10;
		public int    FontSize { get { return m_fontSize; } set { m_fontSize = value; } }
		public const int MIN_VEH_PIXEL_RADIUS = 20; // default size if !doSize

		public override void init(bool doName)
		{
			base.init(doName);
		}

		// interface Tickable
		public override void tick()
		{
			Name = "" + Label + (m_showTime ? (" - " + (m_time.Equals(DateTime.MinValue) ? DateTime.Now : m_time)) : "");
		}

		public override void PutOnMap(IDrawingSurface tileSet, ITile iTile, IObjectsLayoutManager olm)
		{
			base.PutOnMap(tileSet, iTile, olm);

			m_enabled = true;

			placeLabel(5, m_fontSize, false);	// may turn m_doName to false

			boundingRect();		// make sure we have current values there

			//LibSys.StatusBar.Trace("Vehicle:PutOnMap():  veh - " + Location + m_pixelLocation + " - " + " BR=" + m_boundingRect + " LBR=" + m_labelBoundingRect + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
		}
		
		public override void AdjustPlacement(IDrawingSurface map, ITile iTile, IObjectsLayoutManager olm)
		{
			if(m_intersections > 0) 
			{
#if DEBUG
				LibSys.StatusBar.Trace("AdjustPlacement():  Vehicle: - " + Location + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
#endif
				placeLabel(0, m_fontSize, true);	// may turn m_doName to false
				boundingRect();		// make sure we have current values there
				//LibSys.StatusBar.Trace("                int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
			}
		}

		public virtual string toStringPopup()
		{
			return Label + "\n" + Desc;
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
			if(!m_enabled) 
			{
				return;
			}

			Point pLoc = tileSet.toPixelLocation(Location, iTile);
			//LibSys.StatusBar.Trace("Vehicle::Paint()  - " + Location + " - " + m_doName + m_labelBoundingRect);
        
			Rectangle r = m_imageBoundingRect;
        
			Font font = Project.getLabelFont(m_fontSize);
			
			int xx = m_labelBoundingRect.X - 3;
			int yy = m_labelBoundingRect.Y - 1;
			//string label = "" + m_labelPosition + Name;
			string label = Name;
			if(Project.vehicleUseShadow) 
			{
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx,   yy);
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx+2, yy);
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx,   yy-2);
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx+2, yy-2);
			}
			graphics.DrawString(label, font, Project.vehicleFontBrush, xx+1, yy-1);
        
			if(m_image == null) 
			{
				graphics.DrawEllipse(Project.vehiclePen, r.X+2, r.Y+2, r.Width-4, r.Height-4);
			} 
			else 
			{
				graphics.DrawImage(m_image, r.X, r.Y);
			}

			// debug only:
			//	graphics.DrawRectangle(Project.debugPen, m_boundingRect);
			//	graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);
			//	graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);
		}

		public override Rectangle intersectionSensitiveRect2()
		{
			return m_labelBoundingRect;
		}

		public override Rectangle imageBoundingRect()
		{
			m_imageBoundingRect = m_image == null ?
				  base.imageBoundingRect()
				: new Rectangle(m_pixelLocation.X-m_image.Width/2, m_pixelLocation.Y-m_image.Height/2,
								m_image.Width, m_image.Height);

			return m_imageBoundingRect;
		}

	}
}
