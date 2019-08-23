using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvorangesUtils
{
	/// <summary>
	/// Methods which combine other <see cref="System.Linq"/> methods.
	/// </summary>
	public static class LinqUtils
	{
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
		/// Returns all non null values of <paramref name="source"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
			=> source.Where(x => x != null);
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
	}
}