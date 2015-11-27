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
using System.Drawing;
using LibSys;

namespace LibGeo
{
	public enum LiveObjectTypes
	{
		[EnumDescription("none")]
		LiveObjectTypeNone,
		[EnumDescription("wpt")]
		LiveObjectTypeWaypoint,
		[EnumDescription("trkpt")]
		LiveObjectTypeTrackpoint,
		[EnumDescription("rtept")]
		LiveObjectTypeRoutepoint,
		[EnumDescription("geocache")]
		LiveObjectTypeGeocache,
		[EnumDescription("landmark")]
		LiveObjectTypeLandmark,
		[EnumDescription("vehicle")]
		LiveObjectTypeVehicle,
		[EnumDescription("earthquake")]
		LiveObjectTypeEarthquake,
	}

	public abstract class LiveObject : ITickable
	{
		protected string	 m_name = null;
		public string        Name { get { return m_name; } set { m_name = value; } }

		protected GeoCoord	 m_location = null;
		public GeoCoord      Location { get { return m_location; } set { m_location = value; } }
		
		protected bool	     m_simulated = false;
		public bool          Simulated { get { return m_simulated; } set { m_simulated = value; } }

		protected bool	     m_dynamic = false;
		public bool          Dynamic { get { return m_dynamic; } set { m_dynamic = value; } }

		// what amounts to "wpt", "rtept", "trkpt", "geocache", "earthquake"
		protected LiveObjectTypes m_type = LiveObjectTypes.LiveObjectTypeNone;
		public LiveObjectTypes LiveObjectType { get { return m_type; } set { m_type = value; } }
		
		public LiveObject()
		{
		}

		public LiveObject(string name, GeoCoord loc)
		{
			m_name = name;
			m_location = loc;
		}

		public string LiveObjectTypeToString()
		{
			return LiveObjectTypeToString(this.LiveObjectType);
		}

		public static string LiveObjectTypeToString(LiveObjectTypes type)
		{
//			return EnumDescription.getDescription(type);		-- obfuscator breaks this code

			switch(type)
			{
				default:
				case LiveObjectTypes.LiveObjectTypeNone:
					return "none";
				case LiveObjectTypes.LiveObjectTypeWaypoint:
					return "wpt";
				case LiveObjectTypes.LiveObjectTypeTrackpoint:
					return "trkpt";
				case LiveObjectTypes.LiveObjectTypeRoutepoint:
					return "rtept";
				case LiveObjectTypes.LiveObjectTypeGeocache:
					return "geocache";
				case LiveObjectTypes.LiveObjectTypeLandmark:
					return "landmark";
				case LiveObjectTypes.LiveObjectTypeVehicle:
					return "vehicle";
				case LiveObjectTypes.LiveObjectTypeEarthquake:
					return "earthquake";
			}
		}

		public void CleanName()
		{
			int pos = m_name.IndexOf("(");
			if(pos > 0) 
			{
				m_name = m_name.Substring(0, pos).Trim();
			}
			pos = m_name.ToLower().IndexOf("city of");
			if(pos > 0) 
			{
				m_name = m_name.Substring(0, pos).Trim();
			}
		}

		// interface ITickable
		public virtual void tick()
		{
		}

		public void cleanup()
		{
		}

		public virtual string toTableString()
		{
			return Location + " " + Name;
		}

		public Distance distanceFrom(GeoCoord from)
		{
			return m_location.distanceFrom(from);
		}

		public virtual int CompareTo(Object other, int criteria)
		{
			return Name.CompareTo(((LiveObject)other).Name);
		}

		public abstract void Paint(Graphics graphics, IDrawingSurface tileSet, ITile iTile);

		public abstract void Paint(Graphics graphics, IDrawingSurface tileSet, ITile iTile, int offsetX, int offsetY);

		public abstract void PaintLabel(Graphics graphics, IDrawingSurface tileSet, ITile iTile, bool isPrint);

		public abstract void PaintLabel(Graphics graphics, IDrawingSurface tileSet, ITile iTile, bool isPrint, int offsetX, int offsetY);

		//
		// map-related functionality here:
		//

		protected IDrawingSurface m_map;
		protected IObjectsLayoutManager m_olm;
		protected ITile m_itile;
		protected Rectangle m_boundingRect;
		public Rectangle BoundingRect { get { return m_boundingRect; } }
		protected Rectangle m_imageBoundingRect;
		protected Rectangle m_labelBoundingRect;
		protected Font m_font = null;
		protected Font m_fontPrint = null;
		protected Point m_pixelLocation;
		public Point PixelLocation { get { return m_pixelLocation; } }
		protected int m_labelWidth;
		protected int m_labelHeight;
		protected int m_intersections = 0;
		protected int m_labelPosition = 0;	// 0 to N_LABEL_POSITIONS-1
		public int LabelPosition { get { return m_labelPosition; } }
		public int N_LABEL_POSITIONS = 8;
		public int MAX_INTERSECTIONS = 20;
		public const int MAX_BIGCITY_INTERSECTIONS_FACTOR = 10;

		protected int m_pixelRadius = 0;
		public int PixelRadius { get { return m_pixelRadius; } set { m_pixelRadius = value; } }

		protected bool m_doName = true;
		public bool	   DoName { get { return m_doName; } set { m_doName = value; } }

		protected bool m_doSize = true;

		protected bool m_enabled = false;
		public bool	   Enabled { get { return m_enabled; } set { m_enabled = value; } }

		// some strings useful for KML formatting:
		protected const string m_startTable = "<br/><hr><font face='Verdana'><table>\n";
		protected const string m_startTr1 = "<tr><td colspan=\"2\">\n";
		protected const string m_startTr2 = "<tr><td>\n";
		protected const string m_nextTd = "</td><td>\n";
		protected const string m_endTr = "</td></tr>\n";
		protected const string m_endTable = "</td></tr></table></font><hr>\n";

		public virtual void PutOnMap(IDrawingSurface map, ITile iTile, IObjectsLayoutManager iOlm)
		{
			m_map = map;
			m_itile = iTile;
			m_olm = iOlm;	// can be null

			m_pixelLocation = map.toPixelLocation(m_location, iTile);
		}

		public virtual void init(bool doName)
		{
			m_intersections = 0;
			m_labelPosition = 0;
			//m_pixelRadius = 0;
			m_doName = doName;
			m_doSize = true;
			m_enabled = false;
			m_imageBoundingRect = Rectangle.Empty;
			m_labelBoundingRect = Rectangle.Empty;
			m_boundingRect = Rectangle.Empty;
			m_pixelLocation = Point.Empty;
		}

		public virtual void AdjustPlacement(IDrawingSurface map, ITile iTile, IObjectsLayoutManager tile)
		{
		}
		
		public int nextLabelPosition()
		{
			m_labelPosition++;

			if(m_labelPosition > N_LABEL_POSITIONS-1) 
			{
				m_labelPosition = 0;
			}

			return m_labelPosition;
		}

		protected virtual void recalcLabelMetrics(string label, Graphics graphics)
		{
			// other threads can throw "invalid argument" exception in g.MeasureString:

			SizeF size = graphics.MeasureString(label, m_font);
			m_labelWidth = (int)(size.Width * 0.95d); 
			m_labelHeight = (int)(size.Height * 0.85d);
		}

		// big place: 0 - small, 5 - must have
		public virtual void placeLabel(int bigPlace, int fontSize, bool isAdjusting)
		{
			//LibSys.StatusBar.Trace("LiveObject::placeLabel(" + bigPlace + "," + fontSize + ")");
			m_font = Project.getLabelFont(fontSize);

			Graphics g = m_map.getGraphics();
			if(g == null) 
			{
				LibSys.StatusBar.Trace("Graphics null in LiveObject::placeLabel()");
				return;
			}
			// other threads can throw "invalid argument" exception in g.MeasureString:
			recalcLabelMetrics(Name, g);

			// Now try to place the label so that it does not intersect with
			// any other label (or at least minimize intersections)
			int jmin = 100000;
			int imin = 0;
			m_labelPosition = 0;
			Rectangle frame = m_map.getFrameRectangle(m_itile);
			for(int i=0; i < N_LABEL_POSITIONS ;i++,nextLabelPosition()) 
			{
				labelBoundingRect();
				Rectangle lbr = intersectionSensitiveRect();
				// first check if label in this position will be clipped by frame:
				Rectangle tmp = new Rectangle(frame.X, frame.Y, frame.Width, frame.Height);
				tmp.Intersect(lbr);
				if(!lbr.Equals(tmp)) 
				{
					//LibSys.StatusBar.Trace("pos=" + m_labelPosition + "  clipped by frame");
					continue;
				}
				// now count intersections with other labels:
				int j = m_map.countIntersections(m_olm, this, lbr);
				if(j == 0) 
				{	// no intersections; we better pick this choice.
					imin = i;
					jmin = 0;
					break;
				}
				if(j < jmin) 
				{	// minimal intersections is our goal
					jmin = j;
					imin = i;
				}
			}
			m_labelPosition = imin;
			m_intersections = jmin;
			if(jmin > MAX_INTERSECTIONS) 
			{	// bad luck - there are too many 
				// places already there
				if(bigPlace < 4	&& !isAdjusting && jmin > ((bigPlace + 1) * MAX_BIGCITY_INTERSECTIONS_FACTOR)) 
				{
					m_doName = false;
					boundingRect();
					//LibSys.StatusBar.Trace("Name omited, intersections=" + jmin + "  Place=" + Name);
				}
			} 
		}

		public virtual Rectangle imageBoundingRect()
		{
			m_imageBoundingRect = new Rectangle(
										m_pixelLocation.X-m_pixelRadius, m_pixelLocation.Y-m_pixelRadius,
										m_pixelRadius*2, m_pixelRadius*2);

			return m_imageBoundingRect;
		}

		public virtual Rectangle labelBoundingRect()
		{
			if(!m_doName) 
			{
				m_labelBoundingRect = Rectangle.Empty;
				return Rectangle.Empty;
			}

			switch(m_labelPosition) 
			{
				case 0:
					if(m_doSize) 
					{
						// label up to the right
						m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_pixelRadius/2,
							m_pixelLocation.Y-m_labelHeight-m_pixelRadius*3/4,
							m_labelWidth+1, m_labelHeight);
					} 
					else 
					{
						// label with point in the middle
						m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth/2,
							m_pixelLocation.Y-m_labelHeight/2,
							m_labelWidth+1, m_labelHeight);
					}
					break;
				case 1:		// label middle to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_pixelRadius+2,
						m_pixelLocation.Y-m_labelHeight/2,
						m_labelWidth, m_labelHeight);
					break;
				case 2:		// label down to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_pixelRadius/2,
						m_pixelLocation.Y+m_pixelRadius/2,
						m_labelWidth, m_labelHeight);
					break;
				case 3:		// label down to the middle
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth/2,
						m_pixelLocation.Y+m_pixelRadius/2,
						m_labelWidth, m_labelHeight);
					break;
				case 4:		// label up to the middle
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth/2,
						m_pixelLocation.Y-m_labelHeight-m_pixelRadius/2,
						m_labelWidth, m_labelHeight);
					break;
				case 5:		// label down to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_pixelRadius/2,
						m_pixelLocation.Y+m_pixelRadius/2,
						m_labelWidth, m_labelHeight);
					break;
				case 6:		// label middle to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_pixelRadius,
						m_pixelLocation.Y-m_labelHeight/2,
						m_labelWidth, m_labelHeight);
					break;
				case 7:
				default:	// label up to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_pixelRadius/2,
						m_pixelLocation.Y-m_labelHeight-m_pixelRadius/2,
						m_labelWidth, m_labelHeight);
					break;
			}

			return m_labelBoundingRect;
		}

		public virtual Rectangle intersectionSensitiveRect()
		{
			return m_doName ? m_labelBoundingRect : Rectangle.Empty;
		}

		public virtual Rectangle intersectionSensitiveRect2()
		{
			return m_imageBoundingRect;
		}

		public virtual Rectangle boundingRect()
		{
			imageBoundingRect();
		
			m_boundingRect = m_imageBoundingRect;	//new Rectangle(m_imageBoundingRect.X, m_imageBoundingRect.Y,
													//m_imageBoundingRect.Width, m_imageBoundingRect.Height);

			labelBoundingRect();	// will become empty if !m_doName

			if(m_doName)
			{
				m_boundingRect = Rectangle.Union(m_boundingRect, m_labelBoundingRect);
			}

			m_boundingRect.Inflate(1, 1);
			return m_boundingRect;
		}
	}
}
