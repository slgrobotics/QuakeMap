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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Win32;
//using mshtml;
using SHDocVw;

// Lutz Roeders's .NET Reflector, October 2000.
// Copyright (C) 2000-2002 Lutz Roeder. All rights reserved.
// http://www.aisto.com/roeder/dotnet
// roeder@aisto.com

namespace LibSys
{
	/// <summary>
	/// Summary description for HtmlBrowser.
	/// </summary>
	public class HtmlBrowser : AxHost, IWebBrowserEvents
	{
		public event BrowserNavigateEventHandler BeforeNavigate;
		public event BrowserNavigateEventHandler NavigateComplete;
		IWebBrowser control = null;
		ConnectionPointCookie cookie;
		Boolean activate = false;
		String url = String.Empty;
		String html = String.Empty;
		String body = String.Empty;

		private ArrayList allocatedTempFiles = new ArrayList();

		public HtmlBrowser() : base("8856f961-340a-11d0-a96b-00c04fd705a2")
		{
			HandleCreated += new EventHandler(Me_HandleCreated);
			NavigateComplete += new BrowserNavigateEventHandler(Me_NavigateComplete);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			foreach(string file in allocatedTempFiles)
			{
				try
				{
					File.Delete(file);
				}
				catch {}
			}
			allocatedTempFiles.Clear();
			base.Dispose( disposing );
		}

		public void Activate()
		{
			activate = true;
		}

		public void Navigate(string url)
		{
			if (!IsHandleCreated)
			{
				this.url = url;
				return;
			}

			Object flags = 0;
			Object targetFrame = String.Empty;
			Object postData = String.Empty;
			Object headers = String.Empty;
			control.Navigate(url, ref flags, ref targetFrame, ref postData, ref headers);
		}

		public void SetHtmlText(string text)
		{
			this.html = text;
		}

		public void SetBodyText(string text)
		{
			if (control != null)
			{
				IHTMLDocument2 document = control.GetDocument();
				if (document != null)
				{
					IHTMLElement body = document.GetBody();
					if (body != null)
					{
						if (activate) DoVerb(-4);
						body.SetInnerHTML(text);
						return;
					}
				}
			}

			this.body = text;
		}

		/// <summary>
		/// Displays specific HTML in the web browser right away
		/// </summary>
		/// <param name="text">The HTML to show</param>
		public void DisplayHtml(string text)
		{
			url = Path.GetTempFileName();
			StreamWriter writer = new StreamWriter(url, false);
			writer.WriteLine(text);
			writer.Flush();
			writer.Close();
			allocatedTempFiles.Add(url);

			Navigate(url);
		}

		void Me_HandleCreated(Object s, EventArgs e)
		{
			HandleCreated -= new EventHandler(Me_HandleCreated);

			if (url == String.Empty)
			{
				url = Path.GetTempFileName();
				StreamWriter writer = new StreamWriter(url, false);
				writer.WriteLine(html);
				writer.Flush();
				writer.Close();
				allocatedTempFiles.Add(url);
			}

			Navigate(url);
		}

		void Me_NavigateComplete(Object s, BrowserNavigateEventArgs e)
		{
			if (activate) DoVerb(-4);

			if (html != String.Empty)
			{
				File.Delete(url);
				this.html = String.Empty;
			}

			if (body != String.Empty)
			{
				SetBodyText(body);
				this.body = String.Empty;
			}
		}

		protected override void CreateSink()
		{
			try 
			{
				cookie = new ConnectionPointCookie(GetOcx(), this,
					  typeof(IWebBrowserEvents)); } 
			catch { }
		}

		protected override void DetachSink()
		{
			try { cookie.Disconnect(); } 
			catch { }
		}

		protected override void AttachInterfaces()
		{
			try { control = (IWebBrowser) GetOcx(); } 
			catch { }
		}

		protected override Boolean IsInputKey(Keys keyData)
		{
			return (keyData == Keys.Escape) ? false : base.IsInputKey(keyData);
		}

		public void RaiseBeforeNavigate(String url, int flags, String
			targetFrameName, ref Object postData, String headers, ref Boolean cancel)
		{
			BrowserNavigateEventArgs e = new BrowserNavigateEventArgs(url, false);
			if (BeforeNavigate != null) BeforeNavigate(this, e);
			cancel = e.Cancel;
		}

		public void RaiseNavigateComplete(String url)
		{
			BrowserNavigateEventArgs e = new BrowserNavigateEventArgs(url, false);
			if (NavigateComplete != null) NavigateComplete(this, e);
		}
	}

	[Guid("eab22ac2-30c1-11cf-a7eb-0000c05bae0b"),
	InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IWebBrowserEvents
	{
		[DispId(100)]
		void RaiseBeforeNavigate(String url, int flags, String targetFrameName,
			ref Object postData, String headers, ref Boolean cancel);
		[DispId(101)]
		void RaiseNavigateComplete(String url);
	}

	public delegate void BrowserNavigateEventHandler(Object s,
	BrowserNavigateEventArgs e);

	public class BrowserNavigateEventArgs
	{
		String url;
		Boolean cancel;

		public BrowserNavigateEventArgs(String url, Boolean cancel)
		{
			this.url = url;
			this.cancel = cancel;
		}

		public String Url
		{
			get { return url; }
		}

		public Boolean Cancel
		{
			set { cancel = value; }
			get { return cancel; }
		}
	}


	[Guid("eab22ac1-30c1-11cf-a7eb-0000c05bae0b")]
	interface IWebBrowser
	{
		void GoBack(); void GoForward(); void GoHome(); void GoSearch();
		void Navigate(String Url, ref Object Flags, ref Object targetFrame, ref
			Object postData, ref Object headers);
		void Refresh(); void Refresh2(); void Stop();
		void GetApplication(); void GetParent(); void GetContainer();
		[return: MarshalAs(UnmanagedType.Interface)]
		IHTMLDocument2 GetDocument();
	}

	[Guid("332C4425-26CB-11D0-B483-00C04FD90119"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
	interface IHTMLDocument2
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetScript();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetAll();
		[return: MarshalAs(UnmanagedType.Interface)]
		IHTMLElement GetBody();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetActiveElement();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetImages();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetApplets();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetLinks();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetForms();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetAnchors();
		void SetTitle([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetTitle();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetScripts();
		void SetDesignMode([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetDesignMode();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetSelection();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetReadyState();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetFrames();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetEmbeds();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetPlugins();
		void SetAlinkColor([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetAlinkColor();
		void SetBgColor([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetBgColor();
		void SetFgColor([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetFgColor();
		void SetLinkColor([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetLinkColor();
		void SetVlinkColor([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetVlinkColor();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetReferrer();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetLocation();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetLastModified();
		void SetURL([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetURL();
		void SetDomain([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetDomain();
		void SetCookie([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetCookie();
		void SetExpando([In, MarshalAs(UnmanagedType.Bool)] Boolean p);
		[return: MarshalAs(UnmanagedType.Bool)] Boolean GetExpando();
		void SetCharset([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetCharset();
		void SetDefaultCharset([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetDefaultCharset();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetMimeType();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetFileSize();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetFileCreatedDate();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetFileModifiedDate();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetFileUpdatedDate();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetSecurity();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetProtocol();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetNameProp();
		void DummyWrite([In, MarshalAs(UnmanagedType.I4)] int psarray);
		void DummyWriteln([In, MarshalAs(UnmanagedType.I4)] int psarray);
		[return: MarshalAs(UnmanagedType.Interface)]
		Object Open([In, MarshalAs(UnmanagedType.BStr)] String URL, [In,
			MarshalAs(UnmanagedType.Struct)] Object name, [In,
			MarshalAs(UnmanagedType.Struct)] Object features, [In,
			MarshalAs(UnmanagedType.Struct)] Object replace);
		void Close();
		void Clear();
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean QueryCommandSupported([In, MarshalAs(UnmanagedType.BStr)] String
			cmdID);
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean QueryCommandEnabled([In, MarshalAs(UnmanagedType.BStr)] String
			cmdID);
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean QueryCommandState([In, MarshalAs(UnmanagedType.BStr)] String
			cmdID);
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean QueryCommandIndeterm([In, MarshalAs(UnmanagedType.BStr)] String
			cmdID);
		[return: MarshalAs(UnmanagedType.BStr)]
		String QueryCommandText([In, MarshalAs(UnmanagedType.BStr)] String cmdID);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object QueryCommandValue([In, MarshalAs(UnmanagedType.BStr)] String
			cmdID);
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean ExecCommand([In, MarshalAs(UnmanagedType.BStr)] String cmdID, [In,
			MarshalAs(UnmanagedType.Bool)] Boolean showUI, [In,
			MarshalAs(UnmanagedType.Struct)] Object value);
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean ExecCommandShowHelp([In, MarshalAs(UnmanagedType.BStr)] String
			cmdID);
		[return: MarshalAs(UnmanagedType.Interface)]
		Object CreateElement([In, MarshalAs(UnmanagedType.BStr)] String eTag);
		void SetOnhelp([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnhelp();
		void SetOnclick([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnclick();
		void SetOndblclick([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOndblclick();
		void SetOnkeyup([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnkeyup();
		void SetOnkeydown([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnkeydown();
		void SetOnkeypress([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnkeypress();
		void SetOnmouseup([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmouseup();
		void SetOnmousedown([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmousedown();
		void SetOnmousemove([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmousemove();
		void SetOnmouseout([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmouseout();
		void SetOnmouseover([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmouseover();
		void SetOnreadystatechange([In, MarshalAs(UnmanagedType.Struct)] Object
			p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnreadystatechange();
		void SetOnafterupdate([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnafterupdate();
		void SetOnrowexit([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnrowexit();
		void SetOnrowenter([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnrowenter();
		void SetOndragstart([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOndragstart();
		void SetOnselectstart([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnselectstart();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object ElementFromPoint([In, MarshalAs(UnmanagedType.I4)] int x, [In,
			MarshalAs(UnmanagedType.I4)] int y);
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetParentWindow(); // IHTMLWindow2
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetStyleSheets();
		void SetOnbeforeupdate([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnbeforeupdate();
		void SetOnerrorupdate([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnerrorupdate();
		[return: MarshalAs(UnmanagedType.BStr)]
		String toString();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object CreateStyleSheet([In, MarshalAs(UnmanagedType.BStr)] String
			bstrHref, [In, MarshalAs(UnmanagedType.I4)] int lIndex);
	}

	[Guid("3050F1FF-98B5-11CF-BB82-00AA00BDCE0B"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
	interface IHTMLElement
	{
		void SetAttribute([In, MarshalAs(UnmanagedType.BStr)] String
			strAttributeName, [In, MarshalAs(UnmanagedType.Struct)] Object
			AttributeValue, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
		void GetAttribute([In, MarshalAs(UnmanagedType.BStr)] String
			strAttributeName, [In, MarshalAs(UnmanagedType.I4)] int lFlags, [Out,
			MarshalAs(UnmanagedType.LPArray)] Object[] pvars);
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean RemoveAttribute([In, MarshalAs(UnmanagedType.BStr)] String
			strAttributeName, [In, MarshalAs(UnmanagedType.I4)] int lFlags);
		void SetClassName([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetClassName();
		void SetId([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetId();
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetTagName();
		[return: MarshalAs(UnmanagedType.Interface)]
		IHTMLElement GetParentElement();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetStyle(); // IHTMLStyle
		void SetOnhelp([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnhelp();
		void SetOnclick([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnclick();
		void SetOndblclick([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOndblclick();
		void SetOnkeydown([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnkeydown();
		void SetOnkeyup([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnkeyup();
		void SetOnkeypress([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnkeypress();
		void SetOnmouseout([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmouseout();
		void SetOnmouseover([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmouseover();
		void SetOnmousemove([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmousemove();
		void SetOnmousedown([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmousedown();
		void SetOnmouseup([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnmouseup();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetDocument();
		void SetTitle([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetTitle();
		void SetLanguage([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetLanguage();
		void SetOnselectstart([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnselectstart();
		void ScrollIntoView([In, MarshalAs(UnmanagedType.Struct)] Object
			varargStart);
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean Contains([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement
			pChild);
		[return: MarshalAs(UnmanagedType.I4)]
		int GetSourceIndex();
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetRecordNumber();
		void SetLang([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetLang();
		[return: MarshalAs(UnmanagedType.I4)]
		int GetOffsetLeft();
		[return: MarshalAs(UnmanagedType.I4)]
		int GetOffsetTop();
		[return: MarshalAs(UnmanagedType.I4)]
		int GetOffsetWidth();
		[return: MarshalAs(UnmanagedType.I4)]
		int GetOffsetHeight();
		[return: MarshalAs(UnmanagedType.Interface)]
		IHTMLElement GetOffsetParent();
		void SetInnerHTML([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetInnerHTML();
		void SetInnerText([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetInnerText();
		void SetOuterHTML([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetOuterHTML();
		void SetOuterText([In, MarshalAs(UnmanagedType.BStr)] String p);
		[return: MarshalAs(UnmanagedType.BStr)]
		String GetOuterText();
		void InsertAdjacentHTML([In, MarshalAs(UnmanagedType.BStr)] String where,
			[In, MarshalAs(UnmanagedType.BStr)] String html);
		void InsertAdjacentText([In, MarshalAs(UnmanagedType.BStr)] String where,
			[In, MarshalAs(UnmanagedType.BStr)] String text);
		[return: MarshalAs(UnmanagedType.Interface)]
		IHTMLElement GetParentTextEdit();
		[return: MarshalAs(UnmanagedType.Bool)]
		Boolean GetIsTextEdit();
		void Click();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetFilters();
		void SetOndragstart([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOndragstart();
		[return: MarshalAs(UnmanagedType.BStr)]
		String toString();
		void SetOnbeforeupdate([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnbeforeupdate();
		void SetOnafterupdate([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnafterupdate();
		void SetOnerrorupdate([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnerrorupdate();
		void SetOnrowexit([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnrowexit();
		void SetOnrowenter([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnrowenter();
		void SetOndatasetchanged([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOndatasetchanged();
		void SetOndataavailable([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOndataavailable();
		void SetOndatasetcomplete([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOndatasetcomplete();
		void SetOnfilterchange([In, MarshalAs(UnmanagedType.Struct)] Object p);
		[return: MarshalAs(UnmanagedType.Struct)]
		Object GetOnfilterchange();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetChildren();
		[return: MarshalAs(UnmanagedType.Interface)]
		Object GetAll();
	}
}
