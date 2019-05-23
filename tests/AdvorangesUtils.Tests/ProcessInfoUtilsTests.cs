using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesUtils.Tests
{
	[TestClass]
	public class ProcessInfoUtilsTests
	{
		[TestMethod]
		public void StartTime_Test()
		{
			var runs = TestSpeeds(ProcessInfoUtils.GetStartTime);
		}
		[TestMethod]
		public void Uptime_Test()
		{
			var runs = TestSpeeds(ProcessInfoUtils.GetUptime);
		}
		[TestMethod]
		public void ThreadCount_Test()
		{
			void MakeNewThread() => new Thread(() => Thread.Sleep(5000)).Start();
			var runs = TestSpeeds(ProcessInfoUtils.GetThreadCount, MakeNewThread);
		}
		[TestMethod]
		public void Memory_Test()
		{
			var runs = TestSpeeds(ProcessInfoUtils.GetMemory);
		}
		[TestMethod]
		public void MemoryMB_Test()
		{
			var runs = TestSpeeds(ProcessInfoUtils.GetMemoryMB);
		}
		[TestMethod]
		public void ProcessId_Test()
		{
			var runs = TestSpeeds(ProcessInfoUtils.GetProcessId);
		}

		private (long Ticks, T Value)[] TestSpeeds<T>(Func<T> func, Action doBefore = null, int runs = 5)
		{
			var info = new (long Ticks, T Value)[runs];
			for (var i = 0; i < runs; ++i)
			{
				doBefore?.Invoke();
				var sw = new Stopwatch();
				sw.Start();
				var val = func();
				sw.Stop();
				info[i] = (sw.ElapsedTicks, val);
			}
			return info;
		}
	}
}
