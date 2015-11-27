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
using System.IO;
using System.Threading;
using System.Text;

using LibSys;
using LibNet;
using LibGui;
using LibGeo;

namespace WindowsTiler
{
	/// <summary>
	/// Summary description for DlgWeeklyCacheImport.
	/// </summary>
	public class DlgWeeklyCacheImport : System.Windows.Forms.Form
	{
		private PictureManager m_pictureManager;
		private static int m_errorCount = 0;
		private static bool m_stop = false;
		private const int m_maxLinks = 20;
		private const int m_processingDelaySec = 5;

		private System.Windows.Forms.TextBox importTextBox;
		private System.Windows.Forms.Button importButton;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label msgLabel;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.LinkLabel geocachingLinkLabel;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgWeeklyCacheImport(PictureManager pictureManager)
		{
			m_pictureManager = pictureManager;
			m_stop = false;

			InitializeComponent();

			msgLabel.Text = 
				  "Copy some text with the closest links from your Geocaching Weekly Notification email into the text box below."
				+ "Then click Import button at the bottom and wait till the program imports all data. You can later access individual geocaches pages by clicking on their names on the map.\n\n"
				+ "There is a limit of " + m_maxLinks + " geocaches and an artificial " + m_processingDelaySec + " seconds delay in processing, that helps to keep the load of Geocaching servers low."; 

			importTextBox.Text = "\r\n\r\nPaste your Geocaching Weekly Notification email here.";

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
			this.msgLabel = new System.Windows.Forms.Label();
			this.importTextBox = new System.Windows.Forms.TextBox();
			this.importButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.statusLabel = new System.Windows.Forms.Label();
			this.geocachingLinkLabel = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// msgLabel
			// 
			this.msgLabel.Location = new System.Drawing.Point(22, 30);
			this.msgLabel.Name = "msgLabel";
			this.msgLabel.Size = new System.Drawing.Size(555, 84);
			this.msgLabel.TabIndex = 0;
			this.msgLabel.Text = "msg";
			// 
			// importTextBox
			// 
			this.importTextBox.Location = new System.Drawing.Point(15, 121);
			this.importTextBox.Multiline = true;
			this.importTextBox.Name = "importTextBox";
			this.importTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.importTextBox.Size = new System.Drawing.Size(562, 159);
			this.importTextBox.TabIndex = 1;
			this.importTextBox.Text = "";
			// 
			// importButton
			// 
			this.importButton.Location = new System.Drawing.Point(409, 333);
			this.importButton.Name = "importButton";
			this.importButton.Size = new System.Drawing.Size(69, 22);
			this.importButton.TabIndex = 2;
			this.importButton.Text = "Import";
			this.importButton.Click += new System.EventHandler(this.importButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(504, 333);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(69, 22);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(15, 333);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(372, 23);
			this.progressBar.TabIndex = 4;
			// 
			// statusLabel
			// 
			this.statusLabel.Location = new System.Drawing.Point(15, 295);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(562, 22);
			this.statusLabel.TabIndex = 5;
			this.statusLabel.Text = "Ready";
			// 
			// geocachingLinkLabel
			// 
			this.geocachingLinkLabel.Location = new System.Drawing.Point(402, 8);
			this.geocachingLinkLabel.Name = "geocachingLinkLabel";
			this.geocachingLinkLabel.Size = new System.Drawing.Size(161, 21);
			this.geocachingLinkLabel.TabIndex = 6;
			this.geocachingLinkLabel.TabStop = true;
			this.geocachingLinkLabel.Text = "http://www.geocaching.com";
			this.geocachingLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.geocachingLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.geocachingLinkLabel_LinkClicked);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(22, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(387, 21);
			this.label1.TabIndex = 7;
			this.label1.Text = "Please visit GeoCaching.com at this link and contribute ---->";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DlgWeeklyCacheImport
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(591, 369);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label1,
																		  this.geocachingLinkLabel,
																		  this.statusLabel,
																		  this.progressBar,
																		  this.closeButton,
																		  this.importButton,
																		  this.importTextBox,
																		  this.msgLabel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DlgWeeklyCacheImport";
			this.Text = "Geocaching.com - Weekly Cache Email Import";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DlgWeeklyCacheImport_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void DlgWeeklyCacheImport_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			m_stop = true;
		}

		private void geocachingLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Project.RunBrowser(geocachingLinkLabel.Text);
		}

		private void importButton_Click(object sender, System.EventArgs e)
		{
			//Project.threadPool.PostRequest (new WorkRequestDelegate (Run), "import geocache");
			ThreadPool2.QueueUserWorkItem (new WaitCallback (Run), "import geocache"); 
			importButton.Enabled = false;
		}

		//private void RunPM(object state, DateTime requestEnqueueTime)
		private void RunPM(object state)
		{
			ProgressMonitorForm.BringFormUp(true);
		}

		//private void Run(object state, DateTime requestEnqueueTime)
		private void Run(object state)
		{
			string emailText = importTextBox.Text;
			string lineToSearch = "http://www.geocaching.com/seek/cache_details.asp";

			m_errorCount = 0;

			//ArrayList links = new ArrayList();
			int nCaches;
			int gcCount = 0;
			int lineno = 0;
			string line;

			StringReader stream = new StringReader(emailText);

			// first, count the links to make sure there are some, and to set progress bar maximum:
			try
			{
				while((line = stream.ReadLine()) != null)
				{
					lineno++;
					if(line.IndexOf(lineToSearch) != -1)
					{
						gcCount++;
						//links.Add(line.Trim());
					}
				}
			} 
			catch {}

			nCaches = gcCount;

			if(nCaches == 0) 
			{
				statusLabel.Text = "Error: pasted text does not contain links to geocache pages.";
			} 
			else
			{
				statusLabel.Text = "OK: email contains " + nCaches + " caches";
				progressBar.Maximum = nCaches;
				progressBar.Minimum = 0;

				// bring up Progress monitor form, first sleep in the loop will give it some time to initialize:
				//Project.threadPool.PostRequest (new WorkRequestDelegate (RunPM), "import geocache"); 
				ThreadPool2.QueueUserWorkItem (new WaitCallback (RunPM), "import geocache"); 

				lineno = 0;
				gcCount = 0;

				try
				{
					stream = new StringReader(emailText);

					while((line = stream.ReadLine()) != null && gcCount < m_maxLinks && !m_stop)
					{
						lineno++;
						try 
						{
							if(line.IndexOf(lineToSearch) != -1)
							{
								// try not to overload the server:
								Thread.Sleep(gcCount == 0 ? 1000 : m_processingDelaySec * 1000);

								gcCount++;
								progressBar.Value = gcCount;
								string link = line.Trim();
								statusLabel.Text = "IP: processing " + link + " - " + gcCount + " of " + nCaches;

								DownloadThread dt = new DownloadThread();
								dt.DownloadUrl = link;
								dt.tile = null;
								int pos = link.IndexOf("ID=");
								dt.baseName = pos < 0 ? link : link.Substring(pos);
								dt.fileName = "";
								dt.CompleteCallback += new DownloadCompleteHandler( gcDownloadCompleteCallback );
								dt.ProgressCallback += new DownloadProgressHandler( gcDownloadProgressCallback );
								dt.addMonitoredMethod = new AddMonitoredMethod(ProgressMonitor.addMonitored);

								//add dt worker method to the thread pool / queue a task 
								//Project.threadPool.PostRequest (new WorkRequestDelegate (dt.Download), dt.baseName); 
								ThreadPool2.QueueUserWorkItem (new WaitCallback (dt.Download), dt.baseName); 
							}
						}
						catch (Exception ee) 
						{
							LibSys.StatusBar.Error(" line=" + lineno + " " + ee.Message);
						}
					}
				} 
				catch {}

				statusLabel.Text = "OK: finished processing, " + gcCount + " links processed, " + m_errorCount + " errors";
				if(gcCount >= m_maxLinks)
				{
					statusLabel.Text += " [limit " + m_maxLinks + " reached]"; 
				}
				WaypointsCache.RefreshWaypointsDisplayed();
				m_pictureManager.Refresh();
			}
			importButton.Enabled = true;
		}

		private static void gcDownloadProgressCallback ( DownloadInfo info )
		{
			//LibSys.StatusBar.Trace("IP: DlgWeeklyCacheImport:gcDownloadProgressCallback() - " + info.baseName + " - " + info.bytesProcessed + " of " + info.dataLength);
			if(info.monitored != null)
			{
				if(info.dataLength > 0)
				{
					info.monitored.Progress = info.bytesProcessed * 100 / info.dataLength;
					ProgressMonitor.WorkValues();
				}
				else
				{
					info.monitored.Progress = 20;
				}
			}
		}

		private static void gcDownloadCompleteCallback ( object obj, DownloadInfo info, string gcFileName, byte[] dataDownloaded )
		{
			LibSys.StatusBar.Trace("IP: DlgWeeklyCacheImport:gcDownloadCompleteCallback() - " + info.baseName + " " + (dataDownloaded == null? "null" : "" + dataDownloaded.Length) + " bytes loaded");

			if(m_stop)
			{
				ProgressMonitor.markComplete(info.monitored, false, "stopped");
				return;
			}

			if(dataDownloaded == null || dataDownloaded.Length < 100 || Project.is404(dataDownloaded)) 
			{
				string comment = dataDownloaded == null ? "no data" : "404";
				ProgressMonitor.markComplete(info.monitored, false, comment);
			} 
			else 
			{
				try 
				{
					string comment = "" + dataDownloaded.Length + " bytes";
					LibSys.StatusBar.Trace("OK: link " + info.strUrl + " delivered");

					bool success = processGcPage(info.strUrl, dataDownloaded);
					if(!success)
					{
						m_errorCount++;
					}
					
					ProgressMonitor.markComplete(info.monitored, true, comment);
				} 
				catch (Exception e) 
				{
					LibSys.StatusBar.Error("" + e.Message);
					ProgressMonitor.markComplete(info.monitored, false, e.Message);
				}
			}
		}

		private static bool processGcPage(string url, byte[] dataDownloaded)
		{
			bool ret = false;

			int lineno = 0;
			string line;
			int state = 0;
			string lineToSearch = "<span id=\"CacheName\">";
			int pos;
			string cacheName = "";

			ASCIIEncoding enc = new ASCIIEncoding();
			string pageText = new String(enc.GetChars(dataDownloaded));

			CreateInfo createInfo = new CreateInfo();	// we will recycle it in place, filling every time.

			StringReader stream = new StringReader(pageText);

			try
			{
				while((line = stream.ReadLine()) != null && state < 99)
				{
					lineno++;
					switch(state)
					{
						case 0:
							if((pos = line.IndexOf(lineToSearch)) != -1)
							{
								pos += lineToSearch.Length;
								int pos1 = line.IndexOf("</span>", pos);
								if(pos1 > pos)
								{
									cacheName = line.Substring(pos, pos1-pos).Trim();
									LibSys.StatusBar.Trace("OK: cacheName='" + cacheName + "'");

									state = 1;
									lineToSearch = "<span id=\"LatLon\"";
								}
							}
							break;
						case 1:
							if((pos = line.IndexOf(lineToSearch)) != -1)
							{
								pos += lineToSearch.Length;
								int pos1 = line.IndexOf("</span>", pos);
								if(pos1 > pos)
								{
									string sCacheCoords = line.Substring(pos, pos1-pos).Trim();
									LibSys.StatusBar.Trace("OK: cacheCoords='" + sCacheCoords + "'");
									GeoCoord loc = getCleanLocation(sCacheCoords);
									//LibSys.StatusBar.Trace("OK: loc='" + loc + "'");

									createInfo.init("wpt");
									createInfo.lat = loc.Lat;
									createInfo.lng = loc.Lng;
									createInfo.typeExtra = "geocache";
									createInfo.source = url;
									createInfo.name = cacheName;
									createInfo.url = url;

									WaypointsCache.insertWaypoint(createInfo);

									WaypointsCache.isDirty = true;

									ret = true;	// report successfully parsed page
									state = 99;
									lineToSearch = "";
								}
							}
							break;
					}
				}
			} 
			catch {}

			return ret;
		}

		private static GeoCoord getCleanLocation(string str)
		{
			// string like '><font size="3">N 33?? 27.661 W 117?? 42.945</STRONG> (WGS84)<STRONG><br></font>'
			// or ' style="font-size:Small;">N 33?? 27.661 W 117?? 42.945</STRONG> (WGS84)<STRONG><br>'

			int pos = str.IndexOf("\">");
			if(pos == -1)
			{
				return null;
			}
			str = str.Substring(pos + 2);
			pos = str.IndexOf("<");
			if(pos == -1)
			{
				return null;
			}
			str = str.Substring(0, pos);

			// 'N 33?? 27.661 W 117?? 42.945'
			str = str.Replace("?? ",",");
			str = str.Replace("N ","N");
			str = str.Replace("S ","S");
			str = str.Replace("W ","W");
			str = str.Replace("E ","E");

			// 'N33,27.661 W117,42.945' is good for GeoCoord constructor

			return new GeoCoord(str);
		}
	}
}
