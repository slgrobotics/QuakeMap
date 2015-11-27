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

using System.Xml;
using System.IO;

namespace LibSys
{
	/// <summary>
	/// Summary description for DictionaryEditorControl.
	/// </summary>
	public class DictionaryEditorControl : System.Windows.Forms.UserControl
	{
		private string m_filename;

		public Hashtable Dictionary
		{
			get 
			{
				Hashtable ret = new Hashtable();
				foreach(DataRow row in dictionaryDataTable.Rows)
				{
					string key = (string)row["Key"];
					string translation = (string)row["Translation"];
					ret.Add(key, translation);
				}
				return ret;
			}
		}

		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.Panel gridPanel;
		private System.Windows.Forms.DataGrid dictionaryDataGrid;
		private System.Data.DataSet dictionaryDataSet;
		private System.Windows.Forms.DataGridTableStyle mainDataGridTableStyle;
		private System.Windows.Forms.DataGridTextBoxColumn dataGridTextBoxColumnKey;
		private System.Windows.Forms.DataGridTextBoxColumn dataGridTextBoxColumnTranslation;
		private System.Data.DataTable dictionaryDataTable;
		private System.Data.DataColumn keyDataColumn;
		private System.Data.DataColumn translationDataColumn;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.TextBox fileNameTextBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DictionaryEditorControl(string filename)
		{
			m_filename = filename;

			InitializeComponent();

			fileNameTextBox.Text = m_filename;

			filename = m_filename + ".default.xml";			// default file to open

			// try saved version first, then fall back to default:
			if(File.Exists(m_filename))
			{
				readDictionaryFile(m_filename);
			}
			else if(File.Exists(filename))
			{
				readDictionaryFile(filename);
			}
			else
			{
				LibSys.StatusBar.Error("no dictionary at " + m_filename);
			}

			sizeTheColumns();
		}

		private void readDictionaryFile(string filename)
		{
			try
			{
				// Read the XML document back in. 
				System.IO.FileStream fsReadXml = new System.IO.FileStream(filename, System.IO.FileMode.Open);
				System.Xml.XmlTextReader myXmlReader = new System.Xml.XmlTextReader(fsReadXml);
				dictionaryDataSet.ReadXml(myXmlReader, XmlReadMode.IgnoreSchema);		// we already have schema
				myXmlReader.Close();

				dictionaryDataSet.AcceptChanges();
			}
			catch
			{
				LibSys.StatusBar.Error("bad dictionary at " + filename);
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
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.fileNameTextBox = new System.Windows.Forms.TextBox();
			this.saveButton = new System.Windows.Forms.Button();
			this.gridPanel = new System.Windows.Forms.Panel();
			this.dictionaryDataGrid = new System.Windows.Forms.DataGrid();
			this.dictionaryDataTable = new System.Data.DataTable();
			this.keyDataColumn = new System.Data.DataColumn();
			this.translationDataColumn = new System.Data.DataColumn();
			this.mainDataGridTableStyle = new System.Windows.Forms.DataGridTableStyle();
			this.dataGridTextBoxColumnKey = new System.Windows.Forms.DataGridTextBoxColumn();
			this.dataGridTextBoxColumnTranslation = new System.Windows.Forms.DataGridTextBoxColumn();
			this.dictionaryDataSet = new System.Data.DataSet();
			this.bottomPanel.SuspendLayout();
			this.gridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dictionaryDataGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dictionaryDataTable)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dictionaryDataSet)).BeginInit();
			this.SuspendLayout();
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.fileNameTextBox,
																					  this.saveButton});
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = new System.Drawing.Point(0, 325);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(575, 35);
			this.bottomPanel.TabIndex = 0;
			// 
			// fileNameTextBox
			// 
			this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.fileNameTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.fileNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.fileNameTextBox.Location = new System.Drawing.Point(5, 10);
			this.fileNameTextBox.Name = "fileNameTextBox";
			this.fileNameTextBox.ReadOnly = true;
			this.fileNameTextBox.Size = new System.Drawing.Size(450, 13);
			this.fileNameTextBox.TabIndex = 1;
			this.fileNameTextBox.Text = "";
			// 
			// saveButton
			// 
			this.saveButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.saveButton.Location = new System.Drawing.Point(460, 5);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(110, 23);
			this.saveButton.TabIndex = 0;
			this.saveButton.Text = "Save Dictionary";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// gridPanel
			// 
			this.gridPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.dictionaryDataGrid});
			this.gridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridPanel.Name = "gridPanel";
			this.gridPanel.Size = new System.Drawing.Size(575, 325);
			this.gridPanel.TabIndex = 1;
			// 
			// dictionaryDataGrid
			// 
			this.dictionaryDataGrid.AlternatingBackColor = System.Drawing.SystemColors.Control;
			this.dictionaryDataGrid.BackColor = System.Drawing.SystemColors.Control;
			this.dictionaryDataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
			this.dictionaryDataGrid.CaptionBackColor = System.Drawing.SystemColors.Control;
			this.dictionaryDataGrid.CaptionText = "Keywords from Template are replaced by words you specify here";
			this.dictionaryDataGrid.DataMember = "";
			this.dictionaryDataGrid.DataSource = this.dictionaryDataTable;
			this.dictionaryDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dictionaryDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dictionaryDataGrid.Name = "dictionaryDataGrid";
			this.dictionaryDataGrid.PreferredColumnWidth = 200;
			this.dictionaryDataGrid.Size = new System.Drawing.Size(575, 325);
			this.dictionaryDataGrid.TabIndex = 0;
			this.dictionaryDataGrid.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																										   this.mainDataGridTableStyle});
			// 
			// dictionaryDataTable
			// 
			this.dictionaryDataTable.Columns.AddRange(new System.Data.DataColumn[] {
																					   this.keyDataColumn,
																					   this.translationDataColumn});
			this.dictionaryDataTable.Constraints.AddRange(new System.Data.Constraint[] {
																						   new System.Data.UniqueConstraint("Constraint1", new string[] {
																																							"Key"}, false)});
			this.dictionaryDataTable.TableName = "DictionaryTable";
			// 
			// keyDataColumn
			// 
			this.keyDataColumn.AllowDBNull = false;
			this.keyDataColumn.Caption = "Key";
			this.keyDataColumn.ColumnName = "Key";
			// 
			// translationDataColumn
			// 
			this.translationDataColumn.AllowDBNull = false;
			this.translationDataColumn.Caption = "Translation";
			this.translationDataColumn.ColumnName = "Translation";
			this.translationDataColumn.DefaultValue = "";
			// 
			// mainDataGridTableStyle
			// 
			this.mainDataGridTableStyle.DataGrid = this.dictionaryDataGrid;
			this.mainDataGridTableStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																													 this.dataGridTextBoxColumnKey,
																													 this.dataGridTextBoxColumnTranslation});
			this.mainDataGridTableStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.mainDataGridTableStyle.MappingName = "DictionaryTable";
			this.mainDataGridTableStyle.PreferredColumnWidth = 200;
			// 
			// dataGridTextBoxColumnKey
			// 
			this.dataGridTextBoxColumnKey.Format = "";
			this.dataGridTextBoxColumnKey.FormatInfo = null;
			this.dataGridTextBoxColumnKey.HeaderText = "As seen in Template";
			this.dataGridTextBoxColumnKey.MappingName = "Key";
			this.dataGridTextBoxColumnKey.Width = 120;
			// 
			// dataGridTextBoxColumnTranslation
			// 
			this.dataGridTextBoxColumnTranslation.Format = "";
			this.dataGridTextBoxColumnTranslation.FormatInfo = null;
			this.dataGridTextBoxColumnTranslation.HeaderText = "Will appear in HTML as";
			this.dataGridTextBoxColumnTranslation.MappingName = "Translation";
			this.dataGridTextBoxColumnTranslation.Width = 300;
			// 
			// dictionaryDataSet
			// 
			this.dictionaryDataSet.DataSetName = "Dictionary";
			this.dictionaryDataSet.Locale = new System.Globalization.CultureInfo("en-US");
			this.dictionaryDataSet.Tables.AddRange(new System.Data.DataTable[] {
																				   this.dictionaryDataTable});
			// 
			// DictionaryEditorControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.gridPanel,
																		  this.bottomPanel});
			this.Name = "DictionaryEditorControl";
			this.Size = new System.Drawing.Size(575, 360);
			this.Resize += new System.EventHandler(this.DictionaryEditorControl_Resize);
			this.bottomPanel.ResumeLayout(false);
			this.gridPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dictionaryDataGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dictionaryDataTable)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dictionaryDataSet)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		public bool lastChanceToSave(bool forceSave)
		{
			bool ret = false;

			if(dictionaryDataSet.HasChanges())
			{
				if(forceSave || Project.YesNoBox(this.ParentForm, "You've changed the dictionary, want to save?"))
				{
					saveData();
					ret = true;
				}
				else
				{
					dictionaryDataSet.AcceptChanges();
				}
			}

			return ret;
		}

		private void saveButton_Click(object sender, System.EventArgs e)
		{
			saveData();
		}

		private void saveData()
		{
			dictionaryDataSet.AcceptChanges();

			// Write the XML schema and data to file with FileStream.
			System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(m_filename, System.Text.Encoding.Unicode);
			xmlWriter.Formatting = Formatting.Indented;
			xmlWriter.Indentation = 4;
			dictionaryDataSet.WriteXml(xmlWriter);
			xmlWriter.Close();
		}

		private void DictionaryEditorControl_Resize(object sender, System.EventArgs e)
		{
			sizeTheColumns();
		}

		private void sizeTheColumns()
		{
			this.dataGridTextBoxColumnTranslation.Width = this.Width - this.dataGridTextBoxColumnKey.Width - 40;
		}
	}
}
