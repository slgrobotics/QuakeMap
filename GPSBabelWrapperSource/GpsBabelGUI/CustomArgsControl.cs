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
	/// Summary description for CustomArgsControl.
	/// </summary>
	public class CustomArgsControl : System.Windows.Forms.UserControl
	{
		public event EventHandler selectionChanged;
		public string CustomArgs { get { return customArgsTextBox.Text; } }

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox customArgsTextBox;
		internal System.Windows.Forms.Label hintLabel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CustomArgsControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitForm call

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
			this.customArgsTextBox = new System.Windows.Forms.TextBox();
			this.hintLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(70, 30);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "Custom Options:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// customArgsTextBox
			// 
			this.customArgsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.customArgsTextBox.Location = new System.Drawing.Point(175, 30);
			this.customArgsTextBox.Name = "customArgsTextBox";
			this.customArgsTextBox.Size = new System.Drawing.Size(415, 20);
			this.customArgsTextBox.TabIndex = 1;
			this.customArgsTextBox.Text = "";
			this.customArgsTextBox.TextChanged += new System.EventHandler(this.customArgsTextBox_TextChanged);
			// 
			// hintLabel
			// 
			this.hintLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.hintLabel.Location = new System.Drawing.Point(170, 5);
			this.hintLabel.Name = "hintLabel";
			this.hintLabel.Size = new System.Drawing.Size(420, 20);
			this.hintLabel.TabIndex = 2;
			this.hintLabel.Text = "Enter custom options to be used in GpsBabel command line";
			// 
			// CustomArgsControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.hintLabel,
																		  this.customArgsTextBox,
																		  this.label1});
			this.Name = "CustomArgsControl";
			this.Size = new System.Drawing.Size(595, 60);
			this.ResumeLayout(false);

		}
		#endregion

		private void onSelectionChanged()
		{
			if(selectionChanged != null)
			{
				selectionChanged(this, null);
			}
		}

		private void customArgsTextBox_TextChanged(object sender, System.EventArgs e)
		{
			onSelectionChanged();
		}
	}
}
