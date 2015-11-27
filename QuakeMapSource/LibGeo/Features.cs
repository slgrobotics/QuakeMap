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
using System.Xml;
using System.IO;
using LibSys;

namespace LibGeo
{
	public class Features : MyCacheable
	{
		protected ArrayList m_list = new ArrayList();
		public int Count { get { return m_list.Count; } }
		public ArrayList List { get { return m_list; } }
		protected bool m_hasLoaded = false;
		public bool HasLoaded { get { return m_hasLoaded; } set { m_hasLoaded = value; } }
		protected bool m_hasPutOnMap = false;
		public bool HasPutOnMap { get { return m_hasPutOnMap; } set { m_hasPutOnMap = value; } }

		public void Add(LiveObject lo) {
			m_list.Add(lo);
			if(lo.GetType().Equals(City.city.GetType())) { citiesCount++; } 
			else if (lo.GetType().Equals(Place.place.GetType())) { namesCount++; }			
		}

		protected bool checkForDuplicates = true;
		protected int m_lineno;
		protected string m_fileName;
		protected int citiesCount = 0;
		protected int namesCount = 0;

		public Features(string fileName, string baseName, bool doFill) : base(baseName)
		{
			m_fileName = fileName;
			if(doFill)
			{
				if(m_fileName != null) 
				{
					Fill();
				} 
				else 
				{
					IsEmpty = true;
				}
			}
		}

		public override string ToString()
		{
			return "Features: " + m_baseName + " " + citiesCount + "/" + namesCount
							+ " used " + (new DateTime(LastUsed)) + " usage " + m_usageCounter + " IsEmpty=" + IsEmpty + " HasLoaded=" + HasLoaded;
		}

		// the Dispose will be called before removing from cache, so release anything that holds files or memory
		public override void Dispose()
		{
			m_list.Clear();
			m_hasLoaded = false;
			m_hasPutOnMap = false;
		}

		public void Fill()
		{
			m_hasPutOnMap = false;

			XmlDocument xmlDoc = new XmlDocument();

			try 
			{
				// let's hope it is good XML and we can load it the easy way:
				xmlDoc.Load(m_fileName);

				// we want to traverse XmlDocument fast, as tile load operations can be numerous
				// and come in pack. So we avoid using XPath and rely mostly on "foreach child":
				foreach(XmlNode nnode in xmlDoc.ChildNodes) 
				{
					if(nnode.Name.Equals("features")) 
					{
						foreach(XmlNode node in nnode)
						{
							try 
							{
								string type = node.Name;
								if(type.Equals("name")) 
								{
									Place place = new Place(node);
									if(place.Importance > 0 && !place.Name.Equals("NAME NOT KNOWN")) {
										// if(checkForDuplicates) {
										bool found = false;
										foreach(LiveObject lo in m_list) 
										{
											if(lo.GetType().Equals(place.GetType())) 
											{
												Place p = (Place)lo;
												if (p.sameAs(place)) 
												{
													found = true;
													//LibSys.StatusBar.Trace("duplicate PLACE: " + place + " to " + p);
													break;
												}
											}
										}
										if(!found) 
										{
											m_list.Add(place);
											namesCount++;
										}
										// } else {	// checkForDuplicates
										//     m_list.Add(place);
										//	   namesCount++;
										// }
									}
								}
								else if(type.Equals("ppl")) 
								{
									City city = new City(node);
									if(checkForDuplicates) 
									{
										bool found = false;
										foreach(LiveObject lo in m_list) 
										{
											if(lo.GetType().Equals(city.GetType())) 
											{
												City c = (City)lo;
												if (c.sameAs(city)) 
												{
													found = true;
													break;
												}
											}
										}
										if(!found) 
										{
											m_list.Add(city);
											citiesCount++;
										}
									} 
									else 
									{
										m_list.Add(city);
										citiesCount++;
									}
								}
							} 
							catch (Exception ee) 
							{
								// bad node, no big deal. Just continue.
								LibSys.StatusBar.Error("Features:Fill() bad node: " + ee.Message);
							}
						}
					}
				}
			} 
			catch (Exception e) 
			{
				// bad XML. Try to load it the old way.
#if DEBUG
				LibSys.StatusBar.Trace("Warning: Features:Fill(): " + m_fileName + " - bad XML. Loading old style");
#endif
				StreamReader stream = null;
				try 
				{
					stream = new StreamReader(m_fileName);
					bool isXml = false;
					int m_lineno = 0;
					string str;

					string name = null;
					string sLat = null;
					string sLong = null;
					GeoCoord loc = null;
					string country = null;
					string state = null;
					string county = null;
					int population = 0;
					int importance = 0;
					string dsg = null;
					bool doCleanup = false;
					bool inPpl = false;
					bool inName = false;

					while((str = stream.ReadLine()) != null) 
					{
						m_lineno++;
						if(m_lineno < 3 && str.StartsWith("<?xml"))
						{
							isXml = true;
						}

						if(isXml) 
						{
							// this data formatted in XML comes from NIMA populated_places, converted through a database:
							try	
							{
								int slen = str.Length;
								if(str.StartsWith("<ppl>")) 
								{
									inPpl = true;
									doCleanup = true;
								} 
								else if(str.StartsWith("<name>")) 
								{
									inName = true;
									doCleanup = true;
								}
								if(doCleanup) 
								{
									name = null;
									sLat = null;
									sLong = null;
									loc = null;
									country = null;
									state = null;
									county = null;
									population = 0;
									importance = 0;
									dsg = null;
									doCleanup = false;
									continue;
								}
								if(inPpl) 
								{
									if(str.StartsWith("</ppl>")) 
									{
										if(name != null) 
										{
											// this is the last token, we have all info to list the city:
											City city = new City(name, loc, country, state, county, population, importance, dsg);
											//LibSys.StatusBar.Trace("" + m_lineno + " CITY: " + city.toTableString());
											if(checkForDuplicates) 
											{
												bool found = false;
												foreach(LiveObject lo in m_list) 
												{
													if(lo.GetType().Equals(city.GetType())) 
													{
														City c = (City)lo;
														if (c.sameAs(city)) 
														{
															found = true;
															break;
														}
													}
												}
												if(!found) 
												{
													m_list.Add(city);
													citiesCount++;
												}
											} 
											else 
											{
												m_list.Add(city);
												citiesCount++;
											}
										}
										inPpl = false;
										continue;
									}
									if(str.StartsWith("<p4>")) 
									{
										sLat = str.Substring(4, slen-9);
									} 
									else if(str.StartsWith("<p5>")) 
									{
										sLong = str.Substring(4, slen-9);
										double latitude = Convert.ToDouble(sLat);
										double longitude = Convert.ToDouble(sLong);
										loc = new GeoCoord(longitude, latitude);
									} 
									else if(str.StartsWith("<p12>")) 
									{
										string tmpDsg = str.Substring(5, slen-11);
										if(tmpDsg != null && (tmpDsg.Equals("C") || tmpDsg.Equals("A"))) 
										{
											dsg = tmpDsg;
										}
									} 
									else if(str.StartsWith("<p13>")) 
									{
										importance = Convert.ToInt32(str.Substring(5, slen-11));
									} 
									else if(str.StartsWith("<p14>")) 
									{
										country = str.Substring(5, slen-11);
									} 
									else if(str.StartsWith("<p15>")) 
									{
										state = str.Substring(5, slen-11);
									} 
									else if(str.StartsWith("<p16>")) 
									{
										county = str.Substring(5, slen-11);
									} 
									else if(str.StartsWith("<p17>")) 
									{
										int ppos = str.IndexOf("</p17>");
										population = Convert.ToInt32(str.Substring(5, ppos-5));
									} 
									else if(str.StartsWith("<p25>")) 
									{
										name = str.Substring(5, slen-11);
									}
								}
								if(inName) 
								{
									if(str.StartsWith("</name>")) 
									{
										if(name != null && dsg != null && loc != null) 
										{
											// this is the last token, we have all info to list the place:
                                
											// adjust importance here:
											if(dsg.Equals("SEA") || dsg.Equals("OCN")) 
											{    // seas and oceans
												importance = 1;
												// } else if(dsg.Equals("ADM1")) {    //
												//     importance = 2;
											} 
											else if(dsg.Equals("ISLS")|| dsg.Equals("ADM1")) 
											{    //
												importance = 3;
												// } else if() {    //
												//     importance = 4;
											} 
											else if(dsg.Equals("ISL") || dsg.Equals("PEN")
												// || dsg.Equals("ADM4") || dsg.Equals("ADM3") || dsg.Equals("ADM2")
												|| dsg.Equals("LK") || dsg.Equals("LKS")) 
											{    //
												importance = 5;
											}
											if(importance > 0 && !name.Equals("NAME NOT KNOWN")) 
											{
												Place place = new Place(name, loc, importance, dsg);
												//LibSys.StatusBar.Trace("" + m_lineno + " PLACE: " + place.toTableString());
												// if(checkForDuplicates) {
												bool found = false;
												foreach(LiveObject lo in m_list) 
												{
													if(lo.GetType().Equals(place.GetType())) 
													{
														Place p = (Place)lo;
														if (p.sameAs(place)) 
														{
															found = true;
															// LibSys.StatusBar.Trace("duplicate PLACE: " + place + " to " + p);
															break;
														}
													}
												}
												if(!found) 
												{
													m_list.Add(place);
													namesCount++;
												}
												// } else {	// checkForDuplicates
												//     m_list.Add(place);
												//	   namesCount++;
												// }
											}
											//else 
											//{
											//	LibSys.StatusBar.Trace("" + m_lineno + " ignored PLACE: " + name + " " + loc + " dsg=" + dsg + " imp=" + importance);
											//}
										}
										inName = false;
										continue;
									}
									if(str.StartsWith("<p4>")) 
									{
										sLat = str.Substring(4, slen-9);
										//LibSys.StatusBar.Trace("line " + m_lineno + " of " + m_fileName + " sLat=" + sLat);
									} 
									else if(str.StartsWith("<p5>")) 
									{
										sLong = str.Substring(4, slen-9);
										//LibSys.StatusBar.Trace("line " + m_lineno + " of " + m_fileName + " sLong=" + sLong);
										double latitude = Convert.ToDouble(sLat);
										double longitude = Convert.ToDouble(sLong);
										loc = new GeoCoord(longitude, latitude);
									} 
									else if(str.StartsWith("<p12>")) 
									{
										dsg = str.Substring(5, slen-11);
									} 
									else if(str.StartsWith("<p13>")) 
									{
										importance = Convert.ToInt32(str.Substring(5));
									} 
									else if(str.StartsWith("<p25>")) 
									{
										name = str.Substring(5, slen-11);
									}
								}
							} 
							catch (Exception eee) 
							{
								LibSys.StatusBar.Trace("line " + m_lineno + " of " + m_fileName + " " + eee.Message);
								//is.close();
							}
						}
						else 
						{
							try 
							{
								City city = parseCityString(str);
								// LibSys.StatusBar.Trace("CITY: " + city.toTableString());
								if(checkForDuplicates) 
								{
									bool found = false;
									foreach(LiveObject lo in m_list) 
									{
										if(lo.GetType().Equals(city.GetType())) 
										{
											City c = (City)lo;
											if (c.sameAs(city)) 
											{
												found = true;
												break;
											}
										}
									}
									if(!found) 
									{
										m_list.Add(city);
										citiesCount++;
									}
								} 
								else 
								{
									m_list.Add(city);
									citiesCount++;
								}
							}
							catch (Exception eee) 
							{
								LibSys.StatusBar.Trace("line " + m_lineno + " of " + m_fileName + " " + eee.Message);
							}
						}
					}
				} 
				catch (Exception ee) 
				{
					LibSys.StatusBar.Error("Features:Fill(): " + ee.Message);
					IsEmpty = true;
				}
				finally
				{
					if(stream != null)
					{
						stream.Close();
					}
				}
			}

			m_hasLoaded = true;

#if DEBUG
			LibSys.StatusBar.Trace("OK: Features:Fill(): " + m_baseName + " cities=" + citiesCount + " names=" + namesCount);
#endif
		}

		public void PutOnMap(IDrawingSurface map, ITile iTile, IObjectsLayoutManager tile)
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: Features:PutOnMap()  " + m_list.Count + "  " + tile);
#endif

			// here is a slow and ineffective method to restore all live objects to original state.
			// it does not work on substitutes though:
			// m_list.Clear();
			// this.Fill();

			// if lo.init() works well, the loop below will do the job of Clear/Fill:
			foreach(LiveObject lo in m_list)
			{
				try 
				{
					lo.init(true);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("Features:PutOnMap(): lo=" + lo + " " + e.Message);
				}
			}
			foreach(LiveObject lo in m_list)
			{
				try 
				{
					lo.PutOnMap(map, iTile, tile);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("Features:PutOnMap(): lo=" + lo + " " + e.Message);
				}
			}
			foreach(LiveObject lo in m_list)
			{
				try 
				{
					lo.AdjustPlacement(map, iTile, tile);
				} 
				catch(Exception e) 
				{
					LibSys.StatusBar.Error("Features:PutOnMap() - AdjustPlacement: lo=" + lo + " " + e.Message);
				}
			}
			m_hasPutOnMap = true;
		}
		
		public City parseCityString(string str)
		{
			//System.Console.WriteLine(str);
			string name = str.Substring(0,49).Trim();
			//System.Console.WriteLine("\"" + name + "\"");
			string country = "US";
			//System.Console.WriteLine("\"" + country + "\"");
			string state = str.Substring(54,3).Trim();
			//System.Console.WriteLine("\"" + state + "\"");
			string county = str.Substring(58,19).Trim();
			//System.Console.WriteLine("\"" + county + "\"");
			string latDir = str.Substring(83,1);
			//System.Console.WriteLine("\"" + latDir + "\"");
			string longDir = str.Substring(93,1);
			//System.Console.WriteLine("\"" + longDir + "\"");
			int importance = 0;
			//System.Console.WriteLine("\"" + importance + "\"");
			string dsg = str.Substring(52,1).Trim();;
			//System.Console.WriteLine("\"" + dsg + "\"");
            
			double[] a = new double[8];
			try 
			{
				a[0] = Convert.ToDouble(str.Substring(77,2));
				a[1] = Convert.ToDouble(str.Substring(79,2));
				a[2] = Convert.ToDouble(str.Substring(81,2));
				a[3] = Convert.ToDouble(str.Substring(86,3));
				a[4] = Convert.ToDouble(str.Substring(89,2));
				a[5] = Convert.ToDouble(str.Substring(91,2));
				a[6] = Convert.ToDouble(str.Substring(141).Replace(' ','0'));
			} 
			catch (Exception ee) 
			{
				LibSys.StatusBar.Trace("line " + m_lineno + " of " + m_fileName + ee.Message);
			}
            
			/*
			int i;
			for(i=0; i < a.length ;i++)
				System.Console.WriteLine("" + i + "  " + a[i]);
			*/
            
			GeoCoord loc;
			if(Project.EqualsIgnoreCase("W", longDir) && Project.EqualsIgnoreCase("N", latDir)) 
			{
				loc = new GeoCoord(-a[3], -a[4], -a[5],	a[0], a[1], a[2]);
			} 
			else if(Project.EqualsIgnoreCase("W", longDir) && Project.EqualsIgnoreCase("S", latDir)) 
			{
				loc = new GeoCoord(-a[3], -a[4], -a[5],	-a[0], -a[1], -a[2]);
			} 
			else if(Project.EqualsIgnoreCase("E", longDir) && Project.EqualsIgnoreCase("S", latDir)) 
			{
				loc = new GeoCoord(a[3], a[4], a[5], -a[0], -a[1], -a[2]);
			} 
			else 
			{  // "W" "S"
				loc = new GeoCoord(a[3], a[4], a[5], a[0], a[1], a[2]);
			}
			City city = new City(name, loc, country, state, county, (int)a[6], importance, dsg);
			//if(name.StartsWith("Santa"))
			//{
			//	LibSys.StatusBar.Trace("line " + m_lineno + " of " + m_fileName + "  " + city.ToString());
			//}
			return city;
		}

	}
}
