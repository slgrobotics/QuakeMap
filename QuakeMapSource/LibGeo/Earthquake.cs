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
using System.Text;

using LibSys;

namespace LibGeo
{
	public class Earthquake : LiveObject
	{
		// we need to have a handy Earthquake object for GetType() comparisons:
		public static Earthquake earthquake = new Earthquake();
		public static Type getType() { return Earthquake.earthquake.GetType(); }

		protected long m_id;
		public long Id { get { return m_id; } set { m_id = value; } }
		protected double m_magn;
		public double Magn { get { return m_magn; } set { m_magn = value; } }
		protected DateTime m_dateTime;		// always UTC
		public DateTime DateTime { get { return m_dateTime; } set { m_dateTime = value; } }
		public string sDateTime 
		{
			get 
			{
				string sTime;
				if(Project.useUtcTime)
				{
					string format = "yyyy MMM dd HH:mm:ss";
					sTime = m_dateTime.ToString(format) + " UTC";
				}
				else
				{
					sTime = Project.zuluToLocal(m_dateTime).ToString();	// current culture
				}
				return sTime;
			}
		}
		protected string m_quality = "";
		public string Quality { get { return m_quality; } set { m_quality = value; } }
		protected string m_source = "";
		public string Source { get { return m_source; } set { m_source = value; } }
		protected string m_url = "";
		public string Url { get { return m_url; } set { m_url = value; } }
		protected string m_comment = "";
		public string Comment { get { return m_comment; } set { m_comment = value; } }
		public bool IsSimulated { get { return false; } }

		// getLabel is generally called on labels layout, and it will know to use Project.waypointNameStyle
		public string		NameDisplayed { get { return m_name; } set { m_name = value; Name = getLabel(true); } }

		// deepest are BLUE, shallowest are RED:
		public static Brush[] depthPalette = null;
		protected static int m_fontSize = Project.FONT_SIZE_REGULAR;
		public static int FontSize { get { return m_fontSize; } set { m_fontSize = value; } }
		public static Font mm_font;	// to avoid using LiveObject.m_font 
		protected static int m_fontSizePrint = Project.FONT_SIZE_PRINT;
		public static int FontSizePrint { get { return m_fontSizePrint; } set { m_fontSizePrint = value; } }
		public static Font mm_fontPrint;	// to avoid using LiveObject.m_font 

		static Earthquake()
		{
			mm_font = Project.getLabelFont(m_fontSize);
			mm_fontPrint = Project.getLabelFont(m_fontSizePrint);

			depthPalette = new Brush[10]; 
			depthPalette[0] = new SolidBrush(Color.FromArgb(255, 0,   0));	// 0-5
			depthPalette[1] = new SolidBrush(Color.FromArgb(228, 0,  28));	// 5-10
			depthPalette[2] = new SolidBrush(Color.FromArgb(198, 0,  56));	// 10-15
			depthPalette[3] = new SolidBrush(Color.FromArgb(169, 0,  84));	// 15-20
			depthPalette[4] = new SolidBrush(Color.FromArgb(140, 0, 112));	// 20-25
			depthPalette[5] = new SolidBrush(Color.FromArgb(112, 0, 140));	// 25-30
			depthPalette[6] = new SolidBrush(Color.FromArgb( 84, 0, 169));	// 30-35
			depthPalette[7] = new SolidBrush(Color.FromArgb( 56, 0, 198));	// 35-40
			depthPalette[8] = new SolidBrush(Color.FromArgb( 28, 0, 228));	// 40-45
			depthPalette[9] = new SolidBrush(Color.FromArgb(  0, 0, 255));	// 45-AND MORE
		}

		public Earthquake()
		{
			this.LiveObjectType = LiveObjectTypes.LiveObjectTypeEarthquake;
			m_pixelRadius = MIN_EQ_PIXEL_RADIUS + 2;
			N_LABEL_POSITIONS = 12;
		}

		public Earthquake(GeoCoord loc, double magn, DateTime dateTime, string quality, string comment, string source, string url)
				: base("", loc)
		{
			this.LiveObjectType = LiveObjectTypes.LiveObjectTypeEarthquake;
			m_pixelRadius = MIN_EQ_PIXEL_RADIUS + 2;
			N_LABEL_POSITIONS = 12;

			m_magn = magn;
			m_dateTime = dateTime;		// always UTC
			m_quality = quality;
			m_source = source;
			m_url = url;

			Name = getLabel(false); //"" + m_magn + " - " + m_dateTime;
		}

		public Earthquake(string[] infos)		// may throw an exception if the data is not right
		{
			this.LiveObjectType = LiveObjectTypes.LiveObjectTypeEarthquake;
			m_pixelRadius = MIN_EQ_PIXEL_RADIUS + 2;
			N_LABEL_POSITIONS = 12;

			/*
				infos: 0 '2002/09/13 22:28:31'
				infos: 1 '13.10N'
				infos: 2 '93.11E'
				infos: 3 '33.0'
				infos: 4 '6.5'
				infos: 5 'A'
				infos: 6 'ANDAMAN ISLANDS, INDIA REGION'
				infos: 7 'http://neic.usgs.gov/neis/bulletin/neic_jabw.html'
				infos: 8 'bulletin'
			 */

			// time may be PDT from SCEC or HST from Hawaii - see EarthquakesCache:processQuakes0()
			bool notUtc = false;
			double zoneShift = 0.0d;

			if(infos[0].EndsWith(" PDT")) 
			{
				notUtc = true;
				zoneShift = 8.0d;
			} else if(infos[0].EndsWith(" HST")) 
			{
				notUtc = true;
				zoneShift = 10.0d;		// this one (10.0d) works in winter (tested Jan 30th 2005)
				if(System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
				{
					zoneShift = 11.0d;	// supposedly works in summer (TODO: test it!)
				}
			}


			if(notUtc)
			{
				string sTime = infos[0].Substring(0, infos[0].Length - 4);
				DateTime localTime = Convert.ToDateTime(sTime);
				// convert local time to UTC (with Daylight shift):
				//DaylightTime daylightTime = new DaylightTime(...);
				//if(System.TimeZone.IsDaylightSavingTime(localTime, daylightTime))
				if(System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(localTime))
				{
					m_dateTime = localTime.AddHours(zoneShift - 1.0d);  // to zulu, with daylight savings
				} 
				else
				{
					m_dateTime = localTime.AddHours(zoneShift);		// to zulu
				}
			} 
			else		// UTC from NEIC
			{
				m_dateTime = Convert.ToDateTime(infos[0]);	// already zulu
			}
			double lat;
			double lng;
			double depth;
			if(infos[1].ToLower().EndsWith("s") || infos[1].ToLower().EndsWith("n")) 
			{
				lat = Convert.ToDouble(infos[1].Substring(0, infos[1].Length - 1));
				if(infos[1].ToLower().EndsWith("s")) 
				{
					lat = -lat;
				}
				lng = Convert.ToDouble(infos[2].Substring(0, infos[2].Length - 1));
				if(infos[2].ToLower().EndsWith("w")) 
				{
					lng = -lng;
				}
			} 
			else 
			{
				lat = Convert.ToDouble(infos[1]);
				lng = Convert.ToDouble(infos[2]);
			}

			try 
			{
				depth = Convert.ToDouble(infos[3]) * 1000.0d;
			} 
			catch (Exception e)
			{
				depth = 0.0d;
			}
			m_location = new GeoCoord(lng, lat, -depth);
		
			try 
			{
				m_magn = Convert.ToDouble(infos[4]);
			} 
			catch (Exception e)
			{
				m_magn = 0.0d;
			}
			m_quality = infos[5];
			m_comment = infos[6];
			m_url = infos[7];
			m_source = infos[8];

			Name = getLabel(false);;
		}

		public Earthquake(XmlNode node)		// may throw an exception if XML node is not right
		{
			this.LiveObjectType = LiveObjectTypes.LiveObjectTypeEarthquake;
			m_pixelRadius = MIN_EQ_PIXEL_RADIUS + 2;
			N_LABEL_POSITIONS = 12;

			m_magn = 4.8d;
			m_dateTime = DateTime.Now;
			m_quality = "M";
			m_source = "auto";
			m_url = "http://www.quakemap.com/sergei/index.html";

			Name = getLabel(false); //"" + m_magn + " - " + m_dateTime;
		}

		public override string ToString()
		{
			return Location + " mag=" + string.Format("{0:F1}", m_magn) + " " + Project.zuluToLocal(m_dateTime) + " q=" + m_quality + " comment='" + m_comment + "' src=" + m_source + " url=" + m_url;
		}

		private void addKmlCoords(StringBuilder builder)
		{
			string sep = "<br/>";

			int coordStyle = Project.coordStyle;
			if(Project.coordStyle == 3)
			{
				coordStyle = 1;
			}
			builder.Append("<b>Loc:</b> " + this.Location.toString00(false, coordStyle) + sep);
			builder.Append("<b>UTM:</b> " + this.Location.toString0(false, 3) + sep);

			Distance dElev = new Distance(-this.Location.Elev);
			double depthKm = Math.Round(-this.Location.Elev / 1000.0d, 1);

			builder.Append("<b>Depth:</b> " + dElev.ToString() + " (" + string.Format("{0:F1}", depthKm) + " Km)");
		}

		public string toStringKmlDescr()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(m_startTable);
			builder.Append(m_startTr1);

			builder.Append("<b>Magnutude:</b> " + string.Format("{0:F1}", m_magn) + "<br/>\n");
			if(("" + m_quality).Length > 0)
			{
				builder.Append("<b>Quality:</b> " + m_quality + "<br/>\n");
			}
			if(("" + m_comment).Length > 0)
			{
				builder.Append("<b>Comment:</b> " + m_comment + "<br/>\n");
			}
			builder.Append("<b>Source:</b> " + m_source + "<br/>\n");
			if(("" + this.Url).Length > 0)
			{
				builder.Append("<b>URL:</b> " + String.Format("<a href=\"{0}\">{0}</a><br/>\n", this.Url));
			}

			addKmlCoords(builder);

			builder.Append(m_endTr);
			builder.Append(m_endTable);

			builder.Replace("<br><br>","<br>");
			builder.Replace("<br>\n<br>","<br>");

			return builder.ToString();
		}

		public override string toTableString()
		{
			return ToString();
		}

		public bool sameAs(Earthquake other)
		{
			return Magn == other.Magn && Location.sameAs(other.Location);
		}

		//
		// map-related (visual) part below
		//

		public const int MIN_EQ_PIXEL_RADIUS = 20; // default size if !doSize

		protected int m_shift = 0;
		protected const int MIN_SHIFT = 35; //27;
		protected const int MAX_SHIFT = 70; //54;
		public const int MAX_BIGQUAKE_INTERSECTIONS_FACTOR = 10;
		protected bool m_doFill = false;
		// m_triWidth is actually half of the upper side of the triangle:
		protected int m_triWidth = 0;
        
		public const int STYLE_DOT					=  0;
		public const int STYLE_CIRCLES              =  1;
		public const int STYLE_FILLCIRCLES			=  2;
		public const int STYLE_CONCENTRICCIRCLES    =  3;
		public const int STYLE_SQUARES              =  4;
		public const int STYLE_FILLSQUARES			=  5;   // default in Project.earthquakeStyle
		public const int STYLE_TRIANGLE				=  6;

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
		
/*
		// eartquakes are sorted east to west, so the east (right) wing of labels should go first:
		private const int LPOS_UP_RIGHT_MAX			=  0;
		private const int LPOS_UP_RIGHT				=  1;
		private const int LPOS_UP_RIGHT_MIN			=  2;
		private const int LPOS_DOWN_RIGHT_MIN		=  3;
		private const int LPOS_DOWN_RIGHT			=  4;
		private const int LPOS_DOWN_RIGHT_MAX		=  5;
		private const int LPOS_UP_LEFT_MAX			=  6;
		private const int LPOS_UP_LEFT				=  7;
		private const int LPOS_UP_LEFT_MIN			=  8;
		private const int LPOS_DOWN_LEFT_MIN        =  9;
		private const int LPOS_DOWN_LEFT			= 10;
		private const int LPOS_DOWN_LEFT_MAX        = 11;
*/


/*
		// visual prettiness order:
		private const int LPOS_UP_RIGHT_MAX			=  9;
		private const int LPOS_UP_RIGHT				=  2;
		private const int LPOS_UP_RIGHT_MIN			=  0;
		private const int LPOS_DOWN_RIGHT_MIN		=  3;
		private const int LPOS_DOWN_RIGHT			=  4;
		private const int LPOS_DOWN_RIGHT_MAX		=  5;
		private const int LPOS_UP_LEFT_MAX			=  8;
		private const int LPOS_UP_LEFT				=  1;
		private const int LPOS_UP_LEFT_MIN			=  6;
		private const int LPOS_DOWN_LEFT_MIN        =  7;
		private const int LPOS_DOWN_LEFT			= 10;
		private const int LPOS_DOWN_LEFT_MAX        = 11;
*/

		protected double m_magnFactor = Project.MAX_MAGN_FACTOR;	// current meters per magnitute point
		protected double m_elevKm;

		public override void init(bool doName)
		{
			base.init(doName);
			m_labelPosition = 0;	// 0 to N_LABEL_POSITIONS-1
			m_shift = 0;
		}

		// interface Tickable
		public override void tick()
		{
		}

		protected string getLabel(bool getAll)
		{
			string label = null;

			if(IsSimulated) 
			{
				label = string.Format("Simulated {0:F1}", m_magn);
			} 
			else  //if(this.isRecent()) {
			{
				label = "";
				if(getAll || Project.displayEqMagn) 
				{
					label += string.Format("{0:F1}", m_magn);
				}

				// no time on world map and state maps
				if((getAll || Project.displayEqTime) && m_doName && m_elevKm < 5000.0d) 
				{
					if(label.Length > 0) 
					{
						label += " - ";
					}
					string sTime = sDateTime;
					label += sTime; // + " " + Location;
				}
			}
			return label;
		}

		// big place ignored, Magn used instead
		public override void placeLabel(int bigPlace, int fontSize, bool isAdjusting)
		{
			//LibSys.StatusBar.Trace("Earthquake::placeLabel(" + bigPlace + "," + fontSize + ")");

			Graphics g = m_map.getGraphics();
			if(g == null) 
			{
				LibSys.StatusBar.Error("Graphics null in Earthquake::placeLabel()");
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
				// quakes already there
				if(Magn < 4.0d	&& !isAdjusting && jmin > ((Magn + 1) * MAX_BIGQUAKE_INTERSECTIONS_FACTOR)) 
				{
					m_doName = false;
					boundingRect();
					//LibSys.StatusBar.Trace("---- placeLabel(): Name omited, intersections=" + jmin + "  quake=" + Name + m_labelBoundingRect);
				}
			} 
			else 
			{
				//LibSys.StatusBar.Trace("placeLabel(): pos=" + m_labelPosition + " intersections=" + jmin + "  quake=" + Name + m_labelBoundingRect);
			}
		}

		/*
			"1 hour", "2 hours", "4 hours", "8 hours", "12 hours", "18 hours", "1 day",
			"2 days", "3 days", "5 days", "10 days"
		 */
		double[] recentTimes = new double[] {
												60.0d, 120.0d, 240.0d, 480.0d, 720.0d, 1080.0d, 1440.0d,
												2880.0d, 4320.0d, 7200.0d, 14400.0d
											};

		public override void PutOnMap(IDrawingSurface layer, ITile iTile, IObjectsLayoutManager olm)
		{
			base.PutOnMap(layer, iTile, olm);

			m_elevKm = layer.getCameraElevation() / 1000.0d;
			m_doFill = m_magn > 1.0d
				&& (!Project.earthquakeStyleFillRecent
						|| Project.localToZulu(DateTime.Now).AddMinutes(-recentTimes[Project.earthquakeStyleFillHowRecentIndex]) < this.DateTime);
			if(m_elevKm > 5000.0d) 
			{
				m_doName = m_magn > 5.0d;
			}
			else if(m_elevKm > 2000.0d)
			{
				m_doName = m_magn > 4.0d;
			} 
			else if(m_elevKm > 900.0d) 
			{
				m_doName = m_magn > 3.0d;
			} 
			else if(m_elevKm > 370.0d) 
			{
				m_doName = m_magn > 1.0d;
			} 
			else if(m_elevKm > 100.0d) 
			{
				m_doName = m_magn > 0.0d;
			} 
			else 
			{
				m_doName = true;
			}

			m_enabled = true;

			Name = getLabel(true);

			placeLabel(0, m_fontSize, false);	// may turn m_doName to false

			boundingRect();		// make sure we have current values there

			//LibSys.StatusBar.Trace("Earthquake:PutOnMap():  eq - " + Location + m_pixelLocation + " - " + Magn + " BR=" + m_boundingRect + " LBR=" + m_labelBoundingRect + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
		}
		
		public override void AdjustPlacement(IDrawingSurface map, ITile iTile, IObjectsLayoutManager olm)
		{
			//LibSys.StatusBar.Trace("Earthquake:AdjustPlacement():   - " + Location + " - " + Magn + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
			if(m_intersections > 0) 
			{
				//LibSys.StatusBar.Trace("Earthquake:AdjustPlacement():   - " + Location + " - " + Magn + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
				placeLabel(0, m_fontSize, true);	// may turn m_doName to false
				boundingRect();		// make sure we have current values there
				//LibSys.StatusBar.Trace("                int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
			}
		}

		// for the LiveMap:isRelevant() method 
		// we need to know how big the earthquake is on the map without
		// instantiating the visial object. So, we use static method here.
		public static Rectangle boundingRectEstimate(IDrawingSurface map, Earthquake eq, double magnFactor)
		{
			Point pPoint;
				
			try
			{
				pPoint =	map.toPixelLocation(eq.Location, null);
			} 
			catch (Exception e)
			{
				return Rectangle.Empty;
			}

			int w, h;
			int minSize = 15;
			int maxSize = 300;
			int xFactor = 2;
			int yFactor = 2;
			int yExtra = 0;

			double xMetersPerPixel = map.xMetersPerPixel();

			w = (int)(eq.Magn * magnFactor / map.xMetersPerPixel());
			if(w < minSize)	// we don't want the quake to show up too small or too big
			{
				w = minSize;
			}
			if(w > maxSize)
			{
				w = maxSize;
			}

			h = (int)(eq.Magn * magnFactor / map.yMetersPerPixel());
			if(h < minSize) 
			{
				h = minSize;
			}
			if(h > maxSize)
			{
				h = maxSize;
			}

			double cameraElevationKm = map.getCameraElevation() / 1000.0d;

			switch (Project.earthquakeStyle) 
			{
				default:
				case STYLE_CONCENTRICCIRCLES:	// concentric circles
				case STYLE_SQUARES:
				case STYLE_FILLSQUARES:
				case STYLE_CIRCLES:
				case STYLE_FILLCIRCLES:		// filled circles Alan Jones "Seismic Eruptions" style
				{
					double magMin;
					double magOff;
					double magFact;

					if(cameraElevationKm > 4000.0d)  // world map
					{
						magMin = 5.0d;
						magOff = 4.5d;
						magFact = 7.0d;
					}
					else if(cameraElevationKm > 900.0d)
					{	 // small scale
						magMin = 4.0d;
						magOff = 2.5d;
						magFact = 6.0d;
					} 
					else if(cameraElevationKm > 370.0d) 
					{	 // medium scale
						magMin = 2.0d;
						magOff = 0.5d;
						magFact = 5.0d;
					} 
					else 
					{	 // very large scale
						magMin = 1.0d;
						magOff = 0.5d;
						magFact = 4.0d;
					}

					if(eq.Magn <= magMin) 
					{
						w = 3;
						h = 3;
					} 
					else 
					{
						w = h = (int)((eq.Magn - magOff) * magFact);
					}
				}
					break;
				case STYLE_TRIANGLE:
					// triangle pointing down to epicenter; the deeper the quake,
					// the more narrow the triangle is:
					h = (int)(((double)h) * (1.0d + cameraElevationKm / 10000.0d));	// make them more visible on small scale maps:
					w = eq.calcTriWidth(h) * 2;	// triWidth is actually half of the upper side of the triangle.
					if(w < 50 && Project.displayEqDepth)
					{
						w = 50;
					}
					yFactor = 1;
					xFactor = 1;
					yExtra = 3;			// 3 pixels for the epicenter oval
					break;
				case STYLE_DOT:			// dot
					w = 3;
					h = 3;
					break;
			}

			Rectangle re = new Rectangle(pPoint.X-(w*xFactor/2), pPoint.Y-h, w*xFactor, h*yFactor + yExtra);

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

		/*
		public override Rectangle boundingRect()
		{
			
			m_boundingRect = imageBoundingRect();

			labelBoundingRect();	// will become empty if !m_doName

			if(m_doName) 
			{
				m_boundingRect = Rectangle.Union(m_boundingRect, m_labelBoundingRect);
			}

			return m_boundingRect;
		}
		*/

		public override Rectangle imageBoundingRect()
		{
			// recalculate br, as dx, dy may change in time:
			Rectangle re = boundingRectEstimate(m_map, this, m_magnFactor);

			m_shift = Math.Max(MIN_SHIFT, Math.Min(re.Width / 2, MAX_SHIFT));

			m_imageBoundingRect = re; // new Rectangle(re.X, re.Y, re.Width, re.Height);

			return m_imageBoundingRect;
		}

		protected int calcTriWidth(int h)
		{
			// triWidth is actually half of the upper side of the triangle:
			double depthMeters = Math.Max(0.0, -Location.Z);
			double depthMax = 10000.0d;
			double widthRelative = Math.Max(0.1, (depthMax - depthMeters) / depthMax);
			m_triWidth = Math.Max(2, (int)((double)h * widthRelative));
			return m_triWidth;
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

			//LibSys.StatusBar.Trace("Earthquake::Paint()  - " + Location + " - " + Magn + " : " + m_doName + m_labelBoundingRect);
        
			int x, y, w, h;

			x = m_imageBoundingRect.X + 1 + offsetX;
			y = m_imageBoundingRect.Y + 1 + offsetY;
			w = m_imageBoundingRect.Width-2;
			h = m_imageBoundingRect.Height-2;

			switch (Project.earthquakeStyle) 
			{
				case STYLE_DOT:		// small dot
					//g.setColor(color(3));
					graphics.DrawEllipse(Project.earthquakePen, x, y, w, h);
					break;
				case STYLE_TRIANGLE:
				{
					// triangle pointing down to epicenter; the deeper the quake,
					// the more narrow the triangle is:
					// m_triWidth is actually half of the upper side of the triangle.
					//g.setColor(color(3));
					graphics.DrawLine(Project.earthquakePen, x+w/2-m_triWidth, y, x+w/2+m_triWidth, y);
					graphics.DrawLine(Project.earthquakePen, x+w/2-m_triWidth, y, x+w/2, y+h);
					graphics.DrawLine(Project.earthquakePen, x+w/2+m_triWidth, y, x+w/2, y+h);

					if(h > 15 && Project.displayEqDepth && m_doName) 
					{
						double depthMeters = Math.Max(0.0, -Location.Z);
						Distance dist = new Distance(depthMeters);
						string sDist = dist.ToString();
						if(m_triWidth > 50)
						{
							sDist += " deep";
						}

						// other threads can throw "invalid argument" exception in graphics.MeasureString:
						SizeF size = graphics.MeasureString(sDist, mm_font);
						int depthLabelWidth = (int)(size.Width * 0.95d); 
						int depthLabelHeight = (int)(size.Height * 0.85d);
						//if(depthLabelWidth > m_triWidth*5/6) 
						//{
						//	depthLabelWidth = m_triWidth*5/6;
						//}
						int xx = x+w/2-depthLabelWidth/2;
						int yy = y + 2; //-depthLabelHeight-2;
						if(Project.earthquakeUseShadow) 
						{
							//g.setColor(PrjColor.palette[Project.earthquakeColorShadowIndex]);
							graphics.DrawString(sDist, mm_font, Project.earthquakeBackgroundBrush, xx,   yy);
							graphics.DrawString(sDist, mm_font, Project.earthquakeBackgroundBrush, xx+2, yy);
							graphics.DrawString(sDist, mm_font, Project.earthquakeBackgroundBrush, xx,   yy-2);
							graphics.DrawString(sDist, mm_font, Project.earthquakeBackgroundBrush, xx+2, yy-2);
						}
						//g.setColor(color(3));
						graphics.DrawString(sDist, mm_font, Project.earthquakeFontBrush, xx+1, yy-1);
					}
				}
					break;
				case STYLE_SQUARES:
					// Cyan looks like a good contrast color on the map, very suitable for border around filled circles and squares. 
					// g.setColor(PrjColor.palette[PrjColor.COLOR_CYAN]);
					//g.setColor(color(3));
					graphics.DrawRectangle(Project.earthquakePen, x, y, w, h);
					break;
				case STYLE_FILLSQUARES:
					if(m_doFill) 
					{
						graphics.FillRectangle(brushByDepth(), x, y, w, h);
					}
					if(w > 6) 
					{
						//g.setColor(color(3));   // PrjColor.palette[PrjColor.COLOR_CYAN]);
						graphics.DrawRectangle(Project.earthquakePen, x, y, w, h);
					}
					break;
				case STYLE_CIRCLES:
					//g.setColor(color(3));
					graphics.DrawEllipse(Project.earthquakePen, x, y, w, h);
					break;
				case STYLE_FILLCIRCLES:
					if(m_doFill) 
					{
						graphics.FillEllipse(brushByDepth(), x, y, w, h);
					}
					if(w > 6) 
					{
						//g.setColor(color(3));   // PrjColor.palette[PrjColor.COLOR_CYAN]);
						graphics.DrawEllipse(Project.earthquakePen, x, y, w, h);
					}
					break;
				case STYLE_CONCENTRICCIRCLES:
				default:
					// Oval drawn without "+1" would go beyond the bounding rectangle
					//g.setColor(color(3));
					graphics.DrawEllipse(Project.earthquakePen, x, y, w, h);
					if(w > 12 && h > 12) 
					{
						//g.setColor(color(2));
						graphics.DrawEllipse(Project.earthquakePen, x+w/6, y+h/6, w*2/3, h*2/3);
						if(w > 24 && h > 24) 
						{ 
							//g.setColor(color(1));
							graphics.DrawEllipse(Project.earthquakePen, x+w/3, y+h/3, w/3, h/3);
						}
					}
					break;
			}
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

			//LibSys.StatusBar.Trace("Earthquake::Paint()  - " + Location + " - " + Magn + " : " + m_doName + m_labelBoundingRect);
        
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
				int yyy = m_labelBoundingRect.Y + offsetY + (isPrint ? 1 : 0);
				Font font = isPrint ? mm_fontPrint : mm_font;
				if(Project.earthquakeUseShadow) 
				{
					graphics.DrawString(label, font, Project.earthquakeBackgroundBrush, xxx,   yyy);
					graphics.DrawString(label, font, Project.earthquakeBackgroundBrush, xxx+2, yyy);
					graphics.DrawString(label, font, Project.earthquakeBackgroundBrush, xxx,   yyy-2);
					graphics.DrawString(label, font, Project.earthquakeBackgroundBrush, xxx+2, yyy-2);
				}
				//label += (" " + m_labelPosition);
				graphics.DrawString(label, font, Project.earthquakeFontBrush, xxx+1, yyy-1);
    
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

				//graphics.DrawString("***"+label, mm_font, Project.earthquakeFontBrush, xx, yy-m_labelHeight);
				graphics.DrawLine(Project.earthquakePen, xx+1, yy+1, xx+m_labelWidth, yy+1);

				switch(m_labelPosition) 
				{
					default:
					case LPOS_UP_LEFT_MAX:
					case LPOS_UP_LEFT:
					case LPOS_UP_LEFT_MIN:
						xx += m_labelWidth;
						graphics.DrawLine(Project.earthquakePen, xe-1, ye-1, xx, yy+1);
						break;
					case LPOS_DOWN_LEFT_MIN:
					case LPOS_DOWN_LEFT:
					case LPOS_DOWN_LEFT_MAX:
						xx += m_labelWidth;
						graphics.DrawLine(Project.earthquakePen, xe-1, ye+1, xx, yy+1);
						break;
					case LPOS_UP_RIGHT_MIN:
					case LPOS_UP_RIGHT:
					case LPOS_UP_RIGHT_MAX:
						graphics.DrawLine(Project.earthquakePen, xe+1, ye-1, xx, yy+1);
						break;
					case LPOS_DOWN_RIGHT_MIN:
					case LPOS_DOWN_RIGHT:
					case LPOS_DOWN_RIGHT_MAX:
						graphics.DrawLine(Project.earthquakePen, xe+1, ye+1, xx, yy);
						break;
				}
				//if(Options.earthquakeUseShadow) {
				//	g.setColor(PrjColor.palette[Options.earthquakeColorShadowIndex]);
				//	g.fillOval(xe-3, ye-3, 5, 5);
				//}

				//g.setColor(color(3));
				graphics.DrawEllipse(Project.earthquakePen, xe-1, ye-1, 3, 3);

				// debug only - show bounding rectangles:
				//graphics.DrawRectangle(Project.debugPen, m_boundingRect);			// red
				//graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);		// green
				//graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);		// yellow
				//graphics.DrawRectangle(Project.debug3Pen, intersectionSensitiveRect2());		// yellow
			}
		}

		protected Brush brushByDepth()
		{
			double depth = -Location.Z; // meters
			int colorIndex = (int)(depth / 5000.0);
			int maxIndex = depthPalette.Length - 1;
			if(colorIndex > maxIndex) 
			{
				colorIndex = maxIndex;
			} 
			else if(colorIndex < 0) 
			{
				colorIndex = 0;
			}

			return depthPalette[colorIndex];
		}

		protected override void recalcLabelMetrics(string label, Graphics graphics)
		{
			// other threads can throw "invalid argument" exception in g.MeasureString:
			SizeF size = graphics.MeasureString(label, mm_font);
			m_labelWidth = (int)(size.Width * 0.95d); 
			m_labelHeight = (int)(size.Height * 0.85d);
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
			// returns rectangle around diagonal line connecting eq point with label and the endpoint oval

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
