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

using LibSys;

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for OptionControlBase.
	/// </summary>
	public class OptionControlBase : System.Windows.Forms.UserControl
	{
		public event EventHandler selectionChanged;

		private bool m_required = false;
		public bool Required 
		{
			get
			{
				return m_required;
			}
			set
			{
				m_required = value;
				useCheckBox.Checked = value;
				useCheckBox.Enabled = !value;
				requiredLabel.Visible = value;
			}
		}

		internal virtual string valueType { get { return null; } }

		internal virtual string valueValue { get { return null; } set { ; } }

		internal bool Used { get { return useCheckBox.Checked; } set { useCheckBox.Checked = value; setLook(); } }

		internal string Descr { get { return descrLabel.Text; } set { descrLabel.Text = value; } }

		private string m_name = "";
		public string OptionName { 
			get { return m_name; }
			set { m_name = value; setNameLabel(); }
		}

		private System.Windows.Forms.CheckBox useCheckBox;
		internal System.Windows.Forms.Panel mainPanel;
		private System.Windows.Forms.Label descrLabel;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.Label requiredLabel;
		
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OptionControlBase()
		{
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
			this.useCheckBox = new System.Windows.Forms.CheckBox();
			this.mainPanel = new System.Windows.Forms.Panel();
			this.descrLabel = new System.Windows.Forms.Label();
			this.nameLabel = new System.Windows.Forms.Label();
			this.requiredLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// useCheckBox
			// 
			this.useCheckBox.Location = new System.Drawing.Point(15, 0);
			this.useCheckBox.Name = "useCheckBox";
			this.useCheckBox.Size = new System.Drawing.Size(20, 20);
			this.useCheckBox.TabIndex = 0;
			// 
			// mainPanel
			// 
			this.mainPanel.Location = new System.Drawing.Point(35, 0);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = new System.Drawing.Size(225, 20);
			this.mainPanel.TabIndex = 1;
			// 
			// descrLabel
			// 
			this.descrLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.descrLabel.Location = new System.Drawing.Point(440, 0);
			this.descrLabel.Name = "descrLabel";
			this.descrLabel.Size = new System.Drawing.Size(410, 20);
			this.descrLabel.TabIndex = 2;
			// 
			// nameLabel
			// 
			this.nameLabel.Location = new System.Drawing.Point(270, 0);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(160, 20);
			this.nameLabel.TabIndex = 3;
			// 
			// requiredLabel
			// 
			this.requiredLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.requiredLabel.Name = "requiredLabel";
			this.requiredLabel.Size = new System.Drawing.Size(15, 20);
			this.requiredLabel.TabIndex = 4;
			this.requiredLabel.Text = "*";
			this.requiredLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// OptionControlBase
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.requiredLabel,
																		  this.nameLabel,
																		  this.descrLabel,
																		  this.mainPanel,
																		  this.useCheckBox});
			this.Name = "OptionControlBase";
			this.Size = new System.Drawing.Size(845, 20);
			this.ResumeLayout(false);

		}
		#endregion

		protected virtual void setNameLabel()
		{
			nameLabel.Text = OptionName;
		}

		protected void setNameLabel(string text)
		{
			nameLabel.Text = text;
		}

		public virtual string getGeneratedText()
		{
			return Used ? OptionName : "";
		}

		private void setLook()
		{
			requiredLabel.Enabled = Used;
			nameLabel.Enabled = Used;
			descrLabel.Enabled = Used;
			mainPanel.Enabled = Used;
		}

		internal void addToOptionsXml(XmlDocument xmlDoc, XmlNode root)
		{
			XmlNode node = xmlDoc.CreateElement("param");

			XmlAttribute attr = xmlDoc.CreateAttribute("name");
			attr.InnerText = OptionName;
			node.Attributes.Append(attr);

			if(Required)
			{
				attr = xmlDoc.CreateAttribute("required");
				attr.InnerText = "true";
				node.Attributes.Append(attr);
			}

			if(Used)
			{
				attr = xmlDoc.CreateAttribute("used");
				attr.InnerText = "true";
				node.Attributes.Append(attr);
			}

			if(valueType != null)
			{
				attr = xmlDoc.CreateAttribute("valuetype");
				attr.InnerText = valueType;
				node.Attributes.Append(attr);
			}

			if(valueValue != null)
			{
				attr = xmlDoc.CreateAttribute("value");
				attr.InnerText = valueValue;
				node.Attributes.Append(attr);
			}

			attr = xmlDoc.CreateAttribute("descr");
			attr.InnerText = Descr.Trim();
			node.Attributes.Append(attr);

			root.AppendChild(node);
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
