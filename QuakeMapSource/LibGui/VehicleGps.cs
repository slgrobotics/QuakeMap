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
using System.IO;
using System.Drawing;

using LibSys;
using LibNet;
using LibGeo;
using LibGps;

namespace LibGui
{
	/// <summary>
	/// Summary description for VehicleGps.
	/// </summary>
	public class VehicleGps : Vehicle
	{
		// we need to have a handy Vehicle object for GetType() comparisons:
		public static VehicleGps vehicleGps = new VehicleGps();
		public static new Type getType() { return VehicleGps.vehicleGps.GetType(); }

		protected bool m_doTrackLog = false;
		public bool doTrackLog { get { return m_doTrackLog; } set { m_doTrackLog = value; } }

		public static Bitmap m_image_nofix = null;
		public static Bitmap m_image_default = null;

		public Bitmap m_imageSym = null;
		public DateTime m_lastFix = DateTime.MinValue;	// last good fix
 
		public static Brush brushVelocity = new SolidBrush(Color.Red);
		public static Pen penVelocity = new Pen(Color.Red);

		public VehicleGps()
		{
		}

		public VehicleGps(GpsRealTimeData rtData, string name, string sym, string source, string url, string desc, bool doTrackLog)
			: base(rtData.location.Clone(), name, sym, source, url, desc)			// location must be not null
		{
			RealTimeData = rtData;
			m_doTrackLog = doTrackLog;

			if(m_doTrackLog)
			{
				logTrack(rtData);
			}

			if(m_image_nofix == null)
			{
				m_image_nofix = VehiclesCache.getVehicleImage("gps_nofix");
			}

			if(m_image_default == null)
			{
				m_image_default = VehiclesCache.getVehicleImage("gps_car_default");
			}

			setImage(sym);

			Image = m_imageSym;  //m_image_nofix;				// may be null
			
			imageBoundingRect();
		}

		public void setImage(string sym)
		{
			m_imageSym = VehiclesCache.getVehicleImage(sym);	// may be null
		}

		public void ProcessMove(GpsRealTimeData rtData)
		{
			if(rtData == null)
			{
				resetTrackLog();
				return;
			}

			if(rtData.location != null)
			{
				RealTimeData = rtData;

				bool locationChanged = false;
				bool imageChanged = false;
				bool haveFix = false;
				
				Rectangle prev = this.BoundingRect;	// on previous image/location
				Rectangle prevImg = this.m_imageBoundingRect;	// on previous image

				switch(rtData.fix)
				{
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
						if(Image != m_imageSym) 
						{
							imageChanged = true;
							Image = m_imageSym;
						}
						haveFix = true;
						m_lastFix = DateTime.Now;
						//if(rtData.location.Lng != this.Location.Lng || rtData.location.Lat != this.Location.Lat)
						double moveTreshold = 0.000005d;
						if(Math.Abs(rtData.location.Lng - this.Location.Lng) > moveTreshold
							|| Math.Abs(rtData.location.Lat - this.Location.Lat) > moveTreshold)
						{
							locationChanged = true;
							this.Location.Lng = rtData.location.Lng;
							this.Location.Lat = rtData.location.Lat;
						}
						imageBoundingRect();
						if(!prevImg.Equals(this.m_imageBoundingRect))
						{
							imageChanged = true;
						}
						break;
					default:
						if(Image != m_image_nofix) 
						{
							imageChanged = true;
							Image = m_image_nofix;
						}
						break;
				}

				if(imageChanged)
				{
					m_pixelRadius = (Image == null) ? MIN_VEH_PIXEL_RADIUS : (Image.Width + Image.Height) * 3 / 16;
				}

				this.tick();

				if(haveFix && m_doTrackLog)
				{
					logTrack(rtData);
				}

				if(locationChanged || imageChanged)
				{
					VehiclesCache.vehicleMoved(this, prev);	// will put on map and recalculate image bounding rect
				}
			} 
			else if(Image != m_image_nofix && m_lastFix.AddSeconds(10).CompareTo(DateTime.Now) < 0)
			{
				RealTimeData = rtData;
				Rectangle prev = this.BoundingRect;	// on previous image/location
				Image = m_image_nofix;
				this.tick();
				VehiclesCache.vehicleMoved(this, prev);	// will put on map and recalculate image bounding rect
			}
		}

		#region Track logging -- logTrack()

		private int howfarIndex = 2;
		private long trackId = -1;
		private int totalTrkpt = 0;
		private double lastLat = -1000.0d;
		private double lastLng = -1000.0d;
		private double lastElev = -1000000.0d;
		private GeoCoord lastLoc = null;

		private void resetTrackLog()
		{
			trackId = -1;
			totalTrkpt = 0;
			lastLat = -1000.0d;
			lastLng = -1000.0d;
			lastElev = -1000000.0d;
			lastLoc = null;
		}

		// real-time track logging:
		private bool logTrack(GpsRealTimeData  rtData)
		{
			bool ret = false;

			// only good fixes with non-null location reach here
			string source = this.Label + " " + DateTime.Now;

			if(trackId == -1)
			{
				trackId = ++Project.trackId;

				string trackName = "VEHICLE-LOG-" + trackId + " " + this.Label;

				CreateInfo createInfo = new CreateInfo();

				createInfo.init("trk");
				createInfo.name	= trackName;
				createInfo.id = trackId;
				createInfo.source = source;

				totalTrkpt = 0;

				WaypointsCache.insertWaypoint(createInfo);	// actually inserts a track
				WaypointsCache.isDirty = true;
			}

			// garmin sends data every second, even if it is same point. Ignore repetetions to save memory.

			double[] howfars = new Double[] { 0.0d, 1.0d, 10.0d, 20.0d, 50.0d, 100.0d };

			double howFarMeters = howfars[howfarIndex];
			GeoCoord loc = new GeoCoord(rtData.location.Lng, rtData.location.Lat, rtData.location.Elev);
			// tolerance 0.0001 degree = 10 meters
			if(howfarIndex == 0 || !loc.almostAs(lastLoc, howFarMeters * 0.00001d))		// 0 means every incoming reading
			{
				CreateInfo createInfo = new CreateInfo();

				createInfo.init("trkpt");
				createInfo.id = trackId;			// relate waypoint to track
				createInfo.dateTime = rtData.time;
				createInfo.lat = rtData.location.Lat;
				createInfo.lng = rtData.location.Lng;
				createInfo.elev = rtData.location.Elev;
				createInfo.source = source;
				//createInfo.name = "" + totalTrkpt;		// track point name

				lastLat = rtData.location.Lat;
				lastLng = rtData.location.Lng;
				lastElev = rtData.location.Elev;

				totalTrkpt++;
				ret = true;

				WaypointsCache.insertWaypoint(createInfo);
				lastLoc = loc;
			}
			return ret;
		}
		#endregion

//		// interface Tickable
//		public override void tick()
//		{
//			Name = "" + Label + " - " + DateTime.Now;
//		}

		public override string toStringPopup()
		{
			string tmp = "" + Label;

			if(this.Desc != null && this.Desc.Length > 0)
			{
				tmp += "\n\n" + this.Desc.Trim() + "\n\n";
			}
			if(this.Url != null && this.Url.Length > 0)
			{
				tmp += Project.serverAvailable ? "[click vehicle or label to browse]\n" : "[can't browse offline]\n";
			}

			return tmp;
		}

		public override void Paint(Graphics graphics, IDrawingSurface tileSet, ITile iTile)
		{
			if(!m_enabled) 
			{
				return;
			}

			Point pLoc = tileSet.toPixelLocation(Location, iTile);
			//LibSys.StatusBar.Trace("VehicleGps::Paint()  - " + Location + " - " + m_doName + m_labelBoundingRect);
        
			Font font = Project.getLabelFont(m_fontSize);
			
			int xx = m_labelBoundingRect.X - 3;
			int yy = m_labelBoundingRect.Y - 1;
			//string label = "" + m_labelPosition + " " + Name;
			string label = Name;
			if(Project.vehicleUseShadow) 
			{
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx,   yy);
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx+2, yy);
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx,   yy-2);
				graphics.DrawString(label, font, Project.vehicleBackgroundBrush, xx+2, yy-2);
			}
			graphics.DrawString(label, font, Project.vehicleFontBrush, xx+1, yy-1);
        
			if(Image == null) 
			{
				graphics.DrawEllipse(Project.vehiclePen, m_pixelLocation.X-MIN_VEH_PIXEL_RADIUS+2, m_pixelLocation.Y-MIN_VEH_PIXEL_RADIUS+2, MIN_VEH_PIXEL_RADIUS*2-4, MIN_VEH_PIXEL_RADIUS*2-4);
				graphics.FillEllipse(Project.vehicleBrush, m_pixelLocation.X-2, m_pixelLocation.Y-2, 4, 4);
			} 
			else 
			{
				// assume the center of the image is hotpoint:
				graphics.DrawImage(Image, m_pixelLocation.X-Image.Width/2, m_pixelLocation.Y-Image.Height/2);
				graphics.DrawEllipse(Project.vehiclePen, m_pixelLocation.X-2, m_pixelLocation.Y-2, 4, 4);
			}

			if(arrowPoints != null)
			{
				graphics.FillPolygon(brushVelocity, arrowPoints);
				graphics.DrawLine(penVelocity, new Point(m_pixelLocation.X, m_pixelLocation.Y), arrowPoints[0]);
			}

			// debug only:
			//	graphics.DrawRectangle(Project.debugPen, m_boundingRect);
			//	graphics.DrawRectangle(Project.debug2Pen, m_imageBoundingRect);
			//	graphics.DrawRectangle(Project.debug3Pen, m_labelBoundingRect);
		}

		private const int		ARROW_WINGS		= 24;
		private const double	ARROW_LENGTH	= 50.0d;
		private Point[]			arrowPoints		= null;

		private void calcArrowPoints()
		{
			double vecLen = Math.Sqrt(RealTimeData.velocityEast * RealTimeData.velocityEast
										+ RealTimeData.velocityNorth * RealTimeData.velocityNorth);
			//LibSys.StatusBar.Trace("VehicleGps:: velocity vecLen=" + vecLen);
			if(vecLen > 0.1d)
			{
				Point p1 = new Point(m_pixelLocation.X, m_pixelLocation.Y);
				Point p2 = new Point(p1.X, p1.Y);
				int dx = (int)(RealTimeData.velocityEast / vecLen * ARROW_LENGTH);
				int dy = -(int)(RealTimeData.velocityNorth / vecLen * ARROW_LENGTH);
				p2.Offset(dx, dy);
				// draw arrow
				// thanks to http://www.codeguru.com/mfc/comments/27158.shtml
				Point dlta = new Point(p2.X - p1.X, p2.Y - p1.Y);
				int len = (int)Math.Sqrt(dlta.X * dlta.X + dlta.Y * dlta.Y);
				double mltpl = ((double)ARROW_WINGS) / ((double)(len==0?1:len));
					
				Point A = new Point(p2.X - (int)(mltpl * dlta.X), p2.Y - (int)(mltpl * dlta.Y));
				arrowPoints = new Point[3];
				arrowPoints[0] = p2;
				mltpl /= 4;	// 2 or 3 makes arrow fatter
				arrowPoints[1] = new Point(A.X - (int)(mltpl * dlta.Y), A.Y + (int)(mltpl * dlta.X));
				arrowPoints[2] = new Point(A.X + (int)(mltpl * dlta.Y), A.Y - (int)(mltpl * dlta.X));
			}
			else
			{
				arrowPoints = null;
			}
		}

		public override Rectangle imageBoundingRect()
		{
			// assume center of the image is hotpoint:
			m_imageBoundingRect = (Image == null) ?
				  new Rectangle(m_pixelLocation.X-MIN_VEH_PIXEL_RADIUS+2, m_pixelLocation.Y-MIN_VEH_PIXEL_RADIUS+2,
										MIN_VEH_PIXEL_RADIUS*2-4, MIN_VEH_PIXEL_RADIUS*2-4)
				: new Rectangle(m_pixelLocation.X-Image.Width/2, m_pixelLocation.Y-Image.Height/2,
										Image.Width, Image.Height);

			calcArrowPoints();

			if(arrowPoints != null)
			{
				int minX = 65000;
				int maxX = -65000;
				int minY = 65000;
				int maxY = -65000;
				for(int i=0; i < 3 ;i++)
				{
					minX = Math.Min(Math.Min(minX, arrowPoints[i].X), m_imageBoundingRect.X);
					maxX = Math.Max(Math.Max(maxX, arrowPoints[i].X), m_imageBoundingRect.X + m_imageBoundingRect.Width);
					minY = Math.Min(Math.Min(minY, arrowPoints[i].Y), m_imageBoundingRect.Y);
					maxY = Math.Max(Math.Max(maxY, arrowPoints[i].Y), m_imageBoundingRect.Y + m_imageBoundingRect.Height);
				}
				m_imageBoundingRect.X = minX;
				m_imageBoundingRect.Y = minY;
				m_imageBoundingRect.Width = maxX - minX + 1;
				m_imageBoundingRect.Height = maxY - minY + 1;
			}
			return m_imageBoundingRect;
		}

	}
}
