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
using System.Drawing;

namespace LibGeo
{
	/// <summary>
	/// Summary description for IDrawingSurface.
	/// It provides "drawing surface" services to cities etc.
	/// handles necessary conversions between Geo coordinates and pixels on the screen.
	/// </summary>
	public interface IDrawingSurface
	{
		Graphics   getGraphics();
		bool       insideScreenRectangle(GeoCoord loc);
		Point	   toPixelLocation(GeoCoord loc, ITile iTile);			// iTile can be null
		Point	   toPixelLocationPrint(GeoCoord loc, ITile iTile);		// iTile can be null
		Point	   toPixelLocationNoNormalize(GeoCoord loc);
		Point	   toPixelLocationNoNormalizePrint(GeoCoord loc);
		Rectangle  getFrameRectangle(ITile tile);						// iTile can be null
		int		   countIntersections(IObjectsLayoutManager iOlm, LiveObject lo, Rectangle rect);
		double     xMetersPerPixel();
		double     yMetersPerPixel();
		double     getCameraElevation();
	}
}
