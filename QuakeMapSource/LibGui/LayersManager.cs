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
using System.Threading;

using LibGeo;
using LibSys;

namespace LibGui
{
	/// <summary>
	/// LayersManager maintains a list of Layers and arranges orderly paint
	/// </summary>
	public class LayersManager : ArrayList
	{
		private PictureManager m_pictureManager;

		public LayersManager(PictureManager pm)
		{
			m_pictureManager = pm;
		}

		public void shutdown()
		{
			try 
			{
				foreach(Layer layer in this)
				{
					layer.shutdown();
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:shutdown(): " + eee.Message);
			}
		}

		public bool ShowBasicMap 
		{
			set 
			{
				foreach(Layer layer in this)
				{
					if(layer.Name().Equals("LayerBasicMap"))
					{
						layer.Enabled = value;
					}
				}
			}
		}

		public bool ShowTerraserver 
		{
			set 
			{
				foreach(Layer layer in this)
				{
					if(layer.Name().Equals("LayerTerraserver"))
					{
						layer.Enabled = value;
						//layer.CameraMoved();		// causes ReTile()
					}
				}
				CameraMoved();		// causes ReTile()
			}
		}

		public bool ShowEarthquakes 
		{
			set 
			{
				foreach(Layer layer in this)
				{
					if(layer.Name().Equals("LayerEarthquakes"))
					{
						layer.Enabled = value;
						Project.mainCommand.eqFilterOn(value && TimeMagnitudeFilter.Enabled);
					}
				}
			}
		}

		public bool ShowWaypoints 
		{
			set 
			{
				foreach(Layer layer in this)
				{
					if(layer.Name().Equals("LayerWaypoints"))
					{
						layer.Enabled = value;
					}
				}
			}
		}

		public bool ShowVehicles 
		{
			set 
			{
				foreach(Layer layer in this)
				{
					if(layer.Name().Equals("LayerVehicles"))
					{
						layer.Enabled = value;
					}
				}
			}
		}

		public bool ShowCustomMaps 
		{
			set 
			{
				foreach(Layer layer in this)
				{
					if(layer.Name().Equals("LayerCustomMaps"))
					{
						layer.Enabled = value;
					}
				}
			}
		}

		public bool ShowFreehand 
		{
			set 
			{
				foreach(Layer layer in this)
				{
					if(layer.Name().Equals("LayerFreehand"))
					{
						layer.Enabled = value;
					}
				}
			}
		}

		public void Paint(object sender, PaintEventArgs e)
		{
			//LibSys.StatusBar.Trace("LayersManager:Paint  rect=" + e.ClipRectangle);

			Project.drawableReady = true;	// first paint turns it on

			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						layer.Paint(sender, e);
					}
				}
				// by this time basic tiles and camera (including spoiling circles) are drawn.
				// do not spend time drawing anything else if we are in spoiling mode.
				if(!Project.spoiling)
				{
					foreach(Layer layer in this)
					{
						if(layer.Enabled) 
						{
							layer.Paint2(sender, e);
						}
					}
				}
			}
			catch(Exception eee) 
			{
#if DEBUG
				LibSys.StatusBar.Error("LM:Paint(): " + eee.Message);
#endif
			}
		}

		public void PictureResized()
		{
			Project.terraserverAvailable = false;
			try 
			{
				foreach(Layer layer in this)
				{
					layer.PictureResized();
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:PictureResized(): " + eee.Message);
			}
		}

		private System.Threading.Thread m_thread = null;

		public void CameraMoved()
		{
			// a lot of work is happening here, re-tiling and downloading of tiles etc.
			// having a separate thread helps main thread not freeze visually.
			//add dt worker method to the thread pool / queue a task 
			
			// looks like threadpool doesn't have high enough priority, and refresh is choppy:
			//Project.threadPool.PostRequest (new WorkRequestDelegate (t_cameraMoved)); 

			//Project.mainForm.Invoke(new MethodInvoker(t_cameraMoved));
			//t_cameraMoved();

			m_thread = new System.Threading.Thread( new System.Threading.ThreadStart(t_cameraMoved));
			m_thread.Priority = ThreadPriority.AboveNormal;
			m_thread.Name = "LayersManager-CameraMoved";
			m_thread.Start();
		}

		private void t_cameraMoved()		//object state, DateTime requestEnqueueTime)
		{
			//Cursor.Current = Cursors.WaitCursor;
			//Project.terraserverAvailable = false;
			try 
			{
				foreach(Layer layer in this)
				{
					layer.CameraMoved();
				}
				m_pictureManager.Refresh();
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:CameraMoved(): " + eee.Message);
			}
			// now when all re-tiling is done, we can turn reload flag back to false (if total refresh was requested):
			if(Project.reloadRefresh)
			{
				LibSys.StatusBar.WriteLine("* OK: Reloaded and refreshed the picture");
				Project.reloadRefresh = false;
			}
			Project.spoiling = false;	// make sure next Refresh() will be in normal mode
			Project.pictureDirtyCounter = 4;	// provoke Refresh for several cycles, so that spoiling disappears for sure
			m_pictureManager.CameraManager.CameraMoving = false;
		}

		public void ProcessMouseMove(Point movePoint, GeoCoord mouseGeoLocation,
										bool controlDown, bool shiftDown, bool altDown)
		{
			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						layer.ProcessMouseMove(movePoint, mouseGeoLocation, controlDown, shiftDown, altDown);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:pictureMouseMove(): " + eee.Message);
			}
		}

		public void ProcessMouseDown(Point movePoint, GeoCoord mouseGeoLocation,
										bool controlDown, bool shiftDown, bool altDown)
		{
			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						layer.ProcessMouseDown(movePoint, mouseGeoLocation, controlDown, shiftDown, altDown);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:ProcessMouseDown(): " + eee.Message);
			}
		}

		public void ProcessMouseUp(Point movePoint, GeoCoord mouseGeoLocation,
										bool controlDown, bool shiftDown, bool altDown)
		{
			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						layer.ProcessMouseUp(movePoint, mouseGeoLocation, controlDown, shiftDown, altDown);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:pictureMouseUp(): " + eee.Message);
			}
		}

		public LiveObject EvalMouseClick(Point clickPoint, GeoCoord mouseGeoLocation,
										bool controlDown, bool shiftDown, bool altDown)
		{
			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						LiveObject lo = layer.EvalMouseClick(clickPoint, mouseGeoLocation, controlDown, shiftDown, altDown);
						if(lo != null)
						{
							return lo;
						}
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:EvalMouseClick(): " + eee.Message);
			}
			return null;
		}

		public void ProcessMouseClick(Point clickPoint, GeoCoord mouseGeoLocation,
			bool controlDown, bool shiftDown, bool altDown)
		{
			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						//LibSys.StatusBar.Trace("LM:ProcessMouseClick(): layer" + layer.Name());
						layer.ProcessMouseClick(clickPoint, mouseGeoLocation, controlDown, shiftDown, altDown);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:ProcessMouseClick(): " + eee.Message);
			}
		}

		public SortedList getCloseObjects(GeoCoord loc, double radiusMeters)
		{
			SortedList ret = new SortedList();

			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						layer.getCloseObjects(ret, loc, radiusMeters);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:ProcessMouseClick(): " + eee.Message);
			}

			return ret;
		}

		//
		// printing support:
		//
		public void printPaint(Graphics graphics)
		{
			try 
			{
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						layer.printPaint(graphics);
					}
				}
				foreach(Layer layer in this)
				{
					if(layer.Enabled) 
					{
						layer.printPaint2(graphics);
					}
				}
			}
			catch(Exception eee) 
			{
				LibSys.StatusBar.Error("LM:ProcessMouseClick(): " + eee.Message);
			}
		}
	}
}
