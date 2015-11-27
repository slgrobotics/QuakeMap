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

namespace LibSys
{
	public class MyTimeZoneDescr
	{
		public double shiftHours;
		public string name;
		public int id;

		public MyTimeZoneDescr(string _name, double _shiftHours, int _id)
		{
			name = _name;
			shiftHours = _shiftHours;
			id = _id;
		}

		public override string ToString()
		{
			return name;
		}
	}
	
	/// <summary>
	/// Summary description for MyTimeZone.
	/// </summary>
	public class MyTimeZone
	{
		// the id should be unique, otherwise feel free to insert new entries below.
		// options.xml memorizes id and will find and set accordingly
		public static MyTimeZoneDescr[] TimeZones = (new MyTimeZoneDescr[] 
				{
					new MyTimeZoneDescr("(UTC-12) - International Date Line West (Yankee)",		 -12.0d, 1120),
					new MyTimeZoneDescr("(UTC-11) - Midway Island, Samoa (X-Ray)",				 -11.0d, 1110),
					new MyTimeZoneDescr("(UTC-10) - Hawaii (Whiskey))",							 -10.0d, 1100),
					new MyTimeZoneDescr("(UTC-9)  - Alaska Standard Time (Victor)",				 -9.0d, 1090),
					new MyTimeZoneDescr("(UTC-8)  - Alaska Daylight Time (Uniform)",			 -8.0d, 1082),
					new MyTimeZoneDescr("(UTC-8)  - Pacific Standard Time (Uniform)",			 -8.0d, 1080),
					new MyTimeZoneDescr("(UTC-7)  - Pacific Daylight Time (Tango)",				 -7.0d, 1072),
					new MyTimeZoneDescr("(UTC-7)  - Mountain Standard Time (Tango)",			 -7.0d, 1070),
					new MyTimeZoneDescr("(UTC-6)  - Mountain Daylight Time (Sierra)",			 -6.0d, 1062),
					new MyTimeZoneDescr("(UTC-6)  - Central Standard Time (Sierra)",			 -6.0d, 1060),
					new MyTimeZoneDescr("(UTC-5)  - Central Daylight Time (Romeo)",				 -5.0d, 1050),
					new MyTimeZoneDescr("(UTC-5)  - Eastern Standard Time (Romeo)",				 -5.0d, 1050),
					new MyTimeZoneDescr("(UTC-4)  - Eastern Daylight Time (Quebec)",			 -4.0d, 1040),
					new MyTimeZoneDescr("(UTC-3)  - Brasilia, Buenos Aires, Greenland (Papa)",	 -3.0d, 1030),
					new MyTimeZoneDescr("(UTC-2)  - Mid-Atlantic Time (Oscar)",					 -2.0d, 1020),
					new MyTimeZoneDescr("(UTC-1)  - Azores Time (November)",					 -1.0d, 1010),
					new MyTimeZoneDescr("(UTC-0)  - Coordinated Universal Time (Zulu)",			  0.0d, 02),
					new MyTimeZoneDescr("(UTC-0)  - Western European Time (Zulu)",				  0.0d, 00),
					new MyTimeZoneDescr("(UTC+1)  - Western European Daylight Time (Alpha)",	 +1.0d, 10),
					new MyTimeZoneDescr("(UTC+2)  - Athens, Cairo, Helsinki, Jerusalem (Bravo)", +2.0d, 20),
					new MyTimeZoneDescr("(UTC+3)  - Baghdad, Moscow, Tehran (Charlie)",			 +3.0d, 30),
					new MyTimeZoneDescr("(UTC+4)  - Yerevan, Baku (Delta)",						 +4.0d, 40),
					new MyTimeZoneDescr("(UTC+5)  - Islamabad, Tashkent (Echo)",				 +5.0d, 50),
					new MyTimeZoneDescr("(UTC+6)  - Novosibirsk, Astana (Foxtrot)",				 +6.0d, 60),
					new MyTimeZoneDescr("(UTC+7)  - Bangkok, Hanoi, Krasnoyarsk (Golf)",		 +7.0d, 70),
					new MyTimeZoneDescr("(UTC+8)  - Beijing, Hong Kong, Irkutsk (Hotel)",		 +8.0d, 80),
					new MyTimeZoneDescr("(UTC+9)  - Tokyo, Seoul, Yakutsk (India)",				 +9.0d, 90),
					new MyTimeZoneDescr("(UTC+10) - Melbourne, Sydney, Vladivostok (Kilo)",		 +10.0d, 100),
					new MyTimeZoneDescr("(UTC+11) - Magadan (Lima)",							 +11.0d, 110),
					new MyTimeZoneDescr("(UTC+12) - Auckland, Fiji, Kamchatka (Mike)",			 +12.0d, 120)
				}
			);

		// never called:
		private MyTimeZone()
		{
		}

		public static TimeSpan zoneTimeShift
		{
			get
			{
				if(System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
				{
					TimeSpan daylightShift = new TimeSpan(1, 0, 0);
					return DateTime.Now - Project.localToZulu(DateTime.Now) - daylightShift;
				}
				else
				{
					return DateTime.Now - Project.localToZulu(DateTime.Now);
				}
			}
		}

		// for setting dropdown 
		public static int indexById(int id)
		{
			int ret = -1;

			for(int i=0; i < TimeZones.Length ;i++)
			{
				if(TimeZones[i].id == id)
				{
					ret = i;
					break;
				}
			}

			if(ret == -1)
			{
				// default in unlikely event that id won't be found - PDT must be there
				ret = indexById(1072);
			}

			return ret;
		}

		public static MyTimeZoneDescr timeZoneById(int id)
		{
			return TimeZones[indexById(id)];
		}

		public static TimeSpan timeSpanById(int id)
		{
			return new TimeSpan((long)(TimeZones[indexById(id)].shiftHours * 36000000000.0d));
		}

	}
}
