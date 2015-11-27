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
using System.Text;

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for ClickMapGenerator.
	/// </summary>
	public class ClickMapGenerator
	{
		private Rectangle m_scaledBounds;
		private Rectangle m_originalBounds;

		public ClickMapGenerator(Rectangle scaledBounds, Rectangle originalBounds)
		{
			m_scaledBounds = scaledBounds;
			m_originalBounds = originalBounds;
		}

//		private string _mapL = "<a  href=\"javascript:window.open('http://smugmug.com/photos/popup.mg?ImageID={0}&Size=Large','{1}','toolbar=no,scrollbars=yes,resizable=yes,width=595,height=850'); void('');\">";
		private string _mapLSmugmugPopup = "javascript:window.open('http://smugmug.com/photos/popup.mg?ImageID={0}&Size={1}','{2}','toolbar=no,scrollbars=yes,resizable=yes'); void('');";
		private string _mapLSmugmug = "http://smugmug.com/photos/popup.mg?ImageID={0}&Size={1}";
		private string _mapLPopup = "javascript:window.open('{0}','{1}','toolbar=no,scrollbars=yes,resizable=yes'); void('');";

		private string getMapLinkForPicture(Waypoint wpt)
		{
			string ret = "";		// Project.WEBSITE_LINK_WEBSTYLE

			string[] sSizes = new string[] {
											"Small",
											"Medium",
											"Large",
											"Original"
										   };

			string sSize = sSizes[Project.photoGalleryPictureSize];
			
			bool usePopup = Project.webPageLinksUsePopup;
			string urlBase = Project.webPagePhotoStorageUrlBase; // like "http://www.quakemap.com/demo/testpictures/";

			string url;
			int pos;

			try
			{
				url = wpt.ThumbSource;
				string smugmugSignature = "smugmug.com/photos/";

				if((pos=url.IndexOf(smugmugSignature)) != -1)
				{
					pos += smugmugSignature.Length;
					int pos1 = url.IndexOf("-Th");
					if(pos1 > pos)
					{
						string sPictureId = url.Substring(pos, pos1 - pos);
						long pictureId = Convert.ToInt64(sPictureId);
						if(usePopup)
						{
							ret = String.Format(_mapLSmugmugPopup, pictureId, sSize, "photo" + pictureId);
						}
						else
						{
							ret = String.Format(_mapLSmugmug, pictureId, sSize);
						}
					}
				}
				else if(wpt.Url.Length > 0)
				{
					if(usePopup)
					{
						ret = String.Format(_mapLPopup, wpt.Url, "photo");
					}
					else
					{
						ret = wpt.Url;
					}
				}
				else if((pos=url.LastIndexOf("|")) != -1)		// zipfile
				{
					if(usePopup)
					{
						url = urlBase + url.Substring(pos+1);
						ret = String.Format(_mapLPopup, url, "photo");
					}
					else
					{
						ret = urlBase + url.Substring(pos+1);
					}
				}
				else if((pos=url.LastIndexOf("\\")) != -1)
				{
					if(usePopup)
					{
						url = urlBase + url.Substring(pos+1);
						ret = String.Format(_mapLPopup, url, "photo");
					}
					else
					{
						ret = urlBase + url.Substring(pos+1);
					}
				}
			}
			catch {}

			return ret;
		}

		private const string _map1 = "<map name=\"mainimagemap\">\r\n";
		private const string _mapR = "<area shape=\"rect\" coords=\"{0},{1},{2},{3}\" href=\"{4}\">\r\n";
		private const string _map99 = "</map>\r\n";

		public string generateClickMap()
		{
			StringBuilder mapContent = new StringBuilder();

			double xRatio = m_scaledBounds.Width / m_originalBounds.Width;
			double yRatio = m_scaledBounds.Height / m_originalBounds.Height;

//			int imageId = 22315402;
//			mapContent.Append(String.Format(_mapL, imageId, "photo" + imageId));

			mapContent.Append(_map1);

			try
			{
				for(int i=WaypointsCache.WaypointsDisplayedNotSorted.Count-1; i >= 0 ;i--)
				{
					Waypoint wpt = (Waypoint)WaypointsCache.WaypointsDisplayedNotSorted[i];
					try 
					{
						if(wpt.ThumbImage != null)
						{
							makeMapEntry(mapContent, wpt.thumbBoundingRect(LayerWaypoints.This), getMapLinkForPicture(wpt));
						}
						else if(wpt.Url != null && wpt.Url.Length > 0)
						{
							makeMapEntry(mapContent, wpt.labelBoundingRect(), wpt.Url);
						}
					} 
					catch(Exception e) 
					{
						LibSys.StatusBar.Error("generateClickMap(): waypoints: " + e.Message);
					}
				}
			} 
			catch {}

			try
			{
				for(int i=EarthquakesCache.EarthquakesDisplayed.Count-1; i >= 0 ;i--)
				{
					Earthquake eq = (Earthquake)EarthquakesCache.EarthquakesDisplayed[i];
					try 
					{
						if(eq.Url != null && eq.Url.Length > 0)
						{
							makeMapEntry(mapContent, eq.labelBoundingRect(), eq.Url);
						}
					} 
					catch(Exception e) 
					{
						LibSys.StatusBar.Error("generateClickMap(): earthquakes: " + e.Message);
					}
				}
			} 
			catch {}

			mapContent.Append(rectMapEntry(0, 0, m_scaledBounds.Width, 12, Project.WEBSITE_LINK_WEBSTYLE));
			mapContent.Append(_map99);

			return mapContent.ToString();
		}

		private void makeMapEntry(StringBuilder mapContent, Rectangle objRect, string link)
		{
			int x1 = objRect.X + WebPageGenerator.margin;
			int y1 = objRect.Y + WebPageGenerator.topOffset;
			int x2 = x1 + objRect.Width;
			int y2 = y1 + objRect.Height;
			mapContent.Append(rectMapEntry(x1, y1, x2, y2, link));
		}

		private static string rectMapEntry(int left, int top, int right, int bottom, string hrefLink)
		{
			return String.Format(_mapR, left, top, right, bottom, hrefLink);
		}
	}
}
