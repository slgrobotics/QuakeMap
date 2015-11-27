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
using LibSys;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// PictureManager is responsible for coordinating Camera, TileSet and MainForm elements/events.
	/// It holds PictureBox and makes it visible for everybody.
	/// </summary>
	public class PictureManager
	{
		public static PictureManager This = null;

		private PictureBox m_pictureBox;
		public PictureBox PictureBox { get { return m_pictureBox; } }
		public Graphics Graphics { get { return m_pictureBox.CreateGraphics(); } }

		private LayersManager m_layersManager;
		public LayersManager LayersManager { get { return m_layersManager; } }

		private CameraTrack m_cameraTrack;
		public CameraTrack CameraTrack { get { return m_cameraTrack; } }

		private CameraManager m_cameraManager;
		public CameraManager CameraManager { get { return m_cameraManager; } }

		private Form m_mainForm;
		public Form MainForm { get { return m_mainForm; } }

		public int Width  { get { return m_pictureBox.Width; } }
		public int Height { get { return m_pictureBox.Height; } }
		public double ShiftValueH { get { return (m_cameraManager.CoverageBottomRight.Lng - m_cameraManager.CoverageTopLeft.Lng) / 4.0d; } }	// one fourth of screen
		public double ShiftValueHSmall { get { return (m_cameraManager.CoverageBottomRight.Lng - m_cameraManager.CoverageTopLeft.Lng) / 16.0d; } }	// one fourth of screen
		public double ShiftValueV { get { return (m_cameraManager.CoverageTopLeft.Lat - m_cameraManager.CoverageBottomRight.Lat) / 4.0d; } }
		public double ShiftValueVSmall { get { return (m_cameraManager.CoverageTopLeft.Lat - m_cameraManager.CoverageBottomRight.Lat) / 16.0d; } }

		// printing support:
		private int m_widthPrint = 0;
		public int WidthPrint  { get { return m_widthPrint; } set { m_widthPrint = value; } }
		private int m_heightPrint = 0;
		public int HeightPrint { get { return m_heightPrint; } set { m_heightPrint = value; } }
		private int m_offsetXPrint = 0;
		public int OffsetXPrint  { get { return m_offsetXPrint; } set { m_offsetXPrint = value; } }
		private int m_offsetYPrint = 0;
		public int OffsetYPrint { get { return m_offsetYPrint; } set { m_offsetYPrint = value; } }

		private Rectangle m_boundsPrint;
		public Rectangle BoundsPrint  { get { return m_boundsPrint; } 
										set 
										{
											m_boundsPrint = value;
											WidthPrint = value.Width;
											HeightPrint = value.Height;
											OffsetXPrint = value.X;
											OffsetYPrint = value.Y;
										}
		}

		public PictureManager(PictureBox pb, Form mainForm, CameraTrack cameraTrack)
		{
			m_pictureBox = pb;
			m_mainForm = mainForm;
			m_cameraTrack = cameraTrack;
			This = this;
		}

		public void init(CameraManager cm)
		{
			m_cameraManager = cm;

			m_layersManager = new LayersManager(this);

			LayerBasicMap lbm = new LayerBasicMap(this, cm);
			if(!Project.drawRelief)
			{
				lbm.Enabled = false;
			}
			m_layersManager.Add(lbm);

			LayerTerraserver lts = new LayerTerraserver(this, cm);
			if(!Project.drawTerraserver)
			{
				lts.Enabled = false;
			}
			m_layersManager.Add(lts);

			LayerTerraserver lts2 = new LayerTerraserver(this, cm);
			if(!Project.drawTerraserver)
			{
				lts2.Enabled = false;
			}
			m_layersManager.Add(lts2);

			LayerCustomMaps lcm = new LayerCustomMaps(this, cm);
			if(!Project.drawCustomMaps)
			{
				lcm.Enabled = false;
			}
			m_layersManager.Add(lcm);

			LayerEarthquakes le = new LayerEarthquakes(this, cm);
			if(!Project.drawEarthquakes)
			{
				le.Enabled = false;
			}
			m_layersManager.Add(le);

			LayerWaypoints lw = new LayerWaypoints(this, cm);
			if(!Project.drawWaypoints)
			{
				lw.Enabled = false;
			}
			m_layersManager.Add(lw);

			LayerVehicles lv = new LayerVehicles(this, cm);
			if(!Project.drawVehicles)
			{
				lv.Enabled = false;
			}
			m_layersManager.Add(lv);

			m_layersManager.Add(m_cameraManager);	// camera manager is a layer too - draws camera and arrows
		}

		
		public void shutdown()
		{
			m_layersManager.shutdown();
		}

		public void Refresh()
		{
			Cursor.Current = Cursors.WaitCursor;
			Project.pictureDirty = true;	// will provoke periodicMaintenance() in MainForm call _Refresh() here
		}

		public void _Refresh()
		{
			try 
			{
				m_pictureBox.Invalidate();
				// Invalidate() just puts a message into GUI loop, while Refresh() calls repainting in the current thread
				// Therefore calling Refresh here causes freezing of the GUI
			} 
			catch {}
		}

		public void ProcessCameraMove()
		{
			m_layersManager.CameraMoved();
		}

		public SortedList getCloseObjects(GeoCoord loc, double radiusMeters)
		{
			return m_layersManager.getCloseObjects(loc, radiusMeters);
		}

		// mouse clicks processing:
		private Point mouseClickPosition;

		public void pictureMouseDown(System.Windows.Forms.MouseEventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			//LibSys.StatusBar.Trace(">>>>  pictureMouseDown(" + e + ") button=" + e.Button);
			this.mouseClickPosition = new Point(e.X, e.Y);
			GeoCoord mouseGeoLocation = m_cameraManager.toGeoLocation(mouseClickPosition);
			m_layersManager.ProcessMouseDown(mouseClickPosition, mouseGeoLocation, controlDown, shiftDown, altDown);
		}

		public void pictureMouseUp(System.Windows.Forms.MouseEventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			//LibSys.StatusBar.Trace(">>>>  pictureMouseUp(" + e + ") button=" + e.Button);
			this.mouseClickPosition = new Point(e.X, e.Y);
			GeoCoord mouseGeoLocation = m_cameraManager.toGeoLocation(mouseClickPosition);
			m_layersManager.ProcessMouseUp(mouseClickPosition, mouseGeoLocation, controlDown, shiftDown, altDown);
		}

		public void pictureMouseMove(System.Windows.Forms.MouseEventArgs e,
			bool controlDown, bool shiftDown, bool altDown)
		{
			try {
				Point movePoint = new Point(e.X, e.Y);
				GeoCoord mouseGeoLocation = m_cameraManager.toGeoLocation(movePoint);
				m_layersManager.ProcessMouseMove(movePoint, mouseGeoLocation, controlDown, shiftDown, altDown);
			} 
			catch {}  // may be null exception when moving mouse before first ProcessCameraMove()
		}

		public void pictureClick(System.EventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			GeoCoord mouseGeoLocation = m_cameraManager.toGeoLocation(mouseClickPosition);
			m_layersManager.ProcessMouseClick(mouseClickPosition, mouseGeoLocation, controlDown, shiftDown, altDown);
		}

		public LiveObject evalPictureClick(System.EventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			GeoCoord mouseGeoLocation = m_cameraManager.toGeoLocation(mouseClickPosition);
			return m_layersManager.EvalMouseClick(mouseClickPosition, mouseGeoLocation, controlDown, shiftDown, altDown);
		}

		public void Invalidate(Rectangle r)
		{
			m_pictureBox.Invalidate(r);
		}

		public Point PointToScreen(Point p)
		{
			return m_pictureBox.PointToScreen(p);
		}

		public void PictureManager_Resize(object sender, System.EventArgs e)
		{
#if DEBUG
			LibSys.StatusBar.Trace("resized to w=" + Width + " h=" + Height);
#endif

			if(Width > 1 && Height > 1)		// protect from minimized window 
			{
				m_cameraManager.ReactPictureResized();
				m_layersManager.PictureResized();
				Refresh();
			}
		}

		//
		// printing support:
		//
		public void printPicture(Graphics graphics, Rectangle marginBounds)
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: printing area: " + marginBounds);
#endif

			// memorize page margin bounds so that camera manager can use it as picture bounds:
			BoundsPrint = marginBounds;

			m_cameraManager.calcCoveragePrint();

			m_layersManager.printPaint(graphics);
		}
	}
}
