using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
	internal class DiagnosticsLogger : IDisposable
	{
		private readonly string _filePath;
		private readonly ConcurrentQueue<string> _logQueue = new();
		private readonly CancellationTokenSource _cts = new();
		private readonly Task _loggingTask;

		public DiagnosticsLogger(string filePath = "diagnostics.txt")
		{
			_filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);

			try
			{
				using (FileStream fs = File.Create(_filePath))
				{
				}
			}
			catch (IOException)
			{
			}

			_loggingTask = Task.Run(ProcessQueue);
		}

		public void LogBallState(int ballId, IVector position, IVector velocity)
		{
			string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
			string logEntry = $"[{timeStamp}] Ball ID: {ballId} | Pos: ({position.x:F2}, {position.y:F2}) | Vel: ({velocity.x:F2}, {velocity.y:F2})";
			_logQueue.Enqueue(logEntry);
		}

		private async Task ProcessQueue()
		{
			while (!_cts.Token.IsCancellationRequested || !_logQueue.IsEmpty)
			{
				if (_logQueue.TryDequeue(out string log))
				{
					try
					{
						using (StreamWriter sw = new StreamWriter(_filePath, true, Encoding.UTF8))
						{
							await sw.WriteLineAsync(log);
						}
					}
					catch (IOException)
					{
						// retry
						_logQueue.Enqueue(log);
						await Task.Delay(10);
					}
				}
				else
				{
					await Task.Delay(5);
				}
			}
		}

		public void Dispose()
		{
			_cts.Cancel();
			try
			{
				_loggingTask.Wait(500);
			}
			catch { }
			_cts.Dispose();
		}
	}
}