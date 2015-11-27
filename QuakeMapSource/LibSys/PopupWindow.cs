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

namespace LibSys
{
	/// <summary>
	/// PopupWindow is used by Project.ShowPopup()
	/// </summary>
	public class PopupWindow : System.Windows.Forms.Form
	{
		private string m_message;
		private const int SHIFT_HORIZ = 8;		// topleft of popup from the Point location argument, sits on bottom right of the arrow cursor
		private const int SHIFT_VERT  = -12; 

		private System.Windows.Forms.Label messageLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PopupWindow(Control parent, string caption, Point location, bool mayBeToLeft)
		{
			m_message = caption;

			InitializeComponent();

			SizeF sizef = messageLabel.CreateGraphics().MeasureString(m_message, messageLabel.Font);
			int labelWidth = (int)(sizef.Width * 1.05f);	// need just a little bit to make sure it stays single line 
			int labelHeight = (int)(sizef.Height);

			Rectangle bounds = new Rectangle(location.X + SHIFT_HORIZ, location.Y - SHIFT_VERT, labelWidth, labelHeight);
			if(mayBeToLeft && parent != null)
			{
				try
				{
					Rectangle parentBounds = new Rectangle(parent.PointToScreen(new Point(0, 0)), parent.Size);
					//m_message = "" + parentBounds;
					if(!parentBounds.Contains(bounds))
					{
						bounds = new Rectangle(location.X - labelWidth - SHIFT_HORIZ, location.Y - SHIFT_VERT, labelWidth, labelHeight);
					}
				}
				catch {}
			}
			this.Bounds = bounds;

			messageLabel.Text = m_message;
			this.Focus();	// this normally won't work as Project.ShowPopup tries to return focus to parent. Hover mouse to regain focus
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
			this.SuspendLayout();
			// 
			// messageLabel
			// 
			this.messageLabel.BackColor = System.Drawing.Color.Khaki;
			this.messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.messageLabel.Location = new System.Drawing.Point(1, 1);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(413, 223);
			this.messageLabel.TabIndex = 0;
			this.messageLabel.Text = "msg";
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.messageLabel.MouseHover += new System.EventHandler(this.messageLabel_MouseHover);
			this.messageLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.messageLabel_MouseMove);
			this.messageLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.messageLabel_MouseDown);
			// 
			// PopupWindow
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.ControlText;
			this.ClientSize = new System.Drawing.Size(415, 225);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.messageLabel});
			this.DockPadding.All = 1;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "PopupWindow";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "PopupWindow";
			this.TopMost = true;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PopupWindow_KeyDown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.messageLabel_MouseDown);
			this.ResumeLayout(false);

		}
		#endregion


		protected override bool ProcessDialogKey(Keys keyData)
		{
			// this normally won't work as Project.ShowPopup tries to return focus to parent. Hover mouse to regain focus
			
			//LibSys.StatusBar.Trace("ProcessDialogKey: " + keyData);

			bool keystrokeProcessed = true;
			switch (keyData) 
			{
				case Keys.Escape:
				case Keys.Alt | Keys.F4:
					Project.ClearPopup();
					break;
				default:
					// if you want popup to stay, move mouse or press keys
					makeStay();
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

		private void messageLabel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.Hide();
			Project.ClearPopup();
		}

		private void messageLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// if you want popup to stay, hover mouse
			if(!this.messageLabel.Text.EndsWith("X close"))
			{
				Project.currentPopupCreated = DateTime.Now;
			}
		}

		private void PopupWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			this.Hide();
			Project.ClearPopup();
		}

		private void messageLabel_MouseHover(object sender, System.EventArgs e)
		{
			makeStay();
		}

		private void makeStay()
		{
			Project.currentPopupCreated = DateTime.Now.AddSeconds(120);
			if(!this.messageLabel.Text.EndsWith("X close"))
			{
				this.SuspendLayout();
				this.messageLabel.BackColor = System.Drawing.Color.AntiqueWhite;
				this.messageLabel.Text += "\n\nX close";
				this.Height += 30;
				this.ResumeLayout(true);
				this.Focus();
			}
		}
	}
}
