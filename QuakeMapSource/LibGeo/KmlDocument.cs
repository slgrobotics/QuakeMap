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
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using LibSys;
using LibFormats;

namespace LibGeo
{
	/// <summary>
	/// KmlDocument handles all aspects of building GoogleEarth KML documents, represent their structure.
	/// </summary>
	public class KmlDocument
	{
		private FileKml m_kmlFile = new FileKml();
		public FileKml kmlFile { get { return m_kmlFile; } }

		private XmlDocument m_xmlDoc = new XmlDocument();
		public XmlDocument xmlDoc { get { return m_xmlDoc; } }

		private XmlNode m_documentNode;
		public XmlNode documentNode { get { return m_documentNode; } }

		public KmlDocument(string name, int open)
		{
			// have the folders and files created to support the KMZ:
			m_kmlFile.homeXmlDoc = m_xmlDoc;
			m_kmlFile.initTempFolder();

			// now work with xml (kml) home doc:
			string seedXml = Project.SEED_XML
				+ "<kml creator=\"" + Project.PROGRAM_NAME_HUMAN + " " + Project.PROGRAM_VERSION_HUMAN + " - " + Project.WEBSITE_LINK_WEBSTYLE + "\"/>";

			m_xmlDoc.LoadXml(seedXml);

			// cannot add this to seed, xmlns will appear in every node:
			XmlAttribute attr = m_xmlDoc.CreateAttribute("xmlns");
			attr.InnerText = "http://earth.google.com/kml/2.0";
			m_xmlDoc.DocumentElement.Attributes.Append(attr);

			m_documentNode = m_xmlDoc.CreateElement("Document");
			m_xmlDoc.DocumentElement.AppendChild(m_documentNode);

			m_documentNode = m_xmlDoc.DocumentElement.FirstChild;

			XmlNode node = m_xmlDoc.CreateElement("name");
			node.InnerText = name.Trim();
			m_documentNode.AppendChild(node);

			if(open >= 0)
			{
				node = m_xmlDoc.CreateElement("open");
				node.InnerText = String.Format("{0}", open);
				m_documentNode.AppendChild(node);
			}

			KmlStyles.importStyles(this);
		}

		public void LookAt(GeoCoord camLoc, double camElev, double camHeading, double camTilt)
		{
			XmlElement elementNode = m_xmlDoc.CreateElement("LookAt");

			XmlNode node;

			node = elementNode.OwnerDocument.CreateElement("longitude");
			node.InnerText = String.Format("{0:F4}", camLoc.Lng);
			elementNode.AppendChild(node);

			node = elementNode.OwnerDocument.CreateElement("latitude");
			node.InnerText = String.Format("{0:F4}", camLoc.Lat);
			elementNode.AppendChild(node);

			node = elementNode.OwnerDocument.CreateElement("range");
			node.InnerText = String.Format("{0:F0}", camElev);			// meters
			elementNode.AppendChild(node);

			node = elementNode.OwnerDocument.CreateElement("tilt");
			node.InnerText = String.Format("{0}", camTilt);
			elementNode.AppendChild(node);

			node = elementNode.OwnerDocument.CreateElement("heading");
			node.InnerText = String.Format("{0}", camHeading);
			elementNode.AppendChild(node);

			m_documentNode.AppendChild(elementNode);
		}

		/// <summary>
		/// creates a root-level folder
		/// </summary>
		/// <param name="name"></param>
		/// <param name="open"></param>
		/// <param name="description"></param>
		/// <returns></returns>
		public KmlFolder CreateFolder(string name, int open, string description)
		{
			KmlFolder ret = new KmlFolder(this, name, open, description);

			m_documentNode.AppendChild(ret.elementNode);

			return ret;
		}

		public KmlFolder CreateFolder(string name)
		{
			return CreateFolder(name, 1, null);
		}

		/// <summary>
		/// creates a new folder and adds it to parent
		/// </summary>
		/// <param name="name"></param>
		/// <param name="open"></param>
		/// <param name="description"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public KmlFolder CreateFolder(string name, int open, string description, KmlElement parent)
		{
			KmlFolder ret = new KmlFolder(this, name, open, description);

			parent.AppendChild(ret);

			return ret;
		}

		/// <summary>
		/// creates and adds an "About" folder. Use after the last folder was created.
		/// </summary>
		public void CreateAbout()
		{
			string about = "Created " + DateTime.Now.ToString()
				+ " using <a href='" + Project.WEBSITE_LINK_WEBSTYLE + "'>"
				+ Project.PROGRAM_NAME_HUMAN + " " + Project.PROGRAM_VERSION_HUMAN + "</a>.";

			this.CreateFolder("About", -1, about);
		}

		public void run()
		{
#if DEBUG
			Save("C:\\temp\\kml.xml");		// debug
#endif
			m_kmlFile.run();
		}

		public void Save(string filename)
		{
			m_xmlDoc.Save(filename);
		}

		public void SaveToKmz(string filename, bool deleteOnExit)
		{
			m_kmlFile.saveToKmz(filename, deleteOnExit);
		}
	}

	public class KmlElement
	{
		private KmlDocument m_ownerDoc;
		public KmlDocument ownerDoc { get { return m_ownerDoc; } }

		private XmlNode m_elementNode;
		public XmlNode elementNode { get { return m_elementNode; } }

		public KmlElement(KmlElement parent, string kmlType, string name, int open, string description)
			: this(parent.ownerDoc, kmlType, name, open, description)
		{
			parent.AppendChild(this);
		}

		public KmlElement(KmlDocument ownerDoc, string kmlType, string name, int open, string description)
		{
			m_ownerDoc = ownerDoc;

			m_elementNode = m_ownerDoc.xmlDoc.CreateElement(kmlType);

			XmlNode node;

			if(name != null)
			{
				node = m_elementNode.OwnerDocument.CreateElement("name");
				node.InnerText = name.Trim();
				m_elementNode.AppendChild(node);
			}

			// setSnippet();
			node = m_elementNode.OwnerDocument.CreateElement("Snippet");
			m_elementNode.AppendChild(node);

			XmlAttribute attr = m_elementNode.OwnerDocument.CreateAttribute("maxLines");
			attr.InnerText = "2";
			node.Attributes.Append(attr);

			if(description != null)
			{
				node = m_elementNode.OwnerDocument.CreateElement("description");
				m_elementNode.AppendChild(node);
				XmlNode node2 = m_elementNode.OwnerDocument.CreateCDataSection("description");
				node2.InnerText = description.Trim();
				node.AppendChild(node2);
			}

			if(open >= 0)
			{
				node = m_elementNode.OwnerDocument.CreateElement("open");
				node.InnerText = String.Format("{0}", open);
				m_elementNode.AppendChild(node);
			}
		}

		public void AppendChild(KmlElement childElementToAdd)
		{
			m_elementNode.AppendChild(childElementToAdd.m_elementNode);
		}
	}

	public class KmlFolder : KmlElement
	{
		public KmlFolder(KmlDocument ownerDoc, string name, int open, string description)
			: base (ownerDoc, "Folder", name, open, description)
		{
		}

		public KmlFolder(KmlFolder parent, string name, int open, string description)
			: this (parent.ownerDoc, name, open, description)
		{
			parent.AppendChild(this);
		}
	}

	public class KmlStyles
	{
		public static void importStyles(KmlDocument kmlDoc)
		{
			XmlDocument stylesXmlDoc = new XmlDocument();

            string filename = Path.Combine(Application.StartupPath, "kmlstyles.xml");   // Project.GetMiscPath("kmlstyles.xml");

            if (File.Exists(filename))
            {
                stylesXmlDoc.Load(filename);

                XmlNodeList styleNodes = stylesXmlDoc.SelectNodes("//Style");

                foreach (XmlNode node in styleNodes)
                {
                    XmlNode myNode = kmlDoc.xmlDoc.ImportNode(node, true);
                    kmlDoc.documentNode.AppendChild(myNode);
                }
                LibSys.StatusBar.Trace("* Ready");
            }
            else
            {
                Project.ErrorBox(null, "Error: could not locate file " + filename);
            }
		}
	}

	public class KmlTrack : KmlFolder
	{
		public KmlTrack(KmlFolder parent, Track trk)
			: base (parent, trk.Name, 1, null)
		{
			bool trksClampToGround = Project.kmlOptions.TrksClampToGround;

			if(!trksClampToGround)
			{
				// do a little bit of looking into waypoints, decide if we need to clamp to ground anyway:
				bool hasAlt = false;
				for(int i=0; i < trk.Trackpoints.Count ;i++)
				{
					Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
					if(wpt.Location.Elev != 0.0d)
					{
						hasAlt = true;
						break;
					}
				}
				if(!hasAlt)
				{
					trksClampToGround = true;
				}
			}

			string actualAltMode = trksClampToGround ? "clampedToGround" : "absolute";
			string actualPathName = trksClampToGround ? "Path - clamped to ground" : "Path-Actual";
			string actualPathStyle = trksClampToGround ? "#TrackPathActual" : "#TrackPathActual";

			KmlPlacemark path = new KmlPlacemark(this, actualPathName, -1, null);

			// first path follows the trackpoints - actual track in the air:
			path.setColor(trk.color, Color.FromArgb(127, Color.Green));

			ArrayList points = new ArrayList();

			for(int i=0; i < trk.Trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
				CamPos campos = new CamPos(wpt.Location.Lng, wpt.Location.Lat, wpt.Location.Elev, true);
				points.Add(campos);
			}

			path.setStyle(actualPathStyle);
			path.LineString(points, 0, 0, actualAltMode);

			if(!trksClampToGround)
			{
				// second path clamps to the ground, in case the first path goes under the surface:
				KmlPlacemark path2 = new KmlPlacemark(this, "Path-Ground", -1, null);

				path2.setColor(trk.color, Color.FromArgb(127, Color.Green));
				path2.setStyle("#TrackPathGround");
				path2.LineString(points, 0, 0, "clampedToGround");

				// third path creates a wall to the ground:
				KmlPlacemark path3 = new KmlPlacemark(this, "Path-Wall", -1, null);

				path3.setColor(Color.Transparent, Color.FromArgb(77, trk.color));		// Lines are completely invisible, wall is 30% transparency
				path3.setStyle("#TrackPathWall");
				path3.LineString(points, 0, 1, "absolute");
			}

			// ok, done with the paths, build all waypoints:
			KmlFolder pointsFolder = new KmlFolder(this, "Trackpoints", -1, null);

			for(int i=0; i < trk.Trackpoints.Count ;i++)
			{
				Waypoint wpt = (Waypoint)trk.Trackpoints.GetByIndex(i);
				Waypoint nextWpt = (i == trk.Trackpoints.Count-1) ? null : (Waypoint)trk.Trackpoints.GetByIndex(i+1);

				if(wpt.ThumbImage == null && nextWpt != null && !wpt.isEndpoint && wpt.Location.sameAs(nextWpt.Location))
				{
					continue;
				}

				string pmName = wpt.ThumbImage == null ? Project.zuluToLocal(wpt.DateTime).ToString() : wpt.Name;
				string pmDescr = wpt.toStringKmlDescr();
				string pmStyleStartTrack = "#TrackPointStart";
				string pmStyleEndTrack = "#TrackPointEnd";
				string pmStyleArrow = "#TrackPointArrow";
				string pmStylePhoto = "#TrackPointPhoto";

				KmlPlacemark pmPoint = new KmlPlacemark(pointsFolder, pmName, -1, pmDescr);

				if(wpt.isEndpoint)
				{
					if(i == 0)
					{
						pmPoint.setStyle(pmStyleStartTrack);
					}
					else
					{
						pmPoint.setStyle(pmStyleEndTrack);
					}
				}
				else if(wpt.ThumbImage != null)
				{
					pmPoint.setStyle(pmStylePhoto);

					try 
					{
						PhotoDescr photoDescr = PhotoDescr.FromThumbnail(wpt.ThumbSource);

						photoDescr.ensureImageExists();

						ownerDoc.kmlFile.addImage(new Bitmap(photoDescr.image), wpt.Name);

						photoDescr.releaseImage();
					}
						// An invalid image will throw an OutOfMemoryException 
						// exception
					catch (OutOfMemoryException) 
					{
						LibSys.StatusBar.Error("bad image: " + wpt.ThumbSource);
					}
				}
				else
				{
					pmPoint.setStyle(pmStyleArrow);

					Color legColor = trk.color;

					if(!trk.isRoute)
					{
						// similar code it Track:Paint()
						if(Project.trackElevColor)
						{
							double elevRange = trk.ElevMax - trk.ElevMin;
							if(elevRange > 1.0d && elevRange < 20000.0d)
							{
								double elevFactor = elevRange / 256.0d;
								double elev = nextWpt.Location.Elev;
								int r = (int)((elev - trk.ElevMin) / elevFactor);
								if(r > 255)
								{
									r = 255;
								}
								int b = 255 - r;
								int g = (255 - (r > b ? r : b)) * 2;		// will be high where R and B are close to equal, amounts to cyan

								legColor = Color.FromArgb(r, g, b);
							}
						}
						else if(Project.trackSpeedColor)
						{
							double speedRange = trk.SpeedMax - trk.SpeedMin;
							if(speedRange > 1000.0d && speedRange < 1000000000.0d)
							{
								double speedFactor = speedRange / 256.0d;
								double speed = nextWpt.Speed;
								int r = (int)((speed - trk.SpeedMin) / speedFactor);
								if(r > 255)
								{
									r = 255;
								}
								int b = 255 - r;
								int g = (255 - (r > b ? r : b)) * 2;		// will be high where R and B are close to equal, amounts to cyan

								legColor = Color.FromArgb(r, g, b);
							}
						}
					}

					double heading = Math.Round(180.0d + wpt.Location.bearing(nextWpt.Location) * 180.0d / Math.PI);
					pmPoint.setArrowHeading(heading, legColor);
				}

				pmPoint.Point(wpt.Location, trksClampToGround || (wpt.Location.Elev == 0.0));
			}

		}
	}

	public class KmlWaypoint : KmlPlacemark
	{
		public KmlWaypoint(KmlFolder parent, Waypoint wpt)
			: base (parent, wpt.Name, -1, wpt.toStringKmlDescr())
		{
			string pmStyleDefault = "#Waypoint";
			string pmStylePhoto = "#WaypointPhoto";
			string pmStyleGeocache = "#Geocache";
			string pmStyleGeocacheFound = "#GeocacheFound";

			if(wpt.ThumbImage != null)
			{
				this.setStyle(pmStylePhoto);

				try 
				{
					PhotoDescr photoDescr = PhotoDescr.FromThumbnail(wpt.ThumbSource);

					photoDescr.ensureImageExists();

					ownerDoc.kmlFile.addImage(new Bitmap(photoDescr.image), wpt.Name);

					photoDescr.releaseImage();
				}
					// An invalid image will throw an OutOfMemoryException 
					// exception
				catch (OutOfMemoryException) 
				{
					LibSys.StatusBar.Error("bad image: " + wpt.ThumbSource);
				}
			}
			else if(wpt.LiveObjectType == LiveObjectTypes.LiveObjectTypeGeocache)
			{
				if(wpt.Found)
				{
					this.setStyle(pmStyleGeocacheFound);
				}
				else
				{
					this.setStyle(pmStyleGeocache);
				}
			}
			else
			{
				this.setStyle(pmStyleDefault);
			}

			bool wptClampToGround = Project.kmlOptions.WptsClampToGround || wpt.Location.Elev == 0.0d;

			XmlNode point = this.Point(wpt.Location, wptClampToGround);

			if(!wptClampToGround)
			{
				XmlNode node = point.OwnerDocument.CreateElement("extrude");
				node.InnerText = "1";
				point.AppendChild(node);
			}

		}
	}

	public class KmlEarthquake : KmlPlacemark
	{
		public KmlEarthquake(KmlFolder parent, Earthquake eq)
			: base (parent, eq.Name, -1, eq.toStringKmlDescr())
		{
			string eqStyleEqSmall = "#EarthquakeSmall";
			string eqStyleEqMedium = "#EarthquakeMedium";
			string eqStyleEqBig = "#EarthquakeBig";

			string eqStyle;

			if(eq.Magn >= 5.0d)
			{
				eqStyle = eqStyleEqBig;
			}
			else if(eq.Magn >= 3.0d)
			{
				eqStyle = eqStyleEqMedium;
			}
			else
			{
				eqStyle = eqStyleEqSmall;
			}

			this.setStyle(eqStyle);

			bool eqClampToGround = true; // eq.Location.Elev == 0.0d;

			XmlNode point = this.Point(eq.Location, eqClampToGround);

//			if(!eqClampToGround)
//			{
//				XmlNode node = point.OwnerDocument.CreateElement("extrude");
//				node.InnerText = "1";
//				point.AppendChild(node);
//			}
		}
	}

	public class KmlPlacemark : KmlElement
	{
		public KmlPlacemark(KmlFolder parent, string name, int open, string description)
			: base (parent, "Placemark", name, open, description)
		{
		}

		public void setArrowHeading(double dHeading, Color color)
		{
			XmlNode node = elementNode.OwnerDocument.CreateElement("Style");
			this.elementNode.AppendChild(node);

			XmlNode node1 = elementNode.OwnerDocument.CreateElement("IconStyle");
			node.AppendChild(node1);

			XmlNode node2 = elementNode.OwnerDocument.CreateElement("heading");
			node2.InnerText = "" + (int)dHeading;
			node1.AppendChild(node2);

			node2 = elementNode.OwnerDocument.CreateElement("color");
			string abgr = String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", color.A, color.B, color.G, color.R);
			node2.InnerText = abgr;
			node1.AppendChild(node2);

//			node2 = elementNode.OwnerDocument.CreateElement("Icon");
//			node1.AppendChild(node2);
//
//			XmlNode node3 = elementNode.OwnerDocument.CreateElement("href");
//			node3.InnerText = "root://icons/palette-5.png";
//			node2.AppendChild(node3);
//
//			node3 = elementNode.OwnerDocument.CreateElement("x");
//			node3.InnerText = "192";
//			node2.AppendChild(node3);
//
//			node3 = elementNode.OwnerDocument.CreateElement("y");
//			node3.InnerText = "224";
//			node2.AppendChild(node3);
//
//			node3 = elementNode.OwnerDocument.CreateElement("w");
//			node3.InnerText = "32";
//			node2.AppendChild(node3);
//
//			node3 = elementNode.OwnerDocument.CreateElement("h");
//			node3.InnerText = "32";
//			node2.AppendChild(node3);
		}

		// Snippet maxLines="2" 
//		public void setSnippet()
//		{
//			XmlNode node = elementNode.OwnerDocument.CreateElement("Snippet");
//			this.elementNode.AppendChild(node);
//
//			XmlAttribute attr = elementNode.OwnerDocument.CreateAttribute("maxLines");
//			attr.InnerText = "2";
//			node.Attributes.Append(attr);
//		}

		public void setStyle(string styleUrl)
		{
			XmlNode node = elementNode.OwnerDocument.CreateElement("styleUrl");
			node.InnerText = styleUrl;
			this.elementNode.AppendChild(node);
		}

		public void setColor(Color lineColor, Color polyColor)
		{
			XmlNode styleNode = elementNode.OwnerDocument.CreateElement("Style");
			this.elementNode.AppendChild(styleNode);
			XmlNode lineStyleNode = elementNode.OwnerDocument.CreateElement("LineStyle");
			styleNode.AppendChild(lineStyleNode);
			XmlNode polyStyleNode = elementNode.OwnerDocument.CreateElement("PolyStyle");
			styleNode.AppendChild(polyStyleNode);

			XmlNode node = elementNode.OwnerDocument.CreateElement("color");
			// order is ABGR
			string abgr = String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", lineColor.A, lineColor.B, lineColor.G, lineColor.R);
			node.InnerText = abgr;
			lineStyleNode.AppendChild(node);

			node = elementNode.OwnerDocument.CreateElement("color");
			abgr = String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", polyColor.A, polyColor.B, polyColor.G, polyColor.R);
			node.InnerText = abgr;
			polyStyleNode.AppendChild(node);

		}

		public XmlNode Point(GeoCoord loc, bool clampedToGround)
		{
			XmlNode ret = ownerDoc.documentNode.OwnerDocument.CreateElement("Point");
			this.elementNode.AppendChild(ret);

			XmlNode node = elementNode.OwnerDocument.CreateElement("altitudeMode");
			node.InnerText = clampedToGround ? "clampedToGround" : "absolute";
			ret.AppendChild(node);

			node = elementNode.OwnerDocument.CreateElement("coordinates");
			node.InnerText = String.Format("{0:F5},{1:F5},{2:F1}\n", loc.Lng, loc.Lat, loc.Elev);
			ret.AppendChild(node);

			return ret;
		}

		public XmlNode LineString(ArrayList points, int tesselate, int extrude, string altitudeMode)
		{
			XmlNode ret = ownerDoc.documentNode.OwnerDocument.CreateElement("LineString");
			this.elementNode.AppendChild(ret);

			XmlNode node;

			if(extrude >= 0)
			{
				node = elementNode.OwnerDocument.CreateElement("extrude");
				node.InnerText = String.Format("{0}", extrude);
				ret.AppendChild(node);
			}

			if(tesselate >= 0)
			{
				node = elementNode.OwnerDocument.CreateElement("tesselate");
				node.InnerText = String.Format("{0}", tesselate);
				ret.AppendChild(node);
			}

			node = elementNode.OwnerDocument.CreateElement("altitudeMode");
			node.InnerText = altitudeMode;
			ret.AppendChild(node);

			StringBuilder sCoords = new StringBuilder();
			foreach(CamPos cp in points)
			{
				sCoords.Append(String.Format("{0:F5},{1:F5},{2:F1}\n", cp.Lng, cp.Lat, cp.Elev));
			}

			node = elementNode.OwnerDocument.CreateElement("coordinates");
			node.InnerText = String.Format("{0}", sCoords.ToString());
			ret.AppendChild(node);

			return ret;
		}
	}

}
