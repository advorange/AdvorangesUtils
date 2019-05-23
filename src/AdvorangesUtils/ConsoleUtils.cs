using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

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
		public static ConcurrentDictionary<string, ConcurrentBag<string>> WrittenLines { get; } = new ConcurrentDictionary<string, ConcurrentBag<string>>();
		/// <summary>
		/// The settings detailing how to print with the console.
		/// </summary>
		public static ConsolePrintingFlags PrintingFlags { get; set; } = 0
			| ConsolePrintingFlags.Print
			| ConsolePrintingFlags.LogTime
			| ConsolePrintingFlags.LogCaller
			| ConsolePrintingFlags.RemoveMarkdown
			| ConsolePrintingFlags.RemoveDuplicateNewLines;

		private static readonly object _MessageLock = new object();

		/// <summary>
		/// Writes the given text to the console with a timestamp and the calling method. Writes in gray by default.
		/// </summary>
		/// <param name="text">The text to write.</param>
		/// <param name="color">The color to use for the text.</param>
		/// <param name="name">The calling method.</param>
		public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray, [CallerMemberName] string name = "")
		{
			if ((PrintingFlags & ConsolePrintingFlags.Print) == 0)
			{
				return;
			}

			text = (PrintingFlags & ConsolePrintingFlags.RemoveMarkdown) != 0 ? text.RemoveAllMarkdown() : text;
			text = (PrintingFlags & ConsolePrintingFlags.RemoveDuplicateNewLines) != 0 ? text.RemoveDuplicateNewLines() : text;

			var time = (PrintingFlags & ConsolePrintingFlags.LogTime) != 0;
			var call = (PrintingFlags & ConsolePrintingFlags.LogCaller) != 0;
			if (time && call)
			{
				text = $"[{DateTime.Now.ToString("HH:mm:ss")}] [{name}]: {text}";
			}
			else if (time)
			{
				text = $"[{DateTime.Now.ToString("HH:mm:ss")}]: {text}";
			}
			else if (call)
			{
				text = $"[{name}]: {text}";
			}

			lock (_MessageLock)
			{
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(text);
				Console.ForegroundColor = oldColor;

				if (!WrittenLines.TryGetValue(name, out var list))
				{
					WrittenLines.TryAdd(name, list = new ConcurrentBag<string>());
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
			if ((PrintingFlags & ConsolePrintingFlags.Print) == 0)
			{
				//To rethrow with the correct stacktrace
				ExceptionDispatchInfo.Capture(e).Throw();
				return;
			}
			WriteLine($"{Environment.NewLine}EXCEPTION: {e}{Environment.NewLine}", ConsoleColor.Red, name);
		}
		/// <summary>
		/// Writes the text only when in debug mode. Only writes in <see cref="ConsoleColor.Cyan"/>.
		/// </summary>
		/// <param name="text">The text to write.</param>
		/// <param name="name">The calling method.</param>
		[Conditional("DEBUG")]
		public static void DebugWrite(string text, [CallerMemberName] string name = "")
			=> WriteLine(text, ConsoleColor.Cyan, name);
	}
}