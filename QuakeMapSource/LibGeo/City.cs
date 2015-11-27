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
	public class City : LiveObject
	{
		protected string    m_country;      // CC1 field in NIMA placenames, two-letter code, see nima_countries.txt, or end of this file
		public string       Country { get { return m_country; } set { m_country = value; } }

		protected string	m_state;		// e.g. CA
		public string       State { get { return m_state; } set { m_state = value; } }

		protected string	m_county;		// e.g. Orange
		public string       County { get { return m_county; } set { m_county = value; } }

		protected int		m_population;	// int holds -2,147,483,648 to 2,147,483,647
		public int          Population { get { return m_population; } set { m_population = value; } }

		protected int       m_importance;   // PC field in NIMA placenames, 1 - high, 5 - low
		public int          Importance { get { return m_importance; } set { m_importance = value; } }

		protected string    m_dsg = null;   // last characters after PPL in DSG field in NIMA placenames (or null for common places) - 
											//  - so, "C" stands for country capital, and "A" for admin unit capital (state capital).
		public string       Dsg { get { return m_dsg; } set { m_dsg = value; } }

		public const int ORDER_NONE				= 0;
		public const int ORDER_BY_NAME			= 1;
		public const int ORDER_BY_POPULATION	= 2;
		public const int ORDER_NATURAL			= 3;	// first by population, then name

		public const int ORDER_DEFAULT			= ORDER_NATURAL;

		// we need to have a handy City object for GetType() comparisons:
		public static City city = new City();

		public City() {}

		public City(string name, GeoCoord loc, string country, string state,
			string county, int population, int importance, string dsg) : base(name, loc)
		{
			m_country = country;
			m_state = state;
			m_county = county;
			m_population = population;
			m_importance = importance;
			if(dsg != null && dsg.Length > 0) { m_dsg = dsg; }
			CleanName();
		}

		public City(XmlNode node)		// may throw an exception if XML node is not right
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
						string tmpDsg = pnode.InnerText;
						if(tmpDsg != null && (tmpDsg.Equals("C") || tmpDsg.Equals("A"))) 
						{
							m_dsg = tmpDsg;
						}
						break;
					case "p13":
						m_importance = Convert.ToInt32(pnode.InnerText);
						break;
					case "p14":
						m_country = pnode.InnerText;
						break;
					case "p15":
						m_state = pnode.InnerText;
						break;
					case "p16":
						m_county = pnode.InnerText;
						break;
					case "p17":
						m_population = Convert.ToInt32(pnode.InnerText);
						break;
					case "p25":
						m_name = pnode.InnerText;
						break;
				}
			}
			m_location = new GeoCoord(lng, lat);
			CleanName();
		}

		public override string ToString()
		{
			return Location + " " + Name.PadRight(20) + " " + Country + " " + State +
				" " + (County==null?" ":County).PadRight(12) + " " + Population + " " + Dsg + " " + Importance;
		}

		public override string toTableString()
		{
			string popString = (Population > 0) ?
				("" + Population + " people") : "";
			string nameString = (Name +
				"                                 ").Substring(0,20);
			string countyString = (County == null ? "                              "
				: (m_county + (County.StartsWith("zipcode") ? "         " : " county ")
				+	"                                 ")).Substring(0,23);
			return nameString + " " + Location + " " + Country + " " + State +
				"  " + countyString + " " + popString + " " + (Dsg == null? " " : Dsg);
		}

		public bool sameAs(City other)
		{
			return Name.ToLower().Equals(other.Name.ToLower()) && Location.Equals(Location);
		}

		public int compareTo(Object other, int criteria)
		{
			int ret = 0;
			switch(criteria) 
			{
				//case ORDER_NONE:
				//	return 0;
				case ORDER_BY_NAME:
					ret = base.CompareTo(other, criteria);
					break;
				case ORDER_BY_POPULATION:
					ret = ((City)other).Population - this.Population;
					break;
				case ORDER_NATURAL:
					int popdiff = ((City)other).Population - this.Population;
					if((this.Population != 0 || ((City)other).Population != 0) && popdiff != 0) 
					{
						return popdiff;
					}
					ret = base.CompareTo(other, criteria);
					break;
			}
			return ret;
		}

		//
		// map-related (visual) part below
		//

		public const int MIN_CITY_PIXEL_RADIUS = 3; // default size if !doSize
		protected bool isCapital = false;
		protected bool isStateCapital = false;
		protected int m_bigCity = 0;

		protected int m_fontSize;
		public int    FontSize { get { return m_fontSize; } set { m_fontSize = value; } }

		public override void init(bool doName)
		{
			base.init(doName);

			m_bigCity = 0;
			isCapital = false;
			isStateCapital = false;
		}

		public override void PutOnMap(IDrawingSurface tileSet, ITile iTile, IObjectsLayoutManager iOlm)
		{
			base.PutOnMap(tileSet, iTile, iOlm);

			// font size will be calculated depending on population
			m_fontSize = Project.CITY_FONT_SIZE;

			//LibSys.StatusBar.Trace("PutOnMap():  City: - " + Location + " - " + Name);
			m_enabled = true;

			m_pixelRadius = MIN_CITY_PIXEL_RADIUS;

			if(m_dsg != null) 
			{
				if(m_dsg.Equals("C")) 
				{       // capital of a country
					isCapital = true;
					m_bigCity = 5;
				}
				if(m_dsg.Equals("A")) 
				{     // capital of a state
					isStateCapital = true;
					m_bigCity = 4;
				}
			}
        
			// we somewhat cheat here to make sizes of important cities and capitals depend on their importance:
			if(m_population == 0) 
			{
				if(m_dsg != null) 
				{
					if(isCapital) 
					{       // capital of a country
						m_population = 5000000;
					}
					if(isStateCapital) 
					{     // capital of a state
						m_population = 1000000;
					} 
					else 
					{
						m_population = 50000;
					}
				} 
				else 
				{
					switch (m_importance) 
					{
						case 1:     // high
							m_population = 2000000;
							m_bigCity = Math.Max(5, m_bigCity);
							break;
						case 2:     // kind of high
							m_population = 1000000;
							m_bigCity = Math.Max(4, m_bigCity);
							break;
						case 3:     // average
							m_population = 500000;
							m_bigCity = Math.Max(2, m_bigCity);
							break;
						case 4:     // rather low
							m_population = 100000;
							break;
						case 5:     // low
							m_population = 50000;
							break;
							// non-important cities just stay with population = 0
					}
				}
			} 
			else 
			{
				if(m_population >= 2000000) 
				{
					m_bigCity = 5;
				}
				else if(m_population >= 1000000) 
				{
					m_bigCity = 4;
				}
			}
			// end of population cheating
        
			if(m_doSize) 
			{
				if(m_population != 0) 
				{
					double sqMetersPerPerson = 100.0d;
					double radiusMeters = Math.Sqrt(sqMetersPerPerson * m_population / 3.14d);
					int rx = (int)(radiusMeters / m_map.xMetersPerPixel());
					int ry = (int)(radiusMeters / m_map.yMetersPerPixel());
					m_pixelRadius = (rx + ry) / 2;
                
					if(m_pixelRadius < (MIN_CITY_PIXEL_RADIUS + 2)) 
					{
						m_pixelRadius = MIN_CITY_PIXEL_RADIUS + 2;
					}
				}
				// cities with 0 population stay at MIN_CITY_PIXEL_RADIUS
			}
        
			if(m_doName) 
			{
				// we increase font size only if the city will be displayed
				// as a bigger circle on the map. Otherwise on a world map
				// the font will be unjustifiably large
				int delta = m_pixelRadius - MIN_CITY_PIXEL_RADIUS;
				if(delta > 0) 
				{
					if(m_population > 800000) 
					{
						m_fontSize += Math.Min(delta, 9);
					} 
					else if(m_population > 400000) 
					{
						m_fontSize += Math.Min(delta, 7);
					} 
					else if(m_population > 150000) 
					{
						m_fontSize += Math.Min(delta, 5);
					} 
					else if(m_population > 50000) 
					{
						m_fontSize += Math.Min(delta, 3);
					} 
					else 
					{
						m_fontSize += Math.Min(delta, 1);
					}
				}
            
				if(isCapital) 
				{
					m_fontSize += 2;
					m_pixelRadius += 2;
				} 
				else if(isStateCapital) 
				{
					//m_fontSize += 2;
					//m_pixelRadius += 2;
				}
            
				/*
				if(bigCity) {
					LibSys.StatusBar.Trace("city=" + m_city.name()
												+ " delta=" + delta	+ " font=" + m_fontSize);
				}
				*/
            
				placeLabel(m_bigCity, m_fontSize, false);	// may turn m_doName to false
			} 
			else 
			{
				labelBoundingRect();	// Empty
			}
        
			boundingRect();		// make sure we have current values there

			//LibSys.StatusBar.Trace("PutOnMap():  City: - " + Location + " - " + Name + " BR=" + m_boundingRect + " LBR=" + m_labelBoundingRect + " doName=" + m_doName);
		}
		
		public override void AdjustPlacement(IDrawingSurface map, ITile iTile, IObjectsLayoutManager iOlm)
		{
			if(m_bigCity > 0 || m_doName && m_intersections > 0) 
			{
				//LibSys.StatusBar.Trace("AdjustPlacement():  City: - " + Location + " - " + Name + " int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
				placeLabel(m_bigCity, m_fontSize, true);	// may turn m_doName to false
				boundingRect();		// make sure we have current values there
				//LibSys.StatusBar.Trace("                int=" + m_intersections + " pos=" + m_labelPosition + " doName=" + m_doName);
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

		public override void Paint(Graphics graphics, IDrawingSurface tileSet, ITile itile, int offsetX, int offsetY)
		{

			if(!m_enabled) 
			{
				return;
			}

			int toffsetX = itile.getOffset().X;
			//LibSys.StatusBar.Trace("City::Paint()  - " + Location + " - " + Name + " : " + m_doName + m_labelBoundingRect + " toffsetX=" + toffsetX + " : " + itile);
        
			Rectangle r = m_imageBoundingRect;
			r.Offset(toffsetX, 0);
        
			bool dontDrawCircle = (r.Width > 1000 || Project.terraserverAvailable
												&& ("aerial".Equals(Project.drawTerraserverMode) || "color aerial".Equals(Project.drawTerraserverMode))
												&& Project.terraLayerOpacity > 0.5d);
        
			Font font = Project.getLabelFont(m_fontSize);
			
			if(m_doName) 
			{
				int xx;
				int yy;
				if(dontDrawCircle && r.Width > m_labelBoundingRect.Width)
				{
					xx = r.X + r.Width/2 - m_labelBoundingRect.Width/2;
					yy = r.Y + r.Height/2 - m_labelBoundingRect.Height;
				}
				else
				{
					xx = m_labelBoundingRect.X - 3 + toffsetX;
					yy = m_labelBoundingRect.Y - 1;
				}
				xx += offsetX;
				yy += offsetY;
				//string label = "" + m_labelPosition + Name;
				string label = Name;
				if(Project.cityUseShadow) 
				{
					graphics.DrawString(label, font, Project.cityBackgroundBrush, xx,   yy);
					graphics.DrawString(label, font, Project.cityBackgroundBrush, xx+2, yy);
					graphics.DrawString(label, font, Project.cityBackgroundBrush, xx,   yy-2);
					graphics.DrawString(label, font, Project.cityBackgroundBrush, xx+2, yy-2);
				}
				graphics.DrawString(label, font, Project.cityBrush, xx+1, yy-1);
			}
        
			if(r.Width < 10 || !dontDrawCircle)
			{
				if(Project.cityUseShadow) 
				{
					graphics.DrawEllipse(Project.cityBackgroundPen, r.X+3+offsetX, r.Y+3+offsetY, r.Width-5, r.Height-5);
					graphics.DrawEllipse(Project.cityBackgroundPen, r.X+1+offsetX, r.Y+1+offsetY, r.Width-2, r.Height-2);
				}
				graphics.DrawEllipse(Project.cityPen, r.X+2+offsetX, r.Y+2+offsetY, r.Width-4, r.Height-4);
			}
        
			// debug only:
			//if(m_bigCity > 0) 
			//{
//				graphics.DrawRectangle(Project.debugPen, m_boundingRect);
//				graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);
//				graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);
			//}
		}
	}

	
	/*
	 Country   Country
	 Code      Name

	  AA     ARUBA    
	  AC     ANTIGUA AND BARBUDA      
	  AE	 UNITED ARAB EMIRATES
	  AF     AFGHANISTAN      
	  AG     ALGERIA  
	  AJ     AZERBAIJAN       
	  AL     ALBANIA  
	  AM     ARMENIA  
	  AN     ANDORRA  
	  AO     ANGOLA   
	  AQ     AMERICAN SAMOA   
	  AR     ARGENTINA
	  AS     AUSTRALIA
	  AT     ASHMORE AND CARTIER ISLANDS      
	  AU     AUSTRIA  
	  AV     ANGUILLA 
	  AY     ANTARCTICA       
	  BA     BAHRAIN  
	  BB     BARBADOS 
	  BC     BOTSWANA 
	  BD     BERMUDA  
	  BE     BELGIUM  
	  BF     BAHAMAS, THE  
	  BG     BANGLADESH       
	  BH     BELIZE   
	  BK     BOSNIA AND HERZEGOVINA   
	  BL     BOLIVIA  
	  BM     BURMA    
	  BN     BENIN    
	  BO     BELARUS  
	  BP     SOLOMON ISLANDS  
	  BQ     NAVASSA ISLAND   
	  BR     BRAZIL   
	  BS     BASSAS DA INDIA   
	  BT     BHUTAN   
	  BU     BULGARIA 
	  BV     BOUVET ISLAND    
	  BX     BRUNEI   
	  BY     BURUNDI  
	  CA     CANADA   
	  CB     CAMBODIA 
	  CD     CHAD     
	  CE     SRI LANKA
	  CF     CONGO, REPUBLIC OF THE
	  CG     CONGO, DEMOCRATIC REPUBLIC OF THE
	  CH     CHINA    
	  CI     CHILE    
	  CJ     CAYMAN ISLANDS   
	  CK     COCOS (KEELING) ISLANDS  
	  CM     CAMEROON 
	  CN     COMOROS  
	  CO     COLOMBIA 
	  CQ     NORTHERN MARIANA ISLANDS 
	  CR     CORAL SEA ISLANDS
	  CS     COSTA RICA       
	  CT     CENTRAL AFRICAN REPUBLIC 
	  CU     CUBA     
	  CV     CAPE VERDE       
	  CW     COOK ISLANDS     
	  CY     CYPRUS   
	  DA     DENMARK  
	  DJ     DJIBOUTI 
	  DO     DOMINICA 
	  DQ     JARVIS ISLAND    
	  DR     DOMINICAN REPUBLIC       
	  EC     ECUADOR  
	  EG     EGYPT    
	  EI     IRELAND  
	  EK     EQUATORIAL GUINEA
	  EN     ESTONIA  
	  ER     ERITREA  
	  ES     EL SALVADOR      
	  ET     ETHIOPIA 
	  EU     EUROPA ISLAND    
	  EZ     CZECH REPUBLIC   
	  FG     FRENCH GUIANA    
	  FI     FINLAND  
	  FJ     FIJI     
	  FK     FALKLAND ISLANDS 
	  FM     MICRONESIA, FEDERATED STATES OF
	  FO     FAROE ISLANDS    
	  FP     FRENCH POLYNESIA 
	  FQ     BAKER ISLAND     
	  FR     FRANCE   
	  FS     FRENCH SOUTHERN AND ANTARCTIC LANDS      
	  GA     GAMBIA, THE      
	  GB     GABON    
	  GG     GEORGIA  
	  GH     GHANA    
	  GI     GIBRALTAR
	  GJ     GRENADA  
	  GK     GUERNSEY 
	  GL     GREENLAND
	  GM     GERMANY  
	  GO     GLORIOSO ISLANDS 
	  GP     GUADELOUPE       
	  GQ     GUAM     
	  GR     GREECE   
	  GT     GUATEMALA
	  GV     GUINEA   
	  GY     GUYANA   
	  GZ     GAZA STRIP       
	  HA     HAITI    
	  HK     HONG KONG
	  HM     HEARD ISLAND AND MCDONALD ISLANDS
	  HO     HONDURAS 
	  HQ     HOWLAND ISLAND   
	  HR     CROATIA  
	  HU     HUNGARY  
	  IC     ICELAND  
	  ID     INDONESIA
	  IM     MAN, ISLE OF     
	  IN     INDIA    
	  IO     BRITISH INDIAN OCEAN TERRITORY   
	  IP     CLIPPERTON ISLAND
	  IR     IRAN     
	  IS     ISRAEL   
	  IT     ITALY    
	  IV     COTE D'IVOIRE    
	  IZ     IRAQ     
	  JA     JAPAN    
	  JE     JERSEY   
	  JM     JAMAICA  
	  JN     JAN MAYEN
	  JO     JORDAN   
	  JQ     JOHNSTON ATOLL   
	  JU     JUAN DE NOVA ISLAND      
	  KE     KENYA    
	  KG     KYRGYZSTAN       
	  KN     KOREA, DEMOCRATIC PEOPLE'S REPUBLIC OF   
	  KQ     KINGMAN REEF     
	  KR     KIRIBATI 
	  KS     KOREA, REPUBLIC OF       
	  KT     CHRISTMAS ISLAND 
	  KU     KUWAIT   
	  KZ     KAZAKHSTAN       
	  LA     LAOS     
	  LE     LEBANON  
	  LG     LATVIA   
	  LH     LITHUANIA
	  LI     LIBERIA  
	  LO     SLOVAKIA 
	  LQ     PALMYRA ATOLL    
	  LS     LIECHTENSTEIN    
	  LT     LESOTHO  
	  LU     LUXEMBOURG       
	  LY     LIBYA    
	  MA     MADAGASCAR       
	  MB     MARTINIQUE       
	  MC     MACAU    
	  MD     MOLDOVA  
	  MF     MAYOTTE  
	  MG     MONGOLIA 
	  MH     MONTSERRAT       
	  MI     MALAWI   
	  MK     MACEDONIA, THE FORMER YUGOSLAV REPUBLIC OF    
	  ML     MALI     
	  MN     MONACO   
	  MO     MOROCCO  
	  MP     MAURITIUS
	  MQ     MIDWAY ISLANDS   
	  MR     MAURITANIA       
	  MT     MALTA    
	  MU     OMAN     
	  MV     MALDIVES 
	  MW     MONTENEGRO       
	  MX     MEXICO   
	  MY     MALAYSIA 
	  MZ     MOZAMBIQUE       
	  NC     NEW CALEDONIA    
	  NE     NIUE     
	  NF     NORFOLK ISLAND   
	  NG     NIGER    
	  NH     VANUATU  
	  NI     NIGERIA  
	  NL     NETHERLANDS      
	  NO     NORWAY   
	  NP     NEPAL    
	  NR     NAURU    
	  NS     SURINAME 
	  NT     NETHERLANDS ANTILLES
	  NU     NICARAGUA
	  NZ     NEW ZEALAND      
	  PA     PARAGUAY 
	  PC     PITCAIRN ISLANDS 
	  PE     PERU     
	  PF     PARACEL ISLANDS  
	  PG     SPRATLY ISLANDS  
	  PK     PAKISTAN 
	  PL     POLAND   
	  PM     PANAMA   
	  PO     PORTUGAL 
	  PP     PAPUA NEW GUINEA 
	  PS	 PALAU, REPUBLIC OF
	  PU     GUINEA-BISSAU    
	  QA     QATAR    
	  RE     REUNION  
	  RO     ROMANIA  
	  RP     PHILIPPINES      
	  RQ     PUERTO RICO      
	  RM     MARSHALL ISLANDS
	  RS     RUSSIA   
	  RW     RWANDA   
	  SA     SAUDI ARABIA     
	  SB     ST. PIERRE AND MIQUELON  
	  SC     ST. KITTS AND NEVIS      
	  SE     SEYCHELLES       
	  SF     SOUTH AFRICA     
	  SG     SENEGAL  
	  SH     ST. HELENA       
	  SI     SLOVENIA 
	  SL     SIERRA LEONE     
	  SM     SAN MARINO       
	  SN     SINGAPORE
	  SO     SOMALIA  
	  SP     SPAIN    
	  SR     SERBIA   
	  ST     ST. LUCIA
	  SU     SUDAN    
	  SV     SVALBARD 
	  SW     SWEDEN   
	  SX     SOUTH GEORGIA AND THE SOUTH SANDWICH ISLANDS     
	  SY     SYRIA    
	  SZ     SWITZERLAND      
	  TD     TRINIDAD AND TOBAGO      
	  TE     TROMELIN ISLAND  
	  TH     THAILAND 
	  TI     TAJIKISTAN       
	  TK     TURKS AND CAICOS ISLANDS 
	  TL     TOKELAU  
	  TN     TONGA    
	  TO     TOGO     
	  TP     SAO TOME AND PRINCIPE    
	  TS     TUNISIA  
	  TT     EAST TIMOR
	  TU     TURKEY   
	  TV     TUVALU   
	  TW     TAIWAN   
	  TX     TURKMENISTAN     
	  TZ     TANZANIA, UNITED REPUBLIC OF     
	  UG     UGANDA   
	  UK     UNITED KINGDOM   
	  UP     UKRAINE  
	  US     UNITED STATES    
	  UV     BURKINA FASO     
	  UY     URUGUAY  
	  UZ     UZBEKISTAN       
	  VC     ST. VINCENT AND THE GRENADINES   
	  VE     VENEZUELA
	  VI     BRITISH VIRGIN ISLANDS   
	  VM     VIETNAM  
	  VQ     VIRGIN ISLANDS   
	  VT     VATICAN CITY     
	  WA     NAMIBIA  
	  WE     WEST BANK
	  WF     WALLIS AND FUTUNA
	  WI     WESTERN SAHARA   
	  WQ     WAKE ISLAND      
	  WS     SAMOA    
	  WZ     SWAZILAND
	  YM     YEMEN    
	  YO     YUGOSLAVIA       
	  ZA     ZAMBIA   
	  ZI     ZIMBABWE 

	 */
}
