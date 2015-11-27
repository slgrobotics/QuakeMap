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
using System.Text;
using System.Xml;

using GpsBabelCmd;

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for FiltersPane.
	/// </summary>
	public class FiltersPane : System.Windows.Forms.UserControl
	{
		private int m_pos;
		private const int m_posStart = 25;

		public event EventHandler selectionChanged;

		private GeneratedFiltersTextPane m_textPane;

		private XmlDocument m_filterDoc = null;

		public XmlDocument filterDoc 
		{
			set
			{
				m_filterDoc = value;
				setFilters();
			}
		}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FiltersPane()
		{
			InitializeComponent();

			m_textPane = new GeneratedFiltersTextPane();
			m_textPane.Location = new Point(0, 0);
			//m_textPane.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);

			m_textPane.selectionChanged += new EventHandler(onFiltersTextChanged);

			m_pos = m_posStart;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// FiltersPane
			// 
			this.Name = "FiltersPane";
			this.Size = new System.Drawing.Size(155, 145);

		}
		#endregion

		public void addFilterControl(XmlNode dataNode)
		{
			string name = "";
			try 
			{
				name += dataNode.Attributes.GetNamedItem("name").InnerText.Trim();
			} 
			catch {}

			string descr = "";
			try 
			{
				descr += dataNode.Attributes.GetNamedItem("descr").InnerText.Trim();
			} 
			catch {}
			
			bool used = false;
			try 
			{
				used = "true".Equals("" + dataNode.Attributes.GetNamedItem("used").InnerText.Trim());
			} 
			catch {}

			FilterControl control = new FilterControl();
			control.FilterName = name;
			control.Descr = descr;
			control.filterNode = dataNode;
			control.Used = used;

			this.Width = Math.Max(this.Width, control.Width);

			Point location = new Point(0, m_pos);
			control.Location = location;
			control.Width = this.Width;
			m_pos += control.Height + 5;

			control.selectionChanged += new EventHandler(onControlSelectionChanged);

			this.Controls.Add(control);
		}
		
		public void fillCommandObject(GpsBabelCommand gbc)
		{
			gbc.FilterOptions = m_textPane.OptionsText;
		}

		public void Reset()
		{
			m_pos = m_posStart;
			this.Controls.Clear();
		}

		public void finishRendering()
		{
			m_textPane.Width = this.Width;
			this.Controls.Add(m_textPane);
			m_textPane.OptionsText = getGeneratedText();
			this.Height = m_pos;
		}

		private void setFilters()
		{
			Reset();
			foreach(XmlNode flt in m_filterDoc.SelectNodes("//args/datafilters/option"))
			{
				addFilterControl(flt);
			}
			finishRendering();
		}

		private string getGeneratedText()
		{
			StringBuilder ret = new StringBuilder();
			foreach(Control control in this.Controls)
			{
				if(control is FilterControl)
				{
					FilterControl filterControl = control as FilterControl;
					if(filterControl.Used)
					{
						ret.Append(filterControl.getGeneratedText());
						ret.Append(" ");
					}
				}
			}
			string sRet = ret.ToString();
			if(sRet.EndsWith(","))
			{
				sRet = sRet.Substring(0, sRet.Length-1);
			}
			return sRet;
		}

		private void onControlSelectionChanged(object sender, System.EventArgs e)
		{
			m_textPane.OptionsText = getGeneratedText();
			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		private void onFiltersTextChanged(object sender, System.EventArgs e)
		{
			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}
		
	}
}
