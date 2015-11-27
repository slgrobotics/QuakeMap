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

using LibGui;
using LibSys;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for OpacityPopup.
	/// </summary>
	public class OpacityPopup : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label opacity3Label;
		private System.Windows.Forms.HScrollBar opacity3ScrollBar;
		private System.Windows.Forms.Label opacity2Label;
		private System.Windows.Forms.HScrollBar opacity2ScrollBar;
		internal System.Windows.Forms.HScrollBar opacityScrollBar;
		private System.Windows.Forms.Label opacityLabel;
		private System.Windows.Forms.CheckBox makeWhitishTransparentCheckBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OpacityPopup()
		{
			InitializeComponent();

			opacityScrollBar.MouseEnter += new System.EventHandler(opacityScrollBar_MouseEnter);
			opacityScrollBar.MouseLeave += new System.EventHandler(opacityScrollBar_MouseLeave);
			opacityScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(opacityScrollBar_Scroll);

			opacity2ScrollBar.MouseEnter += new System.EventHandler(opacityScrollBar_MouseEnter);
			opacity2ScrollBar.MouseLeave += new System.EventHandler(opacityScrollBar_MouseLeave);
			opacity2ScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(opacityScrollBar_Scroll);

			opacity3ScrollBar.MouseEnter += new System.EventHandler(opacityScrollBar_MouseEnter);
			opacity3ScrollBar.MouseLeave += new System.EventHandler(opacityScrollBar_MouseLeave);
			opacity3ScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(opacityScrollBar_Scroll);

			makeWhitishTransparentCheckBox.Checked = Project.makeWhitishTransparent;
			makeWhitishTransparentCheckBox.CheckedChanged += new System.EventHandler(makeWhitishTransparentCheckBox_CheckedChanged);

			setOpacityBarValues();

			Project.setDlgIcon(this);
		}

		private DateTime m_lastRefresh = DateTime.Now;

		public void tick()
		{
			if(groupBox1.Enabled != (Project.drawTerraserver && Project.drawRelief))
			{
				groupBox1.Enabled = Project.drawTerraserver && Project.drawRelief;
			}

			if(groupBox2.Enabled != Project.terraUseOverlay)
			{
				groupBox2.Enabled = Project.terraUseOverlay;
			}

			if(groupBox3.Enabled != (CustomMapsCache.CustomMapsAll.Count > 0))
			{
				groupBox3.Enabled = CustomMapsCache.CustomMapsAll.Count > 0;
			}

			if((DateTime.Now - m_lastRefresh).TotalMilliseconds > 1000)
			{
				this.acceptOpacityIfChanged();
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.opacity3ScrollBar = new System.Windows.Forms.HScrollBar();
			this.opacity3Label = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.opacity2Label = new System.Windows.Forms.Label();
			this.opacity2ScrollBar = new System.Windows.Forms.HScrollBar();
			this.makeWhitishTransparentCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.opacityScrollBar = new System.Windows.Forms.HScrollBar();
			this.opacityLabel = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.groupBox3,
																				 this.groupBox2,
																				 this.groupBox1});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(149, 161);
			this.panel1.TabIndex = 0;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.opacity3ScrollBar,
																					this.opacity3Label});
			this.groupBox3.Location = new System.Drawing.Point(5, 115);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.groupBox3.Size = new System.Drawing.Size(140, 40);
			this.groupBox3.TabIndex = 40;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "GeoTIFF over all";
			// 
			// opacity3ScrollBar
			// 
			this.opacity3ScrollBar.Location = new System.Drawing.Point(10, 18);
			this.opacity3ScrollBar.Name = "opacity3ScrollBar";
			this.opacity3ScrollBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.opacity3ScrollBar.Size = new System.Drawing.Size(66, 15);
			this.opacity3ScrollBar.TabIndex = 35;
			// 
			// opacity3Label
			// 
			this.opacity3Label.Location = new System.Drawing.Point(80, 20);
			this.opacity3Label.Name = "opacity3Label";
			this.opacity3Label.Size = new System.Drawing.Size(51, 15);
			this.opacity3Label.TabIndex = 36;
			this.opacity3Label.Text = "%";
			this.opacity3Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.opacity2Label,
																					this.opacity2ScrollBar,
																					this.makeWhitishTransparentCheckBox});
			this.groupBox2.Location = new System.Drawing.Point(5, 50);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.groupBox2.Size = new System.Drawing.Size(140, 60);
			this.groupBox2.TabIndex = 39;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Topo over Aerial";
			// 
			// opacity2Label
			// 
			this.opacity2Label.Location = new System.Drawing.Point(80, 20);
			this.opacity2Label.Name = "opacity2Label";
			this.opacity2Label.Size = new System.Drawing.Size(51, 15);
			this.opacity2Label.TabIndex = 34;
			this.opacity2Label.Text = "%";
			this.opacity2Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// opacity2ScrollBar
			// 
			this.opacity2ScrollBar.Location = new System.Drawing.Point(10, 18);
			this.opacity2ScrollBar.Name = "opacity2ScrollBar";
			this.opacity2ScrollBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.opacity2ScrollBar.Size = new System.Drawing.Size(66, 15);
			this.opacity2ScrollBar.TabIndex = 33;
			// 
			// makeWhitishTransparentCheckBox
			// 
			this.makeWhitishTransparentCheckBox.Location = new System.Drawing.Point(5, 35);
			this.makeWhitishTransparentCheckBox.Name = "makeWhitishTransparentCheckBox";
			this.makeWhitishTransparentCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.makeWhitishTransparentCheckBox.Size = new System.Drawing.Size(125, 20);
			this.makeWhitishTransparentCheckBox.TabIndex = 37;
			this.makeWhitishTransparentCheckBox.Text = "white as transparent";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.opacityScrollBar,
																					this.opacityLabel});
			this.groupBox1.Location = new System.Drawing.Point(5, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.groupBox1.Size = new System.Drawing.Size(140, 40);
			this.groupBox1.TabIndex = 38;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Aerial over Relief";
			// 
			// opacityScrollBar
			// 
			this.opacityScrollBar.Location = new System.Drawing.Point(10, 18);
			this.opacityScrollBar.Name = "opacityScrollBar";
			this.opacityScrollBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.opacityScrollBar.Size = new System.Drawing.Size(66, 15);
			this.opacityScrollBar.TabIndex = 32;
			// 
			// opacityLabel
			// 
			this.opacityLabel.Location = new System.Drawing.Point(80, 20);
			this.opacityLabel.Name = "opacityLabel";
			this.opacityLabel.Size = new System.Drawing.Size(51, 15);
			this.opacityLabel.TabIndex = 30;
			this.opacityLabel.Text = "%";
			this.opacityLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// OpacityPopup
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(149, 161);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "OpacityPopup";
			this.Opacity = 0.9;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Layers Opacity";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.OpacityPopup_Closing);
			this.panel1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void OpacityPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.acceptOpacityIfChanged();

			e.Cancel = true;
			this.Hide();
		}

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

		private void setOpacityBarValues()
		{
			try
			{
				opacityScrollBar.Value = (int)(Project.terraLayerOpacity * 100.0d);
				opacity2ScrollBar.Value = (int)(Project.terraLayerOpacity2 * 100.0d);
				opacity3ScrollBar.Value = (int)(Project.terraLayerOpacity3 * 100.0d);
			} 
			catch {}

			setOpacityPercents();
		}

		private void setOpacityPercents()
		{
			try
			{
				opacityLabel.Text = "" + (Math.Round(Project.terraLayerOpacity * 100.0d)) + "%";
				opacity2Label.Text = "" + (Math.Round(Project.terraLayerOpacity2 * 100.0d)) + "%";
				opacity3Label.Text = "" + (Math.Round(Project.terraLayerOpacity3 * 100.0d)) + "%";
			} 
			catch {}
		}

		// collect values from the scroll bars and memorize in Project 
		// caution: used from MainFrame when relief layer is activated
		public void setOpacityByScrollBar()
		{
			try
			{
				int sliderPos = opacityScrollBar.Value + 9;	// 10 to 100
				double dSliderPos = sliderPos / 100.0d;

				Project.terraLayerOpacity = Math.Min(1.0d, Math.Max(0.1d, dSliderPos));

				sliderPos = opacity2ScrollBar.Value + 9;	// 10 to 100
				dSliderPos = sliderPos / 100.0d;

				Project.terraLayerOpacity2 = Math.Min(1.0d, Math.Max(0.1d, dSliderPos));

				sliderPos = opacity3ScrollBar.Value + 9;	// 10 to 100
				dSliderPos = sliderPos / 100.0d;

				Project.terraLayerOpacity3 = Math.Min(1.0d, Math.Max(0.25d, dSliderPos));
			} 
			catch {}

			setOpacityPercents();
		}

		// any changes to any scrollbar end up here:
		private void opacityScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			setOpacityByScrollBar();
		}

		// values at the last refresh:
		private int opacityScrollBar_Value = 0;
		private int opacity2ScrollBar_Value = 0;
		private int opacity3ScrollBar_Value = 0;

		private void rememberOpacityValues()
		{
			opacityScrollBar_Value = opacityScrollBar.Value;
			opacity2ScrollBar_Value = opacity2ScrollBar.Value;
			opacity3ScrollBar_Value = opacity3ScrollBar.Value;
		}

		private void opacityScrollBar_MouseEnter(object sender, System.EventArgs e)
		{
			rememberOpacityValues();
		}

		private void opacityScrollBar_MouseLeave(object sender, System.EventArgs e)
		{
			acceptOpacityIfChanged();
		}

		private void acceptOpacityIfChanged()
		{
			if(    opacityScrollBar_Value != opacityScrollBar.Value
				|| opacity2ScrollBar_Value != opacity2ScrollBar.Value
				|| opacity3ScrollBar_Value != opacity3ScrollBar.Value )	// changed since last time?
			{
				PictureManager.This.Refresh();

				rememberOpacityValues();
			}
		}

		private void makeWhitishTransparentCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.makeWhitishTransparent = makeWhitishTransparentCheckBox.Checked;
			PictureManager.This.Refresh();
		}

	}
}
