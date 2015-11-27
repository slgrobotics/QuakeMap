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
using System.Xml;
using LibSys;

namespace LibGeo
{
	/// <summary>
	/// Summary description for Place.
	/// </summary>
	public class Place : LiveObject
	{
		protected int       m_importance;   // PC field in NIMA placenames, 1 - high, 5 - low
		public int          Importance { get { return m_importance; } set { m_importance = value; } }

		protected string    m_dsg;          // last characters after PPL in DSG field in NIMA placenames (or null for common places) - 
											//  - so, "C" stands for country capital, and "A" for admin unit capital (state capital).
		public string       Dsg { get { return m_dsg; } set { m_dsg = value; } }

		protected int       m_code = 0;     // DSG expressed as number
		protected static    Hashtable placeCodes = new Hashtable();

		// we need to have a handy Place object for GetType() comparisons:
		public static Place place = new Place();

		public Place() {}

		///
		/// we need the m_code and the hashtable below as a means to accelerate
		/// processing of different types of places, e.g. while drawing.
		/// Static constructor is called when the class is loaded at the first time.
		///
		static Place()
		{
			System.Console.WriteLine("IP: static Place() constructor");

			/*
				#!/bin/sh
				#
				# This script makes writes some Java code
				#
				#
				while read LINE
				do
						if [ -z "$LINE" ]
						then
						continue
						fi
						# echo $LINE
						set `echo $LINE`
						echo "placeCodes.Add(\""$2"\", "$1");"
				done < list.txt
			*
			* list in the form "196 ZNB A BUFFER ZONE"  - see bottom of this file
			*/
			placeCodes.Add("ADM1", 11);
			placeCodes.Add("ADM2", 12);
			placeCodes.Add("ADM3", 13);
			placeCodes.Add("ADM4", 14);
			placeCodes.Add("ADMD", 15);
			placeCodes.Add("AIRB", 21);
			placeCodes.Add("AIRF", 22);
			placeCodes.Add("AIRH", 23);
			placeCodes.Add("AIRP", 24);
			placeCodes.Add("ATOL", 25);
			placeCodes.Add("CNSU", 31);
			placeCodes.Add("CNYN", 32);
			placeCodes.Add("CNYU", 33);
			placeCodes.Add("CTRA", 34);
			placeCodes.Add("DAM", 35);
			placeCodes.Add("DAMQ", 36);
			placeCodes.Add("DAMSB", 37);
			placeCodes.Add("DSRT", 38);
			placeCodes.Add("ERG", 41);
			placeCodes.Add("FLLS", 42);
			placeCodes.Add("FLLSX", 43);
			placeCodes.Add("FRZU", 43);
			placeCodes.Add("GULF", 51);
			placeCodes.Add("HBR", 52);
			placeCodes.Add("HMDA", 53);
			placeCodes.Add("ISL", 61);
			placeCodes.Add("ISLF", 62);
			placeCodes.Add("ISLM", 63);
			placeCodes.Add("ISLS", 64);
			placeCodes.Add("ISLT", 65);
			placeCodes.Add("KRST", 66);
			placeCodes.Add("LAVA", 71);
			placeCodes.Add("LBED", 72);
			placeCodes.Add("LCTY", 73);
			placeCodes.Add("LGN", 74);
			placeCodes.Add("LGNS", 75);
			placeCodes.Add("LK", 76);
			placeCodes.Add("LKC", 77);
			placeCodes.Add("LKI", 78);
			placeCodes.Add("LKN", 79);
			placeCodes.Add("LKNI", 80);
			placeCodes.Add("LKO", 81);
			placeCodes.Add("LKOI", 82);
			placeCodes.Add("LKS", 83);
			placeCodes.Add("LKSB", 84);
			placeCodes.Add("LKSC", 85);
			placeCodes.Add("LKSI", 86);
			placeCodes.Add("LKSN", 87);
			placeCodes.Add("LKSNI", 88);
			placeCodes.Add("LTHSE", 89);
			placeCodes.Add("MILB", 90);
			placeCodes.Add("MNA", 91);
			placeCodes.Add("MT", 92);
			placeCodes.Add("MTS", 93);
			placeCodes.Add("MTSU", 94);
			placeCodes.Add("MTU", 95);
			placeCodes.Add("NVB", 96);
			placeCodes.Add("OBS", 97);
			placeCodes.Add("OBSR", 98);
			placeCodes.Add("OCN", 101);
			placeCodes.Add("OILF", 102);
			placeCodes.Add("PCL", 103);
			placeCodes.Add("PCLD", 104);
			placeCodes.Add("PCLF", 105);
			placeCodes.Add("PCLI", 106);
			placeCodes.Add("PCLIX", 107);
			placeCodes.Add("PCLS", 108);
			placeCodes.Add("PEN", 109);
			placeCodes.Add("PK", 121);
			placeCodes.Add("PKLT", 122);
			placeCodes.Add("PKS", 123);
			placeCodes.Add("PKSU", 124);
			placeCodes.Add("PKU", 125);
			placeCodes.Add("PLAT", 126);
			placeCodes.Add("PLN", 127);
			placeCodes.Add("PLNU", 128);
			placeCodes.Add("PLNX", 129);
			placeCodes.Add("PLTU", 130);
			placeCodes.Add("PRK", 131);
			placeCodes.Add("PRT", 132);
			placeCodes.Add("PS", 133);
			placeCodes.Add("PSH", 134);
			placeCodes.Add("PYR", 135);
			placeCodes.Add("PYRS", 136);
			placeCodes.Add("RES", 140);
			placeCodes.Add("RESA", 141);
			placeCodes.Add("RESF", 142);
			placeCodes.Add("RESH", 143);
			placeCodes.Add("RESN", 144);
			placeCodes.Add("RESP", 145);
			placeCodes.Add("RESV", 146);
			placeCodes.Add("RESW", 147);
			placeCodes.Add("RF", 148);
			placeCodes.Add("RFC", 149);
			placeCodes.Add("RFSU", 150);
			placeCodes.Add("RFU", 151);
			placeCodes.Add("RGN", 160);
			placeCodes.Add("RGNE", 161);
			placeCodes.Add("RGNL", 162);
			placeCodes.Add("SEA", 170);
			placeCodes.Add("SHFU", 171);
			placeCodes.Add("STLMT", 172);
			placeCodes.Add("SWMP", 173);
			placeCodes.Add("TERR", 180);
			placeCodes.Add("VAL", 190);
			placeCodes.Add("VALS", 191);
			placeCodes.Add("VALU", 192);
			placeCodes.Add("VLC", 193);
			placeCodes.Add("VLSU", 194);
			placeCodes.Add("ZN", 195);
			placeCodes.Add("ZNB", 196);
		}
	    
		public static int codeByDsg(string dsg)
		{
			int code = 0;
			if(dsg != null) 
			{
				if(placeCodes.Contains(dsg)) {
					code = (int)placeCodes[dsg];
				}
			}
			return code;
		}

		public Place(string name, GeoCoord loc, int importance, string dsg) : base(name, loc)
		{
			m_importance = importance;
			m_dsg = dsg;
			m_code = codeByDsg(dsg);
			AdjustImportance();
			//LibSys.StatusBar.Trace("Place(plain): - " + this);
		}

		public Place(XmlNode node)		// may throw an exception if XML node is not right
		{
			double lng = 0.0d;
			double lat = 0.0d;
			foreach(XmlNode pnode in node.ChildNodes)
			{
				switch (pnode.Name) 
				{
					case "p4":
						lat = Convert.ToDouble(pnode.InnerText);
						break;
					case "p5":
						lng = Convert.ToDouble(pnode.InnerText);
						break;
					case "p12":
						m_dsg = pnode.InnerText;
						break;
					case "p13":
						m_importance = Convert.ToInt32(pnode.InnerText);
						break;
					case "p25":
						m_name = pnode.InnerText;
						break;
				}
			}
			m_location = new GeoCoord(lng, lat);
			CleanName();
			AdjustImportance();
			m_code = codeByDsg(m_dsg);
			//LibSys.StatusBar.Trace("Place(xml): - " + this);
		}

		protected void AdjustImportance()
		{
			if(m_dsg == null || m_dsg.Length == 0)
			{
				return;
			}
			if(m_dsg.Equals("SEA") || m_dsg.Equals("OCN")) 
			{    // seas and oceans
				m_importance = 1;
			// } else if(m_dsg.Equals("ADM1")) {    //
			//     m_importance = 2;
			} 
			else if(m_dsg.Equals("ISLS")|| m_dsg.Equals("ADM1")) 
			{    //
				m_importance = 3;
			// } else if() {    //
			//     m_importance = 4;
			} 
			else if(m_dsg.Equals("ISL") || m_dsg.Equals("PEN")
				// || m_dsg.Equals("ADM4") || m_dsg.Equals("ADM3") || m_dsg.Equals("ADM2")
				|| m_dsg.Equals("LK") || m_dsg.Equals("LKS")) 
			{    //
				m_importance = 5;
			}
		}

		public override string ToString()
		{
			return Location + " " + Name.PadRight(20) + " " + Dsg + " " + Importance;
		}

		public override string toTableString()
		{
			return Location + " " + Dsg.PadRight(7) + " " + Name;
		}

		public bool sameAs(Place other)
		{
			return Location.Equals(other.Location) || Name.Equals(other.Name);

			/*
			if(!m_location.Equals(other.location())) 
			{  // || (m_dsg != null && other.dsg() != null && !m_dsg.Equals(other.dsg()))) { 
				return false;
			}
			return true;
			/*
			boolean ret = m_name.equalsIgnoreCase(other.name());
			if(ret) {
				return ret;
			}
			// try eliminate name with ' - comparing heads and tails:
			int pos = m_name 
			ret =
			 **/
		}

		protected const int MIN_PLACE_PIXEL_RADIUS = 3; // default size if !doSize
		protected bool m_doDrawPoint = true;
		protected bool m_doDrawShadow = Project.placeUseShadow;
		protected Brush m_labelBrush;
		protected Brush m_labelShadowBrush;
		protected Pen m_pen;
		protected Pen m_shadowPen;
		protected int m_placeCode;
		protected int m_fontSize;
		public int    FontSize { get { return m_fontSize; } set { m_fontSize = value; } }

		public override void init(bool doName)
		{
			base.init(doName);
		}

		public override void PutOnMap(IDrawingSurface tileSet, ITile iTile, IObjectsLayoutManager tile)
		{
			base.PutOnMap(tileSet, iTile, tile);

			//LibSys.StatusBar.Trace("PutOnMap(): - " + this);

			m_pixelRadius = MIN_PLACE_PIXEL_RADIUS;

			m_enabled = true;

			int importance = Importance;
			string dsg = Dsg;

			m_placeCode = m_code;
			setColors();			// and doSize

			if(m_doName) 
			{

				int bigPlace = (6 - importance);    // 5-high, 0-low

				// font size will be calculated depending on population
				m_fontSize = Project.PLACE_FONT_SIZE;
				if(bigPlace > 0) 
				{
					switch(bigPlace)
					{
						case 5:
							m_fontSize += 3;
							break;
						default:
							m_fontSize += bigPlace / 2;
							break;
					}
				}

				placeLabel(bigPlace, m_fontSize, false);

			} 
			else 
			{
				m_imageBoundingRect = Rectangle.Empty;
			}

			boundingRect();		// make sure we have current values there
		}
		
		public void setColors()
		{
			m_labelShadowBrush = Project.placeBrush;
			m_shadowPen = Project.placeShadowPen;
			switch(m_placeCode) 
			{
				case 76:    // LK
				case 77:    // LKC
				case 78:    // LKI
				case 79:    // LKN
				case 80:    // LKNI
				case 81:    // LKo
				case 82:    // LKoI
				case 83:    // LKS
				case 84:    // LKSB
				case 85:    // LKSC
				case 86:    // LKSI
				case 87:    // LKSN
				case 88:    // LKSNI
					m_labelBrush = Project.blueBrush;
					m_pen = Project.bluePen;
					m_doDrawShadow = false;
					m_doSize = false;
					break;
				case 51:    // GULF
				case 101:   // OCN
				case 170:   // SEA
					m_labelBrush = Project.blueBrush;
					m_pen = Project.bluePen;
					m_doDrawPoint = false;
					m_doDrawShadow = false;
					m_doSize = false;
					break;
				case 103:   // PCL
				case 104:   // PCLD
				case 105:   // PCLF
				case 106:   // PCLI
				case 107:   // PCLIX
				case 108:   // PCLS
					m_labelBrush = Project.redBrush;
					m_pen = Project.redPen;
					m_doDrawPoint = false;
					m_doDrawShadow = false;
					break;
				case 61:   // ISL
				case 62:   // ISLF
				case 63:   // ISLM
				case 64:   // ISLS
				case 65:   // ISLT
					m_labelBrush = Project.blackBrush;
					m_pen = Project.blackPen;
					m_doDrawShadow = false;
					break;
				default:
					m_fontSize = Project.PLACE_FONT_SIZE;
					m_labelBrush = Project.placeBrush;
					m_pen = Project.placePen;
					m_doDrawPoint = false;
					m_doDrawShadow = false;
					break;
			}
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

			//LibSys.StatusBar.Trace("Paint():  Place: - " + Location + " - " + Name);
			Point pLoc = tileSet.toPixelLocation(Location, iTile);
			int toffsetX = iTile.getOffset().X;
        
			Rectangle r = m_imageBoundingRect;
			r.Offset(toffsetX, 0);

			Font font = Project.getLabelFont(m_fontSize);
			
			if(m_doName) 
			{
				int xx = m_labelBoundingRect.X - 3 + toffsetX + offsetX;
				int yy = m_labelBoundingRect.Y - 1 + offsetY;
				//string label = "" + m_labelPosition + Name;
				string label = Name;
				if(m_doDrawShadow) 
				{
					graphics.DrawString(label, font, m_labelShadowBrush, xx,   yy);
					graphics.DrawString(label, font, m_labelShadowBrush, xx+2, yy);
					graphics.DrawString(label, font, m_labelShadowBrush, xx,   yy-2);
					graphics.DrawString(label, font, m_labelShadowBrush, xx+2, yy-2);
				}
				graphics.DrawString(label, font, m_labelBrush, xx+1, yy-1);
			}

			if(m_doDrawPoint) 
			{
				if(m_doDrawShadow) 
				{
					graphics.DrawEllipse(m_shadowPen, r.X+3+offsetX, r.Y+3+offsetY, r.Width-5, r.Height-5);
					graphics.DrawEllipse(m_shadowPen, r.X+1+offsetX, r.Y+1+offsetY, r.Width-2, r.Height-2);
				}
				graphics.DrawEllipse(m_pen, r.X+2+offsetX, r.Y+2+offsetY, r.Width-4, r.Height-4);
			}

			// debug only:
//				graphics.DrawRectangle(Project.debugPen, m_boundingRect);
//				graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);
//				graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);
		}
	}
}


/*
 * Here are codes for places - only those that I chose to go into SELECT statement in doworld_make_fdbfile.pl
 * Full list in C:\USGS\placenames\nima\docs\nima_cross_ref.html
 * code is arbitralily put here by me.
 *
CODE DSG     CLASS
------------------------------------------------------------ 
11  ADM1       A       FIRST-ORDER ADMINISTRATIVE DIVISION
12  ADM2       A       SECOND-ORDER ADMINISTRATIVE DIVISION
13  ADM3       A       THIRD-ORDER ADMINISTRATIVE DIVISION
14  ADM4       A       FOURTH-ORDER ADMINISTRATIVE DIVISION
15  ADMD       A       ADMINISTRATIVE DIVISION

21  AIRB       S       AIRBASE
22  AIRF       S       AIRFIELD
23  AIRH       S       HELIPORT
24  AIRP       S       AIRPORT

25  ATOL       T       ATOLL(S)

31  CNSU       U       CANYONS
32  CNYN       T       CANYON
33  CNYU       U       CANYON

34  CTRA       S       ATOMIC CENTER

35  DAM        S       DAM
36  DAMQ       S       RUINED DAM
37  DAMSB      S       SUB-SURFACE DAM

38  DSRT       T       DESERT

41  ERG        T       SANDY DESERT

42  FLLS       H       WATERFALL(S)
43  FLLSX      H       SECTION OF WATERFALL(S)

43  FRZU       U       FRACTURE ZONE

51  GULF       H       GULF

52  HBR        H       HARBOR(S)

53  HMDA       T       ROCK DESERT

61  ISL        T       ISLAND
62  ISLF       T       ARTIFICIAL ISLAND
63  ISLM       T       MANGROVE ISLAND
64  ISLS       T       ISLANDS
65  ISLT       T       LAND-TIED ISLAND

66  KRST       T       KARST AREA

71  LAVA       T       LAVA AREA
72  LBED       H       LAKE BED(S)
73  LCTY       L       LOCALITY

74  LGN        H       LAGOON
75  LGNS       H       LAGOONS

76  LK         H       LAKE
77  LKC        H       CRATER LAKE
78  LKI        H       INTERMITTENT LAKE
79  LKN        H       SALT LAKE
80  LKNI       H       INTERMITTENT SALT LAKE
81  LKO        H       OXBOW LAKE
82  LKOI       H       INTERMITTENT OXBOW LAKE
83  LKS        H       LAKES
84  LKSB       H       UNDERGROUND LAKE
85  LKSC       H       CRATER LAKES
86  LKSI       H       INTERMITTENT LAKES
87  LKSN       H       SALT LAKES
88  LKSNI      H       INTERMITTENT SALT LAKES

89  LTHSE      S       LIGHTHOUSE

90  MILB       L       MILITARY BASE

91  MNA        L       MINING AREA

92  MT         T       MOUNTAIN
93  MTS        T       MOUNTAINS
94  MTSU       U       MOUNTAINS
95  MTU        U       MOUNTAIN

96  NVB        L       NAVAL BASE

97  OBS        S       OBSERVATORY
98  OBSR       S       RADIO OBSERVATORY

101  OCN        H       OCEAN
102  OILF       L       OILFIELD

103  PCL        A       POLITICAL ENTITY
104  PCLD       A       DEPENDENT POLITICAL ENTITY
105  PCLF       A       FREELY ASSOCIATED STATE
106  PCLI       A       INDEPENDENT POLITICAL ENTITY
107  PCLIX      A       SECTION OF INDEPENDENT POLITICAL ENTITY
108  PCLS       A       SEMI-INDEPENDENT POLITICAL ENTITY
109  PEN        T       PENINSULA

121  PK         T       PEAK
122  PKLT       S       PARKING LOT
123  PKS        T       PEAKS
124  PKSU       U       PEAKS
125  PKU        U       PEAK
126  PLAT       T       PLATEAU

127  PLN        T       PLAIN(S)
128  PLNU       U       PLAIN
129  PLNX       T       SECTION OF PLAIN
130  PLTU       U       PLATEAU

131  PRK        L       PARK

132  PRT        L       PORT

133  PS         S       POWER STATION
134  PSH        S       HYDROELECTRIC POWER STATION

135  PYR        S       PYRAMID
136  PYRS       S       PYRAMIDS

140  RES        L       RESERVE
141  RESA       L       AGRICULTURAL RESERVE
142  RESF       L       FOREST RESERVE
143  RESH       L       HUNTING RESERVE
144  RESN       L       NATURE RESERVE
145  RESP       L       PALM TREE RESERVE
146  RESV       L       RESERVATION
147  RESW       L       WILDLIFE RESERVE
148  RF         H       REEF(S)
149  RFC        H       CORAL REEF(S)
150  RFSU       U       REEFS
151  RFU        U       REEF

160  RGN        L       REGION
161  RGNE       L       ECONOMIC REGION
162  RGNL       L       LAKE REGION

170  SEA        H       SEA
171  SHFU       U       SHELF

172  STLMT      P       ISRAELI SETTLEMENT

173  SWMP       H       SWAMP

180  TERR       A       TERRITORY

190  VAL        T       VALLEY
191  VALS       T       VALLEYS
192  VALU       U       VALLEY
193  VLC        T       VOLCANO
194  VLSU       U       VALLEYS

195  ZN         A       ZONE
196  ZNB        A       BUFFER ZONE

 */