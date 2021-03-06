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

using System.Windows.Forms;


namespace GpsBabelGUI
{
	public class OptionControlString : GpsBabelGUI.OptionControlBase
	{
		private TextBox m_textBox = new TextBox();

		private System.ComponentModel.IContainer components = null;

		internal override string valueType { get { return "string"; } }

		internal override string valueValue { get { return m_textBox.Text; } set { m_textBox.Text = value; } }

		public OptionControlString()
		{
			InitializeComponent();

			m_textBox.Dock = DockStyle.Fill;
			m_textBox.TextChanged += new EventHandler(onSelectionChanged);

			mainPanel.Controls.Add(m_textBox);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		protected override void setNameLabel()
		{
			base.setNameLabel(OptionName + "=<string>");
		}

		public override string getGeneratedText()
		{
			return Used ? (OptionName + "=\"" + m_textBox.Text.Trim() + "\"") : "";
		}
	}
}

