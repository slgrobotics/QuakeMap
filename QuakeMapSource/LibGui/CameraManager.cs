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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections;

using LibFormats;
using LibSys;
using LibGeo;
using LibNet;
using LibNet.TerraServer;

namespace LibGui
{
	/// <summary>
	/// CameraManager knows about current camera position and height, so that scale and central point of the 
	/// picture can be derived from this info.
	/// </summary>
	public class CameraManager : Layer
	{
		public static CameraManager This = null;

		public override string Name() { return "CameraManager"; }
		public bool HasDragRectangle { get { return !dragRectangle.IsEmpty; } }
		public string DragEndHint 
		{
			get { return m_doDistance ? ("distance: " + sDist) : (movePointMode ? "drag mouse to move waypoint" : "click into rectangle to zoom in"); }
		}
		private bool m_doDistance = false;
		private bool movePointMode  
		{
			get { return LibSys.Project.movePointMode; }
		}
		private bool moveLegMode  
		{
			get { return LibSys.Project.moveLegMode; }
		}
		private bool m_cameraMoving = false;
		public bool CameraMoving { get { return m_cameraMoving; } set { m_cameraMoving = value;} }
		public bool CanDrillDown { get { return !m_cameraMoving && dragRectangle.IsEmpty; } }

		private const int m_arrowWidth = 20;
		private const int m_arrowHeight = 20;

		private ILocationWatcher m_locationWatcher;
		public ILocationWatcher m_locationWatcher2 = null;

		private CameraTrack m_cameraTrack;
		public CameraTrack CameraTrack { get { return m_cameraTrack; } }

		private GeoCoord m_location;
		private GeoCoord m_locationPrev = null;	// helps detect if camera actually has moved
		private string m_drawTerraserverMode = Project.drawTerraserverMode;

		public GeoCoord Location {
			get { return m_location; }
			set { m_location = value; ProcessCameraMove();	}	// make sure to use a Clone() on existing location before passing it here
		}

		public double Elev { 
			get { return Location.Elev; }
			set { 
				if(value > Project.CAMERA_HEIGHT_MAX * 1000.0d)
				{
					m_location.Elev = Project.CAMERA_HEIGHT_MAX * 1000.0d;
				} 
				else if(value < Project.cameraHeightMin * 1000.0d)
				{
					m_location.Elev = Project.cameraHeightMin * 1000.0d;
				} 
				else
				{
					m_location.Elev = value;
				}
				ProcessCameraMove();
			} 
		}

		// how many degrees does the picture cover:
		private double m_coverageXDegrees = 1.0d;
		public double CoverageXDegrees { get { return m_coverageXDegrees; } set { m_coverageXDegrees = value; } }
		private double m_coverageYDegrees = 1.0d;
		public double CoverageYDegrees { get { return m_coverageYDegrees; } set { m_coverageYDegrees = value; } }

		// corners of the map as seen in the picture:
		private GeoCoord m_coverageTopLeft;
		private GeoCoord m_coverageTopLeft34;	// main area, 3/4th of the screen
		public GeoCoord CoverageTopLeft { get { return m_coverageTopLeft; } set { m_coverageTopLeft = value; } }
		public GeoCoord CoverageTopLeft34 { get { return m_coverageTopLeft34; } set { m_coverageTopLeft34 = value; } }
		private GeoCoord m_coverageBottomRight;
		private GeoCoord m_coverageBottomRight34;	// main area, 3/4th of the screen
		public GeoCoord CoverageBottomRight { get { return m_coverageBottomRight; } set { m_coverageBottomRight = value; } }
		public GeoCoord CoverageBottomRight34 { get { return m_coverageBottomRight34; } set { m_coverageBottomRight34 = value; } }

		// Terraserver tiles corners (valid if Project.terraserverAvailable == true), filled and reset in TileSetTerra:
		public GeoCoord terraTopLeft = new GeoCoord(0.0d, 0.0d);
		public GeoCoord terraBottomRight = new GeoCoord(0.0d, 0.0d);

		// printing support:
		private double m_coverageXDegreesPrint = 1.0d;
		public double CoverageXDegreesPrint { get { return m_coverageXDegreesPrint; } set { m_coverageXDegreesPrint = value; } }
		private double m_coverageYDegreesPrint = 1.0d;
		public double CoverageYDegreesPrint { get { return m_coverageYDegreesPrint; } set { m_coverageYDegreesPrint = value; } }
		private GeoCoord m_coverageTopLeftPrint;
		public GeoCoord CoverageTopLeftPrint { get { return m_coverageTopLeftPrint; } set { m_coverageTopLeftPrint = value; } }
		private GeoCoord m_coverageBottomRightPrint;
		public GeoCoord CoverageBottomRightPrint { get { return m_coverageBottomRightPrint; } set { m_coverageBottomRightPrint = value; } }

		public int pictureWidth { get { return m_pictureManager.Width; } }
		public int pictureHeight { get { return m_pictureManager.Height; } }
		public int pictureWidthPrint { get { return m_pictureManager.WidthPrint; } }
		public int pictureHeightPrint { get { return m_pictureManager.HeightPrint; } }

		public double m_xScaleFactor = 1.0d;
		public double xScaleFactor	{ get { return m_xScaleFactor; } }	// how much to shrink the picture horizontally

		public double m_xScale = 1.0d;
		public double xScale { get { return m_xScale; } }   // degrees per pixel
		//public double xScale { get { return (m_coverageBottomRight.Lng - m_coverageTopLeft.Lng) * xScaleFactor / pictureWidth; } }    // degrees per pixel

		public double m_yScale = 1.0d;
		public double yScale { get { return m_yScale; } }   // degrees per pixel
		//public double yScale { get { return (m_coverageBottomRight.Lat - m_coverageTopLeft.Lat)	/ pictureHeight; } }    // degrees per pixel

		public double m_xScalePrint = 1.0d;
		public double xScalePrint { get { return m_xScalePrint; } } // degrees per pixel
		//public double xScalePrint { get { return (m_coverageBottomRightPrint.Lng - m_coverageTopLeftPrint.Lng) * xScaleFactor / pictureWidthPrint; } } // degrees per pixel

		public double m_yScalePrint = 1.0d;
		public double yScalePrint { get { return m_yScalePrint; } } // degrees per pixel
		//public double yScalePrint { get { return (m_coverageBottomRightPrint.Lat - m_coverageTopLeftPrint.Lat) / pictureHeightPrint; } } // degrees per pixel

		private Font ffont;

		// override default ITile implementation from Layer - so that all layers could use their getFrameRectangle():
		public override Rectangle getFrameRectangle()
		{
			return m_pictureManager.PictureBox.Bounds;
		}

		// override default ITile implementation from Layer - so that all layers could use their getFrameRectanglePrint():
		public override Rectangle getFrameRectanglePrint()
		{
			return m_pictureManager.BoundsPrint;
		}

		public CameraManager(ILocationWatcher lw, CameraTrack cameraTrack) : base(null, null)
		{
			m_locationWatcher = lw;
			m_cameraTrack = cameraTrack;
			This = this;

			ffont = Project.getLabelFont(Project.FONT_SIZE_REGULAR + 2);

			AdjustableArrowCap arrow = new AdjustableArrowCap(3.0f, 7.0f);
			Project.distancePen.CustomEndCap = arrow;
			Project.distancePen.CustomStartCap = arrow;

			AdjustableArrowCap arrowP = new AdjustableArrowCap(3.0f, 7.0f);
			Project.distancePenPrint.CustomEndCap = arrowP;
			Project.distancePenPrint.CustomStartCap = arrowP;

			AdjustableArrowCap arrowM = new AdjustableArrowCap(3.0f, 5.0f);
			Project.movePointPen.CustomEndCap = arrowM;
		}

		public void init(PictureManager pm)
		{
			m_pictureManager = pm;
			m_cameraManager = this;		// part of the Layer functionality

			if(Project.cameraElev > 0.0d) 
			{
				m_location = new GeoCoord(Project.cameraLng, Project.cameraLat, Project.cameraElev);
#if DEBUG
				LibSys.StatusBar.WriteLine("CameraManager:init() elev=" + Project.cameraElev + "   " + m_location);
#endif
				Project.cameraPositionsBackStack.Push(new CamPos(m_location.Lng, m_location.Lat, m_location.Elev, "", Project.drawTerraserverMode));
			} 
			else
			{
				//m_location = new GeoCoord(-121.0d, 39.0d, 500000.0d);
				//m_location = new GeoCoord(20.0d, 40.0d, 1020000.0d);			// Europe - Italy
				//m_location = new GeoCoord(20.0d, 35.0d, 1020000.0d);			// Mediterranian sea
				//m_location = new GeoCoord(140.0d, 40.0d, 3000000.0d);			// Japan y-level
				//m_location = new GeoCoord(179.88d, -17.5d, 1900000.0d);		// 180 
				//m_location = new GeoCoord(-117.06d, 32.74d, 310000.0d);		// San Diego x-level
				//m_location = new GeoCoord(-116.45d, 32.05d, 99000.0d);		// Mexico - edge of CA coverage a-level
				//m_location = new GeoCoord(-113.07d, 33.28d, 299000.0d);		// Phoenix - edge of CA coverage degree-level
				//m_location = new GeoCoord(-113.97d, 33.90d, 69000.0d);		// Phoenix - edge of CA coverage a-level
				//m_location = new GeoCoord(-110.05d, 37.05d, 299000.0d);		//
				//m_location = new GeoCoord(-102.05d, 40.05d, 299000.0d);		//
				//m_location = new GeoCoord(48.0d, 42.0d, 420000.0d);
				//m_location = new GeoCoord(-117.87d, 33.74d, 30000.0d);		// Santa Ana
				//m_location = new GeoCoord(-116.75d, 34.0d, 450000.0d);
				m_location = new GeoCoord(-66.1127516186667d, 18.4640733896667d, 1000.0d);	// Puerto Rico port

				Project.cameraLat = m_location.Lat;
				Project.cameraLng = m_location.Lng;
				Project.cameraElev = m_location.Elev;
			}
		}

		public override void shutdown()
		{
		}

		string m_formText = Project.PROGRAM_NAME_HUMAN + " - loading maps...";

		private void setMainFormText()
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

		private void _setMainFormText()
		{
			m_pictureManager.MainForm.Text = m_formText;
		}

		/*
		public void setCursorWait(bool enable)
		{
			if(Project.mainForm.InvokeRequired)
			{
				Project.mainForm.Invoke(enable ? new MethodInvoker(_setCursorWait) : new MethodInvoker(_setCursorDefault));
			} 
			else
			{
				if(enable)
				{
					_setCursorWait();
				}
				else 
				{
					_setCursorDefault();
				}
			}
		}

		private void _setCursorWait()
		{
			Cursor.Current = Cursors.WaitCursor;
		}

		private void _setCursorDefault()
		{
			Cursor.Current = Cursors.Default;
		}
		*/

		#region ProcessCameraMove()

		public void ProcessCameraMove()
		{
			Project.ClearPopup();
			_processCameraMove(false);
		}

		/// <summary>
		/// this is a special case when PreloadTiles is active (TileSetTerra:m_retilingSpecial)
		/// at this point it looks like ProcessCameraMove() would do just fine, because TileSetTerra:CameraMoved() knows
		/// not to call ReTile(-1) when m_retilingSpecial is on.
		/// </summary>
		private void ProcessCameraMoveSpecial()
		{
			_processCameraMove(true);
		}

		private void _processCameraMove(bool special)
		{
		    lock(this)
		    {
			Cursor.Current = Cursors.WaitCursor;	// doesn't really work here because Application.DoEvents() will be called and will cancel it
			//setCursorWait(true);
			m_cameraMoving = true;		// will be reset in LayersManager:t_cameraMoved
			
			//if(!special)
			{
				setMainFormText();
			}

			dragIsActive = false;
			m_markPoint		 = Point.Empty;
			prevInvRectangle = Rectangle.Empty;
			dragRectangle	 = Rectangle.Empty;
			startDragPoint	 = Point.Empty;
			currentDragPoint = Point.Empty;
			endDragPoint	 = Point.Empty;
			m_doDistance = false;
			startCoord = null;

			m_location.Normalize();

			calcCoverage();

			logCamtrackFrame();
			
			//if(!special)
			{
				// all layers CameraMoved() are called, and terra data becomes current.
				// terra layer adds attributes, landmarks and tile data to the frame.
				// waypoints layer adds displayed waypoints (including trackpoints).
				m_pictureManager.ProcessCameraMove();	// in track mode "frame" node should be in by this time.		
			}

			m_locationPrev = m_location.Clone();

			if(Project.cameraLat != m_location.Lat || Project.cameraLng != m_location.Lng || Project.cameraElev != m_location.Elev
					|| !m_drawTerraserverMode.Equals(Project.drawTerraserverMode))
			{
				Project.cameraLat = m_location.Lat;
				Project.cameraLng = m_location.Lng;
				Project.cameraElev = m_location.Elev;

				Project.cameraPositionsBackStack.Push(new CamPos(m_location.Lng, m_location.Lat, m_location.Elev, "", Project.drawTerraserverMode));
#if DEBUG
				LibSys.StatusBar.WriteLine("History Stack=" + Project.cameraPositionsBackStack.Count);
#endif
			}
			m_drawTerraserverMode = Project.drawTerraserverMode;

//			LibSys.StatusBar.Trace("calling watcher...");
			m_locationWatcher.LocationChangedCallback(m_location);
			if(m_locationWatcher2 != null)
			{
				m_locationWatcher2.LocationChangedCallback(m_location);
			}
//			LibSys.StatusBar.Trace("watcher done - " + m_location.ToString());
			LibSys.StatusBar.Trace2("" + m_location.ToString());
		    }
		}

		public void logCamtrackFrame()
		{
			if(Project.camTrackOn)
			{
				// track the camera movement for PDA:
				XmlDocument xmlDoc = m_cameraTrack.XmlDoc;
				
				XmlNode fNode = xmlDoc.CreateNode(XmlNodeType.Element, "frame", null);
				if(TileSetTerra.This.RetilingSpecial)
				{
					XmlAttribute aattr = xmlDoc.CreateAttribute("super");
					aattr.InnerText = "1";
					fNode.Attributes.Append(aattr);
				}

				XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, "camera", null);
				node.InnerText = "cam";
				XmlAttribute attr = xmlDoc.CreateAttribute("lon");
				attr.InnerText = "" + ((float)m_location.Lng);
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("lat");
				attr.InnerText = "" + ((float)m_location.Lat);
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("elev");
				attr.InnerText = "" + ((float)m_location.Elev);
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("topleft");
				attr.InnerText = "" + ((float)CoverageTopLeft.Lng) + "," + ((float)CoverageTopLeft.Lat);
				node.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("bottomright");
				attr.InnerText = "" + ((float)CoverageBottomRight.Lng) + "," + ((float)CoverageBottomRight.Lat);
				node.Attributes.Append(attr);
				fNode.AppendChild(node);
				int nFrames = m_cameraTrack.log(fNode);	// creates a "frame" node under the root
				LibSys.StatusBar.Trace("* camera track on - " + nFrames + " frames recorded");
				if((nFrames % 5) == 0)
				{
					Project.MessageBox(null, "Warning: PDA Camera Tracking is on, " + nFrames + " frames recorded.\n\nUse PDA menu to Stop or Cancel recording.");
				}
			}
		}
		#endregion

		#region Coverage, Pixel to Loc and other spatial reference calculations

		public double MetersPerPixel()
		{
			double metersPerPixel = m_location.Elev / 1000.0d;
			return metersPerPixel;
		}

		public void calcCoverage()
		{
			//LibSys.StatusBar.WriteLine("IP: calcCoverage() w=" + pictureWidth + "  h=" + pictureHeight);

			if(m_location.Elev > 6000000.0d && Math.Abs(m_location.Lat) > 70.0d)
			{
				m_xScaleFactor = 2.0d;
			}
			else if(m_location.Elev > 16000000.0d || Math.Abs(m_location.Lat) > 80.0d)
			{
				m_xScaleFactor = 1.0d;
			}
			else
			{
				// make sure we fall within the reasonable range:
				Project.distortionFactor = Math.Max(0.8d, Project.distortionFactor);
				Project.distortionFactor = Math.Min(1.2d, Project.distortionFactor);
				// factor is at 1.0 at equador and grows wildly near 90. It is about 1.3 at 40 and 2.0 at 60 degrees
				m_xScaleFactor = Project.distortionFactor / Math.Sin((90.0d - Math.Abs(m_location.Lat)) * Math.PI / 180.0d);	//1.0d;	// 1.3 produces squares at N37.68
				//m_xScaleFactor = 1.0d;
			}

			double metersPerPixel = m_location.Elev / 1000.0d;

			m_coverageXDegrees = pictureWidth * metersPerPixel * xScaleFactor / Distance.METERS_PER_DEGREE;
			m_coverageYDegrees = pictureHeight * metersPerPixel / Distance.METERS_PER_DEGREE;

			m_coverageTopLeft = new GeoCoord(m_location.Lng - m_coverageXDegrees / 2.0d,
												m_location.Lat + m_coverageYDegrees / 2.0d, 0.0d);
			m_coverageBottomRight = new GeoCoord(m_location.Lng + m_coverageXDegrees / 2.0d,
												m_location.Lat - m_coverageYDegrees / 2.0d, 0.0d);

			m_coverageTopLeft34 = new GeoCoord(m_location.Lng - m_coverageXDegrees * 3.0d / 8.0d,
				m_location.Lat + m_coverageYDegrees * 3.0d / 8.0d, 0.0d);
			m_coverageBottomRight34 = new GeoCoord(m_location.Lng + m_coverageXDegrees * 3.0d / 8.0d,
				m_location.Lat - m_coverageYDegrees * 3.0d / 8.0d, 0.0d);

			//LibSys.StatusBar.Trace("covX=" + m_coverageXDegrees + "  covY=" + m_coverageYDegrees
			//						+ " from " + m_coverageTopLeft.ToString() + " to " + m_coverageBottomRight.ToString());

			m_xScale = (m_coverageBottomRight.Lng - m_coverageTopLeft.Lng) / pictureWidth;
			m_yScale = (m_coverageBottomRight.Lat - m_coverageTopLeft.Lat)	/ pictureHeight;

		}

		public void calcCoveragePrint()
		{
			m_coverageXDegreesPrint = m_coverageXDegrees;
			m_coverageYDegreesPrint = m_coverageYDegrees;

			m_coverageTopLeftPrint = m_coverageTopLeft;
			m_coverageBottomRightPrint = m_coverageBottomRight;

			//LibSys.StatusBar.Trace("covXPrint=" + m_coverageXDegreesPrint + "  covYPrint=" + m_coverageYDegreesPrint
			//						+ " from " + m_coverageTopLeftPrint.ToString() + " to " + m_coverageBottomRightPrint.ToString());

			m_xScalePrint = (m_coverageBottomRightPrint.Lng - m_coverageTopLeftPrint.Lng) / pictureWidthPrint;
			m_yScalePrint = (m_coverageBottomRightPrint.Lat - m_coverageTopLeftPrint.Lat) / pictureHeightPrint;

			TileSetTerra.This.calcPrintRatios();
		}

		// a couple of quick comparisons help save time when loc is not on the screen
		public override bool insideScreenRectangle(GeoCoord loc)
		{
			double margin = (m_coverageBottomRight.Lng - m_coverageTopLeft.Lng) / 8.0d;

			return loc.Lng >= m_coverageTopLeft.Lng - margin && loc.Lng <= m_coverageBottomRight.Lng + margin
				&& loc.Lat <= m_coverageTopLeft.Lat + margin && loc.Lat >= m_coverageBottomRight.Lat - margin;
		}

		public override Point toPixelLocation(GeoCoord loc, ITile tile)
		{
			int xx = 0;
			int yy = 0;

			if(tile != null) 
			{
				// the idea here is to simplify conversion for tiles around 180, when camera operates 
				// on coordinates above 180 and below -180

				int offsetX = 0;
				int offsetY = 0;
				Point offset;

				if(!(offset=tile.getOffset()).IsEmpty) 
				{
					offsetX = offset.X;
					offsetY = offset.Y;
				}

				GeoCoord tileTopLeft = tile.getTopLeft();
				GeoCoord tileTopLeftN = tileTopLeft.Clone();
				tileTopLeftN.Normalize();
				GeoCoord locN = loc.Clone();
				locN.Normalize();
				double dOffsetLng = locN.Lng - tileTopLeftN.Lng;
				double dOffsetLat = locN.Lat - tileTopLeftN.Lat;

				xx = (int)Math.Round((tileTopLeft.Lng + dOffsetLng - m_coverageTopLeft.Lng) / xScale) + offsetX;
				yy = (int)Math.Round((tileTopLeft.Lat + dOffsetLat - m_coverageTopLeft.Lat) / yScale) + offsetY;

				return new Point(xx, yy);
			} 
			else 
			{
				// express location longitude and latitude in terms of camera coordinates:
				double dOffsetLng = loc.Lng - m_location.Lng;
				while(dOffsetLng >= 180.0d)
				{
					dOffsetLng -= 360.0d;
				}
				while(dOffsetLng <= -180.0d)
				{
					dOffsetLng += 360.0d;
				}

				double dOffsetLat = loc.Lat - m_location.Lat;

				if(TileSetTerra.This.hasRenderedTiles && !TileSetTerra.This.RetilingSpecial && TileSetTerra.This.insideCurrentZone(loc) && insideScreenRectangle(loc))
				{
					Point screenPt = TileSetTerra.This.toScreenPoint(loc, false);
					return screenPt;
				}
				else
				{
					xx = (int)Math.Round(dOffsetLng / xScale + pictureWidth / 2.0d);
					yy = (int)Math.Round((loc.Lat - m_location.Lat) / yScale + pictureHeight / 2.0d);
					return new Point(xx, yy);
				}
			}
		}

		public override Point toPixelLocationPrint(GeoCoord loc, ITile tile)
		{
			int xx = 0;
			int yy = 0;

			if(tile != null) 
			{
				// the idea here is to simplify conversion for tiles around 180, when camera operates 
				// on coordinates above 180 and below -180

				int offsetX = 0;
				int offsetY = 0;
				Point offset;

				if(!(offset=tile.getOffset()).IsEmpty) 
				{
					offsetX = offset.X;
					offsetY = offset.Y;
				}

				GeoCoord tileTopLeft = tile.getTopLeft();
				GeoCoord tileTopLeftN = tileTopLeft.Clone();
				tileTopLeftN.Normalize();
				GeoCoord locN = loc.Clone();
				locN.Normalize();
				double dOffsetLng = locN.Lng - tileTopLeftN.Lng;
				double dOffsetLat = locN.Lat - tileTopLeftN.Lat;

				xx = (int)Math.Round((tileTopLeft.Lng + dOffsetLng - m_coverageTopLeftPrint.Lng) / xScalePrint) + offsetX;
				yy = (int)Math.Round((tileTopLeft.Lat + dOffsetLat - m_coverageTopLeftPrint.Lat) / yScalePrint) + offsetY;
			} 
			else 
			{
				// express location longitude and latitude in terms of camera coordinates:
				double dOffsetLng = loc.Lng - m_location.Lng;
				while(dOffsetLng >= 180.0d)
				{
					dOffsetLng -= 360.0d;
				}
				while(dOffsetLng <= -180.0d)
				{
					dOffsetLng += 360.0d;
				}

				if(TileSetTerra.This.hasRenderedTiles && TileSetTerra.This.insideCurrentZone(loc))
				{
					Point screenPt = TileSetTerra.This.toScreenPoint(loc, true);
					screenPt.Offset(m_pictureManager.OffsetXPrint, m_pictureManager.OffsetYPrint);
					return screenPt;
				}
				else
				{
					xx = (int)Math.Round(dOffsetLng / xScalePrint + pictureWidthPrint / 2.0d);
					yy = (int)Math.Round((loc.Lat - m_location.Lat) / yScalePrint + pictureHeightPrint / 2.0d);
				}
			}
			//LibSys.StatusBar.Trace("toPixelLocation(" + loc + ")  x=" + xx + " y=" + yy);

			Point ret =  new Point(xx, yy);
			ret.Offset(m_pictureManager.OffsetXPrint, m_pictureManager.OffsetYPrint);
			return ret;
		}

		public override Point toPixelLocationNoNormalize(GeoCoord loc)
		{
			// express location longitude and latitude in terms of camera coordinates:
			int xx = (int)Math.Round((loc.Lng - m_location.Lng) / xScale + pictureWidth / 2.0d);
			int yy = (int)Math.Round((loc.Lat - m_location.Lat) / yScale + pictureHeight / 2.0d);

			//LibSys.StatusBar.Trace("toPixelLocationNoNormalize(" + loc + ")  x=" + xx + " y=" + yy);

			return new Point(xx, yy);
		}

		public override Point toPixelLocationNoNormalizePrint(GeoCoord loc)
		{
			// express location longitude and latitude in terms of camera coordinates:
			int xx = (int)Math.Round((loc.Lng - m_location.Lng) / xScalePrint + pictureWidthPrint / 2.0d);
			int yy = (int)Math.Round((loc.Lat - m_location.Lat) / yScalePrint + pictureHeightPrint / 2.0d);

			//LibSys.StatusBar.Trace("toPixelLocationNoNormalize(" + loc + ")  x=" + xx + " y=" + yy);

			Point ret =  new Point(xx, yy);
			ret.Offset(m_pictureManager.OffsetXPrint, m_pictureManager.OffsetYPrint);
			return ret;
		}

		public GeoCoord toGeoLocation(Point point)
		{
			double xx = m_coverageTopLeft.Lng + point.X * xScale;
			double yy = m_coverageTopLeft.Lat + point.Y * yScale;

			//LibSys.StatusBar.Trace("toGeoLocation(" + point + ")  lng=" + xx + " lat=" + yy);

			GeoCoord loc = new GeoCoord(xx, yy);

			// if we can, make more precise conversion using UTM:
			if(TileSetTerra.This.hasRenderedTiles && TileSetTerra.This.insideCurrentZone(loc))
			{
				loc = TileSetTerra.This.toGeoLocation(point, false);
			}

			return loc;
		}

		private GeoCoord toGeoLocationPrint(Point point)
		{
			double xx = m_coverageTopLeftPrint.Lng + point.X * xScalePrint;
			double yy = m_coverageTopLeftPrint.Lat + point.Y * yScalePrint;

			//LibSys.StatusBar.Trace("toGeoLocation(" + point + ")  lng=" + xx + " lat=" + yy);

			GeoCoord loc = new GeoCoord(xx, yy);

			// if we can, make more precise conversion using UTM:
			if(TileSetTerra.This.hasRenderedTiles && TileSetTerra.This.insideCurrentZone(loc))
			{
				loc = TileSetTerra.This.toGeoLocation(point, true);
			}

			return loc;
		}

		public double MetersPerPixelX()
		{
			return Math.Abs(xScale * Distance.METERS_PER_DEGREE * Math.Cos(m_location.Lat * Math.PI / 180.0d));
		}

		public double MetersPerPixelY()
		{
			return Math.Abs(yScale * Distance.METERS_PER_DEGREE);
		}

		public double MetersPerPixelXPrint()
		{
			return Math.Abs(xScalePrint * Distance.METERS_PER_DEGREE * Math.Cos(m_location.Lat * Math.PI / 180.0d));
		}

		public double MetersPerPixelYPrint()
		{
			return Math.Abs(yScalePrint * Distance.METERS_PER_DEGREE);
		}
		#endregion

		#region Moves, shifts and zooms

		public override void CameraMoved()
		{
			// we get ProcessCameraMove before all layers, so this one should stay empty
		}

		public override void PictureResized()
		{
			// we get ReactPictureResized before all layers, so this one should stay empty. See ReactPictureResized below.
		}

		DateTime lastResizeAttempt = DateTime.MinValue;
		private System.Windows.Forms.Timer resizeTimer = new System.Windows.Forms.Timer();
		private bool resizeTimerActive = false;
		private const int RESIZE_DELAY = 2; 

		public void resizeHandler(object obj, System.EventArgs args)
		{
			if((DateTime.Now - lastResizeAttempt).Seconds >= RESIZE_DELAY)
			{
#if DEBUG
				LibSys.StatusBar.Trace("finished resizing");
#endif
				resizeTimer.Tick -= new EventHandler(resizeHandler);
				resizeTimerActive = false;
				ProcessCameraMove();
			}
			else
			{
#if DEBUG
				LibSys.StatusBar.Trace("waiting to complete resize");
#endif
				resizeTimer.Enabled = true;		// wait another 2 seconds
			}
		}

		public void ReactPictureResized()
		{
			if(!resizeTimerActive)
			{
#if DEBUG
				LibSys.StatusBar.Trace("activating resize timer");
#endif
				resizeTimer.Interval = 1000;
				resizeTimer.Tick += new EventHandler(resizeHandler);
				resizeTimer.Start();
				resizeTimerActive = true;
			}
			SpoilPicture();
			lastResizeAttempt = DateTime.Now;
		}

		public void shiftLeftSpecial()
		{
			Location.Lng -= m_pictureManager.ShiftValueH;
			ProcessCameraMoveSpecial();
		}

		public void shiftLeft()
		{
			Location.Lng -= m_pictureManager.ShiftValueH;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void shiftLeftNoSpoil()
		{
			Location.Lng -= m_pictureManager.ShiftValueH;
			SpoilLocationPan(Location, 2);
			ProcessCameraMove();
		}

		public void shiftLeftSmall()
		{
			Location.Lng -= m_pictureManager.ShiftValueHSmall;
			SpoilLocationPan(Location, 2);
			ProcessCameraMove();
		}

		public void shiftRightSpecial()
		{
			Location.Lng += m_pictureManager.ShiftValueH;
			ProcessCameraMoveSpecial();
		}

		public void shiftRight()
		{
			Location.Lng += m_pictureManager.ShiftValueH;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void shiftRightNoSpoil()
		{
			Location.Lng += m_pictureManager.ShiftValueH;
			SpoilLocationPan(Location, 3);
			ProcessCameraMove();
		}

		public void shiftRightSmall()
		{
			Location.Lng += m_pictureManager.ShiftValueHSmall;
			SpoilLocationPan(Location, 3);
			ProcessCameraMove();
		}

		public void shiftUpLeftSpecial()
		{
			Location.Lng -= m_pictureManager.ShiftValueH;
			Location.Lat += m_pictureManager.ShiftValueV;
			ProcessCameraMoveSpecial();
		}

		public void shiftUpLeft()
		{
			Location.Lng -= m_pictureManager.ShiftValueH;
			Location.Lat += m_pictureManager.ShiftValueV;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void shiftUpRightSpecial()
		{
			Location.Lng += m_pictureManager.ShiftValueH;
			Location.Lat += m_pictureManager.ShiftValueV;
			ProcessCameraMoveSpecial();
		}

		public void shiftUpRight()
		{
			Location.Lng += m_pictureManager.ShiftValueH;
			Location.Lat += m_pictureManager.ShiftValueV;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void shiftUpSpecial()
		{
			Location.Lat += m_pictureManager.ShiftValueV;
			ProcessCameraMoveSpecial();
		}

		public void shiftUp()
		{
			Location.Lat += m_pictureManager.ShiftValueV;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void shiftUpNoSpoil()
		{
			Location.Lat += m_pictureManager.ShiftValueV;
			SpoilLocationPan(Location, 0);
			ProcessCameraMove();
		}

		public void shiftUpSmall()
		{
			Location.Lat += m_pictureManager.ShiftValueVSmall;
			SpoilLocationPan(Location, 0);
			ProcessCameraMove();
		}

		public void shiftDownSpecial()
		{
			Location.Lat -= m_pictureManager.ShiftValueV;
			ProcessCameraMoveSpecial();
		}

		public void shiftDown()
		{
			Location.Lat -= m_pictureManager.ShiftValueV;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void shiftDownNoSpoil()
		{
			Location.Lat -= m_pictureManager.ShiftValueV;
			SpoilLocationPan(Location, 1);
			ProcessCameraMove();
		}

		public void shiftDownSmall()
		{
			Location.Lat -= m_pictureManager.ShiftValueVSmall;
			SpoilLocationPan(Location, 1);
			ProcessCameraMove();
		}

		public void shiftDownLeftSpecial()
		{
			Location.Lng -= m_pictureManager.ShiftValueH;
			Location.Lat -= m_pictureManager.ShiftValueV;
			ProcessCameraMoveSpecial();
		}

		public void shiftDownLeft()
		{
			Location.Lng -= m_pictureManager.ShiftValueH;
			Location.Lat -= m_pictureManager.ShiftValueV;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void shiftDownRightSpecial()
		{
			Location.Lng += m_pictureManager.ShiftValueH;
			Location.Lat -= m_pictureManager.ShiftValueV;
			ProcessCameraMoveSpecial();
		}

		public void shiftDownRight()
		{
			Location.Lng += m_pictureManager.ShiftValueH;
			Location.Lat -= m_pictureManager.ShiftValueV;
			SpoilPicture(Location);
			ProcessCameraMove();
		}

		public void zoomIn()
		{
			//LibSys.StatusBar.Trace("zoomIn()");
			if(this.Location.Elev > Project.cameraHeightMin * 1000.0d + 10.0d)
			{
				SpoilLocationZoom(Location, 0);
			}
			else
			{
				LibSys.StatusBar.Trace("*Max zoom reached. Use Map-->Options menu if you want more");
			}
			Project.mainCommand.zoomIn();

			/*
			if(Location.Elev > 20000.0d) 
			{
				Location.Elev -= 10000.0d;
				SpoilPicture(mousePosition);
				ProcessCameraMove();
			}
			else if(Location.Elev > Project.cameraHeightMin * 1000.0d)
			{
				Location.Elev -= ((Location.Elev - Project.cameraHeightMin * 1000.0d) / 2.0d);
				SpoilPicture(mousePosition);
				ProcessCameraMove();
			}
			*/
		}

		public void zoomOut()
		{
			//LibSys.StatusBar.Trace("zoomOut()");
			SpoilLocationZoom(Location, 1);
			Project.mainCommand.zoomOut();

			/*
			SpoilPicture(mousePosition);
			Location.Elev += 10000.0d;
			ProcessCameraMove();
			*/
		}

		public void zoomInLarge()
		{
			//LibSys.StatusBar.Trace("zoomInLarge()");
			if(this.Location.Elev > Project.cameraHeightMin * 1000.0d + 10.0d)
			{
				SpoilLocationZoom(Location, 0);
			}
			else
			{
				LibSys.StatusBar.Trace("*Max zoom reached. Use Map-->Options menu if you want more");
			}
			Project.mainCommand.zoomInLarge();

			/*
			if(Location.Elev > 120000.0d) 
			{
				SpoilPicture(mousePosition);
				Location.Elev -= 100000.0d;
				ProcessCameraMove();
			} 
			else
			{
				zoomIn();
			}
			*/
		}

		public void zoomOutLarge()
		{
			//LibSys.StatusBar.Trace("zoomOutLarge()");
			SpoilLocationZoom(Location, 1);
			Project.mainCommand.zoomOutLarge();

			/*
			SpoilPicture(mousePosition);
			Location.Elev += 100000.0d;
			ProcessCameraMove();
			*/
		}

		public void moveToLocation(GeoCoord location)
		{
			//LibSys.StatusBar.Trace("moveToLocation()  loc=" + location);

			SpoilPicture(mousePosition);
			Location.Lat = location.Lat;
			Location.Lng = location.Lng;
			// elevation don't change
			ProcessCameraMove();
		}

		public void zoomToDragRectangle()
		{
			//LibSys.StatusBar.Trace("zoomToDragRectangle()");

			SpoilPicture(mousePosition);

			double ratioX = pictureWidth / ((double)dragRectangle.Width);
			double ratioY = pictureHeight / ((double)dragRectangle.Height);
			double ratio  = Math.Min(ratioX, ratioY);
			double elev   = Math.Max(Location.Elev / ratio, Project.cameraHeightMin * 1000.0d);
			//LibSys.StatusBar.Trace("ratioX=" + ratioX + " ratioY=" + ratioY + " ratio=" + ratio + " elev=" + elev);

			Point centerPoint = new Point(dragRectangle.X + dragRectangle.Width / 2,
											dragRectangle.Y + dragRectangle.Height / 2);
			GeoCoord location = toGeoLocation(centerPoint);
			location.Elev = elev;
			Location = location;	// set() calls ProcessCameraMove()
		}	

		public void googleEarthShowDragRectangle()
		{
			//LibSys.StatusBar.Trace("googleEarthShowDragRectangle()");

			double ratioX = pictureWidth / ((double)dragRectangle.Width);
			double ratioY = pictureHeight / ((double)dragRectangle.Height);
			double ratio  = Math.Min(ratioX, ratioY);
			double elev   = Math.Max(Location.Elev / ratio, Project.cameraHeightMin * 1000.0d);
			//LibSys.StatusBar.Trace("ratioX=" + ratioX + " ratioY=" + ratioY + " ratio=" + ratio + " elev=" + elev);

			Point centerPoint = new Point(dragRectangle.X + dragRectangle.Width / 2,
											dragRectangle.Y + dragRectangle.Height / 2);
			GeoCoord location = toGeoLocation(centerPoint);

			ArrayList earthquakesInDragRectangle = new ArrayList();
			foreach(Earthquake eq in EarthquakesCache.EarthquakesDisplayed)
			{
				if(eq.boundingRect().IntersectsWith(dragRectangle))
				{
					earthquakesInDragRectangle.Add(eq);
				}
			}

			ArrayList waypointsInDragRectangle = new ArrayList();
			foreach(Waypoint wpt in WaypointsCache.WaypointsDisplayed)
			{
				if(wpt.TrackId == -1 && wpt.boundingRect().IntersectsWith(dragRectangle))
				{
					waypointsInDragRectangle.Add(wpt);
				}
			}

			ArrayList tracksInDragRectangle = new ArrayList();
			foreach(Track trk in WaypointsCache.TracksAll)
			{
				Point topLeft = this.toPixelLocation(trk.TopLeft, null);
				Point bottomRight = this.toPixelLocation(trk.BottomRight, null);
				Rectangle boundingRect = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

				if(boundingRect.IntersectsWith(dragRectangle))
				{
					tracksInDragRectangle.Add(trk);
				}
			}

			GoogleEarthManager.runAreaWithObjects(location, elev, waypointsInDragRectangle, earthquakesInDragRectangle, tracksInDragRectangle);

//			GoogleEarthManager.runAtPoint(location, elev, "selected area - around " + location, null);
		}	

		/// <summary>
		/// a shorthand used to zoom after reading files or devices; returns true if actually had to move camera
		/// make sure WaypointsCache.resetBoundaries(); is called before the reading process.
		/// </summary>
		/// <returns></returns>
		public bool zoomToCorners()
		{
			return zoomToCorners(WaypointsCache.TopLeft, WaypointsCache.BottomRight);
		}
	
		// returns true if actually had to move camera
		public bool zoomToCorners(GeoCoord topLeft, GeoCoord bottomRight)
		{
			if(topLeft.Lng < bottomRight.Lng && topLeft.Lat > bottomRight.Lat)	// make sure we are dealing with real corners
			{
				double ratioX = pictureWidth / 1600.0d;
				double ratioY = pictureHeight / 1200.0d;
				double ratio  = Math.Min(ratioX, ratioY);		// adjust to size less than 1600x1200

				double elev1 = Math.Abs((bottomRight.Lng - topLeft.Lng) * Distance.METERS_PER_DEGREE) / ratioX;
				double elev2 = Math.Abs((topLeft.Lat - bottomRight.Lat) * Distance.METERS_PER_DEGREE) / ratioY;

				GeoCoord location = new GeoCoord((topLeft.Lng + bottomRight.Lng)/2, (topLeft.Lat + bottomRight.Lat)/2);
				location.Elev = Math.Max(Project.cameraHeightMin * 1000.0d, Math.Max(elev1, elev2));	// no closer than 400m
				Location = location;	// set() calls ProcessCameraMove()
				return true;
			}
			else if(topLeft.Lng == bottomRight.Lng && topLeft.Lat == bottomRight.Lat && WaypointsCache.boundariesWereMoved())
			{
				GeoCoord location = new GeoCoord(topLeft.Lng, topLeft.Lat);
				location.Elev = Location.Elev;
				Location = location;	// set() calls ProcessCameraMove()
				return true;
			}
			else
			{
				this.ProcessCameraMove();
				return false;
			}
		}

		public bool isInView(GeoCoord location)
		{
			// see if the location goes outside the 3/4th of the screen:
			if(location.Lat > m_coverageTopLeft.Lat || location.Lat < m_coverageBottomRight.Lat
				|| location.Lng < m_coverageTopLeft.Lng || location.Lng > m_coverageBottomRight.Lng)
			{
				return false;
			}
			return true;
		}

		public bool isIn34View(GeoCoord location)
		{
			// see if the location goes outside the 3/4th of the screen:
			if(location.Lat > m_coverageTopLeft34.Lat || location.Lat < m_coverageBottomRight34.Lat
				|| location.Lng < m_coverageTopLeft34.Lng || location.Lng > m_coverageBottomRight34.Lng)
			{
				return false;
			}
			return true;
		}

		public void keepInView(GeoCoord location)
		{
			// see if the location goes outside the 3/4th of the screen, and re-center camera if it does:
			if(!isIn34View(location))
			{
				// shall we change view if moves happen too often? too seldom?
				double elev = this.Elev;
				Location = new GeoCoord(location.Lng, location.Lat, elev);	// set() calls ProcessCameraMove()
			}
		}
		#endregion

		#region mouse clicks processing

		private Point mousePosition;	// updated on mouse move
		private Point movePoint;
		private bool dragIsActive = false;	// true while mouse is down
		private Point startDragPoint;
		private GeoCoord startCoord = null;
		private Point currentDragPoint;
		private Point endDragPoint;
		private Rectangle dragRectangle;
		private Rectangle prevDragRectangle;	// to protect from start drag in Click
		private Rectangle invRectangle;
		private Rectangle prevInvRectangle;
		private MouseButtons btnDown = MouseButtons.None;

		public bool pointInsideDragRectangle(Point point)
		{
			return !dragRectangle.IsEmpty && dragRectangle.Contains(point);
		} 

		public void resetDrag(bool doRedraw)
		{
			//LibSys.StatusBar.Trace("resetDrag(): " + dragRectangle);

			dragIsActive = false;
			startDragPoint = Point.Empty;
			currentDragPoint = Point.Empty;
			endDragPoint = Point.Empty;
			m_doDistance = false;
			startCoord = null;
			if(dragRectangle != Rectangle.Empty)
			{
				if(doRedraw)
					m_pictureManager.Invalidate(dragRectangle);
				dragRectangle = Rectangle.Empty;
			}
			prevDragRectangle = Rectangle.Empty;
			if(invRectangle != Rectangle.Empty)
			{
				if(doRedraw)
					m_pictureManager.Invalidate(invRectangle);
				invRectangle = Rectangle.Empty;
			}
			if(prevInvRectangle != Rectangle.Empty)
			{
				if(doRedraw)
					m_pictureManager.Invalidate(prevInvRectangle);
				prevInvRectangle = Rectangle.Empty;
			}
		}


		public bool pictureClick(System.EventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			//LibSys.StatusBar.Trace(">>>>  pictureMouseClick()");
			// check if the click falls into corner arrows or side arrows:
			int x = mousePosition.X;
			int y = mousePosition.Y;
			int w = pictureWidth;
			int h = pictureHeight;
			int w1 = w/2 - m_arrowWidth;
			int w2 = w/2 + m_arrowWidth;
			int h1 = h/2 - m_arrowHeight;
			int h2 = h/2 + m_arrowHeight;

			// is the click within the arrows?
			if(x < m_arrowWidth)
			{
				if(y < m_arrowHeight)
				{
					shiftUpLeft();
					return true;
				}
				else if(y > h1 && y < h2)
				{
					shiftLeft();
					return true;
				}
				else if(y > h - m_arrowHeight)
				{
					shiftDownLeft();
					return true;
				}
			}
			else if(x > w1 && x < w2)
			{
				if( y < m_arrowHeight)
				{
					shiftUp();
					return true;
				}
				else if(y > h - m_arrowHeight)
				{
					this.shiftDown();
					return true;
				}
			}
			else if(x > w - m_arrowWidth)
			{
				if(y > h - m_arrowHeight)
				{
					shiftDownRight();
					return true;
				}
				else if(y > h1 && y < h2)
				{
					shiftRight();
					return true;
				}
				else if(y < m_arrowHeight)
				{
					shiftUpRight();
					return true;
				}
			}

			// also called before pictureDoubleClick()
			//GeoCoord location = toGeoLocation(mousePosition);
			//LibSys.StatusBar.Trace(">>>>  pictureClick(" + e + ") at " + mousePosition + " = " + location + " dragRect=" + dragRectangle);

			//if(btnDown == MouseButtons.Left && mousePosition.Equals(startDragPoint))
			//{
				// sometimes (rarely) Click is preceded by MouseMove - in which drag is started.
				// we keep a copy of old drag rectangle to reverse effect of such false drag.
				//dragRectangle = prevDragRectangle;
			//}

			if(!m_doDistance && !movePointMode && !moveLegMode)
			{
				if(pointInsideDragRectangle(mousePosition)) 
				{
					// clicking into drag rectangle causes zoom in it;
					// you can also drag holding CONTROL 
					Rectangle narrowDR = Rectangle.Inflate(dragRectangle, -2, -2);
					if(narrowDR.Contains(mousePosition))
					{
						if(Project.pdaWellSelectingMode)
						{
							pdaExportDragRectangle();
							return true;
						}
						else
						{
							zoomToDragRectangle();
							return true;
						}
					}
				}
				else if(!dragRectangle.IsEmpty)
				{
					// click comes on a laptop when you take finger off the touchpad.
					// make sure we don't waste a good drag rectangle on such false click.
					int extraSpace = 20;
					Rectangle wideDR = new Rectangle(dragRectangle.X - extraSpace, dragRectangle.Y - extraSpace,
						dragRectangle.Width + extraSpace*2, dragRectangle.Height + extraSpace*2);
					if(!wideDR.Contains(mousePosition))
					{
						//LibSys.StatusBar.Trace("pictureClick: reset drag");
						resetDrag(false);
					}
				}
			}
			return false;
		}

		public void pictureDoubleClick(System.EventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			GeoCoord location = toGeoLocation(mousePosition);
			//LibSys.StatusBar.Trace(">>>>  pictureDoubleClick(" + e + ") at " + mousePosition + " = " + location);
			// double-click just moves center point (camera) there:
			moveToLocation(location);
		}

		public void pictureMouseDown(System.Windows.Forms.MouseEventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			this.mousePosition = new Point(e.X, e.Y);
			this.movePoint = new Point(e.X, e.Y);
			// ignore it if we expect a click into the drag rectangle, or it is outside the map on the world scale:
			GeoCoord mouseGeoLocation = toGeoLocation(movePoint);
			double lat = mouseGeoLocation.Lat;
			if(lat < 90.0d && lat > -90.0d)
			{
				if(m_doDistance || movePointMode || moveLegMode)
				{
					resetDrag(true);
				}
				if(pointInsideDragRectangle(mousePosition)) 
				{
					// clicking into drag rectangle causes zoom in it;
					Rectangle narrowDR = Rectangle.Inflate(dragRectangle, -2, -2);
					if(narrowDR.Contains(mousePosition))
					{
						return;	// will be processed in mouseClick()
					}
				} 
				this.btnDown = e.Button;
				if(btnDown == MouseButtons.Left && !moveLegMode)	// start drag
				{
					//LibSys.StatusBar.Trace("pictureMouseDown: reset drag");
					resetDrag(true);
					startDragPoint = movePoint;
					dragIsActive = true;
					m_pdaWellDiagonale = Project.pdaWellDiagonale;

					//LibSys.StatusBar.Trace("start drag: " + startDragPoint);
				}
				// the drag will be reset if pictureMouseUp() comes at very close point
			}
		}

		public void pictureMouseUp(System.Windows.Forms.MouseEventArgs e, bool controlDown, bool shiftDown, bool altDown)
		{
			this.mousePosition = new Point(e.X, e.Y);
			this.movePoint = new Point(e.X, e.Y);
			this.btnDown = MouseButtons.None;

			if(dragIsActive)
			{
				if(movePointMode || moveLegMode)
				{
					resetDrag(false);
					dragIsActive = false;
					return;
				}

				// see if the up point is real close to the down point, and reset drag if it is:
				int distance = (startDragPoint.X - e.X)*(startDragPoint.X - e.X) + (startDragPoint.Y - e.Y)*(startDragPoint.Y - e.Y);
				if(distance < 64)	// too close, it is just a click
				{
					//LibSys.StatusBar.Trace("pictureMouseUp: reset drag");
					resetDrag(false);

						//					if(shiftDown) 
						//					{
						//						// holding SHIFT down and clicking into picture causes zoom out:
						//						zoomOut();
						//					}
						//					else if(altDown) 
						//					{
						//						// holding ALT down and clicking into picture calls IE with Microsoft Terraserver page:
						//						GeoCoord location = toGeoLocation(mousePosition);
						//						Project.terraserverMap(location.Lng, location.Lat);
						//						this.MarkLocation(location, 1);
						//					}
						//					else

					if(controlDown) 
					{
						// holding Ctrl down and clicking into picture calls IE with MapQuest Street Map page:
						GeoCoord location = toGeoLocation(mousePosition);
						Project.streetMap(location.Lng, location.Lat, m_cameraManager.Location.Elev);
						this.MarkLocation(location, 1);
					}
					else if(altDown) 
					{
						GeoCoord location = toGeoLocation(mousePosition);
						FileKml.runGoogleEarthAtPoint(new CamPos(location.Lng, location.Lat, m_cameraManager.Location.Elev), "-", "QuakeMap click point");
						this.MarkLocation(location, 1);
					}
					else
					{
						//removeMarkLocation();
					}
				}
				dragIsActive = false;
			}
		}

		public void pictureMouseMove(System.Windows.Forms.MouseEventArgs e,
										bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				if(btnDown == MouseButtons.Left && e.Button == MouseButtons.None)
				{
					// when a help popup is up, MouseUp event is skipped. Make up for it:
					//simulatedMouseUp(e, controlDown, shiftDown, altDown);
				}
				else if(btnDown == MouseButtons.None && e.Button == MouseButtons.Left)
				{
					// when a help popup is up, MouseDown event is skipped. Make up for it:
					//simulatedMouseDown(e, controlDown, shiftDown, altDown);
				}
				else
				{
					this.btnDown = e.Button;
				}
				movePoint = new Point(e.X, e.Y);
				GeoCoord mouseGeoLocation = toGeoLocation(movePoint);
				double lat = mouseGeoLocation.Lat;
				Distance toCamera = mouseGeoLocation.distanceFrom(m_location);
				mouseGeoLocation.Normalize();
				// when in UTM mode, display distance in meters:
				string sToCamera = (TileSetTerra.This.hasRenderedTiles && Project.coordStyle == 3) ? toCamera.ToString(Distance.UNITS_DISTANCE_KM) : toCamera.ToString();
				LibSys.StatusBar.Trace2("" + mouseGeoLocation.ToString() + " / " + sToCamera);
				/*
				mouseGeoLocation.Elev = Location.Elev;
				if(m_location.Elev < 3000.0d)
				{
					LibSys.StatusBar.Trace2("" + mouseGeoLocation.ToString5());
				}
				else
				{
					LibSys.StatusBar.Trace2("" + mouseGeoLocation.toString3());
				}
				*/

				if(dragIsActive) // has drag rectangle, and mouse is down
				{
					int iDistance = (startDragPoint.X - e.X)*(startDragPoint.X - e.X) + (startDragPoint.Y - e.Y)*(startDragPoint.Y - e.Y);
					if(iDistance > 64)	// if too close, wait... otherwise start the dragging
					{
						Project.isDragging = true;
						if(btnDown == MouseButtons.Left && lat < 90.0d && lat > -90.0d) // dragging - draw rubber band
						{
							currentDragPoint = movePoint;
							calculateDragRectangle(controlDown);
						}
					}
				}
				else
				{
					Project.isDragging = false;
				}
			} 
			catch {}  // may be null exception when moving mouse before first ProcessCameraMove()
		}

		private void calculateDragRectangle(bool doDistance)
		{
			int MIN_RECT_SIZE = 20;
			int topLeftX = Math.Min(startDragPoint.X, currentDragPoint.X);
			int topLeftY = Math.Min(startDragPoint.Y, currentDragPoint.Y);
			int w = Math.Max(MIN_RECT_SIZE, Math.Abs(currentDragPoint.X - startDragPoint.X));
			int h = Math.Max(MIN_RECT_SIZE, Math.Abs(currentDragPoint.Y - startDragPoint.Y));
			/*
			int topLeftX = Math.Min(startDragPoint.X, currentDragPoint.X);
			int topLeftY = Math.Min(startDragPoint.Y, currentDragPoint.Y);
			int w = Math.Abs(currentDragPoint.X - startDragPoint.X);
			int h = Math.Abs(currentDragPoint.Y - startDragPoint.Y);
			*/

			// make sure the rectangle diagonale for PDA does not exceed 4 miles (m_pdaWellDiagonale):
			if(Project.pdaWellSelectingMode)
			{
				if(startCoord == null)
				{
					startCoord = this.toGeoLocation(startDragPoint);
				}
				GeoCoord end = this.toGeoLocation(currentDragPoint);
				Distance d = end.distanceFrom(startCoord);

				if(Project.pdaWizardStepControl != null)
				{
					Project.pdaWizardStepControl.callback(0, d.ToString());
				}

				if(d.Meters > m_pdaWellDiagonale)
				{
					//return;		// too big, leave the rectangle as is.
					// make a smaller rectangle, pointing to the mouse point:

					double ratio = m_pdaWellDiagonale / d.Meters;

					w = (int)(w * ratio);
					h = (int)(h * ratio);

					if(startDragPoint.X > currentDragPoint.X)
					{
						topLeftX = startDragPoint.X - w;
					}
					if(startDragPoint.Y > currentDragPoint.Y)
					{
						topLeftY = startDragPoint.Y - h;
					}
				}
			}

			dragRectangle = new Rectangle(topLeftX, topLeftY, w-1, h-1);
			if(m_doDistance) // || movePointMode || moveLegMode)
			{
				int minInvSize = 50;
				if(w < minInvSize || h < minInvSize)
				{
					invRectangle = new Rectangle(topLeftX - minInvSize / 2 - 10, topLeftY - minInvSize / 2 - 10, w + minInvSize + 20, h + minInvSize + 20);
				}
				else
				{
					invRectangle = new Rectangle(topLeftX - 10, topLeftY - 10, w + 20, h + 20);
				}
			}
			else
			{
				invRectangle = new Rectangle(topLeftX, topLeftY, w, h);
			}
			if(prevInvRectangle != Rectangle.Empty)
			{
				m_pictureManager.Invalidate(prevInvRectangle);
			}
			else
			{
				m_doDistance = doDistance;
			}
			m_pictureManager.Invalidate(invRectangle);
			prevInvRectangle = invRectangle;
		}
		#endregion

		#region PDA map wizard related

		private double m_pdaWellDiagonale = 200.0d;	// meters - will be set when drag starts.

		public void pdaExportDragRectangle()
		{
			GeoCoord topLeft = toGeoLocation(new Point(dragRectangle.X, dragRectangle.Y));
			GeoCoord bottomRight = toGeoLocation(new Point(dragRectangle.X + dragRectangle.Width, dragRectangle.Y + dragRectangle.Height));

			// try to get the high-level map to reasonable extent:
			GeoCoord coverageTopLeft = new GeoCoord(m_coverageTopLeft);
			GeoCoord coverageBottomRight = new GeoCoord(m_coverageBottomRight);

			// to paint red rectangle indicating high-level coverage:
			double marginLat = (m_coverageTopLeft.Lat - m_coverageBottomRight.Lat) / 100.0d;
			double marginLng = (m_coverageBottomRight.Lng - m_coverageTopLeft.Lng) / 140.0d;

			Distance screenDiag = coverageTopLeft.distanceFrom(coverageBottomRight);

			this.resetDrag(true);
			Application.DoEvents();

			Graphics graphics = PictureManager.This.Graphics;
			if(screenDiag.Meters > 20000.0d)
			{
				// camera too far to preload all, get area around the well covered: 
				coverageTopLeft.Lat = Math.Min(m_coverageTopLeft.Lat - marginLat, (topLeft.Lat + bottomRight.Lat) / 2.0d + 0.1d);
				coverageTopLeft.Lng = Math.Max(m_coverageTopLeft.Lng + marginLng, (topLeft.Lng + bottomRight.Lng) / 2.0d - 0.14d);

				coverageBottomRight.Lat = Math.Max(m_coverageBottomRight.Lat + marginLat, (topLeft.Lat + bottomRight.Lat) / 2.0d - 0.1d);
				coverageBottomRight.Lng = Math.Min(m_coverageBottomRight.Lng - marginLng, (topLeft.Lng + bottomRight.Lng) / 2.0d + 0.14d);
			}
			else
			{
				// just highlight the whole screen, reminding that the area will be saved:
				coverageTopLeft.Lat = m_coverageTopLeft.Lat - marginLat;
				coverageTopLeft.Lng = m_coverageTopLeft.Lng + marginLng;

				coverageBottomRight.Lat = m_coverageBottomRight.Lat + marginLat;
				coverageBottomRight.Lng = m_coverageBottomRight.Lng - marginLng;
			}
			this.PaintGeoRect(coverageTopLeft, coverageBottomRight, graphics, Pens.Red, "loading 16-64m");
			this.PaintGeoRect(topLeft, bottomRight, graphics, Pens.Yellow, "loading 1-64m");

			// the operations below will add to TerraserverCache.TileNamesCollection:

			TileSetTerraLayout.downloadPdaThemesAtLevel(coverageTopLeft, coverageBottomRight, 14, true);		// scales 16, 32, 64

			// now get the well coverage:
			TileSetTerraLayout.downloadPdaThemesAtLevel(topLeft, bottomRight, 10, true);

			Application.DoEvents();

			if(m_pdaMonitorThread == null)
			{
				m_pdaMonitorThread = new Thread( new ThreadStart(monitorPdaExportProgress));
				// see Entry.cs for how the current culture is set:
				m_pdaMonitorThread.CurrentCulture = Thread.CurrentThread.CurrentCulture; //new CultureInfo("en-US", false);
				m_pdaMonitorThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture; //new CultureInfo("en-US", false);
				m_pdaMonitorThread.IsBackground = true;	// terminate with the main process
				m_pdaMonitorThread.Name = "Monitoring PDA download";
				m_pdaMonitorThread.Start();
			}
		}

		private static Thread m_pdaMonitorThread = null;

		private void monitorPdaExportProgress()
		{
			while(Project.pdaWizardStepControl != null && TerraserverCache.TileNamesCollection != null)
			{
				try
				{
					int toExport = TerraserverCache.TileNamesCollection.Count;
					int queuedToLoad = TerraserverCache.tilesBeingLoadedCount;
					if(queuedToLoad == 0)
					{
						string msg = toExport > 0 ? String.Format("{0} tiles ready for export.\r\n", toExport) : "";
						Project.pdaWizardStepControl.callback(0, msg + "Drag the mouse to select another zoom area, or press Next>> below");
					}
					else
					{
						int countComplete = toExport - queuedToLoad;
						int percentComplete = Math.Min(100, countComplete * 100 / toExport);	//100 - (queuedToLoad % 100);
						Project.pdaWizardStepControl.callback(percentComplete, "Please wait: downloading tiles...   " + queuedToLoad + " to load, " + toExport + " total coverage.");
					}
				}
				catch {}
				Thread.Sleep(300);
			}
			m_pdaMonitorThread = null;
			// and exit, terminating the thread.
		}

		#endregion

		#region Paint related

		/// <summary>
		/// for quick non-persistent painting of geo rects
		/// </summary>
		/// <param name="topLeft"></param>
		/// <param name="bottomRight"></param>
		/// <param name="g"></param>
		public void PaintGeoRect(GeoCoord topLeft, GeoCoord bottomRight, Graphics g, Pen pen, string message)
		{
			Point tl = this.toPixelLocation(topLeft, null);
			Point br = this.toPixelLocation(bottomRight, null);

			int width = br.X - tl.X;
			int height = br.Y - tl.Y;
			g.DrawRectangle(pen, tl.X, tl.Y, width, height);

			if(message != null)
			{
				Font font = Project.getLabelFont(Project.FONT_SIZE_REGULAR + 2);
				g.DrawString(message, font, Project.yellowBrush, tl.X+5, tl.Y+10);
			}
		}

		/// <summary>
		/// for quick non-persistent highlighting of geo rects, likely with a transparent brush
		/// </summary>
		/// <param name="topLeft"></param>
		/// <param name="bottomRight"></param>
		/// <param name="g"></param>
		public void HighlightGeoRect(GeoCoord topLeft, GeoCoord bottomRight, Graphics g, Brush brush)
		{
			Point tl = this.toPixelLocation(topLeft, null);
			Point br = this.toPixelLocation(bottomRight, null);

			int width = br.X - tl.X;
			int height = br.Y - tl.Y;

			g.FillRectangle(brush, tl.X, tl.Y, width, height);
		}

		private string sDist = "";

		public override void Paint(object sender, PaintEventArgs e)
		{
			try 
			{
				base.Paint(sender, e);
				if(Project.spoiling)
				{
					//Rectangle allBox = new Rectangle(0, 0, pictureWidth, pictureHeight);
					//e.Graphics.FillRectangle(Project.spoilBrush, allBox);
					if(spoilPoint.IsEmpty) 
					{
						spoilPoint = toPixelLocation(Location, null);
					}
					int x = spoilPoint.X;
					int y = spoilPoint.Y;

					for (int i=20; i < 600 ;i+=40) 
					{
						int xx = x - i;
						int yy = y - i;
						int ww = i * 2;
						int hh = i * 2;
						e.Graphics.DrawEllipse(Project.spoilPen, xx, yy, ww, hh);
					}
					int fontSize = 40;
					Font font = Project.getLabelFont(fontSize);
					e.Graphics.DrawString("Loading maps...", font, Project.spoilBrush, 250, 250);
				} 
				else 
				{

					if(!m_markPoint.IsEmpty)
					{
						int x = m_markPoint.X;
						int y = m_markPoint.Y;

						switch(m_markType)
						{
							case 0:
								for (int i=20; i < MARK_RADIUS ;i+=15) 
								{
									int xxxx = x - i;
									int yyyy = y - i;
									int wwww = i * 2;
									int hhhh = i * 2;
									e.Graphics.DrawEllipse(Project.markPen1, xxxx, yyyy, wwww, hhhh);
									e.Graphics.DrawEllipse(Project.markPen2, xxxx+1, yyyy+1, wwww-2, hhhh-2);
								}
								break;
							case 1:
								if(m_imageStar != null)
								{
									// assume the center of the image is hotpoint:
									e.Graphics.DrawImage(m_imageStar, x-m_imageStar.Width/2, y-m_imageStar.Height/2);
								}
								else
								{
									e.Graphics.DrawEllipse(Project.markPen1, x-20, y-20, 40, 40);
								}
								break;
							case 2:
								if(m_imageMark != null)
								{
									// assume the center of the image is hotpoint:
									e.Graphics.DrawImage(m_imageMark, x-m_imageMark.Width/2, y-m_imageMark.Height/2);
								}
								else
								{
									e.Graphics.DrawEllipse(Project.markPen1, x-20, y-20, 40, 40);
								}
								break;
							case 3:
								for (int i=5; i < 25 ;i+=15) 
								{
									int xxxx = x - i;
									int yyyy = y - i;
									int wwww = i * 2;
									int hhhh = i * 2;
									e.Graphics.DrawEllipse(Project.markPen1, xxxx, yyyy, wwww, hhhh);
									e.Graphics.DrawEllipse(Project.markPen2, xxxx+1, yyyy+1, wwww-2, hhhh-2);
								}
								break;
						}
					}

					if(Project.drawCentralPoint)
					{
						Point point = toPixelLocation(Location, null);

						int camSize = 20;
						int xx = point.X;
						int yy = point.Y;
						int ww = camSize;
						int hh = camSize;

						Project.drawCross(e.Graphics, Project.cameraPen, point, 20);
						e.Graphics.DrawRectangle(Project.cameraPen, xx - camSize/2, yy - camSize/2, ww - 1, hh - 1);
						//e.Graphics.DrawString(Location.ToString(), Project.getLabelFont(Project.FONT_SIZE_REGULAR + 2), Project.cameraBrush, xx + 20, yy);
					}

//					if(HasDragRectangle)
//					{
//						e.Graphics.DrawRectangle(Project.dragPen,  invRectangle);
//						e.Graphics.DrawRectangle(Project.dragPen,  prevInvRectangle);
//					}

					if(HasDragRectangle && !m_doDistance && !movePointMode && !moveLegMode)
					{
						e.Graphics.DrawRectangle(Project.dragPen,  dragRectangle);
						if(dragRectangle.Width > 150 && dragRectangle.Height > 50)
						{
							int xxxx = dragRectangle.X + 7;
							int yyyy = dragRectangle.Y + 12;
							if(Project.pdaWellSelectingMode)
							{
								e.Graphics.DrawString("click inside the", ffont, Project.dragBrush, xxxx,   yyyy);
								e.Graphics.DrawString("rectangle to save tiles", ffont, Project.dragBrush, xxxx,   yyyy + 15);
								e.Graphics.DrawString("to Export Folder", ffont, Project.dragBrush, xxxx,   yyyy + 30);
							}
							else
							{
								e.Graphics.DrawString("click inside the", ffont, Project.dragBrush, xxxx,   yyyy);
								e.Graphics.DrawString("rectangle to zoom", ffont, Project.dragBrush, xxxx,   yyyy + 15);
								e.Graphics.DrawString("hold Ctrl to measure distance", ffont, Project.dragBrush, xxxx,   yyyy + 30);
							}
						}
					}

					int w = pictureWidth;
					int h = pictureHeight;

					int xxx = 20;
					int yyy = h - 30;
					string label = "Image courtesy of the USGS";
					if(Project.terraserverAvailable || Project.terraserverDisconnected)
					{
                        label += " and msrmaps.com";
					}
					Font font = Project.getLabelFont(Project.FONT_SIZE_REGULAR + 2);
					e.Graphics.DrawString(label, font, Project.blackBrush, xxx,   yyy);
					e.Graphics.DrawString(label, font, Project.blackBrush, xxx+2, yyy);
					e.Graphics.DrawString(label, font, Project.blackBrush, xxx,   yyy-2);
					e.Graphics.DrawString(label, font, Project.blackBrush, xxx+2, yyy-2);
					e.Graphics.DrawString(label, font, Project.yellowBrush, xxx+1, yyy-1);
				}
			}
			catch(Exception eee) 
			{
#if DEBUG
				LibSys.StatusBar.Error("CameraManager:Paint(): " + eee.Message);
#endif
			}
		}

		public override void Paint2(object sender, PaintEventArgs e)
		{
			if(Project.drawRulers && !TileSetTerra.This.RetilingSpecial)
			{
				drawMinuteGrid(e.Graphics, false);
			}

			if(!Project.serverAvailable)
			{
				Font font = Project.getLabelFont(10);
				string str = "offline";
				int x = 22;
				int y = 6;
				e.Graphics.DrawString(str, font, Project.redBrush, x+1, y-1);
				e.Graphics.DrawString(str, font, Project.redBrush, x+1, y+1);
				e.Graphics.DrawString(str, font, Project.redBrush, x-1, y-1);
				e.Graphics.DrawString(str, font, Project.redBrush, x-1, y+1);
				e.Graphics.DrawString(str, font, Project.yellowBrush, x, y);
			}

			if(Project.drawCornerArrows)
			{
				int w = pictureWidth-1;
				int h = pictureHeight-1;
				Pen pen = Project.cameraPenWide;

				// upper left arrow:
				Point sp = new Point(3, 3);
				Point ep = new Point(3, m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.X = m_arrowWidth;
				ep.Y = 3;
				e.Graphics.DrawLine(pen, sp, ep);
				sp.X = 3;
				sp.Y = m_arrowHeight;
				e.Graphics.DrawLine(pen, sp, ep);

				// down left arrow:
				sp = new Point(3, h - 3);
				ep = new Point(3, h - m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.X = m_arrowWidth;
				ep.Y = h - 3;
				e.Graphics.DrawLine(pen, sp, ep);
				sp.X = 3;
				sp.Y = h - m_arrowHeight;
				e.Graphics.DrawLine(pen, sp, ep);

				// upper right arrow:
				sp = new Point(w - 3, 3);
				ep = new Point(w - 3, m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.X = w - m_arrowWidth;
				ep.Y = 3;
				e.Graphics.DrawLine(pen, sp, ep);
				sp.X = w - 3;
				sp.Y = m_arrowHeight;
				e.Graphics.DrawLine(pen, sp, ep);

				// down right arrow:
				sp = new Point(w - 3, h - 3);
				ep = new Point(w - 3, h - m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.X = w - m_arrowWidth;
				ep.Y = h - 3;
				e.Graphics.DrawLine(pen, sp, ep);
				sp.X = w - 3;
				sp.Y = h - m_arrowHeight;
				e.Graphics.DrawLine(pen, sp, ep);

				// left arrow:
				sp = new Point(3, h/2);
				ep = new Point(m_arrowWidth, h/2 - m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.Y = h/2 + m_arrowHeight;
				e.Graphics.DrawLine(pen, sp, ep);

				// right arrow:
				sp = new Point(w - 3, h/2);
				ep = new Point(w - m_arrowWidth, h/2 - m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.Y = h/2 + m_arrowHeight;
				e.Graphics.DrawLine(pen, sp, ep);

				// up arrow:
				sp = new Point(w/2, 3);
				ep = new Point(w/2 - m_arrowWidth, m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.X = w/2 + m_arrowWidth;
				e.Graphics.DrawLine(pen, sp, ep);

				// down arrow:
				sp = new Point(w/2, h - 3);
				ep = new Point(w/2 - m_arrowWidth, h - m_arrowHeight);
				e.Graphics.DrawLine(pen, sp, ep);
				ep.X = w/2 + m_arrowWidth;
				e.Graphics.DrawLine(pen, sp, ep);
			}

			if(HasDragRectangle && !Project.pdaWellSelectingMode && m_doDistance)
			{
				e.Graphics.DrawLine(Project.distancePen, startDragPoint, currentDragPoint);
				if(startCoord == null)
				{
					startCoord = this.toGeoLocation(startDragPoint);
				}
				GeoCoord end = this.toGeoLocation(currentDragPoint);
				Distance d = end.distanceFrom(startCoord);
				sDist = d.ToString();

				SizeF shadowSize = e.Graphics.MeasureString(sDist, ffont);
				int shadowWidth = (int)Math.Round(shadowSize.Width); 
				int shadowHeight = (int)Math.Round(shadowSize.Height - 1.0f); 

				int midX = (startDragPoint.X + currentDragPoint.X - shadowWidth) / 2;
				int midY = (startDragPoint.Y + currentDragPoint.Y - shadowHeight) / 2;

				Rectangle shadowRect = new Rectangle(midX, midY, shadowWidth, shadowHeight);
				e.Graphics.FillRectangle(Project.distanceShadowBrush, shadowRect);
				e.Graphics.DrawRectangle(Project.distanceFramePen, shadowRect);

				e.Graphics.DrawString(sDist, ffont, Project.distanceBrush, midX+1, midY);
			}

			if(HasDragRectangle && !Project.pdaWellSelectingMode && movePointMode)
			{
				e.Graphics.DrawLine(Project.movePointPen, startDragPoint, currentDragPoint);
			}
		}

		//
		// printing support:
		//
		public override void printPaint(Graphics graphics)
		{
			if(HasDragRectangle && !Project.pdaWellSelectingMode && m_doDistance)
			{
				GeoCoord startCoordP = this.toGeoLocation(startDragPoint);
				GeoCoord end = this.toGeoLocation(currentDragPoint);
				Distance d = end.distanceFrom(startCoordP);
				sDist = d.ToString();

				int fontSize = Project.FONT_SIZE_PRINT;
				Font font = Project.getLabelFont(fontSize);

				Point startDragPointP = this.toPixelLocationPrint(startCoordP, null);
				Point currentDragPointP = this.toPixelLocationPrint(end, null);

				SizeF shadowSize = graphics.MeasureString(sDist, font);
				int shadowWidth = (int)Math.Round(shadowSize.Width); 
				int shadowHeight = (int)Math.Round(shadowSize.Height - 1.0f); 

				graphics.DrawLine(Project.distancePenPrint, startDragPointP, currentDragPointP);
				int midX = (startDragPointP.X + currentDragPointP.X - shadowWidth) / 2;
				int midY = (startDragPointP.Y + currentDragPointP.Y - shadowHeight) / 2;

				Rectangle shadowRect = new Rectangle(midX, midY, shadowWidth, shadowHeight);
				graphics.FillRectangle(Project.distanceShadowBrush, shadowRect);
				graphics.DrawRectangle(Project.distanceFramePenPrint, shadowRect);
				
				graphics.DrawString(sDist, font, Project.distanceBrush, midX+1, midY);
			}
		}

		public override void printPaint2(Graphics graphics)
		{
			drawMinuteGrid(graphics, true);
		}

		private static bool doShadow = true;
		private static Brush brushShadow = Brushes.Black;

		public void drawMinuteGrid(Graphics graphics, bool isPrint)
		{
			GeoCoord topLeft = isPrint ? m_coverageTopLeftPrint : m_coverageTopLeft;
			GeoCoord bottomRight = isPrint ? m_coverageBottomRightPrint : m_coverageBottomRight;

			int w = isPrint ? pictureWidthPrint : pictureWidth;
			int h = isPrint ? pictureHeightPrint : pictureHeight; 
			
			// draw a scale ruler:
			double metersPerPixelX = isPrint ? MetersPerPixelXPrint() : MetersPerPixelX();
			double unitFactor = Distance.METERS_PER_MILE;
			string sUnit  = "mile";
			string sUnits = "miles";
			switch(Project.unitsDistance) 
			{ 
				case Distance.UNITS_DISTANCE_KM:
				case Distance.UNITS_DISTANCE_M:
					unitFactor = 1000.0d;
					sUnit  = "km";
					sUnits = "km";
					break;
			}

			double pixelsPerUnit = unitFactor / metersPerPixelX;
			int scaleLength = (int)pixelsPerUnit;
			double stepLength = pixelsPerUnit / 10.0d;
			string unitsName = "1 " + sUnit;

			if(pixelsPerUnit / 12.0d > w / 4.0d)		// overzoom - camera alt 50m to 300m
			{
				scaleLength = (int)(pixelsPerUnit / 100.0d);
				stepLength = pixelsPerUnit / 1000.0d;
				unitsName = "0.01 " + sUnit;
			}
			else if(pixelsPerUnit > w / 4.0d)
			{
				scaleLength = (int)(pixelsPerUnit / 10.0d);
				stepLength = pixelsPerUnit / 100.0d;
				unitsName = "0.1 " + sUnit;
			}
			else if(pixelsPerUnit < 0.5d)
			{
				scaleLength = (int)(pixelsPerUnit * 1000.0d);
				stepLength = pixelsPerUnit * 100.0d;
				unitsName = "1000 " + sUnits;
			}
			else if(pixelsPerUnit < 1.0d)
			{
				scaleLength = (int)(pixelsPerUnit * 500.0d);
				stepLength = pixelsPerUnit * 50.0d;
				unitsName = "500 " + sUnits;
			}
			else if(pixelsPerUnit < 3.0d)
			{
				scaleLength = (int)(pixelsPerUnit * 100.0d);
				stepLength = pixelsPerUnit * 10.0d;
				unitsName = "100 " + sUnits;
			}
			else if(pixelsPerUnit < 10.0d)
			{
				scaleLength = (int)(pixelsPerUnit * 50.0d);
				stepLength = pixelsPerUnit * 5.0d;
				unitsName = "50 " + sUnits;
			}
			else if(pixelsPerUnit < 50.0d)
			{
				scaleLength = (int)(pixelsPerUnit * 10.0d);
				stepLength = pixelsPerUnit;
				unitsName = "10 " + sUnits;
			}

			bool lightMap = Project.drawTerraserverMode == "topo";	// light-colored background

			Pen pen = lightMap ? Pens.Black : Pens.Yellow;
			Brush brush = lightMap ? Brushes.Black : Brushes.Yellow;
			Brush brushShadow = lightMap ? Brushes.White : Brushes.Black;

			// position and draw the ruler:
			int offX = 50;
			int offY = 20;

			int begX = w - offX - scaleLength;
			int endX = w - offX;
			int endY = h - offY;

			graphics.DrawLine(pen, begX, h-offY, endX, h-offY);
			graphics.DrawLine(pen, begX, h-offY-3, endX, h-offY-3);
			int count = 0;
			for(double pos=(double)begX; count < 11 ;pos+=stepLength,count++)
			{
				int len = (count==0 || count==5 || count==10) ? 10 : 6;
				graphics.DrawLine(pen, (int)pos, endY, (int)pos, endY - len);
			}

			int fontSize = isPrint ? Project.FONT_SIZE_PRINT : Project.FONT_SIZE_REGULAR;
			Font font = Project.getLabelFont(fontSize);

			SizeF size = graphics.MeasureString(unitsName, font);
			int labelWidth = (int)(size.Width); 

			int xxxx = (begX + endX - labelWidth) / 2;
			int yyyy = h - offY - 25;

			graphics.DrawString(unitsName, font, brushShadow, xxxx + 1, yyyy);
			graphics.DrawString(unitsName, font, brushShadow, xxxx, yyyy + 1);
			graphics.DrawString(unitsName, font, brushShadow, xxxx - 1, yyyy);
			graphics.DrawString(unitsName, font, brushShadow, xxxx, yyyy - 1);

			graphics.DrawString(unitsName, font, brush, xxxx, yyyy);

			// done with scale ruler, see if side rulers make sense:
			double minsInView = (bottomRight.Lng - topLeft.Lng) * 60.0;
			if(minsInView > 80) 
			{
				return;		// too large view, minute grid does not do us any good
			} 

			// figure out what the big tick increment should be:
			double incMin;		// when decimal minutes coordStyle are used
			double incMinD;		// for seconds-style coords
			double incSnap;
			int divider = 10;
			int dividerD = 10;

			if(minsInView <= 0.2d) 
			{
				incMin  = 0.01d;		// overzoom around 50m
				incMinD = 0.1d / 6.0d;
				incSnap = incMin;
			} 
			else if(minsInView <= 1) 
			{
				incMin  = 0.10d;
				incMinD = 1.0d / 6.0d;
				incSnap = incMin;
			} 
			else if(minsInView <= 2) 
			{
				incMin  = 0.10d;
				incMinD = 1.0d / 6.0d;
				incSnap = incMin;
			} 
			else if(minsInView <= 7) 
			{
				incMin  = 0.50d;
				incMinD = 0.5d;
				incSnap = incMin;
				divider = 5;
				dividerD = 3;
			}
			else if(minsInView <= 15) 
			{
				incMin  = 2.0d;
				incMinD = 2.0d;
				incSnap = incMin;
				divider = 4;
				dividerD = 4;
			}
			else if(minsInView <= 50) 
			{
				incMin  = 5.0d;
				incMinD = 5.0d;
				incSnap = incMin;
				divider = 5;
				dividerD = 5;
			}
			else 
			{
				incMin  = 5.0d;
				incMinD = 10.0d;
				incSnap = incMin;
				divider = 5;
				dividerD = 10;
			}

			double inc = incMin / 60.0d;
			incSnap = incSnap / 60.0d;
			bool forceDegrees = false;

			switch(Project.coordStyle)
			{
				default:
				case 0:		// DD°MM'SS"
					forceDegrees = true;
					inc = incMinD / 60.0d;
					divider = dividerD;
					break;
				case 3:		// UTM = 11S E433603 N3778359 -- use case 1 for now:
				case 1:		// DD°MM.MMMM
					break;
				case 2:		// DD.DDDDDD
					break;
			}

			// snap the horizontal grid to nearest degree
			double startLng = Math.Ceiling(topLeft.Lng);
			while(startLng > topLeft.Lng)
			{
				startLng -= incSnap;
			}

			switch(Project.coordStyle)
			{
				default:
				case 0:		// DD°MM'SS"
					// adjust a bit to ensure the grid sticks to round numbers:
					string sLng;
					do
					{
						startLng -= 1.0d / 18000.0d;
						sLng = "" + GeoCoord.lngToString(startLng, Project.coordStyle, true, false);
					} while(!sLng.EndsWith("0'00\""));
					startLng -= 1.0d / 36000.0d;
					break;
				case 3:		// UTM = 11S E433603 N3778359 -- use case 1 for now:
				case 1:		// DD°MM.MMMM
				case 2:		// DD.DDDDDD
					break;
			}

			bool doSmallGrid = true; //isPrint;

			double topLeftLat = isPrint? toGeoLocationPrint(new Point(0, 0)).Lat : toGeoLocation(new Point(0, 0)).Lat;
			double topRightLat = isPrint? toGeoLocationPrint(new Point(w, 0)).Lat : toGeoLocation(new Point(w, 0)).Lat;
			double bottomLeftLat = isPrint? toGeoLocationPrint(new Point(0, h)).Lat : toGeoLocation(new Point(0, h)).Lat;
			double distortionOffsetLat = topLeftLat - topRightLat;

			double lat = topLeftLat - (distortionOffsetLat <  0 ? 0 : distortionOffsetLat);
			double lat2 = bottomLeftLat - (distortionOffsetLat >  0 ? 0 : distortionOffsetLat);
			double lng;
			int prevLngF = 1000;
			int prevLngC = 1000;
			
			fontSize = isPrint ? Project.FONT_SIZE_PRINT : Project.FONT_SIZE_REGULAR;
			font = Project.getLabelFont(fontSize);

			pen = lightMap ? Pens.Black : (isPrint ? Pens.White : Pens.Yellow);
			Pen penShadow = lightMap ? new Pen(Color.White, 3.0f): new Pen(Color.Black, 3.0f);

			for(lng=startLng+inc; lng < bottomRight.Lng ;lng+=inc)
			{
				GeoCoord tmp = new GeoCoord(lng, lat);
				Point p = isPrint ? toPixelLocationPrint(tmp, null) : toPixelLocation(tmp, null);
				GeoCoord tmp2 = new GeoCoord(lng, lat2);
				Point p2 = isPrint ? toPixelLocationPrint(tmp2, null) : toPixelLocation(tmp2, null);
				graphics.DrawLine(penShadow, p.X, p.Y, p.X, p.Y+10);
				graphics.DrawLine(pen, p.X, p.Y, p.X, p.Y+10);
				if(p2.Y > endY + 5 || p2.X < begX - 10 || p2.X > endX + 10)
				{
					graphics.DrawLine(penShadow, p2.X, p2.Y, p2.X, p2.Y-10);
					graphics.DrawLine(pen, p2.X, p2.Y, p2.X, p2.Y-10);
				}
				if(p.X > 40)
				{
					bool doDegree = (int)Math.Floor(lng) != prevLngF || (int)Math.Ceiling(lng) != prevLngC;
					string sLng = "" + GeoCoord.lngToString(lng, Project.coordStyle, doDegree || forceDegrees, false);
					prevLngF = (int)Math.Floor(lng);
					prevLngC = (int)Math.Ceiling(lng);
					int xxx = p.X-5;
					int yyy = p.Y+10;
					Brush brush1 = brush;
					if(doShadow)
					{
						if(!isPrint)
						{
							// shadow on the screen is always helpful - better contrast
							graphics.DrawString(sLng, font, Project.blackBrush, xxx-1, yyy);
							graphics.DrawString(sLng, font, Project.blackBrush, xxx+1, yyy);
							graphics.DrawString(sLng, font, Project.blackBrush, xxx, yyy-1);
							graphics.DrawString(sLng, font, Project.blackBrush, xxx, yyy+1);
							graphics.DrawString(sLng, font, Project.blackBrush, xxx-1, yyy-1);
							graphics.DrawString(sLng, font, Project.blackBrush, xxx+1, yyy-1);
							graphics.DrawString(sLng, font, Project.blackBrush, xxx-1, yyy+1);
							graphics.DrawString(sLng, font, Project.blackBrush, xxx+1, yyy+1);
							brush1 = Brushes.Yellow;
						} 
						else if(Project.printMinuteRulersUseShadow) 
						{
							// shadow on the print is optional
							SizeF shadowSize = graphics.MeasureString(sLng, font);
							int shadowWidth = (int)Math.Round(shadowSize.Width); 
							int shadowHeight = (int)Math.Round(shadowSize.Height - 1.0f); 
							Rectangle shadowRect = new Rectangle(xxx, yyy, shadowWidth, shadowHeight);
							graphics.FillRectangle(Brushes.White, shadowRect);
							brush1 = Brushes.Black;
						}
					}
					graphics.DrawString(sLng, font, brush1, xxx, yyy);
				}
			}
			for(lng=startLng-inc; doSmallGrid && lng < bottomRight.Lng ;lng+=inc/divider)
			{
				GeoCoord tmp = new GeoCoord(lng, lat);
				Point p = isPrint ? toPixelLocationPrint(tmp, null) : toPixelLocation(tmp, null);
				graphics.DrawLine(penShadow, p.X, p.Y, p.X, p.Y+3);
				graphics.DrawLine(pen, p.X, p.Y, p.X, p.Y+3);
				GeoCoord tmp2 = new GeoCoord(lng, lat2);
				Point p2 = isPrint ? toPixelLocationPrint(tmp2, null) : toPixelLocation(tmp2, null);
				if(p2.Y > endY + 5 || p2.X < begX - 10 || p2.X > endX + 10)
				{
					graphics.DrawLine(penShadow, p2.X, p2.Y, p2.X, p2.Y-3);
					graphics.DrawLine(pen, p2.X, p2.Y, p2.X, p2.Y-3);
				}
			}

			// snap the vertical grid to nearest degree
			double startLat = Math.Floor(topLeft.Lat);

			while(startLat < topLeft.Lat)
			{
				startLat += incSnap;
			}

			switch(Project.coordStyle)
			{
				default:
				case 0:		// DD°MM'SS"

					// adjust a bit to ensure th egrid sticks to round numbers:
					string sLat;
					do
					{
						startLat += 1.0d / 18000.0d;
						sLat = "" + GeoCoord.latToString(startLat, Project.coordStyle, true, false);
					} while(!sLat.EndsWith("0'00\""));
					startLat += 1.0d / 36000.0d;

					break;
				case 3:		// UTM = 11S E433603 N3778359 -- use case 1 for now:
				case 1:		// DD°MM.MMMM
				case 2:		// DD.DDDDDD
					break;
			}

			double topLeftLng = isPrint ? toGeoLocationPrint(new Point(0, 0)).Lng : toGeoLocation(new Point(0, 0)).Lng;
			double bottomLeftLng = isPrint ? toGeoLocationPrint(new Point(0, h)).Lng : toGeoLocation(new Point(0, h)).Lng;
			double topRightLng = isPrint ? toGeoLocationPrint(new Point(w, 0)).Lng : toGeoLocation(new Point(w, 0)).Lng;
			double distortionOffsetLng = topLeftLng - bottomLeftLng;

			lng = topLeftLng + (distortionOffsetLng >  0 ? 0 : -distortionOffsetLng);
			double lng2 = topRightLng - (distortionOffsetLng <  0 ? 0 : distortionOffsetLng);
			int prevLatF = 1000;
			int prevLatC = 1000;
			for(lat=startLat-inc; lat > bottomRight.Lat ;lat-=inc)
			{
				GeoCoord tmp = new GeoCoord(lng, lat);
				Point p = isPrint ? toPixelLocationPrint(tmp, null) : toPixelLocation(tmp, null);
				graphics.DrawLine(penShadow, p.X, p.Y, p.X+10, p.Y);
				graphics.DrawLine(pen, p.X, p.Y, p.X+10, p.Y);
				GeoCoord tmp2 = new GeoCoord(lng2, lat);
				Point p2 = isPrint ? toPixelLocationPrint(tmp2, null) : toPixelLocation(tmp2, null);
				graphics.DrawLine(penShadow, p2.X, p2.Y, p2.X-10, p2.Y);
				graphics.DrawLine(pen, p2.X, p2.Y, p2.X-10, p2.Y);
				if(p.Y > 60 && p.Y < h - 40)
				{
					bool doDegree = (int)Math.Floor(lat) != prevLatF || (int)Math.Ceiling(lat) != prevLatC;
					string sLat = "" + GeoCoord.latToString(lat, Project.coordStyle, doDegree || forceDegrees, false);
					prevLatF = (int)Math.Floor(lat);
					prevLatC = (int)Math.Ceiling(lat);
					int xxx = p.X+11;
					int yyy = p.Y-5;
					Brush brush1 = brush;
					if(doShadow)
					{
						if(!isPrint)
						{
							graphics.DrawString(sLat, font, Project.blackBrush, xxx-1, yyy);
							graphics.DrawString(sLat, font, Project.blackBrush, xxx+1, yyy);
							graphics.DrawString(sLat, font, Project.blackBrush, xxx, yyy-1);
							graphics.DrawString(sLat, font, Project.blackBrush, xxx, yyy+1);
							graphics.DrawString(sLat, font, Project.blackBrush, xxx-1, yyy-1);
							graphics.DrawString(sLat, font, Project.blackBrush, xxx+1, yyy-1);
							graphics.DrawString(sLat, font, Project.blackBrush, xxx-1, yyy+1);
							graphics.DrawString(sLat, font, Project.blackBrush, xxx+1, yyy+1);
							brush1 = Brushes.Yellow;
						} 
						else if(Project.printMinuteRulersUseShadow)
						{
							SizeF shadowSize = graphics.MeasureString(sLat, font);
							int shadowWidth = (int)Math.Round(shadowSize.Width - 1.0f); 
							int shadowHeight = (int)Math.Round(shadowSize.Height - 2.0f); 
							Rectangle shadowRect = new Rectangle(xxx, yyy, shadowWidth, shadowHeight);
							graphics.FillRectangle(Brushes.White, shadowRect);
							brush1 = Brushes.Black;
						}
					}
					graphics.DrawString(sLat, font, brush1, xxx, yyy);
				}
			}
			for(lat=startLat+inc; doSmallGrid && lat > bottomRight.Lat ;lat-=inc/divider)
			{
				GeoCoord tmp = new GeoCoord(lng, lat);
				Point p = isPrint ? toPixelLocationPrint(tmp, null) : toPixelLocation(tmp, null);
				graphics.DrawLine(penShadow, p.X, p.Y, p.X+3, p.Y);
				graphics.DrawLine(pen, p.X, p.Y, p.X+3, p.Y);
				GeoCoord tmp2 = new GeoCoord(lng2, lat);
				Point p2 = isPrint ? toPixelLocationPrint(tmp2, null) : toPixelLocation(tmp2, null);
				graphics.DrawLine(penShadow, p2.X, p2.Y, p2.X-3, p2.Y);
				graphics.DrawLine(pen, p2.X, p2.Y, p2.X-3, p2.Y);
			}
		}

		private Point spoilPoint;

		// make sure that SpoilPicture() is followed by ProcessCameraMove() at some point, or spoiling will stay.
		public void SpoilPicture(Point point)
		{
			Project.spoiling = true;			// will be reset in LayersManager:t_cameraMoved()
			spoilPoint = point;
			m_pictureManager._Refresh();		// invalidate directly, so that spoiling appears
			Project.pictureDirtyCounter = 4;	// provoke Refresh for several cycles, so that spoiling disappears for sure
		}

		public void SpoilPicture()
		{
			SpoilPicture(Point.Empty);
		}

		public void SpoilPicture(GeoCoord loc)
		{
			Point point = toPixelLocation(loc, null);

			SpoilPicture(point);
		}

		public void ReloadAndRefresh()
		{
			Project.reloadRefresh = true;
			TileCache.Clear();
			TerraserverCache.Clear();
			// the following will cause ReTile and empty caches will reload their elements, but not from files:
			ProcessCameraMove();
			// Project.reloadRefresh will be reset by LayersManager in a separate thread, when reloading is initiated.
		}

		private void loadMarkImages()
		{
			if(!m_triedLoad)
			{
				m_triedLoad = true;
				// try loading image files:
				string starFilePath = Project.GetMiscPath("redstar.gif");
				try 
				{
					if(!File.Exists(starFilePath))
					{
						// load the file remotely, don't hold diagnostics if load fails:
						string url = Project.MISC_FOLDER_URL + "/redstar.gif";
						DloadProgressForm loaderForm = new DloadProgressForm(url, starFilePath, false, true);
						loaderForm.ShowDialog();
					}
					m_imageStar = new Bitmap(starFilePath);
				} 
				catch {}
			}
		}

		private const int	MARK_RADIUS = 62;
		private Point		m_markPoint;
		private int			m_markType = 0;
		private Bitmap		m_imageStar = null;
		private Bitmap		m_imageMark = null;
		private bool		m_triedLoad = false;

		public void MarkLocation(GeoCoord loc, int markType)
		{
			m_markType = markType;

			if(m_markType == 1 && m_imageStar == null)
			{
				loadMarkImages();
			}
			removeMarkLocation();
			m_markPoint = toPixelLocation(loc, null);
			Rectangle r = new Rectangle(m_markPoint.X - MARK_RADIUS, m_markPoint.Y - MARK_RADIUS,
											MARK_RADIUS * 2, MARK_RADIUS * 2);
			m_pictureManager.Invalidate(r);
		}

		public void MarkLocation(GeoCoord loc, Bitmap thumb, bool doRemove)
		{
			m_markType = 2;
			m_imageMark = thumb;
			if(doRemove)
			{
				removeMarkLocation();
			}
			m_markPoint = toPixelLocation(loc, null);
			Rectangle r = new Rectangle(m_markPoint.X - MARK_RADIUS, m_markPoint.Y - MARK_RADIUS,
				MARK_RADIUS * 2, MARK_RADIUS * 2);
			m_pictureManager.Invalidate(r);
		}

		public void removeMarkLocation()
		{
			if(!m_markPoint.IsEmpty)
			{
				Rectangle r = new Rectangle(m_markPoint.X - MARK_RADIUS, m_markPoint.Y - MARK_RADIUS,
					MARK_RADIUS * 2, MARK_RADIUS * 2);
				m_markPoint = Point.Empty;
				m_pictureManager.Invalidate(r);
			}
		}

		public void invalidateAround(GeoCoord loc, int width, int height, int position)		// see Project.thumbPosition for values
		{
			Point markPoint = toPixelLocation(loc, null);

			Rectangle r;

			switch(Project.thumbPosition)
			{
				case 0:		// top right
					r = new Rectangle(markPoint.X, markPoint.Y - height, width, height);
					break;
				case 1:		// center
				default:
					r = new Rectangle(markPoint.X - width/2, markPoint.Y - height/2, width, height);
					break;
			}
			m_pictureManager.Invalidate(r);
		}

		private void SpoilLocation(GeoCoord loc)
		{
			SpoilLocation(loc, Project.spoilPen);
		}

		private const int ARROW_SIZE = 100;
		private const int ARROW_SHIFT_IN = 200;
		private const int ARROW_SHIFT_OUT = 120;

		private static DateTime m_lastSpoiled = DateTime.Now;
		private const int LAST_SPOILED_LATENCY = 1000; // ms
 
		private void drawSpoiler(Graphics graphics, int x, int y, Pen pen)
		{
			for (int i=20; i < 100 ;i+=30) 
			{
				int xx = x - i;
				int yy = y - i;
				int ww = i * 2;
				int hh = i * 2;
				graphics.DrawEllipse(pen, xx, yy, ww, hh);
			}
		}

		//
		// dir: 0=up 1=down 2=left 3=right
		// pointOut: 0=in  1=out
		// x, y point to tip of the arrow
		//
		private void drawArrow(Graphics graphics, int x, int y, Pen pen, int dir, int pointOut)
		{
			Point[] triangle = new Point[3];

			if(pointOut == 1)
			{
				switch(dir)
				{
					case 0:		// up
						y -= ARROW_SHIFT_OUT + ARROW_SIZE;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x - ARROW_SIZE, y + ARROW_SIZE);
						triangle[2] = new Point(x + ARROW_SIZE, y + ARROW_SIZE);
						break;
					case 1:		// down
						y += ARROW_SHIFT_OUT + ARROW_SIZE;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x - ARROW_SIZE, y - ARROW_SIZE);
						triangle[2] = new Point(x + ARROW_SIZE, y - ARROW_SIZE);
						break;
					case 2:		// left
						x -= ARROW_SHIFT_OUT + ARROW_SIZE;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x + ARROW_SIZE, y - ARROW_SIZE);
						triangle[2] = new Point(x + ARROW_SIZE, y + ARROW_SIZE);
						break;
					case 3:		// right
						x += ARROW_SHIFT_OUT + ARROW_SIZE;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x - ARROW_SIZE, y - ARROW_SIZE);
						triangle[2] = new Point(x - ARROW_SIZE, y + ARROW_SIZE);
						break;
				}
			}
			else
			{
				// arrows pointing in
				switch(dir)
				{
					case 0:		// up
						y += ARROW_SHIFT_IN;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x - ARROW_SIZE, y + ARROW_SIZE);
						triangle[2] = new Point(x + ARROW_SIZE, y + ARROW_SIZE);
						break;
					case 1:		// down
						y -= ARROW_SHIFT_IN;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x - ARROW_SIZE, y - ARROW_SIZE);
						triangle[2] = new Point(x + ARROW_SIZE, y - ARROW_SIZE);
						break;
					case 2:		// left
						x -= ARROW_SHIFT_IN;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x - ARROW_SIZE, y - ARROW_SIZE);
						triangle[2] = new Point(x - ARROW_SIZE, y + ARROW_SIZE);
						break;
					case 3:		// right
						x += ARROW_SHIFT_IN;
						triangle[0] = new Point(x, y);
						triangle[1] = new Point(x + ARROW_SIZE, y - ARROW_SIZE);
						triangle[2] = new Point(x + ARROW_SIZE, y + ARROW_SIZE);
						break;
				}
			}

			graphics.DrawPolygon(pen, triangle);
		}

		private void SpoilLocationZoom(GeoCoord loc, int pointOut)	// pointOut: 0=in  1=out
		{
			TimeSpan elapsed = DateTime.Now - m_lastSpoiled;
			if(elapsed.TotalMilliseconds < LAST_SPOILED_LATENCY)
			{
				return;
			}

			Pen pen = Project.spoilPenZoom;

			Point spoilPoint = toPixelLocation(Location, null);
			Graphics graphics = PictureManager.This.Graphics;
			int x = spoilPoint.X;
			int y = spoilPoint.Y;

			drawSpoiler(graphics, x, y, pen);

			drawArrow(graphics, x, y, pen, 0, pointOut);
			drawArrow(graphics, x, y, pen, 1, pointOut);
			drawArrow(graphics, x, y, pen, 2, pointOut);
			drawArrow(graphics, x, y, pen, 3, pointOut);

			graphics.DrawString("zoom " + (pointOut==0 ? "in" : "out"), Project.getLabelFont(20), Brushes.Cyan, x, y);

			m_lastSpoiled = DateTime.Now;
		}

		private void SpoilLocationPan(GeoCoord loc, int shiftDir)		// shiftDir: 0=up 1=down 2=left 3=right
		{
			TimeSpan elapsed = DateTime.Now - m_lastSpoiled;
			if(elapsed.TotalMilliseconds < LAST_SPOILED_LATENCY)
			{
				return;
			}

			Pen pen = Project.spoilPenPan;

			Point spoilPoint = toPixelLocation(Location, null);
			Graphics graphics = PictureManager.This.Graphics;
			int x = spoilPoint.X;
			int y = spoilPoint.Y;

			string dirStr;
			int dirArrow;
			int dy = -10;
			switch(shiftDir)
			{
				case 0:
					dirStr = "pan down";
					dirArrow = 1;
					dy = 50;
					break;
				case 1:
					dirStr = "pan up";
					dirArrow = 0;
					dy = -70;
					break;
				case 2:
					dirStr = "pan right";
					dirArrow = 3;
					break;
				default:
				case 3:
					dirStr = "pan left";
					dirArrow = 2;
					break;
			}

			//drawSpoiler(graphics, x, y, pen);

			drawArrow(graphics, x, y, pen, dirArrow, 1);

			graphics.DrawString(dirStr, Project.getLabelFont(20), Brushes.Yellow, x - 50, y + dy);

			m_lastSpoiled = DateTime.Now;
		}

		// instant mark right before redraw is called, for visual reference
		private void SpoilLocation(GeoCoord loc, Pen pen)
		{
			Point spoilPoint = toPixelLocation(Location, null);
			Graphics graphics = PictureManager.This.Graphics;
			int x = spoilPoint.X;
			int y = spoilPoint.Y;

			for (int i=20; i < 200 ;i+=40) 
			{
				int xx = x - i;
				int yy = y - i;
				int ww = i * 2;
				int hh = i * 2;
				graphics.DrawEllipse(pen, xx, yy, ww, hh);
			}

		}
		#endregion

	}
}
