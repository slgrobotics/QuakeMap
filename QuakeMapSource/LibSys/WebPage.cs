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
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.IO;

namespace LibSys
{
	/// <summary>
	/// WebPage holds parameters sufficient for generating a web page HTML.
	/// some of the parameters persist in template folder, while others are loaded dynamically into the dictionary.
	/// </summary>
	public class WebPage
	{
		// main image size:
		public int imageWidth = 0;
		public int imageHeight = 0;

		// some special parameters that are expected to be set directly, but also can be left out:
		public string shortName { set { ToDictionary("shortName", value); } get {return FromDictionary("shortName"); } }
		public string heading { set { ToDictionary("heading", value); } get {return FromDictionary("heading"); } }
		public string description { set { ToDictionary("description", value); } get {return FromDictionary("description"); } }

		public ImageFormat imageFormat = ImageFormat.Jpeg;
		public string imageFormatName = "JPEG";
		public string imageFileExt = ".jpg";

		private string m_templateName;
		private string m_templateFolder;
		private string m_commonFolder;

		private Hashtable m_dictionary = new Hashtable();

		public WebPage(string templateName)
		{
			m_templateName = templateName;
			m_templateFolder = Project.GetWebPageTemplateFolder(templateName);
			m_commonFolder = Path.Combine(Project.GetWebPageTemplatesFolderBase(), "_common");
		}

		public void init(Hashtable commonDictionary, Hashtable templateDictionary)
		{
			// at this point there may be something in the dictionary ("header" for example)
			// read common dictionary, and then the template dictionary:

			foreach(string key in commonDictionary.Keys)
			{
				ToDictionaryIfMissing(key, (string)commonDictionary[key]);
			}

			foreach(string key in templateDictionary.Keys)
			{
				ToDictionaryIfMissing(key, (string)templateDictionary[key]);
			}

//			string placeholder = "<a href=\"" + Project.WEBSITE_LINK_WEBSTYLE + "\">make your own map</a>";

//			m_dictionary.Add("mapimage", "dmapimage");
//			m_dictionary.Add("mapimage.map", "dmapimage.map");
//			m_dictionary.Add("index.html", "dindex.html");
//			m_dictionary.Add("../index.html", "../ddindex.html");

//			m_dictionary.Add("template.index.html", m_templateName + ".index.html");

//			ToDictionaryIfMissing("{{StyleSheet}}", "Brown.css");
//			ToDictionaryIfMissing("{{First}}", Project.WEBSITE_LINK_WEBSTYLE);
//			ToDictionaryIfMissing("{{Next}}", Project.WEBSITE_LINK_WEBSTYLE);
//			ToDictionaryIfMissing("{{Back}}", Project.WEBSITE_LINK_WEBSTYLE);
//			ToDictionaryIfMissing("{{Home}}", Project.WEBSITE_LINK_WEBSTYLE);

			ToDictionary("{{Header}}", heading);
			ToDictionary("{{Body}}", makeBody());

			ToDictionary("{{Date}}", DateTime.Now.ToShortDateString());
			ToDictionary("{{Time}}", DateTime.Now.ToShortTimeString());

//			ToDictionaryIfMissing("{{LeftNavigation}}", placeholder);
//			ToDictionaryIfMissing("{{RightNavigation}}", placeholder);
//			ToDictionaryIfMissing("{{MetaKeywords}}", "quakemap,digital pictures,georeferencing,georeference,geocoding,geocode,EXIF,GIS,GPS,geocaching,hiking,boating,tracklog,track,log,Garmin,G7ToWin,JPG,JPEG,photo,photos,image,images,digital camera,latitude,longitude,timestamp");
//			ToDictionaryIfMissing("{{MetaDescription}}", "clickable map with pictures generated by QuakeMap.com");
//			ToDictionaryIfMissing("{{Footer}}", placeholder);
		}

		private string _body1 = "<img src=\"{0}\" border=0 usemap=\"#mainimagemap\" />";
		private string _body5 = "<br/><p><i>{0}</i></p>";

		private string makeBody()
		{
			StringBuilder bodyContent = new StringBuilder();

			string imageFileName = FromDictionary("mapimage");
			if(!imageFileName.ToLower().EndsWith(imageFileExt))
			{
				imageFileName += imageFileExt;
			}

			bodyContent.Append(String.Format(_body1, imageFileName));
			bodyContent.Append(String.Format(_body5, description));

			return bodyContent.ToString();
		}

		public void ToDictionary(string key, string word)
		{
			if(word == null)
			{
				if(m_dictionary[key] != null)
				{
					m_dictionary.Remove(key);
				}
			}
			else
			{
				m_dictionary[key] = word;
			}
		}

		public void ToDictionaryIfMissing(string key, string word)
		{
			if(word == null)
			{
				if(m_dictionary[key] != null)
				{
					m_dictionary.Remove(key);
				}
			}
			else if(m_dictionary[key] == null)
			{
				m_dictionary.Add(key, word);
			}
		}

		public string FromDictionary(string key)
		{
			object ret = m_dictionary[key];

			return (ret == null) ? key : (string)ret;
		}

		/// <summary>
		/// Generates all web page files using templates and data accumulated in this class.
		/// </summary>
		/// <param name="destinationFolderName"></param>
		/// <param name="physicalBounds"></param>
		public void generateAllPageContentFiles(string destinationFolderName, Rectangle physicalBounds, bool appendToMaster)
		{
			StringBuilder indexPageContent = new StringBuilder();

			// fill in the last missing piece - the click map:
			string clickMapContent = Project.mainCommand.doGenerateClickMap(physicalBounds);
			m_dictionary.Add("{{ClickMap}}", clickMapContent);
		
			/*
			indexPageContent.Append("---------- index page HTML -----------<br><br>\r\n");

			indexPageContent.Append("shortName: <b>" + shortName + "</b><br>\r\n");
			indexPageContent.Append("heading: <b>" + heading + "</b><br>\r\n");
			indexPageContent.Append("description: <b>" + description + "</b><br>\r\n");
			indexPageContent.Append("templateFolder: <b>" + m_templateFolder + "</b><br>\r\n");
			*/

			string destIndex = FromDictionary("index.html");
			string destinationFilename = Path.Combine(destinationFolderName, destIndex);
			string templateFilename = Path.Combine(m_templateFolder, FromDictionary("template.index.html"));

			// read in the template and fill-the-blanks there:
 
			FileStream fs = new FileStream(templateFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
			TextReader textReader = new StreamReader(fs);

			int lineCounter = 0;
			string str;
			while((str=textReader.ReadLine()) != null)
			{
				str = str.Trim();
				lineCounter++;

			again:
				int pos = str.IndexOf("{{");
				if(pos == -1)
				{
					indexPageContent.Append(str);
				}
				else
				{
					indexPageContent.Append(str.Substring(0,pos));
					str = str.Substring(pos + 2);
					int pos1 = str.IndexOf("}}");
					if(pos1 == -1)
					{
						// no closing }} -- bad tag. Try recovering.
						indexPageContent.Append("{{");
					}
					else
					{
						// ok, we are at the point where replacement token needs to be determined
						string keyToken = "{{" + str.Substring(0, pos1) + "}}";
						indexPageContent.Append(FromDictionary(keyToken));
						str = str.Substring(pos1 + 2);
					}
					goto again;
				}
				indexPageContent.Append("\r\n");
			}

			fs.Close();

			Project.writeTextFile(destinationFilename, indexPageContent.ToString());

			// take care of stylesheet:
			string ss = FromDictionary("{{StyleSheet}}");
			if(!"{{StyleSheet}}".Equals(ss))
			{
				File.Copy(Path.Combine(m_templateFolder, ss), Path.Combine(destinationFolderName, ss), true);
			}

			if(appendToMaster)
			{
				DirectoryInfo destinationFolder = new DirectoryInfo(destinationFolderName);
				string masterFile = FromDictionary("../index.html");
				string masterFileName = Path.Combine(destinationFolder.Parent.FullName, masterFile.Replace("../", ""));

				if(!File.Exists(masterFileName))
				{
					Project.ErrorBox(null, masterFileName + " not found, will skip inserion to " + masterFile);
				}
				else
				{
					// find the ../index.html and append relative url of the generated page between <!--trips--> and <!--/trips--> 
					string relativeUrl = destinationFolder.Name + "/" + destIndex;
					string toInsert = "<li><a href=\"" + relativeUrl + "\">" + heading + "</a></li>\r\n";
					bool masterDirty = false;
					indexPageContent = new StringBuilder();

					fs = new FileStream(masterFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
					textReader = new StreamReader(fs);

					lineCounter = 0;
					int state = 0;
					while((str=textReader.ReadLine()) != null)
					{
						str = str.Trim();
						lineCounter++;

						switch(state)
						{
							case 0:
								if(str.Replace(" ","").StartsWith("<!--trips--><ul>"))
								{
									state = 1;
								}
								break;

							case 1:
								if(str.Replace(" ","").StartsWith("<!--/trips--></ul>"))
								{
									indexPageContent.Append(toInsert);
									masterDirty = true;
									state = 2;
								}
								else if(str.IndexOf(toInsert) >= 0)
								{
									Project.ErrorBox(null, masterFile + " already has " + toInsert);
									state = 2;
								}
								break;

							default:
								break;
						}

						indexPageContent.Append(str);
						indexPageContent.Append("\r\n");
					}

					fs.Close();

					if(masterDirty)
					{
						Project.writeTextFile(masterFileName, indexPageContent.ToString());
					}
				}
			}
		}
	}
}
