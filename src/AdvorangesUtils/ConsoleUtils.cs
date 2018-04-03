using System;
using System.Collections.Concurrent;
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
		public static ConcurrentDictionary<string, List<string>> WrittenLines { get; } = new ConcurrentDictionary<string, List<string>>();
		/// <summary>
		/// Whether or not to prefix lines with the time and method that called them.
		/// </summary>
		public static bool LogTimeAndCaller { get; set; } = true;
		/// <summary>
		/// Whether or not to remove markdown before printing to the console;
		/// </summary>
		public static bool RemoveMarkdown { get; set; } = true;
		/// <summary>
		/// Whether or not to remove any duplicate new lines before printing to the console.
		/// </summary>
		public static bool RemoveDuplicateNewLines { get; set; } = true;

		private static Object _MessageLock = new Object();

		/// <summary>
		/// Writes the given text to the console with a timestamp and the calling method. Writes in gray by default.
		/// </summary>
		/// <param name="text">The text to write.</param>
		/// <param name="color">The color to use for the text.</param>
		/// <param name="name">The calling method.</param>
		public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray, [CallerMemberName] string name = "")
		{
			text = RemoveMarkdown ? text.RemoveAllMarkdown() : text;
			text = RemoveDuplicateNewLines ? text.RemoveDuplicateNewLines() : text;
			text = LogTimeAndCaller ? $"[{DateTime.Now.ToString("HH:mm:ss")}] [{name}]: {text}" : text;

			lock (_MessageLock)
			{
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(text);
				Console.ForegroundColor = oldColor;

				if (!WrittenLines.TryGetValue(name, out var list))
				{
					WrittenLines.TryAdd(name, list = new List<string>());
				}
				list.Add(text);
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