using System;
using System.Text.RegularExpressions;

namespace AdvorangesUtils
{
	/// <summary>
	/// A fix to an invalid value in Json.
	/// </summary>
	public struct JsonFix
	{
		/// <summary>
		/// The type to apply this fix to.
		/// </summary>
		public Type Type;
		/// <summary>
		/// The Json path to the value.
		/// </summary>
		public string Path;
		/// <summary>
		/// Regex for checking if any values are invalid.
		/// </summary>
		public Regex[] ErrorValues;
		/// <summary>
		/// The value to replace with.
		/// </summary>
		public string NewValue;
	}
}