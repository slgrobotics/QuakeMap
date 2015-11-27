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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

//* class clsFolderBrowser  {
//* 
//*    Author : Paulo S. Silva Jr. 
//*      Date : September 2002 
//* Objective : ; GAIS <= implement the ability of browse for folder on a .NET environment 
//* 
//* class Properties  {
//* 
//*   +------------+---------+-------------------------------------------------+ 
//*   | Name       | Type    | Description                                     | 
//*   +------------+---------+-------------------------------------------------+ 
//*   | Title      | string   | Information to be displayed on the dialog      | 
//*   | NewUI      | bool | Informs the class to use the new user interface    | 
//*   | ShowStatus | bool | if ( TRUE show a text with the full path info      | 
//*   +------------+---------+-------------------------------------------------+ 
//* 
//* class Methods  {
//* 
//*   +-------------------+----------------------------------------------------+ 
//*   | Name              | Description                                        | 
//*   +-------------------+----------------------------------------------------+ 
//*   | BrowseForComputer | Parameters : None                                  | 
//*   |                   |                                                    | 
//*   |                   | Display a dialog that allows the user browse for a | 
//*   |                   | computer over a network.                           | 
//*   +-------------------+----------------------------------------------------+ 
//*   | BrowseForFolder   | Parameters : None                                  | 
//*   |                   |                                                    | 
//*   |                   | Display a dialog that allows the user browse for a | 
//*   |                   | folder.                                            | 
//*   +-------------------+----------------------------------------------------+ 
//*   | BrowseForFolder   | Parameters : StartPath as string                   | 
//*   |                   |                                                    | 
//*   |                   | Display a dialog that allows the user browse for a | 
//*   |                   | folder, starting from a specified path.            | 
//*   +-------------------+----------------------------------------------------+ 
//*   | BrowseForFolder   | Parameters : StartLocation as CSIDL                | 
//*   |                   |                                                    | 
//*   |                   | Display a dialog that allows the user browse for a | 
//*   |                   | folder, starting from a specified location.        | 
//*   +-------------------+----------------------------------------------------+ 
//*   | BrowseForFile     | Parameters : None                                  | 
//*   |                   |                                                    | 
//*   |                   | Display a dialog that allows the user browse for a | 
//*   |                   | file.                                              | 
//*   +-------------------+----------------------------------------------------+ 
//*   | BrowseForFile     | Parameters : StartPath as string                   | 
//*   |                   |                                                    | 
//*   |                   | Display a dialog that allows the user browse for a | 
//*   |                   | file, starting from a specified path.              | 
//*   +-------------------+----------------------------------------------------+ 
//*   | BrowseForFile     | Parameters : StartLocation as CSIDL                | 
//*   |                   |                                                    | 
//*   |                   | Display a dialog that allows the user browse for a | 
//*   |                   | file, starting from a specified location.          | 
//*   +-------------------+----------------------------------------------------+ 
//* 

public class clsFolderBrowser
{

#region Local Constants

	//* 
	//* Constants used by the Callback
	//* 
	private const int BFFM_INITIALIZED = 1;
	private const int BFFM_SELCHANGED = 2;
	private const int BFFM_VALIDATEFAILED = 3;
	private const int BFFM_ENABLEOK = 0x465;
	private const int BFFM_SETSELECTIONA = 0x466;
	private const int BFFM_SETSTATUSTEXT = 0x464;

	//* 
	//* Constants used to specify the type of browsing 
	//* 
	private const short BIF_EDITBOX = 0x10;
	private const short BIF_VALIDATE = 0x20;
	private const short BIF_STATUSTEXT = 0x4;
	private const short BIF_NEWDIALOGSTYLE = 0x40;
	private const short BIF_DONTGOBELOWDOMAIN = 0x2;

	private const short BIF_RETURNONLYFSDIRS = 0x1;
	private const short BIF_RETURNFSANCESTORS = 0x8;

	private const short BIF_BROWSEFORPRINTER = 0x2000;
	private const short BIF_BROWSEFORCOMPUTER = 0x1000;

	private const short BIF_BROWSEINCLUDEFILES = 0x4000;

	//* 
	//* Maximum size of a string for a path 
	//* 
	private const short MAX_PATH = 260;

#endregion

#region Local Enumeration and Structures

	//* 
	//* These are the values for special folders in a Windows environment 
	//* 
	public enum CSIDL
	{
		ADMINTOOLS = 0x30,
		ALTSTARTUP = 0x1D,
		APPDATA = 0x1A,
		BITBUCKET = 0xA,
		CDBURN_AREA = 0x3B,
		COMMON_ADMINTOOLS = 0x2F,
		COMMON_ALTSTARTUP = 0x1E,
		COMMON_APPDATA = 0x23,
		COMMON_DESKTOPDIRECTORY = 0x19,
		COMMON_DOCUMENTS = 0x2E,
		COMMON_FAVORITES = 0x1F,
		COMMON_MUSIC = 0x35,
		COMMON_OEM_LINKS = 0x3A,
		COMMON_PICTURES = 0x36,
		COMMON_PROGRAMS = 0x17,
		COMMON_STARTMENU = 0x16,
		COMMON_STARTUP = 0x18,
		COMMON_TEMPLATES = 0x2D,
		COMMON_VIDEO = 0x37,
		COMPUTERSNEARME = 0x3D,
		CONNECTIONS = 0x31,
		CONTROLS = 0x3,
		COOKIES = 0x21,
		DESKTOP = 0x0,
		DESKTOPDIRECTORY = 0x10,
		DRIVES = 0x11,
		FAVORITES = 0x6,
		FLAG_CREATE = 0x8000,
		FLAG_DONT_VERIFY = 0x4000,
		FLAG_MASK = 0xFF00,
		FLAG_NO_ALIAS = 0x1000,
		FLAG_PER_USER_INIT = 0x800,
		FONTS = 0x14,
		HISTORY = 0x22,
		INTERNET = 0x1,
		INTERNET_CACHE = 0x20,
		LOCAL_APPDATA = 0x1C,
		MYDOCUMENTS = 0xC,
		MYMUSIC = 0xD,
		MYPICTURES = 0x27,
		MYVIDEO = 0xE,
		NETHOOD = 0x13,
		NETWORK = 0x12,
		PERSONAL = 0x5,
		PRINTERS = 0x4,
		PRINTHOOD = 0x1B,
		PROFILE = 0x28,
		PROGRAM_FILES = 0x26,
		PROGRAM_FILES_COMMON = 0x2B,
		PROGRAM_FILES_COMMONX86 = 0x2C,
		PROGRAM_FILESX86 = 0x2A,
		PROGRAMS = 0x2,
		RECENT = 0x8,
		RESOURCES = 0x38,
		RESOURCES_LOCALIZED = 0x39,
		SENDTO = 0x9,
		STARTMENU = 0xB,
		STARTUP = 0x7,
		SYSTEM = 0x25,
		SYSTEMX86 = 0x29,
		TEMPLATES = 0x15,
		WINDOWS = 0x24
	}

	//* 
	//* struct for Browsing 
	//* 
	private struct BROWSEINFO 
	{
		public IntPtr hOwner;
		public int pidlRoot;
		public string  pszDisplayName;
		public string  lpszTitle;
		public int ulFlags;
		public fbCallBack lpfn;
		public IntPtr lParam;
		public int iImage;
	}

#endregion

#region Local DLL Declarations - .NET style

	//* 
	//* Delegate used as a pointer for the real callback function {
	//* 
	private delegate int fbCallBack(  IntPtr hWnd,  int uMsg,  int lParam,  int lpData);

	[DllImport("ole32.dll")]
	private static extern void CoTaskMemFree( IntPtr addr);

	[DllImport("user32.dll")]
	private  static extern int SendMessage(  IntPtr hWnd,  int uMsg,  int lParam,  int lpData);

	[DllImport("user32.dll")]
	private  static extern int SendMessage(  IntPtr hWnd,  int uMsg,  int lParam,  string  lpData);

	[DllImport("shell32.dll", CharSet=CharSet.Ansi)]
	private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpBrowseInfo);

	[DllImport("shell32.dll", CharSet=CharSet.Ansi)]
	private static extern int SHGetPathFromIDList(  IntPtr pidl,  StringBuilder pszPath);

	[DllImport("shell32.dll", CharSet=CharSet.Ansi)]
	private static extern int SHGetSpecialFolderLocation(  IntPtr hWnd,  int nFolder, ref int pidl);

#endregion

#region Local Variables

	private bool flgInit;
	private bool flgNewUI = true;
	private BROWSEINFO stcBrowseInfo;
	private bool flgShowStatus = false;

#endregion

#region Local Functions

	//* 
	//* Displays the dialog, based on information stored on a BrowseInfo struct 
	//* 
	private string  DoBrowse( string  StartPath)
	{

		IntPtr iprtSelectDir;
		string  strDirSelect;

		//* 
		//* Add the proper flags 
		//* 
		if ( flgNewUI ) { stcBrowseInfo.ulFlags += BIF_NEWDIALOGSTYLE; }
		if ( flgShowStatus ) { stcBrowseInfo.ulFlags += BIF_STATUSTEXT; }

		//* 
		//* Identify for the Callback that it was initialized {
		//* 
		flgInit = true;

		//* 
		//* Specify the StartPath for SHBrowseForFolder 
		//* 
		stcBrowseInfo.lParam = Marshal.StringToHGlobalAnsi(StartPath);

		//* 
		//* Fill pszDisplayName with spaces so SHBrowseForFolder can return a computer name 
		//* 
		StringBuilder  strTmp = new StringBuilder(MAX_PATH);
		strTmp.Insert(0, " ", MAX_PATH);

		stcBrowseInfo.pszDisplayName = strTmp.ToString();

		iprtSelectDir = SHBrowseForFolder(ref stcBrowseInfo);

		//* 
		//* if ( the selected item is a computer, the iprtSelectDir returns 
		//* no dir but the pszDisplayName contains the computer name. 
		//* 
		if ( "".Equals(GetFSPath(iprtSelectDir)) ) 
		{
			strDirSelect = "\\" + stcBrowseInfo.pszDisplayName.Trim();
		} 
		else 
		{
			strDirSelect = GetFSPath(iprtSelectDir);
		}

		//* 
		//* Free the resource allocated by SHBrowseForFolder 
		//* 
		CoTaskMemFree(iprtSelectDir);

		//* 
		//* return the selected Dir or Computer 
		//* 
		return strDirSelect;

	}

	//* 
	//* SHBrowseForFolder Callback function 
	//* 
	private int BrowseCallbackProc(  IntPtr hWnd,  int uMsg,  int lParam,  int lpData)
	{
		if ( uMsg == BFFM_INITIALIZED ) 
		{
			SendMessage(hWnd, BFFM_SETSELECTIONA, 1, lpData);
			flgInit = false;
		} 
		else if ( uMsg == BFFM_SELCHANGED && ! flgInit ) 
		{
			SendMessage(hWnd, BFFM_SETSTATUSTEXT, 0, GetFSPath(new IntPtr(lParam)));
		}
		return 0;
	}

	//* 
	//* Returns the actual path from a pointer 
	//* 
	private string  GetFSPath( IntPtr pidl)
	{

		StringBuilder  strPath = new StringBuilder(MAX_PATH);

		//* 
		//* Checks if the pointer is invalid 
		//* 
		if ( pidl.Equals(IntPtr.Zero) )
		{
			return "";
		} 
		else 
		{
			//* 
			//* get { the actual path from the list 
			//* 
			if ( SHGetPathFromIDList(pidl, strPath) == 1 )
			{
				return strPath.ToString();
			}
			return "";
		}

	}

#endregion

#region public Procedures and Properties

	//* 
	//* class Constructor  {
	//* 
	public clsFolderBrowser( IntPtr Handle, string title)
	{
		stcBrowseInfo.hOwner = Handle;
		stcBrowseInfo.lpfn = new fbCallBack(BrowseCallbackProc);
		Title = title;
	}

#region Browse for Computers
	//* 
	//* Displays a common dialog and selects only computers over a Network 
	//* 
	public string  BrowseForComputers()
	{

		SHGetSpecialFolderLocation(stcBrowseInfo.hOwner, (int)CSIDL.NETWORK, ref stcBrowseInfo.pidlRoot);
		stcBrowseInfo.ulFlags = BIF_BROWSEFORCOMPUTER;
		return DoBrowse("");

	}
#endregion

#region " Browse for Folder "
	//* 
	//* Displays a dialog that allows the user select a folder 
	//* 
	public  string  BrowseForFolder()
	{

		stcBrowseInfo.pidlRoot = 0;
		stcBrowseInfo.ulFlags = BIF_RETURNONLYFSDIRS;
		return DoBrowse("");

	}

	//* 
	//* Displays a dialog that allows the user select a folder, starting from a specified dir. 
	//* 
	public  string  BrowseForFolder( string  StartPath)
	{

		stcBrowseInfo.pidlRoot = 0;
		stcBrowseInfo.ulFlags = BIF_RETURNONLYFSDIRS;
		return DoBrowse(StartPath);

	}

	//* 
	//* Displays a dialog that allows the user select a folder, starting from a specified CSIDL 
	//* 
	public  string  BrowseForFolder( CSIDL StartLocation)
	{

		SHGetSpecialFolderLocation(stcBrowseInfo.hOwner, (int)StartLocation, ref stcBrowseInfo.pidlRoot);
		stcBrowseInfo.ulFlags = BIF_RETURNONLYFSDIRS;
		return DoBrowse("");

	}

#endregion

#region Browse for Files

	//* 
	//* Displays a dialog that allows the user select a file. 
	//* 
	public  string  BrowseForFiles() 
	{

		stcBrowseInfo.pidlRoot = 0;
		stcBrowseInfo.ulFlags = BIF_BROWSEINCLUDEFILES;
		return DoBrowse("");

	}

	//* 
	//* Displays a dialog that allows the user select a file, starting from a specified dir. 
	//* 
	public  string  BrowseForFiles( string  StartPath) 
	{

		stcBrowseInfo.pidlRoot = 0;
		stcBrowseInfo.ulFlags = BIF_BROWSEINCLUDEFILES;
		return DoBrowse(StartPath);

	}

	//* 
	//* Displays a dialog that allows the user select a file, starting from a specified CSIDL. 
	//* 
	public  string  BrowseForFiles( CSIDL StartLocation) 
	{

		SHGetSpecialFolderLocation(stcBrowseInfo.hOwner, (int)StartLocation, ref stcBrowseInfo.pidlRoot);
		stcBrowseInfo.ulFlags = BIF_BROWSEINCLUDEFILES;
		return DoBrowse("");

	}

#endregion

#region public Properties

	//* 
	//* Specifies the Title to by shown on the Browse for Folder dialog 
	//* 
	public string  Title  
	{

		get 
		{
			return stcBrowseInfo.lpszTitle;
		}

		set 
		{
			stcBrowseInfo.lpszTitle = value;
		}

	}

	//* 
	//* Flag indicating if SHBrowseForFolder will use the new User Interface. 
	//* 
	public bool NewUI  
	{

		get 
		{
			return flgNewUI;
		}

		set 
		{
			flgNewUI = value;
		}

	}

	//* 
	//* Flag indicating if SHBrowseForFolder will show the Status. 
	//* 
	public bool ShowStatus  
	{

		get 
		{
			return flgShowStatus;
		}

		set 
		{
			flgShowStatus = value;
		}

	}

#endregion

#endregion

}

