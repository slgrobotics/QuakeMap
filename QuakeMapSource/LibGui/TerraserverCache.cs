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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml;
using System.Data;
using System.Text;

using LibSys;
using LibGeo;
using LibNet;

using LibNet.TerraServer;
using LonLatPt = LibNet.TerraServer.LonLatPt;

using LibNet.LandmarkServer;

namespace LibGui
{
	/// <summary>
	/// Summary description for TerraserverCache.
	/// </summary>
	public class TerraserverCache
	{
		private const int maxRetries = 3;

		protected static PictureManager m_pictureManager = null;
		public static PictureManager PictureManager { get { return m_pictureManager; } set { m_pictureManager = value; } }

		protected static CameraManager m_cameraManager = null;
		public static CameraManager CameraManager { get { return m_cameraManager; } set { m_cameraManager = value; } }

		//private static string m_tileDbFileName = "";
		private static string m_landmarksDbFileName = "";
			
		private static string imageExt = Project.JPG_DESCR_EXTENSION;

		private static MyCache m_backdropCache = new MyCache();

		public static event DynamicObjectCreateHandler DynamicObjectCreateCallback;
		public static event DynamicObjectDeleteHandler DynamicObjectDeleteCallback;
		public static event DynamicObjectMoveHandler   DynamicObjectMoveCallback;

		public static Image m_image = null;

		public static TerraService ts = null;
		public static LandmarkService ls = null;

		public static string[]	landmarkPointTypes = null;
		public static bool[]	landmarkPointShow = null;
		private static Hashtable stringToIndex = new Hashtable();
		private static ArrayList m_landmarks = new ArrayList();
		public static ArrayList Landmarks { get { return m_landmarks; } }

		private static DataSet m_landmarksDS = null;
		public static DataSet LandmarksDS { get { return m_landmarksDS; } }
		public static bool LandmarksDSChanged = false;
		
		//private static DataSet m_tilesDS = null;
		//public static DataSet TilesDS { get { return m_tilesDS; } }
		//public static bool TilesDSChanged = false;

		public static bool landmarkCanShow(string landmarkPointType)
		{
			object obj = stringToIndex[landmarkPointType.ToLower()];
			if(obj == null)
			{
				return false;
			}
			int index = (int)obj;
			return landmarkPointShow[index];
		}

		public static void setMappingCacheLocation()
		{
			TerraserverCache.Clear();
			m_landmarksDbFileName = Path.Combine(Project.GetTerraserverMapsBasePath(), "landmarksdb.xml");
		}

		public TerraserverCache()
		{
			DataColumn myDataColumn;
 
			/*
			string mapsPath = Project.GetTerraserverMapsBasePath();
			m_landmarksDbFileName = Path.Combine(mapsPath, "landmarksdb.xml");

			m_tileDbFileName = Path.Combine(mapsPath, "tilesdb.xml");

			DataTable tilesTable;
			tilesTable = new DataTable("tile");

			// Create name column.
			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = true;
			tilesTable.Columns.Add(myDataColumn);
 
			// Create "theme" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "theme";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tilesTable.Columns.Add(myDataColumn);
 
			// Create "scale" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "scale";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tilesTable.Columns.Add(myDataColumn);
 
			// Create "top left lat" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "tllat";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tilesTable.Columns.Add(myDataColumn);
 
			// Create "top left lng" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "tllng";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tilesTable.Columns.Add(myDataColumn);
 
			// Create "bottom right lat" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "brlat";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tilesTable.Columns.Add(myDataColumn);
 
			// Create "bottom right lng" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "brlng";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			tilesTable.Columns.Add(myDataColumn);
 
			// Make the basename column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = tilesTable.Columns["name"];
			tilesTable.PrimaryKey = PrimaryKeyColumns;
 
			// Add the new DataTable to the DataSet.
			m_tilesDS = new DataSet();
			m_tilesDS.Tables.Add(tilesTable);
			*/

			DataTable landmarksTable = new DataTable("lm");

			// Create name column.
			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = true;
			landmarksTable.Columns.Add(myDataColumn);
 
			// Create "type" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "type";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			landmarksTable.Columns.Add(myDataColumn);
 
			// Create "lat" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "lat";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			landmarksTable.Columns.Add(myDataColumn);
 
			// Create "lng" column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Double");
			myDataColumn.ColumnName = "lng";
			myDataColumn.AutoIncrement = false;
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			landmarksTable.Columns.Add(myDataColumn);
 
			// Make the name column the primary key column.
			DataColumn[] PrimaryKeyColumns1 = new DataColumn[1];
			PrimaryKeyColumns1[0] = landmarksTable.Columns["name"];
			landmarksTable.PrimaryKey = PrimaryKeyColumns1;
 
			// Add the new DataTable to the DataSet.
			m_landmarksDS = new DataSet();
			m_landmarksDS.Tables.Add(landmarksTable);

			DataTable landmarkTypesTable = new DataTable("lmtype");

			// Create type column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "type";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = true;
			landmarkTypesTable.Columns.Add(myDataColumn);
 
			// Add the new DataTable to the DataSet.
			m_landmarksDS.Tables.Add(landmarkTypesTable);
		}

		public static void initTerraService()
		{
			if(ts == null && Project.serverAvailable)
			{
				int retries = 0;
				while (retries <= maxRetries)
				{
					try
					{
						ts = new TerraService();
						//ts.Url = "http://" + Project.TERRA_SERVER + "/TerraService.asmx";
						break;
					}
					catch (Exception e)
					{
						retries++;
						if (retries == maxRetries)
						{
							LibSys.StatusBar.Error("Unable to establish a connection to the TerraServer Web Service" + e.Message);
							MessageBox.Show(Project.mainForm,
								"Unable to establish a connection to the TerraServer Web Service" + e.Message,
								"TerraService Error", 
								MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
					}
					catch
					{
						retries++;
						if (retries == maxRetries)
						{
							LibSys.StatusBar.Error("TerraServer Web Service protocol error during initialization");
							MessageBox.Show(Project.mainForm,
								"TerraServer Web Service protocol error during initialization",
								"TerraService Error", 
								MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
					}
				}
			}
		}

		private static bool initedOffline = false;

		public static void initLandmarkService()
		{
			if(ls == null)
			{
				int retries = 0;
				bool failed = !Project.serverAvailable;

				while (Project.serverAvailable && retries <= maxRetries)
				{
					initedOffline = false;
					failed = false;
					try
					{
						ls = new LandmarkService();
						//ls.Url = "http://" + Project.TERRA_SERVER + "/LandmarkService.asmx";
						LandmarkType[] lt = ls.GetLandmarkTypes();
						int numLandmarkTypes = lt.Length;
						landmarkPointTypes = new string[numLandmarkTypes];
						//
						//	Populate a hash table associating an index with a landmark type
						//	This will be used when drawing landmarks, to associate a landmark type
						//	with an image in an image list, which is addressed by index
						//
						int j = 0;
						for (int i=0; i < numLandmarkTypes; i++)
						{
							string type = "" + lt[i].Type;
							// as of May 31,03 , "Institution" and "Recreation Area" cause exception:
							//          Server was unable to process request. --> Data is Null. This method or property cannot be called on Null values.
							if ("Recreation Area".Equals(type) || "Institution".Equals(type))
							{
								continue;
							}
							if ("Terminal".Equals(type))
							{
								type = "Transportation Terminal";
							}
							landmarkPointTypes[j] = type;
							j++;
						}

						if(j > 0)
						{
							// now make a clean compacted copy of the same:
							landmarkPointShow  = new bool[j];
							string[] tmp = new string[j];
							stringToIndex.Clear();
							for (int i=0; i < j; i++)
							{
								tmp[i] = landmarkPointTypes[i].ToLower();
								landmarkPointShow[i] = true;
								stringToIndex.Add (tmp[i], i);
							}
							landmarkPointTypes = tmp;
						}

						// (re)populate lmtype table from m_landmarksDS:
						m_landmarksDS.Tables["lmtype"].Clear();
						foreach(string type in landmarkPointTypes)
						{
							// Create DataRow objects and add them to the DataTable
							DataRow myDataRow;
							myDataRow = m_landmarksDS.Tables["lmtype"].NewRow();
							myDataRow["type"] = type;
							try 
							{
								m_landmarksDS.Tables["lmtype"].Rows.Add(myDataRow);
							} 
							catch {}  // already there, skip it
						}
						break;
					}
					catch(Exception e)
					{
						LibSys.StatusBar.Error("Unable to establish a connection to the LandmarkServer Web Service" + e.Message);
						failed = true;
					}
					catch
					{
						LibSys.StatusBar.Error("LandmarkServer Web Service protocol error during initialization");
						failed = true;
					}

					if(failed)
					{
						retries++;
						if (retries == maxRetries)
						{
							LibSys.StatusBar.Error("Unable to establish a connection to the LandmarkServer Web Service");
							//MessageBox.Show("Unable to establish a connection to the Landmark Web Service"
							//	, "TerraService Error", 
							//	MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							break;
						}
					}
				}

				if(failed && !initedOffline)
				{
					// try populate types from dataset:
					DataRow[] rows = m_landmarksDS.Tables["lmtype"].Select("");
					int numLandmarkTypes = rows.Length;
					if(numLandmarkTypes > 0)
					{
						landmarkPointTypes = new string[numLandmarkTypes];
						landmarkPointShow = new bool[numLandmarkTypes];
						stringToIndex.Clear();
						int i = 0;
						foreach(DataRow row in rows)
						{
							landmarkPointTypes[i] = ((string)row["type"]).ToLower();
							landmarkPointShow[i] = true;
							stringToIndex.Add (landmarkPointTypes[i], i);
							i++;
						}
					}
					if (numLandmarkTypes > 0 && landmarkPointTypes[0] == "terminal")
					{
						landmarkPointTypes[0] = "transportation terminal";
					}
					initedOffline = true;
				}
			}
		}

		public static void shutdown()
		{
			saveState();
		}

		public static void saveState()
		{
			/*
			if(TilesDSChanged)
			{
				m_tilesDS.WriteXml(m_tileDbFileName, XmlWriteMode.WriteSchema);
			}
			*/
			if(LandmarksDSChanged)
			{
				m_landmarksDS.WriteXml(m_landmarksDbFileName, XmlWriteMode.WriteSchema);
			}
		}

		public static void init()
		{
			setMappingCacheLocation();

			LibSys.StatusBar.WriteLine("Actual location for terraserver cache in " + Project.GetTerraserverMapsBasePath());

			/*
			try
			{
				m_tilesDS.ReadXml(m_tileDbFileName, XmlReadMode.IgnoreSchema);
			} 
			catch {}
			*/
			try
			{
				m_landmarksDS.ReadXml(m_landmarksDbFileName, XmlReadMode.IgnoreSchema);
			} 
			catch {}
		}

		/// <summary>
		/// remove all elements from cache - needed for full Refresh
		/// </summary>
		public static void Clear()
		{
			m_backdropCache.Clear();
		}

		/// <summary>
		/// remove old unused elements from cache
		/// </summary>
		public static void purge()
		{
			int cacheMinCount = Project.CACHE_MIN_UNUSED_COUNT;

			//LibSys.StatusBar.Trace("IP: TerraserverCache:purge()  before: " + ToString());

			m_backdropCache.purge(cacheMinCount);

			//LibSys.StatusBar.Trace("IP: TerraserverCache:purge()   after: " + ToString());
		}

		public static Backdrop getBackdrop(TileTerra tile, string baseName)
		{
			string imageFileName = tileImagePath(baseName);

			Backdrop ret = null;
			
			try 
			{
				if(m_backdropCache.ContainsKey(baseName))
				{
					ret = (Backdrop)m_backdropCache[baseName];	// may be IsEmpty, if proven that can't download
					ret.MarkUsed();
					//LibSys.StatusBar.Trace("OK: TerraserverCache:getBackdrop() - tile '" + baseName + "' - found in cache - " + ret);
					return ret;
				}

				bool loadedFromFile = false;
				if(!Project.reloadRefresh && File.Exists(imageFileName)) 
				{
					try 
					{
						ret = new Backdrop(imageFileName, baseName, true);
						//LibSys.StatusBar.Trace("OK: TerraserverCache:getBackdrop() - tile '" + baseName + "' - loaded from file");
						AddBackdrop(baseName, ret);
						loadedFromFile = true;
					} 
					catch {}
				} 

				if(!loadedFromFile && Project.serverAvailable)
				{
					string imageUrl = getImageUrl(baseName);

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

						//						tilesBeingLoadedCount++;

						//add dt worker method to the thread pool / queue a task 
						//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), baseName); 
						ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), baseName); 

#if DEBUG
						LibSys.StatusBar.Trace("OK: TerraserverCache:getBackdrop() - tile '" + baseName + "' - loading remote from " + imageUrl);
#endif
					} 
					else 
					{
						//LibSys.StatusBar.Trace("OK: TerraserverCache:getBackdrop() - tile '" + baseName + "' - already in cache");
					}
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

		public static string tileImagePath(string baseName)
		{
			return Project.GetTerraserverPath(baseName + imageExt);
		}

		public static string getImageUrl(string baseName)
		{
			// this is how the key is formed:
			//	string key = "T" + (Int32)tid.Theme + "-S" + (Int32)tid.Scale + "-Z" + tid.Scene 
			//					+ "-X" + tid.X + "-Y" + tid.Y;

			char[] sep = new char[1] { '-' };
			string[] split = baseName.Split(sep);


			StringBuilder imageUrl = new StringBuilder("http://", 256);
			
			imageUrl.Append(Project.TERRA_SERVER);
			imageUrl.Append("/tile.ashx?t=");
			imageUrl.Append(split[0].Substring(1));
			imageUrl.Append("&s=");
			imageUrl.Append(split[1].Substring(1));
			imageUrl.Append("&x=");
			imageUrl.Append(split[3].Substring(1));
			imageUrl.Append("&y=");
			imageUrl.Append(split[4].Substring(1));
			imageUrl.Append("&z=");
			imageUrl.Append(split[2].Substring(1));	

			return imageUrl.ToString();
		}

		/// <summary>
		/// helps when image file was corrupted, arranges reload.
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="baseName"></param>
		public static void resetBackdrop(TileTerra tile, string baseName)
		{
			string imageFileName = tileImagePath(baseName);
			try
			{
				if(Project.serverAvailable)
				{
					string imageUrl = getImageUrl(baseName);

					DownloadThread dt = new DownloadThread();
					dt.DownloadUrl = imageUrl;
					dt.tile = tile;
					dt.baseName = baseName;
					dt.fileName = imageFileName;
					dt.CompleteCallback += new DownloadCompleteHandler( imageDownloadCompleteCallback );
					dt.ProgressCallback += new DownloadProgressHandler( imageDownloadProgressCallback );
					dt.addMonitoredMethod = new AddMonitoredMethod(ProgressMonitor.addMonitored);

					//					tilesBeingLoadedCount++;

					//add dt worker method to the thread pool / queue a task 
					//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), baseName); 
					ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), baseName); 

#if DEBUG
					LibSys.StatusBar.Trace("OK: TerraserverCache:resetBackdrop() - tile '" + baseName + "' - loading remote from " + imageUrl);
#endif
				}
			} 
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("file '" + imageFileName + "' failed to load on reset: " + e.Message);
			}
		}

		/// <summary>
		/// serves silent preload
		/// </summary>
		public static void downloadIfMissing(string baseName)
		{
			string imageFileName = tileImagePath(baseName);

			try 
			{
				if(m_backdropCache.ContainsKey(baseName) || File.Exists(imageFileName) || !Project.serverAvailable)
				{
					return;
				}

				string imageUrl = getImageUrl(baseName);

				if(!m_backdropCache.ContainsKey(baseName)) 
				{
					DownloadThread dt = new DownloadThread();
					dt.DownloadUrl = imageUrl;
					dt.tile = null;
					dt.baseName = baseName;
					dt.fileName = imageFileName;
					dt.CompleteCallback += new DownloadCompleteHandler( imageDownloadCompleteCallback );
					dt.ProgressCallback += new DownloadProgressHandler( imageDownloadProgressCallback );
					dt.addMonitoredMethod = new AddMonitoredMethod(ProgressMonitor.addMonitored);

					//					tilesBeingLoadedCount++;

					//add dt worker method to the thread pool / queue a task 
					//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), baseName); 
					ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), baseName); 

#if DEBUG
					LibSys.StatusBar.Trace("OK: TerraserverCache:getBackdrop() - tile '" + baseName + "' - loading remote from " + imageUrl);
#endif
				} 
				else 
				{
					//LibSys.StatusBar.Trace("OK: TerraserverCache:getBackdrop() - tile '" + baseName + "' - already in cache");
				}
			} 
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("file '" + imageFileName + "' failed to load: " + e.Message);
			}
		}


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

		/*
		// support for offline operation:
		public static void registerTerraTile(string basename, Theme theme, Scale tileScale, GeoCoord topLeft, GeoCoord bottomRight)
		{
			try
			{
				// Create DataRow objects and add them to the DataTable
				DataRow myDataRow;
				myDataRow = m_tilesDS.Tables[0].NewRow();
				myDataRow["name"] = basename;
				myDataRow["theme"] = (int)theme;
				myDataRow["scale"] = (int)tileScale;
				myDataRow["tllat"] = topLeft.Lat;
				myDataRow["tllng"] = topLeft.Lng;
				myDataRow["brlat"] = bottomRight.Lat;
				myDataRow["brlng"] = bottomRight.Lng;
				m_tilesDS.Tables[0].Rows.Add(myDataRow);
				TilesDSChanged = true;
				//LibSys.StatusBar.Trace("IP: TerraserverCache:registerTerraTile() - " + basename + " - tl:" + topLeft.Lat + "," + topLeft.Lng + " br:" + bottomRight.Lat + "," + bottomRight.Lng);
			} 
			catch {}	// constraint exception means the tile has already been accounted for
		}
		*/

		private static void imageDownloadProgressCallback ( DownloadInfo info )
		{
			//LibSys.StatusBar.Trace("IP: TerraserverCache:imageDownloadProgressCallback() - " + info.baseName + " - " + info.bytesProcessed + " of " + info.dataLength);
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

		public static Hashtable TileNamesCollection = null;		// we need a fast Contains() operation, lists won't do.

		public static int tilesBeingLoadedCount { get { return ThreadPool2.WaitingCallbacks; } }
 
		private static void imageDownloadCompleteCallback ( object otile, DownloadInfo info, string imageFileName, byte[] dataDownloaded )
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: TerraserverCache:imageDownloadCompleteCallback() - " + info.baseName + " " + (dataDownloaded == null? "null" : "" + dataDownloaded.Length) + " bytes loaded");
#endif

//			tilesBeingLoadedCount = tilesBeingLoadedCount > 0 ? tilesBeingLoadedCount - 1 : 0;

			Backdrop backdrop = (Backdrop)m_backdropCache[info.baseName];
			TileTerra tile = (TileTerra)otile;	// can be null for preload

			if(dataDownloaded == null || dataDownloaded.Length < 100 || Project.is404(dataDownloaded)) 
			{
				string comment = dataDownloaded == null ? "no data" : "404";
				if(backdrop != null)
				{
					backdrop.IsEmpty = true;	// proven empty
				}
				ProgressMonitor.markComplete(info.monitored, false, comment);
			} 
				/*
				else if(dataDownloaded.Length == 8321)		// cottage cheese
				{
					string comment = "cottage cheese tile";
					backdrop.IsEmpty = true;	// proven empty
					tile.IsCottageCheese = true; 
					ProgressMonitor.markComplete(info.monitored, false, comment);
				}
				*/ 
			else 
			{
				FileStream fs = null;
				try 
				{
					string comment = "" + dataDownloaded.Length + " bytes";
					if(dataDownloaded.Length == 8321)		// cottage cheese
					{
						if(tile != null)
						{
							tile.IsCottageCheese = true;
						}
						comment = "cottage cheese tile";
					}
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
					LibSys.StatusBar.Error("" + e.Message);
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


		public static void AddLandmark(Landmark lm)
		{
			foreach(Landmark lmm in m_landmarks)
			{
				if(lmm.sameAs(lm))
				{
					return;
				}
			}
			m_landmarks.Add(lm);
		}

		public static void RegisterLandmark(Landmark lm)
		{
			// register the landmark with local database for disconnected operation.
			try
			{
				// Create DataRow objects and add them to the DataTable
				DataRow myDataRow;
				myDataRow = m_landmarksDS.Tables[0].NewRow();
				myDataRow["name"] = lm.Name;
				myDataRow["type"] = lm.LandmarkType;
				myDataRow["lat"]  = lm.Location.Lat;
				myDataRow["lng"]  = lm.Location.Lng;
				m_landmarksDS.Tables["lm"].Rows.Add(myDataRow);
				LandmarksDSChanged = true;
			} 
			catch {}	// constraint exception means the landmark has already been accounted for
		}

		/*
		 * actually m_landmarks contains them in visible area anyway
		/// <summary>
		/// returns ArrayList of Landmark
		/// </summary>
		/// <param name="topLeft"></param>
		/// <param name="bottomRight"></param>
		/// <returns></returns>
		public static ArrayList getLandmarksInArea(GeoCoord topLeft, GeoCoord bottomRight)
		{
			ArrayList ret = new ArrayList();

			double tllat = topLeft.Lat;
			double tllng = topLeft.Lng;
			double brlat = bottomRight.Lat;
			double brlng = bottomRight.Lng;

			// Landmarks array was cleaned in the beginning of ReTile().
			// now fill the landmarks from the database:
			string where = "lng > " + tllng + " AND lng <" + brlng
				+ " AND lat < " + tllat + " AND lat > " + brlat;

			DataRow[] rows = TerraserverCache.LandmarksDS.Tables["lm"].Select(where);

			if(rows != null && rows.Length > 0)
			{
				int numLandmarks = rows.Length;
				if(numLandmarks > 0)
				{
					foreach(DataRow row in rows)
					{
						string name = (string)row["name"];
						string type = (string)row["type"];
						double lat =  (double)row["lat"];
						double lng =  (double)row["lng"];
						Landmark lm = new Landmark(name, new GeoCoord(lng, lat, 0.0d), type);
						ret.Add(lm);
					}
				}
			}

			return ret;
		}
		*/

		/*
		/// <summary>
		/// returns ArrayList of Landmark
		/// </summary>
		/// <param name="topLeft"></param>
		/// <param name="bottomRight"></param>
		/// <returns></returns>
		public static ArrayList getLandmarksInArea(GeoCoord topLeft, GeoCoord bottomRight)
		{
			ArrayList ret = new ArrayList();

			double tllat = topLeft.Lat;
			double tllng = topLeft.Lng;
			double brlat = bottomRight.Lat;
			double brlng = bottomRight.Lng;

			foreach(Landmark lmm in m_landmarks)
			{
				double lat = lmm.Location.Lat;
				double lng = lmm.Location.Lng;
				if(lat >= brlat && lat <= tllat && lng >= tllng && lng <= brlng)
				{
					ret.Add(lmm);
				}
			}

			return ret;
		}
		*/
	}
}
