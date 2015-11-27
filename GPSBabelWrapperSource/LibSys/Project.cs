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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using Microsoft.Win32;		// for registry

namespace LibSys
{
	public sealed class Project
	{
		// ********* modify parameters below for release builds:  *******************

#if DEBUG
		public static bool enableTrace = true;
#endif

		public const string		PROGRAM_MAIN_SERVER = "www.earthquakemap.com";

		public const string     PROGRAM_NAME_LOGICAL = "gpsbabelwrapper";		// used for making URLs on the servers
		public const string     PROGRAM_NAME_HUMAN = "GPSBabelWrapper";	        // used for title in the frame etc.
		public const string     PROGRAM_VERSION_HUMAN = "1.2";					// used for greeting.
		public const string     WEBSITE_NAME_HUMAN = "QuakeMap.com";			// used for watermark printing etc.

		public const string     PROGRAM_VERSION_RELEASEDATE = "20060520";

		public const string     CGIBINPTR_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/cgibinptr.html";

		// supporting files (ico, gif, txt) are loaded from this URL's folder (TILERMISC in cgibinptr):
		public static string	MISC_FOLDER_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/misc";

		// may be overridden by TILERHELP="..." in cgibinptr.html:
		public static string	ABOUT_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/about.html";
		public static string	GPSBABEL_URL = "http://www.gpsbabel.org";
		public static string	UPDATE_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/update.html";
		public static string	HELP_FILE_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/help.chm";
		public const string     HELP_FILE_PATH = "help.chm";
		public static DateTime	HELP_FILE_DATE = DateTime.MinValue;		// comes from cgibinptr.html
		public const int		LICENSE_FILE_MAX_AGE_DAYS = 10;

		public static string	DLOAD_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/download.html";
		public static string	REPORT_BUG_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/reportbug.html";

		// The following are filled with paths relative to current directory in Project():
		public static string	startupPath;
		private static string   miscFolderPath;
        public static string    driveProgramInstalled = "C:\\";

		public const string OPTIONS_STORE_NAME = "options";

		public static string serverMessage = "";	// see TileCache.cs
		public static string upgradeMessage = "";	// see TileCache.cs

		public const string		SEED_XML = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"yes\"?>";

		public const string		OPTIONS_FILE_NAME	= "gboptions.xml";

		//public const string		iniFileName = "gpsbabelwrapper.ini";	// make sure installation script makes this one

		// -----------------  global-use variables, flags, etc:  --------------------------------------------

		public static bool goingDown = false;
		public static Form mainForm;					// we need parent form for message boxes
		public static string traceToken = "";
		public static string serverMessageLast = "";	// last displayed server message to compare with for pop-up.

		public static bool mainFormMaximized = false;
		public static int  mainFormWidth = 0;
		public static int  mainFormHeight = 0;
		public static int  mainFormX = 0;
		public static int  mainFormY = 0;

		public static bool useProxy	 = false;
		public static bool suspendKeepAlive	 = false;
		public static bool forceEmptyProxy	 = false;
		public static string proxyServer = "myproxyservernameoripaddress";
		public static int proxyPort = 8080;
		public static int webTimeoutMs = 7000;

		public static bool serverAvailable = false;				// couldn't reach cgibinptr.

		public static ArrayList recentFiles = new ArrayList();			// list of string (file or folder names)
		public static int recentFilesFirstIndex;						// menu item index, filled in MainForm

		public static string GpsBabelExecutable = "";
		public static string QuakeMapExecutable = "";
		public static int QuakeMapStartupDelay = 5;	// seconds

        public static bool firstRun = false;
		public static int inputSelectedIndex = 0;
		public static int outputSelectedIndex = 0;
		public static string inputFile = "";
		public static int inputFormatIndex = -1;    // -1 means find default 
		public static string outputFile = "";
		public static int outputFormatIndex = -1;    // -1 means find default

		public static int  gpsMakeIndex = 0;
		public static string  gpsPort = "COM1:";
		public static string  gpsBaudRate = "9600";
		public static bool  gpsNoack = false;

		//public static LibSys.ThreadPool threadPool = null;

		#region Constructor and Executables

		public Project()
		{
			FileInfo myExe = new FileInfo(Project.GetLongPathName(Application.ExecutablePath));
			startupPath = myExe.Directory.FullName;
            driveProgramInstalled = startupPath.Substring(0, 3);

            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            miscFolderPath = Path.Combine(appFolderPath, PROGRAM_NAME_HUMAN + @"\Misc");
		}

		public static void resetGpsBabelExecutable()
		{
            GpsBabelExecutable = "";

			// should be installed under our current dir:
            FileInfo gbExe = new FileInfo(Path.Combine(startupPath, @"gpsbabel-1.3.5\gpsbabel.exe"));
            LibSys.StatusBar.Trace("IP: locating  path=" + gbExe.FullName);
            if (File.Exists(gbExe.FullName))
            {
                GpsBabelExecutable = gbExe.FullName;
            }
            else
            {
                // try our development-time path:
                gbExe = new FileInfo(Path.Combine(startupPath, @"..\..\..\gpsbabel-1.3.5\gpsbabel.exe"));
                LibSys.StatusBar.Trace("IP: locating  path=" + gbExe.FullName);
                if (File.Exists(gbExe.FullName))
                {
                    GpsBabelExecutable = gbExe.FullName;
                }
            }

            if (String.IsNullOrEmpty(GpsBabelExecutable))
            {
                string err = "GPSBabel Executable " + gbExe + " not found";
                LibSys.StatusBar.Error(err);
                Project.ErrorBox(null, err);
            }
            else
            {
                LibSys.StatusBar.Trace("OK: found GPSBabel executable,  path=" + gbExe.FullName);
            }
		}

		public static void resetQuakeMapExecutable()
		{
            string quakeMapPath = driveProgramInstalled + "Program Files\\QuakeMap\\QuakeMap\\QuakeMap.exe";

            if (File.Exists(quakeMapPath))
            {
                // newer versions of QuakeMap are here:
                QuakeMapExecutable = quakeMapPath;
            }
            else
            {
                quakeMapPath = null;

                // hope registry key made by older QuakeMap installer is ok:
                string keyPath = "Software\\VitalBytes\\QuakeMap";
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyPath);
                if (regKey != null)
                {
                    quakeMapPath = "" + regKey.GetValue("INSTALLDIR");
                    regKey.Close();
                }

                if (quakeMapPath != null)
                {
                    FileInfo qmExe = new FileInfo(Path.Combine(quakeMapPath, "bin\\quakemap.exe"));
                    if (File.Exists(qmExe.FullName))
                    {
                        QuakeMapExecutable = qmExe.FullName;
                    }
                    else
                    {
                        string err = "QuakeMap Executable " + qmExe + " not found";
                        LibSys.StatusBar.Error(err);
                        Project.ErrorBox(null, err);
                    }
                }
            }

            if(String.IsNullOrEmpty(quakeMapPath))
            {
                string err = "QuakeMap Executable not found - registry key missing. Please install QuakeMap.";
                LibSys.StatusBar.Error(err);
                Project.ErrorBox(null, err);
            }
		}
		#endregion

		#region Read/Write Options file

		public static void ReadOptions()
		{
			string optionsFilePath = GetMiscPath(OPTIONS_FILE_NAME);
#if DEBUG
			LibSys.StatusBar.Trace("IP: Project:ReadOptions() path=" + optionsFilePath);
#endif
			try 
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(optionsFilePath);

				// we want to traverse XmlDocument fast, as tile load operations can be numerous
				// and come in pack. So we avoid using XPath and rely mostly on "foreach child":
				foreach(XmlNode nnode in xmlDoc.ChildNodes) 
				{
					if(nnode.Name.Equals("options")) 
					{
						foreach(XmlNode node in nnode)
						{
							string nodeName = node.Name;
							try 
							{
								if(nodeName.Equals("mainFormWidth")) 
								{
									mainFormWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("mainFormHeight")) 
								{
									mainFormHeight = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("mainFormX")) 
								{
									mainFormX = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("mainFormY")) 
								{
									mainFormY = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("mainFormMaximized")) 
								{
									mainFormMaximized = "true".Equals(node.InnerText.ToLower());
								}
#if DEBUG
								else if(nodeName.Equals("enableTrace")) 
								{
									enableTrace = node.InnerText.ToLower().Equals("true");
								}
#endif
								else if(nodeName.Equals("serverMessageLast")) 
								{
									serverMessageLast = node.InnerText;
								}
								else if(nodeName.Equals("inputSelectedIndex")) 
								{
									inputSelectedIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("outputSelectedIndex")) 
								{
									outputSelectedIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("inputFile")) 
								{
									inputFile = node.InnerText;
								}
								else if(nodeName.Equals("outputFile")) 
								{
									outputFile = node.InnerText;
								}
								else if(nodeName.Equals("inputFormatIndex")) 
								{
									inputFormatIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("outputFormatIndex")) 
								{
									outputFormatIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("gpsMakeIndex")) 
								{
									gpsMakeIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("gpsPort")) 
								{
									gpsPort = node.InnerText;
								}
								else if(nodeName.Equals("gpsBaudRate")) 
								{
									gpsBaudRate = node.InnerText;
								}
								else if(nodeName.Equals("gpsNoack")) 
								{
									gpsNoack = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("GpsBabelExecutable")) 
								{
									GpsBabelExecutable = node.InnerText;
								}
								else if(nodeName.Equals("QuakeMapExecutable")) 
								{
									QuakeMapExecutable = node.InnerText;
								}
								else if(nodeName.Equals("QuakeMapStartupDelay")) 
								{
									QuakeMapStartupDelay = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("recentFiles")) 
								{
									foreach(XmlNode nnnode in node.ChildNodes) 
									{
										string filename = nnnode.Attributes.GetNamedItem("path").InnerText.Trim();
										filename = Project.GetLongPathName(filename);
										FileInfo fi = new FileInfo(filename);
										if(fi.Exists)
										{
											Project.recentFiles.Add(filename);
										} 
										else
										{
											DirectoryInfo di = new DirectoryInfo(filename);
											if(di.Exists)
											{
												Project.recentFiles.Add(filename);
											}
										}
									}
								}
							} 
							catch (Exception ee) 
							{
								// bad node - not a big deal...
								LibSys.StatusBar.Error("Project:ReadOptions() node=" + nodeName + " " + ee.Message);
							}
						}
					}
				}

			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("Project:ReadOptions() " + e.Message);
			}
		}

		public static void SaveOptions()
		{
			string optionsFilePath = GetMiscPath(OPTIONS_FILE_NAME);
			//LibSys.StatusBar.Trace("Project:SaveOptions: " + optionsFilePath);
			try 
			{
				string seedXml = Project.SEED_XML + "<options></options>";
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(seedXml);

				XmlNode root = xmlDoc.DocumentElement;

				SetValue(xmlDoc, root, "time", "" + DateTime.Now);
#if DEBUG
				SetValue(xmlDoc, root, "enableTrace", "" + enableTrace);
#endif
				SetValue(xmlDoc, root, "mainFormMaximized", "" + mainFormMaximized);
				SetValue(xmlDoc, root, "mainFormWidth", "" + mainFormWidth);
				SetValue(xmlDoc, root, "mainFormHeight", "" + mainFormHeight);
				SetValue(xmlDoc, root, "mainFormX", "" + mainFormX);
				SetValue(xmlDoc, root, "mainFormY", "" + mainFormY);
				SetValue(xmlDoc, root, "serverMessageLast", serverMessageLast);

				SetValue(xmlDoc, root, "inputSelectedIndex", "" + inputSelectedIndex);
				SetValue(xmlDoc, root, "outputSelectedIndex", "" + outputSelectedIndex);
				SetValue(xmlDoc, root, "inputFile", "" + inputFile);
				SetValue(xmlDoc, root, "outputFile", "" + outputFile);
				SetValue(xmlDoc, root, "inputFormatIndex", "" + inputFormatIndex);
				SetValue(xmlDoc, root, "outputFormatIndex", "" + outputFormatIndex);

				SetValue(xmlDoc, root, "gpsMakeIndex", "" + gpsMakeIndex);
				SetValue(xmlDoc, root, "gpsPort", "" + gpsPort);
				SetValue(xmlDoc, root, "gpsBaudRate", "" + gpsBaudRate);
				SetValue(xmlDoc, root, "gpsNoack", "" + gpsNoack);

				SetValue(xmlDoc, root, "GpsBabelExecutable", "" + GpsBabelExecutable);
				SetValue(xmlDoc, root, "QuakeMapExecutable", "" + QuakeMapExecutable);
				SetValue(xmlDoc, root, "QuakeMapStartupDelay", "" + QuakeMapStartupDelay);

				XmlNode recentFilesNode = SetValue(xmlDoc, root, "recentFiles", "");
				foreach(string name in recentFiles)
				{
					AddRecentFilesOption(xmlDoc, recentFilesNode, name); 
				}

				xmlDoc.Save(optionsFilePath);
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("Project:SaveOptions() " + e.Message);
			}
		}
		#endregion

		#region XML helpers

		public static XmlNode SetValue(XmlDocument xmlDoc, XmlNode root, string name, string vvalue)
		{
			XmlNode ret = xmlDoc.CreateElement(name);
			if(vvalue != null && vvalue.Length > 0)
			{
				ret.InnerText = vvalue;
			}
			root.AppendChild(ret);
			return ret;
		}

		public static XmlNode SetValue(XmlDocument xmlDoc, XmlNode root, string prefix, string name, string namespaceURI, string vvalue)
		{
			XmlNode ret = xmlDoc.CreateElement(prefix, name, namespaceURI);
			if(vvalue != null && vvalue.Length > 0)
			{
				ret.InnerText = vvalue;
			}
			root.AppendChild(ret);
			return ret;
		}

		// returns true if new node was added
		public static bool CreateIfMissing(XmlDocument xmlDoc, XmlNode root, string name, string vvalue)
		{
			bool ret = false;
			XmlNodeList nodes = xmlDoc.GetElementsByTagName(name);
			if(nodes == null || nodes.Count == 0)
			{
				SetValue(xmlDoc, root, name, vvalue);
				ret = true;
			}
			return ret;
		}

		// returns count of modified nodes, or 0 if new node was added
		public static int SetReplaceValue(XmlDocument xmlDoc, XmlNode root, string name, string vvalue)
		{
			int ret = 0;
			XmlNodeList nodes = xmlDoc.GetElementsByTagName(name);
			if(nodes == null || nodes.Count == 0)
			{
				SetValue(xmlDoc, root, name, vvalue);
			}
			else
			{
				foreach(XmlNode node in nodes)
				{
					node.InnerText = vvalue;
					ret++;
				}
			}
			return ret;
		}

		private static void AddRecentFilesOption(XmlDocument xmlDoc, XmlNode rfNode, string path)
		{
			XmlNode node = xmlDoc.CreateElement("file");
			XmlAttribute attr = xmlDoc.CreateAttribute("path");
			attr.InnerText = "" + path;
			node.Attributes.Append(attr);
			rfNode.AppendChild(node);
		}
		#endregion

		#region System Helpers 

		public static Encoding xmlEncoding = Encoding.ASCII;

		public static byte[] StrToByteArray(string str)
		{
			return xmlEncoding.GetBytes(str);
		}

		public static string ByteArrayToStr(byte[] bytes)
		{
			return xmlEncoding.GetString(bytes, 0, bytes.Length);
		}

		[DllImport("kernel32.dll")]
		static extern uint GetLongPathName(string shortname, StringBuilder longnamebuff, uint buffersize);

		public static string GetLongPathName(string shortname)
		{
			string ret = "";
			if (shortname != null && shortname.Length > 0)
			{
				StringBuilder longnamebuff = new StringBuilder(512);
				uint buffersize =(uint) longnamebuff.Capacity;

				GetLongPathName(shortname, longnamebuff, buffersize);
				ret = longnamebuff.ToString();
			}
			return ret;
		}

		[DllImport( "kernel32.dll", CharSet=CharSet.Auto )]
		private extern static int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

		// converts long paths into 8.3 format, like "C:\PROGRA~1\QUAKEMAP\QUAKEMAP"
		public static string GetShortPathName( string in_LongPath )
		{
			StringBuilder a_ReturnVal = new StringBuilder( 256 );

			int a_Length = GetShortPathName( in_LongPath, a_ReturnVal, a_ReturnVal.Capacity );

			return a_ReturnVal.ToString();
		}

		public static void setDlgIcon(Form dlg)
		{
			try
			{
				string iconFileName = GetMiscPath(PROGRAM_NAME_LOGICAL + ".ico");
				dlg.Icon = new Icon(iconFileName);
			} 
			catch {}
			// any dialog popping up should eliminate current help popup
			ClearPopup();
		}

		public static bool EqualsIgnoreCase(string a, string b)
		{
			if(a == null || b == null)
			{
				return false;
			}
			return a.ToLower().Equals(b.ToLower()); 
		}

		public static string GetMiscPath(string miscFile)
		{
			if(!Directory.Exists(miscFolderPath)) 
			{
				Directory.CreateDirectory(miscFolderPath);
			}
			return Path.Combine(miscFolderPath, miscFile);
		}

		public static string GetNamedStorePath(string storeName, string fileName)
		{
			string folderPath = Path.Combine(Path.GetTempPath(), PROGRAM_NAME_LOGICAL + "_" + storeName);

			if(!Directory.Exists(folderPath)) 
			{
				Directory.CreateDirectory(folderPath);
			}
			return Path.Combine(folderPath, fileName);
		}

		private static PopupWindow currentPopup = null;
		public static DateTime currentPopupCreated = DateTime.MinValue;
		private static Point popupOffset = new Point(10, 10);

		// use this function for all other popups - at least it catches exceptions
		// parent's bounds are used to make sure popup fits into the parent; otherwise supply null and popup will be to the right of the location
		public static void ShowPopup(Control parent, string caption, Point location)
		{
			if(location.IsEmpty && parent != null)
			{
				location = parent.PointToScreen(popupOffset);
			}
			Project._ShowPopup(parent, caption, location, false);
		}

		private static void _ShowPopup(Control parent, string caption, Point location, bool mayBeToLeft)
		{
			try
			{
				if(currentPopup != null)
				{
					currentPopup.Dispose();
				}
				bool returnFocus = (parent != null && parent.Focused);
				currentPopup = new PopupWindow(parent, caption, location, mayBeToLeft);
				currentPopupCreated = DateTime.Now;
				currentPopup.Show();
				if(returnFocus)
				{
					parent.Focus();
				}
			} 
			catch (Exception e)
			{
				LibSys.StatusBar.Trace("popup: e=" + e.Message);
			}
		}

		public static void popupMaintenance()	// called often from MainFrame, as part of periodicMaintenance()
		{
			if(currentPopup != null && ((DateTime.Now - currentPopupCreated).Seconds > 10))
			{
				try
				{
					currentPopup.Dispose();
				}
				catch {}
				currentPopup = null;
			}
		}

		public static void ClearPopup()	// called when we need to force popup down
		{
			currentPopupCreated = DateTime.MinValue;	// popupMaintenance() will take care of cleanup
		}

		public static void MessageBox(Form owner, string message)
		{
			Project.mainForm.BringToFront();

			if(owner == null)
			{
				owner = mainForm;
			}
			else
			{
				owner.BringToFront();
			}

			try 
			{
				owner.Activate();
				System.Windows.Forms.MessageBox.Show (owner,
							message, Project.PROGRAM_NAME_HUMAN, 
							System.Windows.Forms.MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			} 
			catch
			{}
		}

		public static void InfoBox(Form owner, string message)
		{
			Project.mainForm.BringToFront();

			if(owner == null)
			{
				owner = mainForm;
			}
			else
			{
				owner.BringToFront();
			}

			try 
			{
				owner.Activate();
				System.Windows.Forms.MessageBox.Show (owner,
					message, Project.PROGRAM_NAME_HUMAN, 
					System.Windows.Forms.MessageBoxButtons.OK, MessageBoxIcon.Information);
			} 
			catch
			{}
		}

		public static bool YesNoBox(Form owner, string message)
		{
			Project.mainForm.BringToFront();

			if(owner == null)
			{
				owner = mainForm;
			}
			else
			{
				owner.BringToFront();
			}

			try 
			{
				owner.Activate();
				return (System.Windows.Forms.MessageBox.Show (owner,
							message, Project.PROGRAM_NAME_HUMAN, 
							System.Windows.Forms.MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes);
			} 
			catch
			{
				return false;
			}
		}

		public static void ErrorBox(Form owner, string message)
		{
			Project.mainForm.BringToFront();

			if(owner == null)
			{
				owner = mainForm;
			}
			else
			{
				owner.BringToFront();
			}

			try 
			{
				owner.Activate();
				System.Windows.Forms.MessageBox.Show (owner,
					message, Project.PROGRAM_NAME_HUMAN, 
					System.Windows.Forms.MessageBoxButtons.OK, MessageBoxIcon.Error);
			} 
			catch
			{}
		}

		public static void RunBrowser(string url)
		{
			//if(Project.serverAvailable)
			{
				System.Diagnostics.ProcessStartInfo pInfo = new	ProcessStartInfo(@"explorer.exe", url);

				Process p = new Process();
				p.StartInfo = pInfo;
				p.Start(); // This will popup the default web browser with the specified URL
			}
		}

		public static void insertRecentFile(string fileName)
		{
			int index = Project.recentFiles.IndexOf(fileName);
			if(index == -1)
			{
				Project.recentFiles.Insert(0, fileName);
			}
			else
			{
				// file name already there, move it up:
				Project.recentFiles.RemoveAt(index);
				Project.recentFiles.Insert(0, fileName);
			}

			while(Project.recentFiles.Count > 10)
			{
				Project.recentFiles.RemoveAt(Project.recentFiles.Count - 1);
			}
		}

		/// <summary>
		/// this is for closing from dialogs and unusual places. Normal closing of the form calls MainForm_Closing(), with SaveOptions there.
		/// </summary>
		public static void Exit()
		{
			LibSys.StatusBar.WriteLine("Project: Exit");

			Project.Closing();

			Application.Exit();
			Application.ExitThread();
			Environment.Exit(0);
		}

		private static bool hasClosed = false;

		/// <summary>
		/// called from MainForm.MainForm_Closing() when exiting normally
		/// </summary>
		public static void Closing()
		{
			if(!hasClosed)
			{
				goingDown = true;
				LibSys.StatusBar.WriteLine("Project: Closing '" + Thread.CurrentThread.Name + "' - " + DateTime.Now);

				ThreadPool2.EmptyQueue();
				Project.SaveOptions();
				hasClosed = true;
			}
		}

		public static bool fitsScreen(int x, int y, int w, int h)
		{
			int screenWidth = Screen.PrimaryScreen.Bounds.Width;
			int screenHeight = Screen.PrimaryScreen.Bounds.Height;

			if(w < 100 || h < 100 || x < 2 || x > screenWidth - w || x > screenWidth - 100
				|| y < 2 || y > screenHeight - h || y > screenHeight - 100)
			{
				return false;
			}

			return true;
		}

		#endregion

		#region HTML/HTTP helpers

		public static void ApplyGlobalHTTPProxy(bool forceDefault)
		{
			try 
			{
				if(Project.useProxy)
				{
					System.Net.WebProxy proxy	= new System.Net.WebProxy(Project.proxyServer, Project.proxyPort);
					proxy.BypassProxyOnLocal = true;
					System.Net.GlobalProxySelection.Select = proxy;
				}
				else if(Project.forceEmptyProxy)
				{
					System.Net.GlobalProxySelection.Select = System.Net.GlobalProxySelection.GetEmptyWebProxy();
				}
				else if(forceDefault)
				{
					System.Net.GlobalProxySelection.Select = System.Net.WebProxy.GetDefaultProxy();
				}
			}
			catch
			{
			}
		}

		public static bool is404(byte[] data)
		{
			/*
			 * this is how the 404 looks like from Apache:
			 * 
				<HTML>
				<HEAD>
				<TITLE>404 Not Found</TITLE>
				</HEAD>
				<BODY>
				<H1>Not Found</H1>
				The requested URL was not found on this server.
				<P>
				<HR>
				<ADDRESS>
				Apache Server at earthquakemap.com
				</ADDRESS>
				</BODY>
				</HTML>
			 */
			const int CNT = 150;
			char[] chars = new char[CNT];
			int i;
			for(i=0; i < CNT && i < data.Length ;i++) 
			{
				chars[i] = Convert.ToChar(data[i]);
			}
			string strData = new string(chars, 0, i);
			if(strData.ToLower().IndexOf("title>") >= 0 && strData.IndexOf("404") >= 0 && strData.ToLower().IndexOf("not found") >= 0) 
			{
				//LibSys.StatusBar.Trace("is404()  -- true");
				return true;
			}
			//LibSys.StatusBar.Trace("is404()  -- false");
			return false;
		}
		#endregion

		#region Windows messaging to pass strings between processes

		/// <summary>
		/// on the Windows messaging see http://www.dotnet247.com/247reference/msgs/31/159687.aspx
		/// </summary>

		struct COPYDATASTRUCT
		{
			public int dwData;
			public int cbData;
			public IntPtr lpData;
		}

		const int LMEM_FIXED = 0x0000;
		const int LMEM_ZEROINIT = 0x0040;
		const int LPTR = (LMEM_FIXED | LMEM_ZEROINIT);
		public const int WM_COPYDATA = 0x004A;
		public const int HWND_BROADCAST  = 0xffff;      
		public const int WM_USER = 0x400;

		[DllImport("kernel32.dll")]
		public static extern IntPtr LocalAlloc(int flag, int size);

		[DllImport("kernel32.dll")]
		public static extern IntPtr LocalFree(IntPtr p);

		[DllImport("user32",EntryPoint="SendMessage")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32",EntryPoint="PostMessage")]
		public static extern bool PostMessage(IntPtr hwnd,int msg,int wparam,int lparam);

		[DllImport("user32",EntryPoint="RegisterWindowMessage")]
		public static extern int RegisterWindowMessage(string msgString);

		//Create wrappers for the memory API's similar to
		//Marshal.AllocHGlobal and Marshal.FreeHGlobal

		private static IntPtr AllocHGlobal(int cb)
		{
			IntPtr hMemory = new IntPtr();
			hMemory = LocalAlloc(LPTR, cb);
			return hMemory;
		}

		private static void FreeHGlobal(IntPtr hMemory)
		{
			if (hMemory != IntPtr.Zero)
			{
				LocalFree(hMemory);
			}
		}

		public static void SendMsgString(IntPtr hWndDest, string sMsg)
		{
			COPYDATASTRUCT oCDS = new COPYDATASTRUCT();
			oCDS.cbData = (sMsg.Length + 1) * 2;
			oCDS.lpData = LocalAlloc(0x40, oCDS.cbData);
			Marshal.Copy(sMsg.ToCharArray(), 0, oCDS.lpData, sMsg.Length);
			oCDS.dwData = 1;
			IntPtr lParam = AllocHGlobal(oCDS.cbData);
			Marshal.StructureToPtr(oCDS, lParam,false);
			SendMessage(hWndDest, WM_COPYDATA, IntPtr.Zero, lParam);
			LocalFree(oCDS.lpData);
			FreeHGlobal(lParam);
		}

		public static string GetMsgString(IntPtr lParam)
		{
			COPYDATASTRUCT st = (COPYDATASTRUCT) Marshal.PtrToStructure(lParam,	typeof(COPYDATASTRUCT));
			string str = Marshal.PtrToStringUni(st.lpData);
			return str;
		}
		#endregion
	}

	public delegate void delegateShowPopup(Control parent, string caption, Point location);
 
}
