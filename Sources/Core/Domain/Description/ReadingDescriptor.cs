using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Description
{
	public class ReadingDescriptor
	{
		public ReadingDescriptor(
			ReadingDescription description,
			IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> sourceKeys, 
			ReadingValueAccessor accessor)
		{
			Contract.Requires(description, "description").IsNotNull();
			Contract.Requires(sourceKeys, "sourceKeys").IsNotNull();
			Contract.Requires(accessor, "accessor").IsNotNull();

			this.Description = description;
			this.SourceKeys = sourceKeys;
			this.Accessor = accessor;
		}

		public ReadingDescription Description { get; private set; }
		public IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> SourceKeys { get; private set; }
		public ReadingValueAccessor Accessor { get; private set; }
	}
}
