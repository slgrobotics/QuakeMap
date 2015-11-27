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

using LibSys;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgZipcode.
	/// </summary>
	public class DlgZipcode : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox zipcodeTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button closeButton;
		public System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgZipcode()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			zipcodeTextBox.Text = Project.zipcode;
			Project.setDlgIcon(this);
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zipcodeTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.closeButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// zipcodeTextBox
			// 
			this.zipcodeTextBox.Location = new System.Drawing.Point(153, 61);
			this.zipcodeTextBox.Name = "zipcodeTextBox";
			this.zipcodeTextBox.Size = new System.Drawing.Size(81, 20);
			this.zipcodeTextBox.TabIndex = 0;
			this.zipcodeTextBox.Text = "";
			this.zipcodeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.zipcodeTextBox_KeyDown);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(66, 61);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 21);
			this.label5.TabIndex = 7;
			this.label5.Text = "ZIP code:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// closeButton
			// 
			this.closeButton.Location = new System.Drawing.Point(248, 61);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(95, 21);
			this.closeButton.TabIndex = 1;
			this.closeButton.Text = "Continue";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label2.Location = new System.Drawing.Point(19, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(361, 37);
			this.label2.TabIndex = 13;
			this.label2.Text = "Please enter your Postal ZIP code here so that we can place \"Home Location\" in yo" +
				"ur Favorites list";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// DlgZipcode
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(396, 96);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label2,
																		  this.closeButton,
																		  this.label5,
																		  this.zipcodeTextBox});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DlgZipcode";
			this.Text = "Home Location";
			this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			saveData();
			this.Close();
		}

		private void saveData()
		{
            Project.zipcode = zipcodeTextBox.Text;

            Project.SaveOptions();
		}

		private void zipcodeTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyData) 
			{
				case Keys.Enter:
					saveData();
					this.Close();
					break;
			}
		}
	}
}
