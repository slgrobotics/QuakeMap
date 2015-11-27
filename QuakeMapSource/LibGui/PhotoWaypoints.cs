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
using System.IO;

using LibSys;
using LibGeo;

namespace LibGui
{
	/// <summary>
	/// PhotoWaypoints handles operations on the list of waypoints with photos.
	/// </summary>
	public sealed class PhotoWaypoints
	{
		private static SortedList m_waypointsWithThumbs = new SortedList();
		public  static SortedList WaypointsWithThumbs { get { return m_waypointsWithThumbs; } }

		private static SortedList m_photosUnrelated = new SortedList();		// list of PhotoDescr, sorted by imageSource
		public  static SortedList PhotosUnrelated { get { return m_photosUnrelated; } }

		public static int CurrentWptIndex = 0;

		public static SortedList getWaypointsWithThumbs(long trkid)
		{
			SortedList ret = new SortedList();

			for (int i=0; i < WaypointsWithThumbs.Count ;i++)
			{
				Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
				if(wpt.TrackId == trkid)
				{
					ret.Add(wpt.DateTime, wpt);
				}
			}

			return ret;
		}

		// also puts current pointer within the list, if possible
		public static bool hasCurrentWaypoint()
		{
			int count = WaypointsWithThumbs.Count;
			if(count > 0)
			{
				if(CurrentWptIndex >= 0 && CurrentWptIndex <= count - 1)
				{
					return true;
				}
				else if(CurrentWptIndex > count - 1)
				{
					CurrentWptIndex = count - 1;
					return true;
				}
				else
				{
					CurrentWptIndex = 0;
					return true;
				}
			}
			return false;
		}

		public static bool hasNextWaypoint()
		{
			if(!hasCurrentWaypoint())	// make sure the list has some points, and the pointer is sane or has been repaired
			{
				return false;
			}

			int count = WaypointsWithThumbs.Count;
			if(CurrentWptIndex < count - 1)
			{
				return true;
			}
			return false;
		}

		public static bool hasPreviousWaypoint()
		{
			if(!hasCurrentWaypoint())	// make sure the list has some points, and the pointer is sane or has been repaired
			{
				return false;
			}

			return CurrentWptIndex > 0;
		}

		public static Waypoint CurrentWaypoint()
		{
			if(!hasCurrentWaypoint())
			{
				return null;
			}
			return (Waypoint)WaypointsWithThumbs.GetByIndex(CurrentWptIndex);
		}

		public static Waypoint NextWaypoint()
		{
			if(!hasNextWaypoint())
			{
				return null;
			}
			return (Waypoint)WaypointsWithThumbs.GetByIndex(++CurrentWptIndex);
		}

		public static Waypoint PreviousWaypoint()
		{
			if(!hasPreviousWaypoint())
			{
				return null;
			}
			return (Waypoint)WaypointsWithThumbs.GetByIndex(--CurrentWptIndex);
		}

		public static Waypoint FirstWaypoint()
		{
			if(!hasCurrentWaypoint())
			{
				return null;
			}
			CurrentWptIndex = 0;
			return (Waypoint)WaypointsWithThumbs.GetByIndex(CurrentWptIndex);
		}

		public static Waypoint LastWaypoint()
		{
			if(!hasCurrentWaypoint())
			{
				return null;
			}
			int count = WaypointsWithThumbs.Count;
			CurrentWptIndex = count - 1;
			return (Waypoint)WaypointsWithThumbs.GetByIndex(CurrentWptIndex);
		}

		// try to set the pointer to the given waypoint
		public static bool SetCurrentWaypoint(Waypoint wpt)
		{
			if(!hasCurrentWaypoint())
			{
				return false;
			}

			for (int i=0; i < WaypointsWithThumbs.Count ;i++)
			{
				Waypoint curWpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
				if(wpt == curWpt)
				{
					CurrentWptIndex = i;
					return true;
				}
			}

			return false;
		}

		public static void cleanPhotoTrackpoints(string thumbFileName)	// thumbFileName may be null - then clean all, else clean one
		{
			for (int i=0; i < WaypointsWithThumbs.Count ;i++)
			{
				Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
				if(thumbFileName == null || thumbFileName.Equals(wpt.ThumbSource))
				{
					Track trk = WaypointsCache.getTrackById(wpt.TrackId);

					if(trk != null)
					{
						trk.Trackpoints.Remove(wpt.DateTime);
					}

					if(thumbFileName != null)
					{
						WaypointsWithThumbs.RemoveAt(i);
						break;
					}
				}
			}
			if(thumbFileName == null)
			{
				WaypointsWithThumbs.Clear();
			}
			hasCurrentWaypoint();	// make sure the pointer is as sane as it can be
		}

		public static void cleanPhotoTrackpointsBySource(string sourceFileName)
		{
			for (int i=WaypointsWithThumbs.Count-1; i >=0 ;i--)
			{
				Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
				string wptSource = wpt.Source;
				if(wptSource.StartsWith(sourceFileName))
				{
					string rem = wptSource.Substring(sourceFileName.Length);
					if(rem.IndexOf("\\") <= 0)	// can start with \, like in "\trip.gpx"
					{
						Track trk = WaypointsCache.getTrackById(wpt.TrackId);

						if(trk != null)
						{
							trk.Trackpoints.Remove(wpt.DateTime);
						}

						WaypointsWithThumbs.RemoveAt(i);
					}
				}
				else if(wptSource.IndexOf("\\") <= 0)		// came from GPS, not from file - try if thumbSource matches
				{
					string wptImgSource = wpt.ThumbSource;
					if(wptImgSource.StartsWith(sourceFileName))
					{
						Track trk = WaypointsCache.getTrackById(wpt.TrackId);

						if(trk != null)
						{
							trk.Trackpoints.Remove(wpt.DateTime);
						}

						WaypointsWithThumbs.RemoveAt(i);
					}
				}
			}
			hasCurrentWaypoint();	// make sure the pointer is as sane as it can be
		}

		public static void cleanUnrelated(string namePattern)
		{
			if(namePattern == null)
			{
				PhotosUnrelated.Clear();
			}
			else
			{
				for (int i=PhotosUnrelated.Count-1; i >=0 ;i--)
				{
					PhotoDescr pd = (PhotoDescr)PhotosUnrelated.GetByIndex(i);
					string pdSource = pd.imageSource;
					if(pdSource.IndexOf(namePattern) != -1)
					{
						PhotosUnrelated.RemoveAt(i);
					}
				}
			}
		}

		private static void registerUnrelatedPhoto(PhotoDescr photoDescr)
		{
			try
			{
				if(PhotosUnrelated.ContainsKey(photoDescr.imageSource))
				{
					PhotosUnrelated.Remove(photoDescr.imageSource);
				}
				PhotosUnrelated.Add(photoDescr.imageSource, photoDescr);
			} 
			catch {}
		}

		/// <summary>
		/// rebuilds thumbnails for all photo waypoints, so that size changes can take place
		/// </summary>
		public static void reprocessThumbnails()
		{
			for (int i=0; i < WaypointsWithThumbs.Count; i++)
			{
				Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);

// old way:
//				PhotoDescr photoDescr = PhotoDescr.FromFile(wpt.ThumbSource, wpt.Name);
//				wpt.ThumbImage = photoDescr.ThumbnailImage;
//				photoDescr.releaseImage();

				wpt.ThumbImage = PhotoDescr.rebuildThumbnailImage(wpt.ThumbSource);
			}
		}

		// the following helps to figure out why images fall outside the track:
		public static DateTime imagesTimestampMin;		// all times zulu
		public static DateTime imagesTimestampMax;
 
		public static void resetImagesTimestamps()
		{
			imagesTimestampMin = DateTime.MaxValue;
			imagesTimestampMax = DateTime.MinValue;
		}

		private static void registerImageTimestamp(DateTime ts)
		{
			if(ts.CompareTo(imagesTimestampMin) < 0)
			{
				imagesTimestampMin = ts;
			}

			if(ts.CompareTo(imagesTimestampMax) > 0)
			{
				imagesTimestampMax = ts;
			}
		}


		public static Waypoint createPhotoTrackpoint(PhotoDescr photoDescr, TimeSpan photoTimeShift, int tzId)
		{
			if(photoDescr.hasCoordinates)
			{
				// There is EXIF coordinates set, use it.
				GeoCoord loc = new GeoCoord(photoDescr.Longitude, photoDescr.Latitude, photoDescr.Altitude);
				string name = photoDescr.imageName;
				string source = photoDescr.imageSource;
				string url = photoDescr.imageUrl;
				Waypoint wpt0 = new Waypoint(loc, Project.localToZulu(photoDescr.DTOrig),
													LiveObjectTypes.LiveObjectTypeWaypoint, -1, name, source, url) ;
				wpt0.Desc = photoDescr.imageName;
				wpt0.ThumbImage = photoDescr.ThumbnailImage;
				wpt0.ThumbSource = photoDescr.imageThumbSource;
				wpt0.ThumbPosition = Project.thumbPosition;
				wpt0.imageWidth = photoDescr.Width;
				wpt0.imageHeight = photoDescr.Height;
				wpt0.PhotoTimeShift = new TimeSpan(0L);
				// we need to shift the timestamp a bit if the spot is taken:
				while(WaypointsWithThumbs.ContainsKey(wpt0.DateTime))
				{
					DateTime tmp = wpt0.DateTime;
					wpt0.DateTime = tmp.AddMilliseconds(1);
				}
				// make sure the new waypoint is accounted for in the cache:
				WaypointsWithThumbs.Add(wpt0.DateTime, wpt0);
				CurrentWptIndex = WaypointsWithThumbs.Count - 1;
				WaypointsCache.addWaypoint(wpt0);

				return wpt0;
			}

			if(photoDescr.DTOrig.Equals(DateTime.MinValue))
			{
				// try to get it from file name:
				photoDescr.ensureImageExists();
				Waypoint wpt0 = WaypointsCache.getWaypointByName(photoDescr.imageName);
				if(wpt0 != null)
				{
					//wpt0.Desc = photoDescr.imageName;
					wpt0.ThumbImage = photoDescr.ThumbnailImage;
					wpt0.ThumbSource = photoDescr.imageThumbSource;
					wpt0.ThumbPosition = Project.thumbPosition;
					wpt0.imageWidth = photoDescr.Width;
					wpt0.imageHeight = photoDescr.Height;
					wpt0.PhotoTimeShift = new TimeSpan(0L);
					// we need to shift the timestamp a bit if the spot is taken:
					while(WaypointsWithThumbs.ContainsKey(wpt0.DateTime))
					{
						DateTime tmp = wpt0.DateTime;
						wpt0.DateTime = tmp.AddMilliseconds(1);
					}
					// make sure the new waypoint is accounted for in the cache:
					WaypointsWithThumbs.Add(wpt0.DateTime, wpt0);
					CurrentWptIndex = WaypointsWithThumbs.Count - 1;

					return wpt0;
				}
				else
				{
					registerUnrelatedPhoto(photoDescr);
				}
				return null;
			}

			TimeSpan timeZoneSpan = MyTimeZone.timeSpanById(tzId);
			DateTime timeStamp = Project.localToZulu(photoDescr.DTOrig + photoTimeShift + MyTimeZone.zoneTimeShift - timeZoneSpan);
			//LibSys.StatusBar.Trace("adjusted zulu time=" + timeStamp);
			registerImageTimestamp(timeStamp);

			if(photoDescr.imageSourceIsLocal && photoDescr.image == null)
			{
				// try to get it from file name:
				photoDescr.ensureImageExists();
				registerUnrelatedPhoto(photoDescr);
				return null;
			}
			Waypoint wpt = null;
			Waypoint wpt1;
			Waypoint wpt2;
			// wpt2 is hit if exact match on the timestamp; wpt1 may turn null, but not wpt2.
			if(WaypointsCache.trackpointByTime(timeStamp, out wpt1, out wpt2))
			{
				// create duplicate waypoint and add it to the same track - to hold the image
				wpt = new Waypoint(wpt2);
				if(wpt1 == null)
				{
					// exact match on wpt2
				}
				else
				{
					// somewhere between wpt1 and wpt2
					double dt = (wpt2.DateTime - wpt1.DateTime).Ticks;
					double dt1 = (timeStamp - wpt1.DateTime).Ticks;
					if(dt > 0)
					{
						double ratio =  dt1 / dt;
						wpt.Location.Lat = wpt1.Location.Lat + (wpt2.Location.Lat - wpt1.Location.Lat) * ratio;
						wpt.Location.Lng = wpt1.Location.Lng + (wpt2.Location.Lng - wpt1.Location.Lng) * ratio;
					}
				}
				wpt.DateTime = timeStamp;
				wpt.Desc = photoDescr.imageName;
				Track trk = WaypointsCache.getTrackById(wpt.TrackId);
				// we need to shift the timestamp a bit if the spot is taken:
				while(WaypointsWithThumbs.ContainsKey(wpt.DateTime) || trk.Trackpoints.ContainsKey(wpt.DateTime))
				{
					DateTime tmp = wpt.DateTime;
					wpt.DateTime = tmp.AddMilliseconds(1);
				}
				trk.insertWaypoint(wpt);
				wpt.ThumbImage = photoDescr.ThumbnailImage;
				wpt.ThumbSource = photoDescr.imageThumbSource;
				wpt.ThumbPosition = Project.thumbPosition;
				wpt.imageWidth = photoDescr.Width;
				wpt.imageHeight = photoDescr.Height;
				wpt.PhotoTimeShift = photoTimeShift;
				// make sure the new waypoint is accounted for in the cache:
				WaypointsWithThumbs.Add(wpt.DateTime, wpt);
				CurrentWptIndex = WaypointsWithThumbs.Count - 1;
			}
			else
			{
				registerUnrelatedPhoto(photoDescr);
			}
			return wpt;
		}

		public static void resetThumbPositions()
		{
			for (int i=0; i < WaypointsWithThumbs.Count ;i++)
			{
				Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
				wpt.ThumbPosition = Project.thumbPosition;
			}
		}

		public static void RemoveWaypoint(Waypoint _wpt)
		{
			lock(m_waypointsWithThumbs)
			{
				for (int i=WaypointsWithThumbs.Count-1; i >=0 ;i--)
				{
					Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
					try
					{
						if(wpt == _wpt)
						{
							WaypointsWithThumbs.RemoveAt(i);

							Track trk = WaypointsCache.getTrackById(wpt.TrackId);

							if(trk != null)
							{
								trk.Trackpoints.Remove(wpt.DateTime);
							}

							break;
						}
					} 
					catch
					{
					}
				}
			}
		}

		/// <summary>
		/// is called from WaypointsCache, so that the track membership is already taken care of
		/// </summary>
		/// <param name="source"></param>
		public static void RemoveWaypointsBySource(string source)
		{
			lock(m_waypointsWithThumbs)
			{
				for (int i=WaypointsWithThumbs.Count-1; i >=0 ;i--)
				{
					Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
					try
					{
						if(wpt.Source.Equals(source))
						{
							WaypointsWithThumbs.RemoveAt(i);
						}
					} 
					catch
					{
					}
				}
			}
		}

		/// <summary>
		/// is called from WaypointsCache, so that the track membership is already taken care of
		/// </summary>
		/// <param name="source"></param>
		public static void RemoveWaypointsByTrackId(long trackId)
		{
			lock(m_waypointsWithThumbs)
			{
				for (int i=WaypointsWithThumbs.Count-1; i >=0 ;i--)
				{
					Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
					try
					{
						if(wpt.TrackId == trackId)
						{
							WaypointsWithThumbs.RemoveAt(i);
						}
					} 
					catch
					{
					}
				}
			}
		}

		public static Waypoint getWaypointById(int id)
		{
			Waypoint ret = null;
			lock(m_waypointsWithThumbs)
			{
				for (int i=WaypointsWithThumbs.Count-1; i >=0 ;i--)
				{
					Waypoint wpt = (Waypoint)WaypointsWithThumbs.GetByIndex(i);
					try
					{
						if(wpt.Id == id)
						{
							ret = wpt;
							break;
						}
					} 
					catch
					{
					}
				}
			}
			return ret;
		}

		public static PhotoDescr getUnrelatedById(int id)
		{
			PhotoDescr ret = null;
			lock(m_photosUnrelated)
			{
				for (int i=m_photosUnrelated.Count-1; i >=0 ;i--)
				{
					PhotoDescr pd = (PhotoDescr)m_photosUnrelated.GetByIndex(i);
					try
					{
						if(pd.Id == id)
						{
							ret = pd;
							break;
						}
					} 
					catch
					{
					}
				}
			}
			return ret;
		}

		public static bool RemoveUnrelatedById(int id)
		{
			bool ret = false;
			lock(m_photosUnrelated)
			{
				for (int i=m_photosUnrelated.Count-1; i >=0 ;i--)
				{
					PhotoDescr pd = (PhotoDescr)m_photosUnrelated.GetByIndex(i);
					try
					{
						if(pd.Id == id)
						{
							PhotosUnrelated.Remove(pd.imageSource);
							break;
						}
					} 
					catch
					{
					}
				}
			}
			return ret;
		}

	}
}
