using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MetadataExtractor;

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
		/// Gets the width and height of an image through metadata.
		/// If multiple widths and heights are gotten via metadata, will return the smallest ones.
		/// Will not seek on the stream, so make sure it's at the beginning yourself.
		/// </summary>
		/// <param name="s">The image's data.</param>
		/// <param name="method">What to do if more than one width or height is found.</param>
		/// <returns></returns>
		public static (int Width, int Height) GetImageSize(this Stream s, DuplicateSizeMethod method = DuplicateSizeMethod.Minimum)
		{
			try
			{
				var tags = ImageMetadataReader.ReadMetadata(s).SelectMany(x => x.Tags);
				var width = tags.SelectWhere(x => x.Name == "Image Width", x => Convert.ToInt32(x.Description.Split(' ')[0]));
				var height = tags.SelectWhere(x => x.Name == "Image Height", x => Convert.ToInt32(x.Description.Split(' ')[0]));
				if (width.Count() > 1 || height.Count() > 1)
				{
					switch (method)
					{
						case DuplicateSizeMethod.Minimum:
							return (width.Min(), height.Min());
						case DuplicateSizeMethod.Maximum:
							return (width.Max(), height.Max());
						case DuplicateSizeMethod.First:
							return (width.First(), height.First());
						case DuplicateSizeMethod.Last:
							return (width.Last(), height.Last());
						case DuplicateSizeMethod.Throw:
							throw new InvalidOperationException("More than one width or height was gotten for the image.");
					}
				}
				return (width.Single(), height.Single());
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Unable to parse the image's width and height from the stream's metadata.", e);
			}
		}
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
		/// Orders an <see cref="IEnumerable{T}"/> by something that does not implement <see cref="IComparable"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="source">The objects to order.</param>
		/// <param name="keySelector">The property to order by.</param>
		/// <returns>An ordered enumerable.</returns>
		public static IEnumerable<T> OrderByNonComparable<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (keySelector == null)
			{
				throw new ArgumentNullException(nameof(keySelector));
			}

			return source.GroupBy(keySelector).SelectMany(x => x);
		}
		/// <summary>
		/// Makes the source distinct by a selected property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="source"></param>
		/// <param name="keySelector"></param>
		/// <returns></returns>
		public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (keySelector == null)
			{
				throw new ArgumentNullException(nameof(keySelector));
			}

			return source.GroupBy(keySelector).Select(x => x.First());
		}
		/// <summary>
		/// Makes the source into a new type while also only working on the wanted items.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="source">The objects to select where from.</param>
		/// <param name="predicate">The function declaring what items to select.</param>
		/// <param name="selector">The function declaring how to convert the items.</param>
		/// <returns></returns>
		public static IEnumerable<TResult> SelectWhere<T, TResult>(this IEnumerable<T> source, Func<T, bool> predicate, Func<T, TResult> selector)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			foreach (var item in source)
			{
				if (predicate(item))
				{
					yield return selector(item);
				}
			}
		}
		/// <summary>
		/// Puts an <see cref="IEnumerable{T}"/> into a variable amount of <see cref="IEnumerable{T}"/> with each containing <paramref name="groupSize"/> elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="groupSize"></param>
		/// <returns></returns>
		public static IEnumerable<IEnumerable<T>> GroupInto<T>(this IEnumerable<T> source, int groupSize)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.Select((obj, index) => new { Obj = obj, Index = index })
				.GroupBy(x => x.Index / groupSize)
				.Select(g => g.Select(o => o.Obj));
		}
		/// <summary>
		/// Attempts to get the first matching value. Will return default if no matches are found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <param name="found"></param>
		/// <returns></returns>
		public static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T found)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			found = default;
			foreach (var item in source)
			{
				if (predicate(item))
				{
					found = item;
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Attempts to get a single matching value. Will throw if more than one match is found. Will return default if no matches are found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <param name="found"></param>
		/// <returns></returns>
		public static bool TryGetSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T found)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			found = default;
			var matched = false;
			foreach (var item in source)
			{
				if (predicate(item))
				{
					if (matched)
					{
						throw new InvalidOperationException("More than one match found.");
					}
					found = item;
					matched = true;
				}
			}
			return matched;
		}
		/// <summary>
		/// Adds all of the collection to the list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="collection"></param>
		public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if (source is List<T> concrete)
			{
				concrete.AddRange(collection);
			}
			else
			{
				foreach (var item in collection)
				{
					source.Add(item);
				}
			}
		}
		/// <summary>
		/// Removes elements which match the supplied predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="match"></param>
		public static int RemoveAll<T>(this ICollection<T> source, Predicate<T> match)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			if (source is List<T> concrete)
			{
				return concrete.RemoveAll(match);
			}
			else if (source is IList<T> indexable)
			{
				var removedCount = 0;
				for (var i = source.Count - 1; i >= 0; --i)
				{
					if (match(indexable[i]))
					{
						indexable.RemoveAt(i);
						++removedCount;
					}
				}
				return removedCount;
			}
			else
			{
				var removedCount = 0;
				for (var i = source.Count - 1; i >= 0; --i)
				{
					var element = source.ElementAt(i);
					if (match(element))
					{
						source.Remove(element);
						++removedCount;
					}
				}
				return removedCount;
			}
		}
		/// <summary>
		/// Removes elements which are equal according to the supplied equality comparer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="value"></param>
		/// <param name="equalityComparer"></param>
		/// <returns></returns>
		public static int RemoveAll<T>(this ICollection<T> source, T value, IEqualityComparer<T> equalityComparer)
			=> source.RemoveAll(x => equalityComparer.Equals(x, value));

		/// <summary>
		/// Short way to write ConfigureAwait(false).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="task"></param>
		/// <returns></returns>
		public static ConfiguredTaskAwaitable<T> CAF<T>(this Task<T> task)
			=> task.ConfigureAwait(false);
		/// <summary>
		/// Short way to write ConfigureAwait(false).
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static ConfiguredTaskAwaitable CAF(this Task task)
			=> task.ConfigureAwait(false);
	}
}