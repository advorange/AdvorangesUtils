using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AdvorangesUtils
{
	/// <summary>
	/// Sends a file to the recycle bin on Windows. Source: https://stackoverflow.com/a/3282450
	/// </summary>
	public static class RecyclingUtils
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
		private struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.U4)]
			public int wFunc;
			public string pFrom;
			public string pTo;
			public short fFlags;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			public string lpszProgressTitle;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

		private const int FO_DELETE = 3;
		private const int FOF_ALLOWUNDO = 0x40;
		private const int FOF_NOCONFIRMATION = 0x10; //Don't prompt the user

		/// <summary>
		/// Utilizes <see cref="SHFileOperation(ref SHFILEOPSTRUCT)"/> to move a file to the recycle bin with undo preservation and no confirmation.
		/// If invoked on non windows, will simply delete the file.
		/// </summary>
		/// <param name="file">The file to delete.</param>
		/// <returns>The error code gotten from moving the file. 0 indicates success.</returns>
		public static int MoveFile(FileInfo file)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return Move(file.FullName);
			}
			file.Delete();
			return 0;
		}
		/// <summary>
		/// Utilizes <see cref="SHFileOperation(ref SHFILEOPSTRUCT)"/> to move multiple files to the recycle bin with undo preservation and no confirmation.
		/// If invoked on non windows, will simply delete the file.
		/// </summary>
		/// <param name="files">The files to delete.</param>
		/// <returns>The error code gotten from moving the file. 0 indicates success.</returns>
		public static int MoveFiles(IEnumerable<FileInfo> files)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				//Files need to be joined with null char and entire string needs to end with it too
				return Move(String.Join("\0", files.Select(x => x.FullName)) + "\0");
			}
			foreach (var file in files)
			{
				file.Delete();
			}
			return 0;
		}
		private static int Move(string input)
		{
			var shf = new SHFILEOPSTRUCT
			{
				wFunc = FO_DELETE,
				fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION,
				pFrom = input
			};
			return SHFileOperation(ref shf);
		}
	}
}
