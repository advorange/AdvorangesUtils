using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvorangesUtils
{
	/// <summary>
	/// Utilities for getting the flags of an enum.
	/// </summary>
	public static class EnumUtils
	{
		private delegate T ModifyEnum<T>(ref T value, object add);
		private static readonly ModifyEnum<ulong> _ModifyUlongEnum = new ModifyEnum<ulong>((ref ulong a, object b) => a |= Convert.ToUInt64(b));
		private static readonly ModifyEnum<long> _ModifyLongEnum = new ModifyEnum<long>((ref long a, object b) => a |= Convert.ToInt64(b));

		/// <summary>
		/// Converts an enum to the names of the flags it contains.
		/// </summary>
		/// <param name="value">The instance value of the enum.</param>
		/// <param name="useFlagsGottenFromString">Whether or not to use the string representation for checking.
		/// This will not catch every flag, especially if some flags are other flags ORed together.</param>
		/// <returns>The names of the flags <paramref name="value"/> contains.</returns>
		public static IEnumerable<string> GetFlagNames(Enum value, bool useFlagsGottenFromString = false)
		{
			//If the enum ToString is different from its value ToString then that means it's a valid flags enum
			if (useFlagsGottenFromString)
			{
				var str = value.ToString();
				if (str != Convert.ChangeType(value, value.GetTypeCode()).ToString())
				{
					foreach (var name in str.Split(',', '|').Select(x => x.Trim()))
					{
						yield return name;
					}
					yield break;
				}
			}

			//Loop through every possible flag from the enum's values to see which ones match
			var type = value.GetType();
			foreach (Enum e in Enum.GetValues(type))
			{
				if (value.HasFlag(e))
				{
					yield return Enum.GetName(type, e);
				}
			}
		}
		/// <summary>
		/// Converts an enum to the flags it contains.
		/// </summary>
		/// <typeparam name="TEnum">The type of enum to get the flags of.</typeparam>
		/// <param name="value">The instance value of the enum.</param>
		/// <param name="useFlagsGottenFromString">Whether or not to use the string representation for checking.
		/// This will not catch every flag, especially if some flags are other flags ORed together.</param>
		/// <returns>The flags <paramref name="value"/> contains.</returns>
		public static IEnumerable<TEnum> GetFlags<TEnum>(TEnum value, bool useFlagsGottenFromString = false) where TEnum : struct, Enum
		{
			//If the enum ToString is different from its value ToString then that means it's a valid flags enum
			if (useFlagsGottenFromString)
			{
				var str = value.ToString();
				if (str != Convert.ChangeType(value, value.GetTypeCode()).ToString())
				{
					foreach (var name in str.Split(',', '|').Select(x => x.Trim()))
					{
						if (Enum.TryParse(name, out TEnum result))
						{
							yield return result;
						}
					}
					yield break;
				}
			}

			//Loop through every possible flag from the enum's values to see which ones match
			var type = value.GetType();
			foreach (Enum e in Enum.GetValues(type))
			{
				if (value.HasFlag(e))
				{
					yield return (TEnum)e;
				}
			}
		}
		/// <summary>
		/// Converts an enum to the flags it contains.
		/// </summary>
		/// <param name="value">The instance value of the enum.</param>
		/// <param name="useFlagsGottenFromString">Whether or not to use the string representation for checking.
		/// This will not catch every flag, especially if some flags are other flags ORed together.</param>
		/// <returns>The flags <paramref name="value"/> contains.</returns>
		public static IEnumerable<Enum> GetFlags(Enum value, bool useFlagsGottenFromString = false)
		{
			//If the enum ToString is different from its value ToString then that means it's a valid flags enum
			var type = value.GetType();
			if (useFlagsGottenFromString)
			{
				var str = value.ToString();
				if (str != Convert.ChangeType(value, value.GetTypeCode()).ToString())
				{
					foreach (var name in str.Split(',', '|').Select(x => x.Trim()))
					{
						yield return (Enum)Enum.Parse(type, name);
					}
					yield break;
				}
			}

			//Loop through every possible flag from the enum's values to see which ones match
			foreach (Enum e in Enum.GetValues(type))
			{
				if (value.HasFlag(e))
				{
					yield return e;
				}
			}
		}
		/// <summary>
		/// Attempts to parse enums from the supplied values.
		/// </summary>
		/// <typeparam name="TEnum">The enum to parse.</typeparam>
		/// <param name="input">The input names.</param>
		/// <param name="valid">The valid enums.</param>
		/// <param name="invalid">The invalid names.</param>
		/// <returns>A boolean indicating if there were any failed parses.</returns>
		public static bool TryParseMultiple<TEnum>(IEnumerable<string> input, out IReadOnlyCollection<TEnum> valid, out IReadOnlyCollection<string> invalid) where TEnum : struct, Enum
		{
			var tempValid = new List<TEnum>();
			var tempInvalid = new List<string>();
			foreach (var enumName in input)
			{
				if (Enum.TryParse(enumName, true, out TEnum result))
				{
					tempValid.Add(result);
				}
				else
				{
					tempInvalid.Add(enumName);
				}
			}
			valid = tempValid;
			invalid = tempInvalid;
			return invalid.Count == 0;
		}
		/// <summary>
		/// Attempts to parse all enums then OR them together.
		/// </summary>
		/// <typeparam name="TEnum">The enum to parse.</typeparam>
		/// <param name="input">The input names.</param>
		/// <param name="value">The return value of every valid enum ORed together.</param>
		/// <param name="invalid">The invalid names.</param>
		/// <returns>A boolean indicating if there were any failed parses.</returns>
		public static bool TryParseFlags<TEnum>(IEnumerable<string> input, out TEnum value, out IReadOnlyCollection<string> invalid) where TEnum : struct, Enum
		{
			var tempInvalid = new List<string>();
#if false //Method using dynamic
			var temp = new TEnum();
			foreach (var enumName in input)
			{
				if (Enum.TryParse(enumName, true, out TEnum result))
				{
					//Cast as dynamic so bitwise functions can be done on it
					temp |= (dynamic)result; //Doesn't work anymore?
				}
				else
				{
					tempInvalid.Add(enumName);
				}
			}
			value = temp;
#else //Method not using dynamic
			T AddAllEnumValues<T>(ref T val, ModifyEnum<T> modifyVal)
			{
				foreach (var enumName in input)
				{
					if (!Enum.TryParse(enumName, true, out TEnum result))
					{
						tempInvalid.Add(enumName);
						continue;
					}
					modifyVal(ref val, result);
				}
				return val;
			}

			object temp;
			switch (Type.GetTypeCode(typeof(TEnum)))
			{
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					ulong unsigned = 0;
					temp = AddAllEnumValues(ref unsigned, _ModifyUlongEnum);
					break;
				default:
					long signed = 0;
					temp = AddAllEnumValues(ref signed, _ModifyLongEnum);
					break;
			}
			value = (TEnum)Enum.Parse(typeof(TEnum), temp.ToString());
#endif
			invalid = tempInvalid;
			return invalid.Count == 0;
		}
	}
}
