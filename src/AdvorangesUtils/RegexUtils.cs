﻿using System;
using System.Text.RegularExpressions;

namespace AdvorangesUtils
{
	/// <summary>
	/// Actions which are done on a <see cref="Regex"/>.
	/// </summary>
	public static class RegexUtils
	{
		private static Regex _TwitchRegex = new Regex("^[a-zA-Z0-9_]{4,25}$", RegexOptions.Compiled);
		private static TimeSpan _Timespan = new TimeSpan(5000);

		/// <summary>
		/// Returns true if the pattern is found within the input. Has a timeout of 1,000,000 ticks.
		/// Will only check up to the first 2,000 characters since this should only be applied to message content.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="pattern"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public static bool IsMatch(string input, string pattern, int maxLength = 2000)
		{
			var content = input.Substring(0, Math.Min(input.Length, maxLength));

			try
			{
				return Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase, _Timespan);
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}
		/// <summary>
		/// Returns true if the name is null, empty, or matches the <see cref="Regex"/> from https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool IsValidTwitchName(string input)
		{
			return String.IsNullOrEmpty(input) ? true : _TwitchRegex.IsMatch(input);
		}
	}
}
