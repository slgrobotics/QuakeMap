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
using System.Collections;

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Class Api encapsulates all commands from external programs to QuakeMap.
	/// </summary>
	public class Api
	{
		private static VehicleGps m_vehicle = null;

		public static void processApiCall(string strArgs)
		{
			ArrayList files = new ArrayList();	// of string
			bool doMove = false;

			char[] splitter = { '|' };
			string[] args = strArgs.Split(splitter);
			GeoCoord pos = PictureManager.This.CameraManager.Location.Clone();

			switch(args[0])
			{
				case "api":		// something like "api|cmd|data...."
				switch (args[1])
				{
					case "refresh":
						PictureManager.This.CameraManager.ProcessCameraMove();
						break;

					case "resetzoom":
						WaypointsCache.resetBoundaries();
						break;

					case "dozoom":
						PictureManager.This.CameraManager.zoomToCorners();
						break;

					case "newwpt":
					{
						double lat = Convert.ToDouble(args[2]);
						double lng = Convert.ToDouble(args[3]);
						double elev = Convert.ToDouble(args[4]);
						DateTime dateTime = Convert.ToDateTime(args[5]);
						string name = args[6];			// like "GC12345"
						string urlName = args[7];		// like "Mike's cache"
						string type = args[8];			// like "geocache"
						string typeExtra = args[9];		// like "Geocache Found"
						string sym = args[10];			// like "Geocache"
						long trackid = Convert.ToInt64(args[11]);		// -1 is none, -2 - last track created
						if(trackid == -2)
						{
							trackid = Project.trackId;
						}
						string url = args[12];			// can be empty
						string descr = args[13];			// can be empty
						string source = args[14];		// can be empty
						bool keepInView = "true".Equals(args[15].ToLower());

						CreateInfo ci = new CreateInfo();

						ci.init(type);

						ci.comment = "";
						ci.dateTime = dateTime;
						ci.desc = descr;
						ci.elev = elev;
						ci.id = trackid;
						ci.lat = lat;
						ci.lng = lng;
						//ci.magn = 0.0d;
						ci.name = name;
						ci.source = source;
						ci.sym = sym;
						ci.typeExtra = typeExtra;
						ci.url = url;
						ci.urlName = urlName;

						WaypointsCache.insertWaypoint(ci);
						if(keepInView)
						{
							GeoCoord loc = new GeoCoord(lng, lat);
							PictureManager.This.CameraManager.keepInView(loc);
						}
					}
						break;

					case "delwpt":
					{
						string name = args[2];			// like "GC12345"
						string urlName = args[3];		// like "Mike's cache"
						string source = args[4];

						if(name.Length > 0)
						{
							WaypointsCache.RemoveWaypointByName(name);
						}
						else if(urlName.Length > 0)
						{
							WaypointsCache.RemoveWaypointByUrlname(urlName);
						}
						else if(source.Length > 0)
						{
							WaypointsCache.RemoveWaypointsBySource(source);
						}

						//PictureManager.This.CameraManager.ProcessCameraMove();
					}
						break;

					case "newtrk":
					{
						string trackName = args[2];			// like "trip home"
						string type = args[3];				// like "trk" or "rte"
						string source = args[4];			// can be empty

						CreateInfo ci = new CreateInfo();

						Project.trackId++;

						ci.init(type);
						ci.name = trackName;
						ci.id = Project.trackId;
						ci.source = source;

						WaypointsCache.insertWaypoint(ci);
					}
						break;

					case "deltrk":		// or route
					{
						string name = args[2];			// like "track45"			- if empty, use source
						string source = args[3];		// can delete by source		- if empty, delete the last one

						if(name.Length > 0)
						{
							WaypointsCache.RemoveTracksByName(name);
						}
						else if(source.Length > 0)
						{
							WaypointsCache.RemoveTracksBySource(source);
						}
						else
						{
							// move ID pointer to the highest yet existing track/route: 
							while(Project.trackId > 0 && WaypointsCache.getTrackById(Project.trackId) == null)
							{
								Project.trackId--;
							}

							if(Project.trackId >= 0)
							{
								WaypointsCache.RemoveTrackById(Project.trackId);

								// move ID pointer to the highest yet existing track/route: 
								while(Project.trackId > 0 && WaypointsCache.getTrackById(Project.trackId) == null)
								{
									Project.trackId--;
								}
							}
						}

						PictureManager.This.CameraManager.ProcessCameraMove();
					}
						break;

					case "newvehicle":
					{
						double lat = Convert.ToDouble(args[2]);
						double lng = Convert.ToDouble(args[3]);
						double elev = Convert.ToDouble(args[4]);
						DateTime dateTime = args[5].Length > 0 ? Convert.ToDateTime(args[5]) : Project.localToZulu(DateTime.Now);
						string name =   args[6];					// like "My Chevy"
						string sym =    args[7];					// like "1" "2"...  or empty
						string url =    args[8];					// can be empty
						string source = args[9];					// can be empty
						string desc =   args[10];					// can be empty
						bool keepInView = "true".Equals(args[11].ToLower());
						bool doTrackLog = "true".Equals(args[12].ToLower());

						GeoCoord loc = new GeoCoord(lng, lat, elev);

						GpsRealTimeData rtData = new GpsRealTimeData();
						rtData.location = loc;
						rtData.fix = 2;
						rtData.time = dateTime;

						VehicleGps vehicle = new VehicleGps(rtData, name, sym, source, url, desc, doTrackLog);	// also adds to VehicleCache
						vehicle.KeepInView = keepInView;
						VehiclesCache.addVehicle(vehicle);
					}
						break;

					case "movevehicle":
					{
						double lat = Convert.ToDouble(args[2]);
						double lng = Convert.ToDouble(args[3]);
						double elev = Convert.ToDouble(args[4]);
						DateTime dateTime = args[5].Length > 0 ? Convert.ToDateTime(args[5]) : Project.localToZulu(DateTime.Now);
						string name =   args[6];					// like "My Chevy"
						string sym =    args[7];					// like "1" "2"...  or empty
						string url =    args[8];					// can be empty
						string source = args[9];					// can be empty
						string desc =   args[10];					// can be empty
						bool keepInView = "true".Equals(args[11].ToLower());
						bool doTrackLog = "true".Equals(args[12].ToLower());

						GeoCoord loc = new GeoCoord(lng, lat, elev);

						GpsRealTimeData rtData = new GpsRealTimeData();
						rtData.location = loc;
						rtData.fix = 2;
						rtData.time = dateTime;

						VehicleGps vehicle = (VehicleGps)VehiclesCache.getVehicleByName(name);
						if(vehicle != null)
						{
							if(url.Length > 0)
							{
								vehicle.Url = url;
							}
							if(sym.Length > 0)
							{
								vehicle.Sym = sym;
								vehicle.setImage(sym);
							}
							if(desc.Length > 0)
							{
								vehicle.Desc = desc;
							}
							vehicle.KeepInView = keepInView;
							vehicle.doTrackLog = doTrackLog;
							vehicle.ProcessMove(rtData);
						}
					}
						break;

					case "delvehicle":
					{
						string name =   args[2];					// like "My Chevy" or empty
						string source = args[3];					// can be empty

						VehicleGps vehicle = (VehicleGps)VehiclesCache.getVehicleByName(name);
						if(vehicle != null)
						{
							bool wasTracking = vehicle.doTrackLog;
							VehiclesCache.deleteVehicle(vehicle);	// takes care of removing it from LayerVehicle
							if(wasTracking)
							{
								PictureManager.This.CameraManager.ProcessCameraMove();	// refresh to have track endpoints 
							}
						}
					}
						break;

					case "exporttofile":
					{
						string fileName =   args[2];

						int waypointCount = 0;
						int trkpointCount = 0;
						int tracksCount = 0;

						FileAndZipIO.saveGpx(fileName, WaypointsCache.TracksAll, true,
												WaypointsCache.WaypointsAll, true, out waypointCount, out trkpointCount, out tracksCount);

						if(waypointCount > 0 || trkpointCount > 0)
						{
							LibSys.StatusBar.Trace("OK: " + waypointCount + " waypoints and " + trkpointCount + " legs saved to file " + fileName);
						}
						else
						{
							LibSys.StatusBar.Error("failed to save to file (0 waypoints, 0 legs): " + fileName);
						}
					}
						break;
				}
					break;

				default:	// regular command-line style messages - lon, lat, elev, map, file

					foreach(string arg in args)  
					{
						if(arg.StartsWith("/"))
						{
#if DEBUG
							LibSys.StatusBar.Trace("option: '" + arg + "'");
#endif
							try
							{
								if(arg.ToLower().StartsWith("/lat="))
								{
									double lat = Convert.ToDouble(arg.Substring(5));
									if(lat < 90.0d && lat > -90.0d)
									{
										pos.Lat = lat;
										doMove = true;
									}
								}
								else if(arg.ToLower().StartsWith("/lon="))
								{
									double lon = Convert.ToDouble(arg.Substring(5));
									if(lon < 180.0d && lon > -180.0d)
									{
										pos.Lng = lon;
										doMove = true;
									}
								}
								else if(arg.ToLower().StartsWith("/elev="))
								{
									double elev = Convert.ToDouble(arg.Substring(6));
									if(elev < Project.CAMERA_HEIGHT_MAX * 1000.0d && elev > Project.CAMERA_HEIGHT_MIN_DEFAULT * 1000.0d)
									{
										pos.Elev = elev;
										doMove = true;
									}
								}
								else if(arg.ToLower().StartsWith("/map="))
								{
									string mode = arg.Substring(5);
									Project.mainCommand.setMapMode(mode);
									doMove = true;
								}
							}
							catch (Exception ee)
							{
								string message = "argument '" + arg + "' - wrong format (" + ee.Message + ")";
								LibSys.StatusBar.Error(message);
								Project.ErrorBox(null, message);
							}
						}
						else
						{
#if DEBUG
							LibSys.StatusBar.Trace("file: '" + arg + "'");
#endif
							files.Add(arg);
						}
					}

					if(files.Count > 0)
					{
						// similar to drag&drop:
						Cursor.Current = Cursors.WaitCursor; 
						Project.mainForm.Focus();
						Project.mainForm.BringToFront();

						try 
						{
							string[] fileNames = new string[files.Count];
					
							int ii = 0;
							foreach(string fileName in files)  
							{
								fileNames[ii] = fileName;
								ii++;
							}

							bool anySuccess = FileAndZipIO.readFiles(fileNames);

							if(anySuccess)	// we need to zoom into whole set of dropped files, or refresh in case of JPG
							{
								Project.mainCommand.wptEnabled(true);
								if(!PictureManager.This.CameraManager.zoomToCorners())
								{
									PictureManager.This.CameraManager.ProcessCameraMove();	// nowhere to zoom in - just refresh
								}
							}
						} 
						catch
						{
							LibSys.StatusBar.Trace("* API: cannot accept arguments '" + strArgs + "'");
						}
						Cursor.Current = Cursors.Default; 
					}
					break;
			}	// end switch

			if(doMove)
			{
				PictureManager.This.CameraManager.Location = pos;		// calls ProcesCameraMove()
			}
		}
	}
}
