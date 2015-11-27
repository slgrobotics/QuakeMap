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
using System.Net;
using System.IO;
using System.Windows.Forms;
using LibGeo;
using LibSys;

namespace LibNet
{
	/// <summary>
	/// Summary description for ZipcodeServer.
	/// </summary>
	public class ZipcodeServer
	{
		private string m_url;

		public ZipcodeServer(string url)
		{
			m_url = url;
		}

		public static City parseCityString(string cityStr)
		{
			if(!cityStr.StartsWith("REC:")) 
			{
				return null;
			}

			cityStr = cityStr.Substring(4);
        
			String name = "";
			String country = "US";
			String state = "";
			String county = "";
			String latt = "";
			String longt = "";
			int importance = 0;
			String dsg = null;

			char[] splitter = { '|' };
			string[] st = cityStr.Split(splitter);
			int i = 0;
			foreach(string str in st)  
			{
				if(!str.StartsWith("//")) 
				{   // we can comment out lines in the box using //
					System.Console.WriteLine("ZIP info=" + str);
					switch(i) 
					{
						case 0:
							county = "zipcode " + str;
							break;
						case 1:
							name = str;
							break;
						case 2:
							state = str;
							break;
						case 3:
							longt = str;
							break;
						case 4:
							latt = str;
							break;
						default:
							break;
					}
				}
				i++;
			}

			/*
			System.Console.WriteLine("name=\"" + name + "\"");
			System.Console.WriteLine("country=\"" + country + "\"");
			System.Console.WriteLine("state=\"" + state + "\"");
			System.Console.WriteLine("county=\"" + county + "\"");
			System.Console.WriteLine("latt=\"" + latt + "\"");
			System.Console.WriteLine("longt=\"" + longt + "\"");
			System.Console.WriteLine("importance=\"" + importance + "\"");
			System.Console.WriteLine("dsg=\"" + dsg + "\"");
			 */
        
			double dlat = Convert.ToDouble(latt);
			double dlong = Convert.ToDouble(longt);

			GeoCoord loc = new GeoCoord(dlong, dlat);
			//System.Console.WriteLine(name + "  " + loc.toString() + " pop=" + (int)a[6]);
			return new City(name, loc, country, state, county, 0, importance, dsg);
		}

		protected static ArrayList citiesList = new ArrayList();
		protected static ArrayList list = new ArrayList();

		public GeoCoord getCoord(ListBox placesListBox)
		{
			GeoCoord ret = null;

			try 
			{
				int index = placesListBox.SelectedIndex;

				City city = (City)citiesList[index];
				
				ret = city.Location.Clone();
				ret.Elev = Project.CAMERA_HEIGHT_SAFE * 1000.0d;
			} 
			catch (Exception e) 
			{
			}
			return ret;
		}
		
		public void getPlaces(ListBox placesListBox, Label messageLabel, string keyword)
		{
			try 
			{
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(m_url + "?intext=" + keyword);
				req.KeepAlive = false;
				WebResponse res = req.GetResponse();
				Stream responseStream = res.GetResponseStream();
				StreamReader reader = new StreamReader (responseStream);
				citiesList.Clear();
				list.Clear();
				string line;
				while((line=reader.ReadLine()) != null) 
				{
					//LibSys.StatusBar.Trace(line);
					City city=parseCityString(line);
					if(city != null) 
					{
						citiesList.Add(city);
						list.Add(city.toTableString());
						//LibSys.StatusBar.Trace(city.toTableString());
						placesListBox.Items.Add(city.toTableString());
					}
				}
				responseStream.Close();
				if(list.Count > 0)
				{
					placesListBox.SetSelected(0, true);
					messageLabel.Text = "select city from the list below and click this button -------->";
				} 
				else 
				{
					messageLabel.Text = "no cities or US zipcode matched keyword '" + Project.findKeyword + "'";
				}
			} 
			catch (Exception e)
			{
				messageLabel.Text = "" + e.Message;
				//LibSys.StatusBar.Error("ZipcodeServer() - " + e);
			}
		}

		public override string ToString()
		{
			return "ZipcodeServer[" + m_url + "]"; 
		}
	}
}
