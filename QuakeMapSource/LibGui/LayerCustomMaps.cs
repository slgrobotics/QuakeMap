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
using LibSys;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// Summary description for LayerCustomMaps.
	/// </summary>
	public class LayerCustomMaps : Layer, IObjectsLayoutManager
	{
		public static LayerCustomMaps This;

		public override string Name() { return "LayerCustomMaps"; }

		protected bool m_hasPutOnMap = false;
		public bool HasPutOnMap { get { return m_hasPutOnMap; } set { m_hasPutOnMap = value; } }

		public LayerCustomMaps(PictureManager pm, CameraManager cm) : base(pm, cm)
		{
			This = this;

			CustomMapsCache.DynamicObjectCreateCallback += new DynamicObjectCreateHandler(CustomMapCreateHandler);
			CustomMapsCache.DynamicObjectDeleteCallback += new DynamicObjectDeleteHandler(CustomMapDeleteHandler);
			CustomMapsCache.DynamicObjectMoveCallback += new DynamicObjectMoveHandler(CustomMapMoveHandler);
			CustomMapsCache.init();
		}

		public override void shutdown()
		{
			CustomMapsCache.shutdown();
		}

		public void CustomMapCreateHandler(LiveObject lo)
		{
			//LibSys.StatusBar.Trace("IP: LayerCustomMaps:CustomMapCreateHandler() map=" + lo);
			PictureManager.Invalidate(lo.BoundingRect);
		}

		public void CustomMapDeleteHandler(LiveObject lo)
		{
			//LibSys.StatusBar.Trace("IP: LayerCustomMaps:CustomMapDeleteHandler() map=" + lo);
			PictureManager.Invalidate(lo.BoundingRect);
		}

		public void CustomMapMoveHandler(LiveObject lo, Rectangle prev)
		{
			//LibSys.StatusBar.Trace("IP: LayerCustomMaps:CustomMapMoveHandler() veh=" + lo);
			lo.init(true);	// provoke PutOnMap in Paint()
			prev.Inflate(1, 1);
			// it is likely that move is not too far, so do one bigger invalidate - save drawing time:
			PictureManager.Invalidate(Rectangle.Union(prev, lo.BoundingRect));
			//PictureManager.Invalidate(lo.BoundingRect);
			//PictureManager.Invalidate(prev);
		}

		public void init()
		{
			m_hasPutOnMap = false;
		}

		public override void PictureResized()
		{
			// LibSys.StatusBar.Trace("IP: LayerCustomMaps::PictureResized()  drawableReady=" + Project.drawableReady);
			init();
		}

		public override void CameraMoved()
		{
			// LibSys.StatusBar.Trace("IP: LayerCustomMaps::CameraMoved()  drawableReady=" + Project.drawableReady);
			base.CameraMoved();
			init();
		}

		private CustomMap CustomMapByPoint(Point point)
		{
			try
			{
				for(int i=CustomMapsCache.LiveObjects.Count-1; i >= 0 ;i--)
				{
					CustomMap map = (CustomMap)CustomMapsCache.LiveObjects[i];
					try 
					{
						if(map.labelBoundingRect().Contains(point) || map.imageBoundingRect().Contains(point))
						{
							return map;
						}
					} 
					catch(Exception e) 
					{
						LibSys.StatusBar.Error("LV:CustomMapByPoint(): " + e.Message);
					}
				}
				//Cursor.Current = Cursors.Default;
			} 
			catch {}
			return null;
		}

		protected static string sMapPopup = null;

		public override void ProcessMouseMove(Point movePoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
			if(controlDown || shiftDown || altDown || m_cameraManager.HasDragRectangle)
			{
				return;
			}

			try
			{
				CustomMap hoverMap = CustomMapByPoint(movePoint);
				if(hoverMap != null)
				{
					string tmp = hoverMap.toStringPopup();
					if(!tmp.Equals(sMapPopup)) // && movePoint.Y > 30)	// make sure the popup will not overlap menu
					{
						sMapPopup = tmp;
						LibSys.StatusBar.WriteLine("* " + sMapPopup);		// will not be added to log
						//movePoint.Offset(0, 0);
						Point screenPoint = m_pictureManager.PointToScreen(movePoint);
						Project.MapShowPopup(m_pictureManager.PictureBox, sMapPopup, screenPoint);
					}
				}
				else
				{
					sMapPopup = null;	// mouse reentry into the same label is allowed
				}
			} 
			catch {}
		}
		
		// support for menu-building on the right click:
		public override LiveObject EvalMouseClick(Point movePoint, GeoCoord mouseGeoLocation, bool controlDown, bool shiftDown, bool altDown)
		{
			try
			{
				return CustomMapByPoint(movePoint);		// may be null
			} 
			catch {}
			return null;
		}

//		public override void ProcessMouseClick(Point clickPoint, GeoCoord mouseGeoLocation, bool controlDown, bool shiftDown, bool altDown)
//		{
//			try
//			{
//				if(m_cameraManager.CanDrillDown)
//				{
//					CustomMap clickMap = CustomMapByPoint(clickPoint);
//					if(clickMap != null)
//					{
//						if(altDown)
//						{
//							Point screenPoint = m_pictureManager.PointToScreen(clickPoint);
//							Project.MapShowPopup(m_pictureManager.PictureBox, clickMap.toStringPopup(), screenPoint);
//						}
//						else 
//						{
//							string tmp = clickMap.Url;
//							if(tmp != null && tmp.StartsWith("http://"))
//							{
//								tmp = "\"" + tmp + "\"";
//								//LibSys.StatusBar.Trace(tmp);
//								Project.RunBrowser(tmp);
//							}
//							else if(!Project.allowMapPopups)
//							{
//								Point screenPoint = m_pictureManager.PointToScreen(clickPoint);
//								Project.MapShowPopup1(m_pictureManager.PictureBox, clickMap.toStringPopup(), screenPoint);
//							}
//						}
//					}
//				}
//			} 
//			catch {}
//		}
		
		public void PutOnMap()
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: LayerCustomMaps:PutOnMap()  " + CustomMapsCache.LiveObjects.Count);
#endif

			foreach(LiveObject lo in CustomMapsCache.LiveObjects)
			{
				try 
				{
					lo.init(true);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LV:PutOnMap(): lo=" + lo + " " + e.Message);
				}
			}
			foreach(LiveObject lo in CustomMapsCache.LiveObjects)
			{
				try 
				{
					lo.PutOnMap(this, null, this);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("LV:PutOnMap(): lo=" + lo + " " + e.Message);
				}
			}
//			foreach(LiveObject lo in CustomMapsCache.LiveObjects)
//			{
//				try 
//				{
//					lo.AdjustPlacement(this, null, this);
//				} 
//				catch(Exception e) 
//				{
//					LibSys.StatusBar.Error("LV:PutOnMap() - AdjustPlacement: lo=" + lo + " " + e.Message);
//				}
//			}
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
						//LibSys.StatusBar.Trace("LayerCustomMaps:Paint(): calling PutOnMap()");
						PutOnMap();
					}

					foreach(LiveObject lo in CustomMapsCache.LiveObjects)
					{
						if(lo.BoundingRect.IsEmpty)	// new object just added dynamically, or old object moved
						{
							lo.PutOnMap(this, null, this);
							PictureManager.Invalidate(lo.BoundingRect);		// will cause Paint
						}
						else if(e.ClipRectangle.IntersectsWith(lo.BoundingRect)) 
						{
							lo.Paint(e.Graphics, this, null);
						}
						else
						{
							LibSys.StatusBar.Trace("LV:Paint(): outside visible area");
						}
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LV:Paint(): " + eee.Message);
			}
		}

		//
		// printing support:
		//
		public override void printPaint(Graphics graphics)
		{
		}

		public override void printPaint2(Graphics graphics)
		{
		}

		// actually counts intesections' total surface in pixels:
		public int countIntersections(LiveObject llo, Rectangle r)
		{
			int total = 0;
			return total;
		}

	}
}
