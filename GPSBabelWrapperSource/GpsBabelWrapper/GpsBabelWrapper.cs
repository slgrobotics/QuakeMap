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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Diagnostics;

using LibSys;
using Wrapper;

namespace GpsBabelWrapper
{
	/// <summary>
	/// Summary description for GpsBabelWrapper.
	/// </summary>
	public class GpsBabelWrapper : System.Windows.Forms.Form
	{
		[STAThread]
		public static void Main(string[] args)
        {
            m_args = args;

            try
            {
                // used to be static constructors; moved here to provide better diagnostics on failure:
                new LibSys.Project();
                new LibSys.StatusBar();	// writes first line in trace file
                new ThreadPool2();
            }
            catch (Exception e)
            {
                string message = "EntryForm:Main at start: " + e;
                System.Windows.Forms.MessageBox.Show(null, message, Project.PROGRAM_NAME_HUMAN,
                    System.Windows.Forms.MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            try
            {
                Thread.CurrentThread.Name = Project.PROGRAM_NAME_HUMAN + " MainForm";
                LibSys.StatusBar.WriteLine("Framework: " + Environment.Version);
                LibSys.StatusBar.WriteLine("Application in: " + Application.StartupPath);
                LibSys.StatusBar.WriteLine("    Executable: " + Application.ExecutablePath);
                // the following fails after obfuscating, packing and installing:
                //LibSys.StatusBar.WriteLine("       Data in: " + Application.CommonAppDataPath);
                //LibSys.StatusBar.WriteLine("     User path: " + Application.LocalUserAppDataPath);
            }
            catch (Exception e)
            {
                LibSys.StatusBar.WriteLine("Interrupted Main at start: " + e);
            }

            if (CommuninicateToOtherInstance())	// safe to proceed?
            {
                LibSys.StatusBar.WriteLine("Safe to proceed " + DateTime.Now);

                GpsBabelWrapperForm mainForm = new GpsBabelWrapperForm(args);

            again:
                try
                {
                    if (!Project.goingDown)
                    {
                        Application.Run(mainForm);
                    }
                }
                catch (Exception e)
                {
                    LibSys.StatusBar.WriteLine("Interrupted Main: " + e);
#if DEBUG
                    Project.ErrorBox(null, "Interrupted Main: " + e);
#endif
                    goto again;
                }
                catch
                {
                    LibSys.StatusBar.WriteLine("Interrupted Main");
#if DEBUG
                    Project.ErrorBox(null, "Interrupted Main");
#endif
                    goto again;
                }
            }
            LibSys.StatusBar.WriteLine("Quitting in EntryForm.Main " + DateTime.Now);

            Project.Closing();

            LibSys.StatusBar.WriteLine("Exit thread '" + Thread.CurrentThread.Name + "' in EntryForm.Main " + DateTime.Now);
            Application.Exit();
            Application.ExitThread();
            Environment.Exit(0);
        }

        public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LibSys.StatusBar.Error("Application ThreadException " + (e == null ? "" : ("" + e.Exception)));
#if DEBUG
            Project.ErrorBox(null, "Application ThreadException " + (e == null ? "" : ("" + e.Exception)));
#endif
        }

        #region CommuninicateToOtherInstance

        private static string[] m_args = null;
        private static Thread m_discoveryThread = null;
        private static int m_otherProcessesNumber = -1;

        /// <summary>
        /// this method tried to detect/talk to other instance of the app, ensuring that dangerous Process calls don't hang the current instance
        /// returns true if it is safe to proceed, false if there is another process and this instance may need to quit.
        /// </summary>
        /// <param name="args"></param>
        private static bool CommuninicateToOtherInstance()
        {
            // here is a workaround for the folks who have Framework hanging when Process.GetCurrentProcess() is called
            if (File.Exists(Path.Combine(Application.StartupPath, "nomess.txt")))
            {
                LibSys.StatusBar.WriteLine("OK: skipping Other instance discovery thread");
                return true;
            }

            m_otherProcessesNumber = -1;

            m_discoveryThread = new Thread(new ThreadStart(tryDiscoverOtherInstance));
            m_discoveryThread.IsBackground = true;	// terminate with the main process
            m_discoveryThread.Name = "Discovery";
            m_discoveryThread.Priority = ThreadPriority.BelowNormal;
            m_discoveryThread.Start();

            // now we either timeout in 4 seconds, or the discovery will deliver m_otherProcessesNumber >= 0
            int i = 0;
            for (; i < 10; i++)
            {
                Thread.Sleep(400);		// let the thread try discover/communicate to other instance
                if (m_otherProcessesNumber >= 0)
                {
                    break;
                }
            }

            if (m_otherProcessesNumber == -1)
            {
                LibSys.StatusBar.WriteLine("Error: Other instance discovery thread hung");
            }

            try
            {
                i = 0;
                while (m_discoveryThread.IsAlive && i++ < 10)
                {
                    LibSys.StatusBar.WriteLine("IP: aborting discovery thread");
                    m_discoveryThread.Abort();
                    Thread.Sleep(200);		// let the thread try discover/communicate to other instance
                    LibSys.StatusBar.WriteLine("IP: after aborting discovery thread, isAlive=" + m_discoveryThread.IsAlive);
                }
            }
            catch (Exception e)
            {
                LibSys.StatusBar.WriteLine("Error: while aborting discovery thread: " + e.Message);
            }

            m_discoveryThread = null;
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            if (m_args.Length >= 1 && m_otherProcessesNumber > 0 && otherProcessWindowHandle != IntPtr.Zero)
            {
                // we have other process running, send a message:
                LibSys.StatusBar.WriteLine("Sending message to handle=" + otherProcessWindowHandle);
                try
                {
                    //Project.SendMessage(otherProcessWindowHandle,UM_MYMESSAGE,IntPtr.Zero,IntPtr.Zero);
                    string strArgs = "";
                    foreach (string arg in m_args)
                    {
                        strArgs += arg + "|";
                    }
                    if (strArgs.EndsWith("|"))
                    {
                        strArgs = strArgs.Substring(0, strArgs.Length - 1);
                    }
                    LibSys.StatusBar.WriteLine("Sending Windows message '" + strArgs + "'");
                    Project.SendMsgString(otherProcessWindowHandle, strArgs);
                }
                catch (Exception e)
                {
                    LibSys.StatusBar.WriteLine("Error: while sending message: " + e);
                }
            }

            return m_otherProcessesNumber == -1 || m_otherProcessesNumber == 0;	// safe to proceed if true, quit if false
        }

        //static int UM_MYMESSAGE = Project.RegisterWindowMessage("UM_MYMESSAGE");
        static IntPtr otherProcessWindowHandle = IntPtr.Zero;

        /// <summary>
        /// The Process related calls tend to hang, so we try to play it safely in a separate thread:
        /// </summary>
        private static void tryDiscoverOtherInstance()
        {
            Process[] runningProcesses = null;
            Process otherProcess = null;
            string processName = "";
            int otherProcessesNumber = 0;

            try
            {
                // see discussion here: Hyperthreading http://www.dotnet247.com/247reference/msgs/35/178985.aspx
                // (related to GetProcessesByName() can't get process information from remote machine)

                LibSys.StatusBar.WriteLine("Other instance discovery thread");
                Process currentProcess = Process.GetCurrentProcess();
                processName = currentProcess.ProcessName;
                runningProcesses = Process.GetProcessesByName(processName);
                otherProcessesNumber = 0;

                // process: QuakeMap   title: 'QuakeMap - [offline]   4m/pixel * 1.1875'  
                // process: quakemap   title: 'QuakeMap Installation'   - not counted
                // process: QuakeMap   title: ''  

                foreach (Process process in runningProcesses)
                {
                    string isCounted = " - not counted";
                    // don't count installation in process, which happens to be quakemap.exe too
                    if (!process.Id.Equals(currentProcess.Id) && processName.Equals(process.ProcessName) && !("" + process.MainWindowTitle).EndsWith(" Installation"))
                    {
                        otherProcess = process;
                        otherProcessesNumber++;
                        isCounted = "";
                        otherProcessWindowHandle = process.MainWindowHandle;
                    }
                    LibSys.StatusBar.WriteLine("process: " + process.ProcessName + "   title: '" + process.MainWindowTitle + "'  " + isCounted);
                }

                if (otherProcessesNumber > 0)
                {
                    LibSys.StatusBar.WriteLine("Another process with name '" + otherProcess.ProcessName + "'  id=" + otherProcess.Id + " is running.");

                    // this is the place to pass name of the file clicked while the app is already running to the running instance.
                }

                m_otherProcessesNumber = otherProcessesNumber;
                LibSys.StatusBar.WriteLine("Other instance discovery thread delivered " + otherProcessesNumber);
            }
            catch (Exception e)
            {
                // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PerfProc\Performance appeared key "Disable Performance Counters"=dword:00000001
                //  Set it to 0 and all worked. -- from http://www.gotdotnet.ru/Forums/CommonForum/23263.aspx
                LibSys.StatusBar.WriteLine("Error: exception while in tryDiscoverOtherInstance(): " + e.Message);
            }
        }
        #endregion

	}
}
