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

//**************************************************************
// Inspired by GnuPG wrapper by Emmanuel KARTMANN 2002 (emmanuel@kartmann.org)
// (see freeware section at kartmann.org  article at http://www.codeproject.com/csharp/gnupgdotnet.asp )
//**************************************************************

using System;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Xml;

using LibSys;

namespace GpsBabelCmd
{
	/// <summary>
	/// This class is a wrapper class for GpsBabel. It calls CmdRunner to execute the command 
	/// line program (gpsbabel.exe) in an different process.<p/>
	/// 
	/// Please note that you must have INSTALLED GpsBabel before using this class.<p/>
	/// 
	/// This class has been developed and tested with GpsBabel v1.2.4<p/>
	/// 
	/// For more about GpsBabel, please refer to http://gpsbabel.sourceforge.net/ <br/>
	/// </summary>
	public class GpsBabelCommand
	{
		public GpsBabelCommand()
		{
			resetExecutable();
		}


		public void resetExecutable()
		{
			try
			{
				Bindirectory = (new FileInfo(Project.GpsBabelExecutable)).DirectoryName;
				Executable = Project.GpsBabelExecutable;
			}
			catch {}
		}

		/// <summary>
		/// Command property: set the type of command to execute
		/// 
		/// </summary>
		public string executable
		{
			get
			{
				return Executable;
			}
			set 
			{
				Executable = value;
			}
		}

		/// <summary>
		/// inputFileName property: set the file name to convert
		/// 
		/// <p/>Defaults to null.
		/// </summary>
		public string inputFileName
		{
			set 
			{
				InputFileName = value;
			}
		}

		/// <summary>
		/// outputFileName property: set the file name to put converted data
		/// 
		/// <p/>Defaults to null.
		/// </summary>
		public string outputFileName
		{
			set 
			{
				OutputFileName = value;
			}
		}

		/// <summary>
		/// Generate a string of GpsBabel command line arguments, based on the properties
		/// set in this object (e.g. if the <see cref="ProcessRouteInformation">ProcessRouteInformation</see> property is true, 
		/// this method generates the "-r" argument).
		/// </summary>
		/// <returns>string of GpsBabel command line arguments</returns>
		protected string BuildOptions()
		{
			StringBuilder optionsBuilder = new StringBuilder("", 255);

			if (SynthesizeShortnames)
			{
				optionsBuilder.Append("-s ");
			}

			if (ProcessWaypointsInformation)
			{
				optionsBuilder.Append("-w ");
			}

			if (ProcessTracksInformation)
			{
				optionsBuilder.Append("-t ");
			}

			if (ProcessRoutesInformation)
			{
				optionsBuilder.Append("-r ");
			}

			if (AdHoc.Length > 0)
			{
				optionsBuilder.Append(AdHoc + " ");
			}

			switch(InputIOType)
			{
				case IOType.File:
					// input file name
					if(InputFileName != null && InputFileName.Length > 0)
					{
						optionsBuilder.Append("-i " + InputFormat + (InputOptions.Length > 0 ? ("," + InputOptions) : "") + " ");

						optionsBuilder.Append("-f \"");
						optionsBuilder.Append(InputFileName);
						optionsBuilder.Append("\" ");
					}
					break;

				case IOType.Gps:
				{
					optionsBuilder.Append("-i ");

					switch(Project.gpsMakeIndex)
					{
						case 0:
							optionsBuilder.Append("garmin ");
							optionsBuilder.Append("-f " + Project.gpsPort.ToLower() + " ");
							break;
						case 1:
							optionsBuilder.Append("garmin -f usb: ");
							break;
						case 2:
							optionsBuilder.Append("magellan");
							optionsBuilder.Append(",baud=" + Project.gpsBaudRate);
							if(Project.gpsNoack)
							{
								optionsBuilder.Append(",noack");
							}
							optionsBuilder.Append(" -f " + Project.gpsPort.ToLower() + " ");
							break;
						case 3:
							optionsBuilder.Append("nmea");
							optionsBuilder.Append(",baud=" + Project.gpsBaudRate);
							optionsBuilder.Append(" -f " + Project.gpsPort.ToLower() + " ");
							break;
					}
				}
					break;
			
				case IOType.QuakeMap:
					optionsBuilder.Append("-i QuakeMap ");
					break;
			
				case IOType.Custom:
					optionsBuilder.Append("-i " + InputCustom + " ");
					break;
			}

			if (FilterOptions.Length > 0)
			{
				optionsBuilder.Append(FilterOptions + " ");
			}

			switch(OutputIOType)
			{
				case IOType.File:
					// output file name
					if(OutputFileName != null && OutputFileName.Length > 0)
					{
						optionsBuilder.Append("-o " + OutputFormat + (OutputOptions.Length > 0 ? ("," + OutputOptions) : "") + " ");

						optionsBuilder.Append("-F \"");
						optionsBuilder.Append(OutputFileName);
						optionsBuilder.Append("\" ");
					}
					break;

				case IOType.Gps:
				{
					optionsBuilder.Append("-o ");

					switch(Project.gpsMakeIndex)
					{
						case 0:
							optionsBuilder.Append("garmin ");
							optionsBuilder.Append("-F " + Project.gpsPort.ToLower() + " ");
							break;
						case 1:
							optionsBuilder.Append("garmin -F usb:");
							break;
						case 2:
							optionsBuilder.Append("magellan");
							optionsBuilder.Append(",baud=" + Project.gpsBaudRate);
							if(Project.gpsNoack)
							{
								optionsBuilder.Append(",noack");
							}
							optionsBuilder.Append(" -F " + Project.gpsPort.ToLower() + " ");
							break;
						case 3:
							optionsBuilder.Append("nmea");
							optionsBuilder.Append(",baud=" + Project.gpsBaudRate);
							optionsBuilder.Append(" -F " + Project.gpsPort.ToLower() + " ");
							break;
					}
				}
					break;
			
				case IOType.QuakeMap:
					optionsBuilder.Append("-o QuakeMap ");
					break;
			
				case IOType.Custom:
					optionsBuilder.Append("-o " + OutputCustom + " ");
					break;
			}

			return(optionsBuilder.ToString().Trim());
		}

		/// <summary>
		/// returns command string based on current state of this object. Fills the CommandString property,
		/// which will be used while executing command.
		/// </summary>
		/// <returns></returns>
		public string Evaluate()
		{
			string ret = BuildOptions();
			Args = ret;
			return ret;
		}

		private static string m_help = "";

		public string GetHelp()
		{
			if(m_help.Length == 0)
			{
				Execute("-h");

				m_help = m_result.Trim();
			}
			return m_help;
		}

		private XmlDocument m_xmlDocHelp = null;

		public XmlDocument GetHelpXml()
		{
			if(m_xmlDocHelp != null)
			{
				return m_xmlDocHelp;
			}

			GetHelp();	// make sure text is there

			string seedXml = Project.SEED_XML + "<args></args>";
			m_xmlDocHelp = new XmlDocument();
			m_xmlDocHelp.LoadXml(seedXml);

			if(m_help.Length < 100)
			{
				return m_xmlDocHelp;
			}

			XmlNode root = m_xmlDocHelp.DocumentElement;
			XmlNode curGroup = null;
			XmlNode curNode = null;

			StringReader reader = new StringReader (m_help);
				
			int state = 0;
			string line;
			int lineno = 1;
			while((line=reader.ReadLine()) != null) 
			{
				try
				{
					switch(state)
					{
						case 0:
							if(line.StartsWith("GPSBabel Version") || lineno == 1)
							{
								LibSys.StatusBar.Trace(line);
							}
							else if(line.StartsWith("File Types")) 
							{
								curGroup = Project.SetValue(m_xmlDocHelp, root, "filetypes", "");
								state = 10;
							} 
							break;
						case 10:
							if(line.StartsWith("Supported data filters")) 
							{
								curGroup = Project.SetValue(m_xmlDocHelp, root, "datafilters", "");
								state = 100;
							}
							if(line.StartsWith("\t"))
							{
								line = line.Substring(1);

								if(line.StartsWith("  "))
								{
									line = line.Substring(2);
									
									if(curNode != null)
									{
										XmlNode node = Project.SetValue(m_xmlDocHelp, curNode, "param", "");

										parseArgLine(line, m_xmlDocHelp, node);
									}
								}
								else
								{
									curNode = Project.SetValue(m_xmlDocHelp, curGroup, "option", "");

									parseArgLine(line, m_xmlDocHelp, curNode);
								}
							}
							break;
						case 100:
							if(line.StartsWith("\t"))
							{
								line = line.Substring(1);

								if(line.StartsWith("  "))
								{
									line = line.Substring(2);
									
									if(curNode != null)
									{
										XmlNode node = Project.SetValue(m_xmlDocHelp, curNode, "param", "");

										parseArgLine(line, m_xmlDocHelp, node);
									}
								}
								else
								{
									curNode = Project.SetValue(m_xmlDocHelp, curGroup, "option", "");

									parseArgLine(line, m_xmlDocHelp, curNode);
								}
							}
							break;
					}
				}
				catch {}
				lineno++;
			}

			return m_xmlDocHelp;
		}

		private bool parseArgLine(string line, XmlDocument xmlDoc, XmlNode node)
		{
			bool ret = true;
			string lineL = line.ToLower();

			string valueType = "";
			bool required = lineL.IndexOf("(required)") >= 0;

			int pos = line.IndexOf(" ");
			string option = line.Substring(0, pos);

			XmlAttribute attr = xmlDoc.CreateAttribute("name");
			attr.InnerText = option;
			node.Attributes.Append(attr);

			string descr = line.Substring(pos+1).Trim();

			if(required)
			{
				descr = descr.Replace("(required)", "").Trim();

				attr = xmlDoc.CreateAttribute("required");
				attr.InnerText = "true";
				node.Attributes.Append(attr);
			}

			if(!descr.StartsWith("Sort by") && !descr.StartsWith("Suppress") || descr.IndexOf("higher") > 0 )
			{
				if(line.IndexOf("(0/1)") >= 0)
				{
					descr = descr.Replace("(0/1)", "").Trim();
					valueType = "bool";
				}
				else if("depth".Equals(option) || lineL.IndexOf(" length") >= 0 || descr.StartsWith("Length") || lineL.IndexOf(" in pixels") >= 0
					|| descr.IndexOf("DDDD") >= 0 || descr.IndexOf("Maximum") >= 0 || option.Equals("maxcount")
					|| "hdop".Equals(option) || "vdop".Equals(option)
					)
				{
					valueType = "int";
				}
				else if(lineL.IndexOf("path") >= 0 || lineL.IndexOf("(filename)") >= 0 || lineL.IndexOf("file containing") >= 0)
				{
					valueType = "path";
				}
				else if( line.IndexOf("Include short name ") == -1
					&& (lineL.IndexOf(" name") >= 0 || lineL.IndexOf("prepended") >= 0
					|| lineL.IndexOf("base url") >= 0 || lineL.IndexOf("'auto'") >= 0 || descr.StartsWith("Version")
					|| lineL.IndexOf("degrees") >= 0 || lineL.IndexOf("percentage") >= 0
					|| lineL.IndexOf("days after") >= 0 || lineL.IndexOf("marker type ") >= 0
					|| lineL.IndexOf("move") >= 0 || lineL.IndexOf("title") >= 0 || lineL.IndexOf("start") >= 0 || lineL.IndexOf("stop") >= 0
					))
				{
					valueType = "string";
				}
			}

			if(valueType.Length > 0)
			{
				attr = xmlDoc.CreateAttribute("valuetype");
				attr.InnerText = valueType;
				node.Attributes.Append(attr);
			}

			attr = xmlDoc.CreateAttribute("descr");
			attr.InnerText = descr.Trim();
			node.Attributes.Append(attr);

			return ret;
		}


		/// <summary>
		/// Execute the GpsBabel command defined by all parameters/options/properties.
		/// 
		/// <p/>Raise a GpsBabelException whenever an error occurs.
		/// </summary>
		/// <param name="inputText"></param>
		/// <param name="outputText"></param>
		public void Execute(string args)
		{
			string tempFileInput = null;
			string tempFileOutput = null;
			bool quakeMapOutput = false;
			Exception exc = null;

			if(args != null && args.IndexOf("-i QuakeMap") != -1)
			{
				tempFileInput = GetTempFileName();
				args = args.Replace("-i QuakeMap", "-i gpx -f \"" + tempFileInput + "\"");

				// run QuakeMap to dump required info into tempFileInput
				try
				{
					QMApiLib qmApiLib = new QMApiLib();

					qmApiLib.exportToFile(tempFileInput);
				}
				catch (Exception ee)
				{
					LibSys.StatusBar.Error(ee.Message);
					try
					{
						File.Delete(tempFileInput);
					} 
					catch {}
					throw(ee);
				}
			}

			if(args != null && args.IndexOf("-o QuakeMap") != -1)
			{
				quakeMapOutput = true;
				tempFileOutput = GetTempFileName();
				args = args.Replace("-o QuakeMap", "-o gpx -F \"" + tempFileOutput + "\"");
			}

			CmdRunner runner = new CmdRunner();

			string outputText;
			runner.executeCommand(Executable, args == null ? Args : args, Bindirectory, null, out outputText);
			m_result = runner.OutputString;
			m_error = runner.ErrorString;

			if(runner.exitcode == 0 && quakeMapOutput)
			{
				try
				{
					QMApiLib qmApiLib = new QMApiLib();

					qmApiLib.resetZoom();		// prepare for a zoom into the file
					switch(MapType)
					{
						case MapType.Aerial:
							qmApiLib.CommandMappingEngine("/map=aerial");
							break;
						case MapType.ColorAerial:
							qmApiLib.CommandMappingEngine("/map=color");
							break;
						case MapType.Topo:
							qmApiLib.CommandMappingEngine("/map=topo");
							break;
						case MapType.None:
							qmApiLib.CommandMappingEngine("/map=none");
							break;
					}
					qmApiLib.CommandMappingEngine(tempFileOutput);
				}
				catch (Exception ee)
				{
					LibSys.StatusBar.Error(ee.Message);
					exc = ee;
				}
			}

			try
			{
				File.Delete(tempFileInput);
			} 
			catch {}

			try
			{
				File.Delete(tempFileOutput);
			} 
			catch {}

			if(exc != null)
			{
				throw exc;
			}
		}

		private string GetTempFileName()
		{
			string tempPath = Path.GetTempPath();
			string name = System.Guid.NewGuid().ToString() + ".gpx";
			return Path.Combine(tempPath, name);
		}

		// Variables used to store property values:
		public string Executable = "";
		public string Args = "";
		public string Bindirectory = "";

		public IOType InputIOType;
		public IOType OutputIOType;
		public MapType MapType = MapType.Undefined;
		public string InputFileName = "";
		public string InputFormat = "";
		public string InputOptions = "";
		public string InputCustom = "";

		public string OutputFileName = "";
		public string OutputFormat = "";
		public string OutputOptions = "";
		public string OutputCustom = "";

		public string FilterOptions = "";

		private string m_result = "";
		public string ResultString { get { return m_result.Length == 0 && m_error.Length == 0 ? "OK: command succeded" : m_result; } }

		private string m_error = "";
		public string ErrorString { get { return m_error; } }

		// properties:
		public string AdHoc = "";
		public bool ProcessWaypointsInformation;
		public bool ProcessTracksInformation;
		public bool ProcessRoutesInformation;
		public bool SynthesizeShortnames;
	}

	public enum IOType
	{
		File, Gps, QuakeMap, Custom
	}

	public enum MapType
	{
		Undefined, Aerial, ColorAerial, Topo, None
	}
}
