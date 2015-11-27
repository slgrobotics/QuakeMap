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
using System.Globalization;
using Dpapi;
using Microsoft.Win32;

namespace XmlRpcSmugmugLib
{
	/// <summary>
	/// Summary description for AccountSettings.
	/// </summary>
	public class RegistrySettings
	{
		private RegistrySettings()
		{
		}

		static public DateTime LastUpdateTime
		{
			get
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
				DateTime id = DateTime.MinValue;
				if (regKey != null)
				{
					try
					{
						id = DateTime.Parse(regKey.GetValue("LastUpdateTime", "").ToString());
					}
					catch
					{
						regKey.Close();
					}
				}

				return id;
			}
			set
			{
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(keyName);
				regKey.SetValue("LastUpdateTime", value.ToString(CultureInfo.InvariantCulture));
				regKey.Close();
			}
		}

		static public SmugMugApi.AccountType AccountSubscription
		{
			get
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
				string id = "";
				if (regKey != null)
				{
					id = regKey.GetValue("AccountType", "").ToString();
					regKey.Close();
				}

				if (id == "")
					return SmugMugApi.AccountType.Standard;
				else
					return (SmugMugApi.AccountType)Enum.Parse(typeof(SmugMugApi.AccountType), id);
			}
			set
			{
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(keyName);
				regKey.SetValue("AccountType", value.ToString());
				regKey.Close();
			}
		}

		static public string Username
		{
			get
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
				string id = "";
				if (regKey != null)
				{
					id = regKey.GetValue("E-mail", "").ToString();
					regKey.Close();
				}
				return id;
			}
			set
			{
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(keyName);
				regKey.SetValue("E-mail", value);
				regKey.Close();
			}
		}

		static public string AccountName
		{
			get
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
				string id = "";
				if (regKey != null)
				{
					id = regKey.GetValue("AccountName", "").ToString();
					regKey.Close();
				}
				return id;
			}
			set
			{
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(keyName);
				regKey.SetValue("AccountName", value);
				regKey.Close();
			}
		}

		static public string Password
		{
			get
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
				string id = "";
				if (regKey != null)
				{
					id = regKey.GetValue("Password", "").ToString();
					regKey.Close();

					// DPAPI isn't supported on Windows 2000
					if (Environment.OSVersion.Version.Major >= 5 && Environment.OSVersion.Version.Minor > 0)
					{
						try
						{
							Dpapi.DataProtector dp = new DataProtector(Dpapi.Store.UserStore);
							byte[] sourceBytes = Convert.FromBase64String(id);
							byte[] decryptedBytes = dp.Decrypt(sourceBytes);
							id = System.Text.Encoding.Unicode.GetString(decryptedBytes);
						}
						catch
						{
							id = "";
						}
					}
				}
				return id;
			}
			set
			{
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(keyName);

				if (Environment.OSVersion.Version.Major >= 5 && Environment.OSVersion.Version.Minor > 0)
				{
					try
					{
						Dpapi.DataProtector dp = new DataProtector(Dpapi.Store.UserStore);
						byte[] sourceBytes = System.Text.Encoding.Unicode.GetBytes(value);
						byte[] encryptedBytes = dp.Encrypt(sourceBytes);

						regKey.SetValue("Password", Convert.ToBase64String(encryptedBytes));
					}
					catch
					{
						regKey.SetValue("Password", value);
					}
				}
				else
				{
					regKey.SetValue("Password", value);
				}

				regKey.Close();
			}
		}

		static protected string keyName = "Software\\SmugMug";

	}
}