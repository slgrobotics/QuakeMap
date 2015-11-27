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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

using LibSys;

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for FileFormatOptionsSelectorForm.
	/// </summary>
	public class FileFormatOptionsSelectorForm : System.Windows.Forms.Form
	{
		public event EventHandler selectionChanged;
		public event EventHandler defaultButtonClicked;

		private FileFormatOptionsSelector m_selector;
		private HtmlBrowser htmlBrowser;
		
		private System.Windows.Forms.GroupBox xmlGroupBox;
		private OptionsPane optionsPane;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPageControls;
		private System.Windows.Forms.TabPage tabPageXML;
		private System.Windows.Forms.Panel optionsPanel;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FileFormatOptionsSelectorForm(FileFormatOptionsSelector selector)
		{
			m_selector = selector;

			InitializeComponent();

			this.htmlBrowser = new HtmlBrowser();
			this.htmlBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.htmlBrowser.Location = new System.Drawing.Point(3, 16);
			this.htmlBrowser.Name = "htmlBrowser";
			this.htmlBrowser.Size = new System.Drawing.Size(289, 386);
			this.htmlBrowser.TabIndex = 0;

			this.xmlGroupBox.Controls.Add(this.htmlBrowser);

			optionsPane = new OptionsPane();
			optionsPane.Dock = System.Windows.Forms.DockStyle.Fill;
			optionsPane.Name = "optionsPane";
			optionsPane.selectionChanged += new EventHandler(onOptionsSelectionChanged);
			optionsPane.defaultButtonClicked += new EventHandler(onDefaultButtonClicked);

			optionsPanel.Controls.Add(optionsPane);
		}

		private string m_name = "";
		public string OptionName { get { return m_name; } }

		private string m_xmlText = "";

		internal void setOptions(string name, string description, XmlNode node)
		{
			m_name = name;

			m_xmlText = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"yes\"?><" + name + ">\n" + node.InnerXml + "</" + name + ">\n";

			//htmlBrowser.SetHtmlText(m_xmlText);
			htmlBrowser.DisplayHtml(m_xmlText);

			optionsPane.Reset();
			foreach(XmlNode childNode in node.ChildNodes)
			{
				optionsPane.addOptionControl(childNode);
			}
			optionsPane.finishRendering();
			this.Text = description;
		}

		internal string getOptionsText()
		{
			return optionsPane.getOptionsText();
		}

		internal void buildOptionsXml(XmlDocument xmlDoc, XmlNode node)
		{
			optionsPane.buildOptionsXml(xmlDoc, node);
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
					htmlBrowser.Dispose();
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.xmlGroupBox = new System.Windows.Forms.GroupBox();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageControls = new System.Windows.Forms.TabPage();
			this.tabPageXML = new System.Windows.Forms.TabPage();
			this.tabControl1.SuspendLayout();
			this.tabPageControls.SuspendLayout();
			this.tabPageXML.SuspendLayout();
			this.SuspendLayout();
			// 
			// xmlGroupBox
			// 
			this.xmlGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xmlGroupBox.Name = "xmlGroupBox";
			this.xmlGroupBox.Size = new System.Drawing.Size(849, 278);
			this.xmlGroupBox.TabIndex = 1;
			this.xmlGroupBox.TabStop = false;
			// 
			// optionsPanel
			// 
			this.optionsPanel.BackColor = System.Drawing.SystemColors.Control;
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(749, 278);
			this.optionsPanel.TabIndex = 2;
			// 
			// tabControl1
			// 
			this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.tabPageControls,
																					  this.tabPageXML});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(757, 304);
			this.tabControl1.TabIndex = 3;
			// 
			// tabPageControls
			// 
			this.tabPageControls.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this.optionsPanel});
			this.tabPageControls.Location = new System.Drawing.Point(4, 4);
			this.tabPageControls.Name = "tabPageControls";
			this.tabPageControls.Size = new System.Drawing.Size(749, 278);
			this.tabPageControls.TabIndex = 0;
			this.tabPageControls.Text = "Options";
			// 
			// tabPageXML
			// 
			this.tabPageXML.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.xmlGroupBox});
			this.tabPageXML.Location = new System.Drawing.Point(4, 4);
			this.tabPageXML.Name = "tabPageXML";
			this.tabPageXML.Size = new System.Drawing.Size(849, 278);
			this.tabPageXML.TabIndex = 1;
			this.tabPageXML.Text = "XML";
			// 
			// FileFormatOptionsSelectorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(757, 304);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FileFormatOptionsSelectorForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "FileFormatOptionsSelectorForm";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FileFormatOptionsSelectorForm_Closing);
			this.tabControl1.ResumeLayout(false);
			this.tabPageControls.ResumeLayout(false);
			this.tabPageXML.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		protected override bool ProcessDialogKey(Keys keyData)
		{
			bool keystrokeProcessed = true;
			switch (keyData) 
			{
				case Keys.Escape:
				case Keys.Alt | Keys.F4:
					break;
				default:
					keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					break;
			}
			if(keystrokeProcessed) 
			{
				this.Close();
				return true;
			} 
			else 
			{
				return base.ProcessDialogKey(keyData);
			}
		}

		private void onOptionsSelectionChanged(object sender, System.EventArgs e)
		{
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

		private void FileFormatOptionsSelectorForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			this.Hide();
		}

	}
}
