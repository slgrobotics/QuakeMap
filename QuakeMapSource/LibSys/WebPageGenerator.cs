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
using System.IO;

namespace LibSys
{
	/// <summary>
	/// WebPageGenerator is called when a web page HTML needs to be generated.
	/// </summary>
	public class WebPageGenerator
	{
		public static int topOffset = 11;	// size of top white area with links to QuakeMap.com  -- see doPrintPage() in MainForm
		public static int margin = 0;		// offset from the left 

		private WebPage m_webPage;
		private string m_destinationFolderName;

		public string indexFileFullPath { get { return Path.Combine(m_destinationFolderName, m_webPage.FromDictionary("index.html")); } }

		public WebPageGenerator(string destinationFolderName, WebPage webPage)
		{
			m_destinationFolderName = destinationFolderName;
			m_webPage = webPage;
		}

		public void generateAll(bool appendToMaster)
		{
			// generate main image for clicking:

			string filename = Path.Combine(m_destinationFolderName, m_webPage.FromDictionary("mapimage"));

			Rectangle physicalBounds = new Rectangle(0, 0, m_webPage.imageWidth + margin * 2, m_webPage.imageHeight + topOffset + margin);

			Bitmap bitmap = new Bitmap(physicalBounds.Width, physicalBounds.Height, PixelFormat.Format32bppArgb);

			Graphics graphics = Graphics.FromImage(bitmap);

			graphics.FillRectangle(Project.whiteBrush, physicalBounds);

			Project.mainCommand.doPrintPage(physicalBounds, graphics, true);

			if(!filename.ToLower().EndsWith(m_webPage.imageFileExt))
			{
				filename += m_webPage.imageFileExt;
			}

			filename = new FileInfo(filename).FullName;

			bitmap.Save(filename, m_webPage.imageFormat);

//			// generate image map:
//			filename = Path.Combine(m_destinationFolderName, m_webPage.FromDictionary("mapimage.map"));
//			string clickMapContent = Project.mainCommand.doGenerateClickMap(physicalBounds);
//			Project.writeTextFile(filename, clickMapContent);

			// generate index and other files for the web page:

			m_webPage.generateAllPageContentFiles(m_destinationFolderName, physicalBounds, appendToMaster);
		}
	}
}
