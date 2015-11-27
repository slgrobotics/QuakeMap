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

namespace LibGeo
{
	/// <summary>
	/// Summary description for Distance.
	/// </summary>
	public class Distance : IComparable
	{

		public const int UNITS_DISTANCE_KM		= 0;
		public const int UNITS_DISTANCE_M		= 1;
		public const int UNITS_DISTANCE_MILES	= 2;		// statute miles
		public const int UNITS_DISTANCE_NMILES	= 3;		// nautical miles
		public const int UNITS_DISTANCE_FEET	= 4;

		public static string[] unitNames = 	{
												"kilometers",
												"meters",
												"miles",
												"n.miles",
												"feet"
											};

		// set units every time you want to display miles or km:
		//public static int units = Project.UNITS_DISTANCE_DEFAULT;
	
		protected double m_distance;	// meters

		public double Meters { get { return m_distance; } set { m_distance = value; } }

		protected bool roundToInts = true;

		// Some constants:
		public const double METERS_PER_MILE = 1609.344d;
		public const double METERS_PER_NAUTICAL_MILE = 1852.0d;
		public const double METERS_PER_FOOT = 0.3048d;
		public const double METERS_PER_DEGREE = 40075700.0d / 360.0d;		// at equator


		public Distance(double d /* meters */)
		{
			m_distance = d;
		}

		public Distance(Distance dist)
		{
			m_distance = dist.Meters;
		}

		public Distance(double d /* natural units */, bool b)	// b unused
		{
			switch(Project.unitsDistance) 
			{ 
				case UNITS_DISTANCE_M:
					m_distance = d;
					break;
				case UNITS_DISTANCE_KM:
					m_distance = d * 1000;
					break;
				default:
				case UNITS_DISTANCE_MILES:
					m_distance = d * METERS_PER_MILE;
					break;
				case UNITS_DISTANCE_NMILES:
					m_distance = d * METERS_PER_NAUTICAL_MILE;
					break;
				case UNITS_DISTANCE_FEET:
					m_distance = d * METERS_PER_FOOT;
					break;
			}
		}

		public void byUnits(double d /* natural units */, int unitsDistance)
		{
			switch(unitsDistance) 
			{ 
				case UNITS_DISTANCE_M:
					m_distance = d;
					break;
				case UNITS_DISTANCE_KM:
					m_distance = d * 1000;
					break;
				default:
				case UNITS_DISTANCE_MILES:
					m_distance = d * METERS_PER_MILE;
					break;
				case UNITS_DISTANCE_NMILES:
					m_distance = d * METERS_PER_NAUTICAL_MILE;
					break;
				case UNITS_DISTANCE_FEET:
					m_distance = d * METERS_PER_FOOT;
					break;
			}
		}

		public int Units	// current main units
		{
			get 
			{
				return Project.unitsDistance;
			}
		}

		public int UnitsCompl	// complementary units, feet for miles, meters for km
		{
			get 
			{
				switch(Project.unitsDistance) 
				{ 
					case UNITS_DISTANCE_KM:
						return UNITS_DISTANCE_M;
					case UNITS_DISTANCE_MILES:
					case UNITS_DISTANCE_NMILES:
						return UNITS_DISTANCE_FEET;
				}
				return Project.unitsDistance;
			}
		}

		#region ToString() and correspondent ToDouble()

		public override string ToString()
		{
			return ToString(Project.unitsDistance);
		}

		public double ToDouble()
		{
			return ToDouble(Project.unitsDistance);
		}

		public string ToStringCompl()	// complementary units, feet for miles, meters for km
		{
			switch(Project.unitsDistance) 
			{ 
				case UNITS_DISTANCE_KM:
					return ToString(UNITS_DISTANCE_M);
				case UNITS_DISTANCE_MILES:
				case UNITS_DISTANCE_NMILES:
					return ToString(UNITS_DISTANCE_FEET);
			}
			return ToString();
		}

		public double ToDoubleCompl()	// complementary units, feet for miles, meters for km
		{
			switch(Project.unitsDistance) 
			{ 
				case UNITS_DISTANCE_KM:
					return ToDouble(UNITS_DISTANCE_M);
				case UNITS_DISTANCE_MILES:
				case UNITS_DISTANCE_NMILES:
					return ToDouble(UNITS_DISTANCE_FEET);
			}
			return ToDouble();
		}

		public virtual string ToString(int un)
		{
			switch(un)
			{ 
				case UNITS_DISTANCE_KM:
					if(m_distance < 1000.0d)
					{
						return ToString(UNITS_DISTANCE_M);
					}
					break;
				case UNITS_DISTANCE_MILES:
				case UNITS_DISTANCE_NMILES:
					if(m_distance < 300.0d)
					{
						return ToString(UNITS_DISTANCE_FEET);
					}
					break;
			}
			return toStringN(un) + " " + toStringU(un);
		}

		public virtual double ToDouble(int un)
		{
			switch(un) 
			{ 
				case UNITS_DISTANCE_M:
					double dm = roundToInts ? Math.Round(m_distance) : Math.Round(m_distance, 1);
					return dm;

				case UNITS_DISTANCE_KM:
					double dkm = m_distance/1000.0d;
					if(dkm < 1.0d)
					{
						// round it to 1 m:
						dkm = Math.Round(dkm, 3);
					}
					else if(dkm < 10.0d)
					{
						// round it to 0.1 km:
						dkm = Math.Round(dkm, 1);
					}
					else
					{
						// round it to 1 km:
						dkm = Math.Round(dkm);
					}
					return dkm;

				default:
				case UNITS_DISTANCE_MILES:
					double dml = m_distance/METERS_PER_MILE;
					if(dml < 1.0d)
					{
						// round it to 0.01 m:
						dml = Math.Round(dml, 2);
					}
					else if(dml < 10.0d)
					{
						// round it to 0.1 mi:
						dml = Math.Round(dml, 1);
					}
					else
					{
						// round it to 1 mile:
						dml = Math.Round(dml);
					}
					return dml;

				case UNITS_DISTANCE_NMILES:
					double dnml = m_distance/METERS_PER_NAUTICAL_MILE;
					if(dnml < 1.0d)
					{
						// round it to 0.01 m:
						dnml = Math.Round(dnml, 2);
					}
					else if(dnml < 10.0d)
					{
						// round it to 0.1 mi:
						dnml = Math.Round(dnml, 1);
					}
					else
					{
						// round it to 1 mile:
						dnml = Math.Round(dnml);
					}
					return dnml;

				case UNITS_DISTANCE_FEET:
					double dp = m_distance/METERS_PER_FOOT;
					// round it to 1 ft:
					dp = roundToInts ? Math.Round(dp) : Math.Round(dp, 1);
					return dp;
			}
		}

		// does not display units:
		public string toStringN(int un)
		{
			switch(un) 
			{ 
				case UNITS_DISTANCE_M:
					double dm = roundToInts ? Math.Round(m_distance) : Math.Round(m_distance, 1);
					return formatWithCommas(dm);

				case UNITS_DISTANCE_KM:
					double dkm = m_distance/1000.0d;
					if(dkm < 1.0d)
					{
						// round it to 1 m:
						dkm = Math.Round(dkm, 3);
					}
					else if(dkm < 10.0d)
					{
						// round it to 0.1 km:
						dkm = Math.Round(dkm, 1);
						return string.Format("{0:F1}", dkm);
					}
					else
					{
						// round it to 1 km:
						dkm = Math.Round(dkm);
					}
					return formatWithCommas(dkm);

				default:
				case UNITS_DISTANCE_MILES:
					double dml = m_distance/METERS_PER_MILE;
					if(dml < 1.0d)
					{
						// round it to 0.01 m:
						dml = Math.Round(dml, 2);
					}
					else if(dml < 10.0d)
					{
						// round it to 0.1 mi:
						dml = Math.Round(dml, 1);
						return string.Format("{0:F1}", dml);
					}
					else
					{
						// round it to 1 mile:
						dml = Math.Round(dml);
					}
					return formatWithCommas(dml);

				case UNITS_DISTANCE_NMILES:
					double dnml = m_distance/METERS_PER_NAUTICAL_MILE;
					if(dnml < 1.0d)
					{
						// round it to 0.01 m:
						dnml = Math.Round(dnml, 2);
					}
					else if(dnml < 10.0d)
					{
						// round it to 0.1 mi:
						dnml = Math.Round(dnml, 1);
						return string.Format("{0:F1}", dnml);
					}
					else
					{
						// round it to 1 mile:
						dnml = Math.Round(dnml);
					}
					return formatWithCommas(dnml);

				case UNITS_DISTANCE_FEET:
					double dp = m_distance/METERS_PER_FOOT;
					// round it to 1 ft:
					dp = roundToInts ? Math.Round(dp) : Math.Round(dp, 1);
					return formatWithCommas(dp);
			}
		}

		// units only:
		public virtual string toStringU(int un)
		{
			switch(un) 
			{ 
				case UNITS_DISTANCE_M:
					return "m";
				case UNITS_DISTANCE_KM:
					return "km";
				default:
				case UNITS_DISTANCE_MILES:
					return "mi";
				case UNITS_DISTANCE_NMILES:
					return "nm";
				case UNITS_DISTANCE_FEET:
					return "ft";
			}
		}

		// same as ToString(), but without a space between number and units:
		public string ToString2()
		{
			return toStringN(Project.unitsDistance) + toStringU(Project.unitsDistance);
		}

		private string formatWithCommas(double dd)
		{
			string ret = String.Format("{0:n}", dd);

			ret = ret.Replace(".00","");

			return ret;
		}

		#endregion // ToString() and correspondent ToDouble()

		public int CompareTo(object other)
		{
			return (int)(Meters - ((Distance)other).Meters);
		}
	}
}
