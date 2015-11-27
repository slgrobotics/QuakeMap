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
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using System.Threading;
using System.Diagnostics;

using LibSys;
using ICSharpCode.SharpZipLib.Zip;

namespace LibFormats
{
	/// <summary>
	/// Summary description for FileKml.
	/// </summary>
	public class FileKml : BaseFormat
	{
		public DirectoryInfo homeFolder;

		private DirectoryInfo m_imagesFolder = null;
		public DirectoryInfo imagesFolder
		{ 
			get 
			{
				// create only if it was explicitly used:
				if(m_imagesFolder == null)
				{
					m_imagesFolder = Directory.CreateDirectory(Path.Combine(homeFolder.FullName, "images"));
				}
				return m_imagesFolder;
			}
		}

		public string homeFileName;
		public XmlDocument homeXmlDoc;
		public string nameSeed;

		public FileKml()
		{
		}

		public static string FormatName { get { return "Google Earth Compressed .kmz"; } }
		public static string FormatNameShort { get { return "Google Earth Compressed"; } }
		public static string FileExtension { get { return ".kmz"; } }
		public static string FileExtensions { get { return "*.kml;*.kmz"; } }

		public void initTempFolder()
		{
			nameSeed = String.Format("{0}", DateTime.Now.Ticks);

			string tempFolderName = Path.Combine(Path.GetTempPath(), nameSeed);
			Project.filesToDelete.Add(tempFolderName);

			homeFolder = Directory.CreateDirectory(tempFolderName);

			homeFileName = Path.Combine(homeFolder.FullName, "doc.kml");
		}

		public void addImage(Bitmap bitmap, string name)
		{
			string imagePath = Path.Combine(imagesFolder.FullName, name + ".jpg");

			bitmap.Save(imagePath, ImageFormat.Jpeg);
		}

		public void run()
		{
			string zipFileName = Path.Combine(Path.GetTempPath(), nameSeed + ".kmz");

			saveToKmz(zipFileName, true);

            LibSys.StatusBar.Trace("IP: running file: '" + zipFileName + "'");

			Project.RunFile(zipFileName);
        }

        #region saveToKmz()

        public void saveToKmz(string zipFileName, bool deleteOnExit)
		{
			// make sure that homeXmlDoc is filled
			homeXmlDoc.Save(homeFileName);

            using (FileStream fStream = new FileStream(zipFileName, FileMode.Create, FileAccess.Write))
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(homeFolder.FullName);

                using (ZipOutputStream outStream = new ZipOutputStream(fStream))
                {
                    outStream.UseZip64 = UseZip64.Off;

                    if (deleteOnExit)
                    {
                        Project.filesToDelete.Add(zipFileName);
                    }

                    DirectoryInfo di = new DirectoryInfo(homeFolder.FullName);

                    putFolderToKmz(outStream, di, true);

                  //outStream.Finish();
                }
                Directory.SetCurrentDirectory(currentDirectory);
            }
			GC.Collect();
		}

        /// <summary>
        /// recursive - at least for one level down
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="di"></param>
        /// <param name="isRoot"></param>
        private void putFolderToKmz(ZipOutputStream outStream, DirectoryInfo di, bool isRoot)
        {
            foreach (FileInfo fi in di.GetFiles())
            {
                string dirPrefix = isRoot ? "" : (di.Name + "/");
                outStream.IsStreamOwner = false;
                outStream.PutNextEntry(new ZipEntry(dirPrefix + fi.Name));
                using (FileStream inStream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                {
                    Project.CopyStream(inStream, outStream);
                }
            }

            foreach(DirectoryInfo subDi in di.GetDirectories())
            {
                outStream.PutNextEntry(new ZipEntry(subDi.Name + "/"));
                putFolderToKmz(outStream, subDi, false);
            }
        }

        #endregion // saveToKmz()

        public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			return ret;
		}

		#region Kml Point Formatter - special case that doesn't require KmlDocument

		private const string kmlPointFmt = 
			  "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
			+ "<kml xmlns=\"http://earth.google.com/kml/2.0\">"
			+ " <Placemark>"
			+ "  <description>"
			+ "   <![CDATA[{1}]]>"
			+ "  </description>"
			+ "  <name>{0}</name>"
			+ "  <LookAt>"
			+ "   <longitude>{2}</longitude>"
			+ "   <latitude>{3}</latitude>"
			+ "   <range>{4}</range>"
			+ "   <tilt>{5}</tilt>"
			+ "   <heading>{6}</heading>"
			+ "  </LookAt>"
			+ "  <visibility>{7}</visibility>"
			+ "   <Point>"
			+ "    <coordinates>{2},{3},1</coordinates>"
			+ "   </Point>"
			+ " </Placemark>"
			+ "</kml>";

		#endregion // Kml Point Formatter

		// special case that doesn't require KmlDocument:
		public static void runGoogleEarthAtPoint(CamPos campos, string name, string labelHtml)
		{
			double camElev = campos.Elev;	// meters
			double tilt  = 0.0d;			// 0.0 - looking straight down, 90.0 - along the surface
			double heading = 0.0d;			// 0.0 - map is North up  90.0 - East up
			int visibility = 0;				// 0 - label not visible, 1 - visible

			string kmlPoint = String.Format(kmlPointFmt, name, labelHtml, campos.Lng, campos.Lat,
				camElev, tilt, heading, visibility);

			runKml(kmlPoint);
		}

		private static void runKml(string kml)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(kml);

			runKml(xmlDoc);
		}

		public static void runKml(XmlDocument xmlDoc)
		{
			string tempFile = Path.Combine(Path.GetTempPath(), String.Format("{0}.kml", DateTime.Now.Ticks));
			Project.filesToDelete.Add(tempFile);

			xmlDoc.Save(tempFile);

			Project.RunFile(tempFile);
		}
	}
}
