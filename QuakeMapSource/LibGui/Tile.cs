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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using LibSys;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// Tile can draw itself on a PictureBox, reacting to paint events - in FAST and FINE modes.
	/// It knows about it's scale and logically operates as part of TileSet.
	/// </summary>
	public class Tile : ITile, IObjectsLayoutManager
	{
		protected TileSet m_tileSet;
		public TileSet TileSet { get { return m_tileSet; } }

		protected string m_tileScale;
		protected int m_level = 0;
		public int Level { get { return m_level; } }
		protected string m_baseName = "";
		public string baseName { get { return m_baseName; } set { m_baseName = value; } }
		public string getBaseName() { return m_baseName; }
		protected Backdrop m_tileBackdrop = null;
		public Backdrop backdrop { get { return m_tileBackdrop; } set { m_tileBackdrop = value; } }
		protected Features m_tileFeatures = null;
		public Features features { get { return m_tileFeatures; } set { m_tileFeatures = value; } }
		protected GeoCoord m_topLeft;
		public GeoCoord getTopLeft() { return m_topLeft; }
		protected GeoCoord m_bottomRight;
		public GeoCoord getBottomRight() { return m_bottomRight; }

		// world tile is duplicated, and we need offset to handle duplicate locations on the map:
		protected Point m_offset;
		public Point getOffset() { return m_offset; }
		public Point Offset { set { m_offset = value; } }

		// the following helps tileset decide if all tiles are empty, and zoom out:
		protected bool m_isEmpty = false;		// until backdrop is proven empty.
		public bool IsEmpty { get { return m_isEmpty; } }

		// servicing "substitute" tiles - taking 10x10 degrees (x-level) and cutting into single degree and quarters:
		protected string m_substituteName = null;
		public string SubstituteName { get { return m_substituteName; } }
		protected bool m_useSubstitute = false;
		public bool UseSubstitute { get { return m_useSubstitute; } set { m_useSubstitute = value; } }
		public bool MayUseSubstitute { get { return m_level > 2; } }
		protected Rectangle m_substituteRectangle;
		public Rectangle SubstituteRectangle { get { return m_substituteRectangle; } }
		protected int m_aShiftX = 0;
		protected int m_aShiftY = 0;
		protected int m_siLng = 0;
		protected int m_siLat = 0;
		protected int m_substLng = 0;
		protected int m_substLat = 0;


		public Tile(TileSet ts, string tileScale, GeoCoord topLeft, GeoCoord bottomRight)
		{
			m_tileSet = ts;
			m_tileScale = tileScale;
			m_topLeft = topLeft.Clone();
			m_bottomRight = bottomRight.Clone();
#if DEBUG
			LibSys.StatusBar.Trace("Tile() topLeft=" + m_topLeft.ToString() + " bottomRight=" + m_bottomRight.ToString());
#endif
		}

		public void Dispose()
		{
			//LibSys.StatusBar.Trace("Tile:Dispose() baseName=" + m_baseName);
			if(m_tileBackdrop != null) 
			{
				TileCache.BackdropMarkUnused(m_baseName);
				m_tileBackdrop = null;
			}
			if(m_tileFeatures != null) 
			{
				TileCache.FeaturesMarkUnused(m_baseName);
				m_tileFeatures = null;
			}
		}

		public void init()
		{
			GeoCoord topLeft = m_topLeft.Clone();
			topLeft.Normalize();
			GeoCoord bottomRight = m_bottomRight.Clone();
			bottomRight.Normalize();

			int iLat = (int)Math.Ceiling(topLeft.Lat);
			int iLng = (int)Math.Floor(topLeft.Lng);
			m_siLng = iLng;
			m_siLat = iLat;
			if(m_tileScale.Equals("z")) 
			{
				// HACK:
				// compensate for the shift in z-level tiles
				// (their names point to the center, not the top left corner:
				iLat -= 2;
				iLng += 2;
			}
			string latDir = iLat < 0 ? "s" : "n";
			string lngDir = iLng < 0 ? "w" : "e";
			iLat = Math.Abs(iLat);
			iLng = Math.Abs(iLng);

			switch(m_tileScale) 
			{
				case "w":
					m_level = 0;
					m_baseName = "world_good";
					break;
				case "y":
					m_level = 1;
					if(iLat == 60 && latDir.Equals("s")) 
					{
						m_tileBackdrop = null;
						return;
					} 
					else 
					{
						string sLng = ("" + iLng).PadLeft(3,'0');
						string sLat = "" + iLat;
						m_baseName = m_tileScale + sLng + lngDir + sLat + latDir;
					}
					break;
				case "x":
					m_level = 2;
					m_baseName = m_tileScale + iLng + lngDir + iLat + latDir;
					break;
				case "z":
					m_level = 3;
					m_baseName = m_tileScale + iLng + lngDir + iLat + latDir;
					break;
				default:
					m_level = 4;
					iLat = (int)Math.Floor(bottomRight.Lat);
					iLng = (int)Math.Ceiling(bottomRight.Lng);
					if(iLng == -180) 
					{
						// edge correction needed around Lng=180
						iLng = 180;
					}
					latDir = iLat < 0 ? "s" : "n";
					lngDir = iLng <= 0 ? "w" : "e";
					m_siLng = iLng;
					m_siLat = iLat;
					iLat = Math.Abs(iLat);
					iLng = Math.Abs(iLng);
					m_baseName = m_tileScale + iLng + lngDir + iLat + latDir;
					break;
				case "a":
					m_level = 5;
					iLat = (int)Math.Floor(bottomRight.Lat);
					iLng = (int)Math.Ceiling(bottomRight.Lng);
					if(iLng == -180) 
					{
						// edge correction needed around Lng=180
						iLng = 180;
					}
					latDir = iLat < 0 ? "s" : "n";
					lngDir = iLng <= 0 ? "w" : "e";
					m_siLng = iLng;
					m_siLat = iLat;
					iLat = Math.Abs(iLat);
					iLng = Math.Abs(iLng);
					// figure out quadrant and correct the tile scale letter:
					double ty = topLeft.Lat - Math.Floor(topLeft.Lat);
					double tx = topLeft.Lng - Math.Floor(topLeft.Lng);

					if(ty > 0.01d && tx > 0.01d) 
					{
						m_tileScale = "d";
						m_aShiftX = 1;
						m_aShiftY = 1;
					}
					else if(tx > 0.01d) 
					{
						m_tileScale = "b";
						m_aShiftX = 1;
					}
					else if(ty > 0.01d) 
					{
						m_tileScale = "c";
						m_aShiftY = 1;
					}
					m_baseName = m_tileScale + iLng + lngDir + iLat + latDir;
					break;
			}

			if(MayUseSubstitute) 
			{
				// calculate substitute backdrop parameters, just in case real backdrop will be missing:
				int substLat = (int)Math.Floor(bottomRight.Lat);
				if(substLat >= 0) 
				{
					substLat = substLat / 10 + 1;
				}
				else if(substLat < 0 && substLat >= -10) 
				{
					substLat = 0;
					latDir = "n";
				}
				else
				{
					substLat = (int)Math.Ceiling(bottomRight.Lat + 0.0001d);
					substLat = substLat / 10;
				}
				substLat = substLat * 10;

				int substLng = (int)Math.Ceiling(bottomRight.Lng);
				if(substLng > 0) 
				{
					substLng = (int)Math.Floor(topLeft.Lng);
					substLng = substLng / 10;
				} 
				else if(substLng == 0) 
				{
					// edge correction needed around Lng=0
					substLng = 1;
				} 
				else if(substLng == -180) 
				{
					// edge correction needed around Lng=180
					substLng = 17;
				} 
				else 
				{
					substLng = substLng / 10 - 1;
				}
				substLng = substLng * 10;

				m_substLng = substLng;
				m_substLat = substLat;
				substLat = Math.Abs(substLat);
				substLng = Math.Abs(substLng);
				m_substituteName = "x" + substLng + lngDir + substLat + latDir;
#if DEBUG
				LibSys.StatusBar.Trace("Tile::init() m_substituteName=" + m_substituteName);
				LibSys.StatusBar.Trace("siLng=" + m_siLng + " m_substLng=" + m_substLng + " siLat=" + m_siLat + " m_substLat=" + m_substLat);
#endif
			} 

			//LibSys.StatusBar.Trace("Tile:init() baseName=" + m_baseName);
			m_tileBackdrop = TileCache.getBackdrop(this, m_baseName);		// may be Empty, never null
			if(m_tileBackdrop.IsEmpty)
			{
				// unlikely, but we may know immediately that we need to use substitute, if feasible:
				if(MayUseSubstitute) 
				{
					// get substitute backdrop:
					m_useSubstitute = true;
					//LibSys.StatusBar.Trace("Tile::init() - requesting backdrop substitute " + m_substituteName);
					m_tileBackdrop = TileCache.getBackdrop(this, m_substituteName);	// may be Empty, never null
					if(m_tileBackdrop.IsEmpty)
					{
						m_isEmpty = true;
					} 
					else if(m_tileBackdrop.HasImage)
					{
						CalculateSubstituteRectangle();  // may instead set IsEmpty
					}
					// else wait for arrival...
				} 
				else 
				{
					m_isEmpty = true;
				}
			}
			m_tileFeatures = TileCache.getFeatures(this, m_baseName);		// may be Empty, never null
			if(m_tileFeatures.IsEmpty)
			{
				// unlikely, but we may know immediately that we need to use substitute, if feasible:
				if(MayUseSubstitute) 
				{
					// get substitute features:
#if DEBUG
					LibSys.StatusBar.Trace("Tile::init() - requesting features substitute " + m_substituteName);
#endif
					m_tileFeatures = TileCache.getFeatures(this, m_substituteName);	// may be Empty, never null
					if(m_tileFeatures.HasLoaded) 
					{
						CreateFeaturesSubset();
						ProvokeFeaturesPutOnMap();		// provoke PutOnMap on the next Tile_Paint()
					}
					else
					{
#if DEBUG
						LibSys.StatusBar.Trace("Tile::init() - " + m_baseName + " - features hasn't loaded -  nothing to copy...");
#endif
					}
					// wait for arrival...
				} 
			} 
			else 
			{
				ProvokeFeaturesPutOnMap();		// provoke PutOnMap on the next Tile_Paint()
			}
		}

		/// <summary>
		/// for substitute tiles - 
		/// makes a subset of features out of existing features, adds it to cache (under this tile name), 
		/// so the next time this tile is called a cache-resident subset is ready for use
		/// </summary>
		public void CreateFeaturesSubset()
		{
			Features tileFeatures = TileCache.getFeatures(this, m_baseName);
			if(!m_tileFeatures.HasLoaded) 
			{
#if DEBUG
				LibSys.StatusBar.Trace("IP: Tile::CreateFeaturesSubset() - " + m_baseName + " - not loaded yet " + m_tileFeatures);
#endif
				return;
			} 
			if(tileFeatures.HasLoaded) 
			{
#if DEBUG
				LibSys.StatusBar.Trace("IP: Tile::CreateFeaturesSubset() - " + m_baseName + " - found in cache " + tileFeatures);
#endif
			} 
			else 
			{
				GeoCoord bottomRight = m_bottomRight.Clone();
				bottomRight.Normalize();
				GeoCoord topLeft = m_topLeft.Clone();
				topLeft.Normalize();
				double rightLng = bottomRight.Lng;
				double leftLng = topLeft.Lng;
				if(rightLng <= -180.0d) 
				{
					rightLng += 360.0d;
				}
				double bottomLat = bottomRight.Lat;
				double topLat = topLeft.Lat;

				//LibSys.StatusBar.Trace("IP: Tile::CreateFeaturesSubset() - " + m_baseName + " - copying features related to lng:" + leftLng + "<" + rightLng + " lat:" + bottomLat + "<" + topLat + " from " + m_tileFeatures.BaseName);

				// we are not cloning the lo assuming that lo's belonging to a tile are not reappearing
				// in any other tile. That also means that we need to cut selection rectangles correctly. 

				foreach(LiveObject lo in m_tileFeatures.List)
				{
					if(lo.Location.Lat <= topLat && lo.Location.Lat >= bottomLat
						&& lo.Location.Lng >= leftLng && lo.Location.Lng <= rightLng) 
					{
						try 
						{
							//LibSys.StatusBar.WriteLine(" --- " + lo);
							tileFeatures.Add(lo);
						} 
						catch (Exception e)
						{
						}
					}
				}

				tileFeatures.HasLoaded = true;
				tileFeatures.IsEmpty = false;
			}
			m_tileFeatures = tileFeatures;
			//LibSys.StatusBar.Trace("OK: Tile::CreateFeaturesSubset() - " + m_baseName + " result: " + m_tileFeatures);
			//TileCache.ListFeaturesCache();
		}

		public void CalculateSubstituteRectangle()		// needs backdrop image width and height
		{
			// calculate substitute rectangle:
			int bw = m_tileBackdrop.Width;
			double dw = (double)bw;
			double dh = (double)m_tileBackdrop.Height;
			int xxx = 0;
			int yyy = 0;
			int www = 0;
			int hhh = 0;
			switch (m_level) 
			{
				case 3:
					m_isEmpty = true;
					// TODO: put the right code here for z-level
					break;
				case 4:
					// one degree tile
					www = (int)Math.Round( dw / 10.0d );
					hhh = (int)Math.Round( dh / 10.0d );
#if DEBUG
					LibSys.StatusBar.Trace("Tile::CalculateSubstituteRectangle() m_siLng=" + m_siLng + " m_substLng=" + m_substLng);
#endif
					xxx = (int)Math.Round( dw / 10.0d * Math.Abs(m_siLng - m_substLng - 1));
					if(xxx > bw) 
					{
						// edge correction needed around Lng=0
						xxx -= www * 2;
					}
					yyy = (int)Math.Round( dh / 10.0d * Math.Abs(m_siLat - m_substLat + 1));
					break;
				case 5:
					// half degree tile
					www = (int)Math.Round( dw / 20.0d );
					hhh = (int)Math.Round( dh / 20.0d );
					xxx = (int)Math.Round( dw / 20.0d * (Math.Abs(m_siLng - m_substLng - 1) * 2 + m_aShiftX));
					if(xxx > bw) 
					{
						// edge correction needed around Lng=0
						xxx -= www * 4;
					}
					yyy = (int)Math.Round( dh / 20.0d * (Math.Abs(m_siLat - m_substLat + 1) * 2 + m_aShiftY));
					break;
			}
			m_substituteRectangle = new Rectangle(xxx, yyy, www, hhh);
#if DEBUG
			LibSys.StatusBar.Trace("Tile::CalculateSubstituteRectangle() " + m_baseName + " m_substituteRectangle=" + m_substituteRectangle);
#endif
		}

		public void backdropArrived(Backdrop backdrop)
		{
			// the backdrop may arrive when this tile has been phased out due to TileSet.ReTile()
			// this tile then hangs around as a means to pass backdrop to TileSet, which is supposed
			// to find appropriate location for the backdrop.
#if DEBUG
			LibSys.StatusBar.Trace("Tile::backdropArrived() - backdrop=" + backdrop);
#endif
			m_tileBackdrop = backdrop;
			if(m_tileBackdrop.IsEmpty)
			{
				// more likely, need to use substitute is discovered here.
				if(MayUseSubstitute && !UseSubstitute) 
				{
					// get substitute backdrop:
					m_useSubstitute = true;
#if DEBUG
					LibSys.StatusBar.Trace("Tile::backdropArrived() - requesting substitute " + m_substituteName);
#endif
					m_tileBackdrop = TileCache.getBackdrop(this, m_substituteName);	// may be Empty, never null
					if(m_tileBackdrop.IsEmpty)
					{
#if DEBUG
						LibSys.StatusBar.Trace("Tile::backdropArrived() - got empty substitute " + m_substituteName);
#endif
						m_isEmpty = true;
					} 
					else if(m_tileBackdrop.HasImage)
					{
#if DEBUG
						LibSys.StatusBar.Trace("Tile::backdropArrived() - got good substitute " + m_tileBackdrop);
#endif
						CalculateSubstituteRectangle();  // may instead set IsEmpty
					} 
					else
					{
#if DEBUG
						LibSys.StatusBar.Trace("Tile::backdropArrived() - will wait for substitute ");
#endif
						return;		// waiting for substitute backdrop image to arrive
					}
				} 
				else 
				{
					m_isEmpty = true;
				}
			} 
			else if(UseSubstitute && m_tileBackdrop.HasImage)
			{
				CalculateSubstituteRectangle();  // may instead set IsEmpty
			}
			m_tileSet.tileBackdropArrived(this);
		}

		public void featuresArrived(Features features)
		{
			// the Features may arrive when this tile has been phased out due to TileSet.ReTile()
			// this tile then hangs around as a means to pass Features to TileSet, which is supposed
			// to find appropriate location for the Features.
#if DEBUG
			LibSys.StatusBar.Trace("featuresArrived() - features=" + features);
#endif
			m_tileFeatures = features;
			if(m_tileFeatures.IsEmpty)
			{
				// more likely, need to use substitute is discovered here.
				if(MayUseSubstitute) 
				{
					// get substitute features:
#if DEBUG
					LibSys.StatusBar.Trace("Tile::featuresArrived() - requesting substitute " + m_substituteName);
#endif
					m_tileFeatures = TileCache.getFeatures(this, m_substituteName);	// may be Empty, never null
					if(m_tileFeatures.IsEmpty)
					{
#if DEBUG
						LibSys.StatusBar.Trace("Tile::featuresArrived() - got empty substitute " + m_substituteName);
#endif
					} 
					else
					{
#if DEBUG
						LibSys.StatusBar.Trace("Tile::featuresArrived() - copying relevant features");
#endif
						CreateFeaturesSubset();
						ProvokeFeaturesPutOnMap();		// provoke PutOnMap on the next Tile_Paint()
					}
				} 
			} 
			else
			{
				ProvokeFeaturesPutOnMap();		// provoke PutOnMap on the next Tile_Paint()
			}
			m_tileSet.tileFeaturesArrived(this);
		}

		public override string ToString()
		{
			return "tile " + m_baseName + " " + m_tileBackdrop;
		}
		
		public void ProvokeFeaturesPutOnMap()
		{
			if(m_tileFeatures != null)
			{
				m_tileFeatures.HasPutOnMap = false;
			}
		}

		private bool m_triedReload = false;		// limits reload attempts

		public void Tile_Paint(object sender, PaintEventArgs e)
		{
			//LibSys.StatusBar.Trace("Tile_Paint(): " + m_baseName + " painting to " + e.ClipRectangle);

			try 
			{
				Rectangle destRect = getFrameRectangle();
				int www = 0;
				int hhh = 0;
				//LibSys.StatusBar.Trace("Tile_Paint(): tileImage=" + m_tileBackdrop);
				if(m_tileBackdrop != null && !m_tileBackdrop.IsEmpty && m_tileBackdrop.HasImage) 
				{
					//int compensate = 2;
					//destRect.Inflate(compensate, compensate);
					//destRect.Offset(-compensate / 2, -compensate / 2);

					if(UseSubstitute) 
					{
						int offX = m_substituteRectangle.X;
						int offY = m_substituteRectangle.Y;
						www = m_substituteRectangle.Width;
						hhh = m_substituteRectangle.Height;
						e.Graphics.DrawImage(m_tileBackdrop.Image, destRect, offX, offY, www, hhh,
							System.Drawing.GraphicsUnit.Pixel);
					}
					else 
					{
						www = m_tileBackdrop.Width;
						hhh = m_tileBackdrop.Height;
						//LibSys.StatusBar.Trace("Tile_Paint(): tileImage w=" + www + " h=" + hhh);
						// destRect = e.ClipRectangle;
						e.Graphics.DrawImage(m_tileBackdrop.Image, destRect, 0, 0, www, hhh,
																	System.Drawing.GraphicsUnit.Pixel);
					}
					if(Project.drawGrid) 
					{
						DrawGrid(e.Graphics, destRect, www, hhh, null);
					}
				} 
				else 
				{
					if(m_tileBackdrop != null && m_tileBackdrop.ImageCorrupted)
					{
						DrawGrid(e.Graphics, destRect, www, hhh, "BAD JPG");
						if(!m_triedReload)
						{
							m_triedReload = true;
							TileCache.resetBackdrop(this, m_baseName);		// try reloading
						}
					}
					else
					{
						DrawGrid(e.Graphics, destRect, www, hhh, "NOT LOADED");
					}
				}

			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("Tile:Tile_Paint(): " + m_baseName + " " + eee);
			}
		}

		public void Tile_Paint2(object sender, PaintEventArgs e)
		{
			//LibSys.StatusBar.Trace("Tile_Paint2(): " + m_baseName + " painting to " + e.ClipRectangle);

			try 
			{
				//LibSys.StatusBar.Trace("Tile_Paint2(): tileFeatures=" + m_tileFeatures);
				if(m_tileFeatures != null && !m_tileFeatures.IsEmpty && m_tileFeatures.Count > 0) 
				{
					if(!m_tileFeatures.HasPutOnMap) 
					{
						//LibSys.StatusBar.Trace("Tile_Paint2(): " + m_baseName + " calling features.PutOnMap()");
						m_tileFeatures.PutOnMap(m_tileSet, this, this);
					}

					// we need to paint backwards, so that important cities are painted last:
					int i = m_tileFeatures.Count - 1;
					Type cityType = City.city.GetType();
					for( ; i >= 0; i--) 
					{
						LiveObject lo = (LiveObject)m_tileFeatures.List[i];
						try 
						{
							if(Project.drawCities || !lo.GetType().Equals(cityType))
							{
								Point pixelPos = m_tileSet.toPixelLocation(lo.Location, this);
								Rectangle adjustedBoundingRect = lo.BoundingRect;
								adjustedBoundingRect.Offset(m_offset);
								if(e.ClipRectangle.IntersectsWith(adjustedBoundingRect)) 
								{
									lo.Paint(e.Graphics, m_tileSet, this);
								}
							}
						} 
						catch(Exception ee) 
						{
							LibSys.StatusBar.Error("Tile:Tile_Paint2(): lo=" + lo + " " + ee);
						}
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("Tile:Tile_Paint(): " + m_baseName + " " + eee);
			}
		}

		public void Tile_PaintPrint(Graphics graphics)
		{
			//LibSys.StatusBar.Trace("Tile_PaintPrint(): " + m_baseName + " painting to " + ppe.ClipRectangle);

			try 
			{
				Rectangle destRect = getFrameRectanglePrint();
				int www = 0;
				int hhh = 0;
				//LibSys.StatusBar.Trace("Tile_PaintPrint(): tileImage=" + m_tileBackdrop);
				if(m_tileBackdrop != null && !m_tileBackdrop.IsEmpty && m_tileBackdrop.HasImage) 
				{
					if(UseSubstitute) 
					{
						int offX = m_substituteRectangle.X;
						int offY = m_substituteRectangle.Y;
						www = m_substituteRectangle.Width;
						hhh = m_substituteRectangle.Height;
						graphics.DrawImage(m_tileBackdrop.Image, destRect, offX, offY, www, hhh,
							System.Drawing.GraphicsUnit.Pixel);
					}
					else 
					{
						www = m_tileBackdrop.Width;
						hhh = m_tileBackdrop.Height;
						//LibSys.StatusBar.Trace("Tile_PaintPrint(): tileImage w=" + www + " h=" + hhh);
						// destRect = e.ClipRectangle;
						graphics.DrawImage(m_tileBackdrop.Image, destRect, 0, 0, www, hhh,
																	System.Drawing.GraphicsUnit.Pixel);
					}
					if(Project.drawGrid) 
					{
						DrawGrid(graphics, destRect, www, hhh, null);
					}
				} 
				else 
				{
					DrawGrid(graphics, destRect, www, hhh, null);
				}

			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("Tile:Tile_PaintPrint(): " + m_baseName + " " + eee);
			}
		}

		public void Tile_Paint2Print(Graphics graphics)
		{
			//LibSys.StatusBar.Trace("Tile_Paint2Print(): " + m_baseName + " painting to " + ppe.ClipRectangle);

			int offsetSave = m_offset.X;
			m_offset.X = 0;	// we use direct method of calculating print surface location, world offset does not apply.

			try 
			{
				//LibSys.StatusBar.Trace("Tile_Paint2Print(): tileFeatures=" + m_tileFeatures);
				if(m_tileFeatures != null && !m_tileFeatures.IsEmpty && m_tileFeatures.Count > 0) 
				{
					// we need to paint backwards, so that important cities are painted last:
					int i = m_tileFeatures.Count - 1;
					Type cityType = City.city.GetType();
					for( ; i >= 0; i--) 
					{
						LiveObject lo = (LiveObject)m_tileFeatures.List[i];
						try 
						{
							if(Project.drawCities || !lo.GetType().Equals(cityType))
							{
								Point pixelPosPrint = m_tileSet.toPixelLocationPrint(lo.Location, this);
								Point pixelPosDispl = lo.PixelLocation;
								int offsetX = pixelPosPrint.X - pixelPosDispl.X;
								int offsetY = pixelPosPrint.Y - pixelPosDispl.Y;
								lo.Paint(graphics, m_tileSet, this, offsetX, offsetY);
							}
						} 
						catch(Exception ee) 
						{
							LibSys.StatusBar.Error("Tile:Tile_Paint2Print(): lo=" + lo + " " + ee);
						}
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("Tile:Tile_Paint2Print(): " + m_baseName + " " + eee);
			}
			m_offset.X = offsetSave;
		}

		protected void DrawGrid(Graphics graphics, Rectangle destRect, int www, int hhh, string message)
		{
			// grid and useful labels:
			int xx = destRect.X;
			int yy = destRect.Y;
			int ww = destRect.Width;
			int hh = destRect.Height;

			graphics.DrawRectangle(Project.gridPen, xx, yy, ww - 1, hh - 1);
			Brush brush = Project.gridTextBrush;
			Font font = Project.getLabelFont(8);
			graphics.DrawString("" + m_topLeft.Lat, font, brush, xx + 40, yy + 5);
			graphics.DrawString("" + m_topLeft.Lng, font, brush, xx + 5, yy + 40);
			if(message != null)
			{
				graphics.DrawString(message, font, brush, xx + 40, yy + 20);
			}
#if DEBUG
			if(UseSubstitute) 
			{
				graphics.DrawString(m_substituteName + " (for " + m_baseName + ")", font, brush, xx + 50, yy + 70);
				graphics.DrawString("" + m_substituteRectangle, font, brush, xx + 50, yy + 90);
				graphics.DrawString("" + www + "x" + hhh + "-->" + ww + "x" + hh, font, brush, xx + 50, yy + 110);
				graphics.DrawString(("" + m_tileFeatures).Substring(10), font, brush, xx + 50, yy + 130);
			} 
			else 
			{
				graphics.DrawString(m_baseName + " " + www + "x" + hhh + "-->" + ww + "x" + hh, font, brush, xx + 50, yy + 70);
			}
			graphics.DrawString(m_baseName, font, brush, xx + ww/2 - 30, yy + hh/2 - 12);
			graphics.DrawString(m_baseName, font, brush, xx + 50, yy + hh - 80);
			graphics.DrawString(m_baseName, font, brush, xx + ww - 120, yy + hh - 80);
			graphics.DrawString(m_baseName, font, brush, xx + ww - 120, yy + 50);
#endif
			graphics.DrawString("" + m_bottomRight.Lat, font, brush, xx + ww - 60, yy + hh - 25);
			graphics.DrawString("" + m_bottomRight.Lng, font, brush, xx + ww - 35, yy + hh - 50);
		}

		// actually counts intesections' total surface in pixels:
		public int countIntersections(LiveObject llo, Rectangle r)
		{
			int total = 0;
			foreach(LiveObject lo in m_tileFeatures.List) 
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

		private Point pixelTopLeft()
		{
			return m_tileSet.toPixelLocationNoNormalize(m_topLeft);
		}

		private Point pixelBottomRight()
		{
			Point pbr = m_tileSet.toPixelLocationNoNormalize(m_bottomRight);
			//pbr.Offset(1, 1);
			return pbr;
		}

		public Rectangle getFrameRectangle()
		{
			Point topLeftPoint = pixelTopLeft();
			Point bottomRightPoint = pixelBottomRight();
			int xx = topLeftPoint.X;
			int yy = topLeftPoint.Y;
			int ww = bottomRightPoint.X - xx + 1;
			int hh = bottomRightPoint.Y - yy + 1;

			return new Rectangle(xx, yy, ww, hh);
		}

		public Point pixelTopLeftPrint()
		{
			return m_tileSet.toPixelLocationNoNormalizePrint(m_topLeft);
		}

		public Point pixelBottomRightPrint()
		{
			Point pbr = m_tileSet.toPixelLocationNoNormalizePrint(m_bottomRight);
			//pbr.Offset(1, 1);
			return pbr;
		}

		public Rectangle getFrameRectanglePrint()
		{
			Point topLeftPoint = pixelTopLeftPrint();
			Point bottomRightPoint = pixelBottomRightPrint();
			int xx = topLeftPoint.X;
			int yy = topLeftPoint.Y;
			int ww = bottomRightPoint.X - xx + 1;
			int hh = bottomRightPoint.Y - yy + 1;

			return new Rectangle(xx, yy, ww, hh);
		}

	}
}
