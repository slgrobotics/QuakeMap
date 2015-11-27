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
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Xml;

namespace LibSys
{
	public class ToolDescr
	{
		public string displayName;
		public string executablePath;	// if null - run in browser
		public string arguments;

		public ToolDescr()
		{
		}

		public ToolDescr(XmlNode nnode)
		{
			foreach(XmlNode node in nnode)
			{
				if(node.Name.Equals("displayName")) 
				{
					displayName = node.InnerText.Trim();
				}
				else if(node.Name.Equals("executablePath")) 
				{
					executablePath = node.InnerText.Trim();
				}
				else if(node.Name.Equals("arguments")) 
				{
					arguments = node.InnerText.Trim();
				}
			}
		}

		public override string ToString()
		{
			return displayName;
		}

		public XmlNode ToXml(XmlDocument xmlDoc)
		{
			XmlNode ret = xmlDoc.CreateElement("tool");
			XmlNode node = xmlDoc.CreateElement("displayName");
			node.InnerText = "" + displayName;
			ret.AppendChild(node);
			if(this.executablePath != null)
			{
				node = xmlDoc.CreateElement("executablePath");
				node.InnerText = executablePath;
				ret.AppendChild(node);
			}
			if(this.arguments != null && this.arguments.Length > 0)
			{
				node = xmlDoc.CreateElement("arguments");
				node.InnerText = arguments;
				ret.AppendChild(node);
			}
			return ret;
		}

		public string substituteArgs(double lat, double lon, string name, string guid)
		{
			string args = this.arguments;
			if(args != null)
			{
				args = args.Replace("%lat%", "" + lat);
				args = args.Replace("%lon%", "" + lon);
				if(name != null)
				{
					args = args.Replace("%waypoint%", name);
				}
				if(guid != null)
				{
					args = args.Replace("%guid%", guid);
				}

				// figure out what the latest recent gpx or loc file was:
				foreach(string fileName in Project.recentFiles)
				{
					if(File.Exists(fileName))
					{
						if(args.IndexOf("%file%") >= 0 && (fileName.ToLower().EndsWith(".gpx") || fileName.ToLower().EndsWith(".loc")))
						{
							args = args.Replace("%file%", fileName);
							break;
						}
						if(args.IndexOf("%filegpx%") >= 0 && fileName.ToLower().EndsWith(".gpx"))
						{
							args = args.Replace("%filegpx%", fileName);
							break;
						}
						if(args.IndexOf("%fileloc%") >= 0 && fileName.ToLower().EndsWith(".loc"))
						{
							args = args.Replace("%fileloc%", fileName);
							break;
						}
					}
				}
			}
			else
			{
				args = "";
			}
			return args;
		}

		public void run(double lat, double lon, string name, string guid)
		{
			string args = substituteArgs(lat, lon, name, guid);

			LibSys.StatusBar.Trace("Run Tool: '" + this.executablePath + "'  args: '" + args + "'");
			if(this.executablePath == null)
			{
				Project.RunBrowser("\"" + args + "\"");
			}
			else
			{
				if(File.Exists(executablePath))
				{
					System.Diagnostics.ProcessStartInfo pInfo = new	ProcessStartInfo(executablePath, args);

					Process p = new Process();
					p.StartInfo = pInfo;
					p.Start();
				}
				else
				{
					Project.ErrorBox(null, "Could not find file " + executablePath + "\n\nInstall the Tool, or use \"Tools-->Manage\" and correct the Executable for " + this.displayName);
				}
			}
		}
	}

	/// <summary>
	/// Tools collection stores all external tools defined for the "Tools" menu.
	/// </summary>
	public class Tools
	{
		public ArrayList tools = new ArrayList();
		public bool isDirty = false;

		public Tools()
		{
		}

		public void Save()
		{
			//LibSys.StatusBar.Trace("Tools:Save: " + isDirty);
			if(isDirty)
			{
				try 
				{
					string toolsFilePath = Project.GetMiscPath(Project.TOOLS_FILE_NAME);
					string seedXml = Project.SEED_XML + "<tools></tools>";
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(seedXml);

					XmlNode root = xmlDoc.DocumentElement;

					Project.SetValue(xmlDoc, root, "time", "" + DateTime.Now);

					foreach(ToolDescr tool in tools)
					{
						XmlNode node = tool.ToXml(xmlDoc);
						root.AppendChild(node);
					}

					xmlDoc.Save(toolsFilePath);
					isDirty = false;
				}
				catch (Exception e) 
				{
					LibSys.StatusBar.Error("Tools:Save() " + e.Message);
				}
			}
		}

		public void Restore(string toolsFilePath)
		{
			tools.Clear();

			try 
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(toolsFilePath);

				foreach(XmlNode nnode in xmlDoc.ChildNodes) 
				{
					if(nnode.Name.Equals("tools")) 
					{
						foreach(XmlNode node in nnode)
						{
							if(node.Name.Equals("tool")) 
							{
								try 
								{
									ToolDescr td = new ToolDescr(node);
									tools.Add(td);
								}
								catch (Exception ee) 
								{
									// bad node - not a big deal...
									LibSys.StatusBar.Error("Tools:Restore() " + ee.Message);
								}
							}
						}
					}
				}
				isDirty = false;
			}
			catch (FileNotFoundException e) 
			{
				LibSys.StatusBar.Trace("OK: " + e.Message);
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("Tools:Restore() " + e.Message);
			}

			if(tools.Count == 0)
			{
				// populate it with default set:

				ToolDescr td = new ToolDescr();
				td.displayName = "Microsoft Streets&Trips";
				td.executablePath = Project.driveSystem + "Program Files\\Microsoft Streets and Trips\\Streets.exe";
				td.arguments = "";
				tools.Add(td);

				td = new ToolDescr();
				td.displayName = "Google street level map";
				td.executablePath = null;	// browser type
				td.arguments = "http://maps.google.com/maps?ll=%lat%,%lon%&z=4";
//							"http://www.mapquest.com/maps/map.adp?size=big&zoom=9&latlongtype=decimal&latitude=%lat%&longitude=%lon%";
				tools.Add(td);

				td = new ToolDescr();
                td.displayName = "MSR Maps map";
				td.executablePath = null;	// browser type
				td.arguments = "http://terraserver.homeadvisor.msn.com/addressimage.aspx?t=4&s=8&Lon=%lon%&Lat=%lat%&w=2&opt=0";
				tools.Add(td);

				td = new ToolDescr();
				td.displayName = "GPSBabelWrapper";
				td.executablePath = Project.driveSystem + "Program Files\\VitalBytes\\GpsBabelWrapper\\bin\\gpsbabelwrapper.exe";
				td.arguments = "";
				tools.Add(td);

				td = new ToolDescr();
				td.displayName = "GSAK";
				td.executablePath = Project.driveSystem + "Program Files\\GSAK\\gsak.exe";
				td.arguments = "\"%file%\"";
				tools.Add(td);

				td = new ToolDescr();
				td.displayName = "Geocache By Waypoint Name";
				td.executablePath = null;
				td.arguments = "http://www.geocaching.com/seek/cache_details.aspx?pf=y&wp=%waypoint%&decrypt=y&log=y&numlogs=10";
				tools.Add(td);

				td = new ToolDescr();
				td.displayName = "Geocache By GUID";
				td.executablePath = null;
				td.arguments = "http://www.geocaching.com/seek/cache_details.aspx?pf=y&guid=%guid%&decrypt=y&log=y&numlogs=10";
				tools.Add(td);

				isDirty = true;
				//this.Save(toolsFilePath);
			}
		}
	}
}
