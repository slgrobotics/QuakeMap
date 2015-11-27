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
using LibSys;

namespace LibFormats
{
	public delegate bool FormatProcessor(string url, string filename, string source);

	/// <summary>
	/// To keep lists of all formats for use in import manager.
	/// </summary>
	public sealed class AllFormats
	{
		// all methods static, no way to instantiate this class

		public static string[] gpsDefaultExtension = new string[] {
																FileEasyGps.FileExtension,
																FileGpxZip.FileExtension,
																FileGeocachingLoc.FileExtension,
																FileNmeaLog.FileExtension,
																FilePlainWpt.FileExtension,
																FileStreetsTripsCsv.FileExtension,
																FileDelormeTxt.FileExtension,
																FileGeoTIFF.FileExtension,
																FileAwoisMdb.FileExtension
																//FilePlainWpt.FileExtension
															};

		public static string[] gpsDefaultExtensions = new string[] {
																FileEasyGps.FileExtensions,
																FileGpxZip.FileExtensions,
																FileGeocachingLoc.FileExtensions,
																FileNmeaLog.FileExtensions,
																FilePlainWpt.FileExtensions,
																FileStreetsTripsCsv.FileExtensions,
																FileDelormeTxt.FileExtensions,
																FileGeoTIFF.FileExtensions,
																FileAwoisMdb.FileExtensions
																//FilePlainWpt.FileExtensions
															};

		public static string[] gpsFormatNames = new string[] {
																FileEasyGps.FormatName,
																FileGpxZip.FormatName,
																FileGeocachingLoc.FormatName,
																FileNmeaLog.FormatName,
																FilePlainWpt.FormatName,
																FileStreetsTripsCsv.FormatName,
																FileDelormeTxt.FormatName,
																FileGeoTIFF.FormatName,
																FileAwoisMdb.FormatName
																//FilePlainWpt.FormatName
															};

		public static string[] gpsFormatHelps = new string[] {
																"Topographics .gpx file\n(see EasyGPS.com)",
																"Zip'ed .gpx files,\nPhoto collection .gpz\n",
																"Geocaching .loc file\n(see geocaching.com)",
																"GPS Tracker .log or .nmea file\n",
																"Garmin .wpt file\n(see garmin.com)",
																"Streets&Trips .csv file\n(Excel .csv)",
																"DeLorme .txt file\n(BlueLogger)",
																"GeoTIFF .tif file\n(example: USGS DRG Maps)",
																"AWOIS .mdb file\n(shipwrecks)"
																//"OziExplorer .wpt file\n(see oziexplorer.com)"
															};

		public static BaseFormat[] gpsFormatObjects = new BaseFormat[] {
																new FileEasyGps(),
																new FileGpxZip(),
																new FileGeocachingLoc(),
																new FileNmeaLog(),
																new FilePlainWpt(),
															    new FileStreetsTripsCsv(),
															    new FileDelormeTxt(),
															    new FileGeoTIFF(),
															    new FileAwoisMdb()
																//new FilePlainWpt()
															};

		// ----------------------------------------------------------------------

		public static string[] eqDefaultExtension = new string[]  {
																	".htm",
																	".htm",
																	".htm"
																  };

		public static string[] eqDefaultExtensions = new string[] {
																	  "*.htm;*.html;*.txt",
																	  "*.htm;*.html;*.txt",
																	  "*.htm;*.html;*.txt"
																  };

		public static string[] eqFormatNames = new string[] {
																"ANSS saved search .htm",
																"NGDC Significant Earthquake Database saved search .htm",
																"NGDC Earthquake Intensity Database saved search .htm"
															};

		public static string[] eqFormatHelps = new string[] {
																"ANSS saved search .htm file\n(see http://quake.geo.berkeley.edu/anss)",
																"NGDC Significant Earthquake Database saved search .htm file\n(see http://www.ngdc.noaa.gov/seg/hazard/sig_srch.shtml)",
																"NGDC Earthquake Intensity Database saved search .htm file\n(see http://www.ngdc.noaa.gov/seg/hazard/int_srch.shtml)"
															};

		public static BaseFormat[] eqFormatObjects = new BaseFormat[] {
																new FileCnssSavedSearch(),
																new FileSignificantSavedSearch(),
																new FileIntensitySavedSearch()
															};

		// ----------------------------------------------------------------------

		public static BaseFormat formatByName(string name)
		{
			for (int i=0; i < gpsFormatNames.Length ;i++)
			{
				if(gpsFormatNames[i].Equals(name))
				{
					return gpsFormatObjects[i];
				}
			}

			for (int i=0; i < eqFormatNames.Length ;i++)
			{
				if(eqFormatNames[i].Equals(name))
				{
					return eqFormatObjects[i];
				}
			}

			return null;
		}

		public static BaseFormat formatByFileName(string name)
		{
			name = name.ToLower();
			if(name.EndsWith(".gpx"))
			{
				return gpsFormatObjects[0];
			} 
			else if(isZipFile(name))
			{
				return gpsFormatObjects[1];
			} 
			else if(name.EndsWith(".loc"))
			{
				return gpsFormatObjects[2];
			}
            else if (name.EndsWith(".log") || name.EndsWith(".nmea"))
			{
				return gpsFormatObjects[3];
			}
			else if(name.EndsWith(".wpt"))
			{
				return gpsFormatObjects[4];
			}
			else if(name.EndsWith(".csv"))
			{
				return gpsFormatObjects[5];
			}
			else if(name.EndsWith(".txt"))
			{
				return gpsFormatObjects[6];
			}
			else if(isTiffFile(name))
			{
				return gpsFormatObjects[7];
			}
			else if(name.EndsWith(".mdb"))
			{
				return gpsFormatObjects[8];
			}

			return null;
		}

		public static string formatNameByFileName(string name)
		{
			name = name.ToLower();
			if(name.EndsWith(".gpx"))
			{
				return gpsFormatNames[0];
			} 
			else if(isZipFile(name))
			{
				return gpsFormatNames[1];
			}
			else if(name.EndsWith(".loc"))
			{
				return gpsFormatNames[2];
			}
            else if (name.EndsWith(".log") || name.EndsWith(".nmea"))
            {
                return gpsFormatNames[3];
            }
            else if (name.EndsWith(".wpt"))
			{
				return gpsFormatNames[4];
			}
			else if(name.EndsWith(".csv"))
			{
				return gpsFormatNames[5];
			}
			else if(name.EndsWith(".txt"))
			{
				return gpsFormatNames[6];
			}
			else if(isTiffFile(name))
			{
				return gpsFormatNames[7];
			}
			else if(name.ToLower().EndsWith(".mdb"))
			{
				return gpsFormatNames[8];
			}

			return null;
		}

		public static bool isZipFile(string filename)
		{
			filename = filename.ToLower();
			return filename.EndsWith(".zip") || filename.EndsWith(".gpz");
		}

		public static bool isTiffFile(string filename)
		{
			filename = filename.ToLower();
			return filename.EndsWith(".tif") || filename.EndsWith(".tiff");
		}

		public static bool isGpxFile(string filename)
		{
			filename = filename.ToLower();
			return filename.EndsWith(".gpx");
		}

		public static bool isLocFile(string filename)
		{
			filename = filename.ToLower();
			return filename.EndsWith(".loc");
		}

	}
}
