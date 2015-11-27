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
using System.IO;
using System.Collections;

/*
	Test code:
					using System;
					using System.IO;

					namespace PalmTestCsharp
					{
						class Class1
						{
							/// <summary>
							/// The main entry point for the application.
							/// </summary>
							[STAThread]
							static void Main(string[] args)
							{
								// source folder:
                                // "c:\Documents and Settings\<user>\Application Data\Camtrack"  or "C:\Users\Sergei\AppData\Roaming\QuakeMap\Camtrack"
								string camtrackFolderPath = "C:\\Program Files\\QuakeMap\\QuakeMap\\Camtrack";
								string destFolderPath = "C:\\Projects\\TilerPalm\\_fromAlex\\par utility\\par";

								int LINES_PER_CHUNK = 1; //400;

								// source file:
								string descrFilePath = Path.Combine(camtrackFolderPath, "descr.xml");

								// destination files:
								string descrPdbFilePath = Path.Combine(destFolderPath, "descr1.pdb");
								string imagesPdbFilePath = Path.Combine(destFolderPath, "palmapdata1.pdb");

								PalmPdbFormat ppf = new PalmPdbFormat();

								FileStream fsIn = new FileStream(descrFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
								TextReader textReader = new StreamReader(fsIn);

								// create a .pdb file from text, broken into chunks of reasonable size to fit into 32kb/record limitation:
								ppf.MakeTextPdb("descr", LINES_PER_CHUNK, textReader, descrPdbFilePath);

								// pack all .bmp files from the specified source folder into a .pdb file:
								ppf.PackImagesIntoPdb("palmapdata", camtrackFolderPath, imagesPdbFilePath);

								fsIn.Close();
							}
						}
					}
 */


namespace LibSys
{
	using DWORD		= System.UInt32;
	using WORD		= System.UInt16;

	#region ByteWriter
	public class ByteWriter
	{
		// fixed length string to bytes, padded with zeroes to the length or truncated to length if needed
		public static int writeString(byte[] bytes, int offset, string str, int length)
		{
			char[] chars = str.ToCharArray();
			int cnt = 0;
			foreach(char ch in chars)
			{
				bytes[offset + (cnt++)]   = (byte)ch;
				if(cnt > length)
				{
					break;
				}
			}
			while(cnt < length)
			{
				bytes[offset + (cnt++)]   = 0;
			}
			return offset + length;
		}

		public static int writeString(byte[] bytes, int offset, string str)
		{
			int length = str.Length;

			ByteWriter.writeWord (bytes, 0, (ushort)length); 
			length = ByteWriter.writeString(bytes, 2, str, length);
			//while(length % 4 != 0)
			//{
			//	bytes[length++] = 0;
			//}
			return offset + length;
		}

		public static int writeUshort(byte[] bytes, int offset, ushort val)
		{
			bytes[offset]   = (byte)((val >> 8) & 0xFF);
			bytes[offset+1] = (byte)(val & 0xFF);

			offset += 2;
			return offset;
		}

		public static int writeWord(byte[] bytes, int offset, WORD val)
		{
			return writeUshort(bytes, offset, (ushort)val);
		}

		public static int writeDword(byte[] bytes, int offset, DWORD val)
		{
			bytes[offset]   = (byte)((val >> 24) & 0xFF);
			bytes[offset+1] = (byte)((val >> 16) & 0xFF);
			bytes[offset+2] = (byte)((val >> 8) & 0xFF);
			bytes[offset+3] = (byte)(val & 0xFF);

			offset += 4;
			return offset;
		}

	}
	#endregion

	#region PDBHeader

	public class PDBHeader : ByteWriter
	{ 
		public string	name; //		name : byte[32]; 
		public ushort	attributes; 
		public ushort	version; 
		public DWORD	creationDate;			// the number of seconds since '1904-01-01 00:00:00'
		public DWORD	modificationDate; 
		public DWORD	lastBackupDate; 
		public DWORD	modificationNumber; 
		public DWORD	appInfoID; 
		public DWORD	sortInfoID; 
		public string	type = "data"; 
		//public string	creator = "QmaP"; 
		public string	creator = "qmap"; 
		public DWORD	unqueIDSeed; 
		public DWORD	nextRecordistID; 
		public WORD		numRecords;

		public int Length = 78;	// just an estimate, better defined after write() is called

		public PDBHeader(string _name, int recCount)
		{
			DWORD dateNow = makeDate(DateTime.Now);

			name = _name;
			attributes = 0x08;
			version = 0x01;
			creationDate = dateNow;
			modificationDate = dateNow;
			lastBackupDate = dateNow;
			modificationNumber = 0x0006;
			appInfoID = 0x0000;
			sortInfoID = 0x0000;
			unqueIDSeed = 0x0000;
			nextRecordistID = 0x0000;
			numRecords = (ushort)recCount;
		}

		public void writeHeader(FileStream fsOut)
		{
			byte[] bytes = new byte[100];

			Length = 0;
			Length = writeString(bytes, Length, name, 32);
			Length = writeUshort(bytes, Length, attributes);
			Length = writeUshort(bytes, Length, version);
			Length = writeDword (bytes, Length, creationDate);
			Length = writeDword (bytes, Length, modificationDate);
			Length = writeDword (bytes, Length, lastBackupDate);
			Length = writeDword (bytes, Length, modificationNumber);
			Length = writeDword (bytes, Length, appInfoID);
			Length = writeDword (bytes, Length, sortInfoID);
			Length = writeString(bytes, Length, type, 4);
			Length = writeString(bytes, Length, creator, 4);
			Length = writeDword (bytes, Length, unqueIDSeed);
			Length = writeDword (bytes, Length, nextRecordistID);
			Length = writeWord  (bytes, Length, numRecords);

			fsOut.Write(bytes, 0, Length);
		}

		private int m_recordOffset = 0;
		private int m_firstRecAlign = 0;

		// must be called after write(), so that Length is defined:
		public void writeLocators(FileStream fsOut, ArrayList lengths, int totalCount, bool startFromScratch, bool doAlignment)
		{
			byte[] buffer = new Byte[8];

			if(startFromScratch)
			{
				m_firstRecAlign = 0;
				m_recordOffset = Length + totalCount * 8;
				while(m_recordOffset % 4 != 0)
				{
					m_recordOffset++;		// alignment - first record is word-aligned
					m_firstRecAlign++;
				}
			}
 
			for(int i=0; i < lengths.Count ;i++) 
			{ 
				int len = (int)lengths[i];
				ByteWriter.writeDword(buffer, 0, (uint)m_recordOffset); 
				ByteWriter.writeDword(buffer, 4, 0x40000000);				// 40H is database attribute 
				fsOut.Write(buffer, 0, 8);
				m_recordOffset += len;
				//while(m_recordOffset % 4 != 0)
				//{
				//	m_recordOffset++;
				//}
			}

			if(m_firstRecAlign != 0 && doAlignment)
			{
				ByteWriter.writeDword (buffer, 0, 0); 
				fsOut.Write(buffer, 0, m_firstRecAlign);		// alignment
			}
		}

		public DWORD makeDate(DateTime date)
		{
			// must be the number of seconds since '1904-01-01 00:00:00'

			DateTime timeZero = new DateTime(1904, 01, 01, 00, 00, 00);
			
			return (DWORD)(date - timeZero).Seconds;
		}
	}; 
	#endregion

	/// <summary>
	/// PalmPdbFormat makes specific .pdb files for Palmap.
	/// </summary>
	/// 
	public class PalmPdbFormat
	{
		private const string fileNamePrefix = "images//";

		public PalmPdbFormat()
		{
		}

		/*
		// create a .pdb file from text, broken into chunks of reasonable size to fit into 32kb/record limitation:
		public void MakeTextPdb(string name, int linesPerChunk, TextReader textReader, string pdbFilePath)
		{
			FileStream fsOut = new FileStream(pdbFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

			ArrayList chunks = new ArrayList();				// of string
			ArrayList chunkLengths = new ArrayList();		// of int, with 2 added for string length

			// make chunks of no more than LINES_PER_CHUNK lines and count them:
			int maxChunkLength = makeChunks(linesPerChunk, chunks, chunkLengths, textReader);

			// make and write header:
			PDBHeader header = new PDBHeader(name, chunks.Count);
			header.writeHeader(fsOut);

			// now write record locators, 8 bytes per each record, containing offsets of actual records:
			header.writeLocators(fsOut, chunkLengths, chunkLengths.Count, true, true);

			// write actual records as length-specified strings:
			byte[] buffer = new Byte[maxChunkLength + 2 + 4];		// 2 bytes for length, up to 4 for padding
			for(int i=0; i < chunks.Count ;i++) 
			{ 
				string _chunk = (string)chunks[i];
				int length = ByteWriter.writeString (buffer, 0, _chunk);
				fsOut.Write(buffer, 0, length);
			} 
			fsOut.Close();
		}

		// pack all .bmp files from specified folder into a .pdb file:
		public void PackImagesIntoPdb(string name, string srcFolderPath, string pdbFilePath)
		{
			FileStream fsOut = new FileStream(pdbFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

			ArrayList files = new ArrayList();				// of FileInfo of .bmp files
			ArrayList fileLengths = new ArrayList();		// of int, sizes of .bmp files involved

			int maxFileLength = makeImageArray(files, fileLengths, srcFolderPath);

			// make and write header:
			PDBHeader header = new PDBHeader(name, files.Count);
			header.writeHeader(fsOut);

			// now write record locators, 8 bytes per each record, containing offsets of actual records:
			header.writeLocators(fsOut, fileLengths, fileLengths.Count, true, true);

			// write actual records as length-specified strings (image name), followed by raw bmp data:
			byte[] buffer = new Byte[Math.Max(maxFileLength, 2 + 4 + 256)];		// 2 bytes for length, up to 4 for padding, 256 for image name
			for(int i=0; i < files.Count ;i++) 
			{ 
				FileInfo fi = (FileInfo)files[i];
				string imageName = fileNamePrefix + fi.Name;
				int length = ByteWriter.writeString (buffer, 0, imageName);
				fsOut.Write(buffer, 0, length);
				FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				length = fs.Read(buffer, 0, (int)fi.Length);
				fs.Close();
				fsOut.Write(buffer, 0, length);
			} 
			fsOut.Close();
		}
		*/

		// pack all .bmp files from specified folder, and the descr file into a .pdb file:
		public void PackImagesAndDescrIntoPdb(string name, string srcFolderPath, string pdbFilePath, int linesPerChunk, TextReader textReader)
		{
			FileStream fsOut = new FileStream(pdbFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

			// images:
			ArrayList files = new ArrayList();				// of FileInfo of .bmp or .jpg files
			ArrayList fileLengths = new ArrayList();		// of int, sizes of .bmp or .jpg files involved

			// descr:
			ArrayList chunks = new ArrayList();				// of string
			ArrayList chunkLengths = new ArrayList();		// of int, with 2 added for string length

			ArrayList cntLengths = new ArrayList();			// of int, just one counter at the end of pdb

			int maxFileLength = makeImageArray(files, fileLengths, srcFolderPath);

			// make chunks of no more than LINES_PER_CHUNK lines and count them:
			int maxChunkLength = makeChunks(linesPerChunk, chunks, chunkLengths, textReader);

			cntLengths.Add(2);		// just one ushort counter at the end of pdb

			int recCount = fileLengths.Count + chunkLengths.Count + cntLengths.Count;

			// make and write header:
			PDBHeader header = new PDBHeader(name, recCount);
			header.writeHeader(fsOut);

			// now write record locators, 8 bytes per each record, containing offsets of actual records:
			header.writeLocators(fsOut, fileLengths, recCount, true, false);	// start writing
			header.writeLocators(fsOut, chunkLengths, 0, false, false);			// continue writing
			header.writeLocators(fsOut, cntLengths, 0, false, true);			// continue writing, finish with alignment

			// write actual records as length-specified strings (image name), followed by raw bmp data:
			byte[] buffer = new Byte[Math.Max(maxFileLength, 2 + 4 + 256)];		// 2 bytes for length, up to 4 for padding, 256 for image name
			int length = 0;
			for(int i=0; i < files.Count ;i++) 
			{
				FileInfo fi = (FileInfo)files[i];
				string imageName = fileNamePrefix + fi.Name;
				length = ByteWriter.writeString (buffer, 0, imageName);
				fsOut.Write(buffer, 0, length);
				FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				length = fs.Read(buffer, 0, (int)fi.Length);
				fs.Close();
				fsOut.Write(buffer, 0, length);
			} 

			// write actual records as length-specified strings:
			buffer = new Byte[maxChunkLength + 2 + 4];		// 2 bytes for length, up to 4 for padding
			for(int i=0; i < chunks.Count ;i++) 
			{ 
				string _chunk = (string)chunks[i];
				length = ByteWriter.writeString (buffer, 0, _chunk);
				fsOut.Write(buffer, 0, length);
			}
 
			// write the number of image files inside the pdb as the last record:
			length = ByteWriter.writeUshort (buffer, 0, (ushort)files.Count); 
			fsOut.Write(buffer, 0, length);

			fsOut.Close();
		}

		// pack single image file into a .pdb file:
		public void PackImageIntoPdb(string name, string srcFilePath, string pdbFilePath, int format)
		{
			FileStream fsOut = new FileStream(pdbFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

			// images:
			ArrayList files = new ArrayList();				// of FileInfo of .bmp or .jpg files
			ArrayList fileLengths = new ArrayList();		// of int, sizes of .bmp or .jpg files involved

			ArrayList cntFormats = new ArrayList();			// of int, just one counter at the end of pdb

			int maxFileLength = makeSingleImageArray(files, fileLengths, srcFilePath);

			cntFormats.Add(2);		// just one ushort format indicator at the end of pdb

			int recCount = fileLengths.Count + cntFormats.Count;

			// make and write header:
			PDBHeader header = new PDBHeader(name, recCount);
			header.writeHeader(fsOut);

			// now write record locators, 8 bytes per each record, containing offsets of actual records:
			header.writeLocators(fsOut, fileLengths, recCount, true, false);	// start writing
			header.writeLocators(fsOut, cntFormats, 0, false, true);			// continue writing, finish with alignment

			// write actual records as length-specified strings (image name), followed by raw bmp data:
			byte[] buffer = new Byte[Math.Max(maxFileLength, 2 + 4 + 256)];		// 2 bytes for length, up to 4 for padding, 256 for image name
			int length = 0;
			for(int i=0; i < files.Count ;i++) 
			{
				FileInfo fi = (FileInfo)files[i];
				string imageName = fi.Name;
				length = ByteWriter.writeString (buffer, 0, imageName);
				fsOut.Write(buffer, 0, length);
				FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				length = fs.Read(buffer, 0, (int)fi.Length);
				fs.Close();
				fsOut.Write(buffer, 0, length);
			} 

			// write the number of image files inside the pdb as the last record:
			length = ByteWriter.writeUshort (buffer, 0, (ushort)format); 
			fsOut.Write(buffer, 0, length);

			fsOut.Close();
		}

		#region makeChunks and makeImageArray

		private int makeChunks(int linesPerChunk, ArrayList chunks, ArrayList chunkLengths, TextReader textReader)
		{
			int maxChunkLength = 0;
			int lineCounter = 0;
			string chunk = "";
			string str;
			bool inTag = false;
			while((str=textReader.ReadLine()) != null)
			{
				str = str.Trim();
				lineCounter++;

				if(str.StartsWith("<wpt") && !str.EndsWith("/>") && !str.EndsWith("</wpt>"))
				{
					inTag = true;
				}
				if(str.StartsWith("</wpt>"))
				{
					inTag = false;
				}

				chunk += str;
				if(lineCounter >= linesPerChunk && !inTag)
				{
					lineCounter = 0;
					chunks.Add(chunk);
					chunkLengths.Add(chunk.Length + 2);		// 2 bytes for string length
					maxChunkLength = Math.Max(maxChunkLength, chunk.Length);
					chunk = "";
				}
			}
			if(chunk.Length > 0)
			{
				chunks.Add(chunk);
				chunkLengths.Add(chunk.Length + 2);		// 2 bytes for string length
				maxChunkLength = Math.Max(maxChunkLength, chunk.Length);
			}
			return maxChunkLength;
		}

		private int makeImageArray(ArrayList files, ArrayList fileLengths, string srcFolderPath)
		{
			int maxFileLength = 0;

			string fileExt = "*.bmp";

			switch(Project.pdaExportImageFormat)
			{
				case 0:		// bmp websafe
					break;
				case 1:		// bmp optimized
					break;
				case 2:		// jpeg
					fileExt = "*.jpg";
					break;
			}

			if(srcFolderPath != null && Directory.Exists(srcFolderPath))	// just to make sure that we have right parameter
			{
				DirectoryInfo dirInfo = new DirectoryInfo(srcFolderPath);
				foreach(FileInfo fi in dirInfo.GetFiles(fileExt))
				{
					files.Add(fi);
					string inPdbName = fileNamePrefix + fi.Name;
					int recordLength = (int)(inPdbName.Length + 2 + fi.Length);
					fileLengths.Add(recordLength);
					if(fi.Length > maxFileLength)
					{
						maxFileLength = (int)fi.Length;
					}
				}
			}

			return maxFileLength;
		}

		private int makeSingleImageArray(ArrayList files, ArrayList fileLengths, string srcFilePath)
		{
			int maxFileLength = 0;

			if(srcFilePath != null && File.Exists(srcFilePath))	// just to make sure that we have right parameter
			{
				FileInfo fi = new FileInfo(srcFilePath);
				files.Add(fi);
				string inPdbName = fi.Name;
				int recordLength = (int)(inPdbName.Length + 2 + fi.Length);
				fileLengths.Add(recordLength);
				maxFileLength = (int)fi.Length;
			}

			return maxFileLength;
		}

		#endregion
	}
}
