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

using CookComputing.XmlRpc;

// Keep in mind that this file cannot be obfuscated
// and is compiled into a separate library - libsmugmug.dll

namespace XmlRpcSmugmugLib
{
	public struct Credentials
	{
		private string sessionID;
		public string SessionID
		{
			get { return sessionID; }
			set { sessionID = value; }
		}

		private int userID;
		public int UserID
		{
			get { return userID; }
			set { userID = value; }
		}

		private string passwordHash;
		public string PasswordHash
		{
			get { return passwordHash; }
			set { passwordHash = value; }
		}
	}

	public struct LoginResult
	{
		private string sessionID;
		public string SessionID
		{
			get { return sessionID; }
			set { sessionID = value; }
		}
	}

	public struct Category
	{
		private int categoryID;
		public int CategoryID
		{
			get { return categoryID; }
			set { categoryID = value; }
		}

		private string title;
		public string Title
		{
			get { return title; }
			set { title = value; }
		}
	}

	public struct SubCategory
	{
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public int CategoryID;

		private int subCategoryID;
		public int SubCategoryID
		{
			get { return subCategoryID; }
			set { subCategoryID = value; }
		}

		private string title;
		public string Title
		{
			get { return title; }
			set { title = value; }
		}
	}

	public struct Album
	{
		/// <summary>
		/// Force a display style.
		/// </summary>
		/// <remarks>Power and Pro only.</remarks>
		public enum TemplateType
		{
			ViewerChoice = 0,
			Elegant = 3,
			Traditional = 4,
			AllThumbs = 7,
			Slideshow = 8,
			Journal = 9
		}

		/// <summary>
		/// The method by which to sort the photos when displaying them. The default is <see cref="Position"/>
		/// </summary>
		public enum SortMethodType
		{
			/// <summary>
			/// Sorts by user-specified position.
			/// </summary>
			[XmlRpcMember("Position")]
			Position,
			/// <summary>
			/// Sorts by the image captions.
			/// </summary>
			[XmlRpcMember("Caption")]
			Caption,
			/// <summary>
			/// Sorts by the filename of each photo.
			/// </summary>
			[XmlRpcMember("FileName")]
			FileName,
			/// <summary>
			/// Sorts by the date uploaded to smugmug.
			/// </summary>
			[XmlRpcMember("Date")]
			Date,
			/// <summary>
			/// Sorts by the date last modified, as told by EXIF data. Many files don't have this field correctly set.
			/// </summary>
			[XmlRpcMember("DateTime")]
			DateTime,
			/// <summary>
			/// Sorts by the date taken, as told by EXIF data. Many cameras don't report this properly.
			/// </summary>
			[XmlRpcMember("DateTimeOriginal")]
			DateTimeOriginal
		}

		private int albumID;
		/// <summary>
		/// The ID of the Album.
		/// </summary>
		public int AlbumID
		{
			get { return albumID; }
			set { albumID = value; }
		}

		private string title;
		/// <summary>
		/// The Title of the Album.
		/// </summary>
		/// <remarks>This cannot be null or empty.</remarks>
		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		/// <summary>
		/// The name of the Category the Album belongs to.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string Category;
		
		/// <summary>
		/// The ID of Category the Album belongs to.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public int CategoryID;
		
		/// <summary>
		/// Does this album belong to a SubCategory? Default is 0.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string SubCategory;
		
		/// <summary>
		/// The ID of SubCategory the Album belongs to.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public int SubcategoryID;
		
		/// <summary>
		/// The album's position within it's Category/SubCategory, starting with 1 at the top. Default is 1.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public int Position;
		
		/// <summary>
		/// The method by which to sort the photos when displaying them. 
		/// Default is <see cref="SortMethodType.Position"/>
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public SortMethodType SortMethod;
		
		/// <summary>
		/// Which direction to order the SortMethod.
		/// Default is false (Ascending).
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool SortDirection;
		
		/// <summary>
		/// The number of images in the album. Only set when 
		/// <see cref="SmugMugApi.GetAlbumInfo"/> is called.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public int ImageCount;
		
		/// <summary>
		/// Text description of the contents of the album.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string Description;
		
		/// <summary>
		/// Any keywords used to search or describe the album.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string Keywords;
		
		/// <summary>
		/// The last modified date of images in the album. Only set when 
		/// <see cref="SmugMugApi.GetAlbumInfo"/> is called.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string LastUpdated;
		
		/// <summary>
		/// Is this album a part of a community? 
		/// Default is 0.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public int CommunityID;
		
		/// <summary>
		/// is this album public?
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Public;
		
		/// <summary>
		/// Optional password to protect this album.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string Password;
		
		/// <summary>
		/// Allow prints to be purchased?
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Printable;
		
		/// <summary>
		/// Allow Original photos to be seen?
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Filenames;
		
		/// <summary>
		/// Allow comments?
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Comments;
		
		/// <summary>
		/// Allow external linking? 
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool External;
		
		/// <summary>
		/// Allow Original photos to be seen? 
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Originals;
		
		/// <summary>
		/// Display extended camera information?
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool EXIF;
		
		/// <summary>
		/// Enable 'easy sharing' button and features?
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Share;

		/// <summary>
		/// Valid for <see cref="SmugMugApi.AccountType.PowerUser"/> 
		/// or <see cref="SmugMugApi.AccountType.Pro"/> only.
		/// Default is false.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Header;
		
		/// <summary>
		/// Valid for <see cref="SmugMugApi.AccountType.PowerUser"/> 
		/// or <see cref="SmugMugApi.AccountType.Pro"/> only.
		/// Default is <see cref="TemplateType.ViewerChoice"/>
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public TemplateType TemplateID;
		
		/// <summary>
		/// Allow Large photos to be seen?
		/// Valid for <see cref="SmugMugApi.AccountType.PowerUser"/> 
		/// or <see cref="SmugMugApi.AccountType.Pro"/> only.
		/// Default is true.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Larges;
		
		/// <summary>
		/// Display album in Clean style? 
		/// Valid for <see cref="SmugMugApi.AccountType.Pro"/> only.
		/// Default is false.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Clean;
		
		/// <summary>
		/// Enable heavy image protection?
		/// Valid for <see cref="SmugMugApi.AccountType.Pro"/> only.
		/// Default is false.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Protected;
		
		/// <summary>
		/// Valid for <see cref="SmugMugApi.AccountType.Pro"/> only.
		/// Default is false.
		/// </summary>
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public bool Watermark;
	}

	[XmlRpcUrl("https://upload.smugmug.com/xmlrpc/")]
	public interface ISmugMug
	{
//		[XmlRpcMethod("loginWithPassword")]
		[XmlRpcMethod("smugmug.login.withPassword")]
		Credentials LoginWithPassword(string emailAddress, string password, string version, string APIKey);
		
//		[XmlRpcMethod("loginWithHash")]
		[XmlRpcMethod("smugmug.login.withHash")]
		LoginResult LoginWithHash(string userID, string passwordHash, string version, string APIKey);

//		[XmlRpcMethod("loginAnonymously")]
		[XmlRpcMethod("smugmug.login.anonymously")]
		LoginResult LoginAnonymously(string version, string APIKey);
//		XmlRpcStruct LoginAnonymously(string version, string APIKey);

//		[XmlRpcMethod("logout")]
		[XmlRpcMethod("smugmug.logout")]
		void Logout(string sessionID);

//		[XmlRpcMethod("getCategories")]
		[XmlRpcMethod("smugmug.categories.get")]
		Category[] GetCategories(string sessionID);

//		[XmlRpcMethod("getSubCategories")]
		[XmlRpcMethod("smugmug.subcategories.get")]
		SubCategory[] GetSubCategories(string sessionID, int categoryID);

//		[XmlRpcMethod("getAllSubCategories")]
		[XmlRpcMethod("smugmug.subcategories.getAll")]
		SubCategory[] GetAllSubCategories(string sessionID);

//		[XmlRpcMethod("getAlbums")]
		[XmlRpcMethod("smugmug.albums.get")]
		Album[] GetAlbums(string sessionID);

//		[XmlRpcMethod("getAlbumInfo")]
		[XmlRpcMethod("smugmug.albums.getInfo")]
		Album GetAlbumInfo(string sessionID, int albumID);

//		[XmlRpcMethod("createAlbum")]
		[XmlRpcMethod("smugmug.albums.create")]
		int CreateAlbum(string sessionID, string title, int CategoryID, XmlRpcStruct album);

//		[XmlRpcMethod("getImages")]
		[XmlRpcMethod("smugmug.images.get")]
		int[] GetImages(string sessionID, int albumID);

//		[XmlRpcMethod("getImageURLs")]
		[XmlRpcMethod("smugmug.images.getURLs")]
		XmlRpcStruct GetImageURLs(string sessionID, int imageId);

//		[XmlRpcMethod("getImageEXIF")]
		[XmlRpcMethod("smugmug.images.getEXIF")]
		XmlRpcStruct GetImageEXIF(string sessionID, int imageId);
	}
}
