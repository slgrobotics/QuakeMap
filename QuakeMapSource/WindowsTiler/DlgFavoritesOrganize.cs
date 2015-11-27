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
using System.Data;

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgFavoritesOrganize.
	/// </summary>
	public class DlgFavoritesOrganize : System.Windows.Forms.Form
	{
		private DataGridTableStyle dataGridTableStyle;
		private DataSet favoritesDataSet;
		private CameraManager m_cameraManager;

		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.DataGrid favoritesDataGrid;
		private System.Windows.Forms.Button gotoButton;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgFavoritesOrganize(CameraManager cameraManager)
		{
			m_cameraManager = cameraManager;

			InitializeComponent();

			makeFavoritesDataSet();
			makeDataGridTableStyle();

			int id = 0;
			foreach(CamPos camPos in Project.favorites)
			{
				GeoCoord loc = new GeoCoord(camPos);
				string sLoc = loc.ToString() + "  /  " + loc.heightToString();

				object[] values = new Object[7];
				values[0] = camPos.Name;
				values[1] = camPos.Lat;
				values[2] = camPos.Lng;
				values[3] = camPos.Elev;
				values[4] = sLoc;
				values[5] = id++;
				values[6] = camPos.Type;

				favoritesDataSet.Tables["favoritesTable"].LoadDataRow(values, true);
			}

			favoritesDataGrid.TableStyles.Add(this.dataGridTableStyle);
			favoritesDataGrid.SetDataBinding(favoritesDataSet, "favoritesTable");
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[favoritesDataGrid.DataSource, favoritesDataGrid.DataMember];
			((DataView)cm.List).AllowNew = false;
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
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.gotoButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.messageLabel = new System.Windows.Forms.Label();
			this.closeButton = new System.Windows.Forms.Button();
			this.favoritesDataGrid = new System.Windows.Forms.DataGrid();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.favoritesDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// panel2
			// 
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label1,
																				 this.gotoButton,
																				 this.deleteButton,
																				 this.messageLabel,
																				 this.closeButton});
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 266);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(533, 55);
			this.panel2.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(15, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 22);
			this.label1.TabIndex = 4;
			this.label1.Text = "Selected:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// gotoButton
			// 
			this.gotoButton.Location = new System.Drawing.Point(153, 5);
			this.gotoButton.Name = "gotoButton";
			this.gotoButton.Size = new System.Drawing.Size(74, 22);
			this.gotoButton.TabIndex = 2;
			this.gotoButton.Text = "Go There";
			this.gotoButton.Click += new System.EventHandler(this.gotoButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Location = new System.Drawing.Point(80, 5);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(69, 22);
			this.deleteButton.TabIndex = 1;
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.messageLabel.Location = new System.Drawing.Point(15, 35);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(1461, 16);
			this.messageLabel.TabIndex = 1;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(460, 5);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(68, 22);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// favoritesDataGrid
			// 
			this.favoritesDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.favoritesDataGrid.DataMember = "";
			this.favoritesDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.favoritesDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.favoritesDataGrid.Name = "favoritesDataGrid";
			this.favoritesDataGrid.Size = new System.Drawing.Size(533, 266);
			this.favoritesDataGrid.TabIndex = 3;
			// 
			// DlgFavoritesOrganize
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(533, 321);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.favoritesDataGrid,
																		  this.panel2});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "DlgFavoritesOrganize";
			this.Text = "Organize Favorites";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgFavoritesOrganize_Closing);
			this.panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.favoritesDataGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Making favoritesDataTable and favoritesDataStyle
		private void makeFavoritesDataSet()
		{
			DataTable favoritesDataTable = new DataTable();
			favoritesDataTable.TableName = "favoritesTable";

			DataColumn myDataColumn;

			// 
			// nameDataColumn
			// 
			myDataColumn = new DataColumn();
			myDataColumn.Caption = "Name";
			myDataColumn.ColumnName = "name";
			favoritesDataTable.Columns.Add(myDataColumn);
			// 
			// latDataColumn
			// 
			myDataColumn = new DataColumn();
			myDataColumn.Caption = "Latitude";
			myDataColumn.ColumnName = "lat";
			myDataColumn.DataType = typeof(System.Double);
			favoritesDataTable.Columns.Add(myDataColumn);
			// 
			// lngDataColumn
			// 
			myDataColumn = new DataColumn();
			myDataColumn.Caption = "Longitude";
			myDataColumn.ColumnName = "lng";
			myDataColumn.DataType = typeof(System.Double);
			favoritesDataTable.Columns.Add(myDataColumn);
			// 
			// elevDataColumn
			// 
			myDataColumn = new DataColumn();
			myDataColumn.Caption = "Camera Altitude";
			myDataColumn.ColumnName = "elev";
			myDataColumn.DataType = typeof(System.Double);
			favoritesDataTable.Columns.Add(myDataColumn);
			// 
			// locationDataColumn
			// 
			myDataColumn = new DataColumn();
			myDataColumn.Caption = "Location";
			myDataColumn.ColumnName = "location";
			myDataColumn.ReadOnly = true;
			favoritesDataTable.Columns.Add(myDataColumn);
			// 
			// idDataColumn
			// 
			myDataColumn = new DataColumn();
			myDataColumn.Caption = "Id";
			myDataColumn.ColumnName = "id";
			myDataColumn.DataType = typeof(System.Int32);
			favoritesDataTable.Columns.Add(myDataColumn);
			// 
			// typeDataColumn
			// 
			myDataColumn = new DataColumn();
			myDataColumn.Caption = "Type";
			myDataColumn.ColumnName = "type";
			favoritesDataTable.Columns.Add(myDataColumn);

			// Add the new DataTable to the DataSet.
			favoritesDataSet = new DataSet();
			favoritesDataSet.Tables.Add(favoritesDataTable);
		}

		private void makeDataGridTableStyle()
		{
			this.dataGridTableStyle = new DataGridTableStyle();

			this.dataGridTableStyle.DataGrid = this.favoritesDataGrid;
			this.dataGridTableStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridTableStyle.MappingName = "favoritesTable";

			// 
			// nameDataGridTextBoxColumn
			// 
			DataGridTextBoxColumn nameDataGridTextBoxColumn = new DataGridTextBoxColumn();
			nameDataGridTextBoxColumn.Format = "";
			nameDataGridTextBoxColumn.FormatInfo = null;
			nameDataGridTextBoxColumn.HeaderText = "Name";
			nameDataGridTextBoxColumn.MappingName = "name";
			nameDataGridTextBoxColumn.NullText = "";
			nameDataGridTextBoxColumn.Width = 200;
			this.dataGridTableStyle.GridColumnStyles.Add(nameDataGridTextBoxColumn);
			// 
			// locationDataGridTextBoxColumn
			// 
			DataGridTextBoxColumn locationDataGridTextBoxColumn = new DataGridTextBoxColumn();
			locationDataGridTextBoxColumn.Format = "";
			locationDataGridTextBoxColumn.FormatInfo = null;
			locationDataGridTextBoxColumn.HeaderText = "Location / Camera Altitude";
			locationDataGridTextBoxColumn.MappingName = "location";
			locationDataGridTextBoxColumn.NullText = "";
			locationDataGridTextBoxColumn.Width = 350;
			this.dataGridTableStyle.GridColumnStyles.Add(locationDataGridTextBoxColumn);

			// Create an int column style and add it to the tablestyle 
			// this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[favoritesDataSet, "favoritesTable"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 1; 
			dataGridTableStyle.GridColumnStyles.Add(csIDInt); 

			/*
			// 
			// latDataGridTextBoxColumn
			// 
			DataGridTextBoxColumn latDataGridTextBoxColumn = new DataGridTextBoxColumn();
			latDataGridTextBoxColumn.Format = "";
			latDataGridTextBoxColumn.FormatInfo = null;
			latDataGridTextBoxColumn.HeaderText = "Latitude";
			latDataGridTextBoxColumn.MappingName = "lat";
			latDataGridTextBoxColumn.NullText = "";
			latDataGridTextBoxColumn.Width = 75;
			this.dataGridTableStyle.GridColumnStyles.Add(latDataGridTextBoxColumn);
			// 
			// lngDataGridTextBoxColumn
			// 
			DataGridTextBoxColumn lngDataGridTextBoxColumn = new DataGridTextBoxColumn();
			lngDataGridTextBoxColumn.Format = "";
			lngDataGridTextBoxColumn.FormatInfo = null;
			lngDataGridTextBoxColumn.HeaderText = "Longitude";
			lngDataGridTextBoxColumn.MappingName = "lng";
			lngDataGridTextBoxColumn.NullText = "";
			lngDataGridTextBoxColumn.Width = 75;
			this.dataGridTableStyle.GridColumnStyles.Add(lngDataGridTextBoxColumn);
			// 
			// elevDataGridTextBoxColumn
			// 
			DataGridTextBoxColumn elevDataGridTextBoxColumn = new DataGridTextBoxColumn();
			elevDataGridTextBoxColumn.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
			elevDataGridTextBoxColumn.Format = "";
			elevDataGridTextBoxColumn.FormatInfo = null;
			elevDataGridTextBoxColumn.HeaderText = "Camera Altitude";
			elevDataGridTextBoxColumn.MappingName = "elev";
			elevDataGridTextBoxColumn.NullText = "";
			elevDataGridTextBoxColumn.Width = 120;
			this.dataGridTableStyle.GridColumnStyles.Add(elevDataGridTextBoxColumn);
			*/

			favoritesDataGrid.CaptionVisible = false;
		}
		#endregion

		private void DlgFavoritesOrganize_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// rebuild the Project.favorites list:
			Project.favorites.Clear();
			foreach(DataRow row in favoritesDataSet.Tables["favoritesTable"].Rows)
			{
				try
				{
					if(row.RowState != DataRowState.Deleted)
					{
						string type = (string)row["type"];
						string name = (string)row["name"];
						double lat  = (double)row["lat"];
						double lng  = (double)row["lng"];
						double elev = (double)row["elev"];

						CamPos camPos = new CamPos(lng, lat, elev, name, type);
						Project.favorites.Add(camPos);
					}
				} 
				catch 
				{
				}
			}
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		public ArrayList GetSelectedRows() 
		{
			ArrayList al = new ArrayList();

			for(int i = 0; i < ((DataSet)favoritesDataGrid.DataSource).Tables[0].Rows.Count; ++i)
			{
				if(favoritesDataGrid.IsSelected(i))
				{
					al.Add(i);
				}
			}
			return al;
		}


		private void gotoButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows();
			if(selected.Count > 0)
			{
				try
				{
					int row = (int)selected[0];
					int idx = (int)favoritesDataGrid[row, 2];
					DataTable table = favoritesDataSet.Tables["favoritesTable"];
					for(int i=0; i < table.Rows.Count ;i++)
					{
						if((int)table.Rows[i][5] == idx)
						{
							double lat  = (double)table.Rows[i][1];
							double lng  = (double)table.Rows[i][2];
							double elev = (double)table.Rows[i][3];
							string type = (string)table.Rows[i][6];

							GeoCoord newLoc = new GeoCoord(lng, lat, elev);
							if(type != null && type.Length > 0)
							{
								Project.drawTerraserverMode = type;
							}
							m_cameraManager.Location = newLoc;
						}
					}
				}
				catch (Exception exc)
				{
					LibSys.StatusBar.Error("Error: " + exc.Message);
				}
			} 
			else
			{
				messageLabel.Text = "Error: select a location first";
			}
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows();
			if(selected.Count > 0)
			{
				DataTable table = favoritesDataSet.Tables["favoritesTable"];
				int ii = selected.Count-1;
				while(ii >= 0)
				{
					int row = (int)selected[ii];
					try
					{
						int id = (int)favoritesDataGrid[row, 2];
						int i = table.Rows.Count-1;
						while(i >= 0 )
						{
							if(table.Rows[i].RowState != DataRowState.Deleted && (int)table.Rows[i][5] == id)
							{
								//LibSys.StatusBar.Trace("deleting: id=" + id + " row=" + i);
								table.Rows.RemoveAt(i);
							}
							i--;
						}
					}
					catch (Exception exc)
					{
						LibSys.StatusBar.Error(exc.Message);
					}
					ii--;
				}
			} 
			else
			{
				messageLabel.Text = "Error: select a location first";
			}
		}
	}
}
