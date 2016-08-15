using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Domain.Descriptors
{
	public class ReadingDescriptor
	{
		public ReadingDescriptor(
			ReadingDescription description,
			IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> sourceKeys, 
			ReadingValueAccessor accessor)
		{
			Contract.Requires(description, "description").NotToBeNull();
			Contract.Requires(sourceKeys, "sourceKeys").NotToBeNull();
			Contract.Requires(accessor, "accessor").NotToBeNull();

			this.Description = description;
			this.SourceKeys = sourceKeys;
			this.Accessor = accessor;
		}

		public ReadingDescription Description { get; private set; }
		public IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> SourceKeys { get; private set; }
		public ReadingValueAccessor Accessor { get; private set; }
	}
}
