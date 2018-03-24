using System;
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
		/// <summary>
		/// The lines which have been written to the console.
		/// </summary>
		public static SortedDictionary<string, List<string>> WrittenLines
		{
			get
			{
				if (_WrittenLines == null)
				{
					_WrittenLines = new SortedDictionary<string, List<string>>();
				}
				return _WrittenLines;
			}
		}

		private static SortedDictionary<string, List<string>> _WrittenLines;
		private static Object _MessageLock = new Object();

		/// <summary>
		/// Writes the given text to the console with a timestamp and the calling method. Writes in gray by default.
		/// </summary>
		/// <param name="text">The text to write.</param>
		/// <param name="name">The calling method.</param>
		/// <param name="color">The color to use for the text.</param>
		public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray, [CallerMemberName] string name = "")
		{
			var line = $"[{DateTime.Now.ToString("HH:mm:ss")}] [{name}]: {text.RemoveAllMarkdown().RemoveDuplicateNewLines()}";

			lock (_MessageLock)
			{
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(line);
				Console.ForegroundColor = oldColor;

				if (!WrittenLines.TryGetValue(name, out var list))
				{
					WrittenLines.Add(name, list = new List<string>());
				}
				list.Add(line);
			}
		}
		/// <summary>
		/// Writes the exception in red to the console.
		/// </summary>
		/// <param name="e">The exception to write.</param>
		/// <param name="name">The calling method.</param>
		public static void Write(this Exception e, [CallerMemberName] string name = "")
		{
			WriteLine($"{Environment.NewLine}EXCEPTION: {e}{Environment.NewLine}", ConsoleColor.Red, name);
		}
		/// <summary>
		/// Writes the text only when in debug mode. Only writes in <see cref="ConsoleColor.Cyan"/>.
		/// </summary>
		/// <param name="text">The text to write.</param>
		/// <param name="name">The calling method.</param>
		[Conditional("DEBUG")]
		public static void DebugWrite(string text, [CallerMemberName] string name = "")
		{
			WriteLine(text, ConsoleColor.Cyan, name);
		}
	}
}