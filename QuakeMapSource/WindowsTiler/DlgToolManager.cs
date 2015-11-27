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
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using LibSys;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgToolManager.
	/// </summary>
	public class DlgToolManager : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.ListBox toolsListBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox displayNameTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox executableTextBox;
		private System.Windows.Forms.TextBox argumentsTextBox;
		private System.Windows.Forms.Button upButton;
		private System.Windows.Forms.Button downButton;
		private System.Windows.Forms.Button addToolButton;
		private System.Windows.Forms.Button removeToolButton;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Label helpLabel;
		private System.Windows.Forms.CheckBox browserCheckBox;
		private System.Windows.Forms.Label exeLabel;
		private System.Windows.Forms.Label argsLabel;
		private System.Windows.Forms.Button runButton;
		private System.Windows.Forms.Label validationLabel;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgToolManager()
		{
			InitializeComponent();
		}

		private void DlgToolManager_Load(object sender, System.EventArgs e)
		{
			refreshToolsListBox(null);

			Project.setDlgIcon(this);

			helpLabel.Text = "in the Arguments or URL field the %lat% and %lon% tokens will be substituted for actual coordinates in the center of the map or at the right-click point.\n\"%file%\" stands for latest recent .GPX or .LOC file.";

			toolsListBox.Focus();
		}

		private void refreshToolsListBox(ToolDescr td)
		{
			toolsListBox.DataSource = null;
			toolsListBox.DataSource = Project.tools.tools;
			if(td != null)
			{
				toolsListBox.SelectedItem = td;
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

		private void DlgToolManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Project.tools.Save();		// wont do anything if !isDirty
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.closeButton = new System.Windows.Forms.Button();
			this.toolsListBox = new System.Windows.Forms.ListBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.validationLabel = new System.Windows.Forms.Label();
			this.browserCheckBox = new System.Windows.Forms.CheckBox();
			this.helpLabel = new System.Windows.Forms.Label();
			this.argsLabel = new System.Windows.Forms.Label();
			this.argumentsTextBox = new System.Windows.Forms.TextBox();
			this.exeLabel = new System.Windows.Forms.Label();
			this.executableTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.displayNameTextBox = new System.Windows.Forms.TextBox();
			this.upButton = new System.Windows.Forms.Button();
			this.downButton = new System.Windows.Forms.Button();
			this.addToolButton = new System.Windows.Forms.Button();
			this.removeToolButton = new System.Windows.Forms.Button();
			this.browseButton = new System.Windows.Forms.Button();
			this.runButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(520, 368);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(72, 23);
			this.closeButton.TabIndex = 1;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// toolsListBox
			// 
			this.toolsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolsListBox.Location = new System.Drawing.Point(3, 16);
			this.toolsListBox.Name = "toolsListBox";
			this.toolsListBox.Size = new System.Drawing.Size(178, 329);
			this.toolsListBox.TabIndex = 0;
			this.toolsListBox.SelectedIndexChanged += new System.EventHandler(this.toolsListBox_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.toolsListBox});
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(184, 352);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Tools";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label2,
																					this.validationLabel,
																					this.browserCheckBox,
																					this.helpLabel,
																					this.argsLabel,
																					this.argumentsTextBox,
																					this.exeLabel,
																					this.executableTextBox,
																					this.label1,
																					this.displayNameTextBox});
			this.groupBox2.Location = new System.Drawing.Point(216, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(376, 352);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Selected";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(8, 165);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 23);
			this.label2.TabIndex = 9;
			this.label2.Text = "Hint:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// validationLabel
			// 
			this.validationLabel.Location = new System.Drawing.Point(24, 232);
			this.validationLabel.Name = "validationLabel";
			this.validationLabel.Size = new System.Drawing.Size(328, 104);
			this.validationLabel.TabIndex = 8;
			// 
			// browserCheckBox
			// 
			this.browserCheckBox.Location = new System.Drawing.Point(104, 64);
			this.browserCheckBox.Name = "browserCheckBox";
			this.browserCheckBox.Size = new System.Drawing.Size(248, 24);
			this.browserCheckBox.TabIndex = 7;
			this.browserCheckBox.Text = "run in browser";
			this.browserCheckBox.CheckedChanged += new System.EventHandler(this.browserCheckBox_CheckedChanged);
			// 
			// helpLabel
			// 
			this.helpLabel.Location = new System.Drawing.Point(56, 168);
			this.helpLabel.Name = "helpLabel";
			this.helpLabel.Size = new System.Drawing.Size(296, 56);
			this.helpLabel.TabIndex = 6;
			// 
			// argsLabel
			// 
			this.argsLabel.Location = new System.Drawing.Point(16, 128);
			this.argsLabel.Name = "argsLabel";
			this.argsLabel.Size = new System.Drawing.Size(80, 23);
			this.argsLabel.TabIndex = 5;
			this.argsLabel.Text = "Arguments:";
			this.argsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// argumentsTextBox
			// 
			this.argumentsTextBox.Location = new System.Drawing.Point(104, 128);
			this.argumentsTextBox.Name = "argumentsTextBox";
			this.argumentsTextBox.Size = new System.Drawing.Size(248, 20);
			this.argumentsTextBox.TabIndex = 4;
			this.argumentsTextBox.Text = "";
			this.argumentsTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.argumentsTextBox_Validating);
			this.argumentsTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.argumentsTextBox_KeyUp);
			// 
			// exeLabel
			// 
			this.exeLabel.Location = new System.Drawing.Point(16, 96);
			this.exeLabel.Name = "exeLabel";
			this.exeLabel.Size = new System.Drawing.Size(80, 23);
			this.exeLabel.TabIndex = 3;
			this.exeLabel.Text = "Executable:";
			this.exeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// executableTextBox
			// 
			this.executableTextBox.Location = new System.Drawing.Point(104, 96);
			this.executableTextBox.Name = "executableTextBox";
			this.executableTextBox.Size = new System.Drawing.Size(216, 20);
			this.executableTextBox.TabIndex = 2;
			this.executableTextBox.Text = "";
			this.executableTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.executableTextBox_Validating);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(5, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 23);
			this.label1.TabIndex = 1;
			this.label1.Text = "Display Name:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// displayNameTextBox
			// 
			this.displayNameTextBox.Location = new System.Drawing.Point(104, 32);
			this.displayNameTextBox.Name = "displayNameTextBox";
			this.displayNameTextBox.Size = new System.Drawing.Size(248, 20);
			this.displayNameTextBox.TabIndex = 0;
			this.displayNameTextBox.Text = "";
			this.displayNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.displayNameTextBox_KeyDown);
			this.displayNameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.displayNameTextBox_Validating);
			// 
			// upButton
			// 
			this.upButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.upButton.Location = new System.Drawing.Point(192, 32);
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(16, 23);
			this.upButton.TabIndex = 4;
			this.upButton.Text = "5";
			this.upButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.upButton.Click += new System.EventHandler(this.upButton_Click);
			// 
			// downButton
			// 
			this.downButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.downButton.Location = new System.Drawing.Point(192, 56);
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(16, 23);
			this.downButton.TabIndex = 5;
			this.downButton.Text = "6";
			this.downButton.Click += new System.EventHandler(this.downButton_Click);
			// 
			// addToolButton
			// 
			this.addToolButton.Location = new System.Drawing.Point(16, 368);
			this.addToolButton.Name = "addToolButton";
			this.addToolButton.Size = new System.Drawing.Size(48, 23);
			this.addToolButton.TabIndex = 6;
			this.addToolButton.Text = "New";
			this.addToolButton.Click += new System.EventHandler(this.addToolButton_Click);
			// 
			// removeToolButton
			// 
			this.removeToolButton.Location = new System.Drawing.Point(128, 368);
			this.removeToolButton.Name = "removeToolButton";
			this.removeToolButton.Size = new System.Drawing.Size(64, 23);
			this.removeToolButton.TabIndex = 7;
			this.removeToolButton.Text = "Remove";
			this.removeToolButton.Click += new System.EventHandler(this.removeToolButton_Click);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(544, 104);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 23);
			this.browseButton.TabIndex = 8;
			this.browseButton.Text = "...";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// runButton
			// 
			this.runButton.Location = new System.Drawing.Point(224, 368);
			this.runButton.Name = "runButton";
			this.runButton.Size = new System.Drawing.Size(64, 23);
			this.runButton.TabIndex = 9;
			this.runButton.Text = "Run";
			this.runButton.Click += new System.EventHandler(this.runButton_Click);
			// 
			// DlgToolManager
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(600, 406);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.runButton,
																		  this.browseButton,
																		  this.removeToolButton,
																		  this.addToolButton,
																		  this.downButton,
																		  this.upButton,
																		  this.groupBox2,
																		  this.groupBox1,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DlgToolManager";
			this.Text = "Tools Manager";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgToolManager_Closing);
			this.Load += new System.EventHandler(this.DlgToolManager_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region clickers

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			Close();		
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

			if(File.Exists(this.executableTextBox.Text))
			{
				openFileDialog.FileName = this.executableTextBox.Text;
			}
			else
			{
				openFileDialog.InitialDirectory = this.executableTextBox.Text;
			}
			openFileDialog.DefaultExt = ".exe";
			openFileDialog.AddExtension = false;
			// "Text files (*.txt)|*.txt|All files (*.*)|*.*"
			openFileDialog.Filter = "executable files (*.exe)|*.exe|All files (*.*)|*.*";

			DialogResult result = openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileInfo fi = new FileInfo(openFileDialog.FileName);
				this.executableTextBox.Text = Project.GetLongPathName(fi.FullName);
				ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
				if(fi.FullName != td.executablePath)
				{
					td.executablePath = fi.FullName;
					Project.tools.isDirty = true;
				}
			}
		}

		private void upButton_Click(object sender, System.EventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			int index = toolsListBox.SelectedIndex;
			if(index > 0)
			{
				Project.tools.tools.RemoveAt(index);
				Project.tools.tools.Insert(index - 1, td);
				Project.tools.isDirty = true;
				refreshToolsListBox(td);
			}
		}

		private void downButton_Click(object sender, System.EventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			int index = toolsListBox.SelectedIndex;
			if(index < Project.tools.tools.Count - 1)
			{
				Project.tools.tools.RemoveAt(index);
				Project.tools.tools.Insert(index + 1, td);
				Project.tools.isDirty = true;
				refreshToolsListBox(td);
			}
		}

		private void addToolButton_Click(object sender, System.EventArgs e)
		{
			ToolDescr td = new ToolDescr();
			td.displayName = "New Tool";
			td.executablePath = Project.driveSystem + "Program Files";
			td.arguments = "longitude=%lon% latitude=%lat%";
			Project.tools.tools.Add(td);
			Project.tools.isDirty = true;
			refreshToolsListBox(td);
		}

		private void removeToolButton_Click(object sender, System.EventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			Project.tools.tools.Remove(td);
			Project.tools.isDirty = true;
			refreshToolsListBox(null);
		}

		private void runButton_Click(object sender, System.EventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			try 
			{
				td.run(PictureManager.This.CameraManager.Location.Lat, PictureManager.This.CameraManager.Location.Lng, null, null);
			} 
			catch
			{
				LibSys.StatusBar.Trace("* Error: tool not valid: " + td);
			}

		}

		private void toolsListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(toolsListBox.SelectedItem != null)
			{
				ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
				if(td.executablePath == null)
				{
					this.browserCheckBox.Checked = true;
					this.argsLabel.Text = "URL:";
					this.exeLabel.Visible = false;
					this.executableTextBox.Visible = false;
					this.browseButton.Visible = false;
					this.executableTextBox.Text = "";
				}
				else
				{
					this.browserCheckBox.Checked = false;
					this.argsLabel.Text = "Arguments:";
					this.exeLabel.Visible = true;
					this.executableTextBox.Visible = true;
					this.browseButton.Visible = true;
					this.executableTextBox.Text = "" + td.executablePath;
				}
				this.displayNameTextBox.Text = td.displayName;
				this.argumentsTextBox.Text = td.arguments;
				setValidationText(td);
			}
		}

		private void browserCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			if(browserCheckBox.Checked)
			{
				this.argsLabel.Text = "URL:";
				this.exeLabel.Visible = false;
				this.executableTextBox.Visible = false;
				this.browseButton.Visible = false;
				if(td.executablePath != null)
				{
					td.executablePath = null;
					this.executableTextBox.Text = "";
					Project.tools.isDirty = true;
				}
			}
			else
			{
				this.argsLabel.Text = "Arguments:";
				this.exeLabel.Visible = true;
				this.executableTextBox.Visible = true;
				this.browseButton.Visible = true;
				if(td.executablePath == null)
				{
					td.executablePath = Project.driveSystem + "Program Files";
					this.executableTextBox.Text = td.executablePath;
					Project.tools.isDirty = true;
				}
			}
		}

		private void executableTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			string newExe = executableTextBox.Text.Trim();
			if(!newExe.Equals(td.executablePath))
			{
				td.executablePath = newExe;
				Project.tools.isDirty = true;
			}
		}

		private void argumentsTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			string newArgs = argumentsTextBox.Text.Trim();
			if(!newArgs.Equals(td.arguments))
			{
				td.arguments = newArgs;
				Project.tools.isDirty = true;
			}
		}

		private void displayNameTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			collectDisplayName();
		}

		private void collectDisplayName()
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			string newName = displayNameTextBox.Text.Trim();
			if(!newName.Equals(td.displayName))
			{
				td.displayName = newName;
				refreshToolsListBox(td);
				Project.tools.isDirty = true;
			}
		}

		private void displayNameTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyData) 
			{
				case Keys.Enter:
					collectDisplayName();
					break;
			}
		}

		private void argumentsTextBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			ToolDescr td = (ToolDescr)toolsListBox.SelectedItem;
			if(td != null)
			{
				string newArgs = argumentsTextBox.Text.Trim();
				if(!newArgs.Equals(td.arguments))
				{
					td.arguments = newArgs;
					Project.tools.isDirty = true;
					setValidationText(td);
				}
			}
		}

		private void setValidationText(ToolDescr td)
		{
			this.validationLabel.Text = td.substituteArgs(PictureManager.This.CameraManager.Location.Lat, PictureManager.This.CameraManager.Location.Lng, null, null).Replace("&", "&&");
		}

		#endregion
	}
}
