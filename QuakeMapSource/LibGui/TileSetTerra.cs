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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

using LibSys;
using LibGeo;
using LibNet.TerraServer;
using LonLatPt = LibNet.TerraServer.LonLatPt;
using LibNet.LandmarkServer;
using LibNet;

namespace LibGui
{
	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://terraserver-usa.com/terraserver/")]
	public enum Theme 
	{
		Photo = 1,
		Topo = 2,
		Relief = 3,
		Urban = 4
	}
    

	/// <summary>
	/// TileSetTerra is responsible for instantiating a matrix of tiles needed to cover whole
	/// area of the PictureBox. It directs drawing events to individual tiles, so that they could draw themselves.
	/// It reacts on resizing and rescaling of PictureBox.
	/// It provides "drawing surface" services to cities etc.
	/// </summary>
	public abstract class TileSetTerra : Layer, IObjectsLayoutManager
	{
		public static TileSetTerra This = null;	// make instance available for UTMP projection in Camera Manager.

		// second instance of TileSetTerra is used to overlay topo map on aerial/color
		private bool m_isSecond = false;
		public bool isSecond { get { return m_isSecond; } }
		private bool isPassiveSecond { get { return m_isSecond && (!Project.terraUseOverlay || "topo".Equals(Project.drawTerraserverMode)); } }

		protected double m_tileResolutionDegreesH;	// degrees per tile - horizontal
		protected double m_tileResolutionDegreesV;	// degrees per tile - vertical
		public double TileResolutionDegreesH { get { return m_tileResolutionDegreesH; } }
		public double TileResolutionDegreesV { get { return m_tileResolutionDegreesV; } }

		private string tilesLock = "";
		protected int m_vCount = 0;		// m_tiles array dimensions - vertical and horizontal
		protected int m_hCount = 0;
		protected TileTerra[,] m_tiles = null;

		protected int m_fontSize = 8;
		public int FontSize { get { return m_fontSize; } set { m_fontSize = value; } }
		protected Font m_font = null;
		protected bool m_hasPutOnMap = false;	// landmarks
		protected bool m_needsRefresh = false;
		public bool HasPutOnMap { get { return m_hasPutOnMap; } set { m_hasPutOnMap = value; } }
		protected bool m_retilingSpecial = false;
		public bool RetilingSpecial { get { return m_retilingSpecial; } set { m_retilingSpecial = value; } }

		// evaluation spoiler constants and variables:
		private const string m_evalWord = "Evaluation";
		private const int m_ehCount = 8, m_evCount = 8;
		private int m_evalWordLength;
		private byte[] m_rands;
		private int m_treshold;
		private int m_daysSinceInstall = Project.TRIAL_DAYS;
		private int m_opacity = 255;	// 100 is almost transparent, 200 looks like 50%, 255 is opaque

		public TileSetTerra(PictureManager pm, CameraManager cm) : base(pm, cm)
		{
			TerraserverCache.PictureManager = pm;
			TerraserverCache.CameraManager = cm;

			if(This == null)
			{
				This = this;	// let camera manager and others use coordinate transformers
				TerraserverCache.init();
			}
			else
			{
				m_isSecond = true;	// we are topo when the first one is aerial/color
			}

			m_font = Project.getLabelFont(m_fontSize);
			m_evalWordLength = m_evalWord.Length;

			m_opacity = 0;
		}

		public override void shutdown()
		{
			TerraserverCache.shutdown();
		}

		public void cleanTiles()
		{
			lock(tilesLock)
			{
				if(m_tiles != null)
				{
					TileTerra[,] tmp = m_tiles;
					m_tiles = null;
					foreach(TileTerra t in tmp) 
					{
						if(t != null) 
						{
							t.Dispose();
						}
					}
				}
				m_hCount = 0;
				m_vCount = 0;
			}
		}

		// support for Projection:
		public bool hasRenderedTiles = false;
		private TileTerra m_topLeftTile = null;
		private Int32  screenUtmX, screenUtmY, screenUtmZone = 0;
		private Scale  m_scale;
		private Int32  m_metersPerPixel;
		private double m_ratioX;
		private double m_ratioY;
		private double m_ratioXPrint;
		private double m_ratioYPrint;
		private double m_offsetX;
		private double m_offsetY;
		private double m_offsetXPrint;
		private double m_offsetYPrint;

		internal static int getZone(GeoCoord loc)
		{
			LonLatPt lonlat = new LonLatPt();
			lonlat.Lat = loc.Lat;
			lonlat.Lon = loc.Lng;

			int zone = Projection.LonLatPtToZone(lonlat);

			return zone;
		}

		public bool insideCurrentZone(GeoCoord loc)
		{
			return getZone(loc) == screenUtmZone;
		}

		private void calcRatios()
		{
			// compute parameters for Projection:
			Rectangle tileRect = m_topLeftTile.getFrameRectangle();
			m_ratioX = ((double)tileRect.Width - 1.5d) / 200.0d;		// -1.5 corrects added 1 in getFrameRectangle(), and probability of rounding
			m_ratioY = ((double)tileRect.Height - 1.5d) / 200.0d;
			m_offsetX = tileRect.X;
			m_offsetY = tileRect.Y;
		}

		public void calcPrintRatios()
		{
			try
			{
				if(hasRenderedTiles)
				{
					Rectangle tileRectPrint = m_topLeftTile.getFrameRectanglePrint();
					tileRectPrint.Offset(-m_pictureManager.OffsetXPrint, -m_pictureManager.OffsetYPrint);
					m_ratioXPrint = ((double)tileRectPrint.Width - 1.5d) / 200.0d;
					m_ratioYPrint = ((double)tileRectPrint.Height - 1.5d) / 200.0d;
					m_offsetXPrint = tileRectPrint.X;
					m_offsetYPrint = tileRectPrint.Y;
				} 
			}
			catch {}
		}

		public Point toScreenPoint(GeoCoord loc, bool isPrint)
		{
			Point ret = new Point();

			LonLatPt lonlat = new LonLatPt();
			lonlat.Lat = loc.Lat;
			lonlat.Lon = loc.Lng;

			UtmPt utmpt = Projection.LonLatPtToUtmNad83Pt(lonlat);

			// calculate where the point would be before the tiles are scaled to the screen:
			double pX = (utmpt.X - screenUtmX) / m_metersPerPixel;		// in pixels
			double pY = (screenUtmY - utmpt.Y) / m_metersPerPixel;

			// now scale it to the screen:
			if(isPrint)
			{
				double offsetX = m_offsetXPrint / m_ratioXPrint;
				double offsetY = m_offsetYPrint / m_ratioYPrint;

				pX += offsetX;
				pY += offsetY + 200;

				// and scale it back:
				pX *= m_ratioXPrint;
				pY *= m_ratioYPrint;
			}
			else
			{
				double offsetX = m_offsetX / m_ratioX;
				double offsetY = m_offsetY / m_ratioY;

				pX += offsetX;
				pY += offsetY + 200;

				// and scale it back:
				pX *= m_ratioX;
				pY *= m_ratioY;
			}
			ret.X = (int)pX;
			ret.Y = (int)pY;

			return ret;
		}

		public GeoCoord toGeoLocation(Point point, bool isPrint)
		{
			GeoCoord  ret = new GeoCoord(0.0d, 0.0d);

			double pX, pY;

			if(isPrint)
			{
				double offsetX = m_offsetXPrint / m_ratioXPrint;
				double offsetY = m_offsetYPrint / m_ratioYPrint;

				// this would be pixel screen coord if there were no scaling:
				pX = point.X / m_ratioXPrint - offsetX;	// tile pixels before scaling
				pY = - point.Y / m_ratioYPrint + offsetY + 200;
			}
			else
			{
				double offsetX = m_offsetX / m_ratioX;	// tile pixels before scaling
				double offsetY = m_offsetY / m_ratioY;

				// this would be pixel screen coord if there were no scaling:
				pX = point.X / m_ratioX - offsetX;	// tile pixels before scaling
				pY = - point.Y / m_ratioY + offsetY + 200;
			}

			// now calculate it in meters and offset the screen UTM
			pX = pX * m_metersPerPixel + screenUtmX;
			pY = pY * m_metersPerPixel + screenUtmY;

			UtmPt utmpt = new UtmPt();
			utmpt.X = pX;
			utmpt.Y = pY;
			utmpt.Zone = screenUtmZone;

			LonLatPt lonlat = Projection.UtmNad83PtToLonLatPt(utmpt);

			ret.Lat = lonlat.Lat;
			ret.Lng = lonlat.Lon;

			return ret;
		}

		struct ScaleDescr 
		{
			public Scale scale;
			public string scaleName;
			public bool supportedPhoto;
			public bool supportedColorPhoto;
			public bool supportedTopo;
			public bool supportedRelief;

			public ScaleDescr(Scale s, string sn, bool sp, bool sc, bool st, bool sr)
			{
				scale = s;
				scaleName = sn;
				supportedPhoto = sp;
				supportedColorPhoto = sc;
				supportedTopo = st;
				supportedRelief = sr;
			}
		}

		static ScaleDescr[] m_scales = new ScaleDescr[]
		{
			//
			//          all scalesfrom TerraServer/References.cs (via Class View-->LibNet)
			//			Photo valid 10 to 16, topo 11 to 21 (in reality to 17), relief 20 to 24
			//
			//                       scale    |  name    |  photo | color | topo  | relief
			//  ----------------------------------------------------------------------------
			new ScaleDescr(Scale.Scale1mm,		"1mm",		false,	false,	false,	false),			// 0
			new ScaleDescr(Scale.Scale2mm,		"2mm",		false,	false,	false,	false),			// 1
			new ScaleDescr(Scale.Scale4mm,		"4mm",		false,	false,	false,	false),			// 2
			new ScaleDescr(Scale.Scale8mm,		"8mm",		false,	false,	false,	false),			// 3
			new ScaleDescr(Scale.Scale16mm,		"16mm",		false,	false,	false,	false),			// 4
			new ScaleDescr(Scale.Scale32mm,		"32mm",		false,	false,	false,	false),			// 5
			new ScaleDescr(Scale.Scale63mm,		"63mm",		false,	false,	false,	false),			// 6
			new ScaleDescr(Scale.Scale125mm,	"125mm",	false,	false,	false,	false),			// 7
			new ScaleDescr(Scale.Scale250mm,	"250mm",	false,	true,	false,	false),			// 8
			new ScaleDescr(Scale.Scale500mm,	"500mm",	false,	true,	false,	false),			// 9
			new ScaleDescr(Scale.Scale1m,		"1m",		true,	true,	false,	false),			// 10
			new ScaleDescr(Scale.Scale2m,		"2m",		true,	true,	true,	false),			// 11
			new ScaleDescr(Scale.Scale4m,		"4m",		true,	true,	true,	false),			// 12
			new ScaleDescr(Scale.Scale8m,		"8m",		true,	true,	true,	false),			// 13
			new ScaleDescr(Scale.Scale16m,		"16m",		true,	true,	true,	false),			// 14
			new ScaleDescr(Scale.Scale32m,		"32m",		true,	true,	true,	false),			// 15
			new ScaleDescr(Scale.Scale64m,		"64m",		true,	true,	true,	false),			// 16
			new ScaleDescr(Scale.Scale128m,		"128m",		false,	false,	true,	false),			// 17
			new ScaleDescr(Scale.Scale256m,		"256m",		false,	false,	false,	false),			// 18
			new ScaleDescr(Scale.Scale512m,		"512m",		false,	false,	false,	false),			// 19
			new ScaleDescr(Scale.Scale1km,		"1km",		false,	false,	false,	true),			// 20
			new ScaleDescr(Scale.Scale2km,		"2km",		false,	false,	false,	true),			// 21
			new ScaleDescr(Scale.Scale4km,		"4km",		false,	false,	false,	true),			// 22
			new ScaleDescr(Scale.Scale8km,		"8km",		false,	false,	false,	true),			// 23
			new ScaleDescr(Scale.Scale16km,		"16km",		false,	false,	false,	true)			// 24
		};

		private int _scaleIndex;
		public int ScaleIndex { get { return _scaleIndex; } }

		protected bool calcScale(int scaleIndexHint)	// scaleIndex 0-24 from m_scales[], or -1 for automatic
		{
			double elev  = m_cameraManager.Location.Elev;

			Scale tileScale;
			if(!calcScaleIndex(m_curTheme, elev, scaleIndexHint, out _scaleIndex, out tileScale))
			{
				return false;
			}
			m_tileScale = tileScale;
			m_tileScaleName = m_scales[_scaleIndex].scaleName;
			m_metersPerPixelTileExpected = (1 << _scaleIndex) / 1000;

			return true;
		}

		public static bool calcScaleIndex(Theme theme, double elev, int scaleIndexHint, out int scaleIndex, out Scale tileScale)	// scaleIndex 0-24 from m_scales[], or -1 for automatic
		{
			scaleIndex = scaleIndexHint;
			tileScale = 0;

			// each tile scale has it's threshold, after which the next tile scale is used.
			// the idea is to use the tile while it maps closely to 1:1 in pixels, and when 
			// the mapping requires too much magnification (i.e. tile 200x200 --> 300x300 on the picture),
			// then use the next level

			// here are the real experimental numbers:
			//  64      -- from 43.6 to 87   km
			//  32      -- from 21.9 to 43.5 km
			//  16      -- from 11.0 to 21.8 km
			//   8      -- from 5.46 to 10.9 km
			//   4      -- from 2.75 to 5.45 km
			//   2      -- from 1.36 to 2.70 km
			//   1      -- from 0.25 to 1.35 km

			if(scaleIndexHint == -1)
			{
				// figure out optimal scale for this camera height:

				double cameraHeight = elev;	// m

				// 1 km height translates to 1km X 1km coverage.
				// At 1000 pixels on screen the closest scale is 1m
 
				Int64 iScale = (Int64)(cameraHeight * 0.75d);		// mm

				scaleIndex = 0;
				while(iScale != 0 && scaleIndex < m_scales.Length - 1)
				{
					iScale = iScale >> 1;
					scaleIndex++;
				}
			}

			bool supported = false;
			switch(theme)
			{
				case Theme.Photo:
					// see http://terraserver.homeadvisor.msn.com/addressimage.aspx?t=4&s=8&Lon=-122.19993897&Lat=47.86114017&Alon=-122.19993897&Alat=47.86114017&w=2&opt=0&ref=A%7c2403+150th+Ct+SE%2c+Bothell%2c+WA+98012
					if(scaleIndex < 10)		// color actually goes to s=8, but I don't know how to get a tile via http with s < 10		
					{
						scaleIndex = 10;
						supported = true;
					}
					else
					{
						supported = m_scales[scaleIndex].supportedPhoto;
					}
					break;
				case Theme.Topo:
					if(scaleIndex < 11)
					{
						scaleIndex = 11;
						supported = true;
					}
					else
					{
						supported = m_scales[scaleIndex].supportedTopo;
					}
					break;
				case Theme.Relief:
					supported = m_scales[scaleIndex].supportedRelief;
					break;
			}

			if(supported)
			{
				tileScale = m_scales[scaleIndex].scale;
			}
			return supported;
		}

		private string scaleName(Scale scale)
		{
			string ret = "";
			foreach(ScaleDescr sd in m_scales)
			{
				if(sd.scale == scale)
				{
					ret = sd.scaleName;
					break;
				}
			}
			return ret;
		}

		protected Theme m_curTheme = Theme.Photo;
		protected bool m_curThemeColor = false;

		private int getThemeCode()
		{
			int themeCode = (m_curThemeColor && m_curTheme == Theme.Photo) ? (int)Theme.Urban : ((int)m_curTheme);

			return themeCode;
		}

		protected Scale m_tileScale;
		protected string m_tileScaleName;
		protected int m_metersPerPixelTileExpected = 0;
		protected LandmarkPoint[] lps = null;
		protected static string m_formText = Project.PROGRAM_NAME_HUMAN;
		protected static string m_placeDescription = "";
		public static string PlaceDescription { get { return m_placeDescription; } }
		protected int m_imageWidth = 0;
		protected int m_imageHeight = 0;

		#region ReTile

		//private GeoCoord m_topLeftTileCorner = null;
		//private GeoCoord m_bottomRightTileCorner = null;

		private void ReTile(int scaleIndex)	// scaleIndex 0-24 from m_scales[], or -1 for automatic
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: TileSetTerra.ReTile(" + scaleIndex + ")  drawableReady=" + Project.drawableReady);
#endif
			hasRenderedTiles = false;
			m_topLeftTile = null;
			abbMessage = "offline";
			if(!Project.drawableReady)	// when starting, we get several resize events with different sizes, as 
				// layout settles. We want to ignore these and retile after the first paint.
			{
				return;
			}

			if(isPassiveSecond)
			{
				return;
			}

			lps = null;
			m_placeDescription = "";
			Project.terraserverDisconnected = false;
			Project.terraserverAvailable = false;
			m_cameraManager.terraTopLeft = new GeoCoord(0.0d, 0.0d);		// make sure we don't have stale values there for EarthquakesDisplayed
			m_cameraManager.terraBottomRight = new GeoCoord(0.0d, 0.0d);

			TerraserverCache.Landmarks.Clear();

			if(!Project.drawTerraserver)
			{
				m_formText = Project.PROGRAM_NAME_HUMAN;
				return;
			}

			if(Project.terraserverUseServices)
			{
				// try to create ts and ls and to make connection, if they are missing:
				TerraserverCache.initTerraService();
				if(TerraserverCache.ts == null)
				{
					// no connection, can't query terraserver data. Use disconnected mode.
					m_formText = Project.PROGRAM_NAME_HUMAN;
					Project.terraserverDisconnected = true;
				}
			}

			// what is the picture area in degrees?
			// use CameraManager.m_coverageXDegrees, m_coverageYDegrees for span in degrees

			if(m_isSecond)
			{
				m_curTheme = Theme.Topo;
				m_curThemeColor = false;
			}
			else
			{
				switch(Project.drawTerraserverMode)
				{
					default:
					case "aerial":
						m_curTheme = Theme.Photo;
						m_curThemeColor = false;
						break;
					case "color aerial":
						m_curTheme = Theme.Photo;
						m_curThemeColor = true;
						break;
					case "topo":
						m_curTheme = Theme.Topo;
						m_curThemeColor = false;
						break;
					case "relief":
						m_curTheme = Theme.Relief;
						m_curThemeColor = false;
						break;
				}
			}

			// what is appropriate tile resolution?
			if(!calcScale(scaleIndex)) 
			{
				// sorry, no sense in aerial/topo at small scale
				if(Project.serverAvailable)
				{
					m_formText = Project.PROGRAM_NAME_HUMAN + " - [" + Project.drawTerraserverMode + " map not available at this scale]";
				}
				else
				{
					m_formText = Project.PROGRAM_NAME_HUMAN + " - [" + abbMessage + "]";
				}
				cleanTiles();
				return;
			}

			// account for scaling of 200x200 pixel tiles into actual tile frame:
			m_imageWidth = (int)Math.Round(m_pictureManager.Width * m_cameraManager.MetersPerPixelX() * 1.1d / m_metersPerPixelTileExpected);
			m_imageHeight = (int)Math.Round(m_pictureManager.Height * m_cameraManager.MetersPerPixelY() * 1.1d / m_metersPerPixelTileExpected);

			if(Project.terraserverUseServices && Project.serverAvailable && scaleIndex == -1)
			{
				m_formText = Project.PROGRAM_NAME_HUMAN + " - reaching terraserver...";
				setMainFormText();
				queryTerraserver();		// will set Project.terraserverDisconnected on failure

				if(Project.terraserverDisconnected)
				{
					queryDisconnected();
				}
			}
			else
			{
				if(Project.serverAvailable)
				{
					abbMessage = "no ws";
				}
				queryDisconnected();
			}
				
#if DEBUG
			//LibSys.StatusBar.Trace("TileSetTerra:ReTile() ----------------- after " + TileCache.ToString());
#endif
			TerraserverCache.purge();

			logCamtrackFrame();

			if(m_needsRefresh)
			{
				m_needsRefresh = false;
				m_pictureManager.Refresh();
			}
		}

		#endregion

		#region queryDisconnected

		// we want the coverage for the first superframe to be the same as the regular frame on top of it
		private bool firstSuperframe = false;
		private int xStart = 0;
		private int yStart = 0;
		private int xEnd = 0;
		private int yEnd = 0;
		private int m_lastFactor = 0;

		protected void queryDisconnected()
		{
			GeoCoord covTL = m_cameraManager.CoverageTopLeft;
			GeoCoord covBR = m_cameraManager.CoverageBottomRight;

			double marginY = m_retilingSpecial ? 0.0d : ((covBR.Lng - covTL.Lng) / 40.0d);
			double marginX =  m_retilingSpecial ? 0.0d : ((covTL.Lat - covBR.Lat) / 40.0d * m_cameraManager.xScaleFactor);

			double covTL_Lng = covTL.Lng - marginX;
			double covTL_Lat = covTL.Lat + marginY;
			double covBR_Lng = covBR.Lng + marginX;
			double covBR_Lat = covBR.Lat - marginY;

			int currentZone = getZone(m_cameraManager.Location);

			LonLatPt lonlat = new LonLatPt();
			lonlat.Lat = covTL_Lat;
			lonlat.Lon = covTL_Lng;

			UtmPt utmptTL = Projection.LonLatPtToUtmNad83Pt(lonlat);		// can be in other than currentZone

			lonlat.Lat = covBR_Lat;
			lonlat.Lon = covBR_Lng;

			UtmPt utmptBR = Projection.LonLatPtToUtmNad83Pt(lonlat);		// can be in other than currentZone

			if(utmptTL.Zone != currentZone)
			{
				lonlat.Lat = m_cameraManager.Location.Lat;
				lonlat.Lon = m_cameraManager.Location.Lng;

				UtmPt utmptCam = Projection.LonLatPtToUtmNad83Pt(lonlat);	// can be in other than currentZone

				double dX = utmptBR.X - utmptCam.X;
				utmptTL.X = utmptCam.X - dX;
				utmptTL.Zone = currentZone;
			}
			else if(utmptBR.Zone != currentZone)
			{
				lonlat.Lat = m_cameraManager.Location.Lat;
				lonlat.Lon = m_cameraManager.Location.Lng;

				UtmPt utmptCam = Projection.LonLatPtToUtmNad83Pt(lonlat);	// can be in other than currentZone

				double dX = utmptCam.X - utmptTL.X;
				utmptBR.X = utmptCam.X + dX;
				utmptBR.Zone = currentZone;
			}

			int iScale = (int)m_tileScale;
			int metersPerPixel = (1 << ((int) iScale - 10));
			int factor = 200 * metersPerPixel;

			if(!firstSuperframe || xEnd - xStart == 0 || m_lastFactor == 0)
			{
				xStart =  (int)Math.Floor(utmptTL.X / factor);
				yStart =  (int)Math.Ceiling(utmptTL.Y / factor);

				xEnd =  (int)Math.Ceiling(utmptBR.X / factor);
				yEnd =  (int)Math.Floor(utmptBR.Y / factor);
				
				m_lastFactor = factor;
			}
			else
			{
				xStart =  xStart * m_lastFactor / factor;
				yStart =  yStart * m_lastFactor / factor;

				xEnd =  xEnd * m_lastFactor / factor;
				yEnd =  yEnd * m_lastFactor / factor;

				firstSuperframe = false;
			}

			int numTilesX = xEnd - xStart;
			int numTilesY = yStart - yEnd;

			// we need to remember UTM coordinates for Projection operation:
			m_scale = m_tileScale;
			m_metersPerPixel = (1 << ((Int32) m_scale - 10));
			screenUtmZone	 = currentZone;
			screenUtmX		 = xStart * factor;
			screenUtmY		 = (yStart - 1) * factor;

			cleanTiles();			// dispose of previous tile array, if any

			m_hCount = numTilesX;
			m_vCount = numTilesY;

			UtmPt utmpt = new UtmPt();
			utmpt.X = xStart * factor;
			utmpt.Y = yStart * factor;
			utmpt.Zone = currentZone;

			lonlat = Projection.UtmNad83PtToLonLatPt(utmpt);

			double topLeftLat = lonlat.Lat;
			double topLeftLng = lonlat.Lon;

			utmpt.X = (xStart + numTilesX) * factor;
			utmpt.Y = (yStart - numTilesY) * factor;

			lonlat = Projection.UtmNad83PtToLonLatPt(utmpt);

			double bottomRightLat = lonlat.Lat;
			double bottomRightLng = lonlat.Lon;

			m_tileResolutionDegreesH = Math.Abs(bottomRightLng - topLeftLng) / m_hCount;
			m_tileResolutionDegreesV = Math.Abs(bottomRightLat - topLeftLat) / m_vCount;
#if DEBUG
			LibSys.StatusBar.Trace("m_tileResolutionDegreesH=" + m_tileResolutionDegreesH + " m_tileResolutionDegreesV=" + m_tileResolutionDegreesV);
#endif

			int themeCode = getThemeCode();

			lock(tilesLock)
			{
				m_tiles = new TileTerra[m_vCount, m_hCount];

				SnapList snapLat = new SnapList(m_tileResolutionDegreesV, true);
				SnapList snapLng = new SnapList(m_tileResolutionDegreesH, true);

				int xx = (int)xStart;

				for(int hhh=0; hhh < m_hCount ;hhh++) 
				{
					int yy = (int)yStart;

					for(int vvv=0; vvv < m_vCount ;vvv++) 
					{
						utmpt.X = xx * factor;
						utmpt.Y = (yy) * factor;
						utmpt.Zone = currentZone;

						lonlat = Projection.UtmNad83PtToLonLatPt(utmpt);

						double tllat = lonlat.Lat;
						double tllng = lonlat.Lon;

						utmpt.X = (xx + 1) * factor;
						utmpt.Y = (yy - 1) * factor;

						lonlat = Projection.UtmNad83PtToLonLatPt(utmpt);

						double brlat = lonlat.Lat;
						double brlng = lonlat.Lon;

						GeoCoord tileTopLeft = new GeoCoord(tllng, tllat);
						GeoCoord tileBottomRight = new GeoCoord(brlng, brlat);

						String baseName = String.Format("T{0}-S{1}-Z{2}-X{3}-Y{4}", themeCode, (Int32)m_tileScale, currentZone, xx, (yy - 1));
						//String baseName = "T" + themeCode + "-S" + (Int32)m_tileScale + "-Z" + currentZone + "-X" + xx + "-Y" + (yy - 1);

						if(!m_retilingSpecial)
						{
							// adding to snap lists prepares them for calculating snap points:
							snapLat.Add(tllat);
							snapLat.Add(brlat);
							snapLng.Add(tllng);
							snapLng.Add(brlng);
						}

						m_tiles[vvv, hhh] = new TileTerra(this, m_tileScale, tileTopLeft, tileBottomRight);
						m_tiles[vvv, hhh].baseName = baseName;
						m_tiles[vvv, hhh].init();		// gets backdrop, or starts the process of loading backdrop

#if DEBUG
						//LibSys.StatusBar.Trace("[" + vvv + "," + hhh + "]  " + baseName);
						//LibSys.StatusBar.Trace("        -- topLeft=" + tileTopLeft + " bottomRight=" + tileBottomRight);
#endif
						yy--;
					}
					xx++;
				}

				// snap to the grid. If special retiling, distortion is ok:
				for(int vvv=0; !m_retilingSpecial && vvv < m_vCount ;vvv++) 
				{
					for(int hhh=0; hhh < m_hCount ;hhh++) 
					{
						TileTerra tile = m_tiles[vvv, hhh];
						// snap the tile's corners to grid:
						tile.getBottomRight().X = snapLng.snap(tile.getBottomRight().X);
						tile.getBottomRight().Y = snapLat.snap(tile.getBottomRight().Y);
						tile.getTopLeft().X = snapLng.snap(tile.getTopLeft().X);
						tile.getTopLeft().Y = snapLat.snap(tile.getTopLeft().Y);
					}
				}

				// we need topleft tile for Projection:
				m_topLeftTile = m_tiles[0, 0];

				calcRatios();

				// compute resolution to display, meters per pixel, with overzoom indicator:
				double ratio = (m_ratioX + m_ratioY) / 2.0d;
				string overzoom = ("1m".Equals(m_tileScaleName) && ratio > 1.1d) ? " (overzoom)" : "";

				hasRenderedTiles = true;

				m_placeDescription = "[" + abbMessage + "]";
				m_formText = Project.PROGRAM_NAME_HUMAN + " - " + m_placeDescription + "   " + m_tileScaleName + "/pixel * " + ratio + overzoom; // + " (" + m_tileScale + ")";

			}	// end lock

			#region old code relying on Tiles DB in TerraserverCache
			/*
			int iTheme = (int)m_curTheme;
			string where = "theme = " + iTheme + " AND scale = " + iScale + " AND name LIKE '%-Z" + currentZone + "-%'"
				+ " AND brlng > " + covTL_Lng + " AND tllng <" + covBR_Lng
				+ " AND brlat < " + covTL_Lat + " AND tllat > " + covBR_Lat;

#if DEBUG
			LibSys.StatusBar.Trace("WHERE " + where);
#endif

			// sort by name gives us rows sorted from bottom left up to top left (per line scan) all the way to top right -
			// like a TV scanning, turned on the left side. The Y coord axis goes up, the X - to the right. 
			string sort = "name";

			DataRow[] rows = TerraserverCache.TilesDS.Tables[0].Select(where, sort);

			// not all tiles may be present in the grid. Restore the grid structure:

			if(rows != null && rows.Length > 0)
			{
				// use the first tile for grid evaluation:
				int tileScale = (int)rows[0]["scale"];
				double tllng = (double)rows[0]["tllng"];
				double tllat = (double)rows[0]["tllat"];
				double brlng = (double)rows[0]["brlng"];
				double brlat = (double)rows[0]["brlat"];

				m_tileResolutionDegreesH = Math.Abs(brlng - tllng);
				m_tileResolutionDegreesV = Math.Abs(brlat - tllat);

#if DEBUG
				LibSys.StatusBar.Trace("tl=" + tllng + "," + tllat + " br=" + brlng + "," + brlat);
				LibSys.StatusBar.Trace("m_tileResolutionDegreesH=" + m_tileResolutionDegreesH + " m_tileResolutionDegreesV=" + m_tileResolutionDegreesV);
				LibSys.StatusBar.Trace("tile 0 -- topLeft=" + (new GeoCoord(tllng,tllat)) + " bottomRight=" + (new GeoCoord(brlng,brlat)));
#endif
				// evaluate dimensions of tile matrix and corners of the tile-covered frame:
				double d;
				int hCount = 0;
				for (d=tllng; d > covTL.Lng ;d -= m_tileResolutionDegreesH)
				{
					//LibSys.StatusBar.Trace("working left -- topLeft=" + (new GeoCoord(d,tllat)));
					hCount++;
				}
				double topLeftLng = d;
				for (d=tllng; d < covBR.Lng ;d += m_tileResolutionDegreesH)
				{
					//LibSys.StatusBar.Trace("working right -- topLeft=" + (new GeoCoord(d,tllat)));
					hCount++;
				}
				double bottomRightLng = d;

				int vCount = 0;
				for (d=tllat; d < covTL.Lat ;d += m_tileResolutionDegreesV)
				{
					//LibSys.StatusBar.Trace("working up -- topLeft=" + (new GeoCoord(tllng, d)));
					vCount++;
				}
				double topLeftLat = d;
				for (d=tllat; d > covBR.Lat ;d -= m_tileResolutionDegreesV)
				{
					//LibSys.StatusBar.Trace("working down -- topLeft=" + (new GeoCoord(tllng, d)));
					vCount++;
				}
				double bottomRightLat = d;

#if DEBUG
				LibSys.StatusBar.Trace("hCount=" + hCount + " vCount=" + vCount);
				LibSys.StatusBar.Trace("topLeft=" + topLeftLng + "," + topLeftLat + " bottomRight=" + bottomRightLng + "," + bottomRightLat);
				LibSys.StatusBar.Trace("topLeft=" + (new GeoCoord(topLeftLng,topLeftLat)) + " bottomRight=" + (new GeoCoord(bottomRightLng,bottomRightLat)));
#endif

				cleanTiles();			// dispose of previous tile array, if any

				m_vCount = vCount+2;	// make more cells for inperfections in tile corners.
				m_hCount = hCount+2;	// extra tiles will be initialized as "empty". The order of the tiles is random.

				lock(tilesLock)
				{
					m_tiles = new TileTerra[m_vCount, m_hCount];

					SnapList snapLat = new SnapList(m_tileResolutionDegreesV, true);
					SnapList snapLng = new SnapList(m_tileResolutionDegreesH, true);

					int vv = 0;
					int hh = 0;

					int rowCount = 0;
					foreach(DataRow row in rows)
					{
						string basename = (string)row["name"];
						tileScale = (int)row["scale"];
						tllng = (double)row["tllng"];
						tllat = (double)row["tllat"];
						brlng = (double)row["brlng"];
						brlat = (double)row["brlat"];

						// adding to snap lists prepares them for calculating snap points:
						snapLat.Add(tllat);
						snapLat.Add(brlat);
						snapLng.Add(tllng);
						snapLng.Add(brlng);

						// find the right position for the tile:
						// (turns out that order of tiles does not matter)

						// place the tile:
						GeoCoord tileTopLeft = new GeoCoord(tllng, tllat, 0.0d);
						GeoCoord tileBottomRight = new GeoCoord(brlng, brlat, 0.0d);
						m_tiles[vv, hh] = new TileTerra(this, (Scale)tileScale, tileTopLeft, tileBottomRight);
#if DEBUG
						LibSys.StatusBar.Trace("    ----- tile: " + basename + " --- tl=" + tllng + "," + tllat + " br=" + brlng + "," + brlat);
#endif
						m_tiles[vv, hh].baseName = basename;
						m_tiles[vv, hh].init();		// gets backdrop, or starts the process of loading backdrop

						if(++rowCount >= m_hCount * m_vCount)
						{
							// no matter what, we don't want to overflow the array and get an exception. Shouldn't happen though.
							LibSys.StatusBar.Error("too many tiles in rowset");
							break;
						}

						hh++;
						if(hh >= m_hCount)
						{
							hh = 0;
							vv++;
						}

						// adjust frame corners to eliminate rounding errors, if possible:
						topLeftLng = Math.Min(topLeftLng, tllng);
						topLeftLat = Math.Max(topLeftLat, tllat);
						bottomRightLng = Math.Max(bottomRightLng, brlng);
						bottomRightLat = Math.Min(bottomRightLat, brlat);
						//break;
					}
					m_cameraManager.terraTopLeft = new GeoCoord(topLeftLng, topLeftLat);
					m_cameraManager.terraBottomRight = new GeoCoord(bottomRightLng, bottomRightLat);

					// make sure there are no null tiles in the array:
					tllng = topLeftLng;
					tllat = topLeftLat;
					brlng = tllng + m_tileResolutionDegreesH;
					brlat = tllat - m_tileResolutionDegreesV;
					for(int vvv=0; vvv < m_vCount ;vvv++) 
					{
						for(int hhh=0; hhh < m_hCount ;hhh++) 
						{
							TileTerra tile = m_tiles[vvv, hhh];
							if(tile == null)
							{
								GeoCoord tileTopLeft = new GeoCoord(snapLng.snap(tllng), snapLat.snap(tllat), 0.0d);
								GeoCoord tileBottomRight = new GeoCoord(snapLng.snap(brlng), snapLat.snap(brlat), 0.0d);
								m_tiles[vvv, hhh] = new TileTerra(this, (Scale)tileScale, tileTopLeft, tileBottomRight);
								m_tiles[vvv, hhh].baseName = "empty";
								tile = m_tiles[vvv, hhh];
								//m_tiles[vvv, hhh].init();		// gets backdrop, or starts the process of loading backdrop
							}
							else
							{
								// snap the tile's corners to grid:
								tile.getBottomRight().X = snapLng.snap(tile.getBottomRight().X);
								tile.getBottomRight().Y = snapLat.snap(tile.getBottomRight().Y);
								tile.getTopLeft().X = snapLng.snap(tile.getTopLeft().X);
								tile.getTopLeft().Y = snapLat.snap(tile.getTopLeft().Y);
							}
							// we need topleft tile for Projection:
							if(m_topLeftTile == null || tile.getTopLeft().X <= m_topLeftTile.getTopLeft().X 
								&& tile.getTopLeft().Y >= m_topLeftTile.getTopLeft().Y)
							{
								m_topLeftTile = tile;
							}
							tllng += m_tileResolutionDegreesH;
							brlng += m_tileResolutionDegreesH;
						}
						tllat -= m_tileResolutionDegreesV;
						brlat -= m_tileResolutionDegreesV;
					}
				}	// end lock

				// compute parameters for Projection:
				Rectangle tileRect = m_topLeftTile.getFrameRectangle();
				m_ratioX = ((double)tileRect.Width - 1.5d) / 200.0d;		// -1.5 corrects added 1 in getFrameRectangle(), and probability of rounding
				m_ratioY = ((double)tileRect.Height - 1.5d) / 200.0d;
				m_offsetX = tileRect.X;
				m_offsetY = tileRect.Y;

				// compute resolution to display, meters per pixel, with overzoom indicator:
				double ratio = (m_ratioX + m_ratioY) / 2.0d;
				string overzoom = ("1m".Equals(m_tileScaleName) && ratio > 1.1d) ? " (overzoom)" : "";

				try 
				{
					// compute UTMP reference values for Projection operation:
					screenUtmZone = currentZone;
					m_scale = (Scale)m_tileScale;
					m_metersPerPixel = (1 << ((Int32) m_scale - 10));
					// we have to parse tile name here - something like T1-S16-Z17-X57-Y368
					char[]   sep = new Char[1] { '-' };
					string[] split = m_topLeftTile.baseName.Split(sep);

					int utmX = Convert.ToInt32(split[3].Substring(1));
					int utmY = Convert.ToInt32(split[4].Substring(1));
					screenUtmX = (utmX * 200 * m_metersPerPixel);
					screenUtmY = (utmY * 200 * m_metersPerPixel);

					hasRenderedTiles = true;
				} 
				catch {}

				m_placeDescription = "[" + abbMessage + "]";
				m_formText = Project.PROGRAM_NAME_HUMAN + " - " + m_placeDescription + "   " + m_tileScaleName + "/pixel * " + ratio + overzoom; // + " (" + m_tileScale + ")";
			}
			else
			{
				// there are no tiles for this scale, but if we zoomed in the existing tiles may work. Make sure distortion is taken care of. 
				try 
				{
					// find which tile is now top-left (screen corner contained in this tile):
					Rectangle tileRect = new Rectangle(0, 0, 0, 0);

					for(int vvv=0; vvv < m_vCount ;vvv++) 
					{
						for(int hhh=0; hhh < m_hCount ;hhh++) 
						{
							TileTerra tile = m_tiles[vvv, hhh];
							tileRect = tile.getFrameRectangle();
							if(tileRect.Contains(0, 0))
							{
								m_topLeftTile = tile;
								goto found;
							}
						}
					}
					goto notFound;

				found:
					// compute parameters for Projection:
					m_ratioX = ((double)tileRect.Width - 1.5d) / 200.0d;		// -1.5 corrects added 1 in getFrameRectangle(), and probability of rounding
					m_ratioY = ((double)tileRect.Height - 1.5d) / 200.0d;
					m_offsetX = tileRect.X;
					m_offsetY = tileRect.Y;

					// compute resolution to display, meters per pixel, with overzoom indicator:
					double ratio = (m_ratioX + m_ratioY) / 2.0d;
					string overzoom = " (overzoom)";

					// compute UTMP reference values for Projection operation:
					screenUtmZone = currentZone;
					m_metersPerPixel = (1 << ((Int32) m_scale - 10));
					// we have to parse tile name here - something like T1-S16-Z17-X57-Y368
					char[]   sep = new Char[1] { '-' };
					string[] split = m_topLeftTile.baseName.Split(sep);

					m_tileScale = (Scale)Convert.ToInt32(split[1].Substring(1));
					m_scale = (Scale)m_tileScale;
					m_tileScaleName = scaleName(m_scale);
					int utmX = Convert.ToInt32(split[3].Substring(1));
					int utmY = Convert.ToInt32(split[4].Substring(1));
					screenUtmX = (utmX * 200 * m_metersPerPixel);
					screenUtmY = (utmY * 200 * m_metersPerPixel);

					hasRenderedTiles = true;

					m_placeDescription = "[" + abbMessage + "]";
					m_formText = Project.PROGRAM_NAME_HUMAN + " - " + m_placeDescription + "   " + m_tileScaleName + "/pixel * " + ratio + overzoom; // + " (" + m_tileScale + ")";

				notFound:
					;
				} 
				catch {}
			}
			*/
			#endregion

			// Landmarks array was cleaned in the beginning of ReTile().
			// now fill the landmarks from the database:
			string where = "lng > " + covTL_Lng + " AND lng <" + covBR_Lng
				+ " AND lat < " + covTL_Lat + " AND lat > " + covBR_Lat;

			DataRow[] rows = TerraserverCache.LandmarksDS.Tables["lm"].Select(where);

			if(rows != null && rows.Length > 0)
			{
				int numLandmarks = rows.Length;
				if(numLandmarks > 0)
				{
					foreach(DataRow row in rows)
					{
						string name = (string)row["name"];
						string type = (string)row["type"];
						double lat =  (double)row["lat"];
						double lng =  (double)row["lng"];
						Landmark lm = new Landmark(name, new GeoCoord(lng, lat, 0.0d), type);
						TerraserverCache.AddLandmark(lm);
					}
				}
			}
		}
		#endregion

		private void warningCantReachServer(string reason)
		{
			if(TileCache.ZipcodeServer != null)	// we are or were online with QuakeMap.com server. If not, user knows he works offline. 
			{
				if(msgBoxCount-- > 0)
				{
					string message = "\nUnable to retrieve location from TerraServer Web Service\n" + reason + "\n\nWill try compute tiles layout locally.\n "; 
					//Point popupOffset = new Point(100, 380);
					//Point screenPoint = Project.mainForm.PointToScreen(popupOffset);
					//Project.ShowPopup (Project.mainForm, message, screenPoint);
					MessageBox.Show(message, "TerraService Not Available", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					m_needsRefresh = true;
				}
			}
		}

		// goes to terraserver for UTM to LonLat conversion -- not any more:
		private LonLatPt getTopLeftByTileId(TileId tid)
		{
			// from http://terraserver.microsoft.net/about.aspx?n=AboutTerraServiceOverview (select Data Structures link):
			// Most TerraServer image tiles are in the UTM NAD 83 projection system. 
			// The Scale, Scene, X, and Y fields can be used to compute the UTM NAD 83 coordinates
			// for the lower left hand pixel in the TerraServer tile (XOffset=0 and YOffset=200).
			// The following formulas are required to compute the UTM NAD 83 for the lower left hand pixel:
			//		Int32 UtmZone = Scene;
			//		Int32 metersPerPixel = (1 << ((Int32) Scale - 10));
			//		Int32 UtmEasting = X * 200 * metersPerPixel;
			//		Int32 UtmNorthing = Y * 200 * metersPerPixel;

			UtmPt utm = new UtmPt();
			utm.Zone = tid.Scene;
			int metersPerPixel = (1 << ((int) tid.Scale - 10));
			// top left:
			utm.X = tid.X * 200 * metersPerPixel;
			utm.Y = (tid.Y + 1) * 200 * metersPerPixel - 1;
			LonLatPt llpt = Projection.UtmNad83PtToLonLatPt(utm); //TerraserverCache.ts.ConvertUtmPtToLonLatPt(utm);
			return llpt;
		}

		// goes to terraserver for UTM to LonLat conversion -- not any more:
		private LonLatPt getBottomRightByTileId(TileId tid)
		{
			UtmPt utm = new UtmPt();
			utm.Zone = tid.Scene;
			int metersPerPixel = (1 << ((int) tid.Scale - 10));
			// lower right:
			utm.X = (tid.X + 1) * 200 * metersPerPixel - 1;
			utm.Y = tid.Y * 200 * metersPerPixel;
			LonLatPt llpt = Projection.UtmNad83PtToLonLatPt(utm); //TerraserverCache.ts.ConvertUtmPtToLonLatPt(utm);
			return llpt;
		}

		private static int msgBoxCount = 1;
		private string abbMessage = "offline";
		private static DateTime date1970 = new DateTime(1970, 1, 1, 1, 1, 1);

		private AreaBoundingBox abb = null;		// if not null, it is sitting around waiting for reuse
		private Theme m_savedCurTheme;
		protected bool m_savedCurThemeColor;
		private Scale m_savedTileScale;
		private int m_savedImageWidth;
		private int m_savedImageHeight;
		private double m_savedCenterPointLat;
		private double m_savedCenterPointLon;

		#region queryTerraserver

		protected void queryTerraserver()
		{
			LonLatPt centerPoint = new LonLatPt();
			centerPoint.Lat = m_cameraManager.Location.Lat;
			centerPoint.Lon = m_cameraManager.Location.Lng;

			bool isValidAbb = false;
			int retriesMax = 2;

#if DEBUG
			LibSys.StatusBar.Trace("IP: TileSetTerra.queryTerraserver() Fetching Area Bounding Box from TerraServer");
#endif
			bool canReuseAbb = false;
			try 
			{
				if(abb != null && m_savedCurTheme == m_curTheme && m_savedCurThemeColor == m_curThemeColor && m_savedTileScale == m_tileScale
					&& m_savedImageWidth == m_imageWidth && m_savedImageHeight == m_imageHeight
					&& m_savedCenterPointLat == centerPoint.Lat && m_savedCenterPointLon == centerPoint.Lon)
				{
					canReuseAbb = true;
					isValidAbb = true;
#if DEBUG
					LibSys.StatusBar.Trace("IP: TileSetTerra.queryTerraserver() reusing Area Bounding Box");
#endif
				}
			}
			catch {}

			int retries = 0;
			while (!canReuseAbb && retries <= retriesMax && Project.serverAvailable)
			{
				try
				{
					abb = TerraserverCache.ts.GetAreaFromPt(centerPoint, (int)m_curTheme, m_tileScale, m_imageWidth, m_imageHeight);
#if DEBUG
					LibSys.StatusBar.Trace("IP: TileSetTerra.queryTerraserver() Got the Area Bounding Box from TerraServer");
#endif
					// make a real quick sanity check of acquired abb here:
					isValidAbb = abb != null;
					if(isValidAbb)
					{
						TileMeta tmm = abb.Center.TileMeta;
						if(tmm.Capture.CompareTo(date1970) < 0)
						{
							m_placeDescription = abb.NearestPlace;
							abbMessage = (m_placeDescription == null ? "" : (m_placeDescription + " - ")) + "this area not covered by Terraserver";
							isValidAbb = false;
							break;
						}
					}

					// react if abb is invalid:
					if(isValidAbb)
					{
						abbMessage = "terraserver reached";
						break;
					}
					else if (retries++ < retriesMax)
					{
#if DEBUG
						LibSys.StatusBar.Error("TileSetTerra.queryTerraserver() while getting abb, retry # " + retries + " abb=null");
#endif
						continue;
					}
					else
					{
						abbMessage = "terraserver not reached";
						warningCantReachServer("");
						break;
					}
				}
				catch (Exception ie)
				{
					if (retries++ < retriesMax)
					{
#if DEBUG
						LibSys.StatusBar.Error("TileSetTerra.queryTerraserver() while getting abb, retry # " + retries + " " + ie.Message);
#endif
						// compensate for currently present bug around Seattle:
						if(ie.Message.IndexOf("There is an error in XML document") >= 0)
						{
							abbMessage = "terraserver ws failed";
							break;
						}
						continue;
					}
					else
					{
						abbMessage = "terraserver not reached";
						warningCantReachServer(ie.Message);
						break;
					}
				}
			}

			if(!isValidAbb)
			{
				LibSys.StatusBar.Error("TileSetTerra.queryTerraserver() couldn't get abb - working offline [" + abbMessage + "]");
				Project.terraserverDisconnected = true;
				cleanTiles();
				m_formText = Project.PROGRAM_NAME_HUMAN + " - [" + Project.drawTerraserverMode + " map not available - " + abbMessage + "]";
				abb = null;
				return;
			}

			m_savedCurTheme = m_curTheme;
			m_savedCurThemeColor = m_curThemeColor;
			m_savedTileScale = m_tileScale;
			m_savedImageWidth = m_imageWidth;
			m_savedImageHeight = m_imageHeight;
			m_savedCenterPointLat = centerPoint.Lat;
			m_savedCenterPointLon = centerPoint.Lon;

			// calculate abb corners geo coordinates, keeping in mind that not all corner tiles exist (actually all four may be missing):
			double topLeftLng = 0.0d;
			double topLeftLat = 0.0d;
			double bottomRightLng = 0.0d;
			double bottomRightLat = 0.0d;

			if(abb.NorthWest.TileMeta.TileExists)
			{
				topLeftLng = abb.NorthWest.TileMeta.NorthWest.Lon;
				topLeftLat = abb.NorthWest.TileMeta.NorthWest.Lat;
			}
			else
			{
				if(abb.SouthWest.TileMeta.TileExists)
				{
					topLeftLng = abb.SouthWest.TileMeta.NorthWest.Lon;
					if(abb.NorthEast.TileMeta.TileExists)
					{
						topLeftLat = abb.NorthEast.TileMeta.NorthEast.Lat;
					}
					else
					{
						// both top corners are missing, go get the top lat by Utm to LonLat conversion:
						LonLatPt llpt = getTopLeftByTileId(abb.NorthWest.TileMeta.Id);
						topLeftLat = llpt.Lat;
					}
				}
				else
				{
					// both left side corners are missing, go get the top left corner by Utm to LonLat conversion: 
					LonLatPt llpt = getTopLeftByTileId(abb.NorthWest.TileMeta.Id);
					topLeftLng = llpt.Lon;
					topLeftLat = llpt.Lat;
				}
			}

			if(abb.SouthEast.TileMeta.TileExists)
			{
				bottomRightLng = abb.SouthEast.TileMeta.SouthEast.Lon;
				bottomRightLat = abb.SouthEast.TileMeta.SouthEast.Lat;
			}
			else
			{
				if(abb.NorthEast.TileMeta.TileExists)
				{
					bottomRightLng = abb.NorthEast.TileMeta.SouthEast.Lon;
					if(abb.SouthWest.TileMeta.TileExists)
					{
						bottomRightLat = abb.SouthWest.TileMeta.SouthWest.Lat;
					}
					else
					{
						// both bottom corners are missing, go get the bottom lat by Utm to LonLat conversion:
						LonLatPt llpt = getBottomRightByTileId(abb.SouthEast.TileMeta.Id);
						bottomRightLat = llpt.Lat;
					}
				}
				else
				{
					// both right side corners are missing, go get the bottom right corner by Utm to LonLat conversion: 
					LonLatPt llpt = getBottomRightByTileId(abb.SouthEast.TileMeta.Id);
					bottomRightLng = llpt.Lon;
					bottomRightLat = llpt.Lat;
				}
			}

			// more sanity check - in unlikely case we were unable to figure out abb corners:
			if(topLeftLng == 0.0d || topLeftLat == 0.0d || bottomRightLng == 0.0d || bottomRightLat == 0.0d)
			{
				abbMessage = "response from terraserver cannot be mapped";
				LibSys.StatusBar.Error("TileSetTerra.queryTerraserver() couldn't get abb - working offline [" + abbMessage + "]");
				Project.terraserverDisconnected = true;
				cleanTiles();
				m_formText = Project.PROGRAM_NAME_HUMAN + " - [" + Project.drawTerraserverMode + " map not available - " + abbMessage + "]";
				return;
			}

			m_placeDescription = abb.NearestPlace;
			TileMeta tm = abb.Center.TileMeta;
			/*
			if(tm.Capture.CompareTo(date1970) < 0)	
			{
				// we won't get here as date is checked before, in sanity check
				m_placeDescription += " [" + Project.drawTerraserverMode + " map]";
			}
			else
			{ */
			m_placeDescription += " [" + Project.drawTerraserverMode + " map from " + tm.Capture.ToString("D", null) + "]";
			//}

#if DEBUG
			LibSys.StatusBar.Trace("IP: retrieved info for " + m_placeDescription);
#endif

			m_scale = abb.Center.TileMeta.Id.Scale;
			m_metersPerPixel = (1 << ((Int32) m_scale - 10));

			int factor = 200 * m_metersPerPixel;

			m_lastFactor = factor;

			// even if a corner tile does not exist, the Id is filled with valid information:
			xStart = abb.NorthWest.TileMeta.Id.X;
			int yyStart = abb.NorthWest.TileMeta.Id.Y;

			// actually superframe is computed in offline mode, so the saved xStart...yEnd values will be used in queryDisconnected()
			yStart = yyStart + 1;

			xEnd = abb.NorthEast.TileMeta.Id.X + 1;
			int yyEnd = abb.SouthWest.TileMeta.Id.Y;

			yEnd = yyEnd;
			
			// we need to remember UTMP coordinates for Projection operation:
			screenUtmZone	 = abb.Center.TileMeta.Id.Scene;
			screenUtmX		 = xStart * factor;
			screenUtmY		 = yyStart * factor;

			cleanTiles();		// dispose of previous tile array, if any

			m_hCount = xEnd - xStart;
			m_vCount = yyStart - yyEnd + 1;

#if DEBUG
			LibSys.StatusBar.Trace("TileSetTerra:queryTerraserver() m_vCount=" + m_vCount + " m_hCount=" + m_hCount);
#endif

			int themeCode = getThemeCode();

			lock(tilesLock)
			{
				m_tiles = new TileTerra[m_vCount, m_hCount];

#if DEBUG
				LibSys.StatusBar.Trace("TileSetTerra:queryTerraserver() ----------------- before " + TileCache.ToString());
#endif
				GeoCoord covTL = m_cameraManager.CoverageTopLeft;
				GeoCoord covBR = m_cameraManager.CoverageBottomRight;

				m_cameraManager.terraTopLeft = new GeoCoord(topLeftLng, topLeftLat);
				m_cameraManager.terraBottomRight = new GeoCoord(bottomRightLng, bottomRightLat);

				m_tileResolutionDegreesH = Math.Abs(bottomRightLng - topLeftLng) / m_hCount;
				m_tileResolutionDegreesV = Math.Abs(bottomRightLat - topLeftLat) / m_vCount;

#if DEBUG
				LibSys.StatusBar.Trace("abb: topleft=" + topLeftLat + "," + topLeftLng + "  bottomRight=" + bottomRightLat + "," + bottomRightLng);
				LibSys.StatusBar.Trace("   : m_tileResolutionDegreesH=" + m_tileResolutionDegreesH + "  m_tileResolutionDegreesV=" + m_tileResolutionDegreesV);
#endif
				SnapList snapLat = new SnapList(m_tileResolutionDegreesV, true);
				SnapList snapLng = new SnapList(m_tileResolutionDegreesH, true);

				int x = 0;
				int y = 0;
				int vv = 0;
				int hh = 0;

				// for corners of the current tile:
				double tileTopLeftLng = topLeftLng;
				double tileBottomRightLng = tileTopLeftLng + m_tileResolutionDegreesH;
				double tileTopLeftLat;
				double tileBottomRightLat;

				TileId tid = new TileId(); // we need to clone abb.NorthWest.TileMeta.Id and leave abb intact

				for ( x = xStart; hh < m_hCount; x++, hh++) 
				{
					vv = 0;
					tileTopLeftLat = topLeftLat;
					tileBottomRightLat = tileTopLeftLat - m_tileResolutionDegreesV;
				
					for ( y = yyStart; vv < m_vCount; y--, vv++) 
					{
						tid.X = x;
						tid.Y = y;
						tid.Scale = abb.NorthWest.TileMeta.Id.Scale;
						tid.Scene = abb.NorthWest.TileMeta.Id.Scene;
						tid.Theme = (int)m_curTheme;	// why isn't Theme already set?

						String baseName = String.Format("T{0}-S{1}-Z{2}-X{3}-Y{4}", themeCode, (Int32)tid.Scale, tid.Scene, tid.X, tid.Y);
						//String baseName = "T" + themeCode + "-S" + (Int32)tid.Scale + "-Z" + tid.Scene + "-X" + tid.X + "-Y" + tid.Y;

						// adding to snap lists prepares them for calculating snap points:
						snapLat.Add(tileTopLeftLat);
						snapLat.Add(tileBottomRightLat);
						snapLng.Add(tileTopLeftLng);
						snapLng.Add(tileBottomRightLng);

						GeoCoord tileTopLeft = new GeoCoord(tileTopLeftLng, tileTopLeftLat, 0.0d);
						GeoCoord tileBottomRight = new GeoCoord(tileBottomRightLng, tileBottomRightLat, 0.0d);

						m_tiles[vv, hh] = new TileTerra(this, m_tileScale, tileTopLeft, tileBottomRight);
						m_tiles[vv, hh].baseName = baseName;
						m_tiles[vv, hh].init();		// gets backdrop, or starts the process of loading backdrop

#if DEBUG
						//LibSys.StatusBar.Trace("[" + vv + "," + hh + "]  " + baseName);
						//LibSys.StatusBar.Trace("        -- topLeft=" + tileTopLeft + " bottomRight=" + tileBottomRight);
#endif
						// we know a lot about this tile now.
						// register the tile with local database for disconnected operation.
						// it may turn out to be a cottage cheese or non-arrival though, and the registration would stay there.
						//TerraserverCache.registerTerraTile(baseName, m_curTheme, m_tileScale, tileTopLeft, tileBottomRight);

						tileTopLeftLat -= m_tileResolutionDegreesV;
						tileBottomRightLat -= m_tileResolutionDegreesV;
					}
					tileTopLeftLng += m_tileResolutionDegreesH;
					tileBottomRightLng += m_tileResolutionDegreesH;
				}

				// snap to the grid (corrects small gaps at maximum zoom):
				for(int vvv=0; vvv < m_vCount ;vvv++) 
				{
					for(int hhh=0; hhh < m_hCount ;hhh++) 
					{
						TileTerra tile = m_tiles[vvv, hhh];
						// snap the tile's corners to grid:
						tile.getBottomRight().X = snapLng.snap(tile.getBottomRight().X);
						tile.getBottomRight().Y = snapLat.snap(tile.getBottomRight().Y);
						tile.getTopLeft().X = snapLng.snap(tile.getTopLeft().X);
						tile.getTopLeft().Y = snapLat.snap(tile.getTopLeft().Y);
					}
				}

				// we need topleft tile for Projection:
				m_topLeftTile = m_tiles[0, 0];

				Project.terraserverAvailable = true;
			} // end lock

			calcRatios();

			hasRenderedTiles = true;

			// compute visible magnification ratio:
			double ratio = (m_ratioX + m_ratioY) / 2.0d;
			string overzoom = ("1m".Equals(m_tileScaleName) && ratio > 1.1d) ? " (overzoom)" : "";

			m_formText = Project.PROGRAM_NAME_HUMAN + " - " + m_placeDescription + "   " + m_tileScaleName + "/pixel  x" + ratio + overzoom; // + " (" + m_tileScale + ")";

			if(Project.drawLandmarks)
			{
				TerraserverCache.initLandmarkService();
				if(TerraserverCache.ls != null && TerraserverCache.landmarkPointTypes != null && TerraserverCache.landmarkPointTypes.Length > 0)
				{

					// retrieve lanfdmarks information:
					BoundingRect br = new BoundingRect();

					LonLatPt tmp = abb.SouthEast.TileMeta.SouthEast;
					br.LowerRight = new LibNet.LandmarkServer.LonLatPt();
					br.LowerRight.Lon = tmp.Lon;
					br.LowerRight.Lat = tmp.Lat;

					tmp = abb.NorthWest.TileMeta.NorthWest;
					br.UpperLeft = new LibNet.LandmarkServer.LonLatPt();
					br.UpperLeft.Lon = tmp.Lon;
					br.UpperLeft.Lat = tmp.Lat;

					m_hasPutOnMap = false;

					bool isValidLps = false;
					retries = 0;

					//string[] types = new string[] { "Building", "Cemetery", "Church", "Encarta Article",
					//								"Golf Course", "Hospital", "Institution", "Landmark", "Locale",
					//								"Parks", "Populated Place", "Recreation Area", "Retail Center",
					//								"Stream Gauge", "Summit", "Transportation Terminal", "Unknown Type" };
				
					/*
					string[] types = new string[] { "Building", "Cemetery", "Church", "Encarta Article",
													  "Golf Course", "Hospital", "Landmark", "Locale",
													  "Parks", "Populated Place", "Retail Center",
													  "Stream Gauge", "Summit", "Transportation Terminal", "Unknown Type"};
					*/

					// as of May 31,03 , "Institution" and "Recreation Area" cause exception:
					//          Server was unable to process request. --> Data is Null. This method or property cannot be called on Null values.
					// see TerraserverCache:295 correcting this.

					/*
					string[] types = new string[TerraserverCache.landmarkPointTypes.Length];
					for(int i=0; i < types.Length ;i++)
					{
						types[i] = "" + TerraserverCache.landmarkPointTypes[i];
					}
					*/

					while (retries <= retriesMax)
					{
						try
						{
							lps = TerraserverCache.ls.GetLandmarkPointsByRect(br, TerraserverCache.landmarkPointTypes);		// ,types);
							// make a sanity check of acquired lps here:
							isValidLps = lps != null;
							break;
						}
						catch (Exception e)
						{
							if (retries == retriesMax)
							{
								MessageBox.Show("Unable to get landmark information" + e.Message, "TerraService Error");
								break;
							}
							else
							{
								retries++;
							}
						}
						catch
						{
							if (retries == retriesMax)
							{
								MessageBox.Show("Unable to get landmark information", "TerraService Error");
								break;
							}
							else
							{
								retries++;
							}
						}

					}

					if (isValidLps)
					{
						int lpsCount = lps.Length;
						foreach (LandmarkPoint lp in lps) 
						{
							GeoCoord location = new GeoCoord(lp.Point.Lon, lp.Point.Lat);
							string name = lp.Name;
							string type = lp.Type;
							Landmark lm = new Landmark(name, location, type);
							TerraserverCache.AddLandmark(lm);			// to ArrayList, for this immediate display
							TerraserverCache.RegisterLandmark(lm);		// add to DataSet, for disconnected operation
						}
					}
				}
			}
		}
		#endregion

		public override void PictureResized()
		{
			// we rely on CameraManager:ReactPictureResized() to call ReTile() eventually through ProcessCameraMove()
			// LibSys.StatusBar.Trace("IP: TileSetTerra::PictureResized()  drawableReady=" + Project.drawableReady);
			//ReTile();
			//setMainFormText();
		}

		private void setMainFormText()
		{
			if(!m_isSecond)
			{
				if(Project.mainForm.InvokeRequired)
				{
					Project.mainForm.Invoke(new MethodInvoker(_setMainFormText));
				} 
				else
				{
					_setMainFormText();
				}
			}
		}

		private void _setMainFormText()
		{
			m_pictureManager.MainForm.Text = m_formText;
		}

		private void logCamtrackFrame()
		{
			if(Project.camTrackOn && !m_isSecond)
			{
				CameraTrack cameraTrack = TerraserverCache.CameraManager.CameraTrack;
				XmlDocument xmlDoc = cameraTrack.XmlDoc;

				// by this time CameraManager.processCameraMove() has added new frame to the root.
				// Therefore, cameraTrack.currentFrameNode contains a pointer to that frame.

				/*
				float flng, flat;
				XmlAttribute attr = xmlDoc.CreateAttribute("topleft");
				flng = (float)m_cameraManager.terraTopLeft.Lng;
				flat = (float)m_cameraManager.terraTopLeft.Lat;
				attr.InnerText = "" + flng + "," + flat;
				cameraTrack.CurrentFrameNode.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("bottomright");
				flng = (float)m_cameraManager.terraBottomRight.Lng;
				flat = (float)m_cameraManager.terraBottomRight.Lat;
				attr.InnerText = "" + flng + "," + flat;
				cameraTrack.CurrentFrameNode.Attributes.Append(attr);
				*/

				XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, "terratiles", null);
				XmlAttribute attr = xmlDoc.CreateAttribute("width");
				attr.InnerText = "" + m_hCount;
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("height");
				attr.InnerText = "" + m_vCount;
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("mode");
				attr.InnerText = "" + Project.drawTerraserverMode;
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("scale");
				attr.InnerText = "" + m_tileScale;
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("scalename");
				attr.InnerText = m_tileScaleName;
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("description");
				attr.InnerText = "" + m_placeDescription;
				node.Attributes.Append(attr);
				for(int vvv=0; vvv < m_vCount ;vvv++) 
				{
					for(int hhh=0; hhh < m_hCount ;hhh++) 
					{
						TileTerra tile = m_tiles[vvv, hhh];
						XmlNode tileNode = tile.ToXmlNode(xmlDoc);
						CameraTrack.AddTerraTile(tile.baseName, m_tileResolutionDegreesH, m_tileResolutionDegreesV, tile);
						attr = xmlDoc.CreateAttribute("cell");
						attr.InnerText = "" + hhh + "," + vvv;
						tileNode.Attributes.Append(attr);
						node.AppendChild(tileNode);
					}
				}
				cameraTrack.log(node);

				/*
				// this works only when Web Services are enabled:
				if (lps != null && lps.Length > 0)
				{
					node = xmlDoc.CreateNode(XmlNodeType.Element, "landmarks", null);
					int lpsCount = lps.Length;
					foreach (LandmarkPoint lp in lps) 
					{
						double lng = lp.Point.Lon;
						double lat = lp.Point.Lat;
						string name = lp.Name;
						string type = lp.Type;

						XmlNode lmNode = xmlDoc.CreateNode(XmlNodeType.Element, "lm", null);
						lmNode.InnerText = name;
						attr = xmlDoc.CreateAttribute("lon");
						attr.InnerText = "" + ((float)lng);
						lmNode.Attributes.Append(attr);
						attr = xmlDoc.CreateAttribute("lat");
						attr.InnerText = "" + ((float)lat);
						lmNode.Attributes.Append(attr);
						attr = xmlDoc.CreateAttribute("type");
						attr.InnerText = "" + type;
						lmNode.Attributes.Append(attr);
						node.AppendChild(lmNode);
					}
					cameraTrack.log(node);
				}
				*/				

				// this works in offline too:
				ArrayList landmarks = TerraserverCache.Landmarks; //TerraserverCache.getLandmarksInArea(m_cameraManager.terraTopLeft, m_cameraManager.terraBottomRight);
				if (landmarks.Count > 0)
				{
					node = xmlDoc.CreateNode(XmlNodeType.Element, "landmarks", null);
					int lpsCount = landmarks.Count;
					foreach (Landmark lm in landmarks) 
					{
						if(TerraserverCache.landmarkCanShow(lm.LandmarkType))
						{
							double lng = lm.Location.Lng;
							double lat = lm.Location.Lat;
							string name = lm.Name;
							string type = lm.LandmarkType;

							XmlNode lmNode = xmlDoc.CreateNode(XmlNodeType.Element, "lm", null);
							lmNode.InnerText = name;
							attr = xmlDoc.CreateAttribute("lon");
							attr.InnerText = "" + ((float)lng);
							lmNode.Attributes.Append(attr);
							attr = xmlDoc.CreateAttribute("lat");
							attr.InnerText = "" + ((float)lat);
							lmNode.Attributes.Append(attr);
							attr = xmlDoc.CreateAttribute("type");
							attr.InnerText = "" + type;
							lmNode.Attributes.Append(attr);
							node.AppendChild(lmNode);
						}
					}
					cameraTrack.log(node);
				}				
			}
		}

		public override void CameraMoved()
		{
			// LibSys.StatusBar.Trace("IP: TileSetTerra::CameraMoved()  drawableReady=" + Project.drawableReady);
			base.CameraMoved();	// provoke recalc

			if(m_retilingSpecial)
			{
				//ReTile(-1);	// can't have it here, will leave cleaned tiles while using arrowpad
				//PictureManager.This.LayersManager.CameraMoved();
			}
			else
			{
				ReTile(-1);
			}

			setMainFormText();
		}

		/// <summary>
		/// clears isEmpty flags for all tiles in the cache
		/// </summary>
		/// <returns></returns>
		public void ReTileSpecialClear()
		{
			if(!m_isSecond)
			{
				TerraserverCache.Clear();
			}
		}

		public bool ReTileSpecial(int scaleIndex, out int total, out int toLoad, out GeoCoord topLeft, out GeoCoord bottomRight)
									// scaleIndex 0-24 from m_scales[]
		{
			topLeft = null;
			bottomRight = null;

			// first check if requested scale is supported for this type of map
			if(m_isSecond || !calcScale(scaleIndex))
			{
				total = 0;
				toLoad = 0;
				return false;
			}

			if(!m_retilingSpecial && Project.camTrackOn)
			{
				firstSuperframe = true;
			}
			m_retilingSpecial = true;

			m_cameraManager.logCamtrackFrame();

			// ok, we have a fair chance of rendering the map at the requested scale.
			base.CameraMoved();	// provoke recalc

			ReTile(scaleIndex);

			m_pictureManager.ProcessCameraMove();	// need to call all layers here

			setMainFormText();
			total = m_hCount * m_vCount;
			// count tiles that have images and need not be downloaded:
			int _toLoad = total;
			for(int hhh=0; hhh < m_hCount ;hhh++) 
			{
				for(int vvv=0; vvv < m_vCount ;vvv++) 
				{
					TileTerra tile = m_tiles[vvv, hhh];
					if(tile != null && tile.backdrop != null && tile.backdrop.HasImage)
					{
						_toLoad--;
					}
				}
			}
			toLoad = _toLoad;
			topLeft = new GeoCoord(m_cameraManager.CoverageTopLeft);
			bottomRight = new GeoCoord(m_cameraManager.CoverageBottomRight);

			return true;
		}

		public void tileBackdropArrived(TileTerra tile)
		{
			// the backdrop may arrive when the tile has been phased out due to TileSetTerra.ReTile()
			// the tile then hangs around as a means to pass backdrop to TileSetTerra, which is supposed
			// to find appropriate location for the backdrop.
			bool found = false;
#if DEBUG
			//LibSys.StatusBar.Trace("TileSetTerra::tileBackdropArrived() - " + tile.ToString());
#endif
			lock(tilesLock)
			{
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						TileTerra tt = m_tiles[vv, hh];	// never null
						//LibSys.StatusBar.Trace(" = " + tt.baseName);
						if(tt == tile) 
						{
							// tile still in the array
#if DEBUG
							//LibSys.StatusBar.Trace(" - still in array : " + tt.baseName);
#endif
							ArrangeBackdropRedraw(tt);
							found = true;
						} 
						else if(tt.baseName.Equals(tile.baseName))
						{
							// tile has been replaced by another tile with the same base name (due to ReTile())
#if DEBUG
							//LibSys.StatusBar.Trace(" - " + tt.baseName + " - replaced by " + tile.baseName + " " + tile.backdrop);
#endif
							tt.backdrop = tile.backdrop;
							ArrangeBackdropRedraw(tt);
							found = true;
						}
						if(found)
						{
							goto endLoop;
						}
					}
				}
			endLoop:
				;
			} // end lock
			// tile not found - bitmap/backdrop stays in file system and cache for the future.
		}

		public void ArrangeBackdropRedraw(TileTerra tile)
		{
			try
			{
				//LibSys.StatusBar.Trace("TileSetTerra::ArrangeBackdropRedraw() - " + tile.ToString());
				Rectangle tileRect = tile.getFrameRectangle();
				//LibSys.StatusBar.Trace("     - tileRect " + tileRect);
				Rectangle toDrawRect = m_pictureManager.PictureBox.Bounds;
				//LibSys.StatusBar.Trace("     - toDrawRect " + toDrawRect);
				toDrawRect.Intersect(tileRect);
				//LibSys.StatusBar.Trace("     - " + tile.ToString() + " invalidating " + toDrawRect);
				m_pictureManager.Invalidate(toDrawRect);
			} 
			catch {}
		}

		private static bool m_drawableReady = false;

		public override void Paint(object sender, PaintEventArgs e)
		{
			//LibSys.StatusBar.Trace("TileSetTerra:Paint  rect=" + e.ClipRectangle);

			if(isPassiveSecond)
			{
				return;
			}

			if(!Project.spoiling)
			{
				try 
				{
					if(!m_drawableReady) 
					{
						m_drawableReady = true;
						m_retilingSpecial = false;
						ReTile(-1);
						setMainFormText();
					}

					for(int vv=0; vv < m_vCount ;vv++) 
					{
						for(int hh=0; hh < m_hCount ;hh++) 
						{
							TileTerra tile = m_tiles[vv, hh];
							//LibSys.StatusBar.Trace("TileSetTerra:Paint() - tile " + tile);
							Rectangle tileRect = tile.getFrameRectangle();
							if(tile != null && tileRect.IntersectsWith(e.ClipRectangle)) 
							{
								tile.Tile_Paint(sender, e);
								if(m_retilingSpecial)
								{
									e.Graphics.DrawRectangle(Pens.Black, tileRect);
								}
							}
						}
					}
				}
				catch(Exception eee) 
				{
#if DEBUG
					LibSys.StatusBar.Error("TileSetTerra:Paint(): " + eee.Message);
#endif
				}
			}
		}

		// support for evaluation spoiling:
		private int computeTreshold(byte[] rands, int wordLength)
		{
			// return an integer that is guaranteed to be the closest
			// minimum greater than exactly wordLength numbers in the array 
			int ret = 1;
			int cnt = 0;
			while (ret < 256 && cnt < wordLength)
			{
				cnt = 0;
				foreach(byte b in rands)
				{
					if((int)b < ret)
					{
						cnt++;
					}
				}
				ret++;
			}
			return ret;
		}

		public override Rectangle getFrameRectangle(ITile iTile)
		{
			return iTile == null ? m_cameraManager.getFrameRectangle() : iTile.getFrameRectangle();
		}

		public override void Paint2(object sender, PaintEventArgs e)
		{
			if(isPassiveSecond)
			{
				return;
			}

			//LibSys.StatusBar.Trace("TileSetTerra:Paint2  rect=" + e.ClipRectangle);
			if(Project.drawLandmarks)
			{
				try 
				{
					if(m_drawableReady)
					{
						if(!HasPutOnMap) 
						{
							//LibSys.StatusBar.Trace("TileSetTerra:Paint2(): calling PutOnMap()");
							PutOnMap();
						}

						foreach(Landmark lm in TerraserverCache.Landmarks)
						{
							if(lm.BoundingRect.IsEmpty)	// new object just added dynamically, or old object moved
							{
								lm.PutOnMap(this, null, this);
								PictureManager.Invalidate(lm.BoundingRect);
							}
							Point pixelPos = toPixelLocation(lm.Location, null);
							if(e.ClipRectangle.IntersectsWith(lm.BoundingRect)) 
							{
								if(TerraserverCache.landmarkCanShow(lm.LandmarkType))
								{
									lm.Paint(e.Graphics, this, null);
								}
							}
						}

						foreach(LiveObject lm in TerraserverCache.Landmarks)
						{
							lm.AdjustPlacement(this, null, this);
						}

						foreach(Landmark lm in TerraserverCache.Landmarks)
						{
							Point pixelPos = toPixelLocation(lm.Location, null);
							if(e.ClipRectangle.IntersectsWith(lm.BoundingRect)) 
							{
								if(TerraserverCache.landmarkCanShow(lm.LandmarkType))
								{
									lm.PaintLabel(e.Graphics, this, null, false);
								}
							}
						}
					}
				}
				catch(Exception eee) 
				{
#if DEBUG
					LibSys.StatusBar.Error("TileSetTerra:Paint2(): " + eee);
#endif
				}
			}
		}
		
		//
		// printing support:
		//
		public override void printPaint(Graphics graphics)
		{
			if(isPassiveSecond)
			{
				return;
			}

			//LibSys.StatusBar.Trace("TileSetTerra:Paint  clip rect=" + ppe.ClipRectangle);

			try 
			{
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						TileTerra tile = m_tiles[vv, hh];
						//LibSys.StatusBar.Trace("TileSetTerra:printPaint() - tile " + tile);
						if(tile != null) 
						{
							tile.Tile_PaintPrint(graphics);
						}
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("TileSetTerra:printPaint(): " + eee);
			}
		}

		public override void printPaint2(Graphics graphics)
		{
			if(isPassiveSecond)
			{
				return;
			}

			//LibSys.StatusBar.Trace("TileSetTerra:printPaint2  rect=" + e.ClipRectangle);
			if(Project.drawLandmarks)
			{
				try 
				{
					foreach(Landmark lm in TerraserverCache.Landmarks)
					{
						if(TerraserverCache.landmarkCanShow(lm.LandmarkType))
						{
							// find pixel offset between display position and print position:
							Point pixelPosPrint = toPixelLocationPrint(lm.Location, null);
							Point pixelPosDispl = lm.PixelLocation;
							int offsetX = pixelPosPrint.X - pixelPosDispl.X;
							int offsetY = pixelPosPrint.Y - pixelPosDispl.Y;
							lm.Paint(graphics, this, null, offsetX, offsetY);
							lm.PaintLabel(graphics, this, null, true, offsetX, offsetY);
						}
					}
				}
				catch(Exception eee) 
				{
					LibSys.StatusBar.Error("TileSetTerra:printPaint2(): " + eee);
				}
			}
		}

		public void PutOnMap()
		{
			if(m_isSecond)
			{
				return;
			}

			if(TerraserverCache.Landmarks.Count == 0)
			{
				return;		// try when landmarks arrive
			}

#if DEBUG
			LibSys.StatusBar.Trace("IP: TileSetTerra:PutOnMap()  " + TerraserverCache.Landmarks.Count + " landmarks");
#endif

			foreach(LiveObject lo in TerraserverCache.Landmarks)
			{
				try 
				{
					lo.init(true);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("TileSetTerra:PutOnMap(): lo=" + lo + " " + e);
				}
			}
			foreach(LiveObject lo in TerraserverCache.Landmarks)
			{
				try 
				{
					lo.PutOnMap(this, null, this);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("TileSetTerra:PutOnMap(): lo=" + lo + " " + e);
				}
			}
			foreach(LiveObject lo in TerraserverCache.Landmarks)
			{
				try 
				{
					lo.AdjustPlacement(this, null, this);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("TileSetTerra:PutOnMap() - AdjustPlacement: lo=" + lo + " " + e);
				}
			}
			m_hasPutOnMap = true;
		}

		// actually counts intesections' total surface in pixels:
		public int countIntersections(LiveObject llo, Rectangle r)
		{
			int total = 0;
			foreach(LiveObject lo in TerraserverCache.Landmarks)
			{
				if(llo != lo)		// self don't count
				{
					Rectangle sr = lo.intersectionSensitiveRect();
					sr.Intersect(r);
					total += (sr.Width * sr.Height);
					sr = lo.intersectionSensitiveRect2();
					sr.Intersect(r);
					total += (sr.Width * sr.Height / 2);     // count only part of it, less important
				}
			}
			//LibSys.StatusBar.Trace("Intersection total=" + total + " at pos " + llo.LabelPosition);
			return total;
		}

		public ArrayList getListOfTiles()
		{
			ArrayList ret = new ArrayList();

			lock(tilesLock)
			{
				if(m_tiles != null)
				{
					foreach(TileTerra t in m_tiles) 
					{
						if(t != null && !t.baseName.StartsWith("empty")) 
						{
							ret.Add(t.baseName.Clone());
						}
					}
				}
			}

			return ret;
		}
	}
}
