using System;
using System.IO;

using LibGui;
using LibSys;

namespace WindowsTiler.DlgControls
{
	/// <summary>
	/// PdaHelper contains commonly used pda-related methods.
	/// </summary>
	public class PdaHelper
	{
		// all methods static
		protected PdaHelper()
		{
		}

		public static string exportSingleFile(string tileName, string srcPath, bool neverWrapPdb)
		{
			string newSrcPath = srcPath;

			string fileNameDst = tileName + ".jpg";
			string dstPath = Path.Combine(Project.exportDestFolder, fileNameDst);

			// format conversions:
			switch(Project.pdaExportImageFormat)
			{
				case 0:		// bmp websafe
				case 1:		// bmp optimized
					newSrcPath = CameraTrack.convertAndSaveTileImage(tileName, srcPath, Path.GetTempPath());
					fileNameDst = tileName + ".bmp";
					dstPath = Path.Combine(Project.exportDestFolder, fileNameDst);
					break;
				case 2:		// jpeg
					break;
			}

			// now either pack it in PDB, or just copy to destination:
			if(!neverWrapPdb && Project.pdaExportWrapPdb)
			{
				PalmPdbFormat ppf = new PalmPdbFormat();
				string catalogName = tileName; //dstPath;

				string fileNamePdb = tileName + ".pdb";
				dstPath = Path.Combine(Project.exportDestFolder, fileNamePdb);

				int format = Project.pdaExportImageFormat + (Project.pdaExportUseWebsafeGrayscalePalette ? 4 : 0);
				ppf.PackImageIntoPdb(catalogName, newSrcPath, dstPath, format);
			}
			else
			{
				if(File.Exists(dstPath))
				{
					File.Delete(dstPath);
				}
				File.Copy(newSrcPath, dstPath, true);
			}

			// get rid of temp file:
			if(!newSrcPath.Equals(srcPath))
			{
				try
				{
					File.Delete(newSrcPath);
				} 
				catch {}
			}
			return dstPath;
		}
	}
}
