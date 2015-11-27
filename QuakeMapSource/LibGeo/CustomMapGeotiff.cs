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
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using LibSys;
using LibFormats;

namespace LibGeo
{
	/// <summary>
	/// CustomMapGeotiff represents a GeoTIFF image and helps to lay it out on the map.
	/// </summary>
	public class CustomMapGeotiff : CustomMap
	{
		private GeoTiff m_geoTiff = null;
		private Bitmap m_image = null;

		public override string MapName { get { return m_geoTiff.MapName; } set { m_geoTiff.MapName = value; } }

		public override string CustomMapType { get { return "geotiff"; } }

		public override string Source { get { return m_geoTiff.FileName; } }

		public override GeoCoord TopLeft 
		{
			get 
			{
				return new GeoCoord(m_geoTiff.TopLeftLng, m_geoTiff.TopLeftLat);
			}
		}

		public override GeoCoord BottomRight 
		{
			get 
			{
				return new GeoCoord(m_geoTiff.BottomRightLng, m_geoTiff.BottomRightLat);
			}
		}

		public override void PutOnMap(IDrawingSurface tileSet, ITile iTile, IObjectsLayoutManager olm)
		{
			base.PutOnMap(tileSet, iTile, olm);

			m_pixelLocationBottomLeft = tileSet.toPixelLocation(new GeoCoord(m_geoTiff.BottomLeftLng, m_geoTiff.BottomLeftLat), iTile);
			m_pixelLocationTopRight = tileSet.toPixelLocation(new GeoCoord(m_geoTiff.TopRightLng, m_geoTiff.TopRightLat), iTile);
			m_pixelLocationBottomRight = tileSet.toPixelLocation(new GeoCoord(m_geoTiff.BottomRightLng, m_geoTiff.BottomRightLat), iTile);

			boundingRect();		// make sure we have current values there
		}
		
		public CustomMapGeotiff(GeoTiff geoTiff)   : base(geoTiff.MapName, new GeoCoord(geoTiff.TopLeftLng, geoTiff.TopLeftLat))
		{
			m_geoTiff = geoTiff;

			m_enabled = true;

			setImage();		// actually reads geotiff and sets image and location (loc)
		}

		public Bitmap Image 
		{
			get { return m_image; }
			set 
			{
				m_image = value;
				m_pixelRadius = m_image == null ? MIN_MAP_PIXEL_RADIUS : (m_image.Width + m_image.Height) * 3 / 16;
			}
		}

		private void setImage()
		{
			Image = getCashedImage();	// may be null
		}

		#region CustomMap bitmaps cache

		private static Hashtable bitmaps = new Hashtable();		// bitmap by sym
		private static Hashtable tried   = new Hashtable();		// if already tried to load bitmap by name, bool

		private Bitmap getCashedImage()
		{
			Bitmap ret = (Bitmap)bitmaps[MapName];

			if(ret != null)
			{
				return ret;
			}

			try 
			{
				ret = new Bitmap(m_geoTiff.FileName);
				bitmaps[MapName] = ret;
			} 
			catch (Exception exc)
			{
				// ok, we are possibly dealing with image that is not formatted in RGB (contignious) manner.
				// try converting it to a usable temp file.
				try 
				{
					LibSys.StatusBar.Error(exc.Message + " -- trying to convert image...");
					ret = m_geoTiff.FromNoncontigImage();
				} 
				catch (Exception exc1)
				{
					// can't do much.
					LibSys.StatusBar.Error(exc1.ToString());
					return null;
				}
			}

			return ret;
		}
		#endregion

		#region Paint() related

		public override void Paint(Graphics graphics, IDrawingSurface tileSet, ITile iTile, int offsetX, int offsetY)
		{
			if(!m_enabled) 
			{
				return;
			}

			Rectangle r = m_imageBoundingRect;
        
			if(m_image == null) 
			{
				Font font = Project.getLabelFont(m_fontSize);
			
				int xx = m_labelBoundingRect.X - 3;
				int yy = m_labelBoundingRect.Y - 1;
				string label = MapName + " - missing Tiff file or GeoTiff information";
				graphics.DrawString(label, font, Project.customMapFontBrush, xx+1, yy-1);

				graphics.DrawEllipse(Project.customMapPen, r.X+2, r.Y+2, r.Width-4, r.Height-4);
			} 
			else 
			{
				double angle = Math.Atan(((double)(m_pixelLocationTopRight.Y - m_pixelLocation.Y)) / ((double)(m_pixelLocationTopRight.X - m_pixelLocation.X)));
				float scaleX = ((float)(m_pixelLocationTopRight.X - m_pixelLocation.X)) / ((float)(m_pixelLocationBottomRight.X - m_pixelLocation.X));
				float scaleY = ((float)(m_pixelLocationBottomLeft.Y - m_pixelLocation.Y)) / ((float)(m_pixelLocationBottomRight.Y - m_pixelLocation.Y));
				Matrix X = new Matrix();
				X.Translate(m_pixelLocation.X, m_pixelLocation.Y);
				X.Scale(scaleX, scaleY);
				X.Rotate((float)(angle * 180.0d / Math.PI));
				//X.Shear(-0.01f, -0.01f);
				X.Translate(-m_pixelLocation.X, -m_pixelLocation.Y);
				graphics.Transform = X; 

				ImageAttributes imageAttr = new ImageAttributes(); 
 
				ColorMatrix cm = new ColorMatrix(); 
 
				cm.Matrix00 = 1.0f; 
				cm.Matrix11 = 1.0f; 
				cm.Matrix22 = 1.0f; 
				float opacity = (float)(Project.terraLayerOpacity3);
				cm.Matrix33 = opacity; 
 
				imageAttr.SetColorMatrix(cm);

				int www = m_image.Width;
				int hhh = m_image.Height;
				graphics.DrawImage(m_image, r, 0, 0, www, hhh, System.Drawing.GraphicsUnit.Pixel, imageAttr);
				//graphics.DrawImage(m_image, r);

				graphics.Transform = new Matrix(); 

				// add confidence to allow double-check positioning - draw corner points:

				Project.drawCross(graphics, Project.redPen, m_pixelLocation, 20);
				Project.drawCross(graphics, Project.redPen, m_pixelLocationBottomLeft, 20);
				Project.drawCross(graphics, Project.redPen, m_pixelLocationTopRight, 20);
				Project.drawCross(graphics, Project.redPen, m_pixelLocationBottomRight, 20);
			}
		}

		public override Rectangle intersectionSensitiveRect2()
		{
			return Rectangle.Empty;
		}

		public override Rectangle imageBoundingRect()
		{
			int width = m_pixelLocationBottomRight.X - m_pixelLocation.X;
			int height = m_pixelLocationBottomRight.Y - m_pixelLocation.Y;

			m_imageBoundingRect = m_image == null ?
				base.imageBoundingRect()
				: new Rectangle(m_pixelLocation.X, m_pixelLocation.Y, width, height);

			return m_imageBoundingRect;
		}
		#endregion

	}
}
