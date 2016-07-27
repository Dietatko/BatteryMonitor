using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor
{
	public class RepeatableTask : IDisposable
	{
		//private readonly ITracer tracer;
		private readonly object m_syncLock;
		private readonly AutoResetEvent m_triggerEvent;
		private readonly Action m_taskBody;
		private CancellationTokenSource m_cancelSource;
		private Task m_task;

		public RepeatableTask(Action taskBody)
			: this(taskBody, null)
		{
		}

		public RepeatableTask(Action taskBody, string threadName)
		{
			Contract.Requires(taskBody, "taskBody").IsNotNull();

			//this.tracer = Tracer.Create(this.GetType());
			this.m_syncLock = new object();
			this.m_triggerEvent = new AutoResetEvent(false);
			this.m_taskBody = taskBody;
			this.ThreadName = threadName;
		}

		public string ThreadName { get; set; }
		public TimeSpan MinTriggerTime { get; set; }
		public TimeSpan ThrottleTime { get; set; }

		public bool IsRunning
		{
			get { return this.m_task != null; }
		}

		public void Start()
		{
			lock (this.m_syncLock)
			{
				if (this.IsRunning)
					return;
				
				this.m_triggerEvent.Reset();
				this.m_cancelSource = new CancellationTokenSource();
				this.m_task = Task.Factory.StartNew(this.ThreadBody, this.m_cancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
				this.m_task.Start();
			}
		}

		public void Stop()
		{
			lock (this.m_syncLock)
			{
				if (!this.IsRunning)
					return;

				// Request cancellation
				this.m_cancelSource.Cancel();

				// Cleanup task
				try
				{
					this.m_task.Wait();
				}
				catch (AggregateException aggregateException)
				{
					aggregateException.Flatten().Handle(ex => ex is TaskCanceledException);
				}
				this.m_task.Dispose();
				this.m_task = null;

				// Clenaup cancellation token
				this.m_cancelSource.Dispose();
				this.m_cancelSource = null;
			}
		}

		public void Trigger()
		{
			try
			{
				this.m_triggerEvent.Set();
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
			if (Thread.CurrentThread.Name == null  && this.ThreadName != null)
				Thread.CurrentThread.Name = this.ThreadName;
				
			// Start the end-less loop
			var handles = new[] { this.m_triggerEvent, this.m_cancelSource.Token.WaitHandle };
			while (true)
			{
				// Check whether cancellation was requested
				this.m_cancelSource.Token.ThrowIfCancellationRequested();

				// Wait for more triggers 
				TimeSpan throttleTime = this.ThrottleTime;
				if (throttleTime > TimeSpan.Zero)
				{
					Thread.Sleep(throttleTime);
					this.m_triggerEvent.Reset();
				}

				// Execute action
				try
				{
					this.m_taskBody();
				}
				catch (Exception)
				{
					// TODO: improve exception handling

					//this.tracer.Write(Category.Error, ex, "An error occurred in repeatable task thread while executing the repeatable action! Retriggering ...", new object[0]);
					this.Trigger();
				}

				// Determine max wait time
				TimeSpan waitTime = this.MinTriggerTime;
				if (waitTime <= TimeSpan.Zero)
					waitTime = TimeSpan.FromMilliseconds(-1);

				// Wait until any of:
				//    - Next execution is explicitely triggered
				//    - Cancellation request
				//    - Min periode elapses
				WaitHandle.WaitAny(handles, waitTime);
			}
		}
// ReSharper restore FunctionNeverReturns


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Stop();
				this.m_triggerEvent.Dispose();
				if (this.m_cancelSource != null)
				{
					this.m_cancelSource.Dispose();
				}
			}
		}
	}
}
