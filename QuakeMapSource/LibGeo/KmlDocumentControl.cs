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

using LibSys;

namespace LibGeo
{
	/// <summary>
	/// Summary description for KmlDocumentControl.
	/// </summary>
	public class KmlDocumentControl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.CheckBox clampWptsCheckBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox clampTrksCheckBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public KmlDocumentControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			clampWptsCheckBox.Checked = Project.kmlOptions.WptsClampToGround;
			clampTrksCheckBox.Checked = Project.kmlOptions.TrksClampToGround;

			// doesn't work through obfuscation:
//			clampWptsCheckBox.DataBindings.Add("Checked", Project.kmlOptions, "WptsClampToGround");
//			clampTrksCheckBox.DataBindings.Add("Checked", Project.kmlOptions, "TrksClampToGround");
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
			this.clampWptsCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.clampTrksCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// clampWptsCheckBox
			// 
			this.clampWptsCheckBox.Location = new System.Drawing.Point(10, 20);
			this.clampWptsCheckBox.Name = "clampWptsCheckBox";
			this.clampWptsCheckBox.Size = new System.Drawing.Size(170, 15);
			this.clampWptsCheckBox.TabIndex = 11;
			this.clampWptsCheckBox.Text = "waypoints";
			this.clampWptsCheckBox.CheckedChanged += new System.EventHandler(this.clampWptsCheckBox_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.clampTrksCheckBox,
																					this.clampWptsCheckBox});
			this.groupBox1.Location = new System.Drawing.Point(10, 10);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(185, 65);
			this.groupBox1.TabIndex = 12;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Clamp To Ground";
			// 
			// clampTrksCheckBox
			// 
			this.clampTrksCheckBox.Location = new System.Drawing.Point(10, 40);
			this.clampTrksCheckBox.Name = "clampTrksCheckBox";
			this.clampTrksCheckBox.Size = new System.Drawing.Size(170, 15);
			this.clampTrksCheckBox.TabIndex = 12;
			this.clampTrksCheckBox.Text = "tracks and routes";
			this.clampTrksCheckBox.CheckedChanged += new System.EventHandler(this.clampTrksCheckBox_CheckedChanged);
			// 
			// KmlDocumentControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox1});
			this.Name = "KmlDocumentControl";
			this.Size = new System.Drawing.Size(210, 85);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void clampWptsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.kmlOptions.WptsClampToGround = clampWptsCheckBox.Checked;
		}

		private void clampTrksCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.kmlOptions.TrksClampToGround = clampTrksCheckBox.Checked;
		}
	}
}
