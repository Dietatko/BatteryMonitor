using System;
using System.Threading;
using System.Threading.Tasks;
using ImpruvIT.Contracts;

namespace ImpruvIT.Threading
{
	/// <summary>
	/// A repeatable task that is executed regularly (time based) and/or by manual trigger.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class RepeatableTask : IDisposable
	{
		//private readonly ITracer tracer;
		private readonly object syncLock;
		private readonly AutoResetEvent triggerEvent;
		private readonly Action taskBody;
		private CancellationTokenSource cancelSource;
		private Task task;

		/// <summary>
		/// Initializes a new instance of the <see cref="RepeatableTask"/> class.
		/// </summary>
		/// <param name="taskBody">The task body.</param>
		/// <param name="threadName">Name of the thread for easy debugging.</param>
		public RepeatableTask(Action taskBody, string threadName = null)
		{
			Contract.Requires(taskBody, "taskBody").NotToBeNull();

			//this.tracer = Tracer.Create(this.GetType());
			syncLock = new object();
			triggerEvent = new AutoResetEvent(false);
			this.taskBody = taskBody;
			ThreadName = threadName;
		}

		/// <summary>
		/// Gets or sets the name of the thread.
		/// </summary>
		public string ThreadName { get; }

		/// <summary>
		/// Gets or sets how often is the task triggered.
		/// </summary>
		public TimeSpan MinTriggerTime { get; set; }
		
		/// <summary>
		/// Gets or sets the time how long to wait for manual triggers until the task is triggered. This allows to group several manual triggers to one task execution.
		/// </summary>
		public TimeSpan ThrottleTime { get; set; }

		/// <summary>
		/// Gets a value indicating whether the task is automatically and/or can be manually triggered. Does not need to mean that the task body is executing.
		/// </summary>
		public bool IsRunning => task != null;

		/// <summary>
		/// Starts the repeatable task.
		/// </summary>
		public void Start()
		{
			lock (syncLock)
			{
				if (IsRunning)
					return;

				triggerEvent.Reset();
				cancelSource = new CancellationTokenSource();
				task = Task.Factory.StartNew(ThreadBody, cancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			}
		}

		/// <summary>
		/// Stops the repeatable task. Method waits until current run of task body is finished before stopping the repeatable task.
		/// </summary>
		public void Stop()
		{
			lock (syncLock)
			{
				if (!IsRunning)
					return;

				// Request cancellation
				cancelSource.Cancel();

				// Cleanup task
				try
				{
					task.Wait();
				}
				catch (AggregateException aggregateException)
				{
					aggregateException.Flatten().Handle(ex => ex is TaskCanceledException);
				}
				task.Dispose();
				task = null;

				// Cleanup cancellation token
				cancelSource.Dispose();
				cancelSource = null;
			}
		}

		/// <summary>
		/// Manually triggers the task body execution.
		/// </summary>
		public void Trigger()
		{
			try
			{
				triggerEvent.Set();
			}
			catch (ObjectDisposedException)
			{
				//this.tracer.Write(Category.Error, "Unable to trigger because the object was already disposed!", new object[0]);

				// Intentionally swallowed
			}
		}

		// ReSharper disable FunctionNeverReturns - An exception is thrown if cancelled
		private void ThreadBody()
		{
			// Set thread name for easier identification of the thread
			if (Thread.CurrentThread.Name == null && ThreadName != null)
				Thread.CurrentThread.Name = ThreadName;

			// Start the end-less loop
			var handles = new[] { triggerEvent, cancelSource.Token.WaitHandle };
			while (true)
			{
				// Check whether cancellation was requested
				cancelSource.Token.ThrowIfCancellationRequested();

				// Wait for more triggers 
				TimeSpan throttleTime = ThrottleTime;
				if (throttleTime > TimeSpan.Zero)
				{
					Thread.Sleep(throttleTime);
					triggerEvent.Reset();
				}

				// Execute action
				try
				{
					taskBody();
				}
				catch (Exception)
				{
					// TODO: improve exception handling

					//this.tracer.Write(Category.Error, ex, "An error occurred in repeatable task thread while executing the repeatable action! Retriggering ...", new object[0]);
					Trigger();
				}

				// Determine max wait time
				TimeSpan waitTime = MinTriggerTime;
				if (waitTime <= TimeSpan.Zero)
					waitTime = TimeSpan.FromMilliseconds(-1);

				// Wait until any of:
				//    - Next execution is explicitly triggered
				//    - Cancellation request
				//    - Min period elapses
				WaitHandle.WaitAny(handles, waitTime);
			}
		}
		// ReSharper restore FunctionNeverReturns


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!disposing) 
				return;
			
			Stop();
			triggerEvent.Dispose();
			cancelSource?.Dispose();
		}
	}
}
