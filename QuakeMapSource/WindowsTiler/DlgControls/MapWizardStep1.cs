using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using LibSys;

namespace WindowsTiler.DlgControls
{
	/// <summary>
	/// Summary description for MapWizardStep1.
	/// </summary>
	public class MapWizardStep1 : UserControl, IWizardStepControl
	{
		private const string headerText = "Making a map to use on PDA";
		private const string infoText = "This wizard will help you create mapping sets that can be copied to a memory card and used on a PDA.\r\n\r\n"
			+ "The map should be positioned over the zoom center(s) where the most detailed views will be available.\r\n\r\n"
			+ "To learn more about creating PDA mapping sets click \"Help\" link below.\r\n";

		private System.Windows.Forms.Label labelHeader;
		private System.Windows.Forms.Label labelInfo;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MapWizardStep1()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			labelHeader.Text = headerText;
			labelInfo.Text = infoText;

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
			this.SuspendLayout();
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.labelHeader.Location = new System.Drawing.Point(70, 45);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Size = new System.Drawing.Size(365, 23);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "step one header here";
			this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelInfo
			// 
			this.labelInfo.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelInfo.Location = new System.Drawing.Point(70, 85);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(365, 155);
			this.labelInfo.TabIndex = 1;
			this.labelInfo.Text = "step one text here";
			// 
			// MapWizardStep1
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.labelInfo,
																		  this.labelHeader});
			this.Name = "MapWizardStep1";
			this.Size = new System.Drawing.Size(500, 250);
			this.ResumeLayout(false);

		}
		#endregion

		// implementing WizardStepControl:
		public void activate(object obj)
		{
		}

		public void deactivate(object obj)
		{
		}

		public string getNextText() { return "Start >>"; }

		public void callback(int percentComplete, string message) { ; }
	}
}
