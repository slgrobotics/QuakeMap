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
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Xml;

using LibSys;
using LibFormats;
using LibGeo;
using LibGui;

using ICSharpCode.SharpZipLib.Zip;

namespace WindowsTiler
{
    public class PhotoFolderToProcess		// list element
    {
        public string name;
        public ProcessFile imageFileProcessor;
    }

    /// <summary>
    /// A place to keep all IO related code --  FileAndZipIO.
    /// </summary>
    public sealed class FileAndZipIO
    {
        // all methods static, no way to instantiate this class

        public static bool isRepeatRead(string fileName)
        {
            bool repeatRead = false;
            foreach (FormattedFileDescr ffd in Project.FileDescrList)
            {
                if (ffd.filename.Equals(fileName))
                {
                    repeatRead = true;
                    break;
                }
            }
            return repeatRead;
        }

        #region Export to GPX and CSV formats

        /// <summary>
        /// returns true if file was successfully saved.
        /// </summary>
        /// <param name="tracks"></param>
        /// <param name="doSaveTracks"></param>
        /// <param name="waypoints"></param>
        /// <param name="doSaveWaypoints"></param>
        /// <param name="diag"></param>
        /// <returns></returns>
        public static bool saveGpx(string fileName, ArrayList tracks, bool doSaveTracks, ArrayList waypoints, bool doSaveWaypoints,
            out int waypointCount, out int trkpointCount, out int tracksCount)
        {
            waypointCount = 0;
            trkpointCount = 0;
            tracksCount = 0;

            string seedXml = Project.SEED_XML
                + "<gpx version=\"1.0\" creator=\"" + Project.PROGRAM_NAME_HUMAN + " " + Project.PROGRAM_VERSION_HUMAN + " - " + Project.WEBSITE_LINK_WEBSTYLE + "\"/>";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(seedXml);

            XmlNode root = xmlDoc.DocumentElement;

            Project.fillGpxRootNode(xmlDoc, root);	// fills in attributes so so that validation goes clean

            Project.SetValue(xmlDoc, root, "time", Project.localToZulu(DateTime.Now).ToString(Project.dataTimeFormat));

            if (doSaveWaypoints)
            {
                // GPX format requires that waypoints go before routes and tracks
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Waypoint wpt = (Waypoint)waypoints[i];
                    XmlNode wptNode = wpt.ToGpxXmlNode(xmlDoc);
                    root.AppendChild(wptNode);
                    waypointCount++;
                }
            }

            if (doSaveTracks)
            {
                // GPX format requires that routes go before tracks
                for (int i = 0; i < tracks.Count; i++)
                {
                    int cnt = 0;
                    Track track = (Track)tracks[i];
                    if (track.isRoute)
                    {
                        XmlNode wptNode = track.ToGpxXmlNode(xmlDoc, out cnt);
                        root.AppendChild(wptNode);
                        trkpointCount += cnt;
                    }
                }
                for (int i = 0; i < tracks.Count; i++)
                {
                    int cnt = 0;
                    Track track = (Track)tracks[i];
                    if (!track.isRoute)
                    {
                        XmlNode wptNode = track.ToGpxXmlNode(xmlDoc, out cnt);
                        root.AppendChild(wptNode);
                        trkpointCount += cnt;
                        tracksCount++;
                    }
                }
            }

            xmlDoc.Save(fileName);
            return true;
        }

        /// <summary>
        /// returns true if file was successfully saved.
        /// </summary>
        /// <param name="tracks"></param>
        /// <param name="doSaveTracks"></param>
        /// <param name="waypoints"></param>
        /// <param name="doSaveWaypoints"></param>
        /// <param name="diag"></param>
        /// <returns></returns>
        public static bool saveCsv(string fileName, ArrayList tracks, bool doSaveTracks, ArrayList waypoints, bool doSaveWaypoints, out int waypointCount, out int trkpointCount)
        {
            waypointCount = 0;
            trkpointCount = 0;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Name,Latitude,Longitude,Name 2,URL,Type,Elev,Description,Time,Track,Speed,Odometer,Found,Photo,Coord,Elev ft\n");

            if (doSaveWaypoints)
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Waypoint wpt = (Waypoint)waypoints[i];
                    stringBuilder.Append(wpt.ToStreetsTripsCsv());
                    waypointCount++;
                }
            }

            if (doSaveTracks)
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    Track track = (Track)tracks[i];
                    for (int j = 0; j < track.Trackpoints.Count; j++)
                    {
                        Waypoint wpt = (Waypoint)track.Trackpoints.GetByIndex(j);
                        stringBuilder.Append(wpt.ToStreetsTripsCsv());
                        trkpointCount++;
                    }
                }
            }

            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            TextWriter tw = new StreamWriter(fs);
            tw.WriteLine(stringBuilder.ToString());
            tw.Close();

            return true;
        }

        #endregion

        #region Photo Collection Processing

        /// <summary>
        /// fileName points to a file in a folder.
        /// imageFileProcessor can be null
        /// reads photoParameters, calls processImagesInPhotoFolder() to process images in the folder and subfolders
        /// returns number of successfully related images
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="imageFileProcessor"></param>
        /// <returns></returns>
        public static int processPhotoFolderByFile(string fileName, ProcessFile imageFileProcessor)	// processor can be null
        {
            int ret = 0;

            LibSys.StatusBar.Trace("IP: processPhotoFolderByFile file: " + fileName + " photoAnalyzeOnLoad=" + Project.photoAnalyzeOnLoad);

            bool isZipFile = AllFormats.isZipFile(fileName);

            FileAndZipIO.usedPhotoParametersFile = false;

            if (Project.photoAnalyzeOnLoad || isZipFile)
            {
                if (!isZipFile)
                {
                    warnPhotoAnalyzeOnLoad(fileName);	// can turn Project.photoAnalyzeOnLoad off
                }

                if (Project.photoAnalyzeOnLoad)
                {
                    try
                    {
                        if (imageFileProcessor == null)
                        {
                            imageFileProcessor = new ProcessFile(FileAndZipIO.processImageFile);
                        }

                        string folderPath;

                        if (isZipFile)
                        {
                            folderPath = fileName;
                        }
                        else
                        {
                            FileInfo fi = new FileInfo(fileName);

                            folderPath = fi.DirectoryName;
                        }

                        // read in whatever was stored in parameters file (like time shift):
                        bool hasFoundFile = false;
                        readPhotoParameters(folderPath, out hasFoundFile);
                        FileAndZipIO.usedPhotoParametersFile = hasFoundFile;

                        ret += processImagesInPhotoFolder(imageFileProcessor, folderPath, true);
                    }
                    catch { }
                }
            }
            return ret;
        }

        /// <summary>
        /// returns number of successfully related images. Does all the creating and attaching of the photoDescr and wpt.
        /// this is default image processor, with no user interface. It will be used when functions here are passed imageFileProcessor == null 
        /// a similar function is located in DlgPhotoManager.cs, where it allows previewing of images being processed.
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="zipEntry"></param>
        /// <param name="imageFileName"></param>
        /// <returns></returns>
        private static int processImageFile(ZipFile zip, ZipEntry zipEntry, string gpxFileName, string imageFileName)
        {
            int ret = 0;

            PhotoDescr photoDescr = PhotoDescr.FromFileOrZipEntry(zip, zipEntry, imageFileName, null);

            // relate to and create a trackpoint, using current Project.photoTimeShiftCurrent
            // (which is set to Camera by default, and to File if photoParameters file is found:
            Waypoint wpt = PhotoWaypoints.createPhotoTrackpoint(photoDescr, Project.photoTimeShiftCurrent, Project.photoTimeZoneIdCurrent);

            if (wpt != null)
            {
                // be careful here. The source is used for clearing images out, beautifying it can break the Reprocess.
                wpt.Source = (zip == null) ? gpxFileName : zip.Name;
                //				wpt.ImageSource = imageFileName;
                ret++;
            }

            if (photoDescr != null && photoDescr.image != null)
            {
                photoDescr.image.Dispose();
                photoDescr.image = null;
            }
            return ret;
        }

        private static ArrayList m_fileList = new ArrayList();		// temp list to hold photo files names

        // recursively goes through the current folder and its subfolders and picks all ".jpg" files into the list
        private static void fillImageFilesList(string folderName, bool doSubfolders)
        {
            // traverse directory structure and make a list of all .jpg files:
            DirectoryInfo di = new DirectoryInfo(folderName);

            foreach (FileInfo fi in di.GetFiles("*.jpg"))
            {
                m_fileList.Add(fi.FullName);
            }

            // try Hummingbird fishfinder BMP files:
            if (di.Name.ToUpper().EndsWith("SNAPSHOT"))
            {
                foreach (FileInfo fi in di.GetFiles("*.bmp"))
                {
                    m_fileList.Add(fi.FullName);
                }
            }

            if (doSubfolders)
            {
                foreach (DirectoryInfo ddi in di.GetDirectories())
                {
                    fillImageFilesList(ddi.FullName, true);
                }
            }
        }

        /// <summary>
        /// imageFileProcessor can be null.
        /// folderName is either a .zip/.gpz file name or a regular folder name
        /// processImagesInPhotoFolder() just calls imageFileProcessor on every file ending with ".jpg" - make sure time shift is in place before calling it
        /// returns number of photos realted to trackpoints.
        /// </summary>
        /// <param name="imageFileProcessor"></param>
        /// <param name="folderName"></param>
        /// <param name="doSubfolders"></param>
        /// <returns></returns>
        public static int processImagesInPhotoFolder(ProcessFile imageFileProcessor, string folderName, bool doSubfolders)
        {
            int ret = 0;
            m_fileList.Clear();

            if (imageFileProcessor == null)
            {
                imageFileProcessor = new ProcessFile(FileAndZipIO.processImageFile);
            }

            LibSys.StatusBar.Trace("IP: started bulk image analysis  in " + folderName + (doSubfolders ? " and subfolders" : "") + " -----------  " + DateTime.Now);

            if (AllFormats.isZipFile(folderName))
            {
                if (!File.Exists(folderName))
                {
                    LibSys.StatusBar.Error("Failed to open Zip");
                    throw new Exception("processImagesInPhotoFolder: Failed to open Zip " + folderName);
                }

                using (ZipFile zip = new ZipFile(folderName))
                {

                    foreach (ZipEntry entry in zip)
                    {
                        string fileName = entry.Name;
                        //LibSys.StatusBar.Trace("IP: File inside: " + fileName);
                        if (!entry.IsDirectory && fileName.ToLower().EndsWith(".jpg"))
                        {
                            try
                            {
                                LibSys.StatusBar.Trace("   ...processing enclosed image: " + fileName);

                                ret += imageFileProcessor(zip, entry, null, fileName);
                            }
                            catch (Exception exc)
                            {
                                LibSys.StatusBar.Error("Exception: " + exc); //.Message);
                            }
                        }
                    }
                }
            }
            else
            {
                fillImageFilesList(folderName, doSubfolders);

                foreach (string fileName in m_fileList)
                {
                    try
                    {
                        LibSys.StatusBar.Trace("   ...processing image file: " + fileName);

                        ret += imageFileProcessor(null, null, folderName, fileName);
                    }
                    catch (Exception exc)
                    {
                        LibSys.StatusBar.Error("Exception: " + exc); //.Message);
                    }
                }
            }

            if (ret > 0)
            {
                PhotoWaypoints.FirstWaypoint();	// set current waypoint to the first in list 
            }

            LibSys.StatusBar.Trace("OK: finished bulk image analysis in " + folderName + " - related " + ret + " photos -----------  " + DateTime.Now);

            return ret;
        }
        #endregion

        #region GPX and earthquakes files processing

        private static int elementCount = 0;

        // these ones we need for Drag&Drop:
        public static void insertWaypoint(CreateInfo createInfo)
        {
            elementCount++;
            WaypointsCache.insertWaypoint(createInfo);
        }

        public static void insertEarthquake(string[] infos)
        {
            elementCount++;
            EarthquakesCache.insertEarthquake(infos);
        }

        // knows about .gpx files and Project.photoAnalyzeOnLoad:
        public static void enlistPhotoFolder(string filename, SortedList foldersToProcess, ProcessFile imageFileProcessor)
        {
            string folderName = null;		// stays null for .loc files
            if (AllFormats.isZipFile(filename))
            {
                folderName = filename;
            }
            else if (AllFormats.isGpxFile(filename))
            {
                if (Project.photoAnalyzeOnLoad)
                {
                    FileInfo fi = new FileInfo(filename);
                    folderName = fi.DirectoryName;
                }
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(filename);
                if (di.Exists)
                {
                    folderName = di.FullName;
                }
            }

            // gpx and zip files are candidates for processing (containing) folder for photos:
            if (folderName != null && !foldersToProcess.ContainsKey(folderName))
            {
                // delayed processing of photo images in folder(s), as we need to have all trackpoints in memory first to relate to them:
                PhotoFolderToProcess pf = new PhotoFolderToProcess();
                pf.name = folderName;
                pf.imageFileProcessor = imageFileProcessor;
                foldersToProcess.Add(folderName, pf);
            }
        }

        #region readFile() - precise reading using supplied insert handlers, takes .jpg or format-based (.gpx,zip,gpz,loc,wpt) files
        /// <summary>
        /// imageFileProcessor can be null - the default one will be used, same with insertWaypointHandler and insertEarthquakeHandler
        /// 
        /// for a .jpg file just calls image file processor.
        /// for .gpx, .loc, .zip, .gpz and .wpt files calls BaseFormat.processor() and adds to foldersToProcess
        /// 
        /// returns true on success
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="insertWaypointHandler"></param>
        /// <param name="insertEarthquakeHandler"></param>
        /// <param name="imageFileProcessor"></param>
        /// <param name="foldersToProcess"></param>
        /// <returns></returns>
        public static bool readFile(string fileName, InsertWaypoint insertWaypointHandler, InsertEarthquake insertEarthquakeHandler,
                                        ProcessFile imageFileProcessor, SortedList foldersToProcess, bool addToRecent)
        {
            bool isSuccess = false;

            fileName = Project.GetLongPathName(new FileInfo(fileName).FullName);	// make sure we are not dealing with 8.3 notation here

            if (insertWaypointHandler == null)
            {
                insertWaypointHandler = new InsertWaypoint(WaypointsCache.insertWaypoint);
            }

            if (insertEarthquakeHandler == null)
            {
                insertEarthquakeHandler = new InsertEarthquake(EarthquakesCache.insertEarthquake);
            }

            if (imageFileProcessor == null)
            {
                imageFileProcessor = new ProcessFile(FileAndZipIO.processImageFile);
            }

            if (AllFormats.isTiffFile(fileName))
            {
                #region  Handle a GeoTiff file

                GeoTiff geoTiff = new GeoTiff(fileName);
                try
                {
                    geoTiff.init();
                }
                catch { }

                if (geoTiff.isValid)
                {
                    CustomMapsCache.RemoveCustomMapsBySource(fileName);

                    CustomMap cm = new CustomMapGeotiff(geoTiff);
                    Project.customMapId++;
                    cm.Id = Project.customMapId;
                    CustomMapsCache.AddCustomMap(cm);

                    WaypointsCache.pushBoundaries(cm.Location);
                    WaypointsCache.pushBoundaries(new GeoCoord(geoTiff.BottomLeftLng, geoTiff.BottomLeftLat));
                    WaypointsCache.pushBoundaries(new GeoCoord(geoTiff.TopRightLng, geoTiff.TopRightLat));
                    WaypointsCache.pushBoundaries(new GeoCoord(geoTiff.BottomRightLng, geoTiff.BottomRightLat));

                    // success, worth adding to imported list, if not repeat read:
                    bool repeatRead = isRepeatRead(fileName);
                    if (!repeatRead)
                    {
                        FormattedFileDescr ffd = new FormattedFileDescr(fileName, FileGeoTIFF.FormatName, false);
                        Project.FileDescrList.Add(ffd);
                    }
                    string msg = "OK: read file " + fileName + (repeatRead ? " (repeat read)" : "");
                    LibSys.StatusBar.Trace(msg);
                    LibSys.StatusBar.Trace("* " + msg);

                    isSuccess = true;	// will ensure refresh is called
                }
                else
                {
                    LibSys.StatusBar.Error("readFile - not a GeoTIFF file: " + fileName);
                }
                #endregion  // Handle a GeoTiff file
            }
            else if (fileName.ToLower().EndsWith(".jpg"))
            {
                #region Single JPEG image probably dropped

                PhotoWaypoints.cleanPhotoTrackpoints(fileName);
                try
                {
                    LibSys.StatusBar.Trace("   ...readFile - processing image file: " + fileName);
                    if (imageFileProcessor(null, null, fileName, fileName) > 0)
                    {
                        Project.photoFileName = fileName;
                        isSuccess = true;	// will ensure refresh is called
                    }
                }
                catch (Exception exc)
                {
                    LibSys.StatusBar.Error("Exception: " + exc.Message);
                }
                #endregion // Single JPEG image probably dropped
            }
            else
            {
                LibSys.StatusBar.Trace("   ...readFile - processing formatted file: " + fileName);

                #region Processing formatted file - zip, gpx

                BaseFormat format = AllFormats.formatByFileName(fileName);
                if (format != null)
                {
                    string formatName = AllFormats.formatNameByFileName(fileName);
                    format.InsertWaypoint = insertWaypointHandler;
                    format.InsertEarthquake = insertEarthquakeHandler;
                    FileListStruct fls = new FileListStruct(fileName, formatName, new FormatProcessor(format.process), false);
                    WaypointsCache.RemoveWaypointsBySource(fls.filename);
                    // actually read the file. If ZIP processor is there, it will read all enclosed .GPX files.
                    if (fls.processor("", fls.filename, fls.filename))		// boundaries pushed in WaypointsCache.insertWaypoint()
                    {
                        // knows about .gpx files and Project.photoAnalyzeOnLoad:
                        enlistPhotoFolder(fls.filename, foldersToProcess, imageFileProcessor);

                        // success, worth adding to imported list, if not repeat read:
                        bool repeatRead = isRepeatRead(fileName);
                        if (!repeatRead)
                        {
                            FormattedFileDescr ffd = new FormattedFileDescr(fileName, formatName, false);
                            Project.FileDescrList.Add(ffd);
                        }
                        string msg = "OK: read file " + fileName + (repeatRead ? " (repeat read)" : "");
                        LibSys.StatusBar.Trace(msg);
                        LibSys.StatusBar.Trace("* " + msg);
                        isSuccess = true;
                    }
                    else
                    {
                        LibSys.StatusBar.Trace("* Error: while processing file " + fileName);
                        Project.ErrorBox(Project.mainForm, "Error processing file:\n\n" + fls.filename);
                    }
                }
                else
                {
                    LibSys.StatusBar.Trace("* Error: unrecognized format " + fileName);
                    Project.ErrorBox(Project.mainForm, "Unrecognized format:\n\n" + fileName);
                }
                #endregion // Processing formatted file - zip, gpx

            }
            if (addToRecent && isSuccess)
            {
                Project.insertRecentFile(fileName);
            }
            return isSuccess;
        }
        #endregion

        #region readFiles() - for quick open based on file name(s) only
        /// <summary>
        /// used in Drag&Drop processing, Recent files processing, Quick Open - any place where only file name(s) are available.
        /// calls readFile() and processImagesInPhotoFolder() and adds to Recent files (inside readFile).
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static bool readFiles(string[] fileNames)
        {
            bool anySuccess = false;
            SortedList foldersToProcess = new SortedList();		// for delayed processing of images only

            WaypointsCache.resetBoundaries();

            FileAndZipIO.usedPhotoParametersFile = false;

            foreach (string fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    string fileNameFull = Project.GetLongPathName(new FileInfo(fileName).FullName);	// make sure we are not dealing with 8.3 notation here

                    warnPhotoAnalyzeOnLoad(fileNameFull);	// can turn Project.photoAnalyzeOnLoad off

                    LibSys.StatusBar.Trace("IP: reading file=" + fileNameFull);
                    LibSys.StatusBar.Trace("* reading file " + fileNameFull);

                    bool readOk = FileAndZipIO.readFile(fileNameFull, null, null, null, foldersToProcess, true);

                    anySuccess = anySuccess || readOk;

                    if (readOk && !AllFormats.isTiffFile(fileName))
                    {
                        Project.photoFileName = fileNameFull;		// last successfully processed
                    }
                }
                else if (Directory.Exists(fileName))
                {
                    // read all .gpx and .loc files in the folder first:
                    DirectoryInfo di = new DirectoryInfo(fileName);
                    string fileNameFull = Project.GetLongPathName(di.FullName);	// make sure we are not dealing with 8.3 notation here

                    LibSys.StatusBar.Trace("IP: processing formatted files in folder=" + fileNameFull);
                    LibSys.StatusBar.Trace("* processing folder " + fileNameFull);

                    bool hadGpx = false;
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        if (AllFormats.isGpxFile(fi.ToString()))
                        {
                            bool readOk = FileAndZipIO.readFile(fi.FullName, null, null, null, foldersToProcess, false);

                            hadGpx = hadGpx || readOk;
                            anySuccess = anySuccess || readOk;
                        }
                        else if (AllFormats.isLocFile(fi.ToString()))
                        {
                            bool readOk = FileAndZipIO.readFile(fi.FullName, null, null, null, foldersToProcess, false);

                            anySuccess = anySuccess || readOk;
                        }
                    }

                    if (hadGpx)
                    {
                        // knows about .gpx files and Project.photoAnalyzeOnLoad:
                        enlistPhotoFolder(fileNameFull, foldersToProcess, null);
                    }

                    if (anySuccess)
                    {
                        Project.photoFolderPath = fileNameFull;		// last successfully processed
                        Project.insertRecentFile(fileNameFull);
                    }
                }
                else
                {
                    LibSys.StatusBar.Error("non-existing file or folder - can't read: " + fileName);
                }
            }

            postProcessFolders(foldersToProcess, false);

            return anySuccess;
        }

        private static void postProcessFolders(SortedList foldersToProcess, bool persistName)
        {
            if (foldersToProcess.Count > 0)		// && Project.photoAnalyzeOnLoad 
            {
                // delayed processing of photo images in folder(s), as we need to have all trackpoints in memory first to relate to them:
                LibSys.StatusBar.Trace("IP: delayed processing...");
                for (int i = 0; i < foldersToProcess.Count; i++)
                {
                    PhotoFolderToProcess pf = (PhotoFolderToProcess)foldersToProcess.GetByIndex(i);

                    warnPhotoAnalyzeOnLoad(pf.name);	// can turn Project.photoAnalyzeOnLoad off
                    if (Project.photoAnalyzeOnLoad || AllFormats.isZipFile(pf.name))
                    {
                        LibSys.StatusBar.Trace("IP: delayed processing in folder=" + pf.name);

                        // use "my camera shift" as default for files w/o photo settings - reset for each folder:
                        Project.photoTimeShiftTypeCurrent = 0;

                        if (!Project.photoModeReprocess)
                        {
                            // may overwrite Project.photoTimeShiftFile, Project.photoTimeZoneIdFile:
                            FileAndZipIO.readPhotoParameters(pf.name, out FileAndZipIO.usedPhotoParametersFile);
                        }

                        // call imageProcessor on all image files in the folder:
                        if (processImagesInPhotoFolder(pf.imageFileProcessor, pf.name, true) > 0)
                        {
                            if (persistName)
                            {
                                FileInfo fi = new FileInfo(Project.GetLongPathName(pf.name));
                                Project.photoFileName = fi.FullName;
                            }
                        }
                    }
                }
            }
            else
            {
                LibSys.StatusBar.Trace("IP: no delayed processing - photoAnalyzeOnLoad=" + Project.photoAnalyzeOnLoad);
            }
        }

        #endregion

        #region QuickOpen() - brings up a OpenFileDialog and calls readFiles()
        /// <summary>
        /// Brings up file dialog, and then calls readFiles() to do the actual reading
        /// </summary>
        public static void QuickOpen()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();

            // use .gpx as default format:
            openFileDialog.InitialDirectory = Project.fileInitialDirectory;
            openFileDialog.FileName = "";
            openFileDialog.DefaultExt = ".gpx";
            openFileDialog.AddExtension = true;
            // "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            openFileDialog.Filter = "GPX, GPZ, ZIP and LOC files (*.gpx;*.gpz;*.zip;*.loc)|*.gpx;*.gpz;*.zip;*.loc|GPS Tracker file (*.log;*.nmea)|*.log;*.nmea|ZIP'ed GPX/Photo Collections (*.zip;*.gpz)|*.zip;*.gpz|GPS Exchange files (*.gpx)|*.gpx|Geocaching files (*.loc)|*.loc|DeLorme Earthmate BlueLogger files (*.txt)|*.txt|Comma separated Excel (*" + FileStreetsTripsCsv.FileExtension + ")|*" + FileStreetsTripsCsv.FileExtension + "|GeoTIFF (" + FileGeoTIFF.FileExtensions + ")|" + FileGeoTIFF.FileExtensions + "|AWOIS Shipwrecks (" + FileAwoisMdb.FileExtensions + ")|" + FileAwoisMdb.FileExtensions;
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                string fileName = openFileDialog.FileName;
                int pos = fileName.LastIndexOf("\\");
                if (pos > 0)
                {
                    Project.fileInitialDirectory = fileName.Substring(0, pos);
                    Project.photoFileName = fileName;
                }

                try
                {
                    string[] fileNames = new string[] { fileName };

                    bool anySuccess = FileAndZipIO.readFiles(fileNames);

                    if (anySuccess)	// we need to zoom into whole set of dropped files, or refresh in case of JPG
                    {
                        Project.mainCommand.wptEnabled(true);
                        if (!PictureManager.This.CameraManager.zoomToCorners())
                        {
                            PictureManager.This.CameraManager.ProcessCameraMove();	// nowhere to zoom in - just refresh
                        }
                    }
                }
                catch
                {
                    LibSys.StatusBar.Trace("* Error: file not valid: " + fileName);
                }
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region readPersistentAndDroppedFiles() - start-up time processing
        /// <summary>
        /// used in initial load processing; returns number of processed files:
        /// loads all files marked for persistent loading, and then those which were dropped on us (or supplied as cmd line args)
        /// </summary>
        /// <returns></returns>
        public static int readPersistentAndDroppedFiles(bool doZoom, Label messageLabel)
        {
            int ret = 0;

            LibSys.StatusBar.Trace("Read persistent and dropped files - " + doZoom);
            bool boundariesReset = false;		// also will cause zoom to corners, if true - a sign that dropped files are processed
            SortedList foldersToProcess = new SortedList();

            // actually load all files marked for persistent loading, and then those which were dropped on us (or supplied as cmd line args):
            foreach (FileListStruct fls in Project.FileList)
            {
                string fileName = fls.filename;
                try
                {
                    FileInfo fi = new FileInfo(Project.GetLongPathName(fls.filename));
                    fileName = fi.FullName;

                    messageLabel.Text = "...reading files...\n" + fileName;

                    if (!fls.persistent)
                    {
                        warnPhotoAnalyzeOnLoad(fileName);	// can turn Project.photoAnalyzeOnLoad off
                    }

                    if (!boundariesReset && !fls.persistent)	// first non-persistent file, the rest will be also the dropped ones
                    {
                        WaypointsCache.resetBoundaries();
                        boundariesReset = true;
                    }

                    WaypointsCache.RemoveWaypointsBySource(fileName);
                    CustomMapsCache.RemoveCustomMapsBySource(fileName);

                    // actually read the file. If ZIP processor is there, it will read all enclosed .GPX files.
                    if (fls.formatName.Equals(FileGeoTIFF.FormatName))
                    {
                        string[] fileNames = new string[] { fileName };

                        bool anySuccess = FileAndZipIO.readFiles(fileNames);
                    }
                    else
                    {
                        if (fls.processor("", fileName, fileName))		// boundaries pushed in WaypointsCache.insertWaypoint()
                        {
                            // knows about .gpx files and Project.photoAnalyzeOnLoad:
                            enlistPhotoFolder(fileName, foldersToProcess, null);

                            if (!fls.persistent)
                            {
                                // success, worth adding to imported list, if not repeat read:
                                bool repeatRead = isRepeatRead(fileName);
                                if (!repeatRead)
                                {
                                    FormattedFileDescr ffd = new FormattedFileDescr(fileName, fls.formatName, false);
                                    Project.FileDescrList.Add(ffd);
                                }
                            }
                            ret++;
                        }
                        else
                        {
                            Project.ErrorBox(Project.mainForm, "Error processing file:\n\n" + fileName);
                            boundariesReset = false;	// no zoom to corners
                        }
                    }
                }
                catch
                {
                    Project.ErrorBox(Project.mainForm, "Error processing file:\n\n" + fileName);
                    boundariesReset = false;	// no zoom to corners
                }
            }

            postProcessFolders(foldersToProcess, true);

            if (doZoom && boundariesReset)	// we need to zoom into whole set of dropped files
            {
                //				LibSys.StatusBar.Trace("zooming into set");
                Project.mainCommand.wptEnabled(true);
                PictureManager.This.CameraManager.zoomToCorners();
            }
            else
            {
                //				LibSys.StatusBar.Trace("just ProcessCameraMove()");
                PictureManager.This.CameraManager.ProcessCameraMove();
            }

            if (ret > 0)
            {
                string msg = "OK: done reading files... (" + ret + " total)";
                messageLabel.Text = msg;
                LibSys.StatusBar.Trace(msg);
            }
            LibSys.StatusBar.Trace("Read persistent and dropped files finished");
            return ret;
        }
        #endregion

        #endregion

        #region warnPhotoAnalyzeOnLoad()

        private static bool hasWarned = false;

        private static void warnPhotoAnalyzeOnLoad(string fileName)
        {
            if (hasWarned || !Project.photoAnalyzeOnLoad || !Project.photoAnalyzeOnLoadReminder)
            {
                return;
            }

            string folderName = "current folder";
            try
            {
                if (Directory.Exists(fileName))
                {
                    //folderName = fileName;
                    return;
                }
                if (File.Exists(fileName))
                {
                    //if(AllFormats.isLocFile(fileName) || AllFormats.isZipFile(fileName))
                    if (!AllFormats.isGpxFile(fileName))
                    {
                        return;
                    }
                    folderName = (new FileInfo(fileName)).DirectoryName;
                }
            }
            catch { }

            string sWarn = "Warning: will analyze all JPEG files in \n" + folderName + " and subfolders.\n\n"
                + "To turn this mode off answer \"No\" or use Photo-->Options menu";

            //Project.MessageBox(null, sWarn);
            if (!Project.YesNoBox(null, sWarn))
            {
                Project.photoAnalyzeOnLoad = false;
            }
            hasWarned = true;
        }

        #endregion

        #region Helper method unzipOneFile() - unzip an archive, extracting a specified file

        public static void unzipOneFile(string zipfilePath, string entryName, string plainfilePath)
        {
            if (!File.Exists(zipfilePath))
            {
                throw new Exception("failed to open Zip " + zipfilePath);
            }

            using (ZipFile zip = new ZipFile(zipfilePath))
            {
                ZipEntry zipEntry = zip.GetEntry(entryName);
                if (zipEntry != null)
                {
                    Stream zipEntryStream = zip.GetInputStream(zipEntry);

                    FileStream fileStream = new FileStream(plainfilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    Project.CopyStream(zipEntryStream, fileStream);
                    fileStream.Close();
                    zipEntryStream.Close();
                }
                else
                {
                    LibSys.StatusBar.Trace("IP: " + entryName + " -- NOT FOUND in archive " + zipfilePath);
                }
            }
            GC.Collect();
        }
        #endregion

        public static string lastPhotoParametersFileName = null;
        public static string lastPhotoParametersMessage = "";
        public static bool usedPhotoParametersFile = false;

        #region photoParameters file - read, save, create, modify

        /// <summary>
        /// returns a short message to be displayed in dialog "progress" area
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string readPhotoParameters(string folderPath, out bool hasFoundFile)
        {
            string ret = "";
            lastPhotoParametersFileName = null;
            hasFoundFile = false;
            bool isZip = AllFormats.isZipFile(folderPath);

            string ppfName = isZip ? (folderPath + "|" + Project.PHOTO_PARAM_FILE_NAME)
                                    : Path.Combine(folderPath, Project.PHOTO_PARAM_FILE_NAME);
            string traceMsg = "reading photo parameters file: " + ppfName;
            try
            {
                XmlDocument xmlDoc = null;

                if (isZip)
                {
                    if (!File.Exists(folderPath))
                    {
                        LibSys.StatusBar.Error(traceMsg + " -- NOT FOUND");
                        throw new Exception("failed to open Zip " + folderPath);
                    }

                    using (ZipFile zip = new ZipFile(folderPath))
                    {
                        ZipEntry entry = zip.GetEntry(Project.PHOTO_PARAM_FILE_NAME);
                        if (entry != null)
                        {
                            string fileName = entry.Name;
                            LibSys.StatusBar.Trace("IP: " + traceMsg);
                            xmlDoc = new XmlDocument();

                            Stream stream = zip.GetInputStream(entry);

                            using (StreamReader rdr = new StreamReader(stream))
                            {
                                xmlDoc.LoadXml(rdr.ReadToEnd());
                            }
                        }
                        else
                        {
                            ret = "no " + Project.PHOTO_PARAM_FILE_NAME + " found in archive";
                            LibSys.StatusBar.Trace("IP: " + traceMsg + " -- NOT FOUND");
                        }
                    }
                }
                else  // not a zip file
                {
                    if (File.Exists(ppfName))
                    {
                        LibSys.StatusBar.Trace("IP: " + traceMsg);
                        xmlDoc = new XmlDocument();
                        xmlDoc.Load(ppfName);
                    }
                    else
                    {
                        ret = "no " + Project.PHOTO_PARAM_FILE_NAME + " found in folder";
                        LibSys.StatusBar.Trace("IP: " + traceMsg + " -- NOT FOUND");
                    }
                }

                if (xmlDoc != null)
                {
                    foreach (XmlNode nnode in xmlDoc.DocumentElement.ChildNodes)
                    {
                        if (nnode.Name.Equals("cameraSettings"))
                        {
                            foreach (XmlNode node in nnode)
                            {
                                string nodeName = node.Name;
                                try
                                {
                                    if (nodeName.Equals("cameraTimeShift"))
                                    {
                                        Project.photoTimeShiftFile = new TimeSpan((long)Convert.ToInt64(node.InnerText));
                                        ret = "cameraTimeShift: " + Project.photoTimeShiftFile.TotalSeconds + " seconds";
                                        LibSys.StatusBar.Trace("IP: " + ret);
                                    }
                                    else if (nodeName.Equals("cameraTimeZoneID"))
                                    {
                                        Project.photoTimeZoneIdFile = Convert.ToInt32(node.InnerText);
                                        ret = "Read from the " + Project.PHOTO_PARAM_FILE_NAME + " file in the " + (isZip ? "zipfile" : "folder") + " - Camera Time Zone ID: " + Project.photoTimeZoneIdFile + " (" + MyTimeZone.timeZoneById(Project.photoTimeZoneIdFile).ToString() + ")";
                                        LibSys.StatusBar.Trace("IP: " + ret);
                                    }
                                }
                                catch (Exception ee)
                                {
                                    // bad node - not a big deal...
                                    LibSys.StatusBar.Error("readPhotoParameters() node=" + nodeName + " " + ee.Message);
                                }
                            }
                        }
                    }
                    hasFoundFile = true;
                    lastPhotoParametersFileName = ppfName;
                    Project.photoTimeShiftTypeCurrent = 2;		// found file, use it!
                }
            }
            catch (Exception e)
            {
                LibSys.StatusBar.Trace(traceMsg);
                LibSys.StatusBar.Error("readPhotoParameters() " + e.Message);
                ret = e.Message;
            }

            lastPhotoParametersMessage = ret;

            return ret;
        }

        public static void savePhotoParameters(Form ownerForm, string folderOrZipName)
        {
            if (Project.photoParametersDonotwrite)
            {
                return;
            }

            if (AllFormats.isZipFile(folderOrZipName))
            {
                if (!File.Exists(folderOrZipName))
                {
                    Project.ErrorBox(ownerForm, "Zip file not found: " + folderOrZipName);
                    return;
                }

                string ppfName = folderOrZipName + "|" + Project.PHOTO_PARAM_FILE_NAME;
                string message = "Confirm: will write new Camera Time Shift to " + ppfName
                                    + "\n\nDo you want to create/overwrite the file (inside the ZIP archive)?";

                if (!Project.photoParametersAsk || Project.YesNoBox(ownerForm, message))
                {
                    if (!File.Exists(folderOrZipName))
                    {
                        Project.ErrorBox(ownerForm, "Failed to open Zip file: " + folderOrZipName);
                        return;
                    }

                    XmlDocument xmlDoc = null;

                    using (ZipFile zip = new ZipFile(folderOrZipName))
                    {
                        ZipEntry entry = zip.GetEntry(Project.PHOTO_PARAM_FILE_NAME);
                        if (entry == null)
                        {
                            LibSys.StatusBar.Trace("IP: creating photo parameters entry: " + folderOrZipName + "|" + Project.PHOTO_PARAM_FILE_NAME);
                            xmlDoc = makePhotoParametersXmlDoc();
                            zip.BeginUpdate();
                        }
                        else
                        {
                            string fileName = entry.Name;
                            LibSys.StatusBar.Trace("IP: existing photo parameters entry: " + folderOrZipName + "|" + fileName);

                            xmlDoc = new XmlDocument();
                            Stream stream = zip.GetInputStream(entry);

                            using (StreamReader rdr = new StreamReader(stream))
                            {
                                xmlDoc.LoadXml(rdr.ReadToEnd());
                            }

                            editPhotoParametersXmlDoc(xmlDoc);

                            zip.BeginUpdate();
                            zip.Delete(entry);
                        }

                        StringMemoryDataSource m = new StringMemoryDataSource(xmlDoc.OuterXml);

                        zip.Add(m, Project.PHOTO_PARAM_FILE_NAME);

                        lastPhotoParametersFileName = ppfName;
                            
                        zip.CommitUpdate();
                    }
                }
            }
            else
            {
                if (!Directory.Exists(folderOrZipName))
                {
                    Project.ErrorBox(ownerForm, "Folder not found: " + folderOrZipName);
                    return;
                }

                string ppfName = Path.Combine(folderOrZipName, Project.PHOTO_PARAM_FILE_NAME);
                string message = "Confirm: will write new Camera Time Shift to " + ppfName + "\n\nDo you want to "
                    + (File.Exists(ppfName) ? "overwrite" : "create") + " the file?";
                if (!Project.photoParametersAsk || Project.YesNoBox(ownerForm, message))
                {
                    try
                    {
                        if (File.Exists(ppfName))
                        {
                            LibSys.StatusBar.Trace("IP: existing photo parameters file: " + ppfName);
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.Load(ppfName);

                            editPhotoParametersXmlDoc(xmlDoc);

                            xmlDoc.Save(ppfName);

                            lastPhotoParametersFileName = ppfName;
                        }
                        else
                        {
                            LibSys.StatusBar.Trace("IP: creating photo parameters file: " + ppfName);
                            XmlDocument xmlDoc = makePhotoParametersXmlDoc();

                            xmlDoc.Save(ppfName);

                            lastPhotoParametersFileName = ppfName;
                        }
                    }
                    catch (Exception e)
                    {
                        LibSys.StatusBar.Error("DlgPhotoManager:savePhotoParameters() " + e.Message);
                    }
                }
            }
        }

        private static XmlDocument makePhotoParametersXmlDoc()
        {
            string seedXml = Project.SEED_XML + "<photoParameters></photoParameters>";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(seedXml);

            XmlNode root = xmlDoc.DocumentElement;

            XmlNode cameraSettingsNode = Project.SetValue(xmlDoc, root, "cameraSettings", "");

            Project.SetValue(xmlDoc, cameraSettingsNode, "cameraTimeShift", "" + Project.photoTimeShiftFile.Ticks);
            LibSys.StatusBar.Trace("IP: new cameraTimeShift: " + Project.photoTimeShiftFile.TotalSeconds + " seconds");

            Project.SetValue(xmlDoc, cameraSettingsNode, "cameraTimeZoneID", "" + Project.photoTimeZoneIdFile);
            LibSys.StatusBar.Trace("IP: new cameraTimeZone: " + Project.photoTimeZoneIdFile);

            return xmlDoc;
        }

        // returns number of modified nodes
        // uses Project.photoTimeShiftCurrent, Project.photoTimeZoneIdCurrent to create/replace values in xmlDoc
        private static int editPhotoParametersXmlDoc(XmlDocument xmlDoc)
        {
            int ret = 0;

            XmlNode root = xmlDoc.DocumentElement;

            Project.CreateIfMissing(xmlDoc, root, "cameraSettings", "");

            foreach (XmlNode nnode in xmlDoc.DocumentElement.ChildNodes)
            {
                if (nnode.Name.Equals("cameraSettings"))
                {
                    TimeSpan ts = Project.photoTimeShiftCurrent;
                    int tzId = Project.photoTimeZoneIdCurrent;
                    ret += Project.SetReplaceValue(xmlDoc, nnode, "cameraTimeShift", "" + ts.Ticks);
                    LibSys.StatusBar.Trace("IP: set cameraTimeShift: " + ts);
                    ret += Project.SetReplaceValue(xmlDoc, nnode, "cameraTimeZoneID", "" + tzId);
                    LibSys.StatusBar.Trace("IP: set cameraTimeZoneID: " + tzId
                        + "(" + MyTimeZone.timeZoneById(tzId).shiftHours + ")");
                }
            }

            return ret;
        }
        #endregion

        #region photoPositions file - create/save only

        public static void savePhotoPositions(Form ownerForm, string folderOrZipName)
        {
            if (Project.photoPositionsDonotwrite)
            {
                return;
            }

            XmlDocument xmlDoc = makePhotoPositionsXmlDoc();

            if (AllFormats.isZipFile(folderOrZipName))
            {
                if (!File.Exists(folderOrZipName))
                {
                    Project.ErrorBox(ownerForm, "Zip file not found: " + folderOrZipName);
                    return;
                }

                string ppfName = folderOrZipName + "|" + Project.PHOTO_POSITIONS_FILE_NAME;

                string message = "Confirm: will write photo positions to " + ppfName
                    + "\n\nDo you want to create/overwrite the file (inside the ZIP archive)?";

                if (!Project.photoPositionsAsk || Project.YesNoBox(ownerForm, message))
                {
                    if (!File.Exists(folderOrZipName))
                    {
                        Project.ErrorBox(ownerForm, "Failed to open Zip file: " + folderOrZipName);
                        return;
                    }

                    using (ZipFile zip = new ZipFile(folderOrZipName))
                    {
                        ZipEntry entry = zip.GetEntry(Project.PHOTO_POSITIONS_FILE_NAME);
                        if (entry == null)
                        {
                            LibSys.StatusBar.Trace("IP: creating photo positions entry: " + folderOrZipName + "|" + Project.PHOTO_POSITIONS_FILE_NAME);
                            zip.BeginUpdate();
                        }
                        else
                        {
                            string fileName = entry.Name;
                            LibSys.StatusBar.Trace("IP: existing photo positions entry: " + folderOrZipName + "|" + fileName);

                            zip.BeginUpdate();
                            zip.Delete(entry);
                        }

                        StringMemoryDataSource m = new StringMemoryDataSource(xmlDoc.OuterXml);

                        zip.Add(m, Project.PHOTO_POSITIONS_FILE_NAME);

                        zip.CommitUpdate();
                    }
                }
            }
            else
            {
                if (!Directory.Exists(folderOrZipName))
                {
                    Project.ErrorBox(ownerForm, "Folder not found: " + folderOrZipName);
                    return;
                }

                string ppfName = Path.Combine(folderOrZipName, Project.PHOTO_POSITIONS_FILE_NAME);
                try
                {
                    if (File.Exists(ppfName))
                    {
                        LibSys.StatusBar.Trace("IP: existing photo positions file: " + ppfName);
                    }
                    else
                    {
                        LibSys.StatusBar.Trace("IP: creating photo positions file: " + ppfName);
                    }
                    xmlDoc.Save(ppfName);
                }
                catch (Exception e)
                {
                    LibSys.StatusBar.Error("DlgPhotoManager:savePhotoPositions() " + e.Message);
                }
            }
        }

        private static XmlDocument makePhotoPositionsXmlDoc()
        {
            string seedXml = Project.SEED_XML + "<photoPositions></photoPositions>";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(seedXml);

            XmlNode root = xmlDoc.DocumentElement;

            for (int i = 0; i < PhotoWaypoints.WaypointsWithThumbs.Count; i++)
            {
                Waypoint wpt = (Waypoint)PhotoWaypoints.WaypointsWithThumbs.GetByIndex(i);

                // wpt.Location.Lng, wpt.Location.Lat, wpt.Location.Elev
                if (wpt != null)
                {
                    XmlNode node = Project.SetValue(xmlDoc, root, "photo", "");

                    GeoCoord location = wpt.Location;

                    XmlAttribute attr = xmlDoc.CreateAttribute("lon");
                    attr.InnerText = "" + ((float)location.Lng);
                    node.Attributes.Append(attr);
                    attr = xmlDoc.CreateAttribute("lat");
                    attr.InnerText = "" + ((float)location.Lat);
                    node.Attributes.Append(attr);
                    attr = xmlDoc.CreateAttribute("elev");
                    attr.InnerText = "" + ((float)location.Elev);
                    node.Attributes.Append(attr);
                    attr = xmlDoc.CreateAttribute("image");
                    attr.InnerText = wpt.Desc;
                    node.Attributes.Append(attr);

                    root.AppendChild(node);
                }
            }

            return xmlDoc;
        }

        #endregion
    }

    #region Zip helpers

    class MemoryDataSource : IStaticDataSource
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance.
		/// </summary>
		/// <param name="data">The data to provide.</param>
		public MemoryDataSource(byte[] data)
		{
			data_ = data;
		}
		#endregion

		#region IDataSource Members

		/// <summary>
		/// Get a Stream for this <see cref="DataSource"/>
		/// </summary>
		/// <returns>Returns a <see cref="Stream"/></returns>
		public Stream GetSource()
		{
			return new MemoryStream(data_);
		}
		#endregion

		#region Instance Fields
		byte[] data_;
		#endregion
	}

	class StringMemoryDataSource : MemoryDataSource
	{
		public StringMemoryDataSource(string data)
			: base(Encoding.ASCII.GetBytes(data))
		{
		}
    }

    #endregion // Zip helpers

}
