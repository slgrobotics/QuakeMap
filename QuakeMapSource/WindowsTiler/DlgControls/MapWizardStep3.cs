using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using LibSys;
using LibGui;

namespace WindowsTiler.DlgControls
{
	/// <summary>
	/// Summary description for MapWizardStep2.
	/// </summary>
	public class MapWizardStep3 : UserControl, IWizardStepControl
	{
		private const string hintText = "Drag the mouse, click into rectangle to select zoom centers";

		private bool m_inSet = false;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.CheckBox aerialCheckBox;
		private System.Windows.Forms.CheckBox topoCheckBox;
		private System.Windows.Forms.CheckBox colorCheckBox;
		private System.Windows.Forms.Label label1;
		internal System.Windows.Forms.Panel measurePanel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MapWizardStep3()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			infoLabel.Text = hintText;

			if(!Project.pdaExportDoAerial && !Project.pdaExportDoTopo && !Project.pdaExportDoColor)
			{
				Project.pdaExportDoAerial = true;
			}

			m_inSet = true;
			aerialCheckBox.Checked = Project.pdaExportDoAerial;
			topoCheckBox.Checked = Project.pdaExportDoTopo;
			colorCheckBox.Checked = Project.pdaExportDoColor;
			m_inSet = false;
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
			this.infoLabel = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.aerialCheckBox = new System.Windows.Forms.CheckBox();
			this.topoCheckBox = new System.Windows.Forms.CheckBox();
			this.colorCheckBox = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.measurePanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// infoLabel
			// 
			this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.infoLabel.Location = new System.Drawing.Point(20, 50);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(460, 30);
			this.infoLabel.TabIndex = 2;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.Location = new System.Drawing.Point(15, 30);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(465, 15);
			this.progressBar.Step = 1;
			this.progressBar.TabIndex = 3;
			// 
			// aerialCheckBox
			// 
			this.aerialCheckBox.Location = new System.Drawing.Point(205, 5);
			this.aerialCheckBox.Name = "aerialCheckBox";
			this.aerialCheckBox.Size = new System.Drawing.Size(85, 20);
			this.aerialCheckBox.TabIndex = 4;
			this.aerialCheckBox.Text = "aerial";
			this.aerialCheckBox.CheckedChanged += new System.EventHandler(this.anyCheckBox_CheckedChanged);
			// 
			// topoCheckBox
			// 
			this.topoCheckBox.Location = new System.Drawing.Point(300, 5);
			this.topoCheckBox.Name = "topoCheckBox";
			this.topoCheckBox.Size = new System.Drawing.Size(85, 20);
			this.topoCheckBox.TabIndex = 5;
			this.topoCheckBox.Text = "topo";
			this.topoCheckBox.CheckedChanged += new System.EventHandler(this.anyCheckBox_CheckedChanged);
			// 
			// colorCheckBox
			// 
			this.colorCheckBox.Location = new System.Drawing.Point(395, 5);
			this.colorCheckBox.Name = "colorCheckBox";
			this.colorCheckBox.Size = new System.Drawing.Size(85, 20);
			this.colorCheckBox.TabIndex = 6;
			this.colorCheckBox.Text = "color";
			this.colorCheckBox.CheckedChanged += new System.EventHandler(this.anyCheckBox_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(20, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(170, 20);
			this.label1.TabIndex = 7;
			this.label1.Text = "Select map type(s):";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// measurePanel
			// 
			this.measurePanel.Name = "measurePanel";
			this.measurePanel.Size = new System.Drawing.Size(10, 85);
			this.measurePanel.TabIndex = 8;
			// 
			// MapWizardStep3
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.measurePanel,
																		  this.label1,
																		  this.colorCheckBox,
																		  this.topoCheckBox,
																		  this.aerialCheckBox,
																		  this.progressBar,
																		  this.infoLabel});
			this.Name = "MapWizardStep3";
			this.Size = new System.Drawing.Size(500, 250);
			this.ResumeLayout(false);

		}
		#endregion

		private IEnableDisable iCaller = null;

		// implementing WizardStepControl:
		public void activate(object obj)
		{
			Project.pdaWellSelectingMode = true;
			Project.pdaWizardStepControl = this;
			iCaller = (IEnableDisable)obj;
			iCaller.enable(3);			// resize the dialog
			iCaller.disable(2);			// initially disable the Next button, with the hint pop-up
			if(TerraserverCache.TileNamesCollection != null)
			{
				TerraserverCache.TileNamesCollection.Clear();
			}
			else
			{
				TerraserverCache.TileNamesCollection = new Hashtable();
			}
		}

		public void deactivate(object obj)
		{
			iCaller.disable(3);			// resize the dialog back
			Project.pdaWizardStepControl = null;
			Project.pdaWellSelectingMode = false;
		}

		public string getNextText() { return "Next >>"; }

		public void callback(int percentComplete, string message)
		{ 
			infoLabel.Text = message;
			progressBar.Value = Math.Min(Math.Max(percentComplete, 0), 100);
			if(percentComplete == 0)
			{
				iCaller.enable(0);		// OK, we finished doing something and now the Next button should be enabled. 
			}
			else
			{
				iCaller.disable(0);		// in process. 
			}
		}

		private void anyCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!m_inSet)
			{
				if(!aerialCheckBox.Checked && !topoCheckBox.Checked && !colorCheckBox.Checked)
				{
					Project.ShowPopup(colorCheckBox, "one of the checkboxes must be checked", Point.Empty);
					aerialCheckBox.Checked = Project.pdaExportDoAerial;
					topoCheckBox.Checked = Project.pdaExportDoTopo;
					colorCheckBox.Checked = Project.pdaExportDoColor;
				}
				else
				{
					Project.pdaExportDoAerial = aerialCheckBox.Checked;
					Project.pdaExportDoTopo = topoCheckBox.Checked;
					Project.pdaExportDoColor = colorCheckBox.Checked;
				}
			}
		}
	}
}
