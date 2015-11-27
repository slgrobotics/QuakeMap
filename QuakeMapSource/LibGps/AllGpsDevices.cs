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

namespace LibGps
{
	public delegate void GpsInsertWaypoint(CreateInfo createInfo);

	/// <summary>
	/// Summary description for AllGpsDevices.
	/// </summary>
	public class AllGpsDevices
	{
		public static string[] AllMakesNames = new string[] {
																"Garmin",
																"Magellan",
																"Generic NMEA"
															};

		public static int[] AllMakesDefaultBaudRates = new int[] {
																9600,
																4800,
																4800
															};

		private static GpsButtonPanelControl[] AllMakesPanels = null;

		public static GpsButtonPanelControl GetGpsPanel(int index)
		{
			if(AllMakesPanels == null)
			{
				AllMakesPanels = new GpsButtonPanelControl[] {
																 new GarminGpsButtonPanelControl(),
																 new MagellanGpsButtonPanelControl(),
																 new NmeaGpsButtonPanelControl()
															 };
			}
			return AllMakesPanels[index];
		}

		public static string[][] AllModelsNames = new string[][] {
																	new string[]  {
																					"eTrex Vista",
																					"eTrex Map",
																					"GPS V"
																				  },
																	new string[] {
																					"SporTrak Pro",
																					"SporTrak Map",
																					"SporTrak"
																				  },
																	new string[] {
																					"NMEA 0183 Compliant"
																				  }
																  };

		public static string[][] AllInterfacesNames = new string[][] {
																	new string[]  {
																					   "Garmin",
																					   //"NMEA In/NMEA Out",
																					   //"RTCM In/NMEA Out"
																				   },
																	new string[] {
																					   "Magellan",
																					   //"NMEA"
																				  },
																	new string[] {
																					   "NMEA 0183"
																				  }
																};

		public AllGpsDevices()
		{
		}

		public static GpsBase MakeGpsDriver()
		{
			switch(Project.gpsMakeIndex)
			{
				case 0:
					return new GpsGarmin();
				case 1:
					return new GpsMagellan();
				case 2:
					return new GpsNmea();
			}
			return null;	// should not happen
		}
	}
}
