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

using LibSys;
using ICSharpCode.SharpZipLib.Zip;


namespace LibFormats
{
	/// <summary>
	/// FileGpxZip covers all zipp'ed GPX.Document types - including Geocaching.com Pocket Queries.
	/// </summary>
	public class FileGpxZip : FileGpx
	{
		public FileGpxZip()
		{
		}

		public static string FormatName { get { return "Zipped GPX or Photo Collection .zip;.gpz"; } }
		public static string FileExtension { get { return ".zip"; } }
		public static string FileExtensions { get { return "*.zip;*.gpz"; } }

		public FileGpxZip(InsertWaypoint insertWaypoint) : base(insertWaypoint)
		{
		}

		public override bool process(string url, string filename, string source)
		{
			bool ret = true;

			LibSys.StatusBar.Trace("IP: processing Zipped GPX file: " + filename);

            if (!File.Exists(filename))
            {
                LibSys.StatusBar.Error("File not found: " + filename);
                return false;
            }

            try
            {
                using (ZipFile zip = new ZipFile(filename))
                {
                    LibSys.StatusBar.Trace("IP: Opened Zip");

                    foreach (ZipEntry entry in zip)
                    {
                        //LibSys.StatusBar.Trace("IP: entry inside: " + entry.FileName);
                        if (entry.IsFile && AllFormats.isGpxFile(entry.Name))
                        {
                            LibSys.StatusBar.Trace("IP: processing enclosed: " + entry.Name);

                            XmlDocument xmlDoc = new XmlDocument();

                            Stream stream = zip.GetInputStream(entry);

                            using (StreamReader rdr = new StreamReader(stream))
                            {
                                xmlDoc.LoadXml(rdr.ReadToEnd());
                            }
                            //processGpx(url, xmlDoc, source + "|" + entry.FileName);
                            processGpx(url, xmlDoc, source);
                        }
                        if (entry.IsFile && AllFormats.isLocFile(entry.Name))
                        {
                            LibSys.StatusBar.Trace("IP: processing enclosed: " + entry.Name);

                            XmlDocument xmlDoc = new XmlDocument();

                            Stream stream = zip.GetInputStream(entry);

                            using (StreamReader rdr = new StreamReader(stream))
                            {
                                xmlDoc.LoadXml(rdr.ReadToEnd());
                            }

                            // we need to call processXml here, so no sense in using BaseFormat by file name:
                            FileGeocachingLoc prcLoc = new FileGeocachingLoc(m_insertWaypoint);

                            //prcLoc.processXml(url, xmlDoc, source + "|" + entry.FileName);
                            prcLoc.processXml(url, xmlDoc, source);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LibSys.StatusBar.Error("FileGpxZip process " + e.Message);
                ret = false;
            }
			return ret;
		}

	}
}
