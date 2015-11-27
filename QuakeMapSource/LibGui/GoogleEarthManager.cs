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
using System.Threading;

using LibSys;
using LibFormats;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// Summary description for GoogleEarthManager.
	/// </summary>
	public class GoogleEarthManager
	{
		public GoogleEarthManager()
		{
		}

		public static void runAtPoint(GeoCoord gpoint, double camElev, string name, string labelHtml)
		{
			FileKml.runGoogleEarthAtPoint(new CamPos(gpoint.Lng, gpoint.Lat, camElev), name, labelHtml);
		}

		public static KmlDocument createKmlAreaWithObjects(ArrayList wpts, ArrayList eqs, ArrayList trks)
		{
			KmlDocument kmlDoc = new KmlDocument("Selected Area", 1);

			if(wpts.Count > 0)
			{
				KmlFolder waypointsFolder = kmlDoc.CreateFolder("Waypoints");

				foreach(Waypoint wpt in wpts)
				{
					KmlWaypoint kWpt = new KmlWaypoint(waypointsFolder, wpt);
				}
			}

			if(eqs.Count > 0)
			{
				KmlFolder earthquakesFolder = kmlDoc.CreateFolder("Earthquakes");

				foreach(Earthquake eq in eqs)
				{
					KmlEarthquake kEq = new KmlEarthquake(earthquakesFolder, eq);
				}
			}

			if(trks.Count > 0)
			{
				KmlFolder tracksFolder = kmlDoc.CreateFolder("Tracks");

				foreach(Track trk in trks)
				{
					KmlTrack kTrack = new KmlTrack(tracksFolder, trk);
				}
			}

			kmlDoc.CreateAbout();

			return kmlDoc;
		}

		public static KmlDocument createKmlTrack(Track trk)
		{
			KmlDocument kmlDoc = new KmlDocument(trk.Name, 1);

			KmlFolder waypointsFolder = kmlDoc.CreateFolder("Waypoints");

			KmlFolder tracksFolder = kmlDoc.CreateFolder("Tracks");

			KmlTrack kTrack = new KmlTrack(tracksFolder, trk);

			kmlDoc.CreateAbout();

			return kmlDoc;
		}

		public static KmlDocument createKmlTracks(ArrayList trks)
		{
			KmlDocument kmlDoc = new KmlDocument("Selected Track" + (trks.Count > 1 ? "s" : (": " + ((Track)trks[0]).Name)), 1);

			KmlFolder waypointsFolder = kmlDoc.CreateFolder("Waypoints");

			KmlFolder tracksFolder = kmlDoc.CreateFolder("Tracks");

			foreach(Track trk in trks)
			{
				KmlTrack kTrack = new KmlTrack(tracksFolder, trk);
			}

			kmlDoc.CreateAbout();

			return kmlDoc;
		}

		public static KmlDocument createKmlWaypoints(ArrayList wpts)
		{
			KmlDocument kmlDoc = new KmlDocument("Selected Waypoint" + (wpts.Count > 1 ? "s" : (": " + ((Waypoint)wpts[0]).NameDisplayed)), 1);

			KmlFolder waypointsFolder = kmlDoc.CreateFolder("Waypoints");

			foreach(Waypoint wpt in wpts)
			{
				KmlWaypoint kWpt = new KmlWaypoint(waypointsFolder, wpt);
			}

			kmlDoc.CreateAbout();

			return kmlDoc;
		}

		public static KmlDocument createKmlEarthquakes(ArrayList eqs)
		{
			KmlDocument kmlDoc = new KmlDocument("Selected Earthquake" + (eqs.Count > 1 ? "s" : (": " + ((Earthquake)eqs[0]).NameDisplayed)), 1);

			KmlFolder earthquakesFolder = kmlDoc.CreateFolder("Earthquakes");

			foreach(Earthquake eq in eqs)
			{
				KmlEarthquake kEq = new KmlEarthquake(earthquakesFolder, eq);
			}

			kmlDoc.CreateAbout();

			return kmlDoc;
		}

		public static void runAreaWithObjects(GeoCoord camLoc, double camElev, ArrayList wpts, ArrayList eqs, ArrayList trks)
		{
			KmlDocument kmlDoc = createKmlAreaWithObjects(wpts, eqs, trks);

			kmlDoc.LookAt(camLoc, camElev, 0.0d, 0.0d);
			
			kmlDoc.run();
		}

		public static void runTrack(Track trk)
		{
			KmlDocument kmlDoc = createKmlTrack(trk);
			
			kmlDoc.run();
		}

		public static void runTracks(ArrayList trks)
		{
			if(trks.Count > 0)
			{
				KmlDocument kmlDoc = createKmlTracks(trks);
			
				kmlDoc.run();
			}
		}

		public static void runWaypoints(ArrayList wpts)
		{
			if(wpts.Count > 0)
			{
				KmlDocument kmlDoc = createKmlWaypoints(wpts);
			
				kmlDoc.run();
			}
		}

		public static void runEarthquakes(ArrayList eqs)
		{
			if(eqs.Count > 0)
			{
				KmlDocument kmlDoc = createKmlEarthquakes(eqs);
			
				kmlDoc.run();
			}
		}

		public static void saveTracksWaypoints(string filename, string name, ArrayList tracks, bool saveTracks,
													ArrayList wpts, bool saveWaypoints,
													out int waypointCount, out int trkpointCount)
		{
			waypointCount = 0;
			trkpointCount = 0;

			KmlDocument kmlDoc = new KmlDocument(name, 1);

			if(saveWaypoints)
			{
				KmlFolder waypointsFolder = kmlDoc.CreateFolder("Waypoints");

				foreach(Waypoint wpt in wpts)
				{
					waypointCount++;
					KmlWaypoint kWaypoint = new KmlWaypoint(waypointsFolder, wpt);
				}
			}

			if(saveTracks)
			{
				KmlFolder tracksFolder = kmlDoc.CreateFolder("Tracks");

				foreach(Track trk in tracks)
				{
					trkpointCount += trk.Trackpoints.Count;
					KmlTrack kTrack = new KmlTrack(tracksFolder, trk);
				}
			}

			kmlDoc.CreateAbout();
			
			kmlDoc.SaveToKmz(filename, false);
		}
	}
}
