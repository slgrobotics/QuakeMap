using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using LibGui;
using LibSys;

namespace WindowsTiler.DlgControls
{
	/// <summary>
	/// PdaExportOptions has all controls for defining the export options.
	/// </summary>
	public class PdaExportOptions : System.Windows.Forms.UserControl
	{
		public event EventHandler  OptionsChanged;

		private System.Windows.Forms.CheckBox compressGreyscaleCheckBox;
		private System.Windows.Forms.RadioButton radioButtonBmpOptimized;
		private System.Windows.Forms.RadioButton radioButtonBmpWebsafe;
		private System.Windows.Forms.RadioButton radioButtonJpeg;
		private System.Windows.Forms.CheckBox wrapPdbCheckBox;
		private System.Windows.Forms.LinkLabel helpLinkLabel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PdaExportOptions(bool showWrapPdbCheckBox)
		{
			InitializeComponent();

			wrapPdbCheckBox.Visible	 = showWrapPdbCheckBox;
			setRadioButtons();
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

		private void collectRadioButtons()
		{
			if(radioButtonBmpWebsafe.Checked)
			{
				Project.pdaExportImageFormat = 0;
				this.compressGreyscaleCheckBox.Enabled = true;
			}
			else if(radioButtonBmpOptimized.Checked)
			{
				Project.pdaExportImageFormat = 1;
				this.compressGreyscaleCheckBox.Enabled = true;
			}
			else if(radioButtonJpeg.Checked)
			{
				Project.pdaExportImageFormat = 2;
				this.compressGreyscaleCheckBox.Enabled = false;
			}
			Project.pdaExportUseWebsafeGrayscalePalette = !this.compressGreyscaleCheckBox.Checked;
			Project.pdaExportWrapPdb = this.wrapPdbCheckBox.Checked;
		}

		public void setRadioButtons()
		{
			this.compressGreyscaleCheckBox.Checked = !Project.pdaExportUseWebsafeGrayscalePalette;
			this.wrapPdbCheckBox.Checked = Project.pdaExportWrapPdb;
			switch(Project.pdaExportImageFormat)
			{
				case 0:
					radioButtonBmpWebsafe.Checked = true;
					this.compressGreyscaleCheckBox.Enabled = true;
					break;
				case 1:
					radioButtonBmpOptimized.Checked = true;
					this.compressGreyscaleCheckBox.Enabled = true;
					break;
				case 2:
					radioButtonJpeg.Checked = true;
					this.compressGreyscaleCheckBox.Enabled = false;
					break;
			}
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.compressGreyscaleCheckBox = new System.Windows.Forms.CheckBox();
			this.radioButtonBmpOptimized = new System.Windows.Forms.RadioButton();
			this.radioButtonBmpWebsafe = new System.Windows.Forms.RadioButton();
			this.radioButtonJpeg = new System.Windows.Forms.RadioButton();
			this.wrapPdbCheckBox = new System.Windows.Forms.CheckBox();
			this.helpLinkLabel = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// compressGreyscaleCheckBox
			// 
			this.compressGreyscaleCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.compressGreyscaleCheckBox.Location = new System.Drawing.Point(180, 5);
			this.compressGreyscaleCheckBox.Name = "compressGreyscaleCheckBox";
			this.compressGreyscaleCheckBox.Size = new System.Drawing.Size(185, 24);
			this.compressGreyscaleCheckBox.TabIndex = 13;
			this.compressGreyscaleCheckBox.Text = "compress grayscale aerial";
			this.compressGreyscaleCheckBox.CheckedChanged += new System.EventHandler(this.compressGreyscaleCheckBox_CheckedChanged);
			// 
			// radioButtonBmpOptimized
			// 
			this.radioButtonBmpOptimized.Location = new System.Drawing.Point(5, 55);
			this.radioButtonBmpOptimized.Name = "radioButtonBmpOptimized";
			this.radioButtonBmpOptimized.Size = new System.Drawing.Size(170, 20);
			this.radioButtonBmpOptimized.TabIndex = 12;
			this.radioButtonBmpOptimized.Text = "PocketPC colors";
			this.radioButtonBmpOptimized.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// radioButtonBmpWebsafe
			// 
			this.radioButtonBmpWebsafe.Location = new System.Drawing.Point(5, 30);
			this.radioButtonBmpWebsafe.Name = "radioButtonBmpWebsafe";
			this.radioButtonBmpWebsafe.Size = new System.Drawing.Size(170, 20);
			this.radioButtonBmpWebsafe.TabIndex = 11;
			this.radioButtonBmpWebsafe.Text = "Palm colors";
			this.radioButtonBmpWebsafe.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// radioButtonJpeg
			// 
			this.radioButtonJpeg.Location = new System.Drawing.Point(5, 5);
			this.radioButtonJpeg.Name = "radioButtonJpeg";
			this.radioButtonJpeg.Size = new System.Drawing.Size(170, 20);
			this.radioButtonJpeg.TabIndex = 10;
			this.radioButtonJpeg.Text = "original colors";
			this.radioButtonJpeg.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// wrapPdbCheckBox
			// 
			this.wrapPdbCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.wrapPdbCheckBox.Location = new System.Drawing.Point(180, 35);
			this.wrapPdbCheckBox.Name = "wrapPdbCheckBox";
			this.wrapPdbCheckBox.Size = new System.Drawing.Size(185, 24);
			this.wrapPdbCheckBox.TabIndex = 0;
			this.wrapPdbCheckBox.Text = "wrap in PDB envelope";
			this.wrapPdbCheckBox.CheckedChanged += new System.EventHandler(this.wrapPdbCheckBox_CheckedChanged);
			// 
			// helpLinkLabel
			// 
			this.helpLinkLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.helpLinkLabel.Location = new System.Drawing.Point(225, 65);
			this.helpLinkLabel.Name = "helpLinkLabel";
			this.helpLinkLabel.Size = new System.Drawing.Size(130, 16);
			this.helpLinkLabel.TabIndex = 15;
			this.helpLinkLabel.TabStop = true;
			this.helpLinkLabel.Text = "Format Help";
			this.helpLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.helpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpLinkLabel_LinkClicked);
			// 
			// PdaExportOptions
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.helpLinkLabel,
																		  this.wrapPdbCheckBox,
																		  this.radioButtonJpeg,
																		  this.radioButtonBmpOptimized,
																		  this.radioButtonBmpWebsafe,
																		  this.compressGreyscaleCheckBox});
			this.Name = "PdaExportOptions";
			this.Size = new System.Drawing.Size(365, 85);
			this.ResumeLayout(false);

		}
		#endregion

		private void wrapPdbCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.pdaExportWrapPdb = this.wrapPdbCheckBox.Checked;
		}

		private void compressGreyscaleCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.pdaExportUseWebsafeGrayscalePalette = !this.compressGreyscaleCheckBox.Checked;
			if(OptionsChanged != null)
			{
				OptionsChanged(this, null);
			}
		}

		private void radioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			collectRadioButtons();
			if(OptionsChanged != null)
			{
				OptionsChanged(this, null);
			}
		}

		private void helpLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser("http://www.mapadvisor.com/help/qmexportpdbformats.html");
		}
	}
}
