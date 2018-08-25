using System;
using System.Diagnostics;

namespace AdvorangesUtils
{
	/// <summary>
	/// Acts as a wrapped around <see cref="Process.GetCurrentProcess()"/>.
	/// </summary>
	public static class ProcessInfoUtils
	{
		private static readonly Process _Proc = Process.GetCurrentProcess();

		/// <summary>
		/// Returns the start time of this process in UTC.
		/// </summary>
		/// <returns>The time at which this process started in UTC.</returns>
		public static DateTime GetStartTime()
		{
			return _Proc.StartTime.ToUniversalTime();
		}
		/// <summary>
		/// Returns how long this process has been running.
		/// </summary>
		/// <returns>How long this process has been running.</returns>
		public static TimeSpan GetUptime()
		{
			return DateTime.UtcNow - GetStartTime();
		}
		/// <summary>
		/// Returns how many threads this process is currently using.
		/// </summary>
		/// <returns>The amount of threads this process is currently using.</returns>
		/// <remarks>This method relies on <see cref="Process.Refresh"/> so it is much slower than others.</remarks>
		public static int GetThreadCount()
		{
			_Proc.Refresh();
			return _Proc.Threads.Count;
		}
		/// <summary>
		/// Returns how much memory this process is using in <see cref="Process.PrivateMemorySize64"/>.
		/// </summary>
		/// <returns>The amount of bytes this process is using.</returns>
		/// <remarks>This method relies on <see cref="Process.Refresh"/> so it is much slower than others.</remarks>
		public static double GetMemory()
		{
			_Proc.Refresh();
			return _Proc.PrivateMemorySize64;
		}
		/// <summary>
		/// Returns how much memory in MB this process is using in <see cref="Process.PrivateMemorySize64"/>.
		/// </summary>
		/// <returns>The amount of MB this process is using.</returns>
		/// <remarks>This method relies on <see cref="Process.Refresh"/> so it is much slower than others.</remarks>
		public static double GetMemoryMB()
		{
			return GetMemory() / (1024.0 * 1024.0);
		}
		/// <summary>
		/// Gets the id of this process.
		/// </summary>
		/// <returns>The id of this process.</returns>
		public static int GetProcessId()
		{
			return _Proc.Id;
		}
	}
}