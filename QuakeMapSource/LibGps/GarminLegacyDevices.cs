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

namespace LibGps
{
	// legacy protocol support inspired by Garble 1.0.1, data filled in from Garmin December 1999 specs
	public class legacy_device 
	{
		public uint id;
		public uint version_min;
		public uint version_max;
		public string sprotocols;

		public legacy_device(uint _id, uint vmin, uint vmax, string protocols)
		{
			id = _id;
			version_min = vmin;
			version_max = vmax;
			sprotocols = protocols;
		}
	}

	public class devices
	{
		public const uint max_version =0xffff;
		public const uint min_version = 0;
		
		public static legacy_device[] legacy_devices = new legacy_device[]
			{	
				new legacy_device(7,  min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A500 D500"),
				new legacy_device(25, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A300 D300 A400 D400 A500 D500"),
				new legacy_device(13, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A300 D300 A400 D400 A500 D500"),
				new legacy_device(14, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A400 D400 A500 D500"),
				new legacy_device(15, min_version, max_version,  "L001 A010 A100 D151 A200 D200 D151 A400 D151 A500 D500"),
				new legacy_device(18, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A300 D300 A400 D400 A500 D500"),
				new legacy_device(20, min_version, max_version,  "L002 A011 A100 D150 A200 D201 D150 A400 D450 A500 D550"),
				new legacy_device(22, min_version, max_version,  "L001 A010 A100 D152 A200 D200 D152 A300 D300 A400 D152 A500 D500"),
				new legacy_device(23, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A300 D300 A400 D400 A500 D500"),
				new legacy_device(24, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A300 D300 A400 D400 A500 D500"),
				new legacy_device(29, min_version, 399,          "L001 A010 A100 D101 A200 D201 D101 A300 D300 A400 D101 A500 D500"),
				new legacy_device(29, 400, max_version,          "L001 A010 A100 D102 A200 D201 D102 A300 D300 A400 D102 A500 D500"),
				new legacy_device(31, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(33, min_version, max_version,  "L002 A011 A100 D150 A200 D201 D150 A400 D450 A500 D550"),
				new legacy_device(34, min_version, max_version,  "L002 A011 A100 D150 A200 D201 D150 A400 D450 A500 D550"),
				new legacy_device(35, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A300 D300 A400 D400 A500 D500"),
				new legacy_device(36, min_version, 299,          "L001 A010 A100 D152 A200 D200 D152 A300 D300 A400 D152 A500 D500"),
				new legacy_device(36, 300, max_version,          "L001 A010 A100 D152 A200 D200 D152 A300 D300 A500 D500"),
				new legacy_device(39, min_version, max_version,  "L001 A010 A100 D151 A200 D201 D151 A300 D300 A500 D500"),
				new legacy_device(41, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(42, min_version, max_version,  "L001 A010 A100 D100 A200 D200 D100 A300 D300 A400 D400 A500 D500"),
				new legacy_device(44, min_version, max_version,  "L001 A010 A100 D101 A200 D201 D101 A300 D300 A400 D101 A500 D500"),
				new legacy_device(45, min_version, max_version,  "L001 A010 A100 D152 A200 D201 D152 A300 D300 A500 D500"),
				new legacy_device(47, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(48, min_version, max_version,  "L001 A010 A100 D154 A200 D201 D154 A300 D300 A500 D501"),
				new legacy_device(49, min_version, max_version,  "L001 A010 A100 D102 A200 D201 D102 A300 D300 A400 D102 A500 D501"),
				new legacy_device(50, min_version, max_version,  "L001 A010 A100 D152 A200 D201 D152 A300 D300 A500 D501"),
				new legacy_device(52, min_version, max_version,  "L002 A011 A100 D150 A200 D201 D150 A400 D450 A500 D550"),
				new legacy_device(53, min_version, max_version,  "L001 A010 A100 D152 A200 D201 D152 A300 D300 A500 D501"),
				new legacy_device(55, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(56, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(59, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(61, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(62, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(64, min_version, max_version,  "L002 A011 A100 D150 A200 D201 D150 A400 D450 A500 D551"),
				new legacy_device(71, min_version, max_version,  "L001 A010 A100 D155 A200 D201 D155 A300 D300 A500 D501"),
				new legacy_device(72, min_version, max_version,  "L001 A010 A100 D104 A200 D201 D104 A300 D300 A500 D501"),
				new legacy_device(73, min_version, max_version,  "L001 A010 A100 D103 A200 D201 D103 A300 D300 A500 D501"),
				new legacy_device(74, min_version, max_version,  "L001 A010 A100 D100 A200 D201 D100 A300 D300 A500 D500"),
				new legacy_device(76, min_version, max_version,  "L001 A010 A100 D102 A200 D201 D102 A300 D300 A400 D102 A500 D501"),
				new legacy_device(77, min_version, 300,          "L001 A010 A100 D100 A200 D201 D100 A300 D300 A400 D400 A500 D501"),
				new legacy_device(77, 301, 349,                  "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(77, 350, 360,                  "L001 A010 A100 D103 A200 D201 D103 A300 D300 A500 D501"),
				new legacy_device(77, 361, max_version,          "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(87, min_version, max_version,  "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(88, min_version, max_version,  "L001 A010 A100 D102 A200 D201 D102 A300 D300 A400 D102 A500 D501"),
				new legacy_device(95, min_version, max_version,  "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(96, min_version, max_version,  "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(97, min_version, max_version,  "L001 A010 A100 D103 A200 D201 D103 A300 D300 A500 D501"),
				new legacy_device(98, min_version, max_version,  "L002 A011 A100 D150 A200 D201 D150 A400 D450 A500 D551"),
				new legacy_device(100, min_version, max_version, "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(105, min_version, max_version, "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(106, min_version, max_version, "L001 A010 A100 D103 A200 D201 D103 A300 D300 A400 D403 A500 D501"),
				new legacy_device(112, min_version, max_version, "L001 A010 A100 D152 A200 D201 D152 A300 D300 A500 D501")
		};

		public static string tryUseLegacyDevice(int product_id, int software_version, ArrayList protocols )
		{
			string ret = "";
			// try to compose our own capability info from the device id 
			for (int i = 0; i < legacy_devices.Length; i++)
			{
				legacy_device ldp = legacy_devices[i];

				if (ldp.id == product_id && 
					ldp.version_min <= software_version &&
					ldp.version_max >= software_version)
				{
					char[] sep = new Char[1] { ' ' };
					string[] split = ldp.sprotocols.Split(sep);
					// found it, creating capability info
					for (int j = 0; j < split.Length ; j++)
					{
						protocols.Add(split[j]);
						ret += " " + split[j];
					}
					break;	
				}
			}
			return ret.Trim();
		}
	}
}
