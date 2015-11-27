using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using System.IO;

using LibGui;
using LibSys;

namespace WindowsTiler.DlgControls
{
	/// <summary>
	/// Summary description for MapWizardStep2.
	/// </summary>
	public class MapWizardStep4 : UserControl, IWizardStepControl
	{
		private Form m_parent;

		private const string headerText0 = "Please wait, verifying downloaded tiles...";
		private const string headerText1 = "Please wait, copying to Export Folder...";
		private const string headerText2 = "Finished";
		private const string verifyText = "Mapping set info: {0} tiles ready, {1} to download ({2}%). Fixed: {3} of {1}.\r\n\r\n";
		private const string verifyErrorText = "Fixed only {0} of {1} missing tiles, couldn't download {2} tiles needed for the map.\r\nWant to try again?";
		private const string verifyErrorText0 = "Couldn't download {0} tiles needed for the map. Want to try again?";
		private const string infoText = "Mapping set is ready: {0} tiles in the folder - {1} errors.\r\n\r\n"
			+ "Use Windows Explorer or any other means to copy all files to your Memory Card.\r\n";
		private const string verifyTextEmpty = "Error: Nothing to copy to Export Folder.";

		private System.Windows.Forms.Label labelHeader;
		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.TextBox exportFolderTextBox;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label exportFolderLabel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MapWizardStep4(Form parent)
		{
			m_parent = parent;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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
			this.exportFolderTextBox = new System.Windows.Forms.TextBox();
			this.exportFolderLabel = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.labelHeader.Location = new System.Drawing.Point(20, 45);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Size = new System.Drawing.Size(460, 23);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "step four header here";
			this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelInfo
			// 
			this.labelInfo.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelInfo.Location = new System.Drawing.Point(20, 85);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(460, 65);
			this.labelInfo.TabIndex = 1;
			this.labelInfo.Text = "step four text here";
			// 
			// exportFolderTextBox
			// 
			this.exportFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.exportFolderTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.exportFolderTextBox.Location = new System.Drawing.Point(30, 210);
			this.exportFolderTextBox.Name = "exportFolderTextBox";
			this.exportFolderTextBox.ReadOnly = true;
			this.exportFolderTextBox.Size = new System.Drawing.Size(450, 13);
			this.exportFolderTextBox.TabIndex = 25;
			this.exportFolderTextBox.Text = "";
			// 
			// exportFolderLabel
			// 
			this.exportFolderLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.exportFolderLabel.Location = new System.Drawing.Point(20, 185);
			this.exportFolderLabel.Name = "exportFolderLabel";
			this.exportFolderLabel.Size = new System.Drawing.Size(220, 20);
			this.exportFolderLabel.TabIndex = 24;
			this.exportFolderLabel.Text = "Export Folder location:";
			this.exportFolderLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.Location = new System.Drawing.Point(15, 155);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(465, 15);
			this.progressBar.Step = 1;
			this.progressBar.TabIndex = 26;
			// 
			// MapWizardStep4
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.progressBar,
																		  this.exportFolderTextBox,
																		  this.exportFolderLabel,
																		  this.labelInfo,
																		  this.labelHeader});
			this.Name = "MapWizardStep4";
			this.Size = new System.Drawing.Size(500, 250);
			this.ResumeLayout(false);

		}
		#endregion

		// implementing WizardStepControl:
		public void activate(object obj)
		{
			PictureManager.This.CameraManager.resetDrag(true);
			exportFolderTextBox.Text = Project.exportDestFolder;
			labelHeader.Text = headerText0;

			IEnableDisable iCaller = (IEnableDisable)obj;
			iCaller.disable(1);			// make the Next button invisible, and "Close" appear on cancel button

			Application.DoEvents();

			if(verifyPdaExportSet())
			{
				labelHeader.Text = headerText1;
				Application.DoEvents();

				if(m_pdaCopyThread == null)
				{
					m_pdaCopyThread = new Thread( new ThreadStart(monitorPdaExportProgress));
					// see Entry.cs for how the current culture is set:
					m_pdaCopyThread.CurrentCulture = Thread.CurrentThread.CurrentCulture; //new CultureInfo("en-US", false);
					m_pdaCopyThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture; //new CultureInfo("en-US", false);
					m_pdaCopyThread.IsBackground = true;	// terminate with the main process
					m_pdaCopyThread.Name = "Copying tiles for PDA";
					m_pdaCopyThread.Start();
				}
			}
			else
			{
				labelHeader.Text = verifyTextEmpty;
				labelInfo.Text = "The map set you created appears to be empty.\r\nPlease repeat, selecting zoom area(s) as desired.";
				progressBar.Visible = false;
				exportFolderLabel.Visible = false;
				exportFolderTextBox.Visible = false;
			}
		}

		private bool verifyPdaExportSet()
		{
			int tileCount = 0;
			int toFixCount = 0;
			int fixedCount = 0;
			int toExport = TerraserverCache.TileNamesCollection.Count;
			int percentToFix = 0;
			ArrayList ToFix = new ArrayList();

			if(toExport == 0)
			{
				return false;
			}

			/*
			// for debugging - delete some tiles to test fixing logic:
			int delCnt = 100;
			foreach(string tileName in TerraserverCache.TileNamesCollection.Keys)
			{
				string srcPath = TerraserverCache.tileImagePath(tileName);

				if(File.Exists(srcPath))
				{
					try
					{
						File.Delete(srcPath);
					}
					catch {}
				}
				if(delCnt-- <= 0)
				{
					break;
				}
			}
			// end for debugging
			*/

			foreach(string tileName in TerraserverCache.TileNamesCollection.Keys)
			{
				string srcPath = TerraserverCache.tileImagePath(tileName);

				if(File.Exists(srcPath))
				{
					tileCount++;
				}
				else
				{
					toFixCount++;
					LibSys.StatusBar.Error("file: " + srcPath + "  -- was not downloaded correctly, fixing...");
					ToFix.Add(srcPath);
					tryFixTile(tileName);
				}
			}
			percentToFix = toExport == 0 ? 0 : (toFixCount * 100 / toExport);
			labelInfo.Text = String.Format(verifyText, tileCount, toFixCount, percentToFix, fixedCount);

		fixagain:
			DateTime startedFixing = DateTime.Now;
			while(ToFix.Count > 0 && DateTime.Now < startedFixing.AddSeconds(20))
			{
			  again:
				foreach(string srcPath in ToFix)
				{
					if(File.Exists(srcPath))
					{
						LibSys.StatusBar.Trace("OK: file " + srcPath + "  -- was fixed.");
						ToFix.Remove(srcPath);
						fixedCount++;
						goto again;
					}
				}
				Application.DoEvents();
				labelInfo.Text = String.Format(verifyText, tileCount, toFixCount, percentToFix, fixedCount);
				Thread.Sleep(1000);
			}

			if(ToFix.Count > 0)
			{
				if(Project.YesNoBox(m_parent,
					fixedCount == 0 ? String.Format(verifyErrorText0, ToFix.Count)
					: String.Format(verifyErrorText, fixedCount, toFixCount, ToFix.Count))
				  )
				{
					goto fixagain;
				}
			}

			labelInfo.Text = String.Format(verifyText, tileCount, toFixCount, percentToFix, fixedCount);

			return (toExport - ToFix.Count + fixedCount) > 0;
		}

		private void tryFixTile(string baseName)
		{
			TerraserverCache.resetBackdrop(null, baseName);
		}

		public void deactivate(object obj)
		{
		}

		public string getNextText() { return "Finished"; }

		public void callback(int percentComplete, string message) { ; }

		private static Thread m_pdaCopyThread = null;

		private void monitorPdaExportProgress()
		{
			int errorCount = 0;
			int tileCount = 0;
			int toExport = TerraserverCache.TileNamesCollection.Count;
			int percentComplete = 0;
			int percentCompletePrev = 0;
			foreach(string tileName in TerraserverCache.TileNamesCollection.Keys)
			{
				string srcPath = TerraserverCache.tileImagePath(tileName);

				if(File.Exists(srcPath))
				{
					try
					{
						PdaHelper.exportSingleFile(tileName, srcPath, true);
						if((tileCount % 10) == 0)
						{
							labelInfo.Text = "" + tileCount + " of " + toExport + "      " + tileName;
						}
						tileCount++;
						percentComplete = toExport == 0 ? 0 : (tileCount * 100 / toExport);
						if(percentComplete != percentCompletePrev)
						{
							percentCompletePrev = percentComplete;
							progressBar.Value = percentComplete;
						}
					}
					catch (Exception exc)
					{
						errorCount++;
						LibSys.StatusBar.Error("exporting PDA file: " + srcPath + "  -- " + exc.Message);
					}
				}
				else
				{
					errorCount++;
					LibSys.StatusBar.Error("exporting PDA file: " + srcPath + "  -- file does not exist");
				}
			}
			TerraserverCache.TileNamesCollection.Clear();
			TerraserverCache.TileNamesCollection = null;
			labelHeader.Text = headerText2;
			labelInfo.Text = String.Format(infoText, tileCount, errorCount);
			progressBar.Visible = false;

			m_pdaCopyThread = null;
			// and exit, terminating the thread.
		}
	}
}
