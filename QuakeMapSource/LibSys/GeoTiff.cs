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
	/// <summary>
	/// GeoTiff class wraps all GEOTIFF-related system functionality, so that details of accessing the tif file info are abstracted.
	/// </summary>
	public class GeoTiff
	{
		// Variables used to store property values:
		public const string Executable = "FWTools\\listgeo.exe";
		public const string Bindirectory = "FWTools";

		// The -t tabledir flag overrides the programs concept of how to file the EPSG CSV files, causing it to look in directory "tabledir". 
		// -d makes corner coordinates appear as plain double numbers. 
		public static string Args = "-d";

		private string m_result = "";
		public string ResultString { get { return m_result.Length == 0 && m_error.Length == 0 ? "OK: command succeded" : m_result; } }

		private string m_error = "";
		public string ErrorString { get { return m_error; } }

		protected string m_mapName = "";
		public string MapName { get { return m_mapName; } set { m_mapName = value; } }

		private string m_fileName;
		public string FileName { get { return m_fileName; } }

		public double TopLeftLng = 0.0d;
		public double TopLeftLat = 0.0d;
		public double BottomLeftLng = 0.0d;
		public double BottomLeftLat = 0.0d;
		public double TopRightLng = 0.0d;
		public double TopRightLat = 0.0d;
		public double BottomRightLng = 0.0d;
		public double BottomRightLat = 0.0d;

		public bool isValid 
		{
			get
			{
				return m_error.Length == 0
					&& TopLeftLng < TopRightLng && TopLeftLat > BottomLeftLat
					&& BottomRightLat < TopRightLat && BottomRightLng > BottomLeftLng;
			}
		}

		public GeoTiff(string fileName)
		{
			m_fileName = fileName;

			// map name can be changed later if needed.
			FileInfo fi = new FileInfo(fileName);
			m_mapName = fi.Name;
		}

		// when making a GeoTIFF from image
		public void initImageOnly()
		{
		}

		/// <summary>
		/// Execute the listgeo command, parse it's output filling this object's properties.
		/// </summary>
		public void init()
		{
			m_error = "";
			Exception exc = null;

			CmdRunner runner = new CmdRunner();

			string outputText = "";
			try
			{
				runner.executeCommand(Path.Combine(Project.startupPath, Executable), Args + " \"" + m_fileName + "\"",
										Path.Combine(Project.startupPath, Bindirectory), null, out outputText);
				m_result = runner.OutputString;
				m_error = runner.ErrorString;
			}
			catch (Exception ee)
			{
				m_error = ee.Message;
				LibSys.StatusBar.Error(ee.Message);
				exc = ee;
			}

			if(runner.exitcode == 0)
			{
				try
				{
					parseListgeo(outputText);
					LibSys.StatusBar.Trace(outputText);
				}
				catch (Exception ee)
				{
					m_error = ee.Message;
					LibSys.StatusBar.Error(ee.Message);
					exc = ee;
				}
			}

			if(exc != null)
			{
				throw exc;
			}
		}

		/// <summary>
		/// parses listgeo.exe output, filling in GeoTiff's object properties.
		/// </summary>
		/// <param name="libgeoOutput"></param>
		private void parseListgeo(string libgeoOutput)
		{
			/*
Geotiff_Information:
   Version: 1
   Key_Revision: 0.1
   Tagged_Information:
	  ModelTiepointTag (2,3):
		 0                0                0                
		 664354.283       4601368.61       0                
	  ModelPixelScaleTag (1,3):
		 10.16            10.16            0                
	  End_Of_Tags.
   Keyed_Information:
	  GTModelTypeGeoKey (Short,1): ModelTypeProjected
	  GTRasterTypeGeoKey (Short,1): RasterPixelIsArea
	  ProjectedCSTypeGeoKey (Short,1): PCS_WGS84_UTM_zone_17N
	  PCSCitationGeoKey (Ascii,25): "UTM Zone 17 N with WGS84"
	  End_Of_Keys.
   End_Of_Geotiff.

PCS = 32617 (name unknown)
Projection = 16017 ()
Projection Method: CT_TransverseMercator
   ProjNatOriginLatGeoKey: 0.000000 (  0d 0' 0.00"N)
   ProjNatOriginLongGeoKey: -81.000000 ( 81d 0' 0.00"W)
   ProjScaleAtNatOriginGeoKey: 0.999600
   ProjFalseEastingGeoKey: 500000.000000 m
   ProjFalseNorthingGeoKey: 0.000000 m
GCS: 4326/WGS 84
Datum: 6326/World Geodetic System 1984
Ellipsoid: 7030/WGS 84 (6378137.00,6356752.31)
Prime Meridian: 8901/Greenwich (0.000000/  0d 0' 0.00"E)

Corner Coordinates:
Upper Left    (  664354.283, 4601368.607)  ( -79.0294261W,  41.5471041N)
Lower Left    (  664354.283, 4537888.927)  ( -79.0465367W,  40.9756259N)
Upper Right   (  765151.643, 4601368.607)  ( -77.8223338W,  41.5200659N)
Lower Right   (  765151.643, 4537888.927)  ( -77.8498913W,  40.9491226N)
Center        (  714752.963, 4569628.767)  ( -78.4369247W,  41.2495544N)
				 */

			StringReader reader = new StringReader (libgeoOutput);
				
			int state = 0;
			int cornerCount = 0;
			string line;
			bool hasTransformation = false;
			while((line=reader.ReadLine()) != null) 
			{
				try
				{
					switch(state)
					{
						case 0:
							if(line.StartsWith("Corner Coordinates:")) 
							{
								state = 10;
							}
							else if(line.IndexOf("ModelTransformationTag") != -1) 
							{
								hasTransformation = true;
							} 
							break;
						case 10:
							if(line.StartsWith("Upper Left")) 
							{
								parseCoordLine(line, out TopLeftLng, out TopLeftLat);
								cornerCount++;
							}
							if(line.StartsWith("Lower Left")) 
							{
								parseCoordLine(line, out BottomLeftLng, out BottomLeftLat);
								cornerCount++;
							}
							if(line.StartsWith("Upper Right")) 
							{
								parseCoordLine(line, out TopRightLng, out TopRightLat);
								cornerCount++;
							}
							if(line.StartsWith("Lower Right")) 
							{
								parseCoordLine(line, out BottomRightLng, out BottomRightLat);
								cornerCount++;
							}
							break;
					}
				}
				catch {}
			}

			if(cornerCount == 4)
			{
				// all went well with the corners parsing; now process transformation tags appearing in some weird GeoTIFFs:

				if(hasTransformation)
				{
					Project.ErrorBox(Project.mainForm, "Sorry, GeoTIFF ModelTransformationTag not supported, image may be misplaced or missing.");
				}
			}
			else
			{
				Project.ErrorBox(Project.mainForm, "Sorry, GeoTIFF files not having corner coordinates not supported.");
			}
		}

		private const string libTiffWin32Url = @"http://gnuwin32.sourceforge.net/packages/tiff.htm";
		private const string libTiffMustInstallMessage = @"Please install LibTIFF/Win32 to read this type of GeoTiff files.
The package can be found at {0} and must be installed in defalt location {1}";

		public Bitmap FromNoncontigImage()
		{
			Bitmap bitmap = null;
			m_error = "";
			Exception exc = null;

			string libTiffDir = Project.driveSystem + "Program Files\\GnuWin32";
			string libtiffcp = Path.Combine(libTiffDir, "bin\\tiffcp.exe");
			string cpArgs = "-p contig";

			if(!File.Exists(libtiffcp))
			{
				Project.ErrorBox(Project.mainForm, String.Format(libTiffMustInstallMessage, libTiffWin32Url, libTiffDir));
				return null;
			}

			CmdRunner runner = new CmdRunner();
			string tempFile = Path.GetTempFileName();
			Project.filesToDelete.Add(tempFile);

			string outputText = "";
			try
			{
				runner.executeCommand(libtiffcp, cpArgs + " \"" + m_fileName + "\" \"" + tempFile + "\"",
					Path.Combine(Project.startupPath, Bindirectory), null, out outputText);
				m_result = runner.OutputString;
				m_error = runner.ErrorString;
			}
			catch (Exception ee)
			{
				m_error = ee.Message;
				LibSys.StatusBar.Error(ee.Message);
				exc = ee;
			}

			if(runner.exitcode == 0)
			{
				try
				{
					LibSys.StatusBar.Trace(outputText);
					bitmap = new Bitmap(tempFile);
					LibSys.StatusBar.Trace("OK: contig tiff read in");
				}
				catch (Exception ee)
				{
					m_error = ee.Message;
					LibSys.StatusBar.Error(ee.Message);
					exc = ee;
				}
			}

			if(exc != null)
			{
				throw exc;
			}
			return bitmap;
		}

		/// <summary>
		/// parses listgeo.exe output strings related to corners
		/// </summary>
		/// <param name="line"></param>
		/// <param name="lng"></param>
		/// <param name="lat"></param>
		private void parseCoordLine(string line, out double lng, out double lat)
		{
//			Upper Left    (  664354.283, 4601368.607)  ( -79.0294261W,  41.5471041N)
			lng = 0.0d;
			lat = 0.0d;

			line = line.Substring(line.IndexOf(")"));
			line = line.Substring(line.IndexOf("(") + 1);
			string lineLng = line.Substring(0, line.IndexOf(",") - 1).Trim();
			string lineLat = line.Substring(line.IndexOf(",") + 1);
			lineLat = lineLat.Substring(0, lineLat.IndexOf(")") - 1).Trim();

			lng = Convert.ToDouble(lineLng);
			lat = Convert.ToDouble(lineLat);
		}
	}
}
