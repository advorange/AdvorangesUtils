using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AdvorangesUtils
{
	/// <summary>
	/// Actions involving saving and loading or memory.
	/// </summary>
	public static class IOUtils
	{
		private static JsonSerializerSettings _DefaultSerializingSettings = GenerateDefaultSerializerSettings();

		/// <summary>
		/// Returns the <see cref="Process.PrivateMemorySize64"/> value divided by a MB.
		/// </summary>
		/// <returns>The amount of private memory in MBs.</returns>
		public static double GetMemory()
		{
			using (var process = Process.GetCurrentProcess())
			{
				process.Refresh();
				return process.PrivateMemorySize64 / (1024.0 * 1024.0);
			}
		}
		/// <summary>
		/// Returns a hashed stream for file duplication checking.
		/// </summary>
		/// <param name="s">The stream to hash.</param>
		/// <returns>A string representing a hashed stream.</returns>
		public static string GetMD5Hash(this Stream s)
		{
			s.Seek(0, SeekOrigin.Begin);
			using (var algorithm = MD5.Create())
			{
				var computed = algorithm.ComputeHash(s);
				s.Seek(0, SeekOrigin.Begin);
				return BitConverter.ToString(computed).Replace("-", "").ToLower();
			}
		}
		/// <summary>
		/// Creates a file if it does not already exist, then writes over it.
		/// </summary>
		/// <param name="fileInfo">The file to overwrite.</param>
		/// <param name="text">The text to write.</param>
		public static void OverwriteFile(FileInfo fileInfo, string text)
		{
			File.WriteAllText(fileInfo.FullName, text);
		}
		/// <summary>
		/// Converts the object to json.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <returns>The serialized object.</returns>
		public static string Serialize(object obj, JsonSerializerSettings settings = null)
		{
			return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, settings ?? _DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object of type <typeparamref name="T"/> with the supplied string and type.
		/// </summary>
		/// <typeparam name="T">The general type to deserialize. Can be an abstraction of <paramref name="type"/> but has to be a type where it can be converted to <typeparamref name="T"/>.</typeparam>
		/// <param name="value">The text to deserialize.</param>
		/// <param name="type">The explicit type of object to create.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <param name="fixes">Fixes for Json with errors.</param>
		/// <returns>The value put into an object.</returns>
		public static T Deserialize<T>(string value, Type type, JsonSerializerSettings settings = null, JsonFix[] fixes = null)
		{
			//If no fixes then deserialize normally
			if (fixes == null)
			{
				return (T)JsonConvert.DeserializeObject(value, type, settings ?? _DefaultSerializingSettings);
			}
			//Only use fixes specified for the class
			var typeFixes = fixes.Where(f => f.Type == type || f.Type.IsAssignableFrom(type));
			if (!typeFixes.Any())
			{
				return (T)JsonConvert.DeserializeObject(value, type, settings ?? _DefaultSerializingSettings);
			}

			var jObj = JObject.Parse(value);
			foreach (var fix in typeFixes)
			{
				if (jObj.SelectToken(fix.Path)?.Parent is JProperty jProp && fix.ErrorValues.Any(x => x.IsMatch(jProp.Value.ToString())))
				{
					jProp.Value = fix.NewValue;
				}
			}
			return (T)JsonConvert.DeserializeObject(jObj.ToString(), type, settings ?? _DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object from json stored in a file.
		/// By default will ignore any fields/propties deserializing with errors and parses enums as strings.
		/// </summary>
		/// <typeparam name="T">The general type to deserialize. Can be an abstraction of <paramref name="type"/> but has to be a type where it can be converted to <typeparamref name="T"/>.</typeparam>
		/// <param name="file">The file to read from.</param>
		/// <param name="type">The explicit type of object to create.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <param name="fixes">Fixes for Json with errors.</param>
		/// <returns>The file's text put into the object, or, if deserialization failed, default.</returns>
		public static T DeserializeFromFile<T>(FileInfo file, Type type, JsonSerializerSettings settings = null, JsonFix[] fixes = null) where T : new()
		{
			if (file.Exists)
			{
				try
				{
					using (var reader = new StreamReader(file.FullName))
					{
						return Deserialize<T>(reader.ReadToEnd(), type, settings ?? _DefaultSerializingSettings);
					}
				}
				catch (JsonReaderException jre)
				{
					jre.Write();
				}
			}
			return new T();
		}
		/// <summary>
		/// Generates json serializer settings which ignore most errors, and has a string enum converter.
		/// </summary>
		/// <returns></returns>
		public static JsonSerializerSettings GenerateDefaultSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				//Ignores errors parsing specific invalid properties instead of throwing exceptions making the entire object null
				//Will still make the object null if the property's type is changed to something not creatable from the text
				Error = (sender, e) =>
				{
					ConsoleUtils.WriteLine(e.ErrorContext.Error.Message, ConsoleColor.Red);
					e.ErrorContext.Handled = false;
				},
				Converters = new JsonConverter[] { new StringEnumConverter() }
			};
		}
		/// <summary>
		/// Writes an uncaught exception to a log file in the current directory.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="logFile">The file to log to. If this remains null, will log to the current directory.</param>
		public static void LogUncaughtException(object exception, FileInfo logFile = null)
		{
			var file = logFile ?? new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "CrashLog.txt"));
			//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
			using (var writer = file.AppendText())
			{
				writer.WriteLine($"{DateTime.UtcNow.ToReadable()}: {exception}\n");
			}

			ConsoleUtils.WriteLine($"Something has gone drastically wrong. Check {file} for more details.", ConsoleColor.Red);
		}

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
}