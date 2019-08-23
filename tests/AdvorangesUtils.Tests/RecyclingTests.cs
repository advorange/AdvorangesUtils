using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesUtils.Tests
{
	[TestClass]
	public class RecyclingTests
	{
		[TestMethod]
		public void Recycling_Test()
			=> Assert.AreEqual(0, RecyclingUtils.MoveFile(CreateFile()));

		private FileInfo CreateFile()
		{
			var file = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "RecyclingTestFile.tmp"));
			using (var fs = file.Create())
			{
				fs.Close();
			}
			return file;
		}
	}
}
