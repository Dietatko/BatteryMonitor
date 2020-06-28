using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
    [DebuggerDisplay("{Key}")]
    public abstract class ReadingValueBase
	{
		protected ReadingValueBase(EntryKey key)
		{
			Contract.Requires(key, "key").NotToBeNull();

			this.Key = key;
		}

		public EntryKey Key { get; private set; }


		public event EventHandler ValueChanged;

		protected virtual void OnValueChanged()
		{
			var handlers = this.ValueChanged;
			if (handlers != null)
				handlers(this, EventArgs.Empty);
		}
	}
}
