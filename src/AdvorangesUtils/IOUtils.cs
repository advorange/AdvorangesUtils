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
	/// Actions involving saving and loading.
	/// </summary>
	public static class IOUtils
	{
		//Has to be manually set, but that shouldn't be a problem since the break would have been manually created anyways
		public static JsonFix[] Fixes = new JsonFix[0];
		private static JsonSerializerSettings _DefaultSerializingSettings = GenerateDefaultSerializerSettings();

		/// <summary>
		/// Returns the <see cref="Process.WorkingSet64"/> value divided by a MB.
		/// </summary>
		/// <returns></returns>
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
		/// <param name="fileInfo"></param>
		/// <param name="text"></param>
		public static void OverwriteFile(FileInfo fileInfo, string text)
		{
			File.WriteAllText(fileInfo.FullName, text);
		}
		/// <summary>
		/// Converts the object to json.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static string Serialize(object obj, JsonSerializerSettings settings = null)
		{
			return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, settings ?? _DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object of type <typeparamref name="T"/> with the supplied string and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static T Deserialize<T>(string value, Type type, JsonSerializerSettings settings = null)
		{
			//Only use fixes specified for the class
			var fixes = Fixes.Where(f => f.Type == type || f.Type.IsAssignableFrom(type));
			if (fixes.Any())
			{
				var jObject = JObject.Parse(value);
				foreach (var fix in fixes)
				{
					if (jObject.SelectToken(fix.Path)?.Parent is JProperty jProp && fix.ErrorValues.Any(x => x.IsMatch(jProp.Value.ToString())))
					{
						jProp.Value = fix.NewValue;
					}
				}
				value = jObject.ToString();
			}
			return (T)JsonConvert.DeserializeObject(value, type, settings ?? _DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object from json stored in a file.
		/// By default will ignore any fields/propties deserializing with errors and parses enums as strings.
		/// </summary>
		/// <typeparam name="T">The general type to deserialize. Can be an abstraction of <paramref name="type"/> but has to be a type where it can be converted to <typeparamref name="T"/>.</typeparam>
		/// <param name="file">The file to read from.</param>
		/// <param name="type">The explicit type of object to create.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <param name="create">If true, unable to deserialize an object from the file, and the type has a parameterless constructor, then uses that constructor.</param>
		/// <param name="callback">An action to do after the object has been deserialized.</param>
		/// <returns></returns>
		public static T DeserializeFromFile<T>(FileInfo file, Type type, bool create = false, JsonSerializerSettings settings = null)
		{
			T obj = default;
			var isDefault = true;
			if (file.Exists)
			{
				try
				{
					using (var reader = new StreamReader(file.FullName))
					{
						obj = Deserialize<T>(reader.ReadToEnd(), type, settings ?? _DefaultSerializingSettings);
						isDefault = false;
					}
				}
				catch (JsonReaderException jre)
				{
					jre.Write();
				}
			}
			//Create obj is still default and parameterless constructor exists
			return create && isDefault && type.GetConstructors().Any(x => !x.GetParameters().Any())
				? (T)Activator.CreateInstance(type) : obj;
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
		/// Writes an uncaught exception to a log file.
		/// </summary>
		/// <param name="exception"></param>
		public static void LogUncaughtException(object exception)
		{
			var file = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "CrashLog.txt"));
			//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
			using (var writer = file.AppendText())
			{
				writer.WriteLine($"{DateTime.UtcNow.ToReadable()}: {exception}\n");
			}

			ConsoleUtils.WriteLine($"Something has gone drastically wrong. Check {file} for more details.", ConsoleColor.Red);
		}

		public struct JsonFix
		{
			public Type Type;
			public string Path;
			public Regex[] ErrorValues;
			public string NewValue;
		}
	}
}