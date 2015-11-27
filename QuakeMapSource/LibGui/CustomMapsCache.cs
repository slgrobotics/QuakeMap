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
using System.Threading;
using System.IO;

using LibSys;
using LibGeo;
using LibNet;

namespace LibGui
{

	/// <summary>
	/// Summary description for CustomMapsCache.
	/// </summary>
	public class CustomMapsCache
	{
		private static ArrayList m_customMaps = new ArrayList();
		public  static ArrayList LiveObjects { get { return m_customMaps; } }
		public  static ArrayList CustomMapsAll { get { return m_customMaps; } }

		public static event DynamicObjectCreateHandler DynamicObjectCreateCallback;
		public static event DynamicObjectDeleteHandler DynamicObjectDeleteCallback;
		public static event DynamicObjectMoveHandler DynamicObjectMoveCallback;

		public static void addCustomMap(CustomMap cm)
		{
			m_customMaps.Add(cm);
			DynamicObjectCreateCallback(cm);		// takes care of displaying it on LayerCustomMap
		}

		public static void deleteCustomMap(CustomMap cm)
		{
			DynamicObjectDeleteCallback(cm);
			m_customMaps.Remove(cm);
			cm.cleanup();
		}

		public static void RemoveCustomMapsBySource(string source)
		{
			lock(m_customMaps)
			{
			again:
				for(int i=0; i < m_customMaps.Count ;i++)
				{
					CustomMap cm = (CustomMap)m_customMaps[i];
					try
					{
						if(cm.Source.Equals(source))
						{
							deleteCustomMap(cm);
							goto again;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
		}

		public static void RemoveCustomMapById(long id)
		{
			lock(m_customMaps)
			{
				for(int i=0; i < m_customMaps.Count ;i++)
				{
					CustomMap cm = (CustomMap)m_customMaps[i];
					try
					{
						if(cm.Id == id)
						{
							deleteCustomMap(cm);
							break;
						}
					} 
					catch (Exception e)
					{
					}
				}
			}
		}

		public static CustomMap getCustomMapByName(string name)
		{
			foreach(CustomMap cm in LiveObjects)
			{
				if(name.Equals(cm.MapName))
				{
					return cm;
				}
			}
			return null;
		}

		public static CustomMap getCustomMapById(long id)
		{
			foreach(CustomMap cm in LiveObjects)
			{
				if(id == cm.Id)
				{
					return cm;
				}
			}
			return null;
		}

		public static void customMapMoved(CustomMap cm, Rectangle prev)
		{
			DynamicObjectMoveCallback(cm, prev);
			//keepInView();
		}

		public static void init()
		{
		}

		public static void AddCustomMap(CustomMap cm)
		{
			m_customMaps.Add(cm);
		}

		public static void shutdown()
		{
			//m_thread.Abort();
		}

		// waypoints boundaries, so that zoom-to-group-of-waypoints is easy:
		protected static GeoCoord	m_topLeft = new GeoCoord(180.0d, -90.0d);
		public static GeoCoord		TopLeft { get { return m_topLeft; } set { m_topLeft = value; } }
		protected static GeoCoord	m_bottomRight = new GeoCoord(-180.0d, 90.0d);
		public static GeoCoord		BottomRight { get { return m_bottomRight; } set { m_bottomRight = value; } }

		/// <summary>
		/// resets boundaries (TopLeft, BottomRight) so that newly added waypoints' boundaries are calculated
		/// useful for reading in .loc files 
		/// </summary>
		public static void resetBoundaries()
		{
			m_topLeft = new GeoCoord(180.0d, -90.0d);
			m_bottomRight = new GeoCoord(-180.0d, 90.0d);
		}

		public static void pushBoundaries(GeoCoord loc)
		{
			if(loc.Lat > m_topLeft.Lat)
			{
				m_topLeft.Lat = loc.Lat;
			}
			if(loc.Lat < m_bottomRight.Lat)
			{
				m_bottomRight.Lat = loc.Lat;
			}
			if(loc.Lng < m_topLeft.Lng)
			{
				m_topLeft.Lng = loc.Lng;
			}
			if(loc.Lng > m_bottomRight.Lng)
			{
				m_bottomRight.Lng = loc.Lng;
			}
		}
	}
}
