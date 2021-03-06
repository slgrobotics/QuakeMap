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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using LibSys;

namespace LibGeo
{
	/// <summary>
	/// Landmark comes from TerraServer or similar source.
	/// </summary>
	public class Landmark : LabeledPoint
	{
		// we need to have a handy Landmark object for GetType() comparisons:
		public static Landmark landmark = new Landmark();

		// in most cases type-related processing is associated with LiveObjectType property ("wpt", "rtept", "trkpt", "geocache", "landmark"...)
		// if some descriptive type comes from the source ("geocache|Traditional Geocache") - WptType is the place to put it.
		protected string	m_landmarkType = "";
		public string		LandmarkType  { get { return m_landmarkType; } set { m_landmarkType = value; } }

		public Landmark()
		{
		}

		public Landmark(string name, GeoCoord location, string landmarkType)
		{
			LiveObjectType = LiveObjectTypes.LiveObjectTypeLandmark;
			Location = location;
			Name = name;
			LandmarkType = landmarkType;

			brushFont = Project.landmarkFontBrush;
			brushBackground = Project.landmarkBackgroundBrush;
			penMain = Project.landmarkPen;
			penUnderline = Project.landmarkPen;
		}

		public override string ToString()
		{
			return Location + " type=" + LandmarkType;
		}

		public override string toTableString()
		{
			return ToString();
		}

		protected override int fontSizeByType(LiveObjectTypes type)
		{
			int ret = 8;

			/*
			switch(type)
			{
				case "Institution":
					ret = 18;
					break;
			}
			*/

			return ret;
		}

		protected override string getLabel(bool getAll)
		{
			return m_name;
		}

		public bool sameAs(LabeledPoint other)
		{
			return Location.almostAs(other.Location, 0.01d) && Name.ToLower().Equals(other.Name.ToLower());
		}

	}
}
