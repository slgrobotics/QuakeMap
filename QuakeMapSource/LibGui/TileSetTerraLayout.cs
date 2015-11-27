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
using System.Threading;

using LibSys;
using LibGeo;
using LibNet.TerraServer;
using LonLatPt = LibNet.TerraServer.LonLatPt;
//using LibNet.LandmarkServer;
using LibNet;

namespace LibGui
{
	/// <summary>
	/// TileSetTerraLayout serves as a helper for pre-loading tiles for any area, without showing them on the map yet.
	/// so there is no provision to get the tile drawn or stored for drowing, like TileSetTerra does it.
	/// A fire-and-forget call to queryDisconnected() ensures that the tiles are added to the loading queue.
	/// </summary>
	public class TileSetTerraLayout
	{
		protected GeoCoord m_coverageTopLeft;
		protected GeoCoord m_coverageBottomRight;
		protected GeoCoord m_coverageCenter;
		protected Theme m_curTheme;				// = Theme.Photo;
		protected bool m_curThemeColor;			// = false;

		protected Scale m_tileScale;
		private static Hashtable m_duplicates = new Hashtable();

		public bool isValid;

		public static void reset()
		{
			m_duplicates.Clear();
		}

		public static int DuplicatesCount { get { return m_duplicates.Count; } }

//		public static void downloadAllLevels(GeoCoord topLeft, GeoCoord bottomRight)
//		{
//			downloadAtLevel(topLeft, bottomRight, 10, true);
//		}

		/// <summary>
		/// loads all levels, starting with startScale and up
		/// </summary>
		/// <param name="topLeft"></param>
		/// <param name="bottomRight"></param>
		/// <param name="startScale"></param>
		public static void downloadPdaThemesAtLevel(GeoCoord topLeft, GeoCoord bottomRight, int startScale, bool andUp)
		{
			downloadThemesAtLevel(topLeft, bottomRight, startScale, andUp, Project.pdaExportDoAerial, Project.pdaExportDoColor, Project.pdaExportDoTopo);
		}

		public static void downloadThemesAtLevel(GeoCoord topLeft, GeoCoord bottomRight, int startScale, bool andUp,
														bool doAerial, bool doColor, bool doTopo)
		{
			Theme theme;
			bool themeIsColor;

			if(doAerial)
			{
				theme = Theme.Photo;
				themeIsColor = false;
				downloadTiles(topLeft, bottomRight, startScale, theme, themeIsColor, andUp);
			}
			if(doColor)
			{
				theme = Theme.Photo;
				themeIsColor = true;
				downloadTiles(topLeft, bottomRight, startScale, theme, themeIsColor, andUp);
			}
			if(doTopo)
			{
				theme = Theme.Topo;
				themeIsColor = false;
				downloadTiles(topLeft, bottomRight, startScale, theme, themeIsColor, andUp);
			}
		}

		public static void downloadAtLevel(GeoCoord topLeft, GeoCoord bottomRight, int startScale, bool andUp)
		{
			Theme theme;
			bool themeIsColor;

			// just the current theme:
			switch(Project.drawTerraserverMode)
			{
				default:
				case "aerial":
					theme = Theme.Photo;
					themeIsColor = false;
					break;
				case "color aerial":
					theme = Theme.Photo;
					themeIsColor = true;
					break;
				case "topo":
					theme = Theme.Topo;
					themeIsColor = false;
					break;
				case "relief":
					theme = Theme.Relief;
					themeIsColor = false;
					break;
			}

			downloadTiles(topLeft, bottomRight, startScale, theme, themeIsColor, andUp);
		}

		private static void downloadTiles(GeoCoord topLeft, GeoCoord bottomRight, int startScale, Theme theme, bool themeIsColor, bool andUp)
		{
			for(int scale=startScale; scale <= 16 ;scale++)
			{
				TileSetTerraLayout layout = new TileSetTerraLayout(topLeft, bottomRight, 0.0d, scale, theme, themeIsColor);
				layout.queryDisconnected();
				if(!andUp)
				{
					break;
				}
			}
		}

		public static void listTilesAtAllLevels(Hashtable list, GeoCoord topLeft, GeoCoord bottomRight)
		{
			listTilesAtLevels(list, topLeft, bottomRight, 10, true);
		}

		public static void listTilesAtLevelsWithType(Hashtable list, GeoCoord topLeft, GeoCoord bottomRight,
			int startScale, bool andUp, bool doAerial, bool doColor, bool doTopo)
		{
			if(doAerial)
			{
				for(int scale=startScale; scale <= 16 ;scale++)
				{
					TileSetTerraLayout layout = new TileSetTerraLayout(topLeft, bottomRight, 0.0d, scale, Theme.Photo, false);
					layout.listQueryDisconnected(list);
					if(!andUp)
					{
						break;
					}
				}
			}

			if(doColor)
			{
				for(int scale=startScale; scale <= 16 ;scale++)
				{
					TileSetTerraLayout layout = new TileSetTerraLayout(topLeft, bottomRight, 0.0d, scale, Theme.Photo, true);
					layout.listQueryDisconnected(list);
					if(!andUp)
					{
						break;
					}
				}
			}

			if(doTopo)
			{
				for(int scale=startScale; scale <= 16 ;scale++)
				{
					TileSetTerraLayout layout = new TileSetTerraLayout(topLeft, bottomRight, 0.0d, scale, Theme.Topo, false);
					layout.listQueryDisconnected(list);
					if(!andUp)
					{
						break;
					}
				}
			}
		}

		/// <summary>
		/// lists tiles at all levels, starting with startScale and up
		/// </summary>
		/// <param name="topLeft"></param>
		/// <param name="bottomRight"></param>
		/// <param name="startScale"></param>
		public static int listTilesAtLevels(Hashtable list, GeoCoord topLeft, GeoCoord bottomRight, int startScale, bool andUp)
		{
			int levels = 0;
			Theme theme;
			bool themeIsColor;

			switch(Project.drawTerraserverMode)
			{
				default:
				case "aerial":
					theme = Theme.Photo;
					themeIsColor = false;
					break;
				case "color aerial":
					theme = Theme.Photo;
					themeIsColor = true;
					break;
				case "topo":
					theme = Theme.Topo;
					themeIsColor = false;
					break;
				case "relief":
					theme = Theme.Relief;
					themeIsColor = false;
					break;
			}

			for(int scale=startScale; scale <= 16 ;scale++)
			{
				TileSetTerraLayout layout = new TileSetTerraLayout(topLeft, bottomRight, 0.0d, scale, theme, themeIsColor);
				layout.listQueryDisconnected(list);
				levels++;
				if(!andUp)
				{
					break;
				}
			}
			return levels;
		}

		// Use index from TileSetTerra.m_scale to define desired scale. 
		// if scaleIndexHint==-1 the elev is used to find the best matching scale. See TileSetTerra.calcScaleIndex for more.

		public TileSetTerraLayout(GeoCoord coverageTopLeft, GeoCoord coverageBottomRight, double elev, int scaleIndexHint, Theme theme, bool themeIsColor)
		{
			m_coverageTopLeft = coverageTopLeft;
			m_coverageBottomRight = coverageBottomRight;
			m_curTheme = theme;
			m_curThemeColor = themeIsColor;

			double lng = (coverageTopLeft.Lng + coverageBottomRight.Lng) / 2;
			double lat = (coverageTopLeft.Lat + coverageBottomRight.Lat) / 2;
			m_coverageCenter = new GeoCoord(lng, lat);

			int scaleIndex;
			Scale tileScale;
			isValid = TileSetTerra.calcScaleIndex(m_curTheme, elev, scaleIndexHint, out scaleIndex, out tileScale);
			m_tileScale = tileScale;
		}

		private int getThemeCode()
		{
			int themeCode = (m_curThemeColor && m_curTheme == Theme.Photo) ? (int)Theme.Urban : ((int)m_curTheme);

			return themeCode;
		}

		#region queryDisconnected and listQueryDisconnected()

		public void queryDisconnected()
		{
			if(!isValid)
			{
				return;
			}

			ThreadPool2.QueueUserWorkItem(new WaitCallback (_queryDisconnected), "preloading tiles"); 
		}


		/// <summary>
		/// this is somewhat cut-and-paste from its namesake from the TileSetTerra.cs keep them in sync
		/// </summary>
		/// <param name="obj"></param>
		private void _queryDisconnected(object obj)
		{
			int xStart = 0;
			int yStart = 0;
			int xEnd = 0;
			int yEnd = 0;
			int m_lastFactor = 0;

			GeoCoord covTL = m_coverageTopLeft;
			GeoCoord covBR = m_coverageBottomRight;

			double marginY = 0.0d;
			double marginX = 0.0d;

			double covTL_Lng = covTL.Lng - marginX;
			double covTL_Lat = covTL.Lat + marginY;
			double covBR_Lng = covBR.Lng + marginX;
			double covBR_Lat = covBR.Lat - marginY;

			int currentZone = TileSetTerra.getZone(m_coverageCenter);

			LonLatPt lonlat = new LonLatPt();
			lonlat.Lat = covTL_Lat;
			lonlat.Lon = covTL_Lng;

			UtmPt utmptTL = Projection.LonLatPtToUtmNad83Pt(lonlat);		// can be in other than currentZone

			lonlat.Lat = covBR_Lat;
			lonlat.Lon = covBR_Lng;

			UtmPt utmptBR = Projection.LonLatPtToUtmNad83Pt(lonlat);		// can be in other than currentZone

			if(utmptTL.Zone != currentZone)
			{
				lonlat.Lat = m_coverageCenter.Lat;
				lonlat.Lon = m_coverageCenter.Lng;

				UtmPt utmptCam = Projection.LonLatPtToUtmNad83Pt(lonlat);	// can be in other than currentZone

				double dX = utmptBR.X - utmptCam.X;
				utmptTL.X = utmptCam.X - dX;
				utmptTL.Zone = currentZone;
			}
			else if(utmptBR.Zone != currentZone)
			{
				lonlat.Lat = m_coverageCenter.Lat;
				lonlat.Lon = m_coverageCenter.Lng;

				UtmPt utmptCam = Projection.LonLatPtToUtmNad83Pt(lonlat);	// can be in other than currentZone

				double dX = utmptCam.X - utmptTL.X;
				utmptBR.X = utmptCam.X + dX;
				utmptBR.Zone = currentZone;
			}

			int iScale = (int)m_tileScale;
			int metersPerPixel = (1 << ((int) iScale - 10));
			int factor = 200 * metersPerPixel;

			if(xEnd - xStart == 0 || m_lastFactor == 0)
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
			}

			int numTilesX = xEnd - xStart;
			int numTilesY = yStart - yEnd;

			int hCount = numTilesX;
			int vCount = numTilesY;

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

			int themeCode = getThemeCode();

			int xx = (int)xStart;

			for(int hhh=0; hhh < hCount ;hhh++) 
			{
				int yy = (int)yStart;

				for(int vvv=0; vvv < vCount ;vvv++) 
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

					if(TerraserverCache.TileNamesCollection != null)
					{
						if(!TerraserverCache.TileNamesCollection.ContainsKey(baseName))
						{
							TerraserverCache.TileNamesCollection.Add(baseName, null);
						}
					}

					lock(m_duplicates)
					{
						if(!m_duplicates.ContainsKey(baseName))
						{
							TerraserverCache.downloadIfMissing(baseName);
							m_duplicates.Add(baseName, null);
						}
					}
#if DEBUG
					//LibSys.StatusBar.Trace("[" + vvv + "," + hhh + "]  " + baseName);
					//LibSys.StatusBar.Trace("        -- topLeft=" + tileTopLeft + " bottomRight=" + tileBottomRight);
#endif
					yy--;
				}
				xx++;
			}

			int totalCount = TileSetTerraLayout.DuplicatesCount;
			int newCount = ThreadPool2.WaitingCallbacks;

			if(newCount == 0)
			{
				LibSys.StatusBar.Trace("*Preload tiles: nothing to download - all " + totalCount + " tiles already in cache.");
			}
			else
			{
				if(newCount <= totalCount)
				{
					LibSys.StatusBar.Trace("*Preload tiles: trying to download " + newCount + " new of total " + totalCount + " tiles");
				}
				else
				{
					LibSys.StatusBar.Trace("*Preload tiles: trying to download " + newCount + " new tiles");
				}
			}
		}

		/// <summary>
		/// this is complete cut-and-paste from the above - keep them in sync
		/// </summary>
		/// <param name="obj"></param>
		private void listQueryDisconnected(Hashtable list)
		{
			int xStart = 0;
			int yStart = 0;
			int xEnd = 0;
			int yEnd = 0;
			int m_lastFactor = 0;

			GeoCoord covTL = m_coverageTopLeft;
			GeoCoord covBR = m_coverageBottomRight;

			double marginY = 0.0d;
			double marginX = 0.0d;

			double covTL_Lng = covTL.Lng - marginX;
			double covTL_Lat = covTL.Lat + marginY;
			double covBR_Lng = covBR.Lng + marginX;
			double covBR_Lat = covBR.Lat - marginY;

			int currentZone = TileSetTerra.getZone(m_coverageCenter);

			LonLatPt lonlat = new LonLatPt();
			lonlat.Lat = covTL_Lat;
			lonlat.Lon = covTL_Lng;

			UtmPt utmptTL = Projection.LonLatPtToUtmNad83Pt(lonlat);		// can be in other than currentZone

			lonlat.Lat = covBR_Lat;
			lonlat.Lon = covBR_Lng;

			UtmPt utmptBR = Projection.LonLatPtToUtmNad83Pt(lonlat);		// can be in other than currentZone

			if(utmptTL.Zone != currentZone)
			{
				lonlat.Lat = m_coverageCenter.Lat;
				lonlat.Lon = m_coverageCenter.Lng;

				UtmPt utmptCam = Projection.LonLatPtToUtmNad83Pt(lonlat);	// can be in other than currentZone

				double dX = utmptBR.X - utmptCam.X;
				utmptTL.X = utmptCam.X - dX;
				utmptTL.Zone = currentZone;
			}
			else if(utmptBR.Zone != currentZone)
			{
				lonlat.Lat = m_coverageCenter.Lat;
				lonlat.Lon = m_coverageCenter.Lng;

				UtmPt utmptCam = Projection.LonLatPtToUtmNad83Pt(lonlat);	// can be in other than currentZone

				double dX = utmptCam.X - utmptTL.X;
				utmptBR.X = utmptCam.X + dX;
				utmptBR.Zone = currentZone;
			}

			int iScale = (int)m_tileScale;
			int metersPerPixel = (1 << ((int) iScale - 10));
			int factor = 200 * metersPerPixel;

			if(xEnd - xStart == 0 || m_lastFactor == 0)
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
			}

			int numTilesX = xEnd - xStart;
			int numTilesY = yStart - yEnd;

			int hCount = numTilesX;
			int vCount = numTilesY;

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

			int themeCode = getThemeCode();

			int xx = (int)xStart;

			for(int hhh=0; hhh < hCount ;hhh++) 
			{
				int yy = (int)yStart;

				for(int vvv=0; vvv < vCount ;vvv++) 
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

					if(!list.ContainsKey(baseName))
					{
						list.Add(baseName, null);
					}
#if DEBUG
					//LibSys.StatusBar.Trace("[" + vvv + "," + hhh + "]  " + baseName);
					//LibSys.StatusBar.Trace("        -- topLeft=" + tileTopLeft + " bottomRight=" + tileBottomRight);
#endif
					yy--;
				}
				xx++;
			}
		}

		#endregion

	}
}
