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
	/// Summary description for Layer.
	/// </summary>
	public abstract class Layer : IDrawingSurface, ITile
	{
		protected PictureManager m_pictureManager;
		public PictureManager PictureManager { get { return m_pictureManager; } }
		protected CameraManager m_cameraManager;
		public CameraManager CameraManager { get { return m_cameraManager; } }

		protected bool m_enabled = true;		// LayersManager won't paint or forward clicks to disabled layers
		public bool Enabled { get { return m_enabled; } set { m_enabled = value; } }

		public abstract string Name();

		public Layer(PictureManager pm, CameraManager cm)
		{
			m_pictureManager = pm;
			m_cameraManager = cm;
		}

		// implementation of IDrawingSurface:
		public Graphics getGraphics()
		{
			return m_pictureManager.PictureBox.CreateGraphics();
		}

		public virtual void Paint(object sender, PaintEventArgs e)
		{
		}

		public virtual void Paint2(object sender, PaintEventArgs e)
		{
		}

		public Rectangle PictureRectangle() { return new Rectangle(0, 0, m_pictureManager.Width, m_pictureManager.Height); }

		public virtual bool insideScreenRectangle(GeoCoord loc)
		{
			return m_cameraManager.insideScreenRectangle(loc);
		}

		public virtual Point toPixelLocation(GeoCoord loc, ITile tile)
		{
			return m_cameraManager.toPixelLocation(loc, tile);
		}

		public virtual Point toPixelLocationPrint(GeoCoord loc, ITile tile)
		{
			return m_cameraManager.toPixelLocationPrint(loc, tile);
		}

		public virtual Point toPixelLocationNoNormalize(GeoCoord loc)
		{
			return m_cameraManager.toPixelLocationNoNormalize(loc);
		}

		public virtual Point toPixelLocationNoNormalizePrint(GeoCoord loc)
		{
			return m_cameraManager.toPixelLocationNoNormalizePrint(loc);
		}

		public virtual void ProcessMouseMove(Point movePoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
		}
		
		public virtual void ProcessMouseDown(Point movePoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
		}
		
		public virtual void ProcessMouseUp(Point movePoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
		}
		
		public virtual void ProcessMouseClick(Point clickPoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
		}
		
		public virtual LiveObject EvalMouseClick(Point clickPoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
			return null;
		}
		
		protected double m_xMetersPerPixel;
		protected double m_yMetersPerPixel;

		public double xMetersPerPixel()
		{
			if(m_xMetersPerPixel == 0.0d)
			{
				m_xMetersPerPixel = m_cameraManager.MetersPerPixel();
			}
			return m_xMetersPerPixel;
		}

		public double yMetersPerPixel()
		{
			if(m_yMetersPerPixel == 0.0d)
			{
				m_yMetersPerPixel = m_cameraManager.MetersPerPixel();
			}
			return m_yMetersPerPixel;
		}

		public virtual int countIntersections(IObjectsLayoutManager iOlm, LiveObject lo, Rectangle rect)
		{
			if(iOlm != null) 
			{
				return iOlm.countIntersections(lo, rect);
			}
			else
			{
				return 0;
			}
		}

		// ITile implementation:
		public virtual Point getOffset() { return new Point(0, 0); }
		public GeoCoord getTopLeft() { return m_cameraManager.getTopLeft(); }
		public GeoCoord getBottomRight() { return m_cameraManager.getBottomRight(); }
		public virtual Rectangle getFrameRectangle() { return m_cameraManager.getFrameRectangle(); }
		public virtual Rectangle getFrameRectanglePrint() { return m_cameraManager.getFrameRectanglePrint(); }

		public virtual Rectangle getFrameRectangle(ITile iTile) { return getFrameRectangle(); }
		public virtual Rectangle getFrameRectanglePrint(ITile iTile) { return getFrameRectanglePrint(); }
		public double getCameraElevation() { return m_cameraManager.Elev; }

		public abstract void PictureResized();
		public virtual void CameraMoved()
		{
			// provoke recalculation:
			m_xMetersPerPixel = 0.0d;
			m_yMetersPerPixel = 0.0d;
		}

		public abstract void printPaint(Graphics graphics);
		public abstract void printPaint2(Graphics graphics);

		// returns number of LiveObject based objects added to the list:
		public virtual int getCloseObjects(SortedList list, GeoCoord loc, double radiusMeters)
		{
			return 0;
		}

		public abstract void shutdown();
	}
}
