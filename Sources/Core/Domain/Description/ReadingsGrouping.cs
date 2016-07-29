using System;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Description
{
	public class ReadingDescriptorGrouping : IEnumerable<ReadingDescriptor>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ReadingDescriptorGrouping" /> class.
		/// </summary>
		/// <param name="title">The group title.</param>
		/// <param name="descriptors">The descriptors in the group.</param>
		public ReadingDescriptorGrouping(string title, IEnumerable<ReadingDescriptor> descriptors)
		{
			Contract.Requires(title, "title").IsNotNull();
			Contract.Requires(descriptors, "descriptors").IsNotNull();

			this.Title = title;
			this.Descriptors = descriptors;
		}

		public string Title { get; private set; }
		public IEnumerable<ReadingDescriptor> Descriptors { get; private set; }
		public bool IsDefault { get; set; }

		public IEnumerator<ReadingDescriptor> GetEnumerator()
		{
			return this.Descriptors.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
