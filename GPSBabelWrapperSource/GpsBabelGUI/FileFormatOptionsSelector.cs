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
	/// Summary description for FileFormatOptionsSelector.
	/// </summary>
	public class FileFormatOptionsSelector : System.Windows.Forms.UserControl
	{
		public event EventHandler selectionChanged;

		internal FileFormatOptionsSelectorForm optionsSelectorForm;

		private System.Windows.Forms.Button moreButton;
		internal System.Windows.Forms.TextBox optionsTextBox;

		private bool m_isOutputContext;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FileFormatOptionsSelector(bool isOutputContext)
		{
			m_isOutputContext = isOutputContext;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			optionsSelectorForm = new FileFormatOptionsSelectorForm(this);
			optionsSelectorForm.selectionChanged += new EventHandler(onOptionsSelectionChanged);
			optionsSelectorForm.defaultButtonClicked += new EventHandler(onDefaultButtonClicked);
		}

		private string getOptionFileName()
		{
			return getOptionFileName(optionsSelectorForm.OptionName);
		}

		private string getOptionFileName(string name)
		{
			return Project.GetNamedStorePath(Project.OPTIONS_STORE_NAME + (m_isOutputContext ? "_output" : "_input"), name + ".xml");
		}

		private string m_name;
		private string m_description;
		private XmlNode m_optionsNode;

		// used to reser options to default
		internal void resetToDefaultOptions()
		{
			try
			{
				string filename = getOptionFileName(m_name);
				File.Delete(filename);
			} 
			catch {}

			optionsSelectorForm.setOptions(m_name, m_description, m_optionsNode);
			optionsTextBox.Text = optionsSelectorForm.getOptionsText();
		}

		internal void setOptions(string name, string description, XmlNode optionsNode)
		{
			m_name = name;
			m_description = description;
			m_optionsNode = optionsNode;

			if(!name.Equals(optionsSelectorForm.OptionName))
			{
				bool gotFromFile = false;
				try
				{
					string filename = getOptionFileName(name);
					if(File.Exists(filename))
					{
						XmlDocument savedDoc = new XmlDocument();
						savedDoc.Load(filename);
						XmlNode optionsNodeSaved = savedDoc.SelectSingleNode("//options");
						optionsSelectorForm.setOptions(name, description, optionsNodeSaved);
						gotFromFile = true;
					}
				}
				catch {}

				if(!gotFromFile)
				{
					optionsSelectorForm.setOptions(name, description, optionsNode);
				}
				optionsTextBox.Text = optionsSelectorForm.getOptionsText();
			}
		}

		internal string getOptionsText()
		{
			return optionsTextBox.Text;
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
					optionsSelectorForm.Dispose();
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
			this.moreButton = new System.Windows.Forms.Button();
			this.optionsTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// moreButton
			// 
			this.moreButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.moreButton.Location = new System.Drawing.Point(415, 0);
			this.moreButton.Name = "moreButton";
			this.moreButton.Size = new System.Drawing.Size(30, 20);
			this.moreButton.TabIndex = 0;
			this.moreButton.Text = ">>";
			this.moreButton.Click += new System.EventHandler(this.moreButton_Click);
			// 
			// optionsTextBox
			// 
			this.optionsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.optionsTextBox.Name = "optionsTextBox";
			this.optionsTextBox.ReadOnly = true;
			this.optionsTextBox.Size = new System.Drawing.Size(410, 20);
			this.optionsTextBox.TabIndex = 1;
			this.optionsTextBox.TabStop = false;
			this.optionsTextBox.Text = "";
			// 
			// FileFormatOptionsSelector
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.optionsTextBox,
																		  this.moreButton});
			this.Name = "FileFormatOptionsSelector";
			this.Size = new System.Drawing.Size(445, 20);
			this.ResumeLayout(false);

		}
		#endregion

		private void onDefaultButtonClicked(object sender, System.EventArgs e)
		{
			resetToDefaultOptions();
		}

		private void onOptionsSelectionChanged(object sender, System.EventArgs e)
		{
			optionsTextBox.Text = optionsSelectorForm.getOptionsText();

			string seedXml = Project.SEED_XML + "<options></options>";
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(seedXml);

			XmlNode root = xmlDoc.DocumentElement;

			optionsSelectorForm.buildOptionsXml(xmlDoc, root);

			string filename = getOptionFileName();
			xmlDoc.Save(filename);

			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		private void moreButton_Click(object sender, System.EventArgs e)
		{
			Point location = optionsTextBox.PointToScreen(new Point(0, 20));
			optionsSelectorForm.Location = location;
			optionsSelectorForm.Show();
			//optionsSelectorForm.ShowDialog(Project.mainForm);
		}
	}
}
