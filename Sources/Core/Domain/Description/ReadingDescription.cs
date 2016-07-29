using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Description
{
	public class ReadingDescription
	{
		public ReadingDescription(string title, string description)
		{
			Contract.Requires(title, "title").NotToBeNull().NotToBeEmpty();
			Contract.Requires(description, "description").NotToBeNull();

			this.Title = title;
			this.Description = description;
		}

		public string Title { get; private set; }
		public string Description { get; private set; }
	}
}
