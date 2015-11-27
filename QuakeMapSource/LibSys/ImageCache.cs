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
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Net;

namespace LibSys
{
	internal class ImageStorage : IDisposable
	{
		internal Image image;
		internal string fileName;		// likely a temp file

		public ImageStorage(Image img, string fn)
		{
			image = img;
			fileName = fn;		// can be null
		}

		// as the Bitmap cannot be stored in a hashtable between threads, we just recreate it from temp file.
		public Image getImage(bool isThumb)
		{
			if(isThumb)
			{
				Bitmap tmp = new Bitmap(fileName, false);
				image = new Bitmap(tmp, new Size(Project.thumbWidth, Project.thumbHeight));
				tmp.Dispose();
			}
			else
			{
				image = new Bitmap(fileName, false);
			}

			return image;
		}

		public void Dispose()
		{
			if(image != null)
			{
				image.Dispose();
				image = null;
			}
			GC.SuppressFinalize(this); 
		}

	}

	/// <summary>
	/// The trick in ImageCache is that Bitmap is MrshalByRef, and if stored in a different thread will show up corrupted.
	/// So we load files by http, and then cache their file names to create Bitmap every time we need it.
	/// see http://www.dotnet247.com/247reference/msgs/53/266788.aspx
	/// </summary>
	public class ImageCache
	{
		private static ImageCache This = new ImageCache(); // ensure ~ImageCache() is called

		private ImageCache()	// all methods static
		{
		}

		private static Hashtable m_images = new Hashtable();		// of ImageStorage, by source filename or URL

		private static Hashtable m_imagesThumb = new Hashtable();	// of ImageStorage, by source filename or URL
		private static int Project_thumbWidth = 0;
		private static int Project_thumbHeight = 0;

		public static Image getImage(string source, bool isThumb)
		{
			ImageStorage ret = null;

			if(isThumb)
			{
				// if thumbnail size has changed, invalidate that portion of the cache:
				if(Project.thumbWidth != Project_thumbWidth || Project.thumbHeight != Project_thumbHeight)
				{
					m_imagesThumb.Clear();
					Project_thumbWidth = Project.thumbWidth; 
					Project_thumbHeight = Project.thumbHeight;
				}

				if((ret=(ImageStorage)m_imagesThumb[source]) == null)
				{
					if(source.ToLower().StartsWith("http://"))
					{
						// Create an Image object from the source - thumbnail URL.
						string fileName;
						Image img = getImageByURL(source, out fileName);

						// make a thumbnail bitmap:
						Bitmap thumb = new Bitmap(img, new Size(Project.thumbWidth, Project.thumbHeight));
						ImageCache.putImage(thumb, source, fileName, true);
						return thumb;
					}
					else
					{
						// we need to know image Orientation to create a thumbnail from file.
						// it means that creating the thumbnail must be handled in PhotoDescr rather than here.
						// will return null later.
					}
				}
			}
			else
			{
				// source is either a file name (including isnide a zipfile) or a URL - size dependent.
				if((ret=(ImageStorage)m_images[source]) == null)
				{
					if(source.ToLower().StartsWith("http://"))
					{
						// Create an Image object from the source - thumbnail URL.
						string fileName;
						Image img = getImageByURL(source, out fileName);

						ImageCache.putImage(img, source, fileName, false);
						return img;
					}
					// else handled in PhotoDescr
				}
			}

			if(ret == null)
			{
				// happens for local-source images which we don't want to deal with here
				return null;
			}

			// found container, return contained:
			return ret.getImage(isThumb);
		}

		// source is either source URL, or file name (i.e. sourceFile)
		// sourceFile is never null, should point to a real file
		public static void putImage(Image image, string source, string sourceFile, bool isThumb)
		{
			if(image != null)
			{
				if(sourceFile == null)
				{
					// actual source is likely inside the zipfile, make a handy temp file:
					sourceFile = Path.GetTempFileName();
					image.Save(sourceFile);
					tempFileNames.Add(sourceFile);
				}

				if(isThumb)
				{
					m_imagesThumb[source] = new ImageStorage(image, sourceFile);
				}
				else
				{
					m_images[source] = new ImageStorage(image, sourceFile);
				}
			}
		}

		public static bool releaseImage(Image image)
		{
			bool ret = false;

			foreach(string key in m_images.Keys)
			{
				ImageStorage imageStorage = (ImageStorage)m_images[key];

				if(imageStorage.image.Equals(image))
				{
					if(key.StartsWith("http://"))
					{
						LibSys.StatusBar.Trace("not releasing from image cache: " + key);
					}
					else
					{
						LibSys.StatusBar.Trace("releasing from image cache: " + key);
						imageStorage.Dispose();
						m_images.Remove(key);
						ret = true;
					}
					break;
				}
			}

			foreach(string key in m_imagesThumb.Keys)
			{
				ImageStorage imageStorage = (ImageStorage)m_imagesThumb[key];

				if(imageStorage.image.Equals(image))
				{
					if(key.StartsWith("http://"))
					{
						LibSys.StatusBar.Trace("not releasing from imageThumb cache: " + key);
					}
					else
					{
						LibSys.StatusBar.Trace("releasing from imageThumb cache: " + key);
						imageStorage.Dispose();
						m_imagesThumb.Remove(key);
						ret = true;
					}
					break;
				}
			}

			return ret;
		}

		public static bool releaseImage(string imageSource)
		{
			bool ret = false;

			ImageStorage imageStorage = (ImageStorage)m_imagesThumb[imageSource];

			if(imageStorage != null)
			{
				imageStorage.Dispose();
				m_imagesThumb.Remove(imageSource);
				ret = true;
			}

			imageStorage = (ImageStorage)m_images[imageSource];

			if(imageStorage != null)
			{
				imageStorage.Dispose();
				m_images.Remove(imageSource);
				ret = true;
			}

			return ret;
		}

		private static ArrayList tempFileNames = new ArrayList();

		// helper method:
		private static Image getImageByURL(string sUrl, out string fileName)
		{
			Image ret = null;

			fileName = Path.GetTempFileName();

			LibSys.StatusBar.Trace("IP: downloading image from: " + sUrl);

			Cursor.Current = Cursors.WaitCursor;

			WebClient client = new WebClient ();
			client.DownloadFile(sUrl, fileName);

			//ret = Image.FromFile(fileName, true);
			ret = new Bitmap(fileName);

			tempFileNames.Add(fileName);

			//File.Delete(fileName); // causes exception, file used by a process

			return ret;
		}

		~ImageCache()
		{
			foreach(string fileName in tempFileNames)
			{
				try
				{
					//LibSys.StatusBar.Trace("Deleting: " + fileName);
					File.Delete(fileName);
				}
				catch
				{
					LibSys.StatusBar.Error("Failed to delete: " + fileName);
				}
			}
		}
	}
}
