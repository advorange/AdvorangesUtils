using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvorangesUtils.Tests
{
	[TestClass]
	public class EnumUtilsTests
	{
		[Flags]
		private enum TestEnum : ulong
		{
			None = 0,
			A = 1,
			B = 2,
			C = A | B, //3
			D = 4,
			E = 8,
			F = C | D, //12
			All = E | F, //15
		}

		private readonly IReadOnlyCollection<string> _TryParseInput = new[]
		{
			nameof(TestEnum.None),
			"Dog",
			nameof(TestEnum.A),
			nameof(TestEnum.E),
			"Fish",
			"Cat",
		};

		[TestMethod]
		public void GetFlagNames_Test()
		{
			Assert.AreEqual(1, EnumUtils.GetFlagNames(TestEnum.All, true).Count());
			Assert.AreEqual(8, EnumUtils.GetFlagNames(TestEnum.All, false).Count());
		}
		[TestMethod]
		public void GetFlagsGeneric_Test()
		{
			Assert.AreEqual(1, EnumUtils.GetFlags(TestEnum.All, true).Count());
			Assert.AreEqual(8, EnumUtils.GetFlags(TestEnum.All, false).Count());
		}
		[TestMethod]
		public void GetFlags_Test()
		{
			Assert.AreEqual(1, EnumUtils.GetFlags((Enum)TestEnum.All, true).Count());
			Assert.AreEqual(8, EnumUtils.GetFlags((Enum)TestEnum.All, false).Count());
		}
		[TestMethod]
		public void TryParseMultiple_Test()
		{
			Assert.IsFalse(EnumUtils.TryParseMultiple<TestEnum>(_TryParseInput, out var valid, out var invalid));
			Assert.AreEqual(3, valid.Count);
			Assert.AreEqual(3, invalid.Count);
		}
		[TestMethod]
		public void TryParse_Test()
		{
			Assert.IsFalse(EnumUtils.TryParseFlags(_TryParseInput, out TestEnum value, out var invalid));
			Assert.AreEqual(TestEnum.None | TestEnum.A | TestEnum.E, value);
			Assert.AreEqual(3, invalid.Count);
		}
	}
}