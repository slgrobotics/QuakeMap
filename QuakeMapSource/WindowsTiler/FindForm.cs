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
using LibNet;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for FindForm.
	/// </summary>
	public class FindForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox findTextBox;
		private System.Windows.Forms.Button findButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox placesListBox;
		private System.Windows.Forms.Button goButton;
		private System.Windows.Forms.Label messageLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private CameraManager m_cameraManager;
		private bool m_firstTime;

		public FindForm(CameraManager cameraManager, bool firstTime)
		{
			m_cameraManager = cameraManager;
			m_firstTime = firstTime;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			goButton.Enabled = false;

			findTextBox.Text = Project.findKeyword;

			if(m_firstTime)
			{
				goButton.Text = "Continue";
				goFind();
			}
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
			this.findTextBox = new System.Windows.Forms.TextBox();
			this.findButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.placesListBox = new System.Windows.Forms.ListBox();
			this.goButton = new System.Windows.Forms.Button();
			this.messageLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// findTextBox
			// 
			this.findTextBox.Location = new System.Drawing.Point(16, 24);
			this.findTextBox.Name = "findTextBox";
			this.findTextBox.Size = new System.Drawing.Size(464, 20);
			this.findTextBox.TabIndex = 0;
			this.findTextBox.Text = "";
			this.findTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.findTextBox_KeyDown);
			// 
			// findButton
			// 
			this.findButton.Location = new System.Drawing.Point(488, 24);
			this.findButton.Name = "findButton";
			this.findButton.Size = new System.Drawing.Size(88, 23);
			this.findButton.TabIndex = 1;
			this.findButton.Text = "Find";
			this.findButton.Click += new System.EventHandler(this.findButton_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(15, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(561, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = "Enter US Zipcode or City Name:     (partial names OK)";
			// 
			// placesListBox
			// 
			this.placesListBox.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.placesListBox.ItemHeight = 12;
			this.placesListBox.Location = new System.Drawing.Point(15, 80);
			this.placesListBox.Name = "placesListBox";
			this.placesListBox.Size = new System.Drawing.Size(562, 184);
			this.placesListBox.TabIndex = 2;
			this.placesListBox.DoubleClick += new System.EventHandler(this.placesListBox_DoubleClick);
			this.placesListBox.SelectedIndexChanged += new System.EventHandler(this.placesListBox_SelectedIndexChanged);
			// 
			// goButton
			// 
			this.goButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.goButton.Location = new System.Drawing.Point(488, 52);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(88, 24);
			this.goButton.TabIndex = 3;
			this.goButton.Text = "Go There";
			this.goButton.Click += new System.EventHandler(this.goButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(16, 56);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(464, 16);
			this.messageLabel.TabIndex = 6;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FindForm
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(591, 276);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.messageLabel,
																		  this.goButton,
																		  this.placesListBox,
																		  this.label1,
																		  this.findButton,
																		  this.findTextBox});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FindForm";
			this.Text = "Find City by Name or Zipcode";
			this.ResumeLayout(false);

		}
		#endregion

		private void findButton_Click(object sender, System.EventArgs e)
		{
			goFind();
		}

		private void goFind()
		{
			Cursor.Current = Cursors.WaitCursor;
			placesListBox.Items.Clear();
			goButton.Enabled = false;
			Project.findKeyword = findTextBox.Text;
			TileCache.ZipcodeServer.getPlaces(placesListBox, messageLabel, Project.findKeyword);
			if(placesListBox.Items.Count > 0)
			{
				placesListBox.SelectedIndex = 0;
			}
			Cursor.Current = Cursors.Default;
		}

		private void goButton_Click(object sender, System.EventArgs e)
		{
			GeoCoord location = TileCache.ZipcodeServer.getCoord(placesListBox);
			if(location != null) 
			{
				Cursor.Current = Cursors.WaitCursor;
				m_cameraManager.SpoilPicture();
				m_cameraManager.Location = location;
				Cursor.Current = Cursors.Default;
				if(m_firstTime)
				{
					// first time, make this point a home in favorites:
					CamPos camPos = new CamPos(location.Lng, location.Lat, location.Elev);
					camPos.Name = "Home - " + placesListBox.SelectedItem;
					if(Project.favorites.Count > 0)
					{
						Project.favorites.Insert(0, camPos);
						Project.SaveFavorites();
					}
					else
					{
						Project.favorites.Add(camPos);
						Project.EnsureFavorites();
					}
				}
				this.Close();
			}
			else if(m_firstTime)
			{
				this.Close();
			}
		}
		
		/*
		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		*/

		private void placesListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			goButton.Enabled = true;
		}

		private void findTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyData) 
			{
				case Keys.Enter:
					goFind();
					break;
			}
		}

		private void placesListBox_DoubleClick(object sender, System.EventArgs e)
		{
			goButton_Click(null, EventArgs.Empty);
		}
	}
}
