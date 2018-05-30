﻿using System;
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
		public static ConcurrentDictionary<string, List<string>> WrittenLines { get; } = new ConcurrentDictionary<string, List<string>>();
		/// <summary>
		/// The settings detailing how to print with the console.
		/// </summary>
		public static ConsolePrintingFlags PrintingFlags { get; set; } = 0
			| ConsolePrintingFlags.Print
			| ConsolePrintingFlags.LogTime
			| ConsolePrintingFlags.LogCaller
			| ConsolePrintingFlags.RemoveMarkdown
			| ConsolePrintingFlags.RemoveDuplicateNewLines;

		private static readonly Object _MessageLock = new Object();

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
			if ((PrintingFlags & ConsolePrintingFlags.Print) == 0)
			{
				//To rethrow with the correct stacktrace
				ExceptionDispatchInfo.Capture(e).Throw();
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
		{
			WriteLine(text, ConsoleColor.Cyan, name);
		}
	}

	/// <summary>
	/// Details about how to print from <see cref="ConsoleUtils"/>.
	/// </summary>
	[Flags]
	public enum ConsolePrintingFlags : uint
	{
		/// <summary>
		/// Whether or not to print. If this is false then this library will stop printing, and will throw all exceptions instead of writing them.
		/// </summary>
		Print = (1U << 0),
		/// <summary>
		/// Logs the time.
		/// </summary>
		LogTime = (1U << 1),
		/// <summary>
		/// Logs the calling method.
		/// </summary>
		LogCaller = (1U << 2),
		/// <summary>
		/// Removes markdown before printing.
		/// </summary>
		RemoveMarkdown = (1U << 3),
		/// <summary>
		/// Removes duplicate new lines before printing.
		/// </summary>
		RemoveDuplicateNewLines = (1U << 4),
	}
}