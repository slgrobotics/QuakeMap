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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using CookComputing.XmlRpc;

namespace XmlRpcSmugmugLib
{
	/// <summary>
	/// Summary description for SmugMug.
	/// </summary>
	public class SmugMugApi
	{
		// could be in the Project, but would create unnecessary reference:
		private const string SMUGMUG_API_KEY = "eVtsPOUge0IfMgwQGTbwmkzFD6PzC4yD";
		private const string SMUGMUG_API_NAME = "QuakeMap";
		private const string SMUGMUG_API_DEFAULT_VERSION = "1.1.0";

		public static SmugMugApi SmugMug;

		private string username = String.Empty;
		private string password = String.Empty;
		private bool connected = false;
		private AccountType account = AccountType.Standard;

		private Credentials credentials;
		private static ISmugMug proxy;

		private Category[] categories;
		private SubCategory[] subCategories;
		private Album[] albums;

		/// <summary>
		/// Returns true if a successfull session was established.
		/// </summary>
		public bool Connected
		{
			get { return connected; }
		}

		/// <summary>
		/// The <see cref="AccountType"/> of the user.
		/// </summary>
		public AccountType Account
		{
			get { return account; }
			set { account = value; }
		}

		/// <summary>
		/// Possible values for an <see cref="Account"/>.
		/// </summary>
		public enum AccountType
		{
			Standard = 0,
			PowerUser = 1,
			Pro = 2
		}

		/// <summary>
		/// Initlialized a new instance of the <see cref="SmugMugApi"/>.
		/// You must call <see cref="Login"/> before you can call any
		/// methods.
		/// </summary>
		/// <param name="emailAddress">The email address used to login to SmugMug.</param>
		/// <param name="password">The password used to login to SmugMug.</param>
		public SmugMugApi(string emailAddress, string password)
		{
			this.username = emailAddress;
			this.password = password;
		}

		/// <summary>
		/// Initlialized a new instance of the <see cref="SmugMugApi"/>.
		/// You must call <see cref="LoginAnonymously"/> before you can call any
		/// methods.
		/// </summary>
		public SmugMugApi()
		{
		}

		/// <summary>
		/// Initiates a session to SmugMug looging in the user using the emailAddress
		/// and password supplied in the constructor.
		/// </summary>
		/// <returns>True if the login was successfull.</returns>
		public bool Login()
		{
			if (this.connected == false | proxy == null && this.credentials.UserID == 0)
			{
				try
				{
					proxy = (ISmugMug)XmlRpcProxyGen.Create(typeof(ISmugMug));
					this.credentials = proxy.LoginWithPassword(username, password, SMUGMUG_API_DEFAULT_VERSION, SMUGMUG_API_KEY);
					this.connected = true;
				}
				catch
				{
					return false;
				}
			}
			else
			{
				LoginWithHash();
			}

			return true;
		}

		private void LoginWithHash()
		{
			try
			{
				LoginResult result = proxy.LoginWithHash(this.credentials.UserID.ToString(), this.credentials.PasswordHash, SMUGMUG_API_DEFAULT_VERSION, SMUGMUG_API_KEY);
				
				if (result.SessionID != null && result.SessionID.Length > 0)
				{
					this.credentials.SessionID = result.SessionID;
				}
				else
				{
					throw new SmugMugException("SessionID was empty");
				}
			}
			catch (Exception ex)
			{
				throw new SmugMugException("A login error occured, SessionID may be invalid.", ex.InnerException);
			}
		}

		public void LoginAnonymously()
		{
			try
			{
				proxy = (ISmugMug)XmlRpcProxyGen.Create(typeof(ISmugMug));
				LoginResult result = proxy.LoginAnonymously(SMUGMUG_API_DEFAULT_VERSION, SMUGMUG_API_KEY);
				
				if (result.SessionID != null && result.SessionID.Length > 0)
				{
					this.credentials.SessionID = result.SessionID;
					this.connected = true;
				}
				else
				{
					throw new SmugMugException("SessionID was empty");
				}
			}
			catch (Exception ex)
			{
				throw new SmugMugException("A login error occured, SessionID may be invalid.", ex.InnerException);
			}
		}

		/// <summary>
		/// Disconnects the existing Session from SmugMug, and invalidates the Session.
		/// </summary>
		public void Logout()
		{
			try
			{
				proxy.Logout(this.credentials.SessionID);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Gets all the Categories for the user.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="Category"/>.</returns>
		/// <remarks>Categories are cached for the existing Session.
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the Categories.</remarks>
		public Category[] GetCategories()
		{
			if (this.categories == null)
			{
				try
				{
					this.categories =  proxy.GetCategories(credentials.SessionID);
					this.connected = false;
				}
				catch (Exception ex)
				{
					throw new SmugMugException("Could not retrieve Categories", ex.InnerException);
				}
			}

			return this.categories;
		}

		/// <summary>
		/// Gets all the SubCategories for a given <see cref="Category"/>.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="SubCategory"/>.
		/// If there are no SubCategories then it returns null.</returns>
		/// <remarks>SubCategories are cached for the existing Session.
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the SubCategories.</remarks>
		public SubCategory[] GetSubCategories(int categoryID)
		{
			try
			{
				SubCategory[] subCategory =  proxy.GetSubCategories(this.credentials.SessionID, categoryID);
				return subCategory;
			}
			catch
			{
				return new SubCategory[0];
				//throw new SmugMugException("Could not retrieve SubCategories for CategoryID: " + categoryID, ex.InnerException);
			}
		}

		/// <summary>
		/// Gets all the SubCategories for the user.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="SubCategory"/>.
		/// If there are no SubCategories then it returns null.</returns>
		/// <remarks>SubCategories are cached for the existing Session.
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the SubCategories.</remarks>
		public SubCategory[] GetAllSubCategories()
		{
			if (this.categories == null)
			{
				try
				{
					this.subCategories =  proxy.GetAllSubCategories(this.credentials.SessionID);
					return this.subCategories;
					
				}
				catch (Exception ex)
				{
					throw new SmugMugException("Could not retrieve all SubCategories", ex.InnerException);
				}
			}
			else
			{
				return this.subCategories;
			}
		}

		/// <summary>
		/// Gets all the Albums for the user.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="Album"/>.
		/// <remarks>Albums are cached for the existing Session. 
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the Albums.</remarks>
		public Album[] GetAlbums()
		{
			if (this.albums == null)
			{
				try
				{
					this.albums =  proxy.GetAlbums(this.credentials.SessionID);
					return this.albums;
					
				}
				catch (Exception ex)
				{
					throw new SmugMugException("Could not retrieve Albums", ex.InnerException);
				}
			}
			else
			{
				return this.albums;
			}
		}

		/// <summary>
		/// This method does not seem to currently work.
		/// </summary>
		/// <param name="albumID">The albumID to retreive the AlbumInfo.</param>
		/// <returns></returns>
		public Album GetAlbumInfo(int albumID)
		{
			try
			{
				Album album =  proxy.GetAlbumInfo(this.credentials.SessionID, albumID);
				return album;
			}
			catch (Exception ex)
			{
				throw new SmugMugException("Could not retrieve Album info", ex.InnerException);
			}
		}

		/// <summary>
		/// Creates a new Album.
		/// </summary>
		/// <param name="album">The Album to create</param>
		/// <returns>The AlbumID of the new Album.
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to create the Album.</remarks>
		public int CreateAlbum(Album album)
		{
			if (album.Title == null || album.Title.Length == 0)
				throw new ArgumentException("Title must not be empty or null");

			try
			{
				XmlRpcStruct albumStruct = new XmlRpcStruct();
				if (album.SubcategoryID != 0)
					albumStruct.Add("SubCategoryID", album.SubcategoryID);
				if (album.CommunityID != 0)
					albumStruct.Add("CommunityID", album.CommunityID);
				if (album.Description != null && album.Description.Length != 0)
					albumStruct.Add("Description", album.Description);
				if (album.Keywords != null && album.Keywords.Length != 0)
					albumStruct.Add("Keywords", album.Keywords);
				if (album.Password != null && album.Password.Length != 0)
					albumStruct.Add("Password", album.Password);
				if (album.Position != 0 && album.Position != 1)
					albumStruct.Add("Position", album.Position);
				albumStruct.Add("SortMethod", Enum.GetName(typeof(Album.SortMethodType), album.SortMethod));
				if (album.Public == false)
					albumStruct.Add("Public", album.Public);
				albumStruct.Add("Filenames", album.Filenames);
				if (album.Comments == false)
					albumStruct.Add("Comments", album.Comments);
				if (album.External == false)
					albumStruct.Add("External", album.External);
				if (album.EXIF == false)
					albumStruct.Add("EXIF", album.EXIF);
				if (album.Share == false)
					albumStruct.Add("Share", album.Share);
				if (album.Printable == false)
					albumStruct.Add("Printable", album.Printable);
				if (album.Originals == false)
					albumStruct.Add("Originals", album.Originals);
				if (Account != AccountType.Standard) albumStruct.Add("Header", album.Header);
				if (Account != AccountType.Pro) albumStruct.Add("TemplateID", (int)album.TemplateID);
				if (Account != AccountType.Pro && album.Larges == false)
					albumStruct.Add("Larges", album.Larges);
				if (Account != AccountType.Pro) albumStruct.Add("Clean", album.Clean);
				if (Account != AccountType.Pro) albumStruct.Add("Protected", album.Protected);
				if (Account != AccountType.Pro) albumStruct.Add("Watermark", album.Watermark);

				int albumID =  proxy.CreateAlbum(this.credentials.SessionID, album.Title, album.CategoryID, albumStruct);

				// reload the albums
				this.albums =  proxy.GetAlbums(this.credentials.SessionID);

				return albumID;
			}
			catch (Exception ex)
			{
				throw new SmugMugException("An error occured trying to create a new Album: " + album.Title, ex.InnerException);
			}
		}

		/// <summary>
		/// Not implimented.
		/// </summary>
		public void ChangeAlbumSettings()
		{
		}

		/// <summary>
		/// Not implimented.
		/// </summary>
		public static void ReSortAlbum()
		{
		}

		/// <summary>
		/// This method does not seem to currently work. (works fine -- sergei)
		/// </summary>
		/// <param name="albumID">The albumID to retreive the AlbumInfo.</param>
		/// <returns></returns>
		public int[] GetImages(int albumID)
		{
			try
			{
				int[] images =  proxy.GetImages(this.credentials.SessionID, albumID);
				return images;
			}
			catch (Exception ex)
			{
				throw new SmugMugException("Could not get images", ex.InnerException);
			}
		}

		/// <summary>
		/// returns image URLs.
		/// </summary>
		/// <param name="imageId">The imageId to retreive the image URLs.</param>
		/// <returns></returns>
		public XmlRpcStruct GetImageURLs(int imageId)
		{
			try
			{
				XmlRpcStruct imageUrls =  proxy.GetImageURLs(this.credentials.SessionID, imageId);
				return imageUrls;
			}
			catch (Exception ex)
			{
				throw new SmugMugException("Could not get image URLs", ex.InnerException);
			}
		}

		/// <summary>
		/// Not implimented.
		/// </summary>
		public XmlRpcStruct GetImageExif(int imageId)
		{
			try
			{
				XmlRpcStruct imageExif =  proxy.GetImageEXIF(this.credentials.SessionID, imageId);
				return imageExif;
			}
			catch (Exception ex)
			{
				//throw new SmugMugException("Could not get image EXIF", ex.InnerException);
				return new XmlRpcStruct();
			}
		}

		/// <summary>
		/// Not implimented.
		/// </summary>
		public void ChangeImageSettings()
		{
		}

		/// <summary>
		/// Not implimented.
		/// </summary>
		public void ChangeImagePosition()
		{
		}

		/// <summary>
		/// Uploads the Image to SmugMug using HTTP Post.
		/// </summary>
		/// <param name="path">The full path to the image on disk.</param>
		/// <param name="AlbumID">The AlbumID for the Image to be added.</param>
		/// <returns>Throws an <see cref="SmugMugUploadException"/>
		/// if an error occurs trying to upload the Image.</remarks>
		public int Upload(string path, int AlbumID)
		{
			FileInfo file = new FileInfo(path);
			
			if (file.Exists == false)
				throw new ArgumentException("Image does not exist: " + file.FullName);

//			int byteCount = Convert.ToInt32(file.Length);
//			string md5sum = null;
//
//			using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
//			{
//
//				MD5 md5 = new MD5CryptoServiceProvider();
//				byte[] hash = md5.ComputeHash(fs);
//
//				StringBuilder buff = new StringBuilder();
//				foreach (byte hashByte in hash)
//				{
//					buff.Append(String.Format("{0:X1}", hashByte));
//				}
//				md5sum = buff.ToString();
//			}

			try
			{
				WebClient client = new WebClient ();
				client.BaseAddress = "http://upload.smugmug.com";
				client.Headers.Add("Cookie:SMSESS=" + this.credentials.SessionID);
		
				NameValueCollection queryStringCollection = new NameValueCollection();
				queryStringCollection.Add("AlbumID", AlbumID.ToString());
				queryStringCollection.Add("Version", SMUGMUG_API_DEFAULT_VERSION);
//				queryStringCollection.Add("APIKey", SMUGMUG_API_KEY);
//				queryStringCollection.Add("ByteCount", byteCount.ToString());
//				queryStringCollection.Add("MD5Sum", md5sum);
				client.QueryString = queryStringCollection;

				byte[] responseArray = client.UploadFile("http://upload.smugmug.com/photos/xmladd.mg","POST",file.FullName);
				string response = Encoding.ASCII.GetString(responseArray);
		
				XmlRpcSerializer ser = new XmlRpcSerializer();
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(response);
				XmlRpcResponse rpcResponse = ser.DeserializeResponse(doc, typeof(XmlRpcInt));
				XmlRpcInt imageID = (XmlRpcInt)rpcResponse.retVal;
				return (int)imageID;
			}
			catch (Exception ex)
			{
				throw new SmugMugUploadException("Error uploading image: " + file.FullName, ex.InnerException);
			}
		}
	}
}
