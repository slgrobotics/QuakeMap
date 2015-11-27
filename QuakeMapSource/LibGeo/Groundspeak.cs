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
using System.Xml;

using LibSys;

namespace LibGeo
{
	/// <summary>
	/// container for Groundspeak Log info, to be placed in list associated with GroundspeakCache.
	/// </summary>
	public class GroundspeakLog
	{
		/*
		  <groundspeak:log id="2505715">
			<groundspeak:date>2003-11-29T08:00:00</groundspeak:date> 
			<groundspeak:type>Found it</groundspeak:type> 
			<groundspeak:finder id="2237">MLesko0825</groundspeak:finder> 
			<groundspeak:text encoded="False">A great Thanksgiving project. Everyone solved a peice of the puzzle.
					Located the cache, enjoyed the history, took nothing and left nothing. Signed the log.
						Thanks Dr. Bob! M&M from AZ and Geo-poodle</groundspeak:text> 
	        <groundspeak:log_wpt lat="33.467567" lon="-117.65435" />
		  </groundspeak:log>
		 */
		public int id = -1;
		public DateTime dateTime;
		public string type = "";							// "Found it"
		public int finderId = -1;
		public string finder = "";
		public bool textEncoded = false;
		public string text = "";
		public double logWptLat = 0.0f;
		public double logWptLon = 0.0f;

		public GroundspeakLog(XmlNode node)
		{
			XmlAttribute aattr = node.Attributes["id"];
			if(aattr != null)
			{
				try
				{
					id = Convert.ToInt32(aattr.InnerText);
				} 
				catch {}
			}
			foreach(XmlNode nnode in node.ChildNodes)
			{
				try 
				{
					switch(nnode.Name)
					{
						case "groundspeak:date":
							dateTime = Convert.ToDateTime(nnode.InnerText);
							break;
						case "groundspeak:type":
							type = nnode.InnerText.Trim();
							break;
						case "groundspeak:finder":
						{
							XmlAttribute attr = nnode.Attributes["id"];
							if(attr != null)
							{
								finderId = Convert.ToInt32(attr.InnerText.Trim());
							}
							finder = nnode.InnerText.Trim();
						}
							break;
						case "groundspeak:text":
						{
							XmlAttribute attr = nnode.Attributes["encoded"];
							if(attr != null)
							{
								textEncoded = "true".Equals(attr.InnerText.Trim().ToLower());
							}
							text = nnode.InnerText.Trim();
						}
							break;
						case "groundspeak:log_wpt":
						{
							XmlAttribute attr = nnode.Attributes["lat"];
							if(attr != null)
							{
								logWptLat = Convert.ToDouble(attr.InnerText);
							}
							attr = nnode.Attributes["lon"];
							if(attr != null)
							{
								logWptLon = Convert.ToDouble(attr.InnerText);
							}
						}
							break;
					}
				}
				catch (Exception ee) 
				{
					// bad node - not a big deal...
					LibSys.StatusBar.Error("GroundspeakLog gpx node=" + nnode.Name + " " + ee.Message);
				}
			}
		}

		/// <summary>
		/// returns a formatted string suitable for popups
		/// </summary>
		/// <returns></returns>
		public string ToStringPopup()
		{
			string ret = " -- " + type;

			ret += " on: " + Project.zuluToLocal(dateTime).ToShortDateString();
			ret += " by: " + finder;
			if(logWptLat != 0.0f || logWptLon != 0.0f)
			{
				GeoCoord loc = new GeoCoord(logWptLon, logWptLat);
				ret += " at: " + loc.ToString();
			}

			return ret;
		}

		/// <summary>
		/// used to export GPX files
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <returns></returns>
		public XmlNode ToGpxXmlNode(XmlDocument xmlDoc)
		{
			string prefix = "groundspeak";
			string namespaceURI = "http://www.groundspeak.com/cache/1/0";

			XmlNode ret = xmlDoc.CreateElement(prefix, "log", namespaceURI);

			/*
				<groundspeak:log id="2608186">
				<groundspeak:date>2003-12-27T08:00:00</groundspeak:date>
				<groundspeak:type>Found it</groundspeak:type>
				<groundspeak:finder id="180195">Team Geo-Rangers</groundspeak:finder>
				<groundspeak:text encoded="False">Logged this one while sitting in the bar of the Bomb Shelter!
									  Very cool place for a virtual.  Will be back for sure!</groundspeak:text>
		        <groundspeak:log_wpt lat="33.467567" lon="-117.65435" />
				</groundspeak:log>
			*/

			XmlAttribute attr = xmlDoc.CreateAttribute("id");
			attr.InnerText = "" + this.id;
			ret.Attributes.Append(attr);

			Project.SetValue(xmlDoc, ret, prefix, "date", namespaceURI, this.dateTime.ToString(Project.dataTimeFormat));
			Project.SetValue(xmlDoc, ret, prefix, "type", namespaceURI, this.type.Trim());
			XmlNode cur = Project.SetValue(xmlDoc, ret, prefix, "finder", namespaceURI, this.finder.Trim());
			attr = xmlDoc.CreateAttribute("id");
			attr.InnerText = "" + finderId;
			cur.Attributes.Append(attr);
			cur = Project.SetValue(xmlDoc, ret, prefix, "text", namespaceURI, this.text.Trim());
			attr = xmlDoc.CreateAttribute("encoded");
			attr.InnerText = textEncoded ? "True" : "False";
			cur.Attributes.Append(attr);
			if(logWptLat != 0.0f || logWptLon != 0.0f)
			{
				cur = Project.SetValue(xmlDoc, ret, prefix, "log_wpt", namespaceURI, null);
				attr = xmlDoc.CreateAttribute("lat");
				attr.InnerText = "" + logWptLat;
				cur.Attributes.Append(attr);
				attr = xmlDoc.CreateAttribute("lon");
				attr.InnerText = "" + logWptLon;
				cur.Attributes.Append(attr);
			}

			return ret;
		}
	}

	/// <summary>
	/// container for Groundspeak Travel Bug info, to be placed in list associated with GroundspeakCache.
	/// </summary>
	public class GroundspeakTravelbug
	{
		/*
		  <groundspeak:travelbugs>
			<groundspeak:travelbug id="19449" ref="TB4BF9">
			  <groundspeak:name>One Feather</groundspeak:name> 
			</groundspeak:travelbug>
		  </groundspeak:travelbugs>
		*/
		public int id = -1;
		public string refr = "";
		public string name = "";

		public GroundspeakTravelbug(XmlNode node)
		{
			XmlAttribute aattr = node.Attributes["id"];
			if(aattr != null)
			{
				try
				{
					id = Convert.ToInt32(aattr.InnerText);
				} 
				catch {}
			}
			aattr = node.Attributes["ref"];
			if(aattr != null)
			{
				refr = aattr.InnerText.Trim();
			}
			foreach(XmlNode nnode in node.ChildNodes)
			{
				try
				{
					switch(nnode.Name)
					{
						case "groundspeak:name":
							name = nnode.InnerText.Trim();
							break;
					}
				}
				catch (Exception ee) 
				{
					// bad node - not a big deal...
					LibSys.StatusBar.Error("GroundspeakTravelbug gpx node=" + nnode.Name + " " + ee.Message);
				}
			}
		}

		/// <summary>
		/// returns a formatted string suitable for popups
		/// </summary>
		/// <returns></returns>
		public string ToStringPopup()
		{
			string ret = " -- " + name;

			ret += " id: " + id;
			ret += " ref: " + refr;

			return ret;
		}

		/// <summary>
		/// used to export GPX files
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <returns></returns>
		public XmlNode ToGpxXmlNode(XmlDocument xmlDoc)
		{
			string prefix = "groundspeak";
			string namespaceURI = "http://www.groundspeak.com/cache/1/0";

			XmlNode ret = xmlDoc.CreateElement(prefix, "travelbug", namespaceURI);

			/*
				<groundspeak:travelbug id="5619" ref="TB15F3">
				<groundspeak:name>Alaska to California Keyring</groundspeak:name>
				</groundspeak:travelbug>
			 */

			XmlAttribute attr;
			if(id != -1)
			{
				attr = xmlDoc.CreateAttribute("id");
				attr.InnerText = "" + this.id;
				ret.Attributes.Append(attr);
			}
			if(refr != null && refr.Length > 0)
			{
				attr = xmlDoc.CreateAttribute("ref");
				attr.InnerText = "" + this.refr;
				ret.Attributes.Append(attr);
			}

			Project.SetValue(xmlDoc, ret, prefix, "name", namespaceURI, this.name.Trim());

			return ret;
		}
	}


	/// <summary>
	/// container for <groundspeak:cache/> extentions of GPX format (coming in PocketQuery files).
	/// </summary>
	public class GroundspeakCache
	{
		public int  id = -1;
		public bool available = false;
		public bool archived = false;
		public string name = "";
		public string placedBy = "";
		public int ownerId = -1;
		public string owner = "";
		public string type = "";						// "Traditional Cache"
		public string container = "";
		public float difficulty = 0.0f;					// 1 to 5, can be 1.5
		public float terrain = 0.0f;					// 1 to 5, can be 1.5
		public string country = "";						// "United States"
		public string state = "";						// "California"
		public bool isShortDescriptionHtml = false;		// if short description is HTML
		public string shortDescription = "";
		public bool isLongDescriptionHtml = false;
		public string longDescription = "";
		public string encodedHints = "";
		public ArrayList logs = new ArrayList();		// of GroundspeakLog
		public ArrayList travelbugs = new ArrayList();	// of GroundspeakTravelbug

		public GroundspeakCache(XmlNode node)
		{
			XmlAttribute aattr = node.Attributes["id"];
			if(aattr != null)
			{
				try
				{
					id = Convert.ToInt32(aattr.InnerText);
				} 
				catch {}
			}
			aattr = node.Attributes["available"];
			if(aattr != null)
			{
				available = "true".Equals(aattr.InnerText.Trim().ToLower());
			}
			aattr = node.Attributes["archived"];
			if(aattr != null)
			{
				archived = "true".Equals(aattr.InnerText.Trim().ToLower());
			}
			foreach(XmlNode nnode in node.ChildNodes)
			{
				try 
				{
					switch(nnode.Name)
					{
						case "groundspeak:name":
							name = nnode.InnerText.Trim();
							break;
						case "groundspeak:placed_by":
							placedBy = nnode.InnerText.Trim();
							break;
						case "groundspeak:owner":
						{
							XmlAttribute attr = nnode.Attributes["id"];
							if(attr != null)
							{
								try
								{
									ownerId = Convert.ToInt32(attr.InnerText);
								} 
								catch {}
							}
							owner = nnode.InnerText.Trim();
						}
							break;
						case "groundspeak:type":
							type = nnode.InnerText.Trim();
							break;
						case "groundspeak:container":
							container = nnode.InnerText.Trim();
							break;
						case "groundspeak:difficulty":
							difficulty = (float)Convert.ToDouble(nnode.InnerText);
							break;
						case "groundspeak:terrain":
							terrain = (float)Convert.ToDouble(nnode.InnerText);
							break;
						case "groundspeak:country":
							country = nnode.InnerText.Trim();
							break;
						case "groundspeak:state":
							state = nnode.InnerText.Trim();
							break;
						case "groundspeak:short_description":
						{
							XmlAttribute attr = nnode.Attributes["html"];
							if(attr != null)
							{
								isShortDescriptionHtml = "true".Equals(attr.InnerText.Trim().ToLower());
							}
							shortDescription = nnode.InnerText.Trim();
						}
							break;
						case "groundspeak:long_description":
						{
							XmlAttribute attr = nnode.Attributes["html"];
							if(attr != null)
							{
								isLongDescriptionHtml = "true".Equals(attr.InnerText.Trim().ToLower());
							}
							longDescription = nnode.InnerText.Trim();
						}
							break;
						case "groundspeak:encoded_hints":
							encodedHints = nnode.InnerText.Trim();
							break;
						case "groundspeak:logs":
							//ret += "         logs: " + nnode.ChildNodes.Count + "\n";
							foreach(XmlNode nnnode in nnode.ChildNodes)
							{
								switch(nnnode.Name)
								{
									case "groundspeak:log":
										GroundspeakLog gsl = new GroundspeakLog(nnnode);
										logs.Add(gsl);
										break;
								}
							}
							break;
						case "groundspeak:travelbugs":
							//ret += "         travelbugs: " + nnode.ChildNodes.Count + "\n";
							foreach(XmlNode nnnode in nnode.ChildNodes)
							{
								switch(nnnode.Name)
								{
									case "groundspeak:travelbug":
										GroundspeakTravelbug gtb = new GroundspeakTravelbug(nnnode);
										travelbugs.Add(gtb);
										break;
								}
							}
							break;
					}
				}
				catch (Exception ee) 
				{
					// bad node - not a big deal...
					LibSys.StatusBar.Error("GroundspeakCache gpx node=" + nnode.Name + " " + ee.Message);
				}
			}
		}

		/// <summary>
		/// returns a formatted string suitable for popups
		/// </summary>
		/// <returns></returns>
		public string ToStringPopup()
		{
			string ret = type;

			ret += "   id:" + id + "  available:" + (available ? "Yes" : "No") + "  archived:" + (archived ? "Yes" : "No");
			ret += "\nname: " + name;
			ret += "\nplaced by: " + placedBy;
			ret += "\ncontainer: " + container;
			ret += "\nlogs: " + logs.Count;
			foreach (GroundspeakLog gsl in logs)
			{
				ret += "\n" + gsl.ToStringPopup();
			}
			ret += "\ntravel bugs: " + travelbugs.Count;
			foreach (GroundspeakTravelbug gtb in travelbugs)
			{
				ret += "\n" + gtb.ToStringPopup();
			}

			return ret;
		}

		/// <summary>
		/// used to export GPX files
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <returns></returns>
		public XmlNode ToGpxXmlNode(XmlDocument xmlDoc)
		{
			string prefix = "groundspeak";
			string namespaceURI = "http://www.groundspeak.com/cache/1/0";

			XmlNode ret = xmlDoc.CreateElement(prefix, "cache", namespaceURI);

			/*
				<groundspeak:cache id="48075" available="True" archived="False" xmlns:groundspeak="http://www.groundspeak.com/cache/1/0">
				<groundspeak:name>The Canyon Lake Experience</groundspeak:name>
				<groundspeak:placed_by>trvlnmn</groundspeak:placed_by>
				<groundspeak:owner id="87425">trvlnmn</groundspeak:owner>
				<groundspeak:type>Traditional Cache</groundspeak:type>
				<groundspeak:container>Regular</groundspeak:container>
				<groundspeak:difficulty>2</groundspeak:difficulty>
				<groundspeak:terrain>5</groundspeak:terrain>
				<groundspeak:country>United States</groundspeak:country>
				<groundspeak:state>California</groundspeak:state>
				<groundspeak:short_description html="False">Overlooking Canyon Lake</groundspeak:short_description>
				<groundspeak:long_description html="False">This is a 3.6 mile hike on foot one way (from where I parked). It's 2.5 miles as a crow flies. 
					 Two wheel or 4x4 is stongly recommended.  It's only a fifteen minute ride on a dirtbike.
					 You can park right next to it with a 4x4 jeep, just bring a paper bag for the whoops on the road.
					 You can enter from several locations, including from inside Canyon Lake.  You do not need to live there to find this cache.
					 Just park at these coordinates  N33&amp;deg 44.871  W117&amp;deg 14.566
					 Ammo Box containing several toys.  Feel free to add a log book, but online entering is fine.</groundspeak:long_description>
				<groundspeak:encoded_hints>
				</groundspeak:encoded_hints>
				<groundspeak:logs />
			    <groundspeak:travelbugs />
			*/

			XmlAttribute attr = xmlDoc.CreateAttribute("id");
			attr.InnerText = "" + this.id;
			ret.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("available");
			attr.InnerText = this.available ? "True" : "False";
			ret.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("archived");
			attr.InnerText = this.archived ? "True" : "False";
			ret.Attributes.Append(attr);

			Project.SetValue(xmlDoc, ret, prefix, "name", namespaceURI, this.name.Trim());
			Project.SetValue(xmlDoc, ret, prefix, "placed_by", namespaceURI, this.placedBy.Trim());
			XmlNode cur = Project.SetValue(xmlDoc, ret, prefix, "owner", namespaceURI, this.owner.Trim());
			attr = xmlDoc.CreateAttribute("id");
			attr.InnerText = "" + ownerId;
			cur.Attributes.Append(attr);
			Project.SetValue(xmlDoc, ret, prefix, "type", namespaceURI, this.type.Trim());
			Project.SetValue(xmlDoc, ret, prefix, "container", namespaceURI, this.container.Trim());
			Project.SetValue(xmlDoc, ret, prefix, "difficulty", namespaceURI, "" + this.difficulty);
			Project.SetValue(xmlDoc, ret, prefix, "terrain", namespaceURI, "" + this.terrain);
			Project.SetValue(xmlDoc, ret, prefix, "country", namespaceURI, this.country.Trim());
			Project.SetValue(xmlDoc, ret, prefix, "state", namespaceURI, this.state.Trim());
			cur = Project.SetValue(xmlDoc, ret, prefix, "short_description", namespaceURI, this.shortDescription.Trim());
			attr = xmlDoc.CreateAttribute("html");
			attr.InnerText = isShortDescriptionHtml ? "True" : "False";
			cur.Attributes.Append(attr);
			cur = Project.SetValue(xmlDoc, ret, prefix, "long_description", namespaceURI, this.longDescription.Trim());
			attr = xmlDoc.CreateAttribute("html");
			attr.InnerText = isLongDescriptionHtml ? "True" : "False";
			cur.Attributes.Append(attr);
			// Encoded hints use Rot13 encryption on Geocaching.com but displayed in cleartext here 
			// Text enclosed in brackets [] are not encrypted 
			Project.SetValue(xmlDoc, ret, prefix, "encoded_hints", namespaceURI, this.encodedHints.Trim());
			//if(logs.Count > 0)
			{
				XmlNode logsNode = Project.SetValue(xmlDoc, ret, prefix, "logs", namespaceURI, "");
				foreach (GroundspeakLog gsl in logs)
				{
					XmlNode node = gsl.ToGpxXmlNode(xmlDoc);
					logsNode.AppendChild(node);
				}
				ret.AppendChild(logsNode);
			}

			//if(travelbugs.Count > 0)
			{
				XmlNode travelbugsNode = Project.SetValue(xmlDoc, ret, prefix, "travelbugs", namespaceURI, "");
				foreach (GroundspeakTravelbug gtb in travelbugs)
				{
					XmlNode node = gtb.ToGpxXmlNode(xmlDoc);
					travelbugsNode.AppendChild(node);
				}
				ret.AppendChild(travelbugsNode);
			}

			return ret;
		}
	}
}
