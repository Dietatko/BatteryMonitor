using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class UpdatesSubscription : ISubscription
	{
		public UpdatesSubscription(Action<Battery> consumer, UpdateFrequency frequency, Action<UpdatesSubscription> unsubscribeAction)
		{
			Contract.Requires(consumer, "consumer").IsNotNull();
			Contract.Requires(frequency, "frequency").ToBeDefinedEnumValue();

			this.Consumer = consumer;
			this.Frequency = frequency;
			this.UnsubscribeAction = unsubscribeAction;
		}

		public Action<Battery> Consumer { get; private set; }
		public UpdateFrequency Frequency { get; private set; }
		public Action<UpdatesSubscription> UnsubscribeAction { get; private set; }

		public void Unsubscribe()
		{
			if (this.UnsubscribeAction != null)
				this.UnsubscribeAction(this);
		}

		public void Dispose()
		{
			this.Unsubscribe();
			GC.SuppressFinalize(this);
		}
	}

	// TODO: extract to Bricks
	public static class ContractExtensions
	{
		public static ArgumentValueRequirement<T> ToBeDefinedEnumValue<T>(this ArgumentValueRequirement<T> req)
		{
			Contract.Requires(req, "req").IsNotNull();

			var argType = typeof(T);
			if (!argType.IsEnumDefined(req.Value))
				throw new ArgumentOutOfRangeException(req.ArgumentName, req.Value, "The specified enum valule is not defined in the enumeration.");

			return req;
		}
	}
}
