using System;
using System.Collections.Generic;
using System.Reflection;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public struct Voltage : IEquatable<Voltage>
	{
		private readonly float m_voltage;

		public Voltage(float voltage)
		{
			this.m_voltage = voltage;
		}

		public float MilliVolts
		{
			get { return this.m_voltage / 1000; }
		}

		public float MicroVolts
		{
			get { return this.m_voltage / 1000000; }
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return this.ToString(3);
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public string ToString(int precision)
		{
			return this.ToString(precision, -3, 3);
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public string ToString(int minPower, int maxPower)
		{
			return this.ToString(3, minPower, maxPower);
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public string ToString(int precision, int minPower, int maxPower)
		{
			return Unit.ToString(this.m_voltage, "V", precision, minPower, maxPower);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object obj)
		{
			if (!(obj is Voltage))
				return false;

			return this.Equals((Voltage)obj);
		}

		public bool Equals(Voltage other)
		{
			return (other.m_voltage - this.m_voltage) < Unit.Epsilon;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return m_voltage.GetHashCode();
		}
	}

	public static class VoltageExtensions
	{
		public static Voltage ToVolts(this float value)
		{
			return new Voltage(value);
		}
		
		public static Voltage ToMilliVolts(this int value)
		{
			return new Voltage(value * 1000f);
		}

		public static Voltage ToMicroVolts(this int value)
		{
			return new Voltage(value * 1000000f);
		}
	}
}
