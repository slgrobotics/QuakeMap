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

namespace GpsBabelGUI
{
	/// <summary>
	/// Summary description for GeneratedFiltersTextPane.
	/// </summary>
	public class GeneratedFiltersTextPane : System.Windows.Forms.UserControl
	{
		public event EventHandler selectionChanged;

		internal string OptionsText	{ get { return optionsTextBox.Text; } set { optionsTextBox.Text = value; } }

		internal System.Windows.Forms.TextBox optionsTextBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GeneratedFiltersTextPane()
		{
			InitializeComponent();

			optionsTextBox.TextChanged += new EventHandler(onSelectionChanged);
		}

		protected void onSelectionChanged(object sender, System.EventArgs e)
		{
			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
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
			this.optionsTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// optionsTextBox
			// 
			this.optionsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.optionsTextBox.Location = new System.Drawing.Point(2, 5);
			this.optionsTextBox.Name = "optionsTextBox";
			this.optionsTextBox.Size = new System.Drawing.Size(743, 20);
			this.optionsTextBox.TabIndex = 4;
			this.optionsTextBox.Text = "";
			// 
			// GeneratedFiltersTextPane
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.optionsTextBox});
			this.Name = "GeneratedFiltersTextPane";
			this.Size = new System.Drawing.Size(750, 30);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
