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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.OleDb;
using System.Data;

using LibSys;


namespace LibFormats
{
    /// <summary>
    /// AWOIS gives you shipwrecks data in .PDF (not really printable) or .MDB formats (parsed here).
    /// See AWOIS data at http://www.nauticalcharts.noaa.gov/hsd/AWOIS_download.html
    /// Locally X:\ProjectsData\_Diving\AWOIS
    /// Section 12 is California.
    /// </summary>
    public class FileAwoisMdb : BaseFormat
    {
        public FileAwoisMdb()
		{
		}

        public static string FormatName { get { return "AWOIS MDB .mdb"; } }
        public static string FileExtension { get { return ".mdb"; } }
        public static string FileExtensions { get { return "*.mdb"; } }

        // container for a raw data row from the MDB file:
        internal class SectionRow
        {
            public int RECRD;
            public string VESSLTERMS;
            public string CHART;
            public string AREA;
            public string CARTOCODE;
            public string SNDINGCODE;
            public string DEPTH;
            public string NATIVLAT;
            public string NATIVLON;
            public string LAT83;
            public string LONG83;
            public double LATDEC;
            public double LONDEC;
            public string NATIVDATUM;
            public string CONVERT83;
            public string GPACCURACY;
            public string GPQUALITY;
            public string GPSOURCE;
            public string QUADRANT;
            public string History;
            public string REFERENCE;
            public string YEARSUNK;
        }

        public override bool process(string url, string filename, string source)
        {
            bool ret = true;

            OleDbConnection con = null;

            LibSys.StatusBar.Trace("IP: processing MDB file: " + filename);

            try
            {
                con = new OleDbConnection(@"Provider=Microsoft.JET.OLEDB.4.0;data source=" + filename + "");
                con.Open(); //connection must be opened

                DataTable dt = con.GetSchema("Tables");

                DataRow row = dt.Select("TABLE_TYPE='TABLE'")[0];

                string tableName = row["TABLE_NAME"].ToString();

                OleDbCommand cmd = new OleDbCommand("SELECT * from [" + tableName + "]", con); // creating query command
                OleDbDataReader reader = cmd.ExecuteReader(); // executes query

                int i = 0;
                int errCnt = 0;
                while (reader.Read()) // if can read row from database
                {
                    try
                    {
                        SectionRow sr = new SectionRow()
                        {
                            RECRD = (int)reader.GetValue(0),
                            VESSLTERMS = reader.GetValue(1).ToString(),
                            CHART = reader.GetValue(2).ToString(),
                            AREA = reader.GetValue(3).ToString(),
                            CARTOCODE = reader.GetValue(4).ToString(),
                            SNDINGCODE = reader.GetValue(5).ToString(),
                            DEPTH = reader.GetValue(6).ToString(),
                            NATIVLAT = reader.GetValue(7).ToString(),
                            NATIVLON = reader.GetValue(8).ToString(),
                            LAT83 = reader.GetValue(9).ToString(),
                            LONG83 = reader.GetValue(10).ToString(),
                            LATDEC = (double)reader.GetValue(11),
                            LONDEC = -(double)reader.GetValue(12),
                            NATIVDATUM = reader.GetValue(13).ToString(),
                            CONVERT83 = reader.GetValue(14).ToString(),
                            GPACCURACY = reader.GetValue(15).ToString(),
                            GPQUALITY = reader.GetValue(16).ToString(),
                            GPSOURCE = reader.GetValue(17).ToString(),
                            QUADRANT = reader.GetValue(18).ToString(),
                            History = reader.GetValue(19).ToString(),
                            REFERENCE = reader.GetValue(20).ToString(),
                            YEARSUNK = reader.GetValue(21).ToString()
                        };

                        CreateInfo createInfo = new CreateInfo();	// we will recycle it in place, filling every time.

                        createInfo.init("wpt");
                        createInfo.name = (sr.VESSLTERMS + " " + sr.DEPTH).Trim();
                        createInfo.desc = sr.YEARSUNK;
                        createInfo.lat = sr.LATDEC;
                        createInfo.lng = sr.LONDEC;
                        createInfo.typeExtra = "unknown";	// type: ppl, school, park, locale, airport, reservoir, dam, civil, cemetery, valley, building
                        createInfo.source = source;
                        createInfo.comment = sr.History.Replace(". ", ".\r\n") + ";\r\n" + sr.REFERENCE;

                        m_insertWaypoint(createInfo);
                    }
                    catch (Exception excr)
                    {
                        errCnt++;
                    }

                    i++;
                }
                LibSys.StatusBar.Trace("OK: MDB file: '" + filename + "' processed; found table[" + tableName + "] records/waypoints count=" + i + "  errors count=" + errCnt);
            }
            catch (Exception e)
            {
                LibSys.StatusBar.Error("FileAwoisMdb process() " + e.Message);
                ret = false;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }

            return ret;
        }
    }
}
