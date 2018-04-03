using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesUtils.Tests
{
	[TestClass]
	public class RegexUtilsTests
	{
		[TestMethod]
		public void IsMatch_Test()
		{
			//Basic match
			Assert.IsTrue(RegexUtils.IsMatch("asdlfj", ".*"));
			//Very long, no match
			Assert.IsFalse(RegexUtils.IsMatch(new string('a', 1000000), "b"));
			//Matches, but slowly
			Assert.IsTrue(RegexUtils.IsMatch("xxxxxxxxxxxxxxxxxxxxxxxxy", "(x+x+)+y"));
			//Timeout
			Assert.IsFalse(RegexUtils.IsMatch("xxxxxxxxxxxxxxxxxxxxxxxx", "(x+x+)+y"));
		}
		[TestMethod]
		public void IsValidTwitchName_Test()
		{
			//Valid name
			Assert.IsTrue(RegexUtils.IsValidTwitchName("Advorange"));
			Assert.IsTrue(RegexUtils.IsValidTwitchName(new string('a', 4)));
			Assert.IsTrue(RegexUtils.IsValidTwitchName(new string('a', 25)));

			//Invalid names
			Assert.IsFalse(RegexUtils.IsValidTwitchName("~$@$&^((&*$^%#@!~"));
			Assert.IsFalse(RegexUtils.IsValidTwitchName(new string('a', 26)));
			Assert.IsFalse(RegexUtils.IsValidTwitchName(new string('a', 3)));
		}
	}
}