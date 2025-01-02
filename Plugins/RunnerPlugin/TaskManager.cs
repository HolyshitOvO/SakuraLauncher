using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RunnerPlugin
{
	public class TaskManager
	{
		private readonly ConcurrentDictionary<string, (Task Task, CancellationTokenSource TokenSource)> _tasks = new ConcurrentDictionary<string, (Task Task, CancellationTokenSource TokenSource)>();

		/// <summary>
		/// 启动一个可取消的任务
		/// </summary>
		/// <param name="taskName">任务名称，用于标识任务</param>
		/// <param name="action">任务执行的逻辑</param>
		public void StartTask(string taskName, Action<CancellationToken> action)
		{
			// 如果任务已存在，先取消它
			CancelTask(taskName);

			// 创建新的取消令牌
			var cts = new CancellationTokenSource();
			var token = cts.Token;

			// 创建并存储任务
			var task = Task.Run(() => action(token), token);
			_tasks[taskName] = (task, cts);
		}

		/// <summary>
		/// 取消指定任务
		/// </summary>
		/// <param name="taskName">任务名称</param>
		public void CancelTask(string taskName)
		{
			if (_tasks.TryRemove(taskName, out var taskInfo))
			{
				taskInfo.TokenSource.Cancel(); // 取消任务
				taskInfo.TokenSource.Dispose(); // 释放资源
												// 不需要等待任务完成
			}
		}

		/// <summary>
		/// 检查任务是否正在运行
		/// </summary>
		/// <param name="taskName">任务名称</param>
		/// <returns>任务是否运行中</returns>
		public bool IsTaskRunning(string taskName)
		{
			return _tasks.ContainsKey(taskName);
		}

		/// <summary>
		/// 取消所有任务
		/// </summary>
		public void CancelAllTasks()
		{
			foreach (var taskName in _tasks.Keys)
			{
				CancelTask(taskName);
			}
		}
	}

}
