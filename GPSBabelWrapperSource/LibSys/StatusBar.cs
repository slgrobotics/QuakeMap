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
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using System.IO;

namespace LibSys
{
	/// <summary>
	/// Summary description for StatusBar.
	/// </summary>
	public class StatusBar
	{
		private static Label m_sb = null;
		private static Label m_sb2 = null;
		private static ProgressBar m_pb = null;
		private static string m_traceFileName = null;
		private static TextWriter m_tw;
		private static StatusBar myInstance = new StatusBar();		// we need it to be able to close trace file on ~StatusBar()

		public const int MAX_LOGGED = 1000;

		private static ArrayList m_log = new ArrayList();

		public StatusBar()
		{
			//m_traceFileName = Path.Combine(Application.StartupPath, "trace.txt");
            m_traceFileName = Project.GetMiscPath("trace.txt");
			FileStream fs = new FileStream(m_traceFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			m_tw = new StreamWriter(fs);
			m_tw.WriteLine("Started " + DateTime.Now);
			m_tw.WriteLine("Framework: " + Environment.Version + " Program: " + Project.PROGRAM_NAME_HUMAN + " " + Project.PROGRAM_VERSION_HUMAN  + " Build: " + Project.PROGRAM_VERSION_RELEASEDATE);
			m_tw.Close();
		}

		static void writeTrace(string str)
		{
			FileStream fs = new FileStream(m_traceFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
			m_tw = new StreamWriter(fs);
			m_tw.WriteLine(str);
			m_tw.Close();
		}

		// sets gui pointers, when they are ready. The StatusBar is functional before that, just stores messages in m_log.
		public static void setInterface(Label sb, Label sb2, ProgressBar pb)
		{
			m_sb = sb;
			m_sb2 = sb2;
			m_pb = pb;
		}

		// thread safe
		public static void Trace(string str)
		{
#if DEBUG
			if(Project.enableTrace || (Project.traceToken.Length > 0 && str.ToLower().IndexOf(Project.traceToken.ToLower()) >= 0)) 
			{
#endif
				WriteLine(str);
#if DEBUG
			}		
#endif
		}

		// thread safe
		public static void Error(string str)
		{
			WriteLine("Error: " + str);
		}

		private static string m_message = "";

		// thread safe
		public static void WriteLine(string str)
		{
			m_message = str;
#if DEBUG
			System.Console.WriteLine(str);
#endif
			try
			{

				if(!str.StartsWith("*"))
				{
					writeTrace(str);
					m_log.Add(str);
					if(m_log.Count > MAX_LOGGED) 
					{
						m_log.RemoveAt(0);
					}
				}
			}
			catch
			{
#if DEBUG
				System.Console.WriteLine("Exception in WriteLine()");
#endif
			}
		}
		
		// thread safe - called often
		public static void TraceSync()
		{
			if(m_message != null && m_message.Length > 0 || m_highlightCount == 0)
			{
				if(m_sb != null && m_sb.Created && m_sb.Visible)
				{
					if(m_sb.InvokeRequired)
					{
						m_sb.Invoke(new MethodInvoker(setMessage));
					}
					else
					{
						setMessage();
					}
				}
				m_message = "";
			}
			if(m_highlightCount >= 0)
			{
				m_highlightCount--;	// till it becomes -1
			}
		}

		private static int m_highlightCount = -1;

		private static void setMessage()
		{
			if(m_message != null && m_message.Length > 0)
			{
				if(m_message.StartsWith("*"))
				{
					m_sb.BackColor = Color.Yellow;
					m_highlightCount = 3;
				}
				else
				{
					m_sb.BackColor = Color.LightGray;
				}
				m_sb.Text = m_message;
			}
			else
			{
				m_sb.BackColor = Color.LightGray;
			}
		}

		private static int m_progress;

		private static void setProgress()
		{
			m_pb.Visible = true;
			m_pb.Value = (m_progress > 100) ? 100 : m_progress;
		}

		private static void setProgressOff()
		{
			m_pb.Visible = false;
			m_progress = 0;
		}

		// thread safe
		public static void Progress(int p)	// p preferably within 0-100
		{
			if(m_progress != p && m_pb != null && m_pb.Created && m_pb.Visible)
			{
				m_progress = p;
				if(m_pb.InvokeRequired)
				{
					m_pb.Invoke(new MethodInvoker(setProgress));
				} 
				else
				{
					setProgress();
				}
			}
		}

		// thread safe
		public static void ProgressOff()
		{
			if(m_pb != null)
			{
				if(m_pb.InvokeRequired)
				{
					m_pb.Invoke(new MethodInvoker(setProgressOff));
				} 
				else
				{
					setProgressOff();
				}
			}
		}

		private static string m_message2 = "";

		private static void setMessage2()
		{
			m_sb2.Text = m_message2;
			m_message2 = "";
		}

		// thread safe
		public static void Trace2(string str)
		{
			m_message2 = str;
		}

		public static void Trace2Sync()
		{
			if(m_message2.Length > 0 && m_sb2 != null && m_sb2.Created && m_sb2.Visible)
			{
				if(m_sb2.InvokeRequired)
				{
					m_sb2.Invoke(new MethodInvoker(setMessage2));
				} 
				else
				{
					setMessage2();
				}
			}
		}

		// called every 400ms from ProgressMonitor
		public static void AllSync()
		{
			TraceSync();
			Trace2Sync();
		}

		public static void showLog()
		{
			ListForm lf = new ListForm();
			string text = "";
			foreach(string str in m_log)
			{
				text += (str + "\r\n");
			}
			lf.setText(text);
			lf.ShowDialog();
		}

		public static void statusBar_Click(object sender, System.EventArgs e)
		{
			showLog();
		}
	}
}
