using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AdvorangesUtils
{
	/// <summary>
	/// Actions which are extensions to other classes.
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if two strings are the same.
		/// </summary>
		/// <param name="str1"></param>
		/// <param name="str2"></param>
		/// <returns></returns>
		public static bool CaseInsEquals(this string str1, string str2)
			=> string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string contains a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsContains(this string source, string search)
			=> source != null && search != null && source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to return the index of a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool CaseInsIndexOf(this string source, string search, out int index)
			=> (index = source == null || search == null ? -1 : source.IndexOf(search, StringComparison.OrdinalIgnoreCase)) >= 0;
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsStartsWith(this string source, string search)
			=> source != null && search != null && source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsEndsWith(this string source, string search)
			=> source != null && search != null && source.EndsWith(search, StringComparison.OrdinalIgnoreCase);
		/// <summary>
		/// Returns the string with the oldValue replaced with the newValue case insensitively.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static string CaseInsReplace(this string source, string oldValue, string newValue)
		{
			var sb = new StringBuilder();
			var previousIndex = 0;
			var index = source.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
			while (index != -1)
			{
				sb.Append(source.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = source.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
			}
			return sb.Append(source.Substring(previousIndex)).ToString();
		}
		/// <summary>
		/// Utilizes <see cref="CaseInsEquals(string, string)"/> to check if every string is the same.
		/// </summary>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static bool CaseInsEverythingSame(this IEnumerable<string> enumerable)
		{
			var array = enumerable.ToArray();
			for (var i = 1; i < array.Length; ++i)
			{
				if (!array[i - 1].CaseInsEquals(array[i]))
				{
					return false;
				}
			}
			return true;
		}
		/// <summary>
		/// Utilizes <see cref="StringComparer.OrdinalIgnoreCase"/> to see if the search string is in the enumerable.
		/// </summary>
		/// <param name="enumerable"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsContains(this IEnumerable<string> enumerable, string search)
			=> enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Returns true if the passed in string is a valid url.
		/// </summary>
		/// <param name="input">The uri to evaluate.</param>
		/// <returns>A boolean indicating whether or not the string is a url.</returns>
		public static bool IsValidUrl(this string input)
			=> Uri.TryCreate(input, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
		/// <summary>
		/// Returns true if the passed in uri is a valid url.
		/// </summary>
		/// <param name="uri">The uri to evaluate.</param>
		/// <returns>A boolean indicating whether or not the uri is a url.</returns>
		public static bool IsValidUrl(this Uri uri)
			=> uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
		/// <summary>
		/// Returns true for most image mime types (png, jpg, tiff, etc) but false for gif and anything else.
		/// </summary>
		/// <param name="path">The path or extension to check.</param>
		/// <returns>A boolean inicating whether or not the path leads to an image.</returns>
		public static bool IsImagePath(this string path)
		{
			var mimeType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(path));
			return mimeType != "image/gif" && mimeType.CaseInsContains("image/");
		}
		/// <summary>
		/// Verifies all characters in the string have a value of a less than the upperlimit.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public static bool AllCharsWithinLimit(this string str, int limit = 1000)
			=> !str.Any(x => x > limit);
		/// <summary>
		/// Returns the count of characters equal to \r or \n.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static int CountLineBreaks(this string str)
			=> str?.Count(x => x == '\r' || x == '\n') ?? 0;
		/// <summary>
		/// Splits a string in a similar fashion to how a command line splits.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static IEnumerable<string> SplitLikeCommandLine(this string input)
			=> input.ComplexSplit(new[] { ' ' }, new[] { '"' }, removeQuotes: false);
		/// <summary>
		/// Splits a string by the characters passed in <paramref name="seperators"/> unless they're inside <paramref name="quotes"/>.
		/// </summary>
		/// <remarks>
		/// This method will split like command line when no arguments are provided for the arrays.
		/// When arguments are provided for the arrays, then it can split very differently.
		/// Square brackets, point brackets, etc. other quotes can be used in <paramref name="quotes"/> to treat those as standard quotes.
		/// </remarks>
		/// <param name="input">The string to split.</param>
		/// <param name="seperators">What to split on.</param>
		/// <param name="quotes">What to not split when in between.</param>
		/// <param name="removeQuotes">When a part has something from <paramref name="quotes"/> surrounding it they will be removed.</param>
		/// <returns>An array of strings representing arguments.</returns>
		public static IEnumerable<string> ComplexSplit(this string input, IEnumerable<char> seperators, IEnumerable<char> quotes, bool removeQuotes)
		{
			if (seperators == null)
			{
				throw new ArgumentException($"{nameof(seperators)} cannot be null.", nameof(seperators));
			}
			if (quotes == null)
			{
				throw new ArgumentException($"{nameof(quotes)} cannot be null.", nameof(quotes));
			}

			var inside = false; //Whether or not the current part is inside an ignored
			var part = new ComplexSplitPart(removeQuotes);
			for (var i = 0; i < input.Length; ++i)
			{
				var c = input[i];
				if (quotes.Contains(c))
				{
					inside = !inside;
					part.AddQuoteChar(c);
					continue;
				}
				if (!inside && seperators.Contains(c))
				{
					if (part.Length > 0)
					{
						yield return part.ToString();
					}
					part.Clear();
					continue;
				}
				part.AddChar(c);
			}
			if (part.Length > 0)
			{
				yield return part.ToString();
			}
		}

		/// <summary>
		/// Short way to write <see cref="Task{T}.ConfigureAwait(bool)"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="task"></param>
		/// <returns></returns>
		public static ConfiguredTaskAwaitable<T> CAF<T>(this Task<T> task)
			=> task.ConfigureAwait(false);
		/// <summary>
		/// Short way to write <see cref="Task.ConfigureAwait(bool)"/>.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static ConfiguredTaskAwaitable CAF(this Task task)
			=> task.ConfigureAwait(false);
		/// <summary>
		/// Shot way to write <see cref="ValueTask{TResult}.ConfigureAwait(bool)"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="valueTask"></param>
		/// <returns></returns>
		public static ConfiguredValueTaskAwaitable<T> CAF<T>(this ValueTask<T> valueTask)
			=> valueTask.ConfigureAwait(false);
		/// <summary>
		/// Shot way to write <see cref="ValueTask.ConfigureAwait(bool)"/>.
		/// </summary>
		/// <param name="valueTask"></param>
		/// <returns></returns>
		public static ConfiguredValueTaskAwaitable CAF(this ValueTask valueTask)
			=> valueTask.ConfigureAwait(false);
	}
}