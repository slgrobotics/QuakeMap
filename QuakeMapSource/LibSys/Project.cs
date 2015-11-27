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

		// debug tool: to avoid fetching every time, and reuse cached .html files instead,
		// set doFetchEqHtml = false
		public static bool doFetchEqHtml = true;

#if DEBUG
		public static bool enableTrace = true;
#endif

		public const string		PROGRAM_MAIN_SERVER = "www.earthquakemap.com";
		//public const string	PROGRAM_MAIN_SERVER = "localhost";
		//public static string	TERRA_SERVER = "www.terraserver-usa.com";
        public static string TERRA_SERVER = "msrmaps.com";

		public static string driveSystem = "C:\\";
		public static string driveProgramInstalled = "C:\\";

		//public static string fileInitialDirectory = "C:\\MyWaypoints";
		public static string fileInitialDirectory = "C:\\";
		public static string imageInitialDirectory = "C:\\";
		public static string photoSaveInitialDirectory = "C:\\";
		public static string pdaInitialDirectory = "C:\\";
		public static string exportDestFolder = "";
		public static int pdaExportImageFormat = 0;			// 0-bmp websafe; 1-bmp optimized; 2-jpeg
		public static bool pdaExportUseWebsafeGrayscalePalette = true;		// for speed over space set it to true
		public static bool pdaExportWrapPdb = true;			// whether to export preloaded tiles in PDB format 
		public static bool pdaWellSelectingMode = false;	// whether drag and click on the map selects a PDA well 
		public static double pdaWellDiagonale = 6000.0d;	// meters 
		public static double pdaWellDiagonaleFree = 600.0d;	// meters - for non-licenesed use 

		//public static DateTime	EXPIRE_AT = DateTime.MaxValue; // for infinite operation
		public static DateTime	EXPIRE_AT = DateTime.MaxValue; //new DateTime(2003, 5, 1, 0, 0, 0);
		public const string     PROGRAM_VERSION_RELEASEDATE = "20080803";
		public const string     MAPADVISOR_FORMAT_VERSION = "20040110";

		// **************************************************************************

		public const string     PROGRAM_NAME_LOGICAL = "tiler";			// used for making URLs on the servers
		public const string     PROGRAM_NAME_HUMAN = "QuakeMap";		// used for title in the frame etc.
		public const string     PROGRAM_VERSION_HUMAN = "3.7";			// used for greeting.
		public const string     WEBSITE_NAME_HUMAN = "QuakeMap.com";	// used for watermark printing etc.
		public const string     WEBSITE_LINK_WEBSTYLE = "http://www.quakemap.com";	// used for links etc.

		public const string		iniFileName = "quakemap.ini";			// make sure installation script makes this one

		public const string     CGIBINPTR_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/cgibinptr.html";

		// may be overridden like TILERABOUT="..." in cgibinptr.html (see TileCache.cs):
		public static string	ABOUT_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/about.html";
		public static string	ORDER_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/order.html";
		public static string	DLOAD_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/download.html";
		public static string	UPDATE_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/update.html";
		public static string	PRIVACY_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/privacy.html";
		public static string	PDA_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/pda.html";

		// supporting files (ico, gif, txt) are loaded from this URL's folder (TILERMISC in cgibinptr):
		public static string	MISC_FOLDER_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/misc";

		// sample files are loaded from this URL's folder (TILERSAMPLES in cgibinptr):
		public static string	SAMPLES_FOLDER_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/samples";

		// The following are filled with paths relative to current directory in Project():
		public static string	startupPath;
		private static string	iniFilePath;
		private static string   miscFolderPath;
		private static string   vehFolderPath;
		private static string   wptFolderPath;
		private static string	camtrackFolderPath;
		private static string   webPageTemplatesFolderBasePath;
		public static string    sampleFolderPath;

		// may be overridden by TILERHELP="..." in cgibinptr.html:
		public static string	HELP_FILE_URL = "http://" + PROGRAM_MAIN_SERVER + "/" + PROGRAM_NAME_LOGICAL + "/help.chm";
		public const string     HELP_FILE_PATH = "help.chm";
		public static DateTime	HELP_FILE_DATE = DateTime.MinValue;		// comes from cgibinptr.html
		public const int		LICENSE_FILE_MAX_AGE_DAYS = 10;

		public const int		CLASS				= 1;		// Serial Number Class
		public const int		MIN_SERIES			= 1;		// Serial Number Minimum Series
		public const int		MAX_SERIES			= 10;		// Serial Number Maximum Series

		public const int		TRIAL_DAYS			= 30;   	// for demo = limited time, full functionality, also defines opacity of the spoilers
		public const int		SPOILER_DAYS_START	= 20;  		// after that day "Evaluation" lettering appears on the screen, and on print

		// how many unused features and backdrops (seperate) to keep in cache.
		// Everything above is trimmed, oldest first:
		public static int       CACHE_MIN_UNUSED_COUNT = 30;

		public static string[]  mapsQualities = { "good", "best" };
		public const int        DEFAULT_MAPS_QUALITY_INDEX = 0;  // "good"
		public static string    DEFAULT_MAPS_QUALITY = mapsQualities[DEFAULT_MAPS_QUALITY_INDEX];

		public const string     MAP_CACHE_PATH	= "mapstiler";
		public const string     TERRASERVER_CACHE_PATH	= "terraserver";
		public const string     MAP_DESCR_EXTENSION	= ".map";
		public const string     JPG_DESCR_EXTENSION	= ".jpg";
		public const string     FDB_DESCR_EXTENSION	= ".fdb";
		public const string     MAPTREE_DESCR_EXTENSION	= ".txt";

		public const int        UNITS_DISTANCE_DEFAULT = 2; //Distance.UNITS_DISTANCE_MILES;
		public const int        FONT_SIZE_REGULAR = 8;
		public const int        FONT_SIZE_PRINT = 7;
		public const int        CITY_FONT_SIZE = FONT_SIZE_REGULAR;
		public const int        PLACE_FONT_SIZE = FONT_SIZE_REGULAR;

		// valid graph formats:
		public const int		FORMAT_UNKNOWN					= -1;
		public const int		FORMAT_SERVER_AVAILABILITY		= 0;
		public const int		FORMAT_EARTHQUAKES_INTENSITY	= 1;
		public const int		FORMAT_EARTHQUAKES_STRONGEST	= 2;
		public const int		FORMAT_TRACK_ELEVATION			= 3;

		public const double     CAMERA_HEIGHT_MIN_DEFAULT = 0.4d;			 // km - default value for do not zoom any closer
		public static double    cameraHeightMin = CAMERA_HEIGHT_MIN_DEFAULT; // km - do not zoom any closer
		public const double     CAMERA_HEIGHT_MAX = 25000.0d;		// km - this is far enough to see world map
		public const double     CAMERA_HEIGHT_SAFE = 400.0d;		// km - we can rely on map existing for this height
		public const double     CAMERA_HEIGHT_REAL_CLOSE = 0.5d;	// km - for a 1m/pix close look buttons

		public static double	MAX_MAGN_FACTOR	= 5000.0;	// max meters per magnitute point,
		// when drawing earthquake on the map.

		public const string SELECT_HELP = "\n\nTo select a row in the table, click on the grey area to the left of the row,\nso that whole row is highlighted\n\nTo select multiple rows, use Shift or Ctrl keys. Ctrl/A selects all.\n ";

		public const string		SEED_XML = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"yes\"?>";

		#region Pens and Brushes
		public static Pen debugPen = new Pen(Color.Red);
		public static Pen debug2Pen = new Pen(Color.Green);
		public static Pen debug3Pen = new Pen(Color.Yellow);

		public static Pen cameraPen = new Pen(Color.Red, 1);
		public static Pen cameraPenWide = new Pen(Color.Red, 3);
		public static Brush cameraBrush = new SolidBrush(Color.White);
		public static Pen dragPen = new Pen(Color.Magenta);
		public static Brush dragBrush = new SolidBrush(Color.Yellow);
		public static Pen distancePen = new Pen(Color.Red, 3.0f);
		public static Pen movePointPen = new Pen(Color.Gray, 1.0f);
		public static Pen distanceFramePen = new Pen(Color.Red, 2.0f);
		public static Pen distancePenPrint = new Pen(Color.Red, 2.0f);
		public static Pen distanceFramePenPrint = new Pen(Color.Red, 1.0f);
		public static Brush distanceBrush = new SolidBrush(Color.Black);
		public static Brush distanceShadowBrush = new SolidBrush(Color.White);
		public static Pen spoilPen = new Pen(Color.Red);
		public static Pen spoilPenPan = new Pen(Color.Yellow);
		public static Pen spoilPenZoom = new Pen(Color.Cyan);
		public static Pen markPen1 = new Pen(Color.Cyan);
		public static Pen markPen2 = new Pen(Color.Red);
		public static Brush spoilBrush = new SolidBrush(Color.Red);

		public static Pen gridPen = new Pen(Color.Red);
		public static Brush gridTextBrush = new SolidBrush(Color.Yellow);

		public static Pen cityPen = new Pen(Color.Yellow);
		public static Pen cityFontPen = new Pen(Color.Yellow);
		public static Pen cityBackgroundPen = new Pen(Color.Black);

		public static Brush cityBrush = new SolidBrush(Color.Yellow);			// circle
		public static Brush cityFontBrush = new SolidBrush(Color.Yellow);		// label
		public static Brush cityBackgroundBrush = new SolidBrush(Color.Black);	// bkg for both

		public static Pen earthquakePen = new Pen(Color.Magenta);
		public static Pen earthquakeFontPen = new Pen(Color.White);
		public static Pen earthquakeBackgroundPen = new Pen(Color.Black);

		public static Brush earthquakeBrush = new SolidBrush(Color.White);				// circle
		public static Brush earthquakeFontBrush = new SolidBrush(Color.White);			// label
		public static Brush earthquakeBackgroundBrush = new SolidBrush(Color.Black);	// bkg for both

		public static Pen waypointPen = new Pen(Color.White);
		public static Pen waypointPen1 = new Pen(Color.Red);
		public static Pen waypointPen1bold = new Pen(Color.Red, 2.0f);
		public static Pen waypointPen2 = new Pen(Color.Yellow);
		public static Pen waypointPen2bold = new Pen(Color.LightGreen, 5.0f);
		//public static Pen waypointPen3 = new Pen(Color.Green);

		public static Brush waypointFontBrush = new SolidBrush(Color.White);		// label
		public static Brush waypointBackgroundBrush = new SolidBrush(Color.Blue);	// bkg for both

		public static Pen trackPen = new Pen(Color.Red);

		public static Brush trackFontBrush = new SolidBrush(Color.Yellow);			// label
		public static Brush trackBackgroundBrush = new SolidBrush(Color.Black);		// bkg for both

		public static Pen landmarkPen = new Pen(Color.Yellow);

		public static Brush landmarkFontBrush = new SolidBrush(Color.Yellow);		// label
		public static Brush landmarkBackgroundBrush = new SolidBrush(Color.Brown);	// bkg for both

		public static Pen vehiclePen = new Pen(Color.Magenta);
		public static Pen vehicleFontPen = new Pen(Color.White);
		public static Pen vehicleBackgroundPen = new Pen(Color.Black);

		public static Brush vehicleBrush = new SolidBrush(Color.White);				// circle
		public static Brush vehicleFontBrush = new SolidBrush(Color.White);			// label
		public static Brush vehicleBackgroundBrush = new SolidBrush(Color.Black);	// bkg for both

		public static Pen customMapPen = new Pen(Color.Magenta);
		public static Brush customMapBrush = new SolidBrush(Color.White);				// circle
		public static Brush customMapFontBrush = new SolidBrush(Color.White);			// label

		public static Brush blueBrush = new SolidBrush(Color.Blue);
		public static Pen bluePen = new Pen(Color.Blue);
		public static Brush yellowBrush = new SolidBrush(Color.Yellow);
		public static Brush whiteBrush = new SolidBrush(Color.White);
		public static Brush redBrush = new SolidBrush(Color.Red);
		public static Pen redPen = new Pen(Color.Red);
		public static Brush blackBrush = new SolidBrush(Color.Black);
		public static Brush greenBrush = new SolidBrush(Color.Green);
		public static Pen blackPen = new Pen(Color.Black);
		public static Brush placeBrush = new SolidBrush(Color.Black);
		public static Pen placePen = new Pen(Color.Black);
		public static Brush placeShadowBrush = new SolidBrush(Color.Black);
		public static Pen placeShadowPen = new Pen(Color.Black);
		#endregion // Pens and Brushes

		// -----------------  global-use variables, flags, etc:  --------------------------------------------

		public static IMainCommand mainCommand = null;
		public static IImageBySymbol waypointImageGetter = null;
		public static IWizardStepControl pdaWizardStepControl = null; 
		public static Form mainForm;				// we need parent form for message boxes
		public static Tools tools = new Tools();

		public static bool useProxy	 = false;
		public static bool suspendKeepAlive	 = false;
		public static bool forceEmptyProxy	 = false;
		public static string proxyServer = "myproxyservernameoripaddress";
		public static int proxyPort = 8080;
		public static int webTimeoutMs = 8000;

		public static string geocachingLogin = "";

		public static string serverMessage = "";	// see TileCache.cs
		public static string upgradeMessage = "";	// see TileCache.cs
		public static bool spoiling = false;		// if true, CameraManager is moving camera, no redraw in Paint() allowed
		public static bool drawableReady = false;	// when false, disallows ReTile and other operations  
		// when starting, we get several resize events with different sizes, as 
		// layout settles. We want to ignore these and retile after the first paint.
		public static long trackId = 0;				// auto-incremented to assign track id's in format readers
		public static long eqId = 0;				// auto-incremented to assign earthquake id's on import
		public static long customMapId = 0;			// auto-incremented to assign custom map id's
		public static bool goingDown = false;
		public static bool inhibitRefresh = false;
		public static bool reloadRefresh = false;
		public static bool pictureDirty = false;
		public static int pictureDirtyCounter = 0;	// when >0, decremented in MainForm:periodicMaintenance(), causes Refresh for several cycles

		// debugging tool for GPS protocol:
		public static bool gpsLogProtocol = false;
		public static bool gpsLogErrors = false;
		public static bool gpsLogPackets = false;
		public static bool gpsSimulate = false;

		public static int  startRouteNumber = 1;	// required by ThreadGpsControl

		public static byte[] codeKey = new byte[] { 0x2A, 0xD6, 0x5E, 0xA2 };
		public const string dataTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";	// used like dateTime.ToString(Project.dataTimeFormat) for data packing

		public static bool makeRouteMode = false;	// use LayerWaypoints.This.makeRouteMode = ... to set it 
		public static bool movePointMode = false;	// use LayerWaypoints.This.movePointMode = ... to set it 
		public static bool moveLegMode = false;		// use LayerWaypoints.This.moveLegMode = ... to set it 

		// list of FileListStruct - persistent files to load on start. Prepared by MainForm:
		public static ArrayList FileList = new ArrayList();

		public static bool camTrackOn = false;

		// -----------------  options can be modified when program is running and will persist --------------

		public const string OPTIONS_FILE_NAME	= "options.xml";
		public const string FAVORITES_FILE_NAME	= "favorites.xml";
		public const string GPSPORTCONFIG_FILE_NAME = "portsettings";
		public const string TOOLS_FILE_NAME = "tools.xml";

		public static string serverMessageLast = "";	// last displayed server message to compare with for pop-up.

		public static bool useUtcTime = false;

		public static bool showHelpAtStart = true;
		public static bool drawRelief = true;
		public static bool drawGrid = false;
		public static bool drawCentralPoint = true;
		public static bool drawCornerArrows = true;
		public static bool drawRulers = true;
		public static bool drawCities = true;
		public static bool drawLandmarks = false;
		public static bool drawEarthquakes = false;
		public static bool drawWaypoints = false;
		public static bool drawTrackpoints = true;
		public static bool useWaypointIcons = true;
		public static bool showTrackpointNumbers = false;
		public static bool showWaypointNames = true;
		public static bool drawVehicles = false;
		public static bool drawCustomMaps = true;
		public static bool drawFreehand = true;
		public static int  waypointNameStyle = 0;		// see Waypoint:getLabel() and also see OptionsForm, WaypointsManager and MainForm-context menu
		public static string[] waypointNameStyleChoices = new string[] {
																		   "Auto",
																		   "Waypoint Name",
																		   "Url Name",
																		   "Description",
																		   "Comment",
																		   "Symbol",
																		   "Coordinates" };
		public static int  trackNameStyle = 0;
		public static string[] trackNameStyleChoices = new string[] {
																		   "Start-<number> Time",
																		   "Start-Track Name",
																		   "Track Name",
																		   "(none)"};
		public static bool drawTerraserver = true;
		public static string drawTerraserverMode = "aerial";
		public static double terraLayerOpacity   = 1.0d;	// 0.0 - transparent to 1.0 - opaque -- terra over basemap
		public static double terraLayerOpacity2  = 0.5d;	// 0.0 - transparent to 1.0 - opaque -- terra over topo
		public static double terraLayerOpacity3  = 1.0d;	// 0.0 - transparent to 1.0 - opaque -- custom maps (GeoTiff)
		public static bool makeWhitishTransparent = false;
		public static bool terraUseOverlay = false;

		public static string traceToken = "";
		public static bool allowMapPopups = true;		// do not persist this one
		public static int mouseWheelAction = 1;			// 0 - none, 1 - zoom (Google Earth style), 2 - zoom reverse, 3 - pan 

		public static bool allowUnreasonableZoom = true;

		public static bool cityUseShadow = true;
		public static bool placeUseShadow = true;
		public static bool earthquakeUseShadow = true;
		public static bool waypointUseShadow = true;
		public static bool vehicleUseShadow = true;
		public static bool printMinuteRulersUseShadow = true;
		public static int  printTextFont = 2;			// 0=6.0f, 1=7.0f, 2=8.0f, 3=9.0f, 4=10.0f
		public static double  distortionFactor = 1.0d;	// o.8 to 1.2 to correct for printer vertical distortion

		public static bool displayEqMagn = true;
		public static bool displayEqTime = true;
		public static bool displayEqDepth = true;
		public static int  earthquakeStyle = 5;
		public static bool earthquakeStyleFillRecent = true;		// recent earthquakes are filled, older not
		public static int  earthquakeStyleFillHowRecentIndex = 7;	// how recent are highlighted earthquakes - 2 days default
		public static int  unitsDistance = UNITS_DISTANCE_DEFAULT;
		public static int  coordStyle = 1;				// "N37°28.893'  W117°43.368'"
		public static bool eqUseOldData = true;			// use stored html files for parsing if new are not reached
		public static bool eqFetchOnStart = true;		// fetch earthquakes when starting the program
		public static bool sanityFilter = true;
		public static bool trackElevColor = false;
		public static bool trackSpeedColor = false;

		public static string mapsQuality = Project.DEFAULT_MAPS_QUALITY;

		public static int  gpsMakeIndex = 0;
		public static int  gpsModelIndex = 0;
		public static int  gpsInterfaceIndex = 0;
		public static bool gpsRtTrackOnMap = true;
		public static bool gpsZoomIntoTracks = true;
		public static int  gpsRtToleranceIndex = 2;
		public static bool gpsRtTrackLog = true;
		public static bool gpsRtKeepInView = true;
		public static bool gpsMagellanHandshake = false;
		public static int  gpsMagellanRouteNameConversionMethod = 1;
		public static bool gpsWptDescrAsName = true;		// if false, will load real names (in form of GC****) to GPS, if true - urlName
		public static string gpsVehicleIcon = "gps_car_default";
		public static int  gpsMaxPointsPerRoute = 20;		// for track to route conversion

		public static bool mainFormMaximized = true;
		public static int  mainFormWidth = 0;
		public static int  mainFormHeight = 0;
		public static int  mainFormX = 0;
		public static int  mainFormY = 0;
		public static int  wptManagerWidth = 0;
		public static int  wptManagerHeight = 0;
		public static int  cmManagerWidth = 0;
		public static int  cmManagerHeight = 0;
		public static int  symbolColWidth = 95;
		public static int  nameColWidth = 90;
		public static int  urlNameColWidth = 100;
		public static int  descColWidth = 100;
		public static int  commentColWidth = 120;
		public static int  nameColWidthCm = 100;
		public static int  descColWidthCm = 150;
		public static int  sourceColWidthCm = 150;
		public static int  eqManagerWidth = 0;
		public static int  eqManagerHeight = 0;
		public static int  photoManagerWidth = 0;
		public static int  photoManagerHeight = 0;
		public static int  photoManagerX = 0;
		public static int  photoManagerY = 0;
		public static int  photoFullSizeWidth = 0;
		public static int  photoFullSizeHeight = 0;
		public static int  photoFullSizeX = 0;
		public static int  photoFullSizeY = 0;
		public static int  pmLastTabMode = 0;
		public static int  photoSelectPhotoMode = 0;	// 0-none, 1-selecting a photo, 2-selecting coordinate
		public static int  photoSelectedWptId = -1;
		public static CamPos photoSelectedCoord = null;
		public static DelegateProcessingComplete delegateProcessingComplete = null;
		public static bool preloadDoProgress = false;
		public static int  preloadScale1 = 1;		// for 1, 2, 4, 8
		public static int  preloadScale2 = 0;		// for 16, 32, 64
		public static bool pdaExportDoAerial = true;
		public static bool pdaExportDoTopo = true;
		public static bool pdaExportDoColor = false;

		// Google Earth / KML document parameters:
		public static KmlOptions kmlOptions = new KmlOptions();

		public static string togeotiffFolderPath = "My Documents\\My Pictures";
		public static string togeotiffFileName = "";		// will default to togeotiffFolderPath

		public static string photoFolderPath = "My Documents\\My Pictures";
		public static string photoFileName = "";		// will default to photoFolderPath, or to last zip file with photos, read in on start
		public static string photoGalleryPath = "http://heysmile.smugmug.com/gallery/576522";
		public static int photoGalleryPictureSize = 2;	// 0-SmallURL, 1-MediumURL, 2-LargeURL, 3-OriginalURL 
		public static string[] photoGalleryPictureSizes = new string[] { "SmallURL", "MediumURL", "LargeURL", "OriginalURL" };
		public static bool photoAnalyzeOnLoad = true;
		public static bool photoAnalyzeOnLoadReminder = true;
		public static bool photoDoPreview = true;
		public static bool thumbDoDisplay = true;
		public static bool photoPreserveSize = true;
		public static bool photoFitToSize = true;
		public static long photoSaveQuality = 75L;		// JPEG quality as we save images in PhotoDraw

		public static string webPageTemplate = "standard";
		public static string webPageDestinationFolder = "";
		public static string webPagePhotoStorageUrlBase = Project.WEBSITE_LINK_WEBSTYLE + "/demo/testpictures/";
		public static bool webPageLinksUsePopup = false;

		public static double routeSpeed = 10.0d;		// meters per hour, used for making new routes
		public static TimeSpan  photoTimeShiftYourCamera;
		public static int  photoTimeZoneIdYourCamera = 1072;		// id from MyTimeZone
		public static TimeSpan  photoTimeShiftFile;
		public static int  photoTimeZoneIdFile = 1072;
		public static TimeSpan  photoTimeShiftTemp;
		public static int  photoTimeZoneIdTemp = 1072;

		public static bool photoPositionsAsk = true;
		public static bool photoParametersAsk = true;
		public static bool photoPositionsDonotwrite = false;
		public static bool photoParametersDonotwrite = false;

		public static bool photoModeReprocess = false;		// ignore PhotoParameters file, use photoTimeShiftCurrent

		// this is what photo processing uses, and if it finds photoParameters file it will set it to 2:
		public static int photoTimeShiftTypeCurrent = 0;	// from camera by default; not persistent in options.xml

		public static TimeSpan photoTimeShift(int index)
		{
			TimeSpan ts = Project.photoTimeShiftYourCamera;

			switch(index)
			{
				case 1:
					ts = Project.photoTimeShiftTemp;
					break;
				case 2:
					ts = Project.photoTimeShiftFile;
					break;
			}
			return ts;
		}

		public static void adjustPhotoTimeShift(int index, TimeSpan toAdjust)
		{
			switch(index)
			{
				case 0:
					Project.photoTimeShiftYourCamera += toAdjust;
					break;
				case 1:
					Project.photoTimeShiftTemp += toAdjust;
					break;
				case 2:
					Project.photoTimeShiftFile += toAdjust;
					break;
			}
		}

		public static TimeSpan photoTimeShiftCurrent
		{
			get 
			{
				TimeSpan ts = Project.photoTimeShiftYourCamera;

				switch(Project.photoTimeShiftTypeCurrent)
				{
					case 1:
						ts = Project.photoTimeShiftTemp;
						break;
					case 2:
						ts = Project.photoTimeShiftFile;
						break;
				}
				return ts;
			}

			set
			{
				switch(Project.photoTimeShiftTypeCurrent)
				{
					case 0:
						Project.photoTimeShiftYourCamera = value;
						break;
					case 1:
						Project.photoTimeShiftTemp = value;
						break;
					case 2:
						Project.photoTimeShiftFile = value;
						break;
				}
			}
		}

		public static int photoTimeZoneId(int index)
		{
			int id = Project.photoTimeZoneIdYourCamera;

			switch(index)
			{
				case 1:
					id = Project.photoTimeZoneIdTemp;
					break;
				case 2:
					id = Project.photoTimeZoneIdFile;
					break;
			}
			return id;
		}

		public static int photoTimeZoneIdCurrent
		{
			get 
			{
				int id = Project.photoTimeZoneIdYourCamera;

				switch(Project.photoTimeShiftTypeCurrent)
				{
					case 1:
						id = Project.photoTimeZoneIdTemp;
						break;
					case 2:
						id = Project.photoTimeZoneIdFile;
						break;
				}
				return id;
			}

			set
			{
				switch(Project.photoTimeShiftTypeCurrent)
				{
					case 0:
						Project.photoTimeZoneIdYourCamera = value;
						break;
					case 1:
						Project.photoTimeZoneIdTemp = value;
						break;
					case 2:
						Project.photoTimeZoneIdFile = value;
						break;
				}
			}
		}

		public const string PHOTO_PARAM_FILE_NAME = "photoParameters.xml";
		public const string PHOTO_POSITIONS_FILE_NAME = "photoPositions.xml";

		public static int thumbWidth = 80;
		public static int thumbHeight = 60;
		public static int thumbPosition = 0;

		// a list of FormattedFileDescr - persistent files/urls to load on start. Also used by ImportManager:
		public static ArrayList FileDescrList = new ArrayList();

		public static void FileDescrListAdd(FormattedFileDescr ffd)
		{
			// do not add duplicate files here:
			foreach(FormattedFileDescr tmp in FileDescrList)
			{
				if(tmp.filename.Equals(ffd.filename))
				{
					FileDescrList.Remove(tmp);
					break;
				}
			}
			FileDescrList.Add(ffd);
		}

		// good starting position high over the US, in case everything else fails:
		public static double cameraLat  =  36.817d;
		public static double cameraLng  = -97.471d;
		public static double cameraElev =  10118927.0d;

		public static double breakTimeMinutes = 10.0d;		// to break tracks into trips

		public static string findKeyword = "london";
		public static string zipcode = "";
		public static bool serverAvailable = false;				// couldn't reach cgibinptr.
		public static bool terraserverAvailable = false;		// aerial or topo is showing
		public static bool terraserverUseServices = false;		// get ABB via Terraserver Web Service.
		public static bool terraserverDisconnected = false;		// aerial or topo in disconnected mode

		public static CommBaseSettings gpsPortSettings = new CommBaseSettings();

		public static Stack cameraPositionsBackStack = new Stack();		// stack of CamPos
		public static Stack cameraPositionsFwdStack  = new Stack();		// stack of CamPos

		public static ArrayList favorites = new ArrayList();			// list of CamPos
		public static int favoritesFirstIndex;							// menu item index, filled in MainForm

		public static ArrayList recentFiles = new ArrayList();			// list of string (file or folder names)
		public static int recentFilesFirstIndex;						// menu item index, filled in MainForm
		public static int toolsFirstIndex;								// menu item index, filled in MainForm

		public static string GpsBabelWrapperExecutable = "";

		//public static LibSys.ThreadPool threadPool = null;

		#region Constructor and Executables

		public Project()
		{
			FileInfo myExe = new FileInfo(Project.GetLongPathName(Application.ExecutablePath));
			startupPath = myExe.Directory.Parent.FullName;

			try
			{
				driveProgramInstalled = startupPath.Substring(0,3);

				fileInitialDirectory = driveProgramInstalled;
				imageInitialDirectory = driveProgramInstalled;
				photoSaveInitialDirectory = driveProgramInstalled;
				pdaInitialDirectory = driveProgramInstalled;

				string[] str = Directory.GetLogicalDrives();
				for(int i=0; i < str.Length ;i++)
				{
					DirectoryInfo di = new DirectoryInfo(str[i]);
					if((int)di.Attributes > 0 && (di.Attributes & FileAttributes.System) != 0)
					{
						driveSystem = str[i];
						break;
					}
				}
			}
			catch {}

			iniFilePath = Path.Combine(startupPath, iniFileName);
            if (!File.Exists(iniFilePath))
            {
                // this is probably click-on-file startup, hope registry key made by installer is ok:
                string keyPath = "Software\\QuakeMap\\QuakeMap";
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyPath);
                if (regKey != null)
                {
                    startupPath = "" + regKey.GetValue("INSTALLDIR");
                    iniFilePath = Path.Combine(startupPath, iniFileName);
                    regKey.Close();
                }
            }

            // "c:\Documents and Settings\<user>\Application Data\"  or "C:\Users\Sergei\AppData\Roaming\QuakeMap"
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            miscFolderPath = Path.Combine(appFolderPath, PROGRAM_NAME_HUMAN + @"\Misc");
            vehFolderPath = Path.Combine(appFolderPath, PROGRAM_NAME_HUMAN + @"\Vehicles");
            wptFolderPath = Path.Combine(appFolderPath, PROGRAM_NAME_HUMAN + @"\Waypoints");
            camtrackFolderPath = Path.Combine(appFolderPath, PROGRAM_NAME_HUMAN + @"\Camtrack");
            webPageTemplatesFolderBasePath = Path.Combine(appFolderPath, PROGRAM_NAME_HUMAN + @"\WebTemplates");
            sampleFolderPath = Path.Combine(appFolderPath, PROGRAM_NAME_HUMAN + @"\Samples");

			/*
			threadPool = new LibSys.ThreadPool(5, 10, "ThreadPool");	// initial threads, max threads, name
			threadPool.Priority = System.Threading.ThreadPriority.BelowNormal;
			threadPool.NewThreadTrigger = 1000;		// if requests are waiting in queue more than 1000ms, create new worker thread
			threadPool.DynamicThreadDecay = 10000;

			//threadPool.Priority = System.Threading.ThreadPriority.AboveNormal;
			//threadPool.NewThreadTrigger = 250;
			//threadPool.DynamicThreadDecay = 5000;
			threadPool.Start();
			*/
		}

		~Project()
		{
			cleanupFilesToDelete();
		}

		public static void resetGpsBabelWrapperExecutable()
		{
			string gbwPath = null;

			// hope registry key made by GpsBabelWrapper installer is ok:
			string keyPath = "Software\\VitalBytes\\GpsBabelWrapper";
			RegistryKey regKey = Registry.CurrentUser.OpenSubKey (keyPath);
			if (regKey != null) 
			{
				gbwPath = "" + regKey.GetValue("INSTALLDIR");
				regKey.Close();
			}

			if(gbwPath != null)
			{
				FileInfo gbwExe = new FileInfo(Path.Combine(gbwPath, "bin\\gpsbabelwrapper.exe"));
				if(File.Exists(gbwExe.FullName))
				{
					GpsBabelWrapperExecutable = gbwExe.FullName;
				}
				else
				{
					string err = "GpsBabelWrapper Executable " + gbwExe + " not found";
					LibSys.StatusBar.Error(err);
					//Project.ErrorBox(null, err);
				}
			}
			else
			{
				string err = "GpsBabelWrapper Executable not found - registry key missing. Please install GpsBabelWrapper.";
				LibSys.StatusBar.Error(err);
			}
		}
		#endregion // Constructor and Executables

		/// <summary>
		/// separates string in form "GCXYZXV = Mike's memorial Cache" into name and description.
		/// </summary>
		/// <param name="inDesc"></param>
		/// <param name="name"></param>
		/// <param name="desc"></param>
		/// <returns></returns>
		public static bool splitGcDescr(string inDesc, out string name, out string desc)
		{
			desc = "";
			name = "";
			bool ret = false;

			int pos = 0;
			try
			{
				if(inDesc.StartsWith("GC") && (pos=inDesc.IndexOf(" = ")) > 0 && pos < 15 )
				{
					name = inDesc.Substring(0, pos);
					desc = inDesc.Substring(pos + 3);
					ret = true;
				}
			}
			catch {}

			return ret;
		}

		#region Read/Write Options file

		public static void ReadOptions()
		{
			bool favoritesDirty = false;

			string optionsFilePath = GetMiscPath(OPTIONS_FILE_NAME);
#if DEBUG
			LibSys.StatusBar.Trace("IP: Project:ReadOptions() path=" + optionsFilePath);
#endif
			DateTime startedRead = DateTime.Now;
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
						foreach(XmlNode node in nnode.ChildNodes)
						{
							string nodeName = node.Name;
							try 
							{
								if(nodeName.Equals("cameraLat")) 
								{
									cameraLat = Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("cameraLng")) 
								{
									cameraLng = Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("cameraElev")) 
								{
									cameraElev = Convert.ToDouble(node.InnerText);
								}
#if DEBUG
								else if(nodeName.Equals("enableTrace")) 
								{
									enableTrace = node.InnerText.ToLower().Equals("true");
								}
#endif
								else if(nodeName.Equals("mappingCacheLocation")) 
								{
									mappingCacheLocation = node.InnerText;
								}
								else if(nodeName.Equals("gpsMakeIndex")) 
								{
									gpsMakeIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("gpsModelIndex")) 
								{
									gpsModelIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("gpsInterfaceIndex")) 
								{
									gpsInterfaceIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("gpsRtToleranceIndex")) 
								{
									gpsRtToleranceIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("gpsMaxPointsPerRoute")) 
								{
									gpsMaxPointsPerRoute = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("gpsMagellanRouteNameConversionMethod")) 
								{
									gpsMagellanRouteNameConversionMethod = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("mainFormWidth")) 
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
								else if(nodeName.Equals("wptManagerWidth")) 
								{
									wptManagerWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("wptManagerHeight")) 
								{
									wptManagerHeight = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("cmManagerWidth")) 
								{
									cmManagerWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("cmManagerHeight")) 
								{
									cmManagerHeight = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("symbolColWidth")) 
								{
									symbolColWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("nameColWidth")) 
								{
									nameColWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("nameColWidthCm")) 
								{
									nameColWidthCm = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("urlNameColWidth")) 
								{
									urlNameColWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("descColWidth")) 
								{
									descColWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("descColWidthCm")) 
								{
									descColWidthCm = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("sourceColWidthCm")) 
								{
									sourceColWidthCm = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("commentColWidth")) 
								{
									commentColWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("eqManagerWidth")) 
								{
									eqManagerWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("eqManagerHeight")) 
								{
									eqManagerHeight = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoFullSizeWidth")) 
								{
									photoFullSizeWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoFullSizeHeight")) 
								{
									photoFullSizeHeight = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoFullSizeX")) 
								{
									photoFullSizeX = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoFullSizeY")) 
								{
									photoFullSizeY = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoManagerDlgWidth")) 
								{
									photoManagerWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoManagerDlgHeight")) 
								{
									photoManagerHeight = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoManagerDlgX")) 
								{
									photoManagerX = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoManagerDlgY")) 
								{
									photoManagerY = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("pmLastTabMode")) 
								{
									pmLastTabMode = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("preloadDoProgress")) 
								{
									preloadDoProgress = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("preloadScale1")) 
								{
									preloadScale1 = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("preloadScale2")) 
								{
									preloadScale2 = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("pdaExportDoAerial")) 
								{
									pdaExportDoAerial = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("pdaExportDoTopo")) 
								{
									pdaExportDoTopo = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("pdaExportDoColor")) 
								{
									pdaExportDoColor = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("kmlShowInPopup")) 
								{
									kmlOptions.kmlShowInPopup = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("kmlWptsClampToGround")) 
								{
									kmlOptions.kmlWptsClampToGround = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("kmlTrksClampToGround")) 
								{
									kmlOptions.kmlTrksClampToGround = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("fileInitialDirectory")) 
								{
									fileInitialDirectory = node.InnerText;
								}
								else if(nodeName.Equals("imageInitialDirectory")) 
								{
									imageInitialDirectory = node.InnerText;
								}
								else if(nodeName.Equals("photoSaveInitialDirectory")) 
								{
									photoSaveInitialDirectory = node.InnerText;
								}
								else if(nodeName.Equals("pdaInitialDirectory")) 
								{
									pdaInitialDirectory = node.InnerText;
								}
								else if(nodeName.Equals("exportDestFolder")) 
								{
									exportDestFolder = node.InnerText;
								}
								else if(nodeName.Equals("pdaWellDiagonale")) 
								{
									pdaWellDiagonale = Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("pdaExportImageFormat")) 
								{
									pdaExportImageFormat = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("pdaExportUseWebsafeGrayscalePalette")) 
								{
									pdaExportUseWebsafeGrayscalePalette = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("pdaExportWrapPdb")) 
								{
									pdaExportWrapPdb = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("findKeyword")) 
								{
									findKeyword = node.InnerText;
								}
                                else if (nodeName.Equals("zipcode"))
                                {
                                    zipcode = node.InnerText;
                                }
                                else if (nodeName.Equals("waypointNameStyle")) 
								{
									waypointNameStyle = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("trackNameStyle")) 
								{
									trackNameStyle = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("earthquakeStyle")) 
								{
									earthquakeStyle = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("printTextFont")) 
								{
									printTextFont = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("mouseWheelAction")) 
								{
									mouseWheelAction = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("distortionFactor")) 
								{
									distortionFactor = Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("cameraHeightMin")) 
								{
									cameraHeightMin = Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("coordStyle")) 
								{
									coordStyle = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("unitsDistance")) 
								{
									unitsDistance = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("earthquakeStyleFillHowRecentIndex")) 
								{
									earthquakeStyleFillHowRecentIndex = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("allowMapPopups")) 
								{
									allowMapPopups = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawRelief")) 
								{
									drawRelief = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("serverMessageLast")) 
								{
									serverMessageLast = node.InnerText;
								}
								else if(nodeName.Equals("drawGrid")) 
								{
									drawGrid = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawCentralPoint")) 
								{
									drawCentralPoint = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawCornerArrows")) 
								{
									drawCornerArrows = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawCities")) 
								{
									drawCities = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawLandmarks")) 
								{
									drawLandmarks = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawEarthquakes")) 
								{
									drawEarthquakes = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawWaypoints")) 
								{
									drawWaypoints = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawTrackpoints")) 
								{
									drawTrackpoints = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("useWaypointIcons")) 
								{
									useWaypointIcons = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("showHelpAtStart")) 
								{
									showHelpAtStart = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("gpsWptDescrAsName")) 
								{
									gpsWptDescrAsName = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("gpsVehicleIcon")) 
								{
									gpsVehicleIcon = node.InnerText;
								}
								else if(nodeName.Equals("showTrackpointNumbers")) 
								{
									showTrackpointNumbers = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawVehicles")) 
								{
									drawVehicles = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("eqUseOldData")) 
								{
									eqUseOldData = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("eqFetchOnStart")) 
								{
									eqFetchOnStart = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("earthquakeStyleFillRecent")) 
								{
									earthquakeStyleFillRecent = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("terraserverUseServices")) 
								{
									terraserverUseServices = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("sanityFilter")) 
								{
									sanityFilter = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("gpsRtTrackOnMap")) 
								{
									gpsRtTrackOnMap = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("gpsZoomIntoTracks")) 
								{
									gpsZoomIntoTracks = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("gpsRtTrackLog")) 
								{
									gpsRtTrackLog = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("gpsRtKeepInView")) 
								{
									gpsRtKeepInView = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("gpsMagellanHandshake")) 
								{
									gpsMagellanHandshake = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("terraLayerOpacity")) 
								{
									terraLayerOpacity = Convert.ToDouble(node.InnerText);
									// protect from "disappearing aerial", when transparency goes to 0:
									if(terraLayerOpacity < 0.5d || terraLayerOpacity > 1.0d)
									{
										terraLayerOpacity = 0.5d;
									}
								}
								else if(nodeName.Equals("terraLayerOpacity2")) 
								{
									terraLayerOpacity2 = Convert.ToDouble(node.InnerText);
									// protect from "disappearing aerial", when transparency goes to 0:
									if(terraLayerOpacity2 < 0.15d || terraLayerOpacity2 > 1.0d)
									{
										terraLayerOpacity2 = 0.5d;
									}
								}
								else if(nodeName.Equals("terraLayerOpacity3")) 
								{
									terraLayerOpacity3 = Convert.ToDouble(node.InnerText);
									// protect from "disappearing aerial", when transparency goes to 0:
									if(terraLayerOpacity3 < 0.25d || terraLayerOpacity3 > 1.0d)
									{
										terraLayerOpacity3 = 1.0d;
									}
								}
								else if(nodeName.Equals("terraUseOverlay")) 
								{
									//terraUseOverlay = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("breakTimeMinutes")) 
								{
									breakTimeMinutes = Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("drawTerraserver")) 
								{
									drawTerraserver = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("makeWhitishTransparent")) 
								{
									makeWhitishTransparent = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("drawTerraserverMode")) 
								{
									drawTerraserverMode = node.InnerText;
								}
								else if(nodeName.Equals("photoGalleryPictureSize")) 
								{
									photoGalleryPictureSize = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoFolderPath")) 
								{
									photoFolderPath = node.InnerText;
								}
								else if(nodeName.Equals("photoTimeShiftYourCamera")) 
								{
									long ticks = Convert.ToInt64(node.InnerText);
									photoTimeShiftYourCamera = new TimeSpan(ticks);
								}
								else if(nodeName.Equals("photoTimeZoneIdYourCamera")) 
								{
									photoTimeZoneIdYourCamera = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoTimeZoneIdFile")) 
								{
									photoTimeZoneIdFile = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoTimeZoneIdTemp")) 
								{
									photoTimeZoneIdTemp = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("togeotiffFolderPath"))
								{
									togeotiffFolderPath = node.InnerText;
								}
								else if(nodeName.Equals("togeotiffFileName"))
								{
									togeotiffFileName = node.InnerText;
								}
								else if(nodeName.Equals("photoFileName"))
								{
									photoFileName = node.InnerText;
								}
								else if(nodeName.Equals("photoAnalyzeOnLoad"))
								{
									photoAnalyzeOnLoad = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("photoAnalyzeOnLoadReminder"))
								{
									photoAnalyzeOnLoadReminder = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("photoDoPreview")) 
								{
									photoDoPreview = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("thumbDoDisplay")) 
								{
									thumbDoDisplay = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("thumbWidth")) 
								{
									thumbWidth = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("thumbHeight")) 
								{
									thumbHeight = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("thumbPosition")) 
								{
									thumbPosition = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("photoPreserveSize")) 
								{
									photoPreserveSize = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("photoFitToSize")) 
								{
									photoFitToSize = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("photoSaveQuality")) 
								{
									photoSaveQuality = Convert.ToInt64(node.InnerText.Trim());
								}
								else if(nodeName.Equals("useProxy")) 
								{
									useProxy = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("suspendKeepAlive")) 
								{
									suspendKeepAlive = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("forceEmptyProxy")) 
								{
									forceEmptyProxy = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("proxyServer")) 
								{
									proxyServer = node.InnerText;
								}
								else if(nodeName.Equals("proxyPort")) 
								{
									proxyPort = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("webTimeoutMs")) 
								{
									webTimeoutMs = Convert.ToInt32(node.InnerText);
								}
								else if(nodeName.Equals("geocachingLogin")) 
								{
									geocachingLogin = node.InnerText;
								}
								else if(nodeName.Equals("penTrackThickness")) 
								{
									TrackPalette.penTrackThickness = (float)Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("penRouteThickness")) 
								{
									TrackPalette.penRouteThickness = (float)Convert.ToDouble(node.InnerText);
								}
								else if(nodeName.Equals("webPageTemplate")) 
								{
									webPageTemplate = node.InnerText;
								}
								else if(nodeName.Equals("webPageDestinationFolder")) 
								{
									webPageDestinationFolder = node.InnerText;
								}
								else if(nodeName.Equals("webPagePhotoStorageUrlBase")) 
								{
									webPagePhotoStorageUrlBase = node.InnerText;
								}
								else if(nodeName.Equals("webPageLinksUsePopup")) 
								{
									webPageLinksUsePopup = node.InnerText.ToLower().Equals("true");
								}
								else if(nodeName.Equals("trackPalette")) 
								{
									int i = 0;
									foreach(XmlNode nnnode in node.ChildNodes) 
									{
										try
										{
											string sRGB = nnnode.InnerText;
											string[] split = sRGB.Split(new Char[] {','});
											int r = Convert.ToInt32(split[0]);
											int g = Convert.ToInt32(split[1]);
											int b = Convert.ToInt32(split[2]);
											Color color = Color.FromArgb(r, g, b);

											TrackPalette.setColor(i, color);
										} 
										catch {}
										i++;
									}
								}
								else if(nodeName.Equals("fileDescrList")) 
								{
									foreach(XmlNode nnnode in node.ChildNodes) 
									{
										string filename = nnnode.Attributes.GetNamedItem("path").InnerText.Trim();
										string formatName = nnnode.Attributes.GetNamedItem("format").InnerText;
										FormattedFileDescr ffd = new FormattedFileDescr(filename, formatName, true);
										Project.FileDescrList.Add(ffd);
									}
								}
								else if(nodeName.Equals("favorites")) 
								{
									// this is here only for backwards compatibility. It takes favorites from options.xml
									// there is no save block back to options.xml, favorites go into favorites.xml now

									foreach(XmlNode nnnode in node.ChildNodes) 
									{
										string name = nnnode.InnerText;
										string sLng = nnnode.Attributes.GetNamedItem("lon").InnerText;
										string sLat = nnnode.Attributes.GetNamedItem("lat").InnerText;
										string sElev = nnnode.Attributes.GetNamedItem("elev").InnerText;
										XmlNode tNode = nnnode.Attributes.GetNamedItem("type");
										string type = tNode == null ? "aerial" : tNode.InnerText.Trim();
										double lng = Convert.ToDouble(sLng);
										double lat = Convert.ToDouble(sLat);
										double elev = Convert.ToDouble(sElev);
										CamPos camPos = new CamPos(lng, lat, elev, name, type);
										Project.favorites.Add(camPos);
										favoritesDirty = true;	// provoke SaveFavorites into favorites.xml
									}
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

			LibSys.StatusBar.Trace("ReadOptions: " + Math.Round((DateTime.Now - startedRead).TotalMilliseconds) + " ms");

			// restore gpsPortSettings from the file or fill it with default values:
			restoreOrDefaultGpsSettings();

			if(cameraHeightMin < 0.04d || cameraHeightMin > CAMERA_HEIGHT_MIN_DEFAULT)
			{
				cameraHeightMin = CAMERA_HEIGHT_MIN_DEFAULT;
			}

			if(cameraElev < cameraHeightMin * 1000.0d || cameraElev > CAMERA_HEIGHT_MAX * 1000.0d)
			{
				cameraElev = CAMERA_HEIGHT_SAFE * 1000.0d;
			}

			// restore Tools from the file or fill it with default values:
			string toolsFilePath = GetMiscPath(TOOLS_FILE_NAME);
			Project.tools.Restore(toolsFilePath);

			if(favoritesDirty)
			{
				LibSys.StatusBar.Trace("ReadOptions: saving favorites");
				SaveFavorites();	// in case the old format was read in
				SaveOptions();		// without the favorites part
			}
			else
			{
				ReadFavorites();
			}
			LibSys.StatusBar.Trace("ReadOptions: finished in " + Math.Round((DateTime.Now - startedRead).TotalMilliseconds) + " ms");
		}

		// this really belongs to LibGps.AllGpsDevices:
		public static string[] AllGpsMakes = new string[] {
															  "garmin",
															  "magellan",
															  "nmea"
														  };

		public static string currentGpsMake { get { return AllGpsMakes[gpsMakeIndex]; } }

		// returns true if could restore from file
		public static bool restoreOrDefaultGpsSettings()
		{
			bool ret = false;

			// restore gpsPortSettings from the file or fill it with default values:
			string gpsPortFilePath = GetMiscPath(GPSPORTCONFIG_FILE_NAME + "-" + Project.currentGpsMake + ".xml");
			if(!File.Exists(gpsPortFilePath))
			{
				// older versions used single file for all GPSes
				gpsPortFilePath = GetMiscPath(GPSPORTCONFIG_FILE_NAME + ".xml");
			}
			Stream fs = null;
			try
			{	
				fs = new FileStream(gpsPortFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				gpsPortSettings = CommBaseSettings.LoadFromXML(fs);
				if (gpsPortSettings == null)
				{
					throw new Exception();
				}
				ret = true;
			}
			catch
			{
				gpsPortSettings = new CommBaseSettings();
				gpsPortSettings.port = "COM1:";
				gpsPortSettings.baudRate = 9600;
				gpsPortSettings.parity = Parity.none;
				gpsPortSettings.autoReopen = true;

			}
			finally
			{
				if(fs != null) { fs.Close(); }
			}

			return ret;
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
				SetValue(xmlDoc, root, "fileInitialDirectory", fileInitialDirectory);
				SetValue(xmlDoc, root, "imageInitialDirectory", imageInitialDirectory);
				SetValue(xmlDoc, root, "photoSaveInitialDirectory", photoSaveInitialDirectory);
				SetValue(xmlDoc, root, "pdaInitialDirectory", pdaInitialDirectory);
				SetValue(xmlDoc, root, "exportDestFolder", exportDestFolder);
				SetValue(xmlDoc, root, "pdaWellDiagonale", "" + pdaWellDiagonale);
				SetValue(xmlDoc, root, "pdaExportImageFormat", "" + pdaExportImageFormat);
				SetValue(xmlDoc, root, "pdaExportUseWebsafeGrayscalePalette", "" + pdaExportUseWebsafeGrayscalePalette);
				SetValue(xmlDoc, root, "pdaExportWrapPdb", "" + pdaExportWrapPdb);
#if DEBUG
				SetValue(xmlDoc, root, "enableTrace", "" + enableTrace);
#endif
				SetValue(xmlDoc, root, "cameraLat", "" + cameraLat);
				SetValue(xmlDoc, root, "cameraLng", "" + cameraLng);
				SetValue(xmlDoc, root, "cameraElev", "" + cameraElev);
				if(mappingCacheLocation != null)
				{
					SetValue(xmlDoc, root, "mappingCacheLocation", mappingCacheLocation.Trim());
				}
				SetValue(xmlDoc, root, "gpsMakeIndex", "" + gpsMakeIndex);
				SetValue(xmlDoc, root, "gpsModelIndex", "" + gpsModelIndex);
				SetValue(xmlDoc, root, "gpsInterfaceIndex", "" + gpsInterfaceIndex);
				SetValue(xmlDoc, root, "gpsRtToleranceIndex", "" + gpsRtToleranceIndex);
				SetValue(xmlDoc, root, "gpsMaxPointsPerRoute", "" + gpsMaxPointsPerRoute);
				SetValue(xmlDoc, root, "gpsMagellanRouteNameConversionMethod", "" + gpsMagellanRouteNameConversionMethod);
				SetValue(xmlDoc, root, "mainFormMaximized", "" + mainFormMaximized);
				SetValue(xmlDoc, root, "mainFormWidth", "" + mainFormWidth);
				SetValue(xmlDoc, root, "mainFormHeight", "" + mainFormHeight);
				SetValue(xmlDoc, root, "mainFormX", "" + mainFormX);
				SetValue(xmlDoc, root, "mainFormY", "" + mainFormY);
				SetValue(xmlDoc, root, "wptManagerWidth", "" + wptManagerWidth);
				SetValue(xmlDoc, root, "wptManagerHeight", "" + wptManagerHeight);
				SetValue(xmlDoc, root, "cmManagerWidth", "" + cmManagerWidth);
				SetValue(xmlDoc, root, "cmManagerHeight", "" + cmManagerHeight);
				SetValue(xmlDoc, root, "symbolColWidth", "" + symbolColWidth);
				SetValue(xmlDoc, root, "nameColWidth", "" + nameColWidth);
				SetValue(xmlDoc, root, "nameColWidthCm", "" + nameColWidthCm);
				SetValue(xmlDoc, root, "urlNameColWidth", "" + urlNameColWidth);
				SetValue(xmlDoc, root, "descColWidth", "" + descColWidth);
				SetValue(xmlDoc, root, "descColWidthCm", "" + descColWidthCm);
				SetValue(xmlDoc, root, "sourceColWidthCm", "" + sourceColWidthCm);
				SetValue(xmlDoc, root, "commentColWidth", "" + commentColWidth);
				SetValue(xmlDoc, root, "eqManagerWidth", "" + eqManagerWidth);
				SetValue(xmlDoc, root, "eqManagerHeight", "" + eqManagerHeight);
				SetValue(xmlDoc, root, "photoFullSizeWidth", "" + photoFullSizeWidth);
				SetValue(xmlDoc, root, "photoFullSizeHeight", "" + photoFullSizeHeight);
				SetValue(xmlDoc, root, "photoFullSizeX", "" + photoFullSizeX);
				SetValue(xmlDoc, root, "photoFullSizeY", "" + photoFullSizeY);
				SetValue(xmlDoc, root, "photoManagerDlgWidth", "" + photoManagerWidth);
				SetValue(xmlDoc, root, "photoManagerDlgHeight", "" + photoManagerHeight);
				SetValue(xmlDoc, root, "photoManagerDlgX", "" + photoManagerX);
				SetValue(xmlDoc, root, "photoManagerDlgY", "" + photoManagerY);
				SetValue(xmlDoc, root, "pmLastTabMode", "" + pmLastTabMode);
				SetValue(xmlDoc, root, "preloadDoProgress", "" + preloadDoProgress);
				SetValue(xmlDoc, root, "pdaExportDoAerial", "" + pdaExportDoAerial);
				SetValue(xmlDoc, root, "pdaExportDoTopo", "" + pdaExportDoTopo);
				SetValue(xmlDoc, root, "pdaExportDoColor", "" + pdaExportDoColor);
				SetValue(xmlDoc, root, "kmlShowInPopup", "" + kmlOptions.kmlShowInPopup);
				SetValue(xmlDoc, root, "kmlWptsClampToGround", "" + kmlOptions.kmlWptsClampToGround);
				SetValue(xmlDoc, root, "kmlTrksClampToGround", "" + kmlOptions.kmlTrksClampToGround);
				SetValue(xmlDoc, root, "preloadScale1", "" + preloadScale1);
				SetValue(xmlDoc, root, "preloadScale2", "" + preloadScale2);
				SetValue(xmlDoc, root, "findKeyword", findKeyword);
                SetValue(xmlDoc, root, "zipcode", zipcode);
				SetValue(xmlDoc, root, "allowMapPopups", "" + allowMapPopups);
				SetValue(xmlDoc, root, "drawRelief", "" + drawRelief);
				SetValue(xmlDoc, root, "serverMessageLast", serverMessageLast);
				SetValue(xmlDoc, root, "drawGrid", "" + drawGrid);
				SetValue(xmlDoc, root, "drawCentralPoint", "" + drawCentralPoint);
				SetValue(xmlDoc, root, "drawCornerArrows", "" + drawCornerArrows);
				SetValue(xmlDoc, root, "drawCities", "" + drawCities);
				SetValue(xmlDoc, root, "drawLandmarks", "" + drawLandmarks);
				SetValue(xmlDoc, root, "drawEarthquakes", "" + drawEarthquakes);
				SetValue(xmlDoc, root, "drawWaypoints", "" + drawWaypoints);
				SetValue(xmlDoc, root, "drawTrackpoints", "" + drawTrackpoints);
				SetValue(xmlDoc, root, "useWaypointIcons", "" + useWaypointIcons);
				SetValue(xmlDoc, root, "showHelpAtStart", "" + showHelpAtStart);
				SetValue(xmlDoc, root, "showTrackpointNumbers", "" + showTrackpointNumbers);
				SetValue(xmlDoc, root, "gpsWptDescrAsName", "" + gpsWptDescrAsName);
				SetValue(xmlDoc, root, "gpsVehicleIcon", gpsVehicleIcon);
				SetValue(xmlDoc, root, "drawCities", "" + drawCities);
				SetValue(xmlDoc, root, "drawVehicles", "" + drawVehicles);
				SetValue(xmlDoc, root, "eqUseOldData", "" + eqUseOldData);
				SetValue(xmlDoc, root, "eqFetchOnStart", "" + eqFetchOnStart);
				SetValue(xmlDoc, root, "earthquakeStyleFillRecent", "" + earthquakeStyleFillRecent);
				SetValue(xmlDoc, root, "terraserverUseServices", "" + terraserverUseServices);
				SetValue(xmlDoc, root, "sanityFilter", "" + sanityFilter);
				SetValue(xmlDoc, root, "gpsRtTrackOnMap", "" + gpsRtTrackOnMap);
				SetValue(xmlDoc, root, "gpsZoomIntoTracks", "" + gpsZoomIntoTracks);
				SetValue(xmlDoc, root, "gpsRtTrackLog", "" + gpsRtTrackLog);
				SetValue(xmlDoc, root, "gpsRtKeepInView", "" + gpsRtKeepInView);
				SetValue(xmlDoc, root, "gpsMagellanHandshake", "" + gpsMagellanHandshake);
				SetValue(xmlDoc, root, "waypointNameStyle", "" + waypointNameStyle);
				SetValue(xmlDoc, root, "trackNameStyle", "" + trackNameStyle);
				SetValue(xmlDoc, root, "earthquakeStyle", "" + earthquakeStyle);
				SetValue(xmlDoc, root, "printTextFont", "" + printTextFont);
				SetValue(xmlDoc, root, "mouseWheelAction", "" + mouseWheelAction);
				SetValue(xmlDoc, root, "distortionFactor", "" + distortionFactor);
				SetValue(xmlDoc, root, "cameraHeightMin", "" + cameraHeightMin);
				SetValue(xmlDoc, root, "coordStyle", "" + coordStyle);
				SetValue(xmlDoc, root, "unitsDistance", "" + unitsDistance);
				SetValue(xmlDoc, root, "earthquakeStyleFillHowRecentIndex", "" + earthquakeStyleFillHowRecentIndex);
				SetValue(xmlDoc, root, "terraLayerOpacity", "" + terraLayerOpacity);
				SetValue(xmlDoc, root, "terraLayerOpacity2", "" + terraLayerOpacity2);
				SetValue(xmlDoc, root, "terraLayerOpacity3", "" + terraLayerOpacity3);
				SetValue(xmlDoc, root, "terraUseOverlay", "" + terraUseOverlay);
				SetValue(xmlDoc, root, "breakTimeMinutes", "" + breakTimeMinutes);
				SetValue(xmlDoc, root, "drawTerraserver", "" + drawTerraserver);
				SetValue(xmlDoc, root, "makeWhitishTransparent", "" + makeWhitishTransparent);
				SetValue(xmlDoc, root, "drawTerraserverMode", drawTerraserverMode);
				SetValue(xmlDoc, root, "togeotiffFolderPath", togeotiffFolderPath);
				SetValue(xmlDoc, root, "togeotiffFileName", togeotiffFileName);
				SetValue(xmlDoc, root, "photoGalleryPictureSize", "" + photoGalleryPictureSize);
				SetValue(xmlDoc, root, "photoFolderPath", photoFolderPath);
				SetValue(xmlDoc, root, "photoFileName", photoFileName);
				SetValue(xmlDoc, root, "photoTimeShiftYourCamera", "" + photoTimeShiftYourCamera.Ticks);
				SetValue(xmlDoc, root, "photoTimeZoneIdYourCamera", "" + photoTimeZoneIdYourCamera);
				SetValue(xmlDoc, root, "photoTimeZoneIdFile", "" + photoTimeZoneIdFile);
				SetValue(xmlDoc, root, "photoTimeZoneIdTemp", "" + photoTimeZoneIdTemp);
				SetValue(xmlDoc, root, "photoAnalyzeOnLoad", "" + photoAnalyzeOnLoad);
				SetValue(xmlDoc, root, "photoAnalyzeOnLoadReminder", "" + photoAnalyzeOnLoadReminder);
				SetValue(xmlDoc, root, "photoDoPreview", "" + photoDoPreview);
				SetValue(xmlDoc, root, "thumbDoDisplay", "" + thumbDoDisplay);
				SetValue(xmlDoc, root, "thumbWidth", "" + thumbWidth);
				SetValue(xmlDoc, root, "thumbHeight", "" + thumbHeight);
				SetValue(xmlDoc, root, "thumbPosition", "" + thumbPosition);
				SetValue(xmlDoc, root, "photoPreserveSize", "" + photoPreserveSize);
				SetValue(xmlDoc, root, "photoFitToSize", "" + photoFitToSize);
				SetValue(xmlDoc, root, "photoSaveQuality", "" + photoSaveQuality);
				SetValue(xmlDoc, root, "useProxy", "" + useProxy);
				SetValue(xmlDoc, root, "suspendKeepAlive", "" + suspendKeepAlive);
				SetValue(xmlDoc, root, "forceEmptyProxy", "" + forceEmptyProxy);
				SetValue(xmlDoc, root, "proxyServer", proxyServer);
				SetValue(xmlDoc, root, "proxyPort", "" + proxyPort);
				SetValue(xmlDoc, root, "webTimeoutMs", "" + webTimeoutMs);
				SetValue(xmlDoc, root, "geocachingLogin", geocachingLogin);
				SetValue(xmlDoc, root, "webPageTemplate", webPageTemplate);
				SetValue(xmlDoc, root, "webPageDestinationFolder", webPageDestinationFolder);
				SetValue(xmlDoc, root, "webPagePhotoStorageUrlBase", webPagePhotoStorageUrlBase);
				SetValue(xmlDoc, root, "webPageLinksUsePopup", "" + webPageLinksUsePopup);
				SetValue(xmlDoc, root, "penTrackThickness", "" + TrackPalette.penTrackThickness);
				SetValue(xmlDoc, root, "penRouteThickness", "" + TrackPalette.penRouteThickness);

				XmlNode palNode = SetValue(xmlDoc, root, "trackPalette", "");
				for(int i=0; i < TrackPalette.brushes.Length ;i++)
				{
					Color color = TrackPalette.getColor(i);
					SetValue(xmlDoc, palNode, "c" + i, "" + color.R + "," + color.G + "," + color.B);
				}

				XmlNode fdlNode = SetValue(xmlDoc, root, "fileDescrList", "");
				foreach(FormattedFileDescr ffd in FileDescrList)
				{
					if(ffd.isPersistent)
					{
						AddFfdOption(xmlDoc, fdlNode, ffd); 
					}
				}

				XmlNode recentFilesNode = SetValue(xmlDoc, root, "recentFiles", "");
				foreach(string name in recentFiles)
				{
					AddRecentFilesOption(xmlDoc, recentFilesNode, name); 
				}

				xmlDoc.Save(optionsFilePath);

				tools.Save();
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("Project:SaveOptions() " + e.Message);
			}
		}

		public static void ReadFavorites()
		{
			string favoritesFilePath = GetMiscPath(FAVORITES_FILE_NAME);
#if DEBUG
			LibSys.StatusBar.Trace("IP: Project:ReadFavorites() path=" + favoritesFilePath);
#endif
			try 
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(favoritesFilePath);

				// we want to traverse XmlDocument fast, as tile load operations can be numerous
				// and come in pack. So we avoid using XPath and rely mostly on "foreach child":
				foreach(XmlNode nnode in xmlDoc.ChildNodes) 
				{
					if(nnode.Name.Equals("favorites")) 
					{
						foreach(XmlNode node in nnode.ChildNodes)
						{
							string nodeName = node.Name;
							try 
							{
								if(nodeName.Equals("favorites")) 
								{
									foreach(XmlNode nnnode in node.ChildNodes) 
									{
										string name = nnnode.InnerText;
										string sLng = nnnode.Attributes.GetNamedItem("lon").InnerText;
										string sLat = nnnode.Attributes.GetNamedItem("lat").InnerText;
										string sElev = nnnode.Attributes.GetNamedItem("elev").InnerText;
										XmlNode tNode = nnnode.Attributes.GetNamedItem("type");
										string type = tNode == null ? "aerial" : tNode.InnerText.Trim();
										double lng = Convert.ToDouble(sLng);
										double lat = Convert.ToDouble(sLat);
										double elev = Convert.ToDouble(sElev);
										CamPos camPos = new CamPos(lng, lat, elev, name, type);
										Project.favorites.Add(camPos);
									}
								}
							} 
							catch (Exception ee) 
							{
								// bad node - not a big deal...
								LibSys.StatusBar.Error("Project:ReadFavorites() node=" + nodeName + " " + ee.Message);
							}
						}
					}
				}

			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("Project:ReadFavorites() " + e.Message);
			}

			if(Project.favorites.Count <= 1)
			{
				// something may have gone wrong with the favorites, repopulate.
				EnsureFavorites();
			}
		}

		public static void EnsureFavorites()
		{
				// pre-populate favorites with interesting places:
				Project.favorites.Add(new CamPos(-122.336818d, 47.601259d, 640.0d, "Seattle port in color", "color aerial"));
				Project.favorites.Add(new CamPos(-122.142707d, 47.804027d, 640.0d, "Near Seattle color", "color aerial"));
				Project.favorites.Add(new CamPos(-66.1127516186667d, 18.4640733896667d, 1000.0d, "San Juan, Puerto Rico", "aerial"));
				Project.favorites.Add(new CamPos(-74.0446285231365d, 40.6891647037917d, 4000.0d, "New York, Statue of Liberty", "aerial"));
				Project.favorites.Add(new CamPos(-122.480490052853d, 37.8186409999769d, 2700.0d, "San Francisco, Golden Gate Bridge", "color aerial"));

				Project.SaveFavorites();
		}

		public static void SaveFavorites()
		{
			string favoritesFilePath = GetMiscPath(FAVORITES_FILE_NAME);
			//LibSys.StatusBar.Trace("Project:SaveFavorites: " + favoritesFilePath);
			try 
			{
				string seedXml = Project.SEED_XML + "<favorites></favorites>";
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(seedXml);

				XmlNode root = xmlDoc.DocumentElement;

				XmlNode favNode = SetValue(xmlDoc, root, "favorites", "");
				foreach(CamPos camPos in favorites)
				{
					AddFavOption(xmlDoc, favNode, camPos); 
				}

				xmlDoc.Save(favoritesFilePath);
			}
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("Project:SaveFavorites() " + e.Message);
			}
		}

		public static void savePortSettings()
		{
			FileStream fs = null;

			try
			{
				string gpsPortFilePath = Project.GetMiscPath(Project.GPSPORTCONFIG_FILE_NAME + "-" + currentGpsMake + ".xml");
				fs = new FileStream(gpsPortFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				Project.gpsPortSettings.SaveAsXML(fs);
			}
			catch (Exception ee)
			{
				LibSys.StatusBar.Error("saving port settings: " + ee.Message);
			}
			finally
			{
				if(fs != null) { fs.Close(); }
			}

		}
		#endregion //  Read/Write Options file

		#region Read/Write INI file

        /*
		 * this is how the .ini file looks like:
		 * 
				[folders]
				INSTALLDIR=C:\Program Files\QuakeMap\QuakeMap
				WINDIR=C:\WINNT
				MAPSDIR=C:\Program Files\QuakeMap\QuakeMap\Maps
				SERIALNO=1c309c5638166
		 */

        [DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		/// <summary>
		/// Write Data to the INI File
		/// </summary>
		/// <PARAM name="Section"></PARAM>
		/// Section name
		/// <PARAM name="Key"></PARAM>
		/// Key Name
		/// <PARAM name="Value"></PARAM>
		/// Value Name
		public static void IniWriteValue(string Section,string Key,string Value)
		{
			WritePrivateProfileString(Section, Key, Value, iniFilePath);
		}
        
		/// <summary>
		/// Read Data Value From the Ini File
		/// </summary>
		/// <PARAM name="Section"></PARAM>
		/// <PARAM name="Key"></PARAM>
		/// <PARAM name="Path"></PARAM>
		/// <returns></returns>
		public static string IniReadValue(string Section,string Key)
		{
			StringBuilder temp = new StringBuilder(255);
			int i = GetPrivateProfileString(Section, Key, "", temp,	255, iniFilePath);
			return temp.ToString();
		}

		public static string readIniFile(string key)
		{
			try 
			{
				return IniReadValue("folders", key);
			} 
			catch {}
			return "";
		}
		#endregion //  Read/Write INI file

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

		private static void AddFfdOption(XmlDocument xmlDoc, XmlNode ffdNode, FormattedFileDescr ffd)
		{
			XmlNode node = xmlDoc.CreateElement("file");
			XmlAttribute attr = xmlDoc.CreateAttribute("path");
			attr.InnerText = "" + ffd.filename;
			node.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("format");
			attr.InnerText = "" + ffd.formatName;
			node.Attributes.Append(attr);
			ffdNode.AppendChild(node);
		}

		private static void AddFavOption(XmlDocument xmlDoc, XmlNode favNode, CamPos camPos)
		{
			XmlNode node = xmlDoc.CreateElement("campos");
			node.InnerText = camPos.Name;
			XmlAttribute attr = xmlDoc.CreateAttribute("lon");
			attr.InnerText = "" + camPos.Lng;
			node.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("lat");
			attr.InnerText = "" + camPos.Lat;
			node.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("elev");
			attr.InnerText = "" + camPos.Elev;
			node.Attributes.Append(attr);
			attr = xmlDoc.CreateAttribute("type");
			attr.InnerText = "" + camPos.Type;
			node.Attributes.Append(attr);
			favNode.AppendChild(node);
		}

		private static void AddRecentFilesOption(XmlDocument xmlDoc, XmlNode rfNode, string path)
		{
			XmlNode node = xmlDoc.CreateElement("file");
			XmlAttribute attr = xmlDoc.CreateAttribute("path");
			attr.InnerText = "" + path;
			node.Attributes.Append(attr);
			rfNode.AppendChild(node);
		}

		/// <summary>
		/// helps to fill GPX format headers with the right stuff
		/// </summary>
		/// <param name="root"></param>
		public static void fillGpxRootNode(XmlDocument xmlDoc, XmlNode root)
		{
			// cannot add this to seed, xmlns will appear in every node:
			XmlAttribute aattr = xmlDoc.CreateAttribute("xmlns:xsi");
			aattr.InnerText = "http://www.w3.org/2001/XMLSchema-instance";
			root.Attributes.Append(aattr);
			aattr = xmlDoc.CreateAttribute("xmlns");
			aattr.InnerText = "http://www.topografix.com/GPX/1/0";
			root.Attributes.Append(aattr);
			aattr = xmlDoc.CreateAttribute("xmlns:topografix");
			aattr.InnerText = "http://www.topografix.com/GPX/Private/TopoGrafix/0/2";
			root.Attributes.Append(aattr);
			aattr = xmlDoc.CreateAttribute("xsi","schemaLocation","http://www.w3.org/2001/XMLSchema-instance");
			aattr.Value = "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd http://www.topografix.com/GPX/Private/TopoGrafix/0/2 http://www.topografix.com/GPX/Private/TopoGrafix/0/2/topografix.xsd http://www.groundspeak.com/cache/1/0 http://www.groundspeak.com/cache/1/0/cache.xsd";
			root.Attributes.Append(aattr);
		}
		#endregion // XML helpers

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

		// make cgi-specific substitutions, like # -> %23
		public static string ToCgi(string str)
		{
			int len = str.Length;
			StringBuilder buf = new StringBuilder("");
			buf.Capacity = len + 100;

			char[] chars = str.ToCharArray();
			foreach(char c in chars) 
			{
				switch(c) 
				{
					case '#':
						buf.Append("%23");
						break;
					case '%':
						buf.Append("%25");
						break;
					case '=':
						buf.Append("%3D");
						break;
					case '!':
						buf.Append("%21");
						break;
					case '{':
						buf.Append("%7B");
						break;
					case '}':
						buf.Append("%7D");
						break;
					case '[':
						buf.Append("%5B");
						break;
					case ']':
						buf.Append("%5D");
						break;
					case '\\':
						buf.Append("%5C");
						break;
					case '|':
						//buf.Append("%7C");
						//
						// we do not want "|" to appear in the database entries,
						// because it is used to separate fields in the records;
						// so map it to "+". Server side should do it too, in case
						// somebody wants to force unwanted characters in the 
						// database emulating our client:
						buf.Append("%2B");
						break;
					case '$':
						buf.Append("%24");
						break;
					case '^':
						buf.Append("%5E");
						break;
					case '&':
						buf.Append("%26");
						break;
					case '(':
						buf.Append("%28");
						break;
					case ')':
						buf.Append("%29");
						break;
					case '?':
						buf.Append("%3F");
						break;
					case '/':
						buf.Append("%2F");
						break;
					case '<':
						buf.Append("%3C");
						break;
					case '>':
						buf.Append("%3E");
						break;
					case ' ':
						buf.Append("%20");
						break;
					case '+':
						buf.Append("%2B");
						break;
					case ';':
						buf.Append("%3B");
						break;
					case '~':
						buf.Append("%7E");
						break;
					case '`':
						buf.Append("%60");
						break;
					case ',':
						buf.Append("%2C");
						break;
					case '\"':
						buf.Append("%22");
						break;
					case '\'':
						buf.Append("%27");
						break;
					default:
						buf.Append(c);
						break;
				}
			}
			return buf.ToString();
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
		#endregion // HTML/HTTP helpers

		#region System Helpers 

		public static ArrayList filesToDelete = new ArrayList();

		private static void cleanupFilesToDelete()
		{
			foreach(string fileName in filesToDelete)
			{
				if(Directory.Exists(fileName))
				{
					// allow folder removal only in temp path
					if(fileName.StartsWith(Path.GetTempPath()))
					{
						try
						{
							Directory.Delete(fileName, true);
						}
						catch {}
					}
				}
				else if(File.Exists(fileName))
				{
					try
					{
						File.Delete(fileName);
					}
					catch {}
				}
			}
		}

        /// <summary>
        /// useful for ZipFile related operations
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyStream(Stream source, Stream destination)
        {
            const int BUFFER_SIZE = 65536;
            byte[] buf = new byte[BUFFER_SIZE];

            int nBytesRead = -1;
            while ((nBytesRead = source.Read(buf, 0, BUFFER_SIZE)) > 0)
            {
                destination.Write(buf, 0, nBytesRead);
            }
        }

		public static void writeTextFile(string filename, string content)
		{
			FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
			StreamWriter tw = new StreamWriter(fs);
			tw.WriteLine(content);
			tw.Close();
		}

		public static Encoding xmlEncoding = Encoding.ASCII;

		public static byte[] StrToByteArray(string str)
		{
			return xmlEncoding.GetBytes(str);
		}

		public static string ByteArrayToStr(byte[] bytes)
		{
			return xmlEncoding.GetString(bytes, 0, bytes.Length);
		}

		public static DateTime zuluToLocal(DateTime zulu)
		{
			try 
			{
				// 13:00PDT = 21:00Z + (-8.0d hours)
				return TimeZone.CurrentTimeZone.ToLocalTime(zulu);

				//TimeZone.CurrentTimeZone.IsDaylightSavingTime(time);
				//TimeZone.CurrentTimeZone.GetUtcOffset();
			} 
			catch
			{
				return DateTime.MinValue;
			}
		}

		public static DateTime localToZulu(DateTime local)
		{
			try 
			{
				// 13:00PDT = 21:00Z + (-8.0d hours)
				return TimeZone.CurrentTimeZone.ToUniversalTime(local);
			} 
			catch
			{
				return DateTime.MinValue;
			}
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

		// converts long paths into 8.3 format, like "C:\PROGRA~1\VITALB~1\QUAKEMAP"
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
				string iconFileName = GetMiscPath(PROGRAM_NAME_HUMAN + ".ico");
				dlg.Icon = new Icon(iconFileName);
			} 
			catch {}
			// any dialog popping up should eliminate current help popup
			ClearPopup();
		}

		// we cannot waste system resources allocating new font/pen/brush each time,
		// so we use font pool to get as many different fonts as we really need:
		private static ArrayList fontPool = new ArrayList();

		public static Font getLabelFont(int fontSize)
		{
			foreach(Font ffont in fontPool) 
			{
				int fsz = (int)ffont.SizeInPoints;
				if(fsz == fontSize) 
				{
					return ffont;		// we have this font in the pool
				}
			}

			//LibSys.StatusBar.Trace("Project:getLabelFont() allocating font size" + fontSize);
			Font font = new Font("Arial", (float)fontSize);
			fontPool.Add(font);
			return font;
		}

		public static bool EqualsIgnoreCase(string a, string b)
		{
			if(a == null || b == null)
			{
				return false;
			}
			return a.ToLower().Equals(b.ToLower()); 
		}

		public static void drawCross(Graphics g, Pen pen, Point p, int size)
		{
			int x = p.X;
			int y = p.Y;
			int gap = size / 3;
		
			g.DrawLine(pen, x-size, y, x-gap, y);
			g.DrawLine(pen, x+size, y, x+gap, y);
			g.DrawLine(pen, x, y-size, x, y-gap);
			g.DrawLine(pen, x, y+size, x, y+gap);
		}

		/*
		public static string GetTempPath()
		{
			string tempPath = Path.GetTempPath();
			tempPath = Path.Combine(tempPath, Project.MAP_CACHE_PATH);
			if(!Directory.Exists(tempPath)) 
			{
				Directory.CreateDirectory(tempPath);
			}
			return tempPath;
		}
		*/

		public static string GetMiscPath(string miscFile)
		{
			if(!Directory.Exists(miscFolderPath)) 
			{
				Directory.CreateDirectory(miscFolderPath);
			}
			return Path.Combine(miscFolderPath, miscFile);
		}

		public static string GetVehPath(string vehFile)
		{
			if(!Directory.Exists(vehFolderPath)) 
			{
				Directory.CreateDirectory(vehFolderPath);
			}
			return Path.Combine(vehFolderPath, vehFile);
		}

		public static string GetWptPath(string wptFile)
		{
			if(!Directory.Exists(wptFolderPath)) 
			{
				Directory.CreateDirectory(wptFolderPath);
			}
			return Path.Combine(wptFolderPath, wptFile);
		}

		public static string GetWebPageTemplatesFolderBase()
		{
			if(!Directory.Exists(webPageTemplatesFolderBasePath)) 
			{
				Directory.CreateDirectory(webPageTemplatesFolderBasePath);
			}
			return webPageTemplatesFolderBasePath;
		}

		public static string GetWebPageTemplateFolder(string templateName)
		{
			return Path.Combine(Project.GetWebPageTemplatesFolderBase(), templateName);
		}

		public static string GetSamplePath(string sampleFile)
		{
			if(!Directory.Exists(sampleFolderPath)) 
			{
				Directory.CreateDirectory(sampleFolderPath);
			}
			return Path.Combine(sampleFolderPath, sampleFile);
		}

		private static string makeTsHash(string str)
		{
			string ret = "";

			try
			{
				string[] split = str.Substring(0, str.Length - 4).Split(new char[] { '-' });

				int x = Convert.ToInt32(split[3].Substring(1));
				int y = Convert.ToInt32(split[4].Substring(1));

				int hh = x + y;
				hh = hh % 64;

				ret = String.Format("{0}-{1:D2}", split[0], hh);
			}
			catch {}

			return ret;
		}

		public static bool forceOldTerrafolderStyle = false;

		public static void AttemptTerrafolderMigration()
		{
			try
			{
				forceOldTerrafolderStyle = false;
				if(hasOldTerrafolderStyleFiles())
				{
					DlgMigrateTerraFolder dm = new DlgMigrateTerraFolder(GetTerraserverMapsBasePath());
					dm.ShowDialog();		// will call migrateTerraserverFiles() if migration allowed
					if(!dm.migrated)
					{
						forceOldTerrafolderStyle = true;
					}
				}
			}
			catch (Exception exc)
			{
				Project.MessageBox(null, "Error: " + exc);
			}
		}

		public static bool hasOldTerrafolderStyleFiles()
		{
			string folderName = GetTerraserverMapsBasePath();

			DirectoryInfo di = new DirectoryInfo(folderName);

			if(di.GetFiles("*.jpg").Length > 0)
			{
				// may need t set forceOldTerrafolderStyle = true or migrate the files
				return true;
			}
			forceOldTerrafolderStyle = false;
			return false;
		}

		public static bool migrateTerraserverFiles(Form form, Label progressLabel, ProgressBar progressBar)
		{
			bool success = false; 
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				string folderName = GetTerraserverMapsBasePath();

				DirectoryInfo di = new DirectoryInfo(folderName);

				//			Hashtable folders = new Hashtable();

				FileInfo[] allfiles = di.GetFiles("*.jpg");
				double toMigrate = (double)allfiles.Length;
			
				progressBar.Minimum = 0;
				progressBar.Maximum = 100;

				int progressBarValue = 0;
				int lastProgressBarValue = progressBar.Value;

				int count = 0;
				DateTime started = DateTime.Now;
				string timeLeftStr = "";
				progressLabel.Text = "working...";
				foreach(FileInfo fi in allfiles)
				{
					string hash = makeTsHash(fi.Name);
	//				if(folders.Contains(hash))
	//				{
	//					int count = (int)folders[hash];
	//					count++;
	//					folders[hash] = count;
	//				}
	//				else
	//				{
	//					folders.Add(hash, 1);
	//				}
					string newFolderName = Path.Combine(folderName, hash);
					if(!Directory.Exists(newFolderName)) 
					{
						Directory.CreateDirectory(newFolderName);
					}

					string oldPath = Path.Combine(folderName, fi.Name);
					string newPath = Path.Combine(newFolderName, fi.Name);

					if(File.Exists(newPath))
					{
						// no need to move, just delete the old location
						File.Delete(oldPath);
					}
					else
					{
						File.Move(oldPath, newPath);
					}
//					Thread.Sleep(10);

					count++;
					double percentComplete = ((double)count) * 100.0d / toMigrate;
					progressBarValue = (int)Math.Floor(percentComplete);

					if(count % 50 == 0)
					{
						progressLabel.Text = fi.Name + timeLeftStr;
						progressLabel.Refresh();
						Cursor.Current = Cursors.WaitCursor;
					}

					if(progressBarValue != lastProgressBarValue)
					{
						progressBar.Value = progressBarValue;
						progressBar.Refresh();
						lastProgressBarValue = progressBarValue;

						TimeSpan elapsed = DateTime.Now - started;
						TimeSpan projected = new TimeSpan((long)(((double)elapsed.Ticks) / ((double)percentComplete) * 100.0d));
						TimeSpan left = projected - elapsed;
						timeLeftStr = "    (" + left.Minutes + " min. " + left.Seconds + " sec. left)";

						progressLabel.Text = fi.Name + timeLeftStr;
						progressLabel.Refresh();

					}
				}

				progressLabel.Text = "Done";
				success = true;

	//			System.Console.WriteLine("Hashtable count: " + folders.Count);
	//			foreach(string key in folders.Keys)
	//			{
	//				System.Console.WriteLine("" + key + " : " + folders[key]);
	//			}
			}
			catch (Exception exc)
			{
				Project.MessageBox(form, "Error: " + exc.Message);
				progressLabel.Text = "Error: " + exc.Message;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
			return success;
		}

		public static string GetTerraserverPath(string fileName)
		{
			string folderName = forceOldTerrafolderStyle ?
				GetTerraserverMapsBasePath() : Path.Combine(GetTerraserverMapsBasePath(), makeTsHash(fileName));

			try
			{
				if(!Directory.Exists(folderName)) 
				{
					Directory.CreateDirectory(folderName);
				}
			} 
			catch {}

			return Path.Combine(folderName, fileName);
		}
		
		public static string GetCleanCamtrackFolder()
		{
			string camtrackPath = camtrackFolderPath;
			if(camtrackPath != null)	// just to make sure that we are not deleting something unplanned
			{
				if(Directory.Exists(camtrackPath)) 
				{
					DirectoryInfo dirInfo = new DirectoryInfo(camtrackPath);
					foreach(FileInfo fi in dirInfo.GetFiles())
					{
						File.Delete(fi.FullName);
					}
					foreach(DirectoryInfo di in dirInfo.GetDirectories())
					{
						Directory.Delete(di.FullName, true);
					}
				}
				else
				{
					Directory.CreateDirectory(camtrackPath);
				}
			}
			return camtrackPath;
		}

		public static string mappingCacheLocation = null;	// overrides value from quakemap.ini, persistent if not null, can be reset to null

		public static void setMappingCacheLocation(string path)
		{
			mappingCacheLocation = path.Trim();

			mapsPath = null;
			GetMapsTempPath();
			
			terraPath = null;
			GetTerraserverMapsBasePath();
		}

		public static void resetMappingCacheLocation()
		{
			mappingCacheLocation = null;

			mapsPath = null;
			GetMapsTempPath();
			
			terraPath = null;
			GetTerraserverMapsBasePath();
		}

        public static string mapsPathDefault = @"C:\QuakeMapMaps";
        public static string mapsPath = null;	// C:\QuakeMapMaps   obsolete: readIniFile in MainForm initiates it

		public static string GetMapsTempPath()
		{
			if(mappingCacheLocation != null)
			{
				mapsPath = mappingCacheLocation;
			}

			if(mapsPath == null)
			{
                mapsPath = mapsPathDefault;      // readIniFile("MAPSDIR");
			}

			string ret =  Path.Combine(mapsPath, mapsQuality);
			if(!Directory.Exists(ret)) 
			{
				try
				{
					Directory.CreateDirectory(ret);
				}
				catch
				{
				}
			}
			return ret;
		}

		private static string terraPath = null;

		public static string GetTerraserverMapsBasePath()
		{
			if(mappingCacheLocation != null)
			{
				terraPath = Path.Combine(mappingCacheLocation, Project.TERRASERVER_CACHE_PATH);
			}

			if(terraPath == null)
			{
                terraPath = mapsPathDefault;      // readIniFile("MAPSDIR");
                terraPath = Path.Combine(terraPath, Project.TERRASERVER_CACHE_PATH);
			}

			if(!Directory.Exists(terraPath)) 
			{
				try
				{
					Directory.CreateDirectory(terraPath);
				}
				catch
				{
				}
			}
			return terraPath;
		}

		public static bool isDragging = false;

		// use this function for all map-related popups - they can be canceled in Map-->Options
		public static void MapShowPopup(Control parent, string caption, Point location)
		{
			if(allowMapPopups && !isDragging && mainForm.ContainsFocus)
			{
				parent.Focus();
				Project._ShowPopup(parent, caption, location, true);
			}
		}

		// use of this function can NOT be canceled in Map-->Options
		public static void MapShowPopup1(Control parent, string caption, Point location)
		{
			if(!isDragging && mainForm.ContainsFocus)
			{
				parent.Focus();
				Project._ShowPopup(parent, caption, location, true);
			}
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
//				owner.Activate();

				ErrorBoxForm errorBoxForm = new ErrorBoxForm(message);

				errorBoxForm.ShowDialog();

//				System.Windows.Forms.MessageBox.Show (owner,
//						message, Project.PROGRAM_NAME_HUMAN, 
//						System.Windows.Forms.MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch
			{}
		}

		public static void ErrorPrinterNotInstalled(Form owner)
		{
			Project.ErrorBox(owner, "Before you can print, you need to install a printer.\nUse Start-->Settings-->Printers and double-click Add Printer.");
		}



//		public static void RunStreetsAndTrips(double lng, double lat)
//		{
//			string path = Project.driveSystem + "Program Files\\Microsoft Streets and Trips\\Streets.exe";
//			if(File.Exists(path))
//			{
//				string arg = "";
//
//				System.Diagnostics.ProcessStartInfo pInfo = new	ProcessStartInfo(path, arg);
//
//				Process p = new Process();
//				p.StartInfo = pInfo;
//				p.Start(); // This will popup Microsoft Streets and Trips
//			}
//		}

		public static void RunBrowser(string url)
		{
			if(Project.serverAvailable)
			{
				System.Diagnostics.ProcessStartInfo pInfo = new	ProcessStartInfo(@"explorer.exe", url);

				Process p = new Process();
				p.StartInfo = pInfo;
				p.Start(); // This will popup the default web browser with the specified URL
			}
		}

		public static void RunFile(string fileName)
		{
			try
			{
				fileName = "\"" + fileName + "\"";
				System.Diagnostics.ProcessStartInfo pInfo = new	ProcessStartInfo(fileName);

				Process p = new Process();
				p.StartInfo = pInfo;
				p.Start(); // This will popup the editor associated with this file type
			}
			catch {}
		}

		public static void runGpsBabelWrapper(string options)
		{
			if(Project.GpsBabelWrapperExecutable.Length > 0)
			{
				System.Diagnostics.ProcessStartInfo pInfo = new	ProcessStartInfo(Project.GpsBabelWrapperExecutable, options);

				Process p = new Process();
				p.StartInfo = pInfo;
				p.Start();
			}
		}

//		public static void terraserverMap(double lng, double lat)
//		{
//			// see Tools.cs
//			string url = "\"http://terraserver.homeadvisor.msn.com/addressimage.aspx?t=4&s=8&Lon=" + lng + "&Lat=" + lat + "&w=2&opt=0\"";
//
//			Project.RunBrowser(url);
//		}

		public static void streetMap(double lng, double lat, double camElevMeters)
		{
			// less digits after dot:
			float _lng = (float)lng;
			float _lat = (float)lat;
			int zoom = 10;

			try
			{
				double dzoom = Math.Log(camElevMeters + 700.0d, 1.85d);

//				double ddzoom = Math.Log(1000.0d, 1.9d);		// close
//				ddzoom = Math.Log(25000000.0d, 1.9d);			// world

				zoom = Math.Min(17, Math.Max(2, (int)Math.Round((29.0d - dzoom),0)));

				//LibSys.StatusBar.Trace("dzoom=" + dzoom);
			}
			catch {}

			// old (MapQuest) version:
			//string url = "\"http://www.mapquest.com/maps/map.adp?size=big&zoom=9&latlongtype=decimal&latitude="	+ _lat + "&longitude=" + _lng + "\"";
			////url = "\"http://www.mapquest.com/maps/refreshmap.adp?z=9&size=b&latlongtype=decimal&latitude=33.5759763801097&longitude=-117.662791571618\"";

			// Google: http://maps.google.com/maps?ll=44.460938000000006,-93.18519500000001&z=3&t=k
			string url = "\"http://maps.google.com/maps?ll=" + _lat + "," + _lng + "&z=" + zoom + "\"";

			Project.RunBrowser(url);
		}

		public static void nearestGeocaches(double lng, double lat)
		{
			// less digits after dot:
			float _lng = (float)lng;
			float _lat = (float)lat;

			string url = "\"http://www.geocaching.com/seek/nearest_cache.asp?origin_lat=" + _lat + "&origin_long=" + _lng + "\"";

			// url = http://www.geocaching.com/seek/nearest_cache.asp?origin_lat=36.36092&origin_long=-117.28814

			Project.RunBrowser(url);
		}

		public static void insertRecentFile(string fileName)
		{
			try
			{
				if(fileName.StartsWith(Path.GetTempPath()) || fileName.StartsWith(GetLongPathName(Path.GetTempPath())))
				{
					return;
				}
			}
			catch {}

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
				/*
				if(threadPool != null)
				{
					threadPool.Stop();
					LibSys.StatusBar.WriteLine("Project: stopped threadPool - " + DateTime.Now);
				}
				*/
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

		public static string TimeSpanToString(TimeSpan ts)
		{
			string ret = ts.ToString();
			int pos = ret.IndexOf(".");
			if(pos > 0)
			{
				ret = ret.Substring(0, pos);
			}
			return ret;
		}

		#endregion // System Helpers

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
		#endregion // Windows messaging to pass strings between processes
	}

	#region class TrackPalette

	public class TrackPalette
	{
		public static float penTrackThickness = 2.0f;

		public static float penRouteThickness = 2.0f;

		public static SolidBrush[]	brushes = { new SolidBrush(Color.Cyan), new SolidBrush(Color.Magenta),
												  new SolidBrush(Color.Yellow), new SolidBrush(Color.Red),
												  new SolidBrush(Color.Orange), new SolidBrush(Color.Blue),
												  new SolidBrush(Color.Yellow), new SolidBrush(Color.Red) };

		public static Color getColor(long index)
		{
			return brushes[index % brushes.Length].Color;
		}

		public static void setColor(int index, Color color)
		{
			brushes[index % brushes.Length] = new SolidBrush(color);
		}

		public static Brush getBrush(long id)
		{
			return brushes[id % brushes.Length];
		}

		public static Pen getTrackPen(long id)
		{
			return new Pen(getBrush(id), penTrackThickness);
		}

		public static Pen getRoutePen(long id)
		{
			return new Pen(getBrush(id), penRouteThickness);
		}
	}
	#endregion // class TrackPalette

	public delegate void DelegateProcessingComplete();
	public delegate void DelegateShowPopup(Control parent, string caption, Point location);

	public interface IImageBySymbol
	{
		Bitmap getImageBySymbol(string sym);
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class EnumDescriptionAttribute : Attribute
	{
		private string description;
		public string Description { get { return description; } }

		public EnumDescriptionAttribute(string descr)
		{
			description = descr;
		}
	}

	/*
	-- obfuscator breaks this code too:
	public class EnumDescription
	{
		public static string getDescription(Enum en)
		{
			EnumDescriptionAttribute eda = (EnumDescriptionAttribute)Attribute.GetCustomAttribute(
					en.GetType().GetField(en.ToString()),
					typeof(EnumDescriptionAttribute));

			return eda == null ? null : eda.Description;
		}
	}
	*/

	#region Wizard Support classes

	public interface IWizardStepControl
	{
		void activate(object obj);
		void deactivate(object obj);
		string getNextText();
		void callback(int percentComplete, string message);
	}

	#endregion // Wizard Support classes

    #region class KmlOptions

    public class KmlOptions
    {
        // this class is designed for binding, but that doesn't work with obfuscation.
        public bool kmlShowInPopup = true;

        // Google Earth / KML document parameters:
        public bool kmlWptsClampToGround = false;
        public bool kmlTrksClampToGround = false;

        public bool ShowInPopup { get { return kmlShowInPopup; } set { kmlShowInPopup = value; } }
        public bool DontShowInPopup { get { return !kmlShowInPopup; } set { kmlShowInPopup = !value; } }
        public bool WptsClampToGround { get { return kmlWptsClampToGround; } set { kmlWptsClampToGround = value; } }
        public bool TrksClampToGround { get { return kmlTrksClampToGround; } set { kmlTrksClampToGround = value; } }
    }

    #endregion // class KmlOptions
}
