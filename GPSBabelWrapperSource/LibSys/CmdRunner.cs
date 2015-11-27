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

//**************************************************************
// Inspired by GnuPG wrapper by Emmanuel KARTMANN 2002 (emmanuel@kartmann.org)
// (see freeware section at kartmann.org  article at http://www.codeproject.com/csharp/gnupgdotnet.asp )
//**************************************************************

using System;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Threading;

namespace LibSys
{

	/// <summary>
	/// Specific exception thrown whenever an error occurs.
	/// </summary>
	public class CmdRunnerException: Exception
	{
		public CmdRunnerException(string message): base(message)
		{
		}
	}

	/// <summary>
	/// CmdRunner runs a DOS style command, making output available.
	/// 
	/// It executes the command line program (gpsbabel.exe) in an different process, redirects standard input (stdin),
	/// standard output (stdout) and standard error (stderr) streams, and monitors the 
	/// streams to fetch the results of the operation.<p/>
	/// </summary>
	public class CmdRunner
	{
		private int _ProcessTimeOutMilliseconds = Int32.MaxValue; // 10 seconds
		private int _exitcode = 0;

		// Variables used for monitoring external process and threads
		private Process _processObject;

		private string _outputString;
		public string OutputString { get { return _outputString; } }

		private string _errorString;
		public string ErrorString { get { return _errorString; } }

		public CmdRunner()
		{
		}

		/// <summary>
		/// Exit code from the executed process (0 = success; otherwise an error occured)
		/// </summary>
		public int exitcode
		{
			get
			{
				return(_exitcode);
			}
		}

		/// <summary>
		/// Timeout for CmdRunner process, in milliseconds.
		/// 
		/// <p/>If the process doesn't exit before the end of the timeout period, the process is terminated (killed).
		/// 
		/// <p/>Defaults to 10000 (10 seconds).
		/// </summary>
		public int ProcessTimeOutMilliseconds
		{
			get
			{
				return(_ProcessTimeOutMilliseconds);
			}
			set
			{
				_ProcessTimeOutMilliseconds = value;
			}
		}

		/// <summary>
		/// Execute a DOS command.
		/// 
		/// <p/>Raise a CmdRunnerException whenever an error occurs.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="args"></param>
		/// <param name="workingDirectory"></param>
		/// <param name="inputText"></param>
		/// <param name="outputText"></param>
		public void executeCommand(string cmd, string args, string workingDirectory, string inputText, out string outputText)
		{
			outputText = "";

			LibSys.StatusBar.Trace(cmd + " " + args);
			// Create startinfo object
			ProcessStartInfo pInfo = new ProcessStartInfo(cmd, args);
			pInfo.WorkingDirectory = workingDirectory;
			pInfo.CreateNoWindow = true;
			pInfo.UseShellExecute = false;
			// Redirect everything: 
			// stdin to send interactive input, stdout to get output messages, stderr in case of errors...
			pInfo.RedirectStandardInput = true;
			pInfo.RedirectStandardOutput = true;
			pInfo.RedirectStandardError = true;
			_processObject = Process.Start(pInfo);
//			_processObject.StandardInput.AutoFlush = true;
//			_processObject.StandardInput.BaseStream.SetLength(inputTextOrFilename.Length+1);

			// Send input, if any
			if (inputText != null && inputText != "")
			{
				_processObject.StandardInput.WriteLine(inputText);
				_processObject.StandardInput.Flush();
			}

			_processObject.StandardInput.Close();

			_outputString = "";
			_errorString = "";

			// Create two threads to read both output/error streams without creating a deadlock
			ThreadStart outputEntry = new ThreadStart(StandardOutputReader);
			Thread outputThread = new Thread(outputEntry);
			outputThread.Start();

			ThreadStart errorEntry = new ThreadStart(StandardErrorReader);
			Thread errorThread = new Thread(errorEntry);
			errorThread.Start();

			if (_processObject.WaitForExit(ProcessTimeOutMilliseconds))
			{
				// Process exited before timeout...
				// Wait for the threads to complete reading output/error (but use a timeout!)
				if (!outputThread.Join(ProcessTimeOutMilliseconds/2))
				{
					outputThread.Abort();
				}
				if (!errorThread.Join(ProcessTimeOutMilliseconds/2))
				{
					errorThread.Abort();
				}
			}
			else
			{
				// Process timeout: the command hung somewhere... kill it (as well as the threads!)
				_outputString = "";
				_errorString = "Timed out after " + ProcessTimeOutMilliseconds.ToString() + " milliseconds";
				_processObject.Kill();
				if (outputThread.IsAlive)
				{
					outputThread.Abort();
				}
				if (errorThread.IsAlive)
				{
					errorThread.Abort();
				}
			}

			// Check results and prepare output
			_exitcode = _processObject.ExitCode;
			if (_exitcode == 0)
			{
				outputText = _outputString;
			}
			else
			{
				if (_errorString == "")
				{
					_errorString = "CmdRunner: exit code " + _processObject.ExitCode.ToString() + ": Unknown error";
				}
				throw new CmdRunnerException(_errorString);
			}
		}

		/// <summary>
		/// Reader thread for standard output
		/// 
		/// <p/>Updates the private variable _outputString (locks it first)
		/// </summary>
		public void StandardOutputReader()
		{
			string output = _processObject.StandardOutput.ReadToEnd();
			lock(this)
			{
				_outputString = output;
			}
		}

		/// <summary>
		/// Reader thread for standard error
		/// 
		/// <p/>Updates the private variable _errorString (locks it first)
		/// </summary>
		public void StandardErrorReader()
		{
			string error = _processObject.StandardError.ReadToEnd();
			lock(this)
			{
				_errorString = error;
			}
		}
	}
}
