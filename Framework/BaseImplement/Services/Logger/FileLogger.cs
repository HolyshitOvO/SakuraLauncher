using CandyLauncher.Abstraction.Services;
using System.IO;
using System.Threading.Tasks;

namespace CandyLauncher.Implementation.Services.Logger
{
	internal static class FileLogger
	{
		public static bool Initialized { get; private set; } = false;
		public static bool Disposed { get; private set; } = false;

		public static FileInfo LogFile { get; private set; }
		private static StreamWriter writer;
		private static Stream stream;
		private static readonly object lockObj = new object();  // 添加锁对象用于同步

		public static void Initialize(ICurrentEnvironment env)
		{
			if (Initialized)
				return;

			string filePath = Path.Combine(env.LogDirectory.FullName, "logs.txt");
			LogFile = new FileInfo(filePath);
			if (!LogFile.Exists)
			{
				stream = File.Create(filePath);
				stream.Flush();
				stream.Dispose();
				LogFile = new FileInfo(filePath);
			}
			stream = LogFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
			writer = new StreamWriter(stream);
			Initialized = true;
		}

		public static void Dispose()
		{
			if (Disposed)
				return;

			// 确保在Dispose时只操作一次
			lock (lockObj)
			{
				if (writer != null)
				{
					writer.Flush();
					writer.Close();
					writer.Dispose();
				}
				if (stream != null)
				{
					stream.Close();
					stream.Dispose();
				}
				Disposed = true;
			}
		}

		private static int flushCountdown = 1;

		public static Task LogAsync(string message)
		{
			// 如果流被处置了，就不要继续写入
			if (Disposed)
				return Task.FromResult(0);

			flushCountdown--;
			if (flushCountdown <= 0)
			{
				flushCountdown = 1;
				return WriteAsync(message);
			}
			else
			{
				return WriteAsync(message);
			}
		}

		private static Task WriteAsync(string message)
		{
			// 使用锁定机制避免并发问题
			lock (lockObj)
			{
				if (writer != null && !Disposed)
				{
					return writer.WriteLineAsync(message).ContinueWith(tsk =>
					{
						lock (lockObj)
						{
							if (writer != null && !Disposed)
								writer.Flush();
						}
					});
				}
				else
				{
					return Task.FromResult(0);
				}
			}
		}
	}
}
