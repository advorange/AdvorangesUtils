﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace AdvorangesUtils.Tests
{
	[TestClass]
	public class UtilsTests
	{
		[TestMethod]
		public void CaseIns_Test()
		{
			Assert.IsTrue("ABC".CaseInsEquals("abc"));
			Assert.IsTrue("ABC".CaseInsEquals("ABC"));
			Assert.IsFalse("ABC".CaseInsEquals("AB"));

			Assert.IsTrue("ABC".CaseInsContains("a"));
			Assert.IsTrue("ABC".CaseInsContains("A"));
			Assert.IsFalse("ABC".CaseInsContains("Q"));

			Assert.IsTrue("ABC".CaseInsIndexOf("b", out int firstPos));
			Assert.AreEqual(1, firstPos);
			Assert.IsTrue("ABC".CaseInsIndexOf("C", out int secondPos));
			Assert.AreEqual(2, secondPos);
			Assert.IsFalse("ABC".CaseInsIndexOf("Q", out int thirdPos));
			Assert.AreEqual(-1, thirdPos);

			Assert.IsTrue("ABC".CaseInsStartsWith("a"));
			Assert.IsTrue("ABC".CaseInsStartsWith("A"));
			Assert.IsFalse("ABC".CaseInsStartsWith("Q"));

			Assert.IsTrue("ABC".CaseInsEndsWith("c"));
			Assert.IsTrue("ABC".CaseInsEndsWith("C"));
			Assert.IsFalse("ABC".CaseInsEndsWith("Q"));

			Assert.AreEqual("AbC", "ABC".CaseInsReplace("b", "b"));
			Assert.AreEqual("AbC", "ABC".CaseInsReplace("B", "b"));
			Assert.AreNotEqual("AbC", "ABC".CaseInsReplace("Q", "b"));

			Assert.IsTrue(new[] { "ABC", "ABc", "Abc" }.CaseInsEverythingSame());
			Assert.IsFalse(new[] { "ABC", "ABc", "Abq" }.CaseInsEverythingSame());

			Assert.IsTrue(new[] { "ABC", "ABc", "Abc" }.CaseInsContains("abc"));
			Assert.IsTrue(new[] { "ABC", "ABc", "Abc" }.CaseInsContains("abC"));
			Assert.IsFalse(new[] { "ABC", "ABc", "Abc" }.CaseInsContains("abq"));
		}
		[TestMethod]
		public void AllCharsWithinLimit_Test()
		{
			var valid = new StringBuilder();
			var invalid = new StringBuilder();
			var random = new Random();
			for (int i = 0; i < 50; ++i)
			{
				valid.Append((char)random.Next(33, 1000));
				invalid.Append((char)random.Next(1001, 50000));
			}

			Assert.IsTrue(valid.ToString().AllCharsWithinLimit());
			Assert.IsFalse(invalid.ToString().AllCharsWithinLimit());
		}
		[TestMethod]
		public void CountLineBreaks_Test()
		{
			Assert.AreEqual(0, "abc".CountLineBreaks());
			Assert.AreEqual(1, "\n".CountLineBreaks());
			Assert.AreEqual(1, "\r".CountLineBreaks());
			Assert.AreEqual(2, "\r\n".CountLineBreaks());
		}
		[TestMethod]
		public void SplitLikeCommandLine_Test()
		{
			Assert.AreEqual(9, "The quick brown fox jumps over the lazy dog".SplitLikeCommandLine().Length);
			Assert.AreEqual(7, "The \"quick brown\" fox jumps \"over the\" lazy dog".SplitLikeCommandLine().Length);
			Assert.AreEqual(2, "The \"quick brown fox jumps over the lazy dog".SplitLikeCommandLine().Length);
			Assert.AreEqual(7, "The [quick brown] fox jumps [over the] lazy dog".SplitLikeCommandLine(null, new[] { '[', ']' }).Length);
		}
		[TestMethod]
		public void IsValidUrl_Test()
		{
			Assert.IsTrue("https://www.google.com".IsValidUrl());
			Assert.IsFalse("www.google.com".IsValidUrl());
			Assert.IsFalse("https://".IsValidUrl());
			Assert.IsFalse("".IsValidUrl());

			Assert.IsTrue(new Uri(@"https://www.google.com").IsValidUrl());
			Assert.IsFalse(new Uri(@"C:\Users\User\Downloads\Test.txt").IsValidUrl());
		}
		[TestMethod]
		public void IsImagePath_Test()
		{
			Assert.IsTrue(@"C:\Users\User\Downloads\Test.png".IsImagePath());
			Assert.IsFalse(@"C:\Users\User\Downloads\Test.txt".IsImagePath());
			Assert.IsFalse(@"C:\Users\User\Downloads\Test.gif".IsImagePath());
		}
	}
}