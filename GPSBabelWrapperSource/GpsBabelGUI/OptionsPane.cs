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

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for OptionsPane.
	/// </summary>
	public class OptionsPane : System.Windows.Forms.UserControl
	{
		private int m_pos;
		private const int m_posStart = 35;
		private const int m_shift = 25;

		private GeneratedOptionsTextPane m_textPane;

		public event EventHandler selectionChanged;
		public event EventHandler defaultButtonClicked;

		internal bool Memorized		{
			get { return m_textPane.Memorized; }
			set {
				m_textPane.Memorized = value;
				foreach(Control control in this.Controls)
				{
					if(control is OptionControlBase)
					{
						control.Enabled = !value;
					}
				}
			}
		}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OptionsPane()
		{
			InitializeComponent();

			m_textPane = new GeneratedOptionsTextPane();
			m_textPane.Location = new Point(0, 0);
			m_textPane.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);

			m_textPane.memorizeCheckBox.CheckedChanged += new EventHandler(onMemorizeChanged);
			m_textPane.defaultButton.Click += new EventHandler(onDefaultButtonClicked);
			m_textPane.selectionChanged += new EventHandler(onMemorizeTextChanged);

			m_pos = m_posStart;
		}

		public void Reset()
		{
			m_pos = m_posStart;
			this.Controls.Clear();
			Memorized = false;
		}

		public void addOptionControl(XmlNode dataNode)
		{
			OptionControlBase control = null;

			string name = "";
			try 
			{
				name += dataNode.Attributes.GetNamedItem("name").InnerText.Trim();
			} 
			catch {}

			if("memorized".Equals(name))
			{
				m_textPane.Memorized = true;
				m_textPane.OptionsText = dataNode.InnerText.Trim();
				return;
			}

			string valuetype = "";
			try 
			{
				valuetype += dataNode.Attributes.GetNamedItem("valuetype").InnerText.Trim();
			} 
			catch {}

			string valueValue = null;
			try 
			{
				valueValue = dataNode.Attributes.GetNamedItem("value").InnerText.Trim();
			} 
			catch {}

			string descr = "";
			try 
			{
				descr += dataNode.Attributes.GetNamedItem("descr").InnerText.Trim();
			} 
			catch {}
			
			bool required = false;
			try 
			{
				required = "true".Equals("" + dataNode.Attributes.GetNamedItem("required").InnerText.Trim());
			} 
			catch {}

			bool used = false;
			try 
			{
				used = "true".Equals("" + dataNode.Attributes.GetNamedItem("used").InnerText.Trim());
			} 
			catch {}

			if(name.Length > 0)
			{
				switch (valuetype)
				{
					case "bool":
						control = configureOptionControl(new OptionControlBoolean(), name, valueValue, descr, required, used);
						break;
					case "int":
						control = configureOptionControl(new OptionControlInteger(), name, valueValue, descr, required, used);
						break;
					case "string":
						control = configureOptionControl(new OptionControlString(), name, valueValue, descr, required, used);
						break;
					case "path":
						control = configureOptionControl(new OptionControlPath(), name, valueValue, descr, required, used);
						break;
					default:
						control = configureOptionControl(new OptionControlGeneral(), name, valueValue, descr, required, used);
						break;
				}
			}

			if(control != null)
			{
				Point location = new Point(5, m_pos);
				m_pos += m_shift;
				control.Location = location;
				control.Size = new Size(this.Width, 20);
				control.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);

				control.selectionChanged += new EventHandler(onControlSelectionChanged);

				this.Controls.Add(control);
			}
		}
		
		private void onControlSelectionChanged(object sender, System.EventArgs e)
		{
			m_textPane.OptionsText = getGeneratedText();
			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		private void onMemorizeTextChanged(object sender, System.EventArgs e)
		{
			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}
		
		private void onMemorizeChanged(object sender, System.EventArgs e)
		{
			Memorized = m_textPane.memorizeCheckBox.Checked;

			if(!Memorized)
			{
				m_textPane.OptionsText = getGeneratedText();
			}

			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		private void onDefaultButtonClicked(object sender, System.EventArgs e)
		{
			if(defaultButtonClicked != null)
			{
				defaultButtonClicked(this, null);
			}
		}
		
		public void finishRendering()
		{
			Memorized = Memorized;		// ensure that controls are enabled or disabled as needed.
			this.Controls.Add(m_textPane);
			if(!Memorized)
			{
				m_textPane.OptionsText = getGeneratedText();
			}
			this.Height = m_pos;
		}

		private OptionControlBase configureOptionControl(OptionControlBase control, string name, string valueValue, string descr, bool required, bool used)
		{
			control.OptionName = name;
			control.Descr = descr;
			control.valueValue = valueValue;
			control.Used = used;				// for bool will override valueValue
			control.Required = required;
			return control;
		}

		internal string getOptionsText()
		{
			return m_textPane.OptionsText;
		}

		private string getGeneratedText()
		{
			StringBuilder ret = new StringBuilder();
			foreach(Control control in this.Controls)
			{
				if(control is OptionControlBase)
				{
					OptionControlBase optionControl = control as OptionControlBase;
					if(optionControl.Used)
					{
						ret.Append(optionControl.getGeneratedText());
						ret.Append(",");
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

		internal void buildOptionsXml(XmlDocument xmlDoc, XmlNode node)
		{
			foreach(Control control in this.Controls)
			{
				if(control is OptionControlBase)
				{
					OptionControlBase optionControl = control as OptionControlBase;
					optionControl.addToOptionsXml(xmlDoc, node);
				}
			}
			m_textPane.addToOptionsXml(xmlDoc, node);
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
			// OptionsPane
			// 
			this.AutoScroll = true;
			this.Name = "OptionsPane";
			this.Size = new System.Drawing.Size(155, 145);

		}
		#endregion
	}
}
