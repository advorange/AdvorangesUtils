using System;

namespace AdvorangesUtils
{
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