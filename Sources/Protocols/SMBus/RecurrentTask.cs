using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class RecurrentTask
	{
		private readonly object m_lock = new object();
		private Task m_task;
		private CancellationTokenSource m_cancelSource;
		private readonly AutoResetEvent m_triggerEvent;

		public RecurrentTask(Action taskAction)
			: this(taskAction, TimeSpan.Zero)
		{
		}

		public RecurrentTask(Action taskAction, TimeSpan maxPeriode)
		{
			Contract.Requires(taskAction, "taskAction").IsNotNull();

			this.TaskAction = taskAction;
			this.MaxPeriode = maxPeriode;
			this.m_triggerEvent = new AutoResetEvent(false);
		}

		public bool IsRunning
		{
			get { return this.m_task != null; }
		}

		public TimeSpan MaxPeriode { get; private set; }

		protected Action TaskAction { get; private set; }

		public void Start()
		{
			lock (this.m_lock)
			{
				if (this.IsRunning)
					return;

				this.m_cancelSource = new CancellationTokenSource();
				this.m_task = Task.Factory.StartNew(this.ThreadBody, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);
			}
		}

		public void Stop()
		{
			lock (this.m_lock)
			{
				if (m_task != null)
				{
					this.m_cancelSource.Cancel();
					this.m_task.Wait();
					this.m_task = null;
				}

				if (this.m_cancelSource != null)
				{
					this.m_cancelSource.Dispose();
					this.m_cancelSource = null;
				}
			}
		}

		public void Trigger()
		{
			this.m_triggerEvent.Set();
		}

		private void ThreadBody()
		{
			var waitHandles = new[] { this.m_cancelSource.Token.WaitHandle, this.m_triggerEvent };
			while (true)
			{
				if (this.m_cancelSource.Token.IsCancellationRequested)
					return;

				try
				{
					this.TaskAction();
				}
				catch (Exception ex)
				{
					// Intentionally swallowed
					// TODO: find better way how to ignore/report user exception without shutting down this task and not catching system exceptions in the same time 
				}

				WaitHandle.WaitAny(waitHandles, this.MaxPeriode);
			}
		}
	}
}
