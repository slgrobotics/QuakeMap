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
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgEarthquakesHistorical.
	/// </summary>
	public class DlgEarthquakesHistorical : System.Windows.Forms.Form
	{
		protected PictureManager m_pictureManager;

		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.LinkLabel cnssLinkLabel;
		private System.Windows.Forms.Button invokeImportButton;
		private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label warningLabel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.LinkLabel sigLinkLabel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.LinkLabel intLinkLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgEarthquakesHistorical(PictureManager pictureManager)
		{
			m_pictureManager = pictureManager;

			InitializeComponent();

			string wrn = 
				  "    This program displays data that you select from publically available databases.\n"
				+ "    Author and distributors of this program do not have any control over data collection,\n"
				+ "    selection or the quality of the data. This program may contain bugs and distort data.\n"
				+ "    Analysing and interpreting this data requires a qualified seismologist.\n"
				+ "    Do not jump to conclusions and make your own judgement about risks and probabilities\n"
				+ "    based on what you see. Ask qualified professional for help.";

			warningLabel.Text = wrn;

			string msg =  "To display historical earthquakes in your area, use the following steps:\n\n"
						+ "    1. visit one of ANSS/NGDC search forms and select time and your area of interest\n\n"
						+ "    2. save result of your search on your hard drive\n\n"
						+ "    3. invoke File Import Wizard to read the file (and optionally make it persistent)";

			messageLabel.Text = msg;

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
            this.messageLabel = new System.Windows.Forms.Label();
            this.cnssLinkLabel = new System.Windows.Forms.LinkLabel();
            this.invokeImportButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.warningLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.sigLinkLabel = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.intLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // messageLabel
            // 
            this.messageLabel.Location = new System.Drawing.Point(22, 167);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(504, 113);
            this.messageLabel.TabIndex = 0;
            this.messageLabel.Text = "msg";
            // 
            // cnssLinkLabel
            // 
            this.cnssLinkLabel.Location = new System.Drawing.Point(175, 318);
            this.cnssLinkLabel.Name = "cnssLinkLabel";
            this.cnssLinkLabel.Size = new System.Drawing.Size(351, 22);
            this.cnssLinkLabel.TabIndex = 1;
            this.cnssLinkLabel.TabStop = true;
            this.cnssLinkLabel.Text = "http://quake.geo.berkeley.edu/anss/catalog-search.html";
            this.cnssLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cnssLinkLabel_LinkClicked);
            // 
            // invokeImportButton
            // 
            this.invokeImportButton.Location = new System.Drawing.Point(175, 432);
            this.invokeImportButton.Name = "invokeImportButton";
            this.invokeImportButton.Size = new System.Drawing.Size(220, 21);
            this.invokeImportButton.TabIndex = 4;
            this.invokeImportButton.Text = "File Import Wizard";
            this.invokeImportButton.Click += new System.EventHandler(this.invokeImportButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(424, 432);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(95, 21);
            this.closeButton.TabIndex = 5;
            this.closeButton.Text = "Close";
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(44, 318);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 22);
            this.label1.TabIndex = 4;
            this.label1.Text = "ANSS Search:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // warningLabel
            // 
            this.warningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningLabel.ForeColor = System.Drawing.Color.Red;
            this.warningLabel.Location = new System.Drawing.Point(22, 53);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(504, 106);
            this.warningLabel.TabIndex = 7;
            this.warningLabel.Text = "warning";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(16, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(504, 22);
            this.label3.TabIndex = 8;
            this.label3.Text = "Warning and Disclaimer of Liability:";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(15, 348);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 31);
            this.label4.TabIndex = 10;
            this.label4.Text = "NGDC Significant Earthquake Database:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // sigLinkLabel
            // 
            this.sigLinkLabel.Location = new System.Drawing.Point(175, 356);
            this.sigLinkLabel.Name = "sigLinkLabel";
            this.sigLinkLabel.Size = new System.Drawing.Size(351, 22);
            this.sigLinkLabel.TabIndex = 2;
            this.sigLinkLabel.TabStop = true;
            this.sigLinkLabel.Text = "http://www.ngdc.noaa.gov/seg/hazard/sig_srch.shtml";
            this.sigLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.sigLinkLabel_LinkClicked);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(15, 386);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(138, 31);
            this.label5.TabIndex = 12;
            this.label5.Text = "NGDC Earthquake Intensity Database :";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // intLinkLabel
            // 
            this.intLinkLabel.Location = new System.Drawing.Point(175, 394);
            this.intLinkLabel.Name = "intLinkLabel";
            this.intLinkLabel.Size = new System.Drawing.Size(351, 22);
            this.intLinkLabel.TabIndex = 3;
            this.intLinkLabel.TabStop = true;
            this.intLinkLabel.Text = "http://www.ngdc.noaa.gov/seg/hazard/int_srch.shtml";
            this.intLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.intLinkLabel_LinkClicked);
            // 
            // DlgEarthquakesHistorical
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(535, 467);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.intLinkLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.sigLinkLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.invokeImportButton);
            this.Controls.Add(this.cnssLinkLabel);
            this.Controls.Add(this.messageLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgEarthquakesHistorical";
            this.Text = "Importing Historical Earthquakes";
            this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void invokeImportButton_Click(object sender, System.EventArgs e)
		{
			FileImportForm fileImportForm = new FileImportForm(false, FileImportForm.MODE_EQ, FileImportForm.TYPE_ANY);
			fileImportForm.ShowDialog();
		}

		private void cnssLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(cnssLinkLabel.Text);
		}

		private void sigLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(sigLinkLabel.Text);
		}

		private void intLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(intLinkLabel.Text);
		}
	}
}
