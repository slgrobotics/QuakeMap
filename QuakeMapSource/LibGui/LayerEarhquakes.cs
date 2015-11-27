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
using System.Windows.Forms;
using System.Diagnostics;

using LibSys;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// Summary description for LayerEarthquakes.
	/// </summary>
	public class LayerEarthquakes : Layer, IObjectsLayoutManager
	{
		public override string Name() { return "LayerEarthquakes"; }
		public static LayerEarthquakes This = null; 

		protected bool m_hasPutOnMap = false;
		public bool HasPutOnMap { get { return m_hasPutOnMap; } set { m_hasPutOnMap = value; } }

		public LayerEarthquakes(PictureManager pm, CameraManager cm) : base(pm, cm)
		{
			EarthquakesCache.PictureManager = pm;
			EarthquakesCache.CameraManager = cm;

			EarthquakesCache.DynamicObjectCreateCallback += new DynamicObjectCreateHandler(EarthquakeCreateHandler);
			EarthquakesCache.DynamicObjectDeleteCallback += new DynamicObjectDeleteHandler(EarthquakeDeleteHandler);
			EarthquakesCache.DynamicObjectMoveCallback += new DynamicObjectMoveHandler(EarthquakeMoveHandler);

			EarthquakesCache.init();
			This = this;
		}

		public override void shutdown()
		{
			EarthquakesCache.shutdown();
		}

		public void EarthquakeCreateHandler(LiveObject lo)
		{
			//LibSys.StatusBar.Trace("IP: LayerEarthquakes:EarthquakeCreateHandler() eq=" + lo);
			PictureManager.Invalidate(lo.BoundingRect);
		}

		public void EarthquakeDeleteHandler(LiveObject lo)
		{
			//LibSys.StatusBar.Trace("IP: LayerEarthquakes:EarthquakeRemoveHandler() eq=" + lo);
			PictureManager.Invalidate(lo.BoundingRect);
		}

		public void EarthquakeMoveHandler(LiveObject lo, Rectangle prev)
		{
			//LibSys.StatusBar.Trace("IP: LayerEarthquakes:EarthquakeRemoveHandler() eq=" + lo);
			lo.init(true);	// provoke PutOnMap in Paint()
			PictureManager.Invalidate(prev);
		}

		public void init()
		{
			m_hasPutOnMap = false;
		}

		public override void PictureResized()
		{
			// LibSys.StatusBar.Trace("IP: LayerEarthquakes::PictureResized()  drawableReady=" + Project.drawableReady);
			init();
		}

		public override void CameraMoved()
		{
			// LibSys.StatusBar.Trace("IP: LayerEarthquakes::CameraMoved()  drawableReady=" + Project.drawableReady);
			base.CameraMoved();
			EarthquakesCache.RefreshEarthquakesDisplayed();
			sEqPopup = null;
			init();
		}

		protected Earthquake EarthquakeByPoint(Point point)
		{
			foreach(Earthquake eq in EarthquakesCache.EarthquakesDisplayed)
			{
				try 
				{
					if(eq.labelBoundingRect().Contains(point))
					{
						return eq;
					}
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LE:EartquakeByPoint(): " + e.Message);
				}
			}
			return null;
		}

		protected static string sEqPopup = null;

		public override void ProcessMouseMove(Point movePoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				Earthquake hoverEq = EarthquakeByPoint(movePoint);
				if(hoverEq != null)
				{
					string tmp = string.Format("{0:F1} - ", hoverEq.Magn) + hoverEq.sDateTime + "\n" + hoverEq.Comment + "\n" + hoverEq.Source;
					if(!tmp.Equals(sEqPopup))  // && movePoint.Y > 30)	// make sure the popup will not overlap menu
					{
						sEqPopup = tmp;
						LibSys.StatusBar.WriteLine("* " + sEqPopup);		// will not be added to log

						//movePoint.Offset(0, -15);
						Point screenPoint = m_pictureManager.PointToScreen(movePoint);
						Project.MapShowPopup(m_pictureManager.PictureBox, sEqPopup, screenPoint);
					}
					Cursor.Current = Cursors.Hand;
				}
				else
				{
					sEqPopup = null;	// mouse reentry into the same label is allowed
				}
			} 
			catch (Exception e)
			{
			}
		}
		
		// support for menu-building on the right click:
		public override LiveObject EvalMouseClick(Point movePoint, GeoCoord mouseGeoLocation, bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				return EarthquakeByPoint(movePoint);		// may be null
			} 
			catch {}
			return null;
		}
		
		public override void ProcessMouseClick(Point clickPoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				if(m_cameraManager.CanDrillDown)
				{
					Earthquake clickEq = EarthquakeByPoint(clickPoint);
					if(clickEq != null)
					{
						if(altDown)
						{
							Point screenPoint = m_pictureManager.PointToScreen(clickPoint);
							string tmp = string.Format("{0:F1} - ", clickEq.Magn) + clickEq.sDateTime + "\n" + clickEq.Comment + "\n" + clickEq.Source;
							Project.MapShowPopup(m_pictureManager.PictureBox, tmp, screenPoint);
						}
						else
						{
							string tmp = clickEq.Url;
							if(tmp != null && tmp.StartsWith("http://"))
							{
								tmp = "\"" + tmp + "\"";
								//LibSys.StatusBar.Trace(tmp);
								Project.RunBrowser(tmp);
							}
						}
					}
				}
			} 
			catch {}
		}
		
		public void PutOnMap()
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: LayerEarthquakes:PutOnMap()  " + EarthquakesCache.EarthquakesAll.Count);
#endif

			Project.mainCommand.eqFilterOn(TimeMagnitudeFilter.Enabled);

			foreach(LiveObject lo in EarthquakesCache.EarthquakesAll)
			{
				try 
				{
					lo.init(true);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LE:PutOnMap(): eq=" + lo + " " + e.Message);
				}
			}
			foreach(Earthquake eq in EarthquakesCache.EarthquakesAll)
			{
				try 
				{
					if(TimeMagnitudeFilter.passesAll(eq.DateTime, eq.Magn))
					{
						eq.PutOnMap(this, null, this);
					}
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LE:PutOnMap(): eq=" + eq + " " + e.Message);
				}
			}
			foreach(Earthquake eq in EarthquakesCache.EarthquakesAll)
			{
				try 
				{
					if(TimeMagnitudeFilter.passesAll(eq.DateTime, eq.Magn))
					{
						eq.AdjustPlacement(this, null, this);
					}
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LE:PutOnMap() - AdjustPlacement: eq=" + eq + " " + e.Message);
				}
			}
			m_hasPutOnMap = true;
		}

		public override void Paint(object sender, PaintEventArgs e)
		{
			try 
			{
				base.Paint(sender, e);
				if(!Project.spoiling)
				{
					if(!HasPutOnMap) 
					{
						//LibSys.StatusBar.Trace("LayerEarthquakes:Paint(): calling PutOnMap()");
						PutOnMap();
					}

					foreach(Earthquake eq in EarthquakesCache.EarthquakesAll)
					{
						if(TimeMagnitudeFilter.passesAll(eq.DateTime, eq.Magn))
						{
							if(eq.BoundingRect.IsEmpty)	// new object just added dynamically, or old object moved
							{
								eq.PutOnMap(this, null, this);
								PictureManager.Invalidate(eq.BoundingRect);
							}
							Point pixelPos = toPixelLocation(eq.Location, null);
							if(e.ClipRectangle.IntersectsWith(eq.BoundingRect)) 
							{
								eq.Paint(e.Graphics, this, null);
							}
						}
					}

					foreach(Earthquake eq in EarthquakesCache.EarthquakesAll)
					{
						if(TimeMagnitudeFilter.passesAll(eq.DateTime, eq.Magn))
						{
							eq.AdjustPlacement(this, null, this);
						}
					}

					foreach(Earthquake eq in EarthquakesCache.EarthquakesAll)
					{
						Point pixelPos = toPixelLocation(eq.Location, null);
						if(e.ClipRectangle.IntersectsWith(eq.BoundingRect) && TimeMagnitudeFilter.passesAll(eq.DateTime, eq.Magn))
						{
							eq.PaintLabel(e.Graphics, this, null, false);
						}
					}
				}
			}
			catch(Exception eee) 
			{
				//LibSys.StatusBar.Error("LayerEarthquakes:Paint(): " + eee);
			}
		}

		//
		// printing support:
		//
		public override void printPaint(Graphics graphics)
		{
			foreach(Earthquake eq in EarthquakesCache.EarthquakesAll)
			{
				if(TimeMagnitudeFilter.passesAll(eq.DateTime, eq.Magn))
				{
					Point pixelPosPrint = toPixelLocationPrint(eq.Location, null);
					Point pixelPosDispl = eq.PixelLocation;
					int offsetX = pixelPosPrint.X - pixelPosDispl.X;
					int offsetY = pixelPosPrint.Y - pixelPosDispl.Y;
					eq.Paint(graphics, this, null, offsetX, offsetY);
					eq.PaintLabel(graphics, this, null, true, offsetX, offsetY);
				}
			}
		}

		public override void printPaint2(Graphics graphics)
		{
		}

		// actually counts intesections' total surface in pixels:
		public int countIntersections(LiveObject llo, Rectangle r)
		{
			int total = 0;
			foreach(LiveObject lo in EarthquakesCache.EarthquakesAll) 
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

	}
}
