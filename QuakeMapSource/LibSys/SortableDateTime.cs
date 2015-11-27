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
	public enum DateTimeDisplayMode
	{
		MayConvertToUTC,
		ConvertToLocal,
		DisplayAsIs
	}

	/// <summary>
	/// SortableDateTime sorts in the columns like DateTime and allows flexibility in ToString() output
	/// </summary>
	public class SortableDateTime : IComparable
	{
		protected DateTime m_dateTime;		// always UTC for earthquakes, trackpoints
		private DateTimeDisplayMode m_dateTimeDisplayMode = DateTimeDisplayMode.MayConvertToUTC;
		public DateTime DateTime { get { return m_dateTime; } set { m_dateTime = value; } }

		public static DateTime tooOld = DateTime.MinValue.AddDays(5);

		public SortableDateTime()		{}

		public SortableDateTime(DateTime dateTime)
		{
			m_dateTime = dateTime;
		}

		public SortableDateTime(DateTime dateTime, DateTimeDisplayMode dateTimeDisplayMode)
		{
			m_dateTime = dateTime;
			m_dateTimeDisplayMode = dateTimeDisplayMode;
		}

		public override string ToString()
		{
			if(m_dateTime.CompareTo(tooOld) < 0)
			{
				return "";
			}
			else
			{
				string sTime;
				switch(m_dateTimeDisplayMode)
				{
					default:
					case DateTimeDisplayMode.MayConvertToUTC:
						if(Project.useUtcTime)
						{
							string format = "yyyy MMM dd HH:mm:ss";
							sTime = m_dateTime.ToString(format) + " UTC";
						}
						else
						{
							sTime = m_dateTime.ToString();	// current culture
						}
						break;

					case DateTimeDisplayMode.ConvertToLocal:
						sTime = Project.zuluToLocal(m_dateTime).ToString();		// trackpoints store UTC, print local time.
						break;

					case DateTimeDisplayMode.DisplayAsIs:
						sTime = m_dateTime.ToString();	// current culture
						break;
				}
				return sTime;
			}
		}

		public int CompareTo(object other)
		{
			return m_dateTime.CompareTo(((SortableDateTime)other).m_dateTime);
		}
	}
}