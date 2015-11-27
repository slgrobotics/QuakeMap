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
using LibSys;

namespace LibFormats
{
	/// <summary>
	/// Summary description for FileGeocachingLoc.
	/// </summary>
	public class FileGeocachingLoc : BaseFormat
	{
		public FileGeocachingLoc()
		{
		}

		public static string FormatName = "Geocaching .loc";
		public static string FileExtension = ".loc";
		public static string FileExtensions = "*.loc";

		public FileGeocachingLoc(InsertWaypoint insertWaypoint) : base(insertWaypoint)
		{
			m_insertWaypoint = insertWaypoint;
		}

		public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: FileGeocachingLoc:process() filename=" + filename);
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(filename);
				processXml(url, xmlDoc, source);
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("FileGeocachingLoc:process() " + e.Message);
				ret = false;
			}
			return ret;
		}

		public bool processXml(string url, XmlDocument xmlDoc, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: FileGeocachingLoc:processXml()");
			try
			{
				XmlNodeList waypointNodes = xmlDoc.SelectNodes("/loc/waypoint");

				/*
					<waypoint>
						<name id="GC37EB"><![CDATA[Gotta Play by Cyn]]></name>
						<coord lat="33.6033" lon="-117.651433333333"/>
						<type>Geocache</type>
						<link text="Cache Details">http://www.geocaching.com/seek/cache_details.aspx?wp=GC37EB</link>
					</waypoint>
				 */
				
				CreateInfo createInfo = new CreateInfo();	// we will recycle it in place, filling every time.

				// we want to traverse XmlDocument fast, as tile load operations can be numerous
				// and come in pack. So we avoid using XPath and rely mostly on "foreach child":
				foreach(XmlNode nnode in waypointNodes) 
				{
					try 
					{
						//LibSys.StatusBar.Trace("FileGeocachingLoc:process() node=" + nnode.Name);
						createInfo.init("wpt");
						createInfo.typeExtra = "unknown";	// type: geocache
						createInfo.source = source;

						foreach(XmlNode node in nnode.ChildNodes)
						{
							//LibSys.StatusBar.Trace("    child node=" + node.Name);
							switch(node.Name)
							{
								case "name":
									createInfo.name = node.Attributes["id"].InnerText.Trim();
									createInfo.urlName = node.InnerText.Trim();
									break;
								case "coord":
									createInfo.setLat(node.Attributes["lat"].InnerText);
									createInfo.setLng(node.Attributes["lon"].InnerText);
									break;
								case "ele":		// just in case
									createInfo.setElev(node.InnerText);
									break;
								case "elev":	// just in case
									createInfo.setElev(node.InnerText);
									break;
								case "type":
									createInfo.typeExtra = node.InnerText.Trim();
									if(createInfo.typeExtra.ToLower().IndexOf("geocache") >= 0)
									{
										createInfo.sym = "Geocache";
									}
									break;
								case "link":
									createInfo.url = node.InnerText.Trim();
									//LibSys.StatusBar.Trace("    link='" + createInfo.url + "'");
									break;
							}
						} 

						m_insertWaypoint(createInfo);
					}
					catch (Exception ee) 
					{
						// bad node - not a big deal...
						LibSys.StatusBar.Error("FileGeocachingLoc:processXml() node=" + nnode.Name + " " + ee.Message);
					}
				}
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("FileGeocachingLoc:processXml() " + e.Message);
				ret = false;
			}
			return ret;
		}
	}
}
