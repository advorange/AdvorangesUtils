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
		/// <summary>
		/// The serializer settings to use by default.
		/// </summary>
		public static JsonSerializerSettings DefaultSerializingSettings { get; set; } = new JsonSerializerSettings
		{
			Converters = new[] { new StringEnumConverter() }
		};

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
		/// Will not seek on the stream before or after hashing.
		/// </summary>
		/// <param name="s">The stream to hash.</param>
		/// <returns>A string representing a hashed stream.</returns>
		public static string GetMD5Hash(this Stream s)
		{
			using (var md5 = MD5.Create())
			{
				return BitConverter.ToString(md5.ComputeHash(s)).Replace("-", "").ToLower();
			}
		}
		/// <summary>
		/// Converts the object to json.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <returns>The serialized object.</returns>
		public static string Serialize(object obj, JsonSerializerSettings settings = null)
		{
			return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, settings ?? DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object of type <typeparamref name="TAbstraction"/> with the supplied string and type.
		/// </summary>
		/// <typeparam name="TAbstraction">The abstracted type to deserialize to. Can be the same as <typeparamref name="TImplementation"/>.</typeparam>
		/// <typeparam name="TImplementation">The actual implementation of the type.</typeparam>
		/// <param name="value">The text to deserialize.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <param name="fixes">Fixes for Json with errors.</param>
		/// <returns>The value put into an object.</returns>
		public static TAbstraction Deserialize<TAbstraction, TImplementation>(string value, 
			JsonSerializerSettings settings = null, JsonFix[] fixes = null) where TImplementation : TAbstraction
		{
			//If no fixes then deserialize normally
			if (fixes == null)
			{
				return JsonConvert.DeserializeObject<TImplementation>(value, settings ?? DefaultSerializingSettings);
			}
			//Only use fixes specified for the class
			var typeFixes = fixes.Where(f => f.Type == typeof(TImplementation) || f.Type.IsAssignableFrom(typeof(TImplementation)));
			if (!typeFixes.Any())
			{
				return JsonConvert.DeserializeObject<TImplementation>(value, settings ?? DefaultSerializingSettings);
			}

			var jObj = JObject.Parse(value);
			foreach (var fix in typeFixes)
			{
				if (jObj.SelectToken(fix.Path)?.Parent is JProperty jProp && fix.ErrorValues.Any(x => x.IsMatch(jProp.Value.ToString())))
				{
					jProp.Value = fix.NewValue;
				}
			}
			return JsonConvert.DeserializeObject<TImplementation>(jObj.ToString(), settings ?? DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object from json stored in a file.
		/// By default will ignore any fields/propties deserializing with errors and parses enums as strings.
		/// </summary>
		/// <typeparam name="TAbstraction">The abstracted type to deserialize to. Can be the same as <typeparamref name="TImplementation"/>.</typeparam>
		/// <typeparam name="TImplementation">The actual implementation of the type.</typeparam>
		/// <param name="file">The file to read from.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <param name="fixes">Fixes for Json with errors.</param>
		/// <returns>The file's text put into the object, or, if deserialization failed, default.</returns>
		public static TAbstraction DeserializeFromFile<TAbstraction, TImplementation>(FileInfo file,
			JsonSerializerSettings settings = null, JsonFix[] fixes = null) where TImplementation : TAbstraction, new()
		{
			if (file.Exists)
			{
				try
				{
					using (var reader = new StreamReader(file.FullName))
					{
						return Deserialize<TAbstraction, TImplementation>(reader.ReadToEnd(), settings ?? DefaultSerializingSettings, fixes);
					}
				}
				catch (JsonReaderException jre)
				{
					jre.Write();
				}
			}
			return new TImplementation();
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