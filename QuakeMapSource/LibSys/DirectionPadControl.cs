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

namespace LibSys
{
	public class DirectionPadEventArgs : EventArgs 
	{   
		public readonly int btn = -1;

		// Constructor.
		public DirectionPadEventArgs(int _btn) { btn = _btn; }		// look at the telephone pad to see how numbers are placed on the pad
	}

	public delegate void ClickEventHandler(object sender, DirectionPadEventArgs e);

	/// <summary>
	/// Summary description for DirectionPadControl.
	/// </summary>
	public class DirectionPadControl : System.Windows.Forms.UserControl
	{
		public event ClickEventHandler clickHandler;

		public System.Windows.Forms.Button moveUpRightButton;
		public System.Windows.Forms.Button moveDownRightButton;
		public System.Windows.Forms.Button moveDownLeftButton;
		public System.Windows.Forms.Button moveUpLeftButton;
		public System.Windows.Forms.Button moveRightButton;
		public System.Windows.Forms.Button moveLeftButton;
		public System.Windows.Forms.Button moveDownButton;
		public System.Windows.Forms.Button moveUpButton;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DirectionPadControl()
		{
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
			this.moveUpRightButton = new System.Windows.Forms.Button();
			this.moveDownRightButton = new System.Windows.Forms.Button();
			this.moveDownLeftButton = new System.Windows.Forms.Button();
			this.moveUpLeftButton = new System.Windows.Forms.Button();
			this.moveRightButton = new System.Windows.Forms.Button();
			this.moveLeftButton = new System.Windows.Forms.Button();
			this.moveDownButton = new System.Windows.Forms.Button();
			this.moveUpButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// moveUpRightButton
			// 
			this.moveUpRightButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveUpRightButton.Location = new System.Drawing.Point(34, 1);
			this.moveUpRightButton.Name = "moveUpRightButton";
			this.moveUpRightButton.Size = new System.Drawing.Size(15, 14);
			this.moveUpRightButton.TabIndex = 32;
			this.moveUpRightButton.TabStop = false;
			this.moveUpRightButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// moveDownRightButton
			// 
			this.moveDownRightButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveDownRightButton.Location = new System.Drawing.Point(34, 37);
			this.moveDownRightButton.Name = "moveDownRightButton";
			this.moveDownRightButton.Size = new System.Drawing.Size(15, 14);
			this.moveDownRightButton.TabIndex = 31;
			this.moveDownRightButton.TabStop = false;
			this.moveDownRightButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// moveDownLeftButton
			// 
			this.moveDownLeftButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveDownLeftButton.Location = new System.Drawing.Point(1, 37);
			this.moveDownLeftButton.Name = "moveDownLeftButton";
			this.moveDownLeftButton.Size = new System.Drawing.Size(16, 14);
			this.moveDownLeftButton.TabIndex = 30;
			this.moveDownLeftButton.TabStop = false;
			this.moveDownLeftButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// moveUpLeftButton
			// 
			this.moveUpLeftButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveUpLeftButton.Location = new System.Drawing.Point(1, 1);
			this.moveUpLeftButton.Name = "moveUpLeftButton";
			this.moveUpLeftButton.Size = new System.Drawing.Size(16, 14);
			this.moveUpLeftButton.TabIndex = 29;
			this.moveUpLeftButton.TabStop = false;
			this.moveUpLeftButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// moveRightButton
			// 
			this.moveRightButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveRightButton.Location = new System.Drawing.Point(34, 16);
			this.moveRightButton.Name = "moveRightButton";
			this.moveRightButton.Size = new System.Drawing.Size(15, 20);
			this.moveRightButton.TabIndex = 28;
			this.moveRightButton.TabStop = false;
			this.moveRightButton.Text = "4";
			this.moveRightButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// moveLeftButton
			// 
			this.moveLeftButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveLeftButton.Location = new System.Drawing.Point(1, 16);
			this.moveLeftButton.Name = "moveLeftButton";
			this.moveLeftButton.Size = new System.Drawing.Size(15, 20);
			this.moveLeftButton.TabIndex = 27;
			this.moveLeftButton.TabStop = false;
			this.moveLeftButton.Text = "3";
			this.moveLeftButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// moveDownButton
			// 
			this.moveDownButton.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveDownButton.Location = new System.Drawing.Point(17, 32);
			this.moveDownButton.Name = "moveDownButton";
			this.moveDownButton.Size = new System.Drawing.Size(17, 19);
			this.moveDownButton.TabIndex = 26;
			this.moveDownButton.TabStop = false;
			this.moveDownButton.Text = "6";
			this.moveDownButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// moveUpButton
			// 
			this.moveUpButton.Font = new System.Drawing.Font("Webdings", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
			this.moveUpButton.Location = new System.Drawing.Point(17, 1);
			this.moveUpButton.Name = "moveUpButton";
			this.moveUpButton.Size = new System.Drawing.Size(17, 19);
			this.moveUpButton.TabIndex = 25;
			this.moveUpButton.TabStop = false;
			this.moveUpButton.Text = "5";
			this.moveUpButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// DirectionPadControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.moveUpRightButton,
																		  this.moveDownRightButton,
																		  this.moveDownLeftButton,
																		  this.moveUpLeftButton,
																		  this.moveRightButton,
																		  this.moveLeftButton,
																		  this.moveDownButton,
																		  this.moveUpButton});
			this.Name = "DirectionPadControl";
			this.Size = new System.Drawing.Size(51, 52);
			this.ResumeLayout(false);

		}
		#endregion

		private void moveButton_Click(object sender, System.EventArgs e)
		{
			if (clickHandler != null)
			{
				if(sender == this.moveUpLeftButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(1);
					clickHandler(this, ee);
				}
				else if(sender == this.moveUpButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(2);
					clickHandler(this, ee);
				}
				else if(sender == this.moveUpRightButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(3);
					clickHandler(this, ee);
				}
				else if(sender == this.moveLeftButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(4);
					clickHandler(this, ee);
				}
				else if(sender == this.moveRightButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(6);
					clickHandler(this, ee);
				}
				else if(sender == this.moveDownLeftButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(7);
					clickHandler(this, ee);
				}
				else if(sender == this.moveDownButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(8);
					clickHandler(this, ee);
				}
				else if(sender == this.moveDownRightButton)
				{
					DirectionPadEventArgs ee = new DirectionPadEventArgs(9);
					clickHandler(this, ee);
				}
			}
		}
	}
}
