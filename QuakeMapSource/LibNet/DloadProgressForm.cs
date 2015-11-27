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
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

using LibSys;

namespace LibNet
{
	/// <summary>
	/// Summary description for DloadProgressForm.
	/// </summary>
	public class DloadProgressForm : System.Windows.Forms.Form
	{
		private bool m_doRun;
		private bool m_closeIfFailed;
		private System.Windows.Forms.Timer m_closeTimer = new System.Windows.Forms.Timer();

		private System.Windows.Forms.GroupBox outputGroupBox;
		private System.Windows.Forms.TextBox totalBytesTextBox;
		private System.Windows.Forms.TextBox bytesDownloadedTextBox;
		private System.Windows.Forms.Label bytesDownloadedLbl;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label totalBytesLbl;
		private System.Windows.Forms.Label urlLabel;
		private System.Windows.Forms.TextBox downloadUrlTextBox;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox downloadFileTextBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DloadProgressForm(string dloadUrl, string dloadFileName, bool doRun, bool closeIfFailed)
		{
			m_doRun = doRun;					// for help file only
			m_closeIfFailed = closeIfFailed;	// for unimportant files - don't hold failed dload form open

			InitializeComponent();

			downloadUrlTextBox.Text = dloadUrl;
			downloadFileTextBox.Text = dloadFileName;

			this.outputGroupBox.Enabled = true;

			this.bytesDownloadedTextBox.Text = "";
			this.totalBytesTextBox.Text = "";
			this.progressBar.Minimum = 0;
			this.progressBar.Maximum = 0;
			this.progressBar.Value = 0;

			// load Dld file and run it:

			DownloadThread dt = new DownloadThread();
			dt.DownloadUrl = dloadUrl;
			dt.tile = this;
			dt.baseName = "dld";
			dt.fileName = dloadFileName;
			dt.CompleteCallback += new DownloadCompleteHandler( completeCallback );
			dt.ProgressCallback += new DownloadProgressHandler( progressCallback );

			//add dt worker method to the thread pool / queue a task 
			//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), dloadUrl); 
			ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), dloadUrl); 

#if DEBUG
			LibSys.StatusBar.Trace("OK: DloadProgressForm:DloadProgressForm() - loading remote from " + dloadUrl);
#endif
			this.Text = Project.PROGRAM_NAME_HUMAN + " - downloading file";
			Project.setDlgIcon(this);
		}

		private void progressCallback ( DownloadInfo info )
		{
			// marshall the call to the thread where this form was created, so that 
			// component-related operations are not hanging the message pump
			this.Invoke(new DownloadProgressHandler(_progressCallback), new object[] { info } );
		}

		private void _progressCallback ( DownloadInfo info )
		{
			//LibSys.StatusBar.Trace("IP: DloadProgressForm:progressCallback() - " + info.baseName + " - " + info.bytesProcessed + " of " + info.dataLength);
			bytesDownloadedTextBox.Text = info.bytesProcessed.ToString("#,##0");

			if ( info.dataLength != -1 )
			{
				progressBar.Minimum = 0;
				progressBar.Maximum = info.dataLength;
				progressBar.Value = info.bytesProcessed;
				totalBytesTextBox.Text = info.dataLength.ToString("#,##0");
			}
			else
			{
				progressBar.Visible = false;
				totalBytesTextBox.Text = "Total File Size Not Known";
			}
		}

		private void completeCallback ( object formObj, DownloadInfo info, string dldFileName, byte[] dataDownloaded )
		{
			// marshall the call to the thread where this form was created, so that 
			// component-related operations are not hanging the message pump
			this.Invoke(new DownloadCompleteHandler(_completeCallback), new object[] { formObj, info, dldFileName, dataDownloaded } );
		}

		private void closeSoon()
		{
			m_closeTimer.Interval = 2000;
			m_closeTimer.Tick += new EventHandler(closeNow);
			m_closeTimer.Start();
		}

		private void closeNow(object obj, System.EventArgs args)
		{
			m_closeTimer.Dispose();
			this.Close();
			this.Dispose();
		}

		private void _completeCallback ( object formObj, DownloadInfo info, string dldFileName, byte[] dataDownloaded )
		{
#if DEBUG
			LibSys.StatusBar.Trace("IP: DloadProgressForm:completeCallback() - " + info.baseName + " " + (dataDownloaded == null? "null" : "" + dataDownloaded.Length) + " bytes loaded");
#endif
			Form form = (Form)formObj;
			if(dataDownloaded == null || Project.is404(dataDownloaded)) 
			{
				string message = "failed: " + (dataDownloaded == null ? "no connection or no data" : "404 - file not found");
				messageLabel.Text = message;
				LibSys.StatusBar.Error(message);
				if(m_closeIfFailed)
				{
					closeSoon();
				}
			}
			else
			{
				if ( !progressBar.Visible )
				{
					progressBar.Visible = true;
					progressBar.Minimum = 0;
					progressBar.Value = progressBar.Maximum = 1;
					totalBytesTextBox.Text = bytesDownloadedTextBox.Text;
				}

				FileStream fs = null;
				try 
				{
					fs = new FileStream(dldFileName, FileMode.Create);
					fs.Write(dataDownloaded, 0, dataDownloaded.Length);
					fs.Close();
					fs = null;
					LibSys.StatusBar.Trace("OK: file " + dldFileName + " created");
					if(m_doRun)	
					{
						// actually for help file only
						Help.ShowHelp(form, dldFileName);
					}
					closeNow(null, EventArgs.Empty);
				} 
				catch (Exception e) 
				{
					string message = "failed: " + e.Message;
					messageLabel.Text = message;
					LibSys.StatusBar.Error(message);
					if(m_closeIfFailed)
					{
						closeSoon();
					}
				}
				finally
				{
					if(fs != null)
					{
						fs.Close();
					}
				}
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
			this.outputGroupBox = new System.Windows.Forms.GroupBox();
			this.totalBytesTextBox = new System.Windows.Forms.TextBox();
			this.bytesDownloadedTextBox = new System.Windows.Forms.TextBox();
			this.bytesDownloadedLbl = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.totalBytesLbl = new System.Windows.Forms.Label();
			this.messageLabel = new System.Windows.Forms.Label();
			this.urlLabel = new System.Windows.Forms.Label();
			this.downloadUrlTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.downloadFileTextBox = new System.Windows.Forms.TextBox();
			this.outputGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// outputGroupBox
			// 
			this.outputGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.totalBytesTextBox,
																						 this.bytesDownloadedTextBox,
																						 this.bytesDownloadedLbl,
																						 this.progressBar,
																						 this.totalBytesLbl,
																						 this.messageLabel});
			this.outputGroupBox.Enabled = false;
			this.outputGroupBox.Location = new System.Drawing.Point(8, 72);
			this.outputGroupBox.Name = "outputGroupBox";
			this.outputGroupBox.Size = new System.Drawing.Size(439, 104);
			this.outputGroupBox.TabIndex = 6;
			this.outputGroupBox.TabStop = false;
			this.outputGroupBox.Text = " progress ";
			// 
			// totalBytesTextBox
			// 
			this.totalBytesTextBox.Location = new System.Drawing.Point(176, 16);
			this.totalBytesTextBox.Name = "totalBytesTextBox";
			this.totalBytesTextBox.ReadOnly = true;
			this.totalBytesTextBox.Size = new System.Drawing.Size(72, 20);
			this.totalBytesTextBox.TabIndex = 4;
			this.totalBytesTextBox.Text = "";
			this.totalBytesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// bytesDownloadedTextBox
			// 
			this.bytesDownloadedTextBox.Location = new System.Drawing.Point(80, 16);
			this.bytesDownloadedTextBox.Name = "bytesDownloadedTextBox";
			this.bytesDownloadedTextBox.ReadOnly = true;
			this.bytesDownloadedTextBox.Size = new System.Drawing.Size(72, 20);
			this.bytesDownloadedTextBox.TabIndex = 3;
			this.bytesDownloadedTextBox.Text = "";
			this.bytesDownloadedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// bytesDownloadedLbl
			// 
			this.bytesDownloadedLbl.Location = new System.Drawing.Point(24, 18);
			this.bytesDownloadedLbl.Name = "bytesDownloadedLbl";
			this.bytesDownloadedLbl.Size = new System.Drawing.Size(48, 16);
			this.bytesDownloadedLbl.TabIndex = 2;
			this.bytesDownloadedLbl.Text = "Bytes:";
			this.bytesDownloadedLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(256, 16);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(176, 20);
			this.progressBar.TabIndex = 0;
			// 
			// totalBytesLbl
			// 
			this.totalBytesLbl.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.totalBytesLbl.Location = new System.Drawing.Point(152, 18);
			this.totalBytesLbl.Name = "totalBytesLbl";
			this.totalBytesLbl.Size = new System.Drawing.Size(16, 16);
			this.totalBytesLbl.TabIndex = 2;
			this.totalBytesLbl.Text = "of";
			this.totalBytesLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// messageLabel
			// 
			this.messageLabel.ForeColor = System.Drawing.Color.Red;
			this.messageLabel.Location = new System.Drawing.Point(8, 48);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(424, 45);
			this.messageLabel.TabIndex = 7;
			// 
			// urlLabel
			// 
			this.urlLabel.Location = new System.Drawing.Point(15, 15);
			this.urlLabel.Name = "urlLabel";
			this.urlLabel.Size = new System.Drawing.Size(73, 18);
			this.urlLabel.TabIndex = 5;
			this.urlLabel.Text = "Source URL :";
			this.urlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// downloadUrlTextBox
			// 
			this.downloadUrlTextBox.Location = new System.Drawing.Point(88, 16);
			this.downloadUrlTextBox.Name = "downloadUrlTextBox";
			this.downloadUrlTextBox.ReadOnly = true;
			this.downloadUrlTextBox.Size = new System.Drawing.Size(354, 20);
			this.downloadUrlTextBox.TabIndex = 4;
			this.downloadUrlTextBox.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(15, 45);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 18);
			this.label1.TabIndex = 9;
			this.label1.Text = "Local File :";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// downloadFileTextBox
			// 
			this.downloadFileTextBox.Location = new System.Drawing.Point(88, 45);
			this.downloadFileTextBox.Name = "downloadFileTextBox";
			this.downloadFileTextBox.ReadOnly = true;
			this.downloadFileTextBox.Size = new System.Drawing.Size(354, 20);
			this.downloadFileTextBox.TabIndex = 8;
			this.downloadFileTextBox.Text = "";
			// 
			// DloadProgressForm
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(458, 180);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label1,
																		  this.downloadFileTextBox,
																		  this.outputGroupBox,
																		  this.urlLabel,
																		  this.downloadUrlTextBox});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DloadProgressForm";
			this.Text = "Downloading File";
			this.TopMost = true;
			this.outputGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
