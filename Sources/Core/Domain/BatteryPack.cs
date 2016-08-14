using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public abstract class BatteryPack : BatteryElement
	{
		private readonly ProductDefinitionWrapper m_productWrapper;

		protected BatteryPack(IEnumerable<BatteryElement> subElements)
		{
			Contract.Requires(subElements, "subElements")
				.NotToBeNull();
			this.m_subElements = subElements.ToList();
			Contract.Requires(this.m_subElements, "subElements")
				.NotToBeEmpty();

			this.m_productWrapper = new ProductDefinitionWrapper(this.CustomData);
			this.SubElements.ForEach(x => x.ValueChanged += (s, a) => this.OnValueChanged(a));
		}

		public override IProductDefinition Product
		{
			get { return this.m_productWrapper; }
		}

		public IEnumerable<BatteryElement> SubElements
		{
			get { return this.m_subElements; }
		}
		private readonly List<BatteryElement> m_subElements; 

		public BatteryElement this[int index]
		{
			get { return this.m_subElements[index]; }
		}

		public int ElementCount
		{
			get { return this.m_subElements.Count; }
		}
	}
}
