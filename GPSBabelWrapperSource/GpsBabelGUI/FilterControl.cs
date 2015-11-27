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
using System.Xml;
using System.IO;

using LibSys;

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for FilterControl.
	/// </summary>
	public class FilterControl : System.Windows.Forms.UserControl
	{
		public event EventHandler selectionChanged;

		private OptionsPane m_optionsPane;

		internal bool Used { get { return useCheckBox.Checked; } set { useCheckBox.Checked = value; setLook(); } }

		internal string Descr { get { return descrLabel.Text; } set { descrLabel.Text = value; } }

		private string m_name = "";
		public string FilterName 
		{ 
			get { return m_name; }
			set { m_name = value; setNameLabel(); }
		}

		private XmlNode m_filterNode; 

		public XmlNode filterNode 
		{
			set
			{
				m_filterNode = value;
				setOptions();
			}
		}

		internal System.Windows.Forms.Panel mainPanel;
		private System.Windows.Forms.CheckBox useCheckBox;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.Label descrLabel;
		private System.Windows.Forms.GroupBox groupBox1;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FilterControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			useCheckBox.CheckedChanged += new EventHandler(onSelectionChanged);

			setLook();
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
			this.mainPanel = new System.Windows.Forms.Panel();
			this.useCheckBox = new System.Windows.Forms.CheckBox();
			this.nameLabel = new System.Windows.Forms.Label();
			this.descrLabel = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainPanel
			// 
			this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.mainPanel.Location = new System.Drawing.Point(25, 35);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = new System.Drawing.Size(821, 20);
			this.mainPanel.TabIndex = 3;
			// 
			// useCheckBox
			// 
			this.useCheckBox.Location = new System.Drawing.Point(10, 10);
			this.useCheckBox.Name = "useCheckBox";
			this.useCheckBox.Size = new System.Drawing.Size(20, 20);
			this.useCheckBox.TabIndex = 2;
			// 
			// nameLabel
			// 
			this.nameLabel.Location = new System.Drawing.Point(30, 13);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(85, 20);
			this.nameLabel.TabIndex = 5;
			// 
			// descrLabel
			// 
			this.descrLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.descrLabel.Location = new System.Drawing.Point(120, 13);
			this.descrLabel.Name = "descrLabel";
			this.descrLabel.Size = new System.Drawing.Size(726, 20);
			this.descrLabel.TabIndex = 4;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.mainPanel,
																					this.nameLabel,
																					this.descrLabel,
																					this.useCheckBox});
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(2, 2);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(851, 61);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			// 
			// FilterControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox1});
			this.DockPadding.All = 2;
			this.Name = "FilterControl";
			this.Size = new System.Drawing.Size(855, 65);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void setLook()
		{
			nameLabel.Enabled = Used;
			descrLabel.Enabled = Used;
			mainPanel.Enabled = Used;
		}

		protected virtual void setNameLabel()
		{
			nameLabel.Text = FilterName;
		}

		protected void setNameLabel(string text)
		{
			nameLabel.Text = text;
		}

		public virtual string getGeneratedText()
		{
			if(Used)
			{
				string ret = "-x " + FilterName;
				string options = m_optionsPane.getOptionsText();
				if(options.Length > 0)
				{
					ret += "," + options;
				}
				return ret;
			}
			else
			{
				return "";
			}
		}

		private void onDefaultButtonClicked(object sender, System.EventArgs e)
		{
			resetToDefaultOptions();
		}

		// used to reser options to default
		internal void resetToDefaultOptions()
		{
			try
			{
				string filename = getFilterFileName(m_name);
				File.Delete(filename);
			} 
			catch {}

			setOptions();
			//optionsTextBox.Text = optionsSelectorForm.getOptionsText();
		}

		internal void setOptions()
		{
			bool gotFromFile = false;
			try
			{
				string filename = getFilterFileName(m_name);
				if(File.Exists(filename))
				{
					XmlDocument savedDoc = new XmlDocument();
//					LibSys.StatusBar.Trace("FILE: -- " + filename);
					savedDoc.Load(filename);
					XmlNode filterNode = savedDoc.SelectSingleNode("//options");
					// we need to copy parameters found in the memorized file on m_filterNode
					// so that when the options layout changes the new schema is used with old values.
//					LibSys.StatusBar.Trace("FILE: -- " + filterNode.OuterXml);
//					foreach(XmlNode childNode in filterNode.ChildNodes)
//					{
////						string name = childNode.Attributes["name"].InnerText;
////						string used = childNode.Attributes["used"].InnerText;
////						string val = childNode.Attributes["value"].InnerText;
//						LibSys.StatusBar.Trace("    " + childNode.OuterXml);
//					}
					XmlNode memorizedNode = savedDoc.SelectSingleNode("//options/param[@name=\"memorized\"]");

//					LibSys.StatusBar.Trace("SCHEMA: -- " + m_filterNode.OuterXml);

					// go through the (possibly new) schema and transfer values from the file to it:
					foreach(XmlNode childNode in m_filterNode.ChildNodes)
					{
						string name = childNode.Attributes["name"].InnerText;

						try
						{
							XmlNode origValueNode = filterNode.SelectSingleNode("./param[@name=\"" + name + "\"]");

							if(origValueNode.Attributes["used"] != null)
							{
								string used = origValueNode.Attributes["used"].InnerText;

								XmlAttribute attr = childNode.OwnerDocument.CreateAttribute("used");
								attr.InnerText = used;
								childNode.Attributes.Append(attr);

//								LibSys.StatusBar.Trace("   USED: " + used);
							}

							if(origValueNode.Attributes["value"] != null)
							{
								string val = origValueNode.Attributes["value"].InnerText;

								XmlAttribute attr = childNode.OwnerDocument.CreateAttribute("value");
								attr.InnerText = val;
								childNode.Attributes.Append(attr);

//								LibSys.StatusBar.Trace("   VALUE: " + val);
							}
						}
						catch (Exception exc)
						{
							LibSys.StatusBar.Error("name=" + name + "    " + exc.ToString());
						}

//						LibSys.StatusBar.Trace("    " + childNode.OuterXml);
					}
					if(memorizedNode != null)
					{
						XmlNode node = m_filterNode.OwnerDocument.CreateElement("param");

						XmlAttribute attr = m_filterNode.OwnerDocument.CreateAttribute("name");
						attr.InnerText = "memorized";
						node.Attributes.Append(attr);

						node.InnerText = memorizedNode.InnerText;

						m_filterNode.AppendChild(node);
					}
					setOptionsPane(m_filterNode);
					gotFromFile = true;
				}
			}
			catch (Exception exc)
			{
				LibSys.StatusBar.Error(exc.ToString());
			}

			if(!gotFromFile)
			{
				setOptionsPane(m_filterNode);
			}
		}

		internal void setOptionsPane(XmlNode filterNode)
		{
			mainPanel.Controls.Clear();

			m_optionsPane = new OptionsPane();
			m_optionsPane.AutoScroll = false;
			m_optionsPane.Width = this.Width;
			m_optionsPane.Name = "optionsPane";

			m_optionsPane.Reset();
			foreach(XmlNode childNode in filterNode.ChildNodes)
			{
				m_optionsPane.addOptionControl(childNode);
			}
			m_optionsPane.finishRendering();

//			mainPanel.Width = this.Width - 20;
			mainPanel.Height = m_optionsPane.Height;
//			m_optionsPane.Dock = DockStyle.Fill;

			m_optionsPane.selectionChanged += new EventHandler(onOptionsSelectionChanged);
			m_optionsPane.defaultButtonClicked += new EventHandler(onDefaultButtonClicked);
			mainPanel.Controls.Add(m_optionsPane);
			this.Height = mainPanel.Height + mainPanel.Location.Y + 10;
		}

		private string getFilterFileName()
		{
			return getFilterFileName(m_name);
		}

		private string getFilterFileName(string name)
		{
			return Project.GetNamedStorePath(Project.OPTIONS_STORE_NAME + "_filter", name + ".xml");
		}

		private void onOptionsSelectionChanged(object sender, System.EventArgs e)
		{
			string seedXml = Project.SEED_XML + "<options></options>";
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(seedXml);

			XmlNode root = xmlDoc.DocumentElement;

			m_optionsPane.buildOptionsXml(xmlDoc, root);

			string filename = getFilterFileName();
			xmlDoc.Save(filename);

			// setLook();

			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		protected void onSelectionChanged(object sender, System.EventArgs e)
		{
			setLook();

			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}
	}
}
