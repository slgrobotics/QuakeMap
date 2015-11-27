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
using System.Xml.XPath;


using System.IO;

using LibSys;

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for FileFormatSelectorControl.
	/// </summary>
	public class FileFormatSelectorControl : System.Windows.Forms.UserControl
	{
		public event EventHandler selectionChanged;

		public string FileName { get { return fileTextBox.Text; } set { fileTextBox.Text = value; } }
		public string FileFormat { get { return "" + formatComboBox.SelectedItem; } }

		internal FileFormatOptionsSelector OptionsSelector;

		private bool m_isOutputContext;	// true: opens Save dialog; false: Open dialog

		private XmlDocument m_formatDoc = null;

		public XmlDocument formatDoc 
		{
			set
			{
				m_formatDoc = value;
				setFormats();
				formatComboBox.SelectedIndexChanged  += new EventHandler(onFormatComboBoxSelectedIndexChanged);
			}
		}

		public int FormatIndex 
		{
			get { return formatComboBox.SelectedIndex; }
			set { formatComboBox.SelectedIndex = value; }
		}
 
		public string FormatName 
		{
			get { return "" + Formats[formatComboBox.Text]; }
		}
 
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox formatComboBox;
		public System.Windows.Forms.TextBox fileTextBox;
		public System.Windows.Forms.Button browseFileButton;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Panel optionsPanel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FileFormatSelectorControl(bool isOutputContext)
		{
			m_isOutputContext = isOutputContext;

			OptionsSelector = new FileFormatOptionsSelector(m_isOutputContext);

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			OptionsSelector.Dock = DockStyle.Fill;
			optionsPanel.Controls.Add(OptionsSelector);
			OptionsSelector.selectionChanged += new EventHandler(onFormatOptionsChanged);
		}

		internal void setOptions(string name, string description, XmlNode options)
		{
			OptionsSelector.setOptions(name, description, options);
		}

		internal string getOptionsText()
		{
			return OptionsSelector.getOptionsText();
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
			this.label1 = new System.Windows.Forms.Label();
			this.formatComboBox = new System.Windows.Forms.ComboBox();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.browseFileButton = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(5, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 20);
			this.label1.TabIndex = 18;
			this.label1.Text = "Format:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// formatComboBox
			// 
			this.formatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.formatComboBox.Location = new System.Drawing.Point(60, 30);
			this.formatComboBox.Name = "formatComboBox";
			this.formatComboBox.Size = new System.Drawing.Size(360, 21);
			this.formatComboBox.TabIndex = 17;
			// 
			// fileTextBox
			// 
			this.fileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.fileTextBox.Location = new System.Drawing.Point(60, 5);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.Size = new System.Drawing.Size(560, 20);
			this.fileTextBox.TabIndex = 14;
			this.fileTextBox.Text = "";
			this.fileTextBox.TextChanged += new System.EventHandler(this.fileTextBox_TextChanged);
			// 
			// browseFileButton
			// 
			this.browseFileButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.browseFileButton.Location = new System.Drawing.Point(625, 5);
			this.browseFileButton.Name = "browseFileButton";
			this.browseFileButton.Size = new System.Drawing.Size(30, 23);
			this.browseFileButton.TabIndex = 16;
			this.browseFileButton.Text = "...";
			this.browseFileButton.Click += new System.EventHandler(this.browseFileButton_Click);
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(5, 5);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(50, 20);
			this.label5.TabIndex = 15;
			this.label5.Text = "File:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// optionsPanel
			// 
			this.optionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.optionsPanel.Location = new System.Drawing.Point(425, 30);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(230, 20);
			this.optionsPanel.TabIndex = 19;
			// 
			// FileFormatSelectorControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.optionsPanel,
																		  this.label1,
																		  this.formatComboBox,
																		  this.fileTextBox,
																		  this.browseFileButton,
																		  this.label5});
			this.Name = "FileFormatSelectorControl";
			this.Size = new System.Drawing.Size(655, 55);
			this.ResumeLayout(false);

		}
		#endregion

		private void browseFileButton_Click(object sender, System.EventArgs e)
		{
			bringUpFileSelectionDialog();
		}

		public void bringUpFileSelectionDialog()
		{
			if(m_isOutputContext)
			{
				System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

				saveFileDialog.InitialDirectory = fileTextBox.Text;
				saveFileDialog.DefaultExt = "";
				saveFileDialog.AddExtension = false;
				// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
				//saveFileDialog.Filter = "GPX, GPZ, ZIP and LOC files (*.gpx;*.gpz;*.zip;*.loc)|*.gpx;*.gpz;*.zip;*.loc|ZIP'ed GPX/Photo Collections (*.zip;*.gpz)|*.zip;*.gpz|JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*";
				saveFileDialog.Filter = "GPX and LOC files (*.gpx;*.loc)|*.gpx;*.loc|All files (*.*)|*.*";

				DialogResult result = saveFileDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					FileInfo fi = new FileInfo(saveFileDialog.FileName);
					setFileText(fi.FullName);
				}
			}
			else
			{
				System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

				openFileDialog.InitialDirectory = fileTextBox.Text;
				openFileDialog.DefaultExt = "";
				openFileDialog.AddExtension = false;
				// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
				//openFileDialog.Filter = "GPX, GPZ, ZIP and LOC files (*.gpx;*.gpz;*.zip;*.loc)|*.gpx;*.gpz;*.zip;*.loc|ZIP'ed GPX/Photo Collections (*.zip;*.gpz)|*.zip;*.gpz|JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*";
				openFileDialog.Filter = "GPX and LOC files (*.gpx;*.loc)|*.gpx;*.loc|All files (*.*)|*.*";

				DialogResult result = openFileDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					FileInfo fi = new FileInfo(openFileDialog.FileName);
					setFileText(Project.GetLongPathName(fi.FullName));
				}
			}
		}

		private void setFileText(string txt)
		{
			fileTextBox.Text = txt;
			fileTextBox.Select(txt.Length,0);
			fileTextBox.ScrollToCaret();
		}

		public Hashtable Formats = new Hashtable();

		private void setFormats()
		{
			//Font font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			Font font = formatComboBox.Font;
			Graphics graphics = this.CreateGraphics();
			float stringWidthLongest = graphics.MeasureString("mapconverter****", font).Width;

			ArrayList list = new ArrayList();
			foreach(XmlNode fmt in m_formatDoc.SelectNodes("//args/filetypes/option"))
			{
				XmlNode name = fmt.Attributes.GetNamedItem("name");
				XmlNode descr = fmt.Attributes.GetNamedItem("descr");
				string sItem = name.InnerText;
				SizeF stringSize;
				// make descriptions evenly spaced:
				do 
				{
					sItem = sItem.Replace("*", " ") + "*";
					stringSize = graphics.MeasureString(sItem, font);
				} while (stringSize.Width < stringWidthLongest);

				sItem = sItem.Replace("*", " ") + descr.InnerText.Trim();
				list.Add(sItem);
				Formats.Add(sItem, name.InnerText);
			}
			formatComboBox.DataSource = list;
		}

		public void setFormat()
		{
			string name = (string)Formats[formatComboBox.Text];
			if(name != null)
			{
				XmlNode fmtOptions = m_formatDoc.SelectSingleNode("//args/filetypes/option[@name=\"" + name + "\"]");
				setOptions(name, formatComboBox.Text, fmtOptions);
			}
		}

		private void onSelectionChanged()
		{
			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		private void fileTextBox_TextChanged(object sender, System.EventArgs e)
		{
			onSelectionChanged();
		}

		private void onFormatComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
		{
			onSelectionChanged();
		}

		private void onFormatOptionsChanged(object sender, System.EventArgs e)
		{
			onSelectionChanged();
		}
	}
}
