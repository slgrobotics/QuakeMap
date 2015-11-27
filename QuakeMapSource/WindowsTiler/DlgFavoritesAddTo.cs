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
using LibGeo;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgFavoritesAddTo.
	/// </summary>
	public class DlgFavoritesAddTo : System.Windows.Forms.Form
	{
		private CamPos m_camPos;
		private MenuItem m_parentMenuItem;
		private EventHandler m_eventHandler;

		private System.Windows.Forms.Label locationLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.ComboBox nameComboBox;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		// closest is SortedList of string (name) by key=double (distance meters)
		public DlgFavoritesAddTo(CamPos camPos, SortedList closest, MenuItem parentMenuItem, EventHandler eventHandler)
		{
			m_camPos = camPos;
			m_parentMenuItem = parentMenuItem;
			m_eventHandler = eventHandler;

			InitializeComponent();

			GeoCoord loc = new GeoCoord(m_camPos);

			Distance camHD = new Distance(m_camPos.H);
			int unitsCompl = camHD.UnitsCompl;
			locationLabel.Text = "Location: " + loc.ToString() + " / Camera at " + camHD.ToString(unitsCompl);
			nameComboBox.Text = m_camPos.Name;
			for(int i=0; i < closest.Count && i < 20 ;i++)
			{
				string name = ((LiveObject)closest.GetByIndex(i)).Name;
				nameComboBox.Items.Add(name);
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
			this.locationLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.addButton = new System.Windows.Forms.Button();
			this.nameComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// locationLabel
			// 
			this.locationLabel.Location = new System.Drawing.Point(10, 10);
			this.locationLabel.Name = "locationLabel";
			this.locationLabel.Size = new System.Drawing.Size(380, 22);
			this.locationLabel.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(10, 70);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 22);
			this.label1.TabIndex = 2;
			this.label1.Text = "Name:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// addButton
			// 
			this.addButton.Location = new System.Drawing.Point(325, 105);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(70, 21);
			this.addButton.TabIndex = 2;
			this.addButton.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// nameComboBox
			// 
			this.nameComboBox.Location = new System.Drawing.Point(60, 70);
			this.nameComboBox.Name = "nameComboBox";
			this.nameComboBox.Size = new System.Drawing.Size(336, 21);
			this.nameComboBox.TabIndex = 1;
			this.nameComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.nameComboBox_KeyDown);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(10, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(380, 22);
			this.label2.TabIndex = 5;
			this.label2.Text = "Pick up a name from the list or type it in:";
			// 
			// DlgFavoritesAddTo
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(404, 138);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label2,
																		  this.nameComboBox,
																		  this.addButton,
																		  this.label1,
																		  this.locationLabel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgFavoritesAddTo";
			this.Text = "Add To Favorites";
			this.ResumeLayout(false);

		}
		#endregion

		private void goAdd()
		{
			m_camPos.Name = nameComboBox.Text;

			Project.favorites.Add(m_camPos);

			MenuItem menuItem = new MenuItem();

			menuItem.Text = m_camPos.Name;
			menuItem.Click += m_eventHandler;
			menuItem.Index = m_parentMenuItem.MenuItems.Count;

			m_parentMenuItem.MenuItems.Add(menuItem);

			this.Close();

			Project.SaveFavorites();		// save favorites
		}

		private void addButton_Click(object sender, System.EventArgs e)
		{
			goAdd();
		}

		private void nameComboBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyData) 
			{
				case Keys.Enter:
					goAdd();
					break;
			}
		}
	}
}
