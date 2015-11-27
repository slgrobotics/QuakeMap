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
	public class DatedEvent : SortableDateTime
	{
		protected int		m_nValues;
		protected double[]	m_values;
		protected string[]	m_sValues = null;
		protected object[]	m_oValues = null;		// object like Distance or Speed
		protected long[]	m_ids = null;

		// use sValues for properties or types, and ids to get to actual object

		public DatedEvent(DateTime date, double[] values, string[] sValues, long[] ids, int nValues) : base(date)
		{
			m_nValues = nValues;
			m_values = new double[nValues];
			Array.Copy(values, 0, m_values, 0, nValues);
			if(sValues != null) 
			{
				m_sValues = new string[nValues];
				Array.Copy(sValues, 0, m_sValues, 0, nValues);
			} 
			if(ids != null) 
			{
				m_ids = new long[nValues];
				Array.Copy(ids, 0, m_ids, 0, nValues);
			} 
		}

		public double[] values(int format)
		{
			switch(format)
			{
				case Project.FORMAT_EARTHQUAKES_INTENSITY:
				case Project.FORMAT_EARTHQUAKES_STRONGEST:
				case Project.FORMAT_UNKNOWN:
					return m_values;
				case Project.FORMAT_SERVER_AVAILABILITY:
					double[] vv = new double[m_nValues];

					for(int j=0; j < m_nValues ;j++) 
					{
						double val = m_values[j];
						if(val > 28) 
						{
							val = 28;
						}
						vv[j] = (double)j //+ Math.random();
							+ (double)(val)/30.0; //(val == 0 ? 0 : 1);
					}
					return vv;
				case Project.FORMAT_TRACK_ELEVATION:
					break;
			}
			return m_values;
		}

		public string[] sValues()
		{
			return m_sValues;
		}

		public long[] ids()
		{
			return m_ids;
		}

		public int nValues()
		{
			return m_nValues;
		}

		public string toString()
		{
			bool dsUseUtcTime = Project.useUtcTime;
			return "" + (dsUseUtcTime ? m_dateTime : Project.zuluToLocal(m_dateTime)) + "(" + m_nValues + " values)";
		}

		/// <summary>
		/// concatenates values
		/// </summary>
		/// <returns></returns>
		public string toTableString()
		{
			bool dsUseUtcTime = Project.useUtcTime;

			string values = (m_nValues > 1) ? "  values:" : "     ";

			for(int j=0; j < m_nValues ;j++) 
			{
				values += " " + m_values[j];
			}
			DateTime dt = dsUseUtcTime ? m_dateTime : Project.zuluToLocal(m_dateTime);
			return dt.ToLongDateString() + " " + dt.ToLongTimeString() + values;
		}

		/// <summary>
		/// concatenates sValues
		/// </summary>
		/// <returns></returns>
		public string toTableString1()
		{
			string values = "";

			for(int j=0; j < m_nValues ;j++) 
			{
				if(m_sValues != null) 
				{
					values += " " + m_sValues[j];
				}
			}
			return values;
		}

		/// <summary>
		/// concatenates values and sValues
		/// </summary>
		/// <returns></returns>
		public string toTableString2()
		{
			string values = "  values:";

			for(int j=0; j < m_nValues ;j++) 
			{
				values += " " + m_values[j];
				if(m_sValues != null) 
				{
					values += " " + m_sValues[j];
				}
			}
			return "" + m_dateTime + values;
		}

		public bool Equals(DatedEvent de)
		{
			return de.toString().Equals(this.toString());
		}

	}
}
