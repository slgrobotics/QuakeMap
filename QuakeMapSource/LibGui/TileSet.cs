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
using LibGeo;
using LibSys;

namespace LibGui
{
	/// <summary>
	/// TileSet is responsible for instantiating a matrix of tiles needed to cover whole
	/// area of the PictureBox. It directs drawing events to individual tiles, so that they could draw themselves.
	/// It reacts on resizing and rescaling of PictureBox.
	/// It provides "drawing surface" services to cities etc.
	/// </summary>
	public abstract class TileSet : Layer
	{
		protected double m_tileResolutionDegreesH;	// degrees per tile - horizontal
		protected double m_tileResolutionDegreesV;	// degrees per tile - vertical
		public double TileResolutionDegreesH { get { return m_tileResolutionDegreesH; } }
		public double TileResolutionDegreesV { get { return m_tileResolutionDegreesV; } }

		private string tilesLock = "";
		protected int m_vCount = 0;		// m_tiles array dimensions - vertical and horizontal
		protected int m_hCount = 0;
		protected Tile[,] m_tiles = null;

		public TileSet(PictureManager pm, CameraManager cm) : base(pm, cm)
		{
		}

		public override void PictureResized()
		{
			// we rely on CameraManager:ReactPictureResized() to call ReTile() eventually through ProcessCameraMove()
				// LibSys.StatusBar.Trace("IP: TileSet::PictureResized()  drawableReady=" + Project.drawableReady);
				//ReTile();
		}

		public override void CameraMoved()
		{
			// LibSys.StatusBar.Trace("IP: TileSet::CameraMoved()  drawableReady=" + Project.drawableReady);
			base.CameraMoved();
			ReTile();
		}

		public override void shutdown()
		{
		}

		public void cleanTiles()
		{
			lock(tilesLock)
			{
				if(m_tiles != null)
				{
					Tile[,] tmp = m_tiles;
					m_tiles = null;
					foreach(Tile t in tmp) 
					{
						if(t != null) 
						{
							t.Dispose();
						}
					}
				}
			}
		}

		private void ReTile()
		{
			//LibSys.StatusBar.Trace("IP: ReTile()  drawableReady=" + Project.drawableReady);

			if(!Project.drawableReady)	// when starting, we get several resize events with different sizes, as 
										// layout settles. We want to ignore these and retile after the first paint.
			{
				return;
			}

			// what is the picture area in degrees?
			// use CameraManager.m_coverageXDegrees, m_coverageYDegrees for span in degrees

			// what is appropriate tile resolution?
			double cameraHeightKm = (m_cameraManager.Location.Elev) / 1000.0d;	// km
			string tileScale;
			// each tile scale has it's threshold, after which the next tile scale is used.
			// the idea is to use the tile while it maps closely to 1:1 in pixels, and when 
			// the mapping requires magnification (i.e. x tile 1200x1200 --> 1300x1300 on the picture),
			// then use the next level
			if(cameraHeightKm > 5000.0d) 
			{
				m_tileResolutionDegreesH = 360.0d;	// degrees per tile
				m_tileResolutionDegreesV = 180.0d;	// degrees per tile
				tileScale = "w";
			}
			else if(cameraHeightKm > 2000.0d)
			{
				m_tileResolutionDegreesH = 40.0d;	// degrees per tile
				m_tileResolutionDegreesV = 50.0d;	// degrees per tile
				tileScale = "y";
			}
			//else if(cameraHeightKm > 900.0d)
			else if(cameraHeightKm > 370.0d)
			{
				m_tileResolutionDegreesH = 10.0d;	// degrees per tile
				m_tileResolutionDegreesV = 10.0d;	// degrees per tile
				tileScale = "x";
			}
			/*
			else if(cameraHeightKm > 370.0d)
			{
				m_tileResolutionDegreesH = 4.0d;	// degrees per tile
				m_tileResolutionDegreesV = 4.0d;	// degrees per tile
				tileScale = "z";
			}
			*/
			else if(cameraHeightKm > 100.0d)
			{
				m_tileResolutionDegreesH = 1.0d;	// degrees per tile
				m_tileResolutionDegreesV = 1.0d;	// degrees per tile
				tileScale = "";
			}
			else
			{
				m_tileResolutionDegreesH = 0.5d;	// degrees per tile
				m_tileResolutionDegreesV = 0.5d;	// degrees per tile
				tileScale = "a";
			}

			// how many tiles v*h do we need to cover the picture area?
			GeoCoord topLeftTileCorner = getTopLeftTileCorner(m_cameraManager.CoverageTopLeft, tileScale, m_tileResolutionDegreesH, m_tileResolutionDegreesV);
			GeoCoord bottomRightTileCorner = getBottomRightTileCorner(m_cameraManager.CoverageBottomRight, tileScale, m_tileResolutionDegreesH, m_tileResolutionDegreesV);
#if DEBUG
			LibSys.StatusBar.Trace("TileSet:ReTile() topLeftTileCorner=" + topLeftTileCorner + " bottomRightTileCorner=" + bottomRightTileCorner);
#endif
			m_hCount = (int)((bottomRightTileCorner.Lng - topLeftTileCorner.Lng) / m_tileResolutionDegreesH); 
			m_vCount = (int)((topLeftTileCorner.Lat - bottomRightTileCorner.Lat) / m_tileResolutionDegreesV); 

			// world tile is replicated, if 180 meridian is in the picture:
			bool doWorldOffset = false;
			switch(tileScale) 
			{
				case "w":
					if(m_cameraManager.CoverageBottomRight.Lng > 180.0d)
					{
						m_hCount++;
						doWorldOffset = true;
					}
					else if(m_cameraManager.CoverageTopLeft.Lng < -180.0d) 
					{
						topLeftTileCorner.Lng -= 360.0d;	// left tile starts there
						m_hCount++;
						doWorldOffset = true;
					}
					break;
			}

#if DEBUG
			LibSys.StatusBar.Trace("TileSet:ReTile() m_vCount=" + m_vCount + " m_hCount=" + m_hCount + " doWorldOffset=" + doWorldOffset);
#endif

			// calculate meters per pixel for selected tile scale:
			m_xMetersPerPixel = m_cameraManager.MetersPerPixelX();
			m_yMetersPerPixel = m_cameraManager.MetersPerPixelY();
#if DEBUG
			LibSys.StatusBar.Trace("TileSet:ReTile() m_xMetersPerPixel=" + m_xMetersPerPixel + " m_yMetersPerPixel=" + m_yMetersPerPixel);
			LibSys.StatusBar.Trace("TileSet:ReTile() ----------------- before " + TileCache.ToString());
#endif

			double topLeftLat = topLeftTileCorner.Lat;
			cleanTiles();
			lock(tilesLock)
			{
				m_tiles = new Tile[m_vCount, m_hCount];
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					double topLeftLng = topLeftTileCorner.Lng;
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						GeoCoord topLeft = new GeoCoord(topLeftLng, topLeftLat, 0.0d);
						GeoCoord bottomRight = new GeoCoord(topLeftLng + m_tileResolutionDegreesH,
							topLeftLat - m_tileResolutionDegreesV, 0.0d);
						m_tiles[vv, hh] = new Tile(this, tileScale, topLeft, bottomRight);
						m_tiles[vv, hh].init();
						switch(tileScale) 
						{
							case "w":
								// world tiles use doWorldOffset around 180 meridian:
								if(doWorldOffset && hh > 0) 
								{
									int offsetX = m_tiles[vv, hh].getFrameRectangle().Width;
									m_tiles[vv, hh].Offset = new Point(offsetX, 0);
								}
								break;
							default:
								m_tiles[vv, hh].Offset = new Point(0, 0);
								break;
						}
#if DEBUG
						LibSys.StatusBar.Trace("    hh=" + hh + " ---- offset=" + m_tiles[vv, hh].getOffset());
#endif

						topLeftLng += m_tileResolutionDegreesH;
					}
					topLeftLat -= m_tileResolutionDegreesV;
				}
			} // lock
#if DEBUG
			LibSys.StatusBar.Trace("TileSet:ReTile() ----------------- after " + TileCache.ToString());
#endif
			TileCache.purge();
			double safeHeight = ((double)Project.CAMERA_HEIGHT_SAFE) * 1000.0d;
			if(m_cameraManager.Elev < safeHeight && (!Project.allowUnreasonableZoom && IsAllSubstitutes || IsEmpty))
			{
				// we get here if all tiles are already in cache, and all are empty.
				LibSys.StatusBar.Trace("TileSet:ReTile() - all tiles empty - repositioning");
				m_cameraManager.Elev = safeHeight;
			}
		}

		protected GeoCoord getTopLeftTileCorner(GeoCoord topLeft, string tileScale, double tileResolutionDegreesH, double tileResolutionDegreesV)
		{
			GeoCoord ret;
            
			double topLeftTileLng = -180.0d;
			double topLeftTileLat = 90.0d;

			switch(tileScale) 
			{
				case "y":
				case "z":
					topLeftTileLng = -260.0d;	// far enough to be off screen
					while(topLeftTileLng + tileResolutionDegreesH < topLeft.Lng) 
					{
						topLeftTileLng += tileResolutionDegreesH;
					}
					while(topLeftTileLat - tileResolutionDegreesV > topLeft.Lat) 
					{
						topLeftTileLat -= tileResolutionDegreesV;
					}
					break;
				case "w":
					break;
				default:
					topLeftTileLng = Math.Floor(topLeft.Lng / tileResolutionDegreesH) * tileResolutionDegreesH;
					topLeftTileLat = Math.Ceiling(topLeft.Lat / tileResolutionDegreesV) * tileResolutionDegreesV;
					break;
			}

			ret = new GeoCoord(topLeftTileLng, topLeftTileLat);
			return ret;
		}

		protected GeoCoord getBottomRightTileCorner(GeoCoord bottomRight, string tileScale, double tileResolutionDegreesH, double tileResolutionDegreesV)
		{
			GeoCoord ret;

			double bottomRightTileLng = 180.0d;
			double bottomRightTileLat = -90.0d;

			switch(tileScale) 
			{
				case "y":
				case "z":
					bottomRightTileLng = -180.0d;
					while(bottomRightTileLng < bottomRight.Lng) 
					{
						bottomRightTileLng += tileResolutionDegreesH;
					}
					bottomRightTileLat = 90.0d;
					while(bottomRightTileLat > bottomRight.Lat) 
					{
						bottomRightTileLat -= tileResolutionDegreesV;
					}
					break;
				case "w":
					break;
				default:
					bottomRightTileLng = Math.Ceiling(bottomRight.Lng / tileResolutionDegreesH) * tileResolutionDegreesH;
					bottomRightTileLat = Math.Floor(bottomRight.Lat / tileResolutionDegreesV) * tileResolutionDegreesV;
					break;
			}

			ret = new GeoCoord(bottomRightTileLng, bottomRightTileLat);
			return ret;
		}

		public void tileBackdropArrived(Tile tile)
		{
			// the backdrop may arrive when the tile has been phased out due to TileSet.ReTile()
			// the tile then hangs around as a means to pass backdrop to TileSet, which is supposed
			// to find appropriate location for the backdrop.
			bool found = false;
#if DEBUG
			LibSys.StatusBar.Trace("TileSet::tileBackdropArrived() - " + tile.ToString());
#endif
			lock(tilesLock)
			{
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						Tile tt = m_tiles[vv, hh];
						//LibSys.StatusBar.Trace(" = " + tt.baseName + " UseSubstitute=" + tt.UseSubstitute + " SubstituteName=" + tt.SubstituteName);
						if(tt == tile) 
						{
							// tile still in the array
#if DEBUG
							LibSys.StatusBar.Trace(" - still in array : " + tt.baseName);
#endif
							if(tt.UseSubstitute && tile.backdrop.HasImage && tt.SubstituteRectangle.IsEmpty)
							{
								tt.CalculateSubstituteRectangle();
							}
							ArrangeBackdropRedraw(tt);
							found = true;
						} 
						else
							if((!tt.UseSubstitute && tt.baseName.Equals(tile.baseName))
							|| (tt.UseSubstitute && tile.UseSubstitute && tt.SubstituteName.Equals(tile.SubstituteName)))
						{
							// tile has been replaced by another tile with the same base name (due to ReTile())
#if DEBUG
							LibSys.StatusBar.Trace(" - " + tt.baseName + " - replaced by " + tile.baseName + " " + tile.backdrop);
#endif
							tt.backdrop = tile.backdrop;
							if(!tt.baseName.Equals(tile.baseName)) 
							{
								//tt.backdrop.MarkUsed();
								tt.CalculateSubstituteRectangle();
							}
							ArrangeBackdropRedraw(tt);
							found = true;
						}
						if(found && !tt.MayUseSubstitute)
						{
							goto endLoop;
						}
					}
				}
			endLoop:
				;
			} // lock
			// tile not found - bitmap/backdrop stays in file system and cache for the future.
		}

		public void tileFeaturesArrived(Tile tile)
		{
			// the features may arrive when the tile has been phased out due to TileSet.ReTile()
			// the tile then hangs around as a means to pass features to TileSet, which is supposed
			// to find appropriate location for the features.
			bool found = false;
			Features features = tile.features;
			lock(tilesLock)
			{
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						Tile tt = m_tiles[vv, hh];
						if(tt == tile) 
						{
							// tile still in the array.
							// a tile with full substitute features will have same features name:
							if(tt.UseSubstitute && tt.features.BaseName.Equals(features.BaseName))
							{
								tt.CreateFeaturesSubset();
								tt.ProvokeFeaturesPutOnMap();		// provoke PutOnMap on the next Tile_Paint()
							}
							ArrangeFeaturesRedraw(tt);
							found = true;
						} 
						else
							if((!tt.UseSubstitute && tt.baseName.Equals(tile.baseName))
							|| (tt.UseSubstitute && tile.UseSubstitute && tt.SubstituteName.Equals(tile.SubstituteName)))
						{
							// tile has been replaced by a similar tile
							if(!tt.UseSubstitute) 
							{
								tt.features = features;
							}
							else if(tt.features.BaseName.Equals(features.BaseName))
							{
								// for tiles using substitutes and not having own features subset:
#if DEBUG
								LibSys.StatusBar.Trace(" - " + tt.baseName + " - copying features from " + features.BaseName);
#endif
								tt.features = features;
								tt.CreateFeaturesSubset();
								tt.ProvokeFeaturesPutOnMap();		// provoke PutOnMap on the next Tile_Paint()
							}
							ArrangeFeaturesRedraw(tt);
							found = true;
						}
						if(found && !tt.MayUseSubstitute)
						{
							goto endLoop;
						}
					}
				}
			endLoop:
				;
			} // lock
			// tile not found - features stay in file system and cache for the future.
		}

		public void ArrangeBackdropRedraw(Tile tile)
		{
			double safeHeight = ((double)Project.CAMERA_HEIGHT_SAFE) * 1000.0d;
			if(m_cameraManager.Elev < safeHeight && (!Project.allowUnreasonableZoom && IsAllSubstitutes || IsEmpty))
			{
				// we get here if the last tile arrived empty. and whole tile set is empty.
#if DEBUG
				LibSys.StatusBar.Trace("TileSet:tileBackdropArrived() - all tiles empty - repositioning");
#endif
				m_cameraManager.Elev = safeHeight;
				return;
			}
			if(m_cameraManager.Elev >= 5000000.0d)		// world map may span 180, redraw whole thing
			{
				Rectangle toDrawRect = m_pictureManager.PictureBox.Bounds;
				m_pictureManager.Invalidate(toDrawRect);
			}
			else
			{
				//LibSys.StatusBar.Trace("TileSet::tileBackdropArrived() - " + tile.ToString());
				//Point topLeft = tile.pixelTopLeft();
				//Point bottomRight = tile.pixelBottomRight();
				//Size size = new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
				//Rectangle tileRect = new Rectangle(topLeft, size);
				Rectangle tileRect = tile.getFrameRectangle();     //new Rectangle(topLeft, size);
				//LibSys.StatusBar.Trace("     - tileRect " + tileRect);
				Rectangle toDrawRect = m_pictureManager.PictureBox.Bounds;
				//LibSys.StatusBar.Trace("     - pbRect " + toDrawRect);
				toDrawRect.Intersect(tileRect);
				//LibSys.StatusBar.Trace("     - " + tile.ToString() + " invalidating " + toDrawRect);
				m_pictureManager.Invalidate(toDrawRect);
			}
		}

		public void ArrangeFeaturesRedraw(Tile tile)
		{
			//Point topLeft = tile.pixelTopLeft();
			//Point bottomRight = tile.pixelBottomRight();
			//Size size = new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
			Rectangle tileRect = tile.getFrameRectangle();     //new Rectangle(topLeft, size);
			//LibSys.StatusBar.Trace("tileFeaturesArrived() - tileRect " + tileRect);
			Rectangle toDrawRect = m_pictureManager.PictureBox.Bounds;
			//LibSys.StatusBar.Trace("tileFeaturesArrived() - pbRect " + toDrawRect);
			if(m_cameraManager.Elev < 5000000.0d)		// world map may span 180, redraw whole thing
			{
				toDrawRect.Intersect(tileRect);
			}
#if DEBUG
			LibSys.StatusBar.Trace("tileFeaturesArrived() - tile " + tile.ToString() + " invalidating " + toDrawRect);
#endif
			m_pictureManager.Invalidate(toDrawRect);
		}

		public bool IsEmpty		// true if ALL tiles are proven empty. 
		{
			get 
			{
				int count = 0;
				try 
				{
					foreach(Tile tt in m_tiles) 
					{
						if(tt.IsEmpty)
						{
							count++;
						}
					}
					//LibSys.StatusBar.Trace("TileSet::IsEmpty=" + (count == m_tiles.Length) + " count=" + count + " of " + m_tiles.Length);
					return count == m_tiles.Length;
				} 
				catch 
				{
					return false;
				}
			}
		}

		public bool IsAllSubstitutes		// true if ALL tiles are substitutes. 
		{
			get 
			{
				int count = 0;
				try 
				{
					foreach(Tile tt in m_tiles) 
					{
						if(tt.UseSubstitute)
						{
							count++;
						}
					}
					//LibSys.StatusBar.Trace("TileSet::IsAllSubstitutes=" + (count == m_tiles.Length) + " count=" + count + " of " + m_tiles.Length);
					return count == m_tiles.Length;
				} 
				catch 
				{
					return false;
				}
			}
		}

		private static bool m_drawableReady = false;

		public override void Paint(object sender, PaintEventArgs e)
		{
			//LibSys.StatusBar.Trace("TileSet:Paint()  rect=" + e.ClipRectangle);

			try 
			{
				if(!m_drawableReady) 
				{
					m_drawableReady = true;
					Project.spoiling = false;
					ReTile();
				}

				//lock(tilesLock)
				{
					for(int vv=0; vv < m_vCount ;vv++) 
					{
						for(int hh=0; hh < m_hCount ;hh++) 
						{
							Tile tile = m_tiles[vv, hh];
							//LibSys.StatusBar.Trace("TileSet:Paint() - tile " + tile.ToString());
							if(tile != null && tile.getFrameRectangle().IntersectsWith(e.ClipRectangle)) 
							{
								tile.Tile_Paint(sender, e);
							}
						}
					}
				}
			}
			catch(Exception eee) 
			{
#if DEBUG
				LibSys.StatusBar.Error("TileSet:Paint(): " + eee);
#endif
			}
		}

		public override void Paint2(object sender, PaintEventArgs e)
		{
			//LibSys.StatusBar.Trace("TileSet:Paint2()  rect=" + e.ClipRectangle);

			try 
			{
				//lock(tilesLock)
				{
					for(int vv=0; vv < m_vCount ;vv++) 
					{
						for(int hh=0; hh < m_hCount ;hh++) 
						{
							Tile tile = m_tiles[vv, hh];
							//LibSys.StatusBar.Trace("TileSet:Paint2() - tile " + tile.ToString());
							if(tile != null && tile.getFrameRectangle().IntersectsWith(e.ClipRectangle)) 
							{
								tile.Tile_Paint2(sender, e);
							}
						}
					}
				}
			}
			catch(Exception eee) 
			{
#if DEBUG
				LibSys.StatusBar.Error("TileSet:Paint2(): " + eee);
#endif
			}
		}

		//
		// printing support:
		//
		public override void printPaint(Graphics graphics)
		{
			//LibSys.StatusBar.Trace("TileSet:printPaint()  rect=" + ppe.ClipRectangle);

			try 
			{
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						Tile tile = m_tiles[vv, hh];
						//LibSys.StatusBar.Trace("TileSet:printPaint() - tile " + tile.ToString());
						tile.Tile_PaintPrint(graphics);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("TileSet:printPaint(): " + eee);
			}
		}

		public override void printPaint2(Graphics graphics)
		{
			//LibSys.StatusBar.Trace("TileSet:printPaint2()  rect=" + ppe.ClipRectangle);

			try 
			{
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						Tile tile = m_tiles[vv, hh];
						//LibSys.StatusBar.Trace("TileSet:printPaint2() - tile " + tile.ToString());
						tile.Tile_Paint2Print(graphics);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("TileSet:printPaint2(): " + eee);
			}
		}

		// returns number of LiveObject based objects added to the list:
		public override int getCloseObjects(SortedList list, GeoCoord loc, double radiusMeters)
		{
			int count = 0;

			try 
			{
				for(int vv=0; vv < m_vCount ;vv++) 
				{
					for(int hh=0; hh < m_hCount ;hh++) 
					{
						Tile tile = m_tiles[vv, hh];
						Features features = tile.features;
						if(features != null && !features.IsEmpty)
						{
							foreach(LiveObject lo in features.List)
							{
								double dMeters = loc.distanceFrom(lo.Location).Meters;
								if(dMeters <= radiusMeters)
								{
									list.Add(dMeters, lo);
									count++;
								}
							}
						}
					}
				}
			}
			catch {}

			return count;
		}

		public override Rectangle getFrameRectangle(ITile iTile)
		{
			return iTile.getFrameRectangle();
		}

		public override int countIntersections(IObjectsLayoutManager olm, LiveObject lo, Rectangle rect)
		{
			return olm.countIntersections(lo, rect);
		}
	}
}
