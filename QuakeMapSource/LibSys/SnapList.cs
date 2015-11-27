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

namespace LibSys
{
	/// <summary>
	/// SnapList allows double values to be snapped to a grid. See TileSetTerra for example usage.
	/// </summary>
	public class SnapList 
	{
		public ArrayList snaps = new ArrayList();

		private ArrayList temp = new ArrayList();
		private bool snapping = false;	// initially in acquisition mode
		private bool m_doSpread;		// will make even intervals between snap points
		private double snapWindow;

		public SnapList(double perTile, bool doSpread)
		{
			snapWindow = perTile / 2.5d;
			m_doSpread = doSpread;
		}

		public void Add(double snapValue)
		{
			if(snapValue != 0.0d && !Double.IsNaN(snapValue) && temp.Count < 1000)
			{
				temp.Add(snapValue);
			}
#if DEBUG
			else
			{
				LibSys.StatusBar.Error("SnapList:Add(" + snapValue + ") - at count=" + temp.Count);
			}
#endif
		}

		private bool canBeSnapped(double d)
		{
			foreach(double snap in snaps)
			{
				if(d > snap - snapWindow && d < snap + snapWindow)
				{
					return true;
				}
			}
			return false;
		}

		public double snap(double snapValue)
		{
			if(!snapping)
			{
				snapping = true;
				if(temp.Count == 0)
				{
					// something wrong, just do not snap at all.
					return snapValue;
				}

				// calculate snap array - the first time we enter here
				while(true)
				{
					double cur = (double)temp[0];
					bool foundSnapPointCandidate = false;

					foreach(double d in temp)
					{
						if(!canBeSnapped(d))
						{
							cur = d;
							foundSnapPointCandidate = true;
							break;
						}
					}

					if(!foundSnapPointCandidate)
					{
						break;	// no more candidate points, done building snap grid
					}

					int count = 0;
					double acc = 0;

					// compute snapping array:
					foreach(double d in temp)
					{
						if(d > cur - snapWindow && d < cur + snapWindow)
						{
							acc += d;
							count++;
						}
					}
					acc /= count;
					//LibSys.StatusBar.Trace("SNAP: " + acc + " (count=" + count + " of " + temp.Count + ")");
					snaps.Add(acc);
				}
				temp.Clear();	// no need to keep it around

				if(m_doSpread && snaps.Count > 2)
				{
					// spread snap points evenly:
					SortedList ttemp = new SortedList();
                    foreach(double d in snaps)
					{
						ttemp.Add(d, d);
					}
					double minVal = (double)ttemp.GetByIndex(0);
					double maxVal = (double)ttemp.GetByIndex(ttemp.Count - 1);
					double step = (maxVal - minVal) / (ttemp.Count - 1);		// evaluate the step
					double falsePointCount = 0;

					// weed out points that don't have interval close to evaluated step: 
					for(int i=1; i < ttemp.Count ;i++)
					{
						double d1 = (double)ttemp.GetByIndex(i-1);
						double d2 = (double)ttemp.GetByIndex(i);
						if(d2 - d1 < step * 0.3d)		// 0.7 seems to work fine too
						{
							falsePointCount++;
						}
					}

					// rebuild even-spread snaps array:
					snaps.Clear();
					step = (maxVal - minVal) / (ttemp.Count - falsePointCount - 1);		// now calculate the step
					double curVal = minVal;
					for(int i=0; i < ttemp.Count - falsePointCount ;i++)
					{
						snaps.Add(curVal);
						//LibSys.StatusBar.Trace("SPREAD: " + curVal);
						curVal += step;
					}
				}
			}

			foreach(double snap in snaps)
			{
				if(snapValue > snap - snapWindow && snapValue < snap + snapWindow)
				{
					return snap;
				}
			}
			return snapValue;
		}
	}
}
