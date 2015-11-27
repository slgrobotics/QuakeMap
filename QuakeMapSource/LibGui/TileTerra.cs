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
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using LibSys;
using LibGeo;
using LibNet;
using LibNet.TerraServer;
using LonLatPt = LibNet.TerraServer.LonLatPt;

namespace LibGui
{
	/// <summary>
	/// Tile can draw itself on a PictureBox, reacting to paint events - in FAST and FINE modes.
	/// It knows about it's scale and logically operates as part of TileSet.
	/// </summary>
	public class TileTerra : ITile, IObjectsLayoutManager
	{
		protected TileSetTerra m_tileSet;
		public TileSetTerra TileSet { get { return m_tileSet; } }

		protected Scale m_tileScale;
		protected int m_level = 0;
		public int Level { get { return m_level; } }
		protected string m_baseName = "";
		public string baseName { get { return m_baseName; } set { m_baseName = value; } }
		public string getBaseName() { return m_baseName; }
		protected Backdrop m_tileBackdrop = null;
		public Backdrop backdrop { get { return m_tileBackdrop; } set { m_tileBackdrop = value; } }
		protected GeoCoord m_topLeft;
		public GeoCoord getTopLeft() { return m_topLeft; }
		protected GeoCoord m_bottomRight;
		public GeoCoord getBottomRight() { return m_bottomRight; }

		public Point getOffset() { return Point.Empty; }	// Implementing ITile

		// the following helps tileset decide if all tiles are empty, and zoom out:
		protected bool m_isEmpty = false;		// until backdrop is proven empty.
		public bool IsEmpty { get { return m_isEmpty; } }
		protected bool m_isCottageCheese = false;
		public bool IsCottageCheese { get { return m_isCottageCheese; } set { m_isCottageCheese = value; } }

		public TileTerra(TileSetTerra ts, Scale tileScale, GeoCoord topLeft, GeoCoord bottomRight)
		{
			m_tileSet = ts;
			m_tileScale = tileScale;
			m_topLeft = topLeft.Clone();
			m_bottomRight = bottomRight.Clone();
			//LibSys.StatusBar.Trace("TileTerra() topLeft=" + m_topLeft.ToString() + " bottomRight=" + m_bottomRight.ToString());
		}

		public void Dispose()
		{
			//LibSys.StatusBar.Trace("TileTerra:Dispose() baseName=" + m_baseName);
			if(m_tileBackdrop != null) 
			{
				TileCache.BackdropMarkUnused(m_baseName);
				m_tileBackdrop = null;
			}
		}

		public void init()
		{
			//LibSys.StatusBar.Trace("Tile:init() baseName=" + m_baseName);
			m_tileBackdrop = TerraserverCache.getBackdrop(this, m_baseName);		// may be Empty, never null
			if(m_tileBackdrop.IsEmpty)
			{
				m_isEmpty = true;
			}
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
				m_isEmpty = true;
			} 
			m_tileSet.tileBackdropArrived(this);
		}

		public override string ToString()
		{
			return "tileTerra " + m_baseName + " " + m_tileBackdrop;
		}

		private Rectangle compensateForDraw(Rectangle destRect)
		{
			// this is to eliminate visible "gaps" in tiles - when tiles are enlarged DrawImage does marginal job at the bottom and right edges:
			if(destRect.Width > 2000)
			{
				int compensate = (int)Math.Round(destRect.Width / 1000.0d) + 3;
				destRect.Inflate(compensate, compensate);
				destRect.Offset(-compensate / 2, -compensate / 2);
			}
			else if(destRect.Width > 1000)
			{
				int compensate = (int)Math.Round(destRect.Width / 900.0d) + 1;
				destRect.Inflate(compensate, compensate);
				destRect.Offset(-compensate / 2, -compensate / 2);
			}
			else if(destRect.Width > 400)
			{
				int compensate = (int)Math.Round(destRect.Width / 700.0d);
				destRect.Inflate(compensate, compensate);
				destRect.Offset(-compensate / 2, -compensate / 2);
			}

			return destRect;
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
					www = m_tileBackdrop.Width;
					hhh = m_tileBackdrop.Height;
					//LibSys.StatusBar.Trace("Tile_Paint(): tileImage w=" + www + " h=" + hhh);

					ImageAttributes imageAttr = new ImageAttributes(); 
 
					ColorMatrix cm = new ColorMatrix(); 
 
					Image image = m_tileBackdrop.Image;

					if(this.TileSet.isSecond && Project.makeWhitishTransparent)
					{
						Bitmap bmp = new Bitmap(image);

						// it takes way too long to make all whitish colors transparent. It may be possible to do it once and cache it though.
//						for (int r=253; r <= 255 ;r++)
//						{
//							for (int g=253; g <= 255 ;g++)
//							{
//								for (int b=253; b <= 255 ;b++)
//								{
									// make whitish areas transparent, so that aerial or color will be completely visible through the overlay:
									//bmp.MakeTransparent(Color.FromArgb(r, g, b));
									bmp.MakeTransparent(Color.FromArgb(255, 255, 255));
//								}
//							}
//						}

						image = bmp;
					}

					cm.Matrix00 = 1.0f; 
					cm.Matrix11 = 1.0f; 
					cm.Matrix22 = 1.0f; 
					float opacity = (float)(TileSet.isSecond ? Project.terraLayerOpacity2 : Project.terraLayerOpacity);
					cm.Matrix33 = opacity; 
 
					imageAttr.SetColorMatrix(cm); 

					destRect = compensateForDraw(destRect);

					e.Graphics.DrawImage(image, destRect, 0, 0, www, hhh, System.Drawing.GraphicsUnit.Pixel, imageAttr);

					if(Project.drawGrid) 
					{
						DrawGrid(e.Graphics, destRect, www, hhh, null);
					}
				} 
				else if(m_isCottageCheese)
				{
					e.Graphics.DrawLine(Project.cameraPen, destRect.X, destRect.Y, destRect.X + destRect.Width, destRect.Y + destRect.Height);
					e.Graphics.DrawLine(Project.cameraPen, destRect.X + destRect.Width, destRect.Y, destRect.X, destRect.Y + destRect.Height);
				}
				else 
				{
					if(m_tileBackdrop != null && m_tileBackdrop.ImageCorrupted)
					{
						DrawGrid(e.Graphics, destRect, www, hhh, "BAD JPG");
						if(!m_triedReload)
						{
							m_triedReload = true;
							TerraserverCache.resetBackdrop(this, m_baseName);		// try reloading
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
#if DEBUG
				LibSys.StatusBar.Error("TileTerra:Tile_Paint(): " + m_baseName + " " + eee);
#endif
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
					www = m_tileBackdrop.Width;
					hhh = m_tileBackdrop.Height;
					//LibSys.StatusBar.Trace("Tile_Paint(): tileImage w=" + www + " h=" + hhh);

					Image image = m_tileBackdrop.Image;

					ImageAttributes imageAttr = new ImageAttributes(); 
 
					ColorMatrix cm = new ColorMatrix(); 
 
					cm.Matrix00 = 1.0f; 
					cm.Matrix11 = 1.0f; 
					cm.Matrix22 = 1.0f; 
					float opacity = (float)(TileSet.isSecond ? Project.terraLayerOpacity2 : Project.terraLayerOpacity);
					cm.Matrix33 = opacity; 
 
					imageAttr.SetColorMatrix(cm); 

					destRect = compensateForDraw(destRect);

					graphics.DrawImage(image, destRect, 0, 0, www, hhh, System.Drawing.GraphicsUnit.Pixel, imageAttr);

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
				LibSys.StatusBar.Error("TileTerra:Tile_PaintPrint(): " + m_baseName + " " + eee);
			}
		}

		private static char[]  sep = new Char[1] { '-' };

		protected void DrawGrid(Graphics graphics, Rectangle destRect, int www, int hhh, string message)
		{
			// grid and useful labels:
			int xx = destRect.X;
			int yy = destRect.Y;
			int ww = destRect.Width;
			int hh = destRect.Height;

			graphics.DrawRectangle(Project.gridPen, xx, yy, ww - 1, hh - 1);

			if(ww > 100 && hh > 100)
			{
				Brush brush = baseName.StartsWith("T1") ? Project.gridTextBrush : Project.blackBrush;
				Font font = Project.getLabelFont(8);

#if DEBUG
				/*
				// to verify that we are in sync with accurate UTM conversion:
				LonLatPt lonlat = new LonLatPt();
				lonlat.Lat = m_topLeft.Lat;
				lonlat.Lon = m_topLeft.Lng;

				UtmPt utmpt = Projection.LonLatPtToUtmNad83Pt(lonlat);

				graphics.DrawString("" + ((long)utmpt.Y), font, brush, xx + 30, yy + 15);
				graphics.DrawString("" + ((long)utmpt.X), font, brush, xx + 5, yy + 50);
				graphics.DrawString("Z" + utmpt.Zone, font, brush, xx + 5, yy + 25);
				*/
#endif

				try 
				{
					// compute UTM by tile name:
					// we have to parse tile name here - something like T1-S16-Z17-X57-Y368
					string[] split = baseName.Split(sep);

					int scale = Convert.ToInt32(split[1].Substring(1));
					int metersPerPixel = (1 << (scale - 10));
					int utmZone = Convert.ToInt32(split[2].Substring(1));
					int utmX = Convert.ToInt32(split[3].Substring(1));
					int utmY = Convert.ToInt32(split[4].Substring(1)) + 1;

					int screenUtmX = (utmX * 200 * metersPerPixel);
					int screenUtmY = (utmY * 200 * metersPerPixel);

					graphics.DrawString(string.Format("{0:d07}", screenUtmY), font, brush, xx + 30, yy + 2);
					graphics.DrawString(string.Format("{0:d07}", screenUtmX), font, brush, xx + 2, yy + 40);
					//graphics.DrawString("z" + utmZone, font, brush, xx + 2, yy + 15);
					if(message != null)
					{
						graphics.DrawString(message, font, brush, xx + 40, yy + 20);
					}
				}
				catch {}

				//graphics.DrawString("" + m_topLeft.Lat, font, brush, xx + 40, yy + 5);
				//graphics.DrawString("" + m_topLeft.Lng, font, brush, xx + 5, yy + 40);
#if DEBUG
				//if(Project.enableTrace)
				//{
				//	graphics.DrawString(m_baseName, font, brush, xx + 20, yy + hh/2 - 12);
				//	graphics.DrawString("" + www + "x" + hhh + "-->" + ww + "x" + hh, font, brush, xx + 10, yy + hh/2 + 12);
				//}
#endif
			}
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

		private Point pixelTopLeftPrint()
		{
			return m_tileSet.toPixelLocationNoNormalizePrint(m_topLeft);
		}

		private Point pixelBottomRightPrint()
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

		// actually counts intesections' total surface in pixels:
		public int countIntersections(LiveObject llo, Rectangle r)
		{
			int total = 0;
			return total;
		}

		public XmlNode ToXmlNode(XmlDocument xmlDoc)
		{
			XmlNode ret = xmlDoc.CreateNode(XmlNodeType.Element, "terratile", null);

			ret.InnerText = "" + baseName;
			/*
			XmlAttribute attr = xmlDoc.CreateAttribute("topleft");
			attr.InnerText = "" + ((float)m_topLeft.Lng) + "," + ((float)m_topLeft.Lat);
			ret.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("bottomright");
			attr.InnerText = "" + ((float)m_bottomRight.Lng) + "," + ((float)m_bottomRight.Lat);
			ret.Attributes.Append(attr);
			*/
			return ret;
		}
	}
}
