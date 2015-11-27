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
using System.Security;
using System.Collections;
using Microsoft.Win32;

// original: The Org.Mentalis.Utilities namespace implements several useful utilities that are missing from the standard .NET framework.

namespace LibSys
{

	/// <summary>Creates file associations for your programs.</summary>
	/// <example>The following example creates a file association for the type XYZ with a non-existent program.
	/// <br></br><br>VB.NET code</br>
	/// <code>
	/// Dim FA as New FileAssociation
	/// FA.Extension = "xyz"
	/// FA.ContentType = "application/myprogram"
	/// FA.FullName = "My XYZ Files!"
	/// FA.ProperName = "XYZ File"
	/// FA.AddCommand("open", "C:\mydir\myprog.exe %1")
	/// FA.Create
	/// </code>
	/// <br>C# code</br>
	/// <code>
	/// FileAssociation FA = new FileAssociation();
	/// FA.Extension = "xyz";
	/// FA.ContentType = "application/myprogram";
	/// FA.FullName = "My XYZ Files!";
	/// FA.ProperName = "XYZ File";
	/// FA.AddCommand("open", "C:\\mydir\\myprog.exe %1");
	/// FA.Create();
	/// </code>
	/// </example>
	public class FileAssociation 
	{
		/// <summary>Initializes an instance of the FileAssociation class.</summary>
		public FileAssociation() 
		{
			FileInfo = new FileType();
			FileInfo.Commands.Captions = new ArrayList();
			FileInfo.Commands.Commands = new ArrayList();
		}
		/// <summary>Gets or sets the proper name of the file type.</summary>
		/// <value>A String representing the proper name of the file type.</value>
		public string ProperName 
		{
			get 
			{
				return FileInfo.ProperName;
			}
			set 
			{
				FileInfo.ProperName = value;
			}
		}
		/// <summary>Gets or sets the full name of the file type.</summary>
		/// <value>A String representing the full name of the file type.</value>
		public string FullName 
		{
			get 
			{
				return FileInfo.FullName;
			}
			set 
			{
				FileInfo.FullName = value;
			}
		}
		/// <summary>Gets or sets the content type of the file type.</summary>
		/// <value>A String representing the content type of the file type.</value>
		public string ContentType 
		{
			get 
			{
				return FileInfo.ContentType;
			}
			set 
			{
				FileInfo.ContentType = value;
			}
		}
		/// <summary>Gets or sets the extension of the file type.</summary>
		/// <value>A String representing the extension of the file type.</value>
		/// <remarks>If the extension doesn't start with a dot ("."), a dot is automatically added.</remarks>
		public string Extension 
		{
			get 
			{
				return FileInfo.Extension;
			}
			set 
			{
				if (value.Substring(0, 1) != ".")
					value = "." + value;
				FileInfo.Extension = value;
			}
		}
		/// <summary>Gets or sets the index of the icon of the file type.</summary>
		/// <value>A short representing the index of the icon of the file type.</value>
		public short IconIndex 
		{
			get 
			{
				return FileInfo.IconIndex;
			}
			set 
			{
				FileInfo.IconIndex = value;
			}
		}
		/// <summary>Gets or sets the path of the resource that contains the icon for the file type.</summary>
		/// <value>A String representing the path of the resource that contains the icon for the file type.</value>
		/// <remarks>This resource can be an executable or a DLL.</remarks>
		public string IconPath 
		{
			get 
			{
				return FileInfo.IconPath;
			}
			set 
			{
				FileInfo.IconPath = value;
			}
		}
		/// <summary>Adds a new command to the command list.</summary>
		/// <param name="Caption">The name of the command.</param>
		/// <param name="Command">The command to execute.</param>
		/// <exceptions cref="ArgumentNullException">Caption -or- Command is null (VB.NET: Nothing).</exceptions>
		public void AddCommand(string Caption, string Command) 
		{
			if (Caption == null || Command == null)
				throw new ArgumentNullException();
			FileInfo.Commands.Captions.Add(Caption);
			FileInfo.Commands.Commands.Add(Command);
		}

		/// <summary>
		/// tests if the association is already in place. All it needs is Extension and Command/open
		/// </summary>
		/// <returns></returns>
		public bool Test()
		{
			if (Extension == null)
				throw new ArgumentNullException();
			if (Extension == "")
				throw new ArgumentException();

			try
			{
				RegistryKey RegKey = Registry.ClassesRoot.OpenSubKey(Extension);
				string properName = "" + RegKey.GetValue("");
				if(RegKey == null || properName.Length == 0)
				{
					return false;	// Extension not there
				}
				RegKey = Registry.ClassesRoot.OpenSubKey(properName);
				foreach(string subKeyName in RegKey.GetSubKeyNames())
				{
					if(subKeyName.Equals("Shell"))
					{
						RegistryKey subKey = RegKey.OpenSubKey("Shell");
						RegistryKey subSubKey = subKey.OpenSubKey("" + FileInfo.Commands.Captions[0]);			// "open"
						RegistryKey subSubSubKey = subSubKey.OpenSubKey("Command");
						string sVal = "" + subSubSubKey.GetValue("");
						string expected = "" + FileInfo.Commands.Commands[0];
						subSubSubKey.Close();
						subSubKey.Close();
						subKey.Close();
						if(sVal.Equals(expected))
						{
							return true;
						}
					}
				}
			} 
			catch {}

			return false;
		}

		/// <summary>Creates the file association.</summary>
		/// <exceptions cref="ArgumentNullException">Extension -or- ProperName is null (VB.NET: Nothing).</exceptions>
		/// <exceptions cref="ArgumentException">Extension -or- ProperName is empty.</exceptions>
		/// <exceptions cref="SecurityException">The user does not have registry write access.</exceptions>
		public void Create() 
		{
			// remove the extension to avoid incompatibilities [such as DDE links]
			try 
			{
				Remove();
			} 
			catch (ArgumentException) {} // the extension doesn't exist
			// create the exception
			if (Extension == "" || ProperName == "")
				throw new ArgumentException();
			int cnt;
			try 
			{
				RegistryKey RegKey = Registry.ClassesRoot.CreateSubKey(Extension);
				RegKey.SetValue("", ProperName);
				if (ContentType != null && ContentType != "")
					RegKey.SetValue("Content Type", ContentType);
				RegKey.Close();
				RegKey = Registry.ClassesRoot.CreateSubKey(ProperName);
				RegKey.SetValue("", FullName);
				RegKey.Close();
				if (IconPath != "") 
				{
					RegKey = Registry.ClassesRoot.CreateSubKey(ProperName + "\\" + "DefaultIcon");
					RegKey.SetValue("", IconPath + "," + IconIndex.ToString());
					RegKey.Close();
				}
				for (cnt = 0; cnt < FileInfo.Commands.Captions.Count; cnt++) 
				{
					RegKey = Registry.ClassesRoot.CreateSubKey(ProperName + "\\" + "Shell" + "\\" + (String)FileInfo.Commands.Captions[cnt]);
					RegKey = RegKey.CreateSubKey("Command");
					RegKey.SetValue("", FileInfo.Commands.Commands[cnt]);
					RegKey.Close();
				}
			} 
			catch 
			{
				throw new SecurityException();
			}
		}
		/// <summary>Removes the file association.</summary>
		/// <exceptions cref="ArgumentNullException">Extension -or- ProperName is null (VB.NET: Nothing).</exceptions>
		/// <exceptions cref="ArgumentException">Extension -or- ProperName is empty -or- the specified extension doesn't exist.</exceptions>
		/// <exceptions cref="SecurityException">The user does not have registry delete access.</exceptions>
		public void Remove() 
		{
			if (Extension == null || ProperName == null)
				throw new ArgumentNullException();
			if (Extension == "" || ProperName == "")
				throw new ArgumentException();
			Registry.ClassesRoot.DeleteSubKeyTree(Extension);
			Registry.ClassesRoot.DeleteSubKeyTree(ProperName);
		}
		/// <summary>Holds the properties of the file type.</summary>
		private FileType FileInfo;
	}

	/// <summary>List of commands.</summary>
	internal struct CommandList 
	{
		/// <summary>
		/// Holds the names of the commands.
		/// </summary>
		public ArrayList Captions;
		/// <summary>
		/// Holds the commands.
		/// </summary>
		public ArrayList Commands;
	}

	/// <summary>Properties of the file association.</summary>
	internal struct FileType 
	{
		/// <summary>
		/// Holds the command names and the commands.
		/// </summary>
		public CommandList Commands;
		/// <summary>
		/// Holds the extension of the file type.
		/// </summary>
		public string Extension;
		/// <summary>
		/// Holds the proper name of the file type.
		/// </summary>
		public string ProperName;
		/// <summary>
		/// Holds the full name of the file type.
		/// </summary>
		public string FullName;
		/// <summary>
		/// Holds the name of the content type of the file type.
		/// </summary>
		public string ContentType;
		/// <summary>
		/// Holds the path to the resource with the icon of this file type.
		/// </summary>
		public string IconPath;
		/// <summary>
		/// Holds the icon index in the resource file.
		/// </summary>
		public short IconIndex;
	}

}
