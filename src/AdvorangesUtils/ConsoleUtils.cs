﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AdvorangesUtils
{
	/// <summary>
	/// Actions involving the console.
	/// </summary>
	public static class ConsoleUtils
	{
		private static SortedDictionary<string, List<string>> _WrittenLines;
		private static Object _MessageLock = new Object();

		/// <summary>
		/// Returns a dictionary containing lines that have been written to the console if it exists. Otherwise creates it.
		/// </summary>
		/// <returns></returns>
		public static SortedDictionary<string, List<string>> GetOrCreateWrittenLines()
		{
			lock (_MessageLock)
			{
				return _WrittenLines = _WrittenLines ?? new SortedDictionary<string, List<string>>();
			}
		}
		/// <summary>
		/// Writes the given text to the console with a timestamp and the calling method. Writes in gray by default.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="name"></param>
		/// <param name="color"></param>
		public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray, [CallerMemberName] string name = "")
		{
			var line = $"[{DateTime.Now.ToString("HH:mm:ss")}] [{name}]: {text.RemoveAllMarkdown().RemoveDuplicateNewLines()}";

			lock (_MessageLock)
			{
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(line);
				Console.ForegroundColor = oldColor;

				if (_WrittenLines == null)
				{
					return;
				}
				if (!_WrittenLines.TryGetValue(name, out var list))
				{
					_WrittenLines.Add(name, list = new List<string>());
				}
				list.Add(line);
			}
		}
		/// <summary>
		/// Writes the exception in red to the console.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="name"></param>
		public static void Write(this Exception e, [CallerMemberName] string name = "")
		{
			WriteLine($"{Environment.NewLine}EXCEPTION: {e}{Environment.NewLine}", ConsoleColor.Red, name);
		}
		/// <summary>
		/// Writes the text only when in debug mode. Only writes in <see cref="ConsoleColor.Cyan"/>.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="name"></param>
		[Conditional("DEBUG")]
		public static void DebugWrite(string text, [CallerMemberName] string name = "")
		{
			WriteLine(text, ConsoleColor.Cyan, name);
		}
	}
}