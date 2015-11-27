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
using System.Diagnostics;
using System.Windows.Forms;

namespace XmlRpcSmugmugLib
{
	/// <summary>
	/// Summary description for Login.
	/// </summary>
	public class Login : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label labelLogin;
		private System.Windows.Forms.TextBox textBoxLogin;
		private System.Windows.Forms.Label labelPassword;
		private System.Windows.Forms.TextBox textBoxPassword;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.CheckBox checkBoxSignInAutomatically;
		private System.Windows.Forms.LinkLabel linkLabelSignup;
		private System.Windows.Forms.Label labelAccount;
		private System.Windows.Forms.TextBox textBoxAccount;
		private System.Windows.Forms.Label labelSmugMugCom;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Login()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.labelLogin = new System.Windows.Forms.Label();
			this.textBoxLogin = new System.Windows.Forms.TextBox();
			this.labelPassword = new System.Windows.Forms.Label();
			this.textBoxPassword = new System.Windows.Forms.TextBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.checkBoxSignInAutomatically = new System.Windows.Forms.CheckBox();
			this.linkLabelSignup = new System.Windows.Forms.LinkLabel();
			this.labelAccount = new System.Windows.Forms.Label();
			this.textBoxAccount = new System.Windows.Forms.TextBox();
			this.labelSmugMugCom = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// labelLogin
			// 
			this.labelLogin.Location = new System.Drawing.Point(40, 8);
			this.labelLogin.Name = "labelLogin";
			this.labelLogin.Size = new System.Drawing.Size(64, 21);
			this.labelLogin.TabIndex = 0;
			this.labelLogin.Text = "E-mail:";
			this.labelLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxLogin
			// 
			this.textBoxLogin.Location = new System.Drawing.Point(112, 8);
			this.textBoxLogin.Name = "textBoxLogin";
			this.textBoxLogin.Size = new System.Drawing.Size(184, 21);
			this.textBoxLogin.TabIndex = 1;
			this.textBoxLogin.Text = "";
			// 
			// labelPassword
			// 
			this.labelPassword.Location = new System.Drawing.Point(40, 32);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(64, 21);
			this.labelPassword.TabIndex = 0;
			this.labelPassword.Text = "Password:";
			this.labelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Location = new System.Drawing.Point(112, 32);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.PasswordChar = '*';
			this.textBoxPassword.Size = new System.Drawing.Size(184, 21);
			this.textBoxPassword.TabIndex = 1;
			this.textBoxPassword.Text = "";
			// 
			// buttonOK
			// 
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonOK.Location = new System.Drawing.Point(136, 112);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonCancel.Location = new System.Drawing.Point(216, 112);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			// 
			// checkBoxSignInAutomatically
			// 
			this.checkBoxSignInAutomatically.Location = new System.Drawing.Point(112, 80);
			this.checkBoxSignInAutomatically.Name = "checkBoxSignInAutomatically";
			this.checkBoxSignInAutomatically.Size = new System.Drawing.Size(152, 24);
			this.checkBoxSignInAutomatically.TabIndex = 3;
			this.checkBoxSignInAutomatically.Text = "Sign-in Automatically";
			// 
			// linkLabelSignup
			// 
			this.linkLabelSignup.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkLabelSignup.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.linkLabelSignup.Location = new System.Drawing.Point(8, 112);
			this.linkLabelSignup.Name = "linkLabelSignup";
			this.linkLabelSignup.TabIndex = 4;
			this.linkLabelSignup.TabStop = true;
			this.linkLabelSignup.Text = "Sign up...";
			this.linkLabelSignup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.linkLabelSignup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSignup_LinkClicked);
			// 
			// labelAccount
			// 
			this.labelAccount.Location = new System.Drawing.Point(32, 56);
			this.labelAccount.Name = "labelAccount";
			this.labelAccount.Size = new System.Drawing.Size(64, 21);
			this.labelAccount.TabIndex = 0;
			this.labelAccount.Text = "Account:";
			this.labelAccount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxAccount
			// 
			this.textBoxAccount.Location = new System.Drawing.Point(112, 56);
			this.textBoxAccount.Name = "textBoxAccount";
			this.textBoxAccount.Size = new System.Drawing.Size(72, 21);
			this.textBoxAccount.TabIndex = 1;
			this.textBoxAccount.Text = "";
			// 
			// labelSmugMugCom
			// 
			this.labelSmugMugCom.Location = new System.Drawing.Point(192, 56);
			this.labelSmugMugCom.Name = "labelSmugMugCom";
			this.labelSmugMugCom.Size = new System.Drawing.Size(104, 21);
			this.labelSmugMugCom.TabIndex = 0;
			this.labelSmugMugCom.Text = ".smugmug.com";
			this.labelSmugMugCom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Login
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(306, 144);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.linkLabelSignup,
																		  this.checkBoxSignInAutomatically,
																		  this.buttonOK,
																		  this.textBoxLogin,
																		  this.textBoxPassword,
																		  this.labelLogin,
																		  this.labelPassword,
																		  this.buttonCancel,
																		  this.labelAccount,
																		  this.textBoxAccount,
																		  this.labelSmugMugCom});
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Login";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Login";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Login_Closing);
			this.Load += new System.EventHandler(this.Login_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonOK_Click(object sender, System.EventArgs e)
		{
			if (this.textBoxLogin.Text != String.Empty && this.textBoxPassword.Text != String.Empty)
			{
				SmugMugApi.SmugMug = new SmugMugApi(this.textBoxLogin.Text, this.textBoxPassword.Text);
				
				bool connected = SmugMugApi.SmugMug.Login();

				if (connected)
				{
					RegistrySettings.Username = this.textBoxLogin.Text;
					RegistrySettings.AccountName = this.textBoxAccount.Text;

					if (this.checkBoxSignInAutomatically.Checked)
					{
						RegistrySettings.Password = this.textBoxPassword.Text;
					}

					this.Close();
				}
				else
				{
					MessageBox.Show("Your username or password was incorrect",
						"Wrong username or password",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
				}
			}
			else
			{
				MessageBox.Show("You must enter a valid username and password",
					"Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void Login_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			
		}

		private void Login_Load(object sender, System.EventArgs e)
		{
			this.textBoxLogin.Text = RegistrySettings.Username;
			this.textBoxAccount.Text = RegistrySettings.AccountName;
			if (RegistrySettings.Password.Length > 0)
			{
				this.checkBoxSignInAutomatically.Checked = true;
				this.textBoxPassword.Text = RegistrySettings.Password;
			}
		}

		private void linkLabelSignup_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.smugmug.com/?referrer=hDBXAc8lccGdQ");
		}
	}
}
