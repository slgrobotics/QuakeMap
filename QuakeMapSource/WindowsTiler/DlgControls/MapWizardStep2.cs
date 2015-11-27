using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;

using LibSys;

namespace WindowsTiler.DlgControls
{
	/// <summary>
	/// Summary description for MapWizardStep2.
	/// </summary>
	public class MapWizardStep2 : UserControl, IWizardStepControl
	{
		private const string headerText = "Select Export Folder";
		private const string infoText = "The Export Folder is a place to temporarily store your tiles before copying them to your memory card."
			+ " Each file contains a mapping \"tile\".\r\n\r\n"
			+ "You can empty the folder before creating a completely new mapping set, or you can freely add new tiles for new zoom centers.\r\n";

		private PdaExportOptions pdaExportOptions = new PdaExportOptions(false);

		private System.Windows.Forms.Label labelHeader;
		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox exportFolderTextBox;
		private System.Windows.Forms.GroupBox exportOptionsGroupBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MapWizardStep2()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			pdaExportOptions.Dock = DockStyle.Fill;
			exportOptionsGroupBox.Controls.Add(pdaExportOptions);

			labelHeader.Text = headerText;
			labelInfo.Text = infoText;
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
			this.labelHeader = new System.Windows.Forms.Label();
			this.labelInfo = new System.Windows.Forms.Label();
			this.browseButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.exportFolderTextBox = new System.Windows.Forms.TextBox();
			this.exportOptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.SuspendLayout();
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.labelHeader.Location = new System.Drawing.Point(70, 5);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Size = new System.Drawing.Size(365, 23);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "step two header here";
			this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelInfo
			// 
			this.labelInfo.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelInfo.Location = new System.Drawing.Point(15, 35);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(475, 65);
			this.labelInfo.TabIndex = 1;
			this.labelInfo.Text = "step two text here";
			// 
			// browseButton
			// 
			this.browseButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.browseButton.Location = new System.Drawing.Point(415, 115);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(70, 20);
			this.browseButton.TabIndex = 21;
			this.browseButton.Text = "Change...";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// label1
			// 
			this.label1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.label1.Location = new System.Drawing.Point(15, 115);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 20);
			this.label1.TabIndex = 22;
			this.label1.Text = "Export Folder:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// exportFolderTextBox
			// 
			this.exportFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.exportFolderTextBox.Location = new System.Drawing.Point(105, 115);
			this.exportFolderTextBox.Name = "exportFolderTextBox";
			this.exportFolderTextBox.ReadOnly = true;
			this.exportFolderTextBox.Size = new System.Drawing.Size(305, 22);
			this.exportFolderTextBox.TabIndex = 23;
			this.exportFolderTextBox.Text = "";
			// 
			// exportOptionsGroupBox
			// 
			this.exportOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.exportOptionsGroupBox.Location = new System.Drawing.Point(15, 145);
			this.exportOptionsGroupBox.Name = "exportOptionsGroupBox";
			this.exportOptionsGroupBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.exportOptionsGroupBox.Size = new System.Drawing.Size(470, 100);
			this.exportOptionsGroupBox.TabIndex = 24;
			this.exportOptionsGroupBox.TabStop = false;
			this.exportOptionsGroupBox.Text = "Format Options";
			// 
			// MapWizardStep2
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.exportOptionsGroupBox,
																		  this.exportFolderTextBox,
																		  this.label1,
																		  this.browseButton,
																		  this.labelInfo,
																		  this.labelHeader});
			this.Name = "MapWizardStep2";
			this.Size = new System.Drawing.Size(500, 250);
			this.ResumeLayout(false);

		}
		#endregion

		// implementing WizardStepControl:
		public void activate(object obj)
		{
			exportFolderTextBox.Text = Project.exportDestFolder;
		}

		public void deactivate(object obj)
		{
			while(!Directory.Exists(Project.exportDestFolder))
			{
				setExportDestFolder();
			}
		}

		public string getNextText() { return "Next >>"; }

		public void callback(int percentComplete, string message) { ; }

		private void setExportDestFolder()
		{
			IntPtr owner = this.Handle;
			clsFolderBrowser folderBrowser = new clsFolderBrowser(owner, "Select Export Folder");

			folderBrowser.Title = "Locate/Create Export Folder";

			string tmp = folderBrowser.BrowseForFolder(Project.exportDestFolder);
			if(!"\\".Equals(tmp))		// cancel button causes "\" returned
			{
				Project.exportDestFolder = tmp;
			}
			exportFolderTextBox.Text = Project.exportDestFolder;
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			setExportDestFolder();
		}
	}
}
