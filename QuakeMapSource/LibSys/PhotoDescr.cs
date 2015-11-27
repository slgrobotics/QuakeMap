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
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections;

using ICSharpCode.SharpZipLib.Zip;

namespace LibSys
{
	/// <summary>
	/// class PhotoDescr replaces Image, stores DateTime, file name, extracts EXIF tags to fill the PhotoDescr fields.
	/// </summary>
	public sealed class PhotoDescr : IDisposable
	{
		public DateTime DTOrig = DateTime.MinValue;
		public double Latitude = 0.0d;
		public double Longitude = 0.0d;
		public double Altitude = 0.0d;		// meters
		public bool hasCoordinates = false;
		public int Width;
		public int Height;
		public int Orientation = -1;
		public string imageName = null;		// usually something like IMAGE, will be generated from file name if not passed to constructor
		public Image image = null;
		public bool imageDisposable = true;	// if not, Dispose() must be called by the containing control
		public int Id;

		private string m_imageFileName = null;	// can be in the form "C:\\aaa\aaa.zip|bbb\IMAGE.jpg" or null if URL-driven
		private Hashtable imageUrls = null;		// will stay null unless there are URLs

		public bool imageSourceIsLocal 
		{
			get 
			{
				return m_imageFileName != null;
			}
		}

		/// <summary>
		///  the intent here is to have a stable source name, independent on the image size
		/// </summary>
		public string imageSource
		{
			get 
			{
				if(imageSourceIsLocal)
				{
					return m_imageFileName;
				}
				else
				{
					return (string)imageUrls["OriginalURL"];
				}
			}

			set 
			{
				m_imageFileName = value;
			}
		}

		/// <summary>
		/// can change dependent on the selected image size
		/// </summary>
		public string imageUrl 
		{
			get 
			{
				if(imageSourceIsLocal)
				{
					return m_imageFileName;
				}
				else
				{
					if(Project.photoGalleryPictureSize >= Project.photoGalleryPictureSizes.Length)
					{
						Project.photoGalleryPictureSize = 2;
					}
					return (string)imageUrls[Project.photoGalleryPictureSizes[Project.photoGalleryPictureSize]];
				}
			}
		}

		public string imageThumbSource 
		{
			get 
			{
				if(imageSourceIsLocal)
				{
					return m_imageFileName;
				}
				else
				{
					return (string)imageUrls["ThumbURL"];
				}
			}
		}

		private static int nextId = 0;

		// use FromFile() to create
		private PhotoDescr()
		{
			Id = ++nextId;
		}

		/// <summary>
		/// Make a clone useable for a new PhotoViewerControl, like in DlgPhotoDraw
		/// </summary>
		/// <param name="prototype"></param>
		/// <returns></returns>
		public static PhotoDescr FromPhotoDescr(PhotoDescr prototype)
		{
			PhotoDescr ret = new PhotoDescr();

			ret.DTOrig = prototype.DTOrig;
			ret.Latitude = prototype.Latitude;
			ret.Longitude = prototype.Longitude;
			ret.Altitude = prototype.Altitude;
			ret.hasCoordinates = prototype.hasCoordinates;
			ret.Width = prototype.Width;
			ret.Height = prototype.Height;
			ret.Orientation = prototype.Orientation;
			ret.imageName = prototype.imageName;

			ret.image = null;	// to be assigned directly, if needed for draw; may be by ensureOriginalImageExists()

			ret.m_imageFileName = prototype.m_imageFileName;
			if(prototype.imageUrls != null)
			{
				ret.imageUrls = new Hashtable();
				foreach(string key in prototype.imageUrls.Keys)
				{
					ret.imageUrls.Add(key, prototype.imageUrls[key]);
				}
			}

			return ret;
		}

		public void Dispose()
		{
			if(this.image != null)
			{
				ImageCache.releaseImage(image);
				this.image.Dispose();
			}
			this.image = null;
			GC.SuppressFinalize(this); 
		}

		/// <summary>
		/// fileName can be in form "C:\\aaa\aaa.zip|bbb\bbb.jpg"
		/// imageName will be generated if name=null is passed (in the form of "bbb.jpg")
		/// </summary>
		public static PhotoDescr FromFile(string fileName, string name)
		{
			return FromFileOrZipEntry(null, null, fileName, name);
		}

		private static Hashtable m_photoDescrByThumbnailUrl = new Hashtable();

		private static PhotoDescr FromSmugmugThumbnail(string thumbUrl)
		{
			return (PhotoDescr)m_photoDescrByThumbnailUrl[thumbUrl];
		}

		/// <summary>
		/// this works on URLs only if a photoDescr has been cached before, and on locals - always
		/// </summary>
		/// <param name="thumbSource"></param>
		/// <returns></returns>
		public static PhotoDescr FromThumbnail(string thumbSource)
		{
			PhotoDescr photoDescr = null;

			if(thumbSource.StartsWith("http://"))
			{
				photoDescr = PhotoDescr.FromSmugmugThumbnail(thumbSource);
			}
			else
			{
				photoDescr = PhotoDescr.FromFileOrZipEntry(null, null, thumbSource, null);
			}

			return photoDescr;
		}

		public static PhotoDescr FromSmugmugPhotoGallery(string imageName, Hashtable _imageUrls, Hashtable imageExif)
		{
			PhotoDescr ret = new PhotoDescr();

			ret.imageName = imageName;

			try
			{
				string sTime = "" + imageExif["DateTimeOriginal"];		// "2003:08:05 20:12:23"
				string[] split = sTime.Split(new Char[] {' '});
				sTime = split[0].Replace(":", "/") + " " + split[1];
				ret.DTOrig = Convert.ToDateTime(sTime);			// local, how time in the camera is set
			}
			catch (Exception) 
			{
			}

//	TinyURL: http://heysmile.smugmug.com/photos/24096277-Ti.jpg
//	MediumURL: http://heysmile.smugmug.com/photos/24096277-M.jpg
//	ThumbURL: http://heysmile.smugmug.com/photos/24096277-Th.jpg
//	AlbumURL: http://heysmile.smugmug.com/gallery/576522/1/24096277
//	SmallURL: http://heysmile.smugmug.com/photos/24096277-S.jpg
//	LargeURL: http://heysmile.smugmug.com/photos/24096277-L.jpg
//	OriginalURL: http://heysmile.smugmug.com/photos/24096277-O.jpg

			ret.imageUrls = new Hashtable(_imageUrls);		// make a local copy

			string thumbUrl = "" + ret.imageUrls["ThumbURL"];

			// Create an Image object from the thumbnail URL.
//			string fileName = "C:\\temp\\aaa.jpg";
//			Image img = null;
			try 
			{
				ImageCache.getImage(thumbUrl, true);
//				ret.Width = img.Width;
//				ret.Height = img.Height;
//				ret.image = img;

				m_photoDescrByThumbnailUrl[thumbUrl] = ret;
			}
				// An invalid image will throw an OutOfMemoryException 
				// exception
			catch (OutOfMemoryException) 
			{
				throw new InvalidOperationException("'"	+ ret.imageUrls["OriginalURL"] + "' is not a valid and accessible gallery image.");
			}

			return ret;
		}

		/// <summary>
		/// fileName can be in form "C:\\aaa\aaa.zip|bbb\bbb.jpg"
		/// imageName will be generated if name=null is passed (in the form of "bbb.jpg")
		/// </summary>
		/// <param name="zip"></param>
		/// <param name="zipEntry"></param>
		/// <param name="fileName"></param>
		/// <param name="imageName"></param>
		/// <returns></returns>
		public static PhotoDescr FromFileOrZipEntry(ZipFile zip, ZipEntry zipEntry, string fileName, string name)
		{
			PhotoDescr ret = new PhotoDescr();

			Stream stream = null;
			bool doCloseZip = false;
			ret.imageName = name;

			if(zip == null)
			{
				int pos = fileName.IndexOf("|");
				if(pos >= 0)
				{
					// this is a .zip or .gpz file
					string zipFileName = fileName.Substring(0, pos);

                    if (!File.Exists(zipFileName))
                    {
                        LibSys.StatusBar.Error("Failed to open Zip");
                        throw new InvalidOperationException("'" + zipFileName + "' not found or not a valid zip file");
                    }

                    zip = new ZipFile(zipFileName);

                    doCloseZip = true;
                    string photoFileName = fileName.Substring(pos + 1);

                    zipEntry = zip.GetEntry(photoFileName);
                    if (zipEntry == null)
                    {
                        zip.Close();
                        throw new InvalidOperationException("'" + photoFileName + "' is not found inside " + zipFileName);
                    }
				}
				else
				{
					// plain file
					ret.imageSource = fileName;
					if(ret.imageName == null)
					{
						// generate image name based on file name:
						ret.imageName = imageNameFromFileName(Path.GetFileName(fileName));
					}
				}
			}

			if(zip != null)
			{
				ret.imageSource = zip.Name + "|" + zipEntry.Name;
				if(ret.imageName == null)
				{
					// generate image name based on file name:
					int pos = zipEntry.Name.LastIndexOf("\\");
					string imageFileName;
					if(pos >= 0)
					{
						imageFileName = zipEntry.Name.Substring(pos + 1);
					}
					else
					{
						imageFileName = zipEntry.Name;
					}
					ret.imageName = imageNameFromFileName(imageFileName);
				}

                stream = zip.GetInputStream(zipEntry);
			}

			// Create an Image object from the specified file.
			Image img = null;
			try 
			{
				img = (zip == null) ? Image.FromFile(fileName, true) : new Bitmap(stream);
				ret.Width = img.Width;
				ret.Height = img.Height;
				ret.image = img;
			}
        		// An invalid image will throw an OutOfMemoryException 
			catch (OutOfMemoryException) 
			{
				if(doCloseZip)
				{
					zip.Close();
				}
				throw new InvalidOperationException("'"	+ fileName + "' is not a valid image file.");
			}

			if(doCloseZip)
			{
				zip.Close();
			}

			try 
			{
				//	<tagMetadata id="36867" category="EXIF">
				//		<name>DTOrig</name>
				//		<description>Date and time when the original image data was generated. For a DSC, the date and time when the picture was taken. The format is YYYY:MM:DD HH:MM:SS with time shown in 24-hour format and the date and time separated by one blank character (0x2000). The character string length is 20 bytes including the NULL terminator. When the field is empty, it is treated as unknown.</description>
				//	</tagMetadata>
				//	<tagMetadata id="36868" category="EXIF">
				//		<name>DTDigitized</name>
				//		<description>Date and time when the image was stored as digital data. If, for example, an image was captured by DSC and at the same time the file was recorded, then DateTimeOriginal and DateTimeDigitized will have the same contents. The format is YYYY:MM:DD HH:MM:SS with time shown in 24-hour format and the date and time separated by one blank character (0x2000). The character string length is 20 bytes including the NULL terminator. When the field is empty, it is treated as unknown.</description>
				//	</tagMetadata>

				PropertyItem prop = img.GetPropertyItem(36867);
				switch(prop.Type)
				{
					case 2:
						string sTime = Project.ByteArrayToStr(prop.Value);		// "2003:08:05 20:12:23"
						string[] split = sTime.Split(new Char[] {' '});
						sTime = split[0].Replace(":", "/") + " " + split[1];
						ret.DTOrig = Convert.ToDateTime(sTime);			// local, how time in the camera is set
						break;
				}
			}
			catch (Exception) 
			{
			}

			try 
			{
				//	<tagMetadata id="1" category="GPS">
				//		<name>GPSLatitudeRef</name>
				//		<description>Indicates whether the latitude is north or south latitude. 
				//		The ASCII value 'N' indicates north latitude, and 'S' is south latitude.</description>
				//		<valueOptions>
				//			<option key="N" keyType="CHAR" value="North latitude" />
				//			<option key="S" keyType="CHAR" value="South latitude" />
				//			<optionOtherwise value="reserved" />
				//		</valueOptions>
				//	</tagMetadata>
				//	<tagMetadata id="2" category="GPS">
				//		<name>GPSLatitude</name>
				//		<description>Indicates the latitude. The latitude is expressed as three 
				//		RATIONAL values giving the degrees, minutes, and seconds, respectively. 
				//		When degrees, minutes and seconds are expressed, the format is dd/1,mm/1,ss/1. 
				//		When degrees and minutes are used and, for example, fractions of minutes are 
				//		given up to two decimal places, the format is dd/1,mmmm/100,0/1.</description>
				//	</tagMetadata>
				//	<tagMetadata id="3" category="GPS">
				//		<name>GPSLongitudeRef</name>
				//		<description>Indicates whether the longitude is east or west longitude. 
				//		The ASCII value 'E' indicates east longitude, and 'W' is west longitude.</description>
				//		<valueOptions>
				//			<option key="E" keyType="CHAR" value="East longitude" />
				//			<option key="W" keyType="CHAR" value="West longitude" />
				//			<optionOtherwise value="reserved" />
				//		</valueOptions>
				//	</tagMetadata>
				//	<tagMetadata id="4" category="GPS">
				//		<name>GPSLongitude</name>
				//		<description>Indicates the longitude. The longitude is expressed as three 
				//		RATIONAL values giving the degrees, minutes, and seconds, respectively. 
				//		When degrees, minutes and seconds are expressed, the format is ddd/1,mm/1,ss/1. 
				//		When degrees and minutes are used and, for example, fractions of minutes are 
				//		given up to two decimal places, the format is ddd/1,mmmm/100,0/1.</description>
				//	</tagMetadata>

				string sLatRef = null;
				double lat = 0.0d;
				string sLngRef = null;
				double lng = 0.0d;

				PropertyItem prop = img.GetPropertyItem(1);
				switch(prop.Type)
				{
					case 2:
						sLatRef = Project.ByteArrayToStr(prop.Value);		// "N" or "S"
						break;
				}

				prop = img.GetPropertyItem(2);
				switch(prop.Type)
				{
					case 5:
						lat = ParseRationalCoord(prop);		// "51.0 56.0 33.123"
						break;
				}

				prop = img.GetPropertyItem(3);
				switch(prop.Type)
				{
					case 2:
						sLngRef = Project.ByteArrayToStr(prop.Value);		// "W" or "E"
						break;
				}

				prop = img.GetPropertyItem(4);
				switch(prop.Type)
				{
					case 5:
						lng = ParseRationalCoord(prop);		// "4.0 22.0 33.123"
						break;
				}

				if(sLatRef != null && sLngRef != null)
				{
					bool isWest = sLngRef.ToLower().StartsWith("w");
					bool isSouth = sLatRef.ToLower().StartsWith("s");
					if(isSouth)
					{
						lat = -lat;
					}
					if(isWest)
					{
						lng = -lng;
					}
					ret.Latitude = lat;
					ret.Longitude = lng;
					ret.hasCoordinates = true;
				}
			}
			catch (Exception) 
			{
			}

			try 
			{
				//	<tagMetadata id="274" category="">
				//		<name>Orientation</name>
				//		<description>Image orientation viewed in terms of rows and columns.</description>
				//		<valueOptions>
				//			<option key="1" value="The 0th row is at the visual top of the image, and the 0th column is the visual left-hand side." />
				//			<option key="2" value="The 0th row is at the visual top of the image, and the 0th column is the visual right-hand side." />
				//			<option key="3" value="The 0th row is at the visual bottom of the image, and the 0th column is the visual right-hand side." />
				//			<option key="4" value="The 0th row is at the visual bottom of the image, and the 0th column is the visual left-hand side." />
				//			<option key="5" value="The 0th row is the visual left-hand side of of the image, and the 0th column is the visual top." />
				//			<option key="6" value="The 0th row is the visual right-hand side of of the image, and the 0th column is the visual top." />
				//			<option key="7" value="The 0th row is the visual right-hand side of of the image, and the 0th column is the visual bottom." />
				//			<option key="8" value="The 0th row is the visual left-hand side of of the image, and the 0th column is the visual bottom." />
				//			<optionOtherwise value="reserved" />
				//		</valueOptions>
				//	</tagMetadata>
				PropertyItem prop = img.GetPropertyItem(274);
				switch(prop.Type)
				{
					case 3:
						ret.Orientation = (int)prop.Value[0];		// 6 and 8 are known to appear here 
						break;
				}
			}
			catch (Exception) 
			{
			}

			return ret;
		}

		/// <summary>Index byte jump for a PropertyItem's byte array of Longs.</summary>
		private const int BYTEJUMP_LONG			= 4;
		/// <summary>Index byte jump for a PropertyItem's byte array of Rationals.</summary>
		private const int BYTEJUMP_RATIONAL		= 8;

		/// <summary>Parse a Rational tag (unsigned).</summary>
		private static double ParseRationalCoord(PropertyItem propItem) 
		{
			double ret = 0.0d;
			double div = 1.0d;
			for (int i = 0; i < propItem.Len; i = i + BYTEJUMP_RATIONAL) 
			{
				System.UInt32 numer = BitConverter.ToUInt32(propItem.Value, i);
				System.UInt32 denom = BitConverter.ToUInt32(propItem.Value, i + BYTEJUMP_LONG);

				double dbl;
				if (denom  == 0)
					dbl = 0.0;
				else
					dbl = (double)numer / (double)denom;
				ret += dbl / div;
				div *= 60.0d;
			}
			return ret;
		}

		public static void toItemValueInt16(int val, Byte[] bytes)
		{
			bytes[0] = (byte)(val % 256);
			bytes[1] = (byte)(val / 256);
		}

		public static byte[] ToPackedRationalCoord(double val)
		{
			byte[] ret = new Byte[24];
			UInt32[] vals = new UInt32[3];
			UInt32[] denoms = new UInt32[3];

			double dAbsX = Math.Abs(val);
			double dFloorX = Math.Floor(dAbsX);
			double dMinX = (dAbsX - dFloorX) * 60.0;
			vals[0] = (uint)Math.Round(dFloorX);			// deg
			denoms[0] = 1;
			vals[1] = (uint)Math.Round(Math.Floor(dMinX));	// min
			denoms[1] = 1;
			vals[2] = (uint)Math.Round(Math.Floor((dMinX - (double)vals[1]) * 60.0 * 100));		// sec
			denoms[2] = 100;

			int j = 0;
			for (int i = 0; i < ret.GetLength(0); i = i + BYTEJUMP_RATIONAL) 
			{
				System.UInt32 numer = vals[j];
				System.UInt32 denom = denoms[j];
				
				byte[] numerBytes = BitConverter.GetBytes(numer);
				Array.Copy(numerBytes, 0, ret, i, 4);
				byte[] denomBytes = BitConverter.GetBytes(denom);
				Array.Copy(denomBytes, 0, ret, i + BYTEJUMP_LONG, 4);
				
				j++;
			}

			return ret;
		}

		private static string imageNameFromFileName(string fileName)
		{
			if(fileName.ToLower().EndsWith(".jpg") || fileName.ToLower().EndsWith(".bmp"))
			{
				fileName = fileName.Substring(0, fileName.Length - 4);
			}
			return fileName;
		}

		/// <summary>
		/// releases the image from memory and file. To get it back use ensureImageExists()
		/// </summary>
		public void releaseImage()
		{
			if(imageDisposable)
			{
				if(this.image != null && ImageCache.releaseImage(image))
				{
					this.image.Dispose();		// only local images are disposed, those from image cache will stay there.
				}
				this.image = null;
			}
			GC.Collect();
		}

		public static Bitmap bitmapFromFileOrZipfile(string imageFileName)
		{
			Bitmap ret = null;

			// get it from the file or zip archive
			ZipFile zip = null;
			ZipEntry zipEntry = null;
			Stream stream = null;
			bool doCloseZip = false;

			int pos = imageFileName.IndexOf("|");
			if(pos >= 0)
			{
				// this is a .zip or .gpz file
				string zipFileName = imageFileName.Substring(0, pos);

                if (!File.Exists(zipFileName))
                {
                    LibSys.StatusBar.Error("Failed to open Zip");
                    throw new InvalidOperationException("'" + zipFileName + "' not found or not a valid zip file");
                }

                zip = new ZipFile(zipFileName);

				doCloseZip = true;
				string photoFileName = imageFileName.Substring(pos + 1);

				zipEntry = zip.GetEntry(photoFileName);
				if(zipEntry == null)
				{
					zip.Close();
					throw new InvalidOperationException("'"	+ photoFileName + "' is not found inside " + zipFileName);
				}
			}

			if(zip != null)
			{
                stream = zip.GetInputStream(zipEntry);
			}

			// Create a Bitmap object from the specified file.
			try 
			{
				//ret = (zip == null) ? Image.FromFile(imageFileName, true) : new Bitmap(stream);
				ret = (zip == null) ? new Bitmap(imageFileName, true) : new Bitmap(stream);
			}
				// An invalid image will throw an OutOfMemoryException 
				// exception
			catch (OutOfMemoryException) 
			{
				if(doCloseZip)
				{
					zip.Close();
				}
				throw new InvalidOperationException("'"	+ imageFileName + "' is not a valid image file.");
			}

			if(doCloseZip)
			{
				zip.Close();
			}

			return ret;
		}

		/// <summary>
		/// restores image from file, if it is not already in memory
		/// </summary>
		public void ensureImageExists()
		{
			if(this.image != null)
			{
				return;
			}

			if(this.imageSourceIsLocal)
			{
				Image img = bitmapFromFileOrZipfile(m_imageFileName);
				this.image = img;
			}
			else
			{
				// get it from the gallery

				try 
				{
					this.image = ImageCache.getImage(this.imageUrl, false);
				}
					// An invalid image will throw an OutOfMemoryException 
					// exception
				catch (OutOfMemoryException) 
				{
					throw new InvalidOperationException("'"	+ this.imageUrl + "' could not deliver a valid image file.");
				}
			}
		}

		public void ensureOriginalImageExists()
		{
			// for local images do nothing if image is attached:
			if(this.imageSourceIsLocal && this.image != null)
			{
				return;
			}

			if(this.imageSourceIsLocal)
			{
				// local image, use local rule:
				ensureImageExists();
			}
			else
			{
				// get original image from the gallery

				string originalImageUrl = this.imageSource;

				try 
				{
					Image orgImage = ImageCache.getImage(originalImageUrl, false);

					releaseImage();		// make sure we nicely get rid of existing image

					this.image = orgImage;
				}
					// An invalid image will throw an OutOfMemoryException 
					// exception
				catch (OutOfMemoryException) 
				{
					throw new InvalidOperationException("'"	+ originalImageUrl + "' could not deliver a valid original image file.");
				}
			}
		}

		/// <summary>
		/// delete image file inside the zip file, or return a name of file to delete on the hard drive.
		///  Make sure you dispose of this PhotoDescr before deleting the file.
		/// </summary>
		public string deleteFromDisk()
		{
			string ret = null;

			if(this.imageSourceIsLocal)
			{
				// make sure the image is not used/locked and we can actually delete it:
				releaseImage();
				ImageCache.releaseImage(m_imageFileName);
				GC.Collect();

				int pos = m_imageFileName.IndexOf("|");
				if(pos >= 0)
				{
					// this is a .zip or .gpz file
					string zipFileName = m_imageFileName.Substring(0, pos);
                    if (!File.Exists(zipFileName))
                    {
                        LibSys.StatusBar.Error("Failed to open Zip");
                        throw new InvalidOperationException("'" + zipFileName + "' not found or not a valid zip file");
                    }

                    using (ZipFile zip = new ZipFile(zipFileName))
                    {
                        string photoFileName = m_imageFileName.Substring(pos + 1);

                        ZipEntry zipEntry = zip.GetEntry(photoFileName);
                        if (zipEntry != null)
                        {
                            zip.BeginUpdate();
                            zip.Delete(zipEntry);
                        }
                        else
                        {
                            zip.Close();
                            throw new InvalidOperationException("'" + photoFileName + "' is not found inside " + zipFileName);
                        }
                        zip.CommitUpdate();
                    }
				}
				else
				{
					ret = m_imageFileName;
				}
			}

			return ret;
		}

		public static Image rebuildThumbnailImage(string thumbSource)
		{
			Image ret = null;

			if(thumbSource.ToLower().StartsWith("http://"))
			{
				// a gallery, and actual URL of the thumbnail

				ret = ImageCache.getImage(thumbSource, true);
			}
			else
			{
				// local file, containing large image. 

				PhotoDescr pd = PhotoDescr.FromFileOrZipEntry(null, null, thumbSource, "none");

				float imageRatio = (float)pd.image.Width / (float)pd.image.Height;
				float panelRatio = (float)Project.thumbWidth / (float)Project.thumbHeight;

				int thumbWidth;
				int thumbHeight;

				if(imageRatio > panelRatio)	// image wider
				{
					thumbWidth = Project.thumbWidth;
					thumbHeight = (int)(thumbWidth / imageRatio);
				}
				else
				{
					thumbHeight = Project.thumbHeight;
					thumbWidth = (int)(thumbHeight * imageRatio);
				}

				// make a thumbnail bitmap:
				Bitmap thumb = new Bitmap(pd.image, new Size(thumbWidth, thumbHeight));
				// the following produces ugly big unfocused thumbnail, so we stay with Bitmap for now:
				//Image thumb = photoDescr.image.GetThumbnailImage(Project.thumbWidth, Project.thumbHeight, null, IntPtr.Zero);
				// rotate thumbnail based on photoDescr.Orientation.
				switch(pd.Orientation)
				{
						// see PhotoDescr.cs for EXIF orientation codes:
					case 4:
						thumb.RotateFlip(RotateFlipType.Rotate180FlipNone);
						break;
					case 8:
						thumb.RotateFlip(RotateFlipType.Rotate270FlipNone);
						break;
					case 6:
						thumb.RotateFlip(RotateFlipType.Rotate90FlipNone);
						break;
				}
				if(thumbSource.IndexOf("|") != -1)
				{
					ImageCache.putImage(thumb, thumbSource, null, true);	// zipfile - putImage() will generate local temp file
				}
				else
				{
					ImageCache.putImage(thumb, thumbSource, thumbSource, true);
				}
				ret = thumb;

				pd.releaseImage();
			}

			return ret;
		}

		public Image ThumbnailImage
		{
			get
			{
				Image ret = null;

				if(this.imageSourceIsLocal)
				{
					if((ret=ImageCache.getImage(this.m_imageFileName, true)) == null)
					{
						ret = rebuildThumbnailImage(this.m_imageFileName);
					}
				}
				else
				{
					if((ret=ImageCache.getImage(this.imageThumbSource, true)) == null)
					{
						// image will be retrieved as part of getImage()
					}
				}

				return ret;
			}
		}
	}
}
