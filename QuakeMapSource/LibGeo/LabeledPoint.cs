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
using System.Globalization;
using System.Xml;
using System.Drawing;
using LibSys;

namespace LibGeo
{
	public class LabeledPoint : LiveObject
	{
		protected int m_fontSize = Project.FONT_SIZE_REGULAR;
		public int FontSize { get { return m_fontSize; } set { m_fontSize = value; } }
		protected int m_fontSizePrint = Project.FONT_SIZE_PRINT;
		public int FontSizePrint { get { return m_fontSizePrint; } set { m_fontSizePrint = value; } }

		public LabeledPoint()
		{
			m_pixelRadius = 3;
			N_LABEL_POSITIONS = 12;
		}

		//
		// map-related (visual) part below
		//

		protected Brush brushFont = null;
		protected Brush brushBackground = null;
		protected Pen penMain = null;
		protected Pen penUnderline = null;

		protected int m_shift = 0;
		protected const int MIN_SHIFT = 35;
		protected const int MAX_SHIFT = 70;
        
		// the numbers define the order in which label positions are
		// selected. First pick is 0, then 1, 2, .... 11:

		// top first order:
		private const int LPOS_UP_RIGHT_MAX			=  0;
		private const int LPOS_UP_RIGHT				=  2;
		private const int LPOS_UP_RIGHT_MIN			=  4;
		private const int LPOS_DOWN_RIGHT_MIN		=  6;
		private const int LPOS_DOWN_RIGHT			=  8;
		private const int LPOS_DOWN_RIGHT_MAX		= 10;
		private const int LPOS_UP_LEFT_MAX			=  1;
		private const int LPOS_UP_LEFT				=  3;
		private const int LPOS_UP_LEFT_MIN			=  5;
		private const int LPOS_DOWN_LEFT_MIN        =  7;
		private const int LPOS_DOWN_LEFT			=  9;
		private const int LPOS_DOWN_LEFT_MAX        = 11;

		public override void init(bool doName)
		{
			base.init(doName);
			m_labelPosition = 0;	// 0 to N_LABEL_POSITIONS-1
			m_shift = 0;
		}

		protected virtual string getLabel(bool getAll)
		{
			return m_name;
		}

		public override void placeLabel(int bigPlace, int fontSize, bool isAdjusting)
		{
			//LibSys.StatusBar.Trace("LabeledPoint::placeLabel(" + bigPlace + "," + fontSize + ")");
			m_font = Project.getLabelFont(fontSize);
			m_fontPrint = Project.getLabelFont(fontSize - Project.FONT_SIZE_REGULAR + Project.FONT_SIZE_PRINT);

			Graphics g = m_map.getGraphics();
			if(g == null) 
			{
				LibSys.StatusBar.Trace("Graphics null in LabeledPoint::placeLabel()");
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
			if(jmin > MAX_INTERSECTIONS && bigPlace < 5)  // LiveObjectType != LiveObjectTypes.LiveObjectTypeGeocache) 
			{	// bad luck - there are too many 
				// waypoints already there
				m_doName = false;
				boundingRect();
				//LibSys.StatusBar.Trace("---- placeLabel(): Name omited, intersections=" + jmin + "  wpt=" + Name + m_labelBoundingRect);
			} 
			else 
			{
				//LibSys.StatusBar.Trace("placeLabel(): pos=" + m_labelPosition + " intersections=" + jmin + "  wpt=" + Name + m_labelBoundingRect);
			}
		}

		protected virtual int fontSizeByType(LiveObjectTypes type)
		{
			int ret = Project.FONT_SIZE_REGULAR;
			return ret;
		}

		protected virtual int imageSizeByType(LiveObjectTypes type)
		{
			int ret = 3;
			return ret;
		}

		public override void PutOnMap(IDrawingSurface layer, ITile iTile, IObjectsLayoutManager olm)
		{
			base.PutOnMap(layer, iTile, olm);

			//m_doName = true;
			m_enabled = true;

			Name = getLabel(true);

			m_fontSize = fontSizeByType(LiveObjectType);

			placeLabel(0, m_fontSize, false);	// may turn m_doName to false

			boundingRect();		// make sure we have current values there

			//LibSys.StatusBar.Trace("LabeledPoint:PutOnMap():  " + Location + m_pixelLocation + " BR=" + m_boundingRect + " LBR=" + m_labelBoundingRect + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
		}
		
		public override void AdjustPlacement(IDrawingSurface map, ITile iTile, IObjectsLayoutManager olm)
		{
			//LibSys.StatusBar.Trace("LabeledPoint:AdjustPlacement():   - " + Location + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
			if(m_intersections > 0) 
			{
				//LibSys.StatusBar.Trace("LabeledPoint:AdjustPlacement():   - " + Location + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
				placeLabel(0, m_fontSize, true);	// may turn m_doName to false
				boundingRect();		// make sure we have current values there
				//LibSys.StatusBar.Trace("                int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
			}
		}

		// for the LiveMap:isRelevant() method 
		// we need to know how important/big the waypoint representation is on the map without
		// instantiating the visial object. So, we use static method here.
		public static Rectangle boundingRectEstimate(IDrawingSurface map, LabeledPoint wp)
		{
			Point pPoint;
				
			try
			{
				pPoint = map.toPixelLocation(wp.Location, null);
			}
			catch (Exception e)
			{
				return Rectangle.Empty;
			}

			int w = wp.imageSizeByType(wp.LiveObjectType);
			int h = w;
			
			Rectangle re = new Rectangle(pPoint.X-w, pPoint.Y-h, w*2, h*2);

			return re;
		}

		public override Rectangle labelBoundingRect()
		{
			if(!m_doName) 
			{
				m_labelBoundingRect = Rectangle.Empty;
				return Rectangle.Empty;
			}

			if(m_shift == 0) 
			{
				imageBoundingRect();
				// m_shift should be known by now as result of imageBoundingRect()
			}

			switch(m_labelPosition) 
			{
				default:
				case LPOS_UP_RIGHT_MIN:		// label up to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_shift,
						m_pixelLocation.Y-m_labelHeight-m_shift/3,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_UP_RIGHT:			// label up to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_shift,
						m_pixelLocation.Y-m_labelHeight-m_shift,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_UP_RIGHT_MAX:		// label high up to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_shift*2/3,
						m_pixelLocation.Y-m_labelHeight-m_shift*3/2,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_DOWN_RIGHT:		// label down to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_shift,
						m_pixelLocation.Y-m_labelHeight+m_shift,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_DOWN_RIGHT_MIN:	// label down to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_shift,
						m_pixelLocation.Y-m_labelHeight+m_shift/3,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_DOWN_RIGHT_MAX:	// label way down to the right
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X+m_shift*2/3,
						m_pixelLocation.Y-m_labelHeight+m_shift*3/2,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_DOWN_LEFT_MIN:	// label 1/3 down to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_shift,
						m_pixelLocation.Y-m_labelHeight+m_shift/3,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_DOWN_LEFT:		// label down to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_shift,
						m_pixelLocation.Y-m_labelHeight+m_shift,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_UP_LEFT_MIN:		// label 1/3 up to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_shift,
						m_pixelLocation.Y-m_labelHeight-m_shift/3,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_DOWN_LEFT_MAX:	// label way down to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_shift*2/3,
						m_pixelLocation.Y-m_labelHeight+m_shift*3/2,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_UP_LEFT_MAX:		// label high up to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_shift*2/3,
						m_pixelLocation.Y-m_labelHeight-m_shift*3/2,
						m_labelWidth, m_labelHeight);
					break;
				case LPOS_UP_LEFT:			// label up to the left
					m_labelBoundingRect = new Rectangle(m_pixelLocation.X-m_labelWidth-m_shift,
						m_pixelLocation.Y-m_labelHeight-m_shift,
						m_labelWidth, m_labelHeight);
					break;
			}

			return m_labelBoundingRect;
		}

		public override Rectangle imageBoundingRect()
		{
			// recalculate br, as dx, dy may change in time:
			Rectangle re = boundingRectEstimate(m_map, this);

			m_shift = Math.Max(MIN_SHIFT, Math.Min(re.Width / 2, MAX_SHIFT));

			m_imageBoundingRect = re;

			return m_imageBoundingRect;
		}

		public override void Paint(Graphics graphics, IDrawingSurface layer, ITile iTile)
		{
			Paint(graphics, layer, iTile, 0, 0);
		}

		public override void Paint(Graphics graphics, IDrawingSurface layer, ITile iTile, int offsetX, int offsetY)
		{
			if(!m_enabled) 
			{
				return;
			}

			//LibSys.StatusBar.Trace("LabeledPoint::Paint()  - " + Location + " : " + m_doName + m_labelBoundingRect);
        
			int x, y, w, h;

			x = m_imageBoundingRect.X + 1 + offsetX;
			y = m_imageBoundingRect.Y + 1 + offsetY;
			w = m_imageBoundingRect.Width-2;
			h = m_imageBoundingRect.Height-2;
 
			graphics.DrawEllipse(penMain, x, y, w, h);

			// debug only - show bounding rectangles:
			//graphics.DrawRectangle(Project.debugPen, m_boundingRect);			// red
			//graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);		// green
			//graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);		// yellow
		}

		public override void PaintLabel(Graphics graphics, IDrawingSurface layer, ITile iTile, bool isPrint)
		{
			PaintLabel(graphics, layer, iTile, isPrint, 0, 0);
		}

		public override void PaintLabel(Graphics graphics, IDrawingSurface layer, ITile iTile, bool isPrint, int offsetX, int offsetY)
		{
			if(!m_enabled || !m_doName) 
			{
				return;
			}

			//LibSys.StatusBar.Trace("LabeledPoint::PaintLabel()  - " + Location + " : " + m_doName + m_labelBoundingRect);
        
			string label = getLabel(false);
			if(!label.Equals(Name)) 
			{
				Name = label;
				recalcLabelMetrics(label, graphics);
			}

			if(label != null && label.Length > 0) 
			{
				int x, y, w, h;

				x = m_imageBoundingRect.X + 1 + offsetX;
				y = m_imageBoundingRect.Y + 1 + offsetY;
				w = m_imageBoundingRect.Width-2;
				h = m_imageBoundingRect.Height-2;

				int xxx = m_labelBoundingRect.X + offsetX;
				int yyy = m_labelBoundingRect.Y + offsetY + (isPrint ? 2 : 0);
				Font font = isPrint ? m_fontPrint : m_font;
				if(Project.waypointUseShadow) 
				{
					graphics.DrawString(label, font, brushBackground, xxx,   yyy);
					graphics.DrawString(label, font, brushBackground, xxx+2, yyy);
					graphics.DrawString(label, font, brushBackground, xxx,   yyy-2);
					graphics.DrawString(label, font, brushBackground, xxx+2, yyy-2);

					graphics.DrawString(label, font, brushBackground, xxx+2, yyy-1);
					graphics.DrawString(label, font, brushBackground, xxx+1, yyy);
					graphics.DrawString(label, font, brushBackground, xxx,   yyy-1);
					graphics.DrawString(label, font, brushBackground, xxx+1, yyy-2);
				}
				//label += (" " + m_labelPosition);

				graphics.DrawString(label, font, brushFont, xxx+1, yyy-1);
    
				// white lines on topo maps are invisible, use Black pen instead:
				Pen pen = "topo".Equals(Project.drawTerraserverMode) || (!Project.drawTerraserver && !Project.drawRelief) ? Project.blackPen : penUnderline;

				int xe = m_pixelLocation.X + offsetX;
				int ye = m_pixelLocation.Y + offsetY;
				int xx = xe;
				int yy = ye;
				switch(m_labelPosition) 
				{
					default:
					case LPOS_UP_RIGHT_MIN:
						xx += m_shift;
						yy -= m_shift/3;
						break;
					case LPOS_UP_RIGHT:
						xx += m_shift;
						yy -= m_shift;
						break;
					case LPOS_UP_RIGHT_MAX:
						xx += m_shift*2/3;
						yy -= m_shift*3/2;
						break;
					case LPOS_DOWN_RIGHT:
						xx += m_shift;
						yy += m_shift;
						break;
					case LPOS_DOWN_RIGHT_MIN:
						xx += m_shift;
						yy += m_shift/3;
						break;
					case LPOS_DOWN_RIGHT_MAX:
						xx += m_shift*2/3;
						yy += m_shift*3/2;
						break;
					case LPOS_DOWN_LEFT_MIN:
						xx -= m_labelWidth + m_shift;
						yy += m_shift/3;
						break;
					case LPOS_DOWN_LEFT:
						xx -= m_labelWidth + m_shift;
						yy += m_shift;
						break;
					case LPOS_UP_LEFT_MIN:
						xx -= m_labelWidth + m_shift;
						yy -= m_shift/3;
						break;
					case LPOS_DOWN_LEFT_MAX:
						xx -= m_labelWidth + m_shift*2/3;
						yy += m_shift*3/2;
						break;
					case LPOS_UP_LEFT_MAX:
						xx -= m_labelWidth + m_shift*2/3;
						yy -= m_shift*3/2;
						break;
					case LPOS_UP_LEFT:
						xx -= m_labelWidth + m_shift;
						yy -= m_shift;
						break;
				}

				graphics.DrawLine(pen, xx+1, yy+1, xx+m_labelWidth, yy+1);

				switch(m_labelPosition) 
				{
					default:
					case LPOS_UP_LEFT_MAX:
					case LPOS_UP_LEFT:
					case LPOS_UP_LEFT_MIN:
						xx += m_labelWidth;
						graphics.DrawLine(pen, xe-1, ye-1, xx, yy+1);
						break;
					case LPOS_DOWN_LEFT_MIN:
					case LPOS_DOWN_LEFT:
					case LPOS_DOWN_LEFT_MAX:
						xx += m_labelWidth;
						graphics.DrawLine(pen, xe-1, ye+1, xx, yy+1);
						break;
					case LPOS_UP_RIGHT_MIN:
					case LPOS_UP_RIGHT:
					case LPOS_UP_RIGHT_MAX:
						graphics.DrawLine(pen, xe+1, ye-1, xx, yy+1);
						break;
					case LPOS_DOWN_RIGHT_MIN:
					case LPOS_DOWN_RIGHT:
					case LPOS_DOWN_RIGHT_MAX:
						graphics.DrawLine(pen, xe+1, ye+1, xx, yy);
						break;
				}

				graphics.DrawEllipse(pen, xe-1, ye-1, 3, 3);

				// debug only - show bounding rectangles:
				//graphics.DrawRectangle(Project.debugPen, m_boundingRect);					// red
				//graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);			// green
				//graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);			// yellow
				//graphics.DrawRectangle(Project.debug3Pen, intersectionSensitiveRect2());	// yellow
			}
		}

		public override Rectangle intersectionSensitiveRect()
		{
			if(!m_doName)
			{
				return Rectangle.Empty;
			}
			Rectangle ret = m_labelBoundingRect;
			ret.Inflate(0, 6);	// account for underlining line

			return ret;
		}

		public override Rectangle intersectionSensitiveRect2()
		{
			// returns rectangle around diagonal line connecting wp point with label and the endpoint oval

			int xe = m_pixelLocation.X;
			int ye = m_pixelLocation.Y;
			int xx = xe;
			int yy = ye;
			switch(m_labelPosition) 
			{
				default:
				case LPOS_UP_RIGHT_MIN:
					xx += m_shift;
					yy -= m_shift/3;
					break;
				case LPOS_UP_RIGHT:
					xx += m_shift;
					yy -= m_shift;
					break;
				case LPOS_UP_RIGHT_MAX:
					xx += m_shift*2/3;
					yy -= m_shift*3/2;
					break;
				case LPOS_DOWN_RIGHT:
					xx += m_shift;
					yy += m_shift;
					break;
				case LPOS_DOWN_RIGHT_MIN:
					xx += m_shift;
					yy += m_shift/3;
					break;
				case LPOS_DOWN_RIGHT_MAX:
					xx += m_shift*2/3;
					yy += m_shift*3/2;
					break;
				case LPOS_DOWN_LEFT_MIN:
					xx -= m_shift;
					yy += m_shift/3;
					break;
				case LPOS_DOWN_LEFT:
					xx -= m_shift;
					yy += m_shift;
					break;
				case LPOS_UP_LEFT_MIN:
					xx -= m_shift;
					yy -= m_shift/3;
					break;
				case LPOS_DOWN_LEFT_MAX:
					xx -= m_shift*2/3;
					yy += m_shift*3/2;
					break;
				case LPOS_UP_LEFT_MAX:
					xx -= m_shift*2/3;
					yy -= m_shift*3/2;
					break;
				case LPOS_UP_LEFT:
					xx -= m_shift;
					yy -= m_shift;
					break;
			}

			int topX = Math.Min(xx, xe);
			int topY = Math.Min(yy, ye);
			int w = Math.Abs(xe - xx);
			int h = Math.Abs(ye - yy);

			Rectangle ret = new Rectangle(topX, topY, w, h);
			Rectangle rOval = new Rectangle(xe-2, ye-2, 5, 5);
			return Rectangle.Union(ret, rOval);
		}
	}
}
