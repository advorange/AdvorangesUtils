using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AdvorangesUtils
{
	/// <summary>
	/// Formatting for basic things, such as escaping characters or removing new lines.
	/// </summary>
	public static class FormattingUtils
	{
		/// <summary>
		/// Joins the strings together with <paramref name="separator"/> after selecting them.
		/// </summary>
		/// <typeparam name="T">The type of arguments being passed in.</typeparam>
		/// <param name="source">The values to join.</param>
		/// <param name="separator">The value to join each string with.</param>
		/// <param name="selector">How to convert each value to a string.</param>
		/// <returns>All strings joined together.</returns>
		public static string Join<T>(this IEnumerable<T> source, Func<T, string> selector, string separator)
			=> source.Select(selector).Join(separator);
		/// <summary>
		/// Joins the strings together with <see cref="TextInfo.ListSeparator"/> from <see cref="CultureInfo.CurrentCulture"/> after selecting them.
		/// </summary>
		/// <typeparam name="T">The type of arguments being passed in.</typeparam>
		/// <param name="source">The values to join.</param>
		/// <param name="selector">How to convert each value to a string.</param>
		/// <returns>All strings joined together.</returns>
		public static string Join<T>(this IEnumerable<T> source, Func<T, string> selector)
		{
			var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
			return source.Join(selector, separator);
		}
		/// <summary>
		/// Joins the strings together with <paramref name="separator"/>.
		/// </summary>
		/// <param name="source">The values to join.</param>
		/// <param name="separator">The value to join each string with.</param>
		/// <returns>All strings joined together.</returns>
		public static string Join(this IEnumerable<string> source, string separator)
			=> string.Join(separator, source);
		/// <summary>
		/// Joins the strings together with <see cref="TextInfo.ListSeparator"/> from <see cref="CultureInfo.CurrentCulture"/>.
		/// </summary>
		/// <param name="source">The values to join.</param>
		/// <returns>All strings joined together.</returns>
		public static string Join(this IEnumerable<string> source)
		{
			var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
			return source.Join(separator);
		}
		/// <summary>
		/// Returns a string which is a numbered list of the passed in objects. The format is for the passed in arguments; the counter is added by default.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values">The values to put into a numbered list.</param>
		/// <param name="func">How to format each entry.</param>
		/// <returns>A numbered list of objects formatted in the supplied way.</returns>
		public static string FormatNumberedList<T>(this IEnumerable<T> values, Func<T, string> func)
		{
			var list = values.ToArray();
			var maxLen = list.Length.ToString().Length;
			return list.Select((x, index) => $"`{(index + 1).ToString().PadLeft(maxLen, '0')}.` {func(x)}").Join("\n");
		}
		/// <summary>
		/// Returns the input string with ` escaped.
		/// </summary>
		/// <param name="input">The input to escape backticks from.</param>
		/// <returns>The input with escaped backticks.</returns>
		public static string EscapeBackTicks(this string input)
			=> input?.Replace("`", "\\`");
		/// <summary>
		/// Returns the input string without \, *, _, ~, and `.
		/// </summary>
		/// <param name="input">The input to remove markdown from.</param>
		/// <returns>The input without any markdown.</returns>
		public static string RemoveAllMarkdown(this string input)
			=> input?.Replace("\\", "")?.Replace("*", "")?.Replace("_", "")?.Replace("~", "")?.Replace("`", "");
		/// <summary>
		/// Returns the input string with no duplicate new lines. Also changes any carriage returns to new lines.
		/// </summary>
		/// <param name="input">The input to remove duplicate new lines from.</param>
		/// <returns>The input without any duplicate new lines.</returns>
		public static string RemoveDuplicateNewLines(this string input)
		{
			var str = input.Replace("\r", "\n");
			int len;
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
				if (char.IsUpper(c) && i > 0 && !char.IsWhiteSpace(input[i - 1]))
				{
					sb.Append(' ');
				}
				//Determine if the char should be made capital
				sb.Append(i == 0 || (i > 0 && char.IsWhiteSpace(input[i - 1])) ? char.ToUpper(c) : c);
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
			=> sb.Append(value + "\n");

		/// <summary>
		/// Returns a formatted string displaying the bot's current uptime in the format of 1:24:60:60.
		/// </summary>
		/// <returns>Formatted string displaying how long the program has been running.</returns>
		public static string GetUptime()
		{
			var span = ProcessInfoUtils.GetUptime();
			return $"{(int)span.TotalDays}:{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
		}
		/// <summary>
		/// Returns the current UTC time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns>Formatted string for use in file names.</returns>
		public static string ToSaving()
			=> DateTime.UtcNow.ToSaving();
		/// <summary>
		/// Returns the time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns>Formatted string for use in file names.</returns>
		public static string ToSaving(this DateTime dt)
			=> dt.ToString("yyyyMMdd_hhmmss");
		/// <summary>
		/// Returns the passed in time as a human readable time.
		/// </summary>
		/// <param name="dt">The datetime to format.</param>
		/// <returns>Formatted string that is readable by humans.</returns>
		public static string ToReadable(this DateTime dt)
		{
			var utc = dt.ToUniversalTime();
			var month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(utc.Month);
			return $"{month} {utc.Day}, {utc.Year} at {utc.ToLongTimeString()}";
		}
	}
}
