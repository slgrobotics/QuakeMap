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

using LibGeo;
using LibSys;

namespace LibGui
{
	/// <summary>
	/// Summary description for TrackProfileControl.
	/// </summary>
	public class TrackProfileControl : System.Windows.Forms.UserControl, IEnableDisable
	{
		private static TrackProfileControl This;

		private Track m_track;
		private Waypoint m_trkpt;

		private TrackGraphControl tgc;

		private System.Windows.Forms.Panel controlPanel;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.Panel graphPanel;
		private System.Windows.Forms.Label infoLabel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TrackProfileControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			This = this;

			tgc = new TrackGraphControl();
			tgc.Dock = System.Windows.Forms.DockStyle.Fill;
			tgc.enableDisable = this;

			tgc.itemSelected += new GraphItemEvent(this.itemSelectedHandler);
			tgc.itemRangeSelected += new GraphItemsRangeEvent(this.itemsRangeSelectedHandler);

			graphPanel.Controls.Add(tgc);
		}

		public static Track track
		{
			get { return This.m_track; }
			
			set { 
				resetSelection();

				This.m_track = value;
				if(This.m_track != null)
				{
					This.infoLabel.Text = This.m_track.ToStringProfile();
				}
				else
				{
					This.infoLabel.Text = "no track or route selected";
				}
			}
		}

		public static void resetSelection()
		{
			if(SelectFilter.Enabled)
			{
				SelectFilter.reset();
				PictureManager.This.Refresh();
			}
		}

		public static void resetTrack()
		{
			resetSelection();

			This.m_track = null;
			This.m_trkpt = null;
			This.tgc.setTrackAndTrackpoint(null, null);
			This.infoLabel.Text = "click on track or route to see elevation profile";
		}

		public static void setTrackByTrackpoint(Waypoint trkpt)
		{
			if(trkpt == null)
			{
				resetTrack();
			}

			if(Project.mainCommand.PlannerPaneVisible())
			{
				track = WaypointsCache.getTrackById(trkpt.TrackId);
				This.m_trkpt = trkpt;
				if(trkpt != null)
				{
					This.infoLabel.Text += " | " + trkpt.ToStringProfile();
					if(Project.mainCommand.PlannerPaneVisible())
					{
						GeoCoord loc = new GeoCoord(trkpt.Location.X, trkpt.Location.Y, PictureManager.This.CameraManager.Elev);
						PictureManager.This.CameraManager.MarkLocation(loc, 3);
					}
					else
					{
						PictureManager.This.CameraManager.removeMarkLocation();
					}
				}
				This.tgc.setTrackAndTrackpoint(track, This.m_trkpt);
			}
		}

		private void itemSelectedHandler(GraphItem gi)
		{
			resetSelection();

			try
			{
				Waypoint trkpt = (Waypoint)gi.itemKeyObj;
				PictureManager.This.CameraManager.MarkLocation(trkpt.Location, 3);
			}
			catch {}
		}

		private void itemsRangeSelectedHandler(GraphItem fromGi, GraphItem toGi)
		{
			try
			{
				Waypoint fromTrkpt = (Waypoint)fromGi.itemKeyObj;
				Waypoint toTrkpt = (Waypoint)toGi.itemKeyObj;

				if(fromTrkpt != null && toTrkpt != null)
				{
					SelectFilter.track = this.tgc.Track;
					SelectFilter.fromTrkpt = fromTrkpt;
					SelectFilter.toTrkpt = toTrkpt;
					SelectFilter.Enabled = true;
				}
				else
				{
					SelectFilter.reset();
				}
			}
			catch
			{
				SelectFilter.reset();
			}
			PictureManager.This.Refresh();
		}

		#region IEnableDisable implementation

		public void disable(int feature)
		{
			/*
			switch(feature) 
			{
				case GraphByTimeControl.ED_SELECTED:
					showSelectedButton.Enabled = false;
					applySelectedButton.Enabled = false;
					scopeFilteredRadioButton.Enabled = false;
					scopeSelectedGraphRadioButton.Enabled = false;
					break;
				case GraphByTimeControl.ED_ALLDATA:
					//m_buttonAllData.disable();
					break;
				case GraphByTimeControl.ED_ZOOM:
					zoomGraphButton.Enabled = false;
					break;
				case GraphByTimeControl.ED_BROWSE:
					browseGraphButton.Enabled = false;
					break;
			}
			*/
		}

		public void enable(int feature)
		{
			/*
			switch(feature) 
			{
				case 0:
					showSelectedButton.Enabled = true;
					try
					{
						// DateTime range that can be selected is much more that DateTimePicker can take:
						if((fromDateTimePicker.MinDate - graphByTimeControl.getStartSelectedDateTime()).Seconds < 0)
						{
							applySelectedButton.Enabled = true;
						}
					} 
					catch {}
					scopeFilteredRadioButton.Enabled = true;
					//scopeSelectedGraphRadioButton.Enabled = true;
					break;
				case 1:
					//m_buttonAllData.enable();
					break;
				case 2:
					zoomGraphButton.Enabled = true;
					break;
				case 3:
					browseGraphButton.Enabled = true;
					break;
			}
			*/
		}
		#endregion

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
			this.controlPanel = new System.Windows.Forms.Panel();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.graphPanel = new System.Windows.Forms.Panel();
			this.infoLabel = new System.Windows.Forms.Label();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlPanel
			// 
			this.controlPanel.BackColor = System.Drawing.Color.LawnGreen;
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.controlPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new System.Drawing.Size(65, 120);
			this.controlPanel.TabIndex = 1;
			// 
			// bottomPanel
			// 
			this.bottomPanel.BackColor = System.Drawing.Color.Pink;
			this.bottomPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.infoLabel});
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = new System.Drawing.Point(65, 100);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(735, 20);
			this.bottomPanel.TabIndex = 2;
			// 
			// graphPanel
			// 
			this.graphPanel.BackColor = System.Drawing.Color.DeepSkyBlue;
			this.graphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.graphPanel.Name = "graphPanel";
			this.graphPanel.Size = new System.Drawing.Size(800, 120);
			this.graphPanel.TabIndex = 0;
			// 
			// infoLabel
			// 
			this.infoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(735, 20);
			this.infoLabel.TabIndex = 0;
			// 
			// TrackProfileControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		this.graphPanel,
																		this.bottomPanel,
																		this.controlPanel});
			this.Name = "TrackProfileControl";
			this.Size = new System.Drawing.Size(800, 120);
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
