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
using System.Threading;
using System.Data;

using LibSys;
using LibGui;
using LibGeo;
using LibGps;

namespace WindowsTiler
{
	/// <summary>
	/// DlgCustomMapsManager allows users to list and manage GeoTIFF and other custom maps.
	/// </summary>
	public class DlgCustomMapsManager : System.Windows.Forms.Form
	{
		private static DlgCustomMapsManager This = null;	// helps to ensure that only one dialog stays up

		private DataSet m_geotiffDS = null;
		private DataGridTableStyle m_geotiffTS = null;

		// ensure persistence of some columns' widths:
		private DataGridColumnStyle NameCol; 
		private DataGridColumnStyle DescrCol; 
		private DataGridColumnStyle SourceCol; 

		private System.Windows.Forms.Panel centerPanel;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.DataGrid geotiffDataGrid;
		private System.Windows.Forms.Button importButton;
		private System.Windows.Forms.Button zoomButton;
		private System.Windows.Forms.Button hideButton;
		private System.Windows.Forms.Button showButton;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label messageLabel;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgCustomMapsManager()
		{
			if(This != null)
			{
				This.Dispose();
				//GC.Collect();
				//GC.WaitForPendingFinalizers();
			}
			This = this;

			InitializeComponent();

			if(Project.fitsScreen(100, 100, Project.cmManagerWidth, Project.cmManagerHeight))
			{
				this.ClientSize = new System.Drawing.Size(Project.cmManagerWidth, Project.cmManagerHeight);
			}

			Project.setDlgIcon(this);
		}

		private void DlgCustomMapsManager_Load(object sender, System.EventArgs e)
		{
			rebuildGeotiffTab();		
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
			this.centerPanel = new System.Windows.Forms.Panel();
			this.geotiffDataGrid = new System.Windows.Forms.DataGrid();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.messageLabel = new System.Windows.Forms.Label();
			this.importButton = new System.Windows.Forms.Button();
			this.zoomButton = new System.Windows.Forms.Button();
			this.hideButton = new System.Windows.Forms.Button();
			this.showButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.centerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.geotiffDataGrid)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// centerPanel
			// 
			this.centerPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.centerPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.geotiffDataGrid});
			this.centerPanel.Location = new System.Drawing.Point(5, 5);
			this.centerPanel.Name = "centerPanel";
			this.centerPanel.Size = new System.Drawing.Size(750, 300);
			this.centerPanel.TabIndex = 0;
			// 
			// geotiffDataGrid
			// 
			this.geotiffDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.geotiffDataGrid.DataMember = "";
			this.geotiffDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.geotiffDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.geotiffDataGrid.Name = "geotiffDataGrid";
			this.geotiffDataGrid.Size = new System.Drawing.Size(750, 300);
			this.geotiffDataGrid.TabIndex = 2;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.bottomPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.messageLabel,
																					  this.importButton,
																					  this.zoomButton,
																					  this.hideButton,
																					  this.showButton,
																					  this.deleteButton,
																					  this.closeButton});
			this.bottomPanel.Location = new System.Drawing.Point(5, 310);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(750, 50);
			this.bottomPanel.TabIndex = 1;
			// 
			// messageLabel
			// 
			this.messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.messageLabel.Location = new System.Drawing.Point(5, 35);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(740, 12);
			this.messageLabel.TabIndex = 20;
			// 
			// importButton
			// 
			this.importButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.importButton.Location = new System.Drawing.Point(575, 5);
			this.importButton.Name = "importButton";
			this.importButton.Size = new System.Drawing.Size(68, 21);
			this.importButton.TabIndex = 18;
			this.importButton.Text = "Import";
			this.importButton.Click += new System.EventHandler(this.importButton_Click);
			// 
			// zoomButton
			// 
			this.zoomButton.Location = new System.Drawing.Point(135, 5);
			this.zoomButton.Name = "zoomButton";
			this.zoomButton.Size = new System.Drawing.Size(55, 21);
			this.zoomButton.TabIndex = 16;
			this.zoomButton.Text = "Zoom";
			this.zoomButton.Click += new System.EventHandler(this.zoomButton_Click);
			// 
			// hideButton
			// 
			this.hideButton.Location = new System.Drawing.Point(70, 5);
			this.hideButton.Name = "hideButton";
			this.hideButton.Size = new System.Drawing.Size(55, 21);
			this.hideButton.TabIndex = 15;
			this.hideButton.Text = "Hide";
			this.hideButton.Click += new System.EventHandler(this.hideButton_Click);
			// 
			// showButton
			// 
			this.showButton.Location = new System.Drawing.Point(5, 5);
			this.showButton.Name = "showButton";
			this.showButton.Size = new System.Drawing.Size(55, 21);
			this.showButton.TabIndex = 14;
			this.showButton.Text = "Show";
			this.showButton.Click += new System.EventHandler(this.showButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Location = new System.Drawing.Point(200, 5);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(80, 21);
			this.deleteButton.TabIndex = 17;
			this.deleteButton.Text = "Remove";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(680, 5);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(69, 22);
			this.closeButton.TabIndex = 19;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// DlgCustomMapsManager
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(759, 361);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.bottomPanel,
																		  this.centerPanel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "DlgCustomMapsManager";
			this.Text = "GeoTIFF Maps Manager";
			this.Resize += new System.EventHandler(this.DlgCustomMapsManager_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgCustomMapsManager_Closing);
			this.Load += new System.EventHandler(this.DlgCustomMapsManager_Load);
			this.centerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.geotiffDataGrid)).EndInit();
			this.bottomPanel.ResumeLayout(false);
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
					this.Close();
					break;
				default:
					keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					break;
			}
			if(keystrokeProcessed) 
			{
				return true;
			} 
			else 
			{
				return base.ProcessDialogKey(keyData);
			}
		}

		private bool rebuildingCustomMaps = false;

		private void rebuildGeotiffTab()
		{
			geotiffDataGrid.SuspendLayout();
			makeGeotiffDS();
			makeGeotiffDataGridStyle();
			geotiffDataGrid.SetDataBinding(m_geotiffDS, "geotiff");
			//no adding of new rows thru dataview... 
			CurrencyManager cm = (CurrencyManager)this.BindingContext[geotiffDataGrid.DataSource, geotiffDataGrid.DataMember];
			DataView dv = (DataView)cm.List;
			//dv.AllowEdit = false;
			dv.AllowNew = false;		// causes selecting by arrows and shift to throw exception when selection reaches end and you press arrow up.
			selectCustomMapRow();
			geotiffDataGrid.ResumeLayout();
		}

		private void makeGeotiffDataGridStyle()
		{
			if(m_geotiffTS != null)
			{
				return;
			}

			//STEP 1: Create a DataTable style object and set properties if required. 
			m_geotiffTS = new DataGridTableStyle(); 
			//specify the table from dataset (required step) 
			m_geotiffTS.MappingName = "geotiff"; 
			// Set other properties (optional step) 
			//m_geotiffTS.AlternatingBackColor = Color.LightBlue; 

			int colCount = 0;

			//STEP 1: Create an int column style and add it to the tablestyle 
			//this requires setting the format for the column through its property descriptor 
			PropertyDescriptorCollection pdc = this.BindingContext[m_geotiffDS, "geotiff"].GetItemProperties(); 
			//now created a formated column using the pdc 
			DataGridDigitsTextBoxColumn csIDInt = new DataGridDigitsTextBoxColumn(pdc["id"], "i", true); 
			csIDInt.MappingName = "id"; 
			csIDInt.HeaderText = ""; 
			csIDInt.Width = 30;
			csIDInt.ReadOnly = true;
			m_geotiffTS.GridColumnStyles.Add(csIDInt); 
			colCount++;

			//STEP 2: Create a string column and add it to the tablestyle 
			NameCol = new DataGridTextBoxColumn(); 
			NameCol.MappingName = "name"; //from dataset table 
			NameCol.HeaderText = "Name"; 
			NameCol.Width = Math.Max(10, Project.nameColWidthCm); 
			NameCol.ReadOnly = true;
			m_geotiffTS.GridColumnStyles.Add(NameCol); 
			colCount++;

			//STEP 3: Add the checkbox 
			DataGridColumnStyle boolCol = new MyDataGridBoolColumn(colCount); 
			boolCol.MappingName = "displayed"; 
			boolCol.HeaderText = "Shown"; 
			//hook the new event to our handler in the grid
			((MyDataGridBoolColumn)boolCol).BoolValueChanged += new BoolValueChangedEventHandler(HandleCustomMapShowChanges);
			//uncomment this line to get a two-state checkbox 
			((DataGridBoolColumn)boolCol).AllowNull = false; 
			boolCol.Width = 45; 
			m_geotiffTS.GridColumnStyles.Add(boolCol); 
			colCount++;

			//STEP 3: Add the checkbox 
			DataGridColumnStyle boolCol2 = new MyDataGridBoolColumn(colCount); 
			boolCol2.MappingName = "persist"; 
			boolCol2.HeaderText = "Load on start"; 
			//hook the new event to our handler in the grid
			((MyDataGridBoolColumn)boolCol2).BoolValueChanged += new BoolValueChangedEventHandler(HandleCustomMapPersistChanges);
			//uncomment this line to get a two-state checkbox 
			((DataGridBoolColumn)boolCol2).AllowNull = false; 
			boolCol2.Width = 80; 
			m_geotiffTS.GridColumnStyles.Add(boolCol2); 
			colCount++;

			//STEP 4: Create a string column and add it to the tablestyle 
			DescrCol = new DataGridTextBoxColumn(); 
			DescrCol.MappingName = "descr"; //from dataset table 
			DescrCol.HeaderText = "Description"; 
			//DescrCol.Width = Math.Max(10, Project.descColWidthCm); 
			DescrCol.Width = 1; 
			DescrCol.ReadOnly = true;
			m_geotiffTS.GridColumnStyles.Add(DescrCol); 
			colCount++;

			//STEP 5: Create a string column and add it to the tablestyle
			SourceCol = new DataGridTextBoxColumn();
			SourceCol.MappingName = "source"; //from dataset table
			SourceCol.HeaderText = "Source";
			int colWidthLeft = Math.Max(100, this.Width - csIDInt.Width - NameCol.Width - boolCol.Width - DescrCol.Width - 140);
			SourceCol.Width = Math.Max(colWidthLeft, 100); // Project.sourceColWidthCm);
			SourceCol.ReadOnly = true;
			m_geotiffTS.GridColumnStyles.Add(SourceCol);
			colCount++;

			geotiffDataGrid.CaptionVisible = false;

			//STEP 6: Add the tablestyle to your datagrid's tablestlye collection:
			geotiffDataGrid.TableStyles.Add(m_geotiffTS);

			/* how to test for checked checkboxes:
			if((bool)geotiffDataGrid[row, column]) 
				MessageBox.Show("I am true"); 
			else 
				MessageBox.Show("I am false");
			*/
		}

		private void makeGeotiffDS()
		{
			DataTable geotiffTable;

			rebuildingCustomMaps = true;

			// we cannot reuse the table, as boolean "Show" checkboxes will otherwise have their events messed up badly.
			if(m_geotiffDS != null)
			{
				m_geotiffDS.Dispose();
			}

			geotiffTable = new DataTable("geotiff");

			DataColumn myDataColumn;

			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "id";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			geotiffTable.Columns.Add(myDataColumn);

			// Create name column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "name";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Name";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			geotiffTable.Columns.Add(myDataColumn);

			// Create source column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "source";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Source";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			geotiffTable.Columns.Add(myDataColumn);

			// Create descr column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "descr";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Description";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = false;
			geotiffTable.Columns.Add(myDataColumn);

			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "displayed";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Display";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			geotiffTable.Columns.Add(myDataColumn);

			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Boolean");
			myDataColumn.ColumnName = "persist";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "Persist";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			geotiffTable.Columns.Add(myDataColumn);

			// Make the ID column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = geotiffTable.Columns["id"];
			geotiffTable.PrimaryKey = PrimaryKeyColumns;

			// Add the new DataTable to the DataSet.
			m_geotiffDS = new DataSet();
			m_geotiffDS.Tables.Add(geotiffTable);
 
			geotiffTable = m_geotiffDS.Tables[0];
			geotiffTable.ColumnChanging -= new DataColumnChangeEventHandler(this.geotiff_ColumnChanging);
			geotiffTable.RowDeleting -= new DataRowChangeEventHandler(this.geotiff_RowDeleting);

			// Create DataRow objects and add them to the DataTable
			DataRow myDataRow;
			for (int i = 0; i < CustomMapsCache.CustomMapsAll.Count; i++)
			{
				CustomMap customMap = (CustomMap)CustomMapsCache.CustomMapsAll[i];
				myDataRow = geotiffTable.NewRow();
				myDataRow["id"] = customMap.Id;
				
				myDataRow["name"] = customMap.MapName;
				myDataRow["descr"] = customMap.Desc;
				myDataRow["source"] = "" + customMap.Source;
				myDataRow["displayed"] = customMap.Enabled;
				myDataRow["persist"] = customMap.Persist;
				geotiffTable.Rows.Add(myDataRow);
			}

			geotiffTable.ColumnChanging += new DataColumnChangeEventHandler(this.geotiff_ColumnChanging);
			geotiffTable.RowDeleting += new DataRowChangeEventHandler(this.geotiff_RowDeleting);

			rebuildingCustomMaps = false;
		}

		private void HandleCustomMapShowChanges(object sender, BoolValueChangedEventArgs e)
		{
#if DEBUG
			string str = "Show changed: row " + e.Row + "   value " + e.BoolValue;
#endif
			int column = 0;
			DataTable table = m_geotiffDS.Tables[0];
			int row = e.Row;
			long geotiffId = (long)((int)geotiffDataGrid[row, column]);
			CustomMap cm = CustomMapsCache.getCustomMapById(geotiffId);
#if DEBUG
			str += "  " + row + "=" + geotiffId + " ";
#endif
			if(cm != null)
			{
				cm.Enabled = e.BoolValue;
				PutOnMap();
			}
#if DEBUG
			LibSys.StatusBar.Trace(str);
			//messageLabel.Text = str;
#endif
		}

		private void HandleCustomMapPersistChanges(object sender, BoolValueChangedEventArgs e)
		{
#if DEBUG
			string str = "Persist changed: row " + e.Row + "   value " + e.BoolValue;
#endif
			int column = 0;
			DataTable table = m_geotiffDS.Tables[0];
			int row = e.Row;
			long geotiffId = (long)((int)geotiffDataGrid[row, column]);
			CustomMap cm = CustomMapsCache.getCustomMapById(geotiffId);
#if DEBUG
			str += "  " + row + "=" + geotiffId + " ";
#endif
			if(cm != null)
			{
				cm.Persist = e.BoolValue;
				//PutOnMap();
			}
#if DEBUG
			LibSys.StatusBar.Trace(str);
			//messageLabel.Text = str;
#endif
		}

		private void geotiff_ColumnChanging(object sender, System.Data.DataColumnChangeEventArgs e) 
		{
		}

		private bool deletingCustomMaps = false;

		private void geotiff_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e) 
		{
			if (!rebuildingCustomMaps && !deletingCustomMaps) 
			{
				try 
				{
					int column = 0;
					long cmId = (long)((int)e.Row[column]);
					unlistCustomMap(cmId);
					CustomMapsCache.RemoveCustomMapById(cmId);
					PutOnMap();
				} 
				catch {}
			}
		}

		private void unlistCustomMap(long id)
		{
			CustomMap cm = CustomMapsCache.getCustomMapById(id);

			if(cm != null)
			{
				for(int i=0;  i < Project.FileDescrList.Count ;i++)
				{
					FormattedFileDescr ffd = (FormattedFileDescr)Project.FileDescrList[i];
					if(ffd.filename.Equals(cm.Source))
					{
						Project.FileDescrList.RemoveAt(i);
						break;
					}
				}
			}
		}

		private void selectCustomMapRow()
		{
		}

		// call PutOnMap() when set of CustomMaps changed, but the view point doesn't move:
		private void PutOnMap()
		{
			LayerCustomMaps.This.init();		// provoke PutOnMap on layer's Paint()
			PictureManager.This.Refresh();
		}

		private void DlgCustomMapsManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try		// columns may be not initialized yet
			{
				Project.nameColWidthCm = NameCol.Width;
				Project.descColWidthCm = DescrCol.Width;
				Project.sourceColWidthCm = SourceCol.Width;
			} 
			catch {}

			//PictureManager.This.Refresh();

			This = null;
		}

		private void DlgCustomMapsManager_Resize(object sender, System.EventArgs e)
		{
			Project.cmManagerWidth = this.ClientSize.Width;
			Project.cmManagerHeight = this.ClientSize.Height;
		}

		private void ClearMessages()
		{
			messageLabel.Text = "";
		}

		public ArrayList GetSelectedRows(DataGrid dataGrid) 
		{ 
			ArrayList al = new ArrayList(); 
 
			for(int i = 0; i < ((DataSet)dataGrid.DataSource).Tables[0].Rows.Count; ++i) 
			{ 
				if(dataGrid.IsSelected(i))
				{
					al.Add(i);
				}
			} 
			return al; 
		} 

		private void showButton_Click(object sender, System.EventArgs e)
		{
			setCustomMapsEnabled(geotiffDataGrid, m_geotiffDS, true, showButton);
		}

		private void hideButton_Click(object sender, System.EventArgs e)
		{
			setCustomMapsEnabled(geotiffDataGrid, m_geotiffDS, false, hideButton);
		}

		private void setCustomMapsEnabled(DataGrid dataGrid, DataSet dataSet, bool enabled, Control owner)
		{
			ArrayList selected = GetSelectedRows(dataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				int column = 0;
				DataTable table = dataSet.Tables[0];
				foreach(int row in selected)
				{
					long cmId = (long)((int)geotiffDataGrid[row, column]);
					CustomMap cm = CustomMapsCache.getCustomMapById(cmId);
					if(cm != null)
					{
						// rows in the table are numbered differently than rows in the grid, so we go by track ID:
						for(int indx=0; indx < table.Rows.Count ;indx++ )
						{
							long rowCustomMapId = (long)((int)table.Rows[indx][column]);
							if(rowCustomMapId == cmId)
							{
								cm.Enabled = enabled;
								table.Rows[indx]["displayed"] = enabled;
								break;
							}
						}
					}
				}
				PutOnMap();
			}
			else
			{
				Project.ShowPopup(owner, "\nselect a custom map first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void zoomButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(geotiffDataGrid);
			if(selected.Count > 0)
			{
				ClearMessages();
				CustomMapsCache.resetBoundaries();
				int column = 0;
				DataTable table = m_geotiffDS.Tables[0];
				foreach(int row in selected)
				{
					long cmId = (long)((int)geotiffDataGrid[row, column]);
					CustomMap cm = CustomMapsCache.getCustomMapById(cmId);
					if(cm != null)
					{
						CustomMapsCache.pushBoundaries(cm.TopLeft);
						CustomMapsCache.pushBoundaries(cm.BottomRight);
					}
				}
				PictureManager.This.CameraManager.zoomToCorners(CustomMapsCache.TopLeft, CustomMapsCache.BottomRight);
			}		
			else
			{
				Project.ShowPopup(zoomButton, "\nselect a custom map first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			ArrayList selected = GetSelectedRows(geotiffDataGrid);
			if(selected.Count > 0)
			{
				deletingCustomMaps = true;
				ClearMessages();
#if DEBUG
				string str = "";
#endif
				int column = 0;
				DataTable table = m_geotiffDS.Tables[0];
				for(int i=selected.Count-1; i >=0 ;i--)
				{
					int row = (int)selected[i];
					long cmId = (long)((int)geotiffDataGrid[row, column]);
#if DEBUG
					str += "" + row + "=" + cmId + " ";
#endif
					// rows in the table are numbered differently than rows in the grid, so we go by CustomMap ID:
					for(int indx=0; indx < table.Rows.Count ;indx++ )
					{
						long rowCustomMapId = (long)((int)table.Rows[indx][column]);
						if(rowCustomMapId == cmId)
						{
							table.Rows[indx].Delete();
							unlistCustomMap(cmId);
							CustomMapsCache.RemoveCustomMapById(cmId);
							break;
						}
					}
				}
#if DEBUG
				messageLabel.Text = "selected: " + str;
#else
				messageLabel.Text = "";
#endif
				PutOnMap();
				deletingCustomMaps = false;
			}
			else
			{
				//messageTrkLabel.Text = "Error: select a GeoTIFF map to delete first";
				Project.ShowPopup(deleteButton, "\nselect a custom map to delete first" + Project.SELECT_HELP, Point.Empty);
			}
		}

		private void importButton_Click(object sender, System.EventArgs e)
		{
			FileImportForm fileImportForm = new FileImportForm(false, FileImportForm.MODE_GPS, FileImportForm.TYPE_GEOTIFF);
			fileImportForm.ShowDialog();
			rebuildGeotiffTab();			// import may add maps, we need to see new ones in the grid
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
