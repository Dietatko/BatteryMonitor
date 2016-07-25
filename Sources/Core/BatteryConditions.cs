using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ImpruvIT.BatteryMonitor
{
	public class BatteryConditions : INotifyPropertyChanged, ICloneable
	{
		public BatteryConditions()
		{
			this.m_cellVoltages = new float[4];
			this.CellVoltages = new ReadOnlyCollection<float>(this.m_cellVoltages);
		}

		private bool IsUpdating { get; set; }
		private bool WasUpdated { get; set; }

		// Actuals
		public float Voltage
		{
			get { return this.m_voltage; }
			set
			{
				var oldValue = this.m_voltage;
				this.m_voltage = value;
				this.OnPropertyChanged("Voltage", oldValue, value);
			}
		}
		private float m_voltage;

		public ReadOnlyCollection<float> CellVoltages { get; private set; }
		private readonly float[] m_cellVoltages;

		public float Current
		{
			get { return this.m_current; }
			set
			{
				var oldValue = this.m_current;
				this.m_current = value;
				this.OnPropertyChanged("Current", oldValue, value);
			}
		}
		private float m_current;

		public float AverageCurrent
		{
			get { return this.m_averageCurrent; }
			set
			{
				var oldValue = this.m_averageCurrent;
				this.m_averageCurrent = value;
				this.OnPropertyChanged("AverageCurrent", oldValue, value);
			}
		}
		private float m_averageCurrent;

		public float Temperature
		{
			get { return this.m_temperature; }
			set
			{
				var oldValue = this.m_temperature;
				this.m_temperature = value;
				this.OnPropertyChanged("Temperature", oldValue, value);
			}
		}
		private float m_temperature;

		// Discharging
		public float RemainingCapacity
		{
			get { return this.m_remainingCapacity; }
			set
			{
				var oldValue = this.m_remainingCapacity;
				this.m_remainingCapacity = value;
				this.OnPropertyChanged("RemainingCapacity", oldValue, value);
			}
		}
		private float m_remainingCapacity;

		public int AbsoluteStateOfCharge
		{
			get { return this.m_absoluteStateOfCharge; }
			set
			{
				var oldValue = this.m_absoluteStateOfCharge;
				if (oldValue == value)
					return;

				this.m_absoluteStateOfCharge = value;

				this.OnPropertyChanged("AbsoluteStateOfCharge", oldValue, value);
			}
		}
		private int m_absoluteStateOfCharge;

		public int RelativeStateOfCharge
		{
			get { return this.m_relativeStateOfCharge; }
			set
			{
				var oldValue = this.m_relativeStateOfCharge;
				if (oldValue == value)
					return;

				this.m_relativeStateOfCharge = value;

				this.OnPropertyChanged("RelativeStateOfCharge", oldValue, value);
			}
		}
		private int m_relativeStateOfCharge;

		public TimeSpan RunTimeToEmpty
		{
			get { return this.m_runTimeToEmpty; }
			set
			{
				var oldValue = this.m_runTimeToEmpty;
				if (oldValue == value)
					return;

				this.m_runTimeToEmpty = value;

				this.OnPropertyChanged("RunTimeToEmpty", oldValue, value);
			}
		}
		private TimeSpan m_runTimeToEmpty;

		public TimeSpan AverageTimeToEmpty
		{
			get { return this.m_averageTimeToEmpty; }
			set
			{
				var oldValue = this.m_averageTimeToEmpty;
				if (oldValue == value)
					return;

				this.m_averageTimeToEmpty = value;

				this.OnPropertyChanged("AverageTimeToEmpty", oldValue, value);
			}
		}
		private TimeSpan m_averageTimeToEmpty;
		
		// Charging
		public float ChargingVoltage
		{
			get { return this.m_chargingVoltage; }
			set
			{
				var oldValue = this.m_chargingVoltage;
				this.m_chargingVoltage = value;
				this.OnPropertyChanged("ChargingVoltage", oldValue, value);
			}
		}
		private float m_chargingVoltage;

		public float ChargingCurrent
		{
			get { return this.m_chargingCurrent; }
			set
			{
				var oldValue = this.m_chargingCurrent;
				this.m_chargingCurrent = value;
				this.OnPropertyChanged("ChargingCurrent", oldValue, value);
			}
		}
		private float m_chargingCurrent;

		public TimeSpan AverageTimeToFull
		{
			get { return this.m_averageTimeToFull; }
			set
			{
				var oldValue = this.m_averageTimeToFull;
				if (oldValue == value)
					return;

				this.m_averageTimeToFull = value;

				this.OnPropertyChanged("AverageTimeToFull", oldValue, value);
			}
		}
		private TimeSpan m_averageTimeToFull;

		public void SetCellVoltage(int cellIndex, float cellVoltage)
		{
			this.m_cellVoltages[cellIndex] = cellVoltage;
			this.OnPropertyChanged("CellVoltages", null, this.CellVoltages);
		}

		public void BeginUpdate()
		{
			this.IsUpdating = true;
		}

		public void EndUpdate()
		{
			if (this.IsUpdating)
			{
				this.IsUpdating = false;

				if (this.WasUpdated)
				{
					this.WasUpdated = false;
					this.OnPropertyChanged<object>(null, null, null);
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged<T>(string propertyName, T oldValue, T newValue)
		{
			if (this.IsUpdating)
			{
				this.WasUpdated = true;
			}
			else
			{
				var handlers = this.PropertyChanged;
				if (handlers != null)
				{
					handlers(this, new PropertyValueChangedEventArgs<T>(propertyName, oldValue, newValue));
				}
			}
		}

		public BatteryConditions Clone()
		{
			return (BatteryConditions)this.MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
	}
}
