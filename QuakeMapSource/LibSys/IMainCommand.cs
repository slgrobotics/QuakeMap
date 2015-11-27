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
using System.Windows.Forms;
using System.Collections;

namespace LibSys
{
	//
	// MainForm implements this interface:
	//
	public interface IMainCommand 
	{
		void trackProperties(long trackId);		// bring up WaypointsManager with track selected
		void wptEnabled(bool enable);			// enable/disable waypoints layer
		void eqEnabled(bool enable);			// enable/disable earthquakes layer
		void eqFilterOn(bool enable);			// on/off earthquakes filter indicator
		void camtrackOn(bool enable);			// on/off camera track indicator
		object getTrackById(long trackId);		// returns Track or null, uses WaypointsCache.getTrackById(long trackId)
		void doBack();
		void doForward();
		void zoomIn();							// emulates clicks on arrows of the heightScrollBar
		void zoomOut();
		void zoomInLarge();
		void zoomOutLarge();
		void setMapMode(string mode);
		void photoViewerUp(object wpt);
		void photoEditorUp(object wpt);
		Form gpsManagerForm();					// parent for error box
		SortedList getWaypointsWithThumbs(long trkid);
		string toUtmString(double lon, double lat, double elev);
		bool fromUtmString(string text, out double lng, out double lat);
		void doPrintPage(Rectangle physicalBounds, Graphics graphics, bool webStyle);
		string doGenerateClickMap(Rectangle physicalBounds);
		void trySetInternetAvailable(bool onoff);
		Bitmap getVehicleImage(string name);
		bool PlannerPaneVisible();
	}
}
