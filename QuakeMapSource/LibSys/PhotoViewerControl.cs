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
	/// <summary>
	/// Summary description for PhotoViewerControl.
	/// </summary>
	public class PhotoViewerControl : System.Windows.Forms.UserControl
	{
		private Image m_image = null;
		public Image image { get { return m_image; } }
		private int m_imageWidth = 0;
		private int m_imageHeight = 0;
		private bool hasLoaded = false;

		private PhotoDescr m_photoDescr = null;
		public PhotoDescr photoDescr {
			get { return m_photoDescr; }
			set {
				if(m_photoDescr != null)
				{
					m_photoDescr.releaseImage();
				}
				m_photoDescr = value;
				if(m_photoDescr != null)
				{
					m_photoDescr.ensureImageExists();
					// rotate image based on photoDescr.Orientation.
					switch(photoDescr.Orientation)
					{
							// see PhotoDescr.cs for EXIF orientation codes:
						case 4:
						{
							Bitmap bmp = new Bitmap(m_photoDescr.image);
							bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
							m_image = bmp;
						}
							break;
						case 6:
						{
							Bitmap bmp = new Bitmap(m_photoDescr.image);
							bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
							m_image = bmp;
						}
							break;
						case 8:
						{
							Bitmap bmp = new Bitmap(m_photoDescr.image);
							bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
							m_image = bmp;
						}
							break;
						default:
							m_image = m_photoDescr.image;
							break;
					}
					m_imageWidth = m_image.Width;
					m_imageHeight = m_image.Height;

					pictureBox.Image = m_image;
					if(!fitToSize)
					{
						// a workaround to make control refresh completely
						fitToSize = true;
						fitToSize = false;
					}
					else
					{
						sizeThePictureBox();
					}
				}
				else
				{
					pictureBox.Image = null;
				}
			}
		}

		private bool m_fitToSize = true;
		public bool fitToSize {
			get { return m_fitToSize; }
			set
			{
				m_fitToSize = value;
				this.sizeThePictureBox();
			}
		} 

		public System.Windows.Forms.PictureBox pictureBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PhotoViewerControl()
		{
			InitializeComponent();
		}

		public void sizeThePictureBox()
		{
			if(m_image != null && hasLoaded)
			{
				if(fitToSize)
				{
					this.AutoScroll = false;	// before we mess with layout
					this.SuspendLayout();
					this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;

					float imageRatio = (float)m_imageWidth / (float)m_imageHeight;
					float panelRatio = (float)this.Width / (float)this.Height;

					Size clientSize = new Size();
					Point loc = new Point();
					if(imageRatio > panelRatio)	// image wider
					{
						clientSize.Width = this.Width;
						clientSize.Height = (int)(clientSize.Width / imageRatio);
						loc.Y = (this.Height - clientSize.Height) / 2;
					}
					else
					{
						clientSize.Height = this.Height;
						clientSize.Width = (int)(clientSize.Height * imageRatio);
						loc.X = (this.Width - clientSize.Width) / 2;
					}
					this.pictureBox.ClientSize = clientSize;
					this.pictureBox.Location = loc;
					this.ResumeLayout(false);
				}
				else
				{
					this.SuspendLayout();
					this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;

					Size clientSize = new Size(m_imageWidth, m_imageHeight);
					this.pictureBox.ClientSize = clientSize;
					this.pictureBox.Location = new Point(0, 0);

					this.ResumeLayout(false);
					this.AutoScroll = true;		// this.AutoScroll must be true here, so that the full-size picture can be scrolled.

					int dw = m_imageWidth - this.ClientSize.Width;
					int dh = m_imageHeight - this.ClientSize.Height;
					this.AutoScrollPosition = new Point(dw > 0 ? dw / 2 : 0, dh > 0 ? dh / 2 : 0);
				}
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(m_photoDescr != null)
			{
				m_photoDescr.releaseImage();
				m_photoDescr = null;
			}
			pictureBox.Image = null;
			if(m_image != null)
			{
				m_image.Dispose();
				m_image = null;
			}
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
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(8, 8);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(256, 216);
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			this.pictureBox.Click += new System.EventHandler(this.pictureBox_Click);
			// 
			// PhotoViewerControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.pictureBox});
			this.Name = "PhotoViewerControl";
			this.Size = new System.Drawing.Size(520, 408);
			this.Resize += new System.EventHandler(this.PhotoViewerControl_Resize);
			this.Load += new System.EventHandler(this.PhotoViewerControl_Load);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.PhotoViewerControl_Layout);
			this.ResumeLayout(false);

		}
		#endregion

		private void PhotoViewerControl_Load(object sender, System.EventArgs e)
		{
			hasLoaded = true;
			sizeThePictureBox();
		}

		private void PhotoViewerControl_Resize(object sender, System.EventArgs e)
		{
			if(m_fitToSize)
			{
				sizeThePictureBox();
			}
		}

		private void pictureBox_Click(object sender, System.EventArgs e)
		{
			this.OnClick(e);
		}

		private void PhotoViewerControl_Layout(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			//sizeThePictureBox();
		}
	}
}
