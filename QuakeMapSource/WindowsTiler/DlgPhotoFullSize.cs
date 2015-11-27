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
using System.Threading;
using System.IO;

using LibSys;
using LibGeo;
using LibGui;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgPhotoFullSize.
	/// </summary>
	public class DlgPhotoFullSize : System.Windows.Forms.Form
	{
		private static DlgPhotoFullSize This = null;
		public static bool isUp { get { return This != null; } }

		private bool duplicate;	// disables all functions, causes close()
		private static PhotoDescr m_photoDescr = null;
		private static Waypoint m_wpt = null;
		private static bool m_keepInView = true;

		private LibSys.PhotoViewerControl photoViewerControl;
		private PhotoPropertiesPopup photoPropertiesPopup;

		private System.Windows.Forms.Panel controlPanel;
		private System.Windows.Forms.CheckBox fitToSizeCheckBox;
		private System.Windows.Forms.Button fwdButton;
		private System.Windows.Forms.Button backButton;
		private System.Windows.Forms.Button ffwdButton;
		private System.Windows.Forms.Button rewindButton;
		private System.Windows.Forms.CheckBox keepInViewCheckBox;
		private System.Windows.Forms.Panel viewerPanel;
		private System.Windows.Forms.Panel btnPanel;
		private System.Windows.Forms.Button zoomOutButton;
		private System.Windows.Forms.Button zoomCloseButton;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button managerButton;
		private System.Windows.Forms.LinkLabel detailLinkLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private DlgPhotoFullSize()
		{
			if(isUp) 
			{
				// duplicate for this instance stays true, so that all functions are disabled
				// and the form will be closed on load.
				this.duplicate = true;
				this.Load += new System.EventHandler(this.DlgPhotoFullSize_Load);	// we will close this instance on load
			}
			else
			{
				This = this;
				duplicate = false;
				InitializeComponent();

				// 
				// photoViewerControl
				// 
				this.photoViewerControl = new LibSys.PhotoViewerControl();
				this.viewerPanel.Controls.AddRange(new System.Windows.Forms.Control[] { this.photoViewerControl});
				this.photoViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.photoViewerControl.Name = "photoViewerControl";
				this.photoViewerControl.photoDescr = m_photoDescr;
				this.photoViewerControl.TabIndex = 1;
				this.photoViewerControl.Cursor = Cursors.Hand;
				this.photoViewerControl.Click += new System.EventHandler(this.photoViewerControl_Click);

				photoPropertiesPopup = new PhotoPropertiesPopup();

				setPhotoDetail();

				fitToSizeCheckBox.Checked = Project.photoFitToSize;
				keepInViewCheckBox.Checked = m_keepInView;

				if(Project.fitsScreen(Project.photoFullSizeX, Project.photoFullSizeY, Project.photoFullSizeWidth, Project.photoFullSizeHeight))
				{
					inResize = true;
					this.Location = new Point(Project.photoFullSizeX, Project.photoFullSizeY);
					this.ClientSize = new System.Drawing.Size(Project.photoFullSizeWidth, Project.photoFullSizeHeight);		// causes Resize()
				}

				setupButtons();

				Project.setDlgIcon(this);
			}
		}

		private void DlgPhotoFullSize_Load(object sender, System.EventArgs e)
		{
			if(duplicate)
			{
				this.Dispose();
			}
			else
			{
				photoViewerControl.fitToSize = Project.photoFitToSize;	// also performs layout
				inResize = false;
			}
		}

		private void DlgPhotoFullSize_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(!duplicate)
			{
				This = null;
			}
		}

		/// <summary>
		/// Clean up any resources being used. Can be called multiple times
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(!duplicate)
			{
				This = null;
			}
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			photoViewerControl.Dispose();
			photoPropertiesPopup.canClose = true;
			photoPropertiesPopup.Dispose();

			base.Dispose( disposing );

			GC.Collect();
		}

		public static void DisposeIfUp()
		{
			if(This != null)
			{
				This.Dispose();
			}
		}

		// can be called from different thread:
		public static void BringFormUp(PhotoDescr photoDescr, Waypoint wpt)
		{
			try 
			{
				if(wpt != null)
				{
					m_wpt = wpt;
					PhotoWaypoints.SetCurrentWaypoint(m_wpt);
				}
				else
				{
					m_wpt = PhotoWaypoints.CurrentWaypoint();
				}

				if(photoDescr == null)
				{
					try 
					{
						photoDescr = PhotoDescr.FromThumbnail(m_wpt.ThumbSource);
					}
						// An invalid image will throw an OutOfMemoryException 
						// exception
					catch (OutOfMemoryException) 
					{
					}
				}
				else if(wpt == null)
				{
					m_wpt = null;	// from Unrelated photos
				}
				m_photoDescr = photoDescr;

				if(This == null)
				{
					if(Project.mainForm.InvokeRequired)
					{
						Project.mainForm.Invoke(new MethodInvoker(RunPhotoFullSizeForm));
					} 
					else
					{
						RunPhotoFullSizeForm();
					}
				} 
				else
				{
					if(Project.mainForm.InvokeRequired)
					{
						Project.mainForm.Invoke(new MethodInvoker(RunBringUp));
					} 
					else
					{
						RunBringUp();
					}
				}
			} 
			catch {}
		}

		private static void RunPhotoFullSizeForm()
		{
			new DlgPhotoFullSize();
			This.Show();
		}

		private static void RunBringUp()
		{
			if(m_photoDescr != null)
			{
				This.photoViewerControl.photoDescr = m_photoDescr;
				This.setPhotoDetail();
			}
			This.setupButtons();
			This.BringToFront();
		}

		private void disableButtons()
		{
			if(m_wpt != null)
			{
				btnPanel.Visible = true;
				fwdButton.Enabled = ffwdButton.Enabled = false;
				backButton.Enabled = rewindButton.Enabled = false;
			}
		}

		private void setupButtons()
		{
			if(m_wpt != null)
			{
				btnPanel.Visible = true;
				fwdButton.Enabled = ffwdButton.Enabled = PhotoWaypoints.hasNextWaypoint();
				backButton.Enabled = rewindButton.Enabled = PhotoWaypoints.hasPreviousWaypoint();
			}
			else
			{
				// viewing unrelated photos.
				btnPanel.Visible = false;
			}

			PhotoDescr photoDescr = this.photoViewerControl.photoDescr;

			if(photoDescr != null)
			{
				this.Text = "Photo Preview - " + photoDescr.imageName;
			}

			if(photoDescr != null && photoDescr.imageSourceIsLocal)
			{
				this.deleteButton.Enabled = true;
			}
			else
			{
				this.deleteButton.Enabled = false;
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			//LibSys.StatusBar.Trace("ProcessDialogKey: " + keyData);

			bool keystrokeProcessed = true;
			switch (keyData) 
			{
				case Keys.Escape:
				case Keys.Alt | Keys.F4:
					this.Close();
					break;
				case Keys.PageUp:
					PhotoWaypoints.FirstWaypoint();
					preview();
					break;
				case Keys.PageDown:
					PhotoWaypoints.LastWaypoint();
					preview();
					break;
				case Keys.Back:
				case Keys.Up:
				case Keys.Left:
					PhotoWaypoints.PreviousWaypoint();
					preview();
					break;
				case Keys.Down:
				case Keys.Right:
					PhotoWaypoints.NextWaypoint();
					preview();
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.controlPanel = new System.Windows.Forms.Panel();
			this.fitToSizeCheckBox = new System.Windows.Forms.CheckBox();
			this.detailLinkLabel = new System.Windows.Forms.LinkLabel();
			this.managerButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.btnPanel = new System.Windows.Forms.Panel();
			this.zoomOutButton = new System.Windows.Forms.Button();
			this.zoomCloseButton = new System.Windows.Forms.Button();
			this.rewindButton = new System.Windows.Forms.Button();
			this.fwdButton = new System.Windows.Forms.Button();
			this.backButton = new System.Windows.Forms.Button();
			this.ffwdButton = new System.Windows.Forms.Button();
			this.keepInViewCheckBox = new System.Windows.Forms.CheckBox();
			this.viewerPanel = new System.Windows.Forms.Panel();
			this.controlPanel.SuspendLayout();
			this.btnPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlPanel
			// 
			this.controlPanel.BackColor = System.Drawing.SystemColors.Control;
			this.controlPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.fitToSizeCheckBox,
																					   this.detailLinkLabel,
																					   this.managerButton,
																					   this.deleteButton,
																					   this.btnPanel,
																					   this.keepInViewCheckBox});
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.controlPanel.Location = new System.Drawing.Point(0, 499);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new System.Drawing.Size(667, 25);
			this.controlPanel.TabIndex = 0;
			// 
			// fitToSizeCheckBox
			// 
			this.fitToSizeCheckBox.Location = new System.Drawing.Point(5, 5);
			this.fitToSizeCheckBox.Name = "fitToSizeCheckBox";
			this.fitToSizeCheckBox.Size = new System.Drawing.Size(85, 20);
			this.fitToSizeCheckBox.TabIndex = 100;
			this.fitToSizeCheckBox.Text = "Fit to Size";
			this.fitToSizeCheckBox.CheckedChanged += new System.EventHandler(this.fitToSizeCheckBox_CheckedChanged);
			// 
			// detailLinkLabel
			// 
			this.detailLinkLabel.Location = new System.Drawing.Point(350, 5);
			this.detailLinkLabel.Name = "detailLinkLabel";
			this.detailLinkLabel.Size = new System.Drawing.Size(65, 20);
			this.detailLinkLabel.TabIndex = 119;
			this.detailLinkLabel.TabStop = true;
			this.detailLinkLabel.Text = "details >>";
			this.detailLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.detailLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.detailLinkLabel_LinkClicked);
			// 
			// managerButton
			// 
			this.managerButton.Location = new System.Drawing.Point(200, 5);
			this.managerButton.Name = "managerButton";
			this.managerButton.Size = new System.Drawing.Size(72, 20);
			this.managerButton.TabIndex = 118;
			this.managerButton.Text = "Manage";
			this.managerButton.Click += new System.EventHandler(this.managerButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Location = new System.Drawing.Point(275, 5);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(72, 20);
			this.deleteButton.TabIndex = 117;
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// btnPanel
			// 
			this.btnPanel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.zoomOutButton,
																				   this.zoomCloseButton,
																				   this.rewindButton,
																				   this.fwdButton,
																				   this.backButton,
																				   this.ffwdButton});
			this.btnPanel.Location = new System.Drawing.Point(455, 0);
			this.btnPanel.Name = "btnPanel";
			this.btnPanel.Size = new System.Drawing.Size(215, 30);
			this.btnPanel.TabIndex = 116;
			// 
			// zoomOutButton
			// 
			this.zoomOutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomOutButton.Location = new System.Drawing.Point(190, 5);
			this.zoomOutButton.Name = "zoomOutButton";
			this.zoomOutButton.Size = new System.Drawing.Size(21, 20);
			this.zoomOutButton.TabIndex = 121;
			this.zoomOutButton.Text = "^";
			this.zoomOutButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zoomOutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomCloseButton
			// 
			this.zoomCloseButton.Location = new System.Drawing.Point(125, 5);
			this.zoomCloseButton.Name = "zoomCloseButton";
			this.zoomCloseButton.Size = new System.Drawing.Size(65, 20);
			this.zoomCloseButton.TabIndex = 120;
			this.zoomCloseButton.Text = "1m/pix";
			this.zoomCloseButton.Click += new System.EventHandler(this.zoomCloseButton_Click);
			// 
			// rewindButton
			// 
			this.rewindButton.Location = new System.Drawing.Point(5, 5);
			this.rewindButton.Name = "rewindButton";
			this.rewindButton.Size = new System.Drawing.Size(31, 20);
			this.rewindButton.TabIndex = 111;
			this.rewindButton.Text = "|<";
			this.rewindButton.Click += new System.EventHandler(this.rewindButton_Click);
			// 
			// fwdButton
			// 
			this.fwdButton.Location = new System.Drawing.Point(60, 5);
			this.fwdButton.Name = "fwdButton";
			this.fwdButton.Size = new System.Drawing.Size(20, 20);
			this.fwdButton.TabIndex = 114;
			this.fwdButton.Text = ">";
			this.fwdButton.Click += new System.EventHandler(this.fwdButton_Click);
			// 
			// backButton
			// 
			this.backButton.Location = new System.Drawing.Point(35, 5);
			this.backButton.Name = "backButton";
			this.backButton.Size = new System.Drawing.Size(20, 20);
			this.backButton.TabIndex = 113;
			this.backButton.Text = "<";
			this.backButton.Click += new System.EventHandler(this.backButton_Click);
			// 
			// ffwdButton
			// 
			this.ffwdButton.Location = new System.Drawing.Point(80, 5);
			this.ffwdButton.Name = "ffwdButton";
			this.ffwdButton.Size = new System.Drawing.Size(31, 20);
			this.ffwdButton.TabIndex = 112;
			this.ffwdButton.Text = ">|";
			this.ffwdButton.Click += new System.EventHandler(this.ffwdButton_Click);
			// 
			// keepInViewCheckBox
			// 
			this.keepInViewCheckBox.Location = new System.Drawing.Point(90, 5);
			this.keepInViewCheckBox.Name = "keepInViewCheckBox";
			this.keepInViewCheckBox.Size = new System.Drawing.Size(110, 20);
			this.keepInViewCheckBox.TabIndex = 115;
			this.keepInViewCheckBox.Text = "View on Map";
			this.keepInViewCheckBox.CheckedChanged += new System.EventHandler(this.keepInViewCheckBox_CheckedChanged);
			// 
			// viewerPanel
			// 
			this.viewerPanel.BackColor = System.Drawing.SystemColors.ControlDark;
			this.viewerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.viewerPanel.Name = "viewerPanel";
			this.viewerPanel.Size = new System.Drawing.Size(667, 499);
			this.viewerPanel.TabIndex = 1;
			// 
			// DlgPhotoFullSize
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(667, 524);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.viewerPanel,
																		  this.controlPanel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(630, 380);
			this.Name = "DlgPhotoFullSize";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Photo Preview";
			this.Resize += new System.EventHandler(this.DlgPhotoFullSize_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgPhotoFullSize_Closing);
			this.Load += new System.EventHandler(this.DlgPhotoFullSize_Load);
			this.Move += new System.EventHandler(this.DlgPhotoFullSize_Move);
			this.controlPanel.ResumeLayout(false);
			this.btnPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void fitToSizeCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			Project.photoFitToSize = fitToSizeCheckBox.Checked;
			photoViewerControl.fitToSize = Project.photoFitToSize;
		}

		private void setPhotoDetail()
		{
			photoPropertiesPopup.setParameters(m_wpt, m_photoDescr);
		}
				
		/// <summary>
		/// returns true if it could actually find something to display
		/// </summary>
		/// <returns></returns>
		private bool preview()
		{
			bool ret = false;

			disableButtons();
			// Create an Image object from the specified file.
			try 
			{
				m_wpt = PhotoWaypoints.CurrentWaypoint();
				if(m_wpt != null)
				{
					PhotoDescr photoDescr = PhotoDescr.FromThumbnail(m_wpt.ThumbSource);

					photoViewerControl.photoDescr = m_photoDescr = photoDescr;

					setPhotoDetail();
					if(m_keepInView)
					{
						PictureManager.This.CameraManager.keepInView(m_wpt.Location);
					}
					PictureManager.This.CameraManager.MarkLocation(m_wpt.Location, 0);
					ret = true;
				}
				else
				{
					photoViewerControl.photoDescr = m_photoDescr = null;
				}
			}
			// An invalid image will throw an OutOfMemoryException 
			// exception
			catch (OutOfMemoryException) 
			{
				throw new InvalidOperationException("'"	+ m_wpt.ThumbSource + "' is not a valid image file.");
			}

			if(ret)
			{
				setupButtons();
			}
			DlgPhotoManager.sync(false);
			this.BringToFront();

			return ret;
		}

		private void rewindButton_Click(object sender, System.EventArgs e)
		{
			PhotoWaypoints.FirstWaypoint();
			preview();
		}

		private void backButton_Click(object sender, System.EventArgs e)
		{
			PhotoWaypoints.PreviousWaypoint();
			preview();
		}

		private void fwdButton_Click(object sender, System.EventArgs e)
		{
			PhotoWaypoints.NextWaypoint();
			preview();
		}

		private void ffwdButton_Click(object sender, System.EventArgs e)
		{
			PhotoWaypoints.LastWaypoint();
			preview();
		}

		private void keepInViewCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			m_keepInView = keepInViewCheckBox.Checked;
			if(m_wpt != null && m_keepInView)
			{
				PictureManager.This.CameraManager.keepInView(m_wpt.Location);
			}
		}

		private bool inResize = false;

		private void DlgPhotoFullSize_Resize(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private void DlgPhotoFullSize_Move(object sender, System.EventArgs e)
		{
			memorizeSizeAndPosition();
		}

		private void memorizeSizeAndPosition()
		{
			bool formMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
			if(!formMaximized && !inResize)
			{
				Project.photoFullSizeWidth = this.ClientSize.Width;
				Project.photoFullSizeHeight = this.ClientSize.Height;
				Project.photoFullSizeX = this.Location.X;
				Project.photoFullSizeY = this.Location.Y;
			}
		}

		private void zoomCloseButton_Click(object sender, System.EventArgs e)
		{
			Waypoint wpt = PhotoWaypoints.CurrentWaypoint();
			if(wpt != null)
			{
				if(Math.Round(PictureManager.This.CameraManager.Location.Elev/10.0d) != Math.Round(Project.CAMERA_HEIGHT_REAL_CLOSE * 100.0d))
				{
					DlgPhotoManager.savedLocation = new GeoCoord(PictureManager.This.CameraManager.Location);
				}
				GeoCoord newLocation = new GeoCoord(wpt.Location);
				newLocation.Elev = Project.CAMERA_HEIGHT_REAL_CLOSE * 1000.0d;		// for 1m/pixel
				PictureManager.This.CameraManager.SpoilPicture();
				PictureManager.This.CameraManager.Location = newLocation;		// calls ProcessCameraMove();
				zoomCloseButton.Enabled = false;
				zoomOutButton.Enabled = true;
			}
		}

		private void zoomOutButton_Click(object sender, System.EventArgs e)
		{
			if(DlgPhotoManager.savedLocation != null)
			{
				PictureManager.This.CameraManager.SpoilPicture();
				PictureManager.This.CameraManager.Elev = DlgPhotoManager.savedLocation.Elev;		// calls ProcessCameraMove();
				DlgPhotoManager.savedLocation = null;
				zoomCloseButton.Enabled = true;
				zoomOutButton.Enabled = false;
			}
		}

		private void managerButton_Click(object sender, System.EventArgs e)
		{
			new DlgPhotoManager(0).ShowDialog();
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			if(m_photoDescr != null && m_photoDescr.imageSourceIsLocal)
			{
				string imageSource = m_photoDescr.imageSource;
				try
				{
					string msg;
					int pos = imageSource.IndexOf("|");
					if(pos > 0)
					{
						// this is a .zip or .gpz file
						string zipFileName = imageSource.Substring(0, pos);
						string photoFileName = imageSource.Substring(pos + 1);
						msg = "Will permanently delete entry '" + photoFileName + "' from the zip archive '" + zipFileName
									+ "'.\n\nThe zip archive file will NOT be deleted.\n\nAre you sure?";
					}
					else
					{
						// plain file
						msg = "Will permanently delete file '" + imageSource + "' from disk.\n\nAre you sure?";
					}

					string fileToDelete = null;

					if(Project.YesNoBox(this, msg))
					{
						if(m_wpt != null)
						{
							if(m_wpt.TrackId == -1)
							{
								WaypointsCache.RemoveWaypointById(m_wpt.Id);
							}
							PhotoWaypoints.RemoveWaypoint(m_wpt);		// from track too, if any

							photoViewerControl.photoDescr = null;
							
							fileToDelete = m_photoDescr.deleteFromDisk();
							
							if(preview())		// will find current waypoint, if any, and set m_photoDescr
							{
								setPhotoDetail();
								photoViewerControl.Refresh();
							}
							else
							{
								// no image to display, need to wrap up
								m_photoDescr = null;
								this.Close();
							}
							PictureManager.This.Refresh();
						}
						else
						{
							PhotoWaypoints.RemoveUnrelatedById(m_photoDescr.Id);
							
							fileToDelete = m_photoDescr.deleteFromDisk();
							
							if(PhotoWaypoints.PhotosUnrelated.Count > 0)
							{
								photoViewerControl.photoDescr = m_photoDescr = (PhotoDescr)PhotoWaypoints.PhotosUnrelated.GetByIndex(0);
								setPhotoDetail();
								photoViewerControl.Refresh();
								this.setupButtons();
								this.BringToFront();
							}
							else
							{
								photoViewerControl.photoDescr = m_photoDescr = null;
								this.Close();
							}
						}

						DlgPhotoManager.sync(true);

						if(fileToDelete != null)
						{
							GC.Collect();
							FileInfo fi = new FileInfo(fileToDelete);
							fi.Delete();
						}
					}
				}
				catch (Exception exc)
				{
					Project.ErrorBox(this, "Failed to delete " + imageSource + "\n\nException: " + exc.Message);
					this.Close();
				}
			}
		}

		private void photoViewerControl_Click(object sender, System.EventArgs e)
		{
			DlgPhotoDraw dlgPhotoDraw = new DlgPhotoDraw(m_wpt, m_photoDescr);

			if(DlgPhotoDraw.m_dlgSizeX < 100)
			{
				dlgPhotoDraw.Top = this.PointToScreen(new Point(0, 0)).Y + 20;
				dlgPhotoDraw.Left = this.PointToScreen(new Point(0, 0)).X + 20;
			}
	
			dlgPhotoDraw.ShowDialog();
		}


		private void detailLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			photoPropertiesPopup.Top = detailLinkLabel.PointToScreen(new Point(0, 0)).Y + 30;
			photoPropertiesPopup.Left = detailLinkLabel.PointToScreen(new Point(0, 0)).X + detailLinkLabel.Width - photoPropertiesPopup.Width;
			photoPropertiesPopup.Show();
		}

	}
}
