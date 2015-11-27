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
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using LibSys;
using LibNet;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// Summary description for TileCache.
	/// </summary>
	public class TileCache
	{
		private static string m_mapsPath;
			
		private static string imageExt = Project.JPG_DESCR_EXTENSION;
		private static string featuresExt = Project.FDB_DESCR_EXTENSION;

		private static MyCache m_backdropCache = new MyCache();
		private static MyCache m_featuresCache = new MyCache();
		private static ArrayList m_mappingServers = new ArrayList();

		private static ZipcodeServer m_zipcodeServer = null;
		public  static ZipcodeServer ZipcodeServer { get { return m_zipcodeServer; } }

		public static event DownloadProgressHandler ProgressCallback;	// needed for WebDownload, stays empty here

		public static void setMappingCacheLocation(string path)
		{
			Project.setMappingCacheLocation(path);
			m_mapsPath = Project.GetMapsTempPath();

			TileCache.Clear();	// release all files

			TerraserverCache.setMappingCacheLocation();
		}

		public static void init(string cgibinptrUrl)
		{
			m_mapsPath = Project.GetMapsTempPath();

			LibSys.StatusBar.WriteLine("Actual location for mapping cache in " + m_mapsPath);

			try 
			{
#if DEBUG
				LibSys.StatusBar.Trace("IP: reaching cgibinptr URL='" + cgibinptrUrl + "'");
#endif
				/*
				 * this code has long uncontrolled timeout, and has been replaced with the WebDownload-based code below
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(cgibinptrUrl);
				if(Project.suspendKeepAlive)
				{
					req.KeepAlive = false;
				}
				WebResponse res = req.GetResponse();
				Stream responseStream = res.GetResponseStream();
				StreamReader reader = new StreamReader (responseStream);
				*/
				
				WebDownload webDL = new WebDownload(Project.webTimeoutMs);		// default 7 seconds to time out
				// Create the state object.
				DownloadInfo info = new DownloadInfo();
				info.baseName = "";
				info.strUrl = cgibinptrUrl;
				info.addMonitoredMethod = null;

				byte[] downloadedData = null;
				int tries = 1;
				int maxTries = 1;
				while (tries <= maxTries && downloadedData == null && !info.is404)
				{
					downloadedData = webDL.Download(info, ProgressCallback);		// will timeout 
#if DEBUG
					if(downloadedData != null) 
					{
						LibSys.StatusBar.Trace("IP: try " + tries + " TileCache:Download() - " + cgibinptrUrl + " delivered " + downloadedData.Length + " bytes" );
					} 
					else 
					{
						LibSys.StatusBar.Trace("IP: try " + tries + " TileCache:Download() - " + cgibinptrUrl + " delivered null bytes" );
					}
#endif
					tries++;
				}

				if(downloadedData == null || downloadedData.Length == 0)
				{
					LibSys.StatusBar.Error("failed to reach QuakeMap.com");
					return;		// Project.serverAvailable will be set to false, and no web attempts will take place.
				}
				LibSys.StatusBar.Trace("OK: reached QuakeMap.com");

				string responseString = Project.ByteArrayToStr(downloadedData);
				StringReader reader = new StringReader (responseString);
				
				string upgrVersion = "";
				bool ignoreUpgrade = false;
				int state = 0;
				string line;
				while((line=reader.ReadLine()) != null) 
				{
					try
					{
						switch(state)
						{
							case 0:
								if(line.StartsWith("MAPSERVER=")) 
								{
									MappingServer ms = new MappingServer(line.Substring("MAPSERVER=".Length));
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - ms='" + ms + "'");
#endif
									m_mappingServers.Add(ms);
								} 
								else if(line.StartsWith("ZIPSERVER=")) 
								{
									ZipcodeServer zs = new ZipcodeServer(line.Substring("ZIPSERVER=".Length));
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - zs='" + zs + "'");
#endif
									m_zipcodeServer = zs;
								}
								else if(line.StartsWith("TILERABOUT=")) 
								{
									Project.ABOUT_URL = line.Substring("TILERABOUT=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - about='" + Project.ABOUT_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERORDER=")) 
								{
									Project.ORDER_URL = line.Substring("TILERORDER=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - order='" + Project.ORDER_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERDLOAD=")) 
								{
									Project.DLOAD_URL = line.Substring("TILERDLOAD=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - download='" + Project.DLOAD_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERUPDATE=")) 
								{
									Project.UPDATE_URL = line.Substring("TILERUPDATE=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - update='" + Project.UPDATE_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERPRIVACY=")) 
								{
									Project.PRIVACY_URL = line.Substring("TILERPRIVACY=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - privacy='" + Project.PRIVACY_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERPDA=")) 
								{
									Project.PDA_URL = line.Substring("TILERPDA=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - pda='" + Project.PDA_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERHELP=")) 
								{
									Project.HELP_FILE_URL = line.Substring("TILERHELP=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - help='" + Project.HELP_FILE_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERHDATE=")) 
								{
									string sDate = line.Substring("TILERHDATE=".Length);
									Project.HELP_FILE_DATE = Convert.ToDateTime(sDate);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - helpFileDate='" + Project.HELP_FILE_DATE + "'");
#endif
								}
								else if(line.StartsWith("TILERMISC=")) 
								{
									Project.MISC_FOLDER_URL = line.Substring("TILERMISC=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - misc='" + Project.MISC_FOLDER_URL + "'");
#endif
								}
								else if(line.StartsWith("TILERSAMPLES=")) 
								{
									Project.SAMPLES_FOLDER_URL = line.Substring("TILERSAMPLES=".Length);
#if DEBUG
									LibSys.StatusBar.Trace("OK: TileCache() - samples='" + Project.SAMPLES_FOLDER_URL + "'");
#endif
								}
								else if(line.StartsWith("MESSAGE=")) 
								{
									state = 1;
									Project.serverMessage = "";
								}
								else if(line.StartsWith("UPGR")) 
								{
									upgrVersion = line.Substring(4);
									if(Project.PROGRAM_VERSION_RELEASEDATE.Equals(upgrVersion))
									{
										ignoreUpgrade = true;
									}
									else
									{
										Project.upgradeMessage = "\n";
									}
									state = 2;
								}
								break;
							case 1:
								if(line.StartsWith("ENDMESSAGE")) 
								{
									state = 0;
								}
								else 
								{
									Project.serverMessage += (line + "\n");
								}
								break;
							case 2:
								if(line.StartsWith("ENDUPGR")) 
								{
									state = 0;
								}
								else 
								{
									if(!ignoreUpgrade)
									{
										Project.upgradeMessage += (line + "\n");
									}
								}
								break;
						}
					}
					catch {}
				}

				if(!Project.serverMessageLast.Equals(Project.serverMessage) && Project.upgradeMessage.Length == 0)
				{
					string message = Project.serverMessage; // + Project.upgradeMessage;   upgrade message shows up in MainForm
					LibSys.StatusBar.Trace(message);
					if(greetingForm == null)
					{
						Project.MessageBox(null, message);
					}
					else
					{
						System.Windows.Forms.MessageBox.Show (greetingForm, message, Project.PROGRAM_NAME_HUMAN, 
																System.Windows.Forms.MessageBoxButtons.OK,
																MessageBoxIcon.Exclamation);
					}
					Project.serverMessageLast = Project.serverMessage;
				}
			} 
			catch (Exception e)
			{
				LibSys.StatusBar.Error("exception: " + e.Message);
			}
		}

		// to help initial server message dialog stay on top and not disappear in the stack:
		public static Form greetingForm = null;

		public static void AddBackdrop(string baseName, Backdrop b)
		{
			try
			{
				m_backdropCache.Add(baseName, b);
			}
			catch (Exception e)
			{
			} 
		}

		public static void AddFeatures(string baseName, Features f)
		{
			try
			{
				m_featuresCache.Add(baseName, f);
			}
			catch (Exception e)
			{
			} 
		}

		public static void ListBackdropCache()
		{
#if DEBUG
			LibSys.StatusBar.Trace("BackdropCache: " + m_backdropCache.Count + " items");
			foreach (Backdrop b in m_backdropCache.Values) 
			{
				LibSys.StatusBar.Trace(" - " + b);
			}
			LibSys.StatusBar.Trace(" ----- ");
#endif
		}

		public static void ListFeaturesCache()
		{
#if DEBUG
			LibSys.StatusBar.Trace("FeaturesCache: " + m_featuresCache.Count + " items");
			foreach (Features f in m_featuresCache.Values) 
			{
				LibSys.StatusBar.Trace(" - " + f);
			}
			LibSys.StatusBar.Trace(" ----- ");
#endif
		}

		public static void BackdropMarkUnused(string baseName)
		{
			//LibSys.StatusBar.Trace("OK: TileCache:BackdropMarkUnused() - tile '" + baseName + "'");
			m_backdropCache.MarkUnused(baseName); 
		}

		public static void FeaturesMarkUnused(string baseName)
		{
			//LibSys.StatusBar.Trace("OK: TileCache:FeaturesMarkUnused() - tile '" + baseName + "'");
			m_featuresCache.MarkUnused(baseName); 
		}

		/// <summary>
		/// remove old unused elements from cache
		/// </summary>
		public static void purge()
		{
			int cacheMinCount = Project.CACHE_MIN_UNUSED_COUNT;

			//LibSys.StatusBar.Trace("IP: TileCache:purge()  before: " + ToString());

			m_featuresCache.purge(cacheMinCount);
			m_backdropCache.purge(cacheMinCount);

			//LibSys.StatusBar.Trace("IP: TileCache:purge()   after: " + ToString());
		}

		/// <summary>
		/// remove all elements from cache - needed for full Refresh
		/// </summary>
		public static void Clear()
		{
			m_featuresCache.Clear();
			m_backdropCache.Clear();
		}

		public static Features getFeatures(Tile tile, string baseName)
		{
			string featuresFileName = Path.Combine(m_mapsPath, baseName + featuresExt);

			Features ret = null;
			try 
			{
				if(m_featuresCache.ContainsKey(baseName))
				{
					ret = (Features)m_featuresCache[baseName];	// may be still downloading
					if(ret != null)
					{
						ret.MarkUsed();
					}
#if DEBUG
					LibSys.StatusBar.Trace("OK: TileCache:getFeatures() - tile '" + baseName + "' - found in cache - " + ret);
#endif
					return ret;
				}

				bool loadedFromFile = false;
				if(!Project.reloadRefresh && File.Exists(featuresFileName)) 
				{
					try
					{
						ret = new Features(featuresFileName, baseName, true);
#if DEBUG
						LibSys.StatusBar.Trace("OK: TileCache:getFeatures() - tile '" + baseName + "' - loaded local");
#endif
						AddFeatures(baseName, ret);
						loadedFromFile = true;
					} 
					catch {}
				} 

				if(!loadedFromFile && m_mappingServers.Count > 0)
				{
					string featuresUrl = getFileUrl(tile, baseName, featuresFileName);

					if(featuresUrl == null) 
					{
						ret = new Features(null, baseName, true);	// doFill with null name makes it Empty
						AddFeatures(baseName, ret);
						return ret;
					}

					if(!m_featuresCache.ContainsKey(baseName)) 
					{
						ret = new Features(featuresFileName, baseName, false);	// fill later
						AddFeatures(baseName, ret);

						DownloadThread dt = new DownloadThread();
						dt.DownloadUrl = featuresUrl;
						dt.tile = tile;
						dt.baseName = baseName;
						dt.fileName = featuresFileName;
						dt.CompleteCallback += new DownloadCompleteHandler( featuresDownloadCompleteCallback );
						dt.ProgressCallback += new DownloadProgressHandler( featuresDownloadProgressCallback );
						dt.addMonitoredMethod = new AddMonitoredMethod(ProgressMonitor.addMonitored);

						//add this to the thread pool / queue a task 
						//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download)); 
						ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), baseName); 

#if DEBUG
						LibSys.StatusBar.Trace("OK: TileCache:getFeatures() - tile '" + baseName + "' - loading remote from " + featuresUrl);
#endif
					}
				}
			} 
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("file '" + featuresFileName + "' failed to load: " + e.Message);
			}
			// whatever happened before, returning null is not an option.
			if(ret == null) 
			{
				ret = new Features(null, baseName, true);	// doFill with null name makes it Empty
				AddFeatures(baseName, ret);
			}
			return ret;
		}

		public static Backdrop getBackdrop(Tile tile, string baseName)
		{
			string imageFileName = Path.Combine(m_mapsPath, baseName + imageExt);

			Backdrop ret = null;
			try 
			{
				if(m_backdropCache.ContainsKey(baseName))
				{
					ret = (Backdrop)m_backdropCache[baseName];	// may be IsEmpty, if proven that can't download
					ret.MarkUsed();
#if DEBUG
					LibSys.StatusBar.Trace("OK: TileCache:getBackdrop() - tile '" + baseName + "' - found in cache - " + ret);
#endif
					return ret;
				}

				bool loadedFromFile = false;
				if(!Project.reloadRefresh && File.Exists(imageFileName)) 
				{
					try 
					{
						ret = new Backdrop(imageFileName, baseName, true);
#if DEBUG
						LibSys.StatusBar.Trace("OK: TileCache:getBackdrop() - tile '" + baseName + "' - loaded from file");
#endif
						AddBackdrop(baseName, ret);
						loadedFromFile = true;
					} 
					catch {}
				} 

				if(!loadedFromFile && m_mappingServers.Count > 0)
				{
					string imageUrl = getFileUrl(tile, baseName, imageFileName);

					if(imageUrl == null) 
					{
						ret = new Backdrop(null, baseName, true);	// doFill with null name makes it Empty
						AddBackdrop(baseName, ret);
						return ret;
					}

					if(!m_backdropCache.ContainsKey(baseName)) 
					{
						ret = new Backdrop(imageFileName, baseName, false);		// fill later
						AddBackdrop(baseName, ret);
						DownloadThread dt = new DownloadThread();
						dt.DownloadUrl = imageUrl;
						dt.tile = tile;
						dt.baseName = baseName;
						dt.fileName = imageFileName;
						dt.CompleteCallback += new DownloadCompleteHandler( imageDownloadCompleteCallback );
						dt.ProgressCallback += new DownloadProgressHandler( imageDownloadProgressCallback );
						dt.addMonitoredMethod = new AddMonitoredMethod(ProgressMonitor.addMonitored);

						//add dt worker method to the thread pool / queue a task 
						//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download)); 
						ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), baseName); 

#if DEBUG
						LibSys.StatusBar.Trace("OK: TileCache:getBackdrop() - tile '" + baseName + "' - loading remote from " + imageUrl);
#endif
					} 
#if DEBUG
					else 
					{
						LibSys.StatusBar.Trace("OK: TileCache:getBackdrop() - tile '" + baseName + "' - already in cache");
					}
#endif
				}
			} 
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("file '" + imageFileName + "' failed to load: " + e.Message);
			}

			// whatever happened before, returning null is not an option.
			if(ret == null) 
			{
				ret = new Backdrop(null, baseName, true);	// doFill with null name makes it Empty
				AddBackdrop(baseName, ret);
			}
			return ret;
		}

		private static string getFileUrl(Tile tile, string baseName, string fileName)
		{
			string fileUrl = null;

			foreach(MappingServer ms in m_mappingServers)
			{
#if DEBUG
				LibSys.StatusBar.Trace("IP: ms=" + ms);
#endif
				if(ms.isLevelSupported(tile.Level)) 
				{
					fileUrl = ms.Url + "/" + Project.mapsQuality + "/" + baseName + ms.getRightExtention(fileName);
#if DEBUG
					LibSys.StatusBar.Trace("IP: fileUrl=" + fileUrl);
#endif
					break;
				}
			}
			return fileUrl;
		}

		/// <summary>
		/// helps when image file was corrupted, arranges reload.
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="baseName"></param>
		public static void resetBackdrop(Tile tile, string baseName)
		{
			string imageFileName = Path.Combine(m_mapsPath, baseName + imageExt);
			try
			{
				if(Project.serverAvailable)
				{
					string imageUrl = getFileUrl(tile, baseName, imageFileName);

					DownloadThread dt = new DownloadThread();
					dt.DownloadUrl = imageUrl;
					dt.tile = tile;
					dt.baseName = baseName;
					dt.fileName = imageFileName;
					dt.CompleteCallback += new DownloadCompleteHandler( imageDownloadCompleteCallback );
					dt.ProgressCallback += new DownloadProgressHandler( imageDownloadProgressCallback );
					dt.addMonitoredMethod = new AddMonitoredMethod(ProgressMonitor.addMonitored);

					//add dt worker method to the thread pool / queue a task 
					//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), baseName); 
					ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), baseName); 

#if DEBUG
					LibSys.StatusBar.Trace("OK: TileCache:resetBackdrop() - tile '" + baseName + "' - loading remote from " + imageUrl);
#endif
				}
			} 
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("file '" + imageFileName + "' failed to load on reset: " + e.Message);
			}
		}

		private static void imageDownloadProgressCallback ( DownloadInfo info )
		{
			//LibSys.StatusBar.Trace("IP: TileCache:imageDownloadProgressCallback() - " + info.baseName + " - " + info.bytesProcessed + " of " + info.dataLength);
			if(info.monitored != null)
			{
				if(info.dataLength > 0)
				{
					info.monitored.Progress = info.bytesProcessed * 100 / info.dataLength;
					ProgressMonitor.WorkValues();
				}
				else
				{
					info.monitored.Progress = 20;
				}
			}
		}

		private static void imageDownloadCompleteCallback ( object otile, DownloadInfo info, string imageFileName, byte[] dataDownloaded )
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: TileCache:imageDownloadCompleteCallback() - " + info.baseName + " " + (dataDownloaded == null? "null" : "" + dataDownloaded.Length) + " bytes loaded");
#endif
			Backdrop backdrop = (Backdrop)m_backdropCache[info.baseName];
			Tile tile = (Tile)otile;

			if(dataDownloaded == null || info.is404 || Project.is404(dataDownloaded)) 
			{
				string comment = dataDownloaded == null ? "no data" : "404";
				if(backdrop != null)
				{
					backdrop.IsEmpty = true;	// proven empty
				}
				ProgressMonitor.markComplete(info.monitored, false, comment);
			}
			else 
			{
				FileStream fs = null;
				try 
				{
					string comment = "" + dataDownloaded.Length + " bytes";
					fs = new FileStream(imageFileName, FileMode.Create);
					fs.Write(dataDownloaded, 0, dataDownloaded.Length);
					fs.Close();
					fs = null;
#if DEBUG
					LibSys.StatusBar.Trace("OK: file " + imageFileName + " created");
#endif
					if(backdrop != null)
					{
						backdrop.Fill();
					}
					ProgressMonitor.markComplete(info.monitored, true, comment);
				} 
				catch (Exception e) 
				{
#if DEBUG
					LibSys.StatusBar.Error("" + e);
#endif
					if(backdrop != null)
					{
						backdrop.IsEmpty = true;	// proven empty
					}
					ProgressMonitor.markComplete(info.monitored, false, e.Message);
				}
				finally
				{
					if(fs != null)
					{
						fs.Close();
					}
				}
			}

			if(tile != null)
			{
				tile.backdropArrived(backdrop);
			}
		}

		private static void featuresDownloadProgressCallback ( DownloadInfo info )
		{
			//LibSys.StatusBar.Trace("IP: TileCache:featuresDownloadProgressCallback() - " + info.baseName + " - " + info.bytesProcessed + " of " + info.dataLength);
			if(info.monitored != null)
			{
				if(info.dataLength > 0)
				{
					info.monitored.Progress = info.bytesProcessed * 100 / info.dataLength;
					ProgressMonitor.WorkValues();
				}
				else
				{
					info.monitored.Progress = 20;
				}
			}
		}

		private static void featuresDownloadCompleteCallback ( object otile, DownloadInfo info, string featuresFileName, byte[] dataDownloaded )
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: TileCache:featuresDownloadCompleteCallback() - " + info.baseName + " " + (dataDownloaded == null? "null" : "" + dataDownloaded.Length) + " bytes loaded");
#endif
			Features features = (Features)m_featuresCache[info.baseName];
			Tile tile = (Tile)otile;

			if(dataDownloaded == null || info.is404 || Project.is404(dataDownloaded)) 
			{
				string comment = dataDownloaded == null ? "no data" : "404";
				if(features != null)
				{
					features.IsEmpty = true;	// proven empty
				}
				ProgressMonitor.markComplete(info.monitored, false, comment);
			}
			else
			{
				FileStream fs = null;
				try 
				{
					string comment = "" + dataDownloaded.Length + " bytes";
					fs = new FileStream(featuresFileName, FileMode.Create);
					fs.Write(dataDownloaded, 0, dataDownloaded.Length);
					fs.Close();
					fs = null;
#if DEBUG
					LibSys.StatusBar.Trace("OK: file " + featuresFileName + " created");
#endif
					if(features != null)
					{
						features.Fill();
					}
					ProgressMonitor.markComplete(info.monitored, true, comment);
				} 
				catch (Exception e) 
				{
#if DEBUG
					LibSys.StatusBar.Error("" + e);
#endif
					if(features != null)
					{
						features.IsEmpty = true;	// proven empty
					}
					ProgressMonitor.markComplete(info.monitored, false, e.Message);
				}
				finally
				{
					if(fs != null)
					{
						fs.Close();
					}
				}
			}
			if(tile != null)
			{
				tile.featuresArrived(features);
			}
		}

		public static new string ToString()
		{
			return "TileCache: " + m_featuresCache.Count + " features (" + m_featuresCache.countUnused() + " unused)   "
								 + m_backdropCache.Count + " backdrops (" + m_backdropCache.countUnused() + " unused)";
		}

	}
}
