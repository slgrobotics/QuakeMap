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

namespace LibGui
{
	public class DisplayElement
	{
		public Monitored monitored = null;
		public ProgressBar progressBar = new System.Windows.Forms.ProgressBar();
		public Label label = new System.Windows.Forms.Label();
	}

	public class ProgressMonitorForm : System.Windows.Forms.Form
	{
		private static ProgressMonitorForm This = null;
		private static bool m_closeWhenDone;
		private const int SECONDS_TO_STAY_UP = 2;

		private bool duplicate;	// disables all functions, causes close()

		private SortedList List = new SortedList();
		private DateTime formUpDateTime;

		private System.Windows.Forms.Label headLabel;
		private System.Windows.Forms.CheckBox keepFinishedCheckBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ProgressMonitorForm(bool closeWhenDone)
		{
			if(This != null) 
			{
				// duplicate for this instance stays true, so that all functions are disabled
				// and the form will be closed on load.
				duplicate = true;
				this.Load += new System.EventHandler(this.ProgressMonitorForm_Load);
			}
			else
			{
				This = this;
				duplicate = false;
				inRebuild = true;
				m_closeWhenDone = closeWhenDone;
				formUpDateTime = DateTime.Now;

				InitializeComponent();
#if !DEBUG
				keepFinishedCheckBox.Visible = false;
#endif
                RebuildProgressBars();

				ProgressMonitor.Display = this;
				Project.setDlgIcon(this);
			}
		}

		// can be called from different thread:
		public static void BringFormUp(bool closeWhenDone)
		{
			try 
			{
				if(This == null)
				{
					m_closeWhenDone = closeWhenDone;
					if(Project.mainForm.InvokeRequired)
					{
						Project.mainForm.Invoke(new MethodInvoker(RunProgressMonitorForm));
					} 
					else
					{
						RunProgressMonitorForm();
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

		private static void RunProgressMonitorForm()
		{
			new ProgressMonitorForm(m_closeWhenDone);
			This.Show();
		}

		private static void RunBringUp()
		{
			This.BringToFront();
		}

		// can be called from different thread (namely from ProgressMonitor worker):
		public void RebuildGui()
		{
			doRebuild = true;
			doRefresh = true;
		}

		// can be called from different thread (namely from ProgressMonitor worker):
		public void MayRefresh()
		{
			if(doRefresh)
			{
				this.Invalidate();
				doRefresh = false;
			}
		}

		private bool doRebuild = false;
		private bool inRebuild = false;
		private bool doRefresh = false;

		// must be called from this form's thread - actually is called from the constructor and from Paint()
		private void RebuildProgressBars()
		{
			inRebuild = true;
			this.SuspendLayout();

			try
			{
				int i;
				for(i=0; i < List.Count ;i++)
				{
					DisplayElement de = (DisplayElement)List.GetByIndex(i);
					this.Controls.Remove(de.progressBar);
					this.Controls.Remove(de.label);
				}
				List.Clear();

				int yPos = 48;
				int activeCount = 0;
				for(i=0; i < ProgressMonitor.List.Count && yPos < 1000 ;i++)
				{
					DateTime key = (DateTime)ProgressMonitor.List.GetKey(i);

					Monitored mm = (Monitored)ProgressMonitor.List.GetByIndex(i);

					DisplayElement de = new DisplayElement();
					de.monitored = mm;

					if(!mm.Complete)
					{
						activeCount++;
					}

					// 
					// progressBar
					// 
					de.progressBar.Location = new System.Drawing.Point(16, yPos);
					de.progressBar.Name = "progressBar" + i;
					de.progressBar.Size = new System.Drawing.Size(120, 16);
					de.progressBar.TabIndex = 1;
					// 
					// progressLabel
					// 
					de.label.Location = new System.Drawing.Point(152, yPos);
					de.label.Name = "progressLabel" + i;
					de.label.Size = new System.Drawing.Size(800, 16);
					de.label.TabIndex = 2;

					List.Add(key, de);

					this.Controls.Add(de.progressBar);
					this.Controls.Add(de.label);
					yPos += 24;
				}
				int newHeight = yPos + 20;
				// do not allow it to resize too often, just when it grows or shrinks fast.
				if(ProgressMonitor.List.Count == 0 || newHeight > this.ClientSize.Height || newHeight < this.ClientSize.Height - 120)
				{
					int width = this.ClientSize.Width;
					this.ClientSize = new System.Drawing.Size(width, newHeight);
				}
				headLabel.Text = "Download Tasks - total:" + i + " active: " + activeCount
					+ " threads: " + ThreadPool2.ActiveThreads					//Project.threadPool.CurrentThreadCount
					+ " queue: " + ThreadPool2.WaitingCallbacks;				//Project.threadPool.RequestQueueCount; 
				_WorkValues();		// set all current values
			}
			catch {}
			finally
			{
				this.ResumeLayout(false);
				inRebuild = false;
			}
		}


		// update values on all bars and labels.
		// can be called from different thread (namely from ProgressMonitor worker):
		public void WorkValues()
		{
			this.Invoke(new MethodInvoker(_WorkValues));
		}

		// local thread only:
		private void _WorkValues()
		{
			try
			{
				for(int i=0; i < List.Count ;i++)
				{
					DisplayElement de = (DisplayElement)List.GetByIndex(i);
					de.progressBar.Value = de.monitored.Progress > 100 ? 100 : de.monitored.Progress;
					de.label.Text = "" + de.monitored;
					if(de.monitored.Complete)
					{
						if(de.monitored.Success)
						{
							de.label.ForeColor = Color.Green;
						} 
						else 
						{
							de.label.ForeColor = Color.Red;
						}
					}
				}
			}
			catch {}
		}

		// called 3 times per second
		// can be called from different thread (namely from ProgressMonitor worker):
		public void purge()
		{
			this.Invoke(new MethodInvoker(_purge));
		}

		// local thread only:
		private void _purge()
		{
			if(This != null && m_closeWhenDone && ProgressMonitor.List.Count == 0
				&& formUpDateTime.AddSeconds(SECONDS_TO_STAY_UP).CompareTo(DateTime.Now) < 0)
			{
				This.Close();
				return;
			}
				
			int deleted = 0;
			try
			{
				if(!keepFinishedCheckBox.Checked)
				{
					for(int i=List.Count-1; i >= 0 ;i--)
					{
						DisplayElement de = (DisplayElement)List.GetByIndex(i);
						if(de.monitored.Purged)
						{
							this.Controls.Remove(de.progressBar);
							this.Controls.Remove(de.label);
							List.RemoveAt(i);
							deleted++;
						}
					}
				}
			}
			catch {}
			finally
			{
				if(deleted > 0)
				{
					RebuildGui();
				}
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(!duplicate)
			{
				ProgressMonitor.Display = null;
				doRebuild = false;
				This = null;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.headLabel = new System.Windows.Forms.Label();
			this.keepFinishedCheckBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// headLabel
			// 
			this.headLabel.Location = new System.Drawing.Point(15, 15);
			this.headLabel.Name = "headLabel";
			this.headLabel.Size = new System.Drawing.Size(445, 22);
			this.headLabel.TabIndex = 0;
			this.headLabel.Text = "headLabel";
			// 
			// keepFinishedCheckBox
			// 
			this.keepFinishedCheckBox.Location = new System.Drawing.Point(475, 8);
			this.keepFinishedCheckBox.Name = "keepFinishedCheckBox";
			this.keepFinishedCheckBox.Size = new System.Drawing.Size(212, 22);
			this.keepFinishedCheckBox.TabIndex = 1;
			this.keepFinishedCheckBox.Text = "Keep Finished Items";
			// 
			// ProgressMonitorForm
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(723, 359);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.keepFinishedCheckBox,
																		  this.headLabel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ProgressMonitorForm";
			this.Text = "Progress Monitor - press Esc to hide (processes will continue)";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ProgressMonitorForm_Closing);
			this.Load += new System.EventHandler(this.ProgressMonitorForm_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ProgressMonitorForm_Paint);
			this.ResumeLayout(false);

		}
		#endregion

		private void ProgressMonitorForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(!duplicate)
			{
				ProgressMonitor.Display = null;
				doRebuild = false;
				This = null;
			}
		}

		private void ProgressMonitorForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			try
			{
				if(!duplicate && doRebuild && !inRebuild)
				{
					doRebuild = false;
					RebuildProgressBars();
				}
			}
			catch {}
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

		private void ProgressMonitorForm_Load(object sender, System.EventArgs e)
		{
			if(duplicate)
			{
				this.Dispose();
			}
		}
	}
}
