using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AdvorangesUtils
{
	/// <summary>
	/// Formatting for basic things, such as escaping characters or removing new lines.
	/// </summary>
	public static class Formatting
	{
		/// <summary>
		/// Joins all strings which are not null with the given string.
		/// </summary>
		/// <param name="seperator">The value to join each string with.</param>
		/// <param name="values">The strings to join.</param>
		/// <returns>All strings which were not null joined together.</returns>
		public static string JoinNonNullStrings(this IEnumerable<string> values, string seperator)
		{
			return String.Join(seperator, values.Where(x => !String.IsNullOrWhiteSpace(x)));
		}
		/// <summary>
		/// Returns a string which is a numbered list of the passed in objects. The format is for the passed in arguments; the counter is added by default.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values">The values to put into a numbered list.</param>
		/// <param name="format">How to format each entry.</param>
		/// <returns>A numbered list of objects formatted in the supplied way.</returns>
		public static string FormatNumberedList<T>(this IEnumerable<T> values, Func<T, string> func)
		{
			var list = values.ToList();
			var maxLen = list.Count.ToString().Length;
			return String.Join("\n", list.Select((x, index) => $"`{(index + 1).ToString().PadLeft(maxLen, '0')}.` {func(x)}"));
		}
		/// <summary>
		/// Returns the input string with ` escaped.
		/// </summary>
		/// <param name="input">The input to escape backticks from.</param>
		/// <returns>The input with escaped backticks.</returns>
		public static string EscapeBackTicks(this string input)
		{
			return input?.Replace("`", "\\`");
		}
		/// <summary>
		/// Returns the input string without \, *, _, ~, and `.
		/// </summary>
		/// <param name="input">The input to remove markdown from.</param>
		/// <returns>The input without any markdown.</returns>
		public static string RemoveAllMarkdown(this string input)
		{
			return input?.Replace("\\", "")?.Replace("*", "")?.Replace("_", "")?.Replace("~", "")?.Replace("`", "");
		}
		/// <summary>
		/// Returns the input string with no duplicate new lines. Also changes any carriage returns to new lines.
		/// </summary>
		/// <param name="input">The input to remove duplicate new lines from.</param>
		/// <returns>The input without any duplicate new lines.</returns>
		public static string RemoveDuplicateNewLines(this string input)
		{
			var str = input.Replace("\r", "\n");
			var len = str.Length;
			do
			{
				len = str.Length;
				str = str.Replace("\n\n", "\n");
			} while (len != str.Length);
			return str;
		}
		/// <summary>
		/// Adds in spaces between each capital letter and capitalizes every letter after a space.
		/// </summary>
		/// <param name="input">The input to put into title case.</param>
		/// <returns>The input in title case.</returns>
		public static string FormatTitle(this string input)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < input.Length; ++i)
			{
				var c = input[i];
				if (Char.IsUpper(c) && (i > 0 && !Char.IsWhiteSpace(input[i - 1])))
				{
					sb.Append(' ');
				}
				//Determine if the char should be made capital
				sb.Append(i == 0 || i > 0 && Char.IsWhiteSpace(input[i - 1]) ? Char.ToUpper(c) : c);
			}
			return sb.ToString();
		}
		/// <summary>
		/// Only appends a \n after the value. On Windows <see cref="StringBuilder.AppendLine(string)"/> appends \r\n (which isn't
		/// necessarily wanted).
		/// </summary>
		/// <param name="sb">The stringbuilder to append text to.</param>
		/// <param name="value">The text to append before the new line.</param>
		/// <returns>The stringbuilder that was passed in.</returns>
		public static StringBuilder AppendLineFeed(this StringBuilder sb, string value = "")
		{
			return sb.Append(value + "\n");
		}

		/// <summary>
		/// Returns a formatted string displaying the bot's current uptime.
		/// </summary>
		/// <returns>Formatted string displaying how long the program has been running.</returns>
		public static string GetUptime()
		{
			var span = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
			return $"{(int)span.TotalDays}:{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
		}
		/// <summary>
		/// Returns the current time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns>Formatted string for use in file names.</returns>
		public static string ToSaving()
		{
			return DateTime.UtcNow.ToString("yyyyMMdd_hhmmss");
		}
		/// <summary>
		/// Returns the passed in time as a human readable time.
		/// </summary>
		/// <param name="dt">The datetime to format.</param>
		/// <returns>Formatted string that is readable by humans.</returns>
		public static string ToReadable(this DateTime dt)
		{
			var utc = dt.ToUniversalTime();
			var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(utc.Month);
			return $"{monthName} {utc.Day}, {utc.Year} at {utc.ToLongTimeString()}";
		}
		/// <summary>
		/// Returns the passed in time as a human readable time and says how many days ago it was. Uses markdown.
		/// </summary>
		/// <param name="dt">The datetime to format.</param>
		/// <returns>Formatted string that says when something was created with markdown.</returns>
		public static string ToCreatedAt(this DateTime dt, bool withMarkdown = true)
		{
			var diff = DateTime.UtcNow.Subtract(dt).TotalDays;
			return $"**Created:** `{ToReadable(dt)}` (`{diff:0.00}` days ago)";
		}
	}
}
