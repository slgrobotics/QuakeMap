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
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;

using LibGui;
using LibGeo;
using LibSys;

namespace LibGui
{
	/// <summary>
	/// CameraTrack saves XML-encoded frames to be loaded in PDA.
	/// for format of the XML see CameraManager, look for "Project.camTrackOn"
	/// </summary>
	public class CameraTrack
	{
		public static ArrayList camTrackTerraTiles = new ArrayList();		// of string - terra tiles names go here
		public static SnapList snapLng = null;
		public static SnapList snapLat = null;

		private string m_camtrackFolderPath;
		private string DESCR_FILE_NAME = "descr";
		private string m_descrFilePath;
		public string DescrFilePath { get { return m_descrFilePath; } }


		private XmlDocument xmlDoc = new XmlDocument();
		public XmlDocument XmlDoc { get { return xmlDoc; } }

		private XmlNode root;

		private XmlNode m_currentFrameNode;
		public XmlNode CurrentFrameNode { get { return m_currentFrameNode; } }

		public CameraTrack()
		{
		}

		public void init()
		{
			m_camtrackFolderPath = Project.GetCleanCamtrackFolder();	// removes content of the folder, if any

			m_descrFilePath = Path.Combine(m_camtrackFolderPath, DESCR_FILE_NAME + ".xml");

			string seedXml = "<camtrack/>";
			xmlDoc.LoadXml(seedXml);
			root = xmlDoc.DocumentElement;
			m_currentFrameNode = null;

			CameraTrack.camTrackTerraTiles.Clear();
			snapLng = null;	// will be allocated as the first tile arrives
			snapLat = null;
		}

		public static void AddTerraTile(string baseName, double lngPerTile, double latPerTile, TileTerra tile) 
		{
			if(snapLat == null)
			{
				snapLng = new SnapList(lngPerTile, false);
				snapLat = new SnapList(latPerTile, false);
			}
			if(!CameraTrack.camTrackTerraTiles.Contains(baseName))
			{
				CameraTrack.camTrackTerraTiles.Add(baseName);
				// adding to snap lists prepares them for calculating snap points:
				GeoCoord tl = tile.getTopLeft();
				GeoCoord br = tile.getBottomRight();
				snapLat.Add(tl.Lat);
				snapLat.Add(br.Lat);
				snapLng.Add(tl.Lng);
				snapLng.Add(br.Lng);
			}
		}

		// returns number of frames already in the xmldoc, including this one:
		public int log(XmlNode node)
		{
			//Project.SetValue(xmlDoc, root, "time", "" + DateTime.Now);
			if(node.Name.Equals("frame"))
			{
				root.AppendChild(node);
				m_currentFrameNode = node;
			}
			else
			{
				m_currentFrameNode.AppendChild(node);
			}
			return root.ChildNodes.Count;
		}

		#region Bmp conversion and Color Palette

		private static ColorPalette GetColorPalette( uint nColors )
		{
			// Assume monochrome image.
			PixelFormat     bitscolordepth = PixelFormat.Format1bppIndexed;
			ColorPalette    ret;    // The Palette we are stealing
			Bitmap          bitmap;     // The source of the stolen palette

			// Determine number of colors.
			if (nColors > 2)
				bitscolordepth = PixelFormat.Format4bppIndexed;
			if (nColors > 16)
				bitscolordepth = PixelFormat.Format8bppIndexed;

			// Make a new Bitmap object just to get its Palette.
			bitmap = new Bitmap( 1, 1, bitscolordepth );
			ret = bitmap.Palette;   // Grab the palette
			return ret;             // Send the palette back
		}

		private static void SaveBmpWithNewPalette(Image image, string filename, bool noCompression)
		{
			Bitmap  bitmap = null;
 
			if(noCompression)
			{
				// the resulting bmp's are 41kb
				switch(Project.pdaExportImageFormat)
				{
					case 0:
					{
						// 8-bit bmp with Palm websafe palette
						WebsafeQuantizer quantizer = new WebsafeQuantizer();
						bitmap = quantizer.Quantize ( image );
					}
						break;
					default:			// JPEG never happens here
					case 1:
					{
						// 8-bit bmp with individually optimized palette, good for full-color PocketPC devices.
						// each tile will have a different palette.
						OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
						bitmap = quantizer.Quantize ( image );
					}
						break;
				}
			}
			else if(Project.pdaExportUseWebsafeGrayscalePalette)
			{
				// still no compression, as the requested format uses 8-bit bmp with Palm websafe palette, out of which the greyscale portion is used.
				// the resulting bmp's are 41kb
				WebsafeGrayscaleQuantizer quantizer = new WebsafeGrayscaleQuantizer();
				bitmap = quantizer.Quantize ( image );
			}
			else
			{
				// Here is a block that compresses the bitmap into a 16-color 4-bit greyscale 20kb bmp files, saving space in pdb.
				// these bmps are real slow to display on Palm, as they need to be converted to current palette there.

				// see http://support.microsoft.com/default.aspx?scid=kb%3ben-us%3b319061 for MSDN KB article oc changing the depth

				int   Width = image.Width;
				int   Height = image.Height;

				// Because GetPixel isn't defined on an Image, make a copy in a Bitmap instead.
				// Use PixelFormat32BppARGB so you can wrap a Graphics around it.
				Bitmap BmpCopy = new Bitmap(Width, Height, PixelFormat.Format32bppArgb); 
				{
					Graphics g = Graphics.FromImage(BmpCopy);
					g.PageUnit = GraphicsUnit.Pixel;
					g.DrawImage(image, 0, 0, Width, Height);
				}

				// we need palette for bpp4 conversion - for aerial images:
				ColorPalette palette = GetColorPalette( 16 );

				// Initialize a new color table with entries that are determined
				// by some optimal palette-finding algorithm; just use a grayscale here.
				// Actually a Palm 16-color greyscale palette is created below, 0=0xFFFFFF 15=0x000000
				for (uint i = 0; i < 16; i++)
				{
					uint Alpha = 0xFF;								// Colors are opaque.
					//uint Intensity = (15-i)*0xFF/(16-1);			// Even distribution. 
					uint Intensity = i*0xFF/(16-1);					// Even distribution. 

					// Create a gray scale.
					palette.Entries[i] = Color.FromArgb((int)Alpha, (int)Intensity, (int)Intensity, (int)Intensity);
				}

				// Make a new 4-BPP indexed bitmap that is the same size as the source image.
				PixelFormat pf = PixelFormat.Format4bppIndexed;
				bitmap = new Bitmap(Width, Height, pf); 
				bitmap.Palette = palette;	// Set the palette into the new Bitmap object.

				// Lock a rectangular portion of the bitmap for writing.
				BitmapData  bitmapData;
				Rectangle   rect = new Rectangle(0, 0, Width, Height);

				bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, pf);

				// Write to the temporary buffer that is provided by LockBits.
				// Copy the pixels from the source image in this loop.
				// Because you want an index, convert RGB to the appropriate palette index here.

				IntPtr pixels = bitmapData.Scan0;

				unsafe 
				{ 
					// Get the pointer to the image bits.
					byte *  pBits;
					if (bitmapData.Stride > 0)
					{
						pBits = (byte *)pixels.ToPointer();
					}
					else
					{
						// If the Stide is negative, Scan0 points to the last 
						// scanline in the buffer. To normalize the loop, obtain
						// a pointer to the front of the buffer that is located 
						// (Height-1) scanlines previous.
						pBits = (byte *)pixels.ToPointer() - bitmapData.Stride*(Height-1);
					}
					uint stride = (uint)Math.Abs(bitmapData.Stride);

					for ( uint row = 0; row < Height; ++row )
					{
						for ( uint col = 0; col < stride; ++col )		// stride is 100 while Width is 200
						{
							// Map palette indexes for a gray scale.
							// If you use some other technique to color convert,
							// put your favorite color reduction algorithm here.
							int colStep = 2;
							Color     pixel1 = BmpCopy.GetPixel((int)col*colStep, (int)row);
							Color     pixel2 = BmpCopy.GetPixel((int)col*colStep, (int)row);

							// Use luminance/chrominance conversion to get grayscale.
							// Basically, turn the image into black and white TV.
							// Do not calculate Cr or Cb because you discard the color anyway.
							// Y = Red * 0.299 + Green * 0.587 + Blue * 0.114

							// This expression is best as integer math for performance,
							// however, because GetPixel listed earlier is the slowest 
							// part of this loop, the expression is left as floating point for clarity.

							//double luminance = (pixel.R *0.299) + (pixel.G *0.587) + (pixel.B *0.114);
							double luminance1 = pixel1.R *0.33 + pixel1.G *0.33 + pixel1.B *0.33;
							double luminance2 = pixel2.R *0.33 + pixel2.G *0.33 + pixel2.B *0.33;

							// these will become quads:
							byte lum1 = (byte)(luminance1 * (16-1)/255 + 0.5);
							byte lum2 = (byte)(luminance2 * (16-1)/255 + 0.5);

							// there is a trick here: the two quads below should be indexes into palette,
							// but we assume that the palette is linear so that index and value are the same.

							// pack the two quads into a byte:
							byte pixValue = (byte)((lum1 & 0xF) + ((lum2 & 0xF) << 4));

							byte * pBytePixel = pBits + row*stride + col;
							*pBytePixel = pixValue;

						} // end loop for col 
					} // end loop for row

				} // end unsafe

				// To commit the changes, unlock the portion of the bitmap.  
				bitmap.UnlockBits(bitmapData);
			}

			bitmap.Save(filename, ImageFormat.Bmp);

			// Bitmap goes out of scope here and is also marked for garbage collection.
			// BmpCopy goes out of scope here and is marked for garbage collection.
		}
		#endregion

		/*
		// goes through all selected nodes and snaps "topleft" and "bottomright" to the grid:
		private void snapCoords(XmlDocument xmlDoc, string xpath)
		{
			char[] sep = new char[] { ',' };

			foreach (XmlNode node in xmlDoc.SelectNodes(xpath))
			{
				foreach (XmlNode attr in node.Attributes)
				{
					try
					{
						//LibSys.StatusBar.Trace("attr:" + attr.Name + "=" + attr.InnerText);
						switch(attr.Name)
						{
							case "topleft":
							case "bottomright":
							{
								//string before = attr.Name + "=" + attr.InnerText;
								string[] coords = attr.InnerText.Split(sep);
								double lng = Convert.ToDouble(coords[0]);
								double lat = Convert.ToDouble(coords[1]);
								float flng = (float)snapLng.snap(lng);
								float flat = (float)snapLat.snap(lat);
								attr.InnerText = "" + flng + "," + flat;
								/*
								string after = attr.Name + "=" + attr.InnerText;
								if(!before.Equals(after))
								{
									LibSys.StatusBar.Trace("before : " + before);
									LibSys.StatusBar.Trace("after  : " + after);
								}
								* /
							}
								break;
						}
					} 
					catch {}
				}
			}
		}
		*/

		/// <summary>
		/// for a frame node produces really long string with all names of tiles
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private string nodeFootprint(XmlNode node)
		{
			string ret = "";

			if(node.HasChildNodes)
			{
				foreach(XmlNode nnode in node.ChildNodes)
				{
					ret += nodeFootprint(nnode);
				}
			}
			
			ret += node.InnerText + " ";

			return ret;
		}

		// must be called before save() to prepare Camtrack descr.xml and *.bmp files. May throw exception.
		public void savePrepare()
		{
			// remove all empty frame nodes, resulting from CameraManager.processCameraMove()
			// being called and TileSetTerra.ReTile() not called (or failing before call to CameraTrack.log()
			again:
				foreach(XmlNode node in root.ChildNodes)
				{
					if("frame".Equals(node.Name) && (!node.HasChildNodes || node.ChildNodes.Count < 2))
					{
						root.RemoveChild(node);
						goto again;
					}
				}

			// remove identical frame nodes:
			again2:
				foreach(XmlNode node in root.ChildNodes)
				{
					if("frame".Equals(node.Name))
					{
						string footprint = nodeFootprint(node);
						foreach(XmlNode otherNode in root.ChildNodes)
						{
							if(node != otherNode && "frame".Equals(node.Name) &&  footprint.Equals(nodeFootprint(otherNode)))
							{
								root.RemoveChild(otherNode);
								goto again2;
							}
						}
					}
				}

			/* this is the version that cuts descr file into frames:
			ArrayList savedNodes = new ArrayList();
			foreach (XmlNode frame in xmlDoc.DocumentElement.ChildNodes)
			{
				savedNodes.Add(frame);
			}

			int i = 0;
			foreach (XmlNode frame in savedNodes)
			{
				xmlDoc.RemoveAll();
				string seedXml = "<camtrack/>";
				xmlDoc.LoadXml(seedXml);
				//root = xmlDoc.DocumentElement;
				xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(frame, true));

				string descrFilePath = Path.Combine(folderPath, DESCR_FILE_NAME + i + ".xml");

				xmlDoc.Save(descrFilePath);
				i++;
			}
			*/

			// by this time snap lists are ready - we need just call snap() on every lat/long
			// in the xmldoc to snap all tiles' coordinates to grid:
			//snapCoords(xmlDoc, "/camtrack/frame");
			//snapCoords(xmlDoc, "/camtrack/frame/terratiles/terratile");

			m_camtrackFolderPath = Project.GetCleanCamtrackFolder();	// removes content of the folder, if any

			root.Attributes.RemoveAll();
			XmlAttribute attr = xmlDoc.CreateAttribute("version");
			attr.InnerText = Project.MAPADVISOR_FORMAT_VERSION;
			root.Attributes.Append(attr);

			switch(Project.pdaExportImageFormat)
			{
				case 0:		// bmp websafe
					break;
				case 1:		// bmp optimized
					break;
				case 2:		// jpeg
					attr = xmlDoc.CreateAttribute("format");
					attr.InnerText = "jpg";
					root.Attributes.Append(attr);
					break;
			}

			// ok, data in descriptor is good to go, save it:
			xmlDoc.Save(m_descrFilePath);

			// convert and save all tiles:
			foreach(string tileName in CameraTrack.camTrackTerraTiles)
			{
				if(!tileName.StartsWith("empty"))
				{
					string fileNameJpg = tileName + ".jpg";
					string srcPath = Project.GetTerraserverPath(fileNameJpg);

					if(File.Exists(srcPath))
					{
						try 
						{
							convertAndSaveTileImage(tileName, srcPath, m_camtrackFolderPath);
						}
						catch (Exception ee)
						{
#if DEBUG
							// some files may be broken/missing, not a big deal
							LibSys.StatusBar.Error("CameraTrack:savePrepare() " + ee.Message);
#endif
						}
					}
				}
			}
		}

		public static string convertAndSaveTileImage(string tileName, string srcFilePath, string folderPath)
		{
			string ret = null;

			switch(Project.pdaExportImageFormat)
			{
				case 0:		// bmp websafe
				case 1:		// bmp optimized
				{
					// bmp files need conversion (from jpeg) and proper palette:
					string fileNameBmp = tileName + ".bmp";
					string dstPath = Path.Combine(folderPath, fileNameBmp);
					Image image = Image.FromFile(srcFilePath);
					if(tileName.StartsWith("T1"))
					{
						// aerial map - use bpp4 and grayscale palette:
						SaveBmpWithNewPalette(image, dstPath, false);
					}
					else if(tileName.StartsWith("T4"))
					{
						// color aerial map - use bpp8 and color palette:
						SaveBmpWithNewPalette(image, dstPath, true);
					}
					else	// probably starts with T2 being a topo
					{
						// topo map - preserve colors with bpp8:
						//image.Save(dstPath, ImageFormat.Bmp);		// topos are 40kb 8bit images
						SaveBmpWithNewPalette(image, dstPath, true);
					}
					ret = dstPath;
				}
					break;
				case 2:		// jpeg - just copy it to camtrack folder
				{
					string fileNameDst = tileName + ".jpg";
					string dstPath = Path.Combine(folderPath, fileNameDst);

					File.Copy(srcFilePath, dstPath, true);
					ret = dstPath;
				}
					break;
			}
			return ret;
		}

		// takes destination names for .pdb files. May throw file-related exceptions.
		public void save(string catalogName, string descrPdbFilePath)	//, string imagesPdbFilePath)
		{
			int LINES_PER_CHUNK = 200;

			PalmPdbFormat ppf = new PalmPdbFormat();

			FileStream fsIn = new FileStream(m_descrFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			TextReader textReader = new StreamReader(fsIn);

			/*
			// create a .pdb file from text, broken into chunks of reasonable size to fit into 32kb/record limitation:
			ppf.MakeTextPdb("descr", LINES_PER_CHUNK, textReader, descrPdbFilePath);

			fsIn.Close();

			// pack all .bmp files from the specified source folder into a .pdb file:
			ppf.PackImagesIntoPdb(catalogName, m_camtrackFolderPath, imagesPdbFilePath);
			*/

			ppf.PackImagesAndDescrIntoPdb(catalogName, m_camtrackFolderPath, descrPdbFilePath, LINES_PER_CHUNK, textReader);

			fsIn.Close();
		}

		// can work only after savePrepare()
		private long FolderSize()
		{
			long ret = 0;

			try
			{
				ret += new FileInfo(m_descrFilePath).Length;
			}
			catch
			{
			}

			foreach(string tileName in CameraTrack.camTrackTerraTiles)
			{
				string fileName = tileName + (Project.pdaExportImageFormat == 2 ? ".jpg" : ".bmp");
				string srcPath = Path.Combine(m_camtrackFolderPath, fileName);
				try 
				{
					ret += new FileInfo(srcPath).Length;
				}
				catch
				{
				}
			}
			return ret;
		}

		public override string ToString()
		{
			string ret = "";
			
#if DEBUG
			ret = "Folder: " + m_camtrackFolderPath + "\r\n";
#endif

			ret += "Frames: " + root.ChildNodes.Count + "\r\n";

			long fs = FolderSize();
			ret += "Data size: " + (fs / 1024L) + " Kb  (" + fs + " bytes)\r\n";

			string compressed = "";

			if(!Project.pdaExportUseWebsafeGrayscalePalette && "aerial".Equals(Project.drawTerraserverMode))
			{
				compressed = " - compressed";
			}

			switch(Project.pdaExportImageFormat)
			{
				case 0:
					ret += "Format: BMP (Palm colors)" + compressed + "\r\n";
					break;
				case 1:
					ret += "Format: BMP (PocketPC colors)" + compressed + "\r\n";
					break;
				case 2:
					ret += "Format: JPEG (original colors)\r\n";
					break;
			}

			return ret;
		}
	}
}
