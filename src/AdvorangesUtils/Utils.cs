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
		{
			return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string contains a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsContains(this string source, string search)
		{
			return source != null && search != null && source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to return the index of a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool CaseInsIndexOf(this string source, string search, out int index)
		{
			return (index = source == null || search == null ? -1 : source.IndexOf(search, StringComparison.OrdinalIgnoreCase)) >= 0;
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsStartsWith(this string source, string search)
		{
			return source != null && search != null && source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsEndsWith(this string source, string search)
		{
			return source != null && search != null && source.EndsWith(search, StringComparison.OrdinalIgnoreCase);
		}
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
		{
			return enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);
		}
		/// <summary>
		/// Returns true if the passed in string is a valid url.
		/// </summary>
		/// <param name="input">The uri to evaluate.</param>
		/// <returns>A boolean indicating whether or not the string is a url.</returns>
		public static bool IsValidUrl(this string input)
		{
			return Uri.TryCreate(input, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
		}
		/// <summary>
		/// Returns true if the passed in uri is a valid url.
		/// </summary>
		/// <param name="uri">The uri to evaluate.</param>
		/// <returns>A boolean indicating whether or not the uri is a url.</returns>
		public static bool IsValidUrl(this Uri uri)
		{
			return uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
		}
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
		{
			return !str.Any(x => x > limit);
		}
		/// <summary>
		/// Returns the count of characters equal to \r or \n.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static int CountLineBreaks(this string str)
		{
			return str?.Count(x => x == '\r' || x == '\n') ?? 0;
		}
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
				var width = tags.Where(x => x.Name == "Image Width").Select(x => Convert.ToInt32(x.Description.Split(' ')[0]));
				var height = tags.Where(x => x.Name == "Image Height").Select(x => Convert.ToInt32(x.Description.Split(' ')[0]));
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
		public static string[] SplitLikeCommandLine(this string input)
		{
			return input.ComplexSplit(new[] { ' ' }, new[] { '"' }, removeQuotes: true);
		}
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
		public static string[] ComplexSplit(this string input, char[] seperators, char[] quotes, bool removeQuotes)
		{
			if (seperators == null)
			{
				throw new ArgumentException($"{nameof(seperators)} cannot be null.", nameof(seperators));
			}
			if (quotes == null)
			{
				throw new ArgumentException($"{nameof(quotes)} cannot be null.", nameof(quotes));
			}

			var parts = new List<string>();
			var inside = false; //Whether or not the current part is inside an ignored
			var part = new ComplexSplitPart(removeQuotes);
			for (int i = 0; i < input.Length; ++i)
			{
				var c = input[i];
				var q = quotes.Contains(c);
				var s = seperators.Contains(c);
				var e = i > 0 && input[i - 1] == '\\';

				if (q && !e)
				{
					inside = !inside;
					part.AddQuoteChar(c);
					continue;
				}
				if (s && !inside)
				{
					parts.Add(part.ToString());
					part.Clear();
					continue;
				}
				part.AddChar(c);
			}
			parts.Add(part.ToString());
			return parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
		}
		/// <summary>
		/// Orders an <see cref="IEnumerable{T}"/> by something that does not implement <see cref="IComparable"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="input">The objects to order.</param>
		/// <param name="keySelector">The property to order by.</param>
		/// <returns>An ordered enumerable.</returns>
		public static IEnumerable<T> OrderByNonComparable<T, TKey>(this IEnumerable<T> input, Func<T, TKey> keySelector)
		{
			return input.GroupBy(keySelector).SelectMany(x => x);
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
			return source.Select((obj, index) => new { Obj = obj, Index = index })
				.GroupBy(x => x.Index / groupSize)
				.Select(g => g.Select(o => o.Obj));
		}
		/// <summary>
		/// Adds all of the collection to the list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="collection"></param>
		public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if (list is List<T> concrete)
			{
				concrete.AddRange(collection);
			}
			else
			{
				foreach (var item in collection)
				{
					list.Add(item);
				}
			}
		}
		/// <summary>
		/// Removes elements which match the supplied predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="match"></param>
		public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			if (list is List<T> concrete)
			{
				return concrete.RemoveAll(match);
			}
			else
			{
				var removedCount = 0;
				for (int i = list.Count - 1; i >= 0; --i)
				{
					if (match(list[i]))
					{
						list.RemoveAt(i);
						++removedCount;
					}
				}
				return removedCount;
			}
		}
		/// <summary>
		/// Gets the specified attribute from the supplied attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="attrs"></param>
		/// <returns></returns>
		public static T GetAttribute<T>(this IEnumerable<Attribute> attrs)
		{
			foreach (var attr in attrs)
			{
				if (attr is T t)
				{
					return t;
				}
			}
			return default;
		}
		/// <summary>
		/// Tests whether the specified generic type is a parent of the current type or is the current type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool InheritsFromGeneric(this Type type, Type c)
		{
			if (!c.IsGenericType)
			{
				throw new ArgumentException($"Use {nameof(Type.IsAssignableFrom)} rather than this method if passing in a non generic type.");
			}
			if (type == typeof(object))
			{
				return false;
			}
			if (type == c || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(c)))
			{
				return true;
			}
			return type.BaseType.InheritsFromGeneric(c);
		}
		/// <summary>
		/// Short way to write ConfigureAwait(false).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="task"></param>
		/// <returns></returns>
		public static ConfiguredTaskAwaitable<T> CAF<T>(this Task<T> task)
		{
			return task.ConfigureAwait(false);
		}
		/// <summary>
		/// Short way to write ConfigureAwait(false).
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static ConfiguredTaskAwaitable CAF(this Task task)
		{
			return task.ConfigureAwait(false);
		}
	}
}